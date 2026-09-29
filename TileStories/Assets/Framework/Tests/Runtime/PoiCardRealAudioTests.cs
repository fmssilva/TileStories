using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // The one test class of the audio guide that leaves the wall's card exactly as the app builds it (_3.1 step 9A): the REAL audio owner
    // over the card's ONE real AudioSource, playing the real 200 s narration through Unity's audio engine. It needs the machine's audio
    // to work, so it asserts only what the engine reports right after a tap (playing, which clip, where, at what pitch) and never waits
    // for the clip to run: every other audio behaviour is proven in PoiCardAudioSceneTests over a silent device.
    public class PoiCardRealAudioTests : SearchSceneFixture
    {
        private PoiCardSheetView Sheet => Card.Sheet;

        private static IEnumerator Tap(VisualElement e)
        {
            yield return CardTestInput.Tap(e.panel, e.worldBound.center);
        }

        [UnityTest]
        public IEnumerator TheWallsOwnCard_PlaysTheRealClipThroughItsOneAudioSource_PausesSeeksAndChangesSpeed()
        {
            // - the card as the app builds it: one CardAudioService over the AudioSource on the card object, in 2D, not looping
            Assert.IsInstanceOf<CardAudioService>(Card.Audio, "the app's audio owner");
            var source = Card.GetComponent<AudioSource>();
            Assert.IsNotNull(source, "the card object holds the audio source");
            Assert.IsFalse(source.playOnAwake, "nothing plays by itself");
            Assert.IsFalse(source.loop);
            Assert.AreEqual(0f, source.spatialBlend, "a narration is not placed in the room");
            Assert.AreEqual(1, Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Count(s => s.GetComponent<PoiCardHost>() != null), "ONE owner of the sound");
            Assert.IsFalse(source.isPlaying);

            SelectionEventBus.Select("lamp");
            yield return CardTestInput.Settle();
            var chip = Sheet.Stack.BoundViews.OfType<AudioGuideBlockView>().Single(v => v.Variant == BuiltInBlocks.AudioGuideHeroChip);
            yield return Tap(chip.PlayButton);
            yield return null;
            Assert.AreEqual(CardAudioState.Playing, Card.Audio.State);
            Assert.IsTrue(source.isPlaying, "Unity's audio engine is playing");
            Assert.IsNotNull(source.clip);
            Assert.AreEqual("castelo_s_jorge_guide_pt", source.clip.name, "the real narration");
            Assert.AreEqual(200.39f, source.clip.length, 0.1f);
            Assert.AreEqual(1f, source.volume, "full volume");

            Card.Audio.Seek(50f);
            yield return null;
            Assert.AreEqual(50f, source.time, 1f, "the engine's playhead moved to the sought second");

            yield return Tap(chip.PlayButton);
            yield return null;
            Assert.AreEqual(CardAudioState.Paused, Card.Audio.State);
            Assert.IsFalse(source.isPlaying, "paused in the engine too");
            float pausedAt = source.time;
            yield return null;
            yield return null;
            Assert.AreEqual(pausedAt, source.time, 0.05f, "a paused clip's playhead stands still");
            Assert.Greater(pausedAt, 49f, "it kept its place");

            yield return Tap(chip.PlayButton);
            yield return null;
            Assert.IsTrue(source.isPlaying, "resumed");
            Assert.Greater(source.time, pausedAt - 0.05f, "from where it was");

            Card.Audio.SetSpeed(1.25f);
            Assert.AreEqual(1.25f, source.pitch, "speed is the source's pitch");

            // - closing the card keeps it playing; the mini-player draws the real position
            yield return Tap(Sheet.CloseButton);
            yield return null;
            Assert.IsTrue(source.isPlaying, "the card closed: the sound goes on");
            Assert.IsTrue(Card.AudioCoordinator.Mini.IsShown);

            Card.Audio.Stop();
            yield return null;
            Assert.IsFalse(source.isPlaying, "stopped");
            Assert.AreEqual(0, Card.Media.RefCount("audio/castelo_s_jorge_guide_pt.mp3"), "and the clip was given back");
        }
    }
}
