using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Tests
{
    // Phase B of model_3d, turntable (_3.1 step 10A.2b.3): the REAL LivingRoomScene, its shipped config and the real
    // CardPreviewStage behind PoiCardHost. The Lamp holds two turntable blocks: one on the Framework's own
    // `default:azulejo_arch`, and one on the LivingRoom-only `146267-LivingRoom2-tex.glb` test fixture (a real
    // room scan, never a Framework default). A real one-finger drag rotates the model; closing the card releases it
    // (PreviewService.ReleaseAll through PoiCardHost.Close).
    public class PoiCardModel3DSceneTests : SearchSceneFixture
    {
        private PoiCardSheetView Sheet => Card.Sheet;

        private IEnumerator OpenFull(string poiId)
        {
            SelectionEventBus.Select(poiId);
            yield return CardTestInput.Settle();
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
        }

        private System.Collections.Generic.List<ModelTurntableBlockView> Models =>
            Sheet.Stack.BoundViews.OfType<ModelTurntableBlockView>().ToList();

        [UnityTest]
        public IEnumerator TheLamp_HoldsBothTurntableBlocks_TheDefaultModelAndTheRealRoomScan_BothRenderAndARealDragRotates()
        {
            yield return OpenFull("lamp");
            var models = Models;
            Assert.AreEqual(2, models.Count, "the Lamp's two model_3d blocks (default + the room scan) are both bound");

            string[] captureNames = { "Default_Idle", "RoomScan_Idle" };
            string[] draggedNames = { "Default_Dragged", "RoomScan_Dragged" };
            for (int i = 0; i < models.Count; i++)
            {
                var model = models[i];
                Sheet.Stack.Scroll.ScrollTo(Sheet.Stack.SlotOf(model));
                yield return CardTestInput.Settle(0.2f);
                Assert.IsTrue(model.ShowsModel, "the real CardPreviewStage rendered this block's model");
                Assert.IsNotNull(model.Texture);
                yield return Capture("Model3D_" + captureNames[i]);

                float before = model.State.Yaw;
                yield return CardTestInput.DragFrom(model.Frame.panel, model.Frame.worldBound.center, new Vector2(150f, 0f));
                yield return null;
                Assert.AreNotEqual(before, model.State.Yaw, "a real one-finger drag rotated this model");
                yield return CardTestInput.Settle(0.1f);
                yield return Capture("Model3D_" + draggedNames[i]);
            }

            // - the room scan is the LivingRoom-only test fixture, never a Framework default
            var roomScan = models[1];
            Assert.IsTrue(roomScan.ShowsModel);

            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
        }

        // Opens and closes the Lamp once first: CardPreviewStage builds its shared rig (camera + key + fill light +
        // rig root, 4 GameObjects) lazily on first use and correctly keeps it for reuse across every later slot --
        // the same "one owner beside the card, never released" shape as CardAudioPlayer/CardVideoPlayer. The baseline
        // for a leak check is taken AFTER that one-time cost, so it measures only the per-handle objects Release()
        // actually owns (the model root + the instantiated model, per slot).
        private IEnumerator OpenAndCloseOnceToBuildTheRig()
        {
            yield return OpenFull("lamp");
            foreach (var model in Models)
            {
                Sheet.Stack.Scroll.ScrollTo(Sheet.Stack.SlotOf(model));
                yield return CardTestInput.Settle(0.15f);
            }
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
        }

        [UnityTest]
        public IEnumerator ClosingTheCard_ReleasesBothModelSlots_NoLeftoverGameObjects()
        {
            yield return OpenAndCloseOnceToBuildTheRig();
            int baseline = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;

            yield return OpenFull("lamp");
            Assert.AreEqual(2, Models.Count);
            foreach (var model in Models)
            {
                Sheet.Stack.Scroll.ScrollTo(Sheet.Stack.SlotOf(model));
                yield return CardTestInput.Settle(0.2f);
                Assert.IsTrue(model.ShowsModel);
            }

            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            yield return null;

            int after = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;
            Assert.AreEqual(baseline, after, "closing the card released both preview slots: no leftover GameObjects");
        }

        [UnityTest]
        public IEnumerator OpeningAndClosingTheLampThreeTimes_NeverLeaksAModelHandle()
        {
            yield return OpenAndCloseOnceToBuildTheRig();
            int baseline = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;
            for (int i = 0; i < 3; i++)
            {
                yield return OpenFull("lamp");
                foreach (var model in Models)
                {
                    Sheet.Stack.Scroll.ScrollTo(Sheet.Stack.SlotOf(model));
                    yield return CardTestInput.Settle(0.15f);
                }
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
            }
            yield return null;
            int after = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;
            Assert.AreEqual(baseline, after, "three open/close cycles left nothing behind");
        }
    }
}
