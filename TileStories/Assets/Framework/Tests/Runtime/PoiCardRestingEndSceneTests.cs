using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // The card's END where a visitor's drags rest (_3.3 R10.8: Finished = the scroll has reached its end), on the REAL LivingRoomScene with
    // real fingers: The Lamp and The Painting with each bottom element that shows chrome at the end (Next Up, Bottom Bar) and The Lamp's sticky
    // footer alone. At the end the bottom area comes back (CardChromeRule) and takes its height off the stack's viewport.
    // The measuring instrument runs only while TestEvidence/RestingEnd/MEASURING names a phase (LookMeasure): per case the viewport and the
    // range with the area away and back, where small drags and a fling rest, and what the navigation says there, plus a render at rest.
    [Ignore(TestSkips.OldNavigationElements)]
    public class PoiCardRestingEndSceneTests : SearchSceneFixture
    {
        private const string Lamp = "lamp";
        private const string Painting = "painting";

        // poi / bottom element ("none" = the card's own footer alone: only The Lamp has a sticky CTA)
        private static readonly string[] Cases =
        {
            Lamp + "/" + CardOptions.NavNone,
            Lamp + "/" + BuiltInNavElements.NextUp,
            Painting + "/" + BuiltInNavElements.NextUp,
        };

        private PoiCardSheetView Sheet => Card.Sheet;
        private NavController Nav => Sheet.Nav;
        private CardNavModel Model => Nav.Model;
        private ScrollView Scroll => Sheet.Stack.Scroll;
        private CardChromeView Chrome => Sheet.Chrome;
        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;
        private float Viewport => Scroll.contentViewport.layout.height;
        private float Content => Scroll.contentContainer.layout.height;
        private float Range => NavScrollRule.Range(Content, Viewport);

        private IEnumerator Push(System.Action<WallConfigData> edit)
        {
            var copy = ConfigCopy();
            edit(copy);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        // The case's card at Full, read fresh (an in-memory state: the block waiting for the end is revealed by THIS reading)
        private IEnumerator Open(string poi, string element)
        {
            yield return Push(c => c.card_settings.navigation.bottom = element);
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            SelectionEventBus.Select(poi);
            yield return CardTestInput.Settle();
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            Assert.AreEqual(poi, Card.ShownPoiId, "precondition: " + poi + "'s card is shown");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "precondition: at Full");
            Assert.AreEqual(element, Sheet.Slots.BottomKey ?? CardOptions.NavNone, "precondition: the bottom element is " + element);
            Assert.Greater(Range, Viewport * 3f, "precondition: " + poi + "'s card scrolls several screens (range " + Range + ")");
        }

        // One real finger stroke up the screen (further down the card) by `share` of the viewport over `frames` frames, from just above the
        // viewport's bottom edge -- measured before every stroke: the bottom area coming back moves that edge
        private IEnumerator Stroke(float share, int frames)
        {
            var viewport = Scroll.contentViewport.worldBound;
            yield return CardTestInput.TouchDrag(Scroll.panel, new Vector2(viewport.center.x, viewport.yMax - 40f), new Vector2(0f, -viewport.height * share), frames);
            yield return CardTestInput.Settle(0.5f);
        }

        // Real strokes down the card until three in a row move the stack no more: where the visitor's drags REST, whatever the range says.
        // `largestViewport` = the viewport while the bottom area was away
        private IEnumerator StrokeUntilStill(float share, int frames, System.Action<float> largestViewport)
        {
            float largest = Viewport;
            int still = 0;
            for (int stroke = 0; stroke < 200 && still < 3; stroke++)
            {
                float before = Scroll.scrollOffset.y;
                yield return Stroke(share, frames);
                largest = Mathf.Max(largest, Viewport);
                still = Scroll.scrollOffset.y > before + OnePixel ? 0 : still + 1;
            }
            Assert.AreEqual(3, still, "precondition: the strokes came to rest");
            yield return HeldStill();
            largestViewport(largest);
        }

        // With no finger on it the stack can still be moving (the ScrollView's release and elastic animations run for a while, longer in an
        // unfocused Editor's slower frames): its rest is where the offset holds for StillFrames frames
        private IEnumerator HeldStill()
        {
            const int StillFrames = 60;
            float last = Scroll.scrollOffset.y;
            int same = 0;
            for (int frame = 0; frame < 1200 && same < StillFrames; frame++)
            {
                yield return null;
                same = Mathf.Abs(Scroll.scrollOffset.y - last) < 0.01f ? same + 1 : 0;
                last = Scroll.scrollOffset.y;
            }
            Assert.AreEqual(StillFrames, same, "precondition: the stack held still on its own");
        }

        // The section of the card's last target shown in the scroll (cut from the model's targets, never Model.Runs)
        private int LastRunsSection()
        {
            var shown = Model.Targets.Where(t => Sheet.Stack.SlotOfBlock(t) is { } s && Sheet.Stack.IsInScroll(s) && s.resolvedStyle.display == DisplayStyle.Flex).ToList();
            return Model.SectionOf(shown[shown.Count - 1]);
        }

        // What the card says where the drags rest, as one line
        private string AtRest(string element, float hiddenViewport)
        {
            float offset = Scroll.scrollOffset.y;
            var viewport = Scroll.contentViewport.worldBound;
            float contentBottomBelowViewport = Scroll.contentContainer.worldBound.yMax - viewport.yMax;
            string last = Model.Targets[Model.Targets.Count - 1];
            int lastRun = LastRunsSection();
            string bar = element == BuiltInNavElements.NextUp && Sheet.Slots.BottomView is NextUpNavView next
                ? $" | next_up finished {next.Finished} '{next.Caption.text}' {next.Position.text}" : "";
            return $"content {Content:0.0} viewport {Viewport:0.0} (away {hiddenViewport:0.0}, area {hiddenViewport - Viewport:0.0}) range {Range:0.0} " +
                   $"offset {offset:0.0} short by {Range - offset:0.0} | area shown {Chrome.BottomShown} footer {Chrome.FooterShown} bar {Chrome.BottomNavShown} | " +
                   $"end in view {contentBottomBelowViewport <= OnePixel} ({contentBottomBelowViewport:0.0} below) | IsAtEnd {offset >= Range - NavScrollRule.EndTolerance} | " +
                   $"spy {(Nav.Current >= 0 ? Model.Sections[Nav.Current].Key : "-")} (last run {Model.Sections[lastRun].Key}) block {Nav.CurrentBlock} (last {last}) | " +
                   $"last target seen {Nav.HasViewed(last)} | content seen {Sheet.Stack.ContentSeen}{bar}";
        }

        // The end where real strokes rest -- small ones (a reader's) and flings -- on both cards with each bottom element: the stack rests at
        // the END of its range with the bottom area shown, the card's last pixel above the area, and everything that judges the end agrees
        // there: NavScrollRule's end, Next Up's Finished words with no Next, the bar's mark on the last run, the spy, the last target's tick
        // and the card read. Before the fix, small strokes left The Lamp with Next Up 125 px short (the Sources' last lines under the area) and
        // The Painting with Next Up at the end with the area still away (its finished words off screen)
        [UnityTest, Timeout(900000)]
        public IEnumerator RealStrokesToTheEnd_RestAtTheEnd_WithTheCardsLastPixelAboveTheArea(
            [ValueSource(nameof(Cases))] string caseName, [Values("small", "fling")] string stroke)
        {
            string poi = caseName.Split('/')[0], element = caseName.Split('/')[1];
            yield return Open(poi, element);
            float away = 0f;
            yield return stroke == "small" ? StrokeUntilStill(0.2f, 12, v => away = v) : StrokeUntilStill(0.8f, 3, v => away = v);
            string where = caseName + " " + stroke + ": " + AtRest(element, away);

            Assert.Greater(away, Viewport + OnePixel, "precondition: the area was away while reading and took its height back at the end: " + where);
            Assert.IsTrue(Chrome.BottomShown, "the bottom area is back at the end: " + where);
            Assert.AreEqual(Range, Scroll.scrollOffset.y, OnePixel, "the drags rest at the end of the range with the area shown: " + where);
            var viewport = Scroll.contentViewport.worldBound;
            Assert.LessOrEqual(Scroll.contentContainer.worldBound.yMax, viewport.yMax + OnePixel, "the card's last pixel is in the viewport: " + where);
            Assert.LessOrEqual(viewport.yMax, Sheet.BottomArea.worldBound.yMin + OnePixel, "above the area, not under it: " + where);

            int lastRun = LastRunsSection();
            string last = Model.Targets[Model.Targets.Count - 1];
            Assert.AreEqual(lastRun, Nav.Current, "the spy names the last run's section: " + where);
            Assert.AreEqual(last, Nav.CurrentBlock, "the block read is the last target: " + where);
            Assert.IsTrue(Nav.HasViewed(last), "the last target is ticked as seen: " + where);
            Assert.IsTrue(Sheet.Stack.ContentSeen, "the card is read: " + where);
            if (Sheet.Slots.BottomView is NextUpNavView next)
            {
                var words = new CardStrings(Card.StringTable.Entries(), CardStringSources.Shared.Entries(), null, "en", "en");
                Assert.IsTrue(next.Finished, "Next Up says Finished: " + where);
                Assert.AreEqual(words.Get(CardStrings.Keys.NavNextUpDone), next.Caption.text, "with its finished words");
                Assert.IsFalse(CardTestInput.IsShown(next.Next, Sheet.Root), "and no Next that goes nowhere");
                Assert.IsTrue(CardTestInput.IsShown(next.Caption, Sheet.Root), "the words on screen");
            }
            if (Sheet.Slots.BottomView is BottomBarNavView bar)
                Assert.AreEqual(lastRun, bar.CurrentIndex, "the bar marks the last run's section: " + where);
        }

        // Within the area's own height of the end it never hides: a real drag back up a little and down again from the end keeps the area on
        // screen at every scroll step and rests at the end again -- before, the move down hid it, the grown viewport clamped the stack back
        // and the area came again (the loop that held small drags 125 px short on The Lamp with Next Up)
        [UnityTest, Timeout(900000)]
        public IEnumerator AtTheEnd_ADragUpAndDownWithinTheArea_NeverHidesIt_AndRestsAtTheEndAgain()
        {
            yield return Open(Lamp, BuiltInNavElements.NextUp);
            yield return StrokeUntilStill(0.8f, 3, _ => { });
            Assert.AreEqual(Range, Scroll.scrollOffset.y, OnePixel, "precondition: at the end");
            float area = Sheet.BottomArea.layout.height;
            Assert.IsTrue(Chrome.BottomShown && area > 100f, "precondition: the footer and Next Up are back (" + area + " px)");
            bool hid = false;
            void Watch(float _) => hid |= !Chrome.BottomShown;
            Scroll.verticalScroller.valueChanged += Watch;
            try
            {
                var viewport = Scroll.contentViewport.worldBound;
                // - a finger moves the stack more than its own travel (~1.7x, 4.2.3): a third of the area up is well inside it
                yield return CardTestInput.TouchDrag(Scroll.panel, new Vector2(viewport.center.x, viewport.center.y), new Vector2(0f, area / 3f));
                yield return CardTestInput.Settle(0.5f);
                Assert.Less(Scroll.scrollOffset.y, Range - OnePixel, "precondition: the drag up moved the stack off the end");
                Assert.Greater(Scroll.scrollOffset.y, Range - area, "precondition: still within the area's height of the end");
                yield return CardTestInput.TouchDrag(Scroll.panel, new Vector2(viewport.center.x, viewport.center.y), new Vector2(0f, -area / 2f));
                yield return CardTestInput.Settle(0.5f);
            }
            finally { Scroll.verticalScroller.valueChanged -= Watch; }
            Assert.IsFalse(hid, "the area stayed on screen at every scroll step");
            Assert.IsTrue(Chrome.BottomShown);
            Assert.AreEqual(Range, Scroll.scrollOffset.y, OnePixel, "back at the end");
        }

        [UnityTest, Timeout(1800000)]
        public IEnumerator TheEnd_Measured([ValueSource(nameof(Cases))] string caseName)
        {
            string phase = LookMeasure.RequirePhase(LookMeasure.RestingEnd);
            string poi = caseName.Split('/')[0], element = caseName.Split('/')[1];
            string name = poi + "_" + element;

            yield return Open(poi, element);
            LookMeasure.FreezeHero(Sheet);
            LookMeasure.Note(LookMeasure.RestingEnd, phase, $"{name} at the top: content {Content:0.0} viewport {Viewport:0.0} range {Range:0.0} area shown {Chrome.BottomShown}");
            float hidden = 0f;
            // - small strokes (a fifth of the screen, slow): a reader's drags
            yield return StrokeUntilStill(0.2f, 12, v => hidden = v);
            LookMeasure.Note(LookMeasure.RestingEnd, phase, $"{name} small strokes rest: " + AtRest(element, hidden));
            yield return LookMeasure.Render(LookMeasure.RestingEnd, phase, name + "_end", Sheet.Root);
            LookMeasure.Note(LookMeasure.RestingEnd, phase, $"{name} after the render: offset {Scroll.scrollOffset.y:0.0} range {Range:0.0} viewport {Viewport:0.0} area shown {Chrome.BottomShown}");

            // - flings (most of the screen, fast) from the top again
            yield return Open(poi, element);
            LookMeasure.FreezeHero(Sheet);
            yield return StrokeUntilStill(0.8f, 3, v => hidden = v);
            LookMeasure.Note(LookMeasure.RestingEnd, phase, $"{name} flings rest: " + AtRest(element, hidden));
        }
    }
}
