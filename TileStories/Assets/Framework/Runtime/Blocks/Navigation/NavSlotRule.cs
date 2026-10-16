using System;

namespace TileStories
{
    // Which navigation element a slot of the card holds (_3.2 sections 3 and 4.2). Pure: the sheet's NavSlots only applies what it says.
    //   - the wall's choice for the slot (Top Bar), "none" or blank = nothing; a key no element of that slot is registered under =
    //     nothing (the Editor names it missing), never a guess at another element
    //   - a card below Show Navigation From holds no element at all (the model's ShowsNavigation, rule 3 of 2.5)
    //   - an element is not drawn where what it chooses between has fewer than two options (rule 7 of 2.5): sections for tabs, groups
    //     for the segmented element; an element that lists kinds or blocks is drawn on a one-family card too
    // WHEN a held element shows -- the stop, the scroll, the room the stop has -- is CardChromeRule's.
    public static class NavSlotRule
    {
        // The key of the element the top slot holds for this card, or null for none. `topElement`: the definition registered under a
        // key for the TOP slot, null when there is none (NavElementRegistry)
        public static string TopElement(CardNavigationSettings nav, CardNavModel model, Func<string, NavElementDefinition> topElement)
        {
            if (model == null || !model.ShowsNavigation) return null;
            string key = (nav ?? new CardNavigationSettings()).top;
            if (string.IsNullOrWhiteSpace(key) || key == CardOptions.NavNone) return null;
            var element = topElement?.Invoke(key);
            if (element == null) return null;
            return OffersAChoice(element, model) ? key : null;
        }

        // Rule 7: does this card give the element something to choose between?
        public static bool OffersAChoice(NavElementDefinition element, CardNavModel model)
        {
            switch (element.Choice)
            {
                case NavChoice.Sections: return NavOntologyRule.OffersASectionChoice(model.Sections.Count);
                case NavChoice.Groups: return NavOntologyRule.OffersASectionChoice(model.Groups.Count);
                default: return true;
            }
        }
    }
}
