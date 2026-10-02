using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The one fit of the card's horizontal swipe tracks (the timeline's events, the related carousel's cards; _3.1 15.3.4, 15.4.6): every
    // cell gets CardTrackRule's width for the strip, from the cell's own USS (min-width = the cell token, margin-right = the gap) and -- while
    // Peek Next Card is on -- the slice of the next cell the --ts-track-peek token asks for.
    public static class CardTrackFit
    {
        private static readonly CustomStyleProperty<float> PeekToken = new("--ts-track-peek");

        // Size `cells` for `track`'s strip as it is laid out now (nothing happens before it has a size: the cell token's width shows)
        public static void Apply(ScrollView track, IReadOnlyList<VisualElement> cells, bool peekNext)
        {
            if (cells.Count == 0) return;
            float strip = track.contentViewport.layout.width;
            var style = cells[0].resolvedStyle;
            float narrowest = style.minWidth.value, gap = style.marginRight;
            if (float.IsNaN(strip) || strip <= 0f || float.IsNaN(narrowest) || narrowest <= 0f) return;
            float width = CardTrackRule.CellWidth(strip, narrowest, gap, PeekOf(track, peekNext), cells.Count);
            foreach (var cell in cells) cell.style.width = width;
        }

        // The slice of the next cell `track` shows at rest: the --ts-track-peek token while the option is on, else none. Read on the card's
        // root (.ts-card), where the tokens are declared and PoiCardSheetView reads its own: a custom property is only in the customStyle of
        // the element whose rule sets it (a `var()` copy of it on the track does not arrive there)
        public static float PeekOf(VisualElement track, bool peekNext)
        {
            if (!peekNext) return 0f;
            for (var e = track; e != null; e = e.parent)
                if (e.ClassListContains("ts-card")) return e.customStyle.TryGetValue(PeekToken, out float peek) ? peek : 0f;
            return 0f;
        }
    }
}
