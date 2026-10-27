# Plan tracker -- _3.3 S3a: the story's Editor (the Page's element table and the Preview) + the S2c review fixes

Brief: `_3.3` R16 row 6 + "S2c review" row, R17.1/.3/.4/.5/.7, R14.5/.8. Base: HEAD b8ce380 (S2c closed: full EditMode 2234, full PlayMode 1429).
Worker re-reads this file after a compaction. Tick `[x]` as steps close.

## GATE
- [x] 0 commits ahead of origin (nothing to push)
- [x] Game view + POI Editor captures taken and opened (TestEvidence/Editor/S3a/gate_game.png, gate_editor.png); capture window closed, none left
- [x] Tree compiles (forced compile: ExitCode 0, Tundra build success, no error CS after it)
- [x] Oswald Bold SDF / Roboto-Bold SDF unmodified
- [x] Unity's place asked before each run / capture (4.2.3 a / b / c, word for word)
- Note: the Editor runs the DARK (Pro) skin, not the light one the brief names: captures show the skin as set; flagged in the report

## Decisions (autonomous)
- Where: the Elements field gets its drawer in the generic block-field drawer (any composing kind), new partials in `Editor/POIEditor/SpecificMarker/`
  (ComposedBlock, ElementTable, ElementRowEditor, ElementPreview, ComposedBlockHelp); pure edit operations `ElementEdits` (Editor assembly).
- How: an edit is decided while drawing and applied AFTER the row loop (inside the tab's DrawConfigMutationScope: undo / unsaved / live).
- Controls: segmented toolbars for every closed list (real-clickable); "+ Add element" = an inline palette grouped Text / Media / Structure.
- Marks: built-in icons TestPassed / TestFailed (ASCII labels only); a category colour chip + 2-letter tag per element; a thumbnail for picture / video rows.
- Custom preview: switching to Custom (or "Use in the preview" while Automatic) seeds links to the current automatic picks.
- Overrides (Open Row, Page Chrome): the _2.0 pattern -- on seeds from the wall's values, off keeps the block's.
- Card Components gains the Open Row row (emphasis), first (R17.6).
- Test hygiene: `DeviceStateStore.Current` (PlayerPrefs default) read by the card, recent searches, last view, the Editor's reset; every test
  assembly's SetUpFixture swaps a MemoryCardStateStore for the run; a source scan guards it; the developer's saved mode read before / after the full run.
- Breadcrumb: the page's name is dropped when the page shows its own title (pure `ElementRules.HasOwnTitle`).
- Not built (no data / runtime, not in the brief): the mock's Page Opening, Preview Tap / Idle demo / block-entry Motion rows.

## Steps
- [x] 1a. Breadcrumb context: ElementRules.HasOwnTitle + PageTopBar name switch + BlockFullView; tests (rule, sheet)
- [x] 1b. In-place video (test re-based: scroll to the button; PlayMode re-run due): full-screen button -> the card's player; test
- [x] 1c. DeviceStateStore + SetUpFixtures + fixtures cleaned + source-scan guard
- [x] 2. ElementEdits (pure) + EditMode tests on The Painting's story
- [x] 3. Page section: table, open row + accordions, add palette, header validate, Page Chrome override, Motion table
- [x] 4. Preview section: Automatic Shows, Custom linked / own / broken, Open Row override, reading time; Card Components Open Row row
- [x] 5. Real-window Editor tests (every action + Ctrl+Z), DrawingNeverWrites extended, Phase B live sync on the real card
- [x] 6. Captures of every state, judged
- [x] 7. Runs: targeted EditMode/PlayMode, FULL PlayMode (background), full EditMode
- [x] 8. Docs (_5.1, _3.3 Build log, 10-structure, 40-testing rule), LivingRoom scene ready, final report + review questions

## Notes
- PoiCardSceneTests.ARealTapOnTheStickyAction red in the first targeted run (1034 > 845): S2b's known red; re-run alone owed.
- Segments: SelectionGrid cells sit a margin apart; a probe that ignored it clicked into the gap (fixed). Motion segments say Auto; the note says what Auto plays.
- Targeted PlayMode 202/0/1 skip (behind). FULL PlayMode 1430: 2 red (state-leak tests, re-based, 20/20 alone). Full EditMode 2261: 2260/0/1 skip.
- Sentinels (mode dark, language pt, a recent search) survived the full PlayMode run; removed after (PlayerPrefs were empty before).
