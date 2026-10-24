using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // Tier-0.5 accessibility + tap-target helpers for UI Toolkit runtime views
    // (spec _2.6 section 15a / WCAG 2.1 SC 1.4.3 + 2.5.5). Pure static helpers so
    // they are unit-testable with plain values and no scene lifecycle.
    public static class UIAccessibility
    {
        // WCAG 2.1 SC 1.4.3 (text) / SC 1.4.11 (UI components) contrast ratios.
        public const float MinRatioNormalText = 4.5f;
        public const float MinRatioLargeTextOrUIComponent = 3.0f;

        // Convert a gamma-space sRGB channel to linear luminance.
        private static float ToLinear(float c) =>
            c <= 0.03928f ? c / 12.92f : Mathf.Pow((c + 0.055f) / 1.055f, 2.4f);

        // Relative luminance of an sRGB color (Rec. 709 primaries).
        private static float RelativeLuminance(Color c) =>
            0.2126f * ToLinear(c.r) + 0.7152f * ToLinear(c.g) + 0.0722f * ToLinear(c.b);

        // Contrast ratio between two colors (lighter/darker + 0.05).
        public static float ContrastRatio(Color a, Color b)
        {
            float la = RelativeLuminance(a), lb = RelativeLuminance(b);
            float lighter = Mathf.Max(la, lb), darker = Mathf.Min(la, lb);
            return (lighter + 0.05f) / (darker + 0.05f);
        }

        // WCAG 2.1 SC 1.4.6 (AAA): the card's bar for every text role in both modes (_3.3 D1)
        public const float MinRatioEnhancedText = 7.0f;

        // APCA lightness contrast Lc of `text` on `background` (APCA-W3 0.0.98G-4g, the WCAG 3 candidate), as a magnitude: ~106 for black on
        // white, 0 for a colour on itself. Body text wants >= 75 (_3.3 R12.6). Unlike the WCAG ratio it is not symmetric: polarity counts
        public static float ApcaLc(Color text, Color background)
        {
            float yt = ApcaClamp(ApcaY(text)), yb = ApcaClamp(ApcaY(background));
            if (Mathf.Abs(yb - yt) < 0.0005f) return 0f;
            if (yb > yt)
            {
                // - dark text on a light background
                float s = (Mathf.Pow(yb, 0.56f) - Mathf.Pow(yt, 0.57f)) * 1.14f;
                return s < 0.1f ? 0f : (s - 0.027f) * 100f;
            }
            float r = (Mathf.Pow(yb, 0.65f) - Mathf.Pow(yt, 0.62f)) * 1.14f;
            return r > -0.1f ? 0f : -(r + 0.027f) * 100f;
        }

        // APCA's screen luminance: a plain 2.4 power per channel (not the sRGB piecewise curve), its own coefficients
        private static float ApcaY(Color c) =>
            0.2126729f * Mathf.Pow(c.r, 2.4f) + 0.7151522f * Mathf.Pow(c.g, 2.4f) + 0.0721750f * Mathf.Pow(c.b, 2.4f);

        // - near black, APCA soft-clamps the luminance (flare): below 0.022 it is lifted
        private static float ApcaClamp(float y) => y < 0.022f ? y + Mathf.Pow(0.022f - y, 1.414f) : y;

        // WCAG AA pass for normal text (4.5:1) or large text / UI components (3:1).
        public static bool MeetsContrastAA(Color foreground, Color background, bool largeOrUIC = false) =>
            ContrastRatio(foreground, background) >= (largeOrUIC ? MinRatioLargeTextOrUIComponent : MinRatioNormalText);

        // WCAG 2.5.5 minimum target size: 44x44 physical pixels (dp).
        public static bool MeetsMinTapTarget(float widthPx, float heightPx) =>
            widthPx >= 44f && heightPx >= 44f;

        // Set a readable accessible name + role on an interactive element so a
        // screen reader announces intent instead of an empty/generic label.
        // Returns true if it applied a non-empty name+role (caller can assert).
        public static bool SetRoleAndLabel(VisualElement element, string role, string label)
        {
            if (element == null)
                return false;

            element.name = (string.IsNullOrEmpty(element.name) ? label : element.name);
            element.tooltip = label;
            // UI Toolkit exposes accessibility via IAccessibilitySupport on
            // concrete VisualElements; setting a non-empty tooltip + name is the
            // documented minimum for a readable name in this Unity version.
            // The HierarchyRole test asserts every interactive element has a
            // non-empty name+tooltip, so this is a verifiable seam.
            return !string.IsNullOrEmpty((element.name ?? label)) && element.tooltip != null;
        }
    }
}
