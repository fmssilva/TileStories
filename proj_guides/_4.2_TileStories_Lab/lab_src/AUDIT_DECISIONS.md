# Lab v8: decisions from the Unity audit (2026-11-01)

Rule: every good option with every good choice; when Unity and the Lab differ, merge the good choices; leave out the bad ones,
with the reason. NEW = neither had it (a proposal from the audit's gaps), shown with the badge PROPOSED so the developer judges it.
Reports: report_A..E.md.

## 1 · Card
- s1a Sheet
  - Opens at: Peek / Half * (drop Full: the card never covers the wall by itself, D8).
  - Half height max: 0.30..0.60, 0.40 * (the measured Half 337.6 / 844; Unity clamps at 0.40).
  - Full height (ADD): To the top * / Leave a camera strip (48 pt).
  - Tap outside closes: help from Unity (a tap on a marker still selects it; a drag never closes; the X and a swipe down always close).
  - Phone's Back button (ADD): Steps the card down * / Left to the app (Android). behaviour.
  - Fix: Peek draws as Unity's 57 pt (the bar / compact header only).
- s1peek Next card peek: DROP (a swipe up at Peek raises the card; the next point is one tap away on the wall and in Related).
- s1b Look
  - Default mode: Light * / Dark / Follow the phone.
  - Accent (ADD): Heritage * / Azulejo / Terracotta / Garden ("every choice keeps 7:1 in light and dark").
  - Roundness (NEW, spec knob): Sharp / Soft * / Round.
  - Layout: One scroll * / Tabs / Pages / Accordion; Open one at a time (ADD, under Accordion): Off * / On.
  - Top band: Fade * / Glass (drop Clear: the title would sit on scrolling text, D1 / D8).
  - Swipe strips (from Unity's Peek Next Card): Show the next item's edge * / Whole items only. Carousel, Timeline strip, Related strip.
- s1c Sound: When another sound starts: Fade out and switch * (0.5 s; a paused audio guide resumes from its play button) /
  Lower the other (it goes on quietly, back to full after) / Queue the new one (audio after audio; a video still fades the audio out).
  Drop Mix (two voices at full volume are not understood). When earbuds disconnect (NEW): Pause * / Keep playing on the speaker.
- s1mini Mini player: Shown On *; With the screen locked (NEW): Pause * / Keep playing; Resume where the visitor left (NEW): On * / Off.
- s1m Media: Own stock media is built in Unity (guide 6.6 fix).

## 2 · Sections
- s2cfg Style: Open * / Cards; with Cards: Glass light On * / Off, Rim light On * / Off (ADD). Block order: By section * / As placed /
  As placed, Sources last. Notice on start / divider / tint while Style = Cards.
- s2start Style: Tiles * / Wave / Arches / Skyline / Fade (the tint only) / Plain (nothing), plus any Edge of the Effects pool and the
  own strip picture. Drop Bands (tints the whole section). Motion (ADD): Reveal * / Still (drop Drift: endless motion beside text, D6).
- s2div two rows: Between sections: None * / Hairline / Tile Ornament / Tile Row / Mosaic Strip / Own strip picture (only before a Fade
  or Plain start). Between blocks: None * / Hairline / Tile Ornament (+ every line divider of the pool).
- s2tint: Colour Section colour * / Accent / Neutral (Neutral = a warm grey tint); Intensity 0.04..0.20 = 0.12; Depth 40..340 = 120.
- s2next Next Up (ADD, from Unity's Up Next): Shown On * / Off; in the flow at a section's end: next heading, its place ("5 of 13",
  left out under Pages), Next; at the card's end "You have reached the end", no button.
- sec:* Shown: On the card and the bar * / On the card only / Hidden. Icon: Unity's 25 + star + ar. Start list = s2start's (Skyline was missing).
  Reset to the framework's sections (button). Notice when a card shows more sections than Items before More.

## 3 · Blocks & Components
### Every block (bd:)
- Offered on this wall: On * / Off (none on Header).
- Look: the kind's looks (below); the marker says As the block (X).
- Frame: Normal * / Featured / Quiet (Cards look only; Sources Quiet *).
- Tap: Open its page * / Only the Open Row / Expand here / Nothing (no page). Only the kind's own taps are offered; Header has no Tap.
- Pages: Story, Gallery, Timeline, Dialogue, Video, 3D Model, 360 Panorama, Knowledge Check, Hotspot Image, Today Map. No page:
  Compare Points, Audio Guide (the player holds speed, captions, chapters), Practical Info + Sources (Expand here *).
### Per kind
- Header: Hero look: Compact / Text only / Image parallax * / Split then and now / Spotlight crop / Video loop / Model turntable
  (a look without its media falls back to the plate). Hero motion: Pan and zoom * / Pan / Zoom / None. Words on the picture:
  Tone + Halo * / Edge scrim / Halo only (Shade reach = Effects › Tone). Spotlight zoom 1.5x / 2x * / 3x. Model size = the 3D Fit.
  Listen button: Shown * / Hidden (Unity's Audio Guide Hero Chip).
- Status: Look Ring * / Scale (the condition outline; fix LIB's "open, closed, under works").
- Quick Facts: Look Grid * / Chips / Big numbers; Show category: No * / Category / Category and level.
- Actions: Look Pill row * / Circles / Sticky; each button's Action: Open a block / Show on the wall.
- Story: Preview Automatic * / Custom; Page layout One scroll * / Chapters; Starts with: Story -- magazine * / Story -- short / Empty /
  the wall's templates (+ Templates list); Story resume (NEW): On * / Off.
- Timeline: Preview look Strip * / Vertical (the page is vertical); Now point Off * / On; a picture per event (NEW).
- Compare Points: no options (Compare By has one choice: drop).
- Dialogue: no options.
- Gallery: Look = a Carousel style; Pictures from: Its own * / Every picture on the card.
- Before and After: Start at 25 / 50 * / 75 % (share of Before shown).
- Hotspot Image: Spots Numbered * / Loupes; Spot motion Pulse * / Still; a spot Opens: Its own text * / a block.
- Video: Captions at start On * / Off; chapters show whenever the block has them (drop the Chapters look).
- 3D Model: Fit Yaw safe * / Sphere / At rest / Visible; Auto spin On * (off under Reduce Motion); See it in the room (NEW) on its page.
- 360 Panorama: Look around by Dragging * / Turning the phone; Starts facing (marker, degrees).
- Audio Guide: Look Player * / Chip; Speed button Narration * / Wide / Off; Captions at start On * / Off; Chapters (NEW, marker).
- Knowledge Check: Question type Multiple choice * / True or false (swipe) / Picture choice; Appears With the card * / After the card
  is read; Score None * / At the end; Retry (NEW) Off * / On.
- Poll: bars only with real results, else "Your choice" + thanks (a rule).
- Feedback: Answer with Thumbs * / Stars.
- Wall Locator: Look Strip * / Neighbours.
- Today Map: Look Map * / Then and now; Directions built from the coordinates when no link is given (a rule).
- Related: Look Strip * / Next along the wall; Points from Picked by hand * / Same category / Nearest (up to 6).
- Practical Info: each row's Icon Time / Access / Location / Info / Ticket / Light / none.
- Place In AR: Distance from the wall 0-200 cm = 10; Size Real size * / A set height (1-500 cm = 30) / Times the marker (0.5-20 = 3);
  When the visitor picks another point: Take it away * / Keep it standing. Drop Anchor (one choice).
- Contents: Rows Sections first * / Every block; Read ticks Shown * / Hidden.
- Sources: Look List * / With confidence; marker Content status None * / Verified / Draft.
### Elements (s3e) — style options
- Tone everywhere: Neutral / Section colour / Accent / Inverse (one name: "Section colour", not "Family colour").
- Title Size L on Display; Heading + Alignment; Eyebrow, Lede, Paragraph, List: no Tone (Eyebrow keeps Tone); Lede Size L.
- Pull Quote + Tone (Section colour *), Attribution per language (Author + Source).
- List Markers: Bullets * / Numbers / Steps (circles joined by a line; each item a title + text).
- Callout: + Postcard style (a tilted card, PROPOSED).
- Picture: Interaction Open the viewer * / Hold to reveal / Tilt to look / None; With no picture Hide * / Show the next heading's
  initial; Proportion (NEW) As the picture * / 3:2 / 4:3 / 1:1; per row: Alt text (NEW), Focus point (NEW).
- Picture Pair: Frame; Interaction Open the viewer * / None; each picture its caption + credit.
- Carousel: Layout Peek strip * / One at a time / Grid (2 x 2 + "+N") / Filmstrip / Stack; Position marks Dots * / Counter / None;
  Width Full bleed * / Inset; Frame; 2-12 pictures, each with caption + credit.
- Video: Frame; On tap Play here * / Open the player; Clip + Credit per row.
- 3D Model: Fit (as above). 360: Look around by. Audio: Speed button, Captions at start.
- Swatch Row: Show colour code Off * / On; Layout Row * / Grid; Name / Colour / Note per swatch.
- Spacer: 16 / 32 * / 48 (theme steps). Map: drop Paper / Night (the map is an authored picture).
- s3e note: an app may register its own element kinds.
### Patterns (s3p)
- ADD Text (one Paragraph) and Text with headings (Heading + Paragraph) — home of the old Rich Text.
- Steps uses List › Steps.

## 4 · Motion & Effects
- s4m: Motion level Full * / Reduced (the wall's; a phone or visitor asking for Reduce Motion always gets Reduced). New type
  By itself: Slow zoom (1 -> 1.06 and back, 28 s), Pan (26 s); the scroll one is renamed "Zoom with the scroll". Strength lives in
  the preset (no per-row strength). Fixed notes: the stagger stops after 5 rows; only the picture nearest the middle moves by itself.
- s4f: Light gains Living glass and Rim light (the card's one glow, D5: Open Row Shine warns when Rim light is on). Import an edge:
  SVG or a tileable PNG strip. Own strip picture effect (file, height 24).
- Feedback: unchanged.

## 5 · Card Controls
- s5top: Title on the bar Shown * / Hidden; the settings button is always shown (drop Hidden: it holds the visitor's Light / Dark);
  draw the sliders mark, not ☰.
- s5settings: Language (2+); Appearance Light / Dark / Follow the phone; Motion (visitor) Follow the phone * / Reduced; Text size
  (NEW) Default * / Large / Larger; Haptics and sounds (NEW) On * / Off.
- s5bar: Enabled; Position; Shape Floating * / Full width; Fill Solid * / Glass / Clear; Indicator Raised * / Pill / Underline / None;
  Raised depth; Chosen icon Filled * / Plain; Chosen colour Section colour * / Accent / Ink; Icon colour; Labels Icon and text * /
  Icon / Text / Text on the chosen; Shown from 2..30 blocks = 8; Shown at Half and Full * / Full only (Top only); Shown from the start
  Always * / After the hero; Hide while scrolling; Items before More 4 / 5 * / 6; A tap moves the card Smooth * / Instant; After a
  tap the card is at Full * / Where it is. Fixed: the chosen icon is always bold; the bar follows the scroll by runs; a second tap
  on the chosen item goes to the section's start; needs two sections.
- s5contents: Shown On *; Rows Sections first * / Every block; Read ticks Shown * / Hidden; Filter chips Shown * / Hidden (time,
  no headphones / camera / network, Clear).
- s5pager: no options; drawn under Pages ("2 of 6" + the next page's edge) and under Tabs with the bar off.
- s5openrow: Default look Plain / Nudge * / Shine. s5expand: + Open one at a time (shared with Accordion), the full-view button.
- s5back / s5crumb / s5prevnext as now; Page chips (ADD, sibling blocks of the section): Shown * / Hidden.
- s5loc: as now (+ hidden without the camera); keys ar_teal, glass_ping.
- s5sticky: Shown Automatic * / Shown / Hidden; Slides away while reading On * / Off.
- s5term: Opens as Popup by the word * / Under the paragraph; Underline a term (NEW) The first time on the card * / Every time.
- New components: Picture viewer (Counter Chips * / "3 of 12" / None; double-tap zoom 2x, pinch 1-4x), Video player (Captions at start
  On *), Placed model chip (no option: always shown; its Remove is the only way to take a model away).

## 0 · Preview & Test
- s0card: Forget saved answers (button). The demo card holds every kept kind once (fix: today it is the Ribeira card).

## Languages & Terms
- w1: Opens in (NEW) The phone's language when the wall has it * / The fallback; the full language list; reorder.
- w3: App texts filter; Back to the framework's wording per word.

## Specific Marker
- mcard: Card starter (Simple point / Rich point / AR point / Start from scratch; "Custom" once edited; confirm "Replace N blocks on
  <point>?"); per block Look, Frame, Reading time Automatic * / Set by hand, Open Row words; "N to check"; warnings (not shown, missing
  terms, an action twice, the hero needs its picture, the title on 3+ lines).
- mpages: Story Preview rows (Custom: Link a page row / Add own row / Unlink / Remove; broken link row), "Shows:" line, P marks;
  row tools: drag, Duplicate, Checked column, "(required, empty)", "Not shown yet" warnings; Template line (Re-apply, Save as template)
  and Save these rows as a pattern. Header page (title, eyebrow, subtitle, pictures, loop clip, model, focus point).

## Dropped (with reasons, for the developer)
Opens at Full · Top band Clear · Next card peek (s1peek) · Mix · Android earbud check (the framework decides) · Bands start · Drift
section motion · Vary · per-section chosen icon · Group By · Follow Scroll as an option (a fixed rule) · Bold chosen icon as an option
(fixed) · Settings button Hidden · Placed model chip option · per-point page chrome · Chapters look of Video · Anchor · Compare By ·
Show On Wall's neighbours · Map Paper / Night · per-row motion strength · the page's Motion table · Kind chips, tabs, the bar's second
tap, Progress map, Navigation presets · Tone on Lede / Paragraph / List, Size on Eyebrow / Paragraph / List, Alignment on Paragraph.

## Guide fixes (_3.3)
6.2 (s1peek is not Unity's Peek Next Card) · 6.6 (Own stock media built) · 8.3 Hero motion default Pan and zoom · 8.5 page list.
