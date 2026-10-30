// POIEditorToolWindow.CardNavigationPreview.cs
//
// Partial: Detail Card > Navigation's Test > Preview Navigation (the existing demo card, switched on from here) and the one drawer every
// note of the section and of its Navigation Bar goes through. The notes' conditions are NavigationWarningRule; their texts live in
// POIEditorToolWindow.CardNavigationHelp.cs and ...CardNavigationBarHelp.cs.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // Test seam, null in production: every warning or note this section draws reports its text here on a Repaint, so a test can prove
        // one is drawn on its condition and absent otherwise (a HelpBox leaves no other trace for a test to read)
        internal static Action<string> NavigationMessageProbe;

        // The registry the warnings and the preview note read the cards' maps through: the app's. A test hands one with more families
        [NonSerialized] private BlockRegistry _navigationRegistry;

        internal BlockRegistry NavigationRegistry
        {
            get => _navigationRegistry ?? BlockRegistry.Shared;
            set => _navigationRegistry = value;
        }

        // A warning or a note under one of the section's rows: a HelpBox that also tells the test seam it was drawn
        private static void DrawNavigationMessage(string text, MessageType type)
        {
            EditorGUILayout.HelpBox(text, type);
            if (NavigationMessageProbe != null && Event.current.type == EventType.Repaint) NavigationMessageProbe(text);
        }

        // A point's name the way every message of this window names it: its list title, never its id
        private string NavigationCardName(int poiIndex) =>
            poiIndex >= 0 && poiIndex < _config.pois.Count ? EditorNames.Poi(poiIndex, _config.pois[poiIndex]) : "";

        // Test > Preview Navigation: the demo card (card_settings.demo_card) on a chosen point, switched on from this section so a developer can
        // change a row and watch. The SAME fields and the same runtime opener as Card Container > Test > Show demo card -- no second path
        private void DrawNavigationPreviewRows()
        {
            var demo = _config.card_settings.demo_card ??= new CardDemoSettings();
            bool on = DrawToggleField("Preview Navigation", demo.enabled, CardPreviewNavigationHelp, IndentLevel1);
            if (on != demo.enabled) SetNavigationPreview(on);
            if (!demo.enabled) return;

            DemoPointChoices(out var ids, out var titles);
            string poi = DrawReferencePopupField("Preview Point", demo.poi_id, ids, titles, allowNone: true, CardPreviewPointHelp, IndentLevel1 * 2);
            if (poi != demo.poi_id) SetCardDemoPoi(poi);
            if (string.IsNullOrEmpty(demo.poi_id))
            {
                DrawNavigationMessage(CardDemoNoPointNote, MessageType.Info);
                return;
            }
            int at = ids.IndexOf(demo.poi_id);
            if (at >= 0 && !NavigationWarningRule.CardShowsNavigation(_config.card_settings, _config.pois, at, NavigationRegistry))
                DrawNavigationMessage(CardPreviewNoNavigationNote(NavigationCardName(at), _config.card_settings.navigation?.min_blocks ?? 0), MessageType.Info);
        }

        // Turn the demo card on or off from Preview Navigation; turning it on opens it at Full, where the Navigation Bar shows
        internal void SetNavigationPreview(bool on)
        {
            var demo = _config.card_settings.demo_card ??= new CardDemoSettings();
            demo.enabled = on;
            if (on) demo.stop = CardOptions.StopFull;
        }

        // The wall's points as the demo card's popups list them: their ids and, beside them, their list titles
        private void DemoPointChoices(out List<string> ids, out List<string> titles)
        {
            var pois = _config.pois ?? new List<POIData>();
            ids = new List<string>();
            titles = new List<string>();
            for (int i = 0; i < pois.Count; i++)
            {
                ids.Add(pois[i]?.id);
                titles.Add(EditorNames.Poi(i, pois[i]));
            }
        }
    }
}
