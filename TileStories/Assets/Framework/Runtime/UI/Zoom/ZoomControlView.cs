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
    // Mounts itself into its UIDocument on Start and, every frame, shows the overlay only while
    // the wall's zoom is enabled AND Show UI Buttons is on (ShouldShowButtons), so a live Play
    // Mode edit of either switch takes effect at once. All visual constants (colors,
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
        [Tooltip("UIDocument the overlay mounts into on Start (the Shared PanelSettings).")]
        [SerializeField] private UIDocument _document;

        // Mount into the scene's own UIDocument (tests call Mount directly instead)
        private void Start()
        {
            if (_root == null && _document != null && _template != null && _zoom != null)
                Mount(_document);
        }

        // Follow the wall's zoom switches every frame (cheap: two bool reads)
        private void Update()
        {
            if (_root != null && _zoom != null)
                SetButtonsVisible(ShouldShowButtons(_zoom.Settings));
        }

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
            // - the clone's container must fill the panel: the strip docks to ITS bottom-right corner,
            //   and an empty container is 0 px tall (the strip once sat above the top of the screen)
            // - and let taps through everywhere except on the buttons themselves
            _root.style.flexGrow = 1f;
            _root.pickingMode = PickingMode.Ignore;
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

        // The visibility rule, pure: buttons only while zoom itself is enabled and wanted on screen
        public static bool ShouldShowButtons(ZoomSettings settings) =>
            settings != null && settings.enabled && settings.show_ui_buttons;
    }
}
