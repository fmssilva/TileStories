using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // A strip of kind chips (_3.2 section 5.1): one chip per kind it is given, its words with its family's colour as a dot BESIDE them,
    // scrolling sideways when wider than the card (NavStrip). The chip of the kind being read is marked (border + bolder words) from the
    // controller's CurrentBlock through the model's KindOf; a tap jumps to the kind's first block through the controller. What kind_chips
    // shows for the whole card and icon_tabs_chips for its current section.
    public sealed class NavChipRow
    {
        public VisualElement Root { get; }
        public NavStrip Strip { get; }
        public IReadOnlyList<NavChip> Chips => _chips.Shown;
        // The marked chip (-1 = none)
        public int CurrentIndex { get; private set; } = -1;

        private readonly string _elementKey;
        private readonly ElementPool<NavChip> _chips;
        private readonly NavTint _tint;
        private NavController _nav;
        private CardNavModel _model;

        public NavChipRow(string elementKey, string name)
        {
            _elementKey = elementKey;
            Root = new VisualElement { name = name };
            Root.AddToClassList("nav-chips");
            Strip = new NavStrip(name, "nav-chips__viewport", "nav-chips__row");
            Root.Add(Strip.Viewport);
            _chips = new ElementPool<NavChip>(NewChip, chip => chip.Button.RemoveFromHierarchy());
            _tint = new NavTint(Root);
        }

        // Show these kinds of this card's model, follow its controller
        public void Bind(IReadOnlyList<CardNavModel.Kind> kinds, CardNavModel model, NavController nav, BlockBindContext context)
        {
            Unbind();
            _nav = nav;
            _model = model;
            var strings = context?.Strings;
            bool counts = context?.Settings?.navigation?.show_counts ?? false;
            foreach (var kind in kinds)
            {
                var chip = _chips.Take();
                chip.Kind = kind;
                string words = kind.LabelKey != null && strings != null ? strings.Get(kind.LabelKey) : "";
                chip.Label.text = string.IsNullOrEmpty(words) ? kind.Key : words;
                NavCount.Set(chip.Count, counts, kind.Count);
                _tint.Fill(chip.Dot, kind.ColorToken);
                Strip.Row.Add(chip.Button);
            }
            Strip.Reset();
            nav.CurrentBlockChanged += OnCurrentBlockChanged;
            OnCurrentBlockChanged(nav.CurrentBlock);
        }

        public void Unbind()
        {
            if (_nav != null) _nav.CurrentBlockChanged -= OnCurrentBlockChanged;
            _nav = null;
            _model = null;
            _chips.ReleaseAll();
            _tint.Clear();
            CurrentIndex = -1;
        }

        private NavChip NewChip(int index) => NavChip.Create(Root.name + "-chip-", index, () =>
        {
            var kind = index < _chips.Shown.Count ? _chips.Shown[index].Kind : null;
            if (kind != null) _nav?.JumpToBlock(kind.Targets[0], _elementKey);
        });

        // The block being read moved: mark the chip of its kind (none when that kind is not in this row) and bring it into view
        private void OnCurrentBlockChanged(string block)
        {
            var kind = _model?.KindOf(block);
            CurrentIndex = -1;
            foreach (var chip in _chips.Shown)
            {
                bool marked = kind != null && chip.Kind == kind;
                chip.Button.EnableInClassList(NavChip.CurrentClass, marked);
                if (marked) CurrentIndex = chip.Index;
            }
            if (CurrentIndex >= 0) Strip.Show(_chips.Shown[CurrentIndex].Button);
        }
    }
}
