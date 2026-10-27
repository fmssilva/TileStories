
# `claude agent`
- **GATE TASK:** start by confirming UnityMCP server mcp is working in this claude chat. (don't confuse with a failed and different unity-mcp). check telemetry_status to confirm the good one if needed. If UnityMCP tools appear unavailable, don't assume they're unimplemented. STOP and tell the user what to check to confirm unity mcp works - check /mcp and reconnect...

- **CLAUDE.md:** I don't have it in this project. Instead i have this .clinerules folder with these files which are the general guidelines for this project:
  .clinerules\00-process.md
  .clinerules\10-structure.md
  .clinerules\20-code-quality.md
  .clinerules\30-ui-content.md
  .clinerules\40-testing.md
  .clinerules\50-terminal_and_tools.md
  .clinerules\60-finishing.md
So start by reading them all. 



## `And now: Test tiers -- BLOCK / CORE / FULL (the first Test Suite Review, audit + the tiers + the biggest savings)`

GATE: `git push` if the branch is ahead of origin (report how many commits). Confirm the tree compiles; if not, STOP and report.
Check the two SDF fonts (`Oswald Bold SDF.asset`, `Roboto-Bold SDF.asset`): `git checkout --` them if modified. Leave `IPCE/` alone
(not this project). Ask about Unity's place with `40-testing.md` 4.2.3's a / b / c actions, word for word; WAIT for runs as 4.2.5
says. Never disable a tool's safety check.
BASELINE: HEAD a46bcc2 (S4a: the page Back, Full Height, the premium hero, the Navigation Bar) was committed WITHOUT a known full-run
result. Known reds there: full EditMode 6 tests reading the developer's own LivingRoom config (the LiveSync badge x5,
LivePlayModeNavigationPreset) + 9 DetailCard navigation tests doubled by an order-dependent leaked window. This block's FULL runs
are also S4a's verification: a red that is a PRODUCT bug of S4a -> fix it if it is small and clear (say so), else STOP that test's
work and report it.

Spec (read ALL lines): `proj_guides/skills/Test_Suite_Review.md` (this block = its first run, AUDIT + FIX), `.clinerules/40-testing.md`
(all of section 4), the watched flakes in `proj_guides/__mixed_TODOs.md`.
WORKER MODEL: Opus-class -- test infrastructure every fixture stands on.
GOAL (the developer's): every block's tests run in a few minutes and test that block; every 3-4 blocks a CORE run of ~30 min;
every ~12 blocks the FULL suite, ~90 min at most.

PART 1 -- measure. Make `TestRunRecorder` write each run's per-FIXTURE times (fixture, tests, passed / failed / skipped, seconds)
to `TestEvidence/Harness/fixture_times.csv`, overwritten per run. Then one FULL PlayMode run + full EditMode (background, action
c) on the baseline: the time table AND S4a's verdict. Rank the fixtures by time.
PART 2 -- the CORE tier: `[Category("Core")]` on the tests that guard each domain's critical paths -- at least: open a card by a
real tap and close it; the sheet's stops and drag; the block page and Back; the Navigation Bar (jump, the chosen group follows the
scroll); the hero and the theme (light / dark); the story block and its page; audio play / stop; markers, search and zoom; the
Editor's live sync and a config round trip; the visitor-state isolation. Where a fixture is slow, put the category on its
cheapest test that proves the path, not on the class. Time it with `category_names: ["Core"]`; target 25-30 min; list each Core
test and the path it guards in `_Test_Suite_1.md`.
PART 3 -- the BLOCK tier: any PlayMode fixture over ~3 min is a block's bottleneck: split it by subject or slim it (Test_Suite_Review
step 2: scene once per fixture, waits on conditions, a matrix cut to one case per behaviour + the pairs that really interact,
duplicates merged). Never drop a real bug's regression or a contract guard (tokens, fields, render targets, contrast); say what
proves each removed case now. Do the largest savings that fit this block; the rest goes in `_Test_Suite_1.md`'s fix list, in
block-sized chunks with the expected saving each.
PART 4 -- the reds and the flake: the order-dependent leaked window that doubles the 9 DetailCard navigation tests (find what
leaves it open, fix it at the source -- 4.2.3); every test that reads the developer's own LivingRoom config choices instead of its
own fixture config (the LiveSync badge x5, LivePlayModeNavigationPreset, any other the full runs show): make it own its data, so a
developer's edit of the wall never turns a test red. The watched flakes: cause or a named quarantine.
PART 5 -- the rule. Rewrite `40-testing.md` 4.2 "Which tests, when" as current truth, replacing its levels with:
  "**Which tests, when (three tiers).** RUN TIMES (replaced after a run when a number moved > ~10 %): full EditMode ~N min; CORE ~N
  min; FULL PlayMode ~N min; per-fixture times in `TestEvidence/Harness/fixture_times.csv`.
  - **BLOCK -- every block, after each change:** compile, the full EditMode suite, then the PlayMode fixtures of the changed code
    (the feature's own + every fixture that greps for the changed type, method or config key), named one by one in `test_names`;
    a few minutes. A block closes on this tier unless its TEST PLAN says CORE or FULL.
  - **CORE -- `category_names: ["Core"]`, ~30 min, background:** every 3rd-4th block, and at the end of a block on SHARED GROUND
    (the definition below).
  - **FULL -- everything, <= 90 min, background:** every ~12 blocks with the maintenance, before a domain close-out, a device
    session or a build.
  - A wider tier confirms, never discovers: green on BLOCK before CORE, green on CORE before FULL. The report names the tier, its
    counts, and the blocks since the last CORE and the last FULL."
  Keep the SHARED GROUND definition (shorten it), the `test_names` JSON-array note, the 25-failures note and the gated MEASUREMENTS
  note; drop the "fourth block" full-run rule and every sentence the tiers replace.
TEST PLAN: the FULL run of Part 1 (baseline + S4a verdict); at the end one CORE run (timed) and one FULL PlayMode + full EditMode
run, all green, background (action c).
RESULT: `proj_guides/_Test_Suite_1.md` (time table before / after, the Core list, findings, the fix list, S4a's verdict). DOCS:
`40-testing.md` as above, `10-structure.md` (only your lines).
OUT OF SCOPE: product code (only test code and the recorder) except the small S4a fixes above; new features.
Stop when Parts 1-5 are done, CORE and FULL are timed and green. Finish with `.clinerules/60-finishing.md` 6.4's final report and
END with the developer's review questions. Do not commit: the Architect commits after the review.


## `DO THIS IN 2 MAIN STEPS: PLAN AND ACT`
## `STEP 1 - PLAN`
Before you start executing and implementing things, perform a critical review of the project on disk, and prepare a granular blueprint for the given tasks to be done. For that do the following: 

### 1.1. Guidelines & Architecture Re-Grounding
- Workspace rules: read ./.clinerules/ (specifically 00-process.md, 10-structure.md, 20-code-quality.md, 30-design-system.md, 40-testing.md, 50-terminal_and_tools.md, 60-finishing.md)
- Structure map: read ./.clinerules/10-structure.md
»» read all the lines on all these files

### 1.2. Deep Codebase Audit & Gap Analysis
- Inspect physical files on disk that might be related with this task before planning. Do NOT trust past conversation summaries alone — a prior session's summary claiming something exists or is missing must be independently verified on disk. And sometimes in previous commands you said that some files were missing and not implemented and then we found them in some other place. So actually check the existing files: 
  - Framework Path: C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework
  - App Path: C:\Users\franc\Desktop\TileStories\TileStories\Assets\Apps\LivingRoom
- Identify missing details, typos, logical gaps, or misalignments with .clinerules.

*Read File:* for big files use `read_file` paginate with `start_line`/`end_line`__ (1-indexed) on the same path. for binary prefabs use the MCP `manage_prefabs get_hierarchy` to get live Unity-resolved structure

- Confirm project compile and all unit/editor/player mode test pass via Unity MCP (if unity MCP is not available STOP immediately and report).

### 1.3. Online & Technical Research (If Needed)
If a decision genuinely needs outside confirmation (Unity patterns, package APIs, platform constraints...), search or inspect local node_modules documentation rather than relying on memory.

If needed you can use Tavily MCP to check Unity docs and patterns, research papers, or community solutions.

### 1.4. Proposed Plan
Present a clear, exact implementation plan for the requested adjustments/tasks. The plan should be well though and detailed, BUT WRITTEN HERE IN CHAT IN A CONCISE MANNER. Think about what we need to do, how, where, why, tests, etc. Example think about: 
   - **WHAT:** Acceptance criteria and deliverables.
   - **HOW:** Design patterns, code quality rules (20-code-quality.md), de-risking code stubs, or exact package paths to import...
   - **WHERE:** Exact disk paths per 10-structure.md.
   - **WHY:** the rationale for this approach over the alternatives considered.
   - **TESTS:** per .clinerules/40-testing.md, see which layers of tests you should implement and what each one specifically needs to check for this task — don't plan a boilerplate list of tests. But make sure to include tests to confirm that the logic works, and all config params work as expecteed, etc. And implement real tests, not "mock code tests that don't really confirm anything". Focus on "language based tests (Edit/Play Mode) - vision and human will be done later.
       - In terms of the tests themselves, so you know, we have 3 sets of already defined POI markers around the "Lamp, Painting and Camera" main POIs. Many with different positions, categories, effects, hierarchy, with reveal delays, etc... The Lamp set is the bigger one with some 12+ POIs. So confirm well the POIs configs when implementing some test, and if needed add new POIs to make sure we actually test all situations and edge cases. 
       - And in terms of tests to implement, lets use real code for real testing. No mock code!!

### 1.5. Architectural & Implementation Decision Analysis
#### In general, for each doubt it occurs to you, and when deciding the best options, remember some main principles: 
- That I want to implement a very complete and versatile Framework with all the good options and features that is good to have for this domain, and with clear "selection options of those features in the Editor Tab + clear variables and param config means" for the developer to select and adjust what he wants for its concrete App. 
- That I want the code well organized and simple and clean as possible, with all the WHAT/HOW/WHERE/WHY questions answered according to the .clinerules/ and to the Target Domain Spec we are implemeting now. 
#### And so do you best analysis, reason at two separate levels: 
1. **Architecture Level (Where it lives):** Map folder, assembly, and component scope. Evaluate 3 distinct structural options with trade-offs, state the choice, and explain why.
2. **Implementation Level (How it works):** Evaluate 3 concrete implementation choices (e.g., event-driven vs. direct call, ScriptableObject vs. hardcoded) and select the simplest, most robust option.

### 1.6. Decisions & Questions
At the end of your response, categorize decisions clearly:
a) **Decisions made autonomously by the agent:** Fully resolved choices embedded directly into the proposed plan. All decisions that have a clear winner or where both options are indeed good should be automatic decided, plan with it, and just tell the user at the end decisions made. 
b) **Decisions requiring developer input (IF ANY):** Trade-offs where no clear winner exists, requiring developer selection before proceeding. Important, you don't need to have this. If you can solve all decisions by yourself so do it.

### 1.7. End of Step 1 - present in chat:
a) present the **learning summary** explaining the main topics of what we'll do.
b) present decisions you made autonomously and (IF ANY) decisions for me to make.
c) gate: 
- if i need to make decisions before we finalize plan so STOP and wait for my answer.
- if there are no blocking decisions so present the full implementation plan with all important details BUT IN A CONCISE MANNER, and then you can continue right away to the ACT STEP. 


## `STEP 2 - ACT:`
### [CURRENT_MODE: ACT_MODE]`

After the plan, lets continue for the implementation. You are now already in ACT MODE. Proceed directly with disk edits, script execution, and code implementation.

### 2.1. Plan as Ground Truth
- Follow the implementation plan we just did. Write it to `./proj_guides/__curr_plan_tracker.md`, REPLACING whatever the previous block left there (it is in git), and tick its steps as you go: it is your TODO list and what you re-read after a compaction.

### 2.2. Strict Scope Lock
- Lets implement things related to the given tasks only. 
- NO unsolicited refactoring, code formatting cleanups, or extra features outside the active task. 

### 2.3. Iterative Execution Cycle
For every step/group of sub-tasks execute the following cycle:
1. **Read:** Confirm the step to do next (ground in the ./proj_guides/__curr_plan_tracker.md file if the plan is there). 
2. **Implement Code:** Write or modify only the files strictly required for that step/task. Maintain clear alignment with .clinerules.
3. **Implement Tests:** per .clinerules/40-testing.md, see which layers of tests you should implement and what each one specifically needs to check for this task — don't plan a boilerplate list of tests. Only the ones real needed for fast and robust developement. 
4. **Verify Code** Compile code and execute Edit/Playmode tests to confirm everything is ok. 
5. **Run Tests** Run the necessary tests you implemented - TESTS THAT ACTUALLY CONFIRM IF THINGS WORK OK FOR REAL, LIKE IF THE USER WAS CLICKING AND DOING THEM - NO MOCK TESTS. 
- 5b. **Look at it:** for anything with a visible result (POI Editor rows/tables/popups, Game-view renders, demo grids, the card), capture it and check it yourself with an item-by-item checklist (.clinerules/40-testing.md 4.5, "Visual verification is the agent's job"). Unity is open and unobstructed for this. Fix what the capture shows and capture again. Never report "not looked at".
6. **Learning Summary** present a "Learning Summary" for these steps we just did in chat, for me to keep track of what is beeing done, with all important details BUT in a concise manner, like i explained above. 

Repeat steps 1–6 until every step/group of sub-tasks in the plan is done. 

### 2.4. Terminal & Tool Discipline
Per ./.clinerules/50-terminal_and_tools.md:
- Chain commands with `;`, not `&&`.
- Use editor with verbatim old_text (including leading whitespace and allway replacing complete lines); if matching fails or is ambiguous, fall back to unityMCP__apply_text_edits or python edit_file.py. Always verify immediately by re-reading the edited region, running refresh_unity, and passing all tests.

### 2.4b. Keep going (the brief is the approval)
- The brief below IS the developer's approval for everything in it. Do NOT stop between sub-steps to ask "should I continue?".
  After each sub-step: tests green, a short learning summary, a suggested one-line commit message -- then go straight on.
- **The GATE is not optional.** Do every GATE item first (captures opened, compile, the Oswald font restored) and report
  each one in your final summary; a summary that skips a GATE item is incomplete.
- **Language and commit message:** write summaries in English. Suggest ONE short, natural commit message (one line,
  imperative, about 50-72 characters, no step codes), e.g. "Add 360 panorama viewer to the POI card".
- **How to go straight on:** in Claude Code a reply that contains ONLY text ends your turn and waits for the developer. So
  write the sub-step's learning summary and, in the SAME reply, make the first tool call of the next sub-step. Never end a
  reply with "continuing to X next" and no tool call.
- Stop ONLY for: (1) a failed GATE; (2) a decision with a real trade-off that the brief and the guides do not settle; (3) the
  brief's stop condition; (4) the session budget below. "The rest looks big" is not a reason to stop: finish the current
  sub-step green and continue with the next.
- **Session budget:** past roughly 500K tokens (or once the context has been compacted), START no new sub-step that writes code
  or changes behaviour. CLOSING work goes on up to roughly 700K: waiting for a running suite, re-running a red test alone or in
  a small group to see whether it is yours, docs, captures, the final report. Then stop with a HANDOFF: what is done (with
  commits), what is not, the exact next sub-step, and the state of the tree. Never leave the tree red or half-edited. Wait for
  long runs without polling (`40-testing.md` 4.2.5 "Wait without burning tokens"): every poll re-reads the whole session.

### 2.5. Hard Stop
When you complete all the Iterative Execution Cycles for each step/task in the plan AND WE ACHIEVE AND TESTED WITH REAL TEST THAT ALL FEATURES WORK, ALL CONFIG PARAMS WORK AND ALL TESTS PASS, so then **STOP immediately**. Output the final learning summary, PUT the "LivingRoom Sene" to be ready to play in unity (not the test scenes tc...), and STOP. 


## `DURING THE WHOLE PROCESS: BE A TEACHER!!! - GIVE LEARNING SUMMARIES`
Act as a **Senior Staff Engineer** planning and implementing the whole plan, AND also, most important, as a **Teacher** explaining me all important details BUT IN A CONCISE AND NATURAL MANNER LIKE A CODER GUY EXPLAINING SOME DETAILS TO ANOTHER CODER GUY. So tell me **learnig summaries** along the whole process of planning and implementation and testing, of what we are doing and how and where, and maybe why if some nice decision was done. I want to learn how to do things without AI agents help, so teach me all important details in a concise manner (example things like syntax or workings of c# or unity and its workings,or framework used, or other details about cde implementations and also about tests, how we run tests etc.). So don't do some big report with formal tables as summaries!! Just tell me all important details in natural language and concise manner, like if it were clean, quick engineering notes I was taking in a class in order to study later so I can build and test this independently without AI. 

### AND SO, YOU NEED TO PRESENT ME A LEARNING SUMMARY ALONG THE WAY, EXAMPLE AFTER EACH FEATURE OR FILE IMPLEMENTATION, OR AFTER EACH TEST IMPLEMENTATION OR RUNNING OR AFTER EACH DEBUG SESSION, ETC. NOT JUST AT THE END OF CHAT. I WANT A LEARNING SUMMARY ALONG THE WAY SO I CAN FOLLOW YOUR ACTION CLOSELY!!!!


