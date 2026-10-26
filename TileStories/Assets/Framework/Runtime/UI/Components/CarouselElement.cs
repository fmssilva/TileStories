using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The carousel element (_3.3 R14.2, the P4.4b review's App Store style): a strip of pictures the reader swipes through, each in a frame of
    // the strip's one proportion (ElementMediaRule.CarouselAspect) with its caption and credit under it. Full bleed (its default) the strip runs
    // edge to edge while its first item lines up with the text's margin, so items bleed off the edges as it moves; a released swipe rests an
    // item on that margin (NavStrip.SnapsToItems). The strip is a NavStrip, not a horizontal ScrollView: its SwipeGrab takes a finger only once
    // it moves clearly sideways (R10.5: horizontal = the block, vertical = the card), so a drag up or down still scrolls the page or the card.
    // A swipe is its only gesture (R17.4: fixed). Items are made once and reused; any panel (R15.2).
    public sealed class CarouselElement : VisualElement, IElementComponent
    {
        // One item of the strip: its figure, frame and words
        public sealed class Item
        {
            public VisualElement Figure;
            public MediaFrame Frame;
            public MediaCaption Words;
        }

        // The row glides onto its item after a release (not under Reduce Motion: it rests there at once)
        public const string GlidesClass = "ts-el-carousel__row--glides";
        // The last item keeps no gap after it (USS has no sibling selector)
        private const string LastClass = "ts-el-carousel__item--last";

        public VisualElement Root => this;
        public NavStrip Strip { get; }
        // The items shown now, in order
        public IReadOnlyList<Item> Items => _shown;

        private readonly List<Item> _pool = new();
        private readonly List<Item> _shown = new();

        public CarouselElement()
        {
            ElementStyle.Init(this, BuiltInElements.Carousel);
            AddToClassList("ts-el-media");
            Strip = new NavStrip("ts-el-carousel", "ts-el-carousel__viewport", "ts-el-carousel__row") { SnapsToItems = true };
            Add(Strip.Viewport);
        }

        public void Bind(ElementRow row, ElementBindContext context)
        {
            Unbind();
            ElementStyle.Apply(this, row, context);
            Strip.Row.EnableInClassList(GlidesClass, !(context.Block?.ReduceMotion ?? false));
            bool arch = ElementStyle.IsArch(row, context);
            foreach (var media in row.Media)
            {
                var item = Take();
                var (caption, credit) = MediaFrame.WordsOf(media, context);
                item.Frame.Show(context.Block?.Media, media.Asset, context.Block?.Strings, arch, ElementMediaRule.CarouselAspect);
                item.Words.Show(context.Compact ? "" : caption, context.Compact ? "" : credit);
                item.Figure.RemoveFromClassList(LastClass);
                Strip.Row.Add(item.Figure);
                _shown.Add(item);
            }
            if (_shown.Count > 0) _shown[_shown.Count - 1].Figure.AddToClassList(LastClass);
            Strip.Reset();
        }

        public void Unbind()
        {
            foreach (var item in _shown)
            {
                item.Figure.RemoveFromHierarchy();
                item.Frame.Clear();
                item.Words.Show("", "");
                _pool.Add(item);
            }
            _shown.Clear();
            Strip.Row.RemoveFromClassList(NavStrip.SettlingClass);
            Strip.Reset();
        }

        private Item Take()
        {
            if (_pool.Count > 0)
            {
                var reused = _pool[_pool.Count - 1];
                _pool.RemoveAt(_pool.Count - 1);
                return reused;
            }
            var item = new Item { Figure = new VisualElement { name = "ts-el-carousel-item" }, Frame = new MediaFrame(), Words = new MediaCaption() };
            item.Figure.AddToClassList("ts-el-carousel__item");
            item.Figure.Add(item.Frame.Root);
            item.Figure.Add(item.Words.Root);
            return item;
        }
    }
}
