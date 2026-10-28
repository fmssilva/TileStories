# Plan tracker -- Premium P3b: old navigation out of the runs, a fluid bump, steady hero words

Brief: `proj_guides/__AI_worker.md` + "P3b" pasted in chat (developer's P3a review, `_3.3` fourth round > "P3a review" a-f).
BASELINE: HEAD b89c9d7 = origin. Worker: Opus, effort High. STOP after the report: P3c = Local Shade on the bar's labels + the FAB.
RULE (review phase): BLOCK = full EditMode + the card PlayMode fixtures (the list below, minus the fixtures now skipped whole).

## GATE
- [x] Unity MCP: telemetry_status ok
- [x] git: HEAD = origin/master = b89c9d7, fetch brought nothing; nothing to push
- [x] fonts: no font / SDF asset in git status
- [x] compile clean: forced compile, ExitCode 0, zero error CS
- [x] baseline full EditMode: 2339 / 2272 / 0 failed / 67 skipped (Unity behind; a first try found Unity left in Play Mode: stopped, re-run)
- [x] Unity c asked before the first PlayMode run (Pronto); no Editor capture this block (no Editor change)

## PART 1 -- old navigation out of the runs (fixture level)
- [x] scratch tool split_fixture.py: X -> abstract XFixture (set-up + helpers, private -> protected) + X (its own tests, same name) +
      XOldNavigationTests ([Ignore(TestSkips.OldNavigationElements)])
- [x] whole fixtures skipped (the old element IS the subject): EditMode EdgeScrubberRuleTests, NavBottomElementRulesTests, NavFitRuleTests,
      NavRailRuleTests, BentoTileRuleTests, BentoLayoutRuleTests, AutoNavBlockTests, NavigationWarningRuleTests; PlayMode
      PoiCardNavContentsSceneTests, PoiCardNavRailsSceneTests, PoiCardNavBentoSceneTests, PoiCardBentoLayoutSceneTests, PoiCardNavBlockSceneTests
- [x] mixed fixtures split (test-level skips -> the sibling fixture): DetailCardEditorTabTests, NavElementRegistryTests, FloatingSlotTests,
      CardLayoutRuleTests (+ TabsWithNoBar), PoiCardChromeSceneTests, PoiCardHeroSceneTests, PoiCardLayoutSceneTests, PoiCardPagesSceneTests,
      PoiCardPaintingSceneTests, SectionSheetSceneTests; newly: NavTopElementRulesTests (Groups / progress map / rule 7 / Labels),
      NavFloatingRulesTests (dial, radial, dims), SectionSheetMeasureSceneTests (rails, bento), PoiCardSheetDragSceneTests (the top strip)
- [x] scope: HeroMeasureSceneTests measures the default only (no Menu matrix: the bar draws no menu)
- [x] grep: no test outside an old-navigation fixture writes top / bottom / menu / floating / edge / auto_nav_block, none outside the bar's own
      fixtures writes a bar option
- [x] inventory in `_3.3` (one line per element: what it is, what the bar covers, rebuild / retire); `_Test_Suite_1.md` section 2
- [x] compile + full EditMode (skips moved, nothing red)

## PART 2 -- the Raised bump is a fluid bell (P3a review a)
- [x] NavBarShapeRule.Path: crest = the disc's own top arc (peak = the disc's top, tangent there flat), left at the angle where the bell leaves it,
      then a soft shoulder to the edge (C1 at every join); near a side the shoulder narrows into the top corner (corner shrinks only as needed)
- [x] tokens: --ts-navbar-bump 18 -> 12 (peak = disc top 14 at room 26), --ts-navbar-bump-half 38 -> 50 (wide shoulders), --ts-navbar-disc-gap
      removed (DiscTop(room, bump) = the bell's peak)
- [x] NavBarView: the bell centred on the disc (Centre), disc radius passed
- [x] EditMode NavBarShapeRuleTests rewritten: peak = disc top +-0.5; C1 at every join; monotonic each side; the disc's top arc IS the outline;
      first / last group smooth; mirror at Top; flat without Raised
- [x] PlayMode NavBar fixture: the disc-gap test -> the peak on the disc's top (Bottom / Top, light / dark renders); zoomed renders looked at

## PART 3 -- the hero's words at Half (P3a review b)
- [x] MEASURE first: resolved opacity of eyebrow / title / line / Listen at Half, no scroll; ring contrast on the real hero (Ribeira Palace)
- [x] HeroMotionRule.TextOpacity: driven by the scroll only (whole at rest wherever the stop puts the words; fades as the scroll carries them
      to the chrome's foot); EditMode tests
- [x] PlayMode: open at Half, no scroll -> opacity 1 and >= 7:1; a real scroll -> the opacity the rule gives for that offset
- [x] Local Shade NOT built: after the fix every word >= 7:1 as seen (9.09-14.48:1); before: opacity 0.029 at Half, 1.04-1.06:1

## PART 4 -- a jump lands below the Top Band; one ornament per boundary (P3a review e, f)
- [x] PoiCardSheetView: LandingInset = max(the strip's foot, the Top Band's foot); hero words finish fading there too
- [x] SectionStyleRule.Of: a Tiles / Wave Start takes the Divider's place (none before it); EditMode test
- [x] PlayMode (Sections fixture): a jump to each section lands its Start's top at or below the band's foot; a Divider before Fade / Plain
      still shows, none before Tiles / Wave

## PART 5 -- BLOCK, look, report
- [ ] compile; full EditMode; the card fixtures (list below)
- [ ] renders looked at (bump zoomed Bottom / Top light / dark; hero at Half; a section after a jump)
- [ ] `_3.3` build log + inventory, `_Test_Suite_1.md`, `10-structure.md` lines; LivingRoomScene open; report + ONE commit message

## The card PlayMode fixtures of BLOCK
PoiCardSectionsSceneTests, PoiCardNavBarSceneTests, PoiCardHeroSceneTests, PoiCardChromeMarksSceneTests, PoiCardSceneTests, CardLightSceneTests,
ThemeFrameSceneTests, SectionSheetSceneTests, PoiCardLayoutSceneTests, PoiCardPagesSceneTests, PoiCardPaintingSceneTests, PoiCardThemeSceneTests,
PoiCardChromeSceneTests, PoiCardPageChromeSceneTests (14; PoiCardNavRailsSceneTests and PoiCardNavBlockSceneTests now skipped whole,
PoiCardRestingEndSceneTests already was)

## Results so far
- EditMode after the block: 2341 / 2189 / 0 failed / 152 skipped (Unity behind both ends)
- the 9 reds of the first targeted run were the tests' aim (EdgeAt sparse samples, a dark hero pixel near the dark fill, the hairline's AA at
  1.5 pt; a divider "gap" assertion (f) made obsolete) -- corrected; 25 / 25 green by name
- hero words measured before / after: TestEvidence/Premium/P3b/hero_words_at_rest_BEFORE.txt / hero_words_at_rest.txt
