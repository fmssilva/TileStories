using System.Globalization;

namespace TileStories
{
    // How a player writes and places a time (_3.1 step 9A), pure.
    public static class AudioTimeRule
    {
        // "3:07" (minutes:seconds, the seconds cut down), "1:02:03" from an hour up, "0:00" for anything below zero or not a number
        public static string Format(float seconds)
        {
            if (float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds < 0f) seconds = 0f;
            int total = (int)seconds;
            int h = total / 3600, m = total / 60 % 60, s = total % 60;
            return h > 0
                ? h.ToString(CultureInfo.InvariantCulture) + ":" + m.ToString("00", CultureInfo.InvariantCulture) + ":" + s.ToString("00", CultureInfo.InvariantCulture)
                : m.ToString(CultureInfo.InvariantCulture) + ":" + s.ToString("00", CultureInfo.InvariantCulture);
        }

        // How far along a clip is, 0..1 (0 when the length is not known yet)
        public static float Fraction(float position, float length)
        {
            if (!(length > 0f) || float.IsNaN(position)) return 0f;
            return position <= 0f ? 0f : position >= length ? 1f : position / length;
        }

        // The time a 0..1 point of the scrubber stands for (clamped)
        public static float TimeAt(float fraction, float length)
        {
            if (!(length > 0f) || float.IsNaN(fraction)) return 0f;
            return (fraction <= 0f ? 0f : fraction >= 1f ? 1f : fraction) * length;
        }
    }
}
