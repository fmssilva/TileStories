# Block Components Lab: the prototype's code

This is the code of this prototype, not the Unity project (that is `.clinerules/10-structure.md`). It says where each feature
lives, so a change goes to one place. Update it in the same round as every change (`skills/Feature_Prototyping.md` step 5).

## Rules

- **One home per feature:** its state and defaults, its drawing, its Editor rows, each row's demo and its test.
- **Extension points, not wrappers:** the shared parts call named registries (`ext.js`); a module joins them. Never
  `var fOld=f; f=function(){...fOld()...}`, never a function declared twice, never a default set in two modules.
- **No silent fallback:** when one source stands in for another, the Editor says so at that moment.
- **The build only joins and embeds:** `build.py` joins `MODULES` and `STYLES` in order and puts the assets in for their
  placeholders (`__HERO__`, `__AZUGLB_*__`…). It never rewrites code.
- **The check:** `build.py` runs `check_structure.js`. It counts wrappers, re-assignments, second declarations and defaults set
  twice; `structure_baseline.json` holds the allowed counts, and a count may only go down.

## Build, prove, publish

1. `python build.py` writes `block-components-lab.html` (not committed) and runs the structure check.
2. `runtest8.ps1` runs the self-test (`#selftest`, about 10 minutes, alone): every Editor option, text and demo field must
   change the phone or be classed behaviour. The baseline is `ok=1113 behaviour=622 bad=0`.
3. `node run_tests.js` runs every `test*.js` (not the `*_live` ones) and lists the failed checks. Each test is described in
   `README.md`.
4. Copy the page to `../Block Components Lab.html` (the record copy) and publish it to the Lab's link (`README.md`).

## Where the code is today

The restructure is under way: the code is still in the old layers, in load order. The Editor's and the phone's shared
functions (`edCard`, `edMarker`, `renderEditor`, `renderPhone`, the screens, `modelSig`, `renderEl`, `setOpt`…) have one
definition each, in the layer that first defines them, and call named points (`renderPhone.before`, `edCard.html`,
`cardScreen.html`…); each layer joins them with `on()` from its own file. The guide, the mascot, the world and the agent still
wrap the layer before them.

| Layer | Holds |
|---|---|
| `v3.js` | Opens the shared scope. Catalogues (kinds, motions, effects, patterns, the library), the state `S`, the elements' drawing (`renderEl`), the phone (`renderPhone`, screens, the sheet), the Editor's widgets (`row`, `seg`, `fo`) and its events |
| `ext.js` | The extension points: `on`, `emit`, `emitLatestFirst` (a "before" point), `pipe` (a value through each joiner), `firstAnswer` (the latest joiner with an answer wins) |
| `v4.js` | Detail Card's organisation: sections, the Navigation Bar, the card's Editor (`edCard`, `edMarker`) |
| `v5.js` | The flow (the wall, the marker, the stops) and the self-test |
| `v6.js` | The Guide: the tour, the device voice (`say`), captions, Ask |
| `v7.js` | Texts on their parts, the demo card, Languages & Terms, the Editor's focus map (`FOCUS`, `COMP`) |
| `v8a`-`v8e` | The Unity audit merged: card and sections, every block kind, elements and motion, Specific Marker and languages, the hero and the Editor tools |
| `v8f`-`v8h` | The sheet's details, the demo in one component, the Hints, Loading & states |
| `v8i`-`v8o` | The guide: read-along, focus, a lively Azu, audiences, lenses, the guide as a person, the Guide Dock |
| `v8r`, `v8p` | Recorded voices and the Voice test; Ask about the view |
| `v8q`, `v8s` | Azu, the tile of light (WebGL); Azu as the guide's button, the test wall |
| `v8t`, `v8u` | Ask Azu, the live agent; its test scenes |
| `v8v`-`v8z` | The markers on the picture; the 3D Azu and its models; the tile of light's styles; size and character |
| `v9a`, `v9b` | The live answers' voice; the real mouth |
| `boot.js` | Starts the page (the `#hash` routes) and closes the shared scope |

Styles: `v2.css`, `v3extra.css`, then one `.css` per layer. The page: `head.html`, `page.html`. Words: `realstrings.json`
(the framework's words, with their owner part; `strings.py` re-imports them from Unity's `CardStrings.asset`, and the owners
set here must be kept).

## The target: modules by feature

The rounds move each area out of the layers into `src/<area>/`, one feature per module, its `.css` beside it:

| Area | Modules |
|---|---|
| `core` | `ext.js` (the extension points), `catalogues.js`, `state.js` (the root of `S`), `util.js`, `words.js` |
| `editor` | `shell.js` (tabs, foldouts, rows), `demo.js` (each row's demo), `json.js`, `selftest.js` |
| `phone` | `shell.js` (`renderPhone`, the screens), `sheet.js`, `preview-bar.js`, `settings.js` |
| `card` | `elements.js`, `motion.js`, `blocks.js`, `sections.js`, `hero.js`, `story.js`, `gallery.js`, `viewer.js`, `demo-card.js`, `hints.js`, `states.js`, `locate.js`, `languages.js` |
| `audiences` | `audiences.js`, `lenses.js` |
| `guide` | `tour.js`, `voice.js` (device, recorded and live sources), `readalong.js`, `focus.js`, `person.js`, `dock.js`, `ask.js`, `voice-test.js` |
| `mascot` | `azu2d.js`, `azu-gl.js`, `azu3d.js`, `models.js`, `mouth.js`, `size.js` |
| `world` | `wall.js`, `markers.js`, `camera-ask.js`, `guide-button.js` |
| `agent` | `agent.js`, `tools.js`, `console.js`, `live-voice.js` |

Rounds, each one proven by the self-test and `run_tests.js` with the same results as before, and the structure check lower:

1. The build only joins (done: the page it writes is byte-identical to the old build's).
2. The extension points; the Editor and the phone join them (`edCard`, `edMarker`, `renderEditor`, `renderPhone`, the
   screens, `modelSig`, `renderEl`, `setOpt`) (done).
3. The guide: the tour's steps, the voice (`say`, `stopSpeech`, `markWord`), `renderGuide`, Ask.
4. The mascot, the world and the agent (`camLayer`, `azMouth`, `agPrompt`, `ptool`) and what is left.
5. The code moves into `src/` by feature; the layers go; this file lists the modules; the check allows 0.
