using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Tier 0 of what step 9 decides (_3.2 sections 4.2 and 5.4) with the numbers of the run's phone frame (390 x 844, The Lamp's stops 195 /
    // 337.6 / 796, a 48 px button 16 px from the right edge, 8 px above the sheet): where the floating slot rests at each stop
    // (FloatingSlotRule), which way the dial's list opens (FabFanRule), the radial's arc and which item a release picks -- every item's own
    // direction, a sector's edges, a release outside the arc and inside the dead zone (RadialMenuRule) -- and what the placed-model chip
    // says (PlacedModelChipRule: placed or not, this card or another, the control off).
    public class NavFloatingRulesTests
    {
        private const float Layer = 844f;
        private const float Width = 390f;
        private const float Button = 48f;
        private const float Gap = 8f;
        private const float Margin = 16f;
        // A radial item's size (its round button and its word under it), the smallest arc and the dead zone, as the card's tokens say
        private static readonly RadialMenuRule.ItemSize Item = new(96f, 76f);
        private const float MinRadius = 96f;
        private const float DeadZone = 24f;

        // The room around the button's centre when the sheet rests at this height
        private static RadialMenuRule.Room RoomAt(float sheetHeight)
        {
            float bottom = FloatingSlotRule.Bottom(sheetHeight, Layer, Button, Gap, Chrome);
            float centreY = Layer - bottom - Button / 2f;
            return new RadialMenuRule.Room(centreY, Layer - centreY, Width - Margin - Button / 2f, Margin + Button / 2f);
        }

        private static RadialMenuRule.Arc ArcAt(int count, RadialMenuRule.Room room) => RadialMenuRule.Layout(count, room, Item, MinRadius);

        // ---------------- FloatingSlotRule ----------------

        // The chrome row's foot under the sheet's top edge: --ts-bar-top 6 + --ts-bar-height 44
        private const float Chrome = 50f;

        [Test]
        public void TheSlot_RestsAGapAboveTheSheetAtPeekAndHalf_AndFlushInTheCameraStripAtFull()
        {
            Assert.AreEqual(195f + Gap, FloatingSlotRule.Bottom(195f, Layer, Button, Gap, Chrome), 1e-3f, "peek: 8 px above the sheet's top edge");
            Assert.AreEqual(337.6f + Gap, FloatingSlotRule.Bottom(337.6f, Layer, Button, Gap, Chrome), 1e-3f, "half: the same, higher");
            // - Full with a camera strip leaves 48 px above the sheet (--ts-sheet-top-gap): the slot rests there, flush on the sheet
            Assert.AreEqual(Layer - Button, FloatingSlotRule.Bottom(796f, Layer, Button, Gap, Chrome), 1e-3f, "full with a strip: never past the layer's top");
            Assert.AreEqual(796f, Layer - Button, 1e-3f, "precondition: at Full the sheet's top IS the slot's bottom (flush, no overlap)");
            // - a dragged sheet is followed exactly, a closed one (0) leaves the slot a gap above the bottom
            Assert.AreEqual(500f + Gap, FloatingSlotRule.Bottom(500f, Layer, Button, Gap, Chrome), 1e-3f);
            Assert.AreEqual(Gap, FloatingSlotRule.Bottom(0f, Layer, Button, Gap, Chrome), 1e-3f);
        }

        // _Test_Suite_1.md F1: at Full to the top the slot cannot rest above the sheet; it rests a gap UNDER the chrome row, never over the X
        [Test]
        public void AtFullToTheTop_TheSlotRestsUnderTheChromeRow_AGapBelowItsFoot()
        {
            float bottom = FloatingSlotRule.Bottom(Layer, Layer, Button, Gap, Chrome);
            float top = Layer - bottom - Button;
            Assert.AreEqual(Chrome + Gap, top, 1e-3f, "its top a gap under the chrome row's foot");
            // - a sheet a few points short of the screen's top: the slot would hang over the sheet's top edge -- under the row too
            float nearly = Layer - 20f;
            float topNearly = Layer - FloatingSlotRule.Bottom(nearly, Layer, Button, Gap, Chrome) - Button;
            Assert.AreEqual(Layer - nearly + Chrome + Gap, topNearly, 1e-3f, "under the row of a sheet that leaves too little room above it");
        }

        // The rule's promise as a sweep: at every sheet height (a drag passes through all of them) the slot never overlaps the chrome row
        // (the sheet's top edge down to the row's foot), and it is never off the layer
        [Test]
        public void AtEverySheetHeight_TheSlotNeverOverlapsTheChromeRow_AndStaysOnTheLayer()
        {
            for (float sheet = 0f; sheet <= Layer; sheet += 0.5f)
            {
                float bottom = FloatingSlotRule.Bottom(sheet, Layer, Button, Gap, Chrome);
                float slotTop = Layer - bottom - Button, slotFoot = Layer - bottom;
                float rowTop = Layer - sheet, rowFoot = rowTop + Chrome;
                Assert.IsTrue(slotFoot <= rowTop + 1e-3f || slotTop >= rowFoot - 1e-3f,
                    "sheet " + sheet + ": the slot " + slotTop + ".." + slotFoot + " overlaps the chrome row " + rowTop + ".." + rowFoot);
                Assert.GreaterOrEqual(slotTop, -1e-3f, "sheet " + sheet + ": on the layer (top)");
                Assert.GreaterOrEqual(bottom, 0f, "sheet " + sheet + ": on the layer (bottom)");
            }
        }

        [Test]
        public void TheSlot_ShowsOnlyOverAnOpenCard_WithSomethingInIt()
        {
            Assert.IsTrue(FloatingSlotRule.Shown(true, true));
            Assert.IsFalse(FloatingSlotRule.Shown(true, false), "nothing in it: no empty row");
            Assert.IsFalse(FloatingSlotRule.Shown(false, true), "no card: no slot");
            Assert.IsFalse(FloatingSlotRule.Shown(false, false));
        }

        [Test]
        public void TheCardDims_OnlyWhileTheSlotsOwnElementIsOpen_AsAnAnchoredThing()
        {
            Assert.IsTrue(FloatingSlotRule.Dims(BuiltInNavElements.FabDial, false, BuiltInNavElements.FabDial), "the dial's column open: dimmed");
            Assert.IsFalse(FloatingSlotRule.Dims(null, false, BuiltInNavElements.FabDial), "nothing open");
            Assert.IsFalse(FloatingSlotRule.Dims("bottom_bar_more", false, BuiltInNavElements.FabDial), "another element's list: the card stays readable");
            Assert.IsFalse(FloatingSlotRule.Dims(BuiltInNavElements.DrawerTree, true, BuiltInNavElements.FabDial), "a modal menu dims with its own scrim, over the slot");
            Assert.IsFalse(FloatingSlotRule.Dims(BuiltInNavElements.FabDial, false, null), "no floating element on this card");
        }

        // ---------------- FabFanRule ----------------

        [Test]
        public void TheDialsList_OpensUpWhenItFits_ElseDown_ElseTowardsTheBiggerRoom()
        {
            // - The Lamp's 8 sections: 8 pills of a touch target and a 4 px gap
            float list = 8 * (Button + 4f);
            var peek = RoomAt(195f);
            Assert.AreEqual(FabFanRule.Direction.Up, FabFanRule.Opens(peek.Above - Button / 2f, peek.Below - Button / 2f, list), "peek: the camera view above holds it");
            var full = RoomAt(796f);
            Assert.AreEqual(FabFanRule.Direction.Down, FabFanRule.Opens(full.Above - Button / 2f, full.Below - Button / 2f, list), "full: no room above, down over the card");
            Assert.AreEqual(FabFanRule.Direction.Up, FabFanRule.Opens(300f, 200f, 600f), "neither fits: the bigger side, where it scrolls");
            Assert.AreEqual(FabFanRule.Direction.Down, FabFanRule.Opens(200f, 300f, 600f));
            Assert.AreEqual(300f, FabFanRule.Room(FabFanRule.Direction.Up, 300f, 200f));
            Assert.AreEqual(200f, FabFanRule.Room(FabFanRule.Direction.Down, 300f, 200f));
            Assert.AreEqual(0f, FabFanRule.Room(FabFanRule.Direction.Up, -5f, 200f), "never a negative room");
        }

        // ---------------- RadialMenuRule: the arc ----------------

        // Every item of `arc` -- its whole rectangle -- inside the room on all four sides, no two overlapping (a width apart across or a height
        // apart down), at most 45 degrees apart, top to bottom
        private static void AssertInsideAndApart(RadialMenuRule.Arc arc, RadialMenuRule.Room room, string at)
        {
            Assert.IsTrue(arc.Fits, at + ": the arc fits");
            for (int i = 0; i < arc.Count; i++)
            {
                var offset = arc.Offset(i);
                Assert.LessOrEqual(-offset.y + Item.Height / 2f, room.Above + 0.01f, at + ": item " + i + " stays under the layer's top");
                Assert.LessOrEqual(offset.y + Item.Height / 2f, room.Below + 0.01f, at + ": item " + i + " stays over the layer's bottom");
                Assert.LessOrEqual(-offset.x + Item.Width / 2f, room.LeftOf + 0.01f, at + ": item " + i + " stays inside the layer's left edge");
                Assert.LessOrEqual(offset.x + Item.Width / 2f, room.RightOf + 0.01f,
                    at + ": item " + i + " stays inside the layer's RIGHT edge (straight up or down it sits on the button's column)");
                Assert.LessOrEqual(offset.x, 0.01f, at + ": item " + i + "'s centre lies left of the button's (the left half-plane)");
                if (i > 0) Assert.Greater(offset.y, arc.Offset(i - 1).y, at + ": the first item on top, the rest in reading order");
                for (int j = 0; j < i; j++)
                {
                    var gap = offset - arc.Offset(j);
                    Assert.IsTrue(Mathf.Abs(gap.x) >= Item.Width - 0.01f || Mathf.Abs(gap.y) >= Item.Height - 0.01f,
                        at + ": items " + j + " and " + i + " never overlap (" + gap + ")");
                }
            }
            Assert.GreaterOrEqual(RadialMenuRule.Apart(arc, Item), 1f - 1e-4f, at + ": Apart agrees");
            Assert.LessOrEqual(arc.StepDegrees, RadialMenuRule.MaxStepDegrees + 1e-3f, at + ": never wider than a marking menu's eighth");
        }

        [Test]
        public void TheArcAtPeekAndHalf_IsTheHalfCircleLeftOfTheButton_AtTheSmallestRadiusThatKeepsFiveItemsApart()
        {
            foreach (var (stop, sheet) in new[] { ("peek", 195f), ("half", 337.6f) })
            {
                var room = RoomAt(sheet);
                var arc = ArcAt(5, room);
                AssertInsideAndApart(arc, room, stop);
                // - five items over (nearly) the whole left half: from just past straight up to just short of straight down (the right edge
                //   keeps the end items off the button's own column), centred on left
                Assert.AreEqual(180f, arc.DegreesOf(2), 0.01f, stop + ": the middle item straight left");
                Assert.AreEqual(360f, arc.DegreesOf(0) + arc.DegreesOf(4), 0.01f, stop + ": the arc symmetric about left");
                Assert.Greater(arc.StepDegrees, 40f, stop + ": sectors of about an eighth each: a slide has room to miss");
                // - the smallest radius that keeps them apart: one less and two items would overlap
                var tighter = new RadialMenuRule.Arc(5, arc.Radius - 1f, arc.FirstDegrees, arc.StepDegrees, true);
                Assert.Less(RadialMenuRule.Apart(tighter, Item), 1f, stop + ": a radius smaller by one would crowd them");
                Assert.AreEqual(5, RadialMenuRule.Capacity(5, room, Item, MinRadius), stop + ": all of Bar Slots fits");
            }
        }

        [Test]
        public void TheArcAtFull_TurnsLeftAndDown_HoldsFewerItems_AndStillKeepsThemApart()
        {
            var room = RoomAt(796f);
            Assert.AreEqual(Button / 2f, room.Above, 0.01f, "precondition: at Full the button's centre is half a button under the top");
            Assert.IsFalse(ArcAt(5, room).Fits, "five items do not fit a quarter arc in the screen's corner: said, never hidden");
            int capacity = RadialMenuRule.Capacity(5, room, Item, MinRadius);
            Assert.AreEqual(4, capacity, "four do: the arc holds what fits, the rest behind More");
            var arc = ArcAt(capacity, room);
            AssertInsideAndApart(arc, room, "full");
            Assert.Greater(arc.DegreesOf(0), 180f, "no room above: even the first item lies below the button's row");
            Assert.Greater(arc.DegreesOf(capacity - 1), 260f, "the last nearly straight down, over the card");
        }

        [Test]
        public void AShortArc_IsCentredOnLeft_ACrampedRoomHoldsTwo_AndCapacityNeverGoesUnderTwoOrOverWhatIsWanted()
        {
            var room = RoomAt(195f);
            var two = ArcAt(2, room);
            AssertInsideAndApart(two, room, "two items");
            Assert.AreEqual(157.5f, two.DegreesOf(0), 0.01f, "45 degrees apart, centred on left");
            Assert.AreEqual(202.5f, two.DegreesOf(1), 0.01f);
            // - two items mirrored about left sit straight above one another: 0.765 R apart (2 sin 22.5) must reach the item's height
            Assert.AreEqual(100f, two.Radius, "the smallest radius that puts the two a height apart");
            var cramped = new RadialMenuRule.Room(30f, 30f, 120f, 40f);
            Assert.IsFalse(RadialMenuRule.Layout(5, cramped, Item, MinRadius).Fits, "a room no arc of five fits in: said, never hidden");
            Assert.AreEqual(5, RadialMenuRule.Layout(5, cramped, Item, MinRadius).Count, "and every item still has a place");
            Assert.AreEqual(RadialMenuRule.MinItems, RadialMenuRule.Capacity(5, cramped, Item, MinRadius), "never under two: one section and More");
            Assert.AreEqual(3, RadialMenuRule.Capacity(3, room, Item, MinRadius), "never more than wanted (Bar Slots 3)");
        }

        // ---------------- RadialMenuRule: the release ----------------

        // A travel of `length` in the direction `degrees` (0 = right, 90 = up), in panel units (y down)
        private static Vector2 Travel(float degrees, float length = 115f) =>
            new(length * Mathf.Cos(degrees * Mathf.Deg2Rad), -length * Mathf.Sin(degrees * Mathf.Deg2Rad));

        [Test]
        public void AReleaseTowardsAnItem_PicksIt_ASectorsEdgesBelongToIt_AndOutsideTheArcOrInsideTheDeadZoneNothing()
        {
            var arc = ArcAt(5, RoomAt(195f));
            for (int i = 0; i < arc.Count; i++)
            {
                Assert.AreEqual(i, RadialMenuRule.ItemAt(Travel(arc.DegreesOf(i)), arc, DeadZone), "towards item " + i);
                Assert.AreEqual(i, RadialMenuRule.ItemAt(Travel(arc.DegreesOf(i), DeadZone + 1f), arc, DeadZone), "a short slide counts once past the dead zone");
                Assert.AreEqual(i, RadialMenuRule.ItemAt(arc.Offset(i), arc, DeadZone), "the finger ending on the item itself");
            }
            float half = arc.StepDegrees / 2f;
            Assert.AreEqual(0, RadialMenuRule.ItemAt(Travel(arc.DegreesOf(0) - half + 0.1f), arc, DeadZone), "just inside the first item's outer edge");
            Assert.AreEqual(0, RadialMenuRule.ItemAt(Travel(arc.DegreesOf(0) - half), arc, DeadZone), "ON the outer edge: still the first");
            Assert.AreEqual(-1, RadialMenuRule.ItemAt(Travel(arc.DegreesOf(0) - half - 1f), arc, DeadZone), "past it: outside the arc");
            Assert.AreEqual(4, RadialMenuRule.ItemAt(Travel(arc.DegreesOf(4) + half), arc, DeadZone), "the last item's outer edge");
            Assert.AreEqual(-1, RadialMenuRule.ItemAt(Travel(arc.DegreesOf(4) + half + 1f), arc, DeadZone), "past the last: outside");
            Assert.AreEqual(1, RadialMenuRule.ItemAt(Travel(arc.DegreesOf(1) + half - 0.1f), arc, DeadZone), "just before the border: still item 1");
            Assert.AreEqual(2, RadialMenuRule.ItemAt(Travel(arc.DegreesOf(1) + half + 0.1f), arc, DeadZone), "just after it: item 2");
            Assert.AreEqual(1, RadialMenuRule.ItemAt(Travel(arc.DegreesOf(1) + half), arc, DeadZone), "exactly between two: the first of them");
            Assert.AreEqual(-1, RadialMenuRule.ItemAt(Travel(0f), arc, DeadZone), "towards the screen's edge, away from the arc: nothing");
            Assert.AreEqual(-1, RadialMenuRule.ItemAt(Travel(20f), arc, DeadZone));
            Assert.AreEqual(-1, RadialMenuRule.ItemAt(Travel(arc.DegreesOf(2), DeadZone - 1f), arc, DeadZone), "inside the dead zone: the finger never left the button");
            Assert.AreEqual(-1, RadialMenuRule.ItemAt(Vector2.zero, arc, DeadZone));
        }

        [Test]
        public void TheDirectionOfATravel_IsReadWithPanelYDown()
        {
            Assert.AreEqual(0f, RadialMenuRule.DegreesOf(new Vector2(10f, 0f)), 1e-3f, "right");
            Assert.AreEqual(90f, RadialMenuRule.DegreesOf(new Vector2(0f, -10f)), 1e-3f, "up: a smaller panel y");
            Assert.AreEqual(180f, RadialMenuRule.DegreesOf(new Vector2(-10f, 0f)), 1e-3f, "left");
            Assert.AreEqual(270f, RadialMenuRule.DegreesOf(new Vector2(0f, 10f)), 1e-3f, "down");
            Assert.AreEqual(315f, RadialMenuRule.DegreesOf(new Vector2(10f, 10f)), 1e-3f, "down-right, never negative");
        }

        // ---------------- PlacedModelChipRule ----------------

        [Test]
        public void TheChip_SaysPlacedWithRemoveOnTheCardThatPlacedIt_PlacedWithoutRemoveOnAnother_AndNothingOtherwise()
        {
            string chip = CardOptions.PlacedModelChip, none = CardOptions.PlacedModelNone;
            Assert.AreEqual(PlacedModelChipRule.State.ThisCard, PlacedModelChipRule.Of(chip, true, "lamp", "lamp"), "the card that placed it: Remove");
            Assert.AreEqual(PlacedModelChipRule.State.OtherCard, PlacedModelChipRule.Of(chip, true, "lamp", "lamp_military"), "another point's card: no Remove");
            Assert.AreEqual(PlacedModelChipRule.State.Hidden, PlacedModelChipRule.Of(chip, true, null, "lamp"), "nothing stands");
            Assert.AreEqual(PlacedModelChipRule.State.Hidden, PlacedModelChipRule.Of(chip, false, "lamp", null), "no card open");
            Assert.AreEqual(PlacedModelChipRule.State.Hidden, PlacedModelChipRule.Of(none, true, "lamp", "lamp"), "Placed Model Control = None");
            Assert.AreEqual(PlacedModelChipRule.State.ThisCard, PlacedModelChipRule.Of("garbled", true, "lamp", "lamp"), "an unknown value reads as the default");
            Assert.AreEqual(PlacedModelChipRule.State.ThisCard, PlacedModelChipRule.Of(null, true, "lamp", "lamp"), "an old config without the key: the default");
            Assert.AreEqual(chip, new CardNavigationSettings().placed_model_control, "the default is the chip");
            Assert.AreEqual(CardOptions.NavNone, new CardNavigationSettings().floating, "and no Floating Button");
            Assert.AreEqual(chip, CardOptions.PlacedModelControls[0], "the first option is the default the card reads for an unknown value");
        }
    }
}
