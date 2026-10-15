
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

## `And now: POI Card Navigation (3.2) -- step 3, the nav controller, scroll-spy and jumps`

GATE: `git push` if the branch is ahead of origin (1 commit is waiting; report how many you pushed). Take one Unity capture
(Game view AND the POI Editor window) and open it; if none works, STOP and say so. Confirm the tree compiles; if not, STOP
and report. Check the two SDF fonts (`Oswald Bold SDF.asset`, `Roboto-Bold SDF.asset`) and `git checkout --` them if TMP
dirtied them. Baseline: full EditMode on the tree as you find it -- it must be 1683/1683 (commit "Add the card navigation
model and a Format for every block kind"; its PlayMode rides on 34d04f7's 778). Run it with Unity IN FRONT this once:
PART 1 is exactly the test that fails unfocused, and you need its red before you fix it.

Domain spec (read ALL lines): proj_guides/_3.2_POI_Card_Navigation.md -- "## TODOs" (the [step 3] line is yours; the two
[step 5] lines are NOT) and the status table (steps 1-2 done; step 3 is yours). Sections 4.1, 4.4 and 9 are the step;
4.2 tells you where the chrome will sit later, so the controller must not assume it.
REQUIRED READING (all lines): .clinerules/40-testing.md 4.2 (the cadence; this block DOES owe a full PlayMode run), 4.2.3
(tests never depend on window focus -- PART 1 is a violation of it) and 4.2.4b; _5.1_Editor_Tab.md "PlayMode Live Config"
(the sheet is rebuilt live and the controller must survive it).
Context only: _3.1_POI_Card_Blocks.md section 3, .clinerules/10-structure.md.
WORKER MODEL: Opus-class -- it wires the first stateful owner into the live sheet and touches the shared test harness.
TEST PLAN: full EditMode at the GATE (Unity in front, see above) and at the end. This block DOES owe a FULL PlayMode run:
it edits the sheet / stack and the shared test harness, which 4.2 calls shared ground. Take it with Unity in the
background -- that is now the default, and PART 1 is what makes it trustworthy. It is also the run the next three blocks
will ride on, so report its counts carefully.

PART 1 -- the harness fix that the cadence now depends on (do it FIRST, before any new code):
1. `LivePlayModeCardTests.TheRealWindow_WhilePlayModeRuns_...` loses keystrokes when Unity is not the active application
   ("Live heading" arrived as "Live ", "Dismiss" as "Dis"); reproduced in `_3.2` step 2 and logged in `__mixed_TODOs.md`
   "Test harness". The cause is in the SHARED harness, `Tests/Editor/PoiEditorWindowHost.ReplaceText`: it sends one
   `KeyDown` per character and assumes one repaint delivers one character, which an unfocused Editor does not promise.
   Fix it in the harness, not in the test, and not by forcing focus. Weigh at least three ways (00-process PLAN MODE) --
   e.g. read the field back after each character and only advance when it took; type and then poll the probe's value to a
   bounded timeout with a message that names what arrived; drive the field through the same event path a focused Editor
   uses -- and say why you picked yours. PROOF: the fixture passes with `editor_is_focused: false` TWICE in a row, and
   still passes focused. Then re-run every other EditMode fixture that types through this harness (grep `ReplaceText`)
   unfocused: the harness is shared ground, so a fix there must not quietly change what they assert.
   Remove the item from `__mixed_TODOs.md` when it is proven; leave the before_after one there (still not reproduced).

PART 2 -- step 3:

3.1 `NavController` (Runtime/UI/Cards/Navigation/, one per sheet, owned by the sheet next to the stack).
  WHAT: `JumpTo(sectionIndex)`, `JumpToBlock(blockKey)`, `Current`, `CurrentChanged`. It owns no element and no element
  owns it. ACCEPTANCE: a jump puts the target block's slot top at the viewport top within +-1 px, clamped to the scroll
  range; the spy names the right section at three different offsets.
  HOW: the arithmetic belongs in the pure `NavScrollRule` (Runtime/Blocks/Navigation/), the controller only applies it --
  target offset = the slot's `layout.y` clamped to `[0, content - viewport]`, current section = the last whose first
  target's top is `<= offset + viewport/3`, and the LAST section once the offset is within 1 px of the end (`_3.2` 4.4).
  `jump_raises_to` (default `full`) raises the sheet BEFORE scrolling; `jump_motion` smooth animates `scrollOffset` over
  `--ts-duration` through the panel scheduler, instant and reduce-motion set it at once. While a jump animates the spy is
  FROZEN on the target section, then released -- test that explicitly, it is the flicker the guide warns about.
  Do NOT use `ScrollView.ScrollTo` (the docs do not say where the child lands, `_3.2` references).
3.2 `IBlockHost.ScrollToBlock` + the `nav_jump` `CardEvent` (element key, section key) through `ICardEvents`, so the
  evaluation of `_7.1` can later count which elements visitors use. Find the existing `CardEvent` / `ICardEvents` shape
  first and follow it; do not invent a second event path.
3.3 The [step 3] TODO of the guide: `BlockStackView` states the ShowAfterViewed facts inline while `NavOntologyRule`
  states them too. Make the view call `NavOntologyRule.WaitsToBeRevealed` with the same `Seen` predicate the host hands
  the model, so the stack and the nav model can never disagree about a hidden block. TEST: one assertion that both
  answers come from the same call for a waiting block and for a revealed one.
3.4 The sheet builds the model and the controller in `Show` and rebuilds them on a live edit, a POI switch, a language
  switch and a reveal. `PoiCardHost` stays a wiring class (it was 266 lines): no decision in it.
  TESTS: Tier 0 `NavScrollRuleTests` (offsets, the clamp, the spy line at 3 offsets, the end-of-range case, the freeze).
  Phase B in LivingRoomScene with real input (`CardTestInput`): a jump to a late family lands within +-1 px (assert the
  slot top against the viewport, RELATIVE, never a pixel constant); a real drag moves the spy to the right section; a
  live edit rebuilds the model without losing the scroll (the pattern `PoiCardLiveUpdateTests` already proves).
  VISUALS: none required -- nothing new is drawn yet. If you capture anything, it is for your own check, not the report.

3.5 DOCS: `_3.2` status row 3 + proof, and remove its [step 3] TODO line; `10-structure.md` for the new files; the RUN
  TIMES line of `40-testing.md` 4.2 after the full PlayMode run (it is the record the next three blocks ride on).

Rules as before: pure rules hold the arithmetic, the controller applies it, views own nothing; no literal sizes (the spy
line and the durations come from tokens or named constants with a comment); strict ASCII in code and logs; no
backward-compatibility shim; real fixtures only -- never change a POI's identity fields.
OUT OF SCOPE: `BlockRef`, `open_block` and system Back (step 4); every nav element, slot and the chrome rule (step 5+);
the two [step 5] Editor TODOs; layouts.
Stop when the full EditMode suite and the FULL PlayMode suite are green with zero failures and 3.5 is written.
Ideas outside this domain -> proj_guides/__mixed_TODOs.md.
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


