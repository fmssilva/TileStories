# Plan tracker -- _3.3 S2a: the text components, the story kind and the Open Row

Brief: R16 row 5. Baseline: none (HEAD 2ddd76f; S1 full EditMode 2192 + 277 targeted PlayMode green).

## GATE
- [x] push: 0 ahead, nothing to push
- [x] captures: TestEvidence/Gate/gate_game_view-1.png + gate_poi_editor.png (opened, correct, no window left)
- [x] compile: forced, ExitCode 0, 0 error CS after the offset
- [x] SDF fonts unmodified

## Steps
1. [x] PART 1 -- POICardData.last_block_number + TaxonomyRowKeys.TakeBlockKey / SeedBlockKeyMemory (LoadConfig); AddCardBlock + CardStarters; 3 EditMode tests + 2 mutants killed; both configs migrated through the Editor's save
2. [x] Theme -- .ts-el--display, --ts-accent-shine (light 8.32 on surface / 7.3 on surface-2; dark ~14), TextRoles audit; USS regenerated
3. [x] Rules -- DropCapRule (4 tests, mutant killed); ElementPreviewRule TitleLead (eyebrow joins its title; a surviving mutant found a missing case, added, killed); ElementRules.FirstWords = the title row first (S1 tests re-based)
4. [x] Components layer -- IElementComponent, ElementBindContext, ElementComponentPool, ElementStyle, Components.uss; ElementRegistry factories; CardTextView Leading + DropCapFlow; CardIcons Azulejo / QuoteOpen
5. [x] The 10 components (7 classes)
6. [x] OpenRowView + OpenRowMotionRule; CardStrings kind_story / open_row_story / open_row_meta; card_settings.components.open_row_emphasis (default nudge)
7. [x] ComposedBlockView + story kind; IBlockHost.FullHeight; BlockBindContext.DevGallery; Components.uss in the three scenes' blockStyles
8. [x] Phase A -- CardGalleryDefinitions.Story.cs + CardStoryGalleryTests 35/35 (background); captures judged
9. [x] PART 5 -- The Painting's story block_19 via LoadConfig / AddCardBlock / SaveConfig / Copy (EN + PT from the shipped rich text)
10. [x] Phase B -- PoiCardStorySceneTests 6/6 (background)
11. [x] Full EditMode (first pass 2201: 6 re-bases fixed) -> targeted Card|Poi|Theme|Section 1120 (0 failed) -> FULL PlayMode 1372 (1 red, not reproduced: InterleavedSpy) -> full EditMode 2202 (1 red, not reproduced: ConfigUndoGrouping slider)
12. [x] Docs: 10-structure.md, 40-testing RUN TIMES counts, _3.1 5.2 / 5.5, _3.3 Build log + R16 row; final report

## Notes / re-bases
- line-height tag = % of the FACE's line (not CSS): tokens converted (paragraph 123 = CSS 1.6)
- the page clipped at its left by a -32 bleed: the fit test measured the 390 frame, not the visible page -> new assertion, proven red with the old bleed
- EditMode re-bases: BlockKindRoundTripTests (Elements filler), ElementRegistryTests (the story composes), DetailCardEditorTabTests (Elements left to S3 by name; components edit; count 44), LiveSyncFieldMatrixTests (components row), LivingRoomSceneWiringTests (Components/ folder; app sheet last)
