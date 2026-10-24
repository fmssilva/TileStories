# Plan tracker -- _3.3 P4.4c1: section glass sheets and section openers (static)

Brief: proj_guides/__AI_worker.md. Baseline: HEAD 5e2b2dc (P4.4b closed green: full PlayMode 1288 / 0 failed, EditMode 2078/2079 flake).
GATE (done): 0 commits ahead (no push); Game view + POI Editor captures opened (TestEvidence/Gate/); compile = DLLs newer than every .cs,
last build ExitCode 0; fonts untouched (git status: only __AI_worker.md).

## Design (decided)
- WRAPPER: contentContainer > [hero, sheet(opener, slots...), sheet...]. A sheet = consecutive scroll blocks of one FAMILY (R10.7), bare
  (no tint / opener / overlap) for a family with OpensSection = false (navigation).
- ONE source for positions: BlockStackView.TopInScroll(element) (sum of layout.y up to the contentContainer, transform-free),
  LandingTopOf(blockKey) (the sheet's top for a sheet's first shown block, else the slot's), TryGetReadablePart in scroll space,
  IsInScroll(blockKey). NavController Landing / TargetAtOffset / SectionAtOffset / ScrollingSlot read them. Tests re-base to them.
- Widths unchanged: sheet padding-x 16 = the old slot margin; slot side margins 0 inside a sheet (block 358 at 390).
- Opener shown unless the previous VISIBLE sheet has the same family (interleaved + tabs / pages); a sheet with no visible slot hides.
- Colours inline through NavTint (+ Ink paint): --ts-family-<k>-glass (sheet), --ts-family-<k> (pill, line), --ts-family-<k>-ink (words).

## Steps
- [x] S0 measure first: SectionSheetMeasureSceneTests (gated, LookMeasure.Sections), "before" numbers + renders (6/6, background)
- [x] S1 data: theme colours (family glass x10, glass edge / hi / shadow, radius-xl), display selector, regenerated USS (additions only);
      FamilyDefinition.OpensSection (navigation false); CardStrings section_opener_block / _blocks; FamilyIconAndWordsTests +1
- [x] S2 SectionSheetRule (pure) + SectionSheetRuleTests (EditMode, 5/5 by name)
- [x] S3 SectionOpenerView + SectionSheets + BlockStackView (Sheets, IsInScroll, TopInScroll, LandingTop, LandingElement, LandingTopOf,
      TryGetReadablePart / CollapseRow in scroll space) + NavController (Landing, TargetAtOffset, SectionAtOffset, ScrollingSlot) + NavTint.Ink
      + CardBentoView.PutHostBeforeTheBlocks + PoiCard.uss / CardTokens.uss. Found + fixed: a border ON the sheet cost the blocks 2 px
      (358 -> 356, The Lamp's vertical timeline +22 px): the edge is an absolute part now
- [x] S4 re-based: 25 runtime test files + BentoLayoutSheetTests + CardTestInput.UnderTheSpyLine (ChapterStart) + CardGalleryChecks / PoiCardSceneTests (IsInScroll)
      + FullViewSiblings / NavContents; targeted 1188: 14 red -> every one by cause (incl. a REAL bug: the last sheet's shadow grew the scroll range 16 px) -> re-run 660/660 green
- [x] S5 SectionSheetSceneTests 13/13 + ThemeFrameSceneTests edge vs its own sheet (5.81 / 3.27) + opener words render (8.85 / 9.27); captures judged
- [x] S6 full PlayMode 1307: 1270 passed, 0 failed, 37 gated skipped (3995 s, mostly focused); full EditMode 2085: 2084 + the KeywordListFieldTypingTests flake (1/1 alone); Build log, 10-structure.md

## Re-base log (old -> new, from source)
- every "its top meets the viewport's top" landing check (17 fixtures, the copied AssertLanded shape): slot.layout.y / slot.worldBound
  -> Stack.LandingTop(slot) / Stack.LandingElement(slot) (a section's first block lands on its SHEET's top: R10.7, intended)
- a block's place in the scroll (readable part, order, offsets: CardNavGalleryTests, NavContents, NavOptions, NavRails, LiveUpdate)
  -> Stack.TopInScroll(slot); "parent == contentContainer" -> Stack.IsInScroll (CardGalleryChecks, PoiCardNavSceneTests)
- order of the scroll's children (IndexOf: NavBlock, NavContents, NavRails, CardNavGallery x3, BentoLayoutSheetTests) -> the slot's
  SHEET's index (Stack.Sheets.SheetOf(slot).Root); ThemeFrame / Theme scene: framed slots found by Query (descendants), not Children()

## Measures (before -> after; TestEvidence/Sections/{before,after}/measures.txt)
- stops 57 / 337.6 / 796 both cards: unchanged. Rails (strip x 17 w 356, cards 150 / 166, 2 whole in view): identical. Bento tiles: identical.
- landings: a section's first block lands on its sheet's top = the previous shift + 8 (24 foot pad - 16 overlap); the section's other
  blocks + 69 more (16 top pad + 41 opener + 12). The Lamp's first block 317 -> 317, its last 16822 -> 17823 (+1001, 16 sheets);
  The Painting 317 -> 317 ... 6336 -> 6867 (+531, 8 sheets). Content: Lamp 17980 -> 19151, Painting 7063 -> 7695.
