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
using TileStories.Tests;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Editor.Tests
{
    // Phase B of the story's Editor (_3.3 S3a PART 3): while Play Mode runs the REAL wall, The Painting's story page is open on the card, and
    // REAL clicks and typing in the REAL POI Editor window change it live -- through the path every Card Content row uses
    // (DrawConfigMutationScope -> the card applier -> WallSession.ApplyCardSettings -> the host's rebind, the page reopened over it): a row's
    // words, its tone, its motion, a reorder, a delete. Read off the OPEN page each time; real Ctrl+Z presses walk it all back.
    public class LivePlayModeStoryEditorTests
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;
        private const string WallScenePath = "Assets/Apps/LivingRoom/LivingRoomScene.unity";
        private const string Poi = ReferenceCard.Golden;
        private static readonly string StoryKey = ReferenceCard.BlockOf(BuiltInBlocks.StoryKind);
        private PoiEditorWindowHost _window;
        private PoiCardSheetView _sheet;

        [TearDown]
        public void TearDown()
        {
            _window?.Close();
            _window = null;
        }

        private static IEnumerator Frames(int count)
        {
            for (int i = 0; i < count; i++) yield return null;
        }

        // The story's page as the open card draws it now
        private ComposedBlockView Page
        {
            get
            {
                Assert.IsTrue(_sheet.FullView.IsOpen, "the story's page is still open on the card");
                Assert.AreEqual(StoryKey, _sheet.FullView.BlockKey, "...on the story");
                return (ComposedBlockView)_sheet.FullView.View;
            }
        }

        private List<string> ShownIds => Page.Shown.Select(s => s.Row.Id).ToList();
        private VisualElement RootOf(string id) => Page.Shown.Single(s => s.Row.Id == id).Component.Root;
        private string TextOf(string id) => ((LineElement)Page.Shown.Single(s => s.Row.Id == id).Component).Text.text;

        private IEnumerator Click(string probeKey)
        {
            yield return _window.ScrollTo(probeKey);
            _window.Click(probeKey);
            yield return _window.WaitForRepaint();
            yield return Frames(15);
        }

        [UnityTest]
        public IEnumerator RealEdits_InTheStorysEditor_WhilePlayModeRuns_ChangeTheOpenPage_AndCtrlZWalksThemBack()
        {
            LogAssert.ignoreFailingMessages = true;
            Assert.IsFalse(EditorApplication.isPlaying, "precondition: Edit Mode");
            yield return new EnterPlayMode(expectDomainReload: false);
            try
            {
                yield return EditorSceneManager.LoadSceneAsyncInPlayMode(WallScenePath, new LoadSceneParameters(LoadSceneMode.Single));
                var editorRig = GameObject.Find("POIEditorRig");
                if (editorRig != null) UnityEngine.Object.Destroy(editorRig);
                WallSession session = null;
                PoiCardHost card = null;
                for (int i = 0; i < 900 && (session == null || session.SearchPois.Count == 0 || card == null || card.Sheet == null); i++)
                {
                    session = UnityEngine.Object.FindFirstObjectByType<WallSession>();
                    card = UnityEngine.Object.FindFirstObjectByType<PoiCardHost>();
                    yield return null;
                }
                Assert.IsNotNull(session, "the wall scene runs a WallSession");
                SelectionEventBus.Select(Poi);
                yield return Frames(20);
                _sheet = card.Sheet;
                _sheet.SetStop(SheetStopRule.Stop.Full);
                yield return Frames(20);
                _sheet.OpenFullView(StoryKey);
                yield return Frames(20);
                var before = ShownIds;
                Assert.AreEqual(17, before.Count, "precondition: the story's page draws its 17 rows");
                string title = TextOf("el_2");

                // - the window the developer edits in: the config the scene runs, the story's title row open with its parts
                var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "LivingRoom/config.json")));
                _window = new PoiEditorWindowHost(config, "_showPoiCardContent");
                var tabField = typeof(POIEditorToolWindow).GetField("_selectedTab", Instance);
                _window.SetWindowField("_selectedTab", Enum.Parse(tabField.FieldType, "SpecificMarker"));
                var t = typeof(POIEditorToolWindow);
                ((Dictionary<string, bool>)t.GetField("_poiFoldouts", Instance).GetValue(_window.Editor))[Poi] = true;
                ((Dictionary<string, bool>)t.GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor))[Poi + "/" + StoryKey] = true;
                var open = (Dictionary<string, bool>)t.GetField("_composedOpen", Instance).GetValue(_window.Editor);
                foreach (string part in new[] { "row/el_2", "row/el_2/Look", "row/el_2/Motion" }) open[POIEditorToolWindow.ComposedKey(Poi, StoryKey, part)] = true;
                yield return _window.WaitForRepaint();
                int steps = _window.UndoStepsLeft;

                // - its words: typed in the real field
                yield return _window.ScrollTo("Story page text en#2");
                yield return _window.ReplaceText("Story page text en#2", "Two hundred and fifty years, live");
                yield return _window.ClickAway();
                yield return Frames(15);
                // - the label's text carries the component's own leading tag before the words (ElementStyle.WithLeading)
                StringAssert.EndsWith("Two hundred and fifty years, live", TextOf("el_2"), "the open page shows the typed title");

                // - its tone: Accent
                int accent = Array.IndexOf(new[] { BuiltInElements.ToneNeutral, BuiltInElements.ToneFamily, BuiltInElements.ToneAccent, BuiltInElements.ToneInverse },
                    BuiltInElements.ToneAccent);
                yield return Click("Story page look tone " + accent + "#2");
                Assert.IsTrue(RootOf("el_2").ClassListContains(ElementStyle.RootClass + "--tone-accent"), "the open page's title wears the Accent tone");

                // - its motion: None (Auto = a reveal before)
                Assert.AreEqual(ElementOptions.MotionReveal, Page.Motion.EffectOf(RootOf("el_2")), "precondition: the title is revealed (Auto)");
                yield return Click("Story page motion effect 1#2");
                Assert.AreEqual("", Page.Motion.EffectOf(RootOf("el_2")), "None: the open page's title does not move any more");

                // - one step down, then the lede below it deleted
                yield return Click("Story page down#2");
                var moved = ShownIds;
                Assert.AreEqual(moved.IndexOf("el_3") + 1, moved.IndexOf("el_2"), "the title now under the lede, on the open page");
                yield return Click("Story page delete#2");
                CollectionAssert.DoesNotContain(ShownIds, "el_3", "the deleted lede is gone from the open page");
                Assert.AreEqual(16, ShownIds.Count);

                // - every edit one step: five Ctrl+Z give the shipped page back, live
                Assert.AreEqual(steps + 5, _window.UndoStepsLeft, "five edits, five undo steps");
                for (int i = 0; i < 5; i++)
                {
                    yield return _window.PressUndo();
                    yield return Frames(15);
                }
                CollectionAssert.AreEqual(before, ShownIds, "Ctrl+Z x5: the page's rows and order are the shipped ones");
                Assert.AreEqual(title, TextOf("el_2"), "its title's words");
                Assert.IsFalse(RootOf("el_2").ClassListContains(ElementStyle.RootClass + "--tone-accent"), "its tone");
                Assert.AreEqual(ElementOptions.MotionReveal, Page.Motion.EffectOf(RootOf("el_2")), "its motion");
            }
            finally
            {
                _window?.Close();
                _window = null;
            }
            yield return new ExitPlayMode();
        }

        // _3.3 S3b, live: while Play Mode runs, a block added from a template by a REAL pick reaches the running card's point, and the story's
        // Tap / Block Entry rows changed by REAL clicks rebind its preview on the card (the entry no longer rises); Ctrl+Z walks it back
        [UnityTest]
        public IEnumerator FromATemplate_AndThePreviewsTapAndEntry_WhilePlayModeRuns_ReachTheRunningCard_AndCtrlZWalksThemBack()
        {
            LogAssert.ignoreFailingMessages = true;
            Assert.IsFalse(EditorApplication.isPlaying, "precondition: Edit Mode");
            yield return new EnterPlayMode(expectDomainReload: false);
            try
            {
                yield return EditorSceneManager.LoadSceneAsyncInPlayMode(WallScenePath, new LoadSceneParameters(LoadSceneMode.Single));
                var editorRig = GameObject.Find("POIEditorRig");
                if (editorRig != null) UnityEngine.Object.Destroy(editorRig);
                WallSession session = null;
                PoiCardHost card = null;
                for (int i = 0; i < 900 && (session == null || session.SearchPois.Count == 0 || card == null || card.Sheet == null); i++)
                {
                    session = UnityEngine.Object.FindFirstObjectByType<WallSession>();
                    card = UnityEngine.Object.FindFirstObjectByType<PoiCardHost>();
                    yield return null;
                }
                Assert.IsNotNull(session, "the wall scene runs a WallSession");
                SelectionEventBus.Select(Poi);
                yield return Frames(20);
                _sheet = card.Sheet;
                _sheet.SetStop(SheetStopRule.Stop.Full);
                yield return Frames(20);
                ComposedBlockView Preview() => (ComposedBlockView)_sheet.Stack.ViewOfBlock(StoryKey);
                Assert.AreEqual(ElementOptions.MotionReveal, Preview().Motion.EffectOf(Preview().Root), "precondition: the story's preview rises in");

                var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "LivingRoom/config.json")));
                _window = new PoiEditorWindowHost(config, "_showPoiCardContent");
                var tabField = typeof(POIEditorToolWindow).GetField("_selectedTab", Instance);
                _window.SetWindowField("_selectedTab", Enum.Parse(tabField.FieldType, "SpecificMarker"));
                var t = typeof(POIEditorToolWindow);
                ((Dictionary<string, bool>)t.GetField("_poiFoldouts", Instance).GetValue(_window.Editor))[Poi] = true;
                ((Dictionary<string, bool>)t.GetField("_cardBlockFoldouts", Instance).GetValue(_window.Editor))[Poi + "/" + StoryKey] = true;
                yield return _window.WaitForRepaint();
                int steps = _window.UndoStepsLeft;
                int blocks = _sheet.Context.Poi.card.blocks.Count;

                // - Block Entry None, Tap Nothing: real clicks; the rebound preview is simply there
                yield return Click("Story preview entry 1#0");
                Assert.AreEqual("", Preview().Motion.EffectOf(Preview().Root), "the running preview no longer rises in");
                yield return Click("Story preview tap 1#0");
                Assert.AreEqual(ElementOptions.PreviewTapNone, BlockSurfaces.PreviewTap(_sheet.Context.Poi.card.blocks.Single(b => b.key == StoryKey)),
                    "the running card's story taps open nothing but its Open Row");

                // - "+ From a template...": a real pick of Story -- short
                var seen = new Dictionary<int, Rect>();
                NamedOptionPickerPopup.RowRectProbe = (i, r) => seen[i] = r;
                yield return _window.ScrollTo("Card add from template#0");
                _window.Click("Card add from template#0");
                var popup = EditorPopup.FindOpen(BlockTemplates.PickerKind);
                Assert.IsNotNull(popup, "the template picker opens");
                for (int frame = 0; frame < 90 && !seen.ContainsKey(1); frame++) { popup.Repaint(); yield return null; }
                popup.SendEvent(new Event { type = EventType.MouseDown, button = 0, clickCount = 1, mousePosition = seen[1].center });
                yield return _window.WaitForRepaint();
                yield return Frames(15);
                var added = _sheet.Context.Poi.card.blocks.Last();
                Assert.AreEqual(blocks + 1, _sheet.Context.Poi.card.blocks.Count, "the running card's point holds the new block");
                Assert.AreEqual(BlockTemplates.ShortKey, BlockSurfaces.TemplateOf(added));
                Assert.AreEqual(4, BlockSurfaces.PageRows(added, BuiltInBlocks.Story).Count, "with the template's four rows");

                Assert.AreEqual(steps + 3, _window.UndoStepsLeft, "three edits, three undo steps");
                for (int i = 0; i < 3; i++)
                {
                    yield return _window.PressUndo();
                    yield return Frames(15);
                }
                Assert.AreEqual(blocks, _sheet.Context.Poi.card.blocks.Count, "Ctrl+Z: the block left the running card");
                Assert.AreEqual(ElementOptions.MotionReveal, Preview().Motion.EffectOf(Preview().Root), "Ctrl+Z: the preview rises in again");
            }
            finally
            {
                NamedOptionPickerPopup.RowRectProbe = null;
                EditorPopup.CloseAll();
                _window?.Close();
                _window = null;
            }
            yield return new ExitPlayMode();
        }
    }
}
