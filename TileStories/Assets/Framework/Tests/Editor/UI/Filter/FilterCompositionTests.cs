using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // THE 2.6-i regression suite (spec _2.6 sections 5+7; 40-testing 4.2.1):
    // a facet toggle must produce ONE result set pushed to every surface -- results
    // list, minimap dots, and the search index candidates. Written FIRST for Block 4
    // because its absence is exactly what let the original gap ship green. Real views
    // + real index, fabricated config; scene-marker alpha is asserted in PlayMode
    // Phase B instead (CanvasGroup fades are coroutine-driven, don't tick in EditMode).
    public class FilterCompositionTests
    {
        private const string PanelAssetPath =
            "Assets/Framework/Runtime/UI/Shared/PanelSettings.asset";

        private WallConfigData _config;
        private POISearchIndex _index;
        private GameObject _docGO;
        private ResultSetCoordinator _coordinator;

        [SetUp]
        public void SetUp()
        {
            CategoryPalette.ClearOverrides();
            MarkerHierarchyResolver.ResetToDefaults();

            _config = new WallConfigData
            {
                wall_id = "comp_wall",
                wall_name = "Composition Test Wall",
                category_styles = new List<CategoryStyleEntry>
                {
                    new CategoryStyleEntry { category = "religious", color_hex = "#FF0000" },
                    new CategoryStyleEntry { category = "civic", color_hex = "#00FF00" },
                },
                hierarchy_levels = new List<HierarchyLevelEntry>
                {
                    new HierarchyLevelEntry { key = "hero", size_cm = 20f },
                    new HierarchyLevelEntry { key = "satellite", size_cm = 8f },
                },
                pois = new List<POIData>
                {
                    new POIData { id = "p_relig_1", name = "Religious One", category = "religious", hierarchy_level_key = "hero", x_norm = 0.1f, y_norm = 0.9f },
                    new POIData { id = "p_relig_2", name = "Religious Two", category = "religious", hierarchy_level_key = "satellite", x_norm = 0.3f, y_norm = 0.7f },
                    new POIData { id = "p_civic_1", name = "Civic Hall", category = "civic", hierarchy_level_key = "hero", x_norm = 0.6f, y_norm = 0.4f },
                    new POIData { id = "p_civic_2", name = "Civic Tower", category = "civic", hierarchy_level_key = "satellite", x_norm = 0.8f, y_norm = 0.2f },
                },
            };

            _index = new POISearchIndex();
            _index.Build(_config);
            CategoryPalette.Configure(_config.category_styles);

            var panel = UnityEditor.AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelAssetPath);
            Assert.IsNotNull(panel, "PanelSettings.asset missing at " + PanelAssetPath);

            // One shared UIDocument for all three surfaces, exactly like runtime.
            _docGO = new GameObject("filter-comp-doc");
            var doc = _docGO.AddComponent<UIDocument>();
            doc.panelSettings = panel;

            var tray = _docGO.AddComponent<FilterTrayView>();
            tray.Initialize(_config);
            var results = _docGO.AddComponent<ResultsListView>();
            results.Initialize(_config, _index);
            var minimap = _docGO.AddComponent<MinimapView>();
            minimap.Initialize(_config, _index);

            // No spawned markers in EditMode: their alpha behaviour is Phase-B (PlayMode).
            _coordinator = new ResultSetCoordinator(_index, _config, tray, results, minimap,
                () => new List<MarkerView>());
        }

        [TearDown]
        public void TearDown()
        {
            _coordinator?.Dispose();
            if (_docGO != null)
                UnityEngine.Object.DestroyImmediate(_docGO);
            CategoryPalette.ClearOverrides();
            MarkerHierarchyResolver.ResetToDefaults();
        }

        private List<ResultsListView.ResultRow> CurrentRows()
        {
            var listView = _docGO.GetComponent<UIDocument>()
                .rootVisualElement.Q<ListView>("results-list-view");
            Assert.IsNotNull(listView, "results-list-view missing from built UI");
            return listView.itemsSource as List<ResultsListView.ResultRow>;
        }

        private void ToggleFacet(string key, bool value)
        {
            var toggle = _docGO.GetComponent<UIDocument>()
                .rootVisualElement.Q<Toggle>("facet-toggle-" + key);
            Assert.IsNotNull(toggle, "facet toggle '" + key + "' missing -- tray did not build sections");
            toggle.value = value; // fires the real RegisterValueChangedCallback path
        }

        [Test]
        public void EmptyQuery_WithActiveFilter_ReturnsFilteredSet()
        {
            // The most common real usage: visitor opens tray, taps Religious, no typing.
            ToggleFacet("religious", true);

            var rows = CurrentRows();
            Assert.IsNotNull(rows);
            Assert.AreEqual(2, rows.Count, "empty query + active facet must show the whole filtered set");
            foreach (var row in rows)
                StringAssert.StartsWith("p_relig_", row.poiId,
                    "filtered result set must contain only facet-matching POIs");
        }

        [Test]
        public void ToggledFacet_ChangesResultSet_AcrossListAndMinimap()
        {
            Assert.AreEqual(0, CurrentRows().Count); // baseline: unfiltered empty query -> empty list

            ToggleFacet("civic", true);

            var rows = CurrentRows();
            Assert.AreEqual(2, rows.Count);
            foreach (var row in rows)
                StringAssert.StartsWith("p_civic_", row.poiId);

            // Same single result set drives the minimap: civic visible, religious hidden.
            var root = _docGO.GetComponent<UIDocument>().rootVisualElement;
            Assert.AreEqual(DisplayStyle.Flex,
                root.Q<VisualElement>("minimap-hit-p_civic_1").style.display.value);
            Assert.AreEqual(DisplayStyle.None,
                root.Q<VisualElement>("minimap-hit-p_relig_1").style.display.value);
            Assert.AreEqual(DisplayStyle.None,
                root.Q<VisualElement>("minimap-hit-p_relig_2").style.display.value);
        }

        [Test]
        public void ClearFilters_RevertsToUnrestricted()
        {
            ToggleFacet("religious", true);
            Assert.AreEqual(2, CurrentRows().Count);

            _docGO.GetComponent<FilterTrayView>().ClearAllFilters();

            // Facets cleared -> unrestricted semantics: empty query shows nothing again,
            // and every minimap dot is visible.
            Assert.AreEqual(0, CurrentRows().Count);
            var root = _docGO.GetComponent<UIDocument>().rootVisualElement;
            foreach (var poi in _config.pois)
                Assert.AreEqual(DisplayStyle.Flex,
                    root.Q<VisualElement>("minimap-hit-" + poi.id).style.display.value,
                    $"dot {poi.id} must be visible after clearing filters");
        }

        [Test]
        public void CandidateNarrowing_NeverReturnsExcludedPoi_EvenWhenItScoresHigher()
        {
            // "hall" matches Civic Hall; restricting candidates to Civic Tower must yield
            // zero results -- an excluded POI may not leak in regardless of score.
            var results = _index.Search("hall", SearchMatchMode.Any,
                new HashSet<string> { "p_civic_2" });
            Assert.AreEqual(0, results.Count);

            var hit = _index.Search("tower", SearchMatchMode.Any,
                new HashSet<string> { "p_civic_2" });
            Assert.AreEqual(1, hit.Count);
            Assert.AreEqual("p_civic_2", hit[0].POIId);
        }
    }
}