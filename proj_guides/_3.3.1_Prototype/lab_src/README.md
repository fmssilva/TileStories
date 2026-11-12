# Block Components Lab: sources

The Lab is one HTML page joined from its modules. Where each feature lives, the rules that keep it that way and the order of
the modules: `PROTOTYPE_STRUCTURE.md`. Below: the words, the build, the tests and the keys.

- **The code:** `src/<area>/<module>.js` with its `.css`, in the order of `build.py`'s `MODULES` and `STYLES`; the page in `src/page/`,
  the pictures in `assets/`. Each module is described in `PROTOTYPE_STRUCTURE.md`.
- **Words:** `src/core/realstrings.json` (the framework's real words, with their owner part; `python strings.py` re-imports them
  from Unity's `CardStrings.asset`); the Lab's own words are in `src/core/words.js`'s `LABSTR`.
- **Build:** `python build.py` writes `block-components-lab.html` here (not committed). Copy it to `../Block Components Lab.html` (the
  record copy) and publish it to https://claude.ai/artifact/Gpjf7yvoYHjGebmmNhuw2o.
- **Prove it:** `powershell -ExecutionPolicy Bypass -File tests/runtest8.ps1` opens the page headless with `#selftest`: every option,
  text and demo field must change the phone or be classed behaviour, and every row of every tab has its demo but the named
  exemptions (`ROW_DEMO_EXEMPT`, each with its reason; guidance round 10) (it prints `bad=0` when all is well). It walks the tabs as three walks side by side and sums them (about 14 minutes at round 17b);
  `-tabs guide` walks one tab (`#selftest=guide`; add `words` for the texts); `-single` is the old one walk, many times slower.
  `tests/shots8b.ps1` captures the demo card at each kind (`#dcshot=<kind>`); `#pshot=<stop>|<scroll>` opens the card at a
  stop and a scroll. `node run_tests.js` runs every test in `tests/` (not the `*_live` ones) and lists the failed checks:
  `--only=test8i,test9b` runs some, `--core` the core set and `--changed` lists a round's files (below, "Levels"); one file at a time (two at once slow each other past
  their timings), and a file that crashed at its start runs once more. Each test drives the page with real taps, drags and keys (Playwright with the installed Edge;
  `#probe` opens a handle on the page's state) and prints PASS / FAIL per check. Never run the self-test and `run_tests.js`
  together: each slows the other past its limits.
- **Levels: what a round runs** (the developer, 2026-11-12: "Feature tests per round, each 4 rounds core tests, each 12 rounds full
  tests"; it replaces the levels 1 to 3 of 2026-11-06 to 11-09). A prototype's tests never cost more than building it. A round is one
  change the developer can look at (a feature, a fix, a family of rows).
  - **While building, look first** (`../../skills/Feature_Prototyping.md` step 3.3): after each change only `python build.py`
    (the check 0/0/0/0), then run it and capture the phone at 390 px, light and dark, and the Editor tab, open each capture
    and fix until the app and the Editor look premium and clean. No test file runs in this loop.
  - **Feature tests, every round (about 5-15 min):** the round's own test file(s) and their mutation (`MUTATE=1`), the self-test
    of the tab it touched (`tests/runtest8.ps1 -tabs <tab>`, from PowerShell), and at most 1-2 more files the brief names:
    `node run_tests.js --only=<the round's files>,<the named ones>`. Never "the files of every module you change", never
    `--changed`'s pull (`--changed` only lists the files a round touches; they wait for the core tests).
    - A walk holds one big tab alone: a walk's cost grows faster than its controls (the Guide tab alone is about 1300
      controls, 61 s, but `-tabs guide,words` took 65 min). Run `-tabs guide`, `-tabs words` and `-tabs card` as separate calls.
    - The pure logic checks run in Node, with no browser, where that is simple.
  - **Core tests, every 4 rounds (about 30 min):** `tests/runtest8.ps1` with no `-tabs` (every walk side by side, about 14
    min at round 17b: the card walk is the long one; bad=0), then `node run_tests.js --core`: the core set, the files that test the shared modules (`core/`, the phone and
    the card's chrome, the Editor's shell, the guide's engine: the tour, the dock, the person, the tour from the card, the saved
    place), about 18 min at round 17b's timings (25 min at the baseline's). `run_tests.js --core` reads this list (one list, in one place):
    <!-- core set -->
    - `test8f` (81 s): the phone's sheet, the grabber and the card's chrome (`phone/sheet`, `phone/grabber`, `card/chrome`):
      every screen sits in them; a change must move no frame.
    - `test8i` (36 s): the tour's read-along (`guide/tour`, `guide/readalong`, `guide/voice`): every step's words lit as said.
    - `test8n` (55 s): the guide as a person (`guide/person`): the introduction, pause and resume, the step placed above the panel.
    - `test8o` (37 s): the Guide Dock (`guide/dock`): its heights, the bubble, the talk button, the words' contrast.
    - `test9da` (194 s): the tour's controls (`guide/controls`, `world/guide-button`): Stop, its toast and the one offer back into
      a stopped tour, Previous, the ✦ and its menu on the card and the wall.
    - `test9n` (87 s): the Editor's shell (`editor/shell`, `editor/demo`, `editor/card-tab`): every row of every tab has its demo
      (NO DEMO), a sample of each kind of row by real clicks.
    - `test9r` (71 s): the Guide tab (`editor/guide-tab`): its six sections, every row key and foldout kept, the levels, the draw time.
    - `test9se` (201 s): the shared words (`core/words`' `nameIn`: a name's article mid-sentence) and Mascot Off's ✦ alone.
    - `test9t` (271 s): the tour from each card (`guide/autotour`, `core/content`'s `KWORDS` and `POINTS`, `card/blocks`'
      `blockSays`): the four points' tours, EN and PT, no fact twice, the links, the intro lines, the teaser, the play by real taps.
    - `test9u` (229 s) and `test9ub` (240 s): the saved place and Tell me more (`guide/saved-place`, `guide/tell-more`): the one
      offer in each place, Continue within a visit and on the next, the deep parts, the pace, expiry, a changed tour, blocked storage.
    <!-- /core set -->
  - **Full tests, every 12 rounds and before a hand-off to Unity:** `tests/runtest8.ps1`, then `node run_tests.js` (every file,
    about 2 h at round 17's timings), then every mutation the tests name (`MUTATE=1`); the results replace `tests_baseline.json`
    (a new baseline).
  - **Every file stays under 300 s**, so a full run stays bounded: a file over it is split by its own sections, the same checks and
    mutations moved, never dropped (round 10: test9d into test9da and test9db; round 16: test9se's [azu] into test9sf; round 17b:
    test9db into test9db and test9dc, test9e into test9e and test9ea, test9sf into test9sf and test9sg).
  - A wider tier confirms, never discovers: green on the feature tests before the core tests, on the core before the full. A
    round's report names the tier it closed on and the rounds since the last core and full tests.
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
    round 9's test9d, split in guidance round 10 by its own sections (it took 570 s of its 600 s), every check kept; test9db split again in
    round 17b (530 s): `tests/test9dc.js` presses the second half of the rows' labels (the same code, `HALF=2`).
    `tests/test9da.js` presses the controls as a finger does: Pause and Stop at Mini, Compact by a swipe and back by the ✦, the hide
    chevron, Stop's toast and its Continue the tour (step N of M) (round 17b: the one offer; the step and the sentence), Ask first, Previous replaying the sentence; the ✦ and every item of
    its menu (on the card and on the wall), a tap on Azu (it plays; the menu or the hint only when asked), the hint at most once a visit,
    the label folding after 4 s; the contrast, light and dark; the seven scenes in Ask Azu's console; and the fixes (Azu 0 px over words,
    the mouth of a paused recorded line, the wall's answers in the one order). `MUTATE=1 node tests/test9da.js` makes the toast's Resume
    start the tour again from the start: the named checks turn red. `tests/test9db.js`: each new Editor row's values on the phone; every
    row of the Guide tab and 6 · Audiences showing its demo (a press on each label, a change), and the self-test failing a row without one.
  - *The Guide tab and the guidance, round 1* (`editor/guide-tab`, `guidance/catalog`, `guidance/scheduler`, `guidance/service`,
    `guidance/present`, `guidance/editor`, `guidance/scenes`): `tests/test9e.js` checks the scheduler's `gdPick` alone on a fake clock (each
    gate and rule by itself, deterministic), then on the page with the visit's clock moved by hand: the opening once a visit (its look: 2
    lines, 7:1, 0 px over words, not under the ✦, light, dark and in Portuguese); Tap a marker 20 s after the opening and only once the
    phone has been still 1.2 s, never during a real drag on the wall, beside the nearest marker (Azu pointing, the ring 3:1 on the wall's
    picture), again after 25 s, at most 3 a visit, never after a card opens (this visit and the next, until Reset the visitor), never at
    the same time as Azu's remark; Mascot Off's plain bubble; Reduce Motion; the wall's pill gone and Global Scene's link; the Guide tab's
    sections and every former 6 · Guide row (the same 90 rows and 15 foldouts); each Guidance row's demo (the ×10 chip); Show next now and
    the visitor; the five scenes in Ask Azu's console. `MUTATE=1 node tests/test9e.js` makes the scheduler ignore Done when: the checks
    named [Done when] turn red. Split in round 17b (415 s): `tests/test9ea.js` runs its sections 10 and 11 (the Guide tab, 4 · Guidance's rows,
    defaults and demos; the same code, `PART=2`).
  - *Guidance, round 2* (the same modules, `guide/tour`'s `tourStep.ended`, the `view.opened` of `world/guide-button` and `world/camera-ask`): `tests/test9f.js` checks each new gate, limit and learning rule alone on `gdPick` with a fake clock (a touch in the last 0.8 s, the card's first 1.5 s, the dock at Full, a system state, quiet in a tour but Between steps, never over its own target; at most a minute and a visit, a kind's cap and gap; three ignored ×2, a dismissed App tip ×3, a dismissed Content item's group resting, Fewer like this); the stage and the score (the kinds' weights at s = 0 / 0.3 / 1, Along the visit, freshness, fit, ties, deterministic, the mix); the learning and the memory on the page (Remember across visits Off, Leave for 31 min, Reset the visitor, each item's memory, the features used); the two new items (Ask about the view beside the ✦ and A tour of {point}: 2 lines, 7:1, 0 px over words, light, dark, Portuguese; spoken once the guide has been heard, its word lit; their taps); the bubble's tap, swipe and long press by the mouse; the 15-minute script (App first, then Content; at most 2 a minute; none over a sentence); Why not now? word for word the scheduler's; every new Editor row's demo with its ×10 chip, the read-backs and the Log; the five new scenes in Ask Azu's console. `MUTATE=1 node tests/test9f.js` makes the score ignore the stage of the visit (the kinds' weights and Along the visit at ×1): the checks named [the stage] turn red.
  - *Guidance, round 3* (the same modules, `world/guide-button`'s `menu.opened`, `mascot/azu2d`'s `mascot.poked`, `world/wall`'s link row): Azu's remarks and the ✦ hint become items, their timers gone. `tests/test9g.js` checks each rule of the two items alone on `gdPick` with a fake clock (Content › What's in view, one item a point of the wall: settled in the middle 1.2 s, never while panning, once a visit for each point, only its own point and only in the middle, the group's gap 7 s standing in for Content's, at most 12 a visit, the group's On, the 3 lines in turn and a point with a card's own, spoken as the kind; App › The wall › The ✦: after 4 pokes or 25 s idle, Lively only, never Solemn, once a visit, done when the ✦ menu opens, wherever the ✦ is, never both it and Ask about the view in the visit's first minute, the Editor's order); a remark never within the breathing room of a tip, either way, and the yield (pure and on the page); the remark's look (2 lines, 7:1, 0 px over words, light, dark, Portuguese), spoken with its word lit, its taps (a point with a card: its tour; any other: Ask about the view about it); The ✦ after 4 real clicks on Azu (not after 3, once a visit), after 25 s idle (a touch or the phone moving starts it again); Tap on Azu = Points to the ✦ at once (a pull, the tip showing gives way); the ✦ menu ending it for good, and a tap on its bubble opening the menu; the old timers gone; Ask › Ask about the view › Azu comments as a link row (its On / Off the group's, its button to What's in view); every new row's demo with its ×10 chip, the read-backs; Why not now? and the Log naming each point's remark; the words and their owners; the two new scenes in Ask Azu's console. `MUTATE=1 node tests/test9g.js` makes the scheduler ignore the breathing room for the remarks: the checks named [breathing] turn red.
  - *Guidance, round 4* (the same modules, `card/hints`, `card/locate`, `phone/grabber`, `world/guide-button`'s How does this work?): the card's Hints become App › The card (cues) and How to use the card becomes App › How to use the card (a walkthrough, pulled only). `tests/test9h.js` checks each cue's rules alone on `gdPick` with a fake clock (the Grabber nudge at the card's opening at Half, past the touch and the card's first 1.5 s, and only then; the Locate label once a visit or every card; the Navigation Bar, Before / After, Swipe strips and Underlined words at a pause with their element in view; a touch, a scroll, the card's first 1.5 s, a panel, a tour; Reduce Motion skipping the moving ones; never on the wall; the group's On; Done when a step of How to use the card is seen; a cue is not a tip for the tips' limits; the walkthrough's steps never the scheduler's); a cue and a tip never at once and the cue room (5 s) between them, either way, none between two cues (pure, and on the page: The ✦ on the card while a Before / After comes into view); How to use the card from Settings and from the ✦ (real clicks, Guidance Off: no gate stops a pull), a step seen retiring its cue; How does this work? on the card (the walkthrough) and on the wall (one bubble beside the nearest marker, 2 lines, 7:1, 0 px over words or the ✦, its chip Open a card 44 px, light, dark, Portuguese; the chip opens the card); its entries following the group; Card Controls > Hints as a view (Edited in Guidance, its links opening the group and a cue, no control of its own, reading Guidance's values) and the old names (`S.hints.on`, `S.ctl.locate.label`) as Guidance's values; every new row's demo (the cue on the card, the walkthrough from a step, the Cue room's ×10 chip), the read-backs, the words and their owners, the old Hints code gone; the two new scenes in Ask Azu's console. `MUTATE=1 node tests/test9h.js` lets the cues past the one-at-a-time gate: the scheduler's [at once] check turns red (the page's stay green: the service never asks the scheduler while a tip or a cue shows, a second lock).
  - *Guidance, round 5* (the same modules, `audiences/audiences`'s Guidance preset and `audience.set`): the simulator, the presets, Talkativeness, the Mix table, the audiences' preset and the ×10 demos. `tests/test9i.js` checks the Mix along the visit as data (a change moves `gdScore`, pure; the kinds' read-backs, the curve and the Editor's table follow); each Talkativeness level's numbers in force (the room, at most a minute and a visit, each kind's cap; on `gdPick` and in the rows' words); each preset's values (never the items), the Editor's Preset, Custom and its list, the help's tables; the audiences' Guidance preset (Family tapped in Who's exploring, Expert in Settings); each scripted visitor's timeline against its expected counts and order (Instant, in the page: First visit 12 tips in 15 min, App 5 then Content 7; Fast learner 10; Idle 9 with the room ×2 and ×4; Tour lover 7 and three tours (round 17b: five, and the Fast learner two: since round 16 the Torreão, the Terreiro do Paço and the Tagus have a card, so a tapped remark about one starts its tour; the app changed on purpose, the expectation follows); Expert 7, Content after Welcome back), deterministic, through the real `gdPick`; the timeline (lanes, dots apart or merged, it fits); a dot tapped by the mouse showing its item on the phone, a merged count's list, a drag scrubbing to the scheduler's own words, Play ×30; a change re-running the simulator and naming what moved, the old ×10 chip demos with the values in force, every new row's demo; the three scenes in Ask Azu's console. `MUTATE=1 node tests/test9i.js` makes the Mix table and Talkativeness ignored (always First visit's mix, always Balanced): the checks named [mix] and [talk] turn red.
  - *Guidance, round 6* (the same modules, `guide/focus`'s shared cut-out, `guide/dock`'s `dockThread.html`): the look and the full item form. `tests/test9ja.js` and `tests/test9jb.js` (the item form's buttons; split on 2026-11-09) check the scheduler's new rules alone on `gdPick` (the visitor's Tips: All / Essentials only / Off; Only if: the audience, a returning visit, has / has not used, the latest second; When on an event; After a dismissal; Must be on screen No); Says (variants in turn, never the same twice in a row; an audience's line; the returning visitor's; `{point}` `{year}`); each Focus (None, Glint, Ring, Spotlight at the item's Dim, the marker and its name in the cut-out) and the Spotlight offered only for Essentials and walkthrough steps (disabled with its reason, a Ring when not allowed, said in the read-back); the hop (a 0.45 s arc) and Walks to it (their end beside the marker, 0 px over it or its words); the ghost finger (a tap, 0 px over words, the marker or the bubble; Off; a drag across a Before / After; a swipe on a strip); each Button chip's action by a real click (Show me, Start the tour, Open the card, Ask about the view; 44 px); Reduce Motion's still versions; Tips stay until tapped (30 s later still there, its 44 px close button); the screen-reader stand-in (announced, never spoken); the dock's Tips (✦ › Ask, real clicks); Settings › The guide › Tips and Show the tips again with its Undo toast (no popup); every button, select and number of the item form pressed once by a real click, and its own buttons (a variant, an audience's line, a returning line, a chip's ×, ▸); each named warning made to appear, then fixed (kind Off, Only if contradicting itself, Done when already true, a line too long in Portuguese, a line said twice, no words), with its badge; each template; Duplicate, Delete and Undo; the drag reorder (a real drag) changing the tie-break and an In order group's sequence; every new row's demo; the four scenes in Ask Azu's console. `MUTATE=1 node tests/test9j.js` offers the Spotlight on any item and makes Delete's Undo do nothing: the checks named [spot] and [undo] turn red.
  - *Guidance, round 7* (the same modules, `mascot/azu2d`'s Moments and jokes, `world/camera-ask`'s hunt, `world/guide-button`'s band, Specific Marker › Guide script › Jokes): Fun. `tests/test9k.js` checks the Moments through the scheduler (between steps only, Lively only, never on a Solemn point, once a card, 5 min apart; the old timers gone: no Moment without the scheduler); a joke after pokes (real clicks on Azu), its variants rotating (the least seen); a point's joke when it settles, none for a point without one; the treasure hunt invitation (Family only, after a card, once a visit; its chip starts the real hunt; done when found); each seasonal window with Today is, and the 1st of November never a joke; the Solemn gate for every Fun item with Why not now?'s words; Calm and Lively only; the Fun lane and the sixth visitor's timeline (Instant, in the page); Guide › Mascot's link rows; the ✦ fix (0 px over words on the card past the hero); every new row's demo; the five scenes in Ask Azu's console. `MUTATE=1 node tests/test9k.js` removes the Fun kind's Solemn gate: the checks named [solemn] turn red. `test8k` follows the Moments and the joke into the guidance (its timers became items).
  - *Guidance, round 8* (the same modules, `audiences/audiences`'s Where to ask and `audience.asked`): the first pages (the Page form), Start the app, Who's exploring's Where to ask. `tests/test9l.js` checks the scheduler's gate and the pages' data alone (the order's ends, a first run's pages for each Where to ask and the camera's permission, the Opening message's short line, the run under 30 s); the run in order by real taps (Start the app on the preview bar, Hello, the language chip switching the app's language, Let's go, Who's exploring with Family setting the Family preset, the camera and the phone's request, the wall with the short line, then Tap a marker as round 1; the scheduler quiet meanwhile); each Skip and skipping all; Show me how it works (How to use the card's bubble says the visit's first words); the camera allowed, refused (never the wall, 30 s later), Try again, Open Settings; 44 px, 4.5:1 and 0 px over the words, light and dark; Where to ask's three values by real clicks (the first card asking or not); a later launch (no pages, Welcome back) and Remember across visits Off (the pages again); Reduce Motion and Mascot Off; Preview & Test's Start the app and the visitor's line; + Add guidance's Page template and the drag within the rules (real drags); every new row's demo; the four scenes in Ask Azu's console. `MUTATE=1 node tests/test9l.js` lets a refused camera go on to the wall: the checks named [camera] turn red.
  - *Guidance, round 9* (the same modules, `world/guide-button.js`'s Things to try, `agent/agent.js` and `agent/tools.js`'s show_how, `guide/guide-scenes.js`'s menu scene): the visitor's pull. `tests/test9m.js` checks Things to try's rows from the App items that teach (one a feature, in the Editor's order, In Things to try Yes where an item Teaches), the count and the ticks from the same memory as Done when or the feature used ([Done when]), the "how do I…" match in English and Portuguese (a history question matches none); the ✦ menu's Things to try by real clicks on the card and on the wall (44 px), Hidden (Settings › The guide's link still opens it); the sheet (the count, the rows and their words, a panel; Show me and ✕ 44 px, 4.5:1, nothing over its words, light, dark, Portuguese; the ticks following the memory; the all-done line, the rows staying ticked, the menu still opening it); each Show me from the sheet (Before / After by a real click: the card at it, a drag; strips: a swipe; the Navigation Bar and the Locate Button: a tap; Open a card from the card: the wall's marker; Ask about the view and the ✦ menu: the ✦; Pause the guide: a tour at Mini and ⏸), the scheduler quiet while it plays, no memory written, 0 px over the wall's words; Tips Off and Essentials only ([tips off]: no pushed tip, Things to try still there, its Show me playing); In Things to try per item (the Editor's button) and one row a feature; Ask Azu's "how do I…" offline (typed in the dock by real keys: the feature's words, a Show me card 44 px that plays, the item seen and not done; Portuguese; a history question with no card; on the wall); show_how in the page with no network (its schema, result, the card, the item seen, a wrong feature refused; a test double as the live agent calling it; policy Off: no tool, no rule); Settings › The guide's link; Fewer like this's toast line on an App group and not on a Content group, the rest as before; the visitor's line and Overall's row; every new row's demo (and by real clicks); the words and their owners; the four scenes in Ask Azu's console. `MUTATE=1 node tests/test9m.js` makes Tips Off hide Things to try too: the checks named [tips off] turn red.
  - *The guide panel's Looks, guidance round 11* (`guide/dock`, `guide/controls`, `guide/voice`, `guide/tour`'s music, `world/guide-button`,
    `mascot/azu2d`'s prop, `guidance/scheduler`'s Fun hold, `editor/demo.css`'s ▸): `tests/test9o.js` checks each Look (Paper glass, Ink
    glass, Player sheet, Teleprompter) at Mini and Open, light and dark (its parts, inside the phone, 0 px over the words told and over
    its own words, 44 px, 4.5:1); the speed chip's seven steps by real taps (the voice's rate follows); Previous / Next part, Pause /
    Play, Stop and its Undo; a real drag on the progress bar (the knob and the time follow the finger; the stop and the sentence on
    release); the bars moving while the voice speaks and still when paused; the teleprompter's lines moving up, Aa, Read all, Words Off;
    a typed question (real keys), a suggestion, the keyboard; the ✦ growing into the panel and back by real taps (hidden while it shows),
    Reduce Motion's crossfade; Fun held during a tour (pure and on the page) and a Moment's prop beside Azu, 0 px over any words, waiting
    with no room; the music (the volume and the ducking from the gain, the manifest's tracks looping, the visitor's choice, the credit for
    a credited track only, Music credits); the green ▸ (22 px, 3:1) and every new row's demo by a real click on it. `MUTATE=1 node
    tests/test9o.js` lets Fun through during a tour: the checks named [fun hold] turn red. The tracks: `../music/tracks.js` (the
    Architect's approved ones, with their licences; a credited one carries its credit line).
  - *Fun › On the tour, guidance round 12* (`guidance/catalog`, `guidance/scheduler`, `guidance/service`, `guidance/present`, `guidance/editor`,
    `guidance/scenes`, `mascot/azu2d`'s props, `guide/dock`'s star, `world/guide-button`'s Your N questions): `tests/test9p.js` checks the tour's
    plan alone on `gdTourPlan` (deterministic; its rhythm and caps by length; never two seams in a row nor in the first 20 s; pairs whole; its
    words) and its gates (Solemn, Calm, a screen reader, Games on the tour, two Not nows, never over a sentence, Fun's hold never holding a
    beat); the quiz's question picking (steps heard, one a third), the speech match, the guess's bands, the prop by date; on the page the
    promise and its star in every Look at Mini and Open, Deal / waiting / No quiz by real taps, the quiz (a right and a wrong answer by tap, one
    by the recogniser's text, Not sure, Show me, the score lines, the best kept, Skip, too few saved), a skipped step dropping its beat, the
    breather (the glass beside Azu 0 px over words, 8 s Go on, A question first and Back to our story), Guess the year (a real drag, Tell me,
    the two pins), the two-Not-now rule, Settings' Games on the tour, the Editor's rows and the plan line, every new row's demo (a real click on
    a sample), Draft with AI with `sampleFn` stubbed and Reviewed, and the eight scenes. `MUTATE=1 node tests/test9p.js` lets the plan ignore
    the tour's cap and rhythm: the checks named [cap] turn red.
  - *Fun › On the tour's second half, guidance round 13* (the same modules, `mascot/azu2d`'s `azu.tap` / `azu.tickled`, `world/wall`'s drag): `tests/test9q.js` checks the plan alone on `gdTourPlan` (the card's own quiz step skipped while the quiz is promised and kept when it is not, the pair as one beat, Don't say it! In a step with its two seams and nothing inside them, Find it on the wall at the seam after its step and on the wall only; the gates: a screen reader, Calm, Expert, a word said once, not reviewed, no word, a span not in the tour, a language without its word), the word suggestions with their counts and spans in EN and PT and the rules they bend, the slips' matcher, the Editor's warnings; on the page 13.0 (the step skipped, the promise's chips up while step 1 is told at Mini, Compact and Open, 0 px over its words, No quiz bringing the step back), Don't say it! by real clicks on Azu (the announce, the word card and its chips, a false alarm with no poke, the window from the word time, a catch and its pip, a second tap ignored, Previous never counting twice, a tap after the window's end, the result and the slip played back, the result lines, the quiz's callback), Pause freezing a window, Heard it! at Compact and with Mascot Off, the screen reader, a language change, Portuguese with its own word; Find it on the wall (a real drag to the point and the 1.2 s settle, the hint's direction, I give up at 20 s with Azu at the edge, the wall lost, the wall only); every new row's demo by a real click on a sample, the Guide script's word with its suggestions and Reviewed, the words and their owners. `tests/test9qs.js` (split off to keep each file under 300 s) presses the thirteen scenes in Ask Azu's console (the voice at 1.5 once each tour's plan is made). `MUTATE=1 node tests/test9q.js` makes the tap window ignore its end: the checks named [window] turn red.
  - *The Guide tab's organization, guidance round 14* (`editor/guide-tab`, `editor/card-tab`'s `edCardDoc("guide")`, `editor/texts`, `guidance/editor`,
    `mascot/azu2d`, `agent/agent`, `card/hints`): `tests/test9r.js` checks the six sections in order with their names (1 · Azu, 2 · The tour,
    3 · Games on the tour, 4 · Azu speaks up, 5 · Ask Azu, 6 · Preview & Test: last since round 14b; the Cookie inside the breather, `gd:var:water`); each moved foldout under its new parent, by id; no foldout without a row
    or a word to edit, no section with a single row, NO DEMO for none; every row key and foldout id of round 13's tab still there
    (`tests/guide_rows_r13.json`, taken from master's build before the change), but the ones the developer took out (Water break after; the old
    Mascot section gt2 and Tour controls s7tour, taken apart); the levels (1 the sections, 2 their foldouts, an item's groups never by level); an
    item opened by a real click shows its group headers only, each with its changed count (Essential clicked: 1; Reset: 0; the default
    clicked: 0); Says as one list (a real PT edit is Languages & Terms' word and its Go to opens the item's Says; + Add a variant; Remove and its
    Undo); a point's own field's link (a real click) opening Specific Marker › Guide script at the word and at a step's quiz question; an old
    open state (gd:fun > gd:g:ontour; gt2) opening the new place; the tab's names at its top; a foldout added later landing at the end of 2 · The
    tour with a console warning; no guide foldout in Detail Card; the Guide tab's draw time, faster than the whole Detail Card document
    round 13 built for every draw, measured in the same page (round 13's own draw on master's build is in `guide_rows_r13.json`, printed beside). `MUTATE=1 node tests/test9r.js` leaves Ask about the view out of its new place: the checks
    named [moved] turn red.
  - *The design pass of the AI guide, guidance round 15* (no feature, no behaviour: the look of every guide surface measured; the fixes in
    `guide/dock.css`, `guide/person.js` and `guide/dock.js`, `agent/agent.css`, `agent/scenes.css`, `guidance/present.css`, `audiences/audiences.css`):
    `tests/g15lib.js` holds the rules as arithmetic, measured on what the app really drew (installed in the page as `G15`): every guide
    target's hit area 44 × 44 (probed with `elementFromPoint` 20 px each side of its centre and its box at least 43.5 px, so a hit area
    larger than its drawing counts and a layer over it does not; a target wholly under another layer is listed, not judged); every guide
    text 4.5:1 against the fills really behind it (a gradient: each stop; a word painted by its background, the read-along: its stops; a
    glass panel with no opaque fill: over the card's paper, white and black); 0 px between each bubble, beat card, strip, Azu, the ✦ row,
    the panel (not at Full: it is the whole text) or a focus ring's stroke and the words being told (the told block's word boxes in view,
    the captions' current line; a word the panel already hides is not judged); inside the phone at 390 × 844 and 390 × 712 (its edges, the
    notch's 47 px, never partly under the panel); one focal point (a bubble, Azu's bubble, the panel's bubble, a beat card, the ✦ menu, a
    first page or Things to try: at most one). `tests/test9s.js` opens each surface by the app's own functions or real taps (the ✦ and its
    menu on the card and the wall, Azu beside a marker and on the card, the panel's four Looks at Mini, Compact and Open, the focus on a
    block; light for every Look, dark for Ink glass, 712 for Ink and Paper glass) and Reduce Motion; `tests/test9sc.js` the focus on a
    spot in a picture, the guidance's Glint, Ring and Spotlight on a marker, the bubbles (Tip, Invitation, Remark, Moment, Mascot Off's plain bubble) and the
    first pages with Welcome back; `tests/test9sd.js` Ask Azu (typed, the talk button's states, an answer with its link cards and trace,
    Show me) (one table in test9s.js, split to keep each file under 300 s); `tests/test9sb.js` every beat card played by the tour itself (the
    promise and its strip, the quiz and its verdict, Guess the year's slider and pins, Don't say it!'s announce, word card and result,
    Find it on the wall's seek ring, hint and Azu at the edge, the breather, the cookie) in Ink and Paper glass at Mini and Compact. Each
    check is named `[rule] <nn> <surface>` so a red one says which. `CAP=<dir>` saves every state (`<nn>_<surface>_<look>_<height>_<theme>.png`;
    with CAP every Look and theme). `MUTATE=1 node tests/test9s.js` makes the panel's question chips 36 px tall and draws Azu 170 px to the left over the
    told words, `MUTATE=1 node tests/test9sb.js` makes a beat card's chips 36 px: the checks named [44 px] (and [0 px]) turn red.
  - *The developer's answers on the design pass, guidance round 15b* (`world/guide-button`, `guidance/present`, `core/words`' `nameIn`,
    `guidance/catalog`, `world/camera-ask`, `guide/person`, `guide/dock`'s `azuPlace`): `tests/test9se.js` checks Mascot Off (no Azu picture
    beside the ✦ on the card past the hero and on the wall, light and dark, Guide button = Azu only showing the ✦; the plain bubble of a tip
    and a remark signed by a small ✦: present, inside the bubble, 0 px over its words, not a tap target, aria-hidden, and round 15's rules
    on it; Guide button = ✦ only with the mascot on signed too; Azu beside the ✦: unchanged); a point's name mid-sentence with a lower-case article (EN and PT, a slot mid-sentence, after a colon, at
    the start kept, a name with no article; on the page a remark about the Torreão on the wall with the point's label still "The Torreão";
    Azu's introduction, What am I looking at? and the hunt); Azu 0 px over the words after a quick step jump (`stopSpeech();G.i=j;runStep()`),
    6 repeats at g1, g5, g7, light and dark, once Azu stands still and 1.5 s later (`G15.azuInk`: the card's words under Azu's whole box).
    `MUTATE=1 node tests/test9se.js` puts Azu back beside the ✦ and unsigns the bubble, writes every name as written, and never places Azu
    again after its step's start: the checks named [mascot off], [article] and [azu] turn red. `CAP=<dir>` saves the changes at 390 px. Since guidance round 16 its [azu] check is `tests/test9sf.js` (test9se ran 324-340 s; each
    file now stays under 300 s), with the same check and mutation; since round 17b (370 s) `tests/test9sf.js` runs the repeats 1-3 and
    `tests/test9sg.js` the repeats 4-6 (`REP0=3`).
  - *The automatic tour, guidance round 16* (`guide/autotour.js`, `core/content.js`'s `KWORDS` and `POINTS`, `card/blocks.js`'s `blockSays`,
    `guide/tour.js`'s `tourMinutes` and the Guide script's Does / Keep / Order, `world/camera-ask.js`'s points with a card, `card/pages.js`'s
    quiz tap): `tests/test9t.js` switches the Ribeira card to From the card by the Guide tab's own row and checks its tour in EN and PT
    against the test's own arithmetic ([teaser] step 0 at the hero, two sentences at most, the name and the minutes; [blocks] every block with
    something to tell in the card's order; [skip] Audio Guide and Practical Info none; [cap]; [twice]; [links]; [minutes] 170 words a
    minute and 0.7 s a line, on the hero's button too; [card words] the page's own atCheck); [follows] the Gallery's 🗑 and + Timeline in
    Specific Marker: the tour follows; [play] the tour through the hero's Guide button and the panel's Next, every step's words in the
    captions and the focus on its block, the flip card, the before and afters and the quiz answered by real taps and drags, the games on
    the tour planned on its steps; [mine] Make it mine (a real click), an edit typed in a step's box kept through Regenerate of another, the
    changed-blocks notice and Add them in the card's order, Dismiss, and Ribeira asking first; [ai] the model stubbed (never the live one):
    nothing asked until the box is ticked, the stored lines with their template lines, a new number kept as the template, Use the template
    line, no model said in the row; [point] the Torreão's pin opening its card, A tour of {point} with its minutes and its tap starting
    that tour, the city on the hill still "has no tour". `MUTATE=1 node tests/test9t.js` drops the teaser, leaves the Gallery out and says
    a link twice in a row: the checks named [teaser], [blocks] and [links] turn red.
  - *The developer's answers on the automatic tour, guidance round 16b* (`guide/autotour.js`'s fact keys and links, `guide/tour.js`'s
    `tourLength` and the teaser's pictures, `guidance/catalog.js`'s short line, `core/words.js`'s `nameJoin`, `guide/person.js`'s hero placing,
    `card/hero.js`'s crop): `tests/test9t.js` also checks [facts] no fact twice in the four points' tours, EN and PT, by the test's own fact keys
    (numbers, dates, decades, numbers in words, proper names; a reference year; a bare year repeating a date; the point's name), the page's
    atCheck agreeing and the named cases (Ribeira's story on its link alone, the Torreão's facts and timeline, the Tagus's River); [links]
    every link from its block's own words or the neutral one, at most 6 words, none of round 16's generic links, never twice in a row, and the
    Editor's step list showing each link with its source; [short] under 2 minutes "a short tour" in the teaser, the hero's button, the
    invitation and the Editor, from 2 up the minutes; [hero words] the hero's eyebrow, title and line clear of the panel, the bubble and Azu
    at the teaser at Mini (`G15.hero`, `tests/g15lib.js`), the four tours and the written g0, the Start button hidden while the tour plays;
    [crop] each new point's hero a crop of the wall around its point (the wall's file, its size and offset by the test's own arithmetic, at
    Full and Half, three different, kept during the teaser); [follows] now adds a Timeline (its dates all told by the story: no step) and a
    Dialogue (its step with the neutral link). `MUTATE=1` also turns the fact rule off, puts "Look at this." back as the neutral link, takes the short-tour words away, leaves
    the hero at the top with its Start button and draws no crop: the checks named [facts], [links], [short], [hero words] and [crop] turn red.
  - *The developer's answers on the guide's words, guidance round 16c* (`guide/autotour.js`'s intro sets and skipped blocks, `core/words.js`'s
    `at_in_*` words, `guide/tour.js`'s Guide block, `world/guide-button.js`'s ✦ menu, the PT names in `card/hero.js`, `core/content.js`,
    `world/markers.js`, `world/camera-ask.js`, `agent/tools.js`'s start_tour): `tests/test9t.js` also checks [intro] every step's intro line
    from its kind's set (the words the Editor edits), at its step's start, never twice in a tour, the point's name in at most two (every line
    made a {point} line: the third says "this place" / "este lugar"), under 20 words, a leading line going on into the block's own words with
    its content kept, none of round 16b's links, no line repeated across the four tours, EN and PT, and the Editor's step list showing each
    line and its set; [nothing new] Ribeira's Quick Facts with no step, shown skipped with why; [article] "O Terreiro do Paço", "O Paço da
    Ribeira" (the hero, the marker, the teaser) and the real invitation "Uma visita curta ao Terreiro do Paço?"; [length] the ✦ menu's Start
    the tour and the Guide block's line saying the same length as the hero's button and the invitation; [edit] a line typed in its EN box
    said by the next tour, + Add a line, Remove, Back to the defaults and the row's ▸ by real clicks. `MUTATE=1` also says an intro twice,
    puts a lone Quick Facts step back with no row saying why, takes the article away, writes the Guide block's 3 min and reads the lines
    from their defaults: the checks named [intro], [nothing new], [article], [length] and [edit] turn red too.
  - *The developer's answers on round 16c and the plan's two features, guidance round 17* (Part A: `core/words.js`'s "The Ribeira Palace" and the
    Guide block's title, `guide/autotour.js`'s teaser openings and one fact; Part B: `guide/saved-place.js`, `guide/tell-more.js`, the deep parts in
    `guide/autotour.js`, the ways in of `card/hero.js`, `world/guide-button.js` and `guidance/`): `tests/test9t.js` also checks [name] "The Ribeira
    Palace" as written on the hero, the marker, the wall's point and the teaser's start, "the Ribeira Palace" mid-sentence (the story's intro line, the
    ✦ menu's and the pill's Ask about, Ask Azu's context to the model, stubbed); [one fact] the One fact set and the fact as a sentence ("Keep this one
    in mind: it was lost on 1 November 1755.", PT "…: foi perdido a 1 de novembro de 1755."), two facts keeping the facts set; [teaser] the four tours
    opening differently, EN and PT, each a line of the set, the length still said; [block] the Guide block's title naming the point on every card
    (its base turns the saved place Off: its offer would change the hero's Start). `tests/test9u.js` and `tests/test9ub.js` (split to keep each file under 300 s: [expiry], [changed], [rows], [blocked]) check Part B through the app's own controls:
    [deep] each step's deep part, the words its rule left out, only the card's words, none where there is nothing deeper; [facts] no fact twice with
    every deep part heard (the test's own fact keys, the four points, EN and PT) and a card made for the deep pass; [minutes] the short version's
    length, both in the Editor's list; [more] the chip only where there is more (44 px, inside the phone, above the panel), Next as Skip ahead, a
    real tap: More coming, the deep part after the line, then on; [pace] three Tell me more in a row: deep first; three Skip ahead: lighter; Start
    deep for: Everyone; [edit] a written step's Tell me more typed in Specific Marker said; [ask] "tell me more" typed (real keys) and spoken (the
    recogniser stand-in), the model stubbed and never asked for it; [offer] a tour stopped at step 5, the page reloaded (a place is offered on the next visit only): the hero, the ✦ menu and A
    tour of {point}? offering "Continue the tour (step 5 of 7)" and "Start again", PT too; [continue] each way in: "Where we were: …" (the step
    before's words, no new fact), then that step; [again]; [finished]; [expiry] by Preview & Test › Today is (7, 30, 1 days, Never); [changed] the
    step gone (the Gallery's 🗑) and another source (the Ribeira's Written), the notice once; [rows] Remember Off, Tell me more Off, each row's ▸;
    [blocked] storage blocked: no error, nothing kept for the next visit, the Editor saying so; within a visit nothing is offered (round 11's Resume carries on). `MUTATE=1 node tests/test9t.js` also writes the name
    without its article, says one fact with no line of its own, gives the teaser one opening and the Guide block its old title: [name], [one fact],
    [teaser], [block] turn red; `MUTATE=1 node tests/test9u.js` takes the recap away (Continue from the start), never forgets a place, never goes
    deep and turns the fact rule off: [continue], [pace] and [facts] turn red, and in test9ub [expiry].
  - *The developer's answers on round 17, guidance round 17b* (`guide/saved-place.js`'s one offer, `guide/controls.js`'s toast, `world/guide-button.js`'s
    menu, `guide/autotour.js`'s gallery deep part, `card/blocks.js`'s `kindWords`): `tests/test9u.js` [offer] and [continue] check the one way back into
    a stopped tour: a Stop within the visit (the panel's Stop, real taps): the Stop toast, the ✦ menu, the hero and A tour of {point}? all offer
    "Continue the tour (step N of M)" and "Start again", and Continue (a real tap in each) goes back to the step and the sentence with no recap; on a
    later visit (a reload) the same words, and Continue says "Where we were: …" first; Start again from the toast clears both places.
    `tests/test9t.js` [gallery more] checks a gallery's deep part: its lead from its set (Azu's intro lines › A gallery's Tell me more, EN and PT), the
    pictures' own captions said one after another without their medium (no "a painting of", no list's colon), only the card's words and the lead, under
    its cap, no fact twice; [block words] (the crash the core test found: a Dialogue's page drew its kind with no block) every block kind's
    preview and full page drawn with words of its own that lack a field fall back field by field to the kind's (a Dialogue with no lines says the
    kind's lines), no error; `MUTATE=1` puts the list back ("In its pictures: a painting of …") and the words object by object: [gallery more] and
    [block words] turn red.
  - *Every Editor row's demo, guidance round 10* (`editor/demo`, the rows' modules: `editor/card-tab`, `card/controls`, `card/blocks`,
    `card/pages`, `card/states`, `card/hints`, `editor/texts`, `editor/components`, `editor/marker-tab`, `mascot/azu2d`, `guidance/editor`):
    `tests/test9n.js` checks that the self-test's NO DEMO check walks every tab and finds no row without a demo but its named exemptions
    (each with its reason); a row's demo taken away in the page (Card Controls, a block's option, Guide › Preview & Test, a marker's
    block) or a row added with none (Detail Card, Specific Marker) is reported NO DEMO with its tab; a sample of each kind of row, two or
    more a section, by real clicks (a change and a press on the label ▸): the phone in that row's view and state, the part ringed (a
    type row, a colour row and its Open Row in view, a motion played once, a block's option with the block in view, a page option, a
    section, Card Controls, the loader after its wait, the Guide's simulator rows, a marker's block, Solemn point, Page of); the demo
    starting within one frame of the change and ringing 60 ms later; Show each row's demo = Off. `MUTATE=1 node tests/test9n.js` drops
    the Specific Marker tab from the NO DEMO check: the checks named [every tab] turn red.
- **The live voice by hand:** with the keys on this computer, open the page from its file (not `#probe`), set Ask Azu >
  Voice of the answers, and ask; it spends a few hundred characters an answer.
- **Keys:** the local Lab's live agent and live voice read `keys.local.js`, which `python keys_local.py` writes from the workspace's
  `.secrets/keys.env` (git ignores both); `python keys_local.py --check` asks each service whether its key works. The headless
  checks (`#probe`) never load it; `#probe-live` does. See `../../__Services_And_Keys.md`.
- **The audit** that shaped the card's merged options: `AUDIT_DECISIONS.md` and `audit/` (one report per area).
- A change: make it in the feature's own place (`PROTOTYPE_STRUCTURE.md`, "Where a change goes"), rebuild, run the self-test
  and `node run_tests.js`, look at a capture, then fold the decision into `_3.3`.
