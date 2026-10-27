// POIEditorToolWindow.CardNavigationBar.cs
//
// Partial: Detail Card > Navigation > Navigation Bar (_3.3 S4a review) -- the FIRST component of the Navigation section: ONE configurable bar
// of the card's groups (card_settings.navigation.bar, NavBarView). Its Enabled row, then four sub-components, each under a sub-heading:
//   - Bar Look: Shape, Corner Radius, Side Margin, Height, Fill, Fill Opacity, Border, Shadow
//   - Chosen Group: Indicator, Slide Indicator, Bold Chosen Icon, Enlarge Chosen Icon, Chosen Colour, Other Groups' Colour, Labels
//   - Where And When: Position, Show, Hide While Scrolling
//   - Bar Groups: one folding row per group (Shown, its name, ^ / v, delete), opened: its Name per language, Icon, Chosen Icon, Colour and
//     Families; "+ Add group", "Reset To The Families", and the warnings (a family in no group, too many groups for a phone)
// While the bar is on, the section's Elements rows are hidden (the card draws the bar instead; their values are kept). Drawing never writes:
// a wall with no Bar Groups rows sees one group per family, and its first edit of the table writes them all (MaterializeBarItems).
// The (i) texts live in POIEditorToolWindow.CardNavigationBarHelp.cs.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        internal static readonly string[] NavigationBarSubTitles = { "Bar Look", "Chosen Group", "Where And When", "Bar Groups" };
        // The groups the developer unfolded in the Bar Groups table (by key; a session's view state, never saved)
        private readonly HashSet<string> _openBarGroups = new HashSet<string>();
        // Wider than this many groups, a phone gives each one under 60 points (390 / 7): the words of a group crowd
        internal const int NavBarComfortableGroups = 6;

        // The component: its title, Enabled, and -- while on -- its four sub-components
        private void DrawNavigationBarComponent(CardNavigationSettings nav)
        {
            var bar = nav.bar ??= new CardNavBarSettings();
            DrawEditorRow(out float titleRow, out _);
            GUILayout.Label("Navigation Bar", EditorStyles.boldLabel, GUILayout.Width(Mathf.Max(40f, titleRow - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw("Navigation Bar", CardNavBarHelp);
            EditorRowEnd();
            bar.enabled = DrawToggleField("Enabled", bar.enabled, CardNavBarEnabledHelp);
            if (!bar.enabled) return;

            DrawNavigationBarSubHeading(0);
            bar.shape = DrawPopupField("Shape", bar.shape, CardOptions.NavBarShapes, CardOptions.NavBarShapeLabels, CardNavBarShapeHelp);
            // - a full-width bar has no corners and no margin: the two rows would change nothing
            if (NavBarRule.Shape(bar) == CardOptions.NavBarFloating)
            {
                bar.corner_radius = DrawSliderField("Corner Radius", bar.corner_radius, 0f, CardNavBarSettings.CornerRadiusMax, CardNavBarCornerRadiusHelp);
                bar.side_margin = DrawSliderField("Side Margin", bar.side_margin, 0f, CardNavBarSettings.SideMarginMax, CardNavBarSideMarginHelp);
            }
            bar.height = DrawSliderField("Height", bar.height, CardNavBarSettings.HeightMin, CardNavBarSettings.HeightMax, CardNavBarHeightHelp);
            bar.fill = DrawPopupField("Fill", bar.fill, CardOptions.NavBarFills, CardOptions.NavBarFillLabels, CardNavBarFillHelp);
            if (NavBarRule.Fill(bar) != CardOptions.NavBarClear)
            {
                bar.fill_opacity = DrawSliderField("Fill Opacity", bar.fill_opacity, CardNavBarSettings.FillOpacityMin, 1f, CardNavBarFillOpacityHelp);
                bar.border = DrawToggleField("Border", bar.border, CardNavBarBorderHelp);
                bar.shadow = DrawToggleField("Shadow", bar.shadow, CardNavBarShadowHelp);
            }

            DrawNavigationBarSubHeading(1);
            bar.indicator = DrawPopupField("Indicator", bar.indicator, CardOptions.NavBarIndicators, CardOptions.NavBarIndicatorLabels, CardNavBarIndicatorHelp);
            if (NavBarRule.Indicator(bar) != CardOptions.NavBarIndicatorNone)
                bar.slide_indicator = DrawToggleField("Slide Indicator", bar.slide_indicator, CardNavBarSlideHelp);
            bar.bold_selected = DrawToggleField("Bold Chosen Icon", bar.bold_selected, CardNavBarBoldHelp);
            bar.enlarge_selected = DrawToggleField("Enlarge Chosen Icon", bar.enlarge_selected, CardNavBarEnlargeHelp);
            bar.selected_color = DrawPopupField("Chosen Colour", bar.selected_color, CardOptions.NavBarSelectedColours, CardOptions.NavBarSelectedColourLabels,
                CardNavBarChosenColourHelp);
            bar.idle_color = DrawPopupField("Other Groups' Colour", bar.idle_color, CardOptions.NavBarIdleColours, CardOptions.NavBarIdleColourLabels,
                CardNavBarIdleColourHelp);
            bar.labels = DrawPopupField("Labels", bar.labels, CardOptions.NavBarLabels, CardOptions.NavBarLabelLabels, CardNavBarLabelsHelp);

            DrawNavigationBarSubHeading(2);
            bar.position = DrawPopupField("Position", bar.position, CardOptions.NavBarPositions, CardOptions.NavBarPositionLabels, CardNavBarPositionHelp);
            bar.show = DrawPopupField("Show", bar.show, CardOptions.NavBarShows, CardOptions.NavBarShowLabels, CardNavBarShowHelp);
            bar.hide_on_scroll = DrawPopupField("Hide While Scrolling", bar.hide_on_scroll, CardOptions.NavBarHides, CardOptions.NavBarHideLabels,
                CardNavBarHideHelp);

            DrawNavigationBarSubHeading(3);
            DrawBarGroupsTable(nav);
        }

        private void DrawNavigationBarSubHeading(int index)
        {
            GUILayout.Space(TableGapBetweenGroups);
            DrawEditorRow(out float rowWidth, out _, IndentLevel1);
            GUILayout.Label(NavigationBarSubTitles[index], EditorStyles.miniBoldLabel, GUILayout.Width(Mathf.Max(40f, rowWidth - 36f)), GUILayout.ExpandWidth(false));
            if (NavigationGroupProbe != null && Event.current.type == EventType.Repaint)
                NavigationGroupProbe(NavigationBarSubTitles[index], GUILayoutUtility.GetLastRect());
            EditorRowEnd();
        }

        // ---------------- Bar Groups ----------------

        // One folding row per group (the wall's rows, or the families' defaults), then the table's buttons and its warnings
        private void DrawBarGroupsTable(CardNavigationSettings nav)
        {
            var languages = WallLanguages();
            DrawEditorRow(out float titleRow, out _, IndentLevel1);
            GUILayout.Label("Groups, in the bar's order", EditorStyles.miniLabel, GUILayout.Width(Mathf.Max(40f, titleRow - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw("Bar Groups", CardNavBarGroupsHelp);
            EditorRowEnd();

            var rows = nav.bar.items != null && nav.bar.items.Count > 0 ? nav.bar.items : NavBarRule.DefaultRows(nav, BlockRegistry.Shared);
            int moveAt = -1, moveBy = 0, deleteAt = -1;
            for (int i = 0; i < rows.Count; i++)
            {
                int row = i;
                var item = rows[i];
                if (item == null) continue;
                string name = BarGroupName(item, languages[0]);

                // - the group's line: Shown, its fold with its name, its arrows, its delete
                DrawEditorRow(out float rowWidth, out _, IndentLevel1);
                bool shown = EditorGUILayout.Toggle(item.shown, GUILayout.Width(18f));
                ReportTableCellRect("Bar group shown", i);
                if (shown != item.shown) MaterializeBarItems(nav)[row].shown = shown;
                bool open = _openBarGroups.Contains(item.key);
                bool unfolded = EditorGUILayout.Foldout(open, (i + 1) + ". " + name + (item.shown ? "" : "  (hidden)"), true);
                if (unfolded != open)
                {
                    if (unfolded) _openBarGroups.Add(item.key);
                    else _openBarGroups.Remove(item.key);
                }
                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledScope(i == 0))
                    if (GUILayout.Button("^", EditorStyles.miniButton, GUILayout.Width(FamilyArrowButtonWidth))) { moveAt = i; moveBy = -1; }
                ReportTableCellRect("Bar group up", i);
                using (new EditorGUI.DisabledScope(i >= rows.Count - 1))
                    if (GUILayout.Button("v", EditorStyles.miniButton, GUILayout.Width(FamilyArrowButtonWidth))) { moveAt = i; moveBy = 1; }
                ReportTableCellRect("Bar group down", i);
                if (DeleteButton.DrawLayout("Delete group " + name)) deleteAt = i;
                ReportTableCellRect("Bar group delete", i);
                EditorRowEnd();
                if (!unfolded) continue;

                // - opened: what the group is made of
                var label = new BlockFieldDefinition { Key = "label", Label = "Name", Type = BlockFieldType.LocalizedText, Help = CardNavBarGroupNameHelp };
                DrawLocalizedRows(label, languages, IndentLevel2, "Bar group name", i,
                    lang => LanguageEntryValue(item.label, lang),
                    (lang, text) => SetLanguage(MaterializeBarItems(nav)[row].label ??= new List<LocalizedEntry>(), lang, text));
                var (iconKeys, iconLabels) = BarIconChoices("Its Family's Icon");
                string icon = DrawPopupField("Icon", item.icon ?? "", iconKeys, iconLabels, CardNavBarGroupIconHelp, IndentLevel1);
                if (icon != (item.icon ?? "")) MaterializeBarItems(nav)[row].icon = icon;
                var (chosenKeys, chosenLabels) = BarIconChoices("Same As Icon");
                string chosen = DrawPopupField("Chosen Icon", item.icon_selected ?? "", chosenKeys, chosenLabels, CardNavBarGroupChosenIconHelp, IndentLevel1);
                if (chosen != (item.icon_selected ?? "")) MaterializeBarItems(nav)[row].icon_selected = chosen;
                var (colourKeys, colourLabels) = BarColourChoices();
                string colour = DrawPopupField("Colour", item.color ?? "", colourKeys, colourLabels, CardNavBarGroupColourHelp, IndentLevel1);
                if (colour != (item.color ?? "")) MaterializeBarItems(nav)[row].color = colour;
                DrawBarGroupFamilies(nav, item, row);
            }

            DrawEditorRow(out float addRow, out _, IndentLevel1);
            float half = Mathf.Max(40f, (addRow - 36f) * 0.5f - FamilyCellGap);
            if (GUILayout.Button("+ Add group", GUILayout.Width(half), GUILayout.ExpandWidth(false)))
            {
                var all = MaterializeBarItems(nav);
                string key = TaxonomyRowKeys.NextFree(all, g => g.key, TaxonomyRowKeys.GroupPrefix);
                all.Add(new CardNavBarItem { key = key });
                _openBarGroups.Add(key);
            }
            ReportTableCellRect("Bar group add", 0);
            using (new EditorGUI.DisabledScope(nav.bar.items == null || nav.bar.items.Count == 0))
                if (GUILayout.Button("Reset To The Families", GUILayout.Width(half), GUILayout.ExpandWidth(false)))
                    nav.bar.items.Clear();
            ReportTableCellRect("Bar groups reset", 0);
            EditorRowEnd();

            if (moveAt >= 0)
            {
                var all = MaterializeBarItems(nav);
                POIEditorToolWindow.ReorderRow(all, moveAt, moveBy);
            }
            if (deleteAt >= 0) MaterializeBarItems(nav).RemoveAt(deleteAt);

            foreach (string note in BarGroupWarnings(nav, languages[0])) DrawNavigationMessage(note, MessageType.Warning);
        }

        // The Families row of an opened group: a multi-pick of the families a visitor can navigate to, by their words
        private void DrawBarGroupFamilies(CardNavigationSettings nav, CardNavBarItem item, int row)
        {
            var families = new List<FamilyDefinition>();
            foreach (var family in FamiliesInEffectiveOrder(nav))
                if (family.InNav) families.Add(family);
            var names = new string[families.Count];
            int mask = 0;
            for (int f = 0; f < families.Count; f++)
            {
                string words = FamilyWords(families[f]);
                names[f] = string.IsNullOrWhiteSpace(words) ? families[f].Key : words;
                if (item.families != null && item.families.Contains(families[f].Key)) mask |= 1 << f;
            }
            DrawEditorRow(out float rowWidth, out _, IndentLevel1);
            EditorGUILayout.PrefixLabel("Families");
            int picked = EditorGUILayout.MaskField(mask, names, GUILayout.Width(Mathf.Max(40f, rowWidth - EditorGUIUtility.labelWidth - 36f)),
                GUILayout.ExpandWidth(false));
            ReportTableCellRect("Bar group families", row);
            HelpInfoButton.Draw("Families", CardNavBarGroupFamiliesHelp);
            EditorRowEnd();
            if (picked == mask) return;
            var keys = new List<string>();
            for (int f = 0; f < families.Count; f++)
                if ((picked & (1 << f)) != 0) keys.Add(families[f].Key);
            MaterializeBarItems(nav)[row].families = keys;
        }

        // The wall's own Bar Groups rows, written now if it has none: the families' defaults, so the first edit changes only what was edited
        internal static List<CardNavBarItem> MaterializeBarItems(CardNavigationSettings nav)
        {
            nav.bar ??= new CardNavBarSettings();
            nav.bar.items ??= new List<CardNavBarItem>();
            if (nav.bar.items.Count == 0) nav.bar.items.AddRange(NavBarRule.DefaultRows(nav, BlockRegistry.Shared));
            return nav.bar.items;
        }

        // A group's name in one language, as the card shows it: its own words, else its first family's word, else its key
        private string BarGroupName(CardNavBarItem item, string language)
        {
            string own = LanguageEntryValue(item.label, language);
            if (!string.IsNullOrWhiteSpace(own)) return own;
            if (item.families != null)
                foreach (string key in item.families)
                    if (BlockRegistry.Shared.TryGetFamily(key, out var family))
                    {
                        string words = FamilyWords(family);
                        if (!string.IsNullOrWhiteSpace(words)) return words;
                    }
            return item.key;
        }

        // An Icon popup's choices: the blank (its family's icon / the same icon), then every icon of the card's set by a readable name
        internal static (string[] Keys, string[] Labels) BarIconChoices(string blankLabel)
        {
            var keys = new List<string> { "" };
            var labels = new List<string> { blankLabel };
            foreach (string key in CardIcons.All)
            {
                keys.Add(key);
                labels.Add(ObjectNames.NicifyVariableName(key.Replace('_', ' ')));
            }
            return (keys.ToArray(), labels.ToArray());
        }

        // The Colour popup's choices: the blank (its family's colour), each family's theme colour by the family's words, then the accent
        private (string[] Keys, string[] Labels) BarColourChoices()
        {
            var keys = new List<string> { "" };
            var labels = new List<string> { "Its Family's Colour" };
            foreach (var family in FamiliesInEffectiveOrder(_config.card_settings.navigation))
            {
                if (string.IsNullOrEmpty(family.ColorToken) || keys.Contains(family.ColorToken)) continue;
                string words = FamilyWords(family);
                keys.Add(family.ColorToken);
                labels.Add((string.IsNullOrWhiteSpace(words) ? family.Key : words) + " Colour");
            }
            keys.Add("--ts-accent");
            labels.Add("Accent");
            return (keys.ToArray(), labels.ToArray());
        }

        // The warnings under the table: each navigable family no shown group holds (named by its words), a shown group with no family, and
        // more groups than a phone fits comfortably
        internal List<string> BarGroupWarnings(CardNavigationSettings nav, string language)
        {
            var notes = new List<string>();
            var items = NavBarRule.Items(nav, BlockRegistry.Shared);
            var outside = new List<string>();
            foreach (var family in FamiliesInEffectiveOrder(nav))
                if (family.InNav && NavBarRule.ItemOfFamily(items, family.Key) < 0)
                {
                    string words = FamilyWords(family);
                    outside.Add(string.IsNullOrWhiteSpace(words) ? family.Key : words);
                }
            if (outside.Count > 0)
                notes.Add("In no shown group: " + string.Join(", ", outside) + ". The bar has no way to these families' blocks; add them to a "
                    + "group's Families, or show the group that holds them.");
            var rows = nav.bar?.items;
            if (rows != null)
                foreach (var row in rows)
                    if (row != null && row.shown && (row.families == null || row.families.Count == 0))
                        notes.Add("The group " + BarGroupName(row, language) + " has no families: it leads nowhere, so no card shows it. Pick its "
                            + "Families.");
            // - a card shows only the groups it has blocks of: the busiest card says whether the bar crowds (built only for a long table)
            if (items.Count > NavBarComfortableGroups)
            {
                var busiest = NavigationWarningRule.BusiestBar(_config.card_settings, _config.pois, BlockRegistry.Shared);
                if (busiest.Sections > NavBarComfortableGroups)
                    notes.Add(NavigationCardName(busiest.PoiIndex) + " shows " + busiest.Sections + " groups: on a phone each gets under 60 points, so "
                        + "their words crowd. Hide a group, merge two, or set Labels to Icon Only.");
            }
            return notes;
        }
    }
}
