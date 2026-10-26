using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The story_chapters block (_3.1 Tier 1 group B): a story told one chapter at a time. segmented: a bar of one segment per
    // chapter (the ones read so far filled), "Chapter 2 of 3" (CardStrings), the chapter's title and text (a CardTextView),
    // then Previous / Next buttons. Every chapter is laid out side by side in a clipped row (only the one shown is visible), so the block is as
    // tall as its TALLEST chapter and Previous / Next never move as the chapters turn: a finger that taps Next again and again stays in one
    // place (_3.2 step 14). Which chapter shows is VIEW state: every bind starts at the first chapter and nothing is
    // stored (_3.1: CardLocalState is for answers and collections, not for where a reader is). A row with no title or text
    // is left out (ItemIsComplete); a single chapter needs no buttons. Only classes here; Stories.uss draws it.
    public sealed class StoryChaptersBlockView : IBlockView
    {
        public VisualElement Root { get; }
        public VisualElement Progress { get; }
        public Label Counter { get; }
        // The words of the chapter shown: its title and its text
        public Label Title => _pages.Shown[Index].Title;
        public CardTextView Body => _pages.Shown[Index].Body;
        // The clipped row of every chapter, side by side; it slides to the chapter shown
        public VisualElement Chapters { get; }
        public VisualElement Track { get; }
        public VisualElement Nav { get; }
        public Button Previous { get; }
        public Button Next { get; }
        public IReadOnlyList<VisualElement> Segments => _segments.Shown;
        // The chapter shown (0-based) and how many there are
        public int Index { get; private set; }
        public int Count => _chapters.Count;

        private readonly List<(string Title, IReadOnlyList<string> Body)> _chapters = new();
        // One chapter in the row: its title and its text
        private sealed class ChapterPage
        {
            public readonly VisualElement Root = new();
            public readonly Label Title = new();
            public readonly CardTextView Body = new("card-story__paragraph");

            public ChapterPage()
            {
                Root.AddToClassList("card-story__chapter");
                Title.AddToClassList("card-story__title");
                Root.Add(Title);
                Root.Add(Body.Root);
            }
        }

        private readonly ElementPool<ChapterPage> _pages = new(_ => new ChapterPage(), page =>
        {
            page.Body.Clear();
            page.Root.RemoveFromHierarchy();
        });
        private readonly ElementPool<VisualElement> _segments = new(_ => NewSegment(), segment => segment.RemoveFromHierarchy());
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
            Chapters = new VisualElement();
            Chapters.AddToClassList("card-story__chapters");
            Track = new VisualElement();
            Track.AddToClassList("card-story__track");
            Chapters.Add(Track);
            Nav = new VisualElement();
            Nav.AddToClassList("card-story__nav");
            (Previous, _previousLabel) = NavButton("card-story__previous", () => Show(Index - 1));
            (Next, _nextLabel) = NavButton("card-story__next", () => Show(Index + 1));
            Nav.Add(Previous);
            Nav.Add(Next);
            Root.Add(Progress);
            Root.Add(Counter);
            Root.Add(Chapters);
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
                Progress.Add(_segments.Take());
                var page = _pages.Take();
                page.Title.text = _chapters[i].Title;
                page.Body.Bind(_chapters[i].Body, _glossary);
                Track.Add(page.Root);
            }
            _previousLabel.text = _strings?.Get(CardStrings.Keys.StoryPrevious) ?? "";
            _nextLabel.text = _strings?.Get(CardStrings.Keys.StoryNext) ?? "";
            Nav.style.display = _chapters.Count > 1 ? DisplayStyle.Flex : DisplayStyle.None;
            Show(0);
        }

        public void Unbind()
        {
            _segments.ReleaseAll();
            _pages.ReleaseAll();
            _chapters.Clear();
            Index = 0;
            Track.style.translate = StyleKeyword.Null;
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        // Show chapter `index` (clamped): its words, the bar filled up to it, only the buttons that go somewhere
        public void Show(int index)
        {
            if (_chapters.Count == 0) return;
            Index = System.Math.Clamp(index, 0, _chapters.Count - 1);
            // - the row slides to the chapter shown; the others stay laid out (their height keeps the buttons in place) but out of sight and
            //   out of reach of a tap
            Track.style.translate = new Translate(Length.Percent(-100f * Index), 0f);
            var pages = _pages.Shown;
            for (int i = 0; i < pages.Count; i++) pages[i].Root.style.visibility = i == Index ? Visibility.Visible : Visibility.Hidden;
            string format = _strings?.Get(CardStrings.Keys.StoryChapterOf) ?? "";
            Counter.text = string.Format(System.Globalization.CultureInfo.InvariantCulture, format, Index + 1, _chapters.Count);
            var segments = _segments.Shown;
            for (int i = 0; i < segments.Count; i++)
            {
                segments[i].EnableInClassList("card-story__segment--read", i <= Index);
                segments[i].EnableInClassList("card-story__segment--current", i == Index);
            }
            // - hidden, not removed: each button keeps its side of the row (Previous appearing never pushes Next sideways)
            Previous.style.visibility = Index > 0 ? Visibility.Visible : Visibility.Hidden;
            Next.style.visibility = Index < _chapters.Count - 1 ? Visibility.Visible : Visibility.Hidden;
        }

        // One segment of the progress bar (a chapter)
        private static VisualElement NewSegment()
        {
            var segment = new VisualElement();
            segment.AddToClassList("card-story__segment");
            return segment;
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
