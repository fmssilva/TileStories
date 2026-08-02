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
    }
}