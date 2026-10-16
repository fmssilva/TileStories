using UnityEngine.UIElements;

namespace TileStories
{
    // A row of navigation items that scrolls sideways under a finger when it is wider than the card (_3.2 step 5a, shared since step 6 by
    // every top element that can overflow: the tabs, the chips, the progress map). Built as a clipped viewport and a row moved by
    // NavStripRule's offset, NOT a horizontal ScrollView: Unity's ScrollView takes a touch that moved 10 px sideways even when it mostly
    // moved up or down, and offers no way to judge it first. The row's drag is a SwipeGrab (never captured at touch-down, claimed only when
    // clearly sideways), so a tap still reaches the item under the finger and an up / down drag still reaches the sheet behind it.
    public sealed class NavStrip
    {
        // What clips the strip, and the row of items it moves
        public VisualElement Viewport { get; }
        public VisualElement Row { get; }
        // How far the row is moved left now (NavStripRule's offset)
        public float Offset { get; private set; }

        private readonly SwipeGrab _drag;
        private float _dragStartOffset;
        // The item to bring into view once the row is laid out (null = none pending)
        private VisualElement _pendingShow;

        // `name`: the element's name, the strip's parts are named after it ("<name>-viewport", "<name>-row")
        public NavStrip(string name, string viewportClass, string rowClass)
        {
            Viewport = new VisualElement { name = name + "-viewport" };
            Viewport.AddToClassList(viewportClass);
            Row = new VisualElement { name = name + "-row" };
            Row.AddToClassList(rowClass);
            Viewport.Add(Row);
            _drag = new SwipeGrab(Viewport, SwipeGrabRule.ClaimsPointer);
            _drag.Claimed += _ => _dragStartOffset = Offset;
            _drag.Moved += travel => SetOffset(_dragStartOffset - travel.x);
            // - a new width (another card's words, a rotated screen) can leave the row past its end, or the marked item out of view
            Row.RegisterCallback<GeometryChangedEvent>(_ => Relayout());
            Viewport.RegisterCallback<GeometryChangedEvent>(_ => Relayout());
        }

        // What the row can be seen through: the viewport less its padding (the row starts at the padding, so its end must stop there too)
        public float VisibleWidth => Viewport.contentRect.width;

        // Back to the row's start (a new card)
        public void Reset()
        {
            _pendingShow = null;
            SetOffset(0f);
        }

        // Bring a whole item into view with the least move -- now if it is laid out, else as soon as it is
        public void Show(VisualElement item)
        {
            _pendingShow = item;
            Relayout();
        }

        private void Relayout()
        {
            if (_pendingShow != null)
            {
                var box = _pendingShow.layout;
                if (_pendingShow.parent != Row || float.IsNaN(box.width) || float.IsNaN(VisibleWidth))
                {
                    if (_pendingShow.parent != Row) _pendingShow = null;
                    return;
                }
                _pendingShow = null;
                SetOffset(NavStripRule.OffsetToShow(box.x, box.width, VisibleWidth, Row.layout.width, Offset));
                return;
            }
            SetOffset(Offset);
        }

        private void SetOffset(float offset)
        {
            Offset = NavStripRule.Clamp(offset, Row.layout.width, VisibleWidth);
            Row.style.translate = new Translate(-Offset, 0f);
        }
    }
}
