using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using TileStories.Tests;
using UnityEngine.TestTools;

namespace TileStories.Editor.Tests
{
    // The story's Editor (_3.3 S3a: R14.5, R14.8, R17.1, R17.3, R17.4, R17.5, R17.7) on the REAL POI Editor window with REAL clicks, typing
    // and a drag, each a one-tick gesture (40-testing 4.2.3): Specific Marker > The Painting > Card Content > its shipped story (17 rows).
    // Every action is read back from the config AND through the card's own rules (ElementPreviewRule, BlockSurfaces, PageChromeRule), is one
    // undo step taken back by a real Ctrl+Z, and lights the unsaved flag; drawing alone writes nothing.
    public class StoryEditorTabTests
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;
        private const string Poi = ReferenceCard.Golden;
        private static readonly string StoryKey = ReferenceCard.BlockOf(BuiltInBlocks.StoryKind);
        private PoiEditorWindowHost _window;

        [TearDown]
        public void TearDown() => _window?.Close();

        // The window on the shipped wall, The Painting's Card Content open with its story block open (and what else a test asks open)
        private IEnumerator OpenTheStory(params string[] openParts)
        {
            _window = new PoiEditorWindowHost(ShippedWall.WithFrameworkNavigation(), "_showPoiCardContent");
            var tabField = typeof(POIEditorToolWindow).GetField("_selectedTab", Instance);
            _window.SetWindowField("_selectedTab", Enum.Parse(tabField.FieldType, "SpecificMarker"));
            Field<Dictionary<string, bool>>("_poiFoldouts")[Poi] = true;
            Field<Dictionary<string, bool>>("_cardBlockFoldouts")[Poi + "/" + StoryKey] = true;
            foreach (string part in openParts) Composed[POIEditorToolWindow.ComposedKey(Poi, StoryKey, part)] = true;
            yield return _window.WaitForRepaint();
            Assert.AreEqual(17, Page.Count, "precondition: the shipped story's 17 rows");
        }

        private T Field<T>(string name) => (T)typeof(POIEditorToolWindow).GetField(name, Instance).GetValue(_window.Editor);
        private Dictionary<string, bool> Composed => Field<Dictionary<string, bool>>("_composedOpen");

        // Undo replaces the config: always the live story
        private BlockInstanceData Story => _window.Config.pois.Single(p => p.id == Poi).card.blocks.Single(b => b.key == StoryKey);
        private static BlockKindDefinition Kind => BlockRegistry.Shared.TryGet(BuiltInBlocks.StoryKind, out var k) ? k : null;
        private IReadOnlyList<BlockItemData> Page => BlockSurfaces.PageRows(Story, Kind);
        private List<string> PageIds => Page.Select(r => new ElementRow(r).Id).ToList();
        private ElementRow Row(string id) => new(Page.Single(r => new ElementRow(r).Id == id));
        private ElementPreviewRule.Result Preview =>
            ElementPreviewRule.Of(ElementScope.Of(_window.Config.pois.Single(p => p.id == Poi), Story, BlockRegistry.Shared), ElementRegistry.Shared);
        private string ShowsLine => POIEditorToolWindow.StoryShowsLine(Preview, Page, ElementRegistry.Shared);

        // A real click on a probed control, scrolled into view first (the host is clamped to the screen)
        private IEnumerator Click(string probeKey)
        {
            yield return _window.ScrollTo(probeKey);
            _window.Click(probeKey);
            yield return _window.WaitForRepaint();
        }

        // The rows of the Page table that wear the P mark now, as the window drew them
        private List<int> RowsMarkedP()
        {
            var marked = new List<int>();
            for (int i = 0; i < Page.Count; i++)
                if (_window.ScreenRects.ContainsKey("Story page preview mark#" + i)) marked.Add(i);
            return marked;
        }

        private IEnumerator FreshProbes()
        {
            _window.ScreenRects.Clear();
            yield return _window.WaitForRepaint();
        }

        // One undo step taken back by a real Ctrl+Z
        private IEnumerator UndoesInOneStep(int stepsBefore, string what)
        {
            Assert.AreEqual(stepsBefore + 1, _window.UndoStepsLeft, what + ": one undo step");
            Assert.IsTrue(_window.Unsaved, what + ": Save lights up");
            yield return _window.PressUndo();
            Assert.AreEqual(stepsBefore, _window.UndoStepsLeft, what + ": Ctrl+Z took it back");
        }

        [UnityTest]
        public IEnumerator TheStory_DrawsItsPageFirst_ThenItsPreview_NumbersEveryRow_MarksTheAutomaticPicks_AndCountsTheRowsToValidate()
        {
            yield return OpenTheStory();
            Assert.Less(_window.RectOf("Story page section#0").y, _window.RectOf("Story preview section#0").y, "the Page first, then the Preview (R17.7)");
            for (int i = 0; i < 17; i++) _window.RectOf("Story page element#" + i);
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3 }, RowsMarkedP(), "P on the picture, eyebrow, title and lede the Automatic preview takes");
            Assert.AreEqual("Picture #1, Eyebrow #2, Title #3, Lede #4", ShowsLine, "the Preview's live line");
            _window.RectOf("Story preview shows#0");
            // - the block row's validation words, probed by the story's own row in The Painting's list (never a typed index)
            _window.RectOf("Card block validation#" + _window.Config.pois.Single(p => p.id == Poi).card.blocks.FindIndex(b => b.key == StoryKey));
            Assert.AreEqual(17, BlockSurfaces.RowsToValidate(Story, Kind));
            Assert.AreEqual("17 to validate", POIEditorToolWindow.StoryValidationWords(17));
            Assert.AreEqual("all validated", POIEditorToolWindow.StoryValidationWords(0));
            Assert.IsFalse(_window.Unsaved, "drawing the whole story writes nothing");
            Assert.AreEqual(0, _window.UndoStepsLeft);
        }

        [UnityTest]
        public IEnumerator Validated_ARealClickMarksARow_TheHeaderCountFollows_AllRowsMarkedReadAllValidated_AndCtrlZ()
        {
            yield return OpenTheStory();
            int steps = _window.UndoStepsLeft;
            yield return Click("Story page validated#2");
            Assert.IsTrue(Row("el_2").Validated, "the title row is marked");
            Assert.AreEqual(16, BlockSurfaces.RowsToValidate(Story, Kind));
            yield return UndoesInOneStep(steps, "Validated");
            Assert.IsFalse(Row("el_2").Validated);

            for (int i = 0; i < 17; i++) yield return Click("Story page validated#" + i);
            Assert.AreEqual(0, BlockSurfaces.RowsToValidate(Story, Kind), "every row marked: the header reads " + POIEditorToolWindow.StoryValidationWords(0));
            yield return Click("Story page validated#16");
            Assert.AreEqual(1, BlockSurfaces.RowsToValidate(Story, Kind), "a second click takes the mark off");
        }

        [UnityTest]
        public IEnumerator MoveDownUp_AndARealDrag_ReorderThePage_ThePMarksAndShowsFollow_AndCtrlZ()
        {
            yield return OpenTheStory();
            int steps = _window.UndoStepsLeft;
            yield return Click("Story page down#0");
            CollectionAssert.AreEqual(new[] { "el_1", "el_12" }, PageIds.Take(2), "the picture went one down");
            yield return FreshProbes();
            // - the eyebrow no longer touches the title (the picture is between them): it leads nothing, the preview drops it
            Assert.AreEqual("Picture #2, Title #3, Lede #4", ShowsLine, "the preview follows the Page");
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, RowsMarkedP(), "and the P marks with it");
            yield return UndoesInOneStep(steps, "Move down");
            Assert.AreEqual("el_12", PageIds[0]);

            // - a real drag of the video's grip (row 17) above the first row, press / drag / release in ONE editor tick
            yield return _window.ScrollTo("Story page handle#0");
            var from = _window.RectOf("Story page handle#16");
            var to = _window.RectOf("Story page handle#0");
            Assert.That(_window.Local(from.center).y, Is.InRange(20f, _window.VisibleHeight - 40f), "precondition: the video's grip is on screen");
            _window.Send(new Event { type = EventType.MouseDown, button = 0, clickCount = 1, mousePosition = _window.Local(from.center) });
            _window.PaintNow();
            _window.Send(new Event { type = EventType.MouseDrag, button = 0, mousePosition = _window.Local(to.center - new Vector2(0f, 8f)) });
            _window.PaintNow();
            _window.Send(new Event { type = EventType.MouseUp, button = 0, clickCount = 1, mousePosition = _window.Local(to.center - new Vector2(0f, 8f)) });
            _window.PaintNow();
            yield return _window.WaitForRepaint();
            Assert.AreEqual("el_17", PageIds[0], "the video was dropped first");
            Assert.AreEqual(17, Page.Count, "nothing lost");
            yield return FreshProbes();
            Assert.AreEqual("Video #1, Eyebrow #3, Title #4, Lede #5", ShowsLine, "a video plays the preview's picture now");
            yield return UndoesInOneStep(steps, "Drag");
            Assert.AreEqual("el_12", PageIds[0], "Ctrl+Z put the video back at the end");
            Assert.AreEqual("el_17", PageIds[16]);
        }

        [UnityTest]
        public IEnumerator DuplicateAndDelete_ARealClickEach_NewIdsAreNeverReused_ThePreviewFollowsADelete_AndCtrlZ()
        {
            yield return OpenTheStory();
            int steps = _window.UndoStepsLeft;
            yield return Click("Story page duplicate#4");
            Assert.AreEqual(18, Page.Count);
            var copy = new ElementRow(Page[5]);
            Assert.AreEqual("el_18", copy.Id, "the copy right under its source, a new id");
            Assert.AreEqual(BuiltInElements.Paragraph, copy.Kind);
            Assert.IsFalse(copy.Validated, "a copy is checked again");
            Assert.IsTrue(Composed[POIEditorToolWindow.ComposedKey(Poi, StoryKey, "row/el_18")], "the copy opens");
            yield return UndoesInOneStep(steps, "Duplicate");
            Assert.AreEqual(17, Page.Count, "Ctrl+Z: the copy is gone (and its id with it: it never existed)");

            yield return Click("Story page delete#0");
            Assert.AreEqual(16, Page.Count);
            CollectionAssert.DoesNotContain(PageIds, "el_12");
            yield return FreshProbes();
            Assert.AreEqual("Eyebrow #1, Title #2, Lede #3, Picture #5", ShowsLine, "the next picture takes the deleted one's place");
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 4 }, RowsMarkedP(), "and wears the P");
            yield return _window.PressUndo();
            Assert.AreEqual(17, Page.Count);
            Assert.AreEqual("el_12", PageIds[0], "Ctrl+Z: the deleted picture is back first");

            // - a row that existed and was deleted keeps its id taken: a link may still name it
            yield return Click("Story page duplicate#0");
            Assert.AreEqual("el_18", new ElementRow(Page[1]).Id);
            yield return Click("Story page delete#1");
            yield return Click("Story page duplicate#0");
            Assert.AreEqual("el_19", new ElementRow(Page[1]).Id, "el_18 was deleted, never handed out again");
        }

        [UnityTest]
        public IEnumerator AddElement_ThePaletteByCategory_ANewRowIsBornOpenWithItsDefaults_ItsEmptyFieldsMarked_TypingFillsIt_AndCtrlZ()
        {
            yield return OpenTheStory();
            int steps = _window.UndoStepsLeft;
            yield return Click("Story page add#0");
            foreach (var element in ElementRegistry.Shared.All) _window.RectOf("Story page add " + element.Key + "#0");
            Assert.AreEqual(steps, _window.UndoStepsLeft, "opening the palette is no edit");
            yield return Click("Story page add " + BuiltInElements.Callout + "#0");
            Assert.AreEqual(18, Page.Count);
            var added = new ElementRow(Page[17]);
            Assert.AreEqual(BuiltInElements.Callout, added.Kind);
            Assert.AreEqual("el_18", added.Id);
            Assert.AreEqual(BuiltInElements.ToneAccent, added.Value(BuiltInElements.ToneStyle), "born with its designed tone");
            foreach (string part in new[] { "", "/Look", "/Motion" })
                Assert.IsTrue(Composed[POIEditorToolWindow.ComposedKey(Poi, StoryKey, "row/el_18" + part)], "born open: row" + part);
            var empty = ElementValidation.Of(ElementScope.Of(_window.Config.pois.Single(p => p.id == Poi), Story, BlockRegistry.Shared), ElementRegistry.Shared)
                .Where(p => p.RowId == "el_18").ToList();
            Assert.AreEqual(1, empty.Count, "its one required field is empty (marked on the window)");
            Assert.AreEqual(BuiltInElements.TextField, empty[0].FieldKey);
            yield return UndoesInOneStep(steps, "Add element");
            yield return _window.PressRedo();

            yield return _window.ScrollTo("Story page text en#17");
            yield return _window.ReplaceText("Story page text en#17", "Do not miss the chapel");
            yield return _window.ClickAway();
            Assert.AreEqual("Do not miss the chapel", BlockFieldReader.Pick(new ElementRow(Page[17]).Field(BuiltInElements.TextField).text, "en", "en"));
            Assert.IsFalse(ElementValidation.Of(ElementScope.Of(_window.Config.pois.Single(p => p.id == Poi), Story, BlockRegistry.Shared), ElementRegistry.Shared)
                .Any(p => p.RowId == "el_18"), "complete once written");
            yield return _window.PressUndo();
            Assert.AreEqual("", BlockFieldReader.Pick(new ElementRow(Page[17]).Field(BuiltInElements.TextField)?.text, "en", "en"), "Ctrl+Z took the typing back");

            // - a media element: no picture yet, then a real "+ Add picture" gives it its first item
            yield return Click("Story page add#0");
            yield return Click("Story page add " + BuiltInElements.Picture + "#0");
            var picture = new ElementRow(Page[18]);
            Assert.AreEqual(BuiltInElements.Picture, picture.Kind);
            Assert.AreEqual(0, picture.Media.Count);
            yield return Click("Story page media add#18");
            Assert.AreEqual(1, new ElementRow(Page[18]).Media.Count, "one picture item to fill");
        }

        [UnityTest]
        public IEnumerator LookMotionTap_TheInlineParts_ARealClickOnASegmentSetsTheRowsValue_EachOneUndoStep()
        {
            yield return OpenTheStory("row/el_12", "row/el_12/Look", "row/el_12/Motion", "row/el_12/Tap");
            int steps = _window.UndoStepsLeft;
            Assert.AreEqual(BuiltInElements.WidthFullBleed, Row("el_12").Value(BuiltInElements.WidthStyle), "precondition: the shipped hero is edge to edge");
            yield return Click("Story page look width 0#0");
            Assert.AreEqual(BuiltInElements.WidthInset, Row("el_12").Value(BuiltInElements.WidthStyle), "Inset picked");
            Assert.AreEqual(steps + 1, _window.UndoStepsLeft, "width: its own step");
            yield return Click("Story page look frame 1#0");
            Assert.AreEqual(BuiltInElements.FrameArch, Row("el_12").Value(BuiltInElements.FrameStyle));
            Assert.AreEqual(steps + 2, _window.UndoStepsLeft, "frame: its own step");
            int slowZoom = Array.IndexOf(ElementOptions.Motions, ElementOptions.MotionSlowZoom);
            // - a picture's 7 effects are one dropdown (S3b): a real click opens its list, a real click on an option picks it
            yield return Click("Story page motion effect#0");
            Assert.AreEqual(steps + 2, _window.UndoStepsLeft, "opening the list writes nothing");
            yield return Click("Story page motion effect " + slowZoom + "#0");
            Assert.AreEqual(ElementOptions.MotionSlowZoom, Row("el_12").Motion);
            Assert.AreEqual(steps + 3, _window.UndoStepsLeft, "effect: its own step");
            Assert.IsTrue(ElementRegistry.Shared.TryGet(BuiltInElements.Picture, out var picture));
            Assert.AreEqual(ElementOptions.MotionSlowZoom, ElementMotionRule.EffectOf(Row("el_12"), picture), "the card plays it");
            yield return Click("Story page motion strength 1#0");
            Assert.AreEqual(ElementOptions.StrengthStrong, Row("el_12").Strength);
            Assert.AreEqual(steps + 4, _window.UndoStepsLeft, "strength: its own step");
            yield return Click("Story page tap 1#0");
            Assert.AreEqual(ElementOptions.TapNone, ElementRules.TapOf(Row("el_12"), picture), "a tap on the picture does nothing now");
            Assert.AreEqual(steps + 5, _window.UndoStepsLeft, "five clicks, five undo steps");
            for (int i = 0; i < 5; i++) yield return _window.PressUndo();
            Assert.AreEqual(ElementOptions.TapOpenViewer, ElementRules.TapOf(Row("el_12"), picture), "Ctrl+Z x5: the tap...");
            Assert.AreEqual(ElementOptions.MotionAuto, Row("el_12").Motion, "...the motion...");
            Assert.AreEqual(BuiltInElements.WidthFullBleed, Row("el_12").Value(BuiltInElements.WidthStyle), "...and the look are back");
        }

        [UnityTest]
        public IEnumerator TheMotionTable_EditsTheSameValueAsTheRowsMotionPart_OneLinePerRow_ReducedReadOnly()
        {
            yield return OpenTheStory("motion", "row/el_2", "row/el_2/Motion");
            for (int i = 0; i < 17; i++) _window.RectOf("Story motion effect#" + i);
            int steps = _window.UndoStepsLeft;
            yield return Click("Story motion effect#2");
            Assert.AreEqual(ElementOptions.MotionNone, Row("el_2").Motion, "Auto -> None: the next motion the title offers");
            yield return FreshProbes();
            // - the row's own Motion part reads the same value: its segment for None is the selected one (index 1 of the title's choices)
            Assert.IsTrue(ElementRegistry.Shared.TryGet(BuiltInElements.Title, out var title));
            Assert.AreEqual(1, title.MotionChoices.ToList().IndexOf(Row("el_2").Motion));
            yield return Click("Story motion effect#2");
            Assert.AreEqual(ElementOptions.MotionReveal, Row("el_2").Motion);
            yield return Click("Story page motion effect 3#2");
            Assert.AreEqual(ElementOptions.MotionSlideIn, Row("el_2").Motion, "and the part's click shows in the table's chip (the same field)");
            Assert.AreEqual("Slide In", POIEditorToolWindow.StoryMotionWords(Row("el_2"), title));
            Assert.AreEqual(steps + 3, _window.UndoStepsLeft);
            yield return _window.PressUndo();
            yield return _window.PressUndo();
            yield return _window.PressUndo();
            Assert.AreEqual(ElementOptions.MotionAuto, Row("el_2").Motion);
        }

        [UnityTest]
        public IEnumerator Custom_UseLinked_Copy_ABrokenLinkIsAWarningRow_UnlinkAndRemove_EachARealClick_AndCtrlZ()
        {
            yield return OpenTheStory("row/el_6");
            int steps = _window.UndoStepsLeft;
            yield return Click("Story preview mode 1#0");
            Assert.AreEqual(ElementOptions.PreviewCustom, BlockSurfaces.PreviewMode(Story));
            Assert.AreEqual(4, BlockSurfaces.PreviewRows(Story).Count, "Custom starts as the Automatic preview showed it: four links");
            yield return UndoesInOneStep(steps, "Custom");
            Assert.AreEqual(ElementOptions.PreviewAutomatic, BlockSurfaces.PreviewMode(Story));

            // - from an open Page row: Use in the preview (linked), then Copy to the preview
            yield return Click("Story page use linked#7");
            Assert.AreEqual(ElementOptions.PreviewCustom, BlockSurfaces.PreviewMode(Story), "the preview turned Custom");
            Assert.AreEqual(5, BlockSurfaces.PreviewRows(Story).Count, "the four picks, then the link");
            Assert.IsTrue(new ElementRow(BlockSurfaces.PreviewRows(Story)[4]).IsRef);
            yield return FreshProbes();
            CollectionAssert.Contains(RowsMarkedP(), 7, "the linked Page row wears the P");
            yield return Click("Story page copy#7");
            var copy = new ElementRow(BlockSurfaces.PreviewRows(Story)[5]);
            Assert.IsFalse(copy.IsRef, "a copy is the preview's own row");
            Assert.AreEqual(BuiltInElements.Paragraph, copy.Kind);

            // - delete the title on the Page: its link becomes a warning row; Unlink makes an empty title, Remove takes a row away
            yield return Click("Story page delete#2");
            var broken = Preview.Entries.Single(e => e.Source == ElementPreviewRule.Source.Broken);
            int brokenAt = BlockSurfaces.PreviewRows(Story).ToList().IndexOf(broken.Row);
            yield return FreshProbes();
            _window.RectOf("Story preview unlink#" + brokenAt);
            _window.RectOf("Story preview remove#" + brokenAt);
            Assert.IsFalse(_window.ScreenRects.ContainsKey("Story preview open#" + brokenAt), "a broken row has nothing to open");
            int before = _window.UndoStepsLeft;
            yield return Click("Story preview unlink#" + brokenAt);
            var made = new ElementRow(BlockSurfaces.PreviewRows(Story)[brokenAt]);
            Assert.IsFalse(made.IsRef);
            Assert.AreEqual(BuiltInElements.Title, made.Kind, "an empty row of the element the link showed");
            Assert.IsFalse(Preview.Entries.Any(e => e.Source == ElementPreviewRule.Source.Broken), "no broken row left");
            yield return UndoesInOneStep(before, "Unlink");
            Assert.AreEqual(1, Preview.Entries.Count(e => e.Source == ElementPreviewRule.Source.Broken), "Ctrl+Z: broken again");
            int count = BlockSurfaces.PreviewRows(Story).Count;
            yield return Click("Story preview remove#" + brokenAt);
            Assert.AreEqual(count - 1, BlockSurfaces.PreviewRows(Story).Count, "Remove took it away");
            Assert.IsFalse(Preview.Entries.Any(e => e.Source == ElementPreviewRule.Source.Broken));

            // - a link of the Page row the preview does not show yet, from the + Link list
            yield return Click("Story preview link#0");
            Assert.AreEqual("el_8", PageIds[11], "precondition: the pull quote is Page row 12 once the title is gone");
            yield return Click("Story preview link page#11");
            Assert.AreEqual(BuiltInElements.PullQuote, new ElementRow(BlockSurfaces.PreviewRows(Story).Last()).Kind, "the pull quote linked");
        }

        [UnityTest]
        public IEnumerator OpenRowAndPageChrome_Overrides_TurnedOnSeedFromTheWall_ARealClickChangesThem_InheritKeepsThem_AndCtrlZ()
        {
            yield return OpenTheStory();
            var wall = _window.Config.card_settings.components;
            Assert.AreEqual(ElementOptions.EmphasisNudge, wall.open_row_emphasis, "precondition: the wall's Open Row nudges");
            int steps = _window.UndoStepsLeft;
            yield return Click("Story open row 1#0");
            var own = BlockSurfaces.OverrideOf(Story, ElementOptions.ComponentOpenRow);
            Assert.IsTrue(own.IsOn);
            Assert.AreEqual(ElementOptions.EmphasisNudge, own.Emphasis, "seeded from the wall's");
            yield return Click("Story open row emphasis 2#0");
            Assert.AreEqual(ElementOptions.EmphasisShine, BlockSurfaces.OverrideOf(Story, ElementOptions.ComponentOpenRow).Emphasis);
            yield return Click("Story open row 0#0");
            Assert.IsFalse(BlockSurfaces.OverrideOf(Story, ElementOptions.ComponentOpenRow).IsOn, "Inherit: the wall's again");
            Assert.AreEqual(ElementOptions.EmphasisShine, BlockSurfaces.OverrideOf(Story, ElementOptions.ComponentOpenRow).Emphasis, "its own value kept");
            Assert.AreEqual(steps + 3, _window.UndoStepsLeft);

            yield return Click("Story page chrome 1#0");
            Assert.IsTrue(PageChromeRule.Of(wall, Story).Breadcrumb, "seeded: the wall shows the breadcrumb");
            yield return Click("Story page chrome breadcrumb 1#0");
            var chrome = PageChromeRule.Of(_window.Config.card_settings.components, Story);
            Assert.IsFalse(chrome.Breadcrumb, "this block's page hides it");
            Assert.IsTrue(chrome.PrevNext);
            yield return _window.PressUndo();
            Assert.IsTrue(PageChromeRule.Of(_window.Config.card_settings.components, Story).Breadcrumb, "Ctrl+Z");
        }

        [UnityTest]
        public IEnumerator ReadingTime_AutoOrMinutesByHand_SwitchingStartsFromTheAutoCount_TypedMinutesWin_AndCtrlZ()
        {
            yield return OpenTheStory();
            var scope = ElementScope.Of(_window.Config.pois.Single(p => p.id == Poi), Story, BlockRegistry.Shared);
            int auto = ElementRules.Minutes(scope, ElementRegistry.Shared, "en", "en");
            Assert.Greater(auto, 0);
            int steps = _window.UndoStepsLeft;
            yield return Click("Story reading time 1#0");
            Assert.AreEqual(auto, BlockSurfaces.MinutesSetByHand(Story), "by hand starts from the Auto count: nothing changes on the card");
            yield return _window.ScrollTo("Story reading time minutes#0");
            yield return _window.ReplaceText("Story reading time minutes#0", "9");
            yield return _window.ClickAway();
            Assert.AreEqual(9, BlockSurfaces.MinutesSetByHand(Story));
            Assert.AreEqual(9, ElementRules.Minutes(ElementScope.Of(_window.Config.pois.Single(p => p.id == Poi), Story, BlockRegistry.Shared), ElementRegistry.Shared, "en", "en"));
            yield return Click("Story reading time 0#0");
            Assert.AreEqual(0, BlockSurfaces.MinutesSetByHand(Story), "Auto again");
            Assert.AreEqual(steps + 3, _window.UndoStepsLeft);
            yield return _window.PressUndo();
            Assert.AreEqual(9, BlockSurfaces.MinutesSetByHand(Story), "Ctrl+Z");
        }

        // Every text the story's Editor shows a developer -- (i) helps, tooltips, notes -- and every element's help, labels and option labels:
        // ASCII, app-agnostic (no wall's point, no doc or code file), never empty
        [Test]
        public void EveryStoryEditorText_IsAsciiAndAppAgnostic_AndEveryElementHasHelpOnEveryOption()
        {
            var texts = typeof(POIEditorToolWindow).GetFields(BindingFlags.NonPublic | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(string) && f.Name.StartsWith("Story") && f.IsLiteral)
                .ToDictionary(f => f.Name, f => (string)f.GetValue(null));
            Assert.GreaterOrEqual(texts.Count, 30, "precondition: the story's texts are found");
            foreach (string required in new[] { "StoryPageHelp", "StoryPreviewHelp", "StoryMotionTableHelp", "StoryOpenRowOverrideHelp", "StoryReadingTimeHelp",
                         "StoryAddElementHelp", "StoryCustomRowsHelp", "StoryPageChromeOverrideHelp" })
                Assert.IsTrue(texts.ContainsKey(required), required + " exists");
            foreach (var pair in texts)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(pair.Value), pair.Key + " is empty");
                EditorTextChecks.AssertAscii(pair.Value, pair.Key);
                foreach (string forbidden in new[] { ".md", ".cs", "_3.", "_5.1", "LivingRoom", "Painting", "painting", "Ribeira", "card_settings", "Assets/" })
                    StringAssert.DoesNotContain(forbidden, pair.Value, pair.Key + " names '" + forbidden + "'");
            }
            foreach (var element in ElementRegistry.Shared.All)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(element.Help), element.Key + " has a (i) text");
                EditorTextChecks.AssertAscii(element.DisplayName + element.Help, element.Key);
                foreach (var field in (element.Fields ?? Array.Empty<BlockFieldDefinition>()).Concat(element.StyleFields ?? Array.Empty<BlockFieldDefinition>()))
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(field.Label) || string.IsNullOrWhiteSpace(field.Help), element.Key + "." + field.Key + " has a label and help");
                    EditorTextChecks.AssertAscii(field.Label + field.Help + string.Join("", field.OptionLabels ?? Array.Empty<string>()), element.Key + "." + field.Key);
                }
            }
        }

        [UnityTest]
        public IEnumerator CardComponents_TheWallsOpenRowEmphasis_IsARealClick_TheStoryInheritsIt_AndCtrlZ()
        {
            _window = new PoiEditorWindowHost(ShippedWall.WithFrameworkNavigation(), "_showCardComponents");
            var tabField = typeof(POIEditorToolWindow).GetField("_selectedTab", Instance);
            _window.SetWindowField("_selectedTab", Enum.Parse(tabField.FieldType, "DetailCard"));
            yield return _window.WaitForRepaint();
            Assert.Less(_window.RectOf("Card components open row emphasis#0").y, _window.RectOf("Breadcrumb").y, "the Open Row first, then Page Chrome");
            int steps = _window.UndoStepsLeft;
            yield return Click("Card components open row emphasis 2#0");
            Assert.AreEqual(ElementOptions.EmphasisShine, _window.Config.card_settings.components.open_row_emphasis);
            yield return UndoesInOneStep(steps, "Open Row emphasis");
            Assert.AreEqual(ElementOptions.EmphasisNudge, _window.Config.card_settings.components.open_row_emphasis);
        }
    }
}
