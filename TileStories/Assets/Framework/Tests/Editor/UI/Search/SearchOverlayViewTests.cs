using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Tier-0 tests for SearchOverlayView's policy helpers, exercisable in EditMode
    // with no UIDocument/scene (CreateUI is null-guarded). (spec _2.6 section 9)
    public class SearchOverlayViewTests
    {
        private GameObject _go;
        private SearchOverlayView _view;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("SearchOverlayTest");
            _view = _go.AddComponent<SearchOverlayView>();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_go);

        private static WallConfigData Config(bool voiceEnabled) => new WallConfigData
        {
            voice_search_enabled = voiceEnabled,
            voice_search_match_mode = "all",
            pois = new List<POIData>
            {
                new POIData { id = "poi_1", name = "Cathedral", category = "religious" },
                new POIData { id = "poi_2", name = "Church", category = "religious" },
                new POIData { id = "poi_3", name = "Town Hall", category = "civic" },
            }
        };

        [Test]
        public void IsMicVisible_VoiceEnabled_ShowsMic()
        {
            _view.Initialize(Config(true), null, null,
                new RecentSearchesManager(), new SuggestedSearchesManager());
            Assert.IsTrue(_view.IsMicVisible());
        }

        [Test]
        public void IsMicVisible_VoiceDisabled_HidesMic()
        {
            _view.Initialize(Config(false), null, null,
                new RecentSearchesManager(), new SuggestedSearchesManager());
            Assert.IsFalse(_view.IsMicVisible());
        }

        [Test]
        public void GetDisplayedSuggestions_ReturnsCategoryDistribution()
        {
            var suggested = new SuggestedSearchesManager(topN: 5)
            {
                Source = SuggestedSearchesManager.SuggestedSource.CategoryDistribution
            };
            _view.Initialize(Config(true), null, null,
                new RecentSearchesManager(), suggested);

            var suggestions = _view.GetDisplayedSuggestions();
            Assert.That(suggestions, Has.Count.EqualTo(2));
            Assert.AreEqual("religious", suggestions[0]); // 2 POIs -> first
            Assert.AreEqual("civic", suggestions[1]);
        }

        [Test]
        public void GetDisplayedSuggestions_RecentFirst_MixesRecentAndCategories()
        {
            var recent = new RecentSearchesManager();
            recent.Add("bridge");
            var suggested = new SuggestedSearchesManager(topN: 5)
            {
                Source = SuggestedSearchesManager.SuggestedSource.RecentFirst
            };
            _view.Initialize(new WallConfigData
            {
                voice_search_enabled = true,
                voice_search_match_mode = "all",
                suggested_source = "recent_first",
                pois = new List<POIData>
                {
                    new POIData { id = "poi_1", name = "Cathedral", category = "religious" },
                    new POIData { id = "poi_2", name = "Church", category = "religious" },
                    new POIData { id = "poi_3", name = "Town Hall", category = "civic" },
                }
            }, null, null, recent, suggested);

            var suggestions = _view.GetDisplayedSuggestions();
            Assert.That(suggestions, Has.Count.EqualTo(3));
            Assert.AreEqual("bridge", suggestions[0]);
            Assert.AreEqual("religious", suggestions[1]);
            Assert.AreEqual("civic", suggestions[2]);
        }

        // --- Tier-0.5: CreateUI accessibility tooltips (spec _2.6 section 15a) ---

        [Test]
        public void CreateUI_SearchField_HasAccessibilityTooltip()
        {
            var uiDoc = _go.AddComponent<UIDocument>();
            _view.Initialize(Config(true), null, null,
                new RecentSearchesManager(), new SuggestedSearchesManager(), uiDoc);

            var searchField = uiDoc.rootVisualElement.Q<TextField>("search-field");
            Assert.IsNotNull(searchField, "Search field should be created");
            Assert.IsFalse(string.IsNullOrEmpty(searchField.tooltip),
                "Search field should have an accessibility tooltip");
        }

        [Test]
        public void CreateUI_MicButton_HasAccessibilityTooltip()
        {
            var uiDoc = _go.AddComponent<UIDocument>();
            _view.Initialize(Config(true), null, null,
                new RecentSearchesManager(), new SuggestedSearchesManager(), uiDoc);

            var micButton = uiDoc.rootVisualElement.Q<Button>("voice-mic-btn");
            Assert.IsNotNull(micButton, "Mic button should be created");
            Assert.IsFalse(string.IsNullOrEmpty(micButton.tooltip),
                "Mic button should have an accessibility tooltip");
        }

        [Test]
        public void CreateUI_MicButton_TextIsMicAtIdle()
        {
            var uiDoc = _go.AddComponent<UIDocument>();
            _view.Initialize(Config(true), null, null,
                new RecentSearchesManager(), new SuggestedSearchesManager(), uiDoc);

            var micButton = uiDoc.rootVisualElement.Q<Button>("voice-mic-btn");
            Assert.AreEqual("Mic", micButton.text,
                "Mic button text should be 'Mic' in idle state");
        }

        [Test]
        public void CreateUI_SuggestionRows_HaveTooltips()
        {
            var suggested = new SuggestedSearchesManager(topN: 5)
            {
                Source = SuggestedSearchesManager.SuggestedSource.CategoryDistribution
            };
            var uiDoc = _go.AddComponent<UIDocument>();
            _view.Initialize(Config(true), null, null,
                new RecentSearchesManager(), suggested, uiDoc);

            var suggestions = _view.GetDisplayedSuggestions();
            Assert.That(suggestions, Is.Not.Empty, "Should have suggestions to render");

            foreach (var term in suggestions)
            {
                var suggestionBtn = uiDoc.rootVisualElement.Q<Button>("suggestion-" + term);
                Assert.IsNotNull(suggestionBtn, $"Suggestion button for '{term}' should exist");
                Assert.IsFalse(string.IsNullOrEmpty(suggestionBtn.tooltip),
                    $"Suggestion '{term}' should have an accessibility tooltip");
            }
        }

        [Test]
        public void CreateUI_VoiceActivityBar_HasAccessibilityTooltip()
        {
            var uiDoc = _go.AddComponent<UIDocument>();
            _view.Initialize(Config(true), null, null,
                new RecentSearchesManager(), new SuggestedSearchesManager(), uiDoc);

            var bar = uiDoc.rootVisualElement.Q<VisualElement>("voice-activity-bar");
            Assert.IsNotNull(bar, "Voice activity bar should be created");
            Assert.IsFalse(string.IsNullOrEmpty(bar.tooltip),
                "Voice activity bar should have an accessibility tooltip");
        }

        [Test]
        public void CreateUI_VoiceActivityBar_HiddenAtIdle()
        {
            var uiDoc = _go.AddComponent<UIDocument>();
            _view.Initialize(Config(true), null, null,
                new RecentSearchesManager(), new SuggestedSearchesManager(), uiDoc);

            var bar = uiDoc.rootVisualElement.Q<VisualElement>("voice-activity-bar");
            Assert.AreEqual(DisplayStyle.None, bar.style.display.value,
                "Voice activity bar should be hidden when not actively listening");
        }
    }
}
