# Plan tracker -- P3c (continued, 3): the halo for Half, Raised Depth, Icon Colour; close green

Brief: `proj_guides/__AI_worker.md` "P3c (continued, 3)" (`_3.3` fourth round > "P3c review" b and "P3c review, second pass" a + c).
Contrast policy: words over a picture >= 4.5:1, all else 7:1 (icons on the bar >= 4.5:1). Worker: Opus, effort High.
TEST PLAN: BLOCK = full EditMode + the 14 card fixtures (list at the end) on the FINAL state. STOP after the report.
OUT OF SCOPE (P3d): the Reveal both ways (Motion > Replay), Tint Depth, the FAB.

## GATE
- [x] MCP live (telemetry_status ok); origin = HEAD = fbc2f36, nothing to push; SDF fonts unmodified (git status clean under both Fonts/)
- [x] Play Mode off; compile clean (last Tundra ExitCode 0, the log's `error CS` lines older than it; console 0 errors); no InitTestScene
- [x] baseline full EditMode on the uncommitted tree: 2346 / 2194 passed / 0 failed / 152 skipped, 205 s, Unity behind both ends
- [ ] captures (Unity b): POI Editor rows in the dark skin; Game-view renders

## PART 1 -- the halo for Half: (c) a two-layer halo + (a) the far-side check at Full only -- DONE (the developer's picks below)
- [x] InkRects.ShownLabels (the one rule of which labels carry ink); HeroWordsOnPicture owns the SECOND layer: a halo layer in the tone
      layer holding one pooled back label per shown word (same words, face, size, box; the box grown by the SDF reach so a cut eyebrow
      never clips its glow); Media.uss .card-hero__halo (outline --ts-text-halo-wide-width 1.5px + glow --ts-text-halo-wide-blur 18px)
- [x] FINDING -> developer pick "Wider atlas": the glow could not reach past the font SDF (padding 6 @ 48 pt = 0.125 em); FontAtlasBuilder
      UiAtlasPadding 18 / UiAtlasSize 2048 for the 4 UITK theme assets (TMP untouched; glyphs 290 / 325 as before)
- [x] FINDING -> developer pick "Keep halo, (a)": measured on the render the halo takes 0.10-0.26 off the title's brightest ring pixel but
      0.00-0.02 off the eyebrow / Listen time (~2 px reach on 12 px text, whatever outline width): no credit counted; the credit code removed
- [x] far-side ~0 at Full only; the looks read at Full; every word >= 4.5:1 unchanged
- [x] developer review of the renders (TestEvidence/Premium/P3c/review_words_on_picture.png): (1) the tone's reach is taste -> Tone Spread,
      a header Number field 0.4..1.5 (default 1), HeroWordsRule.Spread / Layer(spread); (2) BUG: scrolling, the tone layer rose with the
      words and bared the picture's foot under a hard edge -> the layer stays, Rise(rise) moves its anchor and the halo layer
- [x] EditMode LookKitRulesTests 13 / 13 (ToneSpread_...), FieldVisibilityRuleTests (Tone Spread in every picture look): 24 / 24
- [x] PlayMode PoiCardHeroSceneTests 58 / 58 (Unity behind): TheHalo_IsTwoLayers_..., Scrolling_TheToneRisesWithTheWords_..._NoCutEdge,
      ToneSpread_LessDarkensLessOfThePicture_AndEveryWordStillReads (196945 vs 260492 px darkened), far side at Full

## PART 4 -- DONE: docs (_3.3 build log, _Test_Suite_1, 10-structure, the --ts-scrim-hero note + the regenerated USS); BLOCK on the final
## tree: full EditMode 2352 / 2200 / 0 / 152 (257 s), 14 card fixtures 272 / 272 (30.3 min), both Unity behind; the Editor rows captured in
## the dark skin (TestEvidence/Editor/p3c_navbar_rows.png); LivingRoomScene open, Play Mode off. STOP.

## PART 2 + 3 -- DONE: Raised Depth (rule, config, live push, Editor slider) and Icon Colour (rule, config, Editor row, Chosen Colour
## default Group; the shipped bar moved to Group and given the two new fields). EditMode NavBarShapeRuleTests / NavBarRuleTests /
## NavigationBarEditorTabTests 32 / 32; LivePlayModeNavigationBarTests 2 / 2; PlayMode PoiCardNavBarSceneTests 46 / 47 -> the red one the
## old default (an accent disc), its assertion moved to the group's colour; icons >= 9.48:1, disc icons 7.50-9.12:1

## PART 2 -- Raised Depth (slider -1..+1, default +1; 0 flat; < 0 a cradle)
- [ ] CardNavBarSettings.raised_depth = 1; NavBarRule.RaisedDepth (clamped, NaN = 1)
- [ ] NavBarShapeRule.EdgeRise (depth R above 0, depth (R + gap) below) + CrestRadius (R / R + gap); Path takes a SIGNED rise -- a cradle
      is the bell mirrored about the flat edge (concentric with the disc at -1); --ts-navbar-cradle-gap; the disc never moves
- [ ] NavBarView: the path at EdgeRise, the disc at its own place (BellHeight)
- [ ] Editor: "Raised Depth" slider after Indicator, only under Raised, (i); drawing never writes
- [ ] EditMode: NavBarShapeRuleTests at -1 / -0.5 / 0 / 0.5 / 1; NavBarRuleTests default / clamp / old config; NavigationBarEditorTabTests
      (rows, a real slider drag = one undo step, hidden without Raised)
- [ ] Live push: LivePlayModeNavigationBarTests -- a real drag of Raised Depth in the window reaches the open card
- [ ] PlayMode PoiCardNavBarSceneTests: pixels at -1 / 0 / +1, Bottom and Top

## PART 3 -- Icon Colour: Group (default) / Accent / Neutral
- [ ] CardNavBarSettings.icon_color = "group"; CardOptions.NavBarIconColours / labels; Chosen Colour's default = Group (the disc in its
      group's colour by default); NavBarRule.IconColour, IconToken (group's -text / --ts-accent-text / --ts-ink-2), DiscInkToken (the
      group's -ink / --ts-on-accent / --ts-surface); NavBarView.Paint (labels stay ink / ink-2)
- [ ] Editor: "Icon Colour" after Labels, (i)
- [ ] EditMode: NavBarRuleTests (defaults, tokens; every group's -text >= 4.5:1 on --ts-bar and its -ink >= 4.5:1 on its colour, light /
      dark, from the theme); NavigationBarEditorTabTests (row + pick)
- [ ] PlayMode PoiCardNavBarSceneTests: icon_color cases; per group on the render, light / dark, every fill: icons >= 4.5:1, the disc
      its group's colour with its icon >= 4.5:1

## PART 4 -- docs, BLOCK, looks, report
- [ ] _3.3 build log; _Test_Suite_1; 10-structure (InkRects, PictureLuminance, LookKit/Tone.cs, HeroWordsRule.cs, HeroWordsOnPicture.cs,
      images/night_hero.png; the changed lines); Theme.default.asset's --ts-scrim-hero note (regenerate the USS)
- [ ] BLOCK: full EditMode + the 14 card fixtures (+ LivePlayModeNavigationBarTests) on the final state
- [ ] LOOK: Editor rows in the dark skin; the renders
- [ ] report with ONE commit message for all of P3c; LivingRoomScene open, ready to play

## The card PlayMode fixtures of BLOCK
PoiCardSectionsSceneTests, PoiCardNavBarSceneTests, PoiCardHeroSceneTests, PoiCardChromeMarksSceneTests, PoiCardSceneTests, CardLightSceneTests,
ThemeFrameSceneTests, SectionSheetSceneTests, PoiCardLayoutSceneTests, PoiCardPagesSceneTests, PoiCardPaintingSceneTests, PoiCardThemeSceneTests,
PoiCardChromeSceneTests, PoiCardPageChromeSceneTests (14)
