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

The restructure is under way: the code is still in the old layers, in load order, but nothing wraps or re-assigns a
function any more (the check counts 0 of each). Every shared function has one definition, in the layer that first defines it
(or, where a later layer replaced its whole body, in that layer), and calls named points (`renderPhone.before`,
`edCard.html`, `runStep.after`, `say.source`…); each layer joins them with `on()` from its own file. The self-test silences
the drawing with the flag `noDraw` (v3), not with stub functions.

**The voice.** `say` (v6) asks its sources, the latest first: the live answers (`lvSay`, v9a: only `G.answer`, with
`liveVoice` Azure or ElevenLabs and its key), then a recorded line (`recSay`, v8r: Voice = Recorded files and the line
rendered); if none takes the line, the device voice (`sayDevice`, v6) speaks it. A source that fails hands the line to the
ones older than it (`sayFrom`; v9a's `lvDevice`). Then `say.after` (v9b: the recorded line's mouth). The device voice has
`sayDevice.before` (v8o the answer in the thread, v8n the resume point, v8i the read-along), `.wait` (v8i waits for the
browser's voices), `.started`, `.ended`, `.after` (v8n: retry a failed voice). `camSay` (v8p) is built the same way
(`camSay.source`: v9a's `lvCamSay`).

| Layer | Holds |
|---|---|
| `v3.js` | Opens the shared scope. Catalogues (kinds, motions, effects, patterns, the library), the state `S`, the elements' drawing (`renderEl`), the phone (`renderPhone`, screens, the sheet), the Editor's widgets (`row`, `seg`, `fo`) and its events |
| `ext.js` | The extension points: `on`, `emit`, `emitLatestFirst` (a "before" point), `pipe` (a value through each joiner), `firstAnswer` (the latest joiner with an answer wins), `answerBelow` (the same, from the joiners older than one) |
| `v4.js` | Detail Card's organisation: sections, the Navigation Bar, the card's Editor (`edCard`, `edMarker`) |
| `v5.js` | The flow (the wall, the marker, the stops) and the self-test |
| `v6.js` | The Guide: the tour (`startTour`, `runStep`, `next`), the voice (`say`, `sayDevice`), captions (`renderGuide`), Ask (`ask`) |
| `v7.js` | Texts on their parts, the demo card's screen, Languages & Terms, the preview bar (`ptool`), the Editor's focus map (`FOCUS`, `COMP`) |
| `v8a`-`v8e` | The Unity audit merged: card and sections, every block kind (and the demo card, `S.demoCard`), elements and motion, Specific Marker and languages, the hero and the Editor tools |
| `v8f`-`v8h` | The sheet's details (`initGrab`, `fitHero`, `locFit`), the demo in one component (`testTile`), the Hints, Loading & states |
| `v8i`-`v8o` | The guide: read-along (`voiceFor`, `markWord`), focus, a lively Azu (`azMouth`), audiences, lenses, the guide as a person (`pauseTour`, `resumeTour`, `gesture`), the Guide Dock (`pointAt`, `freeHeight`, `heard`) |
| `v8r`, `v8p` | Recorded voices (a source of `say`) and the Voice test; Ask about the view (`camLayer`, `camSay`, `camAsk`) |
| `v8q`, `v8s` | Azu, the tile of light (WebGL); Azu as the guide's button (`fabLayer`), the test wall (`camGeom`, `camPaint`) |
| `v8t`, `v8u` | Ask Azu, the live agent (`agentAsk`, `agPrompt`…); its test scenes (`agScene`, `agRunScene`) |
| `v8v`-`v8z` | The markers on the picture; the 3D Azu and its models (`az3dModel`); the tile of light's styles; size and character |
| `v9a`, `v9b` | The live answers' voice (a source of `say` and `camSay`; the default `liveVoice`); the real mouth (`lvAzure` with the facial curves) |
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
3. and 4. (one round) Everything left: the guide (the tour's steps, the voice `say` with its sources, `stopSpeech`,
   `markWord`, `renderGuide`, Ask), the mascot, the world and the agent (`camLayer`, `azMouth`, `agPrompt`, `ptool`…), the
   self-test's silenced drawing (a flag), and each default set once (done: the check counts 0 / 0 / 0 / 0).
5. The code moves into `src/` by feature; the layers go; this file lists the modules; the check allows 0.
