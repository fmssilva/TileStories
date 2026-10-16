namespace TileStories
{
    // What an icon-capable navigation item shows (_3.2 section 3, Labels), pure. Icon And Text (the default) shows both, Icon the
    // icon alone, Text the words alone; an unknown value reads as the default. An item with NO icon (a format section: formats have
    // none) always shows its words, whatever Labels says -- an item is never empty. Words are never left out where no icon stands for
    // them, and an icon alone still names its item to a screen reader (the view sets its tooltip)
    public static class NavLabelRule
    {
        public static bool ShowsIcon(string labels, bool hasIcon) => hasIcon && labels != CardOptions.LabelsText;

        public static bool ShowsWords(string labels, bool hasIcon) => !hasIcon || labels != CardOptions.LabelsIcon;
    }
}
