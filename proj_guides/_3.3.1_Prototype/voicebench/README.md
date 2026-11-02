# voicebench: the guide's narration, rendered once with a provider's voice

The guide's lines are rendered at authoring time into audio files, each with every word's start and end. The Lab (Detail Card >
6 · Guide > Narration: Voice = Recorded files) plays them as the app will, and the Voice test compares voices blind. The Unity
app reads the same files: one MP3 and a word-times list per line, found by what the line says.

## The accounts (the developer creates them; the keys stay on this computer)

1. **Azure for Students** (free, no card, the university e-mail): https://azure.microsoft.com/free/students
   - Create a resource: **Speech** (tier **F0**, region e.g. West Europe).
   - Open **Keys and Endpoint** and copy KEY 1 and the Location.
   - F0 includes 0.5 M neural characters a month. The pt-PT voices are Raquel, Duarte and Fernanda, and the en-GB voices are
     Sonia, Ryan, Libby and others. MAI-Voice (pt-PT-Rui) is paid from the $100 credit.
2. **ElevenLabs** (Free: 10k characters a month, non-commercial; Starter $6, first month $1): https://elevenlabs.io
   - Profile > API Keys.
   - Pick a voice in the Voice Library (filter: Portuguese, Portugal accent) and copy its voice id.
3. **Google AI Studio** (Gemini; free for development, paid tier needed before a study with EEA users):
   https://aistudio.google.com > Get API key. Gemini TTS gives no word times: not rendered here yet.

Copy `.env.example` to `.env` in this folder and fill in the keys. `.env` is ignored by git.

## Rendering

```
node export_script.js                                                  # every line the guide says -> script.json
pip install azure-cognitiveservices-speech                             # once, for Azure
python render.py azure  --en en-GB-SoniaNeural --pt pt-PT-RaquelNeural
python render.py azure  --en en-GB-RyanNeural  --pt pt-PT-DuarteNeural
python render.py eleven --en <voice id> --pt <voice id>
```

- **Output:** `out/<set>/` (MP3 + `set.json`) and `out/voicesets.js`, which the Lab loads.
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
