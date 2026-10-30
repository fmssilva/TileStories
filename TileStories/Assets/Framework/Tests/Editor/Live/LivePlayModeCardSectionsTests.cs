using System;
using System.Collections;
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
    // Phase B of P3d's Editor rows (_3.3 P3c review c-d, second pass b, P2 review e): while Play Mode runs on the real wall, edits in the REAL
    // POI Editor window reach the OPEN card through the live-sync path every Detail Card row uses (DrawConfigMutationScope -> the card applier
    // -> WallSession.ApplyCardSettings): a REAL drag of Sections > Tint Depth deepens every open section's tint, Replay Once reaches its sections,
    // Navigation Bar > Locate Button off takes the button away; Ctrl+Z walks each back on the running card
    public class LivePlayModeCardSectionsTests
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;
        private const string WallScenePath = "Assets/Apps/LivingRoom/LivingRoomScene.unity";
        private PoiEditorWindowHost _window;

        [TearDown]
        public void TearDown()
        {
            EditorPopup.CloseAll();
            _window?.Close();
            _window = null;
        }

        private static IEnumerator Frames(int count)
        {
            for (int i = 0; i < count; i++) yield return null;
        }

        private void Pick(Action edit) =>
            typeof(POIEditorToolWindow).GetMethod("DrawConfigMutationScope", Instance).Invoke(_window.Editor, new object[] { edit, false });

        [UnityTest]
        public IEnumerator TintDepthReplayAndLocateButton_WhilePlayModeRuns_ReachTheOpenCard_AndCtrlZWalksThemBack()
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
                SelectionEventBus.Select("painting");
                yield return Frames(20);
                var sheet = card.Sheet;
                sheet.SetStop(SheetStopRule.Stop.Full);
                yield return Frames(20);
                Assert.IsTrue(sheet.IsOpen, "the Ribeira Palace's card is open");
                var sections = sheet.Stack.Sheets.All.Where(s => s.Root.ClassListContains(SectionSheets.OpenClass) && s.Open.Edge.style.display.value == UnityEngine.UIElements.DisplayStyle.Flex).ToList();
                Assert.Greater(sections.Count, 1, "precondition: the card's open sections");
                Assert.AreEqual(CardSectionSettings.TintDepthDefault, sections[1].Open.Style.TintDepth, "precondition: the shipped wall's 120");
                Assert.IsTrue(sheet.Locate.IsShown, "precondition: the Locate Button on the shipped wall's card");

                var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "LivingRoom/config.json")));
                _window = new PoiEditorWindowHost(config, "_showCardSections");
                _window.SetWindowField("_showCardNavigation", true);
                var tabField = typeof(POIEditorToolWindow).GetField("_selectedTab", Instance);
                _window.SetWindowField("_selectedTab", Enum.Parse(tabField.FieldType, "DetailCard"));
                yield return _window.WaitForRepaint();

                // - a real drag along Tint Depth's track, inside the track and inside one editor tick (press, moves, release): one undo step
                yield return _window.ScrollTo("Tint Depth");
                Rect row = _window.RectOf("Tint Depth");
                float y = row.center.y, startX = row.xMax - 70f;
                int steps = _window.UndoStepsLeft;
                _window.Send(new Event { type = EventType.MouseDown, button = 0, clickCount = 1, mousePosition = _window.Local(new Vector2(startX, y)) });
                _window.PaintNow();
                for (int i = 1; i <= 6; i++)
                {
                    _window.Send(new Event { type = EventType.MouseDrag, button = 0, mousePosition = _window.Local(new Vector2(startX + 20f * i, y)), delta = new Vector2(20f, 0f) });
                    _window.PaintNow();
                }
                _window.Send(new Event { type = EventType.MouseUp, button = 0, clickCount = 1, mousePosition = _window.Local(new Vector2(startX + 120f, y)) });
                _window.PaintNow();
                yield return _window.WaitForRepaint();
                yield return Frames(20);
                float dragged = _window.Config.card_settings.sections.tint_depth;
                Assert.Greater(dragged, CardSectionSettings.TintDepthDefault + 10f, "precondition: the drag deepened it (" + dragged + ")");
                // - the steps the drag took, recorded: one in its own runs, two once in a full EditMode run (watched, _Test_Suite_1 section 4;
                //   the grouping's own fixture is ConfigUndoGroupingTests)
                int dragSteps = _window.UndoStepsLeft - steps;
                File.WriteAllText(TileStories.Tests.TestEvidence.PathFor("Editor", "live_tint_depth_drag_steps.txt"), dragSteps + "\n");
                sections = sheet.Stack.Sheets.All.Where(s => s.Root.ClassListContains(SectionSheets.OpenClass) && s.Open.Edge.style.display.value == UnityEngine.UIElements.DisplayStyle.Flex).ToList();
                Assert.AreEqual(dragged, sections[1].Open.Style.TintDepth, 1e-3f, "the open card's sections read the depth");
                var stops = sections[1].Open.GroundOptions.Stops;
                Assert.AreEqual(dragged, stops.Last().Offset - stops.First().Offset, 0.5f, "...and their tint fades over it");

                // - Replay Once and Locate Button off, through the window's own edit path
                Pick(() => _window.Config.card_settings.sections.replay = CardOptions.SectionReplayOnce);
                yield return _window.WaitForRepaint();
                yield return Frames(20);
                Assert.IsTrue(sheet.Stack.Sheets.All.First(s => s.Root.ClassListContains(SectionSheets.OpenClass)).Open.Style.Once, "the open card's sections replay Once");
                Pick(() => _window.Config.card_settings.navigation.bar.locate_button = false);
                yield return _window.WaitForRepaint();
                yield return Frames(20);
                Assert.IsFalse(sheet.Locate.IsShown, "Locate Button off: the open card's button gone");

                // - Ctrl+Z three times: the button back, Every Pass back, 120 back
                yield return _window.PressUndo();
                yield return Frames(20);
                Assert.IsTrue(sheet.Locate.IsShown, "Ctrl+Z: the button back");
                yield return _window.PressUndo();
                yield return Frames(20);
                Assert.IsFalse(sheet.Stack.Sheets.All.First(s => s.Root.ClassListContains(SectionSheets.OpenClass)).Open.Style.Once, "Ctrl+Z: Every Pass again");
                // - then the drag's own step(s), back to the shipped 120
                Assert.LessOrEqual(dragSteps, 2, "the drag took " + dragSteps + " undo steps");
                for (int press = 0; press < dragSteps; press++)
                {
                    yield return _window.PressUndo();
                    yield return Frames(20);
                }
                sections = sheet.Stack.Sheets.All.Where(s => s.Root.ClassListContains(SectionSheets.OpenClass) && s.Open.Edge.style.display.value == UnityEngine.UIElements.DisplayStyle.Flex).ToList();
                Assert.AreEqual(CardSectionSettings.TintDepthDefault, sections[1].Open.Style.TintDepth, 1e-3f, "Ctrl+Z: 120 again on the card");
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
