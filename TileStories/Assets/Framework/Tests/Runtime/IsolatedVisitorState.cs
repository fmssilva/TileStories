using System;

namespace TileStories.Tests
{
    // PUBLIC test helper: while it lives, everything the app keeps for its visitor (the card's answers, votes, language, light / dark,
    // the search's recent queries and last view: DeviceStateStore) goes to a store of its own in memory, never the developer's
    // PlayerPrefs -- a run neither reads what the developer saved by playing by hand nor clears it (the S2c review). Every test assembly's
    // SetUpFixture holds one for the whole run; a scene fixture starts each test on a fresh one. Dispose gives the previous store back.
    public sealed class IsolatedVisitorState : IDisposable
    {
        private readonly ICardStateStore _previous;

        // What the code under test stored while this one was current
        public MemoryCardStateStore Store { get; } = new();

        public IsolatedVisitorState()
        {
            _previous = DeviceStateStore.Current;
            DeviceStateStore.Current = Store;
        }

        public void Dispose()
        {
            if (ReferenceEquals(DeviceStateStore.Current, Store)) DeviceStateStore.Current = _previous;
        }
    }
}
