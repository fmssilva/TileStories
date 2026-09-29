using UnityEngine;

namespace TileStories
{
    // How an equirect 360 picture maps to directions around the viewer (_3.1 step 10A): the ONE convention the panorama viewer's
    // sphere and the default-library generator both use, so a picture and its viewer never disagree about which way is forward.
    //   u (0..1, left to right) is the yaw: 0.5 looks straight ahead (+Z), growing u turns right (+X), the edges meet behind
    //   v (0..1, bottom to top, a texture's own v) is the pitch: 0.5 is the horizon, 1 straight up, 0 straight down
    // Pure, so both directions are unit tests.
    public static class EquirectRule
    {
        // The unit direction a picture point looks at
        public static Vector3 DirectionOf(float u, float v)
        {
            float yaw = (u - 0.5f) * 2f * Mathf.PI;
            float pitch = (v - 0.5f) * Mathf.PI;
            float c = Mathf.Cos(pitch);
            return new Vector3(Mathf.Sin(yaw) * c, Mathf.Sin(pitch), Mathf.Cos(yaw) * c);
        }

        // The picture point a direction looks at (the reverse of DirectionOf; any length, zero reads as straight ahead)
        public static Vector2 UvOf(Vector3 direction)
        {
            if (direction.sqrMagnitude < 1e-12f) return new Vector2(0.5f, 0.5f);
            var d = direction.normalized;
            float u = 0.5f + Mathf.Atan2(d.x, d.z) / (2f * Mathf.PI);
            float v = 0.5f + Mathf.Asin(Mathf.Clamp(d.y, -1f, 1f)) / Mathf.PI;
            return new Vector2(u, v);
        }

        // An equirect picture is twice as wide as it is tall
        public const float Aspect = 2f;
    }
}
