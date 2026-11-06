You are building round 2 of the guidance system in a browser prototype, the "Block Components Lab", in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app;
every feature must be buildable in Unity (name its path). Only the Lab: no Unity project file is touched.

READ FIRST (all lines): `../../_3.8_Guidance_Study.md` (the decided design: this round is section 8, round 2; the model is
sections 3.1-3.4; the decisions section 9); `PROTOTYPE_STRUCTURE.md` (rules, modules, "Where a change goes", the row-demo
contract); `README.md` ("Prove it" and "Levels": how this round runs its tests); `../../skills/Feature_Prototyping.md` step 5;
round 1's modules, which this round extends in place: `src/guidance/catalog.js`, `scheduler.js`, `service.js`, `present.js`,
`editor.js`, `scenes.js`, and `tests/test9e.js`; `src/editor/guide-tab.js`, `src/editor/demo.js`, `src/world/camera-ask.js`,
`src/guide/tour.js` (startTour, the steps), `src/agent/scenes.js` (the console's scenes). Round 1's brief, for the shape:
`brief_round1.md`. The spec `../../_3.3_POI_Card_Styles.md` section 11 "Guidance": read; do not edit (the Architect does).

## This round (numbered blocks: run level 1 after each one, README "Levels")

**2.1 The memory and the visit.** Per item: shown this visit / ever, last shown, the line used, done and by what, dismissed,
acted. Per visitor: visits, active minutes, met the guide (the tour's introduction heard), the features used (the events of
study 3.2 the Lab has: card opened, tour step ended, a question asked, Ask about the view opened). A visit ends after 30 min
away (in the Lab: Preview & Test > The visitor > **Leave for 31 min**, which moves the visit clock); **Remember across visits**
On / Off (Off: every visit starts as a first visit). The bubble's controls (study 3.4): a tap does its action and counts as
acted; a swipe away or letting it time out counts as dismissed / ignored; a long press offers **Fewer like this?** (its group
rests for the visit).

**2.2 The gates and the limits** (study 3.3, 1-2). Add the gates round 1 lacks: a touch in the last 0.8 s, the card's first
1.5 s, the dock at Full, a system state showing (Loading & states), never over its own target or the block being told, quiet
in a tour except items allowed Between steps (a field **During a tour**: Never / Between steps). Limits as Overall rows: **At
most a minute** (2), **At most a visit** (30); each kind's own **cap** and **gap** (kind rows). Each gate and limit gives its
reason in words, as round 1's do.

**2.3 The stage of the visit and the score** (study 3.3, 3-4). `s = max(active minutes / 15, share of the milestones done)`
with the milestones above; a returning visitor starts at 5 min. Score = importance × along-the-visit(s) × kind weight(s) ×
group weight × freshness × fit; kind weights at s = 0 / 0.3 / 1: App 70 / 40 / 15, Content 25 / 50 / 60 (a fixed table this
round; round 5 makes it the editable Mix table); freshness `1 − e^(−t/τ)` with τ an Overall row **Freshness recovers over**
(2 min); fit ×1.5 when the target is in the middle, ×1.3 when the item Teaches an open App tip. **Along the visit** per item:
As the kind / Steady / Early / Late. Ties: the Editor's order. Deterministic.

**2.4 Learning from the visitor** (study 3.3, 5): three ignored in a row double the breathing room (up to ×4) for the visit,
an acted item resets it; a dismissed App item waits ×3 its Again after; a dismissed Content item rests its group 5 min;
Fewer like this? rests the group for the visit.

**2.5 Two new items**, so the 15-minute script can show App first, then Content (do not convert Azu's wall remarks, the ✦ hint
or the card Hints: those are rounds 3-4):
- App › **The wall** › **Ask about the view** ("Ask me about anything you see." / PT per Languages & Terms): about the Ask
  about the view pill, Teaches and Done when "Ask about the view opened", Up to 2 a visit, Again after 90 s, not before 60 s
  of the visit.
- The kind **Content** (Spoken: once the visitor has heard the guide this visit, else a bubble, decision 2) with the group
  **Tours** and the item **A tour of {point}** ("{point} has a story. A tour of {min} minutes?"): about the point in the
  middle, a tap starts the tour of that point (the Lab's startTour), Once a visit, Along the visit Late, Done when "tour step
  ended". The slots `{point}` `{min}` filled from the point in view.
Every word in Languages & Terms with its owner (the item's foldout), en and pt.

**2.6 The Editor** (Guide > 4 · Guidance): 0 · Preview & Test: Show next now (round 1), **Why not now?** (pick an item: its
reason in words, the same words the scheduler gives), **The visitor** (first / returning, visits, active minutes, the
features used, Leave for 31 min, Reset the visitor), **Log** (the last 30 decisions: the visit time, shown / skipped / ended,
and why). 1 · Overall: the new rows above. 2 · App and 3 · Content: the kind row (On, cap, gap, the weight read back in
words), the groups, each item as round 1's rows, plus the new fields (During a tour, Along the visit). Every new row has its
demo (`rowDemos`); a limit or a learning row shows its effect on a ×10 clock on the phone with a countdown chip, as round 1's
Again after does. No placeholder rows (nothing that says "a later round").

**2.7 Scenes in Ask Azu's console** the developer can press (their rule): "15 minutes in" (a scripted first visit at ×30:
the App items first, then the tour invitation once the stage has grown; the log shows each pick and why), "Ignore three"
(the breathing room doubles), "Fewer like this", "Leave and come back" (the returning line, the stage from 5 min), "During
a tour" (nothing but Between steps items). Each with its ✓ check, like round 1's five.

## Design pass first (look, then tests)
Captures at 390 px, light and dark: the Ask about the view tip beside its pill, the tour invitation (a bubble, and spoken
after the guide has been heard), Fewer like this? on a long press, the Editor's Why not now?, The visitor, the Log, the
Overall and kind rows. Open each yourself; judge one focal point, nothing covering its target or other words (0 px over
text), text 4.5:1 or more, 44 px targets, the bubble not under the ✦ or the nav bar. Fix and recapture. Save the final
captures in `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\cce29d1a-ef01-45c5-860e-cad7113106b6\scratchpad\g2\`.

## Tests
`tests/test9f.js` (Playwright, the installed Edge, the page's visit clock moved by hand as test9e does): each new gate, limit
and learning rule alone on a fake clock (a pure check of `gdPick`, as test9e's section 1); the score and the stage (the
weights at s = 0 / 0.3 / 1, freshness, fit, ties); the memory (Remember across visits Off, Leave for 31 min, Reset); the two
new items on the page (their look: 2 lines, 7:1, 0 px over words); the bubble's tap / swipe / long press; the 15-minute script
(App first, then Content; at most 2 a minute; none over a sentence); every new Editor row's demo; Why not now? equals the
scheduler's reason; the five scenes. A mutation (`MUTATE=1`: e.g. the kind weights ignored, so the script's "App first, then
Content" check turns red), then restore. Round 1's `test9e` stays green unchanged; if a round-1 check must change (a value
this round's study corrects), list it and why.

## Proof (README "Levels")
After each block 2.1-2.7, level 1: `python build.py` (the check 0/0/0/0); `node run_tests.js --changed`; the self-test of the
Guide tab (`powershell -ExecutionPolicy Bypass -File tests/runtest8.ps1 -tabs guide,words`). To close the round, level 2:
`tests/runtest8.ps1` (every tab, about 3 min) `bad=0` (say how ok / behaviour moved and why), then `node run_tests.js` (every
file, about 30 min) all green. Never run the self-test and run_tests.js at the same time. A start crash that the runner's
retry does not clear: rerun that file alone. Update `PROTOTYPE_STRUCTURE.md` (the modules' rows and their tests column, so
`--changed` finds test9f), `README.md` (test9f) and `tests_baseline.json`.

## Limits
Edit only files under lab_src. Do not commit, do not publish. No keys, voicebench, mascot model files or Unity. No CSS-only
effects (name each one's Unity path in the report). One home per feature: extend round 1's modules, no wrappers, no
re-assignment of another module's function (the build refuses them). Patch scripts with the Write tool (bash heredocs mangle
apostrophes). If a usage limit stops you, your work is saved on disk; say in your last message which blocks are done and
which level each passed.

## Report (your final message)
What was built, block by block, with Unity paths; the new rows and their defaults; the captures (paths) and what you judged and
fixed; the tests, the mutation, any test you changed and why; the level 1 results per block; the self-test line; the run_tests
summary; 3-5 specific questions for the developer about the look and the flow.
