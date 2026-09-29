using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    public static partial class CardGalleryDefinitions
    {
        // ---------------- feedback (Tier 3 group A) ----------------

        public const string FeedbackQuestion = "Was this description useful?";
        public const string FeedbackLongQuestion = "You have just read what the panel shows and how it was made, from the arcade to the river; how well did this description help you to understand what you saw?";

        private static void AddFeedback(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.Feedback.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.FeedbackKind, variant, "short", FeedbackBlock(variant, FeedbackQuestion)));
                list.Add(new Entry(BuiltInBlocks.FeedbackKind, variant, "long", FeedbackBlock(variant, FeedbackLongQuestion)));
                // - no question written: the card asks its own, in its own words
                list.Add(new Entry(BuiltInBlocks.FeedbackKind, variant, "noquestion", FeedbackBlock(variant, null)));
            }
        }

        private static BlockInstanceData FeedbackBlock(string variant, string question)
        {
            var block = new BlockInstanceData { key = QuizBlockKey, kind = BuiltInBlocks.FeedbackKind, variant = variant };
            if (question != null) block.fields.Add(Text(BuiltInBlocks.FeedbackQuestionField, question));
            return block;
        }

    }
}
