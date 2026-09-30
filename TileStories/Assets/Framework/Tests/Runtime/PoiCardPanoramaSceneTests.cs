using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of panorama_360 (_3.1 step 10A.4.3): the REAL LivingRoomScene, its shipped config and the real CardPreviewStage behind PoiCardHost.
    // The Lamp holds two panorama blocks on the Framework's default `default:tiled_room_360`: block_60, the drag look, Display Inline (a real
    // drag turns the view right on the card), and block_61, the gyro look with Start Heading 90, Display Takeover (a teaser on the card, the
    // viewer full screen through a second slot; this machine has no attitude sensor, so the gyro look behaves as dragging and says so).
    // Closing the card releases every slot (PreviewService.ReleaseAll through PoiCardHost.Close): nothing of a sphere is left behind.
    public class PoiCardPanoramaSceneTests : SearchSceneFixture
    {
        private PoiCardSheetView Sheet => Card.Sheet;

        private IEnumerator OpenFull(string poiId)
        {
            SelectionEventBus.Select(poiId);
            yield return CardTestInput.Settle();
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
        }

        private System.Collections.Generic.List<PanoramaBlockView> Panoramas =>
            Sheet.Stack.BoundViews.OfType<PanoramaBlockView>().ToList();

        private static int CountNamed<T>(string prefix) where T : Object
        {
            int n = 0;
            foreach (var o in Resources.FindObjectsOfTypeAll<T>()) if (o.name.StartsWith(prefix)) n++;
            return n;
        }

        private static float Difference(Color a, Color b) => Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);

        [UnityTest]
        public IEnumerator TheLamp_HoldsBothPanoramaBlocks_TheInlineDragAndTheGyroTakeoverTeaser()
        {
            yield return OpenFull("lamp");
            var views = Panoramas;
            Assert.AreEqual(2, views.Count, "the Lamp's two panorama_360 blocks are both bound");

            var inline = views[0];
            Sheet.Stack.Scroll.ScrollTo(Sheet.Stack.SlotOf(inline));
            yield return CardTestInput.Settle(0.2f);
            Assert.AreEqual(CardOptions.DisplayInline, inline.Display, "block_60's real config display");
            Assert.IsFalse(inline.IsGyroLook, "block_60 is the drag look");
            Assert.IsTrue(inline.ShowsPanorama, "the real CardPreviewStage drew the default panorama");
            Assert.AreEqual("Drag to look around, pinch to zoom", inline.Hint.text);
            yield return Capture("Panorama_Drag_Idle");
            Color before = PreviewPixels.Average(inline.Texture);
            float yaw = inline.State.Yaw;
            yield return CardTestInput.DragFrom(inline.Frame.panel, inline.Frame.worldBound.center, new Vector2(150f, 0f));
            yield return null;
            Assert.AreNotEqual(yaw, inline.State.Yaw, "a real one-finger drag turned the inline view");
            Assert.Greater(Difference(before, PreviewPixels.Average(inline.Texture)), 0.01f, "and the stage DREW the turn");
            yield return CardTestInput.Settle(0.1f);
            yield return Capture("Panorama_Drag_Dragged");

            var takeover = views[1];
            Sheet.Stack.Scroll.ScrollTo(Sheet.Stack.SlotOf(takeover));
            yield return CardTestInput.Settle(0.2f);
            Assert.AreEqual(CardOptions.DisplayTakeover, takeover.Display, "block_61's real config display");
            Assert.IsTrue(takeover.IsGyroLook, "block_61 is the gyro look");
            Assert.AreEqual(90f, takeover.StartHeading, 0.01f, "with the authored Start Heading");
            Assert.AreEqual(90f, takeover.State.Yaw, 0.01f);
            Assert.AreEqual("The tiled room", takeover.TeaserName.text, "the authored English title");
            Assert.IsTrue(takeover.ShowsPanorama);
            yield return Capture("Panorama_Takeover_Teaser");
            float teaserYaw = takeover.State.Yaw;
            yield return CardTestInput.DragFrom(takeover.Frame.panel, takeover.Frame.worldBound.center, new Vector2(150f, 0f));
            yield return null;
            Assert.AreEqual(teaserYaw, takeover.State.Yaw, "the teaser itself never turns on drag");

            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
        }

        [UnityTest]
        public IEnumerator TheLamp_GyroTakeover_ARealTapOpensItFullScreen_ARealDragTurnsIt_BackKeepsTheCardsStopAndScroll()
        {
            Assert.IsFalse(DeviceAttitude.Available, "precondition: no attitude sensor: the gyro look behaves as dragging");
            yield return OpenFull("lamp");
            var teaser = Panoramas[1];
            Sheet.Stack.Scroll.ScrollTo(Sheet.Stack.SlotOf(teaser));
            yield return CardTestInput.Settle(0.2f);
            var scroll = Sheet.Stack.Scroll;
            float scrollOffset = scroll.scrollOffset.y;
            Assert.Greater(scrollOffset, 0f, "precondition: the card is really scrolled to the teaser");

            yield return CardTestInput.Tap(teaser.TeaserOpenButton.panel, teaser.TeaserOpenButton.worldBound.center);
            yield return CardTestInput.Settle(0.15f);
            var page = Sheet.Takeover;
            Assert.IsTrue(page.IsOpen, "a real tap opened the panorama full screen");
            Assert.IsTrue(page.Crumb.text.EndsWith(" > The tiled room"), "the Lamp's title, then the panorama's authored one");
            var full = teaser.FullScreenView;
            Assert.IsNotNull(full);
            Assert.IsTrue(full.ShowsPanorama);
            Assert.AreEqual(90f, full.State.Yaw, 0.01f, "the page opens at the same Start Heading");
            Assert.AreEqual("Drag to look around, pinch to zoom", full.Hint.text, "no sensor here: the drag words, not 'move your phone'");
            yield return Capture("Panorama_Takeover_FullScreen");

            float before = full.State.Yaw;
            yield return CardTestInput.DragFrom(full.Frame.panel, full.Frame.worldBound.center, new Vector2(150f, 0f));
            yield return null;
            Assert.AreNotEqual(before, full.State.Yaw, "a real one-finger drag turns it full screen");

            yield return CardTestInput.Tap(page.Back.panel, page.Back.worldBound.center);
            yield return CardTestInput.Settle(0.15f);
            Assert.IsFalse(page.IsOpen, "Back closed it");
            Assert.IsNull(teaser.FullScreenView, "and released the second slot");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "the card kept the stop it had");
            Assert.AreEqual(scrollOffset, scroll.scrollOffset.y, 1f, "and the scroll it had");
            Assert.IsTrue(teaser.ShowsPanorama, "the card's own teaser picture is untouched");

            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
        }

        // Opening and closing the Lamp builds and releases every sphere: after the stage's one-time rig, nothing of them is left
        [UnityTest]
        public IEnumerator ClosingTheCard_ReleasesEverySphereMeshMaterialAndTexture_ThreeOpenClosesLeakNothing()
        {
            // - the first open builds the stage's shared rig once and keeps it: the baseline is taken after that one-time cost
            yield return OpenFull("lamp");
            foreach (var view in Panoramas)
            {
                Sheet.Stack.Scroll.ScrollTo(Sheet.Stack.SlotOf(view));
                yield return CardTestInput.Settle(0.15f);
            }
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            yield return null;
            Assert.AreEqual(0, CountNamed<Mesh>("CardPanoramaSphere_"), "precondition: closing the card already let every sphere mesh go");
            int baselineObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;

            for (int i = 0; i < 3; i++)
            {
                yield return OpenFull("lamp");
                foreach (var view in Panoramas)
                {
                    Sheet.Stack.Scroll.ScrollTo(Sheet.Stack.SlotOf(view));
                    yield return CardTestInput.Settle(0.15f);
                    Assert.IsTrue(view.ShowsPanorama, "round " + i);
                }
                Assert.Greater(CountNamed<Mesh>("CardPanoramaSphere_"), 0, "round " + i + ": spheres exist while the card is open");
                Assert.Greater(CountNamed<Material>("CardPanorama_"), 0);
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
                yield return null;
            }
            Assert.AreEqual(0, CountNamed<Mesh>("CardPanoramaSphere_"), "three open/close cycles: no sphere mesh left");
            Assert.AreEqual(0, CountNamed<Material>("CardPanorama_"), "no material instance left");
            Assert.AreEqual(baselineObjects, Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length, "no GameObject left");
            Assert.AreEqual(0, DeviceAttitude.Users, "and no sensor held");
        }
    }
}
