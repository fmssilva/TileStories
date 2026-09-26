using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // The LOD demo field's placement (DemoFieldLayout), one Editor Tab control at a time: every test
    // changes ONE control of LOD > Test > Add LOD demo field and asserts exactly what that control must
    // change in the generated field and nothing else. Pure: the spawner and these tests read the same list.
    public class DemoFieldLayoutTests
    {
        private static readonly List<HierarchyLevelEntry> Levels = new()
        {
            new HierarchyLevelEntry { key = "top", level_name = "Top" },
            new HierarchyLevelEntry { key = "mid", level_name = "" },     // no name: the key is the label
            new HierarchyLevelEntry { key = "low", level_name = "Low" },
        };
        private static readonly List<string> Categories = new() { "a", "b" };

        private static DemoFieldSettings Settings(int perLevel = 5, int clump = 6) => new()
        {
            enabled = true, seed = 3, distance_m = 1.5f, width_m = 6f, height_m = 2.5f, depth_m = 8f,
            dense_clump_count = clump, dense_clump_radius_m = 0.08f,
            level_counts = Levels.Select(l => new DemoFieldLevelCount { level_key = l.key, count = perLevel }).ToList(),
        };

        private static List<DemoFieldLayout.Entry> Build(DemoFieldSettings s) => DemoFieldLayout.Build(s, Levels, Categories);
        private static List<DemoFieldLayout.Entry> Scattered(List<DemoFieldLayout.Entry> e) => e.Where(x => !x.InClump).ToList();
        private static List<DemoFieldLayout.Entry> Clump(List<DemoFieldLayout.Entry> e) => e.Where(x => x.InClump).ToList();

        [Test]
        public void MarkersPerHierarchyLevel_EachSliderSetsExactlyItsOwnLevelsCount()
        {
            var s = Settings(perLevel: 0, clump: 0);
            s.level_counts.First(r => r.level_key == "mid").count = 7;
            var entries = Build(s);
            Assert.AreEqual(7, entries.Count(e => e.LevelKey == "mid"), "the 'mid' slider asks for 7");
            Assert.AreEqual(0, entries.Count(e => e.LevelKey != "mid"), "the other levels stay at 0");
            Assert.IsTrue(entries.All(e => e.Name.StartsWith("mid ")), "a level without a name is labelled by its key");

            s.level_counts.First(r => r.level_key == "top").count = 2;
            entries = Build(s);
            Assert.AreEqual(2, entries.Count(e => e.LevelKey == "top"));
            Assert.IsTrue(entries.Where(e => e.LevelKey == "top").All(e => e.Name.StartsWith("Top ")), "named levels use their name");
        }

        [Test]
        public void MarkersPerHierarchyLevel_AreClampedToTheSliderMaximum_AndALevelWithoutARowIsZero()
        {
            var s = Settings(perLevel: 0, clump: 0);
            s.level_counts = new List<DemoFieldLevelCount> { new() { level_key = "low", count = 1000 } };
            var entries = Build(s);
            Assert.AreEqual(DemoFieldSettings.MaxCountPerLevel, entries.Count(e => e.LevelKey == "low"));
            Assert.AreEqual(0, entries.Count(e => e.LevelKey == "top"), "no row = 0 markers");
        }

        [Test]
        public void DenseClump_AddsExactlyThatManyPackedMarkers_SpreadOverTheLevels()
        {
            var entries = Build(Settings(perLevel: 1, clump: 9));
            var clump = Clump(entries);
            Assert.AreEqual(9, clump.Count);
            CollectionAssert.AreEquivalent(Levels.Select(l => l.key), clump.Select(c => c.LevelKey).Distinct(),
                "clump members cycle through every level, so priority matters inside the clump too");
            Assert.AreEqual(3, Scattered(entries).Count, "the clump never changes the scattered markers");

            var s = Settings(perLevel: 1, clump: 1000);
            Assert.AreEqual(DemoFieldSettings.MaxClumpCount, Clump(Build(s)).Count, "clamped to the slider maximum");
        }

        [TestCase(0.02f)]
        [TestCase(0.3f)]
        public void ClumpRadius_KeepsEveryClumpMarkerInsideThatRadius(float radius)
        {
            var s = Settings(perLevel: 0, clump: 20);
            s.dense_clump_radius_m = radius;
            var clump = Clump(Build(s));
            var centre = new Vector3(0f, 0f, s.distance_m + s.depth_m * 0.25f);
            foreach (var c in clump)
            {
                var offset = c.LocalPosition - centre;
                Assert.LessOrEqual(new Vector2(offset.x, offset.y).magnitude, radius + 1e-4f, c.Id + ": across the view");
                Assert.LessOrEqual(Mathf.Abs(offset.z), radius * 0.25f + 1e-4f, c.Id + ": in depth");
            }
            float spread = clump.Max(a => clump.Max(b => (a.LocalPosition - b.LocalPosition).magnitude));
            Assert.Greater(spread, radius * 0.5f, "the markers really use the radius (not all on one point)");
        }

        [Test]
        public void FieldDistanceAndDepth_BoundTheScatteredMarkersAlongTheView()
        {
            var s = Settings(perLevel: 20, clump: 0);
            s.distance_m = 3f;
            s.depth_m = 4f;
            var scattered = Scattered(Build(s));
            Assert.IsTrue(scattered.All(e => e.LocalPosition.z >= 3f - 1e-4f && e.LocalPosition.z <= 7f + 1e-4f),
                "every marker between Field Distance (3 m) and Field Distance + Field Depth (7 m)");
            Assert.Less(scattered.Min(e => e.LocalPosition.z), 4f, "the near part of the box is used");
            Assert.Greater(scattered.Max(e => e.LocalPosition.z), 6f, "and the far part");
        }

        [Test]
        public void FieldWidthAndHeight_BoundTheScatteredMarkersAcrossTheView()
        {
            var s = Settings(perLevel: 20, clump: 0);
            s.width_m = 2f;
            s.height_m = 0.5f;
            var scattered = Scattered(Build(s));
            Assert.IsTrue(scattered.All(e => Mathf.Abs(e.LocalPosition.x) <= 1f + 1e-4f), "half the Field Width each side");
            Assert.IsTrue(scattered.All(e => Mathf.Abs(e.LocalPosition.y) <= 0.25f + 1e-4f), "half the Field Height up and down");

            s.width_m = 12f;
            Assert.Greater(Scattered(Build(s)).Max(e => Mathf.Abs(e.LocalPosition.x)), 3f, "a wider field really spreads wider");
        }

        [Test]
        public void Reshuffle_SameSettingsSameField_NewSeedNewPlacesSameMarkers()
        {
            var s = Settings();
            var first = Build(s);
            var again = Build(s);
            CollectionAssert.AreEqual(first.Select(e => e.LocalPosition), again.Select(e => e.LocalPosition), "same seed = same field");

            s.seed++;   // what the Reshuffle button does
            var shuffled = Build(s);
            CollectionAssert.AreEqual(first.Select(e => e.Id), shuffled.Select(e => e.Id), "the same markers");
            CollectionAssert.AreNotEqual(first.Select(e => e.LocalPosition), shuffled.Select(e => e.LocalPosition), "in new places");
        }

        [Test]
        public void Categories_CycleThroughTheWallsCategories_SoAClusterPieHasSeveralSlices()
        {
            var entries = Build(Settings(perLevel: 2, clump: 4));
            CollectionAssert.AreEquivalent(Categories, entries.Select(e => e.Category).Distinct());
        }
    }
}
