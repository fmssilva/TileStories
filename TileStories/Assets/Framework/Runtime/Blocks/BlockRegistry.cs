using System;
using System.Collections.Generic;

namespace TileStories
{
    // Every block kind the card can show, with the view that draws it (_3.1 section 3). The extension point:
    // BuiltInBlocks registers the framework's kinds into Shared, and an app registers its own kinds the same way
    // (BlockRegistry.Shared.Register) with zero Framework edits. Tests build their own instance.
    public sealed class BlockRegistry
    {
        private readonly List<BlockKindDefinition> _kinds = new();
        private readonly Dictionary<string, Func<IBlockView>> _views = new();

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

        // Every registered kind grouped by family, for a person choosing one (the Editor's Block Library and "+ Add block"): the families in the
        // order each first appeared, then the kinds of a family in registration order. An app's kind therefore sits with its own family,
        // not after every built-in kind.
        public IReadOnlyList<BlockKindDefinition> Ordered
        {
            get
            {
                // - a list, not a dictionary: only a list promises to keep the order the families first appeared in
                var families = new List<string>();
                foreach (var kind in _kinds)
                    if (!families.Contains(kind.Family)) families.Add(kind.Family);
                var ordered = new List<BlockKindDefinition>(_kinds.Count);
                foreach (var family in families)
                    foreach (var kind in _kinds)
                        if (kind.Family == family) ordered.Add(kind);
                return ordered;
            }
        }

        // Add a kind and the factory of its view. A second kind with the same key is refused (a silent
        // replace would hide a clash between an app kind and a framework kind).
        public void Register(BlockKindDefinition definition, Func<IBlockView> viewFactory)
        {
            string problem = Validate(definition);
            if (problem != null) throw new ArgumentException("[Blocks] cannot register kind '" + definition?.Key + "': " + problem);
            if (viewFactory == null) throw new ArgumentNullException(nameof(viewFactory));
            if (_views.ContainsKey(definition.Key)) throw new ArgumentException("[Blocks] kind '" + definition.Key + "' is already registered");
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
            if (d.Variants == null || d.Variants.Count == 0) return "no variants";
            if (!d.HasVariant(d.DefaultVariant)) return "its default variant '" + d.DefaultVariant + "' is not one of its variants";
            if (d.DisplayModes == null || d.DisplayModes.Count == 0) return "no display modes";
            string fieldProblem = ValidateFields(d.Fields, allowItems: true);
            if (fieldProblem != null) return fieldProblem;
            if (d.ShowAfterViewedField != null && d.Field(d.ShowAfterViewedField)?.Type != BlockFieldType.Toggle)
                return "its show-after-viewed field '" + d.ShowAfterViewedField + "' is not one of its Toggle fields";
            return null;
        }

        private static string ValidateFields(IReadOnlyList<BlockFieldDefinition> fields, bool allowItems)
        {
            if (fields == null) return "no field list";
            var keys = new HashSet<string>();
            foreach (var f in fields)
            {
                if (f == null || string.IsNullOrWhiteSpace(f.Key)) return "a field has no key";
                if (!keys.Add(f.Key)) return "two fields are keyed '" + f.Key + "'";
                if (f.Type == BlockFieldType.Choice && (f.Options == null || f.Options.Count == 0)) return "Choice field '" + f.Key + "' has no options";
                if (f.Type == BlockFieldType.Choice && f.OptionLabels != null && f.OptionLabels.Count != f.Options.Count)
                    return "Choice field '" + f.Key + "' has " + f.OptionLabels.Count + " labels for " + f.Options.Count + " options";
                if (f.Type == BlockFieldType.Items)
                {
                    if (!allowItems) return "Items field '" + f.Key + "' inside an item (items cannot nest)";
                    if (f.ItemFields == null || f.ItemFields.Count == 0) return "Items field '" + f.Key + "' has no item fields";
                    string inner = ValidateFields(f.ItemFields, allowItems: false);
                    if (inner != null) return inner;
                }
            }
            return null;
        }
    }
}
