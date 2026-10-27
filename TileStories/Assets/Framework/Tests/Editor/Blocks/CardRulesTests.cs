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
        public void RevealsBlocks_OnlyHalfAndFull_NotPeekOrDismissed()
        {
            Assert.IsTrue(SheetStopRule.RevealsBlocks(SheetStopRule.Stop.Half));
            Assert.IsTrue(SheetStopRule.RevealsBlocks(SheetStopRule.Stop.Full));
            Assert.IsFalse(SheetStopRule.RevealsBlocks(SheetStopRule.Stop.Peek), "peek shows the compact header only: a block's stage sits under the sheet");
            Assert.IsFalse(SheetStopRule.RevealsBlocks(SheetStopRule.Stop.Dismissed));
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

        // _3.3 S4a: the wall's Full Height -- To The Top leaves no gap (the sheet reaches the safe area's top), Leave A Camera Strip the card's
        // top gap; anything else (an old or hand-edited value) is the default; only Full moves
        [Test]
        public void FullTopGap_ToTheTopIsNone_ACameraStripIsTheGap_AnUnknownChoiceIsTheDefault_AndOnlyFullMoves()
        {
            Assert.AreEqual(0f, SheetStopRule.FullTopGap(CardOptions.FullHeightTop, 48f), "To The Top");
            Assert.AreEqual(48f, SheetStopRule.FullTopGap(CardOptions.FullHeightCameraStrip, 48f), "Leave A Camera Strip");
            foreach (string other in new[] { null, "", "full", "TOP" })
                Assert.AreEqual(0f, SheetStopRule.FullTopGap(other, 48f), "'" + other + "' reads as the default");
            Assert.AreEqual(0f, SheetStopRule.FullTopGap(CardOptions.FullHeightCameraStrip, -5f), "a negative gap is none");
            CollectionAssert.AreEqual(new[] { "To The Top", "Leave A Camera Strip" }, CardOptions.FullHeightLabels, "a label per option, in order");
            Assert.AreEqual(CardOptions.FullHeights.Length, CardOptions.FullHeightLabels.Length);

            var top = SheetStopRule.Compute(844f, 57f, 0.40f, SheetStopRule.FullTopGap(CardOptions.FullHeightTop, 48f));
            var strip = SheetStopRule.Compute(844f, 57f, 0.40f, SheetStopRule.FullTopGap(CardOptions.FullHeightCameraStrip, 48f));
            Assert.AreEqual(844f, top.Full, 1e-4f, "To The Top: the whole available height");
            Assert.AreEqual(796f, strip.Full, 1e-4f, "a camera strip of 48");
            Assert.AreEqual(top.Peek, strip.Peek);
            Assert.AreEqual(top.Half, strip.Half);
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

        // _3.3 S4a: a tap belongs to what was under its PRESS -- a block page's Back closes the page on the release, and the same point then
        // shows the camera. The real CardTapOutside with no zoom (it closes at once) and no EventSystem in Edit Mode, so the release lands on
        // nothing: only the press decides
        [Test]
        public void CardTapOutside_ATapThatLandedOnSomething_NeverClosesTheCard_EvenWhenItsReleaseFindsNothing()
        {
            int closes = 0;
            var tapOutside = new CardTapOutside(() => 0f, () => new CardSettings(), () => null, () => true, () => closes++);
            var at = new Vector2(40f, 40f);
            Assert.IsFalse(tapOutside.HandleScreenTap(at, at, 0f, 0.1f, pressedOnSomething: true), "the press landed on a button: the button's tap");
            Assert.AreEqual(0, closes, "...the card stays open");
            Assert.IsTrue(tapOutside.HandleScreenTap(at, at, 1f, 1.1f, pressedOnSomething: false), "a press and a release on nothing: a tap on empty space");
            Assert.AreEqual(1, closes, "...closes the card");
        }

        // _3.1 step 7B: only a whole web address ever reaches the device (today_map's Directions)
        [Test]
        public void WebLinkRule_OpensOnlyAWholeHttpOrHttpsAddress()
        {
            Assert.IsTrue(WebLinkRule.IsOpenable("https://maps.example.org/place?q=38.7,-9.1"));
            Assert.IsTrue(WebLinkRule.IsOpenable("http://example.org"));
            Assert.AreEqual("https://example.org/a", WebLinkRule.Openable("  https://example.org/a  "), "trimmed");
            foreach (var refused in new[] { null, "", "   ", "www.example.org", "example.org/place", "javascript:alert(1)", "file:///C:/secret.txt",
                         "mailto:someone@example.org", "https://", "maps" })
            {
                Assert.IsFalse(WebLinkRule.IsOpenable(refused), "refused: '" + refused + "'");
                Assert.AreEqual("", WebLinkRule.Openable(refused));
            }
            // - a stored Url field: HasContent is "a link the card opens", so a required one that is not is not shown
            Assert.IsTrue(BlockFieldReader.HasContent(new BlockFieldValue { value = "https://example.org" }, BlockFieldType.Url));
            Assert.IsFalse(BlockFieldReader.HasContent(new BlockFieldValue { value = "example.org" }, BlockFieldType.Url));
        }
    }
}
