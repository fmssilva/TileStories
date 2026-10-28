using UnityEngine;

namespace TileStories
{
    // Pure: how a composed block's preview fits its cap without cutting its title (_3.3 S4a). The cap (R10.2) cuts the rows with a fade; when
    // that cut would reach the words that must stay whole -- the title clear of the fade and at least one line of the lede -- the picture
    // gives way first: it gets shorter by exactly what those words overrun the room, never under its floor. Every height is in panel units,
    // measured from the rows' top with the picture at its NATURAL height (its own proportion), so the answer never feeds back on itself.
    public static class PreviewFitRule
    {
        // Half a pixel: a layout that lands exactly on the room is not over it (ComposedBlockView's own cut tolerance)
        public const float Tolerance = 0.5f;

        // One pixel: layout snaps every edge to a whole pixel, so a shortened picture's measures can read up to a pixel off its whole ones
        // (a natural 228.38 is laid out at 229, a 227.38 at 227)
        public const float SnapBand = 1f;

        // The bottom the preview must keep in view: the title's bottom with the fade's height under it (the fade never lies over the title),
        // and the lede's first line
        public static float KeepBottom(float titleBottom, float fadeHeight, float ledeTop, float ledeLine)
        {
            float title = float.IsNaN(titleBottom) ? 0f : titleBottom + Mathf.Max(0f, fadeHeight);
            float lede = float.IsNaN(ledeTop) || float.IsNaN(ledeLine) ? 0f : ledeTop + Mathf.Max(0f, ledeLine);
            return Mathf.Max(title, lede);
        }

        // The picture's height so that `keepBottom` sits inside `room`, or NaN to leave the picture at its natural height: the rows fit,
        // the cut falls below what must be kept, or there is no picture to shorten. Never under `minPicture` (nor taller than its natural
        // height): a room too small for the words even then cuts below the floor's picture. `shortened`: the picture is shortened now
        public static float PictureHeight(float rowsHeight, float room, float keepBottom, float naturalPicture, float minPicture, bool shortened)
        {
            // - a shortened picture gives its height back only once the words fit by more than the snap: measured shortened, the same words
            //   can read a pixel nearer, and without the band the answer flips on every layout pass (UI Toolkit: "Layout update is struggling")
            float fits = shortened ? Tolerance - SnapBand : Tolerance;
            if (!(naturalPicture > 0f) || float.IsNaN(rowsHeight) || float.IsNaN(room) || rowsHeight <= room + fits) return float.NaN;
            float over = keepBottom - room;
            if (float.IsNaN(over) || over <= fits) return float.NaN;
            return Mathf.Min(naturalPicture, Mathf.Max(Mathf.Min(Mathf.Max(0f, minPicture), naturalPicture), naturalPicture - over));
        }
    }
}
