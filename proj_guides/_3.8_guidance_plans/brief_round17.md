You are building round 17 in a browser prototype, the "Block Components Lab", in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app: every
part must be buildable there (say each part's Unity path). Work only in the Lab.

READ FIRST: `PROTOTYPE_STRUCTURE.md` (the rules, the modules, "Where a change goes", the rows of rounds 16, 16b and 16c), `README.md`
("Prove it", "Levels"), `../../skills/Feature_Prototyping.md`, `C:\Users\franc\Desktop\TileStories\proj_guides\_3.8_guidance_plans\guide_polish_plan.md`
section 5, `src/guide/autotour.js` whole, `src/guide/tour.js`, and `tests/test9t.js`. The four tours' current text:
`C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g16c\tour_en.md` and `tour_pt.md`.
Grep before you edit; one home per feature, no wrappers, no re-assignment of another module's function.

## Part A: the words (the developer's answers on round 16c, 2026-11-12; do not reopen them)

1. **"The Ribeira Palace"** in English, everywhere (the hero, the marker's label, the invitation, the tour, the ✦ menu, Ask Azu's
   context): the article lower-case mid-sentence by round 15b's `nameIn` ("the story of the Ribeira Palace"), capital at a sentence
   start, a title or a label as written ("The Ribeira Palace"). Update the tests that expect the old name.
2. **One fact left gets its own wording**: a small set in the facts kind for a single fact (e.g. "One thing worth knowing: it was lost
   on 1 November 1755." / "Uma coisa que vale a pena saber: …"), the warm storyteller tone, editable in the Editor's intro rows like the
   others; the many-facts set stays for two or more.
3. **The teaser's line is a set too**: a few warm openings ("Come along on a short tour: …" and others), picked by the point like the
   intro lines, so the four tours open differently; editable in the Editor (one more row in Azu's intro lines); the minutes or "short"
   still said.
4. **The Guide block's PT title** says "Conheça o palácio com o guia" on every point's card (the square and the river too): it names the
   point or says something true of any point ("Conheça o Terreiro do Paço com o guia" via `nameIn`), EN and PT.

## Part B: the two features (approved 2026-11-11, plan section 5)

5. **A tour resumes on the next visit where the visitor stopped.** Today the Resume button (Editor `s7resume`) resumes within the
   session. Now: when a tour stops before its end (closed, the app left, the card closed), the point and the step are saved (the
   Lab: localStorage, wrapped in try/catch, the app working without it; Unity: PlayerPrefs or the save file, per point). On the next
   open of that point (a reload counts as the next visit), the invitation, the hero's button and the ✦ menu offer "Continue the tour
   (step 5 of 13)" / "Continuar a visita (passo 5 de 13)" and "Start again"; continuing replays a one-line "Where we were: …" recap
   (from the step before, no new facts) then the step. A saved place expires (an Editor row: after 7 days by default, Never, 1 day,
   30 days); a tour finished clears it; a tour changed since (another written tour, the card's blocks changed so the step is gone)
   starts again and says so once, gently. Editor rows (Guide › 1 or 2, where round 14 put the Resume button): Remember where the
   visitor stopped (On), Forget after, the words; each with its demo (a saved place at step 5, the offer shown).
6. **Short or deep per step, at the visitor's own pace.** Careful: "Short / Deep" already names the games' rhythm on the tour (Guide ›
   3 · Games on the tour); do not reuse that key or mix them; name this one "Tell me more" in the Editor and the panel.
   - Each step can have a deep version: for a story or text step, the paragraph's further sentences beyond the cap (the automatic
     tour already cuts a long block to its first sentences: the rest is the deep part); for a facts step, the facts left out; for a
     gallery, the other pictures' captions; for a written tour, an optional "more" text per step in the Specific Marker's Guide
     script. A step with no deeper words has no "Tell me more".
   - The visitor's side: while a step plays, the guide panel shows "Tell me more" (only where there is more) and the existing Next as
     "Skip ahead"; Tell me more says the deep part, then goes on; a few Tell me more in a row make the next steps offer it first
     (the visitor's pace, remembered for the tour: Audiences' "Tour lover" may start deep), and a few Skip ahead make the tour lighter
     (the deep part never offered first). Ask Azu's "tell me more" phrase (typed or spoken) does the same.
   - The rule of no fact twice still holds across the deep parts (a fact said in a deep part is not said again later).
   - The Editor: Tell me more (On), Start deep for (none / Tour lover / everyone), the words; the deep text visible under each step in
     the step lists (generated: read only; written: editable). Each row with its demo.
   - Minutes: the tour's length says the short version's minutes (the deep parts are the visitor's choice); the Editor's step list
     shows both.

## How you work

- After each change only `python build.py`; its check must read 0/0/0/0.
- Capture and look (390 px, light and dark, Mini and Compact): the four teasers' new openings, the one-fact step, the Guide block's
  title on the square, the Continue offer on the invitation, the hero and the ✦ menu after a reload, the recap line, Tell me more on a
  story step and its deep part playing, the Editor's new rows; in
  `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g17\` with `index.md`, and the
  four tours' text with their deep parts in `g17\tour_en.md`, `g17\tour_pt.md` (read them as a listener).

## Tests

- New `tests/test9u.js` (and 9ub if it nears 300 s) for Part B, through the app's own controls (Playwright with the installed Edge):
  a tour stopped at a step, the page reloaded, the offer on all three ways in with the step's number, Continue (the recap, then that
  step), Start again, the expiry (the clock moved through the app's own test hook, never a real wait), a finished tour clears it, a
  changed tour starts again with its one notice, storage blocked (the app still works); Tell me more where there is more and not where
  there is none, its deep part said, the pace remembered (deep offered first after a few, lighter after a few Skip ahead), Ask Azu's
  "tell me more" (the model stubbed, never live), no fact twice across the deep parts, the Editor's rows and an edited written "more"
  text said. A mutation (`MUTATE=1`) turns the named checks red.
- test9t gains Part A's checks ("the Ribeira Palace" mid-sentence and as a title, the one-fact wording, the four teasers different,
  the Guide block's title per point) and their mutation.
- Then, one at a time, after checking the CPU (`powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`;
  above about 40 %: wait): test9u (and 9ub), test9t, test9s, test9k, test8o, test8v, test9g, the files of every other module you change
  (README's "Prove it"), and the self-test `tests\runtest8.ps1 -tabs guide` (bad=0). Never `-tabs guide,words`. Update
  `PROTOTYPE_STRUCTURE.md`, `README.md` and `tests_baseline.json`.
- Known and left for the Architect's level 2 (do not fix unless your change touches them): test9i's two old reds (the scripted
  visitors' tour counts); test9db runs about 413 s.

## Limits

- Edit only files under lab_src (captures in the scratchpad). Do not commit, do not publish, do not switch branches. No keys, no live
  model, no downloads, no Unity. Patch scripts through the Write tool; keep each file's line endings. `until(...)`, never a fixed wait,
  after an action; a check that measures a placement waits for the placement's own condition.
- Before you finish, make sure none of your test processes is still running.
- If a usage limit stops you, your work is saved on disk; say in your last message which parts are done.

## Report (your final message)

What changed where (file, Unity path); Part A's new lines (examples, EN and PT); how the saved place and Tell me more work, with the
rules; the paths of the text and the captures; every test file run with its numbers and time; the mutation; the CPU load; and 2-4
questions for the developer.
