using System;

namespace TileStories
{
    // Which navigation element a slot of the card shows, and at which stops (_3.2 sections 3 and 4.2). Pure: the sheet's NavSlots
    // only applies what it says.
    //   - the wall's choice for the slot (Top Bar), "none" or blank = nothing; a key no element of that slot is registered under =
    //     nothing (the Editor names it missing), never a guess at another element
    //   - a card below Show Navigation From shows no element at all (the model's ShowsNavigation, rule 3 of 2.5)
    //   - never at peek: the strip sits under the header's peek part, so a peek card shows exactly its header, and adding navigation
    //     never moves a stop. Show Navigation At / Hide While Scrolling refine this at step 5b (CardChromeRule)
    public static class NavSlotRule
    {
        // The key of the element the top slot draws for this card, or null for none. `isTopElement`: whether a key is registered for
        // the top slot (NavElementRegistry)
        public static string TopElement(CardNavigationSettings nav, bool showsNavigation, Func<string, bool> isTopElement)
        {
            if (!showsNavigation) return null;
            string key = (nav ?? new CardNavigationSettings()).top;
            if (string.IsNullOrWhiteSpace(key) || key == CardOptions.NavNone) return null;
            return isTopElement != null && isTopElement(key) ? key : null;
        }

        // Whether a navigation slot is shown while the sheet rests at (or is dragged from) this stop
        public static bool ShownAt(SheetStopRule.Stop stop) => stop == SheetStopRule.Stop.Half || stop == SheetStopRule.Stop.Full;
    }
}
