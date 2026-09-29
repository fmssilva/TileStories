using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    public static partial class CardGalleryDefinitions
    {
        // ---------------- knowledge_check (Tier 3 group A) ----------------

        // One authored question row: the question, the option texts and pictures (slot 1, 2...), the right slot (1-based; 0 = none
        // written), whether a true / false statement is true, and the explanation
        private readonly struct QuestionRow
        {
            public readonly string Question;
            public readonly string[] Options;
            public readonly string[] Images;
            public readonly int Right;
            public readonly bool IsTrue;
            public readonly string Explanation;

            public QuestionRow(string question, string[] options, string[] images, int right, bool isTrue, string explanation)
            {
                Question = question;
                Options = options;
                Images = images;
                Right = right;
                IsTrue = isTrue;
                Explanation = explanation;
            }
        }

        // - a property, not a field: `All = Build()` above runs first, so a static field declared down here would still be null
        private static string[] None => System.Array.Empty<string>();

        // The short question the promise tests answer, right and wrong
        public const string KeepQuestion = "What is the tallest tower of a castle called?";
        public const string KeepExplanation = "The keep is the strongest tower: the last refuge when the walls fell.";
        public const string CurtainStatement = "A curtain wall joins the towers of a castle.";
        public const string CurtainExplanation = "Yes: the curtain wall runs between the towers and closes the ring.";
        public const string PanelQuestion = "Which picture shows the panel before the earthquake?";
        public const string PanelExplanation = "The earlier picture still has the whole arcade standing.";
        // The block key of every knowledge_check entry: the answers are stored under it
        public const string QuizBlockKey = "block_2";

        private static void AddKnowledgeChecks(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.KnowledgeCheck.Variants)
                foreach (string content in new[] { "short", "long", "partial" })
                    list.Add(new Entry(BuiltInBlocks.KnowledgeCheckKind, variant, content, KnowledgeBlock(variant, KnowledgeRows(variant, content))));
        }

        private static QuestionRow[] KnowledgeRows(string variant, string content)
        {
            bool tf = variant == BuiltInBlocks.KnowledgeCheckTrueFalseSwipe;
            bool pictures = variant == BuiltInBlocks.KnowledgeCheckImageChoice;
            if (content == "short")
                return new[]
                {
                    tf ? new QuestionRow(CurtainStatement, None, None, 0, true, CurtainExplanation)
                    : pictures ? new QuestionRow(PanelQuestion, new[] { "Before", "After" }, new[] { "before.png", "after.png" }, 1, false, PanelExplanation)
                    : new QuestionRow(KeepQuestion, new[] { "The curtain wall", "The keep", "The gatehouse" }, None, 2, false, KeepExplanation),
                };
            if (content == "long")
                return LongRows(tf, pictures);
            // - "partial": one complete question and rows the look must leave out (no explanation, too few options, a right option
            //   that is not shown, no question)
            const string good = "Complete question.";
            const string why = "Because it is complete.";
            if (tf)
                return new[]
                {
                    new QuestionRow(good, None, None, 0, false, why),
                    new QuestionRow("No explanation written.", None, None, 0, true, ""),
                    new QuestionRow("", None, None, 0, true, why),
                };
            if (pictures)
                return new[]
                {
                    new QuestionRow(good, new[] { "One", "Two" }, new[] { "one.png", "two.png" }, 2, false, why),
                    new QuestionRow("Only one picture.", new[] { "One" }, new[] { "one.png" }, 1, false, why),
                    new QuestionRow("The right one has no picture.", new[] { "One", "Two", "Three" }, new[] { "one.png", "two.png" }, 3, false, why),
                };
            return new[]
            {
                new QuestionRow(good, new[] { "Wrong", "Right", "Also wrong" }, None, 2, false, why),
                new QuestionRow("No explanation written.", new[] { "A", "B" }, None, 1, false, ""),
                new QuestionRow("Only one option.", new[] { "A" }, None, 1, false, why),
                new QuestionRow("The right option is blank.", new[] { "A", "B" }, None, 4, false, why),
            };
        }

        // Three questions with long words, each look: the first is the one the promise tests answer
        private static QuestionRow[] LongRows(bool tf, bool pictures)
        {
            const string longQuestion = "Long ago the castle on the hill was rebuilt again and again, after every siege and every earthquake; which of these is the " +
                                        "tallest and strongest tower, the one the defenders kept as their last refuge when everything else had fallen?";
            const string longWhy = "The keep is the strongest tower of a castle. It stood apart from the walls, so the defenders could hold it even when the " +
                                   "walls and the other towers had been taken, and the painter of the panel drew it larger than all the rest.";
            if (tf)
                return new[]
                {
                    new QuestionRow("The keep of a castle was usually built on the lowest ground, far from the walls, so that the enemy could not see it from the towers.",
                        None, None, 0, false, longWhy),
                    new QuestionRow("Biscuit is clay fired once, hard but not yet glazed.", None, None, 0, true, "Yes: the first firing makes the clay hard, the glaze comes after."),
                    new QuestionRow("The panel was made after the earthquake of 1755.", None, None, 0, false, "No: it shows the city as it was before, which is why it matters."),
                };
            if (pictures)
                return new[]
                {
                    new QuestionRow(longQuestion, new[] { "The panel as first painted, with the arcade", "The panel today, with its missing tiles", "The keep seen from the river", "The old cathedral" },
                        new[] { "one.png", "two.png", "three.png", "four.png" }, 3, false, longWhy),
                    new QuestionRow("Which picture is the after one?", new[] { "Before", "After" }, new[] { "before.png", "after.png" }, 2, false, PanelExplanation),
                    new QuestionRow("Which of these is portrait?", new[] { "One", "Two", "Three" }, new[] { "one.png", "two.png", "three.png" }, 3, false, "The third picture is taller than it is wide."),
                };
            return new[]
            {
                new QuestionRow(longQuestion,
                    new[] { "The curtain wall that joins the towers all around the hill", "The keep, the tallest and strongest tower of the castle",
                            "The gatehouse with its two round towers and the old drawbridge" }, None, 2, false, longWhy),
                new QuestionRow("Which of these was NOT part of the old palace?", new[] { "The chapel", "The arcade", "The lighthouse", "The customs house" }, None, 3,
                    false, "The palace stood on the riverside with a chapel, an arcade and the customs house; there was no lighthouse."),
                new QuestionRow("What is biscuit?", new[] { "Clay fired once, hard but not yet glazed", "Clay that is glazed but not fired" }, None, 1, false,
                    "Biscuit is the first firing of a tile, before any glaze."),
            };
        }

        private static BlockInstanceData KnowledgeBlock(string variant, QuestionRow[] rows, bool showAfterViewed = false)
        {
            var block = new BlockInstanceData { key = QuizBlockKey, kind = BuiltInBlocks.KnowledgeCheckKind, variant = variant };
            var questions = new BlockFieldValue { key = BuiltInBlocks.KnowledgeCheckQuestionsField };
            foreach (var row in rows)
            {
                var item = Item(ItemText(BuiltInBlocks.KnowledgeCheckQuestionField, row.Question), ItemText(BuiltInBlocks.KnowledgeCheckExplanationField, row.Explanation));
                for (int i = 0; i < row.Options.Length; i++) item.fields.Add(ItemText(KnowledgeCheckRule.OptionField(i + 1), row.Options[i]));
                for (int i = 0; i < row.Images.Length; i++) item.fields.Add(new BlockItemFieldValue { key = KnowledgeCheckRule.ImageField(i + 1), asset = row.Images[i] });
                if (row.Right > 0) item.fields.Add(new BlockItemFieldValue { key = BuiltInBlocks.KnowledgeCheckCorrectField, value = row.Right.ToString() });
                if (row.IsTrue) item.fields.Add(new BlockItemFieldValue { key = BuiltInBlocks.KnowledgeCheckIsTrueField, flag = true });
                questions.items.Add(item);
            }
            block.fields.Add(questions);
            if (showAfterViewed) block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.KnowledgeCheckShowAfterViewedField, flag = true });
            return block;
        }

        // The POI of the gated question: a header, a long text, then a multiple-choice question that waits until the card was read
        // (show_after_viewed). `longText` false: the text is one short paragraph, so all of it fits on a card at full
        public const string GatedPoiId = "gated_knowledge_check";
        public const string GatedQuizBlockKey = "block_3";

        public static POIData GatedKnowledgePoi(bool longText)
        {
            var poi = new POIData { id = GatedPoiId, name = "Gate", category = CategoryKey, hierarchy_level_key = LevelKey };
            var header = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.HeaderKind, variant = BuiltInBlocks.HeaderTextOnly };
            header.fields.Add(Text(BlockStackBuilder.HeaderTitleField, "Gate"));
            poi.card.blocks.Add(header);
            var text = RichText(BuiltInBlocks.RichTextPlain, longText ? LongText + "\n\n" + LongText + "\n\n" + LongText : "Built on the hill.", false);
            text.key = "block_2";
            poi.card.blocks.Add(text);
            var quiz = KnowledgeBlock(BuiltInBlocks.KnowledgeCheckMultipleChoice, KnowledgeRows(BuiltInBlocks.KnowledgeCheckMultipleChoice, "short"), showAfterViewed: true);
            quiz.key = GatedQuizBlockKey;
            poi.card.blocks.Add(quiz);
            return poi;
        }


        // ---------------- poll, collect, dialogue, show_on_wall (Tier 3 group B) ----------------

        public const string PollQuestion = "Which side of the wall would you visit first?";
        public const string PollLongQuestion = "You have seen how the panel was made and what the earthquake left standing; which part of the whole story would you most like the museum to tell in more detail next season?";
        public static string[] PollShortOptions => new[] { "The arcade", "The river gate", "The bell tower" };
        public static string[] PollLongOptions => new[]
        {
            "The long arcade with its many arches and the market that once stood under it",
            "The river gate", "The bell tower", "The cloister", "The old cemetery beside the chapel of Saint George", "The kitchen garden",
        };

        private static void AddPolls(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.Poll.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.PollKind, variant, "short", PollBlock(variant, PollQuestion, PollShortOptions)));
                list.Add(new Entry(BuiltInBlocks.PollKind, variant, "long", PollBlock(variant, PollLongQuestion, PollLongOptions)));
                // - "partial": blank rows are left out; the two with words are still a poll
                list.Add(new Entry(BuiltInBlocks.PollKind, variant, "partial", PollBlock(variant, PollQuestion, "The arcade", "", "The river gate", "  ")));
            }
        }

        private static BlockInstanceData PollBlock(string variant, string question, params string[] options)
        {
            var block = new BlockInstanceData { key = QuizBlockKey, kind = BuiltInBlocks.PollKind, variant = variant };
            block.fields.Add(Text(BuiltInBlocks.PollQuestionField, question));
            var rows = new List<BlockItemData>();
            foreach (string option in options) rows.Add(Item(ItemText(BuiltInBlocks.PollOptionTextField, option)));
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.PollOptionsField, items = rows });
            return block;
        }

        public const string CollectItem = "Stamp of the old gate";
        public const string CollectSeries = "Gates and towers";

        // Three more collectable points on the fabricated wall: the wall's total is then 4 (the shown point + these)

        private static void ThreeMoreCollectables(WallConfigData wall)
        {
            for (int i = 1; i <= 3; i++)
            {
                var poi = WallPoi("collect_" + i, "Collectable " + i, new Vector3(i, 0f, 0f), null);
                poi.card.blocks.Add(new BlockInstanceData { key = "block_9", kind = BuiltInBlocks.CollectKind, variant = BuiltInBlocks.CollectAddToStory });
                wall.pois.Add(poi);
            }
        }

        private static void AddCollects(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.Collect.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.CollectKind, variant, "short", CollectBlock(variant, CollectItem, CollectSeries), null, ThreeMoreCollectables));
                // - nothing written: the point's card title and its category name
                list.Add(new Entry(BuiltInBlocks.CollectKind, variant, "defaults", CollectBlock(variant, null, null), null, ThreeMoreCollectables));
                list.Add(new Entry(BuiltInBlocks.CollectKind, variant, "long",
                    CollectBlock(variant, "Stamp of the great river gate of the Royal Palace of the Kings of Portugal and of the Algarves",
                        "The gates, towers, arcades and cloisters of the palace by the river"), null, ThreeMoreCollectables));
                // - the only collectable on its wall: 0 of 1
                list.Add(new Entry(BuiltInBlocks.CollectKind, variant, "alone", CollectBlock(variant, CollectItem, CollectSeries)));
            }
        }

        private static BlockInstanceData CollectBlock(string variant, string itemName, string series)
        {
            var block = new BlockInstanceData { key = QuizBlockKey, kind = BuiltInBlocks.CollectKind, variant = variant };
            if (itemName != null) block.fields.Add(Text(BuiltInBlocks.CollectItemNameField, itemName));
            if (series != null) block.fields.Add(Text(BuiltInBlocks.CollectSeriesField, series));
            return block;
        }

        // One authored dialogue row: the speaker, the line, and up to three (choice, reply) pairs
        public readonly struct DialogueRow
        {
            public readonly string Speaker;
            public readonly string Line;
            public readonly (string Choice, string Reply)[] Replies;

            public DialogueRow(string speaker, string line, params (string Choice, string Reply)[] replies)
            {
                Speaker = speaker;
                Line = line;
                Replies = replies;
            }
        }

        public static DialogueRow[] DialoguePlain => new DialogueRow[]
        {
            new("The mason", "Welcome. Mind the dust: we are mending the arch."),
            new("The mason", "The stone comes from the quarry across the river."),
            new("The mason", "Come back next spring and it will look as it did in 1640."),
        };

        public static DialogueRow[] DialogueWithChoices => new DialogueRow[]
        {
            new("The mason", "Welcome. Do you know why this arch was rebuilt?", ("No, tell me", "The earthquake brought it down in 1755."), ("Yes, the earthquake", "Then you know more than most visitors.")),
            new("The mason", "It took eleven winters to finish."),
            new("The mason", "Would you like to hold a chisel?", ("Yes please", "Careful: it is sharper than it looks."), ("Not today", ""), ("Maybe later", "I am here until dusk.")),
            new("The mason", "Thank you for listening."),
        };

        public static DialogueRow[] DialogueLong => new DialogueRow[]
        {
            new("The royal chronicler of the household of the Kings of Portugal and of the Algarves",
                "The palace stood by the river for more than two hundred years and its halls, its gardens and its long arcades were known across the whole of Europe for their tiles and for their paintings, and every visitor to the court was taken through them in the same order."),
            new("The royal chronicler of the household of the Kings of Portugal and of the Algarves",
                "Then, on the morning of the first of November, everything moved.", ("What happened next?", "The river rose, the fires started, and by evening the palace was gone; only the foundations and the memory were left."), ("How do you know?", "It was written down by people who were there, and I have read every page of it.")),
        };

    }
}
