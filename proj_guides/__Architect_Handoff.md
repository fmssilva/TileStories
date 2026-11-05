# Architect hand-off -- the POI card's prototype rounds (`_3.3`)

Read this first in a new Architect chat, then `__AI_Architect.md`. It says where the work is and how a round runs.

## 0. Start here (next conversation, from 2026-11-04)

- **State:**
  - **The Lab:** v27 is published, with layers through `v9b` (the commit "Move the mouth by Azure's facial curves: live
    answers and the recorded Azure lines; loudness for other voices; Azure the answers' default voice").
  - **Built on 2026-11-03 (second chat):**
    - `v8v`: the card's markers are u, v places on the wall's picture (a drag or zoom moves them with it); the Ribeira wall
      shows its points; the preview bar's Wall button; the console's scene "Ribeira wall: a drag moves the markers with the
      picture" (no model; `test8v.js`, with a mutation proof).
    - `v8w`: Mascot source = 3D model (GLB): Azu the tile, modelled by script in Blender (`proj_guides/mascote/3d/`, `_3.7`
      section 10), drawn by three.js; clips by state, mouth morphs by viseme, blink; Play a clip; Import (names what a GLB
      lacks); Generate from a picture (the generators' table). `test8w.js`.
    - Voices: the Azure Raquel + Sonia and Duarte + Ryan sets, every line, published with the Lab. The partial Gemini set left
      the manifest.
  - **Built on 2026-11-03/04 (third chat):** `v8x`: Mascot source = 3D model > **Model**, one card per model: Azu
    detailed (default, the developer's choice), Azu simple, the mouse, the explorer, the poet, the soldier. The five Meshy
    models are rigged in Blender by `proj_guides/mascote/3d/rig_meshy.py` (`_3.7` section 11); the soldier on Meshy's own
    rig (FBX) with its walk. `test8x.js`, with a mutation proof.
    `v8y`: the tile of light's styles under Mascot source = Tile of light: Glaze (Cobalt / Manganese / Polychrome),
    Pattern (Rosette / Diamond point / Corner sprigs / Plain), Light (Pastel clouds / Glaze colours / Cobalt / Gold),
    Corners (Crisp / Soft / Pillow), and a preview at rest and as light on its own canvas. The defaults are the old look; the
    still Azu (Reduce Motion, the preview) moved to a moment where the face is clear of the highlight. `test8y.js`, with a
    mutation proof.
    `v8z`: the mascot's Size (Small / Medium / Large) and a Character per audience (Looks per audience). `test8z.js`.
    `v9a`: Ask Azu > Voice of the answers: Device / Azure (live, visemes) / ElevenLabs (live, letter times); the mouth on the
    voice's own timing; the device voice without a key or on an error. `test9a.js` (faked services) and a live run by hand.
    Voices: the ElevenLabs set Luis (pt-PT) + Bella (en-US, kept: the developer's choice) rendered by a subagent and
    published with the Lab.
    `v9b`: the real mouth: Azure's facial curves (60 frames a second) for the live answers and the recorded Azure lines (both
    Azure sets rendered again with `--mouth`); the loudness for ElevenLabs; the 3D Azu's mouth and the tile of light's opening
    made larger. Azure is the answers' default voice (the developer's choice). `test9b.js`, with a mutation proof.
  - **MCP:** blender-mcp is at user scope; Blender 5.2.3 with add-on 1.8 on port 9876. In this Blender, new shape keys start
    at value 1 (set them to 0). The add-on's Rodin trial key is empty (`API_INSUFFICIENT_FUNDS`): no generator works until the
    developer adds a key or brings a GLB made by hand on a generator's web. The ElevenLabs MCP still needs `/mcp` >
    Authenticate.
  - **Keys:** Gemini, Azure Speech (region `germanywestcentral`) and ElevenLabs all answer
    (`python proj_guides/_3.3.1_Prototype/lab_src/keys_local.py --check`).
- **The tasks, in this order:**
  1. **The developer looks at the Lab** (v27) and says what to adjust:
     - the six 3D models (6 · Guide > Azu > Mascot source = 3D model > Model, then Play a clip);
     - the tile of light's styles (Mascot source = Tile of light), the Size, a Character per audience (7 · Audiences);
     - the real mouth: the recorded tour with the voice "Azure Raquel" (published Lab), and a question in Ask Azu in the
       local Lab opened from its file (the live Azure voice needs this computer's keys);
     - the markers on a drag and the Wall button (from the second chat).
     - The Editor was audited on 2026-11-04 for the mascots and the AI guide (animations, effects, actions): it covers them;
       the three gaps found (the live answers' voice and mouth, the mascot's size, a character per audience) are built.
  2. **More mascots by the same route** (`rig_meshy.py`: a SPEC entry with the landmarks from a front render; `build_fbx`
     when Meshy's rig comes with it): the cat (`cat_clean.png`) as a quadruped. The Lab page is 14.2 MB of 16 MB: a
     seventh model needs supporting files or 512 px textures. The cat waits for the developer's Meshy model (textured GLB;
     the developer will make it later). The mouse's sleeve is sculpted into its coat, so it cannot lift its arm without
     tearing the coat (tried by script on 2026-11-04, `_3.7` section 11); **decided: the mouse keeps its gentle wave** (an
     A-pose regenerated on Meshy, then Meshy's rig, would be the fix if it is ever wanted).
  3. **The rest of item 6:** Unity import of the GLBs (glTFast, the two layers of `_3.7` section 4) and the tile of
     light's Shader Graph with its style properties, as Worker blocks (the Unity MCP must be connected).
  4. **Voices:** the ElevenLabs set is published (Bella is American; a British EN voice is the developer's choice); the
     app's Worker streams the live voice a sentence at a time (the Lab makes the whole answer first).
  5. **The form of address** (tu / polite / você): a proposal in `_3.5` section 3.9, waiting for the developer's decision.
- **Rules learned this round:**
  - Keys go only in `.secrets/keys.env`, never into a script (`keys_local.py` is committed).
  - MCP servers are added with `--scope user` (memory `mcp-add-needs-user-scope`).
  - Every test runs in the prototype, through controls the developer can press (memory `tests-run-in-the-prototype-by-hand`).
  - blender-mcp: each `execute_blender_code` call starts with a fresh namespace (exec `rig_meshy.py` in every call), and
    `read_homefile` / `open_mainfile` must be a call of their own (the next call has the context back).
  - Script edits with apostrophes or backslashes: write a small Python patch with the Write tool (a bash heredoc mangles
    them; memory `bash-heredoc-apostrophes-use-write-tool`).
  - The live voice's keys reach the local Lab by `keys_local.py` (`keys.local.js`, ignored); the published Lab has none.
  - `IPCE/aula_8_no_sol.py` is the developer's own file: never commit it with a round.

## 1. Where the work is

- **The target is the Block Components Lab** (https://claude.ai/artifact/KDs2Ko1dYdBWYwZVhjvuEp), the record copy
  `_3.3.1_Prototype/Block Components Lab.html`, and its sources `_3.3.1_Prototype/lab_src/` (layers, build, self-test, captures;
  its README says how to change and prove it).
- **The spec is `_3.3_POI_Card_Styles.md`**: each part of the card and its Editor as the Lab shows it, with what Unity has today
  (Built / Partly / Not yet + the class). Section 17 lists the open items and the PROPOSED options.
- **The Lab holds every good option of Unity's card** (the audit of 2026-11-01: `lab_src/AUDIT_DECISIONS.md`, `lab_src/audit/`).
  The self-test passes with no bad value.
- **Next:** the developer improves the prototype step by step. First the PROPOSED options, one by one (keep or drop). Then the
  look and flow of each part. When a part is final, it becomes Worker blocks.
- **Unity:** CORE and FULL green at M3; no Worker block since. Sonnet subagents are rate-limited until 2026-11-05.

## 1b. The guide's stretch: where it stands (2026-11-03)

- **Lab published:** v21. The guide's layers and what each holds:
  - `v8i`: voice from the first step; the read-along light.
  - `v8j`: focus, the spot zoom, Azu points.
  - `v8k`: a lively Azu and play.
  - `v8l`: audiences.
  - `v8m`: lenses.
  - `v8n`: the guide as a person (intro, the invitation ladder, scroll, the voice survives a pause, the page is the phone).
  - `v8o`: the Guide Dock.
  - `v8r`: recorded voices and the blind Voice test.
  - `v8p`: Ask about the view.
  - `v8q`: Azu, the tile of light (WebGL).
  - `v8s`: Azu as the guide's button, its comments, the Belém test wall, a tour opens the card.
  - `v8t`: Ask Azu, the live agent (`_3.3` section 11, `_3.6` section 9).
  - `v8u`: the agent tested in the prototype (test scenes with checks, a trace under each answer, every point named).
  - `v8v`: the markers on the picture, the Ribeira wall's points, the Wall button, the markers' scene.
  - `v8w`: Azu as a 3D model (GLB), Import, Generate from a picture.
  - `v8x`: the 3D model chosen in the Editor (Azu detailed, Azu simple, the mouse, the explorer, the poet, the soldier).
  - `v8y`: the tile of light's styles (Glaze, Pattern, Light, Corners, a preview).
- **Tests:** each layer has its `test8*.js`, listed in `lab_src/README.md`. Run them all after the self-test; `test8r` needs
  two voice sets.
- **Studies (current state):**
  - `_3.4_The_Guide_Study.md`: voice, read-along, focus, mascot, play, sound, the dock, interactions, scroll, intro, the
    camera, the mascot in 3D / UI / AR, MCPs, student plans.
  - `_3.5_Audiences_Study.md`.
  - `_3.6_Guide_Agent_Study.md`: the live agent's architecture, tool manifest, knowledge, models and Editor panel.
  - `_3.7_Mascot_Pipeline_Study.md`: from a picture to the mascot; the body behind and the arm in front; AR walking; formats.
  - The spec is `_3.3` section 11 (the Guide's components) and section 11b (Audiences).
- **Decided by the developer:** the read-along default is the Comet; the Explorer's Azu is Lively.
- **Keys and accounts:** `__Services_And_Keys.md`.
  - Every key is in `.secrets/keys.env` (git ignores it); `keys_local.py` gives the local Lab its keys, and `--check` tests them.
  - The current Gemini key is used to build and test the system. The developer replaces every key once it all works (section 4
    of that doc).
- **Voices:**
  - `_3.3.1_Prototype/voicebench/`: `export_script.js`, `render.py` (azure / eleven / edge / gemini), README.
  - Gemini TTS: the free tier allows 10 requests a day per model; its partial set is no longer published.
  - Best available is the set with the most lines (a published one first when as full): in the published Lab the Azure sets;
    on this computer the Edge set with one line more. The Editor names each set with its line count.
  - Edge sets are private (`out-private/`, ignored, never published: an undocumented service).
- **The developer's accounts:**
  - Azure for Students: active, FCT NOVA directory, no Speech resource yet.
  - Google AI Studio: the key works (text, embeddings, TTS, and the live agent in the local Lab). It was shown in a chat on
    2026-11-03, so it is replaced at the end.
  - ElevenLabs: an account; its MCP page is open.
- **Built on 2026-11-03:** items 1-4 below (`v8s`, `test8s`) and item 5 (`v8t`, `test8t`, `test8t_live`).
- **Asked on 2026-11-03, not yet built:**
  - item 6 (the mascot's sources in the Editor, and the tile of light's styles);
  - the visitor's form of address (proposal in `_3.5` section 3.9, to decide).
- **The developer's rule (2026-11-03):** every test runs in the prototype itself, so it can be redone by hand (the scenes in
  Ask Azu's console); no separate test pages.
- **The asks of 2026-11-03:**
  1. **Play from a half card:** the card opens to Full; if the visitor lowers it to Half, the tour goes on there.
  2. **Azu always present as the guide's button (a FAB):** bottom left, above the nav bar, when the dock is closed and in the
     world view.
  3. **In the world view, Azu replaces the "Ask about…" pill:**
     - it watches where the camera points and makes small comments by itself in a small bubble ("In the middle is the Ribeira
       Palace. Tap to hear more");
     - some bubbles are actionable (a tap opens the card and starts the tour).
  4. **The test wall:** a large tile-wall picture with about 10 demo points; a drag pans and a pinch or the wheel zooms, standing
     in for moving the phone nearer or farther.
  5. **The live agent's architecture, configured in the Editor:**
     - scripted tours only, or also a live agent;
     - the model and provider of the developer's choice;
     - its inputs: text, voice, photo, the app's metadata and camera;
     - its tools: navigate to a page or block, find a feature such as "is there a 3D model?", light points, routes, search the
       library;
     - its knowledge: a RAG library the developer builds (BM25 and vectors, then a re-rank);
     - its outputs: text, voice, navigation, and clickable links in the chat (ranked options);
     - fields for the services' keys, telling the developer what to do;
     - Unity buildable.
  6. **The mascot pipeline from the developer's own picture** (`proj_guides/mascote/`: a tile with a hat and a Belém tower, a
     travelling cat):
     - picture to a usable mascot: platforms, formats;
     - the body behind a UI Toolkit block with the head out and an arm pointing into the block;
     - walking on the wall in AR;
     - the options in the Editor (models, styles, import).
- **Answered in chat on 2026-11-03:** the concrete steps for the accounts (Azure Speech F0 resource keys, the ElevenLabs API key
  and MCP, the Gemini key) and for Blender with blender-mcp.

## 2. How a round runs

1. The developer looks at the Lab and writes corrections (with captures).
2. The Architect changes the Lab in the feature's own module (`lab_src/PROTOTYPE_STRUCTURE.md` says which), runs the self-test and `run_tests.js`, takes captures of what changed
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
