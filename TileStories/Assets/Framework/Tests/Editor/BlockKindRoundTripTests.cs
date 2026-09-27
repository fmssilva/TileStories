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
        private static readonly string[] Languages = { "en", "pt" };

        private static IEnumerable<string> KindKeys => BlockRegistry.Shared.All.Select(k => k.Key);

        private static string TextOf(string key, string lang) => key + " in " + lang;

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
                case BlockFieldType.Asset: v.asset = "folder/" + f.Key + ".png"; break;
                case BlockFieldType.PoiRef: v.value = "poi_2"; break;
                case BlockFieldType.Items:
                    for (int row = 0; row < 2; row++)
                    {
                        var item = new BlockItemData();
                        foreach (var sub in f.ItemFields)
                        {
                            var s = new BlockItemFieldValue { key = sub.Key };
                            if (sub.Type == BlockFieldType.Choice) s.value = sub.Options[row % sub.Options.Count];
                            else if (sub.Type == BlockFieldType.Number) s.number = row + 1;
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
                config.pois.Add(new POIData { id = "poi_2", name = "South Gate" });
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

                var built = BlockStackBuilder.Build(loaded.pois[0], loaded.card_settings, BlockRegistry.Shared);
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
                case BlockFieldType.Asset: Assert.AreEqual("folder/" + f.Key + ".png", read.Asset(f.Key), where); break;
                case BlockFieldType.PoiRef: Assert.AreEqual("poi_2", read.Value(f.Key), where); break;
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
                            else
                                Assert.AreEqual(TextOf(sub.Key + row, lang), read.ItemText(items[row], sub.Key), where + " row " + row + "." + sub.Key);
                        }
                    break;
            }
        }
    }
}
