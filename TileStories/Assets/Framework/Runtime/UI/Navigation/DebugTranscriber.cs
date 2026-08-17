using System;
using UnityEngine;

namespace TileStories
{
    // ITranscriber implementation used in the Editor and for automated tests.
    // Returns a preset/injected transcript instead of touching a microphone,
    // so voice search is testable end-to-end without the yasirkula plugin or a
    // real device (Tier A mock-localization equivalent for voice). (spec §12)
    public sealed class DebugTranscriber : ITranscriber
    {
        public bool IsSupported => true;
        public bool IsBusy { get; private set; }

        // The transcript emitted synchronously on StartListening. Set before the
        // controller starts listening so tests can drive a known utterance.
        public string PresetTranscript { get; set; } = "cathedral";

        // Simulated voice-level amplitude (0..1) emitted at listen start.
        public float SimulatedVoiceLevel { get; set; } = 0.5f;

        public event Action<string> OnResult;
        public event Action<string> OnError;
        public event Action<float> OnVoiceLevel;

        // Debug backend is always permitted; no-op (no permission prompt).
        public void RequestPermission() { }

        public void StartListening()
        {
            IsBusy = true;
            OnVoiceLevel?.Invoke(SimulatedVoiceLevel);

            // Always emit a result (even if empty/whitespace) so the controller's
            // own empty-transcript handling is exercised in tests. A real backend
            // would surface 'no speech detected' via OnError instead of an empty
            // OnResult.
            OnResult?.Invoke(PresetTranscript?.Trim() ?? string.Empty);

            IsBusy = false;
        }

        public void StopListening()
        {
            IsBusy = false;
        }

        // Test hook: synthesize a transcriber error as if no speech was detected.
        public void SimulateError(string message)
        {
            IsBusy = false;
            OnError?.Invoke(message);
        }
    }
}
