// POIEditorToolWindow.HierarchyHelp.cs
//
// Partial: every constant of the Hierarchy Levels section of Global Scene (_2.3_Marker_Hierarchy.md):
// column (i) help texts and the three Test guides. ASCII only; app-agnostic (no real wall's POI ids,
// categories or level names -- _5.1_Editor_Tab.md, "Domain Manual Tests"); every text names an Editor
// Tab control, never a project doc or code file. The guide contract test (HierarchyDesignAuthoringTests)
// reads these by reflection.

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private const string HierarchyPreviewHelp =
            "Editor and development builds only (release builds ignore it). The SAME switch as Effects > Test > 'Add effects demo grid' and Labels, Text & Fonts > Test > 'Add Labels demo grid' -- ticking one ticks all three. Its last row shows one marker per hierarchy level with that level's real Size, Show Marker Label?, Marker Label Style ('Aa'), Ripple/Halo/Pulse, Spin Ring and Reveal timing, on its own camera far from the wall so no scenery can hide it. Every cell is labelled with the Base marker's own name, exactly as a real POI at that level would be -- a hierarchy level name is never shown under a marker. Edit this table while Play Mode runs and the grid rebuilds live. Only ONE demo grid can be on screen at a time: ticking this unticks Outline > Test > 'Add outline demo grid'. Play reads the SAVED config: after ticking, press Save All to JSON and Copy to StreamingAssets, then Play. Off by default.";

        private const string HierarchySearchKeywordsHelp =
            "Extra search terms every POI at this level is indexed with, on top of its own keywords (Search & Filter section). Comma-separated. Useful for a term that applies to a whole tier (a framework default suggestion: 'landmark' for every top-tier POI) instead of repeating it per POI.";

        // ---------------- Hierarchy Levels guides ----------------
        //
        // One block per column group, in table order. They describe framework behaviour, not one
        // specific wall's taxonomy; a concrete example is framed as a "framework default suggestion".

        private static readonly string HierarchySceneTestGuide =
            "SETUP\n" +
            "- Top bar 'Load & Populate Rig'. The rig refreshes on every change here, so the Scene view always shows the current look (static: nothing animates in Edit Mode).\n\n" +
            "MARKER SIZE\n" +
            "- Change a level's Marker Size (cm): every POI at that level resizes at once. Compare your largest level against your smallest -- the difference should be obvious.\n\n" +
            "MARKER LABEL\n" +
            "- Tick/untick Show Marker Label? on a level: every POI at that level shows/loses the text under its Symbol at once. The text is always the POI's own name, never the level name.\n" +
            "- 'Aa' (Marker Label Style): tick 'Override global label config' and move Gap / Font size / Font -- only that level's labels change. Untick it and they follow Global Scene > Labels, Text & Fonts again.\n\n" +
            "PER-POI (Specific Marker > POI > Marker Style > Hierarchy Level)\n" +
            "- Change one POI's level: it takes on the new level's size and label at once.\n" +
            "- A POI with '(none)' renders at the 12cm framework Fallback (no label, no effects), and Config Validation flags it -- a framework default suggestion is to give every real POI an explicit level.\n\n" +
            "SPIN RING\n" +
            "- Only shown while the wall's Outline is enabled. The ring only turns in Play Mode.\n\n" +
            "EFFECTS, REVEAL AND FACING\n" +
            "- Not possible in Scene test: Ripple / Halo / Pulse, the reveal fade/scale-in and Spin Ring turning all need Play Mode to tick; Facing Override needs a moving camera. Use Play Mode or a device.";

        private static readonly string HierarchyPlaymodeTestGuide =
            "SETUP\n" +
            "- 'Save All to JSON' then 'Copy to StreamingAssets' (Play reads the copy), open your wall's scene, press Play, click into the Game view.\n" +
            "- Editor camera: W/A/S/D = move, E = up, Q = down, RMB + mouse or arrow keys = look, Z/C = roll.\n" +
            "- LIVE: while Play runs, every column of this table and each POI's own Hierarchy Level (Specific Marker > Marker Style) reach the running wall at once. Nothing is saved by that; press Save All to JSON (then Copy) to keep it.\n" +
            "- Fastest check: tick 'Add Hierarchy demo grid' below -- one marker per level, side by side, labelled with the Base marker's name.\n\n" +
            "MARKER SIZE AND MARKER LABEL\n" +
            "- Watch POIs at different levels: sizes taper from your top tier to your lowest, and typically only the top tier shows a label (a framework default suggestion, not a rule).\n" +
            "- 'Aa' (Marker Label Style): drag its window clear of the Game view, tick 'Override global label config' and move the sliders -- that level's markers and its demo grid cell follow live.\n\n" +
            "EFFECTS AND REVEAL\n" +
            "- Give two levels different Ripple/Halo/Pulse combinations and watch both running at once.\n" +
            "- Stop and Play again to watch the reveal: Delay staggers when each level fades/scales in, Duration sets how slowly.\n\n" +
            "PRIORITY AND FACING\n" +
            "- Priority is not visible by itself: it decides which markers LOD hides first and which one Displacement moves (see those sections).\n" +
            "- Facing Override: set one level to Always Facing Camera and walk around -- its POIs turn to face you while a level set to Inherit keeps the wall's Orientation > Facing Options.\n\n" +
            "PER-POI\n" +
            "- Move one POI to another level in Specific Marker > Marker Style > Hierarchy Level: it changes size, label, effects and facing at once.\n" +
            "- Automated: Test Runner, EditMode + PlayMode, zero failures.";

        private static readonly string HierarchyDeviceTestGuide =
            "SETUP (Android)\n" +
            "- USB: enable USB debugging, plug in, accept the prompt, 'adb devices' lists it.\n" +
            "- Wi-Fi: Developer options > Wireless debugging > Pair with code; 'adb pair <ip>:<pair-port>' + code, then 'adb connect <ip>:<port>'; or 'npx adb-qr-connect'.\n" +
            "- Untick 'Add Hierarchy demo grid' first if it was left on (development builds would show the grid instead of the wall). 'Save All to JSON' + 'Copy to StreamingAssets', then Build Settings > Android > Build And Run. No editor camera on device.\n\n" +
            "LOGS\n" +
            "- 'adb logcat -c', then 'adb logcat -s Unity > __logcat.txt' and read the file (no piping).\n" +
            "- Look for '[Config] loaded N hierarchy levels' and any ripple_effect/halo_effect 'Unknown ... ignoring' warning.\n\n" +
            "WHAT ONLY A DEVICE SHOWS\n" +
            "- Is your smallest level still recognisable at arm's length outdoors in bright light, and is its label (if shown) readable?\n" +
            "- Does the Reveal Delay stagger read as intentional (not just lag) on real hardware?\n" +
            "- Battery and frame rate with every level's effects and a spinning outline running together over a few minutes.";
    }
}
