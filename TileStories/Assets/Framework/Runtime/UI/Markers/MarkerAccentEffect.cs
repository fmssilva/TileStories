using UnityEngine;
using UnityEngine.UI;

namespace TileStories
{
    // Single reusable accent ring/disc, usable on ANY marker (not hero-gated).
    // Covers SimpleSun (filled, breathing), RingPulse (thin contour, breathing),
    // and Beacon (thin contour, grow+fade sawtooth) from one class + two enums,
    // rather than three near-identical effect files. Shares sprite generation
    // with MarkerSunEffect via MarkerCircleSpriteFactory (section 19.3) -- this
    // class never builds a Texture2D itself.
    public class MarkerAccentEffect : MarkerEffect
    {
        public enum AccentShape { FilledCircle, Contour }
        public enum AccentMotion { Breathe, Beacon }

        [SerializeField] private RectTransform target;
        [SerializeField] private Image accentImage;
        [SerializeField] private AccentShape shape = AccentShape.FilledCircle;
        [SerializeField] private AccentMotion motion = AccentMotion.Breathe;
        [SerializeField] private Color baseTint = new Color(0.949f, 0.792f, 0.443f, 1f);
        [SerializeField, Range(0f, 1f)] private float baseAlpha = 0.28f;
        [SerializeField] private float size = 0.24f;
        [SerializeField, Range(0.72f, 0.98f)] private float contourOuterScale = 0.90f;
        [SerializeField, Range(0.5f, 0.9f)] private float contourInnerScale = 0.80f;
        [SerializeField, Range(0.85f, 1f)] private float filledRadiusScale = 0.84f;

        // Breathe: gentle, always-visible scale oscillation, constant alpha.
        [SerializeField, Range(0f, 0.4f)] private float breatheAmplitude = 0.15f;
        [SerializeField, Min(0.1f)] private float period = 2.0f;

        // Beacon: sawtooth grow+fade, resets to small+solid each cycle. Same
        // shape of animation as MarkerSunEffect.AnimateCircle, deliberately
        // not factored into a shared helper -- see section 19.0's reasoning.
        [SerializeField] private float beaconStartScale = 1.0f;
        [SerializeField] private float beaconEndScale = 1.8f;

        private bool _active;
        private Vector3 _baseScale = Vector3.one;

        public bool IsActive => _active;

        // Configure the accent to render a specific shape and motion on a
        // specific target RectTransform. Called by MarkerView.ApplyHeroState
        // when an accent effect flag is set.
        public void Configure(RectTransform configuredTarget, AccentShape accentShape, AccentMotion accentMotion)
        {
            target = configuredTarget;
            shape = accentShape;
            motion = accentMotion;
            if (target == null) return;
            _baseScale = target.localScale;
            EnsureImage();
        }

        public override void SetActive(bool active)
        {
            _active = active;
            EnsureImage();
            if (accentImage == null) return;
            accentImage.enabled = active;
            if (!active) ResetVisual();
        }

        private void Update()
        {
            if (!_active || accentImage == null) return;

            if (motion == AccentMotion.Breathe)
            {
                float wave = Mathf.Sin(Time.time * Mathf.PI * 2f / period) * 0.5f + 0.5f;
                float scale = 1f + breatheAmplitude * wave;
                accentImage.rectTransform.localScale = _baseScale * scale;
                var c = baseTint;
                c.a = baseAlpha;
                accentImage.color = c;
            }
            else // Beacon
            {
                float t = Mathf.Repeat(Time.time / period, 1f);
                float smooth = t * t * (3f - 2f * t);
                float scale = Mathf.Lerp(beaconStartScale, beaconEndScale, smooth);
                float fade = Mathf.Lerp(baseAlpha, 0f, smooth);
                accentImage.rectTransform.localScale = _baseScale * scale;
                var c = baseTint;
                c.a = fade;
                accentImage.color = c;
            }
        }

        private void ResetVisual()
        {
            if (accentImage == null) return;
            accentImage.rectTransform.localScale = _baseScale;
            var c = baseTint;
            c.a = motion == AccentMotion.Breathe ? baseAlpha : 0f;
            accentImage.color = c;
        }

        private void EnsureImage()
        {
            if (target == null) return;

            if (accentImage == null)
            {
                var existing = transform.Find("Accent");
                if (existing != null)
                {
                    accentImage = existing.GetComponent<Image>() ?? existing.gameObject.AddComponent<Image>();
                }
                else
                {
                    var go = new GameObject("Accent", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    var rect = (RectTransform)go.transform;
                    rect.SetParent(transform, false);
                    rect.SetSiblingIndex(0);
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    accentImage = go.GetComponent<Image>();
                }
            }

            accentImage.rectTransform.sizeDelta = new Vector2(size, size);
            accentImage.raycastTarget = false;
            accentImage.preserveAspect = true;
            accentImage.type = Image.Type.Simple;
            accentImage.sprite = shape == AccentShape.FilledCircle
                ? MarkerCircleSpriteFactory.GetFilled(filledRadiusScale)
                : MarkerCircleSpriteFactory.GetRing(contourOuterScale, contourInnerScale);
            accentImage.enabled = false;
        }
    }
}
