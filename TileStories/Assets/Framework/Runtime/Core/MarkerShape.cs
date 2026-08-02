namespace TileStories
{
    // The base silhouette used for the Symbol (and, by default, the Badge). Purely
    // a visual choice, orthogonal to MarkerStyle -- a wall can be
    // OutlineGold+Hexagon, Badge+Star, whatever combination reads best.
    //
    // Deliberately NOT extended to the status ring: a dashed ring only has pre-made
    // art for Circle (see MarkerRingView / Sprites/Rings). Pairing a non-circle
    // shape with OutlineGold/OutlineSameHue is still fully supported -- the ring is
    // simply always drawn as a circle regardless of the fill shape, which reads
    // fine in practice and avoids needing a full dash-ring asset set multiplied by
    // shape count. A wall that wants a non-circle shape AND a shape-matched status
    // cue should reach for MarkerStyle.Badge instead, since the badge is always
    // circular anyway.
    public enum MarkerShape
    {
        Circle,
        RoundedSquare,
        Hexagon,
        Diamond,
        Star,
    }
}