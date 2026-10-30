# Plan tracker -- M1: the test ground audit (Test Suite Review 2)

Brief: the M1 brief (`_3.3_POI_Card_Styles.md` M1, pasted in chat). Worker: Opus, effort Medium. AUDIT ONLY: no product or test code changes.
Targets: (1) a component block tests in a few minutes on its own fixtures + EditMode; (2) one change to a block kind = ONE place of test
data; (3) no test spends time on something old, retired or no longer true.
Developer note (chat): the reference POI is the Ribeira Palace (`painting`); another POI only where needed.
RESULT: `proj_guides/_Test_Suite_2.md`. STOP with one AskUserQuestion.

## Steps
- [x] 0. GATE: UnityMCP live (telemetry_status), HEAD 5801c6d = origin/master, compile clean (0 error CS, ExitCode 0), marker SDF fonts static
- [x] 1. Data: POI x kind inventory of the shipped config (26 POIs; Lamp 62 blocks, Painting 18), test file x POI id map (lamp 100 files,
       painting 40), 64 test files hard-code block keys; last FULL PlayMode per-test times (TestResults.xml 08:00-09:16, copied to scratchpad)
- [x] 2. EditMode per-fixture times: full EditMode in the background (behind / behind): 2240 / 2208 / 0 failed / 32 skipped, 5.2 min
- [x] 3. Subagent S1 (Sonnet, medium): PART 2 -- held except two "duplicates" (About:200 vs Hero:221; ChromeScene footer names)
- [x] 4. Subagent S2 (Sonnet, medium): PART 1 move map -- held; block_15 lacks show_after_viewed (re-read)
- [x] 5. Gallery inventory: 170 block + 40 header cases, ids `kind_look_content`, ~1.8 s each (from the FULL XML)
- [x] 6. PART 3 component map + CORE check (21.4 min PlayMode, NavBar class tag 9.0 min)
- [x] 7. `_Test_Suite_2.md` written; `_3.3` status entry; `40-testing.md` RUN TIMES EditMode 5.2 min
- [x] 8. Final report (60-finishing 6.4, Subagents line) + AskUserQuestion
