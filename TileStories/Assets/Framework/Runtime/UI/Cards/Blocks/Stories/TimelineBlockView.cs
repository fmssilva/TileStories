using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The timeline block (_3.1 Tier 1 group B): dated events in the authored order, each a dot on one line, its date, title
    // and optional text (a CardTextView). vertical: down the card, the line running from dot to dot; horizontal: side by side
    // in a horizontal ScrollView the visitor swipes (a phone card is too narrow for more than two events). Highlight Now
    // adds a last, highlighted "Now" point (CardStrings). A row with no date or title is left out (ItemIsComplete). The
    // look is only classes: Stories.uss turns the same event elements into a column or a row.
    public sealed class TimelineBlockView : IBlockView
    {
        public sealed class Event
        {
            public VisualElement Box;
            public VisualElement Dot;
            public VisualElement Rail;
            public Label Date;
            public Label Title;
            public CardTextView Text;
            public bool IsNow;
        }

        public VisualElement Root { get; }
        // The horizontal look's swipe track (the vertical look lays the events straight into Root)
        public ScrollView Track { get; }
        public IReadOnlyList<Event> Events => _shown;

        private readonly List<Event> _pool = new();
        private readonly List<Event> _shown = new();
        private string _variantClass;

        public TimelineBlockView()
        {
            Root = new VisualElement { name = "card-timeline" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-timeline");
            // - no desktop scroll bar: a finger swipes the track (the card's own stack hides its bar the same way)
            Track = new ScrollView(ScrollViewMode.Horizontal)
            {
                name = "card-timeline-track",
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
                verticalScrollerVisibility = ScrollerVisibility.Hidden,
            };
            Track.AddToClassList("card-timeline__track");
            Track.contentContainer.AddToClassList("card-timeline__track-content");
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _variantClass = "card-timeline--" + context.Variant;
            Root.AddToClassList(_variantClass);
            bool horizontal = context.Variant == BuiltInBlocks.TimelineHorizontal;
            var parent = horizontal ? Track.contentContainer : Root;
            if (horizontal) Root.Add(Track);

            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            var rowFields = BuiltInBlocks.Timeline.Field(BuiltInBlocks.TimelineItemsField).ItemFields;
            foreach (var item in read.Items(BuiltInBlocks.TimelineItemsField))
            {
                if (!BlockFieldReader.ItemIsComplete(item, rowFields)) continue;
                var paragraphs = GlossaryMarkup.Paragraphs(read.ItemText(item, BuiltInBlocks.TimelineTextField));
                Show(parent, read.ItemText(item, BuiltInBlocks.TimelineDateField), read.ItemText(item, BuiltInBlocks.TimelineTitleField), paragraphs, context.Glossary, isNow: false);
            }
            if (read.Flag(BuiltInBlocks.TimelineHighlightNowField) && _shown.Count > 0)
                Show(parent, context.Strings?.Get(CardStrings.Keys.TimelineNow) ?? "", "", System.Array.Empty<string>(), context.Glossary, isNow: true);
            // - the line runs from each dot to the next one: none after the last
            for (int i = 0; i < _shown.Count; i++)
                _shown[i].Rail.style.visibility = i < _shown.Count - 1 ? Visibility.Visible : Visibility.Hidden;
            Track.scrollOffset = UnityEngine.Vector2.zero;
        }

        public void Unbind()
        {
            foreach (var e in _shown)
            {
                e.Text.Clear();
                e.Box.RemoveFromHierarchy();
            }
            _shown.Clear();
            Track.RemoveFromHierarchy();
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        private void Show(VisualElement parent, string date, string title, IReadOnlyList<string> paragraphs, CardGlossary glossary, bool isNow)
        {
            var e = Take(_shown.Count);
            e.IsNow = isNow;
            e.Box.EnableInClassList("card-timeline__event--now", isNow);
            e.Date.text = date;
            e.Title.text = title;
            e.Title.style.display = title.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            e.Text.Bind(paragraphs, glossary);
            e.Text.Root.style.display = paragraphs.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            parent.Add(e.Box);
            _shown.Add(e);
        }

        private Event Take(int index)
        {
            while (_pool.Count <= index)
            {
                var e = new Event
                {
                    Box = new VisualElement(), Dot = new VisualElement(), Rail = new VisualElement(), Date = new Label(), Title = new Label(),
                    Text = new CardTextView("card-timeline__paragraph"),
                };
                e.Box.AddToClassList("card-timeline__event");
                var marker = new VisualElement();
                marker.AddToClassList("card-timeline__marker");
                e.Dot.AddToClassList("card-timeline__dot");
                e.Rail.AddToClassList("card-timeline__rail");
                marker.Add(e.Dot);
                marker.Add(e.Rail);
                var body = new VisualElement();
                body.AddToClassList("card-timeline__body");
                e.Date.AddToClassList("card-timeline__date");
                e.Title.AddToClassList("card-timeline__title");
                body.Add(e.Date);
                body.Add(e.Title);
                body.Add(e.Text.Root);
                e.Box.Add(marker);
                e.Box.Add(body);
                _pool.Add(e);
            }
            return _pool[index];
        }
    }
}
