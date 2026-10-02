using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    public static partial class CardGalleryDefinitions
    {
        // ---------------- status ----------------

        public const string OutlineIntact = "outline_1";
        public const string OutlinePartial = "outline_2";
        public const string OutlineHeavy = "outline_3";
        public const string OutlineDestroyed = "outline_4";
        public const string OutlineUnknown = "unknown";

        private static void AddStatus(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.Status.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.StatusKind, variant, "intact", StatusBlock(variant, null), Condition(OutlineIntact, 0f, false)));
                list.Add(new Entry(BuiltInBlocks.StatusKind, variant, "partial", StatusBlock(variant, null), Condition(OutlinePartial, 20f, false)));
                list.Add(new Entry(BuiltInBlocks.StatusKind, variant, "unknown", StatusBlock(variant, null), Condition(OutlineUnknown, 100f, true)));
                list.Add(new Entry(BuiltInBlocks.StatusKind, variant, "label", StatusBlock(variant, "Kept as the painter saw it, with every merlon in place"),
                    Condition(OutlineIntact, 0f, false)));
                // - a wall with no Outline Types rows: no marker ring to borrow, the card's own status tokens
                list.Add(new Entry(BuiltInBlocks.StatusKind, variant, "tokens", StatusBlock(variant, null), Condition("", 60f, false),
                    wall => wall.outline_levels.Clear()));
            }
        }

        private static BlockInstanceData StatusBlock(string variant, string label)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.StatusKind, variant = variant };
            if (label != null) block.fields.Add(Text(BuiltInBlocks.StatusLabelField, label));
            return block;
        }

        private static System.Action<POIData> Condition(string levelKey, float pct, bool unknown) => poi =>
        {
            poi.has_status = true;
            poi.status_level_key = levelKey;
            poi.status_pct = pct;
            poi.status_unknown = unknown;
        };


        // ---------------- rich_text ----------------

        public const string ShortText = "Built on the hill above the river, the gate let carts into the upper town.";
        public const string LongText =
            "[[Keep|keep]] and walls were rebuilt after 1147, when the town changed hands, and again after every siege the " +
            "chroniclers wrote down. The painter shows them whole, with every merlon in place.\n\n" +
            "The [[curtain wall]] runs from the gate to the river and closes the lower town. Its towers are square on the " +
            "hill side and round where the ground falls away.\n\n" +
            "By 1700 the castle was a barracks and a prison; the panel is older than that change and keeps the royal colours " +
            "on the flags.\n\n" +
            "Most of what the painter saw is still standing, but the long arcade by the water was lost in the earthquake.";

        private static void AddRichText(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.RichText.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.RichTextKind, variant, "short", RichText(variant, ShortText, withSections: true)));
                list.Add(new Entry(BuiltInBlocks.RichTextKind, variant, "long", RichText(variant, LongText, withSections: true)));
            }
            // - the optional Sections left empty: the Sections look shows the text alone
            list.Add(new Entry(BuiltInBlocks.RichTextKind, BuiltInBlocks.RichTextSections, "nosections",
                RichText(BuiltInBlocks.RichTextSections, ShortText, withSections: false)));
        }

        private static BlockInstanceData RichText(string variant, string body, bool withSections)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.RichTextKind, variant = variant };
            block.fields.Add(Text(BuiltInBlocks.RichTextBodyField, body));
            if (withSections)
            {
                var sections = new BlockFieldValue { key = BuiltInBlocks.RichTextSectionsField };
                sections.items.Add(Item(ItemText(BuiltInBlocks.RichTextSectionTitleField, "Before the earthquake"),
                    ItemText(BuiltInBlocks.RichTextSectionBodyField, "The arcade ran along the water for two hundred metres.")));
                sections.items.Add(Item(ItemText(BuiltInBlocks.RichTextSectionTitleField, "After 1755"),
                    ItemText(BuiltInBlocks.RichTextSectionBodyField, "Only the [[keep]] and the gate stood; the rest was rebuilt in stone.")));
                block.fields.Add(sections);
            }
            return block;
        }


        // ---------------- quick_facts ----------------

        private static void AddQuickFacts(List<Entry> list)
        {
            var shortFacts = new[] { ("Built", "1147"), ("Height", "32 m") };
            var longFacts = new[]
            {
                ("Built", "1147"), ("Rebuilt", "After the earthquake of 1755"), ("Architect", "Unknown master masons of the royal works"),
                ("Towers", "11"), ("Walls", "Rammed earth and limestone"), ("Open", "Every day"),
            };
            // - one row with only a value, one with only a label: each shows the text it has
            var partialFacts = new[] { ("", "1147"), ("Architect", "") };
            foreach (var variant in BuiltInBlocks.QuickFacts.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.QuickFactsKind, variant, "short", Facts(variant, shortFacts)));
                list.Add(new Entry(BuiltInBlocks.QuickFactsKind, variant, "long", Facts(variant, longFacts)));
                list.Add(new Entry(BuiltInBlocks.QuickFactsKind, variant, "partial", Facts(variant, partialFacts)));
            }
        }

        private static BlockInstanceData Facts(string variant, (string Label, string Value)[] facts)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.QuickFactsKind, variant = variant };
            var rows = new BlockFieldValue { key = BuiltInBlocks.QuickFactsItemsField };
            foreach (var (label, value) in facts)
                rows.items.Add(Item(ItemText(BuiltInBlocks.QuickFactsLabelField, label), ItemText(BuiltInBlocks.QuickFactsValueField, value)));
            block.fields.Add(rows);
            return block;
        }


        // ---------------- fun_fact ----------------

        private static void AddFunFacts(List<Entry> list)
        {
            var shortFacts = new[] { "The gate was locked every night at nine." };
            var longFacts = new[]
            {
                "The [[keep]] has no door at ground level: its only entrance is a wooden stair that the garrison could pull up.",
                "Peacocks have lived in the castle gardens for longer than anyone remembers.\n\nThey are not wild: the city feeds them.",
                "The painter left out a whole tower, probably to make room for the ships.",
            };
            foreach (var variant in BuiltInBlocks.FunFact.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.FunFactKind, variant, "short", FunFacts(variant, shortFacts)));
                list.Add(new Entry(BuiltInBlocks.FunFactKind, variant, "long", FunFacts(variant, longFacts)));
            }
        }

        private static BlockInstanceData FunFacts(string variant, string[] facts)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.FunFactKind, variant = variant };
            var rows = new BlockFieldValue { key = BuiltInBlocks.FunFactItemsField };
            foreach (string fact in facts) rows.items.Add(Item(ItemText(BuiltInBlocks.FunFactTextField, fact)));
            block.fields.Add(rows);
            return block;
        }


        // ---------------- pull_quote ----------------

        private static void AddPullQuotes(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.PullQuote.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.PullQuoteKind, variant, "short", Quote(variant, "A city on a hill cannot be hidden.", "A chronicler", "1147")));
                list.Add(new Entry(BuiltInBlocks.PullQuoteKind, variant, "long", Quote(variant,
                    "We saw the [[keep]] from the river long before the town, white in the morning, and every man on the ship fell " +
                    "silent, for it looked as if it had been there before the hill itself.",
                    "Osbern, a crusader priest from the English fleet", "Letter on the conquest of the city, written the following winter")));
                // - the optional author and source left empty: no attribution line at all
                list.Add(new Entry(BuiltInBlocks.PullQuoteKind, variant, "noauthor", Quote(variant, "A city on a hill cannot be hidden.", "", "")));
            }
        }

        private static BlockInstanceData Quote(string variant, string quote, string author, string source)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.PullQuoteKind, variant = variant };
            block.fields.Add(Text(BuiltInBlocks.PullQuoteTextField, quote));
            if (author.Length > 0) block.fields.Add(Text(BuiltInBlocks.PullQuoteAuthorField, author));
            if (source.Length > 0) block.fields.Add(Text(BuiltInBlocks.PullQuoteSourceField, source));
            return block;
        }


        // ---------------- process_steps ----------------

        private static void AddProcessSteps(List<Entry> list)
        {
            var shortSteps = new[] { ("Shape the clay", "Pressed into a square wooden frame."), ("Fire it", "") };
            var longSteps = new[]
            {
                ("Dig and wash the clay from the river banks outside the city walls",
                    "The clay was left to rest in water for weeks, so that stones and roots sank to the bottom.\n\nOnly the fine top layer was kept."),
                ("Shape and dry the tiles", "Pressed into square frames and dried in the shade for a month."),
                ("First firing", "A day and a night in the kiln turned the clay into hard [[biscuit]]."),
                ("Glaze and paint", "A coat of white tin glaze, then the drawing in cobalt blue with a fine brush."),
                ("Second firing", "The glaze melted into glass and the blue sank into it for good."),
            };
            // - the second row has no title (not shown, and the count does not skip a number); the last has no text
            var partialSteps = new[] { ("Shape the clay", "Pressed into a frame."), ("", "A row without a title."), ("Fire it", "") };
            foreach (var variant in BuiltInBlocks.ProcessSteps.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.ProcessStepsKind, variant, "short", StepsBlock(variant, shortSteps)));
                list.Add(new Entry(BuiltInBlocks.ProcessStepsKind, variant, "long", StepsBlock(variant, longSteps)));
                list.Add(new Entry(BuiltInBlocks.ProcessStepsKind, variant, "partial", StepsBlock(variant, partialSteps)));
            }
        }

        private static BlockInstanceData StepsBlock(string variant, (string Title, string Text)[] steps)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.ProcessStepsKind, variant = variant };
            var field = new BlockFieldValue { key = BuiltInBlocks.ProcessStepsItemsField };
            foreach (var (title, text) in steps)
                field.items.Add(Item(ItemText(BuiltInBlocks.ProcessStepsTitleField, title), ItemText(BuiltInBlocks.ProcessStepsTextField, text)));
            block.fields.Add(field);
            return block;
        }


        // ---------------- swatches ----------------

        private static void AddSwatches(List<Entry> list)
        {
            var shortSwatches = new[] { ("Cobalt blue", "#1F3F8F", "The drawing"), ("Tin white", "#F2EEE3", "The glaze") };
            var longSwatches = new[]
            {
                ("Cobalt blue", "#1F3F8F", "Every outline and shadow of the panel"),
                ("Tin white", "#F2EEE3", "The glaze under the drawing"),
                ("Antimony yellow", "#D9A93A", "The borders and the royal flags"),
                ("Copper green", "#3C7A4E", "Trees and the river bank, only on the oldest panels"),
                ("Manganese purple, almost black where the brush stopped", "#4A2F45", "Outlines before cobalt became common"),
                ("Iron red", "#9C3B24", ""),
            };
            // - no name, a colour that is not one ("#12", "blue"): three rows left out; "#abc" is the short form of a real colour
            var partialSwatches = new[] { ("", "#1F3F8F", "No name"), ("Bad colour", "#12", ""), ("A word", "blue", ""), ("Grey", "#abc", ""), ("Umber", "#2B2118", "Close to the card's own colour") };
            foreach (var variant in BuiltInBlocks.Swatches.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.SwatchesKind, variant, "short", SwatchesBlock(variant, shortSwatches)));
                list.Add(new Entry(BuiltInBlocks.SwatchesKind, variant, "long", SwatchesBlock(variant, longSwatches)));
                list.Add(new Entry(BuiltInBlocks.SwatchesKind, variant, "partial", SwatchesBlock(variant, partialSwatches)));
                // - Show Code on: the colour's hex under its name (off by default: a visitor rarely wants "#1F3F8F")
                var codes = SwatchesBlock(variant, shortSwatches);
                codes.fields.Add(new BlockFieldValue { key = BuiltInBlocks.SwatchesShowCodeField, flag = true });
                list.Add(new Entry(BuiltInBlocks.SwatchesKind, variant, "codes", codes));
            }
        }

        private static BlockInstanceData SwatchesBlock(string variant, (string Name, string Colour, string Note)[] swatches)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.SwatchesKind, variant = variant };
            var field = new BlockFieldValue { key = BuiltInBlocks.SwatchesItemsField };
            foreach (var (name, colour, note) in swatches)
                field.items.Add(Item(ItemText(BuiltInBlocks.SwatchesNameField, name), new BlockItemFieldValue { key = BuiltInBlocks.SwatchesColourField, value = colour },
                    ItemText(BuiltInBlocks.SwatchesNoteField, note)));
            block.fields.Add(field);
            return block;
        }


        // ---------------- actions ----------------

        private static void AddActions(List<Entry> list)
        {
            var one = new[] { ("See it on the wall", BuiltInBlocks.ActionShowOnWall) };
            var many = new[]
            {
                ("See it on the wall", BuiltInBlocks.ActionShowOnWall),
                ("Find the keep on the panel from the river side", BuiltInBlocks.ActionShowOnWall),
                ("Where is it?", BuiltInBlocks.ActionShowOnWall),
            };
            // - a button with no words (it reads its action's card text) and one with an action this framework does not know (left out)
            var partial = new[] { ("", BuiltInBlocks.ActionShowOnWall), ("Listen", "listen"), ("See it on the wall", BuiltInBlocks.ActionShowOnWall) };
            foreach (var variant in BuiltInBlocks.Actions.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.ActionsKind, variant, "short", ActionsBlock(variant, one)));
                list.Add(new Entry(BuiltInBlocks.ActionsKind, variant, "long", ActionsBlock(variant, many)));
                list.Add(new Entry(BuiltInBlocks.ActionsKind, variant, "partial", ActionsBlock(variant, partial)));
            }
        }

        private static BlockInstanceData ActionsBlock(string variant, (string Label, string Action)[] buttons)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.ActionsKind, variant = variant };
            var field = new BlockFieldValue { key = BuiltInBlocks.ActionsItemsField };
            foreach (var (label, action) in buttons)
                field.items.Add(Item(ItemText(BuiltInBlocks.ActionsLabelField, label), new BlockItemFieldValue { key = BuiltInBlocks.ActionsActionField, value = action }));
            block.fields.Add(field);
            return block;
        }

    }
}
