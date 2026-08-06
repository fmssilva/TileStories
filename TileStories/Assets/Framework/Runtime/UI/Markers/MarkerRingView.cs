using System;
using UnityEngine;
using UnityEngine.UI;

namespace TileStories
{
    // Renders the outline ring for MarkerStyle.OutlineGold/OutlineSameHue. Dash
    // patterns are pre-made sprites swapped by key -- uGUI cannot draw a dashed
    // circle procedurally without a custom shader; this is the same low-tech
    // sprite-swap pattern used elsewhere in the project.
    public class MarkerRingView : MonoBehaviour
    {
        [SerializeField] private Image ringImage;
        [SerializeField] private RingSpriteSet spriteSet;

        // Extensible line-style lookup (section 20.3). When assigned, this takes
        // precedence over the fixed 5-slot spriteSet -- a wall can author a custom
        // line style (e.g. "line_wavy") that resolves from its icon library. The
        // five built-in keys (solid/dash_long/dash_medium/dash_short/dotted) are
        // registered in the same library under those exact keys.
        [SerializeField] private SpriteKeyLibrary lineStyleLibrary;

        public RectTransform RectTransform => (RectTransform)transform;

        // Push the active wall icon library into the ring view so custom line
        // styles resolve from the same library that serves icons and shapes.
        public void SetLineStyleLibrary(SpriteKeyLibrary library) => lineStyleLibrary = library;

        public void Apply(StatusLevel level, Color? ringColorOverride = null)
        {
            if (ringImage == null) return;
            ringImage.enabled = true;
            ringImage.color = ringColorOverride ?? level.RingColor;
            Sprite ringSprite = ResolveLineSprite(level.RingSpriteKey);
            if (ringSprite != null)
                ringImage.sprite = ringSprite;
        }

        // Resolve a line-style sprite: prefer the extensible library, fall back to
        // the fixed spriteSet, then to solid if neither has the key.
        private Sprite ResolveLineSprite(string key)
        {
            if (lineStyleLibrary != null)
            {
                Sprite fromLibrary = lineStyleLibrary.Get(key);
                if (fromLibrary != null)
                    return fromLibrary;
            }

            if (spriteSet != null)
            {
                Sprite fromSet = spriteSet.Get(key);
                if (fromSet != null)
                    return fromSet;
            }

            return spriteSet != null ? spriteSet.solid : null;
        }

        public void Hide()
        {
            if (ringImage != null) ringImage.enabled = false;
        }

        [SerializeField, Range(0f, 360f)] private float rotationDegreesPerSecond = 60f;
        private bool _rotating;

        // Enable/disable the ring's continuous rotation. Only meaningful when
        // the ring is visible -- ApplyVisuals gates this on showRing.
        public void SetRotating(bool rotating) => _rotating = rotating;

        private void Update()
        {
            if (!_rotating || ringImage == null || !ringImage.enabled) return;
            // Negative sign: Unity's positive Z-rotation is counter-clockwise
            // facing the camera on a standard uGUI canvas; clockwise reads more
            // natural for a "scanning" ring. Will be confirmed visually in
            // Phase A (section 18.10).
            ringImage.rectTransform.Rotate(0f, 0f, -rotationDegreesPerSecond * Time.deltaTime);
        }
    }

    [System.Serializable]
    public class RingSpriteSet
    {
        public Sprite solid;
        public Sprite dashLong;
        public Sprite dashMedium;
        public Sprite dashShort;
        public Sprite dotted;

        public Sprite Get(string key)
        {
            switch (key)
            {
                case "solid": return solid;
                case "dash_long": return dashLong;
                case "dash_medium": return dashMedium;
                case "dash_short": return dashShort;
                case "dotted": return dotted;
                default: return solid;
            }
        }
    }
}