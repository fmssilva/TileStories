## 1. Roles and Modes

Two agents share these rules:
- **Architect** (a review chat): verifies the last block on disk, decides, keeps the guides in sync, commits, and writes the next
  block's brief. Its own procedure: `proj_guides/__AI_Architect.md`. It does not write code.
- **Worker** (a fresh Claude Code session per block): plans the block, then builds it. Its own procedure: `proj_guides/__AI_worker.md`
  with the brief pasted into it. Moment-specific procedures: `proj_guides/skills/`.

Both work in two modes, always in this order:
- **PLAN MODE:** read, audit the code on disk, weigh options, write the plan. No file modifications except the plan tracker.
- **ACT MODE:** modify files, build, test, look, verify. No redesign of core architecture without going back to PLAN MODE.

After a context compaction, re-read the brief and `proj_guides/__curr_plan_tracker.md` (the Worker's plan for the current block)
before continuing.

---

## 2. Shared Core Rules

### Architectural & Benchmark Standards
* **Academic Reference Quality:** Write code as if this project is a reference implementation for learning. If existing code is messy or incorrect, do **not** patch around it with backward-compatibility shims, wrappers, or adapters. Propose a clean refactor, obtain agreement, and fix it properly.
* **Patience Over Speed:** Quality and clarity supersede execution speed. Unverified, messy, or duplicate code is unacceptable.
* **Stop & Clarify:** If existing code is ambiguous, contradictory, or appears wrong, halt immediately. Ask or flag the issue instead of building on assumptions.

### Framework Completeness
* **A complete, versatile framework, configured in the Editor.** Every feature a domain should have is built, and every
  behaviour a wall developer may want different is a choice in the POI Editor: a row with an (i) help, a sensible default (per
  wall, and per block / item where it makes sense), undo / redo / save, live sync in Scene and Play Mode, and a Test guide
  (Scene / Play Mode / Device) that names Editor controls, never code, and is app-agnostic. A behaviour that is a trade-off
  becomes options with a default, each option tested; one hard-coded rule only where one answer is right for every wall.
* **Real fixtures.** Tests run on the LivingRoom wall's POI sets (The Lamp's 12+ POIs, The Painting, The Camera); add a POI
  when an edge case needs one, never change an existing POI's identity fields (id, name, category, summary, keywords).

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
2. **Deep audit + baseline:** open the files that actually exist for this task; a prior
   "we built X / X doesn't exist" claim is unverified until seen on disk. Find gaps, typos,
   and misalignments with the rules. Confirm the project compiles and the existing tests
   pass BEFORE changing anything; if the tooling to do that is unavailable, stop and say so.
3. **Architecture Level (where it lives):** map folder/assembly/component scope, weigh 3 distinct structural options with trade-offs, state the choice and why.
4. **Implementation Level (how it works):** weigh 3 concrete options (event-driven vs direct call, ScriptableObject vs hardcoded, ...), pick the simplest robust one.
5. **Execution plan + decisions:** itemized TODO incl. testing steps. Separate the choices you made autonomously (clear winner) from any real trade-off needing the developer's pick -- block only on the latter; otherwise present the plan and proceed to ACT.

### ACT MODE
1. **Strict scope lock:** implement exactly the planned task; no unsolicited refactor, formatting cleanup, or extra features.
2. **Incremental + verified:** write only the files the step needs, then per step: implement -> add/run the tests that genuinely exercise it -> compile + run tests -> read the mechanical result. Stop when the plan is fully verified; don't keep polishing past it.
3. **Teach as you go:** after each meaningful step (a feature, a test, a debug session), leave a short plain-language "learning summary" of what changed and why -- like a developer's own notes, not a report. The durable handoff is the 60-finishing.md §6.4 chat summary.