using System.Collections.Generic;

namespace TileStories
{
    // Which buttons an actions block draws, as one pure rule so the view, the Editor's repeated-action warning and the tests read the
    // same answer (_3.1 Tier 1): a row is drawn when its action is one this framework knows (BuiltInBlocks.ActionOptions); the sticky look
    // draws only the first such row (one call to action); a row's words are its own, else the card text of its action (DefaultWordsKey),
    // so the same action reads the same on every block that leaves its words empty.
    public static class ActionsRule
    {
        public readonly struct Button
        {
            // The authored row (0-based), its action and the words the row itself carries ("" when it has none)
            public readonly int Row;
            public readonly string Action;
            public readonly string Words;

            public Button(int row, string action, string words)
            {
                Row = row;
                Action = action;
                Words = words;
            }
        }

        // Whether this framework has this action
        public static bool IsKnown(string action) => System.Array.IndexOf(BuiltInBlocks.ActionOptions, action) >= 0;

        // The CardStrings key read for an action's words when its row has none (null: the action has no words of its own)
        public static string DefaultWordsKey(string action) => action == BuiltInBlocks.ActionShowOnWall ? CardStrings.Keys.ShowOnWallButton : null;

        // The buttons a block in `variant` draws, in order
        public static List<Button> Buttons(BlockFieldReader read, string variant)
        {
            var buttons = new List<Button>();
            bool onlyOne = variant == BuiltInBlocks.ActionsStickyCta;
            var rows = read.Items(BuiltInBlocks.ActionsItemsField);
            for (int row = 0; row < rows.Count; row++)
            {
                if (onlyOne && buttons.Count == 1) break;
                string action = read.ItemValue(rows[row], BuiltInBlocks.ActionsActionField);
                if (!IsKnown(action)) continue;
                buttons.Add(new Button(row, action, read.ItemText(rows[row], BuiltInBlocks.ActionsLabelField)));
            }
            return buttons;
        }
    }
}
