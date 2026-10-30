using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TileStories.Editor.Tests
{
    // Phase B of Detail Card > Locate Button (_3.3 P3d review b): while Play Mode runs on the real wall, edits in the REAL POI Editor window
    // reach the OPEN card's pin through the live-sync path every Detail Card row uses (DrawConfigMutationScope -> the card applier ->
    // WallSession.ApplyCardSettings): Style Glass + Ping (the disc, no words), Colour Cyan (the pin's rim token), a REAL click on Float off (its
    // motion stops); Ctrl+Z walks each back on the running card
    public class LivePlayModeCardLocateTests
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
        public IEnumerator StyleColourAndFloat_WhilePlayModeRuns_ReachTheOpenCardsPin_AndCtrlZWalksThemBack()
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
                sheet.SetStop(SheetStopRule.Stop.Half);
                yield return Frames(30);
                var fab = sheet.Locate;
                Assert.IsTrue(fab.IsShown, "precondition: the Locate Button on the shipped wall's card");
                Assert.AreEqual(CardOptions.LocatePinLabel, fab.Style, "precondition: the default Pin + Label");
                Assert.IsTrue(fab.Moving, "precondition: Float on, its motion running");

                var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "LivingRoom/config.json")));
                _window = new PoiEditorWindowHost(config, "_showCardLocate");
                var tabField = typeof(POIEditorToolWindow).GetField("_selectedTab", Instance);
                _window.SetWindowField("_selectedTab", Enum.Parse(tabField.FieldType, "DetailCard"));
                yield return _window.WaitForRepaint();

                Pick(() => _window.Config.card_settings.locate.style = CardOptions.LocateGlassPing);
                yield return _window.WaitForRepaint();
                yield return Frames(20);
                Assert.AreEqual(CardOptions.LocateGlassPing, fab.Style, "Glass + Ping on the open card");
                Assert.AreEqual(UnityEngine.UIElements.DisplayStyle.Flex, fab.Glass.style.display.value, "...its glass disc drawn");
                Assert.IsFalse(fab.LabelShown, "...no words");

                Pick(() => _window.Config.card_settings.locate.colour = CardOptions.LocateCyan);
                yield return _window.WaitForRepaint();
                yield return Frames(20);
                Assert.AreEqual("--ts-locate-cyan-rim", fab.Pin.Tokens.Rim, "Cyan: the open card's pin painted from the cyan tokens");

                yield return _window.ScrollTo("Float");
                _window.Click("Float");
                yield return _window.WaitForRepaint();
                yield return Frames(20);
                Assert.IsFalse(_window.Config.card_settings.locate.float_motion, "precondition: the real click turned Float off");
                Assert.IsFalse(fab.Moving, "Float off: the open card's pin still");

                yield return _window.PressUndo();
                yield return Frames(20);
                Assert.IsTrue(fab.Moving, "Ctrl+Z: it floats again");
                yield return _window.PressUndo();
                yield return Frames(20);
                Assert.AreEqual("--ts-locate-teal-rim", fab.Pin.Tokens.Rim, "Ctrl+Z: AR Teal again");
                yield return _window.PressUndo();
                yield return Frames(20);
                Assert.AreEqual(CardOptions.LocatePinLabel, fab.Style, "Ctrl+Z: Pin + Label again");
                Assert.IsTrue(fab.LabelShown, "...its words back (the card was not scrolled)");
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
