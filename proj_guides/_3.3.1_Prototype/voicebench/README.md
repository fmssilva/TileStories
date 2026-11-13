# voicebench: the guide's narration, rendered once with a provider's voice

The guide's lines are rendered at authoring time into audio files, each with every word's start and end. The Lab (Detail Card >
6 · Guide > Narration: Voice = Recorded files) plays them as the app will, and the Voice test compares voices blind. The Unity
app reads the same files: one MP3 and a word-times list per line, found by what the line says.

## The accounts and keys

- **Accounts:** the steps for each one (Azure Speech F0, ElevenLabs, Google AI Studio) are in
  [`__Services_And_Keys.md`](../../__Services_And_Keys.md).
- **Keys:** they are read from the workspace's one key file, `.secrets/keys.env`, which git ignores. Copy
  `.secrets/keys.env.example` to start it.
- **Voices:**
  - Azure pt-PT: Raquel, Duarte and Fernanda. Azure en-GB: Sonia, Ryan, Libby and others.
  - ElevenLabs: a voice id from the Voice Library (filter: Portuguese, Portugal accent).
  - Gemini: its prebuilt voices.

## Rendering

```
node export_script.js                                                  # every line the guide says -> script.json
pip install azure-cognitiveservices-speech                             # once, for Azure
python render.py azure  --en en-GB-SoniaNeural --pt pt-PT-RaquelNeural
python render.py azure  --en en-GB-RyanNeural  --pt pt-PT-DuarteNeural
python render.py eleven --en <voice id> --pt <voice id>
python render.py gemini --en Kore --pt Kore               # needs: pip install lameenc (MP3)
```

- **Gemini TTS** (`gemini-3.8-flash-tts`, the AI Studio key):
  - **Word times:** it gives none, so they are estimated by characters over the part of the audio with sound, and the set is
    marked `timings: estimated`.
  - **Style direction:** in square brackets before the text. It is followed and not read aloud. Measured on 2026-11-03: a
    direction ending in a colon is spoken, which made the line 11.5 s against 6.8 s.
  - **Free tier:** about 10 requests a minute, so a 7 s pause between lines, and a retry after 20 s on a 429.
  - **Daily cap:** 10 requests a day for each TTS model (measured on 2026-11-03), so a full set (both languages, every
    audience) needs the paid tier, a few cents a set. Until then the set stays partial; the Editor shows each set's line count, and a line it lacks uses the device voice.
  - **Voices:** the prebuilt ones (Kore, Puck, Charon, Fenrir, Aoede…). The accent comes from the direction (European
    Portuguese, Lisbon).

- **Output:** `out/<set>/` (MP3 + `set.json`) and `out/voicesets.js`, which the Lab loads.
- **Published sets:** `azure-sonia-raquel` and `azure-ryan-duarte` (2026-11-03), and the ElevenLabs set
  `eleven-hpp4j3vqnfwauoo0d1us-ejartsrf33xovlh9hmuy` (2026-11-04: Luis, pt-PT, and Bella, en-US; Multilingual v2; 5,173
  characters sent; the set's id is made from the voice ids), every line of that day (58 files each). **Round 19 (2026-11-13):** `export_script.js` now asks the Lab's `guideScript()` for every
  line the guide can say (511 lines); the 459 missing ones were rendered in Ryan · Duarte only (30,747 characters, Azure F0), the
  guide's default voice. The partial Gemini set left the
  manifest (its `set.json` was never kept, and its one listed line had no audio); its MP3s stay in `out/gemini-kore-kore/`.
- **Install:** `python -m pip install azure-cognitiveservices-speech` (plain `pip` on this computer installs into a conda
  environment, not the Python that runs `render.py`).
- **The mouth's curves (Azure):** each Azure line keeps `mouth`: Azure's facial blend shapes as open / round / wide, 30
  frames a second, three characters a frame (`MOUTH_ALPHA`). `--mouth` renders again the Azure lines kept without them
  (both Azure sets were, on 2026-11-04).
- **Only what changed:** a line already rendered with the same text is skipped, so a re-run renders only the edited lines.
- **Size:** the demo point is about 5,200 characters a voice (both languages, all audiences).
- **edge** (`python render.py edge ...`): Microsoft Edge's online Read Aloud voices, the same voices as Azure.
  - It is an undocumented service with no licence, so it writes to `out-private/` (ignored by git, never published) for a
    private A/B on this computer only.
  - Use Azure F0 for anything kept.

## Publishing

- The Lab's record copy (`../Block Components Lab.html`) and the published Lab load `voicebench/out/voicesets.js` and the
  audio beside it.
- **Packs (round 19, 2026-11-13):** the published Lab is one artifact holding at most 511 files, and Ryan · Duarte now has 517
  lines. So `python pack.py azure-ryan-duarte` packs a set's MP3s into a few scripts of data URIs (`out/<set>/pack-N.js`, about
  7 MB each, listed in `out/packs.js`; git-ignored, made again before each publish). The page loads them when it is not opened
  from disk; from disk it reads the MP3s directly.
- Publish `voicesets.js`, `packs.js` and the packs with the page (the Architect does it with the Artifact tool's `files`); the
  older sets' MP3s stay published one file each.
- The Voice test's scores are saved in the published Lab's shared data, each listener's own.
