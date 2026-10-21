
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


## `And now: POI Card Navigation (3.2) -- step 17b, the Card Starter and the demo point`

GATE: `git push` if the branch is ahead of origin (1 commit is waiting; report how many you pushed). Take one Unity
capture (Game view AND the POI Editor window) and open it; if none works, STOP and say so. Confirm the tree compiles;
if not, STOP and report. The SDF fonts are Static since step 13: a modified font is a FINDING, not a `git checkout`.
Baseline: the tree is clean on commit "Add navigation presets, Editor warnings and per-kind chip icons" (full EditMode
1972/1972; it rides on 15b's full PlayMode 1113/1113 -- NOT 7a's, which the last report mis-stated) -- run the full
EditMode suite and say which commit you trusted.

Domain spec (read ALL lines): proj_guides/_3.2_POI_Card_Navigation.md -- "## TODOs" (the [step 17b] line is yours) and
the status table (1-17a done; 17b is yours). Sections 8.2 (the Card Starter: READ IT FIRST, it is the whole brief),
8.1 (why the demo point exists) and 7 (the row order, now corrected).
REQUIRED READING (all lines): `_3.1_POI_Card_Blocks.md` -- the Card Starter writes `poi.card.blocks`, which is THAT
domain's data: its facts and its ontology belong there, not in `_3.2`. Read how a block instance is shaped, how looks
and defaults resolve, and how Card Content draws a point's blocks; _5.1_Editor_Tab.md "HOW TO USE THIS FILE" + section
"0. Guidelines"; .clinerules/40-testing.md 4.2 (no full run this block), 4.2.3, 4.5; .clinerules/00-process.md
("Framework Completeness": this block is that rule's strongest claim -- a new wall gets good cards with zero authoring).
Context only: _3.3_POI_Card_Styles.md, .clinerules/10-structure.md.
WORKER MODEL: Opus-class -- it writes a point's content, which is the one thing in this project that is hard to undo
by hand, and it decides what a good card IS.
TEST PLAN: full EditMode at the GATE and at the end. NO full PlayMode run: an Editor writer plus fixture content, no
container, no stops, no harness. It closes on every PlayMode fixture whose name holds Card, Poi or SizeComparison,
plus anything a grep for the changed types adds. Green before you call the block done. It rides on 15b's full run; the
next full run is step 19's close-out.
WATCH ITEM: `PoiCardAudioSceneTests` failed twice in 17a's run and passed 11/11 alone (second sighting of that
fixture). If it fails in yours, STOP and investigate rather than re-logging it.

PART 1 -- the owed fix:
1.1 The Preset row moves to the TOP of the Navigation section (section 7's order is already corrected in the guide: a
  preset is where a wall author starts, not where they finish). One Editor test for the new order, recapture
  `300_CardNavigation_Preset_620.png`.

PART 2 -- step 17b:

2.1 THE CARD STARTER (section 8.2). Three or four ready-made sets of BLOCKS applied to a point in one click, plus
  "start from scratch". A set is DATA -- a list of kinds with their looks -- never code, and it writes ordinary block
  instances that the author then edits like any other.
  Candidates to settle WITH THE REAL CARD IN FRONT OF YOU, not on paper: Simple point (header, rich_text, gallery,
  sources); Rich point (+ timeline, quick_facts, fun_fact, audio_guide, related); AR point (+ model_3d, place_in_ar,
  show_on_wall). Build each, open it on a real point, LOOK at it, and change the lists until each one is a card you
  would ship. Say in the report what you changed and why -- that judgement is the deliverable, as much as the code.
  SAFETY: it applies to a point with NO blocks without asking. On a point that already has blocks it asks first, in a
  question that names how many blocks it will replace (`EditorDecision`, never a silent overwrite -- and never a modal
  inside a test, `40-testing` 4.2.2). One Ctrl+Z restores the point exactly as it was: prove it by walking the point's
  blocks before and after.
  WHERE ITS FACTS LIVE: `_3.1_POI_Card_Blocks.md` (the sets, their kinds, the rule) and `_5.1` (the Editor row).
  `_3.2` only points at them -- one home per fact.
2.2 THE DEMO POINT. The Painting has zero blocks today; give it a real card with a starter plus whatever hand editing
  makes it worth opening, and give Lamp - Residential all four in-flow nav kinds (`nav_icon_grid`, `nav_contents`,
  `nav_rails`, `nav_bento`) so one point shows the whole domain at once (section 8.1). Both configs, and The Lamp
  stays untouched -- its authored card is what the Auto Nav Block and Block Order tests measure.
  Write the LivingRoom wall's own `navigation` block too, by applying the preset you judge best for this wall: today
  the wall writes nothing and runs on the framework default. The shipped FRAMEWORK default does not change (section
  8.2) -- this is the wall's choice, written into the wall's config, which is exactly what a wall author does.
  Say which preset you chose and why.
2.3 The POI's own words. `The Lamp`'s summary is literally "POI_Lamp" and its card is titled "St George's Castle".
  For the demo to read as an app rather than a fixture, the points the developer will open need a real name and
  summary. CAREFUL: name and summary are IDENTITY-ADJACENT -- `00-process` forbids changing a fixture POI's identity
  fields (id, name, category, summary, keywords) because the search tests index them. So do NOT edit them here:
  measure what would break (grep the search fixtures for the names and summaries) and report it as a finding with the
  list, so the Architect can decide whether it is a block of its own. Build nothing of it.
  TESTS: Tier 0 on the starter sets (each set's kinds are all registered, every required field it leaves empty is one
  the author must fill, the "start from scratch" path writes nothing). Editor tests on the REAL window: a real click
  applies a set to an empty point and writes exactly its blocks; on a point with blocks the question is asked and
  Cancel changes nothing; one Ctrl+Z restores. Phase B: The Painting's card opens, renders and navigates like any
  other -- a jump, the spy, and the layout the wall now sets.
  VISUALS: The Painting's new card at 390 px in EN and PT at Half and Full, Lamp - Residential with its four nav
  kinds, and the Card Starter row in the Editor. Judge item by item -- and say honestly whether The Painting's card
  looks like something you would show someone.

2.4 DOCS: `_3.2` status row 17b + proof and the [step 17b] TODO removed; `_3.1` gains the Card Starter's sets and
  rule (its home); `_5.1` for the Card Starter row and the moved Preset row; `10-structure.md`.

Rules as before: no literal colours, sizes or visitor strings in C#; strict ASCII in code, comments, logs and Editor
labels (the cards' own words are DATA and keep their Portuguese accents); Editor rows, popups and warnings name
things, never ids; no backward-compatibility shim; never change a fixture POI's identity fields.
OUT OF SCOPE: the facets (18); the domain review (19); the framework's shipped default; renaming any point.
Stop when the full EditMode suite and the named PlayMode fixtures are green with zero failures, the captures are
judged and 2.4 is written. Ideas outside this domain -> proj_guides/__mixed_TODOs.md.
If the guide disagrees with the real code, STOP and report -- as 17a rightly did about the kind icons.
Finish with the final report of `.clinerules/60-finishing.md` 6.4. Commit only if the developer asks.



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


