using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The conditions of Detail Card > Navigation's notes (_3.2 section 7), pure, read from the REAL card maps of the shipped wall.
    // (The Navigation Bar's own "more groups than a phone's bar holds" note is NavigationBarEditorTabTests'.)
    public class NavigationWarningRuleTests
    {
        private static WallConfigData Shipped() =>
            JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));

        [Test]
        public void CardShowsNavigation_IsTheCardsOwnAnswer_ForALongCardAndForOneWithNoBlocks()
        {
            var config = Shipped();
            int lamp = config.pois.FindIndex(p => p.id == "lamp");
            int blockless = config.pois.FindIndex(p => p.card.blocks.Count == 0);
            Assert.GreaterOrEqual(blockless, 0, "precondition: the wall has points with no blocks (25 of 26)");
            Assert.IsTrue(NavigationWarningRule.CardShowsNavigation(config.card_settings, config.pois, lamp, BlockRegistry.Shared));
            Assert.IsFalse(NavigationWarningRule.CardShowsNavigation(config.card_settings, config.pois, blockless, BlockRegistry.Shared));
            Assert.IsFalse(NavigationWarningRule.CardShowsNavigation(config.card_settings, config.pois, -1, BlockRegistry.Shared));
            Assert.IsFalse(NavigationWarningRule.CardShowsNavigation(config.card_settings, config.pois, config.pois.Count, BlockRegistry.Shared));
        }
    }
}
