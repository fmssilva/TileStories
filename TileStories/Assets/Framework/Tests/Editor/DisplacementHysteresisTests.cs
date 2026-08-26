using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TileStories;

namespace TileStories.Tests
{
    // Tier-0 EditMode tests for MarkerOverlapResolver.CommitGroupMembership (section 9).
    // Pure static: fabricate a stability dict + per-cycle observations, no scene/camera.
    // Mirrors the contract of LODController.CommitDensityState (2-cycle commit gate).
    public class DisplacementHysteresisTests
    {
        private static Dictionary<string, DisplacementStabilityState> NewDict()
            => new Dictionary<string, DisplacementStabilityState>();

        // 1. A single in-group observation is provisional -> must not commit.
        [Test]
        public void SingleAgreeingCycle_DoesNotCommit()
        {
            var s = NewDict();
            bool c1 = MarkerOverlapResolver.CommitGroupMembership("p0", true, s);
            Assert.IsFalse(c1, "Cycle 1 in-group must be provisional, not committed.");
            Assert.IsFalse(s["p0"].committed, "committed must stay false after one cycle.");
        }

        // 2. Two consecutive in-group observations commit.
        [Test]
        public void TwoAgreeingCycles_CommitIn()
        {
            var s = NewDict();
            MarkerOverlapResolver.CommitGroupMembership("p0", true, s); // provisional
            bool c2 = MarkerOverlapResolver.CommitGroupMembership("p0", true, s);
            Assert.IsTrue(c2, "Second consecutive in-group cycle must commit.");
            Assert.IsTrue(s["p0"].committed, "committed must be true after two agreeing cycles.");
        }

        // 3. A contrary out-group cycle cancels a pending (provisional) entry.
        [Test]
        public void ContraryCycle_CancelsPending()
        {
            var s = NewDict();
            MarkerOverlapResolver.CommitGroupMembership("p0", true, s);  // provisional in
            bool c = MarkerOverlapResolver.CommitGroupMembership("p0", false, s);
            Assert.IsFalse(c, "One contrary cycle must cancel pending and keep committed false.");
            Assert.IsFalse(s["p0"].committed, "committed must stay false after pending cancellation.");
            Assert.IsFalse(s["p0"].pending, "pending must be cleared, not left mid-transition.");
            Assert.AreEqual(0, s["p0"].pendingCycles);
        }

        // 4. A committed-in member HOLDS displacement through one out-group cycle.
        [Test]
        public void CommittedMember_HoldsOnOneOutCycle()
        {
            var s = NewDict();
            MarkerOverlapResolver.CommitGroupMembership("p0", true, s);  // provisional
            MarkerOverlapResolver.CommitGroupMembership("p0", true, s);  // commit in
            bool held = MarkerOverlapResolver.CommitGroupMembership("p0", false, s); // one out
            Assert.IsTrue(held, "One contrary out-cycle must NOT clear a committed displacement.");
            Assert.IsTrue(s["p0"].committed, "committed must remain true (hold).");
        }

        // 5. A committed-in member clears only after a SECOND consecutive out-group cycle.
        [Test]
        public void SecondOutCycle_CommitsOut()
        {
            var s = NewDict();
            MarkerOverlapResolver.CommitGroupMembership("p0", true, s);  // provisional
            MarkerOverlapResolver.CommitGroupMembership("p0", true, s);  // commit in
            MarkerOverlapResolver.CommitGroupMembership("p0", false, s); // out #1 -> hold
            Assert.IsTrue(s["p0"].committed, "First out-cycle must hold (still committed).");
            bool cleared = MarkerOverlapResolver.CommitGroupMembership("p0", false, s); // out #2
            Assert.IsFalse(cleared, "Second consecutive out-cycle must commit out.");
            Assert.IsFalse(s["p0"].committed, "committed must be false after two consecutive out-cycles.");
        }

        // 6. A fresh unseen marker starts not displaced.
        [Test]
        public void FreshMarker_StartsNotDisplaced()
        {
            var s = NewDict();
            bool first = MarkerOverlapResolver.CommitGroupMembership("p0", true, s);
            Assert.IsFalse(first, "First observation of a fresh marker must be committed=false.");
            Assert.IsFalse(s["p0"].committed);
        }

        // 7. Per-poi isolation: observing one id does not affect another.
        [Test]
        public void PerPoiIsolation()
        {
            var s = NewDict();
            // p0 commits in over two cycles...
            MarkerOverlapResolver.CommitGroupMembership("p0", true, s);
            bool c_p0 = MarkerOverlapResolver.CommitGroupMembership("p0", true, s);
            Assert.IsTrue(c_p0, "p0 must commit after two in-group cycles.");
            // ...while a brand-new p1 observing the same is still provisional.
            bool c_p1 = MarkerOverlapResolver.CommitGroupMembership("p1", true, s);
            Assert.IsFalse(c_p1, "p1 (first observation) must be provisional, independent of p0.");
            Assert.IsFalse(s["p1"].committed, "p1 committed flag must be independent of p0.");
        }

        // 8. Null / blank poiId must not throw (the resolver normalizes to string.Empty).
        [Test]
        public void NullOrBlankId_NoThrow()
        {
            var s = NewDict();
            Assert.DoesNotThrow(() =>
            {
                MarkerOverlapResolver.CommitGroupMembership(null, true, s);
                MarkerOverlapResolver.CommitGroupMembership("", true, s);
                MarkerOverlapResolver.CommitGroupMembership(null, true, s);
            });
            // null and "" both collapse to the empty key -> a single state slot.
            Assert.AreEqual(1, s.Count, "null and blank must map to one shared key.");
        }
    }
}
