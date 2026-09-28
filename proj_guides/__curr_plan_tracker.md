# Current plan: POI Detail Card step 11 (an app registers its own block kind) + card services on the bind context

Gate done 2026-09-28: UnityMCP answered (telemetry_status), Game view capture and POI Editor window capture opened and checked, capture windows
closed, tree committed (only docs + screenshots modified). Baseline: 0 error CS, EditMode 1295/1295 (Unity in background), PlayMode 542: 8 red in
PoiCardTapZoomTests (real touches) -> reproduced with another application in front, fixed at its cause (AddFinger order), 9/9 in the background.

## Part 1 -- card services (Framework)
- [x] 1.1 Runtime/Blocks/CardServices.cs (Add / Get / Has / Remove, static Shared)
- [x] 1.2 BlockBindContext.Services + Service<T>(); BlockStackView copies it; PoiCardHost.Services + CardGalleryHarness.Services
- [x] 1.3 PollResults removed from context / host / harness; PollBlockView asks Service<IPollResults>()
- [x] 1.4 Existing tests moved to Services (CardGalleryTests poll, PoiCardSceneTests seam assert)
- [x] 1.5 CardServicesTests (EditMode) 6/6
- [x] 1.6 Compile + EditMode green; PlayMode (below)

## Part 2 -- the app kind (zero Framework runtime edits)
- [x] 2.1 Assets/Apps/LivingRoom/Scripts/: asmdef, LivingRoomBlocks.cs, SizeComparison/ (definition, rule, view, uss), IFamiliarObjects + FamiliarObjects
- [x] 2.2 Framework tests: CardGalleryChecks extracted; wiring test allows Assets/Apps sheets; Lamp catalog test filters non-built-in kinds
- [x] 2.3 App tests: Tests/EditMode 13/13, Tests/PlayMode 15 (guard mutation-checked: planting the app's name in a Framework file turned it red)
- [x] 2.4 The Lamp gets one size_comparison block (round trip byte-identical first; +71 / -0 per config copy); scene blockStyles += app uss (1 line)
- [x] 2.5 Card Content warning: show_on_wall block + sticky show_on_wall action (Tier3GroupBRulesTests +2)
- [x] 2.6 Captures checked: gallery Card_size_comparison_*, Search_Lamp_SizeComparison(_pt), Editor_BlockLibrary_AppKind, Editor_CardContent_SizeComparison_a/_b

## Docs
- [x] _3.1 (row 11 needs final counts), 10-structure.md, _5.1

## Final runs (fill in)
- [x] full EditMode 1317/1317 + PlayMode 557/557 (0 skipped) after the last code edit; Editor.log no error CS this session
