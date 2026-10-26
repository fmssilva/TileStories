using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The search bar (spec _2.6 sections 5, 12, 13): the query field, a mic button (voice only), a
    // Filters button with the number of active filters, a Map button (minimap "toggle" only), and the
    // suggestions shown under an empty focused field. It reports what the visitor does and decides
    // nothing about results. "dynamic" search reports the query after a short pause in typing,
    // "explicit" only on Enter. Plain C#: built into the parent the search UI hands it.
    public sealed class SearchOverlayView
    {
        // Pause after the last keystroke before a dynamic search runs
        public const long DebounceMs = 150;

        public VisualElement Root { get; }
        public VisualElement Suggestions { get; }

        // The query to search now ("" = no search)
        public event Action<string> QueryChanged;
        // The visitor pressed Enter on this query (it is worth remembering as a recent search)
        public event Action<string> Submitted;
        public event Action FiltersPressed;
        public event Action MapPressed;
        public event Action MicPressed;

        private readonly TextField _field;
        private readonly Button _mic;
        private readonly Button _filters;
        private readonly Button _map;
        private readonly VisualElement _listenBar;
        private IVisualElementScheduledItem _pending;
        private bool _dynamic = true;

        public SearchOverlayView(VisualElement parent)
        {
            Root = new VisualElement { name = "search-bar" };
            Root.AddToClassList("search-bar");

            _field = new TextField { name = "search-field", tooltip = "Search by name, category or keyword" };
            _field.textEdition.placeholder = "Search";
            _field.AddToClassList("search-field");
            _field.RegisterValueChangedCallback(evt => OnTyped(evt.newValue));
            _field.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode != KeyCode.Return && evt.keyCode != KeyCode.KeypadEnter) return;
                // - text, not value: a delayed (explicit) field has not committed its value yet
                string typed = _field.text ?? "";
                RunNow(typed);
                if (!string.IsNullOrWhiteSpace(typed)) Submitted?.Invoke(typed.Trim());
            }, TrickleDown.TrickleDown);
            _field.RegisterCallback<FocusInEvent>(_ => ShowSuggestionsIfEmpty());
            _field.RegisterCallback<FocusOutEvent>(_ => Suggestions.schedule.Execute(() => Suggestions.style.display = DisplayStyle.None).ExecuteLater(200));

            _mic = MakeButton("search-mic", "Mic", "Voice search", () => MicPressed?.Invoke());
            _filters = MakeButton("search-filters", "Filters", "Show filters", () => FiltersPressed?.Invoke());
            _map = MakeButton("search-map", "Map", "Show map", () => MapPressed?.Invoke());

            Root.Add(_field);
            Root.Add(_mic);
            Root.Add(_filters);
            Root.Add(_map);
            parent.Add(Root);

            _listenBar = new VisualElement { name = "search-listen-bar", tooltip = VoiceActivityIndicatorView.ListenBarLabel };
            _listenBar.AddToClassList("listen-bar");
            _listenBar.style.display = DisplayStyle.None;
            parent.Add(_listenBar);

            Suggestions = new VisualElement { name = "search-suggestions" };
            Suggestions.AddToClassList("search-panel");
            Suggestions.AddToClassList("search-suggestions");
            Suggestions.style.display = DisplayStyle.None;
            parent.Add(Suggestions);
        }

        public string Query => _field.value ?? "";
        public bool MicShown => _mic.style.display != DisplayStyle.None;
        public bool MapButtonShown => _map.style.display != DisplayStyle.None;
        public string FiltersText => _filters.text;
        public bool IsDelayed => _field.isDelayed;

        private List<string> _suggestions = new();
        public IReadOnlyList<string> SuggestionTerms => _suggestions;

        // Apply the wall's search settings; the mic shows only when voice can really run
        public void Configure(SearchSettings search, bool voiceAvailable)
        {
            _dynamic = (search?.mode ?? SelectFilterSearchOptions.ModeDynamic) != SelectFilterSearchOptions.ModeExplicit;
            // - explicit: the field reports its value only on Enter / focus loss
            _field.isDelayed = !_dynamic;
            _mic.style.display = voiceAvailable ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // Put a query in the field and search it at once (a suggestion, a voice transcript, the Editor)
        public void SetQuery(string query)
        {
            _field.SetValueWithoutNotify(query ?? "");
            RunNow(_field.value);
        }

        public void SetFilterCount(int count) => _filters.text = count > 0 ? $"Filters ({count})" : "Filters";

        public void SetMapButton(bool shown, bool open)
        {
            _map.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;
            _map.EnableInClassList("search-button--on", open);
        }

        // Show a voice state: the mic label (mic_text style) or the listen bar (listen_bar style)
        public void ShowVoiceState(VoiceActivityIndicatorView indicator, VoiceSearchState state)
        {
            if (indicator.Style == VoiceActivityIndicatorView.IndicatorStyle.MicText)
                _mic.text = indicator.MicLabelForState(state);
            else
                _listenBar.style.display = indicator.IsBarVisible(state) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public string MicText => _mic.text;
        public bool ListenBarShown => _listenBar.style.display != DisplayStyle.None;

        // The terms offered under an empty field
        public void SetSuggestions(List<string> terms)
        {
            _suggestions = terms ?? new List<string>();
            Suggestions.Clear();
            foreach (var term in _suggestions)
            {
                var t = term;
                var b = new Button(() =>
                {
                    SetQuery(t);
                    Submitted?.Invoke(t);
                }) { name = "suggestion-" + t, text = t, tooltip = $"Search for {t}" };
                b.AddToClassList("suggestion");
                Suggestions.Add(b);
            }
        }

        private void OnTyped(string text)
        {
            Suggestions.style.display = DisplayStyle.None;
            if (!_dynamic)
            {
                RunNow(text);
                return;
            }
            _pending?.Pause();
            _pending = _field.schedule.Execute(() => RunNow(_field.value));
            _pending.ExecuteLater(DebounceMs);
            if (string.IsNullOrEmpty(text)) RunNow(text);
        }

        private void RunNow(string text)
        {
            _pending?.Pause();
            QueryChanged?.Invoke((text ?? "").Trim());
        }

        private void ShowSuggestionsIfEmpty() =>
            Suggestions.style.display = string.IsNullOrEmpty(_field.value) && _suggestions.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;

        private static Button MakeButton(string name, string text, string tooltip, Action onClick)
        {
            var b = new Button(onClick) { name = name, text = text, tooltip = tooltip };
            b.AddToClassList("search-button");
            return b;
        }
    }
}
