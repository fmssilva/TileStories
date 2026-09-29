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
            Assert.AreEqual(6 + 6 + 3, edits.Count, "every card_settings field (walked by reflection) has an edit: 6 wall-level + 6 container + 3 demo card");

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
            yield return _window.ReplaceText("Block field title en#1", "Gate");
            yield return _window.ClickAway();
            skipped = BlockStackBuilder.Build(_window.Config.pois[0], _window.Config.card_settings, BlockRegistry.Shared).Skipped.Single();
            Assert.AreEqual(BlockStackBuilder.SkipReason.ExtraHeader, skipped.Reason);
            StringAssert.Contains("one Header", POIEditorToolWindow.CardBlockSkipText(skipped.Reason, null), "the row says why");

            _window.Click("Card block down#0");
            yield return _window.WaitForRepaint();
            CollectionAssert.AreEqual(new[] { "block_2", "block_1" }, _window.Config.pois[0].card.blocks.Select(b => b.key), "a real click moves it down");

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
