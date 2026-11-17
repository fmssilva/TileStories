
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


## `And now: Lab round -- merge P1b.1 (the theme on Unity's tokens: the table, its test, the looks, one dark mode), proj_guides\_4.2_Lab_Merge_Plan.md`
GATE: Lab.
- The Lab: `proj_guides/_4.2_TileStories_Lab/lab_src`. Read `PROTOTYPE_STRUCTURE.md`, `README.md`, `STATE_MAP.md` and **`THEME_MAP.md` in full**: the map is this round's spec. Its §5 recommendations are accepted as written; §1.1 (Unity's values win) is the developer's call (plan §5, call 6).
- `npm install` is done; `python build.py` must show every check at 0.
- No git writes: the Architect reviews and commits.

Domain spec, read ALL lines: `proj_guides/_4.2_Lab_Merge_Plan.md` (§1-3, §5), then the "P1b" row of its table. P1b is split in two rounds: this one (P1b.1) and P1b.2 (the profile model, the one bar, the Editor kit).

REQUIRED READING:
- `proj_guides/skills/Feature_Prototyping.md` step 3 (the round's cycle and the test tiers), step 5 (one home per feature, no wrappers);
- `.clinerules/00-process.md` Shared Core Rules (no shims, no aliases);
- `.clinerules/30-ui-content.md` (sizes, contrast, the 13 pt minimum);
- Unity: `TileStories/Assets/Framework/Runtime/UI/Theme/` (`ThemeAsset.cs`, `ThemeController`, `Generated/Theme.default.uss`), `Framework/Runtime/Resources/TileStories/Themes/Theme.default.asset`, `Framework/Editor/Theme/ThemeUssGenerator.cs`, and the theme tests in `Framework/Tests/Editor/UI/Theme/` (read only).

WORKER MODEL: Opus + effort High. Shared ground: every module's colours.

SUBAGENTS: read-only audits only (Sonnet).

TEST PLAN:
- No opening baseline: P1a.2 closed green on CORE (`4bd31ba`), and the FULL at `78ea86f` is reviewed.
- Shared ground, so the round closes on **CORE**: the self-test over every walk `bad=0`, then `node run_tests.js --core`. The round's new test files join the core set.
- One test file at a time; each has a 10-minute limit.

WHAT (acceptance):
1. **The token table.**
   - `D.theme` in the ThemeAsset's shape (`defaultMode`, `shared`, `modes`, `knobs`, `type`). Its Unity home is in STATE_MAP. PROPOSED tokens are flagged `proposed: true`.
   - One owner, `core/theme.js`. `applyTokens` writes the resolved table as class rules in one style element. It is regenerated on `configChanged`, so a mode or look switch swaps one class, as `ThemeController.SetMode` does.
2. **The token test** (Node, no browser; it joins core):
   - every non-proposed token equals Unity's, in light and dark;
   - the 4 accents equal the asset's `knobs:`;
   - every PROPOSED token is absent from the `.uss`, so the test says when Unity adds one;
   - contrast is composed in linear light: body 7:1, other text 4.5:1.
3. **The looks** are knob options on the table. Premium is the base; Playful is a `ThemeKnobOption` (map §5.3), with its own accent as a fifth measured accent option. The other knobs the looks change are knob options too: roundness, density, glass and motion level. The text step is P1b.2.
4. **One dark mode.**
   - There is one resolver and one class. The per-module dark settings go.
   - The card's table (`--paper`, `--ink1`, `--ink2`, `--hair`, `--box`, `--accent2`, the section colours, `--bar`) is renamed straight to `var(--ts-…)`, with no alias layer.
   - B's ten dark modes follow the map's verdicts (§2).
   - The dock, beat, skin and chip colours keep their values for now under a **ratchet** in `build.py` that counts colour literals outside the theme; it only goes down (ported in P3).
   - The phone font sizes this round touches come from the type tokens. A ratchet counts the phone sizes still under 13 pt.
5. **The Editor:** every theme choice the author makes has its row with a demo, in the section the map names:
   - the wall's default mode;
   - its accent (swatch rows, a closed list);
   - its look;
   - the knobs.

   Undo works on them. Free colours stay developer-only rows with a live contrast badge (map §4).

LOOK:
- The matrix is now Premium / Playful × light / dark × text default / L, over the 4 devices.
- The states are the wall, Explore, the card at peek / half / full, Settings and the dock in a tour, plus the Editor's theme section.
- Before / after: this round changes what the visitor sees (Unity's values, sizes up to 13 pt). List each visible difference with its reason; anything else that differs is a bug.
- Fix what is broken (clipped, overlapping, unreadable, under contrast). List what only needs design for P3.

TESTS (real input, each with `MUTATE=1`):
- the token test;
- switching mode and look by real clicks changes the phone through one class, and no module keeps a colour of its own for it;
- an Editor accent or look change reaches the phone, and Undo returns it.

DOCS:
- PROTOTYPE_STRUCTURE: the theme rule (no literal colour, size, radius or duration in changed code; a missing token goes to the table, flagged PROPOSED) and the new module rows.
- README: the themes, the token test, and that the Lab blends in sRGB while the test measures in linear light.
- STATE_MAP: `D.theme`.
- THEME_MAP: mark each row done or left for P3.

OUT OF SCOPE:
- the profile model, the one bar and the Editor kit (P1b.2);
- the voice service (P2);
- components and the ratcheted colours (P3);
- landscape and tablet design (P4).

Stop when CORE is green, the new tests and mutations pass, the captures are judged and the docs are updated. Final report: STEP 1's plan (short), the learning summaries, changes by area, test counts and ratchet numbers, capture paths with verdicts and the list of visible differences, anything open, and one commit message.

## `And now (parallel, read only): Lab prep -- the component map (P3) and the voice map (P2), proj_guides\_4.2_Lab_Merge_Plan.md`
GATE: Lab, READ ONLY. Two prep briefs, **A** and **B**, run beside the P1b.1 builder (the section above, which edits `src/`). The Architect's prompt names your letter; do only that one.
- **Write one file only:** A writes `proj_guides/_4.2_TileStories_Lab/lab_src/COMPONENT_MAP.md`, B writes `.../lab_src/VOICE_MAP.md` (new docs, version 0). You may also write anything in your scratch folder.
- Never edit `src/`, tests, tools, voicebench or other docs. Do not run the build, tests or voicebench renders, and make no git writes.
- Read B's code from `git archive HEAD` into your scratch folder, not from the working tree, which the P1b.1 builder is changing. Note the commit at the top of your map.
- Read first: `_4.2_Lab_Merge_Plan.md` §1-5 (§5 calls 6-7: Unity's values win; **W's App design wins**), `lab_src/STATE_MAP.md`, `lab_src/THEME_MAP.md`, and `.clinerules/00-process.md` "Framework Completeness".
- Mark every guess "(guess)". WORKER MODEL: Opus. SUBAGENTS: read-only (Sonnet), for the big counts.
- Final report: the map's summary, the open decisions (each with a recommendation), the size of the round it prepares, and anything that blocks it.

### A · The component map (prepares P3)
1. **Inventory.** List every visual component family the visitor sees.
   - In B: the card's chrome, sheets, chips (about 18), buttons (about 7), bubbles (about 5), badges, toasts, the dock, the guide's panels, the beats, the tiles and the blocks' shared parts.
   - In W (`proj_guides/_4.1.2_Prototype/lab_src`, frozen): its App components.
   - For each family give its files, its variants and their counts.
2. **Verdicts.** Each B variant maps to one W component with options (call 7: W's design wins). Merge every good option; drop the rest with a one-line reason.
   - Where W has no component for a role, keep B's, marked "redrawn on the tokens".
   - Missing options become PROPOSED.
3. **Unity.** Give each component its UI Toolkit home: the existing USS classes and VisualElements under `TileStories/Assets/Framework/Runtime/UI/`, the 36 BlockViews (read only), or "new". Note what UI Toolkit cannot do (`Feature_Prototyping.md` 3.2: no blur, no gradients, one text-shadow, no z-index).
4. **The reach test.** List every visible part the P3 reach test must cover, and the profile options each one must follow: look, mode, text size, audience and motion.
5. **The order of work.** Propose P3's order (the components first, then the card's chrome, then the dock and panels) and a size in files.

### B · The voice map (prepares P2)
1. **Every speech path.** List every place the app speaks or plays a voice, with its file and line: lines, tour steps, tips, games, pages, Tell me more, answers, resume, speed, back-to-game and the fallbacks.
   - For each, say how it picks the voice today and which of the plan's §4 causes it shows.
2. **The one service.** Propose `guideVoiceFor(ctx)` and the voice service's API: its owner module, the D / P fields it reads (STATE_MAP), its events, and the order of fallbacks, each one shown in the Editor at that moment.
   - Name the Unity home (the guide-config asset and the visitor's settings).
3. **Coverage.** Compare each voice set in `voicebench/out` with every line the app can say: `voicebench/script.json`, plus the words the code composes.
   - Give per set: lines covered of the total, and the missing lines with their owner and language.
   - Give the composed lines and how each should speak: recorded parts, or live.
   - Count the characters needed to render Sonia / Raquel in full, and estimate the share of Azure F0's 0.5 M a month.
4. **Settings.** List every Editor and visitor control that touches the voice today, with its root. Map them onto plan §5.4: the Guide tab (`D`), App tab › Settings (which choices visitors see), and the visitor's Settings › The guide (`P`).
5. **The order of work.** Propose P2's order and a size in files.

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


