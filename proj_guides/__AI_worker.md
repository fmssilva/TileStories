
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

## `And now: _3.3 POI Card Look, Feel & Motion -- P4.3 the app's fonts (Fraunces + Atkinson Hyperlegible Next)`

GATE: `git push` if the branch is ahead of origin (report how many commits). Take one Unity capture (Game view AND the POI
Editor window) and open it; if none works, STOP and say so. Confirm the tree compiles; if not, STOP and report. Marker SDF
fonts: `git checkout --` them only if they changed (they hold 'ł' since P1b). The spike's dev fonts get rewritten by some test
runs: restore `Assets/Dev/StyleSpike/Fonts` with `git checkout --` before you finish. Baseline: none needed (HEAD c558d0b; P4.1
closed with full PlayMode 1233/1235, the two reds known and unrelated, full EditMode 2045/2046, the known focus flake). Keep Unity
FOCUSED during EditMode runs.

Domain spec (read ALL lines): `proj_guides/_3.3_POI_Card_Styles.md` -- R16 (this is row 1), R12 (all; R12.3 is this block),
R8 S2 (one asset cannot serve both systems; a TMP asset in USS BLANKS a label silently; a dynamic asset rewrote itself 5.8 KB ->
2.1 MB; static atlases), R12 "Build log", R15.2.
REQUIRED READING (all lines): `proj_guides/_2.0_Labels_And_Fonts_Design.md` (FontKeyLibrary, the wall font library, "Add font"),
`.clinerules/40-testing.md` 4.2 (fonts are shared ground: a full PlayMode run is owed), 4.2.4b, 4.5; `.clinerules/30-ui-content.md`.
Context only: `Assets/Dev/StyleSpike/Fonts/` and `S2*` tests (how P0 built the assets), `Runtime/UI/Theme/` (P4.1),
`ShippedTextGlyphTests`.
WORKER MODEL: Opus-class -- shared ground (every text of the app), two text systems, atlas generation, a ripple through text-
measuring tests.
TEST PLAN: targeted Card / Poi / Search / Theme / Marker fixtures first, then the FULL PlayMode run, then the full EditMode suite.

GOAL: the app speaks in two faces, from ONE font library serving both text systems: display = Fraunces SemiBold (600), body =
Atkinson Hyperlegible Next (Regular 400, Medium 500, Bold 700). Sizes, colours and layout DO NOT change in this block (P4.4):
only the faces. This is the first block whose result is visible.

MEASURE FIRST: render the P4.1 pixel set + the real card at Peek / Half / Full on The Painting and The Lamp, EN and PT, and
record the sheet's three stops and the header's measured heights. After: the same set, side by side, and the same numbers.

PART 1 (number sub-steps 4.3.1 ...):
1. Fonts into the Framework: `Runtime/UI/Theme/Fonts/<family>/` with the OFL text beside each `.ttf`. Static instances from the
   official google/fonts variable sources (fontTools, as P0 S2 did): Fraunces SemiBold, Atkinson Hyperlegible Next Regular /
   Medium / Bold. The spike's copies stay in Dev (do not move them; the spike's tests use them).
2. `FontKeyLibrary` grows from `{key -> TMP_FontAsset}` to `{key -> TMP asset + TextCore FontAsset}` built from the SAME `.ttf`
   (`_2.0`'s "Add font" and the wall library keep working: a wall font without a TextCore asset still works for markers and is
   simply not offered for the card). Framework keys: `fraunces_semibold`, `atkinson_next_regular`, `_medium`, `_bold` beside the 3
   existing ones. Markers' default font is UNCHANGED (a wall can now pick the new keys).
3. Static atlases, never dynamic at runtime: each asset holds Latin-1 + Latin Extended-A + the typographic punctuation the app
   uses (“ ” ‘ ’ – — … · ›), built by an Editor command. `ShippedTextGlyphTests` grows: every character of every shipped string
   (CardStrings EN + PT, option labels) and of the LivingRoom wall's content is in BOTH body and display assets; a missing glyph
   fails naming the character and the string. Bold / Medium through the TextCore font weight table, so `-unity-font-style: bold`
   picks Atkinson Bold (prove it: a bold label renders the Bold face, not a faux bold).
4. Theme type roles: `ThemeAsset` gets display / body font keys (+ the weights); the generator emits them so EVERY UI Toolkit
   panel's text is Atkinson by default and the display roles (the card title, block headings, section openers, page titles,
   menu headers -- list the selectors) are Fraunces. First check whether a USS variable can carry a font definition
   (`-unity-font-definition: var(--ts-font-body)`) on a real panel; if not, the generator writes the rules directly. A guard: no
   USS outside the generated theme sets a font definition.
5. Text-measuring tests will shift (widths, wraps, truncation, heights). Re-base each deliberately (old -> new in the tracker),
   expected values read from their source (4.2.4b), never weakened; a changed sheet STOP or a layout loop is a FINDING, not a
   re-base -- report it.

PART 2 -- docs: `10-structure.md`, `_2.0` (the library's new shape, the new keys), `_3.3` R12 Build log (P4.3 done, the stops /
heights before -> after, the counts).

VISUALS: before / after side by side for the pixel set and the real card (EN + PT, Peek / Half / Full), plus the P0 glare
render of a block in the new faces. Open each and judge: hierarchy, wraps, truncations, PT diacritics (ã õ ç á é í ó ú â ê ô à ł).

OUT OF SCOPE: sizes, the type scale, colours, light mode, the block frame (all P4.4); markers' default font; any new text.
Stop when every shipped and content character renders in both faces, the guard and glyph tests are green with mutation proof, the
targeted fixtures, the full PlayMode run and the full EditMode suite are green. Ideas outside this domain ->
proj_guides/__mixed_TODOs.md. If the guide disagrees with the real code, STOP and report. Finish with the final report of
`.clinerules/60-finishing.md` 6.4. Do not commit: the Architect commits after the review.


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


