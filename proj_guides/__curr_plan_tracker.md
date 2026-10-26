# Plan tracker -- _3.3 S2c: the Page Chrome and edge-to-edge block pages

Brief: `_3.3` R16 row 5c, S2b review decisions 1 + 5, R17.5. Base: HEAD 8ccec31 (S2b closed: full PlayMode 1404, full EditMode 2212).
Worker re-reads this file after a compaction. Tick `[x]` as steps close.

## GATE
- [x] 0 commits ahead of origin (nothing to push)
- [x] Game view + POI Editor captures taken and opened (TestEvidence/Card/PageChrome/gate_game.png, gate_editor.png)
- [x] Tree compiles (Editor.log: last compile ExitCode 0, no error CS after it)
- [x] Oswald Bold SDF / Roboto-Bold SDF unmodified (git status clean for them)
- [ ] Unity place for runs: 4.2.3 action c, asked before each background run

## Decisions (autonomous)
- Components `PageTopBar` + `PageFootView` (Runtime/UI/Components/, VisualElements, host-agnostic); pure `PageChromeRule` (wall default + block override) and `PageCrumbFitRule` (which part is ellipsised first) in Runtime/Blocks/Elements/.
- TakeoverView keeps its public surface (Back, Crumb*, CrumbText, ShowSection, SectionTapped, chips); it builds the PageTopBar. Back is a 44 pt round chevron; its tooltip = accessible name (CardStrings `page_back`, replaces `takeover_back`).
- Foot lives at the END of the page's scroll (as the prototype), built by BlockFullView.PageFor; Previous / Next call takeover.Turn(+-1). Sibling chips + swipe stay as they are (question for the developer).
- Config: `card_settings.components.page_chrome_breadcrumb` / `page_chrome_prev_next` (bool, default true). Block override = BlockSurfaces component `page_chrome` (Choice shown / hidden per part; composing kinds only: the only kinds that carry the Component Overrides field today).
- Edge to edge: takeover root padding 0; the page keeps the 16 pt gutter through `.card-takeover__page`; the block-page Scroll bleeds out by `--ts-page-bleed` and its content container takes the gutter back, so a full-bleed element reaches the phone's edges while text keeps 16 pt.
- The Painting's story video row: tap `play_in_place` (config.json + StreamingAssets copy, one token).

## Steps
- [x] 1. Data + pure rules: CardComponentSettings fields, ElementOptions.ComponentPageChrome, BlockSurfaces fields + ComponentOverride readers, PageChromeRule, PageCrumbFitRule; EditMode tests (rule edges, round trip, old config)
- [x] 2. Components: PageTopBar, PageFootView, USS tokens/classes, CardStrings (page_back, page_previous, page_next EN/PT)
- [x] 3. Wire: TakeoverView on PageTopBar, BlockFullView foot + per-block chrome, CardFullViewOpener passes the wall default; edge-to-edge USS; fix tests that read the old chrome
- [x] 4. Editor: Detail Card > Card Components section (Page Chrome row, (i), Test guide), live sync, DrawingNeverWrites, real-click Editor test, undo
- [x] 5. Painting video tap = Play In Place (config copies) + test
- [x] 6. PlayMode: Phase A gallery (long / short title, EN + PT, light + dark, 390 px, contrast), Phase B The Painting (real Back tap keeps scroll, real Previous / Next taps, full-bleed pixels at the phone's edges, gallery page)
- [x] 7a. Targeted set (793) green apart from explained environment items (saved visitor mode, leaked sensor count, bento flake, reveal re-based)
- [ ] 7b. FULL PlayMode (background, started 266098535 log offset), then reload the domain, then full EditMode
- [ ] 8. Final captures judged (after the foot alignment + Back ring tweaks); docs done (_3.3 Build log numbers to fill, _5.1, _3.1, _3.2, 10-structure); LivingRoom scene ready; final report (60-finishing 6.4)
