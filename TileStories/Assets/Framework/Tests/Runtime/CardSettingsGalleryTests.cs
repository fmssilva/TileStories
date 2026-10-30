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

        // _3.3 S4 third round (v5) + _Test_Suite_1.md F2: settings is a thin "sliders" mark in a 36 pt glass disc at the bar's right, level with
        // the X at its left, a whole touch target; off the hero its mark reads at 7:1 on its glass over the darkest AND the lightest backdrop
        // (over the hero's picture it is measured on the render: PoiCardChromeMarksSceneTests)
        [UnityTest]
        public IEnumerator TheSettingsButton_IsASlidersMarkInAGlassDisc_FingerSized_ReadsAt7To1_LevelWithTheX_AtTheRight([Values("light", "dark")] string mode)
        {
            ThemeController.SetMode(mode);
            _harness.Show(IndexOf(HeaderEntryNames[0]));
            yield return CardTestInput.Settle();

            var button = Sheet.SettingsButton;
            var glyph = Sheet.SettingsGlyph;
            var disc = glyph.parent;
            float onePixel = CardGalleryChecks.OnePixel(button);
            Assert.AreEqual(DisplayStyle.Flex, button.resolvedStyle.display, "the settings button shows");
            Assert.AreEqual("", button.text, "no words on it: the mark says it");
            Assert.AreEqual(CardIcons.Shape.Sliders, glyph.Kind, "the drawn mark is the sliders, not a gear (v5)");
            Assert.IsFalse(glyph.Filled, "a thin outline mark");
            Assert.AreEqual("Card settings", button.tooltip, "its name for a screen reader and a long press");
            // - the sizes from their tokens: a 44 pt target, a 36 pt disc centred in it, a 16 pt mark centred in that, a 1.5 stroke
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(button.worldBound.width, button.worldBound.height), "finger-sized: " + button.worldBound);
            Assert.AreEqual(CardTestInput.TokenPx("--ts-bar-height"), button.worldBound.width, onePixel + 0.01f, "as wide as the bar is tall");
            Assert.IsTrue(disc.ClassListContains(PoiCardSheetView.MarkDiscClass) && disc.parent == button, "the mark sits in the button's disc");
            Assert.AreEqual(CardTestInput.TokenPx("--ts-chrome-disc"), disc.worldBound.width, onePixel + 0.01f, "a 36 pt disc");
            Assert.AreEqual(disc.worldBound.width / 2f, disc.resolvedStyle.borderTopLeftRadius, 0.01f, "round");
            Assert.AreEqual(button.worldBound.center.x, disc.worldBound.center.x, onePixel, "the disc centred in the target");
            Assert.AreEqual(CardTestInput.TokenPx("--ts-chrome-mark"), glyph.worldBound.width, onePixel + 0.01f, "a 16 pt mark");
            Assert.AreEqual(disc.worldBound.center.y, glyph.worldBound.center.y, onePixel, "centred in the disc");
            Assert.AreEqual(1.5f, glyph.worldBound.width * CardIcons.StrokeUnits(glyph.Kind), 0.01f, "v5's thin 1.5 stroke");
            // - off the hero (this entry has no picture): the card's ink on its disc AS SEEN on the render, over the darkest and the lightest
            //   camera behind the card -- the disc's glass lies on the sheet's ground, drawn as a picture a colour walk does not see
            Assert.IsFalse(Sheet.Root.ClassListContains(BlockStackView.BarsOnHeroClass), "precondition: the bar is off the hero");
            CardTestInput.HoldSectionsStill(Sheet);
            Sheet.Stack.Light.Clock = () => 0f;
            Sheet.Stack.Light.Tick();
            yield return ThemePixelSet.SeenOverBackdrops("settings_" + mode, new[] { ((VisualElement)glyph, disc) }, (backdrop, _, read) =>
            {
                Assert.Greater(read.InkPixels, 0, "precondition: the mark draws pixels over " + backdrop);
                Assert.GreaterOrEqual(read.Ratio, 7f, mode + ": the mark reads at 7:1 on its glass over " + backdrop + " as seen (" + read.Ratio.ToString("F2")
                    + ":1, worst pixel " + (Color32)read.Worst + ", D1)");
            });

            // - v5's places: the X at the left, settings at the right, level, each its inset from the card's side
            Rect close = Sheet.CloseButton.worldBound, settings = button.worldBound, card = Sheet.Root.worldBound;
            Assert.Less(close.xMax, settings.xMin, "the X at the left, settings at the right: " + close + " vs " + settings);
            Assert.AreEqual(close.yMin, settings.yMin, onePixel + 0.01f, "level with the X");
            Assert.AreEqual(close.xMin - card.xMin, card.xMax - settings.xMax, onePixel, "the same inset from each side");
            Assert.AreEqual(CardTestInput.TokenPx("--ts-chrome-inset"), close.xMin - card.xMin, onePixel, "the X its inset from the left");
        }

        [Category("CardSmoke")]
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
            Assert.AreEqual("Definições do cartão", Sheet.SettingsButton.tooltip, "and the settings mark's name");
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
            Assert.LessOrEqual(title.worldBound.xMax, aa.worldBound.xMin + 0.5f, name + ": the title ends before settings, never under it: " + title.worldBound + " vs " + aa.worldBound);
            Assert.GreaterOrEqual(title.worldBound.xMin, Sheet.CloseButton.worldBound.xMax - 0.5f, name + ": and starts after the X: " + title.worldBound);
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
