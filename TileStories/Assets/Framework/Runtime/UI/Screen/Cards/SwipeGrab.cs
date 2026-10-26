using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The ONE way a swipeable part of the card takes a finger (_3.2, the pattern of every swipeable element: the true / false card,
    // the tab strip, later the rails, the pager and the scrubber). It never captures at touch-down: a finger that lands on it is only
    // remembered, so a tap still reaches the button under it and a drag the element does not want still reaches whatever is behind
    // it (the stack's ScrollView, the sheet). Once the finger has moved, `claims` judges its travel (SwipeGrabRule: past the slop
    // and clearly along the element's own axis); a claimed drag captures the pointer, follows it (Moved) and ends on release
    // (Released) or when something else takes the pointer (Cancelled). Plain C#: the element owns one per area.
    public sealed class SwipeGrab
    {
        public VisualElement Area { get; }

        // Whether a claimed drag is going on now
        public bool IsClaimed { get; private set; }

        // A drag was claimed: where its finger went down (panel units)
        public event Action<Vector2> Claimed;
        // The claimed finger moved: its travel from where it went down
        public event Action<Vector2> Moved;
        // The claimed finger was lifted: its whole travel
        public event Action<Vector2> Released;
        // The claimed drag ended without a release (the pointer was cancelled or taken by another element)
        public event Action Cancelled;

        // Every grab's area wears this class: an area that leaves fingers to the parts inside it (the card's pages) knows another
        // swipeable part by it
        public const string AreaClass = "ts-swipe-area";

        private readonly Func<Vector2, bool> _claims;
        // Whether a finger that went down on a part that drags by itself is left to that part (SwipeGrabRule.LeavesToPart)
        private readonly bool _leavesToParts;
        // The finger that went down here (-1 = none), where, and whether it has moved since (a capture before any move = taken at touch-down)
        private int _pointer = -1;
        private Vector2 _start;
        private bool _movedSinceDown;

        // `claims`: whether a drag that travelled this far is this element's (SwipeGrabRule.ClaimsPointer for a sideways element).
        // `leavesToParts`: for an area that holds whole blocks (the card's pages), a finger that went down on a part that drags by itself (a
        // sideways strip, another swipeable part, a stage or slider that took it at touch-down) is never this area's
        public SwipeGrab(VisualElement area, Func<Vector2, bool> claims, bool leavesToParts = false)
        {
            Area = area ?? throw new ArgumentNullException(nameof(area));
            _claims = claims ?? throw new ArgumentNullException(nameof(claims));
            _leavesToParts = leavesToParts;
            area.AddToClassList(AreaClass);
            // - an event aimed at an element INSIDE the area is heard on its way down to it: a Button under the finger stops the press
            //   from bubbling up (its Clickable takes it), so an area that holds buttons only hears the finger before they do. Nested
            //   areas judge outside-in (the sheet's strip area, then the strip's own row): the first to claim captures and stops the
            //   event, the inner one never sees that drag
            area.RegisterCallback<PointerDownEvent>(evt => { if (evt.target != Area) OnPointerDown(evt); }, TrickleDown.TrickleDown);
            area.RegisterCallback<PointerMoveEvent>(evt => { if (evt.target != Area && _capturedChild == null) OnPointerMove(evt); }, TrickleDown.TrickleDown);
            area.RegisterCallback<PointerUpEvent>(evt => { if (evt.target != Area && _capturedChild == null) OnPointerUp(evt); }, TrickleDown.TrickleDown);
            area.RegisterCallback<PointerCancelEvent>(evt => { if (evt.target != Area) OnPointerCancel(evt); }, TrickleDown.TrickleDown);
            // - an event aimed at the area ITSELF (nothing pickable under the finger, or the area holds the capture after a claim) is heard
            //   at the target: a captured pointer's events reach only their target, and not through the trickle-down callbacks
            area.RegisterCallback<PointerDownEvent>(evt => { if (evt.target == Area) OnPointerDown(evt); });
            area.RegisterCallback<PointerMoveEvent>(evt => { if (evt.target == Area) OnPointerMove(evt); });
            area.RegisterCallback<PointerUpEvent>(evt => { if (evt.target == Area) OnPointerUp(evt); });
            area.RegisterCallback<PointerCancelEvent>(evt => { if (evt.target == Area) OnPointerCancel(evt); });
            // - a captured finger's events go to the capturing element ONLY, never up to its parents: when a button under the finger
            //   captures it at touch-down, its moves are followed on the button itself until this area claims the drag (UI Toolkit's own
            //   ScrollView does the same with a captured child)
            area.RegisterCallback<PointerCaptureEvent>(OnSomeoneCaptured);
            area.RegisterCallback<PointerCaptureOutEvent>(OnSomeoneLetGo);
        }

        // The element under the finger that captured it (null = none), whose moves this area follows meanwhile
        private VisualElement _capturedChild;

        private void OnSomeoneCaptured(PointerCaptureEvent evt)
        {
            if (evt.pointerId != _pointer || evt.target == Area || evt.target is not VisualElement child) return;
            // - a part that took the finger at touch-down to drag it keeps it (not a button: it only wants the tap). That includes a ScrollView
            //   stopping its fling: it captures its content at the touch that stops it, and UI Toolkit then hands that finger's moves to it alone
            if (_leavesToParts && !_movedSinceDown && SwipeGrabRule.LeavesToPart(false, false, child is not Button))
            {
                Forget();
                return;
            }
            StopFollowing();
            _capturedChild = child;
            child.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            child.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        private void OnSomeoneLetGo(PointerCaptureOutEvent evt)
        {
            if (evt.target == _capturedChild) StopFollowing();
            // - this area lost a claimed finger to another element (the ScrollView claiming a vertical drag): the drag ends
            else if (evt.target == Area) Cancel();
        }

        private void StopFollowing()
        {
            if (_capturedChild == null) return;
            _capturedChild.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            _capturedChild.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            _capturedChild = null;
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            // - only the main mouse button drags (a touch has no buttons to tell apart)
            if (evt.pointerType == UnityEngine.UIElements.PointerType.mouse && evt.button != 0) return;
            if (_leavesToParts && StartsOnAPartThatDrags(evt.target as VisualElement)) return;
            _pointer = evt.pointerId;
            _start = evt.position;
            _movedSinceDown = false;
            IsClaimed = false;
        }

        // Whether the press landed inside a part (between it and this area) that drags sideways by itself: a sideways ScrollView, or another
        // grab's area
        private bool StartsOnAPartThatDrags(VisualElement target)
        {
            bool sidewaysScroll = false, otherArea = false;
            for (var e = target; e != null && e != Area; e = e.parent)
            {
                if (e is ScrollView scroll && scroll.mode != ScrollViewMode.Vertical) sidewaysScroll = true;
                if (e.ClassListContains(AreaClass)) otherArea = true;
            }
            return SwipeGrabRule.LeavesToPart(sidewaysScroll, otherArea, false);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (_pointer != evt.pointerId) return;
            _movedSinceDown = true;
            // - a finger lifted where this element never heard it (outside the panel): forget it
            if (evt.pressedButtons == 0)
            {
                Forget();
                return;
            }
            Vector2 travel = (Vector2)evt.position - _start;
            if (!IsClaimed)
            {
                // - a drag is judged ONCE, as it leaves the slop: not this area's then means never (a finger that turns later stays with
                //   whoever took it -- the ScrollView, the sheet, an inner strip)
                if (!_claims(travel))
                {
                    if (Mathf.Max(Mathf.Abs(travel.x), Mathf.Abs(travel.y)) > SwipeGrabRule.Slop) Forget();
                    return;
                }
                IsClaimed = true;
                Area.CapturePointer(evt.pointerId);
                Claimed?.Invoke(_start);
            }
            if (!Area.HasPointerCapture(evt.pointerId)) return;
            Moved?.Invoke(travel);
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (_pointer != evt.pointerId) return;
            if (!IsClaimed || !Area.HasPointerCapture(evt.pointerId))
            {
                Forget();
                return;
            }
            Vector2 travel = (Vector2)evt.position - _start;
            // - ended before the release, so the capture-out it raises is not read as a cancel
            Forget();
            Area.ReleasePointer(evt.pointerId);
            Released?.Invoke(travel);
            evt.StopPropagation();
        }

        private void OnPointerCancel(PointerCancelEvent evt)
        {
            if (_pointer == evt.pointerId) Cancel();
        }

        private void Cancel()
        {
            bool wasClaimed = IsClaimed;
            Forget();
            if (wasClaimed) Cancelled?.Invoke();
        }

        private void Forget()
        {
            StopFollowing();
            IsClaimed = false;
            _pointer = -1;
        }
    }
}
