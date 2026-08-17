namespace TileStories
{
    // Resolves which ITranscriber the app should use at runtime.
    //
    // The YasirkulaTranscriber adapter (device backend wrapping
    // yasirkula/UnitySpeechToText) is DEFERRED -- it only compiles once the
    // package is imported. Until then, and when voice search is disabled,
    // DebugTranscriber is always returned, so the entire voice-search stack is
    // exercised offline. Swap the single branch below for
    // `new YasirkulaTranscriber()` once the plugin ships -- VoiceSearchController
    // never changes. (spec _2.6 section 12)
    public interface ITranscriberFactory
    {
        ITranscriber Create(bool voiceEnabled);
    }

    public sealed class TranscriberFactory : ITranscriberFactory
    {
        // Returns DebugTranscriber unconditionally in this build. The Yasirkula
        // branch is intentionally omitted (not yet built) to avoid a hard
        // dependency on a package that isn't installed.
        public ITranscriber Create(bool voiceEnabled) => new DebugTranscriber();
    }
}
