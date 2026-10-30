using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using FontAsset = UnityEngine.TextCore.Text.FontAsset;

namespace TileStories.Tests
{
    // The app's two faces end to end on the REAL wall scene (_3.3 P4.3, R12.3): the theme asset's type roles -> the generated stylesheet ->
    // every screen's marked root -> the face each text is DRAWN with. Body (Atkinson Hyperlegible Next) for every text of every panel,
    // display (Fraunces SemiBold) for exactly the theme's display selectors; and the weights through the UI assets' weight tables: a bold
    // body text draws the Bold cut, a bold display title draws its own SemiBold cut -- never a synthesized bold, which TextCore draws (wider,
    // smeared) when a weight table has no face for the weight asked.
    public class ThemeTypeTests : SearchSceneFixture
    {
        // - a sample with wide and narrow letters and the Portuguese marks: two faces (or a real and a fake bold) never measure alike on it
        private const string Sample = "Castelo de São Jorge, Paço da Ribeira";
        // - one panel pixel: two measures of the SAME face differ by less (layout rounding), two different faces by tens of pixels
        private const float SameFace = 1f;

        private static ThemeType Type => ThemeController.Load(ThemeController.DefaultThemeKey).Type;
        private static FontAsset Ui(string key) => Type.fontLibrary.GetUiFont(key);
        // The theme's halo twins of its two faces (ThemeUssGenerator writes them on the themed root)
        private static readonly CustomStyleProperty<FontAsset> BodyHalo = new("--ts-font-body-halo");
        private static readonly CustomStyleProperty<FontAsset> DisplayHalo = new("--ts-font-display-halo");

        [UnityTest]
        public IEnumerator EveryTextOfEveryScreen_DrawsTheBodyFace_AndExactlyTheDisplayRolesTheDisplayFace()
        {
            yield return OpenFull("lamp");
            FontAsset body = Ui(Type.bodyFont), display = Ui(Type.displayFont);
            Assert.IsNotNull(body, "precondition: the body key has a UI Toolkit face");
            Assert.IsNotNull(display, "precondition: the display key has a UI Toolkit face");
            var displayClasses = Type.displaySelectors.Select(s => s.TrimStart('.')).ToList();

            int checkedTexts = 0, displayed = 0;
            var wrong = new List<string>();
            var documents = Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
            Assert.GreaterOrEqual(documents.Length, 3, "precondition: the wall scene's screens (card, search, zoom)");
            // - a display role reaches its own text and the texts inside it (the font is inherited)
            bool IsDisplay(VisualElement text)
            {
                for (var e = text; e != null; e = e.parent) if (displayClasses.Any(e.ClassListContains)) return true;
                return false;
            }
            int haloCopies = 0;
            foreach (var document in documents)
            {
                // - the theme's halo twins, read where the theme declares its faces (the themed root, .ts-app), as the hero reads them
                var app = document.rootVisualElement.ClassListContains("ts-app") ? document.rootVisualElement : document.rootVisualElement.Q(className: "ts-app");
                FontAsset bodyHalo = null, displayHalo = null;
                app?.customStyle.TryGetValue(BodyHalo, out bodyHalo);
                app?.customStyle.TryGetValue(DisplayHalo, out displayHalo);
                foreach (var text in document.rootVisualElement.Query<TextElement>().ToList())
                {
                    if (string.IsNullOrEmpty(text.text) || text.resolvedStyle.display == DisplayStyle.None) continue;
                    var drawn = text.resolvedStyle.unityFontDefinition.fontAsset;
                    bool isDisplay;
                    FontAsset expected;
                    if (text.ClassListContains(HeroWordsOnPicture.HaloClass))
                    {
                        // - a halo copy of the hero's words (HeroWordsOnPicture's back label) draws the halo TWIN of the face of the word it
                        //   lies under -- the wide SDF its glow needs lives only there; its word: the nearest shown label with its text that is
                        //   not a copy
                        TextElement front = null;
                        for (var e = text.parent; e != null && front == null; e = e.parent)
                            front = e.Query<TextElement>().Where(t => t.text == text.text && !t.ClassListContains(HeroWordsOnPicture.HaloClass)
                                && t.resolvedStyle.display != DisplayStyle.None).First();
                        Assert.IsNotNull(front, "precondition: the halo copy '" + Short(text.text) + "' lies under a word of the hero");
                        isDisplay = IsDisplay(front);
                        expected = isDisplay ? displayHalo : bodyHalo;
                        Assert.IsNotNull(expected, "precondition: the theme names its " + (isDisplay ? "display" : "body") + " halo twin");
                        haloCopies++;
                    }
                    else
                    {
                        isDisplay = IsDisplay(text);
                        expected = isDisplay ? display : body;
                        if (isDisplay) displayed++;
                    }
                    checkedTexts++;
                    if (drawn != expected)
                        wrong.Add(document.name + " '" + Short(text.text) + "' (" + string.Join(" ", text.GetClasses()) + "): " + (drawn != null ? drawn.name : "no font asset") + ", not " + expected.name);
                }
            }
            Assert.Greater(checkedTexts, 100, "precondition: the open Lamp card, search and zoom hold many texts");
            Assert.Greater(displayed, 3, "precondition: the card's title and block headings are display roles");
            Assert.Greater(haloCopies, 0, "precondition: the open Lamp's hero draws its words' halo copies (Words On Picture's default look)");
            Assert.IsEmpty(wrong, wrong.Count + " text(s) drawn in another face: " + string.Join("; ", wrong.Take(10)));
        }

        // `-unity-font-style: bold` on body text draws the Atkinson Bold CUT (the weight table's 700): a bold label measures exactly like a
        // label given the Bold asset itself, and unlike the Regular; rich text font-weight=500 draws the Medium cut the same way
        [UnityTest]
        public IEnumerator BoldAndMediumBodyText_DrawTheirOwnCuts_NotASynthesizedBold()
        {
            yield return OpenFull("lamp");
            var root = Card.GetComponent<UIDocument>().rootVisualElement;
            var regular = Probe(root, Sample);
            var bold = Probe(root, Sample);
            bold.style.unityFontStyleAndWeight = FontStyle.Bold;
            var boldCut = Probe(root, Sample);
            boldCut.style.unityFontDefinition = FontDefinition.FromSDFFont(Ui(Type.bodyBoldFont));
            var medium = Probe(root, "<font-weight=500>" + Sample + "</font-weight>");
            var mediumCut = Probe(root, Sample);
            mediumCut.style.unityFontDefinition = FontDefinition.FromSDFFont(Ui(Type.bodyMediumFont));
            yield return null;
            yield return null;
            try
            {
                Assert.AreEqual(Ui(Type.bodyFont), regular.resolvedStyle.unityFontDefinition.fontAsset, "precondition: a plain probe inherits the body face");
                float w(Label l) => l.MeasureTextSize(l.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined).x;
                Debug.Log($"[Theme] body widths: regular {w(regular):0.0}, bold {w(bold):0.0}, Bold cut {w(boldCut):0.0}, medium {w(medium):0.0}, Medium cut {w(mediumCut):0.0}");
                Assert.AreEqual(w(boldCut), w(bold), SameFace, "a bold body text draws the Bold cut (a synthesized bold measures otherwise)");
                Assert.Greater(Mathf.Abs(w(bold) - w(regular)), 10 * SameFace, "precondition: the Bold cut is not the Regular");
                Assert.AreEqual(w(mediumCut), w(medium), SameFace, "font-weight=500 draws the Medium cut");
                Assert.Greater(Mathf.Abs(w(medium) - w(regular)), 2 * SameFace, "precondition: the Medium cut is not the Regular");
            }
            finally { foreach (var probe in new[] { regular, bold, boldCut, medium, mediumCut }) probe.RemoveFromHierarchy(); }
        }

        // The card's title is a display role AND bold (About.uss): it must draw the Fraunces SemiBold cut as it is -- it measures exactly like
        // the same words given the display face at normal weight. Without the display asset's weight 700 entry TextCore would fake a bold on
        // top of the SemiBold (wider)
        [UnityTest]
        public IEnumerator TheBoldCardTitle_DrawsTheDisplayCutItself_NoSynthesizedBold()
        {
            yield return OpenFull("lamp");
            var title = Card.Sheet.Root.Q<Label>(className: "card-header__title");
            Assert.IsNotNull(title, "precondition: the card has its title");
            Assert.AreEqual(FontStyle.Bold, title.resolvedStyle.unityFontStyleAndWeight, "precondition: the title is styled bold (About.uss)");
            Assert.AreEqual(Ui(Type.displayFont), title.resolvedStyle.unityFontDefinition.fontAsset, "precondition: the title speaks in the display face");
            var root = Card.GetComponent<UIDocument>().rootVisualElement;
            var plain = Probe(root, title.text);
            plain.style.unityFontDefinition = FontDefinition.FromSDFFont(Ui(Type.displayFont));
            plain.style.fontSize = title.resolvedStyle.fontSize;
            yield return null;
            yield return null;
            try
            {
                float w(TextElement l) => l.MeasureTextSize(l.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined).x;
                Debug.Log($"[Theme] title '{title.text}' at {title.resolvedStyle.fontSize}: bold title {w(title):0.0}, display cut at normal {w(plain):0.0}");
                Assert.AreEqual(w(plain), w(title), SameFace, "the bold title draws the SemiBold cut itself, not a synthesized bold");
            }
            finally { plain.RemoveFromHierarchy(); }
        }

        // An unwrapped label of `text` at the body size under `root` (it inherits the root's face), kept out of the layout's way
        private static Label Probe(VisualElement root, string text)
        {
            var label = new Label(text) { enableRichText = true, pickingMode = PickingMode.Ignore };
            label.style.position = Position.Absolute;
            label.style.left = 0; label.style.top = 0;
            label.style.whiteSpace = WhiteSpace.NoWrap;
            label.style.fontSize = 20;
            label.style.opacity = 0;
            root.Add(label);
            return label;
        }

        private IEnumerator OpenFull(string poiId)
        {
            SelectionEventBus.Select(poiId);
            yield return CardTestInput.Settle();
            Assert.IsTrue(Card.Sheet.IsOpen, "precondition: " + poiId + "'s card is open");
            Card.Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
        }

        private static string Short(string text) => text.Length <= 24 ? text : text.Substring(0, 24) + "...";
    }
}
