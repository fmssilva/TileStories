You are building round 13 in a browser prototype, the "Block Components Lab". It lives in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app,
so every feature must be buildable in Unity: name its Unity path. Work only in the Lab; do not touch any Unity project file.

Round 12 built Fun › On the tour: the tour plan and its rhythm, pairs, the 4 s Not now, the breather, the promised quiz and
Guess the year (`brief_round12.md`). This round first applies the developer's four answers on round 12, then builds the other
half of the games on the same base: **Don't say it!** (a game inside a step) and **Find it on the wall**.

READ FIRST (every line):
- `../../_3.8_guidance_plans/research_tour_entertainment.md`: sections 3 (the model), 3.5, 4.4, 4.7, 5.4 (every line of it), 6.
- `brief_round12.md` and its report's summary in `../../__Architect_Handoff.md` (section 0, "Round 12" and "The developer's
  answers on round 12").
- `PROTOTYPE_STRUCTURE.md`: the rules, the modules, "Where a change goes", "Each row's demo" (`rowDemos`).
- `src/guidance/` (catalog, scheduler: `gdTourPlan`, service, present: the beat card `gdBeat*`, scenes, editor), `src/guide/tour.js`
  (the word-time events that drive the lit word; the recorded sets' word times; Previous), `src/guide/dock.js` (heights, Looks,
  `talkState`), `src/mascot/azu2d.js` (pokes, reactions, the blush), `src/world/` (the wall: the middle point settling, the ✦),
  `src/audiences/audiences.js` (the AI drafts with a **Reviewed** tick; reuse that pattern, as round 12's quiz did).
- `README.md`: "Prove it" and "Levels". `../../skills/Feature_Prototyping.md`: step 3.3 and step 5.
- The spec `../../_3.3_POI_Card_Styles.md`, section 11 (Fun › On the tour, written after round 12) and section 16 (Unity 6.3
  limits). Read it, do not edit it (the Architect does).

## The developer's decisions (do not reopen them)

- Everything in round 12's brief still holds (the group, its fixed gates, its rhythm, A game starts with a 4 s Not now, two Not
  nows end the games, AI drafts need **Reviewed**, answers by tap always and by voice only as an extra).
- **13.0, the answers on round 12 (2026-11-10):**
  1. When the quiz is promised in a tour, **Azu's quiz replaces the tour's "Test yourself" step** (the card's quiz block): the
     tour skips that step (the plan says so; the block stays on the card to play freely). With no quiz planned, the step plays
     as before. At round 12's finale the two quizzes were on screen at once.
  2. **The promise and its quiz count as one beat** in the tour's cap and rhythm. The demo tour (1:24 of narration, room for
     2) then gets the quiz pair and one more beat. Update Why not now? and the Editor's plan line.
  3. The promise's **4 s buttons stay up while step 1 starts** (Deal / No quiz, thanks): the tour no longer waits about 9 s.
     The buttons must not cover the words being told (place them like the Mini's controls or above the dock; measure 0 px).
  4. **The Cookie stays** as the breather's variant (no change; it plays in tours without the quiz).
- Don't say it!: Family and Explorer, Lively only, never Solemn, one game a tour, **In a step** (its first use). The word is
  suggested by the Editor from the tour's own text per language and needs the author's **Reviewed** tick; a language without a
  reviewed word does not offer the game. The announce's chips: **Let's play** / **Not now** for 4 s; waiting is Let's play.
- Find it on the wall: Between steps, the wall only (a tour of a card whose point is on the current wall), once a tour, every
  audience, **Calm too**; 20 s at most, then Azu shows it; never while tracking is lost.

## This round

**13.0 The four answers** (above). Look again at round 12's captures of the promise and the finale after the change.

**13.1 Don't say it!** (research 4.4 and 5.4, every step of it, its edge cases too):
- The Editor's word suggestions per language (2-4 times in a span of 1-2 steps, 45-120 s, not in the span's first sentence,
  4 letters or more, a noun the story is about), each with its count and span ("palace: 3 times in steps 2-3 (70 s)"); the
  author picks one per language and ticks **Reviewed**; warnings for a count of 1 or over 4. Per point, in Specific Marker ›
  Guide script (beside round 12's quiz question and guess). The demo point gets a reviewed word in EN and in PT.
- The announce at the seam before the span, the word card pinned by Azu during the span, the tap windows from the word times
  (start to end + **2.5 s**), caught (flinch, blush, the "Caught 1" pip, without stopping the voice), false alarms ("Nope!"),
  pokes off during the span (three quick taps still tickle), the result line at the next seam with **the missed slip played
  back** (the recorded clip from 1.2 s before the word to 0.3 s after; with the device voice, the phrase spoken again), the
  score said again in the quiz finale's callback.
- The edge cases: Pause / drag / Ask freeze the windows; Previous never counts a slip twice; Compact and Mascot Off get a
  **Heard it!** button by the ✦ (44 px); a screen reader: not offered; Reduce Motion: the pip only; a language change or a
  hidden step ends it quietly; captions light the word like every word.
- Editor rows: The word (suggested) · The span · Tap window (2.5 s) · False taps · Play my slip back (On).

**13.2 Find it on the wall** (research 4.7): "Lower the card and find the ship on the real wall. Point me at it." The card
goes to Mini, the wall view returns with a soft seek ring in the middle; when the point settles in the middle (today's
event, 1.2 s still), "That's it!", the point lights with a short glint; **Hint after** 10 s ("A little to the left…", from the
point's screen direction); **I give up** (and after 20 s) Azu walks to it and shows it; then back to the card and the tour
goes on. In the Lab the wall drag stands in for the phone. Editor rows: The point · Says · Give up after (20 s) · Hint after
(10 s). The demo point gets one "find" (a detail on the wall that the tour's text names).

- Every new row with its demo (the row shows the beat on the phone, in a tour, now). Ask Azu's scenes for each (research 5.4's
  "Tests the developer can press": catch every slip; miss one, hear it back; tap too early; pause in a window; the Compact
  dock's Heard it!; Portuguese with its own word; and for 4.7: find it; the hint; I give up; tracking lost).
- Words: new `tour_*` words, EN and PT (European Portuguese, the visitor addressed politely in the third person).

## How you work (the same as round 12)

1. After each change: only `python build.py`. Its check must read 0/0/0/0.
2. Capture, judge, fix, until it is right. Captures at 390 px, light and dark, in the default Look (Ink glass) and one other,
   **at every dock height where the part shows** (Compact, Mini, Open): the promise's buttons over step 1, the announce with
   its word card, a catch (the pip), a false alarm, the result with the slip played back, Heard it! at Compact, the seek ring,
   the hint, That's it!, I give up, the Editor's new rows and the plan line. Judge: one focal point; nothing over words being
   told; 44 px targets; text 4.5:1. Save them in
   `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g13\`.
3. Only then write the tests, and run them once, at the end, after checking the CPU load
   (`powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`; above about 40 %: say so and wait).

## Lessons (read them)

- Round 12's review found: "Close!" as the line for every wrong answer (500 years is not close to 250): read each line
  against its worst case. Three older checks were written for the old state and only level 2 found them: test9m (Settings ›
  The guide's rows: now 7), test8u (the scene count: now 57, and its Stop), test8w (it set Azu's state on the element; the
  dock's timer copies the app's own state at every height). **Before your end run, grep the tests for what you change**
  (scene counts, Settings rows, row counts, step counts, `dataset.state`) and run those files too.
- The Guide tab's full redraw takes about 1.3 s: never redraw the Editor on a timer during a tour; update only the plan line.
- A check that reads a state the instant after an action races the app's timers: `until(...)`, never a fixed wait. A
  Playwright click waits for a still button, so count at the moment the click lands (test8u now does).
- Keep each new file under 300 s. The self-test: one big tab per walk; never `-tabs guide,words`.
- Do not call the live model in tests: Draft with AI and the word suggestions' AI part (if any) are tested with `sampleFn`
  stubbed, as the audiences' tests do.

## Tests

- `tests/test9q.js` (Playwright with the installed Edge), under 300 s: 13.0 (the step skipped when the quiz is promised and
  played when not; the pair as one beat in the plan; the promise's buttons up while step 1 is told, 0 px over its words);
  Don't say it! (the suggestions and their counts in EN and PT, Reviewed, the windows from the word times, a catch, a false
  alarm, one count per slip with Previous, Pause freezing a window, the result lines, the slip played back, Heard it! at
  Compact and with Mascot Off, the screen reader, a language change); Find it on the wall (a real drag on the wall to the
  point, the 1.2 s settle, the hint's direction, I give up at 20 s, tracking lost, the wall only); every new row's demo; the
  scenes. If test9p's checks change with 13.0, change them there and say why.
- A mutation (`MUTATE=1`): for example the tap window ignoring its end, so the named checks turn red. Then restore.
- At the end, one run at a time: the self-test `-tabs guide`, then `-tabs marker,global`, then `-tabs card` (each must give
  bad=0); then `node run_tests.js --only=test9q,test9p,test8k,test8u,test8w,test9e,test9f,test9g,test9h,test9i,test9ja,test9k,test9l,test9m,test9n,test9o`
  plus every file your grep found; then `MUTATE=1 node tests/test9q.js`.
- Update `PROTOTYPE_STRUCTURE.md` (the modules' rows and tests), `README.md` (test9q) and `tests_baseline.json`.

## Limits

- Edit only files under lab_src. Do not commit, do not publish.
- No keys, voicebench, mascot model files or Unity. Do not call the live model. Do not download anything.
- One home per feature: extend the existing modules; no wrappers, no re-assignment of another module's function.
- Write patch scripts with the Write tool: bash heredocs mangle apostrophes.
- If a usage limit stops you, your work is saved on disk. Say in your last message which parts are done.

## Report (your final message)

- What was built, part by part, with the Unity paths; how 13.0 changed the plan.
- The new rows and their defaults; the per-step fields in the Guide script.
- The shared code you changed, and which test files cover it.
- The captures (paths), what you judged and what you fixed.
- The tests: test9q, the mutation, every check you changed in another file and why; the CPU load and each file's time.
- 3-5 specific questions for the developer about the games (how they feel, the words, the timing).
