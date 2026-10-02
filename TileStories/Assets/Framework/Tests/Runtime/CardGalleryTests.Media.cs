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
    // Phase A promises of the media family: gallery and its lightbox, hotspot_image, before_after, zoom_image.
    public partial class CardGalleryTests
    {
        [UnityTest]
        public IEnumerator Gallery_EachLookLaysOutItsPictures_ARowOutsideTheFolderIsLeftOut_AMissingFileShowsUnavailable()
        {
            GalleryBlockView gallery = null;
            yield return ShowBlock("gallery_carousel_long", v => gallery = (GalleryBlockView)v);
            Assert.AreEqual(4, gallery.Shots.Count);
            Assert.AreEqual(gallery.Track, gallery.Shots[0].Box.GetFirstAncestorOfType<ScrollView>(), "carousel: on the swipe track");
            Assert.AreEqual(gallery.Shots[0].Box.worldBound.yMin, gallery.Shots[1].Box.worldBound.yMin, 0.5f, "side by side");
            Assert.AreEqual(4, gallery.Dots.childCount, "a dot per picture");
            Assert.IsTrue(gallery.Dots[0].ClassListContains("card-gallery__dot--current"));
            gallery.Track.scrollOffset = new Vector2(gallery.Shots[2].Box.layout.x, 0f);
            yield return CardTestInput.Settle(0.15f);
            Assert.AreEqual(2, gallery.Current, "scrolled to the third picture: its dot is marked");
            Assert.IsTrue(gallery.Dots[2].ClassListContains("card-gallery__dot--current") && !gallery.Dots[0].ClassListContains("card-gallery__dot--current"));

            yield return ShowBlock("gallery_grid_long", v => gallery = (GalleryBlockView)v);
            Rect a = gallery.Shots[0].Box.worldBound, b = gallery.Shots[1].Box.worldBound, c = gallery.Shots[2].Box.worldBound;
            Assert.AreEqual(a.yMin, b.yMin, 0.5f, "grid: two per row");
            Assert.Greater(c.yMin, a.yMax - 0.5f, "the third on the next row");
            Assert.AreEqual(a.width, b.width, 0.5f, "equal cells");

            yield return ShowBlock("gallery_filmstrip_long", v => gallery = (GalleryBlockView)v);
            Assert.AreEqual("one.png", gallery.Main.Path, "filmstrip: the first picture large");
            Assert.Greater(gallery.Main.Root.worldBound.height, gallery.Shots[0].Box.worldBound.height * 2f, "...much larger than the strip");
            yield return CardTestInput.Tap(gallery.Root.panel, gallery.Shots[2].Box.worldBound.center);
            yield return null;
            Assert.AreEqual(2, gallery.Current, "a real tap on a small picture...");
            Assert.AreEqual("three.png", gallery.Main.Path, "...shows it large");
            Assert.IsFalse(_harness.Sheet.Takeover.IsOpen, "(the filmstrip's small pictures pick, they do not open the lightbox)");
            Assert.IsTrue(gallery.Shots[2].Box.ClassListContains("card-gallery__thumb--current") && !gallery.Shots[0].Box.ClassListContains("card-gallery__thumb--current"));
            Assert.AreEqual(1, _harness.Media.RefCount("one.png"), "the old large picture was given back at once (only its small one holds it)");
            Assert.AreEqual(2, _harness.Media.RefCount("three.png"), "small + large");

            yield return ShowBlock("gallery_stack_long", v => gallery = (GalleryBlockView)v);
            Assert.AreEqual(3, gallery.Shots.Count, "stack: three prints of the four");
            Assert.AreEqual("4 pictures", gallery.Count.text, "and how many there are (CardStrings)");
            Assert.AreEqual(0, gallery.Shots.Last().Page, "the first picture drawn last: on top");
            Assert.AreNotEqual(gallery.Shots[0].Box.worldBound, gallery.Shots.Last().Box.worldBound, "the prints are fanned, not stacked exactly");

            yield return ShowBlock("gallery_grid_partial", v => gallery = (GalleryBlockView)v);
            Assert.AreEqual(2, gallery.PictureCount, "outside the folder and not-a-picture rows left out; the kept one and the missing file stay");
            Assert.IsNotNull(gallery.Shots[0].Image.Texture);
            var ghost = gallery.Shots[1].Image;
            Assert.IsNull(ghost.Texture, "no such file: nothing loaded, no exception");
            Assert.IsTrue(ghost.Root.ClassListContains("card-image--unavailable"));
            Assert.AreEqual("Picture unavailable", ghost.Unavailable.text);
            Assert.IsTrue(CardTestInput.IsShown(ghost.Unavailable, gallery.Root), "the frame says so");
        }

        // _3.1 [7B]: the lightbox turns by a real sideways drag, zooms like zoom_image, and an enlarged picture moves
        // instead of turning. (Two real fingers on the real scene: PoiCardTapZoomTests.)
        [UnityTest]
        public IEnumerator Lightbox_ARealSwipeTurnsThePage_AZoomedPictureMovesInstead_AndNothingTurnsPastTheEnds()
        {
            GalleryBlockView gallery = null;
            yield return ShowBlock("gallery_grid_long", v => gallery = (GalleryBlockView)v);
            var takeover = _harness.Sheet.Takeover;
            yield return CardTestInput.Tap(gallery.Root.panel, gallery.Shots[1].Box.worldBound.center);
            yield return CardTestInput.Settle(0.15f);
            Assert.AreEqual(1, takeover.PageIndex, "precondition: the lightbox at picture 2");
            var panel = takeover.Page.panel;
            VisualElement Frame() => takeover.Page.Q("card-gallery-full");
            float travel = Frame().worldBound.width * (SwipePageRule.MinTravelShare + 0.15f);

            yield return CardTestInput.DragFrom(panel, Frame().worldBound.center, new Vector2(-travel, 0f));
            yield return CardTestInput.Settle(0.15f);
            Assert.AreEqual(2, takeover.PageIndex, "a real drag to the left: the next picture");
            Assert.AreEqual(2, takeover.Chips.Children().ToList().FindIndex(c => c.ClassListContains("card-takeover__chip--current")), "its chip is marked");
            var picture = takeover.Page.Q(className: "card-image");
            yield return PixelAt(picture, picture.worldBound.center, c => AssertColour(PictureColour("three.png"), c, "the page shows picture 3"));
            Assert.AreEqual(1, _harness.Media.RefCount("two.png"), "the page it left gave its picture back");

            yield return CardTestInput.DragFrom(panel, Frame().worldBound.center, new Vector2(travel, 0f));
            yield return CardTestInput.Settle(0.15f);
            Assert.AreEqual(1, takeover.PageIndex, "a real drag to the right: back one picture");

            yield return CardTestInput.DragFrom(panel, Frame().worldBound.center, new Vector2(-travel * 0.3f, 0f));
            yield return CardTestInput.Settle(0.15f);
            Assert.AreEqual(1, takeover.PageIndex, "a short drag is not a swipe");
            yield return CardTestInput.DragFrom(panel, Frame().worldBound.center, new Vector2(-travel * 0.4f, travel));
            yield return CardTestInput.Settle(0.15f);
            Assert.AreEqual(1, takeover.PageIndex, "a mostly vertical drag is not a swipe");

            // - the ends: nothing before the first picture
            yield return CardTestInput.DragFrom(panel, Frame().worldBound.center, new Vector2(travel, 0f));
            yield return CardTestInput.Settle(0.15f);
            Assert.AreEqual(0, takeover.PageIndex);
            yield return CardTestInput.DragFrom(panel, Frame().worldBound.center, new Vector2(travel, 0f));
            yield return CardTestInput.Settle(0.15f);
            Assert.AreEqual(0, takeover.PageIndex, "no page before the first");
            Assert.IsTrue(takeover.IsOpen, "...and the lightbox stays open");

            // - zoom like zoom_image (Ctrl + wheel = the desktop pinch), then the same sideways drag MOVES the picture
            Rect frame = Frame().worldBound;
            yield return CardTestInput.CtrlWheel(panel, frame.center, -24f);
            yield return CardTestInput.Settle(0.1f);
            var zoomed = takeover.Page.Q(className: "card-zoompan__image");
            Assert.Greater(zoomed.worldBound.width, frame.width * 1.5f, "Ctrl + wheel enlarged the lightbox picture");
            float x0 = zoomed.worldBound.x;
            yield return CardTestInput.DragFrom(panel, frame.center, new Vector2(-travel, 0f));
            yield return CardTestInput.Settle(0.15f);
            Assert.AreEqual(0, takeover.PageIndex, "an enlarged picture never turns the page...");
            Assert.Less(takeover.Page.Q(className: "card-zoompan__image").worldBound.x, x0 - 10f, "...the drag moved it instead");
            Assert.LessOrEqual(zoomed.worldBound.xMin, frame.xMin + 0.5f, "never an empty edge");
            Assert.GreaterOrEqual(zoomed.worldBound.xMax, frame.xMax - 0.5f);
            yield return Render("Card_gallery_lightbox_zoomed");

            // - a chip goes back to 1x on its page (a new page is always whole)
            yield return CardTestInput.Tap(takeover.Chips[3].panel, takeover.Chips[3].worldBound.center);
            yield return CardTestInput.Settle(0.15f);
            Assert.AreEqual(3, takeover.PageIndex);
            Assert.LessOrEqual(takeover.Page.Q(className: "card-zoompan__image").worldBound.width, Frame().worldBound.width + 0.5f, "the new page shows its picture whole");
            takeover.Close();
        }

        // ---------------- hotspot_image (Tier 2 group B) ----------------

        // Where a spot's point is on the screen: x / y of the PICTURE's rectangle (never the frame's)
        private static Vector2 SpotPoint(HotspotImageBlockView view, int i)
        {
            Rect picture = view.Image.Root.worldBound;
            return new Vector2(picture.x + view.Spots[i].At.x * picture.width, picture.y + view.Spots[i].At.y * picture.height);
        }

        [UnityTest]
        public IEnumerator HotspotImage_Numbered_EverySpotSitsOnItsPointOfThePicture_ARealTapOpensItsText_ASecondClosesIt()
        {
            HotspotImageBlockView view = null;
            yield return ShowBlock("hotspot_image_numbered_long", v => view = (HotspotImageBlockView)v);
            Assert.AreEqual(5, view.Spots.Count, "a spot per row");
            Rect frame = view.Frame.worldBound, picture = view.Image.Root.worldBound;
            Assert.Less(picture.width, frame.width - 10f, "precondition: a portrait picture, narrower than its frame (whole, centred)");
            Assert.AreEqual(frame.center.x, picture.center.x, 1f, "the picture is centred in the frame");
            for (int i = 0; i < view.Spots.Count; i++)
            {
                Assert.AreEqual((i + 1).ToString(), view.Spots[i].Number.text, "numbered in row order, on the picture");
                var pin = view.Spots[i].Pin.worldBound;
                Assert.AreEqual(SpotPoint(view, i).x, pin.center.x, 1f, "spot " + (i + 1) + " sits on its point of the picture (across)");
                Assert.AreEqual(SpotPoint(view, i).y, pin.center.y, 1f, "...and down");
                Assert.IsTrue(view.Spots[i].Pin.ClassListContains("card-tap"), "a numbered spot is the tap target");
            }
            yield return PixelAt(view.Frame, view.Image.Root.worldBound.center + new Vector2(0f, 40f), c => AssertColour(PictureColour("three.png"), c, "the frame shows the picture"));
            Assert.IsFalse(CardTestInput.IsShown(view.Detail, view.Root), "no text is open before a tap");
            Assert.AreEqual("Tap a spot to read about it", view.Hint.text, "the hint from the card's texts");

            yield return CardTestInput.Tap(view.Root.panel, view.Spots[1].Pin.worldBound.center);
            yield return CardTestInput.Settle(0.1f);
            Assert.AreEqual(1, view.OpenIndex, "a real tap on spot 2 opened it");
            Assert.IsTrue(CardTestInput.IsShown(view.Detail, view.Root), "its text shows under the picture");
            Assert.Greater(view.Detail.worldBound.yMin, frame.yMax - 0.5f, "...under it, not over it");
            Assert.AreEqual("2", view.DetailNumber.text);
            Assert.AreEqual("The bell tower", view.DetailTitle.text);
            StringAssert.Contains("Rebuilt after the earthquake, taller than before.", view.DetailText.Paragraphs[0].text);
            Assert.IsTrue(view.Spots[1].Pin.ClassListContains("card-hotspot__pin--open"), "the open spot is marked");
            yield return Render("Card_hotspot_image_numbered_open");

            yield return CardTestInput.Tap(view.Root.panel, view.Spots[0].Pin.worldBound.center);
            yield return CardTestInput.Settle(0.1f);
            Assert.AreEqual(0, view.OpenIndex, "another spot: its text replaces the first");
            Assert.AreEqual(2, view.DetailText.Paragraphs.Count, "its two paragraphs");
            Assert.IsFalse(view.Spots[1].Pin.ClassListContains("card-hotspot__pin--open"));

            yield return CardTestInput.Tap(view.Root.panel, view.Spots[0].Pin.worldBound.center);
            yield return CardTestInput.Settle(0.1f);
            Assert.AreEqual(-1, view.OpenIndex, "a second tap on the open spot closes it");
            Assert.IsFalse(CardTestInput.IsShown(view.Detail, view.Root));
        }

        [UnityTest]
        public IEnumerator HotspotImage_ARowWithNoTitleIsLeftOut_TheCountHasNoGap_AndCornerSpotsSitOnTheCorners()
        {
            HotspotImageBlockView view = null;
            yield return ShowBlock("hotspot_image_numbered_partial", v => view = (HotspotImageBlockView)v);
            Assert.AreEqual(2, view.Spots.Count, "the row with no title is not shown");
            Assert.AreEqual("2", view.Spots[1].Number.text, "...and the next one is still 2");
            Assert.AreEqual("Bottom right corner", view.Spots[1].Title);
            Rect picture = view.Image.Root.worldBound;
            Assert.AreEqual(new Vector2(picture.xMin, picture.yMin), view.Spots[0].Pin.worldBound.center, "0 / 0: the picture's top-left corner");
            Assert.AreEqual(picture.xMax, view.Spots[1].Pin.worldBound.center.x, 1f, "1 / 1: its bottom-right corner");
            Assert.AreEqual(picture.yMax, view.Spots[1].Pin.worldBound.center.y, 1f);
            // - the frame leaves room for half a circle round the picture: a corner spot is never cut in half
            Rect frame = view.Frame.worldBound;
            foreach (var corner in new[] { view.Spots[0].Number.worldBound, view.Spots[1].Number.worldBound })
                Assert.IsTrue(corner.xMin >= frame.xMin - 0.5f && corner.yMin >= frame.yMin - 0.5f && corner.xMax <= frame.xMax + 0.5f && corner.yMax <= frame.yMax + 0.5f,
                    "a corner spot's circle " + corner + " lies whole inside the frame " + frame);

            yield return ShowBlock("hotspot_image_numbered_missing", v => view = (HotspotImageBlockView)v);
            Assert.IsTrue(view.Image.Root.ClassListContains("card-image--unavailable"), "a missing picture says so");
            Assert.AreEqual(2, view.Spots.Count, "...and its spots still open their texts");
            yield return CardTestInput.Tap(view.Root.panel, view.Spots[0].Pin.worldBound.center);
            yield return CardTestInput.Settle(0.1f);
            Assert.AreEqual(0, view.OpenIndex);
        }

        [UnityTest]
        public IEnumerator HotspotImage_Loupes_EachCloseUpShowsItsSpotEnlarged_ARealTapOnOneOpensItsText()
        {
            HotspotImageBlockView view = null;
            yield return ShowBlock("hotspot_image_loupes_long", v => view = (HotspotImageBlockView)v);
            Assert.IsTrue(CardTestInput.IsShown(view.Loupes, view.Root), "a row of close-ups under the picture");
            Assert.AreEqual(5, view.Loupes.childCount, "one per spot");
            for (int i = 0; i < view.Spots.Count; i++)
            {
                var spot = view.Spots[i];
                Assert.IsFalse(spot.Pin.ClassListContains("card-tap"), "loupes: the mark on the picture is not the tap target...");
                Assert.IsTrue(spot.Loupe.ClassListContains("card-tap"), "...the close-up is");
                Assert.AreEqual(SpotPoint(view, i).x, spot.Pin.worldBound.center.x, 1f, "the small ring sits on the spot");
                // - the close-up is placed inside the loupe's border
                Rect box = spot.Loupe.LocalToWorld(spot.Loupe.contentRect);
                var crop = SpotlightCropRule.Place(box.size, spot.LoupeImage.Aspect, spot.At, HotspotImageBlockView.LoupeZoom);
                Assert.AreEqual(crop.Size.x, spot.LoupeImage.Root.worldBound.width, 1f, "loupe " + (i + 1) + ": the picture enlarged " + HotspotImageBlockView.LoupeZoom + " times");
                Assert.AreEqual(box.x + crop.Focus.x, spot.LoupeRing.worldBound.center.x, 1f, "...and its ring on the spot inside the close-up");
                Assert.AreEqual(box.y + crop.Focus.y, spot.LoupeRing.worldBound.center.y, 1f);
                Assert.IsTrue(box.Contains(spot.LoupeRing.worldBound.center), "the spot is inside its close-up");
            }
            // - a spot away from the edges sits at the centre of its close-up (the middle spot: 0.5 / 0.5)
            Assert.AreEqual(view.Spots[2].Loupe.worldBound.center.x, view.Spots[2].LoupeRing.worldBound.center.x, 1f, "a middle spot is centred");
            Assert.AreEqual("", view.Spots[0].Number.text, "loupes: a bare ring on the picture, no number to read");
            yield return PixelAt(view.Spots[2].Loupe, view.Spots[2].Loupe.worldBound.center + new Vector2(12f, 0f), c => AssertColour(PictureColour("three.png"), c, "a close-up shows the picture"));

            yield return CardTestInput.Tap(view.Root.panel, view.Spots[3].Loupe.worldBound.center);
            yield return CardTestInput.Settle(0.1f);
            Assert.AreEqual(3, view.OpenIndex, "a real tap on the fourth close-up opened spot 4");
            Assert.AreEqual("The river gate", view.DetailTitle.text);
            Assert.AreEqual("4", view.DetailNumber.text, "the open spot's number beside its title");
            Assert.IsTrue(view.Spots[3].Loupe.ClassListContains("card-hotspot__loupe--open"), "that close-up is marked");
            yield return Render("Card_hotspot_image_loupes_open");
        }

        [UnityTest]
        public IEnumerator Gallery_ARealTapOpensTheLightbox_ChipsAndBack_KeepTheCard_AndEveryPictureIsGivenBack()
        {
            GalleryBlockView gallery = null;
            yield return ShowBlock("gallery_grid_long", v => gallery = (GalleryBlockView)v);
            var sheet = _harness.Sheet;
            var takeover = sheet.Takeover;
            float scroll = sheet.Stack.Scroll.scrollOffset.y;
            int heldOnCard = _harness.Media.HeldCount;

            yield return CardTestInput.Tap(gallery.Root.panel, gallery.Shots[2].Box.worldBound.center);
            yield return CardTestInput.Settle(0.15f);
            Assert.IsTrue(takeover.IsOpen, "a real tap on a picture opens it full screen");
            Assert.AreEqual(2, takeover.PageIndex, "...at that picture");
            Assert.AreEqual("Gate > Gallery", takeover.Crumb.text, "the breadcrumb: card title > the gallery");
            Assert.AreEqual("Back", takeover.Back.text);
            Assert.AreEqual(4, takeover.Chips.childCount, "a chip per sibling picture");
            Assert.IsTrue(takeover.Chips[2].ClassListContains("card-takeover__chip--current"));
            Assert.AreEqual(sheet.Layer.worldBound, takeover.Root.worldBound, "full screen over the card's whole layer");
            var picture = takeover.Page.Q(className: "card-image");
            Assert.IsNotNull(picture);
            yield return PixelAt(picture, picture.worldBound.center, c => AssertColour(PictureColour("three.png"), c, "the lightbox shows the tapped picture"));
            Assert.AreEqual(2, _harness.Media.RefCount("three.png"), "the lightbox loaded its own copy (grid cell + lightbox)");
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(takeover.Back.worldBound.width, takeover.Back.worldBound.height), "Back >= 44");
            foreach (var chip in takeover.Chips.Children())
                Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(chip.worldBound.width, chip.worldBound.height), "a chip >= 44");
            Assert.GreaterOrEqual(CardTestInput.Contrast(takeover.Crumb.resolvedStyle.color, CardTestInput.EffectiveBackground(takeover.Crumb)), UIAccessibility.MinRatioNormalText);
            yield return Render("Card_gallery_lightbox");

            yield return CardTestInput.Tap(takeover.Chips[0].panel, takeover.Chips[0].worldBound.center);
            yield return CardTestInput.Settle(0.15f);
            Assert.AreEqual(0, takeover.PageIndex, "a real tap on chip 1: the first picture");
            Assert.AreEqual(1, _harness.Media.RefCount("three.png"), "the page before gave its picture back");
            Assert.AreEqual(2, _harness.Media.RefCount("one.png"));
            var caption = takeover.Page.Q<Label>(className: "card-gallery__full-caption");
            StringAssert.StartsWith("The gate from the square", caption.text, "caption");
            Assert.AreEqual("Photo: Municipal archive, catalogue 12/447", takeover.Page.Q<Label>(className: "card-gallery__full-credit").text, "credit");

            yield return CardTestInput.Tap(takeover.Back.panel, takeover.Back.worldBound.center);
            yield return CardTestInput.Settle(0.15f);
            Assert.IsFalse(takeover.IsOpen, "Back closes it");
            Assert.AreEqual(0, takeover.HeldMedia, "...giving back every picture it loaded");
            Assert.AreEqual(heldOnCard, _harness.Media.HeldCount, "the card holds what it held before");
            Assert.AreEqual(1, _harness.Media.RefCount("one.png"));
            Assert.AreEqual(scroll, sheet.Stack.Scroll.scrollOffset.y, 0.01f, "the card kept its scroll position (the long real card proves it scrolled: PoiCardSceneTests)");
            Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop, "...and its stop");

            yield return CardTestInput.Tap(gallery.Root.panel, gallery.Shots[1].Box.worldBound.center);
            yield return CardTestInput.Settle(0.15f);
            Assert.IsTrue(takeover.IsOpen);
            sheet.Hide();
            Assert.IsFalse(takeover.IsOpen, "closing the card closes its full-screen view");
            Assert.AreEqual(0, _harness.Media.HeldCount, "a closed card holds no picture at all (lazy load, release on unbind)");
        }

        [UnityTest]
        public IEnumerator BeforeAfter_ARealDragMovesTheHandle_TheCutFollows_AndTheStackDoesNotScroll()
        {
            BeforeAfterBlockView slider = null;
            yield return ShowBlock("before_after_slider_short", v => slider = (BeforeAfterBlockView)v);
            Rect frame = slider.Frame.worldBound;
            Assert.AreEqual(0.5f, slider.Position, 1e-4f, "Start At unset: half and half");
            Assert.AreEqual(frame.width * 0.5f, slider.BeforeClip.worldBound.width, 1f, "Before shows left of the handle");
            Assert.AreEqual("Before", slider.BeforeLabel.text);
            Assert.AreEqual("After", slider.AfterLabel.text);
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(slider.Knob.worldBound.width, slider.Knob.worldBound.height), "the knob >= 44");
            yield return PixelAt(slider.Frame, new Vector2(frame.xMin + frame.width * 0.25f, frame.center.y + 20f), c => AssertColour(PictureColour("before.png"), c, "left of the handle: Before"));
            yield return PixelAt(slider.Frame, new Vector2(frame.xMin + frame.width * 0.75f, frame.center.y + 20f), c => AssertColour(PictureColour("after.png"), c, "right of the handle: After"));
            float scroll = _harness.Sheet.Stack.Scroll.scrollOffset.y;

            yield return CardTestInput.DragFrom(slider.Frame.panel, new Vector2(frame.center.x, frame.center.y), new Vector2(frame.width * 0.3f, 40f));
            yield return null;
            Assert.AreEqual(0.8f, slider.Position, 0.02f, "a real drag to 80% of the width");
            Assert.AreEqual(frame.width * slider.Position, slider.BeforeClip.worldBound.width, 1f, "the cut follows");
            Assert.AreEqual(frame.xMin + frame.width * slider.Position, slider.Handle.worldBound.center.x, 1f, "the handle follows");
            Assert.AreEqual(scroll, _harness.Sheet.Stack.Scroll.scrollOffset.y, 0.01f, "the drag was the slider's: the stack did not scroll");
            Assert.AreEqual(frame.width, slider.Before.Root.worldBound.width, 1f, "Before keeps its full width inside the cut (cut, never squeezed)");
            yield return PixelAt(slider.Frame, new Vector2(frame.xMin + frame.width * 0.7f, frame.center.y + 20f), c => AssertColour(PictureColour("before.png"), c, "70% is now Before"));
            yield return Render("Card_before_after_dragged");

            yield return ShowBlock("before_after_slider_labels", v => slider = (BeforeAfterBlockView)v);
            Assert.AreEqual(0.3f, slider.Position, 1e-4f, "Start At");
            Assert.AreEqual("1740, before the earthquake", slider.BeforeLabel.text, "the block's own words");
            Assert.AreEqual("Today", slider.AfterLabel.text);

            yield return ShowBlock("before_after_slider_missing", v => slider = (BeforeAfterBlockView)v);
            Assert.IsNull(slider.After.Texture, "a missing After file: the unavailable frame, no exception");
            Assert.IsNotNull(slider.Before.Texture);
        }

        [UnityTest]
        public IEnumerator ZoomImage_CtrlWheelZoomsAboutThePointer_ADragPans_AndAtOneAPlainWheelScrollsTheStack()
        {
            ZoomImageBlockView zoom = null;
            yield return ShowBlock("zoom_image_pinch_short", v => zoom = (ZoomImageBlockView)v);
            Rect frame = zoom.Frame.worldBound, picture = zoom.Image.Root.worldBound;
            Assert.AreEqual(1f, zoom.Scale);
            Assert.AreEqual(frame.height, picture.height, 1f, "1x: the square picture fits the frame's height (whole, not cropped)");
            Assert.AreEqual(frame.center.x, picture.center.x, 1f, "...centred");
            Assert.AreEqual("Pinch to zoom in", zoom.Hint.text);

            var at = frame.center + new Vector2(40f, 10f);
            Vector2 before = (at - picture.position) / picture.width;
            yield return CardTestInput.CtrlWheel(zoom.Frame.panel, at, -12f);
            yield return null;
            Assert.AreEqual(2f, zoom.Scale, 0.01f, "one Ctrl + wheel step (12 notches) doubles the scale");
            picture = zoom.Image.Root.worldBound;
            Vector2 after = (at - picture.position) / picture.width;
            Assert.AreEqual(before.x, after.x, 0.01f, "the picture point under the pointer stayed under it (across)");
            Assert.AreEqual(before.y, after.y, 0.01f, "(down)");
            yield return CardTestInput.CtrlWheel(zoom.Frame.panel, at, -60f);
            yield return null;
            Assert.AreEqual(ZoomPanRule.MaxScale, zoom.Scale, 0.01f, "never past 4x");

            float scroll = _harness.Sheet.Stack.Scroll.scrollOffset.y;
            float x0 = zoom.Image.Root.worldBound.x;
            yield return CardTestInput.DragFrom(zoom.Frame.panel, frame.center, new Vector2(-30f, 0f));
            yield return null;
            Assert.AreEqual(x0 - 30f, zoom.Image.Root.worldBound.x, 1f, "a real one-finger drag moves the enlarged picture");
            Assert.AreEqual(scroll, _harness.Sheet.Stack.Scroll.scrollOffset.y, 0.01f, "...and does not scroll the stack");
            Assert.LessOrEqual(zoom.Image.Root.worldBound.xMin, frame.xMin + 0.5f, "never an empty edge");
            yield return Render("Card_zoom_image_zoomed");

            yield return CardTestInput.CtrlWheel(zoom.Frame.panel, at, 200f);
            yield return null;
            Assert.AreEqual(1f, zoom.Scale, 0.01f, "zoomed back out: 1x");
            Assert.AreEqual(frame.center.x, zoom.Image.Root.worldBound.center.x, 1f, "the whole picture, centred again");

            yield return CardTestInput.Wheel(zoom.Frame, -3f);
            yield return CardTestInput.Settle(0.15f);
            Assert.AreEqual(1f, zoom.Scale, "a plain wheel never zooms (it is the stack's: PoiCardTapZoomTests scrolls the long real card with it)");
        }
    }
}
