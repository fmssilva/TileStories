using System.Globalization;
using UnityEngine.UIElements;

namespace TileStories
{
    // The family_pager navigation element (_3.2 section 6.1, step 14): the section being read, named between two chevrons -- the pages
    // layout's own header (CardLayoutRule.TopElement puts it in the top slot of every pages card) and a Top Bar any layout may pick. It shows
    // the section's family icon in its colour, its words and "2 of 8" -- in pages only "2 of 8": the page's opener is its title (_3.3 R16);
    // a chevron moves the card to the section before / after through the
    // controller (in pages: that page), and is hidden -- keeping its place -- at the first / last section, so it never offers a way to
    // nothing. It follows the controller's Current (the spy), so the name is always the section on screen. Views only: the sections, their
    // words, icons and colours come from the CardNavModel.
    public sealed class FamilyPagerNavView : INavElementView
    {
        public VisualElement Root { get; }
        public Button Previous { get; }
        public Button Next { get; }
        public VisualElement Icon { get; }
        public Label Words { get; }
        public Label Position { get; }
        // The section named now (-1 = none)
        public int CurrentIndex { get; private set; } = -1;

        // The section's name (its icon and words): hidden in the pages layout, where the page's opener is its title (_3.3 R16)
        public VisualElement NameRow { get; }

        private readonly NavTint _tint;
        private NavController _nav;
        private CardNavModel _model;
        private CardStrings _strings;

        public FamilyPagerNavView()
        {
            Root = new VisualElement { name = "nav-family-pager" };
            Root.AddToClassList("nav-pager");
            Previous = Chevron("nav-pager-previous", "nav-pager__chevron--previous", -1);
            var middle = new VisualElement { pickingMode = PickingMode.Ignore };
            middle.AddToClassList("nav-pager__middle");
            NameRow = new VisualElement { pickingMode = PickingMode.Ignore };
            NameRow.AddToClassList("nav-pager__name");
            Icon = CardIcons.Create();
            Icon.AddToClassList("nav-pager__icon");
            Icon.pickingMode = PickingMode.Ignore;
            Words = new Label { pickingMode = PickingMode.Ignore };
            Words.AddToClassList("nav-pager__words");
            NameRow.Add(Icon);
            NameRow.Add(Words);
            Position = new Label { pickingMode = PickingMode.Ignore };
            Position.AddToClassList("nav-pager__position");
            middle.Add(NameRow);
            middle.Add(Position);
            Next = Chevron("nav-pager-next", "nav-pager__chevron--next", +1);
            Root.Add(Previous);
            Root.Add(middle);
            Root.Add(Next);
            _tint = new NavTint(Root);
        }

        public void Bind(CardNavModel model, NavController nav, BlockBindContext context)
        {
            Unbind();
            _model = model;
            _nav = nav;
            _strings = context?.Strings;
            Previous.tooltip = _strings?.Get(CardStrings.Keys.NavPagerPrevious) ?? "";
            Next.tooltip = _strings?.Get(CardStrings.Keys.NavPagerNext) ?? "";
            // - pages: the opener names the page, the pager keeps its place ("2 of 8") and its chevrons
            bool names = CardLayoutRule.PagerNamesTheSection(CardLayoutRule.Effective(context?.Settings?.navigation, model));
            NameRow.style.display = names ? DisplayStyle.Flex : DisplayStyle.None;
            nav.CurrentChanged += OnCurrentChanged;
            OnCurrentChanged(nav.Current);
        }

        public void Unbind()
        {
            if (_nav != null) _nav.CurrentChanged -= OnCurrentChanged;
            _nav = null;
            _model = null;
            _strings = null;
            _tint.Clear();
            CurrentIndex = -1;
        }

        // The spy moved (a scroll, a jump, a page turned): name that section, offer only the chevrons that lead somewhere
        private void OnCurrentChanged(int index)
        {
            int count = _model?.Sections.Count ?? 0;
            CurrentIndex = index >= 0 && index < count ? index : -1;
            if (CurrentIndex < 0)
            {
                Words.text = "";
                Position.text = "";
                Previous.style.visibility = Visibility.Hidden;
                Next.style.visibility = Visibility.Hidden;
                return;
            }
            var section = _model.Sections[CurrentIndex];
            // - the visitor's word for the section (its family's, or its format's), the key only when no table names it
            string words = section.LabelKey != null && _strings != null ? _strings.Get(section.LabelKey) : "";
            Words.text = string.IsNullOrEmpty(words) ? section.Key : words;
            CardIcons.SetKey(Icon, section.IconKey);
            Icon.style.display = section.IconKey != null ? DisplayStyle.Flex : DisplayStyle.None;
            _tint.Icon(Icon, section.ColorToken);
            string format = _strings?.Get(CardStrings.Keys.NavPagerPosition) ?? "";
            Position.text = string.Format(CultureInfo.InvariantCulture, format, CurrentIndex + 1, count);
            // - hidden, not removed: each chevron keeps its side, so the name never shifts at the first or last section
            Previous.style.visibility = CurrentIndex > 0 ? Visibility.Visible : Visibility.Hidden;
            Next.style.visibility = CurrentIndex < count - 1 ? Visibility.Visible : Visibility.Hidden;
        }

        // A chevron button (a whole touch target) that moves `step` sections from the one named
        private Button Chevron(string name, string sideClass, int step)
        {
            var button = new Button(() =>
            {
                if (_nav != null && CurrentIndex >= 0) _nav.JumpTo(CurrentIndex + step, BuiltInNavElements.FamilyPager);
            }) { name = name };
            button.AddToClassList("nav-pager__chevron");
            button.AddToClassList(sideClass);
            var glyph = CardIcons.CreateVector(CardIcons.Shape.Chevron);
            glyph.pickingMode = PickingMode.Ignore;
            glyph.AddToClassList("nav-pager__glyph");
            button.Add(glyph);
            return button;
        }
    }
}
