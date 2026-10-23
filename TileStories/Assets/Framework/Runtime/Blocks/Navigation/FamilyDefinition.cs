namespace TileStories
{
    // One family of the card's ontology (_3.2 section 2.2): the visitor's intent behind a group of block kinds
    // ("what do I want?" -- About, Media, Listen...). A family is a REGISTERED DEFINITION, not a string literal:
    // BlockRegistry.RegisterFamily takes one, and registering a kind whose family is not registered throws, so a
    // typo can no longer invent a family. An app registers its own family exactly like a kind, with zero
    // Framework edits; its label key goes in the app's own string table (CardStringSources).
    // IconKey and ColorToken joined at _3.2 step 6 with the elements that draw them (icon tabs, the progress map, the kind chips' dots).
    public sealed class FamilyDefinition
    {
        // What a kind's Family says and what a wall's Families row points at (data identity: never renamed)
        public string Key;
        // Where the family sits among the others. Built-in families leave gaps of 10 so an app can slot between
        // two of them (Order 35 lands between guides at 30 and stories at 40). A wall's own row overrides it.
        public int Order;
        // The words a visitor reads for this family: a CardStrings key, so it is localized and a wall can reword
        // it in Card Texts. Empty for a family that is never shown as a navigation target.
        public string LabelKey;
        // The family's icon: one of CardIcons.All (a line icon drawn by the card's stylesheet, no picture). An app family picks one too
        public string IconKey;
        // The family's colour, as the name of a THEME colour ("--ts-family-about", Theme.default.asset): a view reads it through
        // CardTokenColours (the theme it sits under), so the value stays data. An app family names one of the theme's colours
        // (ThemeTokenGuardTests holds every registered family to one). Never shown alone: an icon or the family's words always travel
        // with it (30-ui-content)
        public string ColorToken;
        // Whether the family is offered as a navigation target at all (a nav block is never a nav target)
        public bool InNav = true;
        // Whether the family's blocks close the card, whatever order they were authored in (meta: Sources).
        // Read by the Block Order option end_families_last (_3.2 section 2.3).
        public bool AtEnd;
    }
}
