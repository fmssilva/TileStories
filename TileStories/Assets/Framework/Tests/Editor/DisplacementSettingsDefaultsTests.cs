using NUnit.Framework;

namespace TileStories.Tests
{
    // Tier-0 EditMode test: asserts the DisplacementSettings schema contract
    // (spec _2.5 section 10). Pure-data -- constructs new DisplacementSettings()
    // directly, no scene and no MonoBehaviour. The WallSession.DisplacementSettings
    // accessor merely projects _config?.displacement_settings (mirrors LodSettings) and
    // is covered transitively once Block 6 wires the resolver against it.
    public class DisplacementSettingsDefaultsTests
    {
        private DisplacementSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _settings = new DisplacementSettings();
        }

        [Test]
        public void Defaults_MatchSpec10()
        {
            Assert.IsTrue(_settings.enabled);
            Assert.AreEqual(40f, _settings.overlap_threshold_px);
            Assert.AreEqual("label_only", _settings.displace_target);
            Assert.AreEqual("force_directed", _settings.displacement_algorithm);
            Assert.AreEqual(4, _settings.force_directed_iterations);
            Assert.AreEqual(120f, _settings.max_displacement_px);
            Assert.IsTrue(_settings.leader_lines_enabled);
            Assert.AreEqual("straight", _settings.leader_line_style);
            Assert.AreEqual(15f, _settings.leader_line_min_distance_px);
            Assert.AreEqual(0.01f, _settings.leader_line_width, 0.001f);
            Assert.AreEqual(1.0f, _settings.leader_line_opacity, 0.001f);
            Assert.AreEqual("symmetric", _settings.displacement_tiebreak);
        }

        [Test]
        public void WallLevelField_IsInitialisedByDefault()
        {
            var data = new WallConfigData();

            Assert.IsNotNull(data.displacement_settings);
            Assert.AreEqual(40f, data.displacement_settings.overlap_threshold_px);
            Assert.AreEqual("label_only", data.displacement_settings.displace_target);
        }
    }
}
