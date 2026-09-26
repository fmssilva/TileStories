namespace TileStories
{
    // Which speech-to-text backend voice search uses (spec _2.6 section 12). The Editor gets the
    // DebugTranscriber (a fixed test phrase, no microphone). A device gets none until a real backend is
    // added here (the planned adapter wraps yasirkula/UnitySpeechToText), so on a device the mic stays
    // hidden instead of pretending to listen.
    public static class TranscriberFactory
    {
        public static ITranscriber Create(bool isEditor) => isEditor ? new DebugTranscriber() : null;
    }
}
