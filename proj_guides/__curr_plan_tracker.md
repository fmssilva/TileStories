# Plan tracker -- P3d: the Reveal both ways, Tint Depth, the "Show me where it is" FAB

Brief: `proj_guides/__AI_worker.md` "P3d" (`_3.3` fourth round > "P2 review" e, "P3c review" c-d, "P3c review, second pass" b).
Worker: Opus, effort High. TEST PLAN: BLOCK = full EditMode + the card fixtures (list at the end) on the FINAL state. CORE deferred.
OUT OF SCOPE: retiring the old navigation (next block), the story block.

## GATE
- [x] MCP live (telemetry_status ok); origin = HEAD = a663ec1 (the brief commit), nothing to push; SDF fonts unmodified (git clean under both Fonts/)
- [x] Play Mode off; compile clean (last ExitCode 0 after the log's old `error CS` lines); no InitTestScene
- [x] baseline full EditMode (Unity c): 2352 / 2200 passed / 0 failed / 152 skipped, 295 s, Unity behind both ends
- [x] captures (Unity b): TestEvidence/Editor/p3d_rows.png (dark skin); Game-view renders TestEvidence/Premium/P3d/

## Decisions (autonomous)
- Reveal progress on the Start row's (the edge's) BOTTOM: 0 at the bottom bar's resting band top (no bottom bar: the viewport's foot),
  1 a third of the viewport up from its foot; each tile its own 25 % window of the travel, in order. Once = once whole, stays whole.
- Tint Depth 40..340 (default 120), from under the Start row; Bands keeps its whole-section flat tint.
- FAB: only while the bar is on, Half / Full (not Peek); 12 above the bar's highest point (the raised disc's top) or 12 above the foot
  / a shown footer; hides with Hide While Scrolling; the hero's words column ends 12 before it (lifting them breaks Half).
- Sticky Action: Automatic (default: off while the bar is at the Bottom) / Shown / Hidden; Locate Button on / off.

## PART 1 -- the Reveal both ways (Motion > Replay)
- [x] LookKit/Reveal.cs: RevealOptions (Window, FadeShare, Ease, ReduceMotion) / RevealRule (Progress, Held, TileProgress, Turn, Alpha)
- [x] CardSectionSettings.replay + CardOptions.SectionReplay* + SectionStyleRule.Replay; Style.Replay
- [x] OpenSectionView.Scrolled(progress) (no reveal ticker; Drift starts the first time p > 0); SectionSheets.OnScrolled(footInset);
      BlockStackView.FootInset (the sheet: the bar's FootClearance at Bottom); RimLightView passes it
- [x] Editor: Sections > Replay after Motion (hidden under Still), (i); CardSectionsRows
- [x] EditMode: LookKitRulesTests (reveal), SectionStyleRuleTests (Replay), CardSectionsEditorTabTests (Replay row)
- [x] PlayMode PoiCardSectionsSceneTests: REAL drags -- half ~ half the tiles, a third up all, back down past the bar 0 (Every Pass) /
      16 (Once); Reduce Motion whole throughout

## PART 2 -- Tint Depth
- [x] CardSectionSettings.tint_depth = 120 (40..340); SectionStyleRule.TintDepth; AmountAt / Stops over it (FadeLength gone)
- [x] Editor: Sections > Tint Depth slider after Intensity, (i)
- [x] EditMode: SectionStyleRuleTests (default, clamp, old config, amounts at 0 / depth / below)
- [x] PlayMode: on the render tint at the ground's top, ~paper at the depth and below; eyebrow + body >= 7:1 (existing test)
- [x] Live push (Phase B): a real Tint Depth drag + a Replay pick in the real window reach the open card; Ctrl+Z

## PART 3 -- the FAB (+ the gallery's Sticky-look entries pin Sticky Action Shown: their subject)
- [x] CardNavBarSettings.locate_button = true, sticky_action = "auto"; CardOptions.NavBarSticky*; NavBarRule.LocateButton / StickyActionShown
- [x] LocateFabRule (Shown, Bottom, WordsClear); LocateFabView (56 accent disc, the show_on_wall mark in --ts-on-accent, 44+ target,
      tooltip = show_on_wall_button, a tap = ShowOnWall); tokens --ts-fab-size / --ts-fab-gap; USS
- [x] Sheet wiring (construct, bind, place, hide with the scroll); hero words right clear; stack drops footer + hero Primary when off
- [x] Editor: Navigation Bar > Locate Button, Sticky Action after Hide While Scrolling, (i); NavigationBarRows
- [x] EditMode: LocateFabRuleTests, NavBarRuleTests (defaults), NavigationBarEditorTabTests (rows, picks one undo step)
- [x] PlayMode PoiCardLocateFabSceneTests: real tap at Half / Full -> Peek, selection kept; never meets the bar's items (every chosen
      group, Bottom / Top) or the hero's words (Half / Full); 44+; Hide While Scrolling; light / dark renders, mark >= 4.5:1; sticky matrix

## PART 4 -- the unchosen labels on a full-size render (light / dark, every fill): >= 7:1 or fix
- [x] the labels test writes a fresh file per case, chosen / unchosen marked; last run's worst unchosen 8.41:1 (bottom glass light)
- [x] re-measured in BLOCK: 120 unchosen readings, worst 8.41:1, all >= 7:1 -- no fix

## PART 5 -- DONE: docs (_3.3 build log, _Test_Suite_1, 10-structure, _5.1's bar rows), BLOCK (full EditMode 2358 / 2206 / 0 / 152; card fixtures 286 + hero re-runs 67 / 67), LOOK, report

## The card PlayMode fixtures of BLOCK
PoiCardSectionsSceneTests, PoiCardNavBarSceneTests, PoiCardHeroSceneTests, PoiCardChromeMarksSceneTests, PoiCardSceneTests, CardLightSceneTests,
ThemeFrameSceneTests, SectionSheetSceneTests, PoiCardLayoutSceneTests, PoiCardPagesSceneTests, PoiCardPaintingSceneTests, PoiCardThemeSceneTests,
PoiCardChromeSceneTests, PoiCardPageChromeSceneTests (14) + PoiCardLocateFabSceneTests (new) + EditMode-hosted live: LivePlayModeNavigationBarTests,
LivePlayModeCardSectionsTests (new)
