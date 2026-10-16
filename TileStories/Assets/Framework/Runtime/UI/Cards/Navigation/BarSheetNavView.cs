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
    // list closes on a pick, on the arrow's own slot again, when More opens, when the spy leaves its family (it would name a family nobody
    // is reading) and on every rebind. Back and a scroll away from it are the menu overlay's (step 8), like More's list.
    public sealed class BarSheetNavView : BottomBarNavView
    {
        // On the root while the kinds list is open
        public const string KindsOpenClass = "nav-bar--kinds-open";

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
            SetKindsOpen(false, -1);
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
            SetKindsOpen(false, -1);
            _kindRows.ReleaseAll();
            foreach (var (_, arrow) in _arrows) arrow.RemoveFromHierarchy();
            _arrows.Clear();
            _model = null;
            _strings = null;
            base.Unbind();
        }

        // More opening closes the kinds: one list above the bar at a time
        public override void OpenMore()
        {
            SetKindsOpen(false, -1);
            base.OpenMore();
        }

        // The first tap on a family jumps; a second tap on the current one (with a choice of kinds) opens or closes its kinds
        public override void Tap(int section)
        {
            if (_model != null && section >= 0 && section < _model.Sections.Count
                && BarSheetRule.OpensKinds(section, CurrentIndex, _model.Sections[section].Kinds.Count))
            {
                if (KindsOpen && KindsSection == section) SetKindsOpen(false, -1);
                else OpenKinds(section);
                return;
            }
            SetKindsOpen(false, -1);
            JumpTo(section);
        }

        // The spy moved: the arrow follows the current slot, and a list open for a family nobody reads any more closes
        protected override void OnCurrentChanged(int index)
        {
            base.OnCurrentChanged(index);
            if (KindsOpen && index != KindsSection) SetKindsOpen(false, -1);
            ShowArrows(index);
        }

        // List this section's kinds above the bar, each with its block count
        private void OpenKinds(int section)
        {
            CloseMore();
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
            SetKindsOpen(true, section);
        }

        // A tap on a kind's row (its place in the open list): jump to the kind's first block and close the list
        public void PickKind(int index)
        {
            if (!KindsOpen || _model == null) return;
            var kinds = _model.Sections[KindsSection].Kinds;
            if (index < 0 || index >= kinds.Count) return;
            string block = kinds[index].Targets[0];
            SetKindsOpen(false, -1);
            Nav?.JumpToBlock(block, ElementKey);
        }

        private void SetKindsOpen(bool open, int section)
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
