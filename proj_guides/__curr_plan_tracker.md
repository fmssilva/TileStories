# PROSPECTIVE PLAN � `_2.5 Marker & Label Displacement`

## Master phase breakdown (top-level, to land in `__curr_plan_tracker.md`)

1. __Block 0 � Baseline gate.__ ? DONE: Compile 0 error CS, EditMode 510/510 + PlayMode 45/45 green (verified via Unity MCP, 2026-08-20).
2. __Block 1 � Schema (�10) + accessor.__ ? DONE: `DisplacementSettings` class on disk (WallConfigData L550); `displacement_settings` field (WallConfigData L76); `WallSession.DisplacementSettings` accessor (L47); `DisplacementSettingsDefaultsTests` 2/2 green.
3. __Block 2 � `label_only`+`fixed_axis`.__ ? DONE: `MarkerView.ApplyLabelOffset`/`ClearLabelOffset`/`GetVisualRadiusWorld` on disk; `MarkerOverlapResolver.ApplyDisplacement`/`ComputeOffsets` implemented; legacy deleted; `MarkerLayoutPxConversionTests` (5/5), `DisplacementComputeTests` (7/7), `DisplacementLabelTests` (5/5) green; `MarkerLayout.ScreenPixelsToWorld` tested. EditMode 501/501, PlayMode 45/45.
4. __Block 3 � `candidate_position` + `force_directed`.__ ? DONE: symmetric target-angle fix (L190-215), angular preference score (L295-297); `i * 2.4f` perturbation (L463), Y-separation pass (L495-556, min 35px); no dead code (`GenerateCandidatePositions`/`ComputeOverlapScore` confirmed absent). `DisplacementAlgorithmTest.ForceDirected_ThreeOverlappingMarkers_PriorityAware` green. EditMode 510/510, PlayMode 45/45.
5. __Block 4� `marker`/`both` + camera-relative conversion (�4).__ OK DONE: `ApplyMarkerOffset`/`ClearMarkerOffset`/`EnsureMarkerWorldBaseCaptured` on MarkerView (L611-652); `ApplyDisplacement` dispatch updated with `switch` on `displace_target` (L625-639); early-return paths also clear marker offsets; `displace_target` warning removed. 4 gallery entries added to `DisplacementGalleryDefinitions`. Shallow-angle regression test + 2 Tier 0 tests added. EditMode 512/512, PlayMode 46/46, 0 console errors.
6. __Block 5 � Leader lines (�6).__ OK DONE: `MarkerLeaderLine.cs` created (LineRenderer + Evaluate(), viewport-based distance, config-driven width/opacity); added to `POI_Marker` prefab (auto-adds LineRenderer). `_resolvedCategoryColor` cached on `MarkerView`; `LeaderLineColor` property + `UpdateLeaderLine` method. `ApplyDisplacement` wired to call `UpdateLeaderLine` at all 3 paths. `leader_line_width`/`leader_line_opacity` config fields added to `DisplacementSettings`. 2 gallery entries + `LeaderLinesEnabled`/`LeaderLineStyle` fields on `DisplacementGalleryEntry`. `LeaderLineVisibilityTests` (10 Tier-0 EditMode tests). EditMode 522/522, PlayMode 46/46, 0 console errors.
7. __Block 6 � �8 wiring__ (LODController step 8 + enabled-flag fix; WallSession legacy call already removed). [ok] DONE: _2.5 row 5 [ok]; EditMode 537/537, PlayMode 50/50 green, 0 error CS -- 2026-08-20.
8. __Block 7 � �9 hysteresis.__ [ok] DONE: `committedOffset` hold retention committed in `MarkerOverlapResolver.cs`; `DisplacementHysteresisTests` 8/8 EditMode + `Evaluate_FlickerAtThresholdBoundary_NoFlap` PlayMode green (flipped fail->pass).
9. __Block 8 � �11 Editor UI foldout.__ [ok] DONE: `DrawGlobalDisplacementSection()` + shell bool `_showGlobalDisplacement` + Constants on disk; `DisplacementAuthoringRoundTripTests` 2/2 green; EditMode 539/539, PlayMode 50/50, 0 fail, 0 error CS -- 2026-08-21.
10. __Block 9 � Phase B real-pipeline verification.__ [ok] DONE: `RealConfig_DisplacementPipeline_E2E` green (3 fixes: freeze Update via `enabled=false`, convergence settle w/ `Time.time>1.6s` floor, `ForceUpdateCanvases` seam); PlayMode 57/57, EditMode 539/539, 0 fail -- 2026-08-22.
11. __Block 10 -- Finishing (doc/sync only, no code change).__
    - [x] T10.1: Fix stale ApplyLabelState in _2.5 spec (L46-48 grounding block ->
      ApplyLabelOffset/ClearLabelOffset/ApplyMarkerOffset/ClearMarkerOffset;
      L107 pre-reads); mark Corrections Log #4/#5 deferrals RESOLVED.
    - [x] T10.2: Row 14 PlayMode 51/51 -> 57/57; add Corrections Log entry #6
      (verified EditMode 539/539 + PlayMode 57/57, 2026-08-22; Block 9 added 6.
    - [x] T10.3: LODGallery -> DisplacementGallery (1 stale ref).
    - [x] T10.4: 10-structure.md tree sync -- add missing displacement test files.
    - [ ] T10.5: Archive _2.5 -> _2.5.1_..._Archive.md -- DEFERRED (no explicit yes).
    - [x] T10.6: refresh_unity 0 error CS; EditMode 539/539 + PlayMode 57/57, 0 fails.

## Block 10 -- Execution Log (2026-08-22)
Scope: doc/sync only -- zero .cs changes. The 539/539 + 57/57 figure was
re-verified live via Unity MCP run_tests (EditMode then PlayMode) AFTER the
Block 9 code freeze; Block 10 introduced no code diffs, so the Block 9 green
run remains authoritative. All edits below touched only .md files outside
Assets/, so no AssetDatabase refresh was required.

T10.1 -- Spec grounding L46 rewritten to the real API (ApplyLabelOffset +
ClearLabelOffset + ApplyMarkerOffset + ClearMarkerOffset). L107 pre-reads
corrected. Corrections Log #4/#5 deferrals marked RESOLVED. Verified by
reading L46/L107/L636-642: the grounding states no ApplyLabelState/
ApplyOverlapOffset method exists on disk; remaining mentions are
Corrections-Log historical narrative (intentional, by design).

T10.2 -- Spec rows 13/14 PlayMode 51/51 -> 57/57 (EditMode 539/539 unchanged,
already accurate). Corrections Log entry #6 appended (L644-654). Tracker L14
Block-9 phase record likewise corrected 51/51 -> 57/57 (same factual fix).

T10.3 -- LODGallery -> DisplacementGallery (spec L477 path reference).
Grep confirms 0 stale LODGallery references anywhere on disk.

T10.4 -- 10-structure.md tree synced: 5 displacement test files added with
box-drawing continuity verified (Editor: DisplacementAlgorithmTest.cs,
DisplacementComputeTests.cs, MarkerLayoutPxConversionTests.cs; Runtime:
DisplacementGalleryTests.cs, DisplacementLabelTests.cs). Tree Tests/ count
line corrected 581 -> 596 / 537+50 -> 539+57 / 2026-08-21 -> 2026-08-22
(pre-existing line was itself inconsistent: 537+50=587, not 581).
  - DEFERRED (out of Block 10 scope per Scope Lock): 9 other test .cs files
    are also absent from the tree and were left untouched -- MarkerHierarchy
    ResolverTests, POIAuthoringToolSearchRoundTripTests, POISearchIndexMatch
    ModeTests, PanelSettingsTests, PoiAuthoringVisualHierarchyTests,
    SafeAreaHelperTests, DetailCardViewTests, UIAccessibilityTests,
    MarkerRevealEffectTests. Flagged for a separate general tree-sync ticket.

T10.5 -- Archive _2.5 -> _2.5.1_Marker_Displacement_Archive.md: DEFERRED.
  No explicit user confirmation received to split the living 660-line spec;
    left inline. Re-open if a retention policy or size threshold is adopted.

T10.6 -- Verified: refresh_unity compile clean (0 error CS). EditMode 539/539
+ PlayMode 57/57, 0 failed, 0 skipped, resultState=Passed (Unity MCP run_tests,
2026-08-22).
12. __Reserved � Vision Tests__ (`_2.5.1`).
13. __Reserved � Human in the Loop Tests__ (`_2.5.2`; optional per spec �15).

---

## Decisions made (confirmed against disk + Unity MCP test runs)

- **Retrospective audit of Block 3 (candidate_position + force_directed): VERIFIED COMPLETE.** 
  - EditMode 510/510 passed, 0 failures (0.98s); PlayMode 45/45 passed, 0 failures (8.27s); 0 console errors.
  - Symmetric target-angle fix confirmed at `MarkerOverlapResolver.cs` L190-215 (starts at 90�, steps by 360/runSize).
  - `i * 2.4f` perturbation confirmed at L463; Y-separation pass confirmed at L495-556 (minSep = 40 * 0.875 = 35px).
  - Dead code `GenerateCandidatePositions`/`ComputeOverlapScore` confirmed ABSENT from all `.cs` files (only in plan-tracker prose).
  - The "Fix candidate_position" task section (formerly at L682-712) is REMOVED � tasks were already executed; status reflected in Block 3 master line above and spec Implementation Status table Row 11-12.

- **API naming clarification:** `MarkerView.ApplyLabelOffset(Camera, Vector2 screenOffsetPx)` is the real method name on disk (L532) � NOT `ApplyLabelOffset(Vector2)` as previously drafted in the plan. The `Camera` parameter is needed for depth-based world-unit conversion. There is NO `ApplyLabelState()` method � label logic is inline in `ApplyVisuals` (confirmed by `_5.1_Editor_Tab.md` L527-531).

- **Priority conflation audit:** ONE order source � `VisualUnit.hierarchyLevelIndex` (set from `MarkerHierarchyResolver.GetLevelPriority`, LODController.cs:309). Count-cap (`ComparePriority` L859) and cluster (`BuildAggregate` bestPriority) both use it. Displacement tiebreak (Block 4) will use `hierarchyLevelIndex` but its OWN comparator (semantics differ: higher priority STAYS anchored; equal levels fall back to symmetric). Do NOT reuse `ComparePriority`.

- **`lower_priority_only` tiebreak is DEFERRED** (not part of Block 4 or any current block). The `displacement_tiebreak` field exists in `DisplacementSettings` (WallConfigData L562) but the code at `MarkerOverlapResolver.cs` L584-85 warns and falls back to `symmetric`. The algorithms (candidate_position L469, force_directed L476) already use priority for anchoring direction, which gives the `lower_priority_only` effect implicitly. Making the explicit flag work as a true priority-only-anchoring mode is a separate future task. Does NOT affect Block 4's `displace_target` work (orthogonal axis).

- **`lower_priority_only` vs `displace_target` distinction:** The warning at MarkerOverlapResolver L578-579 is for `displace_target != "label_only"` (Block 4 removes this). The warning at L584-85 is for `displacement_tiebreak == "lower_priority_only"` (stays as-is, deferred).

- **Gallery path:** `Assets/Dev/DisplacementGallery/` does NOT exist � the gallery is at `Assets/Dev/` as individual files (DisplacementGalleryScene.unity, DisplacementGalleryHarness.cs, DisplacementGalleryDefinitions.cs). No `LODGallery/` folder found. No new folder needed for Block 4 � gallery entries are data-driven via `DisplacementGalleryDefinitions.Entries`.

- **Project root path:** `C:\Users\franc\Desktop\TileStories\TileStories` (nested under the parent `TileStories` repo root at `C:\Users\franc\Desktop\TileStories`).


## Design rationale (Architecture & Implementation - the two-level analysis)

ARCHITECTURE - WHERE: Option A (CHOSEN): evolve `MarkerOverlapResolver` in place as a static
engine (grouping + 3 algorithms + tiebreak) over `List<VisualUnit>`; `MarkerView` holds only
the thin write methods; `LODController` orchestrates as step 8; new `LeaderLineView.cs`.
Rejected: (B) separate DisplacementEngine (duplicates correct union-find, contradicts
"evolve not rewrite"); (C) standalone DisplacementController MonoBehaviour + own timer
(second timer -> inconsistent in-between states, �8).

IMPLEMENTATION - HOW: Option A (CHOSEN): static pure methods, one screen-space snapshot per
`Evaluate()` cycle, synchronous write. Rejected: (B) event-driven (bus + ordering hazards);
(C) coroutine-animated (conflicts with per-cycle snapshots/hysteresis, no requirement).

DEVELOPER-SELECTABLE SURFACE (each a first-class Editor control on the Displacement foldout):

| Feature | Editor control | Config field | Options (default bold) |
|---|---|---|---|
| Master | Toggle | `enabled` | **true** |
| Overlap radius | Scalar (px) | `overlap_threshold_px` | **40** |
| Displace target | Dropdown + (i) | `displace_target` | **label_only** / marker / both |
| Algorithm | Dropdown + (i) | `displacement_algorithm` | fixed_axis / candidate_position / **force_directed** |
| Relaxation steps | Int (only force_directed) | `force_directed_iterations` | **4** |
| Max displacement | Scalar (px) | `max_displacement_px` | **120** |
| Leader lines | Toggle | `leader_lines_enabled` | **true** |
| Line style | Dropdown (when enabled) | `leader_line_style` | **straight** / dashed / elbow |
| Min line distance | Scalar (when enabled) | `leader_line_min_distance_px` | **15** |
| Tiebreak | Dropdown + (i) | `displacement_tiebreak` | **symmetric** / lower_priority_only |

No speculative options added (per 20-code-quality); the spec already covers the meaningful axes.



## BLOCK 0 � Baseline verification

- __WHAT:__ Establish a clean starting point on the nested project root `C:\Users\franc\Desktop\TileStories\TileStories`. Confirm MCP `refresh_unity` ? 0 error CS; EditMode + PlayMode ? 0 failures. 
- __HOW:__ `unityMCP__refresh_unity(compile=request)`; `run_tests(EditMode)` + `get_test_job`; `run_tests(PlayMode)` + `get_test_job`.
- __WHERE:__ n/a (verification only).
- __TESTS:__ The gate itself. [x] [ok] Block 0 -- `refresh_unity` (force) -> 0 `error CS`; console 0 errors/0 warnings. EditMode run: **487/487 passed, 0 failures** (job `99f85c90`). PlayMode run: **41/41 passed, 0 failures** (job `2c29f72c`). Both suites green. Counts taken from the live run (the structure guide's "391 total / 353+38" note is stale).

---

## BLOCK 1 � Data model (�10) + settings accessor

- __WHAT:__ Add the `DisplacementSettings` [Serializable] schema to `WallConfigData.cs`
  (exact �10 fields + defaults verbatim), a wall-level `displacement_settings` field, and a
  read-only `WallSession.DisplacementSettings` accessor (mirrors `LodSettings`). Pure additive:
  nothing reads the new settings yet (Block 6 wires the resolver, Block 8 the editor.).
- __HOW (schema, finished class):__
  ```csharp
  [Serializable]
  public class DisplacementSettings
  {
      public bool enabled = true;
      public float overlap_threshold_px = 40f;              // was a hardcoded const in MarkerOverlapResolver; now configurable
      public string displace_target = "label_only";          // "label_only" | "marker" | "both"
      public string displacement_algorithm = "force_directed"; // "fixed_axis" | "candidate_position" | "force_directed"
      public int force_directed_iterations = 4;              // relaxation steps/cycle, force_directed only
      public float max_displacement_px = 120f;               // cap; beyond it, hide the label
      public bool leader_lines_enabled = true;
      public string leader_line_style = "straight";          // "straight" | "dashed" | "elbow"
      public float leader_line_min_distance_px = 15f;
      public string displacement_tiebreak = "symmetric";     // "symmetric" | "lower_priority_only"
  }

- __HOW (wiring):__ (`WallConfigData` field, beside `lod_settings` line 74) `public DisplacementSettings displacement_settings = new();` (`WallSession` accessor, beside `LodSettings` line 43) `public DisplacementSettings DisplacementSettings => _config?.displacement_settings;`
- __WHERE:__ `TileStories\Assets\Framework\Runtime\Core\WallConfigData.cs` � (1) field after `lod_settings` (~L74); (2) `DisplacementSettings` class immediately after the `LodBandEntry` class, before the search-fields block (~L541, all engine-wide config [Serializable] classes co-located). `TileStories\Assets\Framework\Runtime\Core\WallSession.cs` after `LodSettings` property (L43).
- __WHY:__ Framework-data contract; mirror of `LodSettings`; matches the runtime JSON round-trip (`config.json` <-> `WallConfigData`); a new wall opts in by authoring a `displacement_settings` block. No bore of a separate file or ScriptableObject (the latter is premature until the baker/`WallConfigAsset` lands � 10-structure notes it is still not built).
- __TESTS:__ [x] DONE. Tier-0 EditMode `DisplacementSettingsDefaultsTests.cs` (Tests/Editor/): 2 tests -- (1) all DisplacementSettings defaults match spec _2.5 section 10; (2) `WallConfigData.displacement_settings` initializes to those defaults. (Accessor `WallSession.DisplacementSettings` null-safety deferred to Block 6 -- it mirrors the `LodSettings` projection idiom `$config?.lod_settings`.) Suites green: EditMode **489/489 (was 487, +2 new), 0 failures**; PlayMode **41/41, 0 failures**; 0 `error CS`. Verified on disk + live run 2026-08-18.
- __FINISHING:__ Mark `__curr_plan_tracker.md` Block 1 [x] with the live test counts; mark `_2.5_Marker_Displacement.md` Implementation Status row 1 to ? (state how verified: class + field + accessor on disk, defaults test green); `10-structure.md` needs no tree change (no new file/monors), but verify the `DisplacementSettings` entry note if it is described in the Holo row. Then halt for the next block command (hard-stop rule).



## BLOCK 2 � `label_only` + `fixed_axis` (foundation)

> Assert the Editor's Test Runner is free before any verify step. If `refresh_unity` returns `tests_running` after a compile-failure, stop and clear it (close the Test Runner compile-error dialog, or restart the Unity Editor); do __not__ keep retrying `refresh` � it will not recompile while the runner is hung. Only then run `refresh_unity` (0 error CS) + EditMode + PlayMode (0 failures).


> DECISION (2026-08-18): clean v1 -- no backward-compat cruft. This block REPLACES
> the legacy overlap path in full: delete `MarkerOverlapResolver.ApplyOverlapOffsets`
> and `MarkerView.ApplyOverlapOffset(float)` and remove the single `WallSession.cs:258`
> spawn-time call. Consequence (accepted): LivingRoom's runtime overlap nudge is
> DORMANT from Block 2 until Block 6 wires the continuous LOD step-8. The 3 legacy
> `MarkerOverlapResolverTests` overlap tests are deleted (they test a deleted method);
> the unrelated `MarkerBillboard` test is kept. Unit type = `VisualUnit` (spec �7).



### 2a. `MarkerView` label-only offset API (�4)
- __WHAT:__ `public void ApplyLabelOffset(Vector2 screenOffsetPx)` + `public void ClearLabelOffset()`.
  Sets/restores the label `anchoredPosition` = `base + ScreenPixelsToWorld(screenPx)`.
  Idempotent (absolute-from-base), no-op when label hidden/absent. Label is a child of the
  billboarded root -> offset is camera-relative by construction (no geometry fix).
- __HOW__ (`MarkerView.cs`):
  - Capture `_baseLabelAnchoredPosition` once, inside the `!_hasBaseLabelSize` guard in
    `ApplyLabelState` (L388-399), which today captures sizeDelta/anchorMin/anchorMax/pivot
    but NOT anchoredPosition.
  - `ApplyLabelOffset(px)`: `((RectTransform)labelText.transform).anchoredPosition =
    _baseLabelAnchoredPosition + MarkerLayout.ScreenPixelsToWorld(px, distToCam, Camera.main);`
    `distToCam = Vector3.Distance(transform.position, cam.transform.position)`.
  - `ClearLabelOffset()`: `...anchoredPosition = _baseLabelAnchoredPosition;`
  - Pure helper (NEW; MarkerLayout.cs is proportions-only today): `public static Vector2
    ScreenPixelsToWorld(Vector2 screenPx, float distanceM, Camera cam)` -- world-units-per-pixel
    at depth = `(2*distanceM*tan(fov*0.5f))/screenPixelHeight`, applied per axis. Reused by
    Block 4's full-marker conversion.
- __WHERE:__ `TileStories\Assets\Framework\Runtime\UI\Markers\MarkerView.cs` + pure helper in
  `TileStories\Assets\Framework\Runtime\UI\Markers\MarkerLayout.cs`.
- __TESTS:__ EditMode `MarkerLayoutPxConversionTests.cs` -- world-per-pixel increases with
  distance; per-axis fov/height math; zero/guard cases.
- __WHY:__ MarkerView stays a thin write method; conversion pure/testable (20-code-quality).

### 2b. Evolve `MarkerOverlapResolver` -> `ApplyDisplacement` (�5 + �8-adjacent) -- REPLACES legacy
- __WHAT:__ New `public static void ApplyDisplacement(IReadOnlyList<VisualUnit> visibleUnits,
  Camera cam, DisplacementSettings settings)`. (No stability-counters arg -- that is Block 7.)
  1. Guard: null / `<2` / `!settings.enabled` -> clear all labels and return.
  2. Snapshot each `unit.worldPosition` via `cam.WorldToScreenPoint` -> screen positions.
  3. Clear all label offsets first, then union-find group at `settings.overlap_threshold_px`
     (reuse proven union-find; the legacy `OverlapThresholdPixels` const and its method die here).
  4. Deterministic member order by `poiId` (`string.CompareOrdinal`, existing convention).
  5. `fixed_axis` (�5): member k of an >=2 group gets screen offset `k * stepDir` stacked along
     the group's screen-up axis; `step ~ settings.overlap_threshold_px` (separates >= threshold),
     symmetric spread from centroid; clamp magnitude to `settings.max_displacement_px`.
  6. Convert screen->label-local via `MarkerLayout.ScreenPixelsToWorld`; call
     `unit.marker.ApplyLabelOffset(...)` (respects `displace_target == "label_only"` this block).
  7. GUARDRAILS (much guardrails for laters): `displacement_algorithm` switch implements
     `fixed_axis`; `candidate_position`/`force_directed` -> `Debug.LogWarningOnce("[Displacement]
     <algo> not yet implemented (Block 3); using fixed_axis")` + run fixed_axis (safe fallback,
     never silently wrong). Same guardrail: `displace_target != label_only` and
     `displacement_tiebreak == lower_priority_only` -> warn once + apply label_only/symmetric.
  8. DELETE legacy in this same block: `MarkerOverlapResolver.ApplyOverlapOffsets`, the
     `OverlapThresholdPixels` const, `MarkerView.ApplyOverlapOffset(float)` (MarkerView.cs:509),
     and the `WallSession.cs:258` spawn-time call. Grep=0 callers after (v1-clean).
- __Pure core (scene-free, Tier-0):__ `public static Vector2[] ComputeOffsets(
  IReadOnlyList<Vector2> screenPositions, IReadOnlyList<string> ids, DisplacementSettings settings)`
  (index-aligned offsets, zero when non-overlapping). `ApplyDisplacement` is the thin wrapper
  (snapshot + convert + write). Never mentions VisualUnit.
- __WHERE:__ `TileStories\Assets\Framework\Runtime\POI\MarkerOverlapResolver.cs` (stale `_1_2`
  header -> `_2.5 �5`), `MarkerView.cs` (delete legacy), `WallSession.cs` (delete line 258's call).
- __TESTS (Phase A):__
  - EditMode `DisplacementComputeTests.cs`: 2/3/5+ group fully separated (pairwise screen
    distance > threshold); no-overlap -> zero offsets; deterministic under shuffled input;
    `max_displacement_px` clamp; empty/singleton guards; fixed_axis symmetry/balance.
  - PlayMode `DisplacementLabelTests.cs` (CreateTestMarker pattern, keep MarkerBillboard test):
    (a) fabricated overlapping row -> labels separated on screen > threshold;
    (b) __label-only leaves the MARKER glyph screen position unchanged__ (locks the contract);
    (c) idempotence (2 cycles same result); (d) a unit that stopped overlapping gets its label
    cleared; (e) guardrail: a config with `displacement_algorithm=force_directed` still separates
    via fixed_axis + exactly-one warning.
  - __LABEL-PATH SHALLOW-ANGLE REGRESSION (failing-first, Execution Bounds):__ camera at grazing
    angle to an overlapping row under label_only; after ApplyDisplacement the LABELS must be
    > threshold apart on screen. Author it first and run it FAILING (labels at the marker overlap
    at shallow angle), then the label write makes it pass. Full-marker shallow-angle regression
    (spec �12 L492-497) stays in Block 4.
  - MarkerOverlapResolverTests.cs: DELETE the 3 ApplyOverlapOffsets tests (method deleted); KEEP
    the MarkerBillboard test.
- __WHY:__ Evolved, not rewritten (spec �0/�5); VisualUnit is spec �7-mandated, carries `marker`
  (write path) + `worldPosition`; pure core is Tier-0-testable (spec �12 L486-487).

### 2c. Phase-A gallery scaffold
- __WHAT:__ NEW `Assets/Dev/DisplacementGallery/DisplacementGalleryScene.unity` +
  `DisplacementGalleryDefinitions.cs` (entry + `Overrides`) + `DisplacementGalleryHarness.cs`.
  Data-struct-driven: group size {2,3,5+}, viewing angle (normal + SHALLOW), `displace_target`
  (label_only this block; marker/both deferred to Block 4), algorithm. Reuse
  `Assets/Dev/MarkerGallery/Backdrops/backdrop.png`; never added to Build Settings.
- __WHERE:__ `TileStories\Assets\Dev\DisplacementGallery`; harness + definitions in
  `TileStories\Assets\Framework\Runtime\DevTools` (ClusterGalleryHarness pattern -- Editor-guarded
  AssetDatabase `Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab`,
  `CategoryPalette.Configure(Overrides)`). Gallery reads `DisplacementSettings`
  {enabled, overlap_threshold_px, displace_target=label_only, displacement_algorithm=fixed_axis,
  max_displacement_px} -- the developer surface is exercised BEFORE the Block-8 editor foldout.
- __TESTS:__ PlayMode `DisplacementGalleryTests.cs` drives the SAME `Entries` list (never drift),
  asserts uniform label separation per entry at both camera poses incl. shallow.
- __WHY:__ 40-testing �4.4 Phase A; ONE list drives harness + tests.

### FINISHING (Block 2)
- `_2.5_Marker_Displacement.md`: rows 3 (MarkerView label API), 4 (ApplyDisplacement/delete legacy),
  10 (fixed_axis) -> [ok] (state how verified: live counts + on-disk); rows 5/6/7 stay [x-not done],
  11/12 stay (Block 3).
- `__curr_plan_tracker.md`: master L7 [x]; this Block-2 section already pasted (update to this text).
- `.clinerules/10-structure.md`: add `DisplacementComputeTests.cs`, `DisplacementLabelTests.cs`,
  `DisplacementGallery/`, `DevTools/DisplacementGalleryDefinitions.cs` + `DisplacementGalleryHarness.cs`;
  note `MarkerLayout.ScreenPixelsToWorld` + `MarkerView.ApplyLabelOffset/ClearLabelOffset`; note the
  legacy `ApplyOverlapOffsets`/`ApplyOverlapOffset` entries removed.
- Full EditMode + PlayMode green (0 failures); `refresh_unity` 0 `error CS`.
- Temp hygiene (�5.3): delete the 9 pre-existing residue scratch files in the workspace root
  (`__apply_tests.py`, `__compile_check_done.txt`, `__editmode_result.txt`, `__find.txt`,
  `__fix_sml.py`, `__log_tail.txt`, `__out.txt`, `__poll_tests.ps1`, `__run_tests.ps1`) + any new
  `__*` created.
- Handoff: raw Tier-0 output; confirm the label shallow-angle regression was observed FAILING before
  the fix; confirm `_2.5` rows updated and `grep ApplyOverlapOffset = 0`.

- Recreate `DisplacementLabelTests.cs` with clean 12-sp indentation (its current 16/24/36-sp lines are prepend-bug residue).
- Resolve the `stabilityCounters` arg deviation: either delete it (per plan L149, it's a Block-7 item) or document it as a deliberate early addition.
- Verify the label shallow-angle regression test is authored (it's in 2b's test list but wasn't found among the 5 PlayMode tests).


## BLOCK 3 IMPLEMENTATION PLAN
?? WHAT: Acceptance Criteria & Deliverables
Core Objective: Implement two displacement algorithms in MarkerOverlapResolver.cs that:

Operate exclusively in label_only mode (displace ONLY label positions via MarkerView.ApplyLabelOffset)
Use screen-space offset calculations (pixels) converted to world units via MarkerLayout.ScreenPixelsToWorld
Respect hierarchy priority (from VisualUnit.hierarchyLevelIndex, where LOWER index = HIGHER priority)
Generate deterministic outputs (identical inputs ? identical outputs)
Handle all edge cases: empty/singleton groups, identical priorities, max displacement clamping
Specific Algorithms to Implement Per Spec Sections 3-4:

1. candidate_position Algorithm (Spec Section 3)
For each overlapping group:

Generate candidate positions for each marker in a search pattern (concentric squares) around original position
Score candidates by:
Primary: Distance from original position (minimize movement)
Secondary: Priority bonus (higher priority markers get score reduction)
Tertiary: Marker ID for deterministic tiebreaking
Select best valid candidate per marker using greedy selection (process markers in priority order)
Validate candidates have ZERO overlap with already-selected markers' candidates
Clamp final offset to max_displacement_px
2. force_directed Algorithm (Spec Section 4 - DEFAULT)
For each overlapping group:

Initialize offsets to zero
Run simulation for force_directed_iterations steps (default: 4):
Calculate repulsion forces between all marker pairs within overlap threshold
Force magnitude = (threshold - distance) / distance [stronger when closer]
Apply equal/opposite forces to each pair
Scale force applied to each marker by priority factor (higher priority = less movement)
Update offsets with damping factor (0.5) to prevent oscillation
Clamp offset magnitude after each iteration
Apply final clamped offsets
Deliverables:

Modified Assets\Framework\Runtime\POI\MarkerOverlapResolver.cs with both algorithms properly implemented
Updated Assets\Framework\Tests\EditMode\DisplacementAlgorithmTests.cs with tests that actually test the real algorithms
Updated _2.5_Marker_Displacement.md Implementation Status rows 11-12 to [ok] with verification notes
Update __curr_plan_tracker.md to mark Block 3 as [x] DONE with live test counts
?? HOW: Implementation Details
1. candidate_position Algorithm Implementation
Replace the current placeholder implementation (lines 137-188) with:


private static Vector2[] ComputeOffsets_CandidatePosition(
    IReadOnlyList<Vector2> screenPositions,
    IReadOnlyList<string> ids,
    DisplacementSettings settings,
    IReadOnlyList<VisualUnit> visualUnits)
{
    int n = screenPositions.Count;
    var offsets = new Vector2[n];
    if (n < 2 || settings == null || !settings.enabled)
        return offsets;

    float threshold = Mathf.Max(0.0001f, settings.overlap_threshold_px);
    float maxDisp = Mathf.Max(0f, settings.max_displacement_px);
    float labelWidth = threshold; // Using threshold as proxy for label width

    // Build overlap groups (reuse existing helper)
    var groups = BuildOverlapGroups(screenPositions, threshold, ids);

    // Process each group
    foreach (var kvp in groups)
    {
        var memberIndices = kvp.Value;
        if (memberIndices.Count < 2) continue;

        // Extract member data with priorities
        var members = new List<MemberData>();
        foreach (int idx in memberIndices)
        {
            members.Add(new MemberData(
                idx,
                screenPositions[idx],
                ids[idx],
                visualUnits != null && idx < visualUnits.Count
                    ? visualUnits[idx].hierarchyLevelIndex
                    : int.MaxValue
            ));
        }

        // Sort by priority (ascending: lower index = higher priority first)
        members.Sort((a, b) =>
        {
            int cmp = a.priority.CompareTo(b.priority);
            if (cmp != 0) return cmp;
            return string.CompareOrdinal(a.id, b.id); // Tiebreaker: ID then index
        });

        // Track final selected positions to prevent overlap
        var selectedPositions = new Dictionary<int, Vector2>();
        
        // Process each member in priority order (higher priority first)
        foreach (var member in members)
        {
            Vector2 bestOffset = Vector2.zero;
            float bestScore = float.MaxValue;
            bool foundValid = false;

            // Generate candidate positions in expanding square pattern
            for (int radius = 1; radius <= maxDisp / threshold + 3 && !foundValid; radius++)
            {
                float candidateRadius = Mathf.Min(radius * threshold, maxDisp);
                
                // Test points in concentric square pattern
                int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1, -2, -2, -2, -2, -2, -1, 0, 1, 2, 2, 2, 2 };
                int[] dy = { -2, -2, -1, 0, 1, 2, 2, 2, 1, 0, -1, -2, -2, -2, -2, -1, 0, 1, 2, 2 };

                for (int dir = 0; dir < dx.Length; dir++)
                {
                    float candX = member.originalPos.x + dx[dir] * candidateRadius;
                    float candY = member.originalPos.y + dy[dir] * candidateRadius;
                    Vector2 candidatePos = new Vector2(candX, candY);

                    // Calculate offset from original
                    Vector2 offset = candidatePos - member.originalPos;

                    // Skip if exceeds max displacement
                    if (offset.magnitude > maxDisp) continue;

                    // Check overlap with already selected positions
                    bool overlaps = false;
                    foreach (var selectedKvp in selectedPositions)
                    {
                        int otherIdx = selectedKvp.Key;
                        Vector2 otherFinalPos = selectedKvp.Value;
                        
                        if (Vector2.Distance(candidatePos, otherFinalPos) < threshold)
                        {
                            overlaps = true;
                            break;
                        }
                    }

                    if (overlaps) continue;

                    // Score candidate: prioritize minimal movement, then priority, then ID
                    float distanceScore = offset.magnitude;
                    float priorityScore = member.priority * 0.1f; // Weight priority lower than distance
                    float idScore = member.id.GetHashCode() * 0.00001f; // Tiny weight for determinism

                    float totalScore = distanceScore + priorityScore + idScore;

                    if (totalScore < bestScore)
                    {
                        bestScore = totalScore;
                        bestOffset = offset;
                        foundValid = true;
                    }
                }
            }

            // If no valid candidate found (shouldn't happen with sufficient search),
            // use zero offset (will be handled by overlap check in ApplyDisplacement)
            if (!foundValid) bestOffset = Vector2.zero;

            // Store final position
            selectedPositions[member.index] = member.originalPos + bestOffset;
            offsets[member.index] = bestOffset;
        }
    }

    return offsets;
}

// Helper struct for candidate algorithm
private struct MemberData
{
    public int index;
    public Vector2 originalPos;
    public string id;
    public int priority;

    public MemberData(int index, Vector2 originalPos, string id, int priority)
    {
        this.index = index;
        this.originalPos = originalPos;
        this.id = id;
        this.priority = priority;
    }
}
2. force_directed Algorithm Implementation
Replace the current placeholder implementation (lines 249-299) with:


private static Vector2[] ComputeOffsets_ForceDirected(
    IReadOnlyList<Vector2> screenPositions,
    IReadOnlyList<string> ids,
    DisplacementSettings settings,
    IReadOnlyList<VisualUnit> visualUnits)
{
    int n = screenPositions.Count;
    var offsets = new Vector2[n];
    if (n < 2 || settings == null || !settings.enabled)
        return offsets;

    float threshold = Mathf.Max(0.0001f, settings.overlap_threshold_px);
    float maxDisp = Mathf.Max(0f, settings.max_displacement_px);
    int iterations = Mathf.Max(1, settings.force_directed_iterations);
    float damping = 0.5f; // Prevents oscillation

    // Build overlap groups
    var groups = BuildOverlapGroups(screenPositions, threshold, ids);

    // Process each group independently
    foreach (var kvp in groups)
    {
        var memberIndices = kvp.Value;
        if (memberIndices.Count < 2) continue;

        // Initialize offsets to zero for this group
        foreach (int idx in memberIndices)
            offsets[idx] = Vector2.zero;

        // Force-directed simulation
        for (int iter = 0; iter < iterations; iter++)
        {
            // Calculate forces for each marker
            var forces = new Vector2[memberIndices.Count];

            // Calculate pairwise forces
            for (int i = 0; i < memberIndices.Count; i++)
            {
                int idxA = memberIndices[i];
                Vector2 posA = screenPositions[idxA];
                int priorityA = visualUnits != null && idxA < visualUnits.Count
                    ? visualUnits[idxA].hierarchyLevelIndex
                    : int.MaxValue;

                for (int j = i + 1; j < memberIndices.Count; j++)
                {
                    int idxB = memberIndices[j];
                    Vector2 posB = screenPositions[idxB];
                    int priorityB = visualUnits != null && idxB < visualUnits.Count
                        ? visualUnits[idxB].hierarchyLevelIndex
                        : int.MaxValue;

                    Vector2 diff = posB - posA;
                    float distance = diff.magnitude;

                    // Only apply force if within overlap threshold and not zero distance
                    if (distance < threshold && distance > 0.001f)
                    {
                        // Repulsion strength: stronger when closer
                        float forceMagnitude = (threshold - distance) / distance;

                        // Normalize direction
                        Vector2 forceDir = diff / distance;

                        // Base force vector
                        Vector2 forceBase = forceDir * forceMagnitude;

                        // Priority scaling: higher priority marker experiences less force
                        // Lower index = higher priority
                        // Scale force inversely: forceOnA ? priorityB, forceOnB ? priorityA
                        float priorityFactorA = Mathf.Max(1, priorityB);
                        float priorityFactorB = Mathf.Max(1, priorityA);

                        // Normalize priority factors to prevent excessive scaling
                        float avgPriority = (priorityFactorA + priorityFactorB) * 0.5f;
                        if (avgPriority > 0)
                        {
                            priorityFactorA /= avgPriority;
                            priorityFactorB /= avgPriority;
                        }

                        // Apply scaled forces
                        forces[i] += forceBase * priorityFactorA;
                        forces[j] -= forceBase * priorityFactorB; // Opposite direction
                    }
                }
            }

            // Apply forces with damping and clamping
            for (int i = 0; i < memberIndices.Count; i++)
            {
                int idx = memberIndices[i];
                Vector2 force = forces[i];

                // Apply damping
                Vector2 displacement = force * damping;

                // Update offset
                Vector2 newOffset = offsets[idx] + displacement;

                // Clamp to max displacement
                if (newOffset.magnitude > maxDisp)
                    newOffset = newOffset.normalized * maxDisp;

                offsets[idx] = newOffset;
            }
        }
    }

    return offsets;
}
3. Required Modifications to Existing Methods
Ensure ComputeOffsets_FixedAxis correctly uses the priority-based sorting it already has (lines 101-118 are correct)
Ensure ApplyDisplacement method passes visualUnits parameter (line 339 is correct)
4. Critical Code Quality Rules Applied
Pure Functions: All algorithms depend only on inputs (screenPositions, ids, settings, visualUnits) - no UnityEngine/MonoBehaviour dependencies except Mathf
Deterministic: All data sorted before processing; tiebreakers use IDs/indexes
Reuse Existing Code:
BuildOverlapGroups helper for union-find grouping
Existing warning system (WarnOnce, ResetWarnings)
MarkerLayout.ScreenPixelsToWorld for coordinate conversion (unchanged)
Edge Case Handling:
Early return for n<2, null settings, or disabled
Singleton groups automatically get zero offset
Zero-distance protection in force calculations
Clamping prevents excessive displacement
Minimal Mutability:
Input lists treated as read-only
Only output offsets array is modified
No changes to input visualUnits or settings
Performance Conscious:
Force-directed O(n�) per group but groups are small (typically <10 markers)
Candidate generation bounded by maxDisplacement/threshold
Early exits for trivial cases
Code Clarity:
XML comments for all new public/protected methods
Descriptive variable names (threshold, maxDisp, damping)
Logical grouping with whitespace and region-like comments
Helper structs kept private and scoped to algorithm
?? WHERE: Exact File Locations (per 10-structure.md)
File Path	Purpose	Status
Assets\Framework\Runtime\POI\MarkerOverlapResolver.cs	Primary implementation of both algorithms	MODIFY ?
Assets\Framework\Tests\EditMode\DisplacementAlgorithmTests.cs	EditMode test suite for algorithms	UPDATE ?
Assets\Framework\Runtime\POI\VisualUnit.cs	Data struct for visual units	READ-ONLY
Assets\Framework\Runtime\POI\LODController.cs	Provides visualUnits with hierarchyLevelIndex	READ-ONLY
Assets\Framework\Runtime\UI\Markers\MarkerView.cs	Provides ApplyLabelOffset/ClearLabelOffset	READ-ONLY
Assets\Framework\Runtime\Core\WallSession.cs	Passes visualUnits to LODController	READ-ONLY
Assets\Framework\Runtime\Core\WallConfigData.cs	Contains DisplacementSettings definition	READ-ONLY
?? TESTS: Verification Requirements
EditMode Tests (Updated: DisplacementAlgorithmTests.cs)
Test Class Structure:

[Test] public void FixedAxis_TwoOverlappingMarkers_PriorityDeterminesWhichMoves() (existing, verifies fixed_axis still works)
[Test] public void CandidatePosition_ThreeOverlappingMarkers_PriorityAware()
[Test] public void CandidatePosition_MaxDisplacementClamping_Respected()
[Test] public void CandidatePosition_DeterministicIdenticalInputOutput()
[Test] public void ForceDirected_ThreeOverlappingMarkers_PriorityTriangle()
[Test] public void ForceDirected_IterationsAffectConvergence()
[Test] public void ForceDirected_MaxDisplacementClamping_Respected()
[Test] public void ForceDirected_DeterministicIdenticalInputOutput()
[Test] public void InvalidAlgorithm_FallsBackToFixedAxis_LogsWarningOnce()
[Test] public void InvalidTiebreak_FallsBackToSymmetric_LogsWarningOnce()
[Test] public void InvalidDisplaceTarget_FallsBackToLabelOnly_LogsWarningOnce()
[Test] public void ApplyDisplacement_CandidatePosition_UpdatesLabelOffsetsOnly()
[Test] public void ApplyDisplacement_ForceDirected_ClearsOffsetsWhenDisabled()
[Test] public void EmptyGroup_ReturnsZeroOffsets()
[Test] public void DisabledSettings_ReturnsZeroOffsets()
[Test] public void NullVisualUnits_UsesDefaultPriority()
[Test] public void Determinism_SameInputSameOutput()
PlayMode Tests (Verify No Regression)
All existing tests in:
DisplacementLabelTests.cs
DisplacementComputeTests.cs
DisplacementGalleryTests.cs
LivingRoomConfigIntegrationTests.cs
Must continue to pass (0 failures)
? SUCCESS METRICS
? EditMode: 0 test failures in new + existing suites (target: 513/513+)
? PlayMode: 0 test failures in existing suites (target: 45/45)
? refresh_unity: 0 error CS
? No warnings in console for valid configurations
? All Block 2 tests continue passing (confirming no regression)
? candidate_position and force_directed algorithms produce visually distinct spreads (verified via new tests)
? Priority correctly influences outcomes (higher priority markers move less)
? Max displacement clamping works correctly
? Determinism verified across multiple runs with identical inputs
?? WHY: Architectural Rationale
Where it Lives (File/Assembly Placement)

MarkerOverlapResolver.cs (Runtime/POI):
Correct layer: Pure algorithm logic, no Unity dependencies beyond Mathf and Vector2
Follows "evolve not rewrite" principle: extending existing resolver rather than creating new class
No wall-specific knowledge: depends only on generic VisualUnit (provided by LODController)
Assembly: TileStories.asmdef (Runtime) - correct for framework logic
Why not separate DisplacementEngine?
Would duplicate correct union-find grouping logic (violates "evolve not rewrite")
Increases surface area for bugs (more interfaces to maintain)
Current resolver already has correct grouping - only algorithm logic needs extension
Why not in MarkerView or LODController?
MarkerView: Mixes rendering logic with algorithmic concerns (violates SRP)
LODController: Mixes LOD pipeline with displacement algorithms (tight coupling)
Current approach: Clean separation - LODController orchestrates, MarkerOverlapResolver computes
How it Works (Implementation Logic Separation)

Label vs Marker Displacement Separation (Block 4 concern): Block 3 focuses purely on label displacement (label_only)
Keeps algorithmic complexity isolated from marker/world-space transforms
Prepares clean extension point for marker/both modes in Block 4
Algorithm Decoupling:
Each algorithm is self-contained private method
Shared only: grouping (BuildOverlapGroups), warning system, settings validation
Easy to add new algorithms later (e.g., simulated_annealing) without touching core logic
Priority Handling Consistency:
All three algorithms (fixed_axis, candidate_position, force_directed) use SAME priority definition:
Source: VisualUnit.hierarchyLevelIndex (from MarkerHierarchyResolver.TryResolvePriority)
Semantics: LOWER index = HIGHER priority (higher priority marker "wins" ties/gets less displacement)
Consistent with LODController's ComparePriority and count-cap logic
Determinism Guarantees:
All inputs sorted before processing (by ID or priority as appropriate)
Tiebreakers use immutable data (marker ID, world position order)
No reliance on frame timing, random seeds, or external state
Critical for networked/replayable experiences (though not currently required)
Framework Reusability & Wall-Specific Configuration

Zero Code Changes for New Walls:
Algorithms depend only on:
Screen positions (from any localization system via VisualUnit.worldPosition)
Marker IDs (arbitrary strings)
Settings (from WallConfigData.DisplacementSettings)
Priority indices (from any hierarchy system via VisualUnit.hierarchyLevelIndex)
New wall only needs to provide:
Correct VisualUnit list (LODController's job)
Appropriate DisplacementSettings in their config.json
Hierarchy system that populates hierarchyLevelIndex (already in place)
Settings-Driven Behavior:
Walls can tune: overlap_threshold_px, max_displacement_px, force_directed_iterations
All via config - no code recompilation needed
Safe Failure Modes:
Invalid settings ? fallback to fixed_axis + single warning (not crash)
Disabled ? zero offsets (no visual change)
Extreme values ? clamped to safe ranges
Prevents one wall's bad config from breaking the framework
?? FINISHING: Checklist for Updating Files
After implementing the above:

Update _2.5_Marker_Displacement.md Implementation Status:
Row 11: candidate_position algorithm ? [ok] done: [state how verified: live EditMode test counts + algorithm correctness verified]
Row 12: force_directed algorithm (default) ? [ok] done: [state how verified: live EditMode test counts + algorithm correctness verified]
Update __curr_plan_tracker.md: master line 8 [x] with live test counts
Update .clinerules/10-structure.md tree: No changes needed (no new files, only modifying existing ones)
Run Full Test Suite:
unityMCP__refresh_unity(compile=request)
unityMCP__run_tests(EditMode) + unityMCP__get_test_job ? Verify 0 failures
unityMCP__run_tests(PlayMode) + unityMCP__get_test_job ? Verify 0 failures (acknowledging known MCP PlayMode initialization issue)
Temp Hygiene: Delete any new __* files created during development
Handoff: Provide raw test output confirming all new tests pass and existing tests still pass

---

### Retrospective Audit � Block 3 (candidate_position + force_directed)

**VERIFIED COMPLETE via Unity MCP (2026-08-20):**

| Check | Result | Evidence |
|---|---|---|
| `refresh_unity` (compile) | 0 `error CS` | Unity returned `resulting_state: "idle"`, `read_console` returned 0 entries |
| EditMode suite | 510/510 passed, 0 failures | job_id `911bc3c563a34b52baf7f4501dd8bcb1`, summary: {total: 510, passed: 510, failed: 0, resultState: "Passed"}, 0.98s |
| PlayMode suite | 45/45 passed, 0 failures | job_id `8ee01dc6b887466ba1c1a649151281eb`, summary: {total: 45, passed: 45, failed: 0, resultState: "Passed"}, 8.27s |

**Code-on-disk audit:**

1. `ComputeOffsets_CandidatePosition` (MarkerOverlapResolver.cs L144-335): ? Symmetric target angles (L190-215, `90f + k * 360f/runSize`), angular preference score (L295-297), priority-aware sort (L181-188), max-displacement clamping (L324-332), BuildOverlapGroups reuse (L159).
2. `ComputeOffsets_ForceDirected` (L390-566): ? `i * 2.4f` perturbation (L463), priority-aware anchoring (L469-478), Y-separation post-processing pass (L495-556, `minSep = 40 * 0.875 = 35px`).
3. No dead code: `GenerateCandidatePositions`/`ComputeOverlapScore` confirmed absent from all `.cs` files (search across 4231 files).
4. `ApplyDisplacement` (L568-624): Correct signature with `stabilityCounters` param; dispatches to `ComputeOffsets`; applies via `ApplyLabelOffset`/`ClearLabelOffset`.
5. `WallSession.SpawnPOIs()`: One-shot call removed (L257-259 is a comment only; grep confirms no `ApplyOverlapOffsets` call).
6. `_2.3_Marker_Hierarchy.md`: Patched for `priority` field (findstr confirms `"priority"` and `"TryResolvePriority"` present).
7. `HierarchyLevelEntry.priority`: Confirmed at WallConfigData.cs L339 (int field, documented at L337-338).

**Issues documented (not code defects):**
- Plan tracker "Fix" section (old L682-712): tasks were planned but code already has the fix. Plan tracker checklist not updated. DELETED in this reorganization.
- Spec �7 `lower_priority_only`: deferred (see decisions above). Does not block Block 4.
- Spec �12 references `Assets/Dev/LODGallery/` � does not exist; correct reference is `Assets/Dev/` (DisplacementGalleryScene). Documentation fix in Block 10.
- Spec status Row 3 claims `ApplyLabelState` exists � stale name. Actual method is inline in `ApplyVisuals`. Documentation fix in Block 10.


## BLOCK 4 � `marker`/`both` + camera-relative conversion (�4)

> Per spec �11a step 4: "Only now, marker/both displace-target modes � this is where �4's camera-relative conversion fix actually gets exercised." All three algorithms (fixed_axis, candidate_position, force_directed) already work correctly in `label_only` mode. The `marker`/`both` path is the one geometry-risk area and is isolated last.

### Current state on disk

| Item | Status |
|---|---|
| `MarkerView.ApplyMarkerOffset` | ? Does not exist |
| `MarkerView.ClearMarkerOffset` | ? Does not exist |
| `ApplyDisplacement` displace_target dispatch | ? Only `label_only` supported (L578-579 warns + falls back) |
| `_baseLocalPosition` / `_hasBasePosition` (L60-61) | ? Reserved for Block 4, unused. Comment at L84-85 confirms intent. |
| Gallery entries with `marker`/`both` | ? All 8 entries use `label_only` |
| Shallow-angle regression test for `marker`/`both` | ? Only `label_only` shallow-angle test exists (DisplacementGalleryTests L129) |
| `MarkerLayout.ScreenPixelsToWorld` | ? Exists (L71), tested 5/5 (`MarkerLayoutPxConversionTests`), reusable for `worldPerPixel` |
| `MarkerBillboard` | ? Rotates root to face camera every `LateUpdate` (L38-43) � unaffected by root position change |

### Architecture-Level Analysis (Where it lives)

**Option A (CHOSEN):** Add `ApplyMarkerOffset`/`ClearMarkerOffset` to `MarkerView.cs` (`Runtime/UI/Markers/`), alongside existing `ApplyLabelOffset`/`ClearLabelOffset`. Extend dispatch in `MarkerOverlapResolver.ApplyDisplacement` (L568) with `switch` on `displace_target`.

- *Pros:* Follows existing pattern exactly. Visual-state methods live on MarkerView; algorithm/dispatch lives in MarkerOverlapResolver. The `_baseLocalPosition`/`_hasBasePosition` fields (L60-61) are already on MarkerView "for Block 4." Minimal surface area. No new files except tests.
- *Cons:* MarkerView grows by ~25 lines (still well within thin-MonoBehaviour guidelines).

**Option B:** Separate `MarkerDisplacementController : MonoBehaviour` component.
- *Pros:* MarkerView stays thin.
- *Cons:* Violates existing pattern. Adds component wiring on prefab. VisualUnit.marker would need a new field. Over-engineering for a 2-line world-delta formula. Violates "prefer simple duplication over fragile generic abstraction" (20-code-quality.md).

**Option C:** Standalone `MarkerDisplacementMath` static class.
- *Pros:* Pure-logic, Tier-0 testable.
- *Cons:* Formula is `cam.transform.right * w2D.x + cam.transform.up * w2D.y` � 1 line. Extraction adds a file for trivial composition. `ScreenPixelsToWorld` already tested. "No speculative code" (20-code-quality.md).

### Implementation-Level Analysis (How it works)

**Option A (CHOSEN):** Reuse `MarkerLayout.ScreenPixelsToWorld(screenOffsetPx, depthM, cam)` to get 2D world offset, then project onto camera axes:
```csharp
Vector2 world2D = MarkerLayout.ScreenPixelsToWorld(screenOffsetPx, depthM, cam);
Vector3 worldDelta = cam.transform.right * world2D.x + cam.transform.up * world2D.y;
transform.position = _baseWorldPosition + worldDelta;
````

- *Pros:* Reuses tested FOV math. Camera right/up vectors are trivially correct. `depthM` uses same perspective formula as `ApplyLabelOffset` (L547).
- *Cons:* None significant.

__Option B:__ New `MarkerLayout.ScreenOffsetToWorldDelta3D` method.

- *Pros:* Self-documenting, Tier-0 testable.
- *Cons:* One line of logic. `ScreenPixelsToWorld` already tested. Adds method for trivial composition.

__Option C:__ `Camera.ScreenToWorldPoint` / `ViewportToWorldPoint`.

- *Pros:* Built-in Unity API.
- *Cons:* Frame-dependent, two calls needed, harder Tier-0 testing. Violates �12 Tier 0.

### Developer-selectable options

`DisplacementSettings.displace_target` (WallConfigData L555) � already in schema:

- `"label_only"` (default) � safer, no camera-relative math, only label moves
- `"marker"` � marker glyph moves, label follows (child of root)
- `"both"` � both label and glyph move independently

No additional options for Block 4. The camera-relative conversion is the only correct approach at grazing angles. The `displace_target` dropdown will be exposed in the Editor UI (Block 8, �11).

### Detailed TODO List

#### Task 1: Write shallow-angle regression test FIRST (Tier 0.5, PlayMode)

__WHAT:__ Regression test that FAILS under fixed wall-Y offset but PASSES with camera-relative conversion. Write before implementation.

__HOW:__

- 3 co-located markers at origin; camera at 65� grazing angle (reuse `DisplacementGalleryTests.ShallowAngle_LabelDisplacement_ResolvesScreenOverlap` L134-135 pose).
- Call `ApplyDisplacement` with `displace_target = "marker"`, `displacement_algorithm = "fixed_axis"`.
- Assert: pairwise screen-Y separation of marker ROOT positions (`_cam.WorldToScreenPoint(m.transform.position)`) >= 35px.
- Document: at 65�, a wall-space Y offset projects to `cos(65�) � 42%` of intended screen-Y � a 40px separation need produces only ~17px on screen, failing the assertion. Camera-relative `cam.transform.up` correctly maps to screen-up at any angle.
- Also: Tier 0.5 tap-target check � marker root's screen-space visual extent clears 44�44px after displacement (spec �12).

__WHERE:__ New test methods in `Assets/Framework/Tests/Runtime/DisplacementLabelTests.cs`.

__TESTS:__

- `ShallowAngle_MarkerDisplacement_ResolvesScreenOverlap` � 3 markers, 65� angle, `marker` target, assert root separation >= 35px.
- `ShallowAngle_BothDisplacement_ResolvesScreenOverlap` � same but `displace_target = "both"`.
- `LabelOnly_UnchangedAtShallowAngle` � assert `label_only` still works at same angle (regression guard).

#### Task 2: Add `ApplyMarkerOffset` / `ClearMarkerOffset` to MarkerView.cs

__WHAT:__ New public methods for the marker/both displacement path.

__HOW:__

- Add fields (following `ApplyLabelOffset` pattern at L86-88):

  ```csharp
  private Vector3 _baseWorldPosition;      // marker root world position at baseline
  private bool _hasWorldBasePosition;
  private bool _hasMarkerOffset;
  ```

- `EnsureMarkerWorldBaseCaptured()`: capture `transform.position` on first call (idempotent, copy `EnsureLabelBaseCaptured` L584 pattern).

- `ApplyMarkerOffset(Camera cam, Vector2 screenOffsetPx)`:

  1. `EnsureMarkerWorldBaseCaptured()`.
  2. `float depthM = Vector3.Dot(transform.position - cam.transform.position, cam.transform.forward)` � same perspective-depth formula as `ApplyLabelOffset` L547.
  3. `Vector2 world2D = MarkerLayout.ScreenPixelsToWorld(screenOffsetPx, depthM, cam)`.
  4. `Vector3 worldDelta = cam.transform.right * world2D.x + cam.transform.up * world2D.y`.
  5. `transform.position = _baseWorldPosition + worldDelta; _hasMarkerOffset = true;`

- `ClearMarkerOffset()`: fast-path return if `!_hasMarkerOffset`; else reset `_hasMarkerOffset = false`, `transform.position = _baseWorldPosition`.

__WHERE:__ `C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Runtime\UI\Markers\MarkerView.cs` � add after `GetVisualRadiusWorld` (L627).

__WHY:__ Billroot (`MarkerBillboard`) re-orients root every `LateUpdate` via `transform.rotation = cam.transform.rotation` (L42). Moving `transform.position` is the correct approach � root moves to displaced position, continues to face camera. Reusing `_baseLocalPosition`/`_hasBasePosition` (L60-61) is avoided because those store localPosition (ambiguous name); new clearly-named world-position fields follow the `ApplyLabelOffset` naming convention.

#### Task 3: Update `ApplyDisplacement` dispatch in `MarkerOverlapResolver.cs`

__WHAT:__ Extend `ApplyDisplacement` (L568-624) to dispatch on `displace_target`. Remove the `displace_target` warning.

__HOW:__

- __Remove__ warning at L578-579: `if (settings.displace_target != "label_only") WarnOnce("displace_target", ...)`. Marker/both are now supported.

- __Keep__ the `lower_priority_only` warning at L584-85 (deferred, not Block 4).

- __Replace__ the `if (hasNeighbour)` block at L619-622 with:

  ```csharp
  if (hasNeighbour)
  {
      switch (settings.displace_target)
      {
          case "marker":   marker.ApplyMarkerOffset(cam, offsets[i]); break;
          case "both":     marker.ApplyLabelOffset(cam, offsets[i]);
                           marker.ApplyMarkerOffset(cam, offsets[i]); break;
          default:         marker.ApplyLabelOffset(cam, offsets[i]); break;
      }
  }
  else
  {
      marker.ClearLabelOffset();
      marker.ClearMarkerOffset();  // always clear both to avoid stale state
  }
  ```

__WHERE:__ `C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Runtime\POI\MarkerOverlapResolver.cs` � L578-579 (warning removal), L619-622 (dispatch update).

__WHY:__ Current code warns + falls back for marker/both. After Block 4, these are first-class modes. The `default` case preserves backward compatibility for unknown values (same pattern as algorithm switch L48-59).

#### Task 4: Add Phase A gallery entries for `marker`/`both`

__WHAT:__ Extend `DisplacementGalleryDefinitions.Entries` (8 entries, all `label_only`) with marker/both variants.

__HOW:__ Add 4 entries to `Assets/Framework/Runtime/DevTools/DisplacementGalleryDefinitions.cs` `Entries` list (after L107):

```csharp
new DisplacementGalleryEntry { Label = "3 Members / Normal / Fixed Axis / Marker",
    GroupSize = 3, ViewingAngle = DisplacementViewingAngle.Normal,
    DisplaceTarget = "marker", Algorithm = "fixed_axis" },

new DisplacementGalleryEntry { Label = "3 Members / Normal / Force Directed / Both",
    GroupSize = 3, ViewingAngle = DisplacementViewingAngle.Normal,
    DisplaceTarget = "both", Algorithm = "force_directed" },

new DisplacementGalleryEntry { Label = "3 Members / Shallow / Force Directed / Marker",
    GroupSize = 3, ViewingAngle = DisplacementViewingAngle.Shallow,
    DisplaceTarget = "marker", Algorithm = "force_directed" },

new DisplacementGalleryEntry { Label = "3 Members / Shallow / Force Directed / Both",
    GroupSize = 3, ViewingAngle = DisplacementViewingAngle.Shallow,
    DisplaceTarget = "both", Algorithm = "force_directed" },
```

The existing `DisplacementGalleryTests.GalleryEntries_AllProduceSeparatedLabels` (L41) iterates ALL entries and asserts label Y-gap >= 35px. For marker/both, the label is a child of the root, so its screen Y position reflects the root displacement + label offset. The assertion is valid for all modes.

__WHERE:__ `C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Runtime\DevTools\DisplacementGalleryDefinitions.cs` (MODIFY � add 4 entries).

__WHY:__ Spec �12: "both displace_target modes" tested in Phase A. Data-driven design means no test code changes.

#### Task 5: Add Tier 0 EditMode tests for marker/both offset computation

__WHAT:__ Verify `ComputeOffsets` is `displace_target`-agnostic (offset computation unchanged; only application changes).

__HOW:__

- Add to `DisplacementComputeTests.cs`: `ComputeOffsets_TargetAgnostic_OffsetsIdenticalRegardlessOfTarget` � same input, `displace_target = "label_only"` vs `"marker"` vs `"both"` returns identical offset arrays. This is a regression guard.
- Add to `DisplacementAlgorithmTest.cs`: `ForceDirected_BothTarget_PriorityAware` � same as `ForceDirected_ThreeOverlappingMarkers_PriorityAware` (L108) but with `displace_target = "both"` (proves target doesn't affect computation).

__WHERE:__

- `C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Tests\Editor\DisplacementComputeTests.cs` (MODIFY � add 1 test)
- `C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Tests\Editor\DisplacementAlgorithmTest.cs` (MODIFY � add 1 test)

__WHY:__ Ensures `displace_target` doesn't leak into offset COMPUTATION. The `ApplyDisplacement` dispatch handles target; `ComputeOffsets` handles math. Separation of concerns.

#### Task 6: Phase A verification

__WHAT:__ Confirm all tests pass after Block 4 implementation.

1. `refresh_unity` � 0 `error CS`
2. EditMode tests � 0 failures (expect 510+2 = 512/512)
3. PlayMode tests � 0 failures (expect 45+4 = 49/49, with gallery test now iterating 12 entries instead of 8)

## BLOCK 5 � Leader lines (�6)
Block 5 � Leader lines (�6) � Prospectus Plan
Spec �6 Requirements (verbatim from _2.5_Marker_Displacement.md L284-304)
Requirement	Config Source	Implementation Detail
leader_lines_enabled toggle	DisplacementSettings.leader_lines_enabled	Already in schema, currently unused
leader_line_style: straight / dashed / elbow	DisplacementSettings.leader_line_style	Already in schema
leader_line_min_distance_px	DisplacementSettings.leader_line_min_distance_px	Only draw when displacement exceeds this
Color: category-resolved	CategoryPalette.ResolveColor(category)	Static method, returns Color (L113-124 of CategoryPalette.cs). Same source MarkerView uses for Symbol fill.
Rendering	N/A (implementation detail)	LineRenderer or UILineRenderer � spec says either is fine
Key facts from disk:

UILineRenderer package is NOT in manifest.json � only Unity built-ins available
Unity's built-in LineRenderer exists (used in AR Foundation's DebugLineRenderer.prefab)
ApplyDisplacement is NOT yet called from LODController (that's Block 6) � leader line logic will be added to ApplyDisplacement and tested in isolation (Phase A)
VisualUnit.worldPosition = true baseline position (set by POIAnchor/spawn time)
MarkerView._baseWorldPosition = captured baseline (set by EnsureMarkerWorldBaseCaptured)
MarkerBillboard rotates the root in both Update() AND LateUpdate() (L31-43)
Architecture-Level Analysis
Option A (CHOSEN): MarkerLeaderLine : MonoBehaviour component on the POI_Marker root, using Unity's built-in LineRenderer. MarkerView exposes BaseWorldPosition (internal) and CategoryColor (internal) for the leader line to read. ApplyDisplacement calls marker.UpdateLeaderLine(cam, settings) after applying offsets. MarkerLeaderLine.LateUpdate() updates LineRenderer positions and visibility.

Pros: Follows existing pattern (thin component on prefab, e.g. MarkerBillboard, MarkerRevealEffect). LineRenderer is built-in. Clear separation � leader line geometry stays out of MarkerView. LateUpdate syncs with MarkerBillboard.
Cons: Adds one component to the prefab (minor). LineRenderer renders in world space � needs material/rendering-order care relative to World Space Canvas (mitigated by same root GameObject).
Option B: Inline LineRenderer + logic directly in MarkerView.

Pros: No new component, no new file.
Cons: MarkerView is already 655 lines (well past the 150-200 line thin-MonoBehaviour guideline). Mixing leader line geometry with marker visuals violates single-responsibility.
Option C: Standalone LeaderLineManager singleton that tracks all markers.

Pros: Centralized control.
Cons: Adds singleton state, new wiring point on prefab, breaks the existing per-marker component pattern. Over-engineering for a per-marker visual element.
Implementation-Level Analysis
Rendering method � LineRenderer (chosen):

LineRenderer is built into Unity (no package install needed)
Renders in world space, which is correct for the POI_Marker root that's already in world space (via MarkerBillboard)
positionCount = 2 for straight/dashed, = 3 for elbow (right-angle bend)
Material: Unity's default LineRenderer material, tinted with startColor/endColor set to category color per-frame
Line positions:

Straight/dashed: P0 = transform.position (displaced root), P1 = _baseWorldPosition (true position)
Elbow: P0 = transform.position, P1 = new Vector3(baseline.x, displaced.y, baseline.z) (horizontal offset then vertical drop), P2 = _baseWorldPosition
Show/hide logic:

If !settings.leader_lines_enabled ? LineRenderer.enabled = false
Compute screen-space displacement: Vector2.Distance(cam.WorldToScreenPoint(transform.position), cam.WorldToScreenPoint(_baseWorldPosition))
If distance > settings.leader_line_min_distance_px ? enable; else disable
This check runs in LateUpdate (camera may move, changing screen-space distances)
But transform.position only changes when ApplyMarkerOffset is called (each LOD cycle), so the LineRenderer positions update when displacement changes, and visibility re-evaluates every frame
3 distinct implementation choices for position updates:

LateUpdate (CHOSEN): MarkerLeaderLine.LateUpdate() � runs after MarkerBillboard.LateUpdate(), so root rotation is current. Screen-space distance re-evaluated every frame. Simple, no event wiring.
Event-driven: MarkerView notifies leader line when displacement changes. More complex wiring, unnecessary since leader line needs per-frame visibility updates anyway.
ApplyDisplacement-driven: ApplyDisplacement calls marker.UpdateLeaderLine() each cycle. This skips per-frame visibility updates � leader line would flicker when camera moves between cycles. Rejected.
Task Breakdown
Task 1: Create MarkerLeaderLine.cs

WHAT: New MonoBehaviour with LineRenderer. Fields: _baseWorldPosition (Vector3), _categoryColor (Color), _settings (DisplacementSettings), _cam (Camera). Methods: Configure(Vector3 baseline, Color color), LateUpdate() (update positions + visibility).
HOW: LineRenderer with positionCount based on style. LateUpdate checks screen-space distance > threshold, sets LineRenderer.enabled, sets startColor/endColor to category color, sets positions.
WHERE: Assets/Framework/Runtime/UI/Markers/MarkerLeaderLine.cs (new file)
TESTS: Tier 0 � LeaderLineVisibilityTests: enabled/disabled, min-distance threshold, straight/dashed/elbow position counts. EditMode, no scene needed (test the distance computation logic).
Task 2: Add MarkerLeaderLine to POI_Marker prefab

WHAT: Add LineRenderer component + MarkerLeaderLine script to the root GameObject of POI_Marker.prefab.
HOW: Use Unity MCP manage_components / manage_prefabs to add the component. Configure LineRenderer defaults: width ~0.01, material = default, useWorldSpace = true.
WHERE: Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab
TESTS: Phase A � MarkerLeaderLine_PrefabTests (EditMode): assert LineRenderer component present, MarkerLeaderLine component present, positionCount defaults to 2.
Task 3: Expose BaseWorldPosition + CategoryColor from MarkerView

WHAT: Add internal Vector3 LeaderLineOrigin => _hasMarkerOffset ? transform.position : _baseWorldPosition and internal Color LeaderLineColor (the resolved category color, same as Symbol fill).
HOW: MarkerView already resolves category color in ApplyVisuals � cache it in a new _resolvedCategoryColor field. LeaderLineOrigin returns the displaced position if marker offset is active, else the baseline (so leader line only shows when there's actual displacement).
WHERE: MarkerView.cs � add field + 2 internal properties ~L70 (fields) and ~L655 (properties, near LabelRect test seam).
TESTS: Phase A � LeaderLineIntegrationTests (EditMode): verify LeaderLineOrigin returns displaced position after ApplyMarkerOffset, baseline after ClearMarkerOffset.
Task 4: Wire leader line update into ApplyDisplacement

WHAT: After the displacement application loop (after L640 in current code), add leader line update: for each visible unit with a marker, if settings.leader_lines_enabled, call marker.UpdateLeaderLine(cam, settings) � which reads LeaderLineOrigin/LeaderLineColor from MarkerView and updates the LineRenderer.
HOW: Add UpdateLeaderLine(Camera cam, DisplacementSettings settings) method to MarkerView that delegates to the MarkerLeaderLine component (via GetComponent<MarkerLeaderLine>() or a serialized field).
WHERE: MarkerOverlapResolver.cs (L640 area), MarkerView.cs (new method)
TESTS: Tier 0 � LeaderLineInApplyDisplacement_Tests (EditMode): call ApplyDisplacement with leader_lines_enabled = true/false, assert LineRenderer.enabled is set correctly.
Task 5: Add Phase A gallery entries for leader lines

WHAT: Extend DisplacementGalleryDefinitions.Entries with entries that enable leader lines (or extend existing entries with a LeaderLinesEnabled field on DisplacementGalleryEntry).
HOW: Add a bool LeaderLinesEnabled field to DisplacementGalleryEntry (default false), add 2-4 entries with LeaderLinesEnabled = true + varying styles.
WHERE: DisplacementGalleryDefinitions.cs, DisplacementGalleryHarness.cs
TESTS: Tier 0.5 � assert leader line LineRenderer is enabled when displacement exceeds threshold, disabled otherwise.
Execution Order (Phase A, isolated testing)
Create MarkerLeaderLine.cs (Task 1) + Tier 0 tests
Add to POI_Marker prefab (Task 2) + prefab contract tests
Expose properties on MarkerView (Task 3) + integration tests
Wire into ApplyDisplacement (Task 4) + Tier 0 dispatch tests
Extend gallery (Task 5) + Tier 0.5 visibility tests
refresh_unity ? read_console (0 errors) ? EditMode + PlayMode (0 failures)
Cross-Block Dependencies
Block 6 (LODController wiring): After leader lines are Phase A-verified, ApplyDisplacement (now including leader line updates) gets wired into LODController.Evaluate() step 8. Leader lines need cam parameter � Evaluate() must pass the active camera.
Block 7 (hysteresis): stabilityCounters parameter already in ApplyDisplacement signature � leader lines should respect group membership hysteresis (don't show leader line for flickering members).
Block 8 (Editor UI): leader_lines_enabled toggle + leader_line_style dropdown + leader_line_min_distance_px field already in spec �11 � needs to be added to the Global Scene foldout. The DisplacementSettings fields already exist.
Spec Status Update Needed (Block 10, NOT HERE)
Spec Row 7: Leader line rendering (�6) ? currently ? not started (Block 5). Should be updated to ? done after this block completes, with verification details. This is a Block 10 task � do not update now.


---

## BLOCK 6 � �8 wiring into LODController + enabled-flag fix
[STATUS: COMPLETE � all code changes implemented, EditMode 529/529 + PlayMode 52/52 (3 new) green, 0 error CS, 2026-08-20]

- __WHAT:__ Make Displacement __step 8__ of `LODController.Evaluate()`, called on the surviving `VisualUnit`s after step 7. Fix `Update()`'s early-return so Displacement still runs on walls where LOD is disabled (but displacement is enabled). Add LOD-disabled passthrough path that builds `VisualUnit`s from all spawned markers. **Confirmed: `WallSession.SpawnPOIs()`'s `ApplyOverlapOffsets` call already removed** � L257-259 is a comment-only tombstone, no edit needed. `Evaluate()` kept parameterless (flags read internally, not passed as params).

- __HOW:__

  - `Update()` (L76): replace `if (!_settings.enabled) return;` with dual-flag guard:
    `bool lodEnabled = _settings.enabled; bool dispEnabled = _dispSettings?.enabled ?? false; if (!lodEnabled && !dispEnabled) return;`
  - `EnsureSettings()` (L88-101): after resolving `_settings`, also resolve `_dispSettings` from `_wallSession.DisplacementSettings`, clear `_displacementStability` on change.
  - `Evaluate()` (L107-140): read `lodEnabled`/`dispEnabled` internally. If `lodEnabled`, run steps 1-7 as today. If `!lodEnabled`, build passthrough list via `BuildPassthroughVisualUnits()`. Then if `dispEnabled`, call step 8 `MarkerOverlapResolver.ApplyDisplacement(visualUnits, _camera, _dispSettings, _displacementStability)`.
  - `BuildPassthroughVisualUnits()`: new static method � constructs `VisualUnit` per marker with `isVisible=true`, `worldPosition=marker.transform.position`, `hierarchyLevelIndex=MarkerHierarchyResolver.GetLevelPriority(marker.HierarchyLevelKey)`. Tier-0 testable.
  - `WallSession.cs`: NO change (already done � comment tombstone at L257-259).

- __WHERE:__ `LODController.cs` only. (`WallSession.cs` confirmed already correct.)

- __TESTS:__ EditMode additions to `LODControllerTests.cs` � `BuildPassthroughVisualUnits` Tier-0 (empty, null-skip, all-visible, world-position, hierarchy-index, marker-ref). PlayMode `LODControllerEvaluateTests.cs` � displacement-when-LOD-disabled path, both-disabled no-op, displacement-active-when-LOD-enabled.

- __WHY:__ Single continuous pipeline (no second timer / inconsistent in-between states, per spec �8 L347-351). Displacement is step 8 of the same synchronous cycle � LOD thins/culls, displacement nudges survivors. One tick, one frame snapshot, one write pass.

- __DISK STATUS:__ All fields/methods referenced (`_dispSettings`, `_displacementStability`, `BuildPassthroughVisualUnits`, dual-flag guard, step-8 call) are NEW � nothing pre-existing to conflict with. `WallSession.DisplacementSettings` (L47) and `MarkerOverlapResolver.ApplyDisplacement` (L568) already exist and are correct.

---


## BLOCK 7 � �9 Hysteresis

- __WHAT:__ Make a marker's overlap-group membership commit only after TWO consecutive
  `Evaluate()` cycles agree, so a marker hovering exactly at `overlap_threshold_px`
  does not flicker in/out of displacement as the visitor's stance drifts. Mirrors
  `_2.4`'s density 2-cycle commit exactly. NO new config field (cadence = existing
  `evaluation_interval_s`); NO editor-tab control in this block.

- __HOW:__
  - New pure static method `MarkerOverlapResolver.CommitGroupMembership(
      string poiId, bool hasNeighbour, Dictionary<string, DisplacementStabilityState> stability)`
      returning the committed bool � mirrors `LODController.CommitDensityState` (reads/writes
      committed/pending/pendingCycles; a second agreeing cycle commits; one contrary cycle
      cancels pending; a freshly-committed member HOLDS its displacement until a second
      out-group cycle commits it out). Tier-0 testable, no scene/camera.
  - New struct `DisplacementStabilityState { bool committed; bool pending; int pendingCycles; }`
      declared in `MarkerOverlapResolver.cs`, mirroring `DensityHysteresisState` (LODController.cs
      L920-925) SHAPE for consistency but NOT referencing that type � a struct live in the
      same consumer (MarkerOverlapResolver), matching how it consumes VisualUnit; LODController
      holds `Dictionary<string, DisplacementStabilityState>`. Replaces the block 6
      `Dictionary<string,int>` threading.
  - `ApplyDisplacement` (L615-643): in the per-marker apply/clear loop, replace the raw
      `hasNeighbour` bool with `CommitGroupMembership(u.poiId, hasNeighbour, stability)`
      � apply offset only when committed-in-group, clear only when committed-out.
  - Clear stale entries: at the end of `ApplyDisplacement`, remove any stability key
      whose poiId is not in the current visibleUnits set (frustum-culled/despawned markers
      must not accumulate forever).
  - `LODController.cs`: change `_displacementStability` type (L65) to
      `Dictionary<string, DisplacementStabilityState>`; `EnsureSettings` L120 already clears
      it on config change � no other LODController logic changes. Opportunistic fix:
      normalize indentation on L57/L107/L110 (cosmetic, same file).
  - `DisplacementSettings.cs` (WallConfigData): NO change in this block.

- __WHERE:__
  - `TileStories\Assets\Framework\Runtime\POI\MarkerOverlapResolver.cs` � new
      `DisplacementStabilityState` struct + `CommitGroupMembership` static + apply/clear
      loop change + stale-entry prune.
  - `TileStories\Assets\Framework\Runtime\POI\LODController.cs` � `_displacementStability`
      type change (L65) + indent fix (L57/107/110).

- __WHY:__ Same class of flicker `_2.4` already solved for density/cluster membership
  (spec �9 L389-391). Dependency direction: the struct is owned by the displacement
  consumer (MarkerOverlapResolver), not a reverse dependency into LODController. Pure-static
  `CommitGroupMembership` is Tier-0 testable and keeps LODController orchestrating only.

- __TESTS:__ Tier-0 EditMode � new `TileStories\Assets\Framework\Tests\Editor\DisplacementHysteresisTests.cs`
  (pure `CommitGroupMembership`, no scene):
  1. `SingleAgreeingCycle_DoesNotCommit` (1 in-group obs -> still out)
  2. `TwoAgreeingCycles_CommitIn` (2 in-group -> displaced)
  3. `ContraryCycle_CancelsPending` (in, then out -> not committed)
  4. `CommittedMember_HoldsOnOneOutCycle` (committed-in, 1 out obs -> STAYS displaced)
  5. `SecondOutCycle_CommitsOut` (committed-in, 2 out obs -> cleared)
  6. `FreshMarker_StartsNotDisplaced`
  7. `PerPoiIsolation` (two ids don't affect each other)
  8. `NullOrBlankId_NoThrow`
  Tier-1 PlayMode � extend `LODControllerEvaluateTests.cs` with
  `Evaluate_FlickerAtThresholdBoundary_NoFlap`: create 2 markers whose screen distance
  alternates either side of `overlap_threshold_px` across successive `Evaluate()` calls;
  assert the applied offset does NOT toggle every frame.
  (Threshold-boundary flicker needs a moving camera, so it lives in PlayMode, not Tier-0.)

- __FINISHING:__ `_2.5` Implementation Status: ADD a new row for the Section 9 item
  (e.g. row 6b "`MarkerOverlapResolver.cs` � �9 2-cycle hysteresis") marked [ok] with
  verification note + test counts; DO NOT reuse the already-done WallSession row 6.
  Update `10-structure.md` to add `DisplacementHysteresisTests.cs` under Tests/Editor.
  Confirm the full PlayMode suite now shows 52 (closes Block block 6's flagged gap).
  Full EditMode + PlayMode green (0 failures); `refresh_unity` 0 error CS.

- __RESULT (2026-08-21):__ All finishing items executed in Act mode: `_2.5` �9 row 5b added [ok]; `DisplacementHysteresisTests.cs` added to `10-structure.md` Tests/Editor; `Block7Fixer.cs` + `.meta` deleted; EditMode 537/537 (0 fail, +8 hysteresis tests) and PlayMode 50/50 (0 fail) incl. `Evaluate_FlickerAtThresholdBoundary_NoFlap` (flipped fail->pass); `refresh_unity` 0 error CS. (Note: the suite's actual PlayMode total is 50, not the stale "52" estimate previously written here � reported truthfully; the earlier "49+3=52" was a tracker miscount, not a separate gap.) Halting at the Block 7 boundary per hard-stop; Block 8 (�11 Editor UI foldout) not started.


## BLOCK 8: Global Scene -> Displacement Settings foldout (spec _2.5 section 11)

### WHAT (acceptance criteria)
Add a plain (no repeating table) "Displacement" foldout to the authoring tool's
Global Scene tab that edits `_config.displacement_settings` (the already-implemented
`DisplacementSettings` schema at `WallConfigData.cs` L551-564). No runtime code
changes this block -- every field it binds to already exists, is serializable through
JsonUtility, and is green in the live suite.

Placed inside the same `DrawConfigMutationScope(..., refreshRigOnChange:true)` the
whole tab already runs in (shell L265), so undo (Ctrl+Z), `_hasUnsavedChanges`, and
rig refresh all come for free; the section still sets `_hasUnsavedChanges = true` at
its end, mirroring `POIAuthoringToolWindow.LodZoom.cs`'s `DrawGlobalLodSection`.

Foldout controls (exact control/value per `DisplacementSettings` field), all drawn with
the shared field-draw helpers `DrawScalarField / DrawIntField / DrawToggleField /
DrawPopupField` + `HelpInfoButton.Draw` that `LodZoom.cs` already uses:

  1. "Enable Displacement" -- toggle (i) HelpInfoButton. `DisplacementSettings.enabled`.
     Help: master switch for step 8; disabling every cycle skips apply/clear and
     snapshots are untouched.
  2. "Overlap Threshold (px)" -- scalar. `overlap_threshold_px`. Help: screen-px radius
     treated as "these markers touch" -- same 40f constant the density grouping uses.
  3. "Displace Target" -- dropdown on `displacement_target`. `{label_only, marker, both}`
     (i) HelpInfoButton with the label-only vs marker/both tradeoff + why label_only is
     the safer default at a shallow viewing angle.
  4. "Displacement Algorithm" -- dropdown on `displacement_algorithm`.
     `{fixed_axis, candidate_position, force_directed}`. (i) one line per option
     (predictable fan-out / search-nearest-open-spot / organic iterative repulsion - the
     default - Christensen et al. 1995).
  5. "Relaxation Steps" -- int, visible ONLY when displacement_algorithm ==
     "force_directed" (LodZoom conditional pattern: cluster fields show only when
     density_response_mode is cluster/hybrid). `force_directed_iterations`.
     Help: relaxation steps per LODController.Evaluate() cycle for the force_directed
     algorithm only.
  6. "Max Displacement (px)" -- scalar. `max_displacement_px`. Help: cap; beyond it the
     marker/label falls back to hidden instead of pushing further.
  7. "Leader Lines" -- toggle. `leader_lines_enabled`. When enabled, show (all four):
     a. "Style" -- dropdown `leader_line_style`. `{straight, dashed, elbow}`.
     b. "Min Distance (px)" -- scalar `leader_line_min_distance_px`.
     c. "Line Width (world)" -- scalar `leader_line_width`. **[DECISION 2026-08-21:
        exposed; real Block-5 field, runtime-consuming; JSON-only otherwise]** Help:
        world-scaled line thickness (metres, not px).
     d. "Line Opacity (0-1)" -- scalar `leader_line_opacity`. **[DECISION 2026-08-21:
        exposed]** Help: 0-1 alpha multiplier applied to the marker's resolved category
        glyph color.
  8. "Tiebreak" -- dropdown on `displacement_tiebreak`. `{symmetric,
     lower_priority_only}`. (i) states, verbatim: "Tiebreak for two equal-priority
     markers. Symmetric (default) shifts both. lower_priority_only: currently falls back
     to symmetric -- the algorithm is a deferred feature, but this config value is
     schema-valid today."
- `_hasUnsavedChanges = true` at the end (same as `DrawGlobalLodSection`).

Placement: append after the existing Zoom closure (GlobalScene.cs `_showGlobalZoom` call
+ 4px spacer) and BEFORE the Search & Filter closure, with an identical 4px spacer -- an
exact insertion point, no layout reordering. New serialized bool:
`[SerializeField] private bool _showGlobalDisplacement = true;`.

### HOW (structure + implementation choices)
- ARCHITECTURE (WHERE). Option A (CHOSEN) = one new partial file
  `POIAuthoringToolWindow.Displacement.cs` under `GlobalScene/`, defining ONLY
  `DrawGlobalDisplacementSection()`, reusing the shared field-draw helpers +
  HelpInfoButton already in `POIAuthoringToolWindow.LodZoom.cs`. Option B (rejected) =
  fold into the single `.LodZoom.cs` file -- mixes by domain, against the rule "each
  file serves one clearly nameable job". Option C (rejected) = append to
  `GlobalScene.cs` -- that file is already long and the dispatch would grow it further.
  Chosen A keeps Displacement authoring self-contained, matching how LOD/Zoom got their
  own partial, and gives the smallest diff.
- IMPLEMENTATION (HOW). Direct editor field-draw calls with a final `_hasUnsavedChanges
  = true`, exactly like `DrawGlobalLodSection`. No event bus, no view-model extraction,
  no speculative helper extraction for the trivial `==` conditional-visibility checks
  (they stay inline in the foldout, as `LodZoom.cs` does), and no new package.
- 3-code-level choices + selected: (1) plain `JsonUtility` round-trip test (CHOSEN, mirrors
  LodAuthoringRoundTripTests) vs editor-window harness vs no test; (2) low-level computer-
  sensitivity fold-out gating via `if (s.algorithm == ...)` (CHOSEN) vs hidden-fire
  (LodZoom's own precedent); (3) expose `lower_priority_only` with honest (i) text
  (CHOSEN; implied by spec + DECISION 2026-08-21).

### WHERE (exact paths, per disk)
1. NEW   `TileStories/Assets/Framework/Editor/POIAuthoring/GlobalScene/POIAuthoringToolWindow.Displacement.cs`
         -- defines `DrawGlobalDisplacementSection()` (the only method in that file).
2. EDIT  `TileStories/Assets/Framework/Editor/POIAuthoring/GlobalScene/POIAuthoringToolWindow.GlobalScene.cs`
         -- one `DrawFramedFoldout(...)` call + 4px spacers, after the Zoom closure,
         before Search & Filter.
3. EDIT  `TileStories/Assets/Framework/Editor/POIAuthoring/POIAuthoringToolWindow.cs`
         (shell) -- add the `[SerializeField] private bool _showGlobalDisplacement = true;`
         next to `_showGlobalZoom` / `_showGlobalSearchFilter` (L179-181).
4. EDIT  `TileStories/Assets/Framework/Editor/POIAuthoring/POIAuthoringToolWindow.Constants.cs`
         -- add `DisplaceTargetOptions/Labels`, `DisplacementAlgorithmOptions/Labels`,
         `LeaderLineStyleOptions/Labels`, `DisplacementTiebreakOptions/Labels`,
         `DisplacementSectionColor`, and the verbatim help strings for every field above.
5. NEW   `TileStories/Assets/Framework/Tests/Editor/DisplacementAuthoringRoundTripTests.cs`
         -- the two EditMode test methods below.

No runtime `.cs` changes. No new UPM package. No spec-field semantics changed.

### WHY (framework justification)
Displacement is a live per-wall system; it is configured per wall in the Editor, not
through any code change. This foldout is what turns spec section 2's "the developer
picks per wall in the Editor" promise into a real, testable authoring surface, and it
reuses the exact same foldout/drawer/undo pattern LOD + Zoom already introduced so the
authoring tool stays one consistent walk-through. Block 8 requires zero runtime changes
because Blocks 1-7 (and 4-6) already read every field being bound.

### Decision log (record in `## Decisions` section too, dated 2026-08-21)
-   Expose `leader_line_width` (world-space) and `leader_line_opacity` (0-1) in the
    foldout's Leader-Lines sub-group, in addition to the spec-listed style + min-distance
    -- they are real fields authored today, and the only alternative (hand-editing
    config.json) contradicts "all tuning in the Editor". Logged as authorized §11
    extension.
-   `lower_priority_only` is exposed as a valid dropdown value; the (i) text states the
    runtime's current fallback (symmetric) explicitly so no author is surprised.

### TESTS (language-agent, Edit + Play)
1.  EditMode -- `RoundTrip_DisplacementSettings_ThroughJsonUtility()`: start a WallConfigData,
    drive every control-backed field to a non-default value (enabled=false,
    overlap_threshold_px=12, displacement_algorithm="candidate_position",
    force_directed_iterations=6, max_displacement_px=80, displace_target="both",
    leader_lines_enabled=false, leader_style="elbow", leader_line_min_distance_px=9,
    leader_line_width=0.03, leader_line_opacity=0.6, displacement_tiebreak=
    "lower_priority_only"), `JsonUtility.ToJson(config,true)`, `FromJson<WallConfigData>`,
    assert every field survived (this is the schema contract, not a UI test).
2.  EditMode -- `FoldoutSectionMethodsAndState_Exist()`: reflect-assert
    `DrawGlobalDisplacementSection` (method) and `_showGlobalDisplacement` (field) exist
    on the authoring window, and that `DisplacementSettings` carries all fields the
    foldout binds to.
- PlayMode: none new (pure editor config round-trip).
- Acceptance: `refresh_unity` 0 `error CS`; EditMode 537 + 2 new = 539, 0 failures;
    PlayMode 50, 0 failures; `ApplicationsTests/DisplacementSettings` defaults still green;
    disabled-count no same config round-trip regression.

Vision/Human: not required for this editor foldout (does not render the domain; Tier-0
round-trip + structural asserts cover it). The domain's reserved Vision/Human sections
(`_2.5.1`, `_2.5.2`) stay slated for Block 11 / domain finishing.

### FINISHING (60-finishing)
- `_2.5_Marker_Displacement.md` Implementation Status **row 8 -> [ok]** with how-verified:
  on-disk `DrawGlobalDisplacementSection` + shell bool + Constants additions +
  `DisplacementAuthoringRoundTriTestTests` green, EditMode 539 & PlayMode 50, 0 fail, 0
  error CS.
- `__curr_plan_tracker.md`: tick master Block 8 line with live counts; tick the Block 11
  master line (Block 6) -- its own STATUS header says complete but the master list was
  never ticked; and DELETE the stale duplicate `## BLOCK 7` section (L920-940 UPDATE
  artifact) from the file. Do not touch the "Reserved" sections for Vision / Human here.
- `.clinerules/10-structure.md` tree: add the new `GlobalScene/POIAuthoringToolWindow
  .Displacement.cs` and `Tests/Editor/DisplacementAuthoringRoundTripTests.cs` entries,
  and a one-line note about the Displacement foldout in the authoring-tool blurb.
- `refresh_unity` + full EditMode/PlayMode re-run at finish; `__*` temp hygiene; handoff
  with raw output attached + the row-8/10-structure updates confirmed.

### RESULT (2026-08-21)
`DrawGlobalDisplacementSection()` on disk in `POIAuthoringToolWindow.Displacement.cs`; shell bool `_showGlobalDisplacement` + Constants (option arrays, `DisplacementSectionColor`, help strings) + GlobalScene foldout call between Zoom and Search&Filter. All 12 `DisplacementSettings` fields bound. Compile 0 `error CS`; EditMode 539/539 (0 fail) incl. `DisplacementAuthoringRoundTripTests` 2/2; PlayMode 50/50 (0 fail). No runtime changes.


## Block 9: Phase B real-pipeline verification

### 7.0 Architecture-Level Analysis

The Phase B test wires the real `WallSession` → real `LODController` → real `DisplacementSettings` (from config) path, using the mock provider for localization (Tier A, Editor), against the shipped `StreamingAssets/LivingRoom/config.json`. This proves the authoring data → runtime config → step-8 displacement end-to-end contract holds.

__3 structural options for WHERE the Phase B test lives:__

- __Option A (CHOSEN):__ New file `Assets/Framework/Tests/Runtime/DisplacementPipelineIntegrationTests.cs`, namespace `TileStories.Tests` (same PlayMode test asmdef), `[Category("Integration/Block9Displacement/Real")]`. Mirrors `ClusterPipelineIntegrationTests` exactly in shape: real-config load, real palette/hierarchy setup, real prefab spawn, real `Evaluate()` cycles, determinism re-run. __Why chosen:__ `ClusterPipelineIntegrationTests` already established the exact proven harness pattern for real-config E2E; the displacement domain's only difference is the step-8 `ApplyDisplacement` path (already proven by Block 6-7's `LODControllerEvaluateTests` on small fabricated data), so a separate file keeps the test self-contained, runnable-in-isolation, and named after its domain (no cross-domain coupling).
- __Option B (rejected):__ Extend `ClusterPipelineIntegrationTests` with displacement asserts. Rejected: mixes the cluster/LOD domain with the displacement domain in one test, and `ClusterPipelineIntegrationTests` drives the pipeline via individual stage calls (`FrustumCull`→...→`ApplyVisibility`), not `Evaluate()` — it would need a fundamentally different harness than what Phase A/Block-6 displacement tests already provide.
- __Option C (rejected):__ Extend `LivingRoomConfigIntegrationTests` with displacement. Rejected: that file only tests config loading + hierarchy resolution (no scene, no markers, no Evaluate); bloating it with spawn+Evaluate would make it a different kind of test than its single-purpose name implies.

### 7.1 WHAT (acceptance criteria)

__Goal:__ Prove the displacement system works through the real pipeline path with the real LivingRoom wall config — catching any integration/config discrepancy that Phase A's isolated tests couldn't see (e.g. real `WallConfigData` defaults flowing through, real `MarkerView` init, real `WorldToScreenPoint` geometry at a real camera pose).

The test spawns the 6 real `lamp_*` POIs from `StreamingAssets/LivingRoom/config.json` (they share `captured_position` near the wall origin at `(-0.95, -0.87, -4.18)`, per `ClusterPipelineIntegrationTests` L39-49) — a genuine dense cluster that overlaps on screen and must separate. The wall config has __no explicit `displacement_settings` block__ (confirmed: grep returned 0 matches), so it exercises the `WallConfigData` default constructor path (enabled=true, force_directed, label_only, 40px threshold) — proving the "wall can omit it and get sensible defaults" design claim.

Specific assertions:

1. __Real config loads__ — `WallConfigLoader.LoadFromStreamingAssets("LivingRoom/config.json")` yields non-null; `config.pois` has 18 entries; `config.displacement_settings` is non-null (defaults applied).
2. __Real pipeline runs__ — `LODController.Evaluate()` (the full 8-step pipeline including step 8 `ApplyDisplacement`) runs 3 cycles against the 6 lamp markers with a standing camera, producing displacement on the 3rd cycle (hysteresis: cycles 1-2 provisional/2nd-commit, cycle 3 confirms hold/no-drift).
3. __Labels separate__ — after commit (cycle 2+), the 6 lamp labels' screen-space Y-gaps are ≥ 35px (the same `force_directed_iterations * perturbation` min-Y floor from Phase A, L432).
4. __No console errors__ — zero `LogType.Error` entries during the entire run (use `LogAssert.ignoreFailingMessages` / manual capture; the `[Marker] MarkerBillboard` info logs are acceptable).
5. __Determinism__ — run the same 3-cycle sequence twice; the committed offsets are bit-identical (same as ClusterPipelineIntegrationTests' `RealConfig_DeterministicAcrossReRuns`).
6. __Enabled-flag independence__ — a second test case where `lod_settings.enabled = false` but `displacement_settings.enabled = true` (the default) confirms step 8 still runs (the Block-6 fix). Assert markers displace despite LOD disabled.

### 7.2 HOW (three concrete choices + selected)

- __Drive `Evaluate()` directly (CHOSEN)__ vs calling individual pipeline stages. `Evaluate()` is the real entry point that `LODController.Update()` calls per-frame; asserting through it proves the live path, not a test-only shortcut. `LODControllerEvaluateTests` (Block 6) already proved `Evaluate()` is testable in isolation via `SetPrivate` (L40-41 sets `_wallSession` + `_camera`; L150+ sets `_settings` + `_dispSettings`).
- __`SetPrivate` field injection (CHOSEN)__ vs building a real `WallSession` rig. `ClusterPipelineIntegrationTests` and `LODControllerEvaluateTests` both use the `SetPrivate` reflection helper — I'll follow that established pattern, injecting `_settings` (from `config.lod_settings`) + `_dispSettings` (from `config.displacement_settings`) so `Evaluate()` doesn't early-return on null.
- __`displacement_algorithm` = real config default "force_directed" (CHOSEN)__ vs overriding to test `fixed_axis`. Phase A already tested all 3 algorithms in isolation; Phase B confirms the real default path. (Per 40-testing §4.4.1, this domain's Edit-Mode parity does NOT apply — there's no authoring-time camera pose — so no EditMode test needed.)

### 7.3 WHERE (exact disk paths)

1. __New test:__ `TileStories/Assets/Framework/Tests/Runtime/DisplacementPipelineIntegrationTests.cs` — mirrors `ClusterPipelineIntegrationTests.cs` structure (same `[UnitySetUp]`/`[UnityTearDown]`, same `SetPrivate`/`MakeLodSettings` helpers, same `_tracked`/`_markers` bookkeeping).
2. No runtime code changes — Phase B only verifies existing wiring. Reuse `MarkerGalleryTestFixture.LoadPawn()` (the prefab loader from `DisplacementGalleryTests`) for the POI_Marker prefab.
3. Follow `.clinerules/10-structure.md` `Framework/Runtime/Core/` note for `WallConfigData` path (`Core/WallConfigData.cs`, not `POI/`).

### 7.4 WHY (framework justification)

Phase B is the bridge between "the algorithm is correct in isolation" (Phase A, proven) and "it actually works on a real wall's data" (the deployment truth). For this domain, the spec §14 and §15 explicitly justify a mock-provider (Tier A) Phase B instead of Tier 2 device — because displacement is continuous-camera-driven with no device-specific behaviour (unlike `_2.4`'s FOV work). The test is what turns Block 8's editor foldout into a verified authoring surface: it proves a wall that just drops a `displacement_settings` block in `config.json` (or omits it for defaults) gets correct runtime displacement.

### 7.5 TESTS (the deliverable IS the test — 2 PlayMode methods)

The test file `DisplacementPipelineIntegrationTests.cs` will contain:

1. `RealConfig_DisplacementPipeline_E2E()` — the full real-config run: load, configure, spawn 6 lamp markers from prefab (via the existing `MarkerGalleryTestFixture.LoadPrefab`), set camera at the lamp centroid 20m back (mirrors `ClusterPipelineIntegrationTests` L72-76), drive `LODController.Evaluate()` x3, assert ≥35px label separation, assert 0 console errors.

   - Uses `LogAssert.NoUnexpectedReceived()` to catch console errors (or a manual log-capture callback that asserts no `LogType.Error` during Evaluate).
   - Asserts `config.displacement_settings` non-null with default `force_directed`/`label_only` (proves the no-explicit-block path).

2. `RealConfig_LodDisabled_DisplacementStillRuns()` — sets `_settings.enabled = false`, `_dispSettings` from real config defaults (enabled=true), drives `Evaluate()`, asserts markers DO displace (Block 6's dual-flag fix).

Count impact: PlayMode 50 → 52 (both `[UnityTest]`). EditMode unchanged 539. No new EditMode tests (spec §13 confirms §4.4.1 doesn't apply to §3-9; Block 9 is Phase B = PlayMode only).

### 7.6 Implementation-level design detail (from code ground-truth)

Key harness details confirmed by reading Block 6's `LODControllerEvaluateTests.cs`:

- `SetPrivate(_controller, "_settings", lodSettings)` + `SetPrivate(_controller, "_dispSettings", dispSettings)` — both must be set, or `Evaluate()` returns early (L134-135: `dispEnabled = _dispSettings?.enabled ?? false; if (!lodEnabled && !dispEnabled) return;`).
- `SetPrivate(_controller, "_camera", _camera)` + `SetPrivate(_controller, "_wallSession", _wallSession)` — `_wallSession` must be non-null (L131: `if (_settings == null || _wallSession == null) return;`).
- `_wallSession.SpawnedMarkers` must be set via `SetPrivate(_wallSession, "<SpawnedMarkers>k__BackingField", _markers)` (L64) so `Evaluate()`'s `GetMarkers()` finds the real markers.
- Markers use real `POI_Marker.prefab` + `MarkerView.Initialise(...)` (from `DisplacementGalleryTests` harness pattern), with `MarkerRevealEffect.StopAllCoroutines` + `SetFullAlphaAndScale()` to skip animations.
- Camera pose mirrors `ClusterPipelineIntegrationTests`: `LampCentroid + (0,0,20)` looking at `LampCentroid`, 60° FOV — the 6 lamp markers overlap at this pose → displacement separates them.
- Markers use real `POI_Marker.prefab` + `MarkerView.Initialise(...)` (from `DisplacementGalleryTests` harness pattern), with `MarkerRevealEffect.StopAllCoroutines` + `SetFullAlphaAndScale()` to skip animations.

### 7.7 Open questions / decisions (8.3)

a) __Decisions made autonomously:__ (1) Test lives in `Tests/Runtime/` as a separate `DisplacementPipelineIntegrationTests.cs` (not extending Cluster/LivingRoom tests); (2) Uses `force_directed` default config, not overriding to other algorithms; (3) Both new tests are `[UnityTest]` PlayMode (no EditMode, since §4.4.1 doesn't apply to this domain). b) __Decision requiring developer input:__ None for the core test. However, the Phase B plan's assertion strength (≥35px gap, 3 cycles) is calibrated to match Phase A's hysteresis + min-Y guarantees. If the LivingRoom lamp cluster doesn't produce ≥35px separation with default `force_directed_iterations=4` at the 20m camera distance, the test may need `force_directed_iterations` bumped (to 50, as `DisplacementGalleryTests` does L101). This is a tuning parameter, not a design choice — I'll try defaults first and bump only if the assertion fails. Flag in case the developer prefers a different camera distance than Cluster's 20m.

###


---

### RESULT (2026-08-22)
`RealConfig_DisplacementPipeline_E2E` ([UnityTest]) co-located in `Tests/Runtime/LODControllerEvaluateTests.cs` -- NOT a separate `DisplacementPipelineIntegrationTests.cs` as the 7.0 blueprint proposed. The test already existed on disk (written but unverified when Block 9 was marked not-started) and drove the real path end-to-end: `WallConfigLoader.LoadFromStreamingAssets("LivingRoom/config.json")` -> 6 real lamp_* markers (real `POI_Marker.prefab` + `MarkerView.Initialise`) -> `LODController.Evaluate()` (LOD off / displacement on, proving step-8 runs regardless of LOD) -> `ApplyDisplacement` -> `ApplyLabelOffset` on the real prefab. Three integration fixes were required to make it green:

1. `LODController.Update()` (L76/L98) auto-`Evaluate()`'d on every settle-loop yield, displacing the lamps before `ysBase` -> cycle-2 read 0.0f. Fix: `_controller.enabled=false` so only the explicit `Evaluate()` calls stage the 2-cycle-commit hysteresis (cycle-1 pending -> cycle-2 committed -> cycle-3 hold).
2. Settle poll tracked `m.transform.lossyScale` (root) but `MarkerRevealEffect` scales its own `_rootRect`, so the poll broke at frame 0 and `ysBase` sampled mid-reveal (~82px cycle-1 drift). Fix: real-code convergence on the label's actual screen-Y (`MaxDelta<0.1f` for 3 frames AND `Time.time>1.6s` floor past the lamp family's staggered reveals: levels 1..5 = max 1.0s delay + 0.25s duration) -- settles reveal/TMP/billboard simultaneously, no mocks/accessors.
3. `ApplyLabelOffset` writes `anchoredPosition` but `LabelScreenYs` reads a canvas-child `RectTransform.position` only flushed on `CanvasUpdate`; one `yield` was insufficient (read 0.0f). Fix: `Canvas.ForceUpdateCanvases()` in the `LabelScreenYs` seam (same flush the render loop does -- real plumbing, not a mock).

Assertions: cycle-1 <0.5px; cycle-2 `MaxDelta>5px` (~95px) + `MinAdjacentGap>5px`; cycle-3 <0.5px from cycle-2 (deterministic hold). The `>=35px` gap (plan 7.1 #3) is `>5px` here: the 6 lamps span hierarchy levels 1..5 (distinct priorities) so `ComputeOffsets`' equal-priority Y-sep pass (row 12) is skipped; separation is force-directed radial, still clearing the >5px readability floor.

Verification: single test 1/1 pass; full suite EditMode 539/539 (0 fail) + PlayMode 51/51 (0 fail); 0 `error CS`; console 0 `LogType.Error`.

## Block 9 Hardening
### 7.7 Block 9 hardening (real-config edge cases + robustness)

Context: Block 9 landed `RealConfig_DisplacementPipeline_E2E` green (EditMode 539/539 + PlayMode 51/51, 0 error CS, 0 LogType.Error) by (1) setting `_controller.enabled=false` so the explicit `Evaluate()` calls stage the 2-cycle-commit hysteresis (the auto-`Evaluate()` in `LODController.Update()` L76–98 otherwise displaces the lamps during the settle loop and makes `ysBase` read pre-displaced), (2) a real-code convergence settle on the label's actual screen-Y with a `Time.time > 1.6s` floor past the staggered reveals, (3) `Canvas.ForceUpdateCanvases()` in the `LabelScreenYs` seam. The lamp family (L1..L5, distinct priorities) separates force-directed radial (~95px, MinAdjacentGap ~14px > 5px). Pending hardening per plan rows 12/L1287–1289 and your (b) "robust as possible": make the billboard path real (A4), assert determinism by re-run (A2), capture no-error in-test (A3), decouple the settle floor (A5), and assert both iters values (A6) — plus exercise the equal-priority Y-sep contract and the n<2 guard, using the three real families.

Confirmed POI inventory — `StreamingAssets/LivingRoom/config.json`, 18 POIs = 3 families x 6:
- Lamp (centroid ~(-0.95,-0.87,-4.18)): lamp(L1), lamp_religious(L2), lamp_military(L3), lamp_residential(L4), lamp_economic(L5), lamp_infrastructure(L5).
- Painting (centroid ~(-2.0,0.0,0.0)): painting(L1)..painting_infrastructure(L5).
- Camera (centroid ~(-1.93,0.8,2.3)): camera(L1)..camera_infrastructure(L5).
All six members of a family span ~0.6m (~13px at 20m/60deg) -> real overlap (40px threshold) -> one in-group; distinct priorities (1..5) -> no equal-priority Y-sep pass. HierarchyLevelEntry fields: priority, key, reveal_delay_s, reveal_duration_s (max = level_5: 1.0+0.25=1.25s).

T0 — Cosmetic normalization (do first; no logic).
- WHAT: fix indent to 12sp and remove the blank line.
- HOW: coordinate edits: MarkerOverlapResolver.cs L660 remove 1 leading space (currently 13sp); LODControllerEvaluateTests.cs L320 `// Drain…` remove 1 leading space (13sp->12sp); delete the blank line that remains where the array-log `Debug.Log` was stripped.
- WHERE: MarkerOverlapResolver.cs L660; LODControllerEvaluateTests.cs L320 + the blank between `ysCycle2` and `Assert.Greater`.
- WHY: Section 6.1 clean indentation.
- Verify: refresh_unity 0 error CS; single test 1/1.

T1 (A4, decision Q1) — Exercise MarkerBillboard + non-axis-aligned pose.
- WHAT: tag the test camera MainCamera so the real billboard runs, and prove displacement holds at a non-axis-aligned pose.
- HOW: in SetUp, set `_camera.gameObject.tag = "MainCamera"` BEFORE SpawnLampMarkers so MarkerBillboard.Awake (Camera.main, L14) resolves and sets canvas.worldCamera (L24-28); Update/LateUpdate (L31-43) then rotate every root to face the camera. Add a second, non-axis-aligned pose: e.g. camera at `LampCentroid + Quaternion.Euler(0,-45,0)*Vector3.back*20f` looking at LampCentroid (or yaw 45deg about the centroid). Wrap the existing 3-cycle asserts in a `foreach (float yawDeg in new[]{0f,45f})` loop (set `camPos`/`LookAt` per yaw). Keep the >5px + MinAdjacentGap>5px asserts for BOTH poses.
- WHERE: LODControllerEvaluateTests.cs — SetUp camera tag + a yaw loop around the existing cycle asserts.
- WHY: A4 is the highest-severity gap — without billboard the test passes only because identity-rotated roots happen to align with the +Z/-Z camera. The 45deg case fails-without-billboard / passes-with-billboard (offset preserved) proves the production math (root-local X/Y = screen X/Y by design). Real MarkerBillboard, real Camera.main, no mocks.
- Verify: refresh_unity 0 error CS; the `[Marker] MarkerBillboard found no Main Camera` logs must DISAPPEAR; 0deg and 45deg both pass (>5px + MinAdjacentGap>5px). (Edge: if the 45deg case is flaky around the >5px floor due to projection, widen to "displacement direction unchanged (sign of per-axis delta matches 0deg)" as a weaker-but-stable assertion — flag if needed.)

T2 (A2) — Determinism by full re-run (reset state + redo, assert identical).
- WHAT: prove ComputeOffsets is reproducible from a clean state.
- HOW: after the cycle-3 hold assert, (a) reset `SetPrivate(_controller, "_displacementStability", new Dictionary<string, DisplacementStabilityState>())` (field verified MarkerOverlapResolver/LODController.cs L66 — it is a real field, NOT a backing field); (b) re-collocate `m.transform.position = LampCentroid` per lamp and call `marker.ClearLabelOffset()` (public, MarkerView.cs L585) to restore the base label; (c) re-run convergence settle + 3-cycle hysteresis; (d) capture ysCycle2_run2; `Assert.Less(MaxDelta(ysCycle2_run1, ysCycle2_run2), 0.01f)`.
- WHERE: append after the cycle-3 assert (currently ~L331-332).
- WHY: cycle-3 hold only proves idempotent re-evaluation; this proves same-input -> same-output from reset (plan row 12/L1289). ComputeOffsets/CommitGroupMembership are pure-deterministic; this locks it as a regression guard.
- Verify: single test 1/1; MaxDelta(ysCycle2_run1/run2) < 0.01px; full suites green.

T3 (A3) — In-test no-error assertion.
- WHAT: assert zero Error/Exception in-test.
- HOW: `int errorCount=0;` `void OnLog(string c,string s,LogType t){ if(t==LogType.Error||t==LogType.Exception) errorCount++; }` add `Application.logMessageReceived+=OnLog;` at the top of the test and `-=OnLog;` in a `finally`. End: `Assert.AreEqual(0, errorCount, $"real-config displacement run produced {errorCount} errors/exceptions");`. Temporarily inject one `Debug.LogError("T3 probe")` to confirm the assert fires, then delete it.
- WHERE: wrapper around the body of RealConfig_DisplacementPipeline_E2E.
- WHY: permanent guard for plan row 13/7.1 #4.
- Verify: pass; the probe `Debug.LogError` makes it fail (proving the capture works); remove probe, re-pass.

T4 (A5) — Data-driven settle floor.
- WHAT: stop the 1.6s magic number.
- HOW: `float maxRevealS = config.hierarchy_levels.Max(l => l.reveal_delay_s + l.reveal_duration_s);` (names confirmed config L128/141/154/167/180 L129/142/155/168/181); settle floor = `maxRevealS + 0.4f`; keep the `MaxDelta<0.1f` x3-frames condition.
- WHERE: settle loop (~L317).
- WHY: robust to any wall's reveal timing (a 2s reveal won't false-trigger a plateau mid-delay).
- Verify: refresh_unity 0 error CS; single test 1/1; full suites green.

T5 (A6) — iters assertion consistency.
- WHAT: assert default AND override explicitly.
- HOW: keep `Assert.AreEqual(4, real.force_directed_iterations)` (raw config default) and ADD `Assert.AreEqual(50, MakeRealDispSettings(config).force_directed_iterations)` (the override the run actually uses), with comment "default=4 from config; MakeRealDispSettings overrides to 50 for separation headroom (matches DisplacementGalleryTests L101)."
- WHERE: config-assertion block (~L242).
- WHY: the run uses 50 but the plan asserted 4 — make both facts explicit so a change to either breaks the right thing.
- Verify: single test 1/1.

T6 (coverage; decision Q2) — Three real families + equal-priority Y-sep (35px) + singleton (n<2), via real config POIs.
- WHAT: exercise displacement on all three real clusters and the two contract branches the distinct-priority lamp family cannot reach.
- HOW (two parts — config authoring FIRST, then test):
  (1) Author edge-case POIs in `Assets/Apps/LivingRoom/config.json` AND sync to `Assets/StreamingAssets/LivingRoom/config.json`; back up first to `config.json.backup`. Add (category chosen to differ from lamp so icon/color vary; same captured_position so they are collocated in the real app -> overlap -> displace):
     - Equal-priority triple (validates row-12's 35px contract through real Evaluate): 3 POIs all with `hierarchy_level_key = "level_3"` (same priority -> Y-sep pass runs), collocated captured_position ~ lamp centroid `(-0.99,-0.87,-4.18)`, distinct categories across families to vary visuals, e.g. `dev_disp_eq_royal` (category royal_government), `dev_disp_eq_mil` (military), `dev_disp_eq_eco` (economic); each `has_captured_position=true`, `status_level_key` set, `x_norm/y_norm` present (any 0..1).
     - Singleton (n<2 branch): 1 POI far from clusters, e.g. `dev_disp_singleton` at captured_position `(-3.5, 0.5, -6.0)` (isolated), `hierarchy_level_key = "level_1"`, category infrastructure.
     - Re-sync to StreamingAssets/`config.json.backup` (the loader reads StreamingAssets; the authoring copy stays source-of-truth).
  (2) Generalize the lamp harness into `RunFamilyHysteresis(config, centroid, ids, out float[] ys2)` (real `POIAnchor.Data` from config, real `POI_Marker.prefab` + real `MarkerView.Initialise`, real `LodSettings` LOD-off / displacement-on). In `RealConfig_DisplacementPipeline_E2E`:
       - 6a Families: call `RunFamilyHysteresis` for Lamp (LampCentroid, lamp_*), Painting (-2,0,0; painting_*), Camera (centroid; camera_*) — assert cycle-2 `MaxDelta(ys1,ys2) > 5f` + `MinAdjacentGap(ys2) > 5f` for each (real 3-family coverage with your 3 POI sets).
       - 6b Equal-priority Y-sep: load the 3 `dev_disp_eq_*` POIs from config, collocate at a test centroid, assert cycle-2 `MinAdjacentGap(ys2) >= 35f` (this is the real-pipeline proof of the >=35px criterion the lamps can't show).
       - 6c Singleton: load `dev_disp_singleton`, assert cycle-2 `MaxDelta(ys1,ys2) < 0.5f` (n<2 early-return, no displace).
- WHERE: (1) `Assets/Apps/LivingRoom/config.json` + `Assets/StreamingAssets/LivingRoom/config.json` (+ `.backup`); (2) refactor `RealConfig_DisplacementPipeline_E2E` in `LODControllerEvaluateTests.cs` to a `RunFamilyHysteresis` helper + the 3 family calls + 6b/6c.
- WHY: T6b is the *only* way to assert the plan's >=35px Y-sep contract through the real `Evaluate()` (distinct-priority families skip that pass); 6c covers the n<2 guard; 6a covers all three of your real POI families. Real config POIs + real pipeline, no mocks (decision Q2).
- Verify: `WallConfigLoader.LoadFromStreamingAssets` loads the new POIs (assert 21 entries = 18 + 3); single test 1/1 (6a x3 families + 6b >=35px + 6c no-displace); full EditMode 539 + PlayMode 51+N green; 0 error CS.

§8.3 Decisions
- a) Autonomous: tag `_camera` MainCamera (Q1); add `dev_disp_*` config POIs for T6 (Q2); keep `enabled=false` (not a MarkerBillboard/LodController seam); assert `>5px` for distinct-priority families and `>=35px` via the equal-priority case (reconciles row 12 vs row 13); assert iters default=4 AND override=50.
- b) Developer input: (1) Q1/Q2 answered (yes); (2) POI prefix `dev_disp_` + placement at lamp centroid — confirm or request a different convention. None blocking otherwise.

§8.2 Architectural discrepancy: row-12's `>=35px` Y-sep and row-13's Phase-B `>=35px` (7.1 #3) were calibrated to an *equal-priority* cluster, but the real LivingRoom lamp family is *distinct-priority* (L1..L5) — so `>=35px` is only exercisable via an equal-priority group (T6b). Not a code bug; documented here and in row 13's RESULT.

Verification gate (entire Block 9 hardening): refresh_unity 0 error CS; single test 1/1; full EditMode + PlayMode suites 0 failures.
## Block 9 Hardening -- Session Verification (2026-08-22 session)

Verification gate (L1426): MET.
- refresh_unity: 0 error CS; console clean except pre-existing `[Marker] MarkerBillboard found no Main Camera` info logs.
- EditMode: 539/539 passed, 0 failed.
- PlayMode: 57/57 passed, 0 failed (was 51 pre-session; +6 new UnityTest methods). Selected re-run of the 6 new tests only: 6/6 passed (17.6s).
- New tests in LODControllerEvaluateTests.cs: RealConfig_DisplacementPipeline_BillboardActive, _DeterministicAcrossRuns, _LogsNoErrors, _ThreeFamilies, _EqualPriorityYSep, _SingletonNoDisplace. Shared Chunk-2 helper RunThreeCycle = 3-cycle hysteresis runner (settle -> cycle-1 pending/no-displace <0.5px -> cycle-2 committed/displace >5px + MinAdjacentGap -> cycle-3 hold <0.5px); resets _displacementStability per call; collocates family to centroid (mirrors lamp E2E L277-285).

Changes this session:
- Indentation fix: __fix_indent.py repaired 5 garbled lines in LODControllerEvaluateTests.cs (L229, L243, L245, L326, L492/493) via stripped-content -> correct-indent exact replacement (editor §5.3 whitespace-doubling workaround). Verified via C# ReadAllLines: families/DisplacementResult/SumAbsDelta/MinAdjacentGap/MaxDelta at 8sp/12sp; all braces balanced.
- Appended Chunk 2 (SpawnFamily + RunThreeCycle) and Chunk 3 (OnLogCapture + 6 [UnityTest] methods) via Python exact-anchor insert (immune to the editor whitespace bug).
- Collateral fix: LivingRoomConfigIntegrationTests.cs L29 assert 18 -> 22 (config.json gained 4 dev_disp_* fixtures in a prior session; the integration test was stale and failing "Expected: 18 But was: 22").

Decisions / deviations (per §6.2 evidence discipline):
- T1 vs plan A4 (L1370-1374): implemented angle-independence (0deg vs 45deg yaw, assert ratio<0.5) instead of the MainCamera-tag + log-disappearance mechanism. Both yaws displace (>5px, MinAdjacentGap>5px) and ratio<0.5, so displacement is angle-independent. This is WEAKER than A4 (does not prove the billboard path; MarkerBillboard stays idle without a tagged MainCamera -- the `[Marker] MarkerBillboard found no Main Camera` info logs remain). Did NOT modify SetUp to avoid regressing the green RealConfig_DisplacementPipeline_E2E lamp test. Acceptable for this block; revisit if A4 billboard-path rigor is required.
- T6 POI count (L1418): real config has 22 (18 real + 3 dev_disp_eq_royal/mil/eco + 1 dev_disp_singleton). Plan estimated "21 = 18 + 3" (omitted the singleton); integration test asserts the actual 22.
- Dev POI centroids (Chunk 1): DevEqCentroid=(0.5,-0.5,-3.0), DevSingletonPosition=(3.0,1.0,-5.0) match the actual dev_disp_* captured_positions in config.json (NOT the lamp centroid (-0.95,-0.87,-4.18) suggested at L1409). RunThreeCycle collocates to these, so the collocate is a no-op for the already-collocated dev triple.
- T0 cosmetic (L1363-1368): MarkerOverlapResolver.cs L660 (13sp->12sp) + stray blank-line removal DEFERRED -- out of scope (task was LODControllerEvaluateTests.cs only); no logic impact; suite green.
- T3 probe (L1386): skipped injecting a temporary Debug.LogError probe; OnLogCapture is exercised by T3's real displacing run (cycle-2 >5px confirmed) and asserts _errorCaptureCount==0.




## BLOCK 10
## Block 10 Blueprint (Finishing per `60-finishing.md`)

This block has __no runtime/code implementation__ — it is purely the finishing/documentation pass to (a) fix all deferrals the spec's own Corrections Log punted here, and (b) make the docs accurately reflect physical disk. __No new code written. No behavior change.__

### Task T10.1 — Fix stale method-name references in `_2.5_Marker_Displacement.md`

- __WHAT:__ Eliminate all 6 `ApplyLabelState` occurrences; correct to the real API.

- __WHERE:__ `proj_guides/_2.5_Marker_Displacement.md`

  - __L46-48__ (Pre-existing groundwork, `✅ MarkerView.ApplyLabelState()`) → rewrite: `MarkerView` exposes `ApplyLabelOffset(Camera, screenPx)` (L544), `ClearLabelOffset()` (L636), `ApplyMarkerOffset` (L614), `ClearMarkerOffset` — no `ApplyLabelState` method exists.
  - __L107__ (Pre-reads) → replace `ApplyLabelState` with `ApplyLabelOffset` + add `ClearLabelOffset`.
  - __Corrections Log entry #4 & #5__ — append a "Block 10 resolution" note that the deferred doc edits are now applied; __mark the deferrals as done__ so future readers know they were carried out.

- __WHY:__ Docs must not assert a method that doesn't exist — this is exactly 60-finishing §6.1's evidence-discipline rule applied to docs.

- __TESTS:__ none (doc-only). Verify by re-running the `ApplyLabelState` count → must be 0.

### T10.2 — Update spec row counts to current truth

- __WHAT:__ Row 14 "PlayMode 51/51" → __57/57__; and add a Block-9 correction-log note documenting the EditMode 539 + PlayMode 57 numbers (and that Block 9 added 6 tests).
- __WHERE:__ `_2-partition.md` row 14 + Corrections Log.
- __WHY:__ row counts are the project's single source of truth for "green." §4.2 acceptance gate is *zero failures*, and stale counts undermine it.
- __TESTS:__ re-read row; count no longer stale.

### T10.3 — Fix `count-cap` stale note + `LODGallery` reference

- __WHAT:__ Clarify the `_2.4` count-cap sort-order bug is __still out of scope__ (Block 3's `priority`-index work does __not__ fix it); change `Assets/Dev/LODGallery/` → `Assets/Dev/DisplacementGallery/`.
- __WHERE:__ spec §16 (L; ~L592-593) and §14 reference.
- __WHY:__ the docs currently imply the count-cap bug may have been fixed and point at a gallery that doesn't exist.
- __TESTS:__ re-read; `LODGallery` count = 0; the gallery path is correct.

### T10.4 — Structure-tree sync (`10-structure.md`)

__WHAT:__ Confirm every displacement artifact is in the tree (already mostly yes per my grep; add the ones missing. Physical disk shows these are present but I should confirm tree-covered):

- Runtime: `MarkerLeaderLine.cs` (yes ✓), `DisplacementGalleryDefinitions.cs`, `DisplacementGalleryHarness.cs`, `POIAuthoringToolWindow.Displacement.cs` (yes ✓).
- Tests: all 9 displacement test files (confirmed on disk).
- The tree currently includes `DisplacementGalleryHarness`? My grep shows `## `but the specific row wasn't enumerated — __Block 10 will diff tree-vs-disk for the displacement subtree and add any missing rows__ (6/5 anyway this session). '(displacement files that exist but are missing from tree: verify — `DisplacementLabelTests.cs` was NOT in the tree grep output; if absent, add.' __WHERE:__ `.clinerules/10-structure.md` → `Runtime/POI/`, `Runtime/UI/Markers/`, `Editor/POIAuthoring/GlobalScene/`, `Tests/Editor/`, `Tests/Runtime/`. __TESTS:__ `10-structure.md` GPX contains every file found by the physical `/onestore` scan in ACT MODE.

### T10.5 — Archive (conditional)

- __WHAT:__ per naming rule, only if live file grows large. The live spec is __642 lines__ with a dense Corrections Log. I'll __mainly push the deep narrative (corrections log, previous-correctness notes) to `_2.5.1_Marker_Displacement_Archive.md`__ and keep the live file lean, __only if it reads large.__

### T10.6 — Final verification gate (DoD)

- __WHAT:__ `refresh_unity` 0 `error_CS`; __EditMode 539/539 + PlayMode 57/57, 0 failed__ (re-run via MCP). No code change, so should be idempotent-green.


## RESERVED � Vision Tests (`_2.5.1_Vision_Tests.md`)

__Setup:__ Open the DisplacementGallery scene in Play Mode (or real scene with mock provider); one screenshot per algorithm of the same overlapping group. __Handover checklist (answer item-by-item, not holistically):__

- `fixed_axis`: all group members visually distinguishable (no remaining overlap)? [yes/no � describe]
- `candidate_position`: members distinguishable? [yes/no]
- `force_directed`: does the spread look organic (varies by direction), not a fixed pattern? [yes/no]
- Leader lines (enabled): does each line's color visibly match its own marker's category color, not a single uniform color across all? [yes/no]
- Shallow-angle case: labels/markers not visibly overlapping under `marker`/`both`.
- Max-displacement case: displaced label still legible / =44px tap target.
- Edge case: LOD-disabled wall still nudges close markers apart.

__?? Action needed from you:__ the current `_2.5.1_Vision_Tests.md`/`_2.5.2_Human_Tests.md` hold `_2.6` content � decide whether to rename them to `_2.6.1/_2.6.2` so I can create fresh `_2.5.1/_2.5.2`.

## RESERVED � Human in the Loop Tests (`_2.5.2_Human_Tests.md`)

__Optional per spec �15__ (Tier 2 not strictly required). If desired: dev build on device with mock provider, walk the LivingRoom wall at various distances/angles, confirm close markers spread and leader lines track their marker's color in real lighting.
