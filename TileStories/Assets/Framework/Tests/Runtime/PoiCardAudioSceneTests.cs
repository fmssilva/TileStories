using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of the audio guide (_3.1 step 9A): the REAL LivingRoomScene, its shipped config and the fixture's real clips loaded through the
    // card's own media source. The Lamp carries a hero chip and a player on the castle's Portuguese narration (with placeholder captions),
    // Lamp - Military a chip on a 20 s generated tone, Lamp - Economic a player on the long English promotion. Every action is a real
    // tap or drag on the panel. Only the sound DEVICE is a stand-in (a silent ManualAudioOutput) and the fade's clock a number, so the
    // 200 s narration never costs 200 s of waiting; PoiCardRealAudioTests puts the real AudioSource under the same card.
    public class PoiCardAudioSceneTests : SearchSceneFixture
    {
        private const string GuidePath = "audio/castelo_s_jorge_guide_pt.mp3";
        private const string EnglishPath = "audio/portugal_tourism_guide_en.mp3";

        private PoiCardSheetView Sheet => Card.Sheet;
        private HeaderBlockView Header => (HeaderBlockView)Sheet.Stack.BoundViews[0];
        private CardSettings Live => Session.CardSettings;
        private ManualAudioOutput _out;
        private CardAudioService _audio;
        private float _now;

        [UnitySetUp]
        public IEnumerator UseASilentAudioDevice()
        {
            _now = 0f;
            _out = new ManualAudioOutput();
            _audio = new CardAudioService(_out, () => Card.Media, () => _now, () => Live.container.audio_when_another_starts);
            Card.UseAudioService(_audio);
            // - the card remembers in memory: a run never touches the developer's saved answers
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            yield break;
        }

        private List<AudioGuideBlockView> Views => Sheet.Stack.BoundViews.OfType<AudioGuideBlockView>().ToList();
        private AudioGuideBlockView Chip => Views.Single(v => v.Variant == BuiltInBlocks.AudioGuideHeroChip);
        private AudioGuideBlockView Player => Views.Single(v => v.Variant == BuiltInBlocks.AudioGuidePlayer);
        private MiniPlayerView Mini => Card.AudioCoordinator.Mini;

        // Audio time passes in half-second frames (the device's clip, the fade's clock, the service's step)
        private void Advance(float seconds)
        {
            for (float t = 0f; t < seconds - 0.001f; t += 0.5f)
            {
                float step = Mathf.Min(0.5f, seconds - t);
                _now += step;
                _out.Advance(step);
                _audio.Tick();
            }
        }

        private IEnumerator Select(string poiId)
        {
            SelectionEventBus.Select(poiId);
            yield return CardTestInput.Settle();
        }

        private IEnumerator OpenFull(string poiId)
        {
            // - selecting the point that is already selected would clear it (a second tap on a marker deselects)
            if (SelectionEventBus.CurrentPoiId != poiId) yield return Select(poiId);
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
        }

        private IEnumerator ScrollTo(IBlockView view)
        {
            Sheet.Stack.Scroll.ScrollTo(Sheet.Stack.SlotOf(view));
            yield return CardTestInput.Settle(0.2f);
        }

        private static IEnumerator Tap(VisualElement e)
        {
            yield return CardTestInput.Tap(e.panel, e.worldBound.center);
        }

        private const string CaptionOf2 = "Legenda de teste 2 de 20: texto provisório, não é a transcrição.";

        // ---------------- what the wall's cards hold ----------------

        [UnityTest]
        public IEnumerator TheLamp_HoldsAHeroChipUnderItsTitle_AndAPlayerInItsStack_OnTheCastlesRealClip()
        {
            yield return Select("lamp");
            Assert.AreEqual(SheetStopRule.Stop.Peek, Sheet.Stop, "the shipped card opens at peek");
            Assert.AreEqual(2, Views.Count, "the chip and the player");
            var chip = Chip;
            Assert.AreSame(Sheet.Stack.PinnedTop, Sheet.Stack.SlotOf(chip).parent, "the chip is pinned under the title");
            Assert.LessOrEqual(chip.Root.worldBound.yMax, Sheet.Root.worldBound.yMax + 2f, "and it shows at the peek stop, with nothing scrolled");
            Assert.LessOrEqual(chip.PlayButton.worldBound.yMax, Sheet.Root.worldBound.yMax + 1f, "its play button too");
            Assert.AreSame(Sheet.Stack.PinnedTop, Sheet.Stack.PeekPart, "the peek stop ends under the chip");
            // - The Lamp's real subtitle ("Military - c. 1700 on the panel"): the chip is pinned ABOVE it, so the peek stop -- which
            //   ends at the chip -- never drags the subtitle in with it (the same claim _3.1 already made for a header with no chip)
            var subtitle = Header.Root.Q<Label>("card-header-subtitle");
            Assert.IsTrue(Header.SubtitleShown, "precondition: The Lamp's header has a subtitle");
            Assert.GreaterOrEqual(subtitle.worldBound.yMin, chip.Root.worldBound.yMax - 1f, "the subtitle sits below the chip");
            Assert.GreaterOrEqual(subtitle.worldBound.yMin, Sheet.Root.worldBound.yMax - 1f, "...and so stays below the card's bottom edge: hidden at peek");
            Assert.IsTrue(chip.ClipAvailable, "the real narration loaded through the wall's Media Folder");
            Assert.AreEqual("0:00 / 3:20", chip.TimeText.text, "its real length: 200.39 s");
            Assert.AreEqual("Guided tour of the castle", chip.Title.text);
            Assert.AreEqual(CardAudioState.Idle, _audio.State, "opening a card plays nothing by itself");
            Assert.IsFalse(Mini.IsShown, "no mini-player while a card is open");

            yield return OpenFull("lamp");
            var player = Player;
            Assert.AreEqual("Listen", Sheet.Stack.HeadingOf(player).text, "the authored heading");
            Assert.AreEqual(20, player.Cues.Count, "the captions file loaded and parsed");
            Assert.AreEqual(DisplayStyle.Flex, player.CaptionsButton.style.display.value);
            Assert.AreEqual(DisplayStyle.Flex, player.SpeedChip.style.display.value);
            Assert.AreEqual("1x", player.SpeedChip.text);
            Assert.IsFalse(player.CaptionsShown, "captions are off until the visitor switches them on");
            Assert.AreEqual(chip.Track.ClipPath, player.Track.ClipPath, "one clip: one audio");
            Assert.AreEqual(GuidePath, player.Track.ClipPath);
        }

        [UnityTest]
        public IEnumerator TheOtherCards_HoldTheirOwn_TheToneChipOnMilitary_TheLongEnglishPlayerOnEconomic()
        {
            yield return Select("lamp_military");
            var chip = Views.Single();
            Assert.AreEqual(BuiltInBlocks.AudioGuideHeroChip, chip.Variant);
            Assert.AreEqual("0:00 / 0:20", chip.TimeText.text, "the 20 s generated tone");
            Assert.AreEqual(4, chip.Cues.Count, "with its four generated captions");

            yield return OpenFull("lamp_economic");
            var player = Views.Single();
            Assert.AreEqual(BuiltInBlocks.AudioGuidePlayer, player.Variant);
            Assert.AreEqual("0:00 / 3:52", player.TimeText.text, "the English clip's real length: 232.9 s");
            Assert.AreEqual(0, player.Cues.Count);
            Assert.AreEqual(DisplayStyle.None, player.CaptionsButton.style.display.value, "no captions file: no switch");
            yield return ScrollTo(player);
            foreach (string speed in new[] { "1.25x", "1.5x", "2x", "0.5x", "0.75x", "1x" })
            {
                yield return Tap(player.SpeedChip);
                Assert.AreEqual(speed, player.SpeedChip.text, "the Wide list: half speed to double");
            }
            Assert.AreEqual("Portugal, promotional audio", player.Title.text);
        }

        // ---------------- playing ----------------

        [UnityTest]
        public IEnumerator ARealTapOnTheChipAtPeek_Plays_TheCaptionsFollowTheClipTime_AndARealScrubMovesThemWithIt()
        {
            yield return Select("lamp");
            yield return Tap(Chip.PlayButton);
            Assert.AreEqual(CardAudioState.Playing, _audio.State, "a real tap on the chip started the narration");
            Assert.AreEqual("lamp", _audio.Current.PoiId);
            Assert.AreEqual(GuidePath, _audio.Current.ClipPath);
            Assert.IsTrue(_out.IsPlaying);
            Assert.AreEqual("castelo_s_jorge_guide_pt", _out.Clip.name, "the real clip is what the device plays");
            Assert.AreEqual(SheetStopRule.Stop.Peek, Sheet.Stop, "the card stayed at peek");
            Advance(6f);
            Assert.AreEqual("0:06 / 3:20", Chip.TimeText.text);
            yield return Capture("Lamp_Audio_Peek_Playing");

            yield return OpenFull("lamp");
            var player = Player;
            Assert.AreEqual(CardIcons.Shape.Pause, player.PlayGlyph.Kind, "the player shows what the chip started");
            Assert.AreEqual("0:06 / 3:20", player.TimeText.text);
            yield return ScrollTo(player);
            yield return Tap(player.CaptionsButton);
            Assert.IsTrue(player.CaptionsShown);
            Advance(6f);
            Assert.AreEqual(CaptionOf2, player.CaptionLine.text, "at 0:12 the second placeholder caption (10 s to 20 s), Portuguese letters intact");
            yield return Capture("Lamp_Audio_Player_Captions");

            Rect bar = player.Scrubber.Root.worldBound;
            yield return CardTestInput.Tap(player.Root.panel, new Vector2(bar.x + bar.width * 0.5f, bar.center.y));
            Assert.AreEqual(100.2f, _audio.Position, 1.5f, "a real press in the middle of the bar: half of the 200 s");
            Assert.AreEqual("Legenda de teste 11 de 20: texto provisório, não é a transcrição.", player.CaptionLine.text, "the caption line moved with the audio");
            Assert.AreEqual(CardAudioState.Playing, _audio.State);
        }

        [UnityTest]
        public IEnumerator ClosingTheCard_WithKeepAudioPlayingOn_ShowsTheMiniPlayer_ATapOnItReopensTheLampsCard_TheAudioNeverStopped()
        {
            Assert.IsTrue(Live.container.keep_audio_on_close, "precondition: the shipped setting");
            yield return Select("lamp");
            yield return Tap(Chip.PlayButton);
            Advance(8f);
            yield return Tap(Sheet.CloseButton);
            Assert.IsFalse(Sheet.IsOpen, "a real tap on the X closed the card");
            Assert.IsNull(SelectionEventBus.CurrentPoiId, "and cleared the selection");
            Assert.AreEqual(CardAudioState.Playing, _audio.State, "the narration plays on");
            Assert.IsTrue(Mini.IsShown, "the mini-player is on the wall");
            yield return CardTestInput.Settle(0.15f);
            Assert.AreEqual("Guided tour of the castle", Mini.Title.text);
            Assert.AreEqual("0:08 / 3:20", Mini.TimeText.text);
            Advance(2f);
            Assert.AreEqual("0:10 / 3:20", Mini.TimeText.text, "it follows the clip");
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(Mini.OpenButton.worldBound.width, Mini.OpenButton.worldBound.height));
            // - the AR zoom buttons keep the bottom-right corner: the mini-player must not cover them
            var zoomRoot = Object.FindFirstObjectByType<ZoomControlView>().Root.Q("zoom-control-root");
            Assert.IsNotNull(zoomRoot, "precondition: the wall has its zoom buttons");
            Assert.IsTrue(zoomRoot.worldBound.width > 0f && zoomRoot.worldBound.height > 0f, "precondition: they are laid out");
            Assert.IsFalse(Mini.Root.worldBound.Overlaps(zoomRoot.worldBound), "the mini-player " + Mini.Root.worldBound + " never covers the zoom buttons " + zoomRoot.worldBound);
            yield return Capture("Lamp_Audio_MiniPlayer");

            yield return Tap(Mini.PlayButton);
            Assert.AreEqual(CardAudioState.Paused, _audio.State, "the mini-player's button pauses it");
            Assert.IsTrue(Mini.IsShown);
            yield return Tap(Mini.PlayButton);
            Assert.AreEqual(CardAudioState.Playing, _audio.State);

            yield return Tap(Mini.OpenButton);
            yield return CardTestInput.Settle(0.3f);
            Assert.AreEqual("lamp", SelectionEventBus.CurrentPoiId, "a tap on the title selected the point again");
            Assert.AreEqual("lamp", Card.ShownPoiId, "its card is open again");
            Assert.IsTrue(Sheet.IsOpen);
            Assert.IsFalse(Mini.IsShown, "the card holds the player again");
            Assert.AreEqual(CardIcons.Shape.Pause, Chip.PlayGlyph.Kind, "the reopened card shows the audio that kept playing");
            Assert.AreEqual("0:10 / 3:20", Chip.TimeText.text, "at the same place: it never restarted");
            Assert.AreEqual(1, _out.LoadCount, "one load from the first tap to now");
            Assert.AreEqual(CardAudioState.Playing, _audio.State);
        }

        [UnityTest]
        public IEnumerator ClosingTheCard_WithKeepAudioPlayingOff_StopsTheAudio_AndNoMiniPlayerAppears()
        {
            Live.container.keep_audio_on_close = false;
            yield return Select("lamp");
            yield return Tap(Chip.PlayButton);
            Advance(3f);
            Assert.AreEqual(CardAudioState.Playing, _audio.State);
            yield return Tap(Sheet.CloseButton);
            Assert.AreEqual(CardAudioState.Idle, _audio.State, "the card closed: its narration stopped");
            Assert.IsFalse(_out.IsPlaying);
            Assert.IsFalse(Mini.IsShown);
            Assert.AreEqual(0, Card.Media.RefCount(GuidePath), "and the clip was given back");

            // - switching to another point's card stops it too (the audio belongs to the card that started it)
            yield return Select("lamp");
            yield return Tap(Chip.PlayButton);
            Assert.AreEqual(CardAudioState.Playing, _audio.State);
            yield return Select("lamp_military");
            Assert.AreEqual(CardAudioState.Idle, _audio.State, "another point's card opened: the Lamp's audio stopped with Keep off");
        }

        // ---------------- one audio at a time ----------------

        [UnityTest]
        public IEnumerator AnotherPointsAudio_InSwitchMode_FadesTheLampOutAndPlaysTheTone_InQueueMode_TheToneWaitsAndThenPlays()
        {
            Assert.AreEqual(CardOptions.AudioSwitch, Live.container.audio_when_another_starts, "precondition: the shipped setting is Switch");
            yield return Select("lamp");
            yield return Tap(Chip.PlayButton);
            Advance(4f);

            yield return Select("lamp_military");
            Assert.AreEqual(CardAudioState.Playing, _audio.State, "opening another card keeps the narration going (Keep Audio Playing)");
            Assert.IsFalse(Mini.IsShown, "a card is open, so no mini-player");
            yield return Tap(Chip.PlayButton);
            Assert.AreEqual("lamp_military", _audio.Current.PoiId, "the tone is the current audio at once");
            Assert.AreEqual(CardIcons.Shape.Pause, Chip.PlayGlyph.Kind);
            Assert.AreEqual("castelo_s_jorge_guide_pt", _out.Clip.name, "the narration is still what plays, fading");
            _now += 0.25f;
            _audio.Tick();
            Assert.AreEqual(0.5f, _out.Volume, 0.05f, "halfway through the 500 ms fade");
            _now += 0.25f;
            _audio.Tick();
            Assert.AreEqual("lamp_tone", _out.Clip.name, "the fade is over: the tone plays");
            Assert.IsTrue(_out.IsPlaying);
            Assert.AreEqual(1f, _out.Volume);
            Assert.AreEqual(0, Card.Media.RefCount(GuidePath), "the narration's clip was given back");

            // - the same again with the setting on Queue: the tone plays, the Lamp is asked for and waits
            Live.container.audio_when_another_starts = CardOptions.AudioQueue;
            yield return Select("lamp");
            yield return Tap(Chip.PlayButton);
            Assert.IsTrue(_audio.IsQueued(Chip.Track), "the narration waits for the tone");
            Assert.AreEqual("Up next", Chip.Note.text, "the chip says so");
            Assert.AreEqual("lamp_tone", _out.Clip.name, "the tone is not interrupted");
            Advance(30f);
            Assert.AreEqual("lamp", _audio.Current.PoiId, "the tone ended: the narration started by itself");
            Assert.AreEqual("castelo_s_jorge_guide_pt", _out.Clip.name);
            Assert.AreEqual(CardIcons.Shape.Pause, Chip.PlayGlyph.Kind);
            Assert.AreEqual(DisplayStyle.None, Chip.Note.resolvedStyle.display);
            Assert.AreEqual(0, _audio.Queue.Count);
        }

        // ---------------- a live edit while it plays ----------------

        private static BlockInstanceData Block(WallConfigData config, string poi, string variant) =>
            config.pois.Single(p => p.id == poi).card.blocks.Single(b => b.kind == BuiltInBlocks.AudioGuideKind && b.variant == variant);

        private IEnumerator Push(System.Action<WallConfigData> edit)
        {
            var copy = ConfigCopy();
            edit(copy);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        [UnityTest]
        public IEnumerator ALiveEditOfTheBlockWhilePlaying_KeepsThePlaybackSane_TextEditsChangeNothingOfTheSound()
        {
            yield return OpenFull("lamp");
            yield return Tap(Chip.PlayButton);
            Advance(7f);
            float position = _audio.Position;
            int loads = _out.LoadCount;

            // - the player's heading and both titles are rewritten in the Editor
            yield return Push(config =>
            {
                Block(config, "lamp", BuiltInBlocks.AudioGuidePlayer).fields.Single(f => f.key == BlockKindDefinition.HeadingField).text.Single(t => t.lang == "en").value = "Hear it";
                foreach (string variant in new[] { BuiltInBlocks.AudioGuidePlayer, BuiltInBlocks.AudioGuideHeroChip })
                    Block(config, "lamp", variant).fields.Single(f => f.key == BuiltInBlocks.AudioGuideTitleField).text.Single(t => t.lang == "en").value = "The castle, out loud";
            });
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "the card kept its stop");
            Assert.AreEqual("Hear it", Sheet.Stack.HeadingOf(Player).text, "the new heading is on the open card");
            Assert.AreEqual("The castle, out loud", Player.Title.text);
            Assert.AreEqual(CardAudioState.Playing, _audio.State, "the audio never noticed");
            Assert.AreEqual(position, _audio.Position, 0.01f, "at the same second");
            Assert.AreEqual(loads, _out.LoadCount, "not loaded again");
            Assert.AreEqual(CardIcons.Shape.Pause, Player.PlayGlyph.Kind, "the rebound player shows it playing");
            Assert.AreEqual(CardIcons.Shape.Pause, Chip.PlayGlyph.Kind, "so does the rebound chip");
            Assert.IsTrue(_audio.IsCurrent(Player.Track), "a renamed block is still the same audio");
            Advance(2f);
            Assert.AreEqual("0:09 / 3:20", Player.TimeText.text, "and it plays on");

            // - both audio blocks are deleted while it plays: the sound goes on, the mini-player shows it after the close
            yield return Push(config =>
            {
                var blocks = config.pois.Single(p => p.id == "lamp").card.blocks;
                blocks.RemoveAll(b => b.kind == BuiltInBlocks.AudioGuideKind);
            });
            Assert.AreEqual(0, Views.Count, "no audio block left on the card");
            Assert.AreEqual(CardAudioState.Playing, _audio.State, "the narration was not cut");
            Assert.AreEqual(loads, _out.LoadCount);
            yield return Tap(Sheet.CloseButton);
            Assert.IsTrue(Mini.IsShown, "the mini-player still controls it");
            Assert.AreEqual("The castle, out loud", Mini.Title.text, "under the title it started with");
        }

        [UnityTest]
        public IEnumerator ALiveEditOfTheClipWhilePlaying_MakesItAnotherAudio_TheOldOnePlaysOn_UntilTheNewOneIsAsked()
        {
            yield return OpenFull("lamp");
            yield return Tap(Chip.PlayButton);
            Advance(3f);
            yield return Push(config => Block(config, "lamp", BuiltInBlocks.AudioGuidePlayer).fields.Single(f => f.key == BuiltInBlocks.AudioGuideClipField).asset = EnglishPath);
            var player = Player;
            Assert.AreEqual(EnglishPath, player.Track.ClipPath, "the player now names the English clip");
            Assert.AreEqual(CardIcons.Shape.Play, player.PlayGlyph.Kind, "another clip is another audio: it is not the one playing");
            Assert.AreEqual(CardIcons.Shape.Pause, Chip.PlayGlyph.Kind, "the chip still names the narration and shows it playing");
            Assert.AreEqual("0:00 / 3:52", player.TimeText.text, "with its own length");
            Assert.AreEqual(GuidePath, _audio.Current.ClipPath, "the narration plays on, untouched");
            Assert.AreEqual(1, _out.LoadCount);

            yield return ScrollTo(player);
            yield return Tap(player.PlayButton);
            Assert.AreEqual(EnglishPath, _audio.Current.ClipPath, "asked for: the English clip is the current audio (Switch)");
            Advance(0.5f);
            Assert.AreEqual("portugal_tourism_guide_en", _out.Clip.name, "after the fade it is what plays");
            Assert.AreEqual(CardIcons.Shape.Play, Chip.PlayGlyph.Kind, "and the chip, still naming the narration, now shows it idle");
        }

        // ---------------- interruptions through the real MonoBehaviour ----------------

        [UnityTest]
        public IEnumerator TheRealAudioPlayerComponent_TicksTheService_AndForwardsTheAppPauseAndTheDeviceChange()
        {
            var player = Card.GetComponent<CardAudioPlayer>();
            Assert.IsNotNull(player, "the card host owns one CardAudioPlayer");
            Assert.AreSame(_audio, player.Service, "it ticks the service the card uses");
            Assert.AreEqual(1, Card.GetComponents<AudioSource>().Length, "and owns the ONE AudioSource of the card");
            Assert.AreEqual(1, Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Count(s => s.GetComponent<PoiCardHost>() != null), "no other audio source belongs to the card");

            yield return Select("lamp_military");
            yield return Tap(Chip.PlayButton);
            Advance(5f);
            int changes = 0;
            _audio.Changed += () => changes++;
            yield return null;
            yield return null;
            yield return null;
            Assert.Greater(changes, 0, "real frames tick the playing service (its players are redrawn)");

            // - the phone call: Unity sends OnApplicationPause to every MonoBehaviour
            player.SendMessage("OnApplicationPause", true);
            Assert.IsFalse(_out.IsPlaying, "the app went to the background: the narration is paused");
            Assert.AreEqual(CardAudioState.Playing, _audio.State);
            player.SendMessage("OnApplicationPause", false);
            Assert.IsTrue(_out.IsPlaying, "back: it goes on");
            Assert.AreEqual(5f, _out.Time, 0.01f, "from the same place");

            // - the earbuds change: AudioSettings.OnAudioConfigurationChanged reaches the player's handler
            int loads = _out.LoadCount;
            _out.DropPlayback();
            typeof(CardAudioPlayer).GetMethod("OnConfigurationChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(player, new object[] { true });
            Assert.AreEqual(loads + 1, _out.LoadCount, "the clip is loaded again");
            Assert.IsTrue(_out.IsPlaying);
            Assert.AreEqual(5f, _out.Time, 0.01f, "at the position it had");
        }

        // ---------------- Portuguese ----------------

        [UnityTest]
        public IEnumerator InPortuguese_TheChipThePlayerAndTheMiniPlayerSpeakPortuguese()
        {
            Live.languages = new List<string> { "pt", "en" };
            yield return Select("lamp");
            Assert.AreEqual("Visita guiada ao castelo", Chip.Title.text, "the authored Portuguese title");
            Assert.AreEqual("Reproduzir", Chip.PlayButton.tooltip);
            yield return OpenFull("lamp");
            var player = Player;
            Assert.AreEqual("Ouça", Sheet.Stack.HeadingOf(player).text);
            Assert.AreEqual("Legendas", player.CaptionsButton.text);
            yield return ScrollTo(player);
            yield return Tap(player.PlayButton);
            Assert.AreEqual("Pausar", player.PlayButton.tooltip);
            yield return Tap(player.CaptionsButton);
            Advance(3f);
            Assert.AreEqual("Legenda de teste 1 de 20: texto provisório, não é a transcrição.", player.CaptionLine.text);
            yield return Capture("Lamp_Audio_Player_PT");
            yield return Tap(Sheet.CloseButton);
            Assert.AreEqual("Abrir o cartão", Mini.OpenButton.tooltip);
            Assert.AreEqual("Visita guiada ao castelo", Mini.Title.text);
            yield return CardTestInput.Settle(0.15f);
            yield return Capture("Lamp_Audio_MiniPlayer_PT");
        }
    }
}
