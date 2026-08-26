# `Git`


# ---

# The workflow alternates strictly between two operating modes:
- **PLAN MODE:** Analyzing requirements, auditing code, drafting options, and building execution roadmaps.
- **ACT MODE:** Modifying files, running builds, executing tests, and verifying behavior.

## **Mode Sync Protocol**
To prevent drift across context compaction, summarizations, or long task executions YOU SHOULD ALWAYS CONFIRM IN THE FILE`C:\Users\franc\Desktop\babyproof.fun\proj_guides\__mode.md` the active mode and the current command that should be executed. 

THIS FILE IS UPDATED BY THE USER FREQUENTELY AND SHOULD ALWYAS BE THE GROUND TRUTH. SO YOU SHOULD ALSO FREQUENTLY READ THE __mode.md FILE AGAIN AND CONFIRM THE CURRENT MODE AND THE CURRENT COMMAND. DO NOT TRUST AMYTHIMG ELSE!!! CHECK YOUR MODE AND CURRENT COMMAND TO EXECUTE FREQUENTELY EVERYTIME SOMETHING CHANGE (CONTEXT COMPACT, NEW COMMAND, NEW INSTRUCTION FROM CLINE, ETC...).

# The MODE changed in the file:
 `C:\Users\franc\Desktop\babyproof.fun\proj_guides\__mode.md`. Read the file again to confirm active mode and the current command that should be executed. 

THIS FILE IS UPDATED BY THE USER FREQUENTELY AND SHOULD ALWYAS BE THE GROUND TRUTH. IF IN ANY DOUBT DON'T TRUST AMYTHIMG ELSE. TRUST ONLY ON THIS FILE INSTRUCTIONS WHICH ARE UPDATED CONSTANTELY.



# Prototypes:
- You can also see that corresponding prototype in this url: https://storytree-studio.lovable.app/
(you can use the MCP tool to access it and see the website for yourself). 
- And you can see the corresponding prototype sample code in the folder:
`C:\Users\franc\Desktop\babyproof.fun\proj_guides\prototypes\home_page\lovable_prototype`
- And also see these prototypes: 
  - C:\Users\franc\Desktop\babyproof.fun\proj_guides\prototypes\C_papercut_storybook.html
  - C:\Users\franc\Desktop\babyproof.fun\proj_guides\prototypes\M_living_forest_real_layers.html
  - C:\Users\franc\Desktop\babyproof.fun\proj_guides\prototypes\O_living_forest_sketchy.html


# Tests in the home page: 
## QA recipe — "object/component test showcase" on the Home page

Goal: let a human eyeball ANY component(s) in isolation, parked in front of
everything else on the page, then remove it immediately. 

1) Wrap one instance of each component to verify in its own slot:
   each slot is a <div class="qc-slot"> with (a) a tiny text label of the
   component name, and (b) the component rendered with its OWN props.
2) Lay the slots out inside ONE fixed, full-viewport overlay:
      .qc-grid { position:fixed; inset:0; z-index: Z.test; /* QA seam, do not SDK/ship */
                 display:flex; flex-wrap:wrap; align-items:flex-start;
                 padding: 12px; backdrop/… none, keep plain }
   Each .qc-slot is itself positioned/inset so the component "reads" alone,
   with a contrast-safe muted tag (e.g. --ink-soft) naming it.
3) Rendered it once at the ROOT of the page underneath <main> (e.g. at the
   very top of the Home app wrapper), ABOVE the page content, because it is
   position fixed + Z.test. It must NOT intercept:
   .qc-grid { pointer-pointer-events: none } on the grid; the labels are
   aria-hidden + the whole thing aria-hidden="true" (it's a decorative QA
   scene, not interactive).
4) Config is a top-of-file data array (one entry per slot:
   { name, component, props }) — per 20-code-quality §2.2. No inline scatter.
5) It is a DELIBERATE disposable fixture. Put a "QA in progress — REMOVE
   AFTER CONFIRM" comment at top. On confirmation the agent deletes the
   overlay + its import from the page and logs the removal in
   __deferred_TODOs.md (treated as an intentional, recorded cleanup).
6. Verification by the human: npm run dev, open /, browser_snapshot +
   browser_take_screenshot(absolute path) in BOTH themes (toggle the theme
   button), confirm each component legible/未 its intended size + dark mode
   correct, nothing else on the page broke, then remove.
7. (optional, keep simple if not needed) gate behind ?qc=1 on the URL so it's off
   by default and resurfaceable without re-editing markup.



# `FUTURE TASKS`




# Logos... 
Lets do a deep analysis of the logos of our app. I added some new png to the folder 
C:\Users\franc\Desktop\babyproof.fun\tinyfingers_web\public\favicon

and also svg to the folder: 
C:\Users\franc\Desktop\babyproof.fun\tinyfingers_web\src\branding\favicon

so confirm that all images look good and are with the good dimensions etc... and then confirm that our site is already using them correctly in the correct places... example in the branding folder to add the correct logo to the side of the website name etc... 

so do a deep analysis of all this and confirm any missing detail we are missing to do, any wiring, any bad config etc... check all the problems we might need to solve and all the tests we should implement to confirm evrything is working well in terms of these logos and brands and favicons etc...????


# `Header & Pages...`

 
## objects vs > Note on name collision: these `design/objects/star|leaf|sparkle` are ambient decorative *objects*, distinct from the existing home *card-doodle icons* `domains/home/icons/ic-star|ic-leaf|ic-sparkle`. Both stay; a future consolidation (only if a second feature reuses them identically) is a deferred todo, flagged, not forced now.
??
# `DELETE THE __CURR_PLAN_TRACKER!!!`
# `Motion functions`
## current motivation - clean home pagee domain folder and reorganize the whole project. 
- Lets do a deep clean up of our code of the home page and tokens and z values and so on. So for example, i think we have the whole effects things like sun, mooon, stars, fireflies, birds, cmmets eetc... all inside some main containers...??? but that is always breaking things, because for example we want to have sun and moon on the back of all the scene images (grass, house, forest, mountains), while we want to have fire flies in front of forest, house and grass, and other confliciting configs for each effect. 
And we are always breaking things up because of this "grouping of things" to build a higher level component that then is useless and don't allow to fine tune things for each effect, or for each image in the scene. 

So lets make a deep review and clean up of our code we have currently. Lets make it much more granular and each effect as a single component that we can import and use where we want and configure as we want. 

so lets do a deep analysis and clean up this whole project in terms of z values, and then implement the objects folder and then add them all directly to the home page directly at the root of the home page so we see them directly in the home page above anything else we already have at the home page. WE CAN EVEN CREATE SOME TEST COMPONENT IN THE HOME PAGE, AND WE PUT ALL OUR OBJECTS THERE IN THIS COMPONENT...??? SEE THE BEST CLEAN WAY... 

## Check what we already did: 
- Check all the style objects we have implemented in 
C:\Users\franc\Desktop\babyproof.fun\tinyfingers_web\src\design\objects
those are the "static definition of different objects".

also check the current "motion effects functions we are alrady using in the home page domain folder for the sun, moon, fireflies, etc... LETS NOT REUSE OR HAVE ANY BACKWARD COMPATIBILITY WITH THESE... WE WILL DELETE THEM ALL.. AND REBUILD THINGS MUCH MORE ORGANIZED. JUST USE THEM AS INITIAL INSPIRATION. 

## now lets implement the "movement or motion functions!!!": 
Now lets implement some sort of "motion functions folder"??? 
example of functions might be elipse movement, and then we use it for the sun and moon... or horizontal movement and then we use it for a commet etc... or random movement inside some given area or component and then we use it for fireflies etc... and so on... ... some linear for other things, some linear in terms of main direction with some sinusoidal + random oscialtory movements for for example leafs falling down the sky which aways come down in a bit random oscialtory movement, and some loop movement with random oscilatory movements example for fireflies to always fly around etc......
a) is this a good "helper folder to have about movement functions that we can use in the website???) or not really?? 
b) if yes... where to put thi folder? whith what name?
c) how will we later use these functions? should they have soome common shared type to be easy to import or not really??? or they just convert simply in somee div...?? 
d) what should be the args of these functions?? should they accept the objects like sun moon etc as arg?? plus some config params like the speed of the motion, the time duration of the motion, the start_delay time; some flag to tell if it always start over again when it finishes the effect so we can for example have birds flying across multiple times for ever... the orientation of that main motion in degrees example the linear + a bit random movement of leafs are vrtical or a bit diagoonal while the clouds or birds might be horizontal... the amplitude of the random movements, the init and final positions of the movements etc... so all the args to cnfig that motion effect of the function. and then we also accept a object as arg that will be subject to that montion function, for example we pass the sun or the moon etc... should we have these motions to be returned inside a component or div or whatever and so the size will be relative to that?? or should we just return the motion with the object to the parent component..??? and if we have many objects with the same motion, but maybe different, so should we also have the arg of the quantity, example 20 fireflies...  and then the other args so maybe can be a range instead of a single numbeer, and this function return the whole set of objects with the respective function..???? what is the cleanest way to do it???





# `STEP 1 - PLAN`
## [CURRENT_MODE: PLAN_MODE]

Perform a critical review of the project on disk, and prepare a granular blueprint for the given tasks to be done. For that do the following: 

## 1. Guidelines & Architecture Re-Grounding
- Workspace rules: read ./.clinerules/ (specifically 00-process.md, 10-structure.md, 20-code-quality.md, 30-design-system.md, 40-testing.md, 50-terminal_and_tools.md, 60-finishing.md)
- Structure map: read ./.clinerules/10-structure.md

## 2. Target Domain & Context Re-Grounding
- Target domain spec: `./proj_guides/_1_site_base.md` (and companion `_1_site_base_APPENDIX_home_components.md`)
- Current plan tracker: ./proj_guides/__curr_plan_tracker.md

## 3. INITIAL INSPECTION: 
- Run test commands: "cd .\tinyfingers_web\; npx astro check; npx vitest run tests/unit/; npm run build" »» and wait 2 minutes because my PC is very slow, AND THEN CHECK THE WHOLE OUTPUT OF EACH ONE OF THE 3 COMMANDS! (You need to run these commands not from the root but from .\tinyfingers_web\)
- Webpage inspection: use the shared browser page first if available. If not, open http://localhost:4321/ in the live browser and verify the UI like a human and if any console logs problems... 
  
## 4. Deep Codebase Audit & Gap Analysis
Inspect physical files on disk that might be related with this task before planning. Do NOT trust past conversation summaries alone — a prior session's summary claiming something exists or is missing must be independently verified on disk.

- *Note* - The project is not in the root of the workspace but yes in the folder "tinyfingers_web/"
 
*Audit Task:* Verify actual file paths and check for near-duplicate classes/widgets/components across folders. Ensure .clinerules/10-structure.md accurately reflects the physical disk layout before creating new tasks.

## 5. Online & Technical Research (if needed)
If a decision genuinely needs outside confirmation (package APIs, platform constraints, browser limits), search or inspect local node_modules documentation rather than relying on memory.

## 6. Proposed Plan 
Present a clear, exact and well detailed implementation plan for the requested adjustments/tasks. The plan should be well though and detailed. Detail: 
- **WHAT:** acceptance criteria and deliverables.
- **HOW:** design pattern, which .clinerules/20-code-quality.md rules
  apply, exact package/API to use.
- **WHERE:** exact disk paths, per .clinerules/10-structure.md.
- **WHY:** the rationale for this approach over the alternatives considered.
- **TESTS:** per .clinerules/40-testing.md, see which layers of tests you should implement and what each one specifically needs to check for this task — don't plan a boilerplate list of tests. Only the ones real needed for fast and robust developement. BUT always do these steps at least: 
a) Implement the task/feature...
b) Implement necessary or usefull unit tests... IF WE ARE CREATING SOME UI COMPONENT OR FEATURE ADD THAT FEATURE TO THE PLAYGROUND PAGE IN A BIG COMPONENT, AT THE TOP OF THE PAGE, SO WE CAN CONFIRM AND TEST THAT COMPONENT/FEATURE IN ISOLATION BEFORE INTEGRATING IT IN THE FINAL PAGE.
c) Run: "cd .\tinyfingers_web\; npx astro check; npx vitest run tests/unit/; npm run build" »» and wait 2 minutes because my PC is very slow, AND THEN CHECK THE WHOLE OUTPUT OF EACH ONE OF THE 3 COMMANDS! (You need to run these commands not from the root but from .\tinyfingers_web\)
d) Open the shared live browser page and click the exact controls involved (http://localhost:4321/). THIS IS WHERE YOU SHOULD RUN COMPREENIVE TESTS!! CLICK THE IMPLEMENTED UI LIKE A HUMAN, EXECUTE THE INTENDED FLOWS OF CLICKS CONFIRM THE EFFECTS, CONFIRM EVERYTHING RENDERS AND IS CLICABLE, ETC. AND IN DIFFERENT LAYOUTS (DESKTOP, MOBILE) 
e) IF NEEDED TO CONFIRM SOME DETAIL THAT YOU ARE NOT SURE YET SO IMPLEMENT playwright TESTS. 
f) Human in the Loop tests: give in chat a full list of all the tests a human should confirm in browser if things really work well. Only then if necessary we'll implement more tests.
- **FINISHING:** checklist per .clinerules/60-finishing.md.

**I want a fully detailed plan.** Bring forward all the information we
currently have, organized, corrected, and updated — without losing any of it.

## 8. Architectural & Implementation Decision Analysis
For key decisions required in this block, apply a two-level evaluation:
1. **Architecture Level (Where it lives):** Evaluate 3 distinct structural options (folder, component scope, assembly) with trade-offs. Select the winner and explain why.
2. **Implementation Level (How it works):** Evaluate 3 concrete implementation choices (e.g., custom logic vs lightweight library vs full package). Select the simplest, highest UX/visual quality, and most robust option without bloat.

Always keep in mind some guiding principles:
- **Goal:** Uncompromising UX/UI quality and "wow" factor for young children, backed by simple, clean, modular code.
- **Scaffolding:** Maintain strict adherence to ./.clinerules/ and the active target domain spec.
- **Completeness:** The plan should retain ALL structural detail (WHAT/HOW/WHERE/WHY/TESTS/FINISHING) without omitting context or truncating sections.

## 9. Decisions & Questions
At the end of your response, categorize decisions clearly:
a) **Decisions made autonomously by the agent:** Fully resolved choices embedded directly into the proposed plan. All decisions that have a clear winner or where both options are indeed good should be automatic decided, plan with it, and just tell the user at the end decisions made. 
b) **Decisions requiring developer input (IF ANY):** Trade-offs where no clear winner exists, requiring developer selection before proceeding. Important, you don't need to have this. If you can solve all decisions by yourself so do it.

## 10. Learning Summary
At the end give me a learning summary where you tell a concise explanation of: 
a) explain the features we are implementing
b) explain the most important decisions in terms of the WHAT, HOW, WHERE, WHY and/or TESTS, so I can keep an easy track of what we are doing and why. This way I learn important concepts and details along the way while we are implementing this project because I want to write a master thesis about it and defend it, so I need to learn all important details.

## 11. Output Discipline
Present the full retrospective, decision analysis, proposed plan, decisions and questions (a,b), and the learning summary, directly in chat.
IMPORTANT: I WANT A FULLY DETAILED PLAN!! So check if there exists already a previous plan for that block, and give me a whole new detailed plan for the whole block well organized and well updated and well corrected, WITHOUT THE LOSS OF INFORMATION. I WANT A VERY COMPLETE AND DETAILED PLAN retaining ALL structural detail (WHAT/HOW/WHERE/WHY for each task or group of tasks - TESTS/FINISHING at the end of the block) without omitting context or truncating sections, so I can just add or replace the whole ## Block N - Plan to the file. Do NOT write or edit any files on disk. The developer will review, copy the plan into __curr_plan_tracker.md, and issue the mode switch command.


# `STEP 2 - ACT:`
## [CURRENT_MODE: ACT_MODE]`

You are now in ACT MODE. Proceed directly with disk edits, script execution, and code implementation.

## 1. Plan as Ground Truth
- The plan to be implemented lives in ./proj_guides/__curr_plan_tracker.md. Re-read this file fresh from disk before starting any task — do NOT rely on conversation summaries or memory buffers. Treat it as the authoritative TODO list.

## 2. Strict Scope Lock
- Active block tasks only. Implement ONLY what is explicitly listed in the tracker file.
- NO unsolicited refactoring, code formatting cleanups, or extra features outside the active task.
- Unplanned ideas or optimization opportunities discovered during implementation must be noted in the final summary for a future `PLAN_MODE` pass — never added to the current diff.
- *Narrow Exception:* If a guideline or linting violation directly blocks the active task from compiling or passing tests, fix that specific violation with a minimal, surgical edit.

## 3. Iterative Execution Cycle
For every task or small task group in the active block, execute the following cycle:
1. **Read:** Parse the active task's specification fresh from ./proj_guides/__curr_plan_tracker.md.
2. **Implement:** Write or modify only the files strictly required for that task.
3. **Verify:** per .clinerules/40-testing.md, see which layers of tests you should implement and what each one specifically needs to check for this task — don't plan a boilerplate list of tests. Only the ones real needed for fast and robust developement. BUT always do these steps at least: 
a) Implement the task/feature...
b) Implement necessary or usefull unit tests... 
c) Run: "cd .\tinyfingers_web\; npx astro check; npx vitest run tests/unit/; npm run build" »» and wait 2 minutes because my PC is very slow, AND THEN CHECK THE WHOLE OUTPUT OF EACH ONE OF THE 3 COMMANDS! (You need to run these commands not from the root but from .\tinyfingers_web\)
d) Open the shared live browser page and click the exact controls involved (http://localhost:4321/). THIS IS WHERE YOU SHOULD RUN COMPREENIVE TESTS!! CLICK THE IMPLEMENTED UI LIKE A HUMAN, EXECUTE THE INTENDED FLOWS OF CLICKS CONFIRM THE EFFECTS, CONFIRM EVERYTHING RENDERS AND IS CLICABLE, ETC. AND IN DIFFERENT LAYOUTS (DESKTOP, MOBILE) 
e) IF NEEDED TO CONFIRM SOME DETAIL THAT YOU ARE NOT SURE YET SO IMPLEMENT playwright TESTS. 
f) Human in the Loop tests: give in chat a full list of all the tests a human should confirm in browser if things really work well. Only then if necessary we'll implement more tests.
4. **Re-Read:** Re-read ./proj_guides/__curr_plan_tracker.md from disk to maintain state alignment.
5. **Audit:** Check edits against ./.clinerules/20-code-quality.md and ./.clinerules/10-structure.md to confirm zero regressions.
6. **Update Tracker:** Mark the task completed in ./proj_guides/__curr_plan_tracker.md` on disk and log any significant technical decisions made.
7. **Progress Check:** Output a concise chat update (files changed, pass/fail status, active next task).

Repeat steps 1–7 until every task in the active block is complete.

## 4. Terminal & Tool Discipline
Per ./.clinerules/50-terminal_and_tools.md:
- Chain commands with `;`, not `&&`.
- Do `cd ./tinyfingers_web/` to run package-manager, Astro, Vitest, build, and `npx` commands. Do not install app dependencies into the workspace root by default.
- Redirect verbose command outputs to log files rather than piping through filters.
- Perform verbatim multi-line code replacements — always re-read changed file regions to confirm accurate updates.

## 5. Definition of Done & Hard Stop
The active block is COMPLETE only when **all** of the following conditions are met:
- [ ] Every task checkbox in the active block within ./proj_guides/__curr_plan_tracker.md is checked.
- [ ] The codebase compiles and analyzes clean with zero errors.
- [ ] All applicable test layers pass with zero failures.
- [ ] Temporary scratch files and test logs are cleaned up.
- [ ] The tracker file accurately reflects disk state.

**HARD STOP:** The exact moment the Definition of Done is met, **STOP immediately**. Do not proceed to the next block, do not invent polish tasks, and do not make unprompted file edits. Output the final summary (files modified, test pass results, next scheduled block) and await developer review and mode switch.

# ---

# `About some points & questions`
## [CURRENT_MODE: PLAN_MODE]
## [DISK_WRITE_ALLOWED: FALSE]
## DIRECTIVE:
1. I already copied the full analysis report and proposed plan text that you gave me in chat to the __curr_plan_tracker.md file. Confirm the plan is correctly written in the file and well organized. IF we need to reorganize it, give me the clear sections to delete and the new text to replace, etc.    - IMPORTANT: I WANT A FULLY DETAILED PLAN!! So see all the info we currently have in the plan, and give me all that well organized and well updated and well corrected, WITHOUT THE LOSS OF INFORMATION. I WANT A VERY COMPLETE AND DETAILED PLAN retaining ALL structural detail (WHAT/HOW/WHERE/WHY/i18n/TESTS/FINISHING) without omitting context or truncating sections.

2. Do a deep analysis about the questions I just answered, and think the best decisions and implications to our proposed plan that is already in the __curr_plan_tracker.md file.

3. Give me the updates that I should do to the file:
   - Tell me the concrete section I should change (example section X.2);
   - Give me here in chat the revised plan for that section, with all the steps and full details;
   - I will then delete the current section and paste the new section guide that you give me here in chat.
   - IMPORTANT: I WANT A FULLY DETAILED PLAN!! So see all the info we currently have in the plan, and give me all that well organized and well updated and well corrected, WITHOUT THE LOSS OF INFORMATION. I WANT A VERY COMPLETE AND DETAILED PLAN. 


## ---

# ---

# `comands`
cd .\tinyfingers_web\
npx astro check; npx vitest run tests/unit/; npm run build
npm run dev
# ---

# `Visual Tests`
**IF YOU ARE A VISION CAPABLE AGENT:** include in the tests actual visual confirmation of concrete visual checks to make sure the design and UX is correct and as intended. So, 1. Call "browser_navigate" to the URL http://localhost:4321, 2. Capture a viewport screenshot of the intended page or components in Light Mode ("browser_take_screenshot" with an __absolute filename__ under "C:...\babyproof.fun.playwright-mcp<name>.png" - relative names fail in this setup). 3. Visual inspect ("read_files" on that PNG — and actually "see" it). 4. Click the Theme Toggle button to switch to Dark Mode (""browser_click" with a selector). 5. Capture a second screenshot. 6. Visual inspect. *In inspections* Check for UI/UX visual bugs: contrast issues (e.g., hard-to-read text, unstyled components, missing dark mode styles...). If you spot visual flaws, locate the corresponding components and apply the necessary fixes. **Re-run the browser check to confirm the visual bugs are resolved.**



