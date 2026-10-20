# Plan tracker -- _3.2 step 17a (navigation presets, the Editor's owed warnings, the sibling chips)

GATE (done): UnityMCP answered (telemetry_status, TileStories@d41388e4a2b2ebd4); 2 commits pushed (1193ec4..50c6723); captures
`TestEvidence/Gate/gate_3_2_step17a_{game,editor}.png` opened (Editor window closed, 0 left); tree compiled (Library dll 13:35, no error CS in the console);
SDF fonts clean (git); EditMode baseline 1946/1946 (101.9 s) on clean 50c6723 (trusted).

## Finding (STOP item, asked at the end of the other work)
1.1 sibling chips: the guide says "each chip carries its KIND's icon (the set exists since step 6)". The set holds FAMILY icons (CardIcons: info,
picture, listen, book, show_on_wall, play, location, people, grid, sources + utility ones); a block KIND has none. Siblings of one section share a
family, so a family icon cannot tell a gallery from a 360 panorama. Ask the developer which way before touching TakeoverView.

## Decisions (autonomous)
- Presets = `NavigationPresets` in the EDITOR assembly (data table of section 8 + Apply + Matching); no runtime type. Phase B through the Editor-assembly
  `LivePlayModeCardTests` family (real window while Play Mode runs), the live-sync path every row uses.
- Apply = ONE `ApplyPopupEdit` (one mutation scope = one history step = one Ctrl+Z). No confirmation: it is undoable.
- Preset row at the END of the section (section 7's order), button shows the matching preset or "Custom"; picker = `EditorPopupContent` (working popup).
- Warnings = pure `NavigationWarningRule` (Editor): counts come from the REAL `CardNavModel` of the busiest card; thresholds 5 (progress map) / 13 (bento);
  Bar Slots warns only while raising it can still help (slots < min(sections, 3..5 max)); "more than two bars" = effective top + bottom + edge.
- One `DrawNavigationWarning` reports to a null-in-production probe so tests prove drawn / not drawn; the existing Tabs warning moves onto it.
- Preview Navigation = toggle + point popup over the demo card's own fields (`demo_card.enabled / poi_id`); turning it on from there sets the stop to Full.

## Steps
- [x] GATE
- [ ] A. `NavigationPresets` + tests (EditMode, pure: table, keys registered, reflection walk, Matching)
- [ ] B. `NavigationPresetPickerPopup` + Preset row + (i) + guide lines; Editor tests on the REAL window (click, walk, one Ctrl+Z)
- [ ] C. `NavigationWarningRule` + the three new warnings + bento + progress_map; drawn / not drawn tests
- [ ] D. Test > Preview Navigation rows + tests
- [ ] E. Phase B live (LivePlayModeCardTests): a preset applied live changes the open card
- [ ] F. Captures: Navigation at 620 pt (preset row, each warning), judged
- [ ] G. 1.1 chips (after the developer's answer) + recapture FullView_Lamp_media_{en,pt}_390
- [ ] H. Docs: _3.2 row 17a + 3 TODO lines out, _5.1, 10-structure.md; full EditMode + Card/Poi/SizeComparison PlayMode fixtures
