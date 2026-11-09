# Block Components Lab: sources

The Lab is one HTML page joined from its modules. Where each feature lives, the rules that keep it that way and the order of
the modules: `PROTOTYPE_STRUCTURE.md`. Below: the words, the build, the tests and the keys.

- **The code:** `src/<area>/<module>.js` with its `.css`, in the order of `build.py`'s `MODULES` and `STYLES`; the page in `src/page/`,
  the pictures in `assets/`. Each module is described in `PROTOTYPE_STRUCTURE.md`.
- **Words:** `src/core/realstrings.json` (the framework's real words, with their owner part; `python strings.py` re-imports them
  from Unity's `CardStrings.asset`); the Lab's own words are in `src/core/words.js`'s `LABSTR`.
- **Build:** `python build.py` writes `block-components-lab.html` here (not committed). Copy it to `../Block Components Lab.html` (the
  record copy) and publish it to https://claude.ai/artifact/4LpLd4ZTgBEnjrXnhUMCVh.
- **Prove it:** `powershell -ExecutionPolicy Bypass -File tests/runtest8.ps1` opens the page headless with `#selftest`: every option,
  text and demo field must change the phone or be classed behaviour, and every row of the Guide tab and 6 · Audiences has its
  demo (it prints `bad=0` when all is well). It walks the tabs as three walks side by side and sums them (about 3 minutes);
  `-tabs guide` walks one tab (`#selftest=guide`; add `words` for the texts); `-single` is the old one walk, many times slower.
  `tests/shots8b.ps1` captures the demo card at each kind (`#dcshot=<kind>`); `#pshot=<stop>|<scroll>` opens the card at a
  stop and a scroll. `node run_tests.js` runs every test in `tests/` (not the `*_live` ones) and lists the failed checks:
  `--only=test8i,test9b` runs some, `--changed` the block's own (below); one file at a time (two at once slow each other past
  their timings), and a file that crashed at its start runs once more. Each test drives the page with real taps, drags and keys (Playwright with the installed Edge;
  `#probe` opens a handle on the page's state) and prints PASS / FAIL per check. Never run the self-test and `run_tests.js`
  together: each slows the other past its limits.
- **Levels: what a block of work runs** (decided 2026-11-06; a prototype's tests never cost more than building it). A block
  is one change the developer can look at (a feature, a fix, a family of rows).
  - **While building, look first** (`../../skills/Feature_Prototyping.md` step 3.3): after each change only `python build.py`
    (the check 0/0/0/0), then run it and capture the phone at 390 px, light and dark, and the Editor tab, open each capture
    and fix until the app and the Editor look premium and clean. No test file runs in this loop.
  - **Level 1, once a round's look is accepted (5-7 minutes; cut on 2026-11-07, `--changed` pulled 8-10 files, about
    30 min):** `node run_tests.js --only=<the round's new test file>,<the file of what it replaced>` (for example
    `test9h,test8g`), its mutation (`MUTATE=1`), and `tests/runtest8.ps1 -tabs <the tab it touched>` from PowerShell.
    - Add the test files of every module the round touched. A guidance round always touches `src/guidance/`, so its level 1
      runs test9e up to its own file (about 25 min; added 2026-11-09, after level 2 found round 8 breaking round 6's
      checks).
    - A walk holds one big tab alone: a walk's cost grows faster than its controls. Since round 7 the Guide tab alone is
      about 1300 controls (61 s), but `-tabs guide,words` took 65 min and `card,guide` hits its TIMEOUT. Run `-tabs guide`,
      `-tabs words` and `-tabs card` as separate calls.
    - `--changed` only lists the other files a round touches; they wait for level 2.
    - The pure logic checks run in Node, with no browser, where that is simple.
  - **Level 2, every 3rd-4th round, before its publish:** `tests/runtest8.ps1` (every tab, about 2 min), then
    `node run_tests.js` (every file, about 30-34 min). It also runs sooner when a round changed existing shared code
    (`core/`, `editor/shell.js`, `editor/card-tab.js`…, the build and test tools). New words only in `core/words.js` are
    covered by `-tabs words`.
  - **Level 3, every 10-12 rounds and before a hand-off to Unity:** level 2, then every mutation the tests name, and the
    results replace `tests_baseline.json`.
  - A wider level confirms, never discovers: green on 1 before 2, on 2 before 3. A round's report names the level it closed on
    and the rounds since the last level 2 and 3.
- **The tests, by feature:**
  - *The sheet and the card's chrome* (`phone/sheet`, `phone/grabber`, `phone/redraw`, `card/chrome-pinned`, `card/locate`):
    `tests/test8f.js` checks the sheet's gestures, the pinned close and settings, the demo card's X, and that a change moves no
    frame.
  - *The demo card and the hints* (`card/demo-card`, `card/hints`): `tests/test8g.js` checks the demo's flow, each hint (its
    moment, Off, Reduce Motion, one at a time: through Guidance's scheduler since round 4), Card Controls > Hints as a view, and How to use the card.
  - *Loading & states* (`card/states`): `tests/test8h.js` shows every state in every look, light and dark, and measures its
    words' contrast.
  - *Audiences* (`audiences/audiences`): `tests/test8l.js` checks the overrides and their fallback, the review rule, the looks,
    the guide's length, Settings, the first card and the coverage. *Lenses* (`audiences/lenses`): `tests/test8m.js` checks the
    chips, what steps aside, the heading rule, the scroll that stays, the guide by lens and the tag table.
  - *The guide:* the read-along (`guide/readalong`, `guide/voice`): `tests/test8i.js` checks the voice that loads late, each
    light style, the glide, Reduce Motion and the words' contrast. Focus (`guide/focus`): `tests/test8j.js` checks the
    spotlight, the zoom to each spot, Azu's arm, a scroll, Reduce Motion and drawing a spot. The guide as a person
    (`guide/person`): `tests/test8n.js` checks the page, the introduction, pause and resume of the voice, a failing voice, the
    gesture mid-sentence, the asks, a scroll and the placing. The Guide Dock (`guide/dock`): `tests/test8o.js` checks the
    dock's heights, the bubble and the block's words, the talk button (a stand-in recogniser), the conversation, Classic and
    the words' contrast. The recorded voices and the Voice test (`guide/voice`, `guide/voice-test`): `tests/test8r.js` (needs
    two voice sets rendered by voicebench) checks the recorded playback, its word times, pause, mute, an unrendered line and
    the Voice test.
  - *The mascot:* Azu in 2D (`mascot/azu2d`): `tests/test8k.js` checks each gesture on Azu, the moods, the mouth, the moments
    and their limits, Calm and Solemn. The tile of light (`mascot/azu-gl`): `tests/test8q.js` (WebGL by SwiftShader) checks
    the shader drawing, its forms by state, the mouth, the peek, the walk and its arrival, Tile, Reduce Motion. Its styles
    (`mascot/tile-styles`): `tests/test8y.js` (the same) presses each style of the tile of light and counts the colours in the
    preview and in the guide's own Azu: each glaze's ink, the four patterns, the lights' warmth, the corners, Reduce Motion
    (with a mutation: a renderer that ignores the style turns seven checks red). The 3D Azu (`mascot/azu3d`):
    `tests/test8w.js` (WebGL by SwiftShader, needs the network for three.js) checks the source, the model's clips and morphs,
    its frame, each state's clip, the mouth, the blink, the peek, Reduce Motion, Import (the real GLB and a bare one) and the
    fallback with no network. The models (`mascot/models`): `tests/test8x.js` (the same) presses each card of Model: the model
    swaps in with its seven clips, four morphs and front arm, is drawn and fits its frame, waves on Play a clip, opens its
    mouth on A; a model seen before comes from the cache; Import still replaces the chosen one (with a mutation: a swap that
    does nothing turns eight checks red; `CAP=<dir>` keeps a capture of each model in the guide). Size and character
    (`mascot/size`): `tests/test8z.js` presses Size and a Character per audience (a mutation: a character never applied). The
    real mouth (`mascot/mouth`): `tests/test9b.js` checks that a recorded Azure line follows its curves on the audio's clock
    (tile of light and 2D tile), a live ElevenLabs answer opens by its loudness, a recorded set without curves from a file
    falls back to the letters (a mutation: a player that ignores the curves turns three checks red).
  - *The world:* Ask about the view (`world/camera-ask`): `tests/test8p.js` checks the pill, the pins, the context, each
    suggestion, the hunt, moving the phone, the model's tools and a card opened from a pin. The test wall and the guide's
    button (`world/wall`, `world/guide-button`): `tests/test8s.js` checks the test wall, the pan and zoom, Azu's remarks and
    their taps, the button's rules and the rise to Full. The markers on the picture (`world/markers`): `tests/test8v.js`
    checks the markers at rest, after a real drag and a zoom, the Ribeira wall's points, the Wall button and the markers'
    scene in the console (with a mutation: markers left where they were turn it red).
  - *Ask Azu, the live agent* (`agent/agent`, `agent/tools`, `agent/console`): `tests/test8t.js` checks the agent with a test
    double and with fetch faked for Gemini and the Claude API (no network): each policy, wrong ids, citations, the fallbacks,
    the wall, the Editor and the console; `tests/test8t_live.js [dir]` asks the real model with this computer's key (by hand
    only: it spends the quota). Its test scenes (`agent/scenes`): `tests/test8u.js` checks the trace, the names on the points,
    the remark that follows the camera and the test scenes with their checks; `tests/test8u_live.js [dir]` presses Run all
    scenes with the real model, then drags the wall and types a question, as the developer does by hand. The live answers'
    voice (`agent/live-voice`): `tests/test9a.js` checks it with ElevenLabs faked (a real MP3, even letter times) and the Azure
    Speech SDK faked: the request, the audio as a live line, the words, the mouth on the voice's shapes, the tour's lines
    untouched, a refused key falling back to the device voice (a mutation: a mouth that ignores the voice turns two checks
    red). The order of sources (`guide/voice`, `agent/live-voice`): `tests/test9c.js` (ElevenLabs and Azure
    faked, no network) checks the default (Recorded, then live, then this device), a recorded line from its file, a line the set
    lacks by the live voice with a key and by the device voice without, a refused key, Voice = Device voice, Speaking now's words
    for each, pause / resume / mute / stop on a recorded, a live (also while it is being made) and a device line, the
    scroll-follow pause, and each answer once in the conversation (`MUTATE=1 node tests/test9c.js` breaks the order: 17 checks
    turn red). The guide's controls (`guide/dock`, `guide/controls`, `world/guide-button`, `editor/demo`, `guide/guide-scenes`):
    `tests/test9d.js` presses each control as a finger does: Pause and Stop at Mini, Compact by a swipe and back by the ✦, the hide
    chevron, Stop's toast and its Resume (the step and the sentence), Ask first, Previous replaying the sentence, the ✦ and every
    item of its menu (on the card and on the wall), a tap on Azu (it plays; the menu or the hint only when asked), the hint at most
    once a visit, the label folding after 4 s; each new Editor row's values on the phone; every row of the Guide tab and 6 · Audiences
    showing its demo, and the self-test failing a row without one; the contrast, light and dark; the seven scenes in Ask Azu's
    console; and the fixes (Azu 0 px over words, the mouth of a paused recorded line, the wall's answers in the one order).
    `MUTATE=1 node tests/test9d.js` makes the toast's Resume start the tour again from the start: the named checks turn red.
  - *The Guide tab and the guidance, round 1* (`editor/guide-tab`, `guidance/catalog`, `guidance/scheduler`, `guidance/service`,
    `guidance/present`, `guidance/editor`, `guidance/scenes`): `tests/test9e.js` checks the scheduler's `gdPick` alone on a fake clock (each
    gate and rule by itself, deterministic), then on the page with the visit's clock moved by hand: the opening once a visit (its look: 2
    lines, 7:1, 0 px over words, not under the ✦, light, dark and in Portuguese); Tap a marker 20 s after the opening and only once the
    phone has been still 1.2 s, never during a real drag on the wall, beside the nearest marker (Azu pointing, the ring 3:1 on the wall's
    picture), again after 25 s, at most 3 a visit, never after a card opens (this visit and the next, until Reset the visitor), never at
    the same time as Azu's remark; Mascot Off's plain bubble; Reduce Motion; the wall's pill gone and Global Scene's link; the Guide tab's
    sections and every former 6 · Guide row (the same 90 rows and 15 foldouts); each Guidance row's demo (the ×10 chip); Show next now and
    the visitor; the five scenes in Ask Azu's console. `MUTATE=1 node tests/test9e.js` makes the scheduler ignore Done when: the checks
    named [Done when] turn red.
  - *Guidance, round 2* (the same modules, `guide/tour`'s `tourStep.ended`, the `view.opened` of `world/guide-button` and `world/camera-ask`): `tests/test9f.js` checks each new gate, limit and learning rule alone on `gdPick` with a fake clock (a touch in the last 0.8 s, the card's first 1.5 s, the dock at Full, a system state, quiet in a tour but Between steps, never over its own target; at most a minute and a visit, a kind's cap and gap; three ignored ×2, a dismissed App tip ×3, a dismissed Content item's group resting, Fewer like this); the stage and the score (the kinds' weights at s = 0 / 0.3 / 1, Along the visit, freshness, fit, ties, deterministic, the mix); the learning and the memory on the page (Remember across visits Off, Leave for 31 min, Reset the visitor, each item's memory, the features used); the two new items (Ask about the view beside the ✦ and A tour of {point}: 2 lines, 7:1, 0 px over words, light, dark, Portuguese; spoken once the guide has been heard, its word lit; their taps); the bubble's tap, swipe and long press by the mouse; the 15-minute script (App first, then Content; at most 2 a minute; none over a sentence); Why not now? word for word the scheduler's; every new Editor row's demo with its ×10 chip, the read-backs and the Log; the five new scenes in Ask Azu's console. `MUTATE=1 node tests/test9f.js` makes the score ignore the stage of the visit (the kinds' weights and Along the visit at ×1): the checks named [the stage] turn red.
  - *Guidance, round 3* (the same modules, `world/guide-button`'s `menu.opened`, `mascot/azu2d`'s `mascot.poked`, `world/wall`'s link row): Azu's remarks and the ✦ hint become items, their timers gone. `tests/test9g.js` checks each rule of the two items alone on `gdPick` with a fake clock (Content › What's in view, one item a point of the wall: settled in the middle 1.2 s, never while panning, once a visit for each point, only its own point and only in the middle, the group's gap 7 s standing in for Content's, at most 12 a visit, the group's On, the 3 lines in turn and a point with a card's own, spoken as the kind; App › The wall › The ✦: after 4 pokes or 25 s idle, Lively only, never Solemn, once a visit, done when the ✦ menu opens, wherever the ✦ is, never both it and Ask about the view in the visit's first minute, the Editor's order); a remark never within the breathing room of a tip, either way, and the yield (pure and on the page); the remark's look (2 lines, 7:1, 0 px over words, light, dark, Portuguese), spoken with its word lit, its taps (a point with a card: its tour; any other: Ask about the view about it); The ✦ after 4 real clicks on Azu (not after 3, once a visit), after 25 s idle (a touch or the phone moving starts it again); Tap on Azu = Points to the ✦ at once (a pull, the tip showing gives way); the ✦ menu ending it for good, and a tap on its bubble opening the menu; the old timers gone; Ask › Ask about the view › Azu comments as a link row (its On / Off the group's, its button to What's in view); every new row's demo with its ×10 chip, the read-backs; Why not now? and the Log naming each point's remark; the words and their owners; the two new scenes in Ask Azu's console. `MUTATE=1 node tests/test9g.js` makes the scheduler ignore the breathing room for the remarks: the checks named [breathing] turn red.
  - *Guidance, round 4* (the same modules, `card/hints`, `card/locate`, `phone/grabber`, `world/guide-button`'s How does this work?): the card's Hints become App › The card (cues) and How to use the card becomes App › How to use the card (a walkthrough, pulled only). `tests/test9h.js` checks each cue's rules alone on `gdPick` with a fake clock (the Grabber nudge at the card's opening at Half, past the touch and the card's first 1.5 s, and only then; the Locate label once a visit or every card; the Navigation Bar, Before / After, Swipe strips and Underlined words at a pause with their element in view; a touch, a scroll, the card's first 1.5 s, a panel, a tour; Reduce Motion skipping the moving ones; never on the wall; the group's On; Done when a step of How to use the card is seen; a cue is not a tip for the tips' limits; the walkthrough's steps never the scheduler's); a cue and a tip never at once and the cue room (5 s) between them, either way, none between two cues (pure, and on the page: The ✦ on the card while a Before / After comes into view); How to use the card from Settings and from the ✦ (real clicks, Guidance Off: no gate stops a pull), a step seen retiring its cue; How does this work? on the card (the walkthrough) and on the wall (one bubble beside the nearest marker, 2 lines, 7:1, 0 px over words or the ✦, its chip Open a card 44 px, light, dark, Portuguese; the chip opens the card); its entries following the group; Card Controls > Hints as a view (Edited in Guidance, its links opening the group and a cue, no control of its own, reading Guidance's values) and the old names (`S.hints.on`, `S.ctl.locate.label`) as Guidance's values; every new row's demo (the cue on the card, the walkthrough from a step, the Cue room's ×10 chip), the read-backs, the words and their owners, the old Hints code gone; the two new scenes in Ask Azu's console. `MUTATE=1 node tests/test9h.js` lets the cues past the one-at-a-time gate: the scheduler's [at once] check turns red (the page's stay green: the service never asks the scheduler while a tip or a cue shows, a second lock).
  - *Guidance, round 5* (the same modules, `audiences/audiences`'s Guidance preset and `audience.set`): the simulator, the presets, Talkativeness, the Mix table, the audiences' preset and the ×10 demos. `tests/test9i.js` checks the Mix along the visit as data (a change moves `gdScore`, pure; the kinds' read-backs, the curve and the Editor's table follow); each Talkativeness level's numbers in force (the room, at most a minute and a visit, each kind's cap; on `gdPick` and in the rows' words); each preset's values (never the items), the Editor's Preset, Custom and its list, the help's tables; the audiences' Guidance preset (Family tapped in Who's exploring, Expert in Settings); each scripted visitor's timeline against its expected counts and order (Instant, in the page: First visit 12 tips in 15 min, App 5 then Content 7; Fast learner 10; Idle 9 with the room ×2 and ×4; Tour lover 7 and three tours; Expert 7, Content after Welcome back), deterministic, through the real `gdPick`; the timeline (lanes, dots apart or merged, it fits); a dot tapped by the mouse showing its item on the phone, a merged count's list, a drag scrubbing to the scheduler's own words, Play ×30; a change re-running the simulator and naming what moved, the old ×10 chip demos with the values in force, every new row's demo; the three scenes in Ask Azu's console. `MUTATE=1 node tests/test9i.js` makes the Mix table and Talkativeness ignored (always First visit's mix, always Balanced): the checks named [mix] and [talk] turn red.
  - *Guidance, round 6* (the same modules, `guide/focus`'s shared cut-out, `guide/dock`'s `dockThread.html`): the look and the full item form. `tests/test9j.js` checks the scheduler's new rules alone on `gdPick` (the visitor's Tips: All / Essentials only / Off; Only if: the audience, a returning visit, has / has not used, the latest second; When on an event; After a dismissal; Must be on screen No); Says (variants in turn, never the same twice in a row; an audience's line; the returning visitor's; `{point}` `{year}`); each Focus (None, Glint, Ring, Spotlight at the item's Dim, the marker and its name in the cut-out) and the Spotlight offered only for Essentials and walkthrough steps (disabled with its reason, a Ring when not allowed, said in the read-back); the hop (a 0.45 s arc) and Walks to it (their end beside the marker, 0 px over it or its words); the ghost finger (a tap, 0 px over words, the marker or the bubble; Off; a drag across a Before / After; a swipe on a strip); each Button chip's action by a real click (Show me, Start the tour, Open the card, Ask about the view; 44 px); Reduce Motion's still versions; Tips stay until tapped (30 s later still there, its 44 px close button); the screen-reader stand-in (announced, never spoken); the dock's Tips (✦ › Ask, real clicks); Settings › The guide › Tips and Show the tips again with its Undo toast (no popup); every button, select and number of the item form pressed once by a real click, and its own buttons (a variant, an audience's line, a returning line, a chip's ×, ▸); each named warning made to appear, then fixed (kind Off, Only if contradicting itself, Done when already true, a line too long in Portuguese, a line said twice, no words), with its badge; each template; Duplicate, Delete and Undo; the drag reorder (a real drag) changing the tie-break and an In order group's sequence; every new row's demo; the four scenes in Ask Azu's console. `MUTATE=1 node tests/test9j.js` offers the Spotlight on any item and makes Delete's Undo do nothing: the checks named [spot] and [undo] turn red.
  - *Guidance, round 7* (the same modules, `mascot/azu2d`'s Moments and jokes, `world/camera-ask`'s hunt, `world/guide-button`'s band, Specific Marker › Guide script › Jokes): Fun. `tests/test9k.js` checks the Moments through the scheduler (between steps only, Lively only, never on a Solemn point, once a card, 5 min apart; the old timers gone: no Moment without the scheduler); a joke after pokes (real clicks on Azu), its variants rotating (the least seen); a point's joke when it settles, none for a point without one; the treasure hunt invitation (Family only, after a card, once a visit; its chip starts the real hunt; done when found); each seasonal window with Today is, and the 1st of November never a joke; the Solemn gate for every Fun item with Why not now?'s words; Calm and Lively only; the Fun lane and the sixth visitor's timeline (Instant, in the page); Guide › Mascot's link rows; the ✦ fix (0 px over words on the card past the hero); every new row's demo; the five scenes in Ask Azu's console. `MUTATE=1 node tests/test9k.js` removes the Fun kind's Solemn gate: the checks named [solemn] turn red. `test8k` follows the Moments and the joke into the guidance (its timers became items).
  - *Guidance, round 8* (the same modules, `audiences/audiences`'s Where to ask and `audience.asked`): the first pages (the Page form), Start the app, Who's exploring's Where to ask. `tests/test9l.js` checks the scheduler's gate and the pages' data alone (the order's ends, a first run's pages for each Where to ask and the camera's permission, the Opening message's short line, the run under 30 s); the run in order by real taps (Start the app on the preview bar, Hello, the language chip switching the app's language, Let's go, Who's exploring with Family setting the Family preset, the camera and the phone's request, the wall with the short line, then Tap a marker as round 1; the scheduler quiet meanwhile); each Skip and skipping all; Show me how it works (How to use the card's bubble says the visit's first words); the camera allowed, refused (never the wall, 30 s later), Try again, Open Settings; 44 px, 4.5:1 and 0 px over the words, light and dark; Where to ask's three values by real clicks (the first card asking or not); a later launch (no pages, Welcome back) and Remember across visits Off (the pages again); Reduce Motion and Mascot Off; Preview & Test's Start the app and the visitor's line; + Add guidance's Page template and the drag within the rules (real drags); every new row's demo; the four scenes in Ask Azu's console. `MUTATE=1 node tests/test9l.js` lets a refused camera go on to the wall: the checks named [camera] turn red.
- **The live voice by hand:** with the keys on this computer, open the page from its file (not `#probe`), set Ask Azu >
  Voice of the answers, and ask; it spends a few hundred characters an answer.
- **Keys:** the local Lab's live agent and live voice read `keys.local.js`, which `python keys_local.py` writes from the workspace's
  `.secrets/keys.env` (git ignores both); `python keys_local.py --check` asks each service whether its key works. The headless
  checks (`#probe`) never load it; `#probe-live` does. See `../../__Services_And_Keys.md`.
- **The audit** that shaped the card's merged options: `AUDIT_DECISIONS.md` and `audit/` (one report per area).
- A change: make it in the feature's own place (`PROTOTYPE_STRUCTURE.md`, "Where a change goes"), rebuild, run the self-test
  and `node run_tests.js`, look at a capture, then fold the decision into `_3.3`.
