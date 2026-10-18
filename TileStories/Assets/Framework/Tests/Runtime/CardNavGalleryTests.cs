using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace TileStories.Tests
{
    // Phase A of the card's navigation elements (_3.2 section 9, 40-testing.md 4.4): every CardNavGalleryDefinitions entry -- each element
    // over each fixture card (62 targets / 8 + Sources, 6 / 3, one family, the big card in Portuguese) -- on the real sheet in the
    // isolated gallery scene (Assets/Dev/CardNavGallery). Every element, whatever it is: it is the wall's pick in its own slot (top or
    // bottom), it builds
    // over the card's real model, it lies inside the card at 390 px, every button in it is at least the touch token both ways, no word in
    // it is cut, every word reads at 4.5:1; then the element's own promise (ElementChecks) and a render Nav_<key>_<case>.png.
    public class CardNavGalleryTests
    {
        private const string ScenePath = "Assets/Dev/CardNavGallery/CardNavGalleryScene.unity";
        private CardNavGalleryHarness _harness;

        private static string[] EntryNames => CardNavGalleryDefinitions.All.Where(e => !e.IsBlock).Select(e => e.Name).ToArray();

        // What each element promises beyond the shared checks, by key: adding an element = a row in the definitions + its check here
        private static readonly Dictionary<string, System.Action<CardNavGalleryHarness, CardNavGalleryDefinitions.Entry>> ElementChecks = new()
        {
            [BuiltInNavElements.TextTabs] = TextTabsKeepTheirPromise,
            [BuiltInNavElements.IconTabs] = IconTabsKeepTheirPromise,
            [BuiltInNavElements.IconTabsChips] = IconTabsChipsKeepTheirPromise,
            [BuiltInNavElements.KindChips] = KindChipsKeepTheirPromise,
            [BuiltInNavElements.Segmented] = SegmentedKeepsItsPromise,
            [BuiltInNavElements.ProgressMap] = ProgressMapKeepsItsPromise,
            [BuiltInNavElements.BottomBar] = BottomBarKeepsItsPromise,
            [BuiltInNavElements.Dock] = DockKeepsItsPromise,
            [BuiltInNavElements.BarSheet] = BarSheetKeepsItsPromise,
            [BuiltInNavElements.NextUp] = NextUpKeepsItsPromise,
            [BuiltInNavElements.DrawerTree] = DrawerTreeKeepsItsPromise,
            [BuiltInNavElements.GridOverlay] = GridOverlayKeepsItsPromise,
            [BuiltInNavElements.FabDial] = FloatingButtonKeepsItsPromise,
            [BuiltInNavElements.FabRadial] = FloatingButtonKeepsItsPromise,
            [BuiltInNavElements.EdgeScrubber] = EdgeScrubberKeepsItsPromise,
        };

        // The element an entry's slot holds now, its key and its slot element, and whether the card shows it (the chrome for a bar; the
        // overlay owner, open with it as its modal menu, for a menu -- the harness opens it)
        private static INavElementView ViewOf(PoiCardSheetView sheet, CardNavGalleryDefinitions.Entry entry) =>
            CardNavGalleryDefinitions.SlotOf(entry) switch
            {
                NavSlotKeys.Bottom => sheet.Slots.BottomView,
                NavSlotKeys.Menu => sheet.Slots.MenuView,
                NavSlotKeys.Floating => sheet.Slots.FloatingView,
                NavSlotKeys.Edge => sheet.Slots.EdgeView,
                _ => sheet.Slots.TopView,
            };

        private static string KeyOf(PoiCardSheetView sheet, CardNavGalleryDefinitions.Entry entry) =>
            CardNavGalleryDefinitions.SlotOf(entry) switch
            {
                NavSlotKeys.Bottom => sheet.Slots.BottomKey,
                NavSlotKeys.Menu => sheet.Slots.MenuKey,
                NavSlotKeys.Floating => sheet.Slots.FloatingKey,
                NavSlotKeys.Edge => sheet.Slots.EdgeKey,
                _ => sheet.Slots.TopKey,
            };

        private static bool ChromeShows(PoiCardSheetView sheet, CardNavGalleryDefinitions.Entry entry) =>
            CardNavGalleryDefinitions.SlotOf(entry) switch
            {
                NavSlotKeys.Bottom => sheet.Chrome.BottomNavShown,
                NavSlotKeys.Menu => sheet.Overlay.IsModal && sheet.Overlay.OpenKey == entry.ElementKey,
                NavSlotKeys.Floating => sheet.Floating.IsShown,
                NavSlotKeys.Edge => sheet.Slots.EdgeView is EdgeScrubberNavView edge && edge.IsShown,
                _ => sheet.Chrome.TopShown,
            };

        // Every slot but the entry's holds nothing: one element judged alone
        private static IEnumerable<INavElementView> OtherSlots(PoiCardSheetView sheet, CardNavGalleryDefinitions.Entry entry)
        {
            string slot = CardNavGalleryDefinitions.SlotOf(entry);
            if (slot != NavSlotKeys.Top) yield return sheet.Slots.TopView;
            if (slot != NavSlotKeys.Bottom) yield return sheet.Slots.BottomView;
            if (slot != NavSlotKeys.Menu) yield return sheet.Slots.MenuView;
            if (slot != NavSlotKeys.Floating) yield return sheet.Slots.FloatingView;
            if (slot != NavSlotKeys.Edge) yield return sheet.Slots.EdgeView;
        }

        [UnitySetUp]
        public IEnumerator SetUp()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#else
            Assert.Ignore("Needs the Editor to load the gallery scene by path.");
#endif
            for (int i = 0; i < 60 && _harness == null; i++)
            {
                _harness = Object.FindFirstObjectByType<CardNavGalleryHarness>();
                yield return null;
            }
            Assert.IsNotNull(_harness, "the navigation gallery scene holds CardNavGalleryHarness");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            StatusRamp.ResetToDefaults();
            CategoryPalette.ClearOverrides();
            MarkerHierarchyResolver.ResetToDefaults();
            var scene = SceneManager.GetSceneByPath(ScenePath);
            SceneManager.SetActiveScene(SceneManager.CreateScene("AfterCardNavGalleryTest"));
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
        }

        // The list is what section 9 asks: every element over the four fixture cases, the big ones holding exactly 62 targets in eight
        // families and a Sources block, the small one 6 in 3, one with a single family -- and that card with a second family (rule 2.5.7)
        [Test]
        public void TheList_IsEveryElementOverTheFiveFixtureCases()
        {
            CollectionAssert.AreEquivalent(new[] { "big", "small", "one_family", "pt_long", "two_families" }, CardNavGalleryDefinitions.Cases.Select(c => c.Key));
            Assert.AreEqual((CardNavGalleryDefinitions.ElementKeys.Count + CardNavGalleryDefinitions.BlockKinds.Count) * 5, CardNavGalleryDefinitions.All.Count);
            CollectionAssert.Contains(CardNavGalleryDefinitions.BlockKinds, BuiltInBlocks.NavIconGridKind, "the nav blocks are in the list too (step 11a)");
            CollectionAssert.IsSubsetOf(CardNavGalleryDefinitions.ElementKeys, NavElementRegistry.Shared.All.Select(e => e.Key), "every listed element is registered");
            foreach (var (key, targets, sections) in new[] { ("big", 62, 8), ("small", 6, 3), ("one_family", 5, 1), ("pt_long", 62, 8), ("two_families", 6, 2) })
            {
                var entry = CardNavGalleryDefinitions.All.First(e => e.Case.Key == key);
                var wall = CardNavGalleryDefinitions.Wall(entry);
                var stack = BlockStackBuilder.Build(CardNavGalleryDefinitions.Poi(entry), wall.card_settings, BlockRegistry.Shared, wall.pois);
                var model = CardNavModel.Build(stack.Entries, BlockRegistry.Shared, wall.card_settings);
                Assert.IsEmpty(stack.Skipped, key + ": every block of the case is shown (none skipped)");
                Assert.AreEqual(targets, model.TargetCount, key + ": targets");
                Assert.AreEqual(sections, model.Sections.Count, key + ": sections");
                Assert.IsTrue(model.ShowsNavigation, key + ": the gallery wall shows navigation from " + CardNavGalleryDefinitions.ShowNavigationFrom);
            }
            Assert.IsTrue(CardNavGalleryDefinitions.Poi(CardNavGalleryDefinitions.All.First(e => e.Case.Key == "big")).card.blocks.Any(b => b.kind == "sources"),
                "the big card has its ninth family, the Sources, which is never a target");
        }

        [UnityTest]
        public IEnumerator EveryEntry_IsTheSlotsElement_FitsAt390_HasFullSizeTargetsAndWholeReadableWords_AndRenders([ValueSource(nameof(EntryNames))] string name)
        {
            int index = CardNavGalleryDefinitions.All.ToList().FindIndex(e => e.Name == name);
            var entry = CardNavGalleryDefinitions.All[index];
            _harness.Show(index);
            yield return CardTestInput.Settle();
            var sheet = _harness.Gallery.Sheet;
            Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop, "rests at Full");
            // - rule 2.5.7: an element between sections is not drawn for one -- the slot is empty and the card shows content only
            NavElementRegistry.Shared.TryGet(entry.ElementKey, out var definition);
            if (!NavSlotRule.OffersAChoice(definition, sheet.Nav.Model))
            {
                Assert.IsTrue(sheet.Nav.Model.ShowsNavigation, "precondition: the card has enough targets for navigation");
                Assert.IsNull(ViewOf(sheet, entry), "one section: no " + entry.ElementKey);
                Assert.IsFalse(ChromeShows(sheet, entry), "and nothing shown in its slot");
                yield return CardGalleryChecks.Render(name);
                yield break;
            }
            Assert.AreEqual(entry.ElementKey, KeyOf(sheet, entry), "the slot shows the wall's pick");
            foreach (var other in OtherSlots(sheet, entry)) Assert.IsNull(other, "and every other slot nothing: one element judged alone");
            var root = ViewOf(sheet, entry).Root;
            // - the card LAYER holds the sheet and the menu layer over it: what is shown is judged up to it
            var layer = sheet.Layer;
            Assert.IsTrue(CardTestInput.IsShown(root, layer), "shown at Full");
            Assert.IsTrue(ChromeShows(sheet, entry), "and the chrome shows its slot");

            // - inside the card at the phone width
            float onePixel = CardGalleryChecks.OnePixel(root);
            var card = sheet.Root.worldBound;
            Assert.AreEqual(sheet.Layer.panel.visualTree.layout.width, 390f, onePixel, "precondition: the run's 390 px frame");
            Assert.GreaterOrEqual(root.worldBound.xMin, card.xMin - onePixel, "the element starts inside the card");
            Assert.LessOrEqual(root.worldBound.xMax, card.xMax + onePixel, "and ends inside it");

            // - every target a visitor can tap at least the touch token (a closed list's rows take no room: the bottom bar's More list is
            //   judged open by its own test), every word whole and readable
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            var buttons = root.Query<Button>().ToList().Where(b => CardTestInput.IsShown(b, layer)).ToList();
            Assert.IsNotEmpty(buttons, "the element offers something to tap");
            foreach (var button in buttons)
            {
                Assert.GreaterOrEqual(button.layout.height, touch - onePixel, button.name + ": tall enough to tap");
                Assert.GreaterOrEqual(button.layout.width, touch - onePixel, button.name + ": wide enough to tap");
            }
            // - only words a visitor sees (Labels = icon hides a tab's words; a progress piece with an icon hides its words): a label that
            //   wraps (a segment) must fit its LONGEST word, one that does not, its whole text
            foreach (var label in root.Query<Label>().ToList().Where(l => l.text.Length > 0 && CardTestInput.IsShown(l, layer)))
            {
                bool wraps = label.resolvedStyle.whiteSpace == WhiteSpace.Normal;
                string widest = wraps ? label.text.Split(' ').OrderByDescending(w => label.MeasureTextSize(w, 0, VisualElement.MeasureMode.Undefined, 0,
                    VisualElement.MeasureMode.Undefined).x).First() : label.text;
                var words = label.MeasureTextSize(widest, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                Assert.GreaterOrEqual(label.layout.width + 0.5f, words.x, "'" + widest + "': the whole word fits, never cut");
                float contrast = CardTestInput.Contrast(label.resolvedStyle.color, CardTestInput.EffectiveBackground(label));
                Assert.GreaterOrEqual(contrast, UIAccessibility.MinRatioNormalText, "'" + label.text + "' reads at 4.5:1 (" + contrast.ToString("F2") + ")");
            }

            Assert.IsTrue(ElementChecks.ContainsKey(entry.ElementKey), "an element of the list has its own check: " + entry.ElementKey);
            ElementChecks[entry.ElementKey](_harness, entry);
            yield return CardGalleryChecks.Render(name);
        }

        // The in-flow nav blocks (_3.2 section 5.6, step 11a) over every case: the wall's Auto Nav Block puts the block right after the
        // header (the builder's step, not the harness), no slot holds anything, and the grid is the model's sections -- one tile per section in
        // its order, each its section's words, its family's icon in the family's colour above them, its block count; every tile a full touch
        // target, every word whole and at 4.5:1, the grid inside the card at 390 px; one section draws no tile (rule 2.5.7). A REAL tap (panel
        // events) on every tile lands its section's first block with one nav_jump named nav_icon_grid; rendered Nav_nav_icon_grid_<case>
        private static string[] BlockEntryNames => CardNavGalleryDefinitions.All.Where(e => e.IsBlock).Select(e => e.Name).ToArray();

        [UnityTest]
        public IEnumerator EveryNavBlockEntry_IsRightAfterTheHeader_OneTilePerSection_WholeAndTappable_ARealTapLandsIt_AndRenders(
            [ValueSource(nameof(BlockEntryNames))] string name)
        {
            int index = CardNavGalleryDefinitions.All.ToList().FindIndex(e => e.Name == name);
            var entry = CardNavGalleryDefinitions.All[index];
            _harness.Show(index);
            yield return CardTestInput.Settle();
            var sheet = _harness.Gallery.Sheet;
            var model = sheet.Nav.Model;
            string autoKey = BlockStackBuilder.AutoNavBlockKey(entry.ElementKey);
            Assert.IsTrue(model.ShowsNavigation, "precondition: the gallery's card has Show Navigation From targets");
            Assert.IsNull(sheet.Slots.TopView, "no element in any slot: the block judged alone");
            Assert.IsNull(sheet.Slots.BottomView);
            CollectionAssert.DoesNotContain(model.Targets, autoKey, "the grid is no target of the card's own navigation");

            var slot = sheet.Stack.SlotOfBlock(autoKey);
            Assert.IsNotNull(slot, "Auto Nav Block put the grid in the stack");
            var content = sheet.Stack.Scroll.contentContainer;
            Assert.AreEqual(0, content.IndexOf(slot), "the first block of the scroll: right after the (pinned) header");
            var grid = slot.Q<VisualElement>("card-nav-icon-grid");
            Assert.IsNotNull(grid);
            var tiles = grid.Query<Button>(className: "nav-icon-grid__tile").ToList().Where(b => b.resolvedStyle.display == DisplayStyle.Flex && b.parent == grid).ToList();

            if (!NavOntologyRule.OffersASectionChoice(model.Sections.Count))
            {
                Assert.AreEqual(DisplayStyle.None, grid.resolvedStyle.display, "one section: no grid (rule 2.5.7)");
                yield return CardGalleryChecks.Render(name);
                yield break;
            }
            Assert.AreEqual(model.Sections.Count, tiles.Count, "one tile per section");
            var layer = sheet.Layer;
            float onePixel = CardGalleryChecks.OnePixel(grid);
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            var card = sheet.Root.worldBound;
            Assert.GreaterOrEqual(grid.worldBound.xMin, card.xMin - onePixel, "the grid starts inside the card");
            Assert.LessOrEqual(grid.worldBound.xMax, card.xMax + onePixel, "and ends inside it");
            var strings = Strings(_harness, entry);
            for (int i = 0; i < tiles.Count; i++)
            {
                var section = model.Sections[i];
                var tile = tiles[i];
                var label = tile.Q<Label>(className: "nav-tabs__label");
                var count = tile.Q<Label>(className: "nav-item__count");
                Assert.AreEqual(strings.Get(section.LabelKey), label.text, "tile " + i + ": its section's words in " + entry.Case.Language);
                Assert.AreEqual(section.Count.ToString(), count.text, label.text + ": its block count");
                Assert.IsTrue(CardTestInput.IsShown(tile, layer), label.text + ": shown at Full");
                Assert.GreaterOrEqual(tile.layout.height, touch - onePixel, label.text + ": tall enough to tap");
                Assert.GreaterOrEqual(tile.layout.width, touch - onePixel, label.text + ": wide enough to tap");
                var icon = tile.Q<VisualElement>(className: "card-icon");
                Assert.AreEqual(section.IconKey, CardIcons.KeyOf(icon), label.text + ": its family's icon");
                Assert.LessOrEqual(icon.worldBound.yMax, label.worldBound.yMin + 0.5f, label.text + ": the icon above the words");
                AssertFamilyPaint(icon.Q(className: "card-icon__frame").resolvedStyle.borderTopColor, section.ColorToken, tile, label.text + "'s icon");
                foreach (var words in new[] { label, count })
                {
                    var size = words.MeasureTextSize(words.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                    Assert.GreaterOrEqual(words.layout.width + 0.5f, size.x, "'" + words.text + "': whole, never cut");
                    float contrast = CardTestInput.Contrast(words.resolvedStyle.color, CardTestInput.EffectiveBackground(words));
                    Assert.GreaterOrEqual(contrast, UIAccessibility.MinRatioNormalText, "'" + words.text + "' reads at 4.5:1 (" + contrast.ToString("F2") + ")");
                }
                Assert.LessOrEqual(tile.worldBound.xMax, grid.worldBound.xMax + onePixel, label.text + ": inside the grid");
            }
            yield return CardGalleryChecks.Render(name);

            // - a real tap on every tile, the last first (the card is at its top, so every tap MOVES it): its section's first block lands
            for (int i = tiles.Count - 1; i >= 0; i--)
            {
                sheet.Stack.Scroll.scrollOffset = Vector2.zero;
                yield return CardTestInput.Settle();
                var center = tiles[i].worldBound.center;
                yield return CardTestInput.Tap(sheet.Root.panel, center);
                yield return Landed(sheet.Nav);
                AssertLanded(sheet, model.Sections[i].Targets[0], model.Sections[i].Key + "'s first block, from its tile");
            }
        }

        // Rule 2.5.7 on the gallery's own cards, side by side: the one-family card renders NO strip, and the same card with a second family
        // does -- one tab per section, two tabs
        [UnityTest]
        public IEnumerator TheOneFamilyCase_RendersNoStrip_AndTheSameCardWithASecondFamilyDoes()
        {
            var all = CardNavGalleryDefinitions.All.ToList();
            int one = all.FindIndex(e => e.ElementKey == BuiltInNavElements.TextTabs && e.Case.Key == "one_family");
            int two = all.FindIndex(e => e.ElementKey == BuiltInNavElements.TextTabs && e.Case.Key == "two_families");
            var sheet = _harness.Gallery.Sheet;

            _harness.Show(one);
            yield return CardTestInput.Settle();
            Assert.AreEqual(1, sheet.Nav.Model.Sections.Count, "precondition: one section");
            Assert.IsTrue(sheet.Nav.Model.ShowsNavigation, "the card shows navigation (5 targets, from 2)");
            Assert.IsNull(sheet.Slots.TopView, "but no strip: one tab is not a choice");
            Assert.AreEqual(DisplayStyle.None, sheet.Slots.Top.resolvedStyle.display, "the slot takes no room");

            _harness.Show(two);
            yield return CardTestInput.Settle();
            Assert.AreEqual(2, sheet.Nav.Model.Sections.Count, "precondition: the second family is a second section");
            var tabs = sheet.Slots.TopView as TextTabsNavView;
            Assert.IsNotNull(tabs, "two sections: the strip");
            Assert.AreEqual(2, tabs.Tabs.Count, "with two tabs");
            Assert.IsTrue(sheet.Chrome.TopShown && sheet.Slots.Top.resolvedStyle.visibility == Visibility.Visible, "shown at Full");
        }

        // text_tabs: a tab per section in the model's order, each saying its section's word in the card's language, the first one marked
        // (underline AND bold) and no other
        private static void TextTabsKeepTheirPromise(CardNavGalleryHarness harness, CardNavGalleryDefinitions.Entry entry)
        {
            var sheet = harness.Gallery.Sheet;
            var tabs = (TextTabsNavView)sheet.Slots.TopView;
            var model = sheet.Nav.Model;
            Assert.AreEqual(model.Sections.Count, tabs.Tabs.Count, "one tab per section");
            var strings = new CardStrings(harness.Gallery.StringTable.Entries(), CardStringSources.Shared.Entries(), null, entry.Case.Language, "en");
            CollectionAssert.AreEqual(model.Sections.Select(s => strings.Get(s.LabelKey)).ToList(), tabs.Tabs.Select(t => t.Label.text).ToList(),
                "each tab says its section's word, in " + entry.Case.Language);
            Assert.AreEqual(0, tabs.CurrentIndex, "the first section is marked at the top of the card");
            foreach (var tab in tabs.Tabs)
            {
                bool marked = tab.Index == 0;
                Assert.AreEqual(marked, tab.Underline.resolvedStyle.display == DisplayStyle.Flex, tab.Label.text + ": underline");
                Assert.AreEqual(marked ? FontStyle.Bold : FontStyle.Normal, tab.Label.resolvedStyle.unityFontStyleAndWeight, tab.Label.text + ": weight");
            }
            // - the strip scrolls when its words are wider than the card, and then only it does
            bool scrolls = NavStripRule.MaxOffset(tabs.Row.layout.width, tabs.Viewport.contentRect.width) > 0f;
            Assert.AreEqual(scrolls, entry.Case.Key == "big" || entry.Case.Key == "pt_long", "only the eight-section cards are wider than the card");
        }

        // The card's words as the gallery entry reads them
        private static CardStrings Strings(CardNavGalleryHarness harness, CardNavGalleryDefinitions.Entry entry) =>
            new(harness.Gallery.StringTable.Entries(), CardStringSources.Shared.Entries(), null, entry.Case.Language, "en");

        // A family colour painted on a part: exactly its token's value on this card, and at least 3:1 on the strip behind it (a
        // non-text mark, WCAG 1.4.11) -- the icon or the words beside it always name the family too
        private static void AssertFamilyPaint(Color painted, string token, VisualElement on, string what)
        {
            Assert.IsTrue(CardTokenColours.TryResolve(on, token, out Color expected), what + ": " + token + " resolves on the card");
            Assert.AreEqual(expected, painted, what + ": painted in " + token);
            float contrast = CardTestInput.Contrast(painted, CardTestInput.EffectiveBackground(on.parent));
            Assert.GreaterOrEqual(contrast, UIAccessibility.MinRatioLargeTextOrUIComponent, what + " reads at 3:1 (" + contrast.ToString("F2") + ")");
        }

        // icon_tabs: the text tabs' promise, and each tab wears its family's icon in its family's colour above its word (Labels' default)
        private static void IconTabsKeepTheirPromise(CardNavGalleryHarness harness, CardNavGalleryDefinitions.Entry entry) =>
            AssertIconTabs((IconTabsNavView)harness.Gallery.Sheet.Slots.TopView, harness, entry);

        private static void AssertIconTabs(TabsNavView tabs, CardNavGalleryHarness harness, CardNavGalleryDefinitions.Entry entry)
        {
            var model = harness.Gallery.Sheet.Nav.Model;
            var strings = Strings(harness, entry);
            Assert.AreEqual(model.Sections.Count, tabs.Tabs.Count, "one tab per section");
            Assert.AreEqual(0, tabs.CurrentIndex, "the first section is marked");
            foreach (var tab in tabs.Tabs)
            {
                var section = model.Sections[tab.Index];
                Assert.AreEqual(strings.Get(section.LabelKey), tab.Label.text, "its section's word, in " + entry.Case.Language);
                Assert.IsNotNull(tab.Icon, "an icon tab");
                Assert.AreEqual(section.IconKey, CardIcons.KeyOf(tab.Icon), tab.Label.text + ": its family's icon");
                Assert.AreEqual(DisplayStyle.Flex, tab.Icon.resolvedStyle.display, tab.Label.text + ": shown (Icon And Text)");
                Assert.AreEqual(DisplayStyle.Flex, tab.Label.resolvedStyle.display, tab.Label.text + ": and its word");
                Assert.LessOrEqual(tab.Icon.worldBound.yMax, tab.Label.worldBound.yMin + 0.5f, tab.Label.text + ": the icon above the word");
                var frame = tab.Icon.Q(className: "card-icon__frame");
                AssertFamilyPaint(frame.resolvedStyle.borderTopColor, section.ColorToken, tab.Button, tab.Label.text + "'s icon");
                bool marked = tab.Index == 0;
                Assert.AreEqual(marked, tab.Underline.resolvedStyle.display == DisplayStyle.Flex, tab.Label.text + ": underline");
                Assert.AreEqual(marked ? FontStyle.Bold : FontStyle.Normal, tab.Label.resolvedStyle.unityFontStyleAndWeight, tab.Label.text + ": weight");
            }
        }

        // icon_tabs_chips: the icon tabs, and under them the chips of the marked section's kinds when it has two or more -- each chip its
        // kind's word, its family's colour as a dot BESIDE the word; the row keeps its place on every card with such a section
        private static void IconTabsChipsKeepTheirPromise(CardNavGalleryHarness harness, CardNavGalleryDefinitions.Entry entry)
        {
            var view = (IconTabsChipsNavView)harness.Gallery.Sheet.Slots.TopView;
            AssertIconTabs(view.Tabs, harness, entry);
            var model = harness.Gallery.Sheet.Nav.Model;
            var first = model.Sections[0];
            Assert.Greater(first.Kinds.Count, 1, "precondition: the first section of every case holds two kinds or more");
            Assert.AreEqual(0, view.ChipsSection, "the chips show the marked section's kinds");
            Assert.LessOrEqual(view.Tabs.Root.worldBound.yMax, view.Chips.Root.worldBound.yMin + 0.5f, "the chips sit under the tabs");
            AssertChips(view.Chips, first.Kinds, harness, entry);
        }

        // kind_chips: one chip per kind of the card, in the sections' order, the first block's kind marked
        private static void KindChipsKeepTheirPromise(CardNavGalleryHarness harness, CardNavGalleryDefinitions.Entry entry)
        {
            var model = harness.Gallery.Sheet.Nav.Model;
            AssertChips(((KindChipsNavView)harness.Gallery.Sheet.Slots.TopView).Row, model.Sections.SelectMany(s => s.Kinds).ToList(), harness, entry);
        }

        private static void AssertChips(NavChipRow row, IReadOnlyList<CardNavModel.Kind> kinds, CardNavGalleryHarness harness, CardNavGalleryDefinitions.Entry entry)
        {
            var strings = Strings(harness, entry);
            var nav = harness.Gallery.Sheet.Nav;
            CollectionAssert.AreEqual(kinds.Select(k => k.Key), row.Chips.Select(c => c.Kind.Key), "one chip per kind, in order");
            foreach (var chip in row.Chips)
            {
                Assert.AreEqual(strings.Get(chip.Kind.LabelKey), chip.Label.text, chip.Kind.Key + ": its kind's word, in " + entry.Case.Language);
                Assert.LessOrEqual(chip.Dot.worldBound.xMax, chip.Label.worldBound.xMin + 0.5f, chip.Kind.Key + ": the dot sits BESIDE the word");
                AssertFamilyPaint(chip.Dot.resolvedStyle.backgroundColor, chip.Kind.ColorToken, chip.Dot, chip.Kind.Key + "'s dot");
                bool marked = chip.Kind == nav.Model.KindOf(nav.CurrentBlock);
                Assert.AreEqual(marked, chip.Button.ClassListContains(NavChip.CurrentClass), chip.Kind.Key + ": marked only when its kind is read");
                Assert.AreEqual(marked ? FontStyle.Bold : FontStyle.Normal, chip.Label.resolvedStyle.unityFontStyleAndWeight, chip.Kind.Key + ": weight");
            }
            // - the spy line (a third down the viewport) names a block at once: short first blocks may already be past it
            Assert.IsNotNull(nav.CurrentBlock, "a block is being read at the top of the card");
            Assert.AreEqual(nav.Model.Sections[0], nav.Model.Sections[nav.Model.SectionOf(nav.CurrentBlock)], "a block of the first section");
            Assert.AreEqual(1, row.Chips.Count(c => c.Button.ClassListContains(NavChip.CurrentClass)), "exactly one chip marked");
        }

        // segmented: one segment per group with a target, equal, inside the track with no scroll; the first group marked (filled + bold)
        private static void SegmentedKeepsItsPromise(CardNavGalleryHarness harness, CardNavGalleryDefinitions.Entry entry)
        {
            var view = (SegmentedNavView)harness.Gallery.Sheet.Slots.TopView;
            var model = harness.Gallery.Sheet.Nav.Model;
            var strings = Strings(harness, entry);
            Assert.GreaterOrEqual(model.Groups.Count, 2, "precondition: drawn only with two groups");
            CollectionAssert.AreEqual(model.Groups.Select(g => NavGroupRule.Words(g.Definition, strings)), view.Segments.Select(s => s.Label.text),
                "each segment says its group's word, in " + entry.Case.Language);
            float onePixel = CardGalleryChecks.OnePixel(view.Root);
            var track = view.Track.worldBound;
            float width = view.Segments[0].Button.layout.width;
            foreach (var segment in view.Segments)
            {
                Assert.AreEqual(width, segment.Button.layout.width, onePixel, segment.Label.text + ": equal segments");
                Assert.GreaterOrEqual(segment.Button.worldBound.xMin, track.xMin - onePixel, segment.Label.text + ": inside the track");
                Assert.LessOrEqual(segment.Button.worldBound.xMax, track.xMax + onePixel, segment.Label.text + ": inside the track (no scroll)");
                bool marked = segment.Index == 0;
                Assert.AreEqual(marked, segment.Button.ClassListContains(SegmentedNavView.CurrentClass), segment.Label.text + ": marked");
                Assert.AreEqual(marked ? FontStyle.Bold : FontStyle.Normal, segment.Label.resolvedStyle.unityFontStyleAndWeight, segment.Label.text + ": weight");
            }
        }

        // progress_map: a piece per section, as wide as ProgressMapRule says (and so never under the touch token), each with its family's
        // icon over its family-coloured piece of bar; the first section current (thicker, solid) and the rest faint
        private static void ProgressMapKeepsItsPromise(CardNavGalleryHarness harness, CardNavGalleryDefinitions.Entry entry)
        {
            var view = (ProgressMapNavView)harness.Gallery.Sheet.Slots.TopView;
            var model = harness.Gallery.Sheet.Nav.Model;
            float onePixel = CardGalleryChecks.OnePixel(view.Root);
            Assert.AreEqual(model.Sections.Count, view.Segments.Count, "one piece per section");
            Assert.AreEqual(model.Sections.Count, view.Widths.Count, "the rule shared the strip");
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            var expected = ProgressMapRule.Widths(model.Sections.Select(s => s.Count).ToList(), Enumerable.Repeat(touch, model.Sections.Count).ToList(),
                view.Strip.VisibleWidth);
            foreach (var segment in view.Segments)
            {
                var section = model.Sections[segment.Index];
                Assert.AreEqual(expected[segment.Index], segment.Button.layout.width, onePixel, section.Key + ": the rule's width for " + section.Count + " targets");
                Assert.AreEqual(section.IconKey, CardIcons.KeyOf(segment.Icon), section.Key + ": its family's icon");
                AssertFamilyPaint(segment.Bar.resolvedStyle.backgroundColor, section.ColorToken, segment.Button, section.Key + "'s piece");
                bool current = segment.Index == 0;
                Assert.AreEqual(current, segment.Button.ClassListContains(ProgressMapNavView.CurrentClass), section.Key + ": current");
                Assert.AreEqual(current ? 1f : CardTestInput.TokenPx("--ts-ahead-opacity"), segment.Bar.resolvedStyle.opacity, 0.01f, section.Key + ": solid up to the one read");
            }
            float thin = CardTestInput.TokenPx("--ts-segment-height"), thick = CardTestInput.TokenPx("--ts-progress-height");
            Assert.AreEqual(thick, view.Segments[0].Bar.layout.height, onePixel, "the current piece is thicker");
            if (view.Segments.Count > 1) Assert.AreEqual(thin, view.Segments[1].Bar.layout.height, onePixel, "than the others");
            Assert.AreEqual(view.Strip.Row.layout.width > view.Strip.VisibleWidth + onePixel, expected.Sum() > view.Strip.VisibleWidth + onePixel,
                "the bar scrolls exactly when its minimums are wider than the card");
        }

        // bottom_bar: at the very bottom of the card, Bar Slots - 1 sections and More when the card has more sections than the 5 slots
        // (else one slot per section and no More), each slot an icon tab -- its family's icon in its colour above its word -- sharing the
        // bar's width equally; the first section marked (underline AND bold); More's list closed, holding exactly the overflow
        private static void BottomBarKeepsItsPromise(CardNavGalleryHarness harness, CardNavGalleryDefinitions.Entry entry)
        {
            var sheet = harness.Gallery.Sheet;
            var bar = (BottomBarNavView)sheet.Slots.BottomView;
            var model = sheet.Nav.Model;
            var strings = Strings(harness, entry);
            float onePixel = CardGalleryChecks.OnePixel(bar.Root);
            Assert.AreEqual(sheet.Root.worldBound.yMax, sheet.Slots.Bottom.worldBound.yMax, onePixel, "the bar closes the card: in the thumb zone");
            Assert.LessOrEqual(sheet.Stack.Scroll.worldBound.yMax, sheet.BottomArea.worldBound.yMin + onePixel, "under the stack");

            int slots = NavOntologyRule.MaxSlots(ConfigNav(entry));
            bool overflows = model.Sections.Count > slots;
            Assert.AreEqual(overflows, entry.Case.Key == "big" || entry.Case.Key == "pt_long", "only the eight-section cards overflow 5 slots");
            int inBar = overflows ? slots - 1 : model.Sections.Count;
            CollectionAssert.AreEqual(model.Sections.Take(inBar).Select(s => strings.Get(s.LabelKey)), bar.Items.Select(t => t.Label.text),
                "the first sections, each its word in " + entry.Case.Language);
            Assert.AreEqual(overflows, bar.HasMore, "More exactly when the sections overflow");
            var slotsShown = bar.Items.Select(t => t.Button).ToList();
            if (overflows)
            {
                slotsShown.Add(bar.More.Button);
                Assert.AreEqual(strings.Get(CardStrings.Keys.NavMore), bar.More.Label.text, "More says its word in " + entry.Case.Language);
                CollectionAssert.AreEqual(model.Sections.Skip(inBar).Select(s => strings.Get(s.LabelKey)), bar.MoreRows.Select(r => r.Label.text),
                    "More holds exactly the rest, in order");
            }
            Assert.IsFalse(bar.MoreOpen, "the list starts closed");
            Assert.AreEqual(DisplayStyle.None, bar.MoreList.resolvedStyle.display, "and takes no room");
            float width = slotsShown[0].layout.width;
            foreach (var button in slotsShown)
                Assert.AreEqual(width, button.layout.width, onePixel, button.name + ": every slot the same share of the bar");
            Assert.AreEqual(bar.Bar.contentRect.width, width * slotsShown.Count, onePixel * slotsShown.Count, "the slots fill the bar");
            foreach (var tab in bar.Items)
            {
                var section = model.Sections[tab.Index];
                Assert.AreEqual(section.IconKey, CardIcons.KeyOf(tab.Icon), tab.Label.text + ": its family's icon");
                Assert.LessOrEqual(tab.Icon.worldBound.yMax, tab.Label.worldBound.yMin + 0.5f, tab.Label.text + ": the icon above the word");
                var frame = tab.Icon.Q(className: "card-icon__frame");
                if (frame.resolvedStyle.display == DisplayStyle.Flex)
                    AssertFamilyPaint(frame.resolvedStyle.borderTopColor, section.ColorToken, tab.Button, tab.Label.text + "'s icon");
                bool marked = tab.Index == 0;
                Assert.AreEqual(marked, tab.Underline.resolvedStyle.display == DisplayStyle.Flex, tab.Label.text + ": underline");
                Assert.AreEqual(marked ? FontStyle.Bold : FontStyle.Normal, tab.Label.resolvedStyle.unityFontStyleAndWeight, tab.Label.text + ": weight");
            }
            Assert.IsFalse(bar.More.Button.ClassListContains(NavTab.CurrentClass), "More is not marked while the first section is read");
        }

        // dock: the bar's slots as an icon PILL. The section read (the first, at the top of the card) shows icon + word on an outlined plate,
        // every other slot its icon alone; the pill is centred and inside the card; the bar's underline is gone (the plate and the widening
        // say "here", never the colour alone); every slot still names itself (tooltip) and is a whole touch target
        private static void DockKeepsItsPromise(CardNavGalleryHarness harness, CardNavGalleryDefinitions.Entry entry)
        {
            var sheet = harness.Gallery.Sheet;
            var dock = (DockNavView)sheet.Slots.BottomView;
            var model = sheet.Nav.Model;
            var strings = Strings(harness, entry);
            float onePixel = CardGalleryChecks.OnePixel(dock.Root);
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            var card = sheet.Root.worldBound;
            Assert.IsTrue(dock.Root.ClassListContains(DockNavView.DockClass), "the dock's own look");
            Assert.AreEqual(card.yMax, sheet.Slots.Bottom.worldBound.yMax, onePixel, "it closes the card: in the thumb zone");

            int slots = NavOntologyRule.MaxSlots(ConfigNav(entry));
            bool overflows = model.Sections.Count > slots;
            Assert.AreEqual(overflows, dock.HasMore, "More exactly when the sections overflow the slots");
            int inBar = overflows ? slots - 1 : model.Sections.Count;
            CollectionAssert.AreEqual(model.Sections.Take(inBar).Select(s => strings.Get(s.LabelKey)), dock.Items.Select(t => t.Label.text),
                "the first sections, each its word in " + entry.Case.Language);
            var all = dock.Items.ToList();
            if (overflows) all.Add(dock.More);

            // - the pill: centred, inside the card, never wider than it
            var pill = dock.Bar.worldBound;
            Assert.LessOrEqual(pill.width, card.width + onePixel, "the pill is never wider than the card (" + pill.width.ToString("F1") + " of " + card.width.ToString("F1") + ")");
            Assert.GreaterOrEqual(pill.xMin, card.xMin - onePixel, "inside the card on the left");
            Assert.LessOrEqual(pill.xMax, card.xMax + onePixel, "and on the right");
            Assert.AreEqual(card.center.x, pill.center.x, 2f * onePixel, "centred");

            // - one marked item with its word beside its icon, on a plate with an outline; the rest icon only
            for (int i = 0; i < all.Count; i++)
            {
                var slot = all[i];
                bool marked = i == 0;
                string name = slot.Label.text;
                Assert.AreEqual(marked, CardTestInput.IsShown(slot.Label, sheet.Root), name + ": its word shows only while it is the section read");
                Assert.IsTrue(CardTestInput.IsShown(slot.Icon, sheet.Root), name + ": its icon always shows");
                Assert.GreaterOrEqual(slot.Button.layout.width, touch - onePixel, name + ": wide enough to tap");
                Assert.GreaterOrEqual(slot.Button.layout.height, touch - onePixel, name + ": tall enough to tap");
                Assert.AreEqual(name, slot.Button.tooltip, name + ": an icon alone still names its slot");
                Assert.AreEqual(marked, slot.Button.ClassListContains(NavTab.CurrentClass), name + ": marked");
                Assert.AreEqual(marked ? CardTestInput.TokenPx("--ts-choice-border") : 0f, slot.Button.resolvedStyle.borderTopWidth, 0.01f, name + ": an outline on the marked plate only");
                if (slot.Underline != null) Assert.AreEqual(DisplayStyle.None, slot.Underline.resolvedStyle.display, name + ": no underline in a pill");
                if (!marked) Assert.Less(slot.Button.layout.width, all[0].Button.layout.width, name + ": narrower than the marked one, which widened");
            }
            var first = all[0];
            Assert.LessOrEqual(first.Icon.worldBound.xMax, first.Label.worldBound.xMin + 0.5f, "the marked item: its word BESIDE its icon");
            Assert.AreEqual(FontStyle.Bold, first.Label.resolvedStyle.unityFontStyleAndWeight, "and bold");
        }

        // bar_sheet: the bar's own promise, then its own: the kinds list starts closed, and the arrow that says "a tap here opens it" is on the
        // slot being read and no other, up, inside its slot and clear of the icon
        private static void BarSheetKeepsItsPromise(CardNavGalleryHarness harness, CardNavGalleryDefinitions.Entry entry)
        {
            BottomBarKeepsItsPromise(harness, entry);
            var sheet = harness.Gallery.Sheet;
            var view = (BarSheetNavView)sheet.Slots.BottomView;
            var model = sheet.Nav.Model;
            float onePixel = CardGalleryChecks.OnePixel(view.Root);
            Assert.IsFalse(view.KindsOpen, "the kinds list starts closed");
            Assert.AreEqual(DisplayStyle.None, view.KindsList.resolvedStyle.display, "and takes no room");
            Assert.Greater(model.Sections[0].Kinds.Count, 1, "precondition: the first section of every case holds two kinds or more");
            Assert.AreEqual(view.Items.Count, view.Arrows.Count, "one arrow per slot");
            foreach (var (slot, arrow) in view.Arrows)
            {
                bool offered = slot.Index == 0;
                Assert.AreEqual(offered, arrow.resolvedStyle.display == DisplayStyle.Flex, slot.Label.text + ": the arrow only on the slot being read");
                if (!offered) continue;
                var box = slot.Button.worldBound;
                var at = arrow.worldBound;
                Assert.GreaterOrEqual(at.xMin, box.xMin - onePixel, "the arrow is inside its slot (left)");
                Assert.LessOrEqual(at.xMax, box.xMax + onePixel, "(right)");
                Assert.GreaterOrEqual(at.yMin, box.yMin - onePixel, "(top)");
                Assert.GreaterOrEqual(at.xMin, slot.Icon.worldBound.xMax - onePixel, "clear of the slot's own icon");
                Assert.AreEqual(180f, arrow.resolvedStyle.rotate.angle.ToDegrees(), 0.5f, "turned up: the list opens above the bar");
                Assert.AreEqual(CardIcons.OpenBlock, CardIcons.KeyOf(arrow), "the card's one arrow");
            }
        }

        // next_up: it names the block AFTER the one read at the top of the card, by the words the stack draws over it, with its place among
        // all the targets and a Next button; the words on the left, the button in the card's right edge; a touch target tall
        private static void NextUpKeepsItsPromise(CardNavGalleryHarness harness, CardNavGalleryDefinitions.Entry entry)
        {
            var sheet = harness.Gallery.Sheet;
            var view = (NextUpNavView)sheet.Slots.BottomView;
            var model = sheet.Nav.Model;
            var strings = Strings(harness, entry);
            float onePixel = CardGalleryChecks.OnePixel(view.Root);
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            var card = sheet.Root.worldBound;
            // - the spy line is a third down the viewport, so a short first block is already past it: the block read at the top of the card is
            //   the first target or one a little further on -- whichever it is, the bar names the one AFTER it
            int at = model.Targets.ToList().IndexOf(sheet.Nav.CurrentBlock);
            Assert.GreaterOrEqual(at, 0, "a target is being read at the top of the card");
            Assert.Less(at, model.TargetCount - 1, "precondition: it is not the last");
            string next = model.Targets[at + 1];
            Assert.AreEqual(next, view.NextBlock, "the bar names the target after the one read");
            Assert.AreEqual(strings.Get(CardStrings.Keys.NavNextUp), view.Caption.text, "Up next, in " + entry.Case.Language);
            Assert.AreEqual(string.Format(System.Globalization.CultureInfo.InvariantCulture, strings.Get(CardStrings.Keys.NavNextUpPosition), at + 2, model.TargetCount),
                view.Position.text, "its place among all " + model.TargetCount + " targets");
            string drawn = sheet.Stack.HeadingOfBlock(next);
            string expected = drawn.Length > 0 ? drawn : strings.Get(model.KindOf(next).LabelKey);
            Assert.IsNotEmpty(expected, "precondition: the next block has words to be named by");
            Assert.AreEqual(expected, view.Heading.text, "the words the stack draws over it (else its kind's)");
            Assert.AreEqual(strings.Get(CardStrings.Keys.NavNextButton), view.NextLabel.text, "the button in " + entry.Case.Language);
            Assert.IsTrue(CardTestInput.IsShown(view.Heading, sheet.Root) && CardTestInput.IsShown(view.Next, sheet.Root), "block and button shown");

            Assert.AreEqual(card.yMax, sheet.Slots.Bottom.worldBound.yMax, onePixel, "the bar closes the card: in the thumb zone");
            Assert.GreaterOrEqual(view.Root.layout.height, touch + 2f * CardTestInput.TokenPx("--ts-space-2") - onePixel, "a whole touch target tall, with its padding");
            var text = view.Root.Q("nav-next-up-text").worldBound;
            var button = view.Next.worldBound;
            Assert.LessOrEqual(text.xMax, button.xMin + onePixel, "the words sit left of the button");
            Assert.LessOrEqual(button.xMax, card.xMax + onePixel, "the button is inside the card's right edge");
            Assert.AreEqual(view.Root.worldBound.center.y, button.center.y, 2f * onePixel, "the button centred on the bar");
            Assert.IsTrue(CardTokenColours.TryResolve(view.Next, "--ts-accent", out Color accent), "precondition: --ts-accent resolves on the card");
            Assert.AreEqual(accent, view.Next.resolvedStyle.borderTopColor, "Next is outlined in the accent (the sticky CTA above it keeps the fill)");
            Assert.AreEqual(CardTestInput.TokenPx("--ts-choice-border"), view.Next.resolvedStyle.borderTopWidth, 0.01f, "with the card's choice outline width");
            Assert.AreNotEqual(accent, view.Next.resolvedStyle.backgroundColor, "and no accent fill: it is the card's second action, not its first");
        }

        // The wall's navigation settings of an entry (what Bar Slots the gallery's wall has)
        private static CardNavigationSettings ConfigNav(CardNavGalleryDefinitions.Entry entry) => CardNavGalleryDefinitions.Wall(entry).card_settings.navigation;

        // The bottom bar's More list OPEN on the eight-section cards (English and Portuguese): above the bar, inside the card, one row per
        // overflow section -- each a whole touch target, its word whole -- and the bar still under it; rendered Nav_bottom_bar_<case>_more
        [UnityTest]
        public IEnumerator TheBottomBarsMoreList_OpensAboveTheBar_WithAWholeRowPerOverflowSection_AndRenders([Values("big", "pt_long")] string caseKey)
        {
            var all = CardNavGalleryDefinitions.All.ToList();
            int index = all.FindIndex(e => e.ElementKey == BuiltInNavElements.BottomBar && e.Case.Key == caseKey);
            _harness.Show(index);
            yield return CardTestInput.Settle();
            var sheet = _harness.Gallery.Sheet;
            var bar = (BottomBarNavView)sheet.Slots.BottomView;
            Assert.IsTrue(bar.HasMore, "precondition: the eight sections overflow the bar");
            bar.OpenMore();
            yield return CardTestInput.Settle();
            Assert.IsTrue(bar.MoreOpen);
            float onePixel = CardGalleryChecks.OnePixel(bar.Root);
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            Assert.LessOrEqual(bar.MoreList.worldBound.yMax, bar.Bar.worldBound.yMin + onePixel, "the list sits above the bar");
            Assert.GreaterOrEqual(bar.MoreList.worldBound.yMin, sheet.Root.worldBound.yMin, "inside the card");
            Assert.AreEqual(sheet.Root.worldBound.yMax, bar.Bar.worldBound.yMax, onePixel, "the bar stays at the card's bottom");
            Assert.AreEqual(sheet.Nav.Model.MoreSections.Count, bar.MoreRows.Count, "one row per overflow section");
            foreach (var row in bar.MoreRows)
            {
                Assert.GreaterOrEqual(row.Button.layout.height, touch - onePixel, row.Label.text + ": tall enough to tap");
                Assert.GreaterOrEqual(row.Button.layout.width, touch - onePixel, row.Label.text + ": wide enough to tap");
                var words = row.Label.MeasureTextSize(row.Label.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                Assert.GreaterOrEqual(row.Label.layout.width + 0.5f, words.x, "'" + row.Label.text + "': whole");
                Assert.LessOrEqual(row.Icon.worldBound.xMax, row.Label.worldBound.xMin + 0.5f, row.Label.text + ": its icon beside its word");
            }
            yield return CardGalleryChecks.Render("Nav_bottom_bar_" + caseKey + "_more");
        }

        private IEnumerator ShowEntry(string elementKey, string caseKey)
        {
            var all = CardNavGalleryDefinitions.All.ToList();
            int index = all.FindIndex(e => e.ElementKey == elementKey && e.Case.Key == caseKey);
            Assert.GreaterOrEqual(index, 0, "precondition: the gallery has " + elementKey + " over " + caseKey);
            _harness.Show(index);
            yield return CardTestInput.Settle();
        }

        private IEnumerator Landed(NavController nav)
        {
            for (int frame = 0; frame < 600 && nav.IsJumping; frame++) yield return null;
            Assert.IsFalse(nav.IsJumping, "the jump came to rest");
            yield return CardTestInput.Settle(0.3f);
        }

        // bar_sheet's kinds list OPEN (a second tap on the family being read): above the bar, inside the card, one row per kind of that
        // family in the model's order -- each a whole touch target, its words whole and readable, its block count beside it; rendered
        // Nav_bar_sheet_<case>_kinds
        [UnityTest]
        public IEnumerator TheBarSheetsKindsList_OpensAboveTheBar_WithAWholeRowPerKind_AndTheirCounts_AndRenders([Values("big", "pt_long", "small")] string caseKey)
        {
            yield return ShowEntry(BuiltInNavElements.BarSheet, caseKey);
            var sheet = _harness.Gallery.Sheet;
            var view = (BarSheetNavView)sheet.Slots.BottomView;
            var model = sheet.Nav.Model;
            Assert.AreEqual(0, sheet.Nav.Current, "precondition: the card is read at its first family");
            view.Tap(0);
            yield return CardTestInput.Settle();
            Assert.IsTrue(view.KindsOpen, "a tap on the family being read opens its kinds");
            var strings = new CardStrings(_harness.Gallery.StringTable.Entries(), CardStringSources.Shared.Entries(), null, CardNavGalleryDefinitions.Cases.First(c => c.Key == caseKey).Language, "en");
            var kinds = model.Sections[0].Kinds;
            Assert.GreaterOrEqual(kinds.Count, 2, "precondition: a list worth opening");
            float onePixel = CardGalleryChecks.OnePixel(view.Root);
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            Assert.LessOrEqual(view.KindsList.worldBound.yMax, view.Bar.worldBound.yMin + onePixel, "the list sits above the bar");
            Assert.GreaterOrEqual(view.KindsList.worldBound.yMin, sheet.Root.worldBound.yMin, "inside the card");
            Assert.AreEqual(sheet.Root.worldBound.yMax, view.Bar.worldBound.yMax, onePixel, "the bar stays at the card's bottom");
            CollectionAssert.AreEqual(kinds.Select(k => strings.Get(k.LabelKey)), view.KindRows.Select(r => r.Label.text), "one row per kind, in order");
            CollectionAssert.AreEqual(kinds.Select(k => k.Count.ToString()), view.KindRows.Select(r => r.Count.text), "each with its block count");
            foreach (var row in view.KindRows)
            {
                Assert.GreaterOrEqual(row.Button.layout.height, touch - onePixel, row.Label.text + ": tall enough to tap");
                Assert.GreaterOrEqual(row.Button.layout.width, touch - onePixel, row.Label.text + ": wide enough to tap");
                Assert.GreaterOrEqual(row.Button.worldBound.xMin, sheet.Root.worldBound.xMin - onePixel, row.Label.text + ": inside the card");
                Assert.LessOrEqual(row.Button.worldBound.xMax, sheet.Root.worldBound.xMax + onePixel, row.Label.text + ": inside the card");
                foreach (var label in new[] { row.Label, row.Count })
                {
                    var words = label.MeasureTextSize(label.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                    Assert.GreaterOrEqual(label.layout.width + 0.5f, words.x, "'" + label.text + "': whole, never cut");
                    float contrast = CardTestInput.Contrast(label.resolvedStyle.color, CardTestInput.EffectiveBackground(label));
                    Assert.GreaterOrEqual(contrast, UIAccessibility.MinRatioNormalText, "'" + label.text + "' reads at 4.5:1 (" + contrast.ToString("F2") + ")");
                }
            }
            yield return CardGalleryChecks.Render("Nav_bar_sheet_" + caseKey + "_kinds");
        }

        // next_up at the END of the card: nothing to go to, so the bar says it is finished in the card's language, still shows the card's size,
        // names no block and has no Next button -- and stays a touch target tall, so it does not shrink under the reader's thumb
        [UnityTest]
        public IEnumerator TheUpNextBar_AtTheLastTarget_SaysTheCardIsFinished_WithNoNext_AndKeepsItsHeight_AndRenders([Values("big", "pt_long", "one_family")] string caseKey)
        {
            yield return ShowEntry(BuiltInNavElements.NextUp, caseKey);
            var sheet = _harness.Gallery.Sheet;
            var view = (NextUpNavView)sheet.Slots.BottomView;
            var model = sheet.Nav.Model;
            string language = CardNavGalleryDefinitions.Cases.First(c => c.Key == caseKey).Language;
            var strings = new CardStrings(_harness.Gallery.StringTable.Entries(), CardStringSources.Shared.Entries(), null, language, "en");
            Assert.IsFalse(view.Finished, "precondition: not at the end yet");
            float captionSize = view.Caption.resolvedStyle.fontSize;
            Assert.IsFalse(view.Root.ClassListContains(NextUpNavView.DoneClass), "precondition: not the finished look yet");
            string last = model.Targets[model.Targets.Count - 1];
            Assert.IsTrue(sheet.Nav.JumpToBlock(last, "test"), "precondition: the last target can be jumped to");
            yield return Landed(sheet.Nav);
            Assert.AreEqual(last, sheet.Nav.CurrentBlock, "precondition: the reader is at the last target");
            Assert.IsTrue(view.Finished);
            Assert.AreEqual(strings.Get(CardStrings.Keys.NavNextUpDone), view.Caption.text, "the bar says the card is finished, in " + language);
            Assert.AreEqual(string.Format(System.Globalization.CultureInfo.InvariantCulture, strings.Get(CardStrings.Keys.NavNextUpPosition), model.TargetCount, model.TargetCount),
                view.Position.text, "keeping the card's size");
            Assert.IsFalse(CardTestInput.IsShown(view.Next, sheet.Root), "no Next button that goes nowhere");
            Assert.IsFalse(CardTestInput.IsShown(view.Heading, sheet.Root), "no block named");
            Assert.IsTrue(CardTestInput.IsShown(view.Caption, sheet.Root) && CardTestInput.IsShown(view.Position, sheet.Root), "but the words and the place are there");
            Assert.IsTrue(view.Root.ClassListContains(NextUpNavView.DoneClass), "the finished look");
            Assert.Greater(view.Caption.resolvedStyle.fontSize, captionSize, "the finished words are bigger than the small line over a block: they are all the bar has to say");
            Assert.AreEqual(FontStyle.Bold, view.Caption.resolvedStyle.unityFontStyleAndWeight, "and bold");
            float onePixel = CardGalleryChecks.OnePixel(view.Root);
            Assert.GreaterOrEqual(view.Root.layout.height, CardTestInput.TokenPx("--ts-touch-target") + 2f * CardTestInput.TokenPx("--ts-space-2") - onePixel,
                "still a whole touch target tall (with its padding)");
            foreach (var label in new[] { view.Caption, view.Position })
            {
                var words = label.MeasureTextSize(label.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                Assert.GreaterOrEqual(label.layout.width + 0.5f, words.x, "'" + label.text + "': whole, never cut");
                float contrast = CardTestInput.Contrast(label.resolvedStyle.color, CardTestInput.EffectiveBackground(label));
                Assert.GreaterOrEqual(contrast, UIAccessibility.MinRatioNormalText, "'" + label.text + "' reads at 4.5:1 (" + contrast.ToString("F2") + ")");
            }
            yield return CardGalleryChecks.Render("Nav_next_up_" + caseKey + "_finished");
        }

        // A block's top meets the viewport's top (+-1 px), or -- a late block -- the stack sits at the end of its range
        private static void AssertLanded(PoiCardSheetView sheet, string block, string what)
        {
            var scroll = sheet.Stack.Scroll;
            float onePixel = sheet.Layer.panel.visualTree.layout.height / Screen.height;
            float range = NavScrollRule.Range(scroll.contentContainer.layout.height, scroll.contentViewport.layout.height);
            Assert.Greater(range, 100f, "precondition: the stack really scrolls (range " + range.ToString("F0") + ")");
            var slot = sheet.Stack.SlotOfBlock(block);
            if (slot.layout.y <= range - onePixel)
                Assert.AreEqual(0f, slot.worldBound.yMin - scroll.contentViewport.worldBound.yMin, onePixel, what + ": its top meets the viewport's top");
            else
                Assert.AreEqual(range, scroll.scrollOffset.y, onePixel, what + ": a late block clamps at the end of the range");
        }

        // bar_sheet's two taps on the real layout (the tap seam: Phase B does it with a real finger): the first tap on a family that is not
        // the current one jumps and opens nothing; a second tap on it opens its kinds, one row each with its count; a pick lands that kind's
        // first block and closes the list; the reader moving on into another family closes an open list; a family with one kind never opens
        // one; a section read in More's list opens its kinds on a second tap too
        [UnityTest]
        public IEnumerator TheBarSheet_FirstTapJumps_SecondTapOnTheCurrentFamilyOpensItsKinds_APickLandsAndCloses_TheSpyLeavingCloses()
        {
            yield return ShowEntry(BuiltInNavElements.BarSheet, "big");
            var sheet = _harness.Gallery.Sheet;
            var view = (BarSheetNavView)sheet.Slots.BottomView;
            var nav = sheet.Nav;
            var model = nav.Model;
            Assert.AreEqual(0, nav.Current, "precondition: the card opens on its first family");

            // - the first tap on another family: it jumps, opens nothing, and the arrow moves to it
            view.Tap(1);
            Assert.IsFalse(view.KindsOpen, "the first tap opens no list, even before the card has landed");
            yield return Landed(nav);
            Assert.AreEqual(1, nav.Current, "the card is in the family tapped");
            Assert.IsFalse(view.KindsOpen, "still no list");
            AssertLanded(sheet, model.Sections[1].Targets[0], model.Sections[1].Key + "'s first block");
            Assert.AreEqual(DisplayStyle.Flex, view.Arrows[1].Arrow.resolvedStyle.display, "the arrow moved to the family being read");
            Assert.AreEqual(DisplayStyle.None, view.Arrows[0].Arrow.resolvedStyle.display, "and left the first");

            // - the second tap on it: its kinds, counts and all
            view.Tap(1);
            yield return CardTestInput.Settle();
            Assert.IsTrue(view.KindsOpen, "a second tap on the family being read opens its kinds");
            var kinds = model.Sections[1].Kinds;
            Assert.GreaterOrEqual(kinds.Count, 2, "precondition: Media holds several kinds");
            CollectionAssert.AreEqual(kinds.Select(k => k.Count.ToString()), view.KindRows.Select(r => r.Count.text), "one row per kind, each with its count");
            Assert.LessOrEqual(view.KindsList.worldBound.yMax, view.Bar.worldBound.yMin + CardGalleryChecks.OnePixel(view.Root), "above the bar");

            // - a pick lands the kind's first block and closes the list (stays in the same family)
            int pick = kinds.Count - 1;
            Assert.AreNotEqual(model.Sections[1].Targets[0], kinds[pick].Targets[0], "precondition: the last kind starts at another block than the family does");
            view.PickKind(pick);
            Assert.IsFalse(view.KindsOpen, "a pick closes the list at once");
            yield return Landed(nav);
            AssertLanded(sheet, kinds[pick].Targets[0], kinds[pick].Key + "'s first block");
            Assert.AreEqual(1, nav.Current, "still in the family");

            // - another element jumps while the list is open: the card's overlay owner closes it (step 8: every jump closes what is open)
            view.Tap(1);
            Assert.IsTrue(view.KindsOpen, "precondition: open again");
            nav.JumpTo(3, "test");
            Assert.IsFalse(view.KindsOpen, "a jump from anywhere closes the list");
            Assert.IsFalse(sheet.Overlay.IsOpen, "through the owner");
            yield return Landed(nav);

            // - one kind (Listen): nothing to choose, a second tap only jumps again
            int listen = model.Sections.ToList().FindIndex(s => s.Key == BuiltInFamilies.Guides);
            Assert.AreEqual(1, model.Sections[listen].Kinds.Count, "precondition: Listen is the one-kind family");
            view.Tap(listen);
            yield return Landed(nav);
            view.Tap(listen);
            yield return Landed(nav);
            Assert.IsFalse(view.KindsOpen, "a one-kind family opens no list");
            Assert.AreEqual(DisplayStyle.None, view.Arrows[listen].Arrow.resolvedStyle.display, "and wears no arrow");

            // - a section read in More's list: its row's first tap jumps, its second opens the kinds (More's list closed meanwhile)
            int overflow = view.Items.Count;
            int several = model.Sections.ToList().FindIndex(overflow, s => s.Kinds.Count >= 2);
            Assert.GreaterOrEqual(several, overflow, "precondition: the overflow holds a family with several kinds");
            view.OpenMore();
            view.Tap(several);
            yield return Landed(nav);
            Assert.AreEqual(several, nav.Current);
            Assert.IsFalse(view.MoreOpen, "a tap on a row closes More's list");
            Assert.IsFalse(view.KindsOpen, "and a first tap opens no kinds");
            view.OpenMore();
            view.Tap(several);
            yield return CardTestInput.Settle();
            Assert.IsTrue(view.KindsOpen, "the second tap, on the family being read, opens its kinds");
            Assert.AreEqual(several, view.KindsSection);
            Assert.IsFalse(view.MoreOpen, "More's list closed");
        }

        // next_up walks the card on the real layout (the Next seam; Phase B taps the real button): each Next lands the next target, the words
        // and the number move on, the block named is the one the stack draws words over, and from the last target on the bar is finished
        [UnityTest]
        public IEnumerator TheUpNextBar_FollowsTheReader_NextLandsTheNextBlock_AndTheWordsMoveOn_UntilTheCardEnds()
        {
            yield return ShowEntry(BuiltInNavElements.NextUp, "big");
            var sheet = _harness.Gallery.Sheet;
            var view = (NextUpNavView)sheet.Slots.BottomView;
            var nav = sheet.Nav;
            var model = nav.Model;
            var strings = new CardStrings(_harness.Gallery.StringTable.Entries(), CardStringSources.Shared.Entries(), null, "en", "en");
            Assert.AreEqual(62, model.TargetCount, "precondition: the big card's 62 targets");
            // - the spy line is a third down the viewport: a short first block is already past it, so the card is read at target 0 or a little on
            int at = model.Targets.ToList().IndexOf(nav.CurrentBlock);
            Assert.That(at, Is.InRange(0, 5), "precondition: a target near the top is being read (" + nav.CurrentBlock + ")");

            // - "read" is the block under the spy line (a third down the viewport): Next lands the next block at the top, and a short landed
            //   block leaves the block under it already on screen, so the reader's place can move on by more than one -- never back, and the
            //   bar always names the block after the one the controller says is read now
            IEnumerator Step(string when)
            {
                string next = model.Targets[at + 1];
                Assert.AreEqual(next, view.NextBlock, when + ": the bar names the block after the one read");
                Assert.AreEqual((at + 2) + " of 62", view.Position.text, when + ": its place among all the targets");
                string drawn = sheet.Stack.HeadingOfBlock(next);
                Assert.AreEqual(drawn.Length > 0 ? drawn : strings.Get(model.KindOf(next).LabelKey), view.Heading.text, when + ": the words over that block");
                view.GoNext();
                yield return Landed(nav);
                AssertLanded(sheet, next, when + ": " + next);
                int now = model.Targets.ToList().IndexOf(nav.CurrentBlock);
                Assert.GreaterOrEqual(now, at + 1, when + ": the reader is at the landed block, or one under the spy line a little further on");
                Assert.LessOrEqual(now, at + 4, when + ": and not far past it (" + nav.CurrentBlock + ")");
                at = now;
                if (at < model.TargetCount - 1)
                    Assert.AreEqual(model.Targets[at + 1], view.NextBlock, when + ": the bar moved on to the block after the one read now");
            }

            for (int i = 0; i < 6; i++) yield return Step("step " + (i + 1));

            // - near the end: Next until the last target lands, and the bar then says the card is finished
            Assert.IsTrue(nav.JumpToBlock(model.Targets[model.TargetCount - 6], "test"));
            yield return Landed(nav);
            at = model.Targets.ToList().IndexOf(nav.CurrentBlock);
            Assert.GreaterOrEqual(at, model.TargetCount - 6, "precondition: near the end");
            int presses = 0;
            while (!view.Finished && presses++ < 10) yield return Step("near the end, press " + presses);
            Assert.IsTrue(view.Finished, "the last target ends the bar (after " + presses + " presses)");
            Assert.AreEqual(model.Targets[model.TargetCount - 1], nav.CurrentBlock, "the card is read at its last target");
            Assert.IsNull(view.NextBlock);
            Assert.IsFalse(CardTestInput.IsShown(view.Next, sheet.Root), "no Next that goes nowhere");
            view.GoNext();
            yield return CardTestInput.Settle();
            Assert.AreEqual(model.Targets[model.TargetCount - 1], nav.CurrentBlock, "a Next at the end moves nothing");
        }

        // The dock's acceptance (_3.2 7b.1): with the Portuguese labels the pill never grows past the card (390 px), whichever section is marked
        // -- the marked word is whole and shown, and exactly one word shows at a time. Each marked state is rendered once for the widest
        [UnityTest]
        public IEnumerator TheDock_NeverGrowsPastTheCard_WhicheverSlotIsMarked_WithThePortugueseLabels()
        {
            yield return ShowEntry(BuiltInNavElements.Dock, "pt_long");
            var sheet = _harness.Gallery.Sheet;
            var dock = (DockNavView)sheet.Slots.BottomView;
            var model = sheet.Nav.Model;
            float onePixel = CardGalleryChecks.OnePixel(dock.Root);
            var card = sheet.Root.worldBound;
            Assert.AreEqual(390f, sheet.Layer.panel.visualTree.layout.width, onePixel, "precondition: the run's 390 px frame");
            var slots = dock.Items.Append(dock.More).ToList();
            float widest = 0f;
            string widestWord = "";
            for (int i = 0; i < model.Sections.Count; i++)
            {
                sheet.Nav.JumpTo(i, "test");
                yield return Landed(sheet.Nav);
                var marked = slots.Where(s => s.Button.ClassListContains(NavTab.CurrentClass)).ToList();
                Assert.AreEqual(1, marked.Count, "section " + i + ": exactly one slot marked");
                var slot = marked[0];
                // - the dock follows the SPY: after a jump lands, a short block (Listen's player) can already have the next family under the spy
                //   line, so the marked slot is the one of the section the controller names now (the real-drag check is Phase B's)
                int read = sheet.Nav.Current;
                Assert.GreaterOrEqual(read, i, "section " + i + ": the spy names the section jumped to or the next one under its line");
                Assert.AreEqual(read < dock.Items.Count ? read : dock.Items.Count, slots.IndexOf(slot), "section " + i + ": the slot of the section read, or More for the overflow");
                var pill = dock.Bar.worldBound;
                Assert.LessOrEqual(pill.width, card.width + onePixel, slot.Label.text + " marked: the pill fits the card (" + pill.width.ToString("F1") + " of " + card.width.ToString("F1") + ")");
                Assert.GreaterOrEqual(pill.xMin, card.xMin - onePixel, slot.Label.text + " marked: inside the card (left)");
                Assert.LessOrEqual(pill.xMax, card.xMax + onePixel, slot.Label.text + " marked: inside the card (right)");
                Assert.AreEqual(1, slots.Count(s => CardTestInput.IsShown(s.Label, sheet.Root)), slot.Label.text + " marked: it alone says its word");
                var words = slot.Label.MeasureTextSize(slot.Label.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                Assert.GreaterOrEqual(slot.Label.layout.width + 0.5f, words.x, "'" + slot.Label.text + "': whole, never cut");
                foreach (var each in slots)
                    Assert.GreaterOrEqual(each.Button.layout.width, CardTestInput.TokenPx("--ts-touch-target") - onePixel, each.Label.text + ": still a whole touch target wide");
                if (pill.width > widest)
                {
                    widest = pill.width;
                    widestWord = slot.Label.text;
                }
            }
            Debug.Log("[Card] Dock at 390 px in Portuguese: widest pill " + widest.ToString("F1") + " px (marked '" + widestWord + "')");
            yield return CardGalleryChecks.Render("Nav_dock_pt_long_widest");
        }

        // A menu's scrim (step 8): over the WHOLE card layer, painted in exactly --ts-scrim (read from the stylesheet, never retyped)
        private static void AssertTheScrim(PoiCardSheetView sheet)
        {
            var scrim = sheet.Overlay.Scrim;
            Assert.AreEqual(DisplayStyle.Flex, scrim.resolvedStyle.display, "the scrim is over the card");
            Assert.AreEqual(sheet.Layer.worldBound.width, scrim.worldBound.width, 0.5f, "as wide as the card layer");
            Assert.AreEqual(sheet.Layer.worldBound.height, scrim.worldBound.height, 0.5f, "as tall");
            Assert.IsTrue(CardTokenColours.TryResolve(scrim, "--ts-scrim", out Color token), "--ts-scrim resolves on the menu layer");
            Assert.AreEqual(token, scrim.resolvedStyle.backgroundColor, "painted in --ts-scrim");
            Assert.Greater(token.a, 0f, "a scrim that darkens");
        }

        // drawer_tree (step 8): opened by the header's menu button over a scrim, a drawer from the left inside the card; one line per section
        // with its words in the card's language and its count, the kinds of every section with a choice under it (each indented past its
        // section's words), the section being read marked, a title and a visible close
        private static void DrawerTreeKeepsItsPromise(CardNavGalleryHarness harness, CardNavGalleryDefinitions.Entry entry)
        {
            var sheet = harness.Gallery.Sheet;
            var drawer = (DrawerTreeNavView)sheet.Slots.MenuView;
            var model = sheet.Nav.Model;
            var strings = Strings(harness, entry);
            Assert.IsTrue(CardTestInput.IsShown(sheet.MenuButton, sheet.Root), "the header shows the menu button");
            AssertTheScrim(sheet);
            Assert.AreEqual(model.Sections.Count, drawer.SectionRows.Count, "one line per section");
            for (int i = 0; i < model.Sections.Count; i++)
            {
                Assert.AreEqual(strings.Get(model.Sections[i].LabelKey), drawer.SectionRows[i].Label.text, "its words, in " + entry.Case.Language);
                Assert.AreEqual(model.Sections[i].Count.ToString(), drawer.SectionRows[i].Count.text, "its count");
            }
            int withKinds = model.Sections.Count(sec => sec.Kinds.Count >= 2);
            Assert.AreEqual(model.Sections.Where(sec => sec.Kinds.Count >= 2).Sum(sec => sec.Kinds.Count), drawer.KindRows.Count,
                "every kind of every section with a choice of kinds (" + withKinds + " sections)");
            foreach (var (row, section, _) in drawer.KindRows)
                Assert.Greater(row.Label.worldBound.xMin, drawer.SectionRows[section].Label.worldBound.xMin - 0.5f, row.Label.text + ": indented under its section's words");
            Assert.IsTrue(drawer.SectionRows[0].Button.ClassListContains(NavTab.CurrentClass), "the first section is the one being read");
            Assert.AreEqual(FontStyle.Bold, drawer.SectionRows[0].Label.resolvedStyle.unityFontStyleAndWeight, "marked by weight too, not colour alone");
            Assert.AreEqual(strings.Get(CardStrings.Keys.NavMenu), drawer.Header.Title.text, "its title");
            Assert.IsTrue(CardTestInput.IsShown(drawer.Header.Close, sheet.Layer), "a visible close");
            Assert.Less(drawer.Root.worldBound.width, sheet.Layer.worldBound.width, "a drawer: the card shows beside it");
        }

        // fab_dial and fab_radial CLOSED (step 9, section 5.4): zero chrome until needed -- the round button alone, a touch target both ways,
        // in the floating slot right above the sheet's top edge (at Full the slot rests in the sheet's top gap, never off the screen), its
        // right edge where the card's content ends, its name the card's word for the sections; nothing else of it shows
        // edge_scrubber (step 10): its host IS the stack's viewport, flush on its right edge; one dot per section in the model's order, each in a
        // full touch-token target that tiles the centred column with no gap, the family's colour on a dark disc (>= 3:1 over a white frame
        // by the disc, >= 3:1 over a black one by its light edge); the section read marked by a LARGER disc with an accent edge, and no word
        // or bubble at rest; a finger beside the column or above / below it meets the content, never the element
        private static void EdgeScrubberKeepsItsPromise(CardNavGalleryHarness harness, CardNavGalleryDefinitions.Entry entry)
        {
            var sheet = harness.Gallery.Sheet;
            var view = (EdgeScrubberNavView)sheet.Slots.EdgeView;
            var model = sheet.Nav.Model;
            var strings = Strings(harness, entry);
            float onePixel = CardGalleryChecks.OnePixel(view.Root);
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            var viewport = sheet.Stack.Scroll.contentViewport;

            // - the host lies over the viewport itself, flush on its right edge, one touch target wide
            Assert.AreSame(viewport, sheet.Slots.Edge.parent, "the host is a child of the stack's viewport");
            Assert.AreEqual(viewport.worldBound.yMin, sheet.Slots.Edge.worldBound.yMin, onePixel, "from the viewport's top");
            Assert.AreEqual(viewport.worldBound.yMax, sheet.Slots.Edge.worldBound.yMax, onePixel, "to its bottom");
            Assert.AreEqual(viewport.worldBound.xMax, sheet.Slots.Edge.worldBound.xMax, onePixel, "flush on its right edge");
            Assert.AreEqual(touch, sheet.Slots.Edge.worldBound.width, onePixel, "one touch target wide");

            // - one dot per section, a full touch target each, tiling a column centred in the viewport
            Assert.IsTrue(view.IsShown, "the viewport holds every target: " + model.Sections.Count + " x " + touch + " in " + viewport.layout.height);
            Assert.AreEqual(model.Sections.Count, view.Items.Count, "one dot per section");
            var expected = EdgeScrubberRule.Layout(model.Sections.Count, viewport.layout.height, touch);
            Assert.AreEqual(expected.Top, view.Column.layout.y, onePixel, "the column is centred in the viewport");
            Assert.AreEqual(expected.Height, view.Column.layout.height, onePixel, "and holds every target");
            for (int i = 0; i < view.Items.Count; i++)
            {
                var item = view.Items[i];
                var section = model.Sections[i];
                Assert.AreEqual(touch, item.Button.layout.width, onePixel, item.Words + ": the tap target is as wide as the touch token");
                Assert.AreEqual(touch, item.Button.layout.height, onePixel, item.Words + ": and as tall");
                Assert.AreEqual(view.Column.worldBound.yMin + i * touch, item.Button.worldBound.yMin, onePixel, item.Words + ": the targets tile, no gap");
                Assert.AreEqual(strings.Get(section.LabelKey), item.Words, item.Words + ": its section's word, in " + entry.Case.Language);
                Assert.AreEqual(item.Words, item.Button.tooltip, item.Words + ": the dot is named for a long press");
                AssertFamilyPaint(item.Fill.resolvedStyle.backgroundColor, section.ColorToken, item.Fill, item.Words + "'s dot");
                // - the dot is drawn well inside its target (small to look at, whole to tap)
                Assert.Less(item.Disc.layout.width, touch - onePixel, item.Words + ": the disc is smaller than its target");
                var centre = item.Button.worldBound.center;
                Assert.IsTrue(item.Button.worldBound.Contains(item.Disc.worldBound.center), item.Words + ": the disc sits in its target");
                // - a finger on the dot meets the dot's own target, nothing over it
                var picked = sheet.Layer.panel.Pick(centre);
                Assert.IsTrue(picked == item.Button || (picked != null && item.Button.Contains(picked)), item.Words + ": the topmost thing at its centre is its own target");
            }
            Assert.AreEqual(view.Items[0].Button.worldBound.yMin, view.Column.worldBound.yMin, onePixel, "the first target starts the column");

            // - contrast of a dot against a light and a dark block behind it (non-text, WCAG 1.4.11): the dark disc over white, its light edge over black
            var disc = view.Items[1].Disc;
            Assert.GreaterOrEqual(UIAccessibility.ContrastRatio(disc.resolvedStyle.backgroundColor, Color.white), UIAccessibility.MinRatioLargeTextOrUIComponent,
                "the dark disc reads over a white block");
            var edge = disc.resolvedStyle.borderTopColor;
            Assert.GreaterOrEqual(UIAccessibility.ContrastRatio(new Color(edge.r * edge.a, edge.g * edge.a, edge.b * edge.a, 1f), Color.black), UIAccessibility.MinRatioLargeTextOrUIComponent,
                "its light edge reads over a black block");

            // - the section read is marked twice: a larger disc AND a larger colour disc with an accent edge; no other dot, no bubble at rest
            var first = view.Items[0];
            Assert.IsTrue(first.Button.ClassListContains(EdgeScrubberNavView.CurrentClass), "the first section is the one read at the top");
            for (int i = 1; i < view.Items.Count; i++)
                Assert.IsFalse(view.Items[i].Button.ClassListContains(EdgeScrubberNavView.CurrentClass), view.Items[i].Words + ": not the one read");
            Assert.Greater(first.Disc.layout.width, view.Items[1].Disc.layout.width, "the current disc is larger");
            Assert.Greater(first.Fill.layout.width, view.Items[1].Fill.layout.width, "and so is its colour");
            Assert.AreNotEqual(view.Items[1].Disc.resolvedStyle.borderTopColor, first.Disc.resolvedStyle.borderTopColor, "with an edge of its own");
            Assert.IsTrue(CardTokenColours.TryResolve(first.Button, "--ts-accent", out Color accent), "the accent resolves on the card");
            Assert.AreEqual(accent, first.Disc.resolvedStyle.borderTopColor, "the accent edge");
            Assert.IsFalse(view.Bubble.ClassListContains(EdgeScrubberNavView.BubbleShownClass), "no bubble at rest");
            Assert.AreEqual(DisplayStyle.None, view.Bubble.resolvedStyle.display, "and it takes no room");
            Assert.AreEqual(-1, view.Pointed, "no finger on a dot");

            // - a finger beside the column or above / below it meets the content under it, never the element: the card still scrolls there
            float x = view.Items[0].Button.worldBound.center.x;
            Assert.IsFalse(view.Root.Contains(sheet.Layer.panel.Pick(new Vector2(viewport.worldBound.xMax - touch - 4f, view.Column.worldBound.center.y))),
                "left of the column: the content");
            if (view.Column.worldBound.yMin - viewport.worldBound.yMin > 8f)
                Assert.IsFalse(view.Root.Contains(sheet.Layer.panel.Pick(new Vector2(x, view.Column.worldBound.yMin - 4f))), "above the column: the content");
            if (viewport.worldBound.yMax - view.Column.worldBound.yMax > 8f)
                Assert.IsFalse(view.Root.Contains(sheet.Layer.panel.Pick(new Vector2(x, view.Column.worldBound.yMax + 4f))), "below the column: the content");
        }

        private static void FloatingButtonKeepsItsPromise(CardNavGalleryHarness harness, CardNavGalleryDefinitions.Entry entry)
        {
            var sheet = harness.Gallery.Sheet;
            var view = sheet.Slots.FloatingView;
            var fab = view is FabDialNavView dial ? dial.Fab : ((FabRadialNavView)view).Fab;
            float onePixel = CardGalleryChecks.OnePixel(fab.Root);
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            Assert.IsFalse(fab.IsOpen, "closed: the sections icon, not the X");
            Assert.AreEqual(touch, fab.Button.layout.width, onePixel, "the button is one touch target wide");
            Assert.AreEqual(touch, fab.Button.layout.height, onePixel, "and tall");
            var slot = sheet.Floating.Root.worldBound;
            Assert.AreEqual(sheet.Root.worldBound.yMin, slot.yMax, onePixel, "at Full the slot rests flush on the sheet's top edge");
            Assert.GreaterOrEqual(slot.yMin, sheet.Layer.worldBound.yMin - onePixel, "inside the card layer, never off the screen");
            Assert.AreEqual(sheet.Layer.worldBound.xMax - CardTestInput.TokenPx("--ts-space-4"), fab.Root.worldBound.xMax, onePixel,
                "its right edge where the card's content ends");
            Assert.AreEqual(Strings(harness, entry).Get(CardStrings.Keys.NavFab), fab.Button.tooltip, "its name: the card's word for the sections");
            var shown = fab.Root.Query<Button>().ToList().Where(b => CardTestInput.IsShown(b, sheet.Layer)).ToList();
            CollectionAssert.AreEqual(new[] { fab.Button }, shown, "closed: the button alone, zero chrome until needed");
        }

        // The floating button OPEN, opened by a real tap on it (step 9): every one of its items inside the card layer, a whole touch target,
        // its words whole and in the case's language, never two overlapping; the dial lists every section with its icon beside its word, the
        // radial holds Bar Slots items (More last when the sections overflow) on its arc; rendered Nav_<key>_<case>_open
        [UnityTest]
        public IEnumerator TheFloatingButton_ARealTapOpensIt_EveryItemWholeInsideTheLayerAndApart_AndRenders(
            [Values(BuiltInNavElements.FabDial, BuiltInNavElements.FabRadial)] string key, [Values("big", "small", "pt_long", "two_families")] string caseKey)
        {
            yield return ShowEntry(key, caseKey);
            var entry = CardNavGalleryDefinitions.All.First(e => e.ElementKey == key && e.Case.Key == caseKey);
            var sheet = _harness.Gallery.Sheet;
            var model = sheet.Nav.Model;
            var strings = Strings(_harness, entry);
            var view = sheet.Slots.FloatingView;
            var fab = view is FabDialNavView d ? d.Fab : ((FabRadialNavView)view).Fab;
            yield return CardTestInput.Tap(fab.Button.panel, fab.Button.worldBound.center);
            yield return CardTestInput.Settle();
            Assert.IsTrue(fab.IsOpen, "a real tap opened it (the button shows the X)");
            Assert.AreEqual(key == BuiltInNavElements.FabDial ? FabDialNavView.ListKey : FabRadialNavView.ArcKey, sheet.Overlay.OpenKey,
                "the one thing open over the card");

            float onePixel = CardGalleryChecks.OnePixel(fab.Root);
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            var layer = sheet.Layer.worldBound;
            var items = new List<(Button Button, Label Label)>();
            if (view is FabDialNavView dial)
            {
                Assert.AreEqual(FabFanRule.Direction.Down, dial.Direction, "at Full the button rests at the top: the column opens down over the card");
                Assert.AreEqual(model.Sections.Count, dial.Items.Count, "a pill per section");
                for (int i = 0; i < dial.Items.Count; i++)
                {
                    var item = dial.Items[i];
                    Assert.AreEqual(strings.Get(model.Sections[i].LabelKey), item.Label.text, "its words, in " + entry.Case.Language);
                    Assert.LessOrEqual(item.Icon.worldBound.xMax, item.Label.worldBound.xMin + 0.5f, item.Label.text + ": its icon beside its word");
                    items.Add((item.Button, item.Label));
                }
                Assert.GreaterOrEqual(dial.List.worldBound.yMin, fab.Root.worldBound.yMax - onePixel, "under the button");
            }
            else
            {
                var radial = (FabRadialNavView)view;
                // - a page holds what Bar Slots allows and the room fits (at Full, in the screen's corner, fewer), More last while sections are left
                Assert.LessOrEqual(radial.Capacity, NavOntologyRule.MaxSlots(ConfigNav(entry)), "never more than Bar Slots");
                int shown = NavOntologyRule.SectionsInBar(model.Sections.Count, radial.Capacity);
                Assert.AreEqual(shown + (shown < model.Sections.Count ? 1 : 0), radial.Items.Count, "a page of the arc: rule 2.5.5 over its capacity");
                Assert.IsTrue(radial.Layout.Fits, "the arc fits the layer (" + radial.Capacity + " items at radius " + radial.Layout.Radius + ")");
                foreach (var item in radial.Items)
                {
                    string words = item.Section >= 0 ? strings.Get(model.Sections[item.Section].LabelKey) : strings.Get(CardStrings.Keys.NavMore);
                    Assert.AreEqual(words, item.Label.text, "its words, in " + entry.Case.Language);
                    Assert.LessOrEqual(item.Button.worldBound.center.x, fab.Root.worldBound.center.x + onePixel, item.Label.text + ": its centre left of the button's (the arc's half-plane)");
                    items.Add((item.Button, item.Label));
                }
            }
            for (int i = 0; i < items.Count; i++)
            {
                var (button, label) = items[i];
                Assert.IsTrue(CardTestInput.IsShown(button, sheet.Layer), label.text + ": shown");
                Assert.GreaterOrEqual(button.layout.height, touch - onePixel, label.text + ": tall enough to tap");
                Assert.GreaterOrEqual(button.layout.width, touch - onePixel, label.text + ": wide enough to tap");
                Assert.IsTrue(layer.Contains(button.worldBound.min) && layer.Contains(button.worldBound.max - Vector2.one * onePixel),
                    label.text + ": whole inside the card layer " + button.worldBound + " in " + layer);
                var size = label.MeasureTextSize(label.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                Assert.GreaterOrEqual(label.layout.width + 0.5f, size.x, "'" + label.text + "': whole, never cut");
                float contrast = CardTestInput.Contrast(label.resolvedStyle.color, CardTestInput.EffectiveBackground(label));
                Assert.GreaterOrEqual(contrast, UIAccessibility.MinRatioNormalText, "'" + label.text + "' reads at 4.5:1 (" + contrast.ToString("F2") + ")");
                for (int j = i + 1; j < items.Count; j++)
                {
                    var a = items[i].Button.worldBound;
                    var b = items[j].Button.worldBound;
                    var shrunk = new Rect(a.x + onePixel, a.y + onePixel, a.width - 2 * onePixel, a.height - 2 * onePixel);
                    Assert.IsFalse(shrunk.Overlaps(b), label.text + " and " + items[j].Label.text + ": never overlapping " + a + " / " + b);
                }
            }
            yield return CardGalleryChecks.Render("Nav_" + key + "_" + caseKey + "_open");

            // - the radial's next page: More turns the arc to the next sections (rendered too)
            if (view is FabRadialNavView arc && arc.Items.Last().Section < 0)
            {
                var more = arc.Items.Last();
                int next = arc.Items.Count - 1;
                yield return CardTestInput.Tap(more.Button.panel, more.Button.worldBound.center);
                yield return CardTestInput.Settle();
                Assert.IsTrue(arc.ShowsMore, "a real tap on More shows the next page");
                Assert.AreEqual(next, arc.PageStart, "starting with the first section the last page did not show");
                int onPage = NavOntologyRule.SectionsInBar(model.Sections.Count - next, arc.Capacity);
                CollectionAssert.AreEqual(model.Sections.Skip(next).Take(onPage).Select(sec => strings.Get(sec.LabelKey)),
                    arc.Items.Where(i => i.Section >= 0).Select(i => i.Label.text), "exactly the next sections, in order");
                Assert.IsTrue(arc.Layout.Fits, "the next page fits too");
                yield return CardGalleryChecks.Render("Nav_" + key + "_" + caseKey + "_more");
            }

            yield return CardTestInput.Tap(fab.Button.panel, fab.Button.worldBound.center);
            yield return CardTestInput.Settle();
            Assert.IsFalse(fab.IsOpen, "a real tap on its X closes it");
            Assert.IsFalse(sheet.Overlay.IsOpen);
        }

        // grid_overlay (step 8): big tiles over a scrim, inside the card, two to a row, one per section with its words and count, each at
        // least two touch targets tall; the Purpose / Format switch on the wall's axis; the Format side lists the model's formats
        private static void GridOverlayKeepsItsPromise(CardNavGalleryHarness harness, CardNavGalleryDefinitions.Entry entry)
        {
            var sheet = harness.Gallery.Sheet;
            var grid = (GridOverlayNavView)sheet.Slots.MenuView;
            var model = sheet.Nav.Model;
            var strings = Strings(harness, entry);
            Assert.IsTrue(CardTestInput.IsShown(sheet.MenuButton, sheet.Root), "the header shows the menu button");
            AssertTheScrim(sheet);
            Assert.AreEqual(model.Sections.Count, grid.Tiles.Count, "a tile per section");
            float tile = CardTestInput.TokenPx("--ts-grid-tile-height");
            foreach (var t in grid.Tiles)
            {
                Assert.AreEqual(strings.Get(model.Sections[t.Index].LabelKey), t.Label.text, "its words, in " + entry.Case.Language);
                Assert.GreaterOrEqual(t.Button.layout.height, tile - 0.5f, t.Label.text + ": a big thumb target");
            }
            if (grid.Tiles.Count >= 2)
                Assert.AreEqual(grid.Tiles[0].Button.worldBound.yMin, grid.Tiles[1].Button.worldBound.yMin, 0.5f, "two tiles to a row");
            Assert.IsTrue(grid.ByPurpose.ClassListContains(GridOverlayNavView.OptionCurrentClass), "it opens on the wall's axis, Purpose");
            Assert.AreEqual(strings.Get(CardStrings.Keys.NavMenuByPurpose), grid.ByPurpose.text);
            Assert.AreEqual(strings.Get(CardStrings.Keys.NavMenuByFormat), grid.ByFormat.text);
            grid.ShowAxis(CardOptions.GroupByFormat);
            CollectionAssert.AreEqual(model.By(CardOptions.GroupByFormat).Sections.Select(sec => strings.Get(sec.LabelKey)), grid.Tiles.Select(t => t.Label.text),
                "the Format side: the model's formats");
            grid.ShowAxis(CardOptions.GroupByFamily);
        }
    }
}
