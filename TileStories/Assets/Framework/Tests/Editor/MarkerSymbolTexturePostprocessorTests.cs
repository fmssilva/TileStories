// MarkerSymbolTexturePostprocessorTests.cs
//
// EditMode test for the MarkerSymbolTexturePostprocessor (section 14.9 of
// _5.1_Editor_Tab.md). The postprocessor is editor-only AssetPostprocessor code,
// so EditMode is the correct tier: no scene, no device, no Play Mode needed. It
// verifies the core claim of section 14.9 -- drop a PNG under any wall's
// MarkerAssets/ folder and it imports as a Sprite (2D and UI) with alpha and no
// mipmaps, with no manual importer step.

using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    public class MarkerSymbolTexturePostprocessorTests
    {
        // A folder whose path contains "/MarkerAssets/" so the postprocessor fires.
        // Lives under the test assembly so it is self-contained and cleans up after itself.
        private const string TestFolder = "Assets/Framework/Tests/Editor/MarkerAssets";
        private const string TestAssetPath = TestFolder + "/postprocessor_test_symbol.png";

        // Convert an Assets-relative asset path into an absolute filesystem path.
        // Replaces the old path.Replace("Assets/", dataPath + "/") which also rewrote
        // the "Assets" substring embedded inside "MarkerAssets/" and produced a
        // malformed Windows path (the cause of the pre-existing test failures).
        private static string ToFilesystemPath(string assetPath)
        {
            return Path.Combine(Application.dataPath,
                assetPath.Substring("Assets".Length).TrimStart('/', '\\'));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            // Belt-and-suspenders cleanup so the test never leaves cruft behind.
            if (File.Exists(ToFilesystemPath(TestAssetPath)))
                AssetDatabase.DeleteAsset(TestAssetPath);
        }

        [Test]
        public void PngUnderMarkerAssetsFolder_ImportsAsSpriteWithAlphaAndNoMipmaps()
        {
            // Make a 2x2 PNG with a fully-transparent pixel so alpha is meaningful.
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, new Color(1f, 0f, 0f, 0f));   // transparent red
            tex.SetPixel(1, 0, new Color(0f, 1f, 0f, 1f));   // opaque green
            tex.SetPixel(0, 1, new Color(0f, 0f, 1f, 1f));   // opaque blue
            tex.SetPixel(1, 1, new Color(1f, 1f, 0f, 0.5f)); // half-transparent yellow
            tex.Apply();

            string fsPath = ToFilesystemPath(TestAssetPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fsPath));
            byte[] png = tex.EncodeToPNG();
            File.WriteAllBytes(fsPath, png);
            Object.DestroyImmediate(tex);

            try
            {
                AssetDatabase.ImportAsset(TestAssetPath, ImportAssetOptions.ForceUpdate);

                var importer = AssetImporter.GetAtPath(TestAssetPath) as TextureImporter;
                Assert.IsNotNull(importer, "TextureImporter not found for the test PNG.");

                Assert.AreEqual(TextureImporterType.Sprite, importer.textureType,
                    "PNG under MarkerAssets/ must import as Sprite (2D and UI).");
                Assert.AreEqual(SpriteImportMode.Single, importer.spriteImportMode,
                    "Must import as a single sprite.");
                Assert.IsFalse(importer.mipmapEnabled,
                    "Mipmaps must be disabled for marker symbols.");
                Assert.IsTrue(importer.alphaIsTransparency,
                    "Alpha is transparency must be enabled for PNGs with an alpha channel.");
            }
            finally
            {
                AssetDatabase.DeleteAsset(TestAssetPath);
            }
        }

        [Test]
        public void PngOutsideMarkerAssetsFolder_IsNotAffected()
        {
            // Sanity: the postprocessor is scoped to MarkerAssets/ paths. A PNG
            // imported elsewhere must NOT be reconfigured as a sprite by this code.
            // (We don't assert the resulting type since Unity's default may vary;
            // we only assert our postprocessor did not throw and the importer loads.)
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            string path = "Assets/Framework/Tests/Editor/postprocessor_out_of_scope.png";
            string fsPath = ToFilesystemPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(fsPath));
            File.WriteAllBytes(fsPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            try
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                Assert.IsNotNull(importer, "Out-of-scope importer should still load.");
            }
            finally
            {
                AssetDatabase.DeleteAsset(path);
            }
        }
    }
}
