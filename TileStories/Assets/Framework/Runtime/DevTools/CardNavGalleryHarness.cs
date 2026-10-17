using UnityEngine;
using UnityEngine.InputSystem;

namespace TileStories
{
    // Phase A harness of the card's navigation elements (Assets/Dev/CardNavGallery, excluded from Build Settings): shows one
    // CardNavGalleryDefinitions entry at a time -- an element over a fixture card -- on the card gallery's real sheet, at Full.
    // It drives the CardGalleryHarness beside it (whose own block list is switched off in this scene); Left / Right step through
    // the entries in Play Mode, Space shows the entry again. CardNavGalleryTests drive Show(index) directly.
    [RequireComponent(typeof(CardGalleryHarness))]
    public sealed class CardNavGalleryHarness : MonoBehaviour
    {
        public CardGalleryHarness Gallery => _gallery != null ? _gallery : _gallery = GetComponent<CardGalleryHarness>();
        public int Index { get; private set; }
        public CardNavGalleryDefinitions.Entry Shown => CardNavGalleryDefinitions.All[Index];

        private CardGalleryHarness _gallery;

        private void Start() => Show(0);

        // Show entry `index` (wrapping) at the full stop, in its case's language
        public void Show(int index)
        {
            var entries = CardNavGalleryDefinitions.All;
            Index = (index % entries.Count + entries.Count) % entries.Count;
            var entry = entries[Index];
            Gallery.EnsureBuilt();
            Gallery.Language = entry.Case.Language;
            Gallery.ShowPoi(CardNavGalleryDefinitions.Poi(entry), CardNavGalleryDefinitions.Wall(entry), null, SheetStopRule.Stop.Full);
            // - a menu is judged open: it is what the header's menu button shows over the card (a card with no choice offers no button)
            if (CardNavGalleryDefinitions.IsMenu(entry)) Gallery.Sheet.Overlay.OpenMenu();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.rightArrowKey.wasPressedThisFrame) Show(Index + 1);
            if (keyboard.leftArrowKey.wasPressedThisFrame) Show(Index - 1);
            if (keyboard.spaceKey.wasPressedThisFrame) Show(Index);
        }
    }
}
