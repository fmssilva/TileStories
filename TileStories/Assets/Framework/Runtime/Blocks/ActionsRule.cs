using System.Collections.Generic;

namespace TileStories
{
    // Which buttons an actions block draws, as one pure rule so the view, the Editor's repeated-action warning and the tests read the
    // same answer (_3.1 Tier 1): a row is drawn when its action is one this framework knows (BuiltInBlocks.ActionOptions) -- an Open Block
    // row only when its target is a block the card can scroll to (BlockRefRule.JumpTargets: never a button that jumps nowhere); the sticky
    // look draws only the first such row (one call to action); a row's words are its own, else the card text of its action
    // (DefaultWordsKey), so the same action reads the same on every block that leaves its words empty -- except Open Block, whose buttons
    // differ by where they go: it says its target's heading first (Words).
    public static class ActionsRule
    {
        public readonly struct Button
        {
            // The authored row (0-based), its action, the words the row itself carries ("" when it has none) and, for Open Block, the
            // key of the block it moves the card to ("" for every other action)
            public readonly int Row;
            public readonly string Action;
            public readonly string Words;
            public readonly string Target;

            public Button(int row, string action, string words, string target = "")
            {
                Row = row;
                Action = action;
                Words = words;
                Target = target ?? "";
            }
        }

        // Whether this framework has this action
        public static bool IsKnown(string action) => System.Array.IndexOf(BuiltInBlocks.ActionOptions, action) >= 0;

        // The CardStrings key read for an action's words when its row has none (null: the action has no words of its own)
        public static string DefaultWordsKey(string action) => action switch
        {
            BuiltInBlocks.ActionShowOnWall => CardStrings.Keys.ShowOnWallButton,
            BuiltInBlocks.ActionOpenBlock => CardStrings.Keys.OpenBlockButton,
            _ => null,
        };

        // The buttons a block in `variant` draws, in order. `jumpTargets`: the blocks the card can scroll to (null = none: no Open Block
        // button is drawn)
        public static List<Button> Buttons(BlockFieldReader read, string variant, ICollection<string> jumpTargets)
        {
            var buttons = new List<Button>();
            bool onlyOne = variant == BuiltInBlocks.ActionsStickyCta;
            var rows = read.Items(BuiltInBlocks.ActionsItemsField);
            for (int row = 0; row < rows.Count; row++)
            {
                if (onlyOne && buttons.Count == 1) break;
                string action = read.ItemValue(rows[row], BuiltInBlocks.ActionsActionField);
                if (!IsKnown(action)) continue;
                string target = action == BuiltInBlocks.ActionOpenBlock ? read.ItemValue(rows[row], BuiltInBlocks.ActionsTargetField) : "";
                if (action == BuiltInBlocks.ActionOpenBlock && (jumpTargets == null || !jumpTargets.Contains(target))) continue;
                buttons.Add(new Button(row, action, read.ItemText(rows[row], BuiltInBlocks.ActionsLabelField), target));
            }
            return buttons;
        }

        // What a button says: its own words, else (Open Block) the heading of the block it opens on `poi`'s card, else its action's card
        // text in `strings` ("" when there is none)
        public static string Words(Button button, POIData poi, string language, string fallbackLanguage, CardStrings strings)
        {
            if (button.Words.Length > 0) return button.Words;
            if (button.Action == BuiltInBlocks.ActionOpenBlock)
            {
                var target = poi?.card?.blocks?.Find(b => b != null && b.key == button.Target);
                string heading = target != null ? new BlockFieldReader(target, language, fallbackLanguage).Text(BlockKindDefinition.HeadingField) : "";
                if (heading.Length > 0) return heading;
            }
            string key = DefaultWordsKey(button.Action);
            return key != null ? strings?.Get(key) ?? "" : "";
        }
    }
}
