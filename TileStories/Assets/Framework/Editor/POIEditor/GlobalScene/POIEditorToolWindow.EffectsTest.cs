// POIEditorToolWindow.EffectsTest.cs
//
// Partial: the Effects domain's part of its "Test" sub-foldout (_5.1_Editor_Tab.md, "Domain Manual
// Tests"; the foldout itself is the shared DrawDomainTestSubSection). Editor-only -- none of this
// ships. There is NO Scene-Mode Preview: Edit Mode never runs Update or coroutines, so no effect can
// be seen animating in the Scene view; the Scene guide says so. Instead the Test foldout carries a
// Play-Mode preview switch (effect_defaults.preview) that makes WallSession spawn a labelled grid of
// real markers. Guide texts live in POIEditorToolWindow.EffectsHelp.cs.

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private readonly TestGuideState _effectsTest = new TestGuideState();

        // Global Scene -> Effects -> Test: the demo grid switch, then the three guides. Drawn even
        // while "Enable effects" is off, so the guides stay reachable (same rule as Badge / Outline).
        private void DrawEffectsTestSubSection()
        {
            DrawDomainTestSubSection(_effectsTest, EffectsSceneTestGuide, EffectsPlaymodeTestGuide,
                EffectsDeviceTestGuide, DrawEffectsPreviewSwitch);
        }

        // "Add effects demo grid" (built by EffectsPreviewSpawner from WallSession): off by default
        private void DrawEffectsPreviewSwitch()
        {
            var preview = _config.effect_defaults.preview;
            bool wasOn = preview.enabled;
            preview.enabled = DrawToggleField("Add effects demo grid", preview.enabled, EffectPreviewHelp, IndentLevel1);
            MakeThisTheOnlyActiveDemoView(DemoView.EffectsGrid, justTurnedOn: preview.enabled && !wasOn);
            if (preview.enabled)
            {
                EffectUsageSummary.PreviewBaseOptions(_config, out string[] ids, out string[] labels);
                preview.base_poi_id = DrawPopupField("Base marker", preview.base_poi_id, ids, labels,
                    EffectPreviewBaseHelp, IndentLevel1 + ConditionalAdvance);
            }
        }
    }
}
