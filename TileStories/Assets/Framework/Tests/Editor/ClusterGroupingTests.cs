using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TileStories;

namespace TileStories.Tests
{
    // Tier 0 (pure static API): ClusterGrouping (spec section 6.1).
    // No scene, no MonoBehaviour, no Camera -- screen-space positions and
    // effective distances are passed in by the caller, so the static math
    // is fully deterministic and EditMode-runnable with new().
    public class ClusterGroupingTests
    {
        private List<LodBandEntry> _defaultBands;

        [SetUp]
        public void SetUp()
        {
            // Pin ARZoomState.ZoomFactor to 1f so CentroidEffectiveDistance == raw
            // Vector3.Distance. SetZoom is the public write path (ZoomFactor is get-only).
            ARZoomState.SetZoom(1f, 1f, 100f);
            _defaultBands = LODController.DefaultBands();
        }

        // --- Group ---

        [Test]
        public void Group_EmptyInput_ReturnsEmpty()
        {
            var g = ClusterGrouping.Group(new List<VisualUnit>(), new Dictionary<string, Vector2>(), 50f);
            Assert.IsNotNull(g);
            Assert.AreEqual(0, g.Count);
        }

        [Test]
        public void Group_NullScreenPos_ReturnsEmpty()
        {
            var units = new List<VisualUnit> { new VisualUnit { poiId = "p1" } };
            var g = ClusterGrouping.Group(units, null, 50f);
            Assert.AreEqual(0, g.Count);
        }

        [Test]
        public void Group_TwoCloseMarkers_OneGroup()
        {
            // 3-4-5 triangle: screen distance exactly 5 <= radius 10 -> clustered.
            var a = new VisualUnit { poiId = "p1" };
            var b = new VisualUnit { poiId = "p2" };
            var screen = new Dictionary<string, Vector2>
            {
                ["p1"] = new Vector2(0, 0),
                ["p2"] = new Vector2(3, 4),
            };
            var groups = ClusterGrouping.Group(new List<VisualUnit> { a, b }, screen, 10f);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(2, groups[0].Count);
            // Members sorted by poiId (deterministic, spec section 6.1).
            Assert.AreEqual("p1", groups[0][0].poiId);
            Assert.AreEqual("p2", groups[0][1].poiId);
        }

        [Test]
        public void Group_FarMarkers_SeparateGroups()
        {
            var a = new VisualUnit { poiId = "p1" };
            var b = new VisualUnit { poiId = "p2" };
            var screen = new Dictionary<string, Vector2>
            {
                ["p1"] = new Vector2(0, 0),
                ["p2"] = new Vector2(100, 0),
            };
            var groups = ClusterGrouping.Group(new List<VisualUnit> { a, b }, screen, 10f);
            Assert.AreEqual(2, groups.Count);
            // Group order by lowest poiId -> p1 group first.
            Assert.AreEqual("p1", groups[0][0].poiId);
            Assert.AreEqual("p2", groups[1][0].poiId);
        }

        [Test]
        public void Group_GroupOrderIsStableAcrossInputOrder()
        {
            // Feed in reverse poiId order; output must still lead with the lowest poiId.
            var units = new List<VisualUnit>
            {
                new VisualUnit { poiId = "p3" },
                new VisualUnit { poiId = "p1" },
                new VisualUnit { poiId = "p2" },
            };
            var screen = new Dictionary<string, Vector2>
            {
                ["p3"] = new Vector2(100, 0), // p3 isolated: > radius from p1/p2 [expect 2 groups]
                ["p1"] = new Vector2(1, 0),
                ["p2"] = new Vector2(2, 0),
            };
            var groups = ClusterGrouping.Group(units, screen, 10f);
            Assert.AreEqual(2, groups.Count);
            Assert.AreEqual("p1", groups[0][0].poiId);
        }

        [Test]
        public void Group_UnitWithoutScreenPos_IsExcluded()
        {
            var a = new VisualUnit { poiId = "p1" };
            var b = new VisualUnit { poiId = "p2" };
            var screen = new Dictionary<string, Vector2> { ["p1"] = new Vector2(0, 0) };
            var groups = ClusterGrouping.Group(new List<VisualUnit> { a, b }, screen, 10f);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(1, groups[0].Count);
            Assert.AreEqual("p1", groups[0][0].poiId);
        }

        [Test]
        public void Group_TransitiveChainOneConnectedComponent()
        {
            // a-b within radius, b-c within radius, a-c NOT -- union-find must still
            // merge all three into one component (not split a from c).
            var units = new List<VisualUnit>
            {
                new VisualUnit { poiId = "a" },
                new VisualUnit { poiId = "b" },
                new VisualUnit { poiId = "c" },
            };
            var screen = new Dictionary<string, Vector2>
            {
                ["a"] = new Vector2(0, 0),
                ["b"] = new Vector2(6, 0),
                ["c"] = new Vector2(11, 0),
            };
            var groups = ClusterGrouping.Group(units, screen, 10f);
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(3, groups[0].Count);
        }

        [Test]
        public void Group_ExactRadiusBoundary_IsIncluded()
        {
            var a = new VisualUnit { poiId = "p1" };
            var b = new VisualUnit { poiId = "p2" };
            var screen = new Dictionary<string, Vector2>
            {
                ["p1"] = new Vector2(0, 0),
                ["p2"] = new Vector2(10, 0), // sqrMagnitude == radius^2 -> <= true
            };
            var groups = ClusterGrouping.Group(new List<VisualUnit> { a, b }, screen, 10f);
            Assert.AreEqual(1, groups.Count);
        }

        // --- Centroid ---

        [Test]
        public void Centroid_MeanOfWorldPositions()
        {
            var group = new List<VisualUnit>
            {
                new VisualUnit { worldPosition = new Vector3(0, 0, 0) },
                new VisualUnit { worldPosition = new Vector3(4, 0, 0) },
                new VisualUnit { worldPosition = new Vector3(2, 6, 0) },
            };
            var c = ClusterGrouping.Centroid(group);
            Assert.AreEqual(new Vector3(2, 2, 0), c);
        }

        [Test]
        public void Centroid_Empty_ReturnsZero()
        {
            Assert.AreEqual(Vector3.zero, ClusterGrouping.Centroid(new List<VisualUnit>()));
        }

        // --- CentroidEffectiveDistance ---

        [Test]
        public void CentroidEffectiveDistance_ZoomOne_EqualsRawDistance()
        {
            var centroid = new Vector3(0, 0, 0);
            var cam = new Vector3(0, 0, 10);
            Assert.AreEqual(10f, ClusterGrouping.CentroidEffectiveDistance(centroid, cam), 1e-4f);
        }

        // --- MemberIds ---

        [Test]
        public void MemberIds_ExtractsPoiIdsInOrder()
        {
            var group = new List<VisualUnit>
            {
                new VisualUnit { poiId = "p1" },
                new VisualUnit { poiId = "p2" },
            };
            var ids = ClusterGrouping.MemberIds(group);
            Assert.AreEqual(2, ids.Count);
            Assert.AreEqual("p1", ids[0]);
            Assert.AreEqual("p2", ids[1]);
        }

        [Test]
        public void MemberIds_EmptyGroup_EmptyList()
        {
            Assert.AreEqual(0, ClusterGrouping.MemberIds(new List<VisualUnit>()).Count);
        }

        // --- Signature ---

        [Test]
        public void Signature_SortedPipeJoined()
        {
            var sig = ClusterGrouping.Signature(new List<string> { "p3", "p1", "p2" });
            Assert.AreEqual("p1|p2|p3", sig);
        }

        [Test]
        public void Signature_OrderIndependent()
        {
            var a = ClusterGrouping.Signature(new List<string> { "p1", "p2", "p3" });
            var b = ClusterGrouping.Signature(new List<string> { "p3", "p1", "p2" });
            Assert.AreEqual(a, b);
        }

        [Test]
        public void Signature_Empty_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, ClusterGrouping.Signature(new List<string>()));
        }

        [Test]
        public void Signature_SingleMember_EqualsId()
        {
            Assert.AreEqual("p1", ClusterGrouping.Signature(new List<string> { "p1" }));
        }

        // --- Overlaps ---

        [Test]
        public void Overlaps_SharedMember_True()
        {
            Assert.IsTrue(ClusterGrouping.Overlaps(new List<string> { "a", "b" }, new List<string> { "b", "c" }));
        }

        [Test]
        public void Overlaps_Disjoint_False()
        {
            Assert.IsFalse(ClusterGrouping.Overlaps(new List<string> { "a", "b" }, new List<string> { "c", "d" }));
        }

        [Test]
        public void Overlaps_EmptyPool_False()
        {
            Assert.IsFalse(ClusterGrouping.Overlaps(new List<string>(), new List<string> { "a" }));
        }

        [Test]
        public void Overlaps_NullPool_False()
        {
            Assert.IsFalse(ClusterGrouping.Overlaps(null, new List<string> { "a" }));
        }

        // --- ResolveBand ---

        [Test]
        public void ResolveBand_CentroidMode_UsesPassedCentroidDistance()
        {
            // 10m centroid distance -> band 2 (7m..9999m), regardless of members.
            var group = new List<VisualUnit> { new VisualUnit { poiId = "p1", effectiveDistance = 0f } };
            var band = ClusterGrouping.ResolveBand(group, "centroid", 10f, _defaultBands);
            Assert.AreEqual(2, band.Index);
            Assert.AreEqual(9999f, band.MaxDistanceM);
        }

        [Test]
        public void ResolveBand_NullBandSource_DefaultsToCentroid()
        {
            var group = new List<VisualUnit> { new VisualUnit { poiId = "p1", effectiveDistance = 0f } };
            var band = ClusterGrouping.ResolveBand(group, null, 0f, _defaultBands);
            Assert.AreEqual(0, band.Index);
        }

        [Test]
        public void ResolveBand_UnknownMode_FallsBackToCentroid()
        {
            var group = new List<VisualUnit> { new VisualUnit { poiId = "p1", effectiveDistance = 0f } };
            var band = ClusterGrouping.ResolveBand(group, "bogus_mode", 0f, _defaultBands);
            Assert.AreEqual(0, band.Index);
        }

        [Test]
        public void ResolveBand_NearestMember_UsesMinMemberDistance()
        {
            var group = new List<VisualUnit>
            {
                new VisualUnit { poiId = "a", effectiveDistance = 20f },
                new VisualUnit { poiId = "b", effectiveDistance = 3f },
            };
            // min member eff distance = 3f -> band 1 (2m..7m)
            var band = ClusterGrouping.ResolveBand(group, "nearest_member", 999f, _defaultBands);
            Assert.AreEqual(1, band.Index);
        }

        [Test]
        public void ResolveBand_FarthestMember_UsesMaxMemberDistance()
        {
            var group = new List<VisualUnit>
            {
                new VisualUnit { poiId = "a", effectiveDistance = 3f },
                new VisualUnit { poiId = "b", effectiveDistance = 20f },
            };
            // max member eff distance = 20f -> band 2
            var band = ClusterGrouping.ResolveBand(group, "farthest_member", 0f, _defaultBands);
            Assert.AreEqual(2, band.Index);
        }

        // --- BuildAggregate ---

        [Test]
        public void BuildAggregate_PopulatesOwnedFields_LeavesCallerFieldsDefault()
        {
            var group = new List<VisualUnit>
            {
                new VisualUnit { poiId = "p2", worldPosition = new Vector3(2, 0, 0), hierarchyLevelIndex = 1 },
                new VisualUnit { poiId = "p1", worldPosition = new Vector3(0, 0, 0), hierarchyLevelIndex = 0 },
            };
            var centroid = new Vector3(1, 0, 0);
            var agg = ClusterGrouping.BuildAggregate(group, 0, "centroid", centroid, 5f);

            Assert.AreEqual(ClusterGrouping.Signature(ClusterGrouping.MemberIds(group)), agg.poiId);
            Assert.AreEqual(centroid, agg.worldPosition);
            Assert.AreEqual(5f, agg.effectiveDistance);
            Assert.AreEqual(0, agg.hierarchyLevelIndex);
            Assert.AreEqual(DensityState.Clustered, agg.densityState);
            Assert.AreEqual(2, agg.clusterMembers.Count);
            Assert.IsNull(agg.clusterView);                          // caller-owned
            Assert.AreEqual(default(LodBand), agg.band);              // caller-owned
        }
    }
}
