using UnityEngine;

namespace TileStories
{
    // Where the card's FLOATING slot rests (_3.2 section 4.2, step 9): a row in the card layer just above the sheet's top-right edge, at
    // every stop -- it costs no reading space, so the chrome floor never applies to it. Pure: the sheet's FloatingSlot only applies it.
    //   - its bottom edge sits `gap` above the sheet's top edge, following the sheet as it rises, falls and is dragged
    //   - never higher than the layer allows: at Full the sheet leaves only its top gap above it, and the slot rests in that gap, flush on the
    //     sheet, instead of leaving the screen
    public static class FloatingSlotRule
    {
        // The slot's bottom edge, measured up from the layer's bottom (what its style.bottom is set to). All lengths in panel units
        public static float Bottom(float sheetHeight, float layerHeight, float slotHeight, float gap)
        {
            float wanted = Mathf.Max(0f, sheetHeight) + Mathf.Max(0f, gap);
            float highest = Mathf.Max(0f, layerHeight - Mathf.Max(0f, slotHeight));
            return Mathf.Min(wanted, highest);
        }

        // The slot shows only over an open card, and only while something is in it (an element, or the placed-model chip)
        public static bool Shown(bool cardOpen, bool holdsSomething) => cardOpen && holdsSomething;

        // Whether the card and the camera view behind the slot are dimmed: while the overlay owner holds the slot's element open -- a
        // floating element opens under its OWN element key -- as an anchored thing (a modal menu dims with its own scrim, above the slot)
        public static bool Dims(string openKey, bool openIsModal, string floatingKey) =>
            !openIsModal && !string.IsNullOrEmpty(openKey) && openKey == floatingKey;
    }

    // Which way fab_dial's list of sections opens from its button (_3.2 section 5.4). Pure.
    //   - up, over the camera view, when the whole list fits above the button (the usual case at peek and half)
    //   - else down, over the card, when it fits there (at Full the slot rests at the top of the screen)
    //   - else towards the side with more room, where the list scrolls
    public static class FabFanRule
    {
        public enum Direction { Up, Down }

        // `roomAbove` / `roomBelow`: from the button's top / bottom edge to the layer's edge; `listHeight`: the whole list's height
        public static Direction Opens(float roomAbove, float roomBelow, float listHeight)
        {
            if (roomAbove >= listHeight) return Direction.Up;
            if (roomBelow >= listHeight) return Direction.Down;
            return roomAbove >= roomBelow ? Direction.Up : Direction.Down;
        }

        // The room the list may take on the side it opens to (it scrolls past that)
        public static float Room(Direction direction, float roomAbove, float roomBelow) =>
            Mathf.Max(0f, direction == Direction.Up ? roomAbove : roomBelow);
    }
}
