using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace TileStories
{
    // Whether a screen point lands on the screen-space UI (a UI Toolkit panel's pickable element: the POI Detail Card,
    // the search UI, the zoom buttons). World-space markers do not count. The AR zoom gestures leave such touches to the
    // UI: a pinch on a card picture zooms the picture, not the camera (_3.1 step 7).
    public static class ScreenUIHit
    {
        private static readonly List<RaycastResult> Hits = new();

        // True when a UI Toolkit element under `screenPoint` (pixels, origin bottom-left) takes pointer input
        public static bool IsOverScreenUI(Vector2 screenPoint)
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null) return false;
            Hits.Clear();
            eventSystem.RaycastAll(new PointerEventData(eventSystem) { position = screenPoint }, Hits);
            foreach (var hit in Hits)
                if (hit.module is PanelRaycaster) return true;
            return false;
        }
    }
}
