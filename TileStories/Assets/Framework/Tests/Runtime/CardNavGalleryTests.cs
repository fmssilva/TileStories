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

        private static string[] EntryNames => CardNavGalleryDefinitions.All.Select(e => e.Name).ToArray();

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
        };

        // The element an entry's slot holds now, its key and its slot element, and whether the chrome shows it
        private static bool IsBottom(CardNavGalleryDefinitions.Entry entry) =>
            NavElementRegistry.Shared.TryGet(entry.ElementKey, out var d) && d.Slot == NavSlotKeys.Bottom;

        private static INavElementView ViewOf(PoiCardSheetView sheet, CardNavGalleryDefinitions.Entry entry) =>
            IsBottom(entry) ? sheet.Slots.BottomView : sheet.Slots.TopView;

        private static string KeyOf(PoiCardSheetView sheet, CardNavGalleryDefinitions.Entry entry) =>
            IsBottom(entry) ? sheet.Slots.BottomKey : sheet.Slots.TopKey;

        private static bool ChromeShows(PoiCardSheetView sheet, CardNavGalleryDefinitions.Entry entry) =>
            IsBottom(entry) ? sheet.Chrome.BottomNavShown : sheet.Chrome.TopShown;

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
            Assert.AreEqual(CardNavGalleryDefinitions.ElementKeys.Count * 5, CardNavGalleryDefinitions.All.Count);
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
            Assert.IsNull(IsBottom(entry) ? sheet.Slots.TopView : sheet.Slots.BottomView, "and the other slot nothing: one element judged alone");
            var root = ViewOf(sheet, entry).Root;
            Assert.IsTrue(CardTestInput.IsShown(root, sheet.Root), "shown at Full");
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
            var buttons = root.Query<Button>().ToList().Where(b => CardTestInput.IsShown(b, sheet.Root)).ToList();
            Assert.IsNotEmpty(buttons, "the element offers something to tap");
            foreach (var button in buttons)
            {
                Assert.GreaterOrEqual(button.layout.height, touch - onePixel, button.name + ": tall enough to tap");
                Assert.GreaterOrEqual(button.layout.width, touch - onePixel, button.name + ": wide enough to tap");
            }
            // - only words a visitor sees (Labels = icon hides a tab's words; a progress piece with an icon hides its words): a label that
            //   wraps (a segment) must fit its LONGEST word, one that does not, its whole text
            foreach (var label in root.Query<Label>().ToList().Where(l => l.text.Length > 0 && CardTestInput.IsShown(l, sheet.Root)))
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
    }
}
