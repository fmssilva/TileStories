using NUnit.Framework;

namespace TileStories.Tests
{
    // ThemeModeRule (_3.3 P4.4b): the visitor's pick, then the wall's Default Mode, then the theme's -- each only when the theme has it
    public class ThemeModeRuleTests
    {
        private static readonly string[] Modes = { "light", "dark" };

        [Test]
        public void TheVisitorsPick_Wins_ThenTheWallsDefault_ThenTheThemes()
        {
            Assert.AreEqual("dark", ThemeModeRule.Shown(Modes, "dark", "light", "light"), "the visitor's pick beats the wall's default");
            Assert.AreEqual("light", ThemeModeRule.Shown(Modes, "light", "dark", "dark"), "and the other way round");
            Assert.AreEqual("dark", ThemeModeRule.Shown(Modes, "", "dark", "light"), "no pick: the wall's Default Mode");
            Assert.AreEqual("light", ThemeModeRule.Shown(Modes, null, null, "light"), "no pick, no wall value (an old config): the theme's");
        }

        [Test]
        public void AModeTheThemeLacks_IsSkipped_NeverShown()
        {
            Assert.AreEqual("dark", ThemeModeRule.Shown(Modes, "sepia", "dark", "light"), "an unknown pick falls through to the wall's");
            Assert.AreEqual("light", ThemeModeRule.Shown(Modes, "sepia", "night", "light"), "an unknown wall value falls through to the theme's");
            Assert.AreEqual("light", ThemeModeRule.Shown(Modes, "", "", "sepia"), "nothing known: the theme's first mode");
            Assert.AreEqual("", ThemeModeRule.Shown(new string[0], "dark", "dark", "dark"), "a theme with no mode: nothing");
        }
    }
}
