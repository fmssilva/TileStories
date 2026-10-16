# Plan tracker -- _3.2 step 6: the rest of the top elements

GATE
- [x] git push: 1 commit pushed (09a2d6b..038a500)
- [x] captures: TestEvidence/Gate/gate_3_2_step6_gameview.png (gallery camera, Edit Mode), gate_3_2_step6_poieditor.png (Detail Card > Navigation), opened
- [x] compile: last ExitCode 0 after the last (old) error CS; SDF fonts clean
- [x] full EditMode baseline 1739/1739 (101 s, background) on 038a500

FINDINGS
- --ts-family-* tokens already exist (8, unused); _3.3 already lists them -> reuse, add --ts-family-meta
- kinds have no visitor words -> BlockKindDefinition.LabelKey + kind_* CardStrings rows (en + pt), guard test
- segmented / chips need a per-block spy -> NavController.CurrentBlock
- PART 1 floor: footer 81 at Lamp Half leaves 25.6 px. DEVELOPER DECIDED: strip keeps the 72 floor and goes first;
  the footer keeps a ONE-LINE floor (24 px). Lamp Half = footer + 25.6 px, no strip.

STEPS
- [x] P1.1 CardChromeRule: strip first (72), footer floor 24 (MinStackViewportUnderFooter); CardChromeRuleTests; Phase B assert; retake Chrome_Lamp_half_default_390.png
      + found: an instant jump at Half ended before the card's reaction (header collapse, strip back) -> block 60 px low.
        NavScrollRule.JumpDone: every jump holds its landing >= one motion length (NavScrollRuleTests updated)
      + PoiCardChromeSceneTests.DragStack starts inside a viewport shorter than 40 px
- [x] P1.2 Follow Scroll Editor row (i, undo, live), between Top Bar and Hide While Scrolling (+ help, guide line, test)
- [x] P1.3 first heading gap under the strip: .poi-card--nav-top-shown pads the stack content (--ts-space-3); CardChromeView sets it, reads content without it; recaptured
- [ ] P1.4 audio TODO left logged unless it shows (decided at the closing run)
- [x] 6.1 (EditMode 1745/1745) FamilyDefinition.IconKey / ColorToken; new icons picture, listen, book, play, people, sources (+ reuse info, show_on_wall, location); --ts-family-meta; kind LabelKey + kind_* rows; guard test
- [x] 6.2a model: Section.IconKey/ColorToken, Kind.LabelKey/ColorToken, Groups (rule 6, NavGroupRule), Targets + SectionOf/KindOf/GroupOf; NavController.CurrentBlock
- [x] 6.2b NavElementDefinition.Choice (Sections / Groups / Blocks) + ReadsLabels / ShowsCounts; NavSlotRule.OffersAChoice
- [x] 6.2c pure ProgressMapRule.Widths, NavLabelRule, NavOntologyRule.OffersAKindChoice; NavTopElementRulesTests (Tier 0)
- [x] 6.2d views: NavStrip, NavItems (NavTab, NavChip, NavCount, NavTint), TabsNavView (+ Text/IconTabs), NavChipRow, KindChips, IconTabsChips,
      Segmented, ProgressMap; CardTokenColours; Navigation.uss; tokens chip-height/radius, ahead-opacity
- [x] Phase A: ElementKeys + ElementChecks for the five; CardNavGalleryTests 32/32; renders judged (picture icon redrawn after the first look)
- [x] 6.3 data: labels, show_counts, groups (CardNavGroup), CardFamilySetting.group; round trip (defaults), field walker 15 nav fields; live matrix unchanged (one row)
- [x] 6.3 Editor rows Group By, Labels, Show Counts (shown per the element's ReadsLabels / ShowsCounts / Choice), Groups table
      (CardNavigationGroups.cs), Families Group column; 2 tests (real click / typing / Ctrl+Z); Top Bar list needed no edit (registry)
- [x] guard load-bearing: --ts-family-meta renamed -> FamilyIconAndWordsTests failed naming meta -> restored
- [x] Phase B: PoiCardTopElementsSceneTests 15/15 (5 elements x: every item tapped lands +-1 px + nav_jump + mark; 6 drags mark the screen; PT 48 px / whole words / render)
- [x] captures: Editor/180_CardNavigation_IconTabs_620, 181_..._Segmented_620 (-> _b Sources popup removed -> _c header overlap fixed + test), 182_..._GroupsTable_620; Card renders judged
- [x] closing: full EditMode 1755/1755 (88 s); Card/Poi PlayMode 25 fixtures 593/593 (1716 s)
- [x] 6.4 docs: _3.2 status 6 + four TODOs removed + 2.2 / 2.5.6 / 4.3 / 5.1 / 7 built notes, _3.3 tokens, _5.1 rows, 10-structure,
      40-testing RUN TIMES, __mixed_TODOs (keyword flake sighting, progress map at 8 families)
- [x] P1.4 the audio failures did not appear (both passed in the closing run, background part): left logged

RESULT: done. Commit message: Add icon tabs, chips, segments and a progress map to the card
