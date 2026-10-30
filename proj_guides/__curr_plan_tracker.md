# Plan tracker -- C2: the 50 FULL PlayMode reds, then FULL green

Brief: `proj_guides/__AI_worker.md` "C2" (the C1 tracker's HANDOFF; `40-testing.md` 4.2). Worker: Opus, effort High.
TEST PLAN: each group by name after its fix; then ONE FULL in the background (PlayMode + EditMode) green -- closes the card review's FULL.
OUT OF SCOPE: the story block's redesign, new features.

## GATE
- [x] MCP live (TileStories@d41388e4, telemetry_status ok, idle, Play Mode off); HEAD = origin/master = e1bcd1f (C1 committed as c57c71d), nothing to push
- [x] Unity restarted (Editor.log 105 KB; the 3.86 GB one is Editor-prev.log); SDF fonts unchanged (last touched d3df243, no local diff); no InitTestScene leftovers
- [x] compile clean (refresh force + compile: ExitCode 0, 0 error CS); no opening test run
- [x] Unity c asked once (Pronto) before the first run

## The 50 (C1's full_playmode.xml, grouped by the Haiku subagent; = the HANDOFF list)
40 CardGalleryTests.EveryHeaderEntry (bar title 1.12 vs 4.5) | 1 PullQuote 17 = 17 | 1 CardSettingsGallery dark 2.76 | 1 CardVideoGallery FullScreen |
3 PoiCardNavSceneTests (Hotspot x2, OpenBlockAndBack) | 1 PlaceInAr | 2 TapZoom story Back | 1 ThemeType (Halo assets)

## Steps
- [x] 1 PART 1 taps: every card-STACK caller of TapInView / bare ScrollTo+tap -> TapInReach / IntoReach (into reach, read, tap), 15 files;
  TapInView kept only for takeover pages (PageScroll x3, nothing floats there), its comment says so
- [x] 2 PART 2 header title: LOOKED (TestEvidence/Card/C2: light / black Peek + Half plate -- the chrome row on opaque paper, all reads);
  the test measures as SEEN (ThemePixelSet.SeenOverBackdrops: render, inks cleared, RingContrast, black + white camera): the bar title only
  while shown (opacity 0 otherwise), the plate's line in VisibleStack, the X off the hero at 7:1; no code fix needed
- [x] 3 PART 3 chrome discs (X AND settings: one AddMark treatment): LOOKED -- dark over a WHITE camera the marks read 15.79:1 at Peek,
  12.36:1 at Full (the sheet's top / the Top Band's painted fade lie under the disc, never the camera); over the hero the dark hero glass.
  NO design change (a denser token moves no pixel that matters); the settings test measures as SEEN at D1's 7:1 -- REPORT this deviation
- [x] 4 PART 4 ThemeType: a halo copy (card-hero__halo) may draw its face's "UITK Halo" twin; every other text the plain asset
- [x] 5 PART 5 PullQuote: LOOKED (both 17 before); About.uss 4-class serif rule in open sections -> serif 22 / minimal 17, light + dark captures
- [x] 6 PART 6 the five tokens: no var(), no C# lookup; --ts-focus only a ThemeContrastTests guard row -> all five deleted (shared, both modes,
  8 accent knob sets), USS regenerated (14 lines), the guard row dropped; _3.3 token table to update in docs
- [x] 7 the 16 fixtures (509): 5 red MINE (PageChrome Back x4: scroll read before TapInReach; a header render never still: hero pan) -> fixed, 44 / 44; EditMode FULL 2240: 2208 / 0 / 32 (4.6 min)
- [x] 8 FULL PlayMode (Unity behind both ends) 1295: 1132 passed, 0 failed, 163 skipped (75.9 min)
- [x] 9 docs (_3.3 build log + FULL counts, _Test_Suite_1 if tests moved, 40-testing / 10-structure if needed); report; commit msg; AskUserQuestion
