using UnityEngine;
using UnityEngine.UI;

namespace TileStories
{
    // Soft outer-glow halo. Slightly more expensive than pulse (needs a second
    // Image), but still cheap. Hero-tier only.
    public class MarkerGlowEffect : MarkerEffect
    {
        [SerializeField] private Image glowImage;
        [SerializeField, Range(0f, 0.5f)] private float amplitude = 0.25f;
        [SerializeField, Min(0.1f)] private float period = 2.5f;

        private Color _baseColor;
        private bool _active;

        private void Awake()
        {
            if (glowImage != null) _baseColor = glowImage.color;
        }

        public void Configure(Image configuredGlowImage)
        {
            glowImage = configuredGlowImage;
            if (glowImage != null)
                _baseColor = glowImage.color;
        }

        public override void SetActive(bool active)
        {
            _active = active;
            if (glowImage != null)
            {
                var c = _baseColor;
                c.a = active ? 0.5f : 0f;
                glowImage.color = c;
            }
        }

        public bool IsActive => _active;

        private void Update()
        {
            if (!_active || glowImage == null) return;
            float wave = Mathf.Sin(Time.time * Mathf.PI * 2f / period) * 0.5f + 0.5f;
            var c = _baseColor;
            c.a = 0.3f + amplitude * wave;
            glowImage.color = c;
        }
    }
}