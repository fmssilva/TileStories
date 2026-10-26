using System;
using System.Collections.Generic;

namespace TileStories
{
    // Every block kind the card can show, with the view that draws it (_3.1 section 3), and every FAMILY those
    // kinds belong to (_3.2 section 2.2). The extension point: BuiltInBlocks registers the framework's families
    // and then its kinds into Shared, and an app registers its own the same way (BlockRegistry.Shared.RegisterFamily
    // / .Register) with zero Framework edits. Tests build their own instance.
    // Families are registered BEFORE kinds: Register refuses a kind whose family is unknown, so a typo in a
    // Family string can no longer invent a family that nothing ever navigates to.
    public sealed class BlockRegistry
    {
        private readonly List<BlockKindDefinition> _kinds = new();
        private readonly Dictionary<string, Func<IBlockView>> _views = new();
        private readonly List<FamilyDefinition> _families = new();

        // The app's registry: the framework kinds plus whatever the app registered
        public static BlockRegistry Shared
        {
            get
            {
                if (_shared == null)
                {
                    _shared = new BlockRegistry();
                    BuiltInBlocks.Register(_shared);
                }
                return _shared;
            }
        }
        private static BlockRegistry _shared;

        // Every registered kind, in registration order (the order the catalog and its tests are written in)
        public IReadOnlyList<BlockKindDefinition> All => _kinds;

        // Every registered family, in registration order (BuiltInFamilies first, then whatever an app added).
        // Their own Order field, not this list's order, is what sorts them -- see Ordered.
        public IReadOnlyList<FamilyDefinition> Families => _families;

        // Every registered kind grouped by family, for a person choosing one (the Editor's Block Library and "+ Add block"): the families
        // sorted by their registered Order, then the kinds of a family in registration order. An app's kind therefore sits with its own
        // family, and an app family with Order 35 really lands between guides (30) and stories (40).
        public IReadOnlyList<BlockKindDefinition> Ordered
        {
            get
            {
                // - a list of the families that actually have a kind here, sorted by their registered Order. A stable insertion sort
                //   keeps two families that share an Order in registration order, so the result never depends on sort internals.
                var families = new List<FamilyDefinition>();
                foreach (var kind in _kinds)
                {
                    if (!TryGetFamily(kind.Family, out var family) || families.Contains(family)) continue;
                    int at = families.Count;
                    while (at > 0 && families[at - 1].Order > family.Order) at--;
                    families.Insert(at, family);
                }
                var ordered = new List<BlockKindDefinition>(_kinds.Count);
                foreach (var family in families)
                    foreach (var kind in _kinds)
                        if (kind.Family == family.Key) ordered.Add(kind);
                return ordered;
            }
        }

        // Add a family the kinds may then belong to. A second family with the same key is refused, like a kind:
        // a silent replace would let an app quietly re-order or re-label a framework family.
        public void RegisterFamily(FamilyDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (string.IsNullOrWhiteSpace(definition.Key)) throw new ArgumentException("[Blocks] cannot register a family with no key");
            if (TryGetFamily(definition.Key, out _))
                throw new ArgumentException("[Blocks] family '" + definition.Key + "' is already registered");
            _families.Add(definition);
        }

        public bool TryGetFamily(string key, out FamilyDefinition definition)
        {
            definition = null;
            if (string.IsNullOrEmpty(key)) return false;
            for (int i = 0; i < _families.Count; i++)
                if (_families[i].Key == key) { definition = _families[i]; return true; }
            return false;
        }

        // Add a kind and the factory of its view. A second kind with the same key is refused (a silent
        // replace would hide a clash between an app kind and a framework kind), and so is a kind whose
        // family nobody registered -- that used to make a new family silently, out of a typo.
        public void Register(BlockKindDefinition definition, Func<IBlockView> viewFactory)
        {
            string problem = Validate(definition);
            if (problem != null) throw new ArgumentException("[Blocks] cannot register kind '" + definition?.Key + "': " + problem);
            if (viewFactory == null) throw new ArgumentNullException(nameof(viewFactory));
            if (_views.ContainsKey(definition.Key)) throw new ArgumentException("[Blocks] kind '" + definition.Key + "' is already registered");
            if (!TryGetFamily(definition.Family, out _))
                throw new ArgumentException("[Blocks] cannot register kind '" + definition.Key + "': its family '" + definition.Family
                    + "' is not registered. Register the family first (RegisterFamily).");
            _kinds.Add(definition);
            _views[definition.Key] = viewFactory;
        }

        public bool TryGet(string kind, out BlockKindDefinition definition)
        {
            definition = null;
            if (string.IsNullOrEmpty(kind)) return false;
            for (int i = 0; i < _kinds.Count; i++)
                if (_kinds[i].Key == kind) { definition = _kinds[i]; return true; }
            return false;
        }

        // A new view of this kind, or null for a kind that is not registered
        public IBlockView CreateView(string kind) => kind != null && _views.TryGetValue(kind, out var f) ? f() : null;

        // What is wrong with a definition, or null when it can be registered
        public static string Validate(BlockKindDefinition d)
        {
            if (d == null) return "no definition";
            if (string.IsNullOrWhiteSpace(d.Key)) return "no key";
            if (string.IsNullOrWhiteSpace(d.Family)) return "no family";
            // - a SHAPE check, unlike the family's: the formats are a fixed list every registry shares, so no registry is needed
            if (string.IsNullOrWhiteSpace(d.Format)) return "no format";
            if (!Contains(CardOptions.Formats, d.Format))
                return "its format '" + d.Format + "' is not one of " + string.Join(", ", CardOptions.Formats);
            // - the needs are a fixed list too; a typo would add a chip no visitor could read
            if (d.Needs != null)
                foreach (string need in d.Needs)
                    if (!Contains(CardOptions.Needs, need))
                        return "its need '" + need + "' is not one of " + string.Join(", ", CardOptions.Needs);
            // - the icon set is one list every registry shares, so a SHAPE check too: a typo would draw an empty chip
            if (string.IsNullOrWhiteSpace(d.IconKey)) return "no icon";
            if (!Contains(CardIcons.All, d.IconKey))
                return "its icon '" + d.IconKey + "' is not one of the card's icons (CardIcons.All)";
            if (d.Variants == null || d.Variants.Count == 0) return "no variants";
            if (!d.HasVariant(d.DefaultVariant)) return "its default variant '" + d.DefaultVariant + "' is not one of its variants";
            if (d.DisplayModes == null || d.DisplayModes.Count == 0) return "no display modes";
            string fieldProblem = ValidateFields(d.Fields, allowItems: true, d.Variants);
            if (fieldProblem != null) return fieldProblem;
            if (d.ShowAfterViewedField != null && d.Field(d.ShowAfterViewedField)?.Type != BlockFieldType.Toggle)
                return "its show-after-viewed field '" + d.ShowAfterViewedField + "' is not one of its Toggle fields";
            // - a common key is the card's, even on a kind that does not carry that common field (a clip kind and 'minutes'): a kind's
            //   own field under it would read as Reading Time to every other reader. The surface keys likewise (_3.3 R17.1)
            int pages = 0;
            foreach (var own in d.OwnFields)
            {
                if (Contains(BlockKindDefinition.CommonFieldKeys, own.Key) || Contains(BlockSurfaces.FieldKeys, own.Key))
                    return "its own field '" + own.Key + "' takes a key every kind shares";
                if (own.Type == BlockFieldType.Elements) pages++;
            }
            // - ONE Page per block: a reference names a block and a row, never a field (ElementRef)
            if (pages > 1) return "it declares " + pages + " Elements fields, but a kind has one Page";
            if (d.ClipLengthField != null)
            {
                var clip = d.Field(d.ClipLengthField);
                if (clip == null || clip.Type != BlockFieldType.Asset || (clip.Media != MediaKind.Audio && clip.Media != MediaKind.Video))
                    return "its clip length field '" + d.ClipLengthField + "' is not one of its audio or video Asset fields";
            }
            return null;
        }

        // What is wrong with a field list (a kind's, an Items field's rows, an element kind's sub-fields: ElementRegistry), or null.
        // allowItems false = the fields of ONE row, where Items and Elements cannot nest
        internal static string ValidateFields(IReadOnlyList<BlockFieldDefinition> fields, bool allowItems, IReadOnlyList<string> variants)
        {
            if (fields == null) return "no field list";
            var keys = new HashSet<string>();
            foreach (var f in fields)
            {
                if (f == null || string.IsNullOrWhiteSpace(f.Key)) return "a field has no key";
                if (!keys.Add(f.Key)) return "two fields are keyed '" + f.Key + "'";
                if (f.Type == BlockFieldType.Choice && (f.Options == null || f.Options.Count == 0)) return "Choice field '" + f.Key + "' has no options";
                // - a fixed list on any other type would be silently ignored: a PoiRef lists the wall's points, a BlockRef its own card's blocks
                if (f.Type != BlockFieldType.Choice && (f.Options != null || f.OptionLabels != null))
                    return f.Type + " field '" + f.Key + "' lists options, but only a Choice field has a fixed list";
                if (f.ChoiceDefault != null && (f.Type != BlockFieldType.Choice || !Contains(f.Options, f.ChoiceDefault)))
                    return "field '" + f.Key + "' defaults to '" + f.ChoiceDefault + "', which is not one of its Choice options";
                if (f.LibraryDefault && (!allowItems || !(f.Type == BlockFieldType.Toggle || (f.Type == BlockFieldType.Choice && f.ChoiceDefault != null))))
                    return "field '" + f.Key + "' takes a Block Library default but is not a top-level Choice with a default or a top-level Toggle";
                if (f.BlankIsFine && f.Type != BlockFieldType.BlockRef) return f.Type + " field '" + f.Key + "' says a blank pick is fine, but only a BlockRef field can be left unpicked";
                if (f.PerLanguage && f.Type != BlockFieldType.Asset) return f.Type + " field '" + f.Key + "' is Per Language, but only an Asset field holds a file per language";
                if (f.Type == BlockFieldType.Choice && f.OptionLabels != null && f.OptionLabels.Count != f.Options.Count)
                    return "Choice field '" + f.Key + "' has " + f.OptionLabels.Count + " labels for " + f.Options.Count + " options";
                if (f.Type == BlockFieldType.Items)
                {
                    if (!allowItems) return "Items field '" + f.Key + "' inside an item (items cannot nest)";
                    if (f.ItemFields == null || f.ItemFields.Count == 0) return "Items field '" + f.Key + "' has no item fields";
                    string inner = ValidateFields(f.ItemFields, allowItems: false, variants);
                    if (inner != null) return inner;
                }
                if (f.Type == BlockFieldType.Elements)
                {
                    if (!allowItems) return "Elements field '" + f.Key + "' inside a row (rows cannot nest)";
                    // - a row's fields come from the element kind it names (ElementRegistry), never from the field
                    if (f.ItemFields != null) return "Elements field '" + f.Key + "' lists item fields, but each row's fields come from its element kind";
                }
            }
            foreach (var f in fields)
            {
                string shownProblem = ValidateShownWhen(f, fields, variants);
                if (shownProblem != null) return shownProblem;
            }
            return null;
        }

        // A field's ShownWhen must name what really exists in its own scope (a typo would hide the field for good): looks of the kind, a
        // Choice field of the same list with those options, a text field of the same list -- never the field itself
        private static string ValidateShownWhen(BlockFieldDefinition f, IReadOnlyList<BlockFieldDefinition> scope, IReadOnlyList<string> variants)
        {
            var when = f.ShownWhen;
            if (when == null) return null;
            string where = "field '" + f.Key + "' is shown when ";
            if (when.Kind == FieldShownWhen.Test.Looks)
            {
                if (when.Values == null || when.Values.Count == 0) return where + "no look is named";
                foreach (string look in when.Values)
                    if (!Contains(variants, look)) return where + "the look is '" + look + "', which is not one of the kind's looks";
                return null;
            }
            BlockFieldDefinition other = null;
            foreach (var s in scope) if (s.Key == when.FieldKey) other = s;
            if (other == null || other == f) return where + "field '" + when.FieldKey + "' is set, but that is not another field beside it";
            if (when.Kind == FieldShownWhen.Test.Choice)
            {
                if (other.Type != BlockFieldType.Choice) return where + "'" + when.FieldKey + "' is an option, but it is not a Choice field";
                if (when.Values == null || when.Values.Count == 0) return where + "no option is named";
                foreach (string option in when.Values)
                    if (!Contains(other.Options, option)) return where + "'" + when.FieldKey + "' is '" + option + "', which is not one of its options";
                return null;
            }
            if (other.Type != BlockFieldType.LocalizedText && other.Type != BlockFieldType.LocalizedLongText)
                return where + "'" + when.FieldKey + "' has words, but it is not a text field";
            return null;
        }

        private static bool Contains(IReadOnlyList<string> list, string value)
        {
            if (list == null) return false;
            for (int i = 0; i < list.Count; i++)
                if (list[i] == value) return true;
            return false;
        }
    }
}
