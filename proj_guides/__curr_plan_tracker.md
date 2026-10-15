# Current plan -- _3.2 POI Card Navigation, step 2 (CardNavModel + NavOntologyRule + Format facet)

Brief: the Architect's "step 2, the nav model and the Format facet" block (2026-10-15).
Re-read this file and the brief after any compaction.

## GATE (done)
- [x] git push: 2 commits pushed (3197227..c7d875f; the brief said 1 -- c7d875f is a docs-only commit on top)
- [x] Captures opened: TestEvidence/Misc/Gate_GameView_nav2.png (LivingRoom scan, Main Camera), TestEvidence/Editor/Gate_PoiEditor_Navigation_nav2.png (Detail Card > Navigation); capture window closed, none left
- [x] Tree compiles: last two Editor.log builds "Tundra build success" / ExitCode 0; the 12 error CS lines are older (previous block)
- [x] Baseline full EditMode: 1620/1620, 0 failed, 83 s. Trusted commit 34d04f7 (full runs EditMode 1620 / PlayMode 778)

## Audit findings
- Fixtures exist: lamp (62 blocks), lamp_military (6), lamp_religious (1)
- Lamp worked example (_3.2 4.4) reproduced by hand from config.json: 56 targets, 18/14/1/7/3/4/7/2
- GUIDE SLIP: Lamp Military has 4 targets, not 5 (guide 4.4) / 6 (brief): block_5 is an audio_guide hero_chip (pinned top, rule 1).
  Same outcome (< 8 -> no navigation). Assert the rule-derived 4; correct the guide line.
- "waiting for ShowAfterViewed" is inline in BlockStackView (view, shared ground): the model reuses the SAME primitives
  (Definition.ShowAfterViewedField + BlockFieldReader.Flag + the seen state as a predicate) -- BlockStackView is NOT touched
- Mandatory Format breaks 10 fabricated test kinds (+ the app's size_comparison): each gets a Format

## Steps
- [x] 1. Format facet
  - CardOptions: FormatRead/Look/Watch/Listen/Model/Interact + Formats; GroupByFamily/GroupByFormat + GroupBys
  - BlockKindDefinition.Format; BlockRegistry.Validate: "no format" / unknown format
  - every built-in kind (33) + SizeComparisonBlock declares one; the 10 fabricated test kinds get one
  - CardStrings.Keys.Format* (6) + All + CardStrings.asset rows (en + pt + where)
  - tests: KindFormatTests (every kind of Shared incl. the app has a valid format; Validate refuses unknown + missing; the guide-named ones)
  - compile + full EditMode
- [x] 2. Data: CardNavigationSettings.group_by ("family"), min_blocks (8, 2..30), max_slots (5, 3..5) + limit consts
  - CardConfigRoundTripTests (old config -> defaults), DetailCardEditorTabTests field guard (8+8+5+3 + int/group_by edits)
- [x] 3. NavOntologyRule (pure, Runtime/Blocks/Navigation/): IsTarget (rule 1), ShowsNavigation (3), DirectTarget (4), SplitBar (5), clamps
- [x] 4. CardNavModel (pure): Build(entries, registry, settings, isRevealed) -> GroupBy, Sections (key, label key, count, kinds, targets,
        DirectTarget), TargetCount, ShowsNavigation, BarSections, MoreSections. Sections in stack order of their first target.
- [x] 5. Tests: NavOntologyRuleTests + CardNavModelTests (Lamp worked example by_family; default order; group_by format multiset;
        ShowAfterViewed via real CardLocalState; Military 4 -> none; Religious none; app family between guides and stories;
        min_blocks / max_slots incl. clamps and exact overflow)
  - compile + full EditMode
- [x] 6. Grep proof of reuse (IsPinnedTop / IsFooter / ShowAfterViewedField)
- [x] 7. Targeted PlayMode: PoiCardSceneTests, PoiCardLiveUpdateTests, LivingRoomCardSceneTests + grep hits PoiCardPlaceInArSceneTests,
        PoiCardAudioSceneTests, PoiCardLanguageSceneTests, SizeComparisonGalleryTests, CardGalleryTests
- [x] 8. Docs: _3.2 status row 2 + 4.1/4.4 corrections; 10-structure (new files, Format, settings, tests); RUN TIMES if moved; final EditMode
