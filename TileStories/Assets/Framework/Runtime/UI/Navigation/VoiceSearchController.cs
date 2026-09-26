using System;

namespace TileStories
{
    // Voice search wiring (spec _2.6 section 12): asks the transcriber to listen and hands the transcript
    // to the same query path typing uses -- no voice-specific search. The transition rules live in
    // VoiceSearchStateMachine. Plain C#.
    public sealed class VoiceSearchController : IDisposable
    {
        private readonly ITranscriber _transcriber;
        private readonly VoiceSearchStateMachine _stateMachine = new();
        private readonly Action<string> _search;

        public VoiceSearchState State => _stateMachine.State;

        public event Action<VoiceSearchState> StateChanged
        {
            add => _stateMachine.StateChanged += value;
            remove => _stateMachine.StateChanged -= value;
        }

        // Whether the mic can run at all (a backend that works on this device)
        public bool IsAvailable => _transcriber != null && _transcriber.IsSupported;

        public VoiceSearchController(ITranscriber transcriber, Action<string> search)
        {
            _transcriber = transcriber;
            _search = search;
            if (_transcriber == null) return;
            _transcriber.OnResult += OnResult;
            _transcriber.OnError += OnError;
        }

        // The mic button: listen once (ignored while listening or without a backend)
        public void StartVoiceSearch()
        {
            if (!IsAvailable || _stateMachine.State == VoiceSearchState.Listening || _stateMachine.State == VoiceSearchState.Processing)
                return;
            _transcriber.RequestPermission();
            _stateMachine.BeginListening();
            _transcriber.StartListening();
        }

        // Cancel a running listen
        public void StopVoiceSearch()
        {
            _transcriber?.StopListening();
            _stateMachine.Reset();
        }

        private void OnResult(string transcript)
        {
            _stateMachine.OnTranscribed(transcript);
            // - nothing heard: back to idle, no search
            if (string.IsNullOrWhiteSpace(transcript))
            {
                _stateMachine.Reset();
                return;
            }
            _search?.Invoke(transcript.Trim());
            _stateMachine.OnSearchSucceeded();
        }

        private void OnError(string error) => _stateMachine.OnTranscriberError(error);

        public void Dispose()
        {
            if (_transcriber == null) return;
            _transcriber.OnResult -= OnResult;
            _transcriber.OnError -= OnError;
        }
    }
}
