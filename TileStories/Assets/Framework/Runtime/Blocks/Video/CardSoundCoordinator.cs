using System;

namespace TileStories
{
    // Joins the card's two owners (_3.1 step 9B): watches the audio and the video owners' Changed and applies CardSoundRule, so neither owner
    // knows the other and no view pauses anything itself. Plain C#; both hosts (the wall's PoiCardHost and the Phase A harness) build one.
    public sealed class CardSoundCoordinator : IDisposable
    {
        private readonly ICardAudio _audio;
        private readonly ICardVideo _video;
        private bool _audioWasPlaying;
        private bool _videoWasPlaying;

        public CardSoundCoordinator(ICardAudio audio, ICardVideo video)
        {
            _audio = audio;
            _video = video;
            _audioWasPlaying = AudioPlaying();
            _videoWasPlaying = VideoPlaying();
            if (_audio != null) _audio.Changed += Evaluate;
            if (_video != null) _video.Changed += Evaluate;
        }

        public void Dispose()
        {
            if (_audio != null) _audio.Changed -= Evaluate;
            if (_video != null) _video.Changed -= Evaluate;
        }

        private void Evaluate()
        {
            bool audio = AudioPlaying(), video = VideoPlaying();
            var step = CardSoundRule.Decide(_audioWasPlaying, audio, _videoWasPlaying, video);
            // - remember the new state BEFORE acting: the pause raises Changed again, and that nested call must see the settled state
            _audioWasPlaying = audio;
            _videoWasPlaying = video;
            if (step == CardSoundRule.Step.PauseAudio) _audio.Pause();
            else if (step == CardSoundRule.Step.PauseVideo) _video.Pause();
        }

        private bool AudioPlaying() => _audio != null && _audio.State == CardAudioState.Playing;

        private bool VideoPlaying() => _video != null && _video.State == CardVideoState.Playing && !_video.PlayingLoop;
    }
}
