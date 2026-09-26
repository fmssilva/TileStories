using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Runs the REAL POIEditorToolWindow.OnGUI inside a small host window so a test can send real mouse and
    // key events to it: controls are found through the window's rect probes (TableCellRectProbe for table
    // cells and "+ Add" buttons, FieldRowRectProbe for slider rows), clicks and keystrokes go through
    // EditorWindow.SendEvent exactly like a person's would. Shared by the taxonomy identity and undo tests.
    public sealed class PoiEditorWindowHost
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;

        private sealed class Host : EditorWindow
        {
            public static POIEditorToolWindow Editor;
            public static Vector2 RootScreen;
            public static int Repaints;

            private void OnGUI()
            {
                if (Editor == null) return;
                RootScreen = GUIUtility.GUIToScreenPoint(Vector2.zero);
                typeof(POIEditorToolWindow).GetMethod("OnGUI", Instance).Invoke(Editor, null);
                if (Event.current.type == EventType.Repaint) Repaints++;
            }
        }

        private readonly Host _host;
        public readonly POIEditorToolWindow Editor;
        public readonly Dictionary<string, Rect> ScreenRects = new();

        // Undo replaces the window's config object, so always read the live one
        public WallConfigData Config => (WallConfigData)typeof(POIEditorToolWindow).GetField("_config", Instance).GetValue(Editor);
        public bool Unsaved => (bool)typeof(POIEditorToolWindow).GetField("_hasUnsavedChanges", Instance).GetValue(Editor);

        // Opens the window on this config with one Global Scene section unfolded (e.g. "_showGlobalBadge")
        public PoiEditorWindowHost(WallConfigData config, string sectionFoldoutField)
        {
            Editor = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            var type = typeof(POIEditorToolWindow);
            type.GetField("_config", Instance).SetValue(Editor, config);
            type.GetMethod("InitializeConfigHistory", Instance).Invoke(Editor, null);
            // The framework library is already an asset: a section then never has to create a wall library file
            type.GetField("_wallIconLibrary", Instance).SetValue(Editor,
                AssetDatabase.LoadAssetAtPath<SpriteKeyLibrary>("Assets/Framework/Runtime/UI/Markers/IconLibrary.asset"));
            type.GetField(sectionFoldoutField, Instance).SetValue(Editor, true);

            POIEditorToolWindow.TableCellRectProbe = (table, row, rect) => ScreenRects[table + "#" + row] = GUIUtility.GUIToScreenRect(rect);
            POIEditorToolWindow.FieldRowRectProbe = (label, rect) => ScreenRects[label] = GUIUtility.GUIToScreenRect(rect);

            Host.Editor = Editor;
            Host.Repaints = 0;
            _host = ScriptableObject.CreateInstance<Host>();
            _host.ShowUtility();
            _host.position = new Rect(40f, 40f, 1800f, 900f);
            // - an IMGUI text field only takes keystrokes while its window has focus
            _host.Focus();
        }

        public void Close()
        {
            POIEditorToolWindow.TableCellRectProbe = null;
            POIEditorToolWindow.FieldRowRectProbe = null;
            Host.Editor = null;
            GUIUtility.keyboardControl = 0;
            GUIUtility.hotControl = 0;
            if (_host != null) _host.Close();
            if (Editor != null) Object.DestroyImmediate(Editor);
        }

        public IEnumerator WaitForRepaint()
        {
            int before = Host.Repaints;
            _host.Repaint();
            for (int i = 0; i < 120 && Host.Repaints == before; i++)
                yield return null;
            Assert.Greater(Host.Repaints, before, "the host window must have repainted");
        }

        public Rect RectOf(string probeKey)
        {
            Assert.IsTrue(ScreenRects.TryGetValue(probeKey, out Rect screenRect),
                "precondition: '" + probeKey + "' must have been drawn (probe saw no rect)");
            return screenRect;
        }

        // Screen point -> the host's own GUI coordinates
        public Vector2 Local(Vector2 screen) => screen - Host.RootScreen;

        public void Send(Event e) => _host.SendEvent(e);

        // A real left click at the centre of a probed control (or 4 px inside its left edge, for a text cell)
        public void Click(string probeKey, bool leftEdge = false)
        {
            Rect r = RectOf(probeKey);
            ClickAt(leftEdge ? new Vector2(r.x + 4f, r.center.y) : r.center);
        }

        public void ClickAt(Vector2 screen)
        {
            Vector2 local = Local(screen);
            Send(new Event { type = EventType.MouseDown, button = 0, clickCount = 1, mousePosition = local });
            Send(new Event { type = EventType.MouseUp, button = 0, clickCount = 1, mousePosition = local });
        }

        // Click into a text cell, select what it holds (Ctrl+A) and type the text, one real key event per character
        public IEnumerator ReplaceText(string probeKey, string text)
        {
            Click(probeKey, leftEdge: true);
            yield return WaitForRepaint();
            Assert.IsTrue(EditorGUIUtility.editingTextField, "precondition: the click must start editing '" + probeKey + "'");
            Send(new Event { type = EventType.KeyDown, keyCode = KeyCode.A, control = true });
            foreach (char c in text)
            {
                Send(new Event { type = EventType.KeyDown, character = c, keyCode = KeyCode.None });
                yield return WaitForRepaint();
            }
        }

        // A real click on empty window space: the text field loses focus, like a person clicking away
        public IEnumerator ClickAway()
        {
            ClickAt(Host.RootScreen + new Vector2(1790f, 890f));
            yield return WaitForRepaint();
            GUIUtility.keyboardControl = 0;
            EditorGUIUtility.editingTextField = false;
            yield return WaitForRepaint();
        }

        // A real Ctrl+Z / Ctrl+Y sent to the window (its own config history, not Unity's Undo)
        public IEnumerator PressUndo()
        {
            Send(new Event { type = EventType.KeyDown, keyCode = KeyCode.Z, control = true });
            yield return WaitForRepaint();
        }

        public IEnumerator PressRedo()
        {
            Send(new Event { type = EventType.KeyDown, keyCode = KeyCode.Y, control = true });
            yield return WaitForRepaint();
        }

        // Set one of the window's own UI-state fields (e.g. collapse a sub-foldout so the control under test is on screen)
        public void SetWindowField(string field, object value)
        {
            var f = typeof(POIEditorToolWindow).GetField(field, Instance);
            Assert.IsNotNull(f, "the window must have a field named " + field);
            f.SetValue(Editor, value);
        }

        // Every Ctrl+Z the window can still take (its history index), read without pressing anything
        public int UndoStepsLeft => (int)typeof(POIEditorToolWindow).GetField("_configHistoryIndex", Instance).GetValue(Editor);
    }
}
