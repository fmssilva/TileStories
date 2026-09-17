## 1. **Mode & Intent Disambiguation**

The workflow alternates strictly between two operating modes:
- **PLAN MODE:** Analyzing requirements, auditing code, drafting options, and building execution roadmaps.
- **ACT MODE:** Modifying files, running builds, executing tests, and verifying behavior.

### **Mode Sync Protocol**
To prevent drift across context compaction, summarizations, or long task executions:
* **Initial & Recovery Check:** At the start of a task or immediately following any context summarization/compaction, read `proj_guides/__mode.md` from the workspace root to confirm active mode and the current command that should be executed. 
* **Mode Boundary:** Do not perform file modifications while in **PLAN MODE**. Do not redesign core architecture while in **ACT MODE** without returning to PLAN MODE first.

---

## 2. Shared Core Rules

### Architectural & Benchmark Standards
* **Academic Reference Quality:** Write code as if this project is a reference implementation for learning. If existing code is messy or incorrect, do **not** patch around it with backward-compatibility shims, wrappers, or adapters. Propose a clean refactor, obtain agreement, and fix it properly.
* **Patience Over Speed:** Quality and clarity supersede execution speed. Unverified, messy, or duplicate code is unacceptable.
* **Stop & Clarify:** If existing code is ambiguous, contradictory, or appears wrong, halt immediately. Ask or flag the issue instead of building on assumptions.

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