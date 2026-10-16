// POIEditorToolWindow.CardNavigationGroups.cs
//
// Partial: Detail Card > Navigation's Groups (_3.2_POI_Card_Navigation.md sections 3 and 7) -- what the Segmented Top Bar shows:
// the Groups table (one row per group, its name in every wall language, a delete, "+ Add group") and the Families table's Group
// column (which group a family joins). Both are drawn only while the Top Bar's element shows Groups.
// Drawing never writes (_5.1 "Config, rig and undo", rule 9a): a wall with no rows sees the framework's four groups, and its rows
// are written by its first real edit, with those four's words copied in so nothing it saw changes.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // The Groups table: every group the wall's cards use, named in each wall language, then a line naming the families that are in
        // no group (a Segmented card never shows their blocks)
        private void DrawGroupsTable(CardNavigationSettings nav)
        {
            var languages = WallLanguages();
            DrawEditorRow(out float titleRow, out _);
            GUILayout.Label("Groups (the Segmented bar)", EditorStyles.miniBoldLabel, GUILayout.Width(Mathf.Max(40f, titleRow - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw("Groups", CardGroupsHelp);
            EditorRowEnd();

            var groups = NavGroupRule.Groups(nav);
            int deleteAt = -1;
            for (int i = 0; i < groups.Count; i++)
            {
                int row = i;
                var group = groups[i];
                if (i > 0) GUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);
                DrawEditorRow(out float rowWidth, out _, IndentLevel1);
                GUILayout.Label("Group " + (i + 1) + ": " + GroupName(group, languages[0]), GUILayout.Width(Mathf.Max(40f, rowWidth - 36f)), GUILayout.ExpandWidth(false));
                if (DeleteButton.DrawLayout("Delete group " + GroupName(group, languages[0]))) deleteAt = i;
                ReportTableCellRect("Group delete", i);
                EditorRowEnd();

                var name = new BlockFieldDefinition { Key = "label", Label = "Name", Type = BlockFieldType.LocalizedText, Help = CardGroupNameHelp };
                DrawLocalizedRows(name, languages, IndentLevel1, "Group name", i,
                    lang => GroupName(group, lang),
                    (lang, text) =>
                    {
                        var rows = MaterializeGroups(nav);
                        SetLanguage(rows[row].label ??= new List<LocalizedEntry>(), lang, text);
                    });
            }

            DrawEditorRow(out float addRow, out _, IndentLevel1);
            if (GUILayout.Button("+ Add group", GUILayout.Width(Mathf.Max(40f, addRow)), GUILayout.ExpandWidth(false)))
            {
                var rows = MaterializeGroups(nav);
                rows.Add(new CardNavGroup { key = TaxonomyRowKeys.NextFree(rows, g => g.key, TaxonomyRowKeys.GroupPrefix) });
            }
            ReportTableCellRect("Group add", 0);
            EditorRowEnd();
            if (deleteAt >= 0) MaterializeGroups(nav).RemoveAt(deleteAt);

            // - always true of the current state, so a static hint (not a popup): what a Segmented card leaves out, and a group no visitor
            //   could read
            var notes = GroupWarnings(nav, languages[0]);
            if (notes.Count > 0)
                foreach (string note in notes) EditorGUILayout.HelpBox(note, MessageType.Warning);
        }

        // One Families row's Group cell: a popup of the wall's groups by name, and "(no group)". Writes only when the developer picks
        // another one (creating the family's row then, like At The End)
        private void DrawFamilyGroupCell(CardNavigationSettings nav, FamilyDefinition family, Rect rect, int row, List<string> keys, List<string> names)
        {
            string current = NavGroupRule.GroupOf(family.Key, nav) ?? CardOptions.NavNone;
            int index = Mathf.Max(0, keys.IndexOf(current));
            int picked = EditorGUI.Popup(rect, index, names.ToArray());
            ReportTableCellRect("Family group", row, rect);
            if (picked != index) SetFamilyGroup(nav, family.Key, keys[picked]);
        }

        // What the Group column offers: each group of the wall by its name (first wall language), then no group at all
        internal (List<string> Keys, List<string> Names) GroupChoices(CardNavigationSettings nav)
        {
            string language = WallLanguages()[0];
            var keys = new List<string>();
            var names = new List<string>();
            foreach (var group in NavGroupRule.Groups(nav))
            {
                keys.Add(group.Key);
                names.Add(GroupName(group, language));
            }
            keys.Add(CardOptions.NavNone);
            names.Add(NoGroupLabel);
            return (keys, names);
        }

        private const string NoGroupLabel = "(no group)";

        // A family joins a group ("none" = no group: a key no group ever has, so NavGroupRule finds none)
        internal static void SetFamilyGroup(CardNavigationSettings nav, string familyKey, string groupKey) => EnsureFamilyRow(nav, familyKey).group = groupKey;

        // The wall's own Groups rows, written now if it has none: the framework's four, with their words in every wall language copied in,
        // so the first edit changes only what the developer edited
        internal List<CardNavGroup> MaterializeGroups(CardNavigationSettings nav)
        {
            nav.groups ??= new List<CardNavGroup>();
            if (nav.groups.Count > 0) return nav.groups;
            foreach (var group in NavGroupRule.Groups(null))
            {
                var row = new CardNavGroup { key = group.Key };
                foreach (string language in WallLanguages()) row.label.Add(new LocalizedEntry { lang = language, value = GroupName(group, language) });
                nav.groups.Add(row);
            }
            return nav.groups;
        }

        // A group's name in one language, as the card would show it in that language (the wall's own words, else the framework's)
        private string GroupName(NavGroupRule.Group group, string language)
        {
            var strings = new CardStrings(FrameworkCardStrings()?.Entries(), CardStringSources.Shared.Entries(), _config.card_settings.strings, language, language);
            if (group.Label != null)
                foreach (var entry in group.Label)
                    if (entry != null && entry.lang == language && !string.IsNullOrWhiteSpace(entry.value)) return entry.value;
            return group.LabelKey != null ? strings.Get(group.LabelKey) : "";
        }

        // The warnings under the Groups table: each family in no group (named by its words), each group with no name in a language
        internal List<string> GroupWarnings(CardNavigationSettings nav, string language)
        {
            var notes = new List<string>();
            var outside = new List<string>();
            foreach (var family in FamiliesInEffectiveOrder(nav))
                if (family.InNav && NavGroupRule.GroupOf(family.Key, nav) == null)
                {
                    string words = FamilyWords(family);
                    outside.Add(string.IsNullOrWhiteSpace(words) ? "a family with no name" : words);
                }
            if (outside.Count > 0)
                notes.Add("In no group: " + string.Join(", ", outside) + ". A card with the Segmented Top Bar has no segment for these "
                    + "families' blocks; pick a group for them in the Families table's Group column.");
            var groups = NavGroupRule.Groups(nav);
            for (int i = 0; i < groups.Count; i++)
                foreach (string lang in WallLanguages())
                    if (string.IsNullOrWhiteSpace(GroupName(groups[i], lang)))
                        notes.Add("Group " + (i + 1) + " has no name in " + lang + ": its segment would show nothing a visitor can read. Type one above.");
            return notes;
        }

        // The wall's languages, the first one first ("en" for a wall that lists none)
        private List<string> WallLanguages()
        {
            var languages = _config.card_settings?.languages;
            return languages != null && languages.Count > 0 ? languages : new List<string> { "en" };
        }
    }
}
