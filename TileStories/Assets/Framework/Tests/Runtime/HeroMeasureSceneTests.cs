using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // The measuring instrument of _3.3 P4.4a (the header and the hero), run only while TestEvidence/Hero/MEASURING names a phase
    // (LookMeasure): The Lamp and The Painting, English and Portuguese, with no Menu and with the Drawer -- the sheet's three stops, the
    // pinned header's height, the title's height and the hero's frame, noted per case; Peek, Half and Full rendered, cropped to the sheet.
    public class HeroMeasureSceneTests : SearchSceneFixture
    {
        private static readonly string[] Pois = { "lamp", "painting" };
        private static readonly string[] Languages = { "en", "pt" };
        private static readonly string[] Menus = { CardOptions.NavNone, BuiltInNavElements.DrawerTree };

        [UnityTest]
        public IEnumerator TheCard_AtPeekHalfAndFull([ValueSource(nameof(Pois))] string poi, [ValueSource(nameof(Languages))] string language,
            [ValueSource(nameof(Menus))] string menu)
        {
            string phase = LookMeasure.RequirePhase(LookMeasure.Hero);
            // - the card opens in the first language of the wall's list; the Menu is read on every selection
            Session.CardSettings.languages = new List<string> { language, language == "en" ? "pt" : "en" };
            Session.CardSettings.navigation.menu = menu;
            SelectionEventBus.Select(poi);
            yield return CardTestInput.Settle();
            var sheet = Card.Sheet;
            Assert.IsTrue(sheet.IsOpen, "precondition: " + poi + "'s card is open");
            // - the test owns time: the hero's pan holds still at its start for every render (the pan itself: PoiCardHeroSceneTests)
            var hero = sheet.Stack.HeaderView;
            var realClock = hero.Clock;
            hero.Clock = () => 0f;
            // - and the card's moving light (glass, rim) and the live wall behind the glass hold still too, or no render ever settles
            CardTestInput.HoldTheWallStill(sheet);
            if (menu == CardOptions.NavNone) Assert.IsNull(sheet.Slots.MenuView, "precondition: no Menu");
            else Assert.AreEqual(menu, sheet.Slots.MenuKey, "precondition: the card's Menu");

            sheet.SetStop(SheetStopRule.Stop.Half);
            yield return CardTestInput.Settle();
            var title = sheet.Root.Q(className: "card-header__title");
            Assert.IsNotNull(title, "precondition: the card has its title");
            var heroPart = hero.HeroPart;
            var stops = sheet.Stops;
            string tag = menu == CardOptions.NavNone ? "nomenu" : "menu";
            LookMeasure.Note(LookMeasure.Hero, phase, $"{poi} {language} {tag}: stops peek {stops.Peek:0.0} half {stops.Half:0.0} full {stops.Full:0.0}; " +
                $"header (at Half) {sheet.Stack.HeaderSlot.layout.height:0.0}; title {title.layout.height:0.0}; hero " +
                (heroPart.panel != null ? $"{heroPart.layout.width:0.0} x {heroPart.layout.height:0.0}" : "none") +
                $"; viewport at Half {sheet.Stack.Scroll.contentViewport.layout.height:0.0}; footer shown {sheet.Chrome.BottomShown}, strip shown {sheet.Chrome.TopShown}");

            foreach (var stop in new[] { SheetStopRule.Stop.Peek, SheetStopRule.Stop.Half, SheetStopRule.Stop.Full })
            {
                sheet.SetStop(stop);
                yield return CardTestInput.Settle();
                yield return LookMeasure.Render(LookMeasure.Hero, phase, $"card_{poi}_{language}_{tag}_{stop.ToString().ToLowerInvariant()}", sheet.Root);
            }
            hero.Clock = realClock;
        }
    }
}
