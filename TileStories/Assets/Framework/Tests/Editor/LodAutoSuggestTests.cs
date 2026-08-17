using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // Tier 0 pure-logic tests for LodAutoSuggest (section 6.2 of _2.4).
    // No scene, no EditorWindow -- just Assert on the returned LodSettings.

    public class LodAutoSuggestTests
    {
        [Test]
        public void Suggest_10Pois_ProducesSpecBandsAndThresholds()
        {
            // n=10: outer=5, middle=10, clusterMin=3, shrinkStart=2
            var lod = LodAutoSuggest.Suggest(10);
            Assert.AreEqual(3, lod.bands.Count);
            Assert.AreEqual(2f, lod.bands[0].max_distance_m); Assert.AreEqual(-1, lod.bands[0].max_visible_count);
            Assert.AreEqual(7f, lod.bands[1].max_distance_m); Assert.AreEqual(10, lod.bands[1].max_visible_count);
            Assert.AreEqual(9999f, lod.bands[2].max_distance_m); Assert.AreEqual(5, lod.bands[2].max_visible_count);
            Assert.AreEqual(3, lod.cluster_min_count); Assert.AreEqual(2, lod.shrink_start_neighbor_count);
        }

        [Test]
        public void Suggest_18Pois_ProducesSpecBandsAndThresholds()
        {
            // n=18: outer=5, middle=15, clusterMin=max(3, RoundToInt(1.8)=2)=3, shrinkStart=max(2,1)=2
            var lod = LodAutoSuggest.Suggest(18);
            Assert.AreEqual(3, lod.bands.Count);
            Assert.AreEqual(2f, lod.bands[0].max_distance_m); Assert.AreEqual(-1, lod.bands[0].max_visible_count);
            Assert.AreEqual(7f, lod.bands[1].max_distance_m); Assert.AreEqual(15, lod.bands[1].max_visible_count);
            Assert.AreEqual(9999f, lod.bands[2].max_distance_m); Assert.AreEqual(5, lod.bands[2].max_visible_count);
            Assert.AreEqual(3, lod.cluster_min_count); Assert.AreEqual(2, lod.shrink_start_neighbor_count);
        }

        [Test]
        public void Suggest_150Pois_ClampsOuterAndMiddle_ScalesThresholds()
        {
            // n=150: outer=5, middle=15, clusterMin=15, shrinkStart=7
            var lod = LodAutoSuggest.Suggest(150);
            Assert.AreEqual(3, lod.bands.Count);
            Assert.AreEqual(2f, lod.bands[0].max_distance_m); Assert.AreEqual(-1, lod.bands[0].max_visible_count);
            Assert.AreEqual(7f, lod.bands[1].max_distance_m); Assert.AreEqual(15, lod.bands[1].max_visible_count);
            Assert.AreEqual(9999f, lod.bands[2].max_distance_m); Assert.AreEqual(5, lod.bands[2].max_visible_count);
            Assert.AreEqual(15, lod.cluster_min_count); Assert.AreEqual(7, lod.shrink_start_neighbor_count);
        }

        [Test]
        public void Suggest_ZeroPois_ClampsViaMaxFloors()
        {
            // n=0: outer=0, middle=0, clusterMin=max(3,0)=3, shrinkStart=max(2,1)=2
            var lod = LodAutoSuggest.Suggest(0);
            Assert.AreEqual(3, lod.bands.Count);
            Assert.AreEqual(-1, lod.bands[0].max_visible_count);
            Assert.AreEqual(0, lod.bands[1].max_visible_count);
            Assert.AreEqual(0, lod.bands[2].max_visible_count);
            Assert.AreEqual(3, lod.cluster_min_count);
            Assert.AreEqual(2, lod.shrink_start_neighbor_count);
        }

        [Test]
        public void Suggest_AlwaysThreeExplicitBands_ThresholdsNeverDropBelowFloor()
        {
            for (int n = 0; n <= 200; n++)
            {
                var lod = LodAutoSuggest.Suggest(n);
                Assert.AreEqual(3, lod.bands.Count, $"n={n}");
                Assert.AreEqual(2f, lod.bands[0].max_distance_m, $"n={n}");
                Assert.AreEqual(7f, lod.bands[1].max_distance_m, $"n={n}");
                Assert.AreEqual(9999f, lod.bands[2].max_distance_m, $"n={n}");
                Assert.IsTrue(lod.cluster_min_count >= 3, $"n={n} cluster_min_count floored below 3");
                Assert.IsTrue(lod.shrink_start_neighbor_count >= 2, $"n={n} shrink_start floored below 2");
            }
        }
    }
}
