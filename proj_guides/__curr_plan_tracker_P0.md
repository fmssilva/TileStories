# Plan tracker -- _3.3 POI Card Look, Feel & Motion -- P0 capability spike (Unity 6000.3.19f1, URP 17.3) (2026-10-22)

(Kept beside `__curr_plan_tracker.md` because the P1a content Worker runs at the same time and owns that file.)

Brief: answer with MEASURED evidence what 6.3 UI Toolkit can do for the card's look. Output: `_3.3` section R8 (can / cannot /
costly, one row per S1-S9), captures in `TileStories/TestEvidence/StyleSpike/`, spike code in `Assets/Dev/StyleSpike/` (dev-only,
nothing in Framework references it), ONE permanent EditMode test `UssVariableChainTests` in `Framework/Tests/Editor`.
No change to Framework/Runtime, shipped USS, tokens or the Editor. Commit only if asked. Trusted commit: 60fd76c (docs-only since the
last green full runs).

## GATE
- [x] UnityMCP answered (telemetry_status); `.clinerules` read (10-structure: the sections this block touches)
- [x] git push: 0 commits ahead of origin, nothing to push
- [x] compile: last Tundra build success / ExitCode 0 (Editor.log line 1090533); the 21 `error CS` lines are older (line ~602800); ScriptAssemblies 09:40
- [x] SDF fonts: `git status` clean for Markers/Fonts (no checkout needed)
- [x] Game view capture (`TestEvidence/StyleSpike/gate_gameview.png`: the room scan + painting, no UI in Edit Mode) + POI Editor capture
      (`gate_poi_editor.png`: window only, Global Scene collapsed by default); window closed, 0 left, ShowPopups restored

## Facts found in the audit (re-verified on disk / by reflection)
- 6.3 IStyle has unityMaterial, filter (Tint Opacity Invert Grayscale Sepia Blur Contrast HueRotate Custom), aspectRatio, transition*; NO backdrop.
- EasingMode: 23 named curves, no cubic-bezier type.
- AccessibilitySettings: fontScale, isBoldTextEnabled, isClosedCaptioningEnabled (+ changed events); no reduce-motion.
- FontDefinition.FromFont(Font) / FromSDFFont(TextCore FontAsset). TMP_FontAsset : TMP_Asset : ScriptableObject (ugui 2.0 / TMP 5.0).
- URP ships `UITKBasicTemplate.shadergraph` (UniversalUISubTarget) -> the S4/S5/S7 graphs are copies of it, extended.
- No Android device (`adb devices` empty) -> S8 device read and S9 = "needs device" lists.
- Existing galleries keep their harness in Framework/Runtime/DevTools; this spike must not -> own asmdef under Assets/Dev/StyleSpike.

## Steps
- [x] S1. `UssVariableChainTests` (6 tests, EditMode, real EditorWindow panel) + fixtures `UssVariableChain/ChainTokens.uss`,
      `ChainConsumers.uss`. 6/6 green; mutation (`--chain-role: var(--chain-primitiv)`) -> 5 red, restored -> 6/6.
      Findings: 2 + 3 levels resolve; fallback only when undefined; cross-sheet ancestor works; a primitive overridden on a
      descendant reaches var(--component) below it (substitution at the USING element, unlike CSS); customStyle holds only the
      element's OWN declarations; a broken link paints transparent silently (no warning).
- [x] S2. a) one .ttf -> UITK takes the ttf (-unity-font-definition: url(ttf) / FromFont) or a TextCore asset; TMP needs TMP_FontAsset;
      a TMP url in USS BLANKS the label silently; -unity-font loses to the theme. b) 9 static cuts (fontTools), PT glyphs all present;
      dynamic asset 5.8 KB -> 2.1 MB after 22 glyphs (writes back); static 122 shipped chars = one 512 atlas @ 48. c) bake-off renders +
      glare: body 12.97:1 -> 2.49:1 under a 35 % linear white layer (sRGB model: 4.36:1). S2FontSharingTests 2/2, S2FontBakeOffTests 1/1.
- [x] 0. Scaffold (SpikeStage, SpikeCost on UI markers -- frame time is vsync in the Editor, DrawChain holds the present wait;
      SpikeEvidence; frame pin; SpikeStageTests 1/1). Sunny-wall stand-in = the shared Dev/MarkerGallery backdrop (azulejo wall in sun).
- [x] S3. filter parses (blur / tint / grayscale) and renders; blur blurs the element's own text too; 1 blur: batches 2 -> 9, UI repaint
      0.04 -> 0.25 ms; 10 blurs: 64 batches, 0.79 ms; radius does not change cost; transition animates it (7.93 px at 0.5 s of 0 -> 16). 3/3.
- [x] S4. SpikeContour.shadergraph (generated: Element Layout UV + Custom Function), 6 s turn, _Animate 0 freezes it; a material on a PARENT
      draws its children too (button + text vanished) -> ring = sibling layer; batches 3 -> 5, UI cpu flat. 2/2.
- [x] S5. SpikeFrost.shadergraph (screen-space lookup of a 1/8 RT, 5x5 tent, tint); Element Layout UV is BOTTOM-UP (caught as a mirror by
      the alignment test, fixed: cell diff 0.011 vs 0.078); ground detail frosted 0.0008 vs tinted 0.0091; block edge white vs ground
      1.2-1.6:1 in all levels, 1 px border min 1.00:1 over the photo; frosted +5 batches (copy camera). Device feed: AR Foundation
      GPU_Blit of ARCameraBackground.material into the RT (documented). 3/3.
- [x] (see above) S2. Fonts (a) one asset vs two (real Label + TextMeshProUGUI); (b) download 6 families + OFL, static SemiBold instances
      (fontTools), TextCore + TMP assets, static vs dynamic, PT glyphs; (c) bake-off renders 3 pairs + glare
- [x] (see above) S3. USS filter: captures; cost (ProfilerRecorder) 0 / 1 / 10 filtered; transition on filter
- [x] (see above) S4. UI Shader Graph contour on a pill (6 s loop), a reduce-motion property; batches before / after
- [x] (see above) S5. Glass over a camera-rendered photo: frosted (1/8 RT + UI shader), tinted, solid; blocks with border + shadow; edge contrast
      on pixels; frosted cost; AR Foundation feed research
- [x] S6. Motion: 30 blocks (USS ease-out-cubic and experimental + Stitch bezier; cubic-bezier in USS throws; no double driving; FLIP start under duration 0; 5/15/30 at once 0.21-0.34 ms) 5/5; USS transitions AND experimental.animation; easings; frame time 5 / 15 / 30; frame sequences
- [x] S7. Shadows (9-slice = SG look, 3 vs 13 batches; dark: invisible) 2/2: 9-slice vs Shader Graph vs border, light AND dark; captures + cost
- [x] S8. AccessibilitySettings (fontScale 1, bold False; reduce motion NotAvailable in the Editor) 7/7 read + logged; reduce-motion code path (Android / iOS) written, not shipped; device checklist
- [x] S9. No device: Tier C measurements owed
- [x] R8 in `_3.3`; 10-structure.md entries; __mixed_TODOs
- [x] Full EditMode 2027/2027 (135.5 s, unfocused); spike assembly 26/26 (42.6 s) -- Full EditMode suite green (UssVariableChainTests by name); spike PlayMode fixtures green
- [x] LivingRoomScene open; final report (60-finishing 6.4)
