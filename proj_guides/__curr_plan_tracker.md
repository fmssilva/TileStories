# Plan tracker -- _3.3 P4.1: the theme pipeline (behaviour-free)

Brief: `__AI_worker.md` (P4.1, R12.7 step 1). Worker: Opus-class. No pixel may change. Do not commit (the Architect does).

## GATE (done)
- [x] push: 0 ahead (HEAD 9375c94 = origin)
- [x] UnityMCP: telemetry_status ok
- [x] captures opened: `TestEvidence/Gate/P4.1_gate_game.png` (Game view, Edit Mode: room scan + painting),
      `TestEvidence/Gate/P4.1_gate_editor.png` (POI Editor window; a first try caught a mail inbox -> deleted, retaken at 800x640); 0 windows left
- [x] compile: 0 `error CS` in Editor.log, last builds ExitCode 0; fonts: git status clean -> nothing restored
- [x] baseline: none run (HEAD 9375c94 = last green: EditMode 2039/2039 P1c, PlayMode 1219/1219 P1b part 2)

## Findings (verified on disk)
- Only C# COLOUR read by name: `CardTokenColours` (caller `NavTint`). Layout numbers read from `.ts-card`'s customStyle (stay):
  PoiCardSheetView (top gap, collapsed header), CardTrackFit (track peek), CardBentoView (gap, names width), FabRadialNavView (4), FloatingSlot (float gap).
- UIDocument hosts: PoiCardHost, CardGalleryHarness (CardGallery + CardNavGallery scenes), SearchUIHost, ZoomControlView; SpikeStage = dev-only, untouched.
  One PanelSettings (5 UIDocuments, 3 scenes). NO Editor window / preview hosts a `.ts-card` (0 hits in Framework/Editor).
- Six token roots (sheet, mini, menu, floating + scrim, takeover) all live under the card's UIDocument root -> inherit from `.ts-app`.
- Tests that parse CardTokens.uss text: CardTestInput.TokenPx, PoiCardNavSceneTests.DurationToken, FamilyIconAndWordsTests (family colours),
  LivingRoomBlocksTests (SizeComparison.uss tokens).

## Decisions
- Every COLOUR token -> theme (incl. correct / wrong / result-fill / video-letterbox / clear); type sizes, radii s/m/l/round, space 1-6, line width,
  weight, glass opacity, duration, ease -> theme. Everything else stays in CardTokens.uss on `.ts-card.ts-theme-default` (selector unchanged).
- Asset at `Runtime/Resources/TileStories/Themes/Theme.default.asset`, filled by parsing today's CardTokens.uss once (execute_code, not committed).
- Typed lists in a ThemeTokenSet (shared / per mode / per knob option); knobs empty and not generated (P4.5).
- C# finds the theme from the nearest `.ts-app` root's classes; ThemeController.Apply(root) called by each host.
- Pixel set = permanent [Explicit] PlayMode fixture: baseline/ then latest/ with a zero-diff assert; scene renders cropped to the card.

## Steps
- [x] 4.1.0 Measure: ThemePixelSetTests (12 gallery renders + Lamp Half / Full), [Explicit]. Noise floor found and fixed: rounded corners
      (painted out), AutoAdvanceAudio (off), frame-count waits (grab until still), focus state (.fg/.bg baselines), real-card text AA
      (several baselines + COLLECT + noise mask). Judge run on the unchanged tree: 14/14 exact
- [x] 4.1.1 ThemeAsset + ThemeTokenSet / ThemeToken<T> / ThemeMode / ThemeKnobOption; ThemeResolver; asset filled by parsing CardTokens.uss (56 tokens)
- [x] 4.1.2 ThemeUssGenerator + ThemeAssetEditor (Regenerate USS); generated lines == old lines (56/56 text-identical);
      ThemeUssStaleTests 4/4; mutation (surface-2 -> red at line 9, restored -> green)
- [x] 4.1.3 TileStoriesTheme.tss; PanelSettings.themeUss (one-line diff); ThemeController.Apply in PoiCardHost, CardGalleryHarness, SearchUIHost, ZoomControlView
- [x] 4.1.4 CardTokens.uss: 160 declarations before = 104 card + 56 theme after, text-identical, 0 overlap, 0 colours; 7 stale comments fixed
- [x] 4.1.5 CardTokenColours through ThemeController.TryResolverFor; TokenPx / DurationToken / FamilyIconAndWords / LivingRoomBlocks re-pointed (EditMode 11 + 19 green)
- [x] 4.1.6 ThemeTokenGuardTests 3/3 + 2 mutations (deleted --ts-touch-target -> red; --ts-ink re-declared -> red; restored -> green);
      ThemePipelineTests 3/3
- [x] 4.1.7 Pixel set after: 14/14 EXACT (0 px, max delta 0). Finding: text pixels depend on the font atlas's fill history (A/B with old
      and new token file alike; resolved colours of 120 elements bit-identical, probe deleted). Side-by-side
      `TestEvidence/ThemePixels/P4.1_before_top_after_bottom.png` judged; Game captures `Search/Search_Card_Lamp_Half.png` (card open),
      `Search/Search_Field_search_match_any.png` (search open) judged
- [x] 4.1.8 Targeted 55 fixtures 1021/1021 (3177 s); full PlayMode 1237 results, 2 red not from P4.1 (S4 spike timing, green alone;
      ARealDragToTheEnd layout-struggle entry, 8/8 alone); [Explicit] leaked into the full run -> MEASURING gate (13 skipped off / 13 green on);
      full EditMode 2046, 1 red = KeywordListFieldTypingTests (documented focus flake, green alone); spike Fraunces TMP asset restored
- [x] 4.1.9 Docs: 10-structure, _3.3 build log + section 2 + family row, _3.1 Styles, _3.2 families line, 40-testing RUN TIMES, __mixed_TODOs
- [ ] final: LivingRoomScene active; final report (no commit)
