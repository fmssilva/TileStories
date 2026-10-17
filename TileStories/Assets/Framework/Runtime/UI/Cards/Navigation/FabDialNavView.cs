using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The fab_dial navigation element (_3.2 section 5.4, canvas C1, L1): ONE round button in the card's floating slot -- zero chrome until it
    // is needed -- that fans out the card's sections as labelled shortcuts: a column of pills, each the family's icon in its colour beside its
    // words (never colour alone), the section being read marked (NavTab.CurrentClass: a bar at the row's start and bolder words). A pill moves
    // the card to its section (NavController.JumpTo).
    //   - which way the column opens is FabFanRule's: up over the camera view when it fits (peek, half), else down over the card (at Full the
    //     button rests at the screen's top), else towards the bigger room, where it scrolls
    //   - the open column is an anchored client of the card's overlay owner (CardMenuOverlay): it closes on Back, a jump (its own pick
    //     included), a touch or a wheel on the card, the sheet moving to another stop, another list or a menu opening, every rebind -- the
    //     button keeps only its own toggle (a tap on the X it shows while open)
    // Views only: built from NavFab, NavTab, NavTint and the model.
    public sealed class FabDialNavView : INavElementView
    {
        // The overlay key of the open column: the element's own key, so the floating slot dims the card behind it (FloatingSlotRule.Dims)
        public const string ListKey = BuiltInNavElements.FabDial;
        public const string UpClass = "nav-dial__list--up";
        public const string DownClass = "nav-dial__list--down";

        public VisualElement Root { get; }
        public NavFab Fab { get; }
        public ScrollView List { get; }
        // One pill per section of the model, in its order (Index = the section's index)
        public IReadOnlyList<NavTab> Items => _items.Shown;
        public bool IsOpen { get; private set; }
        public FabFanRule.Direction Direction { get; private set; } = FabFanRule.Direction.Up;

        private readonly ElementPool<NavTab> _items;
        private readonly NavTint _tint;
        private CardNavModel _model;
        private NavController _nav;
        private CardMenuOverlay _overlay;

        public FabDialNavView()
        {
            Fab = new NavFab("nav-dial", Toggle);
            Root = Fab.Root;
            Root.AddToClassList("nav-dial");
            // - a finger scrolls a column taller than the room: no desktop scroller, like every scroll of the card
            List = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "nav-dial-list",
                verticalScrollerVisibility = ScrollerVisibility.Hidden,
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
            };
            List.AddToClassList("nav-dial__list");
            List.AddToClassList(UpClass);
            List.style.display = DisplayStyle.None;
            // - which way it opens needs its real height: decided once it is laid out, and again whenever that changes
            List.RegisterCallback<GeometryChangedEvent>(_ => Fit());
            Root.Add(List);
            _items = new ElementPool<NavTab>(index =>
            {
                var tab = NavTab.Create("nav-dial-item-", index, true, () => Pick(index));
                tab.Button.AddToClassList("nav-dial__item");
                return tab;
            }, tab => tab.Button.RemoveFromHierarchy());
            _tint = new NavTint(Root);
        }

        public void Bind(CardNavModel model, NavController nav, BlockBindContext context)
        {
            Unbind();
            _model = model;
            _nav = nav;
            _overlay = context?.Overlay;
            var strings = context?.Strings;
            Fab.Words(strings);
            foreach (var section in model.Sections)
            {
                var item = _items.Take();
                item.Label.text = Words(section.LabelKey, section.Key, strings);
                item.Button.tooltip = item.Label.text;
                CardIcons.SetKey(item.Icon, section.IconKey);
                item.Icon.style.display = section.IconKey != null ? DisplayStyle.Flex : DisplayStyle.None;
                _tint.Icon(item.Icon, section.ColorToken);
                NavCount.Set(item.Count, false, section.Count);
                List.Add(item.Button);
            }
            nav.CurrentChanged += OnCurrentChanged;
            OnCurrentChanged(nav.Current);
        }

        public void Unbind()
        {
            if (_nav != null) _nav.CurrentChanged -= OnCurrentChanged;
            _overlay?.Close(ListKey);
            Show(false);
            _nav = null;
            _model = null;
            _overlay = null;
            _items.ReleaseAll();
            _tint.Clear();
        }

        // The button: open the column, or close it while it is open
        public void Toggle()
        {
            if (IsOpen)
            {
                if (_overlay != null) _overlay.Close(ListKey);
                else Show(false);
                return;
            }
            // - the overlay owner closes whatever else was open first, and runs Show(false) however this closes
            _overlay?.Open(ListKey, null, () => Show(false));
            Show(true);
        }

        // A pill: its section's first block (the overlay owner hears the jump and closes the column)
        public void Pick(int index)
        {
            if (_model == null || index < 0 || index >= _model.Sections.Count) return;
            _nav?.JumpTo(index, BuiltInNavElements.FabDial);
        }

        private void Show(bool open)
        {
            IsOpen = open;
            Fab.SetOpen(open);
            List.style.display = open ? DisplayStyle.Flex : DisplayStyle.None;
            if (open) Fit();
        }

        // Which way the open column goes and how tall it may grow: the column's whole height against the room above and below the button
        private void Fit()
        {
            if (!IsOpen) return;
            float listHeight = List.contentContainer.layout.height;
            if (float.IsNaN(listHeight)) return;
            var layer = Fab.LayerBounds();
            var button = Root.worldBound;
            // - the column keeps its margin from the button (Navigation.uss): the same on both sides
            float margin = List.resolvedStyle.marginTop;
            float above = button.yMin - layer.yMin - margin, below = layer.yMax - button.yMax - margin;
            var direction = FabFanRule.Opens(above, below, listHeight);
            if (direction != Direction)
            {
                Direction = direction;
                List.EnableInClassList(UpClass, direction == FabFanRule.Direction.Up);
                List.EnableInClassList(DownClass, direction == FabFanRule.Direction.Down);
            }
            float room = FabFanRule.Room(direction, above, below);
            if (List.style.maxHeight.keyword != StyleKeyword.Undefined || System.Math.Abs(List.style.maxHeight.value.value - room) > 0.5f)
                List.style.maxHeight = room;
        }

        // The reader moved: mark the section being read
        private void OnCurrentChanged(int section)
        {
            foreach (var item in _items.Shown) item.Button.EnableInClassList(NavTab.CurrentClass, item.Index == section);
        }

        // A label key's words in the card's language (the key itself when none are found)
        private static string Words(string labelKey, string key, CardStrings strings)
        {
            string words = labelKey != null && strings != null ? strings.Get(labelKey) : "";
            return string.IsNullOrEmpty(words) ? key : words;
        }
    }
}
