# SKILL: Test Suite Review

1. **Name:** "review the test suite" -- keep it trustworthy and fast enough to run.
2. **Trigger:** the maintenance cadence (`__AI_Architect.md` section 4: every ~12 blocks), or when a tier outgrows its time (BLOCK
   past ~10 min, CORE past ~35, FULL past ~90). Input: the last full runs' per-fixture times (the run report / `TestRunRecorder`), `40-testing.md`, the watched flakes in
   `__mixed_TODOs.md`. If no per-fixture times exist, the first step is to produce them.
3. **Procedure (Worker, Opus-class; AUDIT first, then FIX chunks):**
   1. **Where the time goes.** Rank the fixtures by time; the top ~20 usually hold most of it.
   2. **For each slow fixture, the cheapest proof that keeps the guarantee:** a pure rule tested through a scene -> an EditMode test
      of the rule + one scene test of the wiring; a scene loaded per test -> once per fixture where tests do not change shared state;
      real-time waits -> waits on a condition; an exhaustive matrix (every look x mode x language) -> one case per behaviour plus the
      pairs that can really interact; two tests proving the same thing -> one.
   3. **Never drop:** the test of a real past bug (its regression), the contract guards (tokens, fields, render targets, contrast).
   4. **Flakes:** each watched flake gets its cause found or a quarantine (a named category, run apart, with an owner) -- never a
      silent retry.
   5. **Tiers (`40-testing.md` 4.2 states when each runs):** BLOCK = full EditMode + the block's own PlayMode fixtures, a few
      minutes; CORE = the NUnit category `Core` (the MCP run selects it with `category_names`): the critical visitor and Editor
      paths of every domain, ~30 min; FULL = everything, ~90 min at most. Keep CORE true: add a fixture that guards a new critical
      path, drop one that no longer does, and keep its time near 30 min.
4. **Result:** `proj_guides/_Test_Suite_<n>.md` = the time table before, the findings, a fix list in block-sized chunks with the
   expected saving each. FIX mode: one chunk, every guarantee kept (show what proves it now), the time after, `40-testing.md` RUN
   TIMES and tiers updated as current truth.
