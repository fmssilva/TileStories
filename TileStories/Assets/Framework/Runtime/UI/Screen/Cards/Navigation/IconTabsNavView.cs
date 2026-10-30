namespace TileStories
{
    // The icon tabs icon_tabs_chips draws over its chip row (_3.2 section 5.1): TabsNavView's tabs with each family's icon, painted in the
    // family's colour, above its words. Labels says whether a tab shows the icon, the words or both (NavLabelRule); a section with no icon
    // (Group By = format) shows its words. Its jumps are reported under the element key that holds it.
    public sealed class IconTabsNavView : TabsNavView
    {
        public IconTabsNavView(string elementKey, string name) : base(elementKey, name, withIcons: true) { }
    }
}
