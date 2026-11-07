# Block Components Lab: sources

The Lab is one HTML page joined from its modules. Where each feature lives, the rules that keep it that way and the order of
the modules: `PROTOTYPE_STRUCTURE.md`. Below: the words, the build, the tests and the keys.

- **The code:** `src/<area>/<module>.js` with its `.css`, in the order of `build.py`'s `MODULES` and `STYLES`; the page in `src/page/`,
  the pictures in `assets/`. Each module is described in `PROTOTYPE_STRUCTURE.md`.
- **Words:** `src/core/realstrings.json` (the framework's real words, with their owner part; `python strings.py` re-imports them
  from Unity's `CardStrings.asset`); the Lab's own words are in `src/core/words.js`'s `LABSTR`.
- **Build:** `python build.py` writes `block-components-lab.html` here (not committed). Copy it to `../Block Components Lab.html` (the
  record copy) and publish it to https://claude.ai/artifact/QmhHSzTrbJAR3doq1YEoZR.
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
  - **Level 1, once the block's look is accepted (about 10 minutes):** `node run_tests.js --only=<the block's files>` (the
    files this map names for the modules it changed; `--changed` lists them, and also runs every changed test), its
    mutation (`MUTATE=1`), `tests/runtest8.ps1 -tabs <the tab it touched>`. The pure logic checks run in Node, with no
    browser, where that is simple.
  - **Level 2, every 3rd-4th round, and when a round touched shared ground** (`--changed` prints SHARED GROUND or NOT IN THE
    MAP: `core/`, `editor/shell.js`, `editor/card-tab.js`…, the build and test tools; new words only in `core/words.js`
    are covered by `-tabs words`) **before its publish:**
    `tests/runtest8.ps1` (every tab, about 3 min), then `node run_tests.js` (every file, about 30 min).
  - **Level 3, every 10-12 rounds and before a hand-off to Unity:** level 2, then every mutation the tests name, and the
    results replace `tests_baseline.json`.
  - A wider level confirms, never discovers: green on 1 before 2, on 2 before 3. A round's report names the level it closed on
    and the rounds since the last level 2 and 3.
- **The tests, by feature:**
  - *The sheet and the card's chrome* (`phone/sheet`, `phone/grabber`, `phone/redraw`, `card/chrome-pinned`, `card/locate`):
    `tests/test8f.js` checks the sheet's gestures, the pinned close and settings, the demo card's X, and that a change moves no
    frame.
  - *The demo card and the hints* (`card/demo-card`, `card/hints`): `tests/test8g.js` checks the demo's flow, each hint (its
    moment, Off, Reduce Motion, one at a time) and How to use the card.
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
- **The live voice by hand:** with the keys on this computer, open the page from its file (not `#probe`), set Ask Azu >
  Voice of the answers, and ask; it spends a few hundred characters an answer.
- **Keys:** the local Lab's live agent and live voice read `keys.local.js`, which `python keys_local.py` writes from the workspace's
  `.secrets/keys.env` (git ignores both); `python keys_local.py --check` asks each service whether its key works. The headless
  checks (`#probe`) never load it; `#probe-live` does. See `../../__Services_And_Keys.md`.
- **The audit** that shaped the card's merged options: `AUDIT_DECISIONS.md` and `audit/` (one report per area).
- A change: make it in the feature's own place (`PROTOTYPE_STRUCTURE.md`, "Where a change goes"), rebuild, run the self-test
  and `node run_tests.js`, look at a capture, then fold the decision into `_3.3`.
