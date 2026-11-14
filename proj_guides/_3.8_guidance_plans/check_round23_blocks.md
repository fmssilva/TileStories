# The Detail Card's blocks: design check (2026-11-14, opens round 23)

Captures (390 x 844; l/d = light/dark, en/pt, NN = the block's place on the demo card, i = in use):
`C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\b23check\shots\`
(142 PNGs; contrast and tap sizes in `..\log_metrics_*.txt`, radii and type sizes in `..\log_measure_0en.txt`).
No code was changed. The Architect looked at the audio guide, the 3D model after a drag and the wall locator and confirms them.

## Inventory (27 kinds on the demo card)

Shared rows on every block but the Header: Offered on this wall; Tap (Open its page / Only the Open Row / Expand here / Nothing);
Open Row look; Frame; Entrance; for blocks with a page: Page chrome, Breadcrumb, Previous / Next.

Header, Quick Facts, Status, Inline (Fun Fact flip), Actions, Story, Timeline, Compare Points, Dialogue, Gallery, Before and After,
Hotspot Image, Video, 3D Model, 360 Panorama, Audio Guide, Knowledge Check, Poll, Collect, Feedback, Place In AR, Wall Locator,
Today Map, Related, Practical Info, Sources, Contents; plus Up next between sections.

## High

- H1. 3D Model and 360 Panorama: a drag on the card opens the block's page when the finger lifts (Tap defaults to Open its page on
  the whole preview). Options: Tap "Only the Open Row" by default for gesture blocks; count a tap only under ~10 px of movement;
  a still preview on the card and the gesture on the page. (ilen_14_model_turning, ilen_14_model_released, ilen_15_pano_*)
- H2. Hotspot Image: a pin tap opens the page instead of its note; pins 28 px; in dark the number is #8FB0EC on white (2.19:1).
  Options: fixed dark ink on the white pin, or a section-colour pin with paper ink (4.5:1 both modes); 44 px hit area; the H1 fix.
- H3. Audio Guide: the row does not wrap: progress bar at zero width, captions squeezed into a 4-line column, "1x" a second blue
  circle like Play, chapter chips 28 px touching the edge. Options: two rows (Play, title and time, speed as an outline pill; then a
  full-width bar), captions below; chapters as 44 px list rows or 44 px chips on an inset scroll. (len/den/lpt_16, ilen_16_audio_playing)
- H4. Knowledge Check: right and wrong by colour alone, no explanation, no way to question 2, rows 41 px. Options: a check or cross
  icon plus one line ("Not quite: it was Manuel I"); a Next question; 44 px rows. (ilen_17_quiz_wrong / _right)
- H5. Wall Locator: "You are here" sits at 46 % under no dot (the lit dot is at 58 %); no dot is named. Options: "This place" on the
  lit dot and a person icon "You", each label a child of its mark; Neighbours as the default look. (len/den_22_wall_locator)
- H6. Status and Compare Points: a full red ring means "Lost, 100 % damaged" (reads as complete); 100 % said twice; Compare's rings
  never say what they measure. Options: the ring counts what survives; the 5-step scale as default and in Compare; a caption under
  each ring. (len_03_status, len_07_timeline)

## Medium

- M1. Swipe strips (Timeline, Gallery, Related) snap their first item to x = 0, not the 20 px margin; the Gallery's dots do not follow
  the swipe. Options: snap offset by the inset; no snap; dots from the scroll position.
- M2. Taps that leave no trace: Poll vote, Collect, Feedback, Practical Info's See more (stays "See more"). Options: a chosen state
  (tint + check); poll results; Collect "Added" with the tile stamping in; "Thank you"; "See less" with the chevron turned.
- M3. Editor names shown to visitors and untranslated: Up next "Media · Before and After" ("Multimédia · Gallery" in PT), page titles
  and chips "3D Model", "Hotspot Image"; place names in English in PT. Options: a visitor name per kind in the string table; the
  block's own title; Up next with the section only.
- M4. Fun Fact flip: no cue that it turns; the back looks like the front; its label does not fit its words; empty space. Unity: the
  CSS 3D flip has no UI Toolkit twin: scale X 1 to 0, swap, 0 to 1, or a crossfade. Options: a turn icon and "Tap to turn"; the back
  on another plate.
- M5. 3D Model plate #111 in light (a black slab), lost in dark; the drag shows no change and selects the hint's text. Options: the
  box token with a hairline; a rotate badge and the model turning under the finger (RenderTexture, yaw from the drag).
- M6. No shared type scale, button shape or radius: 24 size/weight pairs from 11 to 21 px; questions serif 17 in the quiz, sans 16 in
  Poll and Feedback; choice buttons with 12, 10 px and pill radii. Options: ~6 type steps as USS variables; one choice class; radii
  6 / 12 / pill.
- M7. Small uppercase labels on some blocks only; Practical Info floats with no heading. Options: a label row on every block (Auto /
  Own words / None); at least "Visiting" on Practical Info.
- M8. The Open Row: "Go to this part ›" under five media blocks in a row; 19 px tall; the arrow moves under Reduce Motion; the arrow
  touches the word. Options: words per kind ("See all 4 pictures"); 44 px; the arrow its own element, still under Reduce Motion.
- M9. Story preview: "Continue reading ›" and "Read the story · 4 min ›" stacked; PT title breaks "beira-/rio". Options: one row
  "Continue reading · 4 min left"; the pill only; a non-breaking hyphen.
- M10. Dark mode pictures at full brightness. Option: dim to ~90 % (tint or an 8 % overlay).
- M11. The demo card has no top band: close and settings sit over text (no plate in dark). Option: the main card's band.
- M12. Place In AR's glowing button is louder than Actions' main one. Options: the same main button; keep it as the card's one accent.
- M13. Feedback uses colour emoji. Option: line icons (VectorImage).

## Low

Actions' "Share" wraps alone; Practical Info's values do not line up (75 vs 71 px); the "See more" chevron sits low; Today Map
shows raw coordinates; Video and 360 Panorama look the same at rest; the body font's slashed zero in dates; PT Quick Facts wraps
"1 de novembro de 1755"; block spacing comes from each block's own margins (uneven rhythm); an empty 22 px item under the demo Header.

## Keep

Before and After (the model for gesture blocks); section labels (>= 6.2:1); the azulejo strip at each section; the dark palette (not
pure black); the story preview; Quick Facts' serif values; Dialogue's bubbles; Contents' "n of m read"; Sources' quiet list; Up next
as an idea.

## Patterns

Gestures fight tap-to-open (H1, H2); every swipe strip has the edge problem; answers leave no state; the same thing done several
ways (buttons, question fonts, labels, two open calls); Editor names leak to visitors; section colour as text on a fixed white plate
breaks in dark; tap targets under 44 px (Open Row and See more 19, pins 28, chapter chips 28, Up next's Next 35, Collect 35,
Continue reading 33, quiz rows 41, audio buttons 42); Lab effects needing a named Unity path (flip, the preview's fade mask,
colour-mix tints).
