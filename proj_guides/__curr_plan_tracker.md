# Plan tracker -- _3.3 P1a: real content for the test wall (gather + write, NO Unity)

Brief: `__AI_worker.md` (P1a). GATE: read-only on the repo; write ONLY under `proj_guides/detail_card/content/` (plus this file and
`__mixed_TODOs.md` for out-of-domain ideas). No Unity, no `TileStories/` edits, no writing git, no commit.

Scratch tooling (NOT in the repo): `%TEMP%\claude\...\scratchpad\` -- `cs.py` (Commons API helper), `build_images.py` (download + crop +
resize, writes `images_meta.json`), `osm_map.py`, `narration.ps1` / `build_narration.py`, `ffx/` (ffmpeg unpacked from the
imageio-ffmpeg wheel). The build scripts are copied into `content/tools/` so the content can be rebuilt.

## Decisions (autonomous)
- Unity gate of the generic worker brief is SKIPPED on purpose: the P1a brief forbids driving Unity.
- Sources: Wikimedia Commons only (PD first, then CC BY / CC BY-SA). OSM tiles for the two maps (ODbL, attribution drawn on the picture).
- Names: images are named by ROLE and place (`paco_*`, `castle_*`), not by the placeholder names, so P1b maps them explicitly.
- The Painting has no `before_after` block today and the Lamp's one says "After the earthquake": the pair is supplied (1755 painting
  vs today's square); P1b decides the block / labels (CONTENT_MAP "Notes for P1b").
- Narration: only en-US voices exist (David, Zira); no PT-PT voice. EN narration = David. PT-PT narration is synthesized with the
  closest available voice and FLAGGED as a stand-in; the script re-runs with a PT-PT voice once one is installed (timings and VTT are
  measured, never guessed).
- Video: three own-work deror_avi clips (CC BY-SA 3.0) joined into 55 s; the aerial clip (licence review pending) was withdrawn.

## Steps
- [x] 1. Read rules, spec, F5 + chunk E, README, config blocks of `painting` and `lamp`
- [x] 2. Pictures: find, download, crop to the block proportion (`content/images/*.jpg`) -- 19 pictures
- [x] 3. Two OSM maps (`castle_map.jpg`, `paco_map.jpg`)
- [x] 4. Hotspot positions noted (5 each)
- [x] 5. Video clip cut + poster
- [x] 6. Text EN + PT-PT (`content/text/*.md`): F5 fixes, captions for every picture, quiz, fun facts, timeline
- [x] 7. Narration scripts (EN, PT-PT) -> wav -> mp3 + exact VTT (`content/audio/`)
- [x] 8. `CONTENT_MAP.md` (Part 1) with every slot filled
- [x] 9. `SOURCES.md` (Part 5): one row per file
- [x] 10. Later list (3D, 360), `__mixed_TODOs.md` ideas, final report (60-finishing 6.4); no commit
