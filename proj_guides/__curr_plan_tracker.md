# Implementation Plan: Simplify POI Positioning — Remove Draft, Scene + Capture Flow

> **Date:** 2026-09-11
> **Goal:** Remove the entire Draft/normalized-position sub-domain (x_norm/y_norm interpolation, calibration anchors, Draft tab, OnSceneGUI handles, CorrectionAnchor indirection) and replace with a single clean workflow:
>   1. Click "+" between/after existing POIs (or before first) → new POI appears immediately in Scene view (auto-rig + auto-focus), at origin for first POI, or at previous POI's position + small offset for subsequent.
>   2. Move marker in Scene view with normal transform tools.
>   3. Press "Capture" in single state-aware "Position" tab to lock world position.
>   First POI rotation = 0; subsequent inherit previous POI's editor_rotation_deg.
>   Simplify POIPositionResolver to single captured-position path + (0,0,0) origin fallback. Remove dead Draft code. Clean tests. Update structure guide + docs.


## 2. What is REMOVED (pure Draft domain — delete outright)

- [ ] `POIData.x_norm`, `POIData.y_norm` fields (and their JSON serialization).
- [ ] `WallConfigData.calibration_anchors` list + `CalibrationAnchor` class.
- [ ] `POIPositionResolver.InterpolateBetweenAnchors` and `EstimateWallHeight` (interpolation fallback path).
- [ ] `POIPositionResolver.TryResolvePosition` fallback branch consuming x_norm/y_norm/anchors — replace with single captured-position path (+ origin fallback).
- [ ] Draft tab: `DrawDraftTab` method + Draft-specific UI in `DrawPositionTabs`.
- [ ] `OnSceneGUI` handles (cyan positioning spheres) in `RigLifecycle.cs`.
- [ ] Any helper existing only to support the above (re-check each).

---

---



### 3.1 Rig creation - remove CorrectionAnchor requirement

- `GetOrCreateRig()`: currently requires `_correctionAnchor` to exist; change to create simple "POIAuthoringRig" GameObject at origin (no parent needed). No manual PlacementCorrectionAnchor required.
- `GetExistingRig()`: currently searches under `_correctionAnchor`; change to search scene directly (GameObject.Find("POIAuthoringRig")).
- Remove `_correctionAnchor` field from shell + `TryResolveSceneReferences`'s correction-anchor resolve.
- `CapturePositions` / `CaptureSinglePoi`: currently use `_correctionAnchor.InverseTransformPoint`/`TransformPoint`. With rig at origin, capture child's `position` (world) directly.

### 3.2 Auto-spawn + auto-focus on Add

Add/extend:

- `AddNewPoi()` — appends to end, then auto-spawns + auto-focuses. First POI → position origin (0,0,0); rotation = 0. Subsequent → position = previous POI's rig child world position + offset; rotation = previous POI's `editor_rotation_deg`.
- `AddNewPoiAfter(POIData previous)` — inserts new POI in list right after `previous`, then auto-spawns + auto-focuses using `previous`'s rig child position + offset and `previous`'s rotation. If `previous` has no rig child → fall back to origin + rotation 0, show clear warning.

Shared spawn helper `SpawnRigChildForPoi(POIData poi, Vector3 initialPosition, float initialRotation)`:
1. Ensure rig exists (`GetOrCreateRig()`). If missing → add POI to config, show inline warning, skip scene spawn.
2. Instantiate `POI_Marker.prefab` under rig; name = `poi.id`; set `localPosition` from `initialPosition`; set `localRotation = PoiRotationResolver.ToYawQuaternion(initialRotation)`.
3. Attach + configure `POIAnchor` + `MarkerView` (reuse visual-config logic from `RefreshRigVisuals` — factor shared `ConfigureRigChild(poi, child)` helper so `PopulateRig` and add-flow use identical code).
4. Call `FocusPoiInScene(poi)` → selects child + frames SceneView.
5. Wrap whole add (config add + scene spawn + focus) in `DrawConfigMutationScope` (config) + Unity `Undo` (scene) so one Ctrl+Z undoes both. Set `_hasUnsavedChanges = true`.

### 3.3 Single state-aware "Position" tab (replaces Draft + Precise)

`DrawPositionTabs` → single "Position" foldout whose content depends on `has_captured_position`:

- **Not captured:** one-line hint "Move marker in Scene view, then press Capture to lock position." + "Capture Position" button (→ `CaptureSinglePoi`) + Edit Rotation slider.
- **Captured:** read-only: captured_position X/Y/Z + Source + Timestamp + "Clear Capture" button (→ sets `has_captured_position = false`, nulls capture fields, tab returns to not-captured state). Clearing does NOT move marker.

Remove: Draft tab, Precise tab as separate tabs, `x_norm`/`y_norm` sliders, "Use captured position" toggle.

### 3.4 "+" button placement in POI list (SpecificMarker UI)

In `DrawSpecificMarkerOptions`, inside each expanded POI header area (indented under POI), add small "+ after" button (width ~60-80px) that calls `AddNewPoiAfter(thisPoi)`. Layout:

- Before first POI row: small "+ Add first" button (calls `AddNewPoi()` as "add as first").
- After each POI row: small "+ after" button.
- Last button (after last POI) is same "+ after" style — acts as "add after last".

No separate main "Add POI" button at top of list. (List-internal "+" affordances replace it.)

### 3.5 Empty-state / first-action guidance

When `_config.pois` is empty/null: show clear friendly state in Specific Marker tab: "No POIs yet. Use the + buttons below to add your first POI." + the "+ Add first" button. When first POI is added + auto-spawned: SceneView frames it (auto-focus) so developer sees it immediately.

## 3. What is KEPT and possibly SIMPLIFIED

- `POIData.captured_position`, `has_captured_position`, `captured_position_source`, `captured_position_timestamp` — the capture state. Keep.
- `CaptureSinglePoi(POIData)` — per-POI capture action. Keep. Captures rig child's world position directly.
- `CapturePositions(bool)` — batch capture (called by "Save All to JSON"). Keep; adapt if needed.
- `POIPositionResolver.TryResolvePosition` — keep the method; simplify: if `has_captured_position` && captured_position present (and not NaN/inf) → return it; else return origin (0,0,0) as safe fallback (warn-once optional). Remove interpolation branch entirely.
- `POIPositionResolverTests` — remove tests for x_norm/y_norm interpolation path; keep/add tests for captured-position precedence + origin fallback.
- `MarkerView`, `POIAnchor`, `MarkerOverlapResolver` + displacement system, `MarkerLeaderLine`, `MarkerLayout` — runtime/displacement. Untouched.
- `PoiRotationResolver` — keep (clean editor-only rotation math). Used for yaw on add + refresh.
- `FocusPoiInScene`, `GetOrCreateRig`, `GetExistingRig`, `RefreshRigVisuals`, `PopulateRig`, `CaptureSinglePoi` — keep; rewire where they go through `CorrectionAnchor`.

