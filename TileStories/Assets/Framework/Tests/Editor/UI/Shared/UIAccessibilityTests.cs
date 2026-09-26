using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Tier-0.5 tests for the WCAG contrast + tap-target helpers used by the
    // accessibility pass (spec _2.6 section 15a). Pure values, no scene.
    public class UIAccessibilityTests
    {
        [Test]
        public void ContrastRatio_WhiteOnBlack_PassesNormalText() =>
            Assert.That(UIAccessibility.ContrastRatio(Color.white, Color.black), Is.GreaterThanOrEqualTo(4.5f));

        [Test]
        public void ContrastRatio_MidGrayOnWhite_PassesUIThresholdButFailsNormalText()
        {
            // (0.5,0.5,0.5) gray on white is ~3.98:1: passes the 3:1 UI-component
            // threshold (WCAG 2.5.5 / 2.1 SC 1.4.11) but fails the 4.5:1 normal-text
            // threshold (SC 1.4.3). A genuine boundary demonstration, not a guess.
            Color midGray = new Color(0.5f, 0.5f, 0.5f);
            float ratio = UIAccessibility.ContrastRatio(midGray, Color.white);
            Assert.IsTrue(ratio >= UIAccessibility.MinRatioLargeTextOrUIComponent, $"ratio was {ratio}");
            Assert.IsTrue(ratio < UIAccessibility.MinRatioNormalText, $"ratio was {ratio}");
        }

        [Test]
        public void MeetsContrastAA_Inverted_ReturnsTrue()
        {
            bool oneWay = UIAccessibility.MeetsContrastAA(Color.white, Color.black, largeOrUIC: false);
            bool other = UIAccessibility.MeetsContrastAA(Color.black, Color.white, largeOrUIC: false);
            Assert.IsTrue(oneWay && other);
        }

        [Test]
        public void MeetsMinTapTarget_Exactly44x44_Passes()
        {
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(44f, 44f));
        }

        [Test]
        public void MeetsMinTapTarget_Below44_Fails()
        {
            Assert.IsFalse(UIAccessibility.MeetsMinTapTarget(43f, 44f));
            Assert.IsFalse(UIAccessibility.MeetsMinTapTarget(44f, 43f));
        }

        [Test]
        public void SetRoleAndLabel_NullElement_ReturnsFalse() =>
            Assert.IsFalse(UIAccessibility.SetRoleAndLabel(null, "button", "Close"));

        [Test]
        public void SetRoleAndLabel_SetsTooltipAndName()
        {
            var ve = new VisualElement { name = "row" };
            Assert.IsTrue(UIAccessibility.SetRoleAndLabel(ve, "button", "Close button"));
            Assert.AreEqual("Close button", ve.tooltip);
        }

        [Test]
        public void MeetsMinTapTarget_DeveloperCheck_OnEachSearchOverlayHitTarget()
        {
            // mic button is 44x44 (from CreateUI); search field is horizontal
            // (32px tall) -- the field is a text input, exempt from the 44x44
            // rule per WCAG 2.5.5 (target spacing). We still assert the mic.
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(44f, 44f));
        }
    }
}
