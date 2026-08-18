using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // Tier-0 structural + reflection guards for the POI authoring tool's
    // visual hierarchy: only the 2 top-level tab containers keep colored
    // boxes; per-POI header foldouts are bold + colored (PoiHeaderColor),
    // while the five Specific-Marker inner sub-sections render bold but
    // uncolored (editor-default foldout text via FoldoutDefaultColor). The
    // per-POI colored container is removed. These assertions prove the
    // intended wiring is on disk rather than absent due to a stale recompile.
    public class PoiAuthoringVisualHierarchyTests
    {
        private static Type WindowType => typeof(TileStories.Editor.POIAuthoringToolWindow);

        private static Color ReflectColor(string fieldName)
        {
            var fi = WindowType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(fi, $"Expected private static color field '{fieldName}' on POIAuthoringToolWindow");
            return (Color)fi.GetValue(null);
        }

        private static FieldInfo ReflectField(string fieldName)
        {
            return WindowType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
        }

        private static float ColorDistance(Color a, Color b)
        {
            float dr = a.r - b.r;
            float dg = a.g - b.g;
            float db = a.b - b.b;
            return (float)Math.Sqrt(dr * dr + dg * dg + db * db);
        }

        private const float ColorEpsilon = 0.02f;

        private static string ReadSource(string assetsRelativePath)
        {
            return File.ReadAllText(Path.Combine(Application.dataPath, assetsRelativePath));
        }

        private static int CountOccurrences(string source, string sub)
        {
            if (string.IsNullOrEmpty(sub)) return 0;
            int count = 0;
            int pos = 0;
            while ((pos = source.IndexOf(sub, pos, StringComparison.Ordinal)) >= 0)
            {
                count++;
                pos += sub.Length;
            }
            return count;
        }

        // Color constants drive the two outer containers and the inner titles.
        [Test]
        public void Color_Constants_ExpectedFieldsExist()
        {
            ReflectColor("GlobalSectionColor");
            ReflectColor("GlobalSceneTabColor");
            ReflectColor("SpecificMarkerTabColor");
            ReflectColor("SceneConfigSectionColor");
            ReflectColor("MarkerSectionColor");
            ReflectColor("BadgeSectionColor");
            ReflectColor("OutlineSectionColor");
            ReflectColor("EffectsSectionColor");
            ReflectColor("HierarchySectionColor");
            ReflectColor("LodSectionColor");
            ReflectColor("ZoomSectionColor");
            ReflectColor("SearchFilterSectionColor");
        }

        // The two now-removed colors must be gone so no dead references remain.
        [Test]
        public void Color_Constants_RemovedColorsAreGone()
        {
            Assert.IsNull(ReflectField("InnerSectionColor"),
                "InnerSectionColor was a leftover and should have been removed");
            Assert.IsNull(ReflectField("POISectionColor"),
                "POISectionColor drove the removed per-POI container and should be gone");
            Assert.IsNull(ReflectField("PositionSectionColor"),
                "PositionSectionColor drove a colored POI header that is now uncolored");
            Assert.IsNull(ReflectField("MarkerStyleSectionColor"),
                "MarkerStyleSectionColor drove a colored POI header that is now uncolored");
            Assert.IsNull(ReflectField("BadgeStyleSectionColor"),
                "BadgeStyleSectionColor drove a colored POI header that is now uncolored");
            Assert.IsNull(ReflectField("OutlineSectionColorPoi"),
                "OutlineSectionColorPoi drove a colored POI header that is now uncolored");
            Assert.IsNull(ReflectField("SearchKeywordsSectionColor"),
                "SearchKeywordsSectionColor drove a colored POI header that is now uncolored");
            Assert.IsNull(ReflectField("PoiHeaderColor"),
                "PoiHeaderColor single-color field replaced by PoiHeaderPalette and should be gone");
        }

        [Test]
        public void Color_Constants_TabAndSceneConfigAreDistinct()
        {
            Color global = ReflectColor("GlobalSceneTabColor");
            Color specific = ReflectColor("SpecificMarkerTabColor");
            Color sceneConfig = ReflectColor("SceneConfigSectionColor");
            Color globalSection = ReflectColor("GlobalSectionColor");

            // The two big top-level containers must be visually separable.
            Assert.That(ColorDistance(global, specific), Is.GreaterThan(ColorEpsilon));
            // Scene Configuration must not collide with either tab color.
            Assert.That(ColorDistance(sceneConfig, global), Is.GreaterThan(ColorEpsilon));
            Assert.That(ColorDistance(sceneConfig, specific), Is.GreaterThan(ColorEpsilon));
            Assert.That(ColorDistance(globalSection, global), Is.GreaterThan(ColorEpsilon));
        }

        [Test]
        public void Color_Constants_GlobalInnerPaletteIsDistinct()
        {
            // The 8 Global Scene inner sub-sections each carry their own color so
            // the bold titles are individually distinguishable (no monochrome sea).
            var inner = new (string, Color)[]
            {
                ("Marker", ReflectColor("MarkerSectionColor")),
                ("Badge", ReflectColor("BadgeSectionColor")),
                ("Outline", ReflectColor("OutlineSectionColor")),
                ("Effects", ReflectColor("EffectsSectionColor")),
                ("Hierarchy", ReflectColor("HierarchySectionColor")),
                ("Lod", ReflectColor("LodSectionColor")),
                ("Zoom", ReflectColor("ZoomSectionColor")),
                ("SearchFilter", ReflectColor("SearchFilterSectionColor")),
            };

            for (int i = 0; i < inner.Length; i++)
            for (int j = i + 1; j < inner.Length; j++)
            {
                Assert.That(ColorDistance(inner[i].Item2, inner[j].Item2),
                    Is.GreaterThan(ColorEpsilon),
                    $"Inner palette collision: {inner[i].Item1} ~= {inner[j].Item1}");
            }
        }

        // DrawFramedFoldout keeps its signature (bold colored title + content).
        [Test]
        public void Method_DrawFramedFoldout_HasExpectedSignature()
        {
            var m = WindowType.GetMethod("DrawFramedFoldout",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(m, "DrawFramedFoldout should still exist");
            var ps = m.GetParameters();
            Assert.AreEqual(4, ps.Length);
            Assert.AreEqual(typeof(Action), ps[1].ParameterType);
            Assert.AreEqual(typeof(string), ps[2].ParameterType);
            Assert.AreEqual(typeof(Color), ps[3].ParameterType);
        }

        // The outer-container helper is retained for the 2 big tab containers.
        [Test]
        public void Method_DrawTabContentContainer_HasExpectedSignature()
        {
            var m = WindowType.GetMethod("DrawTabContentContainer",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(m, "DrawTabContentContainer should still exist");
            var ps = m.GetParameters();
            Assert.AreEqual(4, ps.Length);
            Assert.AreEqual(typeof(Action), ps[0].ParameterType);
            Assert.AreEqual(typeof(Color), ps[1].ParameterType);
        }

        [Test]
        public void Method_CreateFoldoutStyle_HasExpectedSignature()
        {
            var m = WindowType.GetMethod("CreateFoldoutStyle",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(m, "CreateFoldoutStyle should still exist");
            var ps = m.GetParameters();
            Assert.AreEqual(1, ps.Length);
            Assert.AreEqual(typeof(Color), ps[0].ParameterType);
        }

        // Structural guard: per-POI colored container wrapper is gone and the
        // 5 inner foldouts render directly under the POI header.
        [Test]
        public void Source_SpecificMarker_PerPoiContainerWrapperRemoved()
        {
            string src = ReadSource(@"Framework\Editor\POIAuthoring\SpecificMarker\POIAuthoringToolWindow.SpecificMarker.cs");
            Assert.IsFalse(src.Contains("DrawTabContentContainer"),
                "SpecificMarker must no longer wrap per-POI content in DrawTabContentContainer");
            int foldouts = CountOccurrences(src, "DrawFramedFoldout(");
            Assert.AreEqual(5, foldouts,
                "SpecificMarker should still define 5 inner foldouts (Position, Marker Style, Badge Style, Outline, Search Keywords)");
        }

        // Structural guard: left-border accent removed from DrawFramedFoldout,
        // while the outer tab containers remain in the shell file.
        [Test]
        public void Source_MainFile_FramedFoldoutHasNoBorderAccent_ButOuterContainersRemain()
        {
            string src = ReadSource(@"Framework\Editor\POIAuthoring\POIAuthoringToolWindow.cs");
            Assert.IsFalse(src.Contains("ContourDrawer.DrawLeftBorder"),
                "DrawFramedFoldout's left-border accent call must be removed");
            Assert.IsFalse(src.Contains("borderRect"),
                "borderRect computation must be removed from DrawFramedFoldout");
            Assert.IsTrue(src.Contains("DrawTabContentContainer"),
                "The 2 outer tab containers (Global Scene + Specific Marker) must remain");
        }

        // Structural guard: the per-POI IndentLevelScope wrapper that nests
        // the five inner foldouts under each POI header is present in source.
        [Test]
        public void Source_SpecificMarker_PerPoiContainerWrapperPresent()
        {
            string src = ReadSource(@"Framework\Editor\POIAuthoring\SpecificMarker\POIAuthoringToolWindow.SpecificMarker.cs");
            Assert.IsTrue(src.Contains("IndentLevelScope"),
                "SpecificMarker must wrap per-POI sub-sections in EditorGUI.IndentLevelScope to nest under the POI header");
        }

        // Structural guard: no Space() spacer between the selected-tab color
        // restore and the tab content switch, so the container sits flush
        // beneath the tab button row.
        [Test]
        public void Source_MainFile_NoSpaceBetweenTabsAndTabContentContainer()
        {
            string src = ReadSource(@"Framework\Editor\POIAuthoring\POIAuthoringToolWindow.cs");
            int startIdx = src.IndexOf("originalBgColor", StringComparison.Ordinal);
            Assert.IsTrue(startIdx >= 0, "Expected originalBgColor anchor in main file");
            int endIdx = src.IndexOf("switch (_selectedTab)", startIdx, StringComparison.Ordinal);
            Assert.IsTrue(endIdx > startIdx, "Expected switch (_selectedTab) after originalBgColor");
            string slice = src.Substring(startIdx, endIdx - startIdx);
            Assert.IsFalse(slice.Contains("Space("),
                "No EditorGUILayout.Space(...) should appear between the tab-bg color restore and the tab content switch");
        }

        // Structural guard: POI header foldouts are bold + colored via the shared
        // CreateFoldoutStyle helper + PoiHeaderColorFor (stable per-POI palette
        // color), while the five inner sub-section foldouts pass the uncolored
        // FoldoutDefaultColor token.
        [Test]
        public void Source_SpecificMarker_PoiHeaderViaPalette_InnerSectionsUncolored()
        {
            string src = ReadSource(@"Framework\Editor\POIAuthoring\SpecificMarker\POIAuthoringToolWindow.SpecificMarker.cs");
            Assert.IsTrue(src.Contains("PoiHeaderColorFor"),
                "POI header foldouts must be colored via PoiHeaderColorFor");
            Assert.IsTrue(src.Contains("CreateFoldoutStyle(PoiHeaderColorFor"),
                "POI header foldout must be rendered bold+colored via CreateFoldoutStyle(PoiHeaderColorFor(...))");
            Assert.IsFalse(src.Contains("CreateFoldoutStyle(PoiHeaderColor)"),
                "old CreateFoldoutStyle(PoiHeaderColor) call must be gone, replaced by CreateFoldoutStyle(PoiHeaderColorFor(...))");
            Assert.AreEqual(5, CountOccurrences(src, "FoldoutDefaultColor"),
                "Each of the 5 inner sub-sections must use the uncolored FoldoutDefaultColor token");
            Assert.IsFalse(src.Contains("PositionSectionColor"),
                "PositionSectionColor must be gone from SpecificMarker source");
            Assert.IsFalse(src.Contains("MarkerStyleSectionColor"),
                "MarkerStyleSectionColor must be gone from SpecificMarker source");
            Assert.IsFalse(src.Contains("BadgeStyleSectionColor"),
                "BadgeStyleSectionColor must be gone from SpecificMarker source");
            Assert.IsFalse(src.Contains("OutlineSectionColorPoi"),
                "OutlineSectionColorPoi must be gone from SpecificMarker source");
            Assert.IsFalse(src.Contains("SearchKeywordsSectionColor"),
                "SearchKeywordsSectionColor must be gone from SpecificMarker source");
        }

        // Tier-0: the per-POI header palette is a static Color[] of at least 10 entries.
        [Test]
        public void Color_Constants_PoiHeaderPalette_ArrayPresent_WithAtLeastTenColors()
        {
            var fi = WindowType.GetField("PoiHeaderPalette",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(fi, "PoiHeaderPalette field should exist on POIAuthoringToolWindow");
            Color[] arr = fi.GetValue(null) as Color[];
            Assert.IsNotNull(arr, "PoiHeaderPalette should deserialize as a Color[]");
            Assert.That(arr.Length, Is.GreaterThanOrEqualTo(10),
                "PoiHeaderPalette should hold at least 10 colors");
        }

        // Tier-0: palette entries are visually distinct (no two near-collisions).
        [Test]
        public void Color_Constants_PoiHeaderPalette_SelfDistinct()
        {
            var fi = WindowType.GetField("PoiHeaderPalette",
                BindingFlags.NonPublic | BindingFlags.Static);
            Color[] arr = (Color[])fi.GetValue(null);
            Assert.That(arr.Length, Is.GreaterThanOrEqualTo(2));
            for (int i = 0; i < arr.Length; i++)
            for (int j = i + 1; j < arr.Length; j++)
            {
                Assert.That(ColorDistance(arr[i], arr[j]), Is.GreaterThan(ColorEpsilon),
                    "Palette colors must be visually distinct (no near-duplicates)");
            }
        }

        // Tier-0: PoiHeaderColorFor is deterministic per key, falls back to the
        // index seed for empty keys, and always resolves to a palette member.
        [Test]
        public void Method_PoiHeaderColorFor_ReturnsPaletteMember_Stable_AndFallsBackToIndex()
        {
            var fi = WindowType.GetField("PoiHeaderPalette",
                BindingFlags.NonPublic | BindingFlags.Static);
            var arr = (Color[])fi.GetValue(null);
            Assert.IsNotNull(arr);
            var m = WindowType.GetMethod("PoiHeaderColorFor",
                BindingFlags.NonPublic | BindingFlags.Static,
                null, new[] { typeof(string), typeof(int) }, null);
            Assert.IsNotNull(m, "PoiHeaderColorFor(string,int) should exist");

            Color a1 = (Color)m.Invoke(null, new object[] { "lamp_1", 0 });
            Color a2 = (Color)m.Invoke(null, new object[] { "lamp_1", 0 });
            Assert.AreEqual(a1, a2, "Same POI key must resolve to the same color across calls");
            Assert.That(Array.IndexOf(arr, a1), Is.GreaterThanOrEqualTo(0),
                "Keyed header color must be a palette member");

            Color b = (Color)m.Invoke(null, new object[] { "", 3 });
            Color c = (Color)m.Invoke(null, new object[] { "", 5 });
            Assert.That(Array.IndexOf(arr, b), Is.GreaterThanOrEqualTo(0),
                "Fallback header color (b) must be a palette member");
            Assert.That(Array.IndexOf(arr, c), Is.GreaterThanOrEqualTo(0),
                "Fallback header color (c) must be a palette member");
            Assert.AreNotEqual(b, c, "Distinct fallback seeds must resolve to distinct colors");

            bool[] seen = new bool[arr.Length];
            foreach (string id in new[] {
                "lamp_1", "lamp_2", "lamp_3", "lamp_4", "lamp_5",
                "lamp_6", "lamp_7", "lamp_8", "lamp_9", "lamp_10" })
            {
                Color col = (Color)m.Invoke(null, new object[] { id, 0 });
                int idx = Array.IndexOf(arr, col);
                Assert.That(idx, Is.GreaterThanOrEqualTo(0),
                    "ColorFor result must be a palette member");
                seen[idx] = true;
            }
            int distinct = 0;
            foreach (bool f in seen)
                if (f)
                    distinct++;
            Assert.That(distinct, Is.GreaterThanOrEqualTo(2),
                "Distinct POI keys should distribute across at least 2 palette colors");
        }

        // Tier-0: the tab button row renders above BeginScrollView (pinned header).
        [Test]
        public void Source_MainFile_TabButtonsAboveScrollView()
        {
            string src = ReadSource(@"Framework\Editor\POIAuthoring\POIAuthoringToolWindow.cs");
            int labelIdx = src.IndexOf("Specific Marker", StringComparison.Ordinal);
            Assert.IsTrue(labelIdx >= 0, "Expected the 'Specific Marker' tab button label in source");
            int scrollIdx = src.IndexOf("BeginScrollView", labelIdx, StringComparison.Ordinal);
            Assert.IsTrue(scrollIdx > labelIdx,
                "BeginScrollView must open AFTER the Specific Marker tab button (tabs pinned above scroll)");
            string between = src.Substring(labelIdx, scrollIdx - labelIdx);
            Assert.IsFalse(between.Contains("BeginScrollView"),
                "BeginScrollView must not appear between the tab label and itself (sanity check)");
            Assert.IsFalse(between.Contains("Space("),
                "No Space() spacer should appear between the tab button row and BeginScrollView");
        }

        // Tier-0: DrawTabContentContainer must not call GetLastRect directly after the
        // indent group begins when no label precedes the content (that throws
        // "You cannot call GetLast immediately after beginning a group").
        [Test]
        public void Source_MainFile_TabContentContainer_ReservesRectWhenNoLabel()
        {
            string src = ReadSource(@"Framework\Editor\POIAuthoring\POIAuthoringToolWindow.cs");
            Assert.IsTrue(src.Contains("GetControlRect(false, 0f"),
                "Empty-label path must reserve a zero-height control rect instead of GetLastRect-after-group");
            // Match the qualified call site: a comment may name the API by its
            // bare name, but never as GUILayoutUtility.GetLastRect(), so the first
            // real call site must follow a LabelField draw.
            int grIdx = src.IndexOf("GUILayoutUtility.GetLastRect()", StringComparison.Ordinal);
            int labelIdx = src.IndexOf("EditorGUILayout.LabelField", StringComparison.Ordinal);
            Assert.IsTrue(grIdx > labelIdx,
                "GUILayoutUtility.GetLastRect() should only be used after a control/label has been drawn");
        }

    }
}
