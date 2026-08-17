using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // Screen-space zoom affordance for ARZoomController.
    //
    // Thin MonoBehaviour: it owns no zoom math of its own. It clones the shared
    // UXML template into a UIDocument, applies safe-area padding, and routes the
    // +/-/fit button clicks to the already-built ARZoomController entry points
    // (ZoomIn / ZoomOut / ResetToBase, spec section 9).
    //
    // Visibility is NOT decided here -- ARZoomController.Settings is private, so the
    // caller (WallSession, which owns the public LodSettings accessor) decides
    // whether to mount/toggle this overlay based on LodSettings.zoom_show_ui_buttons
    // and invokes SetButtonsVisible accordingly. All visual constants (colors,
    // radii, button size, the 44px WCAG floor) live in ZoomControlView.uss, never in
    // code or UXML (30-ui-content rule 2).
    [DisallowMultipleComponent]
    public sealed class ZoomControlView : MonoBehaviour
    {
        [Header("Drivers")]
        [Tooltip("Controller whose ZoomIn/ZoomOut/ResetToBase the buttons invoke.")]
        [SerializeField] private ARZoomController _zoom;

        [Header("UI Toolkit")]
        [Tooltip("Shared UXML template cloned into the UI document root.")]
        [SerializeField] private VisualTreeAsset _template;

        private VisualElement _root;
        public VisualElement Root => _root;

        // Mount the control into a UIDocument's root. This is the single entry point
        // used by both the production harness and tests. PanelSettings lives on the
        // UIDocument itself (Shared preset) -- this class does not own it.
        public void Mount(UIDocument document)
        {
            if (document == null) throw new ArgumentException("UIDocument must be assigned", nameof(document));
            if (_template == null) throw new InvalidOperationException("ZoomControlView._template is unassigned");
            if (_zoom == null) throw new InvalidOperationException("ZoomControlView._zoom is unassigned");

            _root = _template.CloneTree();
            SafeAreaHelper.ApplyToRoot(_root);
            document.rootVisualElement.Add(_root);
            BindButtons();
        }

        // Show/hide the whole overlay without re-mounting (e.g. toggle off when no
        // wall supports zoom, or when fullscreen UI takes over).
        public void SetButtonsVisible(bool visible)
        {
            if (_root != null) _root.visible = visible;
        }

        // Wire the three buttons named in ZoomControlView.uxml to the controller.
        private void BindButtons()
        {
            if (_root == null) return;
            var zoomOut = _root.Q<Button>("zoom-out-button");
            var zoomReset = _root.Q<Button>("zoom-reset-button");
            var zoomIn = _root.Q<Button>("zoom-in-button");
            if (zoomOut != null) zoomOut.clicked += ZoomOutClicked;
            if (zoomReset != null) zoomReset.clicked += ZoomResetClicked;
            if (zoomIn != null) zoomIn.clicked += ZoomInClicked;
        }

        // Route clicks to the static ARZoomController entry points (spec section 9).
        private void ZoomInClicked() => _zoom.ZoomIn();
        private void ZoomOutClicked() => _zoom.ZoomOut();
        private void ZoomResetClicked() => _zoom.ResetToBase();

        // Read-only visibility gate mirroring WallSession.LodSettings.zoom_show_ui_buttons.
        // Kept as a pure helper so the gating rule is unit-assertable without a scene.
        public static bool ShouldShowButtons(LodSettings settings) =>
            settings != null && settings.zoom_show_ui_buttons;
    }
}
