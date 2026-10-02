using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The show_on_wall block (_3.1 Tier 3, step 8B): one button that lowers the card to its peek so the visitor sees this point's
    // marker on the wall, through IBlockHost.ShowOnWall -- the same host call the actions block's show_on_wall button makes. The
    // point stays selected (the selection bus is untouched), so its marker stays lit and the others dim as after a tap on it.
    //   button          -- just the button
    //   with_neighbours -- the button and, under it, the nearest points on the wall by their card titles (RelatedPoisRule, the
    //                      related block's own "nearest" pick); a tap on one selects it through IBlockHost.SelectPoi
    // Only classes here; Ar.uss draws it.
    public sealed class ShowOnWallBlockView : IBlockView
    {
        // One neighbouring point: its tap target, its name, the point it selects
        public sealed class Neighbour
        {
            public Button Button;
            public Label Title;
            public string PoiId;
        }

        public VisualElement Root { get; }
        public Button Button { get; }
        public VisualElement Icon { get; }
        public Label ButtonLabel { get; }
        public Label Caption { get; }
        public VisualElement NeighboursRow { get; }
        public IReadOnlyList<Neighbour> Neighbours => _neighbours.Shown;

        private readonly ElementPool<Neighbour> _neighbours;
        private IBlockHost _host;
        private string _variantClass;

        public ShowOnWallBlockView()
        {
            Root = new VisualElement { name = "card-show-on-wall" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-show-on-wall");
            Button = new Button(() => _host?.ShowOnWall()) { name = "card-show-on-wall-button" };
            Button.AddToClassList("card-show-on-wall__button");
            Button.AddToClassList("card-pill");
            Button.AddToClassList("card-tap");
            Icon = CardIcons.Create();
            CardIcons.SetKey(Icon, CardIcons.ShowOnWall);
            Icon.AddToClassList("card-show-on-wall__icon");
            ButtonLabel = new Label { pickingMode = PickingMode.Ignore };
            ButtonLabel.AddToClassList("card-show-on-wall__label");
            Button.Add(Icon);
            Button.Add(ButtonLabel);
            Caption = new Label();
            Caption.AddToClassList("card-show-on-wall__caption");
            NeighboursRow = new VisualElement();
            NeighboursRow.AddToClassList("card-show-on-wall__neighbours");
            Root.Add(Button);
            Root.Add(Caption);
            Root.Add(NeighboursRow);
            _neighbours = new ElementPool<Neighbour>(_ => NewNeighbour(), neighbour => neighbour.Button.RemoveFromHierarchy());
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _host = context.Host;
            _variantClass = "card-show-on-wall--" + context.Variant;
            Root.AddToClassList(_variantClass);
            ButtonLabel.text = context.Strings?.Get(CardStrings.Keys.ShowOnWallButton) ?? "";
            Button.tooltip = ButtonLabel.text;
            bool withNeighbours = context.Variant == BuiltInBlocks.ShowOnWallWithNeighbours;
            var picked = withNeighbours ? RelatedPoisRule.Pick(context.Poi, RelatedPoisRule.SourceNearest, null, context.Taxonomy?.pois) : new List<POIData>();
            for (int i = 0; i < picked.Count && i < BuiltInBlocks.ShowOnWallNeighbourCount; i++)
            {
                var neighbour = _neighbours.Take();
                neighbour.PoiId = picked[i].id;
                neighbour.Title.text = BlockStackBuilder.CardTitleOf(picked[i], context.Language, context.FallbackLanguage);
                neighbour.Button.tooltip = neighbour.Title.text;
                NeighboursRow.Add(neighbour.Button);
            }
            Caption.text = context.Strings?.Get(CardStrings.Keys.ShowOnWallNearby) ?? "";
            bool any = _neighbours.Shown.Count > 0;
            Caption.style.display = any ? DisplayStyle.Flex : DisplayStyle.None;
            NeighboursRow.style.display = any ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void Unbind()
        {
            _neighbours.ReleaseAll();
            _host = null;
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        // One neighbour chip: a tap selects whichever point it names at the time
        private Neighbour NewNeighbour()
        {
            var neighbour = new Neighbour { Button = new Button(), Title = new Label { pickingMode = PickingMode.Ignore } };
            neighbour.Button.AddToClassList("card-show-on-wall__neighbour");
            neighbour.Button.AddToClassList("card-tap");
            neighbour.Title.AddToClassList("card-show-on-wall__neighbour-title");
            neighbour.Button.Add(neighbour.Title);
            neighbour.Button.clicked += () =>
            {
                if (!string.IsNullOrEmpty(neighbour.PoiId)) _host?.SelectPoi(neighbour.PoiId);
            };
            return neighbour;
        }
    }
}
