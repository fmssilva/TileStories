namespace TileStories
{
    // The six selectable marker effects. Chosen per hierarchy level (ripple / halo / pulse
    // columns), then filtered by EffectDefaults.FilterEnabled.
    [System.Flags]
    public enum MarkerEffectFlags
    {
        None = 0,
        Pulse = 1 << 0,
        RippleRings = 1 << 1,
        RippleDiscs = 1 << 2,
        HaloRing = 1 << 3,
        HaloDisc = 1 << 4,
        Beacon = 1 << 5,
    }

    // Player-facing name of one effect: the single source for the editor's foldout titles and
    // usage lines and for the effects preview grid's cell labels.
    public static class MarkerEffectNames
    {
        public static string DisplayName(MarkerEffectFlags effect)
        {
            switch (effect)
            {
                case MarkerEffectFlags.Pulse: return "Pulse";
                case MarkerEffectFlags.RippleRings: return "Ripple Rings";
                case MarkerEffectFlags.RippleDiscs: return "Ripple Discs";
                case MarkerEffectFlags.HaloRing: return "Halo Ring";
                case MarkerEffectFlags.HaloDisc: return "Halo Disc";
                case MarkerEffectFlags.Beacon: return "Beacon";
                default: return effect.ToString();
            }
        }
    }
}
