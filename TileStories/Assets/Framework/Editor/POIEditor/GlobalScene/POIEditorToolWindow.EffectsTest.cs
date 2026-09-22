// POIEditorToolWindow.EffectsTest.cs
//
// Partial: the "Test" sub-foldout at the end of Global Scene > Effects (_5.1_Editor_Tab.md
// section 0, "Domain Manual Tests"). Editor-only -- none of this ships. Unlike Orientation there
// is NO Scene-Mode Preview toggle: Edit Mode never runs Update or coroutines, so no effect can be
// seen animating in the Scene view; the Scene guide says so and lists what can be checked there.
// Instead the foldout carries a Play-Mode preview switch (effect_defaults.preview) that makes
// WallSession spawn a labelled grid of real markers. The three guides reuse DrawTestGuideFoldout
// (Orientation.cs); their text lives in POIEditorToolWindow.EffectsHelp.cs.

using UnityEditor;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private bool _showEffectsTest = true;
        // The three test guides start collapsed: the developer opens only the one they need.
        private bool _showEffectsSceneTestGuide;
        private bool _showEffectsPlaymodeTestGuide;
        private bool _showEffectsDeviceTestGuide;

        // Global Scene -> Effects -> Test: how to verify effect and reveal choices at each tier.
        private void DrawEffectsTestSubSection()
        {
            _showEffectsTest = EditorGUILayout.Foldout(_showEffectsTest, "Test", true, EditorStyles.foldoutHeader);
            if (!_showEffectsTest) return;

            // Play-Mode preview grid (built by EffectsPreviewSpawner from WallSession): off by default.
            var preview = _config.effect_defaults.preview;
            preview.enabled = DrawToggleField("Focus on Effects Grid", preview.enabled, EffectPreviewHelp, IndentLevel1);
            if (preview.enabled)
            {
                EffectUsageSummary.PreviewBaseOptions(_config, out string[] ids, out string[] labels);
                preview.base_poi_id = DrawPopupField("Base marker", preview.base_poi_id, ids, labels,
                    EffectPreviewBaseHelp, IndentLevel1 + ConditionalAdvance);
            }

            EditorGUILayout.Space(4f);
            _showEffectsSceneTestGuide = DrawTestGuideFoldout(_showEffectsSceneTestGuide, "How to Scene Test", EffectsSceneTestGuide);
            _showEffectsPlaymodeTestGuide = DrawTestGuideFoldout(_showEffectsPlaymodeTestGuide, "How to Playmode Test", EffectsPlaymodeTestGuide);
            _showEffectsDeviceTestGuide = DrawTestGuideFoldout(_showEffectsDeviceTestGuide, "How to Device Test", EffectsDeviceTestGuide);
        }
    }
}
