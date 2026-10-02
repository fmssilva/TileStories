using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The related block (_3.1 Tier 2 group B): other points worth seeing from here, picked by RelatedPoisRule (manual rows,
    // or automatically by category / distance).
    //   carousel        -- a horizontal strip, one card per point (its header picture when it has one, and its title); a
    //                       tap selects that point through the card's host (the selection bus: the card rebinds, zoom-on-
    //                       select as a marker tap)
    //   next_along_wall -- one button, the nearest picked point to the right along the wall (RelatedPoisRule.NextAlongWall,
    //                       wall_locator's own axis rule), wrapping to the left at the wall's end; same tap behaviour
    // Only classes and picks here; Visit.uss draws it.
    public sealed class RelatedBlockView : IBlockView
    {
        // One carousel card: its tap target, its picture, its title, the point it selects
        public sealed class Card
        {
            public VisualElement Box;
            public CardImage Image;
            public Label Title;
            public string PoiId;
        }

        public VisualElement Root { get; }
        public ScrollView Track { get; }
        public VisualElement NextRow { get; }
        public Button Next { get; }
        public VisualElement NextChevron { get; }
        public Label NextTitle { get; }
        // next_along_wall: the point Next selects ("" = none, the block would not be shown)
        public string NextPoiId { get; private set; } = "";
        public IReadOnlyList<Card> Cards => _shown;

        private readonly List<Card> _pool = new();
        private readonly List<Card> _shown = new();
        private IBlockHost _host;
        private string _variantClass;
        // Peek Next Card (card_settings.container.peek_next_card), read at bind: a live edit rebinds the card
        private bool _peekNext = true;
        private readonly List<VisualElement> _cells = new();

        public RelatedBlockView()
        {
            Root = new VisualElement { name = "card-related" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-related");

            Track = new ScrollView(ScrollViewMode.Horizontal)
            {
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
                verticalScrollerVisibility = ScrollerVisibility.Hidden,
            };
            Track.AddToClassList("card-related__track");
            Track.contentContainer.AddToClassList("card-related__track-content");

            Next = new Button(() => Pick(NextPoiId)) { name = "card-related-next" };
            Next.AddToClassList("card-related__next");
            Next.AddToClassList("card-tap");
            NextChevron = new VisualElement { pickingMode = PickingMode.Ignore };
            NextChevron.AddToClassList("card-related__chevron");
            NextTitle = new Label { pickingMode = PickingMode.Ignore };
            NextTitle.AddToClassList("card-related__next-title");
            Next.Add(NextChevron);
            Next.Add(NextTitle);
            NextRow = new VisualElement();
            NextRow.AddToClassList("card-related__next-row");
            NextRow.Add(Next);

            Root.Add(Track);
            Root.Add(NextRow);
            // - the strip only has a width once the card lays it out (and a new one when the sheet changes): fit the cards then
            Track.contentViewport.RegisterCallback<GeometryChangedEvent>(_ => FitCards());
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _host = context.Host;
            _peekNext = context.Settings?.container?.peek_next_card ?? true;
            _variantClass = "card-related--" + context.Variant;
            Root.AddToClassList(_variantClass);
            var wallPois = context.Taxonomy?.pois;
            var picked = RelatedPoisRule.Of(context.Poi, instance, wallPois);
            bool nextAlongWall = context.Variant == BuiltInBlocks.RelatedNextAlongWall;
            Track.style.display = nextAlongWall ? DisplayStyle.None : DisplayStyle.Flex;
            NextRow.style.display = nextAlongWall ? DisplayStyle.Flex : DisplayStyle.None;
            if (nextAlongWall) BindNext(context, picked, wallPois);
            else BindCarousel(context, picked);
        }

        public void Unbind()
        {
            foreach (var card in _shown)
            {
                card.Box.RemoveFromHierarchy();
                card.Image.Clear(null);
                _pool.Add(card);
            }
            _shown.Clear();
            Track.Clear();
            Track.scrollOffset = UnityEngine.Vector2.zero;
            NextPoiId = "";
            _host = null;
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        private void BindCarousel(BlockBindContext context, List<POIData> picked)
        {
            foreach (var poi in picked)
            {
                var card = TakeCard();
                string picture = HeaderPictureOf(poi);
                card.Image.Root.style.display = picture.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                if (picture.Length > 0) card.Image.Show(context.Media, picture, context.Strings);
                card.Title.text = BlockStackBuilder.CardTitleOf(poi, context.Language, context.FallbackLanguage);
                card.PoiId = poi.id;
                Track.Add(card.Box);
                _shown.Add(card);
            }
            FitCards();
        }

        // Size the cards for the swipe strip (CardTrackFit): whole cards only, or -- with Peek Next Card -- whole cards and a slice of the next
        // at rest; never a card cut at the end of the swipe (a picture-less card once read "Lamp -"). The narrowest a card may be is its USS
        // min-width (the card token); the gap is its right margin.
        private void FitCards()
        {
            _cells.Clear();
            foreach (var card in _shown) _cells.Add(card.Box);
            CardTrackFit.Apply(Track, _cells, _peekNext);
        }

        private void BindNext(BlockBindContext context, List<POIData> picked, IReadOnlyList<POIData> wallPois)
        {
            var (next, direction) = RelatedPoisRule.NextAlongWall(context.Poi, picked, wallPois);
            NextPoiId = next?.id ?? "";
            NextTitle.text = next != null ? BlockStackBuilder.CardTitleOf(next, context.Language, context.FallbackLanguage) : "";
            NextRow.style.visibility = next != null ? Visibility.Visible : Visibility.Hidden;
            NextChevron.EnableInClassList("card-related__chevron--wrap", direction < 0);
        }

        private void Pick(string poiId)
        {
            if (!string.IsNullOrEmpty(poiId)) _host?.SelectPoi(poiId);
        }

        private Card TakeCard()
        {
            Card card;
            if (_pool.Count > 0)
            {
                card = _pool[_pool.Count - 1];
                _pool.RemoveAt(_pool.Count - 1);
            }
            else
            {
                card = new Card { Box = new VisualElement(), Image = new CardImage("card-related__picture"), Title = new Label() };
                card.Box.AddToClassList("card-related__card");
                card.Box.AddToClassList("card-tap");
                card.Title.AddToClassList("card-related__title");
                card.Box.Add(card.Image.Root);
                card.Box.Add(card.Title);
                var c = card;
                card.Box.AddManipulator(new Clickable(() => Pick(c.PoiId)));
            }
            return card;
        }

        // The picture this POI's own header SHOWS (its hero), or "": the header's own rule decides (HeaderShowsPicture),
        // as TodayMapBlockView's bridge side also reads it (the one rule for "does this point have a header picture")
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
