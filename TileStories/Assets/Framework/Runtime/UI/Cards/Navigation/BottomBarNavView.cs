using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The bottom_bar navigation element (_3.2 section 5.2, canvas B1, L1): a bar in the thumb zone with up to Bar Slots sections, each an
    // icon tab (its family's icon in its colour, its word; Labels picks icon, word or both) sharing the width equally. When the card has
    // more sections than slots the last slot is More (NavOntologyRule.SectionsInBar, read through the model's BarSections /
    // MoreSections): a tap opens a small list of the sections that did not fit, just above the bar -- inside the bottom area, so it slides
    // away with it. The section being read is marked twice (underline AND bolder words); one in the overflow marks More, and its row in the
    // list. A tap on an item or a row jumps through the controller; the list closes on a pick, on More again and on every rebind.
    // Views only: built from NavTab, NavTint and the model, no shared part of its own.
    //
    // Step 7b: the dock and bar_sheet are THIN SUBCLASSES of this bar (the way icon_tabs is of TabsNavView) -- the same slots, More and mark,
    // one thing each changes: the dock only LOOKS different (its stylesheet widens the marked item and hides the others' words), bar_sheet
    // decides a tap differently. The protected members below are exactly those seams.
    public class BottomBarNavView : INavElementView
    {
        // On the root while the More list is open
        public const string MoreOpenClass = "nav-bar--more-open";

        public VisualElement Root { get; }
        // The row of slots, and the More list above it
        public VisualElement Bar { get; }
        public VisualElement MoreList { get; }
        // One tab per section the bar shows by itself (Index = the section's index in the model)
        public IReadOnlyList<NavTab> Items => _items.Shown;
        // The last slot when sections overflow (hidden otherwise)
        public NavTab More { get; }
        // One row per overflow section, in the model's order (Index = the row's place; its section is Items.Count + Index)
        public IReadOnlyList<NavTab> MoreRows => _rows.Shown;
        public bool HasMore { get; private set; }
        public bool MoreOpen { get; private set; }
        // The marked section (-1 = none) -- a section of the overflow marks More
        public int CurrentIndex { get; private set; } = -1;

        // The key the element is registered under: what its nav_jump events name (BuiltInNavElements)
        protected string ElementKey { get; }
        // The controller of the card bound now (null while unbound)
        protected NavController Nav => _nav;

        private readonly ElementPool<NavTab> _items;
        private readonly ElementPool<NavTab> _rows;
        private readonly NavTint _tint;
        private NavController _nav;

        public BottomBarNavView() : this(BuiltInNavElements.BottomBar)
        {
        }

        protected BottomBarNavView(string elementKey)
        {
            ElementKey = elementKey;
            Root = new VisualElement { name = "nav-" + elementKey.Replace('_', '-') };
            Root.AddToClassList("nav-bar");
            MoreList = new VisualElement { name = "nav-bottom-bar-more-list" };
            MoreList.AddToClassList("nav-bar__more-list");
            Bar = new VisualElement { name = "nav-bottom-bar-items" };
            Bar.AddToClassList("nav-bar__items");
            Root.Add(MoreList);
            Root.Add(Bar);
            _items = new ElementPool<NavTab>(index => Slot(NavTab.Create("nav-bottom-bar-item-", index, true, () => Tap(index))),
                tab => tab.Button.RemoveFromHierarchy());
            _rows = new ElementPool<NavTab>(index => Row(NavTab.Create("nav-bottom-bar-more-row-", index, true, () => Tap(_items.Shown.Count + index))),
                tab => tab.Button.RemoveFromHierarchy());
            More = Slot(NavTab.Create("nav-bottom-bar-more", 0, true, OpenMore));
            More.Button.name = "nav-bottom-bar-more";
            CardIcons.SetKey(More.Icon, CardIcons.More);
            _tint = new NavTint(Root);
            SetMoreOpen(false);
        }

        // Whether the Labels setting decides what a slot shows (the bar). False: the element's own stylesheet decides which words show
        // and when (the dock widens the marked item to icon + words), so the view leaves display alone
        protected virtual bool LabelsFollowTheSetting => true;

        public virtual void Bind(CardNavModel model, NavController nav, BlockBindContext context)
        {
            Unbind();
            _nav = nav;
            var strings = context?.Strings;
            string labels = context?.Settings?.navigation?.labels;
            foreach (var section in model.BarSections)
            {
                var tab = _items.Take();
                Fill(tab, section, strings, labels, alwaysWords: false);
                Bar.Add(tab.Button);
            }
            HasMore = model.MoreSections.Count > 0;
            if (HasMore)
            {
                string words = strings?.Get(CardStrings.Keys.NavMore) ?? "";
                More.Label.text = words;
                More.Button.tooltip = words;
                ShowLabels(More, hasIcon: true, labels, alwaysWords: false);
                Bar.Add(More.Button);
                // - a list is read as a list: every row says its words, its icon beside them
                foreach (var section in model.MoreSections)
                {
                    var row = _rows.Take();
                    Fill(row, section, strings, labels, alwaysWords: true);
                    MoreList.Add(row.Button);
                }
            }
            nav.CurrentChanged += OnCurrentChanged;
            OnCurrentChanged(nav.Current);
        }

        public virtual void Unbind()
        {
            if (_nav != null) _nav.CurrentChanged -= OnCurrentChanged;
            _nav = null;
            _items.ReleaseAll();
            _rows.ReleaseAll();
            More.Button.RemoveFromHierarchy();
            _tint.Clear();
            HasMore = false;
            CurrentIndex = -1;
            SetMoreOpen(false);
        }

        // More was tapped: show the rest. The seam of step 8 -- a wall's Menu (when it sets one) opens here instead of the small list
        public virtual void OpenMore() => SetMoreOpen(HasMore && !MoreOpen);

        // A tap on a slot or on a row of More's list (`section` = its index in the model): the bar jumps. bar_sheet decides differently
        public virtual void Tap(int section) => JumpTo(section);

        // Jump to a section as this element, closing More's list
        protected void JumpTo(int section)
        {
            SetMoreOpen(false);
            _nav?.JumpTo(section, ElementKey);
        }

        protected void CloseMore() => SetMoreOpen(false);

        private void SetMoreOpen(bool open)
        {
            MoreOpen = open;
            MoreList.style.display = open ? DisplayStyle.Flex : DisplayStyle.None;
            Root.EnableInClassList(MoreOpenClass, open);
        }

        // A section's words and icon (in its family's colour) on a tab. `alwaysWords`: a list row keeps its words whatever Labels says
        private void Fill(NavTab tab, CardNavModel.Section section, CardStrings strings, string labels, bool alwaysWords)
        {
            string words = section.LabelKey != null && strings != null ? strings.Get(section.LabelKey) : "";
            tab.Label.text = string.IsNullOrEmpty(words) ? section.Key : words;
            // - an icon alone still names its slot (tooltip: what a screen reader and a long press read)
            tab.Button.tooltip = tab.Label.text;
            bool hasIcon = section.IconKey != null;
            CardIcons.SetKey(tab.Icon, section.IconKey);
            ShowLabels(tab, hasIcon, labels, alwaysWords);
            _tint.Icon(tab.Icon, section.ColorToken);
        }

        // Which of a tab's icon and words show. By the Labels setting (the bar); else left to the stylesheet, except a list row, which is
        // read as a list and always says its words
        private void ShowLabels(NavTab tab, bool hasIcon, string labels, bool alwaysWords)
        {
            if (LabelsFollowTheSetting)
            {
                tab.Icon.style.display = NavLabelRule.ShowsIcon(labels, hasIcon) ? DisplayStyle.Flex : DisplayStyle.None;
                tab.Label.style.display = alwaysWords || NavLabelRule.ShowsWords(labels, hasIcon) ? DisplayStyle.Flex : DisplayStyle.None;
                return;
            }
            if (hasIcon) tab.Icon.style.display = StyleKeyword.Null;
            else tab.Icon.style.display = DisplayStyle.None;
            if (alwaysWords) tab.Label.style.display = DisplayStyle.Flex;
            else tab.Label.style.display = StyleKeyword.Null;
        }

        // The spy moved: mark the bar's slot of that section -- More for one in the overflow -- and its row in the list
        protected virtual void OnCurrentChanged(int index)
        {
            CurrentIndex = index;
            foreach (var tab in _items.Shown) tab.Button.EnableInClassList(NavTab.CurrentClass, tab.Index == index);
            foreach (var row in _rows.Shown) row.Button.EnableInClassList(NavTab.CurrentClass, _items.Shown.Count + row.Index == index);
            More.Button.EnableInClassList(NavTab.CurrentClass, MoreMarks(index));
        }

        // Does the section being read sit in the overflow (so More stands for it)?
        private bool MoreMarks(int index) => HasMore && index >= _items.Shown.Count;

        private static NavTab Slot(NavTab tab)
        {
            tab.Button.AddToClassList("nav-bar__item");
            return tab;
        }

        private static NavTab Row(NavTab tab)
        {
            tab.Button.AddToClassList("nav-bar__more-row");
            return tab;
        }
    }
}
