using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // The Navigation Bar (_3.3 S4a review), pure: what NavBarView draws from card_settings.navigation.bar and the card's map.
    //   - Items: the wall's Bar Groups rows, or -- with none -- one group per family a visitor can navigate to (InNav), in the families'
    //     effective order (the Families table's), each with its family's words, icon and colour
    //   - Entries: the groups ONE card shows -- a shown row with at least one target of its families on this card, in the table's order (a
    //     bar is a fixed map, like an app's tab bar: its order never follows the scroll); each lands on its first target in the stack
    //   - EntryOf: the group a block belongs to (its family's first shown row), -1 for none
    //   - the option readers: an unknown value reads as the first of its CardOptions list (the default), so the Editor and the card agree
    // The bar shows only on a card the wall gives navigation (NavOntologyRule.ShowsNavigation) and with two groups at least: one is no choice.
    public static class NavBarRule
    {
        // The least groups a bar needs to be drawn
        public const int MinEntries = 2;

        // One group of the bar as the card reads it
        public readonly struct Item
        {
            public readonly string Key;
            public readonly IReadOnlyList<LocalizedEntry> Label;
            // The CardStrings key of its first family's word (null when none): the words when the row has none of its own
            public readonly string LabelKey;
            public readonly string Icon;
            public readonly string SelectedIcon;
            public readonly string ColorToken;
            public readonly IReadOnlyList<string> Families;

            public Item(string key, IReadOnlyList<LocalizedEntry> label, string labelKey, string icon, string selectedIcon, string colorToken,
                IReadOnlyList<string> families)
            {
                Key = key;
                Label = label;
                LabelKey = labelKey;
                Icon = icon;
                SelectedIcon = selectedIcon;
                ColorToken = colorToken;
                Families = families;
            }
        }

        // One group ONE card shows: its item and the targets of its families on the card, in the stack's order
        public readonly struct Entry
        {
            public readonly Item Item;
            public readonly IReadOnlyList<string> Targets;
            public string FirstTarget => Targets.Count > 0 ? Targets[0] : null;

            public Entry(Item item, IReadOnlyList<string> targets)
            {
                Item = item;
                Targets = targets;
            }
        }

        public static bool Enabled(CardNavigationSettings nav) => nav?.bar != null && nav.bar.enabled;

        // The bar's groups for this wall: its rows when it has any (the hidden ones left out), else the families' defaults
        public static List<Item> Items(CardNavigationSettings nav, BlockRegistry registry)
        {
            var rows = nav?.bar?.items;
            if (rows == null || rows.Count == 0) return DefaultItems(nav, registry);
            var items = new List<Item>();
            foreach (var row in rows)
                if (row != null && row.shown && !string.IsNullOrEmpty(row.key)) items.Add(ItemOf(row, registry));
            return items;
        }

        // The rows a wall with no table of its own reads (and the table's first edit writes): one per navigable family, in their order
        public static List<CardNavBarItem> DefaultRows(CardNavigationSettings nav, BlockRegistry registry)
        {
            var rows = new List<CardNavBarItem>();
            foreach (var family in FamiliesInOrder(nav, registry))
            {
                if (!family.InNav) continue;
                rows.Add(new CardNavBarItem { key = family.Key, families = new List<string> { family.Key } });
            }
            return rows;
        }

        public static List<Item> DefaultItems(CardNavigationSettings nav, BlockRegistry registry)
        {
            var items = new List<Item>();
            foreach (var row in DefaultRows(nav, registry)) items.Add(ItemOf(row, registry));
            return items;
        }

        // A row as the card reads it: a blank icon, selected icon or colour takes its first family's (the selected icon: the icon)
        public static Item ItemOf(CardNavBarItem row, BlockRegistry registry)
        {
            var families = row.families ?? new List<string>();
            FamilyDefinition first = null;
            foreach (string key in families)
                if (registry != null && registry.TryGetFamily(key, out first)) break;
            string icon = !string.IsNullOrEmpty(row.icon) ? row.icon : first?.IconKey;
            string selected = !string.IsNullOrEmpty(row.icon_selected) ? row.icon_selected : icon;
            string colour = !string.IsNullOrEmpty(row.color) ? row.color : first?.ColorToken;
            return new Item(row.key, row.label, string.IsNullOrEmpty(first?.LabelKey) ? null : first.LabelKey, icon, selected, colour, families);
        }

        // The groups this card shows: each item with its targets here, in the table's order; an item with none is left out
        public static List<Entry> Entries(IReadOnlyList<Item> items, IReadOnlyList<string> targets, System.Func<string, string> familyOf)
        {
            var entries = new List<Entry>();
            if (items == null || targets == null || familyOf == null) return entries;
            var ofItem = new List<string>[items.Count];
            for (int i = 0; i < items.Count; i++) ofItem[i] = new List<string>();
            foreach (string target in targets)
            {
                int at = ItemOfFamily(items, familyOf(target));
                if (at >= 0) ofItem[at].Add(target);
            }
            for (int i = 0; i < items.Count; i++)
                if (ofItem[i].Count > 0) entries.Add(new Entry(items[i], ofItem[i]));
            return entries;
        }

        // The index of the entry a block belongs to, -1 for none
        public static int EntryOf(IReadOnlyList<Entry> entries, string block)
        {
            if (entries == null || block == null) return -1;
            for (int i = 0; i < entries.Count; i++)
                foreach (string target in entries[i].Targets)
                    if (target == block) return i;
            return -1;
        }

        // The first item that holds this family (a family in two rows belongs to the first), -1 for none
        public static int ItemOfFamily(IReadOnlyList<Item> items, string family)
        {
            if (items == null || family == null) return -1;
            for (int i = 0; i < items.Count; i++)
                if (items[i].Families != null)
                    foreach (string key in items[i].Families)
                        if (key == family) return i;
            return -1;
        }

        // Whether the bar is drawn on this card
        public static bool Shows(CardNavigationSettings nav, bool cardShowsNavigation, int entries) =>
            Enabled(nav) && cardShowsNavigation && entries >= MinEntries;

        // The words a visitor reads for a group: the row's own in the card's language (then the fallback, then any), else its first family's
        // word, else its key
        public static string Words(Item item, CardStrings strings)
        {
            if (item.Label != null && item.Label.Count > 0)
            {
                string own = BlockFieldReader.Pick(new List<LocalizedEntry>(item.Label), strings?.Language, strings?.FallbackLanguage);
                if (!string.IsNullOrWhiteSpace(own)) return own;
            }
            if (item.LabelKey != null && strings != null)
            {
                string words = strings.Get(item.LabelKey);
                if (!string.IsNullOrWhiteSpace(words)) return words;
            }
            return item.Key;
        }

        // ---- the options, each read with its default ----

        public static string Position(CardNavBarSettings bar) => Pick(bar?.position, CardOptions.NavBarPositions);
        public static bool AtBottom(CardNavBarSettings bar) => Position(bar) == CardOptions.NavBarBottom;
        public static string Show(CardNavBarSettings bar) => Pick(bar?.show, CardOptions.NavBarShows);
        public static string HideOnScroll(CardNavBarSettings bar) => Pick(bar?.hide_on_scroll, CardOptions.NavBarHides);
        public static string Shape(CardNavBarSettings bar) => Pick(bar?.shape, CardOptions.NavBarShapes);
        public static string Fill(CardNavBarSettings bar) => Pick(bar?.fill, CardOptions.NavBarFills);
        public static string Indicator(CardNavBarSettings bar) => Pick(bar?.indicator, CardOptions.NavBarIndicators);
        public static string SelectedColour(CardNavBarSettings bar) => Pick(bar?.selected_color, CardOptions.NavBarSelectedColours);
        public static string IdleColour(CardNavBarSettings bar) => Pick(bar?.idle_color, CardOptions.NavBarIdleColours);
        public static string Labels(CardNavBarSettings bar) => Pick(bar?.labels, CardOptions.NavBarLabels);

        // The numbers, held to their ranges (a full-width bar has no radius and no margin)
        public static float CornerRadius(CardNavBarSettings bar) =>
            Shape(bar) == CardOptions.NavBarFullWidth ? 0f : Mathf.Clamp(bar?.corner_radius ?? 26f, 0f, CardNavBarSettings.CornerRadiusMax);
        public static float SideMargin(CardNavBarSettings bar) =>
            Shape(bar) == CardOptions.NavBarFullWidth ? 0f : Mathf.Clamp(bar?.side_margin ?? 12f, 0f, CardNavBarSettings.SideMarginMax);
        public static float Height(CardNavBarSettings bar) =>
            Mathf.Clamp(bar?.height ?? 56f, CardNavBarSettings.HeightMin, CardNavBarSettings.HeightMax);
        public static float FillOpacity(CardNavBarSettings bar) => Mathf.Clamp(bar?.fill_opacity ?? 1f, CardNavBarSettings.FillOpacityMin, 1f);

        // Whether an item shows its icon / its words (a group with no icon always shows its words)
        public static bool ShowsIcon(string labels, bool hasIcon) => hasIcon && labels != CardOptions.NavBarLabelsText;
        public static bool ShowsWords(string labels, bool hasIcon, bool chosen) =>
            !hasIcon || labels == CardOptions.NavBarLabelsIconAndText || labels == CardOptions.NavBarLabelsText
            || labels == CardOptions.NavBarLabelsSelectedOnly && chosen;

        private static string Pick(string value, string[] options)
        {
            foreach (string option in options)
                if (option == value) return value;
            return options[0];
        }

        // Every registered family in its effective order (the wall's Families row when it has one, else the registered default), stable
        private static List<FamilyDefinition> FamiliesInOrder(CardNavigationSettings nav, BlockRegistry registry)
        {
            var families = registry != null ? new List<FamilyDefinition>(registry.Families) : new List<FamilyDefinition>();
            int OrderOf(FamilyDefinition family)
            {
                if (nav?.families != null)
                    foreach (var row in nav.families)
                        if (row != null && row.family == family.Key) return row.order;
                return family.Order;
            }
            for (int i = 1; i < families.Count; i++)
            {
                var family = families[i];
                int order = OrderOf(family);
                int at = i;
                while (at > 0 && OrderOf(families[at - 1]) > order) { families[at] = families[at - 1]; at--; }
                families[at] = family;
            }
            return families;
        }
    }
}
