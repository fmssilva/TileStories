using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The fab_radial navigation element (_3.2 section 5.4, canvas C2, L1): ONE round button in the card's floating slot whose sections open on
    // an ARC around it -- a pie / marking menu (Callahan et al. 1988; Kurtenbach & Buxton 1994): once learned, a press, a slide towards an item
    // and a release is faster than reading a list. Each item is a round button with the family's icon in its colour and its words under it
    // (never colour alone). Two ways in, both always on:
    //   - PRESS-SLIDE-RELEASE: a finger that leaves the button opens the arc (the card's SwipeGrab, claiming a slide in any direction past its
    //     slop -- the one way a part of the card takes a finger); the item its direction points at is marked as it moves; the release picks
    //     it (RadialMenuRule.ItemAt), or picks nothing when it ends inside the dead zone or outside the arc
    //   - a plain TAP opens the arc with every item tappable, and a tap on one picks it -- the single-pointer path WCAG 2.2 SC 2.5.7 (Dragging
    //     Movements) asks for; a tap on the button again (its X) closes the arc
    // A PAGE of the arc holds as many items as Bar Slots allows and the room around the button fits (RadialMenuRule.Capacity: all of Bar Slots
    // at peek and half; fewer at Full, where the button rests in the screen's top corner), the last one More while sections are left (rule 2.5.5,
    // as a bar): a narrow sector is a missed slide. More opens the wall's Menu when it has one, else turns the arc to the next page (a second
    // level of the marking menu). Where the arc lies is RadialMenuRule.Layout's (the left half-plane, every item inside the card layer). The open
    // arc is an anchored client of the card's overlay owner (CardMenuOverlay): Back, a jump, a touch on the card, a stop change close it.
    // Views only: built from NavFab, NavTint, SwipeGrab and the model.
    public sealed class FabRadialNavView : INavElementView
    {
        // The overlay key of the open arc: the element's own key, so the floating slot dims the card behind it (FloatingSlotRule.Dims)
        public const string ArcKey = BuiltInNavElements.FabRadial;
        public const string PointedClass = "nav-radial__item--pointed";

        // One item of the arc: a round button (its family's icon) and its words under it, the whole item the tap target
        public sealed class Item
        {
            public readonly Button Button;
            public readonly VisualElement Disc;
            public readonly VisualElement Icon;
            public readonly Label Label;
            // The section it moves the card to (-1 = More)
            public int Section { get; set; }

            public Item(int index, System.Action clicked)
            {
                Button = new Button(clicked) { name = "nav-radial-item-" + index };
                Button.AddToClassList("nav-radial__item");
                Disc = new VisualElement { pickingMode = PickingMode.Ignore };
                Disc.AddToClassList("nav-radial__disc");
                Icon = CardIcons.Create();
                Icon.pickingMode = PickingMode.Ignore;
                Disc.Add(Icon);
                Label = new Label { pickingMode = PickingMode.Ignore };
                Label.AddToClassList("nav-radial__label");
                Button.Add(Disc);
                Button.Add(Label);
            }
        }

        public VisualElement Root { get; }
        public NavFab Fab { get; }
        // The arc's items, laid out around the button's centre (hidden while closed)
        public VisualElement Arc { get; }
        public IReadOnlyList<Item> Items => _items.Shown;
        public RadialMenuRule.Arc Layout { get; private set; }
        public bool IsOpen { get; private set; }
        // The most items a page holds here (Bar Slots, or fewer where the room is short) and the index of its first section
        public int Capacity { get; private set; }
        public int PageStart { get; private set; }
        // Whether the arc shows a later page (More was picked) rather than the first one
        public bool ShowsMore => PageStart > 0;
        // The item a sliding finger points at now (-1 = none)
        public int Pointed { get; private set; } = -1;

        private static readonly CustomStyleProperty<float> ItemWidthProperty = new("--ts-radial-item-width");
        private static readonly CustomStyleProperty<float> ItemHeightProperty = new("--ts-radial-item-height");
        private static readonly CustomStyleProperty<float> MinRadiusProperty = new("--ts-radial-min-radius");
        private static readonly CustomStyleProperty<float> DeadZoneProperty = new("--ts-radial-dead-zone");
        private readonly ElementPool<Item> _items;
        private readonly NavTint _tint;
        private readonly SwipeGrab _slide;
        private RadialMenuRule.ItemSize _size;
        private float _minRadius, _deadZone;
        private CardNavModel _model;
        private NavController _nav;
        private CardMenuOverlay _overlay;
        private CardStrings _strings;
        private int _barSlots;

        public FabRadialNavView()
        {
            Fab = new NavFab("nav-radial", Tap);
            Root = Fab.Root;
            Root.AddToClassList("nav-radial");
            // - the arc's origin is the button's centre (Navigation.uss); it never takes a finger itself, only its items do
            Arc = new VisualElement { name = "nav-radial-arc", pickingMode = PickingMode.Ignore };
            Arc.AddToClassList("nav-radial__arc");
            Arc.style.display = DisplayStyle.None;
            Root.Add(Arc);
            _items = new ElementPool<Item>(index => new Item(index, () => Pick(index)), item => item.Button.RemoveFromHierarchy());
            _tint = new NavTint(Root);
            // - any direction claims: the arc lies in a whole half-plane, and a slide the wrong way is answered by picking nothing
            _slide = new SwipeGrab(Root, travel => travel.magnitude > SwipeGrabRule.Slop);
            _slide.Claimed += _ => { if (!IsOpen) Open(); };
            _slide.Moved += travel => Point(RadialMenuRule.ItemAt(travel, Layout, _deadZone));
            _slide.Released += Release;
            _slide.Cancelled += Close;
        }

        public void Bind(CardNavModel model, NavController nav, BlockBindContext context)
        {
            Unbind();
            _model = model;
            _nav = nav;
            _overlay = context?.Overlay;
            _strings = context?.Strings;
            _barSlots = NavOntologyRule.MaxSlots(context?.Settings?.navigation);
            Fab.Words(_strings);
            nav.CurrentChanged += OnCurrentChanged;
        }

        public void Unbind()
        {
            if (_nav != null) _nav.CurrentChanged -= OnCurrentChanged;
            _overlay?.Close(ArcKey);
            Show(false);
            _nav = null;
            _model = null;
            _overlay = null;
            _items.ReleaseAll();
            _tint.Clear();
        }

        // The button tapped: open the arc (every item tappable), or close it while it is open
        public void Tap()
        {
            if (IsOpen) Close();
            else Open();
        }

        // Open the arc on its first page
        public void Open()
        {
            if (_model == null) return;
            // - the overlay owner closes whatever else was open first, and runs Show(false) however this closes
            _overlay?.Open(ArcKey, null, () => Show(false));
            ReadTokens();
            // - how many items a page holds HERE: Bar Slots, or fewer where the room around the button is short (a card not laid out yet --
            //   no room known -- holds Bar Slots)
            int wanted = Mathf.Min(_barSlots, Mathf.Max(RadialMenuRule.MinItems, _model.Sections.Count));
            Capacity = TryRoomHere(out var room) ? RadialMenuRule.Capacity(wanted, room, _size, _minRadius) : wanted;
            Draw(0);
            Show(true);
        }

        public void Close()
        {
            if (_overlay != null && _overlay.OpenKey == ArcKey) _overlay.Close();
            else Show(false);
        }

        // An item picked (a tap, or a slide's release): its section's first block; More opens the wall's Menu, else turns the arc to the next page
        public void Pick(int index)
        {
            if (_model == null || index < 0 || index >= _items.Shown.Count) return;
            int section = _items.Shown[index].Section;
            if (section >= 0)
            {
                // - the overlay owner hears the jump and closes the arc
                _nav?.JumpTo(section, BuiltInNavElements.FabRadial);
                return;
            }
            if (_overlay != null && _overlay.HasMenu) _overlay.OpenMenu();
            else
            {
                Draw(PageStart + _items.Shown.Count - 1);
                Point(-1);
            }
        }

        // The slide's release: the item its direction points at, or nothing (and the arc closes)
        private void Release(Vector2 travel)
        {
            int index = RadialMenuRule.ItemAt(travel, Layout, _deadZone);
            Point(-1);
            if (index >= 0) Pick(index);
            else Close();
        }

        // Mark the item a sliding finger points at (a thicker accent edge and bolder words: never colour alone)
        private void Point(int index)
        {
            Pointed = index;
            for (int i = 0; i < _items.Shown.Count; i++) _items.Shown[i].Button.EnableInClassList(PointedClass, i == index);
        }

        // The page starting at section `start`: its sections (rule 2.5.5 over what is left: all of them when they fit, else one fewer and More),
        // laid out on the arc
        private void Draw(int start)
        {
            PageStart = start;
            _items.ReleaseAll();
            _tint.Clear();
            int left = _model.Sections.Count - start;
            int shown = NavOntologyRule.SectionsInBar(left, Capacity);
            for (int i = start; i < start + shown; i++)
            {
                var section = _model.Sections[i];
                Fill(_items.Take(), i, section.LabelKey ?? section.Key, section.IconKey, section.ColorToken);
            }
            if (shown < left) Fill(_items.Take(), -1, CardStrings.Keys.NavMore, CardIcons.More, null);
            LayOut();
            OnCurrentChanged(_nav?.Current ?? -1);
        }

        private void Fill(Item item, int section, string labelKey, string iconKey, string colorToken)
        {
            item.Section = section;
            string words = _strings?.Get(labelKey) ?? "";
            item.Label.text = string.IsNullOrEmpty(words) ? labelKey : words;
            item.Button.tooltip = item.Label.text;
            CardIcons.SetKey(item.Icon, iconKey);
            item.Icon.style.display = iconKey != null ? DisplayStyle.Flex : DisplayStyle.None;
            _tint.Icon(item.Icon, colorToken);
            Arc.Add(item.Button);
        }

        // The room around the button's centre inside the card layer (false before the card is laid out)
        private bool TryRoomHere(out RadialMenuRule.Room room)
        {
            var layer = Fab.LayerBounds();
            var centre = Root.worldBound.center;
            room = default;
            if (float.IsNaN(centre.x) || float.IsNaN(layer.width) || layer.width <= 0f) return false;
            room = new RadialMenuRule.Room(centre.y - layer.yMin, layer.yMax - centre.y, centre.x - layer.xMin, layer.xMax - centre.x);
            return true;
        }

        // Place every item of the page on the arc RadialMenuRule lays out in the room around the button
        private void LayOut()
        {
            if (!TryRoomHere(out var room)) return;
            Layout = RadialMenuRule.Layout(_items.Shown.Count, room, _size, _minRadius);
            for (int i = 0; i < _items.Shown.Count; i++)
            {
                var button = _items.Shown[i].Button;
                var at = Layout.Offset(i);
                // - the whole item is centred on its point of the arc (its size is the tokens', ReadTokens)
                button.style.width = _size.Width;
                button.style.height = _size.Height;
                button.style.left = at.x - _size.Width / 2f;
                button.style.top = at.y - _size.Height / 2f;
            }
        }

        private void Show(bool open)
        {
            IsOpen = open;
            Fab.SetOpen(open);
            Arc.style.display = open ? DisplayStyle.Flex : DisplayStyle.None;
            if (!open)
            {
                PageStart = 0;
                Point(-1);
            }
        }

        // The reader moved: mark the item of the section being read (More for one on a later page)
        private void OnCurrentChanged(int section)
        {
            if (_model == null) return;
            foreach (var item in _items.Shown)
                item.Button.EnableInClassList(NavTab.CurrentClass, item.Section >= 0 ? item.Section == section : section >= PageStart + _items.Shown.Count - 1);
        }

        // The arc's sizes from the card's tokens: a custom property is only in the customStyle of the element that declares it -- the card
        // root (.ts-card) above this view, the floating slot -- so they are read there, as CardTrackFit reads --ts-track-peek
        private void ReadTokens()
        {
            for (var e = Root.parent; e != null; e = e.parent)
            {
                if (!e.ClassListContains("ts-card")) continue;
                e.customStyle.TryGetValue(ItemWidthProperty, out float width);
                e.customStyle.TryGetValue(ItemHeightProperty, out float height);
                _size = new RadialMenuRule.ItemSize(width, height);
                e.customStyle.TryGetValue(MinRadiusProperty, out _minRadius);
                e.customStyle.TryGetValue(DeadZoneProperty, out _deadZone);
                return;
            }
        }
    }
}
