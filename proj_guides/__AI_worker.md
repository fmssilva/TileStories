
# `claude agent`
- **Two kinds of round.** A **Unity block** (the brief says `GATE: Unity`) follows this file as written. A **Lab round** (the brief
  says `GATE: Lab`) builds the HTML prototype: no Unity MCP; the Lab's own folder, `PROTOTYPE_STRUCTURE.md`, `README.md` and build
  replace the Unity paths, `10-structure.md` and the compile; `skills/Feature_Prototyping.md` step 3 is the cycle of 2.3. Lines below
  marked *(Unity)* apply to Unity blocks only.
- **Run by the Architect as a subagent:** the developer does not see your chat. Do not wait for answers or call AskUserQuestion: put
  the learning summaries, decisions and questions in your final report (STEP 1's plan included, short), and stop there.
- **GATE TASK** *(Unity)*: start by confirming UnityMCP server mcp is working in this claude chat. (don't confuse with a failed and different unity-mcp). check telemetry_status to confirm the good one if needed. If UnityMCP tools appear unavailable, don't assume they're unimplemented. STOP and tell the user what to check to confirm unity mcp works - check /mcp and reconnect...

- **CLAUDE.md:** I don't have it in this project. Instead i have this .clinerules folder with these files which are the general guidelines for this project:
  .clinerules\00-process.md
  .clinerules\10-structure.md
  .clinerules\20-code-quality.md
  .clinerules\30-ui-content.md
  .clinerules\40-testing.md
  .clinerules\50-terminal_and_tools.md
  .clinerules\60-finishing.md
So start by reading them all. 


## `And now: Lab round -- merge P1a.2 (router, devices, shot tools, first publish), proj_guides\_4.2_Lab_Merge_Plan.md`
GATE: Lab.
- The Lab: `proj_guides/_4.2_TileStories_Lab/lab_src`. Read `PROTOTYPE_STRUCTURE.md`, `README.md` and `STATE_MAP.md` in full.
- `npm install` is done; `python build.py` must show every check at 0.
- No git writes: the Architect reviews and commits.

Domain spec, read ALL lines: `proj_guides/_4.2_Lab_Merge_Plan.md` (§1-3, §5), then the "P1a.2" row of its table.

REQUIRED READING:
- `proj_guides/skills/Feature_Prototyping.md` step 3 (the round's cycle and the test tiers);
- `.clinerules/00-process.md` Shared Core Rules (no shims);
- `.clinerules/30-ui-content.md` ("every core feature must work without the camera").

Port FROM the frozen World Lab (`proj_guides/_4.1.2_Prototype/lab_src`, read only):
- `js/12_device.js`;
- its device buttons;
- `tools/review/state_shots.py`, `section_shots.py`, `inventory.py`.

WORKER MODEL: Opus + effort High. This round brings a new shared owner (the router) and rewires the phone shell.

SUBAGENTS: read-only audits only (Sonnet).

TEST PLAN:
- No opening baseline: P1a.1 closed green on CORE.
- The Architect runs the FULL tier on a `git worktree` at `LAB_PORT=8643`. **While the flag file `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\2bab6f8c-33fb-4cd2-9e78-7d5d95377279\scratchpad\FULL_RUNNING` exists, run no test, self-test or shot tool**; `build.py` is fine. Do the code, look and docs first.
- The round closes on CORE (self-test every walk `bad=0`, then `node run_tests.js --core`). Its new test files join the core set.

WHAT (acceptance):
1. **Events.** `configChanged` (D), `profileChanged` (P) and `screenChanged`, emitted once each where they happen, through `core/ext.js`. Existing listeners convert only when touched.
2. **The router** (`src/screen/`, Unity: the UI Toolkit screen stack):
   - a screen registry, `go` / `back`, an overlay stack, and action views registered by key;
   - every screen change goes through it;
   - the wall, the card sheet, Settings and the guide's overlays become its screens and overlays, looking exactly as today.
3. **The non-AR path.** An **Explore** screen without the camera: a list of the points that have a card → their card → a tour from the card works there.
   - It is reached from the wall and from a "No camera" preview switch.
   - It is built from the existing parts, not designed now.
4. **Devices.** W's four devices replace `SCREENS`:

   | Device | Size |
   |---|---|
   | phone portrait | 390 × 844 |
   | phone landscape | 844 × 390 |
   | tablet portrait | 820 × 1180 |
   | tablet landscape | 1180 × 820 |

   - W's device buttons; B's preview-bar extras kept; `ui.device` in STATE_MAP.
   - The card and the guide stay correct in all four. Layouts never designed for landscape or tablet are listed, not designed.
5. **Profile-matrix shot tools in this Lab.**
   - Rows: light / dark × text default / larger; looks join at P1b.
   - Columns: the 4 devices.
   - States: the wall, Explore, the card at peek / half / full, Settings, the dock in a tour.
   - Sections: every Editor fold with a demo.
6. **The first publish, prepared.**
   - The page becomes "TileStories Lab" (`tilestories-lab.html` + `assets/`); the old record copy stays as a frozen reference.
   - A publish list gives the files map and the total size: the page, `assets/`, the public voice packs (never `out-private`) and the music. Limits: 16 MB a file, 64 MB / 255 files a publish.
   - The page's relative URLs work both from the server and from the published layout, proven from a temp folder. The Architect publishes.

LOOK:
- Before and after captures of the wall, the card and the dock on phone portrait must be identical.
- Judge the new device cells and Explore, light and dark. Fix anything broken (clipped, overlapping, unreadable); list what only needs design (P3 / P4).

TESTS (real input, each with `MUTATE=1`):
- the router: order, back, overlays, one `screenChanged` per change;
- the non-AR path, end to end, until the guide speaks a line;
- each device's size and orientation, with the card's stops;
- the publish layout: the page, a GLB, a voice line and a music track load.

DOCS: PROTOTYPE_STRUCTURE (router rule, new modules), README (devices, Explore / No camera, shot tools, publish), STATE_MAP.

OUT OF SCOPE:
- tokens and looks (P1b);
- the voice service (P2);
- W's app screens (P4);
- designing landscape or tablet.

Stop when CORE is green, the new tests and mutations pass, the captures are judged and the publish list is ready. Final report: STEP 1's plan (short), the learning summaries, changes by area, test counts, capture paths with verdicts, the publish list and its size, anything open, and one commit message.

## `And now (parallel, read only): Lab round -- prepare merge P1b (the theme map), proj_guides\_4.2_Lab_Merge_Plan.md`
GATE: Lab, READ ONLY. It runs while the P1a.2 builder edits `src/`. If the Architect's prompt names this section, this is your brief; the section above is someone else's.
- **Write one file only:** `proj_guides/_4.2_TileStories_Lab/lab_src/THEME_MAP.md` (a new doc, version 0), plus anything in your scratch folder. Never edit `src/`, tests, tools or other docs. No build, no tests, no git writes.
- Note at its top the commit you audited (`git log -1`) and that P1a.2's uncommitted work was not included.

Domain spec, read ALL lines: `proj_guides/_4.2_Lab_Merge_Plan.md` (§1-3, §5), then the "P1b" row of its table. Read also `lab_src/STATE_MAP.md` (the same kind of map, for state) and `.clinerules/00-process.md` "Framework Completeness".

SOURCES:
- **Unity's theme:** `TileStories/Assets/Framework/Runtime/UI/Theme/Generated/Theme.default.uss` (the 167 `--ts-*` tokens). Find what generates it (the ThemeAsset and its generator) and name the source of truth; P1b's test reads that.
- **The merged Lab (B):** `lab_src/src/**` (CSS in the modules, `core/`, `card/block-looks.js`, `audiences/`, the dark modes, `editor/components.js` and `editor/shell.js`).
- **The frozen World Lab (W), read only:** `proj_guides/_4.1.2_Prototype/lab_src` (`dsT` / `applyDS`, `ds.css`, the Premium / Playful looks, light / dark, the profiles, its Editor widgets, `tests/unity_lint.py`).

WHAT THE MAP HOLDS (acceptance):
1. **Tokens.** One row per `--ts-*` token: its name, kind (colour, size, radius, space, type, motion, shadow), light / dark values where they exist; the B literals or variables that mean it (with counts and files); W's equivalent. Then:
   - B and W values with no `--ts-*` match, each with a verdict: map to an existing token, or a new token marked PROPOSED (with a name in the `--ts-*` scheme);
   - `--ts-*` tokens neither Lab uses.
2. **Looks and modes.** Which tokens change with Premium / Playful and with light / dark, from W. B's ten dark modes listed, what each changes, and how each becomes the one dark mode (merge every good choice, drop the bad with a one-line reason).
3. **The profile model.** B's audiences and W's profiles as one model: Family → 7-11, Explorer / Expert → 12+, Expert forces the Deep time; text size; reduce motion. For each field: its root (`D` or `P`, as STATE_MAP), its Unity home, its default, and the one profile bar that sets it in the Lab.
4. **The Editor kit.** Every Editor widget of B and of W (rows, foldouts, popups, tables, colour pickers, help): one inventory with a verdict per widget (same, merge, drop with reason) and the one kit P1b builds. Merge every good option, never either / or (memory: framework merges all good options).
5. **What P1b's builder must decide**, each with a recommendation; and a rough size of P1b in files touched.

Mark every guess "(guess)". WORKER MODEL: Opus. SUBAGENTS: read-only (Sonnet), for the big counts.

Final report: the map's summary (token counts matched / PROPOSED / unused; the dark modes' verdicts; the profile fields; the kit verdicts), the open decisions with recommendations, and anything that blocks P1b.

## `DO THIS IN 2 MAIN STEPS: PLAN AND ACT`
## `STEP 1 - PLAN`
Before you start executing and implementing things, perform a critical review of the project on disk, and prepare a granular blueprint for the given tasks to be done. For that do the following: 

### 1.1. Guidelines & Architecture Re-Grounding
- Workspace rules: read ./.clinerules/ (specifically 00-process.md, 10-structure.md, 20-code-quality.md, 30-design-system.md, 40-testing.md, 50-terminal_and_tools.md, 60-finishing.md)
- Structure map: read ./.clinerules/10-structure.md
- Skills: read proj_guides/skills/_Skills_Index.md; open a skill when the block's work matches its trigger.
»» read all the lines on all these files

### 1.2. Deep Codebase Audit & Gap Analysis
- Inspect physical files on disk that might be related with this task before planning. Do NOT trust past conversation summaries alone — a prior session's summary claiming something exists or is missing must be independently verified on disk. And sometimes in previous commands you said that some files were missing and not implemented and then we found them in some other place. So actually check the existing files: 
  - Framework Path: C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework
  - App Path: C:\Users\franc\Desktop\TileStories\TileStories\Assets\Apps\LivingRoom
- Identify missing details, typos, logical gaps, or misalignments with .clinerules.

*Read File:* for big files use `read_file` paginate with `start_line`/`end_line`__ (1-indexed) on the same path. for binary prefabs use the MCP `manage_prefabs get_hierarchy` to get live Unity-resolved structure

- *(Unity)* Confirm the project compiles via Unity MCP (if unity MCP is not available STOP immediately and report). *(Lab)* `python build.py` with its checks at 0. Do NOT run a baseline test suite at the start: the last commit is green (the Architect only commits green trees); run one only if the brief's TEST PLAN asks for it.

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
       - In terms of the tests themselves: use the reference fixtures the domain guide names (.clinerules/00-process.md "Real fixtures, one home"): for the card, the one reference card for single-card tests and the Phase A gallery for look x content matrices; for markers and the world, the LivingRoom POI sets (the Lamp 12+ POIs, the Painting, the Camera). Add a POI or block only if a case needs one.
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
2. **Implement Code:** Write or modify only the files strictly required for that step/task, where the structure map says it belongs (reuse first). Maintain clear alignment with .clinerules.
3. **Compile:** *(Unity)* refresh, zero `error CS`, the assemblies rebuilt (.clinerules/40-testing.md 4.2.5). *(Lab)* `python build.py`, every check at 0.
4. **Look and polish FIRST (before any PlayMode test):** for anything with a visible result (the card, a render, POI Editor rows / tables / popups), run it and take several captures (light and dark, the stops it lives at, over the hero and on paper, the motion as a short frame sequence) and judge them as a designer: modern, premium, minimalist, immersive, a "wow", motion that feels alive, nothing clipped, contrast right. Fix and capture again until it looks good (.clinerules/40-testing.md 4.5, "Look before you test"). Unity is open and unobstructed for this. Never report "not looked at".
5. **Implement and Run Tests:** only once the design is accepted -- EditMode for the pure rules, then the PlayMode tests that pin the details: real input as a visitor would do it, edge cases, motion and effects, contrast as seen (.clinerules/40-testing.md: the layers each test really needs, no boilerplate) - TESTS THAT ACTUALLY CONFIRM IF THINGS WORK OK FOR REAL, LIKE IF THE USER WAS CLICKING AND DOING THEM - NO MOCK TESTS. Then the block's tier.
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
When you complete all the Iterative Execution Cycles for each step/task in the plan AND WE ACHIEVE AND TESTED WITH REAL TEST THAT ALL FEATURES WORK, ALL CONFIG PARAMS WORK AND ALL TESTS PASS, so then **STOP immediately**. Output the final learning summary. *(Unity)* PUT the "LivingRoom Sene" to be ready to play in unity (not the test scenes tc...), and STOP with one `AskUserQuestion` (`60-finishing.md` 6.4) so the developer's alarm rings. *(Lab, run as a subagent)* stop with the final report. 


## `DURING THE WHOLE PROCESS: BE A TEACHER!!! - GIVE LEARNING SUMMARIES`
Act as a **Senior Staff Engineer** planning and implementing the whole plan, AND also, most important, as a **Teacher** explaining me all important details BUT IN A CONCISE AND NATURAL MANNER LIKE A CODER GUY EXPLAINING SOME DETAILS TO ANOTHER CODER GUY. So tell me **learnig summaries** along the whole process of planning and implementation and testing, of what we are doing and how and where, and maybe why if some nice decision was done. I want to learn how to do things without AI agents help, so teach me all important details in a concise manner (example things like syntax or workings of c# or unity and its workings,or framework used, or other details about cde implementations and also about tests, how we run tests etc.). So don't do some big report with formal tables as summaries!! Just tell me all important details in natural language and concise manner, like if it were clean, quick engineering notes I was taking in a class in order to study later so I can build and test this independently without AI. 

### AND SO, YOU NEED TO PRESENT ME A LEARNING SUMMARY ALONG THE WAY, EXAMPLE AFTER EACH FEATURE OR FILE IMPLEMENTATION, OR AFTER EACH TEST IMPLEMENTATION OR RUNNING OR AFTER EACH DEBUG SESSION, ETC. NOT JUST AT THE END OF CHAT. I WANT A LEARNING SUMMARY ALONG THE WAY SO I CAN FOLLOW YOUR ACTION CLOSELY!!!!


