// POIEditorToolWindow.DetailCardHelp.cs
//
// Partial: every text of the Detail Card tab and of Specific Marker > Card Content -- option labels, (i) help
// texts and the Scene / Playmode / Device Test guides. Framework-authored and app-agnostic: they name Editor Tab
// controls, never a project doc, a code file or one wall's own content (_5.1_Editor_Tab.md, "Domain Manual Tests").

using System.Collections.Generic;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // Open At labels, in CardOptions.OpenStops order
        private static readonly string[] CardOpenStopLabels = { "Peek (title only)", "Half" };

        private const string CardEnabledHelp =
            "Whether tapping a point opens its Detail Card. Off: a tap still selects the point (highlight, zoom), no card opens. " +
            "The settings below are kept while it is off.";

        private const string CardLanguagesHelp =
            "The languages the card is written in, comma-separated language codes (en, pt, es...). Every text of Card Content " +
            "gets one row per language. The FIRST language is the fallback: a text missing in the visitor's language shows the " +
            "first language's text instead (and any language that has text when that is missing too).";

        private const string CardNoLanguageNote =
            "No language: card texts cannot be written. Add at least one language code in Languages.";

        private const string CardMediaFolderHelp =
            "Where this wall's card images and sounds live: a folder inside a Resources folder of the wall, written as the part " +
            "after Resources/ (a framework default suggestion: <WallName>/CardMedia). A card media field stores a path inside it. " +
            "Empty: the wall has no card media yet.";

        private const string CardOpenAtHelp =
            "How far the card opens when a point is tapped. Peek: only the title and the category chip, over the camera view; " +
            "the visitor drags it up for more. Half: the title, the subtitle and the start of the content. A card never opens " +
            "full by itself.";

        private const string CardHalfHeightHelp =
            "The most of the screen height the Half stop may cover (0.25 to 0.40). Keeps the wall visible above the card; " +
            "dragging up again opens it full.";

        private const string CardTapOutsideHelp =
            "A tap on empty camera space closes the card and clears the selection. A tap on a marker still selects that marker; " +
            "a drag across the view never closes it. The X and a swipe down always close it.";

        private const string CardKeepAudioHelp =
            "For audio blocks: the sound goes on in a small player after the card closes. No built-in block plays audio yet, " +
            "so this has no effect today.";

        private const string BlockLibraryHelp =
            "Every block kind this wall can show, wall-wide. Enabled: off hides every block of that kind on every card (Header " +
            "is the card's title and cannot be switched off). Default Variant: the look a block gets when Card Content does not " +
            "pick one. Details: your own note. The (i) of a row explains that kind.";

        private const string CardContentHelp =
            "This point's Detail Card, top to bottom. The card always starts with a Header: with none here, it shows this " +
            "point's name as the title and its Summary as the subtitle. Add a block with Add Block; reorder with the arrows; open " +
            "a row to write its texts, one per language of Detail Card > Card Container > Languages. A row that would not show " +
            "says why under it.";

        private const string CardContentVariantHelp =
            "The look of this block. Block Library default: the Default Variant of Detail Card > Block Library.";

        private const string CardContentDisplayHelp =
            "How the block sits on the card. Inline: the whole block in the card.";

        private const string CardContentOffNote =
            "The Detail Card is off for this wall (Detail Card > Card Container > Enable Detail Card): this content is kept but not shown.";

        private const string CardSceneTestGuide =
            "Not possible in Scene test: the card is screen-space UI and only draws in Play Mode. Use How to Playmode Test.";

        private const string CardPlaymodeTestGuide =
            "SETUP\n" +
            "- Save All to JSON, then Copy to StreamingAssets (Play reads that copy).\n" +
            "- Open the wall scene, press Play. Card edits are not live yet: stop, save, copy and Play again after a change.\n\n" +
            "CARD CONTAINER\n" +
            "- Tap a marker: the card opens at Open At (Peek: the title and the category chip only).\n" +
            "- Drag the grabber or the title up: Half, never taller than Half Height Max; drag again: full.\n" +
            "- Close it three ways: the X; drag it down below the title; tap empty camera space (only while Tap Outside Closes is on).\n" +
            "- Tap another marker while it is open: the card keeps its height and shows the new point.\n" +
            "- Enable Detail Card off: a tap still selects the marker, no card opens.\n" +
            "- Languages: put another language first; the card shows that language (a missing text falls back).\n\n" +
            "QUESTIONS AND FEEDBACK\n" +
            "- Answer a Knowledge Check: a right answer shows the confirmation and the explanation; a wrong one shows \"Actually...\", " +
            "the explanation and marks the right choice. Close the card and open it again: the answer is still there and cannot be changed.\n" +
            "- Show After Reading on: the question stays hidden until you have scrolled to the end of the card (or all of it fits on the screen).\n" +
            "- Give a Feedback block a thumb or stars: it shows the vote with a thank-you, once.\n" +
            "- Reset Saved Card State (above) forgets every saved answer, vote and revealed question of this wall on this computer: close " +
            "and reopen the card afterwards to answer again.\n" +
            "- Test Runner: EditMode + PlayMode, zero failures.";

        private const string CardStateResetHelp =
            "The card remembers on this device what a visitor did: the answer to each Knowledge Check question, the vote of each " +
            "Feedback block and the questions that were revealed after reading. This clears all of it for this wall, on this " +
            "computer only, so a question can be answered again. It never changes the config. Close and reopen a card that is open " +
            "in Play Mode to see the cleared state.";

        // Why one question row of a Knowledge Check is not shown for the look it will be drawn in
        internal static string CardQuestionProblemText(int rowNumber, KnowledgeCheckRule.Problem problem, string variant)
        {
            string row = "Row " + rowNumber + " of Questions is not shown: ";
            bool pictures = variant == BuiltInBlocks.KnowledgeCheckImageChoice;
            switch (problem)
            {
                case KnowledgeCheckRule.Problem.NoQuestion: return row + "its Question is empty.";
                case KnowledgeCheckRule.Problem.NoExplanation:
                    return row + "its Explanation is empty (every question needs one: it is what the visitor reads after answering).";
                case KnowledgeCheckRule.Problem.TooFewOptions:
                    return pictures
                        ? row + "Image Choice needs at least two pictures inside the Media Folder (Picture 1 to Picture 4)."
                        : row + "it needs at least two options with words (Option 1 to Option 4).";
                case KnowledgeCheckRule.Problem.NoCorrect: return row + "no Right Option is picked.";
                case KnowledgeCheckRule.Problem.CorrectIsEmpty:
                    return row + "its Right Option is one this look does not show" + (pictures ? " (a slot with no picture)." : " (an option with no words).");
                default: return row + "it is incomplete.";
            }
        }

        private const string CardDeviceTestGuide =
            "SETUP\n" +
            "- USB or Wi-Fi: adb pair <ip>:<port> + code, then adb connect (or npx adb-qr-connect).\n" +
            "- Save All to JSON, Copy to StreamingAssets, Build And Run.\n" +
            "- Logs: adb logcat -c, then adb logcat -s Unity > __logcat.txt, and read the file.\n\n" +
            "CARD CONTAINER\n" +
            "- Swipe the card up and down with a real finger: it follows the finger, a quick flick goes one stop further.\n" +
            "- The X and the grabber are easy to reach with one thumb; the card clears the home indicator.\n" +
            "- Read the title and the chip outdoors in bright light.";

        private const string BlockLibrarySceneTestGuide =
            "Not possible in Scene test: blocks only draw on the card in Play Mode. Use How to Playmode Test.";

        private const string BlockLibraryPlaymodeTestGuide =
            "SETUP\n" +
            "- Save All to JSON, then Copy to StreamingAssets; open the wall scene, press Play.\n\n" +
            "BLOCK LIBRARY\n" +
            "- Untick Enabled for a kind a point uses (Specific Marker > Card Content): that block is gone from every card.\n" +
            "- Pick another Default Variant: every block of that kind set to Block Library default takes it.\n" +
            "- Specific Marker > Card Content says under a row why that block would not show.";

        private const string BlockLibraryDeviceTestGuide =
            "Nothing device-specific: the library only decides which blocks draw. Check it on a device with the Card Container guide.";

        private const string CardTextsHelp =
            "The card's own words that no block holds: button names, small headings, hints. Each row is one text, named by " +
            "its framework wording; under it, one field per language of Card Container > Languages. Type your wall's wording " +
            "to replace the framework's; leave a field empty to keep the framework's. The (i) of a row says where the card " +
            "shows it.";

        private const string CardTextsMissingNote =
            "The framework's card texts were not found in the project. Reimport the framework (the card still works, but its " +
            "buttons and small headings would have no words).";

        private const string CardTextsSceneTestGuide =
            "Not possible in Scene test: the card is screen-space UI and only draws in Play Mode. Use How to Playmode Test.";

        private const string CardTextsPlaymodeTestGuide =
            "SETUP\n" +
            "- Save All to JSON, then Copy to StreamingAssets; open the wall scene, press Play. Card edits are not live yet: " +
            "stop, save, copy and Play again after a change.\n\n" +
            "CARD TEXTS\n" +
            "- Type your own wording for Close in the first language; tap a marker; point at the round close button: its " +
            "name is your wording.\n" +
            "- Empty the field again: the framework's wording is back.\n" +
            "- Put another language first in Card Container > Languages: every card text is in that language.";

        private const string CardTextsDeviceTestGuide =
            "Nothing device-specific: the texts are the same on every device. Check them on a device with the Card Container guide.";

        private const string CardGlossaryHelp =
            "Words a visitor can tap in a card text to read what they mean. A card text links a word by writing it between " +
            "double brackets, [[keep]]; to show other words for the same entry write [[shown words|keep]]. Term: the word " +
            "written between the brackets (capital letters do not matter). Definition: one text per language of Card Container " +
            "> Languages. A linked word with no row here shows as plain text, and Card Content warns under its block.";

        private const string CardGlossaryDefinitionHelp =
            "What the visitor reads when they tap the word: one or two short sentences.";

        private const string CardGlossarySceneTestGuide =
            "Not possible in Scene test: the card is screen-space UI and only draws in Play Mode. Use How to Playmode Test.";

        private const string CardGlossaryPlaymodeTestGuide =
            "SETUP\n" +
            "- Add a term here, write [[the term]] in a Rich Text block of a point (Specific Marker > Card Content).\n" +
            "- Save All to JSON, then Copy to StreamingAssets; open the wall scene, press Play.\n\n" +
            "GLOSSARY\n" +
            "- Tap the point's marker, drag the card up to full: the word is underlined.\n" +
            "- Tap the word: its definition opens under the paragraph. Tap the word again (or the definition): it closes.\n" +
            "- Delete the term here, save, copy, Play again: the word is plain text and cannot be tapped.";

        private const string CardGlossaryDeviceTestGuide =
            "SETUP\n" +
            "- Save All to JSON, Copy to StreamingAssets, Build And Run.\n\n" +
            "GLOSSARY\n" +
            "- Tap a linked word with a real finger: the definition opens; a slow drag across the text scrolls the card instead.";

        // A Color field holds text that is not a colour the card accepts
        internal static string CardColorInvalidText(string fieldLabel, string typed) =>
            fieldLabel + " '" + typed.Trim() + "' is not a colour: write it as #RRGGBB (for example #1F3F8F), or pick it. " +
            "Until then the card leaves this row out.";

        // A Url field holds text that is not a link the card opens (WebLinkRule)
        internal static string CardUrlInvalidText(string fieldLabel, string typed) =>
            fieldLabel + " '" + typed.Trim() + "' is not a web link the card opens: write the whole address, starting with https:// " +
            "(for example https://maps.example.org/place). Until then the card shows no button for it.";

        // A Compare Points block names its own point in Compare With
        internal const string CardCompareWithItselfNote =
            "Compare With is this point itself: the card shows the same condition twice. Pick another point.";

        // A Show On Wall block on a card whose sticky call to action already lowers the card to the wall (_3.1 [SHOULD])
        internal const string CardShowOnWallRepeatsStickyText =
            "The Sticky button of this card's Actions block already does what this Show On Wall block does, so the card offers the same " +
            "button twice. Delete one of the two.";

        // A Poll block holds more options with words than it shows
        internal static string CardPollExtraOptionsText(int hidden) =>
            "A poll shows at most " + PollRule.MaxOptions + " options: the last " + hidden + (hidden == 1 ? " option is" : " options are") + " not shown. Delete " +
            (hidden == 1 ? "it" : "them") + " or merge options.";

        // A Dialogue block has rows that will not show, listed by their row number in the Lines table
        internal static string CardDialogueEmptyRowsText(IReadOnlyList<int> rows) =>
            "Not shown: " + (rows.Count == 1 ? "line " : "lines ") + string.Join(", ", rows) + " (a row with no words in Line).";

        // A Dialogue block holds a Reply whose Choice has no words, so the reply can never be picked
        internal static string CardDialogueOrphanReplyText(IReadOnlyList<int> rows) =>
            "A reply in " + (rows.Count == 1 ? "line " : "lines ") + string.Join(", ", rows) + " has no words in its Choice, so the visitor can never pick it. " +
            "Write the Choice, or clear the Reply.";

        // A block with Show After Reading on is not the last content block of the card
        internal const string CardShowAfterReadingMidCardNote =
            "Show After Reading is on, but this block is not the last one before Sources: it appears when the visitor reaches the end of the card, " +
            "so here it would appear above them, where nobody is looking, and push the blocks below it down. Move it to the end of the card " +
            "(only Sources may follow it), or turn Show After Reading off.";

        // An Actions block in the Sticky look holds more buttons than the one it shows
        internal static string CardStickyExtraButtonsText(int hidden) =>
            "Sticky shows only the first button: the other " + hidden + (hidden == 1 ? " row is" : " rows are") +
            " not shown. Pick Circles or Pill Row to show every button.";

        // A block text links a word the Glossary does not have
        internal static string CardGlossaryMissingText(IReadOnlyList<string> terms) =>
            "Not in the Glossary (Detail Card > Glossary), shown as plain text: " + string.Join(", ", terms) + ".";

        // Why a block of Card Content would not show (BlockStackBuilder's reason), in the Editor's words. `field` is the
        // field the reason names (MissingRequired, NoCompleteRow): the words follow its type.
        internal static string CardBlockSkipText(BlockStackBuilder.SkipReason reason, BlockFieldDefinition field, string notForThisPointNote = null) => reason switch
        {
            BlockStackBuilder.SkipReason.NotForThisPoint => "Not shown: " + (notForThisPointNote ?? "this kind has nothing to show for this point."),
            BlockStackBuilder.SkipReason.UnknownKind => "Not shown: no block kind of this name is registered.",
            BlockStackBuilder.SkipReason.KindDisabled => "Not shown: this kind is switched off in Detail Card > Block Library.",
            BlockStackBuilder.SkipReason.MissingRequired => "Not shown: " + CardEmptyFieldText(field) + ".",
            BlockStackBuilder.SkipReason.NoCompleteRow => "Not shown: " + CardNoCompleteRowText(field) + ".",
            BlockStackBuilder.SkipReason.InvalidMedia => "Not shown: " + CardMediaProblemText(field?.Label ?? "the picture", field?.Media ?? MediaKind.Image, MediaPathProblem.None) + ".",
            BlockStackBuilder.SkipReason.ExtraHeader => "Not shown: a card has one Header; the first one is used.",
            _ => "Not shown.",
        };

        private static string CardEmptyFieldText(BlockFieldDefinition field)
        {
            string label = field?.Label ?? "a required field";
            return field?.Type switch
            {
                BlockFieldType.LocalizedText or BlockFieldType.LocalizedLongText => label + " is empty in every language",
                BlockFieldType.Items => label + " has no rows",
                BlockFieldType.Choice or BlockFieldType.PoiRef => label + " is not picked",
                BlockFieldType.Color => label + " is not a colour written as #RRGGBB",
                _ => label + " is empty",
            };
        }

        // "no row of Swatches is complete: each needs Name and Colour (a colour written as #RRGGBB)"
        private static string CardNoCompleteRowText(BlockFieldDefinition field)
        {
            var needed = new List<string>();
            bool colour = false, picture = false;
            foreach (var sub in field?.ItemFields ?? System.Array.Empty<BlockFieldDefinition>())
            {
                if (!sub.Required) continue;
                needed.Add(sub.Label);
                colour |= sub.Type == BlockFieldType.Color;
                picture |= sub.Type == BlockFieldType.Asset;
            }
            return "no row of " + (field?.Label ?? "the list") + " is complete: each needs " + string.Join(" and ", needed)
                   + (colour ? " (a colour written as #RRGGBB)" : "") + (picture ? " (a picture from the Media Folder)" : "");
        }

        // What is wrong with a picture field, for the warning under it and the "Not shown" line. `problem` None = say
        // what the field takes (the builder's reason carries no detail).
        internal static string CardMediaProblemText(string label, MediaKind kind, MediaPathProblem problem)
        {
            string types = string.Join(" / ", MediaPathRule.ImageExtensions).Replace(".", "").ToUpperInvariant();
            return problem switch
            {
                MediaPathProblem.OutsideFolder => label + " is outside the wall's Media Folder (Detail Card > Card Container): the app cannot load it. Move the file into that folder and pick it again",
                MediaPathProblem.WrongType => label + " is not a picture the card can show (" + types + ")",
                MediaPathProblem.Empty => label + " is empty",
                _ => label + " must be a " + types + " picture inside the wall's Media Folder (Detail Card > Card Container)",
            };
        }

        // A header picture look that lacks what it needs (Picture; Split Then Now: both pictures)
        internal static string CardHeaderNeedsPictureText(string variant) =>
            "The " + variant + " look needs " + (variant == BuiltInBlocks.HeaderSplitThenNow ? "Picture and Second Picture" : "Picture") +
            " (pictures inside the Media Folder): until then the card shows the text-only look.";

        // A picture path that is fine by the rule but has no file behind it (deleted or renamed after it was picked)
        internal static string CardMediaMissingText(string label, string path) =>
            label + ": no picture \"" + path + "\" in the Media Folder any more. The card shows its \"picture unavailable\" frame until another is picked.";
    }
}
