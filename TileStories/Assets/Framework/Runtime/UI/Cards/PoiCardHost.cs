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

        [Tooltip("PoiCard.uss: the card's container, written only with the tokens")]
        [SerializeField] private StyleSheet cardStyle;

        [Tooltip("The blocks' looks, added after the container: CardParts.uss, then one Blocks/<Family>/<Family>.uss per family")]
        [SerializeField] private StyleSheet[] blockStyles;

        [Tooltip("The search UI of this scene: its top (bar, filter tray, view switch) steps aside while the card is at full")]
        [SerializeField] private SearchUIHost searchUI;

        [Tooltip("CardStrings.asset: the framework's default wording of the card's UI texts")]
        [SerializeField] private CardStringTable strings;

        public PoiCardSheetView Sheet { get; private set; }
        public CardStringTable StringTable => strings;
        // Where the card's blocks load media from (card_settings.media_resources_path), rebuilt when that changes
        public ResourcesMediaSource Media { get; private set; }
        // The POI the card shows (null while closed)
        public string ShownPoiId { get; private set; }

        // A tap on empty space waits out the zoom's double-tap window before it closes (TapOutsideDismissal)
        public bool ClosePending => _tapOutside.IsPending;

        private readonly TapOutsideDismissal _tapOutside = new();
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
            Sheet = new PoiCardSheetView(root, BlockRegistry.Shared, CardStyleSheets(tokens, cardStyle, blockStyles));
            Sheet.CloseRequested += SelectionEventBus.Clear;
            // - the full card would cover the search bar half-way (neither readable nor tappable): the bar steps aside
            Sheet.StopChanged += stop => { if (searchUI != null) searchUI.SetTopCoveredByCard(SheetStopRule.CoversScreenTop(stop)); };
            Sheet.Layer.RegisterCallback<GeometryChangedEvent>(_ => SafeAreaHelper.ApplyAsOffsets(Sheet.Layer));
        }

        // Every stylesheet of the card, in the order they apply: tokens, container, then the blocks' (shared by the
        // Phase A harness, so the gallery card and the wall's card are styled by exactly the same list)
        public static IEnumerable<StyleSheet> CardStyleSheets(StyleSheet tokens, StyleSheet container, IEnumerable<StyleSheet> blockStyles)
        {
            yield return tokens;
            yield return container;
            if (blockStyles == null) yield break;
            foreach (var sheet in blockStyles) yield return sheet;
        }

        // The block stylesheets this host adds (LivingRoomSceneWiringTests checks the list is complete)
        public IReadOnlyList<StyleSheet> BlockStyles => blockStyles ?? System.Array.Empty<StyleSheet>();

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

            _tapOutside.Cancel();
            var stack = BlockStackBuilder.Build(poi, settings, BlockRegistry.Shared, wallSession.SearchPois);
            if (Application.isEditor || Debug.isDebugBuild)
                foreach (var skipped in stack.Skipped)
                    Debug.LogWarning("[Card] " + poiId + ": block '" + skipped.Instance.key + "' (" + skipped.Instance.kind + ") not shown: " + skipped.Reason
                        + (skipped.FieldKey != null ? " '" + skipped.FieldKey + "'" : ""));

            string language = settings.languages != null && settings.languages.Count > 0 ? settings.languages[0] : "";
            if (Media == null || Media.Root != (settings.media_resources_path ?? "").Trim().Trim('/'))
                Media = new ResourcesMediaSource(settings.media_resources_path);
            var context = new BlockBindContext
            {
                Poi = poi, Taxonomy = wallSession.SearchConfig, Language = language, FallbackLanguage = language, Media = Media,
                Strings = new CardStrings(strings != null ? strings.Entries() : null, settings.strings, language, language),
                MarkerLook = wallSession.MarkerLook,
                Glossary = new CardGlossary(settings.glossary, language, language),
            };
            Sheet.Show(stack.Entries, context, SheetStopRule.OpenStop(settings.container.open_stop), settings.container.half_max_ratio);
            ShownPoiId = poiId;
        }

        private void Close()
        {
            _tapOutside.Cancel();
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
            if (Sheet == null) return;
            if (_tapOutside.TakeDue(Time.unscaledTime, TapOutsideDismissal.DelayFor(wallSession != null ? wallSession.ZoomSettings : null)))
                SelectionEventBus.Clear();
            var pointer = Pointer.current;
            if (pointer == null) return;
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

        // A press at `down` released at `up` (screen pixels, times on the Time.unscaledTime clock). A tap on empty
        // camera space closes the card -- at once while zoom is off, else once the zoom's double-tap window passes
        // with no second tap (ClosePending meanwhile). Returns true when it closed NOW. The input poll above calls
        // it; so can a test, with real positions and times.
        public bool HandleScreenTap(Vector2 down, Vector2 up, float downTime, float upTime)
        {
            var settings = wallSession != null ? wallSession.CardSettings : null;
            var zoom = wallSession != null ? wallSession.ZoomSettings : null;
            bool isTap = CardTapRule.IsTap(down, up, downTime, upTime, Screen.height);
            bool closes = CardTapRule.ShouldDismiss(Sheet != null && Sheet.IsOpen, settings?.container.dismiss_on_tap_outside ?? true,
                isTap, isTap && AnythingUnder(up));
            float window = TapOutsideDismissal.DelayFor(zoom);
            _tapOutside.OnTap(upTime, up, closes, window, zoom?.double_tap_move_tolerance_px ?? 0f);
            // - zoom off: no second tap can make it a double tap, so there is nothing to wait for
            if (!_tapOutside.TakeDue(upTime, window)) return false;
            SelectionEventBus.Clear();
            return true;
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
