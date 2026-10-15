# Current block: _3.2 POI Card Navigation -- step 1 (ontology as data + Block Order)

Brief: `_3.2` sections 1, 2.2, 2.3, 3 (rows `block_order` + `families`), 4.6, 7, 9.
Baseline trusted: commit 3599627 for PlayMode (tree code-identical); EditMode re-run green this session.

## GATE (done)
- [x] UnityMCP live (`manage_editor telemetry_status` -> success)
- [x] `git push` -- 3 commits pushed (f0881b1..3197227)
- [x] Tree compiles, zero `error CS` (`refresh_unity` force + `read_console` errors = 0)
- [x] Full EditMode baseline GREEN 1606/1606, 65.0 s (first run's 25 failures were poisoned by the
      domain reload from my own force-compile immediately before it -- 40-testing 4.2.5 serialize rule;
      re-run on the identical tree is clean)
- [x] Oswald Bold SDF + Roboto-Bold SDF restored (`git checkout`), `TileStories/` tree clean, AssetDatabase refreshed
- [x] Captures opened: `TestEvidence/Editor/gate_gameview.png` (Game view, room mesh + painting),
      `TestEvidence/Editor/gate_poieditor_620.png` (Detail Card tab, Card Container open, 620 pt)

## PART 1 -- fixes
None (`_3.1` closed with no open fix-list item).

## 1.1 Families as data
- [x] `Runtime/Blocks/Navigation/FamilyDefinition.cs` -- `Key`, `Order`, `LabelKey`, `InNav`, `AtEnd` only
- [x] `Runtime/Blocks/Navigation/BuiltInFamilies.cs` -- the 9 keys as constants + `Register(BlockRegistry)`
      (about 10, media 20, guides 30, stories 40, ar 50, play 60, visit 70, community 80, meta 90 AtEnd)
- [x] `BlockRegistry`: `RegisterFamily`, `Families`, `TryGetFamily`; instance `Register` throws naming an
      unregistered family; static `Validate(definition)` keeps its exact signature and shape checks
- [x] `Ordered` sorts by family `Order` (stable; authored kind order inside a family)
- [x] `BuiltInBlocks.Register` registers families BEFORE kinds
- [x] every `Family = "..."` literal (9 files + the app's SizeComparisonBlock) and `ContentSeenRule.MetaFamily`
      read the `BuiltInFamilies` constants
- [x] the 8 bare `new BlockRegistry()` sites register families (fix the tests, never weaken the check)
- [x] `CardStrings.Keys` + `All`: 9 `family_*` keys; `CardStrings.asset` rows en + pt WITH accents
- [x] TESTS: `FamilyRegistryTests`; `BlockRegistryOrderTests` updated; `CardStringsTests` covers the 9 keys

## 1.2 Block Order replaces Sources At The End
- [x] `CardNavigationSettings` (`block_order` + `families` rows {family, order, at_end}) + `CardSettings.navigation`
- [x] `CardOptions.BlockOrders` + `BlockOrderLabels`
- [x] `BlockStackBuilder.Build` applies order AFTER the skip rules, header always first
- [x] DELETE `container.sources_at_end`, its Editor row and its help -- no shim, no migration key
- [x] grep-confirm the LivingRoom config never wrote it (no config file changes)
- [x] TESTS: `BlockStackBuilderTests` (authored / end_families_last / by_family, meta mid-fixture, a wall row
      putting At The End on a second family; `by_family` asserts ONE contiguous run per family, relative order);
      `CardConfigRoundTripTests` walks the new leaves (+ an `int` branch in its `Fill` walker)

## 1.3 Editor: Detail Card > Navigation
- [x] `Editor/POIEditor/DetailCard/POIEditorToolWindow.CardNavigation.cs` -- framed foldout AFTER Card Container,
      before Block Library, own section colour; rows: Block Order (popup, (i)), Families table
      (Order + up/down arrows, Family = visitor label + key, At The End toggle, (i)); Test sub-foldout
- [x] `Editor/POIEditor/DetailCard/POIEditorToolWindow.CardNavigationHelp.cs` -- (i) texts + 3 guides, app-agnostic
- [x] reuse `DrawPopupField`, `TableRowScope`, `DrawConfigMutationScope` and the ONE reorder helper
- [x] NO "Preview navigation" (step 17)
- [x] TESTS: `DetailCardEditorTabTests` (real click on a reorder arrow and on At The End, Ctrl+Z each, save writes
      them; the `sources_at_end` cases move to Block Order); `LiveSyncFieldMatrixTests` row `card_settings.navigation`;
      `PoiCardLiveUpdateTests.SourcesAtTheEnd_...` moves to Block Order

## 1.4 Phase B on the real wall
- [x] LivingRoomScene, The Lamp's card in each of the three orders; `by_family` = one contiguous run per family
      ending with Sources; `authored` keeps the authored order; the default behaves exactly as Sources At The End did.
      Assert RELATIVE order of real block keys.

## 1.5 Visuals (opened and judged item by item)
- [x] POI Editor at 620 pt, Navigation section open (`Editor/1xx_*_620.png` naming)
- [x] The Lamp's card at 390 px under the default and under `by_family`

## 1.6 Docs
- [x] `_3.2` status row 1 -> done + proof
- [x] `_3.1` section 5 drops the `container.sources_at_end` row, points at `_3.2` Block Order
- [x] `_3.1.1_Audit.md` section 12 matrix row: "replaced by Block Order (`_3.2` step 1)"
- [x] `_5.1_Editor_Tab.md`: remove Sources At The End from the Card Container row list (file inventory, 714) AND
      its row detail (4.7); document the new Navigation section in both places
- [x] `10-structure.md` for every new file
- [x] `40-testing.md` 4.2 RUN TIMES after the closing full run

## Closing
- [x] ONE full EditMode + ONE full PlayMode run, zero failures (mandatory: `BlockRegistry` /
      `BlockStackBuilder` are shared ground). Background full run NOT owed this block.


## Result (2026-10-15)

All sub-steps done and verified. Closing runs, zero failures:
- Full EditMode **1620 / 1620**, 71.4 s (baseline was 1606; +14 tests).
- Full PlayMode **778 / 778**, 1769 s (29.5 min), partly with Unity in the background.

Captures opened and judged: `TestEvidence/Editor/gate_gameview.png`, `gate_poieditor_620.png`,
`Editor/160_CardNavigation_620.png`, `Card/Card_BlockOrder_default_390.png`, `Card/Card_BlockOrder_by_family_390.png`.

Two things the captures caught that no test did: the Families table's (i) buttons sat ~70 pt left of the
Block Order row's until the table moved onto the shared-row recipe, and the first two card renders were
pixel-identical (the top of the card, then the first Media block) because About opens the card in every
order -- the renders now sit at the first block AFTER the About run, where the orders really diverge.
