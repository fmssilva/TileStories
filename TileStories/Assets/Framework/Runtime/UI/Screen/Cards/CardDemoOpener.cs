using System;
using System.Collections.Generic;

namespace TileStories
{
    // The developer-only demo card (card_settings.demo_card, _3.1 section 8.4), kept out of PoiCardHost (15.4.2): when its switch is ON, this
    // build allows it (CardDemoRule) and its point is on the wall, that point's card opens by itself at the chosen stop -- again only when the
    // request (point + stop) changes, so never over a card the developer closed on purpose. Off by default; nothing happens in a release build.
    public sealed class CardDemoOpener
    {
        private string _lastRequest = "";

        // Forget the last request (the host was switched off): the next Apply acts on the switch as it stands
        public void Forget() => _lastRequest = "";

        // Act on the wall's demo settings: select the demo point through the bus (never the point already selected: selecting it again would
        // CLEAR it), else `show` the selected point's card again, then put the open card at the demo's stop
        public void Apply(CardSettings settings, IReadOnlyList<POIData> wallPois, bool allowed, string shownPoiId, Action<string> show, PoiCardSheetView sheet)
        {
            var demo = settings?.demo_card;
            string request = CardDemoRule.Request(demo, allowed, wallPois);
            if (request == _lastRequest) return;
            _lastRequest = request;
            if (request.Length == 0) return;
            if (SelectionEventBus.CurrentPoiId != demo.poi_id) SelectionEventBus.Select(demo.poi_id);
            else if (shownPoiId != demo.poi_id) show(demo.poi_id);
            if (sheet != null && sheet.IsOpen) sheet.SetStop(CardDemoRule.StopOf(demo));
        }
    }
}
