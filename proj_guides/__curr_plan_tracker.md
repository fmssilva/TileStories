# Plan tracker -- POI Card 10B-pre (fit options, kind names) + 10B place_in_ar (started 2026-09-30)

Tree = green commit 5481dc5 (+ docs only). GATE: TestEvidence/Gate/gate_10B_game_view.png + gate_10B_poi_editor.png opened and read,
Oswald font unmodified, Editor.log 0 `error CS` / ExitCode 0.

## Design decisions (autonomous)
- yaw_safe: the camera distance FOLLOWS THE PITCH -- exact fit of every yaw at the current pitch (closed form per box corner);
  a fixed yaw-safe distance with the turntable's +-80 tilt equals the sphere (every box corner lies on the bounding sphere).
  Room scan at rest ~0.74 of the shorter side (python check), never clipped at any allowed turn.
- sphere keeps TargetFill 0.8; yaw_safe / at_rest fit box corners to FrameFill 0.9 (5 % margin each side).
- Choice defaults: BlockFieldDefinition.ChoiceDefault + LibraryDefault, BlockKindSetting.field_defaults, BlockLibraryRule.Choice.
- AR: pure ArPlacementRule; ICardArPlacement / ArPlacementService over IArPlacementStage (ArPlacementStage MonoBehaviour) and
  IArWall (WallArSurface over WallSession + IWallTracker); the model shows the viewer the side the card preview shows at rest.

## Steps
- [x] P1  10B-pre.1 Fit choice -- yaw_safe / sphere / at_rest + visible (developer, 2026-10-01; scan 62 % accepted), Choice defaults, Editor rows; EditMode 1514/1514, PlayMode targeted 31/31; Default Fit row recapture pending (_5.1)
- [x] P2  10B-pre.2 kind display names 3D Model / 360 Panorama -- HeadingFor(kind), guard test; EditMode 1514/1515 (typing flake, 5/5 alone), PlayMode gallery 342/342
- [x] B1  10B.1 ArPlacementRule + ICardArPlacement / ArPlacementService (no separate stage: the service instantiates) + WallArSurface / ManualArWall; ArPlacementTests 10/10
- [x] B2  10B.2 place_in_ar kind + view + strings + Card Content row (drawn from the definition); EditMode 1526/1526
- [x] B3  10B.3 Phase A gallery 3/3 + Phase B LivingRoom 2/2 (block_62 fixture +35/-0 per copy); long-label wrap fixed from the capture
- [x] B4  10B.4 EditMode 1526/1526 + PlayMode 740/740; Editor captures at 620 pt (Default Scale Mode label fixed); docs _3.1 / 10-structure / _5.1
