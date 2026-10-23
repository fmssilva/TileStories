
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

## `And now: _3.3 POI Card Look, Feel & Motion -- P4.1 the theme pipeline (behaviour-free)`

GATE: `git push` if the branch is ahead of origin (report how many commits). Take one Unity capture (Game view AND the POI
Editor window) and open it; if none works, STOP and say so. Confirm the tree compiles; if not, STOP and report. The two marker SDF
fonts are committed with 'ł' since P1b: `git checkout --` them only if they changed. Baseline: no full run needed (HEAD 9375c94
= last green state: EditMode 2039/2039 at P1c, full PlayMode 1219/1219 at P1b part 2). Keep Unity FOCUSED during EditMode runs
(known focus flakes: `__mixed_TODOs.md` "Editor-window tests depend on focus").

Domain spec (read ALL lines): `proj_guides/_3.3_POI_Card_Styles.md` -- above all R12 (the whole section: this block is R12.7
step 1), R8 S1 (var chains, `customStyle` holds only an element's OWN declarations, a broken var paints transparent silently)
and S2, R1, and section 2 (today's token table + the LINEAR-colour scrim note). The old status table and sections 3-8 are being
replaced by the re-plan: build none of them.
REQUIRED READING (all lines): `.clinerules/20-code-quality.md`, `30-ui-content.md`; `.clinerules/40-testing.md` 4.2 (this block
touches the tokens and PanelSettings: a full PlayMode run is owed), 4.2.3, 4.2.4, 4.2.4b, 4.4, 4.5; `_3.1_POI_Card_Blocks.md`
section 3 "Styles" (`CardStyleSheets`, how the card's sheets load).
Context only: `Runtime/UI/Cards/CardTokens.uss`, `CardTokenColours.cs`, `PoiCardHost.cs`, `DevTools/CardGalleryHarness.cs`,
`Runtime/UI/Shared/PanelSettings.asset`, `UI Toolkit/UnityThemes/UnityDefaultRuntimeTheme.tss`.
WORKER MODEL: Opus-class -- a new architectural piece (a theme pipeline every panel reads) that must change no pixel.
TEST PLAN: no baseline run. Targeted Card / Poi PlayMode fixtures green first, then the FULL PlayMode run (background), then the
full EditMode suite (focused).

GOAL: one source of truth for the app's look, generated into USS, reaching every UI Toolkit panel, with C# colour reads off USS --
and the card looks EXACTLY as it does today. Visual changes come in later blocks (P4.3 fonts, P4.4 palettes).

MEASURE FIRST (before any edit): pick a fixed set of renders that covers the card's colours -- e.g. 8-10 `CardGalleryTests`
entries (header, a text block, a media block, nav chips with family colours, the menu overlay with its scrim, the mini-player,
the takeover / full view) plus the real card at Half and Full on The Lamp -- render them, and keep the PNGs as the baseline. At
the end, render the same set and diff them pixel by pixel: the expected diff is ZERO (state the max channel delta; any non-zero
pixel is a finding to explain, never to wave away).

PART 1 -- the pipeline (number sub-steps 4.1.1 ...):
1. `ThemeAsset` (ScriptableObject, `Runtime/UI/Theme/`): the T1 + T2 values of R12.2 that `CardTokens.uss` holds TODAY
   (surfaces, text, accent, lines, focus, status ramp, family colours, type sizes, radii, spacing, durations / easings, glass
   opacity). One theme `default`, one mode `dark` (today's Stone & Gold values, exactly). The shape must already allow a second
   mode and the R12.4 knob sets (empty lists now): P4.4 / P4.5 fill them. Values are data in the asset, never re-typed in code.
2. Generator (Editor): writes `Runtime/UI/Theme/Generated/Theme.default.uss` with the tokens on `.ts-app.ts-theme-default.ts-mode-dark`
   (deterministic order, a header saying it is generated and from which asset). A menu item / a button on the asset regenerates.
   `ThemeUssStaleTests` (EditMode): the committed USS equals the generator's output from the committed asset; mutation proof
   (change one asset value, the test goes red, restore).
3. `TileStoriesTheme.tss` imports `unity-theme://default` AND the generated theme; `PanelSettings.themeUss` points to it.
   `ThemeController` puts `ts-app ts-theme-default ts-mode-dark` on every runtime UIDocument root (the card's panel, search, zoom
   -- they all share the one PanelSettings) and on every host that draws the card outside the wall: `CardGalleryHarness` and any
   Editor window / preview that hosts a `.ts-card` (find them all; list them in the report).
4. `CardTokens.uss` keeps ONLY what is not T1 / T2 (sheet layout, the numbers C# reads -- top gap, collapsed header, track peek,
   bento, radial, float gap -- and every block / nav size), declared as today on `.ts-card` so `customStyle` still finds them.
   The T1 / T2 lines are deleted from it: they now arrive by inheritance from the app root. Same token NAMES (views keep reading
   `var(--ts-*)` unchanged). The six roots that add `ts-theme-default` today (`PoiCardSheetView`, `MiniPlayerView`,
   `CardMenuOverlay`, `FloatingSlot` + its scrim, `TakeoverView`) must still resolve every token: check each, and make sure
   none of them depended on re-declaring the colours themselves.
5. `ThemeResolver` (Runtime, plain C#): role + mode -> value from the active `ThemeAsset`. `CardTokenColours` (family colours
   read BY NAME from `.ts-card`'s custom style) is rewritten to read through it -- same results. Grep for every other C# read of a
   T1 / T2 COLOUR token by name and move it too; layout numbers stay as they are (step 4).
6. A guard test: every token NAME any USS file of the card / search / zoom uses as `var(--ts-...)` is defined somewhere it can
   resolve (theme or the component), so a deleted line can never paint transparent silently (S1). Mutation proof.

PART 2 -- docs: `10-structure.md` (the new files), `_3.3` R12 "Build log" (P4.1 done + the commit's counts + the pixel diff
result), the Editor-tab rule only if an Editor row was added (none expected).

VISUALS: the baseline set and the after set side by side, opened and judged (they must be indistinguishable; say so with the
measured diff). Plus one Game-view capture of the wall with the card open and one with search open.

OUT OF SCOPE: any visual change (fonts, light mode, new colours, metal, glass); SearchUI / Zoom / markers moving onto the theme
(P4.2); the knob classes and Editor rows (P4.5); renaming tokens.
Stop when the pixel diff is zero, the stale and guard tests are green with mutation proof, the targeted fixtures, the full
PlayMode run and the full EditMode suite are green. Ideas outside this domain -> proj_guides/__mixed_TODOs.md.
If the guide disagrees with the real code, STOP and report. Finish with the final report of `.clinerules/60-finishing.md` 6.4.
Do not commit: the Architect commits after the review.


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


