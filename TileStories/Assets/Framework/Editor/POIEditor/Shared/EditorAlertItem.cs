using System.Collections.Generic;
using System.Text;

namespace TileStories.Editor
{
    // One config-validation finding (which POI/setting, its value, what is wrong, how to fix
    // it). Produced by the Validate* methods; shown to the developer as one EditorNotice.
    internal struct EditorAlertItem
    {
        // A dialog is not a scrolling list, so a long report shows the first few findings
        // and says how many more there are.
        private const int MaxItemsShown = 6;

        public readonly string poiId;
        public readonly string value;
        public readonly string problem;
        public readonly string fixHint;

        public EditorAlertItem(string poiId, string value, string problem, string fixHint = null)
        {
            this.poiId = poiId;
            this.value = value;
            this.problem = problem;
            this.fixHint = fixHint;
        }

        // Plain-text body for the notice dialog: one block per finding, then the guidance.
        public static string FormatList(IReadOnlyList<EditorAlertItem> items, string guidance)
        {
            var text = new StringBuilder();
            int shown = System.Math.Min(items.Count, MaxItemsShown);
            for (int i = 0; i < shown; i++)
            {
                var item = items[i];
                text.Append("- ").Append(item.poiId);
                if (!string.IsNullOrEmpty(item.value))
                    text.Append(" (").Append(item.value).Append(')');
                text.Append(": ").Append(item.problem);
                if (!string.IsNullOrEmpty(item.fixHint))
                    text.Append("\n  Fix: ").Append(item.fixHint);
                text.Append("\n\n");
            }

            if (items.Count > shown)
                text.Append("...and ").Append(items.Count - shown).Append(" more.\n\n");

            if (!string.IsNullOrEmpty(guidance))
                text.Append(guidance);

            return text.ToString().TrimEnd();
        }
    }
}
