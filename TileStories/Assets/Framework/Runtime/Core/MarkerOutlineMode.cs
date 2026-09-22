namespace TileStories
{
    // Controls whether and how the status contour is rendered.
    public enum MarkerOutlineMode
    {
        Uniform,  // one shared, developer-adjustable colour for every level (default: gold)
        SameHue,  // the ring takes the marker's own category hue, darkening with severity
        PerType,  // each outline level (table row) carries its own colour
        None,
    }
}
