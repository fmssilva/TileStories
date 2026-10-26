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
        // A released drag rests the row on the nearest item's start (NavStripRule.SnapOffset: the story's carousel, App Store style); off for
        // the navigation strips, which stop where the finger leaves them
        public bool SnapsToItems { get; set; }
        // The row wears this while it settles on an item after a release (a stylesheet may glide it there); a new drag takes it off
        public const string SettlingClass = "ts-strip--settling";

        private readonly SwipeGrab _drag;
        private float _dragStartOffset;
        // The item to bring into view once the row is laid out (null = none pending)
        private VisualElement _pendingShow;
        // Whether that item goes to the middle of the strip (ShowCentred) rather than just into view
        private bool _pendingCentred;

        // `name`: the element's name, the strip's parts are named after it ("<name>-viewport", "<name>-row")
        public NavStrip(string name, string viewportClass, string rowClass)
        {
            Viewport = new VisualElement { name = name + "-viewport" };
            Viewport.AddToClassList(viewportClass);
            Row = new VisualElement { name = name + "-row" };
            Row.AddToClassList(rowClass);
            Viewport.Add(Row);
            _drag = new SwipeGrab(Viewport, SwipeGrabRule.ClaimsPointer);
            _drag.Claimed += _ =>
            {
                Row.RemoveFromClassList(SettlingClass);
                _dragStartOffset = Offset;
            };
            _drag.Moved += travel => SetOffset(_dragStartOffset - travel.x);
            _drag.Released += _ => Snap();
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
            _pendingCentred = false;
            Relayout();
        }

        // Bring an item to the middle of the strip (clamped at the ends) -- now if it is laid out, else as soon as it is: a row stepped through
        // by taps keeps the neighbours on both sides in reach (NavStripRule.OffsetToCentre)
        public void ShowCentred(VisualElement item)
        {
            _pendingShow = item;
            _pendingCentred = true;
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
                SetOffset(_pendingCentred
                    ? NavStripRule.OffsetToCentre(box.x, box.width, VisibleWidth, Row.layout.width, Offset)
                    : NavStripRule.OffsetToShow(box.x, box.width, VisibleWidth, Row.layout.width, Offset));
                return;
            }
            SetOffset(Offset);
        }

        // A released drag of a snapping strip: rest on the nearest item's start -- measured from the row's own inset (its left padding), so a
        // row that starts its first item at the text's margin rests every item there
        private void Snap()
        {
            if (!SnapsToItems) return;
            float inset = Row.resolvedStyle.paddingLeft;
            var starts = new System.Collections.Generic.List<float>();
            foreach (var item in Row.Children()) starts.Add(item.layout.x - inset);
            Row.AddToClassList(SettlingClass);
            SetOffset(NavStripRule.SnapOffset(starts, Offset, Row.layout.width, VisibleWidth));
        }

        private void SetOffset(float offset)
        {
            Offset = NavStripRule.Clamp(offset, Row.layout.width, VisibleWidth);
            Row.style.translate = new Translate(-Offset, 0f);
        }
    }
}
