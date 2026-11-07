You are building round 4 of the guidance system in a browser prototype, the "Block Components Lab", in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app,
and every feature must be buildable in Unity: name its path. Work in the Lab only: no Unity project file is touched.

READ FIRST (all lines):
- `../../_3.8_Guidance_Study.md`:
  - this round is section 8, round 4;
  - section 1, points 1-2 (push and pull);
  - section 3 (the levels, the group types, the Cue and Walkthrough forms);
  - section 5, the rows Card Controls > Hints and How to use the card;
  - the decisions in section 9, point 4.
- `PROTOTYPE_STRUCTURE.md`: the rules, the modules, "Where a change goes" and the row-demo contract.
- `README.md`: "Prove it" and "Levels".
- `../../skills/Feature_Prototyping.md` step 3.3 (look until it is right, then test) and step 5.
- `brief_round3.md`, for the shape and the way of working, which stays the same.
- Rounds 1-3, which this round extends in place: `src/guidance/*.js` and `tests/test9e.js` to `tests/test9g.js`.
- Today's card Hints and How to use the card, which this round moves in:
  - `src/card/hints.js`: `HINTS`, `S.hints`, `S.hintDone`, `S.hintBusy`, their timers and moments, and How to use the card
    (`HOW`, the steps run through the tour);
  - its Editor rows: Card Controls > Hints;
  - its test: `tests/test8g.js`.
- The ✦ menu in `src/world/guide-button.js`.
- The spec `../../_3.3_POI_Card_Styles.md`: read the Hints and How to use the card, and section 11 "Guidance". Do not edit it
  (the Architect does).

## This round

**4.1 The card Hints become App › The card (cues).**
- One item per hint in `HINTS`, form **Cue**: motion on the element, no mascot, no bubble unless the hint has words today.
- Their moments and their looks stay as they are today.
- Their own rules become the item's rules: when it may come, how often, Reduce Motion, done when.
- The old one-at-a-time and timer code in `card/hints.js` goes; the scheduler decides, through the gates of rounds 1-3.
- What changes:
  - one memory for the wall and the card;
  - a hint and a tip never at the same time;
  - a cue never within the breathing room of a tip, in either order.
  - Does the breathing room apply to cues too? Decide, and say it in the report: a cue is small motion, so it may need a
    shorter room. If you give it one, make it a row.
- The Editor:
  - the rows are edited in Guide › 4 · Guidance › App › The card;
  - Card Controls > Hints stays as a **view of the same data**: each row reads "Edited in Guidance" and links there. One home
    for the data; the view only reads and links.
  - The hints' On / Off stays where the visitor's Settings have it today.

**4.2 How to use the card becomes App › How to use the card.**
- A group of type In order, form **Walkthrough**, **pulled only**: never scheduled, never capped.
- It opens from Settings, as today, and from the ✦ menu's new item (4.3).
- Its steps are today's `HOW`; it still runs through the tour's steps.
- Each step is a row of the group, with its words in Languages & Terms.
- Its memory: "seen" retires the matching card cue (for example, the grabber step done means the grabber nudge is done).

**4.3 The ✦ menu gets "How does this work?"**
- An item that starts How to use the card.
- On the card it walks the card. On the wall it says, in one bubble, how the wall works (point, tap a marker), then offers
  to open a card.
- Pull: it shows at once.
- Words in en and pt, owned by its group's foldout.

**4.4 Scenes in Ask Azu's console**, each with its ✓ check:
- "A hint and a tip never at once": a card cue is due while a tip shows, and the cue waits its room.
- "How does this work?": from the ✦ menu, on the card and on the wall.
- The card's existing hint scenes, if any, keep passing.

## How you work (the same as round 3)
1. After each change: only `python build.py`, and its check must be 0/0/0/0.
2. Run the page, capture it, and judge each capture yourself.
   - Captures, at 390 px, light and dark:
     - a card cue (the grabber nudge, the Locate label, the strips);
     - a cue waiting while a tip shows;
     - How to use the card from the ✦ menu;
     - "How does this work?" on the wall;
     - the Editor's App › The card and How to use the card;
     - the Card Controls view with "Edited in Guidance".
   - What to judge: one focal point; nothing covering its target or other words (0 px over text); text 4.5:1 or more;
     44 px targets; the bubble not under the ✦ or the nav bar; premium, modern, minimal.
   - Fix and capture again until the look is right.
   - Save the final captures in
     `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g4\`.
3. Only then write the tests, and run them **once, at the end**.
   - Before each run, check the CPU load:
     `powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`.
   - Above about 40 %, say so and wait.

## Tests
- `tests/test9h.js` (Playwright with the installed Edge; the visit clock moved by hand, as test9g does). It checks:
  - each cue's rules alone on `gdPick` (pure);
  - a cue and a tip never at once;
  - the room between them, in either order;
  - How to use the card pulled from Settings and from the ✦, and no gate stopping it;
  - a walkthrough step retiring its cue;
  - "How does this work?" on the card and on the wall;
  - the Card Controls view: it reads "Edited in Guidance", and its link opens the group;
  - every new row's demo;
  - the two scenes.
  - Waits use `until(...)` with a cap. Never a fixed wait over 0.5 s.
- A mutation (`MUTATE=1`): for example, cues skip the one-at-a-time gate, so "never at once" turns red. Then restore.
- `tests/test8g.js` (today's Hints) must stay green. Change it only where the timer moved into the scheduler, and list each
  changed check and why. The other test files set `S.hints` to off in their setup; keep that switch working, so you do not have
  to touch them.
- At the end, one run at a time, never two at once:
  1. `node run_tests.js --changed`;
  2. `MUTATE=1 node tests/test9h.js`;
  3. from PowerShell, not Git Bash:
     `powershell -ExecutionPolicy Bypass -File tests/runtest8.ps1 -tabs card,guide,words`. It must give bad=0.
  - Do not run the full `run_tests.js` or the self-test of every tab. The Architect runs the wider levels.
- Update `PROTOTYPE_STRUCTURE.md` (the modules' rows and their tests column, so `--changed` finds test9h), `README.md` (test9h)
  and `tests_baseline.json`.

## Limits
- Edit only files under lab_src. Do not commit, and do not publish.
- No keys, no voicebench, no mascot model files, no Unity.
- No CSS-only effects. Name each effect's Unity path in the report.
- One home per feature: extend the existing modules. No wrappers, and no re-assignment of another module's function (the
  build refuses them).
- Remove the old timers; do not leave them switched off.
- Write patch scripts with the Write tool: bash heredocs mangle apostrophes.
- If a usage limit stops you, your work is saved on disk. Say in your last message which parts are done.

## Report (your final message)
- What was built, part by part, with the Unity paths.
- The new rows and their defaults.
- Your decision on the room for cues, and why.
- The captures (paths), what you judged, and what you fixed.
- The tests: test9h, the mutation, and every check you changed in another file, with why.
- The CPU load when the tests ran.
- The results of the three end-of-round runs.
- 3-5 specific questions for the developer about the look and the flow.
