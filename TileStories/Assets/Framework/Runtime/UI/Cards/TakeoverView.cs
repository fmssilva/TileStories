using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // Draws page `page` of a full-screen view into `container`, loading its media through `media` (the view's own scope)
    public delegate void TakeoverPageDrawer(VisualElement container, IMediaSource media, int page);

    // The full-screen view over the card (_3.1 section 4, "Takeover"; first user: the gallery's lightbox, step 7). It
    // covers the whole card layer above the sheet: a bar with the way back and the breadcrumb ("St George's Castle >
    // Gallery"), one chip per sibling page, and the page itself, drawn by whoever opened it (TakeoverPageDrawer). The
    // sheet underneath is never rebound, so closing returns to the same stop and scroll position. The view loads through
    // its OWN media scope: switching page gives the previous page's files back, closing gives back everything.
    // Plain C#: built into the layer it is handed; Media.uss styles it.
    public sealed class TakeoverView
    {
        public VisualElement Root { get; }
        public Button Back { get; }
        public Label Crumb { get; }
        public VisualElement Chips { get; }
        public VisualElement Page { get; }

        public bool IsOpen { get; private set; }
        public int PageIndex { get; private set; }
        public int PageCount { get; private set; }
        // How many files the open page holds (0 once closed: nothing kept)
        public int HeldMedia => _media?.HeldCount ?? 0;

        private readonly List<Button> _chips = new();
        private ScopedMediaSource _media;
        private TakeoverPageDrawer _draw;

        public TakeoverView(VisualElement layer)
        {
            Root = new VisualElement { name = "poi-card-takeover" };
            // - the token set lives on these classes (CardTokens.uss); the view is the sheet's sibling, not its child
            Root.AddToClassList("ts-card");
            Root.AddToClassList("ts-theme-default");
            Root.AddToClassList("card-takeover");
            var bar = new VisualElement();
            bar.AddToClassList("card-takeover__bar");
            Back = new Button(Close) { name = "poi-card-takeover-back" };
            Back.AddToClassList("card-takeover__back");
            Back.AddToClassList("card-pill");
            Crumb = new Label { name = "poi-card-takeover-crumb" };
            Crumb.AddToClassList("card-takeover__crumb");
            bar.Add(Back);
            bar.Add(Crumb);
            Chips = new VisualElement();
            Chips.AddToClassList("card-takeover__chips");
            Page = new VisualElement { name = "poi-card-takeover-page" };
            Page.AddToClassList("card-takeover__page");
            Root.Add(bar);
            Root.Add(Page);
            Root.Add(Chips);
            layer.Add(Root);
            Root.style.display = DisplayStyle.None;
        }

        // Open over the card: `crumb` is the whole breadcrumb, `pageCount` pages (a chip each when more than one), at `start`
        public void Open(string crumb, int pageCount, int start, IMediaSource cardMedia, TakeoverPageDrawer draw, CardStrings strings)
        {
            Close();
            if (pageCount <= 0 || draw == null) return;
            _media = new ScopedMediaSource(cardMedia);
            _draw = draw;
            PageCount = pageCount;
            Crumb.text = crumb;
            Back.text = strings?.Get(CardStrings.Keys.TakeoverBack) ?? "";
            for (int i = 0; i < pageCount; i++)
            {
                if (i >= _chips.Count)
                {
                    int page = i;
                    var chip = new Button(() => ShowPage(page));
                    chip.AddToClassList("card-takeover__chip");
                    _chips.Add(chip);
                }
                // - a page number, not a word: nothing to translate
                _chips[i].text = (i + 1).ToString();
                Chips.Add(_chips[i]);
            }
            Chips.style.display = pageCount > 1 ? DisplayStyle.Flex : DisplayStyle.None;
            IsOpen = true;
            Root.style.display = DisplayStyle.Flex;
            ShowPage(start);
        }

        // Show page `page` (clamped): the previous page's files go back first
        public void ShowPage(int page)
        {
            if (!IsOpen) return;
            PageIndex = UnityEngine.Mathf.Clamp(page, 0, PageCount - 1);
            _media.ReleaseAll();
            Page.Clear();
            _draw(Page, _media, PageIndex);
            for (int i = 0; i < PageCount; i++) _chips[i].EnableInClassList("card-takeover__chip--current", i == PageIndex);
        }

        // Back to the card: every file of the view given back, the card as it was
        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            _media.ReleaseAll();
            Page.Clear();
            foreach (var chip in _chips) chip.RemoveFromHierarchy();
            _draw = null;
            Root.style.display = DisplayStyle.None;
        }
    }
}
