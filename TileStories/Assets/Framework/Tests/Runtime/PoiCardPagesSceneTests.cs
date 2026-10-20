using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of step 14 (_3.2 section 6.1, the pages layout and its Family Pager) on the REAL LivingRoomScene with real touches and the
    // card's REAL owners (the preview stage, the video owner over a stand-in picture device). The layout is pushed through the live seam, as
    // the POI Editor's Layout row does. Asserted on the stack, the controller, the pager and the OWNERS, never on pixels:
    //   - THE RULE: a vertical drag inside a page scrolls it and never turns it; a sideways drag turns it and never scrolls it; a diagonal is
    //     judged once, as it leaves the slop, and stays what it was judged (a mostly-sideways one never scrolls however far it then goes up, a
    //     mostly-vertical one never turns however far it then goes sideways)
    //   - SwipePageRule on the real card: a short swipe slides back, the first and last pages go no further, one page per swipe
    //   - the Family Pager: names the page shown (words, icon, place), real taps on its chevrons turn one page, none at the ends
    //   - a jump from a Bottom Bar slot lands its page; every turn reports one nav_jump (pages / family_pager)
    //   - a page left lets its media go (the preview slot, the playing video), exactly as a tab put away
    //   - a part that drags by itself keeps its drags: the gallery's strip scrolls, the 3D stage turns, the page stays
    //   - a card with nothing to switch between scrolls; renders at 390 px in English and Portuguese
    public class PoiCardPagesSceneTests : SearchSceneFixture
    {
        private const string Lamp = "lamp";
        private const string LampMilitary = "lamp_military";

        private PoiCardSheetView Sheet => Card.Sheet;
        private BlockStackView Stack => Sheet.Stack;
        private NavController Nav => Sheet.Nav;
        private CardPagerView Pager => Sheet.Pager;
        private ScrollView Scroll => Stack.Scroll;
        private FamilyPagerNavView FamilyPager => Sheet.Slots.TopView as FamilyPagerNavView;
        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;
        private float Range => NavScrollRule.Range(Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height);
        private int Page => Sheet.Layout.ShownSection;

        private sealed class RecordingCardEvents : ICardEvents
        {
            public readonly List<CardEvent> Raised = new();
            public void Raise(CardEvent cardEvent) => Raised.Add(cardEvent);
        }

        private RecordingCardEvents _events;
        private ManualVideoOutput _videoOut;
        private CardVideoService _video;

        private List<CardEvent> NavJumps => _events.Raised.Where(e => e.Kind == CardEventKinds.NavJump).ToList();

        [UnitySetUp]
        public IEnumerator UseAStandInPictureDeviceAndRecordTheEvents()
        {
            _videoOut = new ManualVideoOutput();
            _video = new CardVideoService(_videoOut, () => Card.Media);
            Card.UseVideoService(_video);
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            _events = new RecordingCardEvents();
            Card.Events = _events;
            yield break;
        }

        [UnityTearDown]
        public IEnumerator LetTheStandInsGo()
        {
            _video.CardClosed();
            _videoOut.Release();
            yield break;
        }

        // ---------------- helpers ----------------

        // The wall's navigation edited and pushed through the live seam, as a POI Editor row does
        private IEnumerator UseNavigation(Action<CardNavigationSettings> edit)
        {
            var copy = ConfigCopy();
            edit(copy.card_settings.navigation);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        private IEnumerator UsePages(Action<CardNavigationSettings> more = null) =>
            UseNavigation(nav => { nav.layout = CardOptions.LayoutPages; more?.Invoke(nav); });

        private IEnumerator OpenFull(string poi)
        {
            if (SelectionEventBus.CurrentPoiId != poi)
            {
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
                SelectionEventBus.Select(poi);
                yield return CardTestInput.Settle();
            }
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "precondition: the card rests at full");
            float rest = Sheet.Stops.HeightOf(Sheet.Stop);
            for (int frame = 0; frame < 600 && Mathf.Abs(Sheet.Root.layout.height - rest) > OnePixel; frame++) yield return null;
            Assert.AreEqual(rest, Sheet.Root.layout.height, OnePixel, "precondition: the sheet rests at its full height");
        }

        // The pages are on and rest: no slide going, no jump gliding
        private IEnumerator Rested()
        {
            for (int frame = 0; frame < 600 && (Nav.IsJumping || Sheet.Root.ClassListContains(CardPagerView.SettlingClass)); frame++) yield return null;
            Assert.IsFalse(Nav.IsJumping, "the jump came to rest");
            yield return CardTestInput.Settle(0.2f);
            Assert.AreEqual(0f, Pager.Offset, "the page rests in its place");
        }

        // A point at the page's right edge, inside the peek: there the press lands on the stack's content itself (its inset), never on a
        // part of a block -- the plain place a thumb starts a page swipe from
        private Vector2 EdgePoint(float atY)
        {
            var viewport = Scroll.contentViewport.worldBound;
            var p = new Vector2(viewport.xMax - 8f, atY);
            var picked = Sheet.Root.panel.Pick(p);
            Assert.AreSame(Scroll.contentContainer, picked, "precondition: the press at the page's edge lands on the content's inset, not a block: " + picked?.name);
            return p;
        }

        private Vector2 EdgePoint() => EdgePoint(Scroll.contentViewport.worldBound.center.y);

        // A point at the page's LEFT side, for a swipe to the right, on plain content (no part that drags by itself under it)
        private Vector2 LeftPoint()
        {
            var viewport = Scroll.contentViewport.worldBound;
            var p = new Vector2(viewport.xMin + 8f, viewport.center.y);
            for (var e = Sheet.Root.panel.Pick(p); e != null && e != Scroll.contentViewport; e = e.parent)
                Assert.IsFalse(e is ScrollView s && s.mode != ScrollViewMode.Vertical || e.ClassListContains(SwipeGrab.AreaClass),
                    "precondition: the press at the page's left side lands on plain content, not on " + e.name);
            return p;
        }

        // The page has stopped coasting after a fling: its scroll offset the same for a few frames
        private IEnumerator Still()
        {
            float last = float.NaN;
            int still = 0;
            for (int frame = 0; frame < 600 && still < 5; frame++)
            {
                yield return null;
                still = Scroll.scrollOffset.y.Equals(last) ? still + 1 : 0;
                last = Scroll.scrollOffset.y;
            }
            Assert.GreaterOrEqual(still, 5, "precondition: the page came to rest");
        }

        private int SectionIndex(string family) => Nav.Model.Sections.ToList().FindIndex(s => s.Key == family);

        // A block's top lands at the viewport's top (relative, one pixel), or the page's end clamps it
        private void AssertLanded(string block, string name)
        {
            var slot = Stack.SlotOfBlock(block);
            if (slot.layout.y <= Range - OnePixel)
                Assert.AreEqual(0f, slot.worldBound.yMin - Scroll.contentViewport.worldBound.yMin, OnePixel, name + ": its top meets the viewport's top");
            else
                Assert.AreEqual(Range, Scroll.scrollOffset.y, OnePixel, name + ": a late block clamps at the end of the range");
        }

        // Every target of section `shown` drawn, every other one set aside (no view) -- what the page shows
        private void AssertShowsOnly(int shown, string when)
        {
            var sections = Nav.Model.Sections;
            for (int i = 0; i < sections.Count; i++)
                foreach (string key in sections[i].Targets)
                {
                    Assert.AreEqual(i != shown, Stack.IsParked(key), when + ": " + key + " (section " + sections[i].Key + ") set aside or not");
                    Assert.AreEqual(i == shown, Stack.ViewOfBlock(key) != null, when + ": " + key + " has a view only on its own page");
                }
        }

        // A finger put down, moved through `path` (offsets from where it went down, one per frame) and lifted; `everyFrame` looks at the card
        // while the finger is still down
        private IEnumerator HandDrag(Vector2 start, IEnumerable<Vector2> path, Action<Vector2> everyFrame = null)
        {
            var panel = Sheet.Root.panel;
            CardTestInput.TouchDown(panel, start);
            yield return null;
            Vector2 last = Vector2.zero;
            foreach (var step in path)
            {
                CardTestInput.TouchMove(panel, start + step);
                last = step;
                yield return null;
                everyFrame?.Invoke(step);
            }
            CardTestInput.TouchUp(panel, start + last);
            yield return null;
        }

        // `to` reached in `frames` equal steps
        private static IEnumerable<Vector2> Line(Vector2 from, Vector2 to, int frames)
        {
            for (int i = 1; i <= frames; i++) yield return Vector2.Lerp(from, to, (float)i / frames);
        }

        // ---------------- the rule that must not break ----------------

        [UnityTest]
        public IEnumerator AVerticalDragScrollsThePage_AndNeverTurnsIt_ASidewaysDragTurnsIt_AndNeverScrollsIt()
        {
            yield return UsePages();
            yield return OpenFull(Lamp);
            Assert.AreEqual(CardOptions.LayoutPages, Sheet.Layout.Effective, "precondition: The Lamp is drawn in pages");
            Assert.IsTrue(Pager.IsOn && Sheet.Root.ClassListContains(CardPagerView.PagedClass), "the pager is on");
            Assert.AreEqual(BuiltInNavElements.FamilyPager, Sheet.Slots.TopKey, "the top slot holds the Family Pager (the Top Bar is Text Tabs)");
            Assert.AreEqual(0, Page, "the first page shows first");
            AssertShowsOnly(0, "opened");
            Assert.Greater(Range, 100f, "precondition: the first page is taller than the viewport, so it really scrolls (range " + Range + ")");

            // - CLEARLY VERTICAL: the page scrolls; it never moves sideways and never turns
            float y0 = Scroll.scrollOffset.y;
            var viewport = Scroll.contentViewport.worldBound;
            var down = new Vector2(viewport.center.x, viewport.yMin + viewport.height * 0.8f);
            bool movedSideways = false;
            yield return HandDrag(down, Line(Vector2.zero, new Vector2(0f, -viewport.height * 0.5f), 16), _ => movedSideways |= Pager.Offset != 0f);
            Assert.Greater(Scroll.scrollOffset.y, y0 + 10f, "a vertical drag scrolled the page (" + y0 + " -> " + Scroll.scrollOffset.y + ")");
            Assert.IsFalse(movedSideways, "and the page never followed it sideways");
            Assert.AreEqual(0, Page, "nor turned");
            Assert.IsEmpty(NavJumps, "no jump");

            // - CLEARLY SIDEWAYS on a page at rest (a page still coasting is the next test's): the page follows the finger and turns; the scroll never moves while the finger is down
            yield return Still();
            float y1 = Scroll.scrollOffset.y;
            float scrolled = 0f, followed = 0f;
            // - what is DRAWN, not only what the pager means: a block of the page and the next page's cue, from their own rest positions
            var block = Stack.SlotOfBlock(Nav.Model.Sections[0].Targets[0]);
            float blockRest = block.worldBound.xMin, viewportRest = Scroll.contentViewport.worldBound.xMin;
            yield return HandDrag(EdgePoint(), Line(Vector2.zero, new Vector2(-Pager.PageWidth * 0.5f, 0f), 12), _ =>
            {
                scrolled = Mathf.Max(scrolled, Mathf.Abs(Scroll.scrollOffset.y - y1));
                followed = Mathf.Min(followed, Pager.Offset);
                Assert.AreEqual(blockRest + Pager.Offset, block.worldBound.xMin, OnePixel, "the page's blocks are drawn where the finger took them");
                Assert.AreEqual(viewportRest + Pager.PageWidth + Pager.Offset, Pager.Peek.worldBound.xMin, OnePixel, "and the next page's cue beside them");
            });
            Assert.AreEqual(0f, scrolled, OnePixel, "the page never scrolled while the finger went sideways");
            Assert.Less(followed, -Pager.PageWidth * 0.25f, "the page followed the finger sideways (" + followed + ")");
            yield return Rested();
            Assert.AreEqual(1, Page, "and turned to the next page");
            AssertShowsOnly(1, "after the swipe");
            AssertLanded(Nav.Model.Sections[1].Targets[0], "the new page's first block");
            var jump = NavJumps.Single();
            Assert.AreEqual(CardPagerView.SwipeKey, jump.Variant, "the turn reports its element: the swipe");
            Assert.AreEqual(Nav.Model.Sections[1].Key, jump.Value, "and the section it landed on");
        }

        // A finger landing on a page that still COASTS from a flick stops it, and that touch is the scroll's: UI Toolkit's ScrollView captures its
        // content at the touch that stops a fling and then hands the finger's moves to it alone (a phone's list does the same). A sideways move
        // then neither scrolls nor turns the page; the next swipe, on the still page, turns it. Whether a flick coasts depends on the frame time
        // of its last moves, so the test flicks until one does (the precondition), then judges
        [UnityTest]
        public IEnumerator AFingerOnACoastingPage_OnlyStopsIt_AndTheNextSwipeTurnsIt()
        {
            yield return UsePages();
            yield return OpenFull(Lamp);
            var viewport = Scroll.contentViewport.worldBound;
            var panel = Sheet.Root.panel;
            bool coasting = false;
            for (int attempt = 0; attempt < 5 && !coasting; attempt++)
            {
                Scroll.scrollOffset = Vector2.zero;
                yield return Still();
                // - a fast flick: big steps over few frames, released at speed
                var flickStart = new Vector2(viewport.center.x, viewport.yMin + viewport.height * 0.85f);
                CardTestInput.TouchDown(panel, flickStart);
                yield return null;
                foreach (var step in Line(Vector2.zero, new Vector2(0f, -viewport.height * 0.6f), 5))
                {
                    CardTestInput.TouchMove(panel, flickStart + step);
                    yield return null;
                }
                CardTestInput.TouchUp(panel, flickStart + new Vector2(0f, -viewport.height * 0.6f));
                float released = Scroll.scrollOffset.y;
                // - the fling's animation runs on the panel scheduler: watch a few frames for it to move the page
                for (int frame = 0; frame < 5 && !coasting; frame++)
                {
                    yield return null;
                    coasting = !Scroll.scrollOffset.y.Equals(released);
                }
            }
            Assert.IsTrue(coasting, "precondition: one of five flicks left the page coasting");
            Assert.AreEqual(0, Page);

            float stopped = float.NaN;
            bool movedSideways = false;
            yield return HandDrag(EdgePoint(), Line(Vector2.zero, new Vector2(-Pager.PageWidth * 0.5f, 0f), 12), _ =>
            {
                if (float.IsNaN(stopped)) stopped = Scroll.scrollOffset.y;
                movedSideways |= Pager.Offset != 0f;
                Assert.AreEqual(stopped, Scroll.scrollOffset.y, OnePixel, "the touch stopped the coasting page and its sideways move never scrolls it");
            });
            Assert.IsFalse(movedSideways, "the touch that stopped the page never moved it sideways");
            yield return Rested();
            Assert.AreEqual(0, Page, "nor turned it");
            Assert.IsEmpty(NavJumps, "no jump");

            yield return Still();
            yield return HandDrag(EdgePoint(), Line(Vector2.zero, new Vector2(-Pager.PageWidth * 0.5f, 0f), 12));
            yield return Rested();
            Assert.AreEqual(1, Page, "on the still page the next swipe turns it");
        }

        [UnityTest]
        public IEnumerator ADiagonalIsJudgedOnce_AndStaysWhatItWasJudged()
        {
            yield return UsePages();
            yield return OpenFull(Lamp);
            Assert.AreEqual(0, Page);
            var start = EdgePoint(Scroll.contentViewport.worldBound.yMin + Scroll.contentViewport.worldBound.height * 0.7f);

            // - MOSTLY SIDEWAYS at the slop (30 degrees), then it turns up steeply: the pager took it, the page never scrolls, and on release
            //   the whole travel is not clearly sideways, so nothing turns either
            float y0 = Scroll.scrollOffset.y;
            float scrolled = 0f;
            var path = Line(Vector2.zero, new Vector2(-16f, -9f), 2).Concat(Line(new Vector2(-16f, -9f), new Vector2(-40f, -240f), 14));
            yield return HandDrag(start, path, _ => scrolled = Mathf.Max(scrolled, Mathf.Abs(Scroll.scrollOffset.y - y0)));
            Assert.AreEqual(0f, scrolled, OnePixel, "judged sideways: the page never scrolled, though the finger then went 240 up");
            yield return Rested();
            Assert.AreEqual(0, Page, "not clearly sideways over its whole travel: no turn, the page slid back");

            // - a 30-degree diagonal all the way: sideways at the slop, and sideways enough to turn
            int jumps = NavJumps.Count;
            float y1 = Scroll.scrollOffset.y;
            scrolled = 0f;
            yield return HandDrag(start, Line(Vector2.zero, new Vector2(-Pager.PageWidth * 0.5f, -Pager.PageWidth * 0.5f * 0.4f), 12),
                _ => scrolled = Mathf.Max(scrolled, Mathf.Abs(Scroll.scrollOffset.y - y1)));
            Assert.AreEqual(0f, scrolled, OnePixel, "a shallow diagonal never scrolls the page");
            yield return Rested();
            Assert.AreEqual(1, Page, "and turns it");
            Assert.AreEqual(jumps + 1, NavJumps.Count, "one jump");

            // - MOSTLY VERTICAL at the slop (60 degrees), then far sideways: the ScrollView took it, the page never follows nor turns
            float y2 = Scroll.scrollOffset.y;
            bool movedSideways = false;
            var steep = Line(Vector2.zero, new Vector2(-8f, -14f), 2).Concat(Line(new Vector2(-8f, -14f), new Vector2(-300f, -60f), 14));
            yield return HandDrag(new Vector2(Scroll.contentViewport.worldBound.center.x, start.y), steep, _ => movedSideways |= Pager.Offset != 0f);
            yield return CardTestInput.Settle(0.5f);
            Assert.IsFalse(movedSideways, "judged vertical: the page never followed, though the finger then went 300 sideways");
            Assert.AreEqual(1, Page, "nor turned");
            Assert.Greater(Scroll.scrollOffset.y, y2 + 5f, "the drag scrolled the page instead (" + y2 + " -> " + Scroll.scrollOffset.y + ")");
        }

        // SwipePageRule on the real card: a swipe short of a fifth of the page slides back; the first page has nothing before it and the last
        // nothing after (no wrap), and towards nothing the page only gives a little; one page per swipe however long
        [UnityTest]
        public IEnumerator AShortSwipeSlidesBack_TheFirstAndLastPagesGoNoFurther_AndOneSwipeTurnsOnePage()
        {
            yield return UsePages();
            yield return OpenFull(Lamp);
            int count = Nav.Model.Sections.Count;
            Assert.AreEqual(8, count, "precondition: The Lamp's eight sections are eight pages");

            float shortSwipe = Pager.PageWidth * SwipePageRule.MinTravelShare * 0.6f;
            float shortFollowed = 0f;
            yield return HandDrag(EdgePoint(), Line(Vector2.zero, new Vector2(-shortSwipe, 0f), 8), _ => shortFollowed = Mathf.Min(shortFollowed, Pager.Offset));
            Assert.Less(shortFollowed, -shortSwipe * 0.5f, "precondition: the page followed the short swipe (" + shortFollowed + ")");
            yield return Rested();
            Assert.AreEqual(0, Page, "a swipe short of the threshold: no turn");
            Assert.IsEmpty(NavJumps);

            // - a swipe to the RIGHT starts at the page's left side (a finger that runs off the screen's edge sends nothing)
            float least = 0f;
            yield return HandDrag(LeftPoint(), Line(Vector2.zero, new Vector2(Pager.PageWidth * 0.6f, 0f), 10), _ => least = Mathf.Max(least, Pager.Offset));
            Assert.AreEqual(Pager.PageWidth * 0.6f * SwipePageRule.EdgeResistance, least, 1f, "towards no page the first page only gives a fraction");
            Assert.AreEqual(Visibility.Hidden, FamilyPager.Previous.resolvedStyle.visibility, "the first page offers no way back");
            Assert.AreEqual(DisplayStyle.None, Pager.PeekBack.resolvedStyle.display, "and nothing peeks before it");
            yield return Rested();
            Assert.AreEqual(0, Page, "the first page stays the first");

            yield return HandDrag(EdgePoint(), Line(Vector2.zero, new Vector2(-Pager.PageWidth * 0.95f, 0f), 12));
            yield return Rested();
            Assert.AreEqual(1, Page, "a long swipe still turns ONE page");

            // - to the last page with real taps on the chevron, then try past it
            for (int i = Page; i < count - 1; i++)
            {
                yield return CardTestInput.Tap(FamilyPager.Next.panel, FamilyPager.Next.worldBound.center);
                yield return Rested();
            }
            Assert.AreEqual(count - 1, Page, "precondition: the last page");
            Assert.AreEqual(Visibility.Hidden, FamilyPager.Next.resolvedStyle.visibility, "the last page offers no way on");
            Assert.AreEqual(DisplayStyle.None, Pager.Peek.resolvedStyle.display, "and no next page peeks");
            int jumps = NavJumps.Count;
            yield return HandDrag(EdgePoint(), Line(Vector2.zero, new Vector2(-Pager.PageWidth * 0.6f, 0f), 10));
            yield return Rested();
            Assert.AreEqual(count - 1, Page, "no wrap to the first page");
            Assert.AreEqual(jumps, NavJumps.Count, "and no jump");
        }

        // ---------------- the Family Pager, the spy, other elements ----------------

        [UnityTest]
        public IEnumerator TheFamilyPager_NamesThePageShown_ItsChevronsTurnOnePage_AndTheNextPagePeeksAtTheEdge()
        {
            yield return UsePages();
            yield return OpenFull(Lamp);
            Assert.IsNotNull(FamilyPager, "precondition: the Family Pager is drawn");
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            float peek = CardTestInput.TokenPx("--ts-page-peek");
            // - the words the card reads, from the same tables (expected values come from their source, 40-testing 4.2.4b)
            var strings = new CardStrings(Card.StringTable.Entries(), CardStringSources.Shared.Entries(), null, "en", "en");
            for (int expected = 0; expected < 3; expected++)
            {
                var section = Nav.Model.Sections[expected];
                Assert.AreEqual(expected, Page);
                Assert.AreEqual(expected, Nav.Current, "the spy names the page shown");
                Assert.AreEqual(expected, FamilyPager.CurrentIndex, "and the pager follows the spy");
                Assert.AreEqual(strings.Get(section.LabelKey), FamilyPager.Words.text, "the pager names the section in the visitor's words");
                StringAssert.StartsWith((expected + 1).ToString(), FamilyPager.Position.text, "its place: " + FamilyPager.Position.text);
                StringAssert.EndsWith(Nav.Model.Sections.Count.ToString(), FamilyPager.Position.text);
                // - the next page's cue: its words, a peek's width inside the viewport's right edge
                var next = Nav.Model.Sections[expected + 1];
                Assert.AreEqual(strings.Get(next.LabelKey), Pager.PeekWords.text, "the peek names the NEXT page");
                var viewport = Scroll.contentViewport.worldBound;
                Assert.AreEqual(peek, viewport.xMax - Pager.Peek.worldBound.xMin, OnePixel, "a peek's width of it shows at the edge");
                foreach (var chevron in new[] { FamilyPager.Previous, FamilyPager.Next })
                    Assert.IsTrue(chevron.worldBound.width >= touch - 0.5f && chevron.worldBound.height >= touch - 0.5f,
                        chevron.name + " is a whole touch target: " + chevron.worldBound);

                yield return CardTestInput.Tap(FamilyPager.Next.panel, FamilyPager.Next.worldBound.center);
                yield return Rested();
                Assert.AreEqual(BuiltInNavElements.FamilyPager, NavJumps.Last().Variant, "a chevron reports itself");
            }
            Assert.AreEqual(3, Page, "three real taps on the right chevron: three pages on");
            AssertShowsOnly(3, "after the chevrons");
            AssertLanded(Nav.Model.Sections[3].Targets[0], "the page's first block");

            yield return CardTestInput.Tap(FamilyPager.Previous.panel, FamilyPager.Previous.worldBound.center);
            yield return Rested();
            Assert.AreEqual(2, Page, "the left chevron: one page back");

            // - a real drag inside the page: it scrolls, the spy and the pager stay on it
            var viewportNow = Scroll.contentViewport.worldBound;
            if (Range > 20f)
            {
                yield return CardTestInput.TouchDrag(Sheet.Root.panel, new Vector2(viewportNow.center.x, viewportNow.yMin + viewportNow.height * 0.8f),
                    new Vector2(0f, -viewportNow.height * 0.4f), frames: 16);
                yield return CardTestInput.Settle(0.5f);
            }
            Assert.AreEqual(2, Nav.Current, "the spy never leaves the page shown");
            Assert.AreEqual(2, FamilyPager.CurrentIndex);
        }

        [UnityTest]
        public IEnumerator AJumpFromABottomBarSlot_TurnsToItsPage_AndLandsItsFirstBlock()
        {
            yield return UsePages(nav => nav.bottom = BuiltInNavElements.BottomBar);
            yield return OpenFull(Lamp);
            var bar = Sheet.Slots.BottomView as BottomBarNavView;
            Assert.IsNotNull(bar, "precondition: the Bottom Bar is drawn");
            Assert.AreEqual(BuiltInNavElements.FamilyPager, Sheet.Slots.TopKey, "the pager stays at the top beside a bottom bar");
            for (int i = bar.Items.Count - 1; i >= 1; i--)
            {
                var slot = bar.Items[i].Button;
                yield return CardTestInput.Tap(slot.panel, slot.worldBound.center);
                yield return Rested();
                Assert.AreEqual(i, Page, "a real tap on bar slot " + i + " turned to its page");
                Assert.AreEqual(i, FamilyPager.CurrentIndex, "and the pager names it");
                AssertShowsOnly(i, "after bar slot " + i);
                AssertLanded(Nav.Model.Sections[i].Targets[0], "slot " + i + "'s first block");
                Assert.AreEqual(BuiltInNavElements.BottomBar, NavJumps.Last().Variant);
            }
        }

        // ---------------- the owners ----------------

        // Exactly as a tab put away (step 13): a page the visitor left holds no view and no file -- asked of the OWNERS
        [UnityTest]
        public IEnumerator APageLeft_LetsItsMediaGo_ThePlayingVideoAndThePreviewSlot()
        {
            yield return UsePages();
            yield return OpenFull(Lamp);
            int media = SectionIndex(BuiltInFamilies.Media);
            Assert.Greater(media, 0, "precondition: Media is a later page");
            Nav.JumpTo(media, "test");
            yield return Rested();
            Assert.AreEqual(media, Page);
            CollectionAssert.Contains(Card.PreviewService.HeldKeys, "block_59", "Media shows: the inline model holds its preview slot");
            var video = Stack.ViewOfBlock("block_56") as VideoBlockView;
            Assert.IsNotNull(video, "precondition: the inline video is bound while its page shows");
            Scroll.ScrollTo(Stack.SlotOfBlock("block_56"));
            yield return CardTestInput.Settle(0.4f);
            yield return CardTestInput.Tap(video.Panel.PlayOverlay.panel, video.Panel.PlayOverlay.worldBound.center);
            yield return null;
            Assert.AreEqual(CardVideoState.Playing, _video.State, "a real tap played the video");

            // - a real swipe to the next page, from the page's edge beside the playing video
            yield return HandDrag(EdgePoint(video.Panel.PlayOverlay.worldBound.center.y), Line(Vector2.zero, new Vector2(-Pager.PageWidth * 0.5f, 0f), 10));
            yield return Rested();
            Assert.AreEqual(media + 1, Page, "the swipe turned the page");
            Assert.IsNull(Stack.ViewOfBlock("block_56"), "the video's block gave its view back");
            Assert.AreNotEqual(CardVideoState.Playing, _video.State, "the video stopped: nothing on screen shows it");
            Assert.IsFalse(_video.IsCurrent(video.Track), "and the owner no longer serves that block");
            CollectionAssert.DoesNotContain(Card.PreviewService.HeldKeys, "block_59", "the model's preview slot given back");
            foreach (string key in Nav.Model.Sections[media].Targets)
                Assert.AreEqual(0, Stack.HeldMediaOf(key), key + ": no file held while its page is left");

            yield return CardTestInput.Tap(FamilyPager.Previous.panel, FamilyPager.Previous.worldBound.center);
            yield return Rested();
            CollectionAssert.Contains(Card.PreviewService.HeldKeys, "block_59", "back on Media: the slot is taken again");
        }

        // A part that drags by itself keeps its drags inside a page (SwipeGrabRule.LeavesToPart): the gallery's strip scrolls sideways and the
        // inline 3D stage turns -- the page neither follows nor turns
        [UnityTest]
        public IEnumerator InsideAPage_AStripStillSwipes_AndThe3DStageStillTurns_ThePageStays()
        {
            yield return UsePages();
            yield return OpenFull(Lamp);
            int media = SectionIndex(BuiltInFamilies.Media);
            Nav.JumpTo(media, "test");
            yield return Rested();

            var strip = Stack.BoundViews.OfType<GalleryBlockView>()
                .FirstOrDefault(v => v.Track != null && v.Track.contentContainer.layout.width > v.Track.contentViewport.layout.width + 1f);
            Assert.IsNotNull(strip, "precondition: the Media page has a gallery whose strip runs past its width");
            Scroll.ScrollTo(strip.Track);
            yield return CardTestInput.Settle(0.4f);
            float before = strip.Track.scrollOffset.x;
            bool followed = false;
            var stripStart = new Vector2(strip.Track.worldBound.center.x, strip.Track.worldBound.center.y);
            yield return HandDrag(stripStart, Line(Vector2.zero, new Vector2(-160f, 0f), 10), _ => followed |= Pager.Offset != 0f);
            yield return CardTestInput.Settle(0.3f);
            Assert.Greater(strip.Track.scrollOffset.x, before + 40f, "the strip scrolled (" + before + " -> " + strip.Track.scrollOffset.x + ")");
            Assert.IsFalse(followed, "the page never followed the strip's swipe");
            Assert.AreEqual(media, Page, "nor turned");

            var stage = Stack.BoundViews.OfType<ModelTurntableBlockView>().FirstOrDefault(v => v.Display == CardOptions.DisplayInline);
            Assert.IsNotNull(stage, "precondition: the Media page has the inline 3D stage");
            Scroll.ScrollTo(stage.Frame);
            yield return CardTestInput.Settle(0.4f);
            float yaw = stage.State.Yaw;
            followed = false;
            yield return HandDrag(stage.Frame.worldBound.center, Line(Vector2.zero, new Vector2(-150f, 0f), 10), _ => followed |= Pager.Offset != 0f);
            yield return null;
            Assert.AreNotEqual(yaw, stage.State.Yaw, "the sideways drag turned the model");
            Assert.IsFalse(followed, "the page never followed it");
            yield return CardTestInput.Settle(0.3f);
            Assert.AreEqual(media, Page, "nor turned");
            Assert.IsEmpty(NavJumps.Where(j => j.Variant == CardPagerView.SwipeKey), "no page turn at all");
        }

        [UnityTest]
        public IEnumerator ACardWithNothingToSwitchBetweenScrolls_AndScrollTakesThePagesAway()
        {
            yield return UsePages();
            yield return OpenFull(LampMilitary);
            Assert.IsFalse(Nav.Model.ShowsNavigation, "precondition: Lamp - Military is under Show Navigation From");
            Assert.AreEqual(CardOptions.LayoutScroll, Sheet.Layout.Effective, "nothing to switch between: it scrolls");
            Assert.IsFalse(Pager.IsOn, "no pages");
            Assert.IsFalse(Sheet.Root.ClassListContains(CardPagerView.PagedClass), "no inset for a peek");
            Assert.AreEqual(DisplayStyle.None, Pager.Peek.resolvedStyle.display, "and no peek");

            yield return OpenFull(Lamp);
            Assert.IsTrue(Pager.IsOn, "The Lamp: pages");
            yield return UseNavigation(nav => nav.layout = CardOptions.LayoutScroll);
            Assert.IsFalse(Pager.IsOn, "live: Scroll takes the pages away");
            Assert.AreEqual(DisplayStyle.None, Pager.Peek.resolvedStyle.display);
            foreach (string key in Nav.Model.Targets)
                Assert.IsNotNull(Stack.ViewOfBlock(key), key + " drawn again in the one scroll");
            Assert.AreEqual(BuiltInNavElements.TextTabs, Sheet.Slots.TopKey, "and the Top Bar the wall picked is back");
        }

        // ---------------- what it looks like ----------------

        [UnityTest]
        public IEnumerator Renders_TheLampInPages_AtRest_MidSwipe_AndThePager_InEnglishAndPortuguese()
        {
            yield return UsePages();
            foreach (string language in new[] { "en", "pt" })
            {
                yield return OpenFull(Lamp);
                if (language == "pt")
                {
                    yield return CardTestInput.Tap(Sheet.LanguageButton.panel, Sheet.LanguageButton.worldBound.center);
                    yield return CardTestInput.Settle(0.3f);
                }
                // - a rebind of the same point keeps its page (the language switch): start this language on the first one
                Nav.JumpTo(0, "test", landsAtOnce: true);
                yield return Rested();
                Assert.AreEqual(0, Page, "precondition: the first page");
                // - every section's name whole between the chevrons, in this language
                for (int i = 0; i < Nav.Model.Sections.Count; i++)
                {
                    if (i > 0)
                    {
                        yield return CardTestInput.Tap(FamilyPager.Next.panel, FamilyPager.Next.worldBound.center);
                        yield return Rested();
                    }
                    var middle = FamilyPager.Words.parent.parent.worldBound;
                    Assert.GreaterOrEqual(FamilyPager.Words.worldBound.xMin, FamilyPager.Previous.worldBound.xMax - 0.5f, language + " page " + i + ": the name starts after the left chevron");
                    Assert.LessOrEqual(FamilyPager.Words.worldBound.xMax, FamilyPager.Next.worldBound.xMin + 0.5f, language + " page " + i + ": and ends before the right one");
                    var size = FamilyPager.Words.MeasureTextSize(FamilyPager.Words.text, 0f, VisualElement.MeasureMode.Undefined, 0f, VisualElement.MeasureMode.Undefined);
                    Assert.LessOrEqual(size.x, FamilyPager.Words.worldBound.width + 0.5f, language + " '" + FamilyPager.Words.text + "' is whole (" + size.x + " in " + FamilyPager.Words.worldBound.width + ")");
                    Assert.IsTrue(middle.width > 0f);
                    if (i == 0) yield return CardGalleryChecks.Render("Nav_Lamp_pages_rest_" + language + "_390");
                }
                // - back to the first page, then hold a swipe half way
                Nav.JumpTo(0, "test", landsAtOnce: true);
                yield return Rested();
                var panel = Sheet.Root.panel;
                var start = EdgePoint();
                CardTestInput.TouchDown(panel, start);
                yield return null;
                foreach (var step in Line(Vector2.zero, new Vector2(-Pager.PageWidth * 0.45f, 0f), 8))
                {
                    CardTestInput.TouchMove(panel, start + step);
                    yield return null;
                }
                Assert.Less(Pager.Offset, -Pager.PageWidth * 0.3f, "precondition: the page is half way");
                yield return CardGalleryChecks.Render("Nav_Lamp_pages_midswipe_" + language + "_390");
                CardTestInput.TouchUp(panel, start + new Vector2(-Pager.PageWidth * 0.45f, 0f));
                yield return Rested();
                Assert.AreEqual(1, Page);
                yield return CardGalleryChecks.Render("Nav_Lamp_pages_second_" + language + "_390");
                if (language == "pt")
                {
                    yield return CardTestInput.Tap(Sheet.LanguageButton.panel, Sheet.LanguageButton.worldBound.center);
                    yield return CardTestInput.Settle(0.3f);
                }
            }
        }
    }
}
