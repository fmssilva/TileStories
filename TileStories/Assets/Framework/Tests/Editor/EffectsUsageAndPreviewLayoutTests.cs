using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Pure logic of the Effects domain, tested with plain `new` data (40-testing 4.2): the per-effect
    // switches, the editor's usage / dropdown text, and the preview grid's cell list and placement.
    public class EffectsUsageAndPreviewLayoutTests
    {
        [TearDown]
        public void TearDown() => MarkerHierarchyResolver.ResetToDefaults();

        private static WallConfigData Wall()
        {
            var config = new WallConfigData { wall_id = "w" };
            config.effect_defaults = new EffectDefaults();
            config.hierarchy_levels.Add(new HierarchyLevelEntry { key = "l1", size_cm = 30f, ripple_effect = "ripple_discs", halo_effect = "halo_ring", pulse = true, reveal_delay_s = 0f, reveal_duration_s = 0.5f });
            config.hierarchy_levels.Add(new HierarchyLevelEntry { key = "l2", size_cm = 20f, ripple_effect = "ripple_rings", halo_effect = "none", pulse = false, reveal_delay_s = 0.2f, reveal_duration_s = 0.4f });
            config.hierarchy_levels.Add(new HierarchyLevelEntry { key = "l3", size_cm = 10f, ripple_effect = "none", halo_effect = "beacon", pulse = true, reveal_delay_s = 0.5f, reveal_duration_s = 0.3f });
            config.pois.Add(new POIData { id = "a", name = "Alpha", hierarchy_level_key = "l1" });
            config.pois.Add(new POIData { id = "b", name = "Bravo", hierarchy_level_key = "l1" });
            config.pois.Add(new POIData { id = "c", name = "Charlie", hierarchy_level_key = "l2" });
            return config;
        }

        // ---------------- EffectDefaults switches ----------------

        [Test]
        public void FilterEnabled_MasterSwitchThenEachEffectsOwnCheckbox()
        {
            var d = new EffectDefaults();
            var everything = MarkerEffectFlags.Pulse | MarkerEffectFlags.RippleRings | MarkerEffectFlags.RippleDiscs
                | MarkerEffectFlags.HaloRing | MarkerEffectFlags.HaloDisc | MarkerEffectFlags.Beacon;
            Assert.AreEqual(everything, d.FilterEnabled(everything), "everything on by default");

            d.halo_disc.enabled = false;
            d.pulse.enabled = false;
            Assert.AreEqual(MarkerEffectFlags.RippleRings | MarkerEffectFlags.RippleDiscs | MarkerEffectFlags.HaloRing | MarkerEffectFlags.Beacon,
                d.FilterEnabled(everything), "only the two unticked effects drop out");
            Assert.AreEqual(MarkerEffectFlags.None, d.FilterEnabled(MarkerEffectFlags.Pulse | MarkerEffectFlags.HaloDisc), "requesting only disabled effects yields none");

            d.effects_enabled = false;
            Assert.AreEqual(MarkerEffectFlags.None, d.FilterEnabled(everything), "master switch off silences all");
            Assert.IsTrue(d.IsEffectEnabled(MarkerEffectFlags.RippleRings), "IsEffectEnabled ignores the master switch");
            Assert.IsFalse(d.IsEffectEnabled(MarkerEffectFlags.HaloDisc));
        }

        // ---------------- usage lines ----------------

        [Test]
        public void DescribeUsage_ListsTheLevelsThatUseTheEffect_WithPoiCounts()
        {
            var w = Wall();
            Assert.AreEqual("Used by: l1, l3 (2 POIs).", EffectUsageSummary.DescribeUsage(w, MarkerEffectFlags.Pulse));
            Assert.AreEqual("Used by: l2 (1 POI).", EffectUsageSummary.DescribeUsage(w, MarkerEffectFlags.RippleRings));
            StringAssert.StartsWith("Not used by any hierarchy level", EffectUsageSummary.DescribeUsage(w, MarkerEffectFlags.HaloDisc));
            StringAssert.Contains("(0 POIs)", EffectUsageSummary.DescribeUsage(w, MarkerEffectFlags.Beacon), "a level with no POIs still counts as used");
        }

        [Test]
        public void DescribeLevelEffects_NamesEffectsAndRevealTiming_AndMarksSwitchedOffOnes()
        {
            var w = Wall();
            Assert.AreEqual("Effects: Pulse, Ripple Discs, Halo Ring. Reveal: 0 s delay, 0.5 s fade-in.",
                EffectUsageSummary.DescribeLevelEffects(w.hierarchy_levels[0], w.effect_defaults));

            w.effect_defaults.halo_ring.enabled = false;
            StringAssert.Contains("Halo Ring (off)", EffectUsageSummary.DescribeLevelEffects(w.hierarchy_levels[0], w.effect_defaults));

            w.effect_defaults.effects_enabled = false;
            StringAssert.Contains("Pulse (off)", EffectUsageSummary.DescribeLevelEffects(w.hierarchy_levels[0], w.effect_defaults));

            Assert.AreEqual("Effects: none. Reveal: 0 s delay, 0 s fade-in.",
                EffectUsageSummary.DescribeLevelEffects(new HierarchyLevelEntry(), null));
            StringAssert.Contains("no hierarchy level selected", EffectUsageSummary.DescribeLevelEffects(null, null));
        }

        // ---------------- level-table dropdown filtering ----------------

        [Test]
        public void FilterEnabledOptions_HidesDisabledEffects_ButKeepsTheCurrentValueVisible()
        {
            var options = new[] { "none", "ripple_rings", "ripple_discs" };
            var labels = new[] { "None", "Rings", "Discs" };
            var d = new EffectDefaults();
            d.ripple_discs.enabled = false;

            EffectUsageSummary.FilterEnabledOptions(options, labels, d, "ripple_rings", out var shown, out var shownLabels);
            CollectionAssert.AreEqual(new[] { "none", "ripple_rings" }, shown, "the disabled effect is not offered");
            CollectionAssert.AreEqual(new[] { "None", "Rings" }, shownLabels);

            EffectUsageSummary.FilterEnabledOptions(options, labels, d, "ripple_discs", out shown, out shownLabels);
            CollectionAssert.AreEqual(options, shown, "a level already using it keeps it in the list");
            Assert.AreEqual("Discs (disabled)", shownLabels[2], "and it is clearly marked");

            var halos = new[] { "none", "halo_ring", "halo_disc", "beacon" };
            var haloLabels = new[] { "None", "Ring", "Disc", "Beacon" };
            d.beacon.enabled = false;
            d.effects_enabled = false;
            EffectUsageSummary.FilterEnabledOptions(halos, haloLabels, d, "none", out shown, out _);
            CollectionAssert.AreEqual(new[] { "none", "halo_ring", "halo_disc" }, shown, "the master switch does not hide options (it is a temporary kill switch)");
        }

        [Test]
        public void PreviewBaseOptions_AlwaysStartWithThePlainCircle_ThenTheWallsPois()
        {
            EffectUsageSummary.PreviewBaseOptions(new WallConfigData(), out var ids, out var labels);
            CollectionAssert.AreEqual(new[] { "" }, ids);
            CollectionAssert.AreEqual(new[] { "Plain grey circle" }, labels);

            EffectUsageSummary.PreviewBaseOptions(Wall(), out ids, out labels);
            CollectionAssert.AreEqual(new[] { "", "a", "b", "c" }, ids);
            Assert.AreEqual("Alpha (a)", labels[1]);
            EffectUsageSummary.PreviewBaseOptions(null, out ids, out _);
            Assert.AreEqual(1, ids.Length, "a null config still offers the plain circle");
        }

        // ---------------- preview grid: cells and placement ----------------

        [Test]
        public void PreviewRows_HaveAControlPlusOneCellPerEffect_AndOneCellPerRealLevel()
        {
            var w = Wall();
            var (effectRow, levelRow) = EffectsPreviewSpawner.BuildRows(w, 25f);

            Assert.AreEqual(7, effectRow.Count, "No effect + 6 effects");
            Assert.AreEqual("No effect", effectRow[0].Name);
            Assert.AreEqual(MarkerEffectFlags.None, effectRow[0].Style.EffectFlags);
            for (int i = 0; i < EffectDefaults.SelectableEffects.Length; i++)
            {
                var effect = EffectDefaults.SelectableEffects[i];
                Assert.AreEqual(MarkerEffectNames.DisplayName(effect), effectRow[i + 1].Name);
                Assert.AreEqual(effect, effectRow[i + 1].Style.EffectFlags, "each effect cell shows exactly that effect");
                Assert.AreEqual(25f, effectRow[i + 1].Style.SizeCm, 1e-4f, "effect cells share the base size");
                Assert.IsTrue(effectRow[i + 1].Style.ShowLabel, "cells are labelled");
            }

            Assert.AreEqual(3, levelRow.Count, "one cell per hierarchy level");
            Assert.AreEqual("l1", levelRow[0].Name);
            Assert.AreEqual(MarkerEffectFlags.RippleDiscs | MarkerEffectFlags.HaloRing | MarkerEffectFlags.Pulse, levelRow[0].Style.EffectFlags);
            Assert.AreEqual(30f, levelRow[0].Style.SizeCm, 1e-4f, "level cells use the level's real size");
            Assert.AreEqual(0.2f, levelRow[1].Style.RevealDelaySeconds, 1e-4f, "and its real reveal timing");
            Assert.AreEqual(0.3f, levelRow[2].Style.RevealDurationSeconds, 1e-4f);
        }

        [Test]
        public void PreviewRows_LabelSwitchedOffEffects_SoAStillCellIsNotMistakenForABrokenOne()
        {
            var w = Wall();
            w.effect_defaults.beacon.enabled = false;
            var (effectRow, _) = EffectsPreviewSpawner.BuildRows(w, 20f);
            Assert.AreEqual("Beacon (off)", effectRow.Last().Name);
            Assert.AreEqual("Pulse", effectRow[1].Name);

            w.effect_defaults.effects_enabled = false;
            (effectRow, _) = EffectsPreviewSpawner.BuildRows(w, 20f);
            Assert.IsTrue(effectRow.Skip(1).All(c => c.Name.EndsWith("(off)")), "master off marks every effect");
        }

        [Test]
        public void PreviewRows_WithNoHierarchyLevels_HaveOnlyTheEffectRow()
        {
            var (effectRow, levelRow) = EffectsPreviewSpawner.BuildRows(new WallConfigData { effect_defaults = new EffectDefaults() }, 20f);
            Assert.AreEqual(7, effectRow.Count);
            Assert.AreEqual(0, levelRow.Count);
        }

                        [Test]
        public void PreviewPlacement_ColumnsAdaptToTheView_CellsNeverOverlap_AndTheWholeGridFits()
        {
            const float fov = 60f;
            foreach (float aspect in new[] { 0.5f, 0.89f, 1.78f, 2.4f })
                foreach (int levels in new[] { 0, 3, 5, 12 })
                {
                    const int effects = 7;
                    int columns = EffectsPreviewSpawner.ChooseColumns(effects, levels, fov, aspect);
                    var extent = EffectsPreviewSpawner.GridExtent(effects, levels, columns);
                    float distance = EffectsPreviewSpawner.FitDistance(extent, fov, aspect);
                    var positions = EffectsPreviewSpawner.CellPositions(effects, levels, columns);
                    string ctx = $"aspect {aspect}, {levels} levels";

                    Assert.AreEqual(effects + levels, positions.Count, ctx + ": one position per cell");
                    Assert.GreaterOrEqual(distance, 0.5f, ctx + ": never closer than the minimum");

                    // No two cells share a spot and neighbours keep at least one cell spacing apart.
                    for (int i = 0; i < positions.Count; i++)
                        for (int j = i + 1; j < positions.Count; j++)
                            Assert.GreaterOrEqual(Vector2.Distance(positions[i], positions[j]), 0.49f, $"{ctx}: cells {i} and {j} overlap");

                    // At that distance every cell centre, plus the marker margin, is inside the visible area.
                    float visH = 2f * distance * Mathf.Tan(fov * Mathf.Deg2Rad * 0.5f), visW = visH * aspect;
                    foreach (var p in positions)
                    {
                        Assert.LessOrEqual(Mathf.Abs(p.x) + 0.25f, visW * 0.5f, ctx + ": cell outside the view horizontally");
                        Assert.LessOrEqual(Mathf.Abs(p.y) + 0.25f, visH * 0.5f, ctx + ": cell outside the view vertically");
                    }

                    // The chosen column count really is the best: no other count needs less distance.
                    for (int other = 1; other <= 12; other++)
                        Assert.LessOrEqual(distance, EffectsPreviewSpawner.FitDistance(EffectsPreviewSpawner.GridExtent(effects, levels, other), fov, aspect) + 1e-3f,
                            $"{ctx}: {other} columns would need less distance than the chosen {columns}");
                }

            // A portrait view wraps into fewer columns than a landscape one; a wider view never needs more distance.
            int portrait = EffectsPreviewSpawner.ChooseColumns(7, 5, 60f, 0.5f);
            int landscape = EffectsPreviewSpawner.ChooseColumns(7, 5, 60f, 2.4f);
            Assert.Less(portrait, landscape, "portrait wraps into fewer columns");
            Assert.LessOrEqual(
                EffectsPreviewSpawner.FitDistance(EffectsPreviewSpawner.GridExtent(7, 5, landscape), 60f, 2.4f),
                EffectsPreviewSpawner.FitDistance(EffectsPreviewSpawner.GridExtent(7, 5, portrait), 60f, 0.5f) + 1e-4f,
                "a wider view never needs more distance");

            // The effect block comes first (top), the level block below it, rows centred.
            var pos = EffectsPreviewSpawner.CellPositions(7, 5, 4);
            Assert.Greater(pos[0].y, pos[7].y, "effect block is above the level block");
            Assert.AreEqual(-pos[3].x, pos[0].x, 1e-4f, "a full row is centred");
            Assert.AreEqual(0f, pos[4].x + pos[6].x, 1e-4f, "a short last row (3 cells) is centred too");
        }

        [Test]
        public void ReleaseBuilds_IgnoreThePreviewSwitch()
        {
            Assert.IsTrue(EffectsPreviewSpawner.IsAllowed(true, false), "Editor");
            Assert.IsTrue(EffectsPreviewSpawner.IsAllowed(false, true), "development build");
            Assert.IsFalse(EffectsPreviewSpawner.IsAllowed(false, false), "release build");
        }
    }
}
