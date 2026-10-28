using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // PreviewFitRule (_3.3 S4a): the story preview's picture gives way so its title is never cut. The numbers are the ones measured on The
    // Ribeira Palace's preview under the Open look (a 406 pt full-bleed frame, a natural picture of 228.38 laid out at 229, room 370): there the
    // answer flipped between "shortened" and "whole" on every layout pass, and UI Toolkit gave up ("Layout update is struggling").
    public class PreviewFitRuleTests
    {
        private const float Natural = 228.38f;
        private const float Room = 370f;

        [Test]
        public void WordsOverTheRoom_ShortenThePictureByWhatTheyOverrun_NeverUnderItsFloor()
        {
            Assert.AreEqual(Natural - 20f, PreviewFitRule.PictureHeight(500f, Room, Room + 20f, Natural, 120f, false), 1e-3f);
            Assert.AreEqual(120f, PreviewFitRule.PictureHeight(500f, Room, Room + 200f, Natural, 120f, false), 1e-3f, "the floor holds");
            Assert.IsNaN(PreviewFitRule.PictureHeight(Room, Room, Room + 20f, Natural, 120f, false), "rows that fit: the picture stays whole");
            Assert.IsNaN(PreviewFitRule.PictureHeight(500f, Room, Room + PreviewFitRule.Tolerance, Natural, 120f, false), "words landing on the room are not over it");
            Assert.IsNaN(PreviewFitRule.PictureHeight(500f, Room, Room + 20f, float.NaN, 120f, false), "no picture to shorten");
        }

        // The measured flip: whole, the words overran by 1 (keep 371) -> shortened to 227.38, laid out at 227; measured shortened (what was
        // taken added back from the laid-out frame) the same words read 370.38 -- 0.38 over, under the half-pixel tolerance -> "whole" again
        [Test]
        public void AShortenedPicture_StaysShortened_WhileItsWordsReadWithinAPixelOfTheRoom()
        {
            float shortened = PreviewFitRule.PictureHeight(446f, Room, 371f, Natural, 120f, false);
            Assert.AreEqual(Natural - 1f, shortened, 1e-3f, "whole: 1 over, the picture gives 1 pt");
            float again = PreviewFitRule.PictureHeight(445.38f, Room, 370.38f, Natural, 120f, true);
            Assert.IsFalse(float.IsNaN(again), "measured shortened, 0.38 over: it stays shortened (was the flip back to whole)");
            Assert.LessOrEqual(UnityEngine.Mathf.Abs(again - shortened), PreviewFitRule.SnapBand, "...within a pixel of the height it has: nothing to rewrite");
            // - words clearly inside the room give the height back, shortened or not
            Assert.IsNaN(PreviewFitRule.PictureHeight(445f, Room, Room - 2f, Natural, 120f, true), "2 pt inside the room: whole again");
        }

        // The band never holds a picture shorter than the words need: a shortened picture whose words fit by exactly the snap is whole again
        [Test]
        public void TheBand_IsOnePixel_AShortenedPictureWhoseWordsFitByMoreIsWholeAgain()
        {
            float edge = Room + PreviewFitRule.Tolerance - PreviewFitRule.SnapBand;
            Assert.IsNaN(PreviewFitRule.PictureHeight(445f, Room, edge, Natural, 120f, true), "at the band's edge: whole");
            float inside = PreviewFitRule.PictureHeight(445f, Room, edge + 0.01f, Natural, 120f, true);
            Assert.IsFalse(float.IsNaN(inside), "just inside it: still shortened");
            Assert.AreEqual(Natural, inside, 1e-3f, "...never taller than its natural height");
        }
    }
}
