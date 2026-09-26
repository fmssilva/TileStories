using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TileStories.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Tests
{
    // The ONE non-blocking popup kind (EditorPopup), driven for real: real windows on screen, real
    // mouse clicks and key presses sent with SendEvent, the real POI Editor factories and config
    // history. Every help (i), Details note, symbol picker, "Aa" label style and notice uses it.
    public class EditorPopupTests
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;
        private const string TestKey = "test-only-popup-notice-key";

        // A window with two real (i) buttons at fixed rects, drawn by the one shared HelpInfoButton
        private sealed class HelpHost : EditorWindow
        {
            public static readonly Rect First = new Rect(10f, 10f, 22f, 18f);
            public static readonly Rect Second = new Rect(50f, 10f, 22f, 18f);
            public static int Repaints;

            private void OnGUI()
            {
                HelpInfoButton.Draw(First, "First help", "Body of the first help text.");
                HelpInfoButton.Draw(Second, "Second help", "Body of the second help text.");
                if (Event.current.type == EventType.Repaint) Repaints++;
            }
        }

        private HelpHost _host;
        private POIEditorToolWindow _owner;

        [SetUp]
        public void SetUp()
        {
            EditorPopup.CloseAll();
            HelpHost.Repaints = 0;
        }

        [TearDown]
        public void TearDown()
        {
            EditorPopup.CloseAll();
            ExistingSymbolPickerPopup.RowRectProbe = null;
            EntryDetailsPopup.TextAreaRectProbe = null;
            NoticePopup.DontShowAgainRectProbe = null;
            NoticePopup.OkRectProbe = null;
            EditorNotice.ShowPopups = false;   // what TestDialogGuard set for the run
            EditorNotice.Clear();
            EditorNotice.Unhide(TestKey);
            if (_host != null) _host.Close();
            if (_owner != null) UnityEngine.Object.DestroyImmediate(_owner);
            GUIUtility.keyboardControl = 0;
        }

        private static IEnumerator Frames(int count = 6)
        {
            for (int i = 0; i < count; i++) yield return null;
        }

        private static void Click(EditorWindow window, Vector2 at)
        {
            window.SendEvent(new Event { type = EventType.MouseDown, button = 0, clickCount = 1, mousePosition = at });
            window.SendEvent(new Event { type = EventType.MouseUp, button = 0, clickCount = 1, mousePosition = at });
        }

        private IEnumerator OpenHost()
        {
            _host = ScriptableObject.CreateInstance<HelpHost>();
            _host.ShowUtility();
            _host.position = new Rect(80f, 80f, 200f, 60f);
            _host.Focus();
            for (int i = 0; i < 120 && HelpHost.Repaints == 0; i++) { _host.Repaint(); yield return null; }
            Assert.Greater(HelpHost.Repaints, 0, "precondition: the host drew its (i) buttons");
        }

        // ---------- help (i) ----------

        [UnityTest]
        public IEnumerator AClickOnAnInfoButton_OpensAFloatingDraggablePopup_WithTheHelpText()
        {
            yield return OpenHost();
            Click(_host, HelpHost.First.center);
            yield return Frames();

            EditorPopup popup = EditorPopup.FindOpen(HelpInfoPopup.PopupKind);
            Assert.IsNotNull(popup, "a real click on (i) must open the help popup");
            var help = popup.Content as HelpInfoPopup;
            Assert.IsNotNull(help);
            Assert.AreEqual("First help", popup.titleContent.text, "the help title is the window's own title bar (the X is on it)");
            Assert.AreEqual("Body of the first help text.", help.Body);
            Assert.IsFalse(popup.docked, "a floating utility window, never a docked tab");

            // draggable: the developer can put it anywhere, e.g. clear of the Game view
            var moved = new Rect(420f, 260f, popup.position.width, popup.position.height);
            popup.position = moved;
            yield return Frames();
            Assert.AreEqual(moved.x, popup.position.x, 1f, "the popup moves where it is dragged");
            Assert.AreEqual(moved.y, popup.position.y, 1f);
        }

        [UnityTest]
        public IEnumerator ASecondInfoButton_RetargetsTheOpenPopup_WhereItWasDragged()
        {
            yield return OpenHost();
            Click(_host, HelpHost.First.center);
            yield return Frames();
            EditorPopup first = EditorPopup.FindOpen(HelpInfoPopup.PopupKind);
            first.position = new Rect(430f, 270f, first.position.width, first.position.height);
            yield return Frames();

            Click(_host, HelpHost.Second.center);
            yield return Frames();

            EditorPopup second = EditorPopup.FindOpen(HelpInfoPopup.PopupKind);
            Assert.AreSame(first, second, "one help popup: a second (i) reuses it instead of stacking another");
            Assert.AreEqual("Second help", second.titleContent.text);
            Assert.AreEqual(430f, second.position.x, 1f, "it stays where the developer dragged it");
            Assert.AreEqual(1, Array.FindAll(Resources.FindObjectsOfTypeAll<EditorPopup>(), w => w.Content is HelpInfoPopup).Length);
        }

        [UnityTest]
        public IEnumerator Esc_ClosesThePopup_AndNoPopupHasItsOwnCloseButton()
        {
            yield return OpenHost();
            Click(_host, HelpHost.First.center);
            yield return Frames();
            EditorPopup popup = EditorPopup.FindOpen(HelpInfoPopup.PopupKind);
            Assert.IsNotNull(popup);

            popup.SendEvent(new Event { type = EventType.KeyDown, keyCode = KeyCode.Escape });
            yield return Frames();
            Assert.IsTrue(popup == null, "Esc closes a popup");

            // a WORKING popup (help, note, picker, label style) closes with its title bar's X: no
            // home-made Close / Cancel / OK button. Only a MESSAGE (NoticePopup) has an OK button.
            string folder = System.IO.Path.Combine(Application.dataPath, "Framework/Editor/POIEditor/Shared/Popups");
            foreach (string file in System.IO.Directory.GetFiles(folder, "*.cs"))
            {
                if (file.EndsWith("NoticePopup.cs")) continue;
                StringAssert.DoesNotMatch(@"GUILayout\.Button\(\s*(""(Close|Cancel|OK)""|OkLabel)", System.IO.File.ReadAllText(file),
                    System.IO.Path.GetFileName(file));
            }
        }

        // ---------- note (Details) and picker: config edits through the window's mutation scope ----------

        private void Owner(WallConfigData config)
        {
            _owner = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            typeof(POIEditorToolWindow).GetField("_config", Instance).SetValue(_owner, config);
            typeof(POIEditorToolWindow).GetMethod("InitializeConfigHistory", Instance).Invoke(_owner, null);
        }

        private WallConfigData LiveConfig => (WallConfigData)typeof(POIEditorToolWindow).GetField("_config", Instance).GetValue(_owner);
        private bool Unsaved => (bool)typeof(POIEditorToolWindow).GetField("_hasUnsavedChanges", Instance).GetValue(_owner);
        private T Call<T>(string method, params object[] args) => (T)typeof(POIEditorToolWindow).GetMethod(method, Instance).Invoke(_owner, args);

        [UnityTest]
        public IEnumerator TypingInADetailsNote_EditsTheConfig_Undoably_AndAnUndoClosesTheNote()
        {
            var entry = new CategoryStyleEntry { category = "cat_a", details = "" };
            Owner(new WallConfigData { pois = new List<POIData>(), category_styles = new List<CategoryStyleEntry> { entry } });

            var note = Call<EntryDetailsPopup>("CreateDetailsPopup", "cat_a",
                (Func<string>)(() => entry.details), (Action<string>)(v => entry.details = v));
            Rect textArea = default;
            EntryDetailsPopup.TextAreaRectProbe = r => textArea = r;
            EditorPopup popup = EditorPopup.Show(note);
            popup.position = new Rect(120f, 120f, 340f, 200f);
            // - an IMGUI text area only edits while its window has focus
            popup.Focus();
            for (int i = 0; i < 120 && textArea.height == 0f; i++) { popup.Repaint(); yield return null; }
            Assert.Greater(textArea.height, 0f, "precondition: the note drew its text area");

            // a real click into the text area, then real key presses
            Click(popup, textArea.center);
            popup.Repaint();
            yield return Frames();
            foreach (char c in "old")
            {
                popup.SendEvent(new Event { type = EventType.KeyDown, character = c, keyCode = KeyCode.None });
                yield return Frames(2);
            }

            Assert.AreEqual("old", entry.details, "real typing reaches the row's note");
            Assert.IsTrue(Unsaved, "through the window's mutation scope: Save All to JSON turns yellow");
            Assert.IsTrue(Call<bool>("CanUndoConfigChange"), "each keystroke is an undo step");

            Call<object>("UndoConfigChange");
            // its next OnGUI checks IsAlive BEFORE drawing, so it can never write into the dead copy
            for (int i = 0; i < 120 && popup != null; i++) { popup.Repaint(); yield return null; }
            Assert.IsTrue(popup == null,
                "an undo replaced the config the note was opened on: it must close, never write into a dead copy");
            Assert.AreEqual("ol", LiveConfig.category_styles[0].details, "the undo took back the last keystroke");
        }

        [UnityTest]
        public IEnumerator ARealClickOnAPickerRow_AssignsTheSymbol_ClosesThePicker_AndIsUndoable()
        {
            var tex = new Texture2D(4, 4);
            var library = ScriptableObject.CreateInstance<SpriteKeyLibrary>();
            var entries = new SerializedObject(library).FindProperty("entries");
            entries.arraySize = 2;
            string[] keys = { "icon_one", "icon_two" };
            for (int i = 0; i < keys.Length; i++)
            {
                var e = entries.GetArrayElementAtIndex(i);
                e.FindPropertyRelative("key").stringValue = keys[i];
                e.FindPropertyRelative("sprite").objectReferenceValue = Sprite.Create(tex, new Rect(0, 0, 4, 4), Vector2.zero, 100f);
            }
            entries.serializedObject.ApplyModifiedPropertiesWithoutUndo();

            try
            {
                var entry = new CategoryStyleEntry { category = "cat_a", icon_key = "old_icon" };
                Owner(new WallConfigData { pois = new List<POIData>(), category_styles = new List<CategoryStyleEntry> { entry } });
                typeof(POIEditorToolWindow).GetField("_wallIconLibrary", Instance).SetValue(_owner, library);

                var rows = new Dictionary<int, Rect>();
                ExistingSymbolPickerPopup.RowRectProbe = (i, r) => rows[i] = r;
                var picker = Call<ExistingSymbolPickerPopup>("CreateSymbolPickerPopup", (Action<string>)(k => entry.icon_key = k));
                EditorPopup popup = EditorPopup.Show(picker);
                popup.position = new Rect(140f, 140f, 260f, 200f);
                for (int i = 0; i < 120 && !rows.ContainsKey(1); i++) { popup.Repaint(); yield return null; }
                Assert.IsTrue(rows.ContainsKey(1), "precondition: the picker drew its rows");

                popup.SendEvent(new Event { type = EventType.MouseDown, button = 0, clickCount = 1, mousePosition = rows[1].center });
                yield return Frames();

                Assert.AreEqual("icon_two", entry.icon_key, "a real click on the second row picks it");
                Assert.IsTrue(popup == null, "a pick closes the picker");
                Assert.IsTrue(Unsaved);
                Call<object>("UndoConfigChange");
                Assert.AreEqual("old_icon", LiveConfig.category_styles[0].icon_key, "Ctrl+Z restores the previous symbol");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(library);
                UnityEngine.Object.DestroyImmediate(tex);
            }
        }

        // ---------- notices: a message with an OK button, and nothing waits for it ----------

        private EditorPopup _notice;
        private Rect _okRect, _checkboxRect;

        // Queue a notice and run the REAL editor-update pump (with the old modal dialog this call never returned)
        private IEnumerator ShowNotice(string key)
        {
            EditorNotice.Unhide(TestKey);
            EditorNotice.ShowPopups = true;
            EditorNotice.Queue("Advice", "Something worth knowing.", 0f, key);
            typeof(EditorNotice).GetMethod("Pump", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
            Assert.IsFalse(EditorNotice.HasPending, "the pump took the notice");

            _notice = EditorPopup.FindOpen(NoticePopup.PopupKind);
            Assert.IsNotNull(_notice, "the notice is shown in the shared popup window");
            _notice.position = new Rect(160f, 160f, 380f, 140f);

            _okRect = _checkboxRect = default;
            NoticePopup.OkRectProbe = r => _okRect = r;
            NoticePopup.DontShowAgainRectProbe = r => _checkboxRect = r;
            for (int i = 0; i < 120 && _okRect.width == 0f; i++) { _notice.Repaint(); yield return null; }
            Assert.Greater(_okRect.width, 0f, "precondition: the notice drew its OK button");
        }

        private IEnumerator WaitClosed()
        {
            for (int i = 0; i < 60 && _notice != null; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator ANotice_IsAMessageWithAnOkButtonOnTheRight_AndTheCheckboxOnTheLeftOfTheSameRow()
        {
            yield return ShowNotice(TestKey);
            Assert.AreEqual("Advice", _notice.titleContent.text);
            Assert.AreEqual("Something worth knowing.", ((NoticePopup)_notice.Content).Message);
            Assert.Greater(_checkboxRect.width, 0f, "an advisory notice offers 'Don't show this again'");
            Assert.AreEqual(_okRect.center.y, _checkboxRect.center.y, 2f, "checkbox and OK share the bottom row");
            Assert.Less(_checkboxRect.xMax, _okRect.xMin, "checkbox on the left, OK on the right");
            Assert.Greater(_okRect.xMax, _notice.position.width - 40f, "OK sits at the right edge, where a message's default button is");
        }

        [UnityTest]
        public IEnumerator ClickingOk_ClosesTheNotice()
        {
            yield return ShowNotice(TestKey);
            Click(_notice, _okRect.center);
            yield return WaitClosed();
            Assert.IsTrue(_notice == null, "OK closes the notice");
            Assert.IsFalse(EditorNotice.IsHidden(TestKey), "OK without the box ticked hides nothing");
        }

        [UnityTest]
        public IEnumerator Enter_PressesOk()
        {
            yield return ShowNotice(TestKey);
            _notice.SendEvent(new Event { type = EventType.KeyDown, keyCode = KeyCode.Return });
            yield return WaitClosed();
            Assert.IsTrue(_notice == null, "Enter = the default button (OK)");
        }

        [UnityTest]
        public IEnumerator TickingDontShowAgain_CountsWhenTheNoticeCloses_ThenItIsNeverShownAgain()
        {
            yield return ShowNotice(TestKey);
            Click(_notice, _checkboxRect.center);
            yield return Frames();
            Assert.IsFalse(EditorNotice.IsHidden(TestKey), "ticking is a choice about NEXT time: stored when the notice closes");

            Click(_notice, _okRect.center);
            yield return WaitClosed();
            Assert.IsTrue(_notice == null);
            Assert.IsTrue(EditorNotice.IsHidden(TestKey), "OK with the box ticked hides this notice from now on");
            EditorNotice.Queue("Advice", "Something worth knowing.", 0f, TestKey);
            Assert.IsFalse(EditorNotice.HasPending, "a hidden notice is never queued again");
        }

        [UnityTest]
        public IEnumerator TickingDontShowAgain_ThenClosingWithTheX_StillCounts()
        {
            yield return ShowNotice(TestKey);
            Click(_notice, _checkboxRect.center);
            yield return Frames();
            _notice.Close();   // what the title bar's X does
            yield return WaitClosed();
            Assert.IsTrue(EditorNotice.IsHidden(TestKey), "the box counts however the notice is closed");
        }

        [UnityTest]
        public IEnumerator ARefusalNotice_HasOkButNoCheckbox()
        {
            yield return ShowNotice(null);
            Assert.Greater(_okRect.width, 0f, "every notice can be acknowledged with OK");
            Assert.AreEqual(0f, _checkboxRect.width, "a notice without a key (a refusal, the result of a click) can never be hidden");
        }
    }
}
