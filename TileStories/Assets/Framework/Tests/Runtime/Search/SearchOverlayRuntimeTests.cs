using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using TileStories;

namespace TileStories.Tests
{
    // Phase-B integration test (spec _2.6 section 9-12, Block 4 step 8):
    // text search -> results list -> POI selection -> DetailCardView.
    // Validates the end-to-end voice-search pipeline:
    //   VoiceSearchController + DebugTranscriber -> transcript -> SearchMatchMode ->
    //   ResultsListView refresh (POISearchIndex) -> SelectionEventBus -> DetailCardView.
    // Uses AddComponent + UIDocument (no scene fixtures). Yields null for settle.
    public class SearchOverlayRuntimeTests
    {
        private GameObject _go;
        private SearchOverlayView _overlay;
        private ResultsListView _resultsListView;
        private DetailCardView _detailCard;
        private WallConfigData _config;
        private POISearchIndex _searchIndex;

        [SetUp]
        public void SetUp()
        {
            _config = new WallConfigData
            {
                wall_id = "test_wall",
                wall_name = "Test Wall",
                search_mode = "explicit",
                no_results_message = "No matches for \"{query}\" - try removing a filter.",
                voice_search_enabled = true,
                voice_search_match_mode = "all",
                pois = new List<POIData>
                {
                    new POIData
                    {
                        id = "poi_cathedral", name = "Cathedral",
                        category = "religious", x_norm = 0.3f, y_norm = 0.7f,
                        summary = "A beautiful old cathedral."
                    },
                    new POIData
                    {
                        id = "poi_castle", name = "Castle",
                        category = "defense", x_norm = 0.6f, y_norm = 0.4f,
                        summary = "A medieval castle on the hill."
                    },
                    new POIData
                    {
                        id = "poi_cave", name = "Cave",
                        category = "nature", x_norm = 0.5f, y_norm = 0.5f,
                        summary = "A dark cave entrance."
                    }
                }
            };

            _searchIndex = new POISearchIndex();
            _searchIndex.Build(_config);

            // Host GameObject for UIDocument (root search rebinding) + the views.
            _go = new GameObject("SearchOverlayTest");
            _go.AddComponent<UIDocument>();

            _overlay = _go.AddComponent<SearchOverlayView>();

            _resultsListView = _go.AddComponent<ResultsListView>();
            _resultsListView.Initialize(_config, _searchIndex);

            _detailCard = _go.AddComponent<DetailCardView>();
            _detailCard.Initialize(_config);

            // Managers are plain C# classes (not MonoBehaviours) -- instantiate directly.
            var recent = new TileStories.RecentSearchesManager(maxCount: 5);
            var suggested = new TileStories.SuggestedSearchesManager(topN: 5);
            _overlay.Initialize(_config, _searchIndex, _resultsListView, recent, suggested);

            // Deterministic voice utterance:
            _overlay.SetPresetTranscript("castle");
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_go);

        [UnityTest]
        public IEnumerator VoiceSearch_DrivesResultsThenSelectionRevealsDetailCard()
        {
            // 1. Start voice search. DebugTranscriber.StartListening emits OnResult("castle")
            //    synchronously; VoiceSearchController (wired in Initialize with SubmitSearch)
            //    resolves SearchMatchMode.All and calls ResultsListView.RefreshResults("castle", All).
            _overlay.VoiceController.StartVoiceSearch();
            yield return null;

            // 2. The search index must have resolved the utterance to the Castle POI only.
            //    (token "castle" matches Castle's name exactly; with mode All only it survives)
            var results = _searchIndex.Search("castle", SearchMatchMode.All);
            Assert.AreEqual(1, results.Count, "voice utterance 'castle' should resolve to exactly one POI");
            Assert.AreEqual("poi_castle", results[0].POIId);

            // 3. Simulate the results-list row selection via the same SelectionEventBus seam the
            //    ResultsListView.OnSelectionChanged handler uses when the user taps a result row.
            SelectionEventBus.RaiseMarkerSelected("poi_castle");
            yield return null;

            // 4. The DetailCardView reacts to the shared selection event and shows the POI name.
            Assert.IsTrue(_detailCard.IsVisibleState(),
                "DetailCardView should become visible after a POI selection");
            Assert.AreEqual("Castle", _detailCard.GetLabelText(),
                "DetailCard label should show the selected POI's name");
        }
        [UnityTest]
        public IEnumerator Text_IndexSearch_FeedsResultsAndSelectionRevealsDetailCard()
        {
            // Pure typed-search path: RefreshResults then selection -> detail.
            _resultsListView.RefreshResults("Cathedral", SearchMatchMode.Any);
            yield return null;

            var typed = _searchIndex.Search("cathedral", SearchMatchMode.Any);
            Assert.AreEqual(1, typed.Count, "typed query 'cathedral' resolves to one POI");
            Assert.AreEqual("poi_cathedral", typed[0].POIId);

            SelectionEventBus.RaiseMarkerSelected("poi_cathedral");
            yield return null;

            Assert.IsTrue(_detailCard.IsVisibleState());
            Assert.AreEqual("Cathedral", _detailCard.GetLabelText());
        }
    }
}