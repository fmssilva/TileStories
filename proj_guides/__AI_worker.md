
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


## `And now: _3.3 POI Card Look, Feel & Motion -- S3a the story's Editor: the Page's element table and the Preview`

GATE: `git push` if the branch is ahead of origin (report how many commits). Take one Unity capture (Game view AND the POI
Editor window) and open it; if none works, STOP and say so. Confirm the tree compiles; if not, STOP and report. Check the two
SDF fonts (`Oswald Bold SDF.asset`, `Roboto-Bold SDF.asset`): `git checkout --` them if modified. Baseline: none (HEAD b8ce380; S2c
closed with full EditMode 2234 and full PlayMode 1429, one non-reproduced layout-struggle red). Ask about Unity's place with
`40-testing.md` 4.2.3's a / b / c actions, word for word (EditMode runs with real-click Editor tests: action a); WAIT for runs as 4.2.5
says. Never disable a tool's safety check (`50-terminal_and_tools.md` 5.2).

Domain spec (read ALL lines): `proj_guides/_3.3_POI_Card_Styles.md` -- R16 row 6 (S3a: this block) and the "S2c review" row (its three
fixes), R17.1 + R17.7 (Page section FIRST, then Preview; the Automatic preview recomputed and shown live; Custom with linked / own rows;
Use in the preview / Copy / Unlink; broken links = warning rows), R17.3 (the Motion table: one line per row, the SAME field as the row's
accordion, the reduced fallback read-only), R17.4 (Tap per element where it offers one), R14.5 + R14.8 (the element table, Style as an
INLINE accordion -- no floating popup --, a new row born fully expanded with its defaults, the Validated column, reading time Auto /
minutes), R17.5 (component overrides: Open Row and Page Chrome), the S1 / S2a / S2b / S2c Build log entries (build on `ElementRow`,
`BlockSurfaces`, `ElementPreviewRule`, the Card Components section).
The visual target: `proj_guides/_3.3.1_Prototype/StoryEditor.dc.html` (the redrawn mock: Page then Preview, the P column, Validated,
Look / Motion / Tap accordions, the Motion table, Automatic / Custom with linked rows).
REQUIRED READING (all lines): `proj_guides/_5.1_Editor_Tab.md` "HOW TO USE THIS FILE" + section 0 (every Editor rule: rows, (i),
warnings, popups, undo / redo, unsaved, live Play Mode sync, captures); `.clinerules/40-testing.md` 4.2, 4.2.3 (one-tick gestures for
Editor-window tests), 4.2.4b, 4.5; `_2.0_Labels_And_Fonts_Design.md` §4 (the override-switch pattern).
WORKER MODEL: Opus-class -- a large IMGUI Editor surface on a new data model, real-input tests, design judgement on captures.
TEST PLAN: no baseline. EditMode real-window Editor tests + targeted PlayMode (live sync on the real card) first, then the FULL PlayMode
run (the wall's data and the Editor are shared ground), then full EditMode.

PART 1 -- the S2c review fixes: (1) the Page Chrome's breadcrumb carries the CONTEXT "POI > section"; the page's own name is added only
when the page has no big title of its own (the story has one); (2) an in-place video keeps a full-screen button to the card's player;
(3) TEST HYGIENE: no test reads or clears the developer's saved visitor state (light / dark in PlayerPrefs, `CardLocalState`): tests use
an isolated store; prove the developer's saved mode survives a full run.
PART 2 -- Specific Marker > Card Content, a story block: the PAGE section first -- the element table as in the mock (icon, kind, first
words or a thumbnail, the live "P" marks, Validated ✓ / ✕ toggles, move up / down + drag, duplicate, delete with the shared danger
affordance, "+ Add element" grouped by category); a row opens its content fields (EN / PT, media pickers) + inline accordions Look /
Motion / Tap; a new row opens fully expanded with its defaults, its empty fields marked; the block header shows "✕ N to validate" /
"✓ all validated"; the Page Chrome override; the Motion table. Then the PREVIEW section: Automatic with "Shows: picture #1 · title #3 ·
lede #4", following every Page edit; Custom with linked rows (from a Page row: "Use in the preview (linked)" / "Copy to the preview"; on
a linked row: "Unlink (make a copy)"; a broken link = a warning row with Unlink / Remove), own rows; the Open Row override; reading
time Auto / minutes.
PART 3 -- every edit through `DrawConfigMutationScope`: undo / redo, the unsaved flag, live Play Mode sync onto the real card (edit a
row's text, its tone, its motion, reorder, delete -- the card follows).
TESTS: real-window Editor tests for every action (one-tick gestures, 4.2.3); undo of each; the live sync on the real card (Phase B);
the Automatic preview's "Shows" and P marks after reorder / delete; a broken link's warning. Captures of every state (the table
closed / a row open with each accordion / the Motion table / Automatic / Custom with a broken link), light Editor skin as the developer
uses.
DOCS as current truth: `_5.1` (the story's Card Content rows), `_3.3` Build log, `10-structure.md` (only your lines).
OUT OF SCOPE: templates and Card Starter (S3b); the POI-level shared elements (phase 2); any new element kind.
Stop when a developer can compose and restyle The Painting's story entirely in the Editor with undo and live sync, the S2c fixes are
in, the targeted fixtures, the full PlayMode run and full EditMode are green. Finish with the final report of
`.clinerules/60-finishing.md` 6.4 and END with the developer's review questions. Do not commit: the Architect commits after the review.


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


