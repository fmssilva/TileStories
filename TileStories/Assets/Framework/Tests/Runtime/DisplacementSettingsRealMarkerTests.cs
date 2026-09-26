using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Tests
{
    // Every Displacement setting (Global Scene > Displacement), one by one, changes what REAL markers do:
    // swapped live through WallSession.ApplyDisplacementSettings (the POI Editor's live push seam) on the
    // displacement demo's crowded groups, evaluated by the real LODController, read back from the markers'
    // labels, roots and leader lines -- plus renders saved under Assets/Screenshots for the vision pass.
    public class DisplacementSettingsRealMarkerTests : DisplacementDemoFixture
    {
        private IEnumerator StartDemo(DisplacementSettings settings)
        {
            Session.ApplyDisplacementSettings(settings);
            yield return Demo(new DisplacementDemoSettings());
        }

        // Label screen positions of the live markers with displacement OFF (their true places)
        private IEnumerator TruePlaces(Dictionary<string, Vector2> labels, Dictionary<string, Vector3> roots)
        {
            yield return Displacement(D(s => s.enabled = false));
            foreach (var m in LiveMarkers)
            {
                labels[m.PoiId] = LabelScreen(m);
                roots[m.PoiId] = m.transform.position;
            }
        }

        // ---- Enable Displacement ----

        [UnityTest]
        public IEnumerator Enable_On_MovesCrowdedLabels_Off_PutsEverythingBackLive()
        {
            yield return StartDemo(D());
            Assert.Greater(Stats.Moved, 0, "on: crowded labels move");
            Assert.IsTrue(LiveMarkers.Any(m => m.HasLabelOffset));
            Assert.Greater(ShownLines, 0, "and get leader lines");

            yield return Displacement(D(s => s.enabled = false));
            Assert.IsNull(Stats, "off: no displacement cycle runs");
            Assert.IsTrue(LiveMarkers.All(m => !m.HasLabelOffset), "off: every label is back on its true place");
            Assert.IsTrue(LiveMarkers.All(m => (m.transform.position - m.UndisplacedWorldPosition).sqrMagnitude < 1e-10f));
            Assert.AreEqual(0, ShownLines, "off: no leader line stays behind");
        }

        // ---- Overlap Distance (px) ----

        [UnityTest]
        public IEnumerator OverlapDistance_DecidesWhichMarkersCountAsCrowded()
        {
            // - a 10 cm spread at 3 m puts the closest two true places of a group ~12 px apart
            Session.ApplyDisplacementSettings(D(s => s.overlap_threshold_px = 5f));
            yield return Demo(new DisplacementDemoSettings { spread_cm = 10f });
            Assert.AreEqual(0, Stats.Groups, "5 px: the group members are farther apart than that");
            Assert.AreEqual(0, Stats.Moved);

            yield return Displacement(D(s => s.overlap_threshold_px = 40f));
            Assert.AreEqual(3, Stats.Groups, "40 px: the three demo groups are crowded");
            Assert.IsFalse(Live("ddemo_lone_1").HasLabelOffset, "the lone marker never moves");
        }

        // ---- What Moves ----

        [UnityTest]
        public IEnumerator WhatMoves_LabelOnly_Marker_Both_AndSwitchingLeavesNoStaleOffset()
        {
            yield return StartDemo(D(s => s.displace_target = "label_only"));
            var moved = LiveMarkers.Where(m => m.HasLabelOffset).ToList();
            Assert.IsNotEmpty(moved, "Label only: labels move");
            Assert.IsTrue(LiveMarkers.All(m => (m.transform.position - m.UndisplacedWorldPosition).sqrMagnitude < 1e-10f),
                "Label only: every marker stays on its true place");

            yield return Displacement(D(s => s.displace_target = "marker"));
            Assert.IsTrue(LiveMarkers.Any(m => (m.transform.position - m.UndisplacedWorldPosition).magnitude > 0.005f), "Marker: markers move");
            Assert.IsTrue(LiveMarkers.All(m => !m.HasLabelOffset), "Marker: the label stays on its marker (no stale label offset)");

            yield return Displacement(D(s => s.displace_target = "both"));
            Assert.IsTrue(LiveMarkers.Any(m => m.HasLabelOffset && (m.transform.position - m.UndisplacedWorldPosition).magnitude > 0.005f),
                "Both: a moved marker's label moves as well");

            yield return Displacement(D(s => s.displace_target = "label_only"));
            Assert.IsTrue(LiveMarkers.All(m => (m.transform.position - m.UndisplacedWorldPosition).sqrMagnitude < 1e-10f),
                "back to Label only: no marker keeps its old offset");
        }

        // ---- Algorithm ----

        [UnityTest]
        public IEnumerator Algorithm_FixedAxis_StacksVertically_TheOthersSpreadSideways_EachLooksDifferent()
        {
            var labels = new Dictionary<string, Vector2>();
            var roots = new Dictionary<string, Vector3>();
            yield return StartDemo(D());
            yield return TruePlaces(labels, roots);

            var results = new Dictionary<string, Dictionary<string, Vector2>>();
            foreach (string algorithm in new[] { "fixed_axis", "candidate_position", "force_directed" })
            {
                yield return Displacement(D(s => s.displacement_algorithm = algorithm));
                Assert.Greater(Stats.Moved, 0, algorithm + " moves the crowded labels");
                results[algorithm] = LiveMarkers.ToDictionary(m => m.PoiId, LabelScreen);
                SavePng(Render(), "displacement_algorithm_" + algorithm);
            }

            var same = Group("same").Select(m => m.PoiId).ToList();
            foreach (var id in same)
                Assert.AreEqual(labels[id].x, results["fixed_axis"][id].x, 1.5f, "Fixed Axis moves " + id + " straight up or down only");
            Assert.IsTrue(same.Any(id => Mathf.Abs(results["candidate_position"][id].x - labels[id].x) > 3f), "Candidate Position moves some label sideways");
            Assert.IsTrue(same.Any(id => Mathf.Abs(results["force_directed"][id].x - labels[id].x) > 3f), "Force Directed moves some label sideways");
            Assert.IsTrue(same.Any(id => (results["candidate_position"][id] - results["force_directed"][id]).magnitude > 3f),
                "the two spreading algorithms give different layouts");
        }

        // ---- Who Gives Way ----

        [UnityTest]
        public IEnumerator WhoGivesWay_LowerPriorityOnly_PinsTheMostImportantMarker_Symmetric_MovesIt()
        {
            // - Fixed Axis: under Symmetric every member of a group gets its own rung, the big marker included
            yield return StartDemo(D(s => { s.displace_target = "marker"; s.displacement_algorithm = "fixed_axis"; s.displacement_tiebreak = "symmetric"; }));
            var big = Live("ddemo_bigsmall_1");
            Assert.Greater((big.transform.position - big.UndisplacedWorldPosition).magnitude, 0.005f, "Symmetric: the big marker moves too");

            yield return Displacement(D(s => { s.displace_target = "marker"; s.displacement_algorithm = "fixed_axis"; s.displacement_tiebreak = "lower_priority_only"; }));
            Assert.Less((big.transform.position - big.UndisplacedWorldPosition).magnitude, 1e-4f, "Lower priority only: the big (top level) marker stays put");
            Assert.IsTrue(Group("bigsmall").Skip(1).Any(m => (m.transform.position - m.UndisplacedWorldPosition).magnitude > 0.005f),
                "the smaller markers move away from it");
            // - one shared level: nobody outranks anybody, so the group falls back to Symmetric
            Assert.Greater(Group("same").Count(m => (m.transform.position - m.UndisplacedWorldPosition).magnitude > 0.005f), 1,
                "the same-level group still moves as Symmetric");
            SavePng(Render(), "displacement_lower_priority_only_marker");
        }

        // ---- Max Move (px) ----

        [UnityTest]
        public IEnumerator MaxMove_CapsEveryMove_AndWithLabelOnly_HidesLabelsThatStillHaveNoRoom()
        {
            yield return StartDemo(D(s => s.max_displacement_px = 10f));
            Assert.LessOrEqual(Stats.LargestMovePx, 10.01f, "no move is longer than Max Move");
            Assert.Greater(Stats.LabelsHidden, 0, "Label only: labels that still overlap at the limit are hidden");
            Assert.AreEqual(Stats.LabelsHidden, LiveMarkers.Count(m => m.ShowsLabel && !m.LabelTextEnabled),
                "exactly that many labels are hidden on screen");

            yield return Displacement(D(s => { s.max_displacement_px = 10f; s.displace_target = "marker"; }));
            Assert.AreEqual(0, Stats.LabelsHidden, "Marker: nothing is ever hidden");
            Assert.IsTrue(LiveMarkers.All(m => m.LabelTextEnabled), "every label is shown again");

            yield return Displacement(D(s => s.max_displacement_px = 400f));
            Assert.AreEqual(0, Stats.LabelsHidden, "enough room: nothing hidden");
            Assert.Greater(Stats.LargestMovePx, 10.01f);
        }

        // ---- Leader lines ----

        [UnityTest]
        public IEnumerator LeaderLines_LabelOnly_DrawsALineFromTheSymbolToEachMovedLabel_Off_None()
        {
            yield return StartDemo(D(s => s.displace_target = "label_only"));
            Assert.Greater(ShownLines, 0, "Label only (the default) draws leader lines");
            foreach (var m in LiveMarkers.Where(v => Line(v).IsShown))
            {
                Assert.IsTrue(m.HasLabelOffset, m.PoiId + ": only a moved label gets a line");
                var lr = Renderer(m);
                Vector3 symbol = m.transform.position;
                Assert.Less((lr.GetPosition(0) - symbol).magnitude, (lr.GetPosition(lr.positionCount - 1) - symbol).magnitude,
                    m.PoiId + ": the line starts at the symbol and ends toward the label");
            }
            Assert.AreEqual(0, LiveMarkers.Count(m => !m.HasLabelOffset && Line(m).IsShown), "an unmoved marker has no line");

            yield return Displacement(D(s => s.leader_lines_enabled = false));
            Assert.AreEqual(0, ShownLines, "Enable Leader Lines off: no line");
            Assert.AreEqual(0, Stats.LeaderLines);
        }

        [UnityTest]
        public IEnumerator LeaderLines_Marker_RunFromTheTruePointToTheMovedMarker()
        {
            yield return StartDemo(D(s => s.displace_target = "marker"));
            var withLine = LiveMarkers.Where(m => Line(m).IsShown).ToList();
            Assert.IsNotEmpty(withLine);
            foreach (var m in withLine)
                Assert.Less((Renderer(m).GetPosition(0) - m.UndisplacedWorldPosition).magnitude, 1e-4f,
                    m.PoiId + ": the line starts exactly at the marker's true point");
        }

        [UnityTest]
        public IEnumerator LeaderLineStyle_Straight_Dashed_Elbow()
        {
            yield return StartDemo(D(s => s.leader_line_style = "straight"));
            var m = LiveMarkers.First(v => Line(v).IsShown);
            Assert.AreEqual(2, Renderer(m).positionCount);
            Assert.AreEqual(LineTextureMode.Stretch, Renderer(m).textureMode);

            yield return Displacement(D(s => { s.leader_line_style = "dashed"; s.leader_line_width = 0.02f; }));
            m = LiveMarkers.First(v => Line(v).IsShown);
            Assert.AreEqual(2, Renderer(m).positionCount);
            Assert.AreEqual(LineTextureMode.Tile, Renderer(m).textureMode, "dashed: the dash texture repeats along the line");
            var block = new MaterialPropertyBlock();
            Renderer(m).GetPropertyBlock(block);
            Assert.AreEqual("LeaderLineDash", block.GetTexture("_MainTex").name, "dashed: drawn with the dash texture");
            Assert.AreEqual(1f / (0.02f * MarkerLeaderLine.DashPeriodInWidths), Renderer(m).textureScale.x, 1e-3f);
            SavePng(Render(), "displacement_leader_line_dashed");

            yield return Displacement(D(s => s.leader_line_style = "elbow"));
            m = LiveMarkers.First(v => Line(v).IsShown);
            var lr = Renderer(m);
            Assert.AreEqual(3, lr.positionCount, "elbow: two legs");
            Assert.AreEqual(0f, (lr.GetPosition(1) - MarkerLeaderLine.ElbowPoint(lr.GetPosition(0), lr.GetPosition(2), Cam.transform.up)).magnitude, 1e-4f,
                "elbow: the corner is straight up/down on screen from the start");
        }

        [UnityTest]
        public IEnumerator LeaderLineMinLength_LongerThanEveryMove_HidesEveryLine()
        {
            yield return StartDemo(D(s => s.leader_line_min_distance_px = 0f));
            Assert.Greater(ShownLines, 0);
            yield return Displacement(D(s => s.leader_line_min_distance_px = Stats.LargestMovePx + 20f));
            Assert.AreEqual(0, ShownLines, "every move is shorter than Min Length: no line");
        }

        [UnityTest]
        public IEnumerator LeaderLineWidthAndOpacity_ReachTheDrawnLine_InTheMarkersCategoryColour()
        {
            yield return StartDemo(D(s => { s.leader_line_width = 0.02f; s.leader_line_opacity = 0.5f; }));
            var m = LiveMarkers.First(v => Line(v).IsShown);
            var lr = Renderer(m);
            Assert.AreEqual(0.02f, lr.startWidth, 1e-5f, "Width (m)");
            Color category = m.LeaderLineColor;
            Assert.AreEqual(category.r, lr.startColor.r, 1e-4f, "the marker's own category colour");
            Assert.AreEqual(category.a * 0.5f, lr.startColor.a, 0.005f, "Opacity 0.5 halves its alpha (8-bit colour)");
        }

        [UnityTest]
        public IEnumerator LeaderLine_RendersInItsColour_NeverTheMissingShaderMagenta()
        {
            yield return StartDemo(D(s => { s.leader_lines_enabled = false; }));
            var without = Render();
            yield return Displacement(D(s => { s.leader_line_width = 0.02f; }));
            Assert.Greater(ShownLines, 0);
            var lr = Renderer(LiveMarkers.First(v => Line(v).IsShown));
            Assert.IsNotNull(lr.sharedMaterial, "the prefab's LineRenderer has a material");
            Assert.AreEqual("Sprites/Default", lr.sharedMaterial.shader.name);
            Assert.AreEqual(UnityEngine.Rendering.ShadowCastingMode.Off, lr.shadowCastingMode, "a flat line casts no shadow");

            var with = Render();
            SavePng(with, "displacement_leader_lines_label_only");
            int changed = 0, magenta = 0;
            var a = without.GetPixels();
            var b = with.GetPixels();
            for (int i = 0; i < a.Length; i++)
            {
                if (Mathf.Abs(a[i].r - b[i].r) + Mathf.Abs(a[i].g - b[i].g) + Mathf.Abs(a[i].b - b[i].b) < 0.05f) continue;
                changed++;
                if (b[i].r > 0.95f && b[i].g < 0.05f && b[i].b > 0.95f) magenta++;
            }
            Assert.Greater(changed, 50, "the leader lines are really drawn");
            Assert.AreEqual(0, magenta, "no missing-shader magenta pixels");
        }

        // ---- who takes part ----

        [UnityTest]
        public IEnumerator HiddenMarkers_NeverPushTheVisibleOnes_AndKeepNoOffset()
        {
            Config.lod_settings = new LodSettings
            {
                enabled = true,
                bands = new List<LodBandEntry> { new LodBandEntry { max_distance_m = 9999f, max_visible_count = 1 } },
                density_response_mode = "none",
                transition_fade_duration_s = 0f,
                evaluation_interval_s = 999f,
            };
            Session.ApplyDisplacementSettings(D());
            yield return Demo(new DisplacementDemoSettings { run_lod = true });
            Assert.AreEqual(1, LiveMarkers.Count(m => m.IsVisible), "precondition: LOD shows one marker");
            Assert.AreEqual(1, Stats.Candidates, "only the visible marker takes part");
            Assert.AreEqual(0, Stats.Groups, "alone, it is not crowded -- invisible neighbours do not count");
            Assert.IsTrue(LiveMarkers.All(m => !m.HasLabelOffset && !Line(m).IsShown), "nothing is moved, nothing has a line");
        }
    }
}
