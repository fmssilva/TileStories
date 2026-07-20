# AI Coding Agent Setup — Complete Guide for TileStories (Unity)
*Researched July 19, 2026. Verify version numbers before installing — this space moves fast.*

## Quick reference: every model considered in this guide

Coding scores are SWE-bench Verified (or the closest published equivalent) — vendor-reported unless noted, treat small gaps between models as noise, not a real ranking difference. "Reasoning/debug" is a qualitative read from independent comparisons, not a single benchmark number.

| Model | Coding score | Reasoning/debug strength | Price (in/out per M) | Free way to access it? |
|---|---|---|---|---|
| Claude Fable 5 | ~95% (best available) | Excellent | Premium, no public per-token list here | No |
| Claude Opus 4.8 | 88.6% (SWE-bench Verified) / 69.2% (SWE-bench Pro) | Excellent | $5 / $25 | No |
| **Claude Sonnet 5** *(the model writing this guide)* | 63.2% SWE-bench Pro, 80.4% Terminal-Bench 2.1 (beats Opus 4.8 on this one specific benchmark) | Near-Opus-4.8 quality on most agentic/coding tasks, notably cheaper | $2 / $10 intro (through Aug 31, 2026), then $3 / $15 | No |
| GPT-5.6 Sol | ~top tier, edges out K3 | Excellent | Premium | No |
| **Kimi K3** | Strong (81.2 FrontierSWE, ranks 4th overall) | Strong, but single "max" reasoning mode → verbose/expensive | **$3 / $15** | No — no free tier, no self-host yet (weights due ~Jul 27) |
| Kimi K2.6 | Good | Good | ~$0.95 / $4.00 | No |
| Kimi K2.5 | Good | Good | ~$0.60 / $3.00 | No |
| GLM-5.2 | Strong (~94% of Opus-class on independent tests) | Strong | ~$1.1 / $4.1 | Limited — Z.ai's free API tier occasionally carries a Flash-tier GLM model, not full 5.2 |
| DeepSeek V4 Pro-Max | 80.6%, MIT, self-hostable | Strong | ~$0.44 / $0.87 hosted | No standing free tier confirmed; historically offered signup credit |
| **DeepSeek V4 Flash** | ~79% | Good, not great on hard debugging | **$0.14 / $0.28** | No standing free tier confirmed |
| **Qwen3 Coder 480B** | 69.6%, Apache-2.0 | Good for routine work | Free (self-host) / provider-hosted otherwise | **Yes — free via OpenRouter (`:free`) and Alibaba Model Studio's trial allowance** |
| **Gemini 3.5 Flash** | Decent, reported to beat Gemini 3.1 Pro on coding | Moderate | $1.50 / $9 (paid) | **Yes — Google's standing free tier (rate-limited)** |
| Gemini 3.1 Flash-Lite | Lower than 3.5 Flash | Basic | $0.25 / $1.50 (paid) | **Yes — free tier, higher throughput than Flash** |
| Llama 3.3 70B / Llama 4 Scout | Meaningfully behind the models above | Basic–moderate | N/A (open weight) | **Yes — free via Groq and OpenRouter, very fast** |
| Mistral Codestral | Decent for completion/refactors specifically | Basic–moderate | N/A on free tier | **Yes — Mistral's free "Experiment" tier, but only ~2 RPM** |

**Bottom line for your project:** DeepSeek V4 Flash (Tier 1) and GLM-5.2/Kimi K2.6 (Tier 2) remain the best paid value. Qwen3 Coder 480B and Gemini 3.5 Flash are the two free options actually worth using for real work, not just testing — details in §3.

---

## 0. First: what's wrong in the notes you attached

Your uploaded notes are mostly directionally right but have real errors — don't follow them blindly:

- **Kimi K3 is real, but it's not a budget model.** Moonshot released it July 16, 2026 (2.8T-parameter MoE, 1M context). It's genuinely strong, but priced at Claude-Sonnet levels ($3/$15 per M tokens) with open weights not yet published — it belongs in the "reserve for hard problems" tier, not the daily-driver tier. See §3 for the full breakdown and where it actually fits.
- **Roo Code (an alternative to Cline) shut down May 15, 2026.** Its own docs point migrators to **Cline** or **Kilo Code**. Good news: your notes already picked Cline.
- `CLINE_COMMAND_PERMISSIONS` / `CLINE_DATA_DIR` env vars, exact `cline config` CLI flags, etc. — I can't fully verify these exact names in current docs, so **check Cline's own docs UI (gear icon → docs link) before relying on them**; use the in-app settings UI as the source of truth rather than memorized env var names.
- `codebase-memory-mcp` **is real** (github.com/DeusData/codebase-memory-mcp, MIT, actively maintained, C# is a supported language) — that part of your notes checks out and is genuinely a good pick for a growing Unity/C# codebase.

---

## 1. The stack, in one picture

```
VS Code
 └─ Cline extension (the agent)
     ├─ Model provider: cheap model for daily work + 1 strong model for hard problems
     ├─ .clinerules/          → your existing TileStories guidelines, split into files
     └─ MCP servers:
         ├─ GitHub (official, github/github-mcp-server)
         ├─ Unity  (CoplayDev/unity-mcp)
         ├─ Browser/docs (Playwright MCP)
         └─ Codebase graph (DeusData/codebase-memory-mcp)
```

Why Cline and not something else: it's free/open-source (Apache 2.0), has the most mature MCP ecosystem of any VS Code agent, works with **any** model provider (so you're never locked into paying Copilot/OpenAI prices), and has an official CLI now too if you ever want to drive it from the terminal or run parallel tasks in git worktrees.

---

## 2. Install the base stack

1. Install **VS Code**, then install **Cline** from the marketplace (extension id `saoudrizwan.claude-dev`). It's currently a very mature project (5M+ installs, MIT/Apache2, v3.8x).
2. Open the Cline sidebar → gear icon → this is where you add providers, not obscure config files. Trust the UI over any cached doc snippet (yours or mine) — it changes often.
3. Optional: install the **Cline CLI** (`v3.0.x`, macOS/Linux, preview) later if you want multi-agent work via git worktrees (one agent per branch) — not needed for a solo six-month thesis project, skip for now.

---

## 3. Models: two-tier setup — cheap daily driver + stronger debug/architecture model

Kimi K3 ranks 4th overall on independent evaluations, ahead of Claude Opus 4.8 on several coding/agentic benchmarks — but it only supports one "max" reasoning-effort level (every call burns a lot of output tokens), is measured slow (~62 tok/s), and has no published weights yet (due July 27, so self-hosting isn't an option even if your hardware allowed it). Strong, not cheap — it goes in Tier 3 below.

You don't need one model for everything — Cline lets you switch the active model per message, and set defaults per task type. Two tiers cover your project well:

### Tier 1 — Daily implementation driver (cheap, near-free)
**DeepSeek V4 Flash** — **$0.14 input / $0.28 output per M tokens**, ~79% SWE-bench Verified, OpenAI-compatible API, automatic prompt caching (drops repeated-context cost by ~90%). This is the cheapest model that's actually good enough for real agentic coding work right now, and it's what should run 90%+ of your tasks: writing POI blocks, wiring UI, implementing the config schema/compiler, refactors, most of your Stage 1–7 feature work. At normal usage this realistically costs **a few dollars a month**, not per day.

*Backup/alternative in this tier:* GLM-5-Flash (Z.ai) — similarly cheap, also solid for routine work, useful if DeepSeek is rate-limited or you want a second opinion without cost jumping.

### Tier 2 — Debugging & harder architecture decisions (still cheap, notably stronger)
**GLM-5.2** — **~$1.1 input / $4.1 output per M tokens** (or **Kimi K2.6** at a similar price point). Both are meaningfully stronger reasoners than the Flash-tier models — GLM-5.2 is reported at roughly 94% of Claude Opus-class coding performance on independent comparisons — while staying well under Claude/GPT/K3 frontier pricing. Reach for one of these specifically for:
- The "three architecture options, pick one" and "three implementation approaches" steps your guidelines require before any non-trivial feature.
- A failing test where the first fix attempt didn't work and you need real reasoning about *why*.
- `CircuitStateMachine`, `LapseStateManager`, badge-trigger evaluation — the state-machine-heavy classes your work plan already flags as edge-case-prone.

### Tier 3 — Reserve, don't default to
Kimi K3, Claude Opus/Fable, or GPT-5.6 — genuinely the strongest available, but priced and rate-limited like it. **Claude Sonnet 5** is worth calling out specifically here: it lands close to Opus 4.8 on most agentic/coding benchmarks at under half the price ($2/$10 introductory through Aug 31, 2026, then $3/$15) — if you do end up leaning on Tier 3 fairly often, Sonnet 5 is the better cost/quality trade-off than reaching straight for Opus or Fable. Still, use Tier 3 only when Tier 2 has failed twice on the same problem, not as a routine escalation.

### Tier 0 — Actually free (not just "cheap")

Yes, there are real free options — but read the caveat below before planning around them.

- **Qwen3 Coder 480B, free via OpenRouter** (`qwen/qwen3-coder:free` or similar tag — check current listing). This is the standout: 69.6% SWE-bench Verified, Apache-2.0, genuinely competitive with several paid mid-tier models above. Also available with a free trial token allowance via Alibaba's own Model Studio if you want it direct rather than through OpenRouter.
- **Gemini 3.5 Flash / Gemini 3.1 Flash-Lite, free via Google AI Studio.** Google's standing free tier (no card, no trial expiry) — Flash is reported to beat the older 3.1 Pro on coding/agentic benchmarks, and it's a real production API, not a sandbox. Roughly 10–15 requests/minute and up to ~1,500 requests/day as of mid-2026 (Google adjusts these; check AI Studio for your project's live numbers). Note: Google states free-tier prompts may be used to improve their models — don't run anything from your codebase through it that you'd consider sensitive (unlikely to matter for a solo AR thesis, but worth knowing).
- **Groq**, free tier, hosting Llama 3.3 70B / Llama 4 Scout / Qwen3-32B — extremely fast inference, weaker coding quality than the options above, best for quick lookups or high-volume trivial tasks rather than real implementation work.
- **Mistral's free "Experiment" tier** includes Codestral (code-completion-specialized) — decent quality, but capped at ~2 requests/minute, too slow for an agentic loop that makes many calls per task; fine for occasional manual completions, not for Cline driving it.

**The real caveat with all of the above:** Cline's agentic loop makes many small tool-calls per task (read file, propose edit, run command, check result...), not one call per task. A "50 requests/day" or even "1,500/day" free cap can get eaten by a single non-trivial feature faster than it looks on paper. Free tiers are genuinely good for: testing your whole setup before spending anything, light/occasional tasks, and Qwen3 Coder specifically is good enough to lean on for real work if you're comfortable hitting rate limits sometimes. They're not a realistic full replacement for Tier 1 across a six-month project — Tier 1's cost is low enough ($ a few/month) that free-tier rate-limit friction usually isn't worth fighting once you're doing daily agentic work.

---

## 4. How to actually connect these models — API keys, no local hosting needed

**Is Cline itself free? Yes — confirm this if your Cline dashboard shows "Credits" or a "ClinePass" upsell.** Cline the extension/CLI is fully open-source and free, no subscription needed. What you may see in its dashboard is two *optional* things layered on top, separate from the tool: a small free welcome "Credits" balance for Cline's own built-in hosted provider, and **ClinePass** ($9.99/mo) — Cline's own flat-fee alternative to OpenRouter, giving higher rate limits (2–5x) on a curated set of open-weight models (GLM, Kimi, DeepSeek, MiniMax, Mimo) without you juggling separate provider accounts. Worth knowing about, but everything below (bring-your-own-key) works with zero Cline platform fee — you're not missing anything by skipping it.

**A caveat on the OpenRouter route specifically (Option A below):** there's a currently open GitHub issue against Cline where its OpenRouter integration doesn't pin requests to one underlying provider, so sequential tool-calls within a single task can silently hit different backend providers. Since prompt caching is provider-local, this can kill your cache hits and inflate cost 5–10x on long agentic runs — directly undercutting the "cheap" math for Tier 1. If you notice costs running higher than expected on OpenRouter, that's the likely cause; direct provider keys (Option B) don't have this problem since there's only ever one provider.

You don't need to host anything yourself, and given your hardware that's the right call — every model above is called as a cloud API over HTTPS, the same way Copilot already works, just pointed at a different company's server. You have two ways to do it:

### Option A — One aggregator key (simplest, recommended for you)
**OpenRouter** (openrouter.ai) is a single gateway in front of 300+ models, including DeepSeek, GLM, Kimi, and everything else above. One account, one API key, one prepaid credit balance, OpenAI-compatible format.
1. Sign up at openrouter.ai, add a small credit balance (even $5–10 goes a long way at these per-token prices, and it also raises your rate limits on any free-tier models you use).
2. Create an API key from the dashboard.
3. In Cline: Settings (gear icon) → API Provider → **OpenRouter** → paste the key.
4. Pick your model from Cline's model dropdown by typing e.g. `deepseek/deepseek-v4-flash` for daily work, `z-ai/glm-5.2` or `moonshotai/kimi-k2.6` for the debug tier — Cline lets you swap this per message, no reconfiguring.
5. Downside: OpenRouter adds a small platform fee (~5%) on top of each provider's raw price, and free-tier models on it rotate/get retired without much notice — fine for a paid setup like yours, just don't build around a specific `:free` model long-term.

### Option B — Direct provider keys (marginally cheaper, more accounts to manage)
Skip the aggregator fee by going straight to each lab:
- **DeepSeek**: platform.deepseek.com → create key → base URL `https://api.deepseek.com`, model `deepseek-v4-flash`.
- **Z.ai (GLM)**: z.ai's developer platform → create key → OpenAI-compatible endpoint, model `glm-5.2`.
- **Moonshot (Kimi)**: platform.moonshot.ai → create key → base URL `https://api.moonshot.ai/v1`, model `kimi-k2.6` (or `kimi-k3` for the reserve tier).

In Cline, add each as a separate provider profile under Settings → API Provider → **"OpenAI Compatible"** (paste that provider's base URL + key + model name). You can keep 2–3 provider profiles configured simultaneously and just switch the active one from Cline's dropdown — no need to pick only one.

**Practical recommendation for you specifically:** start with Option A (OpenRouter) — one signup, one key, and you can try DeepSeek V4 Flash and GLM-5.2/Kimi K2.6 side by side in the same afternoon without juggling three dashboards. Switch a given model to a direct key later only if you end up using it heavily enough that the ~5% aggregator fee actually adds up.

**None of this requires you to run anything locally.** No GPU, no Ollama, no model weights on your machine — Cline just makes an HTTPS call to whichever endpoint you configured, exactly like it would to OpenAI or Anthropic. Your "not a good PC" is irrelevant to any option above; it would only matter if you wanted to self-host an open-weight model, which nothing here requires.

### Connecting Tier 3 (Claude, GPT, Kimi K3)
For occasional escalation, either add Anthropic/OpenAI/Moonshot as direct provider profiles the same way as Option B, or just call them through OpenRouter like everything else (`anthropic/claude-sonnet-5`, `openai/gpt-5.6-sol`, `moonshotai/kimi-k3`) — since these are used rarely, the OpenRouter fee is negligible in absolute terms and not worth a separate signup just for this tier.

### Connecting the free-tier options
- **Qwen3 Coder (free)**: if you're already on OpenRouter for Option A, just select the `:free`-tagged Qwen3 Coder entry from Cline's model dropdown — same key, no extra signup.
- **Gemini (free)**: sign up at Google AI Studio (aistudio.google.com), generate an API key (no card required), then in Cline add it as a separate provider — Cline has a native **Gemini** option in its provider list, so you don't need the generic "OpenAI Compatible" route for this one. Pick `gemini-3.5-flash` or `gemini-3.1-flash-lite`.

---

## 5. Set up your `.clinerules/` from your existing guidelines

You already have a very detailed `TileStories_Unity_Guidelines.md` — don't just dump it as one file. Split it, since Cline reads and toggles `.clinerules/*.md` individually.

Create this structure at the repo root — this covers every section of your guidelines doc, including the two that are easy to miss (§5 UI rules, §8 the write-incrementally rule):

```
.clinerules/
  00-process.md         ← §0, §1, §8, §11 (before-you-start, think-before-you-code,
                           write files incrementally not all-at-once, general principles)
  10-structure.md       ← §2 (Framework/Apps split, Editor/Runtime asmdef separation)
  20-code-quality.md    ← §3, §4 (code quality, comments/logging/assertions)
  30-ui-content.md      ← §5 (UI, visual design, content rules)
  40-testing.md         ← §6, §7 (the 4-tier testing strategy, human-in-the-loop protocol)
  50-terminal.md        ← §9 (PowerShell conventions: `;` not `&&`, redirect not pipe)
  60-finishing.md       ← §10 (plan doc updates, summary format)
```

Each file comfortably clears Cline's own "keep it under ~150 lines per file for reliable adherence" guidance — your whole guidelines doc is ~260 substantive lines split seven ways.

**Don't path-scope these with `paths:` frontmatter.** Cline supports it (a rule with `paths: ["Assets/**"]` only loads when the file it's currently working on matches), and it's genuinely useful on projects that mix unrelated domains. But nearly everything Cline touches on this project already lives under `Assets/` or `Packages/`, so scoping buys almost no context savings — while creating a real failure mode: your §10 finishing step requires updating `/Docs/work-plan.md`, which is outside any Unity path, so a scoped structure/quality rule could silently stop applying at exactly the point Cline is writing that update. Leave every file unscoped (no frontmatter) so it always loads; at ~260 total lines the token cost of "always load everything" is negligible.

**Do add a `description:` frontmatter field to each file** (even without `paths:`) — it's what labels each rule in Cline's toggle UI, so you can see and switch off e.g. `30-ui-content.md` for a pure logic/testing session without hunting through file contents to remember what's in it. Example:
```yaml
---
description: UI, visual design, and content-authoring rules
---
```

**One gotcha to know about, not to use:** don't name any of these files or subfolders `workflows/`, `hooks/`, or `skills/` inside `.clinerules/` — those are reserved directory names Cline treats specially and excludes from normal rule loading.

This keeps your existing, carefully-written guidelines exactly as authoritative — you're just making them machine-readable per-topic instead of one giant file, and toggleable when a session doesn't need a particular topic (e.g. flip off `30-ui-content.md` while deep in `CircuitStateMachine` work, remembering that toggles reset each VS Code session — this is for temporary focus, not permanent removal).

---

## 6. MCP servers, in the right order

Don't install all of these day one — add them as you actually need the capability. Order below is priority.

### 6.1 GitHub — official server
`github/github-mcp-server` (GitHub's own, actively maintained). Two ways to run it:
- **Remote/hosted** (easiest): if your MCP client supports remote servers, GitHub hosts it — OAuth login, no token to manage.
- **Local**: Docker image `ghcr.io/github/github-mcp-server`, or native binary, with a personal access token env var. Use `GITHUB_READ_ONLY=1` at first if you want it to only browse issues/PRs/code without writing — a sane default until you trust the workflow.

### 6.2 Unity — CoplayDev/unity-mcp
This is the de facto standard, **free and MIT-licensed** (don't confuse it with Unity's own official paid MCP offering — the community one is what you want). ~10k GitHub stars, actively maintained, supports Unity 2021.3 LTS → 6.x (covers your Unity 6.3 LTS).
- Install: Unity → Package Manager → Add package from git URL → `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main`
- Requires Python 3.10+ (via `uv`) on your machine for the bridge server.
- Configure: Window → MCP for Unity → "Configure All Detected Clients" — it auto-writes the config for Cline/VS Code.
- Gives the agent 47+ tools: scene/prefab management, script editing, running Unity Test Framework tests, builds, profiling. This directly supports your guidelines' Tier A–D testing workflow (the agent can literally trigger EditMode/PlayMode batch tests itself).

### 6.3 Browser/docs — Playwright MCP
For Immersal API docs, AR Foundation docs, Firebase Unity SDK references, forum troubleshooting. Use Microsoft's **Playwright MCP** (`@playwright/mcp`) — it's the most maintained browser-automation MCP and works well for "fetch and read this doc page" style research. Keep it narrow: don't give it open-ended "browse the internet" permission, just fetch/read.

### 6.4 Codebase graph — codebase-memory-mcp
`DeusData/codebase-memory-mcp` — real, MIT, single static binary, no dependencies, C# is a supported language (via tree-sitter + LSP type resolution), builds a persistent knowledge graph (functions, classes, call chains) instead of re-reading files every session. This is genuinely well-suited to your project: a growing multi-wall C# codebase where "what already calls this / what already does part of this" is exactly the search your own guidelines (§0: "search for existing types before writing a duplicate") ask the agent to do every single task.
- Runs locally, no telemetry, indexes in milliseconds, persists to a local cache.
- Re-index it periodically (or on a git hook) — a stale graph is worse than no graph.

**Skip for now:** a dedicated project-management MCP, Firebase MCP, or anything beyond these four. Your notes' own advice — "don't start with 10 MCPs" — is correct.

---

## 7. First-day checklist

1. Install VS Code + Cline.
2. Sign up for OpenRouter, add credit, generate a key; in Cline set it as provider with **DeepSeek V4 Flash** as the default model.
3. Create `.clinerules/` and split your existing guidelines doc into the 7 files listed in §5.
4. Add the GitHub MCP server (read-only to start).
5. Add the Unity MCP server (`CoplayDev/unity-mcp`) — this is your highest-value MCP given the project.
6. Add Playwright MCP for docs lookups.
7. Add `codebase-memory-mcp` once you have more than a handful of files to index.
8. Work through §8 below to confirm every piece actually talks to every other piece before trusting it with real edits.

## 8. Verify everything actually works, piece by piece

Do these in order — each one isolates a different failure point, so if something's wrong later you'll already know it isn't this layer.

1. **Model connection.** In Cline, send a plain question with no tools involved ("what's 2+2"). Confirms the API key and provider are wired correctly before anything more complex touches it.
2. **`.clinerules` are loading.** Ask Cline to state back, in its own words, one specific rule from `40-testing.md` (e.g. "what's Tier A in this project's testing strategy?"). If it can't answer from the rules file, the folder isn't being picked up — check it's at the repo root and each file is toggled on in Cline's rules panel.
3. **GitHub MCP.** Ask Cline to list your 5 most recent commits or open issues. Confirms auth (OAuth or PAT) and that the toolset is actually registered, not just configured.
4. **Unity MCP.** With the Unity Editor open, ask Cline to report the current scene hierarchy, then to run the EditMode test suite and report pass/fail counts. This exercises both the read path (scene inspection) and the write/execute path (triggering Unity Test Framework) your testing guidelines depend on — check the compile log for `error CS` lines first, exactly as §6.2 of *your* `TileStories_Unity_Guidelines.md` specifies.
5. **Playwright MCP.** Ask Cline to fetch a specific AR Foundation or Immersal docs page and summarize one paragraph. Confirms it can actually reach and parse a live page, not just that the server started.
6. **codebase-memory-mcp.** After indexing, ask a structural question a grep couldn't answer well — "what calls `WallSession`'s localization callback?" A real graph answer (not a guess dressed up as one) confirms the index is populated and being queried, not silently skipped.
7. **End-to-end.** Only after 1–6 pass individually: give Cline one real, small task from your work plan (e.g. a Stage 1 item) and watch it actually use the TODO-list-first, three-options-before-implementing pattern from your guidelines — that's the real proof the whole stack (model + rules + MCPs) is working together, not just each piece in isolation.

## 9. What not to do

- Don't default to a Tier 3 model (Kimi K3, Claude, GPT) for routine edits — that's where costs quietly become "not actually cheaper than Copilot."
- Don't give terminal/shell access unrestricted permissions — approve commands individually until you've built trust, especially since your guidelines rely on precise PowerShell conventions (`;` not `&&`, output redirected to a log file, not piped) that a model might not follow by default without `.clinerules/50-terminal.md` reinforcing it every session.
- Don't skip Tier A/B (mock localization / XR Simulation) testing in favor of always asking the agent to reason about real-device behavior — that's expensive in both your time and model tokens, and your own guidelines already solved this with the 4-tier system; make sure `40-testing.md` actually encodes it so the agent defaults to the fast loop.
- Don't let a stale codebase graph go unnoticed — re-index `codebase-memory-mcp` after large refactors, or it'll confidently answer with outdated structure.