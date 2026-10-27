// POIEditorToolWindow.CardNavigationBarHelp.cs
//
// Partial: the (i) texts of Detail Card > Navigation > Navigation Bar (POIEditorToolWindow.CardNavigationBar.cs). App-agnostic: they name no
// wall, point or family of a particular app.

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private const string CardNavBarHelp =
            "Navigation Bar: one bar of the card's groups, set up here as a whole -- its look, how the chosen group shows, where it sits and "
            + "when it shows -- and, in Bar Groups, each group it holds. While it is on, the card draws it INSTEAD of the Elements rows below "
            + "(Top Bar, Bottom Bar, Menu, Floating Button, Edge): their values are kept for the day it is turned off. It shows on a card with "
            + "navigation (Show Navigation From) and with blocks of two groups at least.";

        private const string CardNavBarEnabledHelp =
            "On: every card with navigation shows the Navigation Bar. Off: the card uses the Elements rows below, as before.";

        private const string CardNavBarShapeHelp =
            "Floating: a rounded bar inset from the screen's sides, the picture or the card showing around it. Full Width: a bar from edge "
            + "to edge with square corners, like a classic app bar.";

        private const string CardNavBarCornerRadiusHelp =
            "How round the floating bar's corners are, in points: 0 is square, about half the Height is a full pill.";

        private const string CardNavBarSideMarginHelp =
            "The gap between the floating bar and each side of the screen, in points.";

        private const string CardNavBarHeightHelp =
            "The bar's height, in points: each group's tap target. 44 is the least a finger can hit reliably; 56 fits an icon over its word.";

        private const string CardNavBarFillHelp =
            "Glass: the theme's see-through bar colour -- the picture or the card shows through it. Solid: the theme's opaque bar colour. "
            + "Clear: no fill at all, the groups float on what is under them (best with a dark picture or a shadowless card). The real camera "
            + "blur of a phone's system bars is not available to the card; Glass is its closest look.";

        private const string CardNavBarFillOpacityHelp =
            "How much of the fill's own opacity is kept: 1 is the theme's colour as it is, lower is more see-through. Keep it high where the "
            + "bar's words sit over busy pictures.";

        private const string CardNavBarBorderHelp =
            "A hairline round the bar, so it reads as one piece over a picture of the same tone.";

        private const string CardNavBarShadowHelp =
            "A soft shadow under the bar, lifting it off the card (light mode; dark mode draws no shadows).";

        private const string CardNavBarIndicatorHelp =
            "How the chosen group (the one being read) is marked, besides its bolder word. Pill: a soft wash of its colour behind it. "
            + "Underline: a short line under it. Raised: a disc of the bar's own fill rises past the bar's top edge and lifts the chosen icon "
            + "into it, as if the bar bulged under the group. None: only the word, the icon and the colour.";

        private const string CardNavBarSlideHelp =
            "On: the indicator glides from group to group as the visitor reads or taps. Off: it jumps.";

        private const string CardNavBarBoldHelp =
            "On: the chosen group's icon is drawn with heavier lines -- the filled look of a selected tab.";

        private const string CardNavBarEnlargeHelp =
            "On: the chosen group's icon grows a little.";

        private const string CardNavBarChosenColourHelp =
            "The chosen group's icon colour. Group Colour: each group's own (its Colour in Bar Groups). Accent: the theme's accent for every "
            + "group. Ink: the theme's text colour. The chosen word stays readable text either way.";

        private const string CardNavBarIdleColourHelp =
            "The other groups' icon colour. Muted: the theme's quiet text colour, so only the chosen group has colour. Group Colour: each "
            + "group keeps its own colour.";

        private const string CardNavBarLabelsHelp =
            "What each group shows: its icon and its word, the icon alone, the word alone, or every icon with the word only under the chosen "
            + "group. A group with no icon always shows its word; an icon alone is still named for a screen reader.";

        private const string CardNavBarPositionHelp =
            "Under The Title Bar: floating just under the card's title bar, at the top. Bottom Of The Screen: at the bottom, where a thumb "
            + "reaches.";

        private const string CardNavBarShowHelp =
            "After The Hero: the bar comes once the visitor starts scrolling the hero's picture away, so the opening picture stays clean. "
            + "Always: the bar shows from the start (over the hero's picture).";

        private const string CardNavBarHideHelp =
            "While Reading Down: the bar slides away while the visitor scrolls down to read, and comes back on a move up or at the top or end "
            + "of the card. Never: it stays. (It also replaces the section's own Hide While Scrolling for the card's sticky button.)";

        private const string CardNavBarGroupsHelp =
            "Bar Groups: each group of the bar, in its order (^ / v move one). A group leads to the blocks of its Families -- a tap lands on "
            + "the first of them on the card -- and a card shows only the groups it has blocks of. Open a group to name it in every "
            + "language, pick its icon, the icon it wears while chosen, and its colour. Shown off keeps the row but leaves the group out of "
            + "the bar. + Add group makes a new group (pick its Families); Reset To The Families goes back to one group per family. Until "
            + "the first edit, the table shows that default.";

        private const string CardNavBarGroupNameHelp =
            "The group's word in each language. Empty: its first family's own word (the same the rest of the card uses).";

        private const string CardNavBarGroupIconHelp =
            "The group's icon, from the card's icon set. Its Family's Icon: the first family's own.";

        private const string CardNavBarGroupChosenIconHelp =
            "The icon the group wears while chosen (for example a fuller version of its icon). Same As Icon: no change, only the indicator, "
            + "the colour and Bold Chosen Icon mark it.";

        private const string CardNavBarGroupColourHelp =
            "The group's colour, one of the theme's: it paints the group's icon (as Chosen Colour and Other Groups' Colour say) and the "
            + "indicator's wash. Its Family's Colour: the first family's own.";

        private const string CardNavBarGroupFamiliesHelp =
            "The families whose blocks this group leads to. Pick several to merge them into one group (for example the pictures and the "
            + "audio as one Media group). A family in two groups belongs to the first; a family in none cannot be reached from the bar.";

        private const string CardNavBarOnNotice =
            "The Navigation Bar is on: the card draws it instead of these elements. Their values are kept for the day it is turned off.";
    }
}
