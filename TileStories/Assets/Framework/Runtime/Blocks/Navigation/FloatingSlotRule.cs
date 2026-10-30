using UnityEngine;

namespace TileStories
{
    // Where the card's FLOATING slot rests (_3.2 section 4.2): a row in the card layer just above the sheet's top-right edge, at
    // every stop -- it costs no reading space, so the chrome floor never applies to it. Pure: the sheet's FloatingSlot only applies it.
    //   - its bottom edge sits `gap` above the sheet's top edge, following the sheet as it rises, falls and is dragged
    //   - never higher than the layer allows: at Full with a camera strip the sheet leaves only that gap above it, and the slot rests in it,
    //     flush on the sheet, instead of leaving the screen
    //   - never over the card's chrome (_Test_Suite_1.md F1: a tap meant for the X removed the placed model): when the slot cannot rest on or
    //     above the sheet's top edge (Full to the top of the screen), it rests UNDER the chrome row -- the row of the X and settings -- a gap
    //     below its foot
    public static class FloatingSlotRule
    {
        // The slot's bottom edge, measured up from the layer's bottom (what its style.bottom is set to); `chromeBottom`: the chrome row's foot,
        // measured down from the sheet's top edge. All lengths in panel units
        public static float Bottom(float sheetHeight, float layerHeight, float slotHeight, float gap, float chromeBottom)
        {
            float sheet = Mathf.Max(0f, sheetHeight), slot = Mathf.Max(0f, slotHeight), space = Mathf.Max(0f, gap);
            float wanted = sheet + space;
            float highest = Mathf.Max(0f, layerHeight - slot);
            float above = Mathf.Min(wanted, highest);
            // - resting on or above the sheet's top edge: nothing of the sheet is under it
            if (above >= sheet) return above;
            // - else under the chrome row, still never under the layer's bottom
            return Mathf.Max(0f, sheet - Mathf.Max(0f, chromeBottom) - space - slot);
        }

        // The slot shows only over an open card, and only while something is in it (the placed-model chip)
        public static bool Shown(bool cardOpen, bool holdsSomething) => cardOpen && holdsSomething;
    }
}
