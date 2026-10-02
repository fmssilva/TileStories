using System.Collections.Generic;

namespace TileStories
{
    // The play family: what the visitor does on the card rather than reads (_3.1 Tier 3). Part of BuiltInBlocks
    // (BuiltInBlocks.cs registers every kind, in catalog order): each kind's key, variant and field constants first, then the
    // definitions. What a visitor answers is remembered by CardLocalState.
    public static partial class BuiltInBlocks
    {
        public const string KnowledgeCheckKind = "knowledge_check";
        public const string KnowledgeCheckMultipleChoice = "multiple_choice";
        public const string KnowledgeCheckTrueFalseSwipe = "true_false_swipe";
        public const string KnowledgeCheckImageChoice = "image_choice";
        public const string KnowledgeCheckQuestionsField = "questions";
        public const string KnowledgeCheckQuestionField = "question";
        // option_1 .. option_4 and image_1 .. image_4 (KnowledgeCheckRule.MaxOptions): a question row cannot nest a list
        public const string KnowledgeCheckOptionPrefix = "option_";
        public const string KnowledgeCheckImagePrefix = "image_";
        public const string KnowledgeCheckCorrectField = "correct";
        public const string KnowledgeCheckIsTrueField = "is_true";
        public const string KnowledgeCheckExplanationField = "explanation";
        public const string KnowledgeCheckShowAfterViewedField = "show_after_viewed";

        // The fields of one question row: the question, four options with their pictures, the right one, the explanation
        private static BlockFieldDefinition[] KnowledgeCheckRowFields()
        {
            var fields = new List<BlockFieldDefinition>
            {
                new BlockFieldDefinition
                {
                    Key = KnowledgeCheckQuestionField, Type = BlockFieldType.LocalizedText, Label = "Question", Required = true,
                    Help = "What the visitor is asked. For True / False Swipe, the statement to judge.",
                },
            };
            var slots = new List<string>();
            var slotLabels = new List<string>();
            for (int slot = 1; slot <= KnowledgeCheckRule.MaxOptions; slot++)
            {
                slots.Add(slot.ToString());
                slotLabels.Add("Option " + slot);
                fields.Add(new BlockFieldDefinition
                {
                    Key = KnowledgeCheckRule.OptionField(slot), Type = BlockFieldType.LocalizedText, Label = "Option " + slot,
                    ShownWhen = FieldShownWhen.Looks(KnowledgeCheckMultipleChoice, KnowledgeCheckImageChoice),
                    Help = "Multiple Choice: an answer the visitor can pick (two to four; a blank option is not shown). Image Choice: a caption " +
                           "under the picture (optional). True / False Swipe: not used.",
                });
            }
            for (int slot = 1; slot <= KnowledgeCheckRule.MaxOptions; slot++)
                fields.Add(new BlockFieldDefinition
                {
                    Key = KnowledgeCheckRule.ImageField(slot), Type = BlockFieldType.Asset, Media = MediaKind.Image, Label = "Picture " + slot,
                    ShownWhen = FieldShownWhen.Looks(KnowledgeCheckImageChoice),
                    Help = "Image Choice only: the picture of option " + slot + " (two to four; a slot with no picture is not shown). " + PictureHelp,
                });
            fields.Add(new BlockFieldDefinition
            {
                Key = KnowledgeCheckCorrectField, Type = BlockFieldType.Choice, Label = "Right Option", Options = slots, OptionLabels = slotLabels,
                ShownWhen = FieldShownWhen.Looks(KnowledgeCheckMultipleChoice, KnowledgeCheckImageChoice),
                Help = "Multiple Choice and Image Choice: which option is the right one. It must be an option that is shown, or the question is left out. " +
                       "True / False Swipe: not used.",
            });
            fields.Add(new BlockFieldDefinition
            {
                Key = KnowledgeCheckIsTrueField, Type = BlockFieldType.Toggle, Label = "Statement Is True",
                ShownWhen = FieldShownWhen.Looks(KnowledgeCheckTrueFalseSwipe),
                Help = "True / False Swipe only: tick when the statement is true (a swipe to the right is right), leave clear when it is false. Not used by the other looks.",
            });
            fields.Add(new BlockFieldDefinition
            {
                Key = KnowledgeCheckExplanationField, Type = BlockFieldType.LocalizedText, Label = "Explanation", Required = true,
                Help = "Shown after the answer, whether it was right or wrong: why the answer is what it is.",
            });
            return fields.ToArray();
        }

        // Tier 3: a question the visitor answers, no marks and no penalty
        public static readonly BlockKindDefinition KnowledgeCheck = new()
        {
            Key = KnowledgeCheckKind,
            Family = "play",
            DisplayName = "Knowledge Check",
            Help = "A question the visitor answers, with no score and no penalty. A right answer shows a confirmation and the explanation; a wrong one " +
                   "a gentle \"Actually...\" and the same explanation, with the right answer marked. The answer is kept on the visitor's device, so " +
                   "the question shows as answered next time. Multiple Choice: two to four written options. True / False Swipe: a statement the " +
                   "visitor swipes right for true and left for false (two buttons do the same). Image Choice: two to four pictures to pick from. " +
                   "With more than one question the visitor moves between them with Previous and Next. With Heading empty the card titles the " +
                   "block with its Card Texts wording (Test yourself); the card's words (Correct, Actually...) are Card Texts too. Shown only " +
                   "while at least one question is complete for the chosen look.",
            Variants = new[] { KnowledgeCheckMultipleChoice, KnowledgeCheckTrueFalseSwipe, KnowledgeCheckImageChoice },
            DefaultVariant = KnowledgeCheckMultipleChoice,
            DisplayModes = new[] { CardOptions.DisplayInline },
            DefaultHeadingKey = CardStrings.Keys.KnowledgeCheckHeading,
            ShowAfterViewedField = KnowledgeCheckShowAfterViewedField,
            ShowsFor = (_, block, variant, _) => KnowledgeCheckRule.Questions(block, variant).Count > 0,
            NotShownForPoiNote = "none of its questions is complete for the chosen look (each needs a question and an explanation; Multiple Choice " +
                                 "and Image Choice also need two to four options -- pictures for Image Choice, inside the Media Folder -- and the " +
                                 "right one picked).",
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = KnowledgeCheckQuestionsField, Type = BlockFieldType.Items, Label = "Questions", Required = true,
                    Help = "One row per question, in the order the visitor meets them. A row with no question or no explanation is not shown.",
                    ItemFields = KnowledgeCheckRowFields(),
                },
                new BlockFieldDefinition
                {
                    Key = KnowledgeCheckShowAfterViewedField, Type = BlockFieldType.Toggle, Label = "Show After Reading",
                    Help = "On: the block stays hidden until the visitor has scrolled to the end of the card's content (or all of it fits on the screen), " +
                           "then it appears and stays. Off: it shows with the rest.",
                },
            },
        };

        // ---------------- poll and collect (step 8B) ----------------

        public const string PollKind = "poll";
        public const string PollBars = "bars";
        public const string PollQuestionField = "question";
        public const string PollOptionsField = "options";
        public const string PollOptionTextField = "text";

        public const string CollectKind = "collect";
        public const string CollectAddToStory = "add_to_story";
        public const string CollectItemNameField = "item_name";
        public const string CollectSeriesField = "series";

        // Tier 3: one question with a few options; the visitor's vote is kept on the device, results only when an app has them
        public static readonly BlockKindDefinition Poll = new()
        {
            Key = PollKind,
            Family = "play",
            DisplayName = "Poll",
            Help = "Asks the visitor a question with two to six options and takes one vote. The vote is kept on the visitor's device and " +
                   "reported once as a poll event; the block then shows the option the visitor picked (Your choice) and a thank-you. " +
                   "There is no server yet, so the card shows NO percentages: results bars only appear once an app supplies other " +
                   "visitors' votes. The words Your choice and the thank-you are Detail Card > Card Texts. Shown only with a question " +
                   "and at least two options that have words. Reordering the option rows changes which option an older vote points at, " +
                   "so settle the rows before the card goes live.",
            Variants = new[] { PollBars },
            DefaultVariant = PollBars,
            DisplayModes = new[] { CardOptions.DisplayInline },
            ShowsFor = (_, block, _, _) => PollRule.CanShow(new BlockFieldReader(block, null, null)),
            NotShownForPoiNote = "it needs a question and at least two options that have words.",
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = PollQuestionField, Type = BlockFieldType.LocalizedText, Label = "Question", Required = true,
                    Help = "What the visitor is asked (Which side of the wall would you visit first?).",
                },
                new BlockFieldDefinition
                {
                    Key = PollOptionsField, Type = BlockFieldType.Items, Label = "Options", Required = true,
                    Help = "One row per option, in the order shown (two to six; a row with no words is not shown).",
                    ItemFields = new[]
                    {
                        new BlockFieldDefinition { Key = PollOptionTextField, Type = BlockFieldType.LocalizedText, Label = "Option", Required = true, Help = "The words of the option." },
                    },
                },
            },
        };

        // Tier 3: this point's collectable item (a stamp or tile card); what the visitor has collected is kept on the device
        public static readonly BlockKindDefinition Collect = new()
        {
            Key = CollectKind,
            Family = "play",
            DisplayName = "Collect",
            Help = "This point's collectable item, like a stamp: the visitor taps Add to my story and the item is kept on their device, " +
                   "with a count of how many of this wall's items they have (3 of 12). The total is counted from the collect blocks " +
                   "on the wall's points, so it grows as you author more of them (a wall whose Block Library switches Collect off " +
                   "counts none). Item Name empty: the point's card title. Series empty: the point's category name. The button words " +
                   "and the count line are Detail Card > Card Texts.",
            Variants = new[] { CollectAddToStory },
            DefaultVariant = CollectAddToStory,
            DisplayModes = new[] { CardOptions.DisplayInline },
            DefaultHeadingKey = CardStrings.Keys.CollectHeading,
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = CollectItemNameField, Type = BlockFieldType.LocalizedText, Label = "Item Name",
                    Help = "What the collectable is called (Stamp of the old gate). Empty: the point's card title.",
                },
                new BlockFieldDefinition
                {
                    Key = CollectSeriesField, Type = BlockFieldType.LocalizedText, Label = "Series",
                    Help = "The set the item belongs to (Gates and towers), shown small above its name. Empty: the point's category name.",
                },
            },
        };
    }
}
