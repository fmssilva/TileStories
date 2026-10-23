using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // The theme pipeline end to end on the REAL wall scene (_3.3 P4.1, R12.1): the theme asset -> the generated stylesheet -> the shared
    // PanelSettings' theme (TileStoriesTheme.tss) -> every screen's marked root -> what the card paints, and C# (ThemeResolver) reading
    // the SAME values the panel resolved. The composition test of 40-testing 4.2.1: each piece has its own tests, this one proves they meet.
    public class ThemePipelineTests : SearchSceneFixture
    {
        private static ThemeAsset Theme => ThemeController.Load(ThemeController.DefaultThemeKey);

        private static Color32 ThemeColour(string token)
        {
            Assert.IsTrue(Theme.ResolverFor(Theme.DefaultMode).TryColour(token, out var colour), "precondition: the theme holds " + token);
            return colour;
        }

        private static void AssertPaints(Color painted, string token, string what) =>
            Assert.AreEqual(ThemeColour(token), (Color32)painted, what + " paints " + token + " (the theme's value)");

        [UnityTest]
        public IEnumerator EveryScreenOnTheSharedPanel_IsMarkedWithTheTheme_AndThePanelReadsTheTileStoriesTheme()
        {
            yield return null;
            var documents = Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
            Assert.GreaterOrEqual(documents.Length, 3, "precondition: the wall scene's screens (card, search, zoom) are UIDocuments");
            foreach (var document in documents)
            {
                Assert.AreEqual("TileStoriesTheme", document.panelSettings.themeStyleSheet.name, document.name + ": the panel's theme imports the generated one");
                var root = document.rootVisualElement;
                Assert.IsTrue(root.ClassListContains(ThemeController.AppClass), document.name + ": its root is marked ts-app");
                Assert.IsTrue(root.ClassListContains(ThemeController.ThemeClassPrefix + Theme.Key), document.name + ": ...with the default theme");
                Assert.IsTrue(root.ClassListContains(ThemeController.ModeClassPrefix + Theme.DefaultMode), document.name + ": ...in its default mode");
            }
        }

        // The panel's resolved value of every theme colour (declared on the marked root by the generated rule, so it is in that root's
        // customStyle) is exactly the value C# reads from the asset
        [UnityTest]
        public IEnumerator EveryThemeColour_ThePanelResolves_IsTheColourCSharpReads()
        {
            yield return Select("lamp");
            var root = Card.GetComponent<UIDocument>().rootVisualElement;
            var colours = Theme.Mode(Theme.DefaultMode).tokens.colours;
            Assert.Greater(colours.Count, 30, "precondition: the theme's colours");
            foreach (var token in colours)
            {
                Assert.IsTrue(root.customStyle.TryGetValue(new CustomStyleProperty<Color>(token.name), out var resolved),
                    token.name + ": the panel resolves it on the marked root (the generated rule reached the panel through TileStoriesTheme.tss)");
                Assert.AreEqual((Color32)token.value, (Color32)resolved, token.name + ": the stylesheet and C# hold the same colour");
                Assert.AreEqual(token.value.a, resolved.a, 1e-6f, token.name + ": the same alpha");
                Assert.IsTrue(CardTokenColours.TryResolve(Card.Sheet.Root, token.name, out var read), token.name + ": C# reads it from the card");
                Assert.AreEqual(token.value, read, token.name + ": C# reads the asset's value");
            }
        }

        // The card's roots declare only the card's OWN tokens now; the theme's arrive by inheritance, and each root still paints with them
        [UnityTest]
        public IEnumerator TheCardsRoots_DeclareOnlyTheirOwnTokens_AndPaintWithTheThemesByInheritance()
        {
            yield return Select("lamp");
            var sheet = Card.Sheet;
            Assert.IsTrue(sheet.Root.customStyle.TryGetValue(new CustomStyleProperty<float>("--ts-sheet-top-gap"), out float gap),
                "the sheet still declares the numbers C# reads on it (CardTokens.uss)");
            Assert.AreEqual(CardTestInput.TokenPx("--ts-sheet-top-gap"), gap, "...with CardTokens.uss's value");
            Assert.IsFalse(sheet.Root.customStyle.TryGetValue(new CustomStyleProperty<Color>("--ts-surface"), out _),
                "the sheet no longer declares a theme colour itself (it would hide the theme's modes)");

            // - the six roots that carry the card's token classes: each sits under the marked root and paints with the theme's values
            AssertPaints(sheet.Root.resolvedStyle.backgroundColor, "--ts-surface", "the sheet");
            AssertPaints(Card.AudioCoordinator.Mini.Root.resolvedStyle.backgroundColor, "--ts-surface", "the mini-player");
            AssertPaints(sheet.Overlay.Scrim.resolvedStyle.backgroundColor, "--ts-scrim", "the menu layer's scrim");
            AssertPaints(sheet.Floating.Scrim.resolvedStyle.backgroundColor, "--ts-scrim", "the floating slot's scrim");
            AssertPaints(sheet.Takeover.Root.resolvedStyle.backgroundColor, "--ts-bg", "the full view");
            // - the scales arrive the same way (a hidden root, like the floating slot here, has no layout to read: the shown sheet is measured)
            Assert.AreEqual(CardTestInput.TokenPx("--ts-line-width"), sheet.Root.resolvedStyle.borderTopWidth, 0.01f, "the sheet's top edge is the theme's --ts-line-width");
            Assert.AreEqual(CardTestInput.TokenPx("--ts-radius-l"), sheet.Root.resolvedStyle.borderTopLeftRadius, 0.01f, "its corners are the theme's --ts-radius-l");
            foreach (var root in new[] { sheet.Root, Card.AudioCoordinator.Mini.Root, sheet.Overlay.Root, sheet.Floating.Root, sheet.Floating.Scrim, sheet.Takeover.Root })
                Assert.IsTrue(ThemeController.TryResolverFor(root, out _), root.name + ": sits under the marked root (C# finds its theme)");
        }

        private IEnumerator Select(string poiId)
        {
            SelectionEventBus.Select(poiId);
            yield return CardTestInput.Settle();
            Assert.IsTrue(Card.Sheet.IsOpen, "precondition: " + poiId + "'s card is open");
        }
    }
}
