using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // Tier 0 tests for MarkerHierarchyResolver -- pure static-class logic, no
    // scene, no MonoBehaviour. Instantiated via static calls only.
    public class MarkerHierarchyResolverTests
    {
        private List<HierarchyLevelEntry> _testLevels;

        [SetUp]
        public void SetUp()
        {
            MarkerHierarchyResolver.ResetToDefaults();
            _testLevels = new List<HierarchyLevelEntry>
            {
                new HierarchyLevelEntry
                {
                                        key = "level_1", label = "1", size_cm = 20f, show_label = true,
                    sun_effect = "sun_circles", accent_effect = "ring_pulse",
                    pulse = true, rotate_contour = true, reveal_delay_s = 0f,
                    reveal_duration_s = 0.5f
                },
                new HierarchyLevelEntry
                {
                                        key = "level_2", label = "2", size_cm = 15f, show_label = false,
                    sun_effect = "sun_contours", accent_effect = "none",
                    pulse = true, rotate_contour = true, reveal_delay_s = 0.15f,
                    reveal_duration_s = 0.4f
                },
                new HierarchyLevelEntry
                {
                                        key = "level_3", label = "3", size_cm = 10f, show_label = false,
                    sun_effect = "none", accent_effect = "simple_sun",
                    pulse = true, rotate_contour = true, reveal_delay_s = 0.3f,
                    reveal_duration_s = 0.35f
                },
                new HierarchyLevelEntry
                {
                                        key = "level_4", label = "4", size_cm = 5f, show_label = false,
                    sun_effect = "none", accent_effect = "beacon",
                    pulse = true, rotate_contour = true, reveal_delay_s = 0.45f,
                    reveal_duration_s = 0.3f
                },
                new HierarchyLevelEntry
                {
                                        key = "level_5", label = "5", size_cm = 2f, show_label = false,
                    sun_effect = "none", accent_effect = "none",
                    pulse = true, rotate_contour = true, reveal_delay_s = 0.6f,
                    reveal_duration_s = 0.25f
                },
            };
        }

        [TearDown]
        public void TearDown()
        {
            MarkerHierarchyResolver.ResetToDefaults();
        }

        // Verify each configured level resolves to the exact expected HierarchyStyle
        // fields. Uses the SAME source data (§13 table) the production config will use.
        [Test]
        public void Configure_WithFiveLevels_ResolvesEachKeyExactly()
        {
            MarkerHierarchyResolver.Configure(_testLevels);

            Assert.IsTrue(MarkerHierarchyResolver.TryResolveByKey("level_1", out var s1), "level_1 should resolve");
            Assert.AreEqual(20f, s1.SizeCm, "level_1 size");
            Assert.IsTrue(s1.ShowLabel, "level_1 show_label");
            Assert.AreEqual(MarkerEffectFlags.SunCircles | MarkerEffectFlags.RingPulse | MarkerEffectFlags.Pulse,
                s1.EffectFlags, "level_1 effects");
            Assert.IsTrue(s1.RotateContour, "level_1 rotate");
                        Assert.AreEqual(0f, s1.RevealDelaySeconds, "level_1 delay");
            Assert.AreEqual(0.5f, s1.RevealDurationSeconds, "level_1 duration");

            Assert.IsTrue(MarkerHierarchyResolver.TryResolveByKey("level_2", out var s2), "level_2 should resolve");
            Assert.AreEqual(15f, s2.SizeCm, "level_2 size");
            Assert.IsFalse(s2.ShowLabel, "level_2 show_label");
            Assert.AreEqual(MarkerEffectFlags.SunContours | MarkerEffectFlags.Pulse,
                s2.EffectFlags, "level_2 effects");
            Assert.IsTrue(s2.RotateContour, "level_2 rotate");
                        Assert.AreEqual(0.15f, s2.RevealDelaySeconds, "level_2 delay");
            Assert.AreEqual(0.4f, s2.RevealDurationSeconds, "level_2 duration");

            Assert.IsTrue(MarkerHierarchyResolver.TryResolveByKey("level_3", out var s3), "level_3 should resolve");
            Assert.AreEqual(10f, s3.SizeCm, "level_3 size");
            Assert.AreEqual(MarkerEffectFlags.SimpleSun | MarkerEffectFlags.Pulse,
                s3.EffectFlags, "level_3 effects");
                        Assert.AreEqual(0.3f, s3.RevealDelaySeconds, "level_3 delay");
            Assert.AreEqual(0.35f, s3.RevealDurationSeconds, "level_3 duration");

            Assert.IsTrue(MarkerHierarchyResolver.TryResolveByKey("level_4", out var s4), "level_4 should resolve");
            Assert.AreEqual(5f, s4.SizeCm, "level_4 size");
            Assert.AreEqual(MarkerEffectFlags.Beacon | MarkerEffectFlags.Pulse,
                s4.EffectFlags, "level_4 effects");
                        Assert.AreEqual(0.45f, s4.RevealDelaySeconds, "level_4 delay");
            Assert.AreEqual(0.3f, s4.RevealDurationSeconds, "level_4 duration");

            Assert.IsTrue(MarkerHierarchyResolver.TryResolveByKey("level_5", out var s5), "level_5 should resolve");
            Assert.AreEqual(2f, s5.SizeCm, "level_5 size");
            Assert.AreEqual(MarkerEffectFlags.Pulse, s5.EffectFlags, "level_5 should only have Pulse");
                        Assert.AreEqual(0.6f, s5.RevealDelaySeconds, "level_5 delay");
            Assert.AreEqual(0.25f, s5.RevealDurationSeconds, "level_5 duration");
        }

        // Empty Configure() must leave TryResolveByKey returning false and falling
        // back to Fallback values -- the safe empty-state for walls with no hierarchy.
        [Test]
        public void Configure_WithEmptyList_FallsThroughToFallback()
        {
            MarkerHierarchyResolver.ResetToDefaults();
            MarkerHierarchyResolver.Configure(new List<HierarchyLevelEntry>());

            Assert.IsFalse(MarkerHierarchyResolver.TryResolveByKey("level_1", out var style));
            Assert.AreEqual(MarkerHierarchyResolver.Fallback.SizeCm, style.SizeCm);
            Assert.AreEqual(MarkerHierarchyResolver.Fallback.ShowLabel, style.ShowLabel);
            Assert.AreEqual(MarkerHierarchyResolver.Fallback.EffectFlags, style.EffectFlags);
            Assert.AreEqual(MarkerHierarchyResolver.Fallback.RotateContour, style.RotateContour);
                        Assert.AreEqual(MarkerHierarchyResolver.Fallback.RevealDelaySeconds, style.RevealDelaySeconds);
            Assert.AreEqual(MarkerHierarchyResolver.Fallback.RevealDurationSeconds, style.RevealDurationSeconds);
        }

        [Test]
        public void Configure_WithNull_ReturnsFalseAndFallbackValues()
        {
            MarkerHierarchyResolver.ResetToDefaults();
            MarkerHierarchyResolver.Configure(null);

            Assert.IsFalse(MarkerHierarchyResolver.TryResolveByKey("anything", out var style));
            Assert.AreEqual(12f, style.SizeCm);
            Assert.AreEqual(MarkerEffectFlags.None, style.EffectFlags);
        }

        [Test]
        public void Configure_WithUnknownKey_ReturnsFalse()
        {
            MarkerHierarchyResolver.Configure(_testLevels);

            Assert.IsFalse(MarkerHierarchyResolver.TryResolveByKey("not_a_real_level", out _));
            Assert.IsFalse(MarkerHierarchyResolver.TryResolveByKey("", out _));
            Assert.IsFalse(MarkerHierarchyResolver.TryResolveByKey(null, out _));
        }

        // ResetToDefaults must clear all previously configured state so the resolver
        // returns to a clean empty state (critical for test isolation).
        [Test]
        public void ResetToDefaults_AfterConfigure_ClearsAllKeys()
        {
            MarkerHierarchyResolver.Configure(_testLevels);
            Assert.IsTrue(MarkerHierarchyResolver.TryResolveByKey("level_1", out _));

            MarkerHierarchyResolver.ResetToDefaults();
            Assert.IsFalse(MarkerHierarchyResolver.TryResolveByKey("level_1", out _));
        }

        // The Fallback must have the exact contract documented: 12cm, no label,
        // no effects, no rotation, 0 delay. This is relied on by MarkerView when
        // a POI has no hierarchy_level_key set.
        [Test]
        public void Fallback_HasCorrectContractValues()
        {
            Assert.AreEqual(12f, MarkerHierarchyResolver.Fallback.SizeCm);
            Assert.IsFalse(MarkerHierarchyResolver.Fallback.ShowLabel);
            Assert.AreEqual(MarkerEffectFlags.None, MarkerHierarchyResolver.Fallback.EffectFlags);
            Assert.IsFalse(MarkerHierarchyResolver.Fallback.RotateContour);
                        Assert.AreEqual(0f, MarkerHierarchyResolver.Fallback.RevealDelaySeconds);
            Assert.AreEqual(0.35f, MarkerHierarchyResolver.Fallback.RevealDurationSeconds);
        }

        // priority is a pure sort key -- authored >= 1 wins over the 1-based row position.
        [Test]
        public void GetLevelPriority_ExplicitPriority_WinsOverPositional()
        {
            var entries = new List<HierarchyLevelEntry>
            {
                new HierarchyLevelEntry { key = "level_1", priority = 5 },
                new HierarchyLevelEntry { key = "level_2", priority = 1 },
                new HierarchyLevelEntry { key = "level_3", priority = 3 },
            };
            MarkerHierarchyResolver.Configure(entries);

            Assert.AreEqual(5, MarkerHierarchyResolver.GetLevelPriority("level_1"));
            Assert.AreEqual(1, MarkerHierarchyResolver.GetLevelPriority("level_2"));
            Assert.AreEqual(3, MarkerHierarchyResolver.GetLevelPriority("level_3"));
        }

        // unset (<= 0 / absent) falls back to the 1-based row index: row 1 -> priority 1.
        [Test]
        public void GetLevelPriority_UnsetPriority_FallsBackTo1BasedRow()
        {
            var entries = new List<HierarchyLevelEntry>
            {
                new HierarchyLevelEntry { key = "level_1" },
                new HierarchyLevelEntry { key = "level_2" },
                new HierarchyLevelEntry { key = "level_3" },
                new HierarchyLevelEntry { key = "level_4" },
                new HierarchyLevelEntry { key = "level_5" },
            };
            MarkerHierarchyResolver.Configure(entries);

            Assert.AreEqual(1, MarkerHierarchyResolver.GetLevelPriority("level_1"));
            Assert.AreEqual(2, MarkerHierarchyResolver.GetLevelPriority("level_2"));
            Assert.AreEqual(3, MarkerHierarchyResolver.GetLevelPriority("level_3"));
            Assert.AreEqual(4, MarkerHierarchyResolver.GetLevelPriority("level_4"));
            Assert.AreEqual(5, MarkerHierarchyResolver.GetLevelPriority("level_5"));
        }

        // unknown / blank / null keys have no ranking -> sort to the bottom (MaxValue).
        [Test]
        public void GetLevelPriority_UnknownKey_ReturnsMaxValue()
        {
            MarkerHierarchyResolver.Configure(_testLevels);

            Assert.AreEqual(int.MaxValue, MarkerHierarchyResolver.GetLevelPriority("not_a_real_level"));
            Assert.AreEqual(int.MaxValue, MarkerHierarchyResolver.GetLevelPriority(""));
            Assert.AreEqual(int.MaxValue, MarkerHierarchyResolver.GetLevelPriority(null));
            Assert.IsFalse(MarkerHierarchyResolver.TryResolvePriority("not_a_real_level", out _));
        }

        // duplicate priorities are legal -- a priority is a tie-break bucket, not an identity key.
        [Test]
        public void GetLevelPriority_DuplicatePrioritiesAccepted()
        {
            var entries = new List<HierarchyLevelEntry>
            {
                new HierarchyLevelEntry { key = "level_1", priority = 1 },
                new HierarchyLevelEntry { key = "level_2", priority = 1 },
            };
            Assert.DoesNotThrow(() => MarkerHierarchyResolver.Configure(entries));
            Assert.AreEqual(1, MarkerHierarchyResolver.GetLevelPriority("level_1"));
            Assert.AreEqual(1, MarkerHierarchyResolver.GetLevelPriority("level_2"));
        }

        // empty / null table -> every key returns MaxValue (lowest possible sort rank).
        [Test]
        public void GetLevelPriority_EmptyAndNullTable_ReturnsMaxValue()
        {
            MarkerHierarchyResolver.Configure(new List<HierarchyLevelEntry>());
            Assert.AreEqual(int.MaxValue, MarkerHierarchyResolver.GetLevelPriority("level_1"));
            Assert.IsFalse(MarkerHierarchyResolver.TryResolvePriority("level_1", out _));

            MarkerHierarchyResolver.ResetToDefaults();
            MarkerHierarchyResolver.Configure(null);
            Assert.AreEqual(int.MaxValue, MarkerHierarchyResolver.GetLevelPriority("level_1"));
            Assert.IsFalse(MarkerHierarchyResolver.TryResolvePriority("level_1", out _));
        }
    }
}
