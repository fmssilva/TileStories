using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // Applies safe-area insets to the root VisualElement of a UI Toolkit panel.
    // UI Toolkit panels are screen-overlay / screen-space; on notched devices the
    // OS safe-area excludes the notch, so root padding must follow Screen.safeArea.
    // Screen.safeArea is bottom-left-origin in screen pixels; UI Toolkit is top-left,
    // hence the origin inversion in ComputePadding (Unity docs, Screen.safeArea).
    // Pure math is exposed via ComputePadding so it is Tier-0 editable without a device.
    public static class SafeAreaHelper
    {
        // Pixel padding resolved from a safe-area rect + total screen size. Pure (no Screen read).
        public readonly struct SafeAreaPadding
        {
            public readonly float left;
            public readonly float top;
            public readonly float right;
            public readonly float bottom;

            public SafeAreaPadding(float left, float top, float right, float bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            // True when the safe area is already edge-to-edge (no insets to apply).
            public bool IsEmpty => left == 0f && top == 0f && right == 0f && bottom == 0f;
        }

        // Convert a bottom-left-origin safeArea into a top-left-origin UI Toolkit padding.
        // Bottom offset (safeArea.y) becomes top padding; top inset (screenH - (y+h)) becomes
        // bottom padding. This is the origin inversion the comment above warns about.
        public static SafeAreaPadding ComputePadding(Rect safeArea, Vector2 screenSize)
        {
            float left = safeArea.x;
            float right = screenSize.x - (safeArea.x + safeArea.width);
            // bottom-left origin (safeArea.y) -> UI Toolkit top padding
            float top = screenSize.y - (safeArea.y + safeArea.height);
            float bottom = safeArea.y;
            // Clamp away float noise from fractional insets.
            return new SafeAreaPadding(
                Mathf.Max(0f, left),
                Mathf.Max(0f, top),
                Mathf.Max(0f, right),
                Mathf.Max(0f, bottom));
        }

        // Current device safe-area padding (reads Screen live).
        public static SafeAreaPadding GetCurrent()
        {
            return ComputePadding(Screen.safeArea, new Vector2(Screen.width, Screen.height));
        }

        // Pad the root element of a UIDocument panel once at init; idempotent re-apply is fine.
        public static void ApplyToRoot(VisualElement root)
        {
            if (root == null) return;
            SafeAreaPadding p = GetCurrent();
            root.style.paddingLeft = new Length(p.left, LengthUnit.Pixel);
            root.style.paddingTop = new Length(p.top, LengthUnit.Pixel);
            root.style.paddingRight = new Length(p.right, LengthUnit.Pixel);
            root.style.paddingBottom = new Length(p.bottom, LengthUnit.Pixel);
        }
    }
}
