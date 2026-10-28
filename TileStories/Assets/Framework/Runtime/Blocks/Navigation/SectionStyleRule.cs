using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // How each section of an OPEN card looks (_3.3 fourth round, v6's Sections), pure: the card's choices (card_settings.sections) with the
    // nearest level winning -- the group's Section Style (its Bar Groups row) over Vary over the card's Start. A section is one Navigation
    // Bar group's run of the scroll. The first section has no divider (nothing stands before it). The tint is v6's sRGB mix, painted exactly
    // (Tint: the project blends in LINEAR light, where a raw alpha of a bright colour over the dark sheet read far brighter than v6's glow).
    //   - every option read with its default: an unknown value reads as the first-listed default, so the Editor and the card agree
    public static class SectionStyleRule
    {
        // The Starts Vary By Group walks, group by group (v6: about fade, story wave, media tiles, ...)
        public static readonly string[] ByGroupStarts = { CardOptions.SectionStartFade, CardOptions.SectionStartWave, CardOptions.SectionStartTiles };
        // How far down a section the tint fades out (v6's 340 px), and Bands' share of the intensity (v6: 0.7)
        public const float FadeLength = 340f;
        public const float BandsShare = 0.7f;
        // The theme colours a section paints by name: neutral's tint, and a group colour's words ("<token>-text")
        public const string NeutralToken = "--ts-section-neutral";
        public const string AccentToken = "--ts-accent";
        public static string TextOf(string colourToken) => string.IsNullOrEmpty(colourToken) ? "--ts-ink" : colourToken + "-text";

        // One section's look, resolved
        public readonly struct Style
        {
            public readonly string Start;
            public readonly string Divider;
            public readonly string Colour;
            public readonly float Intensity;
            public readonly string Motion;

            public Style(string start, string divider, string colour, float intensity, string motion)
            {
                Start = start;
                Divider = divider;
                Colour = colour;
                Intensity = intensity;
                Motion = motion;
            }

            public bool HasTint => Start != CardOptions.SectionStartPlain;
            public bool Moves => Motion != CardOptions.SectionMotionStill;
        }

        public static string Start(CardSectionSettings s) => Pick(s?.start, CardOptions.SectionStarts, CardOptions.SectionStartTiles);
        public static string Divider(CardSectionSettings s) => Pick(s?.divider, CardOptions.SectionDividers, CardOptions.SectionDividerNone);
        public static string Colour(CardSectionSettings s) => Pick(s?.colour, CardOptions.SectionColours, CardOptions.SectionColourGroup);
        public static string Motion(CardSectionSettings s) => Pick(s?.motion, CardOptions.SectionMotions, CardOptions.SectionMotionReveal);
        public static string Vary(CardSectionSettings s) => Pick(s?.vary, CardOptions.SectionVaries, CardOptions.SectionVarySame);
        public static float Intensity(CardSectionSettings s) =>
            Mathf.Clamp(s?.intensity ?? 0.12f, CardSectionSettings.IntensityMin, CardSectionSettings.IntensityMax);
        public static bool IsOpen(CardContainerSettings c) => Pick(c?.blocks, CardOptions.Blocks, CardOptions.BlocksOpen) == CardOptions.BlocksOpen;

        // The section at `position` (0 = the card's first section) of the group at `groupIndex` in the bar's order, whose row's Section Style
        // is `groupStart` ("" = the card's), under the card's settings `s`
        public static Style Of(CardSectionSettings s, int position, int groupIndex, string groupStart)
        {
            string start = System.Array.IndexOf(CardOptions.SectionStarts, groupStart) >= 0 ? groupStart
                : Vary(s) == CardOptions.SectionVaryByGroup ? ByGroupStarts[Mathf.Max(0, groupIndex) % ByGroupStarts.Length]
                : Start(s);
            // - one ornament per boundary (_3.3 P3a review f): a Tiles or Wave Start IS the boundary's ornament, so no divider stands before it
            string divider = position == 0 || OrnamentStart(start) ? CardOptions.SectionDividerNone : Divider(s);
            return new Style(start, divider, Colour(s), Intensity(s), Motion(s));
        }

        // A Start drawn as an ornament of its own (the tile row, the wave): it takes the divider's place
        public static bool OrnamentStart(string start) => start == CardOptions.SectionStartTiles || start == CardOptions.SectionStartWave;

        // The theme colour a section's tint takes: its group's colour, the accent, or the neutral
        public static string TintToken(string colour, string groupColourToken) =>
            colour == CardOptions.SectionColourAccent ? AccentToken
            : colour == CardOptions.SectionColourNeutral ? NeutralToken
            : string.IsNullOrEmpty(groupColourToken) ? AccentToken : groupColourToken;

        // How strong the tint is at `y` down the section's ground (0 at its top): Bands flat (every other section only), the others fading out
        // over FadeLength (a smooth ease, like v6's straight CSS gradient seen through the sRGB mix); Plain none
        public static float AmountAt(Style style, int position, float y)
        {
            if (!style.HasTint || y < 0f) return 0f;
            if (style.Start == CardOptions.SectionStartBands) return position % 2 == 1 ? style.Intensity * BandsShare : 0f;
            return style.Intensity * Mathf.Clamp01(1f - y / FadeLength);
        }

        // The colour (and alpha) to draw over `sheet` so that the panel's LINEAR blend lands exactly on v6's sRGB mix of the two at `amount`:
        // each channel solved in linear light. The alpha is the amount, or more when a channel needs it: a DARK tint over a light sheet would
        // have to be darker than black at the amount -- every family's hue clamped to the same black, the Colour option invisible on light --
        // so it is drawn a little more opaque instead (the least alpha at which every channel is a real colour)
        public static Color Tint(Color sheet, Color tint, float amount)
        {
            if (amount <= 0f) return new Color(tint.r, tint.g, tint.b, 0f);
            float alpha = amount;
            void Needs(float s, float t)
            {
                float under = Mathf.GammaToLinearSpace(s), target = Mathf.GammaToLinearSpace(Mathf.Lerp(s, t, amount));
                // - drawn = under + (target - under) / alpha must stay in 0..1
                if (target < under) alpha = Mathf.Max(alpha, (under - target) / under);
                else if (target > under && under < 1f) alpha = Mathf.Max(alpha, (target - under) / (1f - under));
            }
            Needs(sheet.r, tint.r);
            Needs(sheet.g, tint.g);
            Needs(sheet.b, tint.b);
            float Channel(float s, float t)
            {
                float under = Mathf.GammaToLinearSpace(s), target = Mathf.GammaToLinearSpace(Mathf.Lerp(s, t, amount));
                return Mathf.LinearToGammaSpace(Mathf.Clamp01(under + (target - under) / alpha));
            }
            return new Color(Channel(sheet.r, tint.r), Channel(sheet.g, tint.g), Channel(sheet.b, tint.b), Mathf.Min(1f, alpha));
        }

        // What the panel draws for Tint: the LINEAR blend of `drawn` over `sheet`, back in gamma (the tests' and the contrast's arithmetic)
        public static Color BlendLinear(Color sheet, Color drawn)
        {
            float Channel(float s, float d) =>
                Mathf.LinearToGammaSpace(Mathf.GammaToLinearSpace(d) * drawn.a + Mathf.GammaToLinearSpace(s) * (1f - drawn.a));
            return new Color(Channel(sheet.r, drawn.r), Channel(sheet.g, drawn.g), Channel(sheet.b, drawn.b), 1f);
        }

        // The tint's stops down a section's ground starting `top` below it (under the divider and the start's shape): the amounts sampled
        // along the fade (the sRGB mix is no straight line in the panel's linear light: several stops follow it), clear above `top`
        public static List<(float Offset, float Amount)> Stops(Style style, int position, float top)
        {
            var stops = new List<(float, float)>();
            if (!style.HasTint) return stops;
            stops.Add((top, 0f));
            if (style.Start == CardOptions.SectionStartBands)
            {
                stops.Add((top, AmountAt(style, position, 0f)));
                return stops;
            }
            const int samples = 8;
            for (int i = 0; i <= samples; i++)
            {
                float y = FadeLength * i / samples;
                stops.Add((top + y, AmountAt(style, position, y)));
            }
            return stops;
        }

        private static string Pick(string value, string[] options, string fallback)
        {
            foreach (string option in options)
                if (option == value) return value;
            return fallback;
        }
    }
}
