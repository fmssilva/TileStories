using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Editor.Tests
{
    // Block templates and the story Editor's last rows (_3.3 S3b: R14.4, R14.5, R17.1, R17.3, the S3a review) on the REAL POI Editor window
    // with REAL clicks and typing on the shipped wall, each gesture in one editor tick (40-testing 4.2.3), every effect read back from the
    // config and through BlockTemplates / BlockSurfaces, each ONE undo step taken back by a real Ctrl+Z:
    //   - Detail Card > Block Library > Templates: the built-ins listed, Duplicate, rename, edit a template's rows with the Page's own
    //     table, delete with its question (Cancel / Confirm)
    //   - Card Content: Save as template, "+ From a template...", Re-apply (only the missing rows), the Card Starter's template rows
    //   - the compact dropdown (more than 4 options), the Preview's Tap and Block Entry rows
    // Questions are answered through EditorDecision.Responder: a test never meets a modal (40-testing 4.2.2).
    public class BlockTemplateEditorTabTests
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;
        private const string Painting = "painting";
        private const string StoryKey = "block_19";
        private const string EmptyPoint = "camera";
        private PoiEditorWindowHost _window;
        private readonly List<DecisionRequest> _asked = new();

        [SetUp]
        public void SetUp() => EditorNotice.Clear();

        [TearDown]
        public void TearDown()
        {
            EditorDecision.Responder = TestDialogGuard.CancelEverything;
            NamedOptionPickerPopup.RowRectProbe = null;
            EditorPopup.CloseAll();
            EditorNotice.Clear();
            _window?.Close();
        }

        private static WallConfigData ShippedConfig() => ShippedWall.WithFrameworkNavigation();
        private static ElementRegistry Elements => ElementRegistry.Shared;
        private static BlockKindDefinition Story => BlockRegistry.Shared.TryGet(BuiltInBlocks.StoryKind, out var k) ? k : null;
        private T Field<T>(string name) => (T)typeof(POIEditorToolWindow).GetField(name, Instance).GetValue(_window.Editor);
        private Dictionary<string, bool> Composed => Field<Dictionary<string, bool>>("_composedOpen");
        private POIData Point(string id) => _window.Config.pois.Single(p => p.id == id);
        private List<BlockTemplateData> WallTemplates => _window.Config.card_settings.block_templates;

        // The window on the shipped wall, on `tab` with `foldout` open; every question recorded and answered `answer`
        private IEnumerator Open(string tab, string foldout, DecisionAnswer answer = DecisionAnswer.Cancel, Action before = null)
        {
            _asked.Clear();
            EditorDecision.Responder = request => { _asked.Add(request); return answer; };
            _window = new PoiEditorWindowHost(ShippedConfig(), foldout);
            var tabField = typeof(POIEditorToolWindow).GetField("_selectedTab", Instance);
            _window.SetWindowField("_selectedTab", Enum.Parse(tabField.FieldType, tab));
            before?.Invoke();
            yield return _window.WaitForRepaint();
        }

        // Detail Card > Block Library, open
        private IEnumerator OpenTheLibrary(DecisionAnswer answer = DecisionAnswer.Cancel) => Open("DetailCard", "_showCardBlockLibrary", answer);

        // Specific Marker > `poiId` > Card Content, its block `blockKey` open (when given)
        private IEnumerator OpenCardContent(string poiId, string blockKey = null, params string[] composedParts)
        {
            yield return Open("SpecificMarker", "_showPoiCardContent", DecisionAnswer.Cancel, () =>
            {
                Field<Dictionary<string, bool>>("_poiFoldouts")[poiId] = true;
                if (blockKey != null) Field<Dictionary<string, bool>>("_cardBlockFoldouts")[poiId + "/" + blockKey] = true;
                foreach (string part in composedParts) Composed[POIEditorToolWindow.ComposedKey(poiId, blockKey, part)] = true;
            });
        }

        private IEnumerator Click(string probeKey)
        {
            yield return _window.ScrollTo(probeKey);
            _window.Click(probeKey);
            yield return _window.WaitForRepaint();
        }

        private IEnumerator UndoesInOneStep(int stepsBefore, string what)
        {
            Assert.AreEqual(stepsBefore + 1, _window.UndoStepsLeft, what + ": one undo step");
            Assert.IsTrue(_window.Unsaved, what + ": Save lights up");
            yield return _window.PressUndo();
            Assert.AreEqual(stepsBefore, _window.UndoStepsLeft, what + ": Ctrl+Z took it back");
        }

        // A real click on the button that opens a picker of `kind`, then a real click on its row `index`
        private IEnumerator PickFrom(string buttonProbe, string kind, int index)
        {
            var seen = new Dictionary<int, Rect>();
            NamedOptionPickerPopup.RowRectProbe = (i, r) => seen[i] = r;
            yield return Click(buttonProbe);
            var popup = EditorPopup.FindOpen(kind);
            Assert.IsNotNull(popup, "a real click on '" + buttonProbe + "' opens the picker");
            for (int frame = 0; frame < 90 && !seen.ContainsKey(index); frame++)
            {
                popup.Repaint();
                yield return null;
            }
            Assert.IsTrue(seen.ContainsKey(index), "precondition: the picker drew row " + index);
            popup.SendEvent(new Event { type = EventType.MouseDown, button = 0, clickCount = 1, mousePosition = seen[index].center });
            yield return _window.WaitForRepaint();
            Assert.IsNull(EditorPopup.FindOpen(kind), "a pick closes the picker");
        }

        // Set up state through the window's own history (one recorded step, like any popup edit), so what a test measures after it is
        // only the gesture under test
        private IEnumerator Setup(Action edit)
        {
            typeof(POIEditorToolWindow).GetMethod("ApplyPopupEdit", Instance).Invoke(_window.Editor, new object[] { edit });
            yield return _window.WaitForRepaint();
        }

        private static List<string> Kinds(IEnumerable<BlockItemData> rows) => rows.Select(r => new ElementRow(r).Kind).ToList();

        // ---------------- Detail Card > Block Library > Templates ----------------

        [UnityTest]
        public IEnumerator TheTemplatesTable_ListsTheBuiltIns_Duplicate_ThenARealRename_EachOneUndoStep_AndDrawingWritesNothing()
        {
            yield return OpenTheLibrary();
            yield return _window.ScrollTo("Template name#1");
            Assert.Less(_window.RectOf("Block Library enabled#0").y, _window.RectOf("Template name#0").y, "the Templates table sits under the kinds");
            Assert.IsFalse(_window.ScreenRects.ContainsKey("Template delete#0"), "a built-in has no delete");
            Assert.IsFalse(_window.Unsaved, "drawing the table writes nothing");
            Assert.AreEqual(0, _window.UndoStepsLeft);

            int steps = _window.UndoStepsLeft;
            yield return Click("Template duplicate#0");
            var copy = WallTemplates.Single();
            Assert.AreEqual("Story -- magazine (copy)", copy.name);
            Assert.AreEqual(17, BlockTemplates.RowsOf(copy).Count, "a copy of the magazine's rows");
            Assert.AreEqual(steps + 1, _window.UndoStepsLeft, "Duplicate: one step");

            yield return _window.ScrollTo("Template name#2");
            yield return _window.ReplaceText("Template name#2", "Our long read");
            yield return _window.ClickAway();
            Assert.AreEqual("Our long read", WallTemplates.Single().name, "renamed by real typing");
            Assert.AreEqual(steps + 2, _window.UndoStepsLeft, "the typed name: one step");
            yield return _window.PressUndo();
            Assert.AreEqual("Story -- magazine (copy)", WallTemplates.Single().name, "Ctrl+Z: the old name");
            yield return UndoesInOneStep(steps, "Duplicate");
            Assert.IsEmpty(WallTemplates, "Ctrl+Z: the copy is gone");
        }

        [UnityTest]
        public IEnumerator ATemplatesRows_AreEditedWithThePagesOwnTable_AddMoveLookDelete_NoContentNoMarks_EachCtrlZ()
        {
            yield return OpenTheLibrary();
            yield return Click("Template duplicate#1");
            var template = WallTemplates.Single();
            Assert.AreEqual(4, BlockTemplates.RowsOf(template).Count, "precondition: a copy of Story -- short, opened");
            yield return _window.WaitForRepaint();
            for (int i = 0; i < 4; i++) _window.RectOf("Template page element#" + i);
            Assert.IsFalse(_window.ScreenRects.Keys.Any(k => k.StartsWith("Template page validated") || k.StartsWith("Template page preview mark")),
                "a template's table has no Validated and no P marks");
            int steps = _window.UndoStepsLeft;

            // - "+ Add element", then Divider from the same palette as the Page's
            yield return Click("Template page add#0");
            yield return Click("Template page add " + BuiltInElements.Divider + "#0");
            CollectionAssert.AreEqual(new[] { BuiltInElements.Title, BuiltInElements.Lede, BuiltInElements.Paragraph, BuiltInElements.Picture, BuiltInElements.Divider },
                Kinds(BlockTemplates.RowsOf(WallTemplates.Single())), "the divider added at the end");
            Assert.AreEqual(steps + 1, _window.UndoStepsLeft, "add: one step");
            // - the new row is born open: its Look is there, its content is not (a template holds no words)
            Assert.IsTrue(_window.ScreenRects.ContainsKey("Template page look ornament 1#4"), "the new row's Look");
            Assert.IsFalse(_window.ScreenRects.Keys.Any(k => k.StartsWith("Template page text")), "no content field in a template");
            yield return Click("Template page look ornament 1#4");
            Assert.AreEqual(BuiltInElements.OrnamentAzulejo, new ElementRow(BlockTemplates.RowsOf(WallTemplates.Single())[4]).Value(BuiltInElements.OrnamentStyle));
            Assert.AreEqual(steps + 2, _window.UndoStepsLeft, "a look: one step");

            yield return Click("Template page up#4");
            Assert.AreEqual(BuiltInElements.Divider, Kinds(BlockTemplates.RowsOf(WallTemplates.Single()))[3], "moved up");
            yield return Click("Template page delete#0");
            CollectionAssert.AreEqual(new[] { BuiltInElements.Lede, BuiltInElements.Paragraph, BuiltInElements.Divider, BuiltInElements.Picture },
                Kinds(BlockTemplates.RowsOf(WallTemplates.Single())), "the title deleted");
            Assert.AreEqual(steps + 4, _window.UndoStepsLeft);
            for (int k = 0; k < 4; k++) yield return _window.PressUndo();
            Assert.AreEqual(steps, _window.UndoStepsLeft);
            CollectionAssert.AreEqual(new[] { BuiltInElements.Title, BuiltInElements.Lede, BuiltInElements.Paragraph, BuiltInElements.Picture },
                Kinds(BlockTemplates.RowsOf(WallTemplates.Single())), "four Ctrl+Z: the template as it was copied");
        }

        [UnityTest]
        public IEnumerator DeletingATemplate_AsksFirst_CancelKeepsIt_DeleteTemplateRemovesIt_TheBlocksKeepTheirRows_AndCtrlZBringsItBack(
            [Values(false, true)] bool confirm)
        {
            var answer = confirm ? DecisionAnswer.Confirm : DecisionAnswer.Cancel;
            yield return OpenTheLibrary(answer);
            // - a wall template, and a block made from it on The Camera (through the window's history)
            yield return Setup(() =>
            {
                var config = _window.Config;
                var template = BlockTemplates.Duplicate(config.card_settings, config.pois, BlockTemplates.BuiltIn(Elements)[1], Elements);
                var camera = config.pois.Single(p => p.id == EmptyPoint);
                camera.card ??= new POICardData();
                camera.card.blocks.Add(BlockTemplates.NewBlock(camera.card, template, Elements));
            });
            int steps = _window.UndoStepsLeft;

            yield return Click("Template delete#2");
            Assert.AreEqual(1, _asked.Count, "one question");
            Assert.AreEqual("Delete Template", _asked[0].ConfirmLabel, "the confirm names the action");
            StringAssert.Contains("1 block was made from it: they keep every row", _asked[0].Message, "the question counts the blocks from the real state");
            if (answer == DecisionAnswer.Cancel)
            {
                Assert.AreEqual(1, WallTemplates.Count, "Cancel: the template stays");
                Assert.AreEqual(steps, _window.UndoStepsLeft, "Cancel: no undo step");
                yield break;
            }
            Assert.IsEmpty(WallTemplates, "Delete Template: it is gone");
            var block = Point(EmptyPoint).card.blocks.Last();
            Assert.AreEqual(4, BlockSurfaces.PageRows(block, Story).Count, "the block made from it keeps its four rows");
            yield return UndoesInOneStep(steps, "Delete Template");
            Assert.AreEqual(1, WallTemplates.Count, "Ctrl+Z: the template is back");
        }

        // ---------------- Card Content: Save as template, + From a template, Re-apply, the Card Starter ----------------

        [UnityTest]
        public IEnumerator SaveAsTemplate_ARealClickMakesATemplateOfTheStorysStructure_NamedAfterThePoint_SaysWhere_AndCtrlZ()
        {
            yield return OpenCardContent(Painting, StoryKey);
            int steps = _window.UndoStepsLeft;
            Assert.AreEqual("", BlockSurfaces.TemplateOf(Point(Painting).card.blocks.Single(b => b.key == StoryKey)), "precondition: made from no template");
            yield return Click("Story template save#0");
            var template = WallTemplates.Single();
            Assert.AreEqual("Story -- " + Point(Painting).name, template.name);
            Assert.AreEqual(17, BlockTemplates.RowsOf(template).Count);
            Assert.IsFalse(BlockTemplates.RowsOf(template).Any(r => r.fields.Any(f => f.key == ElementRow.MediaKey || f.text.Any(t => t.value.Length > 0))),
                "no words or files");
            var story = Point(Painting).card.blocks.Single(b => b.key == StoryKey);
            Assert.AreEqual(template.key, BlockSurfaces.TemplateOf(story), "the block is now made from it");
            StringAssert.Contains("Detail Card > Block Library > Templates", EditorNotice.PendingMessage, "a notice says where to rename it");
            yield return _window.WaitForRepaint();
            Assert.IsTrue(_window.ScreenRects.ContainsKey("Story template reapply#0"), "the Template line now offers Re-apply");
            yield return UndoesInOneStep(steps, "Save as template");
            Assert.IsEmpty(WallTemplates);
            Assert.AreEqual("", BlockSurfaces.TemplateOf(Point(Painting).card.blocks.Single(b => b.key == StoryKey)));
        }

        [UnityTest]
        public IEnumerator FromATemplate_ARealPickAddsOneBlockWithCopiesOfItsRows_OpenAndReadyToFill_AndCtrlZ()
        {
            yield return OpenCardContent(EmptyPoint);
            int steps = _window.UndoStepsLeft;
            int before = Point(EmptyPoint).card?.blocks?.Count ?? 0;
            yield return PickFrom("Card add from template#0", BlockTemplates.PickerKind, 1);
            var blocks = Point(EmptyPoint).card.blocks;
            Assert.AreEqual(before + 1, blocks.Count, "one block added");
            var block = blocks.Last();
            Assert.AreEqual(BuiltInBlocks.StoryKind, block.kind);
            Assert.AreEqual(BlockTemplates.ShortKey, BlockSurfaces.TemplateOf(block));
            CollectionAssert.AreEqual(new[] { BuiltInElements.Title, BuiltInElements.Lede, BuiltInElements.Paragraph, BuiltInElements.Picture },
                Kinds(BlockSurfaces.PageRows(block, Story)));
            Assert.IsTrue(Field<Dictionary<string, bool>>("_cardBlockFoldouts")[EmptyPoint + "/" + block.key], "the new block opens");
            // - the card's own rule names what is left to fill: a required row with no complete content
            var built = BlockStackBuilder.Build(Point(EmptyPoint), _window.Config.card_settings, BlockRegistry.Shared, _window.Config.pois);
            Assert.IsTrue(built.Skipped.Any(s => ReferenceEquals(s.Instance, block)), "an empty story is named under its row, not shown to a visitor");
            yield return _window.WaitForRepaint();
            _window.RectOf("Story template name#0");
            _window.RectOf("Story template reapply#0");
            yield return UndoesInOneStep(steps, "+ From a template");
            Assert.AreEqual(before, Point(EmptyPoint).card?.blocks?.Count ?? 0);
        }

        [UnityTest]
        public IEnumerator ReApply_ARealClickAddsBackOnlyTheRowsTheBlockLacks_TellsWhich_LeavesTheRestUntouched_AndCtrlZ()
        {
            yield return OpenCardContent(EmptyPoint);
            yield return PickFrom("Card add from template#0", BlockTemplates.PickerKind, 0);
            var block = Point(EmptyPoint).card.blocks.Last();
            Assert.AreEqual(17, BlockSurfaces.PageRows(block, Story).Count, "precondition: a magazine block");
            // - the developer deletes the eyebrow (row 2) and the video (row 17) with real clicks, and writes the title
            yield return Click("Story page delete#16");
            yield return Click("Story page delete#1");
            yield return Setup(() =>
            {
                var title = new ElementRow(BlockSurfaces.PageRows(Point(EmptyPoint).card.blocks.Last(), Story)[1]);
                Assert.AreEqual(BuiltInElements.Title, title.Kind, "precondition: the title is row 2 now");
                title.SetValue(BuiltInElements.TextField, "");
                title.Field(BuiltInElements.TextField).text.Add(new LocalizedEntry { lang = "en", value = "Kept" });
            });
            block = Point(EmptyPoint).card.blocks.Last();
            Assert.AreEqual(15, BlockSurfaces.PageRows(block, Story).Count);
            var kept = BlockSurfaces.PageRows(block, Story).Select(r => JsonUtility.ToJson(r)).ToList();
            int steps = _window.UndoStepsLeft;

            yield return Click("Story template reapply#0");
            block = Point(EmptyPoint).card.blocks.Last();
            var rows = BlockSurfaces.PageRows(block, Story);
            Assert.AreEqual(17, rows.Count, "the two missing rows are back");
            Assert.AreEqual(BuiltInElements.Eyebrow, new ElementRow(rows[1]).Kind, "the eyebrow in its place");
            Assert.AreEqual(BuiltInElements.Video, new ElementRow(rows[16]).Kind, "the video in its place");
            CollectionAssert.AreEqual(kept, rows.Where((r, i) => i != 1 && i != 16).Select(r => JsonUtility.ToJson(r)),
                "every row it had is untouched, the title's words included");
            StringAssert.StartsWith("Added 2 rows from \"Story -- magazine\": Eyebrow, Video.", EditorNotice.PendingMessage, "it tells what it added");
            yield return UndoesInOneStep(steps, "Re-apply");
            Assert.AreEqual(15, BlockSurfaces.PageRows(Point(EmptyPoint).card.blocks.Last(), Story).Count);

            EditorNotice.Clear();
            yield return Click("Story template reapply#0");
            yield return Click("Story template reapply#0");
            StringAssert.StartsWith("Nothing to add", EditorNotice.PendingMessage, "a second Re-apply has nothing left to add");
        }

        [UnityTest]
        public IEnumerator TheCardStarter_OffersTheTemplatesBesideItsSets_APickAddsOneBlock_KeepsTheCardsBlocks_AsksNothing_AndCtrlZ()
        {
            const string authored = "lamp_military";
            yield return OpenCardContent(authored);
            var before = Point(authored).card.blocks.Select(b => b.key).ToList();
            Assert.Greater(before.Count, 0, "precondition: an authored card");
            int steps = _window.UndoStepsLeft;
            int firstTemplate = CardStarters.All.Count;
            yield return PickFrom("Card Starter", CardStarters.PickerKind, firstTemplate + 1);
            Assert.IsEmpty(_asked, "adding a block replaces nothing: no question");
            var blocks = Point(authored).card.blocks;
            CollectionAssert.AreEqual(before, blocks.Take(before.Count).Select(b => b.key), "every block the card had is kept");
            Assert.AreEqual(BlockTemplates.ShortKey, BlockSurfaces.TemplateOf(blocks.Last()), "one block made from Story -- short, at the end");
            yield return UndoesInOneStep(steps, "Card Starter template");
            CollectionAssert.AreEqual(before, Point(authored).card.blocks.Select(b => b.key));
        }

        // ---------------- the compact dropdown, the Preview's Tap and Block Entry ----------------

        [UnityTest]
        public IEnumerator MoreThanFourOptions_IsOneCompactDropdown_ARealClickOpensItsList_APickClosesIt_FourOrFewerStaySegments_AndCtrlZ()
        {
            // - the hero picture (7 effects) and the title (4) open on their Motion parts
            yield return OpenCardContent(Painting, StoryKey, "row/el_12", "row/el_12/Motion", "row/el_2", "row/el_2/Motion");
            Assert.IsTrue(_window.ScreenRects.ContainsKey("Story page motion effect 0#2"), "the title's 4 effects stay segments, all in view");
            Assert.IsTrue(_window.ScreenRects.ContainsKey("Story page motion effect#0"), "the picture's effect is one dropdown");
            Assert.IsFalse(_window.ScreenRects.ContainsKey("Story page motion effect 0#0"), "closed: its options are not drawn");
            int steps = _window.UndoStepsLeft;

            yield return Click("Story page motion effect#0");
            for (int k = 0; k < ElementOptions.Motions.Length; k++) _window.RectOf("Story page motion effect " + k + "#0");
            Assert.AreEqual(steps, _window.UndoStepsLeft, "opening the list writes nothing");
            int pan = Array.IndexOf(ElementOptions.Motions, ElementOptions.MotionPan);
            Assert.Greater(_window.RectOf("Story page motion effect " + pan + "#0").y, _window.RectOf("Story page motion effect#0").y, "the list opens under it");
            yield return Click("Story page motion effect " + pan + "#0");
            var hero = new ElementRow(BlockSurfaces.PageRows(Point(Painting).card.blocks.Single(b => b.key == StoryKey), Story)[0]);
            Assert.AreEqual(ElementOptions.MotionPan, hero.Motion, "Pan picked by a real click");
            yield return _window.WaitForRepaint();
            Assert.IsFalse(Composed.TryGetValue("dropdown/Story page motion effect#0", out bool open) && open, "a pick closes the list");
            yield return UndoesInOneStep(steps, "Dropdown pick");
            Assert.AreEqual(ElementOptions.MotionAuto, new ElementRow(BlockSurfaces.PageRows(Point(Painting).card.blocks.Single(b => b.key == StoryKey), Story)[0]).Motion);
        }

        [UnityTest]
        public IEnumerator ThePreviewsTapAndBlockEntry_AreRealClicks_DefaultOpenThePageAndRisesIn_EachOneStep_AndCtrlZ()
        {
            yield return OpenCardContent(Painting, StoryKey);
            BlockInstanceData StoryBlock() => Point(Painting).card.blocks.Single(b => b.key == StoryKey);
            Assert.AreEqual(ElementOptions.PreviewTapOpenPage, BlockSurfaces.PreviewTap(StoryBlock()), "the story's default: open the page");
            Assert.AreEqual(ElementOptions.PreviewEntryRise, BlockSurfaces.PreviewEntry(StoryBlock()), "the default entry: rises in");
            Assert.Less(_window.RectOf("Story open row#0").y, _window.RectOf("Story preview tap#0").y, "under the Open Row");
            Assert.Less(_window.RectOf("Story preview tap#0").y, _window.RectOf("Story preview entry#0").y);
            int steps = _window.UndoStepsLeft;

            yield return Click("Story preview tap 1#0");
            Assert.AreEqual(ElementOptions.PreviewTapNone, BlockSurfaces.PreviewTap(StoryBlock()), "Nothing");
            yield return Click("Story preview entry 1#0");
            Assert.AreEqual(ElementOptions.PreviewEntryNone, BlockSurfaces.PreviewEntry(StoryBlock()), "None");
            Assert.AreEqual(steps + 2, _window.UndoStepsLeft, "one step each");
            yield return _window.PressUndo();
            Assert.AreEqual(ElementOptions.PreviewEntryRise, BlockSurfaces.PreviewEntry(StoryBlock()));
            yield return UndoesInOneStep(steps, "Tap");
            Assert.AreEqual(ElementOptions.PreviewTapOpenPage, BlockSurfaces.PreviewTap(StoryBlock()));
        }
    }
}
