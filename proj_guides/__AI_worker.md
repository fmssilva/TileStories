
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


## `And now: POI Card Navigation (3.2) -- step 8, the menu overlay owner, the drawer and the grid`

GATE: `git push` if the branch is ahead of origin (1 commit is waiting; report how many you pushed). Take one Unity
capture (Game view AND the POI Editor window) and open it; if none works, STOP and say so. Confirm the tree compiles; if
not, STOP and report. Check the two SDF fonts (`Oswald Bold SDF.asset`, `Roboto-Bold SDF.asset`) and `git checkout --`
them if TMP dirtied them. Baseline: the tree is clean on commit "Add dock, bar sheet and Up next to the card's bottom
area" (full EditMode 1795/1795; it rides on 7a's full PlayMode 868/868) -- run the full EditMode suite and say which
commit you trusted.

Domain spec (read ALL lines): proj_guides/_3.2_POI_Card_Navigation.md -- "## TODOs" (BOTH [step 8] lines are yours and
both are DECIDED: build what they say) and the status table (1-7b done; 8 is yours). Sections 5.3 (the two elements and
the overlay owner's rules), 4.2 (where the menu layer sits), 4.5 (`NavBackRule`'s order -- you add a branch to it) and 9.
REQUIRED READING (all lines): _5.1_Editor_Tab.md "HOW TO USE THIS FILE" + section "0. Guidelines";
.clinerules/40-testing.md 4.2 (this block OWES a full run -- and read the "a full run confirms, it never discovers"
line: the targeted set goes green first), 4.2.3, 4.4, 4.5; .clinerules/30-ui-content.md.
Context only: _3.3_POI_Card_Styles.md, .clinerules/10-structure.md, _3.1_POI_Card_Blocks.md (the takeover owner, whose
layer yours sits under).
WORKER MODEL: Opus-class -- a new layer on the card with its own lifecycle, and a change to the Back order that every
later overlay inherits.
TEST PLAN: full EditMode at the GATE and at the end. This block OWES the FULL PlayMode run: a new LAYER on the card and
a new branch in the system-Back order (4.2 shared ground). Close the Card / Poi targeted set (~640 tests) GREEN FIRST,
then start the full run, in the background, and report `editor_is_focused` at the first poll and at the end. Steps 9-11
ride on it.

PART 1 -- the two decided findings, which your own work makes cheap:
1. The More list (7a) and the kinds list (7b) are the only things on the card Back cannot dismiss, and a scroll or a
   jump leaves them open. Build the overlay owner FIRST, then make both lists its clients: they join `NavBackRule`'s
   order where the menu overlay sits (after the full view, before the sheet goes down) and they close on a jump and on
   a scroll. One owner, one order, no second dismiss path -- if a list needs its own rule, say why.
2. A bar reading a section that lives in More marks "More". While the current section is in the overflow, the More slot
   shows THAT section's icon and word and still opens the list. Test: a card whose current section is the 7th of 8
   marks the 7th's word in the slot, and tapping the slot still opens the list.

PART 2 -- step 8:

8.1 The overlay owner (the `menu` slot of `_3.2` 4.2: the card layer over the sheet, BELOW `TakeoverView`). ONE overlay
  at a time, a scrim, closes on Back, on a scrim tap, and after a jump; never stacked over another sheet (NN/g bottom
  sheets, Laubheimer 2023 -- the guide's references). It owns the More and kinds lists of PART 1 too. The header gains
  a menu button that opens the chosen `menu` element; it is drawn only when one is set.
  ACCEPTANCE: with an overlay open, Back closes the overlay and NOT the sheet; a second Back then lowers the sheet.
  The stops do not move (measure them, as every block since 5a has: 195.00 / 337.60 / 796.00).
8.2 `drawer_tree` (C3): the whole ontology as an outline with counts -- families, their kinds under them, from the
  header's menu button. The guide calls it the least discoverable and most complete element and says it is NEVER a
  default: keep `menu`'s default at `none` and say so in its (i).
8.3 `grid_overlay` (C4): big family tiles with counts and a Purpose / Format switch (the model has answered both axes
  since step 2 -- read `group_by`, do not re-derive). Kiosk-like, big thumbs.
8.4 The Menu Editor row (`none`, `drawer_tree`, `grid_overlay`), with (i), undo, live sync; and the seam 7a left open:
  when a Menu is set, a bar's More slot opens THAT menu instead of its small list. Prove both paths.
  TESTS: Tier 0 for the overlay owner's state (one at a time; what Back does at each layer, including with a takeover
  open). Phase A: both elements x the four gallery cases. Phase B in LivingRoomScene with real input: the menu button
  opens the drawer, a real tap on a leaf jumps and the overlay closes; a real tap on the scrim closes it and the card
  keeps its scroll; real Escape closes the overlay, then lowers the sheet; with Menu set, More opens the menu.
  VISUALS: the drawer and the grid open on the real Lamp at 390 px in EN and PT, the scrim's contrast against the card
  behind it, and the header with its menu button. Judge item by item: 48 px targets, nothing clipped, the overlay
  readable over the card, never colour alone.

8.5 DOCS: `_3.2` status row 8 + proof and BOTH [step 8] TODO lines removed; `_5.1` for the Menu row and the header
  button; `10-structure.md`; RUN TIMES after the full run.

Rules as before: no literal colours, sizes or visitor strings in C# (tokens + `CardStrings`); strict ASCII in code,
comments, logs and Editor labels; Editor rows and popups name things, never ids; pure rules decide, views draw; no
backward-compatibility shim; real fixtures only -- never change a POI's identity fields.
OUT OF SCOPE: floating elements and the placed-model chip (9); the edge scrubber (10); in-flow nav blocks (11);
`find_block` (18); layouts; presets; Editor warnings (17).
Stop when the full EditMode suite is green, the Card / Poi fixtures are green, the FULL PlayMode suite is green with
zero failures, the captures are judged and 8.5 is written. Ideas outside this domain -> proj_guides/__mixed_TODOs.md.
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


