using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // "Along the wall" (_3.1 step 7B): the ONE rule wall_locator and related read
    public class WallAxisRuleTests
    {
        private static List<float> AlongAll(WallAxis axis, IEnumerable<Vector3> positions) => positions.Select(axis.Along).ToList();

        [Test]
        public void AWallAlongWorldX_IsWorldX_PointingRight_AndHeightNeverCounts()
        {
            var positions = new[] { new Vector3(-2f, 0f, 5f), new Vector3(0f, 1.5f, 5.1f), new Vector3(2f, -1f, 4.9f) };
            var axis = WallAxisRule.Of(positions);
            Assert.AreEqual(1f, axis.Direction.x, 0.01f, "the axis the POIs spread along: world x, pointing right (+x)");
            Assert.AreEqual(0f, axis.Direction.y, 1e-6f, "always on the floor plane");
            Assert.AreEqual(0f, axis.Along(new Vector3(0f, 0f, 5f)), 0.05f, "the POIs' centre is 0");
            Assert.AreEqual(axis.Along(new Vector3(1f, -3f, 5f)), axis.Along(new Vector3(1f, 3f, 5f)), 1e-5f, "high or low on the panel: the same place along the wall");
            Assert.Less(axis.Along(positions[0]), axis.Along(positions[2]), "left to right");
        }

        [Test]
        public void AWallStandingAtAnAngle_IsFoundAlongItsOwnLength_NotWorldX()
        {
            // - a wall at 30 degrees from world x: MinimapLayout's world-x axis would squash these together
            var dir = new Vector3(Mathf.Cos(30f * Mathf.Deg2Rad), 0f, Mathf.Sin(30f * Mathf.Deg2Rad));
            var positions = new[] { -3f, -1f, 0.5f, 2f, 4f }.Select(t => new Vector3(1f, 0.3f * t, 2f) + dir * t).ToList();
            var axis = WallAxisRule.Of(positions);
            Assert.AreEqual(1f, Mathf.Abs(Vector3.Dot(axis.Direction, dir)), 1e-4f, "the axis runs along the wall itself");
            var along = AlongAll(axis, positions);
            for (int i = 1; i < along.Count; i++)
                Assert.AreEqual(along[i - 1] + (new[] { -3f, -1f, 0.5f, 2f, 4f }[i] - new[] { -3f, -1f, 0.5f, 2f, 4f }[i - 1]), along[i], 1e-3f,
                    "distances along the wall are the real distances between the POIs");
        }

        [Test]
        public void TheDirectionIsRightByTheMinimapsConvention_WhateverTheInputOrder()
        {
            var a = new[] { new Vector3(3f, 0f, 1f), new Vector3(-3f, 0f, -1f), new Vector3(0f, 0f, 0f) };
            Assert.Greater(WallAxisRule.Of(a).Direction.x, 0f, "x positive");
            Assert.Greater(WallAxisRule.Of(a.Reverse().ToList()).Direction.x, 0f, "the same, listed the other way");
            var alongZ = new[] { new Vector3(0f, 0f, -2f), new Vector3(0f, 0f, 3f) };
            Assert.AreEqual(1f, WallAxisRule.Of(alongZ).Direction.z, 1e-4f, "a wall exactly along z: z positive");
        }

        [Test]
        public void NoPoint_OnePoint_OrAllInOneSpot_FallBackToWorldX()
        {
            Assert.AreEqual(Vector3.right, WallAxisRule.Of(new List<Vector3>()).Direction);
            Assert.AreEqual(Vector3.right, WallAxisRule.Of(null).Direction);
            var one = WallAxisRule.Of(new[] { new Vector3(4f, 2f, 1f) });
            Assert.AreEqual(Vector3.right, one.Direction);
            Assert.AreEqual(0f, one.Along(new Vector3(4f, 9f, 1f)), 1e-6f, "the lone POI is the centre");
            Assert.AreEqual(Vector3.right, WallAxisRule.Of(new[] { Vector3.one, Vector3.one, Vector3.one }).Direction, "stacked POIs");
        }

        [Test]
        public void Neighbours_AreTheNearestOnEachSide_TiesBrokenById_AndTheEndsHaveOneSide()
        {
            var along = new List<float> { 0f, -1f, 2f, 2f, 5f };
            var ids = new List<string> { "mid", "left", "b", "a", "far" };
            Assert.AreEqual((1, 3), WallAxisRule.Neighbours(along, ids, 0), "mid: left of it 'left', right of it the first of the tie by id ('a')");
            Assert.AreEqual((3, 4), WallAxisRule.Neighbours(along, ids, 2), "'b' ties with 'a' at 2: 'a' comes first, so it is b's left neighbour");
            Assert.AreEqual((-1, 0), WallAxisRule.Neighbours(along, ids, 1), "the left end: no left neighbour");
            Assert.AreEqual((2, -1), WallAxisRule.Neighbours(along, ids, 4), "the right end: no right neighbour");
            Assert.AreEqual((-1, -1), WallAxisRule.Neighbours(along, ids, 9), "not on the wall: none");
            CollectionAssert.AreEqual(new[] { 1, 0, 3, 2, 4 }, WallAxisRule.Order(along, ids), "left to right");
        }

        // The real wall: The Lamp and its satellites on the shipped LivingRoom config
        [Test]
        public void OnTheShippedWall_TheLampsNeighboursAreItsOwnSatellites_OnEitherSide()
        {
            var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));
            var pois = config.pois.Where(p => POIPositionResolver.TryResolvePosition(p, out _, logErrors: false)).ToList();
            var positions = pois.Select(p => { POIPositionResolver.TryResolvePosition(p, out var v, false); return v; }).ToList();
            var axis = WallAxisRule.Of(positions);
            var along = AlongAll(axis, positions);
            var ids = pois.Select(p => p.id).ToList();
            var (left, right) = WallAxisRule.Neighbours(along, ids, ids.IndexOf("lamp"));
            Assert.GreaterOrEqual(left, 0, "The Lamp has a POI on its left");
            Assert.GreaterOrEqual(right, 0, "...and on its right");
            Assert.LessOrEqual(along[left], along[ids.IndexOf("lamp")]);
            Assert.GreaterOrEqual(along[right], along[ids.IndexOf("lamp")]);
            Assert.AreNotEqual(ids[left], ids[right]);
        }
    }
}
