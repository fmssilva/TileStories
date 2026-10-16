# Current plan tracker -- _3.2 step 7a (the bottom slot and the bottom bar)

GATE: [x] pushed 1 commit (038a500..9718433) [x] captures TestEvidence/Gate/gate_3_2_step7a_{game,poi_editor}.png opened
[x] compiled (Editor.log last ExitCode 0) [x] SDF fonts clean [x] EditMode baseline 1755/1755 (96 s, focused) on 9718433

## Part 1
- [x] progress_map (i): proportions only while the families fit (about 4-5 on a phone); below that each piece is one tap target wide
- [x] Half-is-mostly-header TODO: left logged (header owner)

## 7a
- [x] 0. Measure The Lamp's stops BEFORE the change (probe)
- [x] 1. Bottom area: `poi-card__bottom` container (footer + bottom slot) in the sheet; hidden class on the area (PoiCard.uss)
- [x] 2. CardChromeRule: HasFooter/FooterHeight + HasBottomNav/BottomNavHeight; Chrome.BottomNav; pair floor; NavShownAt
       CardChromeView measures both, toggles area + nav part; CardChromeRuleTests updated + pair tests
- [x] 3. NavSlotKeys.Bottom built; NavSlotRule.BottomElement; NavSlots Bottom/BottomView/BottomKey; config `bottom` = none
- [x] 4. BottomBarNavView (NavTab items, More NavTab + in-flow list), `more` icon, `nav_more` string, ReadsBarSlots flag
- [x] 5. Editor: Bottom Bar row, Bar Slots row (hidden unless ReadsBarSlots), Labels / Show Counts for top OR bottom, help + guides
- [x] 6. Tests Tier 0: BottomBar More lists = overflow for 3/4/5 + fewer sections; registry; round trip; Editor rows (real clicks, Ctrl+Z); walker
- [x] 7. Phase A: gallery bottom_bar x cases (definitions + check) + renders
- [x] 8. Phase B PoiCardBottomBarSceneTests: stops set/unset, tap every item, More lists overflow, spy after drag, drag hides pair, peek
- [x] 9. Existing tests adapted (PoiCardChromeSceneTests area, NavElementRegistryTests unbuilt slot)
- [x] 10. Captures: Lamp Full with bar, More open, Half; Phase A renders; Editor section with Bar Slots
- [x] 11. Full EditMode; Card/Poi targeted PlayMode green; THEN full PlayMode (background)
- [x] 12. Docs: _3.2 status 7a + TODO removed; _5.1 Bar Slots; 10-structure; RUN TIMES
