// POIEditorToolWindow.CardNavigationHelp.cs
//
// Partial: every text of Detail Card > Navigation -- option labels, (i) help texts and the Scene / Playmode /
// Device Test guides. Its own file because DetailCardHelp.cs already carries the whole rest of the tab.
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
            "Group (only with the Segmented Top Bar): the group of the Groups table whose segment leads to this family's blocks; " +
            "(no group) leaves them out of the segments. " +
            "A family you never change keeps the order, the tick and the group it comes with, and nothing is written to this wall " +
            "until you really change one, so a family added later needs no edit here.";

        private const string CardGroupByHelp =
            "What the Top Bar's tabs (and its progress pieces) stand for. Family (the default): the visitor's reason for looking -- " +
            "About, Media, Stories... -- each with its icon and colour. Format: how a block is taken in -- Read, Look, Watch, Listen, " +
            "Immersive, Interact -- the same blocks sorted another way; a format has no icon, so its tab always shows its word. " +
            "The Segmented bar always shows Groups of families, whatever this says.";

        private const string CardLabelsHelp =
            "What each tab of the Top Bar, and each slot of the Bottom Bar or the Bar With Kinds, shows (the Dock ignores it: its " +
            "marked slot always shows icon and word, the others icon only). Icon And Text (the default): the family's icon, in " +
            "its colour, above its word -- " +
            "easiest to scan and to read. Icon: the icon alone, for the narrowest bar; its word is still read out by a screen " +
            "reader and shown on a long press. Text: the word alone. A tab with no icon (Group By = Format) always shows its word.";

        private const string CardShowCountsHelp =
            "Whether each tab, chip or segment of the Top Bar shows how many blocks it leads to, as a small number after its word. " +
            "Off (the default) keeps the bar quiet; on helps a visitor judge where most of a long card is.";

        private const string CardBarSlotsHelp =
            "How many slots the Bottom Bar, the Dock and the Bar With Kinds have, from 3 to 5 (default 5): a bar at the bottom of a phone holds three to five " +
            "destinations before its words get too small to read and its slots too narrow for a thumb. A card with more sections " +
            "than slots turns the last slot into More, which lists the sections that did not fit just above the bar. A card with " +
            "fewer sections than slots shows one slot per section and no More.";

        // What Bottom Bar's (i) says: the slot, None, then every element an app can put there -- read from the registry, like Top Bar's
        private static string CardBottomBarHelp(NavElementRegistry registry)
        {
            var text = new System.Text.StringBuilder(
                "What the bar at the bottom of the card shows, under the buttons pinned to the card's bottom (an Actions block with " +
                "the sticky_cta look), where a thumb reaches. It appears only on cards with at least Show Navigation From blocks, at " +
                "the heights Show Navigation At allows. The pinned buttons and this bar are one bottom area: they slide away and come " +
                "back together (Hide While Scrolling), and where a height leaves too little room to read, both give way together. " +
                "None (the default): no bar. It can be used with a Top Bar or instead of one.");
            foreach (var element in registry.OfSlot(NavSlotKeys.Bottom)) text.Append(' ').Append(element.Help);
            return text.ToString();
        }

        private const string CardGroupsHelp =
            "The groups the Segmented Top Bar shows as equal segments that always fit the screen -- three or four work best. " +
            "Each family joins one in the Families table's Group column. A wall with no groups of its own gets four framework " +
            "default suggestions (Discover, Watch, Explore, Visit), which you may rename or delete; the first edit here writes them " +
            "to this wall. A group with no block on a card is left out of that card, and a card whose blocks all fall in one group " +
            "gets no segments.";

        private const string CardGroupNameHelp =
            "What a visitor reads on this group's segment, in each language of the wall. Keep it to one short word: four of them " +
            "share the screen's width.";

        private const string CardShowNavigationFromHelp =
            "How many blocks a card needs before it shows any navigation (the Top Bar). A block counts when a visitor could be taken " +
            "to it: not the title block, not a block pinned under the title or to the bottom, not a block still waiting for Show After " +
            "Reading, and not a block of a family that is left out of navigation (the Sources). A shorter card shows no navigation at " +
            "all and simply scrolls -- a list of shortcuts on a short card only adds length. Default 8.";

        private const string CardShowNavigationAtHelp =
            "At which heights of the card the Top Bar may show; never while only the card's title shows. Half And Full (the default): " +
            "at half and at full height. Full Only: at full height only, so half height keeps all its room for the content. " +
            "The card may also hide the bar BY ITSELF, and that is on purpose, not a fault: at a height where the title, the bar and the " +
            "buttons pinned to the card's bottom (an Actions block with the sticky_cta look) would leave less than a heading and two " +
            "lines of content, the card leaves the bar out first: the bottom buttons are often why the card was opened, and the bar is " +
            "one drag to full height away. The buttons stay while at least one line of content still shows; with a Bottom Bar, the " +
            "buttons and the Bottom Bar are measured together and give way together. The bar comes back " +
            "as soon as there is room: at full height, or once the title shrinks to one line while the visitor reads. Full Only can " +
            "hide the bar more, never bring it back where there is no room.";

        private const string CardFollowScrollHelp =
            "Whether the Top Bar marks the part of the card the visitor is reading. On (the default): as the visitor scrolls, the mark " +
            "moves to the family (or group, or kind) whose blocks are in view, and the strip slides to keep it visible. Off: the mark " +
            "moves only when the visitor taps an item, so the bar reads as a fixed menu rather than a map of where they are.";

        private const string CardHideWhileScrollingHelp =
            "What the bottom of the card does while the visitor reads. When Scrolling Down (the default): the buttons pinned to the " +
            "card's bottom (an Actions block with the sticky_cta look) slide away once the visitor has scrolled down by about a " +
            "finger's travel, giving their room to the content, and come back as soon as the visitor scrolls up a little, or reaches " +
            "the top or the end of the card. A tap's small wobble never hides them. A Bottom Bar slides with them: they are one " +
            "area. The Top Bar always stays: it is the visitor's map of the card. Never: the bottom stays where it is while the " +
            "visitor reads.";

        // What Top Bar's (i) says: the slot, None, then every element an app can put there, each in its own words -- read from the
        // registry, so an element an app registers explains itself here with no edit to this file
        private static string CardTopBarHelp(NavElementRegistry registry)
        {
            var text = new System.Text.StringBuilder(
                "What the strip under the card's title shows, at the heights Show Navigation At allows (never while only the title " +
                "shows). It appears only on cards with at least Show Navigation From blocks. None: no strip, the card only scrolls.");
            foreach (var element in registry.OfSlot(NavSlotKeys.Top)) text.Append(' ').Append(element.Help);
            return text.ToString();
        }

        private const string CardJumpRaisesHelp =
            "What the card does before it moves to a block that a visitor asked for (an Open Block button of an Actions block today; " +
            "the navigation tabs and bars later). Full (the default): the card first rises to its full height, then moves -- at half " +
            "height only a few lines show, too little to land on. Keep Its Stop: the card stays as high as the visitor left it and " +
            "only moves its content, for a wall that wants the camera view to stay in sight.";

        private const string CardJumpMotionHelp =
            "How the card moves to a block a visitor asked for. Smooth (the default): the content glides there in the same time the " +
            "card takes to rise, so the visitor sees where they went. Instant: it is there at once. Reduce Motion (Detail Card > Card " +
            "Container) always makes it instant, whatever this says.";

        private const string CardBackButtonHelp =
            "What the phone's own Back button does while a card is open (Android; an iPhone has none, so nothing changes there). " +
            "Step The Card Down (the default): each press undoes the innermost thing -- an open full-screen view closes; else the " +
            "card goes one height down, full to half to its title; at its title the card closes, as its X does. A visitor who " +
            "presses Back to leave never loses their place in one go. Leave It To The App: the card ignores Back and the app " +
            "decides what it does.";

        private const string CardNavigationSceneTestGuide =
            "Not possible in Scene test: the card is screen-space UI Toolkit, so it draws only in Play Mode (or in Unity's " +
            "UI Builder for the bare layout); the Scene view has no preview of it to keep in sync. Use How to Playmode Test.";

        private const string CardNavigationPlaymodeTestGuide =
            "SETUP\n" +
            "- Save All to JSON, then Copy to StreamingAssets (Play reads that copy).\n" +
            "- Open the wall scene, press Play. Navigation edits are live: pick another Block Order or move a family here " +
            "and the open card reorders in place, keeping its height and its scroll. Ctrl+Z here brings the old order back. " +
            "Jump Opens Card To, Jump Motion and Back Button are live too: the next tap or Back press follows them; so are Show " +
            "Navigation From, Show Navigation At, Top Bar, Bottom Bar, Bar Slots and Hide While Scrolling: the open card's strip, " +
            "bottom bar and bottom buttons " +
            "appear, change or go at once.\n" +
            "- Pick a point whose card has blocks of several families and enough of them to scroll; a very short card " +
            "shows the same order in every option, so it proves nothing.\n\n" +
            "BLOCK ORDER\n" +
            "- Open that point's card and drag it up to full.\n" +
            "- Set Block Order to As Authored: every block sits where Card Content lists it, top to bottom. Compare the card " +
            "with the block list of Specific Marker > Card Content for that point; they must read in the same order.\n" +
            "- Set Block Order to End Families Last: the order is the same, except that the blocks of every family with " +
            "At The End ticked have moved to the bottom, keeping their order among themselves.\n" +
            "- Set Block Order to Grouped By Family: scroll the whole card once. Each family's blocks must now sit together " +
            "in one unbroken run, with no block of another family in between, and the runs must follow the Families table.\n" +
            "- In all three the title block stays at the top of the card.\n\n" +
            "FAMILIES\n" +
            "- With Grouped By Family on, move a family up or down with its arrows: the card reorders at once and that " +
            "family's whole run moves past its neighbour.\n" +
            "- Tick At The End on a family that did not have it: with End Families Last (or Grouped By Family) its blocks " +
            "move to the bottom of the card. Untick it again and they come back.\n" +
            "- Press Ctrl+Z after each change: the card goes back to what it showed before.\n\n" +
            "JUMPS\n" +
            "- In Specific Marker > Card Content, give a point's Actions block a row with Action = Open Block and pick a block far down " +
            "the card in Opens Block (its row must show no warning).\n" +
            "- Open that point's card at half height and tap the button: with Jump Opens Card To = Full the card first rises to full, " +
            "then the picked block's top comes to rest at the top of what scrolls.\n" +
            "- Set Jump Opens Card To = Keep Its Stop and tap again from half height: the card stays at half and only its content moves.\n" +
            "- Set Jump Motion = Instant: the content is there at once; Smooth: it glides. With Reduce Motion ticked it is always at once.\n" +
            "- Pick, in Opens Block, the card's title block or a block waiting for Show After Reading: the row warns and the button is gone.\n\n" +
            "TOP BAR\n" +
            "- With Top Bar = Text Tabs, open a card with at least Show Navigation From blocks and raise it to full: a strip of words " +
            "sits under the title, one per family the card has. Lower the card to its title: the strip is gone.\n" +
            "- Open a card whose blocks all belong to one family: no strip at any height (one tab is not a choice).\n" +
            "- The first word is underlined and bold. Scroll the card with the mouse wheel or a drag: the underline moves to the family " +
            "you are reading.\n" +
            "- Click a word: the card rises to full and that family's first block comes to rest at the top of what scrolls; its word " +
            "is underlined.\n" +
            "- Drag the strip sideways with the mouse: the words slide and the card does not move. Drag it up or down: the card " +
            "moves like it does from its title.\n" +
            "- Raise Show Navigation From above the card's block count: the strip goes. Set Top Bar = None: no strip on any card.\n" +
            "- Untick Follow Scroll and scroll the card: the mark stays where it was; tap a word: it moves there. Tick it again: the " +
            "mark follows the scroll.\n" +
            "- Top Bar = Icon Tabs: every tab shows its family's icon in the family's colour above its word. Set Labels = Icon: the words " +
            "go, the icons stay; Labels = Text: the icons go. Set Group By = Format: the tabs become Read, Look, Watch... with words only.\n" +
            "- Top Bar = Icon Tabs With Chips: under the tabs, a chip per kind of block of the marked family; scroll into another family " +
            "and the chips change; a family with one kind shows an empty row. Tap a chip: the card moves to that kind's first block.\n" +
            "- Top Bar = Kind Chips: one chip per kind of the whole card, a coloured dot beside each word; the chip of the block you read " +
            "is outlined and bold. Open a card with one family only: the chips still show.\n" +
            "- Top Bar = Segmented: three or four equal segments fill the strip with no sideways scroll; the one you read is filled. In the " +
            "Groups table rename a group: the segment's word changes at once. In the Families table pick another Group for a family: its " +
            "blocks now light up that segment. Put every family in one group: the segments go (one segment is not a choice).\n" +
            "- Top Bar = Progress Map: a thin bar split into pieces, a wide piece for a family with many blocks; the pieces up to the one " +
            "you read are solid, the current one thicker. Tap a piece: the card moves there.\n" +
            "- Tick Show Counts: each tab, chip or segment shows its number of blocks.\n\n" +
            "BOTTOM BAR\n" +
            "- Set Top Bar = None and Bottom Bar = Bottom Bar, open a long card with several families and raise it to full: a bar " +
            "sits at the very bottom, under the card's pinned buttons, one slot per family with its icon and word.\n" +
            "- With more families than Bar Slots the last slot reads More. Tap it: a list of the families that did not fit opens " +
            "just above the bar; tap one and the card moves there, the list closes and More is underlined. Tap More twice: the list " +
            "opens and closes.\n" +
            "- Set Bar Slots to 3, 4 and 5: the bar holds two, three or four families and More, and More's list holds the rest. " +
            "Open a card with fewer families than Bar Slots: one slot per family, no More.\n" +
            "- Tap each slot: the card moves to that family's first block and the slot is underlined; scroll the card: the underline " +
            "follows the family you read.\n" +
            "- Scroll down: the pinned buttons and the bar slide away together; scroll up a little: both come back together.\n" +
            "- Lower the card to half height on a card with a tall title: if the title, the buttons and the bar would leave less than " +
            "a line of content, the buttons and the bar both give way. That is on purpose: one line of content is the least a height " +
            "keeps. Set Show Navigation At = Full Only: at half height the bar goes and the buttons stay.\n" +
            "- Lower the card to its title: no bar.\n" +
            "- Bottom Bar = Dock: a rounded pill of icons sits at the bottom, centred. Only the family you are reading shows its word " +
            "beside its icon, on an outlined plate. Scroll the card slowly: the word and the plate move from one icon to the next as " +
            "you cross into each family. Tap an icon: the card moves there. Set Bar Slots to 3: the pill is narrower, never wider than " +
            "the screen, and the last slot reads More. Labels has no effect on it.\n" +
            "- Bottom Bar = Bar With Kinds: it looks like the Bottom Bar. Tap a family you are not in: the card moves there and no " +
            "list opens. Tap the family you are in (underlined, with a small up arrow): a list of its kinds of block opens above the " +
            "bar, each with how many blocks it has. Tap a kind: the card moves to its first block and the list closes. Tap the family " +
            "twice more: the list opens, then closes. Scroll into another family while the list is open: it closes. A family with one " +
            "kind has no arrow, and tapping it again just moves the card there again.\n" +
            "- Bottom Bar = Up Next: a bar with a small Up next line, the next block's heading, its place on the card (for example 5 of " +
            "13) and a Next button. Tap Next: the card moves to that block and the bar names the one after it, one number higher. " +
            "Scroll with your finger instead: the bar follows what you read. Go to the last block: the bar says it is the end and " +
            "has no Next button. On a card with one family the bar still shows.\n\n" +
            "SHOW NAVIGATION AT AND HIDE WHILE SCROLLING\n" +
            "- Open a long card with a tall title and buttons pinned to its bottom (an Actions block with the sticky_cta look) at half " +
            "height. If the title, the strip and the buttons would leave less than a heading and two lines of content, the strip " +
            "is left out and the buttons stay over at least one line of content: that is on purpose. Scroll the content a little: " +
            "the title shrinks to one line and the strip comes back where there is room.\n" +
            "- Raise the card to full: the strip and the buttons show. With Hide While Scrolling = When Scrolling Down, scroll down: " +
            "the buttons slide away and the content takes their room; scroll up a little: they come back. At the very end of the " +
            "card they are there too. The strip never moves.\n" +
            "- Set Hide While Scrolling = Never and scroll down again: the buttons stay.\n" +
            "- Set Show Navigation At = Full Only: at half height the strip never shows, even with the title shrunk; at full height " +
            "it does. Set it back to Half And Full: the strip shows at half height again wherever there is room.\n" +
            "- Lower the card to its title: neither the strip nor the buttons show.\n\n" +
            "BACK BUTTON\n" +
            "- In the Game view, Escape stands for the phone's Back button.\n" +
            "- Open a card, drag it to full, open a picture in its full-screen view. Press Escape four times: the full view closes, " +
            "the card goes to half, then to its title, then it closes and the marker is no longer selected.\n" +
            "- Set Back Button = Leave It To The App and press Escape with a card open: nothing happens to the card.\n" +
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
            "- Top Bar: on a card with its strip, swipe the strip sideways with a thumb -- the words slide, the card stays; start a swipe " +
            "on a word and move up -- the card rises and the word is not tapped; tap each word once -- every tap lands where it should.\n" +
            "- Bottom Bar: hold the phone in one hand and reach every slot and More with the thumb; open More and pick a family from " +
            "its list. Check that every word reads whole and that the bar does not sit under the phone's own gesture bar.\n" +
            "- Hide While Scrolling: read a long card at full height with a thumb. The bottom buttons slide away as you read down and " +
            "come back with a small move up; a tap on the content never makes them jump. Check that the half height of a long card " +
            "shows something to read on this phone's screen.\n" +
            "- Nothing else here needs a device: the order and the jumps are the same on every screen.";
    }
}
