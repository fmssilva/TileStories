namespace TileStories
{
    // What one system Back press does to the open card (_3.2 section 4.5), pure: PoiCardSheetView only applies the answer. With Back
    // Button = step_down each press undoes the innermost thing, in this order:
    //   1. an open full view (TakeoverView) closes;
    //   2. an open menu or list over the card closes (CardMenuOverlay: the wall's Menu, a bar's More list, bar_sheet's kinds list) -- the
    //      sheet stays where it is (step 8);
    //   3. an open compact row closes -- the one opened last (AccordionRule.ClosedByBack): an accordion row, an expandable block (step 13;
    //      NN/g mobile accordions, Budiu 2015: Back collapses the open section) -- the sheet stays where it is;
    //   4. the sheet goes one stop down: Full -> Half -> Peek;
    //   5. at Peek the card closes, the way its X does (the selection is cleared).
    // Each owner that lands later is its own branch with its own test. Back Button = none, or no card open, leaves the press to the app.
    // iOS has no Back button: nothing here runs there.
    public static class NavBackRule
    {
        public enum Step { LeaveToApp, CloseFullView, CloseOverlay, CollapseRow, LowerToHalf, LowerToPeek, CloseCard }

        // Whether the card answers Back at all: anything but none (an unknown value reads as the default, step_down)
        public static bool StepsDown(CardNavigationSettings settings) => settings?.back_button != CardOptions.BackButtonNone;

        // The step one Back press takes, for a card resting at `stop` with its full view, its overlay and a compact row open or not
        public static Step Next(CardNavigationSettings settings, bool fullViewOpen, bool overlayOpen, bool rowOpen, SheetStopRule.Stop stop)
        {
            if (stop == SheetStopRule.Stop.Dismissed || !StepsDown(settings)) return Step.LeaveToApp;
            if (fullViewOpen) return Step.CloseFullView;
            if (overlayOpen) return Step.CloseOverlay;
            // - a row open at Peek is out of sight (the stack shows nothing there): the press goes on to close the card
            if (rowOpen && stop != SheetStopRule.Stop.Peek) return Step.CollapseRow;
            return stop switch
            {
                SheetStopRule.Stop.Full => Step.LowerToHalf,
                SheetStopRule.Stop.Half => Step.LowerToPeek,
                _ => Step.CloseCard,
            };
        }
    }
}
