using UnityEngine;
using UnityEngine.UI;

namespace TileStories
{
    // Three staggered waves flowing outward from the symbol, centre first. Two looks
    // (RippleStyle): thin rings or filled discs; each has its own parameter block in
    // EffectDefaults. A single-wave version of the same motion is MarkerHaloEffect's Beacon.
    public class MarkerRippleEffect : MarkerEffect
    {
        public enum RippleStyle
        {
            Rings,
            Discs,
        }

        [SerializeField] private RectTransform target;
        [SerializeField] private Image innerImage;
        [SerializeField] private Image middleImage;
        [SerializeField] private Image outerImage;
        [SerializeField] private RippleStyle style = RippleStyle.Rings;
        [SerializeField] private Color baseTint = new Color(0.949f, 0.792f, 0.443f, 1f);
        [SerializeField, Min(0.1f)] private float period = 1.8f;
        [SerializeField, Range(0.0f, 0.25f)] private float stagger = 0.12f;
        [SerializeField, Range(0f, 1f)] private float innerAlpha = 0.55f;
        [SerializeField, Range(0f, 1f)] private float middleAlpha = 0.36f;
        [SerializeField, Range(0f, 1f)] private float outerAlpha = 0.2f;

        private bool _active;
        private Vector3 _baseScale = Vector3.one;
        private bool _baseCaptured;

        public bool IsActive => _active;
        protected override RectTransform SizeReference => target;
        public RippleStyle CurrentStyle => style;

        public void SetStyle(RippleStyle newStyle)
        {
            if (style == newStyle)
                return;

            style = newStyle;
            EnsureCircles();
        }

        public void Configure(RectTransform configuredTarget)
        {
            target = configuredTarget;
            if (target == null)
                return;

            // Authored scale is read once: Configure runs on every Initialise, when the
            // target may already be mid-pulse (see MarkerPulseEffect.CaptureBaseScale).
            if (!_baseCaptured)
            {
                _baseScale = target.localScale;
                _baseCaptured = true;
            }
            EnsureCircles();
        }

        // Apply this style's parameter block from EffectDefaults. Safe no-op when null
        // (the compiled-in [SerializeField] values are used instead). Values are clamped to
        // the same limits as the Inspector attributes above.
        public void ApplyDefaults(EffectDefaults.RippleDefaults defaults)
        {
            if (defaults == null) return;
            period = Mathf.Max(MinPeriodSeconds, defaults.period);
            stagger = Mathf.Clamp(defaults.stagger, 0f, 0.25f);
            innerAlpha = Mathf.Clamp01(defaults.inner_alpha);
            middleAlpha = Mathf.Clamp01(defaults.middle_alpha);
            outerAlpha = Mathf.Clamp01(defaults.outer_alpha);
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

            // Centre-first flow: inner starts first, then middle, then outer.
            AnimateCircle(innerImage, 0f, innerAlpha, 1.03f, 1.16f);
            AnimateCircle(middleImage, stagger, middleAlpha, 1.10f, 1.36f);
            AnimateCircle(outerImage, stagger * 2f, outerAlpha, 1.20f, 1.62f);
        }

        private void EnsureCircles()
        {
            if (target == null)
                return;

            innerImage = EnsureCircle("RippleInner", 0.8f, SpriteKind.Inner);
            middleImage = EnsureCircle("RippleMiddle", 1.0f, SpriteKind.Middle);
            outerImage = EnsureCircle("RippleOuter", 1.2f, SpriteKind.Outer);
        }

        private Image EnsureCircle(string name, float sizeInSymbols, SpriteKind kind)
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
                rect.sizeDelta = Vector2.one * sizeInSymbols * SymbolDiameter;
                image = go.GetComponent<Image>();
            }

            image.rectTransform.sizeDelta = Vector2.one * sizeInSymbols * SymbolDiameter;
            image.raycastTarget = false;
            image.preserveAspect = true;
            image.type = Image.Type.Simple;
            image.sprite = ResolveSprite(kind);
            image.color = new Color(baseTint.r, baseTint.g, baseTint.b, 0f);
            image.enabled = false;
            return image;
        }

        // Sprite generation and its domain-reload-safe caching live in the shared
        // MarkerCircleSpriteFactory -- this class never builds a Texture2D itself, so the
        // stale-static-cache fix (archive section 18.12) only has to exist in one place.
        private Sprite ResolveSprite(SpriteKind kind)
        {
            if (style == RippleStyle.Discs)
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
