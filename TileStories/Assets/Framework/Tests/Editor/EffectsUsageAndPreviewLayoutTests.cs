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
        public void PreviewRows_HaveTheQuickAndComboGroupsPlusOneCellPerRealLevel()
        {
            var w = Wall();
            var (quickRow, comboRow, levelRow) = EffectsPreviewSpawner.BuildRows(w, 25f);

            // QUICK ROW: No effect, Pulse, Spin Ring.
            Assert.AreEqual(3, quickRow.Count, "No effect + Pulse + Spin Ring");
            Assert.AreEqual("No effect", quickRow[0].Name);
            Assert.AreEqual(MarkerEffectFlags.None, quickRow[0].Style.EffectFlags);
            Assert.AreEqual("Pulse", quickRow[1].Name);
            Assert.AreEqual(MarkerEffectFlags.Pulse, quickRow[1].Style.EffectFlags);
            Assert.AreEqual("Spin Ring", quickRow[2].Name);
            Assert.AreEqual(MarkerEffectFlags.None, quickRow[2].Style.EffectFlags);
            Assert.IsTrue(quickRow[2].Style.RotateContour, "the Spin Ring cell must actually rotate");
            foreach (var cell in quickRow)
            {
                Assert.AreEqual(25f, cell.Style.SizeCm, 1e-4f, "quick-row cells share the base size");
                Assert.IsTrue(cell.Style.ShowLabel, "quick-row cells are labelled");
                Assert.IsFalse(cell.IsSpacer);
            }

            // COMBO ROW: Ripple Rings, Ripple Discs, spacer, Halo Ring, Halo Disc, Beacon.
            Assert.AreEqual(6, comboRow.Count, "2 ripple variants + spacer + 3 halo variants");
            Assert.AreEqual(MarkerEffectFlags.RippleRings, comboRow[0].Style.EffectFlags);
            Assert.AreEqual(MarkerEffectFlags.RippleDiscs, comboRow[1].Style.EffectFlags);
            Assert.IsTrue(comboRow[2].IsSpacer, "a blank gap separates the Ripple group from the Halo group");
            Assert.AreEqual(MarkerEffectFlags.HaloRing, comboRow[3].Style.EffectFlags);
            Assert.AreEqual(MarkerEffectFlags.HaloDisc, comboRow[4].Style.EffectFlags);
            Assert.AreEqual(MarkerEffectFlags.Beacon, comboRow[5].Style.EffectFlags);

            Assert.AreEqual(3, levelRow.Count, "one cell per hierarchy level");
            Assert.AreEqual("Level: l1", levelRow[0].Name, "a level cell is NAMED after its level (blank level_name -> key)");
            Assert.AreEqual(MarkerEffectFlags.RippleDiscs | MarkerEffectFlags.HaloRing | MarkerEffectFlags.Pulse, levelRow[0].Style.EffectFlags);
            Assert.AreEqual(30f, levelRow[0].Style.SizeCm, 1e-4f, "level cells use the level's real size");
            Assert.AreEqual(0.2f, levelRow[1].Style.RevealDelaySeconds, 1e-4f, "and its real reveal timing");
            Assert.AreEqual(0.3f, levelRow[2].Style.RevealDurationSeconds, 1e-4f);
        }

        // A hierarchy level name is an authoring id, never marker text: a level cell's LABEL is the
        // base marker's own name (what a real POI at that level shows), while its NAME (GameObject)
        // still identifies the level for the Hierarchy window. Show Marker Label? is honoured.
        [Test]
        public void PreviewRows_LevelCellsShowTheBaseMarkerName_NeverTheLevelName_AndHonourShowLabel()
        {
            var w = Wall();
            w.hierarchy_levels[0].level_name = "Hub";
            w.hierarchy_levels[0].show_label = false;
            w.hierarchy_levels[1].level_name = "";   // blank level_name -> the key identifies the cell

            var (_, _, levelRow) = EffectsPreviewSpawner.BuildRows(w, 20f, "The Lamp");
            Assert.AreEqual("The Lamp", levelRow[0].LabelText, "the label shows the base marker's name");
            Assert.AreEqual("Level: Hub", levelRow[0].Name, "the cell is still identifiable by its level");
            Assert.IsFalse(levelRow[0].Style.ShowLabel, "Show Marker Label? off must hide the preview cell's label too");
            Assert.AreEqual("Level: l2", levelRow[1].Name);
            Assert.IsTrue(levelRow.All(c => c.LabelText == "The Lamp"), "no level cell ever shows a level name");

            var (_, _, plainRow) = EffectsPreviewSpawner.BuildRows(w, 20f);
            Assert.IsTrue(plainRow.All(c => c.LabelText == EffectsPreviewSpawner.PlainCircleName),
                "with no base POI the label is 'Plain grey circle', the name the Base marker dropdown uses");
        }

        // The grid's level cells get the level's WHOLE style (StyleOf), Marker Label Style included --
        // the grid once dropped every label override because it built its own 6-field style.
        [Test]
        public void PreviewRows_LevelCellsCarryTheLevelsMarkerLabelStyle()
        {
            var w = Wall();
            w.hierarchy_levels[0].override_label_style = true;
            w.hierarchy_levels[0].label_gap_ratio = 0.3f;
            w.hierarchy_levels[0].label_font_size_ratio = 0.7f;
            w.hierarchy_levels[0].label_font_key = "oswald_bold";

            var (_, _, levelRow) = EffectsPreviewSpawner.BuildRows(w, 20f);

            var style = levelRow[0].Style;
            Assert.IsTrue(style.OverridesLabelStyle);
            Assert.AreEqual(0.3f, style.LabelGapRatio, 1e-5f);
            Assert.AreEqual(0.7f, style.LabelFontSizeRatio, 1e-5f);
            Assert.AreEqual("oswald_bold", style.LabelFontKey);
            Assert.IsFalse(levelRow[1].Style.OverridesLabelStyle, "a level without the override follows the wall default");
        }

        [Test]
        public void PreviewRows_LabelSwitchedOffEffects_SoAStillCellIsNotMistakenForABrokenOne()
        {
            var w = Wall();
            w.effect_defaults.beacon.enabled = false;
            var (_, comboRow, _) = EffectsPreviewSpawner.BuildRows(w, 20f);
            Assert.AreEqual("Beacon (off)", comboRow.Last().Name);

            var (quickRow, _, _) = EffectsPreviewSpawner.BuildRows(w, 20f);
            Assert.AreEqual("Pulse", quickRow[1].Name);

            w.effect_defaults.effects_enabled = false;
            (quickRow, comboRow, _) = EffectsPreviewSpawner.BuildRows(w, 20f);
            Assert.AreEqual("Pulse (off)", quickRow[1].Name, "master off marks Pulse too");
            Assert.IsTrue(comboRow.Where(c => !c.IsSpacer).All(c => c.Name.EndsWith("(off)")), "master off marks every combo-row effect");
        }

        [Test]
        public void PreviewRows_WithNoHierarchyLevels_HaveOnlyTheQuickAndComboRows()
        {
            var (quickRow, comboRow, levelRow) = EffectsPreviewSpawner.BuildRows(new WallConfigData { effect_defaults = new EffectDefaults() }, 20f);
            Assert.AreEqual(3, quickRow.Count);
            Assert.AreEqual(6, comboRow.Count);
            Assert.AreEqual(0, levelRow.Count);
        }

                        [Test]
        public void PreviewPlacement_ColumnsAdaptToTheView_CellsNeverOverlap_AndTheWholeGridFits()
        {
            const float fov = 60f;
            foreach (float aspect in new[] { 0.5f, 0.89f, 1.78f, 2.4f })
                foreach (int levels in new[] { 0, 3, 5, 12 })
                {
                    var blocks = new[] { 3, 6, levels };
                    int columns = EffectsPreviewSpawner.ChooseColumns(blocks, fov, aspect);
                    var extent = EffectsPreviewSpawner.GridExtent(blocks, columns);
                    float distance = EffectsPreviewSpawner.FitDistance(extent, fov, aspect);
                    var positions = EffectsPreviewSpawner.CellPositions(blocks, columns);
                    string ctx = $"aspect {aspect}, {levels} levels";

                    Assert.AreEqual(9 + levels, positions.Count, ctx + ": one position per cell");
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
                        Assert.LessOrEqual(distance, EffectsPreviewSpawner.FitDistance(EffectsPreviewSpawner.GridExtent(blocks, other), fov, aspect) + 1e-3f,
                            $"{ctx}: {other} columns would need less distance than the chosen {columns}");
                }

            // A portrait view wraps into fewer columns than a landscape one; a wider view never needs more distance.
            var demoBlocks = new[] { 3, 6, 5 };
            int portrait = EffectsPreviewSpawner.ChooseColumns(demoBlocks, 60f, 0.5f);
            int landscape = EffectsPreviewSpawner.ChooseColumns(demoBlocks, 60f, 2.4f);
            Assert.Less(portrait, landscape, "portrait wraps into fewer columns");
            Assert.LessOrEqual(
                EffectsPreviewSpawner.FitDistance(EffectsPreviewSpawner.GridExtent(demoBlocks, landscape), 60f, 2.4f),
                EffectsPreviewSpawner.FitDistance(EffectsPreviewSpawner.GridExtent(demoBlocks, portrait), 60f, 0.5f) + 1e-4f,
                "a wider view never needs more distance");

            // Each block comes before the next, top to bottom, rows centred.
            var pos = EffectsPreviewSpawner.CellPositions(new[] { 3, 6, 5 }, 6);
            Assert.Greater(pos[0].y, pos[3].y, "the quick row is above the combo row");
            Assert.Greater(pos[3].y, pos[9].y, "the combo row is above the level row");
            Assert.AreEqual(-pos[3].x, pos[8].x, 1e-4f, "a full row (6 cells) is centred");
            Assert.AreEqual(0f, pos[9].x + pos[13].x, 1e-4f, "a short last row (5 cells over 6 columns) is centred too");
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
