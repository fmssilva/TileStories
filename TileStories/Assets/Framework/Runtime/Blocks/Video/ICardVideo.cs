using System;
using UnityEngine;

namespace TileStories
{
    // Where the card's video stands (ICardVideo.State): nothing loaded, a clip playing, a clip held paused
    public enum CardVideoState
    {
        Idle,
        Playing,
        Paused,
    }

    // One video the card can play: which point it belongs to, the clip's path in the wall's media folder and the words a player shows for it.
    // Two views that name the same clip of the same point are the SAME video (the inline player and its full-screen view show one playback).
    public sealed class VideoTrack
    {
        public readonly string PoiId;
        public readonly string ClipPath;
        public readonly string Title;

        public VideoTrack(string poiId, string clipPath, string title)
        {
            PoiId = poiId ?? "";
            ClipPath = clipPath ?? "";
            Title = title ?? "";
        }

        public bool SameVideo(VideoTrack other) => other != null && PoiId == other.PoiId && ClipPath == other.ClipPath;
    }

    // What a video view (the video block, its full-screen view, a header's loop) may ask of the card's ONE video owner (_3.1 step 9B). The
    // views never touch a VideoPlayer: they ask this and draw its Texture. Two kinds of playback share the one player: the VISITOR's video
    // (started by a tap, with its sound) and a header's LOOP (muted, looping, started by the header itself) -- a loop plays only while no
    // visitor video holds the player, and gives way the moment one starts. Every action is safe to call at any time.
    public interface ICardVideo
    {
        CardVideoState State { get; }
        // What holds the player: the visitor's video or a header loop (null when Idle)
        VideoTrack Current { get; }
        // Current is a header loop (muted, looping), not a video the visitor started
        bool PlayingLoop { get; }
        // Seconds into Current and its length (0 when Idle)
        float Position { get; }
        float Length { get; }
        // A frame of Current is in Texture (false while the clip is still preparing: a view shows its poster until then)
        bool HasFrame { get; }
        // What the player draws into (a RenderTexture; a view shows it as its background while HasFrame)
        Texture Texture { get; }
        // Raised on every change a view draws: a state, a seek, a new current -- and on every Tick while playing (the progress, a new frame)
        event Action Changed;

        bool IsCurrent(VideoTrack track);
        // The one button of a player: play this video (letting go of any other video or loop), pause it, resume it
        void Toggle(VideoTrack track);
        void Pause();
        void Resume();
        void Seek(float seconds);
        // End the visitor's video (a header loop that asked for the player takes it back)
        void Stop();
        // A header asks for its muted loop: it plays whenever no visitor video holds the player
        void PlayLoop(VideoTrack loop);
        // That header went away (unbound, or it asks for no loop any more)
        void StopLoop(VideoTrack loop);
        // A card was shown for `poiId`: the visitor's video of ANOTHER point stops (a video belongs to its card)
        void CardShown(string poiId);
        // The card closed: nothing plays with no card to show it (the visitor's video and the loop)
        void CardClosed();
    }
}
