using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Editor.Tests
{
    // A POI's Category / Badge category / Hierarchy Level / Status level popups REFERENCE a
    // taxonomy row by key. A plain popup shows index 0 for a key it cannot find and writes index 0
    // back on the next pass, so merely opening a POI whose key went stale (row deleted, typo in the
    // JSON) silently rewrote it. ReferencePopupOptions keeps the stale key as "<key> (missing)".
    public class PoiReferencePopupTests
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;

        // ---------------- pure option building ----------------

        [Test]
        public void Build_KnownKey_IsSelected_AndMapsBackToItself()
        {
            var o = ReferencePopupOptions.Build(new[] { "a", "b" }, new[] { "A", "B" }, "b", allowNone: false);
            CollectionAssert.AreEqual(new[] { "A", "B" }, o.Labels);
            Assert.AreEqual(1, o.SelectedIndex);
            Assert.AreEqual("b", o.KeyAt(o.SelectedIndex));
        }

        [Test]
        public void Build_AllowNone_OffersNoneFirst_AndNoneMapsToNull()
        {
            var o = ReferencePopupOptions.Build(new[] { "a" }, new[] { "A" }, null, allowNone: true);
            CollectionAssert.AreEqual(new[] { ReferencePopupOptions.NoneLabel, "A" }, o.Labels);
            Assert.AreEqual(0, o.SelectedIndex);
            Assert.IsNull(o.KeyAt(0));
            Assert.AreEqual("a", o.KeyAt(1));
        }

        [Test]
        public void Build_StaleKey_StaysSelected_AsMissing_InsteadOfTheFirstRow()
        {
            var o = ReferencePopupOptions.Build(new[] { "a", "b" }, new[] { "A", "B" }, "gone", allowNone: true);
            Assert.AreEqual("gone" + ReferencePopupOptions.MissingSuffix, o.Labels[o.SelectedIndex]);
            Assert.AreEqual("gone", o.KeyAt(o.SelectedIndex), "the stale key must be what the popup hands back");
            Assert.AreEqual(4, o.Labels.Length, "(none), A, B, gone (missing)");
        }

        [Test]
        public void Build_BlankKey_WithoutNoneOption_ShowsNone_NotTheFirstRow()
        {
            var o = ReferencePopupOptions.Build(new[] { "a" }, new[] { "A" }, "", allowNone: false);
            Assert.AreEqual(ReferencePopupOptions.NoneLabel, o.Labels[o.SelectedIndex]);
        }

        [Test]
        public void Build_BlankLabel_FallsBackToTheKey_AndDuplicateKeysAreListedOnce()
        {
            var o = ReferencePopupOptions.Build(new[] { "a", "a", "b" }, new[] { "", "A2", null }, "a", allowNone: false);
            CollectionAssert.AreEqual(new[] { "a", "b" }, o.Labels);
        }

        // ---------------- the real window ----------------

        // Hosts the real POIEditorToolWindow.OnGUI (same host pattern as HierarchyTableClickTests)
        private sealed class Host : EditorWindow
        {
            public static POIEditorToolWindow Editor;
            public static int Repaints;

            private void OnGUI()
            {
                if (Editor == null) return;
                typeof(POIEditorToolWindow).GetMethod("OnGUI", Instance).Invoke(Editor, null);
                if (Event.current.type == EventType.Repaint) Repaints++;
            }
        }

        [UnityTest]
        public IEnumerator OpeningAPoiWithStaleReferences_DoesNotRewriteThem()
        {
            var poi = new POIData
            {
                id = "stale_poi", name = "Stale", category = "gone_category", badge_category = "gone_badge",
                hierarchy_level_key = "gone_level", has_status = true, status_level_key = "gone_status", status_pct = 42f,
                search_keywords = new List<string>(), search_keyword_fields = new List<POISearchKeywordField>()
            };
            var config = new WallConfigData
            {
                marker_use_badge = true,
                marker_outline_mode = "uniform",
                effect_defaults = new EffectDefaults(),
                category_styles = new List<CategoryStyleEntry> { new CategoryStyleEntry { category = "real_category", icon_key = "unknown" } },
                badge_categories = new List<BadgeCategoryEntry> { new BadgeCategoryEntry { key = "real_badge", icon_key = "unknown" } },
                outline_levels = new List<OutlineLevelEntry> { new OutlineLevelEntry { key = "real_status", label = "Real", pct = 0f, line_style = "solid" } },
                hierarchy_levels = new List<HierarchyLevelEntry> { new HierarchyLevelEntry { key = "real_level", level_name = "Real", size_cm = 20f } },
                pois = new List<POIData> { poi }
            };

            var editor = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            var type = typeof(POIEditorToolWindow);
            type.GetField("_config", Instance).SetValue(editor, config);
            var tabField = type.GetField("_selectedTab", Instance);
            tabField.SetValue(editor, Enum.Parse(tabField.FieldType, "SpecificMarker"));
            ((Dictionary<string, bool>)type.GetField("_poiFoldouts", Instance).GetValue(editor))[poi.id] = true;
            foreach (string foldout in new[] { "_showPoiMarkerStyle", "_showPoiBadgeStyle", "_showPoiOutline" })
                type.GetField(foldout, Instance).SetValue(editor, true);

            Host.Editor = editor;
            Host.Repaints = 0;
            var host = ScriptableObject.CreateInstance<Host>();
            host.ShowUtility();
            host.position = new Rect(40f, 40f, 900f, 900f);
            try
            {
                for (int pass = 0; pass < 3; pass++)
                {
                    int before = Host.Repaints;
                    host.Repaint();
                    for (int i = 0; i < 120 && Host.Repaints == before; i++) yield return null;
                }
                Assert.GreaterOrEqual(Host.Repaints, 3, "precondition: the window must really have drawn the open POI");

                Assert.AreEqual("gone_category", poi.category, "Category");
                Assert.AreEqual("gone_badge", poi.badge_category, "Badge category");
                Assert.AreEqual("gone_level", poi.hierarchy_level_key, "Hierarchy Level");
                Assert.AreEqual("gone_status", poi.status_level_key, "Status level");
                Assert.AreEqual(42f, poi.status_pct, "Status level must not rewrite the percentage either");
                Assert.IsFalse((bool)type.GetField("_hasUnsavedChanges", Instance).GetValue(editor),
                    "just looking at a POI must not count as an edit");
            }
            finally
            {
                Host.Editor = null;
                host.Close();
                UnityEngine.Object.DestroyImmediate(editor);
            }
        }
    }
}
