using NUnit.Framework;
using UnityEngine;
using TileStories;

namespace TileStories.Tests
{
    // Tier-0 EditMode tests for DisplacementTieBreakStrategy (spec _2.5 Section 7, correction 2.5-g).
    // Pure decision logic: no scene, no camera, no MonoBehaviour.
    public class DisplacementTieBreakStrategyTests
    {
        [Test]
        public void UniqueMin_ReturnsAnchorIndex()
        {
            bool found = DisplacementTieBreakStrategy.TryGetPrimaryAnchorLocalIndex(
                "lower_priority_only", new[] { 3, 1, 2 }, out int anchor);
            Assert.IsTrue(found);
            Assert.AreEqual(1, anchor);
        }

        [Test]
        public void UniqueMinAtFirstPosition_ReturnsZero()
        {
            bool found = DisplacementTieBreakStrategy.TryGetPrimaryAnchorLocalIndex(
                "lower_priority_only", new[] { 1, 2, 3 }, out int anchor);
            Assert.IsTrue(found);
            Assert.AreEqual(0, anchor);
        }

        [Test]
        public void SharedMin_FallsBackToSymmetric_ReturnsFalse()
        {
            bool found = DisplacementTieBreakStrategy.TryGetPrimaryAnchorLocalIndex(
                "lower_priority_only", new[] { 1, 1, 2 }, out int anchor);
            Assert.IsFalse(found);
            Assert.AreEqual(-1, anchor);
        }

        [Test]
        public void AllEqual_FallsBackToSymmetric_ReturnsFalse()
        {
            bool found = DisplacementTieBreakStrategy.TryGetPrimaryAnchorLocalIndex(
                "lower_priority_only", new[] { 2, 2 }, out int anchor);
            Assert.IsFalse(found);
            Assert.AreEqual(-1, anchor);
        }

        [Test]
        public void SymmetricMode_ReturnsFalse()
        {
            bool found = DisplacementTieBreakStrategy.TryGetPrimaryAnchorLocalIndex(
                "symmetric", new[] { 1, 2 }, out int anchor);
            Assert.IsFalse(found);
            Assert.AreEqual(-1, anchor);
        }

        [Test]
        public void UnknownMode_ReturnsFalse()
        {
            bool found = DisplacementTieBreakStrategy.TryGetPrimaryAnchorLocalIndex(
                "bogus_mode", new[] { 1, 2 }, out int anchor);
            Assert.IsFalse(found);
            Assert.AreEqual(-1, anchor);
        }

        [Test]
        public void NullMode_ReturnsFalse()
        {
            bool found = DisplacementTieBreakStrategy.TryGetPrimaryAnchorLocalIndex(
                null, new[] { 1, 2 }, out int anchor);
            Assert.IsFalse(found);
            Assert.AreEqual(-1, anchor);
        }

        [Test]
        public void NullList_ReturnsFalse()
        {
            bool found = DisplacementTieBreakStrategy.TryGetPrimaryAnchorLocalIndex(
                "lower_priority_only", null, out int anchor);
            Assert.IsFalse(found);
            Assert.AreEqual(-1, anchor);
        }

        [Test]
        public void EmptyList_ReturnsFalse()
        {
            bool found = DisplacementTieBreakStrategy.TryGetPrimaryAnchorLocalIndex(
                "lower_priority_only", new int[0], out int anchor);
            Assert.IsFalse(found);
            Assert.AreEqual(-1, anchor);
        }
    }
}
