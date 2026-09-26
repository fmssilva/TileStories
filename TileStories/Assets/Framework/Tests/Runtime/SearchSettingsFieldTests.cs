using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Names the config field(s) a test proves, as a path under select_filter_search / search_demo
    // ("selection.dim_alpha", "search_demo.run_lod"). CoverageGuard_EveryFieldHasARealTest walks both
    // settings classes by reflection and fails for any field no test names -- a new field needs a new test.
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class CoversFieldAttribute : Attribute
    {
        public string Path { get; }
        public CoversFieldAttribute(string path) => Path = path;
    }

    // Every Select, Filter & Search setting, ONE AT A TIME, on the REAL wall scene with the search demo on: each
    // test changes only its field through WallSession.ApplySearchSettings (the seam the Editor's live push
    // calls), asserts what really changed on real markers / the real search UI, and saves a render of that
    // state (Assets/Screenshots/Search_Field_*.png) for the vision checklist (_2.6.1).
    public class SearchSettingsFieldTests : SearchSceneFixture
    {
        private IEnumerator Demo()
        {
            Session.ApplySearchDemo(new SearchDemoSettings { enabled = true, markers_per_category = 2, test_cases = true, show_labels = true });
            yield return Wait(1.2f);
        }

        private bool Found(string query, string poiId)
        {
            Host.SetQuery(query);
            return Host.State.Ids.Contains(poiId);
        }

        private List<MarkerView> Others(string id) => Session.SpawnedMarkers.Where(m => m != null && m.PoiId != id).ToList();

        // ---- the master switch ----

        [UnityTest, CoversField("enabled")]
        public IEnumerator Enabled_OffHidesTheUi_AndTapsSelectNothing()
        {
            yield return Demo();
            yield return ApplySearch(s => s.enabled = false);
            Assert.AreEqual(DisplayStyle.None, Host.Root.style.display.value, "no search UI");
            Assert.IsNull(Session.SelectionHighlight, "no selection responders");
            SelectionEventBus.Select(SearchDemoLayout.AccentId);
            yield return null;
            Assert.AreEqual(Session.SpawnedMarkers.Count, MarkersAt(1f).Count, "a selection dims nothing");
            SelectionEventBus.Clear();
            yield return Capture("Field_enabled_off");
            yield return ApplySearch(s => s.enabled = true);
            Assert.AreEqual(DisplayStyle.Flex, Host.Root.style.display.value, "back on live");
        }

        // ---- Selection ----

        [UnityTest, CoversField("selection.highlight_enabled")]
        public IEnumerator HighlightSelection_DimsTheOthers_OnlyWhileOn()
        {
            yield return Demo();
            yield return ApplySearch(s => s.selection.highlight_enabled = false);
            SelectionEventBus.Select(SearchDemoLayout.AccentId);
            yield return null;
            Assert.AreEqual(Session.SpawnedMarkers.Count, MarkersAt(1f).Count, "off: nothing dims");
            SelectionEventBus.Clear();
            yield return ApplySearch(s => s.selection.highlight_enabled = true);
            SelectionEventBus.Select(SearchDemoLayout.AccentId);
            yield return null;
            Assert.AreEqual(Others(SearchDemoLayout.AccentId).Count, MarkersAt(0.3f).Count, "on: every other marker at Dim Others To");
            yield return Capture("Field_selection_highlight_on");
        }

        [UnityTest, CoversField("selection.dim_alpha")]
        public IEnumerator DimOthersTo_IsTheOthersOpacity()
        {
            yield return Demo();
            yield return ApplySearch(s => s.selection.dim_alpha = 0.6f);
            SelectionEventBus.Select(SearchDemoLayout.AccentId);
            yield return null;
            Assert.AreEqual(Others(SearchDemoLayout.AccentId).Count, MarkersAt(0.6f).Count, "every other marker at 0.6");
            Assert.AreEqual(1f, Marker(SearchDemoLayout.AccentId).SelectionAlpha, 1e-3, "the selected one full");
            yield return Capture("Field_selection_dim_alpha_0.6");
        }

        // ---- Zoom on Select (the clump is a crowd, the test column a lone line 40 cm apart) ----

        private IEnumerator SelectAndSettle(string id)
        {
            ARZoomState.SetZoom(1f, 1f, 4f);
            SelectionEventBus.Clear();
            SelectionEventBus.Select(id);
            yield return Wait(0.8f);
        }

        [UnityTest, CoversField("selection.zoom.enabled")]
        public IEnumerator ZoomOnSelect_Off_NeverZooms()
        {
            yield return Demo();
            yield return ApplySearch(s => s.selection.zoom.enabled = false);
            yield return SelectAndSettle(SearchDemoLayout.ClumpIdPrefix + "0");
            Assert.AreEqual(1f, ARZoomState.ZoomFactor, 1e-3, "off: a crowded tap does not zoom");
            yield return ApplySearch(s => s.selection.zoom.enabled = true);
            yield return SelectAndSettle(SearchDemoLayout.ClumpIdPrefix + "0");
            Assert.AreEqual(2f, ARZoomState.ZoomFactor, 0.05, "on: it zooms by Zoom Factor");
            yield return Capture("Field_zoom_enabled_on");
        }

        [UnityTest, CoversField("selection.zoom.trigger")]
        public IEnumerator ZoomTrigger_ClusterOnly_AMarkerTapNeverZooms()
        {
            yield return Demo();
            yield return ApplySearch(s => s.selection.zoom.trigger = "cluster");
            yield return SelectAndSettle(SearchDemoLayout.ClumpIdPrefix + "0");
            Assert.AreEqual(1f, ARZoomState.ZoomFactor, 1e-3, "Cluster: a crowded MARKER does not zoom (a cluster tap does: SearchDemoTests)");
            yield return ApplySearch(s => s.selection.zoom.trigger = "both");
            yield return SelectAndSettle(SearchDemoLayout.ClumpIdPrefix + "0");
            Assert.AreEqual(2f, ARZoomState.ZoomFactor, 0.05, "Marker or Cluster: it does");
        }

        // The grid marker nearest the screen centre (not a clump point): room for a full zoom, neighbours a
        // grid spacing away
        private string CentralGridMarkerId()
        {
            var centre = new Vector2(Screen.width, Screen.height) * 0.5f;
            return Session.SpawnedMarkers.Where(m => !m.PoiId.StartsWith(SearchDemoLayout.ClumpIdPrefix))
                .OrderBy(m => ((Vector2)Cam.WorldToScreenPoint(m.UndisplacedWorldPosition) - centre).sqrMagnitude).First().PoiId;
        }

        [UnityTest, CoversField("selection.zoom.neighbour_radius_px")]
        public IEnumerator CrowdRadius_DecidesWhoCountsAsANeighbour()
        {
            yield return Demo();
            string id = CentralGridMarkerId();
            yield return ApplySearch(s => s.selection.zoom.neighbour_radius_px = ZoomOnSelectSettings.NeighbourRadiusMin);
            yield return SelectAndSettle(id);
            Assert.AreEqual(0, Session.ZoomOnSelect.LastNeighbourCount, "10 px: the grid's neighbours are too far");
            Assert.AreEqual(1f, ARZoomState.ZoomFactor, 1e-3);
            yield return ApplySearch(s => s.selection.zoom.neighbour_radius_px = 400f);
            yield return SelectAndSettle(id);
            Assert.GreaterOrEqual(Session.ZoomOnSelect.LastNeighbourCount, 2, "400 px: the neighbours count");
            Assert.AreEqual(2f, ARZoomState.ZoomFactor, 0.05, "so the same tap zooms");
        }

        [UnityTest, CoversField("selection.zoom.min_neighbours")]
        public IEnumerator MinNeighbours_IsTheCrowdSize()
        {
            yield return Demo();
            yield return ApplySearch(s => s.selection.zoom.min_neighbours = 10);
            yield return SelectAndSettle(SearchDemoLayout.ClumpIdPrefix + "1");
            Assert.AreEqual(1f, ARZoomState.ZoomFactor, 1e-3, "10: the five-point clump is not a crowd");
            yield return ApplySearch(s => { s.selection.zoom.min_neighbours = 0; s.selection.zoom.neighbour_radius_px = ZoomOnSelectSettings.NeighbourRadiusMin; });
            yield return SelectAndSettle(CentralGridMarkerId());
            Assert.AreEqual(0, Session.ZoomOnSelect.LastNeighbourCount, "precondition: a point with no neighbour");
            Assert.AreEqual(2f, ARZoomState.ZoomFactor, 0.05, "0: every tap zooms, even a lone point");
        }

        [UnityTest, CoversField("selection.zoom.factor")]
        public IEnumerator ZoomFactor_IsHowMuchOneSelectZooms_NeverLosingTheTappedPoint()
        {
            yield return Demo();
            yield return ApplySearch(s => { s.selection.zoom.factor = 3f; s.selection.zoom.min_neighbours = 0; });
            var centre = new Vector2(Screen.width, Screen.height) * 0.5f;
            var central = Session.SpawnedMarkers.OrderBy(m => ((Vector2)Cam.WorldToScreenPoint(m.UndisplacedWorldPosition) - centre).sqrMagnitude).First();
            yield return SelectAndSettle(central.PoiId);
            Assert.AreEqual(3f, ARZoomState.ZoomFactor, 0.05, "a point near the centre: the full x3");
            yield return Capture("Field_zoom_factor_3");

            // - a point well off centre: a full zoom would push it off screen (the old rule did exactly that);
            //   chosen at x1 -- offsets measured in the zoomed view would pick the wrong point
            SelectionEventBus.Clear();
            ARZoomState.SetZoom(1f, 1f, 4f);
            yield return null;
            yield return null;
            bool OnScreen(Vector3 p) => p.z > 0f && p.x > 0f && p.x < Screen.width && p.y > 0f && p.y < Screen.height;
            // - viewport offset 0 = centre, 1 = edge; the farthest point still inside 0.8 (past 0.85 nothing zooms)
            float Offset(Vector3 p) => 2f * Mathf.Max(Mathf.Abs(p.x / Screen.width - 0.5f), Mathf.Abs(p.y / Screen.height - 0.5f));
            var offCentre = Session.SpawnedMarkers.Select(m => (m, sp: Cam.WorldToScreenPoint(m.UndisplacedWorldPosition)))
                .Where(x => OnScreen(x.sp) && Offset(x.sp) < 0.8f)
                .OrderByDescending(x => Offset(x.sp))
                .First();
            // - x4 (the Zoom section's Max Zoom): uncapped, it would carry this point past the margin
            const float big = 4f;
            yield return ApplySearch(s => { s.selection.zoom.factor = big; s.selection.zoom.min_neighbours = 0; });
            ARZoomState.SetZoom(1f, 1f, 4f);
            yield return null;
            float halfFov = Cam.fieldOfView * 0.5f * Mathf.Deg2Rad;
            float uncapped = Offset(offCentre.sp) * Mathf.Tan(halfFov) / Mathf.Tan(halfFov / big);
            Assert.Greater(uncapped, ZoomOnSelectController.KeepOnScreen, "precondition: a full x4 would push this point past the margin");
            yield return SelectAndSettle(offCentre.m.PoiId);
            Assert.Greater(ARZoomState.ZoomFactor, 1f, "it still zooms in");
            Assert.Less(ARZoomState.ZoomFactor, big - 0.05f, "less than x4: capped");
            Vector3 after = Cam.WorldToScreenPoint(offCentre.m.UndisplacedWorldPosition);
            Assert.IsTrue(OnScreen(after), $"the tapped point stays on screen after the zoom ({after.x:0},{after.y:0} in {Screen.width}x{Screen.height})");
            yield return Capture("Field_zoom_factor_4_off_centre");
        }

        // ---- Search ----

        [UnityTest, CoversField("search.mode")]
        public IEnumerator SearchMode_AsYouTypeWaitsForAPause_OnEnterWaitsForEnter()
        {
            yield return Demo();
            var field = Host.Root.Q<TextField>("search-field");
            yield return ApplySearch(s => s.search.mode = "dynamic");
            Assert.IsFalse(Host.SearchBar.IsDelayed);
            field.value = "lantern";
            Assert.AreEqual("", Host.Query, "as you type: not before the pause");
            yield return Wait(0.4f);
            Assert.AreEqual("lantern", Host.Query, "as you type: after the pause");

            field.value = "";
            yield return ApplySearch(s => s.search.mode = "explicit");
            Assert.IsTrue(Host.SearchBar.IsDelayed, "on Enter: the field commits only on Enter (or leaving it)");
            using (var enter = KeyDownEvent.GetPooled('\n', KeyCode.Return, EventModifiers.None))
            {
                field.SetValueWithoutNotify("stone");
                enter.target = field;
                field.SendEvent(enter);
            }
            yield return null;
            Assert.AreEqual("stone", Host.Query, "Enter searches what was typed");
        }

        [UnityTest, CoversField("search.match_mode")]
        public IEnumerator MatchWords_EveryWordVersusAnyWord()
        {
            yield return Demo();
            yield return ApplySearch(s => s.search.match_mode = "all");
            Assert.IsFalse(Found("stone lantern", SearchDemoLayout.FieldId), "Every word: Stone Gate has no 'lantern'");
            yield return ApplySearch(s => s.search.match_mode = "any");
            Assert.IsTrue(Found("stone lantern", SearchDemoLayout.FieldId), "Any word: it is found");
            Assert.IsTrue(Host.State.Ids.Contains(SearchDemoLayout.TypoId), "and so is Lantern Tower");
            yield return Capture("Field_search_match_any");
        }

        [UnityTest, CoversField("search.prefix_matching")]
        public IEnumerator PartialWords_OffNamesOnlyAllFields()
        {
            yield return Demo();
            yield return ApplySearch(s => s.search.prefix_matching = "off");
            Assert.IsFalse(Found("lant", SearchDemoLayout.TypoId), "Off: 'lant' is not 'lantern'");
            yield return ApplySearch(s => s.search.prefix_matching = "name_only");
            Assert.IsTrue(Found("lant", SearchDemoLayout.TypoId), "Names only: a name completes");
            Assert.IsFalse(Found("grani", SearchDemoLayout.FieldId), "Names only: a keyword does not");
            yield return ApplySearch(s => s.search.prefix_matching = "all_fields");
            Assert.IsTrue(Found("grani", SearchDemoLayout.FieldId), "All fields: a keyword completes too");
            yield return Capture("Field_search_prefix_all");
        }

        [UnityTest, CoversField("search.typo_tolerance")]
        public IEnumerator TypoTolerance_ZeroOneTwo()
        {
            yield return Demo();
            yield return ApplySearch(s => s.search.typo_tolerance = 0);
            Assert.IsFalse(Found("lantren", SearchDemoLayout.TypoId), "0: exact words only");
            yield return ApplySearch(s => s.search.typo_tolerance = 1);
            Assert.IsTrue(Found("lantren", SearchDemoLayout.TypoId), "1: one typo");
            Assert.IsFalse(Found("obsrevatroy", SearchDemoLayout.TwoTyposId), "1: not two");
            yield return ApplySearch(s => s.search.typo_tolerance = 2);
            Assert.IsTrue(Found("obsrevatroy", SearchDemoLayout.TwoTyposId), "2: two typos in a long word");
            yield return Capture("Field_search_typo_2");
        }

        [UnityTest, CoversField("search.no_results_message")]
        public IEnumerator NoResultsMessage_IsShown_WithTheQuery()
        {
            yield return Demo();
            yield return ApplySearch(s => s.search.no_results_message = "Nothing called {query} here");
            Host.SetQuery("zzzzqqq");
            yield return null;
            Assert.IsTrue(Host.List.EmptyShown);
            Assert.AreEqual("Nothing called zzzzqqq here", Host.List.EmptyMessage);
            yield return Capture("Field_search_no_results");
        }

        // ---- Filters ----

        private IEnumerator FacetToggle(string group, Action<FilterSettings, bool> set)
        {
            yield return Demo();
            yield return ApplySearch(s => set(s.filter, false));
            Assert.IsNull(Host.Tray.Root.Q("facet-group-" + group), group + " off: its group leaves the tray");
            yield return ApplySearch(s => set(s.filter, true));
            Assert.IsNotNull(Host.Tray.Root.Q("facet-group-" + group), group + " on: back");
        }

        [UnityTest, CoversField("filter.category_facet")]
        public IEnumerator CategoryFilter_Toggle() => FacetToggle("Category", (f, on) => f.category_facet = on);

        [UnityTest, CoversField("filter.badge_facet")]
        public IEnumerator BadgeFilter_Toggle() => FacetToggle("Badge", (f, on) => f.badge_facet = on);

        [UnityTest, CoversField("filter.status_facet")]
        public IEnumerator StatusFilter_Toggle() => FacetToggle("Status", (f, on) => f.status_facet = on);

        [UnityTest, CoversField("filter.hierarchy_facet")]
        public IEnumerator LevelFilter_Toggle() => FacetToggle("Hierarchy", (f, on) => f.hierarchy_facet = on);

        [UnityTest, CoversField("filter.mismatch"), CoversField("filter.dim_alpha")]
        public IEnumerator FilteredOutMarkers_HideOrDimToDimFilteredTo()
        {
            yield return Demo();
            yield return ApplySearch(s => s.filter.mismatch = "hide");
            Host.SetQuery("crowd");
            yield return null;
            Assert.AreEqual(Session.SpawnedMarkers.Count - SearchDemoLayout.ClumpCount, MarkersAt(0f).Count, "Hide: every non-result gone");
            yield return ApplySearch(s => { s.filter.mismatch = "dim"; s.filter.dim_alpha = 0.5f; });
            Host.SetQuery("crowd");
            yield return null;
            Assert.AreEqual(Session.SpawnedMarkers.Count - SearchDemoLayout.ClumpCount, MarkersAt(0.5f).Count, "Dim: at Dim Filtered To");
            Assert.AreEqual(SearchDemoLayout.ClumpCount, MarkersAt(1f).Count, "the results stay full");
            yield return Capture("Field_filter_dim_0.5");
        }

        [UnityTest, CoversField("filter.relax_suggestion")]
        public IEnumerator RelaxSuggestion_OffRemovesTheButton()
        {
            yield return Demo();
            var cfg = ConfigCopy();
            Press(Host.Root.Q<Button>("search-filters"));
            // - a category AND a level none of that category's demo POIs has: two filters with no POI in common
            string catA = cfg.category_styles[0].category;
            var emptyPair = Session.SearchPois.Where(p => p.category == catA).Select(p => p.hierarchy_level_key).Distinct().ToList();
            string levelNone = cfg.hierarchy_levels.Select(l => l.key).First(k => !emptyPair.Contains(k));
            Press(Host.Tray.Root.Q<Toggle>("facet-Category-" + catA));
            Press(Host.Tray.Root.Q<Toggle>("facet-Hierarchy-" + levelNone));
            yield return null;
            Assert.IsEmpty(Host.State.Ids, "precondition: the two filters share no POI");
            Assert.IsNotNull(Host.List.RelaxText, "on: a 'Remove filter' button");
            Press(Host.Root.Q<Button>("search-filters"));   // close the tray: it covers the list
            yield return Wait(0.2f);
            yield return Capture("Field_filter_relax_on");
            yield return ApplySearch(s => s.filter.relax_suggestion = false);
            Assert.IsNull(Host.List.RelaxText, "off: no button");
        }

        [UnityTest, CoversField("search.no_results_filters_message")]
        public IEnumerator NoResultsFiltersMessage_IsShown_WhenFiltersAloneLeaveNothing()
        {
            yield return Demo();
            yield return ApplySearch(s => s.search.no_results_filters_message = "No point fits these filters");
            var cfg = ConfigCopy();
            string catA = cfg.category_styles[0].category;
            var levelsOfA = Session.SearchPois.Where(p => p.category == catA).Select(p => p.hierarchy_level_key).Distinct().ToList();
            string levelNone = cfg.hierarchy_levels.Select(l => l.key).First(k => !levelsOfA.Contains(k));
            Host.Tray.SetFacet(FacetGroup.Category, catA, true);
            Host.Tray.SetFacet(FacetGroup.Hierarchy, levelNone, true);
            yield return null;
            Assert.IsEmpty(Host.State.Ids, "precondition: the two filters share no POI");
            Assert.AreEqual("No point fits these filters", Host.List.EmptyMessage, "no text typed: the filters message");
            yield return Capture("Field_search_no_results_filters");
        }

        // ---- Results & Views ----

        [UnityTest, CoversField("results.default_view")]
        public IEnumerator DefaultView_IsWhereResultsShowFirst()
        {
            yield return Demo();
            yield return ApplySearch(s => s.results.default_view = "camera_highlight");
            Host.SetQuery("crowd");
            yield return null;
            Assert.IsFalse(Host.List.IsShown || Host.Minimap.IsShown, "Highlight: no panel, only the markers");
            yield return Capture("Field_results_view_highlight");
            yield return ApplySearch(s => s.results.default_view = "minimap");
            Host.SetQuery("crowd");
            yield return null;
            Assert.IsTrue(Host.Minimap.IsShown, "Map: the minimap");
            Assert.IsFalse(Host.List.IsShown);
            yield return Capture("Field_results_view_map");
            yield return ApplySearch(s => s.results.default_view = "list");
            Host.SetQuery("crowd");
            yield return null;
            Assert.IsTrue(Host.List.IsShown, "List: the list");
        }

        [UnityTest, CoversField("results.remember_last_view")]
        public IEnumerator RememberLastView_ReopensWithTheVisitorsChoice()
        {
            yield return Demo();
            yield return ApplySearch(s => s.results.remember_last_view = true);
            Host.ViewModes.Choose(ViewMode.CameraHighlight);
            yield return ApplySearch(s => s.results.remember_last_view = true);   // any rebuild = the app reopening the UI
            Assert.AreEqual(ViewMode.CameraHighlight, Host.ViewModes.Mode, "on: the device's last view");
            yield return ApplySearch(s => s.results.remember_last_view = false);
            Assert.AreEqual(ViewMode.List, Host.ViewModes.Mode, "off: always Default View");
        }

        [UnityTest, CoversField("results.recent_count"), CoversField("results.suggestion_source")]
        public IEnumerator RecentSearches_AreRememberedUpToTheCount_AndRecentFirstPutsThemFirst()
        {
            yield return Demo();
            yield return ApplySearch(s => { s.results.recent_count = 0; s.results.suggestion_source = "recent_first"; });
            Host.SetQuery("lantern");
            SelectionEventBus.Select(SearchDemoLayout.TypoId);
            SelectionEventBus.Clear();
            yield return ApplySearch(s => { s.results.recent_count = 0; s.results.suggestion_source = "recent_first"; });
            Assert.IsFalse(Host.SearchBar.SuggestionTerms.Contains("lantern"), "0: nothing remembered");

            yield return ApplySearch(s => { s.results.recent_count = 3; s.results.suggestion_source = "recent_first"; });
            Host.SetQuery("lantern");
            SelectionEventBus.Select(SearchDemoLayout.TypoId);
            SelectionEventBus.Clear();
            yield return ApplySearch(s => { s.results.recent_count = 3; s.results.suggestion_source = "recent_first"; });
            Assert.AreEqual("lantern", Host.SearchBar.SuggestionTerms[0], "3 + Recent first: the last search leads");

            yield return ApplySearch(s => { s.results.recent_count = 3; s.results.suggestion_source = "category_distribution"; });
            Assert.AreNotEqual("lantern", Host.SearchBar.SuggestionTerms[0], "Categories: the wall's categories lead");
            var categories = ConfigCopy().category_styles.Select(c => c.category).ToList();
            Assert.IsTrue(categories.Contains(Host.SearchBar.SuggestionTerms[0]));
        }

        [UnityTest, CoversField("results.suggestions_enabled")]
        public IEnumerator Suggestions_OffOffersNothing()
        {
            yield return Demo();
            yield return ApplySearch(s => s.results.suggestions_enabled = true);
            Assert.IsNotEmpty(Host.SearchBar.SuggestionTerms, "on: the categories are offered");
            yield return ApplySearch(s => s.results.suggestions_enabled = false);
            Assert.IsEmpty(Host.SearchBar.SuggestionTerms, "off: nothing");
        }

        // ---- Minimap ----

        [UnityTest, CoversField("minimap.enabled")]
        public IEnumerator EnableMinimap_OffRemovesMapButtonAndMapView()
        {
            yield return Demo();
            yield return ApplySearch(s => s.minimap.enabled = false);
            Assert.IsFalse(Host.Minimap.IsShown || Host.SearchBar.MapButtonShown, "no map, no Map button");
            Assert.IsFalse(Host.ViewModes.MapOffered, "no Map view");
            yield return ApplySearch(s => s.minimap.enabled = true);
            Assert.IsTrue(Host.SearchBar.MapButtonShown && Host.ViewModes.MapOffered);
        }

        [UnityTest, CoversField("minimap.visibility")]
        public IEnumerator Visibility_AlwaysOrButton()
        {
            yield return Demo();
            yield return ApplySearch(s => s.minimap.visibility = "always");
            Assert.IsTrue(Host.Minimap.IsShown, "Always: on screen with no search");
            Assert.IsFalse(Host.SearchBar.MapButtonShown);
            yield return Capture("Field_minimap_always");
            yield return ApplySearch(s => s.minimap.visibility = "toggle");
            Assert.IsFalse(Host.Minimap.IsShown);
            Assert.IsTrue(Host.SearchBar.MapButtonShown, "Button: a Map button");
        }

        [UnityTest, CoversField("minimap.icon_style")]
        public IEnumerator DotStyle_PlainCategoryColourOrIcon()
        {
            yield return Demo();
            string id = SearchDemoLayout.AccentId;
            var poi = Session.SearchPois.First(p => p.id == id);
            yield return ApplySearch(s => { s.minimap.visibility = "always"; s.minimap.icon_style = "dots_only"; });
            Assert.AreEqual(StyleKeyword.Null, Host.Minimap.DotOf(id).style.backgroundColor.keyword, "Plain dots: the stylesheet's colour");
            yield return ApplySearch(s => { s.minimap.visibility = "always"; s.minimap.icon_style = "category_colored_dots"; });
            Assert.AreEqual(CategoryPalette.ResolveColor(poi.category), Host.Minimap.DotOf(id).style.backgroundColor.value, "Category colours");
            yield return ApplySearch(s => { s.minimap.visibility = "always"; s.minimap.icon_style = "mini_icons"; });
            Assert.IsTrue(Host.Minimap.DotOf(id).ClassListContains("minimap-dot--icon"), "Category icons: the icon dot");
            Assert.IsNotNull(Host.Minimap.DotOf(id).Q("minimap-icon-" + id), "with the category's icon");
            yield return Wait(0.2f);
            yield return Capture("Field_minimap_icons");
        }

        [UnityTest, CoversField("minimap.dot_size_px"), CoversField("minimap.tap_target_px")]
        public IEnumerator DotSize_AndTapTarget_SizeTheDotAndItsHitZone()
        {
            yield return Demo();
            string id = SearchDemoLayout.AccentId;
            yield return ApplySearch(s => { s.minimap.visibility = "always"; s.minimap.dot_size_px = 30f; s.minimap.tap_target_px = 60f; });
            var dot = Host.Minimap.DotOf(id);
            Assert.AreEqual(30f, dot.style.width.value.value, 1e-3, "Dot Size");
            Assert.AreEqual(60f, dot.parent.style.width.value.value, 1e-3, "Tap Target");
            yield return ApplySearch(s => { s.minimap.visibility = "always"; s.minimap.dot_size_px = 8f; s.minimap.tap_target_px = 44f; });
            Assert.AreEqual(8f, Host.Minimap.DotOf(id).style.width.value.value, 1e-3);
            Assert.AreEqual(44f, Host.Minimap.DotOf(id).parent.style.width.value.value, 1e-3);
        }

        [UnityTest, CoversField("minimap.projection")]
        public IEnumerator Projection_WallKeepsTheDemoRows_FloorFlattensThem()
        {
            yield return Demo();
            // - the demo is a flat wall facing the camera: its rows differ in height, not in depth
            float Spread(IEnumerable<float> v) => v.Max() - v.Min();
            yield return ApplySearch(s => s.minimap.projection = "wall");
            float wallRows = Spread(Host.Minimap.Positions.Values.Select(p => p.y));
            yield return ApplySearch(s => s.minimap.projection = "floor");
            float floorRows = Spread(Host.Minimap.Positions.Values.Select(p => p.y));
            Assert.Greater(wallRows, 0.5f, "Wall (front): the rows spread over the map");
            Assert.Less(floorRows, wallRows, "Floor (top-down): seen from above the rows collapse");
            yield return ApplySearch(s => s.minimap.projection = "auto");
            Assert.AreEqual(wallRows, Spread(Host.Minimap.Positions.Values.Select(p => p.y)), 1e-4, "Auto picks the wall view for a flat demo");
        }

        [UnityTest, CoversField("minimap.bounds_mode"), CoversField("minimap.bounds_min"), CoversField("minimap.bounds_max")]
        public IEnumerator Bounds_ManualBoxPlacesTheDots()
        {
            yield return Demo();
            var worlds = Session.SpawnedMarkers.Select(m => m.UndisplacedWorldPosition).ToList();
            Vector3 lo = worlds.Aggregate(Vector3.Min), hi = worlds.Aggregate(Vector3.Max);
            var marker = Marker(SearchDemoLayout.AccentId);

            yield return ApplySearch(s => { s.minimap.projection = "wall"; s.minimap.bounds_mode = "auto"; });
            Vector2 auto = Host.Minimap.Positions[SearchDemoLayout.AccentId];

            Vector3 min = lo - new Vector3(2f, 2f, 2f), max = hi + new Vector3(2f, 2f, 2f);
            yield return ApplySearch(s => { s.minimap.projection = "wall"; s.minimap.bounds_mode = "manual"; s.minimap.bounds_min = min; s.minimap.bounds_max = max; });
            Vector2 manual = Host.Minimap.Positions[SearchDemoLayout.AccentId];
            Vector3 w = marker.UndisplacedWorldPosition;
            Assert.AreEqual(Mathf.InverseLerp(min.x, max.x, w.x), manual.x, 1e-3, "Manual: x inside Bounds Min..Max");
            Assert.AreEqual(Mathf.InverseLerp(min.y, max.y, w.y), manual.y, 1e-3, "Manual: y inside Bounds Min..Max");
            Assert.AreNotEqual(auto, manual, "a bigger box moves the dots toward the centre");
            yield return ApplySearch(s => s.minimap.visibility = "always");
            yield return Capture("Field_minimap_manual_bounds");

            // - each corner on its own moves the dots
            yield return ApplySearch(s => { s.minimap.projection = "wall"; s.minimap.bounds_mode = "manual"; s.minimap.bounds_min = min - Vector3.one * 3f; s.minimap.bounds_max = max; });
            Assert.AreNotEqual(manual, Host.Minimap.Positions[SearchDemoLayout.AccentId], "Bounds Min");
            yield return ApplySearch(s => { s.minimap.projection = "wall"; s.minimap.bounds_mode = "manual"; s.minimap.bounds_min = min; s.minimap.bounds_max = max + Vector3.one * 3f; });
            Assert.AreNotEqual(manual, Host.Minimap.Positions[SearchDemoLayout.AccentId], "Bounds Max");
        }

        // ---- Voice ----

        [UnityTest, CoversField("voice.enabled")]
        public IEnumerator Voice_TheMicSearchesWhatItHears()
        {
            yield return Demo();
            yield return ApplySearch(s => s.voice.enabled = false);
            Assert.IsFalse(Host.SearchBar.MicShown, "off: no mic");
            string saved = DebugTranscriber.EditorPhrase;
            try
            {
                DebugTranscriber.EditorPhrase = "lantern";   // what the Editor's Try a Query sets
                yield return ApplySearch(s => s.voice.enabled = true);
                Assert.IsTrue(Host.SearchBar.MicShown, "on: the mic");
                Press(Host.Root.Q<Button>("search-mic"));
                yield return null;
                Assert.AreEqual("lantern", Host.Query, "the heard phrase is searched");
                Assert.IsTrue(Host.State.Ids.Contains(SearchDemoLayout.TypoId), "and finds its point");
                yield return Capture("Field_voice_heard");
            }
            finally { DebugTranscriber.EditorPhrase = saved; }
        }

        [UnityTest, CoversField("voice.indicator_style")]
        public IEnumerator VoiceIndicator_MicTextOrListenBar_WhileListening()
        {
            yield return Demo();
            foreach (string style in new[] { "mic_text", "listen_bar" })
            {
                yield return ApplySearch(s => { s.voice.enabled = true; s.voice.indicator_style = style; });
                string micWhileListening = null;
                bool barWhileListening = false;
                // - subscribed after the host, so it reads the bar as the host just drew it
                Host.Voice.StateChanged += state =>
                {
                    if (state != VoiceSearchState.Listening) return;
                    micWhileListening = Host.SearchBar.MicText;
                    barWhileListening = Host.SearchBar.ListenBarShown;
                };
                Press(Host.Root.Q<Button>("search-mic"));
                yield return null;
                if (style == "mic_text")
                {
                    Assert.AreEqual("...", micWhileListening, "Mic text: the button reads '...'");
                    Assert.IsFalse(barWhileListening);
                }
                else
                {
                    Assert.IsTrue(barWhileListening, "Listen bar: the bar shows");
                    Assert.AreEqual("Mic", micWhileListening, "and the button keeps its label");
                }
            }
        }

        // ---- the guard ----

        // Every leaf field of the two settings classes, as the path CoversField uses
        private static List<string> FieldPaths()
        {
            var paths = new List<string>();
            void Walk(Type type, string prefix)
            {
                foreach (var f in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (f.FieldType.IsClass && f.FieldType != typeof(string) && f.FieldType.Namespace == typeof(WallConfigData).Namespace)
                        Walk(f.FieldType, prefix + f.Name + ".");
                    else paths.Add(prefix + f.Name);
                }
            }
            Walk(typeof(SelectFilterSearchSettings), "");
            Walk(typeof(SearchDemoSettings), "search_demo.");
            return paths;
        }

        [Test]
        public void CoverageGuard_EveryFieldHasARealTest()
        {
            var covered = typeof(SearchSettingsFieldTests).Assembly.GetTypes()
                .SelectMany(t => t.GetMethods())
                .SelectMany(m => m.GetCustomAttributes<CoversFieldAttribute>().Select(a => a.Path))
                .ToHashSet();
            var paths = FieldPaths();
            Assert.GreaterOrEqual(paths.Count, 43, "precondition: every field was found");
            foreach (string path in paths)
                Assert.IsTrue(covered.Contains(path), $"'{path}' has no [CoversField] test on the real scene: add one");
            foreach (string path in covered)
                Assert.IsTrue(paths.Contains(path), $"[CoversField(\"{path}\")] names no field (renamed or removed?)");
        }
    }
}
