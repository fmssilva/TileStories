using System.Collections;
using System.IO;
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
    // Phase A of the POI Detail Card (_3.1 section 7, 40-testing.md 4.4): the real PoiCardSheetView + header view in
    // the isolated gallery scene (Assets/Dev/CardGallery), fed CardGalleryDefinitions -- the same list the harness
    // shows. Per entry: it builds, sits at its stop, the half stop stays within 40%, the peek shows the whole title
    // and chip, no text runs out of the phone-width frame or under the X, text contrast >= 4.5:1, the X >= 44x44,
    // and a render Card_<entry>.png for the vision pass. Plus the sheet's own gestures on the gallery card.
    public class CardGalleryTests
    {
        private const string ScenePath = "Assets/Dev/CardGallery/CardGalleryScene.unity";
        private CardGalleryHarness _harness;

        private static string[] EntryNames => CardGalleryDefinitions.All.Select(e => e.Name).ToArray();

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
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            SceneManager.SetActiveScene(SceneManager.CreateScene("AfterCardGalleryTest"));
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
        }

        [UnityTest]
        public IEnumerator EveryGalleryEntry_BuildsFitsReadsAndRenders([ValueSource(nameof(EntryNames))] string name)
        {
            int index = CardGalleryDefinitions.All.ToList().FindIndex(e => e.Name == name);
            var entry = CardGalleryDefinitions.All[index];
            _harness.Show(index);
            yield return CardTestInput.Settle();

            var sheet = _harness.Sheet;
            var header = (HeaderBlockView)sheet.Stack.BoundViews[0];
            Assert.AreEqual(entry.Stop, sheet.Stop, "rests at its stop");
            // - layout snaps to physical pixels: one screen pixel in panel units is the honest tolerance
            float onePixel = RuntimePanelUtils.ScreenToPanel(sheet.Root.panel, Vector2.right).x - RuntimePanelUtils.ScreenToPanel(sheet.Root.panel, Vector2.zero).x;
            Assert.AreEqual(sheet.Stops.HeightOf(entry.Stop), sheet.Root.resolvedStyle.height, onePixel + 0.01f, "the sheet is that stop's height once the animation ends");
            Assert.LessOrEqual(sheet.Stops.Half, sheet.Layer.layout.height * CardContainerSettings.HalfMaxRatioMax + 0.5f, "half is at most 40% of the screen");
            Assert.Greater(sheet.Stops.Peek, 0f, "peek was measured");

            // content
            Assert.AreEqual(entry.Title, header.TitleText);
            Assert.AreEqual("Civic Buildings - Landmark", header.ChipText, "the chip names the category and level, never keys");
            bool subtitleExpected = entry.Variant == BuiltInBlocks.HeaderTextOnly && entry.Subtitle.Length > 0;
            Assert.AreEqual(subtitleExpected, header.SubtitleShown, "compact never shows a subtitle");

            // fit: inside the phone-width frame, the title clear of the X
            Rect frame = _harness.Frame.worldBound, card = sheet.Root.worldBound;
            Assert.AreEqual(frame.width, card.width, 0.5f, "the sheet spans the phone-width frame");
            var title = header.Root.Q<Label>("card-header-title");
            Assert.LessOrEqual(title.worldBound.xMax, sheet.CloseButton.worldBound.xMin + 0.5f, "the title wraps before the X");
            Assert.LessOrEqual(header.PeekPart.worldBound.yMax, card.yMin + sheet.Stops.Peek + 0.5f, "peek shows the whole title and chip");
            if (entry.Stop != SheetStopRule.Stop.Peek && subtitleExpected)
            {
                var subtitle = header.Root.Q<Label>("card-header-subtitle");
                Assert.LessOrEqual(subtitle.worldBound.xMax, card.xMax + 0.5f, "the subtitle wraps inside the card");
                // - half never passes 40%, so a long header may continue below it: half shows the subtitle start, full all of it
                Assert.Less(subtitle.worldBound.yMin, card.yMax, "the subtitle starts inside the card at " + entry.Stop);
                if (entry.Stop == SheetStopRule.Stop.Full)
                    Assert.LessOrEqual(subtitle.worldBound.yMax, card.yMax + 0.5f, "the whole subtitle is visible at full");
            }

            // accessibility: contrast from the resolved (token) colours, the X's size
            Color surface = sheet.Root.resolvedStyle.backgroundColor;
            Assert.GreaterOrEqual(CardTestInput.Contrast(title.resolvedStyle.color, surface), UIAccessibility.MinRatioNormalText, "title on the card");
            var chip = header.Root.Q<Label>("card-header-chip");
            Assert.GreaterOrEqual(CardTestInput.Contrast(chip.resolvedStyle.color, chip.resolvedStyle.backgroundColor), UIAccessibility.MinRatioNormalText, "chip text on the chip");
            var sub = header.Root.Q<Label>("card-header-subtitle");
            Assert.GreaterOrEqual(CardTestInput.Contrast(sub.resolvedStyle.color, surface), UIAccessibility.MinRatioNormalText, "subtitle (ink-2) on the card");
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(sheet.CloseButton.worldBound.width, sheet.CloseButton.worldBound.height),
                "the X is " + sheet.CloseButton.worldBound.size + ", below 44x44");

            yield return Render("Card_" + entry.Name);
        }

        [UnityTest]
        public IEnumerator TheSheet_DragsUpThroughItsStops_AndPullingItDownCloses()
        {
            _harness.Show(CardGalleryDefinitions.All.ToList().FindIndex(e => e.Stop == SheetStopRule.Stop.Peek && e.Variant == BuiltInBlocks.HeaderTextOnly));
            yield return CardTestInput.Settle();
            var sheet = _harness.Sheet;

            yield return CardTestInput.Drag(sheet.Handle, -(sheet.Stops.Half - sheet.Stops.Peek));
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Half, sheet.Stop, "a slow drag up by the peek-half gap lands on half");

            yield return CardTestInput.Drag(sheet.Stack.HeaderSlot, -(sheet.Stops.Full - sheet.Stops.Half));
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop, "the header drags the sheet too");
            Assert.AreEqual(sheet.Stops.Full, sheet.Root.resolvedStyle.height, 0.5f);

            yield return CardTestInput.Drag(sheet.Handle, sheet.Stops.Full - sheet.Stops.Peek * 0.3f, frames: 20);
            Assert.IsFalse(sheet.IsOpen, "pulled below peek: the card closes (the harness closes on CloseRequested)");
        }

        [UnityTest]
        public IEnumerator AQuickSwipeUpFromPeek_GoesToHalf_EvenReleasedNearPeek()
        {
            _harness.Show(CardGalleryDefinitions.All.ToList().FindIndex(e => e.Stop == SheetStopRule.Stop.Peek));
            yield return CardTestInput.Settle();
            var sheet = _harness.Sheet;
            float shortMove = (sheet.Stops.Half - sheet.Stops.Peek) * 0.3f;
            yield return CardTestInput.Drag(sheet.Handle, -shortMove, frames: 2);
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Half, sheet.Stop, "a flick counts, not only where the finger stopped");
        }

        [UnityTest]
        public IEnumerator TheX_AsksToClose()
        {
            _harness.Show(0);
            yield return CardTestInput.Settle();
            int asked = 0;
            _harness.Sheet.CloseRequested += () => asked++;
            using (var e = NavigationSubmitEvent.GetPooled())
            {
                e.target = _harness.Sheet.CloseButton;
                _harness.Sheet.CloseButton.SendEvent(e);
            }
            yield return null;
            Assert.AreEqual(1, asked);
            Assert.IsFalse(_harness.Sheet.IsOpen);
        }

        // Save what the Game view shows under Assets/Screenshots
        private static IEnumerator Render(string name)
        {
            yield return new WaitForEndOfFrame();
            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            try
            {
                string dir = Path.Combine(Application.dataPath, "Screenshots");
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir, name + ".png"), tex.EncodeToPNG());
            }
            finally { Object.Destroy(tex); }
        }
    }
}
