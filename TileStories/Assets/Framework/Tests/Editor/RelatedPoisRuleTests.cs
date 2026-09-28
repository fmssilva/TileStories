using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // The related block's picks (_3.1 step 7B): manual / same_category / nearest (RelatedPoisRule.Pick) and
    // next_along_wall (RelatedPoisRule.NextAlongWall, WallAxisRule's own rule)
    public class RelatedPoisRuleTests
    {
        private static POIData Poi(string id, string category, Vector3 position) =>
            new() { id = id, category = category, position = new PositionData { x = position.x, y = position.y, z = position.z } };

        private static readonly List<POIData> Wall = new()
        {
            Poi("self", "cat_a", new Vector3(0f, 0f, 0f)),
            Poi("near", "cat_a", new Vector3(1f, 0f, 0f)),
            Poi("far", "cat_a", new Vector3(4f, 0f, 0f)),
            Poi("other_cat_close", "cat_b", new Vector3(0.5f, 0f, 0f)),
        };

        private static POIData Self => Wall[0];

        [Test]
        public void Manual_KeepsWrittenOrder_LeavesOutSelfAndAPointNoLongerOnTheWall()
        {
            var picked = RelatedPoisRule.Pick(Self, RelatedPoisRule.SourceManual, new[] { "far", "self", "gone", "near" }, Wall);
            CollectionAssert.AreEqual(new[] { "far", "near" }, IdsOf(picked), "self and a stale id are left out, written order kept");
        }

        [Test]
        public void Manual_ADuplicateRow_IsKeptOnlyOnce_AtItsFirstPosition()
        {
            var picked = RelatedPoisRule.Pick(Self, RelatedPoisRule.SourceManual, new[] { "near", "far", "near" }, Wall);
            CollectionAssert.AreEqual(new[] { "near", "far" }, IdsOf(picked));
        }

        [Test]
        public void Manual_WithNoIds_PicksNothing_AnUnknownSource_ReadsAsManual()
        {
            Assert.IsEmpty(RelatedPoisRule.Pick(Self, RelatedPoisRule.SourceManual, null, Wall));
            Assert.IsEmpty(RelatedPoisRule.Pick(Self, "", null, Wall));
            CollectionAssert.AreEqual(new[] { "near" }, IdsOf(RelatedPoisRule.Pick(Self, "not_a_real_source", new List<string> { "near" }, Wall)),
                "an unknown source reads as manual, so its Points list still picks");
        }

        [Test]
        public void SameCategory_ExcludesTheOtherCategory_NearestFirst_TiesBrokenById()
        {
            var picked = RelatedPoisRule.Pick(Self, RelatedPoisRule.SourceSameCategory, null, Wall);
            CollectionAssert.AreEqual(new[] { "near", "far" }, IdsOf(picked), "same category only, nearest first");
        }

        [Test]
        public void Nearest_IncludesEveryOtherCategory_NearestFirst()
        {
            var picked = RelatedPoisRule.Pick(Self, RelatedPoisRule.SourceNearest, null, Wall);
            CollectionAssert.AreEqual(new[] { "other_cat_close", "near", "far" }, IdsOf(picked), "the closer other-category point comes first");
        }

        [Test]
        public void AnAutomaticSource_StopsAtMaxAutomatic()
        {
            var wall = new List<POIData> { Poi("self", "cat_a", Vector3.zero) };
            for (int i = 0; i < RelatedPoisRule.MaxAutomatic + 3; i++) wall.Add(Poi("p" + i, "cat_a", new Vector3(i + 1, 0f, 0f)));
            var picked = RelatedPoisRule.Pick(wall[0], RelatedPoisRule.SourceNearest, null, wall);
            Assert.AreEqual(RelatedPoisRule.MaxAutomatic, picked.Count);
        }

        [Test]
        public void NoWallOrNoPoi_PicksNothing_AndNeverThrows()
        {
            Assert.IsEmpty(RelatedPoisRule.Pick(null, RelatedPoisRule.SourceNearest, null, Wall));
            Assert.IsEmpty(RelatedPoisRule.Pick(Self, RelatedPoisRule.SourceNearest, null, null));
        }

        [Test]
        public void NextAlongWall_PicksTheNearestCandidateToTheRight_WrapsLeftAtTheWallsEnd()
        {
            var wall = new List<POIData>
            {
                Poi("self", "cat_a", new Vector3(0f, 0f, 0f)),
                Poi("left", "cat_a", new Vector3(-2f, 0f, 0f)),
                Poi("right", "cat_a", new Vector3(2f, 0f, 0f)),
            };
            var candidates = new List<POIData> { wall[1], wall[2] };
            var (next, direction) = RelatedPoisRule.NextAlongWall(wall[0], candidates, wall);
            Assert.AreEqual("right", next.id);
            Assert.AreEqual(1, direction, "moving right along the wall");

            // - past both candidates: no candidate to the right, wraps to the nearest on the left
            var pastEveryone = new POIData { id = "self", category = "cat_a", position = new PositionData { x = 5f, y = 0f, z = 0f } };
            var wallAtEnd = new List<POIData> { pastEveryone, wall[1], wall[2] };
            var (wrapped, wrapDirection) = RelatedPoisRule.NextAlongWall(pastEveryone, candidates, wallAtEnd);
            Assert.AreEqual("right", wrapped.id, "the nearest candidate along the wall, even though it is behind now");
            Assert.AreEqual(-1, wrapDirection, "wrapping: the move reads as left");
        }

        [Test]
        public void NextAlongWall_NotOnTheWall_OrNoCandidatesThere_PicksNone()
        {
            var offWall = new POIData { id = "ghost" };
            AssertNone(RelatedPoisRule.NextAlongWall(offWall, new List<POIData> { Wall[1] }, Wall), "this point is not on the wall");
            AssertNone(RelatedPoisRule.NextAlongWall(Self, new List<POIData>(), Wall), "no candidates");
            AssertNone(RelatedPoisRule.NextAlongWall(Self, null, Wall));
        }

        private static void AssertNone((POIData Poi, int Direction) result, string message = "")
        {
            Assert.IsNull(result.Poi, message);
            Assert.AreEqual(0, result.Direction, message);
        }

        private static List<string> IdsOf(List<POIData> pois)
        {
            var ids = new List<string>();
            foreach (var poi in pois) ids.Add(poi.id);
            return ids;
        }
    }
}
