using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{

    // The full-screen view over the card (_3.1 section 4, "Takeover"; first user: the gallery's lightbox, step 7). It
    // covers the whole card layer above the sheet: the Page Chrome's top bar (a Back chevron and the one-line breadcrumb, PageTopBar), one chip per sibling page, and the page itself,
    // drawn by whoever opened it (TakeoverPageDrawer). The sheet underneath is never rebound, so closing returns to the same stop and scroll
    // position. The view loads through its OWN media scope: switching page gives the previous page's files back, closing gives back everything.
    //   - the breadcrumb (_3.2 step 16, _3.3 S2c) is the card's title, then -- for a block of the card's map -- its SECTION as tappable words
    //     (a whole touch target: SectionTapped, which the sheet turns into the card's one jump), then the page's own name, bold, which follows
    //     the page shown ("Castle > Media > 360") -- left out while the page shows a big title of its own (ShowPageName: a story); ONE line,
    //     the middle part cut with an ellipsis first (PageCrumbFitRule); a wall or a block may hide it (PageChromeChoice), the Back arrow then
    //     stands alone
    //   - the chips are ONE line clipped at the screen's width (NavStrip: the current chip is centred, so the next one is always a tap away), each a whole touch
    //     target named by its page; the current one is bold with a ring as well as filled -- never colour alone
    //   - Back (the button, the system Back: GoBack) returns to what the view was opened over when its opener named it -- a picture's viewer
    //     or a video's player opened from a block's page goes back to that page (_3.3 S2b); Close (a jump, the card closing) never returns
    // Plain C#: built into the layer it is handed; Media.uss styles it.
    public sealed class TakeoverView : ITakeoverPage
    {
        // What a tap on the breadcrumb's section reports as its element (nav_jump's Variant)
        public const string CrumbElementKey = "breadcrumb";
        public const string CurrentChipClass = "card-takeover__chip--current";
        // A chip that shows its page's icon (and, while it is the page being read, its name) instead of a page number
        public const string IconChipClass = "card-takeover__chip--icon";

        public VisualElement Root { get; }
        // The icon and the words inside chip `page` (null before the chip exists): what a test reads to know what a chip says
        public VisualElement ChipIcon(int page) => page >= 0 && page < _chipIcons.Count ? _chipIcons[page] : null;
        public Label ChipWords(int page) => page >= 0 && page < _chipWords.Count ? _chipWords[page] : null;
        // The top bar (the Page Chrome's): the Back chevron and the breadcrumb -- its row, the card's title, the section (hidden for a page of
        // no section) and the page's name
        public PageTopBar Bar { get; }
        public Button Back => Bar.Back;
        public VisualElement Crumb => Bar.Crumb;
        public Label CrumbTitle => Bar.Point;
        public Button CrumbSection => Bar.Section;
        public Label CrumbSectionWords => Bar.SectionWords;
        public Label CrumbName => Bar.Name;
        // The whole breadcrumb as a visitor reads it ("St George's Castle > Media > 360"; empty while the chrome hides it)
        public string CrumbText => Bar.CrumbText;
        public bool SectionShown => Bar.SectionShown;
        public bool NameRepeatsSection => Bar.NameRepeatsSection;
        // What the open page's chrome shows (the wall's default, or the block's own choice: PageChromeRule)
        public PageChromeChoice Chrome { get; private set; } = PageChromeChoice.Both;
        // The row of chips (one per page) inside its clipped strip
        public VisualElement Chips => _chipStrip.Row;
        public VisualElement ChipStrip => _chipStrip.Viewport;
        public VisualElement Page { get; }

        public bool IsOpen { get; private set; }
        public int PageIndex { get; private set; }
        public int PageCount { get; private set; }
        // How many files the open page holds (0 once closed: nothing kept)
        public int HeldMedia => _media?.HeldCount ?? 0;

        // The visitor tapped the breadcrumb's section
        public event Action SectionTapped;

        private readonly List<Button> _chips = new();
        // Per chip: the kind's icon and the words of the page being read (_3.2 step 17a); a page with no icon keeps its number as its text
        private readonly List<VisualElement> _chipIcons = new();
        private readonly List<Label> _chipWords = new();
        private readonly NavStrip _chipStrip;
        // What the open page asked to run when it leaves the screen (WhenPageLeaves), run before its files are given back
        private readonly List<Action> _leaving = new();
        private ScopedMediaSource _media;
        private TakeoverPageDrawer _draw;
        private Func<int, string> _nameOfPage;
        // What Back returns to (null = the card): set by the opener of THIS opening, forgotten by any close
        private Action _returnTo;

        // Whether Back returns to something over the card (a block's page) rather than to the card itself
        public bool ReturnsToAPage => IsOpen && _returnTo != null;

        public TakeoverView(VisualElement layer)
        {
            Root = new VisualElement { name = "poi-card-takeover" };
            // - the card's own tokens live on these classes (CardTokens.uss), the theme's arrive from the app root; the view is the
            //   sheet's sibling, not its child
            Root.AddToClassList("ts-card");
            Root.AddToClassList("ts-theme-default");
            Root.AddToClassList("card-takeover");
            Bar = new PageTopBar(GoBack, () => SectionTapped?.Invoke());
            _chipStrip = new NavStrip("poi-card-takeover-chips", "card-takeover__chips-strip", "card-takeover__chips");
            // - the chips are reused from one opening to the next: right after Open they still carry the LAST layout (the row had shrunk to
            //   nothing when they left it), so a centring asked then measures a stale row. Centre again once the row is really laid out (found
            //   by the Portuguese render: the second opening showed chips 1-6 with the current one, 11, out of view). A translate is no layout
            //   change, so this never calls itself again
            Chips.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                if (IsOpen && PageCount > 1) _chipStrip.ShowCentred(_chips[PageIndex]);
            });
            Page = new VisualElement { name = "poi-card-takeover-page" };
            Page.AddToClassList("card-takeover__page");
            Root.Add(Bar);
            Root.Add(Page);
            Root.Add(_chipStrip.Viewport);
            layer.Add(Root);
            Root.style.display = DisplayStyle.None;
        }

        // Open over the card: the breadcrumb starts with `title` and ends with `nameOfPage(page)` for the page shown; `pageCount` pages (a chip
        // each when more than one, named by its page), at `start`. No section until ShowSection names one. `iconOfPage` (optional) gives a
        // CardIcons key per page: such a chip shows that icon instead of a number, and the chip of the page being read adds the page's name
        // beside it (a block's siblings: its kind's icon says what each one is); null or a null key keeps the page number. `returnTo` (optional):
        // what Back opens again once this view closes (the block's page it was opened over). `chrome` (optional): what the Page Chrome shows
        // until a page of a block says otherwise (ApplyChrome); null = both parts
        public void Open(string title, Func<int, string> nameOfPage, int pageCount, int start, IMediaSource cardMedia, TakeoverPageDrawer draw,
            CardStrings strings, Func<int, string> iconOfPage = null, Action returnTo = null, PageChromeChoice? chrome = null)
        {
            Close();
            if (pageCount <= 0 || draw == null) return;
            _returnTo = returnTo;
            _media = new ScopedMediaSource(cardMedia);
            _draw = draw;
            _nameOfPage = nameOfPage;
            PageCount = pageCount;
            Bar.SetTrail(title, null, "");
            Bar.ShowName(true);
            ApplyChrome(chrome ?? PageChromeChoice.Both);
            Bar.SetBackName(strings?.Get(CardStrings.Keys.PageBack) ?? "");
            for (int i = 0; i < pageCount; i++)
            {
                if (i >= _chips.Count)
                {
                    int page = i;
                    var chip = new Button(() => ShowPage(page));
                    chip.AddToClassList("card-takeover__chip");
                    var icon = CardIcons.Create();
                    icon.AddToClassList("card-takeover__chip-icon");
                    var words = new Label { pickingMode = PickingMode.Ignore };
                    words.AddToClassList("card-takeover__chip-words");
                    chip.Add(icon);
                    chip.Add(words);
                    _chips.Add(chip);
                    _chipIcons.Add(icon);
                    _chipWords.Add(words);
                }
                string iconKey = iconOfPage?.Invoke(i);
                bool hasIcon = !string.IsNullOrEmpty(iconKey);
                // - an icon (the page's kind) when the opener gave one, else a page number: nothing to translate either way; the page's own
                //   name is the chip's tooltip (and accessible name), whatever it shows
                _chips[i].text = hasIcon ? "" : (i + 1).ToString();
                _chips[i].EnableInClassList(IconChipClass, hasIcon);
                CardIcons.SetKey(_chipIcons[i], hasIcon ? iconKey : null);
                _chipIcons[i].style.display = hasIcon ? DisplayStyle.Flex : DisplayStyle.None;
                _chipWords[i].text = hasIcon ? NameOf(i) : "";
                _chipWords[i].style.display = DisplayStyle.None;
                _chips[i].tooltip = NameOf(i);
                Chips.Add(_chips[i]);
            }
            _chipStrip.Reset();
            _chipStrip.Viewport.style.display = pageCount > 1 ? DisplayStyle.Flex : DisplayStyle.None;
            IsOpen = true;
            Root.style.display = DisplayStyle.Flex;
            ShowPage(start);
        }

        // The breadcrumb's middle segment: the section the page belongs to in the card's map (its words), or none (null words)
        public void ShowSection(string words) => Bar.SetTrail(CrumbTitle.text, words, CrumbName.text);

        // What the Page Chrome shows now: the breadcrumb beside the Back arrow (the foot's Previous / Next row is BlockFullView's)
        public void ApplyChrome(PageChromeChoice chrome)
        {
            Chrome = chrome;
            Bar.ShowBreadcrumb(chrome.Breadcrumb);
        }

        // Whether the breadcrumb ends with the page's name: off while the page shows a big title of its own (the trail is then the context,
        // "point > section"); every opening starts with it on
        public void ShowPageName(bool shown) => Bar.ShowName(shown);

        // The name of page `page` (what its chip, the breadcrumb and a foot card say)
        public string NameOf(int page) => _nameOfPage?.Invoke(page) ?? "";

        // Show page `page` (clamped): the previous page's files go back first; the breadcrumb names it and its chip is marked and brought into view
        public void ShowPage(int page)
        {
            if (!IsOpen) return;
            PageIndex = UnityEngine.Mathf.Clamp(page, 0, PageCount - 1);
            LeavePage();
            _media.ReleaseAll();
            Page.Clear();
            Bar.SetName(NameOf(PageIndex));
            _draw(this, Page, _media, PageIndex);
            for (int i = 0; i < PageCount; i++)
            {
                _chips[i].EnableInClassList(CurrentChipClass, i == PageIndex);
                // - the page being read says what it is (its name beside its icon); the others stay icons, so seven fit a phone's width
                if (_chips[i].ClassListContains(IconChipClass)) _chipWords[i].style.display = i == PageIndex ? DisplayStyle.Flex : DisplayStyle.None;
            }
            if (PageCount > 1) _chipStrip.ShowCentred(_chips[PageIndex]);
        }

        // Turn `direction` pages (+1 the next, -1 the previous; a page swipe): nothing past the first or the last page
        public void Turn(int direction)
        {
            int page = PageIndex + direction;
            if (IsOpen && page >= 0 && page < PageCount) ShowPage(page);
        }

        // The page being drawn asks for `release` to run when it leaves the screen -- another page shown, or the view closed -- BEFORE the
        // view's media scope gives its files back (a block view unbinds first, as the stack does). Explicit, not DetachFromPanelEvent: that
        // never fires for a page that was never on a panel (Edit Mode), and its order against the release is not the view's to decide.
        // While it runs, IsOpen still tells a TURN (true: another page comes) from the view closing (false)
        public void WhenPageLeaves(Action release)
        {
            if (IsOpen && release != null) _leaving.Add(release);
        }

        private void LeavePage()
        {
            // - a copy: a release that closes or reopens the view adds to a fresh list
            var leaving = _leaving.ToArray();
            _leaving.Clear();
            foreach (var release in leaving) release();
        }

        // The visitor's Back (the button, the system Back): the view closes, then what it was opened over opens again (a block's page), else
        // the card shows as it was
        public void GoBack()
        {
            var returnTo = _returnTo;
            Close();
            returnTo?.Invoke();
        }

        // Back to the card: every file of the view given back, the card as it was (nothing reopened: a jump, the card closing)
        public void Close()
        {
            _returnTo = null;
            if (!IsOpen) return;
            IsOpen = false;
            LeavePage();
            _media.ReleaseAll();
            Page.Clear();
            foreach (var chip in _chips) chip.RemoveFromHierarchy();
            _draw = null;
            _nameOfPage = null;
            Root.style.display = DisplayStyle.None;
        }
    }
}
