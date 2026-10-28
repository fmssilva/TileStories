// POIEditorToolWindow.CardSections.cs
//
// Partial: Detail Card > Sections (_3.3 fourth round, v6's Tweaks "Sections") -- how each section of an OPEN card looks
// (card_settings.sections, SectionStyleRule): its rows mirror v6's -- same names, order and defaults (CardSectionsRows): Start, Divider,
// Colour, Intensity, Motion, Vary; Divider Image's picture under Divider when it is Image. Each row SHOWS what the card draws (an unknown or
// out-of-range value as the rule reads it) and writes only a real pick, so drawing an old config never rewrites it. Under Look Blocks =
// Cards (Card Container) none of it draws: a note says so. A group's own Start is the Bar Groups table's Section Style (Navigation).
// The (i) texts and Test guides live in POIEditorToolWindow.CardSectionsHelp.cs.

using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // The component's rows, in v6's order (the Tweaks "Sections"): what the tests read the window against
        internal static readonly string[] CardSectionsRows = { "Start", "Divider", "Colour", "Intensity", "Motion", "Vary" };
        private readonly TestGuideState _cardSectionsTest = new TestGuideState();

        private void DrawCardSectionsSection()
        {
            var s = _config.card_settings;
            var sections = s.sections ??= new CardSectionSettings();
            if (!SectionStyleRule.IsOpen(s.container))
                EditorGUILayout.HelpBox(CardSectionsCardsNote, MessageType.Info);

            void Popup(string label, string shown, string[] keys, string[] names, string help, System.Action<string> set)
            {
                string picked = DrawPopupField(label, shown, keys, names, help);
                if (picked != shown) set(picked);
            }
            Popup("Start", SectionStyleRule.Start(sections), CardOptions.SectionStarts, CardOptions.SectionStartLabels, CardSectionsStartHelp,
                v => sections.start = v);
            Popup("Divider", SectionStyleRule.Divider(sections), CardOptions.SectionDividers, CardOptions.SectionDividerLabels, CardSectionsDividerHelp,
                v => sections.divider = v);
            if (SectionStyleRule.Divider(sections) == CardOptions.SectionDividerImage)
            {
                string path = DrawTextRow("Divider Image", sections.divider_image ?? "", CardSectionsDividerImageHelp, IndentLevel1);
                if (path != (sections.divider_image ?? "")) sections.divider_image = path;
                if (string.IsNullOrWhiteSpace(sections.divider_image))
                    EditorGUILayout.HelpBox(CardSectionsNoImageNote, MessageType.Warning);
            }
            Popup("Colour", SectionStyleRule.Colour(sections), CardOptions.SectionColours, CardOptions.SectionColourLabels, CardSectionsColourHelp,
                v => sections.colour = v);
            float intensity = SectionStyleRule.Intensity(sections);
            float slid = DrawSliderField("Intensity", intensity, CardSectionSettings.IntensityMin, CardSectionSettings.IntensityMax, CardSectionsIntensityHelp);
            if (!Mathf.Approximately(slid, intensity)) sections.intensity = slid;
            Popup("Motion", SectionStyleRule.Motion(sections), CardOptions.SectionMotions, CardOptions.SectionMotionLabels, CardSectionsMotionHelp,
                v => sections.motion = v);
            Popup("Vary", SectionStyleRule.Vary(sections), CardOptions.SectionVaries, CardOptions.SectionVaryLabels, CardSectionsVaryHelp,
                v => sections.vary = v);

            DrawDomainTestSubSection(_cardSectionsTest, CardSectionsSceneTestGuide, CardSectionsPlaymodeTestGuide, CardSectionsDeviceTestGuide, null);
        }

        // The Bar Groups row's Section Style (an opened group): the card's Start, or one of the Starts for this group alone
        private void DrawBarGroupSectionStyle(CardNavigationSettings nav, CardNavBarItem item, int row)
        {
            var keys = new string[CardOptions.SectionStarts.Length + 1];
            var labels = new string[keys.Length];
            keys[0] = "";
            int card = System.Array.IndexOf(CardOptions.SectionStarts, SectionStyleRule.Start(_config.card_settings.sections));
            labels[0] = "The Card's (" + CardOptions.SectionStartLabels[Mathf.Max(0, card)] + ")";
            for (int i = 0; i < CardOptions.SectionStarts.Length; i++)
            {
                keys[i + 1] = CardOptions.SectionStarts[i];
                labels[i + 1] = CardOptions.SectionStartLabels[i];
            }
            string shown = System.Array.IndexOf(CardOptions.SectionStarts, item.section_style) >= 0 ? item.section_style : "";
            string picked = DrawPopupField("Section Style", shown, keys, labels, CardNavBarGroupSectionStyleHelp, IndentLevel1);
            if (picked != shown) MaterializeBarItems(nav)[row].section_style = picked;
        }
    }
}
