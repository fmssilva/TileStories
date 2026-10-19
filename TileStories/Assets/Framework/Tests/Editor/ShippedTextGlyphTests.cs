using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The guard of the project's two STATIC marker fonts (__mixed_TODOs "The two SDF fonts dirty themselves"): a static TMP font asset adds
    // no glyph at run time, so every character a visitor can be shown must already sit in its character table. The test walks every
    // visitor-facing string the project ships -- the framework's card string table, the app's own table and the wall's config.json -- and
    // asserts each character is in both fonts. The day someone types a new accent (or a language arrives) it fails, naming the character
    // and where it came from, instead of the marker showing an empty box.
    public class ShippedTextGlyphTests
    {
        public static readonly string[] FontPaths =
        {
            "Assets/Framework/Runtime/UI/Markers/Fonts/Oswald Bold SDF.asset",
            "Assets/Framework/Runtime/UI/Markers/Fonts/Roboto-Bold SDF.asset",
        };

        private const string FrameworkTablePath = "Assets/Framework/Runtime/UI/Cards/CardStrings.asset";
        private const string AppTablePath = "Assets/Apps/LivingRoom/Resources/LivingRoom/LivingRoomCardStrings.asset";
        private const string WallConfigPath = "Assets/Apps/LivingRoom/config.json";

        // Printable ASCII and the Portuguese letters, upper and lower case: what any marker label of a Latin / Portuguese wall may hold even
        // before a config types it (a POI renamed live in the POI Editor must not need a font rebuild)
        public const string BaseCharacters =
            " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~" +
            "ÀÁÂÃÇÉÊÍÓÔÕÚÜ" +
            "àáâãçéêíóôõúü";

        // Every character a visitor can be shown, with one place it comes from: the base set, then each string of the three sources.
        // Public: the one-off atlas rebuild reads the same list, so the font and its guard can never disagree on the set
        public static Dictionary<char, string> ShippedCharacters()
        {
            var found = new Dictionary<char, string>();
            Add(found, BaseCharacters, "the base set");
            foreach (var (path, label) in new[] { (FrameworkTablePath, "CardStrings.asset"), (AppTablePath, "LivingRoomCardStrings.asset") })
            {
                var table = AssetDatabase.LoadAssetAtPath<CardStringTable>(path);
                Assert.IsNotNull(table, "precondition: the string table loads: " + path);
                foreach (var row in table.rows)
                    foreach (var entry in row.text)
                        Add(found, entry.value, label + " row " + row.key + " (" + entry.lang + ")");
            }
            var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText(WallConfigPath));
            Assert.IsNotNull(config, "precondition: the wall's config.json parses");
            WalkStrings(config, s => Add(found, s, "config.json"));
            return found;
        }

        [Test]
        public void EveryShippedCharacter_IsInBothMarkerFonts_AndBothAreStatic()
        {
            var characters = ShippedCharacters();
            // - the walk really read Portuguese: a test that collected ASCII only would pass on any font
            Assert.IsTrue(characters.ContainsKey('ç') && characters.ContainsKey('ã'),
                "precondition: the shipped text holds Portuguese (a c-cedilla and an a-tilde)");
            Assert.Greater(characters.Count, BaseCharacters.Length, "precondition: the shipped text adds characters beyond the base set");
            foreach (string path in FontPaths)
            {
                var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                Assert.IsNotNull(font, "precondition: the font asset loads: " + path);
                Assert.AreEqual(AtlasPopulationMode.Static, font.atlasPopulationMode,
                    path + " is Static: a Dynamic font writes every new glyph back into the asset (git sees it change on every Editor launch)");
                var inFont = new HashSet<uint>(font.characterTable.Select(c => c.unicode));
                var missing = characters.Where(c => !inFont.Contains(c.Key)).Select(c => "'" + c.Key + "' (U+" + ((int)c.Key).ToString("X4") + ", from " + c.Value + ")").ToList();
                Assert.IsEmpty(missing, path + " lacks " + missing.Count + " shipped character(s): " + string.Join(", ", missing.Take(12)));
            }
        }

        private static void Add(Dictionary<char, string> found, string text, string where)
        {
            if (string.IsNullOrEmpty(text)) return;
            foreach (char c in text)
            {
                // - a line break or a tab is layout, never a glyph
                if (char.IsControl(c) || found.ContainsKey(c)) continue;
                found[c] = where;
            }
        }

        // Every string field reachable from `node` (the config's serializable classes and their lists: a JsonUtility tree, no cycles)
        private static void WalkStrings(object node, Action<string> visit)
        {
            if (node == null) return;
            foreach (var field in node.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                object value = field.GetValue(node);
                if (value is string s) visit(s);
                else if (value is IList list)
                {
                    foreach (object item in list)
                        if (item is string si) visit(si);
                        else if (item != null && item.GetType().IsClass) WalkStrings(item, visit);
                }
                else if (value != null && field.FieldType.IsClass && field.FieldType.IsSerializable) WalkStrings(value, visit);
            }
        }
    }
}
