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

        private readonly Func<Vector2, bool> _claims;
        // The finger that went down here (-1 = none) and where
        private int _pointer = -1;
        private Vector2 _start;

        // `claims`: whether a drag that travelled this far is this element's (SwipeGrabRule.ClaimsPointer for a sideways element)
        public SwipeGrab(VisualElement area, Func<Vector2, bool> claims)
        {
            Area = area ?? throw new ArgumentNullException(nameof(area));
            _claims = claims ?? throw new ArgumentNullException(nameof(claims));
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
            _pointer = evt.pointerId;
            _start = evt.position;
            IsClaimed = false;
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (_pointer != evt.pointerId) return;
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
