using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Editor.Tests
{
    // Hierarchy Levels domain (_2.3_Marker_Hierarchy.md), authoring side. Mirrors
    // EffectsAuthoringTests' guide-contract shape: reads the Test sub-foldout's guide
    // constants and default foldout state by reflection, real code only, no mocks.
    public class HierarchyDesignAuthoringTests
    {
        private const BindingFlags Static = BindingFlags.NonPublic | BindingFlags.Static;
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;

        [Test]
        public void HierarchyTestGuides_ExistAsciiCoverEveryColumn_AndNamesEveryPoiLevel()
        {
            var t = typeof(POIEditorToolWindow);

            foreach (string guideName in new[] { "HierarchySceneTestGuide", "HierarchyPlaymodeTestGuide", "HierarchyDeviceTestGuide" })
            {
                var guide = (string)t.GetField(guideName, Static)?.GetValue(null);
                Assert.IsFalse(string.IsNullOrWhiteSpace(guide), guideName + " must exist and not be empty");
                AssertAscii(guide, guideName);
            }

            var sceneGuide = (string)t.GetField("HierarchySceneTestGuide", Static).GetValue(null);
            StringAssert.Contains("Not possible in Scene test", sceneGuide, "Scene guide must say what it cannot show");
            foreach (string term in new[] { "MARKER SIZE", "MARKER LABEL", "Show Marker Label?", "Override global label config", "Fallback", "SPIN RING" })
                StringAssert.Contains(term, sceneGuide, "Scene guide must name " + term);

            var playmodeGuide = (string)t.GetField("HierarchyPlaymodeTestGuide", Static).GetValue(null);
            foreach (string term in new[] { "Priority", "Facing Override", "Add Hierarchy demo grid", "LIVE", "Hierarchy Level" })
                StringAssert.Contains(term, playmodeGuide, "Playmode guide must name " + term);

            var deviceGuide = (string)t.GetField("HierarchyDeviceTestGuide", Static).GetValue(null);
            StringAssert.Contains("adb", deviceGuide, "Device guide must give the adb log recipe");
            StringAssert.Contains("Untick 'Add Hierarchy demo grid'", deviceGuide, "Device guide must say to switch the dev-only grid off");
        }

        // Every text the developer reads in the window (the hierarchy and labels (i) helps and guides)
        // points at Editor Tab controls only: never a project doc, a code file or class, one specific
        // wall, or a column name that no longer exists (_5.1_Editor_Tab.md, "Domain Manual Tests").
        [Test]
        public void HierarchyAndLabelTexts_NameOnlyEditorControls_NoDocsCodeWallNamesOrStaleColumnNames()
        {
            var t = typeof(POIEditorToolWindow);
            var names = new[]
            {
                "HierarchySceneTestGuide", "HierarchyPlaymodeTestGuide", "HierarchyDeviceTestGuide", "HierarchyPreviewHelp",
                "HierarchySearchKeywordsHelp", "LabelGapRatioHelp", "LabelFontSizeRatioHelp", "LabelFontKeyHelp", "AddFontHelp",
                "WallFontLibraryPresentHelp", "LevelLabelStyleOverrideHelp", "LabelsPreviewHelp",
                "LabelsAndFontsSceneTestGuide", "LabelsAndFontsPlaymodeTestGuide", "LabelsAndFontsDeviceTestGuide",
            };
            var forbidden = new[] { ".md", ".cs", "_2.", "_5.1", "LivingRoom", "MockLocalizationProvider", "MarkerGalleryScene",
                "Text Label", "Show Text Label", "Label style popup", "Create Wall Font Library" };
            foreach (string name in names)
            {
                var field = t.GetField(name, Static);
                Assert.IsNotNull(field, name + " must exist");
                string text = (string)field.GetValue(null);
                AssertAscii(text, name);
                foreach (string term in forbidden)
                    StringAssert.DoesNotContain(term, text, name + " must not contain '" + term + "'");
            }
        }

        [Test]
        public void HierarchyTestGuides_DoNotHardcodeAWallsRealPoiIdsOrCategories()
        {
            // Guide text must describe framework behaviour, not one specific wall's taxonomy
            // (_5.1_Editor_Tab.md, "Domain Manual Tests" > "Guide content must stay app-agnostic").
            // These are the real LivingRoom POI ids/categories that must never be assumed to exist.
            var t = typeof(POIEditorToolWindow);
            var forbidden = new[] { "lamp", "painting_", "camera_", "dev_marker_", "dev_disp_", "lamp_military", "lamp_economic" };

            foreach (string guideName in new[] { "HierarchySceneTestGuide", "HierarchyPlaymodeTestGuide", "HierarchyDeviceTestGuide" })
            {
                var guide = (string)t.GetField(guideName, Static).GetValue(null);
                foreach (string term in forbidden)
                    StringAssert.DoesNotContain(term, guide, guideName + " must not hardcode the real wall-specific id/category '" + term + "'");
            }
        }

        [Test]
        public void HierarchyTest_FoldoutState_MatchesTheDomainManualTestsConvention()
        {
            // Test foldout opens by default; its three guides start collapsed
            // (_5.1_Editor_Tab.md, "Domain Manual Tests").
            var window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            try
            {
                var field = typeof(POIEditorToolWindow).GetField("_hierarchyTest", Instance);
                Assert.IsNotNull(field, "_hierarchyTest field must exist");
                var state = field.GetValue(window);
                Assert.IsNotNull(state, "_hierarchyTest must be constructed at field-init time");

                var stateType = state.GetType();
                Assert.IsTrue((bool)stateType.GetField("Open", Instance | BindingFlags.Public).GetValue(state), "Test foldout must default open");
                Assert.IsFalse((bool)stateType.GetField("Scene", Instance | BindingFlags.Public).GetValue(state), "Scene guide must default collapsed");
                Assert.IsFalse((bool)stateType.GetField("Playmode", Instance | BindingFlags.Public).GetValue(state), "Playmode guide must default collapsed");
                Assert.IsFalse((bool)stateType.GetField("Device", Instance | BindingFlags.Public).GetValue(state), "Device guide must default collapsed");
            }
            finally
            {
                Object.DestroyImmediate(window);
            }
        }

        // "+ Add hierarchy level" must never hand out a key that already exists: the key is the
        // identity every POI points at. The old "level_" + (Count + 1) collided as soon as a middle
        // row had been deleted (rows level_1, level_3 -> a second level_3).
        [Test]
        public void NextFreeHierarchyLevelKey_NeverRepeatsAnExistingKey()
        {
            var levels = new List<global::TileStories.HierarchyLevelEntry>
            {
                new global::TileStories.HierarchyLevelEntry { key = "level_1" },
                new global::TileStories.HierarchyLevelEntry { key = "level_3" },
            };
            Assert.AreEqual("level_4", POIEditorToolWindow.NextFreeHierarchyLevelKey(levels), "level_3 is taken, so the next free one");

            levels[1].key = "hub";
            Assert.AreEqual("level_3", POIEditorToolWindow.NextFreeHierarchyLevelKey(levels));
            Assert.AreEqual("level_1", POIEditorToolWindow.NextFreeHierarchyLevelKey(new List<global::TileStories.HierarchyLevelEntry>()));
            Assert.AreEqual("level_1", POIEditorToolWindow.NextFreeHierarchyLevelKey(null));
        }

        [Test]
        public void HierarchyLevelEntry_SearchKeywords_RoundTripsThroughJson()
        {
            // Tier 0: the new Search Keywords table column writes entry.search_keywords,
            // an existing schema field (_2.6_Select_Filter_Search.md section 6). Confirm
            // the data layer itself round-trips correctly, independent of the OnGUI cell.
            var entry = new global::TileStories.HierarchyLevelEntry
            {
                key = "level_1",
                level_name = "Hub",
                search_keywords = new List<string> { "landmark", "hero" }
            };

            string json = JsonUtility.ToJson(entry);
            var roundTripped = JsonUtility.FromJson<global::TileStories.HierarchyLevelEntry>(json);

            CollectionAssert.AreEqual(entry.search_keywords, roundTripped.search_keywords);
        }

        // ---- Live Play Mode fingerprint ownership: each hierarchy-level column belongs to
        // exactly one applier (_5.1_Editor_Tab.md "PlayMode Live Config"). Size/Show Label/
        // Rotate Contour/Reveal timing belong to LivePlayModeMarkerApplier (WallSession.
        // ApplyMarkerSettings already re-applies hierarchy levels to every real marker; this
        // fix wires the fingerprint so the dispatcher actually calls it). Ripple/Halo/Pulse
        // stay owned by LivePlayModeEffectsApplier; Facing Override by
        // LivePlayModeOrientationApplier -- confirmed here so an edit to one domain's column
        // never silently re-triggers a different domain's re-apply.

        private static global::TileStories.WallConfigData MakeConfigWithOneLevel()
        {
            var config = new global::TileStories.WallConfigData
            {
                pois = new List<global::TileStories.POIData>(),
                hierarchy_levels = new List<global::TileStories.HierarchyLevelEntry>
                {
                    new global::TileStories.HierarchyLevelEntry
                    {
                        key = "level_1", level_name = "Hub", priority = 1, size_cm = 20f, show_label = true,
                        ripple_effect = "none", halo_effect = "none", pulse = false,
                        rotate_contour = false, reveal_delay_s = 0f, reveal_duration_s = 0.35f,
                        facing_mode_override = "", search_keywords = new List<string>()
                    }
                }
            };
            return config;
        }

        [Test]
        public void MarkerApplier_Fingerprint_ChangesForSizeShowLabelRotateAndReveal()
        {
            var applier = new LivePlayModeMarkerApplier();
            var baseline = MakeConfigWithOneLevel();
            string baseFp = applier.Fingerprint(baseline);

            var sizeEdited = MakeConfigWithOneLevel();
            sizeEdited.hierarchy_levels[0].size_cm = 25f;
            Assert.AreNotEqual(baseFp, applier.Fingerprint(sizeEdited), "Size (cm) edit must change the marker applier's fingerprint");

            var labelEdited = MakeConfigWithOneLevel();
            labelEdited.hierarchy_levels[0].show_label = false;
            Assert.AreNotEqual(baseFp, applier.Fingerprint(labelEdited), "Text Label edit must change the marker applier's fingerprint");

            var rotateEdited = MakeConfigWithOneLevel();
            rotateEdited.hierarchy_levels[0].rotate_contour = true;
            Assert.AreNotEqual(baseFp, applier.Fingerprint(rotateEdited), "Spin Ring edit must change the marker applier's fingerprint");

            var revealEdited = MakeConfigWithOneLevel();
            revealEdited.hierarchy_levels[0].reveal_delay_s = 1.5f;
            Assert.AreNotEqual(baseFp, applier.Fingerprint(revealEdited), "Reveal Delay edit must change the marker applier's fingerprint");
        }

        [Test]
        public void MarkerApplier_Fingerprint_IgnoresColumnsOwnedByEffectsAndOrientationAppliers()
        {
            var applier = new LivePlayModeMarkerApplier();
            var baseline = MakeConfigWithOneLevel();
            string baseFp = applier.Fingerprint(baseline);

            var effectsEdited = MakeConfigWithOneLevel();
            effectsEdited.hierarchy_levels[0].ripple_effect = "ripple_rings";
            effectsEdited.hierarchy_levels[0].halo_effect = "beacon";
            effectsEdited.hierarchy_levels[0].pulse = true;
            Assert.AreEqual(baseFp, applier.Fingerprint(effectsEdited), "Ripple/Halo/Pulse belong to LivePlayModeEffectsApplier, not the marker applier");

            var facingEdited = MakeConfigWithOneLevel();
            facingEdited.hierarchy_levels[0].facing_mode_override = "always_facing_camera";
            Assert.AreEqual(baseFp, applier.Fingerprint(facingEdited), "Facing Override belongs to LivePlayModeOrientationApplier, not the marker applier");
        }

        [Test]
        public void EffectsApplier_Fingerprint_IgnoresColumnsOwnedByTheMarkerApplier()
        {
            // Symmetric check: a Size/Show Label/Rotate/Reveal edit must not re-trigger a
            // needless effects rebuild either.
            var applier = new LivePlayModeEffectsApplier();
            var baseline = MakeConfigWithOneLevel();
            baseline.effect_defaults = new global::TileStories.EffectDefaults();
            string baseFp = applier.Fingerprint(baseline);

            var sizeEdited = MakeConfigWithOneLevel();
            sizeEdited.effect_defaults = new global::TileStories.EffectDefaults();
            sizeEdited.hierarchy_levels[0].size_cm = 99f;
            sizeEdited.hierarchy_levels[0].reveal_delay_s = 9f;
            Assert.AreEqual(baseFp, applier.Fingerprint(sizeEdited), "Size/Reveal belong to the marker applier, not effects");
        }

        // ---- Dropdown options actually parse at runtime (mirrors MarkerDesignAuthoringTests'
        // "every editor-offered option parses at runtime" convention -- the free_colors bug class).

        [Test]
        public void RippleAndHaloDropdownOptions_AllParseToTheExpectedEffectFlag()
        {
            var rippleOptions = (string[])typeof(POIEditorToolWindow).GetField("RippleEffectOptions", Static).GetValue(null);
            var haloOptions = (string[])typeof(POIEditorToolWindow).GetField("HaloEffectOptions", Static).GetValue(null);

            var expectedRipple = new Dictionary<string, global::TileStories.MarkerEffectFlags>
            {
                { "none", global::TileStories.MarkerEffectFlags.None },
                { "ripple_rings", global::TileStories.MarkerEffectFlags.RippleRings },
                { "ripple_discs", global::TileStories.MarkerEffectFlags.RippleDiscs },
            };
            CollectionAssert.AreEquivalent(expectedRipple.Keys, rippleOptions, "RippleEffectOptions must match every string MarkerHierarchyResolver parses");
            foreach (var option in rippleOptions)
            {
                var entry = new global::TileStories.HierarchyLevelEntry { ripple_effect = option, halo_effect = "none" };
                Assert.AreEqual(expectedRipple[option], global::TileStories.MarkerHierarchyResolver.EffectFlagsOf(entry, logWarnings: false), "'" + option + "' must resolve to " + expectedRipple[option]);
            }

            var expectedHalo = new Dictionary<string, global::TileStories.MarkerEffectFlags>
            {
                { "none", global::TileStories.MarkerEffectFlags.None },
                { "halo_ring", global::TileStories.MarkerEffectFlags.HaloRing },
                { "halo_disc", global::TileStories.MarkerEffectFlags.HaloDisc },
                { "beacon", global::TileStories.MarkerEffectFlags.Beacon },
            };
            CollectionAssert.AreEquivalent(expectedHalo.Keys, haloOptions, "HaloEffectOptions must match every string MarkerHierarchyResolver parses");
            foreach (var option in haloOptions)
            {
                var entry = new global::TileStories.HierarchyLevelEntry { ripple_effect = "none", halo_effect = option };
                Assert.AreEqual(expectedHalo[option], global::TileStories.MarkerHierarchyResolver.EffectFlagsOf(entry, logWarnings: false), "'" + option + "' must resolve to " + expectedHalo[option]);
            }
        }

        [Test]
        public void FacingOverrideDropdownOptions_AllResolveThroughTheRealResolver()
        {
            var options = (string[])typeof(POIEditorToolWindow).GetField("FacingModeOverrideOptions", Static).GetValue(null);
            var labels = (string[])typeof(POIEditorToolWindow).GetField("FacingModeOverrideLabels", Static).GetValue(null);
            Assert.AreEqual(options.Length, labels.Length, "Every Facing Override option must have exactly one label");
            CollectionAssert.AreEquivalent(new[] { "", "wall_fixed", "yaw_only", "always_facing_camera" }, options,
                "FacingModeOverrideOptions must match every value _2.1_Marker_Orientation.md's facing_mode accepts");

            var levels = new List<global::TileStories.HierarchyLevelEntry>();
            foreach (var option in options)
                levels.Add(new global::TileStories.HierarchyLevelEntry { key = "k_" + (option.Length == 0 ? "inherit" : option), facing_mode_override = option });

            global::TileStories.MarkerHierarchyResolver.Configure(levels);
            try
            {
                foreach (var level in levels)
                    Assert.AreEqual(level.facing_mode_override, global::TileStories.MarkerHierarchyResolver.ResolveFacingModeOverride(level.key),
                        "'" + level.facing_mode_override + "' must round-trip through the real resolver unchanged");
            }
            finally
            {
                global::TileStories.MarkerHierarchyResolver.ResetToDefaults();
            }
        }

        // ---- Real OnGUI smoke test: the redesigned table (header + Priority/Details/Size/Text
        // Label/Ripple/Halo/Pulse/Spin Ring/Reveal/Facing/Search Keywords/Remove columns) must
        // draw without throwing, in every branch the section's own code conditionally hides a
        // column (Outline enabled/disabled -> Spin Ring shown/hidden; Pulse effect enabled/
        // disabled -> Pulse checkbox enabled/disabled), across an empty and a populated table.
        private sealed class HierarchyTableHarness : EditorWindow
        {
            public static bool DidDraw;
            public static System.Exception Thrown;
            public static bool OutlineOn;
            public static bool PulseEffectOn;
            public static int LevelCount;

            private void OnGUI()
            {
                DidDraw = true;
                Thrown = null;
                var window = CreateInstance<POIEditorToolWindow>();
                try
                {
                    var config = new global::TileStories.WallConfigData
                    {
                        pois = new List<global::TileStories.POIData>(),
                        marker_outline_mode = OutlineOn ? "uniform" : "none",
                        effect_defaults = new global::TileStories.EffectDefaults(),
                        hierarchy_levels = new List<global::TileStories.HierarchyLevelEntry>()
                    };
                    config.effect_defaults.effects_enabled = true;
                    config.effect_defaults.pulse.enabled = PulseEffectOn;
                    for (int i = 0; i < LevelCount; i++)
                        config.hierarchy_levels.Add(new global::TileStories.HierarchyLevelEntry
                        {
                            key = "level_" + i, level_name = "L" + i, priority = i + 1, size_cm = 10f + i,
                            show_label = i == 0, ripple_effect = "none", halo_effect = "none",
                            reveal_duration_s = 0.35f, search_keywords = new List<string>()
                        });

                    typeof(POIEditorToolWindow).GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance)
                        .SetValue(window, config);

                    var method = typeof(POIEditorToolWindow).GetMethod("DrawGlobalHierarchySection", BindingFlags.NonPublic | BindingFlags.Instance);
                    Assert.IsNotNull(method, "DrawGlobalHierarchySection must exist");
                    try
                    {
                        method.Invoke(window, null);
                    }
                    catch (TargetInvocationException ex)
                    {
                        Thrown = ex.InnerException ?? ex;
                    }
                }
                finally
                {
                    Object.DestroyImmediate(window);
                }
            }
        }

        private static IEnumerator DrawTableOnce(bool outlineOn, bool pulseOn, int levelCount)
        {
            HierarchyTableHarness.OutlineOn = outlineOn;
            HierarchyTableHarness.PulseEffectOn = pulseOn;
            HierarchyTableHarness.LevelCount = levelCount;
            HierarchyTableHarness.DidDraw = false;
            HierarchyTableHarness.Thrown = null;

            var window = EditorWindow.GetWindow<HierarchyTableHarness>(true, "HierarchyTableHarness");
            window.minSize = new Vector2(200f, 120f);
            window.position = new Rect(0f, 0f, 1400f, 400f);
            window.Focus();
            window.Repaint();

            for (int i = 0; i < 60 && !HierarchyTableHarness.DidDraw; i++)
                yield return null;

            window.Close();
        }

        [UnityTest]
        public IEnumerator HierarchyTable_DrawsWithoutThrowing_EmptyTable_OutlineOff_PulseOff()
        {
            yield return DrawTableOnce(outlineOn: false, pulseOn: false, levelCount: 0);
            Assert.IsTrue(HierarchyTableHarness.DidDraw, "Harness OnGUI must have run");
            Assert.IsNull(HierarchyTableHarness.Thrown, "DrawGlobalHierarchySection must not throw: " + HierarchyTableHarness.Thrown);
        }

        [UnityTest]
        public IEnumerator HierarchyTable_DrawsWithoutThrowing_PopulatedTable_OutlineOn_PulseOn()
        {
            yield return DrawTableOnce(outlineOn: true, pulseOn: true, levelCount: 3);
            Assert.IsTrue(HierarchyTableHarness.DidDraw, "Harness OnGUI must have run");
            Assert.IsNull(HierarchyTableHarness.Thrown, "DrawGlobalHierarchySection must not throw: " + HierarchyTableHarness.Thrown);
        }

        [UnityTest]
        public IEnumerator HierarchyTable_DrawsWithoutThrowing_PopulatedTable_OutlineOff_PulseOn()
        {
            yield return DrawTableOnce(outlineOn: false, pulseOn: true, levelCount: 2);
            Assert.IsTrue(HierarchyTableHarness.DidDraw, "Harness OnGUI must have run");
            Assert.IsNull(HierarchyTableHarness.Thrown, "DrawGlobalHierarchySection must not throw: " + HierarchyTableHarness.Thrown);
        }

        [UnityTest]
        public IEnumerator HierarchyTable_DrawsWithoutThrowing_PopulatedTable_OutlineOn_PulseOff()
        {
            yield return DrawTableOnce(outlineOn: true, pulseOn: false, levelCount: 1);
            Assert.IsTrue(HierarchyTableHarness.DidDraw, "Harness OnGUI must have run");
            Assert.IsNull(HierarchyTableHarness.Thrown, "DrawGlobalHierarchySection must not throw: " + HierarchyTableHarness.Thrown);
        }

        private static void AssertAscii(string text, string what)
        {
            foreach (char c in text)
                Assert.IsTrue(c == '\n' || (c >= ' ' && c <= '~'), what + " must be ASCII only, found char code " + (int)c);
        }
    }
}
