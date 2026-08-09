using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace TileStories.Tests
{
    public class MarkerLayoutTests
    {
        // Helper: creates a minimal RectTransform for testing
        private static RectTransform MakeRect(string name, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = size;
            return rt;
        }

        [Test]
        public void Apply_RingSizeRatio_ScalesRelativeToSymbol()
        {
            var symbol = MakeRect("Symbol", new Vector2(0.12f, 0.12f));
            var ring = MakeRect("Ring", Vector2.zero);
            var proportions = new MarkerLayoutProportions { ringSizeRatio = 1.1f };

            MarkerLayout.Apply(symbol, ring, null, null, proportions);

            Assert.AreEqual(0.132f, ring.sizeDelta.x, 0.0001f);
        }

        [Test]
        public void Apply_BadgeSizeAndPosition_DerivesFromSymbolRadius()
        {
            var symbol = MakeRect("Symbol", new Vector2(0.12f, 0.12f));
            var badge = MakeRect("Badge", Vector2.zero);
            var proportions = new MarkerLayoutProportions { badgeSizeRatio = 0.36f, badgeDirection = new Vector2(0.7f, 0.7f) };

            MarkerLayout.Apply(symbol, null, badge, null, proportions);

            Assert.AreEqual(0.12f * 0.36f, badge.sizeDelta.x, 0.0001f);
            float expectedRadius = 0.06f;
            Assert.AreEqual(0.7f * expectedRadius, badge.anchoredPosition.x, 0.0001f);
            Assert.AreEqual(0.7f * expectedRadius, badge.anchoredPosition.y, 0.0001f);
        }

        [Test]
        public void Apply_DoublingSymbolSize_DoublesBadgeOffset()
        {
            var proportions = new MarkerLayoutProportions { badgeSizeRatio = 0.36f, badgeDirection = new Vector2(0.7f, 0.7f) };

            var smallSymbol = MakeRect("Small", new Vector2(0.12f, 0.12f));
            var smallBadge = MakeRect("SmallBadge", Vector2.zero);
            MarkerLayout.Apply(smallSymbol, null, smallBadge, null, proportions);

            var bigSymbol = MakeRect("Big", new Vector2(0.24f, 0.24f));
            var bigBadge = MakeRect("BigBadge", Vector2.zero);
            MarkerLayout.Apply(bigSymbol, null, bigBadge, null, proportions);

            Assert.AreEqual(smallBadge.anchoredPosition.x * 2f, bigBadge.anchoredPosition.x, 0.0001f);
        }

        [Test]
        public void Apply_Label_SitsBelowSymbolByRadiusPlusGap()
        {
            var symbol = MakeRect("Symbol", new Vector2(0.12f, 0.12f));
            var label = MakeRect("Label", Vector2.zero);
            var proportions = new MarkerLayoutProportions { labelGap = 0.02f };

            MarkerLayout.Apply(symbol, null, null, label, proportions);

            Assert.AreEqual(-0.08f, label.anchoredPosition.y, 0.0001f);
        }

                [Test]
        public void Apply_NullSymbol_DoesNotThrow()
        {
            var ring = MakeRect("Ring", Vector2.zero);
            Assert.DoesNotThrow(() => MarkerLayout.Apply(null, ring, null, null, new MarkerLayoutProportions()));
        }

        // Hierarchy-level cm-to-meter sizes must produce correctly proportioned
        // RectTransforms. Tests the ratio invariant: ring = 1.18x symbol,
        // badge = 0.36x symbol, at Level 1 (30cm = 0.30m) and Level 5 (7cm = 0.07m).
        [Test]
        public void Apply_HierarchyLevel1Size_ProportionCorrect()
        {
            // 30cm = 0.30m, converted to metres at the /100 call site in MarkerView
            var symbol = MakeRect("Symbol", new Vector2(0.30f, 0.30f));
            var ring = MakeRect("Ring", Vector2.zero);
            var badge = MakeRect("Badge", Vector2.zero);
            var label = MakeRect("Label", Vector2.zero);
            var proportions = new MarkerLayoutProportions();

            MarkerLayout.Apply(symbol, ring, badge, label, proportions);

            Assert.AreEqual(0.30f * 1.18f, ring.sizeDelta.x, 0.0001f, "ring should be 1.18x symbol at Level 1");
            Assert.AreEqual(0.30f * 0.36f, badge.sizeDelta.x, 0.0001f, "badge should be 0.36x symbol at Level 1");
        }

        [Test]
        public void Apply_HierarchyLevel5Size_ProportionCorrect()
        {
            // 7cm = 0.07m
            var symbol = MakeRect("Symbol", new Vector2(0.07f, 0.07f));
            var ring = MakeRect("Ring", Vector2.zero);
            var badge = MakeRect("Badge", Vector2.zero);
            var label = MakeRect("Label", Vector2.zero);
            var proportions = new MarkerLayoutProportions();

            MarkerLayout.Apply(symbol, ring, badge, label, proportions);

            Assert.AreEqual(0.07f * 1.18f, ring.sizeDelta.x, 0.0001f, "ring should be 1.18x symbol at Level 5");
            Assert.AreEqual(0.07f * 0.36f, badge.sizeDelta.x, 0.0001f, "badge should be 0.36x symbol at Level 5");
        }

        // Doubling the symbol size must double every derived RectTransform size
        // (ring, badge, label offset). This is the ratio invariant that hierarchy
        // levels rely on -- a 2x larger marker has a 2x larger ring, badge, and label offset.
        [Test]
        public void Apply_DoublingSymbolSize_DoublesAllDerivedSizes()
        {
            var proportions = new MarkerLayoutProportions();

            var smallSymbol = MakeRect("Small", new Vector2(0.10f, 0.10f));
            var smallRing = MakeRect("SmallRing", Vector2.zero);
            var smallBadge = MakeRect("SmallBadge", Vector2.zero);
            var smallLabel = MakeRect("SmallLabel", Vector2.zero);
            MarkerLayout.Apply(smallSymbol, smallRing, smallBadge, smallLabel, proportions);

            var bigSymbol = MakeRect("Big", new Vector2(0.20f, 0.20f));
            var bigRing = MakeRect("BigRing", Vector2.zero);
            var bigBadge = MakeRect("BigBadge", Vector2.zero);
            var bigLabel = MakeRect("BigLabel", Vector2.zero);
            MarkerLayout.Apply(bigSymbol, bigRing, bigBadge, bigLabel, proportions);

                        Assert.AreEqual(smallRing.sizeDelta.x * 2f, bigRing.sizeDelta.x, 0.0001f, "ring must scale with symbol");
            Assert.AreEqual(smallBadge.sizeDelta.x * 2f, bigBadge.sizeDelta.x, 0.0001f, "badge must scale with symbol");
            // Label offset = -symbolRadius - labelGap. The symbol-derived part doubles,
            // but labelGap is fixed -- so the total does not double exactly.
            float smallSymbolRadius = 0.10f * 0.5f;
            float bigSymbolRadius = 0.20f * 0.5f;
            Assert.AreEqual(-smallSymbolRadius - proportions.labelGap, smallLabel.anchoredPosition.y, 0.0001f, "small label offset");
            Assert.AreEqual(-bigSymbolRadius - proportions.labelGap, bigLabel.anchoredPosition.y, 0.0001f, "big label offset");
            Assert.AreEqual(smallSymbolRadius * 2f, bigSymbolRadius, 0.0001f, "symbol-derived label part must double");
        }
    }
}