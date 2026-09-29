using System;

namespace TileStories
{
    // Joins the card's audio owner to the card's comings and goings (_3.1 step 9A), so PoiCardHost stays thin: it tells this a card was
    // shown or closed, and this applies the wall's Keep Audio Playing (MiniPlayerRule.StopsWithCard) and keeps the mini-player drawn as
    // MiniPlayerRule.IsVisible says. Plain C#; both hosts (the wall's PoiCardHost and the Phase A harness) build one, so the card behaves
    // the same in both.
    public sealed class CardAudioCoordinator
    {
        public ICardAudio Audio { get; }
        public MiniPlayerView Mini { get; }

        private readonly Func<CardSettings> _settings;
        private readonly Func<bool> _cardOpen;

        // `settings`: the wall's card settings now (a live edit of Keep Audio Playing reaches the next close); `cardOpen`: whether a card is open
        public CardAudioCoordinator(ICardAudio audio, MiniPlayerView mini, Func<CardSettings> settings, Func<bool> cardOpen)
        {
            Audio = audio;
            Mini = mini;
            _settings = settings;
            _cardOpen = cardOpen;
            Audio.Changed += Refresh;
        }

        // A card was shown for `poiId` in `strings`' language: audio that belongs to another point stops with Keep Audio Playing off
        public void CardShown(string poiId, CardStrings strings)
        {
            if (MiniPlayerRule.StopsWithCard(KeepAudio(), Audio.Current?.PoiId, poiId)) Audio.Stop();
            Mini.SetStrings(strings);
            Refresh();
        }

        // The card closed: with Keep Audio Playing off its audio stops; otherwise the mini-player takes over
        public void CardClosed()
        {
            if (MiniPlayerRule.StopsWithCard(KeepAudio(), Audio.Current?.PoiId, null)) Audio.Stop();
            Refresh();
        }

        // Stop everything (the host is switched off): nothing may play with no card and no mini-player to control it
        public void Shutdown()
        {
            Audio.Stop();
            Refresh();
        }

        // Redraw the mini-player for the audio and the card as they stand
        public void Refresh() => Mini.Refresh(MiniPlayerRule.IsVisible(Audio.State != CardAudioState.Idle, KeepAudio(), _cardOpen()));

        private bool KeepAudio() => _settings()?.container.keep_audio_on_close ?? true;
    }
}
