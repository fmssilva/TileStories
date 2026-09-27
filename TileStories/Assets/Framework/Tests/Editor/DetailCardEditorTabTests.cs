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
                else if (f.FieldType == typeof(CardContainerSettings))
                    foreach (var c in typeof(CardContainerSettings).GetFields(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (c.FieldType == typeof(bool)) edits.Add((c.Name, s => c.SetValue(s.container, !(bool)c.GetValue(s.container))));
                        else if (c.FieldType == typeof(float)) edits.Add((c.Name, s => c.SetValue(s.container, 0.3f)));
                        else if (c.FieldType == typeof(string)) edits.Add((c.Name, s => c.SetValue(s.container, CardOptions.StopHalf)));
                        else Assert.Fail("no edit for container field " + c.Name);
                    }
                else Assert.Fail("no edit for card_settings field " + f.Name);
            }
            Assert.AreEqual(6 + 4, edits.Count, "every card_settings field (walked by reflection) has an edit: 6 wall-level + 4 container");

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
            Assert.AreEqual("Sair", new CardStrings(framework, s.strings, "pt", "en").Get(CardStrings.Keys.Close), "the card reads it");
            Assert.AreEqual("Close", new CardStrings(framework, s.strings, "en", "en").Get(CardStrings.Keys.Close));

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
            Assert.IsTrue(warning.All(c => c < 128), "ASCII only");
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
                    Assert.IsTrue((field.Label + field.Help + kind.Help).All(c => c < 128), kind.Key + "." + field.Key + ": ASCII only");
                    foreach (var sub in field.ItemFields ?? Array.Empty<BlockFieldDefinition>())
                    {
                        Assert.AreNotEqual(BlockFieldType.Items, sub.Type, kind.Key + "." + field.Key + "." + sub.Key + ": items cannot nest");
                        Assert.IsTrue(POIEditorToolWindow.HasBlockFieldDrawer(sub.Type), kind.Key + "." + field.Key + "." + sub.Key + ": no Editor drawer for " + sub.Type);
                        Assert.IsFalse(string.IsNullOrWhiteSpace(sub.Label) || string.IsNullOrWhiteSpace(sub.Help), kind.Key + "." + field.Key + "." + sub.Key + " has a label and a (i) text");
                        Assert.IsTrue((sub.Label + sub.Help).All(c => c < 128), kind.Key + "." + field.Key + "." + sub.Key + ": ASCII only");
                    }
                }
            }
        }

        private static readonly string[] ForbiddenTerms =
        {
            ".md", ".cs", "_3.", "_5.1", "LivingRoom", "lamp", "Lamp", "painting", "PoiCardHost", "BlockStackBuilder",
            "card_settings", "CardOptions", "Assets/", "Gallery",
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
                Assert.IsTrue(pair.Value.All(c => c < 128), pair.Key + " must be ASCII only");
                foreach (string term in ForbiddenTerms)
                    StringAssert.DoesNotContain(term, pair.Value, pair.Key + " must not contain '" + term + "'");
            }
            StringAssert.Contains("Not possible in Scene test", texts["CardSceneTestGuide"]);
            StringAssert.Contains("Not possible in Scene test", texts["BlockLibrarySceneTestGuide"]);
            foreach (string control in new[] { "Enable Detail Card", "Languages", "Open At", "Half Height Max", "Tap Outside Closes" })
                StringAssert.Contains(control, texts["CardPlaymodeTestGuide"], "the Playmode guide names " + control);
            StringAssert.Contains("not live yet", texts["CardPlaymodeTestGuide"], "the guide says edits need Save + Copy + Play");
            var openLabels = (string[])typeof(POIEditorToolWindow).GetField("CardOpenStopLabels", Static).GetValue(null);
            Assert.AreEqual(CardOptions.OpenStops.Length, openLabels.Length, "one label per Open At option");
        }
    }
}
