using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase A of step 16 (_3.2 section 5.7): the full view's breadcrumb and sibling chips over every gallery case, at the run's phone frame
    // (390 x 844), from the gallery's ONE entry list (the Text Tabs row of each case, as a card the gallery draws):
    //   - the block of the largest section opens among its siblings: the breadcrumb is the card's title > the section pill > the block's
    //     words, every part whole (wrapped, never cut) inside the screen, the pill a whole touch target, its words and the block's at 4.5:1
    //   - a chip per sibling, each a whole touch target, in ONE line inside the screen; the current one bold and ringed as well as filled
    //     (never colour alone), the others not; a real tap on another chip shows that sibling
    //   - a section of one (where the case has one) draws no chips
    // Rendered Nav_full_view_<case>.png (and _one)
    public partial class CardNavGalleryTests
    {
        private static string[] FullViewCaseNames => CardNavGalleryDefinitions.All
            .Where(e => !e.IsBlock && e.Layout == null && e.ElementKey == BuiltInNavElements.TextTabs && IsElementCase(e)).Select(e => e.Name).ToArray();

        [UnityTest]
        public IEnumerator EveryCase_TheFullViewsBreadcrumbAndChips_AreWhole_TouchSized_AndTheCurrentIsMarkedByMoreThanColour(
            [ValueSource(nameof(FullViewCaseNames))] string name)
        {
            int index = CardNavGalleryDefinitions.All.ToList().FindIndex(e => e.Name == name);
            var entry = CardNavGalleryDefinitions.All[index];
            _harness.Show(index);
            yield return CardTestInput.Settle();
            var sheet = _harness.Gallery.Sheet;
            sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            var model = sheet.Nav.Model;
            Assert.Greater(model.Sections.Count, 0, "precondition: the case has sections");
            float onePixel = CardGalleryChecks.OnePixel(sheet.Root);
            var takeover = sheet.Takeover;

            var largest = model.Sections.OrderByDescending(s => s.Count).First();
            string key = largest.Targets[largest.Targets.Count > 1 ? 1 : 0];
            var siblings = FullViewSiblingRule.Siblings(model, BlockRegistry.Shared, key).ToList();
            sheet.OpenFullView(key);
            yield return CardTestInput.Settle();
            Assert.IsTrue(sheet.FullView.IsOpen, "precondition: the full view opened");
            AssertCrumbWhole(takeover, onePixel);
            Assert.IsTrue(takeover.SectionShown, entry.Case.Key + ": the section is in the breadcrumb");

            if (siblings.Count > 1)
            {
                Assert.AreEqual(siblings.Count, takeover.Chips.childCount, "a chip per sibling");
                var strip = takeover.ChipStrip.worldBound;
                Assert.LessOrEqual(strip.xMax, takeover.Root.worldBound.xMax + onePixel, "the chips' strip inside the screen");
                float firstTop = takeover.Chips[0].worldBound.yMin;
                foreach (var chip in takeover.Chips.Children())
                {
                    Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(chip.worldBound.width, chip.worldBound.height), chip.tooltip + ": a whole touch target");
                    Assert.AreEqual(firstTop, chip.worldBound.yMin, onePixel, "ONE line of chips");
                    bool current = chip.ClassListContains(TakeoverView.CurrentChipClass);
                    Assert.AreEqual(current, chip.resolvedStyle.unityFontStyleAndWeight == FontStyle.Bold, chip.tooltip + ": bold exactly when current");
                    Assert.AreEqual(current, chip.resolvedStyle.borderTopColor != chip.resolvedStyle.backgroundColor,
                        chip.tooltip + ": a ring exactly when current (the others' edge is their own fill)");
                    Assert.Greater(chip.resolvedStyle.borderTopWidth, 0f, "the ring's width is kept on every chip, so marking one never moves the others");
                    float contrast = CardTestInput.Contrast(chip.resolvedStyle.color, chip.resolvedStyle.backgroundColor);
                    Assert.GreaterOrEqual(contrast, UIAccessibility.MinRatioNormalText, chip.tooltip + ": its number at 4.5:1 (" + contrast.ToString("F2") + ")");
                }
                var marked = takeover.Chips[takeover.PageIndex];
                Assert.IsTrue(strip.Contains(marked.worldBound.center), "the current chip (" + takeover.PageIndex + " of " + takeover.PageCount + ") is in view: chip "
                    + marked.worldBound + " row " + takeover.Chips.worldBound + " (x " + takeover.Chips.layout.x + ", translate " + takeover.Chips.resolvedStyle.translate
                    + ") strip " + strip);
                yield return CardGalleryChecks.Render("Nav_full_view_" + entry.Case.Key);

                // - a real tap on another chip in view: that sibling
                int other = takeover.PageIndex == 0 ? 1 : 0;
                var chipToTap = takeover.Chips[other];
                Assert.IsTrue(strip.Contains(chipToTap.worldBound.center), "precondition: the chip to tap is in view");
                yield return CardTestInput.Tap(chipToTap.panel, chipToTap.worldBound.center);
                yield return CardTestInput.Settle();
                Assert.AreEqual(siblings[other], sheet.FullView.BlockKey, "a real tap on a chip shows its sibling");
                AssertCrumbWhole(takeover, onePixel);
            }
            else
            {
                Assert.AreEqual(DisplayStyle.None, takeover.ChipStrip.resolvedStyle.display, "a section of one: no chips");
                yield return CardGalleryChecks.Render("Nav_full_view_" + entry.Case.Key);
            }

            // - a section of one, where the case has one: no chips
            var single = model.Sections.FirstOrDefault(s => s.Count == 1);
            if (single != null)
            {
                sheet.OpenFullView(single.Targets[0]);
                yield return CardTestInput.Settle();
                Assert.AreEqual(1, takeover.PageCount);
                Assert.AreEqual(DisplayStyle.None, takeover.ChipStrip.resolvedStyle.display, single.Key + ": a section of one draws no chips");
                AssertCrumbWhole(takeover, onePixel);
                yield return CardGalleryChecks.Render("Nav_full_view_" + entry.Case.Key + "_one");
            }
            Assert.IsTrue(sheet.Back());
            yield return CardTestInput.Settle();
            Assert.IsFalse(takeover.IsOpen, "Back: the card");
        }

        // Every part of the breadcrumb is whole -- a label holds its words at its width (wrapping is fine, cutting is not) -- inside the screen;
        // the section pill is a whole touch target; the words read at 4.5:1
        private static void AssertCrumbWhole(TakeoverView takeover, float onePixel)
        {
            var screen = takeover.Root.worldBound;
            foreach (var label in new[] { takeover.CrumbTitle, takeover.CrumbSectionWords, takeover.CrumbName })
            {
                if (label.resolvedStyle.display == DisplayStyle.None || label.text.Length == 0 || !CardTestInput.IsShown(label, takeover.Root)) continue;
                var needs = label.MeasureTextSize(label.text, label.layout.width, VisualElement.MeasureMode.Exactly, 0, VisualElement.MeasureMode.Undefined);
                Assert.LessOrEqual(needs.y, label.layout.height + 0.5f, "'" + label.text + "': whole, never cut (" + label.layout + ")");
                Assert.LessOrEqual(label.worldBound.xMax, screen.xMax + onePixel, "'" + label.text + "': inside the screen");
                Assert.GreaterOrEqual(label.worldBound.xMin, screen.xMin - onePixel);
                float contrast = CardTestInput.Contrast(label.resolvedStyle.color, CardTestInput.EffectiveBackground(label));
                Assert.GreaterOrEqual(contrast, UIAccessibility.MinRatioNormalText, "'" + label.text + "' reads at 4.5:1 (" + contrast.ToString("F2") + ")");
            }
            if (takeover.SectionShown)
            {
                var pill = takeover.CrumbSection.worldBound;
                Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(pill.width, pill.height), "the section pill is a whole touch target (" + pill + ")");
                Assert.LessOrEqual(pill.xMax, screen.xMax + onePixel, "the pill inside the screen");
            }
        }
    }
}
