namespace TileStories.Tests
{
    // PUBLIC: the reasons a whole fixture is switched off ([Ignore(TestSkips.X)]), one per decision, so every skipped fixture says the same
    // thing and a grep for the constant finds them all (_Test_Suite_1.md lists them)
    public static class TestSkips
    {
        // The card's OLD navigation elements (Top / Bottom / Menu / Floating / Edge elements, the navigation presets) are drawn only while the
        // Navigation Bar is off, and the bar is on by default since _3.3 S4 third round; the developer rebuilds them later on the bar's
        // pattern. Their fixtures wait for that rebuild instead of testing what the card no longer shows by default
        public const string OldNavigationElements =
            "Old navigation element: rebuilt later on the Navigation Bar's pattern (_3.3 S4 third round); the bar is on by default";
    }
}
