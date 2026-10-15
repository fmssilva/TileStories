namespace TileStories
{
    // The framework's own families (_3.2 section 2.2), registered once into a BlockRegistry BEFORE its kinds --
    // Register refuses a kind whose family is not registered yet. The constants are the ONE home for each name:
    // every BlockKindDefinition.Family and every rule that tests a family reads them, never a string literal.
    //
    // Order = the canvas order, in gaps of 10 so an app can slot its own family between two built-in ones.
    // The labels themselves are data, not code: LabelKey names a CardStrings row (CardStrings.asset, en + pt),
    // so a visitor reads them in their language and a wall can reword them in Card Texts.
    //
    // The `navigation` family is NOT here: it arrives at _3.2 step 11 with its first kind (a nav block is never
    // a nav target, so registering it before anything uses it would be a family with no kinds.)
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
        public const string Meta = "meta";

        // Register every built-in family into `registry`, before its kinds
        public static void Register(BlockRegistry registry)
        {
            registry.RegisterFamily(new FamilyDefinition { Key = About, Order = 10, LabelKey = CardStrings.Keys.FamilyAbout });
            registry.RegisterFamily(new FamilyDefinition { Key = Media, Order = 20, LabelKey = CardStrings.Keys.FamilyMedia });
            // - "Listen", not "Guides": its only kind is the audio guide. The KEY stays `guides` (data identity).
            registry.RegisterFamily(new FamilyDefinition { Key = Guides, Order = 30, LabelKey = CardStrings.Keys.FamilyGuides });
            registry.RegisterFamily(new FamilyDefinition { Key = Stories, Order = 40, LabelKey = CardStrings.Keys.FamilyStories });
            registry.RegisterFamily(new FamilyDefinition { Key = Ar, Order = 50, LabelKey = CardStrings.Keys.FamilyAr });
            registry.RegisterFamily(new FamilyDefinition { Key = Play, Order = 60, LabelKey = CardStrings.Keys.FamilyPlay });
            registry.RegisterFamily(new FamilyDefinition { Key = Visit, Order = 70, LabelKey = CardStrings.Keys.FamilyVisit });
            registry.RegisterFamily(new FamilyDefinition { Key = Community, Order = 80, LabelKey = CardStrings.Keys.FamilyCommunity });
            // - the credits close the card and are no navigation target: the two flags that make Sources last
            registry.RegisterFamily(new FamilyDefinition
            {
                Key = Meta, Order = 90, LabelKey = CardStrings.Keys.FamilyMeta, InNav = false, AtEnd = true,
            });
        }
    }
}
