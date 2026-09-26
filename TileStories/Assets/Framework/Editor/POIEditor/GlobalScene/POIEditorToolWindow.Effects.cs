// POIEditorToolWindow.Effects.cs
//
// Partial: Global Scene > Effects (_2.2.4_Markers_Effects.md). This page DEFINES how each
// effect looks; which effect a marker runs is chosen per hierarchy level in the Hierarchy Levels
// table, and every POI takes its level's effects. Layout: a master switch, then one foldout per
// effect (own enabled checkbox, "used by" line, own parameter block), then the Test sub-foldout.
// Turning the master switch or an effect off only HIDES its rows; nothing is reset or dropped from
// the config, so a slip of the mouse never loses settings. All rows go through the shared field
// helpers (LodZoom.cs) so indentation, width caps and the right margin follow _5.1 "Row Indentation
// & Spacing". Constants and help texts live in POIEditorToolWindow.EffectsHelp.cs.

using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // Effect foldouts start collapsed so the six effect types read at a glance.
        private bool _showEffectPulse;
        private bool _showEffectRippleRings;
        private bool _showEffectRippleDiscs;
        private bool _showEffectHaloRing;
        private bool _showEffectHaloDisc;
        private bool _showEffectBeacon;

        private const float EffectToggleWidth = 16f;

        // Global Scene -> Effects foldout. Edits _config.effect_defaults (EffectDefaults in
        // WallConfigData.cs); runtime behaviour lives in MarkerView.ApplyEffects.
        private void DrawGlobalEffectsSection()
        {
            EnsureEffectDefaultsExist();
            var d = _config.effect_defaults;

            d.effects_enabled = DrawToggleField("Enable effects", d.effects_enabled, EffectsEnabledHelp);
            if (!d.effects_enabled)
            {
                DrawEffectNote("Effects are off. Your effect settings are kept and still saved.", IndentLevel1);
                DrawEffectsTestSubSection();
                return;
            }

            EditorGUILayout.Space(4f);
            DrawPulseEffect(d.pulse);
            DrawRippleEffect(ref _showEffectRippleRings, d.ripple_rings, MarkerEffectFlags.RippleRings, RippleRingsHelp);
            DrawRippleEffect(ref _showEffectRippleDiscs, d.ripple_discs, MarkerEffectFlags.RippleDiscs, RippleDiscsHelp);
            DrawHaloRingEffect(d.halo_ring);
            DrawHaloDiscEffect(d.halo_disc);
            DrawBeaconEffect(d.beacon);

            DrawEffectsTestSubSection();
        }

        private void EnsureEffectDefaultsExist()
        {
            if (_config.effect_defaults == null)
            {
                _config.effect_defaults = new EffectDefaults();
                _hasUnsavedChanges = true;
            }
        }

        // ---------------- one foldout per effect ----------------

        private void DrawPulseEffect(EffectDefaults.PulseDefaults p)
        {
            _showEffectPulse = DrawEffectFoldoutHeader(_showEffectPulse, ref p.enabled, MarkerEffectFlags.Pulse, PulseHelp);
            if (!DrawEffectFoldoutIntro(_showEffectPulse, p.enabled, MarkerEffectFlags.Pulse)) return;

            p.amplitude = DrawSliderField("Amplitude", p.amplitude, 0f, 0.45f, EffectAmplitudeHelp, IndentLevel1);
            p.period = DrawScalarField("Period (s)", p.period, EffectsPeriodHelp, IndentLevel1);
        }

        // Rings and Discs share this drawer because their parameter blocks have the same shape;
        // each still edits its OWN block.
        private void DrawRippleEffect(ref bool isOpen, EffectDefaults.RippleDefaults r, MarkerEffectFlags effect, string help)
        {
            isOpen = DrawEffectFoldoutHeader(isOpen, ref r.enabled, effect, help);
            if (!DrawEffectFoldoutIntro(isOpen, r.enabled, effect)) return;

            r.period = DrawScalarField("Period (s)", r.period, EffectsPeriodHelp, IndentLevel1);
            r.stagger = DrawSliderField("Stagger", r.stagger, 0f, 0.25f, EffectStaggerHelp, IndentLevel1);
            r.inner_alpha = DrawSliderField("Inner alpha", r.inner_alpha, 0f, 1f, EffectInnerAlphaHelp, IndentLevel1);
            r.middle_alpha = DrawSliderField("Middle alpha", r.middle_alpha, 0f, 1f, EffectMiddleAlphaHelp, IndentLevel1);
            r.outer_alpha = DrawSliderField("Outer alpha", r.outer_alpha, 0f, 1f, EffectOuterAlphaHelp, IndentLevel1);
            string tint = r.tint_color_hex;
            DrawColorField("Tint Color", ref tint, "", IndentLevel1);
            r.tint_color_hex = tint;
        }

        private void DrawHaloRingEffect(EffectDefaults.HaloRingDefaults h)
        {
            _showEffectHaloRing = DrawEffectFoldoutHeader(_showEffectHaloRing, ref h.enabled, MarkerEffectFlags.HaloRing, HaloRingHelp);
            if (!DrawEffectFoldoutIntro(_showEffectHaloRing, h.enabled, MarkerEffectFlags.HaloRing)) return;

            h.size = DrawSliderField("Size (x symbol)", h.size, 0.5f, 3f, EffectSizeHelp, IndentLevel1);
            h.base_alpha = DrawSliderField("Base alpha", h.base_alpha, 0f, 1f, EffectBaseAlphaHelp, IndentLevel1);
            h.period = DrawScalarField("Period (s)", h.period, EffectsPeriodHelp, IndentLevel1);
            h.breathe_amplitude = DrawSliderField("Breathe amplitude", h.breathe_amplitude, 0f, 0.4f, EffectAmplitudeHelp, IndentLevel1);
            h.outer_scale = DrawSliderField("Outer scale", h.outer_scale, 0.72f, 0.98f, EffectOuterScaleHelp, IndentLevel1);
            h.inner_scale = DrawSliderField("Inner scale", h.inner_scale, 0.5f, 0.9f, EffectInnerScaleHelp, IndentLevel1);
            string tint = h.tint_color_hex;
            DrawColorField("Tint Color", ref tint, "", IndentLevel1);
            h.tint_color_hex = tint;
        }

        private void DrawHaloDiscEffect(EffectDefaults.HaloDiscDefaults h)
        {
            _showEffectHaloDisc = DrawEffectFoldoutHeader(_showEffectHaloDisc, ref h.enabled, MarkerEffectFlags.HaloDisc, HaloDiscHelp);
            if (!DrawEffectFoldoutIntro(_showEffectHaloDisc, h.enabled, MarkerEffectFlags.HaloDisc)) return;

            h.size = DrawSliderField("Size (x symbol)", h.size, 0.5f, 3f, EffectSizeHelp, IndentLevel1);
            h.base_alpha = DrawSliderField("Base alpha", h.base_alpha, 0f, 1f, EffectBaseAlphaHelp, IndentLevel1);
            h.period = DrawScalarField("Period (s)", h.period, EffectsPeriodHelp, IndentLevel1);
            h.breathe_amplitude = DrawSliderField("Breathe amplitude", h.breathe_amplitude, 0f, 0.4f, EffectAmplitudeHelp, IndentLevel1);
            h.radius_scale = DrawSliderField("Radius scale", h.radius_scale, 0.85f, 1f, EffectRadiusScaleHelp, IndentLevel1);
            string tint = h.tint_color_hex;
            DrawColorField("Tint Color", ref tint, "", IndentLevel1);
            h.tint_color_hex = tint;
        }

        private void DrawBeaconEffect(EffectDefaults.BeaconDefaults b)
        {
            _showEffectBeacon = DrawEffectFoldoutHeader(_showEffectBeacon, ref b.enabled, MarkerEffectFlags.Beacon, BeaconHelp);
            if (!DrawEffectFoldoutIntro(_showEffectBeacon, b.enabled, MarkerEffectFlags.Beacon)) return;

            b.size = DrawSliderField("Size (x symbol)", b.size, 0.5f, 3f, EffectSizeHelp, IndentLevel1);
            b.base_alpha = DrawSliderField("Base alpha", b.base_alpha, 0f, 1f, EffectBaseAlphaHelp, IndentLevel1);
            b.period = DrawScalarField("Period (s)", b.period, EffectsPeriodHelp, IndentLevel1);
            b.start_scale = DrawScalarField("Start scale", b.start_scale, EffectStartScaleHelp, IndentLevel1);
            b.end_scale = DrawScalarField("End scale", b.end_scale, EffectEndScaleHelp, IndentLevel1);
            b.outer_scale = DrawSliderField("Outer scale", b.outer_scale, 0.72f, 0.98f, EffectOuterScaleHelp, IndentLevel1);
            b.inner_scale = DrawSliderField("Inner scale", b.inner_scale, 0.5f, 0.9f, EffectInnerScaleHelp, IndentLevel1);
            string tint = b.tint_color_hex;
            DrawColorField("Tint Color", ref tint, "", IndentLevel1);
            b.tint_color_hex = tint;
        }

        // ---------------- shared pieces ----------------

        // One popup for a level-table effect column, offering only enabled effects. Takes a
        // GUIContent (pass GUIContent.none from a table row whose column is already named by the
        // table's own header, so no prefix-label width is reserved and the control fills `width`).
        private string DrawEffectOptionPopup(GUIContent label, string current, string[] options, string[] labels, float width)
        {
            EffectUsageSummary.FilterEnabledOptions(options, labels, _config.effect_defaults, current,
                out string[] shownOptions, out string[] shownLabels);
            int index = System.Array.IndexOf(shownOptions, current);
            if (index < 0) index = 0;
            index = EditorGUILayout.Popup(label, index, shownLabels, GUILayout.Width(width));
            return shownOptions[index];
        }

        // Rect overload -- used where two effect popups must sit pixel-adjacent (e.g. Ripple +
        // Halo with the between-them group gap removed), since a GUILayout.Width call always
        // carries the Popup style's own left/right margin and would reintroduce a small gap here.
        private string DrawEffectOptionPopup(Rect rect, string current, string[] options, string[] labels)
        {
            EffectUsageSummary.FilterEnabledOptions(options, labels, _config.effect_defaults, current,
                out string[] shownOptions, out string[] shownLabels);
            int index = System.Array.IndexOf(shownOptions, current);
            if (index < 0) index = 0;
            index = EditorGUI.Popup(rect, index, shownLabels);
            return shownOptions[index];
        }

        // One header row: enabled checkbox, foldout title, (i) button. Returns the foldout state.
        // One DrawEditorRow so the row follows the window's indent and width-cap rules;
        // indentLevel is zeroed around the controls because Toggle/Foldout would otherwise apply
        // the ambient indent a second time on top of the row's spacer (_5.1 Lesson 4).
        private bool DrawEffectFoldoutHeader(bool isOpen, ref bool enabled, MarkerEffectFlags effect, string helpBody)
        {
            string title = MarkerEffectNames.DisplayName(effect);
            DrawEditorRow(out float rowWidth, out _);
            int savedIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            enabled = EditorGUILayout.Toggle(enabled, GUILayout.Width(EffectToggleWidth), GUILayout.ExpandWidth(false));
            // Fixed-width rect for the title so the (i) button lands at the row's right end.
            float titleWidth = Mathf.Max(60f, rowWidth - EffectToggleWidth - IconButtonSize - 8f);
            Rect titleRect = GUILayoutUtility.GetRect(titleWidth, EditorGUIUtility.singleLineHeight,
                GUILayout.Width(titleWidth), GUILayout.ExpandWidth(false));
            isOpen = EditorGUI.Foldout(titleRect, isOpen, title, true, EditorStyles.foldout);
            HelpInfoButton.Draw(title, helpBody + "\n\n" + EffectsFlowNote);
            EditorGUI.indentLevel = savedIndent;
            EditorRowEnd();
            return isOpen;
        }

        // Content shown right under an open foldout header: the "used by" line, or the disabled note
        // instead of the parameter rows. Returns true when the parameter rows should be drawn.
        private bool DrawEffectFoldoutIntro(bool isOpen, bool enabled, MarkerEffectFlags effect)
        {
            if (!isOpen) return false;

            DrawEffectNote(EffectUsageSummary.DescribeUsage(_config, effect), IndentLevel1);
            if (!enabled)
            {
                DrawEffectNote(DisabledEffectNote, IndentLevel1);
                return false;
            }
            return true;
        }

        // A read-only, word-wrapped grey note row (capped width like every other row).
        private static void DrawEffectNote(string text, float extraIndentPixels)
        {
            DrawEditorRow(out float rowWidth, out _, extraIndentPixels);
            int savedIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            var style = EditorStyles.wordWrappedMiniLabel;
            float height = style.CalcHeight(new GUIContent(text), rowWidth);
            GUILayout.Label(text, style, GUILayout.Width(rowWidth), GUILayout.Height(height), GUILayout.ExpandWidth(false));
            EditorGUI.indentLevel = savedIndent;
            EditorRowEnd();
        }
    }
}
