// POIEditorToolWindow.EffectsHelp.cs
//
// Partial: every constant of the Effects domain (_2.2.4_Markers_Effects.md): the level-table
// dropdown options, the (i) help texts and the three Test guides. Kept out of Constants.cs so that
// file stays a manageable size. ASCII only (20-code-quality.md 2.1); tests read these fields by
// reflection to check ASCII, coverage of every effect name and the level-table option strings.

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // Hierarchy Levels table dropdowns (HierarchyLevelEntry.ripple_effect / halo_effect).
        private static readonly string[] RippleEffectOptions = { "none", "ripple_rings", "ripple_discs" };
        private static readonly string[] RippleEffectLabels = { "None", "Rings", "Discs" };
        private static readonly string[] HaloEffectOptions = { "none", "halo_ring", "halo_disc", "beacon" };
        private static readonly string[] HaloEffectLabels = { "None", "Ring", "Disc", "Beacon" };

        // ---------------- (i) help texts ----------------

        private const string EffectsFlowNote =
            "Flow: 1) tick and tune the effects on this page. 2) Pick which effect each hierarchy level uses in the Hierarchy Levels table (Ripple / Halo / Pulse columns); only ticked effects are offered there. 3) Every POI takes the effects of its hierarchy level. Reveal delay / duration and Rotate are also set per level in that table.";

        private static readonly string EffectsEnabledHelp =
            "Master switch for every marker effect on this wall. Off = no pulse, ripple, halo or beacon anywhere, and everything below is hidden (your settings are kept and still saved, so switching it off by mistake loses nothing). Also removes the animation margin from the displacement radius. Use it for weak devices or as a reduce-motion option. The reveal fade-in is separate.\n\n" + EffectsFlowNote;

        private const string EffectsPeriodHelp =
            "Seconds for one full cycle. Values below 0.1 are raised to 0.1 at runtime (a zero period would break the animation maths).";

        private const string PulseHelp =
            "The marker symbol grows and shrinks smoothly, like breathing. The cheapest effect and it adds no extra shapes.\n\nAmplitude = how big the swing is. Period = seconds per breath.\n\nChosen per hierarchy level: tick Pulse in the Hierarchy Levels table.";
        private const string RippleRingsHelp =
            "Three thin rings flow outward from the symbol one after another, like ripples on water.\n\nPeriod = seconds per cycle. Stagger = delay between the inner, middle and outer ring. Alphas = how strong each ring starts (it fades to 0 as it grows). Tint = ring colour.\n\nChosen per hierarchy level: Ripple column, Rings. For a soft filled version see Ripple Discs; for a single wave see Beacon.";
        private const string RippleDiscsHelp =
            "Three filled discs flow outward from the symbol one after another. Discs cover more area than rings, so they usually need lower alphas.\n\nPeriod, Stagger, Alphas and Tint work as in Ripple Rings but are stored separately.\n\nChosen per hierarchy level: Ripple column, Discs.";
        private const string HaloRingHelp =
            "One thin ring behind the symbol that breathes (grows and shrinks) at a steady brightness. Calmer than Ripple: one layer, no outward flow.\n\nSize = ring diameter as a multiple of the symbol. Base alpha = brightness. Breathe amplitude = size of the swing. Outer / Inner scale = ring thickness.\n\nChosen per hierarchy level: Halo column, Ring.";
        private const string HaloDiscHelp =
            "One soft filled disc behind the symbol that breathes at a steady brightness, like a glow.\n\nSize = disc diameter as a multiple of the symbol. Base alpha = brightness. Breathe amplitude = size of the swing. Radius scale = how much of the disc is filled.\n\nChosen per hierarchy level: Halo column, Disc.";
        private const string BeaconHelp =
            "One ring that grows and fades out, then restarts, like a radar ping. A single outward wave, unlike Ripple's three.\n\nSize = ring diameter as a multiple of the symbol. Base alpha = starting brightness. Start / End scale = how far the wave travels. Outer / Inner scale = ring thickness.\n\nChosen per hierarchy level: Halo column, Beacon.";

        private const string EffectAmplitudeHelp = "How much the layer grows and shrinks per cycle: 0 = not at all, 0.4 = 40 percent swing.";
        private const string EffectStaggerHelp = "Delay between the inner, middle and outer wave, in cycle units (0 = all three move together, 0.25 = widely spaced).";
        private const string EffectInnerAlphaHelp = "Starting brightness of the inner wave (0 to 1). It fades to 0 as it grows. Usually the strongest of the three.";
        private const string EffectMiddleAlphaHelp = "Starting brightness of the middle wave (0 to 1). Usually weaker than the inner one.";
        private const string EffectOuterAlphaHelp = "Starting brightness of the outer wave (0 to 1). Usually the faintest.";
        private const string EffectSizeHelp = "Diameter of the layer as a MULTIPLE of the symbol diameter: 1 = same size as the symbol, 1.2 = 20 percent bigger. It follows the marker size, so the effect looks the same on a 7 cm and a 30 cm marker.";
        private const string EffectBaseAlphaHelp = "Brightness of the layer (0 = invisible, 1 = solid).";
        private const string EffectOuterScaleHelp = "Outer radius of the ring (0.72 to 0.98). Bigger = the ring reaches further out.";
        private const string EffectInnerScaleHelp = "Inner radius of the ring (0.5 to 0.9). The gap between inner and outer scale is the ring thickness.";
        private const string EffectRadiusScaleHelp = "How much of the disc is filled (0.85 to 1).";
        private const string EffectStartScaleHelp = "Size of the wave when it starts, relative to the symbol (1 = same size).";
        private const string EffectEndScaleHelp = "Size of the wave when it has faded out, relative to the symbol. Bigger = the wave travels further.";

        private const string HierarchyEffectColumnsHelp =
            "Ripple, Halo and Pulse choose which effects the POIs of this level run. How each effect looks is defined in Global Scene > Effects; only effects ticked there are offered here (a value pointing at a disabled effect shows '(disabled)' and does nothing until it is ticked again).\n\nRipple = three waves flowing outward (Rings or Discs). Halo = one layer behind the symbol (Ring, Disc or Beacon). Pulse = the symbol breathes. They stack: one Ripple + one Halo + Pulse can all run on the same marker.\n\nRotate (ring spin) and the reveal delay / duration are also set per level, in this table.";

        private const string DisabledEffectNote =
            "Switched off: not offered in the Hierarchy Levels table. Its settings are kept and still saved.";

        private const string EffectPreviewHelp =
            "Play Mode and development builds only (release builds ignore it). When ticked, pressing Play FOCUSES the screen on a labelled grid of real markers: one per effect, plus one per hierarchy level with its real size, effects and reveal timing. The grid lives far away from the wall and is drawn by its own camera on a neutral background, so no wall or scenery can hide it, and your real camera, tracking and markers are untouched. Untick it to see the wall again.\n\nPlay reads the SAVED config: after ticking, press Save All to JSON and Copy to StreamingAssets, then Play. Off by default.";
        private const string EffectPreviewBaseHelp =
            "Which marker the preview cells copy: a plain grey circle, or any POI of this wall (its category colour, icon, status and badge). Use a real POI to check that effect tints stay visible against its colour.";

        // ---------------- test guides ----------------

        // Same shape as the Orientation guides: one collapsible foldout per test area, terse ASCII
        // bullets, one block per part of the domain. Edit Mode never runs Update or coroutines, so
        // the Scene guide says so and only lists what can really be checked there.
        private static readonly string EffectsSceneTestGuide =
            "SETUP\n" +
            "- Top bar 'Load & Populate Rig'. Scene view: RMB + mouse = look, RMB + WASD = fly, Alt + LMB = orbit.\n" +
            "- Edit Mode does not run Update or coroutines: nothing animates in the Scene view.\n\n" +
            "MASTER SWITCH\n" +
            "- Not possible in Scene test: it only stops animation. Use Play Mode.\n\n" +
            "PULSE\n" +
            "- Not possible in Scene test: the scale breathing needs frames. Use Play Mode.\n\n" +
            "RIPPLE (Ripple Rings, Ripple Discs)\n" +
            "- Not possible in Scene test: the waves are animated. Use Play Mode.\n\n" +
            "HALO (Halo Ring, Halo Disc, Beacon)\n" +
            "- Not possible in Scene test: the halo is animated. Use Play Mode.\n\n" +
            "REVEAL AND ROTATE\n" +
            "- Not possible in Scene test: markers appear at full size at once and the ring does not spin.\n\n" +
            "WHAT YOU CAN CHECK HERE\n" +
            "- Hierarchy Levels > Size (cm): 'lamp' (level_1, 30 cm) is clearly bigger than 'lamp_economic' (level_5, 7 cm).\n" +
            "- Effect columns persist: set a level's Ripple to Discs, 'Save All to JSON', 'Load & Populate Rig', the dropdown keeps it. Ctrl+Z / Ctrl+Y undo and redo it.\n" +
            "- Untick an effect above: it disappears from the Ripple / Halo dropdowns (a level still using it shows '(disabled)').";

        private static readonly string EffectsPlaymodeTestGuide =
            "SETUP\n" +
            "- Fastest: tick 'Focus on Effects Grid' (Test, above), Save All to JSON, Copy to StreamingAssets, then Play. The screen shows a labelled grid of markers on a neutral background, each with its effect or level name; the wall cannot hide it. The real markers are hidden while it is on. Untick it when done.\n" +
            "- LIVE: once Play is running, change any value on this Effects page (or a level's Ripple / Halo / Pulse column) and the running markers and the grid update at once. Nothing is saved by that: stop Play and your edits stay in this window; press Save All to JSON (then Copy to StreamingAssets) only when you want to keep them.\n" +
            "- Real markers instead: 'Save All to JSON' then 'Copy to StreamingAssets' (Play reads the copy), open Apps/LivingRoom/LivingRoomScene, press Play, click into the Game view.\n" +
            "- Mock camera (the project's MockLocalizationProvider, Editor only): W/A/S/D = move, E = up, Q = down, RMB + mouse or arrow keys = look.\n\n" +
            "MASTER SWITCH\n" +
            "- 'Enable effects' ON: the preview cells pulse, ripple and glow.\n" +
            "- OFF: every cell goes static at once (live in Play Mode). The reveal fade-in still plays.\n\n" +
            "PULSE\n" +
            "- Amplitude = size of the swing, Period (s) = speed. Try Amplitude 0.45 / Period 0.5 (strong and fast) against 0.05 / 3 (barely visible).\n" +
            "- Untick Pulse: the Pulse cell goes still and Pulse disappears from the Hierarchy Levels table.\n\n" +
            "RIPPLE (Ripple Rings, Ripple Discs)\n" +
            "- Ripple Rings: three thin rings flowing outward. Ripple Discs: three filled discs. Each has its OWN settings.\n" +
            "- Period, Stagger, Inner / Middle / Outer alpha and Tint change the look live. Stagger 0 = all three waves move together.\n\n" +
            "HALO (Halo Ring, Halo Disc, Beacon)\n" +
            "- Halo Ring: thin ring that breathes. Halo Disc: filled glow that breathes. Beacon: one ring that grows and fades, then restarts.\n" +
            "- Size, Base alpha, scales, Breathe amplitude, Start / End scale and Tint Color: change one at a time and compare. Each effect has its own values.\n" +
            "- One halo per marker. Ripple and Pulse stack with it.\n\n" +
            "REVEAL AND ROTATE\n" +
            "- Reveal Delay / Duration in Hierarchy Levels: the level row of the preview appears in order (level_1 first, then 0.2 s, 0.5 s, 0.8 s, 1.0 s in LivingRoom), each fading and scaling in.\n" +
            "- Rotate ticked: the status ring spins. Unticked: static ring.\n\n" +
            "REAL MARKERS\n" +
            "- 'lamp' (level_1) shows Ripple Discs + Halo Ring + Pulse; 'lamp_military' (level_3) Ripple Rings + Halo Disc; 'lamp_economic' (level_5) Beacon.\n" +
            "- Assets/Dev/MarkerGallery/MarkerGalleryScene: Play, every effect variant in one grid.\n" +
            "- Automated: Test Runner, EditMode + PlayMode, zero failures.";

        private static readonly string EffectsDeviceTestGuide =
            "SETUP (Android)\n" +
            "- USB: enable USB debugging, plug in, accept the prompt, 'adb devices' lists it.\n" +
            "- Wi-Fi: Developer options > Wireless debugging > Pair with code.\n" +
            "  - 'adb pair <ip>:<pair-port>' + code, then 'adb connect <ip>:<port>'.\n" +
            "  - Or 'npx adb-qr-connect' and scan the QR code.\n" +
            "- Tick 'Focus on Effects Grid' to get the grid in a DEVELOPMENT build (Build Settings > Development Build). Release builds ignore it.\n" +
            "- 'Save All to JSON' + 'Copy to StreamingAssets', then Build Settings > Android > Build And Run.\n\n" +
            "LOGS\n" +
            "- 'adb logcat -c', then 'adb logcat -s Unity > __logcat.txt' and read the file.\n" +
            "- Look for [Config] loaded N hierarchy levels and any Exception / Error.\n\n" +
            "MASTER SWITCH\n" +
            "- The preview grid is the worst case (every effect at once). Run 5 min with Enable effects ON, then OFF: compare frame rate, heat and battery. Keep OFF if the device struggles.\n\n" +
            "PULSE\n" +
            "- Pulse on every marker at once can read as noise at real distance. Check that only the levels you want draw the eye.\n\n" +
            "RIPPLE (Ripple Rings, Ripple Discs)\n" +
            "- Outdoors in bright light: is the Tint Color still visible against the wall? Discs are stronger than Rings.\n\n" +
            "HALO (Halo Ring, Halo Disc, Beacon)\n" +
            "- Check Beacon on the smallest level at arm's length: is the ring readable, not a blur?\n\n" +
            "REVEAL AND ROTATE\n" +
            "- Point at the wall from a real distance: does the staggered fade-in feel calm or slow? Adjust Reveal Delay / Duration.\n" +
            "- Rotate: a spinning ring on many markers costs battery, tick it only where it matters.";
    }
}
