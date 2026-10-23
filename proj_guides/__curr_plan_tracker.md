# Plan tracker -- _3.3 P4.4a: the header and the hero

Brief: `__AI_worker.md` (P4.4a, R16 row 3, R10.9, R11 v3 + v4, R9 D1 / D5, R12.3). Worker: Opus-class. Do not commit.
Before finishing: `git checkout -- Assets/Dev/StyleSpike/Fonts` + marker SDF fonts if a run rewrote them; LivingRoomScene open.

## GATE (done)
- [x] push: 0 ahead (HEAD 52a152b = origin/master)
- [x] UnityMCP: telemetry_status ok; console: only the XR-sim duplicate-ID lines
- [x] captures opened: `TestEvidence/Gate/P4.4a_gate_game.png` (Main Camera render: room scan + painting, Edit Mode, no markers),
      `TestEvidence/Gate/P4.4a_gate_editor.png` (POI Editor, Detail Card tab; a config-validation notice popped, closed after); 0 windows left, scene not dirty
- [x] compile: 0 `error CS` in Editor.log, last Tundra build success; fonts: git clean -> nothing restored
- [x] baseline: none (HEAD 52a152b; P4.3 full PlayMode 1254 with the two Menu reds)

## Design (decided)
- BAR = grabber + one row: header Root = `card-header__title` (one line, ellipsis) beside the round buttons; fixed height, margin-top
  overlaps the grab zone so the row lines up with the buttons. Peek = the bar's bottom. Title shown (opacity) only when the hero title is
  off screen: `HeroRules.BarShowsTitle` (Peek, or scrolled past the hero title). Replaces HeaderCollapseRule (removed, tests re-based).
- HERO = first in the scroll, for EVERY header: picture looks -> full-width picture + scrim (alpha-ramp PNG tinted by a theme token) +
  text box at the lower third (eyebrow, `card-hero__title` display face, subtitle) + actions row; text_only / compact -> a plain plate.
  Height (picture looks) = max(--ts-hero-min-height, Half - Peek), set by the sheet (`HeroRules.Height`).
- ACTIONS: `HeroActionsRule.Of(entries)` -> primary = first footer (sticky) actions entry, listen = first pinned-top audio_guide else first
  audio_guide. The stack binds a second ActionsBlockView / an AudioGuideBlockView (hero_chip look) into the hero's actions row; the
  pinned-top hero_chip block gets no slot (PinnedTop container retired). hero_chip words: "Listen m:ss" (CardStrings hero_listen).
- CHROME: CardChromeRule inputs HeroBottom + ScrollOffset: while the hero is in view the bottom area hides and the strip needs the whole
  visible hero beside it.
- MOTION: header Choice `hero_motion` (pan | zoom | none, default pan, image_parallax only, (i)); `HeroMotionRule`: pan 26 s ease in-out
  back and forth when wider than the frame, zoom 1 -> 1.06, parallax 0.45x, text rise + fade; reduce motion = centre still, no rise.
  Injectable clock on HeaderBlockView.
- TYPE: --ts-size-heading 20, --ts-size-hero 46 / -narrow 40, bar title 20; display selectors + hero title, timeline date, facts values,
  big numbers. Fraunces Bold only if captures demand.
- CATEGORY: quick_facts Toggle `show_category` (a first fact "Category"); header `show_level` removed. EYEBROW: optional header field
  `eyebrow` (blank = none); Lamp + Painting filled EN + PT.

## Steps
- [x] 4.4a.0 Measure "before": `LookMeasure` (TypefaceMeasure generalised by domain) + `HeroMeasureSceneTests` (Lamp / Painting x EN / PT
      x Menu none / drawer_tree; stops, header, title; Peek / Half / Full renders) -> TestEvidence/Hero/before
- [x] 4.4a.1 Bar: HeaderBlockView bar title, USS bar row, PeekPart, HeroRule.BarShowsTitle, remove HeaderCollapseRule; EditMode tests
- [x] 4.4a.2 Hero: layout + plate + scrim (foot: solid + ramp) + tokens + HeroRule.Height (a minimum) + HeroActionsRule + stack wiring + Listen
- [x] 4.4a.3 hero_motion field + HeroMotionRule + view tick (Clock) ; Editor row = generic drawer (check on the real window: TODO)
- [x] 4.4a.4 Type scale (heading 20, hero 46 / 40 under 400 pt) + display figures (facts values, timeline dates, hero title); category ->
      quick_facts Show Category; eyebrow field (one line) + content
- [x] 4.4a.5 Chrome: CardChromeRule layer 4 (bottom area waits for the hero, strip needs the visible hero); re-based chrome tests
- [x] 4.4a.6 Scrim worst-pixel measure: Painting 7.78 (mid pan) .. Military 10.0, Religious 12.0 (EN); Lamp after the 40 pt title
- [x] 4.4a.7 Targeted fixtures (Card / Poi regex); re-base log
- [x] 4.4a.8 "after" measure + side by side + pan / scroll frame sequences; look item by item
- [x] 4.4a.9 Full PlayMode (background), full EditMode
- [x] Part 2 docs: _3.3 Build log, 10-structure, TODOs closed, 40-testing RUN TIMES
- [ ] Finish: restore fonts, LivingRoom scene, final report

## Measurements (before -> after) -- TestEvidence/Hero/{before,after,compare}/
- Peek: lamp en 168 (menu 198) -> 57; lamp pt 198 (menu 198) -> 57; painting en 104 / pt 104 (menu 134) -> 57 -- every case one value
- header at Half: 116..231 -> 48 (the bar row); hero 358 x 200 (inset) -> 390 x 281 (= Half - Peek, full width)
- Half 337.6 / Full 796 unchanged; Half viewport: lamp en 50, pt 103 (no footer) -> 281, the whole hero, action in it

## Runs
- EditMode targeted (6 fixtures) 144/144; full EditMode 2060/2061 (140 s, focused): the red = KeywordListFieldTypingTests (the documented
  focus flake, red after a full PlayMode run) -> 1/1 alone.
- Hero / Menu / Facet / Chrome fixtures green (Menu's TheLampsStops_DoNotMove_WithAMenuSet unchanged).
- Targeted regex set (Card|Poi|Search|Theme|Zoom|Marker|LodWallScene|LayoutStruggle|LivingRoom|Family|Typeface|Hero): run 1 1172 (3430 s),
  run 2 1172 (3354 s) -- failures capped at 25 by the tool -> the hidden fixtures re-run (493, then 268, 49, 15): every red re-based
  (log) or fixed (the empty-subtitle rule); all green on re-run.
- FULL PlayMode: 1280 results / 1278 tests, 3839 s, background at every poll (front at the end): 2 red, both a tap during the sheet's rise
  in an unfocused Editor (PoiCardLanguageSceneTests chip, PoiCardRealAudioTests Listen) -> wait on rest, 7/7 (Unity in front: proves the
  fix, not focus independence).
- Scrim: 189 measures, worst 7.06:1 (Lamp eyebrow, pan 0); Painting 7.78+ ; Military 10.9; Religious 12.0.

## Re-base log (old -> new, from source)
- PoiCardSceneTests.TheLamp_OpensAtPeek: chip "Royal Government" -> no chip in the header; peek shows the bar, bar title shown.
- PoiCardSceneTests.ScrollingTheStack_CollapsesTheHeader... -> ScrollingTheHeroAway_HandsTheTitleToTheBar_AndTheTopGivesItBack (the
  collapse is gone by design: bar fixed, title handoff).
- PoiCardSceneTests lamp_military chip "Military" -> its Video Loop hero over its poster; PT chip "Governo Real" -> the PT quick fact.
- PoiCardSceneTests.About: TheHeaderChip_... -> QuickFactsShowTheCategory_Alone_WithItsLevel_OrNotAtAll; parallax 0.5 -> 0.45
  (HeroMotionRule); "header collapsed" -> "bar shows no title".
- CardGalleryTests header entries: chip -> none; "a hero exactly for a picture look" -> every header a hero, HasPicture for the picture
  ones, the plate class else; subtitle contrast on the card only for the plate (the picture's words: the scrim test).
- CardGalleryTests.About parallax: HeaderCollapseRule.CollapseAfter -> a known 4 px offset; 0.5 -> 0.45.
- CardAudioGalleryTests hero chip pinned / peek -> TheHeroChip_IsTheHerosListen (no slot, in the hero's actions, "Listen 0:12").
- PoiCardAudioSceneTests Lamp chip pinned -> the hero's Listen ("Listen 2:44"); Views include HeroActionViews (also Video / RealAudio).
- CardGalleryChecks: SettledBlock takes the hero's Listen for a pinned-top entry; AssertHeadingAndGap: no slot = a hero action.
- PoiCardChromeSceneTests: Lamp Half "keeps its footer, strip gives way" -> "shows its whole hero with its action, sticky bar once read";
  "collapsed header gives Half its strip" -> "the hero read away gives it back"; NothingShowsAtPeek precondition read past the hero.
- PoiCardNavContentsSceneTests: precondition "header collapsed" -> "the hero scrolled away".
- FieldVisibilityRuleTests header looks: - show_level, + eyebrow (not compact), + hero_motion (image_parallax).
- CardRulesTests: HeaderCollapseRule test removed with the rule.
- (targeted run 1-2) CardAudioGalleryTests: clip refcount 2 -> 3 (the hero's Listen loads the length too; 1 after close, unchanged).
- CardGalleryTests.Actions sticky: "footer at the card's bottom" -> the hero (plate) shows the same action, the footer waits below the edge.
- CardGalleryTests header entries "long" at Half: "subtitle starts inside the card" -> "the hero's title starts inside the card" (a
  long title + line grow the hero past Half: words never squashed); "nosubtitle": REAL BUG fixed (the empty-subtitle rule was lost).
- CardGalleryTests.HeaderImageParallax: its own POI's 6-line title -> "The Royal Palace" (the test is the picture's); sample near the top.
- CardNavGalleryTests nav blocks: "first in the scroll" index 0 -> 1 (after the header's hero).
- CardVideoGalleryTests loop poster colour: sampled at the hero's top band (PictureColourIn), not under the words' scrim.
- PoiCardAudioSceneTests / PoiCardRealAudioTests / PoiCardVideoSceneTests: "tap the chip at Peek" -> at Half (SelectAtHalf); PT chip
  words "Visita guiada ao castelo" -> "Ouvir 2:08"; Economic: player picked by look, the hero's Listen = the same clip.
- PoiCardBottomBarSceneTests (2): read past the hero first (shared CardTestInput.ReadPastTheHero, the chrome fixture uses it too).
- PoiCardEdgeSceneTests: the dot level with the strip = the middle of the strip-column overlap (the taller viewport moved the column).
- PoiCardExpandableSceneTests / PoiCardLiveUpdateTests / PoiCardSceneTests (full card kinds, every heading, Military): the hero chip
  is a hero action view, not a stack view (62 -> 61 + the hero's two).
- PoiCardSceneTests: Drag-up subtitle read from the hero; QuickFacts + "Category" first; split/spotlight "no hero" -> no picture;
  sticky tap -> the hero's action. PoiCardSheetDragSceneTests: the typed 100 px move -> 60 % of Peek..Half (Peek is 57 now).
- PoiCardBottomElementsSceneTests dock tap: ScrollToTop ends with a resting finger (see the fling finding).

## Findings during the build
- Pan + GrabStill: a moving hero never holds still -> tests own the clock (HeaderBlockView.Clock), HeroMeasure freezes it.
- Scrim v1 (a full-frame gradient, the prototype's) failed: 1.04:1 eyebrow over sky (the hero is 281, not 440: words cover ~70 %).
  Linear blending needs alpha ~0.9 behind white words over a sky -> v2 the scrim FOLLOWS the words (solid foot + 96 px ramp above).
- Lamp EN: "Show me where it is" + "Listen 2:44" do not fit one row at 390 -> the focal pill keeps one line and takes its own row; with
  the 46 pt two-line title the words were squashed (subtitle over the pill) -> hero 40 pt under 400 pt wide, hero height = a MINIMUM.
- Peek read 796 after a live Menu push: Show -> SetStop -> RecomputeStops ran before layout, the re-attached bar's world rect was stale
  -> the peek is now the header SLOT's own layout (never re-parented).
- PT Lamp eyebrow wrapped (hero 305) -> the eyebrow is one line (kicker), the Lamp's eyebrow shortened (content).
- MCP get_test_job caps failures at 25: the first targeted run hid every red after it -> re-ran the hidden fixtures separately.
- FINDING (_3.2, not fixed): a jump started right after a fling inherits the ScrollView's leftover velocity (probe: landed at 8823, drifted
  to 8764 over ~70 frames, the block stayed at 8862). Exposed by the dock test once the hero changed the timing.
- FINDING (design): a very long hero title (the gallery's "long" entries, 6 lines at 40) fills the hero; the hero grows and Half shows
  only the title. Authors should keep hero titles short; an Editor warning is a P8 candidate.
- FINDING (flake?): PoiCardTopElementsSceneTests.ARealDragOfTheStack(progress_map) red once (block_30 vs 31), green alone.
- KNOWN: PoiCardBottomElementsSceneTests.ARealDragToTheEnd "Layout update is struggling" (P4.1's open entry, __mixed_TODOs).
