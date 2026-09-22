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
    // editor -> StreamingAssets -> runtime contract holds.
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
            Assert.AreEqual(26, config.pois.Count(),
                "LivingRoom config should declare 26 POIs (18 real + 4 dev displacement fixtures + 4 dev marker-design fixtures).");

            MarkerHierarchyResolver.Configure(config.hierarchy_levels);
            try
            {
                foreach (var poi in config.pois)
                {
                    // An empty hierarchy_level_key is a legitimate authored state (dev_marker_nolevel:
                    // the "no level assigned" case) -- it resolves to MarkerHierarchyResolver.Fallback,
                    // not a resolver failure, so only a NON-empty, unresolvable key is a real bug here.
                    if (string.IsNullOrWhiteSpace(poi.hierarchy_level_key))
                        continue;
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
