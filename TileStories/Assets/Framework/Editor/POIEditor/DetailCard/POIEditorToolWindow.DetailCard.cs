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

        private readonly TestGuideState _cardContainerTest = new TestGuideState();
        private readonly TestGuideState _blockLibraryTest = new TestGuideState();

        private void DrawDetailCardOptions()
        {
            if (_config == null) return;
            _config.card_settings ??= new CardSettings();
            _showCardContainer = DrawFramedFoldout(ref _showCardContainer, DrawCardContainerSection, "Card Container", CardContainerSectionColor);
            _showCardBlockLibrary = DrawFramedFoldout(ref _showCardBlockLibrary, DrawBlockLibrarySection, "Block Library", BlockLibrarySectionColor);
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
            }

            DrawDomainTestSubSection(_cardContainerTest, CardSceneTestGuide, CardPlaymodeTestGuide, CardDeviceTestGuide);
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

            var kinds = BlockRegistry.Shared.All;
            for (int i = 0; i < kinds.Count; i++)
            {
                var kind = kinds[i];
                using (new TableRowScope())
                {
                    GUILayout.Label(kind.DisplayName, GUILayout.Width(BlockLibraryKindColumnWidth));
                    GUILayout.Label(kind.Family, EditorStyles.miniLabel, GUILayout.Width(BlockLibraryFamilyColumnWidth));
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
            }

            DrawDomainTestSubSection(_blockLibraryTest, BlockLibrarySceneTestGuide, BlockLibraryPlaymodeTestGuide, BlockLibraryDeviceTestGuide);
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
    }
}
