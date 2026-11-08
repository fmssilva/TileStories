You are building round 6 of the guidance system in a browser prototype, the "Block Components Lab". It lives in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app,
so every feature must be buildable in Unity: name its Unity path. Work only in the Lab; do not touch any Unity project file.

READ FIRST (every line):
- `../../_3.8_Guidance_Study.md`:
  - section 8, round 6 (this round);
  - section 3.2, the item's fields, the read-back and the warnings;
  - section 3.4, the visitor's controls;
  - section 4, the presentation with each Unity path;
  - section 7, "+ Add guidance", templates, Duplicate, Delete with Undo, drag;
  - section 9, the decisions.
- `PROTOTYPE_STRUCTURE.md`: the rules, the modules, "Where a change goes" and the row-demo contract.
- `README.md`: "Prove it" and "Levels", including the short level 1.
- `../../skills/Feature_Prototyping.md`: step 3.3 (look until it is right, then test) and step 5.
- `brief_round5.md`, for the shape and the way of working, which stay the same.
- Rounds 1-5, which this round extends in place: `src/guidance/*.js` (`present.js` draws every tip) and `tests/test9e.js`
  to `tests/test9i.js`.
- The guide's existing focus: the spotlight already cut out with an even-odd fill, in `src/guide/focus.js`; its test is
  `tests/test8j.js`.
- The dock's conversation, in `src/guide/dock.js`.
- The spec `../../_3.3_POI_Card_Styles.md`, section 11 "Guidance": read it, do not edit it (the Architect does).

## This round

**6.1 Focus and motion on the phone.**
- **Focus**, per item: None / Glint / **Ring** (today's) / Spotlight.
  - Glint: the element shines once, a light sweep.
  - Spotlight: the rest dims to 0.2-0.6, an item row, with the target cut out. Reuse `guide/focus.js`'s cut-out; do not copy
    it. Spotlight is offered only for Essential items and walkthrough steps: the field says why when it is not offered.
- **The mascot's hop:** today's move to its place beside the target becomes a short 0.45 s arc hop.
  - On the wall, **Walks to it** is a third choice for Mascot goes (Beside it / Stays / Walks to it).
  - Walks to it is a slower walk along the bottom, then up to the point.
- **Shows the gesture** (a ghost finger): a translucent finger that does the gesture once, beside the target.
  - Before / After: a drag across.
  - Swipe strips: a swipe.
  - Tap a marker: a tap.
  - It is an item field (On / Off), on by default for those three.
- **The bubble's look** as the study says: in 220 ms (scale 0.94 → 1 and fade), out 160 ms. Check today's look against it
  and fix only what differs.
- **Button** (an item field): None / Show me / Start the tour / Open the card / Ask about the view. A chip in the bubble, as
  How does this work? already has (round 4).
- **Reduce Motion:** no hop (the mascot appears there), no glint sweep (a still highlight), a still ring, the ghost finger
  shown still at its end. The words stay.

**6.2 Accessibility.**
- The bubble is announced politely (`aria-live`, as today) and **stays until dismissed** when the visitor's Settings › The
  guide › **Tips stay until I tap them** is On. This is WCAG 2.2.1. Default Off.
- No spoken tip while a screen reader is on. In the Lab this is a stand-in switch in Preview & Test › The visitor:
  "Screen reader on".
- Every tip that showed also lands in the dock's conversation under **Tips**, so a bubble is never the only channel.
- Settings › The guide gets **Tips**: All / Essentials only / Off ("You can always tap ✦ for help"). Add **Show the tips
  again**, which resets the memory, with an Undo toast and no popup.

**6.3 The full item form in the Editor**, per item:
- every field group of study 3.2: Identity, Words, About, Show, When, How much.
- **Says**: 1-4 variants that rotate, never the same twice in a row.
- Slots: `{point}` `{year}` `{button}`.
- An optional line per audience, and a returning-visitor line.
- **Only if** chips; earliest and latest minute; **Teaches**; **After a dismissal**: Wait ×3 / Not again this visit / Never.
- The read-back line under each item, as today.
- **Named warnings**, each in words, naming the item:
  - an item that can never show (for example, its kind is Off, Only if contradicts itself, or Done when is already true);
  - a line too long for 2 lines in either language;
  - a line duplicated in another item.
- **+ Add guidance**, with templates: Tip about a control, Remark about the point in view, Offer a tour, Moment, Step of a
  sequence, Blank.
- **Duplicate.**
- **Delete**, with an Undo toast and no popup. The developer's popup rule is in the memory "popup-buttons-ux-and-per-button-tests"
  and in `PROTOTYPE_STRUCTURE.md`. Read it.
- **Drag to reorder** inside a group. This is the tie-break and a sequence's order. The order across kinds stays the Mix
  table.
- Every field row has its demo. Most show the item on the phone with the new value.

**6.4 Scenes in Ask Azu's console**, each with its ✓ check:
- "Spotlight on an Essential".
- "The ghost finger on Before / After".
- "Tips stay until tapped".
- "Screen reader on: nothing spoken, the tip in the dock".

## How you work (the same as round 5)

1. After each change: only `python build.py`. Its check must read 0/0/0/0.
2. Then run it, capture it, and judge each capture yourself.
   - Captures: 390 px, light and dark.
     - Phone: each Focus; the hop's end and the walk's end; the ghost finger mid-gesture; a bubble with a Button chip;
       Reduce Motion's still versions; the dock's Tips; Settings › The guide › Tips.
     - Editor: the full item form open; each warning; + Add guidance with its templates; the Undo toast after a delete;
       the drag mid-way.
   - Judge:
     - one focal point;
     - nothing covering its target, the Navigation Bar or the talk button, or other words (0 px over text);
     - text 4.5:1 or more; the spotlight's dim still leaves the cut-out readable;
     - 44 px targets;
     - premium, modern, minimal.
   - Fix and capture again until it is right.
   - Save the final captures in
     `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g6\`.
3. Only then write the tests, and run them **once, at the end**.
   - Before you run them, check the CPU load:
     `powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`.
   - Above about 40 %: say so and wait.

## Tests (level 1, the short one: 5-7 min)

- `tests/test9j.js` (Playwright with the installed Edge) checks:
  - each Focus;
  - Spotlight offered only where allowed;
  - the hop and the walk (their end position, beside the target, 0 px over its words);
  - the ghost finger;
  - each Button chip's action;
  - Reduce Motion;
  - Tips stay until tapped;
  - the screen-reader stand-in;
  - the dock's Tips;
  - Settings › Tips (All / Essentials only / Off) and Show the tips again with its Undo;
  - every field of the form, pressed once by a real click: each button of the form;
  - each warning (made to appear, then fixed);
  - each template;
  - Duplicate, Delete and Undo;
  - the drag reorder changing the tie-break;
  - every new row's demo;
  - the four scenes.
  - Waits use `until(...)` with a cap; never a fixed wait over 0.5 s.
- A mutation (`MUTATE=1`): for example, the spotlight offered on any item, or Delete with no Undo, so the named checks turn
  red. Then restore.
- `test9e.js` (round 1's items and present) and `test8j.js` (the guide's focus) must stay green. Change them only where a
  field moved, and list each changed check and why.
- At the end, one run at a time, never two at once:
  1. `node run_tests.js --only=test9j,test9e,test8j`
  2. `MUTATE=1 node tests/test9j.js`
  3. From PowerShell, not Git Bash: `powershell -ExecutionPolicy Bypass -File tests/runtest8.ps1 -tabs guide,words`. It must
     give bad=0.
- Do not run `--changed`, the full `run_tests.js` or the self-test of every tab. The Architect runs level 2 after this round.
- Update `PROTOTYPE_STRUCTURE.md` (the modules' rows and their tests column, with test9j), `README.md` (test9j) and
  `tests_baseline.json`.

## Limits

- Edit only files under lab_src. Do not commit, do not publish.
- No keys, voicebench, mascot model files or Unity.
- No CSS-only effects. Name each one's Unity path in the report:
  - glint: a sweep texture on a `VisualElement` with a translate tween;
  - spotlight: Painter2D `FillRule.OddEven`;
  - hop: a translate tween on an arc;
  - ghost finger: a sprite with a translate tween.
- One home per feature: extend the existing modules. No wrappers, and no re-assignment of another module's function (the
  build refuses them).
- Write patch scripts with the Write tool: bash heredocs mangle apostrophes.
- If a usage limit stops you, your work is saved on disk. Say in your last message which parts are done.

## Report (your final message)

- What was built, part by part, with the Unity paths.
- The new rows and their defaults.
- The captures (paths), what you judged and what you fixed.
- The tests: test9j, the mutation, and every check you changed in another file and why.
- The CPU load.
- The results of the end-of-round runs.
- 3-5 specific questions for the developer about the look and the flow.
