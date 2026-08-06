## 1. Before You Start Any Task

**Evidence discipline, stated once here because it applies to every single task in
this document**: never report a rendering/visual/behavioral task as passing without
pointing to a specific mechanical result that proves it — an `Assert` that actually
passed (raw output shown, not paraphrased), or a specific, itemized observation
citing exactly what was seen. "No errors were thrown" is not evidence of
correctness. A previous agent's summary is not evidence of correctness. Only
independently re-derived evidence is. See §6.5 for the full protocol — this rule is
why it exists.


- Read the current project plan / phase plan document in full before writing anything.
- Read every existing file relevant to the task before touching it. Never assume you know
  what a file does from its name — open it and read it.
- Search the project for existing types, classes, ScriptableObjects, or helper methods
  that already do part of what you're about to build. Extend or reuse them instead of
  writing a duplicate.
- Write a comprehensive TODO list of every step the task requires, including test steps,
  before starting implementation. Do not skip steps later because the list didn't include
  them — write the list completely up front.
- Take your time. Quality over speed. There is no reward for finishing fast if the result
  is messy, duplicated, or untested.

---

## 2. Think Before You Code

For every non-trivial feature or fix, stop and reason at two separate levels before
opening a file to edit:

- **Architecture level.** Where does this thing live? Which folder, which assembly, is it
  shared framework code or specific to one wall/app? Write out three distinct structural
  options with their trade-offs, then pick the cleanest one. State the choice and the
  reason in your response before implementing.
- **Implementation level.** How is this actually written, locally, in code? Write out
  three distinct concrete approaches (e.g. event-driven vs. direct method call,
  ScriptableObject-driven vs. hardcoded, composition vs. inheritance), then pick the most
  robust and simplest one.

**Benchmark standard:** write this project as if it were an academic reference
implementation that other students will read to learn from. If an existing type, class,
or system is messy or wrong, do not patch around it with an adapter, a wrapper, or a
backward-compatibility shim. Stop, propose the structural fix as three options, get
agreement, and rewrite it properly. Shortcuts that leave messy code in place because
"it still works" are not acceptable.

---


## 3. Writing Code

- Create the actual file and write its content into it directly. Do not draft a large
  file in your own working memory and dump it all at once at the end — write it as you
  go, so that if the session is interrupted partway through, the work already done is not
  lost.

---


## 4. General Operating Principles

- There is no time pressure that justifies messy or untested code. Be thorough, not fast.
- Every task starts with a complete TODO list, including its own testing steps, written
  before implementation begins — not assembled retroactively once code already exists.
- When something about the existing code is unclear or looks wrong, stop and ask or flag
  it rather than guessing and building on top of an assumption that might be incorrect.


