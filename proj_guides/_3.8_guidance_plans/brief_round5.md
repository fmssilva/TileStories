You are building round 5 of the guidance system in a browser prototype, the "Block Components Lab", in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app,
and every feature must be buildable in Unity: name its path. Work only in the Lab; do not touch any Unity project file.

READ FIRST (all lines):
- `../../_3.8_Guidance_Study.md`:
  - this round is section 8, round 5;
  - the score, the stage and Talkativeness: section 3.3, points 3-6;
  - the Editor's tree, the presets, the row demos and the scripted visitors: section 7;
  - the decisions: section 9.
- `PROTOTYPE_STRUCTURE.md`: the rules, the modules, "Where a change goes" and the row-demo contract.
- `README.md`: "Prove it" and "Levels". Levels was cut on 2026-11-07; read it again.
- `../../skills/Feature_Prototyping.md`: step 3.3 (look until it is right, then test) and step 5.
- `brief_round4.md`, for the shape and the way of working, which stay the same.
- Rounds 1-4, which this round extends in place: `src/guidance/*.js` and `tests/test9e.js` to `tests/test9h.js`. In
  particular `GD_MIX` (the fixed kind-weight table of round 2), `gdScore`, `gdStage`, and the Preview & Test section.
- Audiences: `src/audiences/audiences.js` (Looks per audience) and its test `tests/test8l.js`.
- The spec `../../_3.3_POI_Card_Styles.md`: read section 11 "Guidance". Do not edit it; the Architect does.

## This round

**5.1 The Mix table becomes editable.** It lives in Overall › **Mix along the visit**.
- It is a 3 × 3 table: rows are the kinds, columns are the stage at s = 0 / 0.3 / 1.
- Today it holds App 70 / 40 / 15 and Content 25 / 50 / 60. Add a Fun row at 5 / 10 / 25, ready for round 7. Fun has no
  items yet, so say so beside its row, and add no empty kind.
- Under the table, draw its curve: one line per kind, straight segments between the three points.
- `GD_MIX` stops being a constant. It becomes data in `S.guidance`, read by `gdScore`.
- Each kind row's weight read-back (round 2) follows the table.
- Round 2's `gdScore` tests must stay green with the default table.

**5.2 Talkativeness: one knob over the numbers.** Quiet / **Balanced** / Chatty, in Overall.
- It scales the breathing room, At most a minute, At most a visit and the kinds' caps.
- Write the factors as a table in the Editor's help, so they are read in words. Suggested factors:
  - Quiet: room ×2, the limits ×0.5;
  - Chatty: room ×0.6, the limits ×1.5.
  Judge them with the simulator (5.4).
- Balanced is today's values.
- The rows show the value in force, with the knob's effect in words, for example "15 s × 2 = 30 s (Quiet)".

**5.3 Presets.** In Overall, **Preset**: First visit / Returning / Family / Expert / Quiet / Chatty / Custom.
- A preset writes the Overall and kind values (Talkativeness, the room, the limits, the Mix, Remember). It never writes the
  items.
- Changing any of those values by hand makes the preset Custom. Custom lists its changes in words ("Breathing room 20 s,
  was 15 s").
- The values of each preset go in one table in the module, and the Editor's help reads them back.
- Audiences > Looks per audience gets a row **Guidance preset** per audience: Family → Family, Explorer → First visit,
  Expert → Expert. The audience chosen in Who's exploring then sets the preset.

**5.4 The simulator.** In Preview & Test, **Simulate a visit**.
- A timeline from 0 to 20 min, with lanes App / Content / Fun / Quiet. Quiet means the gates held: a tour, panning, a card
  open.
- It runs the real `gdPick` on the fake visit clock (no copy of the scheduler), with a scripted visitor:
  - **First visit**
  - **Fast learner** (opens a card early, hears a tour step)
  - **Idle** (ignores every tip)
  - **Tour lover** (starts tours)
  - **Expert** (a returning visitor who has used everything)
- Each dot is a shown item. A tap on a dot (or scrubbing to it) shows that item on the phone at that moment. A skip shows
  its reason in words, the scheduler's own.
- Speeds: ×1, ×10, ×30, and Instant (the whole 20 min at once, for the timeline).
- A summary line under it, for example "11 tips in 15 min: App 7, Content 4; at most 2 a minute; none over a sentence".
- The script and its result are deterministic: the same script gives the same timeline.

**5.5 The ×10 demos.** Every Overall and kind row gets its demo as the study says: the row's change re-runs the simulator
for two visit-minutes and highlights what moved. Round 2's ×10 chip demos stay for the rows that have them.

**5.6 Scenes in Ask Azu's console**, each with its ✓ check:
- "Quiet preset": fewer tips in 15 min than Balanced, and the room is longer.
- "Family audience": choosing Family in Who's exploring sets the Family preset.
- "Simulate a first visit": App first, then Content, about 10 items in 15 min, at most 2 a minute.

## How you work (the same as round 4)
1. After each change: only `python build.py`. Its check must read 0/0/0/0.
2. Then run it, capture it, and judge each capture yourself.
   - Captures, at 390 px for the phone, light and dark:
     - the simulator's timeline (each script);
     - a dot tapped, with its item on the phone;
     - the Mix table and its curve;
     - Talkativeness and the Preset row, and Custom with its changes;
     - the audience's Guidance preset row.
   - What to judge:
     - one focal point;
     - the timeline readable at the Editor's width, with every dot at least 10 px apart or merged into a count;
     - text at 4.5:1 or more; targets of 44 px on the phone;
     - premium, modern, minimal.
   - Fix and capture again until it is right.
   - Save the final captures in
     `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g5\`.
3. Only then write the tests, and run them **once, at the end**.
   - Before you run them, check the CPU load:
     `powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`.
   - Above about 40 %, say so and wait.

## Tests (level 1, the short one: 5-7 min)
- `tests/test9i.js` (Playwright with the installed Edge) checks:
  - the Mix table: a change moves the score, pure on `gdScore`;
  - each Talkativeness level's numbers;
  - each preset's values, Custom and its list, and the audience's preset;
  - each scripted visitor's timeline against its expected counts and order. Compute it with the simulator's Instant mode, in
    the page, with no real-time wait;
  - a dot tapped shows its item;
  - the ×10 demos;
  - every new row's demo;
  - the three scenes.
  - Waits use `until(...)` with a cap, never a fixed wait over 0.5 s.
- A mutation (`MUTATE=1`): for example, the Mix table ignored (always Balanced's), so the Quiet / Chatty or Mix checks turn
  red. Then restore.
- `test9f.js` (round 2's score) and `test8l.js` (Audiences) must stay green. Change them only where a constant became data,
  and list each changed check and why.
- At the end, one run at a time, never two at once:
  1. `node run_tests.js --only=test9i,test9f,test8l`;
  2. `MUTATE=1 node tests/test9i.js`;
  3. from PowerShell, not Git Bash: `powershell -ExecutionPolicy Bypass -File tests/runtest8.ps1 -tabs guide,words`. It must
     give bad=0.
- Then a separate call: `-tabs card`, because Audiences lives in the Detail Card. One big tab per walk.
- Do not run `--changed`, the full `run_tests.js` or the self-test of every tab. The Architect runs the wider levels.
- Update `PROTOTYPE_STRUCTURE.md` (the modules' rows and their tests column, with test9i), `README.md` (test9i) and
  `tests_baseline.json`.

## Limits
- Edit only files under lab_src. Do not commit, do not publish.
- No keys, voicebench, mascot model files or Unity.
- No CSS-only effects. Name each one's Unity path in the report; the timeline in Unity is an IMGUI or UI Toolkit Painter2D
  drawing in the Editor window.
- One home per feature: extend the existing modules. No wrappers, and no re-assignment of another module's function (the
  build refuses them).
- Write patch scripts with the Write tool; bash heredocs mangle apostrophes.
- If a usage limit stops you, your work is saved on disk. Say in your last message which parts are done.

## Report (your final message)
- What was built, part by part, with the Unity paths.
- The new rows and their defaults.
- The Talkativeness factors and each preset's values, and why.
- Each scripted visitor's timeline summary.
- The captures (their paths), what you judged and what you fixed.
- The tests: test9i, the mutation, and every check you changed in another file, with why.
- The CPU load.
- The results of the end-of-round runs.
- 3-5 specific questions for the developer about the look and the flow.
