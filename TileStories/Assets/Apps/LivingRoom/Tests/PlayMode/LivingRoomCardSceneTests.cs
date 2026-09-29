using System.Collections;
using System.Linq;
using NUnit.Framework;
using TileStories.Tests;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TileStories.LivingRoom.Tests
{
    // Phase B of the app's own block kind (_3.1 step 11): the REAL LivingRoomScene, its real PoiCardHost, The Lamp's shipped card. The
    // kind and its service are there because the app's own startup registered them (no scene wiring, no Framework field), the card
    // shows the block the wall's config authored, drawn from the app's service, in the card's language.
    public class LivingRoomCardSceneTests : SearchSceneFixture
    {
        private PoiCardSheetView Sheet => Card.Sheet;

        private IEnumerator OpenFull(string poiId)
        {
            SelectionEventBus.Select(poiId);
            yield return CardTestInput.Settle();
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
        }

        private IEnumerator ScrollTo(IBlockView view)
        {
            Sheet.Stack.Scroll.ScrollTo(Sheet.Stack.SlotOf(view));
            yield return CardTestInput.Settle(0.2f);
        }

        private SizeComparisonBlockView SizeView() => Sheet.Stack.BoundViews.OfType<SizeComparisonBlockView>().Single();

        // The Lamp's block as the wall's config authored it (a copy of the running config, not the view's own reading)
        private BlockInstanceData AuthoredBlock() => ConfigCopy().pois.Single(p => p.id == "lamp").card.blocks.Single(b => b.kind == SizeComparisonBlock.Kind);

        [UnityTest]
        public IEnumerator TheLamp_ShowsTheAppsSizeComparison_BeforeTheSources_DrawnToTheAuthoredScale_WithTheAuthoredWords()
        {
            yield return OpenFull("lamp");
            var views = Sheet.Stack.BoundViews.ToList();
            var size = SizeView();
            Assert.Less(views.IndexOf(size), views.FindIndex(v => v is SourcesBlockView), "before the sources, which close the card");
            Assert.IsTrue(size.ObjectKnown, "the app's service reached the view through the real card host");

            var authored = AuthoredBlock();
            var read = new BlockFieldReader(authored, "en", "en");
            float width = read.Number(SizeComparisonBlock.WidthField), height = read.Number(SizeComparisonBlock.HeightField);
            Assert.AreEqual(32f, width, "the fixture's point is 32 cm wide");
            Assert.AreEqual(58f, height);

            yield return ScrollTo(size);
            Assert.AreEqual("How big is it?", Sheet.Stack.HeadingOf(size).text, "the authored heading, drawn by the stack");
            Assert.AreEqual("As tall as almost four phones standing on top of each other.", size.Caption.text, "the authored caption: no word of the view's own");
            Assert.IsTrue(CardTestInput.IsShown(size.Caption, size.Root));

            Rect card = Sheet.Root.worldBound, poi = size.PoiShape.worldBound, obj = size.ObjectShape.worldBound, stage = size.Stage.worldBound;
            float tol = CardGalleryChecks.OnePixel(size.Stage) * 2f + 0.5f;
            Assert.Greater(poi.height, 40f, "a real drawing");
            float scale = poi.height / height;
            Assert.AreEqual(width * scale, poi.width, tol, "the point's proportions are the authored 32 x 58");
            // - the phone (7.2 x 15 cm) beside it at the same scale: its size comes from the app's table, not from the config
            Assert.AreEqual(15f * scale, obj.height, tol, "a phone is 15 cm tall on the point's scale");
            Assert.AreEqual(7.2f * scale, obj.width, tol);
            Assert.AreEqual(poi.yMax, obj.yMax, tol, "one ground line");
            Assert.GreaterOrEqual(poi.xMin, card.xMin - tol, "inside the card");
            Assert.LessOrEqual(obj.xMax, card.xMax + tol);
            Assert.LessOrEqual(stage.yMax, card.yMax + tol);

            // - a name under each shape (step 11-fix): the card's own title under the point, the app's word under the phone
            string title = HeaderTitle();
            Assert.IsNotEmpty(title, "precondition: the card has a title");
            Assert.AreEqual(title, size.PoiName.text, "the point is named by the card's own title");
            Assert.AreEqual("Smartphone", size.ObjectName.text, "the phone is named by the app's card texts");
            Assert.GreaterOrEqual(size.PoiName.worldBound.yMin, stage.yMax - tol, "the names sit under the ground line");
            Assert.AreEqual(poi.center.x, size.PoiName.worldBound.center.x, tol, "the point's name is centred under the point");
            Assert.AreEqual(obj.center.x, size.ObjectName.worldBound.center.x, tol, "the phone's name is centred under the phone");
            Assert.LessOrEqual(size.PoiName.worldBound.xMax, size.ObjectName.worldBound.xMin + tol, "the two names do not overlap");
            Assert.IsTrue(CardTestInput.IsShown(size.PoiName, size.Root) && CardTestInput.IsShown(size.ObjectName, size.Root), "both names are on the card");
            yield return Capture("Lamp_SizeComparison");
        }

        // The card's own title, as the visitor reads it in the pinned header
        private string HeaderTitle() => ((HeaderBlockView)Sheet.Stack.BoundViews[0]).TitleText;

        [UnityTest]
        public IEnumerator AWallsCardTextsWording_RewordsTheAppsObjectName_OnTheRealCard_InThatLanguageOnly()
        {
            Session.CardSettings.strings.Add(new CardStringEntry
            {
                key = LivingRoomCardTexts.Keys.ObjectSmartphone,
                text = new System.Collections.Generic.List<LocalizedEntry> { new() { lang = "en", value = "Mobile phone" } },
            });
            yield return OpenFull("lamp");
            Assert.AreEqual("Mobile phone", SizeView().ObjectName.text, "the wall's wording beats the app's on the real card");
            Session.CardSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            yield return OpenFull("lamp_military");
            yield return OpenFull("lamp");
            Assert.AreEqual("Telem\u00f3vel", SizeView().ObjectName.text, "the wall reworded English only: Portuguese keeps the app's word");
        }

        [UnityTest]
        public IEnumerator InPortuguese_TheHeadingAndTheCaptionFollowTheCardsLanguage()
        {
            Session.CardSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            yield return OpenFull("lamp");
            var size = SizeView();
            yield return ScrollTo(size);
            Assert.AreEqual("Qual é o tamanho?", Sheet.Stack.HeadingOf(size).text);
            Assert.AreEqual("T\u00e3o alto como quase quatro telem\u00f3veis empilhados.", size.Caption.text);
            Assert.AreEqual("Telem\u00f3vel", size.ObjectName.text, "the app's Portuguese word under the phone");
            Assert.AreEqual(HeaderTitle(), size.PoiName.text, "the point's name is the card's title in Portuguese too");
            yield return Capture("Lamp_SizeComparison_pt");
        }

        [UnityTest]
        public IEnumerator TheKindGetsItsDependencyFromTheCardsServices_TakeTheServiceAway_AndTheObjectIsGone_PutItBack_AndItReturns()
        {
            yield return OpenFull("lamp");
            Assert.IsTrue(SizeView().ObjectKnown, "precondition: the app registered its service at startup");
            Assert.IsTrue(CardServices.Shared.Remove<IFamiliarObjects>());
            try
            {
                // - another card, then back: the view is bound again with the registry as it is now
                yield return OpenFull("lamp_military");
                yield return OpenFull("lamp");
                var size = SizeView();
                Assert.IsFalse(size.ObjectKnown, "no service on the card: the kind draws the point alone, never a made-up object");
                Assert.AreEqual(DisplayStyle.None, size.ObjectShape.resolvedStyle.display);
                Assert.Greater(size.PoiShape.resolvedStyle.height, 10f, "the point is still drawn");
            }
            finally
            {
                // - what the app's own startup does
                LivingRoomBlocks.Register(BlockRegistry.Shared, CardServices.Shared, CardStringSources.Shared);
            }
            yield return OpenFull("lamp_military");
            yield return OpenFull("lamp");
            Assert.IsTrue(SizeView().ObjectKnown, "registered again: the object is back");
        }

        [Test]
        public void TheWallScene_WiresTheAppsOwnStylesheet_AfterTheFrameworksBlockSheets()
        {
#if UNITY_EDITOR
            var paths = Card.BlockStyles.Select(s => AssetDatabase.GetAssetPath(s)).ToList();
            int app = paths.IndexOf("Assets/Apps/LivingRoom/Scripts/SizeComparison/SizeComparison.uss");
            Assert.GreaterOrEqual(app, 0, "the wall's card host adds the app's sheet");
            Assert.AreEqual(paths.Count - 1, app, "after every framework sheet: the app can only refine");
#endif
        }
    }
}
