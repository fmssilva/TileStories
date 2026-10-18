
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



## `And now: POI Card Navigation (3.2) -- step 11b, the contents block with reading times and seen ticks`

GATE: `git push` if the branch is ahead of origin (1 commit is waiting; report how many you pushed). Take one Unity
capture (Game view AND the POI Editor window) and open it; if none works, STOP and say so. Confirm the tree compiles; if
not, STOP and report. Check the two SDF fonts (`Oswald Bold SDF.asset`, `Roboto-Bold SDF.asset`) and `git checkout --`
them if TMP dirtied them. Baseline: the tree is clean on commit "Add an icon grid nav block and an Auto Nav Block
option" (full EditMode 1856/1856, full PlayMode 978/978) -- run the full EditMode suite and say which commit you
trusted.

Domain spec (read ALL lines): proj_guides/_3.2_POI_Card_Navigation.md -- "## TODOs" (none is yours) and the status
table (1-11a done; 11b is yours). Sections 5.6 (`nav_contents`), 3 (the `minutes` common field and Auto Nav Block),
4.4 (the spy line -- the seen tick uses the SAME line, not a second one) and 9.
REQUIRED READING (all lines): `_3.1_POI_Card_Blocks.md` on `CardLocalState` and on the common fields -- you add a slot
to saved state and a field every kind can carry, so read how both are owned before adding to them; _5.1_Editor_Tab.md
"HOW TO USE THIS FILE" + section "0. Guidelines" (the Reading Time field and the Test row that resets saved state);
.clinerules/40-testing.md 4.2 (no full run this block -- see TEST PLAN), 4.2.3, 4.4, 4.5.
Context only: _3.3_POI_Card_Styles.md, .clinerules/10-structure.md.
WORKER MODEL: Opus-class -- it writes PERSISTED state for the first time in this domain, and a wrong key or a missed
reset follows a visitor between walls.
TEST PLAN: full EditMode at the GATE and at the end. NO full PlayMode run: one new kind plus an additive slot in saved
state -- no container, no stops, no harness (4.2). It closes on every PlayMode fixture whose name holds Card or Poi,
PLUS every fixture a grep for `CardLocalState` finds (name them: the knowledge check and the content-seen tests are
likely). Green before you call the block done. It rides on 11a's full run (978/978); the next full run falls due at
step 14 -- and sooner than that, because the layouts of 13-15 will each change a container.

PART 1 -- none open.

PART 2 -- step 11b:

11b.1 The `minutes` common field (Editor label "Reading Time"), optional, per block instance (`_3.2` section 3). Audio
  and video do NOT carry it: they read their clip's length instead, so the contents list shows a time for them without
  anyone typing one. Say in the report which kinds end up with the field and which read a clip.
11b.2 Kind `nav_contents` (D2): a contents list at L3 -- one row per TARGET block (not per section): its heading, its
  minutes (the field, or the clip's length), and a seen tick. Rows grouped by section, in the stack's order. A tap
  jumps to the block. It is a BLOCK, like the icon grid: Block Library row, "+ Add block" under Navigation, and it
  joins the Auto Nav Block popup with no window edit (11a listed that popup from the registry -- say that you checked).
  A one-family card is still useful here, so rule 2.5.7 does NOT apply to it (it lists blocks, not sections) -- there
  is a test from 5b that says so in words; make sure it still holds.
11b.3 The seen tick. A block counts as seen once the SPY LINE has passed its top -- the same line `NavScrollRule`
  already uses, never a second definition. It is remembered per wall / POI / block in `CardLocalState` (a new `Viewed`
  slot).
  THE RISKS, and why this block is Opus: (a) the key must include the wall, so a visitor who walks to another wall does
  not arrive with ticks; (b) the auto-inserted block's key is `auto:nav_icon_grid` / `auto:nav_contents` -- stable, but
  check it round-trips through PlayerPrefs with its colon; (c) the Editor's "Reset Saved Card State" row must clear the
  new slot too, and its notice counts what it cleared -- extend it and prove it with the existing real-click test;
  (d) a release build must behave the same as the Editor here (no dev-only path).
  ACCEPTANCE: a tick appears after the block was in view, SURVIVES closing and reopening the card, and is gone after a
  reset. A block never seen has no tick.
11b.4 Auto Nav Block gains `nav_contents` as a choice; its (i) says what each of the two offers in a wall author's
  words.
  TESTS: Tier 0 for the minutes source (field, clip, neither) and for the seen decision at the spy line (just above,
  just below, at the end of the stack). Phase A: `nav_contents` x the four gallery cases, with and without ticks.
  Phase B in LivingRoomScene with real input: a real drag past three blocks ticks exactly those three; the ticks are
  still there after the card is closed and reopened; Reset Saved Card State clears them; a tap on a row lands its
  block (relative, +-1 px).
  VISUALS: the contents block on the real Lamp at 390 px in EN and PT, with some rows ticked and some not (the tick
  must not be the only signal that a row was read -- check contrast and say what the second signal is), the Editor's
  Reading Time field, and the Auto Nav Block popup with both choices.

11b.5 DOCS: `_3.2` status row 11b + proof; `_3.1` wherever it states what `CardLocalState` holds and what the common
  fields are -- those are facts that file states and they are now wrong; `_5.1` for the Reading Time field and the
  reset row's notice; `10-structure.md`.

Rules as before: no literal colours, sizes or visitor strings in C# (tokens + `CardStrings`); strict ASCII in code,
comments, logs and Editor labels; Editor rows and popups name things, never ids; pure rules decide, views draw; no
backward-compatibility shim; real fixtures only -- never change a POI's identity fields.
OUT OF SCOPE: `nav_rails` and `nav_bento` (15); the generic teaser and full view (12); layouts (13-15); presets (17);
`find_block` (18); any change to the slots, the chrome floor or `BlockStackBuilder`'s insertion rule.
Stop when the full EditMode suite and the named PlayMode fixtures are green with zero failures, the captures are
judged and 11b.5 is written. Ideas outside this domain -> proj_guides/__mixed_TODOs.md.
If the guide disagrees with the real code, STOP and report. Finish with the final report of `.clinerules/60-finishing.md` 6.4.
Commit only if the developer asks.

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
- **Session budget:** if the session is past roughly 500K tokens (or the context has been compacted once), finish the current
  sub-step green, then stop with a HANDOFF: what is done (with commits), what is not, the exact next sub-step, and the state of
  the tree. Never leave the tree red or half-edited.

### 2.5. Hard Stop
When you complete all the Iterative Execution Cycles for each step/task in the plan AND WE ACHIEVE AND TESTED WITH REAL TEST THAT ALL FEATURES WORK, ALL CONFIG PARAMS WORK AND ALL TESTS PASS, so then **STOP immediately**. Output the final learning summary and STOP. 


## `DURING THE WHOLE PROCESS: BE A TEACHER!!! - GIVE LEARNING SUMMARIES`
Act as a **Senior Staff Engineer** planning and implementing the whole plan, AND also, most important, as a **Teacher** explaining me all important details BUT IN A CONCISE AND NATURAL MANNER LIKE A CODER GUY EXPLAINING SOME DETAILS TO ANOTHER CODER GUY. So tell me **learnig summaries** along the whole process of planning and implementation and testing, of what we are doing and how and where, and maybe why if some nice decision was done. I want to learn how to do things without AI agents help, so teach me all important details in a concise manner (example things like syntax or workings of c# or unity and its workings,or framework used, or other details about cde implementations and also about tests, how we run tests etc.). So don't do some big report with formal tables as summaries!! Just tell me all important details in natural language and concise manner, like if it were clean, quick engineering notes I was taking in a class in order to study later so I can build and test this independently without AI. 

### AND SO, YOU NEED TO PRESENT ME A LEARNING SUMMARY ALONG THE WAY, EXAMPLE AFTER EACH FEATURE OR FILE IMPLEMENTATION, OR AFTER EACH TEST IMPLEMENTATION OR RUNNING OR AFTER EACH DEBUG SESSION, ETC. NOT JUST AT THE END OF CHAT. I WANT A LEARNING SUMMARY ALONG THE WAY SO I CAN FOLLOW YOUR ACTION CLOSELY!!!!


