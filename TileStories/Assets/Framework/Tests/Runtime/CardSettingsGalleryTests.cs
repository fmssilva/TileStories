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
    // Phase A of the visitor's settings (_3.3 P4.4b; the language chip of _3.1 15.3.2 before): the Aa button beside the X on the real gallery
    // card and the panel it opens, driven by REAL pointer events. The gallery wall offers English + Portuguese (one and three languages in a
    // test each); the harness re-shows the entry in the language picked (the wall's card does the same through PoiCardHost, and the mode
    // switch, proven in PoiCardThemeSceneTests). Also the fit: the title ends before the buttons, on three structurally different headers.
    public class CardSettingsGalleryTests
    {
        private const string ScenePath = "Assets/Dev/CardGallery/CardGalleryScene.unity";
        private CardGalleryHarness _harness;

        private static string[] HeaderEntryNames => CardGalleryDefinitions.All.Where(e => e.IsHeader).Select(e => e.Name).ToArray();

        // The long title (the one that wraps) on a header with its chip row (compact), without it (text_only), and under a picture
        // (image_parallax: the hero opens the scroll, the title row is pinned above it)
        private static readonly string[] HeaderStructures = { "header_compact_long_peek", "header_text_only_long_full", "header_image_parallax_long_full" };

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
            SceneManager.SetActiveScene(SceneManager.CreateScene("AfterCardSettingsTest"));
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
        }

        private PoiCardSheetView Sheet => _harness.Sheet;

        [UnityTearDown]
        public IEnumerator RestoreTheMode()
        {
            ThemeController.SetMode("");
            yield return null;
        }

        // Open the Aa settings with a real tap and wait for them
        private IEnumerator OpenSettings()
        {
            var aa = Sheet.SettingsButton;
            yield return CardTestInput.Tap(aa.panel, aa.worldBound.center);
            yield return null;
            Assert.AreEqual(PoiCardSheetView.SettingsKey, Sheet.Overlay.OpenKey, "a real tap on Aa opened the visitor's settings");
        }

        // A real tap on one choice of the open settings
        private IEnumerator TapChoice(string value)
        {
            var choice = Sheet.Settings.Choice(value);
            Assert.IsNotNull(choice, "the settings offer " + value);
            yield return CardTestInput.Tap(choice.panel, choice.worldBound.center);
            yield return CardTestInput.Settle();
        }

        [UnityTest]
        public IEnumerator TheSettingsButton_IsAGear_FingerSized_Readable_AndLevelWithAndLeftOfTheX()
        {
            _harness.Show(IndexOf(HeaderEntryNames[0]));
            yield return CardTestInput.Settle();

            var aa = Sheet.SettingsButton;
            Assert.AreEqual(DisplayStyle.Flex, aa.resolvedStyle.display, "the settings button shows");
            // - P4.4b part 2: a gear, the universal settings sign (no "Aa" words); its name comes from the card's texts
            Assert.AreEqual("", aa.text, "no words on it: the gear says it");
            Assert.AreEqual(CardIcons.Shape.Gear, Sheet.SettingsGlyph.Kind, "the drawn mark is the gear");
            Assert.AreSame(aa, Sheet.SettingsGlyph.parent, "drawn on the button");
            Assert.AreEqual(CardTestInput.TokenPx("--ts-icon-size"), Sheet.SettingsGlyph.worldBound.width, CardGalleryChecks.OnePixel(aa) + 0.01f, "the X's mark size");
            Assert.IsTrue(aa.worldBound.Contains(Sheet.SettingsGlyph.worldBound.center), "centred in the button");
            Assert.AreEqual("Card settings", aa.tooltip, "its name for a screen reader and a long press");
            Assert.IsTrue(Sheet.Root.ClassListContains("poi-card--has-settings"));
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(aa.worldBound.width, aa.worldBound.height), "finger-sized: " + aa.worldBound);
            float onePixel = CardGalleryChecks.OnePixel(aa);
            Assert.AreEqual(CardTestInput.TokenPx("--ts-touch-target"), aa.worldBound.width, onePixel + 0.01f, "as wide as the touch-target token");
            Assert.GreaterOrEqual(CardTestInput.Contrast(Sheet.SettingsGlyph.resolvedStyle.color, CardTestInput.EffectiveBackground(aa)), 7f, "the gear reads at 7:1 (D1)");

            Rect close = Sheet.CloseButton.worldBound, aaRect = aa.worldBound, card = Sheet.Root.worldBound;
            Assert.LessOrEqual(aaRect.xMax, close.xMin + 0.01f, "Aa sits left of the X, never over it: " + aaRect + " vs " + close);
            Assert.GreaterOrEqual(aaRect.xMin, card.xMin, "inside the card");
            Assert.AreEqual(close.yMin, aaRect.yMin, onePixel + 0.01f, "level with the X");
        }

        [UnityTest]
        public IEnumerator RealTaps_OpenTheSettings_PickTheOtherLanguage_AndBack()
        {
            _harness.Show(IndexOf(HeaderEntryNames[0]));
            yield return CardTestInput.Settle();
            Assert.AreEqual("en", _harness.Language, "precondition: the gallery starts in English");

            yield return OpenSettings();
            Assert.IsTrue(Sheet.Settings.Choice("en").ClassListContains(CardVisitorSettingsView.ChosenClass), "the shown language is marked");
            // - P4.4b part 2: each language by its own speakers' name, the same in every card language
            Assert.AreEqual("English", Sheet.Settings.Choice("en").text, "English names itself");
            Assert.AreEqual("Português", Sheet.Settings.Choice("pt").text, "and Portuguese itself, with its accent");
            Assert.AreEqual("Card settings", Sheet.Settings.Root.Q<Label>(className: "card-settings__title").text, "the panel's title is the button's name");
            Assert.IsTrue(Sheet.Settings.Choice(ThemeController.CurrentMode).ClassListContains(CardVisitorSettingsView.ChosenClass), "the shown mode is marked");
            foreach (var choice in Sheet.Settings.Root.Query<Button>(className: CardVisitorSettingsView.ChoiceClass).ToList())
                Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(choice.worldBound.width, choice.worldBound.height), choice.name + " is finger-sized");

            yield return TapChoice("pt");
            Assert.AreEqual("pt", _harness.Language, "a real tap on PT switched the card to Portuguese");
            Assert.AreEqual("Fechar", Sheet.CloseButton.tooltip, "the card's own words follow");

            yield return OpenSettings();
            Assert.AreEqual("Idioma", Sheet.Settings.Root.Q<Label>(className: "card-settings__group-title").text, "the settings speak Portuguese too");
            Assert.AreEqual("Definições do cartão", Sheet.Settings.Root.Q<Label>(className: "card-settings__title").text, "its title too");
            Assert.AreEqual("Definições do cartão", Sheet.SettingsButton.tooltip, "and the gear's name");
            Assert.AreEqual("English", Sheet.Settings.Choice("en").text, "a language keeps its own name in a Portuguese card");
            Assert.AreEqual("Português", Sheet.Settings.Choice("pt").text);
            yield return TapChoice("en");
            Assert.AreEqual("en", _harness.Language, "and back to English");
            Assert.AreEqual("Close", Sheet.CloseButton.tooltip);
        }

        [UnityTest]
        public IEnumerator AWallWithOneLanguage_KeepsTheAaButton_ItsSettingsHoldOnlyTheModes()
        {
            var entry = CardGalleryDefinitions.All[IndexOf(HeaderEntryNames[0])];
            var wall = CardGalleryDefinitions.Taxonomy();
            wall.card_settings.languages = new System.Collections.Generic.List<string> { "en" };
            _harness.ShowPoi(CardGalleryDefinitions.Poi(entry), wall, entry.Viewer, entry.Stop);
            yield return CardTestInput.Settle();

            Assert.AreEqual(DisplayStyle.Flex, Sheet.SettingsButton.resolvedStyle.display, "one language: the mode is still a choice, Aa stays");
            yield return OpenSettings();
            Assert.AreEqual(DisplayStyle.None, Sheet.Settings.Choice("en").parent.parent.resolvedStyle.display, "nothing to choose between: no Language group");
            foreach (string mode in CardOptions.Modes)
                Assert.AreEqual(DisplayStyle.Flex, Sheet.Settings.Choice(mode).resolvedStyle.display, "the " + mode + " choice shows");
        }

        [UnityTest]
        public IEnumerator AWallWithThreeLanguages_ListsEach_AndARealTapPicksAny()
        {
            _harness.Languages = new[] { "en", "pt", "es" };
            _harness.Show(IndexOf(HeaderEntryNames[0]));
            yield return CardTestInput.Settle();
            foreach (string pick in new[] { "es", "pt", "en" })
            {
                yield return OpenSettings();
                CollectionAssert.AreEqual(new[] { "English", "Portugu\u00eas", "ES" },
                    Sheet.Settings.Choice("en").parent.Children().OfType<Button>().Select(b => b.text), "every language of the wall, in its order");
                yield return TapChoice(pick);
                Assert.AreEqual(pick, _harness.Language, "a real tap picks " + pick + " directly (a list, not a chip that steps)");
            }
        }

        [UnityTest]
        public IEnumerator TheTitleEndsBeforeTheButtons_OnEachHeaderStructure([ValueSource(nameof(HeaderStructures))] string name)
        {
            Assert.GreaterOrEqual(IndexOf(name), 0, "precondition: the gallery still has the entry " + name);
            _harness.Show(IndexOf(name));
            yield return CardTestInput.Settle();
            var header = (HeaderBlockView)Sheet.Stack.BoundViews[0];
            var title = header.Root.Q<Label>("card-header-title");
            var aa = Sheet.SettingsButton;
            Assert.AreEqual(DisplayStyle.Flex, aa.resolvedStyle.display, "precondition: Aa shows on " + name);
            Assert.LessOrEqual(title.worldBound.xMax, aa.worldBound.xMin + 0.5f, name + ": the title ends before Aa, never under it: " + title.worldBound + " vs " + aa.worldBound);
        }

        [UnityTest]
        public IEnumerator TheSettingsRender_IsSavedForTheVisionPass_InBothModes()
        {
            _harness.Show(IndexOf(HeaderEntryNames[0]));
            yield return CardTestInput.Settle();
            yield return CardGalleryChecks.Render("Card_settings_closed");
            yield return OpenSettings();
            ThemeController.SetMode(CardOptions.ModeLight);
            yield return CardTestInput.Settle();
            Assert.IsTrue(Sheet.Settings.Choice(CardOptions.ModeLight).ClassListContains(CardVisitorSettingsView.ChosenClass), "light shown: Light marked");
            yield return CardGalleryChecks.Render("Card_settings_open_light");
            // - the mode changing while the panel is open without a tap in it (a live Default Mode edit): the mark follows (P4.4b part 2)
            ThemeController.SetMode(CardOptions.ModeDark);
            yield return CardTestInput.Settle();
            Assert.IsTrue(Sheet.Settings.Choice(CardOptions.ModeDark).ClassListContains(CardVisitorSettingsView.ChosenClass), "dark shown: Dark marked");
            Assert.IsFalse(Sheet.Settings.Choice(CardOptions.ModeLight).ClassListContains(CardVisitorSettingsView.ChosenClass), "and Light no longer");
            yield return CardGalleryChecks.Render("Card_settings_open_dark");
        }
    }
}
