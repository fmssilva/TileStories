namespace TileStories
{
    // When the mini-player on the wall is drawn (_3.1 step 9A, spec section 2 point 5): an audio started in the card keeps playing after
    // the card closes ("Keep Audio Playing", card_settings.container.keep_audio_on_close) and a small player stays on the wall until it
    // ends or another audio replaces it. Pure.
    public static class MiniPlayerRule
    {
        // Drawn while an audio is active (playing or paused), the wall keeps it, and no card is open to hold its own player
        public static bool IsVisible(bool audioActive, bool keepAudioOnClose, bool cardOpen) => audioActive && keepAudioOnClose && !cardOpen;

        // What closing (or switching away from) a card does to the audio that belongs to it: with Keep Audio Playing off the card's audio
        // stops with the card; audio that belongs to the card now being shown is never stopped
        public static bool StopsWithCard(bool keepAudioOnClose, string audioPoiId, string cardPoiIdNowShown) =>
            !keepAudioOnClose && audioPoiId != null && audioPoiId != cardPoiIdNowShown;
    }
}
