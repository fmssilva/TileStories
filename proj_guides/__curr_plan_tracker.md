# Current plan tracker -- _3.1 step 5b + Tier 1 group A (2026-09-27)

Baseline (re-verified 2026-09-27): Editor.log ExitCode 0; EditMode 1129/1129 (one run had the known flaky
TaxonomyRowIdentityTests...ReportsBothRowsByPosition, green on rerun + alone); PlayMode 254/254.

## 5b
- [x] 5b-1 strings: CardStringEntry + card_settings.strings, CardStringTable SO + CardStrings.asset (en/pt),
      CardStrings resolver, CardStrings.Keys; X drawn in USS, tooltip from table; BlockBindContext.Strings;
      Editor Detail Card > Card Texts; tests (EditMode resolver + asset keys, Editor typing, Phase B pt/override)
- [x] 5b-2 tap vs double-tap: TapOutsideDismissal (pure) + PoiCardHost; EditMode rule tests; PlayMode real touches
      queued on a Touchscreen (two taps / one tap / far taps) -- NOT InputTestFixture (breaks the scene's UI actions)
- [x] 5b-3 status colour: WallSession.MarkerLook, CardStatusRule (MarkerVisualResolver), token fallback; tests incl.
      the card ring == the running marker's Image colour + sprite before/after a live per_type push

## 6A (micro-cycle per kind)
- [x] gallery generalised (entry = kind x variant x content) + generic Phase A checks (fit, contrast on the real
      background, tap targets, render)
- [x] status (ring, scale; unknown state; has_status=false skipped via ShowsFor)
- [x] quick_facts (chips, grid_hairline, big_numbers)  [Items drawer came with rich_text]
- [x] rich_text (plain, drop_cap, lede, sections) + glossary ([[term]], card_settings.glossary, Editor Glossary)
- [x] fun_fact (flip, postcard)
- [x] pull_quote (serif, minimal)
- [x] sources (list, with_confidence)  [Choice drawer]
- [x] actions (circles, pill_row, sticky_cta)  [IBlockHost, footer slot; sticky = ONE call to action]
- [x] fixture: lamp = 19 blocks (every kind x variant, catalog order), lamp_military = header + rich_text + quick_facts
- [x] CardViewSourceRulesTests (no literal colour/size, no visitor string in card views)
- [x] Editor captures (Card Texts, Glossary, Card Content Items + Choice) -> delete buttons moved beside their rows
- [x] docs: _3.1 status (5b, 6A, 6B rows) + body + design history + open points; 10-structure.md; _5.1; _3.3; _2.6

Full runs: EditMode 1177/1177 (09:05) -> rerun after the Editor capture fixes; PlayMode 338/338 (09:05-09:15).
Next session: 6B (process_steps, swatches, timeline, person, story_chapters, compare_points, practical_info).
