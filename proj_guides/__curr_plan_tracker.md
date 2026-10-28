# Plan tracker -- Premium P1: the top of the card (hero contrast, the v5 chrome, F1 / F2 / F3 / F5)

Brief: `__AI_worker.md` (Premium P1). Spec: `_3.3` "S4 review, third round" (Look, Chrome), R11; `_3.3.1_Prototype/Premium_v5.dc.html`;
`_Test_Suite_1.md` section 2. Baseline: HEAD a11c3ad green (CORE 0 failed; FULL PM 1339 / 0 / 67 skipped; EM 2298 / 0). Do not commit.

## GATE
- [x] branch 0 ahead of origin (nothing to push); SDF fonts unmodified; IPCE/ left alone
- [x] compile clean (Editor.log: 0 error CS, Tundra success, ExitCode 0)
- [x] Unity's place asked (40-testing 4.2.3 c) before the first PlayMode run

## Decisions (autonomous)
- v5 layout: X left, title centred, settings right (Menu button left of settings); stops unchanged (--ts-bar-top / --ts-bar-height);
  a 44 pt touch target holds the 36 pt disc
- over the hero the disc is DARK glass (white marks >= 7:1 over a white sky needs a dark ground in linear blending; no blur in UITK);
  after the hero v5's light glass + hairline, ink marks
- the words shade = hero_scrim.png ramp + the --ts-scrim-hero floor (0.94, measured vs pure white), in the hero's foot; hero_glow.png
  removed; top shade 150 px (v5)
- marks = VectorGlyph Shape.Close / Shape.Sliders, stroke 1.5 on a 16 mark
- "bare marks": no existing chrome setting fits -> noted for P2's Look component
- the bar title keeps its pill (no blur), centred

## Steps
1. [x] Diagnose F3 + F5 with runs (un-ignored, red expected): confirm root causes
2. [x] PART 1 hero shades: HeaderBlockView Shade (ramp + floor), Media.uss, CardTokens (fade / reach), top shade 150; delete
       hero_glow.png; un-ignore the 16 hero-word tests; the glow test -> the shades test; CardVideoGalleryTests Glow -> Shade
3. [x] PART 2 chrome: CardIcons Close + Sliders; PoiCardSheetView marks (button > disc > glyph); PoiCard.uss (v5 layout, glass on /
       after the hero); theme tokens --ts-chrome-glass, --ts-chrome-glass-hero, --ts-chrome-line-hero (both modes) + regenerate USS;
       About.uss title centred; tests: CardSettingsGallery F2 (un-ignored, 7:1 on the composite over black AND white), the X tests
       (CardGalleryTests, PoiCardSceneTests), Menu / title-end positions, the marks' 7:1 on the REAL render at every stop x mode x
       darkest / lightest hero (ring helper shared with the hero words test)
4. [x] F5: dial pill background under :hover / :active; the dial's words asserted at 7:1; Assert.Ignore removed
5. [x] PART 3 F1: FloatingSlotRule.Bottom(+ chromeBottom): under the chrome row when it cannot rest above the sheet; FloatingSlot passes
       the X's foot; EditMode rule tests (sweep: never over the chrome row); PlayMode: panel.Pick at X / settings centres = that mark at
       Peek / Half / Full with a model placed; un-ignore the two F1 tests; Full-flush assertions re-based
6. [x] PART 4 F3: fix at the source; un-ignore
7. [x] Renders: real Game view, light + dark, Peek / Half / Full, darkest + lightest hero, chip at Full, after the hero ->
       TestEvidence/Premium/P1/; looked at, item by item
8. [x] BLOCK (narrowed by the developer: full EditMode 2301 / 0 failed; PlayMode hero + chrome marks + Navigation Bar + story 115 / 0 failed; the stopped wider run: 455 / 749 with 0 failed): full EditMode + named fixtures (PoiCardHeroSceneTests, CardSettingsGalleryTests, CardNavGalleryTests, CardLightSceneTests,
       PoiCardPlaceInArSceneTests, PoiCardFloatingSceneTests, CardGalleryTests, CardVideoGalleryTests, PoiCardSceneTests,
       PoiCardMenuSceneTests, PoiCardChromeSceneTests, PoiCardThemeSceneTests, PoiCardNavBarSceneTests, ThemePipelineTests)
9. [x] docs (CORE not run: the developer narrowed the runs) -- docs: _3.3 build log, _Test_Suite_1 skips table, 10-structure lines, report 6.4

## Contrast table (before -> after)
- hero words: 1.3-1.5:1 (title / line), eyebrow 1.67-1.93 -> 8.29-19.65:1 (worst: the painting's eyebrow)
- settings mark on the bar (F2): 6.6:1 -> >= 7:1 on its glass over black and white; 8.48-15.30:1 on the render at every stop
- dial pill words (F5): 1.11:1 -> >= 7:1 (the pill keeps --ts-surface)
