# Plan tracker -- C1: the 16 CORE PlayMode reds, then CORE and FULL green

Brief: `proj_guides/__AI_worker.md` "C1" (the R1 tracker's HANDOFF 2026-10-30; `40-testing.md` 4.2). Worker: Opus, effort High.
TEST PLAN: the 16 by name after each fix group; CORE (PlayMode + EditMode) green; ONE FULL in the background. Closes the card review's
deferred CORE (P1 to R1, 8 blocks). OUT OF SCOPE: the story block's redesign, any new feature. No baseline stash.

## GATE
- [x] MCP live (TileStories@d41388e4, telemetry_status ok, idle, Play Mode off); HEAD = origin/master = d82c572 (R1 committed as f5d1542), nothing to push
- [x] SDF fonts unmodified (git: no change under the Theme / Markers font folders); no InitTestScene leftovers
- [x] no opening test run
- [x] compile clean (refresh force: ExitCode 0, console 0 errors)
- [x] Unity c asked once (Pronto) before the first run

## Leads verified on disk (before any run)
- Video x6 / Audio x2 / Story Open Row: taps through TapInView or a bare ScrollTo + Tap -- the viewport only; since S4a the Navigation Bar
  and the Locate pin float over the stack's foot, where ScrollTo leaves a target. LocateFabView.Layer is PickingMode.Ignore (on paper)
- Story margin 0: open sections never frame a slot (BlockStackView ~465); the composed clip bleeds (-24) but its rows get their margin
  back only under .card-block-slot--framed (Components.uss ~577) -> the words likely touch the screen edge
- Expandable / LiveUpdate: NavBarRule.StickyActionShown false under the bottom bar (Automatic) -> the sticky CTA block is skipped
- ThemePipeline:78: HeaderSlot paints --ts-clear since P2; the Navigation Bar paints --ts-bar / --ts-hair in C# (NavBarView.Paint);
  --ts-bar-glass only in Theme.default.asset (both modes) + the generated USS

## Steps
- [x] 1 diagnostic (16 / 16 red reproduced; every missed tap picked nav-bar-item-N: the floating Navigation Bar, never the Locate layer): CardTestInput.Tap logs what panel.Pick finds; run the 16 by name; read what lies under each missed tap; remove it
- [x] 2 PART 1 taps (TapInReach / IntoReach in Video, Audio, Story): test fix (TapInReach) where the bar / pin covers the target; a code fix + one real-tap PlayMode test if a clear layer eats it
- [x] 3 PART 1 Story (code: open rows take the gutter back + the cut fades into --ts-sheet-open; TestEvidence/Card/C1) margin: LOOK (Painting preview, light / dark) -> Components.uss open rule -> one title-margin assert -> LOOK again
- [x] 4 PART 2: Expandable (precondition + counts 1 / 1), LiveUpdate (minus the footer blocks StickyActionShown leaves out)
- [x] 5 PART 3: ThemePipeline (title bar --ts-clear, the Navigation Bar's --ts-bar / --ts-hair); delete --ts-bar-glass (asset, both modes) + regenerate USS
- [x] 6 the 16 by name green
- [x] 7 CORE PlayMode + EditMode green (PlayMode 230: 4 red my TapInReach change caused in the Story preview -> Preview() IntoReach, the fixture 10 / 10 + 1 skipped; EditMode 120 / 120)
- [x] 8 FULL in the background (Unity c): PlayMode 1295, 1082 passed, 50 failed, 163 skipped (77.4 min); none in CORE; not fixed (the session budget: no new code sub-step after a compaction) -> HANDOFF
- [x] 9 docs (FULL EditMode 2240: 2208 / 0 failed / 32 skipped, 7.0 min; RUN TIMES EditMode ~7 min): _3.3 build log (CORE / FULL counts), _Test_Suite_1 if tests moved, 10-structure if files changed; report; commit message; AskUserQuestion

## Not deleted, reported: theme tokens with no runtime reader by name (scan): --ts-bar-pill, --ts-focus, --ts-glass-opacity, --ts-hero-fade, --ts-weight-body

## The 16
PoiCardAudioSceneTests: ARealTapOnTheHerosListenAtFull_..., InPortuguese_TheChipThePlayerAndTheMiniPlayerSpeakPortuguese
PoiCardExpandableSceneTests: TheLamp_AllExpandable_HoldsNoMediaUntilARealTapOpensARow_...
PoiCardLiveUpdateTests: BlockOrder_TheLampsCardRendersInAllThreeOrders_AndTheOptionIsLive
PoiCardStorySceneTests: ThePaintingsStoryPreview_... x4, ThePreviewsTapNothing_OpensOnlyFromItsOpenRow_...
PoiCardVideoSceneTests: AVideoTappedWhileAnotherPoints..., ClosingTheCard_StopsTheVideo..., InPortuguese_TheVideosSpeakPortuguese...,
  TheAudioGuidePauses_WhenARealTapStartsTheVideo..., TheChaptersVideo_IsATeaser..., TheLamp_InlineVideo_FromTheWallsFolder...
ThemePipelineTests: TheCardsRoots_DeclareOnlyTheirOwnTokens_AndPaintWithTheThemesByInheritance

## HANDOFF (2026-10-30): the FULL PlayMode run's 50 reds (none in CORE; results XML copied to the session scratchpad, the fixture times in TestEvidence/Harness)
- TEST, small, ours (the C1 pattern -- a tap under the floating Navigation Bar; use CardTestInput.TapInReach / IntoReach):
  PoiCardNavSceneTests.Hotspot.cs:97 + :129 (TapInView on a pin / loupe), PoiCardNavSceneTests.OpenBlockAndBack.cs:132 (TapInView),
  PoiCardTapZoomTests.cs:427-436 x2 (bare ScrollTo + Tap on the story's Open Row; read the offset AFTER IntoReach),
  PoiCardPlaceInArSceneTests.cs:443-447 (bare ScrollTo + Tap on the button), CardVideoGalleryTests.cs:289-294 (the gallery harness's
  TapInView on Full Screen: check whether the gallery card floats a bar)
- CardGalleryTests.cs:161 x40 (every header entry): the bar's title contrast 1.12 vs 4.5. The bar is --ts-clear since P4.4b and
  EffectiveBackground(title) finds no ground of the bar's own; decide what the title is measured over (the sheet's glass over the
  darkest / lightest backdrop, as CardSettingsGalleryTests does with GroundOver) -- the test's aim, unless the render shows the title unread
- CardSettingsGalleryTests.cs:124 ("dark"): the settings mark on its glass disc over a WHITE backdrop reads 2.76 (needs 7, D1); the bar
  under it is clear now (P2 / P4.4b). A design trade-off: a denser disc glass in dark, or measure over the backdrops the card really shows
- ThemeTypeTests.cs:58: the hero's halo labels (card-hero__halo, aaa2c1f) draw "<face> UITK Halo" assets (d3df243) and the title's halo
  draws the display face on a body-classed copy; the test should accept a face's Halo variant and inherit the role of the label it
  haloes -- the test's aim
- CardGalleryTests.About.cs:374: PullQuote minimal (17) is no smaller than serif (17): --ts-el-quote-* vs the body size moved; check
  which the look means (code if serif must stand larger)
