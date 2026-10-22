# CONTENT MAP -- P1a (what the Painting and the Lamp show, and what fills each slot)

Source of the "today" column: `config.json`, points `painting` (= Ribeira Palace) and `lamp` (= St George's Castle), read 2026-10-22.
Languages: every text block has EN + PT-PT. Pictures: long side <= 2048 px, JPG q85 (the maps JPG q92). Paths are under `content/`.
All slots are FILLED (status column), except where it says otherwise. Text lives in `text/PACO_RIBEIRA_text.md` (P) and
`text/CASTELO_SAO_JORGE_text.md` (C); every picture's caption is in the text file next to its slot.

## The Painting (`painting`) -- Ribeira Palace

| Block | Kind / look | File today | Expects | Filled with | Status |
|---|---|---|---|---|---|
| block_1 | header image_parallax | `damage_before.png` | 16:9 wide landscape hero + title / subtitle | `images/paco_hero.jpg` 2048x1152; title, subtitle, caption (P 1) | filled |
| block_2 | quick_facts grid_hairline | -- | 4 to 6 label / value pairs | six facts (P 2) | filled |
| block_3 | rich_text lede | -- | lead + paragraphs, "2 min" | 1448 EN / 1474 PT characters, 269 / 270 words, lead + 3 paragraphs (P 3) | filled |
| block_4 | timeline vertical | -- | 4 to 6 dated events | six events (P 4) | filled |
| block_5 | gallery carousel | `gallery_1..4.png` (4:3, #3 portrait) | 4 pictures, one portrait, caption + credit each | `images/paco_gallery_1..4.jpg` (#3 portrait 1536x2048; #1 is only 719x412) (P 5) | filled |
| block_10 | zoom_image pinch | `tile_detail.png` 1:1 | high-res square-ish azulejo + caption | `images/azulejo_detail.jpg` 1824x1824 (P 6) | filled |
| block_6 | fun_fact flip | -- | 2+ facts WITH headings | three facts with headings (P 7) | filled |
| block_7 | audio_guide player | `audio/portugal_tourism_guide_en.mp3` (232.9 s, no captions) | a real 2 to 4 min guide + exact captions | `audio/guide_paco_en.mp3` 191 s + `.vtt`; `guide_paco_pt.mp3` 144 s + `.vtt` | filled (PT voice: Piper pt_PT, see Audio) |
| block_11 | wall_locator strip | -- | no content | none | n/a |
| block_8 | related carousel (nearest) | -- | real neighbours | four real neighbours (P 11); the rename is `_3.3` | text filled, rename out of scope |
| block_9 | sources list | Wikipedia x2 | real sources | P 12 / `SOURCES.md` | filled |
| NEW | before_after slider (the card has none today) | -- | 4:3 pair, same proportion, similar angle | `images/paco_before_1755.jpg` + `images/paco_after_today.jpg`, both 2048x1536, labels (P 8) | filled; P1b adds the block |
| NEW | hotspot_image (the card has none today) | -- | 3 to 5 features with positions | `images/paco_hotspots.jpg` 2048x1704, five spots (P 9) | filled |
| NEW | knowledge_check x3, person | -- | questions with plausible wrong answers; a person line | P 13, P 14 | filled |
| NEW | today_map | -- | OSM export + attribution | `images/paco_map.jpg` 768x432 | filled |

## The Lamp (`lamp`) -- St George's Castle (blocks that show a picture, a clip or long text)

| Block | Kind / look | File today | Expects | Filled with | Status |
|---|---|---|---|---|---|
| block_1 | header image_parallax (+ then / now) | `castle_hero.png` 768x432; pair `castle_then.png`, `castle_now.png` 4:3 | 16:9 hero; a 4:3 pair | `images/castle_hero.jpg` 2048x1152; `castle_then.jpg`, `castle_now.jpg` 2048x1536 (C 1) | filled |
| blocks 6, 7, 8 | quick_facts chips / grid / big numbers | -- | 3 / 4 / 2 facts | C 2 | filled |
| block_2 | rich_text plain, 3 min | -- | about 150 words | 878 EN / 936 PT characters (156 / 167 words), 3 paragraphs (C 3) | filled |
| blocks 3, 4, 5 | rich_text drop_cap / lede / sections | -- | 1 to 3 short paragraphs; 3 sections | C 3 | filled |
| blocks 22, 23 | timeline vertical / horizontal | -- | 4 to 6 events | six events; four (C 4) | filled |
| block_26 | story_chapters | -- | 3 chapters | kept (already real-shaped) (C 4) | kept |
| blocks 29..32 | gallery carousel / grid / filmstrip / stack | `gallery_1..4.png` | 4 pictures, one portrait, caption + credit | `images/castle_gallery_1..4.jpg` (#3 portrait 1536x2048) (C 5) | filled |
| block_33 | before_after slider | `damage_before.png`, `damage_after.png` 4:3 | a same-proportion pair | `images/paco_before_1755.jpg` + `paco_after_today.jpg`; labels become "Before 1755" / "Today" (C 5) | filled; label change for P1b |
| block_34 | zoom_image | `tile_detail.png` | square azulejo detail | `images/azulejo_detail.jpg` | filled |
| blocks 35, 36 | hotspot_image numbered / loupes | `castle_then.png` + 4 spots | 3 to 5 spots | `images/castle_then.jpg` + FIVE spots (C 5) | filled |
| blocks 39, 40 | today_map static / bridge | `castle_map.png` 768x432 | OSM export, attribution | `images/castle_map.jpg` 768x432 | filled |
| blocks 9, 10 | fun_fact flip / postcard | -- | facts with headings | three facts (C 6); extra picture `images/castle_peacock.jpg` | filled |
| blocks 24, 25 | person row / card | -- | name, role, one line | C 7 | filled |
| blocks 11, 12 | pull_quote | invented quotes | a real, checked quote | NOT rewritten: needs a verified source text (C 7) | no free source gathered |
| blocks 43, 44, 45 | knowledge_check | -- | questions with plausible wrong answers | C 8 | filled |
| block_28 | practical_info | -- | six rows | C 9 | filled |
| block_56 | video inline | `video/castelo_s_jorge_video.mp4` 200 s, `photos/castelo_s_jorge_2.jpg` poster, placeholder VTT | a 30 to 120 s clip, poster, captions | `video/lisbon_square_castle_55s.mp4` (55 s, 854x480, sound), poster, `.vtt` + `_pt.vtt` | filled |
| block_57 | video chapters (takeover) | same | chapters inside the clip | chapters 0:00 / 0:20 / 0:28 (C 10) | filled |
| blocks 54, 55 | audio_guide hero_chip / player | `audio/castelo_s_jorge_guide_pt.mp3` 200 s + placeholder VTT | real 2 to 4 min guide + exact captions | `audio/guide_castle_en.mp3` 164 s, `guide_castle_pt.mp3` 129 s, each with `.vtt` | filled (PT voice: Piper pt_PT) |
| blocks 15, 16 | sources | invented rows | real rows | C 11 / `SOURCES.md` | filled |
| blocks 13, 14, 17 to 21, 27, 37, 38, 41, 42, 46 to 53 | status, actions, swatches, steps, polls, dialogue, size_comparison, compare_points, wall_locator, related, feedback, collect, show_on_wall | -- | short UI text about the fixture | left as they are | out of scope |
| blocks 58 to 62 | model_3d, panorama_360, place_in_ar | `default:` models and `models/146267-LivingRoom2-tex.glb` | 3D / 360 | untouched; free candidates in "Later" | out of scope |

## The F5 text fixes (`_3.2.1_Audit.md` F5 items 2, 3, 5)

| Item | Proposal | Where |
|---|---|---|
| 2 one name | marker label = card title: "Ribeira Palace" / "Paço da Ribeira"; subtitle "Royal palace, lost in 1755" / "Palácio real, perdido em 1755". Same rule for the Lamp: "St George's Castle" / "Castelo de São Jorge". Renaming `name` is the `_3.3` rename block | P 0, C 0 |
| 3 translated category | `royal_government` -> PT "Governo Real" | P 0 |
| 5 fun-fact headings | each fact has a heading ("Lost in a day", "A name that stayed", "Eight months of music") | P 7 |
| 5 doubled "pinch" | the zoom caption describes the picture only; the card's hint stays | P 6 |
| 1 placeholders, "PLACEHOLDER DETAIL" | every Painting and castle slot above has a real picture | images |
| 7 fixture neighbours | four real neighbours proposed | P 11 |
| 4, 6, 8 | design / layout items: P3 / P5, not P1a | -- |

## Audio

- Windows voices installed (System.Speech and OneCore registry, checked 2026-10-22): Microsoft David (en-US), Zira (en-US), Mark (en-US). **No PT voice.** EN narration = David (`tools/narrate.ps1`).
- **PT-PT narration = Piper, a local neural voice (`pt_PT-tugao-medium`), run by `tools/narrate_piper.py`** (`pip install piper-tts`, voice files from huggingface.co/rhasspy/piper-voices, nothing sent anywhere, no admin rights). The first Zira stand-in (English voice reading Portuguese) was replaced. I could not listen to it; the pronunciation is the model's, so give it one listen.
- Lengths: paco EN 191 s (30 cues), paco PT 144 s, castle EN 164 s (26 cues), castle PT 129 s. Every VTT is built from the measured length of each cue's audio (`tools/build_narration.py`), so captions match the speech; the mp3 ends 0.6 s after the last cue (trailing gap).
- Pace: EN rate -2; PT length_scale 1.6 (a guide's pace, needed to pass 2 minutes). Gaps 0.55 s after a sentence, 0.12 s inside one.

## Video

One clip, 55 s: three own-work clips by deror_avi (Praça do Comércio walk, the view from the castle, costumed performers in the castle) joined, CC BY-SA 3.0, with the original ambient sound. It replaces the 200 s developer clip. The aerial clip first cut was withdrawn because its licence review is pending (SOURCES "Flagged").

## Notes for P1b

- The Painting has NO before_after, hotspot_image, quiz, person or map block today; the pairs, spots and text are supplied so P1b can add them (or point the Lamp's blocks at them).
- block_33 labels: "After the earthquake" -> "Today".
- Glossary markup `[[shown|term]]` is kept in the castle text.
- `paco_gallery_1.jpg` is only 719x412.
- Pull quotes (blocks 11, 12) are still invented fixtures.
- Credits: the gallery `credit` field and the zoom / before_after captions carry the author and licence; CC BY / CC BY-SA need them shown in the app.

## Later (NOT gathered, NOT used): 3D models and 360 panoramas

Commons has no category for either of Lisbon (the searches returned nothing). Unverified leads only, to check licence by licence:
- 3D: Sketchfab, filter "Downloadable" + CC0 / CC BY (a model titled "Streets of Lisbon", https://sketchfab.com/3d-models/streets-of-lisbon-1c19e901b8f747b1929cbd5e337bcfe5, and the profile https://sketchfab.com/geodrone/models; licences not read); Smithsonian 3D (CC0 objects); Europeana 3D.
- 360: Poly Haven HDRIs (CC0, not Lisbon: usable as a generic interior); any CC BY-SA 360 photo of the square on Wikimedia / Mapillary.
- 2D extras: A. C. Inchbold's 1907 "Lisbon and Cintra" (public domain on Commons: `File:Lisbon and Cintra, Inchbold, 1907.djvu`).

## Real lengths (what the card must hold)

Rich text 878 to 1474 characters (156 to 270 words); timeline event text 36 to 110 characters; gallery caption 70 to 110; fun fact 100 to 150; quiz question 40 to 60 characters with three options of 10 to 40; narration 164 to 226 s of audio from 325 to 384 words; the video 55 s; the pictures 719x412 to 2048x2048.
