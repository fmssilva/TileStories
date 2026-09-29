using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TileStories.Editor.Tests
{
    // Live Play Mode for the POI Detail Card (_3.1 step 12): a card_settings edit or a POI's card edit made in the POI Editor window reaches the
    // OPEN card of the running scene. Two layers here: the applier's fingerprint (pure: every leaf of the card's config, by reflection, changes it and
    // nothing else does), and the real thing -- the real window, real typing and clicks, the real wall scene in Play Mode, the open card read back.
    public class LivePlayModeCardTests
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;
        private const string WallScenePath = "Assets/Apps/LivingRoom/LivingRoomScene.unity";
        private PoiEditorWindowHost _window;

        [TearDown]
        public void TearDown() => _window?.Close();

        private static WallConfigData ShippedConfig() =>
            JsonUtility.FromJson<WallConfigData>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "LivingRoom/config.json")));

        // ---------------- the fingerprint ----------------

        // A card config with every list holding one element whose every leaf is set: something for each field to change
        private static object Filled(Type type)
        {
            var instance = Activator.CreateInstance(type);
            foreach (var f in type.GetFields(BindingFlags.Public | BindingFlags.Instance)) f.SetValue(instance, FilledValue(f.FieldType, f.GetValue(instance)));
            return instance;
        }

        private static object FilledValue(Type t, object current)
        {
            if (t == typeof(string)) return "a";
            if (t == typeof(bool)) return true;
            if (t == typeof(float)) return 0.3f;
            if (t == typeof(int)) return 1;
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))
            {
                var list = (System.Collections.IList)Activator.CreateInstance(t);
                var element = t.GetGenericArguments()[0];
                list.Add(element == typeof(string) ? "a" : Filled(element));
                return list;
            }
            return t.IsClass ? Filled(t) : current;
        }

        // Every leaf field (owner + field) under an object, lists included
        private static IEnumerable<(object Owner, FieldInfo Field)> Leaves(object obj)
        {
            foreach (var f in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var t = f.FieldType;
                var value = f.GetValue(obj);
                bool listOfClasses = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>) && t.GetGenericArguments()[0].IsClass && t.GetGenericArguments()[0] != typeof(string);
                if (listOfClasses)
                    foreach (var element in (System.Collections.IEnumerable)value)
                        foreach (var leaf in Leaves(element)) yield return leaf;
                else if (t.IsClass && t != typeof(string) && !t.IsGenericType) foreach (var leaf in Leaves(value)) yield return leaf;
                else yield return (obj, f);
            }
        }

        // Change one leaf to a different value of its type
        private static void Change(object owner, FieldInfo f)
        {
            object v = f.GetValue(owner);
            if (v is bool b) f.SetValue(owner, !b);
            else if (v is float x) f.SetValue(owner, x + 0.05f);
            else if (v is int i) f.SetValue(owner, i + 1);
            else if (v is string s) f.SetValue(owner, s + "x");
            else if (v is List<string> list) list.Add("x");
            else Assert.Fail("no change for " + f.FieldType + " " + f.Name);
        }

        [Test]
        public void TheFingerprint_ChangesForEveryLeafOfTheCardSettingsAndOfAPoisCard_ByReflection()
        {
            var applier = new LivePlayModeCardApplier();
            var config = new WallConfigData();
            config.card_settings = (CardSettings)Filled(typeof(CardSettings));
            config.pois.Add(new POIData { id = "p1", name = "One", card = (POICardData)Filled(typeof(POICardData)) });

            int leaves = 0;
            foreach (var owner in new object[] { config.card_settings, config.pois[0].card })
                foreach (var (leafOwner, field) in Leaves(owner).ToList())
                {
                    string before = applier.Fingerprint(config);
                    Change(leafOwner, field);
                    Assert.AreNotEqual(before, applier.Fingerprint(config), leafOwner.GetType().Name + "." + field.Name + ": a card edit the running card would miss");
                    leaves++;
                }
            Assert.Greater(leaves, 30, "not vacuous: the walk found the card's many fields (settings, container, demo card, block fields, item rows...)");
        }

        [Test]
        public void TheFingerprint_IgnoresWhatIsNotTheCards_AndNoOtherDomainSeesACardEdit()
        {
            var applier = new LivePlayModeCardApplier();
            var config = ShippedConfig();
            string baseline = applier.Fingerprint(config);
            var poi = config.pois.First(p => p.id == "lamp");
            // - a POI's identity, its summary and keywords, and the markers' look are other domains' fields: the card's fingerprint stays
            config.marker_shape = "hexagon";
            poi.name = "Renamed";
            poi.summary = "Another summary";
            poi.search_keywords.Add("extra");
            poi.hierarchy_level_key = "level_5";
            Assert.AreEqual(baseline, applier.Fingerprint(config), "a marker, a name, a summary or a keyword is not a card edit");

            // - the other way round: every other applier's fingerprint holds still when only the card changes
            var others = typeof(ILivePlayModeApplier).Assembly.GetTypes()
                .Where(t => typeof(ILivePlayModeApplier).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract && t != typeof(LivePlayModeCardApplier))
                .Select(t => (ILivePlayModeApplier)Activator.CreateInstance(t)).ToList();
            Assert.GreaterOrEqual(others.Count, 10, "not vacuous: every other domain's applier was found");
            var fresh = ShippedConfig();
            var before = others.ToDictionary(o => o.Name, o => o.Fingerprint(fresh));
            fresh.card_settings.enabled = !fresh.card_settings.enabled;
            fresh.card_settings.demo_card.enabled = true;
            fresh.pois.First(p => p.id == "lamp").card.blocks[0].variant = "changed";
            foreach (var o in others) Assert.AreEqual(before[o.Name], o.Fingerprint(fresh), o.Name + ": it must not react to a card edit");
        }

        [Test]
        public void TheApplier_IsRegisteredInThePushList_AndAnEditReachesOnlyIt()
        {
            var dispatcher = LivePlayModeConfigPush.CreateDispatcher();
            var appliers = (List<ILivePlayModeApplier>)typeof(LivePlayModeConfigDispatcher).GetField("_appliers", Instance).GetValue(dispatcher);
            Assert.AreEqual(1, appliers.Count(a => a is LivePlayModeCardApplier), "one card applier in the one registration place");
            Assert.AreEqual("detail card", appliers.OfType<LivePlayModeCardApplier>().Single().Name);
        }

        // ---------------- the real window, the real scene, Play Mode ----------------

        private void OpenTab(string tab)
        {
            var tabField = typeof(POIEditorToolWindow).GetField("_selectedTab", Instance);
            _window.SetWindowField("_selectedTab", Enum.Parse(tabField.FieldType, tab));
        }

        // Scroll the window so one drawn control is inside it, the way a person scrolls to it (the shared helper)
        private IEnumerator ScrollWindowTo(string probe) => _window.ScrollTo(probe);

        private static IEnumerator Frames(int count)
        {
            for (int i = 0; i < count; i++) yield return null;
        }

        // Save the Game view (the running card) as TestEvidence/Card/<name>.png, for the vision pass. An EditMode test cannot wait for the end of
        // the frame, so the capture is asked for and its file waited for.
        private static IEnumerator Shot(string name)
        {
            string path = TileStories.Tests.TestEvidence.PathFor("Card", name + ".png");
            if (File.Exists(path)) File.Delete(path);
            ScreenCapture.CaptureScreenshot(path);
            for (int i = 0; i < 60 && !File.Exists(path); i++) yield return null;
            Assert.IsTrue(File.Exists(path), name + ": the Game view capture was written");
        }

        [UnityTest]
        public IEnumerator TheRealWindow_WhilePlayModeRuns_ATypedTextANewBlockAndAnotherVariantChangeTheOpenCard_AndUndoBringsEachBack()
        {
            LogAssert.ignoreFailingMessages = true;
            Assert.IsFalse(EditorApplication.isPlaying, "precondition: Edit Mode");
            // - the test runner's own scene is the open one: enter Play Mode from it, then load the wall scene the way a visitor's app starts it
            yield return new EnterPlayMode(expectDomainReload: false);
            try
            {
                yield return EditorSceneManager.LoadSceneAsyncInPlayMode(WallScenePath, new LoadSceneParameters(LoadSceneMode.Single));
                // - a scene saved while the POI Editor's rig was populated holds its stand-in markers; the visitor's app never does
                var editorRig = GameObject.Find("POIEditorRig");
                if (editorRig != null) UnityEngine.Object.Destroy(editorRig);
                // - the wall spawns its markers, a POI is selected the way a marker tap does, and the card is dragged to full
                WallSession session = null;
                PoiCardHost card = null;
                for (int i = 0; i < 900 && (session == null || session.SearchPois.Count == 0 || card == null || card.Sheet == null); i++)
                {
                    session = UnityEngine.Object.FindFirstObjectByType<WallSession>();
                    card = UnityEngine.Object.FindFirstObjectByType<PoiCardHost>();
                    yield return null;
                }
                Assert.IsNotNull(session, "the wall scene runs a WallSession");
                Assert.Greater(session.SearchPois.Count, 0, "the wall spawned its POIs");
                SelectionEventBus.Select("lamp");
                yield return Frames(20);
                var sheet = card.Sheet;
                sheet.SetStop(SheetStopRule.Stop.Full);
                yield return Frames(20);
                Assert.IsTrue(sheet.IsOpen, "the card is open on The Lamp");
                Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop);

                // - scroll the card down to a block that sits low in the stack (the offset must survive every live edit): the Today Map, whose
                //   heading and look the edits below change, so the Game view shows each edit
                var mapView = sheet.Stack.BoundViews.OfType<TodayMapBlockView>().First();
                sheet.Stack.Scroll.ScrollTo(sheet.Stack.SlotOf(mapView));
                yield return Frames(20);
                float scrolled = sheet.Stack.Scroll.scrollOffset.y;
                Assert.Greater(scrolled, 300f, "precondition: the card is scrolled well down");
                int blocksBefore = sheet.Stack.BoundViews.Count;
                string closeBefore = sheet.CloseButton.tooltip;
                string headingBefore = sheet.Stack.HeadingOf(mapView).text;
                yield return Shot("LiveEdit_1_before");

                // - the window the developer edits in: the shipped config (what the scene runs), Detail Card > Card Texts open
                var config = ShippedConfig();
                _window = new PoiEditorWindowHost(config, "_showCardTexts");
                OpenTab("DetailCard");
                yield return _window.WaitForRepaint();

                // 1. real typing in Card Texts: the close button's wording changes on the open card
                yield return _window.ReplaceText("Card text close en#0", "Dismiss");
                yield return _window.ClickAway();
                yield return Frames(10);
                Assert.AreEqual("Dismiss", CardStringTableChecks.WallWording(_window.Config.card_settings, CardStrings.Keys.Close, "en"), "the window's own config holds the typed wording");
                Assert.AreEqual("Dismiss", CardStringTableChecks.WallWording(session.CardSettings, CardStrings.Keys.Close, "en"), "the running wall holds it (the push reached WallSession)");
                Assert.AreEqual("Dismiss", sheet.CloseButton.tooltip, "the typed wording reached the open card (" + closeBefore + " -> Dismiss)");
                Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop, "the card kept its stop");
                Assert.AreEqual(scrolled, sheet.Stack.Scroll.scrollOffset.y, 2f, "and its scroll");
                Assert.AreEqual("lamp", card.ShownPoiId, "and its point");

                // 2. real typing in Specific Marker > Card Content: a block's Heading changes on the open card
                var lamp = _window.Config.pois.First(p => p.id == "lamp");
                int mapIndex = lamp.card.blocks.FindIndex(b => b.kind == "today_map");
                var foldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_poiFoldouts", Instance).GetValue(_window.Editor);
                foldouts["lamp"] = true;
                var blockFoldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor);
                blockFoldouts["lamp/" + lamp.card.blocks[mapIndex].key] = true;
                _window.SetWindowField("_showPoiCardContent", true);
                OpenTab("SpecificMarker");
                yield return _window.WaitForRepaint();
                yield return ScrollWindowTo("Block field heading en#" + mapIndex);
                yield return _window.ReplaceText("Block field heading en#" + mapIndex, "Live heading");
                yield return _window.ClickAway();
                yield return Frames(10);
                Assert.AreEqual("Live heading", sheet.Stack.HeadingOf(sheet.Stack.BoundViews.OfType<TodayMapBlockView>().First()).text,
                    "the typed heading reached the open card (" + headingBefore + " -> Live heading)");
                yield return Shot("LiveEdit_2_typed_heading");
                Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop, "the stop is kept");
                Assert.AreEqual(scrolled, sheet.Stack.Scroll.scrollOffset.y, 2f, "the scroll is kept");
                Assert.AreEqual(blocksBefore, sheet.Stack.BoundViews.Count, "no block appeared or went");

                // 3. a real click on + Add block: a Show On Wall block joins the open card
                int showOnWallBefore = sheet.Stack.BoundViews.OfType<ShowOnWallBlockView>().Count();
                int kindIndex = BlockRegistry.Shared.Ordered.ToList().FindIndex(k => k.Key == "show_on_wall");
                typeof(POIEditorToolWindow).GetField("_newCardBlockKindIndex", Instance).SetValue(_window.Editor, kindIndex);
                yield return ScrollWindowTo("Card add block#0");
                _window.Click("Card add block#0");
                yield return _window.WaitForRepaint();
                yield return Frames(10);
                Assert.AreEqual(blocksBefore + 1, sheet.Stack.BoundViews.Count, "the new block is on the open card");
                Assert.AreEqual(showOnWallBefore + 1, sheet.Stack.BoundViews.OfType<ShowOnWallBlockView>().Count(), "and it is the Show On Wall the developer added");
                Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop);
                Assert.AreEqual(scrolled, sheet.Stack.Scroll.scrollOffset.y, 2f, "the scroll is kept");

                // 4. another variant of a block, written through the window's own edit path (the Variant popup is a native menu no test can click)
                var info = _window.Config.pois.First(p => p.id == "lamp").card.blocks.First(b => b.kind == "today_map");
                var infoView = sheet.Stack.BoundViews.OfType<TodayMapBlockView>().First();
                BlockRegistry.Shared.TryGet("today_map", out var infoDefinition);
                string current = BlockLibraryRule.DefaultVariant(_window.Config.card_settings, infoDefinition);
                string other = infoDefinition.Variants.First(v => v != current);
                Assert.IsTrue(infoView.Root.ClassListContains("card-today--" + current), "precondition: the card draws the Block Library default look");
                typeof(POIEditorToolWindow).GetMethod("DrawConfigMutationScope", Instance).Invoke(_window.Editor, new object[] { (Action)(() => info.variant = other), false });
                yield return Frames(10);
                var infoAfter = sheet.Stack.BoundViews.OfType<TodayMapBlockView>().First();
                Assert.IsTrue(infoAfter.Root.ClassListContains("card-today--" + other), "the open card draws the block in its new look (" + current + " -> " + other + ")");
                Assert.IsFalse(infoAfter.Root.ClassListContains("card-today--" + current), "and no longer in the old one");
                yield return Shot("LiveEdit_3_other_variant");

                // Undo, one edit at a time: each edit comes back off the open card
                yield return _window.PressUndo();
                yield return Frames(10);
                Assert.IsTrue(sheet.Stack.BoundViews.OfType<TodayMapBlockView>().First().Root.ClassListContains("card-today--" + current), "Ctrl+Z: the old look is back");
                yield return _window.PressUndo();
                yield return Frames(10);
                Assert.AreEqual(blocksBefore, sheet.Stack.BoundViews.Count, "Ctrl+Z: the added block is gone from the open card");
                yield return _window.PressUndo();
                yield return Frames(10);
                Assert.AreEqual(headingBefore, sheet.Stack.HeadingOf(sheet.Stack.BoundViews.OfType<TodayMapBlockView>().First()).text, "Ctrl+Z: the old heading is back");
                yield return _window.PressUndo();
                yield return Frames(10);
                Assert.AreEqual(closeBefore, sheet.CloseButton.tooltip, "Ctrl+Z: the framework's wording is back on the close button");
                Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop, "through all of it the card kept its stop");
                Assert.AreEqual(scrolled, sheet.Stack.Scroll.scrollOffset.y, 2f, "...and its scroll");
                yield return Shot("LiveEdit_4_after_undo");
            }
            finally
            {
                _window?.Close();
                _window = null;
            }
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator TheOpenGalleryButton_InPlayMode_LoadsTheCardGalleryScene_ARealClick()
        {
            LogAssert.ignoreFailingMessages = true;
            yield return new EnterPlayMode(expectDomainReload: false);
            try
            {
                yield return EditorSceneManager.LoadSceneAsyncInPlayMode(WallScenePath, new LoadSceneParameters(LoadSceneMode.Single));
                for (int i = 0; i < 600 && UnityEngine.Object.FindFirstObjectByType<WallSession>() == null; i++) yield return null;
                Assert.IsNull(UnityEngine.Object.FindFirstObjectByType<CardGalleryHarness>(), "precondition: the wall scene is running, not the gallery");

                _window = new PoiEditorWindowHost(ShippedConfig(), "_showCardContainer");
                OpenTab("DetailCard");
                yield return _window.WaitForRepaint();
                yield return ScrollWindowTo("Card gallery open#0");
                _window.Click("Card gallery open#0");
                CardGalleryHarness harness = null;
                for (int i = 0; i < 600 && harness == null; i++)
                {
                    harness = UnityEngine.Object.FindFirstObjectByType<CardGalleryHarness>();
                    yield return null;
                }
                Assert.IsNotNull(harness, "a real click on Open Gallery in Play Mode loaded the gallery scene (its CardGalleryHarness is running)");
                Assert.IsTrue(SceneManager.GetSceneByPath(CardGalleryOpener.ScenePath).isLoaded, "the gallery scene is the loaded one");
                Assert.IsFalse(_window.Unsaved, "opening the gallery never changes the config");
            }
            finally
            {
                _window?.Close();
                _window = null;
            }
            yield return new ExitPlayMode();
        }
    }
}
