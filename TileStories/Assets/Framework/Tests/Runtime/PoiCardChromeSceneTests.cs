using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // PoiCardChromeSceneTests's shared ground: its set-up and helpers, so its own tests and its old-navigation ones stand on one fixture
    public abstract class PoiCardChromeSceneFixture : SearchSceneFixture
    {
        protected const string Lamp = "lamp";

        protected PoiCardSheetView Sheet => Card.Sheet;

        protected ScrollView Scroll => Sheet.Stack.Scroll;

        protected VisualElement Strip => Sheet.Slots.Top;

        protected VisualElement Footer => Sheet.Stack.Footer;

        // The part that slides and gives way: the bottom area the footer sits in (step 7a), with the bottom bar when there is one
        protected VisualElement Bottom => Sheet.BottomArea;

        protected CardChromeView Chrome => Sheet.Chrome;

        // One screen pixel in panel units: the run's pinned frame decides it, so it is measured, not assumed
        protected float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;

        // Everything above the strip, in the sheet's own frame (what CardChromeRule calls the header): the grab zone and the header
        protected float HeaderBottom => Sheet.Stack.HeaderSlot.layout.yMax + Sheet.Stack.HeaderSlot.resolvedStyle.marginBottom;

        // The room a stop leaves under the header: what the stack and the chrome share
        protected float Room => Sheet.Stops.HeightOf(Sheet.Stop) - HeaderBottom;

        protected float Viewport => Scroll.contentViewport.layout.height;

        // What the stack may take at this stop: the whole sheet under its own top -- the header and the strip FLOAT over it since _3.3 S4a,
        // so only the bottom area (in the flow) takes room from it
        protected float StackRoom => Sheet.Stops.HeightOf(Sheet.Stop) - Scroll.layout.yMin;

        // A part of the chrome takes its place in the card: in the flow, visible
        protected static bool InTheCard(VisualElement e) =>
            e.resolvedStyle.display == DisplayStyle.Flex && e.resolvedStyle.visibility == Visibility.Visible && e.resolvedStyle.position == Position.Relative;

        // The strip shows over the card: displayed, visible, not marked hidden -- it FLOATS over the stack since _3.3 S4a (absolute whether
        // shown or not), so CardChromeView's hidden class is what says it went
        protected static bool StripShown(VisualElement strip) =>
            strip.resolvedStyle.display == DisplayStyle.Flex && strip.resolvedStyle.visibility == Visibility.Visible
            && !strip.ClassListContains(CardChromeView.TopHiddenClass);

        // What the chrome decided from, for a precondition's message
        protected string ChromeState() =>
            Sheet.Stop + ": offset " + Scroll.scrollOffset.y.ToString("F0") + ", hero bottom " + Sheet.Stack.HeaderView.HeroBottom().ToString("F0")
            + ", top " + Chrome.TopShown + ", bottom " + Chrome.BottomShown + ", footer " + Chrome.FooterShown + ", travel "
            + Chrome.Scroll.Travel.ToString("F0") + ", hidden by the scroll " + Chrome.Scroll.BottomHidden + "; " + Parts();

        // The footer is where a thumb can reach it: inside the card's visible bottom edge
        protected bool FooterOnScreen => Footer.worldBound.yMax <= Sheet.Root.worldBound.yMax + OnePixel && Footer.worldBound.yMin < Sheet.Root.worldBound.yMax - OnePixel;

        // ... or slid below the card's clipped bottom edge, out of reach and out of sight
        protected bool FooterBelowTheCard => Footer.worldBound.yMin >= Sheet.Root.worldBound.yMax - OnePixel;

        protected IEnumerator Open(string poiId, SheetStopRule.Stop stop)
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
        protected IEnumerator Rested()
        {
            float rest = Sheet.Stops.HeightOf(Sheet.Stop);
            for (int frame = 0; frame < 600 && Mathf.Abs(Sheet.Root.layout.height - rest) > OnePixel; frame++) yield return null;
            Assert.AreEqual(rest, Sheet.Root.layout.height, OnePixel, "precondition: the sheet rests at its " + Sheet.Stop + " height");
            yield return null;
        }

        // Where every part of the sheet sits, for a failure message
        protected string Parts() =>
            "root " + Sheet.Root.layout + ", handle " + Sheet.Handle.layout + ", header " + Sheet.Stack.HeaderSlot.layout + ", strip "
            + Strip.layout + " (" + Strip.resolvedStyle.position + "), stack " + Scroll.layout + ", viewport " + Scroll.contentViewport.layout
            + ", footer " + Footer.layout + " (" + Footer.resolvedStyle.position + ")";

        // Push an edit the way the Editor's live applier does: a private copy of the running config, through the wall's own seam
        protected IEnumerator Push(System.Action<WallConfigData> edit)
        {
            var copy = ConfigCopy();
            edit(copy);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        // A real finger drags the stack by `dy` panel units (negative = up the screen = further down the card), then lets it settle
        protected IEnumerator DragStack(float dy)
        {
            // - the part a finger reaches: under the floating header and strip (_3.3 S4a), above the bottom area
            var viewport = CardTestInput.ReachableStack(Sheet);
            // - 20 px in from the edge the finger moves away from, or the middle of a viewport shorter than that (Half with its footer)
            float inset = Mathf.Min(20f, viewport.height * 0.5f);
            float startY = dy < 0f ? viewport.yMax - inset : viewport.yMin + inset;
            yield return CardTestInput.TouchDrag(Scroll.panel, new Vector2(viewport.center.x, startY), new Vector2(0f, dy));
            yield return CardTestInput.Settle(0.6f);
        }

        // The shared reading past the hero (CardTestInput.ReadPastTheHero), then the sheet at rest
        protected IEnumerator ReadPastTheHero()
        {
            yield return CardTestInput.ReadPastTheHero(Sheet, DragStack);
            yield return Rested();
        }

        // The wall's own navigation settings are the defaults: what every assertion here is about
        protected void AssertTheDefaults()
        {
            var nav = ConfigCopy().card_settings.navigation;
            Assert.AreEqual(CardOptions.NavNone, nav.top, "precondition: the default Top Bar (none: the Navigation Bar is the way round)");
            Assert.AreEqual(CardOptions.ShowNavHalfAndFull, nav.show_at, "precondition: the default Show Navigation At");
            Assert.AreEqual(CardOptions.HideOnScrollDown, nav.hide_on_scroll, "precondition: the default Hide While Scrolling");
        }
    }

    // The card's chrome (_3.2 step 5b, CardChromeRule + CardChromeView) on the REAL LivingRoomScene, The Lamp's real card, the wall's
    // DEFAULT navigation settings and real input: at Half the stack has its reading space back (the 0 px Half of step 5a is gone); a
    // real drag down the card slides the sticky footer away and a real drag up brings it back; nothing shows at peek; the share of the
    // card that shows content at Full rises while scrolling down, against the same card with Hide While Scrolling = Never; a header that
    // collapsed while reading gives Half its strip back; Show Navigation At = Full Only is live and only hides more.
    [Category("Core")]
    public class PoiCardChromeSceneTests : PoiCardChromeSceneFixture
    {
        // ---------------- peek ----------------

        // (the bottom area holds the footer and the floating Navigation Bar since _3.3 S4 third round; the old strip is the bar's now)
        [UnityTest]
        public IEnumerator NothingShowsAtPeek()
        {
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            yield return ReadPastTheHero();
            Assert.IsTrue(Chrome.BottomShown, "precondition: the bottom area shows at Full once the hero is read: " + ChromeState());
            Sheet.SetStop(SheetStopRule.Stop.Peek);
            yield return CardTestInput.Settle();
            yield return Rested();
            Assert.IsFalse(Chrome.BottomShown, "no footer and no bar at peek");
            Assert.IsFalse(InTheCard(Bottom));
            Assert.IsTrue(FooterBelowTheCard, "the footer is below the card's edge");
        }

        // Reduce motion: the footer jumps instead of sliding (no transition left on it)
        [UnityTest]
        public IEnumerator ReduceMotion_MakesTheFooterJump()
        {
            yield return Push(c => c.card_settings.container.reduce_motion = true);
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            Assert.IsTrue(Sheet.Root.ClassListContains(CardChromeView.InstantClass), "the card says instant");
            foreach (var duration in Bottom.resolvedStyle.transitionDuration)
                Assert.AreEqual(0f, duration.value, "no slide on the bottom area (the footer in it)");
        }
    }

    // PoiCardChromeSceneTests's tests whose subject is an old navigation element: they wait for its rebuild on the Navigation Bar's pattern
    [Ignore(TestSkips.OldNavigationElements)]
    public class PoiCardChromeSceneOldNavigationTests : PoiCardChromeSceneFixture
    {
        // ---------------- Half: the reading space is back ----------------

        // The Lamp at Half with the DEFAULT settings (_3.3 P4.4a): the hero fills what Half shows under the bar -- picture, title, line and
        // the card's action at its foot -- so neither the strip nor the sticky bar shows; once the visitor reads past the hero the sticky bar
        // comes (its "Show me where it is" is often why the card was opened) over at least one line, the strip only where the floor allows
        [UnityTest]
        public IEnumerator TheLampAtHalf_ShowsItsWholeHeroWithItsAction_AndTheStickyBarOnceTheHeroIsRead()
        {
            AssertTheDefaults();
            yield return Open(Lamp, SheetStopRule.Stop.Half);
            Assert.IsNotNull(Sheet.Slots.TopView, "precondition: the card holds its strip");
            Assert.Greater(Footer.childCount, 0, "precondition: the card has a sticky footer");
            var header = Sheet.Stack.HeaderView;
            Assert.IsFalse(Chrome.TopShown, "no strip over the hero at Half");
            Assert.IsFalse(Chrome.BottomShown, "no sticky bar while the hero, which shows the action, is in view");
            // - _3.3 S4a: the hero is the whole sheet at Full (HeroRule.Height), its words and the action at its foot; Half shows its top part
            Rect hero = header.HeroPart.worldBound, view = Scroll.contentViewport.worldBound;
            Assert.AreEqual(HeroRule.Height(Sheet.Stops, 0f), hero.height, OnePixel, "the hero is as tall as the sheet at Full: " + Parts());
            Assert.AreEqual(view.yMin, hero.yMin, OnePixel, "its top is in view at Half");
            Assert.Greater(hero.yMax, view.yMax + OnePixel, "...and it runs on under Half's foot");
            var heroAction = header.Actions.Q<Button>(className: "card-action");
            Assert.IsNotNull(heroAction, "the hero shows the card's action");
            Assert.LessOrEqual(heroAction.worldBound.yMax, hero.yMax + OnePixel, "the action sits in the hero's foot: " + heroAction.worldBound + " in " + hero);
            Assert.Greater(heroAction.worldBound.yMin, hero.center.y, "...in its lower half (HeroRule.WordsBottom)");

            yield return ReadPastTheHero();
            float footer = Footer.layout.height, room = Room;
            Assert.IsTrue(Chrome.BottomShown, "the hero read: the sticky bar comes");
            Assert.IsTrue(InTheCard(Bottom), "the footer takes its place");
            Assert.IsTrue(FooterOnScreen, "inside the card's visible bottom edge: " + Footer.worldBound + " in " + Sheet.Root.worldBound);
            Assert.GreaterOrEqual(room - footer, CardChromeRule.MinStackViewportUnderFooter, "over at least one line: " + Parts());
            var button = Footer.Q<Button>();
            Assert.IsNotNull(button, "the footer holds its button");
            Assert.IsTrue(button.worldBound.yMax <= Sheet.Root.worldBound.yMax + OnePixel, "the whole button is on screen: " + button.worldBound);
            Assert.AreEqual(StackRoom - footer, Viewport, OnePixel, "the stack keeps all but the footer (the strip floats over it): " + Parts());
            Assert.GreaterOrEqual(Viewport, CardChromeRule.MinStackViewportUnderFooter - OnePixel, "at least one line: " + Parts());
            // - and something to read there, really drawn: a block of the card is inside the viewport
            Assert.IsTrue(Scroll.contentContainer.Children().Any(c => c.worldBound.Overlaps(Scroll.contentViewport.worldBound) && c != header.HeroPart),
                "a block is in view at Half");
            yield return CardGalleryChecks.Render("Chrome_Lamp_half_default_390");
        }

        // While the hero fills Half there is no room for the strip; reading past it gives the strip back where the floor allows (the map
        // comes back), and Show Navigation At = Full Only -- live -- hides it there anyway: the wall's option hides more, never less
        [UnityTest]
        public IEnumerator TheHeroReadAway_GivesHalfItsStripBack_AndFullOnlyHidesItThere_Live()
        {
            AssertTheDefaults();
            yield return Open(Lamp, SheetStopRule.Stop.Half);
            Assert.IsFalse(Chrome.TopShown, "precondition: no strip at Half over the hero");
            yield return ReadPastTheHero();
            Assert.GreaterOrEqual(Room - Strip.layout.height - (Chrome.BottomShown ? Bottom.layout.height : 0f), CardChromeRule.MinStackViewport,
                "precondition: the hero read leaves room for strip + floor: " + Parts());
            Assert.IsTrue(Chrome.TopShown, "the strip is back at Half");
            Assert.IsTrue(StripShown(Strip), "and shows over the card");
            Assert.GreaterOrEqual(Viewport, CardChromeRule.MinStackViewport - OnePixel, "never under the floor");

            yield return Push(c => c.card_settings.navigation.show_at = CardOptions.ShowNavFullOnly);
            Assert.IsFalse(HeroRule.InView(Scroll.scrollOffset.y, Sheet.Stack.HeaderView.HeroBottom()), "precondition: the live edit kept the scroll past the hero");
            Assert.IsFalse(Chrome.TopShown, "Full Only: no strip at Half, room or not");
            Assert.IsFalse(StripShown(Strip));
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            yield return Rested();
            Assert.IsTrue(Chrome.TopShown, "Full Only: the strip at Full");
            Assert.IsTrue(StripShown(Strip));
        }

        // ---------------- Hide While Scrolling ----------------

        // A real drag down the card slides the footer away and gives its height to the stack; a real drag back up brings it back
        [UnityTest]
        public IEnumerator ARealDragDown_HidesTheFooter_AndADragUpBringsItBack()
        {
            AssertTheDefaults();
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            // - _3.3 P4.4a: at the top the hero shows the action, the footer waits: read past the hero first
            yield return ReadPastTheHero();
            Assert.IsTrue(Chrome.BottomShown, "precondition: the footer shows at Full once the hero is read: " + ChromeState());
            Assert.IsTrue(FooterOnScreen, "precondition: and is on screen " + Footer.worldBound + " in " + Sheet.Root.worldBound);
            Assert.Greater(NavScrollRule.Range(Scroll.contentContainer.layout.height, Viewport), 1000f, "precondition: the stack really scrolls");
            float footer = Footer.layout.height, strip = Strip.layout.height;
            Assert.AreEqual(StackRoom - footer, Viewport, OnePixel, "precondition: the stack has all but the footer (the strip floats over it)");

            // - the room is re-read after the drag: the viewport is measured against it, not against the viewport before the drag
            yield return DragStack(-300f);
            Assert.Greater(Scroll.scrollOffset.y, CardChromeRule.HideAfterTravel, "precondition: the drag moved down the card past the threshold");
            Assert.IsFalse(Chrome.BottomShown, "moving down hid the footer");
            Assert.IsTrue(Chrome.TopShown, "the strip stays: it is the map");
            Assert.IsTrue(FooterBelowTheCard, "the footer slid below the card's edge");
            Assert.AreEqual(StackRoom, Viewport, OnePixel, "and its height went to the stack: the whole sheet under the floating header and strip");

            float offset = Scroll.scrollOffset.y;
            yield return DragStack(60f);
            Assert.Less(Scroll.scrollOffset.y, offset - CardChromeRule.ShowAfterTravel, "precondition: the drag moved back up past the threshold");
            Assert.IsTrue(Chrome.BottomShown, "moving up brought it back");
            Assert.IsTrue(FooterOnScreen, "on screen again " + Footer.worldBound);
            Assert.AreEqual(StackRoom - footer, Viewport, OnePixel, "and the stack gave its height back");
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
            // - _3.3 S4a: the hero is the screen and holds the action, so the footer waits for it; both readings start past it
            yield return ReadPastTheHero();
            yield return DragStack(-300f);
            Assert.Greater(Scroll.scrollOffset.y, 0f, "precondition: scrolled down");
            float down = Viewport / Sheet.Root.layout.height;
            yield return CardGalleryChecks.Render("Chrome_Lamp_full_scrolled_390");

            yield return Push(c => c.card_settings.navigation.hide_on_scroll = CardOptions.HideOnScrollNever);
            Sheet.Stack.Scroll.scrollOffset = Vector2.zero;
            yield return CardTestInput.Settle();
            yield return ReadPastTheHero();
            yield return DragStack(-300f);
            Assert.Greater(Scroll.scrollOffset.y, 0f, "precondition: scrolled down with Never too");
            Assert.IsTrue(Chrome.BottomShown, "Never: the footer stays: " + ChromeState());
            float never = Viewport / Sheet.Root.layout.height;
            Debug.Log("[Card] Lamp content share at Full: at rest " + atRest.ToString("P1") + ", scrolled down " + down.ToString("P1")
                + " (Hide While Scrolling = down) vs " + never.ToString("P1") + " (never)");
            Assert.Greater(down, never, "the default shows more content while reading down");
            Assert.AreEqual(Footer.layout.height / Sheet.Root.layout.height, down - never, OnePixel / Sheet.Root.layout.height,
                "by exactly the footer's share (both headers collapsed the same way)");
        }
    }
}
