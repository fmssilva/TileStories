using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of step 10's scrubber (_3.2 section 5.5: edge_scrubber) on the REAL LivingRoomScene with The Lamp's real card (8 families,
    // Grouped By Family, the default Top Bar) and real touches -- panel events of a touch pointer through CardTestInput, one event at a time
    // where a test must look at the card while the finger is still down:
    //   - the stops do not move with the scrubber, and it is drawn only where the viewport holds EVERY dot's full target (Full: yes; The
    //     Lamp's Half and Peek: no, and nothing there takes a finger)
    //   - TAPS ONLY (WCAG 2.2 SC 2.5.7): a real tap on every dot reaches its family, with no drag at all
    //   - a real drag down the edge: the bubble names the family under the finger and sits at its dot, the card does not scroll under it,
    //     and the release lands the family's first block; a drag past either end keeps the end family for one target and picks nothing
    //     further out; a drag that never leaves the slop is a tap
    //   - a finger that lands beside the column, or above / below it, scrolls the card as usual, and the dot of the family read follows
    //   - the bubble's words are whole in English and Portuguese
    //   - what the dots cover on a block that reaches the right edge: a gallery's filmstrip and carousel (measured, recorded in the guide)
    public class PoiCardEdgeSceneTests : SearchSceneFixture
    {
        private const string Lamp = "lamp";

        private PoiCardSheetView Sheet => Card.Sheet;
        private NavController Nav => Sheet.Nav;
        private CardNavModel Model => Nav.Model;
        private ScrollView Scroll => Sheet.Stack.Scroll;
        private EdgeScrubberNavView Scrubber => (EdgeScrubberNavView)Sheet.Slots.EdgeView;

        private sealed class RecordingCardEvents : ICardEvents
        {
            public readonly List<CardEvent> Raised = new();
            public void Raise(CardEvent cardEvent) => Raised.Add(cardEvent);
        }

        private RecordingCardEvents _events;
        private List<string> _shippedLanguages;

        [UnitySetUp]
        public IEnumerator UseAnInMemoryCardStateAndRecordTheEvents()
        {
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            _events = new RecordingCardEvents();
            Card.Events = _events;
            yield break;
        }

        // ---------------- helpers ----------------

        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;

        private float Range => NavScrollRule.Range(Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height);

        private List<CardEvent> NavJumps => _events.Raised.Where(e => e.Kind == CardEventKinds.NavJump).ToList();

        private CardStrings Words(string language) =>
            new(Card.StringTable.Entries(), CardStringSources.Shared.Entries(), null, language, "en");

        // The wall's navigation for a test: Grouped By Family (every family one section), the default Top Bar, the Scrubber on the Edge, the
        // card in `language`; pushed through the live seam
        private IEnumerator UseScrubber(string language = "en", string edge = BuiltInNavElements.EdgeScrubber)
        {
            var copy = ConfigCopy();
            var nav = copy.card_settings.navigation;
            nav.block_order = CardOptions.BlockOrderByFamily;
            nav.edge = edge;
            _shippedLanguages ??= new List<string>(ConfigCopy().card_settings.languages);
            copy.card_settings.languages = language == "en" ? new List<string>(_shippedLanguages) : new List<string> { language, "en" };
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        private IEnumerator Open(SheetStopRule.Stop stop)
        {
            if (SelectionEventBus.CurrentPoiId != Lamp)
            {
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
                SelectionEventBus.Select(Lamp);
                yield return CardTestInput.Settle();
            }
            Scroll.scrollOffset = Vector2.zero;
            Sheet.SetStop(stop);
            yield return CardTestInput.Settle();
            Assert.AreEqual(stop, Sheet.Stop, "precondition: the card rests at " + stop);
            yield return Rested();
        }

        // The sheet's height animates in real time: every measure waits for it to rest at its stop's height
        private IEnumerator Rested()
        {
            float rest = Sheet.Stops.HeightOf(Sheet.Stop);
            for (int frame = 0; frame < 600 && Mathf.Abs(Sheet.Root.layout.height - rest) > OnePixel; frame++) yield return null;
            Assert.AreEqual(rest, Sheet.Root.layout.height, OnePixel, "precondition: the sheet rests at its " + Sheet.Stop + " height");
            yield return null;
            yield return null;
        }

        private IEnumerator Landed()
        {
            for (int frame = 0; frame < 600 && Nav.IsJumping; frame++) yield return null;
            Assert.IsFalse(Nav.IsJumping, "the jump came to rest");
            yield return CardTestInput.Settle(0.3f);
        }

        // The family's first block lands at the viewport's top (relative, one pixel), or the card's end clamps it
        private void AssertLanded(int section, string name)
        {
            var slot = Sheet.Stack.SlotOfBlock(Model.Sections[section].Targets[0]);
            if (Sheet.Stack.LandingTop(slot) <= Range - OnePixel)
                Assert.AreEqual(0f, Sheet.Stack.LandingElement(slot).worldBound.yMin - Scroll.contentViewport.worldBound.yMin, OnePixel, name + ": its first block's top meets the viewport's top");
            else
                Assert.AreEqual(Range, Scroll.scrollOffset.y, OnePixel, name + ": a late block clamps at the end of the range");
        }

        private Vector2 DotAt(int index) => Scrubber.Items[index].Button.worldBound.center;

        // A real tap by a touch pointer on a point of the card layer
        private IEnumerator TouchTap(Vector2 at)
        {
            yield return CardTestInput.TouchDrag(Sheet.Layer.panel, at, Vector2.zero, frames: 1);
            yield return CardTestInput.Settle();
        }

        // What a finger meets at a point of the card's panel
        private VisualElement TopmostAt(Vector2 point) => Sheet.Layer.panel.Pick(point);

        private bool IsInScrubber(VisualElement element) => element != null && Scrubber.Root.Contains(element);

        // A finger put down on dot `from` and walked, a few panel units a frame, to the point `to` (the finger is still DOWN afterwards)
        private IEnumerator PressAndWalk(Vector2 from, Vector2 to, float step = 12f)
        {
            var panel = Sheet.Layer.panel;
            CardTestInput.TouchDown(panel, from);
            yield return null;
            int frames = Mathf.Max(1, Mathf.CeilToInt((to - from).magnitude / step));
            for (int i = 1; i <= frames; i++)
            {
                CardTestInput.TouchMove(panel, Vector2.Lerp(from, to, (float)i / frames));
                yield return null;
            }
        }

        private IEnumerator Lift(Vector2 at)
        {
            CardTestInput.TouchUp(Sheet.Layer.panel, at);
            yield return null;
        }

        // ---------------- the slot and the stops ----------------

        // Section 4.2: the edge slot takes no room in the flow and changes no stop: with the scrubber, every stop is the card's own (measured
        // with no edge element first; _3.3 P4.3 re-based it from the typed 5a standard, 195.00 / 337.60 / 796.00, which the new faces moved to
        // 168 at Peek -- a title on one line). The rail is drawn only where the viewport holds every dot's full target: at Full yes, one per
        // family; at The Lamp's Half and at Peek no -- and the hidden rail takes no finger
        [UnityTest]
        public IEnumerator TheStops_AreUnchangedWithTheScrubber_AndTheRailIsDrawnOnlyWhereEveryDotGetsAFullTarget()
        {
            var own = new Dictionary<SheetStopRule.Stop, float>();
            foreach (string edge in new[] { CardOptions.NavNone, BuiltInNavElements.EdgeScrubber })
            {
                yield return UseScrubber(edge: edge);
                foreach (var stop in new[] { SheetStopRule.Stop.Peek, SheetStopRule.Stop.Half, SheetStopRule.Stop.Full })
                {
                    yield return Open(stop);
                    if (edge == CardOptions.NavNone) own[stop] = Sheet.Stops.HeightOf(stop);
                    else Assert.AreEqual(own[stop], Sheet.Stops.HeightOf(stop), 0.01f, edge + ": the " + stop + " stop did not move");
                }
            }
            Assert.Less(own[SheetStopRule.Stop.Peek], own[SheetStopRule.Stop.Half], "precondition: three distinct stops were measured");

            yield return Open(SheetStopRule.Stop.Full);
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            Assert.IsNotNull(Sheet.Slots.EdgeView, "the wall's pick is bound");
            Assert.IsTrue(Scrubber.IsShown, "Full: the viewport (" + Scroll.contentViewport.layout.height + ") holds every target");
            Assert.AreEqual(Model.Sections.Count, Scrubber.Items.Count, "one dot per family");
            Assert.GreaterOrEqual(Scroll.contentViewport.layout.height, Model.Sections.Count * touch, "precondition: the room is real");
            var column = Scrubber.Column.worldBound;
            foreach (var item in Scrubber.Items)
            {
                Assert.IsTrue(Sheet.Layer.worldBound.Contains(item.Button.worldBound.center), item.Words + ": on screen");
                Assert.IsTrue(IsInScrubber(TopmostAt(item.Button.worldBound.center)), item.Words + ": what a finger meets at the dot is the scrubber");
            }
            Assert.IsFalse(Scrubber.Bubble.ClassListContains(EdgeScrubberNavView.BubbleShownClass), "no bubble at rest");

            foreach (var stop in new[] { SheetStopRule.Stop.Half, SheetStopRule.Stop.Peek })
            {
                yield return Open(stop);
                Assert.IsFalse(Scrubber.IsShown, stop + ": the viewport (" + Scroll.contentViewport.layout.height + ") cannot hold " + Model.Sections.Count + " x " + touch);
                Assert.AreEqual(Visibility.Hidden, Scrubber.Root.resolvedStyle.visibility, stop + ": hidden");
                // - a hidden rail takes no finger: at the old dots' own points, and on every dot's target
                Assert.IsFalse(IsInScrubber(TopmostAt(column.center)), stop + ": nothing of the scrubber at the old column's centre");
                foreach (var item in Scrubber.Items)
                    Assert.IsFalse(IsInScrubber(TopmostAt(item.Button.worldBound.center)), stop + ": nor at " + item.Words + "'s target");
            }
            yield return Open(SheetStopRule.Stop.Full);
            Assert.IsTrue(Scrubber.IsShown, "and back at Full the rail is back");
        }

        // ---------------- taps only (WCAG 2.2 SC 2.5.7) ----------------

        // The whole point of the element: EVERY family reachable by a plain tap on its dot, with no drag at all -- a real touch tap on each dot
        // of The Lamp's eight, from the top of the card: it lands that family's first block, raises one nav_jump named by the scrubber, and
        // the finger was never claimed as a scrub (no bubble, no dot pointed)
        [UnityTest]
        public IEnumerator EveryFamilyIsReachedByARealTapOnItsDot_WithNoDragAtAll()
        {
            yield return UseScrubber();
            yield return Open(SheetStopRule.Stop.Full);
            Assert.AreEqual(8, Model.Sections.Count, "precondition: The Lamp's eight families");
            for (int i = 0; i < Model.Sections.Count; i++)
            {
                yield return Open(SheetStopRule.Stop.Full);
                int jumps = NavJumps.Count;
                string name = Scrubber.Items[i].Words;
                Assert.AreEqual(0f, Scroll.scrollOffset.y, OnePixel, name + ": precondition: every tap starts from the top of the card");
                yield return TouchTap(DotAt(i));
                yield return Landed();
                Assert.AreEqual(jumps + 1, NavJumps.Count, name + ": one jump for one tap");
                var jump = NavJumps.Last();
                Assert.AreEqual(BuiltInNavElements.EdgeScrubber, jump.Variant, name + ": by the scrubber");
                Assert.AreEqual(Model.Sections[i].Key, jump.Value, name + ": to the family tapped");
                Assert.AreEqual(Model.Sections[i].Targets[0], jump.BlockKey, name + ": its first block");
                AssertLanded(i, name);
                Assert.IsFalse(Scrubber.IsDragging, name + ": the tap was never taken as a scrub");
                Assert.AreEqual(-1, Scrubber.Pointed, name + ": no dot was pointed by a finger");
                Assert.IsFalse(Scrubber.Bubble.ClassListContains(EdgeScrubberNavView.BubbleShownClass), name + ": no bubble for a tap");
                Assert.IsTrue(Scrubber.Items[Nav.Current].Button.ClassListContains(EdgeScrubberNavView.CurrentClass), name + ": the spy marks the family read");
            }
        }

        // A finger that lands on a dot and moves less than the slop is a tap, not a scrub (SwipeGrab judges once it leaves the slop)
        [UnityTest]
        public IEnumerator AFingerThatNeverLeavesTheSlop_IsATapOnItsDot()
        {
            yield return UseScrubber();
            yield return Open(SheetStopRule.Stop.Full);
            int jumps = NavJumps.Count;
            const int section = 3;
            var at = DotAt(section);
            var panel = Sheet.Layer.panel;
            CardTestInput.TouchDown(panel, at);
            yield return null;
            CardTestInput.TouchMove(panel, at + new Vector2(0f, SwipeGrabRule.Slop * 0.5f));
            yield return null;
            Assert.IsFalse(Scrubber.IsDragging, "half the slop: not a scrub yet");
            Assert.AreEqual(-1, Scrubber.Pointed, "and no dot is pointed");
            yield return Lift(at + new Vector2(0f, SwipeGrabRule.Slop * 0.5f));
            yield return Landed();
            Assert.AreEqual(jumps + 1, NavJumps.Count, "a tap: one jump");
            Assert.AreEqual(Model.Sections[section].Key, NavJumps.Last().Value, "to the dot it landed on");
            AssertLanded(section, "the dot under a wobbling finger");
        }

        // ---------------- the drag ----------------

        // A real drag down the edge from the first dot to the sixth: the bubble follows, naming the family under the finger and sitting at its
        // own dot; the card does NOT scroll under the finger and no jump happens until the release; the release lands the family's first block
        [UnityTest]
        public IEnumerator ARealDragDownTheEdge_MovesTheBubble_AndTheReleaseLandsTheFamilysFirstBlock()
        {
            yield return UseScrubber();
            yield return Open(SheetStopRule.Stop.Full);
            var words = Words("en");
            int jumps = NavJumps.Count;
            float offset = Scroll.scrollOffset.y;
            var panel = Sheet.Layer.panel;

            CardTestInput.TouchDown(panel, DotAt(0));
            yield return null;
            Assert.IsFalse(Scrubber.IsDragging, "put down on a dot: nothing is captured at touch-down");
            var finger = DotAt(0);
            foreach (int target in new[] { 3, 5 })
            {
                yield return WalkTo(finger, DotAt(target));
                finger = DotAt(target);
                string name = Scrubber.Items[target].Words;
                Assert.IsTrue(Scrubber.IsDragging, name + ": the finger is a scrub now");
                Assert.AreEqual(target, Scrubber.Pointed, name + ": the dot under the finger is pointed");
                Assert.IsTrue(Scrubber.Items[target].Button.ClassListContains(EdgeScrubberNavView.PointedClass), name + ": and marked");
                Assert.IsTrue(Scrubber.Bubble.ClassListContains(EdgeScrubberNavView.BubbleShownClass), name + ": the bubble shows");
                Assert.AreEqual(words.Get(Model.Sections[target].LabelKey), Scrubber.BubbleLabel.text, name + ": it names the family");
                Assert.AreEqual(DotAt(target).y, Scrubber.Bubble.worldBound.center.y, OnePixel * 2f, name + ": at the dot's own height");
                Assert.LessOrEqual(Scrubber.Bubble.worldBound.xMax, Scrubber.Items[target].Button.worldBound.xMin + OnePixel, name + ": left of the edge, never under the thumb");
                Assert.AreEqual(offset, Scroll.scrollOffset.y, OnePixel, name + ": the card did not scroll under the scrub");
                Assert.AreEqual(jumps, NavJumps.Count, name + ": nothing jumps before the release");
                if (target == 5) yield return Capture("Edge_Lamp_scrub_en");
            }
            yield return Lift(DotAt(5));
            yield return Landed();
            Assert.AreEqual(jumps + 1, NavJumps.Count, "the release: one jump");
            Assert.AreEqual(Model.Sections[5].Key, NavJumps.Last().Value, "to the family the finger was on");
            Assert.AreEqual(BuiltInNavElements.EdgeScrubber, NavJumps.Last().Variant);
            AssertLanded(5, "the release");
            Assert.IsFalse(Scrubber.Bubble.ClassListContains(EdgeScrubberNavView.BubbleShownClass), "the bubble went with the finger");
            Assert.AreEqual(-1, Scrubber.Pointed, "no dot pointed any more");
        }

        // From one dot to another in steps, the finger staying down; leaves the finger at the end point
        private IEnumerator WalkTo(Vector2 from, Vector2 to)
        {
            var panel = Sheet.Layer.panel;
            int frames = Mathf.Max(1, Mathf.CeilToInt((to - from).magnitude / 12f));
            for (int i = 1; i <= frames; i++)
            {
                CardTestInput.TouchMove(panel, Vector2.Lerp(from, to, (float)i / frames));
                yield return null;
            }
        }

        // The ends: a finger one target past the first or last dot still keeps that dot (a thumb overshoots); further out it picks nothing --
        // the bubble goes, and the release moves nothing
        [UnityTest]
        public IEnumerator ADragPastTheEnds_KeepsTheEndFamilyForOneTarget_AndFurtherOutPicksNothing()
        {
            yield return UseScrubber();
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            yield return Open(SheetStopRule.Stop.Full);
            int last = Model.Sections.Count - 1;
            int jumps = NavJumps.Count;

            // - one target below the last dot: still the last family; the release lands it
            yield return PressAndWalk(DotAt(last - 1), DotAt(last) + new Vector2(0f, touch * 0.9f));
            Assert.AreEqual(last, Scrubber.Pointed, "just below the last dot (within one target): still the last family");
            Assert.IsTrue(Scrubber.Bubble.ClassListContains(EdgeScrubberNavView.BubbleShownClass), "and the bubble names it");
            yield return Lift(DotAt(last) + new Vector2(0f, touch * 0.9f));
            yield return Landed();
            Assert.AreEqual(jumps + 1, NavJumps.Count, "released over the overshoot: it lands");
            Assert.AreEqual(Model.Sections[last].Key, NavJumps.Last().Value);

            // - well past the first dot (more than one target above it): nothing is pointed, the bubble is gone, the release moves nothing
            yield return Open(SheetStopRule.Stop.Full);
            jumps = NavJumps.Count;
            float before = Scroll.scrollOffset.y;
            var farAbove = DotAt(0) - new Vector2(0f, touch * 1.6f);
            yield return PressAndWalk(DotAt(2), farAbove);
            Assert.IsTrue(Scrubber.IsDragging, "the finger is still a scrub while it is out there");
            Assert.AreEqual(-1, Scrubber.Pointed, "more than one target above the first dot: no dot");
            Assert.IsFalse(Scrubber.Bubble.ClassListContains(EdgeScrubberNavView.BubbleShownClass), "no bubble: a release here picks nothing");
            yield return Lift(farAbove);
            yield return CardTestInput.Settle();
            Assert.AreEqual(jumps, NavJumps.Count, "released outside the column: no jump");
            Assert.AreEqual(before, Scroll.scrollOffset.y, OnePixel, "and the card did not move");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "nor the sheet");
        }

        // ---------------- a finger that is not the scrubber's ----------------

        // A real finger that lands BESIDE the column (left of the dots) and drags up scrolls the card as usual -- no jump, no bubble, no dot
        // pointed -- and the dot of the family read follows (marked twice: a larger disc and an accent edge, no other dot)
        [UnityTest]
        public IEnumerator AFingerBesideTheDots_ScrollsTheCardAsUsual_AndTheMarkFollowsWhatIsRead()
        {
            yield return UseScrubber();
            yield return Open(SheetStopRule.Stop.Full);
            int jumps = NavJumps.Count;
            Assert.AreEqual(0, Nav.Current, "precondition: the card starts at its first family");
            var viewport = Scroll.contentViewport.worldBound;
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            var start = new Vector2(viewport.xMax - touch - 24f, viewport.center.y + 150f);
            Assert.IsFalse(IsInScrubber(TopmostAt(start)), "precondition: the finger lands on the content, not on the scrubber");
            // - The Lamp's first family is long: several real drags until the spy leaves it
            for (int swipe = 0; swipe < 10 && Nav.Current == 0; swipe++)
            {
                yield return CardTestInput.TouchDrag(Sheet.Layer.panel, start, new Vector2(0f, -420f), frames: 14);
                yield return CardTestInput.Settle(0.5f);
            }
            Assert.Greater(Scroll.scrollOffset.y, 100f, "the card scrolled under the finger (" + Scroll.scrollOffset.y + " of " + Range + ")");
            Assert.AreEqual(jumps, NavJumps.Count, "no jump");
            Assert.IsFalse(Scrubber.IsDragging, "the scrubber never claimed it");
            Assert.AreEqual(-1, Scrubber.Pointed);
            Assert.Greater(Nav.Current, 0, "the spy moved on to a later family");
            var marked = Scrubber.Items.Where(i => i.Button.ClassListContains(EdgeScrubberNavView.CurrentClass)).ToList();
            Assert.AreEqual(1, marked.Count, "exactly one dot is marked");
            Assert.AreSame(Scrubber.Items[Nav.Current], marked[0], "the dot of the family read: " + marked[0].Words);
            Assert.Greater(marked[0].Disc.layout.width, Scrubber.Items[(Nav.Current + 1) % Scrubber.Items.Count].Disc.layout.width, "marked by size, not colour alone");
            yield return Capture("Edge_Lamp_rest_en");
        }

        // A finger that lands ABOVE the column (the viewport's room over it) is the card's too: the first dot's own target is the edge of the
        // scrubber, the pixels beyond it are the card's
        [UnityTest]
        public IEnumerator AFingerAboveOrBelowTheColumn_MeetsTheContent_NotTheScrubber()
        {
            yield return UseScrubber();
            yield return Open(SheetStopRule.Stop.Full);
            var viewport = Scroll.contentViewport.worldBound;
            var column = Scrubber.Column.worldBound;
            Assert.Greater(column.yMin - viewport.yMin, 8f, "precondition: the column leaves room above it (" + (column.yMin - viewport.yMin) + ")");
            float x = column.center.x;
            Assert.IsFalse(IsInScrubber(TopmostAt(new Vector2(x, column.yMin - 4f))), "just above the first dot: the content");
            Assert.IsFalse(IsInScrubber(TopmostAt(new Vector2(x, column.yMax + 4f))), "just below the last dot: the content");
            Assert.IsTrue(IsInScrubber(TopmostAt(new Vector2(x, column.yMin + 4f))), "just inside the first dot's target: the scrubber");
            Assert.IsTrue(IsInScrubber(TopmostAt(new Vector2(x, column.yMax - 4f))), "and the last one's");
            int jumps = NavJumps.Count;
            yield return CardTestInput.TouchDrag(Sheet.Layer.panel, new Vector2(x, column.yMax + 20f), new Vector2(0f, -300f), frames: 12);
            yield return CardTestInput.Settle(0.4f);
            Assert.Greater(Scroll.scrollOffset.y, 50f, "a drag that starts below the column scrolls the card");
            Assert.AreEqual(jumps, NavJumps.Count, "and jumps nowhere");
        }

        // ---------------- words, in both languages ----------------

        // The bubble's words are whole in English and in Portuguese (the longest, "Comunidade", on the last dot), read on a dark surface at
        // 4.5:1, wholly on screen at 390 px -- judged WHILE a real finger is down on that dot
        [UnityTest]
        public IEnumerator TheBubblesWords_AreWholeAndReadable_InEnglishAndPortuguese([Values("en", "pt")] string language)
        {
            yield return UseScrubber(language);
            yield return Open(SheetStopRule.Stop.Full);
            var words = Words(language);
            int last = Model.Sections.Count - 1;
            var panel = Sheet.Layer.panel;
            CardTestInput.TouchDown(panel, DotAt(0));
            yield return null;
            yield return WalkTo(DotAt(0), DotAt(last));
            Assert.AreEqual(last, Scrubber.Pointed, "precondition: the finger is on the last dot");
            string expected = words.Get(Model.Sections[last].LabelKey);
            Assert.IsNotEmpty(expected, "precondition: the last family has words in " + language);
            Assert.AreEqual(expected, Scrubber.BubbleLabel.text, "the bubble names the family in " + language);
            var label = Scrubber.BubbleLabel;
            var measured = label.MeasureTextSize(label.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
            Assert.GreaterOrEqual(label.layout.width + 0.5f, measured.x, "'" + label.text + "': the whole word fits, never cut");
            Assert.GreaterOrEqual(Scrubber.Bubble.worldBound.xMin, Sheet.Layer.worldBound.xMin, "the bubble is wholly on screen (left)");
            Assert.LessOrEqual(Scrubber.Bubble.worldBound.xMax, Sheet.Layer.worldBound.xMax, "and right");
            Assert.GreaterOrEqual(Scrubber.Bubble.worldBound.yMin, Sheet.Layer.worldBound.yMin, "and top");
            float contrast = CardTestInput.Contrast(label.resolvedStyle.color, CardTestInput.EffectiveBackground(label));
            Assert.GreaterOrEqual(contrast, UIAccessibility.MinRatioNormalText, "'" + label.text + "' reads at 4.5:1 (" + contrast.ToString("F2") + ")");
            yield return Capture("Edge_Lamp_scrub_last_" + language);
            yield return Lift(DotAt(last));
            yield return Landed();
            AssertLanded(last, language + ": the last family");
        }

        // ---------------- over a block that reaches the right edge ----------------

        // The finding the brief asks for: the dots lie OVER the content's right edge. Measured on The Lamp's gallery blocks: how far each reaches
        // towards the edge and what the column covers of it; a real horizontal swipe on the filmstrip's strip that starts BESIDE the column
        // still moves it, and so does one that starts ON a dot (only a clearly VERTICAL drag is the scrubber's, SwipeGrabRule.ClaimsVertical;
        // a sideways one is the strip's whoever's pixels it began on): the dots never block a swipe, and nothing jumps.
        // Recorded in _3.2 section 5.5 so a later change to the column's width or the card's gutter shows up here
        [UnityTest]
        public IEnumerator TheDots_LieOverTheRightEdgeOfABlock_TheGalleryStripBesideThemStillSwipes_AndOneStartedOnADotDoesNot()
        {
            yield return UseScrubber();
            yield return Open(SheetStopRule.Stop.Full);
            // - the gallery blocks whose horizontal strip overflows (a filmstrip with few small pictures fits whole; the carousel runs past the edge)
            var gallery = Sheet.Stack.BoundViews.OfType<GalleryBlockView>()
                .Where(v => v.Track != null && v.Track.contentContainer.layout.width > v.Track.contentViewport.layout.width + 1f).ToList();
            Assert.IsNotEmpty(gallery, "precondition: The Lamp has a gallery whose strip runs past the viewport");
            var view = gallery[0];
            Scroll.ScrollTo(view.Track);
            yield return CardTestInput.Settle(0.4f);
            yield return Rested();
            var viewport = Scroll.contentViewport.worldBound;
            var strip = view.Track.worldBound;
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            float gutter = viewport.xMax - strip.xMax;
            var column = Scrubber.Column.worldBound;
            float coveredWidth = Mathf.Max(0f, strip.xMax - column.xMin);
            Assert.AreEqual(Mathf.Max(0f, touch - gutter), coveredWidth, OnePixel, "the column's target covers the strip's right end by the part of " + touch + " px the card's own gutter (" + gutter + " px) does not");
            Assert.IsTrue(column.yMin < strip.yMax && column.yMax > strip.yMin, "precondition: the strip is level with the dots here: strip " + strip + " column " + column);
            Assert.Greater(view.Track.contentContainer.layout.width, view.Track.contentViewport.layout.width, "precondition: the strip holds more pictures than it shows");

            // - a swipe that starts on the strip LEFT of the column moves it
            var start = new Vector2(column.xMin - 40f, strip.center.y);
            Assert.IsFalse(IsInScrubber(TopmostAt(start)), "precondition: the swipe starts on the strip itself");
            float before = view.Track.scrollOffset.x;
            yield return CardTestInput.TouchDrag(Sheet.Layer.panel, start, new Vector2(-160f, 0f), frames: 10);
            yield return CardTestInput.Settle(0.3f);
            Assert.Greater(view.Track.scrollOffset.x, before + 40f, "a swipe beside the dots moves the strip (" + before + " -> " + view.Track.scrollOffset.x + ")");

            // - a swipe that starts ON a dot of the column and goes sideways is still the strip's: the scrubber claims only vertical drags
            int jumps = NavJumps.Count;
            float rest = view.Track.scrollOffset.x;
            // - the dot level with the strip: the middle of the part of the strip the column spans (the column is centred on the viewport,
            //   so where it meets the strip moves with the card's layout -- _3.3 P4.4a's taller viewport moved it off the strip's middle)
            var onDot = new Vector2(column.center.x, (Mathf.Max(strip.yMin, column.yMin) + Mathf.Min(strip.yMax, column.yMax)) * 0.5f);
            int dot = Scrubber.Items.ToList().FindIndex(i => i.Button.worldBound.Contains(onDot));
            Assert.GreaterOrEqual(dot, 0, "precondition: a dot's target is level with the strip at " + onDot);
            yield return CardTestInput.TouchDrag(Sheet.Layer.panel, onDot, new Vector2(-160f, 0f), frames: 10);
            yield return CardTestInput.Settle(0.3f);
            Assert.Greater(Mathf.Abs(view.Track.scrollOffset.x - rest), OnePixel, "a sideways swipe that starts on a dot still moves the strip: the dots never block it");
            Assert.AreEqual(jumps, NavJumps.Count, "and it jumps nowhere (it left the dot, so it is no tap)");
            Assert.IsFalse(Scrubber.IsDragging, "the scrubber never claimed a sideways drag");
            yield return Capture("Edge_Lamp_over_filmstrip_en");
        }
    }
}
