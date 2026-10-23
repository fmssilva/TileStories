namespace TileStories
{
    // The framework's own families (_3.2 section 2.2), registered once into a BlockRegistry BEFORE its kinds --
    // Register refuses a kind whose family is not registered yet. The constants are the ONE home for each name:
    // every BlockKindDefinition.Family and every rule that tests a family reads them, never a string literal.
    //
    // Order = the canvas order, in gaps of 10 so an app can slot its own family between two built-in ones.
    // The labels themselves are data, not code: LabelKey names a CardStrings row (CardStrings.asset, en + pt),
    // so a visitor reads them in their language and a wall can reword them in Card Texts. IconKey names a CardIcons
    // drawing and ColorToken a theme colour (_3.2 step 6, _3.3 R12.1): the values stay in the theme asset.
    //
    // The `navigation` family (order 85, _3.2 step 11a) arrived with its first kind, nav_icon_grid: its blocks move the visitor inside
    // the card, so it is never a navigation target itself (In Nav off) -- no tab, bar or menu ever lists a nav block.
    public static class BuiltInFamilies
    {
        public const string About = "about";
        public const string Media = "media";
        public const string Guides = "guides";
        public const string Stories = "stories";
        public const string Ar = "ar";
        public const string Play = "play";
        public const string Visit = "visit";
        public const string Community = "community";
        public const string Navigation = "navigation";
        public const string Meta = "meta";

        // Register every built-in family into `registry`, before its kinds
        public static void Register(BlockRegistry registry)
        {
            registry.RegisterFamily(Family(About, 10, CardStrings.Keys.FamilyAbout, CardIcons.Info));
            registry.RegisterFamily(Family(Media, 20, CardStrings.Keys.FamilyMedia, CardIcons.Picture));
            // - "Listen", not "Guides": its only kind is the audio guide. The KEY stays `guides` (data identity).
            registry.RegisterFamily(Family(Guides, 30, CardStrings.Keys.FamilyGuides, CardIcons.Listen));
            registry.RegisterFamily(Family(Stories, 40, CardStrings.Keys.FamilyStories, CardIcons.Book));
            // - the AR family's icon is the target the show-on-wall button already wears: "see it there"
            registry.RegisterFamily(Family(Ar, 50, CardStrings.Keys.FamilyAr, CardIcons.ShowOnWall));
            registry.RegisterFamily(Family(Play, 60, CardStrings.Keys.FamilyPlay, CardIcons.Play));
            registry.RegisterFamily(Family(Visit, 70, CardStrings.Keys.FamilyVisit, CardIcons.Location));
            registry.RegisterFamily(Family(Community, 80, CardStrings.Keys.FamilyCommunity, CardIcons.People));
            // - the nav blocks (an icon grid, a contents list): in the card's flow, never a target of its navigation
            var navigation = Family(Navigation, 85, CardStrings.Keys.FamilyNavigation, CardIcons.Grid);
            navigation.InNav = false;
            registry.RegisterFamily(navigation);
            // - the credits close the card and are no navigation target: the two flags that make Sources last
            var meta = Family(Meta, 90, CardStrings.Keys.FamilyMeta, CardIcons.Sources);
            meta.InNav = false;
            meta.AtEnd = true;
            registry.RegisterFamily(meta);
        }

        // A built-in family: its colour is always the card token named after its key
        private static FamilyDefinition Family(string key, int order, string labelKey, string iconKey) => new()
        {
            Key = key, Order = order, LabelKey = labelKey, IconKey = iconKey, ColorToken = "--ts-family-" + key,
        };
    }
}
