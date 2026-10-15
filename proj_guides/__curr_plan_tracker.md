# Current plan -- _3.2 POI Card Navigation, step 3 (NavController, scroll-spy, jumps) + the typing-harness fix

Brief: proj_guides/__AI_worker.md (2026-10-15). Re-read it and this file after a compaction.

## GATE (done)
- [x] git push: 1 commit pushed (c7d875f..7815cf7)
- [x] captures opened: TestEvidence/Card/Gate_3.2_step3_gameview.png, TestEvidence/Editor/Gate_3.2_step3_poi_editor_navigation.png
- [x] tree compiles (ExitCode 0, Tundra build success); SDF fonts clean (no diff)
- [x] EditMode baseline 1683/1683 (76 s, focused)

## PART 1 -- the typing harness (done)
- [x] red reproduced unfocused: "Live heading" -> "Live head" (editor_is_focused false early + end)
- [x] mechanism measured: SendEvent is synchronous; ANY focus change (OS or host.Focus()) ends the IMGUI field's editing session
- [x] fix: PoiEditorWindowHost.ReplaceText types the word in ONE tick once the field edits and the host holds focus; bounded re-entry
      before the first key; read-back of EditorGUI.activeEditor; "" = a real Backspace (TaxonomyRowIdentityTests drops its late Backspace)
- [x] PoiEditorWindowHostTests (FocusThief on EditorApplication.update): theft after the click -> re-entry, one undo step; theft as soon as
      typing landed -> whole word. Load-bearing: the old per-key loop turns the second red
- [x] 6 typing fixtures 88/88 focused; background runs 86/86, 5/5, 5/5, 5/5 (focus read false early, Play Focused pulled Unity back)
- [x] __mixed_TODOs.md: keystroke item removed (and its 2026-10-01 duplicate); before_after kept

## PART 2 -- step 3
- [x] 3.1a NavScrollRule (Runtime/Blocks/Navigation/, pure) + CardNavigationSettings follow_scroll / jump_raises_to / jump_motion + CardOptions
- [x] 3.1b NavScrollRuleTests (Tier 0) + guard updates (DetailCardEditorTabTests field walk, CardConfigRoundTripTests defaults)
- [x] 3.2 CardEventKinds.NavJump; IBlockHost.ScrollToBlock(blockKey, elementKey) (+ StubHost)
- [x] 3.3 BlockStackView.IsRevealed (one delegate) + WaitsToBeRevealed in Bind + Revealed event + SlotOfBlock; agreement EditMode test
- [x] 3.1c NavController (Runtime/UI/Cards/Navigation/): Bind/Unbind, JumpTo, JumpToBlock, Current, CurrentChanged, IsJumping, Clock, Duration
- [x] 3.4 sheet: owns Nav, builds model + controller in Show, rebuilds on Revealed, Duration from its height transition; ScrollToBlock -> Nav
- [x] full EditMode green (1694/1694, background)
- [x] Phase B PoiCardNavSceneTests 10/10 (background) -- found: Duration read 0 (no time custom style), clamp at peek, reveal cancelled jump
- [x] FULL PlayMode (background) 788/788, 2001 s
- [x] 3.5 docs: _3.2 4.4 + [step 3] TODO removed + row 3 proof, _3.1.1 correction, 10-structure.md, 40-testing RUN TIMES
- [ ] final report (60-finishing 6.4)
