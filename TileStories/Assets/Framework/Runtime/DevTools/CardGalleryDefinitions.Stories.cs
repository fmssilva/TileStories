using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    public static partial class CardGalleryDefinitions
    {
        private static void AddDialogues(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.Dialogue.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.DialogueKind, variant, "short", DialogueBlock(variant, DialoguePlain)));
                list.Add(new Entry(BuiltInBlocks.DialogueKind, variant, "choices", DialogueBlock(variant, DialogueWithChoices)));
                list.Add(new Entry(BuiltInBlocks.DialogueKind, variant, "long", DialogueBlock(variant, DialogueLong)));
                // - "partial": a row with no words is left out, a reply with no choice label is never offered, a line may have no speaker
                list.Add(new Entry(BuiltInBlocks.DialogueKind, variant, "partial", DialogueBlock(variant,
                    new DialogueRow("The mason", "The first line."),
                    new DialogueRow("The mason", ""),
                    new DialogueRow("", "A line with no speaker.", ("", "A reply nobody can pick."), ("Go on", "Very well.")))));
                // - one line only: nothing to continue, nothing to start again
                list.Add(new Entry(BuiltInBlocks.DialogueKind, variant, "oneline", DialogueBlock(variant, new DialogueRow("The mason", "Just one thing to say."))));
            }
        }

        private static BlockInstanceData DialogueBlock(string variant, params DialogueRow[] rows)
        {
            var block = new BlockInstanceData { key = QuizBlockKey, kind = BuiltInBlocks.DialogueKind, variant = variant };
            var items = new List<BlockItemData>();
            foreach (var row in rows)
            {
                var fields = new List<BlockItemFieldValue>();
                if (row.Speaker.Length > 0) fields.Add(ItemText(BuiltInBlocks.DialogueSpeakerField, row.Speaker));
                fields.Add(ItemText(BuiltInBlocks.DialogueTextField, row.Line));
                for (int i = 0; row.Replies != null && i < row.Replies.Length; i++)
                {
                    fields.Add(ItemText(DialogueRule.ChoiceField(i + 1), row.Replies[i].Choice));
                    fields.Add(ItemText(DialogueRule.ReplyField(i + 1), row.Replies[i].Reply));
                }
                items.Add(Item(fields.ToArray()));
            }
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.DialogueLinesField, items = items });
            return block;
        }


        // ---------------- timeline ----------------

        private static void AddTimelines(List<Entry> list)
        {
            var shortEvents = new[] { ("1147", "The siege", ""), ("1755", "The earthquake", "Most of the palace fell.") };
            var longEvents = new[]
            {
                ("1147", "The siege", "Crusaders on their way to the Holy Land helped take the town after four months."),
                ("c. 1300", "A royal palace", "The kings made the castle their home and rebuilt the [[keep]]."),
                ("1511", "The court moves to the river", ""),
                ("1 Nov 1755", "The great earthquake and the fire that followed it for six days", "Most of the palace fell; the walls stood."),
                ("1910", "A national monument", ""),
                ("1940", "Rebuilt as the painter saw it", "The restorers used old panels like this one as their plan."),
            };
            // - a row with no date and one with no title are left out
            var partialEvents = new[] { ("1147", "The siege", ""), ("", "No date", ""), ("1755", "", "No title"), ("1940", "Rebuilt", "") };
            foreach (var variant in BuiltInBlocks.Timeline.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.TimelineKind, variant, "short", TimelineBlock(variant, shortEvents, now: false)));
                list.Add(new Entry(BuiltInBlocks.TimelineKind, variant, "long", TimelineBlock(variant, longEvents, now: true)));
                list.Add(new Entry(BuiltInBlocks.TimelineKind, variant, "partial", TimelineBlock(variant, partialEvents, now: true)));
            }
            // - the swipe track both ways (Peek Next Card on above, off here)
            list.Add(new Entry(BuiltInBlocks.TimelineKind, BuiltInBlocks.TimelineHorizontal, "long_nopeek",
                TimelineBlock(BuiltInBlocks.TimelineHorizontal, longEvents, now: true), null, PeekNextCardOff));
        }

        private static BlockInstanceData TimelineBlock(string variant, (string Date, string Title, string Text)[] events, bool now)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.TimelineKind, variant = variant };
            var field = new BlockFieldValue { key = BuiltInBlocks.TimelineItemsField };
            foreach (var (date, title, text) in events)
                field.items.Add(Item(ItemText(BuiltInBlocks.TimelineDateField, date), ItemText(BuiltInBlocks.TimelineTitleField, title),
                    ItemText(BuiltInBlocks.TimelineTextField, text)));
            block.fields.Add(field);
            if (now) block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.TimelineHighlightNowField, flag = true });
            return block;
        }


        // ---------------- person ----------------

        private static void AddPeople(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.Person.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.PersonKind, variant, "short", PersonBlock(variant, "Afonso Henriques", "First king", "Took the town in 1147.")));
                list.Add(new Entry(BuiltInBlocks.PersonKind, variant, "long", PersonBlock(variant,
                    "Dom Manuel I of Portugal, called the Fortunate by the chroniclers of his reign",
                    "King from 1495 to 1521, who moved the royal court from the castle to the new palace by the river",
                    "He left the castle's rooms to the garrison and the prison and built his palace where the ships came in.\n\n" +
                    "The panel shows the castle still with the royal flags, so it was probably painted from older drawings.")));
                // - the optional role and text left empty: the name alone beside its initial
                list.Add(new Entry(BuiltInBlocks.PersonKind, variant, "nameonly", PersonBlock(variant, "Osbern", "", "")));
            }
        }

        private static BlockInstanceData PersonBlock(string variant, string name, string role, string text)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.PersonKind, variant = variant };
            block.fields.Add(Text(BuiltInBlocks.PersonNameField, name));
            if (role.Length > 0) block.fields.Add(Text(BuiltInBlocks.PersonRoleField, role));
            if (text.Length > 0) block.fields.Add(Text(BuiltInBlocks.PersonTextField, text));
            return block;
        }


        // ---------------- story_chapters ----------------

        private static void AddStoryChapters(List<Entry> list)
        {
            var shortChapters = new[] { ("The siege", "Four months outside the walls."), ("The gate", "It opened in October.") };
            var longChapters = new[]
            {
                ("The siege of the town on the hill, in the summer and autumn of 1147",
                    "Ships of crusaders on their way to the Holy Land stopped in the river and joined the king's army.\n\n" +
                    "For four months they camped outside the walls, and the [[keep]] never fell to an attack."),
                ("The palace", "The kings rebuilt the castle as their home and filled it with painted tiles."),
                ("The earthquake", "In 1755 the ground shook for six minutes; the palace fell and a fire burned for six days."),
                ("Today", "The walls were rebuilt in 1940 from old panels and drawings like this one."),
            };
            // - a row with no text and one with no title are left out: one chapter is left, and needs no buttons
            var partialChapters = new[] { ("The siege", "Four months outside the walls."), ("No text", ""), ("", "No title.") };
            foreach (var variant in BuiltInBlocks.StoryChapters.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.StoryChaptersKind, variant, "short", ChaptersBlock(variant, shortChapters)));
                list.Add(new Entry(BuiltInBlocks.StoryChaptersKind, variant, "long", ChaptersBlock(variant, longChapters)));
                list.Add(new Entry(BuiltInBlocks.StoryChaptersKind, variant, "single", ChaptersBlock(variant, partialChapters)));
            }
        }

        private static BlockInstanceData ChaptersBlock(string variant, (string Title, string Body)[] chapters)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.StoryChaptersKind, variant = variant };
            var field = new BlockFieldValue { key = BuiltInBlocks.StoryChaptersItemsField };
            foreach (var (title, body) in chapters)
                field.items.Add(Item(ItemText(BuiltInBlocks.StoryChaptersTitleField, title), ItemText(BuiltInBlocks.StoryChaptersBodyField, body)));
            block.fields.Add(field);
            return block;
        }


        // ---------------- compare_points ----------------

        public const string CompareOtherId = "compare_other";

        private static void AddComparePoints(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.ComparePoints.Variants)
            {
                // - this point intact, the other partly damaged (named by its name: it has no header)
                list.Add(new Entry(BuiltInBlocks.ComparePointsKind, variant, "short", CompareBlock(variant), Condition(OutlineIntact, 0f, false),
                    wall => wall.pois.Add(OtherPoi("Old Cathedral", OutlinePartial, 20f, false, null))));
                // - long titles on both sides; the other named by its card's own header title
                list.Add(new Entry(BuiltInBlocks.ComparePointsKind, variant, "long", CompareBlock(variant), Condition(OutlineHeavy, 60f, false),
                    wall => wall.pois.Add(OtherPoi("Other", OutlineDestroyed, 100f, false,
                        "The Royal Palace of the Kings by the river, before the earthquake"))));
                // - the other nobody assessed: its "?" ring
                list.Add(new Entry(BuiltInBlocks.ComparePointsKind, variant, "unknown", CompareBlock(variant), Condition(OutlinePartial, 20f, false),
                    wall => wall.pois.Add(OtherPoi("Customs House", OutlineUnknown, 100f, true, null))));
            }
        }

        private static BlockInstanceData CompareBlock(string variant)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.ComparePointsKind, variant = variant };
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.ComparePointsOtherField, value = CompareOtherId });
            return block;
        }

        private static POIData OtherPoi(string name, string levelKey, float pct, bool unknown, string headerTitle)
        {
            var poi = new POIData { id = CompareOtherId, name = name, category = CategoryKey, hierarchy_level_key = LevelKey };
            Condition(levelKey, pct, unknown)(poi);
            if (headerTitle != null)
            {
                var header = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.HeaderKind };
                header.fields.Add(Text(BlockStackBuilder.HeaderTitleField, headerTitle));
                poi.card.blocks.Add(header);
            }
            return poi;
        }

    }
}
