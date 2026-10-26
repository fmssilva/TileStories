using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    // Every character a visitor can be shown, each with one place it comes from: the base set, the framework's card string table, the
    // app's own table, every option label of the block kinds and every string of the wall's config.json (its content). ONE list for the
    // two readers that must agree: FontAtlasBuilder, which puts these glyphs in the static atlases, and ShippedTextGlyphTests, which fails
    // naming the character and its source the day a text holds one an atlas lacks (_3.3 R12.3; the marker fonts since _3.2 step 13).
    public static class ShippedTextCharacters
    {
        public const string FrameworkTablePath = "Assets/Framework/Runtime/UI/Screen/Cards/CardStrings.asset";
        public const string AppTablePath = "Assets/Apps/LivingRoom/Resources/LivingRoom/LivingRoomCardStrings.asset";
        public const string WallConfigPath = "Assets/Apps/LivingRoom/config.json";

        // Printable ASCII and the Portuguese letters, upper and lower case: what any marker label of a Latin / Portuguese wall may hold even
        // before a config types it (a POI renamed live in the POI Editor must not need a font rebuild)
        public const string BaseCharacters =
            " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~" +
            "ÀÁÂÃÇÉÊÍÓÔÕÚÜ" +
            "àáâãçéêíóôõúü";

        // The character -> where it was first met. Throws when a source is missing (a list built without it would be silently short)
        public static Dictionary<char, string> Collect()
        {
            var found = new Dictionary<char, string>();
            Add(found, BaseCharacters, "the base set");
            foreach (var (path, label) in new[] { (FrameworkTablePath, "CardStrings.asset"), (AppTablePath, "LivingRoomCardStrings.asset") })
            {
                var table = AssetDatabase.LoadAssetAtPath<CardStringTable>(path);
                if (table == null) throw new InvalidOperationException("[Fonts] the string table does not load: " + path);
                foreach (var row in table.rows)
                    foreach (var entry in row.text)
                        Add(found, entry.value, label + " row " + row.key + " (" + entry.lang + ")");
            }
            foreach (var kind in BlockRegistry.Shared.All)
                foreach (var field in kind.Fields)
                    if (field.OptionLabels != null)
                        foreach (string label in field.OptionLabels)
                            Add(found, label, "the option labels of " + kind.Key + "." + field.Key);
            var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText(WallConfigPath));
            if (config == null) throw new InvalidOperationException("[Fonts] the wall's config.json does not parse: " + WallConfigPath);
            WalkStrings(config, s => Add(found, s, "config.json"));
            return found;
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
