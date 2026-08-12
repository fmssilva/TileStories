using System.Collections.Generic;
using NUnit.Framework;

namespace TileStories.Tests
{
    // Tier 0 tests for MarkerHierarchyResolver.ValidateHierarchyLevelSizeRange --
    // pure static validation logic, no scene, no authoring-window instance.
    public class HierarchyLevelSizeRangeTests
    {
        // Builds a HierarchyLevelEntry by key + size. priority is left unset so
        // these tests never exercise positional priority fallback (that belongs
        // to MarkerHierarchyResolverTests).
        private static global::TileStories.HierarchyLevelEntry Make(string key, float size)
            => new global::TileStories.HierarchyLevelEntry
            {
                key = key,
                size_cm = size
            };

        [Test]
        public void InRange_IncLudingBounds_ReturnsNoIssues()
        {
            var levels = new List<global::TileStories.HierarchyLevelEntry>
            {
                Make("level_1", 3f),
                Make("level_2", 50f),
                Make("level_3", 100f), // upper bound valid
                Make("level_4", 0.5f)  // lower bound valid
            };
            var issues = global::TileStories.Editor.POIAuthoringToolWindow.ValidateHierarchyLevelSizeRange(levels);
            Assert.IsEmpty(issues);
        }

        [Test]
        public void OutOfRange_ReportsEachBadLevel()
        {
            var levels = new List<global::TileStories.HierarchyLevelEntry>
            {
                Make("bad_small", 0.02f), // m/cm typo
                Make("ok", 5f),
                Make("bad_large", 200f)   // m/cm typo
            };
            var issues = global::TileStories.Editor.POIAuthoringToolWindow.ValidateHierarchyLevelSizeRange(levels);
            Assert.AreEqual(2, issues.Count);
            CollectionAssert.AreEquivalent(
                new[] { "bad_small", "bad_large" },
                new[] { issues[0].poiId, issues[1].poiId });
        }

        [Test]
        public void NullList_ReturnsNoIssues()
        {
            Assert.IsEmpty(global::TileStories.Editor.POIAuthoringToolWindow.ValidateHierarchyLevelSizeRange(null));
        }

        [Test]
        public void EmptyList_ReturnsNoIssues()
        {
            Assert.IsEmpty(global::TileStories.Editor.POIAuthoringToolWindow.ValidateHierarchyLevelSizeRange(new List<global::TileStories.HierarchyLevelEntry>()));
        }

        [Test]
        public void NullEntriesWithinList_AreSkipped()
        {
            var levels = new List<global::TileStories.HierarchyLevelEntry>
            {
                null,
                Make("bad", 250f),
                null
            };
            var issues = global::TileStories.Editor.POIAuthoringToolWindow.ValidateHierarchyLevelSizeRange(levels);
            Assert.AreEqual(1, issues.Count);
            Assert.AreEqual("bad", issues[0].poiId);
        }
    }
}
