using System.Collections.Generic;
using NUnit.Framework;
using TileStories.Editor;
using UnityEngine;

namespace TileStories.Tests
{
    // The live LOD readout (LOD > Test): LodStats counts one evaluation's final units, the window turns
    // them into text. Pure: fabricated units in each state the pipeline can leave a marker in. The same
    // counts on REAL markers are asserted by LodWallSceneTests.
    public class LodStatsTests
    {
        private static readonly List<LodBandEntry> Bands = new()
        {
            new LodBandEntry { max_distance_m = 2f, max_visible_count = -1 },
            new LodBandEntry { max_distance_m = 9999f, max_visible_count = 3 },
        };

        private static VisualUnit Unit(int band, bool inView = true, bool visible = true, DensityState state = DensityState.Normal, float shrink = 1f) =>
            new VisualUnit { inView = inView, isVisible = visible, densityState = state, shrinkScale = shrink, band = new LodBand { Index = band } };

        [Test]
        public void From_CountsEveryOutcomeSeparately_AndEachBandsShare()
        {
            var cluster = Unit(1);
            cluster.clusterMembers = new List<VisualUnit> { new(), new(), new(), new() };
            var cappedCluster = Unit(1, visible: false);
            cappedCluster.clusterMembers = new List<VisualUnit> { new(), new() };
            var units = new List<VisualUnit>
            {
                Unit(0), Unit(0, shrink: 0.5f),                            // near band: 2 shown, 1 of them shrunk
                Unit(1), Unit(1, visible: false),                          // far band: 1 shown, 1 cut by Max markers
                Unit(1, visible: false, state: DensityState.Clustered),    // hidden by Select & Hide
                Unit(0, inView: false, visible: false),                    // culled
                cluster, cappedCluster,
            };

            var s = LodStats.From(units, Bands, 2f);

            Assert.AreEqual(6 + 4 + 2, s.Markers, "6 individual units + the members merged into both clusters");
            Assert.AreEqual(3, s.Shown);
            Assert.AreEqual(1, s.Shrunk);
            Assert.AreEqual(1, s.HiddenByCrowding);
            Assert.AreEqual(1 + 2, s.HiddenByMaxMarkers, "one marker and the members of a capped cluster");
            Assert.AreEqual(1, s.OutOfView);
            Assert.AreEqual(1, s.Clusters);
            Assert.AreEqual(4, s.InClusters);
            Assert.AreEqual(2f, s.ZoomFactor);
            Assert.AreEqual(2, s.Bands.Count);
            Assert.AreEqual((2, 2), (s.Bands[0].Shown, s.Bands[0].InBand), "near band: the culled marker is not in it");
            Assert.AreEqual((2, 5), (s.Bands[1].Shown, s.Bands[1].InBand), "far band: 1 marker + 1 cluster of 5 units");
            Assert.AreEqual(3, s.Bands[1].MaxVisibleCount);
        }

        [Test]
        public void Readout_SaysEachCountInTheWordsOfTheEditorTab()
        {
            var s = new LodStats
            {
                Markers = 28, Shown = 12, Shrunk = 4, Clusters = 1, InClusters = 9, HiddenByCrowding = 2,
                HiddenByMaxMarkers = 3, OutOfView = 2, ZoomFactor = 1.5f,
                Bands = new List<LodStats.Band> { new(2f, -1, 5, 5), new(9999f, 5, 9, 5) },
            };
            string text = POIEditorToolWindow.LodLiveReadout(s);
            StringAssert.Contains("Shown: 12 of 28 markers (4 smaller and fainter)", text);
            StringAssert.Contains("Clusters: 1 (holding 9 markers)", text);
            StringAssert.Contains("Hidden by crowding: 2 | by Max markers: 3 | out of view: 2", text);
            StringAssert.Contains("Zoom: 1.5x", text);
            StringAssert.Contains("Band 1 (up to 2 m, all): 5 of 5 shown", text);
            StringAssert.Contains("Band 2 (up to 9999 m, max 5): 5 of 9 shown", text);
            foreach (char c in text) Assert.Less((int)c, 128, "ASCII only");
        }

        [Test]
        public void Stage_EditorUsesTheEmptyStageAndMovesTheCamera_ADeviceUsesItsStartPose()
        {
            var cam = new GameObject("StageCam").transform;
            try
            {
                cam.SetPositionAndRotation(new Vector3(4f, 1.6f, -2f), Quaternion.Euler(20f, 75f, 0f));
                var editor = DemoFieldStage.ChooseFieldPose(true, cam);
                Assert.AreEqual(DemoFieldStage.EditorStagePosition, editor.position);
                Assert.AreEqual(Quaternion.identity, editor.rotation);
                Assert.IsTrue(DemoFieldStage.MovesCamera(true));

                var device = DemoFieldStage.ChooseFieldPose(false, cam);
                Assert.AreEqual(cam.position, device.position, "a device keeps the field where the phone is");
                Assert.AreEqual(75f, device.rotation.eulerAngles.y, 0.01f, "turned like the camera...");
                Assert.AreEqual(0f, device.rotation.eulerAngles.x, 0.01f, "...but level, whatever the camera's pitch");
                Assert.IsFalse(DemoFieldStage.MovesCamera(false), "an AR camera follows the phone: never moved");
            }
            finally { Object.DestroyImmediate(cam.gameObject); }
        }
    }
}
