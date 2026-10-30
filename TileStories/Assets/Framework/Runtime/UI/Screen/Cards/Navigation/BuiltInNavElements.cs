namespace TileStories
{
    // The framework's navigation elements kept for a later rebuild (_3.3 TODOs, [rebuild]), registered into NavElementRegistry.Shared. Their
    // keys are what card_settings.navigation stores; the POI Editor offers none of them until each is rebuilt on the Navigation Bar's
    // pattern, and the card draws one only while the Navigation Bar is off (ASCII, app-agnostic label and help).
    public static class BuiltInNavElements
    {
        public const string IconTabsChips = "icon_tabs_chips";
        public const string KindChips = "kind_chips";
        public const string ProgressMap = "progress_map";
        public const string FamilyPager = "family_pager";
        public const string BarSheet = "bar_sheet";
        public const string NextUp = "next_up";
        public const string DrawerTree = "drawer_tree";

        public static void Register(NavElementRegistry registry)
        {
            registry.Register(new NavElementDefinition(IconTabsChips, NavSlotKeys.Top, "Icon Tabs With Chips",
                "Icon Tabs With Chips: icon tabs, and under them a row of chips for the kinds of block the marked section holds on "
                + "this card (a gallery, a video, a 3D model...) -- any block in two taps. The chips follow the section being read; a section "
                + "with one kind shows no chips. Takes a little more height than tabs alone.",
                NavChoice.Sections, readsLabels: true, showsCounts: true),
                () => new IconTabsChipsNavView());
            registry.Register(new NavElementDefinition(KindChips, NavSlotKeys.Top, "Kind Chips",
                "Kind Chips: every kind of block the card holds as one chip, a dot of its family's colour beside its words; the chip of "
                + "the block being read is outlined and bold. Best for short cards: on a long one most chips sit off screen and the visitor "
                + "must swipe the row to find them. Shown on a card with one family too, since it lists kinds, not families.",
                NavChoice.Blocks, showsCounts: true),
                () => new KindChipsNavView());
            registry.Register(new NavElementDefinition(ProgressMap, NavSlotKeys.Top, "Progress Map",
                "Progress Map: a thin bar split into one piece per section, each as wide as its share of the card's blocks, with the "
                + "family's icon above it. The pieces up to the one being read are solid, so it also shows how far the visitor got. Takes "
                + "the least height of all; a tap on a piece moves the card there. The pieces show their share only while the "
                + "card's families fit the screen side by side (about 4 or 5 on a phone): every piece stays one finger wide so it can "
                + "be tapped, so with more families most pieces are that same width and the bar scrolls sideways.",
                NavChoice.Sections),
                () => new ProgressMapNavView());
            registry.Register(new NavElementDefinition(FamilyPager, NavSlotKeys.Top, "Family Pager",
                "Family Pager: the section being read, named between two arrows -- its family's icon, its word and its place (2 of 8). An "
                + "arrow moves the card to the section before or after it; the first and last sections offer no arrow on their open side. "
                + "The Pages layout always uses it as its header, and the Tabs layout when no other element switches its sections (the "
                + "arrows are the way between pages without a swipe). Needs two sections.",
                NavChoice.Sections),
                () => new FamilyPagerNavView());
            registry.Register(new NavElementDefinition(BarSheet, NavSlotKeys.Bottom, "Bar With Kinds",
                "Bar With Kinds: a bar at the bottom of the card with one slot per section (as many as Bar Slots, the last one More when "
                + "the sections overflow), and one more level on a second tap. The first tap on a section moves the card there; a second tap "
                + "on the section already being read opens a small list above the bar with the kinds of block it holds on this card (a "
                + "gallery, a video, a 3D model...) and how many of each, and a tap on a kind moves the card to it. A small arrow on the section "
                + "being read says a tap would open that list; a section with one kind has none. The list closes when a kind is picked, when "
                + "the section is tapped again, when the visitor reads on into another section and when More opens. Needs two sections.",
                NavChoice.Sections, readsLabels: true, readsBarSlots: true),
                () => new BarSheetNavView());
            registry.Register(new NavElementDefinition(NextUp, NavSlotKeys.Bottom, "Up Next",
                "Up Next: a bar at the bottom of the card that names the block after the one being read -- its heading, and its place among "
                + "the card's blocks, like 5 of 13 -- beside a Next button that moves the card there. A gentle way through the card for a "
                + "visitor who just wants to be led. At the last block it says the card is finished and offers no Next. Works on a card "
                + "with one section too, since it follows blocks, not sections.",
                NavChoice.Blocks),
                () => new NextUpNavView());
            registry.Register(new NavElementDefinition(DrawerTree, NavSlotKeys.Menu, "Drawer",
                "Drawer: the whole card as an outline in a drawer from the side -- every section with how many blocks it holds, and under "
                + "each the kinds of block it has on this card (a gallery, a video, a 3D model...) with theirs. A tap on a line moves the card "
                + "there and closes the drawer. It waits behind the menu button in the card's header, so it is never the only way round. "
                + "Works on a card with one section too, since it lists kinds.",
                NavChoice.Blocks),
                () => new DrawerTreeNavView());
        }
    }
}
