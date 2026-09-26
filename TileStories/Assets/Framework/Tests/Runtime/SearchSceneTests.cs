using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Select, Filter & Search in the REAL wall scene (spec _2.6): the search UI a visitor sees, composed by the
    // scene's SearchUIHost over the wall's real markers and shipped config. Every test acts like a visitor
    // (typing, pressing a filter chip, tapping a marker through the EventSystem) or like the Editor's live push
    // (WallSession.ApplySearchSettings) and checks that the results list, the minimap dots and the markers all
    // show the SAME set -- the composition that was never wired before this review (the views existed, green,
    // but no scene contained them).
    public class SearchSceneTests : SearchSceneFixture
    {
        private static bool Shown(VisualElement e) => e.style.display != DisplayStyle.None;

        private static readonly string[] Military = { "lamp_military", "painting_military", "camera_military", "dev_disp_eq_mil", "dev_marker_custom_symbol" };

        [UnityTest]
        public IEnumerator TheWallScene_ComposesTheSearchUi_OverItsRunningMarkers()
        {
            Assert.AreEqual(Session.SpawnedMarkers.Count, Session.SearchPois.Count, "every running marker is searchable");
            Assert.AreEqual(Session.SearchPois.Count, Session.SearchIndex.PoiCount, "the index holds exactly those POIs");
            Assert.AreEqual(Session.SpawnedMarkers.Count, Host.Minimap.Positions.Count, "one map dot per running marker");
            Assert.IsNotNull(Session.SelectionHighlight, "the selection highlight runs");
            Assert.IsNotNull(Session.ZoomOnSelect, "zoom-on-select runs (the scene has a zoom rig)");
            Assert.Greater(Host.SearchBar.SuggestionTerms.Count, 0, "suggestions under the empty field");

            // idle: the camera view stays clear
            Assert.IsFalse(Host.State.Active);
            Assert.IsFalse(Host.List.IsShown || Host.ViewModes.IsShown || Host.Minimap.IsShown || Shown(Host.Card.Root));
            Assert.AreEqual(Session.SpawnedMarkers.Count, MarkersAt(1f).Count, "no marker dimmed");
            yield return Capture("Idle");
        }

        [UnityTest]
        public IEnumerator ATypedQuery_ListMapAndMarkersShowTheSameSet()
        {
            Host.SetQuery("military");
            yield return Wait(0.5f);
            var ids = new HashSet<string>(Host.State.Ids);
            CollectionAssert.AreEquivalent(Military, ids, "the category word finds every military POI");
            Assert.AreEqual("Lamp - Military", Host.List.Rows[0].Name, "a name match ranks above a category match");
            CollectionAssert.AreEquivalent(ids, ListIds(), "the list shows the set");
            CollectionAssert.AreEquivalent(ids, DotIds(), "the map shows the set");
            CollectionAssert.AreEquivalent(ids, MarkersAt(1f), "only the set's markers stay");
            Assert.AreEqual(Session.SpawnedMarkers.Count - ids.Count, MarkersAt(0f).Count, "Filtered-Out Markers = Hide: the rest are gone");
            Assert.IsTrue(Host.List.IsShown && Host.ViewModes.IsShown);
            yield return Capture("Query_List");

            Host.SetQuery("");
            yield return null;
            Assert.IsFalse(Host.State.Active, "clearing the field clears the search (it once kept the old results)");
            Assert.AreEqual(Session.SpawnedMarkers.Count, MarkersAt(1f).Count, "every marker back");
        }

        [UnityTest]
        public IEnumerator FilterChips_NarrowTheSet_AndTheRelaxButtonBringsResultsBack()
        {
            Press(Host.Root.Q<Button>("search-filters"));
            yield return null;
            Assert.IsTrue(Host.Tray.IsOpen, "Filters opens the tray");
            yield return Capture("FilterTray");

            Press(Host.Tray.Root.Q<Toggle>("facet-Category-military"));   // a real chip tap on the live panel
            yield return null;
            CollectionAssert.AreEquivalent(Military, Host.State.Ids, "a category chip alone");
            CollectionAssert.AreEquivalent(Military, MarkersAt(1f));
            StringAssert.Contains("(1)", Host.Root.Q<Button>("search-filters").text, "the Filters button counts active filters");

            Press(Host.Tray.Root.Q<Toggle>("facet-Badge-intact"));        // military AND intact: none of them
            yield return null;
            CollectionAssert.IsEmpty(Host.State.Ids, "an empty intersection shows nothing");
            Assert.IsTrue(Host.List.EmptyShown);
            // - filters alone, no text: the filters message, never the query one with an empty ""
            Assert.AreEqual(ConfigCopy().select_filter_search.search.no_results_filters_message, Host.List.EmptyMessage);
            StringAssert.StartsWith("Remove filter: military", Host.List.RelaxText, "dropping the category gives the most results");
            yield return Capture("NoResults_Relax");

            Press(Host.List.Root.Q<Button>("results-relax"));
            yield return null;
            Assert.IsFalse(Host.Tray.Selection.IsActive(FacetGroup.Category, "military"), "the relax button removed that filter");
            Assert.IsFalse(Host.Tray.Root.Q<Toggle>("facet-Category-military").value, "its chip shows it");
            Assert.Greater(Host.State.Ids.Count, 0, "results are back");
            Assert.IsTrue(Host.State.Results.All(r => r.Poi.badge_category == "intact"), "only the Badge filter remains");

            Press(Host.Tray.Root.Q<Button>("filter-clear-all"));
            yield return null;
            Assert.IsFalse(Host.State.Active, "Clear filters: back to the whole wall");
        }

        [UnityTest]
        public IEnumerator TheViewSwitch_MovesTheResults_ButTheMarkersKeepShowingThem()
        {
            Host.SetQuery("religious");
            yield return null;
            var ids = new HashSet<string>(Host.State.Ids);
            Assert.Greater(ids.Count, 0);

            Press(Host.ViewModes.Root.Q<Button>("view-mode-Minimap"));
            yield return null;
            Assert.IsTrue(Host.Minimap.IsShown && !Host.List.IsShown, "Map view: the results on the minimap");
            CollectionAssert.AreEquivalent(ids, DotIds());
            yield return Capture("View_Map");

            Press(Host.ViewModes.Root.Q<Button>("view-mode-CameraHighlight"));
            yield return null;
            Assert.IsFalse(Host.Minimap.IsShown || Host.List.IsShown, "Highlight view: no panel");
            CollectionAssert.AreEquivalent(ids, MarkersAt(1f), "the markers alone show the results");
            yield return Capture("View_Highlight");
        }

        [UnityTest]
        public IEnumerator ARealMarkerTap_Selects_DimsTheOthers_OpensTheCard_AndASecondTapClears()
        {
            var target = Session.SpawnedMarkers.Where(m => m != null && m.IsVisible)
                .Select(m => (m, p: ScreenPointOf(m)))
                .FirstOrDefault(x => x.p.HasValue && x.p.Value.y < Screen.height * 0.72f && x.p.Value.y > Screen.height * 0.4f);
            Assert.IsNotNull(target.m, "precondition: a marker is on screen, clear of the search bar and of where the card opens");

            var hit = TapScreen(target.p.Value);
            // - what the tap reached, said in the failure message (this test once failed twice, then never again)
            string reached = $"aimed at '{target.m.PoiId}', hit '{(hit != null ? hit.transform.parent.name + "/" + hit.name : "nothing")}', " +
                $"selected right after the tap: '{SelectionEventBus.CurrentPoiId}'";
            yield return Wait(0.6f);
            Assert.IsNotNull(hit, "the tap hit something");
            Assert.IsNotNull(SelectionEventBus.CurrentPoiId, "the tap selected a POI -- " + reached);
            string selected = SelectionEventBus.CurrentPoiId;
            Assert.AreEqual(1f, Marker(selected).SelectionAlpha, 1e-3, "the selected marker stays full");
            Assert.AreEqual(Session.SpawnedMarkers.Count - 1, MarkersAt(0.3f).Count, "Highlight Selection: every other marker at Dim Others To");
            Assert.IsTrue(Shown(Host.Card.Root), "the card opens");
            Assert.AreEqual(Session.SearchPois.First(p => p.id == selected).name, Host.Card.NameText);
            yield return Capture("Selected_Card");

            TapScreen(ScreenPointOf(Marker(selected)).Value);
            yield return Wait(0.6f);
            Assert.IsNull(SelectionEventBus.CurrentPoiId, "tapping it again clears the selection");
            Assert.AreEqual(Session.SpawnedMarkers.Count, MarkersAt(1f).Count, "every marker back to full");
            Assert.IsFalse(Shown(Host.Card.Root), "the card closes");
        }

        [UnityTest]
        public IEnumerator SelectingAndFiltering_NeverUndoEachOther()
        {
            Host.SetQuery("military");
            yield return null;
            var hidden = MarkersAt(0f);
            Assert.Greater(hidden.Count, 0);

            SelectionEventBus.Select("lamp_military");
            yield return null;
            CollectionAssert.AreEquivalent(hidden, MarkersAt(0f),
                "a selection must not bring filtered-out markers back (it used to set them to the dim alpha)");
            Assert.AreEqual(1f, Marker("lamp_military").SelectionAlpha, 1e-3);
            CollectionAssert.AreEquivalent(Military.Where(id => id != "lamp_military"), MarkersAt(0.3f), "the other results dim");

            Press(Host.Card.Root.Q<Button>("detail-card-close"));
            yield return null;
            Assert.IsNull(SelectionEventBus.CurrentPoiId, "the card's X clears the selection");
            CollectionAssert.AreEquivalent(hidden, MarkersAt(0f), "clearing the selection keeps the filter (it used to reset every marker)");
            CollectionAssert.AreEquivalent(Military, MarkersAt(1f));
        }

        [UnityTest]
        public IEnumerator EverySetting_PushedLive_ReachesTheRunningUi()
        {
            Host.SetQuery("military");
            yield return ApplySearch(s => { s.filter.mismatch = "dim"; s.filter.dim_alpha = 0.2f; });
            Host.Refresh();
            Assert.AreEqual(Session.SpawnedMarkers.Count - 5, MarkersAt(0.2f).Count, "Filtered-Out Markers = Dim at Dim Filtered To");

            yield return ApplySearch(s => s.search.typo_tolerance = 0);
            Host.SetQuery("militray");
            Assert.IsEmpty(Host.State.Ids, "Typo Tolerance 0: a typo finds nothing");
            yield return ApplySearch(s => s.search.typo_tolerance = 1);
            Host.SetQuery("militray");
            Assert.IsNotEmpty(Host.State.Ids, "Typo Tolerance 1: the swapped letters are forgiven");

            yield return ApplySearch(s => s.search.prefix_matching = "off");
            Host.SetQuery("milit");
            Assert.IsEmpty(Host.State.Ids, "Partial Words Off");
            yield return ApplySearch(s => s.search.prefix_matching = "all_fields");
            Host.SetQuery("milit");
            Assert.IsNotEmpty(Host.State.Ids, "Partial Words All fields");

            yield return ApplySearch(s => s.search.match_mode = "all");
            Host.SetQuery("lamp painting");
            Assert.IsEmpty(Host.State.Ids, "Every word: nothing is both");
            yield return ApplySearch(s => s.search.match_mode = "any");
            Host.SetQuery("lamp painting");
            Assert.Greater(Host.State.Ids.Count, 10, "Any word: the lamp and the painting sets");

            yield return ApplySearch(s => s.filter.status_facet = false);
            Assert.IsNull(Host.Tray.Root.Q("facet-group-Status"), "Status Filter off: its group leaves the tray");

            Host.SetQuery("");
            yield return ApplySearch(s => s.minimap.visibility = "always");
            Assert.IsTrue(Host.Minimap.IsShown, "Visibility Always: the map shows with no search");
            Assert.IsFalse(Host.SearchBar.MapButtonShown, "no Map button");
            yield return ApplySearch(s => s.minimap.visibility = "toggle");
            Assert.IsFalse(Host.Minimap.IsShown);
            Press(Host.Root.Q<Button>("search-map"));
            Assert.IsTrue(Host.Minimap.IsShown, "the Map button opens it");
            yield return ApplySearch(s => s.minimap.enabled = false);
            Assert.IsFalse(Host.Minimap.IsShown || Host.SearchBar.MapButtonShown, "Enable Minimap off");
            Assert.IsFalse(Host.ViewModes.MapOffered, "no Map view without a minimap");

            yield return ApplySearch(s => s.selection.highlight_enabled = false);
            SelectionEventBus.Select("lamp");
            Assert.AreEqual(Session.SpawnedMarkers.Count, MarkersAt(1f).Count, "Highlight Selection off: no dim");
            SelectionEventBus.Clear();

            yield return ApplySearch(s => s.enabled = false);
            Assert.IsFalse(Shown(Host.Root), "Enable Select & Search off: no search UI");
            Assert.IsNull(Session.SelectionHighlight, "and no selection responders");
            yield return ApplySearch(s => s.enabled = true);
            Assert.IsTrue(Shown(Host.Root));
            Assert.IsNotNull(Session.SelectionHighlight, "switched back on live");
        }

        [UnityTest]
        public IEnumerator TheMinimap_DrawsTheRoomFromAbove_AndATapOnADotSelects()
        {
            var layout = MinimapLayout.Compute(Session.SpawnedMarkers.Select(m => m.UndisplacedWorldPosition).ToList(),
                ConfigCopy().select_filter_search.minimap);
            Assert.IsTrue(layout.Floor, "Projection Auto: the wall's POIs spread over a room, so the floor plan");
            Press(Host.Root.Q<Button>("search-map"));
            yield return null;
            Host.Minimap.TapAt(Host.Minimap.Positions["camera"]);
            Assert.AreEqual("camera", SelectionEventBus.CurrentPoiId, "a dot tap selects its POI like a marker tap");
            yield return Capture("Minimap_Selected");
        }

        [UnityTest]
        public IEnumerator VoiceSearch_InTheEditor_SearchesTheTestPhrase()
        {
            Assert.IsFalse(Host.SearchBar.MicShown, "voice off: no mic");
            yield return ApplySearch(s => { s.voice.enabled = true; s.voice.indicator_style = "mic_text"; });
            Assert.IsTrue(Host.SearchBar.MicShown, "Enable Voice Search: the mic (the Editor has a test backend)");
            Press(Host.Root.Q<Button>("search-mic"));
            yield return null;
            Assert.AreEqual(new DebugTranscriber().PresetTranscript, Host.Query, "the heard phrase is searched like typed text");
            Assert.AreEqual(VoiceSearchState.Result, Host.Voice.State);
        }

        [UnityTest]
        public IEnumerator ARememberedSearch_BecomesASuggestion()
        {
            yield return ApplySearch(s => { s.results.suggestion_source = "recent_first"; s.results.recent_count = 3; });
            Host.SetQuery("painting");
            SelectionEventBus.Select("painting");       // picking a result remembers the query
            SelectionEventBus.Clear();
            yield return ApplySearch(s => { });          // any rebuild re-reads the suggestions
            Assert.AreEqual("painting", Host.SearchBar.SuggestionTerms[0], "Recent first: the device's last search first");
            yield return ApplySearch(s => s.results.suggestions_enabled = false);
            CollectionAssert.IsEmpty(Host.SearchBar.SuggestionTerms, "Suggestions off");
        }

        [UnityTest]
        public IEnumerator TheRenderedUi_IsReadable_AndEveryControlIsBigEnoughToTap()
        {
            Host.SetQuery("military");
            Press(Host.Root.Q<Button>("search-filters"));
            yield return Wait(0.3f);

            var panel = Host.List.Root;
            var name = Host.List.Root.Q<Label>("result-name");
            Assert.IsNotNull(name, "a result row is drawn");
            foreach (var backdrop in new[] { Color.black, Color.white })
            {
                Color surface = panel.resolvedStyle.backgroundColor;
                Color composed = Color.Lerp(backdrop, new Color(surface.r, surface.g, surface.b), surface.a);
                Assert.GreaterOrEqual(UIAccessibility.ContrastRatio(name.resolvedStyle.color, composed), UIAccessibility.MinRatioNormalText,
                    "result text on the panel over a " + (backdrop == Color.black ? "dark" : "bright") + " camera image");
            }

            var controls = new List<VisualElement>
            {
                Host.Root.Q<Button>("search-filters"), Host.Root.Q<Button>("search-map"),
                Host.ViewModes.Root.Q<Button>("view-mode-List"), Host.Tray.Root.Q<Toggle>("facet-Category-military"),
                Host.Root.Q("result-row-" + Host.List.Rows[0].PoiId),
            };
            foreach (var c in controls)
            {
                Assert.IsNotNull(c, "control drawn");
                Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(c.worldBound.width, c.worldBound.height),
                    c.name + " is " + c.worldBound.width + "x" + c.worldBound.height + " (panel units), below 44x44");
            }
            SelectionEventBus.Select("lamp");
            yield return Wait(0.2f);
            var close = Host.Card.Root.Q<Button>("detail-card-close");
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(close.worldBound.width, close.worldBound.height), "the card's X");
        }
    }
}
