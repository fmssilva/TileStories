using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Every registered block kind (_3.1 section 7, micro-cycle step 2): an instance with EVERY field of its definition
    // filled (item rows included) goes into a POI through the POI Editor's own history (DrawConfigMutationScope), is
    // undone and redone, survives Save / load (JsonUtility), is read back value by value through BlockFieldReader, is
    // kept by BlockStackBuilder and binds on the kind's real view. A kind added later is covered with no test edit.
    public class BlockKindRoundTripTests
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;
        // The file type a filled Asset field carries: one its media kind's rule accepts (a picture unless the field names another kind)
        private static string AssetExtension(BlockFieldDefinition field)
        {
            var extensions = MediaPathRule.ExtensionsOf(field.Media);
            return extensions.Length > 0 ? extensions[0] : ".png";
        }

        private static readonly string[] Languages = { "en", "pt" };

        private static IEnumerable<string> KindKeys => BlockRegistry.Shared.All.Select(k => k.Key);

        private static string TextOf(string key, string lang) => key + " in " + lang;

        // A row's colour: both written forms the Editor accepts
        private static string ColourOf(int row) => row == 0 ? "#1F3F8F" : "#abc";
        // A row's time: minutes:seconds, then plain seconds (TimeCodeRule reads both)
        private static string TimeOf(int row) => row == 0 ? "1:30" : "95";

        // One value of every field type, from the definition
        private static BlockFieldValue Filled(BlockFieldDefinition f)
        {
            var v = new BlockFieldValue { key = f.Key };
            switch (f.Type)
            {
                case BlockFieldType.LocalizedText:
                case BlockFieldType.LocalizedLongText:
                    v.text = Languages.Select(l => new LocalizedEntry { lang = l, value = TextOf(f.Key, l) }).ToList();
                    break;
                case BlockFieldType.Number: v.number = 3.5f; break;
                case BlockFieldType.Toggle: v.flag = true; break;
                case BlockFieldType.Choice: v.value = f.Options[f.Options.Count - 1]; break;
                case BlockFieldType.Asset: v.asset = "folder/" + f.Key + AssetExtension(f); break;
                case BlockFieldType.PoiRef: v.value = "poi_2"; break;
                case BlockFieldType.Color: v.value = "#1F3F8F"; break;
                case BlockFieldType.Url: v.value = "https://example.org/" + f.Key; break;
                case BlockFieldType.Time: v.value = TimeOf(0); break;
                case BlockFieldType.Items:
                    for (int row = 0; row < 2; row++)
                    {
                        var item = new BlockItemData();
                        foreach (var sub in f.ItemFields)
                        {
                            var s = new BlockItemFieldValue { key = sub.Key };
                            if (sub.Type == BlockFieldType.Choice) s.value = sub.Options[row % sub.Options.Count];
                            else if (sub.Type == BlockFieldType.Number) s.number = row + 1;
                            else if (sub.Type == BlockFieldType.Color) s.value = ColourOf(row);
                            else if (sub.Type == BlockFieldType.Toggle) s.flag = row == 0;
                            else if (sub.Type == BlockFieldType.Asset) s.asset = "folder/" + sub.Key + row + AssetExtension(sub);
                            else if (sub.Type == BlockFieldType.PoiRef) s.value = "poi_" + (row + 2);
                            else if (sub.Type == BlockFieldType.Url) s.value = "https://example.org/" + sub.Key + row;
                            else if (sub.Type == BlockFieldType.Time) s.value = TimeOf(row);
                            else s.text = Languages.Select(l => new LocalizedEntry { lang = l, value = TextOf(sub.Key + row, l) }).ToList();
                            item.fields.Add(s);
                        }
                        v.items.Add(item);
                    }
                    break;
            }
            return v;
        }

        [Test]
        public void EveryKind_WithEveryFieldFilled_SurvivesTheWindowHistory_SaveAndLoad_AndReadsBackOnItsView([ValueSource(nameof(KindKeys))] string kindKey)
        {
            Assert.IsTrue(BlockRegistry.Shared.TryGet(kindKey, out var kind));
            var block = new BlockInstanceData { key = "block_1", kind = kind.Key, variant = kind.Variants[kind.Variants.Count - 1], display = kind.DisplayModes[0] };
            foreach (var f in kind.Fields) block.fields.Add(Filled(f));

            var t = typeof(POIEditorToolWindow);
            var window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            try
            {
                var config = new WallConfigData { wall_id = "rt" };
                config.card_settings.languages = new List<string>(Languages);
                config.pois.Add(new POIData { id = "poi_1", name = "North Tower", has_status = true, status_pct = 20f });
                // - a status on both: kinds that compare the POI with another (poi_2) have something to show
                config.pois.Add(new POIData { id = "poi_2", name = "South Gate", has_status = true, status_pct = 60f });
                t.GetField("_config", Instance).SetValue(window, config);
                t.GetMethod("InitializeConfigHistory", Instance).Invoke(window, null);
                WallConfigData Live() => (WallConfigData)t.GetField("_config", Instance).GetValue(window);

                t.GetMethod("DrawConfigMutationScope", Instance).Invoke(window, new object[] { (Action)(() => Live().pois[0].card.blocks.Add(block)), false });
                string written = JsonUtility.ToJson(Live().pois[0].card);
                t.GetMethod("UndoConfigChange", Instance).Invoke(window, null);
                CollectionAssert.IsEmpty(Live().pois[0].card.blocks, kindKey + ": Ctrl+Z removes the block");
                t.GetMethod("RedoConfigChange", Instance).Invoke(window, null);
                Assert.AreEqual(written, JsonUtility.ToJson(Live().pois[0].card), kindKey + ": Ctrl+Y brings back every value");

                var loaded = JsonUtility.FromJson<WallConfigData>(JsonUtility.ToJson(Live(), true));
                var back = loaded.pois[0].card.blocks.Single();
                foreach (var f in kind.Fields)
                    foreach (string lang in Languages)
                        AssertReadsBack(new BlockFieldReader(back, lang, "en"), f, lang, kindKey);

                var built = BlockStackBuilder.Build(loaded.pois[0], loaded.card_settings, BlockRegistry.Shared, loaded.pois);
                CollectionAssert.IsEmpty(built.Skipped, kindKey + ": a block with every field filled is shown");
                if (kind.Key != BuiltInBlocks.HeaderKind) Assert.AreEqual(kind.Key, built.Entries.Last().Definition.Key);

                var view = BlockRegistry.Shared.CreateView(kind.Key);
                view.Bind(back, new BlockBindContext { Poi = loaded.pois[0], Taxonomy = loaded, Variant = back.variant, Language = "pt", FallbackLanguage = "en" });
                Assert.IsNotNull(view.Root, kindKey + ": the view binds every field");
                view.Unbind();
            }
            finally { UnityEngine.Object.DestroyImmediate(window); }
        }

        private static void AssertReadsBack(BlockFieldReader read, BlockFieldDefinition f, string lang, string kind)
        {
            string where = kind + "." + f.Key + " (" + lang + ")";
            switch (f.Type)
            {
                case BlockFieldType.LocalizedText:
                case BlockFieldType.LocalizedLongText: Assert.AreEqual(TextOf(f.Key, lang), read.Text(f.Key), where); break;
                case BlockFieldType.Number: Assert.AreEqual(3.5f, read.Number(f.Key), where); break;
                case BlockFieldType.Toggle: Assert.IsTrue(read.Flag(f.Key), where); break;
                case BlockFieldType.Choice: Assert.AreEqual(f.Options[f.Options.Count - 1], read.Value(f.Key), where); break;
                case BlockFieldType.Asset: Assert.AreEqual("folder/" + f.Key + AssetExtension(f), read.Asset(f.Key), where); Assert.AreEqual("folder/" + f.Key + AssetExtension(f), read.ValidAsset(f.Key, f.Media == MediaKind.None ? MediaKind.Image : f.Media), where + ": a path the rule accepts"); break;
                case BlockFieldType.PoiRef: Assert.AreEqual("poi_2", read.Value(f.Key), where); break;
                case BlockFieldType.Color: Assert.AreEqual("#1F3F8F", read.Value(f.Key), where); break;
                case BlockFieldType.Url: Assert.AreEqual("https://example.org/" + f.Key, read.OpenableUrl(f.Key), where + ": a link the card opens"); break;
                case BlockFieldType.Time: Assert.AreEqual(TimeOf(0), read.Value(f.Key), where); break;
                case BlockFieldType.Items:
                    var items = read.Items(f.Key);
                    Assert.AreEqual(2, items.Count, where + ": both rows");
                    for (int row = 0; row < 2; row++)
                        foreach (var sub in f.ItemFields)
                        {
                            if (sub.Type == BlockFieldType.Choice)
                                Assert.AreEqual(sub.Options[row % sub.Options.Count], items[row].fields.Single(x => x.key == sub.Key).value, where + " row " + row);
                            else if (sub.Type == BlockFieldType.Number)
                                Assert.AreEqual(row + 1, items[row].fields.Single(x => x.key == sub.Key).number, where + " row " + row);
                            else if (sub.Type == BlockFieldType.Color)
                            {
                                Assert.AreEqual(ColourOf(row), read.ItemValue(items[row], sub.Key), where + " row " + row);
                                Assert.IsTrue(read.ItemColor(items[row], sub.Key, out _), where + " row " + row + ": a colour the card accepts");
                            }
                            else if (sub.Type == BlockFieldType.Toggle)
                                Assert.AreEqual(row == 0, read.ItemFlag(items[row], sub.Key), where + " row " + row);
                            else if (sub.Type == BlockFieldType.Asset)
                                Assert.AreEqual("folder/" + sub.Key + row + AssetExtension(sub), read.ItemValidAsset(items[row], sub.Key, sub.Media), where + " row " + row + ": a path the rule accepts");
                            else if (sub.Type == BlockFieldType.PoiRef)
                                Assert.AreEqual("poi_" + (row + 2), read.ItemValue(items[row], sub.Key), where + " row " + row);
                            else if (sub.Type == BlockFieldType.Url)
                                Assert.AreEqual("https://example.org/" + sub.Key + row, read.ItemValue(items[row], sub.Key), where + " row " + row);
                            else if (sub.Type == BlockFieldType.Time)
                            {
                                Assert.AreEqual(TimeOf(row), read.ItemValue(items[row], sub.Key), where + " row " + row);
                                Assert.IsTrue(read.ItemTime(items[row], sub.Key, out float at), where + " row " + row + ": a time the card reads");
                                Assert.AreEqual(row == 0 ? 90f : 95f, at, 0.001f, where + " row " + row);
                            }
                            else
                                Assert.AreEqual(TextOf(sub.Key + row, lang), read.ItemText(items[row], sub.Key), where + " row " + row + "." + sub.Key);
                        }
                    break;
            }
        }
    }
}
