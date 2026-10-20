# Plan tracker -- _3.2 step 14 (pages layout + family_pager) + PART 1 (one "read the card" rule, story_chapters buttons)

GATE (done): UnityMCP answered (telemetry_status); pushed 1 (7862029..6db17e5); Game view + POI Editor (Detail Card > Navigation)
captured and opened (`TestEvidence/Gate/gate_3_2_step14_{gameview,poieditor}.png`, capture window closed, none left); 0 error CS,
ExitCode 0; neither SDF font modified (no finding); EditMode 1903/1903 (106 s, background) on clean 6db17e5 (trusted).
Rides on step 13's full PlayMode 1051/1051. This block OWES the full PlayMode run (pages changes the stack's container).

Decisions (autonomous):
- pages = tabs' mechanism (ONE vertical ScrollView, one section bound, the others parked = unbound) + a horizontal pager over it:
  `CardPagerView` (sheet-owned, beside CardLayoutView) owns a SwipeGrab on the stack's contentViewport, the next-page PEEK (a cue panel
  in the viewport: family icon + words, `--ts-page-peek` wide at rest) and the slide; a turn = `NavController.JumpTo(i, "pages")`
- pure: `SwipePageRule.Landing` (which page a released swipe lands on), `SwipeGrabRule.LeavesToPart` (a drag that starts on a part
  that drags by itself -- a sideways ScrollView, another SwipeGrab area, a non-Button that captured at touch-down -- stays with it),
  `CardLayoutRule` pages (Effective needs >= 2 sections like tabs; parks; spy = shown; TopElement = family_pager)
- `family_pager`: a registered TOP element (chevrons + the section's words + "n of m"), usable in any layout; Pages forces it into the
  top slot; the Editor hides Top Bar under Pages (keeps its value)
- 1.1: `ContentSeenRule.HasReadCard(shownPartAtItsEnd, sections)`: the shown part at its end AND every section set aside read to its
  end = its last readable target viewed in this binding (NavController's step-13 `_viewed`, now kept always) -- scroll / accordion
  unchanged (every section shown: the stack's end reads them all); no new memory
- 1.2: measure the Lamp's chapters' Next button y; decide (fix = chapters side by side in a clipped row, height = tallest; no measuring code)

## PART 1
- [x] 1.1 `ContentSeenRule.HasReadCard` + `ReadToItsEnd` (Tier 0); NavController keeps `_viewed` always + `HasViewed(key)`;
      stack asks the layout before revealing; Phase B: scroll / tabs / pages -- one short section no, all of them yes
- [x] 1.2 story_chapters: measure Next's y per chapter on The Lamp at 390; decide; fix or write the reason in _3.1 / _3.2

## PART 2
- [x] 2.0 De-risk FIRST: a SwipeGrab on contentViewport takes a sideways drag before the ScrollView; vertical stays the ScrollView's
- [x] 2.1 Data / rules: `pages` in CardOptions (+ label), CardLayoutRule pages, SwipePageRule.Landing, SwipeGrabRule.LeavesToPart;
      SwipeGrab yields to a part that drags itself; Tier 0
- [x] 2.2 `CardPagerView` (peek, slide, turn, reduce motion) + CardLayoutView pages (park = release, spy, PrepareJump) + tokens / USS
- [x] 2.3 `FamilyPagerNavView` (+ registration, words en + pt) ; NavSlotRule via CardLayoutRule.TopElement
- [x] 2.4 Editor: Layout lists Pages, (i) caution in a wall author's words, Top Bar hidden under Pages, guides; Editor test
- [x] 2.5 Phase B `PoiCardPagesSceneTests` (3 drag angles, threshold / ends, chevrons, spy, bar jump, media released, nested drags,
      Military scrolls) ; Phase A gallery (pages layout + family_pager over the five cases)
- [x] 2.6 Captures: The Lamp in pages 390 EN + PT (rest + peek, mid-swipe, pager header), Editor Layout = Pages

## Close
- [x] Full EditMode; Card / Poi targeted set green; FULL PlayMode in the background (editor_is_focused first poll + end)
- [x] Docs: _3.2 row 14 + both TODOs; _3.1 (ContentSeenRule, story_chapters); _5.1; 10-structure; RUN TIMES

Notes (done so far): 1.1 Phase B PoiCardShowAfterReadingSceneTests 3/3; 1.2 measured Lamp 106.0 x3 EN/PT (no move), gallery long case: the same
finger MISSED Next on chapter 2 -> fixed (chapters side by side), now 195.0 x4; pages Phase B 9/9 (found: ScrollView owns a touch that stops
its fling -- a swipe on a coasting page only stops it; ScrollView content translate is the scroll's own -> the whole scroll slides).
Targeted Card / Poi (37 fixtures, 839) 836/839 -> the 3 (story fit check met hidden chapters; a flaky coasting precondition) fixed and green;
FULL PlayMode started 2026-10-19 (1076 tests), editor_is_focused false at the first poll.
FULL PlayMode 1075/1076 (2850 s, background start and end); the 1 = PoiCardNavSceneTests final-screen precondition, green alone 18/18 -> __mixed_TODOs. EditMode 1909/1909.
