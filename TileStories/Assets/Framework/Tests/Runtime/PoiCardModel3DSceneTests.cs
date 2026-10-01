using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of model_3d, turntable (_3.1 step 10A.2b.3) and Display Takeover (10A.3.1): the REAL LivingRoomScene, its
    // shipped config and the real CardPreviewStage behind PoiCardHost. The Lamp holds two turntable blocks: block_58,
    // the Framework's own `default:azulejo_arch`, is Display Takeover (a teaser on the card, full screen through a
    // second slot); block_59, the LivingRoom-only `146267-LivingRoom2-tex.glb` test fixture (a real room scan, never
    // a Framework default), stays Display Inline with Fit Visible (a real one-finger drag rotates it right on the card). Closing the
    // card releases every slot (PreviewService.ReleaseAll through PoiCardHost.Close).
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
        public IEnumerator TheLamp_HoldsBothTurntableBlocks_TheTakeoverTeaserAndTheInlineRoomScan()
        {
            yield return OpenFull("lamp");
            var models = Models;
            Assert.AreEqual(2, models.Count, "the Lamp's two model_3d blocks (the takeover teaser + the inline room scan) are both bound");

            var teaser = models[0];
            Sheet.Stack.Scroll.ScrollTo(Sheet.Stack.SlotOf(teaser));
            yield return CardTestInput.Settle(0.2f);
            Assert.AreEqual(CardOptions.DisplayTakeover, teaser.Display, "block_58's real config display");
            Assert.IsTrue(teaser.ShowsModel, "the real CardPreviewStage rendered the teaser's own first render");
            Assert.IsNotNull(teaser.Texture);
            yield return Capture("Model3D_Default_Idle");
            float teaserYaw = teaser.State.Yaw;
            yield return CardTestInput.DragFrom(teaser.Frame.panel, teaser.Frame.worldBound.center, new Vector2(150f, 0f));
            yield return null;
            Assert.AreEqual(teaserYaw, teaser.State.Yaw, "the teaser itself never rotates on drag");

            var roomScan = models[1];
            Sheet.Stack.Scroll.ScrollTo(Sheet.Stack.SlotOf(roomScan));
            yield return CardTestInput.Settle(0.2f);
            Assert.AreEqual(CardOptions.DisplayInline, roomScan.Display);
            // - the hollow scan is the fixture of Fit = Visible (_3.1 step 14); the arch keeps the default, so both choices run here
            Assert.AreEqual(ModelFitMode.Visible, roomScan.Fit, "block_59's authored Fit reached the real card");
            Assert.AreEqual(ModelFitMode.YawSafe, teaser.Fit, "block_58 sets no Fit: the default Yaw Safe");
            Assert.IsTrue(roomScan.ShowsModel, "the real CardPreviewStage rendered this block's model");
            Assert.IsNotNull(roomScan.Texture);
            yield return Capture("Model3D_RoomScan_Idle");
            float before = roomScan.State.Yaw;
            yield return CardTestInput.DragFrom(roomScan.Frame.panel, roomScan.Frame.worldBound.center, new Vector2(150f, 0f));
            yield return null;
            Assert.AreNotEqual(before, roomScan.State.Yaw, "a real one-finger drag rotated the inline room scan");
            yield return CardTestInput.Settle(0.1f);
            PreviewPixels.AssertDrawnInsideWithMargin(roomScan.Texture, "the inline room scan after a real drag");
            yield return Capture("Model3D_RoomScan_Dragged");

            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
        }

        [UnityTest]
        public IEnumerator TheLamp_TakeoverTeaser_ARealTapOpensItFullScreen_ARealDragRotatesIt_BackKeepsTheCardsStopAndScroll()
        {
            yield return OpenFull("lamp");
            var teaser = Models[0];
            Sheet.Stack.Scroll.ScrollTo(Sheet.Stack.SlotOf(teaser));
            yield return CardTestInput.Settle(0.2f);
            var scroll = Sheet.Stack.Scroll;
            float scrollOffset = scroll.scrollOffset.y;
            Assert.Greater(scrollOffset, 0f, "precondition: the card is really scrolled to the teaser");

            yield return CardTestInput.Tap(teaser.TeaserOpenButton.panel, teaser.TeaserOpenButton.worldBound.center);
            yield return CardTestInput.Settle(0.15f);
            var takeover = Sheet.Takeover;
            Assert.IsTrue(takeover.IsOpen, "a real tap opened the model full screen");
            Assert.IsTrue(takeover.Crumb.text.EndsWith(" > The stone arch"), "the Lamp's title, then the model's authored one");
            var full = teaser.FullScreenView;
            Assert.IsNotNull(full);
            Assert.IsTrue(full.ShowsModel);
            yield return Capture("Model3D_Takeover_FullScreen");

            float before = full.State.Yaw;
            yield return CardTestInput.DragFrom(full.Frame.panel, full.Frame.worldBound.center, new Vector2(150f, 0f));
            yield return null;
            Assert.AreNotEqual(before, full.State.Yaw, "a real one-finger drag rotates it full screen");

            yield return CardTestInput.Tap(takeover.Back.panel, takeover.Back.worldBound.center);
            yield return CardTestInput.Settle(0.15f);
            Assert.IsFalse(takeover.IsOpen, "Back closed it");
            Assert.IsNull(teaser.FullScreenView, "and released the second slot");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "the card kept the stop it had");
            Assert.AreEqual(scrollOffset, scroll.scrollOffset.y, 1f, "and the scroll it had");
            Assert.IsTrue(teaser.ShowsModel, "the card's own teaser picture is untouched");

            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
        }

        // 10A.3.2, Phase B: Lamp - Religious's header is model_turntable on `default:azulejo_arch` (the Framework's own
        // default-key Fallback picture `default:azulejo_detail`): at peek only the title shows, at half and full the
        // hero shows the model, auto-spin turns it and a real drag rotates it.
        [UnityTest]
        public IEnumerator LampReligious_HeaderShowsTheModel_AtHalfAndFull_OnlyTheTitleAtPeek_ARealDragRotatesIt()
        {
            SelectionEventBus.Select("lamp_religious");
            yield return CardTestInput.Settle();
            var header = (HeaderBlockView)Sheet.Stack.BoundViews[0];
            Assert.IsTrue(header.HasHero, "the real config's model_turntable header has a hero");
            var model = header.ModelView;
            Assert.IsTrue(header.PeekPart.worldBound.height > 0f, "the title part is what the peek stop shows");
            Assert.AreEqual(SheetStopRule.Stop.Peek, Sheet.Stop);

            Sheet.SetStop(SheetStopRule.Stop.Half);
            yield return CardTestInput.Settle(0.4f);
            Assert.IsTrue(model.ShowsModel, "the real CardPreviewStage rendered the header's model");
            var hero = header.HeroPart.worldBound;
            Assert.GreaterOrEqual(model.Frame.worldBound.width, hero.width - 2f, "the model's stage fills the hero's whole width, not a corner of it");
            Assert.GreaterOrEqual(model.Frame.worldBound.height, hero.height - 2f, "and its whole height");
            Assert.AreEqual(model.Frame.worldBound.width / model.Frame.worldBound.height, (float)model.Texture.width / model.Texture.height, 0.05f,
                "the RenderTexture has the hero stage's aspect");
            Assert.IsFalse(model.Hint.resolvedStyle.display == DisplayStyle.Flex, "the hero shows no caption line");
            yield return Capture("Religious_Header_Model_Half");

            float before = model.State.Yaw;
            yield return CardTestInput.DragFrom(model.Frame.panel, model.Frame.worldBound.center, new Vector2(120f, 0f));
            yield return null;
            Assert.AreNotEqual(before, model.State.Yaw, "a real one-finger drag rotates the header's model");

            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle(0.4f);
            Assert.IsTrue(model.ShowsModel);
            // - 10A.3-fix.2: dragged to another angle, the header's arch used to touch the frame's bottom edge at Full
            PreviewPixels.AssertDrawnInsideWithMargin(model.Texture, "the header's model after a real drag, at full");
            yield return Capture("Religious_Header_Model_Full");

            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
        }

        // 10A.3-fix.1, Phase B: the header's auto-spin clock (TurntableState.IdleSeconds grows only while a tick really runs)
        // stands still while the card rests at peek -- the header's hero sits under the sheet there, hidden -- and runs
        // at half. The same model, the same frames: the half stop is the positive control that the clock CAN run.
        [UnityTest]
        public IEnumerator LampReligious_HeaderModelAutoSpin_TicksOnlyWhileTheCardIsAbovePeek()
        {
            SelectionEventBus.Select("lamp_religious");
            yield return CardTestInput.Settle();
            var model = ((HeaderBlockView)Sheet.Stack.BoundViews[0]).ModelView;
            Assert.AreEqual(SheetStopRule.Stop.Peek, Sheet.Stop, "precondition: the card opened at peek");
            for (int i = 0; i < 120 && !model.ShowsModel; i++) yield return null;
            Assert.IsTrue(model.ShowsModel, "precondition: the real stage rendered the header's model (nothing else can stop the clock)");

            float atPeek = model.State.IdleSeconds;
            yield return CardTestInput.Settle(0.5f);
            Assert.AreEqual(atPeek, model.State.IdleSeconds, 0f, "at peek the hidden header model asked for nothing: its idle clock did not move");

            Sheet.SetStop(SheetStopRule.Stop.Half);
            for (int i = 0; i < 120 && model.State.IdleSeconds <= atPeek; i++) yield return null;
            float atHalf = model.State.IdleSeconds;
            Assert.Greater(atHalf, atPeek, "at half the same model ticks: its idle clock moved (" + atPeek + " -> " + atHalf + ")");

            Sheet.SetStop(SheetStopRule.Stop.Peek);
            yield return CardTestInput.Settle(0.1f);
            float backAtPeek = model.State.IdleSeconds;
            yield return CardTestInput.Settle(0.5f);
            Assert.AreEqual(backAtPeek, model.State.IdleSeconds, 0f, "lowered back to peek it stops again");

            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
        }

        // The same law for a model block IN the stack (the Lamp's inline room scan, Auto Spin on): it ticks at half with the block
        // in view, stops when the card is lowered to peek (the block is then only a stage under the sheet), and stops when it is
        // scrolled out of the viewport at half.
        [UnityTest]
        public IEnumerator TheLamp_InlineRoomScanAutoSpin_TicksOnlyWhileInViewAndAbovePeek()
        {
            SelectionEventBus.Select("lamp");
            yield return CardTestInput.Settle();
            Sheet.SetStop(SheetStopRule.Stop.Half);
            yield return CardTestInput.Settle();
            var roomScan = Models[1];
            Assert.AreEqual(CardOptions.DisplayInline, roomScan.Display, "precondition: block_59 is the inline room scan");
            Sheet.Stack.Scroll.ScrollTo(Sheet.Stack.SlotOf(roomScan));
            yield return CardTestInput.Settle(0.2f);
            for (int i = 0; i < 120 && !roomScan.ShowsModel; i++) yield return null;
            Assert.IsTrue(roomScan.ShowsModel, "precondition: the real stage rendered the room scan");
            var viewport = Sheet.Stack.Scroll.contentViewport.worldBound;
            Assert.IsTrue(roomScan.Frame.worldBound.Overlaps(viewport), "precondition: the block is in view at half: " + roomScan.Frame.worldBound + " in " + viewport);

            float start = roomScan.State.IdleSeconds;
            for (int i = 0; i < 120 && roomScan.State.IdleSeconds <= start; i++) yield return null;
            Assert.Greater(roomScan.State.IdleSeconds, start, "in view at half the room scan ticks (Auto Spin)");

            Sheet.SetStop(SheetStopRule.Stop.Peek);
            yield return CardTestInput.Settle(0.3f);
            float atPeek = roomScan.State.IdleSeconds;
            yield return CardTestInput.Settle(0.5f);
            Assert.AreEqual(atPeek, roomScan.State.IdleSeconds, 0f,
                "at peek the block asks for nothing (its stage " + roomScan.Frame.worldBound + ", the card " + Sheet.Root.worldBound + ")");

            Sheet.SetStop(SheetStopRule.Stop.Half);
            yield return CardTestInput.Settle(0.3f);
            Sheet.Stack.Scroll.scrollOffset = Vector2.zero;
            yield return CardTestInput.Settle(0.3f);
            Assert.IsFalse(roomScan.Frame.worldBound.Overlaps(Sheet.Stack.Scroll.contentViewport.worldBound),
                "precondition: scrolled to the top, the room scan is out of the viewport");
            float scrolledAway = roomScan.State.IdleSeconds;
            yield return CardTestInput.Settle(0.5f);
            Assert.AreEqual(scrolledAway, roomScan.State.IdleSeconds, 0f, "scrolled out of view it asks for nothing");

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
