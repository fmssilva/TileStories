using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace TileStories
{
    // The POI Detail Card in a wall scene (_3.1 section 3): the ONE MonoBehaviour of the card, on the scene object
    // "PoiCard" with its own UIDocument (sort order 2, above the search UI). It listens to the selection bus, asks
    // the wall for the selected POI and the card settings, has BlockStackBuilder decide the blocks and hands them
    // to the sheet. Closing always goes through the bus (SelectionEventBus.Clear), so markers, list and minimap
    // follow: the X, a drag below peek, or a tap on empty camera space (CardTapRule).
    [RequireComponent(typeof(UIDocument))]
    public sealed class PoiCardHost : MonoBehaviour
    {
        [Tooltip("The wall whose POIs this card shows")]
        [SerializeField] private WallSession wallSession;

        [Tooltip("CardTokens.uss: the --ts-* design tokens")]
        [SerializeField] private StyleSheet tokens;

        [Tooltip("PoiCard.uss: the card's layout, written only with the tokens")]
        [SerializeField] private StyleSheet cardStyle;

        public PoiCardSheetView Sheet { get; private set; }
        // Where the card's blocks load media from (card_settings.media_resources_path), rebuilt when that changes
        public ResourcesMediaSource Media { get; private set; }
        // The POI the card shows (null while closed)
        public string ShownPoiId { get; private set; }

        private bool _subscribed;
        private Vector2 _pressPosition;
        private float _pressTime;
        private bool _pressed;

        // Wire to a wall (scene setup, tests)
        public void Bind(WallSession session)
        {
            Unsubscribe();
            wallSession = session;
            if (isActiveAndEnabled) Subscribe();
        }

        private void OnEnable() => Subscribe();

        private void OnDisable() => Unsubscribe();

        private void Subscribe()
        {
            if (_subscribed || wallSession == null) return;
            _subscribed = true;
            BuildOnce();
            SelectionEventBus.OnMarkerSelected += Show;
            SelectionEventBus.OnSelectionCleared += Close;
            wallSession.SearchDataChanged += OnWallDataChanged;
            if (SelectionEventBus.CurrentPoiId != null) Show(SelectionEventBus.CurrentPoiId);
        }

        private void Unsubscribe()
        {
            if (!_subscribed) return;
            _subscribed = false;
            SelectionEventBus.OnMarkerSelected -= Show;
            SelectionEventBus.OnSelectionCleared -= Close;
            if (wallSession != null) wallSession.SearchDataChanged -= OnWallDataChanged;
            Close();
        }

        private void BuildOnce()
        {
            if (Sheet != null) return;
            var root = GetComponent<UIDocument>().rootVisualElement;
            Sheet = new PoiCardSheetView(root, BlockRegistry.Shared, new[] { tokens, cardStyle });
            Sheet.CloseRequested += SelectionEventBus.Clear;
            Sheet.Layer.RegisterCallback<GeometryChangedEvent>(_ => SafeAreaHelper.ApplyAsOffsets(Sheet.Layer));
        }

        // Open (or rebind) the card for this POI
        private void Show(string poiId)
        {
            var settings = wallSession.CardSettings ?? new CardSettings();
            var poi = FindPoi(poiId);
            if (!settings.enabled || poi == null)
            {
                Close();
                return;
            }

            var stack = BlockStackBuilder.Build(poi, settings, BlockRegistry.Shared);
            if (Application.isEditor || Debug.isDebugBuild)
                foreach (var skipped in stack.Skipped)
                    Debug.LogWarning("[Card] " + poiId + ": block '" + skipped.Instance.key + "' (" + skipped.Instance.kind + ") not shown: " + skipped.Reason
                        + (skipped.FieldKey != null ? " '" + skipped.FieldKey + "'" : ""));

            string language = settings.languages != null && settings.languages.Count > 0 ? settings.languages[0] : "";
            if (Media == null || Media.Root != (settings.media_resources_path ?? "").Trim().Trim('/'))
                Media = new ResourcesMediaSource(settings.media_resources_path);
            var context = new BlockBindContext { Poi = poi, Taxonomy = wallSession.SearchConfig, Language = language, FallbackLanguage = language, Media = Media };
            Sheet.Show(stack.Entries, context, SheetStopRule.OpenStop(settings.container.open_stop), settings.container.half_max_ratio);
            ShownPoiId = poiId;
        }

        private void Close()
        {
            Sheet?.Hide();
            ShownPoiId = null;
        }

        // The wall's POI set was rebuilt (a live edit, a demo switched on): show the same POI's new data
        private void OnWallDataChanged()
        {
            if (ShownPoiId != null) Show(ShownPoiId);
        }

        // - a tap is judged on release: pressed and released in about the same place, quickly
        private void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null || Sheet == null) return;
            if (pointer.press.wasPressedThisFrame)
            {
                _pressed = true;
                _pressPosition = pointer.position.ReadValue();
                _pressTime = Time.unscaledTime;
            }
            else if (_pressed && pointer.press.wasReleasedThisFrame)
            {
                _pressed = false;
                HandleScreenTap(_pressPosition, pointer.position.ReadValue(), _pressTime, Time.unscaledTime);
            }
        }

        // A press at `down` released at `up` (screen pixels): close the card when it was a tap on empty camera
        // space. Returns true when it closed. The input poll above calls it; so can a test, with real positions.
        public bool HandleScreenTap(Vector2 down, Vector2 up, float downTime, float upTime)
        {
            var settings = wallSession != null ? wallSession.CardSettings : null;
            bool isTap = CardTapRule.IsTap(down, up, downTime, upTime, Screen.height);
            bool dismiss = CardTapRule.ShouldDismiss(Sheet != null && Sheet.IsOpen, settings?.container.dismiss_on_tap_outside ?? true,
                isTap, isTap && AnythingUnder(up));
            if (dismiss) SelectionEventBus.Clear();
            return dismiss;
        }

        // Whether anything the EventSystem raycasts (a marker, the card, the search UI) is under this screen point
        public static bool AnythingUnder(Vector2 screenPoint)
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null) return false;
            var hits = new List<RaycastResult>();
            eventSystem.RaycastAll(new PointerEventData(eventSystem) { position = screenPoint }, hits);
            return hits.Count > 0;
        }

        private POIData FindPoi(string id)
        {
            var pois = wallSession.SearchPois;
            for (int i = 0; i < pois.Count; i++)
                if (pois[i].id == id) return pois[i];
            return null;
        }
    }
}
