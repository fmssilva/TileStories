// POIEditorToolWindow.CardLocateHelp.cs
//
// Partial: every (i) text and Test guide of Detail Card > Locate Button (ASCII, app-agnostic, Editor controls only).

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private const string CardLocateOffNote =
            "Navigation > Navigation Bar > Locate Button is off (or the bar is off): the card shows no Locate Button and these rows do not " +
            "draw. Turn it on there to see them.";

        private const string CardLocateStyleHelp =
            "Floating Pin: the pin alone, hovering over the bar. Pin + Label (the default): the pin with \"Find it on the wall\" on a light " +
            "chip beside it when the card opens; the words fold away the first time the visitor scrolls the card, and the pin stays. Glass + " +
            "Ping: the pin on a light glass disc, a soft ring going out from it every few seconds. The words are a card text (Card Texts), " +
            "in every language of the wall.";

        private const string CardLocateColourHelp =
            "The pin's colour. AR Teal (the default), Fresh Green or Cyan: the pin's own colours, the same in light and dark. Accent: the " +
            "theme's accent. Each has a thin darker rim (lighter in dark mode) that keeps it clear of the card behind it.";

        private const string CardLocateSizeHelp =
            "The pin's height as a share of the Navigation Bar's chosen group disc: 1 (the default) is as big as the chosen group's " +
            "icon disc, 0.6 a little over an idle bar icon, 2 twice the disc. The pin floats over the card and never moves its words or " +
            "blocks. Whatever the size, the button is a whole fingertip target (44 points or more) round the pin.";

        private const string CardLocateFloatHelp =
            "On (the default): the pin bobs slowly over the bar, its shadow on the bar shrinking as it rises. Off: the pin stands still. " +
            "The card's Reduce Motion (Card Container) always keeps it still.";

        private const string CardLocateSceneTestGuide =
            "Scene: Detail Card > Card Container > Test > Show demo card, pick a point and Demo Stop Half, then press Play (the card draws " +
            "only in Play Mode).";

        private const string CardLocatePlaymodeTestGuide =
            "Play Mode: with a card open at Half, change Style, Colour, Size and Float here and watch the pin at the card's bottom right " +
            "change at once. Under Pin + Label, scroll the card a little: the words fold away and the pin stays; open another point and " +
            "they show again. Tap the pin: the card lowers and the point's marker shows on the wall. Card Container > Reduce Motion on keeps " +
            "the pin still.";

        private const string CardLocateDeviceTestGuide =
            "Device: open a point's card outdoors in bright light and in a dark room, in light and dark mode: the pin stands out from the " +
            "card's paper and from the picture at its top, and its words read clearly. Tap it with a thumb at the largest and the smallest " +
            "Size: one tap always lowers the card.";
    }
}
