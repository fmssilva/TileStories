using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Video;

namespace TileStories.Tests
{
    // The REAL video device under the real card (_3.1 step 9B): the scene's own CardVideoPlayer and Unity's VideoPlayer decoding the fixture's
    // real castle clip. Every other video test uses the ManualVideoOutput; this one proves the device the app ships behaves as the owner
    // expects: frames arrive in the RenderTexture the card draws, a seek lands, a pause holds, a header loop is muted and loops, and closing
    // the card gives the clip back. A real decoder cannot be told what time it is: the test waits for CONDITIONS, frame by frame and with a
    // cap -- never for an amount of time -- and asserts no elapsed time.
    public class PoiCardRealVideoTests : SearchSceneFixture
    {
        private const string ClipPath = "video/lisbon_square_castle_55s.mp4";
        // - frames, not seconds: generous for a decoder on a busy Editor, and still a fixed bound
        private const int MaxFrames = 1200;

        private PoiCardSheetView Sheet => Card.Sheet;

        private IEnumerator Until(System.Func<bool> condition, string what)
        {
            for (int i = 0; i < MaxFrames && !condition(); i++) yield return null;
            Assert.IsTrue(condition(), what + " (within " + MaxFrames + " frames)");
        }

        private IEnumerator OpenFull(string poiId)
        {
            SelectionEventBus.Select(poiId);
            yield return CardTestInput.Settle();
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
        }

        [UnityTest]
        public IEnumerator TheRealVideoPlayer_DecodesTheRealClipIntoTheCardsTexture_SeeksPausesAndGivesItBack()
        {
            Assert.IsInstanceOf<CardVideoService>(Card.Video, "the app's video owner");
            var player = Card.GetComponent<VideoPlayer>();
            Assert.IsNotNull(player, "the card object holds the ONE video player");
            Assert.IsFalse(player.playOnAwake, "nothing plays by itself");
            Assert.AreEqual(VideoRenderMode.RenderTexture, player.renderMode);

            yield return OpenFull("lamp");
            var inline = Sheet.Stack.BoundViews.OfType<VideoBlockView>().Single(v => v.Display == CardOptions.DisplayInline);
            yield return CardTestInput.IntoReach(Sheet, inline.Panel.PlayOverlay);
            yield return CardTestInput.Tap(inline.Panel.PlayOverlay.panel, inline.Panel.PlayOverlay.worldBound.center);
            Assert.AreEqual(CardVideoState.Playing, Card.Video.State, "a real tap started it");
            Assert.AreEqual("lisbon_square_castle_55s", player.clip.name, "the real clip");
            Assert.AreSame(player.targetTexture, Card.Video.Texture, "it decodes into the texture the card draws");

            yield return Until(() => Card.Video.HasFrame, "the decoder delivered a first frame");
            yield return Until(() => inline.Panel.ShowsFrames, "the panel draws the real frames");
            Assert.AreEqual(854, player.targetTexture.width, "a texture the clip's size");
            yield return CardTestInput.Settle(0.2f);
            yield return Capture("Lamp_Video_RealFrame");

            // - the middle of the clip itself (54.9 s since _3.3 P1b), never a typed second that a shorter clip would not reach
            float sought = (float)player.clip.length / 2f;
            Card.Video.Seek(sought);
            yield return Until(() => Mathf.Abs((float)player.time - sought) < 2f, "the real playhead moved to the sought second (" + sought + " s)");
            Card.Video.Pause();
            yield return Until(() => !player.isPlaying, "paused in the engine too");
            Assert.AreEqual(CardVideoState.Paused, Card.Video.State);

            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            Assert.AreEqual(CardVideoState.Idle, Card.Video.State, "closing the card stopped it");
            Assert.IsNull(player.clip, "the player let go of the clip");
            Assert.AreEqual(0, Card.Media.RefCount(ClipPath), "and the media source gave it back");
        }

        [UnityTest]
        public IEnumerator LampMilitarysHeaderLoop_OnTheRealPlayer_IsMutedAndLooping_AndItsFramesArrive()
        {
            var player = Card.GetComponent<VideoPlayer>();
            yield return OpenFull("lamp_military");
            Assert.IsTrue(Card.Video.PlayingLoop, "the header asked for its loop");
            Assert.IsTrue(player.isLooping, "the real player loops");
            Assert.AreEqual(VideoAudioOutputMode.Direct, player.audioOutputMode);
            Assert.IsTrue(player.GetDirectAudioMute(0), "and plays no sound");
            var header = (HeaderBlockView)Sheet.Stack.BoundViews[0];
            yield return Until(() => header.LoopShowsFrames, "the loop's real frames replaced the poster");
            Sheet.Stack.Scroll.scrollOffset = Vector2.zero;
            yield return CardTestInput.Settle(0.2f);
            yield return Capture("Military_Header_Loop_RealFrame");
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            Assert.AreEqual(CardVideoState.Idle, Card.Video.State);
        }
    }
}
