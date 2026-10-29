# Plan tracker -- P3e (continued): the locate pin at the chosen disc's size, floating over the card; the 17 reds green; commit point

Brief: `proj_guides/__AI_worker.md` "P3e (continued)" (`_3.3` fourth round > "P3d review" b; the previous tracker's HANDOFF). Worker: Opus,
effort High. TEST PLAN: BLOCK = full EditMode + the 16 card fixtures (list at the end) on the FINAL state. OUT OF SCOPE: retiring the old
navigation, the story block.

## GATE
- [x] MCP live (TileStories@d41388e4, telemetry_status ok); origin/master = HEAD = f772723 (the Architect's brief), nothing to push
- [x] SDF fonts: UITK 6 / 1024, UITK Halo 18 / 2048, TMP 6 / 1024, all static (population mode 0); no InitTestScene leftovers
- [x] Play Mode was ON (transition) at the start -> off before any run; forced compile ExitCode 0, zero error CS, test dlls rebuilt
- [x] baseline full EditMode (Unity c): 2376 run, 1 failed (LivePlayModeCardLocateTests: the Painting's chip at Half, the old 1.35 pin); started behind, ended in front
- [x] captures: Game view look/ (Unity b) + the Editor rows on the dark skin (TestEvidence/Editor/p3e_locate_rows.png)

## Decisions (autonomous)
- WHERE: the pin's own LAYER in the card layer, right after the sheet (`poi-card-locate`, ts-card + ts-theme-default classes so the tokens
  resolve): drawn after the sheet, under the floating slot, the menu layer and the full view (child order = draw order). The sheet is
  left 0 / right 0 / bottom 0 in the layer, so the rule's "above the sheet's foot" numbers carry over unchanged. Rejected: FloatingSlot
  (it rests ABOVE the sheet's top edge and shows only while it holds something); staying inside the sheet (clipped by it, and the brief
  asks for the floating layer)
- The words' room is REMOVED: LocateFabRule.WordsClear, HeaderBlockView.SetWordsClear / ClearRow / BesideTolerance, PlaceLocate's room code.
  The chip keeps LabelFits (shown only clear of the hero words' ink, else the pin alone)
- Size: SizeDefault 1 (40 pt = the chosen disc), SizeMin 0.6, SizeMax 2; the shipped wall's 1.35 (the old default, never a pick) -> 1
- Touch holds the whole float box: LocateFabRule.ShadowWidth / ShadowHeight / Lift / Standing (pure) -> Touch = max(44, standing height);
  the view lays out from the same numbers. Transparent (clear background) as before
- Contrast over a picture: the sheet tells the pin when it lies on the hero (like the bar's SetOverHero); over a picture the pin draws a
  dark HALO (theme token --ts-locate-halo, light / dark) round its body; measured until the halo's edge vs the picture >= 3:1 on every hero
- Hero tests that turned the button off because it narrowed the words: TheTitleFitRule test keeps it ON now (proves "never narrows")

## PART 1 -- size and place
- [x] 1a CardLocateSettings range / default; wall config 1.35 -> 1 (both copies); Editor help text
- [x] 1b LocateFabRule: drop WordsClear; Standing / Touch from the float box
- [x] 1c LocateFabView: own layer after the sheet (PoiCardSheetView), lay out from the rule, instant class on its own root; hidden with the sheet
- [x] 1d drop SetWordsClear and PlaceLocate's room code
- [x] compile; LOOK (TestEvidence/Premium/P3e/look/): Lamp / Painting Half + Full light, Painting dark on paper (an accepted overlap with a block),
      military Full dark, Size 2 (pin alone: the chip gives way to the subtitle) and 0.6 (18 x 24 in 44), float 0 / 3.5 / 7 / 3.5 pt; accepted

## PART 2 -- contrast over a picture
- [x] measured (look/m/contrast.txt): edge (glow / drop) 1.00-1.89, disc 1.01-14.30; fails: religious light 1.89 / 1.81, Painting Full 2.48 / 2.52
- [x] --ts-locate-halo (black 0.8, both modes) + OverPicture halo (3 pt core + soft fall); USS regenerated; look/m2/contrast.txt: worst 3.54:1
      (Painting Full light), religious 5.45 / 4.18; halo off on the paper; LOOK accepted

## PART 3 -- tests and the reds
- [x] EditMode: LocateFabRuleTests (sizes, touch holds the box, no words' room), CardLocateEditorTabTests (default 1, range)
- [x] PlayMode LocateFab (25 tests): default 1x, its layer after the sheet, the words' rows untouched (same as with the button off), every size inside its
      target (0.6 / 2 x pin / glass), the Painting's label room, over-hero ring >= 3:1 every hero light / dark
- [x] Hero: TheHalo_IsTwoLayers expects the halo twin; the 8 words' rows reds re-run
- [x] BLOCK (final tree, Unity behind): full EditMode 2368 / 2216 / 0 failed / 152 skipped; 16 card fixtures 306 / 306 (35.2 min).
      On the way: ReachableStack stops above the shown button (NavBar strokes began on the chip); the halo and the chip's room
      re-judged on scroll / layout; the turntable hero measured still only round the pin

## PART 4 -- [x] Editor rows dark-skin capture; [x] docs (_3.3 log, _Test_Suite_1, 10-structure, 40-testing RUN TIMES); report + commit message; AskUserQuestion

## The 16 card PlayMode fixtures of BLOCK
PoiCardSectionsSceneTests, PoiCardNavBarSceneTests, PoiCardHeroSceneTests, PoiCardChromeMarksSceneTests, PoiCardSceneTests, CardLightSceneTests,
ThemeFrameSceneTests, SectionSheetSceneTests, PoiCardLayoutSceneTests, PoiCardPagesSceneTests, PoiCardPaintingSceneTests, PoiCardThemeSceneTests,
PoiCardChromeSceneTests, PoiCardPageChromeSceneTests, PoiCardLocateFabSceneTests, PoiCardBrightPageSceneTests
+ EditMode-hosted live: LivePlayModeNavigationBarTests, LivePlayModeCardSectionsTests, LivePlayModeCardLocateTests (inside full EditMode)
