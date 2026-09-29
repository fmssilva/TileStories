using UnityEngine;
using UnityEngine.Video;

namespace TileStories
{
    // The video device the card plays through (_3.1 step 9B): the one seam between CardVideoService's decisions and Unity's VideoPlayer.
    // UnityVideoOutput is the app's; DevTools/ManualVideoOutput is the gallery's and the tests' -- a device that decodes nothing, whose
    // clip time moves only when a test says, so no video time is ever a real-time wait. A real player prepares a clip before its first
    // frame, so the device itself says when a frame is there (HasFrame) and when a clip ran out (Ended): "not playing" is not "finished".
    public interface IVideoOutput
    {
        bool IsPlaying { get; }
        // A clip that does not loop reached its end since the last Play
        bool Ended { get; }
        // Seconds into the loaded clip (a seek before the clip is prepared is applied once it is)
        float Time { get; set; }
        // A frame of the loaded clip is in Texture
        bool HasFrame { get; }
        // What the device draws into (null before the first clip)
        Texture Texture { get; }
        // No sound (a header loop)
        bool Muted { get; set; }
        bool Looping { get; set; }
        // Make `clip` the loaded clip, at its start, not playing
        void Load(VideoClip clip);
        void Play();
        void Pause();
        // Stop and forget the clip
        void Stop();
    }
}
