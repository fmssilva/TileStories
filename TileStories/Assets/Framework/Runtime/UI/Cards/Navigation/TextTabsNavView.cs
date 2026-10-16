namespace TileStories
{
    // The text_tabs navigation element (_3.2 section 5.1, canvas A1): one word per section of the card in a strip under the header, the
    // section being read underlined and bold. Everything it does is TabsNavView's; its tabs carry no icon.
    public sealed class TextTabsNavView : TabsNavView
    {
        public TextTabsNavView() : base(BuiltInNavElements.TextTabs, "nav-text-tabs", withIcons: false) { }
    }
}
