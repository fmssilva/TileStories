
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

