using System.Collections.Generic;
using UnityEngine;
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

        // What the visitor did on this wall's cards (answers, votes, revealed blocks): PlayerPrefs, scoped by the wall's id. A
        // test hands its own (over a MemoryCardStateStore) so a run never touches the developer's saved answers
        internal CardLocalState State { get; set; }

        // Where the cards report what the visitor did (feedback): a log line until the telemetry work replaces it
        internal ICardEvents Events { get; set; } = new LogCardEvents();

        // The services every block on this card may ask for (BlockBindContext.Service<T>): the app's shared registry, which an app fills
        // at startup. No poll backend is registered today, so a poll shows no results (8B). A test hands its own registry
        internal CardServices Services { get; set; } = CardServices.Shared;

        // The visitor words the app added for the kinds it ships (an app registers its table at startup, like its kinds). A test hands its own
        internal CardStringSources StringSources { get; set; } = CardStringSources.Shared;

        // The tap path's clock (press and release times, a pending close falling due): Time.unscaledTime in the app. A test
        // sets its own, so its taps' times are what it says -- no slow frame can decide whether two taps fell in one window
        internal System.Func<float> Clock { get; set; } = () => Time.unscaledTime;
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
            wallSession.CardSettingsChanged += OnWallDataChanged;
            if (SelectionEventBus.CurrentPoiId != null) Show(SelectionEventBus.CurrentPoiId);
            ApplyDemo();
        }

        private void Unsubscribe()
        {
            if (!_subscribed) return;
            _subscribed = false;
            SelectionEventBus.OnMarkerSelected -= Show;
            SelectionEventBus.OnSelectionCleared -= Close;
            if (wallSession != null)
            {
                wallSession.SearchDataChanged -= OnWallDataChanged;
                wallSession.CardSettingsChanged -= OnWallDataChanged;
            }
            _demoRequest = "";
            Close();
        }

        private void BuildOnce()
        {
            if (Sheet != null) return;
            var root = GetComponent<UIDocument>().rootVisualElement;
            Sheet = new PoiCardSheetView(root, BlockRegistry.Shared, CardStyleSheets(tokens, cardStyle, blockStyles));
            Sheet.CloseRequested += SelectionEventBus.Clear;
            Sheet.Viewer = ViewerOnTheWall;
            // - the full card would cover the search bar half-way (neither readable nor tappable): the bar steps aside
            Sheet.StopChanged += stop => { if (searchUI != null) searchUI.SetTopCoveredByCard(SheetStopRule.CoversScreenTop(stop)); };
            Sheet.Layer.RegisterCallback<GeometryChangedEvent>(_ => SafeAreaHelper.ApplyAsOffsets(Sheet.Layer));
        }

        // The camera's place in the wall's frame (where the POI positions live: the markers' spawn root), or null
        private Vector3? ViewerOnTheWall()
        {
            var cam = Camera.main;
            if (cam == null || wallSession == null) return null;
            return wallSession.MarkerSpawnRoot.InverseTransformPoint(cam.transform.position);
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
                Strings = new CardStrings(strings != null ? strings.Entries() : null, StringSources.Entries(), settings.strings, language, language),
                MarkerLook = wallSession.MarkerLook,
                Glossary = new CardGlossary(settings.glossary, language, language),
                State = StateOfThisWall(),
                Events = Events,
                Services = Services,
            };
            Sheet.Show(stack.Entries, context, SheetStopRule.OpenStop(settings.container.open_stop), settings.container.half_max_ratio);
            ShownPoiId = poiId;
        }

        // The card state of the wall this host shows (rebuilt when the host is bound to a wall with another id)
        private CardLocalState StateOfThisWall()
        {
            string wallId = wallSession.SearchConfig?.wall_id ?? "";
            if (State == null || State.WallId != wallId) State = new CardLocalState(new PlayerPrefsCardStateStore(), wallId);
            return State;
        }

        private void Close()
        {
            _tapOutside.Cancel();
            Sheet?.Hide();
            ShownPoiId = null;
        }

        // The wall's POI set or the card's own settings changed (a live edit, a demo switched on): the open card shows the new data, and the
        // developer-only demo card gets its turn
        private void OnWallDataChanged()
        {
            Rebind();
            ApplyDemo();
        }

        // Show the open card's POI again with the data as it is now, keeping the sheet's stop and the stack's scroll (a live edit must not throw the
        // reader back to the top). Does nothing while the card is closed.
        public void Rebind()
        {
            if (ShownPoiId == null || Sheet == null) return;
            var scroll = Sheet.Stack.Scroll;
            var offset = scroll.scrollOffset;
            Show(ShownPoiId);
            // - a new stack starts at the top and has a new height: ask for the old offset now (clamped to what fits), and once more after the
            //   layout pass that measures the new content
            scroll.scrollOffset = offset;
            scroll.schedule.Execute(() => scroll.scrollOffset = offset);
        }

        // The demo request last acted on (CardDemoRule.Request): the card opens again only when it changes
        private string _demoRequest = "";

        // Developer-only demo card (card_settings.demo_card): when its switch is ON, this build allows it and its POI is on the wall, that POI's
        // card opens by itself at the chosen stop. Off by default; nothing happens in a release build.
        private void ApplyDemo()
        {
            var demo = wallSession.CardSettings?.demo_card;
            string request = CardDemoRule.Request(demo, CardDemoRule.IsAllowed(Application.isEditor, Debug.isDebugBuild), wallSession.SearchPois);
            if (request == _demoRequest) return;
            _demoRequest = request;
            if (request.Length == 0) return;
            // - selecting the POI that is already selected would CLEAR it (tap again to deselect): only select another one
            if (SelectionEventBus.CurrentPoiId != demo.poi_id) SelectionEventBus.Select(demo.poi_id);
            else if (ShownPoiId != demo.poi_id) Show(demo.poi_id);
            if (Sheet != null && Sheet.IsOpen) Sheet.SetStop(CardDemoRule.StopOf(demo));
        }

        // - a tap is judged on release: pressed and released in about the same place, quickly
        private void Update()
        {
            if (Sheet == null) return;
            if (_tapOutside.TakeDue(Clock(), TapOutsideDismissal.DelayFor(wallSession != null ? wallSession.ZoomSettings : null)))
                SelectionEventBus.Clear();
            var pointer = Pointer.current;
            if (pointer == null) return;
            if (pointer.press.wasPressedThisFrame)
            {
                _pressed = true;
                _pressPosition = pointer.position.ReadValue();
                _pressTime = Clock();
            }
            else if (_pressed && pointer.press.wasReleasedThisFrame)
            {
                _pressed = false;
                HandleScreenTap(_pressPosition, pointer.position.ReadValue(), _pressTime, Clock());
            }
        }

        // A press at `down` released at `up` (screen pixels, times on the Clock). A tap on empty
        // camera space closes the card -- at once while zoom is off, else once the zoom's double-tap window passes
        // with no second tap (ClosePending meanwhile). Returns true when it closed NOW. The input poll above calls
        // it; so can a test, with real positions and times.
        public bool HandleScreenTap(Vector2 down, Vector2 up, float downTime, float upTime)
        {
            var settings = wallSession != null ? wallSession.CardSettings : null;
            var zoom = wallSession != null ? wallSession.ZoomSettings : null;
            bool isTap = CardTapRule.IsTap(down, up, downTime, upTime, Screen.height);
            bool closes = CardTapRule.ShouldDismiss(Sheet != null && Sheet.IsOpen, settings?.container.dismiss_on_tap_outside ?? true,
                isTap, isTap && ScreenUIHit.IsOverAnything(up));
            float window = TapOutsideDismissal.DelayFor(zoom);
            _tapOutside.OnTap(upTime, up, closes, window, zoom?.double_tap_move_tolerance_px ?? 0f);
            // - zoom off: no second tap can make it a double tap, so there is nothing to wait for
            if (!_tapOutside.TakeDue(upTime, window)) return false;
            SelectionEventBus.Clear();
            return true;
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
