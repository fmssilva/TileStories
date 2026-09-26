# Current plan tracker -- _3.1 POI Card Blocks, steps 0-5

- [x] 0 Baseline: compile clean, EditMode 1094/1094, PlayMode 220/220 (2026-09-26)
- [x] 1 Config model: CardConfigData.cs, WallConfigData.card_settings, POIData.card, WallSession.CardSettings; CardConfigRoundTripTests
- [x] 2 Pure contract: BlockFieldType/Definition, BlockKindDefinition, BlockRegistry, BuiltInBlocks, BlockLibraryRule, BlockStackBuilder, BlockFieldReader, SheetStopRule, CardTapRule + tests
- [x] 3 Card container: IBlockView, PoiCardSheetView, BlockStackView, HeaderBlockView, PoiCardHost, PoiCard.uss, CardTokens.uss, PoiSubtitle; remove DetailCardView + SearchPanels.Card; PoiCard scene object; fixture lamp / lamp_military; migrate old-card tests; Phase A gallery; Phase B PoiCardSceneTests
- [x] 4 Editor: Detail Card tab (Card Container, Block Library), Specific Marker > Card Content, BlockFieldDrawer; DetailCardEditorTabTests; DrawingNeverWrites += DetailCard
- [x] 5 Media seam: IMediaSource, ResourcesMediaSource, ScopedMediaSource, fixture placeholder + README; tests
- [x] Finish: _3.1 status table + body + design history, _3.3 tokens, _2.6 card mentions, _5.1, 10-structure.md, sibling-doc grep
- Final: EditMode 1129/1129, PlayMode 254/254, 0 error CS. Next session: _3.1 step 6 (Tier 1 blocks)
