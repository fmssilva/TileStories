namespace TileStories
{
    // Where this device keeps what its visitor did and picked between launches: the card's answers, votes, revealed blocks, language and
    // light / dark (CardLocalState), the search's recent queries (RecentSearchesManager) and its last result view (ViewModeControl). ONE
    // store for the whole app, PlayerPrefs (App), and every reader asks HERE -- never PlayerPrefs itself -- so a test run hands everything a
    // store of its own (each test assembly's SetUpFixture, IsolatedVisitorState) and never reads or clears the developer's saved state: a
    // "dark" picked while playing by hand once coloured a test's card (the S2c review).
    public static class DeviceStateStore
    {
        // The app's store: PlayerPrefs, every write saved at once
        public static readonly ICardStateStore App = new PlayerPrefsCardStateStore();

        // The store every reader uses now (App unless a test run replaced it; null puts App back)
        public static ICardStateStore Current
        {
            get => _current;
            set => _current = value ?? App;
        }
        private static ICardStateStore _current = App;
    }
}
