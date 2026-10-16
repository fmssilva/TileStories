namespace TileStories
{
    // The icon_tabs navigation element (_3.2 section 5.1, canvas A2 top row): TabsNavView's tabs with each family's icon, painted in the
    // family's colour, above its words. Labels says whether a tab shows the icon, the words or both (NavLabelRule); a section with no icon
    // (Group By = format) shows its words. icon_tabs_chips holds one too, under its own key (its jumps are reported as its own).
    public sealed class IconTabsNavView : TabsNavView
    {
        public IconTabsNavView() : this(BuiltInNavElements.IconTabs, "nav-icon-tabs") { }

        public IconTabsNavView(string elementKey, string name) : base(elementKey, name, withIcons: true) { }
    }
}
