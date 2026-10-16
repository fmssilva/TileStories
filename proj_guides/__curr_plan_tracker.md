# Plan tracker -- _3.2 step 5b: CardChromeRule and the two regressions of 5a

GATE
- [x] git push: 1 commit pushed (4d8cf85..09a2d6b)
- [x] captures: TestEvidence/Gate/gate_3_2_step5b_game.png, gate_3_2_step5b_poi_editor.png (Detail Card > Navigation), opened
- [x] compile: ExitCode 0, no error CS; SDF fonts clean (git status of TileStories/ empty)
- [x] full EditMode baseline 1720/1720 (96 s, background)
- [x] PART 1 reproduced on the DEFAULT config (Top Bar None lines removed = 5b.4 edit done first):
      Model3D InlineRoomScan FAILS (viewport 0 at half), KeepAndInstant FAILS (range 0, viewport 0),
      ABlockAsksTheHost + ARealFingerDrag pass alone. Brief slip: the host-jump test never ran with None.

MEASURED (Lamp, 390 x 844, probe): Half stop 337.6, handle 24 + header 206 -> header bottom 231 -> room 107;
strip 49, footer 81 -> viewport 0. Full: room 565, viewport 435, content 13669. Heading 22 + gap 8, body line 20.
FLOOR = 72 (heading + 2 lines = 70, on the 8 px grid). Half -> neither chrome, 107 px of content.

STEPS
- [x] 5b.1 CardChromeRule (Runtime/Blocks/Navigation): ScrollState.Next (24 / 8 travel, top / end), Decide (peek, full_only, floor bottom-first)
- [x] 5b.1 data: CardNavigationSettings.show_at / hide_on_scroll + CardOptions (default first) + labels
- [x] 5b.1 Tier 0 CardChromeRuleTests
- [x] 5b.3 NavOntologyRule.OffersASectionChoice + NavElementDefinition.BetweenSections + NavSlotRule.TopElement reads it; ShownAt deleted
- [x] 5b.3 tests: NavOntologyRuleTests (predicate + ShowsNavigation unchanged on one family, in words), NavElementRegistryTests updated
- [x] 5b.2 CardChromeView (Runtime/UI/Cards/Navigation) + USS hidden classes + sheet wiring (replaces Slots.ShowAt)
- [x] 5b.4 the two tests on the default config green (+ host-jump + finger drag): 4/4 after the fix (EditMode 1738/1738)
- [x] Phase B PoiCardChromeSceneTests (half reading space, drag down hides / up shows, peek, content share down vs never, full_only live)
- [x] PoiCardTextTabsSceneTests adapted (strip hidden at Half on the Lamp now)
- [x] Phase A: case two_families; one_family renders no strip, two_families does
- [x] 5b.5 Editor rows Show Navigation At + Hide While Scrolling, (i), help, guides; DetailCardEditorTabTests + walker 12 + round trip
- [x] captures: Lamp half default, Lamp full scrolled (footer hidden), Editor section; judged vs 5a
- [x] closing: full EditMode + PlayMode fixtures with Card / Poi in the name
- [x] 5b.6 docs: _3.2 status + TODO lines, _5.1 rows, 10-structure, __mixed_TODOs (the two unreproduced failures; Follow Scroll row)

RESULT: CardChromeRuleTests 13/13; PART 1 4/4 on the default config; PoiCardChromeSceneTests 6/6; Card / Poi PlayMode 553/553 (2nd run;
1st 552 + 1 fixed test); full EditMode 1739/1739. Captures judged. Docs written.
