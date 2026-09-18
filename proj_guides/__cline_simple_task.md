
# `Git`
# `DELETE MODE && __CURR_PLAN_TRACKER!!!`
# `Unity MCP Connected??`


# TODO
» fazer label board in ui toolit pra mostrar automatico primeira vez que corremo app e dp usar pode escolher fechar e escolher nao voltar a motrar... 

# `Marker Deesign`
I want to verify everything we have done in the domain of the marker design system.

Start by reading these files fully and check the necessary and respective code files in the prooject to confirm how everything is implemented currently: 
- C:\Users\franc\Desktop\TileStories\proj_guides\_2.2_Marker_Design.md 
- C:\Users\franc\Desktop\TileStories\proj_guides\_5.1_Editor_Tab.md

then write in this file the normal user flow of actions in terms of this domain: 
C:\Users\franc\Desktop\TileStories\proj_guides\__curr_plan_tracker.md

example the user starts by creating a new POI, and what are the default fields that are created? just position or someething else? 
and then what are the fields that should be edited and in some specifci order or not really?? and how to save things, or undo, or redo...?? 

then tell me if there is any features in code that is not exposed in terms of the editor tab interface and maybe should be...?? so we have clear UI to tests "by hand" if everything works... and if you can actually call the unity mcp and confirm the feature is well exposed in terms of UI, ok, but if you can't confirm, and so for example we might have some edit button or some table colum but you can't really confirm 100% sure if that feature is well exposed, so tell me also all those features that are in doubt in terms of UI exposure. 

then tell me if there is any feature in the guide files that is still missing to be implemented. 

then add to the begining of the file a section "ORDERED TODO", basically a practical reasoning of your findings and your own analysis of what things should we do now? What is the order of things to do to finish this domain? should we implement some missing feature, should we correct or clean or refactor or run more tests to confirm some exiting feature, should we just start testing everything by hand basically following the normal "user flow of tasks"? you are the lead arhitect of this project, so what is the sequence of tasks that we should do now? Add that section to the begining of the file. 


write all this with all important details BUT IN A CONCISE MANNER. I WANT TO BE ABLE TO READ IT FAST. SO ALL IMPORTANT DETAILS BUT AS CONCISE AS POSSIBLE IN A NATURAL LANGUAGE LIKE A CODER GUY TALKING TO ANOTHER CODER GUY. 


# future task?? 
A *stable `key` + `label`* split for categories (like `badge_categories` already has) is argitchitecturally "cleaner" but is a __schema + every-consumer change__ (schema, CategoryPalette, search/filter/minimap/results, editor dropdowns, tests, config backfill) — big and risky for one wall today. The propagated-string approach keeps the current schema, fixes your exact failure, matches the existing POI-rename pattern, and is genuinely small. If a second wall later needs real display-name independence, the `key`/`label` migration can happen then (badge is the template


# `Change Task - No Backward compatibility - No dead code`
So do a deep analyis of all these reorganization ideas i said now. Confirm if they are good ideas and more simple and clear and easy to use and understand... AND confirm if we can implement them in an easy way with simple clean and wwell organized code... AND so confirm if we do it in a clean way and changing everything necessary in the files etc... because this is the first vrsion of the project, so lets not have backward compatibility concerns, for example trying to keep the plan described in the guide proj_guides\_2.1_Marker_Orientation.md... lets just think the best way to implement things in a clean way, and if needed we implement everything new and we change things as we want to make them clear and well organized... and then also make sure we don't leave any deade code from the previous plan...

so this is a big task... make a good plan before you act...



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

## `and now:`
- i was already implementing all the features of the "orientation domain" describeed in the file: 
C:\Users\franc\Desktop\TileStories\proj_guides\_2.1_Marker_Orientation.md

also when needed check the editor window guide for better context:
C:\Users\franc\Desktop\TileStories\proj_guides\_5.1_Editor_Tab.md

and so lets confirm and refine some things in the whole Orientation domain/component:

a) in the Test sub component, we have those 2 rows, each with "info" buttons.
but lets simplify things. 
lets leave the Edit Mode Preview row as is... and then bellow, instead of having a row with a "i" button, lets instead put that whole information text with the steps of how to test each field in the scene and/or play mode... so lets put that text directly in a big text field like if it was a normal row... so we can have it always visible in an easy way while we execute the test procedures... or if you think is better and easy... maybe we can even put it in a sort of detached window that we can  drag and place like any other window like the "hierarchy" or Console", and so this way we an have that "test guide" open at the same time we do other things... 

b) the "up reference" and the Facing Basis" rows have some too big ident... can you see in the image? coonfirm what is wrong and make those rows to be aligned left vertially with the other rows
AND, in fact, lets also change the other rows themselves... so lets move this whole "third level" identation a bit closer to the left side, so a narrower identation... make it for example just 1cm after the second level rows (Vertical Alignemnt, Facing Options, etc..). For you to see an example, see the specific marker tab, where we have already some good formating third level of rows, not too much idented forward. and so maybe use the same identation, and maybe just a bit less... 
AND, can we standardize this whole identation distances for second and third level rows? and so we use some sort of constant or something so we make it easy to configure and update and change... AND if it is a good idea, think the best way to implement it, and then update the file to reflect that guideline for future agents to know how to make good rows well formated right from the begining:
C:\Users\franc\Desktop\TileStories\proj_guides\_5.1_Editor_Tab.md
AND if we indeed create this sort of "global row identation system"... so maybe we can also include there the spacing we also put on the right side to avoid the scroll bar then covring up the right edge f the rows... formating system or row identation on the left 
AND so if we implemnt some better global system foor this whole row identation and spacing management... so update the 5_Editor file, AND also update all rows in the whole Editor Window in all components and sub components to use those measurements etc... 


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
