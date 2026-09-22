using UnityEngine;

namespace TileStories
{
    // Gentle scale "breathing" -- cheapest, most universally-readable "worth a
    // look" cue. Enabled per hierarchy level (pulse column) -- pulsing every
    // marker at once reads as noise, not emphasis, so authors pick which levels pulse.
    public class MarkerPulseEffect : MarkerEffect
    {
        [SerializeField] private RectTransform target;
        [SerializeField, Range(0f, 0.45f)] private float amplitude = 0.18f;
        [SerializeField, Min(0.1f)] private float period = 1.6f;

        private Vector3 _baseScale = Vector3.one;
        private bool _baseCaptured;
        private bool _active;

        private void Awake()
        {
            CaptureBaseScale();
        }

        public void Configure(RectTransform configuredTarget)
        {
            target = configuredTarget;
            CaptureBaseScale();
        }

        // Read the target's authored scale exactly once. Configure runs on every Initialise, by
        // which time this effect may already be animating the same transform -- reading it again
        // would bake the animated scale in as the new "base" and the marker would never shrink back.
        private void CaptureBaseScale()
        {
            if (_baseCaptured || target == null) return;
            _baseScale = target.localScale;
            _baseCaptured = true;
        }

        // Apply per-wall effect defaults from EffectDefaults.
        // Called by MarkerView when effect_defaults is present in the wall config;
        // safe no-op when null (compiled-in [SerializeField] defaults are used instead).
        // Values are clamped to the same limits as the Inspector attributes above.
        public void ApplyDefaults(EffectDefaults.PulseDefaults defaults)
        {
            if (defaults == null) return;
            amplitude = Mathf.Clamp(defaults.amplitude, 0f, 0.45f);
            period = Mathf.Max(MinPeriodSeconds, defaults.period);
        }

        public override void SetActive(bool active)
        {
            _active = active;
            if (!active && target != null) target.localScale = _baseScale;
        }

        public bool IsActive => _active;

        private void Update()
        {
            if (!_active || target == null) return;
            float wave = Mathf.Sin(Time.time * Mathf.PI * 2f / period) * 0.5f + 0.5f;
            target.localScale = _baseScale * (1f + amplitude * wave);
        }
    }
}
