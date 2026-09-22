namespace TileStories
{
    // The base silhouette used for the Symbol (and, by default, the Badge). Purely a visual
    // choice, orthogonal to the outline mode and the badge switch: a wall can be gold outline +
    // hexagon, or same-hue outline + star with a badge, whatever combination reads best.
    //
    // Deliberately NOT extended to the status ring: a dashed ring only has pre-made art for
    // Circle (see MarkerRingView / Rings). Pairing a non-circle shape with an outline is fully
    // supported -- the ring is simply always drawn as a circle regardless of the fill shape,
    // which avoids a dash-ring asset set multiplied by shape count.
    public enum MarkerShape
    {
        Circle,
        RoundedSquare,
        Hexagon,
        Diamond,
        Star,
        // No background silhouette -- the marker renders icon-only against the
        // scene. The status ring and badge still apply per their own rules; only
        // the filled backdrop behind the symbol is suppressed.
        None,
    }
}