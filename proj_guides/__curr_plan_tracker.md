# Plan tracker -- _3.2 step 11a (navigation family, nav_icon_grid, Auto Nav Block)

GATE (done): pushed 1 (dad3d37..04f92db); Game view + POI Editor captured (`TestEvidence/Gate/gate_3_2_step11a_gameview.png`,
`TestEvidence/Editor/230_CardNavigation_Edge_620.png`); compile ExitCode 0; SDF fonts clean; EditMode 1839/1839 (63 s) on clean 04f92db.
PART 1 (done): Edge row judged at 620 pt -- no finding.

Decisions: BlockBindContext.Nav + NavController.Bound (the grid draws on the model the sheet binds AFTER the stack); inserted key
`auto:nav_icon_grid` (authored keys are generated block_N, never a ':'); threshold counts targets with nothing revealed (the stack never
changes on a reveal); "authors its own" = a navigation-family entry in the stack; kind off in the Block Library / none / unknown /
non-navigation key -> nothing; family icon `grid` + `--ts-family-navigation` + `family_navigation` words (the Editor names it);
nav_icon_grid = format interact, look `tiles`, NavTab tiles, styles in Navigation.uss, hidden below 2 sections (rule 2.5.7);
Editor options listed from the registry (navigation-family kinds).

- [x] S1 family (85, InNav off) + grid icon + token + CardStrings rows + BuiltInBlocks.Navigation (nav_icon_grid) + NavIconGridBlockView
      + BlockBindContext.Nav + NavController.Bound; FamilyRegistryTests updated; compile + EditMode
- [x] S2 CardNavigationSettings.auto_nav_block + BlockStackBuilder.InsertAutoNavBlock; AutoNavBlockTests (Tier 0, every branch)
- [x] S3 Editor row Auto Nav Block (after Edge) + (i) + guides; DetailCardEditorTabTests (real click, Ctrl+Z, walker), Block Library +
      Add block; round trip / live sync rows
- [x] S4 Phase A: nav_icon_grid in CardNavGalleryDefinitions x 5 cases + CardNavGalleryTests checks + renders judged
- [x] S5 Phase B: PoiCardNavBlockSceneTests (Lamp yes / Military no, real tile taps land +-1 px, spy + scroll unaffected, stops)
- [x] S6 captures judged (Lamp EN + PT DONE: Search_NavIconGrid_Lamp_full_{en,pt}.png; Editor rows pending: Unity was covered, the first try caught another app and was deleted): Lamp grid 390 EN + PT, Block Library row, Auto Nav Block row
- [x] S7 docs: _3.2 status + TODO, _3.1 kinds/families, _5.1, 10-structure, RUN TIMES
- [x] S8 full EditMode; Card / Poi targeted set green; FULL PlayMode in the background (editor_is_focused first / last poll)

CLOSED: EditMode 1856/1856 (66 s); Card / Poi 32 fixtures 742/742 (2454 s); FULL PlayMode 978/978 (2892 s, unfocused at first poll, focused at the end).
