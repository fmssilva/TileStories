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
    // SectionSheetSceneTests's shared ground: its set-up and helpers, so its own tests and its old-navigation ones stand on one fixture
    public abstract class SectionSheetSceneFixture : SearchSceneFixture
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
    }

    // The card's chapters on the REAL LivingRoomScene (_3.3 P4.4c1, R11, R10.7; SectionSheets): The Lamp's scroll blocks sit on glass sheets
    // by family -- a new sheet wherever the family changes (its framework-default order interleaves families), the navigation family on the
    // plain ground; each sheet tinted with its family's glass and laid over the one before; the opener (a filled family pill + a line) names
    // the family with its blocks' count and minutes, in both languages, its words on the pill (D8), not sticky; a jump to a section lands
    // on its sheet's top with the opener in view and the spy naming it; the layouts (pages: the opener is the page's title; tabs; accordion;
    // bento); the opener's words >= 7:1 on the RENDER in both modes; captures for the eye.
    [Category("Core")]
    public class SectionSheetSceneTests : SectionSheetSceneFixture
    {
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

    // SectionSheetSceneTests's tests whose subject is an old navigation element: they wait for its rebuild on the Navigation Bar's pattern
    [Ignore(TestSkips.OldNavigationElements)]
    public class SectionSheetSceneOldNavigationTests : SectionSheetSceneFixture
    {
        // ---------------- layouts ----------------

        // The opener in each layout (_3.3 R16, decided at the P4.4c1 review): pages -- the opener IS the page's title and the Family Pager keeps
        // only its counter ("1 of 6"), a real chevron tap turns to the next page titled by ITS opener; tabs -- the sheet stays, the opener says no
        // words (the strip names the section): no name, no meta, no pill -- the family's coloured line alone, the sheet's full width (the 4c1b
        // review: an icon-only pill read as a stray badge); accordion and bento as P4.4c1 left them
        [UnityTest]
        public IEnumerator Pages_TheOpenerIsThePagesTitle_ThePagerKeepsItsCounter_TabsOpenerSaysNoWords_AccordionAndBentoAsBefore()
        {
            var words = new CardStrings(Card.StringTable.Entries(), CardStringSources.Shared.Entries(), null, "en", "en");
            yield return UseLayout(CardOptions.LayoutPages);
            yield return Open("painting");
            int count = Sheet.Nav.Model.Sections.Count;
            var pager = Sheet.Slots.TopView as FamilyPagerNavView;
            Assert.IsNotNull(pager, "precondition: pages draws the Family Pager");
            for (int expected = 0; expected < 2; expected++)
            {
                if (expected > 0)
                {
                    yield return CardTestInput.Tap(pager.Next.panel, pager.Next.worldBound.center);
                    for (int frame = 0; frame < 600 && Sheet.Nav.IsJumping; frame++) yield return null;
                    yield return CardTestInput.Settle(0.3f);
                }
                Assert.AreEqual(expected, Sheet.Nav.Current, "precondition: page " + (expected + 1));
                var shown = Shown;
                var page = Sheet.Nav.Model.Sections[expected];
                Assert.AreEqual(page.Key, shown[0].Run.Family, "pages: the page's first sheet is its own section's");
                Assert.IsTrue(shown[0].Look.Opener, "pages: the page opens with its opener");
                Assert.AreEqual(words.Get(page.LabelKey), shown[0].Opener.Name.text, page.Key + ": the opener names the page -- its title");
                Assert.AreEqual(DisplayStyle.Flex, shown[0].Opener.Name.resolvedStyle.display, page.Key + ": and shows it");
                Assert.AreEqual(DisplayStyle.None, pager.NameRow.resolvedStyle.display, page.Key + ": the pager says no name (no word said twice)");
                Assert.AreEqual(string.Format(words.Get(CardStrings.Keys.NavPagerPosition), expected + 1, count), pager.Position.text, page.Key + ": the pager keeps its counter");
                Assert.AreEqual(DisplayStyle.Flex, pager.Position.resolvedStyle.display, page.Key + ": shown");
                // - what else shows on a page is no navigation target (the layout sets aside only targets: the Sources stay on every page)
                foreach (var other in shown.Where(s => s.Run.Family != page.Key))
                    Assert.IsTrue(other.Slots.All(slot => Sheet.Nav.Model.Targets.All(t => Stack.SlotOfBlock(t) != slot)),
                        other.Run.Family + ": a sheet of another family on the page holds no target");
            }

            yield return UseLayout(CardOptions.LayoutTabs);
            yield return Open("painting");
            Assert.AreEqual(Sheet.Nav.Model.Sections[Sheet.Nav.Current].Key, Shown[0].Run.Family, "tabs: the sheet shown first is the tab's own section");
            Assert.IsTrue(Shown.Where(s => s.Run.Family != Shown[0].Run.Family).All(s => s.Slots.All(slot => Sheet.Nav.Model.Targets.All(t => Stack.SlotOfBlock(t) != slot))),
                "tabs: any other sheet holds no target (the Sources stay on every tab)");
            Assert.IsTrue(ThemeController.TryResolverFor(Scroll, out var theme), "precondition: the card sits under the theme");
            foreach (var sheet in Shown.Where(s => s.Look.Opener))
            {
                string family = sheet.Run.Family;
                Assert.AreEqual(DisplayStyle.None, sheet.Opener.Name.resolvedStyle.display, "tabs " + family + ": the opener says no name (the strip does)");
                Assert.AreEqual(DisplayStyle.None, sheet.Opener.Meta.resolvedStyle.display, "tabs " + family + ": and no count");
                Assert.AreEqual(DisplayStyle.Flex, sheet.Opener.Root.resolvedStyle.display, "tabs " + family + ": the opener itself stays");
                Assert.IsTrue(theme.TryColour(sheet.Family.ColorToken, out var fill), "precondition: the fill");
                Assert.IsTrue(theme.TryColour(SectionSheetRule.GlassOf(sheet.Family.ColorToken), out var glass), "precondition: the glass");
                // - 4c1b review: an icon alone on a pill read as a stray badge -- the opener is the family's coloured line alone
                Assert.AreEqual(DisplayStyle.None, sheet.Opener.Pill.resolvedStyle.display, "tabs " + family + ": no pill (no icon-only badge)");
                Assert.AreEqual(fill, sheet.Opener.Line.resolvedStyle.backgroundColor, "tabs " + family + ": its line, in the family's colour");
                float inner = sheet.Root.worldBound.xMin + sheet.Root.resolvedStyle.paddingLeft;
                Assert.AreEqual(inner, sheet.Opener.Line.worldBound.xMin, CardGalleryChecks.OnePixel(sheet.Root), "tabs " + family + ": the line starts at the sheet's padding");
                Assert.AreEqual(sheet.Root.worldBound.xMax - sheet.Root.resolvedStyle.paddingRight, sheet.Opener.Line.worldBound.xMax, CardGalleryChecks.OnePixel(sheet.Root),
                    "tabs " + family + ": and runs its full width");
                Assert.AreEqual(glass, sheet.Root.resolvedStyle.backgroundColor, "tabs " + family + ": the sheet stays, in its glass");
            }

            yield return UseLayout(CardOptions.LayoutAccordion);
            yield return Open("painting");
            Assert.Greater(Shown.Count, 4, "accordion: the rows sit on their family's sheets");
            var rows = Scroll.contentContainer.Query(className: BlockStackView.CompactSlotClass).ToList();
            Assert.Greater(rows.Count, 4, "precondition: the accordion draws the targets as compact rows");
            Assert.IsTrue(rows.All(row => Stack.Sheets.SheetOf(row) != null), "every compact row sits on its family's sheet");
            Assert.IsTrue(Shown.All(s => s.Look.Opener || s.Look.Continues), "each family's rows open with its opener");
            foreach (var sheet in Shown.Where(s => s.Look.Opener))
                Assert.AreEqual(words.Get(sheet.Family.LabelKey), sheet.Opener.Name.text, "accordion " + sheet.Run.Family + ": the opener names its family, as before");

            yield return UseLayout(CardOptions.LayoutBento);
            yield return Open("painting");
            Assert.IsTrue(Shown.All(s => s.Slots.All(slot => Sheet.Nav.Model.Targets.All(t => Stack.SlotOfBlock(t) != slot || slot.resolvedStyle.display == DisplayStyle.None))),
                "bento: every target is set aside for the tiles -- a sheet left in the stack holds no target (the Sources under the tiles)");
        }

        // The width the blocks had before the sheets: the frame's 16 pt inset (the sheet's padding now), the 1 px sheet edge drawn as a part
        [UnityTest]
        public IEnumerator ABlockOnASheet_KeepsTheFramesWidth_AndTheNavigationFamilyIsABareSheet()
        {
            var copy = ConfigCopy();
            copy.card_settings.navigation.auto_nav_block = BuiltInBlocks.NavIconGridKind;
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
            yield return Open("lamp");
            float inset = CardTestInput.TokenPx("--ts-space-4");
            float width = Scroll.contentContainer.layout.width - 2f * inset;
            // - a block that waits for the card to be read is hidden on this test's fresh visitor state: the shown ones are measured
            var slots = Scroll.contentContainer.Query(className: BlockFrame.FramedClass).ToList();
            Assert.AreEqual(BlocksWaitingToBeRead("lamp"), slots.Count(s => s.resolvedStyle.display == DisplayStyle.None),
                "precondition: only the blocks that wait to be read are hidden");
            foreach (var slot in slots.Where(s => s.resolvedStyle.display != DisplayStyle.None))
                Assert.AreEqual(width, slot.layout.width, 0.01f, slot.name + ": a block keeps the width the frame's inset gave it");
            var bare = Stack.Sheets.All.Where(s => s.Run.Bare).ToList();
            Assert.AreEqual(1, bare.Count, "precondition: the icon grid is the one navigation run");
            Assert.AreEqual(DisplayStyle.None, bare[0].Opener.Root.resolvedStyle.display, "a nav run has no opener");
            Assert.AreEqual(0f, bare[0].Root.resolvedStyle.backgroundColor.a, "and no glass: the plain ground");
            Assert.AreEqual(0, Scroll.contentContainer.IndexOf(Stack.HeaderView.HeroPart), "the hero still opens the scroll, the glass starts below it (R11 v4)");
        }
    }
}
