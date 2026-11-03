# Architect hand-off -- the POI card's prototype rounds (`_3.3`)

Read this first in a new Architect chat, then `__AI_Architect.md`. It says where the work is and how a round runs.

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
  - Gemini TTS: the free tier allows 10 requests a day per model, so its set holds 7 of 106 lines.
  - Best available is the set with the most lines: on this computer the full Edge sets; in the published Lab the Gemini set (the
    only one there), whose missing lines use the device voice. The Editor names each set with its line count.
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
2. The Architect changes the Lab in a new layer (`lab_src/v8v.js`, then `v8w`…), runs the self-test, takes captures of what changed
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
5. **Audit against the code before a prototype counts as final** (`skills/Feature_Prototyping.md` step 5).
