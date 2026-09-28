using System.Collections.Generic;

namespace TileStories
{
    // The developer-only demo card (_3.1 step 12, 20-code-quality.md: developer-only features never reach the real app): decides, from
    // plain data, WHETHER the demo may run at all and WHICH card it asks for. Pure, so a test can call it with any inputs; PoiCardHost
    // does the opening. Off by default; the Editor and development builds only, never a release build.
    public static class CardDemoRule
    {
        // Whether the running build may show the demo card: the Editor or a development build (the one small rule the switch is gated by,
        // like EffectsPreviewSpawner.IsAllowed). Release builds ignore a demo left ON in the shipped config.
        public static bool IsAllowed(bool isEditor, bool isDebugBuild) => isEditor || isDebugBuild;

        // The POI the demo asks to open: null when the demo is off, not allowed in this build, has no POI picked, or the POI is not on the wall
        public static string PoiToOpen(CardDemoSettings demo, bool allowed, IReadOnlyList<POIData> pois)
        {
            if (!allowed || demo == null || !demo.enabled || string.IsNullOrEmpty(demo.poi_id) || pois == null) return null;
            for (int i = 0; i < pois.Count; i++)
                if (pois[i] != null && pois[i].id == demo.poi_id) return demo.poi_id;
            return null;
        }

        // The stop the demo card rests at ("peek" / "half" / "full"; anything else opens at half, the middle one)
        public static SheetStopRule.Stop StopOf(CardDemoSettings demo) => (demo?.stop) switch
        {
            CardOptions.StopPeek => SheetStopRule.Stop.Peek,
            CardOptions.StopFull => SheetStopRule.Stop.Full,
            _ => SheetStopRule.Stop.Half,
        };

        // What the demo currently asks for, as one comparable text ("" = nothing). The host opens the card again only when this changes, so a
        // live edit elsewhere never yanks the card the developer is looking at back to the demo's stop.
        public static string Request(CardDemoSettings demo, bool allowed, IReadOnlyList<POIData> pois)
        {
            string poi = PoiToOpen(demo, allowed, pois);
            return poi == null ? "" : poi + "@" + StopOf(demo);
        }
    }
}
