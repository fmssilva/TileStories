using UnityEngine;

namespace TileStories
{
    // Where the card sheet rests (_3.1 section 4): peek (the compact header), half (<= Half Height Max of the
    // available height) and full (the available height minus a top gap), and where a released drag snaps.
    // Heights are in the card panel's own units; the view measures its container and hands them in.
    public static class SheetStopRule
    {
        public enum Stop { Dismissed, Peek, Half, Full }

        public readonly struct Stops
        {
            public readonly float Peek;
            public readonly float Half;
            public readonly float Full;

            public Stops(float peek, float half, float full)
            {
                Peek = peek;
                Half = half;
                Full = full;
            }

            public float HeightOf(Stop stop) => stop switch
            {
                Stop.Peek => Peek,
                Stop.Half => Half,
                Stop.Full => Full,
                _ => 0f,
            };
        }

        // A release faster than this many available-heights per second is a flick: it goes one stop further
        // in its direction even when the finger stopped closer to the old stop (a quick swipe, not a slow drag)
        public const float FlickHeightsPerSecond = 1.2f;

        // Released below this share of the peek height (not a flick): the visitor pulled it away -> close
        public const float DismissBelowPeekRatio = 0.6f;

        // The three stop heights. Half is capped by `halfMaxRatio` (itself clamped to the allowed range) and
        // never under peek; full never under half; nothing above the available height.
        public static Stops Compute(float availableHeight, float peekHeight, float halfMaxRatio, float fullTopGap)
        {
            float available = Mathf.Max(0f, availableHeight);
            float ratio = Mathf.Clamp(halfMaxRatio, CardContainerSettings.HalfMaxRatioMin, CardContainerSettings.HalfMaxRatioMax);
            float full = Mathf.Max(0f, available - Mathf.Max(0f, fullTopGap));
            float peek = Mathf.Clamp(peekHeight, 0f, full);
            float half = Mathf.Clamp(available * ratio, peek, full);
            return new Stops(peek, half, full);
        }

        // Where the card opens for a new selection: "half" opens at half, anything else at peek (never full)
        public static Stop OpenStop(string openStop) => openStop == CardOptions.StopHalf ? Stop.Half : Stop.Peek;

        // Where a released drag goes. `velocity` is the height's rate of change (+ = growing), in panel units per second.
        public static Stop Snap(Stops stops, float height, float velocity, float availableHeight)
        {
            bool flick = availableHeight > 0f && Mathf.Abs(velocity) >= FlickHeightsPerSecond * availableHeight;
            if (flick)
            {
                if (velocity > 0f)
                    return height < stops.Peek ? Stop.Peek : height < stops.Half ? Stop.Half : Stop.Full;
                return height > stops.Half ? Stop.Half : height > stops.Peek ? Stop.Peek : Stop.Dismissed;
            }

            if (height < stops.Peek * DismissBelowPeekRatio) return Stop.Dismissed;
            Stop nearest = Stop.Peek;
            float best = Mathf.Abs(height - stops.Peek);
            if (Mathf.Abs(height - stops.Half) < best) { nearest = Stop.Half; best = Mathf.Abs(height - stops.Half); }
            if (Mathf.Abs(height - stops.Full) < best) nearest = Stop.Full;
            return nearest;
        }
    }
}
