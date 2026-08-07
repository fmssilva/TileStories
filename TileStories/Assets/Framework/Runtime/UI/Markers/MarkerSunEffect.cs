using UnityEngine;
using UnityEngine.UI;

namespace TileStories
{
    // Hero accent: three concentric waves with center-first flow.
    // Supports both contour-ring and filled-circle rendering so we can compare variants.
    public class MarkerSunEffect : MarkerEffect
    {
        public enum SunVisualStyle
        {
            Contours,
            FilledCircles,
        }

        [SerializeField] private RectTransform target;
        [SerializeField] private Image innerImage;
        [SerializeField] private Image middleImage;
        [SerializeField] private Image outerImage;
        [SerializeField] private SunVisualStyle renderStyle = SunVisualStyle.Contours;
        [SerializeField] private Color baseTint = new Color(0.949f, 0.792f, 0.443f, 1f);
        [SerializeField, Range(0.05f, 0.5f)] private float period = 1.8f;
        [SerializeField, Range(0.0f, 0.25f)] private float stagger = 0.12f;
        [SerializeField, Range(0f, 1f)] private float innerAlpha = 0.55f;
        [SerializeField, Range(0f, 1f)] private float middleAlpha = 0.36f;
        [SerializeField, Range(0f, 1f)] private float outerAlpha = 0.2f;

        private bool _active;
        private Vector3 _baseScale = Vector3.one;

        public bool IsActive => _active;
        public SunVisualStyle CurrentStyle => renderStyle;

        public void SetVisualStyle(SunVisualStyle style)
        {
            if (renderStyle == style)
                return;

            renderStyle = style;
            EnsureCircles();
        }

        public void Configure(RectTransform configuredTarget)
        {
            target = configuredTarget;
            if (target == null)
                return;

            _baseScale = target.localScale;
            EnsureCircles();
        }

        // Apply per-wall effect defaults from EffectDefaults.
        // Called by MarkerView when effect_defaults is present in the wall config;
        // safe no-op when null (compiled-in [SerializeField] defaults are used instead).
        public void ApplyDefaults(EffectDefaults.SunDefaults defaults)
        {
            if (defaults == null) return;
            period = defaults.period;
            stagger = defaults.stagger;
            innerAlpha = defaults.innerAlpha;
            middleAlpha = defaults.middleAlpha;
            outerAlpha = defaults.outerAlpha;
            if (!string.IsNullOrEmpty(defaults.tint_color_hex))
                ColorUtility.TryParseHtmlString(defaults.tint_color_hex, out baseTint);
        }

        public override void SetActive(bool active)
        {
            _active = active;
            EnsureCircles();

            if (innerImage == null || middleImage == null || outerImage == null)
                return;

            innerImage.enabled = active;
            middleImage.enabled = active;
            outerImage.enabled = active;

            if (!active)
            {
                ResetCircle(innerImage, 0.0f);
                ResetCircle(middleImage, 0.0f);
                ResetCircle(outerImage, 0.0f);
            }
        }

        private void Update()
        {
            if (!_active || target == null)
                return;

            // Center-first flow: inner starts first, then middle, then outer.
            AnimateCircle(innerImage, 0f, innerAlpha, 1.03f, 1.16f);
            AnimateCircle(middleImage, stagger, middleAlpha, 1.10f, 1.36f);
            AnimateCircle(outerImage, stagger * 2f, outerAlpha, 1.20f, 1.62f);
        }

        private void EnsureCircles()
        {
            if (target == null)
                return;

            innerImage = EnsureCircle("SunInner", 0.16f, SpriteKind.Inner);
            middleImage = EnsureCircle("SunMiddle", 0.20f, SpriteKind.Middle);
            outerImage = EnsureCircle("SunOuter", 0.24f, SpriteKind.Outer);
        }

        private Image EnsureCircle(string name, float size, SpriteKind kind)
        {
            var existing = transform.Find(name);
            Image image;
            if (existing != null)
            {
                image = existing.GetComponent<Image>() ?? existing.gameObject.AddComponent<Image>();
            }
            else
            {
                var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                var rect = (RectTransform)go.transform;
                rect.SetParent(transform, false);
                rect.SetSiblingIndex(0);
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(size, size);
                image = go.GetComponent<Image>();
            }

            image.rectTransform.sizeDelta = new Vector2(size, size);
            image.raycastTarget = false;
            image.preserveAspect = true;
            image.type = Image.Type.Simple;
            image.sprite = ResolveSprite(kind);
            image.color = new Color(baseTint.r, baseTint.g, baseTint.b, 0f);
            image.enabled = false;
            return image;
        }

        // Sprite generation and its domain-reload-safe caching live in the shared
        // MarkerCircleSpriteFactory (section 19.3) -- this class never builds a
        // Texture2D itself, so the section 18.12 stale-static-cache fix only has
        // to exist in one place.
        private Sprite ResolveSprite(SpriteKind kind)
        {
            if (renderStyle == SunVisualStyle.FilledCircles)
                return MarkerCircleSpriteFactory.GetFilled(0.84f);

            return kind switch
            {
                SpriteKind.Inner => MarkerCircleSpriteFactory.GetRing(0.86f, 0.58f),
                SpriteKind.Middle => MarkerCircleSpriteFactory.GetRing(0.84f, 0.66f),
                _ => MarkerCircleSpriteFactory.GetRing(0.82f, 0.72f),
            };
        }

        private void AnimateCircle(Image image, float phaseDelay, float alpha, float startScale, float endScale)
        {
            if (image == null)
                return;

            float cycle = Mathf.Repeat(Time.time / period, 1f);
            float t = Mathf.Repeat(cycle - phaseDelay, 1f);
            float smooth = t * t * (3f - 2f * t);
            float scale = Mathf.Lerp(startScale, endScale, smooth);
            float fade = Mathf.Lerp(alpha, 0f, smooth);

            image.rectTransform.localScale = _baseScale * scale;
            var c = image.color;
            c.r = baseTint.r;
            c.g = baseTint.g;
            c.b = baseTint.b;
            c.a = fade;
            image.color = c;
            image.enabled = _active;
        }

        private void ResetCircle(Image image, float alpha)
        {
            if (image == null)
                return;

            image.rectTransform.localScale = _baseScale;
            var c = image.color;
            c.r = baseTint.r;
            c.g = baseTint.g;
            c.b = baseTint.b;
            c.a = alpha;
            image.color = c;
        }

        private enum SpriteKind
        {
            Inner,
            Middle,
            Outer,
        }
    }
}
