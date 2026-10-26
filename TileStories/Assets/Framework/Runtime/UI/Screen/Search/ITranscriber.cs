using System;

namespace TileStories
{
    // Abstraction over speech-to-text backends (spec _2.6 section 12).
    // The production backend wraps yasirkula/UnitySpeechToText (deferred -- see
    // VoiceSearchController defect notes). DebugTranscriber ships now so the full
    // voice-search path is exercised in the Editor/PlayMode without a plugin
    // installed. Any backend implementing this interface is swappable with zero
    // controller rewrite because VoiceSearchController only knows this seam.
    public interface ITranscriber
    {
        bool IsSupported { get; }
        bool IsBusy { get; }
        void StartListening();
        void StopListening();
        void RequestPermission();
        event Action<string> OnResult;
        event Action<string> OnError;
        event Action<float> OnVoiceLevel;
    }
}
