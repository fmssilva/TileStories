using System.Collections.Generic;

namespace TileStories
{
    // The actions a card offers more than once (_3.1 audit 15.B): every button a SHOWN block draws -- an Actions block's buttons (the sticky
    // look's one pinned button included) and the Show On Wall kind -- counted per action, so "the same action four times on one screen" is a
    // unit test, not a product pass. Pure; the Editor words the warning (one per action, naming the blocks), the card never warns.
    public static class RepeatedActionRule
    {
        // One place the card offers an action
        public sealed class Offer
        {
            public BlockInstanceData Block;
            public BlockKindDefinition Definition;
            public string Variant;
            // The words the offering button carries itself ("" when none: the card's own words show), and whether it is the pinned footer button
            public string Words = "";
            public bool Pinned;
        }

        public sealed class Repeat
        {
            public string Action;
            public readonly List<Offer> Offers = new();
        }

        // The actions offered two times or more across `entries` (a built stack: a block the card leaves out offers nothing), in the
        // order each action is first offered; each with every offer in card order
        public static List<Repeat> Find(IReadOnlyList<BlockStackBuilder.Entry> entries)
        {
            var all = new List<Repeat>();
            Repeat Of(string action)
            {
                var found = all.Find(r => r.Action == action);
                if (found == null) all.Add(found = new Repeat { Action = action });
                return found;
            }

            foreach (var entry in entries)
            {
                if (entry.Definition.Key == BuiltInBlocks.ActionsKind)
                {
                    var read = new BlockFieldReader(entry.Instance, null, null);
                    foreach (var button in ActionsRule.Buttons(read, entry.Variant))
                        Of(button.Action).Offers.Add(new Offer
                        {
                            Block = entry.Instance, Definition = entry.Definition, Variant = entry.Variant, Words = button.Words,
                            Pinned = entry.Variant == BuiltInBlocks.ActionsStickyCta,
                        });
                }
                else if (entry.Definition.Key == BuiltInBlocks.ShowOnWallKind)
                    Of(BuiltInBlocks.ActionShowOnWall).Offers.Add(new Offer { Block = entry.Instance, Definition = entry.Definition, Variant = entry.Variant });
            }
            all.RemoveAll(r => r.Offers.Count < 2);
            return all;
        }
    }
}
