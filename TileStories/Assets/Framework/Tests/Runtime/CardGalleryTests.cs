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
    public partial class CardGalleryTests
    {
        private const string ScenePath = "Assets/Dev/CardGallery/CardGalleryScene.unity";
        private CardGalleryHarness _harness;

        private static string[] HeaderEntryNames => CardGalleryDefinitions.All.Where(e => e.IsHeader).Select(e => e.Name).ToArray();
        private static string[] BlockEntryNames => CardGalleryDefinitions.All.Where(e => !e.IsHeader).Select(e => e.Name).ToArray();

        private static int IndexOf(string name) => CardGalleryDefinitions.All.ToList().FindIndex(e => e.Name == name);

        // One screen pixel in panel units: layout snaps each edge to a physical pixel, so a width and a height set by the
        // same token may differ by up to that much (the honest tolerance whatever the Game view's size)
        private static float OnePixel(VisualElement e) => CardGalleryChecks.OnePixel(e);

        // Show a block entry and scroll its block into view; returns the block's view
        private IEnumerator ShowBlock(string name, System.Action<IBlockView> got)
        {
            _harness.Show(IndexOf(name));
            yield return CardGalleryChecks.SettledBlock(_harness, name, got);
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
                _harness = Object.FindFirstObjectByType<CardGalleryHarness>();
                yield return null;
            }
            Assert.IsNotNull(_harness, "the gallery scene holds CardGalleryHarness");
            // - the 390 x 844 frame is already pinned for the whole run (FixedFrameForTheRun)
            _harness.EnsureBuilt();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // - the harness configures the static marker palettes for the status entries, like a wall does
            StatusRamp.ResetToDefaults();
            CategoryPalette.ClearOverrides();
            MarkerHierarchyResolver.ResetToDefaults();
            var scene = SceneManager.GetSceneByPath(ScenePath);
            SceneManager.SetActiveScene(SceneManager.CreateScene("AfterCardGalleryTest"));
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
        }

        [UnityTest]
        public IEnumerator EveryHeaderEntry_BuildsFitsReadsAndRenders([ValueSource(nameof(HeaderEntryNames))] string name)
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
            Assert.AreEqual("Civic Buildings", header.ChipText, "the chip names the category (by name, never its key) and no level: Show Level is off");
            bool subtitleExpected = entry.Variant != BuiltInBlocks.HeaderCompact && entry.Subtitle.Length > 0;
            Assert.AreEqual(subtitleExpected, header.SubtitleShown, "compact never shows a subtitle; every other look does");
            // - the picture looks: a hero first in the scroll, its picture really loaded; without a picture, the text-only look
            bool pictureLook = System.Array.IndexOf(BuiltInBlocks.HeaderImageVariants, entry.Variant) >= 0 || entry.Variant == BuiltInBlocks.HeaderVideoLoop || entry.Variant == BuiltInBlocks.HeaderModelTurntable;
            bool heroExpected = pictureLook && entry.Content != CardGalleryDefinitions.NoPicture;
            Assert.AreEqual(heroExpected, header.HasHero, "a hero exactly for a picture look that has its picture");
            if (heroExpected)
            {
                Assert.AreEqual(0, sheet.Stack.Scroll.contentContainer.IndexOf(header.HeroPart), "the hero opens what scrolls, under the pinned title");
                if (entry.Variant == BuiltInBlocks.HeaderModelTurntable)
                    Assert.AreSame(header.HeroPart, header.ModelView.Root.parent, "the hero holds the model view, not a picture");
                else
                {
                    Assert.IsNotNull(header.Picture.Texture, "the picture was loaded (lazily, on bind)");
                    Assert.AreEqual(1, _harness.Media.RefCount(header.Picture.Path), "...once, through the header's own media scope");
                }
            }
            else Assert.IsNull(header.HeroPart.parent, "no hero on the card");

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
            Assert.AreEqual("Close", sheet.CloseButton.tooltip, "the X is named from the framework string table");
            foreach (var bar in sheet.CloseButton.Query(className: "poi-card__close-bar").ToList())
            {
                Assert.IsTrue(sheet.CloseButton.worldBound.Contains(bar.worldBound.center), "the X's bars cross inside the button");
                Assert.GreaterOrEqual(UIAccessibility.ContrastRatio(bar.resolvedStyle.backgroundColor, sheet.CloseButton.resolvedStyle.backgroundColor),
                    UIAccessibility.MinRatioLargeTextOrUIComponent, "the X reads against its button (3:1, a UI component)");
            }

            yield return Render("Card_" + entry.Name);
        }

        // Every block entry, whatever its kind (CardGalleryChecks.AssertBlockEntry: it builds at the full stop, every shown text sits
        // inside the phone-width card and reads against what is behind it, every tap target is >= 44x44, its heading and gap are the
        // card's one look), and a render Card_<kind>_<variant>_<content>.png is saved for the vision pass
        [UnityTest]
        public IEnumerator EveryBlockEntry_BuildsFitsReadsAndRenders([ValueSource(nameof(BlockEntryNames))] string name)
        {
            var entry = CardGalleryDefinitions.All[IndexOf(name)];
            IBlockView view = null;
            yield return ShowBlock(name, v => view = v);
            CardGalleryChecks.AssertBlockEntry(_harness, entry, view);
            yield return Render("Card_" + entry.Name);
        }


        // The cause of the old 1.5 px flake: a height measured while the sheet still animated. The gallery sheet has no
        // height transition at all, so every geometry assert above measures a sheet at rest -- whatever the frame rate.
        [UnityTest]
        public IEnumerator TheGallerySheet_HasNoHeightAnimation_SoNoMeasurementRacesOne()
        {
            _harness.Show(IndexOf("header_text_only_long_peek"));
            yield return null;
            var sheet = _harness.Sheet;
            Assert.AreEqual("height", sheet.Root.resolvedStyle.transitionProperty.Single().ToString(), "precondition: the card animates its height (PoiCard.uss)");
            Assert.IsTrue(sheet.Root.resolvedStyle.transitionDuration.All(d => d.value == 0f), "the gallery runs it with a zero duration");
            var real = new PoiCardSheetView(new VisualElement(), BlockRegistry.Shared, System.Array.Empty<StyleSheet>());
            Assert.AreEqual(StyleKeyword.Null, real.Root.style.transitionDuration.keyword, "the real card keeps the stylesheet's duration: only the harness sets one");
        }

        [UnityTest]
        public IEnumerator TheSheet_DragsUpThroughItsStops_AndPullingItDownCloses()
        {
            _harness.Show(CardGalleryDefinitions.All.ToList().FindIndex(e => e.Stop == SheetStopRule.Stop.Peek && e.Variant == BuiltInBlocks.HeaderTextOnly));
            yield return CardTestInput.Settle();
            var sheet = _harness.Sheet;

            yield return CardTestInput.Drag(sheet.Handle, -(sheet.Stops.Half - sheet.Stops.Peek), slowSheet: sheet);
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Half, sheet.Stop, "a slow drag up by the peek-half gap lands on half");

            yield return CardTestInput.Drag(sheet.Stack.HeaderSlot, -(sheet.Stops.Full - sheet.Stops.Half), slowSheet: sheet);
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop, "the header drags the sheet too");
            // - layout snaps to physical pixels: one screen pixel in panel units is the honest tolerance (as for every header entry)
            float onePixel = RuntimePanelUtils.ScreenToPanel(sheet.Root.panel, Vector2.right).x - RuntimePanelUtils.ScreenToPanel(sheet.Root.panel, Vector2.zero).x;
            Assert.AreEqual(sheet.Stops.Full, sheet.Root.resolvedStyle.height, onePixel + 0.01f);

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

        // Save what the Game view shows under TestEvidence/Card
        private static IEnumerator Render(string name) => CardGalleryChecks.Render(name);
            // ---------------- Tier 2: pictures (_3.1 step 7) ----------------

        // The colour a real render shows at a panel position (the shared helper)
        private static IEnumerator PixelAt(VisualElement inPanel, Vector2 panelPoint, System.Action<Color> got) =>
            CardGalleryChecks.PixelAt(inPanel, panelPoint, got);

        private static void AssertColour(Color expected, Color seen, string what)
        {
            Assert.AreEqual(expected.r, seen.r, 0.04f, what + " (red)");
            Assert.AreEqual(expected.g, seen.g, 0.04f, what + " (green)");
            Assert.AreEqual(expected.b, seen.b, 0.04f, what + " (blue)");
        }

        private static Color PictureColour(string name) => CardGalleryDefinitions.Pictures[name].Colour;

        private IEnumerator ShowHeader(string name)
        {
            _harness.Show(IndexOf(name));
            yield return CardTestInput.Settle();
        }

        private HeaderBlockView GalleryHeader => (HeaderBlockView)_harness.Sheet.Stack.BoundViews[0];

        // A real body field long enough that the Full stop's stack genuinely overflows its viewport (a lone header never
        // does at the 390x844 fixed frame: 224pt of content vs 532-658pt of viewport)
        private static BlockFieldValue LongBody() => new()
        {
            key = BuiltInBlocks.RichTextBodyField,
            text = new System.Collections.Generic.List<LocalizedEntry>
            {
                new() { lang = "en", value = string.Join(" ", System.Linq.Enumerable.Repeat(
                    "A real paragraph of visitor text, long enough on its own that the stack truly overflows the Full stop's viewport and a wheel genuinely scrolls it.", 24)) },
            },
        };

        private static BlockFieldValue LocalizedField(string key, string value) => new()
        {
            key = key,
            text = new System.Collections.Generic.List<LocalizedEntry> { new() { lang = "en", value = value } },
        };


        // The events the gallery card's sink was told, in order: a real ICardEvents implementation that keeps them
        private sealed class RecordingEvents : ICardEvents
        {
            public readonly System.Collections.Generic.List<CardEvent> Raised = new();
            public void Raise(CardEvent cardEvent) => Raised.Add(cardEvent);
        }

        // Scroll `target` into view, then a real tap on its centre: what a visitor does when the block is taller than what shows
        // (an answer's verdict pushes Previous / Next below the visible part of the card)
        private IEnumerator ScrollAndTap(VisualElement target)
        {
            _harness.Sheet.Stack.Scroll.ScrollTo(target);
            yield return CardTestInput.Settle(0.15f);
            yield return CardTestInput.Tap(target.panel, target.worldBound.center);
            yield return null;
        }

        private static string StateKeyOf(string entry, int row = 0) =>
            "ts.card.gallery." + entry + "." + CardGalleryDefinitions.QuizBlockKey + ".answer-" + row;
    }
}
