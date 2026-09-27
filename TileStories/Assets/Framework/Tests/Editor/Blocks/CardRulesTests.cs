using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The card's pure rules (_3.1 sections 3-4): BlockFieldReader (typed, language-aware reads), SheetStopRule
    // (stop heights, the half cap, where a released drag snaps) and CardTapRule (tap vs drag, tap-outside dismiss).
    public class CardRulesTests
    {
        private static List<LocalizedEntry> Text(params (string lang, string value)[] entries)
        {
            var list = new List<LocalizedEntry>();
            foreach (var (lang, value) in entries) list.Add(new LocalizedEntry { lang = lang, value = value });
            return list;
        }

        // ---------------- BlockFieldReader ----------------

        [Test]
        public void Localized_ReadsTheVisitorLanguage_ThenTheFallback_ThenAnyText()
        {
            var block = new BlockInstanceData { fields = new List<BlockFieldValue>
            {
                new() { key = "both", text = Text(("en", "Castle"), ("pt", "Castelo")) },
                new() { key = "en_only", text = Text(("en", "Gate")) },
                new() { key = "fr_only", text = Text(("fr", "Porte")) },
                new() { key = "blank_pt", text = Text(("pt", "  "), ("en", "Tower")) },
            } };
            var pt = new BlockFieldReader(block, "pt", "en");
            Assert.AreEqual("Castelo", pt.Text("both"));
            Assert.AreEqual("Gate", pt.Text("en_only"), "missing translation -> the wall's fallback language");
            Assert.AreEqual("Porte", pt.Text("fr_only"), "neither -> whatever text exists, never an empty card");
            Assert.AreEqual("Tower", pt.Text("blank_pt"), "a blank translation counts as missing");
            Assert.AreEqual("", pt.Text("absent"));
        }

        [Test]
        public void TypedReads_ReturnTheStoredValue_OrEmpty()
        {
            var item = new BlockItemData { fields = new List<BlockItemFieldValue> { new() { key = "label", text = Text(("en", "Height")) } } };
            var block = new BlockInstanceData { fields = new List<BlockFieldValue>
            {
                new() { key = "n", number = 12.5f }, new() { key = "f", flag = true }, new() { key = "c", value = "grid" },
                new() { key = "a", asset = "castle/hero.png" }, new() { key = "rows", items = new List<BlockItemData> { item } },
            } };
            var r = new BlockFieldReader(block, "en", "en");
            Assert.AreEqual(12.5f, r.Number("n"));
            Assert.IsTrue(r.Flag("f"));
            Assert.AreEqual("grid", r.Value("c"));
            Assert.AreEqual("castle/hero.png", r.Asset("a"));
            Assert.AreEqual("Height", r.ItemText(r.Items("rows")[0], "label"));
            Assert.AreEqual(0f, r.Number("missing"));
            Assert.AreEqual(0, r.Items("missing").Count);
        }

        [Test]
        public void HasContent_FollowsTheFieldType()
        {
            Assert.IsFalse(BlockFieldReader.HasContent(null, BlockFieldType.Number));
            Assert.IsFalse(BlockFieldReader.HasContent(new BlockFieldValue { text = Text(("en", " ")) }, BlockFieldType.LocalizedText));
            Assert.IsTrue(BlockFieldReader.HasContent(new BlockFieldValue { text = Text(("pt", "x")) }, BlockFieldType.LocalizedLongText));
            Assert.IsFalse(BlockFieldReader.HasContent(new BlockFieldValue(), BlockFieldType.Asset));
            Assert.IsFalse(BlockFieldReader.HasContent(new BlockFieldValue(), BlockFieldType.Items));
            Assert.IsFalse(BlockFieldReader.HasContent(new BlockFieldValue { value = "" }, BlockFieldType.PoiRef));
            Assert.IsTrue(BlockFieldReader.HasContent(new BlockFieldValue(), BlockFieldType.Toggle), "a toggle always has a value");
        }

        // ---------------- SheetStopRule ----------------

        [Test]
        public void Stops_HalfIsCappedAtTheRatio_AndOrdered()
        {
            var s = SheetStopRule.Compute(availableHeight: 800f, peekHeight: 120f, halfMaxRatio: 0.40f, fullTopGap: 48f);
            Assert.AreEqual(120f, s.Peek);
            Assert.AreEqual(320f, s.Half, 1e-3, "40% of 800");
            Assert.AreEqual(752f, s.Full);

            Assert.AreEqual(800f * CardContainerSettings.HalfMaxRatioMax, SheetStopRule.Compute(800f, 120f, 0.9f, 48f).Half, 1e-3, "a ratio above the allowed max is capped: never above 40%");
            Assert.AreEqual(800f * CardContainerSettings.HalfMaxRatioMin, SheetStopRule.Compute(800f, 120f, 0.1f, 48f).Half, 1e-3, "and never below the min");
            Assert.AreEqual(400f, SheetStopRule.Compute(800f, 400f, 0.25f, 48f).Half, "half never under a tall peek");
            var tiny = SheetStopRule.Compute(100f, 300f, 0.40f, 48f);
            Assert.IsTrue(tiny.Peek <= tiny.Half && tiny.Half <= tiny.Full && tiny.Full <= 100f, "a short screen keeps peek <= half <= full <= available");
        }

        [Test]
        public void OpenStop_IsPeekOrHalf_NeverFull()
        {
            Assert.AreEqual(SheetStopRule.Stop.Peek, SheetStopRule.OpenStop(CardOptions.StopPeek));
            Assert.AreEqual(SheetStopRule.Stop.Half, SheetStopRule.OpenStop(CardOptions.StopHalf));
            Assert.AreEqual(SheetStopRule.Stop.Peek, SheetStopRule.OpenStop(CardOptions.StopFull), "never auto-expanded");
            Assert.AreEqual(SheetStopRule.Stop.Peek, SheetStopRule.OpenStop("garbage"));
        }

        [Test]
        public void ASlowRelease_SnapsToTheNearestStop_OrClosesWellBelowPeek()
        {
            var s = SheetStopRule.Compute(800f, 120f, 0.40f, 48f);   // 120 / 320 / 752
            Assert.AreEqual(SheetStopRule.Stop.Peek, SheetStopRule.Snap(s, 200f, 0f, 800f));
            Assert.AreEqual(SheetStopRule.Stop.Half, SheetStopRule.Snap(s, 240f, 0f, 800f));
            Assert.AreEqual(SheetStopRule.Stop.Full, SheetStopRule.Snap(s, 600f, 0f, 800f));
            Assert.AreEqual(SheetStopRule.Stop.Peek, SheetStopRule.Snap(s, 80f, 0f, 800f), "a little below peek springs back");
            Assert.AreEqual(SheetStopRule.Stop.Dismissed, SheetStopRule.Snap(s, 60f, 0f, 800f), "pulled well below peek: closed");
        }

        [Test]
        public void AFlick_GoesOneStopFurtherInItsDirection()
        {
            var s = SheetStopRule.Compute(800f, 120f, 0.40f, 48f);
            float flick = SheetStopRule.FlickHeightsPerSecond * 800f + 1f;
            Assert.AreEqual(SheetStopRule.Stop.Half, SheetStopRule.Snap(s, 140f, flick, 800f), "a quick swipe up from peek -> half, though nearer peek");
            Assert.AreEqual(SheetStopRule.Stop.Full, SheetStopRule.Snap(s, 340f, flick, 800f));
            Assert.AreEqual(SheetStopRule.Stop.Half, SheetStopRule.Snap(s, 700f, -flick, 800f));
            Assert.AreEqual(SheetStopRule.Stop.Dismissed, SheetStopRule.Snap(s, 110f, -flick, 800f), "a quick swipe down from peek closes");
            Assert.AreEqual(SheetStopRule.Stop.Peek, SheetStopRule.Snap(s, 140f, flick * 0.5f, 800f), "under the flick speed it is a normal release");
        }

        // ---------------- CardTapRule ----------------

        [Test]
        public void ATap_IsShortAndStill_ADragOrLongPressIsNot()
        {
            Assert.IsTrue(CardTapRule.IsTap(new Vector2(100, 100), new Vector2(105, 102), 0f, 0.1f, 1000f));
            Assert.IsFalse(CardTapRule.IsTap(new Vector2(100, 100), new Vector2(100, 160), 0f, 0.1f, 1000f), "moved 6% of the screen: a drag");
            Assert.IsFalse(CardTapRule.IsTap(new Vector2(100, 100), new Vector2(100, 100), 0f, 0.8f, 1000f), "held: a long press");
        }

        [Test]
        public void OnlyATapOnNothing_WithTheSettingOn_ClosesAnOpenCard()
        {
            Assert.IsTrue(CardTapRule.ShouldDismiss(cardOpen: true, dismissOnTapOutside: true, isTap: true, anythingUnderTap: false));
            Assert.IsFalse(CardTapRule.ShouldDismiss(true, true, true, anythingUnderTap: true), "a marker or the card itself was tapped");
            Assert.IsFalse(CardTapRule.ShouldDismiss(true, dismissOnTapOutside: false, true, false), "Tap Outside Closes off");
            Assert.IsFalse(CardTapRule.ShouldDismiss(true, true, isTap: false, false), "a drag across the wall");
            Assert.IsFalse(CardTapRule.ShouldDismiss(cardOpen: false, true, true, false), "no card open");
        }

        // _3.1 step 6C: the pinned header collapses once the stack scrolls, opens only back at the top, and never collapses
        // a stack that could not scroll any more afterwards (that would snap to the top, open, scroll... a loop)
        [Test]
        public void HeaderCollapseRule_CollapsesOnScroll_OpensOnlyAtTheTop_AndNeverLoops()
        {
            const float range = 600f, freed = 120f;
            Assert.IsFalse(HeaderCollapseRule.Next(false, 0f, range, freed), "at the top: open");
            Assert.IsFalse(HeaderCollapseRule.Next(false, HeaderCollapseRule.CollapseAfter, range, freed), "a finger's wobble at the top: still open");
            Assert.IsTrue(HeaderCollapseRule.Next(false, 40f, range, freed), "scrolled down: collapsed");
            Assert.IsTrue(HeaderCollapseRule.Next(true, 4f, range, freed), "collapsed, scrolled almost back: stays collapsed (hysteresis)");
            Assert.IsFalse(HeaderCollapseRule.Next(true, 0f, range, freed), "back at the very top: open");
            Assert.IsFalse(HeaderCollapseRule.Next(false, 40f, freed + 5f, freed),
                "collapsing would leave less than CollapseAfter to scroll: stays open (no snap-open loop)");
            Assert.IsTrue(HeaderCollapseRule.Next(false, 40f, freed + HeaderCollapseRule.CollapseAfter + 1f, freed), "just enough left to scroll: collapses");
        }
    }
}
