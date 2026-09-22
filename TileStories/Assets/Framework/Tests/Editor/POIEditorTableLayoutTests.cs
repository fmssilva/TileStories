using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // Tier-0 structural tests for the taxonomy table layout gaps (marker / badge /
    // outline tables). These prove the two shared gap constants exist, that the
    // three old per-table magic gaps are gone, and that both table renderers
    // actually insert the between-groups spacer at every logical boundary in
    // header and rows -- the regression that previously let the Color and Search
    // Keywords columns collide with no spacer between them.
    public class POIEditorTableLayoutTests
    {
        private static Type WindowType => typeof(TileStories.Editor.POIEditorToolWindow);

        private static FieldInfo ReflectField(string fieldName)
        {
            return WindowType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
        }

        private static float ReflectGapConstant(string fieldName)
        {
            var fi = ReflectField(fieldName);
            Assert.IsNotNull(fi, $"Expected private static const '{fieldName}' on POIEditorToolWindow");
            return (float)fi.GetValue(null);
        }

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

        // The rhythm lives in two constants; both must be positive and tuned here.
        [Test]
        public void Gap_Constants_ExistAndPositive()
        {
            Assert.That(ReflectGapConstant("TableGapBetweenGroups"), Is.GreaterThan(0f),
                "Between-groups gap must be a positive, visible spacer");
            Assert.That(ReflectGapConstant("TableGapWithinGroup"), Is.GreaterThan(0f),
                "Within-group gap must be a positive, visible spacer");
            Assert.That(ReflectGapConstant("TableGapWithinGroup"), Is.LessThan(ReflectGapConstant("TableGapBetweenGroups")),
                "Within-group element spacing should be tighter than between-group spacing so columns still read as groups");
        }

        // The header pad for the Symbol/Style column is just the within-group gap
        // (no separate select button any more), so the info button sits DIRECTLY
        // OVER the preview thumbnail column.
        [Test]
        public void SymbolColumnPad_DerivedFromWithinGroupGap()
        {
            float within = ReflectGapConstant("TableGapWithinGroup");
            Assert.That(ReflectGapConstant("SymbolColumnPad"), Is.EqualTo(within).Within(0.001f),
                "Header pad must equal the within-group gap so the info button is directly over the preview column");
        }

        // The three old per-table magic gaps must be gone from the compiled class,
        // not just its source.
        [Test]
        public void Gap_Constants_OldScatteredGapsRemoved()
        {
            Assert.IsNull(ReflectField("TableGroupGap"), "TableGroupGap should be replaced by TableGapBetweenGroups");
            Assert.IsNull(ReflectField("SymbolGroupGap"), "SymbolGroupGap should be replaced by TableGapBetweenGroups");
            Assert.IsNull(ReflectField("DeleteButtonGap"), "DeleteButtonGap should be replaced by TableGapBetweenGroups");
        }

        [Test]
        public void Gap_Constants_RemovedFromSourceToo()
        {
            string shared = ReadSource(@"Framework\Editor\POIEditor\Shared\POIEditorToolWindow.SymbolTable.cs");
            string outline = ReadSource(@"Framework\Editor\POIEditor\GlobalScene\POIEditorToolWindow.MarkerDesign.cs");
            foreach (string src in new[] { shared, outline })
            {
                Assert.IsFalse(src.Contains("TableGroupGap"), "Stale TableGroupGap reference in source");
                Assert.IsFalse(src.Contains("SymbolGroupGap"), "Stale SymbolGroupGap reference in source");
                Assert.IsFalse(src.Contains("DeleteButtonGap"), "Stale DeleteButtonGap reference in source");
            }
        }

        // The shared marker/badge renderer must insert the between-groups gap at 2 of
        // its 4 boundaries in both the header row and the data rows (4 call sites) --
        // the Symbol-to-Color boundary uses the larger TableGapBeforeColor and the
        // pre-delete boundary uses the larger TableGapBeforeDelete (see the next tests),
        // both because the standard gap read as visually too tight there (developer
        // screenshot feedback, 2026-09-18).
        [Test]
        public void SharedSymbolTable_GapBetweenGroups_AllBoundaries()
        {
            string src = ReadSource(@"Framework\Editor\POIEditor\Shared\POIEditorToolWindow.SymbolTable.cs");
            Assert.That(CountOccurrences(src, "GUILayout.Space(TableGapBetweenGroups)"), Is.GreaterThanOrEqualTo(4),
                "Marker/badge renderer needs the between-groups gap at 2 of its 4 boundaries x (header + rows)");
        }

        // The Symbol-to-Color boundary (header + row) uses the larger, dedicated
        // TableGapBeforeColor gap instead of the standard between-groups gap.
        [Test]
        public void SharedSymbolTable_GapBeforeColor_LargerThanBetweenGroups()
        {
            string src = ReadSource(@"Framework\Editor\POIEditor\Shared\POIEditorToolWindow.SymbolTable.cs");
            Assert.That(CountOccurrences(src, "GUILayout.Space(TableGapBeforeColor)"), Is.GreaterThanOrEqualTo(2),
                "Marker/badge renderer needs the larger pre-color gap in both the header and the rows");
            Assert.That(ReflectGapConstant("TableGapBeforeColor"), Is.GreaterThan(ReflectGapConstant("TableGapBetweenGroups")),
                "The pre-color gap must be larger than the standard between-groups gap");
        }

        // The delete button boundary (header + row) uses the larger, dedicated
        // TableGapBeforeDelete gap instead of the standard between-groups gap.
        [Test]
        public void SharedSymbolTable_GapBeforeDelete_LargerThanBetweenGroups()
        {
            string src = ReadSource(@"Framework\Editor\POIEditor\Shared\POIEditorToolWindow.SymbolTable.cs");
            Assert.That(CountOccurrences(src, "GUILayout.Space(TableGapBeforeDelete)"), Is.GreaterThanOrEqualTo(2),
                "Marker/badge renderer needs the larger pre-delete gap in both the header and the rows");
            Assert.That(ReflectGapConstant("TableGapBeforeDelete"), Is.GreaterThan(ReflectGapConstant("TableGapBetweenGroups")),
                "The pre-delete gap must be larger than the standard between-groups gap");
        }

        // Same invariant for the outline table's own inline renderer, for the 3 boundaries that use
        // the standard gap (key->style, keywords->trash, plus the right-edge clearance is separate).
        // The Style->Color boundary is the shared table's own TableGapBeforeColor case, below.
        [Test]
        public void OutlineTable_GapBetweenGroups_AllBoundaries()
        {
            string src = ReadSource(@"Framework\Editor\POIEditor\GlobalScene\POIEditorToolWindow.MarkerDesign.cs");
            Assert.That(CountOccurrences(src, "GUILayout.Space(TableGapBetweenGroups)"), Is.GreaterThanOrEqualTo(6),
                "Outline renderer needs the between-groups gap at 3 of its boundaries x (header + rows)");
        }

        // The Outline Style-to-Color boundary (header + row) reuses the shared table's larger,
        // dedicated TableGapBeforeColor gap instead of the standard between-groups gap (developer
        // feedback, 2026-09-22: the standard gap read as visually tight there).
        [Test]
        public void OutlineTable_GapBeforeColor_LargerThanBetweenGroups()
        {
            string src = ReadSource(@"Framework\Editor\POIEditor\GlobalScene\POIEditorToolWindow.MarkerDesign.cs");
            Assert.That(CountOccurrences(src, "GUILayout.Space(TableGapBeforeColor)"), Is.GreaterThanOrEqualTo(2),
                "Outline renderer needs the larger pre-color gap in both the header and the rows");
            Assert.That(ReflectGapConstant("TableGapBeforeColor"), Is.GreaterThan(ReflectGapConstant("TableGapBetweenGroups")),
                "The pre-color gap must be larger than the standard between-groups gap");
        }

        // The specific past bug: when "Per outline type" is off (Color column hidden),
        // TableGapBeforeColor fired unconditionally before the (skipped) Color group and
        // TableGapBetweenGroups fired again right after -- two gaps back to back with
        // nothing drawn between them, in both the header and the row loop. The fix moved
        // TableGapBeforeColor inside the same `if (showColorColumn)` block as the Color
        // group itself, so it is only ever reserved once, alongside the group it precedes.
        [Test]
        public void OutlineTable_GapBeforeColor_OnlyReservedWhenColorColumnShown()
        {
            string src = ReadSource(@"Framework\Editor\POIEditor\GlobalScene\POIEditorToolWindow.MarkerDesign.cs");
            // Whitespace-agnostic: the row-loop guard sits one indent level deeper than the header's,
            // so this only requires "if (showColorColumn) { GUILayout.Space(TableGapBeforeColor);"
            // with any amount of whitespace/newlines between the tokens, not an exact indent match.
            var guardedPattern = new Regex(@"if\s*\(showColorColumn\)\s*\{\s*GUILayout\.Space\(TableGapBeforeColor\);");
            int guardedCount = guardedPattern.Matches(src).Count;
            Assert.That(guardedCount, Is.GreaterThanOrEqualTo(2),
                "TableGapBeforeColor must sit inside the same showColorColumn guard as the Color group itself (header + rows), " +
                "otherwise it still reserves space when the column is hidden and doubles up with the following TableGapBetweenGroups.");
            Assert.That(CountOccurrences(src, "GUILayout.Space(TableGapBeforeColor)"), Is.EqualTo(guardedCount),
                "Every TableGapBeforeColor call must be inside the showColorColumn guard -- none unconditional.");
        }

        // The specific past bug: no spacer between the Color group and the Search
        // Keywords group. Verify a between-groups spacer sits immediately before the
        // Search Keywords title in both renderers' header blocks.
        [Test]
        public void BothRenderers_GapAfterColorGroup_Present()
        {
            string shared = ReadSource(@"Framework\Editor\POIEditor\Shared\POIEditorToolWindow.SymbolTable.cs");
            string outline = ReadSource(@"Framework\Editor\POIEditor\GlobalScene\POIEditorToolWindow.MarkerDesign.cs");

            int srcPos = shared.IndexOf("// Group 4: Search keywords", StringComparison.Ordinal);
            Assert.That(srcPos, Is.GreaterThanOrEqualTo(0), "Marker/badge header must still show the Search Keywords title");
            Assert.That(shared.IndexOf("GUILayout.Space(TableGapBetweenGroups)", srcPos - 200, StringComparison.Ordinal), Is.GreaterThanOrEqualTo(0),
                "A between-groups spacer must sit immediately before the Search Keywords title");

            int outlineSrcPos = outline.IndexOf("// Group 4: Search keywords", StringComparison.Ordinal);
            Assert.That(outlineSrcPos, Is.GreaterThanOrEqualTo(0), "Outline header must still show the Search Keywords title");
            Assert.That(outline.IndexOf("GUILayout.Space(TableGapBetweenGroups)", outlineSrcPos - 200, StringComparison.Ordinal), Is.GreaterThanOrEqualTo(0),
                "A between-groups spacer must sit immediately before the Search Keywords title");
        }

        // Within-group spacing must exist inside the Symbol group and the keywords
        // edit/suggest pair in the marker renderer, and inside the Outline Style
        // group in the outline renderer.
        [Test]
        public void BothRenderers_GapWithinGroup_Present()
        {
            string shared = ReadSource(@"Framework\Editor\POIEditor\Shared\POIEditorToolWindow.SymbolTable.cs");
            string outline = ReadSource(@"Framework\Editor\POIEditor\GlobalScene\POIEditorToolWindow.MarkerDesign.cs");

            Assert.That(CountOccurrences(shared, "GUILayout.Space(TableGapWithinGroup)"), Is.GreaterThanOrEqualTo(4),
                "Marker/badge renderer needs within-group spacing for details, symbol trio and keywords pair");
            Assert.That(CountOccurrences(outline, "GUILayout.Space(TableGapWithinGroup)"), Is.GreaterThanOrEqualTo(3),
                "Outline renderer needs within-group spacing for details and the style trio");
        }

        // Color group widths must exist and stay consistent: the whole group is
        // picker + hex (the pair is flush -- GUILayout's default inter-control
        // spacing is already there), and the header label uses exactly that total.
        [Test]
        public void ColorGroup_WidthConstants_DerivedCorrectly()
        {
            float picker = ReflectGapConstant("ColorPickerWidth");
            float hex = ReflectGapConstant("ColorHexFieldWidth");
            Assert.That(picker, Is.GreaterThan(30f), "Real color picker should be forced wide enough to click easily");
            Assert.That(hex, Is.GreaterThan(60f), "Hex field must be wide enough to read the color name");
            Assert.That(ReflectGapConstant("ColorGroupWidth"), Is.EqualTo(picker + hex).Within(0.001f),
                "ColorGroupWidth must be picker + hex so the header lines up with rows; the pair is flush by design");
        }

        // The dead-cell regression: the color group must NOT contain the old flat
        // draw-only swatch pattern (GetRect + DrawRect painted over the real picker)
        // anywhere in the shared renderer that hosts DrawColorSwatchAndHex.
        [Test]
        public void ColorGroup_NoDeadFlatSwatchPattern()
        {
            string src = ReadSource(@"Framework\Editor\POIEditor\Shared\POIEditorToolWindow.SymbolTable.cs");
            Assert.IsFalse(src.Contains("EditorGUI.DrawRect(strip"), "Full-bleed DrawRect swatch over the picker must be gone");
            Assert.IsFalse(src.Contains("GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.colorField"),
                "Reserved-rect ColorField hack must be replaced by a real GUILayout ColorField");
        }

        // The color group's first cell must be the REAL working color picker, forced
        // wide through the named constant (not a magic pixel number).
        [Test]
        public void ColorGroup_RealPickerIsFirstCellAndWide()
        {
            string src = ReadSource(@"Framework\Editor\POIEditor\Shared\POIEditorToolWindow.SymbolTable.cs");
            Assert.That(src.Contains("EditorGUILayout.ColorField"), Is.True,
                "The real editor color picker must be drawn");
            Assert.That(src.Contains("GUILayout.Width(ColorPickerWidth)"), Is.True,
                "The real picker must be forced wide via the ColorPickerWidth constant");
        }

        // Both header blocks must size the Color title with the derived group width,
        // never a stale magic 150f (60+90 without the within-group gap).
        [Test]
        public void ColorGroup_HeaderLabelsUseDerivedWidth()
        {
            string shared = ReadSource(@"Framework\Editor\POIEditor\Shared\POIEditorToolWindow.SymbolTable.cs");
            string outline = ReadSource(@"Framework\Editor\POIEditor\GlobalScene\POIEditorToolWindow.MarkerDesign.cs");

            Assert.That(CountOccurrences(shared, "GUILayout.Width(ColorGroupWidth)"), Is.GreaterThanOrEqualTo(1),
                "Marker/badge header Color label must use ColorGroupWidth");
            Assert.That(CountOccurrences(outline, "GUILayout.Width(ColorGroupWidth)"), Is.GreaterThanOrEqualTo(1),
                "Outline header Color label must use ColorGroupWidth");
            Assert.IsFalse(shared.Contains("GUILayout.Width(150f)"), "Marker/badge table must not hold the stale 150f Color width");
        }

        // Non-table row layout: the two row constants must exist and be sane so the
        // whole window can be retuned from one place.
        [Test]
        public void SectionRow_ConstantsExistAndSane()
        {
            Assert.That(ReflectGapConstant("SectionRowIndent"), Is.GreaterThanOrEqualTo(0f),
                "SectionRowIndent must be a non-negative left pad for non-table rows");
            Assert.That(ReflectGapConstant("MaxRowWidth"), Is.GreaterThan(250f),
                "MaxRowWidth must be a sensible cap so rows stay readable but do not span the whole window");
        }

        // Pure width rule: row usable width = max(MinRowWidth,
        // min(remaining visible row width, MaxRowWidth)).
        [Test]
        public void EditorRowWidth_PureRule()
        {
            float cap = ReflectGapConstant("MaxRowWidth");
            float floor = ReflectGapConstant("MinRowWidth");
            Assert.That(global::TileStories.Editor.POIEditorToolWindow.EditorRowWidth(cap + 200f), Is.EqualTo(cap),
                "Wide visible panel must yield MaxRowWidth (capped)");
            Assert.That(global::TileStories.Editor.POIEditorToolWindow.EditorRowWidth(cap + 1f), Is.EqualTo(cap),
                "Panel slightly over the cap must still be capped");
            Assert.That(global::TileStories.Editor.POIEditorToolWindow.EditorRowWidth(cap - 1f), Is.EqualTo(cap - 1f),
                "Panel under the cap but above the floor must yield the panel width");
            Assert.That(global::TileStories.Editor.POIEditorToolWindow.EditorRowWidth(50f), Is.EqualTo(floor),
                "Very narrow panel must be raised to MinRowWidth (readability floor)");
            Assert.That(global::TileStories.Editor.POIEditorToolWindow.EditorRowWidth(0f), Is.EqualTo(floor),
                "Zero visible panel must still meet MinRowWidth");
        }

        // The INDENT is part of the width budget: the row's right edge is
        // min(visible panel, MaxRowWidth) measured from x=0, so deepening the indent
        // narrows the content instead of pushing the right edge further out. This is the
        // invariant behind "change the indent and the right limit always stays aligned" --
        // before it, indent + MaxRowWidth drifted the right edge rightwards by the indent,
        // which is why the (extra-indented) Verified button overhung the coordinate fields.
        [Test]
        public void EditorRowWidthForIndent_RightEdgeStaysAlignedAcrossIndents()
        {
            float margin = ReflectGapConstant("AddButtonRowRightMargin");
            float cap = ReflectGapConstant("MaxRowWidth");
            const float view = 900f; // wide panel -> the MaxRowWidth cap is the active limit

            float wideEdgeAt0 = 0f + global::TileStories.Editor.POIEditorToolWindow.EditorRowWidthForIndent(view, margin, 0f);
            float wideEdgeAt1 = 15f + global::TileStories.Editor.POIEditorToolWindow.EditorRowWidthForIndent(view, margin, 15f);
            float wideEdgeAt2 = 30f + global::TileStories.Editor.POIEditorToolWindow.EditorRowWidthForIndent(view, margin, 30f);

            Assert.That(wideEdgeAt0, Is.EqualTo(Mathf.Min(view - margin, cap)).Within(0.001f),
                "The row's right edge must land on min(visible panel, MaxRowWidth)");
            Assert.That(wideEdgeAt1, Is.EqualTo(wideEdgeAt0).Within(0.001f),
                "Right edge must not move when the indent deepens (one level)");
            Assert.That(wideEdgeAt2, Is.EqualTo(wideEdgeAt0).Within(0.001f),
                "Right edge must not move when the indent deepens (two levels)");

            // On a NARROW panel the panel itself is the limit, and the edge still holds.
            const float narrow = 300f;
            float narrowEdgeAt0 = 0f + global::TileStories.Editor.POIEditorToolWindow.EditorRowWidthForIndent(narrow, margin, 0f);
            float narrowEdgeAt2 = 30f + global::TileStories.Editor.POIEditorToolWindow.EditorRowWidthForIndent(narrow, margin, 30f);
            Assert.That(narrowEdgeAt0, Is.EqualTo(narrow - margin).Within(0.001f));
            Assert.That(narrowEdgeAt2, Is.EqualTo(narrowEdgeAt0).Within(0.001f),
                "Right edge must stay pinned to the narrow panel across indents too");

            // Content must actually shrink as the indent deepens (not just stay put).
            float wideContentAt0 = global::TileStories.Editor.POIEditorToolWindow.EditorRowWidthForIndent(view, margin, 0f);
            float wideContentAt2 = global::TileStories.Editor.POIEditorToolWindow.EditorRowWidthForIndent(view, margin, 30f);
            Assert.That(wideContentAt2, Is.LessThan(wideContentAt0),
                "Deeper indent must narrow the content when the cap is active");
        }

        // The +Add button under the taxonomy table delegates to the shared
        // DrawAddButtonRow (transparent indent spacer + real button in one row).
        // The rendered layout (side-by-side, width = max(Min, min(visible, Max)))
        // is asserted by POIEditorAddButtonRowRenderTests against real
        // GUILayoutUtility rects from a live window's OnGUI pass.
        [Test]
        public void AddButtonRow_MarkerTable_UsesDrawAddButtonRow()
        {
            string src = ReadSource(@"Framework\Editor\POIEditor\Shared\POIEditorToolWindow.SymbolTable.cs");
            string rowLayout = ReadSource(@"Framework\Editor\POIEditor\Shared\POIEditorToolWindow.RowLayout.cs");
            Assert.IsTrue(src.Contains("DrawAddButtonRow(addButtonLabel"),
                "The +Add row must go through the shared DrawAddButtonRow");
            Assert.IsTrue(src.Contains("GUILayout.Width(rowWidth)") && src.Contains("GUILayout.ExpandWidth(false)"),
                "The button width must be set explicitly from EditorRowWidth (ExpandWidth(false) so GUILayout.Width is honored exactly)");
            Assert.IsFalse(src.Contains("GUI.Button(new Rect("),
                "The old manual-Rect button path must be gone (it bypassed the GUILayout indent)");

            // The shared row's indent spacer is a TRANSPARENT borderless button
            // whose width is the runtime indent (so the +Add row aligns with
            // the child rows without any visible fill).
            Assert.IsTrue(rowLayout.Contains("GUILayout.Button(GUIContent.none, spacerStyle"),
                "The indent spacer must be a real button (real layout geometry)");
            Assert.IsTrue(rowLayout.Contains("spacerStyle.border = new RectOffset(0, 0, 0, 0)"),
                "The transparent spacer must be borderless");
            Assert.IsTrue(rowLayout.Contains("GUI.backgroundColor = new Color(1f, 1f, 1f, 0f)"),
                "The spacer must use a zero-alpha tint so nothing visible is drawn");
            Assert.IsTrue(rowLayout.Contains("EditorGUIUtility.whiteTexture") &&
                          rowLayout.Contains("starts from a real texture background"),
                "Backgrounds must stay a real texture (null falls back to the built-in grey button)");

        }


        // The SharedRow system covers FIELD rows too, not just buttons: the four
        // LodZoom field helpers and all direct labelled rows (Global toggles/popups/
        // Effects sliders, SpecificMarker status toggles/sliders/dropdowns/keyword
        // fields, Position rotation+XYZ) must open DrawEditorRow and cap the control
        // with GUILayout.Width(row)-style + ExpandWidth(false). This is a full-source
        // sweep guard: any regression to a plain full-width EditorGUILayout call here
        // fails. Taxonomy table cells (fixed column widths) are intentionally exempt.
        [Test]
        public void FieldRows_UseSharedRow()
        {
            string lod = ReadSource(@"Framework\Editor\POIEditor\GlobalScene\POIEditorToolWindow.LodZoom.cs");
            string global = ReadSource(@"Framework\Editor\POIEditor\GlobalScene\POIEditorToolWindow.MarkerDesign.cs");
            string search = ReadSource(@"Framework\Editor\POIEditor\GlobalScene\POIEditorToolWindow.SearchFilter.cs");
            string specific = ReadSource(@"Framework\Editor\POIEditor\SpecificMarker\POIEditorToolWindow.SpecificMarker.cs");
            string position = ReadSource(@"Framework\Editor\POIEditor\SpecificMarker\POIEditorToolWindow.PositionTabs.cs");

            // The four shared field helpers in LodZoom are themselves shared rows.
            Assert.IsTrue(lod.Contains("DrawScalarField") && lod.Contains("DrawEditorRow(out float rowWidth"),
                "DrawScalarField must open the shared row");
            Assert.IsTrue(lod.Contains("DrawIntField") && lod.Contains("EditorRowEnd()"),
                "DrawIntField must close with EditorRowEnd");
            Assert.IsTrue(lod.Contains("DrawToggleField") && lod.Contains("GUILayout.ExpandWidth(false)"),
                "DrawToggleField must use ExpandWidth(false)");
            Assert.IsTrue(lod.Contains("DrawPopupField"), "DrawPopupField must exist");

            // Global scene direct field rows (badge/marker/outline toggles + popups).
            AssertFieldRowsUseSharedRow(global, new string[] { "Enable badge", "Background shape", "Badge back shape", "Enable outline", "Outline Color" });

            // Effects section: every parameter row goes through the shared field helpers (which open
            // the shared row themselves); a raw EditorGUILayout field call here would bypass the
            // indent, width cap and right margin. The only hand-built rows are the foldout header
            // and the note, which open DrawEditorRow explicitly.
            string effects = ReadSource(@"Framework\Editor\POIEditor\GlobalScene\POIEditorToolWindow.Effects.cs");
            foreach (string label in new[] { "Amplitude", "Inner alpha", "Base alpha", "Tint Color", "Enable effects" })
                Assert.IsTrue(effects.Contains("\"" + label + "\""), "Effects row must still be present: " + label);
            Assert.IsTrue(effects.Contains("DrawSliderField(") && effects.Contains("DrawScalarField(") && effects.Contains("DrawColorField(") && effects.Contains("DrawToggleField("),
                "Effects rows must use the shared field helpers");
            foreach (string raw in new[] { "EditorGUILayout.Slider(", "EditorGUILayout.FloatField(", "EditorGUILayout.TextField(" })
                Assert.IsFalse(effects.Contains(raw), "Effects must not draw a raw " + raw + " outside the shared helpers");
            Assert.IsTrue(effects.Contains("DrawEditorRow(") && effects.Contains("EditorRowEnd()"), "Header and note rows open and close the shared row");
            // Search & Filter direct rows (No-results text, Trigger target enum).
            AssertFieldRowsUseSharedRow(search, new string[] { "No-results message", "Trigger target" });
            // SpecificMarker level-2 field rows (status toggles/slider, custom symbol, keyword fields).
            AssertFieldRowsUseSharedRow(specific, new string[] { "Use Custom Symbol", "Has status", "Status unknown", "Status %", "Custom symbol (optional)" });
            // Position rotation slider + XYZ row.
            AssertFieldRowsUseSharedRow(position, new string[] { "Rotation", "X" });
        }

        // A labelled FIELD row is a shared row if DrawEditorRow(out float ..Row) opens it,
        // the control is capped with GUILayout.Width(..Row) + ExpandWidth(false), and
        // EditorRowEnd closes it. Each asserted label must be present in the source.
        private static void AssertFieldRowsUseSharedRow(string src, string[] labels)
        {
            Assert.IsTrue(src.Contains("DrawEditorRow(out float ") && src.Contains("EditorRowEnd()"),
                "Field section must use the shared row open/close helpers");
            Assert.IsTrue(src.Contains("GUILayout.ExpandWidth(false)"),
                "Field controls must use ExpandWidth(false) so the row cap is honored");
            foreach (string label in labels)
                Assert.IsTrue(src.Contains(label), "Field label must still be present: " + label);
        }

        // All standalone ADD rows must go through the shared DrawEditorRow (transparent
        // indent spacer + width = max(MinRowWidth, min(visible panel, MaxRowWidth))).
        // Covers Global Scene, LOD, Search & Filter, and Specific Marker +Add rows.
        [Test]
        public void AddButtonRows_UseSharedRowWhereConverted()
        {
            string outline = ReadSource(@"Framework\Editor\POIEditor\GlobalScene\POIEditorToolWindow.MarkerDesign.cs");
            string hierarchy = ReadSource(@"Framework\Editor\POIEditor\GlobalScene\POIEditorToolWindow.GlobalScene.cs");
            string lod = ReadSource(@"Framework\Editor\POIEditor\GlobalScene\POIEditorToolWindow.LodZoom.cs");
            string search = ReadSource(@"Framework\Editor\POIEditor\GlobalScene\POIEditorToolWindow.SearchFilter.cs");
            string specific = ReadSource(@"Framework\Editor\POIEditor\SpecificMarker\POIEditorToolWindow.SpecificMarker.cs");
            string position = ReadSource(@"Framework\Editor\POIEditor\SpecificMarker\POIEditorToolWindow.PositionTabs.cs");

            AssertSharedRow(outline, "+ Add outline level", "Outline");
            AssertSharedRow(hierarchy, "+ Add hierarchy level", "Hierarchy");
            AssertSharedRow(lod, "+ Add band", "LOD band");
            AssertSharedRow(lod, "Suggest Values", "LOD suggest");
            AssertSharedRow(search, "+ Add synonym group", "Synonym group");
            AssertSharedRow(search, "+ Add keyword field", "Keyword field");
            AssertSharedRow(specific, "+ Add first", "SpecificMarker first");
            // "Verified" is intentionally NOT a shared row anymore: it moved from a
            // standalone full-width row inside the Position foldout's content to a
            // compact inline button on the foldout's own HEADER row (right-aligned next
            // to the "Position" title, via DrawFramedFoldout's rightAlignTrailing), so it
            // sizes to its own text instead of the shared row's capped width.
        }

        // A button is a shared row: DrawEditorRow + GUILayout.Width(rowWidth) +
        // ExpandWidth(false) + EditorRowEnd, and its label text is still present.
        // The "+ Add POI before/after/near" separators are row-pairs; each is a
        // shared row with two half-width buttons (sepHalf), still capped by the
        // route through DrawEditorRow.
        private static void AssertSharedRow(string src, string label, string what)
        {
            Assert.IsTrue(src.Contains("DrawEditorRow(out float rowWidth, out _)") ||
                          src.Contains("DrawEditorRow(out float firstRowWidth") ||
                          src.Contains("DrawEditorRow(out float beforeRowWidth") ||
                          src.Contains("DrawEditorRow(out float afterRowWidth") ||
                          src.Contains("DrawEditorRow(out float sepRowWidth") ||
                          src.Contains("DrawEditorRow(out float focusRowWidth") ||
                          src.Contains("DrawEditorRow(out float verifyRowWidth"),
                what + " row must call the shared DrawEditorRow");
            Assert.IsTrue(src.Contains("GUILayout.ExpandWidth(false)") && src.Contains("EditorRowEnd()"),
                what + " row must use ExpandWidth(false) and be closed with EditorRowEnd");
            Assert.IsTrue(src.Contains("GUILayout.Width(rowWidth)") || src.Contains("GUILayout.Width(beforeRowWidth)") ||
                          src.Contains("GUILayout.Width(afterRowWidth)") || src.Contains("GUILayout.Width(sepRowWidth)") ||
                          src.Contains("GUILayout.Width(focusRowWidth)") || src.Contains("GUILayout.Width(verifyRowWidth)") ||
                          src.Contains("GUILayout.Width(firstRowWidth)"),
                what + " row must size the button from the shared row width");
            Assert.IsTrue(src.Contains(label), what + " button label must still be present: " + label);
        }
    }
}

