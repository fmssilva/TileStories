You are building round 12 in a browser prototype, the "Block Components Lab". It lives in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app,
so every feature must be buildable in Unity: name its Unity path. Work only in the Lab; do not touch any Unity project file.

The developer asked on 2026-11-10 for a third dimension of the guide, beside teaching the app and telling the content: **what
real guides do to keep a tour alive** ("are you enjoying so far? let me just catch my breath… a sip of water", "at the end I
will be making some questions… pay attention ;)", a forbidden-word game where the visitor taps Azu). A research report was
written and the developer decided how it fits. This round builds the base and the first half of the games; round 13 builds the
other half (Don't say it! and Find it on the wall) on the same base.

READ FIRST (every line):
- `../../_3.8_guidance_plans/research_tour_entertainment.md`: sections 1, 3 (the model), 4.1, 4.3, 4.5, 5.1, 5.3, 6, 7. Its
  sections 4.4, 4.7 and 5.4 are round 13's; read them so the base you build fits them.
- `../../_3.8_Guidance_Study.md` §9, decisions 6 and 8 (the developer's choices), and §8 (the rounds).
- `PROTOTYPE_STRUCTURE.md`: the rules, the modules, "Where a change goes", "Each row's demo" (`rowDemos`).
- `src/guidance/` (catalog, scheduler, service, present, scenes, editor), `src/mascot/azu2d.js` (today's Moments: `azMoment`,
  the props, `propSpot`), `src/guide/tour.js` and `dock.js` (the tour's steps, the seams, the dock's Looks from round 11),
  `src/audiences/audiences.js` (the AI drafts with a **Reviewed** tick: `ovDraft`, `ovState`; reuse that pattern),
  the card's quiz block (`src/card/`).
- `README.md`: "Prove it" and "Levels". `../../skills/Feature_Prototyping.md`: step 3.3 and step 5.
- `brief_round11.md` and its Lessons, for the way of working.
- The spec `../../_3.3_POI_Card_Styles.md`, section 11 (the Guide tab, Fun, the dock's Looks) and section 16 (Unity 6.3
  limits). Read it, do not edit it (the Architect does).

## The developer's decisions (do not reopen them)

- It lives in a new group, **Fun › On the tour**. It keeps Fun's fixed gate (never on a Solemn point) and Liveliness (Calm
  allows only the items marked Calm too).
- **Its own rhythm**, not Fun's visit cap: about one beat per **75 s** of narration (Editor range 60-120 s), never two seams in a
  row, at most **3 a tour** (Short 2, Deep 5), the opening and the finale included. Fun's visit cap does not count them.
- Since round 11, Fun is held while a tour plays. On the tour items are the exception: they are **made for** the tour and play
  only at its seams (or, in round 13, inside a step), never over a sentence.
- **Water break and Cookie** (today's Moments) move into this group; the Water break becomes **the breather** (5.1).
- **A game starts** (no asking first), with a **4 s "Not now" chip**; after two "Not now"s, no more games this visit.
- **The quiz questions** (and, in round 13, the forbidden words) are drafted by AI from the tour's own text in the Editor, and
  each needs the author's **Reviewed** tick before it plays (as the audiences' drafts).
- **Answers by tap always**, and by voice as an extra where the mic is allowed. Never voice only.
- Defaults for the open questions (the developer may change them later): the quiz for Expert is an opt-in ("Want three tough
  questions at the end?"); Solemn points get no beats; the visitor's switch is Settings › The guide › **Games on the tour**
  (On / Off).

## This round

**12.1 The group and the tour plan.**
- Fun › On the tour, with today's group rows (On / Off, Liveliness, Audiences) and its rhythm rows: **One beat per** (75 s),
  **At most a tour** (3; Short 2, Deep 5).
- The new fields of research 3.2 that this round needs: Place in the tour (Opening / Between steps / Finale; In a step comes
  in round 13), The visitor answers by, Waits (8 s), Needs (a named warning when the point lacks it), Pairs with, Scores,
  Calm too, Audiences, Tour length.
- **The tour plan**: when a tour starts, it asks the scheduler once for its plan (the opening, the middle beats at which seams,
  the finale). The plan is fixed and readable; only what the visitor does changes it (Not now, a stop, a skip). Why not now?
  answers from it ("planned for the seam after step 4", "the tour already had 3 beats", "a Solemn point").
- **Pairs**: a promise and its payoff are one; if the tour stops for good, the payoff is offered from the ✦ menu for the rest
  of the visit, or said away in one line ("We'll save the quiz for next time.").
- The fixed gates of research 3.5. **Not now**: the 4 s chip on every game; two of them end games for this visit (said once,
  kindly).
- The Editor shows the plan of the current tour (Guide › Guidance › Fun › On the tour: "This tour's plan: …").

**12.2 The breather** (research 5.1): today's Water break moved in and made active. Azu says the line, drinks (the glass beside
Azu, 0 px over words, as round 11 placed it), then the chips **Go on** / **A question first** (8 s = Go on; A question first opens
the panel with the talk button listening, then "Back to our story."). Its prop variants by date (glass, fan June-September,
scarf December-February), never the same twice in a visit. The Cookie moves in too, as a breather variant before the last
step (or say in your report why it should go).

**12.3 The promised quiz** (research 5.3, every step of it): the promise after the introduction (the star at the end of the
progress line, in every round-11 Look; Deal / No quiz, thanks for 4 s), the choice of questions at the last seam (only steps
heard 80 %, one per third, never two from a step), the finale in the panel (tap a choice, or say it; Not sure; right / wrong
with the why; Show me goes to the block; no timer), the score lines (never mocking), Skip the quiz, the best score per point
kept on the phone.
- **Per step (Specific Marker › Guide script):** a quiz question (question, 2-3 answers, the right one, a why of 12 words or
  fewer), EN and PT, each with **Draft with AI** and **Reviewed**, as the audiences' drafts. Unreviewed questions never play.
  The demo point gets 4-5 reviewed questions written by you (from its own text) so the quiz can be seen.
- The speech path: the recogniser's text matched against the choices' words in the language ("the second", "1755").

**12.4 Guess the year** (research 4.5): before the step that holds the year, a year slider in the panel (the author's range),
Tell me, then the step says the year and Azu reacts by distance (3 bands, never mocking). The guess and the answer as two pins.
The per-step guess (a number, its range, its unit) in the Guide script.

- Every new row with its demo (the row shows the beat on the phone, in a tour, now). Ask Azu's scenes for each beat (research
  5.1 and 5.3's "Tests the developer can press": a promised quiz all right; two wrong, Show me; a stop then the quiz from the ✦;
  a skipped step gives no question; too few questions, no promise; Expert's opt-in; the breather; guess the year).
- Words: new `tour_*` words, EN and PT (European Portuguese, the visitor addressed politely in the third person).

## How you work (the same as round 11)

1. After each change: only `python build.py`. Its check must read 0/0/0/0.
2. Capture, judge, fix, until it is right. Captures at 390 px, light and dark, in the default Look (Ink glass) and one other:
   the promise with its star, the breather's chips, the quiz's question, a right and a wrong answer, the score, the year
   slider with its two pins, the Editor's new rows and the tour plan line. Judge: one focal point; nothing over words; 44 px
   targets; text 4.5:1. Save them in
   `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g12\`.
3. Only then write the tests, and run them once, at the end, after checking the CPU load
   (`powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`; above about 40 %: say so and wait).

## Lessons (read them)

- Round 11's review found: a CSS rule of the base (`.gdock.h-peek .gd-tp{flex:none}`) silently beating a Look's own rule; a
  .97 fill letting the card's text read through; Azu's state copied only by a button the Mini no longer has. **Look at every
  Look and every height** when you add a part to the panel.
- A check that reads a state the instant after an action races the app's timers: `until(...)`, never a fixed wait. A sheet
  may still be growing to its stop when a check begins (test9h's `fresh` now waits for its height to hold).
- Keep each new file under 300 s. The self-test: one big tab per walk; never `-tabs guide,words`.
- Do not call the live model in tests: Draft with AI is tested with `sampleFn` stubbed, as the audiences' tests do.

## Tests

- `tests/test9p.js` (Playwright with the installed Edge), under 300 s: the plan (deterministic, its rhythm and caps by
  length, Why not now? from it); the gates (Solemn, Calm, screen reader, never two in a row, never over a sentence); Not now and
  the two-Not-now rule; the breather (chips, 8 s, A question first, the prop beside Azu with 0 px over words, the variant by
  date); the quiz (promise and star, Deal / No, questions only from steps heard, one per third, a right and a wrong answer by
  tap, one by speech text, Not sure, Show me, the score lines, Skip, the stop-then-✦ path, too few questions, Expert's opt-in,
  unreviewed questions never playing, Draft with AI with the stub); guess the year (a real drag on the slider, the pins, the
  bands); every new row's demo; the scenes.
- A mutation (`MUTATE=1`): for example the plan ignoring the cap, so the named checks turn red. Then restore.
- At the end, one run at a time: the self-test `-tabs guide`, then `-tabs marker,global` (each must give bad=0); then
  `node run_tests.js --only=test9p,test8k,test9e,test9f,test9g,test9h,test9i,test9ja,test9k,test9l,test9n,test9o` (the guidance
  files and round 11's); then `MUTATE=1 node tests/test9p.js`.
- Update `PROTOTYPE_STRUCTURE.md` (the modules' rows and tests), `README.md` (test9p) and `tests_baseline.json`.

## Limits

- Edit only files under lab_src. Do not commit, do not publish.
- No keys, voicebench, mascot model files or Unity. Do not call the live model. Do not download anything.
- One home per feature: extend the existing modules; no wrappers, no re-assignment of another module's function.
- Write patch scripts with the Write tool: bash heredocs mangle apostrophes.
- If a usage limit stops you, your work is saved on disk. Say in your last message which parts are done.

## Report (your final message)

- What was built, part by part, with the Unity paths; how the plan and the rhythm work; where the Moments went.
- The new rows and their defaults; the per-step fields in the Guide script.
- The shared code you changed, and which test files cover it.
- The captures (paths), what you judged and what you fixed.
- The tests: test9p, the mutation, every check you changed in another file and why; the CPU load and each file's time.
- 3-5 specific questions for the developer about the beats (how they feel, the words, the timing).
