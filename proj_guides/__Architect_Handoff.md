# Architect hand-off -- the POI card's prototype rounds (`_3.3`)

Read this first in a new Architect chat, then `__AI_Architect.md`. It says where the work is and how a round runs.

## 0. Start here (next conversation, from 2026-11-07)

- **State:**
  - **The Lab:** `master` holds guidance rounds 1-4, verified on 2026-11-06/07, published as v33 at the new link (section 1;
    the old link stays at v29). Its code is in modules by feature
    (`lab_src/src/<area>/<module>.js`), with no wrappers: `lab_src/PROTOTYPE_STRUCTURE.md` says where each feature lives and
    where a change goes. The build only joins files (`build.py`) and refuses wrappers, re-assignments of functions, second
    declarations, defaults set twice and version-named modules (`check_structure.js`, `structure_baseline.json` at 0).
  - **Proof of the Lab:** the self-test (`tests/runtest8.ps1`: three walks side by side, about 3 min) gives
    `ok=1179 behaviour=785 bad=0` (round 3, 2.1 min); `node run_tests.js` runs the 28 test files one at a time (about 30
    min; `tests_baseline.json`). test9d takes about 490 s of its 600 s limit: watch it.
  - **Test levels** (decided 2026-11-06; cut twice since: the developer refused 1h30 of tests a round, then a 30-min level 1
    on 2026-11-07). The rules are in `lab_src/README.md` "Levels". Every brief repeats them.
    - **While building:** only the build and captures, iterated until the app and the Editor look right
      (`skills/Feature_Prototyping.md` step 3.3).
    - **Level 1, once a round (5-7 min):** the round's new test file, plus the file of what it replaced, its mutation, and the
      self-test of its tab. No `--changed` run: it pulled 8-10 files.
    - **Level 2, every 3rd-4th round (about 35 min):** every tab and every file. It runs sooner when a round changed existing
      shared code.
    - **Level 3, every 10-12 rounds and before a Unity hand-off:** level 2, every mutation and a new baseline.
    - Count the rounds since the last level 2 and 3 in every answer. After guidance round 4 that is 1 and 4: level 2 ran on
      round 3 on 2026-11-07, and level 3 has not run yet.
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
  - **Guidance rounds 1-4 are done** (merged into `master`; the briefs `_3.8_guidance_plans/brief_round1.md` to
    `brief_round4.md`).
    - Round 2: the memory and the visit, every gate and limit, the stage and the score, the learning, Ask about the view
      and A tour of {point}, Why not now? / The visitor / Log, five console scenes; `tests/test9f.js`.
    - Round 3: Azu's wall remarks became Content › What's in view (one item a point) and the ✦ hint App › The wall ›
      The ✦; their old timers removed; two scenes; `tests/test9g.js`.
    - Round 4: the card Hints became App › The card (cues, a Cue room of 5 s) and How to use the card a pulled
      walkthrough, also from the ✦ menu's How does this work?; Card Controls > Hints a view; `tests/test9h.js`.
    - Their captures were judged by the Architect; the developer reviews later, step by step. The open look questions:
      round 1's PT opening line, what "Mascot Off" means, Azu's size beside the marker; rounds 2 and 3's in the closing
      answers of 2026-11-06/07 (among them: a group's own shorter gap, the remarks counting in Content's cap, The ✦ on the
      card, a ring for The ✦ with the 3D model, "The" mid-sentence in the remark lines).
    - **Next: guidance round 5** (`_3.8` section 8: the simulator, presets, Talkativeness, the Mix table, the audiences'
      preset), as a brief to an Opus subagent, in the shape of `brief_round4.md` with the short level 1. Prototype only (no Unity) until the
      developer says so.
  - **MCP:** blender-mcp at user scope (Blender 5.2.3, add-on 1.8, port 9876). The Unity MCP was not connected this stretch.
  - **Keys:** Gemini, Azure Speech and ElevenLabs answer (`python proj_guides/_3.3.1_Prototype/lab_src/keys_local.py --check`).
- **The tasks, in this order:**
  1. **Guidance rounds 1-10** (`_3.8` section 8), each verified and published, with captures and specific questions for the
     developer. Round 10 gives every remaining Editor row (about 148) its demo (the developer's rule, `skills/Feature_Prototyping.md`
     step 5).
  2. **The developer's look at v29+:** the open questions of the guide's controls (the ✦ above the dock at Mini, the ring at
     Compact, the step's name under Resume, one invitation at a time on the wall, the explorer's size).
  3. **Lab upkeep** (`_3.3` section 17, "The Lab"): the self-test's time (38 min, was 10); split the large shared handlers by
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

- **The target is the Block Components Lab** (https://claude.ai/artifact/QmhHSzTrbJAR3doq1YEoZR), the record copy
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
