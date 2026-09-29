using System;
using System.Collections.Generic;

namespace TileStories
{
    // Where the card's audio stands (ICardAudio.State): nothing loaded, a clip playing, a clip held paused
    public enum CardAudioState
    {
        Idle,
        Playing,
        Paused,
    }

    // One audio the card can play: which point it belongs to, the clip's path in the wall's media folder and the words a player shows for it.
    // Two blocks on one card that name the same clip are the SAME audio (PoiId + ClipPath): a hero chip and a player show one playing state.
    public sealed class AudioTrack
    {
        public readonly string PoiId;
        public readonly string ClipPath;
        public readonly string Title;

        public AudioTrack(string poiId, string clipPath, string title)
        {
            PoiId = poiId ?? "";
            ClipPath = clipPath ?? "";
            Title = title ?? "";
        }

        public bool SameAudio(AudioTrack other) => other != null && PoiId == other.PoiId && ClipPath == other.ClipPath;
    }

    // What an audio block (and the mini-player) may ask of the card's ONE audio owner (_3.1 step 9A). The views never touch an AudioSource:
    // they ask this. Every action is safe to call at any time (a view need not know the state first).
    public interface ICardAudio
    {
        // The state of the current audio (Idle when there is none)
        CardAudioState State { get; }
        // The audio playing or held paused (null when Idle); while another audio fades out to make room, this is already the NEW one
        AudioTrack Current { get; }
        // Seconds into the current audio, its length in seconds (0 while none is loaded or a switch fades) and the speed it plays at
        float Position { get; }
        float Length { get; }
        float Speed { get; }
        // The audios waiting for the current one to end (the Queue setting)
        IReadOnlyList<AudioTrack> Queue { get; }
        // Raised on every change a player draws: a state, a seek, a speed, the queue -- and on every Tick while playing (the progress)
        event Action Changed;

        bool IsCurrent(AudioTrack track);
        bool IsQueued(AudioTrack track);
        // The one button of a player: play a new audio (starting it now, or switching / queueing by the setting), pause a playing one,
        // resume a paused one, take a queued one out of the queue
        void Toggle(AudioTrack track);
        void Pause();
        void Resume();
        void Seek(float seconds);
        void SetSpeed(float speed);
        // A block that names the current audio hands over its latest words (a live edit renamed it): the mini-player shows the new title
        // while the sound plays on. Any other audio is ignored; nothing is restarted
        void UpdateTitle(AudioTrack track);
        // End the current audio and forget the queue (the card closed with Keep Audio Playing off)
        void Stop();
    }
}
