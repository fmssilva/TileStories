// POIEditorToolWindow.CardTexts.cs
//
// Partial: Detail Card > Card Texts (_3.1_POI_Card_Blocks.md section 8.1): the wall's own wording of every card UI text, a row per
// framework / app text (named by its wording, never its key) and a field per wall language.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // The framework's default card texts (CardStrings.asset, next to PoiCard.uss), loaded once per domain reload. By its own path, never
        // "the first CardStringTable found": an app's table is a CardStringTable too (CardStringSources) and must not pass for the framework's.
        private const string FrameworkCardStringsPath = "Assets/Framework/Runtime/UI/Cards/CardStrings.asset";
        private static CardStringTable _frameworkCardStrings;
        internal static CardStringTable FrameworkCardStrings() =>
            _frameworkCardStrings != null ? _frameworkCardStrings : _frameworkCardStrings = AssetDatabase.LoadAssetAtPath<CardStringTable>(FrameworkCardStringsPath);

        // card_settings.strings: one row per framework text, named by its framework wording (never its key), then one
        // field per wall language holding this wall's own wording. Empty field = the framework's. Drawing never writes.
        private void DrawCardTextsSection()
        {
            var s = _config.card_settings;
            var table = FrameworkCardStrings();
            var languages = s.languages;
            if (table == null)
                EditorGUILayout.HelpBox(CardTextsMissingNote, MessageType.Warning);
            else if (languages == null || languages.Count == 0)
                EditorGUILayout.HelpBox(CardNoLanguageNote, MessageType.Warning);
            else
            {
                DrawEditorRow(out float titleRow, out _);
                GUILayout.Label("Text on the card", EditorStyles.miniBoldLabel, GUILayout.Width(Mathf.Max(40f, titleRow - 36f)), GUILayout.ExpandWidth(false));
                HelpInfoButton.Draw("Card Texts", CardTextsHelp);
                EditorRowEnd();

                DrawCardTextRows(s, table, languages, "Framework");
                // - an app's own words, each app under its own name (the same rows, the same override field per language)
                foreach (var source in CardStringSources.Shared.All)
                {
                    DrawEditorRow(out float appRow, out _);
                    GUILayout.Label(source.AppName + " (app texts)", EditorStyles.miniBoldLabel, GUILayout.Width(Mathf.Max(40f, appRow - 36f)), GUILayout.ExpandWidth(false));
                    EditorRowEnd();
                    DrawCardTextRows(s, source.Table, languages, source.AppName);
                }
            }

            DrawDomainTestSubSection(_cardTextsTest, CardTextsSceneTestGuide, CardTextsPlaymodeTestGuide, CardTextsDeviceTestGuide);
        }

        // One row per text of a table (the framework's, or one app's), then one field per wall language holding this wall's own wording
        private void DrawCardTextRows(CardSettings s, CardStringTable table, List<string> languages, string owner)
        {
            var entries = table.Entries();
            foreach (var row in table.rows)
            {
                if (row == null || string.IsNullOrEmpty(row.key)) continue;
                // - quoted: the row IS that text (a one-character wording like "?" reads as a title otherwise)
                string name = "\"" + (CardStrings.Find(entries, row.key, languages[0]) ?? CardStrings.Find(entries, row.key, "en") ?? row.key) + "\"";
                DrawEditorRow(out float nameRow, out _, IndentLevel1);
                GUILayout.Label(name, EditorStyles.boldLabel, GUILayout.Width(Mathf.Max(40f, nameRow - 36f)), GUILayout.ExpandWidth(false));
                HelpInfoButton.Draw(name, CardTextRowHelp(row, owner));
                EditorRowEnd();

                foreach (string lang in languages)
                {
                    string current = CardTextOverride(s, row.key, lang);
                    DrawEditorRow(out float rowWidth, out _, IndentLevel1);
                    EditorGUILayout.PrefixLabel(lang);
                    string edited = EditorGUILayout.TextField(current, GUILayout.Width(Mathf.Max(40f, rowWidth - EditorGUIUtility.labelWidth)), GUILayout.ExpandWidth(false));
                    ReportTableCellRect("Card text " + row.key + " " + lang, 0);
                    EditorRowEnd();
                    if (edited != current) SetCardTextOverride(s, row.key, lang, edited);
                }
            }
        }

        // The row's (i): where the card shows it, then its owner's wording (the framework's, or the app's) in each of its languages
        private static string CardTextRowHelp(CardStringTable.Row row, string owner)
        {
            var words = new List<string>();
            foreach (var t in row.text)
                if (t != null && !string.IsNullOrWhiteSpace(t.value)) words.Add(t.lang + ": " + t.value);
            string who = owner == "Framework" ? "Framework wording" : owner + " wording";
            return row.where + "\n\n" + who + " (used where your field is empty): " + string.Join("; ", words) + ".";
        }

        // This wall's own wording of one text in one language ("" when it has none)
        internal static string CardTextOverride(CardSettings s, string key, string lang)
        {
            var entry = s.strings?.Find(e => e != null && e.key == key);
            return entry?.text?.Find(t => t != null && t.lang == lang)?.value ?? "";
        }

        // Set this wall's wording of one text in one language, creating its row and language entry on the first edit
        internal static void SetCardTextOverride(CardSettings s, string key, string lang, string text)
        {
            s.strings ??= new List<CardStringEntry>();
            var entry = s.strings.Find(e => e != null && e.key == key);
            if (entry == null) s.strings.Add(entry = new CardStringEntry { key = key });
            entry.text ??= new List<LocalizedEntry>();
            var t = entry.text.Find(x => x != null && x.lang == lang);
            if (t == null) entry.text.Add(t = new LocalizedEntry { lang = lang });
            t.value = text;
        }
    }
}
