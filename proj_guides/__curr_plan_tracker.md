# Current plan: Effects domain rework - Ripple/Halo model, editor page, preview grid (DONE 2026-09-21)

Baseline (start of the whole Effects task): EditMode 808/808, PlayMode 79/80.
Final (after the preview-visibility pass): zero error CS, EditMode 827/827, PlayMode 105/107 (the 2 failures are pre-existing and unresolved: the raycast test above and LODControllerEvaluateTests.Evaluate_FlickerAtThresholdBoundary_NoFlap, which failed in the baseline run too).
The single PlayMode failure is the pre-existing baseline item
OrientationWallSessionIntegrationTests.SpawnPOIs_RealMarker_SymbolIsTopmostRaycastTarget
(0 raycast hits, also with a focused Editor; unrelated to Effects, cause unknown).

## Phase 1 - model + rename + per-effect blocks (done)
- [x] Six effects, each its own block + enabled checkbox in effect_defaults; master switch; preview settings
- [x] Full rename: Sun -> Ripple (Rings/Discs), Accent -> Halo (Ring/Disc) + Beacon; config keys, code, UI
- [x] EffectDefaults.FilterEnabled / IsEffectEnabled / SelectableEffects; MarkerEffectNames
- [x] MarkerView.ApplyEffects hands each effect its own block; styleOverride seam
- [x] MarkerRippleEffect, MarkerHaloEffect written; Pulse unchanged; resolver EffectFlagsOf
- [x] LivingRoom config migrated and regenerated through JsonUtility (both copies identical)
- [x] Gallery harness/definitions and existing tests renamed

## Phase 2 - editor page (done)
- [x] Effects.cs: master toggle hides all when off; one foldout per effect (checkbox, used-by line, (i), own rows)
- [x] EffectsHelp.cs (constants), EffectUsageSummary.cs (pure text/filter logic)
- [x] Level table: Ripple / Halo dropdowns filtered to enabled effects, "(disabled)" kept, Pulse greyed, (i) on effect columns
- [x] Specific Marker: read-only "Effects: ... Reveal: ..." line under Hierarchy Level
- [x] Old Effects code removed from GlobalScene.cs; Constants.cs cleaned

## Phase 3 - preview grid (done)
- [x] EffectsPreviewSpawner + WallSession hook (Editor + development builds only)
- [x] Test foldout: "Show effects preview in Play Mode" + base marker dropdown (plain grey circle + wall POIs)
- [x] Guides updated to start with the preview toggle

## Tests
- [x] MarkerEffectConfigTests (14, PlayMode), EffectsPreviewSpawnerTests (6, PlayMode)
- [x] EffectsAuthoringTests (reflection-driven round trip + 50-field undo/redo), EffectsUsageAndPreviewLayoutTests
- [x] POIEditorTableLayoutTests.FieldRows_UseSharedRow updated (Effects rows now in Effects.cs)

## Docs
- [x] _2.2 (component map, fields, section 6 rewritten), _5.1, 10-structure.md

Open: eyeball the new Effects page in the real window (foldout header rows, indentation, (i) buttons, Test rows,
wider Hierarchy Levels rows) and watch each effect with the preview grid.

## Second pass (preview grid invisible in the real scene) - DONE
- [x] Root cause: grid placed behind the room mesh (depth-tested world-space UI); confirmed in the live scene
- [x] Grid at preview.distance_m (default 1 m), scaled + wrapped to fit the view, default orientation
- [x] Effect layers sized in symbol diameters (were absolute metres); LivingRoom halo sizes 0.24 -> 1.2
- [x] Toggle renamed "Show effects in Play Mode"; Distance (m) row; help/guides say Play reads the SAVED config
- [x] EffectsPreviewRenderTests (5): pixel-level visibility, animation, size independence, label readability
- [x] Real scene captured from the Game view (Assets/Screenshots/effects_preview_real_scene_hd.png)

## Third pass (Focus on Effects Grid) - DONE
- [x] Grid 5000 m above the scene with its own camera (EffectsPreviewFocus); no scenery can hide it; toggle renamed; distance_m removed
- [x] Fit distance + columns for any aspect; follows main camera rotation; refits on resize
- [x] EffectsPreviewRenderTests (7) incl. wall 50 cm in front, main camera never draws the grid, rotation/roll, refit
- [x] Real scene captured with ScreenCapture: Assets/Screenshots/effects_focus_gameview.png
- [x] Orientation WallSession fixture isolated from the developer preview switch
