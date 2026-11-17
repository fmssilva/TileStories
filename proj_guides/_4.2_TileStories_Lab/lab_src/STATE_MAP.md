# The State Map: where the Lab's state lives

This is the code of this prototype, not the Unity project. The Lab's state lives in four roots, made by `src/core/state.js`:

- **`D`: what the author sets.** The Editor writes it, and every Editor change is one step of Undo / Redo (`src/core/undo.js`).
  Each part names its home in Unity, so the Lab's data has the shape of Unity's data:
  `D.guide` and `D.agent` (a new guide-config asset), `D.tour` (the tour per marker), `D.guidance` (the guidance catalogue),
  `D.card` and `D.blocks` (the wall's `config.json`, `card_settings`), `D.poi` (the open point's `POIData` / `POICardData`),
  `D.wall` (`WallConfigData`), `D.words` (`CardStrings.asset`).
- **`P`: the visitor's profile and choices.** Settings, the onboarding and the tour's own buttons write it; Forget clears the
  visitor's Settings. It never writes `D`. Where the visitor's choice and the author's default name the same thing, the visitor's
  sits in `P` over the author's in `D` (`P.guide` over `D.guide`, read through `guideSetting(k)`).
- **`ui`: what is happening now.** The phone's view and stop, the dock's height, the Editor's tab, selection and foldouts, timers.
- **`LAB`: the Lab's own tools,** never ported (`PROTOTYPE_STRUCTURE.md`, "Lab-only"): Preview & Test, the preview bar, the
  self-test, the probe (`#probe`: `window.LAB` and its `run`), the voice test.

**Rules**
- New state goes in the root of its kind; a `D` part is the Unity asset or class that will hold it.
- A key that mixes kinds is split by field (the rows below with a field: `camCfg.wall`, `m.solemn`, the old `ui`'s fields, the
  guide's voice, speed and captions).
- Names follow Unity where `.clinerules/10-structure.md` names the field; otherwise the old names are kept.
- An Editor path names its root: `"@D.guide.voice"`, `rowDemos({"D.guide.voice": ...})`; `setPath` and `getPath` start at the
  root the path names.
- "(guess)" marks a home or a root that is a judgement; the open ones are listed at the end.
- `check_structure.js` counts the references to the old root (`S.`): 0 allowed.
- **The router's state** (`ui.view`, `ui.stop`, `ui.history`, `ui.overlays`, `ui.device`) is written only in `src/screen/`: every
  change of what the phone shows goes through `screen/router.js` (`goScreen`, `sheetTo`, `setOverlay`, `back`) and emits `screenChanged`;
  `check_structure.js` counts a write anywhere else (0 allowed).

## The map

One row per key of the old root `S`, and a row for each field split off. Writers: the modules that set the key or a field of it.

| Old key | What it holds | New path | Unity home | Writers |
|---|---|---|---|---|
| `guideCfg` | the guide's settings: the mascot, its look and voice set, the tour's panel, words, music, games, the end, the saved place | `D.guide` | new: guide-config asset (GuideConfig) | agent/live-voice, audiences/audiences, guidance/scenes, guidance/service, guide/autotour, guide/controls, guide/demo-hand, guide/dock, guide/focus, guide/game-ask, guide/guide-scenes, guide/hero-start, guide/highlight, guide/mascot-profile, guide/panel-look, guide/person, guide/saved-place, guide/teleprompter, guide/tell-more, guide/tour, guide/tour-end, guide/tour-scroll, guide/voice, mascot/azu-gl, mascot/azu2d, mascot/models, mascot/size, mascot/tile-styles, world/guide-button |
| `guideCfg.captions` (the visitor's) | the visitor's choice in Settings and the tour's CC / Aa buttons | `P.guide.captions` | -- (the visitor's profile on the phone: PlayerPrefs; guess) | world/guide-button (Settings › The guide), guide/tour |
| `guideCfg.speed` (the visitor's) | the visitor's choice in Settings | `P.guide.speed` | -- (the visitor's profile on the phone: PlayerPrefs; guess) | world/guide-button (Settings › The guide) |
| `guideCfg.voice` (the visitor's) | the visitor's choice in Settings | `P.guide.voice` | -- (the visitor's profile on the phone: PlayerPrefs; guess) | world/guide-button (Settings › The guide) |
| `agentCfg` | Ask the mascot: the model, its limits, the spoken answer, the tools | `D.agent` | new: guide-config asset, its Ask part (guess) | agent/agent, agent/live-voice, agent/scenes |
| `guideSteps` | the open point's tour script (the Guide script edits it; a playing tour swaps in a filtered copy) | `D.tour.guideSteps` | new: the tour asset per marker (GuideTour; guess) | audiences/audiences, guidance/present, guide/autotour, guide/tour |
| `tours` | each point's tour: written or from the card, the written one, made / dismissed | `D.tour.tours` | new: the tour asset per marker (GuideTour; guess) | guide/autotour |
| `tourQ` | the tour's quiz questions per step (words EN / PT, hints, reviewed) | `D.tour.tourQ` | new: the tour asset per marker (GuideTour; guess) | guidance/catalog, guidance/editor, guidance/scenes |
| `tourGuess` | Guess the year per step | `D.tour.tourGuess` | new: the tour asset per marker (GuideTour; guess) | guidance/catalog, guidance/editor, guidance/scenes |
| `tourWord` | Don't say it!: the word and its span per language | `D.tour.tourWord` | new: the tour asset per marker (GuideTour; guess) | guidance/catalog, guidance/editor, guidance/scenes |
| `tourFind` | Find it on the wall: the point, after which step, its name | `D.tour.tourFind` | new: the tour asset per marker (GuideTour; guess) | guidance/catalog, guidance/scenes |
| `ptJokes` | each point's jokes (string keys) | `D.tour.ptJokes` | new: the tour asset per marker (GuideTour; guess) | guidance/catalog, guidance/editor, guidance/scenes |
| `guideAsks` | Ask's suggested questions (the dock's chips) | `D.tour.guideAsks` | new: the tour asset per marker (GuideTour; guess) | guide/dock |
| `guidance` | the guidance catalogue: every tip, remark, walkthrough and page, its group, gaps and presets | `D.guidance` | new: GuidanceCatalog asset (guess) | guidance/catalog, guidance/editor, guidance/scenes, guidance/service |
| `cardCfg` | the card's container: on, the opening stop, heights, mode, layout, the top band, audio rules | `D.card.cardCfg` | config.json card_settings (CardConfigData.CardSettings) | card/controls, card/sections |
| `secCfg` | the sections' look: open or cards, the start, dividers, colour, glass and rim | `D.card.secCfg` | config.json card_settings.sections (CardSectionSettings) | card/block-looks, card/blocks, card/sections |
| `sections` | the sections: name, icon, colour, shown on the bar, start, their block kinds | `D.card.sections` | config.json card_settings.sections (CardSectionSettings) | card/sections, phone/settings |
| `ctl` | the card controls: the Navigation Bar, jump, Open Row, breadcrumb, Previous / Next, Locate, sticky action, contents, terms, viewer, player | `D.card.ctl` | config.json card_settings.components (CardComponentSettings) and .locate (CardLocateSettings) | card/controls, card/sections |
| `blockDefs` | each block kind's defaults (entrance, tap, open row, page, chrome, options) | `D.card.blockDefs` | config.json card_settings: the Block Library (guess) | card/blocks |
| `states` | Loading & states: the look of each state, the loader delay | `D.card.states` | config.json card_settings (CardConfigData.CardSettings) (guess) | card/states |
| `motionLevel` | the card's motion level (full / reduced) | `D.card.motionLevel` | config.json card_settings (CardConfigData.CardSettings) (guess) | card/controls, phone/settings |
| `hints` | the card's cues group On (The card (cues)) | `D.card.hints` | config.json card_settings (CardConfigData.CardSettings) (guess) | card/hints |
| `fb` | haptics and sounds: on, strength, volume, per event | `D.card.fb` | config.json card_settings (CardConfigData.CardSettings) (guess) | phone/feedback (Settings' Feedback writes P.vis.feedback only) |
| `lensCfg` | the See it as chips: on, on the story page, shown from | `D.card.lensCfg` | config.json card_settings (CardConfigData.CardSettings) (guess) | audiences/lenses |
| `motions` | the motions: entrances, transitions, scroll, emphasis, imported | `D.blocks.motions` | config.json card_settings: the Block Library (guess) | core/state, editor/shell |
| `inter` | the interactions and their options | `D.blocks.inter` | config.json card_settings: the Block Library (guess) | core/state |
| `effects` | the effects: edges, lines, light, moving | `D.blocks.effects` | config.json card_settings: the Block Library (guess) | core/state |
| `styles` | the element styles per kind (the default starred) | `D.blocks.styles` | config.json card_settings: the Block Library (guess) | card/elements, core/state, editor/shell |
| `patterns` | the patterns (rows of element + style) | `D.blocks.patterns` | config.json card_settings: the Block Library (guess) | core/state, editor/shell |
| `family` | the motion family (the default entrance) | `D.blocks.family` | config.json card_settings: the Block Library (guess) | core/state, editor/shell |
| `replay` | entrances replay every pass or once | `D.blocks.replay` | config.json card_settings: the Block Library (guess) | core/state, editor/shell |
| `card` | the open point's card: its blocks in order with their preview settings | `D.poi.card` | config.json POIData / POICardData: the open point's | card/demo-card, core/content, editor/marker-tab |
| `hero` | the open point's hero: title, eyebrow, line, focus, pictures, crop | `D.poi.hero` | config.json POIData / POICardData: the open point's | card/hero, core/content |
| `story` | the open point's story page: rows, patterns, content | `D.poi.story` | config.json POIData / POICardData: the open point's | core/content |
| `poiSec` | sections hidden on this point | `D.poi.poiSec` | config.json POIData / POICardData: the open point's (guess) | card/sections |
| `previewRows` | the Specific Marker's preview rows (what the card shows) | `D.poi.previewRows` | config.json POIData / POICardData: the open point's (guess) | editor/marker-tab |
| `galleryImgs` | the gallery page's pictures | `D.poi.galleryImgs` | config.json POIData / POICardData: the open point's (guess) | card/gallery |
| `wallPois` | the wall's points: name, place on the wall, year, card, category | `D.wall.wallPois` | config.json WallConfigData | world/camera-ask, world/wall |
| `camCfg` | Ask about the view: on, names on the points, suggestions, the button (its field wall is LAB.wall) | `D.wall.camCfg` | config.json WallConfigData (guess) | agent/scenes, guide/tour-end, world/camera-ask, world/markers, world/wall |
| `camCfg.wall` | the Lab's test wall standing in for the camera (Preview & Test) | `LAB.wall` | Lab-only | agent/scenes, guide/tour-end, world/markers, world/wall |
| `langs` | the wall's languages, the fallback, the opening language | `D.wall.langs` | config.json WallConfigData (guess) | card/languages, editor/texts |
| `terms` | the terms: glossary words and synonym groups (WallConfigData.synonym_groups) | `D.wall.terms` | config.json WallConfigData (guess) | card/terms, editor/texts |
| `lenses` | the point's topics (lenses) | `D.wall.lenses` | config.json WallConfigData (guess) | audiences/lenses |
| `tags` | each block's, row's and step's topics | `D.wall.tags` | config.json WallConfigData (guess) | audiences/lenses |
| `aud` | the audiences: where Who is exploring is asked, its looks, the parts each hides (seen: the visitor answered; P1b) | `D.wall.aud` | config.json WallConfigData (guess) | audiences/audiences, guidance/scenes, guidance/service, mascot/size |
| `tx` | the texts: every visitor word, EN / PT | `D.words.tx` | CardStrings.asset (CardStringTable; and the app's and the wall's tables) | card/languages, core/words, guidance/editor, guide/autotour, guide/mascot-profile |
| `texts` | the Card Texts table (Shared Content) | `D.words.texts` | CardStrings.asset (CardStringTable; and the app's and the wall's tables) (guess) | card/sections |
| `glossary` | the Glossary table (Shared Content) | `D.words.glossary` | CardStrings.asset (CardStringTable; and the app's and the wall's tables) (guess) | card/sections |
| `ov` | the texts written per audience (one entry per field and audience) | `D.words.ov` | CardStrings.asset (CardStringTable; and the app's and the wall's tables) (guess) | audiences/audiences |
| `vis` | the visitor's Settings: language, mode, motion, text size, feedback; audience, length, speed, music | `P.vis` | -- (the visitor's profile on the phone: PlayerPrefs; guess) | audiences/audiences, card/controls, guidance/editor, guidance/scenes, guidance/service, guide/controls, guide/tour, guide/voice-script, guide/voice-test, phone/redraw, phone/settings, world/guide-button |
| `gdVisitor` | the guidance's memory of the visitor: this visit, every visit, what was used and seen, the prefs | `P.gdVisitor` | -- (the visitor's profile on the phone: PlayerPrefs; guess) | guidance/editor, guidance/scenes, guidance/service |
| `lang` | the language shown (Settings; the preview language) | `P.lang` | -- (the visitor's profile on the phone: PlayerPrefs; guess) | card/controls, card/demo-card, core/state, editor/card-tab, editor/selftest-texts, editor/texts, guidance/editor, guidance/present, guidance/scenes, guide/voice-script, guide/voice-test, phone/shell |
| `reduce` | Reduce motion, as resolved (Settings, the preview bar) | `P.reduce` | -- (the visitor's profile on the phone: PlayerPrefs; guess) | card/controls, card/demo-card, core/state, guidance/scenes, phone/sheet |
| `dark` | dark mode, as resolved (Settings, the card mode, the preview bar) | `P.dark` | -- (the visitor's profile on the phone: PlayerPrefs; guess) | card/controls, core/state, editor/card-tab, phone/redraw |
| `introSeen` | the mascot's introduction was heard | `P.introSeen` | -- (the visitor's profile on the phone: PlayerPrefs; guess) | editor/demo, guidance/editor, guidance/present, guidance/scenes, guide/autotour, guide/guide-scenes, guide/person, guide/tell-more, guide/tour-end |
| `guideMute` | the visitor muted the guide's voice | `P.guideMute` | -- (the visitor's profile on the phone: PlayerPrefs; guess) | guidance/scenes, guide/person, guide/tour |
| `kqDone` | the knowledge checks answered (cleared by Forget) | `P.kqDone` | -- (the visitor's profile on the phone: PlayerPrefs; guess) | phone/settings |
| `lens` | the See it as chip chosen | `P.lens` | -- (the visitor's profile on the phone: PlayerPrefs; guess) | audiences/lenses |
| `view` | the screen now (card: the wall with the card sheet over it, story, gallery, kpage, viewer, term, explore, demo, democard) | `ui.view` | -- (runtime: the UI Toolkit screen stack's top) | screen/router (go, back; every module asks it) |
| `stop` | the card sheet's stop now (closed, peek, half, full): the wall screen's base overlay | `ui.stop` | -- (runtime) | screen/router (go, sheetTo; every module asks it) |
| `dockH` | the Guide Dock's height now | `ui.dockH` | -- (runtime) | agent/scenes, editor/demo, guidance/present, guidance/scenes, guide/autotour, guide/controls, guide/dock, guide/game-ask, guide/guide-scenes, guide/teleprompter, guide/tell-more, guide/tour-end, world/guide-button |
| `tab` | the Editor's open tab | `ui.tab` | -- (runtime) | core/boot, core/state, editor/demo, editor/guide-tab, editor/selftest, editor/selftest-texts, editor/shell, editor/texts, editor/words-tab, guidance/editor |
| `step` | the page's flow step (1-5) | `ui.step` | -- (runtime) | core/state, editor/shell |
| `sel` | the Editor's selection (layer, kind, style, block, row, step, term...) | `ui.sel` | -- (runtime) | audiences/audiences, card/blocks, card/gallery, card/sections, card/terms, core/boot, core/content, core/state, editor/card-tab, editor/marker-tab, editor/selftest, editor/shell, editor/texts, editor/words-tab, guidance/editor, guide/autotour, guide/tour |
| `op` | the Editor's foldouts open or closed | `ui.op` | -- (runtime) | card/sections, core/boot, core/state, editor/accordion, editor/card-tab, editor/demo, editor/guide-tab, editor/selftest, editor/selftest-texts, editor/shell, editor/texts, editor/words-tab, guidance/editor |
| `settingsOpen` | gone: the Settings panel is the overlay "settings" | `ui.overlays` | -- (runtime) | screen/router (setOverlay) |
| `contentsOpen` | gone: the Contents sheet is the overlay "contents" | `ui.overlays` | -- (runtime) | screen/router (setOverlay) |
| `activeSec` | the section in view | `ui.activeSec` | -- (runtime) | card/contents, card/controls, card/sections, phone/sheet |
| `openSecs` | the accordion sections open | `ui.openSecs` | -- (runtime) | card/contents, card/controls, card/sections, phone/sheet |
| `pageKind` | the kind of the page open | `ui.pageKind` | -- (runtime) | card/blocks, card/pages |
| `viewerAt` | the picture viewer's picture | `ui.viewerAt` | -- (runtime) | card/pages |
| `_pk` | the page kind the chrome draws for | `ui._pk` | -- (runtime) | card/chrome, card/pages |
| `heroH` | the hero's height, measured | `ui.heroH` | -- (runtime) | card/hero, phone/redraw |
| `lastStop` | the stop at the last draw | `ui.lastStop` | -- (runtime) | phone/redraw |
| `lastView` | the view at the last draw | `ui.lastView` | -- (runtime) | phone/redraw |
| `stopMoved` | the last draw moved the stop | `ui.stopMoved` | -- (runtime) | phone/redraw |
| `justOpened` | the last draw opened the card | `ui.justOpened` | -- (runtime) | phone/redraw |
| `movedAt` | when the stop last moved | `ui.movedAt` | -- (runtime) | guide/hero-start, phone/redraw |
| `locShown` | the Locate label's showings | `ui.locShown` | -- (runtime) | card/hints, card/locate |
| `locFolded` | the Locate label is folded | `ui.locFolded` | -- (runtime) | card/hints, card/locate |
| `fitT` | the Locate label's fit timer | `ui.fitT` | -- (runtime) | card/locate |
| `_gseen` | the terms underlined in this draw | `ui._gseen` | -- (runtime) | card/blocks, card/controls |
| `_lastY` | the sticky action's last scroll | `ui._lastY` | -- (runtime) | card/controls |
| `stSlow` | the loader's slow timer | `ui.stSlow` | -- (runtime) | card/states |
| `stTimer` | a state's timer | `ui.stTimer` | -- (runtime) | card/states |
| `_cb` | the block being drawn | `ui._cb` | -- (runtime) | card/block-looks |
| `beforeDemo` | the view before the demo card | `ui.beforeDemo` | -- (runtime) | phone/redraw |
| `termOpen` | the term whose page is open | `ui.termOpen` | -- (runtime) | card/terms, core/boot, editor/texts |
| `guideMuteOnce` | the voice muted for one retry | `ui.guideMuteOnce` | -- (runtime) | guide/person |
| `audApplied` | the audience last applied | `ui.audApplied` | -- (runtime) | audiences/audiences |
| `audGd` | the audience's guidance preset last applied | `ui.audGd` | -- (runtime) | audiences/audiences |
| `audCharApplied` | the audience's character last applied | `ui.audCharApplied` | -- (runtime) | mascot/size |
| `ovT` | the per-audience text redraw timer | `ui.ovT` | -- (runtime) | audiences/audiences |
| `lensT` | the lens name redraw timer | `ui.lensT` | -- (runtime) | audiences/lenses |
| `howSaved` | How does this work: the tour kept while its walk plays | `ui.howSaved` | -- (runtime) | guidance/present |
| `fullSteps` | the whole tour while a filtered one plays | `ui.fullSteps` | -- (runtime) | audiences/audiences |
| `camX` | the camera's pan now | `ui.camX` | -- (runtime) | agent/agent, agent/scenes, guidance/scenes, guide/tour-end, world/camera-ask, world/markers, world/wall |
| `camY` | the camera's tilt now | `ui.camY` | -- (runtime) | agent/agent, agent/scenes, guidance/scenes, guide/tour-end, world/markers, world/wall |
| `camZoom` | the camera's zoom now | `ui.camZoom` | -- (runtime) | agent/scenes, guidance/editor, guidance/scenes, guide/tour-end, world/markers, world/wall |
| `atNote` | the Editor's note on a tour step | `ui.atNote` | -- (runtime) | guide/autotour |
| `atAINote` | the Editor's note on the AI tours | `ui.atAINote` | -- (runtime) | guide/autotour |
| `atDecide` | the point whose written tour waits for a decision | `ui.atDecide` | -- (runtime) | guide/autotour |
| `atAIBusy` | the AI tours are being drafted | `ui.atAIBusy` | -- (runtime) | guide/autotour |
| `mpNote` | the Editor's note on the mascot's intro | `ui.mpNote` | -- (runtime) | guide/mascot-profile |
| `mpBusy` | the mascot's intro being drafted | `ui.mpBusy` | -- (runtime) | guide/mascot-profile |
| `tqNote` | the Editor's note on the quiz | `ui.tqNote` | -- (runtime) | guidance/editor, guide/game-ask |
| `tqBusy` | a quiz line being drafted | `ui.tqBusy` | -- (runtime) | guidance/editor |
| `gaBusy` | a game hint being drafted | `ui.gaBusy` | -- (runtime) | guide/game-ask |
| `ovNote` | the Editor's note on the per-audience texts | `ui.ovNote` | -- (runtime) | audiences/audiences |
| `ovBusy` | a per-audience text being drafted | `ui.ovBusy` | -- (runtime) | audiences/audiences |
| `starter` | the Specific Marker's card starter chosen | `ui.starter` | -- (runtime) (guess) | editor/marker-tab |
| `pendingStarter` | a card starter waiting for its confirmation | `ui.pendingStarter` | -- (runtime) | editor/marker-tab |
| `m` | the open point (m.poi); its field solemn is D.poi.solemn | `ui.m` | -- (runtime) | core/content, guidance/scenes, mascot/azu2d |
| `m.solemn` | Solemn point (Specific Marker) | `D.poi.solemn` | config.json POIData / POICardData: the open point's (guess) | guidance/scenes, mascot/azu2d |
| `ui` | the Editor's own state, split by field: level, single, accOne, q, cat, txf, txq, gdAddIn, gdJkPoint go to ui; rowDemo, gdSR, gdToday, gdSim, gdSpeed, gdLaunch go to LAB | `ui` | -- (runtime) | card/terms, core/boot, core/state, editor/accordion, editor/demo, editor/selftest, editor/selftest-texts, editor/shell, editor/texts, guidance/editor, guidance/scenes |
| `ui.gdLaunch` | Preview & Test › Start the app | `LAB.gdLaunch` | Lab-only | guidance/editor |
| `ui.gdSR` | the screen reader stand-in | `LAB.gdSR` | Lab-only | guidance/editor, guidance/scenes |
| `ui.gdSim` | the simulator's scripted visitor | `LAB.gdSim` | Lab-only | guidance/editor, guidance/scenes |
| `ui.gdSpeed` | the simulator's speed | `LAB.gdSpeed` | Lab-only | guidance/editor |
| `ui.gdToday` | Preview & Test › Today is | `LAB.gdToday` | Lab-only | guidance/editor, guidance/scenes |
| `ui.rowDemo` | Show each row's demo | `LAB.rowDemo` | Lab-only | editor/demo |
| `hintDone` | gone since guidance round 3 (a test checks it stays unset) | `ui.hintDone` | -- (runtime) | -- |
| `hintCard` | gone (a test checks it stays unset) | `ui.hintCard` | -- (runtime) | -- |
| `hintBusy` | gone (a test checks it stays unset) | `ui.hintBusy` | -- (runtime) | -- |
| `locSeen` | gone (a test sets it) | `ui.locSeen` | -- (runtime) | -- |
| `nudged` | gone (a test sets it) | `ui.nudged` | -- (runtime) | -- |
| `wallTip` | gone (a test checks it stays unset) | `ui.wallTip` | -- (runtime) | -- |
| `lastMove` | gone (a test checks it stays unset) | `ui.lastMove` | -- (runtime) | -- |
| `openedAt` | gone (a test checks it stays unset) | `ui.openedAt` | -- (runtime) | -- |
| `test` | Preview & Test: the demo card on or off, its marker | `LAB.test` | Lab-only | card/demo-card, core/state, editor/shell |
| `previewBar` | the preview bar shown | `LAB.previewBar` | Lab-only | card/demo-card, phone/preview-bar |
| `ptoolPos` | the preview bar's place | `LAB.ptoolPos` | Lab-only | phone/preview-tools |
| -- | the screens to go back to (back(): the top overlay, the sheet, then the screen before) | `ui.history` | -- (runtime: the screen stack) | screen/router |
| -- | the overlays open, bottom first: settings, contents, menu (the guide's ✦ menu), ask (Ask about the view), try (Things to try) | `ui.overlays` | -- (runtime: the panels over the screen) | screen/router (setOverlay, closeOverlays, back) |
| -- | the preview bar's Camera / No camera: a phone without the camera (Explore is home) | `LAB.noCamera` | Lab-only (in the app: the AR session's availability and the camera permission, cameraOn()) | screen/explore |
| `screen` | gone: the device (W's four) | `ui.device` | -- (runtime: Screen.width / height, Screen.orientation; the Device Simulator in the Editor) | screen/device |
| `focusOn` | Focus on what I edit | `LAB.focusOn` | Lab-only | core/state, editor/shell |
| `focusRef` | the part Focus rings | `LAB.focusRef` | Lab-only | core/state, editor/components, phone/shell |
| `vt` | the blind Voice test: its line, order, ratings | `LAB.vt` | Lab-only | guide/voice, guide/voice-test |
| `demoCard` | the demo card's blocks (Preview & Test) | `LAB.demoCard` | Lab-only | card/demo-card |
| `demoC` | each element's demo content (Blocks & Components) | `LAB.demoC` | Lab-only | editor/demo, editor/texts |
| `_stKey` | the option the self-test tries | `LAB._stKey` | Lab-only | editor/selftest |
| `ownAll` | the words' self-test: every text owned | `LAB.ownAll` | Lab-only | editor/selftest-texts |

## Open guesses
- **The guide's homes.** `D.guide` and `D.agent` (one new guide-config asset and its Ask part), `D.tour` (a tour asset per marker)
  and `D.guidance` (a GuidanceCatalog asset) are new Unity assets; their names are proposals for when the guide is carried into Unity.
- **The Block Library.** `D.blocks` (motions, interactions, effects, element styles, patterns) and `D.card.blockDefs`: the card
  settings' Block Library exists in Unity, but the motions and effects have no home there yet.
- **`D.card`'s smaller parts** (`states`, `motionLevel`, `hints`, `fb`, `lensCfg`) are taken as `card_settings` fields.
- **`D.wall`'s audiences, lenses, tags, camera settings and terms** are taken as `WallConfigData` fields. P1b's profile model
  decides the audiences; `D.wall.aud.seen` (the visitor answered Who's exploring) stays with them until then.
- **`D.tour.guideSteps`** is the open point's tour as the Guide script edits it; a playing tour swaps in a filtered copy (the
  audience, How does this work) and the saved place inserts its recap step into it. P2 separates the tour that plays (`ui`) from
  the script (`D`).
- **`P.dark`** is dark mode as resolved from Settings, the card's mode and the preview bar.
- **`D.guide.life`** is also written when the audience changes (`audiences/audiences.js`: the audience's look sets the mascot's
  liveliness). P1b's profile model replaces that write.
