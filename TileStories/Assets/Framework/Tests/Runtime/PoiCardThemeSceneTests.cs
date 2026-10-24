using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of the two modes and the Aa button on the REAL LivingRoomScene (_3.3 P4.4b), with real input and an in-memory CardLocalState
    // for the wall's own id (never the developer's PlayerPrefs): the app opens light by default; a real tap on Aa then on Dark re-colours
    // every screen by its mode class WITHOUT rebuilding the card (the same views, the same stop, the same scroll); the pick is remembered
    // and wins over the wall's Default Mode, which a live edit moves while no pick is saved.
    public class PoiCardThemeSceneTests : SearchSceneFixture
    {
        private PoiCardSheetView Sheet => Card.Sheet;

        [UnityTearDown]
        public IEnumerator RestoreTheMode()
        {
            ThemeController.SetMode("");
            yield return null;
        }

        private void UseMemoryState() => Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);

        private IEnumerator OpenFull(string poiId)
        {
            SelectionEventBus.Select(poiId);
            yield return CardTestInput.Settle();
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            for (int frame = 0; frame < 600 && Mathf.Abs(Sheet.Root.layout.height - Sheet.Stops.Full) > 0.5f; frame++) yield return null;
        }

        // Every UIDocument root of the scene carries this mode's class and no other
        private static void AssertEveryScreenIn(string mode)
        {
            var roots = Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None).Select(d => d.rootVisualElement).Where(r => r != null).ToList();
            Assert.GreaterOrEqual(roots.Count, 3, "precondition: the card, search and zoom screens");
            foreach (var root in roots.Where(r => r.ClassListContains(ThemeController.AppClass)))
                foreach (string m in CardOptions.Modes)
                    Assert.AreEqual(m == mode, root.ClassListContains(ThemeController.ModeClassPrefix + m), "a screen's root in " + mode + ": " + root.name);
        }

        [UnityTest]
        public IEnumerator TheAppOpensLight_ARealTapOnDark_RecoloursWithoutARebuild_TheScrollKept_AndThePickIsRemembered()
        {
            UseMemoryState();
            yield return OpenFull("lamp");
            AssertEveryScreenIn(CardOptions.ModeLight);
            Color lightSurface = default, darkSurface = default;
            Assert.IsTrue(ThemeController.TryResolverFor(Sheet.Stack.Scroll, out var light) && light.TryColour("--ts-surface", out lightSurface), "precondition: under the theme");
            var slot = Sheet.Stack.Scroll.contentContainer.Query(className: BlockFrame.FramedClass).First();
            Assert.AreEqual(lightSurface, slot.resolvedStyle.backgroundColor, "precondition: a block wears the light surface");

            Sheet.Stack.Scroll.scrollOffset = new Vector2(0f, 900f);
            yield return CardTestInput.Settle();
            float scrolled = Sheet.Stack.Scroll.scrollOffset.y;
            Assert.Greater(scrolled, 100f, "precondition: the stack really scrolls");
            var viewsBefore = Sheet.Stack.BoundViews.ToList();

            var aa = Sheet.SettingsButton;
            yield return CardTestInput.Tap(aa.panel, aa.worldBound.center);
            yield return null;
            Assert.AreEqual(PoiCardSheetView.SettingsKey, Sheet.Overlay.OpenKey, "a real tap on Aa opened the settings");
            var dark = Sheet.Settings.Choice(CardOptions.ModeDark);
            yield return CardTestInput.Tap(dark.panel, dark.worldBound.center);
            yield return CardTestInput.Settle();

            AssertEveryScreenIn(CardOptions.ModeDark);
            Assert.IsTrue(ThemeController.TryResolverFor(Sheet.Stack.Scroll, out var darkTheme) && darkTheme.TryColour("--ts-surface", out darkSurface), "under the dark theme");
            Assert.AreEqual(darkSurface, slot.resolvedStyle.backgroundColor, "the same block re-coloured to the dark surface");
            Assert.AreNotEqual(lightSurface, darkSurface);
            CollectionAssert.AreEqual(viewsBefore, Sheet.Stack.BoundViews.ToList(), "nothing was rebuilt: the very same views");
            Assert.AreEqual(scrolled, Sheet.Stack.Scroll.scrollOffset.y, 0.5f, "the scroll stayed where the reader was");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "and the stop");
            Assert.AreEqual(PoiCardSheetView.SettingsKey, Sheet.Overlay.OpenKey, "the settings stay open to show the change");
            Assert.IsTrue(dark.ClassListContains(CardVisitorSettingsView.ChosenClass), "Dark is marked now");
            Assert.AreEqual(CardOptions.ModeDark, Card.State.Mode(), "the pick is remembered for this wall");

            // - a new card keeps the visitor's mode, and the pick wins over the wall's Default Mode
            Sheet.Overlay.Close();
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            Session.CardSettings.default_mode = CardOptions.ModeLight;
            yield return OpenFull("painting");
            AssertEveryScreenIn(CardOptions.ModeDark);
        }

        [UnityTest]
        public IEnumerator TheWallsDefaultMode_MovesTheAppLive_WhileTheVisitorPickedNone()
        {
            UseMemoryState();
            yield return OpenFull("lamp");
            foreach (string mode in new[] { CardOptions.ModeDark, CardOptions.ModeLight })
            {
                var copy = ConfigCopy();
                copy.card_settings.default_mode = mode;
                Session.ApplyCardSettings(copy);
                yield return CardTestInput.Settle(0.3f);
                AssertEveryScreenIn(mode);
            }
        }
    }
}
