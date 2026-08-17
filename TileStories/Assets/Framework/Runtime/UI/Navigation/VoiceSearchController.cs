using System;

namespace TileStories
{
    // Thin wiring layer over VoiceSearchStateMachine (spec _2.6 section 12).
    // ALL transition *rules* live in VoiceStateMachine; this class only decides
    // *when* to ask the transcriber to listen and *when* to feed its transcript
    // through the shared typed-search pipeline (no voice-specific search). It is
    // a plain C# class (not a MonoBehaviour) so it is fully EditMode-testable.
    public sealed class VoiceSearchController
    {
        private readonly ITranscriber _transcriber;
        private readonly VoiceSearchStateMachine _stateMachine;
        private readonly WallConfigData _config;
        private readonly Action<string, SearchMatchMode> _submitSearch;

        // Whether voice search can run: enabled in config AND the transcriber is
        // available. Exposed so the SearchOverlayView mic button can be hidden.
        public bool IsAvailable =>
            _config != null && _config.voice_search_enabled && _transcriber.IsSupported;

        public VoiceSearchState State => _stateMachine.State;

        public event Action<VoiceSearchState> StateChanged
        {
            add => _stateMachine.StateChanged += value;
            remove => _stateMachine.StateChanged -= value;
        }

        public VoiceSearchController(WallConfigData config, ITranscriber transcriber,
                                     Action<string, SearchMatchMode> submitSearch,
                                     ITranscriberFactory factory = null)
        {
            _config = config;
            _transcriber = transcriber ?? (factory ?? new TranscriberFactory()).Create(config?.voice_search_enabled ?? false);
            _submitSearch = submitSearch;
            _stateMachine = new VoiceSearchStateMachine();

            _transcriber.OnResult += OnTranscriberResult;
            _transcriber.OnError += OnTranscriberError;
        }

        // Mic button entry point. Inert (no-op) when voice search is disabled,
        // so the UI never needs to guard before calling this.
        public void StartVoiceSearch()
        {
            if (!IsAvailable)
                return;

            _transcriber.RequestPermission();
            _stateMachine.BeginListening();
            _transcriber.StartListening();
        }

        // Cancel an in-flight listen; returns to Idle so the mic can be tapped again.
        public void StopVoiceSearch()
        {
            _transcriber.StopListening();
            if (_stateMachine.State == VoiceSearchState.Listening ||
                _stateMachine.State == VoiceSearchState.Processing)
                _stateMachine.Reset();
        }

        // Forwarded from the transcriber: runs the shared search pipeline.
        private void OnTranscriberResult(string transcript)
        {
            _stateMachine.OnTranscribed(transcript);

            SearchMatchMode? mode = VoiceSearchStateMachine.ResolveSearchMode(
                transcript, _config.voice_search_match_mode);

            // Empty/whitespace transcript: no search, just return to Idle.
            if (mode == null)
            {
                _stateMachine.Reset();
                return;
            }

            _submitSearch?.Invoke(transcript, mode.Value);
            _stateMachine.OnSearchSucceeded();
        }

        private void OnTranscriberError(string error)
        {
            _stateMachine.OnTranscriberError(error);
        }
    }
}
