namespace TileStories
{
    // What one system Back press does to the open card (_3.2 section 4.5), pure: PoiCardSheetView only applies the answer. With Back
    // Button = step_down each press undoes the innermost thing, in this order:
    //   1. an open full view (TakeoverView) closes;
    //   2. an open menu or list over the card closes (CardMenuOverlay: the wall's Menu, a bar's More list, bar_sheet's kinds list) -- the
    //      sheet stays where it is (step 8);
    //   3. the sheet goes one stop down: Full -> Half -> Peek;
    //   4. at Peek the card closes, the way its X does (the selection is cleared).
    // The order grows with the owners that land later -- an expanded accordion row collapses before the sheet moves (step 13) -- each as
    // its own branch with its own test. Back Button = none, or no card open, leaves the press to the app. iOS has no Back button: nothing
    // here runs there.
    public static class NavBackRule
    {
        public enum Step { LeaveToApp, CloseFullView, CloseOverlay, LowerToHalf, LowerToPeek, CloseCard }

        // Whether the card answers Back at all: anything but none (an unknown value reads as the default, step_down)
        public static bool StepsDown(CardNavigationSettings settings) => settings?.back_button != CardOptions.BackButtonNone;

        // The step one Back press takes, for a card resting at `stop` with its full view and its overlay open or not
        public static Step Next(CardNavigationSettings settings, bool fullViewOpen, bool overlayOpen, SheetStopRule.Stop stop)
        {
            if (stop == SheetStopRule.Stop.Dismissed || !StepsDown(settings)) return Step.LeaveToApp;
            if (fullViewOpen) return Step.CloseFullView;
            if (overlayOpen) return Step.CloseOverlay;
            return stop switch
            {
                SheetStopRule.Stop.Full => Step.LowerToHalf,
                SheetStopRule.Stop.Half => Step.LowerToPeek,
                _ => Step.CloseCard,
            };
        }
    }
}
