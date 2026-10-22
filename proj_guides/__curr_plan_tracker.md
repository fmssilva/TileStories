# Plan tracker -- Domain Review _3.2 (POI Card Navigation), AUDIT close-out pass (2026-10-22)

Brief: a second short pass over `_3.2.1_Audit.md`; writes section 13 only. No features. Trusted commit: b184c8f (pushed at the gate).

## GATE
- [x] UnityMCP answered (telemetry_status); `.clinerules` read; Domain_Review.md read
- [x] `git push` (110d07b..b184c8f)
- [x] compile: 0 `error CS`, ScriptAssemblies rebuilt 09:40
- [x] no SDF font modified
- [x] Game view capture taken and opened; POI Editor capture NOT possible (Unity covered, private content deleted, window closed)

## Steps
- [x] 1. Verify chunks A-G on disk (29 test names, file line counts, guard wiring)
- [x] 2. Verdicts: FirstShown deleted (+ 3 test lines), Preview preset -> `_5.1`, chunk E pointer in `_3.3` (fixed "six" -> eight)
- [x] 3. Triage every open TODO (38 rows); write moved lines to `_3.3`, `_5.1`, `__mixed_TODOs.md`; `_3.2` "## TODOs" emptied
- [x] 4. Section 13 written (gate, chunks, leftovers, triage, numbers, state of the domain)
- [x] 5. Full EditMode 2021/2021, 135.6 s
- [ ] 6. Final report in chat; commit only if asked
