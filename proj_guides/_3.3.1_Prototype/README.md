# _3.3.1 Prototype -- the POI detail card's look, feel and motion (2026-10-23)

The interactive design prototype the `_3.3` re-plan was judged on (R9-R11 of `_3.3_POI_Card_Styles.md`). It is the
**reference for the look**, not code to port: every effect in it maps to a UI Toolkit 6.3 mechanism listed in `_3.3` R11.

- **Live version:** https://claude.ai/artifact/2XsT5Ro1REGK4rRrS5vedc (a Design canvas, private to the developer; artboard
  "Ribeira Palace - card flow", phone 390 x 844, Play to interact). Versions: v1 (P3 gate), v2 (living glass, motion), v3 (metal
  rim, hero over the picture), v4 (panning hero, pictures inside blocks, "All pictures") -- this folder holds **v4**.
- **The premium target (v5):** `Premium_v5.dc.html` (one interactive
  artboard, its Tweaks = the Look and Navigation Bar options of `_3.3` "S4 review, third round"). It supersedes v4's look where they differ.
  (The v5 canvas is gone; `Premium_v5.dc.html` is its record.)
- **Block definitions (v6):** `v6/` (`Main.dc.html` + the `Edge.dc.html` child: a section's divider and how it begins), live at
  https://claude.ai/artifact/AbjCw71ZMvdVkovGQppBB7. Tweaks "Sections": Start (Plain / Bands / Fade / Wave / Tiles), Divider (None /
  Hairline / Ornament / Azulejo / Mosaic), Colour (Group / Accent / Neutral), Intensity, Motion (Still / Reveal / Drift), Vary (Same /
  By Group); Look adds Stop (Full / Half: the hero's words at the foot of what is seen) and Top Band (Fade / Glass).
- **Files here:** `Main.dc.html` (the artboard's whole source: markup, styles, the logic class), `canvas.json` (the canvas
  index). They run only inside the Design canvas (its `support.js` runtime and its uploaded pictures); this copy is the record.
- **Tweaks in the canvas:** mode (light / dark), glass (tinted / frosted / solid), fonts, preview cap, reduce motion.
  Chosen by the developer: light default, tinted, Fraunces + Atkinson Hyperlegible Next, cap 0.5, motion on.

## Pictures (canvas asset -> the test wall's content)

900 px copies of `proj_guides/detail_card/content/images/` (sources and licences in `content/SOURCES.md`):

| `/_blob/` id in the source | file |
|---|---|
| 62ec38f082ea17147941c71b371b2836 | paco_hero.jpg |
| 580b9bc2a3dfa2b20c025043eb2531f3 | paco_gallery_1.jpg |
| 1a15b527dc4fc1b9ab50aa5004dc1eb8 | paco_gallery_2.jpg |
| 51b55b7a570249d27e29be2d883cfbbd | paco_gallery_3.jpg |
| 9e3151f0d2f8f36bf0762a6d42d4ec3b | paco_gallery_4.jpg |
| be43abe742fafb818d3027d854eac4d0 | paco_before_1755.jpg |
| 859a087ab6c150825c75994ed346be9a | paco_after_today.jpg |
| e1597e92cba3082ab7b7c3f019938252 | azulejo_detail.jpg (also the "camera" behind the sheet) |
| 7dbc2ef927d7c937fe6394bf2a6e007c | paco_hotspots.jpg |

## What it covers, and what it does not

Covered: the hero, section glass sheets and openers, quick facts, rich_text preview, fun_fact (flip, dog-ear), timeline
(horizontal strip), gallery ("All pictures"), before_after, hotspot_image, audio_guide, knowledge_check, sources, the full-view
page (Previous / Next), the nav strip and the scroll spy.

NOT covered -- every other kind still needs its design confirmed the same way in P5 (preview, page, affordance demo, light +
dark): video, model_3d, panorama_360, place_in_ar / show_on_wall, zoom_image, person, story_chapters, compare_points,
dialogue, pull_quote, process_steps, swatches, actions, status, poll, collect, feedback, practical_info, today_map,
wall_locator, related, the four nav blocks, and the future kinds (AI / voice guide).
