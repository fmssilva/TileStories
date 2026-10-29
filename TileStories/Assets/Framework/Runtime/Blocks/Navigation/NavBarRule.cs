using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // The Navigation Bar (_3.3 S4 third round, v5), pure: what NavBarView draws from card_settings.navigation.bar and the card's map.
    //   - Items: the wall's Bar Groups rows, or -- with none -- the framework's six groups (DefaultRows: one per navigable family, in the
    //     families' effective order, AR inside Media and Community inside Play), each with its first family's words, icon and colour
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

        // The families a default group takes in beside its own (the joiner -> the family whose group it joins): six groups, so each keeps
        // a phone's comfortable width (390 / 6) -- AR's blocks show the point (Media), Community's ask the visitor something (Play)
        public static readonly IReadOnlyDictionary<string, string> DefaultJoins = new Dictionary<string, string>
        {
            { BuiltInFamilies.Ar, BuiltInFamilies.Media },
            { BuiltInFamilies.Community, BuiltInFamilies.Play },
        };

        // The rows a wall with no table of its own reads (and the table's first edit writes): one per navigable family in their order, a
        // joiner (DefaultJoins) inside its host's row when the host is navigable too -- an app's own family gets a row of its own
        public static List<CardNavBarItem> DefaultRows(CardNavigationSettings nav, BlockRegistry registry)
        {
            var rows = new List<CardNavBarItem>();
            var navigable = new List<FamilyDefinition>();
            foreach (var family in FamiliesInOrder(nav, registry))
                if (family.InNav) navigable.Add(family);
            bool Hosted(FamilyDefinition family) =>
                DefaultJoins.TryGetValue(family.Key, out string host) && navigable.Exists(f => f.Key == host);
            foreach (var family in navigable)
                if (!Hosted(family)) rows.Add(new CardNavBarItem { key = family.Key, families = new List<string> { family.Key } });
            foreach (var family in navigable)
                if (Hosted(family)) rows.Find(row => row.key == DefaultJoins[family.Key]).families.Add(family.Key);
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

        // Position: a pre-v5 "under_title" is Top
        public static string Position(CardNavBarSettings bar) =>
            bar?.position == CardOptions.NavBarUnderTitle ? CardOptions.NavBarTop : Pick(bar?.position, CardOptions.NavBarPositions, CardOptions.NavBarBottom);
        public static bool AtBottom(CardNavBarSettings bar) => Position(bar) == CardOptions.NavBarBottom;
        public static string Shape(CardNavBarSettings bar) => Pick(bar?.shape, CardOptions.NavBarShapes, CardOptions.NavBarFloating);
        public static bool Floating(CardNavBarSettings bar) => Shape(bar) == CardOptions.NavBarFloating;
        public static string Fill(CardNavBarSettings bar) => Pick(bar?.fill, CardOptions.NavBarFills, CardOptions.NavBarSolid);
        public static string Indicator(CardNavBarSettings bar) => Pick(bar?.indicator, CardOptions.NavBarIndicators, CardOptions.NavBarIndicatorRaised);
        public static bool Raised(CardNavBarSettings bar) => Indicator(bar) == CardOptions.NavBarIndicatorRaised;
        // Raised Depth, -1..+1 (NavBarShapeRule.EdgeRise): an unreadable one is the default, +1
        public const float RaisedDepthMin = -1f, RaisedDepthMax = 1f, RaisedDepthDefault = 1f;
        public static float RaisedDepth(CardNavBarSettings bar) =>
            bar == null || float.IsNaN(bar.raised_depth) ? RaisedDepthDefault : Mathf.Clamp(bar.raised_depth, RaisedDepthMin, RaisedDepthMax);
        public static string ChosenIcon(CardNavBarSettings bar) => Pick(bar?.chosen_icon, CardOptions.NavBarChosenIcons, CardOptions.NavBarChosenIconFilled);
        public static string Labels(CardNavBarSettings bar) => Pick(bar?.labels, CardOptions.NavBarLabels, CardOptions.NavBarLabelsIconAndText);
        public static string SelectedColour(CardNavBarSettings bar) =>
            Pick(bar?.selected_color, CardOptions.NavBarSelectedColours, CardOptions.NavBarColourGroup);
        public static string IconColour(CardNavBarSettings bar) => Pick(bar?.icon_color, CardOptions.NavBarIconColours, CardOptions.NavBarIconGroup);

        // The token an idle group's icon is drawn in on the bar (Icon Colour): its group's -text tone (the colour's readable tone: a family's
        // -text, the accent's --ts-accent-text; a group with no colour the idle ink), the accent's text tone, or the idle ink
        public static string IconToken(string iconColour, string groupColourToken) =>
            iconColour == CardOptions.NavBarIconAccent ? "--ts-accent-text"
            : iconColour == CardOptions.NavBarIconGroup && !string.IsNullOrEmpty(groupColourToken) ? groupColourToken + "-text"
            : "--ts-ink-2";

        // The token of the ink drawn ON a colour (the chosen disc's icon): the accent's --ts-on-accent, a family colour's own -ink
        public static string InkOn(string colourToken) =>
            string.IsNullOrEmpty(colourToken) || colourToken == "--ts-accent" ? "--ts-on-accent" : colourToken + "-ink";
        public static bool BoldChosen(CardNavBarSettings bar) => bar?.bold_selected ?? true;
        public static string Show(CardNavBarSettings bar) => Pick(bar?.show, CardOptions.NavBarShows, CardOptions.NavBarShowAlways);
        public static string HideOnScroll(CardNavBarSettings bar) => Pick(bar?.hide_on_scroll, CardOptions.NavBarHides, CardOptions.NavBarHideNever);
        public static bool HidesWhileScrolling(CardNavBarSettings bar) => HideOnScroll(bar) == CardOptions.NavBarHideDown;
        // Locate Button: the card's "Show me where it is" button (LocateFabRule), on unless turned off
        public static bool LocateButton(CardNavBarSettings bar) => bar?.locate_button ?? true;
        public static string StickyAction(CardNavBarSettings bar) => Pick(bar?.sticky_action, CardOptions.NavBarStickyActions, CardOptions.NavBarStickyAuto);

        // Whether the card draws its sticky call to action (an Actions block in its footer look) -- in the hero and in the footer: Shown /
        // Hidden as picked; Automatic leaves it out while the bar floats at the bottom (the Locate Button and the bar stand where the footer
        // would). With the bar off it is drawn, as before the bar
        public static bool StickyActionShown(CardNavigationSettings nav)
        {
            if (!Enabled(nav)) return true;
            string sticky = StickyAction(nav.bar);
            return sticky == CardOptions.NavBarStickyShown || sticky == CardOptions.NavBarStickyAuto && !AtBottom(nav.bar);
        }

        // Whether the bar lies on the hero's picture (v5): at the bottom while the hero reaches `margin` below the bar's top edge, at Top while
        // it reaches that far below the bar's foot -- all in one frame; a card with no picture hero (NaN) never
        public static bool OverHero(bool atBottom, float barTop, float barBottom, float heroBottom, float margin) =>
            !float.IsNaN(heroBottom) && (atBottom ? barTop : barBottom) < heroBottom - margin;

        // Whether the hero's words rise clear of the bar: it floats at the bottom from the very start (Always), over the hero's foot
        public static bool ClearsHeroFoot(CardNavigationSettings nav) =>
            Enabled(nav) && AtBottom(nav.bar) && Show(nav.bar) == CardOptions.NavBarShowAlways;

        // The chosen group wears a disc: the raised bump's, or Filled's at the icon's own place -- never on a bar of words alone (v5)
        public static bool ChosenDisc(CardNavBarSettings bar) =>
            Raised(bar) || ChosenIcon(bar) == CardOptions.NavBarChosenIconFilled && Labels(bar) != CardOptions.NavBarLabelsText;

        // Whether an item shows its icon / its words (a group with no icon always shows its words)
        public static bool ShowsIcon(string labels, bool hasIcon) => hasIcon && labels != CardOptions.NavBarLabelsText;
        public static bool ShowsWords(string labels, bool hasIcon, bool chosen) =>
            !hasIcon || labels == CardOptions.NavBarLabelsIconAndText || labels == CardOptions.NavBarLabelsText
            || labels == CardOptions.NavBarLabelsSelectedOnly && chosen;

        private static string Pick(string value, string[] options, string fallback)
        {
            foreach (string option in options)
                if (option == value) return value;
            return fallback;
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
