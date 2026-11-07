You are building round 3 of the guidance system in a browser prototype, the "Block Components Lab", in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app.
Every feature must be buildable in Unity: name its path. Work in the Lab only; do not touch any Unity project file.

READ FIRST (every line):
- `../../_3.8_Guidance_Study.md`: this round is section 8, round 3; the table "what each of today's parts becomes" (section 5,
  the rows Azu comments and The ✦ hint); the model in sections 3.1-3.4; the decisions in section 9.
- `PROTOTYPE_STRUCTURE.md`: the rules, the modules, "Where a change goes" and the row-demo contract.
- `README.md`: "Prove it" and "Levels". Levels was changed on 2026-11-06: you look first, then test once.
- `../../skills/Feature_Prototyping.md` step 3.3 (look until it is right, then test) and step 5.
- Rounds 1 and 2, which this round extends in place: `src/guidance/*.js` and `tests/test9e.js`, `tests/test9f.js`.
- Today's two parts that this round moves in, both in `src/world/guide-button.js`:
  - Azu's comments on the wall: the 400 ms timer, `G.fab.*`, `fab_comment_*`.
  - The ✦ hint: `gbHint` and its 1 s timer.
- Their Editor rows: `src/world/wall.js`, "Azu comments" and "Between comments", in Ask > Ask about the view.
- `brief_round2.md`, for the shape of a round.
- The spec `../../_3.3_POI_Card_Styles.md`, section 11 "Guidance": read it, do not edit it (the Architect does).

## This round

**3.1 Azu's comments become Content items.**
- Content › **What's in view**: one item per wall point. Today's numbers become its rules:
  - the group's gap is 7 s; Between comments becomes the group's gap row;
  - at most 12 a visit;
  - a point settles in the middle for 1.2 s, never while panning (0.7 s);
  - one per point a visit;
  - the 3 lines rotate, and a point with a card has its own line.
- A tap does what it does today: a point with a card starts its tour; otherwise Ask about the view opens about it.
- The old 400 ms timer and its `G.fab` counters go. The scheduler decides, through the same gates as every item (round 2).
- Spoken as the kind (Content: once the guide has been heard this visit, else a bubble).
- What changes:
  - A remark now yields to a more important item.
  - It never starts within the breathing room of a tip, and a tip never starts within it after a remark.
- Ask > Ask about the view > Azu comments stays, as a link row to Guide › 4 · Guidance › Content › What's in view (as the Wall
  hint row did in round 1). Its On / Off is the group's On.

**3.2 The ✦ hint becomes an App item.**
- App › The wall › **The ✦**:
  - after 4 pokes on Azu, or 25 s idle;
  - Lively only, never on a Solemn point;
  - once a visit;
  - done when the ✦ menu is opened.
- Its words are today's `gb_hint`. Azu points at the ✦ as today: `point-gb` for the 2D figure, `G.pointTo` for the 3D model.
- "Tap on Azu = Points to the ✦" still shows it at once, as asked. It is a pull: no gate but one at a time.
- The old 1 s timer and `G.gb.hinted` go.
- It and round 2's Ask about the view are both about the ✦: never both in one visit's first minute; the Editor's order decides.

**3.3 The Editor** (Guide > 4 · Guidance):
- the new group and items, each with round 2's rows and a demo (`rowDemos`);
- the item's own rows for what is special: the lines per point, the idle seconds, the pokes;
- Why not now? and the Log name them;
- every word in Languages & Terms with its owner (the item's foldout), en and pt;
- no placeholder rows.

**3.4 Scenes in Ask Azu's console** (the developer presses them), each with its ✓ check:
- "A remark yields": a tip is due as a point settles, so the tip shows and the remark waits its breathing room.
- "The ✦ after four pokes".
- Today's "Azu points at the ✦, once a visit" (`guide/guide-scenes.js`) keeps passing.

## How you work (decided 2026-11-06: a prototype's tests never cost more than building it)
1. After each change: only `python build.py` (the check must read 0/0/0/0).
2. Then run it and capture, and judge each capture yourself.
   - Captures: 390 px, light and dark. A remark as a bubble, and spoken after the guide has been heard. The ✦ hint beside the ✦.
     A remark waiting while a tip shows. The Editor's new group and item rows.
   - What to judge: one focal point; nothing covering its target or other words (0 px over text); text 4.5:1 or more; 44 px
     targets; the bubble not under the ✦ or the nav bar; premium, modern, minimal.
   - Fix and capture again until it is right.
   - Save the final captures in `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g3\`.
3. Only then write the tests, and run them **once, at the end**.
   - Before you run them, check the CPU load: `powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`.
     Above about 40 %, say so and wait. A busy machine gives false reds.

## Tests
- `tests/test9g.js` (Playwright with the installed Edge; the visit clock moved by hand, as test9f does):
  - each rule of the two items alone on `gdPick` (pure);
  - a remark never within the breathing room of a tip, either way;
  - the yield;
  - the pokes and the idle;
  - Tap on Azu shows the hint at once;
  - the ✦ menu ends it for the visit;
  - the link row;
  - every new row's demo;
  - the two scenes.
  - Wait with `until(...)` and a cap, never a fixed wait longer than 0.5 s.
- A mutation (`MUTATE=1`): for example, the breathing room ignored for remarks, so the yield check turns red. Then restore.
- The tests that cover today's remarks and hint must stay green, changed only where the timer moved into the scheduler: test8s,
  test8t, test8u, test8v, test9d, test9e, test9f. List each check you changed, and why.
- At the end, one at a time, never at the same time:
  1. `node run_tests.js --changed`: it lists and runs the files of every module you changed.
  2. `MUTATE=1 node tests/test9g.js`.
  3. `powershell -ExecutionPolicy Bypass -File tests/runtest8.ps1 -tabs guide,words`: run it from PowerShell, not Git Bash, or
     it hits TIMEOUT. It must give bad=0.
- Do not run the full `run_tests.js` (every file) or the self-test of every tab: the Architect runs the wider levels.
- Update `PROTOTYPE_STRUCTURE.md` (the modules' rows and their tests column, so `--changed` finds test9g), `README.md` (test9g)
  and `tests_baseline.json`.

## Limits
- Edit only files under lab_src. Do not commit and do not publish.
- No keys, voicebench, mascot model files or Unity.
- No CSS-only effects: name each effect's Unity path in the report.
- One home per feature: extend the existing modules. No wrappers, and no re-assignment of another module's function (the build
  refuses both). The old timers are removed, not left switched off.
- Write patch scripts with the Write tool (bash heredocs mangle apostrophes).
- If a usage limit stops you, your work is saved on disk. In your last message, say which parts are done.

## Report (your final message)
- What was built, part by part, with the Unity paths.
- The new rows and their defaults.
- The captures (paths), what you judged and what you fixed.
- The tests: test9g, the mutation, every check you changed in another file and why.
- The CPU load when the tests ran.
- The results of the three end-of-round runs.
- 3-5 specific questions for the developer about the look and the flow.
