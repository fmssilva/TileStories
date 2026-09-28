using System.Text;
using UnityEngine;

namespace TileStories.Editor
{
    // Live Play Mode updates for the POI Detail Card (_3.1 step 12): the wall's card settings (container, Block Library, Card Texts, Glossary,
    // the developer-only demo card) and every POI's own card. WallSession.ApplyCardSettings swaps them into the running wall and the open card
    // (PoiCardHost.Rebind) shows the new data with its stop and scroll kept. It owns ONLY the card's fields: a POI's name, category, summary and
    // keywords belong to the Marker and Search appliers, whose fingerprints never see a card edit.
    public class LivePlayModeCardApplier : ILivePlayModeApplier
    {
        public string Name => "detail card";

        public string Fingerprint(WallConfigData config)
        {
            var sb = new StringBuilder();
            sb.Append(JsonUtility.ToJson(config.card_settings ?? new CardSettings(), false));
            if (config.pois != null)
                foreach (var p in config.pois)
                    if (p != null) sb.Append('|').Append(p.id).Append('=').Append(JsonUtility.ToJson(p.card ?? new POICardData(), false));
            return sb.ToString();
        }

        // Hand the running wall its own copy so it can never mutate the authoring config
        public void Apply(WallSession session, WallConfigData configCopy) => session.ApplyCardSettings(configCopy);
    }
}
