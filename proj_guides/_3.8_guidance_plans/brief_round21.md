You are building round 21 in a browser prototype, the "Block Components Lab", in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app (say each
change's Unity path). Work only in the Lab. This round is **"Speech on screen and the panels"**: how the mascot's words show while it
speaks (one smooth highlighter everywhere, the bubble that stays by the mascot, a teleprompter during tours), the guide panels smaller,
lighter and more immersive, and ±10 s.

THE CHECKLIST: `C:\Users\franc\Desktop\TileStories\proj_guides\___todo.md`, section "THE CHECKLIST", items **21.1 to 21.7, G.1 and G.2**
(and any item the Architect adds under round 21 while you work). The developer's notes and the prints they describe are above it; read
them. Mark an item `[DONE]` (replace `[ ]`) only after it is built, looked at in your captures and tested; an item you change or cannot
finish gets one line under it saying what and why. Keep the file's CRLF line endings; edit only those items.

READ FIRST: `PROTOTYPE_STRUCTURE.md` (the rules; the rows of `guide/dock.js`, `guide/readalong.js`, `guide/person.js`,
`guide/mascot-profile.js`, `guide/tour-end.js`, `guide/game-ask.js`, `guidance/present.js`), `README.md` ("Prove it", "Levels": a
**feature** round), `src/guide/readalong.js` (today's highlight: Comet, Trail, Glide), `src/guide/dock.js` (the minimal and open
panels, the panel looks of round 11), the bubble's code, `src/guide/voice.js` (`seekWord`, the recorded word times).

## What the developer asked (2026-11-13, their words, shortened)

- "The panel of the AI Guide when opened is too big, with too big elements: the speech has too big words."
- "The highlighting word by word is not smooth. Let's make some interpolation so the highlighting is smooth, leaving the highlighted
  colours vanishing behind like a comet. I think we implemented this already, but maybe only in the guide's balloon? Implement it in a
  way used in all speech text highlighting: in the balloon, in this panel, and in the blocks directly when the guide reads them."
- "We need back 10 seconds and forward 10 seconds buttons, both in the minimal panel and in the big one. In the minimal one maybe only
  the back 10 seconds button, so the whole panel stays minimal."
- "The balloon is pointing at the mascot's head, but then the mascot moves away and the balloon stays in the same place. Let the
  balloon always be near the mascot, really pointing at it. When not possible, for example during the guide where the mascot moves
  around to explain the blocks and a moving balloon would hide those blocks, have a moving teleprompter panel at the bottom of the
  screen: something with a gradient or motion, like a blurred sky, from the bottom up to the top of the teleprompter card, placed above
  the detail card and below the minimal guide panel (like the print, or better: more immersive, minimalist, modern, premium). A card
  coming from the base of the screen behind the minimal panel, going some 2 or 3 cm up, showing the text being spoken highlighted, and
  transparent, disappearing as we move forward."
  - The print (theirs, a reference): a dark glass card just above the dock; the line spoken in bright text with the current words
    underlined in gold, the line before and the line after faded and cut; a small footer "● MESTRE GIL • AUDIO STORY" and "Full Script ›".
- "The AI Guide panels, the minimal and the half screen, are black and too flat. Maybe make them more light, smooth, immersive,
  technological, with some light shapes and colours, 3D shapes, light and gradients and a little motion." Their references: a pastel
  glass app with soft pink-blue gradient cards on white (light, airy, premium), and a dark pill button with a soft green glow and a
  gradient edge (a call to action that glows).
- Standing: the mascot's words are a warm storyteller; every Editor row has its ▸ demo and ? help; messages name things, not ids; the
  Editor says "the mascot"; every good option kept as an option (never either/or: the developer picks the default); Lab features
  must be Unity-buildable (name each effect's Unity path; no CSS-only effect without one); design is checked by arithmetic in the
  test AND by looking at captures; never the live model in tests.

## The design to build (the Architect's; adjust where the captures show something better, and say what and why)

1. **One highlighter (21.2).** One module (e.g. `guide/highlight.js`) that every place showing spoken words uses: the bubble, the
   teleprompter, the open panel, and the block's own text while it is read (today's read-along). It glides: a soft highlight shape
   moves and resizes from word to word on an ease (no jump), timed by the voice's word times (recorded: exact; device: boundaries);
   behind it, the words just said keep a fading warm tint over the Trail's length (the comet). Settings in 1 · The mascot › Speech on
   screen (today's Highlight what is read, Highlight style, Trail, Glide move there for good); each place may only choose its size and
   colours from the theme. Reduced motion: no glide, the word lights at once, the tail stays. Unity: a TextMeshPro / UI Toolkit text
   with a highlight quad tweened between the words' bounds (DOTween), the tail as per-character vertex colours fading.
2. **The open panel (21.1).** Smaller and calmer: the spoken words at most about 20 px (today about 30), the panel at most half of 844
   with room, the controls in one row, nothing cut. Back 10 s and forward 10 s beside play (21.3), on the tour's own timeline (they
   cross from step to step; the time shows where they land). The minimal panel gets back 10 s only, keeping its one line. Each target
   ≥ 44 px. Unity: the dock's UI Toolkit panel, the seek on the tour timeline (sum of the steps' clip lengths).
3. **The bubble stays by the mascot (21.4).** The bubble is anchored to the mascot's head and follows it as it moves (its tail always
   on the head within 4 px, the test's arithmetic, including while the mascot walks), flipping side when the screen's edge needs it.
   Unity: the bubble's anchor follows the mascot's head Transform projected to the panel each frame.
4. **The teleprompter (21.5).** During a tour (and wherever the mascot moves around the blocks), the words go to a teleprompter
   instead of the bubble: a strip rising from the screen's bottom edge behind the minimal panel, about 2-3 cm (say the px you chose and
   why), above the card and below the minimal panel; the spoken line in the one highlighter; the line before fading up and out, the next
   line faint below; a soft gradient that melts into the card above (no hard top edge). A small "Full script ›" opens the open panel's
   script view. In the Editor (Speech on screen): Where the words go during a tour: Teleprompter (default) / The bubble / Off; its
   height; the bubble's place outside tours. Reduced motion: the lines change with a fade only. Unity: a UI Toolkit panel with a
   gradient mask image, lines in a vertical layout scrolled by a DOTween.
5. **Lighter panels (21.6).** Three looks for the minimal panel, the open panel and the teleprompter together, as an Editor option in
   1 · The mascot › Look (or Speech on screen; say which): **Light glass** (frosted white glass, soft pastel light shapes drifting very
   slowly behind, a fine bright rim: the developer's first reference), **Aurora** (deep glass with coloured light gradients and a soft
   glowing rim, a slow drift of light: the second reference), and **Dark** (today's, kept). Plus **Follows the theme** (Light glass in
   light, Aurora in dark), the default unless the captures argue otherwise. "A hint of 3D": a soft inner light and depth on the glass,
   never a busy picture; motion very slow (a drift over 20-30 s), off with reduced motion. Text contrast ≥ 4.5:1 on every look (the
   test's arithmetic on the rendered colours). Unity: the glass as a pre-blurred backdrop (a URP blur of the camera into a render
   texture, sampled under the panel) or a frosted texture when blur is off; the light shapes as gradient sprites moved by a slow
   tween; the rim a 9-slice border image.
6. **The mascot over What next? (21.7).** It covers about 10 px of the Up next card: move it so nothing is covered (the arithmetic).
7. **Asking during the quiz (G.1).** An off-topic question during an open quiz ("Who lived here?") now gets hint 1's "Have a look at
   …: the answer is hiding there". It should answer the question asked, and only if the answer would give the quiz away, say warmly that
   it will tell after the quiz ("I'd love to, but that would give our quiz away. Ask me again once you've answered!" / PT). A question
   about the quiz question keeps the hint ladder.
8. **The card's quiz chip (G.2).** It touches its eyebrow "… OF 3": space them (the arithmetic: ≥ 8 px).

## Tests for this round (feature tier: ~5-15 min)

- A new file `tests/test9z.js` (speech on screen), with its mutation, real taps: [highlighter] one module used by the bubble, the
  teleprompter, the panel and the block (the same function, by a hook count), the glide eased (the highlight's position sampled
  between two words lies between them), the tail's fade, reduced motion; [panel] the open panel ≤ 50 % of 844, words ≤ 20 px, every
  target ≥ 44 px, nothing cut; [seek] back 10 s and forward 10 s land where the timeline says, across a step; the minimal panel's back
  10 s; [bubble] the tail on the head within 4 px while the mascot walks; [teleprompter] during a tour the words there and not in the
  bubble, its top below the card's content it must not cover (the arithmetic), the line before fading, Full script opens the script;
  [looks] each look's text contrast ≥ 4.5:1, light and dark, Follows the theme; [what next] the mascot covers nothing; [quiz ask] the
  off-topic answer answers (stubbed), a spoiler-only question gets the warm "after the quiz" line, EN and PT; [chip] ≥ 8 px; [editor]
  each new row changes what the visitor sees and its ▸ demo plays it.
- The self-test of the tab you touch: `tests\runtest8.ps1 -tabs guide` (bad=0).
- test9w (asking during a game) and test9t (the tour's lines).
- **Feature tier only.** Core ran in round 20 (round 21 is the 1st round since core). The Architect is running the **full** tests at the
  same time on a separate copy of the code: the CPU will be busier than usual; check it before each run
  (`powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`; above about 40 %: wait, with `until(...)`).
  If the Architect adds a checklist item from the full run under round 21, it is part of your round. Add test9z to `tests_baseline.json`.
- Capture and look (390 px, light and dark): the open panel; the minimal panel with back 10 s; the teleprompter during a tour (two
  moments: a line starting, a line ending with the tail); the bubble beside the walking mascot; each look (Light glass, Aurora, Dark)
  for the minimal panel, the open panel and the teleprompter; What next? with the mascot; the card's quiz chip; the Editor's Speech on
  screen; in `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g21\` with
  `index.md`. Look as a designer: premium, light, calm, one focal point, nothing covered; fix what looks off before the tests.

## Limits

- Edit only files under lab_src, plus the checklist's items named above. Captures in the scratchpad. Do not commit, publish or switch
  branches. No keys, no live model, no downloads, no Unity. Patch scripts through the Write tool or Python, never a bash heredoc with
  apostrophes; keep each file's line endings. `until(...)`, never a fixed wait.
- Before you finish, make sure none of your test processes is still running (the tavily MCP node processes and the Architect's full run
  are not yours: never stop them).

## Report (your final message)

What the visitor sees now (each part, EN and PT words), what you changed from this design and why, the Editor rows, each part's Unity
path, every test run with its numbers, time and CPU load, the checklist items you marked, and 1-3 questions for the developer, each
opening with one plain sentence on what the feature does (the default look among them, with the captures named).
