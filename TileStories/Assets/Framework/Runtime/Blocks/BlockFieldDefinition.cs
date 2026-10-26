using System.Collections.Generic;

namespace TileStories
{
    // How a block field's value is stored in BlockFieldValue and drawn by the Editor (_3.1 section 5.3)
    public enum BlockFieldType
    {
        LocalizedText,      // text: one single-line value per wall language
        LocalizedLongText,  // text: one multi-line value per wall language
        Number,             // number
        Toggle,             // flag
        Choice,             // value: one of Options
        Asset,              // asset: a path under card_settings.media_resources_path
        Items,              // items: a repeater whose rows hold ItemFields
        PoiRef,             // value: a POI id of this wall
        Color,              // value: a colour written "#RRGGBB" or "#RGB" (content, e.g. a swatch; BlockFieldReader.TryParseColor)
        Url,                // value: a web link the card may open (http / https: WebLinkRule), one for every language
        Time,               // value: a time in a clip, "1:30" / "90" / "1:02:03" (TimeCodeRule), one for every language
        BlockRef,           // value: the key of another block of the SAME POI's card (BlockRefRule: does the card scroll to it?)
        Elements,           // items: TYPED rows, each naming its element kind (ElementRow over BlockItemData, ElementRegistry; _3.3 R14.1)
    }

    // One field of a block kind: the schema the view, the Editor drawer and the validator all read
    public sealed class BlockFieldDefinition
    {
        public string Key;
        public BlockFieldType Type;
        // The Editor row label ("Title")
        public string Label;
        // A block whose required field is empty is not shown (BlockStackBuilder); an Items row whose required sub-field
        // is empty is not shown either (BlockFieldReader.ItemIsComplete)
        public bool Required;
        // The Editor (i) text: framework-authored, app-agnostic
        public string Help;
        // Choice only (BlockRegistry.Validate refuses them on any other type): the option values (stored in config) and, in the same
        // order, the names the Editor shows
        public IReadOnlyList<string> Options;
        public IReadOnlyList<string> OptionLabels;
        // Choice only: the option read while the block stores none (null = none: the field reads empty, as before)
        public string ChoiceDefault;
        // Choice (with a ChoiceDefault) or Toggle only, top level: a wall may set this field's default for every block of the kind in the
        // Block Library (card_settings.kinds[].field_defaults: an option, or "true" / "false" for a Toggle, which is off otherwise); a block's
        // own value still wins (BlockLibraryRule.Choice / Flag)
        public bool LibraryDefault;
        // Items only: the sub-fields of one row (never Items themselves)
        public IReadOnlyList<BlockFieldDefinition> ItemFields;
        // Asset only: what the file must be (MediaPathRule checks the stored path against it)
        public MediaKind Media;
        // Asset only: the field may hold its own file per wall language (a narration and its captions recorded in each language);
        // the Editor then adds one row per other language under it, blank = the file above (BlockFieldReader.Asset)
        public bool PerLanguage;
        // Number only: the value range (the Editor's slider) and the value read while nothing is stored
        public float NumberMin;
        public float NumberMax = 1f;
        public float NumberDefault;
        // When the Editor draws this field (null = always): e.g. a field only one look uses. A hidden field keeps its value
        // (FieldVisibilityRule; BlockRegistry.Validate checks the looks and fields it names)
        public FieldShownWhen ShownWhen;
        // BlockRef only: leaving it unpicked is a normal state (the holder then just has no jump -- a hotspot pin keeps its own popup), so
        // the Editor does not warn about a blank pick; a pick that leads nowhere still gets its warning (BlockRefRule.Check)
        public bool BlankIsFine;
    }
}
