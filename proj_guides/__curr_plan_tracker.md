# Facing sliders X/Y/Z + verified lock + auto-reveal (Orientation domain)

Baseline (2026-09-18, before changes): EditMode 743 passed / 0 failed, PlayMode 80 passed / 0 failed.
Final: EditMode 762 passed / 0 failed (+19 new), PlayMode 80 passed / 0 failed.

- [x] 1. PoiRotationResolver.IsSameOrientation; SyncPoiRotationFromScene compares orientation (not raw euler triples)
- [x] 2. New partial POIEditorToolWindow.MarkerSceneEdit.cs: ShouldBlockVerifiedRotationChange, ProcessMarkerTransformEdit, RevealPoiInSpecificMarkerTab
- [x] 3. New MarkerEditDetector.cs (pure "did this marker's pose change" tracker)
- [x] 4. HandleSceneGui + OnInspectorUpdate rewired through the new methods (rotation lock, inspector sync, reveal)
- [x] 5. DrawPositionTabs: three Facing sliders X/Y/Z, disabled when verified
- [x] 6. TogglePoiVerification: capture rig rotation on verify; dialog text mentions facing; CapturePositions skips rotation for verified POIs
- [x] 7. Help texts (EditRotationHelpBody, PositionSetupHelpBody) rewritten
- [x] 8. Scroll-to-POI hook in DrawSpecificMarkerOptions
- [x] 9. Tests (real rig, real window, real IMGUI) + AddPoi x/z copy tests
- [x] 10. 10-structure.md + _2.1 / _5.1 docs; final EditMode + PlayMode run
