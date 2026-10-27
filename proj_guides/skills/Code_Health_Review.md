# SKILL: Code Health Review

1. **Name:** "review the code health" -- keep the project's code simple, in its place and easy to learn from.
2. **Trigger:** the maintenance cadence (`__AI_Architect.md` section 4: every ~10 blocks, and before a domain close-out). Input:
   the repo, `.clinerules/` and the blocks since the last review (git log). If the tree does not compile, STOP and say so.
3. **Procedure (Worker, Opus-class; AUDIT first -- a report only -- then FIX chunks):** read the code the period touched, and
   sample the rest; judge every file against the criteria below; for each finding give the file, what is wrong, the simplest fix,
   and the risk.
4. **Result:** `proj_guides/_Code_Health_<n>.md` = the findings by criterion + a fix list in block-sized chunks (largest value
   first). FIX mode: one chunk built, behaviour unchanged (the suites green), docs updated.

## The criteria (a teacher showing students how it is done)

- **In its place.** The right layer and folder (Framework vs App, Runtime vs Editor, pure rule vs view vs host, the render
  targets of `10-structure.md`); dependencies point one way; one concept lives in one place.
- **No dead code.** Unused types, members, parameters, branches, settings nobody reads, commented-out code, leftover debug logs.
- **No duplication.** Two pieces doing the same job -> one, the better one, used by both (search before keeping a copy).
- **No speculative code.** No "might need it later" options, hooks, interfaces with one implementation, generic layers for one
  case. Build what a real feature uses today; the next feature can add what it needs.
- **No over-engineering.** The simplest structure that is correct; a class, an abstraction or a pattern earns its place by
  removing more complexity than it adds.
- **Readable.** Names say what a thing is; small methods with one job; files that fit in a reader's head (split a file that
  mixes jobs, not one that is merely long).
- **Comments like one coder to another.** Say WHY and what is not obvious, in plain words; no narration of the code, no history
  (dates, steps, "was / now"), no comment that the code already says.
- **The project's rules.** No literal colours / sizes / visitor words in code (tokens, string tables); Editor rows complete
  (`_5.1` section 0); no backward-compatibility shims; the guards (render target, token, field) green.
- **Tests are code too.** The same criteria for test code: shared helpers instead of copies, names that say what is proven.
