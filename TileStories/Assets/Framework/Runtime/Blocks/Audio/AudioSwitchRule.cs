namespace TileStories
{
    // What happens when the visitor starts an audio while another one is going (_3.1 step 9A; the work plan's Stage 2 item 3: never two
    // clips at once, "either fades out the current clip and plays the new one, or queues it, per a user-facing setting"). The setting is
    // card_settings.container.audio_when_another_starts (CardOptions.AudioSwitch / AudioQueue). Pure.
    public static class AudioSwitchRule
    {
        public enum Decision
        {
            // Nothing is playing (or only a paused clip is held): the new audio starts now
            StartNow,
            // The playing audio fades out (AudioFadeRule), then the new one starts
            FadeThenStart,
            // The playing audio finishes; the new one waits its turn
            Queue,
        }

        // The seconds the playing audio takes to fade out before the new one starts (work plan: 500 ms)
        public const float FadeSeconds = 0.5f;

        // `playing`: an audio is playing right now (a paused one does not block a new one: the visitor moved on)
        public static Decision Decide(string mode, bool playing)
        {
            if (!playing) return Decision.StartNow;
            return mode == CardOptions.AudioQueue ? Decision.Queue : Decision.FadeThenStart;
        }

        // The volume of the fading audio `elapsed` seconds into the fade: 1 down to 0, straight
        public static float FadeVolume(float elapsed)
        {
            if (!(elapsed > 0f)) return 1f;
            return elapsed >= FadeSeconds ? 0f : 1f - elapsed / FadeSeconds;
        }

        public static bool FadeFinished(float elapsed) => elapsed >= FadeSeconds;
    }
}
