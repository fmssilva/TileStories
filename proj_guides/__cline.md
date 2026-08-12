# `Git`

#
#

# `[CURRENT_MODE: PLAN_MODE]`
## [DISK_WRITE_ALLOWED: FALSE]
## [STATE_OVERRIDE: ALL PREVIOUS ACT_MODE AUTHORIZATIONS AND DISK-WRITE PERMISSIONS ARE REVOKED. YOU ARE STRICTLY FORBIDDEN FROM EDITING FILES, CREATING FILES, OR EXECUTING STATE-CHANGING DISK COMMANDS THIS TURN.]

## DIRECTIVE: RETROSPECTIVE AUDIT & PROSPECTIVE PLAN (PLAN MODE)

You are in PLAN MODE. Do NOT invoke any file-writing tools. Perform a critical review of the current status, audit past implementations, and prepare a granular blueprint for the next block.

## 1. **Guidelines & Architecture Re-Grounding** 
- Workspace Rules: C:\Users\franc\Desktop\TileStories\.clinerules
- Structure Map: C:\Users\franc\Desktop\TileStories\.clinerules\10-structure.md

## 2. **Target Domain & Context Re-Grounding**
- Target Domain Spec: `C:\Users\franc\Desktop\TileStories\proj_guides\_2.4_Marker_LOD.md`
- Reference Editor Code: C:\Users\franc\Desktop\TileStories\proj_guides\_5.1_Editor_Tab.mds
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
- Keep the overall phase progression aligned with `_2.4_Marker_LOD`.
- Reserve two dedicated blocks at the absolute end of the plan: "Vision Tests" and "Human in the Loop Tests".

## 6. **Retrospective Audit (Previous Block)**
- Verify code written in the previous block against .clinerules.
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
   - - »» Vision Handovers: `C:\Users\franc\Desktop\TileStories\proj_guides\_2.4.1_Vision_Tests.md`
   - - »» Human Handovers: `C:\Users\franc\Desktop\TileStories\proj_guides\_2.4.2_Human_Tests.md`
   - - »» These handover guides should have all important details like for agent or human to know what to do and check - expected runtime behaviors, exact test scenes to open, screenshots to inspect, target visual evaluation criteria, edge cases to verify, etc.
   - **FINISHING:** Checklist for updating `__curr_plan_tracker.md` and structure logs (60-finishing). 

## 8. **Clarifications & Open Questions**
If you discover architectural discrepancies, missing prerequisites, or competing implementation choices, list them clearly at the end of your response before we move to Act Mode. 

## 9. Present your full analysis report and proposed plan text in chat. Do not edit disk files yet.

#
# 

# `Answer to Questions`
## [CURRENT_MODE: PLAN_MODE]
## [DISK_WRITE_ALLOWED: FALSE]

## DECISIONS & PLAN REVISION

Here are the decisions for the questions you raised:

...

## DIRECTIVE:
Revise the proposed plan in memory based on these decisions. 

Provide a quick summary of the updated plan block for `__curr_plan_tracker.md in chat so we can do a final confirmation. Do NOT write to disk yet. Once I confirm, I will switch you to ACT MODE to execute the edit and start implementation.

#
#

# `[CURRENT_MODE: ACT_MODE]`

## [DISK_WRITE_ALLOWED: TRUE]
## [STATE_OVERRIDE: ALL PREVIOUS PLAN_MODE READ-ONLY RESTRICTIONS ARE REVOKED. YOU ARE EXPLICITLY AUTHORIZED AND REQUIRED TO WRITE AND MODIFY FILES ON DISK TO EXECUTE THE WORK PLAN.]

## DIRECTIVE: IMPLEMENT TASK & UPDATE WORK PLAN

You are now in ACT MODE. Proceed directly with disk edits and code implementation.

## 1. **Sync Work Plan First**
Overwrite C:\Users\franc\Desktop\TileStories\proj_guides\__curr_plan_tracker.md with the detailed blueprint agreed upon in Plan Mode.
- **MANDATORY GATE:** Do NOT edit application C# code until __curr_plan_tracker.md is updated and confirmed saved on disk.

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
- **Small Anchored Edits:** Read the exact region first ("start_line"/"end_line"). Make small edits (<3000 chars) anchored to unique lines. Verify each edit before proceeding to the next.
- **Fail-Fast & Pivot:** If a tool fails, analyze output and pivot to an alternative tool or command immediately. Never repeat identical failing calls.


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

#
#