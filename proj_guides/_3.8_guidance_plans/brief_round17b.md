You are building round 17b in a browser prototype, the "Block Components Lab", in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app (say each
change's Unity path). Work only in the Lab. This round fixes what the Architect's core test found, applies the developer's answers on
round 17, sets the developer's new test tiers, and runs the core tests.

READ FIRST: `PROTOTYPE_STRUCTURE.md` (the rules, the round 17 rows: `guide/saved-place.js`, `guide/tell-more.js`), `README.md` ("Prove it",
"Levels"), `src/guide/saved-place.js`, `src/guide/tell-more.js`, `src/guide/autotour.js` (the deep parts), `src/card/blocks.js`.

## 1. The crash the core test found (fix first)

`tests\runtest8.ps1` (every tab) on round 17's build: the guide walk is fine (ok=69 bad=0), but the card walk and the marker,words,global
walk never finish ("no json"; "INCOMPLETE: a walk did not finish"). Both pages log on load: `Uncaught TypeError: Cannot read properties of
undefined (reading '0')` at the built page's Dialogue renderer in `src/card/blocks.js`: `"Dialogue":function(b){var dw=kindWords(b).lines;
... dw[0] ...}`. `kindWords(b)` returns the block's own words `b.c` when it has any keys, else `KWORDS[kind]`: a Dialogue whose own words
have no `lines` crashes. Find which rounds' change made it reachable (rounds 15-17 ran only the guide walk), fix it at its root (every
renderer that reads `kindWords(b).<field>` must fall back field by field to its kind's words, not object by object; grep them all), and
make both walks finish: `tests\runtest8.ps1 -tabs card` and `tests\runtest8.ps1 -tabs marker,words,global` (bad=0, or each bad explained
and fixed). The output of the failed run: `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g17\level2\selftest.txt`.

## 2. The developer's answers on round 17 (2026-11-12; do not reopen them)

- **One way back into a stopped tour.** Merge round 11's within-visit "Resume the tour" (the Stop toast, the ✦ menu: back to the sentence)
  and round 17's next-visit "Continue the tour (step N of M)" into one offer with the same words everywhere ("Continue the tour (step 5 of
  13)" / "Continuar a visita (passo 5 de 13)", and "Start again"): within the same visit it goes back to the sentence, as Resume did; on a
  later visit it says the "Where we were: …" recap first. The Stop toast, the ✦ menu, the hero and the invitation all show it. The Editor's
  rows (Ways in › Resume button and the saved place's rows) become one coherent foldout; every key, demo and test of the old rows kept or
  moved (the self-test finds rows by key).
- **Tour lover = a visitor who chose Deep** (the guide's length in their Settings or their Audience): as built; nothing to change.
- **The new captions and the g7 deep part stay as placeholders**; the teaser's openings may differ between EN and PT (as built).
- **The gallery's Tell me more in the storyteller voice** (the Architect, from the developer's standing rule that Azu's words are a warm
  storyteller, never a list): today "In its pictures: a painting of the square with …, an engraving of …, a drawing of …" reads as a
  list. Give a gallery's deep part a warm lead from a small set (EN and PT, editable in Azu's intro lines like the others) and say the
  captions as a guide would ("Swipe through and you'll find the square with the palace and its tower by the river, then the ruins the
  earthquake left…"), still only the card's words plus the lead, no fact twice, under its cap.

## 3. The developer's test tiers (2026-11-12): write them into README's "Levels" and PROTOTYPE_STRUCTURE's rules

"Feature tests per round, each 4 rounds core tests, each 12 rounds full tests."
- **Feature tests, every round** (~5-15 min): the round's own test file(s) and mutation, the self-test of the tab it touched, and at
  most 1-2 files the brief names. Never "the files of every module you change", never `--changed`'s pull.
- **Core tests, every 4 rounds**: every self-test walk (`runtest8.ps1`, all tabs) plus **the core set**: define it now, in README, as the
  files that test the shared modules (`core/`, the phone and the card's chrome, the Editor's shell, the guide's engine: tour, dock,
  person, autotour, saved place), aiming at about 25-30 min in total; say why each file is in it. Add a `--core` option to
  `run_tests.js` that runs that set (one list, in one place).
- **Full tests, every 12 rounds and before Unity**: every file, every mutation, a new baseline.
- Split the files over 300 s so a full run stays bounded: test9db (~530 s), test9e (~415 s), test9sf (~370 s on its last run): the same
  checks and mutations, moved, not dropped.
- test9i's two old reds (the scripted visitors' tour counts: Fast learner 2 tours instead of 1, Tour lover 5 instead of 3; the same on
  round 16's build): find the cause; fix the code if the app is wrong, or the expectation if the app changed on purpose (say which and
  why).

## Tests for this round

- Feature: test9u and test9ub (the merged offer: every place shows it, within a visit back to the sentence, a later visit with the
  recap; their mutation), test9t (the gallery's deep part: from its set, warm, no list; its mutation), the file that covers the Dialogue
  block (grep), test9i.
- **Core (due now: 5 rounds since the last)**: `tests\runtest8.ps1` with no `-tabs` (every walk; bad=0), then `node run_tests.js --core`.
- Check the CPU before each run (`powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`; above about 40 %:
  wait). Update `tests_baseline.json` for the files you ran or split.
- Capture and look (390 px, light and dark): the merged offer in the Stop toast, the ✦ menu and the hero; the gallery's deep part
  playing; the Editor's merged foldout; in
  `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g17b\` with `index.md`.

## Limits

- Edit only files under lab_src (captures in the scratchpad). Do not commit, do not publish, do not switch branches. No keys, no live
  model, no downloads, no Unity. Patch scripts through the Write tool; keep each file's line endings. `until(...)`, never a fixed wait.
- Before you finish, make sure none of your test processes is still running (the tavily MCP node processes are not yours).

## Report (your final message)

The crash's cause and fix (which round made it reachable); the merged offer; the gallery's new deep part (EN and PT); the core set and
its time; the split files; test9i's cause; every test run with its numbers and time; the CPU load; and 1-3 questions for the developer,
each opening with one plain sentence on what the feature does.
