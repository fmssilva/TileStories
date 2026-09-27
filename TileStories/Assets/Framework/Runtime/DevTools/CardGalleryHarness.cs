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
        [SerializeField] private CardStringTable strings;

        public PoiCardSheetView Sheet { get; private set; }
        public VisualElement Frame { get; private set; }
        public int Index { get; private set; }

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
            Sheet = new PoiCardSheetView(Frame, BlockRegistry.Shared, new[] { tokens, cardStyle });
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
            var settings = wall.card_settings;
            // - the marker palettes the status block reads, configured exactly as a wall configures them
            MarkerVisualSettings.ApplyPalettes(wall);
            var poi = CardGalleryDefinitions.Poi(entry);
            var stack = BlockStackBuilder.Build(poi, settings, BlockRegistry.Shared);
            var context = new BlockBindContext
            {
                Poi = poi, Taxonomy = wall, Language = "en", FallbackLanguage = "en",
                Strings = new CardStrings(strings != null ? strings.Entries() : null, settings.strings, "en", "en"),
                Glossary = new CardGlossary(settings.glossary, "en", "en"),
                MarkerLook = MarkerVisualSettings.Resolve(wall, null),
            };
            Sheet.Hide();
            Sheet.Show(stack.Entries, context, SheetStopRule.Stop.Peek, settings.container.half_max_ratio);
            Sheet.SetStop(entry.Stop);
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
