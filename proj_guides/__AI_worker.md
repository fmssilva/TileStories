
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



## `And now: POI Card Navigation (3.2) -- step 4, BlockRef, Open Block and system Back`

GATE: `git push` if the branch is ahead of origin (1 commit is waiting; report how many you pushed). Take one Unity capture
(Game view AND the POI Editor window) and open it; if none works, STOP and say so. Confirm the tree compiles; if not, STOP
and report. Check the two SDF fonts (`Oswald Bold SDF.asset`, `Roboto-Bold SDF.asset`) and `git checkout --` them if TMP
dirtied them. Baseline: the tree is clean on commit "Add the card's nav controller and fix the editor typing harness",
whose full runs were green (EditMode 1694 / PlayMode 788, both in the background) -- run the full EditMode suite only and
say which commit you trusted. Unity may stay in the background throughout, including for the closing runs.

Domain spec (read ALL lines): proj_guides/_3.2_POI_Card_Navigation.md -- "## TODOs" (the [step 4] line is yours; the two
[step 5] lines are NOT) and the status table (steps 1-3 done; step 4 is yours). Sections 3 (the four rows marked step 4),
4.5 and 7 are the step; 5.5 and 5.6 tell you who else will hold a BlockRef later, so design the field for them too.
REQUIRED READING (all lines): _5.1_Editor_Tab.md "HOW TO USE THIS FILE" + section "0. Guidelines" (you add a field TYPE,
four rows and a popup to the POI Editor -- this is the file that says how); .clinerules/40-testing.md 4.2 (the cadence:
this block DOES owe a full PlayMode run, see TEST PLAN), 4.2.3 and 4.5 (captures are judged by you, item by item);
.clinerules/20-code-quality.md "two UI elements that look identical" (a BlockRef popup is not the PoiRef popup: read it
before reusing either).
Context only: _3.1_POI_Card_Blocks.md section 5, .clinerules/10-structure.md.
WORKER MODEL: Opus-class -- a new field TYPE reaches every kind's drawer, and Back touches the card's input at the root.
TEST PLAN: full EditMode at the GATE and at the end. This block DOES owe the FULL PlayMode run: Back wires input into the
host, which 4.2 counts as shared ground. Take it in the background. It is deliberate that THIS block pays rather than an
arbitrary fourth one -- steps 5, 6 and 7 then ride on it, so report its counts carefully for them.

PART 1 -- fixes (small, do them first):
1. `PoiEditorWindowHost` reads `EditorGUI.activeEditor` by reflection and uses the result without checking the FieldInfo
   was found: a Unity version that renames it gives a bare NullReferenceException instead of a readable failure. Assert
   the field exists once, with a message naming it. Proof: the existing `PoiEditorWindowHostTests` stay green.

PART 2 -- step 4:

4.1 The `BlockRef` field type.
  WHAT: a field whose value is the key of another block ON THE SAME POI. In Specific Marker > Card Content it draws a
  popup listing this POI's blocks by the one-line summary the Card Content rows already show (`_3.1` 15.1). A block that
  was deleted later: the row shows a warning that names what is missing and the button is NOT drawn (the visitor never
  gets a dead jump). ACCEPTANCE: picking a block writes its key; Ctrl+Z restores the previous pick; a stale key warns.
  HOW: add it to `BlockFieldType` + `BlockFieldDrawer` beside the existing types, and reuse the summary helper rather
  than re-deriving the words. `BlockRegistry.Validate` must refuse a BlockRef the same way it refuses other broken
  fields. It is NOT the PoiRef popup and not the (i) popup (20-code-quality): share the mechanism, keep the call sites
  apart. `BlockFieldDrawer.cs` is 510 lines and `__mixed_TODOs.md` already asks for it to be split -- if your addition
  pushes it past ~560, split it along the line that TODO names and say so; do not let it drift.
4.2 The `open_block` action on the `actions` kind: an action whose target is a BlockRef. Tapping it calls
  `IBlockHost.ScrollToBlock` (built at step 3) -- do not add a second path. The repeated-action warning of `_3.1` 15.2
  must count it like the others; check what it does with two Open Block buttons pointing at DIFFERENT blocks and decide
  (a warning there would be wrong) -- say what you decided.
4.3 System Back (`NavBackRule`, pure): each press undoes the innermost thing in the order of `_3.2` 4.5 -- an open full
  view closes; else an open menu overlay (none exists yet: leave the branch unbuilt, not stubbed); else the sheet goes
  one stop down (Full -> Half -> Peek); at Peek it closes through `SelectionEventBus.Clear`, like the X. `back_button =
  none` leaves Back to the app. The host reads the Input System's `Keyboard.escapeKey` (where Android's Back arrives in
  a Unity player); the Editor tests press Escape. iOS has no Back: nothing changes there.
  Verify on the REAL input path that Escape does not collide with anything already listening (search has its own
  Escape): if it does, STOP and report rather than choosing for the search domain.
4.4 Editor rows in Detail Card > Navigation, in the order section 7 gives: Jump Opens Card To, Jump Motion, Back Button
  (and Show Navigation At stays for step 5). Each with its (i), default, undo and live sync, in
  `POIEditorToolWindow.CardNavigation.cs` with its help in the CardNavigationHelp partial.
  TESTS: Tier 0 `NavBackRuleTests` (every step of the order, including `none` and the Peek close). Editor:
  `DetailCardEditorTabTests` -- a real pick in the BlockRef popup's write path, the stale-key warning, a real click on
  each new row + Ctrl+Z. Phase B in LivingRoomScene with real input: a real tap on an Open Block button scrolls to its
  block (assert the slot top against the viewport, relative); real Escape presses walk Full -> Half -> Peek -> closed;
  with Back Button = none nothing happens. Add the Back case to the existing nav fixture rather than a new one if it
  fits.
  VISUALS: the POI Editor at 620 pt showing the three new rows AND a Card Content row with a BlockRef field (the popup
  list itself cannot be captured -- a native IMGUI menu, the documented limit). Judge both item by item.

4.5 DOCS: `_3.2` status row 4 + proof and its [step 4] TODO line removed; `_5.1` for the new field type and the three
  rows (both the file inventory and the row detail); `10-structure.md`; RUN TIMES after the full run.

Rules as before: no literal colours, sizes or visitor strings in C#; strict ASCII in code, comments, logs and Editor
labels; Editor warnings and popups name things, never ids; pure rules hold the decisions, the host only wires; no
backward-compatibility shim; real fixtures only -- never change a POI's identity fields.
OUT OF SCOPE: every nav element and slot, `CardChromeRule`, Show Navigation At / Hide While Scrolling (step 5); the menu
overlay (step 8); hotspot pins (step 10); the two [step 5] Editor TODOs; layouts.
Stop when the full EditMode suite and the FULL PlayMode suite are green with zero failures, the captures are judged and
4.5 is written. Ideas outside this domain -> proj_guides/__mixed_TODOs.md.
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


