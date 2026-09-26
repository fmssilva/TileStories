# Select, Filter & Search -- round 2 (2026-09-26) plan tracker

- [x] 0. Baseline: UnityMCP ok; compile clean; EditMode 1058 (1 fail: KeywordListFieldTypingTests, window focus); PlayMode 186 (1 flaky: ARealMarkerTap, passes 4x alone)
- [x] P1. Stabilise the two tests (Focus() in the typing test -- passes unfocused; tap test failure message says what the tap reached)
- [x] P2. SearchKeywordSources.Collect = the one list of a POI's searchable words (index Build + Specific Marker "Found by" preview); CollectDerivedKeywords deleted; per-POI section renamed "Summary & Keywords"
- [x] P3. Per-POI Summary row (undo/redo test; live via the search applier's existing summary fingerprint) -- EditMode 1062/1062
- [x] P4. Keyword Fields as filters (SearchFieldDefinition.filterable + Filter column; FacetGroup value type + Field(key); tray group per field) -- EditMode 1064/1064
- [x] P5. Demo: 10 test cases with Expected(settings) (SearchDemoLayout.TestCases, SearchDemoCheck); Demo Test Cases in the readout; demo Material field (filterable); Demo Query popup; Editor mic hears Try a Query (DebugTranscriber.EditorPhrase)
- [x] P6. SearchSettingsFieldTests: one real-scene test per select_filter_search field (+ search_demo in SearchDemoTests), [CoversField] + CoverageGuard (shown load-bearing), renders + answered vision checklist (_2.6.1 items 15-23)
- [x] Found by the renders and fixed: zoom-on-select pushed the tapped crowd off screen (MaxZoomKeepingOnScreen + clump moved to the demo centre); 'No matches for ""' with filters only (search.no_results_filters_message)
- [x] P7. Cleanups + help/guides for P2-P5 (per-POI help moved to SearchFilterHelp.cs; stale Block-5 comment and brace block gone)
- [x] P8. Docs: _2.6, _2.6.1, _2.6.2, _5.1 (Domain Manual Tests: per-setting tests + guard + self-judging demo), 10-structure
- Final (2026-09-26): compile 0 error CS; EditMode 1067/1067; PlayMode full 220 -- 219 passed + the 1 stale assertion fixed and re-run in the Search suites 51/51
- Open: tray covers the relax button (device check); card covers the lower third after a zoom; Displacement leader line on the demo clump (other domain)
