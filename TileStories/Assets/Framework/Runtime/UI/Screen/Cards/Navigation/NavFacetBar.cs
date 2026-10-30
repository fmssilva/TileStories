using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.UIElements;

namespace TileStories
{
    // The facet chips of a card's menu (_3.2 section 5.8, step 18): one row of chips that narrows what drawer_tree already
    // lists -- reading time (Up to 2 / 5 min), what the visitor does without (No headphones / camera / network) and the formats -- and a Clear
    // chip while any is on. Nothing new is listed and nothing is drawn outside the two menus: a chip only changes which of the card's blocks
    // the menu counts and jumps to.
    // It decides nothing: NavFacetRule says which chips the card offers and which blocks pass, this view draws them and holds the visitor's
    // choice (NavFacets) for the card. Both menus own one, so both filter by the same rule. Each block's facts are read once at Bind: its
    // kind's format and needs, and its minutes (a clip's own length for audio and video, loaded through a scope this view releases).
    // A chip is marked when on by a border AND bolder words (NavChip.CurrentClass), never by colour alone; each is a full touch target.
    public sealed class NavFacetBar
    {
        // On the Clear chip (it is not one of the rule's chips)
        public const string ClearClass = "nav-facets__clear";

        public VisualElement Root { get; }
        public NavStrip Strip { get; }
        // The visitor's choice for this card: cleared on every Bind
        public NavFacets Facets { get; } = new();
        // The chips shown now, in order: the rule's offered chips, then Clear while any facet is on (its index = Offered.Count)
        public IReadOnlyList<NavChip> Chips => _chips.Shown;
        public IReadOnlyList<NavFacetRule.Chip> Offered => _offered;
        public bool HasClear => _chips.Shown.Count > _offered.Count;
        // Raised after a chip was tapped and the facets changed: the menu draws its list again
        public event Action Changed;

        private readonly ElementPool<NavChip> _chips;
        private readonly Dictionary<string, BlockFacts> _facts = new();
        private List<BlockFacts> _card = new();
        private List<NavFacetRule.Chip> _offered = new();
        private CardStrings _strings;
        private ScopedMediaSource _media;

        public NavFacetBar(string name)
        {
            Root = new VisualElement { name = name };
            Root.AddToClassList("nav-facets");
            Strip = new NavStrip(name, "nav-facets__viewport", "nav-facets__row");
            Root.Add(Strip.Viewport);
            _chips = new ElementPool<NavChip>(index => NewChip(name, index), chip => chip.Button.RemoveFromHierarchy());
            Root.style.display = DisplayStyle.None;
        }

        // Read the facts of every target of this card's model and draw the chips it offers, with every facet off
        public void Bind(CardNavModel model, NavController nav, BlockBindContext context)
        {
            Unbind();
            _strings = context?.Strings;
            _media = new ScopedMediaSource(context?.Media);
            var clips = new ClipLengthReader(_media, context?.Language, context?.FallbackLanguage);
            foreach (string block in model.Targets)
            {
                if (!nav.TryGetEntry(block, out var entry)) continue;
                var facts = NavFacetRule.FactsOf(entry.Definition, clips.MinutesOf(entry.Definition, entry.Instance));
                _facts[block] = facts;
                _card.Add(facts);
            }
            Redraw();
        }

        public void Unbind()
        {
            _media?.ReleaseAll();
            _media = null;
            _strings = null;
            _facts.Clear();
            _card = new List<BlockFacts>();
            _offered = new List<NavFacetRule.Chip>();
            Facets.Clear();
            _chips.ReleaseAll();
            Root.style.display = DisplayStyle.None;
        }

        // The facts of one block of this card (nothing known: no format, no time, no needs -- it passes only an empty facet set)
        public BlockFacts FactsOf(string block) => _facts.TryGetValue(block, out var facts) ? facts : default;

        // The blocks of `blocks` that pass the facets on now, in their order -- what a menu counts and jumps to
        public List<string> Passing(IReadOnlyList<string> blocks) => NavFacetRule.Passing(blocks, FactsOf, Facets);

        // A tap on the chip at this place (what a real tap on its button does): switch its facet, or all of them off
        public void Tap(int index)
        {
            if (index < 0 || index >= _chips.Shown.Count) return;
            if (index < _offered.Count) _offered[index].Toggle(Facets);
            else Facets.Clear();
            Redraw();
            Changed?.Invoke();
        }

        // Draw the chips the card offers now: on again after every tap, because a facet on changes what the others would still narrow
        private void Redraw()
        {
            _chips.ReleaseAll();
            _offered = NavFacetRule.Chips(_card, Facets);
            foreach (var chip in _offered) Fill(_chips.Take(), chip);
            if (Facets.IsActive) FillClear(_chips.Take());
            Root.style.display = _chips.Shown.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            foreach (var chip in _chips.Shown) Strip.Row.Add(chip.Button);
            Strip.Reset();
        }

        private void Fill(NavChip chip, NavFacetRule.Chip offered)
        {
            string words = Words(NavFacetRule.LabelKeyOf(offered), offered.Value);
            if (offered.Dimension == NavFacetRule.Dimension.Time) words = string.Format(CultureInfo.InvariantCulture, words, offered.Minutes);
            chip.Label.text = words;
            chip.Button.tooltip = words;
            chip.Button.EnableInClassList(NavChip.CurrentClass, offered.Active);
            chip.Button.EnableInClassList(ClearClass, false);
        }

        private void FillClear(NavChip chip)
        {
            chip.Label.text = Words(CardStrings.Keys.NavFacetClear, "");
            chip.Button.tooltip = chip.Label.text;
            chip.Button.EnableInClassList(NavChip.CurrentClass, false);
            chip.Button.EnableInClassList(ClearClass, true);
        }

        private string Words(string key, string fallback)
        {
            string words = key != null && _strings != null ? _strings.Get(key) : "";
            return string.IsNullOrEmpty(words) ? fallback : words;
        }

        // A chip that is only words: the family dot of a kind chip is hidden, and so is the count
        private NavChip NewChip(string name, int index)
        {
            var chip = NavChip.Create(name + "-chip-", index, () => Tap(index));
            chip.Dot.style.display = DisplayStyle.None;
            NavCount.Set(chip.Count, false, 0);
            return chip;
        }
    }
}
