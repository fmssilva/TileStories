# Audit D: Media, Play, Visit, AR, Community blocks, viewers, players (saved from the hand-back)

96 rows: SAME 19 · ADD 40 · MERGE 25 · DROP 5 · RENAMED 7.

## Headline
1. bd:<Name> foldouts hold only Entrance / Tap / Open Row / Page chrome / Starts with: no kind options.
2. The Lab draws a real preview for 8 kinds only (Quick Facts, Inline, Story, Before and After, Gallery, Audio Guide, Knowledge Check, Practical Info). Hotspot, Video, 3D, 360, Poll, Collect, Feedback, Place In AR, Wall Locator, Today Map, Related, Dialogue have none; Poll/Collect/Feedback/Place In AR/Dialogue show the same generic callout. Demo card = S.card (v7.js:103), not every kind once. No 8.5 idle demos.
3. No full-screen viewer or player in the Lab (Gallery page = vertical list).
4. Page defaults contradict 8.5: Lab gives pages only to Story, Gallery, Timeline, Dialogue, Compare Points, Video (v4.js:20).

## Every block (bd:)
- ADD Look row per kind (wall default; marker "As the block"; where the look is an element style, picks it). draws.
- MERGE In this wall: On * / Off ("Off: no point of this wall shows it"). [kinds[].enabled]
- ADD Library field defaults on bd: (Fit, Size mode, Keep on switch) as wall defaults; marker shows As the block.
- ADD Emphasis: As the block / Normal / Featured / Quiet (Cards look only). draws.
- RENAMED Display: Takeover = Open its page; Expandable = Expand here; Inline = Nothing. A kind offers only its own taps.
- DROP Details note.

## Gallery (Zoom Image merged)
- MERGE el:carousel Layout: Peek Strip * / One at a time / Grid (2x2 + "+N"; Unity's grid shows all, breaks D4) / Filmstrip / Stack. bd:Gallery Look picks a Carousel style.
- ADD el:carousel Position marks: Dots * / Counter / None.
- MERGE content: one caption AND one credit per picture.
- ADD bd:Gallery Pictures from: Its own * / Every picture on the card (page filters All / Pictures / Video / 360 / 3D).
- Zoom moves to the Picture viewer; pinch on the card dropped; one-picture gallery draws as one picture; zoom_hint on bd:Gallery.

## Before and After
- MERGE Start at 25 / 50 * / 75 % = share of Before shown; marker may override.

## Hotspot Image
- MERGE el:hotspot Spots: Numbered * / Loupes (3x close-ups); Spot motion: Pulse * / Still.
- ADD marker content Across/Down with click-to-place; spot Opens: Its own text * / a block (pin shows an arrow).

## Video + player
- DROP Chapters look (player shows chapter buttons whenever rows exist). Keep Width.
- ADD Video player Captions: On at start * / Off at start (block may override).
- ADD content: chapters (t, label), .vtt per language, Title, clip per language.
- ADD el:video Tap: Play here * / Open the player. behaviour.

## 3D Model
- ADD el:model_3d Fit: Yaw Safe * / Sphere / At Rest / Visible ("Yaw Safe stays whole as it turns; Visible sizes a scan to what is seen"); wall default on bd:.
- MERGE Auto Spin On * (Lab) vs Off (Unity): keep On; Reduce Motion off; 12°/s, resumes after 2 s.
- page: the viewer, zoom, AR (Q2). Add Title. Fixed: zoom 1-3x, pitch ±80°.

## 360 Panorama
- MERGE Look around by: Dragging * / Turning the phone. behaviour.
- ADD content Starts facing (°). Page (Q2). Title. Fixed FOV 70 (40-100), pitch ±85.

## Audio Guide
- RENAMED Hero Chip = Header's Listen pill.
- ADD el:audio Speed button: Narration * (0.75-1.5x) / Wide (0.5-2x) / Off.
- ADD el:audio Captions: On at start * / Off at start.
- content: clip + .vtt per language.
- DROP Android output poll.

## Knowledge Check
- ADD Question type: Multiple choice * / True or false (swipe) / Picture choice. draws.
- ADD Appears: With the card * / After the card is read. behaviour.
- content: options 2-4, pictures, right option, is true, explanation.
- MERGE Score (Q1): Unity no score / no retry; 8.5 says score. If wanted: Score: None * / At the end.
- Swipe threshold: keep a share of the width (20%) vs Lab 120 pt.

## Poll, Collect, Feedback
- Poll: options 2-6 content; bars only with real results, else "Your choice" + thanks.
- Collect: Item name, Series content.
- ADD bd:Feedback Answer with: Thumbs * / Stars (5). draws.

## Visit
- ADD bd:Wall Locator Look: Strip * / Neighbours (fix LIB: a wall strip, not a map).
- ADD bd:Today Map Look: Map * / Then and now. Content: map picture, lat, lng, maps link.
- ADD bd:Related Look: Strip * / Next along the wall; Points from: Picked by hand * / Same category / Nearest (up to 6).
- ADD Practical Info row Icon: Time / Access / Location / Info / Ticket / Light / none.
- Lab-only el:map Paper / Night: no meaning for Unity's map picture.

## AR (Place In AR)
- ADD Distance from the wall (cm) 0-200, 10.
- ADD Size: Real size * / A set height (1-500 cm, 30) / Times the marker (0.5-20, 3).
- ADD When the visitor picks another point: Take it away * / Keep it standing.
- DROP Anchor (one choice).
- ADD Card Controls component Placed model chip: Shown On * / Off; move placed_model_remove / placed_model_other there.
- Draw disabled-until-wall-found and lowers-to-Peek states.
- DROP Show On Wall With Neighbours; retire show_on_wall_nearby. s5loc help: hidden without the camera.
- MERGE show_on_wall_button word: common parent (Card Controls) or s5loc with a note.

## Sound (s1c)
- MERGE When another sound starts: Pause the other * / Fade it out and switch / Lower the other (under a video only) / Queue the new one (audio only); drop Mix. (A proposes Fade and switch * default.)

## Viewers and players (new components under 5 Card Controls)
- Picture viewer: Counter: Chips * / "3 of 12" / None; Aa gallery_count, zoom_hint. Controls: Back + breadcrumb, numbered chips, fitted, swipe (20%, resistance 0.3), pinch 1-4x, pan, caption + credit.
- Video player: Captions On at start *; Aa video_full_screen, video_chapters, media_badge, video_unavailable. Controls: poster + play, play/pause, time, scrubber, captions, chapter buttons, full screen (inline only), Back.
- Audio player rows as el:audio. Mini player SAME.
- 3D / 360 viewers live on their elements; add pages (Q2).
- Block page: ADD sibling chip strip: Page chips: Shown * / Hidden (one icon chip per sibling block of the section; swipe to the next sibling).
- AR placed state: see AR.

## Retired kinds
- Swatches -> el:swatches: Show colour code Off * / On; Layout Row * / Grid; Name / Colour / Note as content (colour is content, exempt from no-free-colour).
- Dialogue: Interaction "Choice Point" names this act.

## Gaps (proposals)
G1 Picture viewer double-tap zoom (2x). G2 Knowledge Check Retry Off * / On. G3 Today Map Directions from coordinates when Maps Link empty. G4 Audio Headphones out: Pause * / Keep playing. G5 Transcript on the page from .vtt. G6 3D page "See it in the room" -> Place In AR.

## Open questions
1 Quiz score. 2 Which kinds get a page and default Tap. 3 Sound default. 4 Gallery looks keep all five? 5 Captions at start On or Off.
