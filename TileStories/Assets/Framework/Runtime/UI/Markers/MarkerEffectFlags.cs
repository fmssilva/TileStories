namespace TileStories
{
    [System.Flags]
    public enum MarkerEffectFlags
    {
        None = 0,
        Pulse = 1 << 0,
        SunContours = 1 << 1,
        SunCircles = 1 << 2,
        RingPulse = 1 << 3,
        SimpleSun = 1 << 4,
        Beacon = 1 << 5,

        PulseSunContours = Pulse | SunContours,
        PulseSunCircles = Pulse | SunCircles,
    }
}
