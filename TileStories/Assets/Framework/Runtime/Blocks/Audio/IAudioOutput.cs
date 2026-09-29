using UnityEngine;

namespace TileStories
{
    // The sound device the card's audio plays through (_3.1 step 9A): the one seam between CardAudioService's decisions and Unity's
    // AudioSource. UnityAudioOutput is the app's; DevTools/ManualAudioOutput is the gallery's and the tests' -- a silent device whose
    // clip time moves only when a test says, so no audio time is ever a real-time wait.
    public interface IAudioOutput
    {
        // Playing right now (false while paused, stopped, or after a clip ran out)
        bool IsPlaying { get; }
        // Seconds into the loaded clip
        float Time { get; set; }
        // The loaded clip's length in seconds (0 with none)
        float Length { get; }
        // 0..1, the fade
        float Volume { get; set; }
        // The playing speed (the AudioSource's pitch: 1 = normal)
        float Speed { get; set; }
        // Make `clip` the loaded clip, at its start, not playing
        void Load(AudioClip clip);
        void Play();
        void Pause();
        void Stop();
    }
}
