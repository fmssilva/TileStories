using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Real pointer input for the POI Detail Card tests: UI Toolkit pointer events sent to the panel at a panel
    // position, so the panel itself picks the element under the point (and pointer capture routes the moves),
    // exactly as a finger's events would. A drag is spread over frames so the sheet measures a real speed.
    public static class CardTestInput
    {
        // Drag from the centre of `from` by `deltaY` panel units (negative = up), over `frames` frames
        public static IEnumerator Drag(VisualElement from, float deltaY, int frames = 12)
        {
            Assert.IsNotNull(from?.panel, "the drag starts on an element of a live panel");
            var panel = from.panel;
            Vector2 start = from.worldBound.center;
            Send(panel, EventType.MouseDown, start);
            yield return null;
            for (int i = 1; i <= frames; i++)
            {
                Send(panel, EventType.MouseDrag, start + new Vector2(0f, deltaY * i / frames));
                yield return null;
            }
            Send(panel, EventType.MouseUp, start + new Vector2(0f, deltaY));
            yield return null;
        }

        private static void Send(IPanel panel, EventType type, Vector2 position)
        {
            var e = new Event { type = type, mousePosition = position, button = 0, clickCount = 1 };
            EventBase evt = type switch
            {
                EventType.MouseDown => PointerDownEvent.GetPooled(e),
                EventType.MouseUp => PointerUpEvent.GetPooled(e),
                _ => PointerMoveEvent.GetPooled(e),
            };
            using (evt) panel.visualTree.SendEvent(evt);
        }

        // Wait until the sheet's height transition has finished (it animates for --ts-duration)
        public static IEnumerator Settle(float seconds = 0.45f)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end) yield return null;
        }

        // WCAG contrast of a text colour (alpha blended over its background) against that background
        public static float Contrast(Color text, Color background)
        {
            var blended = Color.Lerp(background, new Color(text.r, text.g, text.b, 1f), text.a);
            return UIAccessibility.ContrastRatio(blended, background);
        }
    }
}
