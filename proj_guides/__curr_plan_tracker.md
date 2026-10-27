# Plan tracker -- _3.3 S4a: the first story review's fixes

Brief: `proj_guides/__AI_worker.md` (S4a). Baseline: none (HEAD 3190b96, S3b full PlayMode 1431 / EditMode 2279 green).
GATE: done -- 0 commits ahead (nothing to push); captures TestEvidence/Gate/gate_game_view-3.png + gate_poi_editor.png (dark skin,
cropped off a sidebar strip); compile ExitCode 0, no error CS; SDF fonts unmodified.

## Root cause found in the audit (Part 1)
CardTapOutside judges a tap at its RELEASE only. A real tap on the takeover's Back clicks the button, the takeover hides, then the
release is judged over what is under it now -- at Full the Back arrow sits in the 48 px camera strip, so "a tap on nothing" ->
the card closes after the zoom's double-tap window. The S2c test tapped with panel events (CardTestInput.Tap), which the Input
System never sees. Fix at the source: a tap belongs to what was under its PRESS (judged at press time).

## Steps
- [ ] 0. Before evidence: archive TestEvidence/Hero/{before,after} -> Hero/_P4.4a/; MEASURING=before; run HeroMeasureSceneTests,
      ThemeFrameSceneTests, CardLightSceneTests, PoiCardStorySceneTests (background); copy outputs to TestEvidence/S4a/before/
- [ ] 1. Part 1 Back bug: CardTapOutside records press-on-something at press; CardTapRule.ShouldDismiss(press || release);
      PoiCardHost.HandleScreenTap keeps its signature. Tests: CardRulesTests case; PoiCardTapZoomTests Phase B real finger:
      The Painting's story page opened by a real tap, real tap on Back (both Full Height options) -> card open, same offset,
      story in view; mutation (old rule) -> red
- [ ] 2. Part 2 Full Height: CardOptions.FullHeights (top / camera_strip) + labels; CardContainerSettings.full_height = top;
      SheetStopRule.FullTopGap; PoiCardSheetView reads it from the bound settings; Editor row "Full Height" + (i) + Test guide;
      Tests: rule + round trip (EditMode), DetailCardEditorTabTests real click + undo, PoiCardFullHeightSceneTests (Phase B:
      top of the safe area / the strip gap / live switch / real grabber drag down / system Back / X)
- [ ] 3. Part 6: delete The Painting's block_3 (rich text) via LoadConfig -> RemoveAt -> SaveConfig -> CopyToStreamingAssets;
      re-base PaintingStory (lede / paragraph from the shipped story block_19) and ElementRefRuleTests; diff the config
- [ ] 4. Part 3 hero: HeroRule.Height(width, aspect, min 200, max Half-Peek); HeaderBlockView.SetHeroRange; flush under the strip
      (--ts-hero-strip-pull); local shade on the text block (48 px fade above, 24 px below), foot transparent; type 46/40 -> 40/34,
      eyebrow 12, line 15; Listen pill = headphones + time, name "Listen, 3:11". Tests: HeroRulesTests; PoiCardHeroSceneTests
      (flush, height per card, shade only behind the text, >= 7:1 x 4 cards x EN/PT x light/dark, the pill); HeroTitleFit re-base
- [ ] 5. Part 4 frame (light): --ts-rim-band 3px (dark 1px), darker --ts-metal-lo, groove part --ts-metal-groove (dark clear),
      sweep band follows the rim; Tests: edge >= 3:1 both modes, band per mode, sweep pixel delta frame sequence
- [ ] 6. Part 5 story preview: picture-first clip flush (top + sides, inner radius); PreviewFitRule (picture gives way first);
      Tests: rule (EditMode), gallery + real scene: title whole at the cap, picture flush
- [ ] 7. After evidence (MEASURING=after), captures judged (before / after), targeted fixtures, FULL PlayMode, full EditMode
- [ ] 8. Docs: _3.3 Build log "S4a", _5.1 Full Height row, 10-structure.md lines; final report + review questions

## Re-base log
(each re-based test: old -> new, from source)

## S4a review round 2 (developer, 2026-10-27): the premium header + the Navigation Bar
Header edits (a-f): words block (Listen included) fixed at the hero's foot, rises faster than the picture, fades near the top;
darker glow (0.72); Ribeira Palace hero = ribeira_palace_hero.jpg; the chrome FLOATS over the stack (no jump when the strip comes:
the stack starts at the sheet top, the hero runs under the bars); slim clear title bar, bare icons (white + top shade over the hero),
title on a glass pill.

### Navigation Bar (Detail Card > Navigation, first component) -- plan
- N1 data: card_settings.navigation.bar (CardNavBarSettings): enabled, position (under_title | bottom), shape (pill | full_width),
  corner radius, side margin, height, fill (glass | solid | clear) + fill opacity, border, shadow; selection: indicator (none |
  underline | pill | raised), slide indicator, bold selected icon, enlarge selected, selected colour (group | accent | ink), idle
  colour (muted | group), labels (icon_and_text | icon | text | selected_only); behaviour: show (after_hero | always), hide while
  scrolling (never | down), show at (half_and_full | full_only). items (CardNavBarItem): key, shown, label (per language), icon,
  icon_selected, color (theme colour), families. Empty items = one per navigable family (the 6 of today). NavBarRule (pure):
  effective items, a card's bar entries, current entry of a block, option fallbacks. EditMode tests.
- N2 runtime: NavBarView (not a registry element: NavSlots binds it into the top or bottom host when the bar is on; the menu /
  floating / edge elements are ignored then, their values kept); one sliding indicator; options as classes + inline numbers.
  CardChromeView: the bar follows its own show / hide options. PlayMode scene test (items, tap jumps, scroll moves the mark,
  each indicator, both positions) + renders.
- N3 Editor: "Navigation Bar" component first in Navigation: Enabled; sub-component "Bar Look" (position, shape, fill, selection
  effects, behaviour); "Bar Groups" table (Shown, ^ v, Label, Icon, Selected Icon, Colour, Families, x; + Add Group); (i) on every
  row; the old Elements rows hidden while the bar is on. Editor tests (real clicks, undo).
- N4 then the rest: compile + renders of the floating chrome, test re-bases, docs, report.

### Round 2 status (2026-10-27)
- [x] header a-f: words block fixed at the hero's foot (rise 0.25, fade near the top), glow 0.72, Ribeira hero picture, floating chrome
      (stack under the bars, LandingInset, TopFloats), slim clear title bar + bare white marks over a top shade, title pill
- [x] N1 data + NavBarRule + NavBarRuleTests (EditMode green)
- [x] N2 NavBarView + NavSlots.BindBar + chrome options + PoiCardNavBarSceneTests (green; renders TestEvidence/NavBar/)
- [x] N3 Editor component + help + NavigationBarEditorTabTests (green); bar ON for the LivingRoom wall (Editor path)
- [x] Re-bases: HeroRulesTests, PoiCardHeroSceneTests (Full instead of Half for the words / taps), ThemePipelineTests, NavigationPresetsTests,
      NavElementRegistryTests, DetailCardEditorTabTests (history walk + bar, first rows, open_stop edit, demo point pin); wall-choice pins
      (bar off, placed_model_control chip, demo poi "") in CardMenuOverlay / FloatingSlot / BentoLayoutSheet / ManySectionsFixture tests
- [x] Docs: _3.3 Build log "S4a" + the review's second round, _5.1 Navigation Bar + Full Height, 10-structure lines
- [ ] Full PlayMode (running), full EditMode: 2279/2295 -- 6 reds from the developer's own config edits (LiveSync badge x5: badges off;
      LivePlayModeNavigationPreset: the wall's rows are no named preset) + 9 DetailCard navigation tests doubled by an order-dependent
      leaked window (75/75 alone, 113/113 with the suites around them)
- [ ] Open for the developer: hero contrast (7:1 vs the light glow: 1.3-1.5:1); words fixed at the hero's foot are below the screen at Half
