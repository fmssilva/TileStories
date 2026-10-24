# Plan tracker -- _3.3 P4.4b part 2: the six reds, the hero row, the settings gear, close

GATE (done): push not needed (0 ahead of origin); compile ExitCode 0, 0 error CS (forced); fonts clean (no Fonts/SDF in git status);
captures TestEvidence/Gate/P4.4b2_gate_gameview.png + P4.4b2_gate_poieditor.png (opened; the first editor capture caught another app ->
deleted, retaken at 640 x 620). Unity place asked (b) for the capture. Tree = part 1's, uncommitted on 4cbb4dd (continued, not reset).

## Root causes (measured, before any fix)
- Rails (1713 px): the strip is 324 px (x 33..357); CardTrackFit gives ONE 280 px card per view, so card 1 sits at x 325..605 and the
  test's tap (465, 581) is off the strip and off the 390 px screen: Pick = nothing, no jump. Before the frame: 358 px, two cards. Same
  cause as the tracks (the frame's 2 x 16 padding + border). The jump's own measure (slot.layout.y in the content) is right.
- Tracks (260 px range): the same 34 px; the short timeline / 2-card carousel no longer fit 324.
- Audio (Idle): NOT the live edit. Diag: state Idle BEFORE the push in the red runs (the tap on the hero's Listen never started it);
  in 7/7 runs where it started, the live edit kept Playing at the same second. OpenFull taps without the rest wait SelectAtHalf has.
- Stars (ink-2 at the centre): the probe, not the drawing. PixelAt returns the DARKEST pixel of a 7 x 7 window; it was centred 1.4 px
  above the star's centre (y 0.55 of the 28 px glyph), and the hollow is only ~3.3 px to the stroke: the window reaches the ink-2
  outline (61,54,45), darker than white in light (in dark the ground is the darkest, so it passed). The render shows white inside.

## Steps
- [x] 1 Remove the three __Diag tests
- [x] 2 Edge-to-edge regions: BlockFrame.BleedClass + one PoiCard.uss rule (--ts-frame-bleed = -padding) on the timeline / related /
      gallery tracks + main picture (+ grid in the grid look only: the stack keeps its print offset), the rails' strips, before/after,
      hotspot, zoom, preview, video surface; ThemeFrameSceneTests.PicturesAndTracks_... (margins = -padding, region = frame's inner edge)
- [x] 3 Rails test: precondition "the tapped card is inside the strip"; rails + tracks green (36-test run)
- [x] 4 Audio: OpenFull waits for rest; TapListenAndPlay precondition; the audio fixture green
- [x] 5 Stars: CardIcons.StarCentre; CardGalleryChecks.PixelExactlyAt; both modes; >= 3:1 checks; green
- [x] 6 Hero row: one row (nowrap, focal shrinks), Listen = play + m:ss, name "Listen 2:44" / Pause; FOUND: Lamp eyebrow 6.69:1 (words
      lower) -> --ts-scrim-hero 0.92 -> 0.94 (>= 7:1 over pure white, ThemeContrastTests pair); hero fixture green
- [x] 7 HeroTitleFitRule (+ AdvanceWidth) + Editor HeroTitleMeasure + Card Content notice; EditMode green; PlayMode agreement green
      (1 / 2 / 5 lines)
- [x] 8 Gear + "Card settings" + native names (en, pt; es dropped: n-tilde not in the static marker atlases, falls back to "ES");
      LanguageLabel prefix kept out of CardStrings.Keys (the key-scan guard); the panel's mode mark follows ModeChanged (capture found it)
- [x] 9 Real-window Editor tests (Default Mode, Emphasis, 3-line notice with real typing) green
- [x] 10 Targeted 1182 (0 failed, 37 gated skips, 3601 s) -> full PlayMode 1288 (0 failed, 37 skipped, 4002 s) -> full EditMode 2078/2079
      (KeywordListFieldTypingTests order flake, green alone)
- [x] 11 Visuals judged (tracks + the timeline's words given air, stars both modes, hero row EN/PT, gear + panel both modes, glare)
- [x] 12 Docs: _3.3 Build log P4.4b part 2, 10-structure, _5.1 (notice + two rows), _3.1 15.3 + replacement row, __mixed_TODOs (scrim
      closed), RUN TIMES; Fraunces spike TMP asset restored (git checkout --); final report; scene

## Re-base log (old -> new, from source)
- Hero Listen pill: Title "Listen 2:44" -> "2:44" + PlayButton.tooltip "Listen 2:44" (CardAudioGalleryTests, PoiCardAudioSceneTests x2,
  PoiCardHeroSceneTests x2); playing: "Listen" + TimeText -> the running time alone, name "Pause"; PT "Reproduzir" -> "Ouvir 2:08"
- Settings button: text "Aa" / tooltip "Reading settings" -> gear glyph, no text, tooltip "Card settings" (CardSettingsGalleryTests);
  language buttons "EN" / "PT" / "ES" -> "English" / "Portugues" / "ES"
- Stars: PixelAt (darkest of 7 x 7 at the box centre) -> PixelExactlyAt at CardIcons.StarCentre, both modes
