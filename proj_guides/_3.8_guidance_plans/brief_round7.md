You are building round 7 of the guidance system in a browser prototype, the "Block Components Lab". It lives in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app,
so every feature must be buildable in Unity: name its Unity path. Work only in the Lab; do not touch any Unity project file.

READ FIRST (every line):
- `../../_3.8_Guidance_Study.md`:
  - section 8, round 7 (this round);
  - section 3.1, the default library (Fun › Moments, Fun › Play);
  - section 3.2, the item's fields (Only if: not on Solemn points; Lively);
  - section 3.3, the score (Fun's row of the Mix: 5 / 10 / 25) and learning (a dismissed Fun item rests its group 5 min);
  - section 5, the row "Moments";
  - section 7, the tree (4 · Fun, "Solemn points: off"; Per point: Specific Marker > Guide script) and the row demos;
  - section 9, the decisions (Fun is a bubble only, never spoken).
- `PROTOTYPE_STRUCTURE.md`: the rules, the modules, "Where a change goes" and the row-demo contract.
- `README.md`: "Prove it" and "Levels", including the short level 1.
- `../../skills/Feature_Prototyping.md`: step 3.3 (look until it is right, then test) and step 5.
- `brief_round6.md`, for the shape and the way of working, which stay the same.
- Rounds 1-6, which this round extends in place: `src/guidance/*.js` and `tests/test9e.js` to `tests/test9j.js`. In
  particular the catalog's kinds and groups, the Mix's Fun row (round 5, "no items yet"), the templates (round 6's Moment),
  the Only if chips (Solemn, Lively) and the simulator's lanes.
- What exists today and moves in:
  - Azu's Moments: the water break and the cookie, between tour steps (`src/mascot/azu2d.js`: `azMoment`, `momentAllowed`,
    `S.guideCfg.moments`, `waterAfter`), and their rows in Guide › Mascot.
  - Azu's jokes after pokes (`mascot_joke_*`, `azu2d.js`).
  - The Solemn point row (`S.m.solemn`, Specific Marker) and Liveliness (`S.guideCfg.life`).
  - The kids' treasure hunt in Ask about the view (`src/world/camera-ask.js`: `G.cam.hunt`, `cam_hunt`, `cam_found`).
  - Specific Marker › Guide script (where a point's own lines live).
  Their tests: find them with `grep -ln "azMoment\|moments\|mascot_joke\|cam_hunt\|solemn" tests/*.js`, and read them.
- The spec `../../_3.3_POI_Card_Styles.md`, section 11 "Guidance": read it, do not edit it (the Architect does).

## This round

**7.1 Fun › Moments move in.**
- The water break and the cookie become items of a group **Fun › Moments**: form Moment (Azu's prop and bubble, as
  today), During a tour Between steps, Lively only, not on Solemn points, today's caps (once a card, 5 min apart).
- Their old timers go; the scheduler decides, with its gates and the Mix's Fun weight. The look and the props stay exactly
  as today.
- Guide › Mascot's Moments rows become link rows, "Edited in Guide › Guidance › Fun › Moments", as round 3 did for Azu
  comments. Their values move into the items (Water break after → the water item's own row).
- Today's Moments tests stay green. Change them only where a timer became an item, and list each changed check and why.

**7.2 Fun › Play.**
- **A joke** (One of these: the least seen shows): Azu's poke jokes become this item's variants. A joke shows after pokes
  (as today) and also, rarely, at a pause on the wall late in the visit (Along the visit Late). Bubble only.
- **Jokes per point:** Specific Marker › Guide script gets **Jokes** (0-3 lines per point, per language). When that point
  settles in the middle, its own joke can show (Fun, once a visit a point). A point with no joke uses none (never a generic
  one in its place). Give 2-3 demo points a joke each, short and kind, about what is painted there.
- **Treasure hunt?** (Invitation, Family audience only, once a visit, after the visitor has opened a card): "Want a treasure
  hunt? Find {thing} on the wall." A tap starts today's hunt in Ask about the view (the same `G.cam.hunt`, no copy); done
  when a hunt is found.
- **Seasonal** (One of these): a line that shows only in its window of dates (for example Santo António in June, Christmas
  in December, the 1st of November for the 1755 earthquake, which is Solemn: never a joke). Each variant has a From and To
  date. Preview & Test gets **Today is** (a date picker, default the real date) so the Editor can try each season.

**7.3 Solemn points and Fun.**
- Fun never shows on a Solemn point: no Moment, no joke, no hunt invite, no seasonal line. This is a fixed gate for the Fun
  kind (no option breaks it), and Why not now? says it in words ("a Solemn point: no fun here").
- Calm (Liveliness) allows only items without Lively only.
- The Fun kind row: On, its cap and gap, its weight read-back, "Solemn points: off" written beside it (not an option).

**7.4 The Editor and the simulator.**
- 4 · Fun shows its groups and items like App and Content, with + Add guidance, Duplicate, Delete with Undo and drag
  (round 6's form, reused).
- The Mix's Fun row loses "no items yet". The simulator's Fun lane shows dots. Add to the scripted visitors: Tour lover
  gets Moments between steps; a sixth visitor **Family on a Solemn wall** (a Solemn point in the middle for 5 minutes, then
  a Lively one): no Fun dot while the Solemn point is in the middle.
- Every new row has its demo. Most show the item on the phone with the new value; Today is shows the season's line.

**7.5 A look fix from round 6's captures.**
- On the card, past the hero, the ✦ and Azu beside it cover the text "Go to this part ›" at the bottom (capture
  `scratchpad\g6\p_ba_finger_mid_light.png`). The ✦ must never cover words (0 px over text): move the ✦ (and Azu) to clear
  the text or the text to clear them, as the spec's ✦ placement says ("above the Navigation Bar"). Capture before and after.

**7.6 Scenes in Ask Azu's console**, each with its ✓ check:
- "A water break between steps".
- "A joke about this point".
- "Treasure hunt for a family".
- "A Solemn point: no fun".
- "Christmas on the wall" (Today is set to a December date).

## How you work (the same as round 6)

1. After each change: only `python build.py`. Its check must read 0/0/0/0.
2. Then run it, capture it, and judge each capture yourself.
   - Captures: 390 px, light and dark.
     - Phone: the water break and the cookie through the scheduler; a joke after pokes; a point's joke; the treasure hunt
       invite and the hunt started; a seasonal line; a Solemn point with Why not now?; the ✦ fix before and after.
     - Editor: 4 · Fun with its groups; Guide script's Jokes; Today is; Guide › Mascot's link rows; the simulator's Fun
       lane and the sixth visitor.
   - Judge:
     - one focal point;
     - nothing covering its target, the Navigation Bar, the talk button or other words (0 px over text);
     - text 4.5:1 or more; 44 px targets;
     - the jokes kind and short (one line, two at most);
     - premium, modern, minimal.
   - Fix and capture again until it is right.
   - Save the final captures in
     `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g7\`.
3. Only then write the tests, and run them **once, at the end**.
   - Before you run them, check the CPU load:
     `powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`.
   - Above about 40 %: say so and wait.

## Lessons from round 6 (read them)

- Level 2 after round 6 found 4 older files broken by shared changes: a new button reusing another's class (`vhow`), a box
  added at the top of the dock's conversation that shifted every answer's trace, and a tip rule that also caught cues. So:
  - give every new element its own class;
  - when you add to a shared list (the dock's thread, Settings, a scene list), grep for code that finds things in it by
    position or by class, and run that code's test file too;
  - when a rule is meant for one kind, say so in its condition.
- **Say which shared code you changed** (anything outside `src/guidance/`), and run the test file of each module you
  touched in level 1. The Architect then decides on level 2.
- Keep test9k under 300 s. A Guide tab redraw costs about 1.1 s, so press rows inside the page where the self-test does,
  and keep real clicks to one per kind of control.

## Tests (level 1, the short one: about 5-10 min)

- `tests/test9k.js` (Playwright with the installed Edge) checks:
  - Moments through the scheduler: between steps only, Lively only, never on a Solemn point, once a card, 5 min apart;
  - the old Moments timers gone (no Moment without the scheduler);
  - a joke after pokes, its variants rotating; a point's joke when it settles; no generic joke for a point without one;
  - the treasure hunt invite: Family only, after a card, once a visit; its tap starts the real hunt; done when found;
  - each seasonal window with Today is, and the 1st of November never a joke;
  - the Solemn gate for every Fun item, with Why not now?'s words; Calm and Lively only;
  - the Fun lane and the sixth visitor's timeline (Instant mode, in the page);
  - Guide › Mascot's link rows;
  - the ✦ fix: 0 px over text on the card past the hero;
  - every new row's demo;
  - the five scenes.
  - Waits use `until(...)` with a cap; never a fixed wait over 0.5 s.
- A mutation (`MUTATE=1`): for example, the Solemn gate removed for Fun, so the named checks turn red. Then restore.
- The tests of what moved in (Moments, the jokes, the hunt, Solemn: the files your grep found) and `test9j.js` must stay
  green. Change them only where a timer or a row moved, and list each changed check and why.
- At the end, one run at a time, never two at once:
  1. `node run_tests.js --only=test9k,test9j,<the files your grep found>`
  2. `MUTATE=1 node tests/test9k.js`
  3. From PowerShell, not Git Bash: `powershell -ExecutionPolicy Bypass -File tests/runtest8.ps1 -tabs guide,words`. It must
     give bad=0. Then a separate call: `-tabs marker` (Specific Marker's Guide script). One big tab per walk.
- Do not run `--changed`, the full `run_tests.js` or the self-test of every tab.
- Update `PROTOTYPE_STRUCTURE.md` (the modules' rows and their tests column, with test9k), `README.md` (test9k) and
  `tests_baseline.json`.

## Limits

- Edit only files under lab_src. Do not commit, do not publish.
- No keys, voicebench, mascot model files or Unity.
- No CSS-only effects. Name each one's Unity path in the report (the props and Azu's reactions are the Rive file's states;
  the bubble as in round 6).
- One home per feature: extend the existing modules. No wrappers, and no re-assignment of another module's function (the
  build refuses them).
- Write patch scripts with the Write tool: bash heredocs mangle apostrophes.
- If a usage limit stops you, your work is saved on disk. Say in your last message which parts are done.

## Report (your final message)

- What was built, part by part, with the Unity paths.
- The new rows and their defaults; the demo jokes and seasonal lines (EN and PT).
- The shared code you changed (outside `src/guidance/`), and which test files cover it.
- The captures (paths), what you judged and what you fixed.
- The tests: test9k, the mutation, and every check you changed in another file and why.
- The CPU load.
- The results of the end-of-round runs, with each file's time.
- 3-5 specific questions for the developer about the look and the flow.
