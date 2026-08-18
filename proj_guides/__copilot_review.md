
# `Git`

# ---

# The workflow alternates strictly between two operating modes:
- **PLAN MODE:** Analyzing requirements, auditing code, drafting options, and building execution roadmaps.
- **ACT MODE:** Modifying files, running builds, executing tests, and verifying behavior.

## **Mode Sync Protocol**
To prevent drift across context compaction, summarizations, or long task executions YOU SHOULD ALWAYS CONFIRM IN THE FILE`C:\Users\franc\Desktop\TileStories\proj_guides\__mode.md` the active mode and the current command that should be executed. 

THIS FILE IS UPDATED BY THE USER FREQUENTELY AND SHOULD ALWYAS BE THE GROUND TRUTH. SO YOU SHOULD ALSO FREQUENTLY READ THE __mode.md FILE AGAIN AND CONFIRM THE CURRENT MODE AND THE CURRENT COMMAND. DO NOT TRUST AMYTHIMG ELSE!!! CHECK YOUR MODE AND CURRENT COMMAND TO EXECUTE FREQUENTELY EVERYTIME SOMETHING CHANGE (CONTEXT COMPACT, NEW COMMAND, NEW INSTRUCTION FROM CLINE, ETC...).

# ---

# The MODE changed in the file:
 `C:\Users\franc\Desktop\TileStories\proj_guides\__mode.md`. Read the file again to confirm active mode and the current command that should be executed. 

THIS FILE IS UPDATED BY THE USER FREQUENTELY AND SHOULD ALWYAS BE THE GROUND TRUTH. IF IN ANY DOUBT DON'T TRUST AMYTHIMG ELSE. TRUST ONLY ON THIS FILE INSTRUCTIONS WHICH ARE UPDATED CONSTANTELY.


# ---

# `[CURRENT_MODE: PLAN_MODE]`
## [DISK_WRITE_ALLOWED: FALSE]
## [STATE_OVERRIDE: ALL PREVIOUS ACT_MODE AUTHORIZATIONS AND DISK-WRITE PERMISSIONS ARE REVOKED. YOU ARE STRICTLY FORBIDDEN FROM EDITING FILES, CREATING FILES, OR EXECUTING STATE-CHANGING DISK COMMANDS THIS TURN.]

## DIRECTIVE: RETROSPECTIVE AUDIT & PROSPECTIVE PLAN (PLAN MODE)

You are in PLAN MODE. Do NOT invoke any file-writing tools. Perform a critical review of the current status, audit past implementations, and prepare a granular blueprint for the next block.

## 1. **Guidelines & Architecture Re-Grounding** 
- Workspace Rules: C:\Users\franc\Desktop\TileStories\.clinerules
- Structure Map: C:\Users\franc\Desktop\TileStories\.clinerules\10-structure.md

## 2. **Target Domain & Context Re-Grounding**
- Target Domain Spec: `C:\Users\franc\Desktop\TileStories\proj_guides\_5.1_Editor_Tab.md`
- Reference Editor Code: C:\Users\franc\Desktop\TileStories\proj_guides\_5.1_Editor_Tab.md
- Current Plan Tracker: C:\Users\franc\Desktop\TileStories\proj_guides\__curr_plan_tracker.md`
 

## 3. **Deep Codebase Audit & Duplicate Prevention**
Inspect physical files on disk before planning. Do NOT trust text descriptions alone.
- Framework Path: C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework
- App Path: C:\Users\franc\Desktop\TileStories\TileStories\Assets\Apps\LivingRoom

*Audit Task:* Verify file paths and check for near-duplicate classes across folders. Ensure 10-structure.md accurately reflects physical disk layout before creating new tasks. (Sometimes in previous commands you said that some files were missing and not implemented and then we found them in some other place). 

## 4. **Online & Technical Research (If Needed)**
If framework architecture decisions require standard Unity patterns or research, use Tavily MCP to check Unity docs, research papers, or community solutions.


## 5. **Master Plan Formulation (__curr_plan_tracker.md)**
IF NOT DONE YET, draft the top-level phase breakdown for C:\Users\franc\Desktop\TileStories\proj_guides\__curr_plan_tracker.md.
- Keep the overall phase progression aligned with `C:\Users\franc\Desktop\TileStories\proj_guides\_5.1_Editor_Tab.md`.
- Reserve two dedicated blocks at the absolute end of the plan: "Vision Tests" and "Human in the Loop Tests".

## 6. **Retrospective Audit (Previous Block)**
- Verify which was the previous block to be implemented and confirm if all the code was well implemented against .clinerules.
- Confirm unit/play test pass rates via Unity MCP (if available).
- Flag any technical debt, missing edge-case handling, or unverified assumptions.
- If there are still gaps to do or correct in previous block, flag them, refine this block plan and don't plan next block. DO NOT PLAN NEXT BLOCK IF THE PREVIOUS BLOCK HAS PENDING TASKS TO BE DONE!!!

## 7. **Prospective Plan (Next Block Blueprint)**
Draft the granular TODO list for the next block to be written to __curr_plan_tracker.md.
For every task block, explicitly detail:
   - **WHAT:** Acceptance criteria and deliverables.
   - **HOW:** Design patterns, code quality rules (20-code-quality.md), de-risking code stubs, or exact package paths to import...
   - **WHERE:** Exact disk paths per 10-structure.md.
   - **WHY:** Architectural rationale for framework reusability.
   - **TESTS:** Language-agent tests (Edit/Play Mode). IF NEEDED Vision/Human tests, they should be referenced at the end of the file in the sections "Vision Tests" and "Human in the Loop Tests" AND a complete and detailed "handover guide" for the vision agent or the human should be writen in the files: 
   - - »» Vision Handovers: `...`
   - - »» Human Handovers: `...`
   - - »» These handover guides should have all important details like for agent or human to know what to do and check - expected runtime behaviors, exact test scenes to open, screenshots to inspect, target visual evaluation criteria, edge cases to verify, etc.
   - **FINISHING:** Checklist for updating __curr_plan_tracker.md and structure logs (60-finishing). 
If the file already add some detailed work plan for the current block, and we need to adjust or complete or correct, give me the clear sections to delete and the new text to replace, etc.
  - IMPORTANT: I WANT A FULLY DETAILED PLAN!! So see all the info we currently have in the plan, and give me all that well organized and well updated and well corrected, WITHOUT THE LOSS OF INFORMATION. I WANT A VERY COMPLETE AND DETAILED PLAN. 

## 8. **Clarifications & Open Questions**
If you discover architectural discrepancies, missing prerequisites, or competing implementation choices, list them clearly at the end of your response before we move to Act Mode. 

## 9. Present your full analysis report and proposed plan text in chat with all and full details. Do not edit disk files yet. I will copy myself this full plan from the chat to the __curr_plan_tracker.md file. Once I confirm and switch you to ACT MODE you can confirm the plan is written in the file, and then you can start implementation.


# ---

# `Questions`
## [CURRENT_MODE: PLAN_MODE]
## [DISK_WRITE_ALLOWED: FALSE]
## DECISIONS & PLAN REVISION
Here are the decisions for the questions you raised:

## ---

## »» Do you bst analysis. Just remember:
- That I want to implement a very complete and versatile Framework with all the good options and features that is good to have for this domain, and with clear "selection options of those features in the Editor Tab + clear variables and param config means" for the developer to select and adjust what he wants for its concrete App. 
- That I want the code well organized and simple and clean as possible, with all the WHAT/HOW/WHERE/WHY questions answered according to the .clinerules/ and to the domain main plan: `C:\Users\franc\Desktop\TileStories\proj_guides\_2.4_Marker_LOD.md`
And so do you best analysis, reason at two separate levels: 
1. **Architecture Level (Where it lives):** Map folder, assembly, and component scope. Evaluate 3 distinct structural options with trade-offs, state the choice, and explain why.
2. **Implementation Level (How it works):** Evaluate 3 concrete implementation choices (e.g., event-driven vs. direct call, ScriptableObject vs. hardcoded) and select the simplest, most robust option.
And then:
- Pick the best option, and we use this option as the default; 
- And then, if there are other options actually good in terms of feature or functionality for the the Framework, so lets allow for the developer to also choose the best option that he prefers for his app, and so we make available in the Editor Tab some drop down or toggle or table or other element for the developer to choose between the different available good quality options, and with the inheritent good variables and params to config that option. 


## --- 

## DIRECTIVE:
1. I already copied the full analysis report and proposed plan text that you gave me in chat to the __curr_plan_tracker.md file. Confirm the plan is correctly written in the file and well organized. IF we need to reorganize it, give me the clear sections to delete and the new text to replace, etc.    - IMPORTANT: I WANT A FULLY DETAILED PLAN!! So see all the info we currently have in the plan, and give me all that well organized and well updated and well corrected, WITHOUT THE LOSS OF INFORMATION. I WANT A VERY COMPLETE AND DETAILED PLAN. 


2. Do a deep analysis about the questions I just answered, and think the best decisions and implications to our proposed plan that is already in the __curr_plan_tracker.md file.

3. Give me the updates that I should do to the file:
   - Tell me the concrete section I should change (example section X.2);
   - Give me here in chat the revised plan for that section, with all the steps and full details;
   - I will then delete the current section and paste the new section guide that you give me here in chat.
   - IMPORTANT: I WANT A FULLY DETAILED PLAN!! So see all the info we currently have in the plan, and give me all that well organized and well updated and well corrected, WITHOUT THE LOSS OF INFORMATION. I WANT A VERY COMPLETE AND DETAILED PLAN. 

# --- `**!!!!! Mudar num curr block!!!**`
So lets implement `Block 5 and More TODO`.


# `[CURRENT_MODE: ACT_MODE]`

## [DISK_WRITE_ALLOWED: TRUE]
## [STATE_OVERRIDE: ALL PREVIOUS PLAN_MODE READ-ONLY RESTRICTIONS ARE REVOKED. YOU ARE EXPLICITLY AUTHORIZED AND REQUIRED TO WRITE AND MODIFY FILES ON DISK TO EXECUTE THE WORK PLAN.]

## DIRECTIVE: IMPLEMENT TASK & UPDATE WORK PLAN

You are now in ACT MODE. Proceed directly with disk edits and code implementation.

## 1. **Confirm Sync Work Plan First**
I copied your full analysis report and proposed plan text from the chat to the __curr_plan_tracker.md file. Confirm that it is there, and use it as the Ground Truth TODO list for you to follow and update during implementation. 

## 2. **Execute Block Tasks via 7-Step Protocol**
For each task or group of tasks in the active block, strictly execute this cycle:
1. **READ & ISOLATE:** Read active task specs in __curr_plan_tracker.md
2. **IMPLEMENT:** Modify only target files required for the active block.
3. **TEST:** Compile code and execute Edit/Play Mode unit tests.
4. **RE-READ WORK FILE:** Re-open __curr_plan_tracker.md from disk to ground context.
5. **GUIDELINE & TEST AUDIT:** Check code against .clinerules and confirm 0 regressions.
6. **PERSIST PROGRESS:** Update __curr_plan_tracker.md on disk (mark [x], log technical decisions).
7. **CHAT SUMMARY:** Output a concise chat report (files changed, compliance status, next task).

Continue for next task or group of tasks, until you complete this block/phase. 


## 3. **Tool Execution & File Editing Discipline - remember "50-terminal_and_tools.md"**
- **PowerShell Syntax:** Use semicolons (";"), redirect outputs to log files ("> __out.txt 2>&1"), never use "&&" or pipe directly through "findstr".
- Use editor with verbatim old_text (including leading whitespace and allway replacing complete lines); if matching fails or is ambiguous, fall back to unityMCP__apply_text_edits or python edit_file.py. Always verify immediately by re-reading the edited region, running refresh_unity, and passing all tests.

## 4. **Scope Lock & Execution Bounds**
- **Active Block Only:** Implement ONLY the specific tasks detailed in the active block/phase of __curr_plan_tracker.md.md.
- **No Invented Tasks:** Do NOT perform unsolicited cleanup, refactoring, formatting changes, or extra features outside what was explicitly specified and approved during Plan Mode.
- **Rule-Driven Refactoring Only:** If a guideline violation (.clinerules) directly blocks or breaks the active task, fix it adhering strictly to 10-structure.md and 20-code-quality.md. Do NOT refactor surrounding code "just because."

## 5. **Definition of Done & Hard Stop Rule**

### Definition of Done (DoD) Checklist
A block is ONLY complete when ALL of the following are true:
- 1. All task checkboxes ([x]) in the current block are completed.
- 2. Code compiles with zero "error CS" warnings/errors (refresh_unity).
- 3. Automated unit tests pass with zero failures (run_tests EditMode + PlayMode).
- 4. All scratch files (__edit.py, __out.txt, etc.) are deleted.
- 5. The __curr_plan_tracker.md file is updated on disk reflecting exact current status.

### MANDATORY HARD STOP
As soon as the Definition of Done is met for the active block:
- 1. **DO NOT** proceed to the next block or phase.
- 2. **DO NOT** invent new tasks, optimizations, or speculative cleanups.
- 3. Output the final Chat Summary (files modified, test results, and next scheduled block).
- 4. **HALT IMMEDIATELY AND STOP PROMPTING.** Wait for explicit user review and command to switch back to Plan Mode for the next block.

# ---