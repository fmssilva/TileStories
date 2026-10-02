using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The hotspot_image block (_3.1 Tier 2 group B): one picture with spots on it. The picture sits whole in its frame
    // (ZoomPanRule.Whole: fitted, centred) with half a spot's circle of room on every side, so a spot on the picture's very
    // edge still shows whole; every spot is placed on the PICTURE (x / y are 0..1 across and down it), not
    // on the frame, so a spot stays on its detail whatever the picture's shape. A tap on a spot opens its title and text
    // under the picture (a CardTextView: paragraphs, glossary words); a second tap on it closes them. Which spot is open is
    // view state, never stored.
    //   numbered -- numbered circles on the picture are the spots
    //   loupes   -- small rings mark the spots on the picture; under it a row of round close-ups (SpotlightCropRule, the
    //               header spotlight's crop rule), one per spot, is what the visitor taps
    // Only classes here; Media.uss draws it.
    public sealed class HotspotImageBlockView : IBlockView
    {
        public sealed class Spot
        {
            public Vector2 At;
            public string Title;
            // The spot's number as the card counts the shown spots ("1", "2"...)
            public string Index;
            // The spot's mark on the picture: the numbered circle (numbered: the tap target) or a small ring (loupes)
            public Button Pin;
            public Label Number;
            // loupes: the round close-up under the picture (the tap target)
            public Button Loupe;
            public CardImage LoupeImage;
            public VisualElement LoupeRing;
        }

        // How many times a loupe enlarges the picture around its spot
        public const float LoupeZoom = 3f;

        public VisualElement Root { get; }
        public VisualElement Frame { get; }
        public CardImage Image { get; }
        public VisualElement Loupes { get; }
        public Label Hint { get; }
        public VisualElement Detail { get; }
        public Label DetailNumber { get; }
        public Label DetailTitle { get; }
        public CardTextView DetailText { get; }
        public IReadOnlyList<Spot> Spots => _spots.Shown;
        // The spot whose text is open (-1: none)
        public int OpenIndex { get; private set; } = -1;

        private readonly ElementPool<Spot> _spots;
        private readonly List<IReadOnlyList<string>> _texts = new();
        private string _variantClass;
        private bool _loupes;
        private BlockBindContext _context;

        public HotspotImageBlockView()
        {
            Root = new VisualElement { name = "card-hotspot" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-hotspot");
            _spots = new ElementPool<Spot>(NewSpot, ReleaseSpot);
            Frame = new VisualElement { name = "card-hotspot-frame" };
            Frame.AddToClassList("card-hotspot__frame");
            Image = new CardImage("card-hotspot__picture");
            Image.Root.pickingMode = PickingMode.Ignore;
            Frame.Add(Image.Root);
            Loupes = new VisualElement();
            Loupes.AddToClassList("card-hotspot__loupes");
            Hint = new Label();
            Hint.AddToClassList("card-hotspot__hint");
            Detail = new VisualElement { name = "card-hotspot-detail" };
            Detail.AddToClassList("card-hotspot__detail");
            var heading = new VisualElement();
            heading.AddToClassList("card-hotspot__detail-heading");
            DetailNumber = new Label();
            DetailNumber.AddToClassList("card-hotspot__number");
            DetailTitle = new Label();
            DetailTitle.AddToClassList("card-hotspot__detail-title");
            DetailText = new CardTextView("card-hotspot__paragraph");
            heading.Add(DetailNumber);
            heading.Add(DetailTitle);
            Detail.Add(heading);
            Detail.Add(DetailText.Root);
            Root.Add(Frame);
            Root.Add(Loupes);
            Root.Add(Hint);
            Root.Add(Detail);
            Frame.RegisterCallback<GeometryChangedEvent>(_ => Place());
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _context = context;
            _loupes = context.Variant == BuiltInBlocks.HotspotLoupes;
            _variantClass = "card-hotspot--" + context.Variant;
            Root.AddToClassList(_variantClass);
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            string picture = read.ValidAsset(BuiltInBlocks.HotspotImageField, MediaKind.Image);
            Image.Show(context.Media, picture, context.Strings);

            var rowFields = BuiltInBlocks.HotspotImage.Field(BuiltInBlocks.HotspotItemsField).ItemFields;
            BlockFieldDefinition xField = null, yField = null;
            foreach (var sub in rowFields)
            {
                if (sub.Key == BuiltInBlocks.HotspotXField) xField = sub;
                if (sub.Key == BuiltInBlocks.HotspotYField) yField = sub;
            }
            foreach (var item in read.Items(BuiltInBlocks.HotspotItemsField))
            {
                if (!BlockFieldReader.ItemIsComplete(item, rowFields)) continue;
                var spot = _spots.Take();
                spot.At = new Vector2(BlockFieldReader.ItemNumber(item, xField), BlockFieldReader.ItemNumber(item, yField));
                spot.Title = read.ItemText(item, BuiltInBlocks.HotspotTitleField);
                // - the card numbers the shown spots itself (this one is the last taken): a row left out never leaves a gap in the count
                spot.Index = _spots.Shown.Count.ToString(System.Globalization.CultureInfo.InvariantCulture);
                // - loupes: the mark on the picture is a bare ring (the close-up under the picture is what is tapped)
                spot.Number.text = _loupes ? "" : spot.Index;
                spot.Pin.tooltip = spot.Title;
                spot.Pin.EnableInClassList("card-tap", !_loupes);
                spot.Pin.pickingMode = _loupes ? PickingMode.Ignore : PickingMode.Position;
                Frame.Add(spot.Pin);
                if (_loupes)
                {
                    spot.Loupe.tooltip = spot.Title;
                    spot.LoupeImage.Show(context.Media, picture, context.Strings);
                    Loupes.Add(spot.Loupe);
                }
                _texts.Add(GlossaryMarkup.Paragraphs(read.ItemText(item, BuiltInBlocks.HotspotTextField)));
            }
            Loupes.style.display = _loupes && _spots.Shown.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            Hint.text = context.Strings?.Get(CardStrings.Keys.HotspotHint) ?? "";
            Open(-1);
            Place();
        }

        public void Unbind()
        {
            _spots.ReleaseAll();
            _texts.Clear();
            Image.Clear(null);
            DetailText.Clear();
            OpenIndex = -1;
            _context = null;
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        // Open spot `index`'s text under the picture (-1, or the open spot again: close it)
        public void Tap(int index) => Open(index == OpenIndex ? -1 : index);

        private void Open(int index)
        {
            OpenIndex = index >= 0 && index < _spots.Shown.Count ? index : -1;
            for (int i = 0; i < _spots.Shown.Count; i++)
            {
                _spots.Shown[i].Pin.EnableInClassList("card-hotspot__pin--open", i == OpenIndex);
                _spots.Shown[i].Loupe.EnableInClassList("card-hotspot__loupe--open", i == OpenIndex);
            }
            // - classes, not an inline display: the detail's look lives in Media.uss
            Detail.EnableInClassList("card-hotspot__detail--open", OpenIndex >= 0);
            if (OpenIndex < 0)
            {
                DetailNumber.text = "";
                DetailTitle.text = "";
                DetailText.Clear();
                return;
            }
            DetailNumber.text = _spots.Shown[OpenIndex].Index;
            DetailTitle.text = _spots.Shown[OpenIndex].Title;
            DetailText.Bind(_texts[OpenIndex], _context?.Glossary);
        }

        // The picture's place in the frame (whole, centred) and every spot's mark on it; each loupe's close-up
        private void Place()
        {
            // - room for half a mark on every side: the mark as Media.uss draws it (numbered circle / loupes ring)
            float mark = _spots.Shown.Count > 0 ? _spots.Shown[0].Number.resolvedStyle.width : 0f;
            float inset = float.IsNaN(mark) ? 0f : mark / 2f;
            var room = new Vector2(Frame.layout.width, Frame.layout.height) - 2f * inset * Vector2.one;
            if (float.IsNaN(room.x) || room.x <= 0f || room.y <= 0f) return;
            var fitted = ZoomPanRule.Whole(room, Image.Aspect);
            var size = ZoomPanRule.SizeOf(fitted, room, Image.Aspect);
            var whole = new ZoomPan(1f, fitted.Offset + inset * Vector2.one);
            Image.Root.style.left = whole.Offset.x;
            Image.Root.style.top = whole.Offset.y;
            Image.Root.style.width = size.x;
            Image.Root.style.height = size.y;
            foreach (var spot in _spots.Shown)
            {
                // - centred on its point: the mark's own size comes from Media.uss, so translate by half of it
                spot.Pin.style.left = whole.Offset.x + spot.At.x * size.x;
                spot.Pin.style.top = whole.Offset.y + spot.At.y * size.y;
                if (!_loupes) continue;
                // - inside the loupe's border: that is where its absolute children are placed
                var box = spot.Loupe.contentRect.size;
                if (float.IsNaN(box.x) || box.x <= 0f) continue;
                var crop = SpotlightCropRule.Place(box, spot.LoupeImage.Aspect, spot.At, LoupeZoom);
                spot.LoupeImage.Root.style.left = crop.Offset.x;
                spot.LoupeImage.Root.style.top = crop.Offset.y;
                spot.LoupeImage.Root.style.width = crop.Size.x;
                spot.LoupeImage.Root.style.height = crop.Size.y;
                spot.LoupeRing.style.left = crop.Focus.x;
                spot.LoupeRing.style.top = crop.Focus.y;
            }
        }

        // The spot of place `at`: its pin on the picture and its close-up, both opening that place's text
        private Spot NewSpot(int at)
        {
            var spot = new Spot { Number = new Label { pickingMode = PickingMode.Ignore } };
            spot.Pin = new Button(() => Tap(at));
            spot.Pin.AddToClassList("card-hotspot__pin");
            spot.Number.AddToClassList("card-hotspot__number");
            spot.Pin.Add(spot.Number);
            spot.Loupe = new Button(() => Tap(at));
            spot.Loupe.AddToClassList("card-hotspot__loupe");
            spot.Loupe.AddToClassList("card-tap");
            spot.LoupeImage = new CardImage("card-hotspot__loupe-picture");
            spot.LoupeImage.Root.pickingMode = PickingMode.Ignore;
            spot.LoupeRing = new VisualElement { pickingMode = PickingMode.Ignore };
            spot.LoupeRing.AddToClassList("card-hotspot__loupe-ring");
            spot.Loupe.Add(spot.LoupeImage.Root);
            spot.Loupe.Add(spot.LoupeRing);
            // - the close-up is placed once its round box has a size
            spot.Loupe.RegisterCallback<GeometryChangedEvent>(_ => Place());
            return spot;
        }

        // Take a shown spot off the picture: its pin, its close-up and the close-up's picture
        private static void ReleaseSpot(Spot spot)
        {
            spot.Pin.RemoveFromHierarchy();
            spot.Loupe.RemoveFromHierarchy();
            spot.LoupeImage.Clear(null);
        }
    }
}
