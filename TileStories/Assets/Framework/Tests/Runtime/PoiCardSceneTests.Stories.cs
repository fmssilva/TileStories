using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of the stories family on the real LivingRoomScene: story_chapters in Portuguese, compare_points, dialogue.
    public partial class PoiCardSceneTests
    {
        [UnityTest]
        public IEnumerator InPortuguese_TheStoryTurnsOnARealTap_WithThePortugueseCounterAndButtons()
        {
            LiveSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            yield return OpenFull("lamp");
            var story = Sheet.Stack.BoundViews.OfType<StoryChaptersBlockView>().Single();
            Assert.AreEqual("Cap\u00edtulo 1 de 3", story.Counter.text, "the framework's Portuguese counter");
            Assert.AreEqual("O cerco", story.Title.text);
            Assert.AreEqual("Seguinte", story.Next.Q<Label>().text);
            Sheet.Stack.Scroll.ScrollTo(story.Root);
            yield return CardTestInput.Settle(0.2f);

            yield return CardTestInput.Tap(story.Next.panel, story.Next.worldBound.center);
            yield return null;
            Assert.AreEqual("Cap\u00edtulo 2 de 3", story.Counter.text, "a real tap on Seguinte");
            Assert.AreEqual("O pal\u00e1cio", story.Title.text);
            Assert.AreEqual("Anterior", story.Previous.Q<Label>().text);
            yield return Capture("Card_Lamp_Story_pt");

            var person = Sheet.Stack.BoundViews.OfType<PersonBlockView>().Last();
            Assert.AreEqual("D. Manuel I", person.Name.text, "person texts in Portuguese too");
            var info = Sheet.Stack.BoundViews.OfType<PracticalInfoBlockView>().Single();
            Assert.AreEqual("Aberto", info.Rows[0].Label.text);
        }

        // _3.2 step 14 (the story_chapters TODO): Previous / Next stay where they are as the chapters turn -- a visitor tapping Next again and
        // again keeps a finger in one place. Measured on The Lamp's real chapters at 390 px, in English and Portuguese, from the block's own
        // top (the scroll never enters the number); the message lists every chapter's place
        [UnityTest]
        public IEnumerator TheStorysPreviousAndNext_StayInOnePlace_AsARealTapTurnsEveryChapter_InEnglishAndPortuguese()
        {
            foreach (string language in new[] { "en", "pt" })
            {
                SelectionEventBus.Clear();
                LiveSettings.languages = language == "pt" ? new System.Collections.Generic.List<string> { "pt", "en" } : new System.Collections.Generic.List<string> { "en", "pt" };
                yield return OpenFull("lamp");
                var story = Sheet.Stack.BoundViews.OfType<StoryChaptersBlockView>().Single();
                Assert.GreaterOrEqual(story.Count, 3, "precondition: The Lamp's story has three chapters");
                Sheet.Stack.Scroll.ScrollTo(story.Root);
                yield return CardTestInput.Settle(0.3f);
                var places = new System.Collections.Generic.List<float>();
                for (int chapter = 0; chapter < story.Count; chapter++)
                {
                    if (chapter > 0)
                    {
                        if (!Sheet.Stack.Scroll.contentViewport.worldBound.Contains(story.Next.worldBound.center))
                        {
                            Sheet.Stack.Scroll.ScrollTo(story.Next);
                            yield return CardTestInput.Settle(0.2f);
                        }
                        yield return CardTestInput.Tap(story.Next.panel, story.Next.worldBound.center);
                        yield return CardTestInput.Settle(0.1f);
                    }
                    Assert.AreEqual(chapter, story.Index, language + ": a real tap on Next turned to chapter " + (chapter + 1));
                    places.Add(story.Next.worldBound.yMin - story.Root.worldBound.yMin);
                }
                string measured = language + " Next's top from the block's top, chapter by chapter: " + string.Join(", ", places.Select(p => p.ToString("0.0")));
                Debug.Log("[Card] " + measured);
                foreach (float place in places)
                    Assert.AreEqual(places[0], place, 0.5f, measured);
            }
        }

        [UnityTest]
        public IEnumerator TheCompareBlock_WearsTheLampsAndLampMilitarysOwnMarkerRings()
        {
            yield return OpenFull("lamp");
            var compare = Sheet.Stack.BoundViews.OfType<ComparePointsBlockView>().Single();
            Assert.AreEqual("lamp", compare.Sides[0].PoiId);
            Assert.AreEqual("lamp_military", compare.Sides[1].PoiId);
            for (int i = 0; i < 2; i++)
            {
                var marker = MarkerRing(compare.Sides[i].PoiId);
                Assert.IsTrue(marker.Shown, "precondition: the marker draws its ring");
                Assert.AreEqual(marker.Color, compare.Sides[i].Ring.resolvedStyle.unityBackgroundImageTintColor, compare.Sides[i].PoiId + ": the running marker's ring colour");
                Assert.AreEqual(marker.Picture, CardTestInput.BackgroundPictureName(compare.Sides[i].Ring), compare.Sides[i].PoiId + ": and its picture");
            }
            CollectionAssert.AreEqual(new[] { "St George's Castle", "Castle Keep" }, compare.Sides.Select(s => s.Title.text), "each named by its card's title");
            CollectionAssert.AreEqual(new[] { "Intact", "Partial Damage" }, compare.Sides.Select(s => s.Level.text));
            Sheet.Stack.Scroll.ScrollTo(compare.Root);
            yield return CardTestInput.Settle(0.2f);
            yield return Capture("Card_Lamp_Compare");

            // - the other point loses its status (a live marker push): no half pair on the card
            var copy = ConfigCopy();
            copy.pois.First(p => p.id == "lamp_military").has_status = false;
            Session.ApplyMarkerSettings(copy);
            yield return null;
            SelectionEventBus.Clear();
            yield return OpenFull("lamp");
            Assert.AreEqual(0, Sheet.Stack.BoundViews.OfType<ComparePointsBlockView>().Count(), "Lamp - Military without a status: the compare block is not shown");
            Assert.AreEqual(1, Sheet.Stack.BoundViews.OfType<SwatchesBlockView>().Count(), "the rest of the card still is");
        }

        // _3.1 step 8B: The Lamp's dialogue: real taps reveal one line at a time, the reply the visitor picks joins the thread, and
        // where they got to is never stored; a Portuguese card says the lines in Portuguese
        [UnityTest]
        public IEnumerator TheLamp_Dialogue_ARealTapRevealsOneLineAtATime_ARepliesJoinTheThread_NothingIsStored_AndInPortugueseTheWordsFollow()
        {
            yield return OpenFull("lamp");
            var dialogue = Only<DialogueBlockView>();
            Assert.AreEqual(3, dialogue.Count);
            Assert.AreEqual(1, dialogue.Bubbles.Count);
            Assert.AreEqual("The stonemason", dialogue.Bubbles[0].Speaker.text);
            Assert.AreEqual("Welcome to the castle. Do you know why its walls are so thick?", dialogue.Bubbles[0].Text.text);
            Assert.IsTrue(dialogue.AwaitsChoice, "the first line offers two replies");
            yield return ScrollAndTap(dialogue.Choices[0].Button);
            Assert.AreEqual(3, dialogue.Bubbles.Count, "the line, the visitor's reply and the stonemason's answer");
            Assert.AreEqual("You", dialogue.Bubbles[1].Speaker.text);
            Assert.AreEqual("No, tell me", dialogue.Bubbles[1].Text.text);
            Assert.AreEqual("Because it had to hold out against a siege for weeks.", dialogue.Bubbles[2].Text.text);
            yield return ScrollAndTap(dialogue.Continue);
            yield return ScrollAndTap(dialogue.Continue);
            Assert.AreEqual(5, dialogue.Bubbles.Count);
            Assert.AreEqual(3, dialogue.Reached);
            Assert.IsTrue(CardTestInput.IsShown(dialogue.Again, dialogue.Root));
            yield return ScrollTo(dialogue);
            yield return Capture("Card_Lamp_Dialogue");
            Assert.IsFalse(_cardStore.Keys.Any(k => k.Contains(".block_50.")), "the reached line and the picked reply are view state: nothing stored");
            Assert.AreEqual(0, _cardEvents.Raised.Count, "and nothing reported");

            SelectionEventBus.Clear();
            yield return OpenFull("lamp");
            Assert.AreEqual(1, Only<DialogueBlockView>().Bubbles.Count, "a new bind starts at the first line");

            SelectionEventBus.Clear();
            LiveSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            yield return OpenFull("lamp");
            dialogue = Only<DialogueBlockView>();
            Assert.AreEqual("O pedreiro", dialogue.Bubbles[0].Speaker.text);
            Assert.AreEqual("N\u00e3o, conte-me", dialogue.Choices[0].Label.text);
            yield return ScrollAndTap(dialogue.Choices[1].Button);
            Assert.AreEqual("Voc\u00ea", dialogue.Bubbles[1].Speaker.text, "the visitor's own name in Portuguese");
            Assert.AreEqual("Então tem bom olho.", dialogue.Bubbles[2].Text.text);
            Assert.AreEqual("Continuar", dialogue.Continue.Q<Label>().text);
        }
    }
}
