using UnityEngine;

namespace TileStories
{
    // Shared "move around and get closer to a marker" control for every Play Mode demo grid
    // (EffectsPreviewFocus, OutlinePreviewFocus): a manual ZOOM (distance) and PAN (local X/Y)
    // offset on top of the grid's own auto-fit framing, driven by DevCameraInput.
    //
    // Deliberately PAN, never independent ROTATION: every cell's MarkerBillboard always faces
    // Camera.main (hardcoded, MarkerBillboard.cs), not this grid's own dedicated view camera --
    // that is what keeps a cell's label upright and on-screen at all. If this dolly rotated the
    // view camera independently of Camera.main, the cells (still facing Camera.main) would
    // desync from what the grid camera is actually pointed at. Panning/zooming only TRANSLATES
    // the view camera, which never touches that guarantee -- so "look around" on a demo grid
    // means moving the viewport over the (billboarded, always-facing-you) grid, not rotating a
    // free camera through it like a real 3D scene.
    //
    // Pure math here (testable without Input); each Focus component's Update() reads
    // DevCameraInput and calls ApplyScroll/ApplyPan, then Apply()/ClampedPan() feed its own
    // camera transform. One instance per grid: a rebuilt grid gets a fresh instance (WallSession
    // destroys and respawns the whole grid root on most marker/hierarchy edits), so by itself
    // this class always starts a fresh instance at zero. WallSession (2026-09-22) now carries
    // the previous instance's offset across that gap via SetOffsets, so a developer's camera
    // position survives edits made while Play Mode runs instead of snapping back to the auto-fit
    // framing on every keystroke -- ClampedPan still protects against an old pan value being
    // nonsensical for a grid whose layout changed shape.
    public class DevPreviewCameraDolly
    {
        // How many extra/fewer metres one full scroll-wheel "click" (Input.mouseScrollDelta.y == 1)
        // moves the camera. Tuned so a handful of clicks meaningfully closes the distance without
        // needing dozens of clicks to cross the whole range.
        public const float MetresPerScrollClick = 0.35f;

        // WASD/Q-E pan speed, metres/second at 1x input magnitude.
        public const float PanMetresPerSecond = 1.2f;

        // Mouse-drag (RMB / Alt+LMB) pan sensitivity: screen pixels of drag -> metres of pan.
        public const float PanMetresPerMouseDragPixel = 0.004f;

        // Never let the manual zoom alone move the camera outside the grid entirely: clamped as a
        // fraction of the grid's own auto-fit distance, both zoomed all the way in (close enough to
        // read one marker's effect in detail) and zoomed all the way back out (a bit of headroom
        // beyond the default fit, not an unbounded retreat).
        private const float MinDistanceFraction = 0.15f;
        private const float MaxDistanceFraction = 2.5f;

        // Pan is clamped so the grid can be pushed off-centre but never entirely out of frame:
        // a fraction of the grid's own half-extent in each axis.
        private const float MaxPanFractionOfExtent = 0.9f;

        private float _zoomOffsetMetres;
        private Vector2 _panOffsetMetres;

        // The manual offsets (test visibility). Zero until the developer scrolls/pans.
        public float ZoomOffsetMetres => _zoomOffsetMetres;
        public Vector2 PanOffsetMetres => _panOffsetMetres;

        // Scroll wheel input (Input.mouseScrollDelta.y): positive scrolls in (closer), negative out.
        public void ApplyScroll(float scrollDeltaY)
        {
            _zoomOffsetMetres -= scrollDeltaY * MetresPerScrollClick;
        }

        // A continuous zoom delta already converted to metres (W/S * PanMetresPerSecond *
        // deltaTime), for keyboard-driven forward/back alongside the scroll wheel's click-based
        // ApplyScroll -- both accumulate into the same offset.
        public void ApplyZoomDelta(float metres)
        {
            _zoomOffsetMetres -= metres;
        }

        // A pan delta already converted to metres (WASD/QE * PanMetresPerSecond * deltaTime, or a
        // mouse-drag delta * PanMetresPerMouseDragPixel) -- kept as a plain add here so the
        // per-input-source scaling stays in the caller, this class only accumulates.
        public void ApplyPan(Vector2 panDeltaMetres)
        {
            _panOffsetMetres += panDeltaMetres;
        }

        // Reset to the auto-fit framing (no manual zoom or pan).
        public void Reset()
        {
            _zoomOffsetMetres = 0f;
            _panOffsetMetres = Vector2.zero;
        }

        // Absolute-set both offsets at once, unlike the delta-based ApplyPan/ApplyZoomDelta above.
        // Used by WallSession to carry a grid's camera position across a rebuild (2026-09-22).
        public void SetOffsets(Vector2 panOffsetMetres, float zoomOffsetMetres)
        {
            _panOffsetMetres = panOffsetMetres;
            _zoomOffsetMetres = zoomOffsetMetres;
        }

        // The actual camera distance to use this frame: the auto-fit distance plus the manual
        // zoom offset, clamped to a sane range around that same auto-fit distance so scrolling can
        // never push the grid out of view or through the camera.
        public float ApplyZoom(float autoFitDistance)
        {
            float min = autoFitDistance * MinDistanceFraction;
            float max = autoFitDistance * MaxDistanceFraction;
            return Mathf.Clamp(autoFitDistance + _zoomOffsetMetres, min, max);
        }

        // The actual local X/Y camera offset to use this frame, clamped against the grid's own
        // extent so panning can push the grid off-centre but never lose it entirely.
        public Vector2 ClampedPan(Vector2 gridExtent)
        {
            float maxX = gridExtent.x * 0.5f * MaxPanFractionOfExtent;
            float maxY = gridExtent.y * 0.5f * MaxPanFractionOfExtent;
            return new Vector2(Mathf.Clamp(_panOffsetMetres.x, -maxX, maxX), Mathf.Clamp(_panOffsetMetres.y, -maxY, maxY));
        }
    }
}
