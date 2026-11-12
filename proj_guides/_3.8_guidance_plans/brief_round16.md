You are building round 16 in a browser prototype, the "Block Components Lab", in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app: every
part must be buildable there (say each part's Unity path). Work only in the Lab; do not touch any Unity project file.

This round is **the automatic tour**: a tour made from a Detail Card's blocks, the default for every point, which the Editor can then
change and edit instead of writing a tour from scratch.

READ FIRST (every line): `PROTOTYPE_STRUCTURE.md` (the rules, the modules, "Where a change goes"), `README.md` ("Prove it", "Levels"),
`../../skills/Feature_Prototyping.md`, `C:\Users\franc\Desktop\TileStories\proj_guides\_3.3_POI_Card_Styles.md` section 11 (the guide)
and the card's blocks sections, `C:\Users\franc\Desktop\TileStories\proj_guides\_3.8_Guidance_Study.md`, and
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.8_guidance_plans\guide_polish_plan.md` section 5. Today's one tour is hand-written in
`src/guide/tour.js` (`S.guideSteps`: `{id, view, ref, act: teaser|show|read|try|open|end, wait, text}`); the card is `S.card` and its
story pages `S.story` (`src/core/content.js`). Grep before you edit; one home per feature, no wrappers, no re-assignment of another
module's function.

## The developer's direction (2026-11-12; do not reopen it)

"For a point with no written tour, have a default tour that the Editor can then change and edit, instead of creating a tour from
scratch: by default the AI guide narrates and explains all the blocks there are in the Detail Card, in a good quality way, and always
a summary / teaser said as a presentation while in the hero section, before really starting." All four remaining features are approved
(this round: the automatic tour; then resume on the next visit, short or deep per step, Ask Azu during a game).

## What to build

1. **The tour from the card** (a new module, e.g. `src/guide/autotour.js`, one pure function `tourFromCard(card, story, lang, opts)`
   returning steps in the written tour's own shape, so every part of the guide works on it unchanged: captions, the focus, Azu,
   the panel, the games on the tour, Audiences, the scheduler, Ask Azu, the voices).
   - **Step 0, the teaser at the hero**, always: the point's name, one hook (the strongest line from the card: its subtitle, its first
     fact, or the story's first sentence), and what the tour will cover and how long ("In about 4 minutes: its story, a before and
     after, and a question to finish."). Two sentences at most, said while the hero shows, before the first block.
   - **One step per block, in the card's order**, by the block's type (a table, one row per block type the Lab has; grep the block
     types): text and story pages are read (their own words; a long one in its first sentences, capped); facts said as one sentence of
     the two or three strongest; a picture or gallery shown with its caption; a before and after, a flip card and a quiz become `try`
     steps with their own wait (as the written tour's g2, g6, g9); a timeline says its first and last dates; the blocks with nothing to
     tell (navigation, credits, links, the header) are skipped. Each type's rule is a row in the Editor (below) with its demo.
   - **Links between steps**: short and varied ("Now the story.", "Back on the card…", "One more thing:"), never the same twice in a
     row, in EN and PT, Azu's voice (the guide's existing words style). **The end step**: one line of recap and "Ask me anything, or
     explore on your own."
   - **Quality rules, checked**: every step's line has a word cap (say it in the Editor), no sentence repeated across steps, the
     article rule of round 15b (`nameIn`) for the point's name, nothing said that is not on the card (the generator only uses the
     card's own words plus the link words), and the tour's minutes computed from its words at the voice's speed (`tourMinutes()`
     returns that number instead of the fixed 3 for a generated tour; the written tour keeps its number).
2. **Write it better with AI (optional, in the Editor only)**: a button that asks the model already configured for Ask Azu to rewrite
   the teaser, the links and the long blocks' summaries in Azu's voice, from the card's words only; the result is stored as that tour's
   text and shown for review (each step's before and after), never generated at run time for a visitor. Tests never call the live model:
   they stub its answer. With no key or offline, the button says so and the template tour stays.
3. **The Editor** (Guide tab › 2 · The tour, round 14's organization; and Specific Marker › Guide script where the point's tour is):
   - **Tour: Written | From the card (automatic)**. A point with no written tour uses From the card. The Ribeira Palace keeps its
     written tour as its default; switching it to From the card shows the generated one.
   - The generated steps listed like the written ones (each with its block, act, line and length), read only, with **Make it mine**:
     copies them into the written tour to edit (each step's text, act, keep or skip, order), and **Regenerate this step** on each one.
   - When the card's blocks change after Make it mine: a notice naming the blocks that are not in the tour ("2 new blocks not in the
     tour: the Gallery, the Timeline. Add them"), with Add them (and Dismiss).
   - The block-type rules (item 1) as rows, each with its demo (the tour at that block's step, the block ringed).
   - Every new row has its key, default, words and demo; every message names things, not ids.
4. **The other points**: the wall's other points (the Torreão, the Terreiro do Paço, the Tagus and its ships: grep how they open a card
   in the Lab) get the automatic tour from the card they show, so "A tour of {point}?" works there too; the scheduler's "has no tour"
   reasons change where a generated tour now exists.
5. **Voices**: the recorded voice files exist only for the written tour's lines; a generated line is spoken by the device voice or
   ElevenLabs as the voice settings say (no new recording; no voicebench). Say how Unity does this (the same TTS path as Ask Azu).

Also, from round 15b: `tests/test9se.js` runs 324-340 s; move its [azu] cases into their own file (`tests/test9sf.js`) so both stay
under 300 s, with the same checks and mutation.

## How you work

1. After each change only `python build.py`; its check must read 0/0/0/0.
2. Build, capture, look, fix (`skills/Feature_Prototyping.md` step 3.3): capture the generated tour playing on the Ribeira card and on
   one other point at 390 px, light and dark, Mini and Compact, plus the Editor's rows, in
   `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g16\` with an `index.md`.
   Read the generated tour's whole text aloud in your head, EN and PT: it must sound like a guide, not a list. Put the full text of both
   in `g16\tour_en.md` and `g16\tour_pt.md`.
3. Only then the tests, after checking the CPU (`powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`;
   above about 40 %: wait).

## Tests

- `tests/test9t.js` (and 9tb if it nears 300 s), Playwright with the installed Edge, through the app's own controls:
  - the generated tour of the Ribeira card: step 0 is the teaser at the hero (two sentences at most, names the point, says the minutes);
    every block that has something to tell has one step, in the card's order; the skipped types have none; each line under its cap; no
    sentence twice; no two links in a row the same; EN and PT; the minutes computed;
  - a card changed through the Editor's own block controls (a block removed, one added): the tour follows;
  - a generated tour plays from start to end through the real Guide button (Next through every step, the try steps answered), with
    captions and the focus on each step's block, and the games on the tour still planned on it;
  - Make it mine copies the steps; an edited step stays after Regenerate of another; the changed-blocks notice and Add them;
  - Write it better with AI with a stubbed model: the before and after shown, nothing changed until accepted;
  - another point's "A tour of {point}?" opens its generated tour;
  - a mutation (`MUTATE=1`): e.g. the teaser dropped, a block skipped, a link repeated: their named checks turn red.
- Then, one at a time: test9t (and 9tb), test9se and test9sf, the files of every module you change (README's "Prove it": likely test8o,
  test9da, test9e, test9k, test9p, test9r for the Editor's tab), the self-test `tests\runtest8.ps1 -tabs guide` (bad=0). Never
  `-tabs guide,words`.
- Update `PROTOTYPE_STRUCTURE.md`, `README.md` and `tests_baseline.json`.

## Limits

- Edit only files under lab_src (captures in the scratchpad). Do not commit, do not publish. No keys in files, no live model, no
  downloads, no voicebench, no Unity.
- Write patch scripts with the Write tool (bash heredocs mangle apostrophes); keep each file's line endings.
- A check that reads a state right after an action races the app's timers: `until(...)`, never a fixed wait.
- If you stop a background loop, kill its child processes too (round 15b's leftover runs overlapped other tests).
- If a usage limit stops you, your work is saved on disk; say in your last message which parts are done.

## Report (your final message)

What was built where (file, Unity path); the block-type table; the paths of `tour_en.md`, `tour_pt.md` and the captures with one line
each; every test file run with its numbers and time; the mutation; the CPU load; and 3-5 questions for the developer about the
generated tour's words and rules.
