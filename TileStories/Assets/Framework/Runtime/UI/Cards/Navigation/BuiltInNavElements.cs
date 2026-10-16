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
                + "the least height of all; a tap on a piece moves the card there.",
                NavChoice.Sections),
                () => new ProgressMapNavView());
        }
    }
}
