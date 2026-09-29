using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // CardPreviewService (_3.1 step 10A.2): the card's slot manager for model_3d/panorama_360 previews, over a fake
    // IPreviewStage (ManualPreviewStage) so "a model loaded/released" is a count, never a real glTFast import or a real
    // RenderTexture camera -- that real pipeline is CardPreviewStage's own concern, proven separately on the real Lamp.
    public class CardPreviewServiceTests
    {
        private static IMediaSource NoMedia() => null;

        [Test]
        public void Request_LoadsOnceForANewKey_ReadyAtOnceThroughTheFakeStage()
        {
            var stage = new ManualPreviewStage();
            var service = new CardPreviewService(stage, NoMedia);
            var slot = service.Request("block_1", MediaKind.Model, "default:azulejo_arch");
            Assert.AreEqual(1, stage.LoadCount);
            Assert.IsFalse(slot.IsLoading, "the fake stage resolves synchronously");
            Assert.IsFalse(slot.Failed);
            Assert.IsNotNull(slot.Texture);
            Assert.AreEqual(1, stage.AliveHandles);
        }

        [Test]
        public void Request_TheSameKeyKindAndPathAgain_ReturnsTheSameSlot_NeverReloads()
        {
            var stage = new ManualPreviewStage();
            var service = new CardPreviewService(stage, NoMedia);
            var first = service.Request("block_1", MediaKind.Model, "default:azulejo_arch");
            var second = service.Request("block_1", MediaKind.Model, "default:azulejo_arch");
            Assert.AreSame(first, second, "a rebind with the same media never reloads");
            Assert.AreEqual(1, stage.LoadCount);
            Assert.AreEqual(1, stage.AliveHandles);
        }

        [Test]
        public void Request_TheSameKeyWithADifferentPath_ReleasesTheOld_LoadsTheNew()
        {
            var stage = new ManualPreviewStage();
            var service = new CardPreviewService(stage, NoMedia);
            service.Request("block_1", MediaKind.Model, "default:azulejo_arch");
            Assert.AreEqual(1, stage.AliveHandles);
            var second = service.Request("block_1", MediaKind.Model, "default:another_model");
            Assert.AreEqual(2, stage.LoadCount);
            Assert.AreEqual(1, stage.AliveHandles, "the old handle for this key was released, only the new one is alive");
            Assert.IsFalse(second.Failed);
        }

        [Test]
        public void Request_TwoDifferentKeys_EachGetsItsOwnSlot_BothAliveAtOnce()
        {
            var stage = new ManualPreviewStage();
            var service = new CardPreviewService(stage, NoMedia);
            var model = service.Request("block_1", MediaKind.Model, "default:azulejo_arch");
            var panorama = service.Request("block_2", MediaKind.Panorama, "default:tiled_room_360");
            Assert.AreEqual(2, stage.AliveHandles, "a card can show a model AND a panorama at once, unlike audio/video");
            Assert.AreNotSame(model, panorama);
        }

        [Test]
        public void Request_AMissingOrWrongTypePath_FailsWithoutALoadedHandle()
        {
            var stage = new ManualPreviewStage();
            var service = new CardPreviewService(stage, NoMedia);
            var slot = service.Request("block_1", MediaKind.Model, "missing_file.glb");
            Assert.IsFalse(slot.IsLoading);
            Assert.IsTrue(slot.Failed);
            Assert.IsNull(slot.Texture);
            Assert.AreEqual(0, stage.AliveHandles, "a failed load leaves nothing alive to release");
        }

        [Test]
        public void Request_AnEmptyPath_FailsAtOnce_WithoutAskingTheStageAtAll()
        {
            var stage = new ManualPreviewStage();
            var service = new CardPreviewService(stage, NoMedia);
            var slot = service.Request("block_1", MediaKind.Model, "");
            Assert.IsTrue(slot.Failed);
            Assert.AreEqual(0, stage.LoadCount, "no field authored: nothing to even try loading");
        }

        [Test]
        public void Release_GivesBackExactlyThatKeysSlot_ARequestForItAfterwardsLoadsAgain()
        {
            var stage = new ManualPreviewStage();
            var service = new CardPreviewService(stage, NoMedia);
            service.Request("block_1", MediaKind.Model, "default:azulejo_arch");
            service.Request("block_2", MediaKind.Panorama, "default:tiled_room_360");
            service.Release("block_1");
            Assert.AreEqual(1, stage.AliveHandles, "only block_1's handle was released");
            service.Request("block_1", MediaKind.Model, "default:azulejo_arch");
            Assert.AreEqual(3, stage.LoadCount, "released, then asked for again: a real reload, not the cached slot (2 initial + 1 reload)");
        }

        [Test]
        public void ReleaseAll_GivesBackEveryStillHeldSlot_TheCardClosing()
        {
            var stage = new ManualPreviewStage();
            var service = new CardPreviewService(stage, NoMedia);
            service.Request("block_1", MediaKind.Model, "default:azulejo_arch");
            service.Request("block_2", MediaKind.Panorama, "default:tiled_room_360");
            service.ReleaseAll();
            Assert.AreEqual(0, stage.AliveHandles, "no GameObject/texture is left behind once every slot gave its media back");
        }

        [Test]
        public void Changed_FiresWhenASlotsStateMoves()
        {
            var stage = new ManualPreviewStage();
            var service = new CardPreviewService(stage, NoMedia);
            int changed = 0;
            service.Changed += () => changed++;
            service.Request("block_1", MediaKind.Model, "default:azulejo_arch");
            Assert.Greater(changed, 0, "ready fired at least one Changed");
            int afterLoad = changed;
            service.Release("block_1");
            Assert.Greater(changed, afterLoad, "release fired its own Changed");
        }
    }
}
