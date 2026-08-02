namespace TileStories
{
    // The three built-in ways a marker can encode its secondary (status) axis.
    // This is a framework rendering choice, not wall content -- unlike `category`
    // and per-POI status (free-form wall data confirmed by the LivingRoom test
    // wall's "furniture"/"art"/"test" categories, which share nothing with the
    // Panorama wall's "religious"/"military" taxonomy), a wall just picks one of
    // these three built-in renderers. That's why this is a fixed enum and
    // category/status stay plain strings.
    public enum MarkerStyle
    {
        // Category fill colour, achromatic gold->rust status ring (dash rhythm +
        // colour drift, never fades to invisible). Stage 2.3 prototype "System A".
        OutlineGold,

        // Category fill colour that itself drains toward black as status worsens;
        // icon fades in step. Thin same-hue echo ring reinforces. "System C".
        //
        // Worth knowing even though it isn't built this stage: this is also the
        // style best positioned for a future "filter by status" toggle (e.g. a
        // search UI where selecting "religious buildings" makes category the
        // dominant read and status secondary, or vice versa for "80%+ destroyed").
        // Because both axes already live on the same hue/lightness channel pair
        // here, swapping which one drives which is a small change to
        // StatusRamp.ShadeTowardBlack's call site, not a new rendering path. The
        // other two styles would need a structurally different toggle mechanism.
        OutlineSameHue,

        // Category fill colour, no ring at all. Status shown via a small
        // outside-overlap badge, top-right, coloured from the status ramp. "System B".
        Badge,
    }
}