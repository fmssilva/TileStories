using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // _3.1 step 10A: the default library's generated model and 360 picture. The GLB writer's bytes are checked as a glTF reader would
    // read them, the SHIPPED model is the real file glTFast imported (its triangles, its vertex colours), and the SHIPPED panorama is
    // the real JPG, read back pixel by pixel in EquirectRule's own directions -- so the picture and the viewer agree on "ahead".
    public class GeneratedModelAndPanoramaTests
    {
        private static string ModelPath => CardMediaLibraryGenerator.ModelsFolder + "/" + CardMediaLibraryGenerator.ArchFile;
        private static string PanoramaPath => CardMediaLibraryGenerator.PanoramasFolder + "/" + CardMediaLibraryGenerator.RoomPanoramaFile;

        [Test]
        public void GlbWriter_WritesAGlbAReaderAccepts_HeaderChunksAlignedAndTheJsonNamesEveryAttribute()
        {
            var mesh = new GlbWriter.MeshData();
            mesh.Box(Vector3.zero, Vector3.one, new Color32(10, 20, 30, 255));
            byte[] glb = GlbWriter.Write(mesh, "box");

            using var r = new BinaryReader(new MemoryStream(glb));
            Assert.AreEqual(0x46546C67u, r.ReadUInt32(), "magic glTF");
            Assert.AreEqual(2u, r.ReadUInt32(), "version 2");
            Assert.AreEqual((uint)glb.Length, r.ReadUInt32(), "the header's length is the file's");
            uint jsonLength = r.ReadUInt32();
            Assert.AreEqual(0x4E4F534Au, r.ReadUInt32(), "the JSON chunk first");
            Assert.AreEqual(0u, jsonLength % 4, "chunks are 4-byte aligned");
            string json = Encoding.UTF8.GetString(r.ReadBytes((int)jsonLength));
            uint binLength = r.ReadUInt32();
            Assert.AreEqual(0x004E4942u, r.ReadUInt32(), "then the binary chunk");
            Assert.AreEqual(0u, binLength % 4);
            Assert.AreEqual(glb.Length, 12 + 8 + jsonLength + 8 + binLength, "nothing after the two chunks");

            foreach (string expected in new[] { "\"POSITION\":0", "\"NORMAL\":1", "\"COLOR_0\":2", "\"indices\":3", "\"count\":24", "\"count\":36", "\"min\":[0,0,0]", "\"max\":[1,1,1]" })
                StringAssert.Contains(expected, json);
            Assert.AreEqual(12, mesh.TriangleCount, "a box: six faces, two triangles each");
        }

        [Test]
        public void GlbWriter_AQuadFacesWhereItIsTold_WhateverOrderItsCornersCameIn()
        {
            foreach (bool reversed in new[] { false, true })
            {
                var mesh = new GlbWriter.MeshData();
                var a = new Vector3(0, 0, 0); var b = new Vector3(1, 0, 0); var c = new Vector3(1, 1, 0); var d = new Vector3(0, 1, 0);
                if (reversed) mesh.Quad(a, d, c, b, Vector3.forward, default);
                else mesh.Quad(a, b, c, d, Vector3.forward, default);
                for (int t = 0; t < 2; t++)
                {
                    var p0 = mesh.Positions[mesh.Indices[t * 3]];
                    var p1 = mesh.Positions[mesh.Indices[t * 3 + 1]];
                    var p2 = mesh.Positions[mesh.Indices[t * 3 + 2]];
                    Assert.Greater(Vector3.Dot(Vector3.Cross(p1 - p0, p2 - p0), Vector3.forward), 0f,
                        "triangle " + t + (reversed ? " (corners listed backwards)" : "") + " is counter-clockwise seen from +Z");
                }
            }
        }

        [Test]
        public void TheShippedArch_IsARealGlbGltfastImported_AFewHundredTriangles_WithItsVertexColours()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            Assert.IsNotNull(prefab, ModelPath + " imported as a prefab (glTFast is the .glb importer)");
            Assert.AreEqual("GltfImporter", AssetImporter.GetAtPath(ModelPath).GetType().Name);
            var filter = prefab.GetComponentInChildren<MeshFilter>();
            Assert.IsNotNull(filter, "one mesh");
            var mesh = filter.sharedMesh;
            int expected = CardMediaLibraryGenerator.Arch().TriangleCount;
            Assert.AreEqual(expected, mesh.triangles.Length / 3, "every written triangle arrived");
            Assert.That(expected, Is.InRange(150, 600), "a few hundred triangles");
            Assert.AreEqual(mesh.vertexCount, mesh.colors32.Length, "the colours are baked into the vertices");
            // - glTF vertex colours are linear: the palette's blue tile (31, 63, 143 sRGB) arrives as its linear value, not the sRGB bytes
            Color blueLinear = ((Color)new Color32(31, 63, 143, 255)).linear;
            Assert.IsTrue(System.Array.Exists(mesh.colors, c => Mathf.Abs(c.r - blueLinear.r) < 0.01f && Mathf.Abs(c.g - blueLinear.g) < 0.01f && Mathf.Abs(c.b - blueLinear.b) < 0.01f),
                "a vertex holds the tiles' blue in linear space " + blueLinear);
            Assert.AreEqual(1.56f, mesh.bounds.size.x, 0.01f, "the step's width, metres");
            Assert.Greater(mesh.bounds.size.y, 1.6f, "pillars and arch stand taller than wide");
        }

        [Test]
        public void TheShippedPanorama_IsTwoToOne_AndLooksAtTheDoorwayAheadAndTheWindowBehind()
        {
            byte[] jpg = File.ReadAllBytes(Path.Combine(Application.dataPath, PanoramaPath.Substring("Assets/".Length)));
            var tex = new Texture2D(2, 2);
            try
            {
                Assert.IsTrue(tex.LoadImage(jpg), "a real JPG");
                Assert.AreEqual(CardMediaLibraryGenerator.PanoramaWidth, tex.width);
                Assert.AreEqual(tex.width / 2, tex.height, "equirect: twice as wide as tall");
                Color Look(Vector3 direction)
                {
                    var uv = EquirectRule.UvOf(direction);
                    return tex.GetPixelBilinear(uv.x, uv.y);
                }
                Color ahead = Look(new Vector3(0f, 0.02f, 1f));
                Color behind = Look(new Vector3(0f, 0.1f, -1f));
                Color down = Look(Vector3.down);
                Assert.Less(ahead.grayscale, 0.25f, "straight ahead: the dark doorway " + ahead);
                Assert.Greater(behind.b, behind.r, "behind: the sky-blue window " + behind);
                Assert.Greater(down.r, down.b, "down: the terracotta floor " + down);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
            var importer = (TextureImporter)AssetImporter.GetAtPath(PanoramaPath);
            Assert.AreEqual(TextureWrapMode.Repeat, importer.wrapModeU, "left and right edges meet behind the viewer: no seam");
        }

        [Test]
        public void EquirectRule_AheadIsTheMiddle_RightGrowsU_UpIsTheTop_AndUvOfUndoesDirectionOf()
        {
            Assert.AreEqual(new Vector2(0.5f, 0.5f), EquirectRule.UvOf(Vector3.forward));
            Assert.AreEqual(0.75f, EquirectRule.UvOf(Vector3.right).x, 1e-5f, "a quarter turn right");
            Assert.AreEqual(0.25f, EquirectRule.UvOf(Vector3.left).x, 1e-5f);
            Assert.AreEqual(1f, EquirectRule.UvOf(Vector3.up).y, 1e-5f, "straight up is the top row");
            Assert.AreEqual(0f, EquirectRule.UvOf(Vector3.down).y, 1e-5f);
            Assert.AreEqual(new Vector2(0.5f, 0.5f), EquirectRule.UvOf(Vector3.zero), "no direction reads as ahead");
            for (float u = 0.05f; u < 1f; u += 0.1f)
                for (float v = 0.05f; v < 1f; v += 0.1f)
                {
                    var back = EquirectRule.UvOf(EquirectRule.DirectionOf(u, v));
                    Assert.AreEqual(u, back.x, 1e-4f, "u " + u + " v " + v);
                    Assert.AreEqual(v, back.y, 1e-4f, "u " + u + " v " + v);
                    Assert.AreEqual(1f, EquirectRule.DirectionOf(u, v).magnitude, 1e-5f, "a unit direction");
                }
        }
    }
}
