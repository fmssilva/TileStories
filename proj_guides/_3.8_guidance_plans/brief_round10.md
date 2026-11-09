You are building round 10, the last round of the guidance plan, in a browser prototype, the "Block Components Lab". It lives
in `C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3
app. Work only in the Lab; do not touch any Unity project file.

READ FIRST (every line):
- `../../_3.8_Guidance_Study.md`: section 8, round 10 (this round), and section 7, "Each row's demo" (the developer's rule).
- `PROTOTYPE_STRUCTURE.md`: the rules, the modules, "Where a change goes", and above all **"Each row's demo"** (the
  `rowDemos({key: spec})` contract, the spec's fields, the one runner `showRowDemo`, `NO DEMO` in the self-test).
- `src/editor/demo.js` (the runner and the spec's fields) and `src/editor/selftest.js` (where `NO DEMO` is decided: today
  only the Guide tab and Detail Card › 6 · Audiences).
- Two modules that already declare their rows' demos well, as models: `src/guide/*.js` (for example the dock's and the
  ✦'s rows) and `src/audiences/audiences.js`.
- `README.md`: "Prove it" and "Levels".
- `../../skills/Feature_Prototyping.md`: step 3.3 (look until it is right, then test) and step 5.
- `brief_round9.md`, for the shape and the way of working, and its "Lessons" section.
- The spec `../../_3.3_POI_Card_Styles.md`: section 3 (the Editor's organisation) and section 15 (Testing). Read them, do
  not edit them (the Architect does).

## This round

**10.1 Every Editor row gets its demo.**
- The rule (the developer's): a row of the Editor shows its own demo on the phone. A changed value, or a press on the
  row's label (▸), puts the phone where that row acts and shows it: the right view (card, story, wall, a page, the
  marker), the right state (the stop, the scroll, the block in view), the change happening, and the part it changes
  ringed (`ring`).
- Today only the Guide tab and Detail Card › 6 · Audiences have one for every row. About 148 rows in the other tabs and
  sections have none: the Detail Card's other sections (Foundations, Card, Sections, Blocks & Components, Motion &
  Effects, Card Controls), the Specific Marker tab, the Global Scene tab, and any other tab with option rows.
- First, **list them**: extend the self-test's `NO DEMO` check to every tab (step 10.2), run it once, and save the list
  of rows without a demo (tab, section, row's label, key) to `scratchpad\g10\nodemo_before.txt`. Report the count.
- Then give each one its demo **in the row's own module**, by `rowDemos({key: spec})`. Never special code in the runner;
  never a demo declared outside the row's module. If a kind of demo needs a field the spec lacks (for example, opening a
  page or a block by id), add that field to `showRowDemo` once, generally, and document it in `PROTOTYPE_STRUCTURE.md`.
- A demo shows the row's own effect, not a generic view: a type row shows the text it sets with the ring on it; a motion
  row plays that motion once; a block's option opens that block in view; a marker row shows the marker. Rows of the same
  kind (for example each block's options) can share one spec built by a small function in that module.
- Rows that change nothing visible on the phone (an Editor-only setting, a key, a file path) get no fake demo: put them
  in the self-test's list of exemptions, each with its reason in words, and list them in the report. Keep exemptions rare
  (aim: under 10).

**10.2 The self-test fails any row without one.**
- `NO DEMO` covers every tab and section (minus the listed exemptions). After 10.1, the self-test of each tab must give
  bad=0.

**10.3 Split test9d** (it takes 570 s of its 600 s limit on round 9). Split it into two files by its own sections (for
example the guide's rows and the ✦'s / the mascot's), each under 300 s, with the same checks: no check lost or weakened.
List which checks went where.

**10.4 The Editor stays fast.** Every demo must start within one frame of the row's change (no wait before; the ring
after 60 ms, as round 6 set). Measure the Guide tab's and the Detail Card tab's draw time before and after; they must not
grow by more than 10 %.

## How you work (the same as round 9)

1. After each change: only `python build.py`. Its check must read 0/0/0/0.
2. Capture a sample, judge it, fix it.
   - Captures (390 px, light and dark): one demo of each kind of row (a type row, a colour row, a motion row, a block's
     option, a section row, a Card Controls row, a marker row, a Global Scene row), each mid-demo with its ring.
   - Judge: the demo shows the row's own change; the ring is on the part that changed; one focal point; nothing covering
     the words it shows.
   - Save the final captures in
     `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g10\`.
3. Only then write the tests, and run them **once, at the end**.
   - Before you run them, check the CPU load:
     `powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`.
   - Above about 40 %: say so and wait.

## Lessons from rounds 6-9 (read them)

- Give every new element its own class; when you add to a shared list or screen, grep for code and tests that find
  things in it by position, by count or by class, and run those test files too.
- A check that reads a state the instant after an action races the app's timers: set and read in one call, or wait with
  `until(...)` for the state.
- **This round touches many modules** (every tab's rows). So its level 1 is wide by nature: see the Tests section.
- The self-test: one big tab per walk. Never `-tabs guide,words` (65 min).

## Tests

- `tests/test9n.js` (Playwright with the installed Edge), under 300 s: for a sample of each kind of row (at least two per
  tab section), a change and a ▸ press show the demo (the phone in the right view, the ring on the right part); and the
  self-test's `NO DEMO` covers every tab (a row's demo removed in the page makes it report `NO DEMO`).
- A mutation (`MUTATE=1`): one tab's rows dropped from the `NO DEMO` check, so the named checks turn red. Then restore.
- The split of test9d: both new files green, and the old file removed from `run_tests.js`'s list and from
  `tests_baseline.json`.
- At the end, one run at a time, never two at once:
  1. From PowerShell, not Git Bash, each as its own call: `powershell -ExecutionPolicy Bypass -File tests/runtest8.ps1
     -tabs card`, `-tabs guide`, `-tabs marker,global`, `-tabs words`. Each must give bad=0.
  2. `node run_tests.js --only=test9n,<the two files split from test9d>,test9e,test8l`
  3. `MUTATE=1 node tests/test9n.js`
- Do not run the full `run_tests.js`: the Architect runs level 3 (everything, every mutation, a new baseline) right after
  this round.
- Update `PROTOTYPE_STRUCTURE.md` (the row-demo contract now covers every tab; any new spec field; the tests column),
  `README.md` (test9n, the split) and `tests_baseline.json`.

## Limits

- Edit only files under lab_src. Do not commit, do not publish.
- No keys, voicebench, mascot model files or Unity.
- One home per feature: each row's demo in its row's module. No wrappers, and no re-assignment of another module's
  function (the build refuses them).
- Write patch scripts with the Write tool: bash heredocs mangle apostrophes.
- If a usage limit stops you, your work is saved on disk. Say in your last message which parts are done.

## Report (your final message)

- The count of rows without a demo before (with the list's path) and after (0), and the exemptions with their reasons.
- The demos by kind, with the module of each; any new spec field.
- The split of test9d: which checks went where, and each file's time.
- The Editor's draw times before and after.
- The captures (paths), what you judged and what you fixed.
- The tests: test9n, the mutation, and every check you changed in another file and why.
- The CPU load and the results of the end-of-round runs, with each file's time.
- 3-5 specific questions for the developer.
