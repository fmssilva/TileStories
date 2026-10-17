using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of step 9 (_3.2 sections 4.2 and 5.4: the floating slot, fab_dial, fab_radial, the placed-model chip) on the REAL LivingRoomScene
    // with The Lamp's real card (8 families, Grouped By Family) and real input -- fingers through CardTestInput's panel events, the real AR
    // placement owner over the wall's own WallArSurface (MockLocalizationProvider), each wall option pushed through the live seam:
    //   - the stops do not move with a Floating Button, and the slot rests just above the sheet at peek and half, flush in the top gap at Full
    //   - fab_radial: a real press-slide-release picks the section its direction points at, and lands it; a slide away from the arc or one
    //     that never leaves the button picks nothing; a real TAP opens the arc, a second tap on an item picks it, More pages through the rest
    //   - fab_dial: a real tap opens its column up over the camera at peek, a pill lands its section; a tap on the dim closes it and the card
    //     stays; an open drawer covers the button
    //   - the placed-model chip (the two AR TODOs): with a model standing it is inside the VISIBLE rect AT PEEK -- whole on screen, above the
    //     sheet, the topmost thing at its own centre -- and at half and full, in English and Portuguese; a real Remove takes the model away and
    //     the chip goes; on another point's card it names the point, offers no Remove, and a real tap on it opens the card that placed it;
    //     Placed Model Control = None hides it, live; it reads over a bright and a dark camera frame
    public class PoiCardFloatingSceneTests : SearchSceneFixture
    {
        private const string Lamp = "lamp";
        private const string LampMilitary = "lamp_military";

        private PoiCardSheetView Sheet => Card.Sheet;
        private NavController Nav => Sheet.Nav;
        private CardNavModel Model => Nav.Model;
        private ScrollView Scroll => Sheet.Stack.Scroll;
        private FloatingSlot Floating => Sheet.Floating;
        private PlacedModelChipView Chip => Floating.Chip;
        private ArPlacementService Placement => Card.ArPlacementService;

        private sealed class RecordingCardEvents : ICardEvents
        {
            public readonly List<CardEvent> Raised = new();
            public void Raise(CardEvent cardEvent) => Raised.Add(cardEvent);
        }

        private RecordingCardEvents _events;
        private List<string> _shippedLanguages;

        [UnitySetUp]
        public IEnumerator UseAnInMemoryCardStateAndRecordTheEvents()
        {
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            _events = new RecordingCardEvents();
            Card.Events = _events;
            yield break;
        }

        // ---------------- helpers ----------------

        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;

        private float Range => NavScrollRule.Range(Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height);

        private List<CardEvent> NavJumps => _events.Raised.Where(e => e.Kind == CardEventKinds.NavJump).ToList();

        private CardStrings Words(string language) =>
            new(Card.StringTable.Entries(), CardStringSources.Shared.Entries(), null, language, "en");

        // The wall's navigation for a test: Grouped By Family (every family one section), no Top Bar, the given Floating Button and Menu, the
        // card in `language`; pushed through the live seam
        private IEnumerator UseNavigation(string floating, string menu = CardOptions.NavNone, string language = "en", string placedModelControl = null)
        {
            var copy = ConfigCopy();
            var nav = copy.card_settings.navigation;
            nav.block_order = CardOptions.BlockOrderByFamily;
            nav.top = CardOptions.NavNone;
            nav.floating = floating;
            nav.menu = menu;
            nav.max_slots = 5;
            if (placedModelControl != null) nav.placed_model_control = placedModelControl;
            // - English keeps the shipped languages (the language chip is part of the header the stops are measured with); another language
            //   leads, English after it. Read from the shipped wall each time, so an earlier push never leaks into the next
            _shippedLanguages ??= new List<string>(ConfigCopy().card_settings.languages);
            copy.card_settings.languages = language == "en" ? new List<string>(_shippedLanguages) : new List<string> { language, "en" };
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        private IEnumerator Open(SheetStopRule.Stop stop, string poiId = Lamp)
        {
            if (SelectionEventBus.CurrentPoiId != poiId)
            {
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
                SelectionEventBus.Select(poiId);
                yield return CardTestInput.Settle();
            }
            Scroll.scrollOffset = Vector2.zero;
            Sheet.SetStop(stop);
            yield return CardTestInput.Settle();
            Assert.AreEqual(stop, Sheet.Stop, "precondition: the card rests at " + stop);
            yield return Rested();
        }

        // The sheet's height animates in real time: every measure waits for it to rest at its stop's height
        private IEnumerator Rested()
        {
            float rest = Sheet.Stops.HeightOf(Sheet.Stop);
            for (int frame = 0; frame < 600 && Mathf.Abs(Sheet.Root.layout.height - rest) > OnePixel; frame++) yield return null;
            Assert.AreEqual(rest, Sheet.Root.layout.height, OnePixel, "precondition: the sheet rests at its " + Sheet.Stop + " height");
            yield return null;
        }

        private IEnumerator Landed()
        {
            for (int frame = 0; frame < 600 && Nav.IsJumping; frame++) yield return null;
            Assert.IsFalse(Nav.IsJumping, "the jump came to rest");
            yield return CardTestInput.Settle(0.3f);
        }

        private void AssertLanded(int section, string name)
        {
            string block = Model.Sections[section].Targets[0];
            var slot = Sheet.Stack.SlotOfBlock(block);
            if (slot.layout.y <= Range - OnePixel)
                Assert.AreEqual(0f, slot.worldBound.yMin - Scroll.contentViewport.worldBound.yMin, OnePixel, name + ": its first block's top meets the viewport's top");
            else
                Assert.AreEqual(Range, Scroll.scrollOffset.y, OnePixel, name + ": a late block clamps at the end of the range");
        }

        private static IEnumerator TouchTap(IPanel panel, Vector2 at) => CardTestInput.TouchDrag(panel, at, Vector2.zero, frames: 1);

        // A real tap on an element the visitor sees (inside the card layer)
        private IEnumerator Tap(VisualElement target, string name)
        {
            Assert.IsTrue(Sheet.Layer.worldBound.Contains(target.worldBound.center), "precondition: " + name + " is on screen " + target.worldBound);
            yield return TouchTap(Sheet.Layer.panel, target.worldBound.center);
            yield return CardTestInput.Settle();
        }

        // Whole inside the card layer (the screen less its safe area), with a pixel's grace
        private void AssertWhollyOnScreen(Rect box, string name)
        {
            var layer = Sheet.Layer.worldBound;
            Assert.GreaterOrEqual(box.xMin, layer.xMin - OnePixel, name + ": its left edge on screen " + box + " in " + layer);
            Assert.LessOrEqual(box.xMax, layer.xMax + OnePixel, name + ": its right edge on screen");
            Assert.GreaterOrEqual(box.yMin, layer.yMin - OnePixel, name + ": its top edge on screen");
            Assert.LessOrEqual(box.yMax, layer.yMax + OnePixel, name + ": its bottom edge on screen");
        }

        // What the visitor's finger meets at `point`: the topmost pickable element of the card's panel
        private VisualElement TopmostAt(Vector2 point) => Sheet.Layer.panel.Pick(point);

        private static bool IsInside(VisualElement element, VisualElement container)
        {
            for (var e = element; e != null; e = e.parent)
                if (e == container) return true;
            return false;
        }

        private FabRadialNavView Radial => (FabRadialNavView)Sheet.Slots.FloatingView;
        private FabDialNavView Dial => (FabDialNavView)Sheet.Slots.FloatingView;

        private PlaceInArBlockView PlaceView() => Sheet.Stack.BoundViews.OfType<PlaceInArBlockView>().First(v => v.BlockKey == "block_62");

        private int PlacedInTheWorld() => Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Count(t => t.name.StartsWith("ArPlaced_"));

        // The Lamp's own place_in_ar block (block_62), placed by a real tap on its button: the card drops to its peek
        private IEnumerator PlaceTheArchForReal()
        {
            yield return Open(SheetStopRule.Stop.Full);
            var view = PlaceView();
            Scroll.ScrollTo(view.Button);
            yield return CardTestInput.Settle(0.2f);
            yield return CardTestInput.Tap(view.Button.panel, view.Button.worldBound.center);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsNotNull(Placement.Placed, "precondition: a real tap placed The Lamp's arch");
            Assert.AreEqual(SheetStopRule.Stop.Peek, Sheet.Stop, "precondition: the card dropped to its peek");
            yield return Rested();
        }

        // ---------------- the slot ----------------

        // Section 4.2: the floating slot is a layer of its own -- the stops never move with a Floating Button set (5a's standard: peek 195.00 /
        // half 337.60 / full 796.00 at 390 x 844) -- and the slot rests a gap above the sheet's top edge at peek and half, flush in the top gap
        // at Full, the button whole on screen; after a real drag of the sheet it rests above the sheet's new edge
        [UnityTest]
        public IEnumerator TheStops_AreUnchangedWithEachFloatingButton_AndTheSlotRestsAboveTheSheetAtEveryStop_AndFollowsARealDrag()
        {
            float gap = float.NaN;
            foreach (string floating in new[] { CardOptions.NavNone, BuiltInNavElements.FabDial, BuiltInNavElements.FabRadial })
            {
                yield return UseNavigation(floating);
                foreach (var (stop, height) in new[] { (SheetStopRule.Stop.Peek, 195f), (SheetStopRule.Stop.Half, 337.6f), (SheetStopRule.Stop.Full, 796f) })
                {
                    yield return Open(stop);
                    Assert.AreEqual(height, Sheet.Stops.HeightOf(stop), 0.01f, floating + ": the " + stop + " stop did not move");
                    if (floating == CardOptions.NavNone)
                    {
                        Assert.IsFalse(Floating.IsShown, "None and nothing standing: no slot");
                        continue;
                    }
                    Assert.IsTrue(Floating.IsShown, floating + " at " + stop + ": the slot shows");
                    var slot = Floating.Root.worldBound;
                    var sheet = Sheet.Root.worldBound;
                    if (float.IsNaN(gap)) gap = CardTestInput.TokenPx("--ts-space-2");
                    if (stop == SheetStopRule.Stop.Full)
                    {
                        Assert.AreEqual(sheet.yMin, slot.yMax, OnePixel, floating + " at Full: flush on the sheet's top edge");
                        Assert.AreEqual(Sheet.Layer.worldBound.yMin, slot.yMin, OnePixel, floating + " at Full: in the top gap, not off the screen");
                    }
                    else Assert.AreEqual(sheet.yMin - gap, slot.yMax, OnePixel, floating + " at " + stop + ": a gap above the sheet's top edge");
                    var fab = floating == BuiltInNavElements.FabDial ? Dial.Fab : Radial.Fab;
                    AssertWhollyOnScreen(fab.Button.worldBound, floating + " at " + stop);
                    Assert.IsTrue(IsInside(TopmostAt(fab.Button.worldBound.center), fab.Button), floating + " at " + stop + ": the button is what a finger meets");
                }
            }

            // - a real drag of the sheet's grab handle from Half down to Peek: the slot rests above the new edge
            yield return Open(SheetStopRule.Stop.Half);
            yield return CardTestInput.Drag(Sheet.Handle, Sheet.Stops.Half - Sheet.Stops.Peek, slowSheet: Sheet);
            yield return CardTestInput.Settle();
            yield return Rested();
            Assert.AreEqual(SheetStopRule.Stop.Peek, Sheet.Stop, "precondition: a real drag lowered the card to its peek");
            Assert.AreEqual(Sheet.Root.worldBound.yMin - gap, Floating.Root.worldBound.yMax, OnePixel, "the slot followed the sheet down");
            yield return Capture("Floating_Lamp_radial_after_drag_peek");
        }

        // ---------------- fab_radial ----------------

        // The arc's own geometry and its first page as the card shows it at `stop`: opened by a real tap, read, closed by a real tap on its X
        private IEnumerator ReadTheArc(System.Action<RadialMenuRule.Arc, List<int>> read)
        {
            yield return Tap(Radial.Fab.Button, "the radial's button");
            Assert.IsTrue(Radial.IsOpen, "precondition: a real tap opened the arc");
            var arc = Radial.Layout;
            var sections = Radial.Items.Select(i => i.Section).ToList();
            yield return Tap(Radial.Fab.Button, "the radial's X");
            Assert.IsFalse(Radial.IsOpen, "precondition: a real tap on its X closed it");
            read(arc, sections);
        }

        // Section 5.4: press-slide-release -- a real finger pressed on the button, slid towards an item and lifted picks the section the
        // direction points at (RadialMenuRule.ItemAt), and the card lands it; a slide the other way, or one that never leaves the button,
        // picks nothing and leaves the card where it was. At peek (Bar Slots items on the half circle) and at half
        [UnityTest]
        public IEnumerator TheRadial_ARealPressSlideRelease_PicksTheSectionItsDirectionPointsAt_AndASlideAwayPicksNothing()
        {
            yield return UseNavigation(BuiltInNavElements.FabRadial);
            foreach (var stop in new[] { SheetStopRule.Stop.Peek, SheetStopRule.Stop.Half })
            {
                yield return Open(stop);
                RadialMenuRule.Arc arc = default;
                List<int> sections = null;
                yield return ReadTheArc((a, s) => { arc = a; sections = s; });
                Assert.AreEqual(5, arc.Count, stop + ": Bar Slots items fit beside the button");
                Assert.AreEqual(-1, sections[4], stop + ": the last is More");
                foreach (int item in new[] { 0, 2, 3 })
                {
                    yield return Open(stop);
                    int jumps = NavJumps.Count;
                    var centre = Radial.Fab.Button.worldBound.center;
                    // - towards the item's own centre on the arc: the finger ends on the item
                    yield return CardTestInput.TouchDrag(Sheet.Layer.panel, centre, arc.Offset(item), frames: 12);
                    yield return Landed();
                    string name = stop + ", item " + item;
                    Assert.AreEqual(jumps + 1, NavJumps.Count, name + ": one jump");
                    var jump = NavJumps.Last();
                    Assert.AreEqual(BuiltInNavElements.FabRadial, jump.Variant, name + ": by the radial");
                    Assert.AreEqual(Model.Sections[sections[item]].Key, jump.Value, name + ": to the section the slide pointed at");
                    Assert.IsFalse(Radial.IsOpen, name + ": the arc closed with the pick");
                    AssertLanded(sections[item], name);
                }

                // - away from the arc (right, towards the screen's edge) and inside the dead zone: nothing picked, the card stays
                foreach (var (travel, what) in new[] { (new Vector2(30f, -60f), "a slide up-right, away from the arc"), (arc.Offset(2).normalized * 16f, "a slide that never left the button") })
                {
                    yield return Open(stop);
                    int jumps = NavJumps.Count;
                    yield return CardTestInput.TouchDrag(Sheet.Layer.panel, Radial.Fab.Button.worldBound.center, travel, frames: 8);
                    yield return CardTestInput.Settle();
                    Assert.AreEqual(jumps, NavJumps.Count, stop + ", " + what + ": no jump");
                    Assert.IsFalse(Radial.IsOpen, stop + ", " + what + ": the arc closed");
                    Assert.AreEqual(stop, Sheet.Stop, stop + ", " + what + ": the card stayed where it was");
                    Assert.AreEqual(0f, Scroll.scrollOffset.y, OnePixel, stop + ", " + what + ": unscrolled");
                }
            }
        }

        // WCAG 2.2 SC 2.5.7: the same element with taps only -- a real tap opens the arc (every item a whole target on screen, the card dimmed
        // behind), a second real tap on an item lands its section; More turns the arc to the next sections, a real tap on one lands it
        [UnityTest]
        public IEnumerator TheRadial_ARealTapOpensTheArc_ASecondTapPicks_AndMorePagesThroughTheRest()
        {
            yield return UseNavigation(BuiltInNavElements.FabRadial);
            yield return Open(SheetStopRule.Stop.Half);
            yield return Tap(Radial.Fab.Button, "the radial's button");
            Assert.IsTrue(Radial.IsOpen, "a real tap opened the arc");
            Assert.IsTrue(Floating.IsDimmed, "the card and the camera view are dimmed behind it");
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            foreach (var item in Radial.Items)
            {
                AssertWhollyOnScreen(item.Button.worldBound, item.Label.text);
                Assert.GreaterOrEqual(item.Button.layout.width, touch - OnePixel, item.Label.text + ": a whole target");
                Assert.IsTrue(IsInside(TopmostAt(item.Button.worldBound.center), item.Button), item.Label.text + ": what a finger meets there");
            }
            yield return Capture("Floating_Lamp_radial_open_half_en");
            var second = Radial.Items[1];
            int section = second.Section;
            yield return Tap(second.Button, second.Label.text);
            yield return Landed();
            Assert.AreEqual(BuiltInNavElements.FabRadial, NavJumps.Last().Variant);
            Assert.AreEqual(Model.Sections[section].Key, NavJumps.Last().Value, "the tapped section");
            Assert.IsFalse(Floating.IsDimmed, "the dim went with the arc");
            AssertLanded(section, second.Label.text);

            yield return Open(SheetStopRule.Stop.Half);
            yield return Tap(Radial.Fab.Button, "the radial's button");
            var more = Radial.Items.Last();
            Assert.AreEqual(-1, more.Section, "precondition: the last item is More");
            int next = Radial.Items.Count - 1;
            yield return Tap(more.Button, "More");
            Assert.IsTrue(Radial.ShowsMore, "a real tap on More turns the arc to the next sections");
            Assert.AreEqual(next, Radial.Items[0].Section, "starting with the first section the arc did not show");
            yield return Capture("Floating_Lamp_radial_more_half_en");
            var later = Radial.Items[1];
            int laterSection = later.Section;
            yield return Tap(later.Button, later.Label.text);
            yield return Landed();
            Assert.AreEqual(Model.Sections[laterSection].Key, NavJumps.Last().Value, "a section from the next page lands");
            AssertLanded(laterSection, later.Label.text);
        }

        // ---------------- fab_dial ----------------

        // Section 5.4: a real tap opens the dial's column UP over the camera view at peek, every pill whole on screen; a real tap on a pill lands
        // its section; a real tap on the dim beside it closes it and the card stays; an open drawer (Menu) covers the button
        [UnityTest]
        public IEnumerator TheDial_ARealTapOpensItsColumnOverTheCamera_APillLands_TheDimClosesIt_AndAnOpenDrawerCoversTheButton()
        {
            yield return UseNavigation(BuiltInNavElements.FabDial);
            yield return Open(SheetStopRule.Stop.Peek);
            yield return Tap(Dial.Fab.Button, "the dial's button");
            Assert.IsTrue(Dial.IsOpen, "a real tap opened the column");
            Assert.AreEqual(FabFanRule.Direction.Up, Dial.Direction, "at peek it opens up, over the camera view");
            Assert.AreEqual(Model.Sections.Count, Dial.Items.Count, "a pill per section");
            foreach (var pill in Dial.Items)
            {
                AssertWhollyOnScreen(pill.Button.worldBound, pill.Label.text);
                Assert.LessOrEqual(pill.Button.worldBound.yMax, Dial.Fab.Root.worldBound.yMin + OnePixel, pill.Label.text + ": above the button");
            }
            yield return Capture("Floating_Lamp_dial_open_peek_en");
            var pick = Dial.Items[3];
            yield return Tap(pick.Button, pick.Label.text);
            yield return Landed();
            Assert.AreEqual(BuiltInNavElements.FabDial, NavJumps.Last().Variant, "by the dial");
            Assert.AreEqual(Model.Sections[3].Key, NavJumps.Last().Value);
            Assert.IsFalse(Dial.IsOpen, "the column closed with the pick");
            AssertLanded(3, pick.Label.text);

            // - a real tap on the dim, on the camera view beside the column: the column closes, the card stays at its peek
            yield return Open(SheetStopRule.Stop.Peek);
            yield return Tap(Dial.Fab.Button, "the dial's button");
            Assert.IsTrue(Floating.IsDimmed, "precondition: dimmed behind the column");
            var beside = new Vector2(Sheet.Layer.worldBound.xMin + 40f, Sheet.Root.worldBound.yMin - 60f);
            Assert.IsTrue(IsInside(TopmostAt(beside), Floating.Scrim), "precondition: the dim is what a finger meets beside the column");
            yield return TouchTap(Sheet.Layer.panel, beside);
            yield return CardTestInput.Settle();
            Assert.IsFalse(Dial.IsOpen, "a tap on the dim closed the column");
            Assert.IsTrue(Sheet.IsOpen, "and not the card");
            Assert.AreEqual(SheetStopRule.Stop.Peek, Sheet.Stop);

            // - with a Menu, an open drawer covers the floating button (the menu layer is over the floating slot)
            yield return UseNavigation(BuiltInNavElements.FabDial, menu: BuiltInNavElements.DrawerTree);
            yield return Open(SheetStopRule.Stop.Half);
            var fabCentre = Dial.Fab.Button.worldBound.center;
            yield return Tap(Sheet.MenuButton, "the menu button");
            Assert.IsTrue(Sheet.Overlay.IsModal, "precondition: the drawer is open");
            Assert.IsFalse(IsInside(TopmostAt(fabCentre), Dial.Fab.Button), "the open drawer covers the floating button");
            Assert.IsTrue(IsInside(TopmostAt(fabCentre), Sheet.Overlay.Root), "the menu layer is what a finger meets there");
            yield return Capture("Floating_Lamp_dial_under_drawer_half_en");
        }

        // ---------------- the placed-model chip ----------------

        // The [step 9] TODO: with a model standing, the chip is inside the VISIBLE rect at PEEK -- whole on screen, above the sheet (nothing of
        // the card hides it), the topmost thing at its own centre -- and at half and full, in English and Portuguese; a real tap on its Remove
        // takes the model away (nothing left in the world) and the chip goes
        [UnityTest]
        public IEnumerator TheChip_WithAModelStanding_IsInsideTheVisibleRectAtPeek_AndAtHalfAndFull_AndARealRemoveTakesItAway([Values("en", "pt")] string language)
        {
            yield return UseNavigation(CardOptions.NavNone, language: language);
            Assert.AreEqual(PlacedModelChipRule.State.Hidden, Chip.State, "precondition: nothing stands, no chip");
            yield return PlaceTheArchForReal();
            var words = Words(language);
            foreach (var stop in new[] { SheetStopRule.Stop.Peek, SheetStopRule.Stop.Half, SheetStopRule.Stop.Full })
            {
                if (stop != SheetStopRule.Stop.Peek) yield return Open(stop);
                Assert.AreEqual(PlacedModelChipRule.State.ThisCard, Chip.State, stop + ": the card that placed it");
                Assert.IsTrue(Floating.IsShown, stop + ": the slot shows the chip");
                var chip = Chip.Root.worldBound;
                AssertWhollyOnScreen(chip, "the chip at " + stop);
                // - the chip is a touch target plus its two light edges; at Full the slot rests at the screen's top, so the chip may reach over the
                //   sheet's top edge by those edges -- in FRONT of it (the slot is the sheet's later sibling): the card never covers it
                float edges = 2f * Chip.Root.resolvedStyle.borderTopWidth;
                float overlap = stop == SheetStopRule.Stop.Full ? edges : 0f;
                Assert.LessOrEqual(chip.yMax, Sheet.Root.worldBound.yMin + overlap + OnePixel, stop + ": above the sheet's top edge (at Full by at most its edges)");
                Assert.Greater(chip.height, 0f, stop + ": laid out");
                Assert.IsTrue(IsInside(TopmostAt(chip.center), Chip.Root), stop + ": the chip is what a finger meets at its centre (the topmost thing there)");
                Assert.AreEqual(words.Get(CardStrings.Keys.PlaceInArPlaced), Chip.Text.text, stop + ": " + language + " words");
                Assert.IsTrue(CardTestInput.IsShown(Chip.RemoveButton, Sheet.Layer), stop + ": Remove offered");
                Assert.AreEqual(words.Get(CardStrings.Keys.PlacedModelRemove), Chip.RemoveButton.Q<Label>().text);
                Assert.GreaterOrEqual(Chip.RemoveButton.layout.height, CardTestInput.TokenPx("--ts-touch-target") - OnePixel, stop + ": Remove a whole target");
                var size = Chip.Text.MeasureTextSize(Chip.Text.text, Chip.Text.layout.width, VisualElement.MeasureMode.AtMost, 0, VisualElement.MeasureMode.Undefined);
                Assert.LessOrEqual(size.y, Chip.Text.layout.height + 0.5f, stop + ": every line of its words shows");
                yield return Capture("Floating_Lamp_chip_" + stop.ToString().ToLowerInvariant() + "_" + language);
            }
            if (language == "pt") StringAssert.Contains("à parede", Chip.Text.text, "Portuguese keeps its accent");

            yield return Open(SheetStopRule.Stop.Peek);
            yield return Tap(Chip.RemoveButton, "Remove");
            Assert.IsNull(Placement.Placed, "a real tap on Remove took the model away");
            Assert.AreEqual(0, PlacedInTheWorld(), "nothing left in the world");
            Assert.AreEqual(PlacedModelChipRule.State.Hidden, Chip.State, "the chip went");
            Assert.IsFalse(Floating.IsShown, "and the slot with it (no Floating Button)");
            Assert.IsFalse(PlaceView().IsPlaced, "the block's button is back to placing");
        }

        // Keep Model On Switch: only the card that placed the model offers Remove. On another point's card the chip names The Lamp and offers
        // no Remove; a real tap on it opens The Lamp's card, where the chip offers Remove again
        [UnityTest]
        public IEnumerator TheChip_OnAnotherPointsCard_NamesThePoint_OffersNoRemove_AndARealTapOpensTheCardThatPlacedIt()
        {
            yield return UseNavigation(CardOptions.NavNone);
            var row = Session.CardSettings.kinds.Find(k => k.kind == BuiltInBlocks.PlaceInArKind);
            if (row == null) Session.CardSettings.kinds.Add(row = new BlockKindSetting { kind = BuiltInBlocks.PlaceInArKind });
            row.field_defaults.RemoveAll(d => d.key == BuiltInBlocks.PlaceInArKeepOnSwitchField);
            row.field_defaults.Add(new BlockFieldDefault { key = BuiltInBlocks.PlaceInArKeepOnSwitchField, value = BlockLibraryRule.FlagTrue });
            Card.Rebind();
            yield return CardTestInput.Settle();
            yield return PlaceTheArchForReal();

            yield return Open(SheetStopRule.Stop.Peek, LampMilitary);
            Assert.AreEqual(LampMilitary, Card.ShownPoiId, "precondition: another point's card");
            Assert.IsNotNull(Placement.Placed, "Keep Model On Switch kept the model");
            Assert.AreEqual(PlacedModelChipRule.State.OtherCard, Chip.State);
            Assert.AreEqual("The Lamp is placed by the wall", Chip.Text.text, "names the point by its name");
            Assert.IsFalse(CardTestInput.IsShown(Chip.RemoveButton, Sheet.Layer), "no Remove on another card");
            AssertWhollyOnScreen(Chip.Root.worldBound, "the chip on another card");
            Assert.LessOrEqual(Chip.Root.worldBound.yMax, Sheet.Root.worldBound.yMin + OnePixel, "above the sheet at peek");
            Assert.IsTrue(IsInside(TopmostAt(Chip.Words.worldBound.center), Chip.Words), "its words are a button a finger meets");
            yield return Capture("Floating_LampMilitary_chip_other_peek_en");

            yield return Tap(Chip.Words, "the chip's words");
            Assert.AreEqual(Lamp, Card.ShownPoiId, "a real tap opened the card that placed the model");
            Assert.AreEqual(Lamp, SelectionEventBus.CurrentPoiId, "through the selection bus, like a marker tap");
            Assert.IsNotNull(Placement.Placed, "the model still stands");
            Assert.AreEqual(PlacedModelChipRule.State.ThisCard, Chip.State, "now the chip offers Remove");
            yield return Rested();
            yield return Tap(Chip.RemoveButton, "Remove");
            Assert.IsNull(Placement.Placed, "and a real Remove takes it away");
            Assert.AreEqual(0, PlacedInTheWorld());
        }

        // Placed Model Control is live: None hides the chip while the model stands, Chip brings it back -- pushed through the Editor's seam
        [UnityTest]
        public IEnumerator PlacedModelControl_None_HidesTheChip_AndChipBringsItBack_Live()
        {
            yield return UseNavigation(CardOptions.NavNone);
            yield return PlaceTheArchForReal();
            Assert.AreEqual(PlacedModelChipRule.State.ThisCard, Chip.State, "precondition: the chip shows");
            yield return UseNavigation(CardOptions.NavNone, placedModelControl: CardOptions.PlacedModelNone);
            Assert.IsNotNull(Placement.Placed, "a live edit keeps the model (the same point)");
            Assert.AreEqual(PlacedModelChipRule.State.Hidden, Chip.State, "None: no chip");
            Assert.IsFalse(Floating.IsShown);
            Assert.IsTrue(PlaceView().IsPlaced, "the block still says it (Remove from room)");
            yield return UseNavigation(CardOptions.NavNone, placedModelControl: CardOptions.PlacedModelChip);
            Assert.AreEqual(PlacedModelChipRule.State.ThisCard, Chip.State, "Chip: back");
            Assert.IsTrue(Floating.IsShown);
        }

        // ---------------- over the camera image ----------------

        // The chip and the floating button float over the camera image, not over the card's own background: their dark surface reads at 3:1 or
        // more against a BRIGHT frame, their light edge at 3:1 or more against a DARK one (WCAG 1.4.11, non-text), their words at 4.5:1 on their
        // own surface whatever is behind -- measured on the colours they resolve to; rendered over a white, a black and the real room frame
        [UnityTest]
        public IEnumerator TheChipAndTheButton_ReadOverABrightAndADarkCameraFrame()
        {
            yield return UseNavigation(BuiltInNavElements.FabDial);
            yield return PlaceTheArchForReal();
            foreach (var (part, what) in new[] { (Chip.Root, "the chip"), (Dial.Fab.Button, "the floating button") })
            {
                var style = part.resolvedStyle;
                Color surface = style.backgroundColor, edge = style.borderTopColor;
                Assert.Greater(style.borderTopWidth, 1f, what + ": an edge thicker than a hairline");
                foreach (var (frame, name) in new[] { (Color.white, "white"), (Color.black, "black") })
                {
                    // - the edge is drawn over the frame with its own alpha, blended in linear light (the project's colour space)
                    Color onFrame = Color.Lerp(frame.linear, edge.linear, edge.a).gamma;
                    float surfaceContrast = CardTestInput.Contrast(surface, frame);
                    float edgeContrast = CardTestInput.Contrast(onFrame, frame);
                    Assert.GreaterOrEqual(Mathf.Max(surfaceContrast, edgeContrast), UIAccessibility.MinRatioLargeTextOrUIComponent,
                        what + " over a " + name + " frame: its surface (" + surfaceContrast.ToString("F2") + ") or its edge (" + edgeContrast.ToString("F2") + ") stands out");
                }
            }
            Assert.GreaterOrEqual(CardTestInput.Contrast(Chip.Text.resolvedStyle.color, CardTestInput.EffectiveBackground(Chip.Text)), UIAccessibility.MinRatioNormalText,
                "the chip's words on its own surface");

            var flags = Cam.clearFlags;
            var background = Cam.backgroundColor;
            int mask = Cam.cullingMask;
            try
            {
                foreach (var (frame, name) in new[] { (Color.white, "white"), (Color.black, "black") })
                {
                    Cam.clearFlags = CameraClearFlags.SolidColor;
                    Cam.backgroundColor = frame;
                    Cam.cullingMask = 0;
                    yield return null;
                    yield return Capture("Floating_Lamp_chip_and_dial_over_" + name + "_peek");
                }
            }
            finally
            {
                Cam.clearFlags = flags;
                Cam.backgroundColor = background;
                Cam.cullingMask = mask;
            }
            yield return null;
            yield return Capture("Floating_Lamp_chip_and_dial_over_room_peek");
        }

        // ---------------- the looks, for the vision pass ----------------

        // Both Floating Buttons on the real Lamp at 390 px, closed and opened by a real tap, at peek, in English and Portuguese
        [UnityTest]
        public IEnumerator Renders_BothFloatingButtons_ClosedAndOpen_OnTheRealLamp_InEnglishAndPortuguese()
        {
            foreach (string floating in new[] { BuiltInNavElements.FabDial, BuiltInNavElements.FabRadial })
                foreach (string language in new[] { "en", "pt" })
                {
                    yield return UseNavigation(floating, language: language);
                    yield return Open(SheetStopRule.Stop.Peek);
                    var fab = floating == BuiltInNavElements.FabDial ? Dial.Fab : Radial.Fab;
                    yield return Capture("Floating_Lamp_" + floating + "_closed_peek_" + language);
                    yield return Tap(fab.Button, floating);
                    Assert.IsTrue(fab.IsOpen, floating + " " + language + ": opened by a real tap");
                    yield return Capture("Floating_Lamp_" + floating + "_open_peek_" + language);
                    yield return Tap(fab.Button, floating + "'s X");
                    Assert.IsFalse(fab.IsOpen);
                }
        }
    }
}
