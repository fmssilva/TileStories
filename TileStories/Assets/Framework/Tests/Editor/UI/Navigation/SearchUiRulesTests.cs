using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.Editor.Tests
{
    // The Select, Filter & Search rules and views without a scene (spec _2.6): the selection bus, the one
    // alpha rule, zoom-on-select, which panels show, the one result set (search + filters + relax), the
    // facet evaluator and tray, the results list, the minimap layout and view, the detail card, the search
    // bar, voice, and the search demo's layout. Views are the real classes built into a plain VisualElement.
    public class SearchUiRulesTests
    {
        [SetUp]
        public void SetUp() => SelectionEventBus.ResetState();

        [TearDown]
        public void TearDown() => SelectionEventBus.ResetState();

        // ---- selection bus ----

        [Test]
        public void SelectingTheSelectedPoiAgain_Clears_AndEveryListenerSeesOneEvent()
        {
            var events = new List<string>();
            System.Action<string> onSelect = id => events.Add("select:" + id);
            System.Action onClear = () => events.Add("clear");
            SelectionEventBus.OnMarkerSelected += onSelect;
            SelectionEventBus.OnSelectionCleared += onClear;
            try
            {
                SelectionEventBus.Select("a");
                SelectionEventBus.Select("b");
                SelectionEventBus.Select("b");     // re-tap: deselect
                SelectionEventBus.Clear();         // nothing selected: no event
                CollectionAssert.AreEqual(new[] { "select:a", "select:b", "clear" }, events);
                Assert.IsNull(SelectionEventBus.CurrentPoiId);
            }
            finally
            {
                SelectionEventBus.OnMarkerSelected -= onSelect;
                SelectionEventBus.OnSelectionCleared -= onClear;
            }
        }

        [Test]
        public void ACluster_ReportsItsMembers_AndIsNeverTheSelection()
        {
            IReadOnlyList<string> got = null;
            System.Action<IReadOnlyList<string>> onCluster = ids => got = ids;
            SelectionEventBus.OnClusterSelected += onCluster;
            try
            {
                SelectionEventBus.SelectCluster(new List<string> { "a", "b" });
                CollectionAssert.AreEqual(new[] { "a", "b" }, got);
                Assert.IsNull(SelectionEventBus.CurrentPoiId);
            }
            finally { SelectionEventBus.OnClusterSelected -= onCluster; }
        }

        // ---- the one alpha rule ----

        [TestCase(true, true, true, false, true, 1f)]     // selected: full even outside the result set
        [TestCase(false, true, true, false, true, 0f)]    // outside the result set: the mismatch alpha, selection or not
        [TestCase(false, false, true, false, true, 0f)]
        [TestCase(false, true, true, true, true, 0.3f)]   // in the set, another selected: the selection dim
        [TestCase(false, true, false, false, true, 0.3f)] // no result set, another selected: the dim
        [TestCase(false, true, false, false, false, 1f)]  // highlight off: no dim
        [TestCase(false, false, true, true, true, 1f)]    // in the set, nothing selected: full
        [TestCase(false, false, false, false, true, 1f)]  // nothing at all: full
        public void SelectionAlphaRule_TruthTable(bool isSelected, bool anySelected, bool resultSetActive, bool inResultSet,
            bool highlight, float expected)
        {
            Assert.AreEqual(expected, SelectionAlphaRule.AlphaFor(isSelected, anySelected, resultSetActive, inResultSet,
                highlight, 0.3f, 0f), 1e-5);
        }

        // ---- zoom-on-select ----

        [Test]
        public void ZoomOnSelect_EverySettingDecides()
        {
            var z = new ZoomOnSelectSettings { enabled = true, trigger = "marker", min_neighbours = 2, factor = 2f };
            Assert.AreEqual(3f, ZoomOnSelectController.ComputeZoomTarget(z, false, 2, 1.5f), "a crowd of 2: zoom x2");
            Assert.IsNull(ZoomOnSelectController.ComputeZoomTarget(z, false, 1, 1.5f), "below Min Neighbours: no zoom");
            Assert.IsNull(ZoomOnSelectController.ComputeZoomTarget(z, true, 9, 1f), "trigger Marker: a cluster does not zoom");
            z.trigger = "cluster";
            Assert.IsNull(ZoomOnSelectController.ComputeZoomTarget(z, false, 9, 1f), "trigger Cluster: a marker does not zoom");
            Assert.AreEqual(2f, ZoomOnSelectController.ComputeZoomTarget(z, true, 1, 1f), "a cluster is always a crowd");
            z.trigger = "both";
            Assert.AreEqual(2f, ZoomOnSelectController.ComputeZoomTarget(z, false, 2, 1f));
            Assert.AreEqual(2f, ZoomOnSelectController.ComputeZoomTarget(z, true, 2, 1f));
            z.factor = 3.5f;
            Assert.AreEqual(3.5f, ZoomOnSelectController.ComputeZoomTarget(z, false, 2, 1f), 1e-5, "Zoom Factor");
            z.enabled = false;
            Assert.IsNull(ZoomOnSelectController.ComputeZoomTarget(z, false, 20, 1f), "Zoom on Select off");
        }

        [Test]
        public void ZoomOnSelect_NeverPushesTheTappedPointOffScreen()
        {
            const float fov = 60f;
            Assert.AreEqual(float.MaxValue, ZoomOnSelectController.MaxZoomKeepingOnScreen(Vector2.zero, fov), "the centre never limits");
            foreach (float m in new[] { 0.1f, 0.3f, 0.5f, 0.8f })
            {
                float f = ZoomOnSelectController.MaxZoomKeepingOnScreen(new Vector2(m, -0.05f), fov);
                // - after zooming by f (fov / f), the point's offset grows by tan(fov/2) / tan(fov/2f)
                float after = m * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad) / Mathf.Tan(fov * 0.5f / f * Mathf.Deg2Rad);
                Assert.AreEqual(ZoomOnSelectController.KeepOnScreen, after, 1e-3, $"offset {m}: the largest zoom lands it exactly on the margin");
            }
            Assert.AreEqual(1f, ZoomOnSelectController.MaxZoomKeepingOnScreen(new Vector2(0.9f, 0f), fov), "already past the margin: no room");

            var z = new ZoomOnSelectSettings { factor = 3f, min_neighbours = 0 };
            float cap = ZoomOnSelectController.MaxZoomKeepingOnScreen(new Vector2(0.5f, 0f), fov);
            Assert.AreEqual(cap, ZoomOnSelectController.ComputeZoomTarget(z, false, 0, 1f, cap).Value, 1e-5, "Zoom Factor capped to keep it on screen");
            Assert.IsNull(ZoomOnSelectController.ComputeZoomTarget(z, false, 0, 1f, 1.05f), "hardly any room: no pointless tiny zoom");
            Assert.AreEqual(3f, ZoomOnSelectController.ComputeZoomTarget(z, false, 0, 1f, 10f).Value, 1e-5, "room enough: the full Zoom Factor");
        }

        [Test]
        public void CountNeighbours_CountsTheOthersInsideTheRadius()
        {
            var others = new List<Vector2> { new(10, 0), new(0, 49), new(50, 1), new(300, 300) };
            Assert.AreEqual(2, ZoomOnSelectController.CountNeighbours(Vector2.zero, others, 50f));
            Assert.AreEqual(3, ZoomOnSelectController.CountNeighbours(Vector2.zero, others, 60f));
            Assert.AreEqual(0, ZoomOnSelectController.CountNeighbours(Vector2.zero, others, 5f));
        }

        // ---- which panels show ----

        [Test]
        public void SearchPanelsRule_Table()
        {
            var idle = SearchPanelsRule.Resolve(false, ViewMode.List, false, true, false, false);
            Assert.IsFalse(idle.List || idle.ViewModes || idle.Minimap || idle.Card, "idle: the camera view stays clear");
            Assert.IsTrue(idle.MinimapButton, "the Map button shows with Visibility = Button");

            var list = SearchPanelsRule.Resolve(true, ViewMode.List, false, true, false, false);
            Assert.IsTrue(list.List && list.ViewModes && !list.Minimap);

            var map = SearchPanelsRule.Resolve(true, ViewMode.Minimap, false, true, false, false);
            Assert.IsTrue(map.Minimap && !map.List, "Map view opens the minimap");

            var highlight = SearchPanelsRule.Resolve(true, ViewMode.CameraHighlight, false, true, false, false);
            Assert.IsFalse(highlight.List || highlight.Minimap, "Highlight: no panel");

            var selected = SearchPanelsRule.Resolve(true, ViewMode.List, true, true, false, false);
            Assert.IsTrue(selected.Card && !selected.List, "a selection: the card instead of the list");

            var always = SearchPanelsRule.Resolve(false, ViewMode.List, false, true, true, false);
            Assert.IsTrue(always.Minimap && !always.MinimapButton, "Visibility = Always: always on, no button");

            var toggled = SearchPanelsRule.Resolve(false, ViewMode.List, false, true, false, true);
            Assert.IsTrue(toggled.Minimap, "opened with its button");

            var off = SearchPanelsRule.Resolve(true, ViewMode.Minimap, false, false, true, true);
            Assert.IsFalse(off.Minimap || off.MinimapButton, "Enable Minimap off: never");
        }

        // ---- view switch ----

        [Test]
        public void ViewModeControl_StartsFromTheSetting_OrTheRememberedView()
        {
            string saved = PlayerPrefs.GetString(ViewModeControl.LastViewPrefsKey, null);
            try
            {
                PlayerPrefs.DeleteKey(ViewModeControl.LastViewPrefsKey);
                var control = new ViewModeControl(new VisualElement());
                control.Configure(new ResultsSettings { default_view = "camera_highlight" }, true);
                Assert.AreEqual(ViewMode.CameraHighlight, control.Mode, "Default View");

                control.Configure(new ResultsSettings { default_view = "minimap" }, false);
                Assert.AreEqual(ViewMode.List, control.Mode, "the Map view without a minimap falls back to List");
                Assert.IsFalse(control.MapOffered, "no Map segment without a minimap");

                var remember = new ResultsSettings { default_view = "list", remember_last_view = true };
                control.Configure(remember, true);
                control.Choose(ViewMode.Minimap);
                var reopened = new ViewModeControl(new VisualElement());
                reopened.Configure(remember, true);
                Assert.AreEqual(ViewMode.Minimap, reopened.Mode, "Remember Last View: the device's last choice");

                var forget = new ResultsSettings { default_view = "list", remember_last_view = false };
                reopened.Configure(forget, true);
                Assert.AreEqual(ViewMode.List, reopened.Mode, "Remember Last View off: always Default View");
            }
            finally
            {
                if (saved == null) PlayerPrefs.DeleteKey(ViewModeControl.LastViewPrefsKey);
                else PlayerPrefs.SetString(ViewModeControl.LastViewPrefsKey, saved);
            }
        }

        // ---- the one result set ----

        private static WallConfigData FacetWall() => new()
        {
            category_styles = new List<CategoryStyleEntry> { new() { category = "religious" }, new() { category = "military" } },
            badge_categories = new List<BadgeCategoryEntry> { new() { key = "intact", label = "Intact" }, new() { key = "ruin", label = "Ruined" } },
            outline_levels = new List<OutlineLevelEntry> { new() { key = "low", label = "Low" } },
            hierarchy_levels = new List<HierarchyLevelEntry> { new() { key = "level_1", level_name = "Hub", priority = 1 } },
            pois = new List<POIData>
            {
                new() { id = "a", name = "Chapel One", category = "religious", badge_category = "intact", hierarchy_level_key = "level_1" },
                new() { id = "b", name = "Fort Two", category = "military", badge_category = "ruin", has_status = true, status_level_key = "low" },
                new() { id = "c", name = "Chapel Three", category = "religious", badge_category = "ruin" },
            },
        };

        private static POISearchIndex IndexOf(WallConfigData c)
        {
            var i = new POISearchIndex();
            i.Build(c);
            return i;
        }

        [Test]
        public void ResultSet_IsInactiveWithoutQueryOrFilter()
        {
            var c = FacetWall();
            var state = ResultSetCoordinator.Compute(IndexOf(c), c.pois, "  ", new FacetSelection(), new SelectFilterSearchSettings());
            Assert.IsFalse(state.Active);
            CollectionAssert.IsEmpty(state.Ids);
        }

        [Test]
        public void ResultSet_FiltersNarrowAndTheQueryRanksWithin()
        {
            var c = FacetWall();
            var facets = new FacetSelection();
            facets.Set(FacetGroup.Category, "religious", true);
            var s = new SelectFilterSearchSettings();
            var state = ResultSetCoordinator.Compute(IndexOf(c), c.pois, "", facets, s);
            CollectionAssert.AreEqual(new[] { "a", "c" }, state.Results.Select(r => r.PoiId).ToArray(), "a filter alone: its POIs");

            state = ResultSetCoordinator.Compute(IndexOf(c), c.pois, "three", facets, s);
            CollectionAssert.AreEqual(new[] { "c" }, state.Ids.ToArray(), "filter AND query");

            facets.Set(FacetGroup.Badge, "ruin", true);
            state = ResultSetCoordinator.Compute(IndexOf(c), c.pois, "", facets, s);
            CollectionAssert.AreEquivalent(new[] { "c" }, state.Ids, "groups AND: religious AND ruined");

            facets.Set(FacetGroup.Category, "military", true);
            state = ResultSetCoordinator.Compute(IndexOf(c), c.pois, "", facets, s);
            CollectionAssert.AreEquivalent(new[] { "b", "c" }, state.Ids, "within a group OR: (religious OR military) AND ruined");
        }

        [Test]
        public void ResultSet_NoMatch_ShowsTheMessage_AndTheBestFilterToDrop()
        {
            var c = FacetWall();
            var facets = new FacetSelection();
            facets.Set(FacetGroup.Category, "military", true);
            facets.Set(FacetGroup.Badge, "intact", true);   // military AND intact: none
            var s = new SelectFilterSearchSettings();
            s.search.no_results_message = "Nothing for {query}";
            s.search.no_results_filters_message = "These filters leave nothing";
            var state = ResultSetCoordinator.Compute(IndexOf(c), c.pois, "", facets, s);
            Assert.IsTrue(state.Active);
            CollectionAssert.IsEmpty(state.Ids, "an empty filter intersection shows nothing (it once showed every POI)");
            Assert.AreEqual("These filters leave nothing", state.EmptyMessage, "no text typed: the filters message (never 'Nothing for \"\"')");
            Assert.AreEqual("Nothing for fort", ResultSetCoordinator.Compute(IndexOf(c), c.pois, "fort", facets, s).EmptyMessage,
                "text typed: the query message");
            Assert.IsTrue(state.Relax.HasValue);
            Assert.AreEqual(FacetGroup.Category, state.Relax.Value.Group, "dropping 'military' leaves the most (a: intact)");
            Assert.AreEqual(1, state.Relax.Value.ResultCount);
            StringAssert.Contains("Remove filter: military (1 results)",
                ResultSetCoordinator.RelaxText(state.Relax.Value, FilterTrayView.BuildOptions(c, s.filter)));

            s.filter.relax_suggestion = false;
            Assert.IsFalse(ResultSetCoordinator.Compute(IndexOf(c), c.pois, "", facets, s).Relax.HasValue, "Relax Suggestion off");
        }

        [Test]
        public void StatusFacet_OnlyCountsPoisWithAStatus()
        {
            var c = FacetWall();
            c.pois[0].status_level_key = "low";   // a stale key without has_status
            var facets = new FacetSelection();
            facets.Set(FacetGroup.Status, "low", true);
            CollectionAssert.AreEquivalent(new[] { "b" }, FilterFacetEvaluator.CandidateIds(c.pois, facets));
        }

        // ---- the filter tray ----

        [Test]
        public void FilterTray_ShowsEnabledGroupsByLabel_AndTogglesFilters()
        {
            var c = FacetWall();
            var filter = new FilterSettings { status_facet = false };
            var groups = FilterTrayView.BuildOptions(c, filter);
            CollectionAssert.AreEqual(new[] { FacetGroup.Category, FacetGroup.Badge, FacetGroup.Hierarchy }, groups.Select(g => g.Group).ToArray(),
                "Status Filter off: no status group");
            Assert.AreEqual("Ruined", groups[1].Choices[1].label, "a badge chip reads the badge's label, not its key");
            Assert.AreEqual("Hub", groups[2].Choices[0].label, "a level chip reads the level's name");

            var tray = new FilterTrayView(new VisualElement());
            int changes = 0;
            tray.Changed += () => changes++;
            tray.Rebuild(groups);
            var chip = tray.Root.Q<Toggle>("facet-Badge-ruin");
            Assert.IsNotNull(chip, "one chip per row");
            // (a real chip tap needs a live panel: SearchSceneTests taps one)
            tray.SetFacet(FacetGroup.Badge, "ruin", true);
            Assert.IsTrue(tray.Selection.IsActive(FacetGroup.Badge, "ruin"));
            Assert.IsTrue(chip.value, "the chip shows the filter is on");
            Assert.AreEqual(1, changes);
            tray.SetFacet(FacetGroup.Badge, "ruin", true);
            Assert.AreEqual(1, changes, "setting a filter that is already on changes nothing");

            groups[1].Choices.RemoveAt(0);          // 'intact' removed from the wall; 'ruin' stays
            tray.Rebuild(groups);
            Assert.IsTrue(tray.Selection.IsActive(FacetGroup.Badge, "ruin"), "a rebuild keeps filters whose row still exists");
            tray.ClearAll();
            Assert.IsFalse(tray.Selection.Any);
            Assert.IsFalse(tray.Root.Q<Toggle>("facet-Badge-ruin").value, "Clear filters unticks the chips");
        }

        // A wall with a "Period" Keyword Field on two POIs (spelled differently on each)
        private static WallConfigData PeriodWall(bool filterable)
        {
            var c = FacetWall();
            c.search_fields = new List<SearchFieldDefinition> { new() { key = "period", label = "Period", filterable = filterable } };
            c.pois[0].search_keyword_fields = new List<POISearchKeywordField> { new() { field_key = "period", keywords = new List<string> { "Baroque", "Gothic" } } };
            c.pois[1].search_keyword_fields = new List<POISearchKeywordField> { new() { field_key = "period", keywords = new List<string> { " baroque " } } };
            return c;
        }

        [Test]
        public void AKeywordFieldMarkedFilter_BecomesAFilterGroup_OneChipPerKeyword()
        {
            Assert.IsFalse(FilterTrayView.BuildOptions(PeriodWall(false), new FilterSettings()).Any(g => g.Group.IsField),
                "Filter off: the field is searched only");

            var groups = FilterTrayView.BuildOptions(PeriodWall(true), new FilterSettings());
            var period = groups.Single(g => g.Group == FacetGroup.Field("period"));
            Assert.AreEqual("Period", period.Title, "the group reads the field's label");
            CollectionAssert.AreEqual(new[] { "baroque", "gothic" }, period.Choices.Select(ch => ch.key).ToArray(),
                "one chip per keyword, case and spaces ignored, A-Z");
            Assert.AreEqual("Baroque", period.Choices[0].label, "the chip reads the first spelling");
            Assert.AreEqual(groups.Count - 1, groups.IndexOf(period), "field groups come after the taxonomy groups");

            var tray = new FilterTrayView(new VisualElement());
            tray.Rebuild(groups);
            Assert.IsNotNull(tray.Root.Q<Toggle>("facet-Field_period-baroque"), "a real chip for it");
        }

        [Test]
        public void AKeywordFieldFilter_PassesAnyPoiHoldingATickedKeyword_AndCombinesLikeTheOthers()
        {
            var c = PeriodWall(true);
            var s = new SelectFilterSearchSettings();
            var facets = new FacetSelection();
            facets.Set(FacetGroup.Field("period"), "baroque", true);
            CollectionAssert.AreEquivalent(new[] { "a", "b" }, ResultSetCoordinator.Compute(IndexOf(c), c.pois, "", facets, s).Ids,
                "both spellings of baroque pass; c holds no period");

            facets.Set(FacetGroup.Field("period"), "gothic", true);
            CollectionAssert.AreEquivalent(new[] { "a", "b" }, FilterFacetEvaluator.CandidateIds(c.pois, facets), "OR within the field");

            facets.Set(FacetGroup.Category, "military", true);
            CollectionAssert.AreEquivalent(new[] { "b" }, FilterFacetEvaluator.CandidateIds(c.pois, facets), "AND with the category group");

            facets.Set(FacetGroup.Field("period"), "baroque", false);
            var state = ResultSetCoordinator.Compute(IndexOf(c), c.pois, "", facets, s);
            CollectionAssert.IsEmpty(state.Ids, "military AND gothic: nothing");
            Assert.IsTrue(state.Relax.HasValue, "the relax suggestion covers field groups too");
            StringAssert.Contains("Remove filter: Gothic", ResultSetCoordinator.RelaxText(state.Relax.Value, FilterTrayView.BuildOptions(c, s.filter)));
        }

        // ---- results list, card, search bar ----

        [Test]
        public void ResultsList_ShowsRows_OrTheMessageAndRelaxButton_AndARowTapSelects()
        {
            var list = new ResultsListView(new VisualElement());
            try
            {
                list.ShowRows(new List<ResultsListView.Row> { new() { PoiId = "a", Name = "Chapel One" } }, "", null, null);
                Assert.AreEqual(1, list.Rows.Count);
                Assert.IsFalse(list.EmptyShown);

                // (the relax button's real click needs a live panel: SearchSceneTests clicks it)
                list.ShowRows(new List<ResultsListView.Row>(), "No matches", "Remove filter: X", () => { });
                Assert.IsTrue(list.EmptyShown);
                Assert.AreEqual("No matches", list.EmptyMessage);
                Assert.AreEqual("Remove filter: X", list.RelaxText);
                list.ShowRows(new List<ResultsListView.Row>(), "No matches", null, null);
                Assert.IsNull(list.RelaxText, "no suggestion: no button");

                list.TapRow("a");
                Assert.AreEqual("a", SelectionEventBus.CurrentPoiId, "a row tap selects its POI through the bus");
            }
            finally { list.Dispose(); }
        }

        [Test]
        public void DetailCard_ShowsTheSelectedPoi_AndItsXClears()
        {
            var c = FacetWall();
            var card = new DetailCardView(new VisualElement());
            try
            {
                card.Configure(id => c.pois.Find(p => p.id == id), c);
                SelectionEventBus.Select("a");
                Assert.AreEqual("Chapel One", card.NameText);
                Assert.AreEqual("religious - Hub", card.SubtitleText);
                SelectionEventBus.Clear();
                Assert.IsNull(SelectionEventBus.CurrentPoiId);
            }
            finally { card.Dispose(); }
        }

        [Test]
        public void SearchBar_FollowsItsSettings()
        {
            var bar = new SearchOverlayView(new VisualElement());
            bar.Configure(new SearchSettings { mode = "explicit" }, false);
            Assert.IsTrue(bar.IsDelayed, "On Enter: the field reports on Enter only");
            Assert.IsFalse(bar.MicShown, "no voice backend: no mic");
            bar.Configure(new SearchSettings { mode = "dynamic" }, true);
            Assert.IsFalse(bar.IsDelayed);
            Assert.IsTrue(bar.MicShown);
            bar.SetFilterCount(2);
            Assert.AreEqual("Filters (2)", bar.FiltersText);

            string query = null;
            bar.QueryChanged += q => query = q;
            bar.SetQuery("  tower ");
            Assert.AreEqual("tower", query, "a set query is searched at once, trimmed");

            bar.SetSuggestions(new List<string> { "religious" });
            Assert.IsNotNull(bar.Suggestions.Q<Button>("suggestion-religious"), "one button per suggestion");
            CollectionAssert.AreEqual(new[] { "religious" }, bar.SuggestionTerms);
        }

        // ---- voice ----

        [Test]
        public void Voice_SearchesTheTranscript_AndNeedsABackend()
        {
            Assert.IsInstanceOf<DebugTranscriber>(TranscriberFactory.Create(true), "the Editor gets the debug backend");
            Assert.IsNull(TranscriberFactory.Create(false), "a device without a real backend gets none (no fake mic)");

            string searched = null;
            var debug = new DebugTranscriber { PresetTranscript = "chapel" };
            var voice = new VoiceSearchController(debug, q => searched = q);
            Assert.IsTrue(voice.IsAvailable);
            voice.StartVoiceSearch();
            Assert.AreEqual("chapel", searched);
            Assert.AreEqual(VoiceSearchState.Result, voice.State);

            searched = null;
            debug.PresetTranscript = "   ";
            voice.StartVoiceSearch();
            Assert.IsNull(searched, "nothing heard: no search");
            Assert.AreEqual(VoiceSearchState.Idle, voice.State);

            Assert.IsFalse(new VoiceSearchController(null, _ => { }).IsAvailable, "no backend: unavailable");
            voice.Dispose();
        }

        // ---- minimap ----

        [Test]
        public void MinimapLayout_ProjectionAndBounds()
        {
            var wall = new List<Vector3> { new(0, 0, 0), new(4, 2, 0.1f), new(2, 1, 0) };
            Assert.IsFalse(MinimapLayout.ResolveFloor(wall, "auto"), "points spread in x/y: the wall view");
            var room = new List<Vector3> { new(0, 0, 0), new(4, 0.3f, 3), new(2, 0.1f, -2) };
            Assert.IsTrue(MinimapLayout.ResolveFloor(room, "auto"), "points spread in x/z: the floor plan");
            Assert.IsTrue(MinimapLayout.ResolveFloor(wall, "floor"));
            Assert.IsFalse(MinimapLayout.ResolveFloor(room, "wall"));

            var layout = MinimapLayout.Compute(wall, new MinimapSettings { projection = "wall" });
            Vector2 lo = layout.Normalize(new Vector3(0, 0, 0)), hi = layout.Normalize(new Vector3(4, 2, 0));
            Assert.Less(lo.x, 0.2f); Assert.Less(lo.y, 0.2f);
            Assert.Greater(hi.x, 0.8f); Assert.Greater(hi.y, 0.8f);
            Assert.Greater(lo.x, 0f, "the auto margin keeps the dot off the border");

            var manual = MinimapLayout.Compute(wall, new MinimapSettings
            { projection = "wall", bounds_mode = "manual", bounds_min = new Vector3(0, 0, 0), bounds_max = new Vector3(8, 4, 0) });
            Assert.AreEqual(new Vector2(0.5f, 0.5f), manual.Normalize(new Vector3(4, 2, 0)), "manual bounds: the box between min and max");

            var single = MinimapLayout.Compute(new List<Vector3> { new(1, 1, 1) }, new MinimapSettings());
            Assert.AreEqual(new Vector2(0.5f, 0.5f), single.Normalize(new Vector3(1, 1, 1)), "one POI: centred, never a zero-size map");
        }

        [Test]
        public void ShippedWall_IsARoom_SoAutoDrawsTheFloorPlan()
        {
            var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText(Path.Combine(Application.dataPath, "Apps/LivingRoom/config.json")));
            var positions = config.pois.Where(p => p.position != null).Select(p => p.position.ToVector3()).ToList();
            Assert.IsTrue(MinimapLayout.ResolveFloor(positions, "auto"));
            var layout = MinimapLayout.Compute(positions, config.select_filter_search.minimap);
            var spots = new HashSet<Vector2>(positions.Select(p => layout.Normalize(p)));
            Assert.Greater(spots.Count, 10, "the dots are spread over the map (the old all-zero bounds stacked them in one corner)");
        }

        [Test]
        public void MinimapView_PlacesFiltersSelectsAndTaps()
        {
            SelectionEventBus.ResetState();
            var pois = new List<(POIData, Vector3)>
            {
                (new POIData { id = "left", name = "Left", category = "religious" }, new Vector3(0, 0, 0)),
                (new POIData { id = "right", name = "Right", category = "military" }, new Vector3(4, 2, 0)),
                (new POIData { id = "", name = "No id" }, new Vector3(2, 1, 0)),
            };
            var map = new MinimapView(new VisualElement());
            try
            {
                map.Rebuild(pois, new MinimapSettings { projection = "wall", icon_style = "dots_only" }, null);
                Assert.AreEqual(2, map.Positions.Count, "one dot per POI with an id");
                Assert.AreEqual(StyleKeyword.Null, map.DotOf("left").style.backgroundColor.keyword,
                    "Plain dots: no per-POI colour, the stylesheet's one dot colour applies");

                var hit = map.DotOf("right").parent;
                Assert.AreEqual(LengthUnit.Percent, hit.style.left.value.unit, "dots are placed by percent (resolution independent)");

                map.SetVisibleIds(new HashSet<string> { "left" });
                Assert.IsTrue(map.IsDotShown("left"));
                Assert.IsFalse(map.IsDotShown("right"), "outside the result set: no dot");

                map.TapAt(new Vector2(0.9f, 0.9f));
                Assert.AreEqual("left", SelectionEventBus.CurrentPoiId, "a tap selects the nearest SHOWN dot");
                Assert.AreEqual(MinimapView.SelectedScale, map.DotOf("left").style.scale.value.value.x, 1e-5, "the selected dot grows");

                map.SetVisibleIds(null);
                SelectionEventBus.Clear();
                map.TapAt(new Vector2(0.9f, 0.9f));
                Assert.AreEqual("right", SelectionEventBus.CurrentPoiId);
                Assert.AreEqual(MinimapView.FadedOpacity, map.DotOf("left").style.opacity.value, 1e-5, "the others fade");

                map.Rebuild(pois, new MinimapSettings { projection = "wall", icon_style = "category_colored_dots" }, null);
                Assert.AreEqual(CategoryPalette.ResolveColor("military"), map.DotOf("right").style.backgroundColor.value,
                    "Category colours: the category's colour");
            }
            finally { map.Dispose(); }
        }

        // ---- the search demo's layout ----

        // The index WallSession builds while the demo is on: the wall's taxonomy and vocabulary, the demo's POIs,
        // plus the demo's own vocabulary (Material field, synonym group)
        private static (List<SearchDemoLayout.Entry> entries, WallConfigData searchConfig) DemoSearchConfig(SearchDemoSettings settings)
        {
            var wall = JsonUtility.FromJson<WallConfigData>(File.ReadAllText(Path.Combine(Application.dataPath, "Apps/LivingRoom/config.json")));
            var entries = SearchDemoLayout.Build(settings, wall);
            var fields = new List<SearchFieldDefinition>(wall.search_fields ?? new List<SearchFieldDefinition>());
            var synonyms = new List<SynonymGroup>(wall.synonym_groups ?? new List<SynonymGroup>());
            SearchDemoLayout.AddDemoVocabulary(settings, fields, synonyms);
            return (entries, new WallConfigData
            {
                category_styles = wall.category_styles, badge_categories = wall.badge_categories,
                outline_levels = wall.outline_levels, hierarchy_levels = wall.hierarchy_levels,
                search_fields = fields, synonym_groups = synonyms, pois = entries.Select(e => e.Poi).ToList(),
            });
        }

        [Test]
        public void SearchDemoLayout_CoversEveryFilterValue_AndHasEveryTestCasesPoi()
        {
            var settings = new SearchDemoSettings { markers_per_category = 2, test_cases = true };
            var (entries, searchConfig) = DemoSearchConfig(settings);
            int categories = searchConfig.category_styles.Count;
            Assert.AreEqual(categories * 2 + SearchDemoLayout.TestPoiCount + SearchDemoLayout.ClumpCount, entries.Count);

            var pois = searchConfig.pois;
            foreach (var c in searchConfig.category_styles) Assert.IsTrue(pois.Any(p => p.category == c.category), "category " + c.category);
            foreach (var l in searchConfig.hierarchy_levels) Assert.IsTrue(pois.Any(p => p.hierarchy_level_key == l.key), "level " + l.key);
            foreach (var b in searchConfig.badge_categories) Assert.IsTrue(pois.Any(p => p.badge_category == b.key), "badge " + b.key);
            foreach (var o in searchConfig.outline_levels) Assert.IsTrue(pois.Any(p => p.status_level_key == o.key), "outline " + o.key);
            Assert.AreEqual(pois.Count, pois.Select(p => p.id).Distinct().Count(), "unique ids");
            foreach (var tc in SearchDemoLayout.TestCases())
                Assert.AreEqual(tc.PoiName, pois.Single(p => p.id == tc.PoiId).name, "test case '" + tc.Query + "' has its POI");

            var material = FilterTrayView.BuildOptions(searchConfig, new FilterSettings()).Single(g => g.Group == FacetGroup.Field(SearchDemoLayout.MaterialFieldKey));
            CollectionAssert.AreEquivalent(SearchDemoLayout.Materials, material.Choices.Select(ch => ch.key), "a Material filter chip per material");
        }

        [Test]
        public void DemoTestCases_EveryVerdictIsRight_ForEveryCombinationOfTheSearchSettings()
        {
            var (_, searchConfig) = DemoSearchConfig(new SearchDemoSettings { markers_per_category = 3, test_cases = true });
            var index = IndexOf(searchConfig);
            int combinations = 0, expectedNotFound = 0;
            foreach (string match in SelectFilterSearchOptions.MatchModes)
                foreach (string prefix in SelectFilterSearchOptions.PrefixModes)
                    for (int typos = 0; typos <= SearchSettings.TypoToleranceMax; typos++)
                    {
                        var s = new SearchSettings { match_mode = match, prefix_matching = prefix, typo_tolerance = typos };
                        var verdicts = SearchDemoCheck.Evaluate(index, s);
                        Assert.AreEqual(SearchDemoLayout.TestCases().Count, verdicts.Count);
                        foreach (var v in verdicts)
                        {
                            Assert.IsTrue(v.Ok, $"{match}/{prefix}/typos {typos}: {SearchDemoCheck.Line(v)}");
                            if (!v.Expected) expectedNotFound++;
                        }
                        combinations++;
                    }
            Assert.AreEqual(18, combinations);
            Assert.Greater(expectedNotFound, 0, "precondition: some settings really switch a case off (the check is not vacuous)");

            var line = SearchDemoCheck.Line(SearchDemoCheck.Evaluate(index, new SearchSettings { typo_tolerance = 0 })
                .Single(v => v.Case.Query == "lantren"));
            Assert.AreEqual("[ok] 'lantren' does not find Lantern Tower -- one typo (two letters swapped) (Typo Tolerance)", line);
        }

        [Test]
        public void SearchDemoLayout_EachControlChangesThePlacement()
        {
            var wall = new WallConfigData { category_styles = new List<CategoryStyleEntry> { new() { category = "a" }, new() { category = "b" } } };
            var near = SearchDemoLayout.Build(new SearchDemoSettings { distance_m = 1f, test_cases = false }, wall);
            var far = SearchDemoLayout.Build(new SearchDemoSettings { distance_m = 4f, test_cases = false }, wall);
            Assert.AreEqual(1f, near[0].LocalPosition.z, 1e-5, "Distance");
            Assert.AreEqual(4f, far[0].LocalPosition.z, 1e-5);

            var tight = SearchDemoLayout.Build(new SearchDemoSettings { spacing_cm = 10f, test_cases = false }, wall);
            var wide = SearchDemoLayout.Build(new SearchDemoSettings { spacing_cm = 40f, test_cases = false }, wall);
            Assert.AreEqual(0.1f, Vector3.Distance(tight[0].LocalPosition, tight[1].LocalPosition), 1e-4, "Spacing");
            Assert.AreEqual(0.4f, Vector3.Distance(wide[0].LocalPosition, wide[1].LocalPosition), 1e-4);

            Assert.AreEqual(2 * SearchDemoSettings.MaxMarkersPerCategory,
                SearchDemoLayout.Build(new SearchDemoSettings { markers_per_category = 99, test_cases = false }, wall).Count, "clamped");
            Assert.IsFalse(SearchDemoLayout.Build(new SearchDemoSettings { test_cases = false }, wall)
                    .Any(e => e.Poi.id == SearchDemoLayout.AccentId || e.Poi.id.StartsWith(SearchDemoLayout.ClumpIdPrefix)),
                "Test Cases off: no test POIs, no clump");
        }

        [Test]
        public void SearchDemoSpawner_Rules()
        {
            Assert.IsTrue(SearchDemoSpawner.IsAllowed(true, false), "Editor");
            Assert.IsTrue(SearchDemoSpawner.IsAllowed(false, true), "development build");
            Assert.IsFalse(SearchDemoSpawner.IsAllowed(false, false), "release build ignores it");
            Assert.IsTrue(SearchDemoSpawner.PausesLod(new SearchDemoSettings { run_lod = false }));
            Assert.IsFalse(SearchDemoSpawner.PausesLod(new SearchDemoSettings { run_lod = true }));
        }
    }
}
