
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


## `And now: Lab round -- merge P1b.2 (one profile model, one profile bar, one Editor kit), proj_guides\_4.2_Lab_Merge_Plan.md`
GATE: Lab.

**The Lab:** `proj_guides/_4.2_TileStories_Lab/lab_src`.
- Read `PROTOTYPE_STRUCTURE.md`, `README.md`, `STATE_MAP.md` and `THEME_MAP.md` in full. **THEME_MAP §3 (the profile model) and §4 (the Editor kit) are this round's spec.**
- Their recommendations are accepted, with three changes from P1b.1:
  - the wall's picks live in `D.wall.theme`, not `D.theme.defaultMode`;
  - the App tab exists (`editor/app-tab.js`);
  - the visitor's mode is in `P.vis.mode` until this round moves it.

**Plan calls that apply:** `_4.2_Lab_Merge_Plan.md` §5, call 1 (Expert forces the Deep time) and call 7 (W's App design wins: the bar and the rows look like W's).

**Setup:**
- `npm install` is done. `python build.py` must show every check at 0, the theme ratchet included.
- No git writes: the Architect reviews and commits.

Domain spec, read ALL lines: `_4.2_Lab_Merge_Plan.md` (§1-3, §5), then the "P1b" row of its table. P1b.1 is done (`6a62c78`); this round finishes P1b.

REQUIRED READING:
- `proj_guides/skills/Feature_Prototyping.md` step 3 and step 5 (one home per feature, extension points, every feature in the Editor, each row names its demo);
- `.clinerules/00-process.md` Shared Core Rules (no shims, no aliases);
- `proj_guides/_5.1_Editor_Tab.md` section 0 (the Editor's look and rules);
- the frozen World Lab, read only (`proj_guides/_4.1.2_Prototype/lab_src`): `js/34_profiles.js`, `js/20_editor_kit.js`, its profile bar.

WORKER MODEL: Opus + effort High. Shared ground: the visitor's state and every Editor row.

SUBAGENTS: read-only audits only (Sonnet).

TEST PLAN:
- No opening baseline: P1b.1 closed green on CORE.
- **The Architect is running the FULL tier now**, on a `git worktree` of `6a62c78` at `LAB_PORT=8643`. Your server is 8642, so you do not wait for it. A red it finds will come to you as an item of this round.
- Shared ground, so the round closes on **CORE**: the self-test over every walk `bad=0`, then `node run_tests.js --core`. The new test files join the core set.
- One test file at a time, with a 10-minute limit. A timing check that fails once under load is rerun alone before it is called red.

WHAT (acceptance):
1. **One profile model** (THEME_MAP §3).
   - **Its owner:** `core/profile.js`.
   - **The visitor's fields:** `P.profile.{audience, time, look, mode, text, motion}`, plus `P.lang` as today.
   - **What it absorbs:** B's `P.vis.aud` / `len` / `mode` / `text` / `motion`, `P.reduce`, and B's audiences module. No alias is left.
   - **The derived values:**
     - the age band comes from the audience (Family → 7-11; Explorer / Expert → 12+);
     - **Expert forces `deep`**, and the Time control shows it locked, saying why;
     - `auto` look = the audience's look.
   - **The author's definitions** live in `D.app.audiences.<a>` and `D.app.times.<k>`. An audience picks named options (define, assign, place), never per-value overrides.
   - **Resolution:** base → look → mode → knobs → text step, once per `profileChanged`.
   - **Who decides the look:** the visitor's `P.profile.look` over the wall's `D.wall.theme.look`.
2. **Text size on every text:**
   - the steps are phone / S 0.9 / M 1 / L 1.2 / XL 1.36, precomputed;
   - B's paragraph-only steps are dropped;
   - nothing clips at XL in the phone matrix.
3. **One profile bar** (THEME_MAP §4):
   - Look · Mode · Audience · Time · Text · Motion · Language, in W's design;
   - it previews on the phone and scopes the design rows to the look and mode they save into;
   - the device and camera switches stay on the preview bar;
   - the visitor's Settings and Who's exploring write the same `P.profile` fields.
4. **One Editor kit** (THEME_MAP §4, "The one kit P1b builds"):
   - `editor/kit.js` owns the widget builders; `shell.js` keeps renderEditor, the tabs and `setOpt`;
   - W's good widgets are ported onto B's pattern;
   - W's checkbox and its per-POI override marking are dropped;
   - the bold + blue bar + ↺ mark stays only for a look over Premium;
   - the structure check counts a widget built outside `kit.js`, with 0 allowed.
5. **The App tab gains Profiles:**
   - Audience and Time definitions, with rows and demos;
   - "Where Who's exploring is asked" (`D.app.onboarding.askAudience`);
   - the author's motion level stays in Depth and motion.
6. **Forget** clears every `P.profile` field, and nothing in `D`.

LOOK:
- **The matrix:** the 3 audiences × Premium / Playful × light / dark × text M / XL, over the 4 devices.
- **The states:** the wall, Explore, the card at peek / half / full, Settings, the dock in a tour, and Who's exploring.
- **The Editor:** capture the App tab, the profile bar, and one fold per kit widget.
- **Judge** each cell as a designer, and compare the bar and the rows with W's in the same look and mode.
- **Fix** what is broken (clipped at XL, overlapping, unreadable). List what only needs design for P3.

TESTS (real input, each with `MUTATE=1`):
- the profile model's logic in Node (no browser): Expert forces deep, auto look, the age band, the resolution order;
- the bar by real clicks: each control changes the phone through `profileChanged` and the tokens;
- Settings and the bar write the same `P.profile`, and Forget clears it;
- XL text: nothing clipped or overlapping on the card and the dock (measured);
- the kit: each widget's row writes its path, its demo shows, and one Undo step reverts it.

DOCS:
- PROTOTYPE_STRUCTURE: the profile rule, the kit rule, and the new module rows.
- README: profiles, the bar, the kit, and the core set.
- STATE_MAP: `P.profile`, `D.app.audiences`, `D.app.times`, `D.app.onboarding`.
- THEME_MAP §3-4: mark each row done.

OUT OF SCOPE:
- components and the ratcheted colours (P3);
- the voice service and the guide's Settings group (P2);
- W's app screens and Settings' 5 areas (P4);
- landscape and tablet design.

Stop when CORE is green, the new tests and mutations pass, the captures are judged and the docs are updated.

Final report:
- STEP 1's plan (short) and the learning summaries;
- changes by area;
- test counts and the ratchet numbers;
- capture paths with verdicts;
- every visible difference, with its reason;
- anything open;
- one commit message.

## `And now (parallel, read only): Lab prep -- the design review, best of both Labs (prepares P3-P6), proj_guides\_4.2_Lab_Merge_Plan.md`
GATE: Lab, READ ONLY. It runs beside the P1b.2 builder (the section above, which edits `src/`). If the Architect's prompt names this section, it is your brief.

**The developer's goal:** the merged app's design is **premium, minimalist, immersive, 3D, "wow" and modern**, combining the best of the two prototypes. This review says how, screen by screen and part by part, with captures as proof. Nothing is built here: it feeds P3 (components), P4 (screens), P5 (media) and P6 (blocks), and the developer sees it before anything is built.

**Files:**
- Write `proj_guides/_4.2_TileStories_Lab/lab_src/DESIGN_REVIEW.md` (a new doc, version 0) and its images in `proj_guides/_4.2_TileStories_Lab/design_review/`, both new.
- Scratch goes in your folder.
- Never edit `src/`, tests, tools or other docs. Make no git writes.

**What to read first:**
- `_4.2_Lab_Merge_Plan.md` §1-5, above all calls 6, 7, 9 and 10. In short: W's App design wins; the dark is W's warm dark and navy; the rule decides sizes.
- `lab_src/THEME_MAP.md`, `lab_src/COMPONENT_MAP.md`.
- `proj_guides/skills/Feature_Prototyping.md` 3.2: the production paths and the UI Toolkit limits.
- The memory rule "Lab features must be Unity-buildable": no CSS-only effect; name each effect's Unity path.

**Captures, both Labs, real pixels.** Do not judge from code.
- **The App prototype (W, frozen):** `proj_guides/_4.1.2_Prototype/World_Editor_Lab.html`, through its `lab_src/tools/review/state_shots.py`. Usage is in its docstring; profiles are `prem:d:m` style. Take every state: onboarding, main, the filter, settings, you, the tapped card layouts, soon.
- **The TileStories Lab (B):**
  - Build a copy of `git archive HEAD` in your scratch folder (`npm install`, `python build.py`).
  - Capture it with its `tools/review/state_shots.py` / `section_shots.py` at `LAB_PORT=8644`. Ports 8642 and 8643 are in use.
  - Take the wall, Explore, the card at peek / half / full and scrolled through its sections and blocks, Settings, the dock in a tour, Who's exploring, and the guide's pages.
- **The profiles:** Premium / Playful × light / dark, text M and L, phone portrait. Add one tablet cell per screen.
- Make **side-by-side sheets** (W | B) for each pair of matching screens and parts.

WHAT THE REVIEW HOLDS (acceptance):
1. **Per screen and per part, a verdict with its image:** W better / B better / both have something.
   - Each verdict says what exactly is better, in design terms: hierarchy, one focal point, spacing, type, colour, depth, motion, density, touch targets and contrast.
   - The result is "the best of both" for that part. Never either / or: merge every good choice (memory: framework merges all good options).
2. **The "wow" layer:** concrete proposals for premium, minimal, immersive, 3D and modern.
   - The areas to cover: depth and layers, glass and light over the camera, Azu in 3D and where he lives, motion and transitions, the hero photo, the wall's 3D objects and the card's 3D block, sound and haptics cues.
   - Each proposal names where it appears and its production path:
     - Native;
     - Build, with its rough mobile cost;
     - Import;
     - Browser-only, which is rejected.
   - Respect the UI Toolkit limits: no blur, no gradients, one text-shadow, no 3D transforms, no z-index. The world content may use 3D, glow and gradients.
3. **The issues found:** every clipped, overlapping, low-contrast, off-token or inconsistent spot, each with its image and the round that should fix it (P3 / P4 / P5 / P6).
4. **The priorities:** the 10 changes with the most effect on "premium wow", each sized and placed in a round.
5. **Questions for the developer:** each with what it changes, in plain words, and a recommendation.

WORKER MODEL: Opus. SUBAGENTS: read-only (Sonnet) for the captures if useful.

**Final report:**
- the review's summary: the verdict counts and the top 10;
- the paths of the 6-8 most telling side-by-side sheets;
- the questions;
- anything that blocks.

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


