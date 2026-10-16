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
    // The POI Editor's third tab, "Detail Card", and Specific Marker > "Card Content" (_3.1 section 8), on the REAL
    // window (PoiEditorWindowHost): real clicks, real typing, real Ctrl+Z / Ctrl+Y through the window's own config
    // history. Every card_settings field is walked by reflection; the texts and the field drawers are guarded so a
    // new kind or a new field cannot slip in without help or without an Editor row.
    public class DetailCardEditorTabTests
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;
        private const BindingFlags Static = BindingFlags.NonPublic | BindingFlags.Static;
        private PoiEditorWindowHost _window;

        [TearDown]
        public void TearDown() => _window?.Close();

        private static WallConfigData ShippedConfig() =>
            JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));

        private static WallConfigData TwoPoiConfig()
        {
            var config = new WallConfigData { wall_id = "t" };
            config.pois.Add(new POIData { id = "poi_1", name = "North Tower", summary = "Built in 1640." });
            config.pois.Add(new POIData { id = "poi_2", name = "South Gate" });
            return config;
        }

        private void OpenTab(string tab)
        {
            var tabField = typeof(POIEditorToolWindow).GetField("_selectedTab", Instance);
            _window.SetWindowField("_selectedTab", Enum.Parse(tabField.FieldType, tab));
        }

        private void OpenPoiCardContent(string poiId)
        {
            OpenTab("SpecificMarker");
            _window.SetWindowField("_showPoiCardContent", true);
            var foldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_poiFoldouts", Instance).GetValue(_window.Editor);
            foldouts[poiId] = true;
        }

        // ---------------- the tab ----------------

        [UnityTest]
        public IEnumerator TheDetailCardTab_DrawsCardContainerAndBlockLibrary_OnTheShippedWall()
        {
            _window = new PoiEditorWindowHost(ShippedConfig(), "_showCardContainer");
            _window.SetWindowField("_showCardBlockLibrary", true);
            OpenTab("DetailCard");
            yield return _window.WaitForRepaint();
            _window.RectOf("Card languages#0");
            _window.RectOf("Tap Outside Closes");
            _window.RectOf("Half Height Max");
            _window.RectOf("Block Library enabled#0");
            Assert.IsFalse(_window.Unsaved, "drawing the tab changes nothing");
        }

        [UnityTest]
        public IEnumerator RealClicks_TapOutsideCloses_AndHalfHeightMax_EditUndoAndRedo()
        {
            _window = new PoiEditorWindowHost(ShippedConfig(), "_showCardContainer");
            OpenTab("DetailCard");
            yield return _window.WaitForRepaint();

            _window.Click("Tap Outside Closes");
            yield return _window.WaitForRepaint();
            Assert.IsFalse(_window.Config.card_settings.container.dismiss_on_tap_outside, "a real click unticks it");
            Assert.IsTrue(_window.Unsaved);
            yield return _window.PressUndo();
            Assert.IsTrue(_window.Config.card_settings.container.dismiss_on_tap_outside, "Ctrl+Z");
            yield return _window.PressRedo();
            Assert.IsFalse(_window.Config.card_settings.container.dismiss_on_tap_outside, "Ctrl+Y");

            // a real drag along the slider track, from near its right end far to the left
            Rect row = _window.RectOf("Half Height Max");
            float y = row.center.y, startX = row.xMax - 70f;
            int steps = _window.UndoStepsLeft;
            _window.Send(new Event { type = EventType.MouseDown, button = 0, clickCount = 1, mousePosition = _window.Local(new Vector2(startX, y)) });
            yield return _window.WaitForRepaint();
            for (int i = 1; i <= 8; i++)
            {
                _window.Send(new Event { type = EventType.MouseDrag, button = 0, mousePosition = _window.Local(new Vector2(startX - 40f * i, y)), delta = new Vector2(-40f, 0f) });
                yield return _window.WaitForRepaint();
            }
            _window.Send(new Event { type = EventType.MouseUp, button = 0, clickCount = 1, mousePosition = _window.Local(new Vector2(startX - 320f, y)) });
            yield return _window.WaitForRepaint();
            float half = _window.Config.card_settings.container.half_max_ratio;
            Assert.Less(half, CardContainerSettings.HalfMaxRatioMax - 0.01f, "the drag moved the slider down");
            Assert.GreaterOrEqual(half, CardContainerSettings.HalfMaxRatioMin, "never below the allowed minimum");
            Assert.AreEqual(steps + 1, _window.UndoStepsLeft, "the whole drag is one history step");
            yield return _window.PressUndo();
            Assert.AreEqual(0.40f, _window.Config.card_settings.container.half_max_ratio, 1e-5f, "one Ctrl+Z puts it back");
        }

        [Test]
        public void EveryCardSettingsField_ThroughTheWindowHistory_UndoesRedoesAndRoundTrips()
        {
            var t = typeof(POIEditorToolWindow);
            var mutate = t.GetMethod("DrawConfigMutationScope", Instance);
            var undo = t.GetMethod("UndoConfigChange", Instance);
            var redo = t.GetMethod("RedoConfigChange", Instance);
            var edits = new List<(string Name, Action<CardSettings> Change)>();
            foreach (var f in typeof(CardSettings).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (f.FieldType == typeof(bool)) edits.Add((f.Name, s => f.SetValue(s, !(bool)f.GetValue(s))));
                else if (f.FieldType == typeof(string)) edits.Add((f.Name, s => f.SetValue(s, "Wall/CardMedia")));
                else if (f.FieldType == typeof(List<string>)) edits.Add((f.Name, s => f.SetValue(s, new List<string> { "pt", "en", "es" })));
                else if (f.FieldType == typeof(List<BlockKindSetting>)) edits.Add((f.Name, s => s.kinds.Add(new BlockKindSetting { kind = "header", default_variant = "compact" })));
                else if (f.FieldType == typeof(List<CardStringEntry>)) edits.Add((f.Name, s => POIEditorToolWindow.SetCardTextOverride(s, CardStrings.Keys.Close, "pt", "Sair")));
                else if (f.FieldType == typeof(List<GlossaryEntry>)) edits.Add((f.Name, s => s.glossary.Add(new GlossaryEntry { term = "keep" })));
                else if (f.FieldType == typeof(CardDemoSettings))
                    foreach (var d in typeof(CardDemoSettings).GetFields(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (d.FieldType == typeof(bool)) edits.Add((d.Name, s => d.SetValue(s.demo_card, !(bool)d.GetValue(s.demo_card))));
                        else if (d.FieldType == typeof(string)) edits.Add((d.Name, s => d.SetValue(s.demo_card, d.Name == "stop" ? CardOptions.StopFull : "lamp")));
                        else Assert.Fail("no edit for demo card field " + d.Name);
                    }
                else if (f.FieldType == typeof(CardNavigationSettings))
                    foreach (var n in typeof(CardNavigationSettings).GetFields(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (n.FieldType == typeof(string))
                            edits.Add((n.Name, s => n.SetValue(s.navigation, n.Name switch
                            {
                                "group_by" => CardOptions.GroupByFormat,
                                "jump_raises_to" => CardOptions.JumpRaisesKeep,
                                "jump_motion" => CardOptions.JumpMotionInstant,
                                "back_button" => CardOptions.BackButtonNone,
                                "top" => CardOptions.NavNone,
                                "bottom" => BuiltInNavElements.BottomBar,
                                "show_at" => CardOptions.ShowNavFullOnly,
                                "hide_on_scroll" => CardOptions.HideOnScrollNever,
                                "labels" => CardOptions.LabelsIcon,
                                _ => CardOptions.BlockOrderByFamily,
                            })));
                        else if (n.FieldType == typeof(bool)) edits.Add((n.Name, s => n.SetValue(s.navigation, !(bool)n.GetValue(s.navigation))));
                        else if (n.FieldType == typeof(int)) edits.Add((n.Name, s => n.SetValue(s.navigation, (int)n.GetValue(s.navigation) - 1)));
                        else if (n.FieldType == typeof(List<CardFamilySetting>))
                            edits.Add((n.Name, s => s.navigation.families.Add(new CardFamilySetting { family = BuiltInFamilies.Meta, order = 15, at_end = false, group = "group_5" })));
                        else if (n.FieldType == typeof(List<CardNavGroup>))
                            edits.Add((n.Name, s => s.navigation.groups.Add(new CardNavGroup
                            {
                                key = "group_5", label = new List<LocalizedEntry> { new() { lang = "en", value = "Read" }, new() { lang = "pt", value = "Ler" } },
                            })));
                        else Assert.Fail("no edit for navigation field " + n.Name);
                    }
                else if (f.FieldType == typeof(CardContainerSettings))
                    foreach (var c in typeof(CardContainerSettings).GetFields(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (c.FieldType == typeof(bool)) edits.Add((c.Name, s => c.SetValue(s.container, !(bool)c.GetValue(s.container))));
                        else if (c.FieldType == typeof(float)) edits.Add((c.Name, s => c.SetValue(s.container, 0.3f)));
                        else if (c.FieldType == typeof(string)) edits.Add((c.Name, s => c.SetValue(s.container, c.Name == "open_stop" ? CardOptions.StopHalf : CardOptions.AudioQueue)));
                        else Assert.Fail("no edit for container field " + c.Name);
                    }
                else Assert.Fail("no edit for card_settings field " + f.Name);
            }
            Assert.AreEqual(8 + 8 + 16 + 3, edits.Count,
                "every card_settings field (walked by reflection) has an edit: 8 wall-level + 8 container + 16 navigation + 3 demo card");

            foreach (var (name, change) in edits)
            {
                var window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
                try
                {
                    var config = ShippedConfig();
                    t.GetField("_config", Instance).SetValue(window, config);
                    t.GetMethod("InitializeConfigHistory", Instance).Invoke(window, null);
                    string before = JsonUtility.ToJson(config.card_settings);

                    mutate.Invoke(window, new object[] { (Action)(() => change(((WallConfigData)t.GetField("_config", Instance).GetValue(window)).card_settings)), false });
                    string after = JsonUtility.ToJson(((WallConfigData)t.GetField("_config", Instance).GetValue(window)).card_settings);
                    Assert.AreNotEqual(before, after, name + ": the edit changed the config");
                    Assert.IsTrue((bool)t.GetField("_hasUnsavedChanges", Instance).GetValue(window), name + ": Save lights up");

                    undo.Invoke(window, null);
                    Assert.AreEqual(before, JsonUtility.ToJson(((WallConfigData)t.GetField("_config", Instance).GetValue(window)).card_settings), name + ": undo");
                    redo.Invoke(window, null);
                    var redone = ((WallConfigData)t.GetField("_config", Instance).GetValue(window)).card_settings;
                    Assert.AreEqual(after, JsonUtility.ToJson(redone), name + ": redo");
                    Assert.AreEqual(after, JsonUtility.ToJson(JsonUtility.FromJson<WallConfigData>(JsonUtility.ToJson(new WallConfigData { card_settings = redone })).card_settings),
                        name + ": survives Save / load");
                }
                finally { UnityEngine.Object.DestroyImmediate(window); }
            }
        }

        // ---------------- Card Texts ----------------

        [UnityTest]
        public IEnumerator CardTexts_ARowPerFrameworkText_RealTypingMakesTheWallsWording_AndCtrlZTakesItBack()
        {
            _window = new PoiEditorWindowHost(ShippedConfig(), "_showCardTexts");
            OpenTab("DetailCard");
            yield return _window.WaitForRepaint();
            foreach (string key in CardStrings.Keys.All)
                foreach (string lang in new[] { "en", "pt" })
                    _window.RectOf("Card text " + key + " " + lang + "#0");
            Assert.IsFalse(_window.Unsaved, "drawing the rows writes nothing");
            CollectionAssert.IsEmpty(_window.Config.card_settings.strings, "precondition: the shipped wall keeps the framework's wording");

            yield return _window.ReplaceText("Card text close pt#0", "Sair");
            yield return _window.ClickAway();
            var s = _window.Config.card_settings;
            Assert.AreEqual("Sair", POIEditorToolWindow.CardTextOverride(s, CardStrings.Keys.Close, "pt"), "real typing wrote the wall's wording");
            Assert.AreEqual("", POIEditorToolWindow.CardTextOverride(s, CardStrings.Keys.Close, "en"), "the other language stays the framework's");
            var framework = POIEditorToolWindow.FrameworkCardStrings().Entries();
            Assert.AreEqual("Sair", new CardStrings(framework, null, s.strings, "pt", "en").Get(CardStrings.Keys.Close), "the card reads it");
            Assert.AreEqual("Close", new CardStrings(framework, null, s.strings, "en", "en").Get(CardStrings.Keys.Close));

            yield return _window.PressUndo();
            CollectionAssert.IsEmpty(_window.Config.card_settings.strings, "one Ctrl+Z: back to the framework's wording, no empty row left");
        }

        // ---------------- Block Library ----------------

        [UnityTest]
        public IEnumerator TheBlockLibrary_NeverWritesByDrawing_TheHeaderIsLocked_AndAnEditMakesItsRow()
        {
            var config = ShippedConfig();
            _window = new PoiEditorWindowHost(config, "_showCardBlockLibrary");
            OpenTab("DetailCard");
            yield return _window.WaitForRepaint();
            CollectionAssert.IsEmpty(_window.Config.card_settings.kinds, "drawing the table creates no row");

            _window.Click("Block Library enabled#0");
            yield return _window.WaitForRepaint();
            CollectionAssert.IsEmpty(_window.Config.card_settings.kinds, "the Header's Enabled is locked: a real click changes nothing");
            Assert.IsFalse(_window.Unsaved);

            // the Default Variant popup's write path, inside the window's own mutation scope
            typeof(POIEditorToolWindow).GetMethod("DrawConfigMutationScope", Instance).Invoke(_window.Editor, new object[]
                { (Action)(() => _window.Editor.SetBlockLibraryVariant(BuiltInBlocks.HeaderKind, BuiltInBlocks.HeaderCompact)), false });
            Assert.AreEqual(1, _window.Config.card_settings.kinds.Count, "the first edit makes the row");
            var painting = _window.Config.pois.Single(p => p.id == "painting");
            Assert.AreEqual(BuiltInBlocks.HeaderCompact, BlockStackBuilder.Build(painting, _window.Config.card_settings, BlockRegistry.Shared).Entries[0].Variant,
                "a POI without a card now gets the compact header");
            yield return _window.PressUndo();
            CollectionAssert.IsEmpty(_window.Config.card_settings.kinds, "Ctrl+Z removes the row again");
        }

        // ---------------- Card Content ----------------

        [UnityTest]
        public IEnumerator CardContent_RealClicksAndTyping_AddWriteReorderDelete_EachUndoable()
        {
            _window = new PoiEditorWindowHost(TwoPoiConfig(), "_showCardContainer");
            OpenPoiCardContent("poi_1");
            yield return _window.WaitForRepaint();
            var poi = _window.Config.pois[0];
            CollectionAssert.IsEmpty(poi.card.blocks, "precondition: no blocks");

            _window.Click("Card add block#0");
            yield return _window.WaitForRepaint();
            poi = _window.Config.pois[0];
            Assert.AreEqual(1, poi.card.blocks.Count, "a real click on + Add block");
            Assert.AreEqual("block_1", poi.card.blocks[0].key);
            Assert.AreEqual(BuiltInBlocks.HeaderKind, poi.card.blocks[0].kind);
            Assert.IsTrue(BlockStackBuilder.Build(poi, _window.Config.card_settings, BlockRegistry.Shared).Entries[0].Synthesized,
                "an empty header is not shown yet: the card still uses the name");

            yield return _window.ReplaceText("Block field title en#0", "Keep");
            yield return _window.ClickAway();
            poi = _window.Config.pois[0];
            Assert.AreEqual("Keep", POIEditorToolWindow.LocalizedValue(poi.card.blocks[0], "title", "en"));
            Assert.AreEqual("", POIEditorToolWindow.LocalizedValue(poi.card.blocks[0], "title", "pt"), "the other language stays unwritten");
            var entry = BlockStackBuilder.Build(poi, _window.Config.card_settings, BlockRegistry.Shared).Entries[0];
            Assert.IsFalse(entry.Synthesized, "the typed title makes the authored header count");
            Assert.AreEqual("Keep", new BlockFieldReader(entry.Instance, "pt", "en").Text("title"), "a visitor in pt reads the fallback");

            yield return _window.PressUndo();
            Assert.AreEqual("", POIEditorToolWindow.LocalizedValue(_window.Config.pois[0].card.blocks[0], "title", "en"), "one Ctrl+Z takes the typed word back");
            yield return _window.PressUndo();
            CollectionAssert.IsEmpty(_window.Config.pois[0].card.blocks, "a second Ctrl+Z removes the block");
            yield return _window.PressRedo();
            yield return _window.PressRedo();
            Assert.AreEqual("Keep", POIEditorToolWindow.LocalizedValue(_window.Config.pois[0].card.blocks[0], "title", "en"), "Ctrl+Y twice");

            // a second header: kept in Card Content, flagged as not shown -- first for its empty title, then as a second header
            _window.Click("Card add block#0");
            yield return _window.WaitForRepaint();
            poi = _window.Config.pois[0];
            Assert.AreEqual("block_2", poi.card.blocks[1].key, "a fresh key");
            var skipped = BlockStackBuilder.Build(poi, _window.Config.card_settings, BlockRegistry.Shared).Skipped.Single();
            Assert.AreEqual("block_2", skipped.Instance.key);
            Assert.AreEqual(BlockStackBuilder.SkipReason.MissingRequired, skipped.Reason, "an empty required field is reported first");
            Assert.AreEqual("Not shown: Title is empty in every language.",
                POIEditorToolWindow.CardBlockSkipText(skipped.Reason, BuiltInBlocks.Header.Field(skipped.FieldKey)), "the row names the field by its label");
            // - two open headers reach below the window's visible edge: scroll to the second one's title first, as a person would
            yield return _window.ScrollTo("Block field title en#1");
            yield return _window.ReplaceText("Block field title en#1", "Gate");
            yield return _window.ClickAway();
            skipped = BlockStackBuilder.Build(_window.Config.pois[0], _window.Config.card_settings, BlockRegistry.Shared).Skipped.Single();
            Assert.AreEqual(BlockStackBuilder.SkipReason.ExtraHeader, skipped.Reason);
            StringAssert.Contains("one Header", POIEditorToolWindow.CardBlockSkipText(skipped.Reason, null), "the row says why");

            yield return _window.ScrollTo("Card block down#0");
            _window.Click("Card block down#0");
            yield return _window.WaitForRepaint();
            CollectionAssert.AreEqual(new[] { "block_2", "block_1" }, _window.Config.pois[0].card.blocks.Select(b => b.key), "a real click moves it down");

            yield return _window.ScrollTo("Card block delete#0");
            _window.Click("Card block delete#0");
            yield return _window.WaitForRepaint();
            CollectionAssert.AreEqual(new[] { "block_1" }, _window.Config.pois[0].card.blocks.Select(b => b.key), "a real click deletes it (no question: Ctrl+Z restores it)");
            yield return _window.PressUndo();
            CollectionAssert.AreEqual(new[] { "block_2", "block_1" }, _window.Config.pois[0].card.blocks.Select(b => b.key));
            CollectionAssert.IsEmpty(_window.Config.pois[1].card.blocks, "the other POI is untouched");
        }

        [UnityTest]
        public IEnumerator EveryLanguage_GetsItsOwnRow()
        {
            var config = TwoPoiConfig();
            config.card_settings.languages = new List<string> { "en", "pt", "es" };
            config.pois[0].card.blocks.Add(new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.HeaderKind });
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenPoiCardContent("poi_1");
            var foldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor);
            foldouts["poi_1/block_1"] = true;
            yield return _window.WaitForRepaint();
            foreach (string lang in new[] { "en", "pt", "es" })
            {
                _window.RectOf("Block field title " + lang + "#0");
                _window.RectOf("Block field subtitle " + lang + "#0");
            }
            Assert.IsFalse(_window.Unsaved, "drawing the rows writes nothing");
            yield return _window.ReplaceText("Block field subtitle es#0", "Puerta");
            yield return _window.ClickAway();
            Assert.AreEqual("Puerta", POIEditorToolWindow.LocalizedValue(_window.Config.pois[0].card.blocks[0], "subtitle", "es"));
        }

        // ---------------- Items rows (a repeater field) ----------------

        [UnityTest]
        public IEnumerator ItemsRows_RealClicksAddTypeReorderDelete_EachUndoable_AndDrawingNeverWrites()
        {
            var config = TwoPoiConfig();
            config.pois[0].card.blocks.Add(new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.RichTextKind, variant = BuiltInBlocks.RichTextSections });
            // - one language keeps every row inside the host window (rows per language: EveryLanguage_GetsItsOwnRow)
            config.card_settings.languages = new List<string> { "en" };
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenPoiCardContent("poi_1");
            var foldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor);
            foldouts["poi_1/block_1"] = true;
            yield return _window.WaitForRepaint();
            Assert.IsFalse(_window.Unsaved, "drawing an empty Sections field writes nothing");
            Assert.IsEmpty(_window.Config.pois[0].card.blocks[0].fields, "no field value created by drawing");

            _window.Click("Block item sections add#0");
            yield return _window.WaitForRepaint();
            _window.Click("Block item sections add#0");
            yield return _window.WaitForRepaint();
            Assert.AreEqual(2, Sections().Count, "two real clicks on + Add row");
            yield return _window.WaitForRepaint();
            Assert.Less(_window.RectOf("Block item sections 0 delete#0").xMin - _window.RectOf("Block item sections 0 down#0").xMax, 40f,
                "a row's delete sits beside its own buttons, not at the far edge of the window");
            Assert.Less(_window.RectOf("Card block delete#0").xMin - _window.RectOf("Card block down#0").xMax, 450f,
                "a block's delete sits right after its row's cells");

            yield return _window.ReplaceText("Block item sections 0 title en#0", "Before");
            yield return _window.ClickAway();
            yield return _window.ReplaceText("Block item sections 1 title en#0", "After");
            yield return _window.ClickAway();
            yield return _window.ReplaceText("Block item sections 1 body en#0", "After 1755 it was rebuilt.");
            yield return _window.ClickAway();
            Assert.AreEqual("Before", POIEditorToolWindow.ItemLocalizedValue(Sections()[0], "title", "en"));
            Assert.AreEqual("After 1755 it was rebuilt.", POIEditorToolWindow.ItemLocalizedValue(Sections()[1], "body", "en"), "real typing in a row's text area");

            _window.Click("Block item sections 1 up#0");
            yield return _window.WaitForRepaint();
            CollectionAssert.AreEqual(new[] { "After", "Before" }, Titles(), "a real click moves the row up");

            _window.Click("Block item sections 0 delete#0");
            yield return _window.WaitForRepaint();
            CollectionAssert.AreEqual(new[] { "Before" }, Titles(), "a real click deletes it");
            yield return _window.PressUndo();
            CollectionAssert.AreEqual(new[] { "After", "Before" }, Titles(), "Ctrl+Z brings the row back");

            var reader = new BlockFieldReader(_window.Config.pois[0].card.blocks[0], "en", "en");
            Assert.AreEqual("After 1755 it was rebuilt.", reader.ItemText(reader.Items(BuiltInBlocks.RichTextSectionsField)[0], "body"), "the card reads what was typed");
        }

        private List<BlockItemData> Sections() =>
            _window.Config.pois[0].card.blocks[0].fields.Single(f => f.key == BuiltInBlocks.RichTextSectionsField).items;

        private IEnumerable<string> Titles() => Sections().Select(s => POIEditorToolWindow.ItemLocalizedValue(s, "title", "en"));

        // ---------------- Choice fields ----------------

        [UnityTest]
        public IEnumerator AChoiceField_IsAPopupOfTheOptionLabels_DrawingNeverWrites_AStaleValueIsKept_AndAPickIsUndoable()
        {
            var config = TwoPoiConfig();
            config.card_settings.languages = new List<string> { "en" };
            var block = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.SourcesKind, variant = BuiltInBlocks.SourcesWithConfidence };
            POIEditorToolWindow.SetChoiceValue(block, BuiltInBlocks.SourcesStatusField, "checked");
            config.pois[0].card.blocks.Add(block);
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenPoiCardContent("poi_1");
            var foldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor);
            foldouts["poi_1/block_1"] = true;
            yield return _window.WaitForRepaint();
            _window.RectOf("Block field content_status#0");
            Assert.IsFalse(_window.Unsaved, "drawing the popup writes nothing");
            Assert.AreEqual("checked", POIEditorToolWindow.ChoiceValue(_window.Config.pois[0].card.blocks[0], BuiltInBlocks.SourcesStatusField),
                "a value that is no longer an option is kept, never rewritten by drawing");

            var field = BuiltInBlocks.Sources.Field(BuiltInBlocks.SourcesStatusField);
            var (values, labels) = POIEditorToolWindow.BlockChoiceOptions(field, "checked");
            CollectionAssert.AreEqual(new[] { "", "verified", "draft", "checked" }, values, "(none), the options, then the stale value");
            CollectionAssert.AreEqual(new[] { "(none)", "Verified", "Draft", "checked (missing)" }, labels, "the Editor shows labels, never the stored words");
            CollectionAssert.AreEqual(new[] { "(none)", "Verified", "Draft" }, POIEditorToolWindow.BlockChoiceOptions(field, "draft").Labels, "a valid value adds nothing");

            // - the popup's write path, inside the window's own mutation scope (a real pick goes through the same setter)
            typeof(POIEditorToolWindow).GetMethod("DrawConfigMutationScope", Instance).Invoke(_window.Editor, new object[]
                { (Action)(() => POIEditorToolWindow.SetChoiceValue(_window.Config.pois[0].card.blocks[0], BuiltInBlocks.SourcesStatusField, "verified")), false });
            Assert.AreEqual("verified", new BlockFieldReader(_window.Config.pois[0].card.blocks[0], "en", "en").Value(BuiltInBlocks.SourcesStatusField));
            Assert.IsTrue(_window.Unsaved);
            yield return _window.PressUndo();
            Assert.AreEqual("checked", POIEditorToolWindow.ChoiceValue(_window.Config.pois[0].card.blocks[0], BuiltInBlocks.SourcesStatusField), "Ctrl+Z");
        }

        // ---------------- field visibility (_3.1 step 15.1) ----------------

        // Every probe this repaint drew, from scratch: a row that is no longer drawn must not be found in an older repaint's rects
        private IEnumerator Redraw()
        {
            _window.ScreenRects.Clear();
            yield return _window.WaitForRepaint();
        }

        private bool Drawn(string probeKey) => _window.ScreenRects.ContainsKey(probeKey);

        // A pick in a popup, through the window's own mutation scope (the native popup menu cannot be clicked; a real pick runs the same setter)
        private void Pick(Action edit) =>
            typeof(POIEditorToolWindow).GetMethod("DrawConfigMutationScope", Instance).Invoke(_window.Editor, new object[] { edit, false });

        [UnityTest]
        public IEnumerator FieldVisibility_SwitchingScaleModeAndAKnowledgeCheckLook_ShowsAndHidesRows_AHiddenValueSurvivesASwitchBack_CtrlZ_AndASave()
        {
            string dir = Path.GetFullPath("Temp/__FieldVisibilityTests");
            Directory.CreateDirectory(dir);
            var config = TwoPoiConfig();
            config.card_settings.languages = new List<string> { "en" };
            // - a Place In AR block with a Height authored earlier (80 cm) and no Scale Mode: Real Size, the field's default
            var ar = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.PlaceInArKind, display = CardOptions.DisplayInline };
            ar.fields.Add(new BlockFieldValue { key = BuiltInBlocks.PlaceInArHeightField, number = 80f });
            // - a Multiple Choice question whose first option has words and whose (unused) Statement Is True is ticked
            var question = new BlockItemData();
            question.fields.Add(new BlockItemFieldValue { key = "option_1", text = new List<LocalizedEntry> { new() { lang = "en", value = "Stone" } } });
            question.fields.Add(new BlockItemFieldValue { key = BuiltInBlocks.KnowledgeCheckIsTrueField, flag = true });
            var quiz = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.KnowledgeCheckKind, variant = BuiltInBlocks.KnowledgeCheckMultipleChoice, display = CardOptions.DisplayInline };
            quiz.fields.Add(new BlockFieldValue { key = BuiltInBlocks.KnowledgeCheckQuestionsField, items = { question } });
            config.pois[0].card.blocks.Add(ar);
            config.pois[0].card.blocks.Add(quiz);
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            _window.SetWindowField("_configPath", Path.Combine(dir, "config.json"));
            OpenPoiCardContent("poi_1");
            var foldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor);
            foldouts["poi_1/block_1"] = true;
            foldouts["poi_1/block_2"] = true;
            BlockInstanceData Ar() => _window.Config.pois[0].card.blocks[0];
            BlockInstanceData Quiz() => _window.Config.pois[0].card.blocks[1];
            float HeightOf(BlockInstanceData b) => POIEditorToolWindow.NumberValue(b, BuiltInBlocks.PlaceInAr.Field(BuiltInBlocks.PlaceInArHeightField));

            try
            {
                yield return Redraw();
                Assert.IsTrue(Drawn("Block field scale#0"), "precondition: the Place In AR row is open");
                Assert.IsFalse(Drawn("Block field height_cm#0"), "Real Size: no Height row");
                Assert.IsFalse(Drawn("Block field marker_multiple#0"), "Real Size: no Marker Multiple row");
                Assert.IsTrue(Drawn("Block item questions 0 option_1 en#1"), "Multiple Choice: the options");
                Assert.IsFalse(Drawn("Block item questions 0 image_1#1"), "Multiple Choice: no pictures");
                Assert.IsFalse(Drawn("Block item questions 0 is_true#1"), "Multiple Choice: no Statement Is True");
                Assert.IsFalse(_window.Unsaved, "hiding rows writes nothing");

                // - Scale Mode Height: its row appears with the value authored before
                Pick(() => POIEditorToolWindow.SetChoiceValue(Ar(), BuiltInBlocks.PlaceInArScaleField, ArPlacementRule.ScaleHeightCm));
                yield return Redraw();
                Assert.IsTrue(Drawn("Block field height_cm#0"), "Height: the Height row appears");
                Assert.IsFalse(Drawn("Block field marker_multiple#0"));
                Assert.AreEqual(80f, HeightOf(Ar()), "showing 80 cm, the value it kept");
                // - Marker Multiple: the other row instead
                Pick(() => POIEditorToolWindow.SetChoiceValue(Ar(), BuiltInBlocks.PlaceInArScaleField, ArPlacementRule.ScaleMarkerMultiple));
                yield return Redraw();
                Assert.IsTrue(Drawn("Block field marker_multiple#0"));
                Assert.IsFalse(Drawn("Block field height_cm#0"), "Marker Multiple: Height is hidden again");
                Assert.AreEqual(80f, HeightOf(Ar()), "hidden, never cleared");
                // - Ctrl+Z goes back to Height: the row and its value are there again
                yield return _window.PressUndo();
                yield return Redraw();
                Assert.AreEqual(ArPlacementRule.ScaleHeightCm, POIEditorToolWindow.ChoiceValue(Ar(), BuiltInBlocks.PlaceInArScaleField), "Ctrl+Z");
                Assert.IsTrue(Drawn("Block field height_cm#0"), "after Ctrl+Z the Height row is back");
                Assert.AreEqual(80f, HeightOf(Ar()));

                // - the Knowledge Check's look: Image Choice adds the pictures, True / False swaps the options for Statement Is True
                Pick(() => Quiz().variant = BuiltInBlocks.KnowledgeCheckImageChoice);
                yield return Redraw();
                Assert.IsTrue(Drawn("Block item questions 0 image_1#1"), "Image Choice: Picture 1 appears");
                Assert.IsTrue(Drawn("Block item questions 0 image_4#1"), "and Picture 4");
                Assert.IsTrue(Drawn("Block item questions 0 option_1 en#1"), "the options stay, as captions");
                Pick(() => Quiz().variant = BuiltInBlocks.KnowledgeCheckTrueFalseSwipe);
                yield return Redraw();
                Assert.IsTrue(Drawn("Block item questions 0 is_true#1"), "True / False: Statement Is True appears");
                Assert.IsFalse(Drawn("Block item questions 0 option_1 en#1"), "the options are hidden");
                Assert.IsFalse(Drawn("Block item questions 0 image_1#1"));
                Assert.IsFalse(Drawn("Block item questions 0 correct#1"), "and Right Option");
                Assert.AreEqual("Stone", POIEditorToolWindow.ItemLocalizedValue(Quiz().fields[0].items[0], "option_1", "en"), "a hidden option keeps its words");
                Pick(() => Quiz().variant = BuiltInBlocks.KnowledgeCheckMultipleChoice);
                yield return Redraw();
                Assert.IsTrue(Drawn("Block item questions 0 option_1 en#1"), "back to Multiple Choice: the option row is drawn again");
                Assert.IsFalse(Drawn("Block item questions 0 is_true#1"));

                // - Save, then load the file back through the window: every hidden value made the trip
                typeof(POIEditorToolWindow).GetMethod("SaveAllToJson", Instance).Invoke(_window.Editor, null);
                Assert.IsFalse(_window.Unsaved, "saved");
                typeof(POIEditorToolWindow).GetMethod("LoadConfig", Instance).Invoke(_window.Editor, null);
                Assert.AreEqual(80f, HeightOf(Ar()), "Height, 80 cm, saved and loaded");
                Assert.AreEqual(ArPlacementRule.ScaleHeightCm, POIEditorToolWindow.ChoiceValue(Ar(), BuiltInBlocks.PlaceInArScaleField));
                Assert.AreEqual("Stone", POIEditorToolWindow.ItemLocalizedValue(Quiz().fields[0].items[0], "option_1", "en"));
                Assert.IsTrue(POIEditorToolWindow.ItemFlagValue(Quiz().fields[0].items[0], BuiltInBlocks.KnowledgeCheckIsTrueField),
                    "the Statement Is True no look of the block drew is still ticked");
                yield return Redraw();
                Assert.IsTrue(Drawn("Block field height_cm#0"), "the loaded block draws its Height row");
                Assert.IsFalse(_window.Unsaved, "and drawing the loaded file writes nothing");
            }
            finally { Directory.Delete(dir, true); }
        }

        // ---------------- collapsed-row summary (_3.1 step 15.1) ----------------

        private static BlockFieldValue Text(string key, params (string lang, string value)[] texts) =>
            new() { key = key, text = texts.Select(t => new LocalizedEntry { lang = t.lang, value = t.value }).ToList() };

        [Test]
        public void ABlockSummary_IsItsHeadingInTheFirstLanguage_ElseItsFirstWords_OnOneLine()
        {
            var languages = new List<string> { "en", "pt" };
            var headed = new BlockInstanceData { kind = BuiltInBlocks.RichTextKind };
            headed.fields.Add(Text(BlockKindDefinition.HeadingField, ("pt", "Antes da reconstrução"), ("en", "Before the earthquake")));
            headed.fields.Add(Text(BuiltInBlocks.RichTextBodyField, ("en", "The tiles were made in Lisbon.")));
            Assert.AreEqual("Before the earthquake", POIEditorToolWindow.CardBlockSummary(headed, BuiltInBlocks.RichText, languages), "the heading, in the first language");
            Assert.AreEqual("Antes da reconstrução", POIEditorToolWindow.CardBlockSummary(headed, BuiltInBlocks.RichText, new List<string> { "pt", "en" }),
                "the wall's FIRST language decides, with its real letters");

            var body = new BlockInstanceData { kind = BuiltInBlocks.RichTextKind };
            body.fields.Add(Text(BuiltInBlocks.RichTextBodyField, ("en", "The [[azulejo|azulejos]] were\nmade   in Lisbon.\n\nSecond paragraph.")));
            Assert.AreEqual("The azulejo were made in Lisbon. Second paragraph.", POIEditorToolWindow.CardBlockSummary(body, BuiltInBlocks.RichText, languages),
                "no heading: its first words, the glossary mark as the visitor reads it, on one line");

            var facts = new BlockInstanceData { kind = BuiltInBlocks.QuickFactsKind };
            var row = new BlockItemData();
            row.fields.Add(new BlockItemFieldValue { key = BuiltInBlocks.QuickFactsLabelField, text = new List<LocalizedEntry> { new() { lang = "en", value = "Built" } } });
            facts.fields.Add(new BlockFieldValue { key = BuiltInBlocks.QuickFactsItemsField, items = { row } });
            Assert.AreEqual("Built", POIEditorToolWindow.CardBlockSummary(facts, BuiltInBlocks.QuickFacts, languages), "words only in rows: the first row's");

            Assert.AreEqual("", POIEditorToolWindow.CardBlockSummary(new BlockInstanceData { kind = BuiltInBlocks.ShowOnWallKind }, BuiltInBlocks.ShowOnWall, languages),
                "a block with no words says nothing");
        }

        [Test]
        public void FitWithEllipsis_KeepsWhatFits_CutsTheRestWithThreeDots_AndNeverExceedsTheWidth()
        {
            var style = EditorStyles.miniLabel;
            const string text = "Before the earthquake of 1755 the castle had seven towers";
            float whole = style.CalcSize(new GUIContent(text)).x;
            Assert.AreEqual(text, POIEditorToolWindow.FitWithEllipsis(text, whole + 1f, style), "room enough: the whole text");
            foreach (float width in new[] { whole - 1f, whole * 0.5f, 40f, 20f })
            {
                string cut = POIEditorToolWindow.FitWithEllipsis(text, width, style);
                StringAssert.EndsWith("...", cut, width + " pt");
                Assert.LessOrEqual(style.CalcSize(new GUIContent(cut)).x, width, width + " pt: '" + cut + "' fits");
                StringAssert.StartsWith(cut.Substring(0, cut.Length - 3), text, "a start of the text");
            }
            // - the longest start that fits: one more character would not
            string best = POIEditorToolWindow.FitWithEllipsis(text, whole * 0.5f, style);
            // - (a trailing space is trimmed before the dots, so step on until the start really grows)
            int length = best.Length - 3;
            string longer;
            do longer = text.Substring(0, ++length).TrimEnd() + "..."; while (longer == best);
            Assert.Greater(style.CalcSize(new GUIContent(longer)).x, whole * 0.5f, "one character more would not fit");
            Assert.AreEqual("", POIEditorToolWindow.FitWithEllipsis(text, 2f, style), "not even the dots fit: nothing");
        }

        [UnityTest]
        public IEnumerator ACollapsedBlockRow_ShowsItsSummaryInsideTheRow_CutToFit_AtEveryWidth_AndAnOpenRowShowsNone()
        {
            var config = TwoPoiConfig();
            config.card_settings.languages = new List<string> { "en", "pt" };
            var shortOne = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.RichTextKind, display = CardOptions.DisplayInline };
            shortOne.fields.Add(Text(BlockKindDefinition.HeadingField, ("en", "Tiles")));
            var longOne = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.RichTextKind, display = CardOptions.DisplayInline };
            longOne.fields.Add(Text(BuiltInBlocks.RichTextBodyField,
                ("en", "Before the great earthquake of 1755 the castle on the hill had seven towers, a palace and a church inside its walls.")));
            config.pois[0].card.blocks.Add(shortOne);
            config.pois[0].card.blocks.Add(longOne);
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenPoiCardContent("poi_1");
            float rightMargin = (float)typeof(POIEditorToolWindow).GetField("AddButtonRowRightMargin", Static).GetRawConstantValue();
            float rowMax = (float)typeof(POIEditorToolWindow).GetField("CardBlockRowMaxWidth", Static).GetRawConstantValue();
            var style = EditorStyles.miniLabel;

            // - the widest the screen lets the host be (the OS clamps it), then the default and the narrow window
            foreach (float width in new[] { _window.Width, 880f, 620f })
            {
                yield return _window.Resize(width);
                yield return Redraw();
                Rect up = _window.RectOf("Card block up#1"), summary = _window.RectOf("Card block summary#1"), delete = _window.RectOf("Card block delete#1");
                float viewRight = _window.RootScreen.x + _window.Width - rightMargin;
                Assert.LessOrEqual(summary.xMax, viewRight + 0.5f, width + " pt: the row ends inside the window, clear of the scrollbar");
                Assert.LessOrEqual(summary.xMax - up.xMin, rowMax + 0.5f, width + " pt: never wider than the row's cap");
                Assert.GreaterOrEqual(summary.xMin, delete.xMax, width + " pt: the summary comes after the delete, which keeps its place beside the cells");
                Assert.Greater(summary.width, style.CalcSize(new GUIContent("...")).x, width + " pt: precondition: the summary column has room for something");
                Assert.AreEqual("Tiles", POIEditorToolWindow.FitWithEllipsis(POIEditorToolWindow.CardBlockSummary(shortOne, BuiltInBlocks.RichText, config.card_settings.languages),
                    _window.RectOf("Card block summary#0").width, style), width + " pt: a short heading whole");
                string drawn = POIEditorToolWindow.FitWithEllipsis(POIEditorToolWindow.CardBlockSummary(longOne, BuiltInBlocks.RichText, config.card_settings.languages), summary.width, style);
                StringAssert.EndsWith("...", drawn, width + " pt: the long text is cut");
                Assert.LessOrEqual(style.CalcSize(new GUIContent(drawn)).x, summary.width, width + " pt: and fits its column");
            }
            // - the delete keeps its place: the summary column is reserved for an open row too
            Rect closedDelete = _window.RectOf("Card block delete#0");
            var foldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor);
            foldouts["poi_1/block_1"] = true;
            yield return Redraw();
            Assert.IsFalse(Drawn("Card block summary#0"), "an open row shows its fields, not a summary");
            Assert.IsTrue(Drawn("Card block summary#1"), "the other row is still collapsed");
            Assert.AreEqual(closedDelete.xMin, _window.RectOf("Card block delete#0").xMin, 0.5f, "opening a row never moves its delete");
            Assert.IsFalse(_window.Unsaved, "a summary is drawing only");
        }

        // ---------------- Default Media (_3.1 step 15.1, audit 15.D) ----------------

        [UnityTest]
        public IEnumerator DefaultMedia_EveryRowsInfoButton_SitsInTheSectionsInfoColumn_At880And620()
        {
            _window = new PoiEditorWindowHost(ShippedConfig(), "_showCardDefaultMedia");
            OpenTab("DetailCard");
            int entries = CardMediaLibraryLookup.Framework.Entries.Count;
            Assert.Greater(entries, 0, "precondition: the Framework's default library is there");
            float rightMargin = (float)typeof(POIEditorToolWindow).GetField("AddButtonRowRightMargin", Static).GetRawConstantValue();

            foreach (float width in new[] { 880f, 620f })
            {
                yield return _window.Resize(width);
                yield return Redraw();
                // - the section's (i) column: where the "Framework defaults" title row puts its (i) (the shared row's rule)
                Rect column = _window.RectOf("Default media title help#0");
                Assert.LessOrEqual(column.xMax, _window.RootScreen.x + width - rightMargin + 0.5f, width + " pt: the column is inside the window");
                int rows = 0;
                for (int i = 0; Drawn("Default media help#" + i); i++, rows++)
                {
                    Rect help = _window.RectOf("Default media help#" + i);
                    // - one physical pixel at 125 % is 0.8 pt: the (i) is the SAME column, not a nearby one
                    Assert.AreEqual(column.xMin, help.xMin, 0.5f, width + " pt: row " + (i + 1) + "'s (i) is in the section's (i) column");
                    Assert.AreEqual(column.width, help.width, 0.5f, "the same button");
                }
                Assert.AreEqual(entries, rows, width + " pt: precondition: every default's row was drawn");
            }
            Assert.IsFalse(_window.Unsaved, "drawing the table writes nothing");
        }

        // ---------------- Color fields ----------------

        [UnityTest]
        public IEnumerator AColourField_RealTypingOfAHexCode_TheCardShowsIt_AnInvalidOneIsKeptWithAWarning_AndCtrlZ()
        {
            var config = TwoPoiConfig();
            config.card_settings.languages = new List<string> { "en" };
            var block = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.SwatchesKind };
            config.pois[0].card.blocks.Add(block);
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenPoiCardContent("poi_1");
            var foldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor);
            foldouts["poi_1/block_1"] = true;
            yield return _window.WaitForRepaint();
            _window.Click("Block item swatches add#0");
            yield return _window.WaitForRepaint();
            _window.RectOf("Block item swatches 0 colour#0");
            Assert.AreEqual(1, Swatches().Count, "a real click added the row");
            Assert.IsNull(Swatches()[0].fields.Find(f => f.key == BuiltInBlocks.SwatchesColourField), "drawing the colour row writes nothing");

            yield return _window.ReplaceText("Block item swatches 0 name en#0", "Cobalt blue");
            yield return _window.ClickAway();
            yield return _window.ReplaceText("Block item swatches 0 colour#0", "#1F3F8F");
            yield return _window.ClickAway();
            Assert.AreEqual("#1F3F8F", POIEditorToolWindow.ItemChoiceValue(Swatches()[0], BuiltInBlocks.SwatchesColourField), "real typing stores the hex text");
            var shown = BlockStackBuilder.Build(_window.Config.pois[0], _window.Config.card_settings, BlockRegistry.Shared);
            CollectionAssert.IsEmpty(shown.Skipped, "a named row with a real colour: the card shows the block");
            var reader = new BlockFieldReader(shown.Entries[1].Instance, "en", "en");
            Assert.IsTrue(reader.ItemColor(reader.Items(BuiltInBlocks.SwatchesItemsField)[0], BuiltInBlocks.SwatchesColourField, out var colour));
            Assert.AreEqual(0x3F / 255f, colour.g, 1e-4f, "the card reads the typed colour");

            yield return _window.ReplaceText("Block item swatches 0 colour#0", "#12");
            yield return _window.ClickAway();
            Assert.AreEqual("#12", POIEditorToolWindow.ItemChoiceValue(Swatches()[0], BuiltInBlocks.SwatchesColourField), "an invalid text is kept as typed, never rewritten");
            var skipped = BlockStackBuilder.Build(_window.Config.pois[0], _window.Config.card_settings, BlockRegistry.Shared).Skipped.Single();
            Assert.AreEqual(BlockStackBuilder.SkipReason.NoCompleteRow, skipped.Reason, "its only row is no longer complete");
            Assert.AreEqual("Not shown: no row of Swatches is complete: each needs Name and Colour (a colour written as #RRGGBB).",
                POIEditorToolWindow.CardBlockSkipText(skipped.Reason, BuiltInBlocks.Swatches.Field(skipped.FieldKey)), "the block's reason names the colour rule");
            Assert.AreEqual("Colour '#12' is not a colour: write it as #RRGGBB (for example #1F3F8F), or pick it. Until then the card leaves this row out.",
                POIEditorToolWindow.CardColorInvalidText("Colour", "#12"), "the warning under the field");

            yield return _window.PressUndo();
            Assert.AreEqual("#1F3F8F", POIEditorToolWindow.ItemChoiceValue(Swatches()[0], BuiltInBlocks.SwatchesColourField), "one Ctrl+Z brings the good colour back");
        }

        [UnityTest]
        public IEnumerator AToggleField_ARealClickTurnsItOn_TheCardReadsIt_DrawingNeverWrites_AndCtrlZ()
        {
            var config = TwoPoiConfig();
            config.card_settings.languages = new List<string> { "en" };
            config.pois[0].card.blocks.Add(new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.TimelineKind });
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenPoiCardContent("poi_1");
            var foldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor);
            foldouts["poi_1/block_1"] = true;
            yield return _window.WaitForRepaint();
            _window.RectOf("Block field highlight_now#0");
            Assert.IsEmpty(_window.Config.pois[0].card.blocks[0].fields, "drawing an unticked toggle writes nothing");
            Assert.IsFalse(_window.Unsaved);

            _window.Click("Block field highlight_now#0");
            yield return _window.WaitForRepaint();
            var block = _window.Config.pois[0].card.blocks[0];
            Assert.IsTrue(POIEditorToolWindow.FlagValue(block, BuiltInBlocks.TimelineHighlightNowField), "a real click ticks it");
            Assert.IsTrue(new BlockFieldReader(block, "en", "en").Flag(BuiltInBlocks.TimelineHighlightNowField), "the card reads it");
            yield return _window.PressUndo();
            Assert.IsFalse(POIEditorToolWindow.FlagValue(_window.Config.pois[0].card.blocks[0], BuiltInBlocks.TimelineHighlightNowField), "Ctrl+Z unticks it");
        }

        // ---------------- PoiRef fields ----------------

        [UnityTest]
        public IEnumerator APoiRefField_ListsThePointsByTheirListTitles_AStaleIdShowsAsMissing_APickIsUndoable_AndItselfWarns()
        {
            var config = TwoPoiConfig();
            config.pois[0].has_status = true;
            config.pois[1].has_status = true;
            config.card_settings.languages = new List<string> { "en" };
            var block = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.ComparePointsKind };
            POIEditorToolWindow.SetChoiceValue(block, BuiltInBlocks.ComparePointsOtherField, "poi_9");
            config.pois[0].card.blocks.Add(block);
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenPoiCardContent("poi_1");
            var foldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor);
            foldouts["poi_1/block_1"] = true;
            yield return _window.WaitForRepaint();
            _window.RectOf("Block field other#0");
            _window.RectOf("Block field axis#0");
            Assert.IsFalse(_window.Unsaved, "drawing the popups writes nothing");
            Assert.AreEqual("poi_9", POIEditorToolWindow.ChoiceValue(_window.Config.pois[0].card.blocks[0], BuiltInBlocks.ComparePointsOtherField),
                "a deleted point's id is kept, never rewritten by drawing");

            var stale = POIEditorToolWindow.PoiRefOptions(_window.Config.pois, "poi_9", allowNone: false);
            CollectionAssert.AreEqual(new[] { "1. North Tower", "2. South Gate", "(missing)" }, stale.Labels, "list titles, and the stale id never shown");
            Assert.AreEqual(2, stale.SelectedIndex, "the stale value stays selected");
            Assert.IsFalse(stale.Labels.Any(l => l.Contains("poi_")), "no POI id in the popup");
            var blank = POIEditorToolWindow.PoiRefOptions(_window.Config.pois, "", allowNone: false);
            CollectionAssert.AreEqual(new[] { "(none)", "1. North Tower", "2. South Gate" }, blank.Labels, "nothing picked yet");
            Assert.AreEqual("poi_2", blank.KeyAt(2), "a label stands for its POI's id");
            var skipped = BlockStackBuilder.Build(_window.Config.pois[0], _window.Config.card_settings, BlockRegistry.Shared, _window.Config.pois).Skipped.Single();
            Assert.AreEqual(BlockStackBuilder.SkipReason.NotForThisPoint, skipped.Reason, "a deleted point: not shown, with the kind's reason");

            // - the popup's write path, inside the window's own mutation scope (a real pick goes through the same setter)
            typeof(POIEditorToolWindow).GetMethod("DrawConfigMutationScope", Instance).Invoke(_window.Editor, new object[]
                { (Action)(() => POIEditorToolWindow.SetChoiceValue(_window.Config.pois[0].card.blocks[0], BuiltInBlocks.ComparePointsOtherField, blank.KeyAt(2))), false });
            var poi = _window.Config.pois[0];
            CollectionAssert.IsEmpty(BlockStackBuilder.Build(poi, _window.Config.card_settings, BlockRegistry.Shared, _window.Config.pois).Skipped, "South Gate picked: shown");
            CollectionAssert.IsEmpty(POIEditorToolWindow.CardBlockWarnings(poi.card.blocks[0], BuiltInBlocks.ComparePoints, _window.Config.card_settings, poi));

            POIEditorToolWindow.SetChoiceValue(poi.card.blocks[0], BuiltInBlocks.ComparePointsOtherField, "poi_1");
            Assert.AreEqual(POIEditorToolWindow.CardCompareWithItselfNote,
                POIEditorToolWindow.CardBlockWarnings(poi.card.blocks[0], BuiltInBlocks.ComparePoints, _window.Config.card_settings, poi).Single(), "itself: warned");
            POIEditorToolWindow.SetChoiceValue(poi.card.blocks[0], BuiltInBlocks.ComparePointsOtherField, "poi_2");

            yield return _window.PressUndo();
            Assert.AreEqual("poi_9", POIEditorToolWindow.ChoiceValue(_window.Config.pois[0].card.blocks[0], BuiltInBlocks.ComparePointsOtherField), "Ctrl+Z");
        }

        // ---------------- BlockRef fields (_3.2 step 4) ----------------

        // A text block of poi_1's card with `words` (its Card Content summary)
        private static BlockInstanceData TextBlock(string key, string words)
        {
            var block = new BlockInstanceData { key = key, kind = BuiltInBlocks.RichTextKind, variant = BuiltInBlocks.RichTextPlain, display = CardOptions.DisplayInline };
            POIEditorToolWindow.SetLocalizedValue(block, BuiltInBlocks.RichTextBodyField, "en", words);
            return block;
        }

        // An Actions row that opens `target`
        private static BlockItemData OpenBlockRow(string target)
        {
            var row = new BlockItemData();
            POIEditorToolWindow.SetItemChoiceValue(row, BuiltInBlocks.ActionsActionField, BuiltInBlocks.ActionOpenBlock);
            POIEditorToolWindow.SetItemChoiceValue(row, BuiltInBlocks.ActionsTargetField, target);
            return row;
        }

        private BlockItemData ActionsRowOfPoi1(int row) =>
            _window.Config.pois[0].card.blocks[1].fields.Single(f => f.key == BuiltInBlocks.ActionsItemsField).items[row];

        // Specific Marker > Card Content: an Open Block row's Opens Block is a popup of the OTHER blocks of the same card named the way
        // their Card Content rows read; a key no block has any more is kept, shown as "(a deleted block)", and warned under the row by
        // name, never by key; the popup's write path picks a block (the warning goes, the card draws the button), and Ctrl+Z brings the
        // deleted pick and its warning back. A Show On The Wall row draws no Opens Block at all.
        [UnityTest]
        public IEnumerator ABlockRefField_ListsTheOtherBlocksByName_AStaleKeyIsKeptAndWarned_APickIsUndoable_AndCtrlZ()
        {
            var config = TwoPoiConfig();
            config.card_settings.languages = new List<string> { "en" };
            var actions = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.ActionsKind, variant = BuiltInBlocks.ActionsPillRow, display = CardOptions.DisplayInline };
            var showOnWall = new BlockItemData();
            POIEditorToolWindow.SetItemChoiceValue(showOnWall, BuiltInBlocks.ActionsActionField, BuiltInBlocks.ActionShowOnWall);
            actions.fields.Add(new BlockFieldValue { key = BuiltInBlocks.ActionsItemsField, items = new List<BlockItemData> { OpenBlockRow("block_9"), showOnWall } });
            config.pois[0].card.blocks.AddRange(new[] { TextBlock("block_1", "The story of the keep."), actions, TextBlock("block_3", "Second text") });
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenPoiCardContent("poi_1");
            var foldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor);
            foldouts["poi_1/block_2"] = true;
            yield return Redraw();

            const string target = "Block item actions 0 target#1";
            _window.RectOf(target);
            Assert.IsTrue(Drawn("Block item actions 0 target warning#1"), "a deleted pick is warned under its row");
            Assert.IsFalse(Drawn("Block item actions 1 target#1"), "a Show On The Wall row has no Opens Block");
            Assert.IsFalse(_window.Unsaved, "drawing the popup writes nothing");
            Assert.AreEqual("block_9", POIEditorToolWindow.ItemChoiceValue(ActionsRowOfPoi1(0), BuiltInBlocks.ActionsTargetField), "the deleted key is kept, never rewritten by drawing");

            var poi = _window.Config.pois[0];
            var stale = POIEditorToolWindow.BlockRefOptions(poi, poi.card.blocks[1], "block_9", config.card_settings.languages);
            CollectionAssert.AreEqual(new[] { "(none)", "1. Rich Text: The story of the keep.", "3. Rich Text: Second text", "(a deleted block)" }, stale.Labels,
                "the other blocks in Card Content order, by row number, kind and first words; the block itself is not offered; the key never shown");
            Assert.AreEqual(3, stale.SelectedIndex, "the deleted pick stays selected");
            Assert.IsFalse(stale.Labels.Any(l => l.Contains("block_")), "no block key in the popup");
            string warning = POIEditorToolWindow.CardBlockRefProblemText("Opens Block", BlockRefRule.Problem.Missing, "");
            StringAssert.Contains("(a deleted block)", warning);
            StringAssert.DoesNotContain("block_", warning);

            // - the popup's write path, inside the window's own mutation scope (a native popup menu cannot be clicked; a real pick runs this setter)
            Pick(() => POIEditorToolWindow.SetItemChoiceValue(ActionsRowOfPoi1(0), BuiltInBlocks.ActionsTargetField, stale.KeyAt(2)));
            Assert.AreEqual("block_3", POIEditorToolWindow.ItemChoiceValue(ActionsRowOfPoi1(0), BuiltInBlocks.ActionsTargetField));
            Assert.IsTrue(_window.Unsaved);
            yield return Redraw();
            Assert.IsFalse(Drawn("Block item actions 0 target warning#1"), "a block of the scrolling part: no warning");
            var built = BlockStackBuilder.Build(poi, _window.Config.card_settings, BlockRegistry.Shared, _window.Config.pois);
            var buttons = ActionsRule.Buttons(new BlockFieldReader(_window.Config.pois[0].card.blocks[1], "en", "en"), BuiltInBlocks.ActionsPillRow,
                BlockRefRule.JumpTargets(built.Entries, _ => false));
            Assert.AreEqual("block_3", buttons.Single(b => b.Action == BuiltInBlocks.ActionOpenBlock).Target, "the card now draws the Open Block button");

            yield return _window.PressUndo();
            Assert.AreEqual("block_9", POIEditorToolWindow.ItemChoiceValue(ActionsRowOfPoi1(0), BuiltInBlocks.ActionsTargetField), "Ctrl+Z");
            yield return Redraw();
            _window.RectOf("Block item actions 0 target warning#1");
        }

        // The warning under a BlockRef names the block it points at the way Card Content does, for every reason the card leaves the jump out,
        // on The Lamp's own blocks; The Lamp's two shipped Open Block rows lead somewhere and are not warned; and the repeated-action warning
        // names an Open Block repeat's target
        [Test]
        public void TheBlockRefWarnings_NameTheBlockForEveryReason_TheLampsOwnRowsHaveNone_AndAnOpenBlockRepeatNamesItsTarget()
        {
            var config = ShippedConfig();
            var lamp = config.pois.Single(p => p.id == "lamp");
            var languages = config.card_settings.languages;
            var built = BlockStackBuilder.Build(lamp, config.card_settings, BlockRegistry.Shared, config.pois);
            var holder = lamp.card.blocks.Single(b => b.key == "block_18");
            var label = BuiltInBlocks.Actions.Field(BuiltInBlocks.ActionsItemsField).ItemFields.Single(f => f.Key == BuiltInBlocks.ActionsTargetField).Label;

            foreach (var row in holder.fields.Single(f => f.key == BuiltInBlocks.ActionsItemsField).items.Skip(1))
                Assert.AreEqual(BlockRefRule.Problem.None, BlockRefRule.Check(POIEditorToolWindow.ItemChoiceValue(row, BuiltInBlocks.ActionsTargetField), holder, lamp, built, _ => false),
                    "the shipped Open Block rows lead to blocks of the scrolling part");

            string header = POIEditorToolWindow.CardBlockName(lamp, "block_1", languages);
            StringAssert.StartsWith("1. Header: ", header, "row number, kind, first words");
            var cases = new[]
            {
                (BlockRefRule.Problem.NotInScroll, "block_1"), (BlockRefRule.Problem.NotInScroll, "block_19"),
                (BlockRefRule.Problem.WaitsForReading, "block_44"), (BlockRefRule.Problem.Itself, "block_18"),
            };
            foreach (var (problem, key) in cases)
            {
                Assert.AreEqual(problem, BlockRefRule.Check(key, holder, lamp, built, _ => false), key);
                string text = POIEditorToolWindow.CardBlockRefProblemText(label, problem, POIEditorToolWindow.CardBlockName(lamp, key, languages));
                StringAssert.StartsWith(label, text, "names the field");
                if (problem != BlockRefRule.Problem.Itself) StringAssert.Contains(POIEditorToolWindow.CardBlockName(lamp, key, languages), text, "names the block");
                StringAssert.DoesNotContain("block_", text, key + ": never a key");
                EditorTextChecks.AssertAscii(text, key);
            }

            // - a second button to the Listen player, in the circles block: the same shortcut twice
            var circles = lamp.card.blocks.Single(b => b.key == "block_17");
            circles.fields.Single(f => f.key == BuiltInBlocks.ActionsItemsField).items.Add(OpenBlockRow("block_55"));
            var repeats = RepeatedActionRule.Find(BlockStackBuilder.Build(lamp, config.card_settings, BlockRegistry.Shared, config.pois).Entries);
            var openBlock = repeats.Single(r => r.Action == BuiltInBlocks.ActionOpenBlock);
            Assert.AreEqual("block_55", openBlock.Target);
            string repeated = POIEditorToolWindow.CardRepeatedActionWarnings(circles, repeats, lamp, languages)
                .Single(w => w.Contains("\"Open Block\""));
            StringAssert.Contains("\"Open Block\" to " + POIEditorToolWindow.CardBlockName(lamp, "block_55", languages) + " 2 times", repeated);
            StringAssert.DoesNotContain("block_", repeated);
            Assert.AreEqual(1, POIEditorToolWindow.CardRepeatedActionWarnings(holder, repeats, lamp, languages).Count(w => w.Contains("\"Open Block\"")),
                "the Sources button of block_18 opens another block: only the Listen shortcut repeats");
        }

        private List<BlockItemData> Swatches() =>
            _window.Config.pois[0].card.blocks[0].fields.Single(f => f.key == BuiltInBlocks.SwatchesItemsField).items;

        // ---------------- Glossary ----------------

        [UnityTest]
        public IEnumerator Glossary_RealClicksAndTyping_AddATerm_TheCardLinksIt_AndCardContentWarnsAboutAMissingOne()
        {
            var config = TwoPoiConfig();
            var block = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.RichTextKind };
            POIEditorToolWindow.SetLocalizedValue(block, BuiltInBlocks.RichTextBodyField, "en", "The [[keep]] and the [[moat]].");
            config.pois[0].card.blocks.Add(block);
            _window = new PoiEditorWindowHost(config, "_showCardGlossary");
            OpenTab("DetailCard");
            yield return _window.WaitForRepaint();
            CollectionAssert.AreEqual(new[] { "keep", "moat" },
                POIEditorToolWindow.MissingGlossaryTerms(block, BuiltInBlocks.RichText, _window.Config.card_settings.glossary), "both words are missing from the empty glossary");

            _window.Click("Glossary add#0");
            yield return _window.WaitForRepaint();
            yield return _window.ReplaceText("Glossary term#0", "Keep");
            yield return _window.ClickAway();
            yield return _window.ReplaceText("Glossary definition en#0", "The strongest tower.");
            yield return _window.ClickAway();
            var glossary = _window.Config.card_settings.glossary;
            Assert.AreEqual(1, glossary.Count);
            Assert.AreEqual("Keep", glossary[0].term);
            Assert.AreEqual("The strongest tower.", new CardGlossary(glossary, "en", "en").Definition("keep"), "the card finds it, ignoring case");
            CollectionAssert.AreEqual(new[] { "moat" },
                POIEditorToolWindow.MissingGlossaryTerms(_window.Config.pois[0].card.blocks[0], BuiltInBlocks.RichText, glossary), "only the undefined word is still flagged");
            StringAssert.Contains("moat", POIEditorToolWindow.CardGlossaryMissingText(new[] { "moat" }));

            yield return _window.PressUndo();
            yield return _window.PressUndo();
            yield return _window.PressUndo();
            CollectionAssert.IsEmpty(_window.Config.card_settings.glossary, "three Ctrl+Z: definition, term, row");
        }

        // ---------------- warnings on blocks that show ----------------

        private static BlockInstanceData ActionsWithRows(string variant, int rows)
        {
            var block = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.ActionsKind, variant = variant };
            var field = new BlockFieldValue { key = BuiltInBlocks.ActionsItemsField };
            for (int i = 0; i < rows; i++)
                field.items.Add(new BlockItemData { fields = new List<BlockItemFieldValue>
                {
                    new() { key = BuiltInBlocks.ActionsLabelField, text = new List<LocalizedEntry> { new() { lang = "en", value = "Button " + i } } },
                    new() { key = BuiltInBlocks.ActionsActionField, value = BuiltInBlocks.ActionShowOnWall },
                } });
            block.fields.Add(field);
            return block;
        }

        // A real drag-and-drop of a project file onto a probed control: DragUpdated then DragPerform at its centre, with the
        // file in DragAndDrop, exactly the events a person dragging from the Project window sends
        private IEnumerator DropOnto(string probeKey, UnityEngine.Object file)
        {
            var at = _window.Local(_window.RectOf(probeKey).center);
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = new[] { file };
            DragAndDrop.paths = new[] { AssetDatabase.GetAssetPath(file) };
            _window.Send(new Event { type = EventType.DragUpdated, mousePosition = at });
            _window.Send(new Event { type = EventType.DragPerform, mousePosition = at });
            DragAndDrop.PrepareStartDrag();
            yield return _window.WaitForRepaint();
        }

        private const string MediaFolder = "LivingRoom/CardMedia";

        [UnityTest]
        public IEnumerator AnAssetField_ARealDropOfAPictureInTheMediaFolder_StoresItsPathThere_OutsideTheFolderIsKeptWithAReason_AndCtrlZ()
        {
            var config = TwoPoiConfig();
            config.card_settings.languages = new List<string> { "en" };
            config.card_settings.media_resources_path = MediaFolder;
            var block = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.ZoomImageKind };
            config.pois[0].card.blocks.Add(block);
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenPoiCardContent("poi_1");
            var foldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor);
            foldouts["poi_1/block_1"] = true;
            yield return _window.WaitForRepaint();
            _window.RectOf("Block field image#0");
            BlockInstanceData Zoom() => _window.Config.pois[0].card.blocks[0];
            Assert.IsNull(Zoom().fields.Find(f => f.key == BuiltInBlocks.ZoomImageImageField), "drawing the picture row writes nothing");

            var inside = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Apps/LivingRoom/Resources/LivingRoom/CardMedia/tile_detail.png");
            Assert.IsNotNull(inside, "precondition: the fixture picture exists");
            yield return DropOnto("Block field image#0", inside);
            Assert.AreEqual("tile_detail.png", POIEditorToolWindow.AssetValue(Zoom(), BuiltInBlocks.ZoomImageImageField),
                "a real drop stores the path inside the Media Folder, not the project path");
            Assert.IsTrue(_window.Unsaved);
            Assert.AreSame(inside, POIEditorToolWindow.MediaAssetFor("tile_detail.png", MediaFolder), "the row shows the picture the app will load (through Resources)");
            var shown = BlockStackBuilder.Build(_window.Config.pois[0], _window.Config.card_settings, BlockRegistry.Shared);
            CollectionAssert.IsEmpty(shown.Skipped, "the card shows the block");

            yield return _window.PressUndo();
            Assert.AreEqual("", POIEditorToolWindow.AssetValue(Zoom(), BuiltInBlocks.ZoomImageImageField), "one Ctrl+Z takes the drop back");
            yield return _window.PressRedo();
            Assert.AreEqual("tile_detail.png", POIEditorToolWindow.AssetValue(Zoom(), BuiltInBlocks.ZoomImageImageField), "Ctrl+Y");

            const string outsidePath = "Assets/Framework/Runtime/UI/Markers/SymbolCircle.png";
            var outside = AssetDatabase.LoadAssetAtPath<Texture2D>(outsidePath);
            Assert.IsNotNull(outside, "precondition: a picture outside the Media Folder");
            yield return DropOnto("Block field image#0", outside);
            Assert.AreEqual(outsidePath, POIEditorToolWindow.AssetValue(Zoom(), BuiltInBlocks.ZoomImageImageField), "a picture from outside is kept as picked, never silently refused");
            var skipped = BlockStackBuilder.Build(_window.Config.pois[0], _window.Config.card_settings, BlockRegistry.Shared).Skipped.Single();
            Assert.AreEqual(BlockStackBuilder.SkipReason.InvalidMedia, skipped.Reason, "the card leaves it out...");
            var imageField = BuiltInBlocks.ZoomImage.Field(BuiltInBlocks.ZoomImageImageField);
            StringAssert.StartsWith("Not shown: Picture must be a PNG / JPG / JPEG picture inside the wall's Media Folder",
                POIEditorToolWindow.CardBlockSkipText(skipped.Reason, imageField), "...and Card Content says why, naming the field");
            StringAssert.Contains("outside the wall's Media Folder",
                POIEditorToolWindow.CardMediaProblemText(imageField.Label, imageField.Media, MediaPathRule.Check(outsidePath, imageField.Media)), "the warning under the row");
            yield return _window.WaitForRepaint();
            Assert.AreSame(outside, POIEditorToolWindow.MediaAssetFor(outsidePath, MediaFolder), "the row still shows what was picked");
            Assert.IsNull(POIEditorToolWindow.MediaAssetFor("ghost.png", MediaFolder), "a path with no file: the row can tell (the missing-file warning)");
        }

        // _3.1 13-fix: an Asset field holding a Framework default shows "Default: <key>" with its Clear default button INSIDE the row (a
        // capture showed it pushed to the window's far edge), a real click clears it, and the "Pick default..." button that replaces it
        // sits inside the row as well; Ctrl+Z gives the default back
        [UnityTest]
        public IEnumerator AnAssetFieldWithADefault_ClearDefaultSitsInsideTheRow_ARealClickClearsIt_PickDefaultTakesItsPlace_AndCtrlZ()
        {
            var config = TwoPoiConfig();
            config.card_settings.languages = new List<string> { "en" };
            config.card_settings.media_resources_path = MediaFolder;
            var block = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.ZoomImageKind };
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.ZoomImageImageField, asset = MediaPathRule.PathForDefaultKey("azulejo_detail") });
            config.pois[0].card.blocks.Add(block);
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenPoiCardContent("poi_1");
            var foldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor);
            foldouts["poi_1/block_1"] = true;
            yield return _window.WaitForRepaint();
            BlockInstanceData Zoom() => _window.Config.pois[0].card.blocks[0];

            Rect field = _window.RectOf("Block field image#0");
            Rect clear = _window.RectOf("Block field image clear default#0");
            Assert.LessOrEqual(clear.xMax, field.xMax + 1f, "Clear default ends inside the row, under its field: " + clear + " vs " + field);
            Assert.Greater(clear.y, field.y, "on the line under the field");

            _window.Click("Block field image clear default#0");
            yield return _window.WaitForRepaint();
            Assert.AreEqual("", POIEditorToolWindow.AssetValue(Zoom(), BuiltInBlocks.ZoomImageImageField), "a real click cleared the default");
            Rect pick = _window.RectOf("Block field image pick default#0");
            Assert.LessOrEqual(pick.xMax, field.xMax + 1f, "Pick default... sits inside the row too: " + pick + " vs " + field);

            yield return _window.PressUndo();
            Assert.AreEqual(MediaPathRule.PathForDefaultKey("azulejo_detail"), POIEditorToolWindow.AssetValue(Zoom(), BuiltInBlocks.ZoomImageImageField),
                "one Ctrl+Z gives the default back");
        }

        // _3.1 step 9A: the audio kind's Asset rows take a real AudioClip and a real captions file (a TextAsset made by the .vtt importer),
        // each stored as its path inside the Media Folder; a picture (or the other kind of file) dropped on the wrong row is not taken
        [UnityTest]
        public IEnumerator TheAudioGuideRows_RealDropsOfARealClipAndItsCaptions_StoreTheirPaths_TheWrongKindIsNotTaken_AndCtrlZ()
        {
            var config = TwoPoiConfig();
            config.card_settings.languages = new List<string> { "en" };
            config.card_settings.media_resources_path = MediaFolder;
            config.pois[0].card.blocks.Add(new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.AudioGuideKind });
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenPoiCardContent("poi_1");
            var foldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor);
            foldouts["poi_1/block_1"] = true;
            yield return _window.WaitForRepaint();
            _window.RectOf("Block field clip#0");
            _window.RectOf("Block field captions#0");
            _window.RectOf("Block field speeds#0");
            _window.RectOf("Block field captions_on#0");
            BlockInstanceData Audio() => _window.Config.pois[0].card.blocks[0];
            Assert.IsNull(Audio().fields.Find(f => f.key == BuiltInBlocks.AudioGuideClipField), "drawing the rows writes nothing");
            Assert.IsFalse(_window.Unsaved);

            const string audioFolder = "Assets/Apps/LivingRoom/Resources/LivingRoom/CardMedia/";
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioFolder + "audio/lamp_tone.wav");
            var captions = AssetDatabase.LoadAssetAtPath<TextAsset>(audioFolder + "audio/lamp_tone.vtt");
            Assert.IsNotNull(clip, "precondition: the fixture clip imports as an AudioClip");
            Assert.IsNotNull(captions, "precondition: the fixture captions import as a TextAsset");

            yield return DropOnto("Block field clip#0", clip);
            Assert.AreEqual("audio/lamp_tone.wav", POIEditorToolWindow.AssetValue(Audio(), BuiltInBlocks.AudioGuideClipField), "a real drop of a clip stores its path inside the Media Folder");
            yield return DropOnto("Block field captions#0", captions);
            Assert.AreEqual("audio/lamp_tone.vtt", POIEditorToolWindow.AssetValue(Audio(), BuiltInBlocks.AudioGuideCaptionsField), "and so does a captions file");
            Assert.IsTrue(_window.Unsaved);
            CollectionAssert.IsEmpty(BlockStackBuilder.Build(_window.Config.pois[0], _window.Config.card_settings, BlockRegistry.Shared).Skipped, "the card shows the block");

            // - the wrong kind of file on a row: a picture on the clip row, a clip on the captions row, a captions file on the clip row
            var picture = AssetDatabase.LoadAssetAtPath<Texture2D>(audioFolder + "tile_detail.png");
            yield return DropOnto("Block field clip#0", picture);
            yield return DropOnto("Block field captions#0", clip);
            yield return DropOnto("Block field clip#0", captions);
            Assert.AreEqual("audio/lamp_tone.wav", POIEditorToolWindow.AssetValue(Audio(), BuiltInBlocks.AudioGuideClipField), "the clip row takes clips only");
            Assert.AreEqual("audio/lamp_tone.vtt", POIEditorToolWindow.AssetValue(Audio(), BuiltInBlocks.AudioGuideCaptionsField), "the captions row takes captions files only");

            Assert.AreSame(clip, POIEditorToolWindow.MediaAssetFor("audio/lamp_tone.wav", MediaFolder, MediaKind.Audio), "the row shows the clip the app will load (through Resources)");
            Assert.AreSame(captions, POIEditorToolWindow.MediaAssetFor("audio/lamp_tone.vtt", MediaFolder, MediaKind.Captions));
            Assert.IsNull(POIEditorToolWindow.MediaAssetFor("audio/ghost.wav", MediaFolder, MediaKind.Audio), "a path with no file: the row can tell");
            Assert.IsNull(POIEditorToolWindow.MediaAssetFor("audio/lamp_tone.wav", MediaFolder, MediaKind.Captions), "the wrong type for the field is no asset");

            yield return _window.PressUndo();
            Assert.AreEqual("", POIEditorToolWindow.AssetValue(Audio(), BuiltInBlocks.AudioGuideCaptionsField), "one Ctrl+Z takes the captions drop back");
            yield return _window.PressUndo();
            Assert.AreEqual("", POIEditorToolWindow.AssetValue(Audio(), BuiltInBlocks.AudioGuideClipField), "and the next the clip's");
            yield return _window.PressRedo();
            Assert.AreEqual("audio/lamp_tone.wav", POIEditorToolWindow.AssetValue(Audio(), BuiltInBlocks.AudioGuideClipField), "Ctrl+Y");
        }

        // The words of a refused audio file, and the speeds list the Choice offers (with its "none" first, as every optional Choice has)
        [Test]
        public void TheAudioGuideFields_WarnInTheirOwnWords_AndTheSpeedsChoiceListsThePresets()
        {
            var clip = BuiltInBlocks.AudioGuide.Field(BuiltInBlocks.AudioGuideClipField);
            var captions = BuiltInBlocks.AudioGuide.Field(BuiltInBlocks.AudioGuideCaptionsField);
            StringAssert.Contains("an MP3 / WAV / OGG audio file inside the wall's Media Folder", POIEditorToolWindow.CardMediaProblemText(clip.Label, clip.Media, MediaPathProblem.None));
            StringAssert.Contains("not an audio file the card can use (MP3 / WAV / OGG)", POIEditorToolWindow.CardMediaProblemText(clip.Label, clip.Media, MediaPathProblem.WrongType));
            StringAssert.Contains("a VTT captions file", POIEditorToolWindow.CardMediaProblemText(captions.Label, captions.Media, MediaPathProblem.None));
            StringAssert.Contains("outside the wall's Media Folder", POIEditorToolWindow.CardMediaProblemText(clip.Label, clip.Media, MediaPathProblem.OutsideFolder));
            StringAssert.Contains("no audio file \"audio/x.mp3\"", POIEditorToolWindow.CardMediaMissingText(clip.Label, "audio/x.mp3", clip.Media));
            var speeds = BuiltInBlocks.AudioGuide.Field(BuiltInBlocks.AudioGuideSpeedsField);
            var (values, labels) = POIEditorToolWindow.BlockChoiceOptions(speeds, "");
            CollectionAssert.AreEqual(new[] { "", AudioSpeedRule.Narration, AudioSpeedRule.Wide, AudioSpeedRule.Off }, values, "none, then the presets");
            Assert.AreEqual(values.Count, labels.Count);
            StringAssert.Contains("0.75x to 1.5x", labels[1]);
            foreach (string text in labels.Concat(new[] { clip.Help, captions.Help, speeds.Help, BuiltInBlocks.AudioGuide.Help })) EditorTextChecks.AssertAscii(text, "an audio guide Editor text");
            // - a stored preset that does not exist stays selected as "(missing)", never dropped silently
            var (withStale, staleLabels) = POIEditorToolWindow.BlockChoiceOptions(speeds, "turbo");
            Assert.AreEqual("turbo", withStale[withStale.Count - 1]);
            StringAssert.Contains("(missing)", staleLabels[staleLabels.Count - 1]);
        }

        // The Card Container's Reduce Motion row (step 9B): off in the shipped wall, drawing writes nothing, a real click turns it on, Ctrl+Z
        [UnityTest]
        public IEnumerator TheReduceMotionRow_IsDrawnOff_ARealClickTurnsItOn_AndCtrlZTakesItBack()
        {
            _window = new PoiEditorWindowHost(ShippedConfig(), "_showCardContainer");
            OpenTab("DetailCard");
            yield return _window.WaitForRepaint();
            _window.RectOf("Reduce Motion");
            Assert.IsFalse(_window.Unsaved, "drawing the row writes nothing");
            Assert.IsFalse(_window.Config.card_settings.container.reduce_motion, "off in the shipped wall: header loops play");

            _window.Click("Reduce Motion");
            yield return _window.WaitForRepaint();
            Assert.IsTrue(_window.Config.card_settings.container.reduce_motion, "a real click turned it on");
            Assert.IsTrue(_window.Unsaved);
            yield return _window.PressUndo();
            Assert.IsFalse(_window.Config.card_settings.container.reduce_motion, "Ctrl+Z");
        }

        // ---------------- Detail Card > Navigation (_3.2 step 1) ----------------

        // The Navigation section's Block Order row: the shipped wall never wrote it, so it reads the default; drawing writes
        // nothing; the popup's value goes through the window's own history (an IMGUI popup opens a native menu no test can
        // click, so the row's VALUE is driven the way EveryCardSettingsField_... drives every other popup of this tab).
        [UnityTest]
        public IEnumerator TheBlockOrderRow_IsDrawnOnItsDefault_AndDrawingWritesNothing()
        {
            _window = new PoiEditorWindowHost(ShippedConfig(), "_showCardNavigation");
            OpenTab("DetailCard");
            yield return _window.WaitForRepaint();
            // - no RectOf here: DrawPopupField reports no rect on purpose, because an IMGUI popup opens a NATIVE menu that
            //   no test can click. Its value is driven through the window's history by EveryCardSettingsField_..., like
            //   Open At and Audio Overlap; what this test pins is that drawing the row changes nothing.
            Assert.IsFalse(_window.Unsaved, "drawing the row writes nothing");
            Assert.AreEqual(CardOptions.BlockOrderEndFamiliesLast, _window.Config.card_settings.navigation.block_order,
                "the shipped wall never set it, so it reads the default");
            CollectionAssert.IsEmpty(_window.Config.card_settings.navigation.families,
                "and no Families row is written just because the section is open");
            Assert.AreEqual(CardOptions.BlockOrders.Length, CardOptions.BlockOrderLabels.Length, "one label per Block Order option");
        }

        // Detail Card > Navigation's step 4 rows (Jump Opens Card To, Jump Motion, Back Button), on the real window: drawn on their
        // defaults (the shipped wall never wrote them) without writing; each popup's write path picks the other option through the
        // window's own history and a real Ctrl+Z takes exactly that pick back (an IMGUI popup opens a native menu no test can click --
        // the Block Order row's limit, above); the option lists and their labels line up, and the help texts say what each option does
        [UnityTest]
        public IEnumerator TheJumpAndBackRows_AreDrawnOnTheirDefaults_DrawingWritesNothing_AndEachPickIsUndoneByCtrlZ()
        {
            _window = new PoiEditorWindowHost(ShippedConfig(), "_showCardNavigation");
            OpenTab("DetailCard");
            yield return Redraw();
            Assert.IsFalse(_window.Unsaved, "drawing the three rows writes nothing");
            var nav = _window.Config.card_settings.navigation;
            Assert.AreEqual(CardOptions.JumpRaisesFull, nav.jump_raises_to);
            Assert.AreEqual(CardOptions.JumpMotionSmooth, nav.jump_motion);
            Assert.AreEqual(CardOptions.BackButtonStepDown, nav.back_button);
            Assert.AreEqual(CardOptions.JumpRaises.Length, CardOptions.JumpRaiseLabels.Length);
            Assert.AreEqual(CardOptions.JumpMotions.Length, CardOptions.JumpMotionLabels.Length);
            Assert.AreEqual(CardOptions.BackButtons.Length, CardOptions.BackButtonLabels.Length);
            Assert.AreEqual(new CardNavigationSettings().jump_raises_to, CardOptions.JumpRaises[0], "an unknown value draws as option 0, which is the card's own default");
            Assert.AreEqual(new CardNavigationSettings().jump_motion, CardOptions.JumpMotions[0]);
            Assert.AreEqual(new CardNavigationSettings().back_button, CardOptions.BackButtons[0]);

            var picks = new (string Name, Action<CardNavigationSettings> Set, Func<CardNavigationSettings, string> Read, string Before, string After)[]
            {
                ("Jump Opens Card To", n => n.jump_raises_to = CardOptions.JumpRaisesKeep, n => n.jump_raises_to, CardOptions.JumpRaisesFull, CardOptions.JumpRaisesKeep),
                ("Jump Motion", n => n.jump_motion = CardOptions.JumpMotionInstant, n => n.jump_motion, CardOptions.JumpMotionSmooth, CardOptions.JumpMotionInstant),
                ("Back Button", n => n.back_button = CardOptions.BackButtonNone, n => n.back_button, CardOptions.BackButtonStepDown, CardOptions.BackButtonNone),
            };
            foreach (var (name, set, read, before, after) in picks)
            {
                Pick(() => set(_window.Config.card_settings.navigation));
                yield return Redraw();
                Assert.AreEqual(after, read(_window.Config.card_settings.navigation), name + ": the pick is stored");
                Assert.IsTrue(_window.Unsaved, name);
                yield return _window.PressUndo();
                Assert.AreEqual(before, read(_window.Config.card_settings.navigation), name + ": a real Ctrl+Z takes the pick back");
            }

            var help = typeof(POIEditorToolWindow);
            foreach (var (field, words) in new[]
            {
                ("CardJumpRaisesHelp", new[] { "Full", "Keep Its Stop" }), ("CardJumpMotionHelp", new[] { "Smooth", "Instant", "Reduce Motion" }),
                ("CardBackButtonHelp", new[] { "Step The Card Down", "Leave It To The App", "iPhone" }),
            })
            {
                string text = (string)help.GetField(field, Static).GetValue(null);
                foreach (string word in words) StringAssert.Contains(word, text, field + " names " + word);
                EditorTextChecks.AssertAscii(text, field);
            }
        }

        // Detail Card > Navigation's step 5a rows on the real window. Show Navigation From: drawn on its default 8 without writing, a real
        // click near the slider's left end lowers it to its minimum, Ctrl+Z takes it back. Top Bar: its choices are None + every element
        // the registry has for the top slot (an app's own one included, with no edit to the window), named by their labels; the pick goes
        // through the window's history and Ctrl+Z takes it back; its help lists every element's own words. Both helps are ASCII
        [UnityTest]
        public IEnumerator TheShowNavigationFromAndTopBarRows_AreDrawnOnTheirDefaults_TakeARealPick_AndCtrlZTakesItBack()
        {
            _window = new PoiEditorWindowHost(ShippedConfig(), "_showCardNavigation");
            OpenTab("DetailCard");
            yield return Redraw();
            Assert.IsFalse(_window.Unsaved, "drawing the two rows writes nothing");
            var nav = _window.Config.card_settings.navigation;
            Assert.AreEqual(8, nav.min_blocks, "the shipped wall never set Show Navigation From: the default");
            Assert.AreEqual(BuiltInNavElements.TextTabs, nav.top, "nor Top Bar: Text Tabs");

            // - a real click near the LEFT end of the slider's track (its number box sits after the track)
            yield return _window.ScrollTo("Show Navigation From");
            Rect row = _window.RectOf("Show Navigation From");
            _window.ClickAt(new Vector2(row.x + EditorGUIUtility.labelWidth + 4f, row.center.y));
            yield return _window.WaitForRepaint();
            Assert.AreEqual(CardNavigationSettings.MinBlocksMin, _window.Config.card_settings.navigation.min_blocks, "a real click at the track's start: the minimum");
            Assert.IsTrue(_window.Unsaved);
            yield return _window.PressUndo();
            Assert.AreEqual(8, _window.Config.card_settings.navigation.min_blocks, "Ctrl+Z");

            var (keys, labels) = POIEditorToolWindow.SlotOptions(NavElementRegistry.Shared, NavSlotKeys.Top);
            CollectionAssert.AreEqual(new[] { CardOptions.NavNone }.Concat(NavElementRegistry.Shared.OfSlot(NavSlotKeys.Top).Select(e => e.Key)), keys);
            CollectionAssert.AreEqual(new[] { CardOptions.NavNoneLabel }.Concat(NavElementRegistry.Shared.OfSlot(NavSlotKeys.Top).Select(e => e.Label)), labels,
                "named by the elements' labels, never their keys");
            Pick(() => _window.Config.card_settings.navigation.top = CardOptions.NavNone);
            yield return Redraw();
            Assert.AreEqual(CardOptions.NavNone, _window.Config.card_settings.navigation.top);
            Assert.IsTrue(_window.Unsaved);
            yield return _window.PressUndo();
            Assert.AreEqual(BuiltInNavElements.TextTabs, _window.Config.card_settings.navigation.top, "Ctrl+Z takes the pick back");

            // - an app's element joins the list with no window edit
            var registry = new NavElementRegistry();
            BuiltInNavElements.Register(registry);
            registry.Register(new NavElementDefinition("app_dots", NavSlotKeys.Top, "App Dots", "App Dots: one dot per section.", NavChoice.Sections), () => new TextTabsNavView());
            var (appKeys, appLabels) = POIEditorToolWindow.SlotOptions(registry, NavSlotKeys.Top);
            Assert.AreEqual("app_dots", appKeys.Last());
            Assert.AreEqual("App Dots", appLabels.Last());
            var topHelp = typeof(POIEditorToolWindow).GetMethod("CardTopBarHelp", Static);
            StringAssert.Contains("App Dots: one dot per section.", (string)topHelp.Invoke(null, new object[] { registry }), "the help speaks for the app's element too");

            string shared = (string)topHelp.Invoke(null, new object[] { NavElementRegistry.Shared });
            foreach (string word in new[] { "None", "Text Tabs", "Show Navigation From" }) StringAssert.Contains(word, shared);
            EditorTextChecks.AssertAscii(shared, "CardTopBarHelp");
            string from = (string)typeof(POIEditorToolWindow).GetField("CardShowNavigationFromHelp", Static).GetValue(null);
            foreach (string word in new[] { "title block", "Show After Reading", "Default 8" }) StringAssert.Contains(word, from);
            EditorTextChecks.AssertAscii(from, "CardShowNavigationFromHelp");
        }

        // Detail Card > Navigation's step 5b rows on the real window: Show Navigation At and Hide While Scrolling are drawn on their
        // defaults without writing (option 0 is the card's own default, so an unknown value draws as what the card does); each pick goes
        // through the window's history and a real Ctrl+Z takes it back; Show Navigation At's (i) tells a wall author the card may hide the
        // bar by itself where a height has no room (so nobody files it as a bug), Hide While Scrolling's that the Top Bar stays; ASCII
        [UnityTest]
        public IEnumerator TheShowNavigationAtAndHideWhileScrollingRows_AreDrawnOnTheirDefaults_AndEachPickIsUndoneByCtrlZ()
        {
            _window = new PoiEditorWindowHost(ShippedConfig(), "_showCardNavigation");
            OpenTab("DetailCard");
            yield return Redraw();
            Assert.IsFalse(_window.Unsaved, "drawing the two rows writes nothing");
            var nav = _window.Config.card_settings.navigation;
            Assert.AreEqual(CardOptions.ShowNavHalfAndFull, nav.show_at, "the shipped wall never set Show Navigation At: the default");
            Assert.AreEqual(CardOptions.HideOnScrollDown, nav.hide_on_scroll, "nor Hide While Scrolling");
            Assert.AreEqual(CardOptions.ShowNavigationAts.Length, CardOptions.ShowNavigationAtLabels.Length);
            Assert.AreEqual(CardOptions.HideOnScrolls.Length, CardOptions.HideOnScrollLabels.Length);
            Assert.AreEqual(new CardNavigationSettings().show_at, CardOptions.ShowNavigationAts[0], "an unknown value draws as option 0, the card's own default");
            Assert.AreEqual(new CardNavigationSettings().hide_on_scroll, CardOptions.HideOnScrolls[0]);

            var picks = new (string Name, Action<CardNavigationSettings> Set, Func<CardNavigationSettings, string> Read, string Before, string After)[]
            {
                ("Show Navigation At", n => n.show_at = CardOptions.ShowNavFullOnly, n => n.show_at, CardOptions.ShowNavHalfAndFull, CardOptions.ShowNavFullOnly),
                ("Hide While Scrolling", n => n.hide_on_scroll = CardOptions.HideOnScrollNever, n => n.hide_on_scroll, CardOptions.HideOnScrollDown, CardOptions.HideOnScrollNever),
            };
            foreach (var (name, set, read, before, after) in picks)
            {
                Pick(() => set(_window.Config.card_settings.navigation));
                yield return Redraw();
                Assert.AreEqual(after, read(_window.Config.card_settings.navigation), name + ": the pick is stored");
                Assert.IsTrue(_window.Unsaved, name);
                yield return _window.PressUndo();
                Assert.AreEqual(before, read(_window.Config.card_settings.navigation), name + ": a real Ctrl+Z takes the pick back");
            }

            var help = typeof(POIEditorToolWindow);
            foreach (var (field, words) in new[]
            {
                ("CardShowNavigationAtHelp", new[] { "Half And Full", "Full Only", "BY ITSELF", "on purpose, not a fault", "a heading and two lines", "comes back" }),
                ("CardHideWhileScrollingHelp", new[] { "When Scrolling Down", "Never", "scrolls up a little", "the end of the card", "The Top Bar always stays" }),
            })
            {
                string text = (string)help.GetField(field, Static).GetValue(null);
                foreach (string word in words) StringAssert.Contains(word, text, field + " says " + word);
                EditorTextChecks.AssertAscii(text, field);
            }
            string guide = (string)help.GetField("CardNavigationPlaymodeTestGuide", Static).GetValue(null);
            foreach (string label in CardOptions.ShowNavigationAtLabels.Concat(CardOptions.HideOnScrollLabels))
                StringAssert.Contains(label, guide, "the Playmode guide names " + label);
            EditorTextChecks.AssertAscii(guide, "CardNavigationPlaymodeTestGuide");
        }

        // Detail Card > Navigation's Follow Scroll row (its data has been read since step 3; the row arrived at step 6), on the real
        // window: drawn ticked (the default) without writing, a real click on its checkbox unticks it, Ctrl+Z ticks it again; it sits
        // between Top Bar and Hide While Scrolling (section 7's order); its (i) says what each state does, in ASCII
        [UnityTest]
        public IEnumerator TheFollowScrollRow_IsARealToggle_BetweenTopBarAndHideWhileScrolling_AndCtrlZTakesItBack()
        {
            _window = new PoiEditorWindowHost(ShippedConfig(), "_showCardNavigation");
            OpenTab("DetailCard");
            yield return Redraw();
            Assert.IsFalse(_window.Unsaved, "drawing the row writes nothing");
            Assert.IsTrue(_window.Config.card_settings.navigation.follow_scroll, "the shipped wall never set it: on");

            yield return _window.ScrollTo("Follow Scroll");
            _window.Click("Follow Scroll");
            yield return _window.WaitForRepaint();
            Assert.IsFalse(_window.Config.card_settings.navigation.follow_scroll, "a real click on the checkbox unticks it");
            Assert.IsTrue(_window.Unsaved);
            yield return _window.PressUndo();
            Assert.IsTrue(_window.Config.card_settings.navigation.follow_scroll, "Ctrl+Z ticks it again");

            // - the rows' order, read from the (i) each row draws in its own line (popup rows report no rect of their own)
            var drawn = new List<(string Title, Rect Rect)>();
            HelpInfoButton.DrawnProbe = (title, rect) => drawn.Add((title, rect));
            try
            {
                yield return Redraw();
            }
            finally
            {
                HelpInfoButton.DrawnProbe = null;
            }
            float Y(string title) => drawn.Single(d => d.Title == title).Rect.y;
            Assert.Less(Y("Top Bar"), Y("Follow Scroll"), "under Top Bar");
            Assert.Less(Y("Follow Scroll"), Y("Hide While Scrolling"), "above Hide While Scrolling");

            string help = (string)typeof(POIEditorToolWindow).GetField("CardFollowScrollHelp", Static).GetValue(null);
            foreach (string word in new[] { "On (the default)", "Off", "scrolls", "taps" }) StringAssert.Contains(word, help);
            EditorTextChecks.AssertAscii(help, "CardFollowScrollHelp");
        }

        // The (i) titles one Repaint of the Navigation section draws, top to bottom (each row draws its (i) on its own line)
        private IEnumerator DrawnInfoTitles(List<(string Title, Rect Rect)> into)
        {
            into.Clear();
            HelpInfoButton.DrawnProbe = (title, rect) => into.Add((title, rect));
            try
            {
                yield return Redraw();
            }
            finally
            {
                HelpInfoButton.DrawnProbe = null;
            }
        }

        // Detail Card > Navigation's step 6 rows on the real window. Group By always; Labels only for a Top Bar element that draws icons;
        // Show Counts only for one that shows counts; the Groups table (and the Families table's Group column) only for Segmented -- read
        // from the element's own definition, so an app's element needs no window edit. A hidden row keeps its value. Each pick goes
        // through the window's history and a real Ctrl+Z takes it back; Show Counts is a real click; every help is ASCII and names its options
        [UnityTest]
        public IEnumerator TheStep6Rows_ShowOnlyForATopBarThatReadsThem_KeepTheirValueHidden_AndEachPickIsUndoneByCtrlZ()
        {
            _window = new PoiEditorWindowHost(ShippedConfig(), "_showCardNavigation");
            OpenTab("DetailCard");
            var drawn = new List<(string Title, Rect Rect)>();
            yield return DrawnInfoTitles(drawn);
            Assert.IsFalse(_window.Unsaved, "drawing the rows writes nothing");
            var nav = _window.Config.card_settings.navigation;
            Assert.AreEqual(BuiltInNavElements.TextTabs, nav.top, "precondition: the default Top Bar");
            var titles = drawn.Select(d => d.Title).ToList();
            CollectionAssert.Contains(titles, "Group By");
            CollectionAssert.Contains(titles, "Show Counts", "Text Tabs show counts");
            CollectionAssert.DoesNotContain(titles, "Labels", "Text Tabs draw no icons: no Labels row");
            CollectionAssert.DoesNotContain(titles, "Groups", "nor the Groups table");
            float Y(string title) => drawn.Single(d => d.Title == title).Rect.y;
            Assert.Less(Y("Families"), Y("Group By"), "Group By follows the Families table (section 7's order)");
            Assert.Less(Y("Group By"), Y("Show Navigation From"));
            Assert.Less(Y("Top Bar"), Y("Show Counts"));
            Assert.Less(Y("Show Counts"), Y("Follow Scroll"));

            // - Show Counts: a real click on its checkbox, Ctrl+Z
            yield return _window.ScrollTo("Show Counts");
            _window.Click("Show Counts");
            yield return _window.WaitForRepaint();
            Assert.IsTrue(_window.Config.card_settings.navigation.show_counts, "a real click ticks Show Counts");
            yield return _window.PressUndo();
            Assert.IsFalse(_window.Config.card_settings.navigation.show_counts, "Ctrl+Z");

            // - Group By: the pick through the window's history (an IMGUI popup opens a native menu), Ctrl+Z
            Pick(() => _window.Config.card_settings.navigation.group_by = CardOptions.GroupByFormat);
            yield return Redraw();
            Assert.AreEqual(CardOptions.GroupByFormat, _window.Config.card_settings.navigation.group_by);
            yield return _window.PressUndo();
            Assert.AreEqual(CardOptions.GroupByFamily, _window.Config.card_settings.navigation.group_by, "Ctrl+Z");

            // - Icon Tabs: Labels appears; its pick and Ctrl+Z
            Pick(() => _window.Config.card_settings.navigation.top = BuiltInNavElements.IconTabs);
            yield return DrawnInfoTitles(drawn);
            Assert.Less(drawn.Single(d => d.Title == "Top Bar").Rect.y, drawn.Single(d => d.Title == "Labels").Rect.y, "Icon Tabs: Labels, under Top Bar");
            Pick(() => _window.Config.card_settings.navigation.labels = CardOptions.LabelsIcon);
            yield return Redraw();
            Assert.AreEqual(CardOptions.LabelsIcon, _window.Config.card_settings.navigation.labels);
            yield return _window.PressUndo();
            Assert.AreEqual(CardOptions.LabelsIconAndText, _window.Config.card_settings.navigation.labels, "Ctrl+Z");

            // - Segmented: the Groups table and the Group column; Labels hidden -- and a hidden row keeps its value
            Pick(() => _window.Config.card_settings.navigation.labels = CardOptions.LabelsText);
            Pick(() => _window.Config.card_settings.navigation.top = BuiltInNavElements.Segmented);
            yield return DrawnInfoTitles(drawn);
            titles = drawn.Select(d => d.Title).ToList();
            CollectionAssert.Contains(titles, "Groups", "Segmented: the Groups table");
            CollectionAssert.DoesNotContain(titles, "Labels", "Segmented draws no icons");
            Assert.Greater(drawn.Single(d => d.Title == "Groups").Rect.y, drawn.Single(d => d.Title == "Back Button").Rect.y, "the Groups table comes last");
            Assert.AreEqual(CardOptions.LabelsText, _window.Config.card_settings.navigation.labels, "the hidden Labels row kept its value");
            var groupCell = _window.RectOf("Family group#0");
            Assert.GreaterOrEqual(groupCell.width, 60f, "the Families table shows its Group column, wide enough for a group's word");
            // - the 620 pt capture caught the "At The End" header word running into "Group": the column starts after that word
            float atEndWord = EditorStyles.miniBoldLabel.CalcSize(new GUIContent("At The End")).x;
            Assert.GreaterOrEqual(groupCell.xMin, _window.RectOf("Family at end#0").xMin + atEndWord, "Group starts after the At The End header word");

            var help = typeof(POIEditorToolWindow);
            foreach (var (field, words) in new[]
            {
                ("CardGroupByHelp", new[] { "Family", "Format", "Segmented" }), ("CardLabelsHelp", new[] { "Icon And Text", "Icon", "Text", "screen reader" }),
                ("CardShowCountsHelp", new[] { "Off (the default)" }), ("CardGroupsHelp", new[] { "Group column", "framework default suggestions", "one group" }),
                ("CardGroupNameHelp", new[] { "one short word" }), ("CardFamiliesHelp", new[] { "Group", "(no group)" }),
            })
            {
                string text = (string)help.GetField(field, Static).GetValue(null);
                foreach (string word in words) StringAssert.Contains(word, text, field + " says " + word);
                EditorTextChecks.AssertAscii(text, field);
            }
            foreach (string label in CardOptions.LabelsLabels.Concat(CardOptions.GroupByLabels)) EditorTextChecks.AssertAscii(label, label);
            string guide = (string)help.GetField("CardNavigationPlaymodeTestGuide", Static).GetValue(null);
            foreach (var element in NavElementRegistry.Shared.OfSlot(NavSlotKeys.Top))
                StringAssert.Contains("Top Bar = " + element.Label, guide, "the Playmode guide walks " + element.Label);
        }

        // Detail Card > Navigation's step 7a rows on the real window. Bottom Bar is drawn on its default, None, under Top Bar and listed
        // from the registry (None, then the bottom slot's elements by their labels); Bar Slots shows only for a Bottom Bar element that is a
        // bar of slots, under the display options and before Follow Scroll, and keeps its value while hidden. A real click at the start of
        // Bar Slots' track picks the minimum, 3; each change is undone by a real Ctrl+Z. With the bar, Labels shows even under Text Tabs (the
        // bar draws icons). Both helps and the Playmode guide say what a wall author needs, in ASCII
        [UnityTest]
        public IEnumerator TheBottomBarAndBarSlotsRows_ShowBarSlotsOnlyForABar_KeepItsValueHidden_AndEachChangeIsUndoneByCtrlZ()
        {
            _window = new PoiEditorWindowHost(ShippedConfig(), "_showCardNavigation");
            OpenTab("DetailCard");
            var drawn = new List<(string Title, Rect Rect)>();
            yield return DrawnInfoTitles(drawn);
            Assert.IsFalse(_window.Unsaved, "drawing the rows writes nothing");
            var nav = _window.Config.card_settings.navigation;
            Assert.AreEqual(CardOptions.NavNone, nav.bottom, "the shipped wall never set Bottom Bar: None");
            Assert.AreEqual(5, nav.max_slots, "nor Bar Slots: 5");
            var titles = drawn.Select(d => d.Title).ToList();
            CollectionAssert.Contains(titles, "Bottom Bar", "the row is drawn on its default");
            CollectionAssert.DoesNotContain(titles, "Bar Slots", "no bar: no Bar Slots");
            CollectionAssert.DoesNotContain(titles, "Labels", "Text Tabs and no bar: no Labels");
            float Y(string title) => drawn.Single(d => d.Title == title).Rect.y;
            Assert.Less(Y("Top Bar"), Y("Bottom Bar"), "Bottom Bar under Top Bar (section 7's order)");
            Assert.Less(Y("Bottom Bar"), Y("Follow Scroll"));

            var (keys, labels) = POIEditorToolWindow.SlotOptions(NavElementRegistry.Shared, NavSlotKeys.Bottom);
            CollectionAssert.AreEqual(new[] { CardOptions.NavNone, BuiltInNavElements.BottomBar }, keys, "None, then the bottom slot's elements");
            CollectionAssert.AreEqual(new[] { CardOptions.NavNoneLabel, "Bottom Bar" }, labels, "named by their labels, never their keys");

            // - the pick through the window's history (an IMGUI popup opens a native menu): Bar Slots and Labels appear
            Pick(() => _window.Config.card_settings.navigation.bottom = BuiltInNavElements.BottomBar);
            yield return DrawnInfoTitles(drawn);
            Assert.IsTrue(_window.Unsaved);
            titles = drawn.Select(d => d.Title).ToList();
            CollectionAssert.Contains(titles, "Bar Slots", "a bar of slots: Bar Slots");
            CollectionAssert.Contains(titles, "Labels", "the bar draws icons: Labels, whatever the Top Bar");
            Assert.Less(Y("Bottom Bar"), Y("Labels"));
            Assert.Less(Y("Show Counts"), Y("Bar Slots"), "Bar Slots after the display options (section 7's order)");
            Assert.Less(Y("Bar Slots"), Y("Follow Scroll"));

            // - a real click near the LEFT end of Bar Slots' track: the minimum, 3; Ctrl+Z gives 5 back
            yield return _window.ScrollTo("Bar Slots");
            Rect row = _window.RectOf("Bar Slots");
            _window.ClickAt(new Vector2(row.x + EditorGUIUtility.labelWidth + 4f, row.center.y));
            yield return _window.WaitForRepaint();
            Assert.AreEqual(CardNavigationSettings.MaxSlotsMin, _window.Config.card_settings.navigation.max_slots, "a real click at the track's start: 3 slots");
            yield return _window.PressUndo();
            Assert.AreEqual(5, _window.Config.card_settings.navigation.max_slots, "Ctrl+Z");

            // - a hidden row keeps its value: 4 slots, then Bottom Bar back to None
            Pick(() => _window.Config.card_settings.navigation.max_slots = 4);
            Pick(() => _window.Config.card_settings.navigation.bottom = CardOptions.NavNone);
            yield return DrawnInfoTitles(drawn);
            CollectionAssert.DoesNotContain(drawn.Select(d => d.Title).ToList(), "Bar Slots", "None: Bar Slots hidden");
            Assert.AreEqual(4, _window.Config.card_settings.navigation.max_slots, "the hidden row kept its value");
            yield return _window.PressUndo();
            Assert.AreEqual(BuiltInNavElements.BottomBar, _window.Config.card_settings.navigation.bottom, "Ctrl+Z brings the bar back");
            yield return _window.PressUndo();
            yield return _window.PressUndo();
            Assert.AreEqual(CardOptions.NavNone, _window.Config.card_settings.navigation.bottom, "and the first pick is undone too");

            var help = typeof(POIEditorToolWindow);
            string bottomHelp = (string)help.GetMethod("CardBottomBarHelp", Static).Invoke(null, new object[] { NavElementRegistry.Shared });
            foreach (string word in new[] { "None (the default)", "Show Navigation From", "Hide While Scrolling", "give way together", "Bottom Bar: ", "More" })
                StringAssert.Contains(word, bottomHelp, "CardBottomBarHelp says " + word);
            EditorTextChecks.AssertAscii(bottomHelp, "CardBottomBarHelp");
            string slotsHelp = (string)help.GetField("CardBarSlotsHelp", Static).GetValue(null);
            foreach (string word in new[] { "from 3 to 5", "default 5", "More", "fewer sections than slots" })
                StringAssert.Contains(word, slotsHelp, "CardBarSlotsHelp says " + word);
            EditorTextChecks.AssertAscii(slotsHelp, "CardBarSlotsHelp");
            string guide = (string)help.GetField("CardNavigationPlaymodeTestGuide", Static).GetValue(null);
            foreach (var element in NavElementRegistry.Shared.OfSlot(NavSlotKeys.Bottom))
                StringAssert.Contains("Bottom Bar = " + element.Label, guide, "the Playmode guide walks " + element.Label);
            StringAssert.Contains("Set Bar Slots to 3, 4 and 5", guide);
        }

        // The Groups table on the real window, with Top Bar = Segmented. A wall with no rows shows the framework's four by their words and
        // writes nothing; REAL typing in a name cell writes all four (their words copied in) with the typed name, and one Ctrl+Z takes the
        // whole write back; "+ Add group" and a row's trash are real clicks, each undone by Ctrl+Z; picking a family's Group moves its blocks
        // into that group on the card's model, "(no group)" leaves it in none and the table warns -- naming the family by its word
        [UnityTest]
        public IEnumerator TheGroupsTable_ShowsTheDefaults_RealTypingWritesThem_AddAndDeleteAreRealClicks_AndAFamilysGroupReachesTheModel()
        {
            var config = ShippedConfig();
            config.card_settings.navigation.top = BuiltInNavElements.Segmented;
            _window = new PoiEditorWindowHost(config, "_showCardNavigation");
            OpenTab("DetailCard");
            yield return Redraw();
            Assert.IsFalse(_window.Unsaved, "drawing the Groups table writes nothing");
            var nav = _window.Config.card_settings.navigation;
            CollectionAssert.IsEmpty(nav.groups, "precondition: the shipped wall has no Groups rows");
            var (keys, names) = _window.Editor.GroupChoices(nav);
            CollectionAssert.AreEqual(NavGroupRule.DefaultKeys.Append(CardOptions.NavNone), keys);
            CollectionAssert.AreEqual(new[] { "Discover", "Watch", "Explore", "Visit", "(no group)" }, names, "named by their words, never their keys");

            // - real typing: the four defaults are written, with the typed name
            yield return _window.ScrollTo("Group name pt#1");
            yield return _window.ReplaceText("Group name pt#1", "Ver e ouvir");
            nav = _window.Config.card_settings.navigation;
            CollectionAssert.AreEqual(NavGroupRule.DefaultKeys, nav.groups.Select(g => g.key), "the first edit writes the four rows");
            Assert.AreEqual("Ver e ouvir", nav.groups[1].label.Single(l => l.lang == "pt").value, "with the typed name");
            Assert.AreEqual("Watch", nav.groups[1].label.Single(l => l.lang == "en").value, "and the words it showed copied in");
            Assert.AreEqual("Descobrir", nav.groups[0].label.Single(l => l.lang == "pt").value);
            yield return _window.PressUndo();
            CollectionAssert.IsEmpty(_window.Config.card_settings.navigation.groups, "one Ctrl+Z takes the whole write back");

            // - + Add group: a real click, a fresh generated key
            yield return _window.ScrollTo("Group add#0");
            _window.Click("Group add#0");
            yield return _window.WaitForRepaint();
            nav = _window.Config.card_settings.navigation;
            CollectionAssert.AreEqual(NavGroupRule.DefaultKeys.Append("group_5"), nav.groups.Select(g => g.key), "the four, then group_5");
            yield return Redraw();
            StringAssert.Contains("Group 5 has no name in en", string.Join("|", _window.Editor.GroupWarnings(nav, "en")), "a group with no name is warned about");
            yield return _window.PressUndo();
            CollectionAssert.IsEmpty(_window.Config.card_settings.navigation.groups, "Ctrl+Z");

            // - a row's trash: a real click deletes it (Watch); Media and Listen are then in no group, and the warning names them by word
            yield return _window.ScrollTo("Group delete#1");
            _window.Click("Group delete#1");
            yield return _window.WaitForRepaint();
            nav = _window.Config.card_settings.navigation;
            CollectionAssert.AreEqual(new[] { NavGroupRule.Discover, NavGroupRule.Explore, NavGroupRule.Visit }, nav.groups.Select(g => g.key), "Watch deleted");
            string warning = string.Join("|", _window.Editor.GroupWarnings(nav, "en"));
            StringAssert.Contains("In no group: Media, Listen", warning, "named by their words");
            StringAssert.DoesNotContain("(media)", warning, "never by their keys");
            yield return _window.PressUndo();
            CollectionAssert.IsEmpty(_window.Config.card_settings.navigation.groups, "Ctrl+Z");

            // - a family's Group (the popup's pick through the history): its blocks move into that group on the card's model
            Pick(() => POIEditorToolWindow.SetFamilyGroup(_window.Config.card_settings.navigation, BuiltInFamilies.Media, NavGroupRule.Discover));
            yield return Redraw();
            var settings = _window.Config.card_settings;
            var lamp = _window.Config.pois.Single(p => p.id == "lamp");
            var model = CardNavModel.Build(BlockStackBuilder.Build(lamp, settings, BlockRegistry.Shared, _window.Config.pois).Entries, BlockRegistry.Shared, settings);
            string mediaBlock = model.Sections.Single(s => s.Key == BuiltInFamilies.Media).Targets[0];
            Assert.AreEqual(NavGroupRule.Discover, model.Groups[model.GroupOf(mediaBlock)].Key, "a Media block now lights Discover");
            Assert.AreEqual(1, settings.navigation.families.Count, "only Media got a row");
            Pick(() => POIEditorToolWindow.SetFamilyGroup(_window.Config.card_settings.navigation, BuiltInFamilies.Media, CardOptions.NavNone));
            yield return Redraw();
            Assert.IsNull(NavGroupRule.GroupOf(BuiltInFamilies.Media, _window.Config.card_settings.navigation), "(no group): in none");
            StringAssert.Contains("In no group: Media.", string.Join("|", _window.Editor.GroupWarnings(_window.Config.card_settings.navigation, "en")));
            yield return _window.PressUndo();
            yield return _window.PressUndo();
            CollectionAssert.IsEmpty(_window.Config.card_settings.navigation.families, "two Ctrl+Z: back to no row");
        }

        // The Families table: one row per registered family, in the registered order, named by the visitor's own word
        [UnityTest]
        public IEnumerator TheFamiliesTable_HasARowPerRegisteredFamily_InOrder_NamedByItsVisitorLabelAndItsKey()
        {
            _window = new PoiEditorWindowHost(ShippedConfig(), "_showCardNavigation");
            OpenTab("DetailCard");
            yield return _window.WaitForRepaint();

            var nav = _window.Config.card_settings.navigation;
            var families = _window.Editor.FamiliesInEffectiveOrder(nav);
            CollectionAssert.AreEqual(BlockRegistry.Shared.Families.Select(f => f.Key), families.Select(f => f.Key),
                "with no wall rows the table shows the registered order");
            Assert.Greater(families.Count, 5, "precondition: the registry really handed its families over");

            var name = typeof(POIEditorToolWindow).GetMethod("FamilyRowName", Instance);
            string shown = (string)name.Invoke(_window.Editor, new object[] { families[0] });
            StringAssert.Contains("About", shown, "the row shows the visitor's own word for the family");
            StringAssert.Contains("(about)", shown, "and its key beside it, so the developer can match it to a block kind");
            Assert.IsFalse(_window.Unsaved, "drawing the table writes nothing");
        }

        // The Families table carries ONE (i), on its header row: the nine per-row ones all opened the same help and said nothing about
        // their own family (_3.2 step 5 review). Counted through the (i) renderer itself, so a per-row (i) drawn any other way still counts
        [UnityTest]
        public IEnumerator TheFamiliesTable_HasOneInfoButton_OnItsHeaderRow_AndNoneOnItsRows()
        {
            _window = new PoiEditorWindowHost(ShippedConfig(), "_showCardNavigation");
            OpenTab("DetailCard");
            var drawn = new List<(string Title, Rect Rect)>();
            HelpInfoButton.DrawnProbe = (title, rect) => drawn.Add((title, rect));
            try
            {
                yield return Redraw();
            }
            finally
            {
                HelpInfoButton.DrawnProbe = null;
            }
            Assert.IsNotEmpty(drawn, "precondition: the Navigation section drew its (i) buttons");
            var rowName = typeof(POIEditorToolWindow).GetMethod("FamilyRowName", Instance);
            var familyRows = _window.Editor.FamiliesInEffectiveOrder(_window.Config.card_settings.navigation)
                .Select(f => (string)rowName.Invoke(_window.Editor, new object[] { f })).ToList();
            Assert.AreEqual(9, familyRows.Count, "precondition: the table lists the nine registered families");
            // - one Repaint pass draws each (i) once: the count is per pass
            Assert.AreEqual(1, drawn.Count(d => d.Title == "Families"), "exactly one (i) for the table");
            CollectionAssert.IsEmpty(drawn.Where(d => familyRows.Contains(d.Title)).Select(d => d.Title).ToList(), "no (i) on a family's own row");
            var header = drawn.Single(d => d.Title == "Families");
            var blockOrder = drawn.Single(d => d.Title == "Block Order");
            Assert.AreEqual(blockOrder.Rect.xMin, header.Rect.xMin, 0.5f, "the table's (i) sits in the section's (i) column, under Block Order's");
        }

        // A real click on a family's down arrow swaps it past its neighbour, writes a row for BOTH of them (and for
        // nobody else), and Ctrl+Z takes it back
        [UnityTest]
        public IEnumerator AFamilysReorderArrow_IsARealClick_SwapsItWithItsNeighbour_AndCtrlZTakesItBack()
        {
            _window = new PoiEditorWindowHost(ShippedConfig(), "_showCardNavigation");
            OpenTab("DetailCard");
            yield return _window.WaitForRepaint();

            var nav = _window.Config.card_settings.navigation;
            var before = _window.Editor.FamiliesInEffectiveOrder(nav).Select(f => f.Key).ToList();
            Assert.AreEqual("about", before[0]);
            Assert.AreEqual("media", before[1], "precondition: media follows about");

            yield return _window.ScrollTo("Family down#0");
            _window.Click("Family down#0");
            yield return _window.WaitForRepaint();

            var after = _window.Editor.FamiliesInEffectiveOrder(_window.Config.card_settings.navigation).Select(f => f.Key).ToList();
            Assert.AreEqual("media", after[0], "a real click moved about down past media");
            Assert.AreEqual("about", after[1]);
            CollectionAssert.AreEqual(before.Skip(2), after.Skip(2), "nothing below the two swapped families moved");
            Assert.IsTrue(_window.Unsaved);
            CollectionAssert.AreEquivalent(new[] { "about", "media" },
                _window.Config.card_settings.navigation.families.Select(f => f.family),
                "only the two families the developer really moved got a row");

            yield return _window.PressUndo();
            CollectionAssert.AreEqual(before,
                _window.Editor.FamiliesInEffectiveOrder(_window.Config.card_settings.navigation).Select(f => f.Key).ToList(), "Ctrl+Z");
        }

        // A real click on At The End ticks that family, and the builder really moves its blocks; Ctrl+Z takes it back
        [UnityTest]
        public IEnumerator AFamilysAtTheEndToggle_IsARealClick_ReordersTheCard_AndCtrlZTakesItBack()
        {
            _window = new PoiEditorWindowHost(ShippedConfig(), "_showCardNavigation");
            OpenTab("DetailCard");
            yield return _window.WaitForRepaint();

            var families = _window.Editor.FamiliesInEffectiveOrder(_window.Config.card_settings.navigation);
            int row = families.FindIndex(f => f.Key == BuiltInFamilies.Media);
            Assert.GreaterOrEqual(row, 0, "precondition: the media family is in the table");
            Assert.IsFalse(families[row].AtEnd, "precondition: media does not close the card by default");

            yield return _window.ScrollTo("Family at end#" + row);
            _window.Click("Family at end#" + row);
            yield return _window.WaitForRepaint();

            var written = _window.Config.card_settings.navigation.families.Single(f => f.family == BuiltInFamilies.Media);
            Assert.IsTrue(written.at_end, "a real click ticked At The End for media");
            Assert.IsTrue(_window.Unsaved);

            // - and the row really changes the card: the Lamp's media blocks move behind its other content
            var lamp = _window.Config.pois.Single(p => p.id == "lamp");
            var keys = BlockStackBuilder.Build(lamp, _window.Config.card_settings, BlockRegistry.Shared)
                .Entries.Select(e => e.Definition.Family).ToList();
            int lastMedia = keys.LastIndexOf(BuiltInFamilies.Media);
            int lastAbout = keys.LastIndexOf(BuiltInFamilies.About);
            Assert.Greater(lastMedia, lastAbout, "with At The End on, the media blocks now sit after the About ones");

            yield return _window.PressUndo();
            CollectionAssert.IsEmpty(_window.Config.card_settings.navigation.families, "Ctrl+Z took the whole row back");
        }

        // The Card Container's Peek Next Card row (15.4.6): ON in the shipped wall (a config written before it has no field and reads the
        // default), drawing writes nothing, a real click turns it off, Ctrl+Z puts it back
        [UnityTest]
        public IEnumerator ThePeekNextCardRow_IsDrawnOn_ARealClickTurnsItOff_AndCtrlZTakesItBack()
        {
            _window = new PoiEditorWindowHost(ShippedConfig(), "_showCardContainer");
            OpenTab("DetailCard");
            yield return _window.WaitForRepaint();
            _window.RectOf("Peek Next Card");
            Assert.IsFalse(_window.Unsaved, "drawing the row writes nothing");
            Assert.IsTrue(_window.Config.card_settings.container.peek_next_card, "on in the shipped wall: the tracks show a slice of the next card");

            _window.Click("Peek Next Card");
            yield return _window.WaitForRepaint();
            Assert.IsFalse(_window.Config.card_settings.container.peek_next_card, "a real click turned it off");
            Assert.IsTrue(_window.Unsaved);
            yield return _window.PressUndo();
            Assert.IsTrue(_window.Config.card_settings.container.peek_next_card, "Ctrl+Z");
        }

        // The Card Container's audio rows: Keep Audio Playing and the Android earbud check are toggles a real click edits (Ctrl+Z takes it
        // back); Audio Overlap is a popup, edited through the window's history by EveryCardSettingsField_... and read in the capture
        [UnityTest]
        public IEnumerator TheCardContainerAudioRows_AreDrawn_ARealClickTicksTheAndroidCheck_AndCtrlZ()
        {
            _window = new PoiEditorWindowHost(ShippedConfig(), "_showCardContainer");
            OpenTab("DetailCard");
            yield return _window.WaitForRepaint();
            _window.RectOf("Keep Audio Playing");
            _window.RectOf("Android Earbud Check");
            Assert.IsFalse(_window.Unsaved, "drawing the rows writes nothing");
            Assert.IsFalse(_window.Config.card_settings.container.audio_android_output_poll, "off until a device test asks for it");
            Assert.AreEqual(CardOptions.AudioSwitch, _window.Config.card_settings.container.audio_when_another_starts, "the shipped default");

            _window.Click("Android Earbud Check");
            yield return _window.WaitForRepaint();
            Assert.IsTrue(_window.Config.card_settings.container.audio_android_output_poll, "a real click ticked it");
            Assert.IsTrue(_window.Unsaved);
            yield return _window.PressUndo();
            Assert.IsFalse(_window.Config.card_settings.container.audio_android_output_poll, "Ctrl+Z");
        }

        [UnityTest]
        public IEnumerator ANumberField_ShowsItsDefaultWithoutWriting_ARealSliderClickStoresAValue_AndCtrlZ()
        {
            var config = TwoPoiConfig();
            config.card_settings.languages = new List<string> { "en" };
            var block = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.BeforeAfterKind };
            config.pois[0].card.blocks.Add(block);
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenPoiCardContent("poi_1");
            var foldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor);
            foldouts["poi_1/block_1"] = true;
            yield return _window.WaitForRepaint();
            BlockInstanceData Slider() => _window.Config.pois[0].card.blocks[0];
            var start = BuiltInBlocks.BeforeAfter.Field(BuiltInBlocks.BeforeAfterStartField);
            Assert.IsNull(Slider().fields.Find(f => f.key == start.Key), "drawing writes nothing");
            Assert.AreEqual(0.5f, POIEditorToolWindow.NumberValue(Slider(), start), "the row shows the default");

            // - a real click near the left of the slider's track: the value jumps there
            Rect row = _window.RectOf("Block field start#0");
            _window.ClickAt(new Vector2(row.x + 6f, row.center.y));
            yield return _window.WaitForRepaint();
            var stored = Slider().fields.Find(f => f.key == start.Key);
            Assert.IsNotNull(stored, "a real click wrote the value");
            Assert.Less(stored.number, 0.2f, "near the track's left end: near 0 (all After)");
            Assert.GreaterOrEqual(stored.number, start.NumberMin, "inside the range");
            yield return _window.PressUndo();
            Assert.IsNull(Slider().fields.Find(f => f.key == start.Key), "one Ctrl+Z: back to the default, nothing stored");
        }

        // _3.1 step 7B: a Number INSIDE an Items row (hotspot_image's Across / Down) is the same real slider, per row
        [UnityTest]
        public IEnumerator AHotspotSpotRow_AcrossAndDownAreRealSliders_DrawingNeverWrites_AClickPlacesTheSpot_AndCtrlZ()
        {
            var config = TwoPoiConfig();
            config.card_settings.languages = new List<string> { "en" };
            var block = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.HotspotImageKind };
            var spots = new BlockFieldValue { key = BuiltInBlocks.HotspotItemsField };
            spots.items.Add(new BlockItemData { fields = new List<BlockItemFieldValue>
            {
                new() { key = BuiltInBlocks.HotspotTitleField, text = new List<LocalizedEntry> { new() { lang = "en", value = "The bell" } } },
            } });
            block.fields.Add(spots);
            config.pois[0].card.blocks.Add(block);
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenPoiCardContent("poi_1");
            var foldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor);
            foldouts["poi_1/block_1"] = true;
            yield return _window.WaitForRepaint();
            BlockItemData Spot() => _window.Config.pois[0].card.blocks[0].fields.Single(f => f.key == BuiltInBlocks.HotspotItemsField).items[0];
            var rowFields = BuiltInBlocks.HotspotImage.Field(BuiltInBlocks.HotspotItemsField).ItemFields;
            var across = rowFields.Single(f => f.Key == BuiltInBlocks.HotspotXField);
            var down = rowFields.Single(f => f.Key == BuiltInBlocks.HotspotYField);
            Rect acrossRow = _window.RectOf("Block item spots 0 x#0");
            Rect downRow = _window.RectOf("Block item spots 0 y#0");
            Assert.Greater(downRow.y, acrossRow.y, "Across, then Down, each its own slider row under the spot");
            Assert.IsFalse(_window.Unsaved, "drawing the sliders writes nothing");
            Assert.IsNull(Spot().fields.Find(f => f.key == across.Key), "no Across stored yet");
            Assert.AreEqual(0.5f, BlockFieldReader.ItemNumber(Spot(), across), "the row shows the default: the middle");

            // - a real click near the RIGHT end of the Across track (the slider's number box sits after the track, at the
            //   row's end): the spot moves to the picture's right edge
            _window.ClickAt(new Vector2(acrossRow.xMax - EditorGUIUtility.fieldWidth - 12f, acrossRow.center.y));
            yield return _window.WaitForRepaint();
            var stored = Spot().fields.Find(f => f.key == across.Key);
            Assert.IsNotNull(stored, "a real click wrote Across");
            Assert.Greater(stored.number, 0.8f, "near the track's right end: near 1 (the right edge)");
            Assert.LessOrEqual(stored.number, across.NumberMax);
            Assert.IsNull(Spot().fields.Find(f => f.key == down.Key), "Down is untouched");

            yield return _window.PressUndo();
            Assert.IsNull(Spot().fields.Find(f => f.key == across.Key), "one Ctrl+Z: back to the default, nothing stored");
        }

        [Test]
        public void AStickyActionsBlockWithSeveralButtons_WarnsThatOnlyTheFirstShows_WhateverSetsTheLook()
        {
            var settings = new CardSettings();
            var warning = POIEditorToolWindow.CardBlockWarnings(ActionsWithRows(BuiltInBlocks.ActionsStickyCta, 3), BuiltInBlocks.Actions, settings, null).Single();
            Assert.AreEqual("Sticky shows only the first button: the other 2 rows are not shown. Pick Circles or Pill Row to show every button.", warning);
            StringAssert.Contains("other 1 row is", POIEditorToolWindow.CardBlockWarnings(ActionsWithRows(BuiltInBlocks.ActionsStickyCta, 2), BuiltInBlocks.Actions, settings, null).Single());

            CollectionAssert.IsEmpty(POIEditorToolWindow.CardBlockWarnings(ActionsWithRows(BuiltInBlocks.ActionsStickyCta, 1), BuiltInBlocks.Actions, settings, null), "one button: nothing hidden");
            CollectionAssert.IsEmpty(POIEditorToolWindow.CardBlockWarnings(ActionsWithRows(BuiltInBlocks.ActionsPillRow, 3), BuiltInBlocks.Actions, settings, null), "pill row shows them all");

            // - no look picked on the block: the Block Library's default decides, exactly as the card does
            var libraryBlock = ActionsWithRows("", 3);
            CollectionAssert.IsEmpty(POIEditorToolWindow.CardBlockWarnings(libraryBlock, BuiltInBlocks.Actions, settings, null), "stock default is pill_row");
            settings.kinds.Add(new BlockKindSetting { kind = BuiltInBlocks.ActionsKind, default_variant = BuiltInBlocks.ActionsStickyCta });
            Assert.AreEqual(BuiltInBlocks.ActionsStickyCta, BlockStackBuilder.Build(new POIData { id = "p", card = new POICardData { blocks = { libraryBlock } } }, settings, BlockRegistry.Shared).Entries[1].Variant,
                "precondition: the card really draws it sticky");
            Assert.AreEqual(1, POIEditorToolWindow.CardBlockWarnings(libraryBlock, BuiltInBlocks.Actions, settings, null).Count, "a sticky Block Library default warns too");
            EditorTextChecks.AssertAscii(warning, "the warning");
            foreach (string term in ForbiddenTerms) StringAssert.DoesNotContain(term, warning);
        }

        // 15.2.2 on the real window: a point that offers one action in three places (a Show On Wall block, an Actions circles block and a sticky
        // footer with no words of its own) shows ONE warning under each of the three and none under a block that offers nothing repeated; a real
        // delete of one still leaves two that repeat; a second real delete clears every warning; Ctrl+Z brings the pair back
        [UnityTest]
        public IEnumerator TheRepeatedActionWarning_IsDrawnUnderEveryBlockThatOffersTheAction_RealDeletesClearIt_AndCtrlZBringsItBack()
        {
            var config = TwoPoiConfig();
            config.card_settings.languages = new List<string> { "en" };
            var blocks = config.pois[0].card.blocks;
            blocks.Add(new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.ShowOnWallKind, display = CardOptions.DisplayInline });
            var circles = ActionsWithRows(BuiltInBlocks.ActionsCircles, 1);
            circles.key = "block_2";
            var sticky = ActionsWithRows(BuiltInBlocks.ActionsStickyCta, 1);
            sticky.key = "block_3";
            sticky.fields[0].items[0].fields.RemoveAll(f => f.key == BuiltInBlocks.ActionsLabelField);
            var unrelated = ActionsWithRows(BuiltInBlocks.ActionsPillRow, 1);
            unrelated.key = "block_4";
            unrelated.fields[0].items[0].fields.Find(f => f.key == BuiltInBlocks.ActionsActionField).value = "listen";
            blocks.AddRange(new[] { circles, sticky, unrelated });
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenPoiCardContent("poi_1");

            yield return Redraw();
            Assert.IsTrue(Drawn("Card block delete#3"), "precondition: all four blocks are drawn");
            for (int i = 0; i < 3; i++) Assert.IsTrue(Drawn("Card repeated action warning#" + i), "the warning under block " + i);
            Assert.IsFalse(Drawn("Card repeated action warning#3"), "none under the block that offers nothing repeated");
            Assert.IsFalse(_window.Unsaved, "drawing the warnings writes nothing");

            _window.Click("Card block delete#2");
            yield return Redraw();
            Assert.AreEqual(3, _window.Config.pois[0].card.blocks.Count, "a real click deleted the sticky block");
            Assert.IsTrue(Drawn("Card repeated action warning#0") && Drawn("Card repeated action warning#1"), "two offers are still a repeat");
            Assert.IsFalse(Drawn("Card repeated action warning#2"), "the block that offers nothing repeated has none");

            _window.Click("Card block delete#1");
            yield return Redraw();
            Assert.IsFalse(Drawn("Card repeated action warning#0") || Drawn("Card repeated action warning#1"), "one offer left: nothing repeats");

            yield return _window.PressUndo();
            yield return Redraw();
            Assert.AreEqual(3, _window.Config.pois[0].card.blocks.Count, "Ctrl+Z brought the Actions block back");
            Assert.IsTrue(Drawn("Card repeated action warning#0") && Drawn("Card repeated action warning#1"), "and the pair's warning with it");
        }

        // 15.2.4 on the real window, the block's row: Keep Model On Switch is drawn off by default, drawing writes nothing, a real click ticks it
        // (a stored value of its own), Ctrl+Z takes it back. With the wall's Block Library default ON the same untouched row shows ticked -- a
        // real click on it unticks it, and the block then STORES false (its own value wins over the wall's default)
        [UnityTest]
        public IEnumerator KeepModelOnSwitch_TheBlockRowIsDrawnOff_ARealClickTicksIt_AndUnderALibraryDefaultOnItShowsTicked_ARealClickStoresFalse()
        {
            var config = TwoPoiConfig();
            config.card_settings.languages = new List<string> { "en" };
            var block = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.PlaceInArKind, display = CardOptions.DisplayInline };
            config.pois[0].card.blocks.Add(block);
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenPoiCardContent("poi_1");
            ((Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor))["poi_1/block_1"] = true;
            BlockInstanceData Block() => _window.Config.pois[0].card.blocks[0];
            var field = BuiltInBlocks.PlaceInAr.Field(BuiltInBlocks.PlaceInArKeepOnSwitchField);

            yield return Redraw();
            Assert.IsTrue(Drawn("Block field keep_on_switch#0"), "the row is on the block");
            Assert.IsFalse(_window.Unsaved, "drawing it writes nothing");
            CollectionAssert.IsEmpty(Block().fields, "no value created by drawing");
            Assert.IsFalse(POIEditorToolWindow.FlagValue(Block(), field.Key), "off by default: the model goes with the point");
            StringAssert.Contains("Remove From Room", field.Help);

            _window.Click("Block field keep_on_switch#0");
            yield return _window.WaitForRepaint();
            Assert.IsTrue(POIEditorToolWindow.FlagValue(Block(), field.Key), "a real click ticks it");
            Assert.IsTrue(_window.Unsaved);
            yield return _window.PressUndo();
            CollectionAssert.IsEmpty(Block().fields, "Ctrl+Z: back to nothing stored");

            // - the wall's Block Library default ON: the untouched row shows what the card will read
            Pick(() => ((POIEditorToolWindow)_window.Editor).SetBlockLibraryFieldDefault(BuiltInBlocks.PlaceInArKind, field.Key, BlockLibraryRule.FlagTrue));
            yield return Redraw();
            Assert.IsTrue(BlockLibraryRule.Flag(_window.Config.card_settings, BuiltInBlocks.PlaceInAr, field, new BlockFieldReader(Block(), null, null)), "precondition: the card reads on");
            _window.Click("Block field keep_on_switch#0");
            yield return _window.WaitForRepaint();
            Assert.IsTrue(new BlockFieldReader(Block(), null, null).Stored(field.Key), "the click stored a value of its own");
            Assert.IsFalse(POIEditorToolWindow.FlagValue(Block(), field.Key), "the row showed ticked, so the click unticked it");
            Assert.IsFalse(BlockLibraryRule.Flag(_window.Config.card_settings, BuiltInBlocks.PlaceInAr, field, new BlockFieldReader(Block(), null, null)), "and the block's own off wins over the wall's on");
        }

        // 15.2.4 on the real window, the Block Library: a Default row under Place In AR with a real tick box -- off in the shipped wall, drawing writes
        // nothing, a real click writes "true" for the kind, a second one "false", Ctrl+Z walks back to no row at all
        [UnityTest]
        public IEnumerator KeepModelOnSwitch_TheBlockLibraryHasADefaultRow_ARealClickWritesTrue_AnotherFalse_AndCtrlZ()
        {
            _window = new PoiEditorWindowHost(ShippedConfig(), "_showCardContainer");
            _window.SetWindowField("_showCardBlockLibrary", true);
            OpenTab("DetailCard");
            yield return _window.WaitForRepaint();
            int at = BlockRegistry.Shared.Ordered.ToList().FindIndex(k => k.Key == BuiltInBlocks.PlaceInArKind);
            string probe = "Block Library default place_in_ar.keep_on_switch#" + at;
            var kind = BuiltInBlocks.PlaceInAr;
            var field = kind.Field(BuiltInBlocks.PlaceInArKeepOnSwitchField);
            _window.RectOf(probe);
            Assert.IsFalse(_window.Unsaved, "drawing the row writes nothing");
            Assert.IsFalse(BlockLibraryRule.FlagDefault(_window.Config.card_settings, kind, field), "off in the shipped wall");
            Assert.IsNull(BlockLibraryRule.Row(_window.Config.card_settings, BuiltInBlocks.PlaceInArKind)?.field_defaults.Find(e => e.key == field.Key), "no entry yet");

            // - the row sits far down the table, maybe below the host's real height: scroll it into view like a person, then click
            yield return _window.ScrollTo(probe);
            _window.Click(probe);
            yield return _window.WaitForRepaint();
            Assert.IsTrue(BlockLibraryRule.FlagDefault(_window.Config.card_settings, kind, field), "a real click ticked it");
            Assert.AreEqual(BlockLibraryRule.FlagTrue, BlockLibraryRule.Row(_window.Config.card_settings, BuiltInBlocks.PlaceInArKind).field_defaults.Single(e => e.key == field.Key).value);
            Assert.IsTrue(_window.Unsaved);
            _window.Click(probe);
            yield return _window.WaitForRepaint();
            Assert.IsFalse(BlockLibraryRule.FlagDefault(_window.Config.card_settings, kind, field), "a second click unticked it");
            Assert.AreEqual(BlockLibraryRule.FlagFalse, BlockLibraryRule.Row(_window.Config.card_settings, BuiltInBlocks.PlaceInArKind).field_defaults.Single(e => e.key == field.Key).value);
            yield return _window.PressUndo();
            Assert.IsTrue(BlockLibraryRule.FlagDefault(_window.Config.card_settings, kind, field), "Ctrl+Z: ticked again");
            yield return _window.PressUndo();
            Assert.IsNull(BlockLibraryRule.Row(_window.Config.card_settings, BuiltInBlocks.PlaceInArKind), "Ctrl+Z again: no Library row at all");
            Assert.IsFalse(BlockLibraryRule.FlagDefault(_window.Config.card_settings, kind, field));
        }

        // 15.2.5 on the real window: a Dialogue line holding a Reply 3 with text while Choice 3 is empty. The warning is drawn under the block and names
        // the Reply and the Choice to write; REAL typing into that Choice (the field the warning names, drawn because Choices 1 and 2 have words)
        // clears it, and Ctrl+Z brings it back
        [UnityTest]
        public IEnumerator TheDialogueWarning_NamesTheHiddenReplyAndTheChoiceToFill_RealTypingInThatChoiceClearsIt_AndCtrlZBringsItBack()
        {
            var config = TwoPoiConfig();
            config.card_settings.languages = new List<string> { "en" };
            BlockItemFieldValue Text(string key, string value) => new() { key = key, text = new List<LocalizedEntry> { new() { lang = "en", value = value } } };
            var line = new BlockItemData { fields = new List<BlockItemFieldValue>
            {
                Text(BuiltInBlocks.DialogueTextField, "Welcome."), Text(DialogueRule.ChoiceField(1), "Hello"), Text(DialogueRule.ChoiceField(2), "Bye"),
                Text(DialogueRule.ReplyField(3), "Nobody can pick this."),
            } };
            var dialogue = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.DialogueKind, display = CardOptions.DisplayInline };
            dialogue.fields.Add(new BlockFieldValue { key = BuiltInBlocks.DialogueLinesField, items = { line } });
            config.pois[0].card.blocks.Add(dialogue);
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenPoiCardContent("poi_1");
            ((Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor))["poi_1/block_1"] = true;

            yield return Redraw();
            Assert.IsTrue(Drawn("Card block warning#0"), "the warning is drawn under the dialogue block");
            Assert.IsTrue(Drawn("Block item lines 0 choice_3 en#0"), "Choice 3, the field the warning names, is drawn (Choice 2 has words)");
            Assert.IsTrue(Drawn("Block item lines 0 reply_3 en#0"), "and so is the Reply");
            Assert.IsFalse(_window.Unsaved, "drawing writes nothing");
            Assert.AreEqual("Line 1: Reply 3 has text but Choice 3 is empty: fill Choice 3 to see it.",
                POIEditorToolWindow.CardBlockWarnings(_window.Config.pois[0].card.blocks[0], BuiltInBlocks.Dialogue, _window.Config.card_settings, _window.Config.pois[0]).Single());

            yield return _window.ReplaceText("Block item lines 0 choice_3 en#0", "Maybe");
            yield return _window.ClickAway();
            yield return Redraw();
            Assert.IsFalse(Drawn("Card block warning#0"), "the Choice has words: the reply can be picked, no warning");
            yield return _window.PressUndo();
            yield return Redraw();
            Assert.IsTrue(Drawn("Card block warning#0"), "Ctrl+Z: the Choice is empty again and the warning is back");
        }

        // ---------------- guards ----------------

        [Test]
        public void EveryRegisteredKind_IsFullyEditableHere_WithHelpOnEveryRow()
        {
            Assert.Greater(BlockRegistry.Shared.All.Count, 0);
            foreach (var kind in BlockRegistry.Shared.All)
            {
                Assert.IsNull(BlockRegistry.Validate(kind), kind.Key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(kind.DisplayName), kind.Key + " has a name");
                Assert.IsFalse(string.IsNullOrWhiteSpace(kind.Help), kind.Key + " has a (i) text");
                foreach (var field in kind.Fields)
                {
                    Assert.IsTrue(POIEditorToolWindow.HasBlockFieldDrawer(field.Type), kind.Key + "." + field.Key + ": no Editor drawer for " + field.Type);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(field.Label), kind.Key + "." + field.Key + " has a label");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(field.Help), kind.Key + "." + field.Key + " has a (i) text");
                    EditorTextChecks.AssertAscii(field.Label + field.Help + kind.Help, kind.Key + "." + field.Key);
                    foreach (var sub in field.ItemFields ?? Array.Empty<BlockFieldDefinition>())
                    {
                        Assert.AreNotEqual(BlockFieldType.Items, sub.Type, kind.Key + "." + field.Key + "." + sub.Key + ": items cannot nest");
                        Assert.IsTrue(POIEditorToolWindow.HasBlockFieldDrawer(sub.Type), kind.Key + "." + field.Key + "." + sub.Key + ": no Editor drawer for " + sub.Type);
                        Assert.IsFalse(string.IsNullOrWhiteSpace(sub.Label) || string.IsNullOrWhiteSpace(sub.Help), kind.Key + "." + field.Key + "." + sub.Key + " has a label and a (i) text");
                        EditorTextChecks.AssertAscii(sub.Label + sub.Help, kind.Key + "." + field.Key + "." + sub.Key);
                    }
                }
            }
        }

        private static readonly string[] ForbiddenTerms =
        {
            ".md", ".cs", "_3.", "_5.1", "LivingRoom", "lamp", "Lamp", "painting", "PoiCardHost", "BlockStackBuilder",
            // - "Open Gallery" is a real button of the tab, so the word is allowed; the scene's and the harness's own names are not
            "card_settings", "CardOptions", "Assets/", "CardGallery",
        };

        [Test]
        public void HelpAndGuideTexts_AreAsciiAppAgnostic_AndNameTheEditorTabControls()
        {
            var texts = typeof(POIEditorToolWindow).GetFields(Static)
                .Where(f => f.FieldType == typeof(string) && (f.Name.StartsWith("Card") || f.Name.StartsWith("BlockLibrary"))
                            && (f.Name.EndsWith("Help") || f.Name.EndsWith("Guide") || f.Name.EndsWith("Note")))
                .ToDictionary(f => f.Name, f => (string)f.GetValue(null));
            foreach (string required in new[] { "CardEnabledHelp", "CardLanguagesHelp", "CardOpenAtHelp", "CardContentHelp", "BlockLibraryHelp",
                         "CardSceneTestGuide", "CardPlaymodeTestGuide", "CardDeviceTestGuide", "BlockLibraryPlaymodeTestGuide" })
                Assert.IsTrue(texts.ContainsKey(required), required + " is scanned");
            foreach (var reason in (BlockStackBuilder.SkipReason[])Enum.GetValues(typeof(BlockStackBuilder.SkipReason)))
                foreach (var kind in BlockRegistry.Shared.All)
                    foreach (var field in kind.Fields)
                        texts["skip " + reason + " " + kind.Key + "." + field.Key] = POIEditorToolWindow.CardBlockSkipText(reason, field, kind.NotShownForPoiNote);

            foreach (var pair in texts)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(pair.Value), pair.Key);
                EditorTextChecks.AssertAscii(pair.Value, pair.Key);
                foreach (string term in ForbiddenTerms)
                    StringAssert.DoesNotContain(term, pair.Value, pair.Key + " must not contain '" + term + "'");
            }
            StringAssert.Contains("Not possible in Scene test", texts["CardSceneTestGuide"]);
            StringAssert.Contains("Not possible in Scene test", texts["BlockLibrarySceneTestGuide"]);
            foreach (string control in new[] { "Enable Detail Card", "Languages", "Open At", "Half Height Max", "Tap Outside Closes",
                         "Show demo card", "Demo Point", "Demo Stop", "Open Gallery" })
                StringAssert.Contains(control, texts["CardPlaymodeTestGuide"], "the Playmode guide names " + control);
            StringAssert.Contains("Card edits are live", texts["CardPlaymodeTestGuide"], "the guide says edits reach the running card");
            StringAssert.DoesNotContain("not live yet", texts["CardPlaymodeTestGuide"], "the old 'save, copy and play again' line is gone");
            // - the demo's own texts say it is developer-only, off by default, and how to turn it off (20-code-quality.md)
            StringAssert.Contains("Developer-only", texts["CardDemoShowHelp"]);
            StringAssert.Contains("Off by default", texts["CardDemoShowHelp"]);
            StringAssert.Contains("release build ignores it", texts["CardDemoShowHelp"]);
            StringAssert.Contains("Save All to JSON", texts["CardDemoShowHelp"], "it says how to turn it off");
            // - the Scene guide says the card renders only in Play Mode / UI Builder (no Scene-view parity)
            StringAssert.Contains("Play Mode", texts["CardSceneTestGuide"]);
            StringAssert.Contains("UI Builder", texts["CardSceneTestGuide"]);
            Assert.AreEqual(CardOptions.DemoStops.Length, ((string[])typeof(POIEditorToolWindow).GetField("CardDemoStopLabels", Static).GetValue(null)).Length, "one label per Demo Stop option");
            var openLabels = (string[])typeof(POIEditorToolWindow).GetField("CardOpenStopLabels", Static).GetValue(null);
            Assert.AreEqual(CardOptions.OpenStops.Length, openLabels.Length, "one label per Open At option");
        }

        // ---------------- the developer-only demo card and the gallery (_3.1 step 12) ----------------

        private void InMutationScope(Action edit) =>
            typeof(POIEditorToolWindow).GetMethod("DrawConfigMutationScope", Instance).Invoke(_window.Editor, new object[] { edit, false });

        [UnityTest]
        public IEnumerator ShowDemoCard_ARealClickTurnsItOn_TheChoicesAreStored_TheBuildGuardSeesIt_AndEachEditUndoes()
        {
            _window = new PoiEditorWindowHost(ShippedConfig(), "_showCardContainer");
            OpenTab("DetailCard");
            yield return _window.WaitForRepaint();
            _window.RectOf("Show demo card");
            Assert.IsFalse(_window.Config.card_settings.demo_card.enabled, "off by default on the shipped wall");
            Assert.IsFalse(_window.Unsaved, "drawing the Test rows writes nothing");
            Assert.IsEmpty(DevFeatureBuildGuard.ActiveMessages(_window.Config, developmentBuild: true), "nothing for the build guard to report");

            _window.Click("Show demo card");
            yield return _window.WaitForRepaint();
            var demo = _window.Config.card_settings.demo_card;
            Assert.IsTrue(demo.enabled, "a real click on the checkbox turned it on");
            Assert.IsTrue(_window.Unsaved);
            Assert.AreEqual("", demo.poi_id, "no point picked yet: the row drew without writing one");
            var guard = DevFeatureBuildGuard.ActiveMessages(_window.Config, developmentBuild: true);
            Assert.AreEqual(1, guard.Count, "the switch the window just set is the one the build guard watches");
            StringAssert.Contains("Show demo card", guard[0]);
            Assert.IsEmpty(DevFeatureBuildGuard.ActiveMessages(_window.Config, developmentBuild: false), "a release build ignores it");

            // - the two popups are native menus no test can click: their write path is the window's own edit setters, in its own edit scope
            InMutationScope(() => _window.Editor.SetCardDemoPoi("lamp"));
            InMutationScope(() => _window.Editor.SetCardDemoStop(CardOptions.StopFull));
            yield return _window.WaitForRepaint();
            Assert.AreEqual("lamp", _window.Config.card_settings.demo_card.poi_id);
            Assert.AreEqual(CardOptions.StopFull, _window.Config.card_settings.demo_card.stop);
            Assert.AreEqual("lamp", CardDemoRule.PoiToOpen(_window.Config.card_settings.demo_card, true, _window.Config.pois), "the runtime rule finds the point the popup stored");

            yield return _window.PressUndo();
            Assert.AreEqual(CardOptions.StopHalf, _window.Config.card_settings.demo_card.stop, "Ctrl+Z: the stop is back");
            yield return _window.PressUndo();
            Assert.AreEqual("", _window.Config.card_settings.demo_card.poi_id, "Ctrl+Z: the point is back");
            yield return _window.PressUndo();
            Assert.IsFalse(_window.Config.card_settings.demo_card.enabled, "Ctrl+Z: the demo is off again");
            Assert.IsEmpty(DevFeatureBuildGuard.ActiveMessages(_window.Config, developmentBuild: true));
        }

        [UnityTest]
        public IEnumerator PreviewLanguage_DrawsWithTwoLanguages_StoresOnlyANonDefaultPick_UndoesAndIsSeenByTheBuildGuard()
        {
            var config = ShippedConfig();
            config.wall_id = "preview_language_test_" + Guid.NewGuid().ToString("N");
            var saved = new CardLocalState(new PlayerPrefsCardStateStore(), config.wall_id);
            try
            {
                config.card_settings.languages = new List<string> { "en", "pt" };
                _window = new PoiEditorWindowHost(config, "_showCardContainer");
                OpenTab("DetailCard");
                yield return _window.WaitForRepaint();
                // - a popup row reports no rect to probe (its native menu cannot be clicked either): the capture of the real window shows the row
                _window.RectOf("Card state reset#0");
                Assert.AreEqual("", _window.Config.card_settings.preview_language, "default: the wall's first language, nothing stored");
                Assert.IsFalse(_window.Unsaved, "drawing the row writes nothing");
                Assert.IsEmpty(DevFeatureBuildGuard.ActiveMessages(_window.Config, developmentBuild: true));

                // - a visitor tap saved Portuguese earlier: picking the preview forgets it, or the card would keep showing the tap's pick
                saved.SetLanguage("pt");
                InMutationScope(() => _window.Editor.SetCardPreviewLanguage("pt"));
                yield return _window.WaitForRepaint();
                Assert.AreEqual("pt", _window.Config.card_settings.preview_language, "a non-default pick is stored");
                Assert.AreEqual("", saved.Language(), "the earlier tap's saved language is forgotten");
                Assert.AreEqual("pt", CardLanguageRule.Shown(_window.Config.card_settings.languages, saved.Language(), _window.Config.card_settings.preview_language, true),
                    "the card now opens in the preview language");
                var guard = DevFeatureBuildGuard.ActiveMessages(_window.Config, developmentBuild: true);
                Assert.AreEqual(1, guard.Count, "a preview left in the config is a developer view the build guard watches");
                StringAssert.Contains("Preview Language", guard[0]);
                Assert.IsEmpty(DevFeatureBuildGuard.ActiveMessages(_window.Config, developmentBuild: false), "a release build ignores it");

                InMutationScope(() => _window.Editor.SetCardPreviewLanguage("en"));
                Assert.AreEqual("", _window.Config.card_settings.preview_language, "picking the first language again stores the default, not 'en'");
                Assert.IsEmpty(DevFeatureBuildGuard.ActiveMessages(_window.Config, developmentBuild: true));

                yield return _window.PressUndo();
                Assert.AreEqual("pt", _window.Config.card_settings.preview_language, "Ctrl+Z: the preview is back");
                yield return _window.PressUndo();
                Assert.AreEqual("", _window.Config.card_settings.preview_language, "Ctrl+Z: the default is back");
            }
            finally
            {
                saved.SetLanguage("");
            }
        }

        [Test]
        public void OpenGallery_LoadsInPlayMode_OpensInTheEditorWhenNothingIsUnsaved_AndOtherwiseAsksToSaveFirst()
        {
            Assert.AreEqual(CardGalleryOpener.Plan.LoadInPlayMode, CardGalleryOpener.PlanFor(isPlaying: true, anySceneHasUnsavedChanges: false));
            Assert.AreEqual(CardGalleryOpener.Plan.LoadInPlayMode, CardGalleryOpener.PlanFor(isPlaying: true, anySceneHasUnsavedChanges: true), "Play Mode loads on top: nothing is replaced on disk");
            Assert.AreEqual(CardGalleryOpener.Plan.OpenInEditor, CardGalleryOpener.PlanFor(isPlaying: false, anySceneHasUnsavedChanges: false));
            Assert.AreEqual(CardGalleryOpener.Plan.SaveTheOpenSceneFirst, CardGalleryOpener.PlanFor(isPlaying: false, anySceneHasUnsavedChanges: true), "never over unsaved work");
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<SceneAsset>(CardGalleryOpener.ScenePath), "the gallery scene the button opens exists");
            foreach (var scene in EditorBuildSettings.scenes)
                Assert.AreNotEqual(CardGalleryOpener.ScenePath, scene.path, "the gallery stays out of Build Settings (40-testing.md 4.4)");
        }

        // ---------------- Tier 3 group A: Reset Saved Card State and the Knowledge Check rows (_3.1 step 8A) ----------------

        [UnityTest]
        public IEnumerator ResetSavedCardState_ARealClick_ClearsThisWallsSavedState_AndOnlyThisWalls_NeverTheConfig()
        {
            string wall = "reset_test_" + Guid.NewGuid().ToString("N");
            string otherWall = "reset_test_other_" + Guid.NewGuid().ToString("N");
            var mine = new CardLocalState(new PlayerPrefsCardStateStore(), wall);
            var theirs = new CardLocalState(new PlayerPrefsCardStateStore(), otherWall);
            try
            {
                mine.SetAnswer("poi_1", "block_1", 0, 2);
                mine.SetVote("poi_1", "block_2", 5);
                mine.MarkSeen("poi_1", "block_3");
                theirs.SetAnswer("poi_1", "block_1", 0, 1);
                var config = TwoPoiConfig();
                config.wall_id = wall;
                _window = new PoiEditorWindowHost(config, "_showCardContainer");
                OpenTab("DetailCard");
                yield return _window.WaitForRepaint();
                Assert.AreEqual(2, mine.Answer("poi_1", "block_1", 0), "precondition: this wall has saved state");
                Assert.IsFalse(_window.Unsaved, "drawing the row writes nothing");

                _window.Click("Card state reset#0");
                yield return _window.WaitForRepaint();
                Assert.AreEqual(-1, mine.Answer("poi_1", "block_1", 0), "the answer is forgotten");
                Assert.AreEqual(-1, mine.Vote("poi_1", "block_2"), "the vote too");
                Assert.IsFalse(mine.Seen("poi_1", "block_3"), "and the revealed question");
                Assert.AreEqual(1, theirs.Answer("poi_1", "block_1", 0), "another wall's saved state is untouched");
                Assert.IsFalse(_window.Unsaved, "a reset never changes the config");
                Assert.AreEqual(wall, _window.Config.wall_id);

                _window.Click("Card state reset#0");
                yield return _window.WaitForRepaint();
                Assert.AreEqual(1, theirs.Answer("poi_1", "block_1", 0), "a second click has nothing to clear and harms nothing");
            }
            finally
            {
                mine.ResetAll();
                theirs.ResetAll();
            }
        }

        [Test]
        public void ResetSavedCardState_ReturnsHowManyEntriesWentAndSaysItInAnotherNotice()
        {
            string wall = "reset_test_" + Guid.NewGuid().ToString("N");
            var state = new CardLocalState(new PlayerPrefsCardStateStore(), wall);
            try
            {
                var config = TwoPoiConfig();
                config.wall_id = wall;
                var window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
                try
                {
                    typeof(POIEditorToolWindow).GetField("_config", Instance).SetValue(window, config);
                    var reset = typeof(POIEditorToolWindow).GetMethod("ResetSavedCardState", Instance);
                    Assert.AreEqual(0, (int)reset.Invoke(window, null), "nothing saved: nothing cleared");
                    state.SetAnswer("poi_1", "block_1", 0, 1);
                    state.SetAnswer("poi_1", "block_1", 1, 2);
                    Assert.AreEqual(2, (int)reset.Invoke(window, null), "two saved answers cleared");
                }
                finally { UnityEngine.Object.DestroyImmediate(window); }
            }
            finally { state.ResetAll(); }
        }

        [Test]
        public void KnowledgeCheck_TheEditorNamesEachQuestionRowTheLookWouldLeaveOut_ByItsRowNumberAndTheReason()
        {
            BlockItemFieldValue Text(string key, string value) => new() { key = key, text = new List<LocalizedEntry> { new() { lang = "en", value = value } } };
            BlockItemData Row(params BlockItemFieldValue[] fields) => new() { fields = new List<BlockItemFieldValue>(fields) };
            var block = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.KnowledgeCheckKind, variant = BuiltInBlocks.KnowledgeCheckMultipleChoice };
            var questions = new BlockFieldValue { key = "questions" };
            questions.items.Add(Row(Text("question", "Fine"), Text("explanation", "Because."), Text("option_1", "a"), Text("option_2", "b"), new() { key = "correct", value = "1" }));
            questions.items.Add(Row(Text("question", "No explanation"), Text("option_1", "a"), Text("option_2", "b"), new() { key = "correct", value = "1" }));
            questions.items.Add(Row(Text("question", "One option"), Text("explanation", "Because."), Text("option_1", "a"), new() { key = "correct", value = "1" }));
            questions.items.Add(Row(Text("question", "Right one blank"), Text("explanation", "Because."), Text("option_1", "a"), Text("option_2", "b"), new() { key = "correct", value = "4" }));
            questions.items.Add(Row(Text("explanation", "No question"), Text("option_1", "a"), Text("option_2", "b"), new() { key = "correct", value = "1" }));
            questions.items.Add(Row(Text("question", "No right option"), Text("explanation", "Because."), Text("option_1", "a"), Text("option_2", "b")));
            block.fields.Add(questions);
            var poi = new POIData { id = "poi_1", name = "North Tower" };

            var warnings = POIEditorToolWindow.CardBlockWarnings(block, BuiltInBlocks.KnowledgeCheck, new CardSettings(), poi);
            CollectionAssert.AreEqual(new[]
            {
                "Row 2 of Questions is not shown: its Explanation is empty (every question needs one: it is what the visitor reads after answering).",
                "Row 3 of Questions is not shown: it needs at least two options with words (Option 1 to Option 4).",
                "Row 4 of Questions is not shown: its Right Option is one this look does not show (an option with no words).",
                "Row 5 of Questions is not shown: its Question is empty.",
                "Row 6 of Questions is not shown: no Right Option is picked.",
            }, warnings, "every row the multiple-choice look leaves out, by the number the Items rows show");

            block.variant = BuiltInBlocks.KnowledgeCheckImageChoice;
            var pictures = POIEditorToolWindow.CardBlockWarnings(block, BuiltInBlocks.KnowledgeCheck, new CardSettings(), poi);
            Assert.AreEqual(6, pictures.Count, "no row has a picture: Image Choice leaves every one out");
            StringAssert.Contains("Image Choice needs at least two pictures inside the Media Folder", pictures[0]);

            block.variant = BuiltInBlocks.KnowledgeCheckTrueFalseSwipe;
            var trueFalse = POIEditorToolWindow.CardBlockWarnings(block, BuiltInBlocks.KnowledgeCheck, new CardSettings(), poi);
            CollectionAssert.AreEqual(new[]
            {
                "Row 2 of Questions is not shown: its Explanation is empty (every question needs one: it is what the visitor reads after answering).",
                "Row 5 of Questions is not shown: its Question is empty.",
            }, trueFalse, "true / false needs only a statement and an explanation: options are not its business");

            block.variant = "";
            var byLibrary = new CardSettings();
            byLibrary.kinds.Add(new BlockKindSetting { kind = BuiltInBlocks.KnowledgeCheckKind, default_variant = BuiltInBlocks.KnowledgeCheckTrueFalseSwipe });
            Assert.AreEqual(2, POIEditorToolWindow.CardBlockWarnings(block, BuiltInBlocks.KnowledgeCheck, byLibrary, poi).Count,
                "no look picked: the warnings judge the look the Block Library will give it");
            foreach (string text in warnings.Concat(pictures).Concat(trueFalse))
                EditorTextChecks.AssertAscii(text, "a Knowledge Check warning");
        }

        [UnityTest]
        public IEnumerator KnowledgeCheckRows_RealClicksAddARowAndTypeItsQuestionAndExplanation_AndTheRowSaysWhatIsMissing()
        {
            var config = TwoPoiConfig();
            config.card_settings.languages = new List<string> { "en" };
            config.pois[0].card.blocks.Add(new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.KnowledgeCheckKind, variant = BuiltInBlocks.KnowledgeCheckMultipleChoice });
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenPoiCardContent("poi_1");
            var foldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor);
            foldouts["poi_1/block_1"] = true;
            yield return _window.WaitForRepaint();
            Assert.IsFalse(_window.Unsaved, "drawing an empty Knowledge Check writes nothing");
            Assert.IsEmpty(_window.Config.pois[0].card.blocks[0].fields, "no field value created by drawing");

            _window.Click("Block item questions add#0");
            yield return _window.WaitForRepaint();
            yield return _window.ReplaceText("Block item questions 0 question en#0", "What is a keep?");
            yield return _window.ClickAway();
            // - a question row has twelve fields: its last one (the explanation) lies below the window's first screen, as it does for a
            //   developer, who scrolls; the same here
            _window.SetWindowField("_scrollPos", new Vector2(0f, 4000f));
            yield return _window.WaitForRepaint();
            yield return _window.ReplaceText("Block item questions 0 explanation en#0", "The strongest tower.");
            yield return _window.ClickAway();
            var row = _window.Config.pois[0].card.blocks[0].fields.Single(f => f.key == "questions").items.Single();
            Assert.AreEqual("What is a keep?", POIEditorToolWindow.ItemLocalizedValue(row, "question", "en"), "typed for real");
            Assert.AreEqual("The strongest tower.", POIEditorToolWindow.ItemLocalizedValue(row, "explanation", "en"));
            Assert.IsTrue(_window.Unsaved);

            var warnings = POIEditorToolWindow.CardBlockWarnings(_window.Config.pois[0].card.blocks[0], BuiltInBlocks.KnowledgeCheck, _window.Config.card_settings, _window.Config.pois[0]);
            Assert.AreEqual(1, warnings.Count, "the typed row still lacks options");
            StringAssert.StartsWith("Row 1 of Questions is not shown: it needs at least two options", warnings[0]);

            yield return _window.PressUndo();
            yield return _window.PressUndo();
            yield return _window.PressUndo();
            yield return _window.PressUndo();
            Assert.IsEmpty(_window.Config.pois[0].card.blocks[0].fields.Where(f => f.items.Count > 0).ToList(), "each real edit undoes: back to no rows");
        }
    }
}
