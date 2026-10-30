using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The bar bar_sheet is built on (_3.2 section 5.2, canvas B1, L1): a bar in the thumb zone with up to Bar Slots sections, each an
    // icon tab (its family's icon in its colour, its word; Labels picks icon, word or both) sharing the width equally. When the card has
    // more sections than slots the last slot is More (NavOntologyRule.SectionsInBar, read through the model's BarSections /
    // MoreSections): a tap opens a small list of the sections that did not fit, just above the bar -- inside the bottom area, so it slides
    // away with it -- or, when the wall set a Menu, that menu over the card (step 8). The section being read is marked twice (underline AND
    // bolder words); one in the overflow marks More, which then SHOWS that section -- its icon and word, with the small "opens a list" arrow
    // -- because the bar's job is to say where the visitor is and "More" does not (step 8), and its row in the list.
    // The list is a client of the card's overlay owner (CardMenuOverlay): it closes on Back, a jump, a scroll, another menu or list opening
    // and every rebind there, one rule for everything open over the card; the bar keeps only its own toggle (More again).
    // Views only: built from NavTab, NavTint and the model, no shared part of its own.
    //
    // bar_sheet is a THIN SUBCLASS of this bar -- the same slots, More and mark; it decides a tap differently. The protected members below
    // are exactly that seam.
    public abstract class BottomBarNavView : INavElementView
    {
        // On the root while the More list is open
        public const string MoreOpenClass = "nav-bar--more-open";
        // What the overlay owner knows the More list as while it is open
        public const string MoreListKey = "more_list";

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
        // The small "opens a list" arrow on More, shown while More stands for a section of its list
        public VisualElement MoreArrow { get; }

        // The key the element is registered under: what its nav_jump events name (BuiltInNavElements)
        protected string ElementKey { get; }
        // The controller of the card bound now (null while unbound)
        protected NavController Nav => _nav;
        // The card's overlay owner (null where the caller has none: the list then opens and closes by itself)
        protected CardMenuOverlay Overlay => _overlay;

        private readonly ElementPool<NavTab> _items;
        private readonly ElementPool<NavTab> _rows;
        private readonly NavTint _tint;
        private NavController _nav;
        private CardMenuOverlay _overlay;
        private CardNavModel _model;
        private CardStrings _strings;
        private string _labels;

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
            MoreArrow = CardIcons.Create();
            CardIcons.SetKey(MoreArrow, CardIcons.OpenBlock);
            MoreArrow.AddToClassList("nav-bar__arrow");
            More.Button.Add(MoreArrow);
            _tint = new NavTint(Root);
            ShowMore(false);
        }

        public virtual void Bind(CardNavModel model, NavController nav, BlockBindContext context)
        {
            Unbind();
            _nav = nav;
            _overlay = context?.Overlay;
            _model = model;
            _strings = context?.Strings;
            _labels = context?.Settings?.navigation?.labels;
            foreach (var section in model.BarSections)
            {
                var tab = _items.Take();
                Fill(tab, section, _strings, _labels, alwaysWords: false);
                Bar.Add(tab.Button);
            }
            HasMore = model.MoreSections.Count > 0;
            if (HasMore)
            {
                Bar.Add(More.Button);
                // - a list is read as a list: every row says its words, its icon beside them
                foreach (var section in model.MoreSections)
                {
                    var row = _rows.Take();
                    Fill(row, section, _strings, _labels, alwaysWords: true);
                    MoreList.Add(row.Button);
                }
            }
            nav.CurrentChanged += OnCurrentChanged;
            OnCurrentChanged(nav.Current);
        }

        public virtual void Unbind()
        {
            if (_nav != null) _nav.CurrentChanged -= OnCurrentChanged;
            _overlay?.Close(MoreListKey);
            _nav = null;
            _overlay = null;
            _model = null;
            _strings = null;
            _items.ReleaseAll();
            _rows.ReleaseAll();
            More.Button.RemoveFromHierarchy();
            _tint.Clear();
            HasMore = false;
            CurrentIndex = -1;
            ShowMore(false);
        }

        // More was tapped: the wall's Menu over the card when it set one (step 8), else the small list of the rest -- opened through the
        // overlay owner, or closed again (the slot's own toggle)
        public virtual void OpenMore()
        {
            if (!HasMore) return;
            if (_overlay != null && _overlay.HasMenu)
            {
                _overlay.OpenMenu();
                return;
            }
            if (MoreOpen)
            {
                CloseMore();
                return;
            }
            ShowMore(true);
            _overlay?.Open(MoreListKey, null, () => ShowMore(false));
        }

        // A tap on a slot or on a row of More's list (`section` = its index in the model): the bar jumps. bar_sheet decides differently
        public virtual void Tap(int section) => JumpTo(section);

        // Jump to a section as this element (the jump closes More's list: the overlay owner hears every jump)
        protected void JumpTo(int section)
        {
            CloseMore();
            _nav?.JumpTo(section, ElementKey);
        }

        // Close More's list, through the overlay owner when it is the one open there
        protected void CloseMore()
        {
            if (_overlay != null && _overlay.OpenKey == MoreListKey) _overlay.Close();
            else ShowMore(false);
        }

        private void ShowMore(bool open)
        {
            MoreOpen = open;
            MoreList.style.display = open ? DisplayStyle.Flex : DisplayStyle.None;
            Root.EnableInClassList(MoreOpenClass, open);
        }

        // A section's words and icon (in its family's colour) on a tab. `alwaysWords`: a list row keeps its words whatever Labels says
        private void Fill(NavTab tab, CardNavModel.Section section, CardStrings strings, string labels, bool alwaysWords)
        {
            tab.Label.text = SectionWords(section, strings);
            // - an icon alone still names its slot (tooltip: what a screen reader and a long press read)
            tab.Button.tooltip = tab.Label.text;
            bool hasIcon = section.IconKey != null;
            CardIcons.SetKey(tab.Icon, section.IconKey);
            ShowLabels(tab, hasIcon, labels, alwaysWords);
            _tint.Icon(tab.Icon, section.ColorToken);
        }

        // Which of a tab's icon and words show: by the Labels setting, except a list row, which is read as a list and always says its words
        private void ShowLabels(NavTab tab, bool hasIcon, string labels, bool alwaysWords)
        {
            tab.Icon.style.display = NavLabelRule.ShowsIcon(labels, hasIcon) ? DisplayStyle.Flex : DisplayStyle.None;
            tab.Label.style.display = alwaysWords || NavLabelRule.ShowsWords(labels, hasIcon) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // The spy moved: mark the bar's slot of that section -- More for one in the overflow -- and its row in the list
        protected virtual void OnCurrentChanged(int index)
        {
            CurrentIndex = index;
            foreach (var tab in _items.Shown) tab.Button.EnableInClassList(NavTab.CurrentClass, tab.Index == index);
            foreach (var row in _rows.Shown) row.Button.EnableInClassList(NavTab.CurrentClass, _items.Shown.Count + row.Index == index);
            More.Button.EnableInClassList(NavTab.CurrentClass, MoreMarks(index));
            ShowMoreSlot(index);
        }

        // What the More slot says: "More" and its dots -- or, while the section being read is one of its list, THAT section's icon (in its
        // colour) and word with the "opens a list" arrow. Still one slot, and a tap on it still opens the list (or the Menu)
        private void ShowMoreSlot(int index)
        {
            if (!HasMore || _model == null) return;
            bool standsFor = MoreMarks(index) && index < _model.Sections.Count;
            var section = standsFor ? _model.Sections[index] : null;
            string words = section != null ? SectionWords(section, _strings) : _strings?.Get(CardStrings.Keys.NavMore) ?? "";
            More.Label.text = words;
            More.Button.tooltip = words;
            CardIcons.SetKey(More.Icon, section != null ? section.IconKey : CardIcons.More);
            _tint.Icon(More.Icon, section?.ColorToken);
            ShowLabels(More, hasIcon: section == null || section.IconKey != null, _labels, alwaysWords: false);
            MoreArrow.style.display = section != null ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // A section's visitor words (its key when no words are found)
        private static string SectionWords(CardNavModel.Section section, CardStrings strings)
        {
            string words = section.LabelKey != null && strings != null ? strings.Get(section.LabelKey) : "";
            return string.IsNullOrEmpty(words) ? section.Key : words;
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
