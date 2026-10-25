# Plan tracker -- _3.3 P4.4c1b: the interleaved-family spy rule + the opener in pages / tabs

Base: HEAD 45e11b2 (H1: full PlayMode 1307, full EditMode 2098, both background, 0 failed). Do not commit (the Architect does).

## GATE
- [x] git push: branch 0 commits ahead of origin -> nothing to push
- [x] captures: TestEvidence/Gate/gate_4c1b_game.png, gate_4c1b_poi_editor.png (first editor capture showed another app: deleted, re-taken narrower), window closed, 0 left
- [x] compile: refresh_unity force + compile -> ExitCode 0, Tundra build success, 0 error CS
- [x] fonts: Dev/StyleSpike/Fonts/Fraunces/Fraunces-SemiBold TMP.asset rewritten by a run -> git checkout --
- [x] baseline: none (clean tree on 45e11b2, H1 green)

## The rule (decided in the plan)
The bar marks the section of the RUN being read: a run = consecutive targets (Model.Targets order) of one section. The spy line
runs over each RUN's first target (not each section's first target); the run found is mapped to its section. By-family order:
runs == sections one to one (Runs[i] = (i, Sections[i].Targets[0])), so the arithmetic is the same list -> the same marks.
CurrentBlock unchanged. A jump to a section = its first target = its FIRST run (unchanged code, now tested on the interleaved card).

## Steps
1. [ ] Pure: CardNavModel.Runs (Section, FirstTarget), built in Build; NavScrollRule.SectionAt(runTops, runSections, ...).
       EditMode: NavScrollRuleTests (+interleaved fixture: every boundary, a short run at the top, the end, by-family identity sweep,
       an unlaid run); CardNavModelTests (+The Lamp default runs: Visit x2, Media x2, every target once, no two neighbours alike;
       The Painting default: About comes back; by_family: runs == sections for both). Mutation proof (old mapping) -> red, restored.
2. [ ] NavController.SectionAtOffset reads Model.Runs + NavScrollRule.SectionAt. CardTestInput.SectionUnderTheSpyLine(scroll, model,
       slotOf) -- runs from model.Targets + SectionOf, read from the screen; the 8 private copies call it.
       PlayMode new fixture PoiCardInterleavedSpySceneTests: real drags over The Lamp (Visit -> Media -> Visit, Media again, the end):
       the tabs and every marker bound mark the run's section; a real tab tap lands on the FIRST sheet of the family; by_family
       Lamp + Painting: the same marks at the same offsets as the old rule (sweep); end of card = the last run's section.
3. [ ] Part 2: CardLayoutRule.OpenerShowsWords(effective) (tabs: no); CardLayoutView.Bind tells SectionSheets; SectionOpenerView
       hides an empty name; FamilyPagerNavView hides its name (icon + words) when the card is in pages, keeps "1 of N".
       EditMode CardLayoutRuleTests +1. PlayMode: SectionSheetSceneTests pages / tabs test re-written to the decision;
       PoiCardPagesSceneTests' pager-words asserts -> the counter + the opener names the page.
4. [ ] Targeted PlayMode fixtures (grep: NavController, NavScrollRule, CardNavModel, SectionSheets, SectionOpenerView,
       FamilyPagerNavView, CardLayoutView, UnderTheSpyLine users) green; full EditMode green.
5. [ ] Visuals: The Lamp interleaved light + dark across Visit -> Media -> Visit (bar mark per step), The Painting by_family,
       pages, tabs. Opened and judged with a checklist.
6. [ ] Full PlayMode (background) + full EditMode (background), 0 failed. RUN TIMES updated.
7. [ ] Docs: _3.3 Build log + the P1b nav TODO closed; _3.2 4.4 (the spy's new meaning); 10-structure entries; fonts checked again.
8. [ ] LivingRoomScene left open; final report (60-finishing 6.4) + review questions.

## Log
- [x] 1 pure: Runs + SectionAt; EditMode 49/49; mutation (old mapping) -> 3 red with the right messages, restored, the type reflected
- [x] 2 controller + SectionUnderTheSpyLine (8 copies replaced) + PoiCardInterleavedSpySceneTests; two harness fixes (the finger placed by the
      first viewport landed on the bottom bar; "offset = Range" is not where drags rest once the bottom area comes back: AtTheEnd)
- [x] 3 Part 2: OpenerShowsWords / PagerNamesTheSection; EditMode 64/64; SectionSheet + Pages + Interleaved 30 -> 29 (the drag end) -> fixed
- [x] 4 targeted 27 fixtures 391: 388 + 3 red -> PoiCardNavSceneTests x2 asserted "the LAST section" (now the last run's: In AR, not Listen
      -- the rule's intended change, re-stated); BottomElements' "Layout update is struggling" = the OPEN __mixed_TODOs entry, NOT reproduced
      (its whole fixture 0 lines, with NavScene + Interleaved: 33/33, behind)
- [x] 5 visuals: TestEvidence/Sections/spy/*.png (lamp light/dark x 4 steps, painting by family x 2, pages x 2, tabs x 2) judged
- [x] full EditMode 2108: 2107 passed, 0 failed, 1 skipped (recorder gate), 144 s, behind both ends
- [x] 6 full PlayMode 1314: 1271 passed, 0 failed, 43 gated skips, 4711 s, behind start to end; 5 "struggling" lines = the guard's own
      self-test (LayoutStruggleGuardTests), 0 real; RUN TIMES + __mixed_TODOs + _3.3 Build log updated
- [x] 7 docs: _3.3 (R10.7 built note, P1b TODO closed, Build log), _3.2 4.4 + the Family Pager, 10-structure (14 entries + 1)
- [x] 8 LivingRoomScene active (Assets/Apps/LivingRoom/LivingRoomScene.unity, not dirty); final report
