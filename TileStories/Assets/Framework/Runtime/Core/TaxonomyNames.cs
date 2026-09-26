namespace TileStories
{
    // What a visitor reads for a taxonomy key a POI stores: the row's name (category / badge / outline label, a
    // hierarchy level's name), or the key itself when the row has no name or no row matches. Every row keeps a
    // generated key apart from its name, so display text is ALWAYS looked up here, never read from the key. Pure.
    public static class TaxonomyNames
    {
        public static string Category(WallConfigData config, string key) =>
            NameOr(config?.category_styles?.Find(e => e != null && e.key == key)?.label, key);

        public static string Badge(WallConfigData config, string key) =>
            NameOr(config?.badge_categories?.Find(e => e != null && e.key == key)?.label, key);

        public static string Status(WallConfigData config, string key) =>
            NameOr(config?.outline_levels?.Find(e => e != null && e.key == key)?.label, key);

        public static string Level(WallConfigData config, string key) =>
            NameOr(config?.hierarchy_levels?.Find(e => e != null && e.key == key)?.level_name, key);

        // A row's own name, or its key when the name is blank
        public static string NameOr(string name, string key) => string.IsNullOrWhiteSpace(name) ? key : name;
    }
}
