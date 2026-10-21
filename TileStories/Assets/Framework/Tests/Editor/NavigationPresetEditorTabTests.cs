using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Editor.Tests
{
    // Detail Card > Navigation's step 17a on the REAL window (PoiEditorWindowHost): the Preset row and its picker clicked with real mouse
    // events, every preset's pick written as exactly its seven rows (a walk over every field of the settings AND a whole-config
    // comparison) and put back by ONE real Ctrl+Z; each warning drawn on its condition and absent otherwise (read from what the window
    // really draws, not from the rule); Test > Preview Navigation; the texts.
    public class NavigationPresetEditorTabTests
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;
        private const BindingFlags Static = BindingFlags.NonPublic | BindingFlags.Static;
        private PoiEditorWindowHost _window;
        private readonly List<string> _messages = new();

        [TearDown]
        public void TearDown()
        {
            POIEditorToolWindow.NavigationMessageProbe = null;
            NamedOptionPickerPopup.RowRectProbe = null;
            HelpInfoButton.DrawnProbe = null;
            EditorPopup.CloseAll();
            _window?.Close();
        }

        // - the shipped wall with the framework navigation (Long read): the presets are measured from the framework default
        private static WallConfigData ShippedConfig() => ShippedWall.WithFrameworkNavigation();

        private CardNavigationSettings Nav => _window.Config.card_settings.navigation;

        private void OpenNavigation(WallConfigData config)
        {
            _window = new PoiEditorWindowHost(config, "_showCardNavigation");
            var tabField = typeof(POIEditorToolWindow).GetField("_selectedTab", Instance);
            _window.SetWindowField("_selectedTab", Enum.Parse(tabField.FieldType, "DetailCard"));
        }

        // An edit as the window records it (its own mutation scope: undo step, unsaved flag, live push)
        private void Pick(Action edit) =>
            typeof(POIEditorToolWindow).GetMethod("DrawConfigMutationScope", Instance).Invoke(_window.Editor, new object[] { edit, false });

        // Repaint the window and collect every warning / note it drew, once each
        private IEnumerator Redraw()
        {
            _messages.Clear();
            POIEditorToolWindow.NavigationMessageProbe = _messages.Add;
            try { yield return _window.WaitForRepaint(); }
            finally { POIEditorToolWindow.NavigationMessageProbe = null; }
        }

        private List<string> Drawn() => _messages.Distinct().ToList();

        private static string Text(string field) => (string)typeof(POIEditorToolWindow).GetField(field, Static).GetValue(null);

        // The popup the Preset button opened, after its rows have been drawn once (their rects, in the popup's own space)
        private IEnumerator OpenThePicker(List<Rect> rows)
        {
            rows.Clear();
            var seen = new Dictionary<int, Rect>();
            NamedOptionPickerPopup.RowRectProbe = (i, r) => seen[i] = r;
            yield return _window.ScrollTo("Preset");
            _window.Click("Preset");
            var popup = EditorPopup.FindOpen(NavigationPresets.PickerKind);
            Assert.IsNotNull(popup, "a real click on the Preset button opens the picker");
            for (int frame = 0; frame < 90 && seen.Count < NavigationPresets.All.Count; frame++)
            {
                popup.Repaint();
                yield return null;
            }
            Assert.AreEqual(NavigationPresets.All.Count, seen.Count, "the picker drew one row per preset");
            for (int i = 0; i < seen.Count; i++) rows.Add(seen[i]);
        }

        private static void ClickRow(EditorPopup popup, Rect row) =>
            popup.SendEvent(new Event { type = EventType.MouseDown, button = 0, clickCount = 1, mousePosition = row.center });

        // ---------------- the Preset row ----------------

        [UnityTest]
        public IEnumerator ThePresetRow_IsTheSectionsFirstRow_DrawingItWritesNothing_AndEscClosesThePickerWithNoChange()
        {
            OpenNavigation(ShippedConfig());
            var titles = new List<(string Title, Rect Rect)>();
            HelpInfoButton.DrawnProbe = (title, rect) => titles.Add((title, rect));
            yield return _window.WaitForRepaint();
            HelpInfoButton.DrawnProbe = null;
            Assert.IsFalse(_window.Unsaved, "drawing the section, the Preset row included, writes nothing");
            float Y(string title) => titles.Last(t => t.Title == title).Rect.y;
            // - section 7's order since the 17a review: a preset is where a wall author starts, so it is read first, above Layout
            var sectionRows = new[] { "Layout", "Block Order", "Show Navigation From", "Top Bar", "Bottom Bar", "Menu", "Floating Button", "Edge",
                "Auto Nav Block", "Follow Scroll", "Hide While Scrolling", "Back Button", "Placed Model Control" };
            foreach (string row in sectionRows)
                Assert.Less(Y("Preset"), Y(row), "the Preset row comes before " + row + " (the section's first row)");
            Assert.AreEqual("Preset", titles.Where(t => t.Title == "Preset" || sectionRows.Contains(t.Title)).OrderBy(t => t.Rect.y).First().Title,
                "no row of the section is drawn above it");
            Assert.Less(Y("Placed Model Control"), Y("Preview Navigation"), "the Test foldout's own rows still close the section");
            Assert.AreSame(NavigationPresets.All[0], NavigationPresets.Matching(Nav), "the framework default's seven rows are Long read");

            int steps = _window.UndoStepsLeft;
            string before = JsonUtility.ToJson(_window.Config);
            var rows = new List<Rect>();
            yield return OpenThePicker(rows);
            var popup = EditorPopup.FindOpen(NavigationPresets.PickerKind);
            popup.SendEvent(new Event { type = EventType.KeyDown, keyCode = KeyCode.Escape });
            yield return null;
            Assert.IsNull(EditorPopup.FindOpen(NavigationPresets.PickerKind), "Esc closes the picker");
            Assert.AreEqual(before, JsonUtility.ToJson(_window.Config), "and writes nothing");
            Assert.AreEqual(steps, _window.UndoStepsLeft);
            Assert.IsFalse(_window.Unsaved);
        }

        [UnityTest]
        public IEnumerator EveryPreset_ARealClickOnItsRow_WritesItsSevenRowsAndNothingElse_AndOneCtrlZPutsAllSevenBack()
        {
            OpenNavigation(ShippedConfig());
            yield return _window.WaitForRepaint();
            // - every OTHER row changed first, so a preset that wrote one too many rows would show; the seven start away from every preset
            Pick(() =>
            {
                var nav = _window.Config.card_settings.navigation;
                nav.block_order = CardOptions.BlockOrderByFamily;
                nav.group_by = CardOptions.GroupByFormat;
                nav.min_blocks = 12;
                nav.max_slots = 3;
                nav.follow_scroll = false;
                nav.show_counts = true;
                nav.labels = CardOptions.LabelsIcon;
                nav.hide_on_scroll = CardOptions.HideOnScrollNever;
                nav.show_at = CardOptions.ShowNavFullOnly;
                nav.jump_raises_to = CardOptions.JumpRaisesKeep;
                nav.jump_motion = CardOptions.JumpMotionInstant;
                nav.back_button = CardOptions.BackButtonNone;
                nav.placed_model_control = CardOptions.PlacedModelNone;
                nav.accordion_one_open = true;
                nav.families.Add(new CardFamilySetting { family = BuiltInFamilies.Media, order = 3, at_end = true, group = "watch" });
                nav.groups.Add(new CardNavGroup { key = "group_1" });
                nav.layout = CardOptions.LayoutTabs;
                nav.top = BuiltInNavElements.IconTabs;
                nav.bottom = BuiltInNavElements.Dock;
                nav.floating = BuiltInNavElements.FabRadial;
                nav.menu = BuiltInNavElements.GridOverlay;
                nav.edge = BuiltInNavElements.EdgeScrubber;
                nav.auto_nav_block = BuiltInBlocks.NavIconGridKind;
            });
            Assert.IsNull(NavigationPresets.Matching(Nav), "precondition: the rows hold no preset (Custom)");
            int steps = _window.UndoStepsLeft;

            var rows = new List<Rect>();
            for (int i = 0; i < NavigationPresets.All.Count; i++)
            {
                var preset = NavigationPresets.All[i];
                string beforeJson = JsonUtility.ToJson(_window.Config);
                var before = NavigationPresetsTests.Snapshot(Nav);

                yield return OpenThePicker(rows);
                var popup = EditorPopup.FindOpen(NavigationPresets.PickerKind);
                if (i == 0)
                {
                    // - the four strongest above a separator: the gap before the fifth row is the separator, no row is spaced like it elsewhere
                    float separator = rows[NavigationPresets.Recommended].y - rows[NavigationPresets.Recommended - 1].yMax;
                    Assert.Greater(separator, 4f, "a separator sits between the fourth and the fifth row (gap " + separator + ")");
                    for (int r = 1; r < rows.Count; r++)
                        if (r != NavigationPresets.Recommended)
                            Assert.Less(rows[r].y - rows[r - 1].yMax, 1f, "rows " + (r - 1) + " and " + r + " touch: only one separator");
                }
                ClickRow(popup, rows[i]);
                yield return _window.WaitForRepaint();

                // - the picker closed on the pick and the rows hold the preset
                Assert.IsNull(EditorPopup.FindOpen(NavigationPresets.PickerKind), preset.Label + ": a pick closes the picker");
                Assert.AreSame(preset, NavigationPresets.Matching(Nav), preset.Label + ": the seven rows now hold it");
                Assert.IsTrue(_window.Unsaved, preset.Label + ": a pick is an edit");
                // - exactly its fields: the walk over every field of the settings, then the WHOLE config against the same config with the preset applied
                var after = NavigationPresetsTests.Snapshot(Nav);
                foreach (string field in before.Keys)
                    if (!NavigationPresets.WrittenFields.Contains(field))
                        Assert.AreEqual(before[field], after[field], preset.Label + ": '" + field + "' is not a preset field and must not move");
                var expected = JsonUtility.FromJson<WallConfigData>(beforeJson);
                NavigationPresets.Apply(expected.card_settings.navigation, preset);
                Assert.AreEqual(JsonUtility.ToJson(expected), JsonUtility.ToJson(_window.Config), preset.Label + ": nothing in the whole config but its seven rows changed");

                // - ONE history step for all seven rows: one real Ctrl+Z puts every one of them back
                Assert.AreEqual(steps + 1, _window.UndoStepsLeft, preset.Label + ": one preset is ONE undo step, not seven");
                yield return _window.PressUndo();
                Assert.AreEqual(steps, _window.UndoStepsLeft, preset.Label + ": Ctrl+Z took exactly that step");
                Assert.AreEqual(beforeJson, JsonUtility.ToJson(_window.Config), preset.Label + ": all seven rows (and everything else) are back");
                Assert.IsNull(NavigationPresets.Matching(Nav), preset.Label + ": back to Custom");
            }
            yield return _window.PressRedo();
            Assert.AreSame(NavigationPresets.All[NavigationPresets.All.Count - 1], NavigationPresets.Matching(Nav), "Ctrl+Y brings the last preset back whole");
        }

        // ---------------- the warnings ----------------

        // The warnings, drawn: each appears on its condition and ONLY then. Read from what the window draws, state by state, on The Lamp's
        // 8-section card, so a warning that is always there (noise) or never drawn fails here
        [UnityTest]
        public IEnumerator EachWarning_IsDrawnOnItsConditionAndAbsentOtherwise_OnTheShippedWall()
        {
            OpenNavigation(ShippedConfig());
            yield return Redraw();
            CollectionAssert.IsEmpty(Drawn(), "the shipped wall's choices (Text Tabs, scroll) raise no warning");
            Assert.IsFalse(_window.Unsaved);
            string lamp = EditorNames.Poi(_window.Config.pois.FindIndex(p => p.id == "lamp"), _window.Config.pois.Single(p => p.id == "lamp"));

            // - the Progress Map: past its limit of sections, only as the Top Bar
            Pick(() => Nav.top = BuiltInNavElements.ProgressMap);
            yield return Redraw();
            Assert.AreEqual(1, Drawn().Count, "Progress Map on an 8-section card: one warning");
            StringAssert.Contains("Progress Map", Drawn()[0]);
            StringAssert.Contains(lamp, Drawn()[0], "it names the busiest card by its list title");
            StringAssert.Contains("8 sections", Drawn()[0]);
            StringAssert.Contains("past " + NavigationWarningRule.ProgressMapSectionsThatFit, Drawn()[0]);
            Pick(() => Nav.top = BuiltInNavElements.Segmented);
            yield return Redraw();
            CollectionAssert.IsEmpty(Drawn(), "another Top Bar: no Progress Map warning");
            Pick(() => Nav.top = BuiltInNavElements.TextTabs);

            // - more than two bars: Top Bar + Bottom Bar + Edge
            Pick(() =>
            {
                Nav.bottom = BuiltInNavElements.BottomBar;
                Nav.edge = BuiltInNavElements.EdgeScrubber;
            });
            yield return Redraw();
            CollectionAssert.AreEqual(new[] { POIEditorToolWindow.CardMoreThanTwoBarsWarning }, Drawn(), "three bars: exactly the bars warning (a bar at 5 slots raises no other)");
            Pick(() => Nav.edge = CardOptions.NavNone);
            yield return Redraw();
            CollectionAssert.IsEmpty(Drawn(), "two bars never warn");
            Pick(() =>
            {
                Nav.top = CardOptions.NavNone;
                Nav.edge = BuiltInNavElements.EdgeScrubber;
            });
            yield return Redraw();
            CollectionAssert.IsEmpty(Drawn(), "bottom bar + edge, no top bar: two");
            Pick(() =>
            {
                Nav.top = BuiltInNavElements.TextTabs;
                Nav.edge = CardOptions.NavNone;
            });

            // - Bar Slots below the sections of the busiest card, while the row could still be raised
            Pick(() => Nav.max_slots = 3);
            yield return Redraw();
            Assert.AreEqual(1, Drawn().Count, "3 slots for 8 sections");
            StringAssert.Contains("Bar Slots is 3", Drawn()[0]);
            StringAssert.Contains("8 sections", Drawn()[0]);
            StringAssert.Contains(lamp, Drawn()[0]);
            Pick(() => Nav.max_slots = 4);
            yield return Redraw();
            StringAssert.Contains("Bar Slots is 4", Drawn().Single(), "4 slots for 8: still short, and the row can go to 5");
            Pick(() => Nav.max_slots = CardNavigationSettings.MaxSlotsMax);
            yield return Redraw();
            CollectionAssert.IsEmpty(Drawn(), "at the row's maximum nothing is left to raise: no warning");
            Pick(() =>
            {
                Nav.max_slots = 3;
                Nav.bottom = CardOptions.NavNone;
            });
            yield return Redraw();
            CollectionAssert.IsEmpty(Drawn(), "no bar reads Bar Slots: the row is hidden, so is its warning, and the 3 stays stored");
            Assert.AreEqual(3, Nav.max_slots);
            Pick(() => Nav.floating = BuiltInNavElements.FabRadial);
            yield return Redraw();
            StringAssert.Contains("Bar Slots is 3", Drawn().Single(), "the Radial caps at Bar Slots like a bar");
            Pick(() => Nav.floating = BuiltInNavElements.FabDial);
            yield return Redraw();
            CollectionAssert.IsEmpty(Drawn(), "the Dial reads no slots");
            Pick(() =>
            {
                Nav.floating = CardOptions.NavNone;
                Nav.max_slots = 5;
            });

            // - Tabs without a bar (built at step 13) and Bento (8 sections fill the card)
            Pick(() =>
            {
                Nav.layout = CardOptions.LayoutTabs;
                Nav.top = CardOptions.NavNone;
            });
            yield return Redraw();
            CollectionAssert.AreEqual(new[] { Text("CardTabsWithoutABarWarning") }, Drawn(), "Tabs with Top Bar and Bottom Bar None");
            Pick(() => Nav.top = BuiltInNavElements.TextTabs);
            yield return Redraw();
            CollectionAssert.IsEmpty(Drawn(), "Tabs with a Top Bar: none");
            Pick(() => Nav.layout = CardOptions.LayoutBento);
            yield return Redraw();
            CollectionAssert.IsEmpty(Drawn(), "Bento on 8 sections: they fill the card, no warning");
        }

        // The two warnings about a card with MORE families than the framework ships: only a wall that registers many app families gets them
        [UnityTest]
        public IEnumerator TheBentoAndProgressMapAndBarSlotsWarnings_AreDrawnOnAWallWithManySections_AndNotBelowTheirLimit()
        {
            OpenNavigation(ManySectionsFixture.Wall(14));
            _window.Editor.NavigationRegistry = ManySectionsFixture.Registry(14);
            string many = EditorNames.Poi(_window.Config.pois.FindIndex(p => p.id == ManySectionsFixture.PoiId),
                _window.Config.pois.Single(p => p.id == ManySectionsFixture.PoiId));
            yield return Redraw();
            CollectionAssert.IsEmpty(Drawn(), "the default choices warn about nothing, however many sections");

            Pick(() => Nav.layout = CardOptions.LayoutBento);
            yield return Redraw();
            Assert.AreEqual(1, Drawn().Count, "Bento on 14 sections");
            StringAssert.Contains("Bento", Drawn()[0]);
            StringAssert.Contains(many, Drawn()[0], "it names the busiest card by its list title");
            StringAssert.Contains(" 14 sections: ", Drawn()[0], "and its section count");
            StringAssert.Contains("about " + NavigationWarningRule.BentoSectionsThatFill, Drawn()[0]);
            Pick(() => Nav.layout = CardOptions.LayoutScroll);
            yield return Redraw();
            CollectionAssert.IsEmpty(Drawn(), "another Layout: no bento warning");

            // - the same wall with exactly the limit: the tiles still fill the card
            OpenWallWithSections(NavigationWarningRule.BentoSectionsThatFill);
            Pick(() => Nav.layout = CardOptions.LayoutBento);
            yield return Redraw();
            CollectionAssert.IsEmpty(Drawn(), "13 sections: the limit itself still fits");

            OpenWallWithSections(14);
            Pick(() => Nav.top = BuiltInNavElements.ProgressMap);
            yield return Redraw();
            Assert.AreEqual(1, Drawn().Count, "Progress Map on 14 sections");
            StringAssert.Contains("14 sections", Drawn()[0]);
            StringAssert.Contains("Progress Map", Drawn()[0]);
            Pick(() =>
            {
                Nav.top = BuiltInNavElements.TextTabs;
                Nav.bottom = BuiltInNavElements.BottomBar;
                Nav.max_slots = 4;
            });
            yield return Redraw();
            StringAssert.Contains("14 sections", Drawn().Single(), "Bar Slots 4 on 14 sections");
            StringAssert.Contains("Bar Slots is 4", Drawn().Single());
        }

        // A fresh window on a wall whose busiest card has `sections` sections (the shipped wall's Lamp has 8)
        private void OpenWallWithSections(int sections)
        {
            _window?.Close();
            OpenNavigation(ManySectionsFixture.Wall(sections));
            _window.Editor.NavigationRegistry = ManySectionsFixture.Registry(sections);
        }

        // ---------------- Test > Preview Navigation ----------------

        [UnityTest]
        public IEnumerator PreviewNavigation_ARealClickTurnsTheDemoCardOnAtFull_OnTheChosenPoint_AndOneCtrlZPutsItBack()
        {
            var config = ShippedConfig();
            Assert.IsFalse(config.card_settings.demo_card.enabled, "precondition: the demo card is off");
            Assert.AreEqual(CardOptions.StopHalf, config.card_settings.demo_card.stop, "precondition: its stop is the default, Half");
            OpenNavigation(config);
            var titles = new List<string>();
            HelpInfoButton.DrawnProbe = (title, rect) => titles.Add(title);
            yield return _window.WaitForRepaint();
            CollectionAssert.DoesNotContain(titles, "Preview Point", "off: no point row yet");
            Assert.IsFalse(_window.Unsaved);
            Assert.IsEmpty(DevFeatureBuildGuard.ActiveMessages(_window.Config, developmentBuild: true), "nothing for the build guard to report");

            yield return _window.ScrollTo("Preview Navigation");
            int steps = _window.UndoStepsLeft;
            _window.Click("Preview Navigation");
            yield return Redraw();
            var demo = _window.Config.card_settings.demo_card;
            Assert.IsTrue(demo.enabled, "a real click turned the demo card on");
            Assert.AreEqual(CardOptions.StopFull, demo.stop, "at Full, where every navigation element can show");
            Assert.AreEqual("", demo.poi_id, "no point picked yet: the click writes none");
            Assert.AreEqual(1, DevFeatureBuildGuard.ActiveMessages(_window.Config, developmentBuild: true).Count, "the build guard already watches this very switch");
            Assert.AreEqual(steps + 1, _window.UndoStepsLeft, "one click, one undo step");
            CollectionAssert.AreEqual(new[] { Text("CardDemoNoPointNote") }, Drawn(), "no point yet: the note says so");

            string blockless = _window.Config.pois.First(p => p.card.blocks.Count == 0).id;
            Pick(() => _window.Editor.SetCardDemoPoi(blockless));
            yield return Redraw();
            Assert.AreEqual(1, Drawn().Count, "a point with no long card");
            StringAssert.Contains("shows no navigation", Drawn()[0]);
            StringAssert.Contains(EditorNames.Poi(_window.Config.pois.FindIndex(p => p.id == blockless), _window.Config.pois.First(p => p.id == blockless)), Drawn()[0],
                "named by its list title");
            Pick(() => _window.Editor.SetCardDemoPoi("lamp"));
            yield return Redraw();
            CollectionAssert.IsEmpty(Drawn(), "a point whose card shows navigation: nothing to say");
            CollectionAssert.Contains(titles, "Preview Point");

            yield return _window.PressUndo();
            yield return _window.PressUndo();
            Assert.AreEqual("", _window.Config.card_settings.demo_card.poi_id);
            yield return _window.PressUndo();
            Assert.IsFalse(_window.Config.card_settings.demo_card.enabled, "one Ctrl+Z turns the preview off");
            Assert.AreEqual(CardOptions.StopHalf, _window.Config.card_settings.demo_card.stop, "and gives the stop back with it");
            Assert.AreEqual(steps, _window.UndoStepsLeft);
        }

        // ---------------- the texts ----------------

        private static readonly string[] ForbiddenTerms =
        {
            ".md", ".cs", "_3.", "_5.1", "LivingRoom", "lamp", "Lamp", "painting", "PoiCardHost", "BlockStackBuilder",
            "card_settings", "CardOptions", "Assets/", "CardGallery", "poi_", "block_",
        };

        [Test]
        public void TheNewTexts_AreAsciiAppAgnostic_NameEditorControls_AndTheGuidesWalkEveryPresetAndWarning()
        {
            var texts = new Dictionary<string, string>
            {
                ["CardPresetHelp"] = Text("CardPresetHelp"),
                ["CardPreviewNavigationHelp"] = Text("CardPreviewNavigationHelp"),
                ["CardPreviewPointHelp"] = Text("CardPreviewPointHelp"),
                ["CardMoreThanTwoBarsWarning"] = POIEditorToolWindow.CardMoreThanTwoBarsWarning,
                ["CardProgressMapWarning"] = POIEditorToolWindow.CardProgressMapWarning("2. A Card Name", 8),
                ["CardBentoWarning"] = POIEditorToolWindow.CardBentoWarning("2. A Card Name", 14),
                ["CardBarSlotsWarning"] = POIEditorToolWindow.CardBarSlotsWarning(3, "2. A Card Name", 8),
                ["CardPreviewNoNavigationNote"] = POIEditorToolWindow.CardPreviewNoNavigationNote("2. A Card Name", 8),
                ["CardNavigationPlaymodeTestGuide"] = Text("CardNavigationPlaymodeTestGuide"),
                ["CardNavigationDeviceTestGuide"] = Text("CardNavigationDeviceTestGuide"),
            };
            foreach (var preset in NavigationPresets.All) texts["preset " + preset.Label] = preset.Label + " " + preset.Suits;
            foreach (var pair in texts)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(pair.Value), pair.Key);
                EditorTextChecks.AssertAscii(pair.Value, pair.Key);
                foreach (string term in ForbiddenTerms)
                    StringAssert.DoesNotContain(term, pair.Value, pair.Key + " must not contain '" + term + "'");
            }
            // - what the Preset (i) promises: which rows, nothing else, one Ctrl+Z, Custom, the shortcut
            foreach (string word in new[] { "Layout", "Top Bar", "Bottom Bar", "Floating Button", "Menu", "Edge", "Auto Nav Block", "nothing else", "Custom", "Ctrl+Z", "Long read" })
                StringAssert.Contains(word, texts["CardPresetHelp"], "the Preset (i) says " + word);
            // - the warnings name the rows to change, and the card by name
            StringAssert.Contains("Top Bar", texts["CardProgressMapWarning"]);
            StringAssert.Contains("Layout", texts["CardBentoWarning"]);
            StringAssert.Contains("Bar Slots", texts["CardBarSlotsWarning"]);
            foreach (string name in new[] { "CardProgressMapWarning", "CardBentoWarning", "CardBarSlotsWarning", "CardPreviewNoNavigationNote" })
                StringAssert.Contains("2. A Card Name", texts[name], name + " names the card by its list title");
            // - the developer-only switch says so, how to turn it off (20-code-quality.md)
            StringAssert.Contains("Developer-only", texts["CardPreviewNavigationHelp"]);
            StringAssert.Contains("Off by default", texts["CardPreviewNavigationHelp"]);
            StringAssert.Contains("release build ignores it", texts["CardPreviewNavigationHelp"]);
            StringAssert.Contains("Save All to JSON", texts["CardPreviewNavigationHelp"]);
            StringAssert.Contains("Show demo card", texts["CardPreviewNavigationHelp"], "it says it is the same switch");

            string guide = texts["CardNavigationPlaymodeTestGuide"];
            foreach (string block in new[] { "PRESETS\n", "WARNINGS\n", "PREVIEW NAVIGATION\n" })
                StringAssert.Contains(block, guide, "the Playmode guide has the block " + block.Trim());
            foreach (var preset in NavigationPresets.All.Take(NavigationPresets.Recommended))
                StringAssert.Contains(preset.Label, guide, "the guide names " + preset.Label);
            foreach (string control in new[] { "Preset", "Preview Navigation", "Preview Point", "Progress Map", "Bento", "Bar Slots", "Edge", "Layout = Tabs" })
                StringAssert.Contains(control, guide, "the guide names " + control);
            StringAssert.Contains("Presets:", texts["CardNavigationDeviceTestGuide"]);
        }
    }
}
