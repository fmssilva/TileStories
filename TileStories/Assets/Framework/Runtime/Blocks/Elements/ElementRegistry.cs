using System;
using System.Collections.Generic;

namespace TileStories
{
    // Every element kind a composed block's rows may name (_3.3 R14.1): the BlockRegistry pattern one level down. BuiltInElements registers
    // the framework's v1 set into Shared; an app registers its own the same way (ElementRegistry.Shared.Register) with zero Framework edits,
    // and tests build their own instance. Each element is registered with the factory of the component that draws it (R14.2), or without
    // one while its component is not built (the media elements before _3.3 S2b): a row of it then draws nothing for a visitor.
    public sealed class ElementRegistry
    {
        private readonly List<ElementKindDefinition> _elements = new();
        private readonly Dictionary<string, Func<IElementComponent>> _components = new();

        // The app's registry: the framework's elements plus whatever the app registered
        public static ElementRegistry Shared
        {
            get
            {
                if (_shared == null)
                {
                    _shared = new ElementRegistry();
                    BuiltInElements.Register(_shared);
                }
                return _shared;
            }
        }
        private static ElementRegistry _shared;

        // Every registered element, in registration order (the order the "+ Add element" menu lists a category in)
        public IReadOnlyList<ElementKindDefinition> All => _elements;

        // Add an element. A broken definition or a second one with the same key is refused: a silent replace would hide a clash between an
        // app's element and a framework one
        public void Register(ElementKindDefinition definition) => Register(definition, null);

        // Add an element with the factory of its component (null: not drawn yet)
        public void Register(ElementKindDefinition definition, Func<IElementComponent> componentFactory)
        {
            string problem = Validate(definition);
            if (problem != null) throw new ArgumentException("[Elements] cannot register element '" + definition?.Key + "': " + problem);
            if (TryGet(definition.Key, out _)) throw new ArgumentException("[Elements] element '" + definition.Key + "' is already registered");
            _elements.Add(definition);
            if (componentFactory != null) _components[definition.Key] = componentFactory;
        }

        // Whether this element has a component that draws it
        public bool HasComponent(string key) => key != null && _components.ContainsKey(key);

        // A new component of this element, or null for an element that is not registered or not drawn yet
        public IElementComponent CreateComponent(string key) => key != null && _components.TryGetValue(key, out var f) ? f() : null;

        public bool TryGet(string key, out ElementKindDefinition definition)
        {
            definition = null;
            if (string.IsNullOrEmpty(key)) return false;
            foreach (var e in _elements)
                if (e.Key == key) { definition = e; return true; }
            return false;
        }

        // What is wrong with a definition, or null when it can be registered
        public static string Validate(ElementKindDefinition d)
        {
            if (d == null) return "no definition";
            if (string.IsNullOrWhiteSpace(d.Key)) return "no key";
            if (string.IsNullOrWhiteSpace(d.DisplayName)) return "no display name";
            // - one icon set for kinds and elements: a typo would draw an empty icon
            if (string.IsNullOrWhiteSpace(d.IconKey)) return "no icon";
            if (!Contains(CardIcons.All, d.IconKey)) return "its icon '" + d.IconKey + "' is not one of the card's icons (CardIcons.All)";

            // - the row's fields: content and style share one row, so one key space, and none of the row's own keys
            var all = new List<BlockFieldDefinition>();
            if (d.Fields != null) all.AddRange(d.Fields);
            if (d.StyleFields != null) all.AddRange(d.StyleFields);
            string fieldProblem = BlockRegistry.ValidateFields(all, allowItems: false, Array.Empty<string>());
            if (fieldProblem != null) return fieldProblem;
            foreach (var f in all)
                if (Contains(ElementRow.ReservedKeys, f.Key)) return "its field '" + f.Key + "' takes a key every row keeps for itself";
            if (d.StyleFields != null)
                foreach (var s in d.StyleFields)
                    if (s.Type != BlockFieldType.Choice || s.ChoiceDefault == null)
                        return "its style option '" + s.Key + "' is not a closed list (a Choice with a default)";

            if (d.Media == MediaKind.None && (d.MinMedia != 0 || d.MaxMedia != 0 || d.SecondsPerMedia != 0f || d.SearchesCaptions))
                return "it holds no media but sets a media count, weight or caption search";
            if (d.Media != MediaKind.None && (d.MinMedia < 0 || d.MaxMedia < 1 || d.MaxMedia < d.MinMedia))
                return "its media count " + d.MinMedia + ".." + d.MaxMedia + " is not a range of at least one item";
            if (d.SecondsPerMedia < 0f) return "its seconds per media item are negative";
            if (d.PreviewRole == ElementPreviewRole.Picture && d.Media != MediaKind.Image && d.Media != MediaKind.Video)
                return "it plays the preview's picture but holds no pictures or video";

            if (d.MotionChoices == null || !d.OffersMotion(ElementOptions.MotionAuto)) return "its motion choices do not offer Auto";
            foreach (string effect in d.MotionChoices)
                if (!Contains(ElementOptions.Motions, effect)) return "its motion '" + effect + "' is not one of " + string.Join(", ", ElementOptions.Motions);
            // - Auto must name a real effect it offers, never Auto again
            if (d.AutoMotion == ElementOptions.MotionAuto || !d.OffersMotion(d.AutoMotion))
                return "its Auto motion '" + d.AutoMotion + "' is not one of its other motion choices";
            if (d.TapChoices != null)
            {
                if (d.TapChoices.Count == 0) return "it lists no tap (leave the list out when there is nothing to choose)";
                foreach (string tap in d.TapChoices)
                    if (!Contains(ElementOptions.Taps, tap)) return "its tap '" + tap + "' is not one of " + string.Join(", ", ElementOptions.Taps);
            }
            if (d.SearchFields != null)
                foreach (string key in d.SearchFields)
                {
                    var f = d.Field(key);
                    if (f == null || (f.Type != BlockFieldType.LocalizedText && f.Type != BlockFieldType.LocalizedLongText))
                        return "its search field '" + key + "' is not one of its text fields";
                }
            return null;
        }

        private static bool Contains(IReadOnlyList<string> list, string value)
        {
            if (list == null) return false;
            foreach (string s in list)
                if (s == value) return true;
            return false;
        }
    }
}
