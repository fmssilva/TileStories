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
    // isolated gallery scene (Assets/Dev/CardNavGallery). Every element, whatever it is: it is the wall's pick in the top slot, it builds
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
        };

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
        // families and a Sources block, the small one 6 in 3, one with a single family
        [Test]
        public void TheList_IsEveryElementOverTheFourFixtureCases()
        {
            CollectionAssert.AreEquivalent(new[] { "big", "small", "one_family", "pt_long" }, CardNavGalleryDefinitions.Cases.Select(c => c.Key));
            Assert.AreEqual(CardNavGalleryDefinitions.ElementKeys.Count * 4, CardNavGalleryDefinitions.All.Count);
            CollectionAssert.IsSubsetOf(CardNavGalleryDefinitions.ElementKeys, NavElementRegistry.Shared.All.Select(e => e.Key), "every listed element is registered");
            foreach (var (key, targets, sections) in new[] { ("big", 62, 8), ("small", 6, 3), ("one_family", 5, 1), ("pt_long", 62, 8) })
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
            Assert.AreEqual(entry.ElementKey, sheet.Slots.TopKey, "the slot shows the wall's pick");
            var root = sheet.Slots.TopView.Root;
            Assert.IsTrue(CardTestInput.IsShown(root, sheet.Root), "shown at Full");

            // - inside the card at the phone width
            float onePixel = CardGalleryChecks.OnePixel(root);
            var card = sheet.Root.worldBound;
            Assert.AreEqual(sheet.Layer.panel.visualTree.layout.width, 390f, onePixel, "precondition: the run's 390 px frame");
            Assert.GreaterOrEqual(root.worldBound.xMin, card.xMin - onePixel, "the element starts inside the card");
            Assert.LessOrEqual(root.worldBound.xMax, card.xMax + onePixel, "and ends inside it");

            // - every target at least the touch token, every word whole and readable
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            var buttons = root.Query<Button>().ToList();
            Assert.IsNotEmpty(buttons, "the element offers something to tap");
            foreach (var button in buttons)
            {
                Assert.GreaterOrEqual(button.layout.height, touch - onePixel, button.name + ": tall enough to tap");
                Assert.GreaterOrEqual(button.layout.width, touch - onePixel, button.name + ": wide enough to tap");
            }
            foreach (var label in root.Query<Label>().ToList().Where(l => l.text.Length > 0))
            {
                var words = label.MeasureTextSize(label.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                Assert.GreaterOrEqual(label.layout.width + 0.5f, words.x, "'" + label.text + "': the whole word fits, never cut");
                float contrast = CardTestInput.Contrast(label.resolvedStyle.color, CardTestInput.EffectiveBackground(label));
                Assert.GreaterOrEqual(contrast, UIAccessibility.MinRatioNormalText, "'" + label.text + "' reads at 4.5:1 (" + contrast.ToString("F2") + ")");
            }

            Assert.IsTrue(ElementChecks.ContainsKey(entry.ElementKey), "an element of the list has its own check: " + entry.ElementKey);
            ElementChecks[entry.ElementKey](_harness, entry);
            yield return CardGalleryChecks.Render(name);
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
    }
}
