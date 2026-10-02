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
        public IReadOnlyList<Event> Events => _events.Shown;

        private readonly ElementPool<Event> _events = new(_ => NewEvent(), e =>
        {
            e.Text.Clear();
            // - a pooled event may come back in the vertical look: give it its USS width again
            e.Box.style.width = StyleKeyword.Null;
            e.Box.RemoveFromHierarchy();
        });
        private string _variantClass;
        private bool _horizontal;
        // Peek Next Card (card_settings.container.peek_next_card), read at bind: a live edit rebinds the card
        private bool _peekNext = true;
        private readonly List<VisualElement> _cells = new();

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
            // - the strip only has a width once the card lays it out (and a new one when the sheet changes): fit the events then
            Track.contentViewport.RegisterCallback<GeometryChangedEvent>(_ => FitEvents());
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _variantClass = "card-timeline--" + context.Variant;
            Root.AddToClassList(_variantClass);
            bool horizontal = context.Variant == BuiltInBlocks.TimelineHorizontal;
            _horizontal = horizontal;
            _peekNext = context.Settings?.container?.peek_next_card ?? true;
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
            if (read.Flag(BuiltInBlocks.TimelineHighlightNowField) && _events.Shown.Count > 0)
                Show(parent, context.Strings?.Get(CardStrings.Keys.TimelineNow) ?? "", "", System.Array.Empty<string>(), context.Glossary, isNow: true);
            // - the line runs from each dot to the next one: none after the last
            for (int i = 0; i < _events.Shown.Count; i++)
                _events.Shown[i].Rail.style.visibility = i < _events.Shown.Count - 1 ? Visibility.Visible : Visibility.Hidden;
            Track.scrollOffset = UnityEngine.Vector2.zero;
            FitEvents();
        }

        // Size the events for the swipe strip (CardTrackFit): whole events only, or -- with Peek Next Card -- whole events and a slice of the
        // next at rest; the swipe always ends on a whole last event. The narrowest an event may be is its USS min-width (the cell token).
        private void FitEvents()
        {
            if (!_horizontal) return;
            _cells.Clear();
            foreach (var e in _events.Shown) _cells.Add(e.Box);
            CardTrackFit.Apply(Track, _cells, _peekNext);
        }

        public void Unbind()
        {
            _events.ReleaseAll();
            _horizontal = false;
            Track.RemoveFromHierarchy();
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        private void Show(VisualElement parent, string date, string title, IReadOnlyList<string> paragraphs, CardGlossary glossary, bool isNow)
        {
            var e = _events.Take();
            e.IsNow = isNow;
            e.Box.EnableInClassList("card-timeline__event--now", isNow);
            e.Date.text = date;
            e.Title.text = title;
            e.Title.style.display = title.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            e.Text.Bind(paragraphs, glossary);
            e.Text.Root.style.display = paragraphs.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            parent.Add(e.Box);
        }

        // One event: its dot and rail beside the date, title and text
        private static Event NewEvent()
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
            return e;
        }
    }
}
