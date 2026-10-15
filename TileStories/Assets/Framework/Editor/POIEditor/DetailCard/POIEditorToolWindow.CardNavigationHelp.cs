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
            "A family you never change keeps the order and the tick it comes with, and nothing is written to this wall " +
            "until you really change one, so a family added later needs no edit here.";

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
            "Jump Opens Card To, Jump Motion and Back Button are live too: the next tap or Back press follows them.\n" +
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
            "- Nothing else here needs a device: the order and the jumps are the same on every screen.";
    }
}
