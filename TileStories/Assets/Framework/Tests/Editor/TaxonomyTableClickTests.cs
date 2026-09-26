using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Editor.Tests
{
    // Marker / Badge symbol tables and the Outline Types table: a text cell must take the click
    // where the developer sees it. Label-less EditorGUI cells run their rect through
    // EditorGUI.IndentedRect, so inside a section (indentLevel 1) they used to be drawn 15 px right
    // of the space the table reserved for them: the left part of every cell was dead. The REAL
    // window draws inside a host (real OnGUI, real DrawFramedFoldout indent scope) and receives a
    // real MouseDown/MouseUp 4 px inside the reserved cell (same pattern as HierarchyTableClickTests).
    public class TaxonomyTableClickTests
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

        private Host _host;
        private POIEditorToolWindow _editor;
        private WallConfigData _config;
        private readonly Dictionary<string, Rect> _screenRects = new();

        private void Open(string sectionFoldoutField)
        {
            _config = new WallConfigData
            {
                pois = new List<POIData>(),
                marker_shape = "circle",
                marker_use_badge = true,
                marker_outline_mode = "uniform",
                effect_defaults = new EffectDefaults(),
                hierarchy_levels = new List<HierarchyLevelEntry>(),
                category_styles = new List<CategoryStyleEntry> { new CategoryStyleEntry { category = "cat_a", icon_key = "unknown", color_hex = "#336699", search_keywords = new List<string>() } },
                badge_categories = new List<BadgeCategoryEntry> { new BadgeCategoryEntry { key = "badge_a", icon_key = "unknown", color_hex = "#999999", search_keywords = new List<string>() } },
                outline_levels = new List<OutlineLevelEntry> { new OutlineLevelEntry { key = "level_1", label = "Intact", pct = 0f, line_style = "solid", search_keywords = new List<string>() } }
            };

            _editor = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            var type = typeof(POIEditorToolWindow);
            type.GetField("_config", Instance).SetValue(_editor, _config);
            // The framework library is already an asset: the Marker section then never has to create a wall library file
            type.GetField("_wallIconLibrary", Instance).SetValue(_editor,
                AssetDatabase.LoadAssetAtPath<SpriteKeyLibrary>("Assets/Framework/Runtime/UI/Markers/IconLibrary.asset"));
            type.GetField(sectionFoldoutField, Instance).SetValue(_editor, true);

            _screenRects.Clear();
            POIEditorToolWindow.TableCellRectProbe = (table, row, rect) =>
                _screenRects[table + "#" + row] = GUIUtility.GUIToScreenRect(rect);

            Host.Editor = _editor;
            Host.Repaints = 0;
            _host = ScriptableObject.CreateInstance<Host>();
            _host.ShowUtility();
            _host.position = new Rect(40f, 40f, 1800f, 900f);
        }

        [TearDown]
        public void TearDown()
        {
            POIEditorToolWindow.TableCellRectProbe = null;
            Host.Editor = null;
            GUIUtility.keyboardControl = 0;
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

        // A real left click 4 px inside the cell's reserved left edge, vertically centred
        private void ClickLeftEdge(string table)
        {
            Assert.IsTrue(_screenRects.TryGetValue(table + "#0", out Rect screenRect),
                "precondition: the '" + table + "' cell must have been drawn (probe saw no rect)");
            Vector2 local = new Vector2(screenRect.x + 4f, screenRect.center.y) - Host.RootScreen;
            GUIUtility.keyboardControl = 0;
            EditorGUIUtility.editingTextField = false;
            _host.SendEvent(new Event { type = EventType.MouseDown, button = 0, clickCount = 1, mousePosition = local });
            _host.SendEvent(new Event { type = EventType.MouseUp, button = 0, clickCount = 1, mousePosition = local });
        }

        [UnityTest]
        public IEnumerator CategoryCell_TakesAClickAtItsLeftEdge()
        {
            Open("_showGlobalMarker");
            yield return WaitForRepaint();
            ClickLeftEdge("Category");
            Assert.IsTrue(EditorGUIUtility.editingTextField, "clicking the left edge of the Category cell must start editing it");
        }

        [UnityTest]
        public IEnumerator BadgeKeyCell_TakesAClickAtItsLeftEdge()
        {
            Open("_showGlobalBadge");
            yield return WaitForRepaint();
            ClickLeftEdge("Badge Key");
            Assert.IsTrue(EditorGUIUtility.editingTextField, "clicking the left edge of the Badge Key cell must start editing it");
        }

        [UnityTest]
        public IEnumerator OutlineLabelCell_TakesAClickAtItsLeftEdge()
        {
            Open("_showGlobalOutline");
            yield return WaitForRepaint();
            ClickLeftEdge("Outline label");
            Assert.IsTrue(EditorGUIUtility.editingTextField, "clicking the left edge of the Outline label cell must start editing it");
        }
    }
}
