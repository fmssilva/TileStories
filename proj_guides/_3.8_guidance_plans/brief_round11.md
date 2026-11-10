You are building round 11 in a browser prototype, the "Block Components Lab". It lives in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app,
so every feature must be buildable in Unity: name its Unity path. Work only in the Lab; do not touch any Unity project file.

The developer asked for this round on 2026-11-10, after seeing the guide's panel during a tour: it "takes too much of the
screen and is not modern, premium, vibrant, minimal or wow". The Architect drew four static layouts; the developer said:
"build all so I can see and choose the options we should keep in the editor".

READ FIRST (every line):
- The four static layouts, plain HTML inside `<x-dc>` (read them as markup), and their captures:
  `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\gp\project\`
  `Main.dc.html` (A · Paper glass), `InkGlass.dc.html` (B · Ink glass), `Player.dc.html` (C · Player sheet),
  `Prompter.dc.html` (D · Teleprompter), `Morph.dc.html` (the ✦ growing into the panel), and the PNGs in `..\shots\`.
  Each layout shows a Mini state (left) and an Open state (right). Their colours passed 4.5:1 in the worst case (the panel over
  black or over white): `..\contrast.js`. Keep those colours or re-run that script for any you change.
- `PROTOTYPE_STRUCTURE.md`: the rules, the modules, "Where a change goes", "Each row's demo" (`rowDemos`).
- `src/guide/dock.js`, `dock.css`, `controls.js`, `readalong.js`, `tour.js` (the music bed: `musicOn`, `musicDuck`),
  `src/world/guide-button.js` (the ✦ and its menu), `src/guidance/` Fun's Moments (the water glass), `src/editor/demo.css`.
- `README.md`: "Prove it" and "Levels". `../../skills/Feature_Prototyping.md`: step 3.3 and step 5.
- `brief_round10.md` and its "Lessons", for the way of working.
- The spec `../../_3.3_POI_Card_Styles.md`, section 11 (the Guide tab, the dock, Fun) and section 16 (Unity 6.3 limits: no
  backdrop blur, no USS gradients). Read it, do not edit it (the Architect does).

## This round

**11.1 The guide panel's Look: four, chosen in the Editor.**
- A new row in Guide › 1 · The tour › Guide panel: **Look**: Paper glass / Ink glass / Player sheet / Teleprompter. Default:
  Ink glass (the Architect's pick; the developer chooses later). Each look is built as drawn, working, in light and dark.
- Two states in every look, on the dock's existing heights: **Mini** (the pill / capsule / mini player / one line) and **Open**
  (the panel). Map them without breaking the existing heights: Compact = hidden (as today), Mini = the look's Mini, Peek = Open
  with the typed question focused, Full = Open with the whole text shown ("Read all" / Aa). Say how you mapped them.
- Every look has, working:
  - **Speed**: a chip cycling 0.5× · 0.75× · 1× · 1.25× · 1.5× · 1.75× · 2× (the visitor's; the Editor's Speed is its default,
    its range widened to 0.5-2).
  - **Previous / Next part** (the stop before / after), **Pause / Play**, **Stop** (with today's Undo toast).
  - **The progress bar**: one segment (or dot) per stop, the knob on where the voice is; a drag (a real drag, 44 px hit
    height) seeks: to that stop, and within it to the sentence at that place. The time "2:10 / 6:30" from the words' timing.
  - **The voice moving**: the bars (or ring) move only while the voice speaks: with a recorded or live voice from its level
    (Lab: an AnalyserNode; Unity: `AudioSource.GetOutputData`), with the device voice from its word events. Still when paused.
  - **The words like a teleprompter**: the sentence read now, its spoken word marked; the line before faded above it and the
    next below; they move up as the voice goes on, never in fixed bubbles. A control shows or hides the words (Aa), and Read all
    shows the whole text.
  - **Ask**: by voice (today's talk button) and typed, and **suggested questions** ready to tap (the point's questions, as Ask
    Azu has them today).
- **The ✦ grows into the panel** (`Morph.dc.html`): a tap on the ✦, or a tour starting, widens the ✦'s circle into the pill
  and then the panel (about 320 ms); the ✦ fades out and stays hidden while the panel shows; closing shrinks it back into the
  ✦. Reduce Motion: a crossfade. Unity: the panel's width, height, radius and position animated (USS transitions or
  `experimental.animation`), the ✦'s opacity.
- Where the static layouts lack something today's dock has (the hide chevron, swipe down to Compact, the step name, captions'
  own bubble, the talk states), keep it in the look's own style and say where it went.
- Editor rows for the parts (the framework rule: every good option, a good default): for example Words (Teleprompter /
  Off), Voice moving (On / Off), Suggested questions (On / Off), Speed button (Shown / Hidden). Each row with its demo.
- No backdrop blur (Unity 6.3 has none): the glass is a translucent fill with a light edge; a gradient is a ramp texture.

**11.2 Fun never during a tour, never over words.**
- The water glass ("A sip of water? Tap the glass.") showed during a tour, beside Azu, over the picture's caption. Fun's
  Moments (and every Fun item) are held while a tour plays (`G.on` and not paused): a gate with its reason in Why not now?
- A moment's prop (the glass, the cookie) is placed beside Azu with 0 px over any words; if there is no room, it waits.
- Do not build new entertainment: the developer asked for research first on what real guides do; it comes in a later round.

**11.3 The Editor's ▸ becomes a play button.**
- The row demo's ▸ after a label (`.row > label.rd::after`, `editor/demo.css`) becomes a green round play button, larger (about
  22-24 px), that reads as Play at once; the label keeps its class `rd` and the same press. Contrast 3:1 or more against the
  Editor's dark background.

**11.4 The music bed.**
- Today: three synthesized sine tones (`musicOn`), On / Off only. Add, in Guide › 1 · The tour:
  - **Music volume**: 0-100 %, default 15 %; **Under the voice**: how low it ducks while Azu speaks (default 30 % of its
    volume).
  - **Track**: a list: "Soft drone (built in)" first, then the tracks of a manifest `../music/tracks.js` (like
    `voicebench/out/voicesets.js`: id, name, file, licence, source; empty for now: the Architect adds approved tracks), and
    **Add your own…** (a file picker; the file plays for this session; Unity: an AudioClip set in the POI Editor).
  - Each row with its demo (the music plays a few seconds at that setting, ducking under a line of Azu's).

## How you work (the same as round 10)

1. After each change: only `python build.py`. Its check must read 0/0/0/0.
2. Capture, judge, fix, until it is right. Captures at 390 px, light and dark: each look in Mini and Open mid-tour (the words
   moving, the bars up), the ✦ mid-growth, the speed chip at 1.5×, the bar being dragged, Read all, the typed question with
   suggestions, the Editor's new rows and the green ▸. Judge against the static layouts: the same layout and spacing; one
   focal point; nothing over the words; 44 px targets; text 4.5:1. Save the final captures in
   `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g11\`.
3. Only then write the tests, and run them once, at the end, after checking the CPU load
   (`powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`; above about 40 %: say so and wait).

## Lessons (read them)

- **15 test files find the dock by its classes** (`grep -ln "gdock\|gd-grab\|h-mini\|gd-hide\|gb-pause\|\.gdbub" tests/*.js`:
  test8k, 8o, 8s, 8u, 9da, 9db, 9e, 9f, 9g, 9h, 9i, 9ja, 9k, 9l, 9n). Keep those hooks on the elements that do the same job in
  every look (`.gdock`, `.h-mini`, `.gd-grab`, `.gd-hide`, `.gb-pause`, the talk button…), give every new element its own
  class, and list any check you had to change, with why.
- A check that reads a state the instant after an action races the app's timers: set and read in one call, or `until(...)`.
- Level 3 (2026-11-09) found tests too slow on a busy machine: keep each new file under 300 s.
- The self-test: one big tab per walk. Never `-tabs guide,words` (65 min).

## Tests

- `tests/test9o.js` (Playwright with the installed Edge), under 300 s: each look's Mini and Open (its parts present and
  inside the phone, 0 px over the words, 44 px targets); the speed chip's seven steps (the voice's rate follows); Previous /
  Next part; Pause / Play; Stop and Undo; a real drag on the bar seeking to a stop and a sentence; the bars moving while the
  voice speaks and still when paused; the words moving up, Aa and Read all; a typed question and a suggestion tapped; the ✦
  growing into the panel and back, and hidden while open; Reduce Motion's crossfade; Fun held during a tour (Why not now? says
  so) and the glass never over words; the green ▸ (its size and contrast) still pressing the demo; the music rows (the volume
  and the ducking read from the gain); every new row's demo.
- A mutation (`MUTATE=1`): for example Fun not held during a tour, so the named checks turn red. Then restore.
- At the end, one run at a time:
  1. From PowerShell, each its own call: `powershell -ExecutionPolicy Bypass -File tests/runtest8.ps1 -tabs guide`, then
     `-tabs card`. Each must give bad=0.
  2. `node run_tests.js --only=test9o,<every file of the grep above>` (about 60 min; never the `_live` files).
  3. `MUTATE=1 node tests/test9o.js`
- Update `PROTOTYPE_STRUCTURE.md` (the modules' rows and tests), `README.md` (test9o) and `tests_baseline.json`.

## Limits

- Edit only files under lab_src (and create `../music/tracks.js` with an empty list). Do not commit, do not publish.
- No keys, voicebench, mascot model files or Unity. Do not call the live model. Do not download music.
- One home per feature: extend the existing modules; no wrappers, no re-assignment of another module's function.
- Write patch scripts with the Write tool: bash heredocs mangle apostrophes.
- If a usage limit stops you, your work is saved on disk. Say in your last message which parts are done.

## Report (your final message)

- What was built, part by part, with the Unity paths; how the states map to the dock's heights; where today's dock parts went.
- The new rows and their defaults.
- The shared code you changed, and which test files cover it.
- The captures (paths), what you judged and what you fixed; anything of the static layouts you could not keep, and why.
- The tests: test9o, the mutation, and every check you changed in another file and why.
- The CPU load and the results of the end runs, with each file's time.
- 3-5 specific questions for the developer about the four looks.
