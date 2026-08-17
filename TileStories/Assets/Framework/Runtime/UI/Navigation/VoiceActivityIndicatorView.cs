using UnityEngine;

namespace TileStories
{
    // Policy + presentation-decisions for the voice-search "listening/processing" indicator
    // (spec _2.6 section 12). Decouples the indicator STYLE (config-driven via
    // WallConfigData.voice_activity_indicator_style) from SearchOverlayView: the view owns the
    // actual UI elements (mic button, listen bar) and forwards each VoiceSearchState here;
    // this plain C# class owns ONLY the "what should show" decisions (which mic label, which
    // bar visibility), so the policy is EditMode-testable with no UI Toolkit / scene
    // (20-code-quality.md: logic in plain classes, not the MonoBehaviour).
    //
    // Styles (WallConfigData.voice_activity_indicator_style):
    //   "mic_text"  (default) mic button text flips to "..." while listening/processing.
    //     Behavior-identical to the legacy inline implementation, so existing walls see no
    //     change unless they opt in.
    //   "listen_bar" renders a dedicated, explicitly-labelled listen bar for the duration.
    public sealed class VoiceActivityIndicatorView
    {
        public enum IndicatorStyle { MicText, ListenBar }

        public const string MicTextStyleName = "mic_text";
        public const string ListenBarStyleName = "listen_bar";
        public static readonly string[] StyleNames = { MicTextStyleName, ListenBarStyleName };

        // Accessible label for the dedicated listen bar (listen_bar mode).
        public const string ListenBarLabel = "Voice search in progress";

        private static bool s_unknownStyleLogged;

        private readonly IndicatorStyle _style;

        // Parse the wall config string -> enum. Unknown/missing values fall back to MicText,
        // logging once so a typo'd field can never silently break voice feedback. Mirrors
        // MarkerVisualsParser's fatal-free fallback convention.
        public static IndicatorStyle ParseStyle(string raw)
        {
            if (string.IsNullOrEmpty(raw) || raw == MicTextStyleName)
                return IndicatorStyle.MicText;
            if (raw == ListenBarStyleName)
                return IndicatorStyle.ListenBar;

            if (!s_unknownStyleLogged)
            {
                s_unknownStyleLogged = true;
                Debug.LogWarning($"[VoiceIndicator] unknown voice_activity_indicator_style \"{raw}\"; defaulting to {MicTextStyleName}.");
            }
            return IndicatorStyle.MicText;
        }

        // Construct from the raw config string (the view passes _config.voice_activity_indicator_style).
        public VoiceActivityIndicatorView(string style)
        {
            _style = ParseStyle(style);
        }

        public IndicatorStyle Style => _style;

        // True for any transient voice state. Both styles treat the state identically at the
        // policy level; only the presentation differs.
        public static bool IsVoiceActive(VoiceSearchState state) =>
            state == VoiceSearchState.Listening || state == VoiceSearchState.Processing;

        // mic_text mode: the label the mic button should show now.
        public string MicLabelForState(VoiceSearchState state) => IsVoiceActive(state) ? "..." : "Mic";

        // listen_bar mode: whether the dedicated bar should be shown.
        public bool IsBarVisible(VoiceSearchState state) => IsVoiceActive(state);
    }
}
