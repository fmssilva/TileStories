using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // The scroll-spy under an INTERLEAVED block order (_3.3 R10.7, P4.4c1b) on the REAL LivingRoomScene with real input. The framework's
    // default order (authored, the Sources last) interleaves The Lamp's families -- Visit before Media and back after it, Media back after
    // Listen -- and The Painting's (its fun fact, About, after Media). Every element that marks a section marks the section of the RUN being
    // read (consecutive targets of one section, read from the screen by CardTestInput.SectionUnderTheSpyLine): real drags down the whole
    // Lamp with three elements bound; the end names the last run's family, not Listen (the P1b TODO); a real tab tap lands on the FIRST
    // sheet of its family. Grouped by family nothing moves: at every offset of the range the mark is the old per-section reading's.
    public class PoiCardInterleavedSpySceneTests : SearchSceneFixture
    {
        private const string Lamp = "lamp";
        private const string Painting = "painting";

        private PoiCardSheetView Sheet => Card.Sheet;
        private NavController Nav => Sheet.Nav;
        private CardNavModel Model => Nav.Model;
        private BlockStackView Stack => Sheet.Stack;
        private ScrollView Scroll => Stack.Scroll;
        private TextTabsNavView Tabs => Sheet.Slots.TopView as TextTabsNavView;
        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;
        private float Range => NavScrollRule.Range(Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height);

        private IEnumerator Push(System.Action<WallConfigData> edit)
        {
            var copy = ConfigCopy();
            edit(copy);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        private IEnumerator Open(string poi)
        {
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            SelectionEventBus.Select(poi);
            yield return CardTestInput.Settle();
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            Assert.AreEqual(poi, Card.ShownPoiId, "precondition: " + poi + "'s card is shown");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "precondition: at Full");
        }

        private IEnumerator Landed()
        {
            for (int frame = 0; frame < 600 && Nav.IsJumping; frame++) yield return null;
            Assert.IsFalse(Nav.IsJumping, "the jump came to rest");
            yield return CardTestInput.Settle(0.3f);
        }

        // The section the reader is in, read from the SCREEN (the runs cut by hand, the spy's own rule over where their first slots are drawn)
        private int OnScreen() => CardTestInput.SectionUnderTheSpyLine(Scroll, Model, Stack.SlotOfBlock);

        // The section of the card's last target: the last run's (cut by hand from the model's targets, never Model.Runs)
        private int LastRunsSection() => Model.SectionOf(Model.Targets[Model.Targets.Count - 1]);

        // Whether the stack rests at its end: the end of the range with the bottom area shown -- the area that comes back at the end keeps
        // the end in view (CardChromeRule.KeepTheEnd), so that is where a visitor's drags rest
        private bool AtTheEnd() => Scroll.scrollOffset.y >= Range - OnePixel;

        private string KeyOf(int section) => section >= 0 && section < Model.Sections.Count ? Model.Sections[section].Key : "(none)";

        // Every element bound marks `section`: the spy itself, the tabs' underline, the bottom bar's tab, the edge scrubber's disc
        private void AssertEveryElementMarks(int section, string when)
        {
            string key = KeyOf(section);
            Assert.AreEqual(section, Nav.Current, when + ": the spy names " + key + ", the section of the run on screen");
            Assert.AreEqual(section, Tabs.CurrentIndex, when + ": the tabs mark " + key);
            var bar = Sheet.Slots.BottomView as BottomBarNavView;
            Assert.AreEqual(section, bar.CurrentIndex, when + ": the bottom bar marks " + key);
            var edge = Sheet.Slots.EdgeView as EdgeScrubberNavView;
            var marked = edge.Items.Where(i => i.Button.ClassListContains(EdgeScrubberNavView.CurrentClass)).ToList();
            Assert.AreEqual(1, marked.Count, when + ": the scrubber marks one disc");
            Assert.AreEqual(section, edge.Items.ToList().IndexOf(marked[0]), when + ": the scrubber marks " + key);
        }

        // A tab wholly inside the strip's visible part, after a real sideways drag when it is not
        private IEnumerator BringIntoView(NavTab tab)
        {
            var visible = Tabs.Viewport.worldBound;
            var rect = tab.Button.worldBound;
            if (rect.xMin < visible.xMin - 0.5f || rect.xMax > visible.xMax + 0.5f)
            {
                yield return CardTestInput.TouchDrag(Sheet.Root.panel, visible.center, new Vector2(visible.center.x - rect.center.x, 0f));
                yield return CardTestInput.Settle(0.3f);
            }
            rect = tab.Button.worldBound;
            Assert.IsTrue(rect.xMin >= visible.xMin - 0.5f && rect.xMax <= visible.xMax + 0.5f, "precondition: tab " + tab.Index + " is inside the strip");
        }

        // ---------------- interleaved: The Lamp at the framework's default order ----------------

        // Real drags down the whole Lamp, Top Bar + Bottom Bar + Edge Scrubber bound: after every drag each element marks the section of the run
        // on screen; the marks go Visit -> Media -> Visit and Media comes back after Listen (the old reading kept Media over the second Visit
        // run and Listen to the end); at the end the last run's family is marked, not Listen
        [UnityTest]
        public IEnumerator TheLamp_RealDragsDownTheWholeCard_EveryElementMarksTheFamilyOfTheRunBeingRead()
        {
            yield return Push(c =>
            {
                c.card_settings.navigation.block_order = CardOptions.BlockOrderEndFamiliesLast;
                c.card_settings.navigation.top = BuiltInNavElements.TextTabs;
                c.card_settings.navigation.bottom = BuiltInNavElements.BottomBar;
                c.card_settings.navigation.edge = BuiltInNavElements.EdgeScrubber;
            });
            yield return Open(Lamp);
            Assert.IsNotNull(Tabs, "precondition: the tabs are bound");
            Assert.IsInstanceOf<BottomBarNavView>(Sheet.Slots.BottomView, "precondition: the bottom bar is bound");
            Assert.IsInstanceOf<EdgeScrubberNavView>(Sheet.Slots.EdgeView, "precondition: the edge scrubber is bound");
            Assert.Greater(Range, Scroll.contentViewport.layout.height * 10f, "precondition: The Lamp scrolls many screens (" + Range + ")");
            AssertEveryElementMarks(OnScreen(), "at the top");

            var marks = new List<string> { KeyOf(Nav.Current) };
            int drags = 0, stillAtTheEnd = 0;
            // - until the card's end is in view and the stack stops moving
            while (stillAtTheEnd < 3 && drags < 200)
            {
                // - measured before every drag: near the end the footer and the bottom bar come back and the viewport shrinks, so a finger
                //   placed by the first screen's viewport would land on the bar, not on the stack
                var viewport = Scroll.contentViewport.worldBound;
                float before = Scroll.scrollOffset.y;
                yield return CardTestInput.TouchDrag(Scroll.panel, new Vector2(viewport.center.x, viewport.yMax - 40f), new Vector2(0f, -viewport.height * 0.2f));
                yield return CardTestInput.Settle(0.5f);
                drags++;
                if (AtTheEnd()) stillAtTheEnd += Scroll.scrollOffset.y > before + OnePixel ? 0 : 1;
                else Assert.Greater(Scroll.scrollOffset.y, before, "precondition: drag " + drags + " scrolled the stack (range " + Range.ToString("F1")
                    + ", content " + Scroll.contentContainer.layout.height.ToString("F1") + ", viewport " + Scroll.contentViewport.layout.height.ToString("F1") + ")");
                int onScreen = OnScreen();
                AssertEveryElementMarks(onScreen, "drag " + drags + " at " + Scroll.scrollOffset.y.ToString("F0"));
                if (marks[marks.Count - 1] != KeyOf(onScreen)) marks.Add(KeyOf(onScreen));
            }
            string walk = string.Join(" > ", marks);
            Debug.Log("[Card] interleaved Lamp, " + drags + " real drags, the marks: " + walk);
            Assert.IsTrue(AtTheEnd(), "precondition: the drags reached the card's end (" + walk + ")");

            // - the comebacks, in the order the visitor meets them
            int media = marks.IndexOf("media");
            Assert.Greater(media, 0, "Media was marked: " + walk);
            Assert.AreEqual("visit", marks[media - 1], "Visit (its practical info) marked before Media: " + walk);
            Assert.AreEqual("visit", marks[media + 1], "and Visit marked AGAIN after Media -- its wall locator's run: " + walk);
            int listen = marks.IndexOf("guides");
            Assert.Greater(listen, media, "Listen marked after Media: " + walk);
            Assert.Contains("media", marks.Skip(listen + 1).ToList(), "Media marked again after Listen -- its 3D models' run: " + walk);
            Assert.AreEqual(KeyOf(LastRunsSection()), marks[marks.Count - 1], "at the end: the last run's family: " + walk);
            Assert.AreNotEqual("guides", marks[marks.Count - 1], "not Listen kept to the end (the P1b TODO): " + walk);
        }

        // A real tap on a family's tab lands on that family's FIRST sheet -- not a later run of it -- with its opener in view, and marks it
        [UnityTest]
        public IEnumerator TheLamp_ARealTabTap_LandsOnTheFirstSheetOfItsFamily_AndMarksIt([Values("visit", "media")] string family)
        {
            yield return Push(c => c.card_settings.navigation.block_order = CardOptions.BlockOrderEndFamiliesLast);
            yield return Open(Lamp);
            int section = Enumerable.Range(0, Model.Sections.Count).Single(s => Model.Sections[s].Key == family);
            var sheets = Stack.Sheets.All.Where(s => s.Run.Family == family && s.Look.Shown).ToList();
            Assert.GreaterOrEqual(sheets.Count, 2, "precondition: " + family + " has two sheets on the interleaved Lamp");

            // - start past the family's LAST sheet, so a jump to a later run would be the nearer one
            Scroll.scrollOffset = new Vector2(0f, Mathf.Min(Range, Stack.TopInScroll(sheets.Last().Root) + 200f));
            yield return CardTestInput.Settle(0.3f);
            var tab = Tabs.Tabs[section];
            yield return BringIntoView(tab);
            yield return CardTestInput.TouchDrag(Sheet.Root.panel, tab.Button.worldBound.center, Vector2.zero, frames: 1);
            yield return Landed();

            var first = sheets[0].Root;
            Assert.AreEqual(Scroll.contentViewport.worldBound.yMin + Sheet.Stack.LandingInset, first.worldBound.yMin, OnePixel, family + ": the FIRST sheet's top meets the viewport's top");
            Assert.IsTrue(Scroll.contentViewport.worldBound.Contains(sheets[0].Opener.Pill.worldBound.center), family + ": its opener in view");
            Assert.AreEqual(section, Nav.Current, family + ": marked once landed");
            Assert.AreEqual(section, OnScreen(), family + ": the section on screen");
            Assert.AreEqual(section, Tabs.CurrentIndex, family + ": its tab marked");
        }

        // The Painting at the default order: its fun fact (About) sits after the pictures -- landed on it, About is marked, not Media
        [UnityTest]
        public IEnumerator ThePainting_LandedOnItsFunFact_MarksAbout_NotMedia()
        {
            yield return Push(c => c.card_settings.navigation.block_order = CardOptions.BlockOrderEndFamiliesLast);
            yield return Open(Painting);
            Assert.AreEqual("about", KeyOf(Model.SectionOf("block_6")), "precondition: the fun fact is About's");
            Assert.AreEqual("media", KeyOf(Model.SectionOf(Model.Targets[Model.Targets.ToList().IndexOf("block_6") - 1])), "precondition: right after a Media block");
            Assert.IsTrue(Nav.JumpToBlock("block_6", "test"), "jump to the fun fact");
            yield return Landed();
            Assert.LessOrEqual(Stack.LandingTop(Stack.SlotOfBlock("block_6")), Range - OnePixel, "precondition: it landed unclamped, at the viewport's top");
            Assert.AreEqual("about", KeyOf(OnScreen()), "the run on screen is About's");
            Assert.AreEqual("about", KeyOf(Nav.Current), "released from the jump, the spy names About");
            Assert.AreEqual(Nav.Current, Tabs.CurrentIndex, "the tabs mark it");
        }

        // ---------------- for the eye ----------------

        [UnityTearDown]
        public IEnumerator RestoreTheMode()
        {
            ThemeController.SetMode("");
            yield return null;
        }

        // For the eye, light and dark: The Lamp (default order) with each of Visit -> Media -> Visit -> ... -> Media's sheets just past the spy
        // line -- the tabs mark that sheet's family at each step; The Painting by family across a boundary; The Painting in pages and in tabs
        [UnityTest]
        public IEnumerator TheMarks_AreRendered_ForTheEye()
        {
            foreach (string mode in CardOptions.Modes)
            {
                yield return Push(c => c.card_settings.navigation.block_order = CardOptions.BlockOrderEndFamiliesLast);
                yield return Open(Lamp);
                CardTestInput.HoldTheWallStill(Sheet);
                ThemeController.SetMode(mode);
                var steps = new[] { ("visit", "block_28"), ("media", "block_29"), ("visit", "block_37"), ("media", "block_58") };
                for (int i = 0; i < steps.Length; i++)
                {
                    var (family, block) = steps[i];
                    var sheet = Stack.Sheets.SheetOf(Stack.SlotOfBlock(block));
                    Assert.AreEqual(family, sheet.Run.Family, "precondition: " + block + " opens a " + family + " sheet");
                    // - the sheet's top a little past the spy line: its run is the one being read, the run before still above it on screen
                    float spy = Scroll.contentViewport.layout.height * NavScrollRule.SpyLineFraction;
                    Scroll.scrollOffset = new Vector2(0f, Stack.TopInScroll(sheet.Root) - spy + 60f);
                    yield return CardTestInput.Settle();
                    Assert.AreEqual(family, KeyOf(OnScreen()), mode + " step " + i + ": the run on screen is " + family + "'s");
                    Assert.AreEqual(family, KeyOf(Tabs.CurrentIndex), mode + " step " + i + ": the tabs mark " + family);
                    yield return Grab($"lamp_{mode}_{i + 1}_{family}");
                }

                yield return Push(c => c.card_settings.navigation.block_order = CardOptions.BlockOrderByFamily);
                yield return Open(Painting);
                CardTestInput.HoldTheWallStill(Sheet);
                var second = Stack.Sheets.All.Where(s => s.Look.Shown).ElementAt(2);
                Scroll.scrollOffset = new Vector2(0f, Stack.TopInScroll(second.Root) - Scroll.contentViewport.layout.height * NavScrollRule.SpyLineFraction + 60f);
                yield return CardTestInput.Settle();
                Assert.AreEqual(second.Run.Family, KeyOf(Tabs.CurrentIndex), mode + ": by family, the tabs mark the sheet just read");
                yield return Grab($"painting_by_family_{mode}");
            }
            ThemeController.SetMode("light");
            foreach (string layout in new[] { CardOptions.LayoutPages, CardOptions.LayoutTabs })
            {
                yield return Push(c => c.card_settings.navigation.layout = layout);
                yield return Open(Painting);
                CardTestInput.HoldTheWallStill(Sheet);
                yield return Grab($"painting_{layout}_first");
                Assert.IsTrue(Nav.JumpTo(1, "test", landsAtOnce: true), "to the second " + layout);
                yield return Landed();
                yield return Grab($"painting_{layout}_second");
            }
        }

        private static IEnumerator Grab(string name)
        {
            yield return new WaitForEndOfFrame();
            var shot = ScreenCapture.CaptureScreenshotAsTexture();
            System.IO.File.WriteAllBytes(TestEvidence.PathFor("Sections/spy", name + ".png"), shot.EncodeToPNG());
            Object.Destroy(shot);
        }

        // ---------------- by family: nothing moves ----------------

        // Grouped by family every section is one run: at EVERY offset of the range (a step of a few pixels) the spy names exactly what the
        // old per-section reading names -- each section's first slot as drawn, the rule as it was -- so nothing moved on a by-family card
        [UnityTest]
        public IEnumerator ByFamily_TheSameMarkAtEveryOffset_AsTheOldPerSectionReading([Values(Lamp, Painting)] string poi)
        {
            yield return Push(c => c.card_settings.navigation.block_order = CardOptions.BlockOrderByFamily);
            yield return Open(poi);
            Assert.Greater(Model.Sections.Count, 3, "precondition: " + poi + " has several sections");
            Assert.Greater(Range, Scroll.contentViewport.layout.height, "precondition: the card scrolls (" + Range + ")");
            float step = 23f;
            int compared = 0;
            var seen = new HashSet<int>();
            for (float offset = 0f; offset <= Range + step; offset += step)
            {
                Scroll.scrollOffset = new Vector2(0f, Mathf.Min(offset, Range));
                yield return null;
                int old = CardTestInput.UnderTheSpyLine(Scroll, Model.Sections.Select(s => Stack.SlotOfBlock(s.Targets[0])));
                Assert.AreEqual(old, Nav.Current, poi + " at " + Scroll.scrollOffset.y.ToString("F0") + ": the old reading's section (the screen's landing inset "
                    + CardTestInput.LandingInsetOf(Scroll) + ", the card's " + Stack.LandingInset + ", the stack's top " + Scroll.layout.yMin + ")");
                Assert.AreEqual(old, OnScreen(), poi + " at " + Scroll.scrollOffset.y.ToString("F0") + ": the runs' reading agrees");
                seen.Add(old);
                compared++;
            }
            Debug.Log("[Card] by family " + poi + ": " + compared + " offsets, " + seen.Count + " sections marked, all as before");
            Assert.AreEqual(Model.Sections.Count, seen.Count, "precondition: the sweep met every section");
        }
    }
}
