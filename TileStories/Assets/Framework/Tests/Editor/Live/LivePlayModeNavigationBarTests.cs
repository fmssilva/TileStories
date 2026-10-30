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
    // Phase B of the Navigation Bar's Editor component (_3.3 S4 third round): while Play Mode runs, edits in the REAL POI Editor window reach the
    // OPEN card through the live-sync path every Detail Card row uses (DrawConfigMutationScope -> the card applier -> WallSession.ApplyCardSettings
    // -> the host's rebind): real typing of a group's name shows as that group's words on the bar, a real click on a group's trash takes it
    // off the bar, an option picked (Indicator: Pill) changes how the chosen group is drawn, and Ctrl+Z walks each back on the running card.
    public class LivePlayModeNavigationBarTests
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

        [UnityTest]
        public IEnumerator RealEditsOfTheBarGroups_WhilePlayModeRuns_ReachTheOpenCard_AndCtrlZWalksThemBack()
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
                var bar = sheet.Slots.Bar;
                Assert.IsNotNull(bar.Root.parent, "precondition: the shipped wall's card shows the Navigation Bar");
                int groups = bar.Items.Count;
                Assert.GreaterOrEqual(groups, NavBarRule.MinEntries);

                // - the window the developer edits in: the config the scene runs, Detail Card > Navigation open
                var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "LivingRoom/config.json")));
                _window = new PoiEditorWindowHost(config, "_showCardNavigation");
                var tabField = typeof(POIEditorToolWindow).GetField("_selectedTab", Instance);
                _window.SetWindowField("_selectedTab", Enum.Parse(tabField.FieldType, "DetailCard"));
                var nav = _window.Config.card_settings.navigation;
                var defaults = NavBarRule.DefaultRows(nav, BlockRegistry.Shared);
                // - the first group the card shows, and its row in the table
                string shownKey = bar.Entries[0].Item.Key;
                int row = defaults.FindIndex(d => d.key == shownKey);
                Assert.GreaterOrEqual(row, 0, "precondition: the bar's first group is a default row");
                ((System.Collections.Generic.HashSet<string>)typeof(POIEditorToolWindow).GetField("_openBarGroups", Instance).GetValue(_window.Editor)).Add(shownKey);
                yield return _window.WaitForRepaint();

                // - real typing: the group's name in the card's language reaches its words on the bar
                string language = sheet.Context.Language;
                string nameKey = "Bar group name " + language + "#" + row;
                Assert.IsTrue(_window.ScreenRects.ContainsKey(nameKey), "the group's Name row in " + language);
                yield return _window.ScrollTo(nameKey);
                yield return _window.ReplaceText(nameKey, "Overview");
                yield return _window.ClickAway();
                yield return Frames(20);
                Assert.AreEqual("Overview", sheet.Slots.Bar.Items[0].Label.text, "the open card's bar shows the typed name");

                // - Indicator: Pill, through the window's own edit path: the open card draws a pill, no bump
                typeof(POIEditorToolWindow).GetMethod("DrawConfigMutationScope", Instance).Invoke(_window.Editor,
                    new object[] { (Action)(() => _window.Config.card_settings.navigation.bar.indicator = CardOptions.NavBarIndicatorPill), false });
                yield return _window.WaitForRepaint();
                yield return Frames(20);
                bar = sheet.Slots.Bar;
                Assert.AreEqual(CardOptions.NavBarIndicatorPill, NavBarRule.Indicator(bar.Settings), "the running bar reads the pick");
                Assert.AreEqual(UnityEngine.UIElements.DisplayStyle.Flex, bar.Pill.resolvedStyle.display, "...and draws the pill");

                // - a real click on the last group's trash: the card shows one group fewer (when that group had blocks here)
                int last = _window.Config.card_settings.navigation.bar.items.Count - 1;
                string lastKey = _window.Config.card_settings.navigation.bar.items[last].key;
                bool lastShown = bar.Entries.Any(e => e.Item.Key == lastKey);
                yield return _window.ScrollTo("Bar group delete#" + last);
                _window.Click("Bar group delete#" + last);
                yield return _window.WaitForRepaint();
                yield return Frames(20);
                Assert.IsFalse(sheet.Slots.Bar.Entries.Any(e => e.Item.Key == lastKey), "the deleted group is off the open card's bar");
                Assert.AreEqual(lastShown ? groups - 1 : groups, sheet.Slots.Bar.Items.Count);

                // - Ctrl+Z three times: the group back, the raised bump back, the family's own words back
                yield return _window.PressUndo();
                yield return Frames(20);
                Assert.AreEqual(groups, sheet.Slots.Bar.Items.Count, "Ctrl+Z: the group back on the card");
                yield return _window.PressUndo();
                yield return Frames(20);
                Assert.AreEqual(CardOptions.NavBarIndicatorRaised, NavBarRule.Indicator(sheet.Slots.Bar.Settings), "Ctrl+Z: Raised again");
                yield return _window.PressUndo();
                yield return Frames(20);
                Assert.AreNotEqual("Overview", sheet.Slots.Bar.Items[0].Label.text, "Ctrl+Z: the family's own word again");
            }
            finally
            {
                _window?.Close();
                _window = null;
            }
            yield return new ExitPlayMode();
        }

        // Raised Depth and Icon Colour live (the P3c review b, second pass c): while Play Mode runs, a REAL drag of the Raised Depth slider
        // in the real window down to its least turns the open card's bump into a cradle (the painted edge dips into the bar under the chosen
        // group, the disc where it was); Icon Colour picked Neutral reaches the running bar; Ctrl+Z walks both back
        [UnityTest]
        public IEnumerator RaisedDepthAndIconColour_WhilePlayModeRuns_ReachTheOpenCard_AndCtrlZWalksThemBack()
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
                var bar = sheet.Slots.Bar;
                Assert.IsTrue(NavBarRule.Raised(bar.Settings), "precondition: the shipped wall's bar is Raised");
                Assert.Greater(bar.BumpHeight, 0f, "precondition: a bell");
                var discBefore = bar.Disc.layout;

                var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "LivingRoom/config.json")));
                _window = new PoiEditorWindowHost(config, "_showCardNavigation");
                var tabField = typeof(POIEditorToolWindow).GetField("_selectedTab", Instance);
                _window.SetWindowField("_selectedTab", Enum.Parse(tabField.FieldType, "DetailCard"));
                yield return _window.WaitForRepaint();

                // - a real drag along the slider's track, far to its left end, inside one editor tick
                yield return _window.ScrollTo("Raised Depth");
                Rect row = _window.RectOf("Raised Depth");
                float y = row.center.y, startX = row.xMax - 70f;
                _window.Send(new Event { type = EventType.MouseDown, button = 0, clickCount = 1, mousePosition = _window.Local(new Vector2(startX, y)) });
                _window.PaintNow();
                for (int i = 1; i <= 10; i++)
                {
                    _window.Send(new Event { type = EventType.MouseDrag, button = 0, mousePosition = _window.Local(new Vector2(startX - 60f * i, y)), delta = new Vector2(-60f, 0f) });
                    _window.PaintNow();
                }
                _window.Send(new Event { type = EventType.MouseUp, button = 0, clickCount = 1, mousePosition = _window.Local(new Vector2(startX - 600f, y)) });
                _window.PaintNow();
                yield return _window.WaitForRepaint();
                yield return Frames(20);
                Assert.AreEqual(NavBarRule.RaisedDepthMin, _window.Config.card_settings.navigation.bar.raised_depth, 1e-4f, "precondition: the drag reached the slider's least");
                bar = sheet.Slots.Bar;
                Assert.AreEqual(NavBarRule.RaisedDepthMin, NavBarRule.RaisedDepth(bar.Settings), 1e-4f, "the running bar reads the depth");
                Assert.Less(bar.BumpHeight, 0f, "...a cradle");
                // - the bump's pieces end on the edge or at its extreme: the deepest of them above the bar's middle is the cradle's trough
                float trough = bar.PaintedPath.Where(c => c.Op == NavBarShapeRule.Op.Cubic).Select(c => c.P3.y)
                    .Where(py => py < bar.Room + bar.BarHeight * 0.5f).Max();
                Assert.Greater(trough, bar.Room + 1f, "the painted edge dips into the bar");
                Assert.AreEqual(discBefore.y, bar.Disc.layout.y, 0.5f, "the disc never moves");

                // - Icon Colour: Neutral, through the window's own edit path
                typeof(POIEditorToolWindow).GetMethod("DrawConfigMutationScope", Instance).Invoke(_window.Editor,
                    new object[] { (Action)(() => _window.Config.card_settings.navigation.bar.icon_color = CardOptions.NavBarIconNeutral), false });
                yield return _window.WaitForRepaint();
                yield return Frames(20);
                Assert.AreEqual(CardOptions.NavBarIconNeutral, NavBarRule.IconColour(sheet.Slots.Bar.Settings), "the running bar reads Icon Colour");

                yield return _window.PressUndo();
                yield return Frames(20);
                Assert.AreEqual(CardOptions.NavBarIconGroup, NavBarRule.IconColour(sheet.Slots.Bar.Settings), "Ctrl+Z: Group again");
                yield return _window.PressUndo();
                yield return Frames(20);
                Assert.AreEqual(1f, NavBarRule.RaisedDepth(sheet.Slots.Bar.Settings), 1e-4f, "Ctrl+Z: the full bell again");
                Assert.Greater(sheet.Slots.Bar.BumpHeight, 0f, "...a bell on the card");
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
