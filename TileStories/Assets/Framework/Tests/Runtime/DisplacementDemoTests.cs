using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Tests
{
    // The dev-only displacement demo (Displacement > Test > "Add displacement demo"), each control one by
    // one on real markers: the switch (stage, camera, wall POIs paused and back), Markers per Group, Spread,
    // Distance, Show labels, Reference copies and Run LOD on the demo.
    public class DisplacementDemoTests : DisplacementDemoFixture
    {
        [UnityTest]
        public IEnumerator Switch_On_BuildsTheDemoOnItsStage_PausesTheWall_Off_GivesEverythingBack()
        {
            Vector3 cameraBefore = Cam.transform.position;
            yield return Demo(new DisplacementDemoSettings());

            Assert.IsNotNull(Session.DisplacementDemoRoot, "the demo exists");
            Assert.AreEqual(DemoFieldStage.DisplacementDemoStagePosition, Session.DisplacementDemoRoot.transform.position, "on its own stage");
            Assert.AreEqual(DemoFieldStage.DisplacementDemoStagePosition, Cam.transform.position, "the camera moved to the stage");
            Assert.IsTrue(WallPois.All(p => !p.activeSelf), "the wall's own POIs pause");
            Assert.IsTrue(LiveMarkers.All(m => m.PoiId.StartsWith("ddemo_") && !m.PoiId.EndsWith("_ref")),
                "displacement runs on the live demo markers only");
            Assert.AreEqual(3 * 4 + 1, LiveMarkers.Count, "three groups of 4 (default) plus the lone marker");
            Assert.IsFalse(Session.LodSettings.enabled, "LOD is paused on the demo by default");
            Assert.Greater(Stats.Groups, 0, "the demo really is crowded");

            Session.ApplyDisplacementDemo(new DisplacementDemoSettings { enabled = false });
            yield return Settle();
            Assert.IsNull(Session.DisplacementDemoRoot, "the demo is gone");
            Assert.AreEqual(cameraBefore, Cam.transform.position, "the camera is back");
            Assert.IsTrue(WallPois.All(p => p.activeSelf), "the wall's POIs are back");
            Assert.AreEqual(WallPois.Length, LiveMarkers.Count, "displacement runs on the wall again");
            Assert.AreSame(Config.lod_settings, Session.LodSettings, "the wall's own LOD settings are back");
        }

        [UnityTest]
        public IEnumerator MarkersPerGroup_SetsEachCrowdedGroupsSize_AndIsClamped()
        {
            yield return Demo(new DisplacementDemoSettings { markers_per_group = 2 });
            Assert.AreEqual(3 * 2 + 1, LiveMarkers.Count);
            yield return Demo(new DisplacementDemoSettings { markers_per_group = 8 });
            Assert.AreEqual(3 * 8 + 1, LiveMarkers.Count);
            Assert.AreEqual(8, Group("same").Length);
            yield return Demo(new DisplacementDemoSettings { markers_per_group = 99 });
            Assert.AreEqual(3 * DisplacementDemoSettings.MaxMarkersPerGroup + 1, LiveMarkers.Count, "clamped to the slider's maximum");
        }

        [UnityTest]
        public IEnumerator Spread_Zero_StacksEveryGroup_Wide_LeavesNothingCrowded()
        {
            yield return Demo(new DisplacementDemoSettings { spread_cm = 0f });
            Assert.AreEqual(3, Stats.Groups, "0 cm: the three groups are stacked, each one crowded");
            Assert.Greater(Stats.Moved, 0);

            // - at 1.5 m, 1 m of demo wall is ~416 px: the closest two true places of a 30 cm spread are ~72 px apart
            yield return Demo(new DisplacementDemoSettings { spread_cm = 30f, distance_m = 1.5f });
            Assert.AreEqual(0, Stats.Groups, "30 cm spread at 1.5 m is far more than 40 px: nobody is crowded");
            Assert.AreEqual(0, Stats.Moved, "and nothing moves");
        }

        [UnityTest]
        public IEnumerator Distance_Farther_MakesTheSameSpreadCrowded_AndKeepsTheDemoInView()
        {
            // - 20 cm spread: closest true places ~11.5 cm apart = ~72 px at 1 m, ~12 px at 6 m
            yield return Demo(new DisplacementDemoSettings { spread_cm = 20f, distance_m = 1f });
            Assert.AreEqual(0, Stats.Groups, "seen from 1 m: well over 40 px, not crowded");

            yield return Demo(new DisplacementDemoSettings { spread_cm = 20f, distance_m = 6f });
            Assert.AreEqual(3, Stats.Groups, "the same spread seen from 6 m: crowded");
            foreach (var m in LiveMarkers.Concat(ReferenceMarkers))
            {
                Vector3 vp = Cam.WorldToViewportPoint(m.transform.position);
                Assert.IsTrue(vp.z > 0f && vp.x > 0f && vp.x < 1f && vp.y > 0f && vp.y < 1f, m.PoiId + " stays in view at 6 m");
            }
        }

        [UnityTest]
        public IEnumerator ShowLabels_On_EveryDemoMarkerHasALabel_Off_NoneAndLabelOnlyMovesNothing()
        {
            yield return Demo(new DisplacementDemoSettings());
            Assert.IsTrue(LiveMarkers.All(m => m.ShowsLabel), "every demo marker shows a label, whatever its level says");
            Assert.Greater(Stats.Moved, 0);

            yield return Demo(new DisplacementDemoSettings { show_labels = false });
            Assert.IsTrue(LiveMarkers.All(m => !m.ShowsLabel), "no demo marker shows a label");
            Assert.Greater(Stats.Groups, 0, "the markers still crowd each other");
            Assert.AreEqual(0, Stats.Moved, "Label only has no label to move");
        }

        [UnityTest]
        public IEnumerator ReferenceCopies_SideBySide_Overlay_Off_AndACopyNeverMoves()
        {
            yield return Demo(new DisplacementDemoSettings { reference_copies = "side_by_side" });
            Assert.AreEqual(LiveMarkers.Count, ReferenceMarkers.Count, "one faded copy per live marker");
            var live = Live("ddemo_same_1");
            var copy = Reference("ddemo_same_1");
            Assert.Less(copy.transform.position.x, live.transform.position.x - 0.1f, "side by side: the copy stands to the left");
            Assert.AreEqual(copy.transform.position.y, live.transform.position.y, 1e-4f);
            Assert.AreEqual(DisplacementDemoLayout.ReferenceAlpha, copy.GetComponent<CanvasGroup>().alpha, 0.01f, "the copy is faded");
            Assert.IsFalse(Session.SpawnedMarkers.Contains(copy), "copies are not given to LOD / displacement");
            Assert.Greater(Stats.Moved, 0, "precondition: the live group was displaced");
            Assert.IsTrue(ReferenceMarkers.All(m => !m.HasLabelOffset && !Line(m).IsShown), "a copy never moves and has no line");

            yield return Demo(new DisplacementDemoSettings { reference_copies = "overlay" });
            Assert.AreEqual(Reference("ddemo_same_1").transform.position, Live("ddemo_same_1").transform.position,
                "overlay: the copy sits exactly under the live marker");

            yield return Demo(new DisplacementDemoSettings { reference_copies = "off" });
            Assert.AreEqual(0, ReferenceMarkers.Count, "off: no copies");
        }

        [UnityTest]
        public IEnumerator RunLod_Off_LodLeavesTheDemoAlone_On_LodActsFirst()
        {
            // - a LOD that would show only 2 markers
            Config.lod_settings = new LodSettings
            {
                enabled = true,
                bands = new System.Collections.Generic.List<LodBandEntry> { new LodBandEntry { max_distance_m = 9999f, max_visible_count = 2 } },
                density_response_mode = "none",
                transition_fade_duration_s = 0f,
                evaluation_interval_s = 999f,
            };
            yield return Demo(new DisplacementDemoSettings { run_lod = false });
            Assert.IsFalse(Session.LodSettings.enabled);
            Assert.IsTrue(LiveMarkers.All(m => m.IsVisible), "LOD hides nothing on the demo");
            Assert.AreEqual(LiveMarkers.Count, Stats.Candidates);

            yield return Demo(new DisplacementDemoSettings { run_lod = true });
            Assert.AreSame(Config.lod_settings, Session.LodSettings, "the wall's LOD runs on the demo");
            Assert.AreEqual(2, LiveMarkers.Count(m => m.IsVisible), "LOD's Max markers = 2 applies");
            Assert.AreEqual(2, Stats.Candidates, "only what LOD shows takes part in displacement");
        }

        [UnityTest]
        public IEnumerator LodDemoField_On_WinsOverTheDisplacementDemo()
        {
            yield return Demo(new DisplacementDemoSettings());
            Assert.IsNotNull(Session.DisplacementDemoRoot);

            var field = new DemoFieldSettings { enabled = true, level_counts = new System.Collections.Generic.List<DemoFieldLevelCount>
                { new DemoFieldLevelCount { level_key = Config.hierarchy_levels[0].key, count = 2 } } };
            Session.ApplyDemoField(field);
            yield return Settle();
            Assert.IsNull(Session.DisplacementDemoRoot, "both demos take over the wall: the LOD demo field wins");
            Assert.IsNotNull(Session.DemoFieldRoot);
            Assert.AreEqual(DemoFieldStage.EditorStagePosition, Cam.transform.position, "the camera is on the LOD field's stage");

            Session.ApplyDemoField(new DemoFieldSettings());
            yield return Settle();
            Assert.IsNotNull(Session.DisplacementDemoRoot, "the displacement demo comes back when the field goes");
            Assert.AreEqual(DemoFieldStage.DisplacementDemoStagePosition, Cam.transform.position);
        }
    }
}
