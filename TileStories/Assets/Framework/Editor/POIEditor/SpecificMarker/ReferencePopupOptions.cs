using System.Collections.Generic;

namespace TileStories.Editor
{
    // Pure: the option list of a popup that picks a REFERENCE to a taxonomy row (a POI's Category,
    // Badge category, Hierarchy Level, Status level). A plain popup shows index 0 for a value it
    // cannot find and writes index 0 back on the next pass -- so merely drawing a POI whose stored
    // key no longer exists (row deleted, typo in the JSON) silently rewrote it. Here the current
    // value always has its own entry: "(none)" when blank, "<key> (missing)" when stale, kept until
    // the developer picks another one. Index <-> key mapping lives here so a test can check it.
    public sealed class ReferencePopupOptions
    {
        public const string NoneLabel = "(none)";
        public const string MissingSuffix = " (missing)";

        public string[] Keys { get; }
        public string[] Labels { get; }
        public int SelectedIndex { get; }

        private ReferencePopupOptions(string[] keys, string[] labels, int selectedIndex)
        {
            Keys = keys;
            Labels = labels;
            SelectedIndex = selectedIndex;
        }

        // keys/labels: the real rows (same length). allowNone: offer "(none)" as a real choice.
        public static ReferencePopupOptions Build(IList<string> keys, IList<string> labels, string current, bool allowNone)
        {
            var outKeys = new List<string>();
            var outLabels = new List<string>();
            bool currentIsBlank = string.IsNullOrWhiteSpace(current);

            if (allowNone || currentIsBlank)
            {
                outKeys.Add(null);
                outLabels.Add(NoneLabel);
            }

            int selected = currentIsBlank ? 0 : -1;
            for (int i = 0; i < keys.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(keys[i]) || outKeys.Contains(keys[i])) continue;
                if (!currentIsBlank && keys[i] == current) selected = outKeys.Count;
                outKeys.Add(keys[i]);
                outLabels.Add(labels != null && i < labels.Count && !string.IsNullOrWhiteSpace(labels[i]) ? labels[i] : keys[i]);
            }

            // Stale reference: keep it selectable (and selected) instead of silently replacing it
            if (selected < 0)
            {
                selected = outKeys.Count;
                outKeys.Add(current);
                outLabels.Add(current + MissingSuffix);
            }

            return new ReferencePopupOptions(outKeys.ToArray(), outLabels.ToArray(), selected);
        }

        // The key an index stands for (null = none); an out-of-range index keeps the current selection
        public string KeyAt(int index)
        {
            if (index < 0 || index >= Keys.Length) index = SelectedIndex;
            return Keys[index];
        }
    }
}
