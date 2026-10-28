// POIEditorToolWindow.CardNavigationBarHelp.cs
//
// Partial: the (i) texts of Detail Card > Navigation > Navigation Bar (POIEditorToolWindow.CardNavigationBar.cs). App-agnostic: they name no
// wall, point or family of a particular app.

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private const string CardNavBarHelp =
            "Navigation Bar: the visitor's main way round the card -- one bar of the card's groups. Set here where it sits, its shape and "
            + "fill, how the chosen group shows, when it shows, and, in Bar Groups, each group it holds. While it is on, the card draws it "
            + "INSTEAD of the Elements rows below (Top Bar, Bottom Bar, Menu, Floating Button, Edge): their values are kept for the day it is "
            + "turned off. It shows on a card with navigation (Show Navigation From) and with blocks of two groups at least.";

        private const string CardNavBarEnabledHelp =
            "On (the default): every card with navigation shows the Navigation Bar. Off: the card uses the Elements rows below.";

        private const string CardNavBarPositionHelp =
            "Bottom (the default): floating over the foot of the screen, where a thumb reaches; the hero's words rise clear of it. Top: "
            + "right under the card's top row (the close and settings marks, the title).";

        private const string CardNavBarShapeHelp =
            "Floating (the default): a rounded bar inset from the screen's sides, the picture or the card showing around it. Full Width: a "
            + "bar from edge to edge with square corners, like a classic app bar.";

        private const string CardNavBarFillHelp =
            "Solid (the default): the theme's opaque bar colour, with a soft shadow under a floating bar. Glass: see-through; over the hero's "
            + "picture it turns dark glass with white words. Clear: transparent over the hero's picture (white words on the picture), solid "
            + "once the hero is read. Only Solid has a shadow: under glass a shadow shows through as a grey haze.";

        private const string CardNavBarIndicatorHelp =
            "How the chosen group (the one being read) is marked, besides its bolder word. Raised (the default): the bar's edge bends into a "
            + "bump under the chosen group and its icon rides up in a filled disc; both slide to the next group with a small overshoot (at "
            + "Top the bump hangs down). Pill: a soft wash behind it. Underline: a short line under it. None: only the word, icon and colour.";

        private const string CardNavBarChosenIconHelp =
            "Filled (the default): the chosen icon sits in a filled disc of the Chosen Colour at its own place, with any indicator. Plain: the "
            + "icon alone, in the Chosen Colour. A bar of words alone (Labels: Text) has no disc.";

        private const string CardNavBarLabelsHelp =
            "What each group shows: Icon And Text (the default), the Icon alone, the Text alone, or every icon with the Text On The Chosen "
            + "group only. A group with no icon always shows its word; an icon alone is still named for a screen reader.";

        private const string CardNavBarChosenColourHelp =
            "The chosen group's colour (its icon, its disc, its word). Accent (the default): the theme's accent. Ink: the theme's text "
            + "colour. Group: each group's own Colour from Bar Groups (its word then stays text colour, readable on the bar).";

        private const string CardNavBarBoldHelp =
            "On (the default): the chosen group's icon is drawn with heavier lines.";

        private const string CardNavBarShowHelp =
            "Always (the default): the bar shows from the start, over the hero's picture. After The Hero: it comes once the visitor scrolls "
            + "the hero's picture away, so the opening picture stays clean.";

        private const string CardNavBarHideHelp =
            "On: the bar slides away while the visitor scrolls down to read, and comes back on a move up or at the top or end of the card. "
            + "Off (the default): it stays. (It also replaces the section's own Hide While Scrolling for the card's sticky button.)";

        private const string CardNavBarGroupsHelp =
            "Bar Groups: each group of the bar, in its order (^ / v move one). A group leads to the blocks of its Families -- a tap lands on "
            + "the first of them on the card -- and a card shows only the groups it has blocks of. Open a group to name it in every "
            + "language, pick its icon, the icon it wears while chosen, and its colour. Shown off keeps the row but leaves the group out of "
            + "the bar. + Add group makes a new group (pick its Families); the trash removes one; Reset To The Families goes back to the "
            + "framework's six groups. Until the first edit, the table shows those six.";

        private const string CardNavBarGroupNameHelp =
            "The group's word in each language. Empty: its first family's own word (the same the rest of the card uses).";

        private const string CardNavBarGroupIconHelp =
            "The group's icon, from the card's icon set. Its Family's Icon: the first family's own.";

        private const string CardNavBarGroupChosenIconHelp =
            "The icon the group wears while chosen (for example a fuller version of its icon). Same As Icon: no change, only the indicator, "
            + "the disc, the colour and Bold Chosen Icon mark it.";

        private const string CardNavBarGroupColourHelp =
            "The group's colour, one of the theme's: it paints the chosen group when Chosen Colour is Group. Its Family's Colour: the first "
            + "family's own.";

        private const string CardNavBarGroupFamiliesHelp =
            "The families whose blocks this group leads to. Pick several to merge them into one group. A family in two groups belongs to "
            + "the first; a family in none cannot be reached from the bar.";

        private const string CardNavBarOnNotice =
            "The Navigation Bar is on: the card draws it instead of these elements. Their values are kept for the day it is turned off.";
    }
}
