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
    // Phase B of the navigation presets (_3.2 step 17a): a preset picked in the REAL POI Editor window while Play Mode runs changes the OPEN
    // card -- through the live-sync path every row of Detail Card > Navigation already uses (DrawConfigMutationScope -> the card applier ->
    // WallSession.ApplyCardSettings -> the host's rebind) -- and ONE Ctrl+Z gives the old card back. Read off the running card: its layout
    // and the view each of its five navigation slots really holds, against the element the preset names.
    public class LivePlayModeNavigationPresetTests
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;
        private const string WallScenePath = "Assets/Apps/LivingRoom/LivingRoomScene.unity";
        private PoiEditorWindowHost _window;

        [TearDown]
        public void TearDown()
        {
            NamedOptionPickerPopup.RowRectProbe = null;
            EditorPopup.CloseAll();
            _window?.Close();
            _window = null;
        }

        private static IEnumerator Frames(int count)
        {
            for (int i = 0; i < count; i++) yield return null;
        }

        // The view class an element key makes (what a slot holding that element holds), null for none: asked of the REAL registry
        private static Type ViewTypeOf(string key) => key == CardOptions.NavNone ? null : NavElementRegistry.Shared.CreateView(key)?.GetType();

        // What the open card holds now, against what the preset says it should
        private static void AssertCardShows(PoiCardSheetView sheet, WallSession session, NavigationPresets.Preset preset, string when)
        {
            var nav = session.CardSettings.navigation;
            Assert.AreSame(preset, NavigationPresets.Matching(nav), when + ": the running wall holds the preset's seven rows (the push reached WallSession)");
            Assert.AreEqual(preset.Layout, sheet.Layout.Effective, when + ": the open card's layout");
            Assert.AreEqual(ViewTypeOf(CardLayoutRule.TopElement(nav)), sheet.Slots.TopView?.GetType(), when + ": the top slot (Pages draws its pager, Bento none)");
            Assert.AreEqual(ViewTypeOf(preset.Bottom), sheet.Slots.BottomView?.GetType(), when + ": the bottom slot");
            Assert.AreEqual(ViewTypeOf(preset.Menu), sheet.Slots.MenuView?.GetType(), when + ": the menu");
            Assert.AreEqual(ViewTypeOf(preset.Floating), sheet.Slots.FloatingView?.GetType(), when + ": the floating slot");
            Assert.AreEqual(ViewTypeOf(preset.Edge), sheet.Slots.EdgeView?.GetType(), when + ": the edge slot");
        }

        private IEnumerator PickPreset(int index)
        {
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
            Assert.AreEqual(NavigationPresets.All.Count, seen.Count, "the picker drew every row");
            popup.SendEvent(new Event { type = EventType.MouseDown, button = 0, clickCount = 1, mousePosition = seen[index].center });
            yield return Frames(15);
        }

        [UnityTest]
        public IEnumerator ARealPresetPick_WhilePlayModeRuns_ChangesTheOpenCard_AndOneCtrlZGivesTheOldCardBack()
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
                Assert.Greater(session.SearchPois.Count, 0, "the wall spawned its POIs");
                SelectionEventBus.Select("lamp");
                yield return Frames(20);
                var sheet = card.Sheet;
                sheet.SetStop(SheetStopRule.Stop.Full);
                yield return Frames(20);
                Assert.IsTrue(sheet.IsOpen, "the card is open on a point with a long card");
                Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop);

                // - the window the developer edits in: the config the scene runs, Detail Card > Navigation open
                var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "LivingRoom/config.json")));
                // - the Navigation Bar at the framework default (off): while it is on it replaces every element a preset places,
                //   and the demo wall turns it on; the first pick below pushes this to the running card
                config.card_settings.navigation.bar.enabled = new CardNavBarSettings().enabled;
                _window = new PoiEditorWindowHost(config, "_showCardNavigation");
                var tabField = typeof(POIEditorToolWindow).GetField("_selectedTab", Instance);
                _window.SetWindowField("_selectedTab", Enum.Parse(tabField.FieldType, "DetailCard"));
                yield return _window.WaitForRepaint();

                // - the test's OWN start, never the demo wall's navigation (its developer's choice, which may match no preset --
                //   40-testing 4.2.3): a real pick of Long read, a strip of tabs on this card; every other one is picked from there
                var longRead = NavigationPresets.All[0];
                yield return PickPreset(0);
                var start = longRead;
                AssertCardShows(sheet, session, start, "the test's start (" + start.Label + ")");
                Assert.IsNotNull(sheet.Slots.TopView, "precondition: Long read shows a strip on this card");

                // - every other preset, one real pick at a time: the card follows, one Ctrl+Z gives the wall's own back
                for (int i = 0; i < NavigationPresets.All.Count; i++)
                {
                    var preset = NavigationPresets.All[i];
                    if (ReferenceEquals(preset, start)) continue;
                    int steps = _window.UndoStepsLeft;
                    yield return PickPreset(i);
                    Assert.AreEqual(steps + 1, _window.UndoStepsLeft, preset.Label + ": one pick, one undo step");
                    AssertCardShows(sheet, session, preset, preset.Label + " picked");
                    Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop, preset.Label + ": the card kept its height");
                    Assert.AreEqual("lamp", card.ShownPoiId, preset.Label + ": and its point");

                    yield return _window.PressUndo();
                    yield return Frames(15);
                    Assert.AreEqual(steps, _window.UndoStepsLeft, preset.Label + ": Ctrl+Z took that one step");
                    AssertCardShows(sheet, session, start, preset.Label + " undone");
                }

                // - from another preset back to Long read: the card returns to its strip of tabs, and Ctrl+Z gives the other one back
                var camera = NavigationPresets.All.Single(p => p.Label == "Over the camera");
                yield return PickPreset(NavigationPresets.All.ToList().IndexOf(camera));
                AssertCardShows(sheet, session, camera, "Over the camera picked");
                yield return PickPreset(0);
                AssertCardShows(sheet, session, longRead, "Long read picked again");
                yield return _window.PressUndo();
                yield return Frames(15);
                AssertCardShows(sheet, session, camera, "Ctrl+Z after Long read");
            }
            finally
            {
                EditorPopup.CloseAll();
                _window?.Close();
                _window = null;
            }
            yield return new ExitPlayMode();
        }
    }
}
