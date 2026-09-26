// POIEditorToolWindow.LodZoomHelp.cs
//
// Partial: every text of the LOD and Zoom sections -- option arrays, (i) help bodies and the three
// Scene / Playmode / Device Test guides of each (_5.1_Editor_Tab.md, "Domain Manual Tests"). Every
// text is ASCII, app-agnostic and names Editor Tab controls only (LodZoomHelpTextTests pins it).

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // ---- option arrays (saved value / what the developer reads) ----

        private static readonly string[] DensityModeOptions = { "none", "select_hide", "cluster", "shrink_and_fade", "hybrid" };
        private static readonly string[] DensityModeLabels = { "None", "Select & Hide", "Cluster", "Shrink & Fade", "Hybrid" };
        private static readonly string[] ClusterIconOptions = { "pie_and_count", "dominant_category", "count_only" };
        private static readonly string[] ClusterIconLabels = { "Pie & Count", "Dominant Category", "Count Only" };
        private static readonly string[] ClusterBandSourceOptions = { "centroid", "nearest_member", "farthest_member" };
        private static readonly string[] ClusterBandSourceLabels = { "Centroid", "Nearest Member", "Farthest Member" };

        // ---- LOD (i) help ----

        private static readonly string LodEnabledHelp =
            "Level of detail: while the visitor moves, decides how many markers show and how prominently, from each marker's " +
            "real distance to the camera and from how crowded the screen is around it.\n\n" +
            "Off: every marker is always shown at full size, whatever the distance or crowding. Turning it off in Play Mode " +
            "brings every hidden, shrunk or clustered marker back at once.";

        private static readonly string LodBandsHelp =
            "Distance bands, from near to far. A marker belongs to the first row whose 'Up to (m)' is larger than its distance " +
            "(real distance divided by the current zoom, so zooming in counts as walking closer).\n\n" +
            "Max markers: how many markers (a cluster counts as one) the band shows; the lowest Hierarchy Levels go first. " +
            "-1 = all.\n\n" +
            "Give the last row a large 'Up to (m)' (for example 9999) so it catches everything farther away.\n\n" +
            "Suggest Values: fills the bands, Crowded At and Shrink Starts At from the number of POIs. Nothing else changes.";

        private static readonly string LodBandHysteresisHelp =
            "Metres a marker must come back past a band edge before it moves to the nearer band again. Moving away takes " +
            "effect at the edge itself. Stops markers blinking when a visitor sways right at a band edge. 0 = no buffer.";

        private static readonly string LodDensityResponseHelp =
            "What happens to a marker when many others are crowded around it on screen:\n\n" +
            "None: nothing (only the Distance Bands apply).\n" +
            "Select & Hide: markers at Crowded At or more are hidden, lowest Hierarchy Levels first.\n" +
            "Cluster: markers at Crowded At or more merge into one cluster marker.\n" +
            "Shrink & Fade: markers shrink and fade from Shrink Starts At to Crowded At, down to the Shrink Floor; none disappears.\n" +
            "Hybrid: Shrink & Fade first, then Cluster once a marker reaches Crowded At.\n\n" +
            "A change only takes effect when two evaluations in a row agree, so a head turn does not make markers blink.";

        private static readonly string LodDensityRadiusHelp =
            "On-screen distance, in pixels, within which two markers count as neighbours. Larger = crowding starts earlier. " +
            "Keep it close to the Displacement section's Overlap Threshold so both sections agree on what 'crowded' means.";

        private static readonly string LodShrinkStartHelp =
            "Neighbour count at which a marker starts to shrink and fade (Shrink & Fade and Hybrid). It must be smaller than " +
            "Crowded At; the window warns when it is not.";

        private static readonly string LodClusterMinHelp =
            "Neighbour count at which a marker counts as crowded: Select & Hide hides it, Cluster and Hybrid merge it into a " +
            "cluster, Shrink & Fade reaches the Shrink Floor. Keep it above 2 so two or three nearby markers are left to the " +
            "Displacement section (nudged apart) instead of disappearing into a cluster.";

        private static readonly string LodShrinkFloorHelp =
            "Size and opacity of a marker at Crowded At, as a share of its normal look (0.4 = 40%). The same value drives both, " +
            "so a crowded marker gets smaller and more transparent together. It never goes below this: a faded marker still " +
            "tells the visitor that something is there.";

        private static readonly string LodSafetyNetHelp =
            "Safety Net: only for Select & Hide and Shrink & Fade (Cluster and Hybrid already cluster on their own). When a marker has more than " +
            "Crowded At x Safety Multiplier neighbours, it is clustered anyway, so a region far denser than expected never " +
            "becomes an unreadable pile. Keep the multiplier high enough that it only catches real surprises.";

        private static readonly string LodClusterIconHelp =
            "What a cluster marker shows:\n\n" +
            "Pie & Count: a pie with one slice per category (share of its members) and '+N'.\n" +
            "Dominant Category: the icon of its most frequent category and '+N'.\n" +
            "Count Only: just '+N'.";

        private static readonly string LodClusterSizeHelp =
            "Cluster diameter as a multiple of its largest member's marker size; it also grows a little with the member count. " +
            "Clusters live on the wall like markers, so they get smaller with distance exactly like markers do.";

        private static readonly string LodClusterBandSourceHelp =
            "Which distance decides a cluster's Distance Band:\n\n" +
            "Centroid: the middle of the cluster (smoothest).\n" +
            "Nearest Member: its closest member (the cluster counts as near as soon as one member is).\n" +
            "Farthest Member: its farthest member (the cluster counts as near only when all members are).";

        private static readonly string LodClusterBandHysteresisHelp =
            "Cluster Band Hysteresis: gives clusters the same Band Hysteresis buffer as markers, so a cluster sitting on a band edge does not blink " +
            "between two bands.";

        private static readonly string LodClusterDissolveGraceHelp =
            "How many evaluations in a row a cluster's group must be gone before the cluster fades out. 0 = it fades out " +
            "immediately. A few evaluations stop clusters popping in and out when the visitor is at the edge of a crowded area.";

        private static readonly string LodTransitionsHelp =
            "Seconds a marker or cluster takes to fade in, fade out, shrink or grow when LOD changes it, instead of popping. " +
            "0 = instant.";

        private static readonly string LodEvalIntervalHelp =
            "Seconds between two LOD evaluations. Distance and crowding change slowly, so this does not need every frame; a " +
            "larger value saves battery on slower phones and very dense walls, a smaller one reacts faster.";

        private static readonly string LodFrustumHelp =
            "Skips markers outside the camera's view before measuring distance and crowding (cheaper). Turn it off only when " +
            "checking why a marker does not show.";

        private static readonly string LodFovMarginHelp =
            "Culling Margin: degrees added around the camera's view for Frustum Culling only, so a marker just outside the screen edge is " +
            "already decided (and fading in) when it comes into view.";

        // ---- LOD demo field (i) help ----

        private static readonly string DemoFieldHelp =
            "Developer only. In Play Mode, replaces the wall's POIs with generated markers: the numbers per Hierarchy Level " +
            "you choose below, scattered through a box that reaches through several Distance Bands, plus one dense clump. " +
            "In the Editor the field stands on an empty stage away from the scene and the camera moves to its start, so " +
            "nothing of the scene can cover it; unticking brings the camera and the wall's POIs back. Walk through it with " +
            "the mock camera to see every LOD setting working; every edit here rebuilds the field in the same place.\n\n" +
            "Works in the Editor and in development builds, never in release builds. Untick it before a normal build (the build " +
            "asks you if it is still on). Turning it on turns off the effects and outline demo grids, which would cover it.";

        private static readonly string DemoFieldLabelsHelp =
            "Show labels off: demo markers show no text label, whatever their Hierarchy Level says -- useful to judge crowding by the " +
            "symbols alone.";

        private static readonly string DemoFieldCountsHelp =
            "How many demo markers of each Hierarchy Level to scatter through the field (0 = none). Their size, label and " +
            "effects follow the level, and the level decides who stays when a band's Max markers is reached.";

        private static readonly string DemoFieldClumpHelp =
            "Dense Clump: extra demo markers packed close together about a quarter of the way into the field, so Crowding and Clusters " +
            "always have something to act on. Clump Radius sets how tightly they are packed.";

        private static readonly string DemoFieldBoxHelp =
            "Where the field is: it starts Field Distance in front of the camera's start and reaches Field Depth further away; Field " +
            "Width and Field Height set its side-to-side and up-down spread. A deep field spans several Distance Bands at once.";

        private static readonly string LodReadoutHelp =
            "In Play Mode, what LOD decided in its last evaluation, updated as you move: how many markers show, how many " +
            "are smaller and fainter (Shrink Floor), how many each rule hid (crowding, a band's Max markers, out of view), " +
            "the clusters and how many markers they hold, the current zoom, and for each Distance Band how many of its " +
            "markers show. Change a setting and watch the numbers answer. Works with the wall's own POIs and with the LOD " +
            "demo field.";

        private static readonly string DemoFieldReshuffleHelp =
            "Same settings = same field every time. Reshuffle scatters the markers differently.";

        // ---- Zoom (i) help ----

        private static readonly string ZoomEnabledHelp =
            "Lets the visitor zoom the camera view in and out (pinch, double-tap, on-screen buttons). The markers zoom with the " +
            "view, and zooming in counts as walking closer for the LOD Distance Bands, so more markers appear. Off: the view " +
            "always stays at 1x.";

        private static readonly string ZoomMinHelp =
            "Smallest zoom. 1 = the camera's own view. Never set it below 1.";

        private static readonly string ZoomMaxHelp =
            "Largest zoom; nothing zooms further. Beyond about 5x the camera image gets blurry on most phones, and LOD treats " +
            "the visitor as very close, so almost every marker shows.";

        private static readonly string ZoomTapStepHelp =
            "How much one double-tap zooms in (1.5 = one and a half times the current zoom).";

        private static readonly string ZoomTapLevelsHelp =
            "How many double-taps zoom in before the next one returns to 1x. 2 = zoom, zoom again, then back to 1x.";

        private static readonly string ZoomTransitionHelp =
            "Seconds a double-tap or button zoom takes to animate. A pinch follows the fingers directly and never animates.";

        private static readonly string ZoomUiButtonsHelp =
            "Shows zoom in, zoom out and back-to-1x buttons on screen, for visitors who do not pinch. Hidden while Enable Zoom " +
            "is off.";

        private static readonly string ZoomDoubleTapWindowHelp =
            "Seconds within which the second tap must follow the first to count as a double-tap.";

        private static readonly string ZoomDoubleTapMoveToleranceHelp =
            "Tap Tolerance: how far, in pixels, the second tap may land from the first and still count as a double-tap rather than a drag.";

        // ---- LOD Test guides ----

        private static readonly string LodSceneTestGuide =
            "SETUP\n" +
            "- LOD decides from a moving camera, so the Scene view shows every marker as authored.\n\n" +
            "DISTANCE BANDS\n" +
            "- Not possible in Scene test, use Play Mode.\n\n" +
            "CROWDING\n" +
            "- Not possible in Scene test, use Play Mode.\n\n" +
            "CLUSTERS\n" +
            "- Not possible in Scene test, use Play Mode.\n\n" +
            "TRANSITIONS & PERFORMANCE\n" +
            "- Not possible in Scene test, use Play Mode.";

        private static readonly string LodPlaymodeTestGuide =
            "SETUP\n" +
            "- Open the wall scene and press Play, then tick Test > Add LOD demo field and set a few markers per level.\n" +
            "- The camera jumps to the field's empty stage (the wall's POIs pause); untick it to come back to the wall.\n" +
            "- To start Play with the field already on: Save All to JSON, then Copy to StreamingAssets (Play reads that copy).\n" +
            "- Game view controls: W/A/S/D move, Q/E down/up, right mouse or Alt + left mouse or arrows look around, Z/C roll.\n" +
            "- LIVE: every LOD and demo field edit takes effect while Play Mode runs. Save All to JSON keeps it.\n\n" +
            "DISTANCE BANDS\n" +
            "- Walk slowly away from the field: past each band's 'Up to (m)' fewer markers show; the lowest Hierarchy Levels " +
            "go first.\n" +
            "- Walk back: markers return only after Band Hysteresis metres past the edge (no blinking).\n" +
            "- Set a band's Max markers to 1: only one marker (or cluster) of that band stays.\n\n" +
            "CROWDING\n" +
            "- Look at the dense clump. Switch Response through its options:\n" +
            "  - None: nothing changes.\n" +
            "  - Select & Hide: clump markers disappear.\n" +
            "  - Cluster: the clump becomes one cluster with '+N'.\n" +
            "  - Shrink & Fade: clump markers get smaller and more transparent, never gone.\n" +
            "  - Hybrid: shrink first, then a cluster once the clump is crowded enough.\n" +
            "- Raise Crowding Radius: crowding starts from farther away. Lower Shrink Floor: crowded markers get smaller.\n\n" +
            "CLUSTERS\n" +
            "- With Cluster or Hybrid, switch Cluster Icon (Pie & Count, Dominant Category, Count Only) and Cluster Size.\n" +
            "- Walk until a cluster sits on a band edge and switch Band Source (Centroid, Nearest Member, Farthest Member).\n" +
            "- Walk away from the clump and back: the cluster fades after Dissolve Grace evaluations.\n\n" +
            "TRANSITIONS & PERFORMANCE\n" +
            "- Set Fade (s) to 1: every hide, show and shrink animates slowly. 0: instant.\n" +
            "- Untick Frustum Culling: nothing visible changes, it only saves work.\n" +
            "- Untick Enable LOD: every marker comes back at full size.\n\n" +
            "ZOOM\n" +
            "- Stand far from the field and zoom in (see the Zoom section's Test): more markers appear.\n\n" +
            "- Test Runner: run EditMode and PlayMode, zero failures.";

        private static readonly string LodDeviceTestGuide =
            "SETUP\n" +
            "- Phone: USB debugging on. Wi-Fi: adb pair <ip>:<port> with the pairing code, then adb connect <ip>:<port> " +
            "(or npx adb-qr-connect).\n" +
            "- For a normal build untick Test > Add LOD demo field; tick it only to test LOD on the phone.\n" +
            "- Save All to JSON, Copy to StreamingAssets, then File > Build And Run (development build to see logs).\n" +
            "- Logs: adb logcat -c, then adb logcat -s Unity > __logcat.txt, and read the file.\n\n" +
            "DISTANCE BANDS\n" +
            "- Walk slowly toward the wall and away: markers appear and disappear at the band distances, with no blinking " +
            "when you stop right at an edge.\n\n" +
            "CROWDING\n" +
            "- Stand close to the wall and look along it at a shallow angle: markers squeezed together on screen shrink, hide " +
            "or cluster by the Response, and settle when you hold still.\n\n" +
            "CLUSTERS\n" +
            "- A cluster is easy to tap and never covers most of the screen; it matches the markers' size on the wall.\n\n" +
            "TRANSITIONS & PERFORMANCE\n" +
            "- Walk for a few minutes: no stutter, the phone does not get hot. If it does, raise Evaluation Interval (s).";

        // ---- Zoom Test guides ----

        private static readonly string ZoomSceneTestGuide =
            "SETUP\n" +
            "- Zoom changes the running camera, so the Scene view never zooms.\n\n" +
            "ZOOM RANGE\n" +
            "- Not possible in Scene test, use Play Mode.\n\n" +
            "GESTURES & BUTTONS\n" +
            "- Not possible in Scene test, use Play Mode.";

        private static readonly string ZoomPlaymodeTestGuide =
            "SETUP\n" +
            "- Click Save All to JSON, then Copy to StreamingAssets, open the wall scene and press Play.\n" +
            "- LIVE: every Zoom edit takes effect while Play Mode runs. Save All to JSON keeps it.\n" +
            "- The Editor has no touch: pinch and double-tap need a phone. The on-screen buttons work with the mouse.\n\n" +
            "ZOOM RANGE\n" +
            "- Click the zoom in button until it stops: that is Max Zoom. Lower Max Zoom: the view zooms back out to it.\n" +
            "- With LOD > Test > Add LOD demo field on, stand far away and zoom in: more markers appear.\n\n" +
            "GESTURES & BUTTONS\n" +
            "- Set Transition (s) to 1: button zooms animate slowly.\n" +
            "- Untick Show On-Screen Buttons: the buttons disappear. Untick Enable Zoom: the view returns to 1x and the " +
            "buttons disappear.";

        private static readonly string ZoomDeviceTestGuide =
            "SETUP\n" +
            "- Phone: USB debugging on. Wi-Fi: adb pair <ip>:<port> with the pairing code, then adb connect <ip>:<port> " +
            "(or npx adb-qr-connect).\n" +
            "- Save All to JSON, Copy to StreamingAssets, then File > Build And Run.\n" +
            "- Logs: adb logcat -c, then adb logcat -s Unity > __logcat.txt, and read the file.\n\n" +
            "ZOOM RANGE\n" +
            "- Pinch out: the camera image AND the markers zoom together, and stop at Max Zoom. Pinch in: back to Min Zoom.\n" +
            "- Check the camera image stays sharp enough at Max Zoom on this phone.\n\n" +
            "GESTURES & BUTTONS\n" +
            "- Double-tap: zooms by Double-Tap Step, Double-Tap Levels times, then back to 1x.\n" +
            "- Two slow taps or a small drag must NOT zoom (Double-Tap Window, Tap Tolerance).\n" +
            "- The on-screen buttons sit clear of the notch and the home bar and are easy to tap.";
    }
}
