using System.Globalization;
using UnityEngine.UIElements;

namespace TileStories
{
    // The today_map block (_3.1 Tier 2 group B): where the place the wall shows is in the city today.
    //   static -- the map picture, the coordinates under it, a Directions button
    //   bridge -- from the wall to today: the point as the wall shows it ("On the wall": its header picture, when it has one,
    //             and its card title) beside a small map of where it is now ("Today"), then the coordinates and Directions
    // The coordinates show only when both Latitude and Longitude were written (CardStrings' format joins them). Directions
    // shows only for a link WebLinkRule opens, and asks the card's host to open it (IBlockHost.OpenUrl: the device in the
    // app, a recorder in a test). Only classes here; Visit.uss draws it.
    public sealed class TodayMapBlockView : IBlockView
    {
        public VisualElement Root { get; }
        public VisualElement Bridge { get; }
        public Label ThenLabel { get; }
        public CardImage ThenPicture { get; }
        public Label ThenTitle { get; }
        public Label NowLabel { get; }
        public CardImage NowMap { get; }
        public CardImage Map { get; }
        public Label Coordinates { get; }
        public Button Directions { get; }
        // The link Directions opens ("" = no button)
        public string DirectionsUrl { get; private set; } = "";

        private IBlockHost _host;
        private string _variantClass;

        public TodayMapBlockView()
        {
            Root = new VisualElement { name = "card-today" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-today");

            Bridge = new VisualElement();
            Bridge.AddToClassList("card-today__bridge");
            var then = new VisualElement();
            then.AddToClassList("card-today__side");
            ThenLabel = new Label();
            ThenLabel.AddToClassList("card-today__side-label");
            ThenPicture = new CardImage("card-today__thumb");
            ThenTitle = new Label();
            ThenTitle.AddToClassList("card-today__then-title");
            then.Add(ThenLabel);
            then.Add(ThenPicture.Root);
            then.Add(ThenTitle);
            var arrow = new VisualElement { pickingMode = PickingMode.Ignore };
            arrow.AddToClassList("card-today__arrow");
            var now = new VisualElement();
            now.AddToClassList("card-today__side");
            NowLabel = new Label();
            NowLabel.AddToClassList("card-today__side-label");
            NowMap = new CardImage("card-today__thumb");
            now.Add(NowLabel);
            now.Add(NowMap.Root);
            Bridge.Add(then);
            Bridge.Add(arrow);
            Bridge.Add(now);

            Map = new CardImage("card-today__map");
            Coordinates = new Label();
            Coordinates.AddToClassList("card-today__coordinates");
            Directions = new Button(() => _host?.OpenUrl(DirectionsUrl)) { name = "card-today-directions" };
            Directions.AddToClassList("card-pill");
            Directions.AddToClassList("card-today__directions");
            Directions.AddToClassList("card-tap");

            Root.Add(Bridge);
            Root.Add(Map.Root);
            Root.Add(Coordinates);
            Root.Add(Directions);
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _host = context.Host;
            _variantClass = "card-today--" + context.Variant;
            Root.AddToClassList(_variantClass);
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            string map = read.ValidAsset(BuiltInBlocks.TodayMapImageField, MediaKind.Image);
            bool bridge = context.Variant == BuiltInBlocks.TodayMapBridge;

            Bridge.style.display = bridge ? DisplayStyle.Flex : DisplayStyle.None;
            Map.Root.style.display = bridge ? DisplayStyle.None : DisplayStyle.Flex;
            if (bridge)
            {
                ThenLabel.text = context.Strings?.Get(CardStrings.Keys.TodayMapThen) ?? "";
                NowLabel.text = context.Strings?.Get(CardStrings.Keys.TodayMapNow) ?? "";
                ThenTitle.text = BlockStackBuilder.CardTitleOf(context.Poi, context.Language, context.FallbackLanguage);
                string picture = HeaderPictureOf(context.Poi);
                ThenPicture.Root.style.display = picture.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                if (picture.Length > 0) ThenPicture.Show(context.Media, picture, context.Strings);
                NowMap.Show(context.Media, map, context.Strings);
            }
            else Map.Show(context.Media, map, context.Strings);

            bool coordinates = read.Stored(BuiltInBlocks.TodayMapLatField) && read.Stored(BuiltInBlocks.TodayMapLngField);
            Coordinates.style.display = coordinates ? DisplayStyle.Flex : DisplayStyle.None;
            Coordinates.text = coordinates
                ? string.Format(CultureInfo.InvariantCulture, context.Strings?.Get(CardStrings.Keys.TodayMapCoordinates) ?? "",
                    read.Number(BuiltInBlocks.TodayMap.Field(BuiltInBlocks.TodayMapLatField)).ToString("F5", CultureInfo.InvariantCulture),
                    read.Number(BuiltInBlocks.TodayMap.Field(BuiltInBlocks.TodayMapLngField)).ToString("F5", CultureInfo.InvariantCulture))
                : "";

            DirectionsUrl = read.OpenableUrl(BuiltInBlocks.TodayMapUrlField);
            Directions.text = context.Strings?.Get(CardStrings.Keys.TodayMapDirections) ?? "";
            Directions.style.display = DirectionsUrl.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void Unbind()
        {
            Map.Clear(null);
            NowMap.Clear(null);
            ThenPicture.Clear(null);
            DirectionsUrl = "";
            _host = null;
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        // The picture this POI's own header SHOWS (its hero), or "": the header's own rule decides (HeaderShowsPicture), so the
        // bridge never shows a picture the card's header does not (a text-only header with a stored picture shows none)
        private static string HeaderPictureOf(POIData poi)
        {
            var blocks = poi?.card?.blocks;
            if (blocks == null) return "";
            foreach (var block in blocks)
            {
                if (block == null || block.kind != BuiltInBlocks.HeaderKind) continue;
                string variant = BuiltInBlocks.Header.HasVariant(block.variant) ? block.variant : BuiltInBlocks.Header.DefaultVariant;
                return BuiltInBlocks.HeaderShowsPicture(variant, block)
                    ? new BlockFieldReader(block, null, null).ValidAsset(BuiltInBlocks.HeaderImageField, MediaKind.Image) : "";
            }
            return "";
        }
    }
}
