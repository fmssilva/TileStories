using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The edge_scrubber navigation element (_3.2 section 5.5, canvas E1, L1): one small dot per section down the right edge of the stack's
    // viewport, in the family's colour, with the section being read marked. Two ways in, both always on:
    //   - DRAG: a finger that lands on the column and moves along it (the card's SwipeGrab, ClaimsVertical -- it never captures at
    //     touch-down and judges the drag once past the slop, so a tap still reaches its dot and a finger that lands anywhere else still
    //     scrolls the card) is a scrub: the dot under the finger is marked and a bubble beside it names the family; the release moves the
    //     card to that family's first block (EdgeScrubberRule.IndexAt: past the ends a release picks nothing, and the bubble goes)
    //   - TAP: every dot is a plain button in a full touch-token target, so any family is reached by one tap with no drag at all
    //     (WCAG 2.2 SC 2.5.7, Dragging Movements -- the single-pointer path the radial also keeps)
    // The element lies over the content, so it is drawn only where the viewport holds EVERY dot's full target (EdgeScrubberRule.Fits): a
    // rail with a family a finger cannot reach would break the promise above. Its host (NavSlots.Edge) is the viewport itself, so a
    // viewport that grows or shrinks with the chrome re-lays the column on its own geometry event. Views only: built from NavTint,
    // SwipeGrab and the model; the arithmetic is EdgeScrubberRule's.
    public sealed class EdgeScrubberNavView : INavElementView
    {
        public const string Key = BuiltInNavElements.EdgeScrubber;
        // The dot of the section being read / the one a finger is on now: a larger disc with an accent edge (never colour alone)
        public const string CurrentClass = "nav-edge__item--current";
        public const string PointedClass = "nav-edge__item--pointed";
        public const string HiddenClass = "nav-edge--hidden";
        public const string BubbleShownClass = "nav-edge__bubble--shown";

        // One dot: its tap target (a button the size of the touch token), the disc drawn inside it and the family's colour inside that.
        // Its place in the column IS its section's index in the model
        public sealed class Item
        {
            public readonly Button Button;
            public readonly VisualElement Disc;
            public readonly VisualElement Fill;
            // The family's words (the bubble's text and the dot's tooltip)
            public string Words = "";

            public Item(int index, System.Action clicked)
            {
                Button = new Button(clicked) { name = "nav-edge-item-" + index };
                Button.AddToClassList("nav-edge__item");
                Disc = new VisualElement { pickingMode = PickingMode.Ignore };
                Disc.AddToClassList("nav-edge__disc");
                Fill = new VisualElement { pickingMode = PickingMode.Ignore };
                Fill.AddToClassList("nav-edge__fill");
                Disc.Add(Fill);
                Button.Add(Disc);
            }
        }

        public VisualElement Root { get; }
        // The column of dots (what the drag watches), placed by EdgeScrubberRule
        public VisualElement Column { get; }
        public VisualElement Bubble { get; }
        public Label BubbleLabel { get; }
        public IReadOnlyList<Item> Items => _items.Shown;
        public EdgeScrubberRule.Column Layout { get; private set; }
        // Whether the rail is drawn now (the viewport holds every dot's full target)
        public bool IsShown { get; private set; }
        // The dot a dragging finger is on now (-1 = none: not dragging, or past the ends)
        public int Pointed { get; private set; } = -1;
        public bool IsDragging => _drag.IsClaimed;

        private readonly ElementPool<Item> _items;
        private readonly NavTint _tint;
        private readonly SwipeGrab _drag;
        private CardNavModel _model;
        private NavController _nav;
        private CardStrings _strings;
        private Vector2 _dragStart;

        public EdgeScrubberNavView()
        {
            Root = new VisualElement { name = "nav-edge", pickingMode = PickingMode.Ignore };
            Root.AddToClassList("nav-edge");
            Root.AddToClassList(HiddenClass);
            Column = new VisualElement { name = "nav-edge-column" };
            Column.AddToClassList("nav-edge__column");
            Bubble = new VisualElement { name = "nav-edge-bubble", pickingMode = PickingMode.Ignore };
            Bubble.AddToClassList("nav-edge__bubble");
            BubbleLabel = new Label { pickingMode = PickingMode.Ignore };
            BubbleLabel.AddToClassList("nav-edge__label");
            Bubble.Add(BubbleLabel);
            Root.Add(Column);
            Root.Add(Bubble);
            _items = new ElementPool<Item>(index => new Item(index, () => Pick(index)), item => item.Button.RemoveFromHierarchy());
            _tint = new NavTint(Root);
            _drag = new SwipeGrab(Column, SwipeGrabRule.ClaimsVertical);
            _drag.Claimed += start => _dragStart = start;
            _drag.Moved += travel => Point(EdgeScrubberRule.IndexAt(Layout, PanelYToViewport(_dragStart.y + travel.y)));
            _drag.Released += travel => Release(EdgeScrubberRule.IndexAt(Layout, PanelYToViewport(_dragStart.y + travel.y)));
            _drag.Cancelled += () => Point(-1);
            Root.RegisterCallback<GeometryChangedEvent>(_ => Arrange());
        }

        public void Bind(CardNavModel model, NavController nav, BlockBindContext context)
        {
            Unbind();
            _model = model;
            _nav = nav;
            _strings = context?.Strings;
            for (int i = 0; i < model.Sections.Count; i++) Fill(_items.Take(), model.Sections[i]);
            nav.CurrentChanged += OnCurrentChanged;
            OnCurrentChanged(nav.Current);
            Arrange();
        }

        public void Unbind()
        {
            if (_nav != null) _nav.CurrentChanged -= OnCurrentChanged;
            Point(-1);
            _nav = null;
            _model = null;
            _strings = null;
            _items.ReleaseAll();
            _tint.Clear();
        }

        // A dot picked (a tap on its button, or a scrub's release over it): the card moves to its section's first block
        public void Pick(int section)
        {
            if (_nav == null || _model == null || section < 0 || section >= _model.Sections.Count) return;
            _nav.JumpTo(section, Key);
        }

        // The scrub ended: the dot under the finger, or nothing (the finger left the column's ends)
        private void Release(int section)
        {
            Point(-1);
            if (section >= 0) Pick(section);
        }

        // Mark the dot a finger is on and name it in the bubble beside it (-1: no dot, no bubble)
        private void Point(int index)
        {
            Pointed = index;
            for (int i = 0; i < _items.Shown.Count; i++) _items.Shown[i].Button.EnableInClassList(PointedClass, i == index);
            bool named = index >= 0 && index < _items.Shown.Count;
            Bubble.EnableInClassList(BubbleShownClass, named);
            if (!named) return;
            BubbleLabel.text = _items.Shown[index].Words;
            // - the bubble sits at the dot's own height, left of the edge: the thumb never covers it
            Bubble.style.top = Layout.CentreOf(index);
        }

        private void Fill(Item item, CardNavModel.Section section)
        {
            string words = _strings?.Get(section.LabelKey ?? section.Key) ?? "";
            item.Words = string.IsNullOrEmpty(words) ? section.LabelKey ?? section.Key : words;
            item.Button.tooltip = item.Words;
            _tint.Fill(item.Fill, section.ColorToken);
            Column.Add(item.Button);
        }

        // The reader moved: mark the dot of the section being read
        private void OnCurrentChanged(int section)
        {
            for (int i = 0; i < _items.Shown.Count; i++) _items.Shown[i].Button.EnableInClassList(CurrentClass, i == section);
        }

        // Draw the rail only where the viewport holds every dot's full target, centred; else hidden (still laid out, so the target stays measurable)
        private void Arrange()
        {
            int count = _items.Shown.Count;
            float target = count > 0 ? _items.Shown[0].Button.resolvedStyle.height : 0f;
            float viewport = Root.layout.height;
            IsShown = EdgeScrubberRule.Fits(count, viewport, target);
            Root.EnableInClassList(HiddenClass, !IsShown);
            if (!IsShown) return;
            Layout = EdgeScrubberRule.Layout(count, viewport, target);
            Column.style.top = Layout.Top;
            Column.style.height = Layout.Height;
        }

        // A panel y (what a pointer event carries) in the viewport's own frame, the rule's
        private float PanelYToViewport(float panelY) => Root.WorldToLocal(new Vector2(0f, panelY)).y;
    }
}
