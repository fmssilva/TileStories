using System.Collections;
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
    // Phase A of the visitor's language choice (_3.1 15.3.2, 40-testing.md 4.4): the language chip beside the X on the real gallery card,
    // driven by REAL pointer events. The gallery wall offers English + Portuguese; the harness re-shows the entry in the language the chip
    // names (the wall's card does the same through PoiCardHost, proven in PoiCardLanguageSceneTests). Also the fit: the title wraps before the chip.
    public class CardLanguageChipGalleryTests
    {
        private const string ScenePath = "Assets/Dev/CardGallery/CardGalleryScene.unity";
        private CardGalleryHarness _harness;

        private static string[] HeaderEntryNames => CardGalleryDefinitions.All.Where(e => e.IsHeader).Select(e => e.Name).ToArray();

        private static int IndexOf(string name) => CardGalleryDefinitions.All.ToList().FindIndex(e => e.Name == name);

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
                _harness = Object.FindFirstObjectByType<CardGalleryHarness>();
                yield return null;
            }
            Assert.IsNotNull(_harness, "the gallery scene holds CardGalleryHarness");
            _harness.EnsureBuilt();
            // - the gallery's own wall is English only: these tests give it a second language, like a bilingual wall
            _harness.Languages = new[] { "en", "pt" };
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            StatusRamp.ResetToDefaults();
            CategoryPalette.ClearOverrides();
            MarkerHierarchyResolver.ResetToDefaults();
            var scene = SceneManager.GetSceneByPath(ScenePath);
            SceneManager.SetActiveScene(SceneManager.CreateScene("AfterCardLanguageChipTest"));
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
        }

        private PoiCardSheetView Sheet => _harness.Sheet;

        [UnityTest]
        public IEnumerator TheChip_NamesTheOtherLanguage_IsFingerSized_Readable_AndClearOfTheX()
        {
            _harness.Show(IndexOf(HeaderEntryNames[0]));
            yield return CardTestInput.Settle();

            var chip = Sheet.LanguageButton;
            Assert.AreEqual(DisplayStyle.Flex, chip.resolvedStyle.display, "two languages: the chip shows");
            Assert.AreEqual("PT", chip.text, "an English card offers Portuguese");
            Assert.AreEqual("Language", chip.tooltip, "its name comes from the card's texts");
            Assert.IsTrue(Sheet.Root.ClassListContains("poi-card--has-language"));
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(chip.worldBound.width, chip.worldBound.height), "a finger-sized chip: " + chip.worldBound);
            float onePixel = CardGalleryChecks.OnePixel(chip);
            Assert.AreEqual(CardTestInput.TokenPx("--ts-touch-target"), chip.worldBound.width, onePixel + 0.01f, "as wide as the touch-target token");
            Assert.GreaterOrEqual(CardTestInput.Contrast(chip.resolvedStyle.color, CardTestInput.EffectiveBackground(chip)), 4.5f, "readable (WCAG AA)");

            Rect close = Sheet.CloseButton.worldBound, chipRect = chip.worldBound, card = Sheet.Root.worldBound;
            Assert.LessOrEqual(chipRect.xMax, close.xMin + 0.01f, "the chip sits left of the X, never over it: " + chipRect + " vs " + close);
            Assert.GreaterOrEqual(chipRect.xMin, card.xMin, "inside the card");
            Assert.AreEqual(close.yMin, chipRect.yMin, onePixel + 0.01f, "level with the X");
        }

        [UnityTest]
        public IEnumerator ARealTap_ShowsTheCardInTheOtherLanguage_AndAnotherTapBringsItBack()
        {
            _harness.Show(IndexOf(HeaderEntryNames[0]));
            yield return CardTestInput.Settle();
            var chip = Sheet.LanguageButton;
            Assert.AreEqual("en", _harness.Language, "precondition: the gallery starts in English");
            Assert.AreEqual("Close", Sheet.CloseButton.tooltip, "precondition: the X is named in English");

            yield return CardTestInput.Tap(chip.panel, chip.worldBound.center);
            yield return CardTestInput.Settle();
            Assert.AreEqual("pt", _harness.Language, "a real tap on the chip switched the card to Portuguese");
            Assert.AreEqual("Fechar", Sheet.CloseButton.tooltip, "the card's own words follow (the X is named in Portuguese)");
            Assert.AreEqual("EN", Sheet.LanguageButton.text, "and the chip now offers English");
            Assert.AreEqual("Idioma", Sheet.LanguageButton.tooltip);

            yield return CardTestInput.Tap(Sheet.LanguageButton.panel, Sheet.LanguageButton.worldBound.center);
            yield return CardTestInput.Settle();
            Assert.AreEqual("en", _harness.Language, "a second tap: back to English");
            Assert.AreEqual("Close", Sheet.CloseButton.tooltip);
            Assert.AreEqual("PT", Sheet.LanguageButton.text);
        }

        [UnityTest]
        public IEnumerator AWallWithOneLanguage_HasNoChip_AndTheHeaderKeepsItsRoom()
        {
            var entry = CardGalleryDefinitions.All[IndexOf(HeaderEntryNames[0])];
            var wall = CardGalleryDefinitions.Taxonomy();
            wall.card_settings.languages = new System.Collections.Generic.List<string> { "en" };
            _harness.ShowPoi(CardGalleryDefinitions.Poi(entry), wall, entry.Viewer, entry.Stop);
            yield return CardTestInput.Settle();

            Assert.AreEqual(DisplayStyle.None, Sheet.LanguageButton.resolvedStyle.display, "nothing to choose between: no chip");
            Assert.IsFalse(Sheet.Root.ClassListContains("poi-card--has-language"), "and the header does not keep room for it");
            var header = (HeaderBlockView)Sheet.Stack.BoundViews[0];
            var title = header.Root.Q<Label>("card-header-title");
            Assert.LessOrEqual(title.worldBound.xMax, Sheet.CloseButton.worldBound.xMin + 0.5f, "the title still wraps before the X");
        }

        [UnityTest]
        public IEnumerator EveryHeaderEntry_TheTitleWrapsBeforeTheChip([ValueSource(nameof(HeaderEntryNames))] string name)
        {
            _harness.Show(IndexOf(name));
            yield return CardTestInput.Settle();
            var header = (HeaderBlockView)Sheet.Stack.BoundViews[0];
            var title = header.Root.Q<Label>("card-header-title");
            Assert.AreEqual(DisplayStyle.Flex, Sheet.LanguageButton.resolvedStyle.display, "precondition: the chip shows on " + name);
            Assert.LessOrEqual(title.worldBound.xMax, Sheet.LanguageButton.worldBound.xMin + 0.5f,
                name + ": the title wraps before the chip, not under it: " + title.worldBound + " vs " + Sheet.LanguageButton.worldBound);
        }

        [UnityTest]
        public IEnumerator TheChipRender_IsSavedForTheVisionPass()
        {
            _harness.Show(IndexOf(HeaderEntryNames[0]));
            yield return CardTestInput.Settle();
            yield return CardGalleryChecks.Render("Card_language_chip_en");
            var chip = Sheet.LanguageButton;
            yield return CardTestInput.Tap(chip.panel, chip.worldBound.center);
            yield return CardTestInput.Settle();
            yield return CardGalleryChecks.Render("Card_language_chip_pt");
        }
    }
}
