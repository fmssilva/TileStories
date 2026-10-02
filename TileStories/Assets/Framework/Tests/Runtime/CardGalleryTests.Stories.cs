using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace TileStories.Tests
{
    // Phase A promises of the stories family: timeline, person, story_chapters, compare_points, dialogue.
    public partial class CardGalleryTests
    {
        // ---------------- timeline ----------------

        [UnityTest]
        public IEnumerator Timeline_Vertical_RunsDownTheCardInTheAuthoredOrder_AndEndsInTheNowPoint()
        {
            TimelineBlockView timeline = null;
            yield return ShowBlock("timeline_vertical_long", v => timeline = (TimelineBlockView)v);
            Assert.AreEqual(7, timeline.Events.Count, "six events + Now");
            CollectionAssert.AreEqual(new[] { "1147", "c. 1300", "1511", "1 Nov 1755", "1910", "1940", "Now" }, timeline.Events.Select(e => e.Date.text),
                "the authored order, never re-sorted; Now from the card strings");
            for (int i = 1; i < timeline.Events.Count; i++)
                Assert.Greater(timeline.Events[i].Box.worldBound.yMin, timeline.Events[i - 1].Box.worldBound.yMin, "down the card");
            Assert.IsNull(timeline.Track.parent, "vertical: no swipe track");
            var now = timeline.Events.Last();
            Assert.IsTrue(now.IsNow && now.Box.ClassListContains("card-timeline__event--now"));
            Assert.IsFalse(CardTestInput.IsShown(now.Title, timeline.Root), "Now has no title of its own");
            Assert.Greater(now.Dot.worldBound.width, timeline.Events[0].Dot.worldBound.width, "the Now point is the larger ring");
            Assert.AreEqual(Visibility.Hidden, now.Rail.resolvedStyle.visibility, "the line ends at Now");
            Assert.AreEqual(Visibility.Visible, timeline.Events[5].Rail.resolvedStyle.visibility, "...and runs up to it");
            Assert.AreEqual(timeline.Events[0].Dot.worldBound.center.x, timeline.Events[6].Dot.worldBound.center.x, 0.5f, "every dot on one line, Now included");
            yield return Render("Card_timeline_vertical_now");

            yield return ShowBlock("timeline_vertical_short", v => timeline = (TimelineBlockView)v);
            Assert.IsFalse(timeline.Events.Any(e => e.IsNow), "Highlight Now off: no Now point");
            Assert.AreEqual(Visibility.Hidden, timeline.Events.Last().Rail.resolvedStyle.visibility, "the line ends at the last event");

            yield return ShowBlock("timeline_vertical_partial", v => timeline = (TimelineBlockView)v);
            CollectionAssert.AreEqual(new[] { "1147", "1940", "Now" }, timeline.Events.Select(e => e.Date.text), "no date or no title: left out");
        }

        [UnityTest]
        public IEnumerator Timeline_Horizontal_LaysTheEventsSideBySide_OnATrackASwipeReachesTheEndOf()
        {
            TimelineBlockView timeline = null;
            yield return ShowBlock("timeline_horizontal_long", v => timeline = (TimelineBlockView)v);
            Assert.AreEqual(timeline.Root, timeline.Track.parent, "horizontal: the events sit on a swipe track");
            Assert.AreEqual(DisplayStyle.None, timeline.Track.horizontalScroller.resolvedStyle.display, "no desktop scroll bar on the phone card");
            var first = timeline.Events[0].Box.worldBound;
            var second = timeline.Events[1].Box.worldBound;
            Assert.AreEqual(first.yMin, second.yMin, 0.5f, "side by side");
            Assert.Greater(second.xMin, first.xMin);
            Assert.AreEqual(timeline.Events[0].Dot.worldBound.center.y, timeline.Events[6].Dot.worldBound.center.y, 0.5f, "every dot on one line");
            Rect card = _harness.Sheet.Root.worldBound;
            var last = timeline.Events.Last();
            Assert.Greater(last.Box.worldBound.xMin, card.xMax, "precondition: the end of the line starts off the card");
            Assert.Greater(timeline.Events[0].Rail.worldBound.width, 0f, "the line runs from each dot towards the next");

            timeline.Track.ScrollTo(last.Box);
            yield return CardTestInput.Settle(0.1f);
            Assert.LessOrEqual(last.Box.worldBound.xMax, card.xMax + 0.5f, "scrolled along the track, the Now point is on the card");
            Assert.AreEqual("Now", last.Date.text);
            yield return Render("Card_timeline_horizontal_end");

            yield return ShowBlock("timeline_horizontal_short", v => timeline = (TimelineBlockView)v);
            Assert.AreEqual(0f, timeline.Track.scrollOffset.x, "a new bind starts at the beginning of the line");
        }

        // ---------------- person ----------------

        [UnityTest]
        public IEnumerator Person_TheInitialSitsBesideTheNameAndRole_TheCardLookIsSetApart_AndEmptyPartsTakeNoSpace()
        {
            PersonBlockView person = null;
            yield return ShowBlock("person_row_short", v => person = (PersonBlockView)v);
            Assert.AreEqual("A", person.Monogram.text, "the first letter of the name holds the photo's place");
            Assert.AreEqual("Afonso Henriques", person.Name.text);
            Assert.AreEqual("First king", person.Role.text);
            Assert.Less(person.Monogram.worldBound.xMax, person.Name.worldBound.xMin, "the initial beside the name");
            Assert.AreEqual(person.Monogram.worldBound.width, person.Monogram.worldBound.height, OnePixel(person.Monogram) + 0.01f, "round");
            Assert.GreaterOrEqual(person.Role.worldBound.yMin, person.Name.worldBound.yMax - 0.5f, "the role under the name");
            Assert.GreaterOrEqual(person.Text.Root.worldBound.yMin, person.Monogram.worldBound.yMax - 0.5f, "the text under them");
            float rowName = person.Name.resolvedStyle.fontSize;
            Assert.AreEqual(0f, person.Root.resolvedStyle.borderTopWidth, "row: in line with the card, no panel");

            yield return ShowBlock("person_card_long", v => person = (PersonBlockView)v);
            Assert.Greater(person.Root.resolvedStyle.borderTopWidth, 0f, "card: an accent edge on its own panel");
            Assert.AreNotEqual(CardTestInput.EffectiveBackground(person.Root.parent), person.Root.resolvedStyle.backgroundColor, "...a panel of its own colour, not the card's behind it");
            Assert.Greater(person.Name.resolvedStyle.fontSize, rowName, "card: the name larger");
            Assert.AreEqual("D", person.Monogram.text);
            Assert.AreEqual(person.Name.worldBound.yMin, person.Monogram.worldBound.yMin, 1f, "a long name: the initial starts level with it, not half way down");
            Assert.AreEqual(2, person.Text.Paragraphs.Count);

            yield return ShowBlock("person_row_nameonly", v => person = (PersonBlockView)v);
            Assert.IsFalse(CardTestInput.IsShown(person.Role, person.Root), "no role written: none shown");
            Assert.IsFalse(CardTestInput.IsShown(person.Text.Root, person.Root), "no text: none shown");

            Assert.AreEqual("M", PersonBlockView.InitialOf("  manuel"), "upper case, spaces ignored");
            Assert.AreEqual("G", PersonBlockView.InitialOf("1640 Gate-keeper"), "skips to the first letter");
            Assert.AreEqual("É", PersonBlockView.InitialOf("élia"), "an accented letter is a letter");
            Assert.AreEqual("", PersonBlockView.InitialOf(""));
        }

        // ---------------- story_chapters ----------------

        [UnityTest]
        public IEnumerator StoryChapters_RealTapsOnNextAndPrevious_TurnTheChapters_TheBarAndTheCounterFollow()
        {
            StoryChaptersBlockView story = null;
            yield return ShowBlock("story_chapters_segmented_long", v => story = (StoryChaptersBlockView)v);
            Assert.AreEqual(4, story.Count);
            Assert.AreEqual(4, story.Segments.Count, "one segment per chapter");
            Assert.AreEqual(0, story.Index, "a bind opens the first chapter");
            Assert.AreEqual("Chapter 1 of 4", story.Counter.text, "the counter from the card strings");
            StringAssert.StartsWith("The siege of the town", story.Title.text);
            Assert.AreEqual(Visibility.Hidden, story.Previous.resolvedStyle.visibility, "nothing before the first chapter");
            Assert.AreEqual("Previous", story.Previous.Q<Label>().text);
            Assert.AreEqual("Next", story.Next.Q<Label>().text);
            Assert.IsTrue(story.Segments[0].ClassListContains("card-story__segment--current"));
            Assert.AreEqual(story.Segments[0].worldBound.width, story.Segments[3].worldBound.width, OnePixel(story.Segments[0]) + 0.5f, "equal segments");
            var nextSpot = story.Next.worldBound.center;

            yield return CardTestInput.Tap(story.Next.panel, story.Next.worldBound.center);
            yield return null;
            Assert.AreEqual(1, story.Index, "a real tap on Next");
            Assert.AreEqual("Chapter 2 of 4", story.Counter.text);
            Assert.AreEqual("The palace", story.Title.text);
            StringAssert.Contains("painted tiles", story.Body.Paragraphs[0].text);
            CollectionAssert.AreEqual(new[] { true, true, false, false }, story.Segments.Select(s => s.ClassListContains("card-story__segment--read")), "the bar fills up to the chapter shown");
            Assert.AreEqual(Visibility.Visible, story.Previous.resolvedStyle.visibility);
            Assert.AreEqual(nextSpot.x, story.Next.worldBound.center.x, 0.5f, "Next keeps its side: Previous appearing does not push it");
            Assert.LessOrEqual(story.Next.worldBound.yMax, _harness.Sheet.Root.worldBound.yMax + 0.5f, "...and stays on the card");
            yield return Render("Card_story_chapters_second");

            yield return CardTestInput.Tap(story.Next.panel, story.Next.worldBound.center);
            yield return CardTestInput.Tap(story.Next.panel, story.Next.worldBound.center);
            yield return null;
            Assert.AreEqual(3, story.Index);
            Assert.AreEqual(Visibility.Hidden, story.Next.resolvedStyle.visibility, "nothing after the last chapter");
            yield return CardTestInput.Tap(story.Next.panel, story.Next.worldBound.center);
            yield return null;
            Assert.AreEqual(3, story.Index, "a tap on the hidden Next does nothing");

            yield return CardTestInput.Tap(story.Previous.panel, story.Previous.worldBound.center);
            yield return null;
            Assert.AreEqual(2, story.Index, "a real tap on Previous goes back");
            Assert.AreEqual("The earthquake", story.Title.text);

            yield return ShowBlock("story_chapters_segmented_short", v => story = (StoryChaptersBlockView)v);
            Assert.AreEqual(0, story.Index, "a new bind starts at the first chapter again: nothing remembered");

            yield return ShowBlock("story_chapters_segmented_single", v => story = (StoryChaptersBlockView)v);
            Assert.AreEqual(1, story.Count, "no title or no text: left out");
            Assert.AreEqual("Chapter 1 of 1", story.Counter.text);
            Assert.IsFalse(CardTestInput.IsShown(story.Nav, story.Root), "one chapter: no buttons");
        }

        // _3.1 step 6C: "Next" rendered as an egg (a short word in a round-cornered box). Previous / Next now wear the ONE
        // pill of the actions: the same height and corner radius, the radius half the height, never narrower than tall
        [UnityTest]
        public IEnumerator StoryChapters_PreviousAndNext_AreTheActionsPill_NeverAnEgg()
        {
            StoryChaptersBlockView story = null;
            yield return ShowBlock("story_chapters_segmented_short", v => story = (StoryChaptersBlockView)v);
            story.Show(1);
            yield return CardTestInput.Settle(0.15f);
            var buttons = new[] { story.Previous, story.Next };
            var sizes = buttons.Select(b => (b.worldBound.width, b.worldBound.height, b.resolvedStyle.borderTopLeftRadius)).ToArray();

            ActionsBlockView actions = null;
            yield return ShowBlock("actions_pill_row_short", v => actions = (ActionsBlockView)v);
            var pill = actions.Actions[0].Button;
            Assert.IsTrue(pill.ClassListContains("card-pill"), "precondition: the actions pill");
            float onePixel = OnePixel(pill);
            for (int i = 0; i < buttons.Length; i++)
            {
                string which = i == 0 ? "Previous" : "Next";
                Assert.IsTrue(buttons[i].ClassListContains("card-pill"), which + " wears the shared pill class");
                Assert.AreEqual(pill.worldBound.height, sizes[i].height, onePixel + 0.01f, which + ": the pill's height");
                Assert.AreEqual(pill.resolvedStyle.borderTopLeftRadius, sizes[i].borderTopLeftRadius, 0.01f, which + ": the pill's corner radius");
                Assert.AreEqual(sizes[i].height / 2f, sizes[i].borderTopLeftRadius, onePixel + 0.01f, which + ": a half-height radius (round ends)");
                Assert.GreaterOrEqual(sizes[i].width, sizes[i].height, which + ": never narrower than tall (no egg)");
            }
        }

        // ---------------- compare_points ----------------

        [UnityTest]
        public IEnumerator ComparePoints_BothRingsAreEachPointsMarkerRing_ThisPointFirst_NamedByTheirCardTitles()
        {
            ComparePointsBlockView compare = null;
            yield return ShowBlock("compare_points_rings_short", v => compare = (ComparePointsBlockView)v);
            var entry = CardGalleryDefinitions.All[IndexOf("compare_points_rings_short")];
            var wall = CardGalleryDefinitions.Taxonomy();
            entry.WallSetup(wall);
            var look = MarkerVisualSettings.Resolve(wall, null);
            var pois = new[] { CardGalleryDefinitions.Poi(entry), wall.pois.Single(p => p.id == CardGalleryDefinitions.CompareOtherId) };
            Assert.AreEqual("Side by side", _harness.Sheet.Stack.HeadingOf(compare).text, "no heading written: the kind's default heading from the card strings (6C)");
            for (int i = 0; i < 2; i++)
            {
                var side = compare.Sides[i];
                var marker = MarkerVisualResolver.Resolve(pois[i], look);
                Assert.AreEqual(pois[i].id, side.PoiId, i == 0 ? "this point first" : "the other second");
                Assert.AreEqual(marker.RingLevel.RingColor, side.Ring.resolvedStyle.unityBackgroundImageTintColor, "the very ring colour its marker gets");
                Assert.AreEqual(CardStatusRule.Resolve(pois[i], look, wall).LineStyle, marker.RingLevel.RingSpriteKey, "precondition: the rule's picture is the marker's");
            }
            Assert.AreEqual("RingSolid", CardTestInput.BackgroundPictureName(compare.Sides[0].Ring), "intact: solid");
            Assert.AreEqual("RingDashLong", CardTestInput.BackgroundPictureName(compare.Sides[1].Ring), "partial damage: its dash");
            Assert.AreNotEqual(compare.Sides[0].Ring.resolvedStyle.unityBackgroundImageTintColor, compare.Sides[1].Ring.resolvedStyle.unityBackgroundImageTintColor,
                "per type: two conditions, two colours");
            CollectionAssert.AreEqual(new[] { "Gate", "Old Cathedral" }, compare.Sides.Select(s => s.Title.text), "each point's card title (the other has no header: its name)");
            CollectionAssert.AreEqual(new[] { "Intact", "Partial Damage" }, compare.Sides.Select(s => s.Level.text), "the Outline Types rows' names, never keys");
            Assert.AreEqual(compare.Sides[0].Ring.worldBound.center.y, compare.Sides[1].Ring.worldBound.center.y, 0.5f, "side by side");
            Assert.Less(compare.Sides[0].Ring.worldBound.xMax, compare.Sides[1].Ring.worldBound.xMin);
            Assert.AreEqual(compare.Sides[0].Box.worldBound.center.x, compare.Sides[0].Ring.worldBound.center.x, 0.5f, "each ring centred in its half");

            yield return ShowBlock("compare_points_rings_long", v => compare = (ComparePointsBlockView)v);
            Assert.AreEqual("The Royal Palace of the Kings by the river, before the earthquake", compare.Sides[1].Title.text, "the other point's own header title");

            yield return ShowBlock("compare_points_rings_unknown", v => compare = (ComparePointsBlockView)v);
            Assert.IsTrue(CardTestInput.IsShown(compare.Sides[1].UnknownMark, compare.Root), "the other nobody assessed: its question mark");
            Assert.IsFalse(CardTestInput.IsShown(compare.Sides[0].UnknownMark, compare.Root));
            Assert.AreEqual("Unknown", compare.Sides[1].Level.text);
        }

        private static string StateKeys(CardGalleryHarness harness) => string.Join("\n", harness.StateStore.Keys);

        [UnityTest]
        public IEnumerator Dialogue_ARealTapRevealsOneLineAtATime_ChoicesAnswerInTheThread_TheReachedLineIsNeverStored()
        {
            DialogueBlockView dialogue = null;
            yield return ShowBlock("dialogue_choices_short", v => dialogue = (DialogueBlockView)v);
            Assert.AreEqual(3, dialogue.Count);
            Assert.AreEqual(1, dialogue.Bubbles.Count, "the first line is said when the card opens");
            Assert.AreEqual("The mason", dialogue.Bubbles[0].Speaker.text);
            Assert.AreEqual("Welcome. Mind the dust: we are mending the arch.", dialogue.Bubbles[0].Text.text);
            Assert.IsTrue(CardTestInput.IsShown(dialogue.Continue, dialogue.Root));
            Assert.AreEqual("Continue", dialogue.Continue.Q<Label>().text);
            Assert.IsFalse(CardTestInput.IsShown(dialogue.Again, dialogue.Root), "nothing to start again yet");
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(dialogue.Continue.worldBound.width, dialogue.Continue.worldBound.height), "Continue is a tap target >= 44 px");

            yield return ScrollAndTap(dialogue.Continue);
            Assert.AreEqual(2, dialogue.Bubbles.Count, "one tap, one more line");
            Assert.AreEqual("The stone comes from the quarry across the river.", dialogue.Bubbles[1].Text.text);
            yield return ScrollAndTap(dialogue.Continue);
            Assert.AreEqual(3, dialogue.Bubbles.Count);
            Assert.AreEqual(3, dialogue.Reached);
            Assert.IsFalse(CardTestInput.IsShown(dialogue.Continue, dialogue.Root), "the last line is said: no Continue");
            Assert.IsTrue(CardTestInput.IsShown(dialogue.Again, dialogue.Root), "...but Start again");
            Assert.AreEqual("Start again", dialogue.Again.Q<Label>().text);
            yield return Render("Card_dialogue_ended");

            // - where the visitor got to is VIEW state: nothing of it was stored, and a rebind starts at the first line
            StringAssert.DoesNotContain(CardGalleryDefinitions.QuizBlockKey, StateKeys(_harness), "the reached line is never in CardLocalState");
            yield return ShowBlock("dialogue_choices_short", v => dialogue = (DialogueBlockView)v);
            Assert.AreEqual(1, dialogue.Bubbles.Count, "a rebind starts again");
            yield return ScrollAndTap(dialogue.Continue);
            yield return ScrollAndTap(dialogue.Continue);
            yield return ScrollAndTap(dialogue.Again);
            Assert.AreEqual(1, dialogue.Bubbles.Count, "Start again: back at the first line");
            Assert.AreEqual(1, dialogue.Reached);

            // - the choice look: the first line offers two replies, and Continue gives way to them
            yield return ShowBlock("dialogue_choices_choices", v => dialogue = (DialogueBlockView)v);
            Assert.IsTrue(dialogue.AwaitsChoice);
            Assert.IsFalse(CardTestInput.IsShown(dialogue.Continue, dialogue.Root), "a line with replies waits for one");
            CollectionAssert.AreEqual(new[] { "No, tell me", "Yes, the earthquake" }, dialogue.Choices.Select(c => c.Label.text).ToList());
            foreach (var choice in dialogue.Choices)
                Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(choice.Button.worldBound.width, choice.Button.worldBound.height), "a reply is a tap target >= 44 px");
            yield return ScrollAndTap(dialogue.Choices[0].Button);
            Assert.IsFalse(dialogue.AwaitsChoice);
            Assert.AreEqual(3, dialogue.Bubbles.Count, "the line, the visitor's own reply, the speaker's answer");
            Assert.AreEqual("You", dialogue.Bubbles[1].Speaker.text, "the visitor's name, from the card strings");
            Assert.AreEqual("No, tell me", dialogue.Bubbles[1].Text.text);
            Assert.IsTrue(dialogue.Bubbles[1].Box.ClassListContains("card-dialogue__bubble--visitor"));
            Assert.Greater(dialogue.Bubbles[1].Box.worldBound.xMin, dialogue.Bubbles[0].Box.worldBound.xMin, "the visitor's bubble sits on the other side");
            Assert.AreEqual("The mason", dialogue.Bubbles[2].Speaker.text);
            Assert.AreEqual("The earthquake brought it down in 1755.", dialogue.Bubbles[2].Text.text);
            Assert.IsTrue(CardTestInput.IsShown(dialogue.Continue, dialogue.Root), "the conversation goes on");
            yield return Render("Card_dialogue_reply");

            // - the third line offers three replies; one of them has no answer: the visitor's bubble only
            yield return ScrollAndTap(dialogue.Continue);
            yield return ScrollAndTap(dialogue.Continue);
            Assert.IsTrue(dialogue.AwaitsChoice);
            Assert.AreEqual(3, dialogue.Choices.Count);
            int before = dialogue.Bubbles.Count;
            yield return ScrollAndTap(dialogue.Choices[1].Button);
            Assert.AreEqual(before + 1, dialogue.Bubbles.Count, "a reply with no answer adds only the visitor's line");
            yield return ScrollAndTap(dialogue.Continue);
            Assert.AreEqual(4, dialogue.Reached);
            Assert.IsTrue(CardTestInput.IsShown(dialogue.Again, dialogue.Root));

            // - rows the block leaves out; a line with no speaker; a reply nobody can pick
            yield return ShowBlock("dialogue_choices_partial", v => dialogue = (DialogueBlockView)v);
            Assert.AreEqual(2, dialogue.Count, "the row with no words is left out");
            yield return ScrollAndTap(dialogue.Continue);
            Assert.IsFalse(CardTestInput.IsShown(dialogue.Bubbles[1].Speaker, dialogue.Bubbles[1].Box), "no speaker: no speaker line");
            Assert.AreEqual(1, dialogue.Choices.Count, "the reply with no label is never offered");
            Assert.AreEqual("Go on", dialogue.Choices[0].Label.text);

            // - one line: nothing to continue and nothing to start again
            yield return ShowBlock("dialogue_choices_oneline", v => dialogue = (DialogueBlockView)v);
            Assert.AreEqual(1, dialogue.Bubbles.Count);
            Assert.IsFalse(CardTestInput.IsShown(dialogue.Continue, dialogue.Root) || CardTestInput.IsShown(dialogue.Again, dialogue.Root));
        }
    }
}
