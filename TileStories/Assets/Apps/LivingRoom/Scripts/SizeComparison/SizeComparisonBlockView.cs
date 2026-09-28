using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.LivingRoom
{
    // The size_comparison block (_3.1 step 11): the point and a familiar object drawn to one scale on a shared ground line, the caption
    // (authored words, in the card's language) under them. The object's real size comes from the app's IFamiliarObjects, asked through
    // BlockBindContext.Service<T>() -- when the app registered none (or does not know the object) the point is drawn alone. The pixel
    // sizes are worked out by SizeComparisonRule once the stage has a size; SizeComparison.uss draws everything else, tokens only.
    public sealed class SizeComparisonBlockView : IBlockView
    {
        public VisualElement Root { get; }
        public VisualElement Stage { get; }
        public VisualElement PoiShape { get; }
        public VisualElement ObjectShape { get; }
        public Label Caption { get; }

        // Whether the app's service knew the chosen object (else only the point is drawn)
        public bool ObjectKnown { get; private set; }

        private Vector2 _poiCm;
        private Vector2 _objectCm;
        private string _variantClass;

        public SizeComparisonBlockView()
        {
            Root = new VisualElement { name = "card-size" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-size");
            Stage = new VisualElement { pickingMode = PickingMode.Ignore };
            Stage.AddToClassList("card-size__stage");
            PoiShape = new VisualElement { pickingMode = PickingMode.Ignore };
            PoiShape.AddToClassList("card-size__shape");
            PoiShape.AddToClassList("card-size__shape--poi");
            ObjectShape = new VisualElement { pickingMode = PickingMode.Ignore };
            ObjectShape.AddToClassList("card-size__shape");
            ObjectShape.AddToClassList("card-size__shape--object");
            Caption = new Label();
            Caption.AddToClassList("card-size__caption");
            Stage.Add(PoiShape);
            Stage.Add(ObjectShape);
            Root.Add(Stage);
            Root.Add(Caption);
            // - the stage only has a width once the card lays it out (and a new one when the phone turns): size the shapes then
            Stage.RegisterCallback<GeometryChangedEvent>(_ => Layout());
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _variantClass = "card-size--" + context.Variant;
            Root.AddToClassList(_variantClass);
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            var definition = SizeComparisonBlock.Definition;
            _poiCm = new Vector2(read.Number(definition.Field(SizeComparisonBlock.WidthField)), read.Number(definition.Field(SizeComparisonBlock.HeightField)));
            var familiar = default(FamiliarObject);
            var service = context.Service<IFamiliarObjects>();
            ObjectKnown = service != null && service.TryGet(read.Value(SizeComparisonBlock.ObjectField), out familiar);
            _objectCm = ObjectKnown ? new Vector2(familiar.WidthCm, familiar.HeightCm) : Vector2.zero;
            ObjectShape.style.display = ObjectKnown ? DisplayStyle.Flex : DisplayStyle.None;
            Caption.text = read.Text(SizeComparisonBlock.CaptionField);
            Layout();
        }

        public void Unbind()
        {
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
            ObjectKnown = false;
        }

        // Size the two shapes for the stage as it is now (nothing to do before it has a size: the geometry event comes back here)
        private void Layout()
        {
            float gap = ObjectKnown ? ObjectShape.resolvedStyle.marginLeft : 0f;
            if (!SizeComparisonRule.TryFit(Stage.contentRect.width, Stage.contentRect.height, gap, _poiCm, ObjectKnown ? _objectCm : (Vector2?)null, out var shapes)) return;
            PoiShape.style.width = shapes.Poi.x;
            PoiShape.style.height = shapes.Poi.y;
            ObjectShape.style.width = shapes.Object.x;
            ObjectShape.style.height = shapes.Object.y;
        }
    }
}
