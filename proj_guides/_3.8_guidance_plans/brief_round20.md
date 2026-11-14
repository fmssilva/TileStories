You are building round 20 in a browser prototype, the "Block Components Lab", in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app (say each
change's Unity path). Work only in the Lab. This round is **"The Editor as an accordion"**: the Editor's Guide tab becomes an accordion that
starts collapsed with sticky headers, each section's options are reviewed, the Start button's glow gets its option, and the **core tests**
run (4 rounds since the last core run).

THE CHECKLIST: `C:\Users\franc\Desktop\TileStories\proj_guides\___todo.md`, section "THE CHECKLIST", items **20.1 to 20.6**. The
developer's notes are above it. Mark an item `[DONE]` (replace `[ ]`) only after it is built, looked at in your captures and tested; an
item you change or cannot finish gets one line under it saying what and why. Keep the file's CRLF line endings; edit only items 20.x.

READ FIRST: `PROTOTYPE_STRUCTURE.md` (the rules; the rows of `editor/*`, `guide/mascot-profile.js`, `guide/hero-start.js`), `README.md`
("Prove it", "Levels": this round runs **core**), `src/editor/guide-tab.js` (GUIDE_SECS, GUIDE_PLAN, the `gtnav` chip row, guideDepths,
isOpen / S.op / S.ui.level), the Editor's `fo()` foldout and its CSS, `src/guide/hero-start.js`.

## What the developer asked (2026-11-13, their words)

- "The editor: we have some sub tabs for the Guide sections, but let's use some sort of accordion style. We start with the sections
  collapsed so the developer sees clearly all we have, and then he can open it, and we keep a sort of sticky headers of the sections and
  sub sections so they are still visible at the top when we scroll down."
- "Confirm that whole editor section is well organised, with the right useful developer config options to choose the right feature and
  options for that feature."
- The Start button's glow: "make it an Editor option; as default it can stay once per visit."
- Standing: every Editor row has its ▸ demo and ? help; messages name things, not ids; the Editor says "the mascot", never "Azu"
  (round 19); prefer one pattern across domains (build the accordion and sticky headers as one Editor feature any tab can switch on);
  Lab features must be Unity-buildable; never the live model in tests.

## The design to build

1. **The accordion (20.1).** The Guide tab opens with its six sections collapsed: one line each, its number, name and a short
   sub-line (what it holds) plus a quiet count ("8 parts"), so all six fit on one screen of the Editor. The chip row "The guide: 1 · …"
   goes (the collapsed list does its job). Opening a section opens it only (others stay as they are; an Editor option "One section open
   at a time" Off by default). A section's foldouts inside also start collapsed. The open state is remembered as today (S.op). Links
   that open a place in the tab (goOwner, Languages & Terms, "link to …" rows from round 19) still open their path and scroll to it. A
   small "Collapse all" at the tab's top. Unity: UI Toolkit Foldouts in the POI Editor's Guide tab, value persisted per id.
2. **Sticky headers (20.2).** While scrolling inside an open section, its header stays pinned at the top of the Editor; inside it, the
   open foldout's header pins just under it (two levels; a third level does not pin). Pinned headers keep the Editor's background, a
   hairline under them, and stay one line (ellipsis). Tapping a pinned header scrolls back to its start (it does not collapse it). The
   test's own arithmetic: each pinned header's top equals the one above's bottom, nothing overlaps, no row hides under them when
   scrolled to (scroll-margin). Built as one Editor feature (`fo()` / CSS), switched on for the Guide tab; say in the report what
   switching it on for the Detail Card and Global tabs would take (do not switch them). Unity: a header element pinned with a
   ScrollView offset (`position: absolute` header updated from the scroll value).
3. **The review (20.3).** Walk each section (1 · The mascot … 6 · Preview & Test) as a developer configuring a new point. In your
   report, a table per section: each foldout and its rows, then what you did: kept, moved (where, why), renamed (old → new), merged,
   or a question. Do the obvious ones (a row in the wrong section, a duplicate, a name that is an id or unclear); anything that removes
   an option or changes what the visitor sees is a question for the developer, not done. Every foldout's sub-line says plainly what it
   holds.
4. **The glow's option (20.5).** In 1 · The mascot › Moments (or where Start the guide's own rows are; say which and why):
   "Start the guide glows": Once per visit (default) / On every new card / Off, with its ▸ demo (opens a card, the glow, a tap, a second
   card) and ? help.
5. **Tests (20.4, 20.6):** below.

## Tests for this round (core tier)

- A new file `tests/test9y.js` (the Editor as an accordion), with its mutation, real taps: [collapsed] the six sections collapsed on a
  fresh open, all six in the first screen; [open] a tap opens one, the others unchanged; one-at-a-time when switched on; Collapse all;
  remembered after a re-render; [links] a link to a deep row opens its path and scrolls it into view under the pinned headers; [sticky]
  scrolled inside a section and inside a foldout: the two headers pinned, stacked exactly, no overlap, a tap scrolls back; [review] the
  rows you moved are where the report says, each with ▸ and ?; [glow] the three choices change what the visitor sees, its demo plays.
- The self-test, **all tabs**: `tests\runtest8.ps1` with no `-tabs` (bad=0). The collapsed start must not hide rows from the walk (it
  opens by level, S.ui.level); if the walk's numbers change, say why.
- **Core:** `node run_tests.js --core` (the set between the `<!-- core set -->` markers in README).
- **20.6:** the ten long files whose Editor labels round 19 changed: test8g, test8s, test8t, test8x, test9db, test9e, test9ea, test9g,
  test9h, test9k. A failure from a label or a moved row: fix the test or the place; a real regression: fix the code; say which.
- Check the CPU before each run (`powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`; above about 40 %:
  wait). Add test9y to `tests_baseline.json` and update the numbers of every file you ran.
- Capture and look (the Editor at its usual width, dark skin and light): the tab collapsed; a section open with a foldout open and
  scrolled so both headers are pinned; the glow's row; in
  `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g20\` with `index.md`.
  Look as a designer: calm, clear hierarchy, one line per header, nothing jumping while scrolling.

## Limits

- Edit only files under lab_src, plus the checklist's 20.x items. Captures in the scratchpad. Do not commit, publish or switch branches.
  No keys, no live model, no downloads, no Unity. Patch scripts through the Write tool or Python, never a bash heredoc with apostrophes;
  keep each file's line endings. `until(...)`, never a fixed wait.
- Before you finish, make sure none of your test processes is still running (the tavily MCP node processes are not yours).

## Report (your final message)

What the developer sees now (the tab collapsed, opening, the sticky headers), the review tables, what you changed from this design and
why, the glow's option, each part's Unity path, what switching the feature on for other tabs would take, every test run with its
numbers, time and CPU load (core, the walk, the ten files, test9y and its mutation), the checklist items you marked, and 1-3 questions
for the developer, each opening with one plain sentence on what the feature does.
