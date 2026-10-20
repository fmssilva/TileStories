namespace TileStories
{
    // The card's LAYOUT (_3.2 section 6.1, steps 13-14), pure: how the SAME bound slots of the stack are arranged -- never what a block's view
    // draws (a kind's view never learns which layout it is in). CardLayoutView applies what it says; NavController asks it where a jump goes.
    //   - scroll (the default): today's card, every block in one scroll
    //   - accordion: every navigation target is a compact row that opens in place (step 12's mechanism as a whole-card layout); with Open
    //     One At A Time, opening one closes the others (AccordionRule)
    //   - tabs: one section shown at a time, the others' blocks set aside (unbound, so they hold no media); a block of no section (the
    //     sources, a nav block) shows with every section. It needs an element to switch sections with: with neither a Top Bar nor a Bottom
    //     Bar the top slot holds Text Tabs, and a card that shows no navigation (under Show Navigation From, one section) simply scrolls
    //   - pages: tabs' one-section-at-a-time turned sideways -- each section a page that scrolls up and down inside, the next page peeking
    //     at the right edge, a sideways swipe or the Family Pager's chevrons turning it (CardPagerView, SwipePageRule). The top slot always
    //     holds the Family Pager: it names the page and is the way without a swipe (WCAG 2.5.7)
    public static class CardLayoutRule
    {
        // The layout the wall picked: scroll for anything it does not know (an unset field, a typo, a layout of a later step)
        public static string Layout(CardNavigationSettings settings)
        {
            string layout = settings?.layout;
            return layout == CardOptions.LayoutAccordion || layout == CardOptions.LayoutTabs || layout == CardOptions.LayoutPages
                ? layout : CardOptions.LayoutScroll;
        }

        // The layout THIS card is drawn in: tabs and pages only where the card offers sections to switch between (it shows navigation and has
        // two or more sections) -- one tab or one page is not a choice, and a short card would otherwise hide blocks behind a bar it never draws
        public static string Effective(CardNavigationSettings settings, CardNavModel model)
        {
            string layout = Layout(settings);
            if (!ParksSections(layout)) return layout;
            return model != null && model.ShowsNavigation && NavOntologyRule.OffersASectionChoice(model.Sections.Count)
                ? layout : CardOptions.LayoutScroll;
        }

        // Whether a layout shows one section at a time and sets the others aside (tabs, pages)
        public static bool ParksSections(string layout) => layout == CardOptions.LayoutTabs || layout == CardOptions.LayoutPages;

        // Whether the card is drawn as sideways pages (CardPagerView turns them)
        public static bool TurnsPages(string effectiveLayout) => effectiveLayout == CardOptions.LayoutPages;

        // Whether the stack draws this block as a compact row: its own display says so (Expandable, BlockDisplayRule), or the card is an
        // accordion and the block is a navigation target -- counted with its reveal gate open, so a block waiting for Show After Reading is
        // a (hidden) row too, and becomes visible as a row, never as a whole block in a card of rows
        public static bool ShowsCompact(CardNavigationSettings settings, BlockStackBuilder.Entry entry, BlockRegistry registry)
        {
            if (BlockDisplayRule.ShowsCompact(entry)) return true;
            return Layout(settings) == CardOptions.LayoutAccordion && NavOntologyRule.IsTarget(entry, registry, _ => true);
        }

        // Open One At A Time: opening a row closes the others -- an accordion option, off in every other layout
        public static bool OneOpenAtATime(CardNavigationSettings settings) =>
            Layout(settings) == CardOptions.LayoutAccordion && (settings?.accordion_one_open ?? false);

        // The Top Bar the card uses: the wall's, except that pages always use the Family Pager (the page's name and its chevrons), and the
        // tabs layout with neither a Top Bar nor a Bottom Bar uses Text Tabs (a tabs card always has a way between its sections; the Editor
        // warns about it)
        public static string TopElement(CardNavigationSettings settings)
        {
            var nav = settings ?? new CardNavigationSettings();
            string layout = Layout(nav);
            if (layout == CardOptions.LayoutPages) return BuiltInNavElements.FamilyPager;
            return layout == CardOptions.LayoutTabs && IsNone(nav.top) && IsNone(nav.bottom) ? BuiltInNavElements.TextTabs : nav.top;
        }

        // Whether the tabs layout leaves the card with no element of its own choosing (what the Editor warns about)
        public static bool TabsWithoutAnElement(CardNavigationSettings settings) =>
            Layout(settings) == CardOptions.LayoutTabs && IsNone(settings?.top) && IsNone(settings?.bottom);

        // In tabs and pages: whether a block of section `blockSection` (-1 = none) shows while section `shownSection` does
        public static bool ShowsWith(int blockSection, int shownSection) => blockSection < 0 || blockSection == shownSection;

        // What a jump does before the stack scrolls to its block (NavController asks; both may apply): in tabs and pages, show the block's
        // section (pages: turn to its page)
        public static bool ShowsSectionBeforeJump(string effectiveLayout) => ParksSections(effectiveLayout);

        // ...and a block drawn as a CLOSED compact row (an accordion row, an expandable block in any layout) opens: the visitor jumped there
        // to see the block, not its one-line header
        public static bool OpensRowBeforeJump(bool targetIsClosedRow) => targetIsClosedRow;

        // The section the scroll-spy names: in tabs and pages, the section shown (the scroll moves inside it, never to another); else what
        // was measured
        public static int SpySection(string effectiveLayout, int shownSection, int measured) =>
            ParksSections(effectiveLayout) && shownSection >= 0 ? shownSection : measured;

        private static bool IsNone(string key) => string.IsNullOrWhiteSpace(key) || key == CardOptions.NavNone;
    }
}
