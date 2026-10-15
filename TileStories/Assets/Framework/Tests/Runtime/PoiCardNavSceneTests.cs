using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // The card's NavController (_3.2 step 3) on the REAL LivingRoomScene with The Lamp's real card: a jump raises the sheet and lands
    // the target block's slot top at the viewport's top (measured against the viewport itself, never a pixel constant), a late jump is
    // clamped to the range, Jump Opens Card To / Jump Motion / reduce motion do what they say, the spy is frozen while a jump glides
    // and follows a real finger drag afterwards, the model is rebuilt on a live edit (scroll kept), a reveal and a point switch, a block
    // asking the host jumps through the same controller, and every jump raises one nav_jump event.
    public partial class PoiCardNavSceneTests : SearchSceneFixture
    {
        // The key a test jump reports as its element (no navigation element exists yet: step 5 adds the first)
        private const string Element = "scene_test_element";
        private const string Lamp = "lamp";

        private PoiCardSheetView Sheet => Card.Sheet;
        private NavController Nav => Sheet.Nav;
        private ScrollView Scroll => Sheet.Stack.Scroll;

        private sealed class RecordingCardEvents : ICardEvents
        {
            public readonly List<CardEvent> Raised = new();
            public void Raise(CardEvent cardEvent) => Raised.Add(cardEvent);
        }

        private RecordingCardEvents _events;

        // An in-memory card state (a run never touches the developer's saved answers) and a sink that keeps the events
        [UnitySetUp]
        public IEnumerator UseAnInMemoryCardStateAndRecordTheEvents()
        {
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            _events = new RecordingCardEvents();
            Card.Events = _events;
            yield break;
        }

        // ---------------- helpers ----------------

        // One screen pixel in panel units: the run's pinned frame decides it, so it is measured, not assumed
        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;

        private float Range => NavScrollRule.Range(Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height);

        private VisualElement Slot(string blockKey) => Sheet.Stack.SlotOfBlock(blockKey);

        // Where a slot's top sits in the viewport (0 = at its top), from the two elements' own screen rects
        private float TopInViewport(string blockKey) => Slot(blockKey).worldBound.yMin - Scroll.contentViewport.worldBound.yMin;

        private List<CardEvent> NavJumps => _events.Raised.Where(e => e.Kind == CardEventKinds.NavJump).ToList();

        private IEnumerator Open(string poiId, SheetStopRule.Stop stop)
        {
            SelectionEventBus.Select(poiId);
            yield return CardTestInput.Settle();
            Sheet.SetStop(stop);
            yield return CardTestInput.Settle();
            Assert.AreEqual(stop, Sheet.Stop, "precondition: the card rests at " + stop);
            Assert.IsNotNull(Nav.Model, "precondition: the sheet built the card's navigation model");
        }

        private IEnumerator Landed()
        {
            for (int frame = 0; frame < 600 && Nav.IsJumping; frame++) yield return null;
            Assert.IsFalse(Nav.IsJumping, "the jump came to rest");
            yield return CardTestInput.Settle(0.3f);
        }

        // Push an edit the way the Editor's live applier does: a private copy of the running config, through the wall's own seam
        private IEnumerator Push(System.Action<WallConfigData> edit)
        {
            var copy = ConfigCopy();
            edit(copy);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        // The last section whose first block can land at the viewport's top without the clamp, read at the current stop's geometry
        private int LatestUnclampedSection()
        {
            for (int i = Nav.Model.Sections.Count - 1; i >= 0; i--)
                if (Slot(Nav.Model.Sections[i].Targets[0]).layout.y < Range - OnePixel) return i;
            return -1;
        }

        // The section the reader is in, read from the screen (each section's first slot against a third of the viewport), never from
        // the controller's own arithmetic
        private int SectionUnderTheSpyLine()
        {
            var viewport = Scroll.contentViewport.worldBound;
            float line = viewport.yMin + viewport.height * NavScrollRule.SpyLineFraction;
            int found = 0;
            for (int i = 0; i < Nav.Model.Sections.Count; i++)
                if (Slot(Nav.Model.Sections[i].Targets[0]).worldBound.yMin <= line) found = i;
            return found;
        }

        // --ts-duration as CardTokens.uss writes it (seconds): the one place the card's motion length lives
        private static float DurationToken()
        {
            var match = Regex.Match(System.IO.File.ReadAllText("Assets/Framework/Runtime/UI/Cards/CardTokens.uss"), @"--ts-duration:\s*([0-9.]+)s\s*;");
            Assert.IsTrue(match.Success, "--ts-duration is defined in CardTokens.uss");
            return float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
        }

        // ---------------- jumps ----------------

        [UnityTest]
        public IEnumerator AJumpFromPeek_RaisesTheCardToFullFirst_ThenLandsALateFamilysFirstBlockAtTheViewportTop_AndReportsOneNavJump()
        {
            // - the section is chosen at Full's geometry (where the jump will land), then the card goes back to peek
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            Assert.GreaterOrEqual(Nav.Model.Sections.Count, 3, "precondition: The Lamp's card has several sections");
            int index = LatestUnclampedSection();
            Assert.Greater(index, 1, "precondition: a LATE section lands without the clamp (" + index + " of " + Nav.Model.Sections.Count + ")");
            var section = Nav.Model.Sections[index];
            Sheet.SetStop(SheetStopRule.Stop.Peek);
            yield return CardTestInput.Settle();
            Assert.AreEqual(0f, Scroll.scrollOffset.y, OnePixel, "precondition: the stack starts at its top");

            Assert.IsTrue(Nav.JumpTo(index, Element));
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "Jump Opens Card To = full (the default): the sheet is raised BEFORE the stack moves");
            yield return Landed();

            Assert.AreEqual(0f, TopInViewport(section.Targets[0]), OnePixel,
                section.Key + ": its first block's slot top meets the viewport's top (offset " + Scroll.scrollOffset.y + ", range " + Range + ")");
            Assert.AreEqual(index, Nav.Current, "the spy names the section jumped to");
            Assert.AreEqual(section.Key, Nav.CurrentSection.Key);
            var jump = NavJumps.Single();
            Assert.AreEqual(section.Targets[0], jump.BlockKey, "nav_jump: the block landed on");
            Assert.AreEqual(Element, jump.Variant, "nav_jump: the element that asked");
            Assert.AreEqual(section.Key, jump.Value, "nav_jump: the section");
            Assert.AreEqual(Lamp, jump.PoiId);
            Assert.AreEqual(Session.SearchConfig.wall_id, jump.WallId);
        }

        [UnityTest]
        public IEnumerator AJumpToABlockInTheFinalScreen_IsClampedToTheRange_AndTheSpyNamesTheLastSection()
        {
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            // - the card's last navigation target (sections come in the order of their FIRST target, so it may belong to any section)
            string block = Nav.Model.Sections.SelectMany(s => s.Targets).OrderBy(k => Slot(k).layout.y).Last();
            Assert.Greater(Slot(block).layout.y, Range + OnePixel, "precondition: the block sits in the final screen (its top cannot reach the viewport's)");

            Assert.IsTrue(Nav.JumpToBlock(block, Element));
            yield return Landed();
            Assert.AreEqual(Range, Scroll.scrollOffset.y, OnePixel, "clamped: the stack rests at the end of its range");
            Assert.Greater(TopInViewport(block), 0f, "so the block's top sits lower in the viewport");
            Assert.AreEqual(Nav.Model.Sections.Count - 1, Nav.Current, "at the end of the range the LAST section is named");
        }

        [UnityTest]
        public IEnumerator KeepAndInstant_TheCardStaysAtItsStop_AndTheStackIsSetAtOnce()
        {
            yield return Push(c =>
            {
                c.card_settings.navigation.jump_raises_to = CardOptions.JumpRaisesKeep;
                c.card_settings.navigation.jump_motion = CardOptions.JumpMotionInstant;
            });
            yield return Open(Lamp, SheetStopRule.Stop.Half);
            int index = LatestUnclampedSection();
            Assert.Greater(index, 0, "precondition: a section lands unclamped at Half");
            string block = Nav.Model.Sections[index].Targets[0];

            Assert.IsTrue(Nav.JumpTo(index, Element));
            Assert.AreEqual(Slot(block).layout.y, Scroll.scrollOffset.y, OnePixel, "instant: the offset is set at once, no frame in between");
            yield return Landed();
            Assert.AreEqual(SheetStopRule.Stop.Half, Sheet.Stop, "Jump Opens Card To = keep: the card stays at Half");
            Assert.AreEqual(0f, TopInViewport(block), OnePixel, "and the block's top meets the viewport's top at Half");
        }

        [UnityTest]
        public IEnumerator ReduceMotion_SetsASmoothJumpAtOnce()
        {
            yield return Push(c => c.card_settings.container.reduce_motion = true);
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            Assert.AreEqual(CardOptions.JumpMotionSmooth, ConfigCopy().card_settings.navigation.jump_motion, "precondition: Jump Motion is smooth");
            int index = LatestUnclampedSection();

            Assert.IsTrue(Nav.JumpTo(index, Element));
            Assert.AreEqual(Slot(Nav.Model.Sections[index].Targets[0]).layout.y, Scroll.scrollOffset.y, OnePixel, "reduce motion: set at once, no glide");
            yield return Landed();
        }

        // ---------------- the spy ----------------

        [UnityTest]
        public IEnumerator WhileASmoothJumpGlides_TheSpyStaysOnItsSection_ThenIsReleasedToTheScroll()
        {
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            Assert.AreEqual(DurationToken(), Nav.Duration, 1e-4f, "a jump glides over the card's --ts-duration token");
            int target = Nav.Model.Sections.Count - 2;
            Assert.Less(Slot(Nav.Model.Sections[target].Targets[0]).layout.y, Range - OnePixel, "precondition: the target lands unclamped");
            Assert.AreEqual(0, Nav.Current, "precondition: the card starts in its first section");
            var changes = new List<int>();
            Nav.CurrentChanged += changes.Add;
            float now = 0f;
            Nav.Clock = () => now;
            try
            {
                Assert.IsTrue(Nav.JumpTo(target, Element));
                now = Nav.Duration * 0.5f;
                for (int frame = 0; frame < 5; frame++) yield return null;
                Assert.IsTrue(Nav.IsJumping, "half its time in, the jump still glides");
                Assert.That(Scroll.scrollOffset.y, Is.GreaterThan(OnePixel).And.LessThan(Slot(Nav.Model.Sections[target].Targets[0]).layout.y - OnePixel),
                    "precondition: the stack is part way there");
                int passing = SectionUnderTheSpyLine();
                Assert.AreNotEqual(target, passing, "precondition: the glide is passing ANOTHER section (" + passing + ")");
                Assert.AreEqual(target, Nav.Current, "frozen: the spy names the jump's section, not the one passing by");

                // - past the glide AND the settle window (both one motion length): the jump must be over
                now = Nav.Duration * 3f;
                yield return Landed();
                CollectionAssert.AreEqual(new[] { target }, changes, "one change for the whole jump: no flicker through the sections it passed");
            }
            finally
            {
                Nav.Clock = () => Time.unscaledTime;
                Nav.CurrentChanged -= changes.Add;
            }

            // - released: a real finger drag back up moves the spy again
            var viewport = Scroll.contentViewport.worldBound;
            yield return CardTestInput.TouchDrag(Scroll.panel, viewport.center, new Vector2(0f, viewport.height * 0.6f));
            yield return CardTestInput.Settle(0.6f);
            Assert.AreEqual(SectionUnderTheSpyLine(), Nav.Current, "released: the spy follows the scroll again");
            Assert.AreNotEqual(target, Nav.Current, "precondition met: the drag really left the section");
        }

        [UnityTest]
        public IEnumerator ARealFingerDrag_MovesTheSpy_ToTheSectionOnScreen_AtThreeOffsets()
        {
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            var viewport = Scroll.contentViewport.worldBound;
            var named = new List<int>();

            // 1. one short drag from the top
            yield return CardTestInput.TouchDrag(Scroll.panel, viewport.center, new Vector2(0f, -viewport.height * 0.3f));
            yield return CardTestInput.Settle(0.6f);
            Assert.Greater(Scroll.scrollOffset.y, OnePixel, "precondition: a finger drag really scrolls the stack");
            Assert.AreEqual(SectionUnderTheSpyLine(), Nav.Current, "offset " + Scroll.scrollOffset.y + ": the section on screen");
            named.Add(Nav.Current);

            // 2. drags until the reader is past the middle of the card
            for (int drag = 0; drag < 60 && Scroll.scrollOffset.y < Range * 0.5f; drag++)
            {
                yield return CardTestInput.TouchDrag(Scroll.panel, viewport.center, new Vector2(0f, -viewport.height * 0.6f));
                yield return CardTestInput.Settle(0.3f);
            }
            yield return CardTestInput.Settle(0.6f);
            Assert.GreaterOrEqual(Scroll.scrollOffset.y, Range * 0.5f, "precondition: the drags reached the middle");
            Assert.Less(Scroll.scrollOffset.y, Range - NavScrollRule.EndTolerance, "precondition: not yet at the end");
            Assert.AreEqual(SectionUnderTheSpyLine(), Nav.Current, "offset " + Scroll.scrollOffset.y + ": the section on screen");
            named.Add(Nav.Current);

            // 3. drags to the very end
            for (int drag = 0; drag < 80 && Scroll.scrollOffset.y < Range - NavScrollRule.EndTolerance; drag++)
            {
                yield return CardTestInput.TouchDrag(Scroll.panel, viewport.center, new Vector2(0f, -viewport.height * 0.6f));
                yield return CardTestInput.Settle(0.3f);
            }
            yield return CardTestInput.Settle(0.6f);
            Assert.AreEqual(Range, Scroll.scrollOffset.y, NavScrollRule.EndTolerance, "precondition: the drags reached the end");
            Assert.AreEqual(Nav.Model.Sections.Count - 1, Nav.Current, "at the end: the last section");
            named.Add(Nav.Current);

            Assert.AreEqual(3, named.Distinct().Count(), "three offsets, three different sections named (" + string.Join(", ", named) + ")");
        }

        // ---------------- rebuilt with the card ----------------

        [UnityTest]
        public IEnumerator ALiveEdit_RebuildsTheModel_AndKeepsTheScrollAndTheSection()
        {
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            int index = LatestUnclampedSection();
            Assert.IsTrue(Nav.JumpTo(index, Element));
            yield return Landed();
            var before = Nav.Model;
            float scrolled = Scroll.scrollOffset.y;
            string section = Nav.CurrentSection.Key;
            int arBefore = before.Sections.Single(s => s.Key == BuiltInFamilies.Ar).Count;

            yield return Push(c =>
            {
                var blocks = c.pois.Single(p => p.id == Lamp).card.blocks;
                blocks.Insert(blocks.FindIndex(b => b.kind == "sources"), new BlockInstanceData { key = "block_live_nav", kind = "show_on_wall" });
            });

            Assert.AreNotSame(before, Nav.Model, "the live edit rebuilt the model");
            Assert.AreEqual(arBefore + 1, Nav.Model.Sections.Single(s => s.Key == BuiltInFamilies.Ar).Count, "the new block is a target of its family");
            Assert.IsTrue(Nav.Model.Sections.SelectMany(s => s.Targets).Contains("block_live_nav"));
            Assert.AreEqual(scrolled, Scroll.scrollOffset.y, 2f, "the stack kept its scroll");
            Assert.AreEqual(section, Nav.CurrentSection.Key, "and the spy the reader's section");
            Assert.AreEqual(SectionUnderTheSpyLine(), Nav.Current, "which is the one on screen");
        }

        [UnityTest]
        public IEnumerator TheRevealOfAWaitingBlock_RebuildsTheModel_WithItAsATarget()
        {
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            var before = Nav.Model;
            Assert.IsFalse(before.Sections.SelectMany(s => s.Targets).Contains("block_44"), "precondition: the Show After Reading block waits");
            int play = before.Sections.Single(s => s.Key == BuiltInFamilies.Play).Count;

            for (int notch = 0; notch < 400 && !Sheet.Stack.ContentSeen; notch++) yield return CardTestInput.Wheel(Scroll, 12f);
            Assert.IsTrue(Sheet.Stack.ContentSeen, "precondition: the visitor read to the end");
            yield return null;

            Assert.AreNotSame(before, Nav.Model, "the reveal rebuilt the model");
            Assert.IsTrue(Nav.Model.Sections.SelectMany(s => s.Targets).Contains("block_44"), "the revealed block is a target now");
            Assert.AreEqual(play + 1, Nav.Model.Sections.Single(s => s.Key == BuiltInFamilies.Play).Count);
            Assert.IsTrue(Nav.JumpToBlock("block_44", Element), "and a jump can land on it");
            yield return Landed();
            Assert.AreEqual(BuiltInFamilies.Play, NavJumps.Last().Value);
        }

        [UnityTest]
        public IEnumerator ASwitchToAnotherPoint_BindsItsOwnModel_AndCloseUnbindsIt()
        {
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            Assert.IsTrue(Nav.Model.ShowsNavigation, "precondition: The Lamp's long card shows navigation");
            SelectionEventBus.Select("lamp_military");
            yield return CardTestInput.Settle();
            Assert.AreEqual(4, Nav.Model.TargetCount, "Lamp - Military's own model: its 4 targets (_3.2 4.4)");
            Assert.IsFalse(Nav.Model.ShowsNavigation);
            Assert.AreEqual(0, Nav.Current, "a new point starts in its first section");

            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            Assert.IsNull(Nav.Model, "closed: nothing to drive");
            Assert.AreEqual(-1, Nav.Current);
            Assert.IsFalse(Nav.JumpTo(0, Element), "a closed card jumps nowhere");
        }

        // ---------------- a block asking the host ----------------

        [UnityTest]
        public IEnumerator ABlockAsksTheHost_ToScrollToABlock_ThroughTheSameController_ButNeverToAHiddenOne()
        {
            yield return Open(Lamp, SheetStopRule.Stop.Half);
            IBlockHost host = Sheet;
            string sources = ConfigCopy().pois.Single(p => p.id == Lamp).card.blocks.First(b => b.kind == "sources").key;
            Assert.IsFalse(Nav.Model.Sections.SelectMany(s => s.Targets).Contains(sources), "precondition: Sources is no navigation target (meta)");

            host.ScrollToBlock("block_44", "actions");
            yield return null;
            Assert.IsFalse(Nav.IsJumping, "a block waiting to be revealed is never jumped to");
            CollectionAssert.IsEmpty(NavJumps, "and reports nothing");

            host.ScrollToBlock(sources, "actions");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "the host's jump raises the card like any other");
            yield return Landed();
            float expected = Mathf.Min(Slot(sources).layout.y, Range);
            Assert.AreEqual(expected, Scroll.scrollOffset.y, OnePixel, "it lands on the block (clamped to the range)");
            var jump = NavJumps.Single();
            Assert.AreEqual(sources, jump.BlockKey);
            Assert.AreEqual("actions", jump.Variant, "the asking block's kind is the element");
            Assert.AreEqual("", jump.Value, "a block of no section reports none");
        }
    }
}
