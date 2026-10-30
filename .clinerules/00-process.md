## 1. Roles and Modes

Two agents share these rules:
- **Architect** (a review chat): verifies the last block on disk, decides, keeps the guides in sync, commits, and writes the next
  block's brief. Its own procedure: `proj_guides/__AI_Architect.md`. It does not write code.
- **Worker** (a fresh Claude Code session per block): plans the block, then builds it. Its own procedure: `proj_guides/__AI_worker.md`,
  the brief in its `## And now:` section (written there by the Architect). Moment-specific procedures: `proj_guides/skills/`.

Both work in two modes, always in this order:
- **PLAN MODE:** read, audit the code on disk, weigh options, write the plan. No file modifications except the plan tracker.
- **ACT MODE:** modify files, build, test, look, verify. No redesign of core architecture without going back to PLAN MODE.

After a context compaction, re-read the brief and `proj_guides/__curr_plan_tracker.md` (the Worker's plan for the current block)
before continuing.

### Delegating to subagents (both agents)
A subagent (the `Agent` tool) has its own context; only its final report comes back. Use it to keep your own context small, the agent
deciding case by case:
- **Delegate** a self-contained task whose answer fits in ~40 lines and that would otherwise fill your context with material you need
  only a conclusion from (many file reads, web pages, long logs). **Do not delegate** what is quicker inline, what needs the developer
  (decisions, reviews), commits, or edits to the guides.
- **Smallest model and effort that does it well; ceiling Opus + medium** (High-effort work stays with the caller). Haiku + low
  (`Explore` when read-only): find usages, list files, pull the failures out of a long log. Sonnet + medium: web research (links and
  short descriptions; pictures saved to the scratchpad, named by path), an audit of files against one rule, a doc draft, mechanical
  edits from an exact spec. Opus + medium: compare options for a self-contained problem, implement one isolated, well-specified unit
  (a pure rule and its EditMode tests). A weak result -> re-run once a rung higher, not patched inline.
- **The brief** gives the goal, the exact paths to read (only the rule sections the task needs -- never the whole session procedure of
  `__AI_worker.md`), what it must NOT do (no edits for research, no Unity, no commits, no git), and the output shape: verdict first,
  evidence as `file:line` or URLs, a length cap.
- **Verification stays with the caller:** a subagent's report is input, not proof (Evidence Discipline). Code it wrote: read the diff,
  compile and run the tests yourself. Research: spot-check the key claims before deciding.
- **Unity has one owner:** one Editor, one UnityMCP. Only one agent drives Unity at a time (compiles, runs, Play Mode, captures); a
  subagent does Unity work only while its caller waits on it. No worktrees for Unity work (no Editor attached).
- The final report (`60-finishing.md` 6.4) lists every subagent used, with its model and effort.

---

## 2. Shared Core Rules

### Architectural & Benchmark Standards
* **Academic Reference Quality:** Write code as if this project is a reference implementation for learning. If existing code is messy or incorrect, do **not** patch around it with backward-compatibility shims, wrappers, or adapters. Propose a clean refactor, obtain agreement, and fix it properly.
* **Patience Over Speed:** Quality and clarity supersede execution speed. Unverified, messy, or duplicate code is unacceptable.
* **Stop & Clarify:** If existing code is ambiguous, contradictory, or appears wrong, halt immediately. Ask or flag the issue instead of building on assumptions.

### Framework Completeness
* **All the good options, none of the noise.**
  - This is a framework, so when several options are good, build them all and make the best one the default. Do not pick
    one and drop the rest.
  - Every candidate option passes one filter:
    - **Option:** it depends on taste or on the wall (style, mood, a building's shape). It becomes a setting.
    - **Fixed rule:** one answer is clearly right for every wall by UX, design or accessibility principles. It is
      hard-coded, and the reason is written in the domain guide.
    - **Dropped:** no wall developer would change it, or nobody would miss it.
  - A good feature is never left out. An irrelevant knob is never added.
* **Configured in the Editor.** Every option is a row in the POI Editor, with:
  - an (i) help;
  - a sensible default, per wall and per block or item where that makes sense;
  - undo / redo / save;
  - live sync in Scene and Play Mode;
  - a Test guide (Scene / Play Mode / Device) that names Editor controls, never code, and is app-agnostic.

  Each option is tested.
* **Define, assign, place.**
  - The wall's Global Scene defines named styles once.
  - Hierarchy levels assign them.
  - A POI or item places its own content (position, shapes, media) and may pick another named style.
  - Values are not overridden one by one per item: a different look is a new named style.
* **Unity's normal way of working.** A wall developer should never have to learn "the framework" before using it:
  - Content is authored with the tools a Unity developer already knows: the Scene view and its handles, Prefab Mode,
    Splines, the Animation window, Timeline, Particle System, the PSD Importer, ProBuilder.
  - Content is ordinary Unity assets in the wall's folders.
  - The framework adds only what needs the wall, the POI or its events: anchoring, config sync, Editor rows, templates.
  - It never builds a private copy of a Unity tool.
* **Defaults are judged in the real scene.** A default is chosen by looking at it in Unity, and on the phone for anything
  visual in AR. Values tried in a prototype are experiments, not defaults.
* **Real fixtures.** Tests run on the LivingRoom wall's POI sets (The Lamp's 12+ POIs, The Painting, The Camera); add a POI
  when an edge case needs one, never change an existing POI's identity fields (id, name, category, summary, keywords).

### Guidelines and the structure map are "version 0", never logs
* **What this covers:** every file in `.clinerules/` (the structure map `10-structure.md` included), `proj_guides/__AI_*.md` and
  `proj_guides/skills/`. They describe how to work and what exists NOW, as if written today for a newcomer.
* **When something changes, REWRITE the line in place** so it states the current truth. Never append what was done, when, by which
  step or block: no dates, no "(added at step X)", no "was / used to / now", no "verified on". History lives in git, in the domain
  guides' status rows and Build logs, and in the plan tracker -- never in a guideline or the structure map.
* **A lesson learned enters a guideline only as a better rule**: the simplest wording that makes the mistake unlikely, placed where
  an agent reads it at the moment it matters. Add a short "why" only when the rule would look arbitrary without it.
* **The structure map:** one concise line per file -- what it IS and does, so an agent can find what exists before creating
  anything. Before you create a file, a class, a function or a document, search this map and the code for one that already does
  the job, and extend it instead of adding a duplicate.

### Evidence Discipline (Mandatory Verification)
Never report a feature, fix, rendering, visual, or behavioral task as completed or passing without providing explicit, un-paraphrased mechanical proof:
* **Valid Evidence:** Raw CLI/Test output showing an `Assert` that passed, or specific itemized observations detailing what was measured or verified.
* **Invalid Evidence:** Statements like "No errors were thrown," "Compilation succeeded," or citations from previous agent summaries do **not** count as proof. Only independently re-derived results are accepted.
* **A tool's status word is not evidence.** "idle", "ready", or "succeeded" from a tool,
  or a passing test count, is not proof that your change actually compiled or that your new
  tests actually ran. A single compile error leaves Unity running the last-good assemblies:
  counts freeze, a compile can read "compiling -> idle" without completing, and the console
  can look clean. If a test total did not move after you added tests, assume a STALE build,
  not green. Confirm the artifact instead: zero `error CS` / `ExitCode 0` in the Editor log,
  a `Library/ScriptAssemblies/*.dll` timestamp that moved, or the new type present in the loaded
  assemblies (reflection).

---

## 3. Mode-Specific Responsibilities

### PLAN MODE
Prepare a reviewed, on-disk-verified plan before writing code.
1. **Re-ground:** read the relevant `.clinerules/` (00/10/20/30/40/50/60) and the structure map (`10-structure.md`).
2. **Deep audit + compile:** open the files that actually exist for this task; a prior
   "we built X / X doesn't exist" claim is unverified until seen on disk. Find gaps, typos,
   and misalignments with the rules. Confirm the project compiles BEFORE changing anything; if the
   tooling to do that is unavailable, stop and say so. No opening test run: the Architect commits
   only green trees, so the last commit IS the baseline -- run one only when the brief's TEST PLAN
   asks for it (e.g. the tree moved since the last green commit).
3. **Architecture Level (where it lives):** map folder/assembly/component scope, weigh 3 distinct structural options with trade-offs, state the choice and why.
4. **Implementation Level (how it works):** weigh 3 concrete options (event-driven vs direct call, ScriptableObject vs hardcoded, ...), pick the simplest robust one.
5. **Execution plan + decisions:** itemized TODO incl. testing steps. Separate the choices you made autonomously (clear winner) from any real trade-off needing the developer's pick -- block only on the latter; otherwise present the plan and proceed to ACT.

### ACT MODE
1. **Strict scope lock:** implement exactly the planned task; no unsolicited refactor, formatting cleanup, or extra features.
2. **Build -> compile -> LOOK -> test, in that order, per step** (write only the files the step needs):
   1. **Build** it where the structure map says it belongs (reuse first).
   2. **Compile clean** (`40-testing.md` 4.2.5).
   3. **Look and polish BEFORE the PlayMode tests** (anything visible): capture it in the real scene (light and dark, the stops
      it lives at, over the hero and on paper), judge it as a designer -- modern, premium, minimalist, immersive, the motion right
      -- fix and capture again until it is good. The PlayMode tests are written against the ACCEPTED design, never before it:
      a test suite run on a look that is about to change is wasted time.
   4. **Test:** EditMode for the pure rules (fast, any time), then the PlayMode tests that pin the details -- real input, edge
      cases, motion, contrast as seen -- then the block's tier.
   Stop when the plan is fully verified; don't keep polishing past it.
3. **Teach as you go:** after each meaningful step (a feature, a test, a debug session), leave a short plain-language "learning summary" of what changed and why -- like a developer's own notes, not a report. The durable handoff is the 60-finishing.md §6.4 chat summary.