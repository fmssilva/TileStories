# Architect hand-off -- the POI card's prototype rounds (`_3.3`)

Read this first in a new Architect chat, then `__AI_Architect.md`. It says where the work is and how a round runs.

## 0. Start here (next conversation, from 2026-11-07)

- **State:**
  - **The Lab:** `master` holds guidance round 1, verified on 2026-11-06 (the Guide tab, the opening message, Tap a marker,
    the wall's pill removed). The published link still shows v29: this session's account could not open the artifact, so
    v30 waits for the developer (share it with edit access, or a new artifact: section 1). Its code is in modules by feature
    (`lab_src/src/<area>/<module>.js`), with no wrappers: `lab_src/PROTOTYPE_STRUCTURE.md` says where each feature lives and
    where a change goes. The build only joins files (`build.py`) and refuses wrappers, re-assignments of functions, second
    declarations, defaults set twice and version-named modules (`check_structure.js`, `structure_baseline.json` at 0).
  - **Proof of the Lab:** the self-test (`tests/runtest8.ps1`: three walks side by side, about 3 min) gives
    `ok=1180 behaviour=634 bad=0`; `node run_tests.js` runs the 26 test files one at a time (643 checks, about 28 min; `tests_baseline.json`).
  - **Test levels (decided 2026-11-06):** blocks of work; level 1 every block (the block's own tests: `run_tests.js --changed`,
    the self-test of its tab), level 2 every 3rd-4th block, on shared ground and before a publish or a merge, level 3 every
    10-12 blocks and before a Unity hand-off. The rules: `lab_src/README.md` "Levels". Count the blocks since the last level 2
    and 3 in every answer (both counts start at 0 on 2026-11-06, when level 2 ran; the first level 3, with every test's
    mutation, is owed by the 12th block).
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
  - **Guidance round 1 is done** (merged into `master`; the brief `_3.8_guidance_plans/brief_round1.md`). Its captures were
    judged by the Architect; the developer's look questions are open (the PT opening line, what "Mascot Off" means, Azu's size
    beside the marker). **Next: guidance round 2** (`_3.8` section 8), as a brief to an Opus subagent.
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

- **The target is the Block Components Lab** (https://claude.ai/artifact/KDs2Ko1dYdBWYwZVhjvuEp), the record copy
  `_3.3.1_Prototype/Block Components Lab.html`, and its sources `_3.3.1_Prototype/lab_src/` (modules by feature, the build,
  the tests; `PROTOTYPE_STRUCTURE.md` says where a change goes, `README.md` how to prove and publish it).
- **Publishing from another account:** the artifact is private to the developer's account. To keep the same link, the
  owner shares it with edit access (the page's Share menu); that account's session then reads the live version and publishes
  to it. Otherwise a new artifact: publish `_3.3.1_Prototype/Block Components Lab.html` with `files` mapping every
  `voicebench/out/...` file (184, from `_3.3.1_Prototype/voicebench/out/`, the same paths) and the `db` capability (rules:
  `voicerate` read view / write owner, `voicerate/{self}` read view / write interact), then give the developer the new link.
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
