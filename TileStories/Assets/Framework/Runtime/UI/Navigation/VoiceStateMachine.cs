using System;

namespace TileStories
{
    // State machine for the voice-search flow (spec _2.6 section 12):
    //   idle -> listening -> processing -> result | error
    // Pure C# class (no MonoBehaviour) so EditMode tests exercise every transition
    // rule without a scene. VoiceSearchController owns the wiring that invokes the
    // transcriber and feeds its transcript through the search pipeline; THIS class
    // owns the *when* of each transition (the policy), keeping decisions out of the
    // wiring layer (20-code-quality.md: logic in plain classes).
    public enum VoiceSearchState
    {
        Idle,
        Listening,
        Processing,
        Result,
        Error
    }

    public sealed class VoiceSearchStateMachine
    {
        public event Action<VoiceSearchState> StateChanged;

        public VoiceSearchState State { get; private set; } = VoiceSearchState.Idle;

        // The last transcript received from the transcriber (set on OnTranscribed).
        public string LastTranscript { get; private set; } = string.Empty;

        // The last error message received (set on OnTranscriberError/OnSearchFailed).
        public string LastError { get; private set; } = string.Empty;

        // Decide whether a transcript should trigger a search and, if so, which
        // match mode to use. Returns null for empty/whitespace transcripts (no
        // search). Defaults to Any on an unrecognised config value so a typo'd
        // voice_search_match_mode can never silently break searching.
        public static SearchMatchMode? ResolveSearchMode(string transcript, string matchModeConfig)
        {
            if (string.IsNullOrWhiteSpace(transcript))
                return null;

            return ParseMatchMode(matchModeConfig);
        }

        public static SearchMatchMode ParseMatchMode(string value) =>
            value == "all" ? SearchMatchMode.All : SearchMatchMode.Any;

        // idle/result/error -> listening. No-op while already listening or
        // processing, so a double mic tap cannot race the pipeline.
        public void BeginListening()
        {
            if (State == VoiceSearchState.Listening || State == VoiceSearchState.Processing)
                return;

            Transition(VoiceSearchState.Listening);
        }

        // A transcript was returned while listening: listening -> processing.
        // Ignored unless currently listening (no re-entry from other states).
        public void OnTranscribed(string transcript)
        {
            if (State != VoiceSearchState.Listening)
                return;

            LastTranscript = transcript ?? string.Empty;
            Transition(VoiceSearchState.Processing);
        }

        // The search pipeline completed successfully: processing -> result.
        public void OnSearchSucceeded()
        {
            if (State != VoiceSearchState.Processing)
                return;

            Transition(VoiceSearchState.Result);
        }

        // The search pipeline failed: processing -> error.
        public void OnSearchFailed(string error)
        {
            LastError = error;
            if (State == VoiceSearchState.Processing)
                Transition(VoiceSearchState.Error);
        }

        // The transcriber itself errored (e.g. permission denied, no speech);
        // this can happen from listening or processing, so it isn't gated.
        public void OnTranscriberError(string error)
        {
            LastError = error;
            Transition(VoiceSearchState.Error);
        }

        // Return to Idle after a result/error cycle so the mic is reusable.
        public void Reset()
        {
            if (State == VoiceSearchState.Idle)
                return;
            Transition(VoiceSearchState.Idle);
        }

        private void Transition(VoiceSearchState newState)
        {
            State = newState;
            StateChanged?.Invoke(State);
        }
    }
}
