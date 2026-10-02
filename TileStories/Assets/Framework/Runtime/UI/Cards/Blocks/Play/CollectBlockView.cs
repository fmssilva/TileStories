using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The collect block (_3.1 Tier 3, step 8B; look add_to_story): this point's collectable item -- a stamp with the series it
    // belongs to and its name -- and one button, Add to my story. Collecting is remembered through the card's CardLocalState (the
    // block opens showing it as collected) and reported ONCE as a collect event through ICardEvents. Under it: how many of THIS
    // WALL's collectable items the visitor has ("3 of 12"), counted from the wall's config (CollectRule.Items: every collect
    // block on every point of the wall), never a number written into the code. The stamp is a ring holding a star: outlined
    // while the item is not yet collected, filled once it is (the shape says it, not only the colour). Only classes here;
    // Play.uss draws it.
    public sealed class CollectBlockView : IBlockView
    {
        public VisualElement Root { get; }
        public VisualElement Tile { get; }
        public VisualElement Stamp { get; }
        // The star inside the stamp: outlined = not collected, filled = collected
        public CardIcons.VectorGlyph StampStar { get; }
        public Label Series { get; }
        public Label ItemName { get; }
        public Button Add { get; }
        public Label AddLabel { get; }
        public CardIcons.VectorGlyph AddMark { get; }
        public Label Progress { get; }
        public VisualElement ProgressTrack { get; }
        public VisualElement ProgressFill { get; }

        // Whether this point's item is in the visitor's story
        public bool IsCollected { get; private set; }

        // The visitor's count and the wall's total, as last shown
        public int Have { get; private set; }
        public int Total { get; private set; }

        private CardStrings _strings;
        private CardLocalState _state;
        private ICardEvents _events;
        private System.Collections.Generic.IReadOnlyList<CollectRule.Item> _items = System.Array.Empty<CollectRule.Item>();
        private string _wallId = "";
        private string _poiId = "";
        private string _blockKey = "";
        private string _variant = "";
        private string _variantClass;
        // Collected items when the card has no state store to keep them in (the view then holds them until it is unbound)
        private readonly System.Collections.Generic.HashSet<string> _memory = new();

        public CollectBlockView()
        {
            Root = new VisualElement { name = "card-collect" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-collect");
            Tile = new VisualElement();
            Tile.AddToClassList("card-collect__tile");
            Stamp = new VisualElement { pickingMode = PickingMode.Ignore };
            Stamp.AddToClassList("card-collect__stamp");
            StampStar = CardIcons.CreateVector(CardIcons.Shape.Star);
            StampStar.AddToClassList("card-collect__star");
            Stamp.Add(StampStar);
            var words = new VisualElement();
            words.AddToClassList("card-collect__words");
            Series = new Label();
            Series.AddToClassList("card-collect__series");
            ItemName = new Label();
            ItemName.AddToClassList("card-collect__item");
            words.Add(Series);
            words.Add(ItemName);
            Tile.Add(Stamp);
            Tile.Add(words);

            Add = new Button(Collect) { name = "card-collect-add" };
            Add.AddToClassList("card-collect__add");
            Add.AddToClassList("card-pill");
            Add.AddToClassList("card-tap");
            AddMark = CardIcons.CreateVector(CardIcons.Shape.Tick);
            AddMark.AddToClassList("card-collect__add-mark");
            AddLabel = new Label { pickingMode = PickingMode.Ignore };
            AddLabel.AddToClassList("card-collect__add-label");
            Add.Add(AddMark);
            Add.Add(AddLabel);

            Progress = new Label();
            Progress.AddToClassList("card-collect__progress");
            ProgressTrack = new VisualElement { pickingMode = PickingMode.Ignore };
            ProgressTrack.AddToClassList("card-collect__track");
            ProgressFill = new VisualElement { pickingMode = PickingMode.Ignore };
            ProgressFill.AddToClassList("card-collect__fill");
            ProgressTrack.Add(ProgressFill);

            Root.Add(Tile);
            Root.Add(Add);
            Root.Add(Progress);
            Root.Add(ProgressTrack);
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _variant = context.Variant;
            _variantClass = "card-collect--" + _variant;
            Root.AddToClassList(_variantClass);
            _strings = context.Strings;
            _state = context.State;
            _events = context.Events;
            _wallId = context.State?.WallId ?? context.Taxonomy?.wall_id ?? "";
            _poiId = context.Poi?.id ?? "";
            _blockKey = instance.key ?? "";
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            string series = CollectRule.Series(read, context.Poi, context.Taxonomy);
            Series.text = series;
            Series.style.display = series.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            ItemName.text = CollectRule.ItemName(read, context.Poi, context.Language, context.FallbackLanguage);
            // - the wall's collectable items come from its config: every collect block of every point
            _items = CollectRule.Items(context.Taxonomy?.pois, context.Settings);
            Add.tooltip = ItemName.text;
            Refresh();
        }

        public void Unbind()
        {
            _state = null;
            _events = null;
            _items = System.Array.Empty<CollectRule.Item>();
            _memory.Clear();
            IsCollected = false;
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        // Add this point's item to the visitor's story (once): remember it, tell the events sink once, show it collected
        public void Collect()
        {
            if (IsCollected) return;
            if (_state != null) _state.SetCollected(_poiId, _blockKey);
            else _memory.Add(_poiId + "/" + _blockKey);
            _events?.Raise(new CardEvent(CardEventKinds.Collect, _wallId, _poiId, _blockKey, _variant, "collected"));
            Refresh();
        }

        private bool Has(string poiId, string blockKey) =>
            _state != null ? _state.Collected(poiId, blockKey) : _memory.Contains(poiId + "/" + blockKey);

        // The block as it stands: the stamp and the button in their state, and the count read again
        private void Refresh()
        {
            IsCollected = Has(_poiId, _blockKey);
            Root.EnableInClassList("card-collect--collected", IsCollected);
            StampStar.Filled = IsCollected;
            AddLabel.text = _strings?.Get(IsCollected ? CardStrings.Keys.CollectCollected : CardStrings.Keys.CollectAdd) ?? "";
            AddMark.style.display = IsCollected ? DisplayStyle.Flex : DisplayStyle.None;
            (Have, Total) = CollectRule.Progress(_items, _poiId, _blockKey, Has);
            string format = _strings?.Get(CardStrings.Keys.CollectProgress) ?? "";
            Progress.text = string.Format(System.Globalization.CultureInfo.InvariantCulture, format, Have, Total);
            ProgressFill.style.width = new Length(Total > 0 ? 100f * Have / Total : 0f, LengthUnit.Percent);
        }
    }
}
