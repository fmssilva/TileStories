using System.Linq;
using NUnit.Framework;
using UnityEngine;
using TileStories.Tests;

namespace TileStories.Editor.Tests
{
    // NavFloatingRulesTests's shared ground: the run's phone frame and the slot's numbers
    public abstract class NavFloatingRulesFixture
    {
        protected const float Layer = 844f;

        protected const float Width = 390f;

        protected const float Button = 48f;

        protected const float Gap = 8f;

        protected const float Margin = 16f;

        // ---------------- FloatingSlotRule ----------------

        // The chrome row's foot under the sheet's top edge: --ts-bar-top 6 + --ts-bar-height 44
        protected const float Chrome = 50f;
    }

    // Tier 0 of what step 9 decides (_3.2 section 4.2) with the numbers of the run's phone frame (390 x 844, The Lamp's stops 195 /
    // 337.6 / 796, a 48 px slot 16 px from the right edge, 8 px above the sheet): where the floating slot rests at each stop
    // (FloatingSlotRule) and what the placed-model chip says (PlacedModelChipRule: placed or not, this card or another, the control off).
    public class NavFloatingRulesTests : NavFloatingRulesFixture
    {
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
            Assert.AreEqual(chip, CardOptions.PlacedModelControls[0], "the first option is the default the card reads for an unknown value");
        }
    }
}
