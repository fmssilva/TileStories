# Plan tracker -- _3.3: the card's resting end (Finished) + the spike out of the full run

Base: HEAD c4b3f3a (4c2: full PlayMode 1322 -- 1 spike timing red, green alone -- full EditMode 2115). Do not commit (the Architect does).

## GATE
- [x] push: 0 ahead, nothing to push
- [x] captures: TestEvidence/Gate/gate_4c3_game_view.png + gate_4c3_poi_editor.png (two desktop reads caught VS Code, deleted; window moved
      to where Unity shows)
- [x] compile: ExitCode 0, 0 error CS
- [x] fonts: Oswald / Roboto SDF and Dev/StyleSpike/Fonts unmodified

## Decision (Part 2, to confirm by Part 1's numbers)
The rule (NavScrollRule) stays: it reads the ScrollView's real numbers. The CHROME stops taking the end away:
- keep the end: when the bottom area comes back at the end, the stack follows the shrinking viewport (offset = the new Range)
- end zone: the chrome's "at the end" = the range WITHOUT the bottom area, so it never hides within its own height of the end (no loop)

## Steps
1. [x] Part 1: LookMeasure.RestingEnd + PoiCardRestingEndSceneTests' gated measure (Lamp + Painting x next_up / bottom_bar, Lamp footer
       alone): viewport / Range hidden + back, where small drags and a fling rest, IsAtEnd, Finished, spy vs last run, bar mark, ticks,
       ContentSeen, render. Phase "before" run (background).
2. [x] Part 2 rule: CardChromeRule.AtEnd(offset, content, viewport, bottomShownHeight) + KeepTheEnd(offset, content, vBefore, vAfter)
3. [x] Part 2 view: CardChromeView passes the area's height to AtEnd; keeps the end on the viewport's geometry change after the area
       comes back at the end
4. [x] EditMode (26/26; mutation: AtEnd ignoring the area + KeepTheEnd returning the offset -> the 4 new red on their own asserts): CardChromeRuleTests -- the chrome's two viewports, the end zone, a short last block, a tall last block (through
       NavScrollRule); mutation proof of both changes
5. [x] PlayMode Phase B (fix v2: the keep waits for a finger to lift -- v1 moved the stack back 214 px in the interleaved test): PoiCardRestingEndSceneTests -- real small drags + a fling to the end on both cards x both elements (+ footer):
       rests at Range, end above the area, Finished / bar mark / last run / ticks / ContentSeen; no hide-loop in the end zone
6. [x] Re-base PoiCardInterleavedSpySceneTests.AtTheEnd; re-check the P1b end tests (BottomElements x 2, NavScene final screen,
       Painting drag) -- do they read the visitor's end
7. [x] Part 3 (no switch: 26/26 skipped "Ignored(Parent)" in 0.05 s; switch on: 26/26 passed, 44 s): StyleSpikeFixedFrameForTheRun gate (TestEvidence/StyleSpike/MEASURING); proven by running the spike assembly with
       and without it; 40-testing.md (full run + how to run it by hand)
8. [x] Runs (full EditMode 2119: 2118 + 1 gate, 0 failed; full PlayMode 1336: 1261 + 75 gated skips, 0 failed, 4364 s; both behind): full EditMode; targeted PlayMode fixtures (named); FULL PlayMode (background); measuring phase "after" + compare
9. [x] Docs (RUN TIMES after the full run): _3.3 Build log, _3.2 4.3 / 4.4 (the end's meaning), 10-structure (touched lines), 40-testing RUN TIMES if moved
10. [x] LivingRoomScene open; final report 6.4 + review questions

## Numbers (Part 1, old behaviour, TestEvidence/RestingEnd/before/measures.txt, Unity behind, read once the stack HELD STILL 60 frames)
- First reading (measures_before_original.txt) was taken 0.5 s after the last stroke: the stack was still moving (the ScrollView's release
  animation; a probe re-noted the offset after the render: 18461 -> 18608 by itself) -> the instrument now waits for HeldStill.
- At true rest, 2 of 10 walks fail, two ways:
  - Lamp + next_up, small strokes: content 19151, viewport 543 (690 away, area 147), Range 18608, rests at 18482.8 -> 125 short, the
    Sources' last 125 px under the area, IsAtEnd False; Next Up already says Finished 57 of 57 (block_62 above the tall Sources)
  - Painting + next_up, small strokes: rests at the end of the range with the area AWAY (viewport 690, bar off screen): the end never
    shows "You have reached the end"
- the other 8 (Lamp footer alone 81 px, Lamp bottom_bar 135, Painting bottom_bar 54, every fling) rest at Range with the area shown

## Re-base log (old -> new, from source)
- PoiCardInterleavedSpySceneTests.AtTheEnd: "Range less the bottom area when it came back" -> "Range" (the visitor's end now)
