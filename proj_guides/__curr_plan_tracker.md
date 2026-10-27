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

## NEXT (exact next sub-step for the following session)
- Run the remaining 48 red fixtures of pm_before.csv in groups of ~8 (background), read every message, apply the five patterns,
  log each re-base here; hero-contrast asserts -> [Ignore("Hero contrast below 7:1 -- _3.3 open, fixed in the premium design
  block")] on exactly those tests; a real product bug -> fix if small and clear, else list it with evidence.
- Biggest red fixtures by count: HeroScene 16, PoiCardSceneTests 8, RestingEnd 5 (+5 skipped), TopElements 5, BottomElements 4,
  NavOptions 4, Menu 4, Pages 4, HeroMeasure 4, CardNavGallery 4. The 9 "doubled DetailCard navigation" reds did NOT show in the
  baseline (full EditMode DetailCardEditorTabTests 75/75): watch for them in the FULL runs.
- Then Parts 2 (Core), 3 (slim: CardGalleryTests 907 s, CardNavGalleryTests 564 s, RestingEnd 256 s, PoiCardSceneTests 249 s), 5, the
  closing CORE + FULL runs.

## P4 -- reds and flakes
- [ ] leaked window doubling the 9 DetailCard navigation tests: find the opener, fix at source
- [ ] tests reading the developer's LivingRoom choices (LiveSync badge x5, LivePlayModeNavigationPreset, others the runs show): own data
- [ ] watched flakes: cause or named quarantine

## P2 -- CORE
- [ ] [Category("Core")] on the cheapest test per critical path (brief's list)
- [ ] time CORE (category_names Core), target 25-30 min

## P3 -- BLOCK
- [ ] fixtures > ~3 min: split / slim (largest savings that fit), every removed case's proof named
- [ ] rest -> fix list in block-sized chunks with expected saving

## P5 -- docs
- [ ] 40-testing.md 4.2 "Which tests, when (three tiers)"
- [ ] _Test_Suite_1.md (times before / after, Core list, findings, fix list, S4a verdict)
- [ ] 10-structure.md lines (FixtureTimes.cs, FixtureTimesTests.cs, changed lines)

## Closing
- [ ] CORE run timed + green (background)
- [ ] FULL EditMode + PlayMode green (background)
- [ ] LivingRoomScene open, final report 6.4 + review questions
