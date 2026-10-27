using System;
using UnityEngine.UIElements;

namespace TileStories
{
    // The universal block frame (_3.3 R13.1, R12.5, P4.4b, S4a): every block of the scroll sits in an opaque surface inside a metal rim. The
    // SLOT itself carries the surface and the rim's outer line -- the dark hairline that is the block's measured edge --, so a block's words
    // read on an ancestor's background (what the contrast checks walk); three parts at the slot's back draw the rest -- the shadow, an
    // outer-only 9-slice that is transparent over the block; the rim's metal band (--ts-rim-band wide: lit top-left, shaded bottom-right,
    // the band the moving light rides); and a thin groove just inside the band (UI Toolkit 6.3 has no z-index: a child draws over its
    // parent's background).
    // The look is PoiCard.uss (.card-block-slot--framed, --<emphasis>); light vs dark is the theme's colours, never code. Plain C#:
    // BlockStackView builds the parts into every slot it makes and frames a slot as it places it -- the scroll's, never the bar's or the
    // footer's.
    public static class BlockFrame
    {
        public const string FramedClass = "card-block-slot--framed";
        public const string ShadowClass = "card-block-frame__shadow";
        public const string RimClass = "card-block-frame__rim";
        public const string GrooveClass = "card-block-frame__groove";
        // How many parts Build puts at the back of a slot (what draws after them -- the rim light -- goes right after)
        public const int PartCount = 3;
        // A picture, carousel, strip or track runs edge to edge to the frame's inner edge (P4.4b part 2, App-Store-card style): words keep
        // the frame's padding, media gets its whole width back (the frame's padding is undone by --ts-frame-bleed, PoiCard.uss)
        public const string BleedClass = "card-block-bleed";
        private const string EmphasisClassPrefix = "card-block-slot--";

        // Mark one region of a block (its track, strip or picture stage) to run to the frame's inner edge; outside a frame it changes nothing
        public static void Bleed(VisualElement region) => region.AddToClassList(BleedClass);

        // Add the frame's parts at the back of a new slot (hidden by the stylesheet until the slot is framed)
        public static void Build(VisualElement slot)
        {
            var shadow = new VisualElement { name = "card-block-shadow", pickingMode = PickingMode.Ignore };
            shadow.AddToClassList(ShadowClass);
            var rim = new VisualElement { name = "card-block-rim", pickingMode = PickingMode.Ignore };
            rim.AddToClassList(RimClass);
            var groove = new VisualElement { name = "card-block-groove", pickingMode = PickingMode.Ignore };
            groove.AddToClassList(GrooveClass);
            slot.Insert(0, shadow);
            slot.Insert(1, rim);
            slot.Insert(2, groove);
        }

        // Frame the slot (or not) with this emphasis; a pooled slot is re-framed every time it is placed
        public static void Apply(VisualElement slot, bool framed, string emphasis)
        {
            slot.EnableInClassList(FramedClass, framed);
            string shown = EmphasisOf(emphasis);
            foreach (string option in CardOptions.Emphases)
                slot.EnableInClassList(EmphasisClassPrefix + option, framed && option == shown);
        }

        // The emphasis a block shows: its own when it is one of CardOptions.Emphases, else normal
        public static string EmphasisOf(string emphasis) =>
            Array.IndexOf(CardOptions.Emphases, emphasis) >= 0 ? emphasis : CardOptions.EmphasisNormal;
    }
}
