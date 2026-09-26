using System.Collections.Generic;

namespace TileStories
{
    // One block kind (_3.1 section 3): its key, family, variants, fields and allowed display modes. The ONE
    // description of a kind -- the runtime view, the Editor field drawer and the validator all read it, so a
    // kind registered by an app is drawn and checked exactly like a built-in one.
    public sealed class BlockKindDefinition
    {
        // Stored in BlockInstanceData.kind ("header")
        public string Key;
        // The ontology family (_3.2): "about", "media", "guides", "stories", "ar", "play", "visit", "community", "meta"
        public string Family;
        // The Editor's name for the kind ("Header")
        public string DisplayName;
        // The Editor (i) text of the kind
        public string Help;
        public IReadOnlyList<string> Variants;
        // Used when an instance names no variant or one this kind does not have
        public string DefaultVariant;
        public IReadOnlyList<BlockFieldDefinition> Fields;
        public IReadOnlyList<string> DisplayModes;

        public bool HasVariant(string variant) => Variants != null && variant != null && ContainsString(Variants, variant);

        // The field definition with this key, or null
        public BlockFieldDefinition Field(string key)
        {
            if (Fields == null) return null;
            for (int i = 0; i < Fields.Count; i++)
                if (Fields[i].Key == key) return Fields[i];
            return null;
        }

        private static bool ContainsString(IReadOnlyList<string> list, string value)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i] == value) return true;
            return false;
        }
    }
}
