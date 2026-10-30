using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // PoiCardLayoutSceneTests's shared ground: its set-up and helpers, so its own tests and its old-navigation ones stand on one fixture
    public abstract class PoiCardLayoutSceneFixture : SearchSceneFixture
    {
        protected const string Lamp = "lamp";

        protected const string LampMilitary = "lamp_military";

        protected PoiCardSheetView Sheet => Card.Sheet;

        protected BlockStackView Stack => Sheet.Stack;

        protected NavController Nav => Sheet.Nav;

        protected ScrollView Scroll => Stack.Scroll;

        protected FamilyPagerNavView Pager => Sheet.Slots.TopView as FamilyPagerNavView;

        protected float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;

        protected float Range => NavScrollRule.Range(Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height);

        protected ManualVideoOutput _videoOut;

        protected CardVideoService _video;

        protected BackgroundSafeInput _backgroundInput;

        protected Keyboard _keyboard;

        [UnitySetUp]
        public IEnumerator UseAStandInPictureDevice()
        {
            _videoOut = new ManualVideoOutput();
            _video = new CardVideoService(_videoOut, () => Card.Media);
            Card.UseVideoService(_video);
            // - the language chip's pick is remembered in this throwaway state, never in the developer's own PlayerPrefs
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            yield break;
        }

        [UnityTearDown]
        public IEnumerator LetTheStandInsGo()
        {
            _video.CardClosed();
            _videoOut.Release();
            if (_keyboard != null) InputSystem.RemoveDevice(_keyboard);
            _keyboard = null;
            _backgroundInput?.Dispose();
            _backgroundInput = null;
            yield break;
        }

        // ---------------- helpers ----------------

        // The wall's navigation edited and pushed through the live seam, as a POI Editor row does
        protected IEnumerator UseNavigation(Action<CardNavigationSettings> edit)
        {
            var copy = ConfigCopy();
            edit(copy.card_settings.navigation);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        protected IEnumerator OpenFull(string poi)
        {
            if (SelectionEventBus.CurrentPoiId != poi)
            {
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
                SelectionEventBus.Select(poi);
                yield return CardTestInput.Settle();
            }
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "precondition: the card rests at full");
            float rest = Sheet.Stops.HeightOf(Sheet.Stop);
            for (int frame = 0; frame < 600 && Mathf.Abs(Sheet.Root.layout.height - rest) > OnePixel; frame++) yield return null;
            Assert.AreEqual(rest, Sheet.Root.layout.height, OnePixel, "precondition: the sheet rests at its full height");
        }

        // `target` in the card's view and still for three frames (a long scroll collapses the header for a few frames after it)
        protected IEnumerator InViewAndStill(VisualElement target)
        {
            Rect last = default;
            int still = 0;
            // - "in view" = where a finger reaches it: not under the floating header or strip (_3.3 S4a), not under the bottom area
            for (int frame = 0; frame < 240 && still < 3; frame++)
            {
                var reach = CardTestInput.ReachableStack(Sheet);
                if (!reach.Contains(target.worldBound.center))
                    Scroll.scrollOffset = new Vector2(0f, Scroll.scrollOffset.y + target.worldBound.center.y - reach.center.y);
                yield return null;
                var now = target.worldBound;
                still = now == last && CardTestInput.ReachableStack(Sheet).Contains(now.center) ? still + 1 : 0;
                last = now;
            }
            Assert.IsTrue(CardTestInput.ReachableStack(Sheet).Contains(target.worldBound.center),
                "precondition: " + target.name + " is inside the reachable part of the card: " + target.worldBound + " in " + CardTestInput.ReachableStack(Sheet));
        }

        // A real tap on a compact row's line, scrolled into view and still first
        protected IEnumerator TapRow(string key)
        {
            var row = Stack.CompactRowOf(key);
            Assert.IsNotNull(row, "precondition: " + key + " is drawn as a compact row");
            yield return InViewAndStill(row.Toggle);
            yield return CardTestInput.Tap(row.Toggle.panel, row.Toggle.worldBound.center);
            yield return CardTestInput.Settle(0.2f);
        }

        // Real taps on the Family Pager's arrows (the tabs layout's own switch while the Navigation Bar is off) until section `index` shows
        protected IEnumerator ShowSection(int index)
        {
            Assert.IsNotNull(Pager, "precondition: the top slot holds the Family Pager");
            // - _3.3 S4a: the strip is chrome and waits for the hero (the screen at Full): a reader taps it once the picture is read away
            if (Sheet.Slots.Top.ClassListContains(CardChromeView.TopHiddenClass)) yield return CardTestInput.PastTheHero(Sheet);
            Assert.IsFalse(Sheet.Slots.Top.ClassListContains(CardChromeView.TopHiddenClass), "precondition: the strip shows (offset "
                + Scroll.scrollOffset.y + ", hero bottom " + Sheet.Stack.HeaderView?.HeroBottom() + ")");
            for (int step = 0; step < Nav.Model.Sections.Count && Pager.CurrentIndex != index; step++)
            {
                var arrow = Pager.CurrentIndex < index ? Pager.Next : Pager.Previous;
                yield return CardTestInput.Tap(arrow.panel, arrow.worldBound.center);
                for (int frame = 0; frame < 600 && Nav.IsJumping; frame++) yield return null;
                Assert.IsFalse(Nav.IsJumping, "the jump came to rest");
                yield return CardTestInput.Settle(0.3f);
            }
            Assert.AreEqual(index, Pager.CurrentIndex, "the pager reached section " + index);
        }

        // A real press and release of Escape on an Input System keyboard (where Android's Back arrives), under BackgroundSafeInput
        protected IEnumerator PressEscape()
        {
            if (_keyboard == null)
            {
                _backgroundInput = new BackgroundSafeInput();
                _keyboard = BackgroundSafeInput.AddEnabled<Keyboard>();
            }
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(Key.Escape));
            yield return null;
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState());
            yield return CardTestInput.Settle(0.3f);
        }

        // A block's top lands at the viewport's top (relative, one pixel), or the card's end clamps it
        protected void AssertLanded(string block, string name)
        {
            var slot = Stack.SlotOfBlock(block);
            if (Sheet.Stack.LandingTop(slot) <= Range - OnePixel)
                Assert.AreEqual(Sheet.Stack.LandingInset, Sheet.Stack.LandingElement(slot).worldBound.yMin - Scroll.contentViewport.worldBound.yMin, OnePixel, name + ": its top meets the viewport's top");
            else
                Assert.AreEqual(Range, Scroll.scrollOffset.y, OnePixel, name + ": a late block clamps at the end of the range");
        }

        protected int SectionIndex(string family) => Nav.Model.Sections.ToList().FindIndex(s => s.Key == family);
    }

    // Phase B of step 13 (_3.2 section 6.1, the accordion and tabs layouts) on the REAL LivingRoomScene with real taps, real drags and a
    // real Escape, and the card's REAL owners (the preview stage loading the real glb; the video owner over a stand-in picture device, as in
    // every video scene test). The layout is pushed through the live seam, as the POI Editor's Layout row does. Asserted on the stack, the
    // controller and the OWNERS, never on pixels:
    //   - accordion: every target of The Lamp is a row holding no view and no file; a real tap opens one (its view and the preview owner's
    //     slot), with Open One At A Time a second tap closes the first (the owner gives its slot back) and the tapped row keeps its place; a
    //     real tap on a tab opens that section's first row and lands it; Escape folds the row opened last before the sheet moves
    //   - tabs: one section at a time, the others set aside (no view, no file); a real tap on a tab shows its section and the spy names it,
    //     a real drag inside never moves it; a section put away lets its media go (the preview slot, the playing video); with the
    //     Navigation Bar off and neither a Top nor a Bottom element the card uses the Family Pager; a card with nothing to switch between
    //     scrolls
    //   - renders at 390 px in English and Portuguese: both layouts
    [Category("Core")]
    public class PoiCardLayoutSceneTests : PoiCardLayoutSceneFixture
    {
        // ---------------- the accordion ----------------

        [UnityTest]
        public IEnumerator Accordion_EveryTargetIsAnEmptyRow_ARealTapOpensOne_OneAtATimeClosesTheOther_AndEscapeFoldsItBeforeTheSheetMoves()
        {
            yield return UseNavigation(nav => { nav.layout = CardOptions.LayoutAccordion; nav.accordion_one_open = true; });
            yield return OpenFull(Lamp);
            var targets = Nav.Model.Targets;
            Assert.AreEqual(56, targets.Count, "precondition: The Lamp's 56 targets");
            foreach (string key in targets)
            {
                Assert.IsNotNull(Stack.CompactRowOf(key), key + ": a row");
                Assert.IsNull(Stack.ViewOfBlock(key), key + ": no view");
                Assert.AreEqual(0, Stack.HeldMediaOf(key), key + ": no file");
            }
            foreach (string whole in new[] { "block_15", "block_16" })
                Assert.IsNotNull(Stack.ViewOfBlock(whole), whole + ": the sources, no target, stay whole");
            CollectionAssert.IsEmpty(Card.PreviewService.HeldKeys, "no 3D / 360 slot held while every row is folded");

            // - a real tap opens the inline model's row: its view and the owner's slot
            yield return TapRow("block_59");
            Assert.IsTrue(Stack.IsExpanded("block_59"), "the real tap opened it");
            CollectionAssert.AreEquivalent(new[] { "block_59" }, Card.PreviewService.HeldKeys, "the preview owner holds the row's slot");

            // - Open One At A Time: a real tap on another row closes the first -- the owner gives its slot back -- and the tapped row stays put
            var textRow = Stack.CompactRowOf("block_2");
            yield return InViewAndStill(textRow.Toggle);
            float before = textRow.Toggle.worldBound.yMin;
            yield return CardTestInput.Tap(textRow.Toggle.panel, textRow.Toggle.worldBound.center);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsTrue(Stack.IsExpanded("block_2"), "the second row opened");
            Assert.IsFalse(Stack.IsExpanded("block_59"), "and the first closed for it");
            CollectionAssert.AreEqual(new[] { "block_2" }, Stack.OpenRows, "exactly one row open");
            CollectionAssert.IsEmpty(Card.PreviewService.HeldKeys, "the closed row's preview slot given back");
            Assert.AreEqual(0, Stack.HeldMediaOf("block_59"), "and its files");
            Assert.AreEqual(before, textRow.Toggle.worldBound.yMin, OnePixel, "the tapped row kept its place on the screen (no auto-scroll)");

            // - Escape: the open row folds, the sheet stays; the next Escape lowers it
            yield return PressEscape();
            Assert.IsFalse(Stack.IsExpanded("block_2"), "Escape folded the open row");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "and the card stayed at full");
            yield return PressEscape();
            Assert.AreEqual(SheetStopRule.Stop.Half, Sheet.Stop, "nothing open: the next Escape lowers the card");
        }

        // ---------------- the tabs layout's own switch ----------------

        [UnityTest]
        public IEnumerator Tabs_WithTheBarOffAndNeitherBar_UsesTheFamilyPager_AndACardWithNothingToSwitchBetweenScrolls()
        {
            yield return UseNavigation(nav =>
            {
                nav.layout = CardOptions.LayoutTabs;
                nav.top = CardOptions.NavNone;
                nav.bottom = CardOptions.NavNone;
                nav.bar.enabled = false;
            });
            yield return OpenFull(Lamp);
            Assert.AreEqual(BuiltInNavElements.FamilyPager, Sheet.Slots.TopKey, "tabs with no bar: the top slot holds the Family Pager");
            Assert.AreEqual(CardOptions.LayoutTabs, Sheet.Layout.Effective);
            int stories = SectionIndex(BuiltInFamilies.Stories);
            Assert.Greater(stories, 0, "precondition: Stories is not the first section, so the pager has to move");
            yield return ShowSection(stories);
            Assert.AreEqual(stories, Sheet.Layout.ShownSection, "and its arrows switch sections");
            foreach (string key in Nav.Model.Sections[stories].Targets) Assert.IsFalse(Stack.IsParked(key), key + " of the shown section is drawn");
            foreach (string key in Nav.Model.Sections[0].Targets) Assert.IsTrue(Stack.IsParked(key), key + " of the first section is set aside");

            yield return OpenFull(LampMilitary);
            Assert.IsFalse(Nav.Model.ShowsNavigation, "precondition: Lamp - Military is under Show Navigation From");
            Assert.AreEqual(CardOptions.LayoutScroll, Sheet.Layout.Effective, "nothing to switch between: it scrolls");
            foreach (string key in Nav.Model.Targets) Assert.IsFalse(Stack.IsParked(key), key + " is not set aside");

            // - live: back to Scroll, every block of The Lamp is back, whole
            yield return OpenFull(Lamp);
            yield return UseNavigation(nav => nav.layout = CardOptions.LayoutScroll);
            foreach (string key in Nav.Model.Targets)
            {
                Assert.IsFalse(Stack.IsParked(key), key + " back");
                Assert.IsNotNull(Stack.ViewOfBlock(key), key + " drawn whole again");
            }
        }
    }

    // PoiCardLayoutSceneTests's tests whose subject is an old navigation element: they wait for its rebuild on the Navigation Bar's pattern
    [Ignore(TestSkips.OldNavigationElements)]
    public class PoiCardLayoutSceneOldNavigationTests : PoiCardLayoutSceneFixture
    {
        [UnityTest]
        public IEnumerator Accordion_ARealTapOnATab_OpensThatSectionsFirstRow_AndLandsIt_AndOpenRowsOpenIndependentlyByDefault()
        {
            yield return UseNavigation(nav => nav.layout = CardOptions.LayoutAccordion);
            yield return OpenFull(Lamp);
            int stories = SectionIndex(BuiltInFamilies.Stories);
            Assert.GreaterOrEqual(stories, 0, "precondition: The Lamp has a Stories section");
            string first = Nav.Model.Sections[stories].Targets[0];
            Assert.IsFalse(Stack.IsExpanded(first), "precondition: its first row is folded");
            yield return ShowSection(stories);
            Assert.IsTrue(Stack.IsExpanded(first), "the tab's jump opened the section's first row");
            Assert.IsNotNull(Stack.ViewOfBlock(first), "its view bound under the row");
            AssertLanded(first, "the opened row");
            Assert.AreEqual(stories, Nav.Current, "the spy names the section");

            // - Open One At A Time off (the default): a second real tap opens another row, the first stays open
            yield return TapRow("block_2");
            CollectionAssert.AreEquivalent(new[] { first, "block_2" }, Stack.OpenRows, "both open: rows open independently");
            yield return PressEscape();
            CollectionAssert.AreEqual(new[] { first }, Stack.OpenRows, "Escape folded the row opened LAST");
        }

        // ---------------- tabs ----------------

        [UnityTest]
        public IEnumerator Tabs_ShowOneSectionAtATime_ARealTabTapShowsAnother_TheSpyNamesIt_AndARealDragInsideNeverMovesIt()
        {
            yield return UseNavigation(nav => nav.layout = CardOptions.LayoutTabs);
            yield return OpenFull(Lamp);
            Assert.AreEqual(CardOptions.LayoutTabs, Sheet.Layout.Effective, "precondition: The Lamp is drawn in tabs");
            var sections = Nav.Model.Sections;
            Assert.AreEqual(0, Sheet.Layout.ShownSection, "the first section shows first");
            void AssertShown(int shown, string when)
            {
                for (int i = 0; i < sections.Count; i++)
                    foreach (string key in sections[i].Targets)
                    {
                        Assert.AreEqual(i != shown, Stack.IsParked(key), when + ": " + key + " (section " + sections[i].Key + ") set aside or not");
                        Assert.AreEqual(i == shown, Stack.ViewOfBlock(key) != null, when + ": " + key + " has a view only while its section shows");
                        Assert.AreEqual(i == shown ? DisplayStyle.Flex : DisplayStyle.None, Stack.SlotOfBlock(key).resolvedStyle.display, when + ": " + key + " drawn or not");
                    }
                foreach (string whole in new[] { "block_15", "block_16" })
                    Assert.AreEqual(DisplayStyle.Flex, Stack.SlotOfBlock(whole).resolvedStyle.display, when + ": the sources show with every section");
            }
            AssertShown(0, "opened");
            Assert.AreEqual(0, Nav.Current);
            Assert.AreEqual(0, Pager.CurrentIndex, "the first tab is marked");

            int media = SectionIndex(BuiltInFamilies.Media);
            yield return ShowSection(media);
            Assert.AreEqual(media, Sheet.Layout.ShownSection, "a real tap on the Media tab shows Media");
            AssertShown(media, "after the tap");
            Assert.AreEqual(media, Nav.Current, "the spy names it");
            Assert.AreEqual(media, Pager.CurrentIndex, "and its tab is marked");
            AssertLanded(sections[media].Targets[0], "Media's first block");

            // - a real drag inside the section: the content scrolls, the spy stays on it
            float offset = Scroll.scrollOffset.y;
            var viewport = Scroll.contentViewport.worldBound;
            yield return CardTestInput.TouchDrag(Sheet.Root.panel, new Vector2(viewport.center.x, viewport.yMin + viewport.height * 0.8f),
                new Vector2(0f, -viewport.height * 0.5f), frames: 20);
            yield return CardTestInput.Settle(0.5f);
            Assert.Greater(Scroll.scrollOffset.y, offset + 10f, "precondition: the real drag scrolled the section");
            Assert.AreEqual(media, Nav.Current, "the spy still names Media: the scroll never leaves the section shown");
            Assert.AreEqual(media, Pager.CurrentIndex);
        }

        [UnityTest]
        public IEnumerator Tabs_ASectionPutAway_LetsItsMediaGo_ThePreviewSlotAndThePlayingVideo()
        {
            yield return UseNavigation(nav => nav.layout = CardOptions.LayoutTabs);
            yield return OpenFull(Lamp);
            int media = SectionIndex(BuiltInFamilies.Media);
            int about = SectionIndex(BuiltInFamilies.About);
            yield return ShowSection(media);
            CollectionAssert.Contains(Card.PreviewService.HeldKeys, "block_59", "Media shows: the inline model holds its preview slot");
            var video = Stack.ViewOfBlock("block_56") as VideoBlockView;
            Assert.IsNotNull(video, "precondition: the inline video is bound while Media shows");
            yield return InViewAndStill(video.Panel.PlayOverlay);
            yield return CardTestInput.Tap(video.Panel.PlayOverlay.panel, video.Panel.PlayOverlay.worldBound.center);
            yield return null;
            Assert.AreEqual(CardVideoState.Playing, _video.State, "a real tap played the video");
            Assert.Greater(Stack.HeldMediaOf("block_56"), 0, "precondition: the video's block holds its files");
            // - read the track BEFORE the section is put away: the view's Unbind clears its own Track, and IsCurrent(null) is always false
            //   (the step 16 fix: this line used to read it after, and asserted nothing)
            var track = video.Track;
            Assert.IsNotNull(track, "precondition: the bound view has its track");
            Assert.IsTrue(_video.IsCurrent(track), "precondition: the owner serves that track");

            yield return ShowSection(about);
            Assert.AreEqual(about, Sheet.Layout.ShownSection, "About shows now");
            Assert.IsNull(Stack.ViewOfBlock("block_56"), "the video's block gave its view back");
            // - section 7.1 (decided at the 15b review): the ONE video owner keeps the paused clip until another video plays or the card closes
            //   -- at most one clip is ever held, and coming back resumes where the visitor left. Proved load-bearing: with Collapsing() calling
            //   the owner's Stop() instead of Pause(), this line turns red
            Assert.IsTrue(_video.IsCurrent(track), "the owner still holds that ONE clip, nothing more (section 7.1)");
            Assert.AreEqual(CardVideoState.Paused, _video.State, "PAUSED: nothing on screen shows it, so it does not play on unseen");
            CollectionAssert.DoesNotContain(Card.PreviewService.HeldKeys, "block_59", "the model's preview slot given back");
            foreach (string key in Nav.Model.Sections[media].Targets)
                Assert.AreEqual(0, Stack.HeldMediaOf(key), key + ": no file held while Media is put away");

            yield return ShowSection(media);
            CollectionAssert.Contains(Card.PreviewService.HeldKeys, "block_59", "back on Media: the slot is taken again");
        }

        // ---------------- what it looks like ----------------

        [UnityTest]
        public IEnumerator Renders_TheLampInEachLayout_InEnglishAndPortuguese()
        {
            foreach (string language in new[] { "en", "pt" })
            {
                yield return UseNavigation(nav => nav.layout = CardOptions.LayoutAccordion);
                yield return OpenFull(Lamp);
                if (language == "pt")
                {
                    yield return CardTestInput.TapNextLanguage(Sheet);
                    yield return CardTestInput.Settle(0.3f);
                }
                Scroll.scrollOffset = Vector2.zero;
                yield return CardTestInput.Settle(0.3f);
                foreach (string key in Nav.Model.Targets)
                {
                    var row = Stack.CompactRowOf(key);
                    if (!row.Root.worldBound.Overlaps(Scroll.contentViewport.worldBound)) continue;
                    Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(row.Toggle.worldBound.width, row.Toggle.worldBound.height), key + ": a whole line to tap");
                    Assert.LessOrEqual(row.Words.worldBound.xMax, row.Chevron.worldBound.xMin + 0.5f, key + ": the words end before the chevron");
                }
                yield return Capture("Layout_Lamp_accordion_" + language);
                yield return TapRow("block_2");
                Scroll.ScrollTo(Stack.SlotOfBlock("block_2"));
                yield return CardTestInput.Settle(0.3f);
                yield return Capture("Layout_Lamp_accordion_open_" + language);

                yield return UseNavigation(nav => nav.layout = CardOptions.LayoutTabs);
                Scroll.scrollOffset = Vector2.zero;
                yield return CardTestInput.Settle(0.3f);
                yield return Capture("Layout_Lamp_tabs_" + language);
                yield return ShowSection(SectionIndex(BuiltInFamilies.Media));
                yield return Capture("Layout_Lamp_tabs_media_" + language);
                if (language == "pt")
                {
                    yield return CardTestInput.TapNextLanguage(Sheet);
                    yield return CardTestInput.Settle(0.3f);
                }
            }
        }
    }
}
