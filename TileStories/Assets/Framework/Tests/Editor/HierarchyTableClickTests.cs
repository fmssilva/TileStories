using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Editor.Tests
{
    // Hierarchy Levels table (_2.3_Marker_Hierarchy.md section 6): the three checkbox columns
    // (Show Marker Label?, Pulse, Spin Ring) must react to a REAL mouse click where the
    // developer sees the checkbox. The whole production window draws inside a host window
    // (real OnGUI, real scroll view, real DrawFramedFoldout indent scope -- the exact stack
    // that once shifted the checkbox hit area off its glyph), and the click is a real
    // MouseDown + MouseUp pair sent to that host. No mocks: the assertion reads the config.
    public class HierarchyTableClickTests
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;

        // Hosts the real POIEditorToolWindow.OnGUI. RootScreen is this window's GUI origin in
        // screen space, so a rect the probe reports (converted to screen space inside its own
        // clip context) maps back to exact host-local mouse coordinates.
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

        private Host _host;
        private POIEditorToolWindow _editor;
        private WallConfigData _config;
        private readonly Dictionary<string, Rect> _screenRects = new();

        [SetUp]
        public void SetUp()
        {
            _config = new WallConfigData
            {
                pois = new List<POIData>(),
                marker_outline_mode = "uniform",           // Spin Ring column shown
                effect_defaults = new EffectDefaults(),
                hierarchy_levels = new List<HierarchyLevelEntry>
                {
                    new HierarchyLevelEntry
                    {
                        key = "level_1", level_name = "Top", priority = 1, size_cm = 20f,
                        show_label = false, pulse = false, rotate_contour = false,
                        ripple_effect = "none", halo_effect = "none", reveal_duration_s = 0.35f,
                        search_keywords = new List<string>()
                    }
                }
            };
            _config.effect_defaults.effects_enabled = true;
            _config.effect_defaults.pulse.enabled = true;  // Pulse checkbox enabled

            _editor = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            typeof(POIEditorToolWindow).GetField("_config", Instance).SetValue(_editor, _config);
            typeof(POIEditorToolWindow).GetField("_showGlobalHierarchy", Instance).SetValue(_editor, true);

            _screenRects.Clear();
            POIEditorToolWindow.HierarchyCheckboxRectProbe = (column, row, rect) =>
                _screenRects[column + "#" + row] = GUIUtility.GUIToScreenRect(rect);

            Host.Editor = _editor;
            Host.Repaints = 0;
            _host = ScriptableObject.CreateInstance<Host>();
            _host.ShowUtility();
            _host.position = new Rect(40f, 40f, 1800f, 900f);
        }

        [TearDown]
        public void TearDown()
        {
            POIEditorToolWindow.HierarchyCheckboxRectProbe = null;
            Host.Editor = null;
            if (_host != null) _host.Close();
            if (_editor != null) Object.DestroyImmediate(_editor);
        }

        private IEnumerator WaitForRepaint()
        {
            int before = Host.Repaints;
            _host.Repaint();
            for (int i = 0; i < 120 && Host.Repaints == before; i++)
                yield return null;
            Assert.Greater(Host.Repaints, before, "the host window must have repainted");
        }

        // A real left click at the centre of the checkbox the developer sees
        private void Click(string column)
        {
            Assert.IsTrue(_screenRects.TryGetValue(column + "#0", out Rect screenRect),
                "precondition: the '" + column + "' checkbox must have been drawn (probe saw no rect)");
            Vector2 local = screenRect.center - Host.RootScreen;
            _host.SendEvent(new Event { type = EventType.MouseDown, button = 0, clickCount = 1, mousePosition = local });
            _host.SendEvent(new Event { type = EventType.MouseUp, button = 0, clickCount = 1, mousePosition = local });
        }

        [UnityTest]
        public IEnumerator ShowMarkerLabelCheckbox_TogglesOnARealClick_AndMarksTheConfigUnsaved()
        {
            yield return WaitForRepaint();
            Click("show_label");
            Assert.IsTrue(_config.hierarchy_levels[0].show_label, "clicking Show Marker Label? must tick it");
            Assert.IsTrue((bool)typeof(POIEditorToolWindow).GetField("_hasUnsavedChanges", Instance).GetValue(_editor),
                "a table click goes through DrawConfigMutationScope, so it must mark the config unsaved");

            yield return WaitForRepaint();
            Click("show_label");
            Assert.IsFalse(_config.hierarchy_levels[0].show_label, "a second click must untick it");
        }

        [UnityTest]
        public IEnumerator PulseCheckbox_TogglesOnARealClick()
        {
            yield return WaitForRepaint();
            Click("pulse");
            Assert.IsTrue(_config.hierarchy_levels[0].pulse, "clicking Pulse must tick it");
        }

        [UnityTest]
        public IEnumerator SpinRingCheckbox_TogglesOnARealClick()
        {
            yield return WaitForRepaint();
            Click("rotate_contour");
            Assert.IsTrue(_config.hierarchy_levels[0].rotate_contour, "clicking Spin Ring must tick it");
        }

        [UnityTest]
        public IEnumerator PulseCheckbox_IgnoresClicks_WhilePulseIsOffWallWide()
        {
            // The DisabledScope must still hold after the fix: a switched-off effect cannot be ticked.
            _config.effect_defaults.pulse.enabled = false;
            yield return WaitForRepaint();
            Click("pulse");
            Assert.IsFalse(_config.hierarchy_levels[0].pulse, "Pulse is off wall-wide, so its column must stay read-only");
        }

        // The row's trash button, clicked for real while a POI still uses the level: the question's
        // "Delete anyway" must delete the row and its Cancel must keep it (the answer is scripted
        // through EditorDecision.Responder; the click, the guard and the table code are real).
        [UnityTest]
        public IEnumerator DeletingALevelStillInUse_AsksFirst_CancelKeepsIt_DeleteAnywayDeletesIt()
        {
            _config.pois.Add(new POIData { id = "poi_a", hierarchy_level_key = "level_1" });
            // the full table is wider than a laptop screen (the host is clamped to it) and IMGUI drops a
            // click outside the visible area: hide the optional columns so the trash is really on screen
            _config.effect_defaults.effects_enabled = false;
            _config.marker_outline_mode = "none";
            var asked = new List<DecisionRequest>();
            var answer = DecisionAnswer.Cancel;
            EditorDecision.Responder = r => { asked.Add(r); return answer; };
            try
            {
                yield return WaitForRepaint();
                Click("delete");
                Assert.AreEqual(1, asked.Count, "a level still in use asks before it goes (trash at "
                    + _screenRects["delete#0"] + ", host " + _host.position + ")");
                Assert.AreEqual("Delete anyway", asked[0].ConfirmLabel);
                StringAssert.Contains("1 POI(s) still reference", asked[0].Message);
                Assert.AreEqual(1, _config.hierarchy_levels.Count, "Cancel keeps the level");

                answer = DecisionAnswer.Confirm;
                yield return WaitForRepaint();
                Click("delete");
                var live = (WallConfigData)typeof(POIEditorToolWindow).GetField("_config", Instance).GetValue(_editor);
                Assert.AreEqual(0, live.hierarchy_levels.Count, "Delete anyway deletes the level");
            }
            finally { EditorDecision.Responder = global::TestDialogGuard.CancelEverything; }
        }
    }
}
