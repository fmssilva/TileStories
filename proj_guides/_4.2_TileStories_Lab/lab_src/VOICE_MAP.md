# The Voice Map: every voice the Lab speaks, and the one voice service

Version 0. **Audited at commit `7f75c38`** ("Brief the parallel component and voice maps in the worker guide"). B's code and
voicebench were read from `git archive 7f75c38`, not from the working tree, where the P1b.1 builder is changing `src/`. The private
Edge sets were read from `voicebench/out-private/voicesets.js` on this computer (git ignores them).

This is the same kind of map as `STATE_MAP.md` and `THEME_MAP.md`, for the voice. Merge P2 (`_4.2_Lab_Merge_Plan.md` §3, §4, §5.2-5.5)
builds from it. "(guess)" marks a judgement that was not measured. File references are `src/<path>:<line>` at `7f75c38`.

## 0 · Summary
- **Every line goes through one function, `say()`** (`guide/voice.js:9`), which asks the sources in `sayOrder()` (`:6`). What is not
  one service yet is everything around it: pause, resume, seek, speed and Back to the game each keep their own idea of the line,
  the live and device voices pick their own voices, and the settings are spread over `D.guide`, `D.agent`, `P.guide`, `P.vis`,
  `P.guideMute` and the keys file.
- **The plan's §4 causes, at this commit:**
  1. **Coverage:** confirmed, and the main cause. Ryan · Duarte covers 513 of 513 lines. Sonia · Raquel, ElevenLabs and both Edge
     sets cover 52 of 513. Gemini covers 0: its 8 MP3s are not in the manifest.
  2. **Exact text:** confirmed. A line is found by `FNV-1a(lang|text)` (`guide/voice.js:86`). Besides edited lines, **a game's line
     resumed after a pause is said as a substring** (`guidance/present.js:488`), so it is never recorded.
  3. **Settings writes the author's setting:** **fixed in P1a.1.** Settings now writes `P.guide` (`world/guide-button.js:118`), and
     Forget clears it (`phone/settings.js:4`). Three loose ends remain:
     - the visitor's speed lives in two places (`P.vis.speed` from the dock's chip, `P.guide.speed` from Settings);
     - the music lives in `P.vis`;
     - mute (`P.guideMute`) survives Forget.
  4. **Resume and the rest call the device voice:** **mostly fixed.** Resume, speed and seek now act on an audio when one plays
     (`guide/person.js:24-28`, `guide/controls.js:55`, `guide/voice.js:113`). One path is still open:
     - `G.sayArgs`, the line kept for "speak again from word k", is written only on the device path (`guide/person.js:12`);
     - so **Back to the game restores a recorded line as the device voice** (`guide/game-ask.js:120`, `:137`), using that stale
       copy (the runtime effect is a guess).
  5. **The fallback voices ignore the chosen voice:** confirmed.
     - The device voice reads `D.guide.voice_en` / `voice_pt` (`guide/voice.js:53`).
     - ElevenLabs live reads its voice ids from the keys file (`:43`).
     - Azure live follows the set only when the set is Azure; otherwise it uses Ryan / Duarte, written into the code (`:85`).
  6. **Answers are never recorded,** and their voice is a separate setting: confirmed. `D.agent.liveVoice` and `D.agent.voiceOut`
     (`agent/live-voice.js:2`, `agent/agent.js:3`) sit apart from `D.guide.voiceSet`.
- **Rendering Sonia / Raquel in full:** 461 lines, **30,912 characters**, about **6.2 %** of Azure F0's 0.5 M a month. A whole set from
  nothing is 35,021 characters (7.0 %).
- **P2's size (guess):** about 28 files touched, 6 of them new, in two rounds (§5).

## 1 · Every speech path
**How a line picks its voice today** (`guide/voice.js:6-16`):
- **A line:** `sayOrder` gives `["recorded","live"]` when Sources is Recorded, `[]` when it is Device voice. Each source answers
  `{speaks}` or `{not, short}` (the why). Whatever is left goes to the device voice, or to Silent when `D.guide.noRec` is
  `"silent"`.
- **An answer** (`text === G.answer`): `["live","recorded"]`, or `["live"]` with Device voice; never Silent.
- **What speaks and why** goes to `G.sayNow`. The Editor shows it in Speaking now (`:122-127`).

Columns: **Kind** (L = a scripted line, A = an answer, C = a composed line), **Its voice today**, **Causes** (§4's numbers).

| Path | Where | Kind | Its voice today | Causes |
|---|---|---|---|---|
| A tour step's line | `guide/tour.js:91` (`stepText`) | L | `say` | 1, 2 |
| The first introduction | `guide/person.js:33` (a step spliced in), said by the step | L | `say` | 1, 2 (a renamed mascot changes every introduction line) |
| Moving on, after the visitor scrolls | `guide/person.js:79` | L | `say` | 1 |
| Invite to try: the nudges and Moving on | `guide/person.js:91` | L | `say` | 1 |
| After the gesture: the step's After words, "There it is" | `guide/person.js:100-101` | L | `say` | 1, 2 |
| A flipped card's back, read in place | `guide/person.js:100` (`back.textContent`) | C (the card's own words) | `say` | 1, 2: not in the script (guess: no demo step without After words) |
| Tell me more: the deep part | `guide/tell-more.js:48` | L | `say` | 1, 2: the script holds only the first deep part of a tour (guess: a later one, de-duplicated, differs) |
| A tip spoken by the guidance (remarks, seasons, jokes, tour invitations) | `guidance/present.js:178` (`gdVoice`), from `guidance/service.js:139` | L | `say` | 1, 2: an item the author sets to Spoken (an App tip, a Fun item) is not in the script |
| A game's beat line (quiz, Guess the year, Don't say it!, Find it, the breather, the cookie) | `guidance/present.js:484-488` (`gdBeatSay` → `gdBeatVoice`), about 30 callers | L / C | `say` | 1, 2 |
| A beat resumed after a pause (Ask during a game, a frozen beat) | `guidance/present.js:488`, `:506` | C (a substring from word k) | `say` of the rest | **2: never recorded**, so it falls to the live voice, then the device |
| Don't say it!'s slip, played back | `guidance/present.js:725-732` | L | a slice of the recorded file (`recFile`), else `gdBeatSay` of the words | 1, 2 |
| Guess the year's distance, Don't say it!'s counts, the quiz's callback | `guidance/present.js:583` (`tour_guess_b2`), `:612`, `:716-717` | C | `say` | live by nature: falls to the live voice, then the device |
| The tour's end: the mascot's question | `guide/tour-end.js:72` (`teAzuAsk`) | L | `say` | 1 |
| What next? › Explore nearby: the invitation "A tour of {point}?" | `guide/tour-end.js:244` (`teInvite`) | L | `say` | 1 |
| Ask during a game: Back to the game | `guide/game-ask.js:133` (`gaBack`) | L | `say` | 1 |
| Ask during a game: the game's own line goes on | `guide/game-ask.js:137` (`gaGo` → `speakFrom`) | L | **the device voice** (`speakFrom`), from a stale `G.sayArgs` | **4** |
| Ask during a game: the hint ladder | `guide/game-ask.js:192` (`gaHint`) | A / C (from the card's words) | `say` with `G.answer` (live first) | 6 |
| Ask during a game: Tell me | `guide/game-ask.js:204`, `:206` | L (quiz) / C (a card's option) | `gdBeatSay` / `say` | 1, 2 |
| Ask (offline search) | `guide/ask.js:25` | A (a block's first sentence; `guide_not_found`) | `say`, live first | 6 |
| Ask the mascot (the live agent), its notes | `agent/agent.js:118-119` | A | `say` when `D.agent.voiceOut`, live first | 6 |
| Ask about the view: answers, the offline lines, `cam_none` | `world/camera-ask.js:32` (`camSay`), `:45-51`, `agent/agent.js:121-125` | A / C (`camOffline` names the points in view) | `say`, live first | 6 |
| Ask the mascot, offline: "how do I…" | `guidance/service.js:481` (`gdHowAsk` → `gdTryHowLine`) | A, though its words are fixed and recordable | `say`, live first | 6 (could be recorded) |
| Resume after a pause | `guide/person.js:25-28` | -- | an audio plays on (`resumeNow.voice`); the device voice speaks again from its word | fixed |
| The speed chip | `guide/controls.js:55` | -- | an audio's `playbackRate`; the device voice speaks again | fixed (a live line still being made keeps the old speed: guess) |
| Previous, 10 s back / on, Continue a stopped tour | `guide/controls.js:12`, `:35`, `:73` → `seekWord` (`guide/voice.js:113`) | -- | an audio seeks; the device voice speaks again from the word | fixed |
| The watchdog: a device voice gone quiet | `guide/person.js:30` | -- | the device voice only (it skips audio) | as meant |
| The Editor's demo of A line with no recording | `guide/mascot-profile.js:60` | L, made to be unrecorded | `say` | as meant (Editor only) |
| The Voice test (developer-only) | `guide/voice-test.js:12` | L | the set's file, directly | `LAB` only |
| The music bed (not a voice) | `guide/tour.js:55-62` | -- | its own audio, lowered while a voice speaks (`musicDuck`) | -- |

**The fallbacks today, in order:**
1. **The recorded file** of the chosen set (`recSay`, `guide/voice.js:88`). A file that will not play hands the line on (`:94`).
2. **The live voice** (`lvSay`, `agent/live-voice.js:52`). It needs a key on this computer. On an error it hands the line on (`:50`).
3. **The device voice** (`sayDevice`, `guide/voice.js:18`), or **Silent** when `D.guide.noRec` is Silent (`:15`).
4. **The words by time:** with no device voice for the language, the words go on alone, captions only (`:24`).

Each step is named in **Speaking now**. The published Lab has no key, so there step 2 always fails and every unrecorded line is the
device voice.

## 2 · The one service
**Owner module:** a new Services layer (`_4.2_Lab_Merge_Plan.md` §2), `src/services/` (a new folder; the language and motion services
of P2 join it).

| File | What it holds | Made from |
|---|---|---|
| `services/voice.js` | the resolver, `say`, the sources, the current line, the controls | `guide/voice.js`; the voice parts of `guide/person.js` (`speakFrom`, the watchdog) |
| `services/voice-live.js` | the Azure and ElevenLabs providers | `agent/live-voice.js`'s provider half (the mouth stays in `mascot/mouth.js`) |
| `services/voice-coverage.js` | the line registry, the coverage, the offered sets | `guide/voice-script.js` |
| `editor/guide-voice.js` | the Guide tab's Voice rows | rows now spread over 6 modules (the plan's ratchet moves Editor code out of domain modules) |

**The authored part, `D.guide.voice`** (the plan's shape; the old keys become fields, with no alias):

```
D.guide.voice = {
  set:    "azure-ryan-duarte",          // a VoiceSet id; "device" = no recordings   (was D.guide.voiceSet + D.guide.voice "recorded"/"device")
  live:   { provider: "set", ids: {}, model: "eleven_flash_v2_5" },
                                         // "set": the set's provider and voices (the default); "azure" | "eleven" | "device"
                                         // ids: only when the provider is not the set's   (was D.agent.liveVoice, elevenModel, the keys file's ids)
  device: { en: "", pt: "" },           // the device voice per language, "" = best for the accent   (was D.guide.voice_en / voice_pt)
  noRec:  "voice",                      // "voice": live, then device | "silent"   (was D.guide.noRec "device" | "silent")
  speed:  1, speeds: "wide",            // the default and the AudioSpeedRule preset (guess: the preset)   (was D.guide.speed)
  answers:{ spoken: true }              // was D.agent.voiceOut
}
```

- **What visitors may change:** `D.app.settings.guide = { voice, speed, captions, music, mascot }`, true / false each. This is W's
  `D.app.settings` shape (`_4.1.2_Prototype/lab_src/js/24_tab_app.js:493`). `D.guide.musicVis` folds into it.
- **The visitor's part:** `P.guide = { voice, speed, captions, music, musicVol, mute }`. It absorbs `P.vis.speed`, `P.vis.music`,
  `P.vis.musicVol` and `P.guideMute`, so Forget clears them all. `guideSetting(k)` (`guide/tour.js:5`) keeps reading `P` over `D`,
  but only for what `D.app.settings.guide` offers.

**The resolver `guideVoiceFor(ctx)`.** It is pure (no DOM, no audio), so a Node test runs it without a browser, like P1b.1's token
test.
- **ctx:** `{ lang, kind: "line" | "answer", env: { keys: {azure, eleven}, published, deviceVoices } }`.
- **It returns:**
  ```
  { order: [ {src:"recorded", set, voice}, {src:"live", provider, voice, model, ok, why},
             {src:"device", voice} | {src:"silent"} ],
    speed, label: "Ryan · Duarte (Azure)" }
  ```
- **Its rules:**
  1. A line: recorded, then live, then device or silent (`noRec`). An answer: live, then recorded, then device; never silent.
  2. A fixed-word answer (`gdTryHowLine`, `cam_none`, `guide_not_found`) is said as a line (`kind:"line"`), so it can be recorded.
     Only the model's and the search's answers are answers.
  3. **The live voice derives from the set:**
     - an Azure set → Azure with the set's voices;
     - an ElevenLabs set → ElevenLabs with the set's voice ids;
     - an Edge set → Azure with the same voice names (Edge's voices are Azure's);
     - a Gemini set → the plain Azure default (guess).
     No voice id is written into the code or kept in the keys file; the keys file keeps only the keys.
  4. **The device voice** is `device[lang]`, else the best one for the accent (`voicesFor`, `guide/voice.js:49`).
  5. **The visitor's voice** (`P.guide.voice`, when offered) replaces `set`. `"device"` makes every speaker the device voice,
     answers included: one setting changes every speaker (the P2 done-when).
  6. **The speed:** the visitor's, else the author's. One list of options (`AudioSpeedRule`), so the dock's chip and Settings offer
     the same speeds. Today the chip offers 0.5-2 and Settings 0.85 / 1 / 1.2.
  7. **The label:** the voice actually speaking. A source standing in says why: "Device voice: no live voice in the published Lab".

**The service's API** (Lab names; the Unity contract has the same shape as `ICardAudio`):
- **Saying:**
  - `say(text, {kind, owner, onWord, onEnd})`: the one entry. Every call site keeps `say` (about 19 direct ones, plus
    `gdBeatSay`, `camSay` and `gdVoice`), and an answer passes `kind:"answer"` in place of the `G.answer` comparison.
  - The service keeps **the current line for every source** (text, kind, callbacks, source, word). This replaces `G.sayArgs`, which
    today only the device path writes.
- **The controls:** `voicePause()`, `voiceResume()`, `voiceSeek(k)`, `voiceSpeed(v)`, `voiceMute(on)`, `voiceStop()`. Each acts on
  the current line whatever speaks it.
  - Resume, Previous, the 10 s seeks, Continue, the speed chip, the watchdog and Back to the game all call these.
  - A beat resumed after a pause seeks its clip. It no longer says a substring.
- **Reading:**
  - `voiceNow()`: `{src, by, why, label}`, for Speaking now and the checks.
  - `voiceCoverage(setId)`: `{have, all, miss:[{where, owner, lang, text}], composed:[{where, owner, how}]}`.
  - `voicesOffered()`: the sets at 100 % in every language of the wall, plus `"device"`.
- **Events:**
  - it listens to `configChanged` (`D.guide.voice.*`) and `profileChanged` (`guide.*`) and drops its memo;
  - it emits `voiceNow` when a line starts or a source hands it on (`sayNow.after` today);
  - the source points `say.recorded`, `say.live`, `say.device`, `say.before` / `say.after` and `stopSpeech.before` stay as they are.
- **The line registry:**
  - Each owner module adds its lines through `pipe("voice.lines", …)`, each with `{where, owner, lang, text, how}`. The owners: tour,
    person, tell-more, saved-place, tour-end, game-ask, and guidance's catalogue and present.
  - So a missing line names its owner, and the coverage does not reach into the guide's code (today `guideScript`,
    `guide/voice-script.js:14`, calls about 25 functions of other modules).
  - `voicebench/export_script.js` reads the same registry.

**What the Editor shows at each fallback:**
- **Speaking now:** the source and the why, as now.
- **Coverage:** "Sonia · Raquel (Azure): 52 of 513". Below 100 %, a list of the missing lines with their owner and language, and
  **Render the missing lines**, which runs `render.py --only <keys>`. `render.py` already skips what is rendered.
- **The live provider's row:** whether its key is on this page.
- **Offered to visitors:** each set marked "offered" or "not offered: 52 of 513".

**The Unity homes** (proposals, carried into Unity only when the developer says so):

| Lab part | Unity home |
|---|---|
| `D.guide.voice` | `GuideConfig` (the new guide-config asset, STATE_MAP), a `[Serializable] GuideVoiceSettings voice` field under `Framework/Runtime/Guide/` (guess: the folder; `10-structure` has only `UI/World/GuideCharacter/`, an empty scaffold) |
| a voice set | a `VoiceSet` asset: clips, word times and mouth curves, keyed by `FNV-1a(lang\|text)`, imported from voicebench |
| `guideVoiceFor` | `GuideVoiceResolver`, plain C#, with EditMode tests |
| the playback | `GuideVoiceService`: the existing `IAudioOutput` (as `CardAudioService` uses it) and the platform's text to speech |
| the speeds | the existing `AudioSpeedRule` (Narration 0.75-1.5, Wide 0.5-2, Off) |
| `P.guide` | the visitor's profile in PlayerPrefs, through the `ICardStateStore` pattern (`DeviceStateStore.App`) (guess) |
| the line registry | the POI Editor's Export voice script, `GuideScript.Collect` |

**Names to avoid or rename:**
- `SelectFilterSearchSettings.VoiceSettings` already exists (the search's voice input), so the guide's part is `GuideVoiceSettings`.
- B's comments name "MascotProfile.voiceSet / .noRecording" (`guide/voice-test.js:26`, `guide/mascot-profile.js:47`,
  `guide/voice-script.js:10`). P2 renames them to `GuideConfig.voice`.

## 3 · Coverage
**The script:** `voicebench/script.json`, 513 lines, 256 EN and 257 PT, 35,021 characters (EN 17,555, PT 17,466). It was made by
`guideScript()` for the Ribeira wall's four points with a card: both languages, the written tour for every audience, the tours from
the cards for one audience.
- It has not been made again since the round-21 commit (`296e373`).
- test9x asserts that Ryan · Duarte covers the live script 100 %, and it passed (27 / 0) in the last full run, `tests_baseline.json`.
  So the script on disk still matches the code (guess: P1a's moves were mechanical, but this was not measured at `7f75c38`).

| Voice set | Provider, voices | Files | Lines covered | Missing EN / PT | Old lines (text since changed) |
|---|---|---|---|---|---|
| `azure-ryan-duarte` (the default) | Azure, en-GB-Ryan / pt-PT-Duarte | 519 | **513 of 513** | 0 / 0 | 6 |
| `azure-sonia-raquel` | Azure, en-GB-Sonia / pt-PT-Raquel | 58 | **52 of 513** | 230 / 231 | 6 |
| `eleven-hpp4j3…-ejarts…` | ElevenLabs, Bella (en-US) / Luis (pt-PT) | 58 | **52 of 513** | 230 / 231 | 6 |
| `edge-ryan-duarte` (private) | Edge, Ryan / Duarte | 58 | **52 of 513** | 230 / 231 | 6 |
| `edge-sonia-raquel` (private) | Edge, Sonia / Raquel | 59 | **52 of 513** | 230 / 231 | 7 |
| `gemini-kore-kore` | Gemini, Kore | 8 MP3s, not in the manifest | **0** (not loaded) | 256 / 257 | -- |

**The missing lines** are the same 461 for every partial set. Each set holds the 52 lines kept from 2026-11-03.
- **By group (counted for Sonia · Raquel):**
  - the recaps (Continue at step N): 74;
  - the tours from the cards (their steps, after-words and Tell me more): 74;
  - the games' fixed lines (the quiz's verdicts and scores, Find it's hints, the breather, Don't say it!'s lines): 71;
  - Continue the tour of {point}? (step N of M): 66;
  - the quiz's questions, wrong answers, Not sure and Tell me: 48;
  - the remarks on the wall's points: 44;
  - the introductions, the tour's end, the invitations and Back to the game: 40;
  - Find it on the wall's ask and its edges: 12;
  - Guess the year's verdicts and Don't say it!'s announcement: 8;
  - the rest (the seasons, the jokes, the water break, the cookie, Changed, two written steps): 24.
- **By owner:**
  - `guide/autotour` and `guide/tour` (the tours);
  - `guide/saved-place` (the recaps, Continue);
  - `guide/tell-more`;
  - `guide/person` (the introduction, the nudges);
  - `guide/tour-end`;
  - `guide/game-ask` (Back to the game, Tell me);
  - `guidance/catalog` with `guidance/present` (the remarks, the invitations, the games).
- The full list per set, with each line's where, language and text, comes from `node coverage.js` in the scratch folder. It is the
  same list that P2's Coverage row shows.

**Composed lines**, which the script cannot hold whole. How each should speak:

| Line | Where | How it should speak |
|---|---|---|
| Guess the year: how far the guess was ("Close! Only {0} years off.", within 20 % of the slider's range) | `guidance/present.js:583` (`tour_guess_b2`) | **Recorded,** reshaped: the distance rounded ("Only about 10 / 20 / 50 years off"), a handful of lines per language (guess: a natural reshape). Otherwise live, labelled |
| Don't say it!'s result ("{c} of {n}", the false taps) | `guidance/present.js:716-717`, joined into one line | **Recorded parts:** split into its sentences, each said in turn. The counts are bounded (n ≤ the slips of a tour), so the script lists each variant |
| The quiz's callback to Don't say it! (appended) | `guidance/present.js:612` | **Recorded parts:** a second `say` after the verdict, its counts enumerated |
| The quiz's score "{r} of {n}" | `guide/voice-script.js:51` assumes n = 3 | **Recorded:** the script lists every n the quiz can have (guess: always 3) |
| A beat resumed from word k | `guidance/present.js:488` | **Recorded:** seek the clip (the service), never a substring |
| Ask during a game, Tell me ("The answer: {0}. {1}") on Guess the year and on a card's knowledge check | `guide/game-ask.js:204`, `:206` | **Recorded:** the year and the card's options are fixed per step and per card, so the registry lists them (the script has the quiz's only) |
| The hint ladder | `guide/game-ask.js:189-192` (`gaStepWords`) | **Recorded** where it uses fixed hint words (the game's hints are authored, `ui.gaBusy`); live otherwise (guess) |
| A flipped card's back | `guide/person.js:100` | **Recorded:** the card's own words go into the registry (owner: the block) |
| Ask about the view, offline ("In view: {names}") | `world/camera-ask.js:35` (`camOffline`) | **Live,** labelled (the names in view vary) |
| The answers (search, agent, the hints from the card's text) | §1 | **Live,** labelled. With no key (the published Lab, the phone builds before the token Worker), the device voice, labelled |
| An item set to Spoken by the author | `guidance/present.js:178` | **Recorded:** the registry lists every item that can speak, from its settings |
| The intro lines after a rename of the mascot | `guide/person.js` (`introLine`) | **Recorded:** a name change is an edit, so Coverage lists the lines and offers Render |

**Characters.**
- **Sonia / Raquel in full:** the 461 missing lines are **30,912 characters** (EN 15,485, PT 15,427), so **6.2 %** of Azure F0's
  500,000 a month. A full set from nothing is 35,021 characters, **7.0 %**.
  - Azure bills the text's characters (guess: the SSML's viseme request and tags are not billed).
  - Each later word edit renders only the changed lines.
- **The ElevenLabs set in full:** the same 30,912 characters, so about 31 k credits with Multilingual v2, or half that with Flash
  (guess: the Creator plan's 100 k credits a month, so about a third of it).
- **Live answers share F0** (guess): 1,000 answers of about 300 characters is 300 k, 60 % of the month. P2 counts each live
  character and shows the month's total next to the live provider's row.

## 4 · Settings
**Every control that touches the voice today**, with its root:

| Control | Where | Root | Module |
|---|---|---|---|
| The voice (the recorded set) | Guide tab › Mascot › Voice | `D.guide.voiceSet` | `guide/voice-test.js:25` |
| Sources (Recorded, then live, then this device / Device voice) | Guide tab › Mascot › Voice | `D.guide.voice` | `guide/tour.js:150` |
| Speed (0.5-2) | Guide tab › Mascot › Voice | `D.guide.speed` | `guide/tour.js:151` |
| A line with no recording (The device voice / Silent) | Guide tab › Mascot › Voice | `D.guide.noRec` | `guide/mascot-profile.js:46` |
| Voice in this browser (the device voice per language) | Guide tab › Mascot › Voice | `D.guide.voice_en` / `voice_pt` | `guide/voice.js:134` |
| Voice of the answers (Device / Azure live / ElevenLabs live) | Guide tab › Mascot › Voice and Ask › What the visitor sees | `D.agent.liveVoice` | `agent/live-voice.js:74` |
| ElevenLabs model | the same two places | `D.agent.elevenModel` | `agent/live-voice.js:76` |
| Spoken answer (On / Off) | Ask › What the visitor sees | `D.agent.voiceOut` | `agent/agent.js:158` |
| Coverage, Speaking now (read only) | Guide tab › Mascot › Voice | -- | `guide/voice-script.js:71`, `guide/voice.js:141` |
| Music bed, its volume, under the voice, the track | Guide tab › 7 · Guide | `D.guide.music`, `musicVol`, `musicDuck`, `musicTrack` | `guide/tour.js:151-157` |
| Visitor can choose the music | Guide tab › 7 · Guide | `D.guide.musicVis` | `guide/tour.js:158` |
| Captions | Guide tab › 7 · Guide | `D.guide.captions` | `guide/tour.js:159` |
| Mascot (On / Off) | Guide tab › 7 · Guide | `D.guide.mascot` | `guide/tour.js:161` |
| Mouth (Follows the words / Still) | Guide tab › Mascot › Manner | `D.guide.lips` | `mascot/azu2d.js:111` |
| A tip is Spoken (As the kind / A bubble only / once the guide is heard) | Guide tab › The mascot speaks up, per item and per kind | `D.guidance.items.<id>.spoken`, `D.guidance.kinds` | `guidance/editor.js:160` |
| The Voice test (developer-only) | Guide tab › The Lab's wall | `LAB.vt` | `guide/voice-test.js` |
| The keys (Azure, ElevenLabs and its voice ids) | `keys.local.js` (from `.secrets/keys.env`) | -- (this computer) | `agent/live-voice.js:6`, `guide/voice.js:43` |
| **Visitor:** Settings › The guide › Voice (Recorded / The phone's voice) | the phone | `P.guide.voice` | `world/guide-button.js:115` |
| **Visitor:** Settings › The guide › Speed (Slower 0.85 / Normal / Faster 1.2) | the phone | `P.guide.speed` (and it clears `P.vis.speed`) | `world/guide-button.js:116` |
| **Visitor:** Settings › The guide › Captions | the phone | `P.guide.captions` | `world/guide-button.js:116` |
| **Visitor:** Settings › The guide › Music and its volume (when offered) | the phone | `P.vis.music`, `P.vis.musicVol` | `world/guide-button.js:120-122` |
| **Visitor:** the dock's speed chip (0.5×-2×) | the guide panel | `P.vis.speed` | `guide/controls.js:55` |
| **Visitor:** the dock's sound (mute) | the guide panel | `P.guideMute` | `guide/tour.js:137` |
| **Visitor:** the dock's CC / Aa | the guide panel | `P.guide.captions` | `guide/tour.js:137`, `guide/dock.js:106` |
| **Visitor:** Forget | Preview & Test | clears `P.vis`, `P.kqDone`, `P.guide`; **not** `P.guideMute` | `phone/settings.js:4` |
| **Visitor stand-ins:** Tips, Tips stay (not the voice, the same Settings group) | Guide tab › Preview & Test | `P.gdVisitor.prefs` | `guidance/editor.js:278-279` |

**Mapped onto plan §5.4:**
- **Guide tab › Voice and guide (`D`, the guide-config asset).** It holds:
  - provider and voice set, with coverage (`set`; the provider is the set's);
  - the live provider (`live.provider`, the same by default) and its model;
  - speed and its preset;
  - a line with no recording (`noRec`);
  - the device voice per language (developer-only: the phone picks its own in the app);
  - Spoken answer (`answers.spoken`);
  - captions, music (bed, track, volume, ducking) and Mascot On / Off (they keep their `D.guide` keys);
  - Coverage, Offered to visitors and Speaking now (read only).

  The Mouth stays in Manner, and the Spoken switch per tip stays with the guidance.
- **App tab › Settings (`D.app.settings.guide`).** What the visitor may change: voice (only sets at 100 %), speed, captions, music,
  the mascot. It replaces `D.guide.musicVis`.
  - **B has no App tab** (its tabs: Global, Marker, Card, Guide, Words). W's App tab › 7 · Settings and You arrives in P4. See
    decision 2.
- **The visitor's Settings › The guide (`P.guide`).** Exactly the offered choices: voice (the offered sets by name, plus "The phone's
  voice"), speed, captions, music, the mascot.
  - The dock's speed chip, mute and CC write the same `P.guide` fields.
  - Forget clears all of them.
  - B's Settings › The guide becomes this group, and it joins W's five-area Settings in P4 (plan §5.4).

## 5 · The order of work
Plan §3 gives P2 one or two rounds, with the language and motion services in the same phase (not mapped here). The voice part, in
order (sizes are guesses):

1. **P2.1 · The voice service.** About 14 files, 3 of them new.
   - `D.guide.voice`, `P.guide` and `D.app.settings.guide` in `core/state.js` and STATE_MAP.
   - `services/voice.js` with the resolver, the current line for every source and the controls; `services/voice-live.js`.
   - The callers move onto it:
     - Resume, the watchdog and Moving on (`guide/person.js`);
     - the speed chip and the seeks (`guide/controls.js`);
     - Back to the game (`guide/game-ask.js`);
     - the beats' resume, which seeks the clip (`guidance/present.js`);
     - answers marked by `kind` (`agent/agent.js`, `world/camera-ask.js`, `guide/ask.js`, `guidance/service.js`).
   - `guide/voice.js` and `agent/live-voice.js` are emptied into the service (the mouth stays in `mascot/mouth.js`).
   - **Tests:**
     - a Node test of `guideVoiceFor` (every rule of §2, mutated);
     - a Playwright test by real clicks: the visitor's voice set in Settings changes every speaker (a step, a tip, a beat, an answer,
       the tour's end, Back to the game, all read in `voiceNow()`); Forget brings the author's back; resume, seek, speed and Back to
       the game keep the source; the published-page case (no keys) labels the device voice.
2. **P2.2 · Coverage and the composed lines.** About 9 files, 1 of them new.
   - `services/voice-coverage.js` with the line registry; each owner module adds its lines.
   - The composed lines reshaped per §3: Don't say it!'s result and the callback split, the bounded counts enumerated, a flip's back
     and the Spoken items in the registry.
   - `voicebench/export_script.js` reads the registry.
   - **Tests:**
     - a voice offered to visitors below 100 % fails;
     - an edited word shows in Coverage with its owner;
     - every composed line is listed with how it speaks.
   - **Then, the developer's call (credits):** render Sonia / Raquel's 461 lines (30,912 characters, 6.2 % of F0) with
     `render.py --only`, so a second voice can be offered.
3. **P2.3 · The Editor and Settings.** About 8 files, 2 of them new.
   - `editor/guide-voice.js`: the Guide tab › Voice and guide, the rows moved out of six domain modules (`guide/tour.js`,
     `guide/voice.js`, `guide/voice-test.js`, `guide/mascot-profile.js`, `agent/live-voice.js`, `guide/voice-script.js`), each with
     its ▸ demo and undo.
   - The App tab's Settings section (decision 2).
   - The visitor's Settings › The guide (`world/guide-button.js`) and Forget (`phone/settings.js`).
   - `editor/guide-tab.js`'s plan.
   - **Tests:** an Editor change of the set or the offer reaches the phone, and Undo returns it.

**Size in all (guess):** about 28 files touched, 6 of them new, plus the docs (STATE_MAP, PROTOTYPE_STRUCTURE, README, this map). That
is two rounds: P2.1 + P2.2, then P2.3 with the language and motion services.

**What it needs first:**
- P1b.2: the profile model and the Editor kit, which P2.3's rows and `P` layer use.
- No key may go into a published page; the token Worker comes after the merge (plan §3).

## 6 · Open decisions
1. **"The phone's voice" chosen by the visitor: does it also speak the answers?** Recommended: **yes.** One setting changes every
   speaker, as P2's done-when asks. Today the answers still try the live voice first.
2. **Where "App tab › Settings" lives before P4.** Recommended: **P2.3 starts the App tab with only its Settings section,** in W's
   shape (`D.app.settings`, here `.guide`). P4 ports W's other App sections into the same tab, so nothing moves twice. The other way,
   a foldout in the Guide tab moved in P4, means a second move.
3. **Sources (Recorded / Device voice) as its own switch.** Recommended: **fold it into the voice choice.** `set: "device"` is the
   device voice for everything. That is one control less, and it matches the visitor's list ("Ryan · Duarte", "Sonia · Raquel", "The
   phone's voice").
4. **A line with no recording.** Recommended: **two options,** "The voice" (live when its key is here, then the device; the default)
   and "Silent". The live step is not a third option: it is tried whenever a key exists, and Speaking now says so.
5. **One list of speeds.** Recommended: **`AudioSpeedRule`'s presets** (Unity's, existing), the author picking the preset. Then the
   chip and Settings offer the same speeds; today they offer 0.5-2 and 0.85 / 1 / 1.2.
6. **Composed lines.** Recommended:
   - enumerate the bounded slots in the registry (counts up to a game's maximum, the wall's point names, step N of M, as Continue
     already does);
   - split joined sentences into consecutive `say` calls;
   - leave only the unbounded ones (answers, a year's distance, the points in view) live, labelled.
7. **Unity names.** Recommended:
   - `GuideConfig` with `GuideVoiceSettings voice`, plus `VoiceSet`, `GuideVoiceResolver` and `GuideVoiceService`;
   - the comments' "MascotProfile.*" renamed;
   - not `VoiceSettings`, which the search already uses.
8. **The leftovers on disk.** Gemini's 8 MP3s outside the manifest, and the 6-7 old lines per set. Recommended: drop Gemini's
   files (Gemini is developer-only, 10 TTS requests a day), and leave the old lines in place (harmless, a few KB). This is the
   developer's call; it is not part of P2.
9. **Render Sonia / Raquel** (30,912 characters, 6.2 % of a month's F0). The developer's call, per plan §5.2. Recommended: yes, in
   P2.2, so the visitor has two voices to choose from.
