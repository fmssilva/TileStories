using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // The card's chrome (_3.2 step 5b, CardChromeRule + CardChromeView) on the REAL LivingRoomScene, The Lamp's real card, the wall's
    // DEFAULT navigation settings and real input: at Half the stack has its reading space back (the 0 px Half of step 5a is gone); a
    // real drag down the card slides the sticky footer away and a real drag up brings it back; nothing shows at peek; the share of the
    // card that shows content at Full rises while scrolling down, against the same card with Hide While Scrolling = Never; a header that
    // collapsed while reading gives Half its strip back; Show Navigation At = Full Only is live and only hides more.
    public class PoiCardChromeSceneTests : SearchSceneFixture
    {
        private const string Lamp = "lamp";

        private PoiCardSheetView Sheet => Card.Sheet;
        private ScrollView Scroll => Sheet.Stack.Scroll;
        private VisualElement Strip => Sheet.Slots.Top;
        private VisualElement Footer => Sheet.Stack.Footer;
        private CardChromeView Chrome => Sheet.Chrome;

        // One screen pixel in panel units: the run's pinned frame decides it, so it is measured, not assumed
        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;

        // Everything above the strip, in the sheet's own frame (what CardChromeRule calls the header): the grab zone and the header
        private float HeaderBottom => Sheet.Stack.HeaderSlot.layout.yMax + Sheet.Stack.HeaderSlot.resolvedStyle.marginBottom;

        // The room a stop leaves under the header: what the stack and the chrome share
        private float Room => Sheet.Stops.HeightOf(Sheet.Stop) - HeaderBottom;

        private float Viewport => Scroll.contentViewport.layout.height;

        // A part of the chrome takes its place in the card: in the flow, visible
        private static bool InTheCard(VisualElement e) =>
            e.resolvedStyle.display == DisplayStyle.Flex && e.resolvedStyle.visibility == Visibility.Visible && e.resolvedStyle.position == Position.Relative;

        // The footer is where a thumb can reach it: inside the card's visible bottom edge
        private bool FooterOnScreen => Footer.worldBound.yMax <= Sheet.Root.worldBound.yMax + OnePixel && Footer.worldBound.yMin < Sheet.Root.worldBound.yMax - OnePixel;

        // ... or slid below the card's clipped bottom edge, out of reach and out of sight
        private bool FooterBelowTheCard => Footer.worldBound.yMin >= Sheet.Root.worldBound.yMax - OnePixel;

        private IEnumerator Open(string poiId, SheetStopRule.Stop stop)
        {
            SelectionEventBus.Select(poiId);
            yield return CardTestInput.Settle();
            Sheet.SetStop(stop);
            yield return CardTestInput.Settle();
            Assert.AreEqual(stop, Sheet.Stop, "precondition: the card rests at " + stop);
            yield return Rested();
        }

        // The sheet's height animates over --ts-duration (real time): a slow frame rate (Unity in the background) can end a frame-count
        // settle mid-rise. Every measure waits for the sheet to rest at its stop's height, and says so with numbers when it does not
        private IEnumerator Rested()
        {
            float rest = Sheet.Stops.HeightOf(Sheet.Stop);
            for (int frame = 0; frame < 600 && Mathf.Abs(Sheet.Root.layout.height - rest) > OnePixel; frame++) yield return null;
            Assert.AreEqual(rest, Sheet.Root.layout.height, OnePixel, "precondition: the sheet rests at its " + Sheet.Stop + " height");
            yield return null;
        }

        // Where every part of the sheet sits, for a failure message
        private string Parts() =>
            "root " + Sheet.Root.layout + ", handle " + Sheet.Handle.layout + ", header " + Sheet.Stack.HeaderSlot.layout + ", strip "
            + Strip.layout + " (" + Strip.resolvedStyle.position + "), stack " + Scroll.layout + ", viewport " + Scroll.contentViewport.layout
            + ", footer " + Footer.layout + " (" + Footer.resolvedStyle.position + ")";

        // Push an edit the way the Editor's live applier does: a private copy of the running config, through the wall's own seam
        private IEnumerator Push(System.Action<WallConfigData> edit)
        {
            var copy = ConfigCopy();
            edit(copy);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        // A real finger drags the stack by `dy` panel units (negative = up the screen = further down the card), then lets it settle
        private IEnumerator DragStack(float dy)
        {
            var viewport = Scroll.contentViewport.worldBound;
            float startY = dy < 0f ? viewport.yMax - 20f : viewport.yMin + 20f;
            yield return CardTestInput.TouchDrag(Scroll.panel, new Vector2(viewport.center.x, startY), new Vector2(0f, dy));
            yield return CardTestInput.Settle(0.6f);
        }

        // The wall's own navigation settings are the defaults: what every assertion here is about
        private void AssertTheDefaults()
        {
            var nav = ConfigCopy().card_settings.navigation;
            Assert.AreEqual(BuiltInNavElements.TextTabs, nav.top, "precondition: the default Top Bar");
            Assert.AreEqual(CardOptions.ShowNavHalfAndFull, nav.show_at, "precondition: the default Show Navigation At");
            Assert.AreEqual(CardOptions.HideOnScrollDown, nav.hide_on_scroll, "precondition: the default Hide While Scrolling");
        }

        // ---------------- Half: the reading space is back ----------------

        // The Lamp at Half with the DEFAULT settings: header + strip + sticky footer do not fit with any content (step 5a: 0 px), so the
        // floor hides both -- the stack's viewport is the whole room under the header, more than a heading and two lines
        [UnityTest]
        public IEnumerator TheLampAtHalf_WithTheDefaults_GivesTheStackItsReadingSpaceBack()
        {
            AssertTheDefaults();
            yield return Open(Lamp, SheetStopRule.Stop.Half);
            Assert.IsNotNull(Sheet.Slots.TopView, "precondition: the card holds its strip (56 targets, 8 sections)");
            Assert.Greater(Footer.childCount, 0, "precondition: the card has a sticky footer");
            float strip = Strip.layout.height, footer = Footer.layout.height, room = Room;
            Debug.Log("[Card] Lamp chrome at Half: room " + room.ToString("F1") + " (sheet " + Sheet.Stops.Half.ToString("F1") + " - header "
                + HeaderBottom.ToString("F1") + "), strip " + strip.ToString("F1") + ", footer " + footer.ToString("F1") + ", viewport " + Viewport.ToString("F1"));
            Assert.Less(room - strip, CardChromeRule.MinStackViewport, "precondition: the strip alone would leave less than the floor");

            Assert.IsFalse(Chrome.TopShown, "the floor hides the strip at Half");
            Assert.IsFalse(Chrome.BottomShown, "and the footer first");
            Assert.IsFalse(InTheCard(Strip), "the strip takes no place in the card");
            Assert.IsFalse(InTheCard(Footer), "nor does the footer");
            Assert.IsTrue(FooterBelowTheCard, "the footer slid below the card's edge: " + Footer.worldBound + " under " + Sheet.Root.worldBound);
            Assert.GreaterOrEqual(Viewport, CardChromeRule.MinStackViewport, "the stack has at least a heading and two lines: " + Parts());
            Assert.AreEqual(room, Viewport, OnePixel, "in fact the whole room under the header: " + Parts());
            // - and something to read there, really drawn: the first block (or the header's picture) is inside the viewport
            var first = Scroll.contentContainer[0];
            Assert.IsTrue(first.worldBound.Overlaps(Scroll.contentViewport.worldBound), "the stack's first element is in view at Half");
            yield return CardGalleryChecks.Render("Chrome_Lamp_half_default_390");
        }

        // While reading at Half the header collapses to its one line: that room is enough for the strip again (the map comes back), and
        // Show Navigation At = Full Only -- live -- hides it there anyway: the wall's option hides more, never less
        [UnityTest]
        public IEnumerator ACollapsedHeader_GivesHalfItsStripBack_AndFullOnlyHidesItThere_Live()
        {
            AssertTheDefaults();
            yield return Open(Lamp, SheetStopRule.Stop.Half);
            Assert.IsFalse(Chrome.TopShown, "precondition: no strip at Half with the header open");
            yield return DragStack(-60f);
            Assert.IsTrue(Sheet.Stack.HeaderCollapsed, "precondition: reading collapsed the header");
            Assert.GreaterOrEqual(Room - Strip.layout.height, CardChromeRule.MinStackViewport, "precondition: the collapsed header leaves room for strip + floor");
            Assert.IsTrue(Chrome.TopShown, "the strip is back at Half");
            Assert.IsTrue(InTheCard(Strip), "and takes its place in the card");
            Assert.GreaterOrEqual(Viewport, CardChromeRule.MinStackViewport - OnePixel, "never under the floor");

            yield return Push(c => c.card_settings.navigation.show_at = CardOptions.ShowNavFullOnly);
            Assert.IsTrue(Sheet.Stack.HeaderCollapsed, "precondition: the live edit kept the scroll, the header is still collapsed");
            Assert.IsFalse(Chrome.TopShown, "Full Only: no strip at Half, room or not");
            Assert.IsFalse(InTheCard(Strip));
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            yield return Rested();
            Assert.IsTrue(Chrome.TopShown, "Full Only: the strip at Full");
            Assert.IsTrue(InTheCard(Strip));
        }

        // ---------------- peek ----------------

        [UnityTest]
        public IEnumerator NothingShowsAtPeek()
        {
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            Assert.IsTrue(Chrome.TopShown && Chrome.BottomShown, "precondition: both show at Full");
            Sheet.SetStop(SheetStopRule.Stop.Peek);
            yield return CardTestInput.Settle();
            yield return Rested();
            Assert.IsFalse(Chrome.TopShown, "no strip at peek");
            Assert.IsFalse(Chrome.BottomShown, "no footer at peek");
            Assert.IsFalse(InTheCard(Strip));
            Assert.IsFalse(InTheCard(Footer));
            Assert.IsTrue(FooterBelowTheCard, "the footer is below the card's edge");
        }

        // ---------------- Hide While Scrolling ----------------

        // A real drag down the card slides the footer away and gives its height to the stack; a real drag back up brings it back
        [UnityTest]
        public IEnumerator ARealDragDown_HidesTheFooter_AndADragUpBringsItBack()
        {
            AssertTheDefaults();
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            Assert.IsTrue(Chrome.BottomShown, "precondition: the footer shows at Full");
            Assert.IsTrue(FooterOnScreen, "precondition: and is on screen " + Footer.worldBound + " in " + Sheet.Root.worldBound);
            Assert.Greater(NavScrollRule.Range(Scroll.contentContainer.layout.height, Viewport), 1000f, "precondition: the stack really scrolls");
            float footer = Footer.layout.height, strip = Strip.layout.height;
            Assert.AreEqual(Room - strip - footer, Viewport, OnePixel, "precondition: the stack has the room the strip and the footer leave");

            // - the drag also collapses the header (its own rule), so the room is re-read: the viewport is measured against it, not
            //   against the viewport before the drag
            yield return DragStack(-300f);
            Assert.Greater(Scroll.scrollOffset.y, CardChromeRule.HideAfterTravel, "precondition: the drag moved down the card past the threshold");
            Assert.IsFalse(Chrome.BottomShown, "moving down hid the footer");
            Assert.IsTrue(Chrome.TopShown, "the strip stays: it is the map");
            Assert.IsTrue(FooterBelowTheCard, "the footer slid below the card's edge");
            Assert.AreEqual(Room - strip, Viewport, OnePixel, "and its height went to the stack: all the room under the header but the strip");

            float offset = Scroll.scrollOffset.y;
            yield return DragStack(60f);
            Assert.Less(Scroll.scrollOffset.y, offset - CardChromeRule.ShowAfterTravel, "precondition: the drag moved back up past the threshold");
            Assert.IsTrue(Chrome.BottomShown, "moving up brought it back");
            Assert.IsTrue(FooterOnScreen, "on screen again " + Footer.worldBound);
            Assert.AreEqual(Room - strip - footer, Viewport, OnePixel, "and the stack gave its height back");
        }

        // The share of the card that shows content at Full, once the reader has moved down: higher with the default (the footer gave
        // its room) than with Hide While Scrolling = Never on the same card after the same drag -- relative, never a pixel constant.
        // Also renders the default's picture (Full, scrolled down, footer gone)
        [UnityTest]
        public IEnumerator TheContentShareAtFull_RisesWhileScrollingDown_AgainstHideWhileScrollingNever()
        {
            AssertTheDefaults();
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            float atRest = Viewport / Sheet.Root.layout.height;
            yield return DragStack(-300f);
            Assert.Greater(Scroll.scrollOffset.y, 0f, "precondition: scrolled down");
            float down = Viewport / Sheet.Root.layout.height;
            yield return CardGalleryChecks.Render("Chrome_Lamp_full_scrolled_390");

            yield return Push(c => c.card_settings.navigation.hide_on_scroll = CardOptions.HideOnScrollNever);
            Sheet.Stack.Scroll.scrollOffset = Vector2.zero;
            yield return CardTestInput.Settle();
            yield return DragStack(-300f);
            Assert.Greater(Scroll.scrollOffset.y, 0f, "precondition: scrolled down with Never too");
            Assert.IsTrue(Chrome.BottomShown, "Never: the footer stays");
            float never = Viewport / Sheet.Root.layout.height;
            Debug.Log("[Card] Lamp content share at Full: at rest " + atRest.ToString("P1") + ", scrolled down " + down.ToString("P1")
                + " (Hide While Scrolling = down) vs " + never.ToString("P1") + " (never)");
            Assert.Greater(down, never, "the default shows more content while reading down");
            Assert.AreEqual(Footer.layout.height / Sheet.Root.layout.height, down - never, OnePixel / Sheet.Root.layout.height,
                "by exactly the footer's share (both headers collapsed the same way)");
        }

        // Reduce motion: the footer jumps instead of sliding (no transition left on it)
        [UnityTest]
        public IEnumerator ReduceMotion_MakesTheFooterJump()
        {
            yield return Push(c => c.card_settings.container.reduce_motion = true);
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            Assert.IsTrue(Sheet.Root.ClassListContains(CardChromeView.InstantClass), "the card says instant");
            foreach (var duration in Footer.resolvedStyle.transitionDuration)
                Assert.AreEqual(0f, duration.value, "no slide on the footer");
        }
    }
}
