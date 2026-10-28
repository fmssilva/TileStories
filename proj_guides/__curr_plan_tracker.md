# Plan tracker -- Test tiers BLOCK / CORE / FULL (first Test Suite Review: audit + tiers + biggest savings)

Brief: `proj_guides/__AI_worker.md`. Spec: `skills/Test_Suite_Review.md`, `40-testing.md` 4, watched flakes in `__mixed_TODOs.md`.
Do not commit (the Architect commits). Result: `proj_guides/_Test_Suite_1.md`.

## GATE
- [x] UnityMCP up (telemetry_status ok)
- [x] git: 0 ahead of origin, nothing to push; SDF fonts unmodified; IPCE/ left alone
- [x] compile confirmed (zero error CS, both test dlls 18:53) -- cleared the orphan job left by an aborted run (player stopped, 18:39)

## P1 -- measure
- [x] FixtureTimes (pure: result tree -> fixture rows -> CSV) in Tests/Editor/FixtureTimes.cs + FixtureTimesTests (ran green inside the full EditMode)
- [x] ICallbacks writer registered in TestRunRecorderHooks -> TestEvidence/Harness/fixture_times.csv every run (works: 218 / 102 fixtures)
- [x] baseline full EditMode (behind/behind): 2298, 6 failed (LiveSync badge x5: wall's marker_use_badge now false; NavPreset: wall matches no preset), 1 skipped -> scratch em_before.csv
- [x] baseline full PlayMode (behind/behind, 19:04-20:32, ~88 min): 1461, 142 FAILED in 51 fixtures, 44 skipped -> scratch pm_before.csv
- [x] UnityMCP reconnected after a Unity restart (job ec7ac2d2 gone; not re-run, per the developer)
- [x] EditMode 6 fixed, own data: LiveSync badge rows (BadgeRow: Setup marker_use_badge = true) 117/117; NavPreset test turns the bar to
  the framework default (off) and starts from its own real pick of Long read 1/1 -- both runs Unity behind/behind

## Classification of the PlayMode reds (from TextTabs, Layout, Chrome: 16 of 19 red, read message by message)
Facts on disk: S4a changed the FRAMEWORK defaults, not only the demo wall: CardContainerSettings.full_height = "top" (Full = 844,
no 48 px gap), the hero = the whole viewport at Full, the chrome (strip / bar / footer) only once the hero is scrolled away
(CardTestInput.PastTheHero / ReadPastTheHero already re-based by S4a). SearchSceneFixture pins navigation to the framework default,
so the demo wall's Navigation Bar ON does NOT reach these fixtures (bar off there).
- RE-BASE (old look pinned where S4a's new design is the default):
  - TextTabs.TheLampsStops "full leaves the top gap 796, was 844" -> Full is 844 (full_height top)
  - Chrome.TheLampAtHalf "hero as tall as Half 337, was 844" -> the hero is the whole screen
  - TextTabs strip at Full / sideways drag / down drag on the strip / tap every tab -> the strip is chrome: it shows only once the hero is read
  - Layout tabs / accordion tab taps -> same (the tab strip waits for the hero)
- WALL CHOICE: none in these three (navigation pinned); full EditMode: 6, fixed.
- HARNESS (found while re-basing; a re-base of the TEST's input, not of the product): the stack runs UNDER the floating header
  (y 7-51) and strip (y 55-105) since S4a -- a finger that starts there takes the sheet or a tab. The Chrome "footer at Full once
  the hero is read: False" was this, NOT a product bug: ReadPastTheHero's 30 px move up started at viewport.yMin + 20 = on the
  header, so the scroll memory never saw an up move (travel 1138, hidden by the scroll True). Same for Layout's accordion taps (a
  row "in view" under the strip: the tap hit the strip).
- PRODUCT BUG: none found in these three.
- HERO CONTRAST [Ignore]: none in these three.

## The five re-base patterns (apply them to the other red fixtures; each from its source)
1. A chrome part's "shown" = displayed + visible + NOT CardChromeView.TopHiddenClass (the strip floats: absolute either way);
   never `position == Relative` (that was the in-flow strip).
2. At Full (and Half) the strip / bar / footer wait for the hero: a test of the strip reads the hero away first
   (CardTestInput.PastTheHero / ReadPastTheHero), then asserts it shows.
3. Where a finger lands / what "in view" means: CardTestInput.ReachableStack(sheet) (NEW) -- the viewport minus the floating header,
   the shown strip and the shown bottom area. Every fixture-local DragStack / InViewAndStill / tap-into-view should use it.
4. The stack's room: StackRoom = the stop's height - the stack's top; only the bottom area takes room (header and strip float).
5. Stops / hero from the source: Full = SheetStopRule.Compute(..., SheetStopRule.FullTopGap(container.full_height, --ts-sheet-top-gap));
   the hero = HeroRule.Height(stops, 0) = the whole sheet at Full, its action in its lower half (WordsBottom), only its top at Half.

## Re-base log (test -> new expectation -> source)
- TextTabs.TheLampsStops: Full from FullTopGap(full_height) (844, was 796); the strip floats over the stack's top, inside it (was "above
  the stack"): CardChromeRule TopFloats, SheetStopRule.FullTopGap
- TextTabs.{TapEveryTab, SidewaysDrag, DownDrag}: OpenWithTheStrip (Full + PastTheHero + strip shown) before touching the strip
- TextTabs.{BelowShowNavFrom, StripAtFull}: strip shown = pattern 1, after PastTheHero
- Chrome.TheLampAtHalf: hero = HeroRule.Height (the sheet at Full), top in view at Half, runs on under Half's foot, the action in its
  lower half (was "the whole hero in view at Half"); room = StackRoom - footer (was room - footer - strip)
- Chrome.TheHeroReadAway / NothingShowsAtPeek: StripShown (pattern 1)
- Chrome.ARealDragDown: StackRoom - footer / StackRoom (was Room - strip - footer / Room - strip); DragStack inside ReachableStack
- Chrome.TheContentShare: both readings start past the hero (the footer waits for it, layer 4)
- Chrome precondition messages now carry ChromeState() (offset, hero bottom, shown parts, scroll memory)
- Layout.TapTab: reads the hero away when the strip is hidden, requires it shown; InViewAndStill = ReachableStack
- Results (BLOCK, Unity behind/behind): TextTabs 7/7, Layout 6/6, Chrome 6/6
- Group 1 (8 fixtures, behind/behind): TopElements 15/15 and HeroMeasure 8/8 green as they stand; 27 reds in the other six:
  - BottomBar / BottomElements / Menu DragStack: pattern 3 (ReachableStack) -- the up-moves started on the floating header (7 reds)
  - Menu.TheLampsStops "a touch target wide 48, was 44": S4a sized the round bar buttons --ts-bar-height (44, PoiCard.uss); expected
    from that token + a >= 44 pt guard (WCAG 2.5.5)
  - NavBlock.ARealTapOnEveryTile "tile 7 on screen": the hero is Full's whole view, the grid after it -- each tile scrolled to the
    middle of ReachableStack, asserted there before the real tap
  - Hero.EveryWordOfTheHero x16 (1.3-1.5:1; the render: no scrim under the words at the hero's foot) -> [Ignore] hero contrast
  - second pass: BottomBar Room -> StackRoom (pattern 4); BottomElements / NavBlock / Pages: PastTheHero before a strip or bar tap
    (pattern 2); all four green (BottomBar 8/8, BottomElements 8/8, NavBlock 6/6, Pages 11/11)
- Group 2 (RestingEnd, NavOptions, PoiCardScene, SearchSettingsField, TapZoom, PlaceInAr, NavScene, Model3D): RestingEnd 11 + 5
  gated skips, NavOptions 27/27, SearchSettingsField 33/33, NavScene 18/18 green as they stand
  - WALL CHOICE (new): the demo wall opens at Half since S4a (container.open_stop "half"; framework "peek"): SearchSceneFixture
    now pins card_settings.container to the framework default too (ShippedContainer keeps the wall's) -- 4 "opens at Peek" reds
  - PoiCardScene.DraggingUp: Half shows the hero's top, the hero runs on under Half's foot; Full's gap = FullTopGap(full_height)
  - TapZoom true/false: IntoReach (pattern 3) before both fingers; the sideways answer under watch (diagnostic message)
  - Model3D header drag: the finger in the middle of the stage's reachable part (the hero's centre is below Half's screen)
  - PlaceInAr: ScrollAndTap into reach (pattern 3); the X tap that does not close: diagnostic (what the X's centre hits)
- Wide run (34 fixtures) + two follow-ups: 17 green as they stand; re-based / fixed:
  - pattern 2 (strip / bar waits for the hero) in Painting, NavCombinations.TurnTo, ShowAfterReading.TapTab, BottomElements, NavBlock, Pages
  - pattern 3, now ONE shared helper: CardTestInput.IntoReach / TapInReach (to the reach's middle, then three frames still -- a set offset
    never stops a running fling); used by NavBlock, PlaceInAr, TapZoom, NavContents, Bento; Expandable.InViewAndStill = Layout's
  - ThemeFrame edge contrast: "in view" = ReachableStack (it measured the chrome over a block)
  - Listen at the hero's foot: RealAudio / VideoScene open at Full; Audio "Listen, 2:44" + its stop Full (test renamed ...AtFull...)
  - Bento Community row: the rule's height, >= the minimum (Full 844 gives it 68, not 64)
  - CardLight: past the hero; rim layer index = BlockFrame.PartCount (S4a added the groove); CardVideoGallery poster under the top shade
  - WALL CHOICE: MarkerConfigDrivesMarker owns marker_use_badge = true
  - PRODUCT (S4a, small, fixed): PoiCardSheetView landing inset measured from the stack's top (was the sheet's: 1 px low, the spy
    disagreed with the screen at a boundary -- InterleavedSpy "10741: 2 vs 3", screen 104 / card 105). CardLight's sweep band read
    from the theme was tried for F3 and changed nothing: reverted, F3 stays open (named skip)
  - PRODUCT (S4a, reported, not fixed: design choice) F1: at Full (no top gap) the floating placed-model chip lies over the X
    (panel.Pick at the X = poi-card-chip-remove) -> two PlaceInAr tests [Ignore] F1
  - CONTRAST F2: the gear on the S4a see-through bar 6.6:1 -> CardSettingsGallery gear test [Ignore] F2 (premium design block)
  - FINDING F4 (named [Ignore]): TapZoom true/false swipe -- vertical and sideways never pass in one run under real fingers since S4a

## NEXT (exact next sub-step for the following session)
State: T1 closed (second session), nothing committed. FULL PlayMode 1339 / 0 failed / 67 skipped, 82.1 min (was 89.0 min, 142 failed);
full EditMode 2298 / 0 failed / 1 skipped, 3.9 min; CORE 227 + 118 tests, ~17.5 min. Skips: 25 named findings with owners + 42 measurements (_Test_Suite_1.md 2).
1. The Architect reviews and commits T1.
2. Next test-suite block = _Test_Suite_1.md section 5 chunk 1: the gallery scene loaded once per fixture (CardGalleryTests 573-759 s,
   CardNavGalleryTests 404-499 s, CardStoryGalleryTests 80 s). Measure first: one fixture with include_details, split scene load vs case.
3. F1 / F2 / F3 / F5 / hero contrast belong to the premium design block; F4 (TapZoom true / false) to the next card block.

## P4 -- reds and flakes
- [x] leaked window doubling the 9 DetailCard navigation tests: find the opener, fix at source
- [x] tests reading the developer's LivingRoom choices: LiveSync badge x5, NavPreset (EditMode); the container's open stop (SearchSceneFixture pin), Use Badge (MarkerConfigDrivesMarker)
- [x] watched flakes: cause or a named skip carrying its finding id

## P2 -- CORE
- [x] [Category("Core")] on the cheapest test per critical path (brief's list)
- [x] time CORE (category_names Core), target 25-30 min

## P3 -- BLOCK
- [x] fixtures > ~3 min: split / slim (largest savings that fit), every removed case's proof named
- [x] rest -> fix list in block-sized chunks with expected saving

## P5 -- docs
- [x] 40-testing.md 4.2 "Which tests, when (three tiers)" (CORE ~17.5 min, FULL ~82 min)
- [x] _Test_Suite_1.md (times before / after, Core list, findings, fix list, S4a verdict)
- [x] 10-structure.md lines (FixtureTimes.cs, FixtureTimesTests.cs, TestRunRecorderHooks, SearchSceneFixture, CardTestInput)

## Closing
- [x] CORE run timed + green (background)
- [x] FULL EditMode + PlayMode green (background)
- [x] LivingRoomScene open, final report 6.4 + review questions
