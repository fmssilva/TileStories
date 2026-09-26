// POIEditorToolWindow.DetailCardHelp.cs
//
// Partial: every text of the Detail Card tab and of Specific Marker > Card Content -- option labels, (i) help
// texts and the Scene / Playmode / Device Test guides. Framework-authored and app-agnostic: they name Editor Tab
// controls, never a project doc, a code file or one wall's own content (_5.1_Editor_Tab.md, "Domain Manual Tests").

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
            "- Languages: put another language first; the card shows that language (a missing text falls back).\n" +
            "- Test Runner: EditMode + PlayMode, zero failures.";

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

        // Why a block of Card Content would not show (BlockStackBuilder's reason), in the Editor's words
        internal static string CardBlockSkipText(BlockStackBuilder.SkipReason reason, string fieldLabel) => reason switch
        {
            BlockStackBuilder.SkipReason.UnknownKind => "Not shown: no block kind of this name is registered.",
            BlockStackBuilder.SkipReason.KindDisabled => "Not shown: this kind is switched off in Detail Card > Block Library.",
            BlockStackBuilder.SkipReason.MissingRequired => "Not shown: " + fieldLabel + " is empty in every language.",
            BlockStackBuilder.SkipReason.ExtraHeader => "Not shown: a card has one Header; the first one is used.",
            _ => "Not shown.",
        };
    }
}
