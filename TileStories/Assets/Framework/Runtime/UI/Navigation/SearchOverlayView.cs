using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // Top-of-screen search overlay: a typed TextField, a voice mic button
    // (shown only when voice search is enabled), and recent/suggested rows.
    // Delegates result rendering to ResultsListView, suggestion generation to the
    // recent/suggested managers, and voice capture to VoiceSearchController --
    // this class owns only the input policy + presentation (spec _2.6 section 9).
    public class SearchOverlayView : MonoBehaviour
    {
        private WallConfigData _config;
        private ResultsListView _resultsListView;
        private RecentSearchesManager _recent;
        private SuggestedSearchesManager _suggested;
        /// <summary>Exposed for test wiring of the voice search controller.</summary>
    public VoiceSearchController VoiceController => _voice;
    /// <summary>Exposed for test wiring of the voice transcriber preset.</summary>
    public ITranscriber Transcriber => _transcriber;
    /// <summary>Sets the preset transcript emitted by DebugTranscriber on StartListening.</summary>
    public void SetPresetTranscript(string transcript)
    {
        // PresetTranscript is a DebugTranscriber-specific property; in a build the factory
        // may return a different ITranscriber implementation without this field.
        if (_transcriber is DebugTranscriber dbg)
            dbg.PresetTranscript = transcript;
    }
        private ITranscriber _transcriber;
        private VoiceSearchController _voice;
        private VoiceActivityIndicatorView _voiceIndicator;
        private UIDocument _uiDocument;
        private VisualElement _root;
        private VisualElement _voiceActivityBar;
        private TextField _searchField;
        private Button _micButton;
        private VisualElement _suggestionsContainer;
        private Coroutine _debounceRoutine;
        private float _lastChangeTime;
        private List<string> _displayedSuggestions = new List<string>();

        // Shared search submit used for typed and voice input: record the query as
        // recent, then feed the result list. Called from VoiceSearchController too.
        public void SubmitSearch(string query, SearchMatchMode matchMode)
        {
            if (string.IsNullOrWhiteSpace(query))
                return;

            _recent?.Add(query);
            _resultsListView?.RefreshResults(query, matchMode);
        }

        // Wires config + dependencies. `uiDocument` may be injected (tests) or
        // located via FindFirstObjectByType at runtime. CreateUI is null-guarded
        // so IsMicVisible/GetDisplayedSuggestions stay testable in EditMode.
        public void Initialize(WallConfigData config, POISearchIndex searchIndex,
                               ResultsListView resultsListView,
                               RecentSearchesManager recent, SuggestedSearchesManager suggested,
                               UIDocument uiDocument = null)
        {
            _config = config;
            _resultsListView = resultsListView;
            _recent = recent;
            _suggested = suggested;

            // (D3) Apply the config's suggested_source to the manager.
            SuggestedSourceApplier.Apply(_config, _suggested);

            _transcriber = new TranscriberFactory().Create(_config?.voice_search_enabled ?? false);
            _voice = new VoiceSearchController(_config, _transcriber, SubmitSearch);
            _voice.StateChanged += OnVoiceStateChanged;
            _voiceIndicator = new VoiceActivityIndicatorView(_config?.voice_activity_indicator_style);

            _uiDocument = uiDocument != null ? uiDocument : FindFirstObjectByType<UIDocument>();
            if (_uiDocument != null && _root == null)
                CreateUI(_uiDocument.rootVisualElement);

            RefreshSuggestions();
        }

        public void SetVisible(bool visible)
        {
            if (_root != null)
                _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public bool IsMicVisible() =>
            SearchInputGuard.IsMicVisible(_config?.voice_search_enabled ?? false,
                                          _transcriber?.IsSupported ?? false);

        public IReadOnlyList<string> GetDisplayedSuggestions() => _displayedSuggestions;

        private void CreateUI(VisualElement root)
        {
            _root = root;

            var container = new VisualElement { name = "search-overlay" };
            container.style.position = Position.Absolute;
            container.style.top = 12;
            container.style.left = 12;
            container.style.right = 12;
            container.style.height = 44;
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            root.Add(container);

            _searchField = new TextField { name = "search-field" };
            _searchField.tooltip = "Search POIs by name, category, or keyword";
            _searchField.style.flexGrow = 1;
            _searchField.style.height = 32;
            _searchField.style.fontSize = 14;
            // explicit fires OnValueChanged on Enter (isDelayed); dynamic fires
            // per keystroke and is debounced in OnSearchFieldChanged.
            _searchField.isDelayed = _config?.search_mode == "explicit";
            _searchField.RegisterValueChangedCallback<string>(OnSearchFieldChanged);
            container.Add(_searchField);

            _micButton = new Button(OnMicClicked) { name = "voice-mic-btn", text = "Mic" };
            _micButton.tooltip = "Activate voice search";
            _micButton.style.width = 44;
            _micButton.style.height = 44;
            _micButton.style.marginLeft = 8;
            _micButton.style.display = IsMicVisible() ? DisplayStyle.Flex : DisplayStyle.None;
            container.Add(_micButton);

            _voiceActivityBar = new VisualElement { name = "voice-activity-bar" };
            _voiceActivityBar.tooltip = VoiceActivityIndicatorView.ListenBarLabel;
            _voiceActivityBar.style.position = Position.Absolute;
            _voiceActivityBar.style.top = 56;
            _voiceActivityBar.style.left = 12;
            _voiceActivityBar.style.right = 12;
            _voiceActivityBar.style.height = 4;
            _voiceActivityBar.style.backgroundColor = new StyleColor(new Color(0.7f, 0.9f, 1.0f, 0.8f));
            _voiceActivityBar.style.display = DisplayStyle.None;
            _voiceActivityBar.AddToClassList("voice-activity-indicator");
            root.Add(_voiceActivityBar);

            _suggestionsContainer = new VisualElement { name = "search-suggestions" };
            _suggestionsContainer.style.position = Position.Absolute;
            _suggestionsContainer.style.top = 56;
            _suggestionsContainer.style.left = 12;
            _suggestionsContainer.style.right = 12;
            _suggestionsContainer.style.flexDirection = FlexDirection.Column;
            root.Add(_suggestionsContainer);
        }

        private void OnSearchFieldChanged(ChangeEvent<string> evt)
        {
            _lastChangeTime = Time.realtimeSinceStartup;
            string mode = _config?.search_mode ?? "dynamic";

            if (SearchInputGuard.ShouldSubmit(evt.newValue, mode, 0f))
            {
                SubmitSearch(evt.newValue, SearchMatchMode.Any);
                return;
            }

            // dynamic: debounce, restarting the wait on every keystroke.
            if (_debounceRoutine != null)
                StopCoroutine(_debounceRoutine);
            _debounceRoutine = StartCoroutine(DelayedSearch(evt.newValue, mode));
        }

        private IEnumerator DelayedSearch(string query, string mode)
        {
            float elapsed = Time.realtimeSinceStartup - _lastChangeTime;
            yield return new WaitForSeconds(
                Mathf.Max(0f, SearchInputGuard.DefaultDebounceSeconds - elapsed));

            if (_searchField != null && _searchField.value == query &&
                SearchInputGuard.ShouldSubmit(query, mode, SearchInputGuard.DefaultDebounceSeconds))
                SubmitSearch(query, SearchMatchMode.Any);

            _debounceRoutine = null;
        }

        private void OnMicClicked() => _voice?.StartVoiceSearch();

        // Delegate voice-state policy to VoiceActivityIndicatorView (spec _2.6 section 12):
        // mic_text mode flips the mic button label, listen_bar mode shows a dedicated bar.
        private void OnVoiceStateChanged(VoiceSearchState state)
        {
            if (_voiceIndicator == null)
                return;

            if (_voiceIndicator.Style == VoiceActivityIndicatorView.IndicatorStyle.MicText)
            {
                if (_micButton != null)
                    _micButton.text = _voiceIndicator.MicLabelForState(state);
            }
            else
            {
                if (_voiceActivityBar != null)
                    _voiceActivityBar.style.display = _voiceIndicator.IsBarVisible(state)
                        ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        public void RefreshSuggestions()
        {
            _displayedSuggestions =
                _suggested?.BuildSuggestions(_config, _recent) ?? new List<string>();

            if (_suggestionsContainer == null)
                return;

            _suggestionsContainer.Clear();
            foreach (string term in _displayedSuggestions)
                _suggestionsContainer.Add(CreateSuggestionRow(term));
        }

        private Button CreateSuggestionRow(string term)
        {
            var button = new Button(() => SubmitSuggestion(term))
            {
                name = "suggestion-" + term,
                text = term,
                tooltip = $"Search for \"{term}\""
            };
            button.style.unityTextAlign = TextAnchor.MiddleLeft;
            button.style.fontSize = 13;
            button.style.height = 32;
            return button;
        }

        private void SubmitSuggestion(string term)
        {
            SubmitSearch(term, SearchMatchMode.Any);
            if (_searchField != null)
                _searchField.SetValueWithoutNotify(term);
        }

        private void OnDisable()
        {
            if (_voice != null)
                _voice.StateChanged -= OnVoiceStateChanged;
        }
    }

    // (D3) Apply the config's suggested_source to a SuggestedSearchesManager.
    // Pure static helper so the wiring is testable without a MonoBehaviour.
    // config string -> enum -> manager.Source.
    public static class SuggestedSourceApplier
    {
        public static void Apply(WallConfigData config, SuggestedSearchesManager manager)
        {
            if (config == null || manager == null)
                return;

            manager.Source = SuggestedSearchesManager.ParseSource(config.suggested_source);
        }
    }
}
