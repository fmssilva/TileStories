using System.Collections;
using System.Linq;
using NUnit.Framework;
using TileStories.Tests;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace TileStories.LivingRoom.Tests
{
    // Phase A of the app's own block kind (_3.1 step 11, 40-testing.md 4.4): size_comparison in the framework's isolated card gallery
    // (Assets/Dev/CardGallery), fed fabricated content -- no AR, no config.json. Every entry goes through the SAME generic checks a
    // built-in kind does (CardGalleryChecks.AssertBlockEntry: fit, contrast, tap targets, heading, gap) and a render, then through the
    // promises of THIS kind: both shapes at one scale on one ground line, as large as the stage allows, and the point drawn alone when
    // the app's service is absent. The app's stylesheet is added to the gallery's frame here: the framework's gallery names no app.
    public class SizeComparisonGalleryTests
    {
        private const string GalleryScene = "Assets/Dev/CardGallery/CardGalleryScene.unity";
        private const string StylePath = "Assets/Apps/LivingRoom/Scripts/SizeComparison/SizeComparison.uss";
        private CardGalleryHarness _harness;

        // One content state: the object, the point's real size, the words, and whether the app's service is registered
        private readonly struct Case
        {
            public readonly string Content;
            public readonly string Object;
            public readonly float Width, Height;
            public readonly string Caption;
            public readonly bool Heading;
            public readonly bool Service;

            public Case(string content, string objectKey, float width, float height, string caption, bool heading = false, bool service = true)
            {
                Content = content;
                Object = objectKey;
                Width = width;
                Height = height;
                Caption = caption;
                Heading = heading;
                Service = service;
            }
        }

        private static readonly Case[] Cases =
        {
            new("phone", FamiliarObjects.Smartphone, 32f, 58f, "As tall as almost four phones standing on top of each other.", heading: true),
            new("card_long", FamiliarObjects.CreditCard, 120f, 70f,
                "About fourteen credit cards wide and eight of them tall, which is a good deal more than it looks from across the room, so " +
                "the caption runs over several lines of the phone-width card and must wrap inside it."),
            new("sheet_bigger", FamiliarObjects.SheetA4, 8f, 10f, "Smaller than a sheet of paper."),
            new("coin_tiny", FamiliarObjects.TwoEuroCoin, 120f, 200f, "A coin is a speck next to it."),
            new("no_service", FamiliarObjects.Smartphone, 32f, 58f, "The point alone.", service: false),
        };

        private static string[] CaseNames => Cases.Select(c => c.Content).ToArray();
        // The cases where the app's service is on the card: the ones with an object to draw
        private static string[] ServiceCaseNames => Cases.Where(c => c.Service).Select(c => c.Content).ToArray();

        private static Case CaseOf(string content) => Cases.Single(c => c.Content == content);

        private static CardGalleryDefinitions.Entry EntryOf(Case c)
        {
            var block = new BlockInstanceData { key = "block_2", kind = SizeComparisonBlock.Kind, variant = SizeComparisonBlock.SideBySide };
            if (c.Heading)
                block.fields.Add(new BlockFieldValue { key = BlockKindDefinition.HeadingField, text = En(CardGalleryDefinitions.HeadingPrefix + SizeComparisonBlock.Kind.Replace('_', ' ')) });
            block.fields.Add(new BlockFieldValue { key = SizeComparisonBlock.ObjectField, value = c.Object });
            block.fields.Add(new BlockFieldValue { key = SizeComparisonBlock.WidthField, number = c.Width });
            block.fields.Add(new BlockFieldValue { key = SizeComparisonBlock.HeightField, number = c.Height });
            block.fields.Add(new BlockFieldValue { key = SizeComparisonBlock.CaptionField, text = En(c.Caption) });
            return new CardGalleryDefinitions.Entry(SizeComparisonBlock.Kind, SizeComparisonBlock.SideBySide, c.Content, block);
        }

        private static System.Collections.Generic.List<LocalizedEntry> En(string value) => new() { new LocalizedEntry { lang = "en", value = value } };

        [UnitySetUp]
        public IEnumerator SetUp()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(GalleryScene, new LoadSceneParameters(LoadSceneMode.Single));
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
#if UNITY_EDITOR
            var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StylePath);
            Assert.IsNotNull(sheet, "the app's stylesheet is there");
            _harness.Frame.styleSheets.Add(sheet);
#endif
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var scene = SceneManager.GetSceneByPath(GalleryScene);
            SceneManager.SetActiveScene(SceneManager.CreateScene("AfterSizeComparisonGalleryTest"));
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
        }

        // Show one case (with or without the app's service on the card) and hand back its view
        private IEnumerator Show(Case c, System.Action<SizeComparisonBlockView> got)
        {
            _harness.Services = c.Service ? CardServices.Shared : new CardServices();
            CardGalleryChecks.ShowEntry(_harness, EntryOf(c));
            IBlockView view = null;
            yield return CardGalleryChecks.SettledBlock(_harness, c.Content, v => view = v);
            got((SizeComparisonBlockView)view);
        }

        // A colour drawn over the card's surface, as the eye sees it (a shape's fill may be translucent)
        private static Color OverSurface(Color colour, Color surface) => Color.Lerp(surface, new Color(colour.r, colour.g, colour.b, 1f), colour.a);

        [UnityTest]
        public IEnumerator EveryEntry_GoesThroughTheGenericChecks_AndRenders([ValueSource(nameof(CaseNames))] string content)
        {
            var c = CaseOf(content);
            SizeComparisonBlockView view = null;
            yield return Show(c, v => view = v);
            CardGalleryChecks.AssertBlockEntry(_harness, EntryOf(c), view);
            yield return CardGalleryChecks.Render("Card_" + EntryOf(c).Name);
        }

        [UnityTest]
        public IEnumerator EveryEntry_DrawsBothShapesAtOneScale_OnOneGroundLine_AsLargeAsTheStageAllows([ValueSource(nameof(ServiceCaseNames))] string content)
        {
            var c = CaseOf(content);
            SizeComparisonBlockView view = null;
            yield return Show(c, v => view = v);
            Assert.IsTrue(view.ObjectKnown, content + ": the app's service knew the object");
            new FamiliarObjects().TryGet(c.Object, out var familiar);

            Rect stage = view.Stage.worldBound, poi = view.PoiShape.worldBound, obj = view.ObjectShape.worldBound;
            // - layout snaps each edge to a physical pixel: a shape's size is right to about two of them
            float tol = CardGalleryChecks.OnePixel(view.Stage) * 2f + 0.5f;
            Assert.Greater(poi.width, 0f, content + ": the point is drawn");
            Assert.Greater(poi.height, 0f);

            // - ONE scale: the point sets it (its own size in cm to pixels, both ways); the object at that same scale is its true size
            float scale = poi.height / c.Height;
            Assert.AreEqual(c.Width * scale, poi.width, tol, content + ": the point keeps its proportions");
            Assert.AreEqual(familiar.HeightCm * scale, obj.height, tol, content + ": the object is drawn at the point's scale (height)");
            Assert.AreEqual(familiar.WidthCm * scale, obj.width, tol, content + ": ...and width");

            // - one ground line: both stand on the bottom of the stage, inside it
            Assert.AreEqual(stage.yMax, poi.yMax, tol, content + ": the point stands on the stage's floor");
            Assert.AreEqual(poi.yMax, obj.yMax, tol, content + ": the object stands on the same line");
            Assert.GreaterOrEqual(poi.yMin, stage.yMin - tol, content + ": nothing above the stage");
            Assert.GreaterOrEqual(obj.yMin, stage.yMin - tol);
            Assert.GreaterOrEqual(poi.xMin, stage.xMin - tol, content + ": nothing left of the stage");
            Assert.LessOrEqual(obj.xMax, stage.xMax + tol, content + ": nothing right of the stage");
            Assert.Greater(obj.xMin, poi.xMax, content + ": the object beside the point, not over it");

            // - as large as the stage allows: the taller one fills the height, or the pair (with its gap) fills the width
            bool fillsHeight = Mathf.Abs(Mathf.Max(poi.height, obj.height) - stage.height) <= tol;
            bool fillsWidth = Mathf.Abs((obj.xMax - poi.xMin) - stage.width) <= tol + 1f;
            Assert.IsTrue(fillsHeight || fillsWidth, content + ": neither the height (" + Mathf.Max(poi.height, obj.height) + " of " + stage.height + ") nor the width ("
                + (obj.xMax - poi.xMin) + " of " + stage.width + ") is used up");

            // - the larger real size is the larger drawing
            Assert.AreEqual(c.Height >= familiar.HeightCm, poi.height >= obj.height - tol, content + ": the drawing orders the sizes as the real ones");

            // - the shapes read against the card (non-text parts of an interface: 3:1)
            Color surface = _harness.Sheet.Root.resolvedStyle.backgroundColor;
            Assert.GreaterOrEqual(CardTestInput.Contrast(view.PoiShape.resolvedStyle.borderTopColor, surface), UIAccessibility.MinRatioLargeTextOrUIComponent, content + ": the point's outline");
            Assert.GreaterOrEqual(CardTestInput.Contrast(OverSurface(view.ObjectShape.resolvedStyle.backgroundColor, surface), surface), UIAccessibility.MinRatioLargeTextOrUIComponent, content + ": the object's fill");
        }

        [UnityTest]
        public IEnumerator WithoutTheAppsService_TheObjectIsHidden_TheCaptionStays_AndThePointIsStillDrawnToItsProportions()
        {
            var c = CaseOf("no_service");
            SizeComparisonBlockView view = null;
            yield return Show(c, v => view = v);
            Assert.IsFalse(view.ObjectKnown, "no IFamiliarObjects on this card's services");
            Assert.AreEqual(DisplayStyle.None, view.ObjectShape.resolvedStyle.display, "the object is not drawn at a made-up size");
            Assert.AreEqual(c.Caption, view.Caption.text);
            Rect poi = view.PoiShape.worldBound;
            Assert.Greater(poi.height, 10f, "the point is drawn");
            float tol = CardGalleryChecks.OnePixel(view.Stage) * 2f + 0.5f;
            Assert.AreEqual(c.Width / c.Height, poi.width / poi.height, 0.02f, "with its proportions");
            Assert.AreEqual(view.Stage.worldBound.height, poi.height, tol, "alone, a tall point takes the whole stage height");

            // - the service back on the card: the same case shows the object again (nothing was cached in the view)
            _harness.Services = CardServices.Shared;
            CardGalleryChecks.ShowEntry(_harness, EntryOf(c));
            yield return CardGalleryChecks.SettledBlock(_harness, c.Content, v => view = (SizeComparisonBlockView)v);
            Assert.IsTrue(view.ObjectKnown);
            Assert.AreNotEqual(DisplayStyle.None, view.ObjectShape.resolvedStyle.display);
        }

        [UnityTest]
        public IEnumerator TheStyleSheet_IsWhatDrawsIt_TheShapesTakeTheirLookFromTheTokens()
        {
            var c = CaseOf("phone");
            SizeComparisonBlockView view = null;
            yield return Show(c, v => view = v);
            Assert.AreEqual(CardTestInput.TokenPx("--ts-media-height"), view.Stage.resolvedStyle.height, CardGalleryChecks.OnePixel(view.Stage) + 0.01f, "the stage's height is --ts-media-height");
            // - layout snaps each edge to a physical pixel: one screen pixel in panel units is the honest tolerance
            float onePixel = CardGalleryChecks.OnePixel(view.Stage) + 0.01f;
            Assert.AreEqual(CardTestInput.TokenPx("--ts-space-4"), view.ObjectShape.resolvedStyle.marginLeft, onePixel, "the gap between them is --ts-space-4");
            Assert.AreEqual(CardTestInput.TokenPx("--ts-choice-border"), view.PoiShape.resolvedStyle.borderTopWidth, onePixel, "the outline's width is --ts-choice-border");
            Assert.AreEqual(view.PoiShape.resolvedStyle.borderTopWidth, view.ObjectShape.resolvedStyle.borderTopWidth, 0.01f, "one outline for both");
        }
    }
}
