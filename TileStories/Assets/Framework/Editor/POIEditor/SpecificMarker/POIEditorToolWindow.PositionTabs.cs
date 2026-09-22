// POIEditorToolWindow.PositionTabs.cs
//
// Partial: Single state-aware Position foldout inside the Specific Marker Position foldout.
// Editor-only.

using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        internal static string[] GetCoordinateLabels()
        {
            return new[] { "X", "Y", "Z" };
        }

        // Entry point called from DrawSpecificMarkerOptions for each POI.
        // Rows sit inside POI (level 1) + DrawFramedFoldout content (level 2); the
        // -1 scope collapses them to one small step past the sub-foldout title.
        internal void DrawPositionTabs(POIData poi)
        {
            if (poi == null) return;

            // IndentLevel0: rows sit at the section content's own indent (no collapsing scope)
            {

            bool hasPosition = poi.position != null;

            if (!hasPosition)
            {
                // No position yet: hint that moving the marker in Scene view sets position
                EditorGUILayout.HelpBox("Move the marker in the Scene view to set its position (positions are updated automatically).", MessageType.Info);
                return;
            }

            var position = poi.position;

            // Layout order is intentionally fixed: facing options first, coords second. The
            // Verified toggle now lives on this foldout's own header row (see
            // DrawPositionHeaderTrailing) instead of at the bottom of this content, so it
            // reads as "commit this position" next to the foldout title itself.
            // Shared row: label keeps a fixed width; slider takes rowWidth minus the
            // label + spacing, inside the capped row (level-2 indent measured at call time).
            // Named "Facing" (not "Rotation") to match the global Orientation section's own
            // Facing Options domain -- these X/Y/Z sliders (kept in sync with the Scene
            // view's rotate gizmo and the Inspector) are exactly the per-POI angle data that
            // domain's Wall Fixed and Y Rotation Only modes read at runtime
            // (_2.1_Marker_Orientation.md v4).
            // Three plain sliders, one per axis, no per-mode logic: the developer decides
            // which angles matter (the help button explains which mode reads which). All
            // three lock together when the POI is Verified.
            bool facingEditable = AreFacingSlidersEditable(poi);
            DrawConfigMutationScope(
                () =>
                {
                    float oldX = poi.editor_rotation_x_deg, oldY = poi.editor_rotation_deg, oldZ = poi.editor_rotation_z_deg;
                    EditorGUI.BeginChangeCheck();
                    poi.editor_rotation_x_deg = DrawFacingSliderRow("Facing X", poi.editor_rotation_x_deg, facingEditable, showHelp: true, out _);
                    poi.editor_rotation_deg = DrawFacingSliderRow("Facing Y", poi.editor_rotation_deg, facingEditable, showHelp: false, out _);
                    poi.editor_rotation_z_deg = DrawFacingSliderRow("Facing Z", poi.editor_rotation_z_deg, facingEditable, showHelp: false, out _);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ApplyPoiEditorRotation(poi);
                        WarnIfFacingEditInvisible(poi, oldX != poi.editor_rotation_x_deg, oldY != poi.editor_rotation_deg, oldZ != poi.editor_rotation_z_deg);
                    }
                },
                refreshRigOnChange: false);

            EditorGUILayout.Space(2f);

            // One compact read-only row: x [val]  y [val]  z [val]. The axis letters are
            // drawn OUTSIDE any disabled scope (see DrawCoordinateRow) -- this row is the
            // only one in the window whose label sat inside BeginDisabledGroup(true), and
            // it is the only label that never showed up.
            // Same level as the Facing rows above: DrawCoordinateRow opens its own shared row, whose
            // spacer already pays the section's indent (no extra IndentLevelScope).
            DrawCoordinateRow(position.x, position.y, position.z, out _, out _, out _);

            }
        }

        // One Facing row: bold axis label + 0..360 slider (+ the help button on the first row
        // only; other rows keep the same gap so all three sliders share one width). When not
        // editable the slider is drawn disabled AND the incoming value is returned untouched.
        // Rows are one shared shape so a render test can measure the real rects.
        internal static float DrawFacingSliderRow(string label, float value, bool editable, bool showHelp, out Rect sliderRect)
        {
            const float labelWidth = 92f;
            const float infoButtonWidth = 26f;
            const float infoButtonGap = 4f;

            float result = value;
            sliderRect = default;

            DrawEditorRow(out float facingRow, out _);
            {
                EditorGUILayout.LabelField(label, EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                float sliderW = Mathf.Max(120f, facingRow - 104f - infoButtonWidth - infoButtonGap);
                using (new EditorGUI.DisabledScope(!editable))
                {
                    float edited = EditorGUILayout.Slider(value, 0f, 360f,
                        GUILayout.Width(sliderW), GUILayout.ExpandWidth(false));
                    if (editable)
                        result = edited;
                }
                sliderRect = GUILayoutUtility.GetLastRect();

                GUILayout.Space(infoButtonGap);
                if (showHelp)
                    HelpInfoButton.Draw("Facing Options", EditRotationHelpBody);
            }
            EditorRowEnd();

            return result;
        }

        // Header-row trailing content for the Position foldout (drawn right-aligned,
        // after the "Position" title): the Verified/Unverified toggle -- the actual
        // "commit this position" action -- followed by the "?" help button (now just
        // HelpInfoButton.Draw's own default size, IconButtonSize, same as every other
        // icon button in this window -- no per-call-site override needed anymore).
        private void DrawPositionHeaderTrailing(POIData poi)
        {
            string buttonLabel = poi.position_verified ? "Verified" : "Unverified";
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = poi.position_verified ? new Color(0.2f, 0.7f, 0.2f) : new Color(0.8f, 0.3f, 0.3f);
            if (GUILayout.Button(buttonLabel, GUILayout.Height(20f)))
                TogglePoiVerification(poi);
            GUI.backgroundColor = originalColor;

            GUILayout.Space(4f);
            HelpInfoButton.Draw("Position", PositionSetupHelpBody);
        }

        // One compact read-only row: x [val]  y [val]  z [val].
        //
        // WHY THE LETTERS LIVE OUTSIDE ANY DISABLED SCOPE: for a long time this was the
        // only row in the whole window that drew its label inside
        // EditorGUI.BeginDisabledGroup(true), and it was the only label that never showed
        // up. Every other visible label here ("Category", "Has status")
        // is drawn un-disabled with the same EditorStyles.boldLabel and renders fine, so
        // the disabled scope was the one structural difference. The fix is therefore
        // structural too: only the VALUE FIELDS are read-only; the axis letters are
        // ordinary un-disabled labels.
        //
        // The first label/value rects are returned via out params so a real OnGUI render
        // test can assert the geometry (same pattern as DrawAddButtonRow).
        internal static void DrawCoordinateRow(float x, float y, float z,
            out Rect firstAxisLabelRect, out Rect firstAxisValueRect, out Rect lastAxisValueRect)
        {
            firstAxisLabelRect = default;
            firstAxisValueRect = default;
            lastAxisValueRect = default;

            DrawEditorRow(out float coordRow, out _);

            // axisLabelWidth is the MEASURED glyph width, not a guess: EditorStyles.
            // boldLabel.CalcSize(GUIContent) reports "X"/"Y"/"Z" at ~10.3-10.5px on this
            // font/DPI. An earlier pass fixed this at 40f based on a binary search done
            // under the OLD GUILayout-auto-layout rendering path (before the indent-
            // compounding bug below was fixed and before this method switched to manual
            // Rect splitting) -- 16f/24f failed to render THEN because of that separate
            // layout bug, not because the glyph itself needs 40px. Now that label and
            // value are two explicitly-split adjacent rects, the label only needs to be as
            // wide as its own glyph; anything wider is dead space between the letter and
            // the value field, which is exactly the "space between letter and coord field"
            // this row keeps getting reported for.
            float axisLabelWidth = Mathf.Ceil(EditorStyles.boldLabel.CalcSize(new GUIContent("X")).x) + 2f;
            const float minValueWidth = 40f;
            const float maxValueWidth = 90f;
            const float groupGap = 8f; // deliberate visual gap between "X val" / "Y val" / "Z val"
            // Shared-row convention (see RowLayout.cs / DrawEditorRow): coordRow already
            // has the row's right margin baked in via AddButtonRowRightMargin, same as
            // every other row in this window (e.g. the Load & Populate Rig / Clear Rig
            // pair: rigShare = (rigRowWidth - 4f) / 2f). This row's own fixed content is
            // 3 label glyphs + 2 inter-group gaps; the 3 value fields split whatever
            // remains -- no extra ad-hoc margin on top of coordRow, which would just make
            // this row's right margin inconsistent with every other row's.
            float valueWidth = Mathf.Clamp((coordRow - 3f * axisLabelWidth - 2f * groupGap) / 3f, minValueWidth, maxValueWidth);
            float pairWidth = axisLabelWidth + valueWidth;

            // GUIStyle.margin was NOT the source of the gap -- verified directly: zeroing
            // both styles' margin to (0,0,0,0) left the measured label-to-value gap
            // unchanged at exactly 3px. EditorGUILayout's automatic horizontal layout is
            // inserting that spacing through some other internal mechanism that resists
            // styling. Rather than keep fighting the auto-layout system, each letter+value
            // PAIR now reserves one combined rect via GUILayoutUtility.GetRect and splits
            // it manually in two: labelRect ends exactly where valueRect begins, drawn with
            // EditorGUI (immediate mode, explicit rects), which does not insert any
            // automatic inter-control spacing at all. This guarantees a true zero-pixel
            // gap by construction instead of hoping a style property is honored.
            int savedIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            try
            {
                string[] axes = GetCoordinateLabels();
                float[] axisValues = { x, y, z };
                float lineHeight = EditorGUIUtility.singleLineHeight;
                for (int i = 0; i < axes.Length; i++)
                {
                    if (i > 0)
                        GUILayout.Space(groupGap);

                    Rect pairRect = GUILayoutUtility.GetRect(pairWidth, lineHeight, GUILayout.Width(pairWidth), GUILayout.ExpandWidth(false));
                    Rect labelRect = new Rect(pairRect.x, pairRect.y, axisLabelWidth, pairRect.height);
                    Rect valueRect = new Rect(pairRect.x + axisLabelWidth, pairRect.y, valueWidth, pairRect.height);

                    EditorGUI.LabelField(labelRect, axes[i], EditorStyles.boldLabel);
                    using (new EditorGUI.DisabledScope(true))
                        EditorGUI.FloatField(valueRect, axisValues[i]);

                    if (i == 0)
                    {
                        firstAxisLabelRect = labelRect;
                        firstAxisValueRect = valueRect;
                    }
                    lastAxisValueRect = valueRect;
                }
            }
            finally
            {
                EditorGUI.indentLevel = savedIndent;
            }

            EditorRowEnd();
        }

        // Toggles a POI's verification status with confirmation gating when unlocking.
        internal void TogglePoiVerification(POIData poi)
        {
            if (poi == null) return;

            if (!poi.position_verified)
            {
                // Unverified -> Verified: capture the current in-memory rig position as the
                // last verified anchor before locking the scene object. This is the value we
                // must snap back to if the user drags a verified marker before saving.
                var rig = GetExistingRig();
                var child = rig != null ? rig.Find(poi.id) : null;
                Vector3 verifiedPosition = child != null ? child.localPosition : (poi.position != null ? new Vector3(poi.position.x, poi.position.y, poi.position.z) : Vector3.zero);

                // Same for facing: whatever the rig shows right now becomes the verified
                // facing (covers an Inspector edit the ~10 Hz poll has not caught yet).
                // Skipped during Edit-Mode preview, whose rotation is not authored.
                if (child != null && _config?.orientation_settings?.edit_mode_preview_enabled != true)
                    SyncPoiRotationFromScene(poi, child.localRotation.eulerAngles);

                _lastVerifiedPositions[poi.id] = verifiedPosition;
                if (poi.position == null)
                    poi.position = new PositionData();
                poi.position.x = verifiedPosition.x;
                poi.position.y = verifiedPosition.y;
                poi.position.z = verifiedPosition.z;

                DrawConfigMutationScope(() => { poi.position_verified = true; }, true);
            }
            else
            {
                _lastVerifiedPositions.Remove(poi.id);

                // Verified -> Unverified: confirm if the user really wants to unlock editing
                bool skipPrompt = EditorPrefs.GetBool(SkipUnverifyPromptPrefKey, false);
                if (!skipPrompt)
                {
                    int choice = EditorUtility.DisplayDialogComplex(
                        "Edit Verified Positions",
                        $"The position and facing for '{poi.name}' are already verified.\n\nAre you sure you want to edit this POI's position and facing again?",
                        "Yes, Unlock",                // 0 = left button
                        "Cancel",                     // 1 = middle button
                        "Yes, and Don't Ask Again"); // 2 = right button

                    if (choice == 0) // Yes, Unlock
                    {
                        DrawConfigMutationScope(() => { poi.position_verified = false; }, true);
                    }
                    else if (choice == 2) // Yes, and Don't Ask Again
                    {
                        EditorPrefs.SetBool(SkipUnverifyPromptPrefKey, true);
                        DrawConfigMutationScope(() => { poi.position_verified = false; }, true);
                    }
                    // choice == 1 or closed via X: Cancel (do nothing)
                }
                else
                {
                    DrawConfigMutationScope(() => { poi.position_verified = false; }, true);
                }
            }
        }

        // Apply the POI's config-driven edit-scene yaw to its rig child so the
        // Scene view reflects the slider live. Runtime billboard ignores this;
        // it only affects the editor preview. Called on slider drag and by
        // RefreshRigVisuals so undo/redo/refresh stay in sync.
        private void ApplyPoiEditorRotation(POIData poi)
        {
            if (poi == null || string.IsNullOrWhiteSpace(poi.id))
                return;
            Transform rig = GetExistingRig();
            if (rig == null)
                return;
            Transform child = rig.Find(poi.id);
            if (child == null)
                return;
            child.localRotation = PoiRotationResolver.ToEulerQuaternion(poi.editor_rotation_x_deg, poi.editor_rotation_deg, poi.editor_rotation_z_deg);
        }
    }
}
