// POIEditorToolWindow.LabelsAndFontsHelp.cs
//
// Partial: every constant of the "Labels, Text & Fonts" domain (_2.0_Labels_And_Fonts_Design.md):
// slider ranges, option arrays, (i) help texts and the three Test guides. ASCII only, app-agnostic
// (no real wall's POI ids or category names -- _5.1_Editor_Tab.md, "Domain Manual Tests"), and
// every help/guide text below names the Editor Tab control a developer would actually use to fix or
// check something, never a project doc or code file (_5.1_Editor_Tab.md, "Messages, dialogs and
// popups" / "Domain Manual Tests").

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // Slider ranges are NOT here: they live once on MarkerVisualSettings (LabelGapRatioMin/Max,
        // LabelFontSizeRatioMin/Max), shared by the runtime clamp and every label slider in the window.

        // The framework default FontLibrary.asset's 3 keys. GetAvailableFontKeyOptions
        // (AssetPaths/POIEditorToolWindow.AssetPaths.cs) unions these with any extra keys the wall's
        // own font library defines, so a wall that added a 4th font can actually select it in the
        // popup instead of only being reachable by hand-editing config.json.
        private static readonly string[] LabelFontKeyOptions = { "liberation_sans", "roboto_bold", "oswald_bold" };
        private static readonly string[] LabelFontKeyLabels = { "Liberation Sans (default)", "Roboto Bold", "Oswald Bold" };

        // ---------------- (i) help ----------------

        private const string LabelGapRatioHelp =
            "Distance from the bottom of the marker Symbol to the top of its text label, as a multiple of the Symbol's own diameter (not a fixed distance) -- so one number gives a correctly proportioned gap on big and small markers alike. Set here for the whole wall; a hierarchy level can use its own value through the 'Aa' (Marker Label Style) button in its Hierarchy Levels row.";
        private const string LabelFontSizeRatioHelp =
            "Text label font size, as a multiple of the marker Symbol's own diameter (not a fixed point size) -- so the label reads at a correctly proportioned size on every hierarchy level, from your smallest to your largest. Set here for the whole wall; a hierarchy level can use its own value (e.g. keeping one level's text large for accessibility even though its marker is small) through the 'Aa' (Marker Label Style) button in its Hierarchy Levels row.";
        private const string LabelFontKeyHelp =
            "Which font the label text uses. Three fonts ship with the framework; any font you add with 'Add font' (just below) is listed here too, and in every other Font popup of this window.";
        private const string AddFontHelp =
            "Drop a TMP Font Asset here (or click the small circle to pick one) to add it to this wall's own font list and select it at once. To turn a .ttf/.otf file into a TMP Font Asset first, use Window > TextMeshPro > Font Asset Creator. The first font you add creates the wall's font list next to its other marker assets; adding the same font twice reuses the entry.";
        private const string WallFontLibraryPresentHelp =
            "This wall has its own font list (it starts with the 3 framework fonts). 'Select Font List' shows it in the Project window so you can rename or remove entries in the Inspector. 'Stop Using Wall Fonts' goes back to the 3 framework fonts only; it does not delete the list, so adding a font again finds it.";
        private const string LevelLabelStyleOverrideHelp =
            "Off: this hierarchy level's labels follow the wall default set in Global Scene > Labels, Text & Fonts, and the rows below only show those values (a change there reaches this level at once). On: this level uses its own Gap, Font size and Font, starting from the current wall default; changing them here never changes the wall default or any other level. Use it when one level genuinely needs a different label, e.g. larger text on small markers for accessibility.";
        private const string LabelsPreviewHelp =
            "Shows a grid of real markers on their own camera far from the wall; its last row has one marker per hierarchy level, labelled with the Base marker's own name exactly as a real POI at that level would be -- the fastest way to see a Gap/Font size/Font change on every level at once, live, in Play Mode. The same switch as Effects > Test's 'Add effects demo grid' and Hierarchy Levels > Test's 'Add Hierarchy demo grid': turning it on here also turns it on there. Editor and development builds only -- turn it off before building for a normal device demo.";

        // ---------------- Test guides ----------------

        private static readonly string LabelsAndFontsSceneTestGuide =
            "SETUP\n" +
            "- Top bar 'Load & Populate Rig'. The rig refreshes on every change here, so the Scene view always shows the current label look (static: nothing animates in Edit Mode).\n\n" +
            "GAP / FONT SIZE\n" +
            "- Change either slider and watch every marker's label move/resize in the Scene view together (proportional to each marker's own size -- a small marker's label should look correctly scaled, not identical in absolute size to a big marker's).\n\n" +
            "FONT\n" +
            "- Switching the Font popup changes every label's typeface at once in the Scene view. A font added with 'Add font' previews here too.\n\n" +
            "PER-LEVEL STYLE\n" +
            "- Hierarchy Levels table > a level's 'Aa' button > tick 'Override global label config' and move its sliders: only that level's markers change in the Scene view. Untick it: they go back to the wall default.\n" +
            "- With the override off, move a slider here: the level follows it. With it on, the level ignores it.";

        private static readonly string LabelsAndFontsPlaymodeTestGuide =
            "SETUP\n" +
            "- 'Save All to JSON' then 'Copy to StreamingAssets' (Play reads the copy), open the wall's scene, press Play, click into the Game view.\n" +
            "- LIVE: while Play runs, edits to Gap / Font size / Font here reach every real marker at once, and a hierarchy level's Marker Label Style (its 'Aa' button) reaches that level's markers at once too -- drag the Marker Label Style window clear of the Game view to see both side by side.\n" +
            "- Tick 'Add Labels demo grid' below (in this section's Test foldout) to see one real marker per hierarchy level, labelled with the Base marker's name, on its own camera -- useful for tuning Gap/Font size/Font before you have placed a single real POI.\n\n" +
            "CHECK\n" +
            "- Ticking/unticking 'Override global label config' in a level's Marker Label Style window should visibly switch that level's markers (and its demo grid cell) between the wall default and its own style, live.";

        private static readonly string LabelsAndFontsDeviceTestGuide =
            "SETUP (Android)\n" +
            "- USB: enable USB debugging, plug in, accept the prompt, 'adb devices' lists it.\n" +
            "- Wi-Fi: Developer options > Wireless debugging > Pair with code; 'adb pair <ip>:<pair-port>' + code, then 'adb connect <ip>:<port>'; or 'npx adb-qr-connect'.\n" +
            "- Untick 'Add Labels demo grid' first if it was left on -- it is Editor and development builds only. 'Save All to JSON' + 'Copy to StreamingAssets', then Build Settings > Android > Build And Run.\n\n" +
            "CHECK\n" +
            "- Labels read clearly at real device screen density and real viewing distance from the wall -- this is the one thing Scene/Playmode testing on a monitor cannot confirm.";
    }
}
