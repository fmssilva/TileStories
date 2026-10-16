using UnityEngine.UIElements;

namespace TileStories
{
    // Shows and hides the card's chrome -- the top navigation strip and the bottom area (the sticky CTA footer; the bottom navigation slot
    // joins it at step 7) -- as CardChromeRule decides (_3.2 section 4.3). Views only: it measures what the rule needs, keeps the scroll's
    // memory (CardChromeRule.ScrollState) and toggles one class per part. A hidden part is taken OUT OF THE FLOW (position: absolute),
    // never out of the layout: the stack gets its room back at once, and the part keeps a measured height -- which the rule needs to
    // know whether it may come back. Re-decided when the stop changes, the stack scrolls, or the header, the chrome or the content changes
    // size (a collapsing header gives a Half its strip back). The sheet owns one, beside its NavSlots.
    public sealed class CardChromeView
    {
        // The top strip, hidden: out of the flow and invisible (PoiCard.uss)
        public const string TopHiddenClass = "poi-card__nav-top--hidden";
        // The bottom area, hidden: out of the flow and slid below the sheet's clipped bottom edge, over --ts-duration (PoiCard.uss)
        public const string BottomHiddenClass = "poi-card__footer--hidden";
        // Reduce motion: the bottom area jumps instead of sliding
        public const string InstantClass = "poi-card--chrome-instant";
        // On the sheet while the top strip shows: the stack's first block keeps a gap under the strip's hairline (PoiCard.uss)
        public const string TopShownClass = "poi-card--nav-top-shown";

        // What the last decision showed
        public bool TopShown { get; private set; }
        public bool BottomShown { get; private set; }
        // The scroll's memory (how far the reader moved in one direction, whether that hid the bottom area)
        public CardChromeRule.ScrollState Scroll { get; private set; }

        private readonly PoiCardSheetView _sheet;
        private CardNavigationSettings _nav;
        private float _lastOffset;

        public CardChromeView(PoiCardSheetView sheet)
        {
            _sheet = sheet;
            var stack = sheet.Stack;
            stack.Scroll.verticalScroller.valueChanged += OnScrolled;
            foreach (var part in new[] { stack.HeaderSlot, sheet.Slots.Top, stack.Footer, stack.Scroll.contentContainer })
                part.RegisterCallback<GeometryChangedEvent>(_ => Apply());
        }

        // A card was shown (or rebound by a live edit, a reveal, a language switch): its settings, a fresh scroll memory
        public void Bind(CardNavigationSettings nav, bool reduceMotion)
        {
            _nav = nav;
            Scroll = default;
            _lastOffset = _sheet.Stack.Scroll.scrollOffset.y;
            _sheet.Root.EnableInClassList(InstantClass, reduceMotion);
            Apply();
        }

        // The card closed: no settings, nothing remembered
        public void Unbind()
        {
            _nav = null;
            Scroll = default;
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
                HasBottom = stack.Footer.childCount > 0,
                ScrollHidesBottom = Scroll.BottomHidden,
                // - the REST height of the stop, never the animated one, so a rising sheet does not flip the chrome on its way
                SheetHeight = _sheet.Stops.HeightOf(_sheet.Stop),
                // - everything above the strip: the grab zone and the header (its pinned part sits inside it), in the sheet's own frame
                HeaderHeight = header.layout.yMax + header.resolvedStyle.marginBottom,
                TopHeight = _sheet.Slots.Top.layout.height,
                BottomHeight = stack.Footer.layout.height,
                // - the blocks' own height: the gap under a shown strip is the chrome's, so showing it cannot change this decision
                ContentHeight = stack.Scroll.contentContainer.layout.height - stack.Scroll.contentContainer.resolvedStyle.paddingTop,
            };
            var chrome = CardChromeRule.Decide(input);
            TopShown = chrome.Top;
            BottomShown = chrome.Bottom;
            _sheet.Slots.Top.EnableInClassList(TopHiddenClass, !chrome.Top);
            _sheet.Root.EnableInClassList(TopShownClass, chrome.Top);
            stack.Footer.EnableInClassList(BottomHiddenClass, !chrome.Bottom);
        }

        // The stack scrolled: one more move for the scroll's memory, then a new decision
        private void OnScrolled(float offset)
        {
            float delta = offset - _lastOffset;
            _lastOffset = offset;
            var scroll = _sheet.Stack.Scroll;
            Scroll = CardChromeRule.Next(Scroll, delta, CardChromeRule.AtTop(offset),
                CardChromeRule.AtEnd(offset, scroll.contentContainer.layout.height, scroll.contentViewport.layout.height), _nav?.hide_on_scroll);
            Apply();
        }
    }
}
