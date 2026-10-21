# Plan tracker -- Domain Review _3.2 (POI Card Navigation), mode AUDIT (2026-10-21)

Brief: `__AI_worker.md` (Domain Review, AUDIT). Output: `proj_guides/_3.2.1_Audit.md`. AUDIT writes NO features: only runs, captures,
the report. Temporary tools (mutation switch, runner, matrix fixture) are deleted before the end; the tree ends as it began.

## GATE
- [x] UnityMCP answered (telemetry_status), 0 compile errors in the console
- [x] .clinerules 00-60 read, Domain_Review.md read, guide _3.2 read (all 1284 lines)
- [x] `git push`: branch was ahead by 1 (24849b9), pushed, now level with origin
- [x] SDF fonts: `git status` shows neither font modified
- [x] captures opened (Game view + POI Editor) -- at the matrix / visitor step
- [x] compile verified (Editor.log, no `error CS`) after the temporary tools go in AND after they come out

## Runs
- [x] Full EditMode: 1999 tests, 104.8 s, 3 failures (NavigationPresetEditorTabTests x3) -- to triage
- [x] Full PlayMode: 1148/1148, 3991 s (focused the first ~6 min, then background)
- [x] ten slowest fixtures from the PlayMode result

## Focus 1 -- mutation re-proof (after PlayMode ends: one Unity job at a time)
- [x] copy `AuditMutation.cs` + `AuditMutationRunner.cs` in, apply `apply_mutations.py` (138 mutations, 8 rules), refresh, no `error CS`
- [x] baseline 0 per fixture set green, then the sweep (results file), table rule -> mutation -> catching test
- [x] survivors: re-run against the wider sets (full EditMode, then the named PlayMode fixtures); a rule whose break nothing catches = MUST
- [x] `git checkout` the 8 rule files, delete the temp files, refresh, no `error CS`, `git status` clean of them

## Focus 2 -- 15 elements x 5 layouts
- [x] matrix from the tests (grep) + presets; name the holes
- [x] render the plausible-to-break ones (edge scrubber in pages, dock in bento, progress map in accordion, ...) with a throwaway fixture; captures judged

## Focus 3 -- flakes (verdict each)
- [x] "Layout update is struggling": count in this run's Editor.log, bisect to the tests, same shape as 17b?
- [x] PoiCardAudioSceneTests timing, ConfigUndoGroupingTests.DraggingASlider, KeywordListFieldTypingTests focus

## Focus 4 -- PoiCardSheetView: 585 lines (584 at step 16: +1), see report

## Focus 5 -- visitor's first card: The Painting at 390 px, EN + PT, end to end
- [x] captures scrolled end to end, judged as a visitor

## Skill sections 1-11 (+ focus sections) -> `_3.2.1_Audit.md`
- [x] 1 flow, 2 matrix, 3 missing features, 4 ontology, 5 editor quality (default width + 620 pt captures), 6 demo, 7 code health,
      8 test health, 9 product pass, 10 docs vs code + TODO triage, 11 fix list in block-sized chunks
- [x] chat finish (GATE, suites, matrix headline, top 5, missing features, not verified, plain summary, one commit message)

## Done 2026-10-21: report `_3.2.1_Audit.md`; temp tools removed; close-out EditMode 1999/1999 (115 s). Not committed (the developer's call).
