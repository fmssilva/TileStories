namespace TileStories
{
    // What a tap on a bar_sheet slot does (_3.2 section 5.2, canvas B3), pure: the FIRST tap on a family jumps to it; a SECOND tap on
    // the family the reader is already in opens that family's kinds with their counts (L1 -> L2, two taps to any block). No long-press:
    // it cannot be discovered and a finger that cannot hold still cannot use it (WCAG 2.5.7).
    // A family with fewer than two kinds has no choice to open (the list would hold one row that only repeats the slot), so a tap on it
    // just jumps again -- the same rule icon_tabs_chips applies to its chip row (NavOntologyRule.OffersAKindChoice).
    public static class BarSheetRule
    {
        // True: open the tapped section's kinds. False: jump to the section (its first target, as every bar does)
        public static bool OpensKinds(int tappedSection, int currentSection, int kindCount) =>
            tappedSection >= 0 && tappedSection == currentSection && NavOntologyRule.OffersAKindChoice(kindCount);
    }
}
