using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace TileStories
{
    // The POI Detail Card in a wall scene (_3.1 section 3): the ONE MonoBehaviour of the card, on the scene object
    // "PoiCard" with its own UIDocument (sort order 2, above the search UI). It listens to the selection bus, asks
    // the wall for the selected POI and the card settings, has BlockStackBuilder decide the blocks and CardContextBuilder
    // what they are bound with, and hands both to the sheet. Closing always goes through the bus (SelectionEventBus.Clear),
    // so markers, list and minimap follow: the X, a drag below peek, or a tap on empty camera space (CardTapRule).
    // The audio / video / preview / AR owners live in CardOwners; this class only wires them.
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
        public bool ClosePending => _tapOutside?.IsPending ?? false;

        // The card's owners (_3.1 steps 9A, 9B, 10A, 10B): one each, built over this object's players / stage and this wall
        private CardOwners _owners;
        public CardAudioCoordinator AudioCoordinator => _owners?.AudioCoordinator;
        public ICardAudio Audio => _owners?.Audio;
        public CardVideoService VideoService => _owners?.Video;
        public ICardVideo Video => VideoService;
        public CardPreviewService PreviewService => _owners?.Preview;
        public ICardPreview Preview => PreviewService;
        public ArPlacementService ArPlacementService => _owners?.ArPlacement;
        public ICardArPlacement ArPlacement => ArPlacementService;

        // The "tap on empty camera space closes the card" path (CardTapOutside) and the developer-only demo card (CardDemoOpener)
        private CardTapOutside _tapOutside;
        private readonly CardDemoOpener _demo = new();

        // What the visitor did on this wall's cards (answers, votes, revealed blocks, the language picked): PlayerPrefs, scoped by the
        // wall's id. A test hands its own (over a MemoryCardStateStore) so a run never touches the developer's saved answers
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
            _demo.Forget();
            Close();
            _owners?.SwitchedOff();
        }

        private void BuildOnce()
        {
            if (Sheet != null) return;
            var root = GetComponent<UIDocument>().rootVisualElement;
            Sheet = new PoiCardSheetView(root, BlockRegistry.Shared, CardStyleSheets.InOrder(tokens, cardStyle, blockStyles));
            Sheet.CloseRequested += SelectionEventBus.Clear;
            Sheet.LanguageRequested += SwitchLanguage;
            Sheet.Viewer = ViewerOnTheWall;
            // - the full card would cover the search bar half-way (neither readable nor tappable): the bar steps aside
            Sheet.StopChanged += stop => { if (searchUI != null) searchUI.SetTopCoveredByCard(SheetStopRule.CoversScreenTop(stop)); };
            Sheet.Layer.RegisterCallback<GeometryChangedEvent>(_ => SafeAreaHelper.ApplyAsOffsets(Sheet.Layer));
            _tapOutside = new CardTapOutside(() => Clock(), () => wallSession != null ? wallSession.CardSettings : null,
                () => wallSession != null ? wallSession.ZoomSettings : null, () => Sheet != null && Sheet.IsOpen, SelectionEventBus.Clear);
            _owners = CardOwners.ForWall(gameObject, wallSession, Sheet.Layer, () => Media, () => Clock(), () => Sheet != null && Sheet.IsOpen, OpenAudiosCard);
        }

        // The owner swaps (the app's own are made in BuildOnce; a test hands one over a manual device: ManualAudioOutput,
        // ManualVideoOutput, ManualPreviewStage, ManualArWall): the old owner lets go first (CardOwners)
        internal void UseAudioService(CardAudioService service) => _owners.UseAudio(service);
        internal void UseVideoService(CardVideoService service) => _owners.UseVideo(service);
        internal void UsePreviewService(CardPreviewService service) => _owners.UsePreview(service);
        internal void UseArPlacementService(ArPlacementService service) => _owners.UseArPlacement(service);

        // - the host going away: nothing it placed stays in the world, nothing listens to the tracker
        private void OnDestroy() => _owners?.Dispose();

        // The mini-player's tap: show that point's card again through the bus, like a tap on its marker (never on the point already
        // selected: selecting it again would clear it)
        private void OpenAudiosCard(string poiId)
        {
            if (SelectionEventBus.CurrentPoiId != poiId) SelectionEventBus.Select(poiId);
        }

        // The camera's place in the wall's frame (where the POI positions live: the markers' spawn root), or null
        private Vector3? ViewerOnTheWall()
        {
            var cam = Camera.main;
            if (cam == null || wallSession == null) return null;
            return wallSession.MarkerSpawnRoot.InverseTransformPoint(cam.transform.position);
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

            _tapOutside?.Cancel();
            _owners.PointChosen(poiId);
            var stack = BlockStackBuilder.Build(poi, settings, BlockRegistry.Shared, wallSession.SearchPois);
            if (Application.isEditor || Debug.isDebugBuild)
                foreach (var skipped in stack.Skipped) Debug.LogWarning(CardContextBuilder.SkipWarning(poiId, skipped));

            var (language, _) = CardContextBuilder.Languages(settings, StateOfThisWall().Language(), CardDemoRule.IsAllowed(Application.isEditor, Debug.isDebugBuild));
            if (CardContextBuilder.NeedsMediaSource(Media?.Root, settings)) Media = new ResourcesMediaSource(settings.media_resources_path);
            // - re-resolved every Show so a live edit to the wall's own default library takes effect at once
            Media.FrameworkDefaults = CardMediaLibraryLookup.Framework;
            Media.WallDefaults = CardMediaLibraryLookup.WallFrom(settings.default_media_library_resources_path);
            var hostParts = new BlockBindContext
            {
                Media = Media, MarkerLook = wallSession.MarkerLook, State = StateOfThisWall(), Events = Events, Services = Services,
                Audio = Audio, Video = Video, Preview = Preview, ArPlacement = ArPlacement,
            };
            var context = CardContextBuilder.Build(hostParts, poi, wallSession.SearchConfig, settings, language,
                strings != null ? strings.Entries() : null, StringSources.Entries());
            Sheet.Show(stack.Entries, context, SheetStopRule.OpenStop(settings.container.open_stop), settings.container.half_max_ratio);
            ShownPoiId = poiId;
            _owners.CardShown(poiId, context.Strings);
        }

        // The visitor tapped the language chip: remember the pick for this wall on this device and show the open card again in it
        // (Rebind keeps the sheet's stop and the stack's scroll)
        private void SwitchLanguage(string code)
        {
            StateOfThisWall().SetLanguage(code);
            Rebind();
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
            _tapOutside?.Cancel();
            Sheet?.Hide();
            ShownPoiId = null;
            _owners?.CardClosed();
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

        // The developer-only demo card gets its turn (CardDemoOpener: off by default, never in a release build)
        private void ApplyDemo() => _demo.Apply(wallSession.CardSettings, wallSession.SearchPois,
            CardDemoRule.IsAllowed(Application.isEditor, Debug.isDebugBuild), ShownPoiId, Show, Sheet);

        // - every frame the tap-outside path sees the pointer (a press, its release, a waiting close falling due)
        private void Update()
        {
            if (Sheet != null) _tapOutside.Poll(Pointer.current);
        }

        // A press at `down` released at `up` (screen pixels, times on the Clock): true when a tap on empty camera space closed the card NOW
        // (CardTapOutside). The input poll calls it; so can a test, with real positions and times.
        public bool HandleScreenTap(Vector2 down, Vector2 up, float downTime, float upTime) => _tapOutside.HandleScreenTap(down, up, downTime, upTime);

        private POIData FindPoi(string id)
        {
            var pois = wallSession.SearchPois;
            for (int i = 0; i < pois.Count; i++)
                if (pois[i].id == id) return pois[i];
            return null;
        }
    }
}
