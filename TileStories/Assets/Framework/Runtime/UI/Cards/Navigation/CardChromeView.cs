using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // Shows and hides the card's chrome -- the top navigation strip and the bottom area (the sticky CTA footer and, under it, the bottom
    // navigation slot: one area, PoiCardSheetView.BottomArea) -- as CardChromeRule decides (_3.2 section 4.3). Views only: it measures what the rule needs, keeps the scroll's
    // memory (CardChromeRule.ScrollState) and toggles one class per part. A hidden part is taken OUT OF THE FLOW (position: absolute),
    // never out of the layout: the stack gets its room back at once, and the part keeps a measured height -- which the rule needs to
    // know whether it may come back. Re-decided when the stop changes, the stack scrolls, or the header, the chrome or the content changes
    // size (a collapsing header gives a Half its strip back). The sheet owns one, beside its NavSlots.
    public sealed class CardChromeView
    {
        // The top strip, hidden: out of the flow and invisible (PoiCard.uss)
        public const string TopHiddenClass = "poi-card__nav-top--hidden";
        // The bottom area, hidden: out of the flow and slid below the sheet's clipped bottom edge, over --ts-duration (PoiCard.uss)
        public const string BottomHiddenClass = "poi-card__bottom--hidden";
        // The bottom bar inside a shown area, hidden (Show Navigation At = Full Only at Half): out of the flow and invisible, so the
        // footer keeps its place and the bar its measured height (PoiCard.uss)
        public const string BottomNavHiddenClass = "poi-card__nav-bottom--hidden";
        // The footer inside a shown area, hidden while the hero shows its action (CardChromeRule layer 4): out of the flow and invisible,
        // so the bottom bar keeps its place and the footer its measured height (PoiCard.uss)
        public const string FooterHiddenClass = "poi-card__footer--hidden";
        // Reduce motion: the bottom area jumps instead of sliding
        public const string InstantClass = "poi-card--chrome-instant";
        // On the sheet while the top strip shows: the stack's first block keeps a gap under the strip's hairline (PoiCard.uss)
        public const string TopShownClass = "poi-card--nav-top-shown";

        // What the last decision showed: the strip, the bottom area, the bottom bar inside it
        public bool TopShown { get; private set; }
        public bool BottomShown { get; private set; }
        public bool BottomNavShown { get; private set; }
        public bool FooterShown { get; private set; }
        // The scroll's memory (how far the reader moved in one direction, whether that hid the bottom area)
        public CardChromeRule.ScrollState Scroll { get; private set; }

        private readonly PoiCardSheetView _sheet;
        private CardNavigationSettings _nav;
        private float _lastOffset;
        // The bottom area just came back at the end: the next viewport resize keeps the end in view (CardChromeRule.KeepTheEnd)
        private bool _keepTheEnd;
        // A finger is on the stack, and the viewport it shrank from when the area came back under it (NaN: nothing waits for the finger)
        private bool _fingerOnStack;
        private float _endToKeepFrom = float.NaN;

        public CardChromeView(PoiCardSheetView sheet)
        {
            _sheet = sheet;
            var stack = sheet.Stack;
            stack.Scroll.verticalScroller.valueChanged += OnScrolled;
            foreach (var part in new[] { stack.HeaderSlot, sheet.Slots.Top, stack.Footer, sheet.Slots.Bottom, stack.Scroll.contentContainer })
                part.RegisterCallback<GeometryChangedEvent>(_ => Apply());
            // - registered after the ScrollView's own handler, so its range is already the new one when the end is kept
            stack.Scroll.contentViewport.RegisterCallback<GeometryChangedEvent>(OnViewportResized);
            // - a dragging ScrollView sets its offset from the finger on every move: an end kept under a finger is undone on the next move and
            //   the jump back reads as a fling up the card, so the end waits for the finger to lift
            stack.Scroll.RegisterCallback<PointerDownEvent>(_ => _fingerOnStack = true, TrickleDown.TrickleDown);
            stack.Scroll.RegisterCallback<PointerUpEvent>(_ => FingerLifted(), TrickleDown.TrickleDown);
            stack.Scroll.RegisterCallback<PointerCancelEvent>(_ => FingerLifted(), TrickleDown.TrickleDown);
        }

        // A card was shown (or rebound by a live edit, a reveal, a language switch): its settings, a fresh scroll memory
        public void Bind(CardNavigationSettings nav, bool reduceMotion)
        {
            _nav = nav;
            Scroll = default;
            _keepTheEnd = false;
            _endToKeepFrom = float.NaN;
            _lastOffset = _sheet.Stack.Scroll.scrollOffset.y;
            _sheet.Root.EnableInClassList(InstantClass, reduceMotion);
            Apply();
        }

        // The card closed: no settings, nothing remembered
        public void Unbind()
        {
            _nav = null;
            Scroll = default;
            _keepTheEnd = false;
            _endToKeepFrom = float.NaN;
            _lastOffset = 0f;
            Apply();
        }

        // Ask the rule again with what is on screen now, and show / hide each part as it says
        public void Apply()
        {
            var stack = _sheet.Stack;
            var header = stack.HeaderSlot;
            var input = new CardChromeRule.Input
            {
                Stop = _sheet.Stop,
                ShowAt = _nav?.show_at,
                HasTop = _sheet.Slots.TopView != null,
                HasFooter = stack.Footer.childCount > 0,
                HasBottomNav = _sheet.Slots.BottomView != null,
                ScrollHidesBottom = Scroll.BottomHidden,
                // - the REST height of the stop, never the animated one, so a rising sheet does not flip the chrome on its way
                SheetHeight = _sheet.Stops.HeightOf(_sheet.Stop),
                // - everything above the strip: the grab zone and the header (its pinned part sits inside it), in the sheet's own frame
                HeaderHeight = header.layout.yMax + header.resolvedStyle.marginBottom,
                TopHeight = _sheet.Slots.Top.layout.height,
                FooterHeight = stack.Footer.layout.height,
                BottomNavHeight = _sheet.Slots.Bottom.layout.height,
                // - the blocks' own height: the gap under a shown strip is the chrome's, so showing it cannot change this decision
                ContentHeight = stack.Scroll.contentContainer.layout.height - stack.Scroll.contentContainer.resolvedStyle.paddingTop,
                // - the hero holds the actions while it is in view (CardChromeRule's 4th layer); its edge in content coordinates
                HeroBottom = stack.HeaderView?.HeroBottom() ?? float.NaN,
                ScrollOffset = stack.Scroll.scrollOffset.y,
            };
            var chrome = CardChromeRule.Decide(input);
            // - the area comes back at the end: its height leaves the viewport on the next layout, and the end must stay in view above it
            var scroll = stack.Scroll;
            if (chrome.Bottom && !BottomShown && CardChromeRule.AtEnd(input.ScrollOffset, scroll.contentContainer.layout.height, scroll.contentViewport.layout.height))
                _keepTheEnd = true;
            TopShown = chrome.Top;
            BottomShown = chrome.Bottom;
            BottomNavShown = chrome.BottomNav;
            FooterShown = chrome.Footer;
            _sheet.Slots.Top.EnableInClassList(TopHiddenClass, !chrome.Top);
            _sheet.Root.EnableInClassList(TopShownClass, chrome.Top);
            _sheet.BottomArea.EnableInClassList(BottomHiddenClass, !chrome.Bottom);
            // - the bar keeps its place while the whole area slides away (it goes with it); hidden on its own only where the area stays
            _sheet.Slots.Bottom.EnableInClassList(BottomNavHiddenClass, chrome.Bottom && !chrome.BottomNav);
            stack.Footer.EnableInClassList(FooterHiddenClass, chrome.Bottom && !chrome.Footer);
        }

        // The stack scrolled: one more move for the scroll's memory, then a new decision
        private void OnScrolled(float offset)
        {
            float delta = offset - _lastOffset;
            _lastOffset = offset;
            var scroll = _sheet.Stack.Scroll;
            // - the end is the range without the area: once back at the end it stays within its own height of it
            float area = BottomShown ? _sheet.BottomArea.layout.height : 0f;
            Scroll = CardChromeRule.Next(Scroll, CardChromeRule.ReaderDelta(delta, _sheet.Nav.IsJumping), CardChromeRule.AtTop(offset),
                CardChromeRule.AtEnd(offset, scroll.contentContainer.layout.height, scroll.contentViewport.layout.height, area), _nav?.hide_on_scroll);
            Apply();
        }

        // The viewport changed size: right after the bottom area came back at the end, the stack follows it so the end stays in view --
        // at once, or once the finger dragging the stack lifts
        private void OnViewportResized(GeometryChangedEvent evt)
        {
            if (!_keepTheEnd) return;
            _keepTheEnd = false;
            if (_fingerOnStack) _endToKeepFrom = evt.oldRect.height;
            else KeepTheEnd(evt.oldRect.height);
        }

        // The finger left the stack: an end that waited for it is kept on the next frame, after the ScrollView took the release
        private void FingerLifted()
        {
            _fingerOnStack = false;
            if (float.IsNaN(_endToKeepFrom)) return;
            float from = _endToKeepFrom;
            _endToKeepFrom = float.NaN;
            _sheet.Stack.Scroll.schedule.Execute(() => KeepTheEnd(from));
        }

        // The stack rests at the end of its range once more if it sat at the end it had before the viewport shrank from `viewportBefore`
        private void KeepTheEnd(float viewportBefore)
        {
            var scroll = _sheet.Stack.Scroll;
            float viewport = scroll.contentViewport.layout.height;
            float kept = CardChromeRule.KeepTheEnd(scroll.scrollOffset.y, scroll.contentContainer.layout.height, viewportBefore, viewport);
            if (!Mathf.Approximately(kept, scroll.scrollOffset.y)) scroll.scrollOffset = new Vector2(scroll.scrollOffset.x, kept);
        }
    }
}
