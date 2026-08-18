
You are a sooftware engeneer and architect and I need you to review and correct the code I implemented. Here are the detailed tasks todo: 

## 1. **Read Guidelines & Architecture Re-Grounding** 
- Workspace Rules: C:\Users\franc\Desktop\TileStories\.clinerules 
- Structure Map: C:\Users\franc\Desktop\TileStories\.clinerules\10-structure.md

## 2. **Target Domain & Context Re-Grounding**
- Target Domain Spec: `C:\Users\franc\Desktop\TileStories\proj_guides\_2.6_Select_Filter_Search.md`
- Reference Editor Code: C:\Users\franc\Desktop\TileStories\proj_guides\_5.1_Editor_Tab.md 

## 3. **Deep Codebase Audit & Duplicate Prevention**
Inspect physical files on disk before planning. Do NOT trust text descriptions alone.
- Framework Path: C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework
- App Path: C:\Users\franc\Desktop\TileStories\TileStories\Assets\Apps\LivingRoom

*Audit Task:* Verify file paths and check for near-duplicate classes across folders. Ensure 10-structure.md accurately reflects physical disk layout before creating new tasks. (Sometimes in previous commands you said that some files were missing and not implemented and then we found them in some other place). 

## 4. **Code deep review and audit**
Read the Target Domain Spec phase by phase block by block and: 
- Confirm if all features and details referenced in the Target Domain Spec are well implemented without missing any feature or option or importatn variable or param to expose to developer in editor tool, or logic to implement, or check, etc... confirm if there is any detail missing. 
- Confirm unit/editor/play test pass rates via Unity MCP (confirm if it is available, if not STOP imediately). 
- Confirm if all the code was well implemented against .clinerules.
- Confirm we don't have dead code. 
- Flag any technical debt, missing edge-case handling, or unverified assumptions.
- If there are still gaps to do or correct, flag them, and present a plan of what should be done. Detail things here in chat that should be done. Explain: 
- - » **WHAT:** Acceptance criteria and deliverables.
- - » **HOW:** Design patterns, code quality rules (20-code-quality.md), de-risking code stubs, or exact package paths to import...
- - » **WHERE:** Exact disk paths per 10-structure.md.
- - » **WHY:** Architectural rationale for framework reusability.
- - » **TESTS:** Edit/Play Mode tests... first with language, until everything is green, and then if needed vision tests... and then if needed explain a detailed todo list of any necessary check that a human in the loop should do to confirm something that you can't with a language model and then with a vision model. 
- - » **FINISHING:** (60-finishing). 

## 5. **Clarifications & Open Questions**
If you discover architectural discrepancies, missing prerequisites, or competing implementation choices, list them clearly and present the best options for me to confirm. 
Just remember, that as a general solution thinking framework I want to follow something like this:
- That I want to implement a very complete and versatile Framework with all the good options and features that is good to have for this domain, and with clear "selection options of those features in the Editor Tab + clear variables and param config means" for the developer to select and adjust what he wants for its concrete App. 
- That I want the code well organized and simple and clean as possible, with all the WHAT/HOW/WHERE/WHY questions answered according to the .clinerules/ and to the Target Domain Spec file. 
And so do you best analysis, reason at two separate levels: 
1. **Architecture Level (Where it lives):** Map folder, assembly, and component scope. Evaluate 3 distinct structural options with trade-offs, state the choice, and explain why.
2. **Implementation Level (How it works):** Evaluate 3 concrete implementation choices (e.g., event-driven vs. direct call, ScriptableObject vs. hardcoded) and select the simplest, most robust option.
And then:
- Pick the best option, and we use this option as the default; 
- And then, if there are other options actually good in terms of feature or functionality for the the Framework, so lets allow for the developer to also choose the best option that he prefers for his app, and so we make available in the Editor Tab some drop down or toggle or table or other element for the developer to choose between the different available good quality options, and with the inheritent good variables and params to config that option. 


**So proceed to execute this tasks step by step**
This is a big set of tasks and you need to do it with a big responsible sense. Lets be good and actually confirm in the code and in the tests that everything is well implemented and working ok and according to the .clinerules/. Don't assume the plan guide files are correct and up to date. Actually confirm things in code. So lets do these tasks step by step, in a methodical way. Lets do each step well complete and analysed at the first time withour rush. I don't want to have to repeat things. So be good, not fast. Actually confirm everything, step by step, phase by phase, block by block. Be good, not fast. 