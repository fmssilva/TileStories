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
            "How fast the ring turns, in degrees per second (0 to 360), for the hierarchy levels that tick Rotate. A slow spin (30 to 60) reads as 'scanning'; fast spins cost attention and battery.";
        private const string OutlinePreviewHelp =
            "Play Mode and development builds only (release builds ignore it). When ticked, pressing Play FOCUSES the screen on a labelled grid of real markers: one cell per outline mode (Uniform color, Same hue as marker, Per outline type) for comparison, plus one cell per configured outline level with its real colour, dash pattern and spin. The grid lives far away from the wall on its own camera and neutral background, so no wall or scenery can hide it, and your real camera, tracking and markers are untouched. Untick it to see the wall again.\n\nPlay reads the SAVED config: after ticking, press Save All to JSON and Copy to StreamingAssets, then Play. Off by default.";
        private const string OutlinePreviewBaseHelp =
            "Which POI's category the Same hue as marker comparison cell borrows. Empty = plain grey circle.";

        // ---------------- Marker guides ----------------

        private static readonly string MarkerSceneTestGuide =
            "SETUP\n" +
            "- Top bar 'Load & Populate Rig'. Scene view: RMB + mouse = look, RMB + WASD = fly, Alt + LMB = orbit. Q/W/E/R are Unity's own tool hotkeys there.\n" +
            "- The rig refreshes on every change here, so the Scene view always shows the current marker look (static: nothing animates in Edit Mode).\n\n" +
            "BACKGROUND SHAPE\n" +
            "- Change the shape: every marker in the rig changes at once. None removes the coloured backdrop and keeps the icon.\n" +
            "- Ctrl+Z / Ctrl+Y undo and redo it.\n\n" +
            "CATEGORY SYMBOLS TABLE\n" +
            "- Change the Color hex or the Symbol of 'religious': 'lamp_religious', 'painting_religious' and 'camera_religious' change, no other marker does.\n" +
            "- Rename a category: the POIs that used it follow. Delete a category still in use: a confirmation says how many POIs are affected.\n\n" +
            "PER-POI (Specific Marker > POI > Marker Style)\n" +
            "- Category dropdown recolours that one marker. Use Custom Symbol: 'dev_marker_custom_symbol' shows its own icon, category colour unchanged.\n" +
            "- 'dev_marker_nolevel' has no Hierarchy Level: it is the 12 cm fallback with no label.\n\n" +
            "NOT POSSIBLE IN SCENE TEST\n" +
            "- Reveal animation, tapping a marker, legibility at real distance. Use Play Mode or a device.";

        private static readonly string MarkerPlaymodeTestGuide =
            "SETUP\n" +
            "- 'Save All to JSON' then 'Copy to StreamingAssets' (Play reads the copy), open Apps/LivingRoom/LivingRoomScene, press Play, click into the Game view.\n" +
            "- Mock camera (MockLocalizationProvider, Editor only): W/A/S/D = move, E = up, Q = down, RMB + mouse or arrow keys = look, Z/C = roll.\n" +
            "- LIVE: while Play runs, edits to Background shape, the Category table and each POI's Category / Use Custom Symbol show on the running markers at once. Nothing is saved by that; press Save All to JSON (then Copy) to keep them. Marker SIZE (Hierarchy Levels) still needs Save + Copy + Play again.\n\n" +
            "BACKGROUND SHAPE\n" +
            "- Switch through all six while Play runs: every marker follows and the ring stays round. None: icon only.\n\n" +
            "CATEGORY SYMBOLS TABLE\n" +
            "- Edit the Color of 'military': the military markers recolour live (cluster aggregates are not part of the live update).\n\n" +
            "PER-POI\n" +
            "- Give 'lamp' a different Category or a custom symbol and watch only that marker change.\n" +
            "- 'dev_marker_nolevel' is smaller than 'lamp' (12 cm against 30 cm) and shows no label.\n\n" +
            "SIDE BY SIDE\n" +
            "- Assets/Dev/MarkerGallery/MarkerGalleryScene: Play, every shape, status, badge and outline variant in one labelled grid.\n" +
            "- Automated: Test Runner, EditMode + PlayMode, zero failures.";

        private static readonly string MarkerDeviceTestGuide =
            "SETUP (Android)\n" +
            "- USB: enable USB debugging, plug in, accept the prompt, 'adb devices' lists it.\n" +
            "- Wi-Fi: Developer options > Wireless debugging > Pair with code; 'adb pair <ip>:<pair-port>' + code, then 'adb connect <ip>:<port>'; or 'npx adb-qr-connect'.\n" +
            "- 'Save All to JSON' + 'Copy to StreamingAssets', then Build Settings > Android > Build And Run. No mock camera on device.\n\n" +
            "LOGS\n" +
            "- 'adb logcat -c', then 'adb logcat -s Unity > __logcat.txt' and read the file (no piping).\n" +
            "- Look for [WallSession] Ready N/N POIs and any marker_shape warning.\n\n" +
            "WHAT ONLY A DEVICE SHOWS\n" +
            "- Shape and icon legibility on the real screen, outdoors in bright light: is the smallest level (7 cm) still recognisable at arm's length?\n" +
            "- Colour contrast of each category against the real wall photo, not the grey Editor background.\n" +
            "- Tap the smallest marker with a real finger: the selection must hit reliably.";

        // ---------------- Badge guides ----------------

        private static readonly string BadgeSceneTestGuide =
            "SETUP\n" +
            "- Top bar 'Load & Populate Rig', then select 'lamp_military' (badge partial_damage) with its crosshair. The rig refreshes on every change (static view, nothing animates).\n\n" +
            "ENABLE BADGE\n" +
            "- Untick: every badge disappears (except the grey '?' of unknown-status POIs) and the Badge Style row of each POI hides. Tick again: they return (values are kept).\n\n" +
            "BADGE BACK SHAPE, CORNER, SIZE\n" +
            "- Back shape: change it, only the badge backdrop changes, the marker shape does not. None = icon alone.\n" +
            "- Corner: the badge jumps to the chosen corner of the symbol.\n" +
            "- Size: drag 0.20 to 0.50; the badge grows against a fixed symbol.\n\n" +
            "BADGE TABLE\n" +
            "- Change the Color or Symbol of 'partial_damage': 'lamp_military', 'lamp_residential', 'painting_military' and 'camera_military' change together.\n" +
            "- Rename a key: the POIs follow. Delete a key in use: a confirmation with the POI count.\n\n" +
            "PER-POI (Specific Marker > POI > Badge Style)\n" +
            "- Badge category dropdown: pick '(none)' and the badge goes. 'dev_marker_nobadge' has a status but no badge category.\n" +
            "- 'lamp_infrastructure' has Status unknown: it always shows the grey '?' badge.\n\n" +
            "NOT POSSIBLE IN SCENE TEST\n" +
            "- Nothing animated is involved; a real device only adds legibility and touch checks.";

        private static readonly string BadgePlaymodeTestGuide =
            "SETUP\n" +
            "- 'Save All to JSON' then 'Copy to StreamingAssets', open Apps/LivingRoom/LivingRoomScene, press Play, click into the Game view. Mock camera: W/A/S/D move, E up, Q down, RMB + mouse look.\n" +
            "- LIVE: while Play runs, Enable badge, Back shape, Corner, Size, the Badge table and each POI's Badge category apply to the running markers at once. Nothing is saved until Save All to JSON (then Copy).\n\n" +
            "ENABLE BADGE\n" +
            "- Untick while Play runs: all badges vanish except the '?' of unknown-status POIs.\n\n" +
            "SHAPE, CORNER, SIZE\n" +
            "- Cycle the corners: the badge moves around the symbol without leaving it. Size 0.50: the badge covers a large part of a 7 cm marker; pick the value that still leaves the icon readable.\n\n" +
            "TABLE AND PER-POI\n" +
            "- Edit a badge Color live and compare 'lamp_economic' (destroyed) with 'lamp' (intact).\n" +
            "- 'dev_marker_nostatus' has neither ring nor badge.\n" +
            "- Assets/Dev/MarkerGallery/MarkerGalleryScene shows every badge variant. Automated: Test Runner, zero failures.";

        private static readonly string BadgeDeviceTestGuide =
            "SETUP (Android)\n" +
            "- Same as the Marker device guide: USB or Wi-Fi debugging, 'Save All to JSON' + 'Copy to StreamingAssets', Build And Run.\n" +
            "- Logs: 'adb logcat -c', then 'adb logcat -s Unity > __logcat.txt'.\n\n" +
            "WHAT ONLY A DEVICE SHOWS\n" +
            "- Is the badge icon readable at Size 0.36 on a 7 cm marker held at arm's length? If not, raise Size or move the badge to the corner with the most free space.\n" +
            "- Do the badge colours (green / yellow / red / grey) stay distinguishable in sunlight?\n" +
            "- Is the grey '?' clearly different from a real damage badge?";

        // ---------------- Outline guides ----------------

        private static readonly string OutlineSceneTestGuide =
            "SETUP\n" +
            "- Top bar 'Load & Populate Rig'. Select 'lamp' (intact, solid ring) and 'lamp_economic' (destroyed, short dashes) with their crosshairs. Static view: the ring does not spin in Edit Mode.\n\n" +
            "ENABLE OUTLINE\n" +
            "- Untick: every ring disappears. Tick again: they return with all levels kept.\n\n" +
            "OUTLINE COLOR\n" +
            "- Uniform color: every ring shares the Outline color swatch that appears below the dropdown -- change it and every level follows. Same hue as marker: rings and symbol fills take the marker's category colour and darken with the damage. Per outline type: each row of the table below picks its own colour.\n\n" +
            "RING SIZE\n" +
            "- Drag 1.00 to 1.35: the ring moves away from the symbol. 1.00 touches it.\n\n" +
            "OUTLINE TYPES TABLE\n" +
            "- Change a level's Outline Style sprite: every marker on that level changes its dash pattern.\n" +
            "- Color: only shown in Per outline type mode. Type a hex on 'partial_damage' and its ring recolours; clear it to return to the ramp colour.\n" +
            "- '+ Add outline level' respaces the percentages; deleting a level in use asks first.\n\n" +
            "PER-POI (Specific Marker > POI > Outline)\n" +
            "- Has status off: 'dev_marker_nostatus' has no ring and no badge. Status level picks the ring; Status unknown gives the dotted grey ring and the '?' badge.\n\n" +
            "NOT POSSIBLE IN SCENE TEST\n" +
            "- Contour spin speed and Rotate: they need frames. Use Play Mode or the demo grid below (Play Mode only).";

        private static readonly string OutlinePlaymodeTestGuide =
            "SETUP\n" +
            "- 'Save All to JSON' then 'Copy to StreamingAssets', open Apps/LivingRoom/LivingRoomScene, press Play, click into the Game view. Mock camera: W/A/S/D move, E up, Q down, RMB + mouse look.\n" +
            "- LIVE: while Play runs, Enable outline, Outline Color, Outline color (Uniform mode), Ring size, Contour spin, the level table and each POI's status apply to the running markers at once. Nothing is saved until Save All to JSON (then Copy).\n\n" +
            "OUTLINE COLOR AND RING SIZE\n" +
            "- Switch Uniform color / Same hue as marker while watching 'lamp_military' and 'painting_military': same hue keeps the military red at every damage level; Uniform gives both the same ring colour whatever their damage.\n" +
            "- In Uniform color mode, drag the Outline color swatch: every ring on the wall recolours at once, even a per-row colour typed earlier in Per outline type mode.\n" +
            "- Ring size 1.35: a clear gap; 1.00: touching.\n\n" +
            "CONTOUR SPIN\n" +
            "- Levels level_1 to level_5 of LivingRoom all tick Rotate: their rings turn. Set Contour spin to 0: they stop and sit upright. 360: one turn per second.\n" +
            "- Untick Rotate in the Hierarchy Levels table for one level: only that level's rings stop.\n\n" +
            "LEVEL TABLE\n" +
            "- Switch to Per outline type, then change 'destroyed' to another Outline Style or Color: 'lamp_economic', 'painting_economic', 'camera_economic' follow.\n\n" +
            "PER-POI\n" +
            "- Toggle Has status on 'lamp': the ring and badge vanish and return.\n" +
            "- 'Add outline demo grid' below: one cell per outline mode and one cell per level, real colours and real spin, without hunting for the right POI in the wall.\n" +
            "- Assets/Dev/MarkerGallery/MarkerGalleryScene shows every outline variant. Automated: Test Runner, zero failures.";

        private static readonly string OutlineDeviceTestGuide =
            "SETUP (Android)\n" +
            "- Same as the Marker device guide: USB or Wi-Fi debugging, 'Save All to JSON' + 'Copy to StreamingAssets', Build And Run.\n" +
            "- Logs: 'adb logcat -c', then 'adb logcat -s Unity > __logcat.txt'; look for marker_outline_mode warnings.\n\n" +
            "WHAT ONLY A DEVICE SHOWS\n" +
            "- Can you tell the five dash patterns apart on the real screen at real distance? If two look alike, change their Outline Style.\n" +
            "- Spinning rings on many markers at once: check frame rate, heat and battery over 5 minutes; lower Contour spin or untick Rotate on the smaller levels.\n" +
            "- Same Hue against the real wall photo: does a dark ring still separate from a dark wall?";
    }
}
