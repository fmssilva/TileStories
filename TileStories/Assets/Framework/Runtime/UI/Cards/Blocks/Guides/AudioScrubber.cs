using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The bar of an audio player (_3.1 step 9A): a thin track with the played part filled and a knob at the position. A press anywhere on
    // the bar (a finger-tall hit area, --ts-touch-target) and a drag along it report the point as a 0..1 fraction; the bar captures the
    // pointer, so the card's stack does not scroll while the visitor scrubs (the same pattern as before_after). It only REPORTS what the
    // visitor asked (Scrubbed): moving the audio is the owner's job, and the bar shows whatever position it is given. Only classes here;
    // Guides.uss holds every size and colour (the played part is a percentage of the track, from the position).
    public sealed class AudioScrubber
    {
        public VisualElement Root { get; }
        public VisualElement Track { get; }
        public VisualElement Fill { get; }
        public VisualElement Knob { get; }

        // 0..1: how much of the clip is played (what the fill and the knob show)
        public float Fraction { get; private set; }
        public bool IsDragging { get; private set; }

        // Raised on a press and on every move of a drag, with the 0..1 point of the bar under the finger
        public event Action<float> Scrubbed;

        public AudioScrubber()
        {
            Root = new VisualElement { name = "card-audio-scrubber" };
            Root.AddToClassList("card-audio__scrubber");
            Track = new VisualElement { pickingMode = PickingMode.Ignore };
            Track.AddToClassList("card-audio__track");
            Fill = new VisualElement { pickingMode = PickingMode.Ignore };
            Fill.AddToClassList("card-audio__fill");
            Knob = new VisualElement { pickingMode = PickingMode.Ignore };
            Knob.AddToClassList("card-audio__knob");
            Track.Add(Fill);
            Track.Add(Knob);
            Root.Add(Track);
            Root.RegisterCallback<PointerDownEvent>(OnPointerDown);
            Root.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            Root.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        // Show `fraction` (clamped to 0..1) as the played part
        public void SetFraction(float fraction)
        {
            Fraction = Mathf.Clamp01(float.IsNaN(fraction) ? 0f : fraction);
            Fill.style.width = Length.Percent(Fraction * 100f);
            Knob.style.left = Length.Percent(Fraction * 100f);
        }

        // Whether a press on the bar counts (the hero chip's bar is a progress line only)
        public void SetInteractive(bool interactive) => Root.pickingMode = interactive ? PickingMode.Position : PickingMode.Ignore;

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0 && evt.pointerType == UnityEngine.UIElements.PointerType.mouse) return;
            Root.CapturePointer(evt.pointerId);
            IsDragging = true;
            Report(evt.localPosition.x);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!IsDragging || !Root.HasPointerCapture(evt.pointerId)) return;
            Report(evt.localPosition.x);
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!Root.HasPointerCapture(evt.pointerId)) return;
            Root.ReleasePointer(evt.pointerId);
            IsDragging = false;
            evt.StopPropagation();
        }

        // The 0..1 point of the bar's width under `localX`
        private void Report(float localX)
        {
            float width = Root.layout.width;
            if (width > 0f) Scrubbed?.Invoke(Mathf.Clamp01(localX / width));
        }
    }
}
