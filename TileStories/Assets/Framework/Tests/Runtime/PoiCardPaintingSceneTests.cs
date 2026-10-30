using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of _3.2 step 17b on the REAL LivingRoomScene with real touches: the demo point. The Painting (the Ribeira Palace on the
    // panel) was given its card with the Card Starter's Rich point set plus hand editing, and the wall writes its OWN navigation (a
    // preset applied in Detail Card > Navigation) -- so this fixture, alone among the card fixtures, undoes SearchSceneFixture's pin to the
    // framework default and measures what the wall ships (ShippedNavigation):
    //   - every authored block of The Painting shows, in the order the card's own builder gives, its title in English and Portuguese
    //   - Lamp - Residential draws its contents block and nothing of its card is left out
    //   - renders at 390 px: The Painting in EN and PT at Half and Full, and Lamp - Residential's contents block
    [Category("Core")]
    public class PoiCardPaintingSceneTests : SearchSceneFixture
    {
        protected const string Painting = "painting";

        protected const string Residential = "lamp_residential";

        protected PoiCardSheetView Sheet => Card.Sheet;

        protected ScrollView Scroll => Sheet.Stack.Scroll;

        protected float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;

        protected List<string> _shippedLanguages;

        [UnitySetUp]
        public IEnumerator UseAnInMemoryCardStateAndTheWallsOwnNavigation()
        {
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            Assert.IsNotNull(ShippedNavigation, "precondition: the wall writes its own navigation (Detail Card > Navigation, a preset applied)");
            yield return Push(null);
        }

        // ---------------- helpers ----------------

        // The wall as it ships -- its own navigation back over the fixture's pin -- in `language` first, pushed through the live seam
        protected IEnumerator Push(Action<WallConfigData> edit, string language = "en")
        {
            var copy = ConfigCopy();
            _shippedLanguages ??= new List<string>(copy.card_settings.languages);
            copy.card_settings.navigation = JsonUtility.FromJson<CardNavigationSettings>(JsonUtility.ToJson(ShippedNavigation));
            // - the wall's preset rows (its tabs, its Menu); the wall's Navigation Bar is measured by PoiCardNavBarSceneTests
            copy.card_settings.navigation.bar.enabled = false;
            copy.card_settings.languages = language == "en" ? new List<string>(_shippedLanguages) : new List<string> { language, "en" };
            edit?.Invoke(copy);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        protected IEnumerator Open(string poi, SheetStopRule.Stop stop)
        {
            if (SelectionEventBus.CurrentPoiId != poi)
            {
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
                SelectionEventBus.Select(poi);
                yield return CardTestInput.Settle();
            }
            Scroll.scrollOffset = Vector2.zero;
            Sheet.SetStop(stop);
            yield return CardTestInput.Settle();
            Assert.AreEqual(stop, Sheet.Stop, "precondition: the card rests at " + stop);
            float rest = Sheet.Stops.HeightOf(Sheet.Stop);
            for (int frame = 0; frame < 600 && Mathf.Abs(Sheet.Root.layout.height - rest) > OnePixel; frame++) yield return null;
            Assert.AreEqual(rest, Sheet.Root.layout.height, OnePixel, "precondition: the sheet rests at its " + stop + " height");
            yield return CardTestInput.Settle(0.3f);
        }

        // ---------------- The Painting's card ----------------

        [UnityTest]
        public IEnumerator ThePaintingsCard_ShowsEveryAuthoredBlock_InTheBuildersOrder_InEnglishAndPortuguese()
        {
            var config = ConfigCopy();
            var painting = config.pois.Single(p => p.id == Painting);
            var built = BlockStackBuilder.Build(painting, config.card_settings, BlockRegistry.Shared, config.pois);
            Assert.IsEmpty(built.Skipped.Select(s => s.Instance.kind + " " + s.Reason + " " + s.FieldKey),
                "every block of The Painting has what it needs: nothing the starter left empty is left out");
            Assert.AreEqual(painting.card.blocks.Count, built.Entries.Count(e => !e.Synthesized), "every authored block is on the card");
            Assert.IsFalse(built.Entries[0].Synthesized, "the authored header, not the point's name and summary");
            // - the expected titles come from the wall's own header block (40-testing 4.2.4b), and the two languages really differ
            string Title(string language) => new BlockFieldReader(built.Entries[0].Instance, language, "en").Text(BlockStackBuilder.HeaderTitleField);
            Assert.AreNotEqual(Title("en"), Title("pt"), "precondition: the header has its own Portuguese title");

            foreach (string language in new[] { "en", "pt" })
            {
                yield return Push(null, language);
                yield return Open(Painting, SheetStopRule.Stop.Full);
                var header = Sheet.Stack.BoundViews.OfType<HeaderBlockView>().Single();
                Assert.AreEqual(Title(language), header.TitleText, language + ": the card's title");
                // - the stack's own order: each block's slot below the one before, in the builder's order (Sources included, which is
                //   no navigation target: its family is left out of navigation)
                //   -- a footer look (the sticky action) is pinned outside the scroll, shown or not; a block waiting for the card to be read
                //   (Show After Reading, a fresh visitor) is hidden until then
                var scrolled = built.Entries.Skip(1).Where(e => !e.Definition.IsFooter(e.Variant) && !(e.Definition.ShowAfterViewedField != null
                    && new BlockFieldReader(e.Instance, null, null).Flag(e.Definition.ShowAfterViewedField))).ToList();
                var slots = scrolled.Select(e => Sheet.Stack.SlotOfBlock(e.Instance.key)).ToList();
                Assert.IsTrue(slots.All(s => s != null), language + ": every block under the header has its slot in the stack");
                for (int i = 1; i < slots.Count; i++)
                    Assert.Greater(slots[i].worldBound.yMin, slots[i - 1].worldBound.yMin, language + ": " + scrolled[i].Instance.kind + " comes after " + scrolled[i - 1].Instance.kind);
                yield return CardGalleryChecks.Render("Painting_" + language + "_full_390");
                Sheet.SetStop(SheetStopRule.Stop.Half);
                yield return CardTestInput.Settle(0.5f);
                yield return CardGalleryChecks.Render("Painting_" + language + "_half_390");
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
            }
            Assert.AreEqual(ConfigCopy().pois.Single(p => p.id == Painting).name, Session.SearchPois.Single(p => p.id == Painting).name, "the card never renames the point");
        }

        // ---------------- Lamp - Residential: the whole domain on one card ----------------

        [UnityTest]
        public IEnumerator LampResidential_DrawsItsContentsBlock_AndNothingOfItsCardIsLeftOut()
        {
            yield return Open(Residential, SheetStopRule.Stop.Full);
            var bound = Sheet.Stack.BoundViews.ToList();
            Assert.AreEqual(1, bound.OfType<NavContentsBlockView>().Count(), "a contents list, its one in-flow nav block");
            var config = ConfigCopy();
            var poi = config.pois.Single(p => p.id == Residential);
            Assert.IsEmpty(BlockStackBuilder.Build(poi, config.card_settings, BlockRegistry.Shared, config.pois).Skipped, "nothing of its card is left out");

            var view = bound.OfType<NavContentsBlockView>().Single();
            Scroll.ScrollTo(view.Root);
            yield return CardTestInput.Settle(0.4f);
            Assert.Greater(view.Root.layout.height, 0f, "the contents list: drawn, not empty");
            yield return CardGalleryChecks.Render("Residential_" + nameof(NavContentsBlockView) + "_full_390");
        }
    }
}
