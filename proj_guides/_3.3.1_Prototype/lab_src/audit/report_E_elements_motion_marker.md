# Audit E: elements, motion, marker (saved from the hand-back)

148 rows: SAME 63 · ADD 48 · MERGE 13 · DROP 11 · RENAMED 14.

## Mapping rule
- Style options: Tone, Size, Alignment, Drop Cap, Markers, Width, Frame, Ornament, Spacer size, Tap (picture/video), Motion (Entrance + With the scroll / By itself preset).
- Per row: words per language, callout title, quote attribution, list items, media files, each item's caption + credit, clip + poster, place, Validated, Custom preview links; (gaps) focus point, alt text.
- Go: per-row Strength (-> preset), "Auto" (-> ★ style), Look/Motion/Tap accordions (-> style picker).

## Elements
- ADD s3e note: an app may register its own element kinds.
- Reuse Unity's per-kind help sentences in el:* (?).
- MERGE Tone everywhere: Neutral / Family colour / Accent / Inverse ("Inverse: light words on a dark plate. Every tone keeps 7:1.").
- Title: Display style should be Size L (Unity new row L). Alignment key: Unity `center`, Lab `centre`: pick one.
- Eyebrow: Tone + Inverse; DROP Size; (?) "Written in sentence case and shown in capitals by the language's rules."
- Heading: ADD Alignment Start * / Centre.
- Lede: DROP Tone; size L (Unity) vs M (Lab).
- Paragraph: DROP Tone, Size, Alignment.
- Callout: Tone default Family colour (keep; Accent default would add a second accent, D5).
- Pull Quote: Attribution per language (MERGE); ADD Tone (Family colour *) "Colours the quotation mark and the bar; the words stay ink." (C: keep two fields Author and Source.)
- List: DROP Tone, Size.
- Picture: Interaction: Open the viewer * / Hold to Reveal / Tilt to Look / None. (?) add "The frame follows the picture's shape, from 4:5 to 2:1." Credit on a quieter line under the caption.
- Picture Pair: each picture Caption EN/PT + Credit; ADD Frame Rectangle * / Arch; Interaction: Open the viewer * / None.
- Carousel: 2..12 items each Caption EN/PT + Credit + ^ v delete, "+ Add picture" up to 12, warning below 2; ADD Width Full Bleed * / Inset; ADD Frame.
- Video: ADD Clip, Credit; ADD Frame; ADD On tap: Play here * / Open the player.
- Spacer: Unity 16/32/48 vs Lab 16/24/40: Unity's follows theme spacing steps.

## Motion
- MERGE per-row Motion into style; Unity Reveal = Lab Rise. ADD s4m type "By itself": Slow Zoom (1->1.06 and back over 28 s), Pan (26 s). Rename Lab's scroll Slow Zoom -> "Zoom with the scroll".
- DROP per-row Strength (presets carry it; copy a preset for a stronger look).
- Unity Auto picture = Parallax; Lab ★ Inset has none.
- ADD fixed-rule notes in s4m: the stagger stops growing after 5 rows; only the picture nearest the middle moves by itself.
- Unity numbers: enter line 92 %, rise 16, slide 24, 0.36 s, 60 ms stagger.
- ADD Validated "Checked" column in mpages + "N to check" on mcard block line.
- MERGE refs: only links to rows of this page inside a Custom preview; drop Another Block / Shared.

## Story preview / page
- ADD Preview Automatic * / Custom (bd:Story and mcard As the block); Automatic rule in (?): first picture, an eyebrow right above the title, the title, the first text; texts Small; pictures without captions; cap half the card, cut with a fade, title never cut, picture shrinks first down to 120.
- ADD "Shows: Picture #1, Eyebrow #2…" line; "P" marks; Custom rows: + Link a page row, + Add own row, Unlink (make a copy), Remove; broken link warning row; "Use in the preview (linked)" / "Copy to the preview" on an open page row.
- MERGE Tap: Open its page * / Only the Open Row / Expand here / Nothing (no page).
- ADD Open Row words per block (empty = the block's "Read the story").
- Page chrome: keep per kind; DROP per-point override (C proposed adding it; E says drop).
- ADD Reading time: Auto (200 wpm; picture 10 s, carousel picture 6 s, video 30 s) * / Set by hand (1-120 min).
- ADD "Save these rows as a pattern"; Re-apply not needed (patterns stay linked). (C: keep templates as copies in Starts with.)

## Page editor tools
- ADD drag handle, Duplicate, "(required, empty)", "Not shown on the card yet: …" warnings naming row and field; new row opens at once. DROP the page's Motion table.

## Card Content (mcard)
- ADD Card Starter first row: Simple point / Rich point / AR point / Start from scratch; reads "Custom" once edited; confirm "Replace N blocks on <point>?".
- ADD Look per block; Frame (Cards only); "N to check"; Not shown reasons; glossary terms missing (point to Languages & Terms); action offered twice; header needs its picture / title 3+ lines; each kind's warnings; note when Detail Card is off.
- Pickers show the item's name, not its key.

## Hero
- ADD bd:Header Hero motion; draw fixed hero motion rules on the phone (>= 26 s per drift, <= 12 pt/s, zoom 1.06, parallax 0.45; words rise 0.25 faster, fade only on scroll).
- ADD Words on picture. Tone Spread = s4f Tone Spread (RENAMED) (C proposes a Shade reach row on bd:Header).
- ADD Header page in mpages (Title, Eyebrow, Subtitle, Picture, Second Picture, Loop Clip, Model, Model fit, Focus point, Crop zoom).

## Look Kit / light / knobs
- MERGE Import an edge: SVG or tileable PNG strip.
- ADD s2start Motion (A: Reveal * / Still, drop Drift).
- ADD s2cfg Glass light On * / Off and Rim light On * / Off (Cards only); s4f Light: Living Glass, Rim Light. Rim light = the card's one glow (D5): Open Row Shine should warn when on.
- MERGE Motion level: Full * / Reduced / Off (spec knob) — conflicts with B (visitor Motion) and A (Wall motion Full/Reduced).
- ADD s1b Accent: Heritage * / Azulejo / Terracotta / Garden ("Every choice keeps 7:1 in light and dark").
- Lab naming clash: "Family colour" (elements) vs "Section colour" (tint, bar): use one name.

## Gaps
1 Alt text per picture and poster per language. 2 Focus point per picture. 3 Picture Proportion: As the picture * / 3:2 / 4:3 / 1:1. 4 Visitor Reduce Motion + text size in Settings. 5 Roundness knob Sharp / Soft * / Round.

## Open questions
1 Tap as style option? 2 Strength into presets? 3 Full Custom preview? 4 Validated keep? 5 Motion level Off?
