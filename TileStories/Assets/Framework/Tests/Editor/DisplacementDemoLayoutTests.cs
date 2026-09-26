using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The displacement demo's pure layout (DisplacementDemoLayout), each demo control one by one: Markers per
    // Group, Spread, Distance, Reference copies, the four scenarios' levels (chosen by Priority, not table
    // order), plus DisplacementDemoSpawner's allow / LOD-pause rules and MarkerOverlapResolver.TakesPart.
    public class DisplacementDemoLayoutTests
    {
        // Table order deliberately differs from priority order
        private static readonly List<HierarchyLevelEntry> Levels = new()
        {
            new HierarchyLevelEntry { key = "mid", priority = 3 },
            new HierarchyLevelEntry { key = "top", priority = 1 },
            new HierarchyLevelEntry { key = "bottom", priority = 5 },
            new HierarchyLevelEntry { key = "second", priority = 2 },
            new HierarchyLevelEntry { key = "fourth", priority = 4 },
        };

        private static readonly string[] Categories = { "a", "b", "c" };

        private static List<DisplacementDemoLayout.Entry> Build(DisplacementDemoSettings s) =>
            DisplacementDemoLayout.Build(s, Levels, Categories);

        private static List<DisplacementDemoLayout.Entry> Live(List<DisplacementDemoLayout.Entry> e) => e.Where(x => !x.IsReference).ToList();

        [Test]
        public void MarkersPerGroup_ThreeGroupsOfN_PlusOneLone_Clamped()
        {
            foreach (int n in new[] { 2, 5, 8 })
            {
                var live = Live(Build(new DisplacementDemoSettings { markers_per_group = n, reference_copies = "off" }));
                Assert.AreEqual(3 * n + 1, live.Count);
                Assert.AreEqual(n, live.Count(e => e.Scenario == DisplacementDemoLayout.SameLevel));
                Assert.AreEqual(1, live.Count(e => e.Scenario == DisplacementDemoLayout.Lone));
            }
            Assert.AreEqual(3 * 2 + 1, Live(Build(new DisplacementDemoSettings { markers_per_group = 0 })).Count, "clamped to the minimum");
            Assert.AreEqual(3 * 8 + 1, Live(Build(new DisplacementDemoSettings { markers_per_group = 50 })).Count, "clamped to the maximum");
        }

        [Test]
        public void Spread_Zero_StacksEveryGroup_OtherwiseTheLastMemberSitsExactlyAtTheSpread()
        {
            foreach (string scenario in new[] { DisplacementDemoLayout.SameLevel, DisplacementDemoLayout.MixedLevels, DisplacementDemoLayout.BigAndSmall })
            {
                var stacked = Live(Build(new DisplacementDemoSettings { spread_cm = 0f })).Where(e => e.Scenario == scenario).ToList();
                Assert.IsTrue(stacked.All(e => e.LocalPosition == stacked[0].LocalPosition), scenario + ": 0 cm = one point");

                var spread = Live(Build(new DisplacementDemoSettings { spread_cm = 12f, markers_per_group = 4 })).Where(e => e.Scenario == scenario).ToList();
                Vector3 centre = spread[0].LocalPosition;
                Assert.AreEqual(0.12f, (spread[3].LocalPosition - centre).magnitude, 1e-4f, scenario + ": the last member at the full spread");
                Assert.IsTrue(spread.All(e => (e.LocalPosition - centre).magnitude <= 0.12f + 1e-4f), scenario + ": nobody beyond the spread");
            }
            Assert.AreEqual(0.3f, DisplacementDemoLayout.SpreadMetres(new DisplacementDemoSettings { spread_cm = 99f }), 1e-5f, "clamped");
        }

        [Test]
        public void Spread_IsIrregular_NoTwoMembersMirrorEachOther()
        {
            // - a symmetric ring cancels every Force Directed push; the spiral never lines two members up
            var group = Live(Build(new DisplacementDemoSettings { spread_cm = 10f, markers_per_group = 6 }))
                .Where(e => e.Scenario == DisplacementDemoLayout.SameLevel).ToList();
            Vector3 centre = group[0].LocalPosition;
            var offsets = group.Skip(1).Select(e => e.LocalPosition - centre).ToList();
            for (int i = 0; i < offsets.Count; i++)
                for (int j = i + 1; j < offsets.Count; j++)
                    Assert.Greater((offsets[i] + offsets[j]).magnitude, 0.005f, "members " + i + " and " + j + " are not mirror images");
        }

        [Test]
        public void Distance_PutsTheWallThere_AndScalesTheSpacingWithIt()
        {
            var near = Build(new DisplacementDemoSettings { distance_m = 1f, spread_cm = 0f, reference_copies = "off" });
            var far = Build(new DisplacementDemoSettings { distance_m = 4f, spread_cm = 0f, reference_copies = "off" });
            Assert.IsTrue(near.All(e => Mathf.Approximately(e.LocalPosition.z, 1f)));
            Assert.IsTrue(far.All(e => Mathf.Approximately(e.LocalPosition.z, 4f)));
            float nearWidth = near.Max(e => e.LocalPosition.x) - near.Min(e => e.LocalPosition.x);
            float farWidth = far.Max(e => e.LocalPosition.x) - far.Min(e => e.LocalPosition.x);
            Assert.AreEqual(4f, farWidth / nearWidth, 0.2f, "4x farther = 4x wider: the demo keeps the same size on screen");
            Assert.AreEqual(DisplacementDemoSettings.MaxDistanceM, DisplacementDemoLayout.DistanceMetres(new DisplacementDemoSettings { distance_m = 100f }));
            Assert.AreEqual(DisplacementDemoSettings.MinDistanceM, DisplacementDemoLayout.DistanceMetres(new DisplacementDemoSettings { distance_m = 0f }));
        }

        [Test]
        public void ReferenceCopies_SideBySide_Overlay_Off_Unknown()
        {
            var side = Build(new DisplacementDemoSettings { reference_copies = "side_by_side" });
            Assert.AreEqual(Live(side).Count, side.Count(e => e.IsReference), "one copy per live marker");
            foreach (var live in Live(side))
            {
                var copy = side.Single(e => e.Id == live.Id + "_ref");
                Assert.Less(copy.LocalPosition.x, live.LocalPosition.x, live.Id + ": the copy stands to the left");
                Assert.AreEqual(copy.LocalPosition.y, live.LocalPosition.y, 1e-6f);
                Assert.AreEqual(live.LevelKey, copy.LevelKey);
                Assert.AreEqual(live.Category, copy.Category);
            }

            var overlay = Build(new DisplacementDemoSettings { reference_copies = "overlay" });
            foreach (var live in Live(overlay))
                Assert.AreEqual(live.LocalPosition, overlay.Single(e => e.Id == live.Id + "_ref").LocalPosition, "overlay: same place");

            Assert.IsFalse(Build(new DisplacementDemoSettings { reference_copies = "off" }).Any(e => e.IsReference), "off: none");
            Assert.AreEqual("side_by_side", DisplacementDemoLayout.ReferenceMode(new DisplacementDemoSettings { reference_copies = "bogus" }));
        }

        [Test]
        public void Scenarios_PickTheirLevelsByPriority_NotByTableOrder()
        {
            var live = Live(Build(new DisplacementDemoSettings { markers_per_group = 6, reference_copies = "off" }));
            Assert.IsTrue(live.Where(e => e.Scenario == DisplacementDemoLayout.SameLevel).All(e => e.LevelKey == "mid"), "Same: the middle priority");
            CollectionAssert.AreEqual(new[] { "top", "second", "mid", "fourth", "bottom", "top" },
                live.Where(e => e.Scenario == DisplacementDemoLayout.MixedLevels).Select(e => e.LevelKey).ToArray(), "Mixed: every level, most important first");
            var bigSmall = live.Where(e => e.Scenario == DisplacementDemoLayout.BigAndSmall).ToList();
            Assert.AreEqual("top", bigSmall[0].LevelKey, "one top-level marker");
            Assert.IsTrue(bigSmall.Skip(1).All(e => e.LevelKey == "bottom"), "among the lowest level");
            Assert.AreEqual("mid", live.Single(e => e.Scenario == DisplacementDemoLayout.Lone).LevelKey);
            Assert.AreEqual(live.Count, live.Select(e => e.Id).Distinct().Count(), "ids are unique");
        }

        [Test]
        public void Spawner_DevelopmentOnly_PausesLodUnlessAsked_AndGivesWayToTheLodDemoField()
        {
            Assert.IsTrue(DisplacementDemoSpawner.IsAllowed(isEditor: true, isDebugBuild: false));
            Assert.IsFalse(DisplacementDemoSpawner.IsAllowed(isEditor: false, isDebugBuild: false), "release builds ignore it");
            Assert.IsTrue(DisplacementDemoSpawner.PausesLod(new DisplacementDemoSettings { run_lod = false }));
            Assert.IsFalse(DisplacementDemoSpawner.PausesLod(new DisplacementDemoSettings { run_lod = true }));

            var prefab = new GameObject("prefab");
            var camGO = new GameObject("cam", typeof(Camera));
            try
            {
                var config = new WallConfigData();
                config.displacement_demo.enabled = true;
                Assert.IsTrue(DisplacementDemoSpawner.ShouldSpawn(config, prefab, camGO.GetComponent<Camera>()));
                config.demo_field.enabled = true;
                Assert.IsFalse(DisplacementDemoSpawner.ShouldSpawn(config, prefab, camGO.GetComponent<Camera>()), "the LOD demo field wins");
            }
            finally { Object.DestroyImmediate(prefab); Object.DestroyImmediate(camGO); }
        }

        [Test]
        public void TakesPart_OnlyVisibleIndividualMarkers()
        {
            Assert.IsFalse(MarkerOverlapResolver.TakesPart(null));
            Assert.IsFalse(MarkerOverlapResolver.TakesPart(new VisualUnit { poiId = "no view" }), "a unit without a MarkerView");
            Assert.IsFalse(MarkerOverlapResolver.TakesPart(new VisualUnit { poiId = "cluster", clusterMembers = new List<VisualUnit>() }), "a cluster aggregate");
        }
    }
}
