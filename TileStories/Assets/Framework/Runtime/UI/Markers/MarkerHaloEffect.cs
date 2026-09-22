using UnityEngine;
using UnityEngine.UI;

namespace TileStories
{
    // One extra layer behind the symbol, in three variants that each have their own parameter
    // block in EffectDefaults: Ring (thin ring, breathing), Disc (filled disc, breathing) and
    // Beacon (thin ring that grows and fades, then restarts: a single outward wave). Shares
    // sprite generation with MarkerRippleEffect via MarkerCircleSpriteFactory -- this class
    // never builds a Texture2D itself.
    public class MarkerHaloEffect : MarkerEffect
    {
        public enum HaloVariant { Ring, Disc, Beacon }

        [SerializeField] private RectTransform target;
        [SerializeField] private Image haloImage;
        [SerializeField] private HaloVariant variant = HaloVariant.Ring;
        [SerializeField] private Color baseTint = new Color(0.949f, 0.792f, 0.443f, 1f);
        [SerializeField, Range(0f, 1f)] private float baseAlpha = 0.28f;
        [SerializeField, Min(0.1f)] private float size = 1.2f;   // diameter as a multiple of the symbol diameter
        [SerializeField, Min(0.1f)] private float period = 2.0f;

        // Ring and Beacon draw a thin ring between these radii; Disc draws a filled disc.
        [SerializeField, Range(0.72f, 0.98f)] private float ringOuterScale = 0.90f;
        [SerializeField, Range(0.5f, 0.9f)] private float ringInnerScale = 0.80f;
        [SerializeField, Range(0.85f, 1f)] private float discRadiusScale = 0.85f;

        // Ring and Disc: gentle scale oscillation at constant alpha.
        [SerializeField, Range(0f, 0.4f)] private float breatheAmplitude = 0.15f;

        // Beacon: grow + fade sawtooth, restarting small and solid each cycle.
        [SerializeField] private float beaconStartScale = 1.0f;
        [SerializeField] private float beaconEndScale = 1.8f;

        private bool _active;
        private Vector3 _baseScale = Vector3.one;
        private bool _baseCaptured;

        public bool IsActive => _active;
        protected override RectTransform SizeReference => target;
        public HaloVariant CurrentVariant => variant;

        // Point the halo at the symbol and choose its variant. Called by MarkerView
        // when a halo effect flag is set.
        public void Configure(RectTransform configuredTarget, HaloVariant haloVariant)
        {
            target = configuredTarget;
            variant = haloVariant;
            if (target == null) return;

            // Authored scale is read once: Configure runs on every Initialise, when the
            // target may already be mid-pulse (see MarkerPulseEffect.CaptureBaseScale).
            if (!_baseCaptured)
            {
                _baseScale = target.localScale;
                _baseCaptured = true;
            }
            EnsureImage();
        }

        // Apply one variant's parameter block from EffectDefaults. Safe no-op when null
        // (the compiled-in [SerializeField] values are used instead). Values are clamped
        // to the same limits as the Inspector attributes above.
        public void ApplyDefaults(EffectDefaults.HaloRingDefaults defaults)
        {
            if (defaults == null) return;
            ApplyCommon(defaults.size, defaults.base_alpha, defaults.period, defaults.tint_color_hex);
            breatheAmplitude = Mathf.Clamp(defaults.breathe_amplitude, 0f, 0.4f);
            ringOuterScale = defaults.outer_scale;
            ringInnerScale = defaults.inner_scale;
        }

        public void ApplyDefaults(EffectDefaults.HaloDiscDefaults defaults)
        {
            if (defaults == null) return;
            ApplyCommon(defaults.size, defaults.base_alpha, defaults.period, defaults.tint_color_hex);
            breatheAmplitude = Mathf.Clamp(defaults.breathe_amplitude, 0f, 0.4f);
            discRadiusScale = defaults.radius_scale;
        }

        public void ApplyDefaults(EffectDefaults.BeaconDefaults defaults)
        {
            if (defaults == null) return;
            ApplyCommon(defaults.size, defaults.base_alpha, defaults.period, defaults.tint_color_hex);
            beaconStartScale = defaults.start_scale;
            beaconEndScale = defaults.end_scale;
            ringOuterScale = defaults.outer_scale;
            ringInnerScale = defaults.inner_scale;
        }

        private void ApplyCommon(float newSize, float alpha, float newPeriod, string tintHex)
        {
            size = newSize;
            baseAlpha = Mathf.Clamp01(alpha);
            period = Mathf.Max(MinPeriodSeconds, newPeriod);
            if (!string.IsNullOrEmpty(tintHex))
                ColorUtility.TryParseHtmlString(tintHex, out baseTint);
        }

        public override void SetActive(bool active)
        {
            _active = active;
            EnsureImage();
            if (haloImage == null) return;
            haloImage.enabled = active;
            if (!active) ResetVisual();
        }

        private void Update()
        {
            if (!_active || haloImage == null) return;

            if (variant == HaloVariant.Beacon)
            {
                float t = Mathf.Repeat(Time.time / period, 1f);
                float smooth = t * t * (3f - 2f * t);
                float scale = Mathf.Lerp(beaconStartScale, beaconEndScale, smooth);
                float fade = Mathf.Lerp(baseAlpha, 0f, smooth);
                haloImage.rectTransform.localScale = _baseScale * scale;
                var c = baseTint;
                c.a = fade;
                haloImage.color = c;
            }
            else
            {
                float wave = Mathf.Sin(Time.time * Mathf.PI * 2f / period) * 0.5f + 0.5f;
                float scale = 1f + breatheAmplitude * wave;
                haloImage.rectTransform.localScale = _baseScale * scale;
                var c = baseTint;
                c.a = baseAlpha;
                haloImage.color = c;
            }
        }

        private void ResetVisual()
        {
            if (haloImage == null) return;
            haloImage.rectTransform.localScale = _baseScale;
            var c = baseTint;
            c.a = variant == HaloVariant.Beacon ? 0f : baseAlpha;
            haloImage.color = c;
        }

        private void EnsureImage()
        {
            if (target == null) return;

            if (haloImage == null)
            {
                var existing = transform.Find("Halo");
                if (existing != null)
                {
                    haloImage = existing.GetComponent<Image>() ?? existing.gameObject.AddComponent<Image>();
                }
                else
                {
                    var go = new GameObject("Halo", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    var rect = (RectTransform)go.transform;
                    rect.SetParent(transform, false);
                    rect.SetSiblingIndex(0);
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    haloImage = go.GetComponent<Image>();
                }
            }

            haloImage.rectTransform.sizeDelta = Vector2.one * size * SymbolDiameter;
            haloImage.raycastTarget = false;
            haloImage.preserveAspect = true;
            haloImage.type = Image.Type.Simple;
            haloImage.sprite = variant == HaloVariant.Disc
                ? MarkerCircleSpriteFactory.GetFilled(discRadiusScale)
                : MarkerCircleSpriteFactory.GetRing(ringOuterScale, ringInnerScale);
            haloImage.enabled = false;
        }
    }
}
