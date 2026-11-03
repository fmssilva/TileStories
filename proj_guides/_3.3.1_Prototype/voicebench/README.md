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
- **Published sets (2026-11-03):** `azure-sonia-raquel` and `azure-ryan-duarte`, every line. The partial Gemini set left the
  manifest (its `set.json` was never kept, and its one listed line had no audio); its MP3s stay in `out/gemini-kore-kore/`.
- **Install:** `python -m pip install azure-cognitiveservices-speech` (plain `pip` on this computer installs into a conda
  environment, not the Python that runs `render.py`).
- **Only what changed:** a line already rendered with the same text is skipped, so a re-run renders only the edited lines.
- **Size:** the demo point is about 5,200 characters a voice (both languages, all audiences).
- **edge** (`python render.py edge ...`): Microsoft Edge's online Read Aloud voices, the same voices as Azure.
  - It is an undocumented service with no licence, so it writes to `out-private/` (ignored by git, never published) for a
    private A/B on this computer only.
  - Use Azure F0 for anything kept.

## Publishing

- The Lab's record copy (`../Block Components Lab.html`) and the published Lab load `voicebench/out/voicesets.js` and the
  audio beside it.
- Publish `out/` with the page (the Architect does it with the Artifact tool's `files`).
- The Voice test's scores are saved in the published Lab's shared data, each listener's own.
