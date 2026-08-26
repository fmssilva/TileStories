
# `Git`

# ---

# `[CURRENT_MODE: PLAN_MODE]`
## [DISK_WRITE_ALLOWED: FALSE]
## DIRECTIVE: TARGETED GUIDE AUDIT & DELTA PLAN

You are in PLAN MODE. Do NOT write or modify files on disk yet.

1. **Grounding & Mode Sync Confirmation**
   - Core rules: C:\Users\franc\Desktop\TileStories\.clinerules
   - Target domain guide file to work on: `C:\Users\franc\Desktop\TileStories\proj_guides\_5.1_Editor_Tab.md`

2. `**Scope of Adjustment / Task**`
  I think these tasks were done already: 
  """
  - You made the concrete marker cntainer (example "The Lamp") all in red color. Lets have some random colors. maybe we have some array of default colors with some 10 or 12 color and then we choose colors at random from there...?? or something like that...??
  - AND currently the 2 tab buttons of the global scene and specific markers are inside the "scroll container". but lets move them out. and so we have the 2 tabs above and then right bellow we have the scroll container (with 0 space on top), and then then inner "border countor color container" and then the normal content containers... 
   """
   BUT: 
   - lets make the colors of the concrete markers titles/names to be vivid colors. they are too much grayish. lets make them "live" (example the ones we have for the containers of the global scene tab, they look good colors, with good visibility over dark color of unity). 
   - AND also the color of the TAB buttons of global scne and specific markers, they are also "grayish", and so lets make them more live also... not grayish... 
   - AND then make sure we correct some errors we have: 
  """You cannot call GetLast immediately after beginning a group.
UnityEngine.GUILayoutUtility:GetLastRect ()
TileStories.Editor.POIAuthoringToolWindow:DrawTabContentContainer (System.Action,UnityEngine.Color,string,UnityEngine.GUIStyle) (at Assets/Framework/Editor/POIAuthoring/POIAuthoringToolWindow.cs:313)
TileStories.Editor.POIAuthoringToolWindow:<OnGUI>b__184_0 () (at Assets/Framework/Editor/POIAuthoring/POIAuthoringToolWindow.cs:266)
TileStories.Editor.POIAuthoringToolWindow:DrawConfigMutationScope (System.Action,bool) (at Assets/Framework/Editor/POIAuthoring/ConfigData/POIAuthoringToolWindow.ConfigHistory.cs:18)
TileStories.Editor.POIAuthoringToolWindow:OnGUI () (at Assets/Framework/Editor/POIAuthoring/POIAuthoringToolWindow.cs:266)
UnityEngine.GUIUtility:ProcessEvent (int,intptr,bool&)
"""
AND
"""
You cannot call GetLast immediately after beginning a group.
UnityEngine.GUILayoutUtility:GetLastRect ()
TileStories.Editor.POIAuthoringToolWindow:DrawTabContentContainer (System.Action,UnityEngine.Color,string,UnityEngine.GUIStyle) (at Assets/Framework/Editor/POIAuthoring/POIAuthoringToolWindow.cs:313)
TileStories.Editor.POIAuthoringToolWindow:<OnGUI>b__184_1 () (at Assets/Framework/Editor/POIAuthoring/POIAuthoringToolWindow.cs:269)
TileStories.Editor.POIAuthoringToolWindow:DrawConfigMutationScope (System.Action,bool) (at Assets/Framework/Editor/POIAuthoring/ConfigData/POIAuthoringToolWindow.ConfigHistory.cs:18)
TileStories.Editor.POIAuthoringToolWindow:OnGUI () (at Assets/Framework/Editor/POIAuthoring/POIAuthoringToolWindow.cs:269)
UnityEngine.GUIUtility:ProcessEvent (int,intptr,bool&)
"""

1. **Targeted Audit & Gap Analysis**
   - Read the target guide file and relevant source files on disk - always ground your plan on real code files.
   - Identify missing details, typos, logical gaps, or misalignments with .clinerules.
   - Do NOT evaluate complex 3-tier architectural redesigns unless explicitly requested; focus strictly on surgical corrections.

2. **Proposed Plan**
   Present a clear, exact and well detailed implementation plan for the requested adjustments/tasks. The plan should be well though and detailed. Detail: 
   - **WHAT:** Acceptance criteria and deliverables.
   - **HOW:** Design patterns, code quality rules (20-code-quality.md), de-risking code stubs, or exact package paths to import...
   - **WHERE:** Exact disk paths per 10-structure.md.
   - **WHY:** Architectural rationale for framework reusability.
   - **TESTS:** Language-agent tests (Edit/Play Mode). Vision/Human tests are not needed now and should be defered. 
   - **FINISHING:** Give clear summary of things done in chat. If some new file was created or some big change update the 10-structure.md and/or the "Target domain guide file to work on" with the changes if relevant...
   »» Present this full detailed plan here in chat. 
    

## 5. Present your full analysis report and proposed plan text in chat with all and full details. Do not edit disk files yet. I will copy myself this full plan from the chat to the __curr_plan_tracker.md file. Once I confirm and switch you to ACT MODE you can confirm the plan is written in the file, and then you can start implementation.


# ---


# `COPY PLAN TO CURR_PLAN FILE!`
# `[CURRENT_MODE: ACT_MODE]`
## [DISK_WRITE_ALLOWED: TRUE]
## DIRECTIVE: EXECUTE TARGETED UPDATES & VERIFY

You are now in ACT MODE. Proceed directly with disk edits and code implementation.

1. **Grounding & Sync Check**
   - Read the updated tasks to be done in C:\Users\franc\Desktop\TileStories\proj_guides\__curr_plan_tracker.md. 
   - ACTUAL READ THAT FILE FROMO DISK SO YOU HAVE THE FULL CURENT CONTENT AND NOT TRUNCTED THINGS ETC... IF SOMETHING IS TRUNCATED THE PROBLEM MIGHT BE YOUR READING NOT THE FILE.
   - Use it as the Ground Truth TODO list for you to follow and update during implementation so you don't skip any step.  

2. **Surgical Execution Protocol**
   - Modify ONLY the specified guide files or code files.
   - Maintain strict formatting, clear parameter descriptions, and alignment with .clinerules.
   - After implementation compile code and execute Edit/Playmode tests to confirm everything is ok. 
   

3. **Tool Execution & File Editing Discipline - remember "50-terminal_and_tools.md"**
- Use editor with verbatim old_text (including leading whitespace and allway replacing complete lines); if matching fails or is ambiguous, fall back to unityMCP__apply_text_edits or python edit_file.py. Always verify immediately by re-reading the edited region, running refresh_unity, and passing all tests.


4. **Cleanup & Verification Checklist (Definition of Done)**
   - [ ] All tasks in __curr_plan_tracker.md implemented 
   - [ ] Zero compile errors.
   - [ ] All Edit/Playmode tests pass. 

5. **Hard Stop**
   - Once all the tasks are complete, output a summary of what was done and achieved in the chat and STOP immediately.

# ---