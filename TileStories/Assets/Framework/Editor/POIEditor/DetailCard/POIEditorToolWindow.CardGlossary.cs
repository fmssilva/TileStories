// POIEditorToolWindow.CardGlossary.cs
//
// Partial: Detail Card > Glossary (_3.1_POI_Card_Blocks.md section 8.1): card_settings.glossary, the words a card's long texts
// link with [[term]], each with its definition in every wall language.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // card_settings.glossary: one row per word (its Term, then its definition in each wall language), a delete per
        // row and "+ Add term". A card text links a word to its row with [[term]]. Drawing never writes.
        private void DrawCardGlossarySection()
        {
            var s = _config.card_settings;
            var languages = s.languages;
            DrawEditorRow(out float titleRow, out _);
            GUILayout.Label("Words a card text can link", EditorStyles.miniBoldLabel, GUILayout.Width(Mathf.Max(40f, titleRow - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw("Glossary", CardGlossaryHelp);
            EditorRowEnd();

            int count = s.glossary?.Count ?? 0;
            int deleteAt = -1;
            for (int i = 0; i < count; i++)
            {
                var entry = s.glossary[i];
                // - a gap between words, so one definition does not run into the next term
                if (i > 0) GUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);
                DrawEditorRow(out float rowWidth, out _, IndentLevel1);
                EditorGUILayout.PrefixLabel("Term");
                string term = EditorGUILayout.TextField(entry.term ?? "", GUILayout.Width(Mathf.Max(40f, rowWidth - EditorGUIUtility.labelWidth - 36f)), GUILayout.ExpandWidth(false));
                ReportTableCellRect("Glossary term", i);
                if (DeleteButton.DrawLayout("Delete glossary term " + entry.term)) deleteAt = i;
                ReportTableCellRect("Glossary delete", i);
                EditorRowEnd();
                if (term != (entry.term ?? "")) entry.term = term;

                if (languages == null || languages.Count == 0) continue;
                var definition = new BlockFieldDefinition { Key = "definition", Label = "Definition", Type = BlockFieldType.LocalizedLongText, Help = CardGlossaryDefinitionHelp };
                DrawLocalizedRows(definition, languages, IndentLevel1, "Glossary definition", i,
                    lang => entry.definition?.Find(e => e != null && e.lang == lang)?.value ?? "",
                    (lang, text) => SetLanguage(entry.definition ??= new List<LocalizedEntry>(), lang, text));
            }

            DrawEditorRow(out float addRow, out _, IndentLevel1);
            if (GUILayout.Button("+ Add term", GUILayout.Width(Mathf.Max(40f, addRow)), GUILayout.ExpandWidth(false)))
                (s.glossary ??= new List<GlossaryEntry>()).Add(new GlossaryEntry());
            ReportTableCellRect("Glossary add", 0);
            EditorRowEnd();
            if (deleteAt >= 0) s.glossary.RemoveAt(deleteAt);

            DrawDomainTestSubSection(_cardGlossaryTest, CardGlossarySceneTestGuide, CardGlossaryPlaymodeTestGuide, CardGlossaryDeviceTestGuide);
        }
    }
}
