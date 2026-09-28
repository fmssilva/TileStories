using UnityEngine;

namespace TileStories
{
    // The default card events sink while there is no backend: one log line per event, in the Editor and development builds
    // (a release build says nothing). Replaced by the telemetry implementation of ICardEvents when Stage 3 builds it.
    public sealed class LogCardEvents : ICardEvents
    {
        public void Raise(CardEvent cardEvent)
        {
            if (!Application.isEditor && !Debug.isDebugBuild) return;
            Debug.Log("[Card] event " + cardEvent.Kind + " wall=" + cardEvent.WallId + " poi=" + cardEvent.PoiId + " block=" + cardEvent.BlockKey
                + " variant=" + cardEvent.Variant + " value=" + cardEvent.Value);
        }
    }
}
