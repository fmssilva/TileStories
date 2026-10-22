# Plan tracker -- _3.3 P1b: real content into the LivingRoom test wall (+ per-language media, per-language category names)

Brief: `__AI_worker.md` (P1b). Developer decisions at the plan gate (2026-10-22): BUILD per-language category labels and
per-language media IN this block (both were guide-vs-code gaps: one label / one asset for every language).

## GATE (done)
- [x] push: 0 ahead (HEAD 23bf1c7 = origin) -- [x] compile: ExitCode 0, no new error CS -- [x] fonts clean -- [x] no job running
- [x] captures opened: `TestEvidence/Card/P1b_gate_gameview.png`, `TestEvidence/Editor/P1b_gate_poieditor.png` (2 bad reads deleted)
- [x] baseline full EditMode 2027/2027 (140.9 s, background)

## Decisions (autonomous)
- Config is written through the POI Editor's own path: `JsonUtility.FromJson<WallConfigData>` -> edit -> `ToJson(prettyPrint)` ->
  both copies (Apps + StreamingAssets). The new blocks are built by a scratch Python script as JSON, spliced in by execute_code.
- New pictures go to `CardMedia/images/`, video + poster to `video/`, guides to `audio/`. Importer settings = a copy of an existing
  same-type `.meta` with a fresh guid (identical settings: textures 2048 / compressed q50, audio Streaming + Vorbis, video H.264 Low).
- Every placeholder / third-party file the brief names is repointed EVERYWHERE on the wall (lamp_military, lamp_residential,
  lamp_economic too) to its real equivalent, then deleted. Kept: `placeholder_hero.png` (media seam fixture), `lamp_tone.*` (the
  generated 20 s four-section seek fixture), `models/` (3D, out of scope). `generate_card_media.py` deleted (it only wrote the
  deleted pictures); `generate_card_audio.py` loses its castelo VTT part.
- Per-language media: `BlockFieldValue` / `BlockItemFieldValue` gain `asset_by_language` (LocalizedEntry list, like `text`); the
  `asset` stays the file for every language. `BlockFieldReader.Asset` picks the card's language, else `asset`. Opt-in per field:
  `BlockFieldDefinition.PerLanguage` (audio_guide clip + captions, video clip + captions); the Editor draws one row per other
  wall language under those fields only ("blank = same file").
- Per-language category label: `CategoryStyleEntry.label_by_language`; `TaxonomyNames.Category(taxonomy, key, language)`;
  the CARD reads it (header chip, collect series). Search has no visitor language yet -> results / suggestions / filters keep
  `label` (TODO for `_2.6`). Editor: one label row per other card language in the Category table.
- Fun fact headings: optional `heading` item field (LocalizedText); blank = the card's "Did you know?".
- 'l-stroke' (Jakub Halun's name) added to both static marker fonts with the documented TryAddCharacters procedure.
- Header caption / before_after credit have no field: credits travel in the gallery `credit`, zoom caption and sources blocks.

## Steps
- [x] 1b.1 Import media: 32 files; pictures nPOTScale None + mipmaps off (the stock settings stretched 4:3 to 1:1 and, without
      POT, fell to raw RGB24); 5 pictures cropped 1-3 px to multiples of 4 (ETC2); the clip's audio resampled 16 -> 44.1 kHz (WMF
      refused 16 kHz AAC). Build delta (Android, imported sizes): +31.1 MB new, -16.7 MB old = +14.4 MB
- [x] 1b.2 Per-language media: data + reader + builder + PerLanguage on 4 fields + Editor rows + tests (EditMode)
- [x] 1b.3 Per-language category label: data + TaxonomyNames + PoiSubtitle + header/collect + Editor rows + tests
- [x] 1b.4 fun_fact heading field + view; font 'l-stroke' (both marker fonts 122 -> 123 chars, still Static)
- [x] 1b.5 Content written (Painting 18 blocks, Lamp 62, other points repointed, category pt), placeholders deleted, tests
      re-based; full EditMode 2031/2031. Targeted Card/Poi PlayMode (49 fixtures): 961 run, 950 passed, 11 failed; 7 of the 11 were
      re-bases I had missed (cues 20 -> 26, the 0:12 video caption, texture 640 -> 854, the shown glossary words, the Lamp timeline
      dates, five spots, the seek to the middle of the 54.9 s clip) -> re-run green. 5 RED (11 - 7 + the glossary title follow-up fixed), need judgement -> HANDOFF below.

## P1b HANDOFF (first session) -> continued below as P1b part 2 (1b.6 = step 2.7, 1b.7 = steps 2.8-2.9)

## P1b part 2 (2026-10-22, second session): the 5 red tests, docs, captures, full runs

### GATE (done)
- [x] push: 0 ahead (HEAD 23bf1c7 = origin; P1b is all uncommitted in the tree)
- [x] compile: ExitCode 0, 0 `error CS`, no .cs newer than Library/ScriptAssemblies (TileStories.Tests.Runtime.dll 17:36)
- [x] Editor.log was 1.9 GB, growing ~2 GB/h: one engine line "Access version should be odd when acquiring lock" repeated since
      P1b's last asset refresh. Developer restarted Unity + deleted the log -> 97 KB, 0 spam lines, ExitCode 0
- [x] captures opened: `TestEvidence/Card/P1b2_gate_gameview.png` (room scan + painting, Edit Mode), `TestEvidence/Editor/P1b2_gate_poieditor.png`
      (Scene Configuration + the three tabs; first read caught 20 px of another app on the right -> re-read 855 px wide); window closed, 0 left
- [x] fonts: Oswald Bold SDF + Roboto-Bold SDF 122 -> 123, only U+0142 added, nothing removed, no top-level key changed -> KEPT

### Diagnosis of red 5 (numbers, LivingRoomScene Play Mode, 390 x 844 pinned, The Painting at Full, wall's own nav)
Listen = 1 block, h 182, top 3813; Play's top 4019 -> Listen spans 206 px. The jump lands exactly (offset 3813); the header collapses
so the viewport grows 525 -> 636 and the spy line is 636/3 = 212 px down = 4025 >= 4019 -> Play. Frame 1 (jumping): Listen (frozen);
frame 2 .. 1.5 s: Play, steady. => REAL `NavScrollRule` bug: a section shorter than a third of the viewport loses its mark to the
next one right after a jump lands it (tap "Listen", the bar marks "Play").

### Steps
- [x] 2.1 Fix `NavScrollRule.CurrentSection`: of the tops past the spy line, the FIRST that still starts on screen
      (top >= offset - LandingTolerance) wins; none on screen -> the last past the line (unchanged); end rule unchanged. Same function
      for sections AND the block spy (one line, they never disagree). Pure EditMode test with a short section (landed, scrolled a
      little, scrolled past, on a drag); mutation-prove (old rule -> new test red -> restore). _3.2 4.4 + 5.2 ("40 presses") corrected.
- [x] 2.2 Re-base red 1 (Next walk): after Finished, real drags to the scroll END, then the reveal + the bar ("57 of 57")
- [x] 2.3 Re-base red 2 (PT drag): drag until the scroll END (offset >= Range - EndTolerance), not until Finished
- [x] 2.4 Re-base red 3 (final screen): the case from the measured layout -- the first wall card (Lamp, then The Painting) with a
      block whose top > Range; the lowest such block; no hard-coded block key
- [x] 2.5 Re-base red 4 (Painting drag): drag until the end, cap 40 (was a fixed 8)
- [x] 2.6 Red 5: green by the fix (no test change). Proof: NavScrollRuleTests 16/16 (mutation: 2 red, restored); full EditMode
      2033/2033 (115 s); the 5 PlayMode tests 5/5 (103 s); Next walked The Lamp in 55 presses (one per target; was 40), 56 -> 57.
      + `CardTestInput.UnderTheSpyLine` replaced the private spy oracle of 8 fixtures (9 copies: TopElements had a block one too)
- [x] 2.7 Docs (1b.6): 10-structure (CardMedia tree, generate_card_media.py gone, new fields/tests of 1b.2-1b.4, the rule fix),
      _3.3 ("P1b done" under R5 + TODOs), CONTENT_MAP slots wired, README + SOURCES checked
- [x] 2.8 Captures (1b.7): Painting + Lamp, 390 pt, EN + PT, Peek / Half / Full + every Full screen to the end; judged; defects ->
      _3.3 TODOs [P3]/[P5] (no fixes)
- [~] 2.9 Targeted 49 Card/Poi fixtures -> full PlayMode (background) -> full EditMode; LivingRoomScene left open

### Done notes (2.7 - 2.8)
- 2.7: 10-structure (CardMedia tree = disk, 37 files; generate_card_media.py removed; generate_card_audio.py; config.json; WallConfigData,
  TaxonomyNames, CardConfigData, PoiSubtitle, FunFactBlockView, BlockFieldDefinition, BlockFieldReader, BlockFieldDrawer.Assets, NavScrollRule;
  tests DetailCardEditorTab, MediaRules, NavScrollRule, TaxonomyRowIdentity, CardTestInput) -- CRLF kept, ASCII only. CONTENT_MAP: slots
  wired (non-wired rows are n/a / kept / out of scope), the F5 table got a Status column. README = disk (checked file by file);
  SOURCES.md got a header naming the rows whose files stayed in the gather folder (castle_peacock, audio .txt, text/, tools/).
  _3.2 4.4 + 5.2 corrected (the rule; 40 -> 55 presses). _3.3 'P1b done' line: after the full runs.
- 2.8: a TEMPORARY fixture (__P1bCaptures, deleted after) rendered every screen via CardGalleryChecks.Render and tiled contact sheets
  (TestEvidence/Card/P1b_sheet_*). First pass: Lamp 'EN' came out Portuguese (the card keeps the visitor's language across points;
  the Painting PT pass ran before) -> deleted, re-captured from a fresh session (precondition: tooltip 'Close'). 15 defects -> _3.3
  TODOs [P3] x4, [P5] x7, [content] x1; status names per language -> __mixed_TODOs.

## HANDOFF (P1b part 2, 2026-10-22 ~22:10, session budget ~450K)
Done: GATE; the spy fix + its pure tests (mutation-proved); the 4 re-bases; the shared screen oracle; docs (10-structure, _3.2, _3.3,
CONTENT_MAP, SOURCES); captures judged + TODOs. Runs on THIS tree: targeted Card/Poi 935/935 (2999 s); full PlayMode 1219/1219 (3582 s, =
1218 suite tests + the temporary __P1bCaptures that was still compiled -- deleted before, compiled out after, reflection: 0 types); full
EditMode 2032/2033: KeywordListFieldTypingTests.TypingTwoKeywords... red in 3 of 3 full runs after the PlayMode run (green at 19:35 on
the same code, green alone). Bisected to the group [H-J]+Keyword (22 tests, red twice); every smaller subset green; a re-focus before
the click did NOT fix it (reverted, file = HEAD). Recorded in __mixed_TODOs.
NOT done: a green full EditMode run. NEXT: restart Unity and run full EditMode; if green, the state came from the PlayMode run (record
that); if red, log focusedWindow / keyboardControl / FieldRect at the click in the [H-J] group. Tree: compiles (ExitCode 0), nothing
committed, LivingRoomScene open.

### Test intents (one line each)
1. Next walk: every real tap on Next lands the block the bar named, the counter only moves forward, and the walk ends finished with the
   reveal counted.
2. PT drag: reading by hand to the end of the card finishes the bar (Portuguese words, whole at 390 px, on screen), and back up names a block.
3. Final screen: a block whose top can never reach the viewport top lands clamped at the range, and the spy names the last section.
4. Painting drag: real drags move the tab mark to the section on screen, through several sections, to the last at the end.
5. Painting tabs: a real tap on every tab lands its section at the viewport top (or clamps), one nav_jump each, and marks that tab.

## Re-based tests (old -> new), filled as they move
- (part 2) PoiCardBottomElementsSceneTests.ARealTapOnNext_...AllTheWayToTheEnd: the reveal + finished asserted after the walk -> after the walk
  AND a real drag to the scroll END (`DragToTheEnd`); 'Finished' + the last target asserted again on the grown card.
- (part 2) ...ARealDragToTheEnd_FinishesTheBar_...InPortuguese: drag until Finished -> drag until the scroll END (`DragToTheEnd`).
- (part 2) PoiCardNavSceneTests.AJumpToABlockInTheFinalScreen_...: the Lamp's lowest block -> the lowest block whose top > Range on the
  first measured card that has one (The Lamp, then The Painting; today The Painting).
- (part 2) PoiCardPaintingSceneTests.ARealDragOfThePaintingsCard_...: 8 drags -> until the end, cap 40.
- (part 2) NOT re-based, fixed in the rule: PoiCardPaintingSceneTests.ARealTapOnEveryTab_... (NavScrollRule.CurrentSection).
- (part 2) 8 fixtures' private SectionUnderTheSpyLine / BlockUnderTheSpyLine -> CardTestInput.UnderTheSpyLine (same subject, new rule).
- PoiCardSceneTests: Lamp subtitle "Military - c. 1700 on the panel" -> "Medieval castle and royal palace"; PT subtitle -> "Castelo medieval
  e paco real" + NEW assert PT chip "Governo Real".
- PoiCardSceneTests.About: story paragraphs 2 -> 3; key dates "1511, c. 1700" -> "c. 1500, 1938 to 1940"; big number "364" -> "800+";
  header paths castle_*.png -> images/castle_*.jpg (texture names unchanged); PT fun fact heading "Sabia que?" -> the fact's own
  "Pavoes nos jardins", text -> "Ha pavoes...", postcard "uma torre inteira" -> "Martim Moniz" + own heading.
- PoiCardSceneTests.Meta: PT sources row 0 title / row 1 author -> row 0 "O Grande Panorama..." / row 3 author "OpenStreetMap contributors".
- PoiCardSceneTests.Play: MC 2 -> 3 questions, prompt/choices/explanation/counters re-based, PT Q2 right answer index 2 -> 1;
  image choice damage_before/after -> paco_before_1755/paco_after_today, words Before/After -> The painting/The photograph.
- PoiCardSceneTests.Media: hotspot 4 -> 5 spots, spot 2 at (0.5, 0.3) "The keep" -> (0.73, 0.06) "A tall tower"; loupe 4 "The curtain
  wall" (no text) -> "The cathedral" (+ text); the "no text = title alone" assert MOVED to CardGalleryTests.Media (partial entry);
  gallery RefCount gallery_2.png -> images/castle_gallery_2.jpg; before_after "After the earthquake" -> "Today", textures re-based.
- PoiCardVideoSceneTests: clip castelo_s_jorge_video (200.3 s, 640x360) -> lisbon_square_castle_55s (54.9 s, 854x480); "3:20" -> "0:54";
  20 cues -> 3; posters castelo_s_jorge_1/2 -> castle_photo_1 / lisbon_square_castle_poster; 4 chapters (0:00/0:50/1:40/2:30) -> 3
  (0:00/0:20/0:28), jump 101 s -> 29 s; NEW: the PT card loads the PT captions (3 cues, first "A atravessar a Praca do Comercio").
- PoiCardAudioSceneTests: guide castelo_s_jorge_guide_pt (200.39 s, "3:20") -> guide_castle_en (164.52 s, "2:44"); Economic's
  portugal_tourism_guide_en (232.9 s, "3:52") -> guide_paco_en (191.40 s, "3:11"); placeholder cues -> the real cues at 0:12 / 82.26 s;
  NEW: the PT card plays guide_castle_pt and shows its first PT cue.
- PoiCardRealAudioTests 200.39 -> 164.52; PoiCardRealVideoTests clip name; PoiCardTapZoomTests tile_detail -> azulejo_detail.
- AudioMediaTests: fixture rows castelo (200.39 s, 20 cues) -> the four guides (164.52/26, 128.97/26, 191.40/30, 143.96/30); the
  "provisorio" letters check -> "Sao Jorge" in the PT VTT; the streaming check covers the four guides.
- CardAudioServiceTests 200.39 -> 164.52; VideoMediaTests 200.3 s 640x360 -> 54.9 s 854x480; DetailCardEditorTabTests tile_detail.png
  -> images/azulejo_detail.jpg (drop / wrong-kind fixtures).
- NOT re-based (pure path-rule strings, no file read): MediaRulesTests "castle_hero.png" / "gallery_1.png".
