# Architect hand-off -- the POI card's prototype rounds (`_3.3`)

Read this first in a new Architect chat, then `__AI_Architect.md`. It says where the work is and how a round runs.

## 0. Start here (next conversation, from 2026-11-07)

- **State:**
  - **The Lab:** `master` holds guidance rounds 1-14, verified on 2026-11-06/11, published at
    https://claude.ai/artifact/Gpjf7yvoYHjGebmmNhuw2o (round 14b) with the 186 voice files, the music and the capabilities (section 1). The
    developer changes account often (four times on 2026-11-08/09); the old links stay in their accounts (RGofgb… at round 13, 19Uiat… at round 12, RKn3Rr… at round 11, J9J4h… at round 9, 4LpLd4… at round 8, Jq6X2K… at round 7, Jrejv7… at
    round 6, QmhHSz… at round 5, KDs2… at v29). A new account cannot update an old link: publish a new one (section 1, Publishing). Its
    code is in modules by feature
    (`lab_src/src/<area>/<module>.js`), with no wrappers: `lab_src/PROTOTYPE_STRUCTURE.md` says where each feature lives and
    where a change goes. The build only joins files (`build.py`) and refuses wrappers, re-assignments of functions, second
    declarations, defaults set twice and version-named modules (`check_structure.js`, `structure_baseline.json` at 0).
  - **Proof of the Lab:** the self-test, one big tab per walk (`tests/runtest8.ps1 -tabs card`, `-tabs guide`, `-tabs
    marker,global`, `-tabs words`, each its own call), gives `ok=1199 behaviour=2046 bad=0` in about 5 min (level 3 after
    round 10, 2026-11-09); it also fails any Editor row without its demo, in every tab. `node run_tests.js` runs every test
    file one at a time (37 files, 1056 checks, about 72 min; `tests_baseline.json`, rewritten by that level 3). test9d was
    split into test9da / test9db (round 10) and test9j into test9ja / test9jb (level 3: it reached its 600 s kill on a busy
    machine, on master too). The slowest files now: test9e 494 s, test9da 322 s, test9db 344-502 s. Never walk
    `guide,words` together in the self-test (65 min). test8q's mouth and test9b's rest were timing races (set and read in two calls; read with no wait):
    fixed on 2026-11-09 (one call; an `until` of 1.5 s), 3 of 3 green each.
  - **Level 3 after round 10 (2026-11-09)** found no break from round 10: its reds were timing races and one file too long
    for a busy machine (the developer's Chrome at 160 % CPU and 1.8 GB of memory free). Fixed: test9i's Talkativeness demo
    waits up to 20 s for its chip (8 s), test9da waits for Azu and the scroll to settle (10 s) and for the dock before its
    drag, test9j split; test9db and test9k were green alone. Every mutation (12 files: test9c, 9da, 9e, 9f, 9g, 9h, 9i, 9ja, 9k, 9l, 9m, 9n) turns its named checks red (test9l's after a rerun: its first start did not load in 90 s).
  - **Level 2 caught real breaks after rounds 6, 7 and 8** (round 6: a reused class, the dock's Tips box shifting the
    answers' traces, a tip rule catching cues; round 7: the card's ✦ and Azu drawn during a tour; round 8: checks of
    round 6's templates and defaults). Two rules came of it: shared code changed means level 2 that round; and every round
    touches `src/guidance/`, so a round's level 1 runs test9e up to its own file, not only the new one.
  - **Test levels** (decided 2026-11-06; cut twice since: the developer refused 1h30 of tests a round, then a 30-min level 1
    on 2026-11-07). The rules are in `lab_src/README.md` "Levels". Every brief repeats them.
    - **While building:** only the build and captures, iterated until the app and the Editor look right
      (`skills/Feature_Prototyping.md` step 3.3).
    - **Level 1, once a round (about 25 min for guidance rounds):** the round's new test file, the file of what it replaced,
      the guidance files test9e up to the round's own, the files of any shared module it touched, its mutation, and the
      self-test of its tab (one big tab per walk). No `--changed` run: it pulled 8-10 files.
    - **Level 2, every 3rd-4th round (about 35 min):** every tab and every file. It runs sooner when a round changed existing
      shared code.
    - **Level 3, every 10-12 rounds and before a Unity hand-off:** level 2, every mutation and a new baseline.
    - Count the rounds since the last level 2 and 3 in every answer. After guidance round 14 that is 0 and 4: level 2 ran
      after rounds 11, 12, 13 and 14 (round 14, 2026-11-11: the self-test's four walks bad=0, card 710/248, guide 31/1364,
      marker+global 23/43, words 479/542; 42 files, 1267 checks, 104 min on a machine at 66-68 % CPU: 9 files red or
      crashed at the start; all green when run again alone at 15-20 % CPU except test8o, whose check still wanted the
      Guide panel before Captions: it now checks round 14's order); level 3 last ran on 2026-11-09.
  - **Where the tests' time goes (measured 2026-11-07):**
    - Starting Edge and loading the 13 MB page costs only about 8 s a file (Edge 0.3-0.8 s, the page 7 s), so one shared Edge
      would save almost nothing.
    - The time is the tests' own work: test9d presses 72 rows' demos (114 s) and clicks about 25 values (68 s); scenes run in
      real time.
    - Done:
      - Run all scenes has a Stop; test8u runs the console's 8 and stops (258 s to 47 s).
      - test9f waits for each chip instead of fixed 4-9 s.
      - test9d no longer presses 4 · Guidance's rows (test9e presses every one).
      - Caps that end as soon as the thing happens were widened, so a busy machine does not fail them.
    - **A busy machine fails timing checks:** VS Code once held the CPU at 60-99 % and every file ran 30-50 % slower with false
      reds. Check the load first (`(Get-CimInstance Win32_Processor).LoadPercentage`); a window reload fixed it.
    - Still open: pure logic in Node (the scheduler reads the page's catalog, so not simple yet).
  - **Decided and built on 2026-11-05/06:**
    - The voice: one order for every line (`sayOrder`, `src/guide/voice.js`): the recorded line, else the live voice (when its
      key is on this computer), else the device's; "Speaking now" in the Editor says which and why. Best available prefers a
      published voice set.
    - The guide's controls: the ✦ guide button with its menu, Azu beside it (a tap plays; once a visit it points at the ✦),
      Pause and Stop at Mini, Compact (the dock hidden, the ✦ ring), Stop with an Undo toast, Previous replays the sentence.
    - The mascot: Mascot source = 3D model, Model = The explorer by default; "Azu, simple" removed; where Azu is not drawn live
      (beside the ✦, the guide block) a 3D source shows the model's picture.
    - Every Editor row of the Guide tab and of Detail Card > 6 · Audiences has its demo (`rowDemos`, `src/editor/demo.js`).
  - **The guidance system:** `_3.8_Guidance_Study.md` (decided 2026-11-06; the two independent plans with the research and
    sources in `_3.8_guidance_plans/`). Decisions: a top-level **Guide** tab (today's 6 · Guide + Guidance + the first pages);
    App and Fun tips as a bubble only, Content spoken once the guide has been heard; a deterministic mix; the card Hints edited
    in Guidance with a view in Card Controls.
  - **Guidance rounds 1-10 are done** (merged into `master`; the briefs `_3.8_guidance_plans/brief_round1.md` to
    `brief_round10.md`).
    - Round 2: the memory and the visit, every gate and limit, the stage and the score, the learning, Ask about the view
      and A tour of {point}, Why not now? / The visitor / Log, five console scenes; `tests/test9f.js`.
    - Round 3: Azu's wall remarks became Content › What's in view (one item a point) and the ✦ hint App › The wall ›
      The ✦; their old timers removed; two scenes; `tests/test9g.js`.
    - Round 4: the card Hints became App › The card (cues, a Cue room of 5 s) and How to use the card a pulled
      walkthrough, also from the ✦ menu's How does this work?; Card Controls > Hints a view; `tests/test9h.js`.
    - Round 5: the editable Mix table, Talkativeness, the presets (and the audiences' Guidance preset), Simulate a visit
      with five scripted visitors, the ×10 demos of the Overall and kind rows; `tests/test9i.js`.
    - Round 6: the look (Focus None / Glint / Ring / Spotlight, the 0.45 s hop and Walks to it, the ghost finger, the Button
      chip, Reduce Motion), accessibility (Tips, Tips stay until I tap them, Show the tips again, the screen-reader stand-in,
      the dock's Tips), the full item form (six groups, variants, Only if chips, named warnings, + Add guidance with
      templates, Duplicate, Delete with Undo, drag), four scenes; `tests/test9j.js`. Shared code changed: `editor/shell.js`
      (a foldout opened by a draw is quiet), `guide/focus.js` (one shared `spotPaint`), `guide/dock.js` (`dockThread.html`).
      A Guide tab draw takes about 1.1 s, mostly the Detail Card document: the main cost of test9j's clicks.
    - Round 7: Fun (a bubble only; 4 a visit, 5 min apart; a fixed Solemn gate): Moments (water, cookie) moved in from
      Azu's timers, A joke, {point}'s joke (Specific Marker › Guide script › Jokes), Treasure hunt? (Family), Seasonal with
      Preview & Test › Today is; a sixth scripted visitor; the ✦ on a paper band on the card and in a tour (no words under
      it); five scenes; `tests/test9k.js`. Shared code changed: `mascot/azu2d.js`, `world/camera-ask.js`,
      `world/guide-button.js` and `.css`, `core/words.js`.
    - Round 8: Sequences › First pages (Hello with Azu forming from a tile and the language chip, Who's exploring when
      asked at the app's start, the camera with its refused path, straight to the wall), Skip, later launches' Welcome
      back; Audiences › Where to ask (`S.aud.where` replaced `S.aud.ask`); Start the app in the preview bar and Preview &
      Test; How to use the card moved to Sequences; four scenes; `tests/test9l.js`. Shared code changed:
      `audiences/audiences.js`, `core/words.js`.
    - Round 9: pull: Things to try (the ✦ menu and Settings; a sheet of 8 features from the App items that Teach, ticked
      from Done when, each with Show me), App › The guide › Pause the guide (a pulled-only Try item), the card controls'
      use events, Ask Azu's "how do I…" (offline match in EN and PT, the live tool `show_how`) with a Show me card,
      Fewer like this's toast; four scenes; `tests/test9m.js`. Shared code changed: `world/guide-button.js` (the ✦ menu has
      5 items on the card, 6 on the wall), `agent/agent.js`, `agent/tools.js` (9 tools), `guide/guide-scenes.js`,
      `core/words.js`. Its level 1 ran 13 files (every guidance file and each touched module's), so the full level 2 was
      left to round 10's level 3.
    - Round 10: every Editor row of every tab has its demo (298 were missing; `rowDemos` in each row's module, a family by
      `rowDemos(function(key){…})`, the spec's new fields `block` and `page`); the self-test's `NO DEMO` covers every tab;
      12 exemptions with reasons (`ROW_DEMO_EXEMPT`, `editor/demo.js`); one ring at a time; a change's demo starts in the
      same task; test9d split; `tests/test9n.js`. Shared code changed: `editor/demo.js`, `editor/shell.js` (`playRef` /
      `replayRef`, one play per part), `editor/texts.css`, and each tab's modules (`card/*`, `editor/*-tab.js`,
      `editor/components.js`, `mascot/azu2d.js`). The Detail Card tab's draw grew about 9 % (the ▸ on 290 more labels).
    - Their captures were judged by the Architect; the developer reviews later, step by step. The open look questions:
      round 1's PT opening line, what "Mascot Off" means, Azu's size beside the marker; rounds 2 and 3's in the closing
      answers of 2026-11-06/07 (among them: a group's own shorter gap, the remarks counting in Content's cap, The ✦ on the
      card, a ring for The ✦ with the 3D model, "The" mid-sentence in the remark lines).
    - Round 11 (2026-11-10, the developer's feedback on the guide's panel): the panel's **Look**, four built side by side
      so the developer chooses (Paper glass, **Ink glass**, Player sheet, Teleprompter), each with Mini and Open: speed
      0.5-2×, previous / next part, pause, stop, a draggable progress bar, the voice moving, the words as a teleprompter,
      Aa and Read all, asking by voice or typed with suggestions; the ✦ grows into the panel and hides while it shows; Fun
      held during a tour (the water glass showed over a caption); the Editor's ▸ a green play button; the music bed's
      volume, ducking, tracks (`music/tracks.js`: four CC0 tracks and Cabezón by Joan Benson, CC BY-SA 2.0, credited) and
      the visitor's Music choice. `tests/test9o.js`. The Architect's review fixed Ink's words running off the panel (a
      `flex:none` winning over Ink's `flex:1`) and the card's text reading through Paper's and the Teleprompter's Open (a
      .97 fill; Open is solid now), Azu no longer showing listening or thinking at Mini (its state was copied only by the
      talk button's timer, and the Mini has no talk button; test8q caught it), and two test races: test9o's music level (a fixed wait on the audio clock) and test9h's
      sheet still growing to Full (184 px of 820) when a check began (red on round 11 about one run in two, green on
      master; now 4 of 4). Level 2: the self-test's four walks bad=0, 38 files green after those fixes.
    - The developer's answers (2026-11-10): **Ink glass** is the default look (the other three stay as options); the Mini
      shows **Pause only** (Stop is in Open); the water glass and the cookie move to round 12; the "blue line" beside Azu was
      the old 2D Azu's pointing arm (`.marm`), still drawn beside the 3D model in a tour: now only the drawn tiles have it
      (`pointAt`, as the ✦ already did; test8j checks both).
    - Round 12 (2026-11-10, `_3.8_guidance_plans/brief_round12.md`): Fun › On the tour, its own rhythm (one beat per 75 s,
      at most 3 a tour, Short 2, Deep 5), the tour plan (`gdTourPlan`, pure, asked once at a tour's start; readable in the
      Editor; Why not now? answers from it), pairs, the 4 s Not now (two end the games), the breather (today's Water break
      with Go on / A question first; the Cookie its variant; props by date), the promised quiz (the star in every Look;
      per-step questions with Draft with AI and Reviewed; tap or speech), Guess the year (slider, two pins, 3 bands); the
      visitor's Settings › The guide › Games on the tour; 8 scenes `gd12_*`; `tests/test9p.js` (60 checks, mutation 5 red).
      The demo tour (1:24 of narration) has room for 2 beats, so it plays the quiz pair only. The Architect's review:
      "Close!" on every wrong quiz answer became "Not quite." (500 years is not close to 250); level 2 found three checks
      written for the old state: test9m (Settings › The guide has 7 choices with Games on the tour), test8u (57 scenes;
      Stop's count now taken as the click lands, since the Guide tab's redraw, about 1.3 s, can hold Playwright's click
      while two more scenes finish) and test8w (it set Azu's state on the element; since round 11 the dock's timer copies
      the app's own state at every height, so it now sets `G.talk` / `G.busy`).
    - The developer's answers on round 12 (2026-11-10), for round 13's first part: when the quiz is promised, **Azu's quiz
      replaces the tour's "Test yourself" step** (the block stays on the card to play freely; at the finale two quizzes
      were on screen); **the promise and its quiz count as one beat** (so a short tour gets the quiz and one more beat);
      the promise's **4 s buttons stay up while step 1 starts** (the tour no longer waits about 9 s); **the Cookie stays**
      as the breather's variant (it plays in tours without the quiz). The words ("Not quite.", "Got it: no more games
      today, just the story.", "Toque na resposta, ou diga-a", "Primeiro uma pergunta") were not changed.
    - Round 13 (2026-11-11, `_3.8_guidance_plans/brief_round13.md`): the four answers (a promised quiz takes the card's
      "Test yourself" step out of the tour, `P.skip` with `gdSkipApply` / `gdSkipUndo`, a dropped quiz puts it back; the pair
      counts as one beat, `b.one`; the promise's Deal / No quiz, thanks on a strip above the panel while step 1 starts,
      `.gdstrip`, 0 px over the words being told), then **Don't say it!** (In a step, Family and Explorer, Lively only: the
      Editor suggests words from the tour's text per language, 4+ letters said 2-4 times in a 1-2 step span, with the
      author's Reviewed tick; tap windows from the word times + 2.5 s; a catch, a false tap's "Nope!", the result with the
      missed slip played back; Pause / drag / Ask freeze the windows; Heard it! at Compact and with Mascot Off; not with a
      screen reader) and **Find it on the wall** (Between steps, the point's own wall, Calm too: the card to Mini, a seek
      ring, the 1.2 s settle, a hint at 10 s from the point's screen direction, I give up / 20 s: Azu walks to it; never
      while tracking is lost; `wall.js`: a drag that starts on the panel, a beat card or the strip no longer pans the wall).
      `tests/test9q.js` (48 checks) and `tests/test9qs.js` (its 15 scenes); 70 scenes in Ask Azu. The Architect's review:
      the captures fine (the strip sits above the caption, the word card and the result line read well, the seek ring
      centred). Level 2 found one check written for the old state: test8n's scroll ran 2400 px to the bottom of the card,
      where the only block is "Test yourself", now out of the tour, so the guide paused (the existing rule: no told block
      in view, it pauses); the check now runs with Games on the tour Off, and a new one checks that pause with the quiz on.
    - The developer's answers on round 13 (2026-11-11): Don't say it!'s span stays **fixed at 45-120 s**; the tap window
      stays **2.5 s** and **the result waits until the last window has closed** (a change for the next round: check that
      the result never comes at a seam while a window is still open); with the device voice **the missed slip is said
      again** (as built); Find it on the wall keeps **the card at Mini and an arrow at the edge** when the point is off the
      screen (as built). Open: the Portuguese words ("Não diga!", "Apanhou 1", "Apanhado!", the pastéis de nata line).
    - Round 13b (2026-11-11, the Architect): the result's wait was already built (`gdWordEnd` waits on the game's clock for
      every open window); test9q gained the check (a slip 1.5 s before the seam: no result at 0.8 s, the result after the
      window; 48 checks, 175 s; the mutation turns it red with the other [window] checks). The PT words stay.
    - **The developer's direction (2026-11-11): only the AI guide domain**, until it is finished and polished (great
      features, design and Editor organization); the rest of the app only where the guide meets it. The plan:
      `_3.8_guidance_plans/guide_polish_plan.md` (the Guide tab's inventory, 649 rows; what is wrong in its organization;
      the proposed tab; the design pass; the features; the order). Parked (not the guide): the card's seven missing phone
      effects (the developer had picked only the system events simulated in Preview & Test), Global Scene (kept as
      headings, developer), the card blocks' stand-ins (Place in AR, Knowledge Check's Appears), the Sticky Action's demo,
      whether every Detail Card change moves the phone.
    - **Round 14 (2026-11-11): the Guide tab's organization** (`_3.8_guidance_plans/brief_round14.md`, the plan's
      section 3). Six sections: `gt0` 0 · Preview & Test (The visitor's table under Details `gd:visitor`; the Voice test
      `s7vtest` moved here), `s7mascot` 1 · Azu (Model tools `gt:tools`; the Moments and Water break after link rows
      gone), `gt1` 2 · The tour (Ways in `gt:ways`: Guide button, Hero button, Guide block, Resume; Tour controls'
      words inside the Guide panel), `gd:g:ontour` 3 · Games on the tour (promoted; the quiz's promise and finale one
      item `gd:pair:quizpromise`; Cookie marked as the breather's variant), `gt4` 4 · Azu speaks up (Fun keeps Play),
      `gt3` 5 · Ask Azu (What the visitor sees `s7ask`, Knowledge `gt:know`, The engine `s7agent`). Removed ids: `gt2`,
      `s7tour` (old saved states map to the new places; a foldout with no home goes to "Not placed yet" with a console
      warning). A strip of the six names on top (`.gtnav`). Each item's groups are folded foldouts
      `gd:f:<item>:<group>` with a summary, "N changed" and Reset; Says is one EN | PT list (▸, Remove with Undo);
      a per-point field links to Specific Marker › Guide script at that field. The tab draws only the guide's part of the
      document: 1.1 s to 0.35 s. `src/editor/guide-tab.css` (new), `tests/test9r.js` (28 checks),
      `tests/guide_rows_r13.json` (round 13's 649 row keys, all still there). The Architect's review: the captures right
      (the strip, the folded groups, the merged Says, Games on the tour, Ask Azu's three parts, the link landing).
    - The developer (2026-11-11): the Detail Card blocks' design (each point's card blocks) stays valid and is the
      **next domain once the AI guide is finished**; postponed, not dropped.
    - The developer's answers on round 14 (2026-11-11): **Preview & Test moves to the end** (6 · Preview & Test: the guide's own
      parts first, the Editor's tools last); a group with nothing changed **stays quiet** (no "0 changed", as built);
      **Answers from stays in What the visitor sees** (as built); **the Cookie goes inside The breather** (as the quiz's pair).
      Ways in stays folded like Model tools and The engine (the Architect's default, said to the developer).
    - Round 14b (2026-11-11, the Architect): `GUIDE_SECS` with `gt0` last as "6"; the breather and its variant drawn as one
      foldout `gd:var:water` ("The breather · with its variant “Cookie”: never both in a tour", `gdVariantsOf`, `gdVarBaseOf`,
      `gdItemPath`); test9r, test9e, test9p, test9k follow (green: 28, 59, 60, 51; the self-test's guide walk bad=0; test9r's
      mutation red). The spec's table, PROTOTYPE_STRUCTURE and README follow.
    - **Next: round 15, the design pass** of every
      guide surface (the plan's section 4, with the guide's open look questions from rounds 1-3), then the four
      features (section 5). Prototype only (no Unity) until the developer says so.
  - **MCP:** blender-mcp at user scope (Blender 5.2.3, add-on 1.8, port 9876). The Unity MCP was not connected this stretch.
  - **Keys:** Gemini, Azure Speech and ElevenLabs answer (`python proj_guides/_3.3.1_Prototype/lab_src/keys_local.py --check`).
- **The tasks, in this order:**
  1. **Guidance rounds 1-10 are done** (`_3.8` section 8), each verified and published. Next: the developer's look review,
     step by step, then what it decides.
  2. **The developer's look at v29+:** the open questions of the guide's controls (the ✦ above the dock at Mini, the ring at
     Compact, the step's name under Resume, one invitation at a time on the wall, the explorer's size).
  3. **Lab upkeep** (`_3.3` section 17, "The Lab"): the self-test's time (about 5 min as one big tab per walk); split the large shared handlers by
     feature when each is next touched.
  4. **Mascots:** the cat waits for the developer's Meshy model; the mouse keeps its gentle wave.
  5. **Unity:** the GLBs' import (glTFast), the tile of light's Shader Graph, and later the guidance (`_3.8` Unity path), as
     Worker blocks (the Unity MCP must be connected).
  6. **Voices:** a British EN ElevenLabs voice is the developer's choice; the app's Worker streams the live voice a sentence at
     a time.
  7. **The form of address** (tu / polite / você): `_3.5` section 3.9, waiting for the developer's decision.
- **Rules learned:**
  - Keys go only in `.secrets/keys.env`, never into a script.
  - MCP servers are added with `--scope user`.
  - Every test runs in the prototype, through controls the developer can press (scenes in Ask Azu's console).
  - Every feature has its Editor row, and every row its demo, checked by the tests (`skills/Feature_Prototyping.md` step 5).
  - Large Lab rounds go to an Opus subagent with a written brief (the files to read, the proofs, the report); the Architect
    runs the proofs again itself before a commit, and looks at the captures.
  - A publish of the Lab to its link first needs the live version read (the Artifact tool's read or a refused publish); the
    voice files stay published beside the page.
  - Script edits with apostrophes or backslashes: a small Python patch written with the Write tool.
  - `IPCE/aula_8_no_sol.py` is the developer's own file: never commit it with a round.

## 1. Where the work is

- **The target is the Block Components Lab** (https://claude.ai/artifact/Gpjf7yvoYHjGebmmNhuw2o), the record copy
  `_3.3.1_Prototype/Block Components Lab.html`, and its sources `_3.3.1_Prototype/lab_src/` (modules by feature, the build,
  the tests; `PROTOTYPE_STRUCTURE.md` says where a change goes, `README.md` how to prove and publish it).
- **Publishing:** the page goes from the record copy on disk, with `root` `_3.3.1_Prototype` (a redeploy to the same link
  keeps the voice files and the capabilities). From an account that cannot open the link (a read says "not found"): the owner
  shares it with edit access (the page's Share menu), or the session publishes a new artifact: the record copy, `files` listing
  every `voicebench/out/...` file at its own path (186), and the capabilities `db` (rules: `voicerate` read view / write owner,
  `voicerate/{self}` read view / write interact), `user` and `sample` (Ask Azu); then every guide's link is replaced.
- **The spec is `_3.3_POI_Card_Styles.md`**: each part of the card and its Editor as the Lab shows it, with what Unity has today
  (Built / Partly / Not yet + the class). Section 17 lists the open items and the PROPOSED options.
- **The Lab holds every good option of Unity's card** (the audit of 2026-11-01: `lab_src/AUDIT_DECISIONS.md`, `lab_src/audit/`).
  The self-test passes with no bad value.
- **Next:** the developer improves the prototype step by step. First the PROPOSED options, one by one (keep or drop). Then the
  look and flow of each part. When a part is final, it becomes Worker blocks.
- **Unity:** CORE and FULL green at M3; no Worker block since.

## 1b. The guide's references

- **Studies:** `_3.4_The_Guide_Study.md` (voice, read-along, focus, mascot, play, the dock, interactions, the camera),
  `_3.5_Audiences_Study.md`, `_3.6_Guide_Agent_Study.md` (the live agent), `_3.7_Mascot_Pipeline_Study.md` (from a picture to
  the mascot, the Meshy models, `rig_meshy.py`), `_3.8_Guidance_Study.md` (when and how the guide speaks up). The spec is
  `_3.3` section 11 (the Guide) and 11b (Audiences).
- **Voices:** `_3.3.1_Prototype/voicebench/` (`render.py`: azure / eleven / edge / gemini). Published sets: Azure Sonia +
  Raquel, Azure Ryan + Duarte, ElevenLabs Luis + Bella (29 lines a language each); Edge sets are private (`out-private/`).
- **Keys and accounts:** `__Services_And_Keys.md`; every key in `.secrets/keys.env`; the developer replaces every key once it
  all works.

## 2. How a round runs

1. The developer looks at the Lab and writes corrections (with captures).
2. The Architect changes the Lab in the feature's own module (`lab_src/PROTOTYPE_STRUCTURE.md` says which), runs the level the block owes (`lab_src/README.md` "Levels"), takes captures of what changed
   and looks at them (D1 contrast, D5 one focal action, the flow), then publishes to the same link and copies the record copy.
3. The same round, each decision goes into its part's section of `_3.3` as current state (no logs), with the Unity today line.
4. Commit and push as `__AI_Architect.md` section 4 says (the whole workspace, `git add -A`).
- A large, independent piece (an audit across areas, a new view family) goes to subagents in parallel, each with a narrow brief
  and a report table. Subagents return text; the Architect saves the reports. Small changes the Architect makes itself.

## 3. Sessions and context

- One Architect chat per stretch of rounds. When a chat grows long (many captures, many rounds), the Architect updates this file
  and asks for a new chat; the new chat starts from this file, not from memory of the old one.
- Everything a new chat needs lives in the repo: the guide, the Lab sources, the audit, this file. Nothing lives only in a
  session's scratchpad.

## 4. Habits that keep earning their place

1. **Mutation proof.** "Break it, watch the named test go red, restore it."
2. **Look at the render.** Every block had a defect the green tests missed and a capture caught. For the Lab: the self-test proves an
   option reaches the phone; only a capture says it looks right.
3. **Measure before you change, and say the number** (the sheet's stops, contrast ratios).
4. **A differently shaped fixture finds what the big one cannot.**
5. **Audit against the code before a prototype counts as final** (`skills/Feature_Prototyping.md` step 6).
