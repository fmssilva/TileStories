# Plan tracker -- Domain Review _3.2 (POI Card Navigation), mode FIX, chunks A + B (2026-10-21)

Brief: Domain_Review FIX, audit `_3.2.1_Audit.md` section 11 chunks A and B ONLY (tests, no feature, no new owner). Trusted commit for the
baseline: 976e16a (clean Unity tree; audit: EditMode 1999/1999, PlayMode 1148/1148).

## GATE
- [x] UnityMCP answered (telemetry_status); Editor was left in Play Mode -> stopped, then runs accepted
- [x] .clinerules 00-60 read (10-structure.md searched, not read whole: 329 KB), Domain_Review.md, audit read whole
- [x] `git push` (976e16a pushed, branch level with origin)
- [x] SDF fonts: no Unity file modified (git status shows only IPCE / proj_guides docs of other work)
- [x] captures opened: TestEvidence/Gate/gate_3_2_fix_ab_gameview.png, gate_3_2_fix_ab_poi_editor.png (window closed, none left)
- [x] full EditMode baseline: 1999/1999, 181 s (job cd2a0a87)

## Chunk B (EditMode, light) -- tests in the existing rule fixtures
- [x] NavFacetRuleTests: 11 (block of exactly 5 min decides the 5 chip), 13 (No headphones on: camera chip judged inside the no-headphone pool),
      19 (time chip on: the other time chip and the format chips judged correctly)
- [x] CardChromeRuleTests: 23 (top brings the area back after a small move up), 25 (exactly 24 does not hide), 27 (exactly -8 does not show)
- [x] NavScrollRuleTests: 83 (a 0 px viewport never counts as seen, on a block straddling the offset)
- [x] BentoLayoutRuleTests: 141 (a pair that exactly fills the width), 144 (minimums exactly equal the room)
- [x] mutation proof each (break -> named test red -> restore), mutation list in the report

## Chunk A (PlayMode, real scene) -- new fixture PoiCardNavOptionsSceneTests (names hold Poi + Card)
- [x] Follow Scroll off / on: real drag; mark stays on the jumped section (off), follows (on); a jump still moves it (mutation 200)
- [x] Show Counts on / off on every element that offers it (registry ShowsCounts): count label = model count, hidden when off (210-212)
- [x] Labels icon / text / icon_and_text on every element that reads it (registry ReadsLabels): icon / words shown or hidden (213-214)
- [x] mutation proof each; captures of the three looks at 390 px judged item by item
- [x] extras if they fit: families (In Navigation off), group_by = format, custom groups on Segmented

## Closing
- [ ] full EditMode at the end; PlayMode fixtures whose name holds Card / Poi / SizeComparison (targeted run)
- [ ] docs: _3.2.1_Audit.md (chunks A, B closed + matrix cells), _3.2 status row, 10-structure.md (new test file), RUN TIMES if moved
- [ ] LivingRoomScene left open and ready (not a test scene)
- [ ] final report per 60-finishing 6.4

## Progress note (2026-10-21)
- full EditMode after chunk B + A: 2007/2007 (106 s). Fixture PoiCardNavOptionsSceneTests 27/27 (111 s). Mutations: all 9 chunk B + 200, 210-214 + 4 extras red, restored with git checkout.
- running now: the 44 Card / Poi / SizeComparison PlayMode fixtures (job 98e240cd...); docs half done (10-structure line, audit matrix cells); still to write: audit section 11 status, _3.2 status row, RUN TIMES.
- finding to report: Segmented + Show Counts: "Discover" breaks mid-word at 390 px (NavOptions_segmented_counts_on_390.png).
