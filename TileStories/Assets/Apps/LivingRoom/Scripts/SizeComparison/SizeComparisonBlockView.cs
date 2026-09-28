using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.LivingRoom
{
    // The size_comparison block (_3.1 step 11): the point and a familiar object drawn to one scale on a shared ground line, a NAME under
    // each shape (the point's card title; the object's name from the app's own card texts, step 11-fix -- so the two shapes are never told
    // apart by colour alone) and the caption (authored words, in the card's language) under them. The object's real size comes from the
    // app's IFamiliarObjects, asked through BlockBindContext.Service<T>() -- when the app registered none (or does not know the object)
    // the point is drawn alone. The pixel sizes are worked out by SizeComparisonRule once the stage has a size; SizeComparison.uss draws
    // everything else, tokens only.
    public sealed class SizeComparisonBlockView : IBlockView
    {
        public VisualElement Root { get; }
        public VisualElement Stage { get; }
        public VisualElement PoiSlot { get; }
        public VisualElement ObjectSlot { get; }
        public VisualElement PoiShape { get; }
        public VisualElement ObjectShape { get; }
        public VisualElement Names { get; }
        public Label PoiName { get; }
        public Label ObjectName { get; }
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
            PoiSlot = Slot("card-size__slot--poi");
            ObjectSlot = Slot("card-size__slot--object");
            PoiShape = Shape("card-size__shape--poi");
            ObjectShape = Shape("card-size__shape--object");
            PoiSlot.Add(PoiShape);
            ObjectSlot.Add(ObjectShape);
            Stage.Add(PoiSlot);
            Stage.Add(ObjectSlot);
            // - the names sit in their own row under the ground line, one per slot and as wide as it, so each is centred under its shape
            Names = new VisualElement { pickingMode = PickingMode.Ignore };
            Names.AddToClassList("card-size__names");
            PoiName = Name("card-size__name--poi");
            ObjectName = Name("card-size__name--object");
            Names.Add(PoiName);
            Names.Add(ObjectName);
            Caption = new Label();
            Caption.AddToClassList("card-size__caption");
            Root.Add(Stage);
            Root.Add(Names);
            Root.Add(Caption);
            // - the stage only has a width once the card lays it out (and a new one when the phone turns): size the shapes then
            Stage.RegisterCallback<GeometryChangedEvent>(_ => Layout());
        }

        private static VisualElement Slot(string variantClass)
        {
            var slot = new VisualElement { pickingMode = PickingMode.Ignore };
            slot.AddToClassList("card-size__slot");
            slot.AddToClassList(variantClass);
            return slot;
        }

        private static VisualElement Shape(string variantClass)
        {
            var shape = new VisualElement { pickingMode = PickingMode.Ignore };
            shape.AddToClassList("card-size__shape");
            shape.AddToClassList(variantClass);
            return shape;
        }

        private static Label Name(string variantClass)
        {
            var label = new Label { pickingMode = PickingMode.Ignore };
            label.AddToClassList("card-size__name");
            label.AddToClassList(variantClass);
            return label;
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
            // - the slot, the shape in it and the name under it all go together (a hidden parent does not change a child's own display)
            var objectDisplay = ObjectKnown ? DisplayStyle.Flex : DisplayStyle.None;
            ObjectSlot.style.display = objectDisplay;
            ObjectShape.style.display = objectDisplay;
            ObjectName.style.display = objectDisplay;
            ObjectShape.EnableInClassList("card-size__shape--round", ObjectKnown && familiar.Round);
            PoiName.text = BlockStackBuilder.CardTitleOf(context.Poi, context.Language, context.FallbackLanguage);
            ObjectName.text = ObjectKnown ? context.Strings?.Get(familiar.NameKey) ?? "" : "";
            Caption.text = read.Text(SizeComparisonBlock.CaptionField);
            Layout();
        }

        public void Unbind()
        {
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
            ObjectKnown = false;
        }

        // Size the two shapes, their slots and their names for the stage as it is now (nothing to do before it has a size: the geometry
        // event comes back here)
        private void Layout()
        {
            float gap = ObjectKnown ? ObjectSlot.resolvedStyle.marginLeft : 0f;
            if (!SizeComparisonRule.TryFit(Stage.contentRect.width, Stage.contentRect.height, gap, _poiCm, ObjectKnown ? _objectCm : (Vector2?)null,
                    NaturalWidth(PoiName), ObjectKnown ? NaturalWidth(ObjectName) : 0f, out var shapes)) return;
            PoiShape.style.width = shapes.Poi.x;
            PoiShape.style.height = shapes.Poi.y;
            ObjectShape.style.width = shapes.Object.x;
            ObjectShape.style.height = shapes.Object.y;
            PoiSlot.style.width = shapes.PoiSlotWidth;
            ObjectSlot.style.width = shapes.ObjectSlotWidth;
            PoiName.style.width = shapes.PoiSlotWidth;
            ObjectName.style.width = shapes.ObjectSlotWidth;
            // - the second name keeps the same gap the second slot has, so each name stays under its own shape
            ObjectName.style.marginLeft = gap;
        }

        // The width one line of this label's text takes at its resolved font (a label with no words asks for nothing)
        private static float NaturalWidth(Label label) =>
            string.IsNullOrEmpty(label.text) ? 0f
            : Mathf.Ceil(label.MeasureTextSize(label.text, 0f, VisualElement.MeasureMode.Undefined, 0f, VisualElement.MeasureMode.Undefined).x);
    }
}
