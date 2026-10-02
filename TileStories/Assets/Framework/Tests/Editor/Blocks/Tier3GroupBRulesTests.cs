using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The pure rules of Tier 3 group B (_3.1 step 8B): poll, collect, dialogue and show_on_wall -- what each block shows, how its numbers
    // are read, what it stores -- and how the card's builder uses them. Nothing here touches a view.
    public class Tier3GroupBRulesTests
    {
        private static List<LocalizedEntry> En(string value) => new() { new LocalizedEntry { lang = "en", value = value } };

        private static BlockItemData Row(params (string key, string text)[] fields)
        {
            var row = new BlockItemData();
            foreach (var (key, text) in fields) row.fields.Add(new BlockItemFieldValue { key = key, text = En(text) });
            return row;
        }

        private static BlockInstanceData Block(string kind, string variant, string question, params string[] options)
        {
            var block = new BlockInstanceData { key = "block_2", kind = kind, variant = variant };
            if (question != null) block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.PollQuestionField, text = En(question) });
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.PollOptionsField, items = options.Select(o => Row((BuiltInBlocks.PollOptionTextField, o))).ToList() });
            return block;
        }

        private static BlockFieldReader Read(BlockInstanceData block) => new(block, "en", "en");

        // ---------------- poll ----------------

        [Test]
        public void Poll_ShowsTheRowsWithWords_InOrder_AtMostSix_KeepingEachRowsAuthoredIndex()
        {
            var block = Block(BuiltInBlocks.PollKind, "", "Which?", "A", "", "B", "  ", "C", "D", "E", "F", "G");
            var options = PollRule.Options(Read(block));
            CollectionAssert.AreEqual(new[] { "A", "B", "C", "D", "E", "F" }, options.Select(o => o.Text).ToList(), "blank rows out, the sixth kept, the seventh dropped");
            CollectionAssert.AreEqual(new[] { 0, 2, 4, 5, 6, 7 }, options.Select(o => o.Row).ToList(), "the authored row, not the position among the shown");
            Assert.IsTrue(PollRule.IsShownRow(options, 2));
            Assert.IsFalse(PollRule.IsShownRow(options, 1), "a blank row is not an option");
            Assert.IsFalse(PollRule.IsShownRow(options, 8), "past the sixth");
            Assert.AreEqual("3", PollRule.EventValue(2), "an event names the option's row number from 1");
        }

        [Test]
        public void Poll_NeedsAQuestion_AndAtLeastTwoOptionsWithWords()
        {
            Assert.IsTrue(PollRule.CanShow(Read(Block(BuiltInBlocks.PollKind, "", "Q", "A", "B"))));
            Assert.IsFalse(PollRule.CanShow(Read(Block(BuiltInBlocks.PollKind, "", "Q", "A"))), "one option");
            Assert.IsFalse(PollRule.CanShow(Read(Block(BuiltInBlocks.PollKind, "", "Q", "A", "", " "))), "one option with words");
            Assert.IsFalse(PollRule.CanShow(Read(Block(BuiltInBlocks.PollKind, "", null, "A", "B"))), "no question");
            Assert.IsFalse(PollRule.CanShow(Read(Block(BuiltInBlocks.PollKind, "", "Q"))), "no rows");
        }

        [Test]
        public void Poll_Percentages_AreOnlyEverMadeFromCounts_AddUpTo100_AndAreNothingWithoutVotes()
        {
            var options = PollRule.Options(Read(Block(BuiltInBlocks.PollKind, "", "Q", "A", "", "B", "C")));
            Assert.IsNull(PollRule.Percentages(options, null), "no counts (no backend): nothing to show, never a made-up number");
            Assert.IsNull(PollRule.Percentages(options, new[] { 0, 0, 0, 0 }), "nobody voted: nothing to show");
            Assert.IsNull(PollRule.Percentages(new List<PollRule.Option>(), new[] { 5 }), "no options");
            CollectionAssert.AreEqual(new[] { 25, 75, 0 }, PollRule.Percentages(options, new[] { 1, 99, 3, 0 }), "counts are indexed by AUTHORED row: the blank row 1 is skipped");
            var thirds = PollRule.Percentages(options, new[] { 1, 0, 1, 1 });
            Assert.AreEqual(100, thirds.Sum(), "a third each still adds up to 100");
            CollectionAssert.AreEqual(new[] { 100, 0, 0 }, PollRule.Percentages(options, new[] { 7 }), "a count list shorter than the rows: the missing ones are 0");
            Assert.AreEqual(100, PollRule.Percentages(options, new[] { 3, 0, 5, 2 }).Sum());
        }

        [Test]
        public void ThePollState_IsKeptPerPoiAndBlock_ByAuthoredRow_ZeroIsAVote_AndResetAllForgetsIt()
        {
            var store = new MemoryCardStateStore();
            var state = new CardLocalState(store, "w");
            Assert.AreEqual(-1, state.PollVote("p", "b"));
            state.SetPollVote("p", "b", 0);
            Assert.AreEqual(0, state.PollVote("p", "b"), "row 0 is a vote");
            state.SetPollVote("p", "b", -3);
            Assert.AreEqual(0, state.PollVote("p", "b"), "a negative row is ignored");
            Assert.AreEqual(-1, state.PollVote("p", "other"), "another block");
            Assert.AreEqual(-1, state.Vote("p", "b"), "a poll vote is not a feedback vote: its own slot");
            Assert.IsTrue(store.TryGet("ts.card.w.p.b.poll", out _));
            Assert.AreEqual(1, state.ResetAll());
            Assert.AreEqual(-1, state.PollVote("p", "b"), "the Editor's reset takes it too");
        }

        [Test]
        public void ThePollDefaultResults_HaveNothing()
        {
            Assert.IsFalse(new NoPollResults().TryGet("w", "p", "b", out var counts));
            Assert.IsNull(counts);
        }

        // ---------------- collect ----------------

        private static POIData Poi(string id, params string[] collectBlocks)
        {
            var poi = new POIData { id = id, name = id };
            foreach (string key in collectBlocks) poi.card.blocks.Add(new BlockInstanceData { key = key, kind = BuiltInBlocks.CollectKind });
            poi.card.blocks.Add(new BlockInstanceData { key = "other", kind = BuiltInBlocks.QuickFactsKind });
            return poi;
        }

        [Test]
        public void Collect_CountsTheWallsCollectableItems_FromTheConfig_NotFromAConstant()
        {
            var settings = new CardSettings();
            var wall = new List<POIData> { Poi("a", "b1"), Poi("b"), Poi("c", "b1", "b2"), null };
            var items = CollectRule.Items(wall, settings);
            CollectionAssert.AreEqual(new[] { "a/b1", "c/b1", "c/b2" }, items.Select(i => i.PoiId + "/" + i.BlockKey).ToList(), "every collect block of every point, in order");
            wall.Add(Poi("d", "b1"));
            Assert.AreEqual(4, CollectRule.Items(wall, settings).Count, "author one more and the total grows");
            settings.kinds.Add(new BlockKindSetting { kind = BuiltInBlocks.CollectKind, enabled = false });
            Assert.AreEqual(0, CollectRule.Items(wall, settings).Count, "a wall that switches Collect off has none");
            Assert.AreEqual(0, CollectRule.Items(null, new CardSettings()).Count);
        }

        [Test]
        public void Collect_Progress_CountsWhatTheVisitorHas_AndAlwaysCountsTheBlockBeingShown()
        {
            var items = CollectRule.Items(new List<POIData> { Poi("a", "b1"), Poi("c", "b1", "b2") }, new CardSettings());
            var have = new HashSet<string> { "a/b1", "c/b2" };
            bool Has(string poi, string block) => have.Contains(poi + "/" + block);
            Assert.AreEqual((2, 3), CollectRule.Progress(items, "c", "b1", Has));
            have.Add("c/b1");
            Assert.AreEqual((3, 3), CollectRule.Progress(items, "c", "b1", Has));
            // - a card shown for a point the list does not hold still counts itself: never "0 of 0"
            Assert.AreEqual((0, 1), CollectRule.Progress(new List<CollectRule.Item>(), "z", "b1", Has));
            have.Add("z/b1");
            Assert.AreEqual((1, 1), CollectRule.Progress(new List<CollectRule.Item>(), "z", "b1", Has));
        }

        [Test]
        public void Collect_NamesTheItemByItsOwnWords_ElseThePointsTitle_AndTheSeriesByItsOwnWords_ElseTheCategory()
        {
            var taxonomy = new WallConfigData();
            taxonomy.category_styles.Add(new CategoryStyleEntry { key = "civic", label = "Civic Buildings" });
            var poi = new POIData { id = "gate", name = "Gate", category = "civic" };
            var empty = new BlockInstanceData { key = "b", kind = BuiltInBlocks.CollectKind };
            Assert.AreEqual("Gate", CollectRule.ItemName(Read(empty), poi, "en", "en"), "the point's card title");
            Assert.AreEqual("Civic Buildings", CollectRule.Series(Read(empty), poi, taxonomy), "the category's name, never its key");
            var own = new BlockInstanceData { key = "b", kind = BuiltInBlocks.CollectKind };
            own.fields.Add(new BlockFieldValue { key = BuiltInBlocks.CollectItemNameField, text = En("Old gate stamp") });
            own.fields.Add(new BlockFieldValue { key = BuiltInBlocks.CollectSeriesField, text = En("Gates") });
            Assert.AreEqual("Old gate stamp", CollectRule.ItemName(Read(own), poi, "en", "en"));
            Assert.AreEqual("Gates", CollectRule.Series(Read(own), poi, taxonomy));
        }

        [Test]
        public void TheCollectedState_IsKeptPerPoiAndBlock_AndResetAllForgetsIt()
        {
            var store = new MemoryCardStateStore();
            var state = new CardLocalState(store, "w");
            Assert.IsFalse(state.Collected("p", "b"));
            state.SetCollected("p", "b");
            Assert.IsTrue(state.Collected("p", "b"));
            Assert.IsFalse(state.Collected("p", "other"));
            Assert.IsFalse(state.Collected("q", "b"));
            Assert.IsFalse(new CardLocalState(store, "another wall").Collected("p", "b"));
            Assert.IsTrue(store.TryGet("ts.card.w.p.b.collected", out _));
            state.ResetAll();
            Assert.IsFalse(state.Collected("p", "b"));
        }

        // ---------------- dialogue ----------------

        private static BlockInstanceData Dialogue(params BlockItemData[] rows)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.DialogueKind };
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.DialogueLinesField, items = rows.ToList() });
            return block;
        }

        [Test]
        public void Dialogue_ReadsLinesInOrder_WithTheirRepliesInFlatSlots_AndLeavesOutWhatCannotBeShown()
        {
            var block = Dialogue(
                Row(("speaker", "Mason"), ("line", "Hello."), ("choice_1", "Hi"), ("reply_1", "Welcome."), ("choice_2", "Bye"), ("reply_2", "")),
                Row(("speaker", "Mason"), ("line", "")),
                Row(("line", "No speaker."), ("reply_1", "A reply with no choice"), ("choice_3", "Third")));
            var lines = DialogueRule.Lines(Read(block));
            Assert.AreEqual(2, lines.Count, "the row with no words is left out");
            Assert.AreEqual(0, lines[0].Row);
            Assert.AreEqual(2, lines[1].Row, "the authored row is kept");
            Assert.AreEqual("Mason", lines[0].Speaker);
            Assert.AreEqual("", lines[1].Speaker);
            CollectionAssert.AreEqual(new[] { "Hi", "Bye" }, lines[0].Choices.Select(c => c.Label).ToList());
            Assert.AreEqual("Welcome.", lines[0].Choices[0].Reply);
            Assert.AreEqual("", lines[0].Choices[1].Reply, "a choice with no reply just moves on");
            CollectionAssert.AreEqual(new[] { "Third" }, lines[1].Choices.Select(c => c.Label).ToList(), "a reply with no choice label is never offered");
        }

        [Test]
        public void Dialogue_Problems_NameTheRowsTheEditorShouldWarnAbout_ByRowNumberFromOne()
        {
            var block = Dialogue(
                Row(("line", "Fine.")),
                Row(("speaker", "Mason"), ("line", " ")),
                Row(("line", "Also fine."), ("choice_1", "Yes"), ("reply_1", "ok"), ("reply_2", "orphan")));
            DialogueRule.Problems(Read(block), out var noWords, out var hidden);
            CollectionAssert.AreEqual(new[] { 2 }, noWords);
            var orphan = hidden.Single();
            Assert.AreEqual((3, 2, 2), (orphan.Row, orphan.Slot, orphan.FillSlot), "line 3: Reply 2 has text, Choice 2 is empty and is the Choice to write (Choice 1 has words)");
            DialogueRule.Problems(Read(Dialogue(Row(("line", "Only")))), out noWords, out hidden);
            CollectionAssert.IsEmpty(noWords);
            CollectionAssert.IsEmpty(hidden);
        }

        [Test]
        public void Dialogue_Problems_NameEveryHiddenReply_AndTheChoiceTheDeveloperCanActuallySeeToFill()
        {
            // - Choice 1 filled, Choice 2 empty with a Reply 2, Choice 3 empty with a Reply 3: each reply named, the Choice to write is the first empty one
            var block = Dialogue(
                Row(("line", "A"), ("choice_1", "Yes"), ("reply_2", "second"), ("reply_3", "third")),
                // - Choice 1 empty (Choices 2 and 3 are then hidden in the Editor): the one to fill is Choice 1, for every reply
                Row(("line", "B"), ("reply_1", "first"), ("reply_3", "third")));
            DialogueRule.Problems(Read(block), out _, out var hidden);
            CollectionAssert.AreEqual(new[] { (1, 2, 2), (1, 3, 2), (2, 1, 1), (2, 3, 1) }, hidden.Select(h => (h.Row, h.Slot, h.FillSlot)).ToList());
            // - a reply whose Choice HAS words is no problem, and a choice with no reply is none either
            DialogueRule.Problems(Read(Dialogue(Row(("line", "C"), ("choice_1", "Yes"), ("reply_1", "ok"), ("choice_2", "No")))), out _, out hidden);
            CollectionAssert.IsEmpty(hidden);

            // - the brief's case, word for word: Choices 1 and 2 filled, a Reply 3 with text and Choice 3 (visible now) empty
            var warnings = POIEditorToolWindow.CardBlockWarnings(Dialogue(Row(("line", "D"), ("choice_1", "Yes"), ("choice_2", "No"), ("reply_3", "orphan"))),
                BuiltInBlocks.Dialogue, new CardSettings(), new POIData { id = "p", name = "Tower" });
            Assert.AreEqual("Line 1: Reply 3 has text but Choice 3 is empty: fill Choice 3 to see it.", warnings.Single());
            EditorTextChecks.AssertAscii(warnings.Single(), "the dialogue warning");
        }

        // ---------------- the builder ----------------

        private static BlockStackBuilder.Result Build(POIData poi, params POIData[] others) =>
            BlockStackBuilder.Build(poi, new CardSettings(), BlockRegistry.Shared, new List<POIData>(others) { poi });

        [Test]
        public void TheBuilder_ShowsAPollOnlyWithAQuestionAndTwoOptions_ADialogueOnlyWithALineWithWords()
        {
            var poi = new POIData { id = "p", name = "Tower" };
            poi.card.blocks.Add(Block(BuiltInBlocks.PollKind, "", "Q", "only one"));
            var skipped = Build(poi).Skipped.Single();
            Assert.AreEqual(BlockStackBuilder.SkipReason.NotForThisPoint, skipped.Reason);
            StringAssert.Contains("at least two options", BuiltInBlocks.Poll.NotShownForPoiNote);
            poi.card.blocks[0] = Block(BuiltInBlocks.PollKind, "", "Q", "one", "two");
            Assert.IsEmpty(Build(poi).Skipped);

            poi.card.blocks[0] = Dialogue(Row(("line", "  ")));
            Assert.AreEqual(BlockStackBuilder.SkipReason.NoCompleteRow, Build(poi).Skipped.Single().Reason, "no row has words");
            poi.card.blocks[0] = Dialogue(Row(("line", "Something to say.")));
            Assert.IsEmpty(Build(poi).Skipped);
        }

        [Test]
        public void TheBuilder_ShowsShowOnWallAlways_ButTheNeighbourLookOnlyWithAnotherPointOnTheWall()
        {
            var poi = new POIData { id = "p", name = "Tower" };
            poi.card.blocks.Add(new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.ShowOnWallKind, variant = BuiltInBlocks.ShowOnWallButton });
            Assert.IsEmpty(Build(poi).Skipped, "the button needs nothing");
            poi.card.blocks[0].variant = BuiltInBlocks.ShowOnWallWithNeighbours;
            Assert.AreEqual(BlockStackBuilder.SkipReason.NotForThisPoint, Build(poi).Skipped.Single().Reason, "alone on the wall: nobody to name");
            Assert.IsEmpty(Build(poi, new POIData { id = "q", name = "Other" }).Skipped);
        }

        [Test]
        public void TheFourNewKinds_AreRegisteredInCatalogOrder_InTheirFamilies()
        {
            var keys = BlockRegistry.Shared.All.Select(k => k.Key).ToList();
            string[] tier3 = { "knowledge_check", "poll", "collect", "feedback", "dialogue", "show_on_wall" };
            CollectionAssert.AreEqual(tier3, keys.Where(k => tier3.Contains(k)).ToList(), "the catalog's Tier 3 order");
            Assert.AreEqual("play", BuiltInBlocks.Poll.Family);
            Assert.AreEqual("play", BuiltInBlocks.Collect.Family);
            Assert.AreEqual("stories", BuiltInBlocks.Dialogue.Family);
            Assert.AreEqual("ar", BuiltInBlocks.ShowOnWall.Family);
            CollectionAssert.AreEqual(new[] { "bars" }, BuiltInBlocks.Poll.Variants);
            CollectionAssert.AreEqual(new[] { "add_to_story" }, BuiltInBlocks.Collect.Variants);
            CollectionAssert.AreEqual(new[] { "choices" }, BuiltInBlocks.Dialogue.Variants);
            CollectionAssert.AreEqual(new[] { "button", "with_neighbours" }, BuiltInBlocks.ShowOnWall.Variants);
        }

        // ---------------- the Editor's warnings ----------------

        [Test]
        public void TheCardContentWarns_OfAPollWithMoreOptionsThanItShows_AndOfDialogueRowsThatCannotShow()
        {
            var settings = new CardSettings();
            var poi = new POIData { id = "p", name = "Tower" };
            var seven = Block(BuiltInBlocks.PollKind, "", "Q", "1", "2", "3", "4", "5", "6", "7");
            var warning = POIEditorToolWindow.CardBlockWarnings(seven, BuiltInBlocks.Poll, settings, poi).Single();
            Assert.AreEqual("A poll shows at most 6 options: the last 1 option is not shown. Delete it or merge options.", warning);
            CollectionAssert.IsEmpty(POIEditorToolWindow.CardBlockWarnings(Block(BuiltInBlocks.PollKind, "", "Q", "1", "2", "3", "4", "5", "6", "", ""), BuiltInBlocks.Poll, settings, poi), "six with words: fine (blank rows do not count)");

            var dialogue = Dialogue(
                Row(("line", "Fine.")),
                Row(("line", "")),
                Row(("line", "Fine too."), ("reply_2", "nobody can pick this")));
            var warnings = POIEditorToolWindow.CardBlockWarnings(dialogue, BuiltInBlocks.Dialogue, settings, poi);
            Assert.AreEqual(2, warnings.Count);
            Assert.AreEqual("Not shown: line 2 (a row with no words in Line).", warnings[0]);
            // - Choice 1 is empty too, so Choice 2 is a field the Editor hides: the text names Choice 1, the one the developer can see
            Assert.AreEqual("Line 3: Reply 2 has text but Choice 2 is empty: fill Choice 1 first (Choice 2 shows once the choice before it has words), then Choice 2 to see it.", warnings[1]);
            foreach (string w in warnings) StringAssert.DoesNotContain("block_", w, "rows are named by their place in the table, never an id");
            CollectionAssert.IsEmpty(POIEditorToolWindow.CardBlockWarnings(Dialogue(Row(("line", "Only line."))), BuiltInBlocks.Dialogue, settings, poi));
        }

        // ---------------- the same action offered more than once (_3.1 audit 15.B) ----------------

        private static BlockInstanceData ActionsBlock(string variant, params (string words, string action)[] buttons)
        {
            var block = new BlockInstanceData { key = "block_3", kind = BuiltInBlocks.ActionsKind, variant = variant };
            var rows = new List<BlockItemData>();
            foreach (var (words, action) in buttons)
            {
                var row = new BlockItemData();
                row.fields.Add(new BlockItemFieldValue { key = BuiltInBlocks.ActionsLabelField, text = En(words) });
                row.fields.Add(new BlockItemFieldValue { key = BuiltInBlocks.ActionsActionField, value = action });
                rows.Add(row);
            }
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.ActionsItemsField, items = rows });
            return block;
        }

        private static POIData CardWith(params BlockInstanceData[] blocks)
        {
            var poi = new POIData { id = "p", name = "Tower" };
            poi.card.blocks.AddRange(blocks);
            return poi;
        }

        // The shown stack of a card (BlockStackBuilder), the way the Editor feeds RepeatedActionRule
        private static List<RepeatedActionRule.Repeat> RepeatsOf(POIData poi, CardSettings settings = null) =>
            RepeatedActionRule.Find(BlockStackBuilder.Build(poi, settings ?? new CardSettings(), BlockRegistry.Shared,
                new List<POIData> { poi, new POIData { id = "q", name = "Other" } }).Entries);

        private static BlockInstanceData ShowOnWallBlock(string variant = BuiltInBlocks.ShowOnWallButton) =>
            new() { key = "block_2", kind = BuiltInBlocks.ShowOnWallKind, variant = variant };

        private static readonly (string, string) See = ("See it on the wall", BuiltInBlocks.ActionShowOnWall);

        [Test]
        public void ActionsRule_DrawsKnownActionsOnly_TheStickyLookTheFirstOnly_AndARowWithNoWordsStillDraws()
        {
            var block = ActionsBlock(BuiltInBlocks.ActionsPillRow, ("", BuiltInBlocks.ActionShowOnWall), ("Listen", "listen"), See, ("No action", ""));
            var pills = ActionsRule.Buttons(Read(block), BuiltInBlocks.ActionsPillRow);
            CollectionAssert.AreEqual(new[] { 0, 2 }, pills.Select(b => b.Row), "an unknown action and a row with no action are left out; the empty-words row is NOT");
            Assert.AreEqual("", pills[0].Words, "no words of its own: the view reads the action's card text");
            Assert.AreEqual("See it on the wall", pills[1].Words);
            var sticky = ActionsRule.Buttons(Read(block), BuiltInBlocks.ActionsStickyCta);
            CollectionAssert.AreEqual(new[] { 0 }, sticky.Select(b => b.Row), "sticky: one call to action, the first drawn row");
            Assert.AreEqual(CardStrings.Keys.ShowOnWallButton, ActionsRule.DefaultWordsKey(BuiltInBlocks.ActionShowOnWall), "the Show On Wall kind's own row");
            Assert.IsNull(ActionsRule.DefaultWordsKey("listen"));
            Assert.IsTrue(ActionsRule.IsKnown(BuiltInBlocks.ActionShowOnWall));
            Assert.IsFalse(ActionsRule.IsKnown("listen"));
        }

        [Test]
        public void RepeatedActions_AShowOnWallBlockAndAStickyButton_AreTwoOffers_TheStickyOneMarkedPinned()
        {
            var sticky = ActionsBlock(BuiltInBlocks.ActionsStickyCta, See);
            var repeat = RepeatsOf(CardWith(ShowOnWallBlock(), sticky)).Single();
            Assert.AreEqual(BuiltInBlocks.ActionShowOnWall, repeat.Action);
            Assert.AreEqual(2, repeat.Offers.Count);
            Assert.AreEqual(BuiltInBlocks.ShowOnWallKind, repeat.Offers[0].Definition.Key);
            Assert.IsFalse(repeat.Offers[0].Pinned);
            Assert.AreSame(sticky, repeat.Offers[1].Block);
            Assert.IsTrue(repeat.Offers[1].Pinned, "the sticky look is the footer's button");
            Assert.AreEqual("See it on the wall", repeat.Offers[1].Words);
        }

        [Test]
        public void RepeatedActions_EveryLookCounts_AndSoDoesTheNeighboursLook_ButOnlyWhatTheCardReallyShows()
        {
            // - pill row, circles and the sticky footer are all buttons on one screen (the old rule warned about the footer only)
            Assert.AreEqual(2, RepeatsOf(CardWith(ShowOnWallBlock(), ActionsBlock(BuiltInBlocks.ActionsPillRow, See))).Single().Offers.Count);
            Assert.AreEqual(2, RepeatsOf(CardWith(ShowOnWallBlock(), ActionsBlock(BuiltInBlocks.ActionsCircles, See))).Single().Offers.Count);
            // - the audit's "four times": circles + pill row + the sticky footer + a Show On Wall block
            var four = CardWith(ActionsBlock(BuiltInBlocks.ActionsCircles, See), ActionsBlock(BuiltInBlocks.ActionsPillRow, See),
                ActionsBlock(BuiltInBlocks.ActionsStickyCta, See), ShowOnWallBlock());
            Assert.AreEqual(4, RepeatsOf(four).Single().Offers.Count);
            // - the neighbours look shows only while another point is on the wall (RepeatsOf has one)
            Assert.AreEqual(2, RepeatsOf(CardWith(ShowOnWallBlock(BuiltInBlocks.ShowOnWallWithNeighbours), ActionsBlock(BuiltInBlocks.ActionsStickyCta, See))).Single().Offers.Count);
            var alone = BlockStackBuilder.Build(CardWith(ShowOnWallBlock(BuiltInBlocks.ShowOnWallWithNeighbours), ActionsBlock(BuiltInBlocks.ActionsStickyCta, See)),
                new CardSettings(), BlockRegistry.Shared, null);
            CollectionAssert.IsEmpty(RepeatedActionRule.Find(alone.Entries), "alone on the wall the neighbours block is not on the card: nothing repeats");
        }

        [Test]
        public void RepeatedActions_NothingRepeats_WhenTheCardOffersEachActionOnce_OrALoneStickyHasTwoRows_OrABlockIsNotShown()
        {
            CollectionAssert.IsEmpty(RepeatsOf(CardWith(ShowOnWallBlock())), "one Show On Wall block");
            CollectionAssert.IsEmpty(RepeatsOf(CardWith(ActionsBlock(BuiltInBlocks.ActionsPillRow, See))), "one Actions button");
            CollectionAssert.IsEmpty(RepeatsOf(CardWith(ActionsBlock(BuiltInBlocks.ActionsStickyCta, See, See))), "sticky draws its first row only: one button");
            CollectionAssert.IsEmpty(RepeatsOf(CardWith(ShowOnWallBlock(), ActionsBlock(BuiltInBlocks.ActionsPillRow, ("Listen", "listen")))), "an action this framework does not know is no offer");
            var off = new CardSettings();
            off.kinds.Add(new BlockKindSetting { kind = BuiltInBlocks.ActionsKind, enabled = false });
            CollectionAssert.IsEmpty(RepeatsOf(CardWith(ShowOnWallBlock(), ActionsBlock(BuiltInBlocks.ActionsStickyCta, See)), off), "Actions is off in the Block Library: it offers nothing");
            // - two rows of the same action in one pill row repeat inside that one block
            var twice = ActionsBlock(BuiltInBlocks.ActionsPillRow, See, ("Where?", BuiltInBlocks.ActionShowOnWall));
            var repeat = RepeatsOf(CardWith(twice)).Single();
            Assert.AreEqual(2, repeat.Offers.Count);
            Assert.IsTrue(repeat.Offers.All(o => ReferenceEquals(o.Block, twice)));
            // - a button with no words is still a button (it reads the action's card text)
            Assert.AreEqual(2, RepeatsOf(CardWith(ShowOnWallBlock(), ActionsBlock(BuiltInBlocks.ActionsStickyCta, ("", BuiltInBlocks.ActionShowOnWall)))).Single().Offers.Count);
        }

        [Test]
        public void TheEditorWarns_OncePerRepeatedAction_UnderEveryBlockThatOffersIt_NamingThemAsTheEditorDoes()
        {
            var showOnWall = ShowOnWallBlock();
            var circles = ActionsBlock(BuiltInBlocks.ActionsCircles, ("On the wall", BuiltInBlocks.ActionShowOnWall));
            var sticky = ActionsBlock(BuiltInBlocks.ActionsStickyCta, ("", BuiltInBlocks.ActionShowOnWall));
            var other = ActionsBlock(BuiltInBlocks.ActionsPillRow, ("Listen", "listen"));
            var repeats = RepeatsOf(CardWith(showOnWall, circles, sticky, other));
            string text = POIEditorToolWindow.CardRepeatedActionText(repeats.Single());
            foreach (var involved in new[] { showOnWall, circles, sticky })
                Assert.AreEqual(text, POIEditorToolWindow.CardRepeatedActionWarnings(involved, repeats).Single(), "the same one warning under each of the three");
            CollectionAssert.IsEmpty(POIEditorToolWindow.CardRepeatedActionWarnings(other, repeats), "a block that offers nothing repeated has none");
            StringAssert.Contains("\"Show On The Wall\" 3 times", text, "the action as the Actions block's Action row names it, and how often");
            StringAssert.Contains("Show On Wall (button)", text);
            StringAssert.Contains("Actions (circles) saying \"On the wall\"", text);
            StringAssert.Contains("Actions (sticky_cta, pinned to the footer)", text);
            StringAssert.DoesNotContain("saying \"\"", text, "a button with no words says nothing about its words");
            StringAssert.DoesNotContain("block_", text, "blocks are named by kind and look, never by key");
            EditorTextChecks.AssertAscii(text, "the repeated-action warning");
            CollectionAssert.IsEmpty(POIEditorToolWindow.CardRepeatedActionWarnings(showOnWall, null), "no repeats computed: no warning");
        }

        [Test]
        public void AnActionsButtonWithNoWords_ReadsTheSameCardTextAsTheShowOnWallKind_InEveryLanguageTheTableHas()
        {
            var table = AssetDatabase.LoadAssetAtPath<CardStringTable>("Assets/Framework/Runtime/UI/Cards/CardStrings.asset");
            Assert.IsNotNull(table, "precondition: the framework's card texts load");
            foreach (string lang in new[] { "en", "pt" })
            {
                var strings = new CardStrings(table.Entries(), null, null, lang, "en");
                string expected = strings.Get(CardStrings.Keys.ShowOnWallButton);
                Assert.IsNotEmpty(expected, lang);
                var view = new ActionsBlockView();
                var block = ActionsBlock(BuiltInBlocks.ActionsStickyCta, ("", BuiltInBlocks.ActionShowOnWall));
                view.Bind(block, new BlockBindContext { Strings = strings, Language = lang, FallbackLanguage = "en", Variant = BuiltInBlocks.ActionsStickyCta });
                Assert.AreEqual(expected, view.Actions.Single().Label.text, lang + ": the Actions button says what the Show On Wall button says");
                view.Unbind();
                var own = ActionsBlock(BuiltInBlocks.ActionsStickyCta, ("Own words", BuiltInBlocks.ActionShowOnWall));
                view.Bind(own, new BlockBindContext { Strings = strings, Language = lang, FallbackLanguage = "en", Variant = BuiltInBlocks.ActionsStickyCta });
                Assert.AreEqual("Own words", view.Actions.Single().Label.text, lang + ": words the block carries win");
            }
        }
    }
}
