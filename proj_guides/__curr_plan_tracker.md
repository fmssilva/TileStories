# Plan tracker -- POI Card 10A.3-fix (x3) + 10A.4 panorama_360 (started 2026-09-30)

Baseline (tree = commit e5822e5 + __AI_worker.md only): EditMode 1483/1483 green. Full PlayMode: see row P0.
GATE: Game-view + POI Editor captures taken and opened (TileStories/TestEvidence/Gate/), tree compiles, Oswald font unmodified.

## Design decisions (autonomous)
- ONE shared base view `PreviewBlockView` (pointer / pinch / slot / teaser / takeover / visibility gate), the model view and the
  new panorama view are thin subclasses. Reason: 10A.3-fix.1 and every later fix must land once, not twice.
  USS classes card-model3d* -> card-preview* (same rules, neutral name).
- Sheet stop reaches blocks through IBlockHost.Stop + pure SheetStopRule.RevealsBlocks(stop) (Half/Full only).
- Fit: pure ModelFitRule (bounding sphere of the renderers' bound corners, distance = r / sin(atan(0.8 * tanHalfFovOfShorterAxis))).
- Panorama sphere: pure PanoramaSphereRule (vertices = EquirectRule.DirectionOf, uv = (u,v), inward winding), unlit URP material
  asset in Resources (a Shader.Find would be stripped from a build).
- Gyro: pure PanoramaGyroRule (attitude quaternion -> yaw/pitch, anchored to the first reading) + DeviceAttitude (reads the real
  Input System AttitudeSensor.current, enables it). Drag is the fallback wherever no sensor exists.
- panorama_360 fields: panorama (Asset/Panorama*), fallback (Asset/Image), start_heading (Number 0..360), title (LocalizedText);
  looks (variants) drag / gyro; display modes inline + takeover.

## Steps
- [x] P0  opening full PlayMode baseline: EditMode 1483/1483; PlayMode 701/702 (OrientationClusterIntegrationTests, stray MainCamera from an earlier fixture; fixed in F1)
- [x] F1  10A.3-fix.1 auto-spin gated on sheet stop + viewport (IBlockHost.Stop, SheetStopRule.RevealsBlocks, Phase B on the Lamp) -- EditMode 1484, targeted PlayMode 14/14
- [x] F2  10A.3-fix.2 bounding-sphere fit (ModelFitRule, stage), pixel test after a 90 degree drag (arch + room scan), header Full recapture, delete [10A.3-followup] -- EditMode 1487, targeted PlayMode 22/22; room scan reads small by design (note in _3.1)
- [x] F3  10A.3-fix.3 capture Header row (model look, 620 pt) + Model 3D rows; clear _5.1 pending entry
- [x] A1  10A.4.1 PreviewBlockView extraction + stage panorama (PanoramaSphereRule, material, handle), release guarantees
- [x] A2  10A.4.2 panorama_360 kind + PanoramaBlockView (drag / pinch / takeover / gyro), CardStrings en+pt, Card Content row, Lamp fixture
- [x] A3  10A.4.3 tests (pure, Phase A gallery entries, Phase B Lamp, gyro with a real Input System test device, round trip)
- [x] A4  10A.4.4 full EditMode 1507/1507 + full PlayMode 732/732 (end to end, after the last fix), captures checked (4.5 questions), docs: _3.1, 10-structure.md, _5.1
