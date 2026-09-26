// POIEditorToolWindow.MarkerDesignHelp.cs
//
// Partial: every constant of the Marker, Badge and Outline sections of Global Scene (_2.2.1,
// _2.2.2, _2.2.3): option arrays, (i) help texts and the three Test guides per domain. ASCII only.
// The guide contract test (MarkerDesignAuthoringTests) reads these by reflection.

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private static readonly string[] BadgeCornerOptions = { "top_right", "top_left", "bottom_right", "bottom_left" };
        private static readonly string[] BadgeCornerLabels = { "Top Right", "Top Left", "Bottom Right", "Bottom Left" };

        // ---------------- (i) help ----------------

        private const string SymbolColumnHelp =
            "SYMBOL (ObjectField cell): lists every Sprite in the whole project. CLICK THE THUMBNAIL PREVIEW: opens the curated picker narrowed to just this wall's symbols plus the framework defaults. All cells write to the same field.\n\n" +
            "TO ADD YOUR OWN IMAGE: drop a PNG anywhere under this wall's Assets/Apps/<Wall>/MarkerAssets/ folder -- Unity auto-imports it as a Sprite. Then pick it from either list; it registers into this wall's icon library automatically. Same flow for badges and outline ring art.";

        private const string MarkerShapeHelp =
            "The silhouette behind every marker's icon: circle, rounded square, hexagon, diamond or star. None draws no backdrop: the icon floats on the wall and only the outline ring and the badge remain. One shape for the whole wall (a per-POI shape is deliberately not offered: a consistent shape is what makes a marker read as 'a point of interest').";
        private const string IconColorHelp =
            "The colour of the icon drawn on every marker symbol and badge. Pick a colour that contrasts with your category colours (a light icon on dark backgrounds, a dark icon on light ones). With Background shape None the icon sits directly on the wall, so check it against the real wall too.";
        private const string IconSizeHelp =
            "The symbol icon's diameter as a fraction of the symbol (0.35 to 0.90). Larger reads better from far away; smaller leaves more of the category colour visible around it. With Background shape None a larger icon usually works best.";
        private const string BadgeEnableHelp =
            "A badge is a small second symbol on the marker (for example the damage state). Off: no marker shows a badge and the Badge Style row of each POI is hidden. Unknown-status POIs always show the grey '?' badge whenever the wall has outline levels.";
        private const string BadgeShapeHelp =
            "The silhouette behind the badge icon. Independent of the marker's Background shape: a hexagon marker can carry a round badge. None draws the badge icon alone.";
        private const string BadgeCornerHelp =
            "Which corner of the symbol the badge sits on. The badge overlaps the symbol edge; top right is the default.";
        private const string BadgeSizeHelp =
            "Badge diameter as a fraction of the symbol diameter (0.20 to 0.50). Larger reads better at a distance but hides more of the symbol.";
        private const string OutlineEnableHelp =
            "The outline ring shows a POI's condition (its status level). Off: no ring on any marker and the level table is ignored at runtime.";
        private const string OutlineModeHelp =
            "Uniform color: every level shares the one colour set below (defaults to the framework's gold), so only the dash pattern changes between levels. Same hue as marker: the ring and the symbol fill take the marker's own category colour and darken as the condition worsens, so a marker's identity never changes with damage. Per outline type: each row of the table below carries its own colour.";
        private const string OutlineUniformColorHelp =
            "The one ring colour used by every outline level while Outline Color is Uniform color. Ignored in the other two modes.";
        private const string RingSizeHelp =
            "Ring diameter as a multiple of the symbol diameter (1.00 to 1.35). 1.00 hugs the symbol, larger leaves a visible gap between symbol and ring.";
        private const string RingSpinHelp =
            "How fast the ring turns, in degrees per second (0 to 360), on the hierarchy levels that tick Spin Ring (Hierarchy Levels table). A slow spin (30 to 60) reads as 'scanning'; fast spins cost attention and battery.";
        private const string OutlineTypesHelp =
            "Each row is one outline type a POI can be set to (a framework default suggestion: intact, partial damage, destroyed, unknown). A POI picks its row in Specific Marker > POI > Outline > Status level, and its ring draws that row's Outline Style (and, in Per outline type mode, its Color). Percentages are spaced evenly when a row is added or deleted.\n\nTo add your own line (e.g. a wavy or double ring): import a transparent PNG ring as a Sprite under this wall's MarkerAssets folder, then pick it in the Outline Style column the same way as a marker or badge symbol.";
        private const string OutlinePreviewHelp =
            "Developer-only: Play Mode and development builds (release builds ignore it). When ticked, pressing Play FOCUSES the screen on a labelled grid of real markers: one 'No outline' cell, then one cell per row of the Outline Types table with its real colour, dash pattern and spin, in the wall's current Outline Color mode. The grid lives far away from the wall on its own camera and neutral background, so no wall or scenery can hide it; your real camera, tracking and markers are untouched. While Play runs, edits to this section redraw it at once.\n\nOnly ONE demo grid can be on screen at a time: ticking this unticks Effects > Test > 'Add effects demo grid' (the same switch as Hierarchy Levels > Test > 'Add Hierarchy demo grid').\n\nPlay reads the SAVED config: after ticking, press Save All to JSON and Copy to StreamingAssets, then Play. Off by default; untick it before a device build.";
        private const string OutlinePreviewBaseHelp =
            "Which marker every grid cell copies: a plain grey circle (rings judged against a neutral disc), or one of this wall's POIs (its category colour, icon and badge; needed to judge Same hue as marker).";

        // ---------------- per-POI (Specific Marker tab) ----------------

        private const string PoiCategoryHelp =
            "Which row of Global Scene > Marker > Category Symbols this POI uses: its symbol colour and icon. A value that no longer matches any row shows as '(missing)' and keeps rendering with an automatic colour until you pick a real category.";
        private const string PoiHierarchyLevelHelp =
            "Which row of Global Scene > Hierarchy Levels this POI uses: its marker size, text label, effects, ring spin, reveal timing and facing override. '(none)' uses the framework fallback (12 cm, no label, no effects). The line below shows the effects this level gives the POI.";
        private const string PoiCustomSymbolHelp =
            "Replaces only this POI's icon with a symbol of your choice (drag a sprite into the field, or click the thumbnail to pick from this wall's and the framework's icons). The category colour, the outline ring and the badge stay unchanged.";
        private const string PoiBadgeCategoryHelp =
            "Which row of Global Scene > Badge this POI's badge shows. '(none)': a POI with a known status still shows a badge, coloured by its status. A POI whose status is unknown shows the unknown badge instead (Status unknown selects the wall's unknown badge row; without one, a grey '?').";
        private const string PoiHasStatusHelp =
            "Whether this POI has a condition at all. Off: no outline ring and no badge on this marker. On: pick its Status level below.";
        private const string PoiStatusLevelHelp =
            "Which row of Global Scene > Outline > Outline Types this POI's ring uses (its dash pattern and, in Per outline type mode, its colour).";
        private const string PoiStatusPctHelp =
            "The condition as a percentage, used while Global Scene > Outline has no Outline Types rows yet: the ring picks the nearest step of the framework's default ramp.";
        private const string PoiStatusUnknownHelp =
            "The condition of this POI is genuinely unknown. The marker shows the grey '?' badge and, when the wall has an 'unknown' outline type, its dotted ring. Ticking it selects that outline type and badge for you when they exist.";

        // ---------------- Marker guides ----------------
        // Framework behaviour only: every step names an Editor Tab control, never a wall's own POIs,
        // a project doc or a code file (_5.1_Editor_Tab.md, "Domain Manual Tests").

        private static readonly string MarkerSceneTestGuide =
            "SETUP\n" +
            "- Top bar 'Load & Populate Rig'. Scene view: RMB + mouse = look, RMB + WASD = fly, Alt + LMB = orbit. Q/W/E/R are Unity's own tool hotkeys there.\n" +
            "- The rig refreshes on every change here, so the Scene view always shows the current marker look (static: nothing animates in Edit Mode).\n\n" +
            "BACKGROUND SHAPE, ICON COLOR, ICON SIZE\n" +
            "- Change the shape: every marker in the rig changes at once. None removes the coloured backdrop and keeps the icon.\n" +
            "- Icon color recolours the icon of every marker and badge; Icon size grows or shrinks every symbol icon.\n" +
            "- Ctrl+Z / Ctrl+Y undo and redo each change.\n\n" +
            "CATEGORY SYMBOLS TABLE\n" +
            "- Change one category's Color or Symbol (drag a sprite, or click the thumbnail to pick one): every POI using that category changes, no other POI does.\n" +
            "- Rename a category: the POIs that used it follow. Delete a category still in use: a confirmation says how many POIs are affected.\n\n" +
            "PER-POI (Specific Marker > POI > Marker Style)\n" +
            "- Category recolours that one marker. Use Custom Symbol: its own icon shows, category colour unchanged.\n" +
            "- A POI with Hierarchy Level '(none)' renders at the 12 cm framework fallback, with no label.\n" +
            "- A value shown as '(missing)' no longer matches any table row: pick a real one.\n\n" +
            "NOT POSSIBLE IN SCENE TEST\n" +
            "- Reveal animation, tapping a marker, legibility at real distance. Use Play Mode or a device.";

        private static readonly string MarkerPlaymodeTestGuide =
            "SETUP\n" +
            "- 'Save All to JSON' then 'Copy to StreamingAssets' (Play reads the copy), open your wall's scene, press Play, click into the Game view.\n" +
            "- Editor mock camera (Editor only): W/A/S/D = move, E = up, Q = down, RMB + mouse or arrow keys = look, Z/C = roll.\n" +
            "- LIVE: while Play runs, every field of this section, the Category table and each POI's Name, Category, Hierarchy Level and custom symbol show on the running markers at once. Nothing is saved by that; press Save All to JSON (then Copy) to keep them.\n\n" +
            "BACKGROUND SHAPE, ICON COLOR, ICON SIZE\n" +
            "- Switch through all six shapes while Play runs: every marker follows and the ring stays round. None: icon only.\n" +
            "- Drag Icon color and Icon size: every running marker follows.\n\n" +
            "CATEGORY SYMBOLS TABLE\n" +
            "- Edit one category's Color: every POI using that category recolours live (cluster aggregates are not part of the live update).\n\n" +
            "PER-POI\n" +
            "- Give one POI a different Category or a custom symbol, or rename it: only that marker changes.\n" +
            "- A POI with Hierarchy Level '(none)' is smaller than one that has a level (the 12 cm framework fallback) and shows no label.\n\n" +
            "AUTOMATED\n" +
            "- Test Runner, EditMode + PlayMode, zero failures.";

        private static readonly string MarkerDeviceTestGuide =
            "SETUP (Android)\n" +
            "- USB: enable USB debugging, plug in, accept the prompt, 'adb devices' lists it.\n" +
            "- Wi-Fi: Developer options > Wireless debugging > Pair with code; 'adb pair <ip>:<pair-port>' + code, then 'adb connect <ip>:<port>'; or 'npx adb-qr-connect'.\n" +
            "- 'Save All to JSON' + 'Copy to StreamingAssets', then Build Settings > Android > Build And Run. No mock camera on device.\n\n" +
            "LOGS\n" +
            "- 'adb logcat -c', then 'adb logcat -s Unity > __logcat.txt' and read the file (no piping).\n" +
            "- Look for [WallSession] Ready N/N POIs and any marker_shape warning.\n\n" +
            "WHAT ONLY A DEVICE SHOWS\n" +
            "- Shape and icon legibility on the real screen, outdoors in bright light: is your smallest configured level still recognisable at arm's length? If not, raise Icon size.\n" +
            "- Colour contrast of each category and of the Icon color against the real wall, not the grey Editor background.\n" +
            "- Tap the smallest marker with a real finger: the selection must hit reliably.";

        // ---------------- Badge guides ----------------

        private static readonly string BadgeSceneTestGuide =
            "SETUP\n" +
            "- Top bar 'Load & Populate Rig', then use a POI's crosshair (Specific Marker tab) to frame a POI that has a status and a badge category. The rig refreshes on every change (static view, nothing animates).\n\n" +
            "ENABLE BADGE\n" +
            "- Untick: every badge disappears (except the grey '?' of unknown-status POIs) and the Badge Style foldout of each POI hides. Tick again: they return (values are kept).\n\n" +
            "BADGE BACK SHAPE, CORNER, SIZE\n" +
            "- Back shape: change it, only the badge backdrop changes, the marker shape does not. None = icon alone.\n" +
            "- Corner: the badge jumps to the chosen corner of the symbol.\n" +
            "- Size: drag 0.20 to 0.50; the badge grows against a fixed symbol.\n\n" +
            "BADGE TABLE\n" +
            "- Change the Color or Symbol of one badge key: every POI using that badge key changes together.\n" +
            "- Rename a key: the POIs follow. Delete a key in use: a confirmation with the POI count.\n\n" +
            "PER-POI (Specific Marker > POI > Badge Style)\n" +
            "- Badge category '(none)' on a POI with a known status: the badge stays, coloured by the status (its outline type's colour).\n" +
            "- Has status off (Outline foldout): no badge at all.\n" +
            "- Tick Status unknown (Outline foldout): the badge switches to the wall's unknown badge row (a grey '?' when there is none).\n\n" +
            "NOT POSSIBLE IN SCENE TEST\n" +
            "- Nothing animated is involved; a real device only adds legibility and touch checks.";

        private static readonly string BadgePlaymodeTestGuide =
            "SETUP\n" +
            "- 'Save All to JSON' then 'Copy to StreamingAssets', open your wall's scene, press Play, click into the Game view. Editor mock camera: W/A/S/D move, E up, Q down, RMB + mouse look.\n" +
            "- LIVE: while Play runs, Enable badge, Back shape, Corner, Size, the Badge table and each POI's Badge category apply to the running markers at once. Nothing is saved until Save All to JSON (then Copy).\n\n" +
            "ENABLE BADGE\n" +
            "- Untick while Play runs: all badges vanish except the '?' of unknown-status POIs.\n\n" +
            "SHAPE, CORNER, SIZE\n" +
            "- Cycle the corners: the badge moves around the symbol without leaving it. Size 0.50 on your smallest configured marker: pick the value that still leaves the icon readable.\n\n" +
            "TABLE AND PER-POI\n" +
            "- Edit a badge Color live and compare two POIs at different status levels that use that badge.\n" +
            "- Set one POI's Badge category to '(none)': its badge takes its status colour instead.\n" +
            "- Automated: Test Runner, zero failures.";

        private static readonly string BadgeDeviceTestGuide =
            "SETUP (Android)\n" +
            "- Same as the Marker device guide: USB or Wi-Fi debugging, 'Save All to JSON' + 'Copy to StreamingAssets', Build And Run.\n" +
            "- Logs: 'adb logcat -c', then 'adb logcat -s Unity > __logcat.txt'.\n\n" +
            "WHAT ONLY A DEVICE SHOWS\n" +
            "- Is the badge icon readable at your configured Size on your smallest marker held at arm's length? If not, raise Size or move the badge to the corner with the most free space.\n" +
            "- Do your badge colours stay distinguishable from each other in sunlight?\n" +
            "- Is the grey '?' clearly different from a real badge?";

        // ---------------- Outline guides ----------------

        private static readonly string OutlineSceneTestGuide =
            "SETUP\n" +
            "- Top bar 'Load & Populate Rig'. Frame two POIs at different status levels with their crosshairs (Specific Marker tab) and compare their ring dash patterns. Static view: the ring does not spin in Edit Mode.\n\n" +
            "ENABLE OUTLINE\n" +
            "- Untick: every ring disappears. Tick again: they return with all types kept.\n\n" +
            "OUTLINE COLOR\n" +
            "- Uniform color: every ring shares the Outline color swatch below the dropdown -- change it and every type follows. Same hue as marker: rings and symbol fills take the marker's category colour and darken with the damage. Per outline type: each row of the table picks its own colour.\n\n" +
            "RING SIZE\n" +
            "- Drag 1.00 to 1.35: the ring moves away from the symbol. 1.00 touches it.\n\n" +
            "OUTLINE TYPES TABLE\n" +
            "- Change a row's Outline Style (drag a sprite or click the thumbnail): every marker on that type changes its dash pattern.\n" +
            "- Color: only shown in Per outline type mode. Type a hex on one row and its rings recolour; clear it to return to the ramp colour.\n" +
            "- '+ Add outline level' respaces the percentages; deleting a type in use asks first. POIs keep their chosen type.\n\n" +
            "PER-POI (Specific Marker > POI > Outline)\n" +
            "- Has status off: that POI has no ring and no badge. Status level picks the ring; Status unknown gives the dotted grey ring and the '?' badge.\n\n" +
            "NOT POSSIBLE IN SCENE TEST\n" +
            "- Contour spin: it needs frames. Use Play Mode or the demo grid above (Play Mode only).";

        private static readonly string OutlinePlaymodeTestGuide =
            "SETUP\n" +
            "- Fastest: tick 'Add outline demo grid' (above), Save All to JSON, Copy to StreamingAssets, Play: one 'No outline' cell and one cell per outline type, with real colours and spin. Untick it to see the wall again.\n" +
            "- Real markers: 'Save All to JSON' then 'Copy to StreamingAssets', open your wall's scene, press Play, click into the Game view. Editor mock camera: W/A/S/D move, E up, Q down, RMB + mouse look.\n" +
            "- LIVE: while Play runs, every field of this section, the Outline Types table and each POI's status apply to the running markers and the grid at once. Nothing is saved until Save All to JSON (then Copy).\n\n" +
            "OUTLINE COLOR AND RING SIZE\n" +
            "- Switch Uniform color / Same hue as marker while watching two POIs of the same category at different status levels: Same hue keeps their category colour at every level; Uniform gives both the same ring colour.\n" +
            "- In Uniform color mode, drag the Outline color swatch: every ring on the wall recolours at once.\n" +
            "- Ring size 1.35: a clear gap; 1.00: touching.\n\n" +
            "CONTOUR SPIN\n" +
            "- Every hierarchy level with Spin Ring ticked (Hierarchy Levels table) spins its POIs' rings. Contour spin 0: they stop and sit upright. 360: one turn per second.\n" +
            "- Untick Spin Ring for one level: only that level's rings stop.\n\n" +
            "OUTLINE TYPES TABLE\n" +
            "- Switch to Per outline type, then change one row's Outline Style or Color: every POI of that type follows.\n\n" +
            "PER-POI\n" +
            "- Toggle Has status on any POI: its ring and badge vanish and return.\n" +
            "- Automated: Test Runner, zero failures.";

        private static readonly string OutlineDeviceTestGuide =
            "SETUP (Android)\n" +
            "- Same as the Marker device guide: USB or Wi-Fi debugging, 'Save All to JSON' + 'Copy to StreamingAssets', Build And Run.\n" +
            "- Untick 'Add outline demo grid' first: a development build would otherwise show the grid instead of the wall.\n" +
            "- Logs: 'adb logcat -c', then 'adb logcat -s Unity > __logcat.txt'; look for marker_outline_mode warnings.\n\n" +
            "WHAT ONLY A DEVICE SHOWS\n" +
            "- Can you tell your outline types' dash patterns apart on the real screen at real distance? If two look alike, change their Outline Style.\n" +
            "- Spinning rings on many markers at once: check frame rate, heat and battery over 5 minutes; lower Contour spin or untick Spin Ring on the smaller levels.\n" +
            "- Same hue against the real wall: does a dark ring still separate from a dark wall?";
    }
}
