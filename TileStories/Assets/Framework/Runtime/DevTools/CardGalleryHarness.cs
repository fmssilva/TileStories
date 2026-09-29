using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace TileStories
{
    // Phase A harness of the POI Detail Card (Assets/Dev/CardGallery, excluded from Build Settings): shows one
    // CardGalleryDefinitions entry at a time on the real PoiCardSheetView, inside a phone-width frame (the shared
    // PanelSettings' reference width). Left / Right arrows step through the entries in Play Mode; the X or a drag
    // below peek closes the card and Space reopens the entry. CardGalleryTests drive Show(index) directly.
    [RequireComponent(typeof(UIDocument))]
    public sealed class CardGalleryHarness : MonoBehaviour
    {
        [SerializeField] private StyleSheet tokens;
        [SerializeField] private StyleSheet cardStyle;
        [SerializeField] private StyleSheet[] blockStyles;
        [SerializeField] private CardStringTable strings;

        // The framework string table the gallery card reads (tests resolve default texts through it)
        public CardStringTable StringTable => strings;

        // The block stylesheets this harness adds (the same list as the wall scene's PoiCardHost; a test checks both)
        public System.Collections.Generic.IReadOnlyList<StyleSheet> BlockStyles => blockStyles ?? System.Array.Empty<StyleSheet>();

        public PoiCardSheetView Sheet { get; private set; }
        // The gallery's pictures, made in memory (no wall folder in Phase A); counted, so tests see what a card holds
        public CardGalleryMedia Media { get; } = new();
        public VisualElement Frame { get; private set; }
        public int Index { get; private set; }
        // What the gallery card remembers (answers, votes, revealed blocks): in memory, so a run never touches the developer's
        // saved answers; every entry is its own POI, so no two entries share an answer
        public MemoryCardStateStore StateStore { get; } = new();
        public CardLocalState State { get; }
        // Where the gallery card reports what the visitor did (a test hands its own to see the events)
        public ICardEvents Events { get; set; } = new LogCardEvents();
        // The services the gallery card's blocks may ask for: the app's shared registry, like the wall's card (a test hands its own)
        public CardServices Services { get; set; } = CardServices.Shared;
        // The app's own card words, like the wall's card (a test hands its own)
        public CardStringSources StringSources { get; set; } = CardStringSources.Shared;
        // The visitor's language for the next ShowPoi (Phase A shows one language at a time; English unless a test says otherwise)
        public string Language { get; set; } = "en";

        // The gallery card's audio (_3.1 step 9A): the SAME service, coordinator and mini-player as the wall's card, over a silent
        // ManualAudioOutput and a clock of its own. Audio time moves only through AdvanceAudio (a test) or, while AutoAdvanceAudio is on,
        // with real time (the developer looking at the gallery); every clip is a silent in-memory one (CardGalleryDefinitions.Clips)
        public ManualAudioOutput AudioOutput { get; } = new();
        public CardAudioService AudioService { get; private set; }
        public CardAudioCoordinator AudioCoordinator { get; private set; }
        // What starting an audio does while another plays (the wall's audio_when_another_starts)
        public string AudioMode { get; set; } = CardOptions.AudioSwitch;
        public bool AutoAdvanceAudio { get; set; } = true;
        private float _audioClock;
        private CardSettings _shownSettings = new();

        // The gallery card's video (_3.1 step 9B): the SAME service and sound coordinator as the wall's card, over a ManualVideoOutput that
        // decodes nothing. Video time moves only through AdvanceVideo (a test) or, while AutoAdvanceVideo is on, with real time; the clips are
        // the small generated files of CardGalleryDefinitions.Videos
        public ManualVideoOutput VideoOutput { get; } = new();
        public CardVideoService VideoService { get; private set; }
        public bool AutoAdvanceVideo { get; set; } = true;
        private CardSoundCoordinator _sound;

        // `seconds` of audio time pass: the output's clip moves on, the fade's clock too, and the service takes its step
        public void AdvanceAudio(float seconds)
        {
            _audioClock += seconds;
            AudioOutput.Advance(seconds);
            AudioService.Tick();
        }

        // `seconds` of video time pass: the output's clip moves on (its first frame arrives) and the service takes its step
        public void AdvanceVideo(float seconds)
        {
            VideoOutput.Advance(seconds);
            VideoService.Tick();
        }

        public CardGalleryHarness() => State = new CardLocalState(StateStore, "gallery");

        private void Start() => EnsureBuilt();

        // Build the frame and the sheet once (tests may call it before Start)
        public void EnsureBuilt()
        {
            if (Sheet != null) return;
            var document = GetComponent<UIDocument>();
            var root = document.rootVisualElement;
            // - a phone-width column in the middle: the panel's own reference width, whatever the Game view's shape
            Frame = new VisualElement { name = "card-gallery-frame", pickingMode = PickingMode.Ignore };
            Frame.style.position = Position.Absolute;
            Frame.style.top = 0;
            Frame.style.bottom = 0;
            Frame.style.width = document.panelSettings.referenceResolution.x;
            Frame.style.left = new Length(50, LengthUnit.Percent);
            Frame.style.translate = new Translate(new Length(-50, LengthUnit.Percent), 0);
            root.Add(Frame);
            Sheet = new PoiCardSheetView(Frame, BlockRegistry.Shared, PoiCardHost.CardStyleSheets(tokens, cardStyle, blockStyles));
            // - Phase A checks LAYOUT: the sheet jumps to its stop (no height animation for a measurement to race after a
            //   slow first frame); the motion itself is the real scene's to test (PoiCardSceneTests)
            Sheet.Root.style.transitionDuration = new StyleList<TimeValue>(new System.Collections.Generic.List<TimeValue> { new TimeValue(0f) });
            AudioService = new CardAudioService(AudioOutput, () => Media, () => _audioClock, () => AudioMode);
            AudioCoordinator = new CardAudioCoordinator(AudioService, new MiniPlayerView(Sheet.Layer, AudioService), () => _shownSettings, () => Sheet.IsOpen);
            // - the gallery has no wall to select a point on: the mini-player's tap shows the current entry again
            AudioCoordinator.Mini.OpenRequested += _ => Show(Index);
            VideoService = new CardVideoService(VideoOutput, () => Media);
            _sound = new CardSoundCoordinator(AudioService, VideoService);
            Sheet.CloseRequested += () =>
            {
                Sheet.Hide();
                AudioCoordinator.CardClosed();
                VideoService.CardClosed();
            };
            Show(0);
        }

        private void OnDestroy()
        {
            _sound?.Dispose();
            VideoService?.CardClosed();
            VideoOutput.Release();
        }

        // Show entry `index` at its own stop
        public void Show(int index)
        {
            EnsureBuilt();
            var entries = CardGalleryDefinitions.All;
            Index = (index % entries.Count + entries.Count) % entries.Count;
            var entry = entries[Index];
            var wall = CardGalleryDefinitions.Taxonomy();
            entry.WallSetup?.Invoke(wall);
            ShowPoi(CardGalleryDefinitions.Poi(entry), wall, entry.Viewer, entry.Stop);
        }

        // Show any POI of a fabricated wall at `stop` (an entry's own, or one a test builds: the gated question's card)
        public void ShowPoi(POIData poi, WallConfigData wall, Vector3? viewer, SheetStopRule.Stop stop)
        {
            EnsureBuilt();
            var settings = wall.card_settings;
            _shownSettings = settings;
            // - the marker palettes the status block reads, configured exactly as a wall configures them
            MarkerVisualSettings.ApplyPalettes(wall);
            // - as on a real wall, the shown POI is one of the wall's POIs (wall_locator lays it among them)
            wall.pois.Add(poi);
            Sheet.Viewer = () => viewer;
            var stack = BlockStackBuilder.Build(poi, settings, BlockRegistry.Shared, wall.pois);
            var context = new BlockBindContext
            {
                Poi = poi, Taxonomy = wall, Language = Language, FallbackLanguage = Language,
                Strings = new CardStrings(strings != null ? strings.Entries() : null, StringSources.Entries(), settings.strings, Language, Language),
                Glossary = new CardGlossary(settings.glossary, Language, Language),
                MarkerLook = MarkerVisualSettings.Resolve(wall, null),
                Media = Media,
                State = State,
                Events = Events,
                Services = Services,
                Audio = AudioService,
                Video = VideoService,
                ReduceMotion = settings.container.reduce_motion,
            };
            Sheet.Hide();
            Sheet.Show(stack.Entries, context, SheetStopRule.Stop.Peek, settings.container.half_max_ratio);
            Sheet.SetStop(stop);
            AudioCoordinator.CardShown(poi.id, context.Strings);
            VideoService.CardShown(poi.id);
        }

        private void Update()
        {
            if (AutoAdvanceAudio && AudioService != null) AdvanceAudio(Time.unscaledDeltaTime);
            if (AutoAdvanceVideo && VideoService != null) AdvanceVideo(Time.unscaledDeltaTime);
            var keyboard = Keyboard.current;
            if (keyboard == null || Sheet == null) return;
            if (keyboard.rightArrowKey.wasPressedThisFrame) Show(Index + 1);
            if (keyboard.leftArrowKey.wasPressedThisFrame) Show(Index - 1);
            if (keyboard.spaceKey.wasPressedThisFrame) Show(Index);
        }
    }
}
