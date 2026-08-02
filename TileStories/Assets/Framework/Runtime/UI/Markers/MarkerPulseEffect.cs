using UnityEngine;

namespace TileStories
{
    // Gentle scale "breathing" -- cheapest, most universally-readable "worth a
    // look" cue. Hero-tier only (see MarkerView.ApplyHeroState) -- pulsing every
    // marker at once reads as noise, not emphasis.
    public class MarkerPulseEffect : MarkerEffect
    {
        [SerializeField] private RectTransform target;
        [SerializeField, Range(0f, 0.45f)] private float amplitude = 0.18f;
        [SerializeField, Min(0.1f)] private float period = 1.6f;

        private Vector3 _baseScale = Vector3.one;
        private bool _active;

        private void Awake()
        {
            if (target != null) _baseScale = target.localScale;
        }

        public void Configure(RectTransform configuredTarget)
        {
            target = configuredTarget;
            if (target != null)
                _baseScale = target.localScale;
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