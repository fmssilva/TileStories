using System.Collections.Generic;

namespace TileStories
{
    // The navigation Groups (_3.2 section 3 and rule 2.5.6), pure: the segmented element shows a card's families merged into 3-4 Groups
    // that always fit. A wall's Groups table holds its own rows (a generated key + its words per language); with none, the framework's
    // four apply. A family joins the group its Families row names, else its default one -- a group the wall does not have leaves the
    // family out (the Editor says so). Rule 6 itself (a group with no target is hidden) is CardNavModel's, which only makes a group out
    // of a target.
    public static class NavGroupRule
    {
        // One group as the card reads it: its key, the wall's own words (may be empty) and the framework's word for it (null for a wall's
        // own row, which has no framework word)
        public readonly struct Group
        {
            public readonly string Key;
            public readonly IReadOnlyList<LocalizedEntry> Label;
            public readonly string LabelKey;

            public Group(string key, IReadOnlyList<LocalizedEntry> label, string labelKey)
            {
                Key = key;
                Label = label;
                LabelKey = labelKey;
            }
        }

        public const string Discover = "discover";
        public const string Watch = "watch";
        public const string Explore = "explore";
        public const string Visit = "visit";

        // The framework's four, in order, and which built-in family each one holds by default (_3.2 section 3)
        public static readonly IReadOnlyList<string> DefaultKeys = new[] { Discover, Watch, Explore, Visit };
        private static readonly Dictionary<string, string> DefaultFamilyGroup = new()
        {
            [BuiltInFamilies.About] = Discover, [BuiltInFamilies.Stories] = Discover,
            [BuiltInFamilies.Media] = Watch, [BuiltInFamilies.Guides] = Watch,
            [BuiltInFamilies.Ar] = Explore, [BuiltInFamilies.Play] = Explore,
            [BuiltInFamilies.Visit] = Visit, [BuiltInFamilies.Community] = Visit,
        };

        // The groups this wall's cards use, in its table's order: its own rows, or the framework's four when it has none
        public static IReadOnlyList<Group> Groups(CardNavigationSettings nav)
        {
            var groups = new List<Group>();
            if (nav?.groups != null && nav.groups.Count > 0)
            {
                foreach (var row in nav.groups)
                    if (row != null && !string.IsNullOrEmpty(row.key)) groups.Add(new Group(row.key, row.label, FrameworkLabelKey(row.key)));
                return groups;
            }
            foreach (string key in DefaultKeys) groups.Add(new Group(key, null, FrameworkLabelKey(key)));
            return groups;
        }

        // The group a family's blocks merge into, or null for none: the family's Families row when it names one, else its default --
        // and only when this wall has that group
        public static string GroupOf(string familyKey, CardNavigationSettings nav)
        {
            string chosen = null;
            if (nav?.families != null)
                foreach (var row in nav.families)
                    if (row != null && row.family == familyKey && !string.IsNullOrEmpty(row.group)) chosen = row.group;
            chosen ??= DefaultGroupOf(familyKey);
            if (chosen == null) return null;
            foreach (var group in Groups(nav))
                if (group.Key == chosen) return chosen;
            return null;
        }

        // A family's group when its Families row names none (null for a family the framework does not know: an app family joins one only
        // through its row)
        public static string DefaultGroupOf(string familyKey) =>
            familyKey != null && DefaultFamilyGroup.TryGetValue(familyKey, out string group) ? group : null;

        // The words a visitor reads for a group: the wall's own in the card's language (then the fallback, then any), else the
        // framework's word for its key, else the key itself
        public static string Words(Group group, CardStrings strings)
        {
            if (group.Label != null && group.Label.Count > 0)
            {
                string own = BlockFieldReader.Pick(new List<LocalizedEntry>(group.Label), strings?.Language, strings?.FallbackLanguage);
                if (!string.IsNullOrWhiteSpace(own)) return own;
            }
            if (group.LabelKey != null && strings != null) return strings.Get(group.LabelKey);
            return group.Key;
        }

        // The framework's word for a group key, when it has one (the four defaults)
        private static string FrameworkLabelKey(string key)
        {
            switch (key)
            {
                case Discover: return CardStrings.Keys.NavGroupDiscover;
                case Watch: return CardStrings.Keys.NavGroupWatch;
                case Explore: return CardStrings.Keys.NavGroupExplore;
                case Visit: return CardStrings.Keys.NavGroupVisit;
                default: return null;
            }
        }
    }
}
