// POIEditorToolWindow.CardComponents.cs
//
// Partial: Detail Card > Card Components (_3.3 R17.5): the shared pieces of chrome every block draws, set ONCE for the wall
// (card_settings.components); a block that composes its own page overrides a component in its Component Overrides row. One row
// per component; Page Chrome (the block page's breadcrumb and Previous / Next row) is the first.

using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private readonly TestGuideState _cardComponentsTest = new TestGuideState();

        // card_settings.components: the Open Row (its emphasis, one click on a segment), then the Page Chrome row -- a heading with its (i),
        // then one toggle per part. Drawing never writes (the object comes with the schema's defaults); an edit goes through the window's
        // history like every card row.
        private void DrawCardComponentsSection()
        {
            var components = _config.card_settings.components ??= new CardComponentSettings();
            DrawEditorRow(out float openRowTitle, out _);
            GUILayout.Label("Open Row", EditorStyles.miniBoldLabel, GUILayout.Width(Mathf.Max(40f, openRowTitle - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw("Open Row", CardOpenRowHelp);
            EditorRowEnd();
            string emphasis = ElementOptions.OneOf(components.open_row_emphasis, ElementOptions.OpenRowEmphases);
            int at = System.Array.IndexOf(ElementOptions.OpenRowEmphases, emphasis);
            int picked = DrawSegmentRow("Emphasis", at, ElementOptions.OpenRowEmphasisLabels, CardOpenRowEmphasisHelp, IndentLevel1, "Card components open row emphasis", 0);
            if (picked >= 0 && picked != at) components.open_row_emphasis = ElementOptions.OpenRowEmphases[picked];

            DrawEditorRow(out float titleRow, out _);
            GUILayout.Label("Page Chrome", EditorStyles.miniBoldLabel, GUILayout.Width(Mathf.Max(40f, titleRow - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw("Page Chrome", CardPageChromeHelp);
            EditorRowEnd();
            components.page_chrome_breadcrumb = DrawToggleField("Breadcrumb", components.page_chrome_breadcrumb, CardPageChromeBreadcrumbHelp, IndentLevel1);
            components.page_chrome_prev_next = DrawToggleField("Previous / Next", components.page_chrome_prev_next, CardPageChromePrevNextHelp, IndentLevel1);

            DrawDomainTestSubSection(_cardComponentsTest, CardComponentsSceneTestGuide, CardComponentsPlaymodeTestGuide, CardComponentsDeviceTestGuide);
        }

        private const string CardOpenRowHelp =
            "The last line of a block's preview: words, the reading time and a chevron, one tap target that opens the block's page. Its words " +
            "are the card's (Detail Card > Card Texts); its emphasis is set here for every block. A block can differ: Specific Marker > Card " +
            "Content > that block > Preview > Open Row, Override.";

        private const string CardOpenRowEmphasisHelp =
            "Nudge (the default): the chevron steps towards the page now and then -- it says open me without a glow. Shine: a light crosses the " +
            "words. Plain: still. A visitor who reduces motion always sees Plain.";

        private const string CardPageChromeHelp =
            "The top bar and the foot of a block's own page: what opens when a visitor taps a block's preview or a row's open-in-full-view " +
            "button. The top bar is a Back arrow and, beside it, the breadcrumb; the foot is two cards that turn to the neighbouring block. " +
            "These two switches are the wall's choice for every block page. A block that composes its own page can differ: Specific " +
            "Marker > Card Content > that block > Page > Page Chrome, Override.";

        private const string CardPageChromeBreadcrumbHelp =
            "On (the default): a one-line trail beside the Back arrow -- the point, the section and the block's own name in bold (left out " +
            "for a page that shows a big title of its own, a story); the section is a button that moves the card to it. When the trail is too long the section is cut first, then the point's name. " +
            "Off: the page shows the Back arrow alone.";

        private const string CardPageChromePrevNextHelp =
            "On (the default): two cards at the end of the page, Previous on the left and Next on the right, each naming the neighbouring " +
            "block; a block with no neighbour shows one card. Off: no cards -- a sideways swipe and the chips under the page still turn it.";

        private const string CardComponentsSceneTestGuide =
            "Not possible in Scene test: the card is screen-space UI and only draws in Play Mode. Use How to Playmode Test.";

        private const string CardComponentsPlaymodeTestGuide =
            "SETUP\n" +
            "- Save All to JSON, then Copy to StreamingAssets; open the wall scene, press Play.\n" +
            "- Tap a point that has several blocks of one section, drag its card up to full, and tap a block's preview or its open button.\n\n" +
            "OPEN ROW\n" +
            "- At half, the preview of a block that opens a page ends with its Open Row (words, minutes, a chevron).\n" +
            "- Emphasis Nudge: the chevron steps towards the page now and then. Shine: a light crosses the words. Plain: still. A block with its own Open Row override keeps its own.\n\n" +
            "PAGE CHROME\n" +
            "- Breadcrumb on: next to the Back arrow one line reads the point, the section and the block (the block in bold; a page with a big title of its own, a story, ends with the section). Tap the section: the card moves there.\n" +
            "- Previous / Next on: scroll to the end of the page; two cards name the neighbouring blocks. Tap Next: the next block's page opens, and its Previous names the one you left.\n" +
            "- Untick Breadcrumb here: the Back arrow stands alone. Untick Previous / Next: the cards are gone, a sideways swipe still turns the page.\n" +
            "- Press Back (or Escape): the card is where you left it. A block with its own Page Chrome override follows that block's values, not these.";

        private const string CardComponentsDeviceTestGuide =
            "SETUP\n" +
            "- Save All to JSON, Copy to StreamingAssets, Build And Run.\n\n" +
            "PAGE CHROME\n" +
            "- Open a block's page with a real finger: the Back arrow is easy to hit with a thumb, the breadcrumb stays on one line with a long point name, and a full-width picture reaches both edges of the screen.";
    }
}
