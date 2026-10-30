// POIEditorToolWindow.CardNavigationHelp.cs
//
// Partial: every text of Detail Card > Navigation outside the Navigation Bar component -- (i) help texts, the notes and the Scene /
// Playmode / Device Test guides. Its own file because DetailCardHelp.cs already carries the whole rest of the tab.
// Framework-authored and app-agnostic: they name Editor Tab controls, never a project doc, a code file or one
// wall's own content (_5.1_Editor_Tab.md, "Domain Manual Tests").

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private const string CardBlockOrderHelp =
            "The order a card shows its blocks in, whatever order Card Content puts them in. " +
            "End Families Last (the default): the blocks stay in the order they were written, then every family whose " +
            "At The End is ticked closes the card, keeping its own order -- so a card ends with its sources. " +
            "As Authored: every block stays exactly where Card Content puts it, nothing is moved. " +
            "Grouped By Family: each family becomes one unbroken run, in the order of the Families table below, and the " +
            "blocks inside a family keep the order they were written. Pick this one when a card is long and you want the " +
            "visitor to be able to scan it family by family. The title block is always first, in every option.";

        private const string CardFamiliesHelp =
            "The families of this wall's cards, in the order Grouped By Family uses. A family is the visitor's reason for " +
            "looking ('what do I want?'), not a file type, and every block kind belongs to one. " +
            "Order: the up and down arrows move a family past its neighbour. At The End: the family's blocks close the " +
            "card instead of staying where they were written -- this is what End Families Last moves. " +
            "A family you never change keeps the order and the tick it comes with, and nothing is written to this wall " +
            "until you really change one, so a family added later needs no edit here.";

        private const string CardGroupByHelp =
            "What the card's sections are: what the Tabs and Pages layouts show one at a time, and what the card says is being read. " +
            "Family (the default): the visitor's reason for looking -- About, Media, Stories... Format: how a block is taken in -- " +
            "Read, Look, Watch, Listen, Immersive, Interact -- the same blocks sorted another way. The Navigation Bar always shows " +
            "its own groups (its Bar Groups table), whatever this says.";

        private const string CardShowNavigationFromHelp =
            "How many blocks a card needs before it shows any navigation (the Navigation Bar). A block counts when a visitor could be " +
            "taken to it: not the title block, not a block pinned under the title or to the bottom, not a block still waiting for Show " +
            "After Reading, and not a block of a family that is left out of navigation (the Sources). A shorter card shows no " +
            "navigation at all and simply scrolls -- a list of shortcuts on a short card only adds length. Default 8.";

        private const string CardShowNavigationAtHelp =
            "At which heights of the card a Navigation Bar at the Top may show; never while only the card's title shows. Half And Full " +
            "(the default): at half and at full height. Full Only: at full height only, so half height keeps all its room for the " +
            "content. The card may also hide the bar BY ITSELF, and that is on purpose, not a fault: at a height where the title, the " +
            "bar and the buttons pinned to the card's bottom would leave less than a heading and two lines of content, the card leaves " +
            "the bar out first. The bar comes back as soon as there is room. Full Only can hide the bar more, never bring it back where " +
            "there is no room. A bar at the Bottom floats over the content and is not affected.";

        private const string CardFollowScrollHelp =
            "Whether the navigation marks the part of the card the visitor is reading. On (the default): as the visitor scrolls, the " +
            "chosen group of the Navigation Bar moves to the one whose blocks are in view. Off: the mark moves only when the visitor taps " +
            "a group, so the bar reads as a fixed menu rather than a map of where they are.";

        private const string CardHideWhileScrollingHelp =
            "What the bottom of the card does while the visitor reads, with the Navigation Bar off (while it is on, its own Hide While " +
            "Scrolling decides). When Scrolling Down (the default): the buttons pinned to the card's bottom (an Actions block with the " +
            "sticky_cta look) slide away once the visitor has scrolled down by about a finger's travel, giving their room to the " +
            "content, and come back as soon as the visitor scrolls up a little, or reaches the top or the end of the card. A tap's " +
            "small wobble never hides them. Never: the bottom stays where it is while the visitor reads.";

        private const string CardJumpRaisesHelp =
            "What the card does before it moves to a block that a visitor asked for (a group of the Navigation Bar, an Open Block " +
            "button of an Actions block). Full (the default): the card first rises to its full height, then moves -- at half height " +
            "only a few lines show, too little to land on. Keep Its Stop: the card stays as high as the visitor left it and only " +
            "moves its content, for a wall that wants the camera view to stay in sight.";

        private const string CardJumpMotionHelp =
            "How the card moves to a block a visitor asked for. Smooth (the default): the content glides there in the same time the " +
            "card takes to rise, so the visitor sees where they went. Instant: it is there at once. Reduce Motion (Detail Card > Card " +
            "Container) always makes it instant, whatever this says.";

        private const string CardLayoutHelp =
            "How the card arranges its blocks. Scroll (the default): every block one after the other, in one scroll. Accordion: every " +
            "block the navigation can lead to is one line (its icon, its heading or first words, its kind of content and minutes); a " +
            "tap opens it in place and the page does not move, another tap folds it, and Back folds the line opened last. Nothing " +
            "of a folded line is loaded, so a long card opens light. Suits a card visitors skim to pick one or two things (Open One " +
            "At A Time keeps it to one). Tabs: one section of the card at a time -- a tap on a group of the Navigation Bar switches " +
            "between them, and the other sections are put away (nothing of them is loaded). The sources show with every section. " +
            "Avoid Tabs when visitors must compare things that sit in different sections: they cannot see both at once. " +
            "Pages: each section is a page the visitor turns sideways with a swipe, and scrolls up and down inside; the edge of the " +
            "next page shows at the right as the cue. Only a clearly sideways swipe turns a page, so reading down never does by " +
            "accident, and like Tabs the pages put away are not loaded. Offer it for a short card read section by section, like a " +
            "story, and never as a wall's only layout: a visitor who has always scrolled down can be lost when a card suddenly turns " +
            "sideways. A card with fewer blocks than Show Navigation From, or with one section, always scrolls.";

        private const string CardOpenOneAtATimeHelp =
            "Accordion only. Ticked: opening a line folds the line that was open, so the card never holds more than one open block " +
            "-- the line you tapped keeps its place on the screen. Unticked (the default): lines open and fold independently, for a " +
            "visitor who wants two blocks open side by side in the scroll.";

        private const string CardBackButtonHelp =
            "What the phone's own Back button does while a card is open (Android; an iPhone has none, so nothing changes there). " +
            "Step The Card Down (the default): each press undoes the innermost thing -- an open full-screen view closes; else an " +
            "open list over the card closes (the card stays where it is); else an open line of an accordion or of an " +
            "Expandable block folds, the one opened last first; else the " +
            "card goes one height down, full to half to its title; at its title the card closes, as its X does. A visitor who " +
            "presses Back to leave never loses their place in one go. Leave It To The App: the card ignores Back and the app " +
            "decides what it does.";

        private const string CardPreviewNavigationHelp =
            "Developer-only. Opens the demo card on the point below, at full height, so you can change a row here and watch the card " +
            "follow in Play Mode. It is the same switch as Show demo card in Detail Card > Card Container > Test: the two turn on and " +
            "off together. Off by default; a release build ignores it. To turn it off, untick it here (or there), then Save All to " +
            "JSON and Copy to StreamingAssets before a normal build.";

        private const string CardPreviewPointHelp =
            "The point whose card opens (the same choice as Demo Point in Card Container > Test). Pick one with a long card: a card with " +
            "fewer blocks than Show Navigation From only scrolls, so there is nothing of this section to see on it.";

        // The note under Preview Point when the picked card shows no navigation at all
        internal static string CardPreviewNoNavigationNote(string cardName, int minBlocks) =>
            "The card of " + cardName + " shows no navigation: it has fewer blocks than Show Navigation From (" + minBlocks +
            "), so it only scrolls. Pick a point with a longer card, or lower Show Navigation From.";

        private const string CardNavigationSceneTestGuide =
            "Not possible in Scene test: the card is screen-space UI Toolkit, so it draws only in Play Mode (or in Unity's " +
            "UI Builder for the bare layout); the Scene view has no preview of it to keep in sync. Use How to Playmode Test.";

        private const string CardNavigationPlaymodeTestGuide =
            "SETUP\n" +
            "- Save All to JSON, then Copy to StreamingAssets (Play reads that copy).\n" +
            "- Open the wall scene, press Play. Navigation edits are live: pick another Block Order or move a family here and the open " +
            "card reorders in place, keeping its height and its scroll. Ctrl+Z here brings the old order back. Layout, Open One At A " +
            "Time, Show Navigation From, Show Navigation At, Follow Scroll, Jump Opens Card To, Jump Motion and Back Button are live " +
            "too: the open card, or its next tap or Back press, follows them at once.\n" +
            "- The rows are in four groups under their own headings: Shape, When Navigation Shows, How They Look (only under Accordion) " +
            "and What A Jump Does. Placed Model Control is not here: it is in Card Container.\n" +
            "- Pick a point whose card has blocks of several families and enough of them to scroll; a very short card shows the same " +
            "order in every option, so it proves nothing. Test > Preview Navigation opens that card for you at full height.\n\n" +
            "SHAPE\n" +
            "- Block Order: pick each option on a long card and read the card from top to bottom: As Authored keeps Card Content's " +
            "order, End Families Last moves the ticked families (Sources) to the end, Grouped By Family makes each family one run in " +
            "the Families table's order. Move a family with its arrows: the open card follows.\n" +
            "- Layout = Accordion: every block is one line; tap a line, it opens right under it and the card does not move; tap it " +
            "again, it folds. Open two lines and press Escape: the one opened last folds, then the other. Tick Open One At A Time: " +
            "opening a line folds the other, and the line you tapped stays where it was.\n" +
            "- Layout = Tabs: the card shows one section at a time; tap another group of the Navigation Bar: only its blocks show, and " +
            "scrolling inside it never moves the mark. The sources at the end show with every section.\n" +
            "- Layout = Pages: swipe sideways on the content: the next page turns in, and a drag straight down only scrolls the page.\n" +
            "- Group By = Format with Tabs: the sections are Read, Look, Watch...; the Navigation Bar keeps its own groups.\n\n" +
            "WHEN NAVIGATION SHOWS\n" +
            "- Show Navigation From: raise it above the open card's block count -- the Navigation Bar goes and the card only scrolls; " +
            "lower it again -- the bar is back.\n" +
            "- Show Navigation At = Full Only with the Navigation Bar at the Top: at half height no bar, at full height the bar; Half And Full: the bar at both.\n" +
            "- Hide While Scrolling = When Scrolling Down (the Navigation Bar off): on a card with buttons pinned to its bottom, read down: they slide away; " +
            "a small move up brings them back. Never: they stay.\n\n" +
            "WHAT A JUMP DOES\n" +
            "- Follow Scroll on: scroll the card by hand, the bar's chosen group follows the blocks in view. Off: it moves only on a tap.\n" +
            "- Jump Opens Card To = Full: at half height, tap a group of the bar -- the card rises to full first, then moves. Keep Its " +
            "Stop: it stays at half and only the content moves.\n" +
            "- Jump Motion = Smooth: the content glides to the block; Instant: it is there at once. With Reduce Motion on (Card " +
            "Container) every jump is instant.\n" +
            "- Back Button = Step The Card Down: open a block's full view and press Escape (the Editor's Back): it closes; again: the " +
            "card goes from full to half, then to its title, then closes. Leave It To The App: Escape does nothing to the card.\n\n" +
            "EXPANDABLE BLOCKS AND THE FULL VIEW\n" +
            "- In Specific Marker > Card Content, set Display = Expandable on a few blocks of a long card (a text, a gallery, a video, a " +
            "3D model). Open the card: each of them is one line -- its icon, its heading (or its first words), its kind of content " +
            "and its minutes -- and its pictures, sound or model are not loaded yet.\n" +
            "- Tap a line: the block opens under it, the arrow turns over and a full-view button shows at its end. Tap the line again: " +
            "it closes; a video playing in it pauses. An open video line has no full-view button of its own: the player's full-screen " +
            "button is the one way in.\n" +
            "- Open a line and tap its full-view button: the block fills the screen, the card's title and the block's words above it. " +
            "Play the video or turn the model there, press Escape: the card shows the same video at the same moment, and its own " +
            "player and model still answer. Switch the language or change a field while the full view is open: it stays, redrawn.\n\n" +
            "PREVIEW NAVIGATION\n" +
            "- In this section's Test, tick Preview Navigation and pick a point with a long card in Preview Point, then press Play: that " +
            "card opens by itself at full height. Change a row above and watch the card follow. A point whose card is too short for " +
            "navigation shows a note under Preview Point. Untick it when you are done.\n" +
            "- Test Runner: EditMode and PlayMode, zero failures.";

        private const string CardNavigationDeviceTestGuide =
            "- Untick any developer-only switch you turned on, then Save All to JSON and Copy to StreamingAssets.\n" +
            "- Connect the phone over USB or Wi-Fi (adb pair <ip>:<port> with the code, then adb connect; or npx adb-qr-connect), " +
            "then Build And Run. There is no mock camera on a device.\n" +
            "- Read the logs from a file, not from a pipe: adb logcat -c, then adb logcat -s Unity > __logcat.txt.\n" +
            "- Only what a device really shows differently: open a long card and scroll it from top to bottom with a thumb. " +
            "With Grouped By Family, check that a visitor can tell where one family ends and the next begins while scrolling " +
            "fast, and that the blocks that close the card still read as the end of it rather than as more content.\n" +
            "- Back Button (Android): open a card, raise it to full, and press the phone's own Back button (or make the Back gesture) " +
            "once per step: full to half, half to its title, then the card closes. With Leave It To The App, Back does what the app " +
            "does without a card. An iPhone has no Back button: nothing to check there.\n" +
            "- Layout: with Accordion, open and fold lines with a thumb while scrolling, and check an open line never jumps the page; " +
            "with Tabs, switch sections with a thumb and check visitors can still tell which section they are in after scrolling; " +
            "with Pages, read a page down with a thumb and turn pages with the same thumb, and check a scroll never turns a page by " +
            "accident and the next page's edge is noticed outdoors.\n" +
            "- Expandable: on a card with expandable blocks, tap each line with a thumb (the whole line opens it) and the full-view " +
            "button of an open one; read every line's words whole at the phone's width.\n" +
            "- Nothing else here needs a device: the order and the jumps are the same on every screen.";
    }
}
