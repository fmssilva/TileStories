namespace TileStories
{
    // What the card's two players do to each other (_3.1 step 9B), pure: the card never plays two sounds at once. Starting a video (with its
    // sound: a header's muted loop does not count) pauses the card's audio; starting the audio pauses the video. Decided from how both stood
    // before a change and how they stand now, so only the one that just STARTED wins; if both start in the same step, the video does (the
    // visitor's last tap was on the picture). Nothing is stopped: the paused one resumes where it was when the visitor asks for it again.
    public static class CardSoundRule
    {
        public enum Step
        {
            None,
            PauseAudio,
            PauseVideo,
        }

        // `videoPlaying`: the visitor's video plays (a header loop is muted and is never a reason to pause anything)
        public static Step Decide(bool audioWasPlaying, bool audioPlaying, bool videoWasPlaying, bool videoPlaying)
        {
            bool videoStarted = videoPlaying && !videoWasPlaying;
            bool audioStarted = audioPlaying && !audioWasPlaying;
            if (videoStarted && audioPlaying) return Step.PauseAudio;
            if (audioStarted && videoPlaying) return Step.PauseVideo;
            return Step.None;
        }
    }
}
