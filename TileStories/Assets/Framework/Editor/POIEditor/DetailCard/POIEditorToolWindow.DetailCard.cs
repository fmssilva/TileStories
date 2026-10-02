// POIEditorToolWindow.DetailCard.cs
//
// Partial: the third tab, "Detail Card" (_3.1_POI_Card_Blocks.md section 8.1). One section per card domain, like
// Global Scene holds one per marker domain: Card Container (card_settings: the sheet, languages, media folder)
// and Block Library (card_settings.kinds: every registered block kind, wall-wide). Navigation (_3.2) and Style &
// Contrast (_3.3) join here later. Texts live in DetailCardHelp.cs; each POI's own blocks are Specific Marker >
// Card Content (CardContent.cs).

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private const float BlockLibraryKindColumnWidth = 120f;
        private const float BlockLibraryFamilyColumnWidth = 80f;
        private const float BlockLibraryEnabledColumnWidth = 60f;
        private const float BlockLibraryVariantColumnWidth = 130f;
        // A field-default row under its kind: indented in the Kind column, named "Default <field>"
        private const float BlockLibraryFieldDefaultIndent = 12f;
        private const string BlockLibraryFieldDefaultPrefix = "Default ";
        // GUILayout's gap between the Details button and the (i) of a kind's row (the collapsed margin of two buttons), which a
        // field-default row, with no Details button, reproduces as plain space
        private const float BlockLibraryDetailsToHelpGap = 4f;
        // The margins the Family label and the Enabled toggle add to a kind's row, which a field-default row (plain space where those
        // controls stand) reproduces so its popup starts under the Default Variant popups (measured on a 620 pt capture: 6 pt short)
        private const float BlockLibrarySkippedControlMargins = 6f;

        private static GUIStyle _blockLibraryFamilyStyle;
        // The family word: mini type, vertically centred so it shares the kind name's line
        private static GUIStyle BlockLibraryFamilyStyle =>
            _blockLibraryFamilyStyle ??= new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleLeft, fixedHeight = 0f };

        private readonly TestGuideState _cardContainerTest = new TestGuideState();
        private readonly TestGuideState _blockLibraryTest = new TestGuideState();
        private readonly TestGuideState _cardTextsTest = new TestGuideState();
        private readonly TestGuideState _cardGlossaryTest = new TestGuideState();
        private readonly TestGuideState _cardDefaultMediaTest = new TestGuideState();

        private void DrawDetailCardOptions()
        {
            if (_config == null) return;
            _config.card_settings ??= new CardSettings();
            _showCardContainer = DrawFramedFoldout(ref _showCardContainer, DrawCardContainerSection, "Card Container", CardContainerSectionColor);
            _showCardBlockLibrary = DrawFramedFoldout(ref _showCardBlockLibrary, DrawBlockLibrarySection, "Block Library", BlockLibrarySectionColor);
            _showCardTexts = DrawFramedFoldout(ref _showCardTexts, DrawCardTextsSection, "Card Texts", CardTextsSectionColor);
            _showCardGlossary = DrawFramedFoldout(ref _showCardGlossary, DrawCardGlossarySection, "Glossary", CardGlossarySectionColor);
            _showCardDefaultMedia = DrawFramedFoldout(ref _showCardDefaultMedia, DrawCardDefaultMediaSection, "Default Media", CardDefaultMediaSectionColor);
        }

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

        // card_settings: whether the card opens, where, how it closes, which languages and where its media lives
        private void DrawCardContainerSection()
        {
            var s = _config.card_settings;
            s.enabled = DrawToggleField("Enable Detail Card", s.enabled, CardEnabledHelp);

            // off hides the settings (their values are kept); the Test stays, like every domain's
            if (s.enabled)
            {
                var c = s.container ??= new CardContainerSettings();
                DrawCardLanguagesRow(s);
                s.media_resources_path = DrawTextRow("Media Folder", s.media_resources_path, CardMediaFolderHelp, IndentLevel0);
                c.open_stop = DrawPopupField("Open At", c.open_stop, CardOptions.OpenStops, CardOpenStopLabels, CardOpenAtHelp);
                c.half_max_ratio = DrawSliderField("Half Height Max", c.half_max_ratio, CardContainerSettings.HalfMaxRatioMin,
                    CardContainerSettings.HalfMaxRatioMax, CardHalfHeightHelp);
                c.dismiss_on_tap_outside = DrawToggleField("Tap Outside Closes", c.dismiss_on_tap_outside, CardTapOutsideHelp);
                c.keep_audio_on_close = DrawToggleField("Keep Audio Playing", c.keep_audio_on_close, CardKeepAudioHelp);
                c.audio_when_another_starts = DrawPopupField("Audio Overlap", c.audio_when_another_starts, CardOptions.AudioModes, CardOptions.AudioModeLabels, CardAudioSwitchHelp);
                c.audio_android_output_poll = DrawToggleField("Android Earbud Check", c.audio_android_output_poll, CardAudioAndroidPollHelp);
                c.reduce_motion = DrawToggleField("Reduce Motion", c.reduce_motion, CardReduceMotionHelp);
                c.sources_at_end = DrawToggleField("Sources At The End", c.sources_at_end, CardSourcesAtEndHelp);
            }

            DrawDomainTestSubSection(_cardContainerTest, CardSceneTestGuide, CardPlaymodeTestGuide, CardDeviceTestGuide, DrawCardTestRows);
        }

        // The Test foldout's own rows: forget the saved answers, then the developer-only demo card and the gallery
        private void DrawCardTestRows()
        {
            DrawCardStateResetRow();
            DrawCardPreviewLanguageRow();
            DrawCardDemoRows();
        }

        // Developer-only: Show demo card (+ which point, which stop) opens one card by itself in Play Mode, and Open Gallery loads the card's
        // isolated test scene. Off by default; ignored by release builds (CardDemoRule); registered in DevFeatureBuildGuard.
        private void DrawCardDemoRows()
        {
            var demo = _config.card_settings.demo_card ??= new CardDemoSettings();
            demo.enabled = DrawToggleField("Show demo card", demo.enabled, CardDemoShowHelp, IndentLevel1);
            if (demo.enabled)
            {
                var pois = _config.pois ?? new List<POIData>();
                var ids = new List<string>();
                var titles = new List<string>();
                for (int i = 0; i < pois.Count; i++)
                {
                    ids.Add(pois[i]?.id);
                    titles.Add(EditorNames.Poi(i, pois[i]));
                }
                string poi = DrawReferencePopupField("Demo Point", demo.poi_id, ids, titles, allowNone: true, CardDemoPoiHelp, IndentLevel1 * 2);
                if (poi != demo.poi_id) SetCardDemoPoi(poi);
                string stop = DrawPopupField("Demo Stop", demo.stop, CardOptions.DemoStops, CardDemoStopLabels, CardDemoStopHelp, IndentLevel1 * 2);
                if (stop != demo.stop) SetCardDemoStop(stop);
                if (string.IsNullOrEmpty(demo.poi_id))
                    EditorGUILayout.HelpBox(CardDemoNoPointNote, MessageType.Info);
            }

            DrawEditorRow(out float rowWidth, out _, IndentLevel1);
            if (GUILayout.Button("Open Gallery", GUILayout.Width(Mathf.Max(40f, rowWidth - 36f)), GUILayout.ExpandWidth(false)))
                CardGalleryOpener.Open();
            ReportTableCellRect("Card gallery open", 0);
            HelpInfoButton.Draw("Open Gallery", CardGalleryHelp);
            EditorRowEnd();
        }

        // Developer-only: the language the card opens in while testing. Shown only with two or more languages; the default (the first one)
        // is stored as "" so an untouched wall carries nothing
        private void DrawCardPreviewLanguageRow()
        {
            var s = _config.card_settings;
            var choices = CardLanguageRule.Choices(s.languages);
            if (choices.Count < 2) return;
            string shown = CardLanguageRule.Shown(s.languages, "", s.preview_language, previewAllowed: true);
            string picked = DrawPopupField("Preview Language", shown, choices.ToArray(), choices.ToArray(), CardPreviewLanguageHelp, IndentLevel1);
            if (picked != shown) SetCardPreviewLanguage(picked);
        }

        // Pick the preview language (the first language = the default, stored as ""), and forget the language earlier taps on the chip saved
        // on this computer, so the card shows the pick at once (the visitor's own pick would win over it)
        internal void SetCardPreviewLanguage(string language)
        {
            var s = _config.card_settings;
            s.preview_language = language == CardLanguageRule.Fallback(s.languages) ? "" : language ?? "";
            new CardLocalState(new PlayerPrefsCardStateStore(), _config.wall_id).SetLanguage("");
        }

        internal void SetCardDemoPoi(string poiId) => _config.card_settings.demo_card.poi_id = poiId ?? "";

        internal void SetCardDemoStop(string stop) => _config.card_settings.demo_card.stop = stop;

        // The Test row that forgets what the card remembered on this computer (answers, votes, revealed questions)
        private void DrawCardStateResetRow()
        {
            DrawEditorRow(out float rowWidth, out _, IndentLevel1);
            if (GUILayout.Button("Reset Saved Card State", GUILayout.Width(Mathf.Max(40f, rowWidth - 36f)), GUILayout.ExpandWidth(false)))
                ResetSavedCardState();
            ReportTableCellRect("Card state reset", 0);
            HelpInfoButton.Draw("Reset Saved Card State", CardStateResetHelp);
            EditorRowEnd();
        }

        // Forget every answer, vote and revealed question this wall's cards stored on this computer (CardLocalState.ResetAll over
        // the same PlayerPrefs the app uses) and say how many entries went. Never touches the config.
        internal int ResetSavedCardState()
        {
            int removed = new CardLocalState(new PlayerPrefsCardStateStore(), _config?.wall_id).ResetAll();
            EditorNotice.Queue("Saved card state cleared", removed == 0
                ? "Nothing was saved for this wall on this computer."
                : removed + (removed == 1 ? " saved entry" : " saved entries") + " (answers, votes, revealed questions) cleared for this wall on this computer.");
            return removed;
        }

        // Languages: one comma-separated list (the first is the fallback); written only when the text changes
        private void DrawCardLanguagesRow(CardSettings s)
        {
            var current = s.languages ?? new List<string>();
            DrawEditorRow(out float rowWidth, out _);
            EditorGUILayout.PrefixLabel("Languages");
            var edited = DrawKeywordListField(current, GUILayout.Width(Mathf.Max(40f, rowWidth - EditorGUIUtility.labelWidth - 36f)), GUILayout.ExpandWidth(false));
            ReportTableCellRect("Card languages", 0);
            HelpInfoButton.Draw("Languages", CardLanguagesHelp);
            EditorRowEnd();
            if (!ReferenceEquals(edited, current)) s.languages = edited;
            if (s.languages == null || s.languages.Count == 0)
                EditorGUILayout.HelpBox(CardNoLanguageNote, MessageType.Warning);
        }

        // card_settings.kinds: one row per registered kind (framework and app kinds alike). Drawing never writes:
        // a kind without a row shows the defaults, and its row is created by the first real edit.
        private void DrawBlockLibrarySection()
        {
            var s = _config.card_settings;
            using (new TableRowScope())
            {
                GUILayout.Label("Kind", EditorStyles.miniBoldLabel, GUILayout.Width(BlockLibraryKindColumnWidth));
                GUILayout.Label("Family", EditorStyles.miniBoldLabel, GUILayout.Width(BlockLibraryFamilyColumnWidth));
                GUILayout.Space(TableGapBetweenGroups);
                GUILayout.Label("Enabled", EditorStyles.miniBoldLabel, GUILayout.Width(BlockLibraryEnabledColumnWidth));
                GUILayout.Label("Default Variant", EditorStyles.miniBoldLabel, GUILayout.Width(BlockLibraryVariantColumnWidth));
                HelpInfoButton.Draw("Block Library", BlockLibraryHelp, 26f);
                GUILayout.FlexibleSpace();
                GUILayout.Space(AddButtonRowRightMargin);
            }

            // - by family, so an app's kind sits with its family (Ordered), not after every built-in one
            var kinds = BlockRegistry.Shared.Ordered;
            for (int i = 0; i < kinds.Count; i++)
            {
                var kind = kinds[i];
                using (new TableRowScope())
                {
                    GUILayout.Label(kind.DisplayName, GUILayout.Width(BlockLibraryKindColumnWidth));
                    // - the mini style is shorter than the row and top-aligned: centred at the row's height it sits on the kind's line
                    GUILayout.Label(kind.Family, BlockLibraryFamilyStyle, GUILayout.Width(BlockLibraryFamilyColumnWidth), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    GUILayout.Space(TableGapBetweenGroups);

                    bool enabled = BlockLibraryRule.IsEnabled(s, kind.Key);
                    // - the header is the card's identity: always on, so its switch is shown but locked
                    using (new EditorGUI.DisabledScope(kind.Key == BuiltInBlocks.HeaderKind))
                    {
                        bool toggled = EditorGUILayout.Toggle(enabled, GUILayout.Width(BlockLibraryEnabledColumnWidth));
                        ReportTableCellRect("Block Library enabled", i);
                        if (toggled != enabled) SetBlockLibraryEnabled(kind.Key, toggled);
                    }

                    string variant = BlockLibraryRule.DefaultVariant(s, kind);
                    var variants = new List<string>(kind.Variants);
                    int picked = EditorGUILayout.Popup(variants.IndexOf(variant), variants.ToArray(), GUILayout.Width(BlockLibraryVariantColumnWidth));
                    if (picked >= 0 && variants[picked] != variant) SetBlockLibraryVariant(kind.Key, variants[picked]);

                    if (GUILayout.Button(DetailsIcon, GUILayout.Width(26f), GUILayout.Height(20f)))
                        EditorPopup.ShowAt(CreateDetailsPopup(kind.DisplayName, () => BlockLibraryRule.Row(_config.card_settings, kind.Key)?.details ?? "",
                            v => EnsureBlockLibraryRow(kind.Key).details = v), GUILayoutUtility.GetLastRect());
                    HelpInfoButton.Draw(kind.DisplayName, kind.Help);
                    GUILayout.FlexibleSpace();
                    GUILayout.Space(AddButtonRowRightMargin);
                }
                DrawBlockLibraryFieldDefaults(kind, i);
            }

            DrawDomainTestSubSection(_blockLibraryTest, BlockLibrarySceneTestGuide, BlockLibraryPlaymodeTestGuide, BlockLibraryDeviceTestGuide);
        }

        // Under a kind's row, one row per field whose wall-wide default the Library sets (LibraryDefault, e.g. a model's Fit): its
        // label across the Kind and Family columns, its options in the Default Variant column, its (i) in the (i) column. Drawing never writes: the
        // row and its entry are made by the first real pick (SetBlockLibraryFieldDefault).
        private void DrawBlockLibraryFieldDefaults(BlockKindDefinition kind, int kindIndex)
        {
            foreach (var field in kind.Fields)
            {
                if (!field.LibraryDefault) continue;
                // - a default no block of the kind can use (the header's Fit while no header is drawn as a model) is not drawn; it is kept
                if (!FieldVisibilityRule.IsShownInLibrary(field, kind, _config.card_settings, _config.pois)) continue;
                using (new TableRowScope())
                {
                    GUILayout.Space(BlockLibraryFieldDefaultIndent);
                    // - the label spans the Kind AND Family columns (a default row has no family of its own): "Default Scale Mode" fits
                    GUILayout.Label(BlockLibraryFieldDefaultPrefix + field.Label, GUILayout.Width(BlockLibraryKindColumnWidth + BlockLibraryFamilyColumnWidth - BlockLibraryFieldDefaultIndent));
                    GUILayout.Space(TableGapBetweenGroups + BlockLibraryEnabledColumnWidth + BlockLibrarySkippedControlMargins);
                    if (field.Type == BlockFieldType.Toggle)
                    {
                        // - a Toggle's default is a tick in the Default Variant column's own width
                        bool ticked = BlockLibraryRule.FlagDefault(_config.card_settings, kind, field);
                        bool edited = EditorGUILayout.Toggle(ticked, GUILayout.Width(EditorGUIUtility.singleLineHeight));
                        ReportTableCellRect("Block Library default " + kind.Key + "." + field.Key, kindIndex);
                        if (edited != ticked) SetBlockLibraryFieldDefault(kind.Key, field.Key, edited ? BlockLibraryRule.FlagTrue : BlockLibraryRule.FlagFalse);
                        GUILayout.Space(BlockLibraryVariantColumnWidth - EditorGUIUtility.singleLineHeight);
                    }
                    else
                    {
                        string current = BlockLibraryRule.ChoiceDefault(_config.card_settings, kind, field);
                        var options = new List<string>(field.Options);
                        var labels = new List<string>();
                        for (int o = 0; o < options.Count; o++) labels.Add(field.OptionLabels != null ? field.OptionLabels[o] : options[o]);
                        int picked = EditorGUILayout.Popup(options.IndexOf(current), labels.ToArray(), GUILayout.Width(BlockLibraryVariantColumnWidth));
                        ReportTableCellRect("Block Library default " + kind.Key + "." + field.Key, kindIndex);
                        if (picked >= 0 && options[picked] != current) SetBlockLibraryFieldDefault(kind.Key, field.Key, options[picked]);
                    }
                    GUILayout.Space(26f + BlockLibraryDetailsToHelpGap);
                    HelpInfoButton.Draw(field.Label, field.Help);
                    GUILayout.FlexibleSpace();
                    GUILayout.Space(AddButtonRowRightMargin);
                }
            }
        }

        // Set a kind's Library default of one field (creates the kind's row and the entry on the first pick)
        internal void SetBlockLibraryFieldDefault(string kind, string fieldKey, string value)
        {
            var row = EnsureBlockLibraryRow(kind);
            row.field_defaults ??= new List<BlockFieldDefault>();
            var entry = row.field_defaults.Find(e => e != null && e.key == fieldKey);
            if (entry == null) row.field_defaults.Add(new BlockFieldDefault { key = fieldKey, value = value });
            else entry.value = value;
        }

        // The Block Library row of a kind, created on the first edit (keeps its kind's defaults until changed)
        private BlockKindSetting EnsureBlockLibraryRow(string kind)
        {
            var s = _config.card_settings;
            var row = BlockLibraryRule.Row(s, kind);
            if (row != null) return row;
            s.kinds ??= new List<BlockKindSetting>();
            s.kinds.Add(row = new BlockKindSetting { kind = kind });
            return row;
        }

        internal void SetBlockLibraryEnabled(string kind, bool enabled) => EnsureBlockLibraryRow(kind).enabled = enabled;

        internal void SetBlockLibraryVariant(string kind, string variant) => EnsureBlockLibraryRow(kind).default_variant = variant;

        // _3.1 step 13: this wall's own default media library path (like Media Folder), then a read-only table of
        // the Framework's own shipped defaults (preview, kind, key, (i)) -- every Asset field's "Pick default..."
        // offers this wall's own library first, then these
        private const float DefaultMediaPreviewSize = 32f;
        private const float DefaultMediaKindColumnWidth = 70f;
        private const float DefaultMediaKeyColumnWidth = 150f;
        // The gap between two cells of a Default Media row (GUILayout's own gap between two labels)
        private const float DefaultMediaCellGap = 4f;

        // A Default Media row's cells: one rect the size and style of the title row's label (the row less the 36 pt (i) column), reserved
        // in the shared row so the (i) drawn after it sits where the title's does
        private static Rect DefaultMediaCellsRect(float rowWidth, float height)
        {
            float width = Mathf.Max(40f, rowWidth - 36f);
            return GUILayoutUtility.GetRect(width, height, EditorStyles.miniBoldLabel, GUILayout.Width(width), GUILayout.Height(height), GUILayout.ExpandWidth(false));
        }

        private static Rect DefaultMediaKindRect(Rect cells) =>
            new Rect(cells.x + DefaultMediaPreviewSize + DefaultMediaCellGap, cells.y, DefaultMediaKindColumnWidth, EditorGUIUtility.singleLineHeight);

        // - never past the cells' own end, so a narrow window cuts the key instead of pushing it under the (i)
        private static Rect DefaultMediaKeyRect(Rect cells)
        {
            float x = cells.x + DefaultMediaPreviewSize + DefaultMediaKindColumnWidth + 2f * DefaultMediaCellGap;
            return new Rect(x, cells.y, Mathf.Max(0f, Mathf.Min(DefaultMediaKeyColumnWidth, cells.xMax - x)), EditorGUIUtility.singleLineHeight);
        }

        private void DrawCardDefaultMediaSection()
        {
            var s = _config.card_settings;
            s.default_media_library_resources_path = DrawTextRow("Default Media Library", s.default_media_library_resources_path,
                CardDefaultMediaLibraryHelp, IndentLevel0);

            DrawEditorRow(out float titleRow, out _);
            GUILayout.Label("Framework defaults", EditorStyles.miniBoldLabel, GUILayout.Width(Mathf.Max(40f, titleRow - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw("Framework defaults", CardDefaultMediaTableHelp);
            ReportTableCellRect("Default media title help", 0);
            EditorRowEnd();

            var library = CardMediaLibraryLookup.Framework;
            if (library == null || library.Entries.Count == 0)
            {
                EditorGUILayout.HelpBox("No default media found (CardMediaLibrary.asset is missing or empty).", MessageType.Warning);
            }
            else
            {
                // - every row is the shared row (DrawEditorRow) built like the title above: its cells in ONE rect of the title label's
                //   width and style, split here, then the (i). So each (i) lands exactly in the section's (i) column, whatever margins the
                //   cells' own styles have (a FlexibleSpace in an uncapped row once ran it to the window's edge)
                DrawEditorRow(out float headerRow, out _);
                Rect header = DefaultMediaCellsRect(headerRow, EditorGUIUtility.singleLineHeight);
                GUI.Label(DefaultMediaKindRect(header), "Kind", EditorStyles.miniBoldLabel);
                GUI.Label(DefaultMediaKeyRect(header), "Key", EditorStyles.miniBoldLabel);
                EditorRowEnd();
                for (int i = 0; i < library.Entries.Count; i++)
                {
                    var entry = library.Entries[i];
                    DrawEditorRow(out float rowWidth, out _);
                    Rect cells = DefaultMediaCellsRect(rowWidth, DefaultMediaPreviewSize);
                    var previewRect = new Rect(cells.x, cells.y, DefaultMediaPreviewSize, DefaultMediaPreviewSize);
                    Texture2D preview = entry.asset != null ? AssetPreview.GetAssetPreview(entry.asset) ?? AssetPreview.GetMiniThumbnail(entry.asset) : null;
                    GUI.Box(previewRect, preview != null ? (Texture)preview : Texture2D.grayTexture);
                    ReportTableCellRect("Default media preview", i, previewRect);
                    GUI.Label(DefaultMediaKindRect(cells), entry.kind.ToString());
                    GUI.Label(DefaultMediaKeyRect(cells), entry.key);
                    HelpInfoButton.Draw(entry.key, CardDefaultMediaRowHelp(entry));
                    ReportTableCellRect("Default media help", i);
                    EditorRowEnd();
                }
            }

            DrawDomainTestSubSection(_cardDefaultMediaTest, CardDefaultMediaSceneTestGuide, CardDefaultMediaPlaymodeTestGuide, CardDefaultMediaDeviceTestGuide);
        }
    }
}
