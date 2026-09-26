using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Editor.Tests
{
    // The Hierarchy Levels table's "Aa" window, "Marker Label Style" (_2.0_Labels_And_Fonts_Design.md
    // section 4), driven for real: the real shared EditorPopup opened by LevelLabelStylePopup.Open on a
    // real POIEditorToolWindow, a real mouse click on its override toggle, a real Ctrl+Z key event.
    // What the rows SHOW is read through the LevelLabelStyleProbe seam; what is STORED is read from
    // the owner's live config. No mocks.
    public class MarkerLabelStyleWindowTests
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;

        private POIEditorToolWindow _owner;
        private EditorPopup _window;
        private int _repaints;
        private Rect _toggleRect;
        private float _shownGap, _shownSize;
        private string _shownFont;

        private WallConfigData Config => (WallConfigData)typeof(POIEditorToolWindow).GetField("_config", Instance).GetValue(_owner);
        private HierarchyLevelEntry Level => Config.hierarchy_levels.Find(l => l.key == "level_1");
        private bool Unsaved => (bool)typeof(POIEditorToolWindow).GetField("_hasUnsavedChanges", Instance).GetValue(_owner);

        [SetUp]
        public void SetUp()
        {
            var config = new WallConfigData
            {
                pois = new List<POIData>(),
                label_gap_ratio = 0.1f,
                label_font_size_ratio = 0.3f,
                label_font_key = "roboto_bold",
                hierarchy_levels = new List<HierarchyLevelEntry>
                {
                    new HierarchyLevelEntry { key = "level_1", level_name = "Top", size_cm = 20f, show_label = true }
                }
            };
            _owner = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            typeof(POIEditorToolWindow).GetField("_config", Instance).SetValue(_owner, config);
            typeof(POIEditorToolWindow).GetMethod("InitializeConfigHistory", Instance).Invoke(_owner, null);

            _repaints = 0;
            POIEditorToolWindow.LevelLabelStyleProbe = (toggle, gap, size, font) =>
            {
                _toggleRect = toggle; _shownGap = gap; _shownSize = size; _shownFont = font; _repaints++;
            };

            _window = LevelLabelStylePopup.Open(_owner, "level_1");
            _window.position = new Rect(60f, 60f, 420f, 220f);
        }

        [TearDown]
        public void TearDown()
        {
            POIEditorToolWindow.LevelLabelStyleProbe = null;
            if (_window != null) _window.Close();
            if (_owner != null) Object.DestroyImmediate(_owner);
        }

        private IEnumerator Repaint()
        {
            int before = _repaints;
            _window.Repaint();
            for (int i = 0; i < 120 && _repaints == before && _window != null; i++)
                yield return null;
        }

        private void ClickOverrideToggle()
        {
            Vector2 p = _toggleRect.center;
            _window.SendEvent(new Event { type = EventType.MouseDown, button = 0, clickCount = 1, mousePosition = p });
            _window.SendEvent(new Event { type = EventType.MouseUp, button = 0, clickCount = 1, mousePosition = p });
        }

        [UnityTest]
        public IEnumerator Window_HasOneFixedTitle_AndReusesOneInstance()
        {
            yield return Repaint();
            Assert.AreEqual("Marker Label Style", _window.titleContent.text, "one short fixed title, no level name glued on");

            var again = LevelLabelStylePopup.Open(_owner, "level_1");
            Assert.AreSame(_window, again, "opening 'Aa' again reuses the open window");
            Assert.AreEqual(1, System.Array.FindAll(Resources.FindObjectsOfTypeAll<EditorPopup>(), w => w.Content is LevelLabelStylePopup).Length, "one label-style popup, never two");
        }

        [UnityTest]
        public IEnumerator OverrideOff_RowsShowTheWallDefault_AndFollowAWallDefaultChange()
        {
            yield return Repaint();
            Assert.Greater(_repaints, 0, "precondition: the window drew its rows");
            Assert.AreEqual(0.1f, _shownGap, 1e-5f, "override off: Gap shows the wall default");
            Assert.AreEqual(0.3f, _shownSize, 1e-5f, "override off: Font size shows the wall default");
            Assert.AreEqual("roboto_bold", _shownFont, "override off: Font shows the wall default");

            Config.label_font_size_ratio = 0.6f;   // edited in Labels, Text & Fonts
            yield return Repaint();
            Assert.AreEqual(0.6f, _shownSize, 1e-5f, "a wall default change is shown at once while the level does not override");
            Assert.IsFalse(Level.override_label_style, "just looking never switches the override on");
        }

        [UnityTest]
        public IEnumerator ClickingOverride_SeedsFromTheWallDefault_IsUndoable_AndMarksUnsaved()
        {
            yield return Repaint();
            ClickOverrideToggle();

            Assert.IsTrue(Level.override_label_style, "a real click on 'Override global label config' ticks it");
            Assert.AreEqual(0.1f, Level.label_gap_ratio, 1e-5f, "seeded from the wall gap");
            Assert.AreEqual(0.3f, Level.label_font_size_ratio, 1e-5f, "seeded from the wall font size");
            Assert.AreEqual("roboto_bold", Level.label_font_key, "seeded from the wall font");
            Assert.IsTrue(Unsaved, "the edit went through the owner's mutation scope: config is unsaved");

            // While overriding, a wall default change must not touch this level (shown or stored).
            Config.label_font_size_ratio = 0.9f;
            yield return Repaint();
            Assert.AreEqual(0.3f, _shownSize, 1e-5f, "override on: the row shows the level's own value");
            Assert.AreEqual(0.3f, Level.label_font_size_ratio, 1e-5f, "override on: the wall change never reaches the level");

            // Real Ctrl+Z sent to THIS window: the owner's history undoes the click.
            var before = Config;
            _window.SendEvent(new Event { type = EventType.KeyDown, keyCode = KeyCode.Z, control = true });
            Assert.AreNotSame(before, Config, "undo swapped the owner's config for the previous snapshot");
            Assert.IsFalse(Level.override_label_style, "undo reverted the override click");

            // The window must now edit the LIVE config (found by key), not the stale object it opened on.
            Config.label_gap_ratio = 0.55f;
            yield return Repaint();
            Assert.IsNotNull(_window, "the window stays open after an undo");
            Assert.AreEqual(0.55f, _shownGap, 1e-5f, "after undo the window reads the live config, not a stale level object");
        }

        [UnityTest]
        public IEnumerator Window_ClosesItself_WhenItsLevelIsDeleted()
        {
            yield return Repaint();
            Config.hierarchy_levels.Clear();
            _window.Repaint();
            for (int i = 0; i < 120 && _window != null; i++)
                yield return null;
            Assert.IsTrue(_window == null, "nothing left to edit: the window must close");
        }
    }
}
