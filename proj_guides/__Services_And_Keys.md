# Services and keys

Every outside service TileStories uses has an entry here, with:

- its account and where its key comes from;
- what reads the key;
- what it costs;
- how to replace the key.

The studies behind the choices are [_3.4 The Guide](_3.4_The_Guide_Study.md) (voices), [_3.6 Guide Agent](_3.6_Guide_Agent_Study.md)
(the live agent) and [_3.7 Mascot Pipeline](_3.7_Mascot_Pipeline_Study.md) (3D and Blender).

## 1. Where the keys live

- **One file:** every key is in `TileStories/.secrets/keys.env`.
  - git ignores the folder; only `keys.env.example` (the same names, empty) is committed.
  - Never copy a key into another file of the repo, a doc, a test or a commit message.
- **Readers:**

| Reader | What it takes | How |
|---|---|---|
| `voicebench/render.py` | Azure, ElevenLabs, Gemini | Reads `.secrets/keys.env` (the environment wins when a variable is set there). |
| The local Lab (file://) | Gemini, Anthropic | `lab_src/keys_local.py` writes `keys.local.js` beside the Lab pages (git ignores it). The Lab's Services panel shows which keys it found, never the key itself. |
| The published Lab (claude.ai) | none | The live agent answers through Claude, through the artifact's own `sample` capability. No key is ever published. |
| The Unity Editor (to build, with the agent's Unity step) | Gemini, Anthropic, Azure, ElevenLabs | A menu item imports `.secrets/keys.env` into EditorPrefs (this machine only), for Editor tests; a build check fails a build whose assets hold a key. |
| The app on a phone | none | The phone asks our token service (a Cloudflare Worker, to build), which holds the keys as secrets. A key in an APK can be read by anyone. |

## 2. The services

Status as of 2026-11-03.

| Service | Used for | Key name(s) | Status | Cost and limits |
|---|---|---|---|---|
| **Google AI Studio** (Gemini) | the agent's model in the local Lab, embeddings for the library, TTS voices | `GEMINI_API_KEY` | **Working.** Text, embeddings and TTS answer. The local Lab's Ask Azu runs on it (Flash-Lite: first words in about 2 s, about $0.0013 a question). A page opened from a file may call it (CORS allows origin `null`). | Free tier for development only: Google may use the data, and EEA users need the paid tier. TTS free tier is 10 requests a day for each model, so the Gemini voice set has 7 of 106 lines. |
| **Anthropic** (Claude) | the recommended agent model (Haiku 5.5) for the app and the user study | `ANTHROPIC_API_KEY` | Not created | Pay as you go; prompt caching cuts the library's cost. |
| **Azure Speech** (Azure for Students) | recorded voices with exact word times (pt-PT Raquel, Duarte, Fernanda; en-GB Sonia, Ryan) | `AZURE_SPEECH_KEY`, `AZURE_SPEECH_REGION` | **Working** (2026-11-03): a Speech resource, region `germanywestcentral` | F0: 0.5 M neural characters a month, free. A full set is about 5,200 characters. |
| **ElevenLabs** | recorded voices with word times (`with-timestamps`) | `ELEVENLABS_API_KEY`, `ELEVENLABS_VOICE_PT`, `ELEVENLABS_VOICE_EN` | **Working** (2026-11-03): the API key answers; the voice ids are not chosen yet | Free: 10k characters a month, non-commercial. Starter: $6 a month, first month $1. |
| **Meshy / Tripo** (optional) | a 3D mascot from a picture | `MESHY_API_KEY`, `TRIPO_API_KEY` | Not created | Free credits, then paid. Only if the 2D/shader mascot is not enough. |
| **Cloudflare Workers** (later) | the token service and the agent's loop for the app | none here: the keys go into the Worker's secrets | Not created | The free plan covers a study. |

## 3. What the developer does, account by account

What the Architect does after each one is in *italics*.

### Google AI Studio: done

The key is in `.secrets/keys.env`.

*The Architect renders the Gemini voices and runs the live agent in the local Lab with it.*

### Azure Speech (about 5 minutes)

1. Open https://portal.azure.com, signed in with the FCT NOVA account.
2. **Create a resource** > search **Speech** > **Create**.
3. Fill in the form:
   - **Subscription:** Azure for Students.
   - **Resource group:** new, e.g. `tilestories`.
   - **Region:** West Europe.
   - **Name:** e.g. `tilestories-speech`.
   - **Pricing tier:** **Free F0**.
4. **Review + create** > **Create**, then **Go to resource**.
5. **Keys and Endpoint:** copy **KEY 1** into `AZURE_SPEECH_KEY`, and **Location/Region** (e.g. `westeurope`) into
   `AZURE_SPEECH_REGION`.

*The Architect renders two Azure sets (Sonia/Raquel and Ryan/Duarte) and publishes them. They become the default, with exact
word times.*

### ElevenLabs (about 5 minutes)

1. Create the API key:
   1. Go to https://elevenlabs.io > the profile (bottom left) > **API Keys** > **Create API key**.
   2. Allow **Text to Speech** and **Voices: read**.
   3. Copy the key into `ELEVENLABS_API_KEY`.
2. Pick the voices:
   1. Go to **Voices** > **Voice Library**.
   2. Filter by language Portuguese and accent Portugal. Listen, then **Add to my voices**.
   3. On the voice, copy its id into `ELEVENLABS_VOICE_PT`.
   4. Do the same for an English voice, into `ELEVENLABS_VOICE_EN`.
3. Optionally, connect the MCP so Claude Code can browse voices and make sound effects:
   1. In a terminal, run `claude mcp add --scope user --transport http elevenlabs https://api.elevenlabs.io/v1/mcp`.
   2. In the VS Code Claude panel, type `/mcp` > elevenlabs > **Authenticate**.
   3. The MCP signs in with the account and needs no key. The renderer still uses the API key, because only the API returns word
      times.

*The Architect renders an ElevenLabs set. The free 10k characters cover one voice in both languages.*

### Anthropic (when the agent moves to the app)

1. Go to https://console.anthropic.com > **API Keys** > **Create Key**.
2. Copy the key into `ANTHROPIC_API_KEY`.
3. Add a few dollars of credit and set a monthly limit (**Limits**).

*The Architect switches the local Lab's agent to Haiku 5.5 to compare it with Gemini, then uses it in the token service.*

### Blender and the blender-mcp server (for the 3D mascot)

1. Install Blender 4.x from https://www.blender.org/download.
2. Install `uv`. In PowerShell, run `powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 | iex"`.
3. Install the add-on:
   1. Download `addon.py` from https://github.com/ahujasid/blender-mcp.
   2. In Blender, open **Edit > Preferences > Add-ons > Install from disk** and pick `addon.py`.
   3. Enable **Interface: Blender MCP**.
4. Connect: in the 3D view press **N** > **BlenderMCP** tab > **Connect to Claude**.
5. Register the server in a terminal: `claude mcp add --scope user blender -- uvx blender-mcp`.
   - Always use `--scope user`. Without it, the server is saved for the folder as the terminal spells it
     (`C:/Users/...`), while VS Code opens the same folder as `c:/Users/...`. To Claude Code those are two projects, and the
     chat never sees the server (found on 2026-11-03).
6. Start a new conversation in the VS Code Claude panel (servers load when a conversation starts). Blender must stay open,
   with the add-on connected (port 9876), while the Architect uses it.

*The Architect builds the tile mascot in Blender, rigs it and exports GLB/FBX for Unity (see _3.7).*

### Meshy or Tripo (optional)

1. Create the account:
   - Meshy: https://www.meshy.ai > **API**.
   - Tripo: https://platform.tripo3d.ai > **API Keys**.
2. Copy the key into `MESHY_API_KEY` or `TRIPO_API_KEY`.

*The Architect sends a picture from `proj_guides/mascote/` and imports the model.*

## 4. Replacing the keys

Do this once the whole system works. Every key is replaced in one file, and nothing else in the repo changes. A key goes
only into `.secrets/keys.env`, never into a script, even for a moment: `keys_local.py` is committed.

1. **Gemini, Azure and ElevenLabs:** all three keys were shown in a chat on 2026-11-03, so all three are replaced.
2. **Gemini:**
   1. In https://aistudio.google.com > **API keys**, delete the current key.
   2. **Create API key** in the same project.
   3. Paste it into `GEMINI_API_KEY`.
3. **Every other key:** create a new one in its console, paste it into `.secrets/keys.env`, then delete the old one.
4. Run `python proj_guides/_3.3.1_Prototype/lab_src/keys_local.py` so the local Lab picks up the new keys.
5. Check that each key answers by running `python proj_guides/_3.3.1_Prototype/lab_src/keys_local.py --check`. It prints `ok` or
   the HTTP error for each service, never the key.
6. If a key ever reaches a commit:
   1. Delete it in its console first, since the history keeps it.
   2. Then remove it from the repo.
