using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // The card's chapters on the REAL LivingRoomScene (_3.3 P4.4c1, R11, R10.7; SectionSheets): The Lamp's scroll blocks sit on glass sheets
    // by family -- a new sheet wherever the family changes (its framework-default order interleaves families), the navigation family on the
    // plain ground; each sheet tinted with its family's glass and laid over the one before; the opener (a filled family pill + a line) names
    // the family with its blocks' count and minutes, in both languages, its words on the pill (D8), not sticky; a jump to a section lands
    // on its sheet's top with the opener in view and the spy naming it; the opener's words >= 7:1 on the RENDER in both modes; captures for
    // the eye. Not Core: the Cards look is an option, and CORE guards the shipped default (the Open sections, PoiCardSectionsSceneTests).
    public class SectionSheetSceneTests : SearchSceneFixture
    {
        // This fixture's subject is the CARDS look (the family glass sheets, the block frame, the moving light): the framework's default is
        // Open since _3.3 P3, so the look measured here is pinned to Cards after SearchSceneFixture's own pins
        [UnitySetUp]
        public IEnumerator PinTheCardsLook()
        {
            var config = ConfigCopy();
            config.card_settings.container.blocks = CardOptions.BlocksCards;
            Session.ApplyCardSettings(config);
            yield return CardTestInput.Settle(0.3f);
        }

        protected PoiCardSheetView Sheet => Card.Sheet;

        protected BlockStackView Stack => Sheet.Stack;

        protected ScrollView Scroll => Stack.Scroll;

        protected float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;

        protected float Range => NavScrollRule.Range(Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height);

        protected List<SectionSheets.Sheet> Shown => Stack.Sheets.All.Where(s => s.Look.Shown).ToList();

        [UnityTearDown]
        public IEnumerator RestoreTheMode()
        {
            ThemeController.SetMode("");
            yield return null;
        }

        protected IEnumerator Open(string poi, SheetStopRule.Stop stop = SheetStopRule.Stop.Full)
        {
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            SelectionEventBus.Select(poi);
            yield return CardTestInput.Settle();
            Sheet.SetStop(stop);
            yield return CardTestInput.Settle();
            Assert.IsTrue(Sheet.IsOpen, "precondition: " + poi + "'s card is open");
        }

        protected IEnumerator UseLayout(string layout)
        {
            var copy = ConfigCopy();
            copy.card_settings.navigation.layout = layout;
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        // The minutes a sheet's shown blocks give (their Reading Time; a clip's length is never loaded for it), read from the wall's config
        protected int MinutesOf(SectionSheets.Sheet sheet)
        {
            int minutes = 0;
            foreach (var block in Session.SearchPois.First(p => p.id == "painting").card.blocks)
            {
                var slot = Stack.SlotOfBlock(block.key);
                if (slot == null || !sheet.Slots.Contains(slot) || slot.resolvedStyle.display != DisplayStyle.Flex) continue;
                Assert.IsTrue(Stack.TryGetEntryOfBlock(block.key, out var entry), "precondition: " + block.key + " is bound");
                minutes += ReadingTimeRule.Minutes(entry.Definition, entry.Instance, 0f);
            }
            return minutes;
        }

        protected static IEnumerator Grab(string name)
        {
            yield return new WaitForEndOfFrame();
            var shot = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(TestEvidence.PathFor("Sections/look", name + ".png"), shot.EncodeToPNG());
            Object.Destroy(shot);
        }

        protected Color PixelAt(Texture2D shot, Vector2 panelPoint)
        {
            float scale = shot.width / Sheet.Layer.panel.visualTree.layout.width;
            int x = Mathf.Clamp(Mathf.FloorToInt(panelPoint.x * scale), 0, shot.width - 1);
            int y = Mathf.Clamp(shot.height - 1 - Mathf.FloorToInt(panelPoint.y * scale), 0, shot.height - 1);
            return shot.GetPixel(x, y);
        }

        // ---------------- structure ----------------

        [UnityTest]
        public IEnumerator TheLampsBlocks_SitOnOneSheetPerRunOfAFamily_ANewSheetWhereverTheFamilyComesBack()
        {
            yield return Open("lamp");
            var sheets = Stack.Sheets.All;
            var scrollSlots = Scroll.contentContainer.Query(className: "card-block-slot").ToList();
            Assert.Greater(scrollSlots.Count, 50, "precondition: The Lamp's scroll holds its blocks");
            foreach (var slot in scrollSlots)
                Assert.AreEqual(1, sheets.Count(s => s.Slots.Contains(slot)), "every block of the scroll sits on exactly one sheet");
            foreach (var sheet in sheets)
            {
                Assert.AreSame(Scroll.contentContainer, sheet.Root.parent, "a sheet is a part of the scroll, laid out in it");
                Assert.IsNotNull(sheet.Family, "precondition: " + sheet.Run.Family + " is a registered family");
            }
            for (int i = 1; i < sheets.Count; i++)
                Assert.AreNotEqual(sheets[i - 1].Run.Family, sheets[i].Run.Family, "two sheets side by side are two families: one family's run is ONE sheet");
            // - the framework's default order interleaves The Lamp's families: Visit comes back after Media, so it has two sheets
            Assert.GreaterOrEqual(sheets.Count(s => s.Run.Family == "visit"), 2, "a family that comes back opens a new sheet (interleaved families)");
            Assert.AreEqual(sheets.Sum(s => s.Slots.Count), scrollSlots.Count, "and nothing is left off a sheet");
        }

        [Category("CardSmoke")]
        [UnityTest]
        public IEnumerator EachSheet_IsLaidOverTheOneBefore_AndTheSpaceBeforeAnOpener_IsTwiceTheBlockGap()
        {
            yield return Open("painting");
            var sheets = Shown;
            Assert.Greater(sheets.Count, 4, "precondition: The Painting reads as several chapters");
            // - the overlap is the sheet's own resolved top margin (--ts-section-overlap: a negative length, which TokenPx does not read)
            float overlap = -sheets[1].Root.resolvedStyle.marginTop, gap = CardTestInput.TokenPx("--ts-block-gap");
            Assert.Greater(overlap, 0f, "precondition: --ts-section-overlap lays a sheet over the one before");
            for (int i = 1; i < sheets.Count; i++)
            {
                var before = sheets[i - 1].Root.worldBound;
                var sheet = sheets[i].Root.worldBound;
                Assert.IsTrue(sheets[i].Look.Overlaps, sheets[i].Run.Family + ": laid over " + sheets[i - 1].Run.Family);
                Assert.AreEqual(overlap, before.yMax - sheet.yMin, OnePixel, sheets[i].Run.Family + ": starts that far above the sheet before ends");
                var lastBlock = sheets[i - 1].Slots.Last(s => s.resolvedStyle.display == DisplayStyle.Flex).worldBound;
                Assert.AreEqual(2f * gap, sheets[i].Opener.Root.worldBound.yMin - lastBlock.yMax, OnePixel,
                    sheets[i].Run.Family + ": twice the block gap between the last block before and the opener (R10.7)");
            }
        }

        // ---------------- the opener ----------------

        [UnityTest]
        public IEnumerator TheOpener_NamesItsFamily_WithItsBlocksAndMinutes_InBothLanguages_ItsWordsOnTheFilledPill(
            [Values("en", "pt")] string language)
        {
            Session.CardSettings.languages = new List<string> { language, language == "en" ? "pt" : "en" };
            yield return Open("painting");
            var table = AssetDatabase.LoadAssetAtPath<CardStringTable>("Assets/Framework/Runtime/UI/Screen/Cards/CardStrings.asset").Entries();
            var words = new CardStrings(table, null, null, language, "en");
            foreach (var sheet in Shown)
            {
                var opener = sheet.Opener;
                Assert.AreEqual(words.Get(sheet.Family.LabelKey), opener.Name.text, sheet.Run.Family + ": the family's own words");
                int count = sheet.Slots.Count(s => s.resolvedStyle.display == DisplayStyle.Flex);
                Assert.AreEqual(SectionSheetRule.Meta(count, MinutesOf(sheet), words.Get), opener.Meta.text, sheet.Run.Family + ": its blocks and minutes");
                StringAssert.StartsWith(count.ToString(), opener.Meta.text, sheet.Run.Family + ": the count the sheet really holds");
                // - D8: no text on the glass -- the pill's fill is the words' ground
                Color fill = default;
                Assert.IsTrue(ThemeController.TryResolverFor(opener.Root, out var theme) && theme.TryColour(sheet.Family.ColorToken, out fill), "precondition: the fill");
                Assert.AreEqual(fill, CardTestInput.EffectiveBackground(opener.Name), sheet.Run.Family + ": its name reads on the filled pill");
                Assert.AreEqual(fill, CardTestInput.EffectiveBackground(opener.Meta), sheet.Run.Family + ": and its meta");
                Assert.AreEqual(fill, opener.Line.resolvedStyle.backgroundColor, sheet.Run.Family + ": the line in the family's colour");
                Assert.Greater(opener.Line.worldBound.width, sheet.Root.worldBound.width * 0.2f, sheet.Run.Family + ": the line runs on to the sheet's edge");
            }
        }

        [UnityTest]
        public IEnumerator EverySheetAndOpener_WearsItsFamilysColours_InBothModes_RepaintedLiveOnASwitch()
        {
            yield return Open("painting");
            foreach (string mode in CardOptions.Modes)
            {
                ThemeController.SetMode(mode);
                yield return null;
                yield return null;
                Assert.IsTrue(ThemeController.TryResolverFor(Scroll, out var theme), "precondition: the card sits under the theme");
                foreach (var sheet in Shown)
                {
                    string token = sheet.Family.ColorToken;
                    Assert.IsTrue(theme.TryColour(SectionSheetRule.GlassOf(token), out var glass), "precondition: the theme's glass for " + token);
                    Assert.IsTrue(theme.TryColour(token, out var fill), "precondition: the theme's " + token);
                    Assert.IsTrue(theme.TryColour(SectionSheetRule.InkOf(token), out var ink), "precondition: the theme's ink for " + token);
                    Assert.AreEqual(glass, sheet.Root.resolvedStyle.backgroundColor, mode + " " + sheet.Run.Family + ": the sheet is its family's glass");
                    Assert.Less(glass.a, 1f, mode + ": translucent, the camera stays present (D8)");
                    Assert.AreEqual(fill, sheet.Opener.Pill.resolvedStyle.backgroundColor, mode + " " + sheet.Run.Family + ": the pill is filled");
                    Assert.AreEqual(ink, sheet.Opener.Name.resolvedStyle.color, mode + " " + sheet.Run.Family + ": the ink on it");
                }
            }
        }

        [UnityTest]
        public IEnumerator TheOpener_IsNotSticky_ItScrollsAwayWithItsSheet()
        {
            yield return Open("painting");
            var second = Shown[1];
            Scroll.scrollOffset = new Vector2(0f, Stack.TopInScroll(second.Root) + second.Root.layout.height * 0.5f);
            yield return CardTestInput.Settle();
            Assert.Less(second.Opener.Root.worldBound.yMax, Scroll.contentViewport.worldBound.yMin,
                "scrolled into its sheet, the opener has gone up with it: the nav strip marks the section, never a second sticky band");
        }

        // ---------------- navigation ----------------

        [UnityTest]
        public IEnumerator AJumpToASection_LandsOnItsSheetsTop_TheOpenerInView_AndTheSpyNamesIt()
        {
            yield return Open("painting");
            var model = Sheet.Nav.Model;
            int checkedSections = 0;
            for (int i = 0; i < model.Sections.Count; i++)
            {
                var slot = Stack.SlotOfBlock(model.Sections[i].Targets[0]);
                var sheet = Stack.Sheets.SheetOf(slot);
                if (sheet == null || !Stack.Sheets.IsFirstShown(slot)) continue;
                Scroll.scrollOffset = Vector2.zero;
                yield return null;
                Assert.IsTrue(Sheet.Nav.JumpTo(i, "test"), "jump to " + model.Sections[i].Key);
                for (int frame = 0; frame < 600 && Sheet.Nav.IsJumping; frame++) yield return null;
                yield return CardTestInput.Settle(0.2f);
                if (Stack.TopInScroll(sheet.Root) > Range - OnePixel) continue;
                Assert.AreEqual(Scroll.contentViewport.worldBound.yMin + Card.Sheet.Stack.LandingInset, sheet.Root.worldBound.yMin, OnePixel, model.Sections[i].Key + ": the sheet's top meets the viewport's top");
                Assert.IsTrue(Scroll.contentViewport.worldBound.Contains(sheet.Opener.Pill.worldBound.center), model.Sections[i].Key + ": its opener is in view");
                Assert.AreEqual(i, Sheet.Nav.Current, model.Sections[i].Key + ": the spy names the section landed on");
                checkedSections++;
            }
            Assert.Greater(checkedSections, 3, "precondition: several sections landed unclamped");
        }

        // ---------------- render ----------------

        // The opener's words on the RENDER: around its name and meta, the pill's worst pixel against the words' colour >= 7:1, in both modes
        [UnityTest]
        public IEnumerator TheOpenersWords_ClearSevenToOne_OnTheRender_InBothModes([Values("light", "dark")] string mode)
        {
            yield return Open("painting");
            CardTestInput.HoldTheWallStill(Sheet);
            ThemeController.SetMode(mode);
            Scroll.scrollOffset = new Vector2(0f, Stack.TopInScroll(Shown[1].Root) - 200f);
            yield return CardTestInput.Settle();
            yield return new WaitForEndOfFrame();
            var shot = ScreenCapture.CaptureScreenshotAsTexture();
            try
            {
                var viewport = Scroll.contentViewport.worldBound;
                var inView = Shown.Where(s => viewport.Contains(s.Opener.Pill.worldBound.min) && viewport.Contains(s.Opener.Pill.worldBound.max)).ToList();
                Assert.Greater(inView.Count, 0, "precondition: an opener is in view to measure");
                float worst = float.MaxValue; string at = "";
                foreach (var sheet in inView)
                    foreach (var words in new[] { sheet.Opener.Name, sheet.Opener.Meta })
                    {
                        var box = words.worldBound;
                        var pill = sheet.Opener.Pill.worldBound;
                        // - the pill's ground right around the words (above, below, before, after): where their letters' edges sit
                        foreach (var p in new[] { new Vector2(box.center.x, Mathf.Lerp(pill.yMin, box.yMin, 0.5f)), new Vector2(box.center.x, Mathf.Lerp(box.yMax, pill.yMax, 0.5f)),
                                     new Vector2(box.xMin - 2f, box.center.y), new Vector2(box.xMax + 2f, box.center.y) })
                        {
                            float ratio = UIAccessibility.ContrastRatio(PixelAt(shot, p), words.resolvedStyle.color);
                            if (ratio < worst) { worst = ratio; at = sheet.Run.Family + " " + words.name + " at " + p + " " + (Color32)PixelAt(shot, p); }
                        }
                    }
                File.WriteAllBytes(TestEvidence.PathFor("Sections", "opener_render_" + mode + ".png"), shot.EncodeToPNG());
                File.AppendAllText(TestEvidence.PathFor("Sections", "opener_contrast.txt"), $"{mode}: worst opener words {worst:0.00}:1 -- {at}\n");
                Assert.GreaterOrEqual(worst, UIAccessibility.MinRatioEnhancedText, mode + ": the opener's words on its pill, worst point -- " + at);
            }
            finally { Object.Destroy(shot); }
        }

        // For the eye: The Lamp and The Painting, light and dark, Half and Full, two scroll positions across a section boundary; the pages layout
        [UnityTest]
        public IEnumerator TheChapters_AreRendered_ForTheEye([Values("lamp", "painting")] string poi)
        {
            foreach (string mode in CardOptions.Modes)
            {
                yield return Open(poi, SheetStopRule.Stop.Half);
                CardTestInput.HoldTheWallStill(Sheet);
                ThemeController.SetMode(mode);
                var boundary = Shown[1].Root;
                Scroll.scrollOffset = new Vector2(0f, Stack.TopInScroll(boundary) - 120f);
                yield return CardTestInput.Settle();
                yield return Grab($"{poi}_{mode}_half_boundary");
                Sheet.SetStop(SheetStopRule.Stop.Full);
                yield return CardTestInput.Settle();
                Scroll.scrollOffset = new Vector2(0f, Stack.TopInScroll(boundary) - 400f);
                yield return CardTestInput.Settle();
                yield return Grab($"{poi}_{mode}_full_before_boundary");
                Scroll.scrollOffset = new Vector2(0f, Stack.TopInScroll(Shown[2].Root) - 60f);
                yield return CardTestInput.Settle();
                yield return Grab($"{poi}_{mode}_full_next_boundary");
            }
            yield return UseLayout(CardOptions.LayoutPages);
            yield return Open(poi);
            CardTestInput.HoldTheWallStill(Sheet);
            Scroll.scrollOffset = new Vector2(0f, Stack.TopInScroll(Shown[0].Root) - 40f);
            yield return CardTestInput.Settle();
            yield return Grab($"{poi}_light_pages");
        }
    }
}
