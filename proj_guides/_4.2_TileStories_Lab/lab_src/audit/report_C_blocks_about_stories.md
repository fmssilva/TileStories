# Audit C: About, Stories, Meta, Navigation, Guides blocks + Block Library (saved from the hand-back)

72 rows: SAME 20 · ADD 31 · MERGE 11 · DROP 4 · RENAMED 6.
Story's Preview Automatic / Custom was in v3.js:578; v4.js dropped it; bd:Story never had it.

## A. Block Library
- ADD bd:<Name> Offered on this wall: On * / Off ("Off: no marker of this wall can place it; blocks already placed are not shown"); none on Header.
- ADD bd:<Name> Look (per-kind choices below); marker "As the block (X)".
- ADD field defaults as kind rows (Header: Hero Motion, Words On Picture, Fit; Contents: Rows).
- MERGE Templates: bd:Story Starts with: Story -- magazine * / Story -- short / Empty / the wall's templates; Templates list (open, Duplicate, Delete). Template = copy; pattern = shared.
- Fix: Story is under bs:stories in the Editor but under About in LIB and _3.3 8.4. Sources (Unity meta, always last), Contents (never a nav target) are in Visit (Q3).
- DROP Details note, test guides.

## B. Every block
- ADD mcard Look: As the block (X) * / looks.
- MERGE Display Inline/Expandable into Tap (Expand here = Expandable).
- ADD Frame (Emphasis): Normal * / Featured / Quiet (Sources: Quiet *) on bd: and marker "As the block". Help: "Featured: deeper shadow, larger title, for the one or two blocks to push. Quiet: a hairline edge, for credits." Keeps 3:1 edge.
- ADD mcard Reading time: Automatic * / Set by hand (1-60 min).

## C. Story preview and page
- ADD mcard Story Preview: Automatic * / Custom (help: "Automatic: the page's first picture, title and first text, cut at half the card. Custom: the rows you pick.").
- ADD mpages Story Preview rows table (with Custom): linked rows / own rows; broken link warns.
- MERGE Tap: Open its page * / Only the Open Row / Expand here / Nothing (no page). Warning when Only the Open Row + Open Row with no words.
- ADD mcard Open Row words: As the block * / own.
- ADD mcard Page chrome: As the block * / Override.
- MERGE mpages Story Template line: its name, Re-apply, Save as template.

## E. Header (Lab Editor has no hero option)
- ADD bd:Header Hero look: Compact / Text Only / Image Parallax / Split Then Now / Spotlight Crop / Video Loop / Model Turntable; proposed default Image Parallax (a look without its media falls back to the plate).
- ADD Hero motion: Pan and zoom * / Pan / Zoom / None (fix _3.3 8.3: code default is Pan And Zoom).
- ADD Words on the picture: Tone + Halo * / Edge Scrim / Halo Only.
- ADD Shade reach 0.4-1.5, 1 (hidden with Halo only).
- ADD marker Focus point (tap sets it); Spotlight zoom 1.5x / 2x * / 3x; Model size (Fit) same as el:model_3d.
- Remove Tap row on bd:Header.

## F-H
- ADD bd:Status Look: Ring * / Scale. Fix LIB v3.js:144 ("open, closed, under works" wrong: Status is the condition outline).
- ADD bd:Quick Facts Look: Grid * / Chips / Big numbers; Show category: No * / Category / Category and Level.
- MERGE bd:Actions Look: Pill row * / Circles / Sticky (with Sticky, s5sticky owns where it shows; no Entrance/Tap).
- MERGE Actions button Action: Open a block / Show on the wall (runs the Locate Button's action); keep RepeatedActionRule warning.

## I-L
- MERGE bd:Timeline Preview look: Strip * / Vertical; page always vertical.
- ADD bd:Timeline Now point: Off * / On.
- DROP Compare By (one choice).

## M-O
- ADD bd:Sources Look: List * / With confidence; marker Content status: None * / Verified / Draft.
- ADD bd:Contents Rows: Sections first * / Every block.
- MERGE bd:Audio Guide Look: Player * / Chip; pinned chip -> bd:Header Listen button: Shown * / Hidden.
- ADD Audio Guide Speeds: Narration * / Wide / Off; Captions at start Off * / On.

## P. Retired kinds
- ADD s3p built-in patterns Text (Paragraph) and Text with headings (Inline has nowhere for a lone paragraph; 7 shipped Rich Text blocks need a home).
- Pull Quote: keep two fields Author and Source.
- ADD el:list Markers: Bullets * / Numbers / Steps (circles + a line; each item a title + optional text).
- ADD el:callout named style Postcard (low priority, Q4).
- ADD el:picture With no picture: Hide * / Show the next heading's initial.
- ADD bd:Story Page layout: One scroll * / Chapters (a chapter starts at each Heading in the Chapter style; nothing turns by itself).
- ADD el:swatches Show code + Name, Colour, Note.

## Q. Defaults
- Compare Points: Tap Nothing *, no page (8.5 says none).
- Audio Guide: recommend no page; speed and captions on the player; correct 8.5.
- Sources: Tap Expand here *, Frame Quiet.
- Contents: add recipe to 8.5; keep out of sections (Q3).
- Header: remove Tap. Actions: no Entrance/Tap with Sticky.

## Gaps
1 Audio Guide chapters (time-coded). 2 A picture per Timeline event. 3 Story Resume (Off / On *). 4 Per-kind Reading time display Shown * / Hidden.

## Open questions
1 Tap Nothing meaning -> four-choice list. 2 Hero Chip drop? 3 Contents/Sources pinned? 4 Postcard? 5 Header default look Image Parallax?
