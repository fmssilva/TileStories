using System.Collections.Generic;
using NUnit.Framework;

namespace TileStories.Tests
{
    // Tier 0 tests for MarkerHierarchyResolver.ValidateHierarchyLevelSizeRange --
    // pure static validation logic, no scene, no editor-window instance.
    public class HierarchyLevelSizeRangeTests
    {
        // Builds a HierarchyLevelEntry by key + size. priority is left unset so
        // these tests never exercise positional priority fallback (that belongs
        // to MarkerHierarchyResolverTests).
        private static global::TileStories.HierarchyLevelEntry Make(string key, float size, string name = null)
            => new global::TileStories.HierarchyLevelEntry
            {
                key = key,
                level_name = name,
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
            var issues = global::TileStories.Editor.POIEditorToolWindow.ValidateHierarchyLevelSizeRange(levels);
            Assert.IsEmpty(issues);
        }

        [Test]
        public void OutOfRange_ReportsEachBadLevel()
        {
            var levels = new List<global::TileStories.HierarchyLevelEntry>
            {
                Make("level_1", 0.02f, "Tiny"), // m/cm typo
                Make("level_2", 5f, "Ok"),
                Make("level_3", 200f, "Huge")   // m/cm typo
            };
            var issues = global::TileStories.Editor.POIEditorToolWindow.ValidateHierarchyLevelSizeRange(levels);
            Assert.AreEqual(2, issues.Count);
            // - named the way the Hierarchy Levels table shows the row, never by its generated key
            CollectionAssert.AreEquivalent(
                new[] { "Hierarchy level 'Tiny'", "Hierarchy level 'Huge'" },
                new[] { issues[0].subject, issues[1].subject });
        }

        [Test]
        public void NullList_ReturnsNoIssues()
        {
            Assert.IsEmpty(global::TileStories.Editor.POIEditorToolWindow.ValidateHierarchyLevelSizeRange(null));
        }

        [Test]
        public void EmptyList_ReturnsNoIssues()
        {
            Assert.IsEmpty(global::TileStories.Editor.POIEditorToolWindow.ValidateHierarchyLevelSizeRange(new List<global::TileStories.HierarchyLevelEntry>()));
        }

        [Test]
        public void NullEntriesWithinList_AreSkipped()
        {
            var levels = new List<global::TileStories.HierarchyLevelEntry>
            {
                null,
                Make("bad", 250f, ""),
                null
            };
            var issues = global::TileStories.Editor.POIEditorToolWindow.ValidateHierarchyLevelSizeRange(levels);
            Assert.AreEqual(1, issues.Count);
            Assert.AreEqual("Hierarchy level 'bad'", issues[0].subject, "a level with no name falls back to its key");
        }
    }
}
