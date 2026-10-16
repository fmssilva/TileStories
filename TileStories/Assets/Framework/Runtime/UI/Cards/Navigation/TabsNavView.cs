using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // A row of tabs, one per section of the card, in a strip under the header (_3.2 section 5.1): what text_tabs (A1) and icon_tabs (A2)
    // share -- they differ only in whether a tab wears its family's icon. The section being read is marked twice -- an underline AND bolder
    // words, never colour alone -- and follows the controller's Current (the scroll-spy); a tap jumps through the controller. A strip wider
    // than the screen scrolls sideways (NavStrip). With Show Counts on (when the element reads it) a tab shows its block count.
    // Views only: the sections, their order, words, icons and colours come from the CardNavModel.
    public abstract class TabsNavView : INavElementView
    {
        public VisualElement Root { get; }
        public NavStrip Strip { get; }
        public VisualElement Viewport => Strip.Viewport;
        public VisualElement Row => Strip.Row;
        public float Offset => Strip.Offset;
        public IReadOnlyList<NavTab> Tabs => _tabs.Shown;
        // The index of the marked tab (-1 = none)
        public int CurrentIndex { get; private set; } = -1;

        private readonly string _elementKey;
        private readonly bool _withIcons;
        private readonly ElementPool<NavTab> _tabs;
        private readonly NavTint _tint;
        private NavController _nav;

        protected TabsNavView(string elementKey, string name, bool withIcons)
        {
            _elementKey = elementKey;
            _withIcons = withIcons;
            Root = new VisualElement { name = name };
            Root.AddToClassList("nav-tabs");
            Strip = new NavStrip(name, "nav-tabs__viewport", "nav-tabs__row");
            Root.Add(Strip.Viewport);
            _tabs = new ElementPool<NavTab>(NewTab, tab => tab.Button.RemoveFromHierarchy());
            _tint = new NavTint(Root);
        }

        public void Bind(CardNavModel model, NavController nav, BlockBindContext context)
        {
            Unbind();
            _nav = nav;
            var strings = context?.Strings;
            var settings = context?.Settings?.navigation;
            bool counts = settings?.show_counts ?? false;
            string labels = settings?.labels;
            for (int i = 0; i < model.Sections.Count; i++)
            {
                var section = model.Sections[i];
                var tab = _tabs.Take();
                // - the visitor's word for the section (its family's, or its format's), the key only when no table names it
                string words = section.LabelKey != null && strings != null ? strings.Get(section.LabelKey) : "";
                tab.Label.text = string.IsNullOrEmpty(words) ? section.Key : words;
                // - an icon alone still names its tab (tooltip: what a screen reader and a long press read)
                tab.Button.tooltip = tab.Label.text;
                bool hasIcon = _withIcons && section.IconKey != null;
                if (tab.Icon != null)
                {
                    CardIcons.SetKey(tab.Icon, section.IconKey);
                    tab.Icon.style.display = NavLabelRule.ShowsIcon(labels, hasIcon) ? DisplayStyle.Flex : DisplayStyle.None;
                    _tint.Icon(tab.Icon, section.ColorToken);
                }
                tab.Label.style.display = !_withIcons || NavLabelRule.ShowsWords(labels, hasIcon) ? DisplayStyle.Flex : DisplayStyle.None;
                NavCount.Set(tab.Count, counts, section.Count);
                Row.Add(tab.Button);
            }
            Strip.Reset();
            nav.CurrentChanged += OnCurrentChanged;
            OnCurrentChanged(nav.Current);
        }

        public void Unbind()
        {
            if (_nav != null) _nav.CurrentChanged -= OnCurrentChanged;
            _nav = null;
            _tabs.ReleaseAll();
            _tint.Clear();
            CurrentIndex = -1;
        }

        private NavTab NewTab(int index) => NavTab.Create(Root.name + "-tab-", index, _withIcons, () => _nav?.JumpTo(index, _elementKey));

        // The spy moved (a scroll, a jump): mark that tab and bring it into view
        private void OnCurrentChanged(int index)
        {
            CurrentIndex = index;
            foreach (var tab in _tabs.Shown) tab.Button.EnableInClassList(NavTab.CurrentClass, tab.Index == index);
            if (index >= 0 && index < _tabs.Shown.Count) Strip.Show(_tabs.Shown[index].Button);
        }
    }
}
