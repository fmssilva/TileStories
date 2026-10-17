# Plan tracker -- _3.2 step 10: edge_scrubber + hotspot pins that open a block

Re-read this after a compaction. Brief: the pasted step 10 brief (10.1 scrubber, 10.2 pins, 10.3 docs). Tick as you go.

## GATE (done 2026-10-17)
- [x] UnityMCP answered (telemetry_status true)
- [x] `git push`: 1 commit pushed (3de449c..dad3d37)
- [x] Game view + POI Editor captures opened (`TestEvidence/Gate/gate_3_2_step10_*.png`), no window left
- [x] Tree compiled (0 error CS), both SDF fonts clean (git status clean)
- [x] EditMode baseline 1829/1829 (61 s) on clean dad3d37

## Design (decided)
- Edge slot host = a child of `Stack.Scroll.contentViewport` (absolute, top/bottom/right 0): its box IS the viewport, no geometry code, no new owner.
- `EdgeScrubberRule` (pure): Fits / Track / IndexAt. Dots tile at the touch-token pitch (48 px), centred. Not Fits -> rail not drawn. Release > 1 pitch past an end picks nothing.
- `EdgeScrubberNavView`: column of 48 x 48 transparent buttons + dot inside, dark capsule; current = bigger dot + accent ring; bubble beside the dot under the thumb; SwipeGrab (ClaimsVertical) on the column; tap = button click = JumpTo(i, "edge_scrubber").
- `HotspotPinRule.JumpTarget(stored, jumpTargets)` pure. Pin with reachable target jumps via Host.ScrollToBlock + shows the open_block arrow; else opens its popup.
- Editor: `DrawBlockRefRow` gets a "blank is fine" flag for the optional field.
- Fixture content: lamp block_35 spot 0 -> block_2; block_36 spot 1 -> block_33 (both config.json copies).

## Steps
- [x] 1 Pure rules + tests: EdgeScrubberRule, HotspotPinRule (+ Tier 0 tests)
- [x] 2 Config + slot plumbing: CardNavigationSettings.edge, NavSlotKeys.Edge, NavSlotRule.EdgeElement, NavSlots host, registry key, sheet hookup
- [x] 3 EdgeScrubberNavView + tokens + USS
- [x] 4 Hotspot opens_block field + view + Editor drawer flag + fixture content
- [x] 5 Editor: Edge row (+ (i), guides), walker 20, round trip, registry tests
- [x] 6 Phase A: gallery key + CardNavGalleryTests edge slot + element check
- [x] 7 Phase B: PoiCardEdgeSceneTests (+ hotspot pin tests)
- [x] 8 Captures judged (Lamp rest + mid-drag EN/PT, over full-width picture, filmstrip finding, Editor hotspot row, Edge row)
- [x] 9 Docs: _3.2 row 10 + 5.5 + 4.2, _5.1 hotspot field, 10-structure.md, _3.1 hotspot line, __mixed_TODOs if any
- [x] 10 Closing: full EditMode + targeted PlayMode (every Card/Poi fixture + hotspot/BlockRef/HotspotImage greps); final report 60-finishing 6.4

## Findings log
(fill in as found)
