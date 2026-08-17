namespace TileStories
{
    // Pure-logic policy for the search TextField (spec _2.6 section 4 / §9):
    // when to (re)run a search and when to show the voice mic button.
    // Stateless and directly unit-testable without Unity lifecycle or a scene.
    public static class SearchInputGuard
    {
        // Default debounce window for `dynamic` search mode, in seconds.
        public const float DefaultDebounceSeconds = 0.15f;

        // Whether a (re)search should fire now, given the query, the configured
        // search_mode and the seconds elapsed since the last input. `explicit`
        // submits immediately on every non-empty change; `dynamic` (default)
        // requires the debounce window to have elapsed first.
        public static bool ShouldSubmit(string query, string searchMode, float elapsedSeconds,
                                        float debounceSeconds = DefaultDebounceSeconds)
        {
            if (string.IsNullOrWhiteSpace(query))
                return false;

            if (searchMode == "explicit")
                return true;

            return elapsedSeconds >= debounceSeconds;
        }

        // The mic button is shown only when voice search is enabled AND a usable
        // transcriber is available -- extracted here so the rule is tested once
        // and never duplicated across the view and its tests.
        public static bool IsMicVisible(bool voiceEnabled, bool transcriberSupported) =>
            voiceEnabled && transcriberSupported;
    }
}
