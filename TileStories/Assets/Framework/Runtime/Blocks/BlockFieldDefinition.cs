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
    }

    // One field of a block kind: the schema the view, the Editor drawer and the validator all read
    public sealed class BlockFieldDefinition
    {
        public string Key;
        public BlockFieldType Type;
        // The Editor row label ("Title")
        public string Label;
        // A block whose required field is empty is not shown (BlockStackBuilder)
        public bool Required;
        // The Editor (i) text: framework-authored, app-agnostic
        public string Help;
        // Choice only: the option values (stored in config) and, in the same order, the names the Editor shows
        public IReadOnlyList<string> Options;
        public IReadOnlyList<string> OptionLabels;
        // Items only: the sub-fields of one row (never Items themselves)
        public IReadOnlyList<BlockFieldDefinition> ItemFields;
    }
}
