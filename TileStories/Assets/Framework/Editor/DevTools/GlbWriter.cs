using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace TileStories.Editor
{
    // A minimal glTF 2.0 binary (.glb) writer for the generated default media (_3.1 step 10A): ONE mesh with positions, normals, vertex
    // colours (COLOR_0) and triangle indices, one plain material, one node. Enough for a small made-in-code model, no dependency on an
    // exporter or a render pipeline's materials. Coordinates are glTF's own (right-handed, +Y up, +Z towards the viewer, triangles
    // counter-clockwise from their front); glTFast converts them to Unity's on import. Pure: the bytes are a unit test.
    public static class GlbWriter
    {
        // The mesh to write: flat lists, three indices per triangle
        public sealed class MeshData
        {
            public readonly List<Vector3> Positions = new();
            public readonly List<Vector3> Normals = new();
            public readonly List<Color32> Colors = new();
            public readonly List<int> Indices = new();

            // One flat-shaded quad (a, b, c, d around its edge) facing `outward`: its winding is chosen from that direction, so a
            // face can never end up inside-out whatever order the corners were listed in
            public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 outward, Color32 color)
            {
                Vector3 n = Vector3.Cross(b - a, c - a);
                if (Vector3.Dot(n, outward) < 0f) (b, d) = (d, b);
                Vector3 normal = outward.normalized;
                int start = Positions.Count;
                foreach (var p in new[] { a, b, c, d })
                {
                    Positions.Add(p);
                    Normals.Add(normal);
                    Colors.Add(color);
                }
                Indices.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });
            }

            // An axis-aligned box (its six faces facing out)
            public void Box(Vector3 min, Vector3 max, Color32 color)
            {
                var (x0, y0, z0, x1, y1, z1) = (min.x, min.y, min.z, max.x, max.y, max.z);
                Quad(new(x0, y0, z1), new(x1, y0, z1), new(x1, y1, z1), new(x0, y1, z1), Vector3.forward, color);
                Quad(new(x0, y0, z0), new(x1, y0, z0), new(x1, y1, z0), new(x0, y1, z0), Vector3.back, color);
                Quad(new(x1, y0, z0), new(x1, y1, z0), new(x1, y1, z1), new(x1, y0, z1), Vector3.right, color);
                Quad(new(x0, y0, z0), new(x0, y1, z0), new(x0, y1, z1), new(x0, y0, z1), Vector3.left, color);
                Quad(new(x0, y1, z0), new(x1, y1, z0), new(x1, y1, z1), new(x0, y1, z1), Vector3.up, color);
                Quad(new(x0, y0, z0), new(x1, y0, z0), new(x1, y0, z1), new(x0, y0, z1), Vector3.down, color);
            }

            public int TriangleCount => Indices.Count / 3;
        }

        private const uint Magic = 0x46546C67;     // "glTF"
        private const uint ChunkJson = 0x4E4F534A; // "JSON"
        private const uint ChunkBin = 0x004E4942;  // "BIN\0"
        private const int ArrayBuffer = 34962;
        private const int ElementArrayBuffer = 34963;
        private const int Float = 5126;
        private const int UnsignedByte = 5121;
        private const int UnsignedShort = 5123;
        private const int UnsignedInt = 5125;

        // The .glb bytes of `mesh`, its node named `name`
        public static byte[] Write(MeshData mesh, string name)
        {
            if (mesh == null || mesh.Positions.Count == 0 || mesh.Indices.Count == 0) throw new ArgumentException("an empty mesh");
            int vertices = mesh.Positions.Count;
            bool shortIndices = vertices <= ushort.MaxValue;

            var bin = new MemoryStream();
            var w = new BinaryWriter(bin);
            int posOffset = 0;
            foreach (var p in mesh.Positions) { w.Write(p.x); w.Write(p.y); w.Write(p.z); }
            int normalOffset = (int)bin.Length;
            foreach (var n in mesh.Normals) { w.Write(n.x); w.Write(n.y); w.Write(n.z); }
            int colorOffset = (int)bin.Length;
            // - glTF's COLOR_0 is LINEAR: the palette's sRGB bytes are converted, so the model shows the colours the palette names
            foreach (var c in mesh.Colors) { w.Write(Linear(c.r)); w.Write(Linear(c.g)); w.Write(Linear(c.b)); w.Write(c.a); }
            int indexOffset = (int)bin.Length;
            foreach (int i in mesh.Indices) { if (shortIndices) w.Write((ushort)i); else w.Write((uint)i); }
            int indexLength = (int)bin.Length - indexOffset;
            Pad(bin, 0);

            Vector3 min = mesh.Positions[0], max = mesh.Positions[0];
            foreach (var p in mesh.Positions) { min = Vector3.Min(min, p); max = Vector3.Max(max, p); }

            string F(float f) => f.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
            string json =
                "{\"asset\":{\"version\":\"2.0\",\"generator\":\"TileStories CardMediaLibraryGenerator\"}," +
                "\"scene\":0,\"scenes\":[{\"nodes\":[0]}]," +
                "\"nodes\":[{\"mesh\":0,\"name\":\"" + name + "\"}]," +
                "\"meshes\":[{\"name\":\"" + name + "\",\"primitives\":[{\"attributes\":{\"POSITION\":0,\"NORMAL\":1,\"COLOR_0\":2},\"indices\":3,\"material\":0}]}]," +
                "\"materials\":[{\"name\":\"" + name + "\",\"pbrMetallicRoughness\":{\"baseColorFactor\":[1,1,1,1],\"metallicFactor\":0,\"roughnessFactor\":0.8}}]," +
                "\"buffers\":[{\"byteLength\":" + bin.Length + "}]," +
                "\"bufferViews\":[" +
                    View(posOffset, normalOffset - posOffset, ArrayBuffer) + "," +
                    View(normalOffset, colorOffset - normalOffset, ArrayBuffer) + "," +
                    View(colorOffset, indexOffset - colorOffset, ArrayBuffer) + "," +
                    View(indexOffset, indexLength, ElementArrayBuffer) + "]," +
                "\"accessors\":[" +
                    "{\"bufferView\":0,\"componentType\":" + Float + ",\"count\":" + vertices + ",\"type\":\"VEC3\"," +
                        "\"min\":[" + F(min.x) + "," + F(min.y) + "," + F(min.z) + "],\"max\":[" + F(max.x) + "," + F(max.y) + "," + F(max.z) + "]}," +
                    "{\"bufferView\":1,\"componentType\":" + Float + ",\"count\":" + vertices + ",\"type\":\"VEC3\"}," +
                    "{\"bufferView\":2,\"componentType\":" + UnsignedByte + ",\"normalized\":true,\"count\":" + vertices + ",\"type\":\"VEC4\"}," +
                    "{\"bufferView\":3,\"componentType\":" + (shortIndices ? UnsignedShort : UnsignedInt) + ",\"count\":" + mesh.Indices.Count + ",\"type\":\"SCALAR\"}]}";

            // - an expandable stream (new MemoryStream(bytes) is fixed-size and could not take the padding)
            var jsonStream = new MemoryStream();
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            jsonStream.Write(jsonBytes, 0, jsonBytes.Length);
            Pad(jsonStream, (byte)' ');

            var glb = new MemoryStream();
            var o = new BinaryWriter(glb);
            o.Write(Magic);
            o.Write(2u);
            o.Write((uint)(12 + 8 + jsonStream.Length + 8 + bin.Length));
            o.Write((uint)jsonStream.Length);
            o.Write(ChunkJson);
            o.Write(jsonStream.ToArray());
            o.Write((uint)bin.Length);
            o.Write(ChunkBin);
            o.Write(bin.ToArray());
            return glb.ToArray();
        }

        // One sRGB colour channel as the linear byte glTF stores
        internal static byte Linear(byte srgb) => (byte)Mathf.RoundToInt(Mathf.GammaToLinearSpace(srgb / 255f) * 255f);

        private static string View(int offset, int length, int target) =>
            "{\"buffer\":0,\"byteOffset\":" + offset + ",\"byteLength\":" + length + ",\"target\":" + target + "}";

        // Every chunk's length is a multiple of 4 (the JSON padded with spaces, the binary with zeros)
        private static void Pad(MemoryStream stream, byte with)
        {
            while (stream.Length % 4 != 0) stream.WriteByte(with);
        }
    }
}
