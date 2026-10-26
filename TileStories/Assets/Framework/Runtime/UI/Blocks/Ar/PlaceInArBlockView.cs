using UnityEngine.UIElements;

namespace TileStories
{
    // The place_in_ar block (_3.1 Tier 5, step 10B.2, placed state 15.2.3): ONE button that asks the card's AR placement owner (ICardArPlacement)
    // to place the block's model in the world at this point, then scrolls the card to its top and lowers it to its peek (IBlockHost.ShowHeaderAtPeek)
    // so the visitor sees the model. While THIS block's model stands the same button reads "Remove from room" and takes it away, a short line
    // says it is placed, and the first state comes back with the model's removal. The card can rise and fall, the model stays until Remove or
    // the card closes. While the wall is not localised the button is disabled and a line under it says why. The view keeps no placement state of
    // its own: it redraws from the owner on every change. Only classes here; Ar.uss draws it.
    public sealed class PlaceInArBlockView : IBlockView
    {
        public VisualElement Root { get; }
        public Button Button { get; }
        public Label ButtonLabel { get; }
        // The line under the button: "Placed by the wall" while placed, why the button is disabled while the wall is not found, else hidden
        public Label Note { get; }

        private IBlockHost _host;
        private ICardArPlacement _placement;
        private ArPlacementRequest _request;
        // The button's words in each state and the line's, read once per Bind (the authored Button Label wins over the card text of the first)
        private string _placeWords = "";
        private string _removeWords = "";
        private string _placedNote = "";
        private string _notLocalisedNote = "";

        public PlaceInArBlockView()
        {
            Root = new VisualElement { name = "card-place-in-ar" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-place-in-ar");
            Button = new Button(Press) { name = "card-place-in-ar-button" };
            Button.AddToClassList("card-place-in-ar__button");
            Button.AddToClassList("card-pill");
            Button.AddToClassList("card-tap");
            ButtonLabel = new Label { pickingMode = PickingMode.Ignore };
            ButtonLabel.AddToClassList("card-place-in-ar__label");
            Button.Add(ButtonLabel);
            Note = new Label();
            Note.AddToClassList("card-place-in-ar__note");
            Root.Add(Button);
            Root.Add(Note);
        }

        // Whether the button can place now and whether this block's model stands (what the view last drew)
        public bool CanPlace => _placement != null && _placement.CanPlace;
        public bool IsPlaced => _placement != null && _request != null && _placement.IsPlaced(_request.Poi?.id, _request.BlockKey);
        // The bound block's key (null while unbound)
        public string BlockKey => _request?.BlockKey;

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _host = context.Host;
            _placement = context.ArPlacement;
            var definition = BuiltInBlocks.PlaceInAr;
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            var settings = context.Settings;
            _request = new ArPlacementRequest
            {
                Poi = context.Poi,
                BlockKey = instance.key,
                ModelPath = read.ValidAsset(BuiltInBlocks.PlaceInArModelField, MediaKind.Model),
                Anchor = BlockLibraryRule.Choice(settings, definition, definition.Field(BuiltInBlocks.PlaceInArAnchorField), read),
                OffsetCm = read.Number(definition.Field(BuiltInBlocks.PlaceInArOffsetField)),
                ScaleMode = BlockLibraryRule.Choice(settings, definition, definition.Field(BuiltInBlocks.PlaceInArScaleField), read),
                HeightCm = read.Number(definition.Field(BuiltInBlocks.PlaceInArHeightField)),
                MarkerMultiple = read.Number(definition.Field(BuiltInBlocks.PlaceInArMultipleField)),
                KeepOnSwitch = BlockLibraryRule.Flag(settings, definition, definition.Field(BuiltInBlocks.PlaceInArKeepOnSwitchField), read),
            };
            string authored = read.Text(BuiltInBlocks.PlaceInArLabelField);
            _placeWords = !string.IsNullOrWhiteSpace(authored) ? authored.Trim() : context.Strings?.Get(CardStrings.Keys.PlaceInArButton) ?? "";
            _removeWords = context.Strings?.Get(CardStrings.Keys.PlaceInArRemove) ?? "";
            _placedNote = context.Strings?.Get(CardStrings.Keys.PlaceInArPlaced) ?? "";
            _notLocalisedNote = context.Strings?.Get(CardStrings.Keys.PlaceInArNotLocalised) ?? "";
            if (_placement != null) _placement.Changed += Refresh;
            Refresh();
        }

        public void Unbind()
        {
            if (_placement != null) _placement.Changed -= Refresh;
            _placement = null;
            _host = null;
            _request = null;
        }

        // Draw what the owner says now. Placed: the button removes (always enabled: Remove must work even if the wall is lost) and the line says
        // it is placed. Not placed: the button places, enabled only while the wall is localised, and the line says why it is not
        private void Refresh()
        {
            bool placed = IsPlaced;
            bool can = CanPlace;
            ButtonLabel.text = placed ? _removeWords : _placeWords;
            Button.tooltip = ButtonLabel.text;
            Button.SetEnabled(placed || can);
            Note.text = placed ? _placedNote : _notLocalisedNote;
            Note.style.display = placed || !can ? DisplayStyle.Flex : DisplayStyle.None;
            Root.EnableInClassList("card-place-in-ar--placed", placed);
        }

        // The one button: Remove while this block's model stands, else Place (then show the card's header at its peek so the model shows)
        private void Press()
        {
            if (_placement == null || _request == null) return;
            if (IsPlaced) _placement.Remove();
            else if (_placement.CanPlace && _placement.Place(_request)) _host?.ShowHeaderAtPeek();
        }
    }
}
