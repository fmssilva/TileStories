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
            Sheet.CloseRequested += Sheet.Hide;
            Show(0);
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
            // - the marker palettes the status block reads, configured exactly as a wall configures them
            MarkerVisualSettings.ApplyPalettes(wall);
            // - as on a real wall, the shown POI is one of the wall's POIs (wall_locator lays it among them)
            wall.pois.Add(poi);
            Sheet.Viewer = () => viewer;
            var stack = BlockStackBuilder.Build(poi, settings, BlockRegistry.Shared, wall.pois);
            var context = new BlockBindContext
            {
                Poi = poi, Taxonomy = wall, Language = "en", FallbackLanguage = "en",
                Strings = new CardStrings(strings != null ? strings.Entries() : null, settings.strings, "en", "en"),
                Glossary = new CardGlossary(settings.glossary, "en", "en"),
                MarkerLook = MarkerVisualSettings.Resolve(wall, null),
                Media = Media,
                State = State,
                Events = Events,
                Services = Services,
            };
            Sheet.Hide();
            Sheet.Show(stack.Entries, context, SheetStopRule.Stop.Peek, settings.container.half_max_ratio);
            Sheet.SetStop(stop);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || Sheet == null) return;
            if (keyboard.rightArrowKey.wasPressedThisFrame) Show(Index + 1);
            if (keyboard.leftArrowKey.wasPressedThisFrame) Show(Index - 1);
            if (keyboard.spaceKey.wasPressedThisFrame) Show(Index);
        }
    }
}
