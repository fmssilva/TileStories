You are building round 8 of the guidance system in a browser prototype, the "Block Components Lab". It lives in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app,
so every feature must be buildable in Unity: name its Unity path. Work only in the Lab; do not touch any Unity project file.

READ FIRST (every line):
- `../../_3.8_Guidance_Study.md`:
  - section 8, round 8 (this round);
  - section 6, "Onboarding at the app's start: the same system" (the pages, in order);
  - section 5, the row "Who's exploring" (Where to ask);
  - section 3.1 (Sequences: First pages) and 3.2 (the Page form);
  - section 7, the tree (5 · Sequences) and the row demos;
  - section 9, the decisions.
- `PROTOTYPE_STRUCTURE.md`: the rules, the modules, "Where a change goes" and the row-demo contract.
- `README.md`: "Prove it" and "Levels", including the short level 1 and the rule "one big tab per walk".
- `../../skills/Feature_Prototyping.md`: step 3.3 (look until it is right, then test) and step 5.
- `brief_round7.md`, for the shape and the way of working, which stay the same, and its "Lessons" section.
- Rounds 1-7, which this round extends in place: `src/guidance/*.js` and `tests/test9e.js` to `tests/test9k.js`. In
  particular round 1's Opening message and Tap a marker (`catalog.js`), round 4's How to use the card (an In order group,
  pulled only) and round 6's item form and templates (Step of a sequence).
- What exists today and moves in:
  - Who's exploring today? on the first card: `src/audiences/audiences.js` (`onbShow`, `S.aud.ask`, `S.aud.seen`, Skip =
    Explorer) and its rows in Detail Card › 6 · Audiences; its test `tests/test8l.js`.
  - The audience sets the guidance preset (`audience.set`, round 5).
  - The preview bar under the phone: `src/phone/preview-bar.js` and `preview-tools.js`.
  - The mascot (2D and 3D sources, Mascot Off) and Reduce Motion.
- The spec `../../_3.3_POI_Card_Styles.md`, section 11 "Guidance" and section 11b "Audiences": read them, do not edit them
  (the Architect does).

## This round

**8.1 The first pages: a sequence of the Page form.**
- A group **Sequences › First pages** (In order, form Page; the scheduler is quiet while it runs), beside How to use the
  card in 5 · Sequences. Each page is an item with round 6's form; every page is skippable; the whole run takes under 30 s.
- The pages, as study section 6 says:
  1. **Hello:** Azu forms from a tile and waves: "Olá! I'm your guide to Lisbon's tiles." The language as a chip (EN /
     PT, it switches the app's language). Two buttons: **Let's go** and **Show me how it works** (it starts How to use the
     card's wall bubble after the pages, round 4).
  2. **Who's exploring** (only when Where to ask = At the app's start): the same three choices and Skip = Explorer as the
     first card's screen today, reused (one home: no copy). It sets the audience, and with it the guidance preset.
  3. **The camera:** "I need the camera to see the tiles. Nothing is recorded." with **Allow the camera**; then a stand-in
     for the system prompt (Allow / Don't allow). If refused: how to allow it, in words, with **Open Settings** (a stand-in
     in the Lab) and **Try again**; the app never goes on to the wall without the camera.
  4. **Straight to the wall:** the short Opening message ("Point at the tiles and tap a glowing marker"), then Tap a marker,
     as round 1 does.
- No carousel of benefits, no account, no tutorial of the card before a card exists.
- **Later launches:** no pages; a "Welcome back" (round 1's returning line). Remember across visits Off: every launch is a
  first.
- Reduce Motion: Azu appears (no forming from a tile, no wave); the words stay. Mascot Off: the pages without Azu.

**8.2 Who's exploring's Where to ask.**
- A new row in Detail Card › 6 · Audiences: **Where to ask**: At the app's start / **On the first card** (today's) /
  Never (Explorer).
- At the app's start: page 2 above asks, and the first card does not. On the first card: today's screen, and page 2 is
  skipped. Never: neither asks.
- test8l (today's first-card screen) stays green with the default.

**8.3 Start the app in the preview bar.**
- **Start the app** replays the first run: the visitor reset to a first visit (as Reset the visitor), the pages from
  Hello. A second button or a choice replays a later launch ("Welcome back").
- Preview & Test gets the same, and The visitor says whether the first pages were seen.

**8.4 The Editor.**
- 5 · Sequences shows First pages and How to use the card. First pages has its items (each page), drag to reorder (the
  order of the run; Hello stays first, Straight to the wall last: the field says why when a drag would break that), + Add
  guidance with the Page template (a page with a title, a line, up to 2 buttons).
- Every new row has its demo: a page row shows that page on the phone with the new value; Where to ask shows where the
  question appears; Start the app plays the run.

**8.5 Scenes in Ask Azu's console**, each with its ✓ check:
- "First run: Hello, Who's exploring, the camera, the wall".
- "Skip every page".
- "The camera refused, then allowed".
- "A later launch: Welcome back, no pages".

## How you work (the same as round 7)

1. After each change: only `python build.py`. Its check must read 0/0/0/0.
2. Then run it, capture it, and judge each capture yourself.
   - Captures: 390 px, light and dark.
     - Phone: each page; Hello's language chip in PT; the camera's prompt stand-in; the refused state; the wall after the
       pages with the Opening message; a later launch's Welcome back; Reduce Motion's Hello; Mascot Off's Hello.
     - Editor: 5 · Sequences with First pages open; a page's form; Where to ask; Start the app in the preview bar.
   - Judge:
     - one focal point; each page readable in 3 s;
     - nothing over the words (0 px over text); 44 px targets; text 4.5:1 or more;
     - the skip always visible and reachable with one thumb;
     - premium, modern, minimal: this is the app's first impression.
   - Fix and capture again until it is right.
   - Save the final captures in
     `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g8\`.
3. Only then write the tests, and run them **once, at the end**.
   - Before you run them, check the CPU load:
     `powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`.
   - Above about 40 %: say so and wait.

## Lessons from rounds 6 and 7 (read them)

- Level 2 after each round found real breaks from shared changes: a new button reusing another's class, a box added at
  the top of a shared list that shifted code finding things by position, a rule meant for one kind catching another, and
  a redraw at a tour's start drawing the card's ✦ and Azu during the tour. So:
  - give every new element its own class;
  - when you add to a shared list or screen (Settings, the dock's thread, the preview bar, the first card), grep for code
    that finds things in it by position or by class, and run that code's test file too;
  - when a rule is meant for one case, say so in its condition;
  - check the state right after a mode starts (a tour, the pages): what the redraw before the mode drew.
- **Say which shared code you changed** (anything outside `src/guidance/`), and run the test file of each module you
  touched in level 1.
- Keep test9l under 300 s. Press rows inside the page where the self-test does; one real click per kind of control.
- The self-test: one big tab per walk. Never `-tabs guide,words` (65 min).

## Tests (level 1, the short one: about 10 min)

- `tests/test9l.js` (Playwright with the installed Edge) checks:
  - the run in order: Hello, Who's exploring (when asked there), the camera, the wall with the Opening message;
  - each Skip, and skipping all;
  - the language chip switching the app's language;
  - Let's go and Show me how it works;
  - the camera allowed, refused, Open Settings, Try again; never the wall without the camera;
  - Where to ask: each of the three, with the first card asking or not;
  - the audience chosen on page 2 setting the guidance preset;
  - a later launch: no pages, Welcome back; Remember across visits Off: the pages again;
  - Reduce Motion and Mascot Off;
  - Start the app (both) and The visitor's line;
  - drag reorder within the rules; the Page template;
  - every new row's demo;
  - the four scenes.
  - Waits use `until(...)` with a cap; never a fixed wait over 0.5 s.
- A mutation (`MUTATE=1`): for example, the camera refused still going on to the wall, so the named checks turn red.
  Then restore.
- `test8l.js` (Audiences, the first card's screen) and `test9e.js` (round 1's opening and Tap a marker) must stay green.
  Change them only where a row moved, and list each changed check and why.
- At the end, one run at a time, never two at once:
  1. `node run_tests.js --only=test9l,test8l,test9e,<the test files of the shared modules you touched>`
  2. `MUTATE=1 node tests/test9l.js`
  3. From PowerShell, not Git Bash, each as its own call: `powershell -ExecutionPolicy Bypass -File tests/runtest8.ps1
     -tabs guide`, then `-tabs card` (Audiences). Each must give bad=0.
- Do not run `--changed`, the full `run_tests.js` or the self-test of every tab.
- Update `PROTOTYPE_STRUCTURE.md` (the modules' rows and their tests column, with test9l), `README.md` (test9l) and
  `tests_baseline.json`.

## Limits

- Edit only files under lab_src. Do not commit, do not publish.
- No keys, voicebench, mascot model files or Unity.
- No CSS-only effects. Name each one's Unity path in the report (Azu forming from a tile and waving: the Rive file's
  states; the pages: UI Toolkit screens; the camera prompt: the platform's permission request, `Application.
  RequestUserAuthorization(UserAuthorization.WebCam)` or AR Foundation's camera permission, with its refused path).
- One home per feature: extend the existing modules. No wrappers, and no re-assignment of another module's function (the
  build refuses them).
- Write patch scripts with the Write tool: bash heredocs mangle apostrophes.
- If a usage limit stops you, your work is saved on disk. Say in your last message which parts are done.

## Report (your final message)

- What was built, part by part, with the Unity paths.
- The new rows and their defaults; each page's words (EN and PT).
- The shared code you changed (outside `src/guidance/`), and which test files cover it.
- The captures (paths), what you judged and what you fixed.
- The tests: test9l, the mutation, and every check you changed in another file and why.
- The CPU load.
- The results of the end-of-round runs, with each file's time.
- 3-5 specific questions for the developer about the look and the flow.
