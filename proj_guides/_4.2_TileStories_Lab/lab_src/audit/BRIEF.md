# Audit brief: Unity's card options vs the Block Components Lab

## Goal
TileStories is a Unity 6.3 framework for AR walls of points of interest (POIs); each POI opens a **detail card** (a bottom sheet on
the phone). The developer is redesigning the card and its Editor in an HTML prototype, the **Lab**. The Lab will become the spec
the Unity implementation follows. Before the final polish, the Lab must hold **every good option a good framework should offer**,
with good values and defaults, and must leave out options that are not relevant or not good.

Your job: for your AREA (below), walk **every** Editor row, config value and phone UI element that Unity has today, and say for each
whether the Lab already has it, and what the Lab should do. Be exhaustive: one missed option is the failure this audit exists to
prevent. Read the code; do not guess from names.

**Read-only.** Do not edit any file in the repo or in the Lab. Write only your report file.

## Where things are
- Unity project: `C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\`
  - Editor tab "Detail Card": `Editor/POIEditor/DetailCard/` (entry `POIEditorToolWindow.DetailCard.cs`; `*Help.cs` hold the (?) texts).
  - Editor tab "Specific Marker": `Editor/POIEditor/SpecificMarker/`.
  - The config schema: `Runtime/Core/CardConfigData.cs` (and `CardOptions` constants in it). Block kinds: `Runtime/Blocks/BuiltInBlocks.*.cs`
    (each kind's fields, options, defaults). Elements: `Runtime/Blocks/Elements/`. UI: `Runtime/UI/` (Screen, Components, Theme, LookKit).
  - Shipped configs that show real values: search `**/config.json` under `TileStories/Assets` (the LivingRoom wall).
- The target spec (current state, per part, with "Unity today" lines): `C:\Users\franc\Desktop\TileStories\proj_guides\_3.3_POI_Card_Styles.md`.
  Section 1 holds the FIXED RULES (contrast >= 7:1, no free colours, Reduce Motion, no auto-advance...). An option that would break a
  fixed rule is never "good".
- The Lab (single HTML page built from JS layers): sources in
  `C:\Users\franc\AppData\Local\Temp\claude\c--Users-franc-Desktop-TileStories\31de4da7-ffdb-4231-b5f8-5872be472ef8\scratchpad\lab\`
  - **Start with the outline of what the Lab's Editor shows**, per tab: `audit\lab_editor_card.txt` (Detail Card), `lab_editor_marker.txt`,
    `lab_editor_words.txt` (Languages & Terms), `lab_editor_global.txt`. Format: `[foldout-id] title`, then `- Row label :: choice | choice * | ? (?) help`
    (`*` = the default; `?` is the help button; "Aa N words" is the texts button). The marker tab's outline is partial: read the JS too.
  - Then the JS for detail (state defaults, catalogues, what the phone draws): `v3.js` (state `S`, catalogues: element kinds and styles,
    patterns, motion presets, interactions, effects, the block library `LIB`), `v4.js` (Detail Card sections, sections, card controls),
    `v5.js` (phone views, pages), `v6.js` (Guide), `v7.js` (texts on their parts, demo card, terms; it re-arranges the Editor into
    components by moving rows). `strings.py` maps each interface word to its owner component.

## What "good" means (the developer's principles)
- A **framework**: every good option a wall author would really use, each with good choices and a good default; nothing that only
  existed for an old implementation, a debug need, or a look that breaks a fixed rule.
- **When Unity and the Lab offer different choices for the same thing, merge the good choices of both** into one option (for example
  Unity's "switch with a fade / queue" and the Lab's "lower / pause / mix" for two sounds at once become one list with every good choice).
- **One place per thing.** Each option lives on the ONE UI element (component foldout) that draws or does it; texts on the element that
  shows them. Three levels where it applies: card -> section -> block, the nearest wins.
- Closed lists, not free values, where a free value could break contrast or consistency.
- Editor help: every option has a one-line (?) that says what the visitor sees.

## Report: one file, `audit\report_<AREA-ID>.md`
For each Unity Editor section / config group / phone UI element in your area, a heading, then a table:

| Unity (Editor label · config key · file:line) | Choices · default | Lab today (foldout id › row, or "missing") | Verdict | Lab change |
|---|---|---|---|---|

- **Verdict** is one of: `SAME` (the Lab has it, same meaning; note value differences), `ADD` (good, missing from the Lab), `MERGE`
  (both have it with different choices: give the merged list), `DROP` (not good / not relevant: give the reason in one line),
  `RENAMED` (the Lab has it under another name or place: say where).
- **Lab change** (for ADD / MERGE): the target foldout (an existing Lab foldout id, or a new component with its phone-facing name),
  the row label, the choices with the default marked, the one-line help, and whether it changes what the phone draws
  (`draws`) or only behaviour (`behaviour`: sound, timing, taps).
- After the tables: **Lab-only** (things the Lab has that Unity lacks, one line each, only if you notice them), then
  **Gaps** (at most 6: good options NEITHER has that a framework of this kind should offer; each with one line of reasoning;
  mark them clearly as proposals), then **Open questions** for the developer (only real decisions, at most 5).
- Last line: counts (rows checked, SAME / ADD / MERGE / DROP / RENAMED).

Be concrete (real labels, real keys, real defaults, file:line). Plain English, short sentences. Finish by returning a 10-line summary
of your verdicts (the ADD and MERGE items by name) as your final message.
