# Plan tracker -- _3.2 step 9: the floating slot, fab_dial, fab_radial and the placed-model chip (+ the two AR TODOs)

Brief: proj_guides/__AI_worker.md + the step 9 brief. Spec: _3.2 sections 4.2, 5.4, 9; TODOs [step 9] x2 (from _3.1's close-out).

## GATE (done)
- [x] UnityMCP answers (telemetry_status, one instance TileStories@d41388e4a2b2ebd4)
- [x] git push: 1 commit (c22876b..3de449c)
- [x] captures opened: TestEvidence/Gate/gate_3_2_step9_game_view.png (LivingRoomScene camera), gate_3_2_step9_poi_editor.png
      (Detail Card > Navigation, capture window closed, 0 left)
- [x] compile: Editor.log 0 `error CS`, ExitCode 0; both SDF fonts clean (no git change)
- [x] baseline full EditMode 1813/1813 (86 s, background) on clean 3de449c (rides on step 8's full PlayMode 921/921)

## Design (decided)
- WHERE: the floating slot is a LAYER in the card layer (the sheet clips its children): `FloatingSlot` (Runtime/UI/Cards/Navigation),
  sheet-owned, inserted right BEFORE CardMenuOverlay.Root -> order: sheet, floating, menu overlay, full view (an open drawer covers it).
  A row, right-aligned: [placed-model chip][FAB]. It follows the sheet's top edge on the sheet root's GeometryChangedEvent (every frame of
  the height transition and of a drag) through pure `FloatingSlotRule.Bottom` (sheet top + gap, clamped so the slot never leaves the
  layer: at Full it rests in the 48 px top gap). Hidden while no card is open or nothing is in it. The chrome floor does not apply.
- Elements: NavSlotKeys.Floating built; NavSlotRule.FloatingElement; NavSlots binds the FAB into FloatingSlot.Elements.
  - `fab_dial` (FabDialNavView, "Dial", NavChoice.Sections): a round FAB (dark surface disc + light edge + accent icon, X while open);
    a tap opens a column of labelled pills (family icon in its colour + words), one per section, up when it fits above, else down over
    the card (pure FabFanRule; a list longer than both sides scrolls); a pill = NavController.JumpTo. An overlay ANCHORED client (Back,
    a jump, a scroll, a stop change close it; the FAB again toggles).
  - `fab_radial` (FabRadialNavView, "Radial", NavChoice.Sections, ReadsBarSlots): items = BarSections + More (rule 2.5.5, like the
    bar) on an arc in the left half-plane laid out by pure `RadialMenuRule.Layout` (first R that keeps every item inside the layer and
    neighbours a footprint apart); press-slide-release through SwipeGrab (claims any direction past the slop) -> ItemAt(travel) on
    release; a plain TAP opens the arc, a tap on an item picks (WCAG 2.5.7). More: opens the Menu when set, else re-fans the arc with
    MoreSections. Overlay anchored client too.
- The chip: `PlacedModelChipView` (Runtime/UI/Cards, beside MiniPlayerView) -- holds no state, draws the ONE ICardArPlacement through pure
  `PlacedModelChipRule` (Hidden / ThisCard: "Placed by the wall" + Remove / OtherCard: "<name> is placed by the wall", a tap opens that
  point's card, no Remove); bound by FloatingSlot with the card's BlockBindContext (ArPlacement, Poi, Strings, Settings, Host),
  released on Unbind (Hide) by unsubscribing from the owner's Changed. Dark surface + light edge: legible over a bright AND a dark frame.
- Tap on empty camera space while something is open over the card closes THAT first (PoiCardSheetView.TapOutside: overlay, else close),
  the order Back already has.
- Data: navigation.floating ("none") + navigation.placed_model_control ("chip"); CardOptions.PlacedModelControls / labels.
- Strings (CardStrings.asset en + pt): nav_fab (Sections / Seccoes with accent), placed_model_remove (Remove / Remover),
  placed_model_other ({0} is placed by the wall / {0} esta junto a parede, accents).
- Tokens: --ts-float-gap (number), --ts-float-edge-width, --ts-radial-footprint, --ts-radial-min-radius (number tokens read in C#).

## Steps
- [x] 9.0 Tier 0 pure rules + EditMode tests: FloatingSlotRule, FabFanRule, RadialMenuRule (layout + ItemAt incl. edges / outside / dead
      zone), PlacedModelChipRule; data fields + CardOptions; round trip defaults
- [x] 9.1 FloatingSlot layer + NavSlotKeys.Floating + NavSlotRule.FloatingElement + NavSlots + chip view + strings + tokens + USS;
      EditMode: layer order (sheet < floating < overlay < takeover), chip states with a real ArPlacementService over ManualArWall
- [x] 9.2 fab_dial view + registration; 9.3 fab_radial view + registration (+ TapOutside closes the overlay first)
- [x] 9.4 Editor rows Floating Button (under Menu) + Placed Model Control (under Back Button), (i), guides, Bar Slots for the radial;
      DetailCardEditorTabTests + walker 17 -> 19
- [x] Phase A: CardNavGalleryDefinitions + fab_dial, fab_radial (x 5 cases, closed + open renders); CardNavGalleryTests checks
- [x] Phase B: PoiCardFloatingSceneTests (LivingRoomScene, real input): stops unchanged; the slot rests above the sheet at peek / half /
      full; a real press-slide-release picks the item the angle points at; a real tap opens the arc, a second tap picks; the dial's items
      jump; tap outside closes the dial first; an open drawer covers the slot; chip inside the VISIBLE rect at peek with a model standing
      (EN + PT), at half and full; Remove removes it and the chip goes; another card: no Remove, a tap opens the first card; control none
      hides it (live); chip over a bright and a dark frame
- [x] Captures judged: both FABs open / closed on the real Lamp at 390 px EN + PT; chip at peek / half / full; Editor section
- [x] Closing: full EditMode; Card / Poi PlayMode fixtures + ArPlacement greps (CardGalleryTests, PoiCardPlaceInArSceneTests,
      PoiCardSceneTests) + the new fixture -- zero failures
- [x] 9.5 docs: _3.2 row 9 + proof, both [step 9] TODOs removed, 4.2 / 5.4 / 7 "Built at step 9"; _5.1 4.7 rows; 10-structure;
      __mixed_TODOs device list (radial with a thumb, chip over the camera)
