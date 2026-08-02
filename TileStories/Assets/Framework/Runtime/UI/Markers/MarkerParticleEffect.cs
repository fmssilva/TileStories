using UnityEngine;

namespace TileStories
{
    // Optional particle wrapper. Not used by any of the three built-in styles,
    // but included for future extension (e.g. a "discovery burst" effect that
    // could be added later without touching MarkerView).
    public class MarkerParticleEffect : MarkerEffect
    {
        [SerializeField] private ParticleSystem particles;

        public override void SetActive(bool active)
        {
            if (particles == null) return;
            if (active) particles.Play();
            else particles.Stop();
        }
    }
}