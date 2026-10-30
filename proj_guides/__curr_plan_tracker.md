# Plan tracker -- R1: retire the old navigation elements + the header turntable under Reduce Motion

Brief: `proj_guides/__AI_worker.md` "R1" (`_3.3` fourth round > "The old navigation elements -- inventory" + "P3c review, second pass" d;
TODO [R1]). Worker: Opus, effort Medium. TEST PLAN: BLOCK = full EditMode + the 16 card fixtures (+ PoiCardModel3DSceneTests for PART 3).
OUT OF SCOPE: the story block, rebuilding any kept element.

## GATE
- [x] MCP live (TileStories@d41388e4, telemetry_status ok, idle, Play Mode off); HEAD = origin/master = 7d47409, nothing to push
- [x] SDF fonts: UITK 6/1024, UITK Halo 18/2048, TMP 6/1024, marker Oswald / Roboto static -- all unmodified; no InitTestScene leftovers
- [x] no opening test run (00-process PLAN 2)
- [x] compile clean before the first run (refresh + Editor log)

## Decisions (autonomous)
- RETIRE (deleted): text_tabs, icon_tabs (+ TabsNavView), segmented (+ NavFitRule, Groups table, CardNavGroup, family.group, NavGroupRule,
  CardNavModel.Groups, NavChoice.Groups), bottom_bar, dock, edge_scrubber (+ EdgeScrubberRule), fab_dial (+ FabFanRule), fab_radial
  (+ RadialMenuRule), grid_overlay, nav_icon_grid / nav_rails (+ NavRailRule) / nav_bento (+ BentoTileRule) blocks, Auto Nav Block,
  the Bento layout (CardBentoView, BentoLayoutRule, the full view's section page)
- The Floating and Edge slots hold no element after this: NavSlotKeys.Floating / Edge, config floating / edge, NavSlotRule.Floating/EdgeElement,
  LayoutOwnsTheEdge, NavSlots' edge host go; FloatingSlot stays (the placed-model chip)
- KEEP (code + registration, fixtures skipped, NOT in the Editor): kind_chips, icon_tabs_chips, progress_map, family_pager, bar_sheet, next_up,
  drawer_tree + CardMenuOverlay + facet chips, nav_contents
- Navigation Presets DELETED, not kept: all nine were lists of retired elements and wrote the retired keys (a TODO to rebuild them on the bar)
- Tabs with the bar off and no Top / Bottom element: the Family Pager (was Text Tabs)
- Editor: the Elements group, Preset row, Groups table / Group column, Labels / Show Counts / Bar Slots rows, their help, warnings go
- Shipped wall: Alfama's block_6 / 8 / 10 (retired kinds) removed from both configs; re-saved through the classes
- Old config: plain JSON ignore + existing fallbacks (unknown layout = scroll, unknown kind = skipped); ONE EditMode test

## PART 1 -- retire
- [x] 1a runtime: views, rules, registry, slots, layout rule, bento, nav blocks, auto nav, groups, config fields, CardOptions, CardStrings, USS
- [x] 1b DevTools: CardNavGalleryDefinitions / CardGalleryDefinitions.Navigation entries
- [x] 1c Editor: rows, help, presets row call, groups table, warnings
- [x] 1d tests: retired fixtures deleted, kept ones compile and stay skipped, registry / layout tests updated
- [x] 1e shipped configs re-saved (both copies), retired blocks gone; the one old-config EditMode test
- [x] compile clean

## PART 2 -- [x] `_3.3` TODOs: one [rebuild] line per kept element

## PART 3 -- [x] HeaderBlockView: Auto Spin = !ReduceMotion; PlayMode test (on: yaw unchanged over 2 s; off: turns)

## PART 4
- [x] LOOK: Editor Detail Card / Navigation (dark skin), the card at Half / Full
- [x] BLOCK: full EditMode 2240 (1 red fixed, its fixture 60 / 60) + the 16 card fixtures + Model3D + ShowAfterReading 319 / 319 (38.2 min)
- [x] docs: _3.3 build log, _Test_Suite_1 (removed fixtures), 10-structure; report + commit message; AskUserQuestion

## The 16 card PlayMode fixtures of BLOCK
PoiCardSectionsSceneTests, PoiCardNavBarSceneTests, PoiCardHeroSceneTests, PoiCardChromeMarksSceneTests, PoiCardSceneTests, CardLightSceneTests,
ThemeFrameSceneTests, SectionSheetSceneTests, PoiCardLayoutSceneTests, PoiCardPagesSceneTests, PoiCardPaintingSceneTests, PoiCardThemeSceneTests,
PoiCardChromeSceneTests, PoiCardPageChromeSceneTests, PoiCardLocateFabSceneTests, PoiCardBrightPageSceneTests (+ PoiCardModel3DSceneTests)

## HANDOFF (2026-10-30) -- the 16 CORE PlayMode reds after R1

State: R1 built, uncommitted on 7d47409 (commit message: "Retire the old navigation elements and still the header turntable under Reduce
Motion"). R1's BLOCK green: full EditMode 2240 (its last red fixed, DetailCardEditorTabTests 60 / 60), the 16 card fixtures + Model3D +
ShowAfterReading 319 / 319. CORE EditMode 120 / 120. CORE PlayMode (`category_names: ["Core"]`, job 4217cd30) 230 run, 16 failed; the same
16 failed again run alone (job e8e2003e, 23 tests). The developer chose a handoff; nothing is fixed. This tells you WHERE TO LOOK -- verify
every claim yourself. Whose reds they are is NOT proven: no baseline run at 7d47409 was made.

Raw failures (NUnit messages, verbatim):
- PoiCardAudioSceneTests.ARealTapOnTheHerosListenAtFull_Plays_TheCaptionsFollowTheClipTime_AndARealScrubMovesThemWithIt -- "Expected: True But was: False"
- PoiCardAudioSceneTests.InPortuguese_TheChipThePlayerAndTheMiniPlayerSpeakPortuguese -- "its first Portuguese cue (0.3 s to 4.46 s) Expected string length 64 but was 0"
- PoiCardExpandableSceneTests.TheLamp_AllExpandable_HoldsNoMediaUntilARealTapOpensARow_AndARealTapClosesIt_GivingTheOwnerItsSlotBack -- "only the header and the sticky CTA bind a stack view Expected: 2 But was: 1"
- PoiCardLiveUpdateTests.BlockOrder_TheLampsCardRendersInAllThreeOrders_AndTheOptionIsLive -- "nothing was dropped by ordering them Expected: 62 But was: 61"
- PoiCardStorySceneTests.ThePaintingsStoryPreview_ShowsItsPictureEyebrowTitleAndLede_CappedWithItsOpenRow x4 (en/pt x light/dark) -- "precondition: the rows keep the text's margin Expected: greater than 0.0f But was: 0.0f"
- PoiCardStorySceneTests.ThePreviewsTapNothing_OpensOnlyFromItsOpenRow_AndBlockEntryNone_IsSimplyThere_OnTheRealCard -- "a real tap on the Open Row still opens the page Expected: True But was: False"
- PoiCardVideoSceneTests: AVideoTappedWhileAnotherPointsNarrationFadesIntoTheLamps... ("a real tap inside the fade started the video Expected: Playing But was: Idle"),
  ClosingTheCard_StopsTheVideoAndGivesTheClipBack... ("Expected: Playing But was: Idle"), InPortuguese_TheVideosSpeakPortuguese... ("Expected: String ending with
  "A praca e o castelo, capitulo a capitulo" But was: <string.Empty>"), TheAudioGuidePauses_WhenARealTapStartsTheVideo... ("Expected: Playing But was: Idle"),
  TheChaptersVideo_IsATeaser... ("a real tap opened it full screen Expected: True But was: False"), TheLamp_InlineVideo_FromTheWallsFolder... ("a real tap started it
  Expected: Playing But was: Idle")
- ThemePipelineTests.TheCardsRoots_DeclareOnlyTheirOwnTokens_AndPaintWithTheThemesByInheritance -- "the bar paints --ts-bar-glass (the theme's value) Expected: RGBA(255, 255, 255, 153) But was: RGBA(0, 0, 0, 0)"

Where to look (leads, not conclusions):
1. FIRST a baseline: the 16 at 7d47409 (stash R1 without the unrelated IPCE files, recompile, run them, restore, check `git status`). It decides
   whether any is R1's.
2. Expandable + LiveUpdate: both one bound view short on the Lamp; Expandable names the sticky CTA. `NavBarRule.StickyActionShown`
   (`BlockStackView.cs` ~440) -- P3d turned the old sticky action off under the bottom bar and pinned 9 tests, not these CORE ones. R1's
   diff does not touch that rule.
3. ThemePipelineTests:78 reads `HeaderSlot.resolvedStyle.backgroundColor` against `--ts-bar-glass`; the token is declared in
   `Theme.default.uss` but no USS reads it (grep) -- the bar was restyled in Premium P2. R1 removed no line using it.
4. Video / Audio / Story real taps that do nothing: check what lies over the tap point (P3e's floating locate-pin layer
   `LocateFabView.Layer` sits over the whole sheet; the Navigation Bar; the tap helpers' `CardTestInput.ReachableStack`). Story's margin 0
   may be its own regression (Premium P3 restyled sections).
5. The `_3.3` build log records no CORE run for Premium P1-P3e (their BLOCKs list the card fixtures), so any of those blocks could own them.
