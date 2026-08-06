// MarkerSymbolTexturePostprocessor.cs
//
// Editor-only: auto-configures the texture-importer for any sprite that lives
// under a wall's MarkerAssets/ folder or the framework's /Icons/ folder, so a
// developer can drop a PNG in and it is immediately usable as a marker/badge/ring
// symbol with zero manual importer fiddling. Lives in the Editor assembly (any
// AssetPostprocessor is editor-only by definition -- this is the one place the
// "Editor" folder name is genuinely required), per _5.1_Editor_Tab.md section 14.

using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    // The attribute guarantees the type is loaded even during a head-less
    // batch-mode import (e.g. an EditMode test forcing a reimport), where a
    // plain AssetPostprocessor subclass would not otherwise be alive.
    [InitializeOnLoad]
    internal class MarkerSymbolTexturePostprocessor : AssetPostprocessor
    {
        // Force the static constructor to run so InitializeOnLoad hooks attach.
        static MarkerSymbolTexturePostprocessor() { }

        // Configure every texture imported under a MarkerAssets/ path as a
        // single Sprite with transparency enabled and mipmaps off -- the exact
        // import profile POI_Marker and its sub-views expect.
        private void OnPreprocessTexture()
        {
            // Auto-configure any texture dropped under either a wall's MarkerAssets/
            // folder or the framework's /Icons/ folder as a single Sprite with
            // transparency enabled and mipmaps off.
            // Check for null/empty FIRST -- the Contains() calls below would throw
            // NullReferenceException if assetPath were null. Unity always passes a
            // non-null path here in practice, but the guard must come first.
            if (string.IsNullOrEmpty(assetPath))
                return;
            bool isMarkerAsset = assetPath.Contains("/MarkerAssets/");
            bool isIcon = assetPath.Contains("/Icons/");
            if (!isMarkerAsset && !isIcon)
                return;

            var importer = assetImporter as TextureImporter;
            if (importer == null)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.sRGBTexture = true;
        }
    }
}
