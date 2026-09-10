using NUnit.Framework;

namespace TileStories.Tests
{
    // Tier-0 tests for the Select/Filter/Search domain master toggle (_2_6 section 3
    // via _2.7 entry 2.6-d). Pure config/gate logic -- no scene, no UI, no mocks.
    public class SearchFilterSelectToggleTests
    {
        [Test]
        public void MasterToggle_DefaultsToEnabled()
        {
            // JsonUtility backfills missing JSON fields from field initializers, so every
            // existing wall config (LivingRoom included) loads with the domain ON.
            var config = new WallConfigData();
            Assert.IsTrue(config.search_filter_select_enabled);
        }

        [Test]
        public void Gate_NullConfig_WiresResponders()
        {
            // Null config must not silently disable selection (fail-open, matching
            // how the rest of WallSession treats a missing config defensively).
            Assert.IsTrue(WallSession.ShouldWireSelectionResponders(null));
        }

        [Test]
        public void Gate_EnabledConfig_WiresResponders()
        {
            var config = new WallConfigData { search_filter_select_enabled = true };
            Assert.IsTrue(WallSession.ShouldWireSelectionResponders(config));
        }

        [Test]
        public void Gate_DisabledConfig_NeverWiresResponders()
        {
            var config = new WallConfigData { search_filter_select_enabled = false };
            Assert.IsFalse(WallSession.ShouldWireSelectionResponders(config));
        }
    }
}