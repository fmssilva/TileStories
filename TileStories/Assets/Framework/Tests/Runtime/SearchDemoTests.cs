using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // The dev-only search demo ("Add search demo", SearchDemoSettings) in the REAL wall scene, driven through
    // WallSession.ApplySearchDemo -- the seam the Editor's live push calls -- and searched through the real
    // search UI. Each demo control is changed alone and what really changed on the real markers is asserted;
    // every test case is found by its query; zoom-on-select runs on the real clump and a real cluster tap.
    public class SearchDemoTests : SearchSceneFixture
    {
        private SearchDemoSettings Demo(System.Action<SearchDemoSettings> edit = null)
        {
            var s = new SearchDemoSettings { enabled = true, markers_per_category = 2, test_cases = true, show_labels = true, distance_m = 3.5f, spacing_cm = 40f };
            edit?.Invoke(s);
            return s;
        }

        private IEnumerator TurnOn(SearchDemoSettings settings)
        {
            Session.ApplySearchDemo(settings);
            yield return Wait(1.2f);   // reveals finish
        }

        private int Categories => ConfigCopy().category_styles.Count;

        private List<MarkerView> DemoMarkers() => Session.SpawnedMarkers.Where(m => m != null && m.PoiId.StartsWith(SearchDemoLayout.IdPrefix)).ToList();

        [UnityTest, CoversField("search_demo.enabled")]
        public IEnumerator TheDemo_TakesOverTheWall_OnItsOwnStage_ThenGivesItBack()
        {
            var wallCamera = new Pose(Cam.transform.position, Cam.transform.rotation);
            int wallPois = Session.SearchPois.Count;
            yield return TurnOn(Demo());

            Assert.IsNotNull(Session.SearchDemoRoot);
            Assert.AreEqual(DemoFieldStage.SearchDemoStagePosition, Session.SearchDemoRoot.transform.position, "its own stage");
            Assert.AreEqual(DemoFieldStage.SearchDemoStagePosition, Cam.transform.position, "the camera moved there");
            int expected = Categories * 2 + SearchDemoLayout.TestPoiCount + SearchDemoLayout.ClumpCount;
            Assert.AreEqual(expected, DemoMarkers().Count);
            Assert.AreEqual(expected, Session.SpawnedMarkers.Count, "only the demo's markers run");
            Assert.AreEqual(expected, Session.SearchPois.Count, "the search covers the demo's POIs");
            Assert.AreEqual(expected, Host.Minimap.Positions.Count, "the map shows the demo");
            Assert.IsFalse(Session.LodSettings.enabled, "Run LOD off: LOD pauses on the demo");
            yield return Capture("Demo_On");

            Session.ApplySearchDemo(new SearchDemoSettings { enabled = false });
            yield return Wait(0.5f);
            Assert.IsNull(Session.SearchDemoRoot);
            Assert.AreEqual(wallCamera.position, Cam.transform.position, "the camera went back");
            Assert.AreEqual(wallPois, Session.SearchPois.Count, "the wall's own POIs are searched again");
        }

        [UnityTest]
        public IEnumerator EveryTestCase_IsFoundByItsQuery_ThroughTheRealSearchUi()
        {
            yield return TurnOn(Demo());
            var settings = ConfigCopy().select_filter_search.search;
            var verdicts = SearchDemoCheck.Evaluate(Session.SearchIndex, settings);
            Assert.AreEqual(SearchDemoLayout.TestCases().Count, verdicts.Count);
            foreach (var v in verdicts)
            {
                Assert.IsTrue(v.Ok, "the readout's verdict on the running index: " + SearchDemoCheck.Line(v));
                // - and what the visitor sees agrees: typed through the real search UI
                Host.SetQuery(v.Case.Query);
                Assert.AreEqual(v.Expected, Host.State.Ids.Contains(v.Case.PoiId), $"'{v.Case.Query}' through the UI ({v.Case.Note})");
                if (!v.Expected) continue;
                Assert.AreEqual(1f, Marker(v.Case.PoiId).SelectionAlpha, 1e-3, "its marker stays shown");
                Assert.IsTrue(Host.List.Rows.Any(r => r.PoiId == v.Case.PoiId), "its row is listed");
            }
            Host.SetQuery("crowd");
            Assert.AreEqual(SearchDemoLayout.ClumpCount, Host.State.Ids.Count, "'crowd' finds the clump");
            yield return Capture("Demo_QueryCrowd");

            Host.SetQuery("lantren");
            yield return Capture("Demo_Typo");
            yield return ApplySearch(s => s.search.typo_tolerance = 0);
            Host.SetQuery("lantren");
            Assert.IsEmpty(Host.State.Ids, "Typo Tolerance 0: the typo case finds nothing");
            var flipped = SearchDemoCheck.Evaluate(Session.SearchIndex, ConfigCopy().select_filter_search.search).Single(v => v.Case.Query == "lantren");
            Assert.IsTrue(flipped.Ok && !flipped.Found, "the readout's line flips with the setting: " + SearchDemoCheck.Line(flipped));
        }

        [UnityTest]
        public IEnumerator TheDemoMaterialField_IsAFilterGroup_ThatFiltersTheDemo()
        {
            yield return TurnOn(Demo());
            Press(Host.Root.Q<Button>("search-filters"));
            var chip = Host.Tray.Root.Q<Toggle>($"facet-Field_{SearchDemoLayout.MaterialFieldKey}-marble");
            Assert.IsNotNull(chip, "the demo's Material keyword field is a filter group with a 'marble' chip");
            Press(chip);
            yield return null;
            var marble = Session.SearchPois.Where(p => p.search_keyword_fields.Any(f => f.field_key == SearchDemoLayout.MaterialFieldKey && f.keywords.Contains("marble")))
                .Select(p => p.id).ToList();
            Assert.Greater(marble.Count, 0, "precondition: demo POIs hold marble");
            CollectionAssert.AreEquivalent(marble, Host.State.Ids, "only the marble POIs");
            CollectionAssert.AreEquivalent(marble, MarkersAt(1f), "and only their markers stay shown");
            Press(Host.Root.Q<Button>("search-filters"));   // close the tray: it covers the view
            yield return Wait(0.2f);
            yield return Capture("Demo_FilterMaterial");
        }

        [UnityTest]
        [CoversField("search_demo.markers_per_category"), CoversField("search_demo.test_cases"), CoversField("search_demo.show_labels")]
        [CoversField("search_demo.distance_m"), CoversField("search_demo.spacing_cm"), CoversField("search_demo.run_lod")]
        public IEnumerator EachDemoControl_ChangesTheDemo()
        {
            yield return TurnOn(Demo(s => s.markers_per_category = 1));
            int oneEach = DemoMarkers().Count;
            yield return TurnOn(Demo(s => s.markers_per_category = 3));
            Assert.AreEqual(oneEach + 2 * Categories, DemoMarkers().Count, "Markers per Category");

            yield return TurnOn(Demo(s => s.test_cases = false));
            Assert.AreEqual(2 * Categories, DemoMarkers().Count, "Test Cases off: only the generated columns");
            Host.SetQuery("church");
            Assert.IsEmpty(Host.State.Ids, "Test Cases off: no demo synonym group ('church' finds nothing)");

            yield return TurnOn(Demo(s => s.show_labels = false));
            Assert.IsTrue(DemoMarkers().All(m => !LabelShown(m)), "Show labels off: no demo label");
            yield return TurnOn(Demo(s => s.show_labels = true));
            Assert.IsTrue(DemoMarkers().All(LabelShown), "Show labels on: every demo label");

            yield return TurnOn(Demo(s => s.distance_m = 1.5f));
            float near = DemoMarkers()[0].transform.localPosition.z;
            yield return TurnOn(Demo(s => s.distance_m = 4f));
            Assert.AreEqual(4f - 1.5f, DemoMarkers()[0].transform.localPosition.z - near, 1e-3, "Distance (m)");

            yield return TurnOn(Demo(s => { s.spacing_cm = 10f; s.test_cases = false; }));
            float tight = Vector3.Distance(DemoMarkers()[0].transform.localPosition, DemoMarkers()[1].transform.localPosition);
            yield return TurnOn(Demo(s => { s.spacing_cm = 40f; s.test_cases = false; }));
            float wide = Vector3.Distance(DemoMarkers()[0].transform.localPosition, DemoMarkers()[1].transform.localPosition);
            Assert.AreEqual(0.1f, tight, 1e-3, "Spacing (cm)");
            Assert.AreEqual(0.4f, wide, 1e-3);

            yield return TurnOn(Demo(s => s.run_lod = true));
            Session.ApplyLodSettings(new LodSettings());   // the wall's LOD on
            Assert.IsTrue(Session.LodSettings.enabled, "Run LOD on the demo: LOD runs on it");
        }

        private static bool LabelShown(MarkerView m) => m.ShowsLabel;

        [UnityTest]
        public IEnumerator FilterChipsOnTheDemo_FilterByTheWallsOwnTaxonomy()
        {
            yield return TurnOn(Demo());
            var cfg = ConfigCopy();
            string category = cfg.category_styles[0].category;
            Press(Host.Root.Q<Button>("search-filters"));
            Press(Host.Tray.Root.Q<Toggle>("facet-Category-" + category));
            yield return null;
            var expected = Session.SearchPois.Where(p => p.category == category).Select(p => p.id);
            CollectionAssert.AreEquivalent(expected, Host.State.Ids, "every demo POI of that category, and nothing else");
            CollectionAssert.AreEquivalent(expected, MarkersAt(1f));
            yield return Capture("Demo_FilterCategory");

            string level = cfg.hierarchy_levels.OrderBy(l => l.priority).First().key;
            Press(Host.Tray.Root.Q<Toggle>("facet-Hierarchy-" + level));
            yield return null;
            CollectionAssert.AreEquivalent(Session.SearchPois.Where(p => p.category == category && p.hierarchy_level_key == level).Select(p => p.id),
                Host.State.Ids, "category AND level");
        }

        [UnityTest]
        public IEnumerator ZoomOnSelect_ZoomsOnTheClump_NotOnALonePoint()
        {
            yield return TurnOn(Demo());
            // - a radius well under the demo's spacing: only the clump is a crowd
            yield return ApplySearch(s => { s.selection.zoom.enabled = true; s.selection.zoom.trigger = "marker"; s.selection.zoom.min_neighbours = 2;
                s.selection.zoom.factor = 2f; s.selection.zoom.neighbour_radius_px = 30f; });
            ARZoomState.SetZoom(1f, 1f, 4f);

            SelectionEventBus.Select(SearchDemoLayout.IdPrefix + "accent");   // a lone test case
            yield return Wait(0.6f);
            Assert.AreEqual(1f, ARZoomState.ZoomFactor, 1e-3, "a lone point never zooms");
            Assert.Less(Session.ZoomOnSelect.LastNeighbourCount, 2);
            SelectionEventBus.Clear();

            SelectionEventBus.Select(SearchDemoLayout.IdPrefix + "clump_0");
            yield return Wait(1f);
            Assert.GreaterOrEqual(Session.ZoomOnSelect.LastNeighbourCount, SearchDemoLayout.ClumpCount - 1, "the clump counts its members");
            Assert.AreEqual(2f, ARZoomState.ZoomFactor, 0.05, "a crowded point zooms by Zoom Factor");
            yield return Capture("Demo_ZoomOnSelect");
            SelectionEventBus.Clear();

            ARZoomState.SetZoom(1f, 1f, 4f);
            yield return ApplySearch(s => s.selection.zoom.min_neighbours = 10);
            SelectionEventBus.Select(SearchDemoLayout.IdPrefix + "clump_1");
            yield return Wait(0.6f);
            Assert.AreEqual(1f, ARZoomState.ZoomFactor, 1e-3, "Min Neighbours above the clump: no zoom");
        }

        [UnityTest]
        public IEnumerator ARealTapOnACluster_ZoomsIn_WhenTheTriggerAllowsClusters()
        {
            yield return TurnOn(Demo(s => s.run_lod = true));
            // LOD on, hybrid: a marker with 3+ neighbours clusters, so the 5-point clump becomes one cluster
            Session.ApplyLodSettings(new LodSettings { cluster_min_count = 3 });
            yield return ApplySearch(s => { s.selection.zoom.enabled = true; s.selection.zoom.trigger = "both"; s.selection.zoom.factor = 2f; });
            ARZoomState.SetZoom(1f, 1f, 4f);
            MarkerClusterView cluster = null;
            for (int i = 0; i < 200 && cluster == null; i++)
            {
                cluster = Object.FindObjectsByType<MarkerClusterView>(FindObjectsSortMode.None)
                    .FirstOrDefault(c => c.isActiveAndEnabled && c.MemberPoiIds.Any(id => id.Contains("clump")));
                yield return null;
            }
            Assert.IsNotNull(cluster, "precondition: LOD clusters the demo's clump");
            Assert.IsNotNull(cluster.GetComponent<MarkerClusterSelectable>(), "the cluster prefab carries its tap target");
            yield return Wait(0.5f);
            yield return Capture("Demo_Cluster");

            // the members merged into it are hidden -- and no longer take taps (they did during their fade-out)
            Assert.IsTrue(DemoMarkers().Where(m => cluster.MemberPoiIds.Contains(m.PoiId)).All(m => !m.IsVisible),
                "precondition: LOD hides the clustered members");
            Vector3 sp = Cam.WorldToScreenPoint(cluster.transform.position);
            var hit = TapScreen(new Vector2(sp.x, sp.y));
            Assert.IsNotNull(hit, "the tap hit something");
            var handler = UnityEngine.EventSystems.ExecuteEvents.GetEventHandler<UnityEngine.EventSystems.IPointerClickHandler>(hit);
            Assert.IsNotNull(handler != null ? handler.GetComponent<MarkerClusterSelectable>() : null,
                "the topmost tap target at the cluster is the cluster itself, not '" + (handler != null ? handler.name : hit.name) + "'");
            yield return Wait(1f);
            Assert.AreEqual(2f, ARZoomState.ZoomFactor, 0.05, "Zoom Trigger Marker or Cluster: a cluster tap zooms in");
            Assert.IsNull(SelectionEventBus.CurrentPoiId, "a cluster is never the selection");

            ARZoomState.SetZoom(1f, 1f, 4f);
            yield return ApplySearch(s => s.selection.zoom.trigger = "marker");
            yield return Wait(0.3f);
            var again = Object.FindObjectsByType<MarkerClusterView>(FindObjectsSortMode.None).FirstOrDefault(c => c.isActiveAndEnabled);
            if (again != null)
            {
                sp = Cam.WorldToScreenPoint(again.transform.position);
                TapScreen(new Vector2(sp.x, sp.y));
                yield return Wait(0.6f);
                Assert.AreEqual(1f, ARZoomState.ZoomFactor, 1e-3, "Zoom Trigger Marker: a cluster tap does not zoom");
            }
        }
    }
}
