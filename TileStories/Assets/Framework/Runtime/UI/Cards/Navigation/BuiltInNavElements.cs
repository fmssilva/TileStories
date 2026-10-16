namespace TileStories
{
    // The framework's navigation elements (_3.2 section 5), registered into NavElementRegistry.Shared. Their keys are what
    // card_settings.navigation stores; the label and help are what the POI Editor's slot popup shows (ASCII, app-agnostic).
    public static class BuiltInNavElements
    {
        public const string TextTabs = "text_tabs";
        public const string IconTabs = "icon_tabs";
        public const string IconTabsChips = "icon_tabs_chips";
        public const string KindChips = "kind_chips";
        public const string Segmented = "segmented";
        public const string ProgressMap = "progress_map";
        public const string BottomBar = "bottom_bar";
        public const string Dock = "dock";
        public const string BarSheet = "bar_sheet";
        public const string NextUp = "next_up";

        public static void Register(NavElementRegistry registry)
        {
            registry.Register(new NavElementDefinition(TextTabs, NavSlotKeys.Top, "Text Tabs",
                "Text Tabs: one word per section of the card (a family: About, Media, Stories...) in a strip under the header. The "
                + "section being read is underlined and bold; a tap moves the card to that section's first block. A strip wider than the "
                + "screen scrolls sideways with a finger. A card with one section gets no strip: one tab is not a choice.",
                NavChoice.Sections, showsCounts: true),
                () => new TextTabsNavView());
            registry.Register(new NavElementDefinition(IconTabs, NavSlotKeys.Top, "Icon Tabs",
                "Icon Tabs: the same tabs with each family's icon, in the family's colour, above its word; Labels picks icon, word or "
                + "both (a section with no icon, when grouped by format, always shows its word). Needs two sections, like Text Tabs.",
                NavChoice.Sections, readsLabels: true, showsCounts: true),
                () => new IconTabsNavView());
            registry.Register(new NavElementDefinition(IconTabsChips, NavSlotKeys.Top, "Icon Tabs With Chips",
                "Icon Tabs With Chips: the icon tabs, and under them a row of chips for the kinds of block the marked section holds on "
                + "this card (a gallery, a video, a 3D model...) -- any block in two taps. The chips follow the section being read; a section "
                + "with one kind shows no chips. Takes a little more height than the tabs alone.",
                NavChoice.Sections, readsLabels: true, showsCounts: true),
                () => new IconTabsChipsNavView());
            registry.Register(new NavElementDefinition(KindChips, NavSlotKeys.Top, "Kind Chips",
                "Kind Chips: every kind of block the card holds as one chip, a dot of its family's colour beside its words; the chip of "
                + "the block being read is outlined and bold. Best for short cards: on a long one most chips sit off screen and the visitor "
                + "must swipe the row to find them. Shown on a card with one family too, since it lists kinds, not families.",
                NavChoice.Blocks, showsCounts: true),
                () => new KindChipsNavView());
            registry.Register(new NavElementDefinition(Segmented, NavSlotKeys.Top, "Segmented",
                "Segmented: the wall's Groups (the Groups table; each family joins one in the Families table's Group column) as equal "
                + "segments that always fit the screen, with no sideways scroll. The group being read is filled and bold. A group with "
                + "no block on a card is left out, and a card whose blocks all fall in one group gets no segments.",
                NavChoice.Groups, showsCounts: true),
                () => new SegmentedNavView());
            registry.Register(new NavElementDefinition(ProgressMap, NavSlotKeys.Top, "Progress Map",
                "Progress Map: a thin bar split into one piece per section, each as wide as its share of the card's blocks, with the "
                + "family's icon above it. The pieces up to the one being read are solid, so it also shows how far the visitor got. Takes "
                + "the least height of all; a tap on a piece moves the card there. The pieces show their share only while the "
                + "card's families fit the screen side by side (about 4 or 5 on a phone): every piece stays one finger wide so it can "
                + "be tapped, so with more families most pieces are that same width and the bar scrolls sideways.",
                NavChoice.Sections),
                () => new ProgressMapNavView());
            registry.Register(new NavElementDefinition(BottomBar, NavSlotKeys.Bottom, "Bottom Bar",
                "Bottom Bar: a bar at the bottom of the card, under its sticky button, where a thumb reaches: one slot per section (its "
                + "icon above its word; Labels picks icon, word or both), as many as Bar Slots. A card with more sections than that turns "
                + "the last slot into More, which lists the rest just above the bar. The section being read is underlined and bold (More "
                + "for one in its list). The bar slides away with the sticky button while the visitor reads down (Hide While Scrolling) and "
                + "both give way together where a stop leaves too little room to read. Needs two sections, like the tabs.",
                NavChoice.Sections, readsLabels: true, readsBarSlots: true),
                () => new BottomBarNavView());
            registry.Register(new NavElementDefinition(Dock, NavSlotKeys.Bottom, "Dock",
                "Dock: a pill of icons at the bottom of the card, where a thumb reaches. The section being read widens to its icon and "
                + "its word while every other slot stays an icon, and the widening follows the card as the visitor reads, not the tap. "
                + "Bar Slots sets how many slots the pill holds (the last becomes More when the sections overflow, like the Bottom Bar), so "
                + "it never grows wider than the screen. Labels does not apply. It slides away with the sticky button while the visitor "
                + "reads down (Hide While Scrolling). Needs two sections, like the tabs.",
                NavChoice.Sections, readsBarSlots: true),
                () => new DockNavView());
            registry.Register(new NavElementDefinition(BarSheet, NavSlotKeys.Bottom, "Bar With Kinds",
                "Bar With Kinds: the Bottom Bar, with one more level on a second tap. The first tap on a section moves the card there; a "
                + "second tap on the section already being read opens a small list above the bar with the kinds of block it holds on this "
                + "card (a gallery, a video, a 3D model...) and how many of each, and a tap on a kind moves the card to it. A small arrow on "
                + "the section being read says a tap would open that list; a section with one kind has none. The list closes when a kind "
                + "is picked, when the section is tapped again, when the visitor reads on into another section and when More opens. "
                + "Labels, Bar Slots and More work as in the Bottom Bar. Needs two sections, like the tabs.",
                NavChoice.Sections, readsLabels: true, readsBarSlots: true),
                () => new BarSheetNavView());
            registry.Register(new NavElementDefinition(NextUp, NavSlotKeys.Bottom, "Up Next",
                "Up Next: a bar at the bottom of the card that names the block after the one being read -- its heading, and its place among "
                + "the card's blocks, like 5 of 13 -- beside a Next button that moves the card there. A gentle way through the card for a "
                + "visitor who just wants to be led. At the last block it says the card is finished and offers no Next. Works on a card "
                + "with one section too, since it follows blocks, not sections.",
                NavChoice.Blocks),
                () => new NextUpNavView());
        }
    }
}
