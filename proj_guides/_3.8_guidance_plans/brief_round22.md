You are building round 22 in a browser prototype, the "Block Components Lab", in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app (say each
change's Unity path). Work only in the Lab. This round is **"The tour as a demo of the app"**: during a tour the app moves exactly as if
a visitor were using it by hand (a natural scroll, a ghost hand that taps, the app's own page openings, interactive blocks shown in
use), the dimming goes, and a slim bar marks the block being told. It is the last round of the guide's polish pass.

THE CHECKLIST: `C:\Users\franc\Desktop\TileStories\proj_guides\___todo.md`, section "THE CHECKLIST", items **22.1 to 22.6** (and any
item the Architect adds under round 22 while you work). The developer's notes are above it; the paragraphs on "the focus of the block
that the AI Guide is explaining" and "the focus we currently give" are this round's. Mark an item `[DONE]` (replace `[ ]`) only after
it is built, looked at in your captures and tested; an item you change or cannot finish gets one line under it saying what and why.
Keep the file's CRLF line endings; edit only those items.

READ FIRST: `PROTOTYPE_STRUCTURE.md` (the rules; the rows of `guide/focus.js`, `guide/autotour.js`, `guide/tour.js`,
`guide/highlight.js`, `guide/teleprompter.js`, `card/blocks.js`, `editor/accordion.js`), `README.md` ("Prove it", "Levels": a
**feature** round), `src/guide/focus.js` (today's spotlight, dimming and the mascot pointing), `src/guide/autotour.js` (how a tour
moves to its block today: the scroll, entering a story or page), the card's own page openings (how a visitor's tap on a story preview
opens its page today) and the interactive blocks (before/after, the 3D model, the flip card, the gallery).

## What the developer asked (2026-11-13, their words, shortened)

- "The focus on the block the guide is explaining is too mechanical: we move to the block instantly, so the visitor has no sense that we
  are scrolling down, or entering the story from the preview to the detail page."
- "I want the guide to replicate all the movements and motion the visitor would need to do by hand to take that tour. The scroll
  should be slower and more natural, with some scrolling effect to show we are moving down or up the page (a grey see-through element
  pulsing while we scroll?)."
- "When the guide does something that is entering a card or clicking something, show it on the screen like a 'how to use the app'
  demo: a little see-through hand moving like a comet from some 2 cm away towards the place to be clicked, then a click effect (a grey
  see-through circle), and then the page opening with the normal effects and animations (a zoom from the whole screen to the block's
  preview position, or others). The app's movement during the guide should be exactly as if a normal visitor were using it, so every
  tour is a demo of how to use the app. The same with the interactive elements: dragging the before/after image, the 3D model…"
- "Remove the focus that highlights the block and darkens the rest of the app: it damages the sense of navigation. Have only a little
  bar that appears smoothly on the left side, pulsing a bit, to show we are talking about that block. Nothing else gets darker:
  everything stays the same."
- 2026-11-14: merge the Detail Card tab's two near-identical switches (22.6).
- Standing: the mascot's words warm; every Editor row has its ▸ demo and ? help; messages name things, not ids; the Editor says "the
  mascot"; every good option kept as an option (the developer picks the default); Lab features must be Unity-buildable (name each
  effect's Unity path); design checked by arithmetic in the test AND by looking at captures; never the live model in tests.
  Kept from round 21 (the developer, 2026-11-14): the panels' look Follows the theme; the teleprompter and the block both lit while a
  block is read; the quiz chip as the pause sign alone.

## The design to build (the Architect's; adjust where the captures show something better, and say what and why)

1. **One "visitor's hand" module (22.2).** A small module (e.g. `guide/demo-hand.js`) the tour uses for every action a visitor would do
   by hand: `tap(el)`, `drag(el, from, to)`, `turn(el, deg)`, `swipe(el, dir)`. The hand: a small see-through hand (an SVG, about
   28-32 px, white with a soft shadow, about 70 % opacity) that glides in on an eased curve from about 2 cm away (≈ 75 px at 390 px
   wide; say the px and why) with a short comet trail, presses (a slight scale-down), and the tap leaves a soft grey ripple (a ring
   growing to about 44 px and fading in ~400 ms); then the real action happens (the same handler a visitor's tap fires: never a
   shortcut that skips the app's own animation). The hand never covers the words being read, and goes away when the visitor touches the
   screen (the visitor's hand wins). Reduced motion: no glide or trail, the ripple only, and the action. Unity: a UI Toolkit image
   tweened along a curve (DOTween), the ripple a scaled ring image, the action invoked through the same UI event the visitor fires.
2. **The natural scroll (22.1).** The tour scrolls like a visitor's flick: eased (in-out, about 500-1200 ms by distance, say the
   curve), never a jump. While it scrolls, a quiet cue at the screen's edge shows the direction (e.g. a soft grey see-through pill or
   chevron that pulses near the bottom when moving down, near the top when moving up), gone when the scroll ends. Long distances may
   start with the hand's swipe (the hand's `swipe`). Reduced motion: a short fade-scroll, no cue motion. Unity: ScrollView.scrollOffset
   tweened (DOTween), the cue a small image faded in and out.
3. **The app's own openings (22.3).** When the tour enters a story, a page or a gallery item, the hand taps its preview and the app's
   own opening plays, the same one a visitor's tap plays (the page zooming from the preview's rectangle to the full screen, or
   whatever the card does today; if the card has no such opening today, build it once for the visitor and the tour alike, and say so).
   Going back plays the closing the same way. Unity: the same page transition as the app (a RectTransform tween from the preview's
   bounds), invoked by the same tap.
4. **Interactive blocks in use (22.4).** When a step tells an interactive block, the hand shows it in use, briefly and gently: drags
   the before/after slider across and back; turns the 3D model a little; flips the flip card and back; swipes the gallery once. Each
   uses the block's own handlers. Only once per block per tour; never while the visitor is touching it. Say each block you covered and
   any you left. Unity: the same input events on each block (the drag on the slider, the rotate on the model's orbit), driven by a
   tween.
5. **No dimming; a left bar (22.5).** The spotlight's dimming of the rest of the app goes: nothing else gets darker. The block being
   told gets a slim bar on its left edge (about 3-4 px wide, the block's height, the theme's accent) that fades in, pulses very softly
   while it is told, and fades out when the tour moves on; it never moves the block's layout. Keep today's dimming as an Editor option
   (the developer picks the default; the default is the bar, per the developer's words) in 2 · The tour › Focus. Contrast of the bar
   against the card ≥ 3:1 (the test's arithmetic). Reduced motion: no pulse. Unity: an Image anchored to the block's left edge, its
   alpha tweened.
6. **The switches (22.6).** Remove the Detail Card tab's old "Foldouts: Many open / One at a time" row; the accordion's "One section
   open at a time" at the top of every tab does it. Keep "Open to level" if it still does something the accordion does not (say).
7. **Editor rows** in 2 · The tour (where they belong; say where): The visitor's hand (Shown / Off), Show blocks in use (On / Off), The
   scroll (Natural / Quick), Focus (The left bar / Dim the rest / Off). Each with its ▸ demo and ? help.

## Tests for this round (feature tier: ~5-15 min of your own runs)

- A new file `tests/test10a.js` (the tour as a demo), with its mutation, real taps: [hand] during a tour, before each tap the hand
  starts about 2 cm away and ends on the target (the arithmetic: its tip within 6 px of the target's centre), the ripple appears there,
  then the same action a visitor's tap fires (an event count); a visitor's touch removes the hand; [scroll] sampled positions are
  monotonic and eased (no jump larger than a set share of the distance in one frame), the direction cue shown while scrolling and gone
  after; [opening] entering a story plays the same opening as a visitor's tap (the page's rectangle starts at the preview's and grows);
  [blocks] each covered interactive block changes and returns (the slider's value, the model's angle, the flip); [focus] no element
  outside the block gets darker (the rendered colours before and during), the bar on the block's left (its x within 2 px of the
  block's left edge, its height the block's), its contrast ≥ 3:1, the Dim option still dims; [reduced motion] no glide, no pulse, the
  ripple and the actions still happen; [editor] each new row changes what the visitor sees and its ▸ demo plays it; [switches] the old
  Foldouts row is gone, One section open at a time still works on the Detail Card tab.
- The self-test of the tabs you touch: `tests\runtest8.ps1 -tabs guide -minutes 10` and `-tabs card -minutes 10` (bad=0).
- The two shared files: `test8j` (focus and the spotlight) and `test9ja` (the automatic tour). A red from the removed dimming: the
  test follows the new design (the dimming is now an option); a real regression: fix the code; say which.
- **Every run has a 10-min limit** (`Start-Process` + `WaitForExit(600000)`, then `taskkill /T /F`; a walk `-minutes 10`); a run that
  hits it is a finding to report, never a reason to wait. Check the CPU before each run
  (`powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`; above about 40 %: wait with an until-loop).
- Tier: **feature**. Rounds since core: 2 after this one (21, 22); core runs after round 24. Add test10a to `tests_baseline.json`.
- Capture and look (390 px, light and dark), in
  `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g22\` with `index.md`
  (write a line per finished item as you go, so progress is visible): the hand mid-glide and at the tap with its ripple; the scroll with
  its cue (down and up); a story opening from its preview (three frames); each interactive block in use (one frame each); the block
  with its left bar and nothing dimmed; the Dim option; the Editor's new rows; the Detail Card tab without the old Foldouts row. Look
  as a designer: calm, natural, like a person showing a friend the app; nothing covered; the hand never in the way of the words.

## Limits

- Edit only files under lab_src, plus the checklist's 22.x items. Captures in the scratchpad. Do not commit, publish or switch
  branches. No keys, no live model, no downloads, no Unity. Patch scripts through the Write tool or Python, never a bash heredoc with
  apostrophes; keep each file's line endings. `until(...)`, never a fixed wait.
- Before you finish, make sure none of your test processes is still running (the tavily MCP node processes are not yours).

## Report (your final message)

What the visitor sees now (each part, EN and PT words where there are words), what you changed from this design and why, the Editor
rows, each part's Unity path, every test run with its numbers, time and CPU load, the checklist items you marked, and 1-3 questions for
the developer, each opening with one plain sentence on what the feature does (with the captures named).
