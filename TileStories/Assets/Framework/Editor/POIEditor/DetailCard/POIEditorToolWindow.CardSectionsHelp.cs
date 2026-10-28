// POIEditorToolWindow.CardSectionsHelp.cs
//
// Partial: every (i) text and Test guide of Detail Card > Sections and of the Bar Groups' Section Style (ASCII, app-agnostic, Editor controls
// only).

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private const string CardSectionsHelp =
            "How each section of an open card looks. A section is one Navigation Bar group: its blocks together, opened by the group's name in " +
            "its colour. These rows set every section; a group's Section Style (Navigation > Bar Groups) gives it a Start of its own. Under " +
            "Card Container > Blocks = Cards none of this draws.";

        private const string CardSectionsCardsNote =
            "Card Container > Blocks is Cards: every family sits on its glass sheet and these rows do not draw. Set Blocks to Open to see them.";

        private const string CardSectionsStartHelp =
            "How a section begins. Plain: nothing but space. Bands: every other section on a faint flat tint, edge to edge. Fade: the section's " +
            "colour fading down from its top. Wave: a soft swell, then the fade. Tiles (the default): a row of tiles drawn in the section's " +
            "colour, rounded at the top corners, then the fade.";

        private const string CardSectionsDividerHelp =
            "What stands between two sections (never before the first). None (the default): only space. Hairline: a thin line. Ornament: one " +
            "tile between two short lines. Azulejo: a strip of tiles. Mosaic: small woven tiles. Image: your own tileable strip picture " +
            "(Divider Image), repeated across.";

        private const string CardSectionsDividerImageHelp =
            "The strip picture Divider Image repeats across the card: a path under the card's Media Folder (Card Container), for example " +
            "dividers/strip.png. Make it tileable: its left and right edges must meet. It is drawn 24 points high.";

        private const string CardSectionsNoImageNote =
            "Divider is Image but no Divider Image is set: no divider draws. Type the strip picture's path under the Media Folder.";

        private const string CardSectionsColourHelp =
            "The colour a section's start is drawn in. Group (the default): its Navigation Bar group's colour. Accent: the theme's accent for " +
            "every section. Neutral: a quiet warm grey. The group's name keeps its own colour either way.";

        private const string CardSectionsIntensityHelp =
            "How strong the tint is where a section begins (it fades out about 340 points down). The highest value still keeps every word " +
            "over it at 7:1 in light and dark.";

        private const string CardSectionsMotionHelp =
            "Still: nothing moves. Reveal (the default): the tiles turn in one after the other the first time their section comes into view. " +
            "Drift: the tiles turn in as with Reveal, then the divider's tiles or strip slide slowly sideways; the start rests. " +
            "The card's Reduce Motion (Card Container) always shows them still.";

        private const string CardSectionsVaryHelp =
            "Same (the default): every section begins with Start. By Group: Fade, Wave and Tiles in turn, group after group. A group's own " +
            "Section Style always wins.";

        private const string CardNavBarGroupSectionStyleHelp =
            "How this group's sections begin on an open card. The Card's: Detail Card > Sections' Start (or Vary). Any other value: this group " +
            "alone begins that way.";

        private const string CardBlocksHelp =
            "How the card's blocks sit. Open (the default): sections open on the card, no frames -- each block its own heading, pictures edge " +
            "to edge, a thin line between two blocks; Detail Card > Sections styles them. Cards: every family on its glass sheet, every block " +
            "in its frame.";

        private const string CardSectionsSceneTestGuide =
            "Scene: Detail Card > Card Container > Test > Show demo card, pick a point with several groups and Demo Stop Full, then press Play " +
            "(the card draws only in Play Mode).";

        private const string CardSectionsPlaymodeTestGuide =
            "Play Mode: with a card open at Full, change Start, Divider, Colour and Intensity here and watch each section of the open card " +
            "change at once. Set Motion to Reveal, scroll the card back to its top, then drag down: each section's tiles turn in as it comes " +
            "into view. Card Container > Reduce Motion on shows them still and whole. In Navigation > Bar Groups, open a group and set its " +
            "Section Style: only that group's sections change.";

        private const string CardSectionsDeviceTestGuide =
            "Device: open a point's card outdoors in bright light and in a dark room, in light and dark mode, at Intensity's highest value: " +
            "every section's name and its text read clearly over its tint. Scroll quickly through the card: the tiles turn in without delaying " +
            "the reading.";
    }
}
