// POIEditorToolWindow.DisplacementHelp.cs
//
// Partial: every text of the Displacement section (_2.5_Marker_Displacement.md) -- option arrays, (i)
// help bodies and the three Scene / Playmode / Device Test guides (_5.1_Editor_Tab.md, "Domain Manual
// Tests"). Every text is ASCII, app-agnostic and names Editor Tab controls only
// (DisplacementEditorTabTests pins it).

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // ---- option arrays (saved value / what the developer reads) ----

        private static readonly string[] DisplaceTargetOptions = { "label_only", "marker", "both" };
        private static readonly string[] DisplaceTargetLabels = { "Label only", "Marker", "Both" };
        private static readonly string[] DisplacementAlgorithmOptions = { "fixed_axis", "candidate_position", "force_directed" };
        private static readonly string[] DisplacementAlgorithmLabels = { "Fixed Axis", "Candidate Position", "Force Directed" };
        private static readonly string[] DisplacementTiebreakOptions = { "symmetric", "lower_priority_only" };
        private static readonly string[] DisplacementTiebreakLabels = { "Symmetric", "Lower priority only" };
        private static readonly string[] LeaderLineStyleOptions = { "straight", "dashed", "elbow" };
        private static readonly string[] LeaderLineStyleLabels = { "Straight", "Dashed", "Elbow" };
        private static readonly string[] DisplacementDemoReferenceLabels = { "Side by side", "Overlay", "Off" };

        // ---- (i) help ----

        private static readonly string DisplacementEnabledHelp =
            "Nudges crowded markers apart on screen so every label stays readable. Unlike LOD nothing is hidden or " +
            "merged: a label (or a marker) moves a little away from its true place and moves back as soon as it has room.\n\n" +
            "It works with LOD on or off. Off: every marker and label stays exactly on its true place; turning it off in " +
            "Play Mode puts everything back at once.";

        private static readonly string DisplacementOverlapHelp =
            "Two markers whose centres are closer than this on screen (pixels) form a crowded group that is spread apart. " +
            "Chains count: if A is close to B and B is close to C, all three form one group.\n\n" +
            "It is measured between marker centres whatever the marker size: raise it if your largest markers still touch, " +
            "lower it if markers move while they clearly have room. A marker must be crowded for two evaluations in a row " +
            "before it moves, so nothing blinks at the edge. This is its own setting, separate from the LOD Crowding Radius.";

        private static readonly string DisplacementTargetHelp =
            "Label only (recommended): only the text label shifts; the marker stays exactly on its point of interest.\n" +
            "Marker: the whole marker, label included, shifts off its point.\n" +
            "Both: the marker shifts and its label shifts again on top, so the label travels twice as far.\n\n" +
            "Only with Label only can Max Move hide a label that still has no room.";

        private static readonly string DisplacementAlgorithmHelp =
            "How a crowded group is spread:\n" +
            "- Fixed Axis: stacks the group in a vertical column, the most important in the middle. Cheapest, always the " +
            "same shape.\n" +
            "- Candidate Position: tries spots on rings around each marker and keeps the nearest free one; the most " +
            "important marker picks first.\n" +
            "- Force Directed (default): the members push each other apart while each is pulled back toward its true " +
            "place, more strongly the more important it is. Looks the most natural.";

        private static readonly string DisplacementTiebreakHelp =
            "When markers of different importance crowd each other:\n" +
            "- Symmetric: every member moves.\n" +
            "- Lower priority only: the most important member (lowest Priority in the Hierarchy Levels table) stays exactly " +
            "in place and the others move away from it. When two or more share that lowest Priority, the group falls back " +
            "to Symmetric.";

        private static readonly string DisplacementMaxMoveHelp =
            "The farthest (screen pixels) a label or marker may travel. With What Moves = Label only, a less important label " +
            "that still overlaps at this limit is hidden until it has room again (its marker stays). With Marker or Both " +
            "nothing is hidden. Raise it for dense walls, lower it to keep labels close to their markers.";

        private static readonly string LeaderLinesEnabledHelp =
            "A thin line in the marker's own category colour from where the moved element belongs to where it is drawn, so " +
            "a visitor can still tell which marker a moved label belongs to. Label only: from the marker's edge to the " +
            "label. Marker or Both: from the true point to the moved marker.";

        private static readonly string LeaderLineStyleHelp =
            "Straight: one segment. Dashed: the same segment drawn as dashes (quieter). Elbow: straight up or down on " +
            "screen first, then across (a tidy right angle).";

        private static readonly string LeaderLineMinLengthHelp =
            "A move shorter than this (screen pixels) gets no line: a small nudge does not need one.";

        private static readonly string LeaderLineWidthHelp =
            "Line thickness in metres, so it gets thinner on screen with distance, like the markers themselves.";

        private static readonly string LeaderLineOpacityHelp =
            "0 = invisible, 1 = the marker's full category colour.";

        // ---- Test: displacement demo ----

        private static readonly string DisplacementDemoHelp =
            "Developer-only. In Play Mode the camera moves to an empty stage with four crowded groups of generated markers " +
            "(the wall's own POIs pause); untick it to come back to the wall.\n" +
            "- Top left: every marker on the same Hierarchy Level (only the Algorithm decides).\n" +
            "- Top right: markers on different levels (shows Who Gives Way).\n" +
            "- Bottom left: one top-level marker among the smallest level (big and small markers).\n" +
            "- Bottom right: one lone marker that must never move.\n" +
            "Works in the Editor and in development builds only; untick it before a normal build.";

        private static readonly string DisplacementDemoGroupHelp =
            "Markers in each crowded group. More markers = a harder crowd to spread.";

        private static readonly string DisplacementDemoSpreadHelp =
            "How far apart (centimetres) the true places of a group's markers are. 0 = exactly on top of each other; raise " +
            "it until the group is no longer crowded to see where Overlap Distance stops acting.";

        private static readonly string DisplacementDemoDistanceHelp =
            "Distance from the camera to the demo. Farther = markers look smaller = more crowding on screen (the demo " +
            "spacing grows with it, so everything stays in view).";

        private static readonly string DisplacementDemoLabelsHelp =
            "Ticked: every demo marker shows its text label, whatever its level says (labels are what Label only moves). " +
            "Unticked: no labels.";

        private static readonly string DisplacementDemoReferenceHelp =
            "Faded copies of the markers at their TRUE places, which never move, so you see what displacement changed.\n" +
            "- Side by side: each group's copy stands to its left (true places) and the live group to its right.\n" +
            "- Overlay: the copy sits exactly under the live group; everything that moved shows as a faded ghost behind.\n" +
            "- Off: only the live markers.";

        private static readonly string DisplacementDemoRunLodHelp =
            "Unticked (recommended): LOD leaves the demo alone, so crowding does not shrink or cluster the very groups you " +
            "are looking at. Ticked: the demo runs through LOD first, exactly like a real wall.";

        private static readonly string DisplacementReadoutHelp =
            "What the last displacement evaluation decided, as numbers, so every setting's effect can be read without " +
            "judging by eye: crowded groups found, markers moved, the longest move, labels hidden by Max Move and leader " +
            "lines drawn. Play Mode only; it follows every edit live.";

        // ---- the three Test guides ----

        private static readonly string DisplacementSceneTestGuide =
            "SETUP\n" +
            "- Nothing to set: this section has no Scene-Mode Preview.\n\n" +
            "OVERLAP DETECTION, RESOLUTION, LEADER LINES\n" +
            "- Not possible in Scene test: crowding is measured on the running camera's screen every evaluation, and the " +
            "Scene view has no such camera. Use Play Mode.\n" +
            "- What the Scene view does show: every POI's true place (Load & Populate Rig). Markers that sit close together " +
            "there are the ones that will be spread apart in Play Mode.";

        private static readonly string DisplacementPlaymodeTestGuide =
            "SETUP\n" +
            "- Open the wall scene and press Play, then tick Test > Add displacement demo.\n" +
            "- The camera jumps to the demo's empty stage (the wall's POIs pause); untick it to come back to the wall.\n" +
            "- Keep Reference copies on Side by side: the faded left half of each group shows the TRUE places.\n" +
            "- Watch the Live Displacement Readout under the demo settings while you edit.\n" +
            "- Game view controls: W/A/S/D move, Q/E down/up, right mouse or Alt + left mouse or arrows look around, Z/C roll.\n" +
            "- LIVE: every Displacement and demo edit takes effect while Play Mode runs. Save All to JSON keeps it.\n\n" +
            "OVERLAP DETECTION\n" +
            "- Raise Spread (cm) step by step: at some point a group stops moving (Groups drops in the readout). Raise " +
            "Overlap Distance (px): it moves again.\n" +
            "- The bottom right Lone marker never moves.\n\n" +
            "RESOLUTION\n" +
            "- What Moves: Label only moves the texts, the circles stay; Marker moves whole markers off their true places; " +
            "Both moves the markers and pushes the labels further.\n" +
            "- Algorithm: Fixed Axis stacks the labels in a column, Candidate Position scatters them to the nearest free " +
            "spots, Force Directed spreads them organically.\n" +
            "- Who Gives Way: switch to Lower priority only and look at the top right and bottom left groups: the most " +
            "important marker (lowest Priority) stops moving, the others move away from it. The top left group (one shared " +
            "level) behaves as Symmetric.\n" +
            "- Max Move (px): lower it to about 20 with Label only: the least important labels disappear (Labels hidden in " +
            "the readout); raise it and they come back.\n\n" +
            "LEADER LINES\n" +
            "- Tick Enable Leader Lines: every moved label or marker gets a line in its category colour.\n" +
            "- Style: Straight, Dashed (the line is broken into dashes), Elbow (up or down first, then across).\n" +
            "- Min Length (px): raise it above the Longest move in the readout: every line disappears.\n" +
            "- Width (m) and Opacity: thicker/thinner, stronger/fainter.\n\n" +
            "DEMO CONTROLS\n" +
            "- Markers per Group, Spread (cm), Distance (m): more markers, closer true places or a farther demo = more crowding.\n" +
            "- Show labels off: no labels, so Label only has nothing to move (Moved drops to 0).\n" +
            "- Run LOD on the demo: ticked, LOD may shrink or cluster the groups before displacement runs.\n" +
            "- Untick Enable Displacement: everything jumps back to its true place and every line disappears.\n\n" +
            "- Test Runner: run EditMode and PlayMode, zero failures.";

        private static readonly string DisplacementDeviceTestGuide =
            "SETUP\n" +
            "- Connect the phone by USB, or by Wi-Fi: adb pair <ip>:<port> with the pairing code, then adb connect " +
            "<ip>:<port> (or npx adb-qr-connect).\n" +
            "- Save All to JSON, then Copy to StreamingAssets (the build reads that copy).\n" +
            "- File > Build Settings > Build And Run. There is no mock camera on the device: walk with the phone.\n" +
            "- Logs: adb logcat -c, then adb logcat -s Unity > __logcat.txt, and read the file.\n\n" +
            "WHAT ONLY A DEVICE SHOWS\n" +
            "- Real screen density: Overlap Distance, Max Move and Min Length are screen pixels, and a phone has many more " +
            "of them per centimetre than the Game view. Stand where POIs crowd and check the labels are readable and the " +
            "lines are not too thin; retune the pixel values for the device.\n" +
            "- Real hand shake: stand still in front of a crowded spot; labels must not blink or wander.\n" +
            "- The displacement demo works in development builds too (Add displacement demo); untick it before a normal build.";
    }
}
