using UnityEngine;

namespace TileStories
{
    // Shared design tokens for the UI Toolkit overlay surfaces (spec _2.6-af
    // design-token decision, 2026-09-08). Before this class existed the results
    // list and filter tray authored text colors but left backgrounds
    // theme-resolved, so no deterministic WCAG contrast pair existed to assert
    // against. These constants give those overlays the same visual language
    // DetailCardView already uses (0.85-black panel + white text) and make the
    // pair assertable in Tier-0.5 tests (SelectFilterAccessibilityTests).
    // Runtime assembly so the views can use them without an Editor dependency.
    public static class UIPalette
    {
        // Dark translucent panel surface used by overlay chrome
        // (results list, filter tray). Matches DetailCardView's panel color.
        public static readonly Color SurfaceDark = new Color(0f, 0f, 0f, 0.85f);

        // Primary text on SurfaceDark (result names, section headers).
        public static readonly Color TextPrimary = Color.white;

        // Secondary text on SurfaceDark (result categories, empty-state hints).
        // 0.78 grey keeps >= 4.5:1 against SurfaceDark even when the translucent
        // panel is composited over a worst-case bright camera feed (verified in
        // SelectFilterAccessibilityTests against both black and white bases).
        public static readonly Color TextSecondary = new Color(0.78f, 0.78f, 0.78f);
    }
}