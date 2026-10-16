using UnityEngine.UIElements;

namespace TileStories
{
    // The icon_tabs_chips navigation element (_3.2 section 5.1, canvas A2, L1 + L2): the icon tabs, and under them a row of chips for
    // the kinds of the CURRENT section that this card holds -- two taps to any block. The row follows the tab the spy marks and has chips
    // only where that section has two kinds or more (one chip would repeat its tab, NavOntologyRule.OffersAKindChoice). Its place is kept
    // whenever ANY section of the card has such a choice -- hidden, not removed, under a one-kind section -- so the strip keeps one height
    // while the visitor reads and the content under it never jumps.
    public sealed class IconTabsChipsNavView : INavElementView
    {
        public VisualElement Root { get; }
        public IconTabsNavView Tabs { get; } = new(BuiltInNavElements.IconTabsChips, "nav-icon-tabs-chips-tabs");
        public NavChipRow Chips { get; } = new(BuiltInNavElements.IconTabsChips, "nav-icon-tabs-chips-row");
        // The section whose kinds the chip row shows now (-1 = none: the row is hidden)
        public int ChipsSection { get; private set; } = -1;

        private CardNavModel _model;
        private NavController _nav;
        private BlockBindContext _context;

        public IconTabsChipsNavView()
        {
            Root = new VisualElement { name = "nav-icon-tabs-chips" };
            Root.AddToClassList("nav-tabs-chips");
            Root.Add(Tabs.Root);
            Root.Add(Chips.Root);
        }

        public void Bind(CardNavModel model, NavController nav, BlockBindContext context)
        {
            Unbind();
            _model = model;
            _nav = nav;
            _context = context;
            Tabs.Bind(model, nav, context);
            bool anyChoice = false;
            foreach (var section in model.Sections) anyChoice |= NavOntologyRule.OffersAKindChoice(section.Kinds.Count);
            Chips.Root.style.display = anyChoice ? DisplayStyle.Flex : DisplayStyle.None;
            nav.CurrentChanged += ShowChipsOf;
            ShowChipsOf(nav.Current);
        }

        public void Unbind()
        {
            if (_nav != null) _nav.CurrentChanged -= ShowChipsOf;
            Tabs.Unbind();
            Chips.Unbind();
            _model = null;
            _nav = null;
            _context = null;
            ChipsSection = -1;
        }

        // The spy marked another section: its kinds in the chip row, or an empty (hidden) row
        private void ShowChipsOf(int section)
        {
            if (_model == null) return;
            bool shown = section >= 0 && section < _model.Sections.Count && NavOntologyRule.OffersAKindChoice(_model.Sections[section].Kinds.Count);
            if (shown && section == ChipsSection) return;
            ChipsSection = shown ? section : -1;
            if (shown) Chips.Bind(_model.Sections[section].Kinds, _model, _nav, _context);
            else Chips.Unbind();
            Chips.Root.style.visibility = shown ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
