namespace TileStories
{
    // The dock navigation element (_3.2 section 5.2, canvas B2, L1): the bottom bar as an icon PILL where the section being read widens
    // to icon + word and every other slot stays an icon. The widening follows the scroll-spy (the bar's own mark, NavTab.CurrentClass),
    // never the tap, so a visitor reading down the card watches it travel. It hides on scroll down with the rest of the bottom area -- that
    // is CardChromeRule's job on the area, so this view never hides itself.
    // The whole difference from bottom_bar is LOOK, and the look is the stylesheet's (Navigation.uss, .nav-bar--dock): the slots, More and
    // its list, the mark and the jumps are the bar's own. Labels does not apply (the marked item always shows icon and word, the others
    // never their word); an icon alone still names its slot through its tooltip.
    public sealed class DockNavView : BottomBarNavView
    {
        // On the root: the stylesheet's hook for the pill look
        public const string DockClass = "nav-bar--dock";

        public DockNavView() : base(BuiltInNavElements.Dock) => Root.AddToClassList(DockClass);

        // The stylesheet decides which words show (the marked one's), not the Labels setting
        protected override bool LabelsFollowTheSetting => false;
    }
}
