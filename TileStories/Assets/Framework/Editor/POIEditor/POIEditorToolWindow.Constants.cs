using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // Taxonomy table layout (marker / badge / outline tables).
        // One rhythm for the whole window: a single gap between every logical
        // column group, and a single gap between sibling controls inside one
        // group. Tune both here instead of hunting per-table magic numbers.
        private const float TableGapBetweenGroups = 6f;
        private const float TableGapWithinGroup = 4f;

        // Row layout for non-table rows (buttons, stand-alone controls).
        // SectionRowIndent: the left pad that makes a bare GUILayout row sit at the
        // same x as EditorGUILayout content inside a foldout (GUILayout ignores
        // EditorGUI.indentLevel; EditorGUILayout does not -- this constant bridges
        // that gap so buttons align with the section's table rows).
        // MaxRowWidth: cap for non-table rows (sliders, plain fields) so they never
        // stretch across the whole window just because the taxonomy tables are wide.
        // Narrow window -> narrower row; wide window -> wider row, capped here.
        private const float SectionRowIndent = 15f;
        private const float MaxRowWidth = 480f;

        // Floor for non-table rows: even on a very narrow panel a button/row must
        // stay readable, so the width clamp is max(MinRowWidth, min(panel, MaxRowWidth)).
        private const float MinRowWidth = 180f;

        // Right spacing between a row's controls and the visible panel edge, when the
        // panel is narrow. This must clear the ScrollView's VERTICAL SCROLLBAR (~15px in
        // the Pro skin): with the old 6f the row's right edge landed underneath the
        // scrollbar whenever the list was long enough to scroll, so the last few pixels
        // of every row were hidden. Used by the row width rule and must be stable across
        // the Layout and Repaint IMGUI passes.
        private const float AddButtonRowRightMargin = 20f;

        // Header pad for the Symbol/Style column: only the single within-group
        // gap that also separates the ObjectField from the preview in the body
        // rows. That puts the info button DIRECTLY OVER the preview thumbnail
        // (the preview is the curated picker now -- there is no separate select
        // button to make room for).
        private const float SymbolColumnPad = TableGapWithinGroup;

        // Color group widths (marker / badge / outline tables).
        // The first cell is the REAL working EditorGUILayout.ColorField, forced
        // wide (ColorPickerWidth) so it is easy to click -- no flat draw-only
        // swatch in front of it, which read as a dead cell. The hex field is the
        // "color name". The pair sits flush (GUILayout's default inter-control
        // spacing is already there), so the group width is just picker + hex.
        private const float ColorPickerWidth = 60f;
        private const float ColorHexFieldWidth = 90f;
        private const float ColorGroupWidth = ColorPickerWidth + ColorHexFieldWidth;

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
        // Editor help text for the three new cluster params (3-7 of _2.4_Marker_LOD.md).
        // Read-only explanations surfaced via HelpInfoButton.Draw -> HelpInfoPopup (Block 2).
        private static readonly string LodClusterBandSourceHelp = "Which cluster member's effective distance decides the cluster's visible LOD band when the group is treated as one unit. Centroid (default; the group moves as one, smoothest), Nearest Member (the first member to cross a threshold band-promotes the whole group), or Farthest Member (the whole group must clear the far edge before promoting).";
        private static readonly string LodClusterBandHysteresisHelp = "Reuses the same hysteresis_margin_m as individual markers: a cluster stays in its current band until its active member's effective distance retreats past the margin before re-evaluating, so clustered groups chatter at band boundaries no differently than individual markers do.";
        private static readonly string LodClusterDissolveGraceHelp = "How many consecutive Evaluate() cycles a group must stay 'ungrouped' (below cluster_min_count neighbors) before its cluster view begins fading out. 0 disables the grace (groups pop in/out immediately). Default 3 smooths the membership flicker when visitors edge in and out of a density region.";

        // Editor help text for the LOD + Zoom foldouts (Block 2 of
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
        private static readonly string ZoomMaxHelp = "Maximum zoom factor. Hard-clamped in ARZoomState.SetZoom -- an unclamped zoom drives effective distance toward zero and breaks every downstream size/LOD formula, plus pushes FOV to a degenerate near-zero. Practical editor ceiling is ~5x; beyond that AR passthrough upscaling degrades the image and is rarely usable on real hardware.";
        private static readonly string ZoomTapStepHelp = "Zoom-factor step applied per double-tap. Distinct from Zoom Transition Speed, which is the animation duration of each step, not the size.";
        private static readonly string ZoomTapLevelsHelp = "Number of double-tap steps before cycling back to 1x. 2 means: step once, step twice, third tap returns to 1x.";
        private static readonly string ZoomTransitionHelp = "Seconds the FOV animates over for double-tap steps and on-screen button taps. Pinch (continuous) does not animate -- it follows the finger directly (section 9).";
        private static readonly string ZoomUiButtonsHelp = "Shows on-screen zoom in / zoom out / fit-to-1x buttons (UI Toolkit, screen-space). Independently toggleable so devs who prefer gestures can hide the chrome.";
   private static readonly string ZoomDoubleTapWindowHelp = "Seconds between the two taps for a double-tap gesture. Second tap must arrive within this window after the first.";
   private static readonly string ZoomDoubleTapMoveToleranceHelp = "Maximum pixel movement allowed between the two taps for a double-tap gesture. Exceeding this distance counts as a drag, not a double-tap.";

                // Show-label options (explicit wording per Â§6 of 2.3 doc, clearer than bare checkbox).
        private static readonly string[] ShowLabelOptions = { "Show Label", "NOT show Label" };

        // --- Orientation editor constants (_2.1_Marker_Orientation.md Block 6) ---
        private static readonly string[] MarkerOrientationModeOptions = { "screen_aligned", "world_up", "yaw_only", "wall_fixed", "none" };
        private static readonly string[] MarkerOrientationModeLabels = { "Screen Aligned", "World Up", "Yaw Only", "Wall Fixed", "None" };
        private static readonly string[] FacingBasisOptions = { "view_plane", "camera_position" };
        private static readonly string[] FacingBasisLabels = { "View Plane", "Camera Position" };
        private static readonly string[] UpReferenceOptions = { "world_gravity", "spawn_root", "custom" };
        private static readonly string[] UpReferenceLabels = { "World Gravity", "Spawn Root", "Custom" };
        private static readonly string[] RollSnapModeOptions = { "none", "quarter_turns", "screen_orientation" };
        private static readonly string[] RollSnapModeLabels = { "None", "Quarter Turns", "Screen Orientation" };
        private static readonly string[] ChildOrientationModeOptions = { "inherit", "screen_up", "world_up" };
        private static readonly string[] ChildOrientationModeLabels = { "Inherit", "Screen Up", "World Up" };
        private static readonly string[] BadgeCornerModeOptions = { "inherit", "screen_fixed" };
        private static readonly string[] BadgeCornerModeLabels = { "Inherit", "Screen Fixed" };
        private static readonly string[] ClusterOrientationModeOptions = { "inherit", "screen_aligned", "world_up", "yaw_only", "none" };
        private static readonly string[] ClusterOrientationModeLabels = { "Inherit", "Screen Aligned", "World Up", "Yaw Only", "None" };
        private static readonly string[] OrientationUpdateModeOptions = { "every_frame", "interval", "on_camera_delta" };
        private static readonly string[] OrientationUpdateModeLabels = { "Every Frame", "Interval", "On Camera Delta" };
        // Hierarchy Levels table's per-level override column (section 4.3): "" = inherit the wall setting.
        private static readonly string[] OrientationOverrideOptions = { "", "screen_aligned", "world_up", "yaw_only", "wall_fixed", "none" };
        private static readonly string[] OrientationOverrideLabels = { "Inherit", "Screen Aligned", "World Up", "Yaw Only", "Wall Fixed", "None" };

        private static readonly string MarkerOrientationModeHelp = "How the marker root is rotated. Screen Aligned (default): always parallel to the camera's near plane, never foreshortens, maximum legibility. World Up: faces the viewer but stays upright in the real world, so turning the phone does not spin it. Yaw Only: classic cylindrical billboard, rotates about the up axis only - foreshortens when looking steeply up/down. Wall Fixed: painted flat onto the wall surface using the POI's authored rotation. None: no rotation applied beyond the parent's.";
        private static readonly string FacingBasisHelp = "How the marker's forward direction is chosen (Screen Aligned / World Up only). View Plane (default): every marker parallel to the camera's near plane, no perspective skew anywhere on screen. Camera Position: each marker's forward points away from the camera individually, which reads as more physical for large markers but introduces slight skew off-centre.";
        private static readonly string UpReferenceHelp = "The 'up' direction used by World Up and Yaw Only. World Gravity (default): real-world up, no gyroscope needed since AR world space is already gravity-aligned. Spawn Root: the wall's own placement anchor up - use when the map frame is not gravity-aligned. Custom: an authored vector below.";
        private static readonly string CustomUpHelp = "The custom up-reference vector, used only when Up Reference is set to Custom.";
        private static readonly string RollSnapModeHelp = "Conditions the marker's measured roll before it is applied. None (default): continuous, unsnapped roll. Quarter Turns: snaps the measured roll to the nearest 90 degrees so a hand-held tilt never leaves a permanently skewed label. Screen Orientation: reads the OS's committed screen orientation instead of measured roll - steadier once the device has settled, but does nothing while the app is orientation-locked.";
        private static readonly string RollSnapHysteresisHelp = "Degrees of deadband around a roll-snap boundary, so a tilt sitting right at the boundary cannot flicker between the two nearest snap values every frame.";
        private static readonly string ClampPitchHelp = "Stops a marker tipping fully edge-on when a visitor looks steeply up or down a tall wall.";
        private static readonly string MaxPitchHelp = "The pitch angle, in degrees, beyond which Clamp Pitch holds the marker instead of following the camera further.";
        private static readonly string RotationSmoothingHelp = "Seconds of exponential damping applied against AR pose jitter. 0 (default) applies the resolved rotation instantly, with no smoothing.";
        private static readonly string LabelOrientationHelp = "Independent orientation for the Label, applied as a pure Z counter-rotation on top of the root. Inherit (default): the label rotates rigidly with the root. Screen Up / World Up: the label's own up stays aligned to that basis regardless of the root's orientation - this is how 'marker faces the camera, label always reads upright' is achieved without moving the root.";
        private static readonly string BadgeOrientationHelp = "Same as Label Orientation, but for the Badge.";
        private static readonly string BadgeCornerModeHelp = "Only meaningful when Badge Orientation is not Inherit. Screen Fixed re-rotates the badge's fixed corner offset by the root's own roll, so the badge holds its screen corner as the device rotates instead of drifting to a different corner.";
        private static readonly string ClusterOrientationModeHelp = "Orientation mode used by cluster aggregate markers. Inherit (default): same mode as the wall's Marker Orientation above. Any other value overrides it for clusters only - useful when individual markers and their aggregates should behave differently.";
        private static readonly string OrientationUpdateModeHelp = "Cost control for how often orientation is re-resolved. Every Frame (default): always up to date, highest cost. Interval: re-resolves at most every Update Interval seconds. On Camera Delta: re-resolves only once the camera has rotated past Camera Delta degrees since the last resolve.";
        private static readonly string OrientationUpdateIntervalHelp = "Seconds between orientation re-resolves, used only when Update Mode is Interval.";
        private static readonly string OrientationCameraDeltaHelp = "Degrees the camera must rotate before orientation re-resolves, used only when Update Mode is On Camera Delta.";
        private static readonly string EditModePreviewHelp = "Shows what World Up / Yaw Only will look like directly in the Scene view, without entering Play Mode. Smoothing and Update Mode are ignored in preview since there is no per-frame tick to smooth over.";

        // --- Search & Filter editor constants (Block 5) ---
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
        private static readonly string MasterToggleHelp = "Master switch for the whole Select / Filter / Search domain (search overlay, facet filters, minimap, results list, marker selection + zoom-on-select). Off = none of it activates for this wall, mirroring the LOD and Displacement master toggles.";

        // Filter mismatch behaviour (maps to WallConfigData.filter_mismatch_behaviour).
        private static readonly string[] FilterMismatchOptions = { "hide", "dim" };
        private static readonly string[] FilterMismatchLabels = { "Hide", "Dim" };
        private static readonly string FilterMismatchHelp = "What happens to markers outside the active filter/search result set. Hide: fully faded out (spec section 7 default). Dim: kept faintly visible so dense walls keep their positional context.";
        private static readonly string MinimapVisibilityHelp = "always: visible permanently. toggle: shows a button to expand/collapse.";
        private static readonly string MinimapIconHelp = "dots_only: plain colored dots. category_colored_dots: dots colored by category. mini_icons: scaled-down marker icons.";
        private static readonly string MinimapDotSizeHelp = "Visual dot diameter in px. Keep small so dense walls stay readable -- tap comfort is the separate 'Dot tap target' value below.";
        private static readonly string MinimapTapTargetHelp = "Invisible hit-zone diameter per dot (the actual tap receiver). Defaults to the 44x44 WCAG floor; overlapping zones at high density resolve by nearest dot center.";

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
        private static readonly Color OrientationSectionColor = new Color(0.50f, 0.50f, 0.95f);
        private static readonly Color BadgeSectionColor = new Color(0.95f, 0.60f, 0.20f);
        private static readonly Color OutlineSectionColor = new Color(0.60f, 0.35f, 0.90f);
        private static readonly Color EffectsSectionColor = new Color(0.20f, 0.65f, 0.90f);
        private static readonly Color HierarchySectionColor = new Color(0.95f, 0.80f, 0.15f);
        private static readonly Color LodSectionColor = new Color(0.00f, 0.70f, 0.70f);
                private static readonly Color ZoomSectionColor = new Color(0.80f, 0.20f, 0.70f);

        // ---- Displacement section (Block 8 of _2.5_Marker_Displacement.md) ----
        // Section color + option arrays + help text for the Global Scene ->
        // Displacement Settings foldout. The draw method lives in
        // POIEditorToolWindow.Displacement.cs; this file only owns the data,
        // mirroring how the LOD/Zoom options/colors live here while their draw
        // method lives in LodZoom.cs.
        private static readonly Color DisplacementSectionColor = new Color(0.85f, 0.30f, 0.95f);
        private static readonly string[] DisplaceTargetOptions = { "label_only", "marker", "both" };
        private static readonly string[] DisplaceTargetLabels = { "Label Only", "Marker", "Both" };
        private static readonly string[] DisplacementAlgorithmOptions = { "fixed_axis", "candidate_position", "force_directed" };
        private static readonly string[] DisplacementAlgorithmLabels = { "Fixed Axis", "Candidate Position", "Force Directed" };
        private static readonly string[] LeaderLineStyleOptions = { "straight", "dashed", "elbow" };
        private static readonly string[] LeaderLineStyleLabels = { "Straight", "Dashed", "Elbow" };
        private static readonly string[] DisplacementTiebreakOptions = { "symmetric", "lower_priority_only" };
        private static readonly string[] DisplacementTiebreakLabels = { "Symmetric", "Lower Priority Only" };

        private static readonly string DisplacementEnabledHelp = "Master switch for displacement (LODController.Evaluate step 8). When off, every visible marker renders at its base position and no offsets are computed; stability snapshots are left untouched so re-enabling settles immediately without popping.";
        private static readonly string DisplacementOverlapThresholdHelp = "Screen-space pixel radius treated as 'these markers touch' when deciding a group needs to separate. Shared with LODController's density radius so the two systems agree on what 'touching' means.";
        private static readonly string DisplacementTargetHelp = "Which part moves to resolve overlap: Label Only (shifts only the readable text -- the safest default; never moves the 3D anchor and reads correctly at a shallow viewing angle), Marker (shifts the 3D anchor itself), or Both.";
        private static readonly string DisplacementAlgorithmHelp = "fixed_axis: deterministic symmetric fan-out (cheapest). candidate_position: nearest open slot per ring (Christensen et al. 1995, good for dense walls). force_directed: organic iterative repulsion with priority anchoring -- the default; cost scales with Relaxation Steps.";
        private static readonly string ForceDirectedIterationsHelp = "Relaxation steps per Evaluate() cycle for the force_directed algorithm only. More steps separate further but cost more.";
        private static readonly string DisplacementMaxHelp = "Cap on how far (screen px) a marker/label may travel to escape overlap; beyond it the displaced element hides instead of pushing further, keeping dense groups legible.";
        private static readonly string LeaderLinesEnabledHelp = "Draw a line from the displaced marker back to its true baseline position so the visitor can still read which POI the shifted label belongs to.";
        private static readonly string LeaderLineStyleHelp = "straight (single segment), dashed (clears over other markers), or elbow (two-segment poly-line to dodge overlapping content).";
        private static readonly string LeaderLineMinDistanceHelp = "Shortest screen-px displacement before a leader line is drawn; tiny nudges get no line to avoid clutter.";
        private static readonly string LeaderLineWidthHelp = "World-space (metre) thickness -- not pixels -- so it scales with viewing distance. Tuned per wall.";
        private static readonly string LeaderLineOpacityHelp = "0-1 alpha multiplier over the marker's resolved category color. 1.0 = full color.";
        private static readonly string DisplacementTiebreakHelp = "Tiebreak for two equal-priority markers that overlap: Symmetric (default) shifts both. lower_priority_only shifts only the lower-priority one -- currently deferred (falls back to Symmetric), but still schema-valid to author now.";
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

        // Info glyph reused for read-only explanation buttons. Unity native
        // icon first, bundled PNG fallback -- mirrors the pencil pattern so it
        // survives domain reloads and never depends on editor skin names.
        public const string InfoIconAssetPath = "Assets/Framework/Editor/POIEditor/Shared/Icons/info-icon.png";

        private static GUIContent _infoIcon;
        public static GUIContent InfoIcon
        {
            get
            {
                if (_infoIcon != null)
                    return _infoIcon;

                var native = EditorGUIUtility.IconContent("_Help");
                _infoIcon = native != null && native.image != null
                    ? native
                    : new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(InfoIconAssetPath));
                return _infoIcon;
            }
        }

        // Select glyph for the curated "Choose existing" picker button.
        // Unity native assign icon first, bundled PNG fallback -- same caching
        // pattern as TrashIcon / InfoIcon so it survives domain reloads.
        // NOTE: the native probe uses FindTexture, NOT IconContent: on Unity 6
        // (6000.x) "ProjectAssign" is not a registered skin name, and
        // IconContent(name) LOGS "Unable to load the icon" for unknown names on
        // every call before falling back. FindTexture returns null silently.
        public const string SelectIconAssetPath = "Assets/Framework/Editor/POIEditor/Shared/Icons/select.png";

        private static GUIContent _selectIcon;
        public static GUIContent SelectIcon
        {
            get
            {
                if (_selectIcon != null)
                    return _selectIcon;

                var nativeTexture = EditorGUIUtility.FindTexture("ProjectAssign");
                _selectIcon = nativeTexture != null
                    ? new GUIContent(nativeTexture)
                    : new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(SelectIconAssetPath));
                return _selectIcon;
            }
        }

        // Details glyph for the per-row edit-note buttons. Uses the bundled PNG
        // (explicitly requested asset) with a text fallback if it ever goes
        // missing -- unlike Info/Select there is no reliable native "three dots".
        public const string DetailsIconAssetPath = "Assets/Framework/Editor/POIEditor/Shared/Icons/details.png";

        private static GUIContent _detailsIcon;
        public static GUIContent DetailsIcon
        {
            get
            {
                if (_detailsIcon != null)
                    return _detailsIcon;

                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(DetailsIconAssetPath);
                _detailsIcon = tex != null ? new GUIContent(tex, "Edit details") : new GUIContent("...");
                return _detailsIcon;
            }
        }

        // Pencil glyph for the per-POI header rename button. Loaded by asset
        // path (not EditorGUIUtility.Load) so it survives domain reloads and
        // never depends on the stray top-level Assets/Editor copy.
        internal const string EditIconAssetPath = "Assets/Framework/Editor/POIEditor/SpecificMarker/Icons/edit-icon.png";

        // Crosshair glyph for the per-POI header focus button (selects + frames the marker).
        internal const string FocusIconAssetPath = "Assets/Framework/Editor/POIEditor/SpecificMarker/Icons/focus-icon.png";

        // Green plus glyph for the per-POI header ADD-NEAR button (adds a new POI below
        // this one, using it as the template).
        internal const string AddIconAssetPath = "Assets/Framework/Editor/POIEditor/SpecificMarker/Icons/add-icon.png";

        private static GUIContent _addIcon;
        internal static GUIContent AddIcon
        {
            get
            {
                if (_addIcon != null)
                    return _addIcon;

                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AddIconAssetPath);
                _addIcon = new GUIContent(tex, "Add a new POI below this one, copied from it");
                return _addIcon;
            }
        }

        // Red trash glyph for the per-POI header DELETE button. Bundled PNG (the dev's own
        // asset) so the danger affordance is the ICON itself, which keeps the button at the
        // same height as its sibling reorder/focus buttons -- a dark-red button FILL read as
        // a bigger control that overhung the header row. Distinct from TrashIcon (Unity's
        // native glyph) which the taxonomy tables still use.
        internal const string DeleteIconAssetPath = "Assets/Framework/Editor/POIEditor/SpecificMarker/Icons/delete-icon.png";

        private static GUIContent _deleteIcon;
        internal static GUIContent DeleteIcon
        {
            get
            {
                if (_deleteIcon != null)
                    return _deleteIcon;

                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(DeleteIconAssetPath);
                _deleteIcon = new GUIContent(tex, "Delete POI");
                return _deleteIcon;
            }
        }

        // POI header row icon-cluster help ((i) button between "+" and delete).
        internal static readonly string PoiHeaderIconsHelpBody =
            "What each icon on this row does, left to right after the name:\n\n" +
            "Crosshair (focus): selects this POI's marker and moves the Scene view camera " +
            "to frame it, without expanding this POI's foldout.\n\n" +
            "Pencil: renames this POI (edits its display name only).\n\n" +
            "Up / Down arrows: reorders this POI earlier or later in the list. This is the " +
            "config's own save order, not a spatial position.\n\n" +
            "+ (add near): adds a new POI directly below this one, copied from it (category, " +
            "hierarchy level, rotation, etc). Use the crosshair on the new row, then Unity's " +
            "normal Move tool, to place it where you actually want it.\n\n" +
            "Trash (delete): permanently removes this POI from the config and from the Scene " +
            "rig, after a confirmation prompt.";

        // Position & rotation setup help (Position foldout title row, (i) button).
        internal static readonly string PositionSetupHelpBody =
            "How to place this marker, step by step:\n\n" +
            "1. Select it. A newly added POI is auto-focused already; otherwise click the " +
            "crosshair icon on this POI's title row to select it and frame it in the Scene view.\n\n" +
            "2. Move it. Use Unity's normal Move tool (same as any other GameObject) to drag the " +
            "marker in the Scene view to where it should sit on the wall.\n\n" +
            "3. Lock it in. Once the position looks right, click 'Verified' (next to this help " +
            "button) to lock it -- turns green. A verified position is protected: if the marker " +
            "gets bumped in the Scene view afterward, it snaps back automatically until you " +
            "unlock it again by clicking Verified a second time.\n\n" +
            "Note on rotation: the Rotation slider below (and Unity's Rotate tool) is NOT covered " +
            "by Verified and never needs to be locked. It only sets an editor-preview angle to " +
            "help you look at the marker while placing it -- at runtime every marker always turns " +
            "to face the camera (billboard), so whatever rotation is saved here never changes what " +
            "a visitor actually sees.";

        // Edit Rotation row help ((i) button next to the slider itself).
        internal static readonly string EditRotationHelpBody =
            "This angle is an editor-only preview aid -- at runtime every marker always " +
            "faces the camera (billboard), so nothing saved here ever changes what a visitor sees.\n\n" +
            "This slider controls yaw (rotation around Y) and stays in sync in both directions with " +
            "Unity's Rotate tool: dragging the slider turns the marker in the Scene view, and rotating " +
            "the marker with the Rotate tool updates this slider live.\n\n" +
            "There is no slider for pitch/roll (X/Z) -- use Unity's Rotate tool directly for those. " +
            "All three axes are captured live and saved to config JSON, then re-applied automatically " +
            "next time the rig is populated.\n\n" +
            "Rotation is always free to edit, even after this POI's position is Verified -- the " +
            "Verified lock only ever applies to position.";

        // Keyword Fields table (Global Scene > Search & Filter).
        private static readonly string SearchFieldKeyHelp = "Stable identifier for this search axis. Never change after editing begins -- existing per-POI keywords reference it by key.";
        private static readonly string SearchFieldLabelHelp = "Human-readable name shown in each POI's keyword editor.";
        private static readonly string SearchFieldForcedHelp = "When enabled, a warning appears on any POI that leaves this field's keyword list empty. Non-blocking -- does not prevent saving.";
        private static readonly string SearchFieldDetailsHelp = "Usage note for the editor team: what vocabulary is useful here, any naming conventions, examples.";
        private static readonly string SearchFieldsTableHelp =
            "Define custom search axes (e.g. 'architect', 'period', 'material'). Each row appears as an editable keyword list in every specific POI.\n" +
            "System axes (category / hierarchy / badge / outline) are handled automatically from their respective tables above.\n" +
            "Synonym suggestion (WordNet EN / AI) is a planned feature -- manual keyword entry is fully functional now.";

        // Per-POI keyword section labels.
        private static readonly string SearchKeywordsDerivedHelp = "These keywords are derived automatically from the taxonomy selections above and indexed at runtime. No action needed.";
        private static readonly string SearchKeywordsOthersHelp = "Freeform keywords that do not belong to any defined search axis (comma-separated). Indexed alongside all other keyword sources.";
    }
}
