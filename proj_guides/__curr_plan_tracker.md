# Plan tracker -- POI Detail Card 15.2 (audit fixes B + C)

Brief: card order + repeated actions (15.B), place_in_ar placed state (15.C), POI-switch option, dialogue hidden reply.
Tree trusted: green commit 0699cbc (no opening full baseline). One full EditMode + PlayMode at the end (15.2.6).
After a compaction: re-read this file and the brief's sub-step list; continue at the first unticked box.

## GATE (done 2026-10-02)
- [x] UnityMCP is the right server (telemetry_status ok)
- [x] Game view capture opened (TestEvidence/Card/Gate_GameView.png: the room scan) and POI Editor capture opened (TestEvidence/Editor/Gate_PoiEditor.png: the window, nothing else); window closed, 0 left
- [x] Oswald Bold SDF.asset not modified (git status clean for it)
- [x] Tree compiles (no error CS after the last reload; TileStories.dll = last green commit's build)

## 15.2.1 Sources At The End (container option)
- [x] CardContainerSettings.sources_at_end = true (initializer = default; old configs load as on)
- [x] BlockStackBuilder: stable partition, family == ContentSeenRule.MetaFamily last, header stays first; off = authored order
- [x] Editor: Card Container row "Sources At The End" (+ (i) help const), undo via config history, live via CardSettingsChanged -> Rebind
- [x] Tests: builder pure (on/off, stable, no meta, two meta, header first); real-window row (default on, click off, Ctrl+Z); PlayMode Phase B (The Lamp's Sources last, live toggle through the session)
- [x] Capture check; learning summary + commit line

## 15.2.2 Repeated actions, one wording
- [x] ActionsRule (pure): which Actions rows the look draws (known action; sticky = first only) -- shared by ActionsBlockView and the offers rule
- [x] ActionsBlockView: empty Words on a known action reads the action's CardStrings row (show_on_wall -> ShowOnWallButton)
- [x] RepeatedActionRule (pure, Runtime/Blocks): per action, every offer of the SHOWN blocks (Actions rows, Show On Wall kind, sticky footer)
- [x] Editor: replace HasStickyShowOnWall / CardShowOnWallRepeatsStickyText by one warning per repeated action naming the blocks
- [x] config.json: The Lamp's sticky label emptied (block_19), every block kept; Actions Words (i) says "empty = the card's own words"
- [x] Tests: ActionsRule + RepeatedActionRule pure; Tier3GroupBRulesTests:317 updated; gallery "partial" Actions entry updated; real-window warning test; PT capture at 390 px
- [x] Learning summary + commit line (Editor capture of the warning rows: with the final captures in 15.2.6)

## 15.2.3 place_in_ar placed state
- [x] CardStrings: place_in_ar_remove reworded "Remove from room" / "Remover da sala" (the chip is gone, one row = the button), new place_in_ar_placed "Placed by the wall" / "Colocado junto a parede" (accents), Keys.All + asset rows
- [x] IBlockHost.ShowHeaderAtPeek (scroll to top + peek) implemented by PoiCardSheetView; StubHost updated
- [x] PlaceInArBlockView: placed -> button = Remove (enabled even if the wall is lost), status line, no chip; remove -> first state; Ar.uss
- [x] Tests: gallery test + Phase B real taps (place, label, status, scroll 0, peek, remove); EN + PT captures 390 px
- [x] Learning summary + commit line

## 15.2.4 Keep Model On Switch (option)
- [x] Toggle field keep_on_switch on place_in_ar, LibraryDefault allowed on Toggle (BlockRegistry, BlockLibraryRule.Flag, Library row drawer)
- [x] ArPlacementRule.RemovesOnSelection (pure) + ArPlacementService.SelectionChanged; PoiCardHost.Show / Close call it
- [x] Tests: pure rule both values; registry/library; real-window rows (block + Block Library default, undo); Phase B real taps (place, select another POI, gone / kept, no leak)
- [x] Learning summary + commit line

## 15.2.5 Dialogue hidden reply
- [x] DialogueRule.Problems names (row, slot); warning "Reply 3 has text but Choice 3 is empty: fill Choice 3 to see it" (first blocking choice when a lower one is empty too)
- [x] Real-window test
- [x] Learning summary + commit line

## 15.2.6 Close
- [x] Full EditMode + PlayMode green
- [x] _3.1 row 15.2 + TODOs, 10-structure.md, _5.1; __mixed_TODOs for stray ideas
- [x] Final report (60-finishing 6.4)
