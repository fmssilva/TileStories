You are building round 17c in a browser prototype, the "Block Components Lab", in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app (say each
change's Unity path). Work only in the Lab. This round designs and builds **the tour's end**: where the tour leaves the visitor and a
small "What next?" panel that keeps them exploring.

READ FIRST: `PROTOTYPE_STRUCTURE.md` (the rules; the rows of `guide/tour.js`, `guide/dock.js`, `guide/person.js`, `guide/autotour.js`,
`guide/saved-place.js`, `world/markers.js`, `world/wall.js`), `README.md` ("Prove it", "Levels": this round is a **feature** round),
`src/guide/tour.js` (`act:"end"`: today the tour ends on the hero, Azu says the end line from Azu's intro lines › The end, and Ask opens),
`src/guide/ask.js`, `src/guide/controls.js`, `src/guide/saved-place.js`.

## What the developer asked (2026-11-12, their words, shortened)

"Think better what the options should really be at the end of the tour. Maybe the tour should end by scrolling back up to the hero or to
some nice block (a video, pictures, a 3D model, a game…) to leave the visitor in a nice, entertaining place to go on exploring by
themselves. Then maybe show a small row with images and names of related points (same category, or nearby in the scene) so they can
'explore related attractions'; some good follow-up questions to ask the guide; a text field and a voice button to ask right away. A well
organised little block, about 1/3 of the screen, with a close button that makes it minimal. Very good design: premium, minimalist, wow.
The mascot there too, with a bubble asking 'So, what do you want to do now?'. And how to organise these options in the Editor tab."

Standing rules that apply: Azu's words are a warm storyteller, never a robot ("Now + the block's name"); every Editor row has its ▸ demo;
messages name things, not ids; Lab features must be Unity-buildable (no CSS-only effect without its Unity path); design work is checked by
arithmetic in the test AND by looking at captures.

## The research (the Architect's, for the design)

- A clear ending matters: in a gallery audio-app case study visitors did not understand how a tour ended until it had its own end.
- Research museum guides end a visit with a short summary and new suggestions (UCL, Grant Museum prototype).
- Context-aware suggested questions (from the page the visitor is on) are the strongest pattern in LLM heritage guides (2025-2026 papers).
- A non-modal bottom panel with snap heights (peek / open) that collapses instead of vanishing, like Apple Maps; a carousel of 3-5 items
  at most, most relevant first (NN/g: people rarely go past 3-4).

## The design to build (the Architect's; adjust details where the captures show something better, and say what and why)

1. **Where the tour leaves the visitor.** After the end line the card scrolls (the tour's own smooth scroll) to **the card's showcase**:
   the first block on the card that invites play or looking, in this order: a game or the quiz the visitor has not played, a 3D model, a
   video, a gallery, a before and after; none: the hero. The block sits fully in the part of the screen above the panel (the test's own
   arithmetic). Editor: "Where the tour ends": The showcase (default) / The hero / A block I pick (a dropdown naming the card's blocks).
   Use the kinds that exist in the Lab; list the order in one place.
2. **Azu asks.** Azu stands beside the panel's top edge with a bubble: "So, what would you like to do now?" from a new set of warm lines
   (Azu's intro lines › After the tour: 4 lines EN and PT, e.g. "So, where shall we go next?" / "Então, para onde vamos agora?"; picked
   with no AI by the point, like the other sets). Spoken, then the bubble stays a few seconds and fades.
3. **The "What next?" panel**, non-modal (the card behind still scrolls and plays), above the bottom bar, at most 34 % of the phone's
   height at 390×844, rounded top, the card's surface colours, light and dark. A small header: a check, "Tour complete · 2 min" (the
   tour's length) and, if the quiz was played, its star and score. Then, in this order (each section on/off and ordered in the Editor):
   - **Ask right away:** one row, a text field ("Ask about the Ribeira Palace…") and a mic button (44 px), using Ask Azu's own path.
     Above it **2-3 suggested questions** as chips (44 px tall, one line each, horizontally scrolling if needed): written per point in
     the Editor; with none written, made by template from the card's own words with no AI (e.g. a callout's heading "What was inside" →
     "What was inside the palace?", a timeline's date → "What happened in 1755?"); never one the tour just answered word for word. A tap
     asks it at once (the answer in the dock as Ask Azu does). Never the live model in tests: stub it as the other Ask tests do.
   - **Explore nearby:** a horizontal row of 3-5 cards (a thumbnail of the point's hero crop, its name, and why: "Part of this palace",
     "On the same square", "Nearby · short tour"), most related first: the Editor's own picks, then the same category, then the nearest on
     the wall (the wall's own coordinates). A tap closes the panel to minimal, flies the wall to that point and opens its card with its
     invitation ("A short tour of the Torreão?").
   - **Small actions** in one quiet row: Start again · Play the quiz (only if not played) · Done.
4. **Minimal and closed.** The panel's chevron or a swipe down makes it **minimal**: a 56 px pill "What next? ▴" with Azu's small face;
   a tap opens it again. Done (or × on the pill) closes it; the ✦ menu then offers "What next?" until the visitor leaves the point.
   Starting another tour, a game or Ask Azu's full view closes it. Entrance: the panel rises (240 ms, ease-out), the cards stagger in
   (40 ms each); reduced motion: no movement, a fade only. Unity: a UI Toolkit panel with two snap heights, a horizontal ScrollView,
   a DOTween or USS transition; say each.
5. **Written tours too:** a written tour's `act:"end"` step gets the same ending (its own end line, then the panel), so every tour ends
   the same way.
6. **The Editor:** Guide › 2 · The tour › a new foldout **"The tour's end"** (after Ways in): Where the tour ends; the panel On/Off;
   Start as Open / Minimal; its sections (Ask right away, Suggested questions, Explore nearby, Small actions) each with On/Off and ▲▼
   order; Suggested questions: how many (2-3), this point's own questions (add, edit, remove; EN and PT), the templates' words; Explore
   nearby: how many (3-5), this point's picks (a dropdown naming the points), the order rule; Azu's "After the tour" lines in Azu's intro
   lines. Each row with its ▸ demo (plays the tour's last step and opens the panel as configured) and its ? help. Words through
   `core/words.js` (EN and PT), shown on their UI element's foldout as the other texts are.

## Tests for this round (feature tier: ~5-15 min)

- A new file `tests/test9v.js` (the tour's end), with its mutation, real taps where the visitor would tap:
  [showcase] the tour ends with the showcase fully above the panel, each case (the quiz unplayed → the quiz; played → the gallery; a
  block picked; the hero) by the test's own arithmetic; [asks] Azu's bubble from the After the tour set, warm, no repeat; [panel] the
  panel ≤ 34 % of 844, above the bottom bar, no horizontal page scroll, every target ≥ 44 px, light and dark; [header] the length and the
  quiz star; [questions] 2-3 chips, from the point's own or the templates, none word for word what the tour said, a tap asks (stubbed);
  [ask] the field and the mic reach Ask Azu; [nearby] 3-5 cards, ordered picks → category → distance (the test's own distances), a tap
  flies to the point and opens its card and invitation; [minimal] the chevron and a swipe make the pill, a tap reopens, Done closes, the
  ✦ menu's What next? brings it back; [written] the written tour ends the same way; [editor] each new row changes what the visitor sees
  and its ▸ demo plays it; [reduced motion].
- The self-test of the tab you touch: `tests\runtest8.ps1 -tabs guide` (bad=0).
- test9u (the saved place and the end must not clash: the tour's end still clears the saved place) and test9t (the end line).
- Not core this round (core ran in 17b). Check the CPU before each run
  (`powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).LoadPercentage"`; above about 40 %: wait). Add test9v to
  `tests_baseline.json`. No other files.
- Capture and look (390 px, light and dark): the tour's end with the showcase and the panel open; Azu's bubble; the panel minimal; a
  nearby card tapped (the new point's invitation); the Editor's new foldout; in
  `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g17c\` with `index.md`.
  Look at them as a designer: premium, minimal, calm; one focal point; nothing covering the showcase; fix what looks off before the tests.

## Limits

- Edit only files under lab_src (captures in the scratchpad). Do not commit, do not publish, do not switch branches. No keys, no live
  model, no downloads, no Unity. Patch scripts through the Write tool; keep each file's line endings. `until(...)`, never a fixed wait.
- Before you finish, make sure none of your test processes is still running (the tavily MCP node processes are not yours).

## Report (your final message)

What the visitor sees at the end (each part, EN and PT words), what you changed from this design and why, the Editor foldout, each part's
Unity path, every test run with its numbers, time and CPU load, and 1-3 questions for the developer, each opening with one plain sentence
on what the feature does.
