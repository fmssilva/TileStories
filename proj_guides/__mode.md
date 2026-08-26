
# `[CURRENT_MODE: PLAN_MODE]`
## [DISK_WRITE_ALLOWED: FALSE]
## [STATE_OVERRIDE: ALL PREVIOUS ACT_MODE AUTHORIZATIONS AND DISK-WRITE PERMISSIONS ARE REVOKED. YOU ARE STRICTLY FORBIDDEN FROM EDITING FILES, CREATING FILES, OR EXECUTING STATE-CHANGING DISK COMMANDS THIS TURN.]

## DIRECTIVE: RETROSPECTIVE AUDIT & PROSPECTIVE PLAN (PLAN MODE)

You are in PLAN MODE. Do NOT invoke any file-writing tools. Perform a critical review of the current status, audit past implementations, and prepare a granular blueprint for the next block.

## 1. **Guidelines & Architecture Re-Grounding** 
- Workspace Rules: C:\Users\franc\Desktop\TileStories\.clinerules
- Structure Map: C:\Users\franc\Desktop\TileStories\.clinerules\10-structure.md

## 2. **Target Domain & Context Re-Grounding**
- Target Domain Spec: `C:\Users\franc\Desktop\TileStories\proj_guides\_2.7_Corrections_TODO.md` »» This is a set of corrections and updated we need to do to previous domains guides. 
- Read only the referenced guide in these corrections TOOD tasks, don't just read them all by default beause they are big. Read the guide referenced in detail and the related files in code and not all of these guides: 
  - C:\Users\franc\Desktop\TileStories\proj_guides\_1.2__Marker_Positioning_future_notes.md
  - C:\Users\franc\Desktop\TileStories\proj_guides\_2.3_Marker_Hierarchy.md
  - C:\Users\franc\Desktop\TileStories\proj_guides\_2.4_Marker_LOD.md
  - C:\Users\franc\Desktop\TileStories\proj_guides\_2.5_Marker_Displacement.md
  - C:\Users\franc\Desktop\TileStories\proj_guides\_2.6_Select_Filter_Search.md
- Reference Editor Code: C:\Users\franc\Desktop\TileStories\proj_guides\_5.1_Editor_Tab.md
- Current Plan Tracker: C:\Users\franc\Desktop\TileStories\proj_guides\__curr_plan_tracker.md
 
## 3. **Deep Codebase Audit & Duplicate Prevention**
Inspect physical files on disk before planning. Do NOT trust text descriptions alone.
- Framework Path: C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework
- App Path: C:\Users\franc\Desktop\TileStories\TileStories\Assets\Apps\LivingRoom

*Audit Task:* Verify file paths and check for near-duplicate classes across folders. Ensure 10-structure.md accurately reflects physical disk layout before creating new tasks. (Sometimes in previous commands you said that some files were missing and not implemented and then we found them in some other place). 

*Read File:* for big files use `read_file` paginate with `start_line`/`end_line`__ (1-indexed) on the same path. for binary prefabs use the MCP `manage_prefabs get_hierarchy` to get live Unity-resolved structure

## 4. **Online & Technical Research (If Needed)**
If a decision genuinely needs outside confirmation (Unity patterns, package APIs, platform constraints...), search or inspect local node_modules documentation rather than relying on memory.

If needed you can use Tavily MCP to check Unity docs and patterns, research papers, or community solutions.


## 5. **Master Plan Formulation (__curr_plan_tracker.md)**
IF NOT DONE YET, draft the top-level phase breakdown for C:\Users\franc\Desktop\TileStories\proj_guides\__curr_plan_tracker.md.
- Keep the overall phase progression aligned with `C:\Users\franc\Desktop\TileStories\proj_guides\_2.7_Corrections_TODO.md`.

## 6. **Retrospective Audit (Previous Block)**
- Verify which was the previous block to be implemented and confirm if all the code was well implemented against .clinerules. Inspect physical files on disk, don't trust descriptions alone.
- Confirm project compile and all unit/editor/player mode test pass via Unity MCP (if unity MCP is not available STOP immediately and report).
- Flag any technical debt, missing edge-case handling, or unverified assumptions.
- Confirm we don't have any dead or not really used code. 
- If there are still gaps to do or correct in previous block, flag them, refine this block plan and don't plan next block. DO NOT PLAN NEXT BLOCK IF THE PREVIOUS BLOCK HAS PENDING TASKS TO BE DONE!!!

## 7. **Prospective Plan (Next Block Blueprint)**
Draft the granular TODO list for the next block to be written to __curr_plan_tracker.md.
For every task block, explicitly detail:
   - **WHAT:** Acceptance criteria and deliverables.
   - **HOW:** Design patterns, code quality rules (20-code-quality.md), de-risking code stubs, or exact package paths to import...
   - **WHERE:** Exact disk paths per 10-structure.md.
   - **WHY:** Architectural rationale for framework reusability.
   - **TESTS:** Language-agent tests (Edit/Play Mode). IF NEEDED Vision/Human tests, they should be referenced and detailed, with a well definded "handover guide" for the vision agent or the human, at the end of the chat, and I will copy those to the files: 
   - `C:\Users\franc\Desktop\TileStories\proj_guides\_2.7.1_Vision_Tests.md`
   - `C:\Users\franc\Desktop\TileStories\proj_guides\_2.7.2_Human_Tests.md`
   - - »» These handover guides should have all important details like for agent or human to know what to do and check - expected runtime behaviors, exact test scenes to open, screenshots to inspect, target visual evaluation criteria, edge cases to verify, etc.
   - In terms of the tests themselves, so you know we have 3 sets of already defined POI markers around the "Lamp, Painting and Camera" maine POIs. Many with different positions, categories, effects, hierarchy, with reveal delays, etc... The Lamp set is the bigger one with some 12+ POIs. So confirm well the POIs configs when implementing some test, and if needed add new POIs to make sure we actually test all situations and edge cases. 
   - And in terms of tests to implement, lets use real code for real testing. No mock code!!
   - **FINISHING:** Checklist for updating __curr_plan_tracker.md and structure logs (60-finishing). 
If the file already add some detailed work plan for the current block, and we need to adjust or complete or correct, give me the clear sections to delete and the new text to replace, etc.
  - IMPORTANT: I WANT A FULLY DETAILED PLAN!! So see all the info we currently have in the plan, and give me all that well organized and well updated and well corrected, WITHOUT THE LOSS OF INFORMATION. I WANT A VERY COMPLETE AND DETAILED PLAN. 

## 8. **Clarifications & Open Questions**
### 8.1. In general, for each doubt it occur to you, check if this thinking framework helps to solve it: 
**Just remember:**
- That I want to implement a very complete and versatile Framework with all the good options and features that is good to have for this domain, and with clear "selection options of those features in the Editor Tab + clear variables and param config means" for the developer to select and adjust what he wants for its concrete App. 
- That I want the code well organized and simple and clean as possible, with all the WHAT/HOW/WHERE/WHY questions answered according to the .clinerules/ and to the Target Domain Spec we are implemeting now. 
And so do you best analysis, reason at two separate levels: 
1. **Architecture Level (Where it lives):** Map folder, assembly, and component scope. Evaluate 3 distinct structural options with trade-offs, state the choice, and explain why.
2. **Implementation Level (How it works):** Evaluate 3 concrete implementation choices (e.g., event-driven vs. direct call, ScriptableObject vs. hardcoded) and select the simplest, most robust option.
And then:
- Pick the best option, and we use this option as the default; 
- And then, if there are other options actually good in terms of feature or functionality for the the Framework, so lets allow for the developer to also choose the best option that he prefers for his app, and so we make available in the Editor Tab some drop down or toggle or table or other element for the developer to choose between the different available good quality options, and with the inheritent good variables and params to config that option. 
### 8.2. If even after applying this thinking framework you discover architectural discrepancies, missing prerequisites, or competing implementation choices that persist, so list them clearly at the end of your response before we move to Act Mode. 

## 8.3. Decisions & Questions
At the end of your response, categorize decisions clearly:
a) **Decisions made autonomously by the agent:** Fully resolved choices embedded directly into the proposed plan. All decisions that have a clear winner or where both options are indeed good should be automatic decided, plan with it, and just tell the user at the end decisions made. 
b) **Decisions requiring developer input:** Trade-offs where no clear winner exists, requiring developer selection before proceeding.

## 9. Learning Summary
At the end give me a learning summary where you tell a concise explanation of: 
a) explain the features we are implementing
b) explain the most important decisions in terms of the WHAT, HOW, WHERE, WHY and/or TESTS, so I can keep an easy track of what we are doing and why. This way I learn important concepts and details along the way while we are implementing this project because I want to write a master thesis about it and defend it, so I need to learn all important details.

## 10. Output Discipline
Present the full retrospective, decision analysis, proposed plan, decisions and questions (a,b), and the learning summary, directly in chat.
IMPORTANT: I WANT A FULLY DETAILED PLAN!! So check if there exists already a previous plan for that block, and give me a whole new detailed plan for the whole block well organized and well updated and well corrected, WITHOUT THE LOSS OF INFORMATION. I WANT A VERY COMPLETE AND DETAILED PLAN retaining ALL structural detail (WHAT/HOW/WHERE/WHY for each task or group of tasks - TESTS/FINISHING at the end of the block) without omitting context or truncating sections, so I can just add or replace the whole ## Block N - Plan to the file. Do NOT write or edit any files on disk. The developer will review, copy the plan into __curr_plan_tracker.md, and issue the mode switch command.

