using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of the ar family on the real LivingRoomScene: show_on_wall.
    public partial class PoiCardSceneTests
    {
        // _3.1 step 8B: The Lamp's show_on_wall blocks on the RUNNING wall: a real tap on the button lowers the card to its peek and the
        // point's marker is the lit one (the selection stays: the others dim); the neighbour look names the three nearest points of the
        // wall and a real tap on one selects it
        [UnityTest]
        public IEnumerator TheLamp_ShowOnWall_ARealTapDropsTheCardToPeek_TheRightMarkerStaysLit_AndNeighboursAreTheNearestPoints()
        {
            yield return OpenFull("lamp");
            var views = Sheet.Stack.BoundViews.OfType<ShowOnWallBlockView>().ToList();
            Assert.AreEqual(2, views.Count, "the button and the neighbour look");
            Assert.AreEqual(0, views[0].Neighbours.Count);
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop);

            yield return ScrollAndTap(views[0].Button);
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Peek, Sheet.Stop, "the card dropped to its peek");
            Assert.IsTrue(Sheet.IsOpen);
            Assert.AreEqual("lamp", SelectionEventBus.CurrentPoiId, "the point stays selected");
            Assert.AreEqual(1f, Marker("lamp").SelectionAlpha, 1e-3, "THE RIGHT marker is the lit one on the running wall");
            Assert.AreEqual(Session.SpawnedMarkers.Count - 1, MarkersAt(0.3f).Count, "every other marker dimmed");
            yield return Capture("Card_Lamp_ShowOnWall_Peek");

            // - the neighbours: the three nearest other points by straight-line distance (worked out here, from the positions alone)
            SelectionEventBus.Clear();
            yield return OpenFull("lamp");
            var neighbours = Sheet.Stack.BoundViews.OfType<ShowOnWallBlockView>().Last();
            var lampPoi = Session.SearchPois.First(p => p.id == "lamp");
            POIPositionResolver.TryResolvePosition(lampPoi, out var here, logErrors: false);
            var expected = Session.SearchPois.Where(p => p.id != "lamp")
                .Select(p => (p, ok: POIPositionResolver.TryResolvePosition(p, out var at, logErrors: false), at))
                .Where(x => x.ok)
                .OrderBy(x => Vector3.Distance(here, x.at)).ThenBy(x => x.p.id, System.StringComparer.Ordinal)
                .Take(3).Select(x => x.p.id).ToList();
            CollectionAssert.AreEqual(expected, neighbours.Neighbours.Select(n => n.PoiId).ToList(), "the nearest three, nearest first");
            for (int i = 0; i < 3; i++)
                Assert.AreEqual(BlockStackBuilder.CardTitleOf(Session.SearchPois.First(p => p.id == expected[i]), "en", "en"), neighbours.Neighbours[i].Title.text, "named by card title");
            yield return ScrollTo(neighbours);
            yield return Capture("Card_Lamp_ShowOnWall_Neighbours");
            yield return ScrollAndTap(neighbours.Neighbours[0].Button);
            yield return CardTestInput.Settle();
            Assert.AreEqual(expected[0], SelectionEventBus.CurrentPoiId, "a real tap on a neighbour selected it");
            Assert.AreEqual(expected[0], Card.ShownPoiId, "the card is now its card");
        }
    }
}
