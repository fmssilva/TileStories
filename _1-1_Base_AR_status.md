# TileStories - Base AR Foundation Work Done (Phase 1)

Date range: this document summarizes the full setup and implementation work completed in this chat session for Stage 1 base AR foundation.

## 1) Scope completed

Goal achieved in this phase:
- Created a clean Unity mobile AR project baseline.
- Installed and wired Immersal SDK.
- Added a local test wall app (LivingRoom) with real Immersal map files.
- Implemented framework/runtime bootstrap code for tracking + config load + POI spawn.
- Built and launched on Android tablet.
- Added runtime diagnostics logs for lock/spawn/visibility.

Deferred intentionally to later stages:
- Card UI details and block rendering.
- Final marker visual design.
- Full editor tooling (validator/baker/wizard implementation).

## 2) Manual steps performed (Unity / Immersal / device)

### 2.1 Unity project creation and baseline
- Created Unity project using template AR Mobile.
- Kept project structure as:
	- workspace root: C:\Users\franc\Desktop\TileStories
	- Unity project root: C:\Users\franc\Desktop\TileStories\TileStories

### 2.2 Scene/template cleanup for minimal baseline
- Removed template UI/spawner/interactor clutter from scene.
- Removed template folders no longer needed.
- Removed unnecessary packages from default template and kept only required AR stack.

### 2.3 Package baseline finalized
- Kept required packages:
	- com.unity.xr.arfoundation
	- com.unity.xr.arcore
	- com.unity.xr.arkit
	- com.unity.xr.management
	- com.unity.inputsystem
	- com.unity.render-pipelines.universal
	- com.unity.mobile.android-logcat
- Added Immersal:
	- com.immersal.core from https://github.com/immersal/imdk-unity.git

### 2.4 Build target and XR settings
- Android build target used.
- ARCore provider enabled in XR Plug-in Management for Android.

### 2.5 LivingRoom dev wall setup
- Added LivingRoom app folder under Assets/Apps for rapid home iteration before real wall deployments.
- Added map files exported from Immersal workflow:
	- 146267-LivingRoom2.bytes
	- 146267-LivingRoom2-tex.glb
- Immersal SDK object added to scene.
- Immersal login/token configured in scene via ImmersalSDK component.

### 2.6 Device deployment
- Android app built and launched successfully on tablet.
- ADB wireless connection used.
- Android Logcat used for runtime diagnostics.

## 3) High-level architecture decisions implemented

- Enforced framework/app separation from day 1:
	- Framework code under Assets/Framework.
	- Wall/app data and media under Assets/Apps/LivingRoom.
- Added a dev-only fourth wall (LivingRoom) in plan for fast iteration.
- Runtime assembly boundaries created with asmdefs to keep clean layering.

## 4) Folder structure created

Created core structure aligned to the work plan:
- Assets/Framework/Runtime/{Core,Tracking,POI,UI,Blocks,Circuits,Telemetry,AI}
- Assets/Framework/Editor/{Validation,Baker,Wizard}
- Assets/Framework/Tests/{Editor,Runtime,TestFixtures}
- Assets/Apps/{Panorama,Chafariz,Mural,LivingRoom}
- Assets/Apps/*/MediaAssets/{Audio,Images,Models3D,Videos}
- Assets/StreamingAssets/LivingRoom
- Assets/Plugins

## 5) Files created/implemented

### 5.1 Assembly definitions
- Assets/Framework/Runtime/TileStories.asmdef
- Assets/Framework/Editor/TileStories.Editor.asmdef
- Assets/Framework/Tests/Editor/TileStories.Editor.Tests.asmdef
- Assets/Framework/Tests/Runtime/TileStories.Tests.asmdef

### 5.2 Runtime core/tracking/poi scripts
- Assets/Framework/Runtime/Core/WallConfigData.cs
- Assets/Framework/Runtime/Core/WallSession.cs
- Assets/Framework/Runtime/Tracking/IWallTracker.cs
- Assets/Framework/Runtime/Tracking/MockLocalizationProvider.cs
- Assets/Framework/Runtime/Tracking/ImmersalWallTracker.cs
- Assets/Framework/Runtime/POI/POIAnchor.cs

### 5.3 Wall test config and app files
- Assets/StreamingAssets/LivingRoom/config.json
- Assets/Apps/LivingRoom/146267-LivingRoom2.bytes
- Assets/Apps/LivingRoom/146267-LivingRoom2-tex.glb
- Assets/Apps/LivingRoom/LivingRoomScene.unity

### 5.4 Tooling script
- check-unity-errors.ps1
	- checks C# compile errors/warnings from Unity Editor.log.

## 6) Runtime behavior implemented in code

### 6.1 Wall config model
- WallConfigData + POIData + CapturedPosition classes.

### 6.2 Tracker abstraction
- IWallTracker interface for pluggable trackers.
- MockLocalizationProvider for editor/dev quick testing.
- ImmersalWallTracker adapter for Immersal Localizer events.

### 6.3 Wall session bootstrap
- WallSession loads config from StreamingAssets.
- Subscribes to tracker localization events.
- Spawns POIs at world positions.

### 6.4 Android-safe StreamingAssets loading
- Implemented UnityWebRequest path for jar:/ APK asset loading on Android.
- Kept direct file IO for editor/desktop.

### 6.5 Simple race handling (clean/small)
- Localization event before config load is ignored.
- After config load, if tracker already localized, spawn immediately from current pose.
- One-time spawn guard prevents duplicates.

### 6.6 POI spawn fallback behavior
- If captured_position has meaningful non-zero values, use it.
- Else fallback to x_norm/y_norm mapping on a synthetic wall plane.
- Added per-POI spawn logs (id, position, distance, inFront flag, dot).

## 7) Main issues encountered and fixes applied

### 7.1 Build and run launcher failure
Problem:
- No activity in manifest with MAIN/LAUNCHER.
Fix:
- Corrected Android application entry setting in ProjectSettings.asset.
- Rebuilt with clean Android artifacts.

### 7.2 Input system mismatch
Problem:
- Old UnityEngine.Input usage while Input System package active.
Fix:
- Updated mock movement/look to Input System usage.
- Added InputSystem reference in runtime asmdef.

### 7.3 StreamingAssets not found on Android
Problem:
- config.json read using file API from APK jar path.
Fix:
- Switched Android path to UnityWebRequest in WallSession.

### 7.4 POI overlap issue
Problem:
- All POIs appeared at same coordinates due to zeroed captured_position being treated as valid.
Fix:
- Added non-zero validation before using captured_position fallback.

### 7.5 Runtime debug marker failures
Problem:
- Device logs showed primitive/shader fallback errors during debug marker creation.
Fix:
- Replaced fragile fallback with simpler/safe debug marker creation path and added explicit spawn diagnostics.

## 8) Latest runtime verification (from logs)

Confirmed in current run logs:
- Wall config loaded on device.
- Immersal first lock achieved.
- WallSession spawn call executed for 3 POIs.
- Per-marker spawn logs emitted.

This confirms the base pipeline is active end-to-end:
- Android app launch -> AR/Immersal startup -> lock event -> framework session -> POI spawn path.

## 9) Files/settings that were also updated

- Packages/manifest.json package set adjusted and Immersal dependency added.
- ProjectSettings/ProjectSettings.asset Android application entry setting corrected.
- _4_united_work_plan.md updated to include LivingRoom as dev-only fourth wall.

## 10) Current status at end of Phase 1

Phase 1 base setup status: functionally established.

What is stable now:
- Project boots, builds, deploys to tablet.
- Immersal stack integrated.
- Framework bootstrap code exists in proper folders.
- Wall config + POI spawning path exists and logs are instrumented.

What should be done next before Stage 2 UI/features:
- Finalize an always-visible debug marker prefab for device validation.
- Confirm non-overlapping marker positions in latest build after captured_position guard.
- Optional cleanup pass on scene object naming and prefabization.

## 11) Manual workflow summary for future framework user guide

End-to-end operator flow used in this phase:
- Create Unity AR project.
- Clean template scene/packages.
- Install Immersal SDK via Package Manager.
- Login/set developer token in ImmersalSDK.
- Capture real environment map with Immersal app/tooling.
- Export/download map assets (.bytes and texture .glb).
- Place map assets under Assets/Apps/<WallName>/ root.
- Define wall config in Assets/StreamingAssets/<WallName>/config.json.
- Wire scene objects: AR Session, XR Origin, ImmersalSDK stack, WallSession + tracker.
- Build and run on Android device.
- Use Logcat and WallSession logs to validate lock + spawn pipeline.

## 12) Audit pass (plan + guidelines + notes cross-check)

This section checks the current implementation against:
- _0_work_plan.md (Stage 1 base AR scope and architecture boundaries)
- _0_guidelines.md (code quality, structure, comments, testing discipline)
- notes.md (critical architectural observations from this session)

### 12.1 Keep as-is

These are aligned and should be kept:
- Framework vs Apps split is in place and correctly used.
- Runtime vs Editor vs Tests assembly split exists via asmdefs.
- Stage 1 core path works on device: app launch -> Immersal lock -> WallSession spawn pipeline.
- Android-safe StreamingAssets loading is implemented.
- MockLocalizationProvider exists and supports fast editor iteration.
- LivingRoom dev wall strategy is valid and useful before real wall roll-out.

### 12.2 Should clean now (before moving to next feature phase)

These items are important and should be cleaned now to keep architecture simple:

1. Split decision logic out of WallSession
- Current WallSession still contains POI position decision branches.
- Per work plan + guidelines, WallSession should orchestrate only.
- Move world-position decision logic to a dedicated resolver class:
	- Framework/Runtime/POI/POIPositionResolver.cs

2. Add a dedicated config reader class
- Current config read/parse is inside WallSession.
- Move Android/editor path handling + JSON parse into:
	- Framework/Runtime/Core/WallConfigLoader.cs
- WallSession should call loader and continue orchestration only.

3. Remove or gate temporary debug marker fallback
- Current debug marker fallback is useful for bring-up, but should not remain ambiguous.
- Either:
	- replace with explicit Stage-1 DebugMarkerView class, or
	- gate fallback behind a clear debug flag and document it.

4. POIAnchor minimal cleanup
- Current POIAnchor still carries debug interaction assumptions.
- Keep it data/anchor-focused and move marker visual concerns to a dedicated view component.

5. Add first deterministic unit tests now
- Highest-value immediate tests:
	- POIPositionResolver uses captured_position when meaningful.
	- POIPositionResolver falls back to x_norm/y_norm when captured_position is zero/absent.
- This directly protects against the overlap regression already seen in device logs.

6. Standardize ASCII-only log/comment text
- Keep all newly added logs/comments ASCII-only as requested in guidelines.
- Avoid non-ASCII punctuation in developer-facing log lines.

### 12.3 Can defer to Stage 2/3

Safe to defer now:
- Full marker visual system and polish (you explicitly chose to defer marker visibility polish).
- Card/detail UI implementation.
- Full telemetry event taxonomy wiring.
- Editor tooling implementation (validator/baker/wizard logic).
- LOD/clustering optimization pass.

### 12.4 Additional findings from notes.md (critical check)

The notes' core architectural warning is valid:
- WallSession is naturally at risk of becoming a god-class.
- The recommended split (WallConfigLoader + POIPositionResolver + thin WallSession) should be applied before adding more POI/UI behavior.

The notes' testing observation is also valid:
- The captured_position fallback bug is a textbook deterministic test case.
- Add tests before layering more features on top of position resolution.

## 13) Pre-next-phase TODO (clean + simple path)

Use this exact sequence before starting Stage 2 feature work:

1. Create WallConfigLoader.cs and move config read/parse there.
2. Create POIPositionResolver.cs and move world-position branching there.
3. Refactor WallSession.cs to pure orchestration (load -> wait lock -> resolve -> spawn).
4. Add POIPositionResolverTests.cs with at least the two regression tests listed above.
5. Decide debug marker strategy (explicit debug component or debug flag) and document choice.
6. Run compile check + edit-mode tests and save a short verification log.
7. Freeze this cleaned baseline, then begin next phase.

Status after this audit:
- Current project is functionally progressing well for Stage 1.
- It is not yet at the cleanest architectural baseline requested by guidelines.
- Applying the cleanup TODO above will put it in a strong state before moving on.

## 14) Pre-Stage-2 cleanup delta applied

Applied now (cleanup only, no new Stage 2 feature logic):

- Added `Assets/Framework/Runtime/Core/WallConfigLoader.cs`.
	- Wall config file I/O and JSON parsing are now centralized there.
	- Android `jar:` and desktop file-path handling remain supported.

- Refactored `Assets/Framework/Runtime/Core/WallSession.cs`.
	- `WallSession` now delegates config loading to `WallConfigLoader`.
	- Removed ad-hoc debug marker fallback creation from session flow.
	- POI anchor prefab is now explicit/required for runtime spawn.

- Cleaned `Assets/Framework/Runtime/POI/POIAnchor.cs`.
	- Removed debug-specific collider/tap behavior from anchor.
	- `POIAnchor` is now only a data anchor + naming bind point.

Compile status after cleanup:
- `check-unity-errors.ps1`: NO COMPILE ERRORS.
- Existing warning remains: `MockLocalizationProvider.OnTrackingLost` event currently unused.

Important scope guard respected:
- POI positioning resolver redesign and POI positioning tests were intentionally not implemented in this cleanup step.
- Those remain planned for next phase implementation.

