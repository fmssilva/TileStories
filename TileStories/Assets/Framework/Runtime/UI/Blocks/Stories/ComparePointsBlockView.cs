using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The compare_points block (_3.1 Tier 1 group B): this point beside another point of the wall, by their condition. rings:
    // under the stack's heading (default: CardStrings compare_heading), two columns -- this point, then the other -- each
    // its marker's own ring picture in its marker's colour, both resolved by CardStatusRule and painted by the status block's own painter (so the two rings
    // can never differ from the markers or from a status block), the point's card title (BlockStackBuilder.CardTitleOf) and
    // its condition's name. BlockStackBuilder never binds it unless both points have a status (BuiltInBlocks.ComparePoints).
    public sealed class ComparePointsBlockView : IBlockView
    {
        public sealed class Side
        {
            public VisualElement Box;
            public VisualElement Ring;
            public Label UnknownMark;
            public Label Title;
            public Label Level;
            public CardStatusRule.Status Status;
            public string PoiId;
        }

        public VisualElement Root { get; }
        // This point first, the other second
        public IReadOnlyList<Side> Sides => _sides;

        private readonly List<Side> _sides = new();
        private string _variantClass;

        public ComparePointsBlockView()
        {
            Root = new VisualElement { name = "card-compare" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-compare");
            var row = new VisualElement();
            row.AddToClassList("card-compare__row");
            for (int i = 0; i < 2; i++)
            {
                var side = new Side { Box = new VisualElement(), Ring = new VisualElement(), UnknownMark = new Label(), Title = new Label(), Level = new Label() };
                side.Box.AddToClassList("card-compare__side");
                side.Ring.AddToClassList("card-status__ring");
                side.Ring.AddToClassList("card-compare__ring");
                side.UnknownMark.AddToClassList("card-status__unknown-mark");
                side.Ring.Add(side.UnknownMark);
                side.Title.AddToClassList("card-compare__title");
                side.Level.AddToClassList("card-compare__level");
                side.Box.Add(side.Ring);
                side.Box.Add(side.Title);
                side.Box.Add(side.Level);
                row.Add(side.Box);
                _sides.Add(side);
            }
            Root.Add(row);
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _variantClass = "card-compare--" + context.Variant;
            Root.AddToClassList(_variantClass);
            var other = BuiltInBlocks.OtherPoi(instance, context.Taxonomy?.pois);
            BindSide(_sides[0], context.Poi, context);
            BindSide(_sides[1], other, context);
        }

        public void Unbind()
        {
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        private static void BindSide(Side side, POIData poi, BlockBindContext context)
        {
            var status = CardStatusRule.Resolve(poi, context.MarkerLook, context.Taxonomy);
            side.Status = status;
            side.PoiId = poi?.id;
            side.Title.text = BlockStackBuilder.CardTitleOf(poi, context.Language, context.FallbackLanguage);
            side.Level.text = StatusBlockView.NameOf(status, context.Strings);
            side.UnknownMark.text = status.Unknown ? context.Strings?.Get(CardStrings.Keys.StatusUnknownMark) ?? "" : "";
            side.UnknownMark.style.display = status.Unknown ? DisplayStyle.Flex : DisplayStyle.None;
            StatusBlockView.Paint(side.Ring, status);
        }
    }
}
