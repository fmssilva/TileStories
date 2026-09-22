
# 1. `Git`
# 2. `DELETE MODE && __CURR_PLAN_TRACKER!!!`
# 3. `Unity MCP Connected??`
# 4. `npx adb-qr-connect`


# TODO
» fazer label board in ui toolit pra mostrar automatico primeira vez que corremo app e dp usar pode escolher fechar e escolher nao voltar a motrar... 




# future task?? 
A *stable `key` + `label`* split for categories (like `badge_categories` already has) is argitchitecturally "cleaner" but is a __schema + every-consumer change__ (schema, CategoryPalette, search/filter/minimap/results, editor dropdowns, tests, config backfill) — big and risky for one wall today. The propagated-string approach keeps the current schema, fixes your exact failure, matches the existing POI-rename pattern, and is genuinely small. If a second wall later needs real display-name independence, the `key`/`label` migration can happen then (badge is the template


# TODO - limpar o 5_Editor 

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

## `And now: Lets confirm the whole "Marker_Design" domain, which includes in the editor tab the marker, badge and outline components - and also the effects component but that is already revised i think`
I want to verify everything we have done in the domain of the marker design system.

a) Start by reading these files fully and check the necessary and respective code files in the prooject to confirm how everything is implemented currently: 
- C:\Users\franc\Desktop\TileStories\proj_guides\_2.2_Marker_Design_Archive.md
- C:\Users\franc\Desktop\TileStories\proj_guides\_2.2_Marker_Design.md
- C:\Users\franc\Desktop\TileStories\proj_guides\_2.2_notes.md
- C:\Users\franc\Desktop\TileStories\proj_guides\_5.1_Editor_Tab.md

b) then tell me the normal sequence of user flow of actions in this domain. Example: 
b.1) The user starts by creating a new POI, and what are the default fields that are created and with which values? maybe we can "copy the values of the "original POI that we copied", similar to how we do with the position and orientation values?

b.2) Is there some feature or functionality of this domain that we have in code and not exposed in a UI in the editor tab that maybe should have, because the developer should be able to enable/disable or config, considering we are implementing a framework to allow the creation of apps for different scenarios? 
b.3) Is there any feature or functionality in the guide files that is not implemented yet and should be? 

b.4) Take the proj_guides\_5.1_Editor_Tab.md file. Read the section ## HOW TO USE THIS FILE (read this first), and then use the section ## 0. Guidelines as a checklist to evaluate the whole quality of these domains, are all the rows well formated, with the good width, indentation, elements and components, helpers, etc... the save, undo, redo things are all working corrected for all fields... we have the correct warnings and pop ups if needed... what is a good sequence of "editing" and config of all those fields, do we have a good "UI Ontology and hierarchy in the editor tab to be evrything easy and well organized and intuitive to use by the developer? What are the lowest level we can test each config? (Scene, PlayMode, Device)... Do we have already implemented an updated "Domain Manual Tests" sub component to guide the developer on how to config and test things? Do we have already "live sync between the editor tab values of the fields and the scene mode preview? For the PlayMode tests do we have the "playmode live sync between the editor tab and the playmode view also? Make a deep analysis of evrything we have and how and where we have it. is it all good or can we make it better, implement what is missing, improve some feature, make the code simpler or better organized... ?? 
 
b.5) In terms of code, any dead or duplicated or bad organized code that we should improve according to the .clinerules? 

b.6) Do we have good set of tests to confirm that each field configuration actually works well and has the expected results in the markers, in the scene and playmode? Lets confirm we have good REAL tests, and also take some screen shots to confirm visually if things are working well. 

b.7) any other detail we should be aware of for this domain or think about? 

c) If there is a lot of things to do, before you start implementing things, make a deep and complete and detailed plan to make sure things get clean and well organized and working well and well tested... keep in mind that this is the first version of project so lets just make things better and organized and clean - no backward compatibility concerns and weird code just to avoid some bigger change... let just change what is necessary to make it all better and simple and clean and well organized, so it gets easier to understand and to maintain in the future with less "coupling" and weird dependencies as possible... lets keep things well organized and easy to change and improve... 

d) after we do and implement and clean and test everything, lets update, if necessary the files: proj_guides\_5.1_Editor_Tab.md and .clinerules\10-structure.md, and also lets write a clean set of guide files: 
C:\Users\franc\Desktop\TileStories\proj_guides\_2.2.1_Marker_Design.md
C:\Users\franc\Desktop\TileStories\proj_guides\_2.2.2_Badge_Design.md
C:\Users\franc\Desktop\TileStories\proj_guides\_2.2.3_Outline_Design.md
»» so lets confirm we have well implemented all the good features described in the files:
proj_guides\_2.2_Marker_Design_Archive.md
proj_guides\_2.2_Marker_Design.md
proj_guides\_2.2_notes.md
and so then we can consolidate these files into the well orgaized files
proj_guides\_2.2.1_Marker_Design.md
proj_guides\_2.2.2_Badge_Design.md
proj_guides\_2.2.3_Outline_Design.md
proj_guides\_2.2.4_Markers_Effects.md
keeping there all important details, including some bibliographic references we might havee used or other impoortant details that might be usefull for when i write my master thesis report about this project. 


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
- Follow the implementation plan we just did. Also check if I copied it to the file "./proj_guides/__curr_plan_tracker.md" and so you can use that file as a clear TODO list so you can keep track of progress. 

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
6. **Learning Summary** present a "Learning Summary" for these steps we just did in chat, for me to keep track of what is beeing done, with all important details BUT in a concise manner, like i explained above. 

Repeat steps 1–6 until every step/group of sub-tasks in the plan is done. 

### 2.4. Terminal & Tool Discipline
Per ./.clinerules/50-terminal_and_tools.md:
- Chain commands with `;`, not `&&`.
- Use editor with verbatim old_text (including leading whitespace and allway replacing complete lines); if matching fails or is ambiguous, fall back to unityMCP__apply_text_edits or python edit_file.py. Always verify immediately by re-reading the edited region, running refresh_unity, and passing all tests.

### 2.5. Hard Stop
When you complete all the Iterative Execution Cycles for each step/task in the plan AND WE ACHIEVE AND TESTED WITH REAL TEST THAT ALL FEATURES WORK, ALL CONFIG PARAMS WORK AND ALL TESTS PASS, so then **STOP immediately**. Output the final learning summary and STOP. 


## `DURING THE WHOLE PROCESS: BE A TEACHER!!! - GIVE LEARNING SUMMARIES`
Act as a **Senior Staff Engineer** planning and implementing the whole plan, AND also, most important, as a **Teacher** explaining me all important details BUT IN A CONCISE AND NATURAL MANNER LIKE A CODER GUY EXPLAINING SOME DETAILS TO ANOTHER CODER GUY. So tell me **learnig summaries** along the whole process of planning and implementation and testing, of what we are doing and how and where, and maybe why if some nice decision was done. I want to learn how to do things without AI agents help, so teach me all important details in a concise manner (example things like syntax or workings of c# or unity and its workings,or framework used, or other details about cde implementations and also about tests, how we run tests etc.). So don't do some big report with formal tables as summaries!! Just tell me all important details in natural language and concise manner, like if it were clean, quick engineering notes I was taking in a class in order to study later so I can build and test this independently without AI. 

### AND SO, YOU NEED TO PRESENT ME A LEARNING SUMMARY ALONG THE WAY, EXAMPLE AFTER EACH FEATURE OR FILE IMPLEMENTATION, OR AFTER EACH TEST IMPLEMENTATION OR RUNNING OR AFTER EACH DEBUG SESSION, ETC. NOT JUST AT THE END OF CHAT. I WANT A LEARNING SUMMARY ALONG THE WAY SO I CAN FOLLOW YOUR ACTION CLOSELY!!!!


# ---

# `Domain Tasks Change - No Backward compatibility - No dead code`
So do a deep analyis of all these reorganization ideas i said now. Confirm if they are good ideas and more simple and clear and easy to use and understand... AND confirm if we can implement them in an easy way with simple clean and wwell organized code... AND so confirm if we do it in a clean way and changing everything necessary in the files etc... because this is the first vrsion of the project, so lets not have backward compatibility concerns, for example trying to keep the plan described in the guide proj_guides\_2.1_Marker_Orientation.md... lets just think the best way to implement things in a clean way, and if needed we implement everything new and we change things as we want to make them clear and well organized... and then also make sure we don't leave any deade code from the previous plan...

so this is a big task... make a good plan before you act...

