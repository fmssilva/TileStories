# Plan tracker -- _3.2 step 18: the in-card facets (the last BUILD step)

Brief: Needs flag on the KIND, facet chips over `drawer_tree` and `grid_overlay`, ONE pure rule read by both. NO text field.
Baseline trusted: commit 52fceea ("Add a Card Starter and a real card for The Painting"), full EditMode 1984/1984 (94 s) re-run today.

## GATE (done)
- [x] UnityMCP answered (telemetry_status)
- [x] git push: 1 commit (ae1222f..52fceea)
- [x] Game view + POI Editor captures opened (`TestEvidence/Gate/gate_3_2_step18_{gameview,poieditor}.png`), window closed (0 left)
- [x] tree compiled (0 `error CS`), git status clean under TileStories: neither SDF font modified
- [x] full EditMode 1984/1984 on 52fceea

## Part 1 -- the owed look (done)
- [x] Simple / Rich / AR point rendered on the empty Camera point, EN + PT, Half + Full (throwaway probe, deleted). Judged in the report; sets unchanged.

## Part 2 -- the facets
Decisions: Needs chips are EXCLUSIONS ("No headphones": the visitor's question), Format chip single-select, time chips single-select
(<= 2, <= 5), untimed blocks do not match a time chip, a chip is offered only if it matches >= 1 and excludes >= 1 of the blocks that pass the
OTHER active facets, active chips stay (so they can be cleared) + a Clear chip, counts follow the filter, a jump lands the first PASSING block.
- [ ] 2.1 `CardOptions.Needs*`, `BlockKindDefinition.Needs`, `BlockRegistry.Validate` refuses an unknown need; audio_guide -> headphones,
      place_in_ar + show_on_wall -> camera (no built-in kind streams: network is in the vocabulary, an app kind may use it)
- [ ] 2.2 pure `NavFacetRule` + `NavFacets` (state) + `BlockFacts`; `ClipLengthReader` (the clip-length load NavContents had privately, now shared)
- [ ] 2.3 CardStrings keys (en + pt) + CardStrings.asset rows; `NavFacetBar` (chips, one NavStrip row, 48 px, border + bold when active)
- [ ] 2.4 drawer_tree + grid_overlay read the bar: filtered counts, hidden empty sections / kinds, jump = first passing block; Navigation.uss
- [ ] 2.5 Tier 0: `NavFacetRuleTests` (each facet alone, combined, none of a facet on the card, no reading time, offered rule); `KindNeedsTests`
- [ ] 2.6 Phase A: `CardNavGalleryTests` both menus with facets over the gallery cases
- [ ] 2.7 Phase B: `PoiCardFacetSceneTests` in LivingRoomScene, real taps: drawer narrows to exactly the matching blocks on The Lamp; the grid too; a jump from a filtered list lands the right block
- [ ] 2.8 captures: drawer + grid, unfiltered / filtered, EN + PT, 390 px; item-by-item
- [ ] 2.9 DOCS: `_3.2` row 18 + proof, [step 18] TODO removed; `_3.1` Needs; `_5.1` if a row changed; `10-structure.md`; `40-testing.md` RUN TIMES if moved
- [ ] close: full EditMode; Card / Poi / SizeComparison PlayMode fixtures named one by one; final report (60-finishing 6.4)
