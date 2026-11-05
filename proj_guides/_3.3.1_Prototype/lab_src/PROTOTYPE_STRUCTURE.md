# Block Components Lab: the prototype's code

This is the code of this prototype, not the Unity project (that is `.clinerules/10-structure.md`). It says where each feature
lives, so a change goes to one place. Update it in the same round as every change (`skills/Feature_Prototyping.md` step 5).

## Rules

- **One home per feature:** its state and defaults, its drawing, its Editor rows, each row's demo and its test.
- **Extension points, not wrappers:** the shared parts call named registries (`src/core/ext.js`); a module joins them. Never
  `var fOld=f; f=function(){...fOld()...}`, never a function declared twice, never a default set in two modules.
- **No silent fallback:** when one source stands in for another, the Editor says so at that moment.
- **The build only joins and embeds:** `build.py` joins `MODULES` and `STYLES` in order and puts the assets in for their
  placeholders (`__HERO__`, `__AZUGLB_*__`…). It never rewrites code.
- **The check:** `build.py` runs `check_structure.js`. It counts wrappers, re-assignments, second declarations and defaults set
  twice; `structure_baseline.json` holds the allowed counts (all 0), and a count may only go down. No module or style may be
  named after a version (`v3.js`, `v8b.css`…).

## Build, prove, publish

1. `python build.py` writes `block-components-lab.html` (not committed) and runs the structure check.
2. `powershell -ExecutionPolicy Bypass -File tests/runtest8.ps1` runs the self-test (`#selftest`, about 40 minutes, alone): every
   Editor option, text and demo field must change the phone or be classed behaviour. The baseline is
   `ok=1180 behaviour=602 bad=0`.
3. `node run_tests.js` runs every `tests/test*.js` (not the `*_live` ones) and lists the failed checks; `tests_baseline.json`
   holds the expected count per file. Each test is described in `README.md`.
4. Copy the page to `../Block Components Lab.html` (the record copy) and publish it to the Lab's link (`README.md`).

## The modules

All the modules share one function scope: `src/core/open.js` opens it and `src/core/boot.js` closes it, so a function declared
in any module can be called from any other. Only what runs at load depends on the order of `MODULES`: a `var` set before it is
read, `S.guideCfg` made before a module adds a key to it, and above all the joins. **The order of `MODULES` is the order in which
each point's joiners run and each element's listeners are added**, so a module that joins a point, or listens on `ph`, `ed` or
`scr`, after another module goes after it in `MODULES`. Within that rule the modules follow the areas (`core`, `editor`,
`phone`, `card`, `audiences`, `guide`, `mascot`, `world`, `agent`); where a later area's join must run first, its module comes
earlier (the guide's tour, for example, joins `renderPhone.after` and `edCard.html` before the merged card options do).

Styles: one `.css` beside the module of its main feature, joined in `STYLES` order (the cascade depends on it). The page:
`src/page/head.html`, `src/page/page.html`. The pictures: `assets/`. Words: `src/core/realstrings.json` (the framework's words,
with their owner part; `strings.py` re-imports them from Unity's `CardStrings.asset`, and the owners set here must be kept).

"Editor sections" are the foldouts a module makes (`fo("s7voice", …)`) or fills (rows added to another module's foldout);
an id ending in `*` stands for a family (`mo:<motion>`).

| Module | What it owns | Editor sections | Points it defines | Points it joins | Tests |
|---|---|---|---|---|---|
| `core/assets.js` | The pictures the build embeds (IMG, as blob URLs) and the shared icons (TILE, PIN, ANIMSVG). |  |  |  |  |
| `core/catalogues.js` | The catalogues: element kinds and their options (KINDS, OPTS, OPTDEF), motion types and paths, the block library (LIB), the fields per kind; lookups by id and use counts. |  | `motionUses.count` |  |  |
| `core/state.js` | The root of the state S: the Editor's selection, the element styles, motions, effects, patterns and interactions. State: `S (the root)`. |  |  |  |  |
| `core/words.js` | Words: T and L (a text in each language), the UI words, the framework's real strings and the Lab's own (S.tx, TXT, TXH), the language list, fmt. State: `S.tx`. |  | `L.answer` |  |  |
| `core/content.js` | The demo point's content: its card's blocks (S.card), its story rows (S.story), the demo texts (DEMO), the wall's points (POIS). State: `S.card`, `S.story`. |  |  |  |  |
| `core/util.js` | Small helpers: esc, byId, setPath and getPath (a dotted path into S). |  |  |  |  |
| `core/ext.js` | The extension points: on, emit, emitLatestFirst (a "before" point), pipe (a value through each joiner), firstAnswer (the latest joiner with an answer wins), answerBelow (the same, from the joiners older than one). |  |  |  |  |
| `editor/components.js` | The Editor's Blocks & Components part: element styles, motions, interactions, effects, patterns and the library, with their thumbnails, demos and fields (edCardV3). | makes `test`, `how1`, `how2`, `mo:*`, `mt:*`, `motion` … (+12) | `motionSummary.answer`, `motionDemo.answer`, `motionBody.answer`, `libThumb.answer` |  |  |
| `editor/shell.js` | The Editor's shell: rows, segments, selects, sliders, foldouts (fo), renderEditor and renderAll, setOpt, and the Editor's own events; the path controls (pseg, psel, pslider) and the row helpers (rowEl, setRow, addRows, dropRow). |  | `contentAt.answer`, `renderEditor.before`, `renderEditor.tab`, `renderEditor.after`, `setOpt.answer` |  |  |
| `editor/json.js` | The JSON views of the Editor (bcJson, markerJson, refreshJson, refreshJsonV4). |  | `refreshJsonV4.answer` |  |  |
| `editor/card-tab.js` | The Detail Card tab (edCard, its point edCard.html), the @path options and the steps bar. | makes `s0`, `s1a`, `s1b`, `s1c`, `s1`, `s2cfg` … (+24); fills `el:divider` | `edCard.html` | `setOpt.answer`, `edCard.html` |  |
| `editor/demo.js` | Each element's demo content (S.demoC) and where a demo field lives; **each Editor row's demo**: a module declares its rows' demos (`rowDemos({key: spec})`), one runner shows them (`showRowDemo`), the label's ▸ and a changed value run it; the self-test fails a row of 6 · Guide or 7 · Audiences without one (NO DEMO); 0 · Preview & Test > Show each row's demo. State: `S.demoC`, `S.ui.rowDemo`. | fills `s0` | `rowDemo.spec` | `renderEl.content`, `renderEl.html`, `contentAt.answer`, `renderEditor.after`, `selfTest.res`, `setOpt.answer`, `edCard.html` | test9d |
| `editor/global-tab.js` | The Global Scene tab: the wall's markers and how a visitor finds them (edGlobal, GSECS). | makes `g:*`, `g:11` |  |  |  |
| `editor/selftest-texts.js` | The self-test of the texts: each text field must reach the phone (selfTestWords). |  | `selfTestWords.before` | `selfTest.res`, `showSelfTest.after` | `runtest8.ps1` |
| `phone/shell.js` | The phone: renderPhone, the screens' top bar, the demo screen, show and the phone's own taps. |  | `topbar.answer`, `demoScreen.html`, `renderPhone.before`, `renderPhone.after` |  |  |
| `card/motion.js` | Motion: entrances, transitions and motion with the scroll; the scroll-driven zoom and pan of a picture. |  |  | `motionSummary.answer`, `motionDemo.answer`, `motionBody.answer`, `motionUses.count`, `renderEl.final` |  |
| `card/interactions.js` | The interactive parts on the phone: scroll boxes, the before/after slider, the deck, hold, the panorama, scrub, tilt, page turns (initBoxes, dragX). |  |  |  |  |
| `card/sections.js` | Detail Card's organisation: the card's and the sections' settings (S.cardCfg, S.secCfg, S.sections, S.ctl), the card screen, the Navigation Bar, the section icons. State: `S.cardCfg`, `S.secCfg`, `S.sections`, `S.ctl`, `S.poiSec`, `S.texts`, `S.glossary`, `S.sel.section`, `S.activeSec`, `S.openSecs`. |  | `cardRuns.runs`, `cardScreen.before`, `cardScreen.html` | `renderPhone.after`, `cardScreen.html` |  |
| `phone/sheet.js` | The sheet: its stops (S.stop, stopTop), the camera behind it, the markers that open it, a tap outside that closes it. State: `S.stop`. |  | `stopTop.answer` | `renderPhone.after` | test8f |
| `phone/feedback.js` | Feedback: haptics and sounds per event (S.fb, feedback). State: `S.fb`. |  |  |  |  |
| `editor/selftest.js` | The self-test: every Editor control is worked and the phone's signature compared (selfTest, modelSig, BEHAVIOUR, showSelfTest); the #probe handle for the headless checks. State: `S._stKey`. |  | `modelSig.before`, `modelSig.sig`, `selfTest.res`, `labelOf.before`, `showSelfTest.after` | `modelSig.before`, `modelSig.sig`, `labelOf.before`, `selfTestWords.before` | `runtest8.ps1` (every Editor row) |
| `editor/words-tab.js` | The Languages & Terms tab (edWords): the words of each part, where each word is used, the term pages. | makes `w1`, `w2`, `w3` | `txRows.answer`, `edWords.html` | `renderEditor.tab`, `refreshJsonV4.answer` |  |
| `editor/focus.js` | The Editor's focus map: opening a foldout shows its part on the phone (FOCUS, focusPhone). |  |  |  |  |
| `card/chrome.js` | The card's chrome: the top band, the sticky action, the close and settings buttons, the page foot. |  | `topBand.after` | `topbar.answer`, `storyScreen.before`, `storyScreen.html`, `galleryScreen.before`, `galleryScreen.html`, `topBand.after` |  |
| `card/gallery.js` | The gallery page and its pictures in the Editor. | makes `pg:gallery`; fills `mpages` | `galleryScreen.before`, `galleryScreen.html` | `edMarker.html` |  |
| `card/contents.js` | The Contents sheet and each block's reading time (contentsSheet, minsOf). State: `S.contentsOpen`. |  | `contentsSheet.html` | `contentsSheet.html` |  |
| `card/effects.js` | Effects: the section edges and the line dividers (edgeSvg, lineSvg, effectArt). |  | `edgeSvg.answer`, `effectArt.answer` | `edgeSvg.answer`, `effectArt.answer` |  |
| `guide/tour.js` | The Guide's tour: its settings and steps (S.guideCfg, S.guideSteps), startTour (from the start, or from a kept step), runStep, next, the captions (renderGuide), the coach and the teaser. State: `S.guideCfg`, `S.guideSteps`. | makes `s7`, `m3` | `startTour.before`, `startTour.after`, `stopTour.before`, `stopTour.after`, `runStep.before`, `runStep.after`, `next.answer`, `stopTap.answer`, `prevTap.answer`, `clearGuideMarks.after`, `renderGuide.after`, `mute.voice` | `cardScreen.html`, `renderPhone.after`, `edCard.html`, `edMarker.html`, `rowDemo.spec` | test8i, test8n, test8o, test9c, test9d |
| `editor/texts.js` | Every text on the part that draws it: the parts' names (COMP), the Texts and Demo panels of each part, the owner bar, the Editor's sections 0 to 5 (Preview & Test, Card Controls). | fills `s6`, `s0`, `s1`, `s1a`, `s1c`, `s1b` … (+11) | `txPanel.html` | `edCard.html` |  |
| `phone/preview-bar.js` | The preview bar under the phone (ptool): phone, wide, tablet, language, dark, reduce motion. State: `S.previewBar`, `S.wallTip`. |  | `ptool.html` | `renderPhone.after` |  |
| `phone/settings.js` | The visitor's settings panel (S.vis, settingsPanel) and the motion level. State: `S.motionLevel`, `S.vis`, `S.settingsOpen`. |  | `settingsPanel.html` |  |  |
| `card/terms.js` | Terms: the glossary words in the text, their popups and pages (S.terms, gloss, termScreen). State: `S.terms`, `S.sel.term`, `S.termOpen`, `S.ui.txf`, `S.ui.txq`. |  | `gloss.html` | `corpus.list`, `searchAnswer.q` |  |
| `card/controls.js` | Card, Sections and Card Controls as in Unity, merged with the Lab's: the accents, the corners, the strips, the section starts, the bar's shape and fill, the term popups. State: `S.cardCfg (more keys)`, `S.secCfg (more keys)`, `S.ctl.bar (more keys)`, `S.ctl (more keys)`. | fills `s1peek`, `s1a`, `s1b`, `s1c`, `s1mini`, `s0card` … (+18) |  | `edCard.html`, `setOpt.answer`, `stopTop.answer`, `cardScreen.before`, `cardScreen.html`, `renderPhone.before`, `renderPhone.after`, `gloss.html`, `storyScreen.before`, `modelSig.before`, `modelSig.sig` |  |
| `phone/preview-tools.js` | The preview bar's grip (drag it anywhere) and its replay. State: `S.ptoolPos`. |  |  |  |  |
| `phone/grabber.js` | The grabber: a tap or a drag moves the sheet between its stops (initGrab, stepStop); the peek row and the hero's tap. |  |  |  | test8f |
| `card/blocks.js` | Every block kind: its options (KOPT), its preview on the card (KPREV, blockPreviewHtml), its defaults (S.blockDefs) and its Editor rows. State: `S.blockDefs`, `S.blockDefs.Practical Info`, `S.blockDefs.Inline`, `S.blockDefs.Before and After`, `S.blockDefs.Gallery`, `S.blockDefs.Knowledge Check`, `S.sel.blockDef`, `S.blockDefs.Sources`, `S.blockDefs.Compare Points`. | fills `bd:*`, `s3b` | `blockPreviewHtml.before`, `blockPreviewHtml.answer`, `blockPreviewHtml.frame`, `blockPreviewHtml.done`, `blockPreviewHtml.html`, `kopt.answer` | `edCard.html`, `blockPreviewHtml.answer`, `blockPreviewHtml.frame`, `cardRuns.runs`, `cardScreen.html`, `modelSig.before`, `modelSig.sig`, `libThumb.answer` |  |
| `card/demo-card.js` | The demo card: one block of every kind (S.demoCard), its screen and the demo tile. State: `S.demoCard`. |  |  | `demoScreen.html` | test8f, test8g, `shots8b.ps1` |
| `card/pages.js` | Each block's full page (kpage) and the picture viewer. State: `S.pageKind`, `S.viewerAt`. |  |  | `storyScreen.html`, `galleryScreen.html`, `renderPhone.before`, `renderPhone.after` |  |
| `card/elements.js` | The elements: renderEl, their options merged with Unity's, the steps list and the carousel. State: `S.styles.title`, `S.styles.lede`. | fills `s3e`, `el:*`, `s4m`, `s5openrow` | `renderEl.answer`, `renderEl.final`, `renderEl.content`, `renderEl.html` | `renderEl.answer`, `renderEl.html`, `edCard.html`, `modelSig.before`, `modelSig.sig` |  |
| `editor/setup-note.js` | The Detail Card tab's note on the order of setting up a card. |  |  | `edCard.html` |  |
| `card/block-looks.js` | A block's own look, frame and reading time on the card. |  |  | `kopt.answer`, `blockPreviewHtml.before`, `blockPreviewHtml.done`, `blockPreviewHtml.html`, `modelSig.before`, `modelSig.sig` |  |
| `editor/marker-tab.js` | The Specific Marker tab (edMarker): the point's card and pages, the starters, the card's warnings, the preview rows. State: `S.starter`, `S.pendingStarter`, `S.previewRows`. | makes `mcard`, `pg:story`, `mpages`, `m0`, `pg:header` | `edMarker.html` | `edMarker.html` |  |
| `card/languages.js` | Languages: the list, the opening language, the texts per language in the Editor (S.langs). State: `S.langs`, `S.langs.opensIn`. | fills `w1` |  | `edWords.html`, `txRows.answer`, `txPanel.html` |  |
| `card/hero.js` | The hero: its content (S.hero), its height by the stop, the bar after it. State: `S.hero`. |  |  | `cardScreen.html`, `renderPhone.after` | `#pshot` |
| `card/story.js` | The story page. |  | `storyScreen.before`, `storyScreen.html` |  |  |
| `card/peek.js` | The peek row's place and the Locate Button at the peek stop. |  |  | `renderPhone.after ×2` | test8f |
| `phone/redraw.js` | A change redraws the phone in place: the scroll stays, no entrance plays again; dark mode in place. |  |  | `renderPhone.before`, `renderPhone.after` | test8f |
| `card/chrome-pinned.js` | The close and settings buttons pinned to the sheet's top; the demo card's close. |  |  | `topBand.after` | test8f |
| `card/locate.js` | The Locate Button: it rides with the bar, its label folds after it is seen (pinLift, locFit); **the rule of a label seen 4 s** (`seenLabel`, used by the Locate Button and the guide's button). State: `S.ctl.locate`, `S.locShown`. |  |  | `renderPhone.after` | test8f, test9d |
| `card/hints.js` | The Hints (S.hints): each one's moment, Off, Reduce Motion, one at a time; How to use the card. State: `S.hints`, `S.hintDone`, `S.hintCard`, `S.hintBusy`, `S.lastMove`, `S.openedAt`. | fills `s0tile`, `s0card`, `s5loc`, `s5hints` |  | `renderPhone.after`, `stopTour.after`, `runStep.after`, `clearGuideMarks.after`, `settingsPanel.html`, `edCard.html` | test8g |
| `card/states.js` | Loading & states: every state in every look (S.states, stateShow). State: `S.states`. | fills `s5hints`, `s5states` |  | `edCard.html` | test8h |
| `guide/ask.js` | Ask the guide: the corpus, the search and the answer (ask). |  | `corpus.list`, `searchAnswer.q`, `ask.answer`, `ask.before`, `ask.after` |  |  |
| `guide/readalong.js` | The read-along light: Comet, Sentence + word, Fill, One word; the trail and the glide (markWord, hlTick). State: `S.guideCfg.hl`, `S.guideCfg.trail`, `S.guideCfg.glide`. | fills `s7cap`, `s7voice` | `markWord.before`, `markWord.after`, `markWord.done`, `wordMs.answer` | `sayDevice.before`, `sayDevice.started`, `sayDevice.ended`, `clearGuideMarks.after`, `stopTour.before`, `renderGuide.after`, `edCard.html` | test8i |
| `guide/focus.js` | Focus: the spotlight, the zoom to a spot named in a picture, Azu pointing; the script's spots. State: `S.guideCfg.cue`, `S.guideCfg.scrim`, `S.guideCfg.zoom`, `S.guideCfg.point`. | fills `s7cap` |  | `runStep.before`, `runStep.after`, `stopTour.before`, `edCard.html`, `edMarker.html` | test8j |
| `mascot/azu2d.js` | Azu in 2D: depth, feet, moods, the mouth by the letters, blinks; play (poke, tickle, a fall, a cookie, a water break) on the guide's Azu or the one beside the guide's button (`azuEl`); Tap on Azu. State: `S.guideCfg.life`, `S.guideCfg.azuTap`, `S.guideCfg.lips`, `S.guideCfg.moods`, `S.guideCfg.moments`, `S.guideCfg.waterAfter`, `S.m`, `S.m.solemn`. | fills `s7mascot`, `m3` | `azMouth.after` | `markWord.after`, `renderGuide.after`, `startTour.before`, `startTour.after`, `next.answer`, `stopTour.before`, `edCard.html`, `edMarker.html` | test8k |
| `audiences/audiences.js` | Audiences: Family, Explorer, Expert; the first-card screen, the looks per audience, the overrides per field and Draft with AI, the guide's length. State: `S.vis.aud`, `S.vis.len`, `S.aud`, `S.ov`. | makes `s8`, `m4`; fills `s7` | `stepFits.ok` | `L.answer`, `startTour.before`, `stopTour.after`, `renderPhone.before`, `renderPhone.after`, `settingsPanel.html`, `ptool.html`, `edCard.html`, `edMarker.html` | test8l |
| `audiences/lenses.js` | Lenses: the See it as chips, the tags per block, story row and guide step. State: `S.lensCfg`, `S.lens`, `S.lenses`, `S.tags`. | makes `m5`; fills `s8look`, `s8lens` |  | `renderPhone.after`, `stepFits.ok`, `edCard.html`, `edMarker.html`, `modelSig.sig` | test8m |
| `guide/person.js` | The guide as a person: the introduction, the invitation ladder, pause and resume, the gesture, the step placed above the panel; the phone takes the window's height. State: `S.guideCfg.intro`, `S.guideCfg.insist`, `S.guideCfg.insistAfter`, `S.guideCfg.onScroll`. | fills `s7coach`, `s7resume`, `s7mascot` | `pauseTour.answer`, `resumeTour.answer`, `pauseNow.voice`, `resumeNow.voice` | `sayDevice.before`, `sayDevice.after`, `runStep.before`, `runStep.after`, `startTour.before`, `pauseTour.answer`, `edCard.html`, `edMarker.html` | test8n, test9c |
| `guide/dock.js` | The Guide Dock: Compact (hidden, the voice goes on), Mini (Pause and Stop beside the talk button), Peek (+ the keyboard), Full; the hide chevron and the swipe; its markup (`dockHtml`); the bubble, the talk button and its states, the conversation, the suggested questions; Azu pointing from beside the block, never over words (`besideWords`). State: `S.guideCfg.panel`, `S.guideCfg.dockStart`, `S.guideCfg.miniCtl`, `S.guideCfg.swipeCompact`, `S.guideCfg.bubbles`, `S.guideCfg.talk`, `S.guideCfg.earcons`, `S.guideCfg.chips`, `S.dockH`, `S.guideAsks`. | fills `s7cap`, `s7dock`, `m3` | `heard.answer` | `renderGuide.after`, `runStep.after`, `ask.before`, `ask.after`, `say.before`, `startTour.before`, `modelSig.sig`, `edCard.html`, `edMarker.html`, `rowDemo.spec` | test8o, test9c, test9d |
| `guide/voice.js` | The voice: one order of sources for every line (sayOrder), the tour's, Azu's own words and the answers on the card and on the wall: a recorded line (recSay), the live voice (say.live), then the device voice (sayDevice); the device voices per language; what speaks now and why (G.sayNow, Speaking now); a line said again from a word (`seekWord`, `sentenceStart`). State: `S.guideCfg.voice_en`, `S.guideCfg.voice_pt`, `S.guideCfg.voiceSet`, `S.vt`. | fills `s7voice` | `say.before`, `say.after`, `say.recorded`, `say.live`, `sayDevice.before`, `sayDevice.ended`, `sayDevice.started`, `sayDevice.wait`, `sayDevice.after`, `stopSpeech.before`, `sayNow.after` | `sayDevice.wait`, `stopSpeech.before ×2`, `say.recorded`, `say.before`, `wordMs.answer`, `pauseNow.voice`, `resumeNow.voice`, `mute.voice`, `sayDevice.started ×2`, `lvPlay.after`, `stopTour.after`, `edCard.html` | test8i, test8n, test8r, test9a, test9c, test9d |
| `guide/controls.js` | The tour's Stop and Previous: Stop with its Undo toast (or Ask first) and the resume point (the step and the sentence, `G.resumeAt`, `resumeTourAt`); Previous mid-sentence says the sentence again, a second tap within 2 s goes back a step. State: `S.guideCfg.stopUndo`. | fills `s7dock` |  | `say.after`, `runStep.after`, `startTour.before`, `stopTap.answer`, `prevTap.answer`, `modelSig.sig`, `edCard.html`, `rowDemo.spec` | test9d |
| `guide/voice-test.js` | The blind Voice test: the voice sets side by side, stars, saved. | fills `s7voice` |  | `edCard.html` | test8r |
| `world/camera-ask.js` | Ask about the view: the points in view as text, the middle one, the model's tools to light points and draw routes, a treasure hunt (camLayer, camSay: its answers through `say`, the one order, so Speaking now names their voice, camAsk). State: `S.camCfg`, `S.camX`, `S.wallPois`. | fills `s7ask`, `s7cam` | `camLayer.after`, `camLight.after`, `camAsk.answer` | `renderPhone.after`, `heard.answer`, `edCard.html`, `modelSig.sig`, `rowDemo.spec` | test8p, test9d |
| `mascot/azu-gl.js` | Azu, the tile of light: the WebGL shader, its forms by state, the peek from behind a block, the walk to the wall. State: `S.guideCfg.azuStyle`, `S.guideCfg.azuWalk`. | fills `s7mascot` |  | `azMouth.after`, `renderGuide.after`, `camLight.after`, `camLayer.after`, `edCard.html` | test8q |
| `world/wall.js` | The wall behind the card (wallLayer) and the test walls: Ribeira and Belém, pan and zoom (WALLS, camGeom, camPaint). State: `S.camCfg.wall`, `S.camCfg.comments`, `S.camCfg.every`, `S.camY`, `S.camZoom`. | fills `s7cam` | `wallLayer.after`, `camPaint.after` | `renderPhone.before`, `renderPhone.after`, `camLayer.after`, `edCard.html`, `modelSig.sig` | test8s, test8v |
| `world/guide-button.js` | The guide's button (✦) and Azu beside it (fabLayer): the ✦ in one place (above the Navigation Bar once the hero's Start is out of sight, the corner on the wall, above the dock during a tour), its menu (Start or Resume the tour, Ask, Ask about the view, Guide settings), its label folding once seen (`seenLabel`), during a tour the progress ring, the pause at Compact and the dock shown or hidden; what a tap on Azu does (`azuTapAct`); Azu's remarks on the wall; the hint that points at the ✦ (`gbHint`); the guide's group in the visitor's Settings. State: `S.guideCfg.gbtn`, `S.guideCfg.gbIcon`, `S.guideCfg.gbSide`, `S.guideCfg.gbFold`. | makes `s7gbtn` | `fabLayer.before` | `startTour.before`, `stopTour.after`, `renderGuide.after`, `settingsPanel.html`, `modelSig.sig`, `edCard.html`, `rowDemo.spec` | test8s, test9d |
| `agent/agent.js` | Ask Azu, the live agent: one model adapter (Claude in the published Lab, Gemini or the Claude API with this computer's key, offline), the prompt, link cards, the Editor's panel. State: `S.agentCfg`. | fills `s7cam`, `s7agent` | `agContext.ctx`, `agPrompt.text`, `agClean.text`, `agentAsk.before`, `agTurnHtml.html` | `ask.answer`, `camAsk.answer`, `renderGuide.after`, `camLayer.after`, `edCard.html`, `renderEditor.before`, `modelSig.sig` | test8t (and test8t_live) |
| `agent/tools.js` | The agent's tool manifest with its policies and the tools' runner (AGTOOLS, agTools, agRunTool). |  |  |  | test8t |
| `agent/console.js` | The agent's test console and its status (agConHtml, agStatus). |  | `agConHtml.html` |  | test8t |
| `agent/scenes.js` | The agent tested in the prototype: the trace under each answer, on the card and on the wall (with the voice that spoke it), every point named, the test scenes with their checks. State: `S.agentCfg.trace`, `S.camCfg.names`. | fills `s7agent`, `s7cam` | `agScene.after`, `agRunScene.answer`, `agScenesHtml.html` | `agTurnHtml.html`, `sayNow.after`, `agentAsk.before`, `camLayer.after`, `fabLayer.before`, `agConHtml.html`, `edCard.html`, `modelSig.sig`, `agContext.ctx`, `agPrompt.text ×2`, `agClean.text` | test8u (and test8u_live), test9c |
| `mascot/azu3d.js` | Azu as a 3D model: the GLB drawn by three.js; the source, a clip, Import, Generate from a picture. | fills `s7mascot` |  | `renderGuide.after`, `azMouth.after`, `startTour.before`, `edCard.html`, `renderEditor.after` | test8w |
| `mascot/models.js` | The 3D models to choose from (AZ3D_MODELS): the explorer (the default), Azu detailed, the mouse, the poet, the soldier. State: `S.guideCfg.azuModel`. | fills `s7mascot` |  | `edCard.html` | test8x |
| `mascot/tile-styles.js` | The tile of light's styles: glaze, pattern, light, corners, and its preview. State: `S.guideCfg.azuGlaze`, `S.guideCfg.azuMotif`, `S.guideCfg.azuLight`, `S.guideCfg.azuShape`. | fills `s7mascot` |  | `edCard.html` | test8y |
| `mascot/size.js` | The mascot's size, and a character per audience. State: `S.guideCfg.azuSize`. | fills `s7mascot`, `s8look` |  | `renderPhone.before`, `edCard.html` | test8z |
| `world/markers.js` | The card's markers on the wall's picture, the Wall button, the markers' test scene; the scenes that act instead of asking (`act`: [ok, note, what it did], or a promise of it). |  |  | `wallLayer.after`, `camPaint.after`, `camLayer.after`, `ptool.html`, `agScene.after`, `agRunScene.answer`, `agScenesHtml.html` | test8v, test9d |
| `guide/guide-scenes.js` | The guide's controls tested in the prototype: seven scenes in Ask Azu's test console (the tour at Mini, Compact and back, Stop and its toast, Previous, the ✦ menu, Azu's tap, the hint), each pressing the controls as a finger does (`gsTouch`) and checking the phone. |  |  |  | test9d |
| `agent/live-voice.js` | The live voice: Azure or ElevenLabs live, for the answers (on the card and on the wall) and the lines the recorded set lacks; the source say.live; the mouth on the voice's own timing (resting while the line is paused, still under Reduce Motion). State: `S.agentCfg.liveVoice`, `S.agentCfg.elevenModel`. | fills `s7agent` | `lvPlay.after` | `markWord.before`, `markWord.done`, `stopSpeech.before`, `say.live`, `pauseNow.voice`, `resumeNow.voice`, `mute.voice`, `edCard.html`, `rowDemo.spec` | test9a, test9c, test9d |
| `mascot/mouth.js` | The real mouth: Azure's facial curves for the live answers and the recorded lines, the loudness for other voices; a paused line, Mouth = Still or Reduce Motion: the mouth rests (`mouthHeld`, `mouthRest`). | fills `s7mascot` |  | `lvPlay.after`, `say.after`, `stopSpeech.before`, `edCard.html` | test9b, test9d |
| `core/boot.js` | Starts the page (the #hash routes) and closes the shared scope. State: `S.view`, `S.tab`. |  |  |  |  |

**The voice.** One order of sources for every line, written once in `sayOrder` (`guide/voice.js`). With Voice = Recorded, then
live, then this device (the default): the tour's lines and Azu's own words take the line's recorded file when the set has it
(`recSay`, `guide/voice.js`, the point `say.recorded`); else the live voice (`lvSay`, `agent/live-voice.js`, `say.live`: Ask Azu >
Voice of the answers, Azure or ElevenLabs, when its key is on this computer); else the device voice (`sayDevice`,
`guide/voice.js`). An answer (`G.answer`) tries the live voice first, then a recorded line, then the device. With Voice = Device
voice the recorded files are skipped (the tour: the device voice; an answer: live, then the device). Each source answers
`{speaks}` when it takes the line or `{not, short}` when it cannot; a source that fails later hands the line on with why
(`sayFrom(src,…,why)`; `agent/live-voice.js`'s `lvDevice`). No silent fallback: `G.sayNow` holds the source and the reasons,
Narration > Speaking now (`guide/voice.js`) shows them as each line starts, and `sayNow.after` (`agent/scenes.js`) puts them in
the trace under an answer. `say.before` (`guide/dock.js`: an answer's words in the captions) and `say.after` (`mascot/mouth.js`:
the recorded line's mouth) run for every source. A live line plays as a recorded one (`G.recAudio`): the read-along, the mouth,
pause, resume and mute act on it the same. Pause / resume / mute act on whatever speaks: `pauseNow` and `resumeTour`
(`guide/person.js`) ask `pauseNow.voice` / `resumeNow.voice` (`guide/voice.js` for an audio, `agent/live-voice.js` for a live
audio still being made: it comes paused), the mute button asks `mute.voice`; else the device voice stops at its word and
speaks on from it (`speakFrom`, the device voice only). Stop (`stopSpeech`) silences every source. An answer reaches the
conversation once, when it is given (`ask.after`, emitted by `guide/ask.js` and `agent/agent.js`, joined by `guide/dock.js`).
The device voice has `sayDevice.before` (`guide/readalong.js` the read-along, `guide/person.js` the resume point), `.wait`
(`guide/voice.js` waits for the browser's voices), `.started` and `.ended` (`guide/readalong.js`; `.started` also
`guide/voice.js`: Speaking now says when this browser has no voice), `.after` (`guide/person.js`: retry a failed voice).
The wall's answers (`camSay`, `world/camera-ask.js`) go through `say` too: an answer, so the live voice first, and Speaking now names it. A line is said again from a word by `seekWord` (`guide/voice.js`: an audio seeks to the word's time, the device voice speaks from it): the Stop toast's Resume and Previous use it.

**Each row's demo.** A row of the Editor shows its own demo on the phone: a module declares the demos of its rows by their key,
`rowDemos({"guideCfg.x": {tour: "g1", dock: "mini", act: function(){…}, ring: ".gd-mini"}})` (it joins `rowDemo.spec`); the one runner,
`showRowDemo` (`editor/demo.js`), puts the phone in the spec's view and state (`view`, `stop`, `past`, `wall`, `tour`, `dock`, `intro`,
`scroll`), does its action (`act`) and rings the part (`ring`, the first of its `|`-joined selectors found). A changed value or a press on
the label (▸) runs it (0 · Preview & Test > Show each row's demo = Off keeps the phone as it is on a change; a test that changes
a row mid-scene sets it Off). The self-test fails a row of 6 · Guide or 7 · Audiences without one (`NO DEMO`); the other sections' rows get
theirs in the next round. `test9d` presses every one.

**The guide's controls.** The dock's heights and what each shows: `guide/dock.js`; Stop, its toast, the resume point and Previous:
`guide/controls.js`; the ✦ and Azu beside it: `world/guide-button.js`; a tap on Azu: `mascot/azu2d.js` asks `azuTapAct`
(`world/guide-button.js`); the label that folds once seen: `seenLabel` (`card/locate.js`); the scenes in Ask Azu's console:
`guide/guide-scenes.js`.

## Where a change goes

- A new Editor row for the guide's panel: `guide/dock.js`, joining `edCard.html` (`addRows(d,"s7dock",…)`), its default
  (`S.guideCfg.x=`), its `FOCUS` entry and its demo (`rowDemos({"guideCfg.x":…})`) in the same module.
- A row's demo: in the row's own module, `rowDemos({key: spec})` (the spec's fields are in `editor/demo.js`); never special code in the runner.
- A new control of the tour: its markup where its height is drawn (`guide/dock.js`'s `dockHtml`, or `world/guide-button.js` for the ✦), its
  behaviour in `guide/controls.js` (through a point of `guide/tour.js`'s taps, as `stopTap.answer`), its scene in `guide/guide-scenes.js`.
- A new voice source: its name in `sayOrder` (`guide/voice.js`, the one order) and a module after `guide/voice.js` joining
  `say.<name>` (it returns `{speaks:who}` when it speaks the line or `{not,short}` saying why it cannot, and hands a line it
  cannot finish to `sayFrom("<name>",…,why)`); an audio of its own goes in `G.recAudio`, so pause, resume and mute act on it.
- A new block kind: `card/blocks.js` (its options in `KOPT`, its preview in `KPREV`, its defaults in `S.blockDefs`); its full
  page in `card/pages.js`; its look and frame on the card in `card/block-looks.js`.
- A new element option: `card/elements.js` (`OPTDEF` and `OPTS`, drawn through `renderEl.html`).
- A new hint: `card/hints.js` (`HINTS`, and its row in the module's `edCard.html` join).
- A new tool for the agent: `agent/tools.js` (`AGTOOLS` and `agTools`); its policy row is in `agent/agent.js`'s panel.
- A new mascot style: `mascot/tile-styles.js` for the tile of light, `mascot/models.js` for a 3D model.
- A new module: put it in `MODULES` after every module whose joiners or listeners must run before its own, and its `.css` at the
  end of `STYLES` unless it must override an earlier rule.
