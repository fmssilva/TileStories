using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TileStories.Tests;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Editor.Tests
{
    // DetailCardEditorTabTests's shared ground: its set-up and helpers, so its own tests and its old-navigation ones stand on one fixture
    public abstract class DetailCardEditorTabFixture
    {
        protected const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;

        protected const BindingFlags Static = BindingFlags.NonPublic | BindingFlags.Static;

        protected PoiEditorWindowHost _window;

        [TearDown]
        public void TearDown() => _window?.Close();

        // - the shipped wall with the framework navigation: the Navigation rows are measured on their defaults, never on the demo
        //   wall's own preset
        protected static WallConfigData ShippedConfig() => ShippedWall.WithFrameworkNavigation();

        protected static WallConfigData TwoPoiConfig()
        {
            var config = new WallConfigData { wall_id = "t" };
            config.pois.Add(new POIData { id = "poi_1", name = "North Tower", summary = "Built in 1640." });
            config.pois.Add(new POIData { id = "poi_2", name = "South Gate" });
            return config;
        }

        protected void OpenTab(string tab)
        {
            var tabField = typeof(POIEditorToolWindow).GetField("_selectedTab", Instance);
            _window.SetWindowField("_selectedTab", Enum.Parse(tabField.FieldType, tab));
        }

        protected void OpenPoiCardContent(string poiId)
        {
            OpenTab("SpecificMarker");
            _window.SetWindowField("_showPoiCardContent", true);
            var foldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_poiFoldouts", Instance).GetValue(_window.Editor);
            foldouts[poiId] = true;
        }

        // ---------------- P4.4b: the two modes and the block frame's rows ----------------

        // Every (i) the next repaint draws, by its title
        protected IEnumerator HelpTitlesDrawn(List<(string Title, Rect Rect)> drawn)
        {
            drawn.Clear();
            HelpInfoButton.DrawnProbe = (title, rect) => drawn.Add((title, rect));
            try { yield return _window.WaitForRepaint(); }
            finally { HelpInfoButton.DrawnProbe = null; }
        }

        protected List<BlockItemData> Sections() =>
            _window.Config.pois[0].card.blocks[0].fields.Single(f => f.key == BuiltInBlocks.RichTextSectionsField).items;

        protected IEnumerable<string> Titles() => Sections().Select(s => POIEditorToolWindow.ItemLocalizedValue(s, "title", "en"));

        // ---------------- field visibility (_3.1 step 15.1) ----------------

        // Every probe this repaint drew, from scratch: a row that is no longer drawn must not be found in an older repaint's rects
        protected IEnumerator Redraw()
        {
            _window.ScreenRects.Clear();
            yield return _window.WaitForRepaint();
        }

        protected bool Drawn(string probeKey) => _window.ScreenRects.ContainsKey(probeKey);

        // A pick in a popup, through the window's own mutation scope (the native popup menu cannot be clicked; a real pick runs the same setter)
        protected void Pick(Action edit) =>
            typeof(POIEditorToolWindow).GetMethod("DrawConfigMutationScope", Instance).Invoke(_window.Editor, new object[] { edit, false });

        // ---------------- collapsed-row summary (_3.1 step 15.1; its words: BlockSummaryRule, CompactRowRuleTests) ----------------

        protected static BlockFieldValue Text(string key, params (string lang, string value)[] texts) =>
            new() { key = key, text = texts.Select(t => new LocalizedEntry { lang = t.lang, value = t.value }).ToList() };

        // ---------------- BlockRef fields (_3.2 step 4) ----------------

        // A text block of poi_1's card with `words` (its Card Content summary)
        protected static BlockInstanceData TextBlock(string key, string words)
        {
            var block = new BlockInstanceData { key = key, kind = BuiltInBlocks.RichTextKind, variant = BuiltInBlocks.RichTextPlain, display = CardOptions.DisplayInline };
            POIEditorToolWindow.SetLocalizedValue(block, BuiltInBlocks.RichTextBodyField, "en", words);
            return block;
        }

        // An Actions row that opens `target`
        protected static BlockItemData OpenBlockRow(string target)
        {
            var row = new BlockItemData();
            POIEditorToolWindow.SetItemChoiceValue(row, BuiltInBlocks.ActionsActionField, BuiltInBlocks.ActionOpenBlock);
            POIEditorToolWindow.SetItemChoiceValue(row, BuiltInBlocks.ActionsTargetField, target);
            return row;
        }

        protected BlockItemData ActionsRowOfPoi1(int row) =>
            _window.Config.pois[0].card.blocks[1].fields.Single(f => f.key == BuiltInBlocks.ActionsItemsField).items[row];

        protected List<BlockItemData> Swatches() =>
            _window.Config.pois[0].card.blocks[0].fields.Single(f => f.key == BuiltInBlocks.SwatchesItemsField).items;

        // ---------------- warnings on blocks that show ----------------

        protected static BlockInstanceData ActionsWithRows(string variant, int rows)
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
        protected IEnumerator DropOnto(string probeKey, UnityEngine.Object file)
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

        protected const string MediaFolder = "LivingRoom/CardMedia";

        // The (i) titles one Repaint of the Navigation section draws, top to bottom (each row draws its (i) on its own line)
        protected IEnumerator DrawnInfoTitles(List<(string Title, Rect Rect)> into)
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

        protected IEnumerator NavKindHasABlockLibraryRowAndPlusAddBlock(string kind, bool shownWithNoField = true)
        {
            _window = new PoiEditorWindowHost(TwoPoiConfig(), "_showCardBlockLibrary");
            OpenTab("DetailCard");
            yield return _window.WaitForRepaint();
            var ordered = BlockRegistry.Shared.Ordered.ToList();
            int row = ordered.FindIndex(k => k.Key == kind);
            Assert.GreaterOrEqual(row, 0, "the kind is in the registry the window reads");
            Assert.AreEqual(BuiltInFamilies.Navigation, ordered[row].Family);
            int afterFamily = ordered.FindIndex(row, k => k.Family != BuiltInFamilies.Navigation);
            Assert.AreEqual(BuiltInFamilies.Meta, ordered[afterFamily].Family, "the Block Library lists the navigation run between visit and the Sources (Order 85)");
            Assert.AreEqual(BuiltInFamilies.Visit, ordered[ordered.FindIndex(k => k.Family == BuiltInFamilies.Navigation) - 1].Family);
            yield return _window.ScrollTo("Block Library enabled#" + row);
            _window.Click("Block Library enabled#" + row);
            yield return _window.WaitForRepaint();
            var setting = _window.Config.card_settings.kinds.Single();
            Assert.AreEqual(kind, setting.kind, "a real click on its Enabled box made ITS row");
            Assert.IsFalse(setting.enabled);
            yield return _window.PressUndo();
            CollectionAssert.IsEmpty(_window.Config.card_settings.kinds, "Ctrl+Z takes it back");

            OpenTab("SpecificMarker");
            _window.SetWindowField("_showPoiCardContent", true);
            var foldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_poiFoldouts", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_window.Editor);
            foldouts[_window.Config.pois[0].id] = true;
            typeof(POIEditorToolWindow).GetField("_newCardBlockKindIndex", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_window.Editor, row);
            yield return _window.WaitForRepaint();
            yield return _window.ScrollTo("Card add block#0");
            int before = _window.Config.pois[0].card.blocks.Count;
            _window.Click("Card add block#0");
            yield return _window.WaitForRepaint();
            var added = _window.Config.pois[0].card.blocks.Last();
            Assert.AreEqual(before + 1, _window.Config.pois[0].card.blocks.Count);
            Assert.AreEqual(kind, added.kind, "a real click on + Add block added the kind");
            StringAssert.StartsWith(TaxonomyRowKeys.BlockPrefix, added.key, "an authored key: block_N");
            var built = BlockStackBuilder.Build(_window.Config.pois[0], _window.Config.card_settings, BlockRegistry.Shared, _window.Config.pois);
            Assert.AreEqual(shownWithNoField, built.Entries.Any(e => e.Instance == added),
                shownWithNoField ? "no field to fill: the card shows it" : "a Required field is empty: the card leaves the block out");
            yield return _window.PressUndo();
            Assert.AreEqual(before, _window.Config.pois[0].card.blocks.Count, "Ctrl+Z takes it back");
        }

        protected static readonly string[] ForbiddenTerms =
        {
            ".md", ".cs", "_3.", "_5.1", "LivingRoom", "lamp", "Lamp", "painting", "PoiCardHost", "BlockStackBuilder",
            // - "Open Gallery" is a real button of the tab, so the word is allowed; the scene's and the harness's own names are not
            "card_settings", "CardOptions", "Assets/", "CardGallery",
        };

        // ---------------- the developer-only demo card and the gallery (_3.1 step 12) ----------------

        protected void InMutationScope(Action edit) =>
            typeof(POIEditorToolWindow).GetMethod("DrawConfigMutationScope", Instance).Invoke(_window.Editor, new object[] { edit, false });
    }

    // The POI Editor's third tab, "Detail Card", and Specific Marker > "Card Content" (_3.1 section 8), on the REAL
    // window (PoiEditorWindowHost): real clicks, real typing, real Ctrl+Z / Ctrl+Y through the window's own config
    // history. Every card_settings field is walked by reflection; the texts and the field drawers are guarded so a
    // new kind or a new field cannot slip in without help or without an Editor row.
    public class DetailCardEditorTabTests : DetailCardEditorTabFixture
    {
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
                else if (f.FieldType == typeof(string)) edits.Add((f.Name, s => f.SetValue(s, f.Name == "default_mode" ? CardOptions.ModeDark : "Wall/CardMedia")));
                else if (f.FieldType == typeof(List<string>)) edits.Add((f.Name, s => f.SetValue(s, new List<string> { "pt", "en", "es" })));
                else if (f.FieldType == typeof(List<BlockKindSetting>)) edits.Add((f.Name, s => s.kinds.Add(new BlockKindSetting { kind = "header", default_variant = "compact" })));
                else if (f.FieldType == typeof(List<CardStringEntry>)) edits.Add((f.Name, s => POIEditorToolWindow.SetCardTextOverride(s, CardStrings.Keys.Close, "pt", "Sair")));
                else if (f.FieldType == typeof(List<GlossaryEntry>)) edits.Add((f.Name, s => s.glossary.Add(new GlossaryEntry { term = "keep" })));
                else if (f.FieldType == typeof(List<BlockTemplateData>))
                    edits.Add((f.Name, s => s.block_templates.Add(new BlockTemplateData { key = "template_9", name = "Kept", block = new BlockInstanceData { kind = "story" } })));
                else if (f.FieldType == typeof(CardComponentSettings))
                    edits.Add((f.Name, s => s.components.open_row_emphasis = ElementOptions.EmphasisShine));
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
                                "top" => BuiltInNavElements.KindChips,
                                "bottom" => BuiltInNavElements.BarSheet,
                                "menu" => BuiltInNavElements.DrawerTree,
                                "placed_model_control" => CardOptions.PlacedModelNone,
                                "show_at" => CardOptions.ShowNavFullOnly,
                                "hide_on_scroll" => CardOptions.HideOnScrollNever,
                                "labels" => CardOptions.LabelsIcon,
                                "layout" => CardOptions.LayoutAccordion,
                                _ => CardOptions.BlockOrderByFamily,
                            })));
                        else if (n.FieldType == typeof(bool)) edits.Add((n.Name, s => n.SetValue(s.navigation, !(bool)n.GetValue(s.navigation))));
                        else if (n.FieldType == typeof(int)) edits.Add((n.Name, s => n.SetValue(s.navigation, (int)n.GetValue(s.navigation) - 1)));
                        else if (n.FieldType == typeof(List<CardFamilySetting>))
                            edits.Add((n.Name, s => s.navigation.families.Add(new CardFamilySetting { family = BuiltInFamilies.Meta, order = 15, at_end = false })));
                        else if (n.FieldType == typeof(CardNavBarSettings))
                            edits.Add((n.Name, s => s.navigation.bar.indicator = CardOptions.NavBarIndicatorUnderline));
                        else Assert.Fail("no edit for navigation field " + n.Name);
                    }
                else if (f.FieldType == typeof(CardContainerSettings))
                    foreach (var c in typeof(CardContainerSettings).GetFields(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (c.FieldType == typeof(bool)) edits.Add((c.Name, s => c.SetValue(s.container, !(bool)c.GetValue(s.container))));
                        else if (c.FieldType == typeof(float)) edits.Add((c.Name, s => c.SetValue(s.container, 0.3f)));
                        else if (c.FieldType == typeof(string)) edits.Add((c.Name, s => c.SetValue(s.container, c.Name switch
                        {
                            // - another stop than the wall's own (the shipped wall may open at either)
                            "open_stop" => (string)c.GetValue(s.container) == CardOptions.StopHalf ? CardOptions.StopPeek : CardOptions.StopHalf,
                            "full_height" => CardOptions.FullHeightCameraStrip,
                            "top_band" => CardOptions.TopBandGlass,
                            "blocks" => CardOptions.BlocksCards,
                            _ => CardOptions.AudioQueue,
                        })));
                        else Assert.Fail("no edit for container field " + c.Name);
                    }
                else if (f.FieldType == typeof(CardSectionSettings))
                    edits.Add((f.Name, s => s.sections.start = CardOptions.SectionStartWave));
                else if (f.FieldType == typeof(CardLocateSettings))
                    edits.Add((f.Name, s => s.locate.style = CardOptions.LocateGlassPing));
                else Assert.Fail("no edit for card_settings field " + f.Name);
            }
            Assert.AreEqual(12 + 1 + 11 + 20 + 3, edits.Count,
                "every card_settings field (walked by reflection) has an edit: 12 wall-level (sections the 11th, locate the 12th) + 1 components + 11 container (blocks the 11th) + 20 navigation (bar the 20th) + 3 demo card");

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
            var camera = _window.Config.pois.Single(p => p.id == "camera");
            Assert.AreEqual(BuiltInBlocks.HeaderCompact, BlockStackBuilder.Build(camera, _window.Config.card_settings, BlockRegistry.Shared).Entries[0].Variant,
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

        // _3.3 P4.4a: a header in the Image Parallax look draws its Hero Motion row with its (i) on the real window (the generic drawer, from
        // the kind's definition), after its Picture and only for that look; drawing it writes nothing
        [UnityTest]
        public IEnumerator TheHeroMotionRow_ShowsForImageParallaxOnly_WithItsHelp()
        {
            var config = TwoPoiConfig();
            config.card_settings.languages = new List<string> { "en" };
            config.pois[0].card.blocks.Add(new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.HeaderKind, variant = BuiltInBlocks.HeaderImageParallax });
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenPoiCardContent("poi_1");
            var foldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor);
            foldouts["poi_1/block_1"] = true;
            yield return _window.WaitForRepaint();

            var drawn = new List<(string Title, Rect Rect)>();
            HelpInfoButton.DrawnProbe = (title, rect) => drawn.Add((title, rect));
            try { yield return _window.WaitForRepaint(); }
            finally { HelpInfoButton.DrawnProbe = null; }
            var titles = drawn.Select(d => d.Title).ToList();
            CollectionAssert.Contains(titles, "Hero Motion", "the row and its (i) for Image Parallax");
            CollectionAssert.Contains(titles, "Eyebrow");
            CollectionAssert.DoesNotContain(titles, "Show Level", "the chip's Show Level is gone");
            Assert.Less(drawn.First(d => d.Title == "Picture").Rect.y, drawn.First(d => d.Title == "Hero Motion").Rect.y, "after the Picture");
            Assert.IsFalse(_window.Unsaved, "drawing the rows writes nothing");
            EditorTextChecks.AssertAscii(BuiltInBlocks.Header.Field(BuiltInBlocks.HeaderHeroMotionField).Help, "Hero Motion help");

            _window.Config.pois[0].card.blocks[0].variant = BuiltInBlocks.HeaderTextOnly;
            drawn.Clear();
            HelpInfoButton.DrawnProbe = (title, rect) => drawn.Add((title, rect));
            try { yield return _window.WaitForRepaint(); }
            finally { HelpInfoButton.DrawnProbe = null; }
            CollectionAssert.DoesNotContain(drawn.Select(d => d.Title).ToList(), "Hero Motion", "Text Only: no picture, no Hero Motion");
        }

        // Detail Card > Card Container on the REAL window: the Default Mode row and its (i) are drawn, drawing writes nothing, a pick of Dark
        // (the popup's own setter, inside the window's mutation scope) is an unsaved edit, and Ctrl+Z gives Light back
        [UnityTest]
        public IEnumerator TheDefaultModeRow_IsDrawnWithItsHelp_APickIsAnEdit_AndCtrlZUndoesIt()
        {
            var config = TwoPoiConfig();
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenTab("DetailCard");
            yield return _window.WaitForRepaint();
            var drawn = new List<(string Title, Rect Rect)>();
            yield return HelpTitlesDrawn(drawn);
            CollectionAssert.Contains(drawn.Select(d => d.Title).ToList(), "Default Mode", "Card Container draws the Default Mode row with its (i)");
            Assert.IsFalse(_window.Unsaved, "drawing the row writes nothing");
            Assert.AreEqual(CardOptions.ModeLight, _window.Config.card_settings.default_mode, "a new wall opens light (D1)");
            CollectionAssert.AreEqual(new[] { "Light", "Dark" }, CardOptions.ModeLabels, "the popup names the modes, never their keys");

            Pick(() => _window.Config.card_settings.default_mode = CardOptions.ModeDark);
            Assert.AreEqual(CardOptions.ModeDark, _window.Config.card_settings.default_mode);
            Assert.IsTrue(_window.Unsaved, "the pick is an edit");
            yield return _window.PressUndo();
            Assert.AreEqual(CardOptions.ModeLight, _window.Config.card_settings.default_mode, "Ctrl+Z: light again");
        }

        // Detail Card > Card Container on the REAL window (_3.3 S4a): the Full Height row and its (i) are drawn, drawing writes nothing, a pick of
        // Leave A Camera Strip is an edit, Ctrl+Z gives To The Top back
        [UnityTest]
        public IEnumerator TheFullHeightRow_IsDrawnWithItsHelp_APickIsAnEdit_AndCtrlZUndoesIt()
        {
            var config = TwoPoiConfig();
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenTab("DetailCard");
            yield return _window.WaitForRepaint();
            var drawn = new List<(string Title, Rect Rect)>();
            yield return HelpTitlesDrawn(drawn);
            CollectionAssert.Contains(drawn.Select(d => d.Title).ToList(), "Full Height", "Card Container draws the Full Height row with its (i)");
            Assert.IsFalse(_window.Unsaved, "drawing the row writes nothing");
            Assert.AreEqual(CardOptions.FullHeightTop, _window.Config.card_settings.container.full_height, "a new wall rises to the top");

            Pick(() => _window.Config.card_settings.container.full_height = CardOptions.FullHeightCameraStrip);
            Assert.AreEqual(CardOptions.FullHeightCameraStrip, _window.Config.card_settings.container.full_height);
            Assert.IsTrue(_window.Unsaved, "the pick is an edit");
            yield return _window.PressUndo();
            Assert.AreEqual(CardOptions.FullHeightTop, _window.Config.card_settings.container.full_height, "Ctrl+Z: To The Top again");
        }

        // Specific Marker > Card Content on the REAL window: an open block draws the Emphasis row with its (i) after its Display; a pick of
        // Quiet is an edit the block keeps, Ctrl+Z gives Normal back
        [UnityTest]
        public IEnumerator TheEmphasisRow_IsDrawnOnAnOpenBlock_APickIsAnEdit_AndCtrlZUndoesIt()
        {
            var config = TwoPoiConfig();
            config.card_settings.languages = new List<string> { "en" };
            config.pois[0].card.blocks.Add(new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.SourcesKind, variant = BuiltInBlocks.SourcesWithConfidence });
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenPoiCardContent("poi_1");
            ((Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor))["poi_1/block_1"] = true;
            yield return _window.WaitForRepaint();
            var drawn = new List<(string Title, Rect Rect)>();
            yield return HelpTitlesDrawn(drawn);
            CollectionAssert.Contains(drawn.Select(d => d.Title).ToList(), "Emphasis", "the open block draws its Emphasis row with its (i)");
            Assert.IsFalse(_window.Unsaved, "drawing the row writes nothing");
            Assert.AreEqual(CardOptions.EmphasisNormal, BlockFrame.EmphasisOf(_window.Config.pois[0].card.blocks[0].emphasis), "a new block is normal");
            Assert.AreEqual(CardOptions.Emphases.Length, CardOptions.EmphasisLabels.Length, "a label per option");

            Pick(() => _window.Config.pois[0].card.blocks[0].emphasis = CardOptions.EmphasisQuiet);
            Assert.AreEqual(CardOptions.EmphasisQuiet, _window.Config.pois[0].card.blocks[0].emphasis);
            Assert.IsTrue(_window.Unsaved, "the pick is an edit");
            yield return _window.PressUndo();
            Assert.AreEqual(CardOptions.EmphasisNormal, BlockFrame.EmphasisOf(_window.Config.pois[0].card.blocks[0].emphasis), "Ctrl+Z: normal again");
        }

        // P4.4b part 2 on the REAL window: a picture header whose title needs 3+ lines on a phone draws the notice under the block; REAL typing
        // of a shorter title clears it, and Ctrl+Z brings it back
        [UnityTest]
        public IEnumerator AHeroTitleOfThreeLines_DrawsTheNotice_RealTypingOfAShorterTitleClearsIt_AndCtrlZBringsItBack()
        {
            const string longTitle = "The Royal Ribeira Palace and its Great Earthquake Ruins";
            Assert.GreaterOrEqual(HeroTitleMeasure.Lines(longTitle), HeroTitleFitRule.NoticeAtLines, "precondition: the title needs 3+ lines in the real face");
            var config = TwoPoiConfig();
            config.card_settings.languages = new List<string> { "en" };
            var header = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.HeaderKind, variant = BuiltInBlocks.HeaderImageParallax };
            header.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderImageField, asset = "images/castle_hero.png" });
            header.fields.Add(new BlockFieldValue { key = BlockStackBuilder.HeaderTitleField, text = new List<LocalizedEntry> { new() { lang = "en", value = longTitle } } });
            config.pois[0].card.blocks.Add(header);
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenPoiCardContent("poi_1");
            ((Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor))["poi_1/block_1"] = true;

            yield return Redraw();
            Assert.IsTrue(Drawn("Card block warning#0"), "the notice is drawn under the header");
            Assert.IsFalse(_window.Unsaved, "drawing writes nothing");
            yield return _window.ReplaceText("Block field title en#0", "The Lamp");
            yield return _window.ClickAway();
            yield return Redraw();
            Assert.AreEqual("The Lamp", new BlockFieldReader(_window.Config.pois[0].card.blocks[0], "en", "en").Text(BlockStackBuilder.HeaderTitleField), "real typing stored the title");
            Assert.IsFalse(Drawn("Card block warning#0"), "one line: no notice");
            yield return _window.PressUndo();
            yield return Redraw();
            Assert.IsTrue(Drawn("Card block warning#0"), "Ctrl+Z: the long title and its notice are back");
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

        // ---------------- the Display popup (_3.2 step 12: Expandable for every kind) ----------------

        // Card Content on The Lamp in the real window: every block row but the Header's draws the Display popup (the header keeps the column's
        // place, empty); the popup names its options; a pick of Expandable is an edit the card's builder draws as a compact row, Ctrl+Z
        // undoes it; the (i) explains the three displays in ASCII
        [UnityTest]
        public IEnumerator TheDisplayPopup_IsOnEveryRowButTheHeaders_NamesItsOptions_AndExpandableIsUndoneByCtrlZ()
        {
            _window = new PoiEditorWindowHost(ShippedConfig(), "_showCardContainer");
            OpenPoiCardContent("lamp");
            yield return Redraw();
            var blocks = _window.Config.pois.Single(p => p.id == "lamp").card.blocks;
            Assert.AreEqual(BuiltInBlocks.HeaderKind, blocks[0].kind, "precondition: The Lamp's first row is its header");
            Assert.IsFalse(Drawn("Card block display#0"), "the header has one display: no popup");
            for (int i = 1; i < blocks.Count; i++)
                Assert.IsTrue(Drawn("Card block display#" + i), blocks[i].key + " (" + blocks[i].kind + "): its row offers Expandable");
            Assert.IsFalse(_window.Unsaved, "drawing the rows writes nothing");

            var story = blocks.Single(b => b.key == "block_2");
            Assert.AreEqual(CardOptions.DisplayInline, story.display, "precondition: the shipped story is inline");
            Pick(() => _window.Config.pois.Single(p => p.id == "lamp").card.blocks.Single(b => b.key == "block_2").display = CardOptions.DisplayExpandable);
            yield return Redraw();
            Assert.IsTrue(_window.Unsaved, "a pick is an edit");
            var lamp = _window.Config.pois.Single(p => p.id == "lamp");
            var entry = BlockStackBuilder.Build(lamp, _window.Config.card_settings, BlockRegistry.Shared, _window.Config.pois).Entries.Single(e => e.Instance.key == "block_2");
            Assert.IsTrue(BlockDisplayRule.ShowsCompact(entry), "the card now draws The story as a compact row");
            yield return _window.PressUndo();
            Assert.AreEqual(CardOptions.DisplayInline, _window.Config.pois.Single(p => p.id == "lamp").card.blocks.Single(b => b.key == "block_2").display,
                "Ctrl+Z brings Inline back");

            string help = (string)typeof(POIEditorToolWindow).GetField("CardContentHelp", Static).GetValue(null);
            foreach (string word in new[] { "Inline shows", "Expandable shows", "full view", "nothing of it", "Takeover (Video, 3D Model, 360 Panorama)" })
                StringAssert.Contains(word, help, "the Card Content (i) says " + word);
            EditorTextChecks.AssertAscii(help, "CardContentHelp");
            string guide = (string)typeof(POIEditorToolWindow).GetField("CardNavigationPlaymodeTestGuide", Static).GetValue(null);
            StringAssert.Contains("EXPANDABLE BLOCKS AND THE FULL VIEW", guide, "the Playmode guide walks a row, its opening and the full view");
            StringAssert.Contains("Display = Expandable", guide);
            string device = (string)typeof(POIEditorToolWindow).GetField("CardNavigationDeviceTestGuide", Static).GetValue(null);
            StringAssert.Contains("- Expandable:", device, "the device guide reads the lines at the phone's width");
            EditorTextChecks.AssertAscii(guide + device, "the navigation test guides");
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
            string first = POIEditorToolWindow.FirstLanguageOf(config.card_settings.languages);

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
                Assert.AreEqual("Tiles", POIEditorToolWindow.FitWithEllipsis(BlockSummaryRule.FirstWords(shortOne, BuiltInBlocks.RichText, first, first),
                    _window.RectOf("Card block summary#0").width, style), width + " pt: a short heading whole");
                string drawn = POIEditorToolWindow.FitWithEllipsis(BlockSummaryRule.FirstWords(longOne, BuiltInBlocks.RichText, first, first), summary.width, style);
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

            var inside = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Apps/LivingRoom/Resources/LivingRoom/CardMedia/images/azulejo_detail.jpg");
            Assert.IsNotNull(inside, "precondition: the fixture picture exists");
            yield return DropOnto("Block field image#0", inside);
            Assert.AreEqual("images/azulejo_detail.jpg", POIEditorToolWindow.AssetValue(Zoom(), BuiltInBlocks.ZoomImageImageField),
                "a real drop stores the path inside the Media Folder, not the project path");
            Assert.IsTrue(_window.Unsaved);
            Assert.AreSame(inside, POIEditorToolWindow.MediaAssetFor("images/azulejo_detail.jpg", MediaFolder), "the row shows the picture the app will load (through Resources)");
            var shown = BlockStackBuilder.Build(_window.Config.pois[0], _window.Config.card_settings, BlockRegistry.Shared);
            CollectionAssert.IsEmpty(shown.Skipped, "the card shows the block");

            yield return _window.PressUndo();
            Assert.AreEqual("", POIEditorToolWindow.AssetValue(Zoom(), BuiltInBlocks.ZoomImageImageField), "one Ctrl+Z takes the drop back");
            yield return _window.PressRedo();
            Assert.AreEqual("images/azulejo_detail.jpg", POIEditorToolWindow.AssetValue(Zoom(), BuiltInBlocks.ZoomImageImageField), "Ctrl+Y");

            const string outsidePath = "Assets/Framework/Runtime/UI/World/Markers/SymbolCircle.png";
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
            var picture = AssetDatabase.LoadAssetAtPath<Texture2D>(audioFolder + "images/azulejo_detail.jpg");
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

        // Per Language media (_3.3 P1b): with two wall languages a Per Language field (the audio guide's Clip and Captions) gets one more row
        // for the second language and no other field does; a real drop on "Clip (pt)" stores that language's own file beside the shared one,
        // the card reads each language's file, and Ctrl+Z takes the drop back
        [UnityTest]
        public IEnumerator APerLanguageMediaField_HasARowPerOtherLanguage_ARealDropStoresThatLanguagesFile_AndTheCardReadsIt()
        {
            var config = TwoPoiConfig();
            config.card_settings.languages = new List<string> { "en", "pt" };
            config.card_settings.media_resources_path = MediaFolder;
            config.pois[0].card.blocks.Add(new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.AudioGuideKind });
            config.pois[0].card.blocks.Add(new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.VideoKind });
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenPoiCardContent("poi_1");
            var foldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor);
            foldouts["poi_1/block_1"] = true;
            foldouts["poi_1/block_2"] = true;
            yield return _window.WaitForRepaint();
            _window.RectOf("Block field clip pt#0");
            _window.RectOf("Block field captions pt#0");
            _window.RectOf("Block field clip pt#1");
            _window.RectOf("Block field poster#1");
            Assert.IsFalse(_window.ScreenRects.ContainsKey("Block field clip en#0"), "the first language has no row of its own: the shared row is its file");
            Assert.IsFalse(_window.ScreenRects.ContainsKey("Block field poster pt#1"), "a media field that is not Per Language (the video's poster) gets no language row");
            BlockInstanceData Audio() => _window.Config.pois[0].card.blocks[0];
            Assert.IsNull(Audio().fields.Find(f => f.key == BuiltInBlocks.AudioGuideClipField), "drawing the rows writes nothing");

            const string folder = "Assets/Apps/LivingRoom/Resources/LivingRoom/CardMedia/audio/";
            var english = AssetDatabase.LoadAssetAtPath<AudioClip>(folder + "guide_castle_en.mp3");
            var portuguese = AssetDatabase.LoadAssetAtPath<AudioClip>(folder + "guide_castle_pt.mp3");
            var portugueseCaptions = AssetDatabase.LoadAssetAtPath<TextAsset>(folder + "guide_castle_pt.vtt");
            Assert.IsNotNull(english, "precondition: the English guide imports as a clip");
            Assert.IsNotNull(portuguese, "precondition: the Portuguese guide imports as a clip");
            Assert.IsNotNull(portugueseCaptions, "precondition: its captions import as a TextAsset");

            yield return DropOnto("Block field clip#0", english);
            yield return DropOnto("Block field clip pt#0", portuguese);
            yield return DropOnto("Block field captions pt#0", portugueseCaptions);
            var clip = Audio().fields.Find(f => f.key == BuiltInBlocks.AudioGuideClipField);
            Assert.AreEqual("audio/guide_castle_en.mp3", clip.asset, "the shared row keeps the file for every language");
            Assert.AreEqual("audio/guide_castle_pt.mp3", POIEditorToolWindow.LanguageEntryValue(clip.asset_by_language, "pt"), "the pt row stored the Portuguese file");
            Assert.AreEqual(1, clip.asset_by_language.Count, "one entry: pt only");
            yield return DropOnto("Block field captions pt#0", english);
            Assert.AreEqual("audio/guide_castle_pt.vtt",
                POIEditorToolWindow.LanguageEntryValue(Audio().fields.Find(f => f.key == BuiltInBlocks.AudioGuideCaptionsField).asset_by_language, "pt"),
                "a language row takes only the field's own kind of file");

            Assert.AreEqual("audio/guide_castle_en.mp3", new BlockFieldReader(Audio(), "en", "en").ValidAsset(BuiltInBlocks.AudioGuideClipField, MediaKind.Audio), "the English card plays the shared file");
            Assert.AreEqual("audio/guide_castle_pt.mp3", new BlockFieldReader(Audio(), "pt", "en").ValidAsset(BuiltInBlocks.AudioGuideClipField, MediaKind.Audio), "the Portuguese card plays its own");
            Assert.AreEqual("", new BlockFieldReader(Audio(), "en", "en").ValidAsset(BuiltInBlocks.AudioGuideCaptionsField, MediaKind.Captions),
                "English has no captions: the Portuguese file is never another language's fallback");
            CollectionAssert.IsEmpty(BlockStackBuilder.Build(_window.Config.pois[0], _window.Config.card_settings, BlockRegistry.Shared).Skipped
                .Where(s => s.Instance.key == "block_1"), "the card shows the audio block (the empty video beside it is only there for its rows)");

            yield return _window.PressUndo();
            // - an undo restores the config from before the drop, when the field itself may not have existed yet: read it null-safe
            Assert.AreEqual("", POIEditorToolWindow.LanguageEntryValue(Audio().fields.Find(f => f.key == BuiltInBlocks.AudioGuideCaptionsField)?.asset_by_language, "pt"), "one Ctrl+Z takes the captions drop back");
            yield return _window.PressUndo();
            Assert.AreEqual("", POIEditorToolWindow.LanguageEntryValue(Audio().fields.Find(f => f.key == BuiltInBlocks.AudioGuideClipField)?.asset_by_language, "pt"), "the next the pt clip's");
            Assert.AreEqual("audio/guide_castle_en.mp3", POIEditorToolWindow.AssetValue(Audio(), BuiltInBlocks.AudioGuideClipField), "and the shared clip stays");
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

        // Detail Card > Card Components > Page Chrome (_3.3 S2c): both parts are drawn ON in the shipped wall (the file written before them reads
        // the schema defaults) and drawing writes nothing; a real click on each turns it off and the rule the card reads follows; each click is
        // one history step, Ctrl+Z / Ctrl+Y walk them
        [UnityTest]
        public IEnumerator ThePageChromeRows_AreDrawnOn_RealClicksTurnEachOff_TheCardsRuleFollows_AndCtrlZ()
        {
            _window = new PoiEditorWindowHost(ShippedConfig(), "_showCardComponents");
            OpenTab("DetailCard");
            yield return _window.WaitForRepaint();
            _window.RectOf("Breadcrumb");
            _window.RectOf("Previous / Next");
            Assert.IsFalse(_window.Unsaved, "drawing the rows writes nothing");
            var wall = _window.Config.card_settings.components;
            Assert.IsTrue(wall.page_chrome_breadcrumb && wall.page_chrome_prev_next, "both parts are on in the shipped wall");
            Assert.IsTrue(PageChromeRule.Wall(wall).Breadcrumb && PageChromeRule.Wall(wall).PrevNext);

            _window.Click("Breadcrumb");
            yield return _window.WaitForRepaint();
            Assert.IsFalse(_window.Config.card_settings.components.page_chrome_breadcrumb, "a real click turned the breadcrumb off");
            Assert.IsTrue(_window.Config.card_settings.components.page_chrome_prev_next, "and only it");
            Assert.IsFalse(PageChromeRule.Wall(_window.Config.card_settings.components).Breadcrumb, "the rule the card reads follows");
            _window.Click("Previous / Next");
            yield return _window.WaitForRepaint();
            Assert.IsFalse(_window.Config.card_settings.components.page_chrome_prev_next, "a real click turned Previous / Next off");
            Assert.IsTrue(_window.Unsaved);

            yield return _window.PressUndo();
            Assert.IsTrue(_window.Config.card_settings.components.page_chrome_prev_next, "Ctrl+Z: Previous / Next back");
            Assert.IsFalse(_window.Config.card_settings.components.page_chrome_breadcrumb, "one step at a time");
            yield return _window.PressUndo();
            Assert.IsTrue(_window.Config.card_settings.components.page_chrome_breadcrumb, "Ctrl+Z: the breadcrumb back");
            yield return _window.PressRedo();
            Assert.IsFalse(_window.Config.card_settings.components.page_chrome_breadcrumb, "Ctrl+Y");
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
                ("CardHideWhileScrollingHelp", new[] { "When Scrolling Down", "Never", "scrolls up a little", "the end of the card", "its own Hide While Scrolling decides" }),
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

        [UnityTest]
        public IEnumerator TheContentsKind_HasABlockLibraryRow_AndPlusAddBlockAddsIt_WithRealClicks() =>
            NavKindHasABlockLibraryRowAndPlusAddBlock(BuiltInBlocks.NavContentsKind);

        // Specific Marker > Card Content (step 10): a hotspot spot's Opens Block is the SAME popup as an Open Block action's (the BlockRef
        // drawer, no second picker) but OPTIONAL: a spot with nothing picked draws no warning (it is the normal spot), a stale pick is
        // kept and warned by name like the action's, a pick is undoable with Ctrl+Z and the card's rule says whether it jumps
        [UnityTest]
        public IEnumerator AHotspotSpotsOpensBlock_IsOptional_BlankIsNotWarned_AStaleKeyIs_AndAPickIsUndoneByCtrlZ()
        {
            var config = TwoPoiConfig();
            config.card_settings.languages = new List<string> { "en" };
            var hotspot = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.HotspotImageKind, variant = BuiltInBlocks.HotspotNumbered, display = CardOptions.DisplayInline };
            var blank = new BlockItemData();
            var staleRow = new BlockItemData();
            POIEditorToolWindow.SetItemChoiceValue(staleRow, BuiltInBlocks.HotspotOpensBlockField, "block_9");
            // - read from the window's config each time: a Ctrl+Z puts back a COPY of the config, so a row held from before is a stale object
            BlockItemData SpotRow(int row) =>
                _window.Config.pois[0].card.blocks[1].fields.Single(f => f.key == BuiltInBlocks.HotspotItemsField).items[row];
            hotspot.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HotspotItemsField, items = new List<BlockItemData> { blank, staleRow } });
            config.pois[0].card.blocks.AddRange(new[] { TextBlock("block_1", "The story of the keep."), hotspot, TextBlock("block_3", "Second text") });
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenPoiCardContent("poi_1");
            var foldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor);
            foldouts["poi_1/block_2"] = true;
            yield return Redraw();

            // - the row is drawn for EVERY spot (blank or not), as the same popup an Open Block action uses
            _window.RectOf("Block item spots 0 opens_block#1");
            _window.RectOf("Block item spots 1 opens_block#1");
            Assert.IsFalse(Drawn("Block item spots 0 opens_block warning#1"), "nothing picked is the normal spot: no warning");
            Assert.IsTrue(Drawn("Block item spots 1 opens_block warning#1"), "a deleted pick is warned under its row, like an Open Block action's");
            Assert.IsFalse(_window.Unsaved, "drawing the rows writes nothing");
            Assert.AreEqual("block_9", POIEditorToolWindow.ItemChoiceValue(SpotRow(1), BuiltInBlocks.HotspotOpensBlockField), "the deleted key is kept, never rewritten by drawing");

            var poi = _window.Config.pois[0];
            var options = POIEditorToolWindow.BlockRefOptions(poi, poi.card.blocks[1], "block_9", config.card_settings.languages);
            CollectionAssert.AreEqual(new[] { "(none)", "1. Rich Text: The story of the keep.", "3. Rich Text: Second text", "(a deleted block)" }, options.Labels,
                "the same popup as an Open Block action's: the other blocks by row number, kind and first words");

            // - a blank row still reports its problem to the RULE (nothing picked leads nowhere); only the Editor row is quiet about it
            var built = BlockStackBuilder.Build(poi, _window.Config.card_settings, BlockRegistry.Shared, _window.Config.pois);
            Assert.AreEqual(BlockRefRule.Problem.Blank, BlockRefRule.Check("", poi.card.blocks[1], poi, built, _ => false));

            // - a pick through the window's mutation scope: the warning goes, the pin now jumps; Ctrl+Z brings the stale key and its warning back
            Pick(() => POIEditorToolWindow.SetItemChoiceValue(SpotRow(1), BuiltInBlocks.HotspotOpensBlockField, options.KeyAt(2)));
            Assert.IsTrue(_window.Unsaved);
            yield return Redraw();
            Assert.IsFalse(Drawn("Block item spots 1 opens_block warning#1"), "a block of the scrolling part: no warning");
            built = BlockStackBuilder.Build(_window.Config.pois[0], _window.Config.card_settings, BlockRegistry.Shared, _window.Config.pois);
            var read = new BlockFieldReader(_window.Config.pois[0].card.blocks[1], "en", "en");
            string stored = read.ItemValue(read.Items(BuiltInBlocks.HotspotItemsField)[1], BuiltInBlocks.HotspotOpensBlockField);
            Assert.AreEqual("block_3", HotspotPinRule.JumpTarget(stored, BlockRefRule.JumpTargets(built.Entries, _ => false)), "the card now draws that pin as a jump");
            yield return _window.PressUndo();
            Assert.AreEqual("block_9", POIEditorToolWindow.ItemChoiceValue(SpotRow(1), BuiltInBlocks.HotspotOpensBlockField), "Ctrl+Z");
            yield return Redraw();
            _window.RectOf("Block item spots 1 opens_block warning#1");
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
            Assert.AreEqual(10, familyRows.Count, "precondition: the table lists the ten registered families");
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
            Assert.AreEqual("stories", before[1], "precondition: stories follows about");

            yield return _window.ScrollTo("Family down#0");
            _window.Click("Family down#0");
            yield return _window.WaitForRepaint();

            var after = _window.Editor.FamiliesInEffectiveOrder(_window.Config.card_settings.navigation).Select(f => f.Key).ToList();
            Assert.AreEqual("stories", after[0], "a real click moved about down past stories");
            Assert.AreEqual("about", after[1]);
            CollectionAssert.AreEqual(before.Skip(2), after.Skip(2), "nothing below the two swapped families moved");
            Assert.IsTrue(_window.Unsaved);
            CollectionAssert.AreEquivalent(new[] { "about", "stories" },
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

        // Specific Marker > Card Content (_3.2 step 11b): Reading Time is drawn on a text block right under its Heading (the generic Number
        // drawer, no new drawer), on its default 0 with nothing stored; a real click near the right end of its track writes whole minutes the
        // contents list reads, and one Ctrl+Z takes them back. An audio and a video block draw NO Reading Time row (their clip's length is
        // their time), and neither does the header
        [UnityTest]
        public IEnumerator TheReadingTimeRow_IsOnATextBlock_NotOnAudioVideoOrTheHeader_AndARealClickIsUndoneByCtrlZ()
        {
            var config = TwoPoiConfig();
            config.card_settings.languages = new List<string> { "en" };
            var audio = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.AudioGuideKind, variant = BuiltInBlocks.AudioGuidePlayer, display = CardOptions.DisplayInline };
            var video = new BlockInstanceData { key = "block_3", kind = BuiltInBlocks.VideoKind, variant = BuiltInBlocks.VideoInline, display = CardOptions.DisplayInline };
            var header = new BlockInstanceData { key = "block_4", kind = BuiltInBlocks.HeaderKind, variant = BuiltInBlocks.HeaderTextOnly, display = CardOptions.DisplayInline };
            config.pois[0].card.blocks.AddRange(new[] { TextBlock("block_1", "The story of the keep."), audio, video, header });
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
            OpenPoiCardContent("poi_1");
            var foldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor);
            foreach (string key in new[] { "block_1", "block_2", "block_3", "block_4" }) foldouts["poi_1/" + key] = true;
            yield return Redraw();
            Assert.IsTrue(Drawn("Block field minutes#0"), "the text block draws Reading Time");
            Assert.Less(_window.RectOf("Block field heading en#0").y, _window.RectOf("Block field minutes#0").y, "right under its Heading");
            Assert.IsTrue(Drawn("Block field clip#1"), "precondition: the audio block is open (its Clip row is drawn)");
            Assert.IsFalse(Drawn("Block field minutes#1"), "an audio block reads its clip: no Reading Time row");
            Assert.IsTrue(Drawn("Block field clip#2"), "precondition: the video block is open");
            Assert.IsFalse(Drawn("Block field minutes#2"), "a video block neither");
            Assert.IsFalse(Drawn("Block field minutes#3"), "nor the header");
            Assert.IsFalse(_window.Unsaved, "drawing the rows writes nothing");
            BlockInstanceData Text() => _window.Config.pois[0].card.blocks[0];
            Assert.IsNull(Text().fields.Find(f => f.key == BlockKindDefinition.MinutesField), "no Reading Time stored yet: 0, no time shown");

            // - a real click near the RIGHT end of the track (the number box sits after it): close to the 60-minute maximum
            Rect row = _window.RectOf("Block field minutes#0");
            _window.ClickAt(new Vector2(row.xMax - EditorGUIUtility.fieldWidth - 12f, row.center.y));
            yield return _window.WaitForRepaint();
            var stored = Text().fields.Find(f => f.key == BlockKindDefinition.MinutesField);
            Assert.IsNotNull(stored, "a real click wrote Reading Time");
            int minutes = ReadingTimeRule.Minutes(BuiltInBlocks.RichText, Text(), 0f);
            Assert.Greater(minutes, 45, "near the track's right end: most of an hour, in whole minutes for the contents list");
            Assert.LessOrEqual(minutes, (int)BlockKindDefinition.MinutesDefinition.NumberMax);
            Assert.IsTrue(_window.Unsaved);

            yield return _window.PressUndo();
            Assert.IsNull(Text().fields.Find(f => f.key == BlockKindDefinition.MinutesField), "one Ctrl+Z: nothing stored again");
            Assert.AreEqual(0, ReadingTimeRule.Minutes(BuiltInBlocks.RichText, Text(), 0f));
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
                    // - a composed kind's rows (Elements: the story's Page and Preview Rows) are drawn by the element table (_3.3 S3a)
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

        [UnityTest]
        public IEnumerator ShowDemoCard_ARealClickTurnsItOn_TheChoicesAreStored_TheBuildGuardSeesIt_AndEachEditUndoes()
        {
            // - the framework default, whatever point the shipped wall last previewed (the wall's own choice must not leak in, 40-testing)
            var config = ShippedConfig();
            config.card_settings.demo_card.poi_id = "";
            _window = new PoiEditorWindowHost(config, "_showCardContainer");
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
            var saved = new CardLocalState(DeviceStateStore.Current, config.wall_id);
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
            var mine = new CardLocalState(DeviceStateStore.Current, wall);
            var theirs = new CardLocalState(DeviceStateStore.Current, otherWall);
            try
            {
                mine.SetAnswer("poi_1", "block_1", 0, 2);
                mine.SetVote("poi_1", "block_2", 5);
                mine.MarkSeen("poi_1", "block_3");
                // - step 11b: the contents ticks
                mine.MarkViewed("poi_1", "block_4");
                mine.MarkViewed("poi_1", "block_900");
                theirs.SetAnswer("poi_1", "block_1", 0, 1);
                theirs.MarkViewed("poi_1", "block_4");
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
                Assert.IsFalse(mine.Viewed("poi_1", "block_4"), "and the contents tick");
                Assert.IsFalse(mine.Viewed("poi_1", "block_900"), "the colon key's tick too");
                Assert.AreEqual(0, mine.CountViewed(), "no tick of this wall is left");
                Assert.AreEqual(1, theirs.Answer("poi_1", "block_1", 0), "another wall's saved state is untouched");
                Assert.IsTrue(theirs.Viewed("poi_1", "block_4"), "another wall keeps its ticks");
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
            var state = new CardLocalState(DeviceStateStore.Current, wall);
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
                    StringAssert.Contains("2 saved entries", EditorNotice.PendingMessage);
                    StringAssert.Contains("0 blocks ticked as read", EditorNotice.PendingMessage, "no tick among them");
                    // - step 11b: the notice counts the ticks it cleared, apart from the rest
                    state.SetAnswer("poi_1", "block_1", 0, 1);
                    state.MarkViewed("poi_1", "block_2");
                    state.MarkViewed("poi_1", "block_3");
                    state.MarkViewed("poi_1", "block_900");
                    Assert.AreEqual(4, (int)reset.Invoke(window, null), "an answer and three ticks cleared");
                    StringAssert.Contains("4 saved entries", EditorNotice.PendingMessage);
                    StringAssert.Contains("3 blocks ticked as read in a Contents block", EditorNotice.PendingMessage);
                    Assert.AreEqual("Nothing was saved for this wall on this computer.", POIEditorToolWindow.CardStateResetMessage(0, 0));
                    StringAssert.Contains("1 saved entry cleared", POIEditorToolWindow.CardStateResetMessage(1, 1));
                    StringAssert.Contains("1 block ticked", POIEditorToolWindow.CardStateResetMessage(1, 1));
                    EditorTextChecks.AssertAscii(POIEditorToolWindow.CardStateResetMessage(4, 3), "the reset notice");
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
