using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Tests
{
    // The wiring of _3.1 step 10A.2b.2: PoiCardHost owns one CardPreviewService over the real CardPreviewStage, and
    // BlockBindContext.Preview reaches it. The Lamp itself gained two real model_3d blocks in 10A.2b.3, so this proves
    // the OWNERSHIP half: opening and closing a real card several times never grows leftover preview GameObjects.
    public class PoiCardPreviewWiringTests : SearchSceneFixture
    {
        [UnityTest]
        public IEnumerator TheCardHost_OwnsOnePreviewServiceOverTheRealStage()
        {
            Assert.IsInstanceOf<CardPreviewService>(Card.PreviewService, "the app's preview owner");
            Assert.AreSame(Card.Preview, Card.PreviewService);
            yield break;
        }

        [UnityTest]
        public IEnumerator OpeningAndClosingTheLampThreeTimes_LeavesNoLeftoverPreviewObjects()
        {
            // - the Lamp's own model_3d blocks make CardPreviewStage build its shared rig (camera + key + fill light +
            //   rig root) lazily on first use; it is kept for reuse, never released, the same shape as
            //   CardAudioPlayer/CardVideoPlayer -- one open/close first pays that one-time cost before the baseline
            SelectionEventBus.Select("lamp");
            yield return CardTestInput.Settle();
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();

            int baseline = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;

            for (int i = 0; i < 3; i++)
            {
                SelectionEventBus.Select("lamp");
                yield return CardTestInput.Settle();
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
            }

            int after = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;
            Assert.AreEqual(baseline, after,
                "closing the card releases every preview slot (ReleaseAll): three open/close cycles must not grow the scene");
        }

        [UnityTest]
        public IEnumerator ClosingTheCard_CallsReleaseAllOnThePreviewService()
        {
            var stage = new ManualPreviewStage();
            var service = new CardPreviewService(stage, () => Card.Media);
            Card.UsePreviewService(service);
            service.Request("probe", MediaKind.Model, "default:azulejo_arch");
            Assert.AreEqual(1, stage.AliveHandles);

            SelectionEventBus.Select("lamp");
            yield return CardTestInput.Settle();
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();

            Assert.AreEqual(0, stage.AliveHandles, "the card closing released the probe slot too");
        }
    }
}
