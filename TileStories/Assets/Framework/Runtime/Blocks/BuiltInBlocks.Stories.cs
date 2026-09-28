using System.Collections.Generic;

namespace TileStories
{
    // The stories family: the point's story told in time, people and chapters, and beside another point.
    // Part of BuiltInBlocks (BuiltInBlocks.cs registers every kind, in catalog order): each kind's key, variant and
    // field constants first, then the definitions.
    public static partial class BuiltInBlocks
    {
        public const string TimelineKind = "timeline";
        public const string TimelineVertical = "vertical";
        public const string TimelineHorizontal = "horizontal";
        public const string TimelineItemsField = "events";
        public const string TimelineDateField = "date";
        public const string TimelineTitleField = "title";
        public const string TimelineTextField = "text";
        public const string TimelineHighlightNowField = "highlight_now";

        public const string PersonKind = "person";
        public const string PersonRow = "row";
        public const string PersonCard = "card";
        public const string PersonNameField = "name";
        public const string PersonRoleField = "role";
        public const string PersonTextField = "text";

        public const string StoryChaptersKind = "story_chapters";
        public const string StoryChaptersSegmented = "segmented";
        public const string StoryChaptersItemsField = "chapters";
        public const string StoryChaptersTitleField = "title";
        public const string StoryChaptersBodyField = "body";

        public const string ComparePointsKind = "compare_points";
        public const string ComparePointsRings = "rings";
        public const string ComparePointsOtherField = "other";
        public const string ComparePointsAxisField = "axis";
        // What two points are compared by. Only the condition today; a later axis joins this list with its view.
        public const string CompareAxisStatus = "status";

        // Tier 1: dated events in order. then_now_pairs (two pictures per event) arrives with Tier 2 images.
        public static readonly BlockKindDefinition Timeline = new()
        {
            Key = TimelineKind,
            Family = "stories",
            DisplayName = "Timeline",
            Help = "What happened to the point, in order: each event a date, a title and an optional text. Vertical: the events " +
                   "down the card on one line, oldest first. Horizontal: the events side by side on a line the visitor swipes " +
                   "along. Highlight Now: the line ends in a Now point after the last event (its word is Detail Card > Card " +
                   "Texts). A row with no date or no title is not shown. " + LongTextHelp,
            Variants = new[] { TimelineVertical, TimelineHorizontal },
            DefaultVariant = TimelineVertical,
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = TimelineItemsField, Type = BlockFieldType.Items, Label = "Events", Required = true,
                    Help = "One row per event, in the order they happened (the card keeps the rows' order; it does not sort dates).",
                    ItemFields = new[]
                    {
                        new BlockFieldDefinition { Key = TimelineDateField, Type = BlockFieldType.LocalizedText, Label = "Date", Required = true, Help = "When it happened, as the visitor should read it (1147, c. 1700, 1 Nov 1755)." },
                        new BlockFieldDefinition { Key = TimelineTitleField, Type = BlockFieldType.LocalizedText, Label = "Title", Required = true, Help = "What happened, in a few words (The earthquake)." },
                        new BlockFieldDefinition { Key = TimelineTextField, Type = BlockFieldType.LocalizedLongText, Label = "Text", Help = "A sentence or two more. May stay empty. " + LongTextHelp },
                    },
                },
                new BlockFieldDefinition
                {
                    Key = TimelineHighlightNowField, Type = BlockFieldType.Toggle, Label = "Highlight Now",
                    Help = "On: the line ends in a highlighted Now point after the last event, so the visitor sees how far the story " +
                           "reaches into today. Off: it ends at the last event.",
                },
            },
        };

        // Tier 1: someone in the point's story. The photo arrives with Tier 2 images (the initial holds its place).
        public static readonly BlockKindDefinition Person = new()
        {
            Key = PersonKind,
            Family = "stories",
            DisplayName = "Person",
            Help = "Someone in the point's story (who built it, who lived there, who painted it): a name, their role and a few " +
                   "lines about them, beside the first letter of the name. Row: in line with the rest of the card. Card: set " +
                   "apart on its own panel, the name larger. Role and Text may stay empty. " + LongTextHelp,
            Variants = new[] { PersonRow, PersonCard },
            DefaultVariant = PersonRow,
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition { Key = PersonNameField, Type = BlockFieldType.LocalizedText, Label = "Name", Required = true, Help = "The person's name as the visitor should read it (King Manuel I)." },
                new BlockFieldDefinition { Key = PersonRoleField, Type = BlockFieldType.LocalizedText, Label = "Role", Help = "Who they were for this point, in a few words (Moved the court to the river, 1511)." },
                new BlockFieldDefinition { Key = PersonTextField, Type = BlockFieldType.LocalizedLongText, Label = "Text", Help = "A few lines about them. " + LongTextHelp },
            },
        };

        // Tier 1: a story told in chapters, one at a time. Chapter pictures arrive with Tier 2 images.
        public static readonly BlockKindDefinition StoryChapters = new()
        {
            Key = StoryChaptersKind,
            Family = "stories",
            DisplayName = "Story Chapters",
            Help = "A longer story told one chapter at a time, so the card stays short. Segmented: a bar of one segment per " +
                   "chapter shows where the visitor is; Previous and Next buttons turn the chapters (their words and the " +
                   "\"Chapter 1 of 3\" line are Detail Card > Card Texts). The block always opens at the first chapter. A row " +
                   "with no title or no text is not shown. " + LongTextHelp,
            Variants = new[] { StoryChaptersSegmented },
            DefaultVariant = StoryChaptersSegmented,
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = StoryChaptersItemsField, Type = BlockFieldType.Items, Label = "Chapters", Required = true,
                    Help = "One row per chapter, first chapter first. Two to five chapters of a short paragraph or two each read best.",
                    ItemFields = new[]
                    {
                        new BlockFieldDefinition { Key = StoryChaptersTitleField, Type = BlockFieldType.LocalizedText, Label = "Title", Required = true, Help = "The chapter's short title (The siege)." },
                        new BlockFieldDefinition { Key = StoryChaptersBodyField, Type = BlockFieldType.LocalizedLongText, Label = "Text", Required = true, Help = "The chapter itself. " + LongTextHelp },
                    },
                },
            },
        };

        // Tier 1: this point beside another one of the wall, by their condition
        public static readonly BlockKindDefinition ComparePoints = new()
        {
            Key = ComparePointsKind,
            Family = "stories",
            DisplayName = "Compare Points",
            Help = "This point beside another point of the wall, by their condition: two outlines drawn exactly as their markers " +
                   "draw them, each with the point's title and its condition's name. Rings: the two side by side. With Heading empty " +
                   "the card titles the block with its Card Texts wording (Side by side). Shown only while both points have a status " +
                   "and the other point is on the wall.",
            Variants = new[] { ComparePointsRings },
            DefaultVariant = ComparePointsRings,
            DisplayModes = new[] { CardOptions.DisplayInline },
            DefaultHeadingKey = CardStrings.Keys.CompareHeading,
            ShowsFor = (poi, block, _, wallPois) => poi != null && poi.has_status && OtherPoi(block, wallPois) is { has_status: true },
            NotShownForPoiNote = "this point or the point it is compared with has no status (Specific Marker > Outline > Has status), " +
                                 "or Compare With names a point that is no longer on this wall.",
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = ComparePointsOtherField, Type = BlockFieldType.PoiRef, Label = "Compare With", Required = true,
                    Help = "The other point of this wall, as the POI list names it. Pick a point with a status; one that is " +
                           "deleted later shows as (missing) here and the block is not shown.",
                },
                new BlockFieldDefinition
                {
                    Key = ComparePointsAxisField, Type = BlockFieldType.Choice, Label = "Compare By",
                    Options = new[] { CompareAxisStatus }, OptionLabels = new[] { "Condition" },
                    Help = "What the two points are compared by. Condition (the outline of Global Scene > Outline) is the only one " +
                           "today; (none) means Condition too.",
                },
            },
        };

        // The POI a compare block points at, among the wall's POIs (null: none picked, gone, or no wall at hand)
        public static POIData OtherPoi(BlockInstanceData block, IReadOnlyList<POIData> wallPois)
        {
            string id = new BlockFieldReader(block, null, null).Value(ComparePointsOtherField);
            if (string.IsNullOrWhiteSpace(id) || wallPois == null) return null;
            for (int i = 0; i < wallPois.Count; i++)
                if (wallPois[i] != null && wallPois[i].id == id) return wallPois[i];
            return null;
        }

        // ---------------- dialogue (step 8B) ----------------

        public const string DialogueKind = "dialogue";
        public const string DialogueChoices = "choices";
        public const string DialogueLinesField = "lines";
        public const string DialogueSpeakerField = "speaker";
        public const string DialogueTextField = "line";
        // choice_1 .. choice_3 and reply_1 .. reply_3 (DialogueRule.MaxChoices): a line row cannot nest a list
        public const string DialogueChoicePrefix = "choice_";
        public const string DialogueReplyPrefix = "reply_";

        // The fields of one dialogue row: who speaks, what is said, and up to three replies the visitor may pick
        private static BlockFieldDefinition[] DialogueRowFields()
        {
            var fields = new List<BlockFieldDefinition>
            {
                new BlockFieldDefinition { Key = DialogueSpeakerField, Type = BlockFieldType.LocalizedText, Label = "Speaker", Help = "Who says the line (The mason). May stay empty." },
                new BlockFieldDefinition { Key = DialogueTextField, Type = BlockFieldType.LocalizedText, Label = "Line", Required = true, Help = "What is said. A row with no words is not shown." },
            };
            for (int slot = 1; slot <= DialogueRule.MaxChoices; slot++)
                fields.Add(new BlockFieldDefinition
                {
                    Key = DialogueRule.ChoiceField(slot), Type = BlockFieldType.LocalizedText, Label = "Choice " + slot,
                    Help = "A reply the visitor may pick after this line (leave all three empty for a plain line). A choice with no words is not offered.",
                });
            for (int slot = 1; slot <= DialogueRule.MaxChoices; slot++)
                fields.Add(new BlockFieldDefinition
                {
                    Key = DialogueRule.ReplyField(slot), Type = BlockFieldType.LocalizedText, Label = "Reply " + slot,
                    Help = "What the speaker answers when the visitor picks Choice " + slot + ". May stay empty: the conversation then simply goes on.",
                });
            return fields.ToArray();
        }

        // Tier 3: a conversation revealed one line at a time, a line may offer a few replies
        public static readonly BlockKindDefinition Dialogue = new()
        {
            Key = DialogueKind,
            Family = "stories",
            DisplayName = "Dialogue",
            Help = "A short conversation told one line at a time: the first line shows, and each tap on Continue shows the next. A line may " +
                   "offer up to three replies; the visitor picks one, it appears as their own line, and the speaker's answer follows, then " +
                   "the conversation goes on. One level of choices only. Where the visitor has got to is not remembered: the block starts " +
                   "again each time the card opens. The Continue and Start again words are Detail Card > Card Texts. A row with no words " +
                   "is not shown.",
            Variants = new[] { DialogueChoices },
            DefaultVariant = DialogueChoices,
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = DialogueLinesField, Type = BlockFieldType.Items, Label = "Lines", Required = true,
                    Help = "One row per line, in the order they are said.",
                    ItemFields = DialogueRowFields(),
                },
            },
        };
    }
}
