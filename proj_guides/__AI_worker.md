
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


## `And now: POI Card Navigation (3.2) -- step 1, the ontology as data and Block Order`

GATE: `git push` if the branch is ahead of origin (3 commits are waiting; report how many you pushed). Take one Unity capture
(Game view AND the POI Editor window) and open it; if none works, STOP and say so. Confirm the tree compiles; if not, STOP and
report. Baseline: the code tree is identical to the last green commit 3599632 (the two commits since are doc-only), so run the
FULL EditMode suite only (~95 s) and trust 3599632 for PlayMode -- say in the report which commit you trusted.

Domain spec (read ALL lines): proj_guides/_3.2_POI_Card_Navigation.md -- start with "## TODOs" ([step 1] lines) and the status
table (nothing done yet; step 1 is yours, steps 2-19 are planned). Sections 1, 2.2, 2.3, 3 (the two rows marked step 1), 4.6,
7 and 9 are the step.
REQUIRED READING (all lines): proj_guides/_5.1_Editor_Tab.md "HOW TO USE THIS FILE" + section "0. Guidelines" (you add a new
Detail Card section, a table with reorder arrows and its captures); .clinerules/40-testing.md 4.2.3 + 4.5 (Editor tests with
real clicks, capture checklists) and 4.2.4b (assert relative order, not indices); proj_guides/_3.1_POI_Card_Blocks.md
section 5 (the `container.sources_at_end` row you delete) and section 8.1 (how the Detail Card tab is built).
Context only: _3.1.1_Audit.md section 12, .clinerules/10-structure.md (Blocks + POIEditor trees), _3.3_POI_Card_Styles.md.
WORKER MODEL: Opus-class -- it changes the block registry every card stands on, removes a config field across 6 files, and
adds a new Editor section with a reorderable table.
TEST PLAN: no full PlayMode baseline (tree code-identical to green 3599632); ONE full EditMode + full PlayMode run at the END
of the block. Background full run: NOT required this block (it is the 2nd since the last one) -- the next block owes it.
`BlockRegistry` and `BlockStackBuilder` are shared ground, so the closing full run is mandatory, not optional.

PART 1 -- fixes: none. `_3.1` closed with no open fix-list item.

PART 2 -- step 1, in this order (commit-sized sub-steps):

1.1 Families as data.
  WHAT: a family is a registered definition, not a string literal. `FamilyDefinition` and `BuiltInFamilies` in
  Runtime/Blocks/Navigation/; `BlockRegistry.RegisterFamily(def)`, `Families`, `TryGetFamily`. Fields THIS step:
  `Key`, `Order`, `LabelKey`, `InNav`, `AtEnd` -- NOT `IconKey` / `ColorToken` (no family icons in `CardIcons` and no
  `--ts-family-*` tokens yet; they land at step 6). The 9 families of `_3.2` section 2.2 with their Order gaps of 10;
  `navigation` is NOT registered here (step 11 brings it with its first kind).
  HOW: `BuiltInBlocks.Register` registers the families BEFORE the kinds. The instance `Register(definition, factory)`
  throws when the kind's family is not registered -- the static `Validate(definition)` keeps its exact signature and its
  shape checks (8 call sites depend on it). `Ordered` sorts by family `Order` (authored kind order inside a family), so an
  app family with Order 35 really lands between `guides` and `stories`. `ContentSeenRule.MetaFamily` and the `Family = "..."`
  literals now read the `BuiltInFamilies` constants -- one home per name.
  The 8 sites that build a bare `new BlockRegistry()` must register families; fix the tests, never weaken the check.
  TESTS: `FamilyRegistryTests` (register; a duplicate key refused; a kind with an unregistered family refused with a message
  that names the family; `Ordered` order; an app family in a gap). `CardStringsTests` covers the 9 new label keys.
  The labels go in `CardStrings.asset` in en + pt WITH their accents (data, `20-code-quality` 2.1 exception); `guides` reads
  "Listen / Ouvir" and its key stays `guides`.

1.2 Block Order replaces Sources At The End.
  WHAT: `CardNavigationSettings` (`CardSettings.navigation`) with `block_order` (default `end_families_last`) and a
  `families` row list {`family`, `order`, `at_end`} -- one row per family the wall changed, nothing else (`in_nav` and
  `group` land at steps 5/6 with the code that reads them). `CardOptions.BlockOrders` + labels.
  HOW: `BlockStackBuilder.Build` applies the three options AFTER the skip rules, header always first (`_3.2` 2.3). The
  effective order / At The End of a family = the registered default, overridden by the wall's row. `container.sources_at_end`
  and its Editor row and help are DELETED -- no shim, no migration key (`00-process`). Confirm by grep that the LivingRoom
  config never wrote it, so no config file changes.
  TESTS: `BlockStackBuilderTests` -- `authored` / `end_families_last` / `by_family` on a fixture whose `meta` block sits in
  the middle, plus a wall row that puts At The End on a second family; `by_family` asserts each family is ONE contiguous run
  (relative order, no hard indices). `CardConfigRoundTripTests` walks the new leaves.

1.3 Editor: Detail Card > Navigation.
  WHAT: a new framed foldout "Navigation", placed AFTER Card Container and before Block Library, with its own section colour.
  Rows this step: **Block Order** (popup, (i)) and the **Families table** -- Order with up/down reorder arrows, Family (the
  visitor label plus its key; name things, never bare ids), At The End (toggle), (i). Then the section's Test sub-foldout
  (Scene / Play Mode / Device guide, app-agnostic wording -- it must not name The Lamp or the LivingRoom wall). "Preview
  navigation" is step 17: do not add it.
  HOW: new partial `Editor/POIEditor/DetailCard/POIEditorToolWindow.CardNavigation.cs`; its (i) texts and guides go in a NEW
  `POIEditorToolWindow.CardNavigationHelp.cs` (DetailCardHelp.cs is already 494 lines). Reuse `DrawPopupField`,
  `TableRowScope`, `DrawConfigMutationScope` and the POI reorder arrows of `POIEditorToolWindow.SpecificMarker.cs:199-207`
  (`ReorderPoi`) -- do not write a second reorder helper. Undo / redo / save and live Play Mode sync like every other row.
  TESTS: `DetailCardEditorTabTests` -- real clicks on the Block Order popup, on a reorder arrow and on At The End, Ctrl+Z
  restores each, save writes them; the `sources_at_end` cases there move to Block Order. `LiveSyncFieldMatrixTests` and
  `PoiCardLiveUpdateTests.SourcesAtTheEnd_...` move to Block Order (a live switch to `by_family` reorders the open card).

1.4 Phase B on the real wall: in LivingRoomScene, The Lamp's card renders in each of the three orders -- `by_family` puts
  every family in one contiguous run and ends with Sources; `authored` keeps the authored order; the default behaves exactly
  as Sources At The End did. Assert relative order of real block keys.

1.5 VISUALS: the POI Editor at 620 pt showing the open Navigation section (follow the existing `Editor/1xx_*_620.png`
  naming), and The Lamp's card at 390 px under the default and under `by_family`. Open every capture and answer an
  item-by-item checklist (rows aligned, (i) column, arrows at full tap size, nothing clipped, the table readable).

1.6 DOCS: `_3.2` status row 1 -> done + proof; `_3.1` section 5 drops the `container.sources_at_end` row and points at
  `_3.2` Block Order; `_3.1.1_Audit.md` section 12's matrix row for it gets a "replaced by Block Order (`_3.2` step 1)" note;
  `_5.1_Editor_Tab.md` -- remove Sources At The End from the Card Container row list (file inventory) and from its row detail,
  and document the new Navigation section in both places; `10-structure.md` for every new file; the RUN TIMES line of
  `40-testing.md` 4.2 after the full run.

Rules as before: no literal colours, sizes or visitor strings in C# (tokens + `CardStrings`); strict ASCII in code, comments,
logs and Editor labels (Portuguese accents only inside the data assets); Editor warnings and popups name things, never ids;
no backward-compatibility shim for the removed field; real fixtures only -- never change a POI's identity fields.
OUT OF SCOPE: `CardNavModel`, `NavOntologyRule`, the Format facet (step 2); any nav element, slot or layout; `group_by`,
`min_blocks`, `max_slots`, `labels` and the other rows of section 3 that belong to steps 5-7; the `navigation` family and nav
kinds (step 11); family icons and colour tokens (step 6 / `_3.3`).
Stop when the full EditMode and PlayMode suites are green with zero failures, the captures are judged, and 1.6 is written.
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


