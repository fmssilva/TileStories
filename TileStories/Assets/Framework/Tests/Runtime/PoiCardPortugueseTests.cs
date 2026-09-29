using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // _3.1 step 9-pre: visitor text is real Portuguese now, so the card's own font has to DRAW it. On the REAL LivingRoomScene
    // the Lamp's card is opened in Portuguese and every letter outside ASCII that any of its visible texts uses is asked of the
    // font that draws that text -- a letter the font lacks would show as an empty box -- and the blocks a reader meets first
    // (a rich text, the feedback, the poll) are captured to be read item by item.
    public class PoiCardPortugueseTests : SearchSceneFixture
    {
        private PoiCardSheetView Sheet => Card.Sheet;

        private IEnumerator OpenFullInPortuguese(string poiId)
        {
            Session.CardSettings.languages = new List<string> { "pt", "en" };
            SelectionEventBus.Select(poiId);
            yield return CardTestInput.Settle();
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
        }

        [UnityTest]
        public IEnumerator TheLampInPortuguese_EveryAccentedLetterItShows_IsDrawnByTheCardsFont()
        {
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            yield return OpenFullInPortuguese("lamp");
            var texts = Sheet.Root.Query<TextElement>().ToList().Where(t => !string.IsNullOrEmpty(t.text)).ToList();
            Assert.Greater(texts.Count, 100, "precondition: the whole Lamp card is bound and holds many texts");

            var missing = new List<string>();
            var seen = new SortedSet<char>();
            foreach (var element in texts)
                foreach (char c in element.text.Where(ch => ch > 127).Distinct())
                {
                    seen.Add(c);
                    var verdict = FontDraws(element, c);
                    Assert.IsNotNull(verdict, "a font could be resolved for \"" + element.text + "\": font=" + element.resolvedStyle.unityFont + " asset=" + element.resolvedStyle.unityFontDefinition.fontAsset + " def.font=" + element.resolvedStyle.unityFontDefinition.font + " (the check is not vacuous)");
                    if (verdict == false) missing.Add(c + " (U+" + ((int)c).ToString("X4") + ") in \"" + element.text + "\"");
                }
            foreach (char needed in "çãéíóúáàêõÚ")
                Assert.Contains(needed, seen.ToList(), "the Lamp's Portuguese uses " + needed + " somewhere");
            CollectionAssert.IsEmpty(missing, "letters the card's font cannot draw");
            Debug.Log("[Card] Portuguese letters on the Lamp card: " + new string(seen.ToArray()));
        }

        // The rich text, the feedback and the poll in Portuguese, each scrolled to the top of the stack and captured
        [UnityTest]
        public IEnumerator TheLampInPortuguese_RichTextFeedbackAndPoll_AreCapturedToBeRead()
        {
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            yield return OpenFullInPortuguese("lamp");
            foreach (var (view, name) in new (IBlockView, string)[]
            {
                (Sheet.Stack.BoundViews.OfType<RichTextBlockView>().First(), "Lamp_PT_RichText"),
                (Sheet.Stack.BoundViews.OfType<FeedbackBlockView>().First(), "Lamp_PT_Feedback"),
                (Sheet.Stack.BoundViews.OfType<PollBlockView>().First(), "Lamp_PT_Poll"),
            })
            {
                Sheet.Stack.Scroll.ScrollTo(Sheet.Stack.SlotOf(view));
                yield return CardTestInput.Settle(0.2f);
                yield return Capture(name);
            }
        }

        // Whether the font that draws `element` has a glyph for `c`: true / false, or null when no font could be found
        private static bool? FontDraws(TextElement element, char c)
        {
            var definition = element.resolvedStyle.unityFontDefinition;
            if (definition.fontAsset != null) return definition.fontAsset.HasCharacter(c, true);
            if (definition.font != null) return definition.font.HasCharacter(c);
            return null;
        }
    }
}
