// POIEditorToolWindow.BlockTemplates.cs
//
// Partial: Detail Card > Block Library > Templates (_3.3 R14.4, R14.5): the wall's block templates under the kinds table. One line per
// template -- the open chevron, its name (a text field for the wall's own; the built-ins are code and read-only), its kind and row count,
// Duplicate, and a delete for the wall's own (a confirm first: the blocks made from it keep their rows). An open template of the wall's
// draws its rows with the SAME element table as a block's Page (ComposedBlock.cs, template mode: no content, no marks). Everything runs
// in the Detail Card tab's DrawConfigMutationScope: one undo step per click or typed word, the unsaved flag, the live Play Mode push.
// The templates' data and rules are BlockTemplates (pure).

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private const float TemplateDuplicateWidth = 72f;

        // The UI-state owner of a template's table (ComposedKey's first part): never a POI's id
        internal static string TemplateOwner(string templateKey) => "template:" + templateKey;

        // The Templates table under the Block Library's kinds
        private void DrawBlockTemplatesTable()
        {
            var s = _config.card_settings;
            var elements = ElementRegistry.Shared;
            GUILayout.Space(TableGapBetweenGroups);
            DrawEditorRow(out float titleRow, out _);
            GUILayout.Label("Templates", EditorStyles.boldLabel, GUILayout.Width(Mathf.Max(40f, titleRow - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw("Templates", BlockTemplatesHelp);
            EditorRowEnd();

            var templates = BlockTemplates.All(s, elements);
            _composedEdit = null;
            System.Action after = null;
            for (int i = 0; i < templates.Count; i++)
            {
                var template = templates[i];
                bool builtIn = BlockTemplates.IsBuiltIn(template.key);
                string owner = TemplateOwner(template.key);
                string openKey = ComposedKey(owner, "", "open");
                bool open = IsComposedOpen(openKey, false);

                var r = StoryRowRect(IndentLevel0, StoryRowHeight);
                if (Event.current.type == EventType.Repaint && open) EditorGUI.DrawRect(r, StoryOpenRowTint);
                var chevron = new Rect(r.x, r.y + 2f, StoryCell - 6f, r.height - 4f);
                bool toggled = StoryChevron(chevron, open);
                ReportTableCellRect("Template open", i, chevron);

                float right = r.xMax;
                var delete = new Rect(right - StoryCell, r.y, StoryCell, EditorGUIUtility.singleLineHeight);
                right -= StoryCell + TableGapBeforeDelete;
                var duplicate = new Rect(right - TemplateDuplicateWidth, r.y, TemplateDuplicateWidth, EditorGUIUtility.singleLineHeight);
                float x = chevron.xMax + 4f;
                float nameWidth = Mathf.Max(60f, (duplicate.x - StoryGap - x) * 0.45f);
                var name = new Rect(x, r.y + 1f, nameWidth, EditorGUIUtility.singleLineHeight);
                var info = new Rect(name.xMax + 8f, r.y, Mathf.Max(0f, duplicate.x - StoryGap - name.xMax - 8f), r.height);

                if (builtIn) GUI.Label(name, BlockTemplates.NameOf(template), EditorStyles.boldLabel);
                else
                {
                    // - at indent 0: the line's rect already holds the indent, and an EditorGUI field would move right by the section's again
                    //   (its click target sliding off what is drawn: _5.1 "Tables")
                    int indent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0;
                    string edited = EditorGUI.TextField(name, template.name ?? "");
                    EditorGUI.indentLevel = indent;
                    if (edited != (template.name ?? "")) template.name = edited;
                }
                ReportTableCellRect("Template name", i, name);
                int rows = BlockTemplates.RowsOf(template).Count;
                string words = (BlockTemplates.KindOf(template)?.DisplayName ?? template.block?.kind) + ", " + (rows == 1 ? "1 row" : rows + " rows") +
                               (builtIn ? "  -  built in" : "");
                GUI.Label(info, new GUIContent(FitWithEllipsis(words, info.width, EditorStyles.miniLabel), BlockTemplates.Describe(template, elements)), EditorStyles.miniLabel);

                if (GUI.Button(duplicate, new GUIContent("Duplicate", "Make a copy of your own to edit"), EditorStyles.miniButton))
                {
                    var source = template;
                    after = () =>
                    {
                        var copy = BlockTemplates.Duplicate(_config.card_settings, _config.pois, source, ElementRegistry.Shared);
                        _composedOpen[ComposedKey(TemplateOwner(copy.key), "", "open")] = true;
                    };
                }
                ReportTableCellRect("Template duplicate", i, duplicate);
                if (!builtIn)
                {
                    if (DeleteButton.Draw(delete, "Delete the template " + BlockTemplates.NameOf(template)))
                    {
                        var gone = template;
                        after = () => DeleteBlockTemplate(gone);
                    }
                    ReportTableCellRect("Template delete", i, delete);
                }
                if (toggled != open) _composedOpen[openKey] = toggled;

                if (!open) continue;
                if (builtIn)
                {
                    DrawStoryNote(TemplateBuiltInNote, IndentLevel1);
                    DrawStoryNote(BlockTemplates.Describe(template, elements), IndentLevel1);
                    continue;
                }
                var kind = BlockTemplates.KindOf(template);
                if (kind == null || !BlockSurfaces.Composes(kind))
                {
                    DrawStoryNote("Its kind \"" + template.block?.kind + "\" is not a registered kind that composes a page.", IndentLevel1);
                    continue;
                }
                DrawStoryPage(owner, template.block, kind, ElementScope.Of(null, template.block, BlockRegistry.Shared), elements, null,
                    _config.card_settings?.languages, template: true);
            }
            // - after every line: a row's table edit, else a Duplicate / delete (one click, one edit)
            var edit = _composedEdit ?? after;
            _composedEdit = null;
            edit?.Invoke();
        }

        // Delete a template of the wall's, after ONE question -- every answer's outcome:
        //   Confirm ("Delete Template"): it leaves the list (one undo step: Ctrl+Z brings it back); the blocks made from it keep every row
        //   Cancel (also Esc, the X): nothing changes
        private void DeleteBlockTemplate(BlockTemplateData template)
        {
            int blocks = BlockTemplates.BlocksMadeFrom(_config.pois, template.key);
            if (EditorDecision.Ask(BuildDeleteTemplateQuestion(BlockTemplates.NameOf(template), blocks)) != DecisionAnswer.Confirm) return;
            BlockTemplates.Remove(_config.card_settings, template.key);
        }

        // Pure: the delete question from the real state -- the template by name, how many blocks were made from it, what each button does
        internal static DecisionRequest BuildDeleteTemplateQuestion(string templateName, int blocksMadeFromIt)
        {
            string made = blocksMadeFromIt == 0 ? "No block was made from it."
                : (blocksMadeFromIt == 1 ? "1 block was" : blocksMadeFromIt + " blocks were") +
                  " made from it: they keep every row and word, and their Template line will say the template is gone (no Re-apply).";
            return new DecisionRequest
            {
                Title = "Delete this template?",
                Message = "\"" + templateName + "\" leaves Detail Card > Block Library > Templates and the + From a template list.\n" + made +
                          "\nDelete Template: it goes, and one Ctrl+Z brings it back. Cancel: nothing changes.",
                ConfirmLabel = "Delete Template",
            };
        }
    }
}
