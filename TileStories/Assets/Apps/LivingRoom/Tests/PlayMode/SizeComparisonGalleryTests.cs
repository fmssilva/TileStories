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
            public readonly string CaptionPt;
            public readonly bool Heading;
            public readonly bool Service;
            // What the block calls the point's drawing under it (null: not authored, the card's title names it)
            public readonly string Label;
            public readonly string LabelPt;

            public Case(string content, string objectKey, float width, float height, string caption, bool heading = false, bool service = true, string captionPt = null,
                string label = null, string labelPt = null)
            {
                Label = label;
                LabelPt = labelPt;
                Content = content;
                Object = objectKey;
                Width = width;
                Height = height;
                Caption = caption;
                CaptionPt = captionPt;
                Heading = heading;
                Service = service;
            }
        }

        // What the app's card texts say under each familiar object, in English and Portuguese (the authored words, written out here on purpose)
        private static readonly System.Collections.Generic.Dictionary<string, (string En, string Pt)> ObjectNames = new()
        {
            { FamiliarObjects.CreditCard, ("Credit card", "Cart\u00e3o de cr\u00e9dito") },
            { FamiliarObjects.TwoEuroCoin, ("2 euro coin", "Moeda de 2 euros") },
            { FamiliarObjects.Smartphone, ("Smartphone", "Telem\u00f3vel") },
            { FamiliarObjects.SheetA4, ("A4 sheet", "Folha A4") },
        };

        private static readonly Case[] Cases =
        {
            new("phone", FamiliarObjects.Smartphone, 32f, 58f, "As tall as almost four phones standing on top of each other.", heading: true),
            // - a small point beside a coin that is a real, visible circle (the other coin case is a speck)
            new("coin_round", FamiliarObjects.TwoEuroCoin, 6f, 6f, "As wide as two and a half coins.", captionPt: "T\u00e3o largo como duas moedas e meia."),
            new("card_long", FamiliarObjects.CreditCard, 120f, 70f,
                "About fourteen credit cards wide and eight of them tall, which is a good deal more than it looks from across the room, so " +
                "the caption runs over several lines of the phone-width card and must wrap inside it."),
            new("sheet_bigger", FamiliarObjects.SheetA4, 8f, 10f, "Smaller than a sheet of paper."),
            new("coin_tiny", FamiliarObjects.TwoEuroCoin, 120f, 200f, "A coin is a speck next to it."),
            new("no_service", FamiliarObjects.Smartphone, 32f, 58f, "The point alone.", service: false),
            // - the block names its drawing: a card titled after a whole building would read as phone-sized without it
            new("phone_labelled", FamiliarObjects.Smartphone, 32f, 58f, "As tall as almost four phones standing on top of each other.",
                captionPt: "Tão alto como quase quatro telemóveis empilhados.", label: "The tile panel", labelPt: "O painel de azulejos"),
        };

        private static string[] CaseNames => Cases.Select(c => c.Content).ToArray();
        // The cases where the app's service is on the card: the ones with an object to draw
        private static string[] ServiceCaseNames => Cases.Where(c => c.Service).Select(c => c.Content).ToArray();

        private static Case CaseOf(string content) => Cases.Single(c => c.Content == content);

        private static CardGalleryDefinitions.Entry EntryOf(Case c, System.Action<WallConfigData> wallSetup = null)
        {
            var block = new BlockInstanceData { key = "block_2", kind = SizeComparisonBlock.Kind, variant = SizeComparisonBlock.SideBySide };
            if (c.Heading)
                block.fields.Add(new BlockFieldValue { key = BlockKindDefinition.HeadingField, text = En(CardGalleryDefinitions.HeadingFor(SizeComparisonBlock.Kind)) });
            block.fields.Add(new BlockFieldValue { key = SizeComparisonBlock.ObjectField, value = c.Object });
            block.fields.Add(new BlockFieldValue { key = SizeComparisonBlock.WidthField, number = c.Width });
            block.fields.Add(new BlockFieldValue { key = SizeComparisonBlock.HeightField, number = c.Height });
            var caption = En(c.Caption);
            if (c.CaptionPt != null) caption.Add(new LocalizedEntry { lang = "pt", value = c.CaptionPt });
            block.fields.Add(new BlockFieldValue { key = SizeComparisonBlock.CaptionField, text = caption });
            if (c.Label != null)
            {
                var label = En(c.Label);
                if (c.LabelPt != null) label.Add(new LocalizedEntry { lang = "pt", value = c.LabelPt });
                block.fields.Add(new BlockFieldValue { key = SizeComparisonBlock.PoiLabelField, text = label });
            }
            return new CardGalleryDefinitions.Entry(SizeComparisonBlock.Kind, SizeComparisonBlock.SideBySide, c.Content, block, wallSetup: wallSetup);
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
        private IEnumerator Show(Case c, System.Action<SizeComparisonBlockView> got, System.Action<WallConfigData> wallSetup = null)
        {
            _harness.Services = c.Service ? CardServices.Shared : new CardServices();
            CardGalleryChecks.ShowEntry(_harness, EntryOf(c, wallSetup));
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

            // - as large as the stage allows: the taller one fills the height, or the two slots (each as wide as its shape or its name, with
            //   the gap) fill the width
            bool fillsHeight = Mathf.Abs(Mathf.Max(poi.height, obj.height) - stage.height) <= tol;
            float slots = view.ObjectSlot.worldBound.xMax - view.PoiSlot.worldBound.xMin;
            bool fillsWidth = Mathf.Abs(slots - stage.width) <= tol + 1f;
            Assert.IsTrue(fillsHeight || fillsWidth, content + ": neither the height (" + Mathf.Max(poi.height, obj.height) + " of " + stage.height + ") nor the width ("
                + slots + " of " + stage.width + ") is used up");

            // - the larger real size is the larger drawing
            Assert.AreEqual(c.Height >= familiar.HeightCm, poi.height >= obj.height - tol, content + ": the drawing orders the sizes as the real ones");

            // - the shapes read against the card (non-text parts of an interface: 3:1)
            Color surface = _harness.Sheet.Root.resolvedStyle.backgroundColor;
            Assert.GreaterOrEqual(CardTestInput.Contrast(view.PoiShape.resolvedStyle.borderTopColor, surface), UIAccessibility.MinRatioLargeTextOrUIComponent, content + ": the point's outline");
            Assert.GreaterOrEqual(CardTestInput.Contrast(OverSurface(view.ObjectShape.resolvedStyle.backgroundColor, surface), surface), UIAccessibility.MinRatioLargeTextOrUIComponent, content + ": the object's fill");
        }

        // The card's own title, as the visitor reads it in the pinned header
        private string HeaderTitle() => ((HeaderBlockView)_harness.Sheet.Stack.BoundViews[0]).TitleText;

        [UnityTest]
        public IEnumerator EveryEntry_NamesBothShapes_TheCardsTitleAndTheAppsWord_EachCentredUnderItsOwnShape([ValueSource(nameof(ServiceCaseNames))] string content)
        {
            var c = CaseOf(content);
            SizeComparisonBlockView view = null;
            yield return Show(c, v => view = v);
            Assert.IsNotEmpty(HeaderTitle(), "precondition: the card has a title");
            Assert.AreEqual(c.Label ?? HeaderTitle(), view.PoiName.text, content + ": the point is named by the block's Point Label, else the card's own title");
            Assert.AreEqual(ObjectNames[c.Object].En, view.ObjectName.text, content + ": the object is named by the app's card texts");
            Assert.AreNotEqual(view.PoiName.text, view.ObjectName.text, content + ": the two shapes are told apart by words, not by colour alone");
            Assert.IsTrue(CardTestInput.IsShown(view.PoiName, view.Root), content + ": the point's name is on the card");
            Assert.IsTrue(CardTestInput.IsShown(view.ObjectName, view.Root), content + ": the object's name is on the card");

            Rect stage = view.Stage.worldBound, poi = view.PoiShape.worldBound, obj = view.ObjectShape.worldBound;
            Rect poiName = view.PoiName.worldBound, objName = view.ObjectName.worldBound;
            float tol = CardGalleryChecks.OnePixel(view.Stage) * 2f + 0.5f;
            // - each name sits under the ground line, centred under its own shape
            Assert.GreaterOrEqual(poiName.yMin, stage.yMax - tol, content + ": the point's name is under the ground line");
            Assert.GreaterOrEqual(objName.yMin, stage.yMax - tol, content + ": the object's name is under the ground line");
            Assert.AreEqual(poi.center.x, poiName.center.x, tol, content + ": the point's name is centred under the point");
            Assert.AreEqual(obj.center.x, objName.center.x, tol, content + ": the object's name is centred under the object");
            // - and never runs into the other name, or out of the card
            Assert.LessOrEqual(poiName.xMax, objName.xMin + tol, content + ": the names do not overlap");
            Rect card = _harness.Sheet.Root.worldBound;
            Assert.GreaterOrEqual(poiName.xMin, card.xMin - tol, content + ": inside the card (left)");
            Assert.LessOrEqual(objName.xMax, card.xMax + tol, content + ": inside the card (right)");
            // - a name is as wide as its slot: a coin's name is far wider than the coin
            Assert.GreaterOrEqual(objName.width + tol, view.ObjectShape.worldBound.width, content + ": a name is at least as wide as its shape");
        }

        [UnityTest]
        public IEnumerator ThePointLabel_NamesTheDrawing_InEachLanguage_AndAnEmptyOneFallsBackToTheCardsTitle()
        {
            var c = CaseOf("phone_labelled");
            SizeComparisonBlockView view = null;
            yield return Show(c, v => view = v);
            Assert.AreNotEqual(HeaderTitle(), view.PoiName.text, "precondition: the card's title is not the drawing's name");
            Assert.AreEqual("The tile panel", view.PoiName.text, "the box is labelled as what it is, not as the whole building");
            Assert.IsTrue(CardTestInput.IsShown(view.PoiName, view.Root));
            float tol = CardGalleryChecks.OnePixel(view.Stage) * 2f + 0.5f;
            Assert.AreEqual(view.PoiShape.worldBound.center.x, view.PoiName.worldBound.center.x, tol, "still centred under its own shape");
            yield return CardGalleryChecks.Render("Card_size_comparison_phone_labelled");

            _harness.Language = "pt";
            yield return Show(c, v => view = v);
            Assert.AreEqual("O painel de azulejos", view.PoiName.text, "the Portuguese label");
            CardGalleryChecks.AssertBlockEntry(_harness, EntryOf(c), view);
            yield return CardGalleryChecks.Render("Card_size_comparison_phone_labelled_pt");

            // - no label in the visitor's language: the wall's first language (English) names it, never the building's title
            _harness.Language = "pt";
            var onlyEnglish = new Case("phone_label_en_only", FamiliarObjects.Smartphone, 32f, 58f, "x", label: "The tile panel");
            yield return Show(onlyEnglish, v => view = v);
            Assert.AreEqual("The tile panel", view.PoiName.text, "a Portuguese card with an English-only label reads the English one");
        }

        [UnityTest]
        public IEnumerator TheCoin_IsDrawnRound_TheOtherObjectsAreRectangles()
        {
            SizeComparisonBlockView view = null;
            yield return Show(CaseOf("coin_round"), v => view = v);
            var coin = view.ObjectShape;
            float onePixel = CardGalleryChecks.OnePixel(view.Stage) + 0.01f;
            Assert.Greater(coin.resolvedStyle.width, 30f, "precondition: a coin big enough to judge");
            Assert.AreEqual(coin.resolvedStyle.width, coin.resolvedStyle.height, onePixel, "a coin is as tall as it is wide");
            Assert.IsTrue(coin.ClassListContains("card-size__shape--round"), "the app's table says the coin is round");
            Assert.GreaterOrEqual(coin.resolvedStyle.borderTopLeftRadius, coin.resolvedStyle.width / 2f - onePixel, "its corners are rounded all the way: a circle");
            // - the point (a rectangle) and a phone keep the card's small corner
            Assert.AreEqual(CardTestInput.TokenPx("--ts-radius-s"), view.PoiShape.resolvedStyle.borderTopLeftRadius, onePixel, "the point's corner is --ts-radius-s");
            yield return Show(CaseOf("phone"), v => view = v);
            Assert.IsFalse(view.ObjectShape.ClassListContains("card-size__shape--round"), "a phone is not round");
            Assert.AreEqual(CardTestInput.TokenPx("--ts-radius-s"), view.ObjectShape.resolvedStyle.borderTopLeftRadius, onePixel, "a phone's corner is --ts-radius-s");
            yield return CardGalleryChecks.Render("Card_size_comparison_coin_round_check");
        }

        [UnityTest]
        public IEnumerator InPortuguese_TheObjectsNameTheCaptionAndTheCardsTitleFollowTheLanguage_AndAWallsWordingWinsOverTheApps()
        {
            _harness.Language = "pt";
            var c = CaseOf("coin_round");
            SizeComparisonBlockView view = null;
            yield return Show(c, v => view = v);
            Assert.AreEqual("Moeda de 2 euros", view.ObjectName.text, "the app's Portuguese word");
            Assert.AreEqual(c.CaptionPt, view.Caption.text, "the authored Portuguese caption");
            Assert.AreEqual(HeaderTitle(), view.PoiName.text, "the point's name is the card's title in this language too");
            CardGalleryChecks.AssertBlockEntry(_harness, EntryOf(c), view);
            yield return CardGalleryChecks.Render("Card_size_comparison_coin_round_pt");

            // - the wall rewords the app's word in Detail Card > Card Texts: its wording wins, in its own language only
            yield return Show(c, v => view = v, wall => wall.card_settings.strings.Add(new CardStringEntry
            {
                key = LivingRoomCardTexts.Keys.ObjectTwoEuroCoin, text = new System.Collections.Generic.List<LocalizedEntry> { new() { lang = "pt", value = "Dois euros" } },
            }));
            Assert.AreEqual("Dois euros", view.ObjectName.text, "the wall's Portuguese wording wins over the app's");
            _harness.Language = "en";
            yield return Show(c, v => view = v, wall => wall.card_settings.strings.Add(new CardStringEntry
            {
                key = LivingRoomCardTexts.Keys.ObjectTwoEuroCoin, text = new System.Collections.Generic.List<LocalizedEntry> { new() { lang = "pt", value = "Dois euros" } },
            }));
            Assert.AreEqual("2 euro coin", view.ObjectName.text, "the wall reworded only Portuguese: English keeps the app's word");
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
            Assert.AreEqual(DisplayStyle.None, view.ObjectName.resolvedStyle.display, "no object, no name for it");
            Assert.AreEqual(HeaderTitle(), view.PoiName.text, "the point is still named");
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
            Assert.AreEqual(CardTestInput.TokenPx("--ts-space-4"), view.ObjectSlot.resolvedStyle.marginLeft, onePixel, "the gap between them is --ts-space-4");
            Assert.AreEqual(view.ObjectSlot.resolvedStyle.marginLeft, view.ObjectName.resolvedStyle.marginLeft, onePixel, "the second name keeps the second slot's gap");
            Assert.AreEqual(CardTestInput.TokenPx("--ts-size-caption"), view.PoiName.resolvedStyle.fontSize, onePixel, "a name is set in --ts-size-caption");
            Assert.AreEqual(CardTestInput.TokenPx("--ts-choice-border"), view.PoiShape.resolvedStyle.borderTopWidth, onePixel, "the outline's width is --ts-choice-border");
            Assert.AreEqual(view.PoiShape.resolvedStyle.borderTopWidth, view.ObjectShape.resolvedStyle.borderTopWidth, 0.01f, "one outline for both");
        }
    }
}
