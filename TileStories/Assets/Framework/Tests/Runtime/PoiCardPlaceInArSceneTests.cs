using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of place_in_ar (_3.1 step 10B.3, 40-testing 4.1 Tier A): the REAL LivingRoomScene with its MockLocalizationProvider, the shipped
    // config (The Lamp's block_62: `default:azulejo_arch`, every other field at its default) and the real ArPlacementService behind PoiCardHost
    // over the wall's own WallArSurface. Real taps place the arch in the world at The Lamp's wall position; the pose is asserted against
    // ArPlacementRule for every Scale option within the tolerances below; the model is seen in the Game view (real screen pixels); Remove, a
    // second placement, losing the wall and closing the card each do what the brief says, and nothing is left behind.
    public class PoiCardPlaceInArSceneTests : SearchSceneFixture
    {
        // The stated tolerances: 1 mm of position, 0.1 degree of rotation, 5 mm of height in the world
        private const float PositionToleranceMetres = 0.001f;
        private const float RotationToleranceDegrees = 0.1f;
        private const float HeightToleranceMetres = 0.005f;
        private const string Arch = "default:azulejo_arch";

        private PoiCardSheetView Sheet => Card.Sheet;
        private ArPlacementService Placement => Card.ArPlacementService;
        private Transform WallFrame => Session.MarkerSpawnRoot;
        private POIData Lamp => Session.SearchPois.First(p => p.id == "lamp");
        private BlockInstanceData LampBlock => Lamp.card.blocks.First(b => b.key == "block_62");

        private IEnumerator OpenFull(string poiId)
        {
            // - selecting the point already selected would CLEAR it (a second tap on a marker deselects)
            if (SelectionEventBus.CurrentPoiId != poiId) SelectionEventBus.Select(poiId);
            yield return CardTestInput.Settle();
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
        }

        private PlaceInArBlockView View(string blockKey = "block_62")
        {
            var view = Sheet.Stack.BoundViews.OfType<PlaceInArBlockView>().FirstOrDefault(v => v.BlockKey == blockKey);
            if (view == null)
            {
                var skipped = BlockStackBuilder.Build(Lamp, Session.CardSettings, BlockRegistry.Shared, Session.SearchPois).Skipped;
                Assert.Fail("precondition: The Lamp's card shows " + blockKey + "; shown " + Card.ShownPoiId + " with " + Sheet.Stack.BoundViews.Count +
                    " views, place_in_ar views [" + string.Join(",", Sheet.Stack.BoundViews.OfType<PlaceInArBlockView>().Select(v => v.BlockKey)) + "], skipped [" +
                    string.Join(",", skipped.Select(k => k.Instance.key + ":" + k.Reason + ":" + k.FieldKey)) + "]");
            }
            return view;
        }

        // Scroll the card to `target` and tap its centre for real
        private IEnumerator ScrollAndTap(VisualElement target)
        {
            Sheet.Stack.Scroll.ScrollTo(target);
            yield return CardTestInput.Settle(0.2f);
            yield return CardTestInput.Tap(target.panel, target.worldBound.center);
            yield return CardTestInput.Settle();
        }

        // Set (or clear, with null) one field of a block in the running wall's config, as a live edit does
        private static void SetField(BlockInstanceData block, string key, string value = null, float? number = null)
        {
            block.fields.RemoveAll(f => f.key == key);
            if (value != null) block.fields.Add(new BlockFieldValue { key = key, value = value });
            if (number.HasValue) block.fields.Add(new BlockFieldValue { key = key, number = number.Value });
        }

        // The placed model's box in the world (it stands upright, so its height is its own)
        private static Bounds WorldBox(GameObject model)
        {
            var renderers = model.GetComponentsInChildren<Renderer>();
            var box = renderers[0].bounds;
            foreach (var r in renderers) box.Encapsulate(r.bounds);
            return box;
        }

        private int PlacedInTheWorld() => Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Count(t => t.name.StartsWith("ArPlaced_"));

        // The visitor turns to the placed model (the mock's own look does the same with the mouse)
        private void LookAtTheModel() => Cam.transform.LookAt(WorldBox(Placement.Placed).center);

        [UnityTest]
        public IEnumerator TheLamp_ARealTapPlacesTheArchAtItsWallPosition_ThePoseIsTheRulesForEveryScale_AndTheCardDropsToPeek()
        {
            Assert.IsTrue(Session.GetComponent<MockLocalizationProvider>().IsLocalised, "precondition: the mock has localised the wall");
            Assert.IsTrue(POIPositionResolver.TryResolvePosition(Lamp, out var lampPosition, logErrors: false));
            Assert.IsTrue(MarkerHierarchyResolver.TryResolveByKey(Lamp.hierarchy_level_key, out var level), "precondition: The Lamp's level is known");
            float markerDiameter = level.SizeCm / 100f;
            float wallScale = WallFrame.lossyScale.x;

            foreach (var scale in new[] { ArPlacementRule.ScaleRealSize, ArPlacementRule.ScaleHeightCm, ArPlacementRule.ScaleMarkerMultiple })
            {
                SetField(LampBlock, BuiltInBlocks.PlaceInArScaleField, value: scale == ArPlacementRule.ScaleRealSize ? null : scale);
                SetField(LampBlock, BuiltInBlocks.PlaceInArHeightField, number: 40f);
                SetField(LampBlock, BuiltInBlocks.PlaceInArMultipleField, number: 4f);
                yield return OpenFull("lamp");
                Card.Rebind();
                yield return CardTestInput.Settle();
                var view = View();
                Assert.IsTrue(view.Button.enabledInHierarchy, scale + ": localised, the button works");
                Assert.AreEqual("See it here in 3D", view.ButtonLabel.text);

                yield return ScrollAndTap(view.Button);
                var model = Placement.Placed;
                Assert.IsNotNull(model, scale + ": a real tap placed the arch");
                Assert.AreSame(WallFrame, model.transform.parent, scale + ": in the wall's frame");
                Assert.AreEqual(Arch, Placement.Current.ModelPath);
                Assert.AreEqual(SheetStopRule.Stop.Peek, Sheet.Stop, scale + ": the card dropped to its peek");
                Assert.IsTrue(Sheet.IsOpen);

                // - the rule from the wall's own facts: The Lamp's position and authored facing, the camera as the viewer, the defaults
                var expected = ArPlacementRule.Place(new ArPlacementRule.Input
                {
                    PoiPosition = lampPosition, PoiRotation = WallSession.AuthoredRotationOf(Lamp),
                    Viewer = WallFrame.InverseTransformPoint(Cam.transform.position), OffsetCm = 10f, ScaleMode = scale, HeightCm = 40f,
                    MarkerMultiple = 4f, MarkerDiameter = markerDiameter, WallScale = wallScale, ModelBounds = ArPlacementService.LocalBoundsOf(model),
                });
                Assert.Less(Vector3.Distance(WallFrame.TransformPoint(expected.LocalPosition), model.transform.position), PositionToleranceMetres, scale + ": world position");
                Assert.Less(Quaternion.Angle(WallFrame.rotation * expected.LocalRotation, model.transform.rotation), RotationToleranceDegrees, scale + ": world rotation");
                // - and what it means, measured in the world: the arch's centre 10 cm out from The Lamp, towards the camera, at the promised height
                var box = WorldBox(model);
                var lampWorld = WallFrame.TransformPoint(lampPosition);
                Assert.AreEqual(0.1f, Vector3.Distance(lampWorld, box.center), 0.01f, scale + ": 10 cm out from The Lamp (the world box of an upright model)");
                Assert.Less(Vector3.Distance(Cam.transform.position, box.center), Vector3.Distance(Cam.transform.position, lampWorld), scale + ": on the camera's side of the wall");
                float promised = scale == ArPlacementRule.ScaleHeightCm ? 0.40f
                    : scale == ArPlacementRule.ScaleMarkerMultiple ? 4f * markerDiameter * wallScale
                    : ArPlacementService.LocalBoundsOf(model).size.y * wallScale;
                Assert.AreEqual(promised, box.size.y, HeightToleranceMetres, scale + ": the arch's height in the world");

                LookAtTheModel();
                yield return CardTestInput.Settle(0.2f);
                yield return Capture("PlaceInAr_Lamp_" + scale);
            }
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            Assert.IsNull(Placement.Placed, "closing the card removed it");
        }

        [UnityTest]
        public IEnumerator TheLamp_ThePlacedArchShowsInTheGameView_RemoveTakesItAway_ASecondReplacesIt_LosingTheWallDisables_ClosingLeavesNothing()
        {
            yield return OpenFull("lamp");
            yield return ScrollAndTap(View().Button);
            var model = Placement.Placed;
            Assert.IsNotNull(model, "a real tap placed the arch");

            // - the Game view: the arch's centre projects on screen, above the peek card, and real pixels there change with it
            LookAtTheModel();
            yield return CardTestInput.Settle(0.2f);
            var centre = Cam.WorldToScreenPoint(WorldBox(model).center);
            Assert.Greater(centre.z, 0f, "in front of the camera");
            float cardTop = Screen.height - Sheet.TargetHeight * Screen.height / Sheet.Layer.layout.height;
            Assert.Less(Screen.height - centre.y, cardTop, "above the peek card: the visitor sees it");
            // - the arch's centre is its opening (the Lamp's marker shows through it): judge a grid over its whole screen box instead
            var screenBox = ScreenBoxOf(WorldBox(model));
            Texture2D withModel = null, without = null;
            yield return Grab(t => withModel = t);
            yield return Capture("PlaceInAr_Lamp_Peek_GameView");
            model.SetActive(false);
            yield return Grab(t => without = t);
            model.SetActive(true);
            float changed = ChangedShare(withModel, without, screenBox, Screen.height - cardTop);
            Object.Destroy(withModel);
            Object.Destroy(without);
            Assert.GreaterOrEqual(changed, 0.2f, "the arch is drawn where it stands: " + changed.ToString("P0") + " of its screen box (" + screenBox + ") changes without it");

            // - the card rises: still placed, the Remove chip shows; a real tap on it takes the arch away
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            var view = View();
            Assert.IsTrue(view.IsPlaced, "re-opening keeps it placed");
            Assert.IsTrue(CardTestInput.IsShown(view.RemoveChip, view.Root), "the Remove chip");
            Sheet.Stack.Scroll.ScrollTo(view.RemoveChip);
            yield return CardTestInput.Settle(0.2f);
            yield return Capture("PlaceInAr_Lamp_RemoveChip");
            yield return ScrollAndTap(view.RemoveChip);
            Assert.IsNull(Placement.Placed, "Remove took it away");
            Assert.AreEqual(0, PlacedInTheWorld(), "nothing in the world");
            Assert.IsFalse(CardTestInput.IsShown(view.RemoveChip, view.Root));

            // - a second place_in_ar block (in memory) replaces the first's model
            var second = new BlockInstanceData { key = "block_62b", kind = BuiltInBlocks.PlaceInArKind, variant = BuiltInBlocks.PlaceInArButton };
            second.fields.Add(new BlockFieldValue { key = BuiltInBlocks.PlaceInArModelField, asset = Arch });
            second.fields.Add(new BlockFieldValue { key = BuiltInBlocks.PlaceInArScaleField, value = ArPlacementRule.ScaleHeightCm });
            Lamp.card.blocks.Add(second);
            try
            {
                Card.Rebind();
                yield return CardTestInput.Settle();
                yield return ScrollAndTap(View().Button);
                var first = Placement.Placed;
                Sheet.SetStop(SheetStopRule.Stop.Full);
                yield return CardTestInput.Settle();
                yield return ScrollAndTap(View("block_62b").Button);
                Assert.IsTrue(first == null, "the first model is destroyed");
                Assert.IsTrue(Placement.IsPlaced("lamp", "block_62b"));
                Assert.IsFalse(Placement.IsPlaced("lamp", "block_62"));
                Assert.AreEqual(1, PlacedInTheWorld(), "ONE model in the world");
                Sheet.SetStop(SheetStopRule.Stop.Full);
                yield return CardTestInput.Settle();
                Assert.IsFalse(CardTestInput.IsShown(View().RemoveChip, View().Root), "only the block that placed it shows Remove");
                Assert.IsTrue(CardTestInput.IsShown(View("block_62b").RemoveChip, View("block_62b").Root));

                // - the wall lost: every place_in_ar button disabled with its line why; found again: enabled
                var tracker = Session.GetComponent<MockLocalizationProvider>();
                tracker.LoseTracking();
                yield return CardTestInput.Settle();
                Assert.IsFalse(View().Button.enabledInHierarchy, "not localised: disabled");
                Assert.IsTrue(CardTestInput.IsShown(View().Note, View().Root), "...with the line why");
                Assert.AreEqual("Point the camera at the wall first, then place it.", View().Note.text);
                Sheet.Stack.Scroll.ScrollTo(View().Note);
                yield return CardTestInput.Settle(0.2f);
                yield return Capture("PlaceInAr_Lamp_NotLocalised");
                tracker.Relocalise();
                yield return CardTestInput.Settle();
                Assert.IsTrue(View().Button.enabledInHierarchy, "the wall found again");

                // - closing the card (a real tap on its X) removes the model and gives back every load
                yield return CardTestInput.Tap(Sheet.CloseButton.panel, Sheet.CloseButton.worldBound.center);
                yield return CardTestInput.Settle();
                Assert.IsFalse(Sheet.IsOpen, "the card closed");
                Assert.IsNull(Placement.Placed, "closing the card removed the model");
                Assert.AreEqual(0, PlacedInTheWorld(), "nothing left in the world");
                Assert.AreEqual(0, Card.Media.RefCount(Arch), "every load of the arch given back");
            }
            finally { Lamp.card.blocks.Remove(second); }
        }

        // A real capture of the Game view (the caller destroys it)
        private static IEnumerator Grab(System.Action<Texture2D> got)
        {
            yield return new WaitForEndOfFrame();
            got(ScreenCapture.CaptureScreenshotAsTexture());
        }

        // The screen rectangle (pixels, origin bottom-left) the world box's corners project to
        private Rect ScreenBoxOf(Bounds box)
        {
            float xMin = float.MaxValue, yMin = float.MaxValue, xMax = float.MinValue, yMax = float.MinValue;
            for (int corner = 0; corner < 8; corner++)
            {
                var p = box.center + Vector3.Scale(box.extents, new Vector3((corner & 1) == 0 ? -1 : 1, (corner & 2) == 0 ? -1 : 1, (corner & 4) == 0 ? -1 : 1));
                var s = Cam.WorldToScreenPoint(p);
                xMin = Mathf.Min(xMin, s.x); xMax = Mathf.Max(xMax, s.x); yMin = Mathf.Min(yMin, s.y); yMax = Mathf.Max(yMax, s.y);
            }
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        // The share of a 12 x 12 grid over `box` (on screen and above `floorY`, the peek card's top) whose colour differs between two captures
        private static float ChangedShare(Texture2D a, Texture2D b, Rect box, float floorY)
        {
            int changed = 0, seen = 0;
            for (int i = 0; i < 12; i++)
                for (int j = 0; j < 12; j++)
                {
                    int x = Mathf.RoundToInt(Mathf.Lerp(box.xMin, box.xMax, (i + 0.5f) / 12f));
                    int y = Mathf.RoundToInt(Mathf.Lerp(box.yMin, box.yMax, (j + 0.5f) / 12f));
                    if (x < 0 || y < floorY || x >= a.width || y >= a.height) continue;
                    seen++;
                    Color ca = a.GetPixel(x, y), cb = b.GetPixel(x, y);
                    if (Mathf.Abs(ca.r - cb.r) + Mathf.Abs(ca.g - cb.g) + Mathf.Abs(ca.b - cb.b) > 0.1f) changed++;
                }
            Assert.Greater(seen, 20, "precondition: most of the arch's screen box is on screen above the card");
            return changed / (float)seen;
        }
    }
}
