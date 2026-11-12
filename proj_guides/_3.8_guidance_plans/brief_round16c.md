You are building round 16c in a browser prototype, the "Block Components Lab", in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app (say
each change's Unity path). Work only in the Lab.

Rounds 16 and 16b built the automatic tour (`src/guide/autotour.js`: `tourFromCard`, `AT_RULES`, `atCheck`, the fact keys, the links
from the blocks' words; the Editor's Guide › 2 · The tour › Tour from the card; `POINTS` in `src/core/content.js`; `tests/test9t.js`).
The four tours' current text is in
`C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g16b\tour_en.md` and `tour_pt.md`:
read both first, as a listener. READ FIRST also: `PROTOTYPE_STRUCTURE.md` (the rules, the round 16 and 16b rows), `README.md` ("Prove
it"), `src/guide/autotour.js` whole, `tests/test9t.js`.

## The developer's answers (2026-11-12; do not reopen them)

1. **Warm, motivating block intros, not "robot block presentation".** The developer, in their words: instead of "Now the story." or
   "First, the facts.", lines like "Before anything else, let me tell you some main facts that you might want to know about the
   Ribeira Palace." or "Let's get to know a bit more about the story of the Ribeira Palace... there are a lot of interesting details
   and facts to know...": "nice and motivating and interesting default sentences".
   - Each block kind that opens a step (the facts, the story as it opens, a text, a gallery or picture, a timeline, a before and after,
     a flip card, a callout, the quiz) gets a small set of warm intro lines, EN and PT (good European Portuguese, not a translation of
     the English word for word), in Azu's voice: inviting, curious, one sentence, as a real guide speaks to a visitor beside them.
     Where round 16b's content-tied link fits (a callout's heading, a gallery's caption, a timeline's date), keep the content in the
     line ("Here's something I love: the square through three centuries."), so the warmth never replaces what is specific.
   - Deterministic, no AI: the line picked from the set by the point and the step (stable for a point, varied across points and steps);
     never the same line twice in a tour; the point's name in at most two intro lines of a tour (otherwise "it", "the palace", "here",
     or no name), with the article rule (`nameIn`, `nameJoin`). A new cap for an intro line (about 20 words; say it in the Editor),
     and the tour's minutes still computed from all its words.
   - The teaser's "In a short tour: its dates, its pictures, and a question to finish." and the end's "That was X. Ask me anything, or
     explore on your own." get the same care (warm, still short; the end may thank the visitor or point to what to look at next on the
     wall). The "Your turn: …" lines of the try steps may stay if they read well; make them warm too where they don't.
   - **The Editor:** each block kind's intro lines are a row in "What each block says" (the lines listed, EN and PT, editable: add,
     edit, remove, back to the defaults), with its demo (the tour at that kind's step, the line said). These are the defaults every
     generated tour uses; Make it mine still copies the resulting lines into the written tour to edit one by one.
   - Read the four tours aloud in your head, EN and PT, before the tests: they must sound like a person who loves the place, not a
     list; no line should feel copy-pasted from another tour.
2. **A block with nothing new is skipped.** A block whose facts were all said already (the Ribeira's Quick Facts today says only "First,
   the facts.") has no step at all; the tour goes straight to the next block. The step list in the Editor shows it as skipped with the
   reason ("nothing new: its facts were said in the teaser"), named, not an id.
3. **Portuguese point names carry their article**: "o Terreiro do Paço", "o Paço da Ribeira" (or whatever the Lab's PT names are: grep
   `POINTS` and the card's PT name), "o Tejo e os seus navios", so "Uma visita curta ao Terreiro do Paço?" via round 16b's `nameJoin`;
   the article lower-case mid-sentence, capital at a sentence start (round 15b's rule); a title or a label shows the name as written.
4. **The minutes everywhere**: the ✦ menu's "Start the tour" and the Guide block's fixed "3 min" follow `tourLength` too ("· short" under
   2 minutes, the computed minutes from 2 up; the written tour keeps its number). Grep every place a tour's length is shown or said.
5. The three new points' card texts stay as placeholders (nothing to change).

## Tests and work

- After each change only `python build.py`; its check must read 0/0/0/0. Capture and look (390 px, light and dark): a warm intro step
  on two points, the Editor's intro rows, the ✦ menu and the Guide block with the short wording, the PT invitation with the article; in
  `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g16c\` with `index.md`,
  and the four tours' new text in `g16c\tour_en.md` and `g16c\tour_pt.md`.
- test9t gains: every intro line from its kind's set (or the Editor's edited set), never twice in a tour, the name in at most two, under
  its cap, EN and PT; a block with nothing new has no step (and the Editor names why); the PT names' article ("ao Terreiro do Paço");
  the ✦ menu and the Guide block say the same length as the invitation; an intro line edited in the Editor is said by the next
  generated tour. Its mutation turns them red. If test9t nears 300 s, move the new checks to `tests/test9tb.js`.
- Then, one at a time, after checking the CPU (`powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`;
  above about 40 %: wait): test9t (and 9tb), test9s, test9k, test8o, the files of every other module you change (README's "Prove it"),
  and the self-test `tests\runtest8.ps1 -tabs guide` (bad=0). Never `-tabs guide,words`. Update `PROTOTYPE_STRUCTURE.md`, `README.md` and
  `tests_baseline.json`.
- Note: test9i's two red checks (the Fast learner and Tour lover tour counts) are old (round 16's build gives the same); leave them for
  the Architect's level 2 unless your change touches them.

## Limits

- Edit only files under lab_src (captures in the scratchpad). Do not commit, do not publish. No keys, no live model, no downloads, no
  Unity. Patch scripts through the Write tool; keep each file's line endings. `until(...)`, never a fixed wait, after an action; a
  check that measures a placement waits for the placement's own condition (round 16b's [hero words] raced the placing loop once).
- Before you finish, make sure none of your test processes is still running.

## Report (your final message)

What changed where (file, Unity path); the intro sets (a few examples per kind, EN and PT) and how a line is picked; the new text's
paths; every test file run with its numbers and time; the mutation; the CPU load; and 2-3 questions for the developer if any remain.
