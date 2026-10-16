using System;
using System.Collections.Generic;

namespace TileStories
{
    // What a navigation element's items choose between (rule 2.5.7): an element is not drawn where that choice has fewer than two
    // options -- one tab is not a choice -- unless it lists kinds or blocks, which is useful on a one-family card too
    public enum NavChoice
    {
        // the card's sections (families, or formats): tabs, a bar, a progress map
        Sections,
        // the wall's Groups (families merged, rule 2.5.6): the segmented element
        Groups,
        // kinds or blocks (kind chips, a contents list): rule 2.5.7 does not apply
        Blocks,
    }

    // One navigation element a wall may put in a slot of the card (_3.2 section 4.1): its key (what card_settings.navigation stores),
    // the slot it lives in, the words the POI Editor shows for it (a label and its (i) help: Editor words, ASCII), what it moves
    // between, and which of the wall's display options it reads -- so the Editor shows a row only where it changes something, for an
    // app's element too, with no edit to the window
    public sealed class NavElementDefinition
    {
        public string Key { get; }
        // One of NavSlotKeys.Built
        public string Slot { get; }
        public string Label { get; }
        public string Help { get; }
        public NavChoice Choice { get; }
        // It draws icons and reads Labels (icon / text / icon and text)
        public bool ReadsLabels { get; }
        // It reads Show Counts (a block count on its items)
        public bool ShowsCounts { get; }
        // It is a bar of a few slots and reads Bar Slots (its last slot becomes More when the sections do not fit, rule 2.5.5)
        public bool ReadsBarSlots { get; }

        public NavElementDefinition(string key, string slot, string label, string help, NavChoice choice, bool readsLabels = false,
            bool showsCounts = false, bool readsBarSlots = false)
        {
            Key = key;
            Slot = slot;
            Label = label;
            Help = help;
            Choice = choice;
            ReadsLabels = readsLabels;
            ShowsCounts = showsCounts;
            ReadsBarSlots = readsBarSlots;
        }
    }

    // The slots of the sheet a navigation element can sit in (_3.2 section 4.2). Only the BUILT ones are listed: an element for a slot
    // the sheet does not draw yet would never show, so the registry refuses it (floating, menu and edge join with their steps)
    public static class NavSlotKeys
    {
        // The strip between the header and the stack
        public const string Top = "top";
        // Under the stack, below the sticky CTA footer: the thumb zone (_3.2 section 4.2, step 7a)
        public const string Bottom = "bottom";
        public static readonly IReadOnlyList<string> Built = new[] { Top, Bottom };
    }

    // Every navigation element the card can show, with the view that draws it -- the same shape as BlockRegistry: BuiltInNavElements
    // registers the framework's into Shared, and an app registers its own for a slot the same way (NavElementRegistry.Shared.Register)
    // with zero Framework edits; the POI Editor lists a slot's elements from here. Tests build their own instance.
    public sealed class NavElementRegistry
    {
        private readonly List<NavElementDefinition> _elements = new();
        private readonly Dictionary<string, Func<INavElementView>> _views = new();

        // The app's registry: the framework's elements plus whatever the app registered
        public static NavElementRegistry Shared
        {
            get
            {
                if (_shared == null)
                {
                    _shared = new NavElementRegistry();
                    BuiltInNavElements.Register(_shared);
                }
                return _shared;
            }
        }
        private static NavElementRegistry _shared;

        // Every registered element, in registration order
        public IReadOnlyList<NavElementDefinition> All => _elements;

        // Add an element and the factory of its view. Refused: no key, a key already taken (a silent replace would hide a clash
        // between an app element and a framework one), a slot the sheet does not draw, no factory
        public void Register(NavElementDefinition definition, Func<INavElementView> viewFactory)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (string.IsNullOrWhiteSpace(definition.Key)) throw new ArgumentException("[Nav] cannot register an element with no key");
            if (definition.Key == CardOptions.NavNone)
                throw new ArgumentException("[Nav] cannot register element '" + definition.Key + "': it is the word for no element");
            if (_views.ContainsKey(definition.Key)) throw new ArgumentException("[Nav] element '" + definition.Key + "' is already registered");
            if (!Contains(NavSlotKeys.Built, definition.Slot))
                throw new ArgumentException("[Nav] cannot register element '" + definition.Key + "': its slot '" + definition.Slot
                    + "' is not one of " + string.Join(", ", NavSlotKeys.Built));
            if (viewFactory == null) throw new ArgumentNullException(nameof(viewFactory));
            _elements.Add(definition);
            _views[definition.Key] = viewFactory;
        }

        public bool TryGet(string key, out NavElementDefinition definition)
        {
            definition = null;
            if (string.IsNullOrEmpty(key)) return false;
            foreach (var e in _elements)
                if (e.Key == key) { definition = e; return true; }
            return false;
        }

        // The elements of one slot, in registration order (what the Editor's popup for that slot lists after None)
        public IReadOnlyList<NavElementDefinition> OfSlot(string slot)
        {
            var found = new List<NavElementDefinition>();
            foreach (var e in _elements)
                if (e.Slot == slot) found.Add(e);
            return found;
        }

        // A new view of this element, or null for a key that is not registered
        public INavElementView CreateView(string key) => key != null && _views.TryGetValue(key, out var f) ? f() : null;

        private static bool Contains(IReadOnlyList<string> list, string value)
        {
            foreach (string s in list)
                if (s == value) return true;
            return false;
        }
    }
}
