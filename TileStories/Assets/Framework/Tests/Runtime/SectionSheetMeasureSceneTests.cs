using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // SectionSheetMeasureSceneTests's shared ground: its set-up and helpers, so its own tests and its old-navigation ones stand on one fixture
    public abstract class SectionSheetMeasureSceneFixture : SearchSceneFixture
    {
        protected static readonly string[] Pois = { "lamp", "painting" };

        protected const float SpyStep = 120f;

        protected PoiCardSheetView Sheet => Card.Sheet;

        protected ScrollView Scroll => Sheet.Stack.Scroll;

        protected float Range => NavScrollRule.Range(Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height);

        protected IEnumerator Open(string poi, SheetStopRule.Stop stop)
        {
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            SelectionEventBus.Select(poi);
            yield return CardTestInput.Settle();
            Sheet.SetStop(stop);
            yield return CardTestInput.Settle();
            Assert.IsTrue(Sheet.IsOpen, "precondition: " + poi + "'s card is open");
            LookMeasure.FreezeHero(Sheet);
        }

        protected static int SectionOf(CardNavModel model, string target)
        {
            for (int i = 0; i < model.Sections.Count; i++)
                if (model.Sections[i].Targets.Contains(target)) return i;
            return -1;
        }
    }

    // The measuring instrument of _3.3 P4.4c1 (the section sheets), run only while TestEvidence/Sections/MEASURING names a phase
    // (LookMeasure): on The Lamp and The Painting at Full, every number the card's navigation reads from a block's position -- the stops,
    // every target's jump landing, the spy's section and block every 120 px of scroll, the rails' fit (Auto Nav Block = Rails) and the
    // bento tiles' rects -- so a sheet's every move is stated. Renders: Half and Full, at the top and past the hero.
    public class SectionSheetMeasureSceneTests : SectionSheetMeasureSceneFixture
    {
        [UnityTest]
        public IEnumerator TheNavigationsNumbers_OnTheScrollLayout([ValueSource(nameof(Pois))] string poi)
        {
            string phase = LookMeasure.RequirePhase(LookMeasure.Sections);
            yield return Open(poi, SheetStopRule.Stop.Full);
            var stops = Sheet.Stops;
            LookMeasure.Note(LookMeasure.Sections, phase, $"{poi}: stops peek {stops.Peek:0.0} half {stops.Half:0.0} full {stops.Full:0.0}; " +
                $"content {Scroll.contentContainer.layout.height:0.0} viewport {Scroll.contentViewport.layout.height:0.0} range {Range:0.0}");
            var model = Sheet.Nav.Model;
            Assert.Greater(model.Targets.Count, 3, "precondition: " + poi + " has targets to jump to");
            foreach (string target in model.Targets)
            {
                Scroll.scrollOffset = Vector2.zero;
                yield return null;
                Sheet.Nav.JumpToBlock(target, "measure");
                for (int frame = 0; frame < 240 && Sheet.Nav.IsJumping; frame++) yield return null;
                LookMeasure.Note(LookMeasure.Sections, phase, $"{poi} landing {target} ({Sheet.Nav.Model.Sections[SectionOf(model, target)].Key}): {Scroll.scrollOffset.y:0.0}");
            }
            for (float offset = 0f; offset <= Range + 0.5f; offset += SpyStep)
            {
                Scroll.scrollOffset = new Vector2(0f, offset);
                yield return null;
                yield return null;
                string section = Sheet.Nav.Current >= 0 ? model.Sections[Sheet.Nav.Current].Key : "-";
                LookMeasure.Note(LookMeasure.Sections, phase, $"{poi} spy at {offset:0}: {section} / {Sheet.Nav.CurrentBlock ?? "-"}");
            }
            Scroll.scrollOffset = Vector2.zero;
            yield return CardTestInput.Settle();
            yield return LookMeasure.Render(LookMeasure.Sections, phase, $"sections_{poi}_full_top", Sheet.Root);
            Scroll.scrollOffset = new Vector2(0f, Sheet.Stack.HeaderView.HeroPart.layout.height + 200f);
            yield return CardTestInput.Settle();
            yield return LookMeasure.Render(LookMeasure.Sections, phase, $"sections_{poi}_full_past_hero", Sheet.Root);
            Sheet.SetStop(SheetStopRule.Stop.Half);
            yield return CardTestInput.Settle();
            yield return LookMeasure.Render(LookMeasure.Sections, phase, $"sections_{poi}_half", Sheet.Root);
        }
    }

    // SectionSheetMeasureSceneTests's tests whose subject is an old navigation element: they wait for its rebuild on the Navigation Bar's pattern
    [Ignore(TestSkips.OldNavigationElements)]
    public class SectionSheetMeasureSceneOldNavigationTests : SectionSheetMeasureSceneFixture
    {
        [UnityTest]
        public IEnumerator TheRailsFit([ValueSource(nameof(Pois))] string poi)
        {
            string phase = LookMeasure.RequirePhase(LookMeasure.Sections);
            var copy = ConfigCopy();
            copy.card_settings.navigation.auto_nav_block = BuiltInBlocks.NavRailsKind;
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
            yield return Open(poi, SheetStopRule.Stop.Full);
            var view = Sheet.Stack.BoundViews.OfType<NavRailsBlockView>().SingleOrDefault();
            Assert.IsNotNull(view, "precondition: Auto Nav Block = Rails puts the rails on " + poi);
            foreach (var rail in view.Rails)
            {
                var cards = rail.Root.Query<Button>().ToList().Where(b => b.resolvedStyle.display == DisplayStyle.Flex).ToList();
                var strip = rail.Strip.Viewport.worldBound;
                int whole = cards.Count(c => c.worldBound.xMin >= strip.xMin - 0.5f && c.worldBound.xMax <= strip.xMax + 0.5f);
                LookMeasure.Note(LookMeasure.Sections, phase, $"{poi} rail {rail.SectionKey}: strip x {strip.xMin:0.0} w {strip.width:0.0}; " +
                    $"card w {(cards.Count > 0 ? cards[0].worldBound.width : 0f):0.0}; cards {cards.Count}, whole in view {whole}");
            }
        }

        [UnityTest]
        public IEnumerator TheBentoTiles([ValueSource(nameof(Pois))] string poi)
        {
            string phase = LookMeasure.RequirePhase(LookMeasure.Sections);
            var copy = ConfigCopy();
            copy.card_settings.navigation.layout = CardOptions.LayoutBento;
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
            yield return Open(poi, SheetStopRule.Stop.Full);
            Assert.IsTrue(Sheet.Bento.IsOn, "precondition: the bento layout is on");
            foreach (var tile in Sheet.Bento.Tiles)
            {
                var box = tile.Button.layout;
                LookMeasure.Note(LookMeasure.Sections, phase, $"{poi} bento tile {tile.Index}: {box.x:0.0}, {box.y:0.0}, {box.width:0.0} x {box.height:0.0}");
            }
        }
    }
}
