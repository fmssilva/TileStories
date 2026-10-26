using UnityEngine;
using UnityEngine.Video;

namespace TileStories
{
    // The MonoBehaviour side of the card's video (_3.1 step 9B), thin on purpose, like CardAudioPlayer: it owns the ONE VideoPlayer (and the
    // output's RenderTexture), gives the service its per-frame Tick and forwards the app going to the background. Every decision is
    // CardVideoService's.
    [RequireComponent(typeof(VideoPlayer))]
    public sealed class CardVideoPlayer : MonoBehaviour
    {
        // The service this player ticks (PoiCardHost builds it; a test may install its own over a ManualVideoOutput)
        public CardVideoService Service { get; set; }

        private UnityVideoOutput _output;

        // The VideoPlayer wrapped as the service's video device (one per player)
        public UnityVideoOutput CreateOutput() => _output ??= new UnityVideoOutput(GetComponent<VideoPlayer>());

        private void Update() => Service?.Tick();

        private void OnApplicationPause(bool paused) => Service?.OnApplicationPause(paused);

        private void OnDestroy() => _output?.Dispose();
    }
}
