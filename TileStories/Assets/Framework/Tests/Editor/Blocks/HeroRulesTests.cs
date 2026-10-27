using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // _3.3 P4.4a: the header's bar and hero, pure -- HeroRule (the hero's height, when the bar shows the title, whether the hero is in
    // view), HeroMotionRule (pan, zoom, parallax, the words' rise and fade, reduce motion), HeroActionsRule (which blocks the hero draws
    // at its foot), the chrome's hero layer (CardChromeRule) and the header's new fields
    public class HeroRulesTests
    {
        private static SheetStopRule.Stops LampStops => new(57f, 337.6f, 796f);

        // _3.3 S4a review: the hero is the whole sheet at Full -- its picture runs up under the floating bars (never under the card's
        // minimum) -- and its words sit at its foot, whatever the stop
        [Test]
        public void TheHero_IsTheWholeSheetAtFull_ItsWordsAtItsFoot()
        {
            var top = new SheetStopRule.Stops(57f, 337.6f, 844f);
            Assert.AreEqual(844f - 57f, HeroRule.FullView(top), 0.001f, "To The Top on the 390 x 844 phone: 787 under the bar");
            Assert.AreEqual(844f, HeroRule.Height(top, 200f), 0.001f, "the hero is the whole sheet, the bars floating over it");
            Assert.AreEqual(796f, HeroRule.Height(LampStops, 200f), 0.001f, "with a camera strip: its Full");
            Assert.AreEqual(200f, HeroRule.Height(new SheetStopRule.Stops(57f, 150f, 200f), 200f), 0.001f, "a tiny screen: never under the minimum");
            Assert.AreEqual(200f, HeroRule.Height(new SheetStopRule.Stops(57f, float.NaN, float.NaN), 200f), 0.001f, "not measured yet: the minimum");
            Assert.AreEqual(0f, HeroRule.Height(new SheetStopRule.Stops(0f, 0f, 0f), -5f), 0.001f, "nothing negative");

            Assert.AreEqual(337.6f - 57f, HeroRule.HalfView(top), 0.001f, "Half's view under the bar");
            Assert.AreEqual(844f, HeroRule.WordsBottom(844f), 0.001f, "the words end at the hero's foot");
            Assert.AreEqual(0f, HeroRule.WordsBottom(float.NaN), 0.001f, "not measured yet: nothing");
            Assert.AreEqual(0f, HeroRule.WordsBottom(-5f), 0.001f, "nothing negative");
        }

        [Test]
        public void TheBar_ShowsTheTitle_AtPeek_AndOnceTheHerosTitleScrolledAway()
        {
            const float titleBottom = 210f;
            Assert.IsTrue(HeroRule.BarShowsTitle(SheetStopRule.Stop.Peek, 0f, titleBottom), "at Peek the hero is below the sheet's edge");
            Assert.IsTrue(HeroRule.BarShowsTitle(SheetStopRule.Stop.Peek, 0f, float.NaN), "...even before a layout");
            foreach (var stop in new[] { SheetStopRule.Stop.Half, SheetStopRule.Stop.Full })
            {
                Assert.IsFalse(HeroRule.BarShowsTitle(stop, 0f, titleBottom), stop + ": at the top the hero carries it");
                Assert.IsFalse(HeroRule.BarShowsTitle(stop, titleBottom - 2f, titleBottom), stop + ": a sliver of the hero's title still shows");
                Assert.IsTrue(HeroRule.BarShowsTitle(stop, titleBottom, titleBottom), stop + ": its title has left the viewport's top");
                Assert.IsFalse(HeroRule.BarShowsTitle(stop, 500f, float.NaN), stop + ": no layout yet = the hero carries it");
            }
        }

        [Test]
        public void TheHero_IsInView_UntilItsBottomPassesTheViewportsTop()
        {
            Assert.IsTrue(HeroRule.InView(0f, 280f));
            Assert.AreEqual(280f, HeroRule.VisiblePart(0f, 280f), 0.001f);
            Assert.AreEqual(80f, HeroRule.VisiblePart(200f, 280f), 0.001f);
            Assert.IsFalse(HeroRule.InView(279.5f, 280f), "within one pixel of rounding: gone");
            Assert.IsFalse(HeroRule.InView(0f, float.NaN), "no hero: nothing in view");
            Assert.AreEqual(0f, HeroRule.VisiblePart(400f, 280f), 0.001f);
        }

        [Test]
        public void ANarrowCard_TakesTheSmallerHeroTitle()
        {
            Assert.IsTrue(HeroRule.IsNarrow(390f), "the reference phone: 40");
            Assert.IsTrue(HeroRule.IsNarrow(360f), "a small phone");
            Assert.IsFalse(HeroRule.IsNarrow(430f), "a large phone / tablet: 46");
            Assert.IsFalse(HeroRule.IsNarrow(float.NaN), "no layout");
        }

        [Test]
        public void ThePan_CrossesTheWholePictureAndBack_OncePerPeriod_EasedAtBothEnds()
        {
            const float overflow = 400f;
            float period = HeroMotionRule.PeriodFor(overflow);
            float half = period / 2f;
            Assert.AreEqual(0f, HeroMotionRule.PanOffset(HeroMotionRule.Pan, false, 0f, overflow), 0.001f, "starts at the picture's left edge");
            Assert.AreEqual(-overflow, HeroMotionRule.PanOffset(HeroMotionRule.Pan, false, half, overflow), 0.001f, "half way: its right edge");
            Assert.AreEqual(0f, HeroMotionRule.PanOffset(HeroMotionRule.Pan, false, period, overflow), 0.001f, "and back");
            Assert.AreEqual(-overflow / 2f, HeroMotionRule.PanOffset(HeroMotionRule.Pan, false, half / 2f, overflow), 0.001f, "the middle at a quarter");
            float early = -HeroMotionRule.PanOffset(HeroMotionRule.Pan, false, half * 0.1f, overflow);
            Assert.Less(early, overflow * 0.1f, "eased: slower than linear near the end (" + early + ")");
            // - monotonic on the way there
            float last = 1f;
            for (float t = 0f; t <= half; t += 0.5f)
            {
                float x = HeroMotionRule.PanOffset(HeroMotionRule.Pan, false, t, overflow);
                Assert.LessOrEqual(x, last + 0.0001f, "always moving the same way on the way there (t " + t + ")");
                last = x;
            }
        }

        // _3.3 S4a: a full-screen hero crops a wide picture a lot -- the pan's period grows with its way, so it never slides faster than
        // MaxPanSpeed; Pan And Zoom drifts AND pushes in on that same period; both are "moving", both still under Reduce Motion
        [Test]
        public void ThePanNeverSlidesFasterThanAWalk_AndPanAndZoom_DriftsAndPushesInTogether()
        {
            Assert.AreEqual(HeroMotionRule.PanPeriod, HeroMotionRule.PeriodFor(100f), 0.001f, "a short way: the 26 s period");
            Assert.AreEqual(2f * 1010f / HeroMotionRule.MaxPanSpeed, HeroMotionRule.PeriodFor(1010f), 0.001f, "the castle cropped to 390 x 787: slower");
            Assert.AreEqual(HeroMotionRule.PanPeriod, HeroMotionRule.PeriodFor(float.NaN), 0.001f);
            // - the fastest the picture moves over a whole period, sampled finely: never above the limit (the ease's peak is 1.5x linear)
            foreach (float overflow in new[] { 100f, 400f, 1010f })
            {
                float period = HeroMotionRule.PeriodFor(overflow), fastest = 0f, step = period / 2000f;
                for (float t = 0f; t < period; t += step)
                    fastest = Mathf.Max(fastest, Mathf.Abs(HeroMotionRule.PanOffset(HeroMotionRule.Pan, false, t + step, overflow)
                                                           - HeroMotionRule.PanOffset(HeroMotionRule.Pan, false, t, overflow)) / step);
                Assert.LessOrEqual(fastest, HeroMotionRule.MaxPanSpeed * 1.5f + 0.01f, "overflow " + overflow + ": its fastest moment " + fastest + " pt/s");
            }
            const float way = 1010f;
            float half = HeroMotionRule.PeriodFor(way) / 2f;
            Assert.AreEqual(HeroMotionRule.PanOffset(HeroMotionRule.Pan, false, half * 0.3f, way),
                HeroMotionRule.PanOffset(HeroMotionRule.PanZoom, false, half * 0.3f, way), 0.001f, "pan and zoom drifts as pan does");
            Assert.AreEqual(HeroMotionRule.ZoomMax, HeroMotionRule.Scale(HeroMotionRule.PanZoom, false, half, way), 0.0001f, "...and is pushed in at the far end");
            Assert.AreEqual(1f, HeroMotionRule.Scale(HeroMotionRule.PanZoom, false, 0f, way), 0.0001f, "...and not at the start");
            Assert.IsTrue(HeroMotionRule.Moves(HeroMotionRule.PanZoom, false));
            Assert.IsFalse(HeroMotionRule.Moves(HeroMotionRule.PanZoom, true), "reduce motion: still");
            Assert.AreEqual(-way / 2f, HeroMotionRule.PanOffset(HeroMotionRule.PanZoom, true, half * 0.3f, way), 0.001f, "...at its centre");
            Assert.AreEqual(1f, HeroMotionRule.Scale(HeroMotionRule.PanZoom, true, half, way), 0.0001f, "...not pushed in");
        }

        [Test]
        public void ReduceMotion_None_Zoom_AndANarrowPicture_KeepThePictureStillAtItsCentre()
        {
            const float overflow = 300f;
            Assert.AreEqual(-150f, HeroMotionRule.PanOffset(HeroMotionRule.Pan, true, 7f, overflow), 0.001f, "reduce motion: the centre");
            Assert.AreEqual(-150f, HeroMotionRule.PanOffset(HeroMotionRule.None, false, 7f, overflow), 0.001f, "none: the centre");
            Assert.AreEqual(-150f, HeroMotionRule.PanOffset(HeroMotionRule.Zoom, false, 7f, overflow), 0.001f, "zoom pans nothing");
            Assert.AreEqual(0f, HeroMotionRule.PanOffset(HeroMotionRule.Pan, false, 7f, 0f), 0.001f, "a picture no wider than its frame stays");
            Assert.IsFalse(HeroMotionRule.Moves(HeroMotionRule.Pan, true));
            Assert.IsFalse(HeroMotionRule.Moves(HeroMotionRule.None, false));
            Assert.IsTrue(HeroMotionRule.Moves(HeroMotionRule.Zoom, false));
        }

        [Test]
        public void TheZoom_PushesInToItsMaxAndBack_OnlyForZoom_AndNeverWithReduceMotion()
        {
            float half = HeroMotionRule.PanPeriod / 2f;
            Assert.AreEqual(1f, HeroMotionRule.Scale(HeroMotionRule.Zoom, false, 0f), 0.0001f);
            Assert.AreEqual(HeroMotionRule.ZoomMax, HeroMotionRule.Scale(HeroMotionRule.Zoom, false, half), 0.0001f);
            Assert.AreEqual(1f, HeroMotionRule.Scale(HeroMotionRule.Zoom, true, half), 0.0001f, "reduce motion");
            Assert.AreEqual(1f, HeroMotionRule.Scale(HeroMotionRule.Pan, false, half), 0.0001f, "pan never scales");
        }

        [Test]
        public void Parallax_AndTheWords_FollowTheScroll_AndReduceMotionStillsThem()
        {
            Assert.AreEqual(45f, HeroMotionRule.Parallax(100f, 280f, false), 0.001f, "0.45 of the scroll");
            Assert.AreEqual(280f, HeroMotionRule.Parallax(1000f, 280f, false), 0.001f, "never past its frame");
            Assert.AreEqual(0f, HeroMotionRule.Parallax(-20f, 280f, false), 0.001f, "an overscroll at the top does not lift it");
            Assert.AreEqual(0f, HeroMotionRule.Parallax(100f, 280f, true), 0.001f, "reduce motion");
            Assert.AreEqual(-25f, HeroMotionRule.TextRise(100f, false), 0.001f, "the words rise past the scroll (they outrun the picture)");
            Assert.AreEqual(0f, HeroMotionRule.TextRise(100f, true), 0.001f);
            // - the fade is by the words' place in the view: whole until they near the chrome's foot (60 here), gone as they reach it
            Assert.AreEqual(1f, HeroMotionRule.TextOpacity(700f, 60f, false), 0.001f, "low on the picture: whole");
            Assert.AreEqual(1f, HeroMotionRule.TextOpacity(60f + HeroMotionRule.TextFadeSpan, 60f, false), 0.001f, "...until the fade's span");
            Assert.AreEqual(0.5f, HeroMotionRule.TextOpacity(60f + HeroMotionRule.TextFadeSpan / 2f, 60f, false), 0.001f, "fading near the top");
            Assert.AreEqual(0f, HeroMotionRule.TextOpacity(40f, 60f, false), 0.001f, "under the chrome: gone");
            Assert.AreEqual(1f, HeroMotionRule.TextOpacity(40f, 60f, true), 0.001f, "reduce motion: whole");
            Assert.AreEqual(1f, HeroMotionRule.TextOpacity(float.NaN, 60f, false), 0.001f, "not laid out: whole");
        }

        private static BlockStackBuilder.Entry Entry(BlockKindDefinition kind, string variant, string key) =>
            new(new BlockInstanceData { key = key, kind = kind.Key, variant = variant }, kind, variant, false);

        [Test]
        public void TheHerosActions_AreTheStickyAction_AndThePinnedAudioGuideElseTheFirstOne()
        {
            var entries = new List<BlockStackBuilder.Entry>
            {
                Entry(BuiltInBlocks.Header, BuiltInBlocks.HeaderImageParallax, "block_1"),
                Entry(BuiltInBlocks.Actions, BuiltInBlocks.ActionsPillRow, "block_2"),
                Entry(BuiltInBlocks.AudioGuide, BuiltInBlocks.AudioGuidePlayer, "block_3"),
                Entry(BuiltInBlocks.AudioGuide, BuiltInBlocks.AudioGuideHeroChip, "block_4"),
                Entry(BuiltInBlocks.Actions, BuiltInBlocks.ActionsStickyCta, "block_5"),
            };
            var picks = HeroActionsRule.Of(entries);
            Assert.AreEqual(4, picks.Primary, "the sticky action, never a pill row");
            Assert.AreEqual(3, picks.Listen, "the pinned hero chip wins over an earlier player");
            entries.RemoveAt(3);
            Assert.AreEqual(2, HeroActionsRule.Of(entries).Listen, "no hero chip: the first audio guide in any look");
            entries.RemoveAt(3);
            entries.RemoveAt(2);
            picks = HeroActionsRule.Of(entries);
            Assert.IsFalse(picks.HasPrimary, "no sticky action: no primary");
            Assert.IsFalse(picks.HasListen, "no audio guide: no Listen");
            Assert.IsFalse(HeroActionsRule.Of(null).HasPrimary);
        }

        private static CardChromeRule.Input HalfWithHero(float scroll) => new()
        {
            Stop = SheetStopRule.Stop.Half, ShowAt = CardOptions.ShowNavHalfAndFull, HasTop = true, HasFooter = true,
            SheetHeight = 337.6f, HeaderHeight = 57f, TopHeight = 49f, FooterHeight = 81f, ContentHeight = 5000f,
            HeroBottom = 280.6f, ScrollOffset = scroll,
        };

        [Test]
        public void TheChrome_WaitsForTheHero_TheStickyBarComesOnceItIsRead_TheStripNeverCutsIt()
        {
            var atTop = CardChromeRule.Decide(HalfWithHero(0f));
            Assert.IsFalse(atTop.Bottom, "the hero shows the action: no sticky bar over it");
            Assert.IsFalse(atTop.Top, "the strip would cut the hero's foot at Half");
            var partly = CardChromeRule.Decide(HalfWithHero(200f));
            Assert.IsFalse(partly.Bottom, "some hero still in view: still no sticky bar");
            Assert.IsTrue(partly.Top, "80 of hero + the strip fit: the map comes back");
            var read = CardChromeRule.Decide(HalfWithHero(300f));
            Assert.IsTrue(read.Bottom, "the hero read: the sticky bar");
            Assert.IsTrue(read.Top, "and the strip (280.6 - 49 - 81 >= the floor)");
            var bar = HalfWithHero(0f);
            bar.HasBottomNav = true;
            bar.BottomNavHeight = 52f;
            bar.Stop = SheetStopRule.Stop.Full;
            bar.SheetHeight = 796f;
            var withBar = CardChromeRule.Decide(bar);
            Assert.IsTrue(withBar.Bottom && withBar.BottomNav, "the bottom bar is navigation: it shows beside the hero where it does not cut it");
            Assert.IsFalse(withBar.Footer, "...its footer still waits for the hero");
            Assert.AreEqual(796f - 57f - 49f - 52f, CardChromeRule.Viewport(bar, withBar), 0.01f, "the viewport counts the bar alone");
            bar.Stop = SheetStopRule.Stop.Half;
            bar.SheetHeight = 337.6f;
            Assert.IsFalse(CardChromeRule.Decide(bar).Bottom, "at Half the bar would cut the hero: it gives way");
            var full = HalfWithHero(0f);
            full.Stop = SheetStopRule.Stop.Full;
            full.SheetHeight = 796f;
            Assert.IsTrue(CardChromeRule.Decide(full).Top, "at Full the strip fits beside the whole hero");
            Assert.IsFalse(CardChromeRule.Decide(full).Bottom, "...the sticky bar still waits for it");
            var noHero = HalfWithHero(0f);
            noHero.HeroBottom = float.NaN;
            Assert.IsTrue(CardChromeRule.Decide(noHero).Bottom, "no hero: the old floors alone");
        }

        [Test]
        public void TheHeader_OffersEyebrowAndHeroMotion_AndNoLongerTheCategoryChip()
        {
            var kind = BuiltInBlocks.Header;
            var motion = kind.Field(BuiltInBlocks.HeaderHeroMotionField);
            Assert.IsNotNull(motion, "Hero Motion");
            Assert.AreEqual(BlockFieldType.Choice, motion.Type);
            CollectionAssert.AreEqual(new[] { HeroMotionRule.PanZoom, HeroMotionRule.Pan, HeroMotionRule.Zoom, HeroMotionRule.None }, motion.Options);
            Assert.AreEqual(HeroMotionRule.PanZoom, motion.ChoiceDefault, "pan and zoom by default (_3.3 S4a)");
            Assert.IsTrue(motion.LibraryDefault, "a wall may set its default in the Block Library");
            Assert.IsFalse(string.IsNullOrEmpty(motion.Help), "its (i)");
            Assert.AreEqual(BlockFieldType.LocalizedText, kind.Field(BuiltInBlocks.HeaderEyebrowField)?.Type, "Eyebrow, per language");
            Assert.IsFalse(kind.Fields.Any(f => f.Key == "show_level"), "Show Level went with the chip");
            var category = BuiltInBlocks.QuickFacts.Field(BuiltInBlocks.QuickFactsCategoryField);
            Assert.IsNotNull(category, "the category's new home: Quick Facts > Show Category");
            Assert.AreEqual(BuiltInBlocks.QuickFactsCategoryNone, category.ChoiceDefault, "off by default");
            Assert.IsTrue(BlockRegistry.Shared.TryGet(kind.Key, out _), "the registry still validates the header with its new fields");
        }
    }
}
