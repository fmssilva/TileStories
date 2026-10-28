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
    }
}
