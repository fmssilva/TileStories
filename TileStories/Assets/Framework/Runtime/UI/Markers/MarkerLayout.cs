using UnityEngine;

namespace TileStories
{
    // Proportions, not pixels. Every offset is relative to the Symbol's own size,
    // so resizing the Symbol (Inspector, or later at runtime for LOD) re-derives
    // ring/badge/label correctly instead of drifting out of position. Defaults
    // match the Stage 2.3 HTML prototype ratios (ring 44 vs symbol 40 = 1.1x,
    // badge ~0.36x) so the browser prototype and shipped markers agree.
    [System.Serializable]
    public class MarkerLayoutProportions
    {
        [Range(1f, 1.35f)] public float ringSizeRatio = 1.18f;
        [Range(0.2f, 0.5f)] public float badgeSizeRatio = 0.36f;

        // Normalised direction from Symbol centre to Badge centre. (0.7, 0.7) =
        // outside-overlap, top-right -- the composition chosen after comparing all
        // four in the interactive prototype (see the HTML file's badge section).
        public Vector2 badgeDirection = new Vector2(0.7f, 0.7f);

        public float labelGap = 0.015f; // world units, symbol bottom edge -> label top
    }

    // Pure function of (symbol size, proportions) -> (ring/badge/label rects). No
    // MonoBehaviour state -- easy to unit test, safe to call from both Awake()
    // (runtime) and OnValidate() (live Editor preview without Play mode).
    public static class MarkerLayout
    {
        public static void Apply(
            RectTransform symbol,
            RectTransform ring,
            RectTransform badge,
            RectTransform label,
            MarkerLayoutProportions proportions)
        {
            if (symbol == null || proportions == null) return;

            Vector2 symbolSize = symbol.sizeDelta;
            float symbolRadius = symbolSize.x * 0.5f;

            if (ring != null)
            {
                ring.sizeDelta = symbolSize * proportions.ringSizeRatio;
                ring.anchoredPosition = symbol.anchoredPosition;
            }

            if (badge != null)
            {
                Vector2 badgeSize = symbolSize * proportions.badgeSizeRatio;
                badge.sizeDelta = badgeSize;
                badge.anchoredPosition = symbol.anchoredPosition
                    + Vector2.Scale(proportions.badgeDirection, new Vector2(symbolRadius, symbolRadius));
            }

            if (label != null)
            {
                Vector2 pos = label.anchoredPosition;
                pos.x = symbol.anchoredPosition.x;
                pos.y = symbol.anchoredPosition.y - symbolRadius - proportions.labelGap;
                label.anchoredPosition = pos;
            }
        }
    }
}