using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of the media family on the real LivingRoomScene: hotspot_image, the gallery's full-screen view, before_after, a default picture.
    public partial class PoiCardSceneTests
    {
        // _3.1 step 7B: The Lamp's two hotspot_image blocks on the real wall: the picture from the wall's media folder, each
        // spot on its point of the picture, a real tap opens its authored text (a second closes it), a loupe does the same
        [UnityTest]
        public IEnumerator TheLamp_HotspotImage_ARealTapOnASpotOpensItsAuthoredText_ALoupeToo_AndInPortuguese()
        {
            yield return OpenFull("lamp");
            var hotspots = Sheet.Stack.BoundViews.OfType<HotspotImageBlockView>().ToList();
            Assert.AreEqual(2, hotspots.Count, "numbered and loupes");
            var numbered = hotspots[0];
            Assert.AreEqual("castle_then", numbered.Image.Texture?.name, "the picture from the wall's own media folder (the historic panel, its own file: the header counts castle_hero alone)");
            Assert.AreEqual(4, numbered.Spots.Count, "the four authored spots");
            Assert.AreEqual("Explore the panel", Sheet.Stack.HeadingOf(numbered).text, "its authored heading");
            Sheet.Stack.Scroll.ScrollTo(numbered.Root);
            yield return CardTestInput.Settle(0.2f);
            Rect picture = numbered.Image.Root.worldBound;
            Assert.AreEqual(picture.x + 0.5f * picture.width, numbered.Spots[1].Pin.worldBound.center.x, 1f, "the keep's spot: half across the picture");
            Assert.AreEqual(picture.y + 0.3f * picture.height, numbered.Spots[1].Pin.worldBound.center.y, 1f, "...and 0.3 down it");

            yield return CardTestInput.Tap(numbered.Root.panel, numbered.Spots[1].Pin.worldBound.center);
            yield return CardTestInput.Settle(0.1f);
            Assert.AreEqual(1, numbered.OpenIndex, "a real tap on spot 2");
            Assert.AreEqual("The keep", numbered.DetailTitle.text);
            StringAssert.Contains("the last refuge of the castle", numbered.DetailText.Paragraphs[0].text);
            Assert.IsTrue(Sheet.IsOpen, "a tap on a spot never closes the card");
            yield return Capture("Card_Lamp_Hotspot_Numbered");
            yield return CardTestInput.Tap(numbered.Root.panel, numbered.Spots[1].Pin.worldBound.center);
            yield return CardTestInput.Settle(0.1f);
            Assert.AreEqual(-1, numbered.OpenIndex, "a second tap closes it");

            var loupes = hotspots[1];
            Sheet.Stack.Scroll.ScrollTo(loupes.Root);
            yield return CardTestInput.Settle(0.2f);
            Assert.AreEqual(4, loupes.Loupes.childCount, "a close-up per spot");
            yield return CardTestInput.Tap(loupes.Root.panel, loupes.Spots[3].Loupe.worldBound.center);
            yield return CardTestInput.Settle(0.1f);
            Assert.AreEqual("The curtain wall", loupes.DetailTitle.text, "a real tap on the fourth close-up");
            Assert.AreEqual(0, loupes.DetailText.Paragraphs.Count, "a spot with no text: its title alone");
            yield return Capture("Card_Lamp_Hotspot_Loupes");

            LiveSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            SelectionEventBus.Clear();
            yield return OpenFull("lamp");
            numbered = Sheet.Stack.BoundViews.OfType<HotspotImageBlockView>().First();
            Assert.AreEqual("Toque num ponto para saber mais", numbered.Hint.text, "the framework's Portuguese hint");
            Sheet.Stack.Scroll.ScrollTo(numbered.Root);
            yield return CardTestInput.Settle(0.2f);
            yield return CardTestInput.Tap(numbered.Root.panel, numbered.Spots[1].Pin.worldBound.center);
            yield return CardTestInput.Settle(0.1f);
            Assert.AreEqual("A torre de menagem", numbered.DetailTitle.text, "the wall's Portuguese title");
        }

        // _3.1 step 13: the Lamp - Military zoom_image block picks a Framework default picture by key (no wall media
        // authored for it) and the real card loads it exactly like an authored one
        [UnityTest]
        public IEnumerator LampMilitary_ZoomImage_PicksAFrameworkDefaultPicture_AndLoadsItLikeAnyOther()
        {
            yield return OpenFull("lamp_military");
            var zoom = Sheet.Stack.BoundViews.OfType<ZoomImageBlockView>().Single();
            Assert.IsNotNull(zoom.Image.Texture, "the default:azulejo_detail key resolved to a real texture");
            StringAssert.Contains("framework's own default media", zoom.Caption.text);

            SelectionEventBus.Clear();
            LiveSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            yield return OpenFull("lamp_military");
            var zoomPt = Sheet.Stack.BoundViews.OfType<ZoomImageBlockView>().Single();
            StringAssert.Contains("própria desta parede", zoomPt.Caption.text, "the caption's own Portuguese, real accents");
        }

        [UnityTest]
        public IEnumerator TheLamp_ARealTapOnAGalleryPicture_OpensItFullScreen_BackKeepsTheScroll_AndClosingGivesEverythingBack()
        {
            yield return OpenFull("lamp");
            var grid = Sheet.Stack.BoundViews.OfType<GalleryBlockView>().Single(g => g.Root.ClassListContains("card-gallery--grid"));
            yield return ScrollTo(grid);
            float scroll = Sheet.Stack.Scroll.scrollOffset.y;
            Assert.Greater(scroll, 500f, "precondition: the long card is scrolled far down to its grid");
            int before = Card.Media.RefCount("gallery_2.png");
            Assert.Greater(before, 0, "the gallery looks hold gallery_2.png");
            foreach (var g in Sheet.Stack.BoundViews.OfType<GalleryBlockView>())
                foreach (var shot in g.Shots) Assert.IsNotNull(shot.Image.Texture, "every gallery picture of the fixture loaded (" + shot.Image.Path + ")");

            var target = grid.Shots[1].Box;
            yield return CardTestInput.Tap(target.panel, target.worldBound.center);
            yield return CardTestInput.Settle(0.2f);
            var takeover = Sheet.Takeover;
            Assert.IsTrue(takeover.IsOpen, "a real tap on the second picture opens it full screen");
            Assert.AreEqual(1, takeover.PageIndex);
            Assert.AreEqual("St George's Castle > Gallery", takeover.Crumb.text, "the card's title > the gallery");
            Assert.AreEqual(before + 1, Card.Media.RefCount("gallery_2.png"), "the full-screen view loaded its own copy");
            Assert.IsTrue(ScreenUIHit.IsOverAnything(ScreenPointOf(takeover.Page)), "a tap on the full-screen view is on the UI: never 'empty space'");
            Assert.AreEqual("lamp", SelectionEventBus.CurrentPoiId, "the selection stays");
            yield return Capture("Card_Lamp_Lightbox");

            yield return CardTestInput.Tap(takeover.Chips[3].panel, takeover.Chips[3].worldBound.center);
            yield return CardTestInput.Settle(0.15f);
            Assert.AreEqual(3, takeover.PageIndex, "a real tap on chip 4");
            Assert.AreEqual(before, Card.Media.RefCount("gallery_2.png"), "the page before gave its picture back");

            yield return CardTestInput.Tap(takeover.Back.panel, takeover.Back.worldBound.center);
            yield return CardTestInput.Settle(0.2f);
            Assert.IsFalse(takeover.IsOpen, "Back");
            Assert.AreEqual(0, takeover.HeldMedia);
            Assert.AreEqual(scroll, Sheet.Stack.Scroll.scrollOffset.y, 0.5f, "the card is where the visitor left it");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop);

            Press(Sheet.CloseButton);
            yield return null;
            Assert.AreEqual(0, Card.Media.LoadedCount, "closed: every picture of the card given back");
        }

        [UnityTest]
        public IEnumerator TheLamp_BeforeAfter_ARealDragOnTheRealCard_MovesTheCut_AndTheCardDoesNotScroll()
        {
            yield return OpenFull("lamp");
            var slider = Sheet.Stack.BoundViews.OfType<BeforeAfterBlockView>().Single();
            yield return ScrollTo(slider);
            Assert.AreEqual("Before 1755", slider.BeforeLabel.text);
            Assert.AreEqual("After the earthquake", slider.AfterLabel.text);
            Assert.AreEqual("damage_before", slider.Before.Texture.name);
            Assert.AreEqual("damage_after", slider.After.Texture.name);
            float scroll = Sheet.Stack.Scroll.scrollOffset.y;
            Rect frame = slider.Frame.worldBound;
            yield return CardTestInput.DragFrom(slider.Frame.panel, frame.center, new Vector2(-frame.width * 0.35f, 0f));
            yield return null;
            Assert.AreEqual(0.15f, slider.Position, 0.02f, "dragged from the middle to 15%");
            Assert.AreEqual(frame.width * slider.Position, slider.BeforeClip.worldBound.width, 1f);
            Assert.AreEqual(scroll, Sheet.Stack.Scroll.scrollOffset.y, 0.01f, "the card did not scroll under the drag");
            yield return Capture("Card_Lamp_BeforeAfter");
        }
    }
}
