using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Tests
{
    // SECTION 14 / 16 L763: end-to-end check that the shipped
    // StreamingAssets/LivingRoom/config.json loads, deserialises, and every POI's
    // hierarchy_level_key resolves against the baked level table -- i.e. the
    // authoring -> StreamingAssets -> runtime contract holds.
    public class LivingRoomConfigIntegrationTests
    {
        [UnityTest]
        public IEnumerator LoadRealLivingRoomConfig_SpawnsAllPoisAndResolvesHierarchy()
        {
            WallConfigData config = null;
            var loader = WallConfigLoader.LoadFromStreamingAssets("LivingRoom/config.json",
                c => config = c);
            yield return loader;

            Assert.IsNotNull(config, "StreamingAssets/LivingRoom/config.json must load at runtime.");
            Assert.IsNotNull(config.pois, "config.pois must not be null.");
            Assert.IsNotNull(config.hierarchy_levels, "config.hierarchy_levels must not be null.");

            Assert.AreEqual(5, config.hierarchy_levels.Count(),
                "LivingRoom config should declare 5 framework-default hierarchy levels.");
            Assert.AreEqual(18, config.pois.Count(), "LivingRoom config should declare 18 POIs.");

            MarkerHierarchyResolver.Configure(config.hierarchy_levels);
            try
            {
                foreach (var poi in config.pois)
                {
                    Assert.IsTrue(MarkerHierarchyResolver.TryResolveByKey(poi.hierarchy_level_key, out _),
                        $"POI '{poi.id}' has unresolvable hierarchy_level_key '{poi.hierarchy_level_key}'.");
                }
            }
            finally
            {
                MarkerHierarchyResolver.ResetToDefaults();
            }
        }
    }
}
