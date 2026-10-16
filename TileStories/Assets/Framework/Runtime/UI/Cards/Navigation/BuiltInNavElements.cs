namespace TileStories
{
    // The framework's navigation elements (_3.2 section 5), registered into NavElementRegistry.Shared. Their keys are what
    // card_settings.navigation stores; the label and help are what the POI Editor's slot popup shows (ASCII, app-agnostic).
    public static class BuiltInNavElements
    {
        public const string TextTabs = "text_tabs";

        public static void Register(NavElementRegistry registry)
        {
            registry.Register(new NavElementDefinition(TextTabs, NavSlotKeys.Top, "Text Tabs",
                "Text Tabs: one word per section of the card (a family: About, Media, Stories...) in a strip under the header. The "
                + "section being read is underlined and bold; a tap moves the card to that section's first block. A strip wider than the "
                + "screen scrolls sideways with a finger."),
                () => new TextTabsNavView());
        }
    }
}
