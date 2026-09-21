# Current plan: Orientation > Test sub-section rework  (DONE 2026-09-21)

Baseline (before changes): compile ok, EditMode 806/806, PlayMode 80/80.
Final: zero error CS, Editor DLLs rebuilt, EditMode 808/808 (806 + 2 new), PlayMode 80/80.

- [x] 1. Rename UI label "Edit-Mode Preview" -> "Scene-Mode Preview" (JSON key unchanged).
- [x] 2. ApplyOrientationPreview ticks Label/Badge MarkerChildOrientation; RestoreRigRotationsFromConfig resets them.
- [x] 3. Test: OrientationEditModePreviewTests.ApplyOrientationPreview_TicksLabelAndBadge_AndRestoreResetsThem (real POI_Marker prefab).
- [x] 4. Three collapsed foldouts: How to Scene / Playmode / Device Test, ordered by config field.
- [x] 5. Test: OrientationEditorRoundTripTests.TestGuides_ExistAsciiCoverEveryFieldAndOption_AndStartCollapsed.
- [x] 6. Docs: _2.1 (sections 0, 8, 9, 13), _5.1, 10-structure.md.
- [x] 7. Compile + EditMode + PlayMode zero failures.

Open: eyeball the foldout indentation once in the real window (no screenshot pass done).
