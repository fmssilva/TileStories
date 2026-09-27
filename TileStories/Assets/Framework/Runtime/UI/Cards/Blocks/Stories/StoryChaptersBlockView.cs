using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The story_chapters block (_3.1 Tier 1 group B): a story told one chapter at a time. segmented: a bar of one segment per
    // chapter (the ones read so far filled), "Chapter 2 of 3" (CardStrings), the chapter's title and text (a CardTextView),
    // then Previous / Next buttons. Which chapter shows is VIEW state: every bind starts at the first chapter and nothing is
    // stored (_3.1: CardLocalState is for answers and collections, not for where a reader is). A row with no title or text
    // is left out (ItemIsComplete); a single chapter needs no buttons. Only classes here; Stories.uss draws it.
    public sealed class StoryChaptersBlockView : IBlockView
    {
        public VisualElement Root { get; }
        public VisualElement Progress { get; }
        public Label Counter { get; }
        public Label Title { get; }
        public CardTextView Body { get; }
        public VisualElement Nav { get; }
        public Button Previous { get; }
        public Button Next { get; }
        public IReadOnlyList<VisualElement> Segments => _segments;
        // The chapter shown (0-based) and how many there are
        public int Index { get; private set; }
        public int Count => _chapters.Count;

        private readonly List<(string Title, IReadOnlyList<string> Body)> _chapters = new();
        private readonly List<VisualElement> _segmentPool = new();
        private readonly List<VisualElement> _segments = new();
        private readonly Label _previousLabel;
        private readonly Label _nextLabel;
        private CardStrings _strings;
        private CardGlossary _glossary;
        private string _variantClass;

        public StoryChaptersBlockView()
        {
            Root = new VisualElement { name = "card-story" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-story");
            Progress = new VisualElement();
            Progress.AddToClassList("card-story__progress");
            Counter = new Label();
            Counter.AddToClassList("card-story__counter");
            Title = new Label();
            Title.AddToClassList("card-story__title");
            Body = new CardTextView("card-story__paragraph");
            Nav = new VisualElement();
            Nav.AddToClassList("card-story__nav");
            (Previous, _previousLabel) = NavButton("card-story__previous", () => Show(Index - 1));
            (Next, _nextLabel) = NavButton("card-story__next", () => Show(Index + 1));
            Nav.Add(Previous);
            Nav.Add(Next);
            Root.Add(Progress);
            Root.Add(Counter);
            Root.Add(Title);
            Root.Add(Body.Root);
            Root.Add(Nav);
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _variantClass = "card-story--" + context.Variant;
            Root.AddToClassList(_variantClass);
            _strings = context.Strings;
            _glossary = context.Glossary;
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            var rowFields = BuiltInBlocks.StoryChapters.Field(BuiltInBlocks.StoryChaptersItemsField).ItemFields;
            foreach (var item in read.Items(BuiltInBlocks.StoryChaptersItemsField))
                if (BlockFieldReader.ItemIsComplete(item, rowFields))
                    _chapters.Add((read.ItemText(item, BuiltInBlocks.StoryChaptersTitleField),
                        GlossaryMarkup.Paragraphs(read.ItemText(item, BuiltInBlocks.StoryChaptersBodyField))));
            for (int i = 0; i < _chapters.Count; i++)
            {
                while (_segmentPool.Count <= i)
                {
                    var segment = new VisualElement();
                    segment.AddToClassList("card-story__segment");
                    _segmentPool.Add(segment);
                }
                Progress.Add(_segmentPool[i]);
                _segments.Add(_segmentPool[i]);
            }
            _previousLabel.text = _strings?.Get(CardStrings.Keys.StoryPrevious) ?? "";
            _nextLabel.text = _strings?.Get(CardStrings.Keys.StoryNext) ?? "";
            Nav.style.display = _chapters.Count > 1 ? DisplayStyle.Flex : DisplayStyle.None;
            Show(0);
        }

        public void Unbind()
        {
            foreach (var segment in _segments) segment.RemoveFromHierarchy();
            _segments.Clear();
            _chapters.Clear();
            Body.Clear();
            Index = 0;
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        // Show chapter `index` (clamped): its words, the bar filled up to it, only the buttons that go somewhere
        public void Show(int index)
        {
            if (_chapters.Count == 0) return;
            Index = System.Math.Clamp(index, 0, _chapters.Count - 1);
            var (title, body) = _chapters[Index];
            Title.text = title;
            Body.Bind(body, _glossary);
            string format = _strings?.Get(CardStrings.Keys.StoryChapterOf) ?? "";
            Counter.text = string.Format(System.Globalization.CultureInfo.InvariantCulture, format, Index + 1, _chapters.Count);
            for (int i = 0; i < _segments.Count; i++)
            {
                _segments[i].EnableInClassList("card-story__segment--read", i <= Index);
                _segments[i].EnableInClassList("card-story__segment--current", i == Index);
            }
            // - hidden, not removed: each button keeps its side of the row (Previous appearing never pushes Next sideways)
            Previous.style.visibility = Index > 0 ? Visibility.Visible : Visibility.Hidden;
            Next.style.visibility = Index < _chapters.Count - 1 ? Visibility.Visible : Visibility.Hidden;
        }

        private static (Button, Label) NavButton(string className, System.Action onClick)
        {
            var button = new Button(onClick);
            button.AddToClassList("card-story__button");
            // - the actions' pill shape (CardParts.uss), so every pill of the card is one shape
            button.AddToClassList("card-pill");
            button.AddToClassList(className);
            button.AddToClassList("card-tap");
            var label = new Label { pickingMode = PickingMode.Ignore };
            label.AddToClassList("card-story__button-label");
            button.Add(label);
            return (button, label);
        }
    }
}
