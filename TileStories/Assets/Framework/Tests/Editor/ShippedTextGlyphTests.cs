using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using FontAsset = UnityEngine.TextCore.Text.FontAsset;
using TextCoreAtlasPopulationMode = UnityEngine.TextCore.Text.AtlasPopulationMode;

namespace TileStories.Editor.Tests
{
    // The guard of the project's STATIC fonts (__mixed_TODOs "The two SDF fonts dirty themselves"; _3.3 R12.3): a static font asset adds no
    // glyph at run time, so every character a visitor can be shown must already sit in its character table. The characters are walked from
    // what the project ships -- the framework's card string table, the app's own table, the block kinds' option labels and the wall's
    // config.json (ShippedTextCharacters, the same list FontAtlasBuilder builds from). The day someone types a new accent (or a language
    // arrives) a test fails, naming the character, where it came from and the font, instead of a label showing an empty box.
    public class ShippedTextGlyphTests
    {
        public static readonly string[] MarkerFontPaths =
        {
            "Assets/Framework/Runtime/UI/World/Markers/Fonts/Oswald Bold SDF.asset",
            "Assets/Framework/Runtime/UI/World/Markers/Fonts/Roboto-Bold SDF.asset",
        };

        private static Dictionary<char, string> Shipped()
        {
            var characters = ShippedTextCharacters.Collect();
            // - the walk really read Portuguese: a test that collected ASCII only would pass on any font
            Assert.IsTrue(characters.ContainsKey('ç') && characters.ContainsKey('ã'),
                "precondition: the shipped text holds Portuguese (a c-cedilla and an a-tilde)");
            Assert.Greater(characters.Count, ShippedTextCharacters.BaseCharacters.Length, "precondition: the shipped text adds characters beyond the base set");
            return characters;
        }

        // Every shipped character `inFont` lacks, as "'x' (U+0078, from <source>)"
        private static List<string> MissingShipped(Dictionary<char, string> shipped, IEnumerable<uint> inFont)
        {
            var have = new HashSet<uint>(inFont);
            return shipped.Where(c => !have.Contains(c.Key)).Select(c => "'" + c.Key + "' (U+" + ((int)c.Key).ToString("X4") + ", from " + c.Value + ")").ToList();
        }

        [Test]
        public void EveryShippedCharacter_IsInBothMarkerFonts_AndBothAreStatic()
        {
            var characters = Shipped();
            foreach (string path in MarkerFontPaths)
            {
                var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                Assert.IsNotNull(font, "precondition: the font asset loads: " + path);
                Assert.AreEqual(AtlasPopulationMode.Static, font.atlasPopulationMode,
                    path + " is Static: a Dynamic font writes every new glyph back into the asset (git sees it change on every Editor launch)");
                var missing = MissingShipped(characters, font.characterTable.Select(c => c.unicode));
                Assert.IsEmpty(missing, path + " lacks " + missing.Count + " shipped character(s): " + string.Join(", ", missing.Take(12)));
            }
        }

        // The app's faces (_3.3 P4.3): every .ttf under the theme's font folder has its two assets, both static; every shipped and content
        // character is in BOTH (the markers may pick the key, the card draws it); and every character of the built set is there unless the
        // face itself lacks it (read from the .ttf's own cmap, not from a typed list)
        [Test]
        public void EveryThemeFace_HasStaticTmpAndUiAssets_HoldingEveryShippedCharacter_AndTheWholeSetTheFaceHas()
        {
            var characters = Shipped();
            string set = FontAtlasBuilder.CharacterSet();
            var sources = FontAtlasBuilder.SourceFonts();
            Assert.AreEqual(4, sources.Length, "precondition: the theme ships 4 faces (Fraunces SemiBold, Atkinson Next Regular / Medium / Bold)");
            foreach (string ttf in sources)
            {
                var face = AssetDatabase.LoadAssetAtPath<Font>(ttf);
                var tmp = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAtlasBuilder.TmpPathFor(ttf));
                var ui = AssetDatabase.LoadAssetAtPath<FontAsset>(FontAtlasBuilder.UiPathFor(ttf));
                Assert.IsNotNull(tmp, ttf + ": its TMP asset exists (TileStories > Fonts > Rebuild Theme Font Atlases)");
                Assert.IsNotNull(ui, ttf + ": its UI Toolkit asset exists (TileStories > Fonts > Rebuild Theme Font Atlases)");
                Assert.AreEqual(AtlasPopulationMode.Static, tmp.atlasPopulationMode, tmp.name + " is Static");
                Assert.AreEqual(TextCoreAtlasPopulationMode.Static, ui.atlasPopulationMode, ui.name + " is Static");

                var tmpCodes = tmp.characterTable.Select(c => c.unicode).ToList();
                var uiCodes = ui.characterTable.Select(c => c.unicode).ToList();
                var missing = MissingShipped(characters, tmpCodes).Select(m => tmp.name + ": " + m)
                    .Concat(MissingShipped(characters, uiCodes).Select(m => ui.name + ": " + m)).ToList();
                Assert.IsEmpty(missing, missing.Count + " shipped character(s) missing: " + string.Join(", ", missing.Take(12)));

                var faceHas = set.Where(c => FontAtlasBuilder.FaceHas(face, c)).ToList();
                Assert.Greater(faceHas.Count, 250, ttf + ": precondition: the face holds most of Latin-1 + Latin Extended-A");
                foreach (var (name, codes) in new[] { (tmp.name, tmpCodes), (ui.name, uiCodes) })
                {
                    var notBuilt = faceHas.Where(c => !codes.Contains(c)).Select(c => "U+" + ((int)c).ToString("X4")).ToList();
                    Assert.IsEmpty(notBuilt, name + " lacks " + notBuilt.Count + " character(s) its face has: " + string.Join(", ", notBuilt.Take(12)));
                }
            }
        }

        // The two UI atlases of every face (_3.3 P3e): the one every text draws keeps the TIGHT SDF (a wide one greyed the body ink); the
        // halo twin -- only the hero words' back labels -- the wide one, static, with exactly the same characters and the same weight
        // table among the twins; the theme stylesheet names both
        [Test]
        public void EveryThemeFace_DrawsTextFromATightAtlas_AndItsHaloTwinFromAWideOne_TheThemeNamingBoth()
        {
            foreach (string ttf in FontAtlasBuilder.SourceFonts())
            {
                var ui = AssetDatabase.LoadAssetAtPath<FontAsset>(FontAtlasBuilder.UiPathFor(ttf));
                var halo = AssetDatabase.LoadAssetAtPath<FontAsset>(FontAtlasBuilder.HaloPathFor(ttf));
                Assert.IsNotNull(halo, ttf + ": its halo twin exists (TileStories > Fonts > Rebuild Theme Font Atlases)");
                Assert.AreSame(halo, FontAtlasBuilder.HaloTwinOf(ui), ui.name + ": HaloTwinOf finds it");
                Assert.AreEqual(FontAtlasBuilder.UiAtlasPadding, ui.atlasPadding, ui.name + ": the tight SDF");
                Assert.AreEqual(6, FontAtlasBuilder.UiAtlasPadding, "the text atlases keep the markers' padding (6 at 48 pt)");
                Assert.AreEqual(FontAtlasBuilder.HaloAtlasPadding, halo.atlasPadding, halo.name + ": the wide SDF");
                Assert.AreEqual(HeroWordsOnPicture.HaloReach, FontAtlasBuilder.HaloAtlasPadding / (float)FontAtlasBuilder.SamplingPointSize, 1e-4f,
                    "the halo's reach is the halo atlas's spread");
                Assert.AreEqual(TextCoreAtlasPopulationMode.Static, halo.atlasPopulationMode, halo.name + " is Static");
                CollectionAssert.AreEquivalent(ui.characterTable.Select(c => c.unicode), halo.characterTable.Select(c => c.unicode),
                    halo.name + " holds exactly its face's characters");
                var weights = new SerializedObject(ui).FindProperty("m_FontWeightTable");
                var haloWeights = new SerializedObject(halo).FindProperty("m_FontWeightTable");
                for (int i = 0; i < weights.arraySize; i++)
                {
                    var face = weights.GetArrayElementAtIndex(i).FindPropertyRelative("regularTypeface").objectReferenceValue as FontAsset;
                    var twin = haloWeights.GetArrayElementAtIndex(i).FindPropertyRelative("regularTypeface").objectReferenceValue as FontAsset;
                    Assert.AreSame(face == null ? null : FontAtlasBuilder.HaloTwinOf(face), twin, halo.name + ": weight " + (i * 100) + " is its face's twin");
                }
            }
            string uss = System.IO.File.ReadAllText("Assets/Framework/Runtime/UI/Theme/Generated/Theme.default.uss");
            StringAssert.Contains("--ts-font-body-halo: url(\"/Assets/Framework/Runtime/UI/Theme/Fonts/AtkinsonHyperlegibleNext/AtkinsonHyperlegibleNext-Regular UITK Halo.asset\")", uss);
            StringAssert.Contains("--ts-font-display-halo: url(\"/Assets/Framework/Runtime/UI/Theme/Fonts/Fraunces/Fraunces-SemiBold UITK Halo.asset\")", uss);
        }
    }
}
