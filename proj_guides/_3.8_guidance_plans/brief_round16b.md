You are building round 16b in a browser prototype, the "Block Components Lab", in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app (say
each change's Unity path). Work only in the Lab.

Round 16 built the automatic tour (`src/guide/autotour.js`: `tourFromCard`, `AT_RULES`, `atCheck`; the Editor's Guide › 2 · The tour ›
Tour from the card; `POINTS` in `src/core/content.js`; `tests/test9t.js`). The four tours' text is in
`C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g16\tour_en.md` and `tour_pt.md`.
READ FIRST: `PROTOTYPE_STRUCTURE.md` (the rules, the round 16 rows), `README.md` ("Prove it"), `src/guide/autotour.js` whole, `tests/test9t.js`.

## The developer's answers (2026-11-12; do not reopen them)

1. **Never repeat a fact.** A number, a date or a named fact (a king, a place) already said in the tour is not said again, even in other
   words ("about 250 years" and "Two hundred and fifty years" are the same fact). The teaser's hook takes the strongest fact; a later
   step drops a sentence or a fact-list item whose fact was said (and if a step is left with nothing new, it says its block's other
   words or, for a show step, only its link). One rule in `autotour.js` (e.g. a fact key from the numbers, the dates and the proper
   names of each sentence), checked by `atCheck` ("no fact twice") and in test9t. Unity: the same rule in the C# builder.
2. **Links tied to the content.** Replace the generic links ("Look at this.", "Here's more.", "Now, something to see.", "Take a look.",
   "And there's this.", "One more thing.") with links that name what comes next, made from the block's own words, deterministic, no AI:
   e.g. "Now, what was inside:" (from a callout's heading), "Here's the square through three centuries." (a gallery from its caption),
   "Then the dates:" (a timeline), "Now the story." / "Back on the card." stay. Still never the same link twice in a row; EN and PT;
   short (at most 6 words before the content). Where no words fit, one neutral link. Show each link's source in the Editor's step list.
3. **"A short tour?"**: a tour under 2 minutes is offered as "A short tour of {point}?" / "Uma visita curta a {point}?" and its teaser
   says "In a short tour: …" instead of "In about a minute"; from 2 minutes up, the minutes as today. In every place the minutes are
   said (the teaser, the invitation, the hero's button, the ✦ menu): grep `tourMinutes`.
4. **The story: up to 6 paragraphs**, as built (round 17 adds short or deep per step). Nothing to change.

## Two fixes found by the Architect

5. **The teaser at the hero, at Mini:** the dock covers the hero's subtitle and its Start button while the teaser plays (the written
   tour's g0 too; captures `g16\cap\01_ribeira_teaser_mini_light.png`, `07_torreao_teaser_mini_dark.png`). Fix it in the module that
   places the told block or the hero during a tour (round 15's rule: 0 px over the words being told; the hero's title and subtitle are
   the words told at the teaser); during a tour the hero's own Start button may hide (the tour is already playing) or move: pick the
   one that keeps a single focal point and say why. Add the check to round 15's family (`tests/g15lib.js`, the file holding the hero
   or test9t). Unity: the same placement on the hero's ScrollView offset.
6. **The new points' hero picture:** the Torreão, the Terreiro do Paço and the Tagus show the Ribeira's hero picture. Give each its own:
   a crop of the Lab's wall painting around that point (the wall image the Lab already has: grep the wall's image and the points'
   positions), drawn as the hero's picture (CSS background-position/size on the same file: no new file, no download). Unity: a UV rect on
   the same texture.

## Tests and work

- After each change only `python build.py`; its check must read 0/0/0/0. Capture and look (390 px, light and dark): the four tours'
  teasers at Mini, a step with a content link, the short-tour invitation, the Editor's step list; in
  `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g16b\` with `index.md`, and
  write the four tours' new text to `g16b\tour_en.md` and `g16b\tour_pt.md` (read it as a listener: it must sound like a guide).
- test9t gains: no fact twice (EN and PT, the four points), every link from its block's words or the neutral one and never twice in a
  row, the short-tour words under 2 minutes and the minutes from 2 up, the hero's words clear at the teaser, each new point's own hero
  crop. Its mutation turns them red.
- Then, one at a time, after checking the CPU (`powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`;
  above about 40 %: wait): test9t, test9s, test9sf, test9k (the invitation), test8o, the files of every other module you change (README's
  "Prove it"), and the self-test `tests\runtest8.ps1 -tabs guide` (bad=0). Never `-tabs guide,words`. Update `PROTOTYPE_STRUCTURE.md`,
  `README.md` and `tests_baseline.json`.

## Limits

- Edit only files under lab_src (captures in the scratchpad). Do not commit, do not publish. No keys, no live model, no downloads, no
  Unity. Patch scripts through the Write tool; keep each file's line endings. `until(...)`, never a fixed wait, after an action.
- Before you finish, make sure none of your test processes is still running (round 16's mutation run outlived its report).

## Report (your final message)

What changed where (file, Unity path); the fact rule and the link rule with examples; the new text's paths; every test file run with
its numbers and time; the mutation; the CPU load; and 2-3 questions for the developer if any remain.
