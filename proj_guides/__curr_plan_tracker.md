# Plan tracker -- _3.2 step 13 (accordion and tabs layouts) + PART 1 (fonts, one full-screen control, closed row never seen, contents collapse)

GATE (done): UnityMCP answered (telemetry_status); pushed 1 (9941a73..7862029); Game view + POI Editor (Detail Card > Navigation)
captured and opened (`TestEvidence/Gate/gate_3_2_step13_{gameview,poieditor}.png`, capture window closed, none left); 0 error CS;
SDF fonts clean; EditMode 1889/1889 (85 s, background) on clean 7862029. Rides on step 12's full PlayMode 1031/1032.

Decisions (autonomous): pure CardLayoutRule + sheet-owned CardLayoutView (stack gets generic compact predicate / park / open order);
tabs park (unbind) hidden sections; NavController calls Layout.PrepareJump before its slot check; HasViewed gets contentShown + the
content box; Back collapses the most recently opened compact row (any layout); accordion rows start closed; blocks of no section show
on every tab; tabs in effect only with navigation shown and >= 2 sections; CurrentSection at the end = last laid-out target;
video OffersOwnFullView; nav_contents Rows (sections_first default, LibraryDefault; synthesized = sections_first) via AccordionRule.

## PART 1
- [x] 1.1 Fonts: generate Static atlases (ASCII + PT + every shipped char); `ShippedTextGlyphTests`; proof after a Unity relaunch
- [x] 1.2 `OffersOwnFullView` (video); CompactRowView hides its button; Phase B: one control per open video / text row
- [x] 1.3 `NavScrollRule.HasViewed(contentShown, ...)`; controller measures the open row's view box; Tier 0 + Phase B (closed row never ticks)
- [x] 1.4 `AccordionRule`; nav_contents Rows field + collapsible section titles (CompactRowView); strings en + pt; Tier 0 + Phase B

## PART 2
- [x] 2.0 Data: `layout`, `accordion_one_open`, CardOptions Layouts + labels; round trip
- [x] 2.1 `CardLayoutRule` (Effective, ShowsCompact, JumpPlan, SpySection, TopElement fallback, RowsToClose) + Tier 0
- [x] 2.2 Stack: compact predicate from the rule, Park/Unpark (ShowOnly), open order, collapse keeps the line in view
- [x] 2.3 `CardLayoutView` + NavController.PrepareJump + spy; NavBackRule.CollapseRow; NavSlotRule tabs fallback; CurrentSection end fix
- [x] 2.4 Editor: Layout row + tabs warning, Open One At A Time; (i), guides; Editor test (real click, Ctrl+Z, walker)
- [x] 2.5 Phase B `PoiCardLayoutSceneTests` (accordion / tabs, owners); Phase A gallery cases
- [x] 2.6 Captures: Lamp accordion + tabs 390 EN + PT, nav_contents folded + opened, Editor rows 260 / 261

## Close
- [x] Full EditMode 1903/1903; Card / Poi 814/815 -> fixed, 59/59; FULL PlayMode 1051/1051 (focused); struggling test passed, closed
- [x] Docs: _3.2 row 13 + TODOs; _3.1; _5.1; 10-structure; RUN TIMES; __mixed_TODOs font + layout notes closed
