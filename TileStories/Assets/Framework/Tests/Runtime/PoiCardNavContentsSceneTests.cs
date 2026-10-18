using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace TileStories.Tests
{
    // Phase B of step 11b (_3.2 section 5.6 D2, nav_contents; section 3, Reading Time and Auto Nav Block) on the REAL LivingRoomScene with
    // real touches:
    //   - Auto Nav Block = Contents, pushed through the live seam: The Lamp's card opens with the list as the first block of its scroll, a
    //     row per target (56) under its family; Lamp - Military (under Show Navigation From) gets none; None takes it away, live
    //   - minutes: the Reading Time The Lamp's fixture gives four blocks, the clip length of its audio guide and its two videos, nothing
    //     for a block with neither
    //   - a real finger drag past three blocks ticks EXACTLY those three (the spy line passed their tops); the ticks are still there after
    //     the card is closed and reopened; the reset the POI Editor's Reset Saved Card State calls (CardLocalState.ResetAll -- the Editor
    //     row itself is proven by its real-click EditMode test) clears them, and a block never seen has no tick
    //   - a real tap on a row lands its block (+-1 px, relative) with one nav_jump nav_contents, and ticks only that block
    //   - nothing is ticked at the peek stop (no part of the stack is visible there)
    //   - renders of the real list at 390 px in English and Portuguese, some rows ticked and some not
    // The card's state is an in-memory CardLocalState for the wall's own id (the host keeps it: same wall), reused across close and reopen
    // as the app's PlayerPrefs one is -- a test must never write the developer's own saved state (PlayerPrefs round trips, the colon key
    // included, are CardLocalStateTests' on a throwaway wall)
    public class PoiCardNavContentsSceneTests : SearchSceneFixture
    {
        private const string Lamp = "lamp";
        private const string LampMilitary = "lamp_military";
        private static readonly string AutoKey = BlockStackBuilder.AutoNavBlockKey(BuiltInBlocks.NavContentsKind);

        private PoiCardSheetView Sheet => Card.Sheet;
        private NavController Nav => Sheet.Nav;
        private CardNavModel Model => Nav.Model;
        private ScrollView Scroll => Sheet.Stack.Scroll;

        private sealed class RecordingCardEvents : ICardEvents
        {
            public readonly List<CardEvent> Raised = new();
            public void Raise(CardEvent cardEvent) => Raised.Add(cardEvent);
        }

        private RecordingCardEvents _events;
        private CardLocalState _state;
        private List<string> _shippedLanguages;

        [UnitySetUp]
        public IEnumerator UseAnInMemoryCardStateAndRecordTheEvents()
        {
            _state = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            Card.State = _state;
            _events = new RecordingCardEvents();
            Card.Events = _events;
            yield break;
        }

        // ---------------- helpers ----------------

        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;

        private float Range => NavScrollRule.Range(Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height);

        private float Viewport => Scroll.contentViewport.layout.height;

        private NavContentsBlockView View => Sheet.Stack.BoundViews.OfType<NavContentsBlockView>().SingleOrDefault();

        private List<string> Ticked => View.Rows.Where(r => r.IsRead).Select(r => r.BlockKey).ToList();

        private float TopOf(string block) => Sheet.Stack.SlotOfBlock(block).layout.y;

        // The wall's navigation for a test: Auto Nav Block `auto`, the card in `language`, pushed through the live seam
        private IEnumerator UseAuto(string auto, string language = "en")
        {
            var copy = ConfigCopy();
            copy.card_settings.navigation.auto_nav_block = auto;
            _shippedLanguages ??= new List<string>(ConfigCopy().card_settings.languages);
            copy.card_settings.languages = language == "en" ? new List<string>(_shippedLanguages) : new List<string> { language, "en" };
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        private IEnumerator Open(string poi, SheetStopRule.Stop stop)
        {
            if (SelectionEventBus.CurrentPoiId != poi)
            {
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
                SelectionEventBus.Select(poi);
                yield return CardTestInput.Settle();
            }
            Scroll.scrollOffset = Vector2.zero;
            Sheet.SetStop(stop);
            yield return CardTestInput.Settle();
            Assert.AreEqual(stop, Sheet.Stop, "precondition: the card rests at " + stop);
            yield return Rested();
        }

        // Close the card the way the visitor does (the selection clears) and open it again on the same point
        private IEnumerator CloseAndReopen(string poi, SheetStopRule.Stop stop)
        {
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            Assert.IsNull(SelectionEventBus.CurrentPoiId, "precondition: the card is closed");
            yield return Open(poi, stop);
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

        // A block's top lands at the viewport's top (relative, one pixel), or the card's end clamps it
        private void AssertLanded(string block, string name)
        {
            var slot = Sheet.Stack.SlotOfBlock(block);
            if (slot.layout.y <= Range - OnePixel)
                Assert.AreEqual(0f, slot.worldBound.yMin - Scroll.contentViewport.worldBound.yMin, OnePixel, name + ": its top meets the viewport's top");
            else
                Assert.AreEqual(Range, Scroll.scrollOffset.y, OnePixel, name + ": a late block clamps at the end of the range");
        }

        // A real finger moving the content UP by `distance` slowly, then HELD still before it lifts -- no fling, so the stack rests where
        // the finger left it. A distance longer than most of the viewport goes in several strokes
        private IEnumerator SlowDragUp(float distance)
        {
            var viewport = Scroll.contentViewport.worldBound;
            float stroke = viewport.height * 0.7f;
            while (distance > 0.5f)
            {
                float move = Mathf.Min(stroke, distance);
                var start = new Vector2(viewport.center.x, viewport.yMin + viewport.height * 0.85f);
                CardTestInput.TouchDown(Scroll.panel, start);
                yield return null;
                const int frames = 30;
                for (int i = 1; i <= frames; i++)
                {
                    CardTestInput.TouchMove(Scroll.panel, start + new Vector2(0f, -move * i / frames));
                    yield return null;
                }
                // - held still a moment before lifting, then the stack is left to rest (whatever it carries on, the caller reads where it
                //   ended: strokes are short)
                for (int i = 0; i < 20; i++)
                {
                    CardTestInput.TouchMove(Scroll.panel, start + new Vector2(0f, -move));
                    yield return null;
                }
                CardTestInput.TouchUp(Scroll.panel, start + new Vector2(0f, -move));
                yield return CardTestInput.Settle(0.5f);
                distance -= move;
            }
        }

        // A real tap by a touch pointer on a point of the card layer
        private IEnumerator TouchTap(Vector2 at)
        {
            yield return CardTestInput.TouchDrag(Sheet.Layer.panel, at, Vector2.zero, frames: 1);
            yield return CardTestInput.Settle();
        }

        // ---------------- the tests ----------------

        [UnityTest]
        public IEnumerator AutoNavBlockContents_PutsTheListFirstOnTheLampsScroll_ARowPerTarget_NotOnLampMilitary_AndNoneTakesItAway_Live()
        {
            yield return UseAuto(CardOptions.NavNone);
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            Assert.IsNull(Sheet.Stack.SlotOfBlock(AutoKey), "the shipped default (None): no contents");

            yield return UseAuto(BuiltInBlocks.NavContentsKind);
            var slot = Sheet.Stack.SlotOfBlock(AutoKey);
            Assert.IsNotNull(slot, "Contents, live: The Lamp's open card gains the list");
            int at = Scroll.contentContainer.IndexOf(slot);
            Assert.AreEqual(Scroll.contentContainer.IndexOf(Sheet.Stack.SlotOfBlock(Model.Targets[0])), at + 1, "right before the card's first block");
            Assert.AreEqual(56, Model.TargetCount, "precondition: The Lamp's 56 targets");
            // - grouped by family: The Lamp's authored order mixes families (15 runs for 9 families), so a family's rows gather under its title,
            //   each family in the order its first block comes, its rows in the card's order
            CollectionAssert.AreEqual(Model.Sections.SelectMany(s => s.Targets).ToList(), View.Rows.Select(r => r.BlockKey).ToList(),
                "a row per target, under its family, in the card's order");
            CollectionAssert.AreEquivalent(Model.Targets, View.Rows.Select(r => r.BlockKey).ToList(), "every target once");
            Assert.AreEqual(Model.Sections.Count, View.Sections.Count, "a title per family");
            Assert.IsEmpty(Ticked, "nothing read yet: no tick");

            yield return Open(LampMilitary, SheetStopRule.Stop.Full);
            Assert.Less(Model.TargetCount, NavOntologyRule.MinBlocks(ConfigCopy().card_settings.navigation), "precondition: under Show Navigation From");
            Assert.IsNull(Sheet.Stack.SlotOfBlock(AutoKey), "Lamp - Military gets no contents");

            yield return Open(Lamp, SheetStopRule.Stop.Full);
            yield return UseAuto(CardOptions.NavNone);
            Assert.IsNull(Sheet.Stack.SlotOfBlock(AutoKey), "None, live: gone again");
        }

        [UnityTest]
        public IEnumerator TheRowsShow_TheReadingTimeTypedForABlock_TheClipLengthOfAudioAndVideo_AndNothingForABlockWithNeither()
        {
            yield return UseAuto(BuiltInBlocks.NavContentsKind);
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            var words = new CardStrings(Card.StringTable.Entries(), CardStringSources.Shared.Entries(), null, "en", "en");
            string Minutes(int n) => string.Format(CultureInfo.InvariantCulture, words.Get(CardStrings.Keys.NavContentsMinutes), n);
            NavContentsBlockView.Row Row(string block) => View.Rows.Single(r => r.BlockKey == block);
            var lamp = ConfigCopy().pois.Single(p => p.id == Lamp);
            // - typed: the fixture's own numbers, read from the config, never re-typed
            foreach (string block in new[] { "block_2", "block_5", "block_22", "block_26" })
            {
                var stored = lamp.card.blocks.Single(b => b.key == block).fields.Single(f => f.key == BlockKindDefinition.MinutesField);
                Assert.Greater(stored.number, 0f, "precondition: the fixture gives " + block + " a Reading Time");
                Assert.AreEqual(Minutes(Mathf.RoundToInt(stored.number)), Row(block).Minutes.text, block + ": its typed Reading Time");
                Assert.AreEqual(DisplayStyle.Flex, Row(block).Minutes.resolvedStyle.display);
            }
            // - clips: the real clip's length, rounded up
            foreach (string block in new[] { "block_55", "block_56", "block_57" })
            {
                Assert.IsTrue(Nav.TryGetEntry(block, out var entry), block + " is shown");
                var field = ReadingTimeRule.ClipField(entry.Definition);
                Assert.IsNotNull(field, "precondition: " + block + " reads a clip");
                string path = new BlockFieldReader(entry.Instance, "en", "en").ValidAsset(field.Key, field.Media);
                float seconds = field.Media == MediaKind.Audio ? Card.Media.Load<AudioClip>(path)?.length ?? 0f : (float)(Card.Media.Load<VideoClip>(path)?.length ?? 0.0);
                Card.Media.Release(path);
                Assert.Greater(seconds, 0f, "precondition: " + block + "'s clip loads");
                Assert.AreEqual(Minutes(ReadingTimeRule.FromClip(seconds)), Row(block).Minutes.text, block + ": its clip's length (" + seconds.ToString("F1") + " s)");
            }
            Assert.AreEqual(DisplayStyle.None, Row("block_13").Minutes.resolvedStyle.display, "a block with no Reading Time and no clip shows no time");
            Assert.AreEqual("", Row("block_13").Minutes.text);
        }

        [UnityTest]
        public IEnumerator ARealDragPastThreeBlocks_TicksExactlyThose_TheyStayAfterCloseAndReopen_AndTheResetClearsThem()
        {
            yield return UseAuto(BuiltInBlocks.NavContentsKind);
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            var targets = Model.Targets;
            // - setup: scroll a little first, inside the (long) list, so the header collapses and the viewport -- and with it the spy line,
            //   a third of it -- stops growing; then the line just above the first target's top (nothing of the card's own blocks read yet)
            Scroll.scrollOffset = new Vector2(0f, 200f);
            yield return CardTestInput.Settle(0.5f);
            Assert.IsTrue(Sheet.Stack.HeaderCollapsed, "precondition: the header collapsed, the viewport is at its reading size");
            Assert.IsEmpty(Ticked, "precondition: the list's top shows no block of the card yet");
            float spy = Viewport * NavScrollRule.SpyLineFraction;
            Scroll.scrollOffset = new Vector2(0f, TopOf(targets[0]) - spy - 10f);
            yield return CardTestInput.Settle(0.3f);
            Assert.AreEqual(spy, Viewport * NavScrollRule.SpyLineFraction, OnePixel, "precondition: the viewport did not change size");
            Assert.IsEmpty(Ticked, "precondition: no tick before the drag");

            // - a real finger moves the content in short strokes, like a reader, until the line has passed the third target's top: each stroke
            //   a quarter of the gap to the fourth, so even the ScrollView's own touch gain (a stroke moved the stack about 1.7 times its length)
            //   cannot carry the line past the fourth
            float goal = TopOf(targets[2]) - spy;
            float before = Scroll.scrollOffset.y;
            float stroke = (TopOf(targets[3]) - TopOf(targets[2])) * 0.25f;
            Assert.Greater(stroke, 10f, "precondition: the third and fourth targets are far enough apart to stop between them");
            for (int i = 0; i < 80 && Scroll.scrollOffset.y + spy <= TopOf(targets[2]); i++)
                yield return SlowDragUp(stroke);
            float line = Scroll.scrollOffset.y + spy;
            string where = " (from " + before.ToString("F0") + " towards " + goal.ToString("F0") + ", at " + Scroll.scrollOffset.y.ToString("F0") +
                           "; tops " + string.Join(", ", targets.Take(4).Select(t => TopOf(t).ToString("F0"))) + ", line " + line.ToString("F0") + ")";
            Assert.Greater(line, TopOf(targets[2]), "precondition: the drag took the line past the third target's top" + where);
            Assert.Less(line, TopOf(targets[3]), "precondition: and not past the fourth's" + where);
            CollectionAssert.AreEquivalent(targets.Take(3).ToList(), Ticked, "exactly the three blocks the line passed are ticked");
            foreach (string block in targets.Take(3)) Assert.IsTrue(_state.Viewed(Lamp, block), block + " is remembered for this wall and point");
            Assert.IsFalse(_state.Viewed(Lamp, targets[3]), "the fourth block was never passed: not remembered");
            Assert.AreEqual(3, _state.CountViewed());

            yield return CloseAndReopen(Lamp, SheetStopRule.Stop.Full);
            Assert.AreEqual(0f, Scroll.scrollOffset.y, "precondition: reopened at its top, no target in view");
            CollectionAssert.AreEquivalent(targets.Take(3).ToList(), Ticked, "the same three ticks after closing and reopening the card");

            // - what the POI Editor's Reset Saved Card State calls, over this wall's store
            Assert.AreEqual(3, _state.ResetAll(), "the reset clears the three ticks");
            yield return CloseAndReopen(Lamp, SheetStopRule.Stop.Full);
            Assert.IsEmpty(Ticked, "after the reset and a reopen: no tick");
        }

        [UnityTest]
        public IEnumerator ARealTapOnARow_LandsItsBlock_WithOneNavJumpNamedNavContents_AndTicksOnlyThatBlock()
        {
            yield return UseAuto(BuiltInBlocks.NavContentsKind);
            foreach (int pick in new[] { 0, 27, 55 })
            {
                _state.ResetAll();
                yield return CloseAndReopen(Lamp, SheetStopRule.Stop.Full);
                var row = View.Rows[pick];
                // - bring the row on screen inside the (long) list, then tap it for real
                float rowTop = row.Button.worldBound.yMin - Scroll.contentContainer.worldBound.yMin;
                Scroll.scrollOffset = new Vector2(0f, Mathf.Max(0f, rowTop - Viewport * 0.5f));
                yield return CardTestInput.Settle(0.3f);
                Assert.IsTrue(Scroll.contentViewport.worldBound.Contains(row.Button.worldBound.center), "precondition: row " + pick + " is on screen");
                Assert.IsEmpty(Ticked, "precondition: nothing read yet");
                _events.Raised.Clear();
                yield return TouchTap(row.Button.worldBound.center);
                yield return Landed();
                AssertLanded(row.BlockKey, row.Heading.text + ", from a real tap on its row");
                var jumps = _events.Raised.Where(e => e.Kind == CardEventKinds.NavJump).ToList();
                Assert.AreEqual(1, jumps.Count, "one nav_jump");
                Assert.AreEqual(NavContentsBlockView.ElementKey, jumps[0].Variant, "named by the contents block");
                Assert.AreEqual(row.BlockKey, jumps[0].BlockKey);
                Assert.IsTrue(row.IsRead, row.Heading.text + ": the block it landed on is in view and ticks");
                float offset = Scroll.scrollOffset.y;
                foreach (string block in Ticked)
                {
                    var slot = Sheet.Stack.SlotOfBlock(block);
                    Assert.Greater(slot.layout.yMax, offset, block + " is ticked, so it must be on screen -- never a block the jump flew past");
                    Assert.LessOrEqual(slot.layout.y, offset + Viewport, block + ": on screen");
                }
            }
        }

        [UnityTest]
        public IEnumerator NothingIsTicked_AtThePeekStop()
        {
            yield return UseAuto(BuiltInBlocks.NavContentsKind);
            yield return Open(Lamp, SheetStopRule.Stop.Peek);
            yield return CardTestInput.Settle(0.5f);
            Assert.AreEqual(0, _state.CountViewed(), "at peek nothing of the stack is visible: no block is remembered as viewed");
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle(0.3f);
            // - at Full the visitor sees the top of the scroll (the hero picture, then the list): the card's blocks are below the line
            foreach (string block in Ticked)
                Assert.LessOrEqual(TopOf(block), Scroll.scrollOffset.y + Viewport * NavScrollRule.SpyLineFraction, block + ": ticked only with its top past the line");
        }

        [UnityTest]
        public IEnumerator TheRealList_ReadsWholeInEnglishAndPortuguese_SomeRowsTicked_AndRenders([Values("en", "pt")] string language)
        {
            yield return UseAuto(BuiltInBlocks.NavContentsKind, language);
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            // - a visitor who read some of the card: every other block of the first two families remembered
            var read = Model.Sections.Take(2).SelectMany(s => s.Targets).Where((key, i) => i % 2 == 0).ToList();
            foreach (string block in read) _state.MarkViewed(Lamp, block);
            yield return CloseAndReopen(Lamp, SheetStopRule.Stop.Full);
            CollectionAssert.AreEquivalent(read, Ticked, "exactly the remembered rows are ticked");
            var words = new CardStrings(Card.StringTable.Entries(), CardStringSources.Shared.Entries(), null, language, "en");
            var card = Sheet.Root.worldBound;
            for (int s = 0; s < View.Sections.Count; s++)
                Assert.AreEqual(words.Get(Model.Sections[s].LabelKey), View.Sections[s].Words.text, "section " + s + " in " + language);
            foreach (var row in View.Rows)
            {
                Assert.GreaterOrEqual(row.Button.layout.height, CardTestInput.TokenPx("--ts-touch-target") - OnePixel, row.Heading.text + ": a full touch target");
                Assert.LessOrEqual(row.Button.worldBound.xMax, card.xMax + OnePixel, row.Heading.text + ": inside the card");
                if (row.Minutes.resolvedStyle.display == DisplayStyle.Flex)
                {
                    var size = row.Minutes.MeasureTextSize(row.Minutes.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                    Assert.GreaterOrEqual(row.Minutes.layout.width + 0.5f, size.x, "'" + row.Minutes.text + "': whole");
                }
            }
            // - the list's top on screen: the rows under the header's hero picture
            float listTop = Sheet.Stack.SlotOfBlock(AutoKey).layout.y;
            Scroll.scrollOffset = new Vector2(0f, listTop);
            yield return CardTestInput.Settle(0.3f);
            yield return Capture("NavContents_Lamp_full_" + language);
        }
    }
}
