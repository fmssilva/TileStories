# Plan tracker -- Premium P3a CONTINUATION 2 (close it green; commit point)

Brief: `proj_guides/__AI_worker.md` + "vCONTINUATION 2 -- P3a" pasted in chat. BASELINE: HEAD be33c64 + the uncommitted P3 tree.
Worker: Opus, effort High. The Architect commits. P3b (Local Shade + FAB) is the next brief: STOP after the report.

RULE (review phase): BLOCK = full EditMode + ALL 17 card PlayMode fixtures, every block (P2 shipped reds in fixtures it did not run).

## GATE
- [x] Unity MCP: telemetry_status ok (UnityMCP, TileStories@d41388e4)
- [x] git: master level with origin/master (0 ahead / 0 behind, be33c64) -- pull nothing, push nothing
- [x] fonts: no font / SDF asset in git status
- [x] compile clean: forced compile, ExitCode 0, zero error CS
- [x] Unity c asked before the first PlayMode run (Pronto)

## First run (ThemeFrame + SectionSheet + CardLight + NavRails + PoiCardScene, 89 tests, failures capped at 25)
- ThemeFrame 2: block edges measured at y 54 -- under the ChromeBand (P2) -> test aim (VisibleStack)
- SectionSheet 1: "pages draws the Family Pager" -- the pager / Text Tabs strip are old elements -> Ignore
- CardLight 1: the stroke starts at viewport.yMax - 40, under the floating bar -> test aim (ReachableStack)
- NavRails 4: BringIntoView checks the raw viewport (under the chrome row / bar) -> test aim (IntoReach)
- PoiCardScene 17: ScrollAndTap / ScrollTo leave the target at the viewport's edge -> test aim (TapInReach); and CODE: the bar's
  host + root picked their whole rectangles (16 px gutters, 20 px gap under, 26 px headroom above: transparent, ate the card's taps)

## Steps
- [x] P0 rule line in 40-testing.md (BLOCK) + here
- [x] P1 ThemeFrame (CardTestInput.VisibleStack) + SectionSheet Pages/Tabs openers -> Ignore(OldNavigationElements)
- [x] P2 CODE: NavBarView.Root + the bar's host PickingMode.Ignore, the disc a tap target for the chosen group; PlayMode test (gutter /
      headroom tap reaches the card, the band still jumps)
- [x] P2 aims: PoiCardScene (TapInReach), NavRails (IntoReach), CardLight (start inside the reach); re-run; per-test verdict below
- [x] P3 Layout / Pages / NavBlock / Painting: run for messages; Ignore only what is about the old elements; fix the rest
- [x] P4 confirm the 5 P3's (PoiCardScene 2, Painting 2, Theme 1) + the edited Reveal test (Sections 26/26 in BLOCK)
- [x] P5 BLOCK: full EditMode + 17 fixtures green; looks (start_*, divider_*, follows_* zoomed on the bump, contrast_*); Editor capture
      Detail Card > Sections (dark skin, Unity b); _3.3 build log + _Test_Suite; 10-structure lines; report + ONE commit message

## P5 BLOCK
- full EditMode: 2339 / 2272 / 0 failed / 67 skipped (ended "Unity in front": not counted as a background run)
- 17 fixtures (Unity behind both ends): 292 tests, 0 failed except PoiCardChromeSceneTests 5 -- P2's (default Top Bar now none), a
  fixture no earlier block ran. 4 skipped (the old strip + the old Show Navigation At / Hide While Scrolling the bar replaces in
  CardChromeView), NothingShowsAtPeek fixed (bottom area only); re-run 6 / 2 / 0 failed / 4 skipped. PoiCardRestingEndSceneTests is a
  whole skipped fixture already: the Architect may name another 17th
- looks: follows_bottom_light_3 Start rows whole; the bump a cut-out (zooms in TestEvidence/Agent/); 60 contrast lines >= 8.20:1;
  editor_sections_dark.png; review notes: Start under the band's fade after a jump; Mosaic + Tiles = two ornament bands
- the tree compiles clean; LivingRoomScene is the open scene; nothing committed (the Architect commits)

## The 17 card PlayMode fixtures of BLOCK (named here: no guide lists them)
PoiCardSectionsSceneTests, PoiCardNavBarSceneTests, PoiCardHeroSceneTests, PoiCardChromeMarksSceneTests (P3's own); PoiCardSceneTests,
PoiCardNavRailsSceneTests, CardLightSceneTests, ThemeFrameSceneTests, SectionSheetSceneTests, PoiCardLayoutSceneTests,
PoiCardPagesSceneTests, PoiCardNavBlockSceneTests, PoiCardPaintingSceneTests, PoiCardThemeSceneTests (the 10 attributed);
PoiCardChromeSceneTests, PoiCardRestingEndSceneTests, PoiCardPageChromeSceneTests (the bar / band / end every card draws)

## Per-test verdicts (PART 2: TEST aim or CODE)
- CODE (all of them, first): the bar's host and root picked their whole rectangles -- the 16 px gutters, the 20 px gap under the bar and
  the 26 px room above its edge (transparent: the visitor sees the card there) ate the card's taps. NavBarView.Root + the host
  PickingMode.Ignore, the disc a Clickable (= the chosen group). New test: PoiCardNavBarSceneTests.TheBarsClearRoom_IsTheCards_...
- TEST aim, PoiCardScene 17 (bare ScrollTo left the target at the viewport's edge, under the bar or the chrome row) -> TapInReach /
  IntoReach: story Seguinte / Next x2, CardsOwnState, Knowledge Check MC + image choice, before/after cut, collect, dialogue, feedback,
  fun fact flip, hotspot loupe, next along wall, poll, related, show on wall neighbour, today map Directions, wall locator. Run 2: 16/17
  green; the gallery test then read the scroll before the aim moved it (re-read after IntoReach)
- TEST aim, NavRails 4 (BringIntoView judged the raw viewport) -> IntoReach on the strip: 7/7 green in run 2
- TEST aim, CardLight 1 (the stroke began at viewport.yMax - 40 = on the bar) -> inside ReachableStack: green in run 2
- TEST aim, ThemeFrame 2 (edges measured under the ChromeBand) -> CardTestInput.VisibleStack: green in run 2
- SKIP, SectionSheet 1 (the pager's counter, the Text Tabs strip naming the section) -> OldNavigationElements
- Run 2 (PoiCardScene + NavRails + CardLight + ThemeFrame + SectionSheet + NavBar): 130 tests, 2 failed (the gallery's scroll read;
  the new test's Contains = descendants only) -- both corrected, green by name in run 3

## PART 3 -- old elements, test by test (run 3: 18 red read; run 4: Layout + Pages + NavBlock + Painting + Theme 32 / 21 / 0 failed / 11 skipped)
- SKIP (the old element IS the subject): Layout 5 (Accordion tab tap, Tabs one-at-a-time, Tabs media put away -- switched only by Text
  Tabs taps, re-point it at the bar when tabs are rebuilt --, Tabs with neither bar, Renders); Pages 3 (Family Pager, Bottom Bar slot jump,
  Renders with the pager); Painting 3 (the wall's Top Bar / Menu pick, tab taps, the tab mark under drags)
- FIXED (the layout is the subject, an old element only a side line): Pages AVerticalDrag (the pager-key line dropped; the stroke inside
  the reach), AShortSwipe (the chevrons' visibility lines dropped; the last page by real swipes), APageLeft (back by a real swipe),
  ACardWithNothingToSwitch (the Top Bar line dropped), InsideAPage (the 3D stage IntoReach -- a TEST aim: the bar covered its centre)
- RE-POINTED: NavBlock TheSpyAndTheScroll -- the jump-lands-the-same half taps every Navigation Bar group instead of Text Tabs
- SectionSheet's openers test: SKIP (PART 1)

## PART 4 -- the 5 P3's
- PoiCardScene 2 and Painting 2: green in runs 2 / 4 with no change of their own (the struggle fix of the previous session)
- Theme 1: TheAppOpensLight read a framed block's surface; Open frames none -> the open ground (BlockStackView.OpenStackClass,
  --ts-sheet-open) recolours light -> dark without a rebuild: green in run 4
- the edited Reveal test: in P5's Sections run
