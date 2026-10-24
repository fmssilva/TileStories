using System;
using FontAsset = UnityEngine.TextCore.Text.FontAsset;

namespace TileStories
{
    // How many lines a card's title takes in its hero (_3.3 P4.4b part 2, the Architect's review of P4.4a), pure: the words wrapped
    // greedily into `width` by `measure` (the width of a run of words in the hero's own face and size -- the Editor passes the real
    // font's advances, a test the real label's). A title that needs NoticeAtLines or more pushes the hero's picture out of view at
    // Half on a phone: the POI Editor says so (Card Content, the header block), the card itself never cuts a title.
    public static class HeroTitleFitRule
    {
        // From this many lines the Editor warns
        public const int NoticeAtLines = 3;

        // The lines `title` wraps to at `width`: words kept whole and joined by one space while the run fits; a single word wider than the
        // line takes as many lines as it is wide (the text engine breaks it). 0 for an empty title or a width with no room
        public static int Lines(string title, float width, Func<string, float> measure)
        {
            if (string.IsNullOrWhiteSpace(title) || !(width > 0f) || measure == null) return 0;
            var words = title.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            int lines = 0;
            string line = "";
            foreach (string word in words)
            {
                string candidate = line.Length == 0 ? word : line + " " + word;
                if (measure(candidate) <= width)
                {
                    line = candidate;
                    continue;
                }
                if (line.Length > 0) lines++;
                // - a word longer than the whole line: the engine breaks it over as many lines as it needs, the last one stays open
                float alone = measure(word);
                int extra = alone > width ? (int)Math.Ceiling(alone / width) - 1 : 0;
                lines += extra;
                line = word;
            }
            return lines + 1;
        }

        // Whether a title of `lines` lines earns the Editor's notice
        public static bool NeedsNotice(int lines) => lines >= NoticeAtLines;

        // A run's width in a UI Toolkit face at `size`: its glyphs' advances, no kerning (a character the static atlas lacks counts as an
        // "M", a wide one -- the notice errs towards warning). The measure the Editor's notice uses; PoiCardHeroTitleFitSceneTests holds it
        // against the real hero label
        public static float AdvanceWidth(FontAsset font, float size, string run)
        {
            if (font == null || string.IsNullOrEmpty(run)) return 0f;
            var table = font.characterLookupTable;
            float scale = size / font.faceInfo.pointSize * font.faceInfo.scale;
            float wide = table.TryGetValue('M', out var m) ? m.glyph.metrics.horizontalAdvance : font.faceInfo.pointSize;
            float units = 0f;
            foreach (char c in run)
                units += table.TryGetValue(c, out var character) && character.glyph != null ? character.glyph.metrics.horizontalAdvance : wide;
            return units * scale;
        }
    }
}
