// POIEditorToolWindow.ComposedBlockHelp.cs
//
// Partial: every (i) text and tooltip of a composed block's Editor (the Page's element table, an open row's parts, the Preview). ASCII,
// app-agnostic, naming Editor controls only (_5.1 "Domain Manual Tests": point at the Editor Tab, never at a doc or a code file).

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private const string StoryPageHelp =
            "The block's own page: what opens when a visitor taps its preview. Write it here once, row by row, in the order a visitor reads " +
            "it. Each row is one element (a title, a paragraph, a picture...). Open a row to write its words in every wall language, pick its " +
            "files, and set its Look, Motion and Tap. Drag a row by its grip (or use the arrows) to move it; P marks the rows the preview shows; " +
            "the check / cross is your own Validated mark (a visitor never sees it).";

        private const string StoryAddElementHelp =
            "Adds a row at the end of the table, opened with every part shown and each option at its designed default: only the words and " +
            "files are left to fill (marked until they are). Text: titles, paragraphs, a callout, a quote, a list. Media: pictures, a carousel, " +
            "a video. Structure: a divider or some space.";

        private const string StoryPageChromeOverrideHelp =
            "The page's top bar and foot. Inherit the wall's: what Detail Card > Card Components > Page Chrome sets for every block page. " +
            "Override: this block's own choice, starting from the wall's; back to Inherit keeps it for later.";

        private const string StoryBreadcrumbHelp =
            "Shown: one line beside the Back arrow -- the point and the section (a page with its own title is not named again). Hidden: the Back " +
            "arrow alone.";

        private const string StoryPrevNextHelp =
            "Shown: two cards at the end of the page that turn to the neighbouring blocks. Hidden: no cards; a sideways swipe still turns the page.";

        private const string StoryMotionTableHelp =
            "The page's choreography in one place: one line per row, in order. Each chip is the SAME value as the row's own Motion part: a click " +
            "steps to the next effect the element offers (Auto plays the element's own, shown in brackets), the second chip its strength. " +
            "Reduce Motion is not a choice: every row is simply still for a visitor who asks for less motion, shown here so you can see it.";

        private const string StoryPreviewMarkTooltip = "P: the preview shows this row (it changes as you reorder or delete rows).";
        private const string StoryValidatedTooltip = "Your own check of each row: a red cross until you mark it, a green check after. Never shown to a visitor.";
        private const string StoryValidatedOnTooltip = "Validated: click to mark this row to check again.";
        private const string StoryValidatedOffTooltip = "Not validated yet: click once you have checked this row.";
        private const string StoryReducedTooltip = "What a visitor who reduces motion sees: the row still. A rule, not a choice.";
        private const string StoryMotionChipTooltip = "Click to step to the next effect this element offers (the same value as the row's Motion part).";
        private const string StoryBrokenLinkNote = "This link's Page row was deleted: Unlink it (an empty row of the same element to write) or remove it.";

        private static string StoryUnknownElementNote(string kind) =>
            "This row names an element that is not registered" + (string.IsNullOrEmpty(kind) ? "" : " ('" + kind + "')") +
            ": the card shows nothing for it. Delete it, or add the element back.";

        private const string StoryLookHelp =
            "How this row looks, from the theme's own options only: every tone and size keeps the theme's contrast in light and dark.";

        private const string StoryRowMotionHelp =
            "How this row moves as the page scrolls. Auto (the default) plays the element's own: text is revealed, a picture moves gently " +
            "inside its frame. None keeps it still. A visitor who reduces motion always sees it still.";

        private const string StoryStrengthHelp = "Subtle (the default) or Strong: how far the effect travels. Nothing to set while the row does not move.";

        private const string StoryTapHelp =
            "What a visitor's tap on this row does on the page: a picture opens the full-screen viewer (or nothing); a video plays where it is " +
            "(with a full-screen button) or opens the card's player.";

        private const string StoryUseLinkedTooltip = "Show this row in the preview as a link: the same words, its own look. The preview becomes Custom.";
        private const string StoryCopyTooltip = "Put a copy of this row in the preview, to change on its own. The preview becomes Custom.";

        private const string StoryUseInPreviewHelp =
            "Use in the preview (linked): the preview shows this row's content, edited once, here; the preview row sets its own look. Copy to " +
            "the preview: an independent copy the preview alone shows. Either one turns the preview to Custom, keeping the rows it already showed.";

        private const string StoryMediaHelp =
            "The row's files, in order, each with a caption in every wall language and a credit line. A file must sit inside the wall's Media " +
            "Folder (Detail Card > Card Container) or be one of the default media.";

        private const string StoryMediaFileHelp = "The picture or clip, picked from the wall's Media Folder (or a default with Pick default...).";
        private const string StoryCaptionHelp = "What the picture shows, under it, in every wall language (optional).";
        private const string StoryCreditHelp = "Who made it and its licence, one line for every language (optional).";

        private const string StoryPreviewHelp =
            "What the card shows of this block before it is opened, ending with its Open Row. Automatic (the default) takes the Page's first " +
            "picture, its title and its first text, and follows every Page edit. Custom: rows of its own, each a link to a Page row or a row only " +
            "the preview holds.";

        private const string StoryPreviewModeHelp =
            "Automatic: derived from the Page, nothing to keep in step. Custom: the rows below; switching starts it as the Automatic preview " +
            "showed it, and switching back keeps the Custom rows for later.";

        private const string StoryCustomRowsHelp =
            "Link a Page row: the same content, its own look (the link icon). Add own row: content only the preview shows, such as a teaser line. " +
            "A link whose Page row was deleted shows as a warning: Unlink makes it a row of its own, the trash removes it.";

        private const string StoryUnlinkTooltip = "Make this row a copy of the Page row's content, to change on its own.";
        private const string StoryUnlinkBrokenTooltip = "Keep the place as an empty row of the same element, to write again.";

        private const string StoryOpenRowOverrideHelp =
            "The preview's last line (words, reading time, a chevron) that opens the page. Inherit the wall's: the emphasis of Detail Card > Card " +
            "Components > Open Row and the words of Detail Card > Card Texts. Override: this block's own words and emphasis, starting from the " +
            "wall's; back to Inherit keeps them for later.";

        private const string StoryOpenRowWordsHelp = "This block's words for its Open Row, in every wall language. Empty: the card's own words.";

        private const string StoryOpenRowEmphasisHelp =
            "Plain, Nudge (the chevron steps towards the page) or Shine (a light crosses the words). A visitor who reduces motion sees Plain.";

        private const string StoryReadingTimeHelp =
            "The minutes the Open Row says. Auto (the default): counted from the Page's words and pictures, and kept up to date. Set by hand: " +
            "the number you type (1 to 120), starting from the Auto count.";

        private const string StoryBlockValidationTooltip = "Rows of this block's Page and Preview you have not marked Validated yet (your own check, never shown to a visitor).";
    }
}
