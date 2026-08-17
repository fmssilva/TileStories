using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // Minimal proof-of-life detail card (spec _2.6 section 14): a small centered
    // panel showing the selected POI's name plus a close (X) button. Shows on
    // selection (marker / minimap / list taps all raise the same SelectionEventBus
    // event) and hides on close or clear. Deliberately scoped: no blocks, scroll
    // view, spring-up animation or safe-area handling -- that is future-domain
    // work in the Blocks/Cards system. This card only proves selection -> detail.
    public class DetailCardView : MonoBehaviour
    {
        private WallConfigData _config;
        private UIDocument _uiDocument;
        private VisualElement _panel;
        private Label _nameLabel;
        private Button _closeButton;

        // Selection state, kept separate from the UI label so the behaviour is
        // testable in EditMode without a UIDocument/scene.
        private bool _visible;
        private string _selectedName = string.Empty;

        // The panel root is shared with other UI Toolkit views (they each add
        // children to the UIDocument root). Exposed for tests.
        public bool IsVisibleState() => _visible;
        public string GetLabelText() => _selectedName;

        public void Initialize(WallConfigData config, UIDocument uiDocument = null)
        {
            _config = config;

            // One selection system, two surfaces: react to the same bus the
            // markers / minimap / list publish to.
            SelectionEventBus.OnMarkerSelected += OnMarkerSelected;
            SelectionEventBus.OnSelectionCleared += OnSelectionCleared;

            _uiDocument = uiDocument != null ? uiDocument : FindFirstObjectByType<UIDocument>();
            if (_uiDocument != null && _panel == null)
                CreateUI(_uiDocument.rootVisualElement);

            // Start hidden; shown when a POI is selected.
            SetVisible(false);
        }

        private void CreateUI(VisualElement root)
        {
            _panel = new VisualElement
            {
                name = "detail-card-panel"
            };
            _panel.style.position = Position.Absolute;
            _panel.style.left = 24;
            _panel.style.right = 24;
            _panel.style.bottom = 24;
            _panel.style.height = 160; // <=40% screen height cap (spec §14)
            _panel.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0.85f));
            _panel.style.borderTopLeftRadius = 12;
            _panel.style.borderTopRightRadius = 12;
            _panel.style.borderBottomLeftRadius = 12;
            _panel.style.borderBottomRightRadius = 12;
            _panel.style.paddingLeft = 16;
            _panel.style.paddingRight = 16;
            _panel.style.paddingTop = 12;
            _panel.style.paddingBottom = 12;
            root.Add(_panel);

            _nameLabel = new Label
            {
                name = "detail-card-label",
                text = string.Empty
            };
            _nameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _nameLabel.style.color = new StyleColor(Color.white);
            _nameLabel.style.fontSize = 16;
            _panel.Add(_nameLabel);

            _closeButton = new Button(OnCloseClicked)
            {
                name = "detail-card-close",
                text = "X"
            };
            _closeButton.style.position = Position.Absolute;
            _closeButton.style.top = 6;
            _closeButton.style.right = 6;
            _closeButton.style.width = 28;
            _closeButton.style.height = 28;
            _panel.Add(_closeButton);
        }

        // Raised by marker / minimap / list taps. Resolve the name via config so
        // the card is the only consumer that needs the POI roster.
        private void OnMarkerSelected(string poiId)
        {
            POIData poi = FindPoi(poiId);
            if (poi == null)
                return;

            _selectedName = poi.name;
            if (_nameLabel != null)
                _nameLabel.text = _selectedName;

            SetVisible(true);
        }

        private POIData FindPoi(string poiId) =>
            _config?.pois?.FirstOrDefault(p => p.id == poiId);

        // X button -> clear selection (the shared path restores full marker opacity
        // + minimap/list highlights via their own SelectionEventBus handlers).
        private void OnCloseClicked()
        {
            SelectionEventBus.RaiseSelectionCleared();
        }

        // Public so tests can drive the close path directly.
        public void Close() => OnCloseClicked();

        private void OnSelectionCleared()
        {
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            _visible = visible;
            if (_panel != null)
                _panel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnDestroy()
        {
            SelectionEventBus.OnMarkerSelected -= OnMarkerSelected;
            SelectionEventBus.OnSelectionCleared -= OnSelectionCleared;
        }
    }
}
