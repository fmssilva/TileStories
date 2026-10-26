namespace TileStories
{
    // What a block page's Page Chrome shows (_3.3 R17.5): the wall's defaults (Detail Card > Card Components) unless the block's own Component
    // Overrides row for the Page Chrome is switched on -- then that row's values, both parts. Pure: no scene, no panel.
    public readonly struct PageChromeChoice
    {
        // The one-line breadcrumb beside the Back chevron, and the Previous / Next row at the page's foot
        public readonly bool Breadcrumb;
        public readonly bool PrevNext;

        public PageChromeChoice(bool breadcrumb, bool prevNext)
        {
            Breadcrumb = breadcrumb;
            PrevNext = prevNext;
        }

        // Both parts: what a wall with no settings, and a page no block owns (a picture viewer), shows
        public static PageChromeChoice Both => new(true, true);
    }

    public static class PageChromeRule
    {
        // The wall's defaults alone (no block asks): a wall without the settings shows both parts
        public static PageChromeChoice Wall(CardComponentSettings wall) =>
            wall == null ? PageChromeChoice.Both : new PageChromeChoice(wall.page_chrome_breadcrumb, wall.page_chrome_prev_next);

        // The chrome of `block`'s page: its override when on, else the wall's
        public static PageChromeChoice Of(CardComponentSettings wall, BlockInstanceData block)
        {
            var overriding = BlockSurfaces.OverrideOf(block, ElementOptions.ComponentPageChrome);
            return overriding.IsOn ? new PageChromeChoice(overriding.BreadcrumbShown, overriding.PrevNextShown) : Wall(wall);
        }
    }
}
