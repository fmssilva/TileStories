# Plan tracker -- Premium P2: the Navigation Bar to v5, and the card's top after the hero

Brief: `__AI_worker.md` (Premium P2). Spec: `_3.3` "S4 review, third round" (Navigation Bar, Chrome, Section separation), R11;
`_3.3.1_Prototype/Premium_v5.dc.html` (nav markup, CSS, renderVals). Baseline: HEAD de82b7a (P1 a3a54d6 + rules). Do not commit.
TEST PLAN: BLOCK only -- full EditMode + NavBarRuleTests, NavigationBarEditorTabTests, PoiCardNavBarSceneTests,
PoiCardChromeMarksSceneTests, PoiCardHeroSceneTests + new fixtures. No CORE, no FULL.

## GATE
- [x] branch 0 ahead of origin (nothing to push); SDF fonts unmodified; IPCE/ left alone
- [x] compile clean before any run (Editor.log: 0 error CS, Tundra success, ExitCode 0)
- [x] Unity's place asked (4.2.3 c, "Pronto")

## Decisions
- (developer) the OLD navigation elements (top / bottom / menu / floating / edge elements, presets) are rebuilt later like the bar:
  their fixtures are switched off with ONE shared [Ignore] reason (skipped, not deleted); only the good elements are tested now
- extra S4a knobs removed (corner_radius, side_margin, height, fill_opacity, border, shadow, slide_indicator, enlarge_selected,
  idle_color): the Editor mirrors v5 exactly; JsonUtility ignores the old keys, so old configs load
- NavBarRule.Pick takes an explicit default (Chosen Icon lists Plain / Filled, default Filled)
- 6 default groups: About, Stories, Media (+ AR), Listen, Play (+ Community), Visit
- over the hero a non-solid bar reuses P1's --ts-chrome-glass-hero / --ts-chrome-line-hero (white on dark glass)
- the raised bump = one Painter2D path in generateVisualContent, animated by its x (ease-out-back); Reduce Motion = jump
- "no block text behind the band": ink seen through the band vs surface seen through it < 1.25:1

## Steps
1. [x] Data + rule: CardNavBarSettings in v5 order + defaults, CardOptions lists / labels, NavBarRule readers (Pick with default,
       under_title -> top), 6 default groups, CardNavigationSettings.top = none; both LivingRoom configs
2. [x] NavBarShapeRule (pure path + overshoot) + EditMode tests; NavBarRuleTests updated (defaults, old JSON loads, 6 groups)
3. [x] Old-element fixtures: shared skip reason, [Ignore] on each; full EditMode green
4. [x] NavBarView rebuilt (surface path, disc, pill / underline, chosen icon, labels, colours, over-the-hero, bold); NavSlots /
       USS; hero words lift over a bottom Always bar
5. [x] PART 4 chrome band (PoiCardSheetView, PoiCard.uss, theme token --ts-chrome-band both modes + regenerate), title pill gone
6. [x] Editor component flat in v5 order + Bar Groups table; help texts; NavigationBarEditorTabTests (+ live sync)
7. [x] PlayMode: PoiCardNavBarSceneTests rewritten, PoiCardChromeMarksSceneTests band test; renders -> TestEvidence/Premium/P2/;
       Editor captures (dark skin); look at each
   EditMode (full, Unity behind): 2313 / 2246 passed / 0 failed / 67 skipped (66 TestSkips.OldNavigationElements + the recorder's own)
8. [x] BLOCK runs: full EditMode 2315 / 2248 / 0 failed / 67 skipped; PlayMode NavBar 34/34, NavBar+ChromeMarks+Hero 88 (87 + the one fixed by --ts-navbar-glass 0.98, then NavBar 34/34)
8b. [x] ADDENDUM (developer, 2026-10-28; v6 Main.dc.html): PART 4 replaced -- Top Band Fade (default; sheet colour, ramp 0.99 -> 0 over
       104 pt, no hairline) / Glass (the built band) in card_settings.container.top_band + Card Container row; PART 5 -- the hero's words at
       the foot of the VISIBLE sheet (Half and Full), above a bottom bar; tests: band per value, words inside the visible sheet + 7:1
   Done before the addendum: Editor capture TestEvidence/Premium/P2/editor_navbar_1.png (dark skin; Families popup ~15 pt out of line)
9. [x] docs: _3.3 build log, _Test_Suite_1 skips, 10-structure lines; LivingRoomScene ready; report 6.4
