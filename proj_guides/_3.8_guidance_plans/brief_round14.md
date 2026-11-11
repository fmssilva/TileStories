You are building round 14 in a browser prototype, the "Block Components Lab". It lives in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app:
the POI Editor's Guide tab here becomes an IMGUI / UI Toolkit tab in Unity, so every organization choice must be buildable
there (foldouts, rows, a link that opens another tab at a foldout). Work only in the Lab; do not touch any Unity project file.

This round changes **no phone behaviour**. It reorganizes the Editor's **Guide tab** so an author finds every choice of the AI
guide where they expect it, and sees the few that matter first. Nothing is lost: every row keeps its key, its demo, its
default, its words and its tests.

READ FIRST (every line):
- `guide_polish_plan.md` (this folder): the inventory (649 rows), what is wrong (section 2), the proposed tab (section 3).
- `PROTOTYPE_STRUCTURE.md`: the rules, the modules, "Where a change goes", "Each row's demo" (`rowDemos`).
- `src/editor/guide-tab.js` (GUIDE_SECS, edGuide: the tab takes the guide's foldouts from the Detail Card document by id),
  `src/guidance/editor.js` (section 4 · Guidance, its groups and items, the item form: `H("Identity",…)` and the other groups,
  `gdSaysHtml`, the words table), `src/editor/texts.js` (the foldouts' names), `src/editor/focus.js` (what each foldout's
  toggle shows on the phone), `src/editor/shell.js` (fo, the foldouts' open state, `S.op`, `S.ui.level`, goOwner / links),
  `src/editor/demo.js` (rowDemos, the self-test's NO DEMO), the modules that make the guide's foldouts (`s7voice`, `s7vtest`,
  `s7dock`, `s7gbtn`, `s7cap`, `s7focus`, `s7coach`, `s7tour`, `s7resume`, `s7hero`, `s7block`, `s7mascot`, `s7ask`, `s7cam`,
  `s7agent`; grep `"s7` in `src/`).
- `README.md`: "Prove it" and "Levels". `../../skills/Feature_Prototyping.md`: step 3.3 and step 5.

## The developer's decisions (2026-11-11; do not reopen them)

- Only the AI guide domain until it is finished and polished. This round: the Guide tab's organization. Then a design pass
  (round 15), then features.
- **The tab, approved as proposed** (plan section 3):
  ```
  Guide
  ├─ 0 · Preview & Test      Start the app · Today is · Show next now · Why not now? · Log
  │                          The visitor: one line (visit, stage, heard the guide) + a "Details" foldout (the table, the counts)
  │                          The visitor's settings (stand-ins): Tips · Tips stay · Games on the tour · Screen reader
  │                          Simulate a visit · Voice test (moved here from The tour)
  ├─ 1 · Azu                 Look (Character, Mascot source, Model) · Size · Liveliness · Mouth · How it moves · Moods ·
  │                          Tap on Azu · Introduction · Walks to the wall · Model tools (a foldout: Play a clip, Import…,
  │                          Generate from a picture). No Mascot › Mascot. Moments and Water break after (pointer rows only)
  │                          go: Games on the tour holds them.
  ├─ 2 · The tour            Narration · Captions · Focus · Invite to try · Guide panel (Tour controls' words move into it:
  │                          no empty foldout) · Ways in (a foldout: Guide button · Hero button · Guide block · Resume button)
  ├─ 3 · Games on the tour   promoted from Guidance › Fun › On the tour: the rhythm and the plan line first, then The quiz
  │                          (the promise and the finale), Guess the year, Don't say it!, Find it on the wall, The breather,
  │                          Cookie (its variant)
  ├─ 4 · Azu speaks up       today's 4 · Guidance (Overall, App, Content, Fun › Play, Sequences); Fun keeps Play only
  └─ 5 · Ask Azu             What the visitor sees (Answers from, Ask about the view, Suggestions, Spoken answer, Voice of the
                             answers, Link cards, Trace under each answer) · Knowledge (App context, Photo of the view,
                             Targets, Library, General knowledge, Actions in the app) · The engine (a foldout: Guide mode,
                             the models, Claude tier, Longest answer, Cost, Text and voice, Paste / Test the key, Model steps,
                             Timeout, Questions a session)
  ```
- **Each item's form: every group folded.** Each group of an item's form (Identity, Words, About, Show, When, Only if,
  How much / Limits, and the per-kind groups) is its own closed foldout under the item's header line (On, name, its when,
  ▸ Show now, which stay as they are). Each group's header says what it holds and **how many of its fields differ from the
  item's defaults** ("When · after 3 pokes or a late pause · 1 changed"). Opening an item shows its group headers only.
- **Words and Says are one list** (the plan's section 2, point 2): each line once, EN | PT side by side, with ▸ (play it) and
  Remove, + Add a variant, Per audience and Returning visitor under it. The Languages & Terms words stay owned here (their
  `gd:i:<id>` owner and keys unchanged: only the drawing changes).
- **Names that say what is inside:** 4 · Azu speaks up's subtitle (tips, remarks, play, walkthroughs, the first pages, on
  its own); Fun's subtitle without "never spoken"; Ask Azu's three parts; Azu (no "Mascot › Mascot").
- **A point's own field links to it:** where an item uses a per-point field (the quiz's questions, Guess the year's year,
  Don't say it!'s word, Find it on the wall's point, a point's joke, What's in view's line, the tour's steps), the item shows
  the demo point's value and a link that opens Specific Marker › Guide script at that field ("This point's word: palace
  (Reviewed) · Specific Marker › Guide script ›"), as the existing `data-goto` / goOwner links do.

## Rules for this round

1. **Keep every row's key, every foldout's `data-fo` id, its demo, default and words.** New foldouts get new ids
   (`gt5` for Ask Azu if needed, `gtg` for Games on the tour, `gd:more:<item>:<group>`-style ids for an item's groups:
   pick one scheme and say it). A moved foldout keeps its id; only its parent changes.
2. **Links and paths follow the move:** `guidePath`, goOwner, Languages & Terms' owner links, `data-goto` targets, the
   Why not now? and the plan line's links, Ask Azu's Show me, the ✦ menu's Editor links (if any), every "Edited in Guide ›
   …" sentence. Grep for "Guidance › Fun › On the tour", "Guide › Guidance", "Mascot ›", "Ask ›" in `src/` and in the
   words (`src/core/words.js`) and fix each path text.
3. **The foldouts' open state:** `S.op` keys by id; a saved state that opened `gd:fun` > `gd:g:ontour` must still open the
   games (map old paths once, or open by id). `S.ui.level` (how deep the foldouts open) must give a sensible first view:
   level 1 shows the six sections; level 2 their foldouts; an item's groups never open by level alone.
4. **The Guide tab's full redraw takes about 1.3 s:** folding the groups should make it faster (closed groups need not be
   drawn until opened, if the shell allows it; measure the draw before and after and report both).
5. **The self-test and NO DEMO** walk every row with every foldout open (`S.ui.level=9`): every row must still be found and
   still have its demo. A group's own header is not a row.
6. **The Detail Card tab** must not show any guide foldout (today `edCard.tab` removes `s7`); the Guide tab must not lose
   a foldout that a module adds later (today the rest goes to the end of The tour: keep a home for leftovers and warn in
   the console if anything lands there).

## How you work

1. After each change: only `python build.py`. Its check must read 0/0/0/0.
2. Capture, judge, fix, until it is right. Captures of the **Editor** (1600 × 1000, dark and light, the developer's Editor is
   dark): the tab at level 1, each section at level 2, one item closed / with its group headers / with one group open (A joke,
   The quiz, Don't say it!), the merged Says list (a joke with 3 variants; a per-audience line), Ask Azu's three parts, Azu's
   section, Preview & Test with The visitor folded, a per-point link and where it lands. Judge: the author's first view
   names every part of the guide in words a non-developer understands; no section with one row; no foldout with zero rows;
   nothing said twice; the counts of changed fields are right. Save them in
   `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g14\`.
3. Only then the tests, run once, at the end, after checking the CPU load
   (`powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`; above about 40 %: say so and wait).

## Lessons (read them)

- Level 2 after rounds 12 and 13 found checks written for the old state (counts of scenes, Settings rows, a scroll that met a
  step taken out of the tour). **This round moves foldouts that 16 test files open by path or count**: before your end run,
  grep `tests/` for `gt1`, `gt2`, `gt3`, `gt4`, `gd:fun`, `gd:g:ontour`, `s7mascot`, `s7agent`, `s7ask`, `s7cam`, `s7vtest`,
  `S.op=`, `S.ui.level`, "Guidance ›", "Mascot", ".row\").length", `guideSecIds`, and every section or row count; fix each
  check to find its row by key, and say which you changed and why.
- A check that reads a state right after an action races the app's timers: `until(...)`, never a fixed wait.
- Keep each new file under 300 s. The self-test: one big tab per walk; never `-tabs guide,words`.
- Do not call the live model in tests.

## Tests

- `tests/test9r.js` (Playwright with the installed Edge), under 300 s: the six sections in order with their names; each moved
  foldout under its new parent (by id); no foldout with zero rows in the tab; no section with a single row; every row key of
  the old tab still present (compare against a list you take from master's build before you start: save it as
  `tests/guide_rows_r13.json`); an item opens to its group headers only, each with its changed count (change one field by a
  real click: the count goes to 1; Reset or the default: back to 0); the merged Says list (a real edit of the PT text
  changes Languages & Terms' word; Add a variant; Remove with Undo); a per-point link opens Specific Marker › Guide script at
  its field (a real click); an old `S.op` state opens the games in their new place; the Guide tab's draw time before and
  after (report; check it is not slower).
- A mutation (`MUTATE=1`): for example a moved foldout left out of its new section, so the named checks turn red. Then restore.
- At the end, one run at a time: the self-test `-tabs guide`, then `-tabs marker,global`, then `-tabs words` (each bad=0); then
  `node run_tests.js --only=test9r` plus every file your grep found (expect most of test8k, test9da, test9db, test9e-test9q);
  then `MUTATE=1 node tests/test9r.js`.
- Update `PROTOTYPE_STRUCTURE.md` (the tab's sections and where each foldout lives now), `README.md` (test9r) and
  `tests_baseline.json`.

## Limits

- Edit only files under lab_src. Do not commit, do not publish.
- No keys, voicebench, mascot model files or Unity. Do not call the live model. Do not download anything.
- One home per feature: extend the existing modules; no wrappers, no re-assignment of another module's function.
- Write patch scripts with the Write tool: bash heredocs mangle apostrophes.
- If a usage limit stops you, your work is saved on disk. Say in your last message which parts are done.

## Report (your final message)

- The new tab, section by section, with every foldout's id and where it came from; the item form's groups and their ids.
- Every link and path text you changed.
- The draw time before and after.
- The captures (paths), what you judged and what you fixed.
- The tests: test9r, the mutation, every check you changed in another file and why; the CPU load and each file's time.
- 3-5 specific questions for the developer about the organization (names, order, what is folded).
