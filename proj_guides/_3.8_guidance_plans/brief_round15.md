You are building round 15 in a browser prototype, the "Block Components Lab". It lives in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app:
every look you make must be buildable there (UI Toolkit: USS, VisualElements, a render texture for 3D; no CSS-only effect that has no
Unity path: say each one's Unity path). Work only in the Lab; do not touch any Unity project file.

This round is **the design pass of the AI guide**: every surface the visitor sees from the guide (Azu, the tour, its games, Azu
speaking up, Ask Azu), captured, measured and judged, then fixed where a rule is broken. It adds no feature and changes no behaviour
(timings, rules, the scheduler stay as they are). The Editor's Guide tab was organized in round 14 and is not in this round.

READ FIRST (every line):
- `C:\Users\franc\Desktop\TileStories\proj_guides\_3.8_guidance_plans\guide_polish_plan.md` section 4 (the design pass) and section 6 (the order).
- `PROTOTYPE_STRUCTURE.md`: the rules, the modules, "Where a change goes". The guide's look lives in `src/guide/` (the panel, the
  captions, the focus, the dock), `src/mascot/` (Azu 2D and 3D), `src/guidance/present.js` and `editor.css` (bubbles, the beat cards,
  the strip), `src/world/guide-button.js` (the ✦ and its menu), `src/agent/` (Ask Azu's answers and link cards). Grep before you edit.
- `README.md`: "Prove it" and "Levels". `../../skills/Feature_Prototyping.md`: step 3.3 and step 5.
- `C:\Users\franc\Desktop\TileStories\proj_guides\_3.3_POI_Card_Styles.md` section 11 (the guide) and `C:\Users\franc\Desktop\TileStories\proj_guides\_3.8_Guidance_Study.md` (the look of the guidance: section 5).

## The developer's decisions (do not reopen them)

- Only the AI guide domain until it is finished and polished (2026-11-11). The order: round 14 the Editor's organization (done),
  **round 15 the design pass (this)**, then four features (a tour resumes on the next visit; short and deep per step; Ask Azu during a
  game; a point with no written tour).
- Values in the Lab are experiments: when a look choice is not a broken rule but a taste, **do not pick for the developer**: show the
  options side by side (below, "The open look questions").

## What to capture (390 px wide phone, the Lab's phone; dark and light app theme; the developer's Editor is dark)

Each surface in **Ink glass and one other panel Look** (Paper glass), at **every dock height where it shows** (Mini, Compact, Open),
on the card and on the wall where it appears:
1. The ✦ (Guide button) and its menu; Azu beside the ✦; Azu on the wall (beside a marker); Azu on the card; Azu at a beat card's edge.
2. The guide panel's four Looks (Paper glass, Ink glass, Player sheet, Teleprompter) at Mini, Compact and Open, with a tour playing.
3. Captions; the focus (Glint, Ring, Spotlight) on a block and on a spot in a picture.
4. Every bubble form: Tip, Invitation, Remark, Moment (and Mascot Off's plain bubble).
5. Every beat card: the quiz's promise (and its strip), the quiz, Guess the year, Don't say it! (the announce, the word card, the result),
   Find it on the wall (the seek ring, the hint, the arrow at the edge), the breather and the cookie.
6. Ask Azu: typed, by voice (the mic's states), an answer with its link cards, Show me, the trace.
7. The first pages (Start the app › A first run) and Welcome back.

Save them in `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g15\`, named
`<nn>_<surface>_<look>_<height>_<theme>.png`, and write `g15\index.md` listing each with your judgment in one line.

## The rules each capture is judged by (measured, not eyeballed)

- **One focal point:** at any moment one thing asks for the eye (Azu speaking, or a beat card, or a bubble), never two competing.
- **0 px over the words being told:** no bubble, card, strip, Azu or focus ring covers a word of the block being narrated, or the
  captions' current line.
- **44 px targets:** every tappable part of the guide (chips, buttons, the ✦, the mic, Azu's tap area, a link card) is at least 44 × 44
  CSS px (its hit area may be larger than its drawing).
- **4.5:1 text contrast** against what is really behind it (glass Looks: measure over the lightest and darkest parts of the wall and the
  card behind; if a glass panel fails, its fix is a scrim or a stronger tint, not a smaller blur).
- **Inside the phone:** nothing clipped by the phone's edges, the notch area or the dock; nothing off-screen at 390 × 844 and at the
  shorter 390 × 712 window.
- **Reduce Motion** (the preview bar's Motion): the same surfaces still read; no motion needed to understand them.

Write these as arithmetic checks in a new test (below), then look at every capture yourself too: a check can pass on an ugly screen.

## The open look questions (the developer decides; you prepare)

For each, capture the options **side by side in one image** (2-3 options, the current one first, labelled), say what each costs in
Unity, and recommend one in one line. Do not change the default.
1. Round 1's **PT opening line** (the Opening message's Portuguese words and how they fit the bubble: two lines max).
2. What **"Mascot Off"** means on the phone (today: the ✦ alone and a plain bubble at the target): the options a visitor would see.
3. **Azu's size beside a marker** on the wall (Small, Medium, Large against the marker and its label).
4. **The ✦ on the card** (where it sits on the card, and whether it shows during a tour).
5. **A ring for The ✦ with the 3D model** (the 3D Azu beside the ✦: a ring or none, how it reads against the wall).
6. **"The" mid-sentence in the remark lines** (a point's name inside a remark: "The Ribeira Palace" vs "the Ribeira Palace").
7. **A group's own shorter gap** and **the remarks counting in Content's cap**: these are rules, not looks: show each on Simulate a
   visit's timeline (Preview & Test) as it is and with the change, one image each.

## Fix as you go (only broken rules)

- A broken rule above is a defect: fix it in the module that draws it (one home per feature; no wrappers; no re-assignment of another
  module's function), then capture again. Say each fix: before and after (the numbers), the file and the Unity path.
- A taste question that is not a broken rule: do not change it; list it in your report as a proposal with a capture.
- Keep every Editor row's key, demo, default and words. A new Editor row (if a fix needs one) has its demo.

## How you work

1. After each change: only `python build.py`. Its check must read 0/0/0/0.
2. Capture, judge, fix, until it is right (`skills/Feature_Prototyping.md` step 3.3).
3. Only then the tests, run once, at the end, after checking the CPU load
   (`powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`; above about 40 %: say so and wait).

## Tests

- `tests/test9s.js` (Playwright with the installed Edge), under 300 s: the rules as arithmetic, on the real surfaces opened by the app's
  own functions or real clicks: each guide target ≥ 44 × 44 (its hit area); each guide text ≥ 4.5:1 against its computed background (for
  glass, the panel's own tint over a light and a dark sample of what is behind); 0 px between each bubble / beat card / strip / Azu and
  the words being told (the told block's word boxes and the captions' line); every surface inside the phone at 390 × 844 and 390 × 712;
  one focal point (at most one of: a bubble, a beat card, a speaking Azu with its bubble, at a time). Name the checks so a red one
  says which surface and which rule. If the file nears 300 s, split it (test9s / test9sb).
- A mutation (`MUTATE=1`): for example a beat card's chip made 36 px tall, or a bubble moved over the told words: its named checks turn
  red. Then restore.
- At the end, one run at a time: `node run_tests.js --only=test9s` (and test9sb), plus the files of every module you changed (find them in
  README's "Prove it"; for the panel expect test8o, test9n, test9o; for the beat cards test9p, test9q; for Azu test8x, test9da), the
  self-test `-tabs guide` (bad=0), then the mutation. Never `-tabs guide,words`.
- Update `PROTOTYPE_STRUCTURE.md` (what changed where), `README.md` (test9s) and `tests_baseline.json`.

## Limits

- Edit only files under lab_src (and write captures in the scratchpad). Do not commit, do not publish.
- No keys, voicebench, mascot model files or Unity. Do not call the live model. Do not download anything.
- Write patch scripts with the Write tool: bash heredocs mangle apostrophes.
- A check that reads a state right after an action races the app's timers: `until(...)`, never a fixed wait.
- If a usage limit stops you, your work is saved on disk. Say in your last message which parts are done.

## Report (your final message)

- `g15\index.md`'s path and a summary: how many surfaces, how many passed every rule first time, what failed.
- Every fix: the rule, before and after in numbers, the file, the Unity path.
- The open look questions: for each, the image's path, the options, the cost in Unity, your one-line recommendation.
- Proposals (taste, not rules), each with a capture.
- The tests: test9s (and 9sb), the mutation, every other file run and its time; the CPU load.
- 3-5 specific questions for the developer about the look.
