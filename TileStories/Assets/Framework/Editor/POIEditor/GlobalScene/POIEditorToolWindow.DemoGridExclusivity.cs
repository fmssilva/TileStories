// POIEditorToolWindow.DemoGridExclusivity.cs
//
// Only one Play Mode demo view can actually be SEEN at a time. EffectsPreviewFocus and
// OutlinePreviewFocus (Runtime/DevTools) each build their own grid camera at "main camera depth +
// 100" over the full viewport, clearing to a solid colour every frame: with two grids on, the
// cameras tie and one silently paints over the other; with any grid on, the LOD demo field (which
// lives in the real scene, seen by the main camera) is hidden behind it. The losing view still
// spawns but is never visible, with no signal why. Enforced as mutual exclusion instead of a
// blocking confirmation dialog: "turning one on turns the others off" is the whole UX a picker would
// offer, and a modal EditorUtility.DisplayDialog can freeze a session driving this window
// programmatically (see the "editornotice dialog freeze" lesson).

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // The Play Mode demo views that take over what the developer sees
        internal enum DemoView
        {
            EffectsGrid,    // "Add effects demo grid" / "Add Hierarchy demo grid" / "Add Labels demo grid" (one switch)
            OutlineGrid,    // "Add outline demo grid"
            LodDemoField,   // "Add LOD demo field"
            DisplacementDemo, // "Add displacement demo" (its own stage, seen by the main camera like the LOD field)
            SearchDemo,     // "Add search demo" (its own stage, seen by the main camera)
        }

        // Call right after a demo view's own toggle, passing which view it is and whether this edit
        // just turned it ON (false -> true this frame). A no-op otherwise, so re-ticking an already-on
        // view, or turning one off, never touches the other switches.
        internal void MakeThisTheOnlyActiveDemoView(DemoView view, bool justTurnedOn)
        {
            if (!justTurnedOn || _config == null) return;

            if (view != DemoView.EffectsGrid && _config.effect_defaults?.preview != null)
                _config.effect_defaults.preview.enabled = false;
            if (view != DemoView.OutlineGrid && _config.outline_preview != null)
                _config.outline_preview.enabled = false;
            if (view != DemoView.LodDemoField && _config.demo_field != null)
                _config.demo_field.enabled = false;
            if (view != DemoView.DisplacementDemo && _config.displacement_demo != null)
                _config.displacement_demo.enabled = false;
            if (view != DemoView.SearchDemo && _config.search_demo != null)
                _config.search_demo.enabled = false;
        }
    }
}
