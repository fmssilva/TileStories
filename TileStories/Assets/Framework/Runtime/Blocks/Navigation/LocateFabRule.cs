using UnityEngine;

namespace TileStories
{
    // The card's "Show me where it is" button (_3.3 P2 review e, P3d), pure -- when it shows and where it rests; LocateFabView applies it:
    //   - a first-class action of every open card while the Navigation Bar and its Locate Button are on, at Half and Full (at Peek the wall
    //     is already in view), over the hero and after it; it goes with the bar while Hide While Scrolling slides the bar away (Bottom or Top)
    //   - at the bottom right, `gap` above the bar's highest point when the bar floats at the bottom (the raised disc's top: it never meets
    //     the bar's items, whichever group is chosen), else `gap` above the sheet's foot or a sticky footer shown there
    //   - it FLOATS (its own layer over the sheet): it takes no room, so it never narrows, wraps or moves the hero's words or a block --
    //     an occasional overlap with a word is accepted; only its chip of words gives way to the hero's words (LabelFits)
    public static class LocateFabRule
    {
        // Whether the button shows now: the bar and the Locate Button on, the sheet at Half or Full, and the scroll not hiding the bar
        public static bool Shown(CardNavigationSettings nav, SheetStopRule.Stop stop, bool scrollHidesBar) =>
            NavBarRule.Enabled(nav) && NavBarRule.LocateButton(nav.bar) && !scrollHidesBar
            && (stop == SheetStopRule.Stop.Half || stop == SheetStopRule.Stop.Full);

        // The button's bottom edge above the sheet's foot: `gap` over the highest of what lies under it -- the bar's highest point above the
        // foot (NaN: no bar at the bottom) and the top of a sticky footer shown there (0: none)
        public static float Bottom(float barHighest, float footerTop, float gap) =>
            Mathf.Max(float.IsNaN(barHighest) ? 0f : barHighest, float.IsNaN(footerTop) ? 0f : Mathf.Max(0f, footerTop)) + Mathf.Max(0f, gap);

        // ---- Its look (_3.3 P3d review b, Detail Card > Locate Button): every option read with its default ----

        public static string Style(CardLocateSettings locate) => Pick(locate?.style, CardOptions.LocateStyles, CardOptions.LocatePinLabel);
        public static string Colour(CardLocateSettings locate) => Pick(locate?.colour, CardOptions.LocateColours, CardOptions.LocateTeal);
        public static float Size(CardLocateSettings locate) =>
            Mathf.Clamp(locate == null || float.IsNaN(locate.size) ? CardLocateSettings.SizeDefault : locate.size, CardLocateSettings.SizeMin, CardLocateSettings.SizeMax);
        // Whether the pin moves by itself now: Float on and the card's Reduce Motion off
        public static bool Floats(CardLocateSettings locate, bool reduceMotion) => (locate?.float_motion ?? true) && !reduceMotion;
        // Whether the chip of words shows beside the pin: Pin + Label, until the visitor's first scroll of this card
        public static bool LabelShown(CardLocateSettings locate, bool scrolled) => Style(locate) == CardOptions.LocatePinLabel && !scrolled;
        public static bool HasGlass(CardLocateSettings locate) => Style(locate) == CardOptions.LocateGlassPing;
        // Whether the pin lies on the hero's picture as drawn now (its halo on): any part of it over the picture (an empty hero rect: none)
        public static bool OverPicture(Rect pin, Rect picture) => picture.width > 0f && picture.height > 0f && picture.Overlaps(pin);
        // Whether the chip has room beside the pin: no ink of the hero's words within `gap` of it (the words never give way to the button --
        // narrowing them wrapped a title into the bright sky -- so where they reach across, the card shows the pin alone)
        public static bool LabelFits(Rect chip, System.Collections.Generic.IEnumerable<Rect> words, float gap)
        {
            var clear = new Rect(chip.x - gap, chip.y - gap, chip.width + 2f * gap, chip.height + 2f * gap);
            foreach (var word in words)
                if (word.Overlaps(clear)) return false;
            return true;
        }

        // The pin's colours by THEME token name: (the body's light top, its deep foot and the dot, its thin rim); Accent = the theme's accent
        public static (string Hi, string Lo, string Rim) Tokens(string colour) => colour switch
        {
            CardOptions.LocateGreen => ("--ts-locate-green-hi", "--ts-locate-green-lo", "--ts-locate-green-rim"),
            CardOptions.LocateCyan => ("--ts-locate-cyan-hi", "--ts-locate-cyan-lo", "--ts-locate-cyan-rim"),
            CardOptions.LocateAccent => ("--ts-accent-shine", "--ts-accent", "--ts-accent-text"),
            _ => ("--ts-locate-teal-hi", "--ts-locate-teal-lo", "--ts-locate-teal-rim"),
        };

        // ---- Its sizes (pt): the study's pin (a 48 x 64 drawing), the glass disc 1.5 times the pin's height, the shadow under it ----

        public const float PinAspect = 48f / 64f;
        public const float GlassToPin = 1.5f;
        public const float MinTouch = 44f;
        // The pin's height: the Navigation Bar's chosen disc (--ts-navbar-disc-raised) times Size
        public static float PinHeight(float chosenDisc, CardLocateSettings locate) => Mathf.Max(0f, chosenDisc) * Size(locate);
        // What floats: the pin, or the glass disc holding it
        public static float FloatBox(float pinHeight, bool glass) => glass ? pinHeight * GlassToPin : pinHeight;
        // The shadow on the bar under the tip: a flat ellipse, narrower than what floats over it
        public static float ShadowWidth(float pinHeight, bool glass) => glass ? FloatBox(pinHeight, true) * 0.6f : pinHeight * PinAspect * 0.8f;
        public static float ShadowHeight(float pinHeight, bool glass) => ShadowWidth(pinHeight, glass) * 0.28f;
        // How far the float box's foot rests over the touch target's foot: on the shadow's middle, the tip a little above it (it floats even
        // at rest)
        public static float FloatBottom(float pinHeight, bool glass) => ShadowHeight(pinHeight, glass) + pinHeight * 0.03f;
        // The height of everything standing in the touch target at the float's top: the shadow, the lift, the float box and its Bob
        public static float Standing(float pinHeight, bool glass) => FloatBottom(pinHeight, glass) + FloatBox(pinHeight, glass) + Bob;
        // The touch target's side: everything standing in it at every size (so the pin never leaves its target as it floats), never under
        // 44 pt; it is clear, the card shows through it
        public static float Touch(float pinHeight, bool glass) => Mathf.Max(MinTouch, Standing(pinHeight, glass));

        // ---- Its motion, on a clock (s); still = 0 / whole. Shapes of the study: ease-in-out bob and breathing shadow, an eased-out ping ----

        // How high the pin floats (pt) and over how long one rise and fall takes (s)
        public const float Bob = 7f;
        public const float BobPeriod = 2.6f;
        // The ping: one ring every PingPeriod (s), growing to PingScale and fading over the first PingShare of it
        public const float PingPeriod = 3.2f;
        public const float PingScale = 1.7f;
        public const float PingShare = 0.6f;

        // How far up the pin floats at `time` (0..1 of Bob): an eased sine, 0 at rest, 1 at the top, back down
        public static float Rise(float time, bool floats) =>
            floats ? 0.5f - 0.5f * Mathf.Cos(2f * Mathf.PI * Wrap(time, BobPeriod) / BobPeriod) : 0f;
        // The shadow on the bar as the pin rises: smaller and fainter (1, 0.9 at rest -> 0.7, 0.5 at the top)
        public static float ShadowScale(float rise) => 1f - 0.3f * Mathf.Clamp01(rise);
        public static float ShadowAlpha(float rise) => 0.9f - 0.4f * Mathf.Clamp01(rise);
        // The ping ring at `time`: its scale over the glass disc and its alpha (0 between rings; none while still)
        public static (float Scale, float Alpha) Ping(float time, bool moves)
        {
            if (!moves) return (1f, 0f);
            float u = Wrap(time, PingPeriod) / (PingPeriod * PingShare);
            if (u >= 1f) return (PingScale, 0f);
            float eased = 1f - (1f - u) * (1f - u);
            return (1f + (PingScale - 1f) * eased, 0.8f * (1f - u));
        }

        private static float Wrap(float time, float period) => float.IsNaN(time) ? 0f : Mathf.Repeat(time, period);

        private static string Pick(string value, string[] options, string fallback) =>
            value != null && System.Array.IndexOf(options, value) >= 0 ? value : fallback;
    }
}
