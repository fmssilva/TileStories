namespace TileStories
{
    // What one displacement cycle decided (MarkerOverlapResolver.ApplyDisplacement), as plain numbers:
    // the POI Editor's Live Displacement Readout prints it and the tests assert on it, so every
    // Displacement setting's effect can be read without judging pixels by eye.
    public sealed class DisplacementStats
    {
        public int Candidates;       // visible individual markers the cycle looked at
        public int Groups;           // overlap groups of two or more markers
        public int Moved;            // markers (or their labels) shifted away from their true place
        public int LabelsHidden;     // labels hidden because Max Move was not enough
        public int LeaderLines;      // moves long enough to get a leader line (0 while leader lines are off)
        public float LargestMovePx;  // the longest shift this cycle, in screen pixels
    }
}
