using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Which questions a knowledge_check block can show and how an answer is judged (_3.1 Tier 3), pure so every case is a unit
    // test. A question row holds every variant's fields (rows cannot nest, so the options are flat slots option_1..4 /
    // image_1..4); the variant decides which of them count:
    //   multiple_choice  -- the options that have text (at least two), `correct` names one of them
    //   image_choice     -- the options that have a picture MediaPathRule accepts (at least two; the text is an optional caption),
    //                       `correct` names one of them
    //   true_false_swipe -- the statement only: `is_true` says which of the two fixed choices is right
    // A row with no question or no explanation, or that its variant cannot use, is left out (Problem says why, for the Editor).
    // The persisted answer is stored under the row's authored index (Question.Row), so leaving a row out never moves another
    // question's saved answer.
    public static class KnowledgeCheckRule
    {
        // Options a question can hold (option_1..option_4)
        public const int MaxOptions = 4;

        // The fewest options a choice question needs
        public const int MinOptions = 2;

        // The two choices of a true / false statement: True is 0, False is 1
        public const int ChoiceTrue = 0;
        public const int ChoiceFalse = 1;

        public sealed class Option
        {
            public string Text = "";
            public string Image = "";
        }

        public sealed class Question
        {
            // The authored row's index (0-based): the slot the answer is stored under
            public int Row;
            public string Text = "";
            public string Explanation = "";
            // The choices, in order (true_false_swipe: two empty ones -- the view words True / False)
            public List<Option> Options = new();
            // The right choice: an index into Options
            public int Correct;
        }

        // Why a row cannot be shown (the Editor words it)
        public enum Problem
        {
            None,
            NoQuestion,
            NoExplanation,
            // a choice variant with fewer than MinOptions usable options
            TooFewOptions,
            // no `correct` picked
            NoCorrect,
            // `correct` names an option this variant does not show (empty, or a missing picture)
            CorrectIsEmpty,
        }

        // The field key of option `slot` (1..MaxOptions) and of its picture
        public static string OptionField(int slot) => BuiltInBlocks.KnowledgeCheckOptionPrefix + slot;
        public static string ImageField(int slot) => BuiltInBlocks.KnowledgeCheckImagePrefix + slot;

        // Read one authored row for `variant`: None and the question, or why not
        public static Problem Read(BlockFieldReader read, BlockItemData item, int row, string variant, out Question question)
        {
            question = null;
            string text = read.ItemText(item, BuiltInBlocks.KnowledgeCheckQuestionField).Trim();
            if (text.Length == 0) return Problem.NoQuestion;
            string explanation = read.ItemText(item, BuiltInBlocks.KnowledgeCheckExplanationField).Trim();
            if (explanation.Length == 0) return Problem.NoExplanation;
            var q = new Question { Row = row, Text = text, Explanation = explanation };

            if (variant == BuiltInBlocks.KnowledgeCheckTrueFalseSwipe)
            {
                q.Options.Add(new Option());
                q.Options.Add(new Option());
                q.Correct = read.ItemFlag(item, BuiltInBlocks.KnowledgeCheckIsTrueField) ? ChoiceTrue : ChoiceFalse;
                question = q;
                return Problem.None;
            }

            bool pictures = variant == BuiltInBlocks.KnowledgeCheckImageChoice;
            int correctSlot = SlotOf(read.ItemValue(item, BuiltInBlocks.KnowledgeCheckCorrectField));
            int correctIndex = -1;
            for (int slot = 1; slot <= MaxOptions; slot++)
            {
                string caption = read.ItemText(item, OptionField(slot)).Trim();
                string image = pictures ? read.ItemValidAsset(item, ImageField(slot), MediaKind.Image) : "";
                if (pictures ? image.Length == 0 : caption.Length == 0) continue;
                if (slot == correctSlot) correctIndex = q.Options.Count;
                q.Options.Add(new Option { Text = caption, Image = image });
            }
            if (q.Options.Count < MinOptions) return Problem.TooFewOptions;
            if (correctSlot == 0) return Problem.NoCorrect;
            if (correctIndex < 0) return Problem.CorrectIsEmpty;
            q.Correct = correctIndex;
            question = q;
            return Problem.None;
        }

        // The questions of `block` that `variant` can show, in the order written
        public static List<Question> Questions(BlockInstanceData block, string variant, string language = null, string fallbackLanguage = null)
        {
            var questions = new List<Question>();
            if (block == null) return questions;
            var read = new BlockFieldReader(block, language, fallbackLanguage);
            var rows = read.Items(BuiltInBlocks.KnowledgeCheckQuestionsField);
            for (int row = 0; row < rows.Count; row++)
                if (Read(read, rows[row], row, variant, out var question) == Problem.None) questions.Add(question);
            return questions;
        }

        // Whether `chosen` is the right choice of `question`
        public static bool IsCorrect(Question question, int chosen) => question != null && chosen == question.Correct;

        // The choice a finished swipe makes on a true / false statement: to the right is True, to the left False, and -1 when it
        // is no swipe (too short, or mostly vertical: SwipePageRule). `travel`: the finger's move from press to release.
        public static int SwipeChoice(Vector2 travel, float cardWidth)
        {
            int direction = SwipePageRule.Direction(travel, cardWidth);
            if (direction < 0) return ChoiceTrue;
            if (direction > 0) return ChoiceFalse;
            return -1;
        }

        // The option slot (1..MaxOptions) a `correct` value names; 0 = none
        private static int SlotOf(string value) =>
            int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int slot) && slot >= 1 && slot <= MaxOptions
                ? slot : 0;
    }
}
