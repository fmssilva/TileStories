// DefaultOutlineLevelsTests.cs
//
// EditMode test for the outline-level defaults seeded by the authoring tool
// when a wall has no outline_levels. Pure data -- no scene, no window, no
// Unity dependencies. Verifies the four heritage levels, their line styles,
// pct/ring_width values, and that "unknown" carries an explicit grey colour.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    public class DefaultOutlineLevelsTests
    {
        [Test]
        public void Create_ReturnsFourEntries()
        {
            List<OutlineLevelEntry> defaults = DefaultOutlineLevels.Create();
            Assert.AreEqual(4, defaults.Count,
                "Should seed exactly four outline level defaults.");
        }

        [Test]
        public void Create_EveryEntry_HasRequiredFields()
        {
            List<OutlineLevelEntry> defaults = DefaultOutlineLevels.Create();

            foreach (var entry in defaults)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.key),
                    "Every default entry must have a key.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.label),
                    "Every default entry must have a label.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.line_style),
                    "Every default entry must have a line_style.");
            }
        }

        [Test]
        public void Create_Levels_MatchSpec()
        {
            // The seeded defaults must match the four heritage outline types
            // defined in _5.1.2_Default_Icons.md section 2: intact (0%),
            // partial_damage (20%), destroyed (100%), unknown (100% with grey).
            List<OutlineLevelEntry> defaults = DefaultOutlineLevels.Create();
            Assert.AreEqual(4, defaults.Count);

            // intact
            Assert.AreEqual("intact", defaults[0].key);
            Assert.AreEqual("Intact", defaults[0].label);
            Assert.AreEqual(0f, defaults[0].pct);
            Assert.AreEqual("solid", defaults[0].line_style);
            Assert.AreEqual(3.2f, defaults[0].ring_width, 0.001f);
            Assert.IsTrue(string.IsNullOrEmpty(defaults[0].color_hex),
                "intact must have empty color_hex so runtime uses StatusRamp gold.");

            // partial_damage
            Assert.AreEqual("partial_damage", defaults[1].key);
            Assert.AreEqual("Partial Damage", defaults[1].label);
            Assert.AreEqual(20f, defaults[1].pct);
            Assert.AreEqual("dash_long", defaults[1].line_style);
            Assert.AreEqual(2.8f, defaults[1].ring_width, 0.001f);
            Assert.IsTrue(string.IsNullOrEmpty(defaults[1].color_hex),
                "partial_damage must have empty color_hex so runtime uses StatusRamp gold.");

            // destroyed
            Assert.AreEqual("destroyed", defaults[2].key);
            Assert.AreEqual("Destroyed", defaults[2].label);
            Assert.AreEqual(100f, defaults[2].pct);
            Assert.AreEqual("dash_short", defaults[2].line_style);
            Assert.AreEqual(2.0f, defaults[2].ring_width, 0.001f);
            Assert.IsTrue(string.IsNullOrEmpty(defaults[2].color_hex),
                "destroyed must have empty color_hex so runtime uses StatusRamp gold.");

            // unknown
            Assert.AreEqual("unknown", defaults[3].key);
            Assert.AreEqual("Unknown", defaults[3].label);
            Assert.AreEqual(100f, defaults[3].pct);
            Assert.AreEqual("dotted", defaults[3].line_style);
            Assert.AreEqual(1.8f, defaults[3].ring_width, 0.001f);
            Assert.AreEqual("#71717A", defaults[3].color_hex,
                "unknown must carry an explicit grey colour, not fall back to StatusRamp.");
        }

        [Test]
        public void Create_ReturnsFreshList_EachCall()
        {
            // Each call must return a new list so callers can mutate without
            // leaking state into the next caller.
            var first = DefaultOutlineLevels.Create();
            var second = DefaultOutlineLevels.Create();

            Assert.AreNotSame(first, second, "Create() must return a fresh list each call.");
            Assert.AreEqual(first.Count, second.Count);
        }

        [Test]
        public void Create_KeysAreUnique()
        {
            List<OutlineLevelEntry> defaults = DefaultOutlineLevels.Create();
            var keys = defaults.Select(e => e.key).ToList();
            var uniqueKeys = new HashSet<string>(keys);

            Assert.AreEqual(keys.Count, uniqueKeys.Count,
                "All default outline level keys must be unique.");
        }
    }
}
