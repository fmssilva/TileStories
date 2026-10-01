using UnityEngine.UIElements;

namespace TileStories
{
    // The place_in_ar block (_3.1 Tier 5, step 10B.2): one primary button that asks the card's AR placement owner (ICardArPlacement) to place
    // the block's model in the world at this point and then lowers the card to its peek (IBlockHost.ShowOnWall, as show_on_wall does) so the
    // visitor sees it. While THIS block's model stands, a Remove chip takes it away; the card can rise and fall, the model stays until Remove
    // or the card closes. While the wall is not localised the button is disabled and a line under it says why. The view keeps no placement
    // state of its own: it redraws from the owner on every change. Only classes here; Ar.uss draws it.
    public sealed class PlaceInArBlockView : IBlockView
    {
        public VisualElement Root { get; }
        public Button Button { get; }
        public Label ButtonLabel { get; }
        public Label Note { get; }
        public Button RemoveChip { get; }
        public Label RemoveLabel { get; }

        private IBlockHost _host;
        private ICardArPlacement _placement;
        private ArPlacementRequest _request;

        public PlaceInArBlockView()
        {
            Root = new VisualElement { name = "card-place-in-ar" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-place-in-ar");
            Button = new Button(Place) { name = "card-place-in-ar-button" };
            Button.AddToClassList("card-place-in-ar__button");
            Button.AddToClassList("card-pill");
            Button.AddToClassList("card-tap");
            ButtonLabel = new Label { pickingMode = PickingMode.Ignore };
            ButtonLabel.AddToClassList("card-place-in-ar__label");
            Button.Add(ButtonLabel);
            Note = new Label();
            Note.AddToClassList("card-place-in-ar__note");
            RemoveChip = new Button(Remove) { name = "card-place-in-ar-remove" };
            RemoveChip.AddToClassList("card-place-in-ar__remove");
            RemoveChip.AddToClassList("card-tap");
            RemoveLabel = new Label { pickingMode = PickingMode.Ignore };
            RemoveLabel.AddToClassList("card-place-in-ar__remove-label");
            RemoveChip.Add(RemoveLabel);
            Root.Add(Button);
            Root.Add(Note);
            Root.Add(RemoveChip);
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
            var settings = context.Taxonomy?.card_settings;
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
            };
            string authored = read.Text(BuiltInBlocks.PlaceInArLabelField);
            ButtonLabel.text = !string.IsNullOrWhiteSpace(authored) ? authored.Trim() : context.Strings?.Get(CardStrings.Keys.PlaceInArButton) ?? "";
            Button.tooltip = ButtonLabel.text;
            Note.text = context.Strings?.Get(CardStrings.Keys.PlaceInArNotLocalised) ?? "";
            RemoveLabel.text = context.Strings?.Get(CardStrings.Keys.PlaceInArRemove) ?? "";
            RemoveChip.tooltip = RemoveLabel.text;
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

        // Draw what the owner says now: the button enabled only while the wall is localised, the line why it is not, the chip while placed
        private void Refresh()
        {
            bool can = CanPlace;
            Button.SetEnabled(can);
            Note.style.display = can ? DisplayStyle.None : DisplayStyle.Flex;
            bool placed = IsPlaced;
            RemoveChip.style.display = placed ? DisplayStyle.Flex : DisplayStyle.None;
            Root.EnableInClassList("card-place-in-ar--placed", placed);
        }

        private void Place()
        {
            if (_placement == null || _request == null || !_placement.CanPlace) return;
            if (_placement.Place(_request)) _host?.ShowOnWall();
        }

        private void Remove() => _placement?.Remove();
    }
}
