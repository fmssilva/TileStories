using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIAuthoringToolWindow
    {
        private static readonly string[] OutlineModeOptions = { "gold", "same_hue", "free_colors" };
        private static readonly string[] OutlineModeLabels = { "Gold", "Same Hue", "Free Colors" };
        private static readonly string[] LineStyleOptions = { "solid", "dash_long", "dash_medium", "dash_short", "dotted" };
        private static readonly string[] LineStyleLabels = { "Continuous", "Big Dashed", "Medium Dashed", "Small Dashed", "Dots" };
        private static readonly string[] ShapeOptions = { "circle", "rounded_square", "hexagon", "diamond", "star", "none" };
        private static readonly string[] ShapeLabels = { "Circle", "Rounded Square", "Hexagon", "Diamond", "Star", "None" };

        // Sun effect options for hierarchy level table (maps to HierarchyLevelEntry.sun_effect).
        private static readonly string[] SunEffectOptions = { "none", "sun_contours", "sun_circles" };
        private static readonly string[] SunEffectLabels = { "None", "Contours", "Circles" };

        // Accent effect options for hierarchy level table (maps to HierarchyLevelEntry.accent_effect).
        private static readonly string[] AccentEffectOptions = { "none", "ring_pulse", "simple_sun", "beacon" };
        private static readonly string[] AccentEffectLabels = { "None", "Ring Pulse", "Simple Sun", "Beacon" };

        // LOD density-response mode options (maps to LodSettings.density_response_mode).
        private static readonly string[] DensityModeOptions = { "none", "select_hide", "cluster", "shrink_and_fade", "hybrid" };
        private static readonly string[] DensityModeLabels = { "None (off)", "Select & Hide", "Cluster", "Shrink & Fade", "Hybrid" };

        // Cluster icon modes (maps to LodSettings.cluster_icon_mode); only
        // applicable when density_response_mode is cluster or hybrid.
        private static readonly string[] ClusterIconOptions = { "pie_and_count", "dominant_category", "count_only" };
        private static readonly string[] ClusterIconLabels = { "Pie & Count", "Dominant Category", "Count Only" };

        // Cluster band-source selection (maps to LodSettings.cluster_band_source);
        // only applicable when density_response_mode is cluster or hybrid.
        private static readonly string[] ClusterBandSourceOptions = { "centroid", "nearest_member", "farthest_member" };
        private static readonly string[] ClusterBandSourceLabels = { "Centroid", "Nearest Member", "Farthest Member" };
        // Authoring-tool help text for the three new cluster params (3-7 of _2.4_Marker_LOD.md).
        // Read-only explanations surfaced via HelpInfoButton.Draw -> HelpInfoPopup (Block 2).
        private static readonly string LodClusterBandSourceHelp = "Which cluster member's effective distance decides the cluster's visible LOD band when the group is treated as one unit. Centroid (default; the group moves as one, smoothest), Nearest Member (the first member to cross a threshold band-promotes the whole group), or Farthest Member (the whole group must clear the far edge before promoting).";
        private static readonly string LodClusterBandHysteresisHelp = "Reuses the same hysteresis_margin_m as individual markers: a cluster stays in its current band until its active member's effective distance retreats past the margin before re-evaluating, so clustered groups chatter at band boundaries no differently than individual markers do.";
        private static readonly string LodClusterDissolveGraceHelp = "How many consecutive Evaluate() cycles a group must stay 'ungrouped' (below cluster_min_count neighbors) before its cluster view begins fading out. 0 disables the grace (groups pop in/out immediately). Default 3 smooths the membership flicker when visitors edge in and out of a density region.";

        // Authoring-tool help text for the LOD + Zoom foldouts (Block 2 of
        // _2.4_Marker_LOD.md, rows 5b / 12 / 13). Read-only explanations, not
        // persisted data -- HelpInfoButton.Draw opens a fixed HelpInfoPopup.
        private static readonly string LodEnabledHelp = "Master switch for the LOD/density/cluster/frustum pipeline. Disabling skips every step of LODController.Evaluate() -- markers render at full detail regardless of distance or screen-space density.";
        private static readonly string LodDensityResponseHelp = "How to thin markers in a dense screen region: None (off); Select & Hide (drop lowest-hierarchy-priority units); Cluster (merge into MarkerClusterView aggregates, section 6.1); Shrink & Fade (scale down + fade proportionally, never vanish); Hybrid (apply the shrink/fade ramp, then escalate to Cluster once neighbor count reaches cluster_min_count).";
        private static readonly string LodBandsHelp = "Distance tiers. A marker's effective distance (real distance divided by the zoom factor) is matched against the first row whose max_distance_m it falls under; rows must be sorted ascending. The last row's large value is a real sentinel, not a special case. max_visible_count = -1 means show every marker in that band.";
        private static readonly string LodDensityRadiusHelp = "Screen-space pixel radius treated as 'this marker is crowded'. Reuses MarkerOverlapResolver's 40f constant so this domain and the Displacement domain agree on what 'touching' means (section 6.2).";
        private static readonly string LodShrinkStartHelp = "Neighbor count (within density_radius_px) at which Shrink & Fade / Hybrid begins shrinking a marker. Must be < cluster_min_count -- validated at config-load time.";
        private static readonly string LodClusterMinHelp = "Neighbor count at which density response escalates to clustering (select_hide / cluster / hybrid). Deliberately higher than Displacement's implicit 2-marker nudging threshold so the two systems don't compete over the same small groups (section 6.2).";
        private static readonly string LodSafetyEscalationHelp = "If a region's neighbor count exceeds cluster_min_count x multiplier while density_response_mode is anything other than Hybrid, LODController overrides to Cluster for that region only this cycle -- a deterministic correctness safety net, not a competing 'smart' system (section 6.2).";
        private static readonly string LodHysteresisHelp = "Meters a distance-band transition must be crossed back before promoting again; the demotion fires immediately at the threshold. Stops flicker from a visitor's body sway at a band boundary (section 7).";
        private static readonly string LodTransitionsHelp = "Seconds visibility/size/alpha changes fade over instead of cutting. Reuses the same CanvasGroup mechanism MarkerRevealEffect introduced (section 7) -- do not build a second fade system.";
        private static readonly string LodEvalIntervalHelp = "Seconds between LODController.Evaluate() cycles. Density/LOD state doesn't need 60Hz; lower-end devices or very dense walls can use a coarser interval (section 4).";
        private static readonly string LodFrustumHelp = "Skips markers outside the camera's FOV (plus margin) before distance/density evaluation. Turn off for unusual wall geometry or to debug visibility (section 8).";
        private static readonly string LodFovMarginHelp = "Degrees added to the FOV used for the frustum-cull test only (not the render camera). A wider margin means markers just outside the edge are 'known' and already mid-transition by the time they scroll on screen.";
        private static readonly string ZoomEnabledHelp = "Master switch for global FOV-based AR camera zoom (section 9).";
        private static readonly string ZoomMinHelp = "Minimum zoom factor. SetZoom clamps to this. 1 = unzoomed (native device FOV).";
        private static readonly string ZoomMaxHelp = "Maximum zoom factor. Hard-clamped in ARZoomState.SetZoom -- an unclamped zoom drives effective distance toward zero and breaks every downstream size/LOD formula, plus pushes FOV to a degenerate near-zero. Practical authoring ceiling is ~5x; beyond that AR passthrough upscaling degrades the image and is rarely usable on real hardware.";
        private static readonly string ZoomTapStepHelp = "Zoom-factor step applied per double-tap. Distinct from Zoom Transition Speed, which is the animation duration of each step, not the size.";
        private static readonly string ZoomTapLevelsHelp = "Number of double-tap steps before cycling back to 1x. 2 means: step once, step twice, third tap returns to 1x.";
        private static readonly string ZoomTransitionHelp = "Seconds the FOV animates over for double-tap steps and on-screen button taps. Pinch (continuous) does not animate -- it follows the finger directly (section 9).";
        private static readonly string ZoomUiButtonsHelp = "Shows on-screen zoom in / zoom out / fit-to-1x buttons (UI Toolkit, screen-space). Independently toggleable so devs who prefer gestures can hide the chrome.";

                // Show-label options (explicit wording per Â§6 of 2.3 doc, clearer than bare checkbox).
        private static readonly string[] ShowLabelOptions = { "Show Label", "NOT show Label" };

        // --- Search & Filter authoring constants (Block 5) ---
        // Search mode dropdown (inert values flagged by ValidateSearchEnumFields).
        private static readonly string[] SearchModeOptions = { "dynamic", "explicit", "scoped", "faceted", "auto_complete" };
        private static readonly string[] SearchModeLabels = { "Dynamic", "Explicit", "Scoped (inert)", "Faceted (inert)", "Auto-Complete (inert)" };
        private static readonly string SearchModeHelp = "dynamic: debounced live filtering as the visitor types. explicit: results only on submit. scoped/faceted/auto_complete are recognized but currently inert (fall back to dynamic).";

        // Result view dropdown.
        private static readonly string[] ResultViewOptions = { "list", "minimap", "camera_highlight" };
        private static readonly string[] ResultViewLabels = { "List", "Minimap", "Camera Highlight" };
        private static readonly string ResultViewHelp = "The default result view shown when search returns results.";

        // Minimap dropdowns.
        private static readonly string[] MinimapVisibilityOptions = { "always", "toggle" };
        private static readonly string[] MinimapVisibilityLabels = { "Always", "Toggle" };
        private static readonly string[] MinimapIconOptions = { "dots_only", "category_colored_dots", "mini_icons" };
        private static readonly string[] MinimapIconLabels = { "Dots Only", "Category Colored", "Mini Icons" };
        private static readonly string MinimapHelp = "Show a 2D minimap overlay for POI navigation.";
        private static readonly string MinimapVisibilityHelp = "always: visible permanently. toggle: shows a button to expand/collapse.";
        private static readonly string MinimapIconHelp = "dots_only: plain colored dots. category_colored_dots: dots colored by category. mini_icons: scaled-down marker icons.";

        // Recent & suggested dropdown.
        private static readonly string[] SuggestedSourceOptions = { "category_distribution", "recent_first" };
        private static readonly string[] SuggestedSourceLabels = { "Category Distribution", "Recent First" };
        private static readonly string RecentCountHelp = "Number of recent search queries to remember locally (PlayerPrefs).";
        private static readonly string SuggestedHelp = "Show suggested search terms based on the wall's live POI distribution.";
        private static readonly string SuggestedSourceHelp = "category_distribution: top-N categories by POI count. recent_first: visitor's recent queries first, then category back-fill.";

        // Voice dropdowns.
        private static readonly string[] VoiceMatchModeOptions = { "all", "any" };
        private static readonly string[] VoiceMatchModeLabels = { "All", "Any" };
        private static readonly string[] VoiceIndicatorOptions = { "mic_text", "listen_bar" };
        private static readonly string[] VoiceIndicatorLabels = { "Mic Text", "Listen Bar" };
        private static readonly string VoiceEnabledHelp = "Enable voice search (requires microphone permission). Off by default due to iOS permission/privacy caveats.";
        private static readonly string VoiceMatchModeHelp = "all: results must match every token. any: results matching any token.";
        private static readonly string VoiceIndicatorHelp = "mic_text: mic button text flips to '...' while listening. listen_bar: also renders a dedicated progress bar.";

        // Selection & Zoom.
        private static readonly string SelectionHighlightHelp = "Dim non-selected markers when a marker is selected.";
        private static readonly string ZoomOnSelectHelp = "Auto-zoom when selecting a marker in dense regions.";
        private static readonly string ZoomOnSelectDensityHelp = "Minimum screen-space neighbours for zoom-on-select to fire.";
        private static readonly string ZoomOnSelectFactorHelp = "Zoom multiplier applied when zoom-on-select fires (e.g. 2.0 doubles the zoom). Clamped to the wall's zoom_min..zoom_max range.";
        private static readonly Color GlobalSectionColor = new Color(0.35f, 0.55f, 0.95f);

        // Tab button and container colors for the enhanced visual hierarchy.
        private static readonly Color GlobalSceneTabColor = new Color(0.15f, 0.50f, 0.95f); // vivid blue
        private static readonly Color SpecificMarkerTabColor = new Color(0.00f, 0.78f, 0.38f); // vivid green
        private static readonly Color TabTextColor = Color.white;
        private static readonly Color SceneConfigSectionColor = new Color(0.45f, 0.55f, 0.85f);
        private static readonly Color MarkerSectionColor = new Color(0.30f, 0.80f, 0.40f);
        private static readonly Color BadgeSectionColor = new Color(0.95f, 0.60f, 0.20f);
        private static readonly Color OutlineSectionColor = new Color(0.60f, 0.35f, 0.90f);
        private static readonly Color EffectsSectionColor = new Color(0.20f, 0.65f, 0.90f);
        private static readonly Color HierarchySectionColor = new Color(0.95f, 0.80f, 0.15f);
        private static readonly Color LodSectionColor = new Color(0.00f, 0.70f, 0.70f);
        private static readonly Color ZoomSectionColor = new Color(0.80f, 0.20f, 0.70f);
        private static readonly Color SearchFilterSectionColor = new Color(0.60f, 0.40f, 0.20f);
        // Specific Marker tab: per-POI header foldouts pick a stable color from
        // PoiHeaderPalette (deterministic FNV-1a hash of the POI id), so concrete
        // POI header titles look varied instead of sharing one crimson. The five
        // inner sub-section titles stay bold but render with the editor's default
        // (uncolored) text via FoldoutDefaultColor.
        private static readonly Color[] PoiHeaderPalette = new Color[]
        {
            new Color(0.95f, 0.25f, 0.25f), // red
            new Color(0.95f, 0.55f, 0.10f), // orange
            new Color(0.95f, 0.78f, 0.15f), // gold
            new Color(0.45f, 0.80f, 0.20f), // lime
            new Color(0.10f, 0.72f, 0.45f), // green
            new Color(0.00f, 0.70f, 0.70f), // teal
            new Color(0.15f, 0.60f, 0.95f), // sky blue
            new Color(0.20f, 0.45f, 0.90f), // blue
            new Color(0.45f, 0.45f, 0.95f), // indigo
            new Color(0.70f, 0.40f, 0.95f), // violet
            new Color(0.90f, 0.30f, 0.70f), // magenta
            new Color(0.95f, 0.20f, 0.55f), // hot pink
        };
        private static Color FoldoutDefaultColor => EditorStyles.foldout.normal.textColor;

        // Resolve a stable per-POI header color by hashing the POI key; the index
        // seed is used as the hash source when the key is null/empty. Deterministic
        // across repaints and editor sessions (no flicker), and reuses
        // CategoryPalette's canonical FNV-1a StableHash so there is one hash
        // implementation site shared with runtime category coloring.
        private static Color PoiHeaderColorFor(string poiKey, int fallbackSeed)
        {
            Color[] p = PoiHeaderPalette;
            if (p == null || p.Length == 0)
                return new Color(0.80f, 0.22f, 0.28f); // defensive fallback = old crimson
            string seed = string.IsNullOrEmpty(poiKey) ? fallbackSeed.ToString() : poiKey;
            int idx = (CategoryPalette.StableHash(seed) & 0x7FFFFFFF) % p.Length;
            return p[idx];
        }

        private static GUIContent _trashIcon;
        private static GUIContent TrashIcon => _trashIcon ?? (_trashIcon = EditorGUIUtility.IconContent("d_TreeEditor.Trash"));

        // Keyword Fields table (Global Scene > Search & Filter).
        private static readonly string SearchFieldKeyHelp = "Stable identifier for this search axis. Never change after authoring begins -- existing per-POI keywords reference it by key.";
        private static readonly string SearchFieldLabelHelp = "Human-readable name shown in each POI's keyword editor.";
        private static readonly string SearchFieldForcedHelp = "When enabled, a warning appears on any POI that leaves this field's keyword list empty. Non-blocking -- does not prevent saving.";
        private static readonly string SearchFieldDetailsHelp = "Usage note for the authoring team: what vocabulary is useful here, any naming conventions, examples.";
        private static readonly string SearchFieldsTableHelp =
            "Define custom search axes (e.g. 'architect', 'period', 'material'). Each row appears as an editable keyword list in every specific POI.\n" +
            "System axes (category / hierarchy / badge / outline) are handled automatically from their respective tables above.\n" +
            "Synonym suggestion (WordNet EN / AI) is a planned feature -- manual keyword entry is fully functional now.";

        // Per-POI keyword section labels.
        private static readonly string SearchKeywordsDerivedHelp = "These keywords are derived automatically from the taxonomy selections above and indexed at runtime. No action needed.";
        private static readonly string SearchKeywordsOthersHelp = "Freeform keywords that do not belong to any defined search axis (comma-separated). Indexed alongside all other keyword sources.";
    }
}
