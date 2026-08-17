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

---

## 3. Mode-Specific Responsibilities

### PLAN MODE
For every non-trivial feature or fix, reason at two separate levels before proposing code edits:
1. **Architecture Level (Where it lives):** Map folder, assembly, and component scope. Evaluate 3 distinct structural options with trade-offs, state the choice, and explain why.
2. **Implementation Level (How it works):** Evaluate 3 concrete implementation choices (e.g., event-driven vs. direct call, ScriptableObject vs. hardcoded) and select the simplest, most robust option.
3. **Execution Plan:** Draft an itemized TODO list including testing steps before requesting transition to ACT MODE.

### ACT MODE
1. **Incremental Edits:** Write changes directly to target files step-by-step.
2. **Strict Plan Adherence:** Execute the TODO list assembled in PLAN MODE.
3. **Verification:** Produce mechanical evidence to validate each step before marking the task complete.