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
                // - the arch the previous scale placed still stands (its button would read Remove from room): this scale places its own
                Placement.Remove();
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
                Assert.AreEqual("Remove from room", view.ButtonLabel.text, scale + ": placed: the same button now removes");
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

            // - the card rises: still placed, the button reads Remove from room; a real tap on it takes the arch away
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            var view = View();
            Assert.IsTrue(view.IsPlaced, "re-opening keeps it placed");
            Assert.AreEqual("Remove from room", view.ButtonLabel.text, "the button removes");
            Sheet.Stack.Scroll.ScrollTo(view.Button);
            yield return CardTestInput.Settle(0.2f);
            yield return Capture("PlaceInAr_Lamp_PlacedButton");
            yield return ScrollAndTap(view.Button);
            Assert.IsNull(Placement.Placed, "Remove took it away");
            Assert.AreEqual(0, PlacedInTheWorld(), "nothing in the world");
            Assert.IsFalse(view.IsPlaced, "the first state is back");
            Assert.AreEqual("See it here in 3D", view.ButtonLabel.text);

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
                Assert.AreEqual("See it here in 3D", View().ButtonLabel.text, "only the block that placed it reads Remove from room");
                Assert.AreEqual("Remove from room", View("block_62b").ButtonLabel.text);

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

        // ---------------- Keep Model On Switch (15.2.4), Phase B ----------------

        // The room scan, loaded BY PATH from the wall's media folder: unlike a `default:` arch (a static library reference, never counted) it is
        // reference-counted by the media source, so a load the owner forgot to give back shows
        private const string Scan = "models/146267-LivingRoom2-tex.glb";
        private const string ScanBlockKey = "block_62c";

        // A second Place In AR block, in memory, on the room scan at 40 cm tall; Rebind shows it on the open card
        private BlockInstanceData AddScanBlock()
        {
            var block = new BlockInstanceData { key = ScanBlockKey, kind = BuiltInBlocks.PlaceInArKind, variant = BuiltInBlocks.PlaceInArButton };
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.PlaceInArModelField, asset = Scan });
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.PlaceInArScaleField, value = ArPlacementRule.ScaleHeightCm });
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.PlaceInArHeightField, number = 40f });
            Lamp.card.blocks.Add(block);
            return block;
        }

        // Place `blockKey`'s model with a real tap, then switch to another point with a real tap on the first neighbour of The Lamp's With
        // Neighbours block (the same bus every marker tap raises); hands back the id of the point the card moved to
        private IEnumerator PlaceThenSwitchPoint(string blockKey, System.Action<string> switched)
        {
            yield return OpenFull("lamp");
            yield return ScrollAndTap(View(blockKey).Button);
            Assert.IsTrue(Placement.IsPlaced("lamp", blockKey), "precondition: a real tap placed " + blockKey);
            Assert.AreEqual(1, PlacedInTheWorld(), "precondition: one model in the world");
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            var neighbours = Sheet.Stack.BoundViews.OfType<ShowOnWallBlockView>().First(v => v.Neighbours.Count > 0);
            string target = neighbours.Neighbours[0].PoiId;
            Assert.AreNotEqual("lamp", target, "precondition: the neighbour is another point");
            yield return ScrollAndTap(neighbours.Neighbours[0].Button);
            Assert.AreEqual(target, SelectionEventBus.CurrentPoiId, "a real tap selected another point");
            Assert.AreEqual(target, Card.ShownPoiId, "the card moved to it");
            switched(target);
        }

        private static void SetFlag(BlockInstanceData block, string key, bool value)
        {
            block.fields.RemoveAll(f => f.key == key);
            block.fields.Add(new BlockFieldValue { key = key, flag = value });
        }

        private void SetWallKeepDefault(bool on)
        {
            var row = Session.CardSettings.kinds.Find(k => k.kind == BuiltInBlocks.PlaceInArKind);
            if (row == null) Session.CardSettings.kinds.Add(row = new BlockKindSetting { kind = BuiltInBlocks.PlaceInArKind });
            row.field_defaults.RemoveAll(d => d.key == BuiltInBlocks.PlaceInArKeepOnSwitchField);
            row.field_defaults.Add(new BlockFieldDefault { key = BuiltInBlocks.PlaceInArKeepOnSwitchField, value = on ? BlockLibraryRule.FlagTrue : BlockLibraryRule.FlagFalse });
        }

        [UnityTest]
        public IEnumerator KeepModelOnSwitch_Off_ARealTapOnAnotherPoint_RemovesTheModel_NothingLeaksAndTheFirstStateIsBack()
        {
            var keep = BuiltInBlocks.PlaceInAr.Field(BuiltInBlocks.PlaceInArKeepOnSwitchField);
            Assert.IsFalse(BlockLibraryRule.Flag(Session.CardSettings, BuiltInBlocks.PlaceInAr, keep, new BlockFieldReader(LampBlock, null, null)),
                "precondition: The Lamp's block and the shipped wall leave it off");
            var scan = AddScanBlock();
            try
            {
                Card.Rebind();
                yield return CardTestInput.Settle();
                string target = null;
                yield return PlaceThenSwitchPoint(ScanBlockKey, t => target = t);
                Assert.IsNull(Placement.Placed, "the model went with the point it was placed for (now showing " + target + ")");
                Assert.AreEqual(0, PlacedInTheWorld(), "nothing stands in the world");
                Assert.AreEqual(0, Card.Media.RefCount(Scan), "its load was given back: no leak");
                Assert.IsTrue(Sheet.IsOpen, "the other point's card is open");

                // - back on The Lamp: the first state (the button places again)
                SelectionEventBus.Select("lamp");
                yield return CardTestInput.Settle();
                Assert.AreEqual("See it here in 3D", View(ScanBlockKey).ButtonLabel.text, "the first state is back on The Lamp's card");
                Assert.IsFalse(View(ScanBlockKey).IsPlaced);
            }
            finally { Lamp.card.blocks.Remove(scan); }
        }

        [UnityTest]
        public IEnumerator KeepModelOnSwitch_OnInTheBlockLibrary_TheModelSurvivesAnotherPointAndAClosedCard_UntilARealTapOnRemoveFromRoom()
        {
            SetWallKeepDefault(true);
            var scan = AddScanBlock();
            try
            {
                Card.Rebind();
                yield return CardTestInput.Settle();
                string target = null;
                yield return PlaceThenSwitchPoint(ScanBlockKey, t => target = t);
                Assert.IsTrue(Placement.IsPlaced("lamp", ScanBlockKey), "kept through another point (now showing " + target + ")");
                Assert.AreEqual(1, PlacedInTheWorld(), "still ONE model in the world");
                Assert.AreEqual(1, Card.Media.RefCount(Scan), "its load is still held, once");

                // - the card closes (a real tap on its X): kept too
                yield return CardTestInput.Tap(Sheet.CloseButton.panel, Sheet.CloseButton.worldBound.center);
                yield return CardTestInput.Settle();
                Assert.IsFalse(Sheet.IsOpen, "the card closed");
                Assert.IsTrue(Placement.IsPlaced("lamp", ScanBlockKey), "kept through a closed card");
                Assert.AreEqual(1, PlacedInTheWorld());

                // - The Lamp's card again: that block's button reads Remove from room, and a real tap takes the model away
                yield return OpenFull("lamp");
                Assert.AreEqual("Remove from room", View(ScanBlockKey).ButtonLabel.text, "the kept model is this block's: its button removes it");
                Assert.AreEqual("See it here in 3D", View().ButtonLabel.text, "the Lamp's other Place In AR block still places");
                yield return ScrollAndTap(View(ScanBlockKey).Button);
                Assert.IsNull(Placement.Placed, "Remove took it away");
                Assert.AreEqual(0, PlacedInTheWorld());
                // - The Lamp's own 3D-room block (block_59) loads the same file while its card is open: close the card, then every load is the owner's
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
                Assert.AreEqual(0, Card.Media.RefCount(Scan), "every load given back, the placed model's included");
            }
            finally { Lamp.card.blocks.Remove(scan); }
        }

        [UnityTest]
        public IEnumerator KeepModelOnSwitch_TheBlocksOwnTickWinsOverTheLibrarysDefault_InBothDirections()
        {
            string target = null;
            // - Library ON, the block unticked: the switch removes the arch
            SetWallKeepDefault(true);
            SetFlag(LampBlock, BuiltInBlocks.PlaceInArKeepOnSwitchField, false);
            yield return PlaceThenSwitchPoint("block_62", t => target = t);
            Assert.IsNull(Placement.Placed, "the block's own off wins: the arch went");
            Assert.AreEqual(0, PlacedInTheWorld());
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();

            // - Library OFF, the block ticked: it stays
            SetWallKeepDefault(false);
            SetFlag(LampBlock, BuiltInBlocks.PlaceInArKeepOnSwitchField, true);
            yield return PlaceThenSwitchPoint("block_62", t => target = t);
            Assert.IsTrue(Placement.IsPlaced("lamp", "block_62"), "the block's own tick wins: the arch stays (now showing " + target + ")");
            Assert.AreEqual(1, PlacedInTheWorld());
            Placement.Remove();
            // - Play Mode destroys at the end of the frame
            yield return CardTestInput.Settle();
            Assert.AreEqual(0, PlacedInTheWorld(), "Remove takes a kept arch away");
        }

        // 15.2.3 Phase B on the REAL scene, real taps, in both languages: the button places and reads Remove from room, a status line says where the
        // model is, the card (scrolled well down when the visitor tapped) is at its TOP at the peek, and a tap on the same button brings the first
        // state back. The words come from the card's strings table; the English ones are also asserted word for word, the Portuguese ones for
        // their accents
        [UnityTest]
        public IEnumerator TheLamp_Placed_TheButtonReadsRemoveFromRoom_AStatusLineShows_TheCardScrollsToTheTopAtPeek_RemoveBringsTheFirstStateBack_InEnglishAndPortuguese()
        {
            foreach (string language in new[] { "en", "pt" })
            {
                SelectionEventBus.Clear();
                Session.CardSettings.languages = new System.Collections.Generic.List<string> { language, "en" };
                yield return OpenFull("lamp");
                var strings = new CardStrings(Card.StringTable.Entries(), Card.StringSources.Entries(), Session.CardSettings.strings, language, language);
                string placeWords = strings.Get(CardStrings.Keys.PlaceInArButton), removeWords = strings.Get(CardStrings.Keys.PlaceInArRemove), placedLine = strings.Get(CardStrings.Keys.PlaceInArPlaced);
                if (language == "en")
                {
                    Assert.AreEqual("See it here in 3D", placeWords);
                    Assert.AreEqual("Remove from room", removeWords);
                    Assert.AreEqual("Placed by the wall", placedLine);
                }
                else
                {
                    StringAssert.Contains("\u00E0", placedLine, "Portuguese keeps its accent: Colocado junto \u00E0 parede");
                    Assert.AreNotEqual("Remove from room", removeWords, "the Portuguese card has its own words");
                }

                var view = View();
                Assert.AreEqual(placeWords, view.ButtonLabel.text, language + ": the first state");
                Assert.IsFalse(CardTestInput.IsShown(view.Note, view.Root), language + ": no line while nothing stands");

                // - scrolled well down, as a visitor who read down to the button is: a real tap places
                Sheet.Stack.Scroll.ScrollTo(view.Button);
                yield return CardTestInput.Settle(0.2f);
                float scrolledBefore = Sheet.Stack.Scroll.scrollOffset.y;
                Assert.Greater(scrolledBefore, 1000f, language + ": precondition: the stack is scrolled far down (" + scrolledBefore + ")");
                yield return CardTestInput.Tap(view.Button.panel, view.Button.worldBound.center);
                yield return CardTestInput.Settle(0.3f);
                Assert.IsNotNull(Placement.Placed, language + ": a real tap placed the arch");
                Assert.IsTrue(view.IsPlaced);
                Assert.AreEqual(removeWords, view.ButtonLabel.text, language + ": the same button now reads Remove from room");
                Assert.IsTrue(CardTestInput.IsShown(view.Note, view.Root), language + ": the status line shows");
                Assert.AreEqual(placedLine, view.Note.text);
                Assert.AreEqual(SheetStopRule.Stop.Peek, Sheet.Stop, language + ": the card lowered to its peek");
                Assert.AreEqual(0f, Sheet.Stack.Scroll.scrollOffset.y, 0.5f, language + ": scrolled to the top first (was " + scrolledBefore + ")");
                yield return Capture("PlaceInAr_Lamp_PlacedPeek_" + language);

                // - the card rises: the placed button, in its quiet look, reads and removes
                Sheet.SetStop(SheetStopRule.Stop.Full);
                yield return CardTestInput.Settle();
                Sheet.Stack.Scroll.ScrollTo(view.Button);
                yield return CardTestInput.Settle(0.2f);
                Assert.IsTrue(CardTestInput.IsShown(view.Note, view.Root), language + ": the line is there again at Full");
                yield return Capture("PlaceInAr_Lamp_PlacedFull_" + language);
                yield return CardTestInput.Tap(view.Button.panel, view.Button.worldBound.center);
                yield return CardTestInput.Settle();
                Assert.IsNull(Placement.Placed, language + ": Remove took it away");
                Assert.AreEqual(0, PlacedInTheWorld(), language + ": nothing in the world");
                Assert.IsFalse(view.IsPlaced);
                Assert.AreEqual(placeWords, view.ButtonLabel.text, language + ": the first state is back");
                Assert.IsFalse(CardTestInput.IsShown(view.Note, view.Root), language + ": the status line is gone");
                Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, language + ": Remove does not move the card");
            }
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
