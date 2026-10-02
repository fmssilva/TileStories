using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace TileStories.Tests
{
    // Phase A promises of the ar family: show_on_wall, place_in_ar.
    public partial class CardGalleryTests
    {
        [UnityTest]
        public IEnumerator ShowOnWall_ARealTapLowersTheCardToPeek_TheNeighboursAreTheNearestPoints_AndATapOnOneSelectsIt()
        {
            SelectionEventBus.ResetState();
            ShowOnWallBlockView view = null;
            yield return ShowBlock("show_on_wall_button_short", v => view = (ShowOnWallBlockView)v);
            Assert.AreEqual("Show me where it is", view.ButtonLabel.text, "the card strings' words");
            Assert.AreEqual(0, view.Neighbours.Count, "the button look names no neighbours");
            Assert.IsFalse(CardTestInput.IsShown(view.NeighboursRow, view.Root));
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(view.Button.worldBound.width, view.Button.worldBound.height), "the button is a tap target >= 44 px");
            Assert.AreEqual(SheetStopRule.Stop.Full, _harness.Sheet.Stop, "the entry opens at full");
            yield return ScrollAndTap(view.Button);
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Peek, _harness.Sheet.Stop, "the card dropped to its peek so the wall shows");
            Assert.IsTrue(_harness.Sheet.IsOpen, "...and stays open");

            // - the neighbours: the three nearest of four (the fourth is 30 m away), by their card titles
            yield return ShowBlock("show_on_wall_with_neighbours_short", v => view = (ShowOnWallBlockView)v);
            CollectionAssert.AreEqual(new[] { "near_a", "near_b", "near_c" }, view.Neighbours.Select(n => n.PoiId).ToList(), "nearest first, three at most");
            Assert.AreEqual("The chapel of Saint George with its bell tower and the old cemetery", view.Neighbours[2].Title.text);
            Assert.AreEqual("Also nearby", view.Caption.text);
            foreach (var n in view.Neighbours)
                Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(n.Button.worldBound.width, n.Button.worldBound.height), "a neighbour is a tap target >= 44 px");
            yield return ScrollAndTap(view.Neighbours[1].Button);
            Assert.AreEqual("near_b", SelectionEventBus.CurrentPoiId, "a real tap on a neighbour selected it through the selection bus");
            SelectionEventBus.ResetState();
        }

        // place_in_ar (_3.1 step 10B.3, Phase A; placed state 15.2.3): the three states one block draws -- localised (the button), placed (the
        // SAME button reads Remove from room and a line says it is placed, kept while the card rises again), not localised (disabled, and a
        // line why) -- each through REAL taps on the gallery card, with the SAME ArPlacementService as the wall's over the harness's
        // ManualArWall, and the real Framework model placed in the world.
        [UnityTest]
        public IEnumerator PlaceInAr_ARealTapPlacesTheModelAndLowersTheCard_TheSameButtonRemovesIt_AndNotLocalisedTheButtonIsDisabledWithWhy()
        {
            var placement = _harness.ArPlacementService;
            var wall = _harness.ArWall;
            wall.IsLocalised = true;
            PlaceInArBlockView view = null;
            yield return ShowBlock("place_in_ar_button_short", v => view = (PlaceInArBlockView)v);
            Assert.AreEqual("See it here in 3D", view.ButtonLabel.text, "the card strings' words");
            Assert.IsTrue(view.Button.enabledInHierarchy, "localised: the button works");
            Assert.IsFalse(CardTestInput.IsShown(view.Note, view.Root), "localised: no line why not");
            Assert.IsFalse(view.IsPlaced, "nothing placed: the button places");
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(view.Button.worldBound.width, view.Button.worldBound.height), "the button is a tap target >= 44 px");
            yield return Render("Card_place_in_ar_localised");

            // - a real tap: the model stands in the world where the rule says, and the card drops to its peek
            yield return ScrollAndTap(view.Button);
            yield return CardTestInput.Settle();
            Assert.IsNotNull(placement.Placed, "a real tap placed the model");
            Assert.AreSame(wall.Root, placement.Placed.transform.parent, "in the wall's frame");
            Assert.AreEqual(1, wall.Root.childCount, "one model");
            Assert.AreEqual(SheetStopRule.Stop.Peek, _harness.Sheet.Stop, "the card dropped to its peek so the model shows");
            Assert.IsTrue(_harness.Sheet.IsOpen);
            var poi = placement.Current.Poi;
            Assert.IsTrue(POIPositionResolver.TryResolvePosition(poi, out var poiPosition, logErrors: false));
            var expected = ArPlacementRule.Place(new ArPlacementRule.Input
            {
                PoiPosition = poiPosition, PoiRotation = WallSession.AuthoredRotationOf(poi), Viewer = null, OffsetCm = 10f,
                ScaleMode = ArPlacementRule.ScaleRealSize, MarkerDiameter = wall.MarkerDiameter, WallScale = 1f,
                ModelBounds = ArPlacementService.LocalBoundsOf(placement.Placed),
            });
            Assert.Less(Vector3.Distance(expected.LocalPosition, placement.Placed.transform.localPosition), 0.001f, "the defaults' pose: 10 cm out, real size");
            Assert.AreEqual(1f, placement.Placed.transform.localScale.x, 0.0001f, "Real Size is the default");

            // - placed: the SAME button now reads Remove from room (the card strings' words) and a line says where the model is
            Assert.IsTrue(view.IsPlaced);
            Assert.AreEqual("Remove from room", view.ButtonLabel.text, "placed: the button's words change");
            Assert.AreEqual("Remove from room", view.Button.tooltip);
            Assert.IsTrue(CardTestInput.IsShown(view.Note, view.Root), "placed: a line says so");
            Assert.AreEqual("Placed by the wall", view.Note.text);
            Assert.AreEqual(0f, _harness.Sheet.Stack.Scroll.scrollOffset.y, 0.5f, "the card was scrolled to its top before it lowered to the peek");

            // - the card rises again: the model stays, the button still removes
            _harness.Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            Assert.IsTrue(view.IsPlaced, "re-opening the card keeps the model placed");
            Assert.IsNotNull(placement.Placed);
            Assert.IsTrue(view.Button.enabledInHierarchy, "placed: Remove works");
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(view.Button.worldBound.width, view.Button.worldBound.height), "the button is a tap target >= 44 px in its placed state too");
            Assert.GreaterOrEqual(CardTestInput.Contrast(view.ButtonLabel.resolvedStyle.color, CardTestInput.EffectiveBackground(view.ButtonLabel)),
                UIAccessibility.MinRatioNormalText, "the placed button's words read");
            Assert.GreaterOrEqual(CardTestInput.Contrast(view.Note.resolvedStyle.color, CardTestInput.EffectiveBackground(view.Note)),
                UIAccessibility.MinRatioNormalText, "the status line reads");
            yield return Render("Card_place_in_ar_placed");

            yield return ScrollAndTap(view.Button);
            yield return CardTestInput.Settle();
            Assert.IsNull(placement.Placed, "a real tap on the placed button took the model away");
            Assert.AreEqual(0, wall.Root.childCount, "nothing left in the world");
            Assert.IsFalse(view.IsPlaced, "the first state is back");
            Assert.AreEqual("See it here in 3D", view.ButtonLabel.text, "...the button places again");
            Assert.IsFalse(CardTestInput.IsShown(view.Note, view.Root), "...and the status line goes");
            Assert.AreEqual(SheetStopRule.Stop.Full, _harness.Sheet.Stop, "Remove does not move the card");

            // - the wall lost: disabled, a line says why, and a real tap places nothing and leaves the card where it is
            wall.IsLocalised = false;
            yield return CardTestInput.Settle();
            Assert.IsFalse(view.Button.enabledInHierarchy, "not localised: the button is disabled");
            Assert.IsTrue(CardTestInput.IsShown(view.Note, view.Root), "...and a line says why");
            Assert.AreEqual("Point the camera at the wall first, then place it.", view.Note.text);
            Assert.GreaterOrEqual(CardTestInput.Contrast(view.Note.resolvedStyle.color, CardTestInput.EffectiveBackground(view.Note)),
                UIAccessibility.MinRatioNormalText, "the line why reads");
            Assert.GreaterOrEqual(CardTestInput.Contrast(view.ButtonLabel.resolvedStyle.color, CardTestInput.EffectiveBackground(view.ButtonLabel)),
                UIAccessibility.MinRatioNormalText, "the disabled button's words still read");
            yield return Render("Card_place_in_ar_not_localised");
            yield return ScrollAndTap(view.Button);
            yield return CardTestInput.Settle();
            Assert.IsNull(placement.Placed, "a tap on the disabled button places nothing");
            Assert.AreEqual(SheetStopRule.Stop.Full, _harness.Sheet.Stop, "...and the card stays where it is");
            wall.IsLocalised = true;
            yield return CardTestInput.Settle();
            Assert.IsTrue(view.Button.enabledInHierarchy, "the wall found again: the button works again");

            // - the authored label and the Height scale: a real tap stands the arch 45 cm tall
            yield return ShowBlock("place_in_ar_button_long", v => view = (PlaceInArBlockView)v);
            Assert.AreEqual("Stand the tiled arch here in front of you, life size, and walk around it", view.ButtonLabel.text, "the authored Button Label wins");
            // - a long label wraps inside the pill (the first capture showed it on one line, running off both sides)
            var card = _harness.Sheet.Root.worldBound;
            Assert.LessOrEqual(view.Button.worldBound.xMax, card.xMax + 0.5f, "the button stays inside the card");
            Assert.GreaterOrEqual(view.ButtonLabel.worldBound.xMin, view.Button.worldBound.xMin - 0.5f, "the words start inside the button");
            Assert.LessOrEqual(view.ButtonLabel.worldBound.xMax, view.Button.worldBound.xMax + 0.5f, "the words end inside the button");
            var oneLine = view.ButtonLabel.MeasureTextSize(view.ButtonLabel.text, 0f, VisualElement.MeasureMode.Undefined, 0f, VisualElement.MeasureMode.Undefined);
            Assert.Greater(oneLine.x, view.ButtonLabel.contentRect.width, "precondition: the words are wider than the button");
            Assert.GreaterOrEqual(view.ButtonLabel.contentRect.height, oneLine.y * 1.8f, "so they wrap onto more lines");
            yield return ScrollAndTap(view.Button);
            yield return CardTestInput.Settle();
            Assert.IsNotNull(placement.Placed);
            var renderers = placement.Placed.GetComponentsInChildren<Renderer>();
            var box = renderers[0].bounds;
            foreach (var r in renderers) box.Encapsulate(r.bounds);
            Assert.AreEqual(0.45f, box.size.y, 0.005f, "Height 45 cm: the arch stands 45 cm tall in the world");

            // - the card closing takes it away
            _harness.Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            yield return CardTestInput.Tap(_harness.Sheet.CloseButton.panel, _harness.Sheet.CloseButton.worldBound.center);
            yield return CardTestInput.Settle();
            Assert.IsNull(placement.Placed, "closing the card removed the model");
            Assert.AreEqual(0, wall.Root.childCount);
        }
    }
}
