using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Editor.Tests
{
    // Drawing the POI Editor must never change the config: OnGUI runs every frame, so a write there fires just
    // because a section is open -- it lights Save, adds an undo step and, worse, can change what the wall does
    // (an unset Priority became 1 = top priority, a missing Outline mode became "uniform" = outlines ON). A file
    // is made explicit ONCE, on load. Every section of both tabs is opened on the REAL window (PoiEditorWindowHost).
    public class DrawingNeverWritesTests
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;

        // Open every section, sub-foldout and POI of one tab, draw it, and return what drawing changed
        private static IEnumerator DrawEverything(WallConfigData config, string tab, List<string> changes)
        {
            var host = new PoiEditorWindowHost(config, "_showGlobalMarker");
            try
            {
                var type = typeof(POIEditorToolWindow);
                foreach (var f in type.GetFields(Instance).Where(f => f.FieldType == typeof(bool) && f.Name.StartsWith("_show")))
                    f.SetValue(host.Editor, true);
                var poiFoldouts = (Dictionary<string, bool>)type.GetField("_poiFoldouts", Instance).GetValue(host.Editor);
                foreach (var poi in config.pois) poiFoldouts[poi.id] = true;
                // - every Card Content block row open too, so its field rows are drawn
                var blockFoldouts = (Dictionary<string, bool>)type.GetField("_cardBlockFoldouts", Instance).GetValue(host.Editor);
                foreach (var poi in config.pois)
                    foreach (var block in poi.card?.blocks ?? new List<BlockInstanceData>())
                        blockFoldouts[poi.id + "/" + block.key] = true;
                // - a composed block's every part open as well: the Motion table, the palettes, the link list, and every element row (Page and
                //   Preview) with its Look / Motion / Tap
                var composed = (Dictionary<string, bool>)type.GetField("_composedOpen", Instance).GetValue(host.Editor);
                foreach (var poi in config.pois)
                    foreach (var block in poi.card?.blocks ?? new List<BlockInstanceData>())
                    {
                        foreach (string part in new[] { "motion", "page/add", "preview/link", "preview/add" })
                            composed[POIEditorToolWindow.ComposedKey(poi.id, block.key, part)] = true;
                        foreach (var field in block.fields ?? new List<BlockFieldValue>())
                            foreach (var row in field?.items ?? new List<BlockItemData>())
                            {
                                string id = new ElementRow(row).Id;
                                if (string.IsNullOrEmpty(id)) continue;
                                foreach (string part in new[] { "", "/Look", "/Motion", "/Tap" })
                                    composed[POIEditorToolWindow.ComposedKey(poi.id, block.key, "row/" + id + part)] = true;
                            }
                    }
                // - every block template open (Detail Card > Block Library > Templates), each of its rows with its parts, and its palette (_3.3 S3b)
                foreach (var template in BlockTemplates.All(config.card_settings, ElementRegistry.Shared))
                {
                    string owner = POIEditorToolWindow.TemplateOwner(template.key);
                    foreach (string part in new[] { "open", "motion", "page/add" }) composed[POIEditorToolWindow.ComposedKey(owner, "", part)] = true;
                    foreach (var row in BlockTemplates.RowsOf(template))
                        foreach (string part in new[] { "", "/Look", "/Motion", "/Tap" })
                            composed[POIEditorToolWindow.ComposedKey(owner, "", "row/" + new ElementRow(row).Id + part)] = true;
                }
                // - every compact dropdown open (a picture's motion effects), its list of options drawn
                for (int i = 0; i < 40; i++)
                    foreach (string table in new[] { "Story page", "Story preview", "Template page" })
                        composed["dropdown/" + table + " motion effect#" + i] = true;
                var tabField = type.GetField("_selectedTab", Instance);
                tabField.SetValue(host.Editor, System.Enum.Parse(tabField.FieldType, tab));

                string[] before = JsonUtility.ToJson(host.Config, true).Split('\n');
                yield return host.WaitForRepaint();
                yield return host.WaitForRepaint();
                string[] after = JsonUtility.ToJson(host.Config, true).Split('\n');

                for (int i = 0; i < Mathf.Min(before.Length, after.Length); i++)
                    if (before[i] != after[i]) changes.Add(tab + ": " + before[i].Trim() + "  ->  " + after[i].Trim());
                if (before.Length != after.Length) changes.Add(tab + ": the config grew or shrank");
                if (host.Unsaved) changes.Add(tab + ": Save was lit by drawing alone");
            }
            finally { host.Close(); }
        }

        [UnityTest]
        public IEnumerator TheShippedWall_WithEverySectionOpen_IsNeverChangedByDrawing()
        {
            var changes = new List<string>();
            foreach (string tab in new[] { "GlobalScene", "SpecificMarker", "DetailCard" })
            {
                var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));
                Assert.Greater(config.pois.Count, 10, "precondition: the real wall, every POI opened");
                Assert.IsTrue(config.pois.Any(p => p.card.blocks.Count > 0), "precondition: authored card blocks are drawn too");
                yield return DrawEverything(config, tab, changes);
            }
            CollectionAssert.IsEmpty(changes, "drawing changed the config:\n" + string.Join("\n", changes));
        }

        // The story's states the shipped wall does not hold -- a Custom preview with a link, an own row and a BROKEN link, both overrides on,
        // minutes set by hand -- drawn with every part open: nothing changes either (_3.3 S3a)
        [UnityTest]
        public IEnumerator AStoryWithACustomPreview_ABrokenLink_AndItsOverridesOn_IsNeverChangedByDrawing()
        {
            var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));
            var painting = config.pois.Single(p => p.id == "painting");
            var story = painting.card.blocks.Single(b => b.kind == BuiltInBlocks.StoryKind);
            Assert.IsTrue(BlockRegistry.Shared.TryGet(BuiltInBlocks.StoryKind, out var kind));
            var scope = ElementScope.Of(painting, story, BlockRegistry.Shared);
            ElementEdits.UseInPreview(scope, ElementRegistry.Shared, "el_8", linked: true);
            ElementEdits.UseInPreview(scope, ElementRegistry.Shared, "el_3", linked: false);
            var page = ElementEdits.PageList(story, kind);
            page.RemoveAll(r => new ElementRow(r).Id == "el_8");
            ElementEdits.SetOverride(story, ElementOptions.ComponentOpenRow, true, config.card_settings.components);
            ElementEdits.SetOverride(story, ElementOptions.ComponentPageChrome, true, config.card_settings.components);
            ElementEdits.SetMinutes(story, 7);
            Assert.AreEqual(1, ElementPreviewRule.Of(scope, ElementRegistry.Shared).Entries.Count(e => e.Source == ElementPreviewRule.Source.Broken),
                "precondition: one broken link");

            var changes = new List<string>();
            yield return DrawEverything(config, "SpecificMarker", changes);
            CollectionAssert.IsEmpty(changes, "drawing changed the config:\n" + string.Join("\n", changes));
        }

        // A wall with templates of its own (one edited, one deleted that a block still names), a block made from a template, a story with Tap
        // Nothing and Block Entry None -- every template, block and part open on both tabs that draw them: nothing changes (_3.3 S3b)
        [UnityTest]
        public IEnumerator TemplatesAndThePreviewsTapAndEntry_WithEverythingOpen_AreNeverChangedByDrawing()
        {
            var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));
            var painting = config.pois.Single(p => p.id == "painting");
            var story = painting.card.blocks.Single(b => b.kind == BuiltInBlocks.StoryKind);
            Assert.IsTrue(BlockRegistry.Shared.TryGet(BuiltInBlocks.StoryKind, out var kind));
            var saved = BlockTemplates.SaveAs(config.card_settings, config.pois, story, kind, "Mine", ElementRegistry.Shared);
            BlockTemplates.Duplicate(config.card_settings, config.pois, BlockTemplates.BuiltIn(ElementRegistry.Shared)[1], ElementRegistry.Shared);
            var camera = config.pois.Single(p => p.id == "camera");
            camera.card ??= new POICardData();
            camera.card.blocks.Add(BlockTemplates.NewBlock(camera.card, BlockTemplates.BuiltIn(ElementRegistry.Shared)[0], ElementRegistry.Shared));
            BlockTemplates.Remove(config.card_settings, saved.key);
            ElementEdits.SetChoice(story, BlockSurfaces.PreviewTapField, ElementOptions.PreviewTapNone);
            ElementEdits.SetChoice(story, BlockSurfaces.PreviewEntryField, ElementOptions.PreviewEntryNone);
            Assert.AreEqual(saved.key, BlockSurfaces.TemplateOf(story), "precondition: the story names a deleted template");

            var changes = new List<string>();
            foreach (string tab in new[] { "SpecificMarker", "DetailCard" })
                yield return DrawEverything(JsonUtility.FromJson<WallConfigData>(JsonUtility.ToJson(config)), tab, changes);
            CollectionAssert.IsEmpty(changes, "drawing changed the config:\n" + string.Join("\n", changes));
        }

        // An older or script-written file leaves out the optional fields (no Priority, no Outline mode, no shape,
        // no effects). Loaded through the real LoadConfig it becomes explicit WITHOUT changing what the wall does,
        // and from then on drawing changes nothing.
        [UnityTest]
        public IEnumerator AFileWithoutTheOptionalFields_LoadsAsWhatItAlreadyMeant_AndDrawingThenChangesNothing()
        {
            const string minimalJson =
                "{\"category_styles\":[{\"key\":\"category_1\",\"label\":\"Civic\"}]," +
                "\"hierarchy_levels\":[{\"key\":\"level_1\",\"level_name\":\"Hub\",\"size_cm\":10}," +
                "{\"key\":\"level_2\",\"level_name\":\"Detail\",\"size_cm\":5}," +
                "{\"key\":\"level_3\",\"level_name\":\"Tiny\",\"size_cm\":2,\"priority\":1}]," +
                "\"pois\":[{\"id\":\"poi_1\",\"name\":\"Gate\",\"category\":\"category_1\",\"hierarchy_level_key\":\"level_2\"}]}";

            // What the runtime does with the file as written, BEFORE the Editor ever sees it
            var raw = JsonUtility.FromJson<WallConfigData>(minimalJson);
            var runtimePriority = new Dictionary<string, int>();
            MarkerHierarchyResolver.Configure(raw.hierarchy_levels);
            try
            {
                foreach (var level in raw.hierarchy_levels)
                {
                    Assert.IsTrue(MarkerHierarchyResolver.TryResolvePriority(level.key, out int p));
                    runtimePriority[level.key] = p;
                }
            }
            finally { MarkerHierarchyResolver.ResetToDefaults(); }
            var runtimeLook = MarkerVisualSettings.Resolve(raw, null);

            // Load it through the real window
            string dir = Path.GetFullPath("Temp/__DrawingNeverWritesTests");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "config.json"), minimalJson);
            var loader = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            WallConfigData loaded;
            try
            {
                typeof(POIEditorToolWindow).GetField("_configPath", Instance).SetValue(loader, Path.Combine(dir, "config.json"));
                LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(@"\[POIEditor\] set 2 hierarchy level Priority"));
                typeof(POIEditorToolWindow).GetMethod("LoadConfig", Instance).Invoke(loader, null);
                loaded = (WallConfigData)typeof(POIEditorToolWindow).GetField("_config", Instance).GetValue(loader);
                Assert.IsTrue((bool)typeof(POIEditorToolWindow).GetField("_hasUnsavedChanges", Instance).GetValue(loader),
                    "the load made the file explicit, so Save is lit to keep it");
            }
            finally
            {
                Object.DestroyImmediate(loader);
                Directory.Delete(dir, true);
            }

            // Explicit now, and meaning exactly what it meant
            foreach (var level in loaded.hierarchy_levels)
                Assert.AreEqual(runtimePriority[level.key], level.priority, level.level_name + ": Priority stored as the order the runtime used");
            CollectionAssert.AreEqual(new[] { 1, 2, 1 }, loaded.hierarchy_levels.Select(l => l.priority).ToList(),
                "Hub and Detail were unset (row order 1, 2); Tiny kept its own 1");
            Assert.AreEqual("none", loaded.marker_outline_mode, "no Outline mode in the file = no outline, and the Editor now says so");
            Assert.AreEqual(runtimeLook.OutlineMode, MarkerVisualSettings.Resolve(loaded, null).OutlineMode, "the wall still runs with the same outline mode");
            Assert.AreEqual(MarkerOutlineMode.None, runtimeLook.OutlineMode);
            Assert.AreEqual("circle", loaded.marker_shape, "no shape in the file = the framework default shape");
            Assert.IsTrue(loaded.hierarchy_levels.All(l => l.ripple_effect == "none" && l.halo_effect == "none"));

            // ...and drawing every section of both tabs changes nothing
            var changes = new List<string>();
            foreach (string tab in new[] { "GlobalScene", "SpecificMarker", "DetailCard" })
                yield return DrawEverything(JsonUtility.FromJson<WallConfigData>(JsonUtility.ToJson(loaded)), tab, changes);
            CollectionAssert.IsEmpty(changes, "drawing changed the config:\n" + string.Join("\n", changes));
        }
    }
}
