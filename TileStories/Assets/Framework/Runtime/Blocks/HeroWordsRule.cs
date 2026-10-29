using TileStories.LookKit;
using UnityEngine;

namespace TileStories
{
    // How the hero's words stand on its picture (_3.3 P3c addendum, "Words On Picture"; the header's Words On Picture field), pure:
    //   - tone_halo (the default): a HALO on the letters (a close 1 px shade + a soft glow, USS) and a TONE -- a very wide, very soft radial
    //     darkening anchored at the words' corner (the prototype's radial-gradient(130% 75% at 20% 92%): .55, .32 at 38 %, .10 at 62 %,
    //     nothing past 80 %): it reads as light falling off, never as a shape
    //   - edge_scrim: no halo; the picture darkens up from the words' foot (.66, .52 at 18 %, .30 at 36 %, .12 at 52 %, nothing past 66 %)
    //   - halo_only: the halo alone, nothing on the picture
    // The tone's and the scrim's STRENGTH is the hero's to work out from its picture (HeroWordsOnPicture: only what 4.5:1 needs)
    public static class HeroWordsRule
    {
        public const string ToneHalo = "tone_halo";
        public const string EdgeScrim = "edge_scrim";
        public const string HaloOnly = "halo_only";
        public static readonly string[] Options = { ToneHalo, EdgeScrim, HaloOnly };
        public static readonly string[] Labels = { "Tone + Halo", "Edge Scrim", "Halo Only" };

        // The prototype's gradients, in its own terms (0 = the anchor, 1 = the reach)
        public static readonly (float At, float Alpha)[] ToneStops = { (0f, 0.55f), (0.38f, 0.32f), (0.62f, 0.10f), (0.80f, 0f) };
        public static readonly (float At, float Alpha)[] ScrimStops = { (0f, 0.66f), (0.18f, 0.52f), (0.36f, 0.30f), (0.52f, 0.12f), (0.66f, 0f) };
        // The tone's ellipse: 130 % of the hero's width, 75 % of the height it is seen over; its anchor 20 % across
        public const float ToneWidth = 1.3f;
        public const float ToneHeight = 0.75f;
        public const float ToneAcross = 0.2f;

        // The look a value names (an unknown or empty one reads as the default)
        public static string Of(string value) => System.Array.IndexOf(Options, value) >= 0 ? value : ToneHalo;

        public static bool HasHalo(string mode) => Of(mode) != EdgeScrim;
        public static bool HasLayer(string mode) => Of(mode) != HaloOnly;

        // The share of the gradient every word lies within: the tone's 38 % stop, the scrim's 36 % -- past them the stops fall too low for any
        // strength to keep a word at 4.5:1 on a bright picture
        public const float ToneDense = 0.38f;
        public const float ScrimDense = 0.36f;

        // Tone Spread (the header's field): the prototype's reach times this -- smaller, the darkening ends sooner round the words; larger,
        // it falls off over more of the picture. Never below what the words need (the grow below keeps them in the dense part)
        public const float SpreadMin = 0.4f;
        public const float SpreadMax = 1.5f;
        public const float SpreadDefault = 1f;

        // A spread as the rule reads it: inside its range, an unset / unreadable one the default
        public static float Spread(float value) => float.IsNaN(value) || value <= 0f ? SpreadDefault : Mathf.Clamp(value, SpreadMin, SpreadMax);

        // The layer for `mode` over a hero `width` wide, seen from `seenTop` down, round the words' box `words`: the tone at the words' corner
        // (20 % across, their foot), or the scrim up from their foot; null for halo_only. Its reach is the prototype's times `spread`, grown --
        // both radii together, the shape kept -- only as far as it takes for the words' farthest corner to lie within the dense part
        public static ToneOptions Layer(string mode, float width, float seenTop, Rect words, Color colour, float spread = SpreadDefault)
        {
            mode = Of(mode);
            if (mode == HaloOnly || !(width > 0f)) return null;
            spread = Spread(spread);
            float foot = words.yMax, seen = Mathf.Max(1f, foot - seenTop);
            if (mode == EdgeScrim)
            {
                float rise = Mathf.Max(seen * spread, words.height / ScrimDense);
                return new ToneOptions { Shape = ToneShape.Edge, Anchor = new Vector2(0f, foot), Reach = new Vector2(width, rise), Stops = ScrimStops, Colour = colour };
            }
            var anchor = new Vector2(width * ToneAcross, foot);
            var reach = new Vector2(width * ToneWidth, seen * ToneHeight) * spread;
            float dx = Mathf.Max(Mathf.Abs(words.xMax - anchor.x), Mathf.Abs(words.xMin - anchor.x)) / reach.x, dy = words.height / reach.y;
            float grow = Mathf.Max(1f, Mathf.Sqrt(dx * dx + dy * dy) / ToneDense);
            return new ToneOptions { Shape = ToneShape.Radial, Anchor = anchor, Reach = reach * grow, Stops = ToneStops, Colour = colour };
        }
    }
}
