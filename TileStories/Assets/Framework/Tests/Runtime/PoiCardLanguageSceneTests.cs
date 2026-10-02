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
    // Phase B of the visitor's language choice (_3.1 15.3.2): on the REAL LivingRoomScene, a real tap on the card's language chip shows The
    // Lamp's card in the other language in place (the sheet's stop and the stack's scroll kept), the pick is remembered for the wall's other
    // points, a text missing in the shown language reads the wall's FIRST language (and the other way round), and the developer's Preview
    // Language moves the open card live. The card's state is an in-memory store: the run never touches the developer's saved pick.
    public class PoiCardLanguageSceneTests : SearchSceneFixture
    {
        private PoiCardSheetView Sheet => Card.Sheet;

        private void UseMemoryState() => Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);

        private IEnumerator OpenFull(string poiId)
        {
            SelectionEventBus.Select(poiId);
            yield return CardTestInput.Settle();
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
        }

        private IEnumerator Push(Action<WallConfigData> edit)
        {
            var copy = ConfigCopy();
            edit(copy);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        private IEnumerator TapTheChip()
        {
            var chip = Sheet.LanguageButton;
            Assert.AreEqual(DisplayStyle.Flex, chip.resolvedStyle.display, "precondition: the chip shows");
            Assert.IsTrue(Sheet.Layer.worldBound.Contains(chip.worldBound.center), "precondition: the chip is on screen: " + chip.worldBound);
            // - what a finger there touches is the chip itself, not something over it (seen once, 15.4.2: a tap that never reached the chip)
            var touched = chip.panel.Pick(chip.worldBound.center);
            Assert.IsTrue(touched == chip || chip.Contains(touched), "precondition: a tap at the chip's centre lands on the chip, not on '"
                + touched?.name + "' (" + touched?.GetType().Name + ")");
            string before = chip.text;
            yield return CardTestInput.Tap(chip.panel, chip.worldBound.center);
            yield return CardTestInput.Settle();
            Assert.AreNotEqual(before, Sheet.LanguageButton.text, "the tap reached the chip: it now offers the other language");
        }

        private IEnumerator ScrollToSources()
        {
            var late = Sheet.Stack.BoundViews.OfType<SourcesBlockView>().First();
            Sheet.Stack.Scroll.ScrollTo(Sheet.Stack.SlotOf(late));
            yield return CardTestInput.Settle(0.2f);
        }

        private string FactsHeading() => Sheet.Stack.HeadingOf(Sheet.Stack.BoundViews.OfType<QuickFactsBlockView>().First()).text;

        private BlockInstanceData FactsBlock() => Session.SearchPois.Single(p => p.id == "lamp").card.blocks.First(b => b.kind == "quick_facts");

        private static void SetHeading(BlockInstanceData block, params (string Lang, string Value)[] texts)
        {
            var field = block.fields.Find(f => f.key == BlockKindDefinition.HeadingField);
            if (field == null) block.fields.Insert(0, field = new BlockFieldValue { key = BlockKindDefinition.HeadingField });
            field.text = texts.Select(t => new LocalizedEntry { lang = t.Lang, value = t.Value }).ToList();
        }

        private static BlockInstanceData Facts(WallConfigData c) =>
            c.pois.Single(p => p.id == "lamp").card.blocks.First(b => b.kind == "quick_facts");

        [UnityTest]
        public IEnumerator ARealTap_ShowsThePortugueseCardInPlace_AnotherTapBringsEnglishBack_TheScrollKept()
        {
            UseMemoryState();
            yield return OpenFull("lamp");
            var block = FactsBlock();
            string english = new BlockFieldReader(block, "en", "en").Text(BlockKindDefinition.HeadingField);
            string portuguese = new BlockFieldReader(block, "pt", "en").Text(BlockKindDefinition.HeadingField);
            Assert.AreNotEqual(english, portuguese, "precondition: The Lamp's Quick Facts has both an English and a Portuguese heading");
            Assert.AreEqual(english, FactsHeading(), "precondition: the card opens in the wall's first language");
            Assert.AreEqual("PT", Sheet.LanguageButton.text, "the chip offers Portuguese");

            yield return ScrollToSources();
            float scrolled = Sheet.Stack.Scroll.scrollOffset.y;
            Assert.Greater(scrolled, 300f, "precondition: the card is scrolled well down: " + scrolled);
            yield return CardGalleryChecks.Render("Card_Lamp_language_EN_scrolled");

            yield return TapTheChip();
            Assert.AreEqual("pt", Card.State.Language(), "the tap was remembered for this wall");
            Assert.AreEqual(portuguese, FactsHeading(), "the open card now speaks Portuguese");
            Assert.AreEqual("Fechar", Sheet.CloseButton.tooltip, "and so do the card's own words");
            Assert.AreEqual("EN", Sheet.LanguageButton.text, "the chip now offers English");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "the sheet kept its stop");
            Assert.AreEqual(scrolled, Sheet.Stack.Scroll.scrollOffset.y, 2f, "and the reader kept their place");
            Assert.AreEqual("lamp", Card.ShownPoiId);
            yield return CardGalleryChecks.Render("Card_Lamp_language_PT_scrolled");

            yield return TapTheChip();
            Assert.AreEqual("en", Card.State.Language());
            Assert.AreEqual(english, FactsHeading(), "a second tap: English again");
            Assert.AreEqual("Close", Sheet.CloseButton.tooltip);
            Assert.AreEqual(scrolled, Sheet.Stack.Scroll.scrollOffset.y, 2f, "the place is still kept");
        }

        [UnityTest]
        public IEnumerator ThePick_StaysForAnotherPoint_AndForANewCard()
        {
            UseMemoryState();
            yield return OpenFull("lamp");
            yield return TapTheChip();
            Assert.AreEqual("Fechar", Sheet.CloseButton.tooltip, "precondition: Portuguese");

            var other = Session.SearchPois.First(p => p.id != "lamp" && p.card?.blocks != null && p.card.blocks.Count > 0);
            SelectionEventBus.Select(other.id);
            yield return CardTestInput.Settle();
            Assert.AreEqual(other.id, Card.ShownPoiId, "another point's card is open");
            Assert.AreEqual("Fechar", Sheet.CloseButton.tooltip, "it speaks Portuguese too: the pick belongs to the wall, not to a point");
            Assert.AreEqual("EN", Sheet.LanguageButton.text);

            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            SelectionEventBus.Select("lamp");
            yield return CardTestInput.Settle();
            Assert.AreEqual("Fechar", Sheet.CloseButton.tooltip, "closing the card and opening it again keeps it");
        }

        [UnityTest]
        public IEnumerator AnEnglishOnlyHeading_OnThePortugueseCard_ReadsEnglish_AndAPortugueseOnlyOne_OnTheEnglishCard_ReadsPortuguese()
        {
            UseMemoryState();
            yield return OpenFull("lamp");
            yield return TapTheChip();
            Assert.AreEqual("Fechar", Sheet.CloseButton.tooltip, "precondition: Portuguese");

            yield return Push(c => SetHeading(Facts(c), ("en", "English wall words")));
            Assert.AreEqual("English wall words", FactsHeading(), "no Portuguese heading: the wall's first language, not an empty block");

            yield return TapTheChip();
            Assert.AreEqual("Close", Sheet.CloseButton.tooltip, "precondition: English");
            yield return Push(c => SetHeading(Facts(c), ("pt", "Palavras so em portugues")));
            Assert.AreEqual("Palavras so em portugues", FactsHeading(), "no English heading either: the language the field has");

            yield return Push(c => SetHeading(Facts(c)));
            Assert.AreEqual("", FactsHeading(), "a heading with no text in any language shows nothing, as before");
        }

        [UnityTest]
        public IEnumerator PreviewLanguage_MovesTheOpenCardLive_AndAVisitorsOwnPickWins()
        {
            UseMemoryState();
            yield return OpenFull("lamp");
            Assert.AreEqual("Close", Sheet.CloseButton.tooltip, "precondition: English");

            yield return Push(c => c.card_settings.preview_language = "pt");
            Assert.AreEqual("Fechar", Sheet.CloseButton.tooltip, "the developer's preview opens the card in Portuguese, live");
            Assert.AreEqual("EN", Sheet.LanguageButton.text, "the chip offers the way back");

            yield return TapTheChip();
            Assert.AreEqual("Close", Sheet.CloseButton.tooltip, "a visitor's tap beats the preview: English");
            Assert.AreEqual("en", Card.State.Language());

            Card.State.SetLanguage("");
            yield return Push(c => c.card_settings.preview_language = "");
            Assert.AreEqual("Close", Sheet.CloseButton.tooltip, "the default is back: the wall's first language");
        }

        [UnityTest]
        public IEnumerator AWallWithOneLanguage_HasNoChip()
        {
            UseMemoryState();
            yield return OpenFull("lamp");
            Assert.AreEqual(DisplayStyle.Flex, Sheet.LanguageButton.resolvedStyle.display, "precondition: two languages, a chip");

            yield return Push(c => c.card_settings.languages = new List<string> { "en" });
            Assert.AreEqual(DisplayStyle.None, Sheet.LanguageButton.resolvedStyle.display, "one language: nothing to choose, no chip");
            Assert.IsFalse(Sheet.Root.ClassListContains("poi-card--has-language"));

            yield return Push(c => c.card_settings.languages = new List<string> { "en", "pt" });
            Assert.AreEqual(DisplayStyle.Flex, Sheet.LanguageButton.resolvedStyle.display, "two again: the chip is back, live");
        }

        [UnityTest]
        public IEnumerator TheWallsFirstLanguage_IsTheFallback_NotTheShownOne()
        {
            UseMemoryState();
            // - Portuguese first: it is the fallback now, and the chip offers English
            Session.CardSettings.languages = new List<string> { "pt", "en" };
            yield return OpenFull("lamp");
            Assert.AreEqual("Fechar", Sheet.CloseButton.tooltip, "precondition: the first language is Portuguese");
            yield return Push(c => SetHeading(Facts(c), ("en", "English only"), ("es", "Solo espanol")));
            Assert.AreEqual("English only", FactsHeading(), "no pt text and the fallback (pt) has none: the first language the field has");
            yield return Push(c => SetHeading(Facts(c), ("es", "Solo espanol"), ("en", "English only")));
            Assert.AreEqual("Solo espanol", FactsHeading(), "with neither pt nor a fallback text, the first language the field has");
        }
    }
}
