using System.Collections.Generic;

namespace TileStories
{
    // Where an element sits in the Editor's "+ Add element" menu (_3.3 R14.2)
    public enum ElementCategory { Text, Media, Structure }

    // The part an element can play in an Automatic preview (ElementPreviewRule, R14.3): the preview's picture, its title, its text -- or none.
    // TitleLead: words that lead into a title (an eyebrow) -- they join the preview only when they sit right before the title it takes
    public enum ElementPreviewRole { None, Picture, Title, Text, TitleLead }

    // One element kind (_3.3 R14.1): the ONE description of a row of an Elements field. The Editor's element table, the validator, the rules
    // (preview, reading time, summary, search, motion) and, from S2, the component that draws it all read it -- so an element an app
    // registers behaves exactly like a built-in one. The BlockKindDefinition pattern, one level down.
    public sealed class ElementKindDefinition
    {
        // Stored in the row's `element` sub-field ("paragraph")
        public string Key;
        // The Editor's name ("Paragraph")
        public string DisplayName;
        // The Editor (i) text: framework-authored, app-agnostic
        public string Help;
        // A CardIcons key: the element table's and the "+ Add element" menu's icon
        public string IconKey;
        public ElementCategory Category;
        // The content sub-fields, as an Items row's (never Items or Elements: rows cannot nest). Null = none (a divider)
        public IReadOnlyList<BlockFieldDefinition> Fields;
        // The style options: CLOSED lists only -- each a Choice with a default -- so a row always has a designed look (R14.2)
        public IReadOnlyList<BlockFieldDefinition> StyleFields;
        // The media list: what each item must be (None = the element holds no media) and how many items a complete row holds
        public MediaKind Media = MediaKind.None;
        public int MinMedia;
        public int MaxMedia;
        // The motion effects the element offers (ElementOptions.Motions, Auto always among them) and the effect Auto plays for it (R17.3)
        public IReadOnlyList<string> MotionChoices;
        public string AutoMotion;
        // The taps the element offers in the page, its default first (ElementOptions.Taps); null = no choice (a carousel always swipes)
        public IReadOnlyList<string> TapChoices;
        // The seconds one media item adds to the block's reading time (text adds its words: ElementRules)
        public float SecondsPerMedia;
        public ElementPreviewRole PreviewRole;
        // The text sub-fields search finds the block by (every language), and whether the media captions count too
        public IReadOnlyList<string> SearchFields;
        public bool SearchesCaptions;

        // The content or style field with this key, or null
        public BlockFieldDefinition Field(string key)
        {
            foreach (var list in new[] { Fields, StyleFields })
                if (list != null)
                    foreach (var f in list)
                        if (f != null && f.Key == key) return f;
            return null;
        }

        // Whether the element offers this motion effect
        public bool OffersMotion(string effect) => Contains(MotionChoices, effect);

        // Whether the element offers this tap
        public bool OffersTap(string tap) => Contains(TapChoices, tap);

        private static bool Contains(IReadOnlyList<string> list, string value)
        {
            if (list == null) return false;
            foreach (string s in list)
                if (s == value) return true;
            return false;
        }
    }
}
