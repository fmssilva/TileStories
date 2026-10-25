# Plan tracker -- _3.3 P4.4c2: living glass and the moving rim light

Base: HEAD 3ec84c8 (code = ed2886c; 4c1b: full PlayMode 1314, full EditMode 2108, background, 0 failed). Do not commit (the Architect does).

## GATE
- [x] push: 0 ahead, nothing to push
- [x] captures: TestEvidence/Editor/gate_poi_editor.png + gate_gameview.png (GUIView.GrabPixels: desktop reads caught a browser twice, deleted)
- [x] compile: ExitCode 0, 0 error CS
- [x] fonts: Oswald / Roboto SDF and Dev/StyleSpike/Fonts unmodified

## Steps
1. [x] Theme data: per mode --ts-family-<k>-light / -light-2 / -sweep (10 families), number --ts-rim-sweep-arc; ThemeResolver.TryNumber;
       regenerate Theme.default.uss; EditMode: FamilyIconAndWordsTests (every family finds them, dark sweep weaker + narrower), resolver test
2. [x] LitBlockRule (Runtime/Blocks/Navigation): the shown block whose centre is nearest 45 % of the viewport; EditMode tests
3. [x] Shaders: Runtime/UI/Cards/Light/SectionGlass.shadergraph + RimSweep.shadergraph (spike S4 shape, _Clock, _Animate, colours, size);
       materials Resources/TileStories/Card/SectionGlass.mat, RimSweep.mat
4. [x] CardLight (plain C#): base materials, per-layer instances, theme colours + ModeChanged, size, Clock tick, ReduceMotion
5. [x] SectionSheets: the light layer (sibling at index 1, bare sheets none); BlockStackView owns CardLight + RimLightView
6. [x] RimLightView: one sweep layer, LitBlockRule on scroll / geometry, fade out -> move -> fade in
7. [x] PlayMode CardLightSceneTests (real scene, injected clock, pixels): glass structure + colours both modes; blobs move / frozen;
       one sweep in the rule's block, real drags move it, dark weaker; reduce motion live; frame sequences + judged
8. [x] Ride-along 1: tabs opener = the line only (re-base SectionSheetSceneTests tabs)
9. [x] Ride-along 2: wall Block Order = by_family through the Editor path; re-base CardNavModelTests:130, DetailCardEditorTabTests:1389 (+ any found)
10. [x] Cost: gated CardLightMeasureSceneTests (Batches, UI update + repaint ms) before / after, The Lamp at Full
11. [x] Runs: full EditMode; targeted Card/Poi/Theme/Section fixtures; FULL PlayMode (background)
12. [x] Docs: _3.3 Build log 4c2 + cost table + TODOs; 10-structure entries; 40-testing RUN TIMES if moved
13. [x] LivingRoomScene open; final report 6.4 + review questions

## Re-base log (old -> new, from source)
- SectionSheetSceneTests tabs: "its filled pill + icon" -> no pill, the line alone at the sheet's full width (ride-along 1)
- CardNavModelTests.TheLampInTheDefaultBlockOrder_...: precondition "shipped = end_families_last" -> ShippedEndFamiliesLast() vs new CardNavigationSettings() (ride-along 2)
- LivingRoomBlocksTests.TheShippedLamp_HoldsOneSizeComparison_...: "after show_on_wall" read in the builder's order -> in the AUTHORED list (By Family regroups it)
## Decisions taken by measurement
- 6 (gating): every sheet lit = SetPass 7.9 -> 40.0; near-view only -> 14.0 -> 16.0: SectionSheets.ShowLightsNear
