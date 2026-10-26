using System;
using UnityEngine.UIElements;

namespace TileStories
{
    // The placed-model chip (_3.2 section 5.4, step 9; the two AR TODOs _3.1 left): while a model placed with place_in_ar stands in the room,
    // a small chip floats above the card (in its FloatingSlot) at EVERY stop -- the visitor knows the model is there even at peek, where the
    // block's own "Placed by the wall" line is out of sight. It mirrors the audio mini-player (MiniPlayerView): it holds no placement state,
    // draws what the ONE AR placement owner (ICardArPlacement) says, redraws on its Changed, and its button asks the owner. What it says is
    // PlacedModelChipRule's:
    //   - on the card that placed the model: "Placed by the wall" and Remove (the owner takes the model away)
    //   - on another point's card (Keep Model On Switch kept it): "<that point> is placed by the wall" and NO Remove -- only the card that
    //     placed it offers that; a tap on the words opens that card (through the selection bus, like a marker tap)
    // It is bound with the open card's context and let go when the card closes (Unbind: it stops listening to the owner). Only classes here;
    // PoiCard.uss draws it with the card tokens.
    public sealed class PlacedModelChipView
    {
        public VisualElement Root { get; }
        // The chip's words: a button on another point's card (it opens the card that placed the model), inert on the placing card
        public Button Words { get; }
        public Label Text { get; }
        public Button RemoveButton { get; }
        public PlacedModelChipRule.State State { get; private set; } = PlacedModelChipRule.State.Hidden;

        public bool IsShown => State != PlacedModelChipRule.State.Hidden;

        // Raised after every redraw (the floating slot shows or hides with the chip)
        public event Action Redrawn;

        private ICardArPlacement _placement;
        private string _control;
        private string _cardPoiId;
        private CardStrings _strings;
        private IBlockHost _host;

        public PlacedModelChipView()
        {
            Root = new VisualElement { name = "poi-card-chip" };
            Root.AddToClassList("poi-card-chip");
            Words = new Button(OpenThePlacingCard) { name = "poi-card-chip-words" };
            Words.AddToClassList("poi-card-chip__words");
            Text = new Label { pickingMode = PickingMode.Ignore };
            Text.AddToClassList("poi-card-chip__text");
            Words.Add(Text);
            RemoveButton = new Button(Remove) { name = "poi-card-chip-remove" };
            RemoveButton.AddToClassList("poi-card-chip__remove");
            var removeWords = new Label { pickingMode = PickingMode.Ignore };
            removeWords.AddToClassList("poi-card-chip__remove-label");
            RemoveButton.Add(removeWords);
            Root.Add(Words);
            Root.Add(RemoveButton);
            Root.style.display = DisplayStyle.None;
        }

        // The open card's point, its words and the owner it draws (context.ArPlacement; none: the chip stays hidden)
        public void Bind(BlockBindContext context)
        {
            Unbind();
            _placement = context?.ArPlacement;
            _control = context?.Settings?.navigation?.placed_model_control;
            _cardPoiId = context?.Poi?.id;
            _strings = context?.Strings;
            _host = context?.Host;
            if (_placement != null) _placement.Changed += Refresh;
            Refresh();
        }

        // The card closed: stop listening to the owner and hide
        public void Unbind()
        {
            if (_placement != null) _placement.Changed -= Refresh;
            _placement = null;
            _cardPoiId = null;
            _host = null;
            Refresh();
        }

        // Draw what the owner says now
        public void Refresh()
        {
            var placed = _placement?.PlacedPoi;
            State = PlacedModelChipRule.Of(_control, _placement != null, placed?.id, _cardPoiId);
            Root.style.display = IsShown ? DisplayStyle.Flex : DisplayStyle.None;
            if (IsShown)
            {
                bool here = State == PlacedModelChipRule.State.ThisCard;
                Text.text = here
                    ? _strings?.Get(CardStrings.Keys.PlaceInArPlaced) ?? ""
                    : string.Format(_strings?.Get(CardStrings.Keys.PlacedModelOther) ?? "{0}", NameOf(placed));
                Words.tooltip = Text.text;
                // - the words are a way to the placing card only from another card; on the placing card they are just words
                Words.pickingMode = here ? PickingMode.Ignore : PickingMode.Position;
                Words.EnableInClassList("poi-card-chip__words--link", !here);
                RemoveButton.style.display = here ? DisplayStyle.Flex : DisplayStyle.None;
                string remove = _strings?.Get(CardStrings.Keys.PlacedModelRemove) ?? "";
                RemoveButton.Q<Label>().text = remove;
                RemoveButton.tooltip = remove;
            }
            Redrawn?.Invoke();
        }

        // Remove: the owner takes the model away (the chip hears its Changed and hides)
        public void Remove()
        {
            if (State == PlacedModelChipRule.State.ThisCard) _placement?.Remove();
        }

        // A tap on the words on another point's card: open the card that placed the model, where Remove is
        public void OpenThePlacingCard()
        {
            var placed = _placement?.PlacedPoi;
            if (State == PlacedModelChipRule.State.OtherCard && placed != null) _host?.SelectPoi(placed.id);
        }

        // The name a visitor knows the point by (its id only when it has none)
        private static string NameOf(POIData poi) => poi == null ? "" : string.IsNullOrWhiteSpace(poi.name) ? poi.id : poi.name;
    }
}
