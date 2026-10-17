using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The bar_sheet navigation element (_3.2 section 5.2, canvas B3, L1 -> L2): the bottom bar whose slots open one level deeper on a
    // SECOND tap. The first tap on a family jumps to it, like every bar; a second tap on the family that is already current (BarSheetRule)
    // opens a small list of that family's KINDS above the bar, each with its block count, and a tap on a kind jumps to the kind's first
    // block and closes the list. A third tap on the family closes it again. No long-press: it cannot be found and not every finger can
    // hold still (WCAG 2.5.7).
    // Progressive disclosure (30-ui-content): the bar shows only families; the kinds appear when the visitor already is in a family and
    // asks for more. So that a visitor can tell the two taps apart, the current slot wears a small up arrow while a tap on it would open a
    // list (a family with two kinds or more), and the list reuses More's list look: it is plainly "a list above the bar".
    // A thin subclass of the bar (slots, More, mark and jumps are the bar's own); what it adds is the tap decision and the kinds list. The
    // list is a client of the card's overlay owner (CardMenuOverlay, step 8), like More's: it closes on Back, on a jump (a pick in it
    // included), on a scroll, when More or a menu opens and on every rebind -- the owner's one rule. Its own dismiss path is only the
    // opener's toggle: the arrow's slot tapped again. (Until step 8 it also closed when the spy left its family; a spy moves only by a scroll
    // or a jump, which close it now, so that second rule went.)
    public sealed class BarSheetNavView : BottomBarNavView
    {
        // On the root while the kinds list is open
        public const string KindsOpenClass = "nav-bar--kinds-open";
        // What the overlay owner knows the kinds list as while it is open
        public const string KindsListKey = "kinds_list";

        // The kinds list, above the slots (and More's list)
        public VisualElement KindsList { get; }
        // One row per kind of the family the list is open for, in the model's order (Index = the row's place)
        public IReadOnlyList<NavTab> KindRows => _kindRows.Shown;
        public bool KindsOpen { get; private set; }
        // The section the list is open for (-1 = closed)
        public int KindsSection { get; private set; } = -1;
        // The slots wearing the "opens a list" arrow now, one per bar slot (shown only on the current one when it has a kinds choice)
        public IReadOnlyList<(NavTab Slot, VisualElement Arrow)> Arrows => _arrows;

        private readonly ElementPool<NavTab> _kindRows;
        private readonly List<(NavTab Slot, VisualElement Arrow)> _arrows = new();
        private CardNavModel _model;
        private CardStrings _strings;

        public BarSheetNavView() : base(BuiltInNavElements.BarSheet)
        {
            KindsList = new VisualElement { name = "nav-bar-sheet-kinds" };
            KindsList.AddToClassList("nav-bar__more-list");
            // - between More's list and the slots: both lists sit right above the bar, and only one is ever open
            Root.Insert(Root.IndexOf(Bar), KindsList);
            _kindRows = new ElementPool<NavTab>(index => KindRow(NavTab.Create("nav-bar-sheet-kind-", index, false, () => PickKind(index))),
                tab => tab.Button.RemoveFromHierarchy());
            ShowKinds(false, -1);
        }

        public override void Bind(CardNavModel model, NavController nav, BlockBindContext context)
        {
            base.Bind(model, nav, context);
            _model = model;
            _strings = context?.Strings;
            foreach (var slot in Items)
            {
                var arrow = CardIcons.Create();
                CardIcons.SetKey(arrow, CardIcons.OpenBlock);
                arrow.AddToClassList("nav-bar__arrow");
                slot.Button.Add(arrow);
                _arrows.Add((slot, arrow));
            }
            ShowArrows(CurrentIndex);
        }

        public override void Unbind()
        {
            CloseKinds();
            _kindRows.ReleaseAll();
            foreach (var (_, arrow) in _arrows) arrow.RemoveFromHierarchy();
            _arrows.Clear();
            _model = null;
            _strings = null;
            base.Unbind();
        }

        // More opening closes the kinds: one list over the card at a time (the overlay owner does it when there is one)
        public override void OpenMore()
        {
            CloseKinds();
            base.OpenMore();
        }

        // The first tap on a family jumps; a second tap on the current one (with a choice of kinds) opens or closes its kinds
        public override void Tap(int section)
        {
            if (_model != null && section >= 0 && section < _model.Sections.Count
                && BarSheetRule.OpensKinds(section, CurrentIndex, _model.Sections[section].Kinds.Count))
            {
                if (KindsOpen && KindsSection == section) CloseKinds();
                else OpenKinds(section);
                return;
            }
            CloseKinds();
            JumpTo(section);
        }

        // The spy moved: the arrow follows the current slot
        protected override void OnCurrentChanged(int index)
        {
            base.OnCurrentChanged(index);
            ShowArrows(index);
        }

        // List this section's kinds above the bar, each with its block count, as the one thing open over the card
        private void OpenKinds(int section)
        {
            // - whatever is open closes BEFORE the new list shows: the owner runs the old list's close as it opens the new one
            CloseMore();
            CloseKinds();
            _kindRows.ReleaseAll();
            foreach (var kind in _model.Sections[section].Kinds)
            {
                var row = _kindRows.Take();
                string words = kind.LabelKey != null && _strings != null ? _strings.Get(kind.LabelKey) : "";
                row.Label.text = string.IsNullOrEmpty(words) ? kind.Key : words;
                row.Button.tooltip = row.Label.text;
                NavCount.Set(row.Count, true, kind.Count);
                KindsList.Add(row.Button);
            }
            ShowKinds(true, section);
            Overlay?.Open(KindsListKey, null, () => ShowKinds(false, -1));
        }

        // A tap on a kind's row (its place in the open list): jump to the kind's first block and close the list
        public void PickKind(int index)
        {
            if (!KindsOpen || _model == null) return;
            var kinds = _model.Sections[KindsSection].Kinds;
            if (index < 0 || index >= kinds.Count) return;
            string block = kinds[index].Targets[0];
            CloseKinds();
            Nav?.JumpToBlock(block, ElementKey);
        }

        // Close the kinds list, through the overlay owner when it is the one open there
        private void CloseKinds()
        {
            if (Overlay != null && Overlay.OpenKey == KindsListKey) Overlay.Close();
            else ShowKinds(false, -1);
        }

        private void ShowKinds(bool open, int section)
        {
            KindsOpen = open;
            KindsSection = open ? section : -1;
            KindsList.style.display = open ? DisplayStyle.Flex : DisplayStyle.None;
            Root.EnableInClassList(KindsOpenClass, open);
        }

        // The arrow shows on the current slot only, and only while a tap on it would open kinds
        private void ShowArrows(int current)
        {
            foreach (var (slot, arrow) in _arrows)
            {
                bool offers = _model != null && slot.Index == current && slot.Index < _model.Sections.Count
                    && BarSheetRule.OpensKinds(slot.Index, current, _model.Sections[slot.Index].Kinds.Count);
                arrow.style.display = offers ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private static NavTab KindRow(NavTab tab)
        {
            tab.Button.AddToClassList("nav-bar__more-row");
            return tab;
        }
    }
}
