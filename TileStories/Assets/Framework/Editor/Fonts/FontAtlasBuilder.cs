using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using FontAsset = UnityEngine.TextCore.Text.FontAsset;
using TextCoreAtlasPopulationMode = UnityEngine.TextCore.Text.AtlasPopulationMode;

namespace TileStories.Editor
{
    // Builds the app's font assets (_3.3 R12.3, P4.3): for every .ttf under Runtime/UI/Theme/Fonts, a TextMeshPro asset (the markers) and a
    // TextCore asset (every UI Toolkit panel) beside it -- "<name> TMP.asset" / "<name> UITK.asset" -- both STATIC: a dynamic asset writes
    // each glyph it meets back into the file (P0 S2: 5.8 KB -> 2.1 MB) and git sees it change on every Editor launch. Their characters:
    // printable ASCII, Latin-1, Latin Extended-A, the app's typographic punctuation and every character the app and the wall ship
    // (ShippedTextCharacters) -- each one the face has. Then each theme's type roles are written into the UI assets' weight tables (a bold
    // body text draws the Bold cut; a bold display title draws the display cut itself, no synthesized bold). An existing asset is cleared and
    // refilled in place, so its GUID -- what the font library and the generated theme stylesheet point at -- never changes.
    public static class FontAtlasBuilder
    {
        public const string FontsFolder = "Assets/Framework/Runtime/UI/Theme/Fonts";
        // - the P0 spike's sampling (48 pt, ~10 % padding); a 1024 page holds the ~330 glyphs of one face
        public const int SamplingPointSize = 48;
        public const int AtlasPadding = 6;
        public const int AtlasSize = 1024;
        // - the UI Toolkit assets: a text-shadow or outline reaches only as far as the SDF does (the padding, 18 / 48 = 0.375 em: the hero's
        //   halo 4-5 px round a 12 px eyebrow, ~11 px round its title -- 6 cut it at 1.5 px), so their page doubles to hold the wider cells
        public const int UiAtlasPadding = 18;
        public const int UiAtlasSize = 2048;

        // The typographic punctuation the app's texts use: curly quotes, en / em dash, ellipsis, middle dot, the affordance arrow
        public const string Typographic = "“”‘’–—…·›";

        // Every character a theme font asset is asked to hold (each face holds those of them it has)
        public static string CharacterSet()
        {
            var set = new SortedSet<char>();
            for (char c = ' '; c <= '~'; c++) set.Add(c);
            for (char c = ' '; c <= 'ſ'; c++) set.Add(c);
            foreach (char c in Typographic) set.Add(c);
            foreach (char c in ShippedTextCharacters.Collect().Keys) set.Add(c);
            return new string(set.ToArray());
        }

        public static string TmpPathFor(string ttfPath) => Path.ChangeExtension(ttfPath, null).Replace('\\', '/') + " TMP.asset";
        public static string UiPathFor(string ttfPath) => Path.ChangeExtension(ttfPath, null).Replace('\\', '/') + " UITK.asset";

        public static string[] SourceFonts() =>
            Directory.GetFiles(FontsFolder, "*.ttf", SearchOption.AllDirectories).Select(p => p.Replace('\\', '/')).OrderBy(p => p).ToArray();

        // Build both assets of every theme font, then wire every theme's weights
        [MenuItem("TileStories/Fonts/Rebuild Theme Font Atlases")]
        public static void RebuildAll()
        {
            string characters = CharacterSet();
            foreach (string ttf in SourceFonts())
            {
                BuildTmp(ttf, characters);
                BuildUi(ttf, characters);
            }
            AssetDatabase.SaveAssets();
            foreach (string guid in AssetDatabase.FindAssets("t:ThemeAsset"))
                WireWeights(AssetDatabase.LoadAssetAtPath<ThemeAsset>(AssetDatabase.GUIDToAssetPath(guid)));
            AssetDatabase.SaveAssets();
            Debug.Log("[Fonts] rebuilt " + SourceFonts().Length + " faces x 2 assets, " + characters.Length + " characters asked");
        }

        // The TextMeshPro asset of `ttfPath`, static, holding every character of `characters` the face has
        public static TMP_FontAsset BuildTmp(string ttfPath, string characters)
        {
            var font = LoadFont(ttfPath);
            string path = TmpPathFor(ttfPath);
            var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (asset == null)
            {
                asset = TMP_FontAsset.CreateFontAsset(font, SamplingPointSize, AtlasPadding, GlyphRenderMode.SDFAA, AtlasSize, AtlasSize,
                    AtlasPopulationMode.Dynamic, enableMultiAtlasSupport: false);
                asset.name = Path.GetFileNameWithoutExtension(path);
                AssetDatabase.CreateAsset(asset, path);
                AddSubAssets(asset, asset.atlasTextures, asset.material);
            }
            else
            {
                asset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                asset.ClearFontAssetData(setAtlasSizeToZero: false);
            }
            asset.TryAddCharacters(characters, out string missing);
            CheckMissing(path, font, missing);
            asset.atlasPopulationMode = AtlasPopulationMode.Static;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        // The TextCore asset of `ttfPath` (UI Toolkit), static, holding every character of `characters` the face has
        public static FontAsset BuildUi(string ttfPath, string characters)
        {
            var font = LoadFont(ttfPath);
            string path = UiPathFor(ttfPath);
            var asset = AssetDatabase.LoadAssetAtPath<FontAsset>(path);
            if (asset == null)
            {
                asset = FontAsset.CreateFontAsset(font, SamplingPointSize, UiAtlasPadding, GlyphRenderMode.SDFAA, UiAtlasSize, UiAtlasSize,
                    TextCoreAtlasPopulationMode.Dynamic, false);
                asset.name = Path.GetFileNameWithoutExtension(path);
                AssetDatabase.CreateAsset(asset, path);
                AddSubAssets(asset, asset.atlasTextures, asset.material);
            }
            else
            {
                asset.atlasPopulationMode = TextCoreAtlasPopulationMode.Dynamic;
                // - an old asset takes the UI padding and page before it is cleared, so the refill packs its glyphs with them (the asset's
                //   own setters are internal: its serialized fields, the Editor's way)
                var serialized = new SerializedObject(asset);
                serialized.FindProperty("m_AtlasPadding").intValue = UiAtlasPadding;
                serialized.FindProperty("m_AtlasWidth").intValue = UiAtlasSize;
                serialized.FindProperty("m_AtlasHeight").intValue = UiAtlasSize;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                asset.ClearFontAssetData(false);
            }
            asset.TryAddCharacters(characters, out string missing);
            CheckMissing(path, font, missing);
            asset.atlasPopulationMode = TextCoreAtlasPopulationMode.Static;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        // Write `theme`'s type roles into its UI assets' weight tables (index = weight / 100): body 400 / 500 / 700 on each body asset;
        // the display asset at its own weight and at 700
        public static void WireWeights(ThemeAsset theme)
        {
            var type = theme.Type;
            if (type.fontLibrary == null) throw new InvalidOperationException("[Fonts] theme '" + theme.Key + "' names no font library");
            FontAsset Ui(string key)
            {
                var asset = type.fontLibrary.GetUiFont(key);
                if (asset == null) throw new InvalidOperationException("[Fonts] theme '" + theme.Key + "': key '" + key + "' has no UI Toolkit font in " + type.fontLibrary.name);
                return asset;
            }
            var regular = Ui(type.bodyFont);
            var body = new Dictionary<int, FontAsset> { [4] = regular, [5] = Ui(type.bodyMediumFont), [7] = Ui(type.bodyBoldFont) };
            foreach (var asset in body.Values.Distinct()) WriteWeightTable(asset, body);
            var display = Ui(type.displayFont);
            WriteWeightTable(display, new Dictionary<int, FontAsset> { [Mathf.Clamp(type.displayWeight / 100, 1, 9)] = display, [7] = display });
        }

        // The asset's weight table: exactly `upright` (index -> face), every other slot empty; italics are not shipped
        private static void WriteWeightTable(FontAsset asset, Dictionary<int, FontAsset> upright)
        {
            var serialized = new SerializedObject(asset);
            var table = serialized.FindProperty("m_FontWeightTable");
            if (table == null || !table.isArray) throw new InvalidOperationException("[Fonts] " + asset.name + " has no m_FontWeightTable to write");
            table.arraySize = 10;
            for (int i = 0; i < 10; i++)
            {
                var pair = table.GetArrayElementAtIndex(i);
                pair.FindPropertyRelative("regularTypeface").objectReferenceValue = upright.TryGetValue(i, out var face) ? face : null;
                pair.FindPropertyRelative("italicTypeface").objectReferenceValue = null;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }

        private static Font LoadFont(string ttfPath)
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
            if (font == null) throw new InvalidOperationException("[Fonts] no font at " + ttfPath);
            return font;
        }

        // The atlas pages and the material live inside the asset file (a static asset keeps them; nothing else references them)
        private static void AddSubAssets(UnityEngine.Object asset, Texture2D[] textures, Material material)
        {
            foreach (var texture in textures)
            {
                texture.name = asset.name + " Atlas";
                AssetDatabase.AddObjectToAsset(texture, asset);
            }
            material.name = asset.name + " Material";
            AssetDatabase.AddObjectToAsset(material, asset);
        }

        // A character the atlas lacks is acceptable only when the FACE lacks it -- and never a shipped one. Anything else means the atlas
        // page was too small (the glyphs did not fit)
        private static void CheckMissing(string path, Font font, string missing)
        {
            if (string.IsNullOrEmpty(missing)) return;
            var shipped = ShippedTextCharacters.Collect();
            var problems = new StringBuilder();
            foreach (char c in missing.Distinct())
            {
                if (FaceHas(font, c)) problems.Append(" '" + c + "' (U+" + ((int)c).ToString("X4") + ": the face has it -- the atlas is full)");
                else if (shipped.TryGetValue(c, out string where)) problems.Append(" '" + c + "' (U+" + ((int)c).ToString("X4") + ", shipped in " + where + ": the face lacks it)");
            }
            if (problems.Length > 0) throw new InvalidOperationException("[Fonts] " + path + ":" + problems);
        }

        // Whether the .ttf itself has a glyph for `c` (TextCore's font engine reads its cmap)
        public static bool FaceHas(Font font, char c)
        {
            if (FontEngine.LoadFontFace(font) != FontEngineError.Success) throw new InvalidOperationException("[Fonts] the font engine cannot read " + font.name);
            return FontEngine.TryGetGlyphIndex(c, out uint index) && index != 0;
        }
    }
}
