using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // place_in_ar's placement (_3.1 step 10B.1): the pure pose rule (ArPlacementRule) and the ONE owner (ArPlacementService) over a
    // ManualArWall and the REAL Framework model `default:azulejo_arch` (a real Resources load and instantiate, released through a counting
    // ScopedMediaSource, so "nothing left behind" is a count of live loads and children, not a promise).
    public class ArPlacementTests
    {
        private const float Tolerance = 1e-4f;
        private const string Arch = "default:azulejo_arch";

        // The Lamp's own authored facing (LivingRoom config): a tilted, turned wall, not an axis-aligned one
        private static readonly Quaternion LampFacing = Quaternion.Euler(342.31494f, 151.98160f, 8.91175f);

        private static ArPlacementRule.Input Input(Quaternion facing, Vector3? viewer = null, string scale = ArPlacementRule.ScaleRealSize,
            float wallScale = 1f) => new()
        {
            PoiPosition = new Vector3(-0.859f, -1.142f, -3.857f),
            PoiRotation = facing,
            Viewer = viewer,
            OffsetCm = 10f,
            ScaleMode = scale,
            HeightCm = 30f,
            MarkerMultiple = 3f,
            MarkerDiameter = 0.12f,
            WallScale = wallScale,
            ModelBounds = new Bounds(new Vector3(0.05f, 0.3f, -0.02f), new Vector3(0.4f, 0.6f, 0.1f)),
        };

        private static void AssertClose(Vector3 expected, Vector3 actual, string what) =>
            Assert.Less(Vector3.Distance(expected, actual), Tolerance, what + ": expected " + expected.ToString("F4") + " but was " + actual.ToString("F4"));

        // ---------------- the rule ----------------

        [Test]
        public void OutwardNormal_IsTheBackOfTheAuthoredFacing_FlippedOnlyWhenTheViewerStandsOnTheOtherSide()
        {
            AssertClose(Vector3.back, ArPlacementRule.OutwardNormal(Quaternion.identity, Vector3.zero, null), "no viewer: the facing's back");
            AssertClose(Vector3.back, ArPlacementRule.OutwardNormal(Quaternion.identity, Vector3.zero, new Vector3(0.3f, 0f, -2f)), "viewer in front");
            AssertClose(Vector3.forward, ArPlacementRule.OutwardNormal(Quaternion.identity, Vector3.zero, new Vector3(0.3f, 0f, 2f)), "viewer behind: flipped");
            var lamp = ArPlacementRule.OutwardNormal(LampFacing, Vector3.zero, null);
            AssertClose(-(LampFacing * Vector3.forward), lamp, "the Lamp's tilted facing");
            Assert.AreEqual(1f, lamp.magnitude, Tolerance, "a unit normal");
        }

        [Test]
        public void Place_PoiOnWall_PushesOutAlongTheNormalByTheOffset_AndPutsTheModelsBoundsCentreThere()
        {
            var input = Input(LampFacing);
            var pose = ArPlacementRule.Place(input);
            AssertClose(input.PoiPosition + pose.Normal * 0.1f, pose.Target, "10 cm out along the normal");
            Assert.AreEqual(0.1f, Vector3.Distance(input.PoiPosition, pose.Target), Tolerance);
            var centre = pose.LocalPosition + pose.LocalRotation * (input.ModelBounds.center * pose.LocalScale);
            AssertClose(pose.Target, centre, "the bounds centre (the preview's pivot) on the target");
        }

        [Test]
        public void Place_TheModelStandsUpright_AndShowsTheViewersSideTheFaceThePreviewShowsAtRest()
        {
            foreach (var facing in new[] { Quaternion.identity, LampFacing, Quaternion.Euler(10f, 250f, -5f) })
            {
                var pose = ArPlacementRule.Place(Input(facing));
                AssertClose(Vector3.up, pose.LocalRotation * Vector3.up, "upright for " + facing.eulerAngles);
                // - the preview's camera stands on the model's -Z at rest: that side faces out of the wall
                var shown = pose.LocalRotation * Vector3.back;
                var outward = Vector3.ProjectOnPlane(pose.Normal, Vector3.up).normalized;
                AssertClose(outward, shown, "the model's -Z along the normal's horizontal part for " + facing.eulerAngles);
            }
        }

        [Test]
        public void Place_APoiFacingStraightUp_FacesTheViewer_AndWithNoViewerStillStandsUpright()
        {
            var up = Quaternion.Euler(90f, 0f, 0f);
            var withViewer = ArPlacementRule.Place(Input(up, viewer: new Vector3(2f, 0f, -3.857f)));
            var target = withViewer.Target;
            var toViewer = Vector3.ProjectOnPlane(new Vector3(2f, 0f, -3.857f) - target, Vector3.up).normalized;
            AssertClose(toViewer, withViewer.LocalRotation * Vector3.back, "a floor/ceiling POI turns the model to the viewer");
            var alone = ArPlacementRule.Place(Input(up));
            AssertClose(Vector3.up, alone.LocalRotation * Vector3.up, "no viewer: still upright");
        }

        [Test]
        public void ScaleOf_RealSizeHeightAndMarkerMultiple_GiveTheHeightsTheyPromise()
        {
            const float modelHeight = 0.6f;
            Assert.AreEqual(1f, ArPlacementRule.ScaleOf(ArPlacementRule.ScaleRealSize, modelHeight, 30f, 3f, 0.12f, 1f), Tolerance, "real_size: its own metres");
            Assert.AreEqual(0.3f, modelHeight * ArPlacementRule.ScaleOf(ArPlacementRule.ScaleHeightCm, modelHeight, 30f, 3f, 0.12f, 1f), Tolerance, "height_cm 30: 0.3 m tall");
            Assert.AreEqual(0.36f, modelHeight * ArPlacementRule.ScaleOf(ArPlacementRule.ScaleMarkerMultiple, modelHeight, 30f, 3f, 0.12f, 1f), Tolerance, "3 x a 12 cm marker");
            Assert.AreEqual(1f, ArPlacementRule.ScaleOf("not_a_mode", modelHeight, 30f, 3f, 0.12f, 1f), Tolerance, "an unknown mode reads as real_size");
            float flat = ArPlacementRule.ScaleOf(ArPlacementRule.ScaleHeightCm, 0f, 30f, 3f, 0.12f, 1f);
            Assert.IsFalse(float.IsInfinity(flat) || float.IsNaN(flat), "a flat model never divides by zero");
        }

        [Test]
        public void Place_OnAScaledWallFrame_RealSizeAndHeightStayWorldMetres_TheMarkerMultipleFollowsTheMarkers()
        {
            const float wallScale = 2f;
            var real = ArPlacementRule.Place(Input(LampFacing, scale: ArPlacementRule.ScaleRealSize, wallScale: wallScale));
            Assert.AreEqual(1f, real.LocalScale * wallScale, Tolerance, "real_size: 1 in the world");
            var height = ArPlacementRule.Place(Input(LampFacing, scale: ArPlacementRule.ScaleHeightCm, wallScale: wallScale));
            Assert.AreEqual(0.3f, 0.6f * height.LocalScale * wallScale, Tolerance, "height_cm: 30 cm in the world");
            var marker = ArPlacementRule.Place(Input(LampFacing, scale: ArPlacementRule.ScaleMarkerMultiple, wallScale: wallScale));
            Assert.AreEqual(0.36f, 0.6f * marker.LocalScale, Tolerance, "marker_multiple: 3 markers tall in the frame the markers live in");
            Assert.AreEqual(0.1f, Vector3.Distance(Input(LampFacing).PoiPosition, real.Target) * wallScale, Tolerance, "the offset is world centimetres");
        }

        // ---------------- the owner, on the real Framework model ----------------

        private ManualArWall _wall;
        private ScopedMediaSource _media;
        private ArPlacementService _service;
        private int _changes;

        [SetUp]
        public void SetUp()
        {
            _wall = new ManualArWall { Viewer = new Vector3(0f, 0f, -2f), MarkerDiameter = 0.1f };
            _media = new ScopedMediaSource(new ResourcesMediaSource("") { FrameworkDefaults = CardMediaLibraryLookup.Framework });
            _service = new ArPlacementService(_wall, () => _media);
            _changes = 0;
            _service.Changed += () => _changes++;
        }

        [TearDown]
        public void TearDown()
        {
            _service.Dispose();
            _wall.Dispose();
        }

        private static ArPlacementRequest Request(string blockKey, string scale = ArPlacementRule.ScaleRealSize) => new()
        {
            Poi = new POIData { id = "lamp", position = new PositionData { x = 0.4f, y = 1.2f, z = 0f }, editor_rotation_deg = 20f },
            BlockKey = blockKey, ModelPath = Arch, OffsetCm = 15f, ScaleMode = scale, HeightCm = 50f, MarkerMultiple = 4f,
        };

        [Test]
        public void Place_PutsTheRealModelUnderTheWallFrame_AtThePoseTheRuleWorksOut_ForEveryScale()
        {
            foreach (var scale in new[] { ArPlacementRule.ScaleRealSize, ArPlacementRule.ScaleHeightCm, ArPlacementRule.ScaleMarkerMultiple })
            {
                var request = Request("block_1", scale);
                Assert.IsTrue(_service.Place(request), scale + ": placed");
                var model = _service.Placed;
                Assert.AreSame(_wall.Root, model.transform.parent, "a child of the wall's frame");
                var bounds = ArPlacementService.LocalBoundsOf(model);
                Assert.Greater(bounds.size.y, 0.01f, "the arch has a real height");
                var expected = ArPlacementRule.Place(new ArPlacementRule.Input
                {
                    PoiPosition = new Vector3(0.4f, 1.2f, 0f), PoiRotation = Quaternion.Euler(0f, 20f, 0f), Viewer = _wall.Viewer,
                    OffsetCm = 15f, ScaleMode = scale, HeightCm = 50f, MarkerMultiple = 4f, MarkerDiameter = 0.1f, WallScale = 1f, ModelBounds = bounds,
                });
                AssertClose(expected.LocalPosition, model.transform.localPosition, scale + ": position");
                Assert.Less(Quaternion.Angle(expected.LocalRotation, model.transform.localRotation), 0.01f, scale + ": rotation");
                Assert.AreEqual(expected.LocalScale, model.transform.localScale.x, Tolerance, scale + ": scale");
                // - the drawn height in the world is what the mode promises
                var world = model.GetComponentInChildren<Renderer>() != null ? WorldHeight(model) : 0f;
                float promised = scale == ArPlacementRule.ScaleHeightCm ? 0.5f : scale == ArPlacementRule.ScaleMarkerMultiple ? 0.4f : bounds.size.y;
                Assert.AreEqual(promised, world, 0.005f, scale + ": the model's height in the world");
            }
        }

        // The model's height in the world (it stands upright, so its world box's height is its own)
        private static float WorldHeight(GameObject model)
        {
            var renderers = model.GetComponentsInChildren<Renderer>();
            var box = renderers[0].bounds;
            foreach (var r in renderers) box.Encapsulate(r.bounds);
            return box.size.y;
        }

        [Test]
        public void Place_ASecondModelReplacesTheFirst_RemoveTakesItAway_AndNothingIsLeftBehind()
        {
            Assert.IsTrue(_service.Place(Request("block_1")));
            var first = _service.Placed;
            Assert.IsTrue(_service.Place(Request("block_2")));
            Assert.IsTrue(first == null, "the first model is destroyed (DestroyImmediate in Edit Mode)");
            Assert.AreEqual(1, _wall.Root.childCount, "ONE model in the world");
            Assert.IsTrue(_service.IsPlaced("lamp", "block_2"));
            Assert.IsFalse(_service.IsPlaced("lamp", "block_1"));
            Assert.AreEqual(1, _media.HeldCount, "the first model's load was given back");

            _service.Remove();
            Assert.AreEqual(0, _wall.Root.childCount, "nothing stands after Remove");
            Assert.AreEqual(0, _media.HeldCount, "every load given back");
            Assert.IsNull(_service.Placed);
            Assert.IsFalse(_service.IsPlaced("lamp", "block_2"));
            Assert.AreEqual(3, _changes, "two placements and one removal");
            _service.Remove();
            Assert.AreEqual(3, _changes, "removing nothing changes nothing");
        }

        [Test]
        public void Place_WhileTheWallIsNotLocalised_PlacesNothing_AndLosingOrFindingTheWallTellsTheBlocks()
        {
            _wall.IsLocalised = false;
            Assert.AreEqual(1, _changes, "losing the wall is a change the blocks redraw for");
            Assert.IsFalse(_service.CanPlace);
            Assert.IsFalse(_service.Place(Request("block_1")));
            Assert.IsNull(_service.Placed);
            Assert.AreEqual(0, _media.HeldCount, "nothing loaded");
            _wall.IsLocalised = true;
            Assert.IsTrue(_service.CanPlace);
            Assert.AreEqual(2, _changes);
        }

        [Test]
        public void Place_AMissingModelOrAPoiWithoutAPosition_PlacesNothing_AndKeepsWhatStands()
        {
            Assert.IsTrue(_service.Place(Request("block_1")));
            var missing = Request("block_2");
            missing.ModelPath = "models/not_there.glb";
            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("could not be loaded"));
            Assert.IsFalse(_service.Place(missing));
            Assert.IsTrue(_service.IsPlaced("lamp", "block_1"), "a failed placement keeps the model that stands");
            var nowhere = Request("block_3");
            nowhere.Poi.position = new PositionData { x = float.NaN, y = 0f, z = 0f };
            Assert.IsFalse(_service.Place(nowhere));
            Assert.IsTrue(_service.IsPlaced("lamp", "block_1"));
        }
    }
}
