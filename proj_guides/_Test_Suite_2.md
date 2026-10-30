# Test Suite Review 2 -- a test ground for fast, isolated card blocks (M1, AUDIT)

The second run of `skills/Test_Suite_Review.md`, AUDIT only (nothing changed). Its three targets, and every finding serves one:
**T1** a component block tests in a few minutes on its own fixtures + EditMode; **T2** one change to a block kind is made in ONE place
of the test data; **T3** no test spends time on something old, retired or no longer true.
Developer's pick (2026-10-30): the reference POI is the **Ribeira Palace** (`painting`); another POI only where a case needs it.

**Inputs.** HEAD 5801c6d. The last FULL PlayMode (C2's closing run, 2026-10-30 08:00-09:16, Unity behind): 1295 tests, 1132 passed,
0 failed, 163 skipped, **75.9 min** (`TestEvidence/Harness/fixture_times.csv`; per-test times from that run's TestResults.xml). One full
EditMode run in this session for its per-fixture times (section 3.4). Two Sonnet subagents read the tests (section 6); their claims were
spot-checked before use.

## 1. PART 1 -- the test data (T2)

### 1.1 What the shipped wall holds (`Apps/LivingRoom/config.json` = the StreamingAssets copy, byte-identical)

26 POIs; 8 carry a card:

| POI | name | blocks | role in the tests today |
|---|---|---|---|
| `lamp` | St George's Castle | 62: every kind (~37) and every look | THE test card: 100 test files name it |
| `painting` | **Ribeira Palace** | 18 (the only `story`) | 40 files: NavBar, Sections, Locate, BrightPage, Story, PageChrome |
| `lamp_military` | Castle Keep | 6, header `video_loop` | a 2nd card, a video-loop hero, "a card without X" |
| `lamp_religious` | Lisbon Cathedral | 1, header `model_turntable` | a turntable hero |
| `lamp_residential` | Alfama | 6, no header, `nav_contents` | a headerless card, video siblings |
| `lamp_economic` | Feira da Ladra | 1, `audio_guide:player` | a second audio for switch / queue |
| `lamp_infrastructure` | Tram 28 | 0, status unknown | no card |
| `camera` | The Camera | 0 | the empty point (Card Starter) |

The other 18 POIs (the Painting's and the Camera's five neighbours each, 8 `dev_*` marker / displacement fixtures) have no card: they are
the **many-markers wall** of the marker, LOD, displacement, orientation and search tests (~20 files name them by id; none needs a change).

### 1.2 Where one kind's test data lives today -- up to five places
1. The Lamp (each of its looks), 2. the Painting (18 kinds again), 3. a Lamp neighbour (gallery, quick_facts, rich_text, audio, video,
model_3d again), 4. `CardGalleryDefinitions.*.cs` (C#: every kind x look x short / long / partial / missing: 170 block + 40 header
cases), 5. hand-built blocks inside EditMode tests (`PaintingStory.cs`, builders in rule tests).
And tests reach a block by its **hard-coded key**: **64 test files** name `"block_N"` (`"block_56"` alone 34 times, `"block_2"` 142).
A block added, removed or reordered on the reference card means editing those files by hand -- the opposite of T2.

### 1.3 The reference set (the proposal)

| purpose | home | why there |
|---|---|---|
| **The golden card**: every kind ONCE, at its default look, real content; every single-card Phase B test | **`painting`, the Ribeira Palace** | what the developer hand-tests IS what the tests check; the visitor's demo card |
| Every LOOK x CONTENT of a kind (short / long / partial / missing, every variant) | the Phase A gallery (`CardGalleryDefinitions`), ONLY there | 1-2 s a case, no wall scene; `40-testing.md` 4.2 "tests stay on scope" |
| A **second real card** (switch / queue audio, compare_points' other side, a marker tap swapping cards, Previous / Next across points) | `lamp`, trimmed to St George's Castle's own real content | two real cards are a real case |
| The **edge cards** | the Lamp neighbours as they are: `lamp_military` (video-loop hero, "no X here"), `lamp_religious` (turntable hero), `lamp_residential` (no header, video siblings), `lamp_economic` (audio only), `lamp_infrastructure` (no card) | small and cheap; each named for its edge |
| The empty point | `camera` | Card Starter, "a card from scratch" |
| Generated fixtures | `night_hero.png`, `placeholder_hero.png`, `GalleryVideo/`, `ManualAudio/VideoOutput`, `ManualArWall`, `ManualPreviewStage` | unchanged |

**The one mechanism T2 needs: a reference-card helper** (one test-side class, e.g. `ReferenceCard`): `Golden` = "painting",
`BlockOf(kind[, look])` -> the key read from the shipped config, `SecondCard`, `Edge.VideoLoopHero`, ... A test asks "the Ribeira
Palace's gallery block", never `"block_56"`. Adding, moving or removing a block then edits the config only; the helper's own EditMode test
asserts the golden card holds every registered kind exactly once (so a new kind is caught the day it is registered).

### 1.4 What the Ribeira Palace must GAIN (one block each, default look, Ribeira content)
From the move map (S2, section 6), with the tests each unlocks: video (inline + chapters) ~11, place_in_ar ~10, model_3d (inline +
takeover) ~9, audio_guide `hero_chip` ~9, actions `sticky_cta` ~8 (the sticky action option), rich_text ~6, size_comparison ~5 (app kind),
story_chapters ~4, panorama_360 (drag + gyro) ~3, show_on_wall ~3, status ~3, feedback 3, poll 2, hotspot_image `loupes` 2, collect,
dialogue, compare_points, practical_info, pull_quote, process_steps, swatches 1 each; and **`show_after_viewed` ON on block_15** (its
true / false check: today only the Lamp's block_44 carries it -- the Show After Reading case would be lost otherwise).
About 20 blocks: the card grows from 18 to ~38. The content is real Ribeira content (`detail_card/content/text/PACO_RIBEIRA_text.md`
style): a developer content pass, not a test author's.

### 1.5 Cases that must NOT be lost in the move (each named, with its home after it)
- **A long card** (56 targets, 8 families): CompactRowRule "every kind builds", Expandable (every kind expandable), NavigationWarningRule,
  the accordion's 56 rows (PoiCardLayoutScene), the 8-section CardLayoutRule case -> the golden card at ~38 blocks / 8 groups covers them;
  re-pin the counts from the config (never re-typed).
- **Two cards at once**: audio switch / queue (Lamp + `lamp_economic`), compare_points rings (Lamp + `lamp_military`), mini-player
  reopening a card, KeepModelOnSwitch (a model kept across points), PageChrome's neighbours -> golden + the Lamp as second card.
- **Edge heroes** (4 pictures: parallax light, video loop, turntable, the night fixture): Hero words, ChromeMarks, Locate halo -> keep the
  neighbours.
- **Headerless card / no card / status unknown / Military gets no contents** -> the neighbours.
- **Show After Reading** (a waiting true / false) -> Ribeira block_15 with the flag (1.4).
- **The real clips** (`PoiCardRealAudioTests` the PT castle clip, `PoiCardRealVideoTests` the castle video) -> the Lamp's media files stay
  in `CardMedia/`; the golden card's video / audio blocks may point at the same files.
- **The marker identity rule** (DemoWallIdentityTests: 18 named points) and every marker / LOD / search test -> unchanged.

### 1.6 The move plan
Not a big bang: the helper and Ribeira's new blocks first (one chunk), then each component's tests move WHEN its block comes up ("the
story, then each block"): the block's own Phase B tests are rewritten onto `ReferenceCard.Golden`, its Lamp look-variant scene tests that
only repeat the gallery's matrix are deleted (GALLERY-COVERS), and the Lamp loses the blocks no test reads any more. EditMode rule tests
that only need "a shipped card with kind X" move in the same chunk as their kind. The tree is never half-moved: a kind is on the Lamp or
on the golden card, never read from both.

## 2. PART 2 -- old and useless tests (T3)

### 2.1 The 163 PlayMode skips (exact, from the run's skip messages)

| n | reason | fixtures | verdict |
|---|---|---|---|
| 129 | `TestSkips.OldNavigationElements` | CardNavGalleryTests (+ .Facets, .FullView) 57; RestingEnd 10; TopElements 9; Menu 9; NavContents 8; Facet 7; BottomElements 5; the split `*OldNavigationTests` of Hero 6, Painting 5, Chrome 4, Layout 4, Pages 3, SectionSheet 2 | **DELETE**: the kept elements' rebuild writes new tests on the bar's pattern |
| 20 | MEASURING-gated instruments | TypefaceMeasure (12 gallery + 4 scene), SectionSheetMeasure 2, CardLightMeasure 1, Story motion cost 1 | **KEEP** while a design block uses them (owner: the measuring block); drop TypefaceMeasure's NavigationElements case; SectionSheetMeasure and CardLightMeasure measure the **Cards** look: the developer says if they are still used |
| 13 | `[Explicit]` ThemePixelSet | gallery 12 (CardNavGalleryScene) + scene 1 | **KEEP** (owner: a block that must move no pixel); its NavigationElements case renders the kept elements: drop it |
| 1 | F4 | PoiCardTapZoomTests.TheLampsTrueFalseStatement_... | **KEEP** as an open finding (owner: the next card block); its gesture is the true / false card: move with it to block_15 |

EditMode: 31 skipped methods under the same reason -- whole files NavBottomElementRulesTests (16), BottomElementsNavViewTests (6),
CardMenuOverlayTests (7); the split classes of NavTopElementRulesTests (2); three EMPTY `*OldNavigationTests` skeletons (CardLayoutRule,
NavElementRegistry, SectionSheetMeasureScene). After the deletions `TestSkips.cs` has no user and goes. Skipped tests cost no run time;
they cost reading, upkeep and a misleading total (T3).
Not skips: the `#else` `Assert.Ignore("needs the Editor ...")` guards in SearchSceneFixture, LodWallSceneTests and the 12 gallery
fixtures never run in the Editor -- keep.

### 2.2 Superseded designs that still RUN
- **The Cards look** (container `blocks = Cards`; Open is the default since P3): SectionSheetSceneTests (11, 64 s), CardLightSceneTests
  (6, 59 s), ThemeFrameSceneTests (7, 30 s) each pin Cards in `[UnitySetUp]`. Cards is still a live option, so they stay -- as ONE
  component, "the Cards look", run only when that component changes. **SectionSheetSceneTests is a Core class**: CORE guards the old
  default and not the Open sections (3.3).
- **The sticky action** (`bar.sticky_action`, left out by Automatic under the default bottom bar): a live option of the Navigation Bar,
  yet forced `Shown` in five fixtures -- Hero (4 cases at Full + the hero action tap), PoiCardSceneTests (the full-card catalogue's footer
  check + About's sticky words EN / PT and its tap), CardGallery `actions_sticky_cta_long`, Locate (:645-663). Verdict: one fixture owns
  the option (the Navigation Bar's), the rule table stays in EditMode (`LocateFabRuleTests` 188-197 -> NavBarRuleTests), the rest drop the
  forcing. (S1 called About's sticky tap a duplicate of the hero's action tap: NOT held -- they tap two different buttons; keep one sticky
  tap, in the bar's fixture.)
- **The Family Pager with the bar off** (a KEPT old element): PoiCardShowAfterReadingSceneTests (2 tests force the bar off and tap the
  pager's chevrons), PoiCardLayoutScene `Tabs_WithTheBarOff_UsesTheFamilyPager`, PoiCardPagesSceneTests (the Pages layout's 8 tests turn
  the pager), CardLayoutRuleTests 64-87. These run a non-default state on a kept element: decide with the pager's rebuild (Pages / Tabs as
  layouts switched by the bar?) -- until then, Show After Reading's tabs / pages cases are the only reason they run.
- **Old element labels in the Editor tab**: DetailCardEditorTabTests 312-341 sweeps kind_chips / bar_sheet / drawer_tree labels and the
  Cards option: drop the old elements' part with 2.1.
- `under_title` (a pre-v5 bar value): read as Top -- an old-config guard, keep the EditMode rule (NavBarRuleTests:207) and the Editor tab
  test; the PlayMode copy (NavBarScene:871) is a duplicate.

### 2.3 Option matrices outside their component (`40-testing.md` 4.2)
- **Locate Button fixture sweeps the Navigation Bar**: :596 (bar at Top), :620 (Hide While Scrolling), :645-663 (Position x Sticky
  Action). Keep the two Locate interplays (rest above a Top bar, slide with Hide While Scrolling); the sticky matrix is the bar's and is
  already the EditMode table -> delete the PlayMode copy.
- `LocateFabRuleTests` 36-46 and 185-197 test NavBarRule (defaults, StickyActionShown) -> move to NavBarRuleTests.
- The Hero fixture sets `sticky_action` and `locate_button` (2.2); PoiCardSectionsScene :624 sets `bar.items` (small, needed for a
  group's Section Style: keep).
- DetailCardEditorTabTests' option sweep (2.2).

### 2.4 Duplicates (same assertion, same subject)
- The footer waits for the hero: CardGallery.About:349, PoiCardHeroScene:177, HeroRulesTests 233-251 (+ the skipped Chrome one) -> keep
  the EditMode rule + ONE render.
- The pinned footer = the actions block: CardGallery.About:344 and PoiCardSceneTests:406 -> keep the gallery's.
- The sticky matrix: Locate :645-663, LocateFabRuleTests 188-197, Expandable:197, LiveUpdate:160 -> the EditMode table.
- `under_title` -> Top: three places (2.2).
- "The default is Open": PoiCardSectionsScene:112, SectionStyleRuleTests:32, CardSectionsEditorTabTests:70 -> the rule + the Editor's
  drawing; the scene test keeps only what it renders.
- PoiCardChromeSceneTests 135-165: S1 said they pass "by accident" on a footer that is gone -- NOT held: they assert the bottom AREA
  (the bar since S4), as their comments say; only the names ("Footer") are stale -> rename.

## 3. PART 3 -- the component map (T1)

### 3.1 Where the time goes (last FULL, per test)
A wall-scene test costs **>= 2.6-4.5 s** whatever it asserts: `SearchSceneFixture` loads `LivingRoomScene` (26 POIs, markers, wall
session) in `[UnitySetUp]` for every test. A gallery case costs ~1.1-2.2 s. Renders (`GrabStill` + pixel contrast over light / dark x
stops x options) cost 5-60 s each. So the slow tests are **rendered option matrices** and **real-drag walks**, not the size of the card:

| fixture | s | tests | where it goes |
|---|---|---|---|
| CardGalleryTests | 664 | 304 | 170 block cases x ~1.8 s (308 s), 40 headers (92 s), 34 compact rows (88 s) |
| PoiCardNavBarSceneTests | 543 | 47 | RealDragsToEachGroupInTurn 4 x 56 s = **224 s**; EveryOptionValue 26 x 4.6 s; labels 7:1 4 x 14 s; Icon Colour 2 x 29 s |
| PoiCardSectionsSceneTests | 388 | 33 | EveryOptionValue 21 x 7 s; the Reveal's real drags 3 x 36 s |
| PoiCardLocateFabSceneTests | 310 | 25 | EveryStyleAndColour 2 x 64 s; every hero picture 8 x 7 s |
| PoiCardHeroSceneTests | 295 | 58 | EveryWordOfTheHero 32 x 4.6 s (4 heroes x EN / PT x light / dark x Half / Full) |
| PoiCardSceneTests | 251 | 56 | the floor: 4.5 s a test (each kind's Phase B interaction) |
| PoiCardPageChromeSceneTests | 133 | 28 | the top bar's titles 12 x 4 s |
| SearchSettingsFieldTests | 123 | 33 | the floor, one per search setting |
| PoiCardNavSceneTests | 106 | 18 | the floor + a spy walk 17 s |
| PoiCardBrightPageSceneTests | 100 | 4 | TheGround: 30 Start x Colour x mode renders = **86 s** |

### 3.2 The component map -- component -> its fixtures (today's s) -> its BLOCK selection

KEPT CURRENT (`40-testing.md` 4.2 points here). A block on a component runs, after the EditMode half (`40-testing.md` 4.2: the suite
without the Live assembly), its row's **selection** -- the fixtures in ONE `test_names` run (full class names: `TileStories.Tests.<X>`,
the app's `TileStories.LivingRoom.Tests.<X>`), its gallery cases in ONE `group_names` run (a regex over the FULL test name, parameters
included; proven on quick_facts: `CardGalleryTests\..*\("quick_facts` ran exactly its 5 looks + its compact row, 6 / 6) -- then
`category_names: ["CardSmoke"]` in a run of its own. Times are the last FULL's (s); a row whose component changes edits its fixtures here.

| component | its fixtures (s) | BLOCK selection | est. |
|---|---|---|---|
| Sheet: stops, drag, close, tap outside | PoiCardSceneTests' generic ~15 tests (~70), SheetDrag 17, FullHeight 20, TapZoom 41 | `test_names` PoiCardSceneTests, PoiCardSheetDragSceneTests, PoiCardFullHeightSceneTests, PoiCardTapZoomTests | ~5.5 min |
| Hero (the header block) | Hero 295, gallery headers 92, ChromeMarks' hero half ~50 | `test_names` PoiCardHeroSceneTests, PoiCardChromeMarksSceneTests; `group_names` `CardGalleryTests\.EveryHeader` (40) | ~8 min (~5 after F5) |
| Chrome: X, settings, top band, bottom area | Chrome 13, ChromeMarks 97, CardSettingsGallery 20, Theme 11 | `test_names` PoiCardChromeSceneTests, PoiCardChromeMarksSceneTests, CardSettingsGalleryTests, PoiCardThemeSceneTests | ~2.5 min |
| Navigation Bar + NavController | NavBar 543, Nav 106 | `test_names` PoiCardNavBarSceneTests, PoiCardNavSceneTests | ~11 min (~6.5 after F5) |
| Open sections (the default container) | Sections 388, BrightPage 100 | `test_names` PoiCardSectionsSceneTests, PoiCardBrightPageSceneTests | ~8 min (~5.5 after F5) |
| Cards look (the other container) | SectionSheet 64, CardLight 59, ThemeFrame 30 | `test_names` SectionSheetSceneTests, CardLightSceneTests, ThemeFrameSceneTests | ~2.5 min |
| Locate pin | Locate 310 | `test_names` PoiCardLocateFabSceneTests | ~5 min (~3.5 after F5) |
| **Story (the next block)** | Story 61, StoryGallery 93, StoryMediaGallery 65, PageChrome 133 | `test_names` PoiCardStorySceneTests, CardStoryGalleryTests, CardStoryMediaGalleryTests, PoiCardPageChromeSceneTests | ~6 min |
| Pages, full view, layouts | PageChrome 133, FullViewSiblings 30, Pages 55, Layout 16, ShowAfterReading 46 | `test_names` PoiCardPageChromeSceneTests, PoiCardFullViewSiblingsSceneTests, PoiCardPagesSceneTests, PoiCardLayoutSceneTests, PoiCardShowAfterReadingSceneTests | ~4.5 min |
| **A block kind** (rich_text, quick_facts, timeline, ...: the next blocks) | its gallery cases (~5-25 x 1.8 s) + its PoiCardSceneTests.<Family> tests on the golden card (~2-6 x 4.5 s) + its compact-row case | `group_names` `CardGalleryTests\..*\("<kind>` (its looks + its compact row) + its Phase B tests by name in `test_names` | **~1-2 min** |
| audio_guide | AudioScene 48, AudioGallery 31, RealAudio 4 + its gallery cases | `test_names` PoiCardAudioSceneTests, CardAudioGalleryTests, PoiCardRealAudioTests; `group_names` `CardGalleryTests\..*\("audio_guide` | ~1.5 min |
| video | VideoScene 32, VideoGallery 22, RealVideo 8 + cases | `test_names` PoiCardVideoSceneTests, CardVideoGalleryTests, PoiCardRealVideoTests; `group_names` `CardGalleryTests\..*\("video` | ~1.3 min |
| model_3d / panorama_360 | Model3D 59, ModelTurntableGallery 23, PreviewStage 7, PreviewWiring 12 / Panorama 20, PanoramaGallery 22 | `test_names` PoiCardModel3DSceneTests, ModelTurntableGalleryTests, CardPreviewStageTests, PoiCardPreviewWiringTests / PoiCardPanoramaSceneTests, PanoramaGalleryTests; + their `group_names` | ~1.7 / ~0.7 min |
| place_in_ar | PlaceInAr 60 + cases | `test_names` PoiCardPlaceInArSceneTests; `group_names` `CardGalleryTests\..*\("place_in_ar` | ~1.2 min |
| timeline (horizontal) / related | CardTrackGallery 18 + cases | `test_names` CardTrackGalleryTests; `group_names` `CardGalleryTests\..*\("(timeline|related)` | ~0.7 min |
| size_comparison (app) | SizeComparisonGallery 46, LivingRoomCardScene 22 | `test_names` TileStories.LivingRoom.Tests.SizeComparisonGalleryTests, TileStories.LivingRoom.Tests.LivingRoomCardSceneTests | ~1.2 min |
| Expandable / Show After Reading | Expandable 44 / ShowAfterReading 46 | `test_names` PoiCardExpandableSceneTests / PoiCardShowAfterReadingSceneTests | ~0.8 min each |
| Theme, type, language | ThemePipeline 8, ThemeType 10, Language 39, Portuguese 8 | `test_names` ThemePipelineTests, ThemeTypeTests, PoiCardLanguageSceneTests, PoiCardPortugueseTests | ~1 min |
| Live config (Play Mode push) | LiveUpdate 78 (+ the Live assembly's 5 fixtures, EditMode) | `test_names` PoiCardLiveUpdateTests; the EditMode half runs FULL (the Live assembly included) | ~1.3 min + ~2 min |
| Editor tabs | EditMode only (3.4) | the EditMode half; full EditMode when a row's live push changes | in EditMode |

The tripwire for a shared look no grep names is the **card smoke**: `[Category("CardSmoke")]` on ONE cheapest test method per row above
(the cheapest METHOD, all its cases summed -- a method's tag tags every case): 21 methods, 22 tests (CardTrackGallery's has 2 cases), measured
in section 3.6. So a block in the review phase is the EditMode half + its row's selection + the smoke, and CORE every 3rd-4th block still runs
every critical path. A component whose fixtures change keeps its smoke test current (its cheapest method, renamed with it).
Outside the card (unchanged, not in a card block's selection): markers, LOD, displacement, orientation, search (SearchSettingsField 123,
SearchDemo 41, SearchScene 33, Effects 62 + 10, LODControllerEvaluate 22, Orientation 20, the galleries < 1 s each).

### 3.3 CORE
CORE's PlayMode half is **~21.4 min** in the last FULL (229 tests; 17.0 when it was tiered). **9.0 min of it is one class tag:
PoiCardNavBarSceneTests** (all 47 tests, renders and the 224 s drag walk included). And CORE guards the **Cards** look
(SectionSheetSceneTests, class) while the Open sections (the default) have no Core test. Fix (chunk F2): NavBar's tag onto its 2-3
critical-path methods (the default bar, a real tap jumps, a real drag follows), SectionSheetScene's off, Sections' cheapest default test
on (a real jump lands a group's Start under the Top Band) -> CORE PlayMode ~13 min, true to the shipped card.

### 3.4 EditMode

This session's full EditMode run (HEAD 5801c6d, Unity behind both ends): **2240 tests, 2208 passed, 0 failed, 32 skipped, 5.2 min**
(218 fixtures; copied results: the session scratchpad's `full_em_1030.xml`; `fixture_times.csv` holds this run now, the FULL
PlayMode times are in section 3.1 and the run's XML). It is part of EVERY block, so it is half of T1's "few minutes":
- **15 fixtures hold 250 of the 309 s**; the other 203 fixtures run in ~1 min together.
- **The six `LivePlayMode*` fixtures enter Play Mode: 113 s** (LivePlayModeCardTests 32 -- one test 27 s --, StoryEditor 30,
  NavigationBar 20, CardLocate 14, CardSections 14, + LodZoomLiveSync 5). They guard the Editor -> running card push: a card block that
  touches no Editor or live-sync code gains nothing from them. MCP cannot EXCLUDE a category (4.2), so the lever is an assembly of their own
  (`TileStories.Editor.Tests.Live`): a block runs `assembly_names` without it, CORE and FULL with it. Saves ~2 min per block.
- **A pure rule at 43 s**: NavBarShapeRuleTests' `RaisedDepth_RisesABell_...` alone takes 33 s and `TheBell_IsAsDeepAsTheDisc_...` 7 s
  -- a pure path rule, so the time is the sampling density (not read yet): the cheapest proof samples the depths and angles the asserts
  name. ~35 s.
- Real-window tests (DetailCardEditorTab 17, BlockTemplateEditorTab 17, StoryEditorTab 15, EditorPopup 8, DrawingNeverWrites 7): fine.
- 32 skips: the 31 old-navigation methods (2.1) + TestRunReportTests' recorder-off case.
With F1 (31 fewer), the Live assembly and the NavBarShape sampling, a card block's EditMode half is **~2.5 min**.

### 3.5 Slow fixtures -- the cheapest proof that keeps each guarantee
| test (s) | guarantee | cheapest proof | saves |
|---|---|---|---|
| NavBar RealDragsToEachGroupInTurn (224) | the bump and disc follow the chosen group under a real finger | ONE real drag across two groups + the jump-to-each-group check that already exists (a tap per group); light / dark does not change motion | ~180 s |
| Locate EveryStyleAndColour (128) | every Style x Colour reads (rim 4.5:1, label 7:1) | the colours' ratios are already the EditMode rule (`LocateFabRuleTests`: every Colour's rim on the paper, the chip 7:1, light / dark); render ONE case per Style | ~100 s |
| BrightPage TheGround (86) | past Tint Depth the ground is the paper for every Start x Colour | read it from the renders Sections' StartTimesColour already takes (34 s): one walk, two asserts | ~80 s |
| NavBar labels (57) + Icon Colour (58) | words 7:1, icons 4.5:1 on every fill | one render per fill and mode measures both | ~50 s |
| Hero EveryWordOfTheHero (148) | every word 4.5:1 round its letters | the words sit on the PICTURE: if light / dark does not change the hero words' ink and halo (to verify in the chunk), the mode axis goes | ~70 s if held |
| the scene load per test (~2.5 s x ~700 wall tests) | a clean card per test | load once per fixture and reset the card + visitor state between tests (Test_Suite_1 chunk 1, never done) -- SHARED GROUND, measure first | up to ~25 min of FULL |

Never dropped: the regression of a real past bug, the contract guards (tokens, fields, render targets, contrast as seen).

### 3.6 The times after F1 + F2 + F4 + F11 (M2, 2026-10-30, measured, Unity behind unless said)
- **A block's EditMode half** (`assembly_names` without Live): **2200 tests, 2199 passed, 0 failed, 1 skipped, 2.0 min** (was the full
  suite, 5.2 min). Full EditMode (CORE / FULL, Live included): 2211 / 2210 / 0 / 1, 3.4 min (that run ended with Unity in front).
- F1: full EditMode right after the deletions 2209 / 2208 / 0 / 1 (was 2240 / 2208 / 0 / 32): the 31 old-navigation skips gone, every
  passing test still there.
- F11: the Live assembly's 5 fixtures 103.8 s (Story Editor 31.2, Card 27.6, Navigation Bar 21.8, Locate 13.4, Sections 9.8).
  NavBarShapeRuleTests **41.2 s -> 0.2 s**, every sample and assert unchanged: the cause was not the sampling but `Room`, `H`, `Half`,
  `Disc`, `Corner` as properties that re-read CardTokens.uss and ran a regex on every access (0.338 ms a read, measured), inside
  per-sample lambdas -- ~98 000 reads in RaisedDepth alone. They are read once in `[OneTimeSetUp]`.
- F4: `group_names` `CardGalleryTests\..*\("quick_facts` ran exactly its 6 cases (5 looks + the compact row), 6 / 6, 9.7 s.
  `category_names: ["CardSmoke"]`: 22 / 22, **1.4 min** (82.1 s) -- the review phase's tripwire, was all 17 card fixtures (~38 min).
- F2: **CORE 12.2 min** -- PlayMode 175 / 174 / 0 failed / 1 skipped (the story measurement), 12.0 min; EditMode 120 / 120, 14 s
  (was ~21.4 min computed from the FULL, 229 PlayMode tests). The NavBar's 2 methods, Sections' default and no SectionSheet test
  confirmed by name in the run. The list: `_Test_Suite_1.md` section 3.

## 4. FIX list -- block-sized chunks

**Done (M2, 2026-10-30): F1, F2, F4, F11, and F10's two deletions** (SectionSheetMeasureSceneTests, CardLightMeasureSceneTests) -- section
3.6. Not held from the audit: DetailCardEditorTabTests 312-314 are no old-element sweep but the reflection walk's edits of the live
`top` / `bottom` / `menu` config fields (every card_settings field undoes / redoes / survives Save): kept. Next: F3 + F6 (M3, the golden card).

Each chunk: its target(s), its effect (minutes saved / places-to-edit removed / tests deleted), its tier. Recommended order: F1, F2, F4
(they make every later block cheap), F3 before the first moved kind, then F7 with each block; F5 / F6 any time; F8, F9 later.

| # | chunk | targets | effect | tier |
|---|---|---|---|---|
| F1 | **Retire the old-navigation tests**: delete 2.1's 129 PlayMode + 31 EditMode skipped tests (6 whole PlayMode files, CardNavGalleryTests + .Facets + .FullView, 3 whole EditMode files, the 7 split `*OldNavigationTests` classes, 3 empty skeletons), `TestSkips.cs`, the NavigationElements cases of TypefaceMeasure and ThemePixelSet, DetailCardEditorTab's old-element sweep; structure map + `_Test_Suite_1.md` row | T3 | -160 skipped tests, ~16 files; 0 run minutes (skips cost none); the suite's totals tell the truth | BLOCK (EditMode + a compile; the PlayMode deletions run nothing) |
| F2 | **Re-aim CORE**: NavBar's class tag onto 2-3 methods, SectionSheetScene's off, the Open sections' cheapest default test on; `_Test_Suite_1.md` section 3 rewritten as current truth | T1 | CORE PlayMode **21.4 -> ~13 min**; CORE guards the shipped default | CORE once |
| F3 | **The reference card**: the `ReferenceCard` helper (+ its EditMode test: every registered kind exactly once on the golden card); the Ribeira Palace gains the ~20 blocks of 1.4 and block_15's Show After Reading (a developer content pass) | T2 | one place for test data; the Ribeira-reading fixtures (NavBar, Sections, Locate, BrightPage, Story, PageChrome, NavBarRule ...) re-run on the longer card | BLOCK + those fixtures |
| F4 | **The per-component BLOCK selection**: prove `group_names` on one kind's gallery cases; tag a `CardSmoke` category (one cheapest test per card component); `40-testing.md` 4.2: the review-phase line becomes "full EditMode + the component's selection (3.2) + CardSmoke"; RUN TIMES | T1 | a kind block **38 min -> ~EditMode + 3 min** | BLOCK + one CORE to confirm |
| F5 | **Slow-fixture proofs** (3.5 rows 1-4; row 5 after measuring) | T1 | ~7 min off FULL; NavBar -4.5, Locate -1.7, Sections -1.3 min of their BLOCK | those fixtures |
| F6 | **Scope and duplicates** (2.2-2.4): the sticky action owned by the bar's fixture, the Locate sweep's bar matrix deleted, NavBarRule tests moved out of LocateFabRuleTests, the under_title runtime copy, the footer / default duplicates, Chrome names | T2, T3 | ~10 tests deleted, 3 rule tests moved, ~1.5 min | the touched fixtures |
| F7 | **Move a component onto the golden card** -- one per coming block (the story first: already Ribeira's, only the helper; then each kind): its Phase B tests through `ReferenceCard`, its Lamp look-variant scene tests that repeat the gallery deleted (S2's GALLERY-COVERS: quick_facts / fun_fact / header / rich_text looks, ThemeFrame's every-block frame), its EditMode rule tests with it | T2, T3 | per kind: 1 place to edit; a few tests deleted | that block's BLOCK |
| F8 | **Trim the Lamp** to the second-card blocks once no test reads the rest | T2, T3 | the second home of every kind gone | BLOCK + CORE |
| F9 | **The wall scene once per fixture** (3.5 last row): measure one fixture's set-up share first; reset the card and visitor state between tests | T1 | up to ~25 min of FULL, ~2.5 s off every wall test of every BLOCK | SHARED GROUND: CORE |
| F10 | **The Cards look and the measurements**: the Cards-look fixtures named one component (out of CORE, F2); the developer says whether SectionSheetMeasure / CardLightMeasure are still used (delete if not; RestingEnd's goes with its file in F1) | T3 | up to 2 instruments deleted | none |
| F11 | **The EditMode half** (3.4): the six LivePlayMode* fixtures into their own assembly (a block runs the rest by `assembly_names`; CORE / FULL all); NavBarShapeRuleTests sampled where its asserts look | T1 | EditMode per block **5.2 -> ~2.5 min** (with F1) | EditMode twice (with and without the assembly) |

## 5. Not verified in this audit
- `group_names` (NUnit's regex on full test names) selecting one kind's gallery cases: exists in the MCP `run_tests` schema, not yet run.
- The Hero words' mode independence (3.5): a hypothesis to measure, not a finding.
- Why NavBarShapeRuleTests' two tests take 40 s (3.4): the test bodies were not read; "sampling density" is the likely cause.
- CORE's ~21.4 min is computed from the FULL run's Core-tagged tests, not a CORE run; RUN TIMES keeps CORE's last measured ~17.5.
- S2's per-file test counts are +-1 (inferred from names and sampled bodies); the move list per file (section 6) is a guide for each
  chunk, re-read on disk when the chunk starts.

## 6. Subagents
- S1 (Sonnet, medium): every skip by reason, superseded designs, out-of-scope matrices, duplicates. Held: the skip map (20 skip sites,
  re-grepped), the Cards pins and SectionSheet's Core tag (re-read), the Locate sweep. Not held: two "duplicates" (2.2, 2.4).
- S2 (Sonnet, medium): the Lamp -> Ribeira move map (92 files; one row each: needs, has it, home). Held: the kinds Ribeira lacks
  (re-derived from the config), block_15's missing Show After Reading flag (re-read: S2 had it "unknown").
