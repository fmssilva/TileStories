You are building round 9 of the guidance system in a browser prototype, the "Block Components Lab". It lives in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app,
so every feature must be buildable in Unity: name its Unity path. Work only in the Lab; do not touch any Unity project file.

READ FIRST (every line):
- `../../_3.8_Guidance_Study.md`:
  - section 8, round 9 (this round);
  - section 3.4, the visitor's controls and memory (Things to try, How does this work?, Ask Azu's "how do I…");
  - section 3.2, the item's fields (Teaches, Done when, Button Show me);
  - section 7, the row demos and the scenes;
  - section 9, the decisions.
- `PROTOTYPE_STRUCTURE.md`: the rules, the modules, "Where a change goes" and the row-demo contract.
- `README.md`: "Prove it" and "Levels". Level 1 changed on 2026-11-09: a guidance round runs test9e up to its own file.
- `../../skills/Feature_Prototyping.md`: step 3.3 (look until it is right, then test) and step 5.
- `brief_round8.md`, for the shape and the way of working, which stay the same, and its "Lessons" section.
- Rounds 1-8, which this round extends in place: `src/guidance/*.js` and `tests/test9e.js` to `tests/test9l.js`. In
  particular round 4's How does this work? in the ✦ menu, round 6's Settings › The guide › Tips (All / Essentials only /
  Off), Show the tips again and the Button field's Show me (`gdShowMe`), and round 2's Fewer like this (the long press).
- What exists today and is extended:
  - The ✦ menu: `src/world/guide-button.js` (`gbItems`, `gbMenuHtml`, `gbMenu`, `menu.opened`); its tests are found by
    `grep -ln "gbmenu" tests/*.js` (test8s, test9d, test9g, test9h).
  - Ask Azu, the live agent: `src/agent/agent.js` and its tools `src/agent/tools.js` (`add(...)`, `show_links`,
    `navigate_to`, `open_feature`); the scripted answers used offline; its tests test8t and test8u (the `_live` files call
    the real model: do not run them).
  - The visitor's Settings: `src/phone/settings.js` and the guide's settings in `guide-button.js`.
- The spec `../../_3.3_POI_Card_Styles.md`, section 11 "Guidance": read it, do not edit it (the Architect does).

## This round

**9.1 Things to try, in the ✦ menu.**
- A new item in the ✦ menu, **Things to try**, opens a short sheet: the App items that teach a feature (each item's
  Teaches / Done when), as a checklist, in the Editor's order:
  - each row: the feature in a few words ("Open a card", "Ask about the view", "Drag Before / After", "Swipe the
    strips", "Pause the guide"…), a tick when done (from the same memory: Done when), and a **Show me** that plays that
    item's gesture on its target (round 6's `gdShowMe`), taking the visitor there first when the target is elsewhere (the
    card, the wall).
  - a line at the top: "3 of 8 tried".
  - done rows stay, ticked; when every row is done, the sheet says so in one line and the menu item still opens it.
- It is pull: never capped, never scheduled, past the limits but never past the gates (round 2's rule for pulled items).
- **Tips Off still lets Things to try show** (the study's proof): Settings › Tips = Off silences the pushed tips, never
  the visitor's own pull.
- The Editor: a row in Guidance › Overall (or App's kind row: choose the one the study implies and say why), **Things to
  try**: In the ✦ menu: Shown / Hidden; and per App item a **In Things to try** (Yes / No; default Yes when it Teaches).

**9.2 Ask Azu's "how do I…?".**
- When the visitor asks Azu how to do something in the app ("how do I pause?", "como vejo o antes e depois?"), the answer
  comes from the same App items: their words, and a **Show me** link card that plays the gesture (as 9.1).
- With the live agent: a new tool, **show_how**: given a feature, it returns the matching App item's line and adds a Show
  me card under the answer. The agent's instructions say to use it for "how do I…" questions about the app, never for the
  history.
- Offline (no key, the scripted answers): a simple match on the question's words against each App item's Teaches and
  words, in both languages, gives the same answer and card.
- The answer marks the item as seen (not done: done is the feature used).

**9.3 Settings › Tips and Fewer like this, checked together.**
- Settings › The guide › Tips (round 6) and the bubble's long press "Fewer like this?" (round 2) already exist. This round
  adds only what joins them to pull:
  - Settings › The guide gets a link **Things to try ›** (the same sheet).
  - Fewer like this's toast gets "You can always find these in Things to try." when the group rested is an App group.
- Check both still work as before; change nothing else in them.

**9.4 The Editor's demos and Preview & Test.**
- Every new row has its demo (Things to try's rows show the sheet on the phone with the new value; In Things to try
  shows the sheet with that row in or out; show_how shows a scripted "how do I…" answer with its card).
- The visitor (Preview & Test) shows "Things to try: 3 of 8".

**9.5 Scenes in Ask Azu's console**, each with its ✓ check:
- "Things to try: Show me on Before / After".
- "Tips Off, Things to try still there".
- "How do I pause the guide? (Show me)".
- "Fewer like this on an App tip, then Things to try".

## How you work (the same as round 8)

1. After each change: only `python build.py`. Its check must read 0/0/0/0.
2. Then run it, capture it, and judge each capture yourself.
   - Captures: 390 px, light and dark.
     - Phone: the ✦ menu with Things to try; the sheet half done, all done; Show me playing from the sheet on the card and
       on the wall; an Ask Azu "how do I…" answer with its Show me card (EN and PT); Settings › The guide with the link;
       the Fewer like this toast's new line.
     - Editor: Things to try's rows; an App item's In Things to try; the visitor's line.
   - Judge:
     - one focal point; the checklist readable in 3 s;
     - nothing over the words (0 px over text); 44 px targets; text 4.5:1 or more;
     - premium, modern, minimal; the sheet in the card's own look (no new style).
   - Fix and capture again until it is right.
   - Save the final captures in
     `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g9\`.
3. Only then write the tests, and run them **once, at the end**.
   - Before you run them, check the CPU load:
     `powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`.
   - Above about 40 %: say so and wait.

## Lessons from rounds 6, 7 and 8 (read them)

- Level 2 after each round found real breaks from shared changes: a new button reusing another's class, a box added at
  the top of a shared list that shifted code finding things by position, a rule meant for one kind catching another, a
  redraw at a tour's start drawing the card's ✦ and Azu during the tour, and older rounds' checks of templates and
  defaults that the round changed. So:
  - give every new element its own class;
  - when you add to a shared list or screen (the ✦ menu, Settings, the dock's thread, the agent's tools), grep for code
    and tests that find things in it by position, by count or by class, and run those test files too;
  - when a rule is meant for one case, say so in its condition;
  - check the state right after a mode starts (a tour, the pages, a sheet): what the redraw before the mode drew.
- **Say which shared code you changed** (anything outside `src/guidance/`), and run the test file of each module you
  touched in level 1.
- A check that reads a state the instant after an action races the app's timers: set and read in one call, or wait with
  `until(...)` for the state (round 8's fixes of test8q and test9b).
- Keep test9m under 300 s. Press rows inside the page where the self-test does; one real click per kind of control.
- The self-test: one big tab per walk. Never `-tabs guide,words` (65 min).

## Tests (level 1 for a guidance round: about 25-30 min)

- `tests/test9m.js` (Playwright with the installed Edge) checks:
  - the ✦ menu's Things to try (Shown / Hidden), the sheet's rows from the App items that teach, the count, the ticks
    from Done when, the all-done line;
  - each Show me from the sheet: the gesture on its target, on the card and on the wall, taking the visitor there first;
  - Tips Off: no pushed tip, Things to try still there and its Show me still plays; Essentials only likewise;
  - In Things to try per item;
  - "how do I…" offline in EN and PT: the matching item's words and a Show me card that plays; a history question gets no
    show_how card;
  - the show_how tool's schema and result (called in the page, without the network);
  - Settings › The guide's link; Fewer like this's toast line on an App group, not on a Content group;
  - The visitor's line;
  - every new row's demo;
  - the four scenes.
  - Waits use `until(...)` with a cap; never a fixed wait over 0.5 s.
- A mutation (`MUTATE=1`): for example, Tips Off also hiding Things to try, so the named checks turn red. Then restore.
- At the end, one run at a time, never two at once:
  1. `node run_tests.js --only=test9m,test9e,test9f,test9g,test9h,test9i,test9j,test9k,test9l,test8s,test8t,test8u,<the test
     files of any other shared module you touched>` (never the `_live` files).
  2. `MUTATE=1 node tests/test9m.js`
  3. From PowerShell, not Git Bash, each as its own call: `powershell -ExecutionPolicy Bypass -File tests/runtest8.ps1
     -tabs guide`, then `-tabs card`. Each must give bad=0.
- Change another file's check only where this round rightly changed what it counts (the ✦ menu's items, the scenes, the
  tools), and list each changed check and why.
- Do not run `--changed`, the full `run_tests.js` or the self-test of every tab.
- Update `PROTOTYPE_STRUCTURE.md` (the modules' rows and their tests column, with test9m), `README.md` (test9m) and
  `tests_baseline.json`.

## Limits

- Edit only files under lab_src. Do not commit, do not publish.
- No keys, voicebench, mascot model files or Unity. Do not call the live model.
- No CSS-only effects. Name each one's Unity path in the report (the sheet: a UI Toolkit panel like the card's sheets;
  show_how: a tool in the agent's tool list, its result a link card).
- One home per feature: extend the existing modules. No wrappers, and no re-assignment of another module's function (the
  build refuses them).
- Write patch scripts with the Write tool: bash heredocs mangle apostrophes.
- If a usage limit stops you, your work is saved on disk. Say in your last message which parts are done.

## Report (your final message)

- What was built, part by part, with the Unity paths.
- The new rows and their defaults; the checklist's lines (EN and PT); the show_how tool's description.
- The shared code you changed (outside `src/guidance/`), and which test files cover it.
- The captures (paths), what you judged and what you fixed.
- The tests: test9m, the mutation, and every check you changed in another file and why.
- The CPU load.
- The results of the end-of-round runs, with each file's time.
- 3-5 specific questions for the developer about the look and the flow.
