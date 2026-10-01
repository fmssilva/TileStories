# SKILL: Guidelines Review

1. **Name:** "review the guidelines" -- keep the rules true, short and in one place.
2. **Trigger:** about monthly, right after a Unity / package / tool upgrade (MCP, Claude Code, glTFast, Input System...), or
   every ~10 blocks. Input: the workspace (all of `.clinerules/`, `proj_guides/__AI_*.md`, `proj_guides/skills/`, `_5.1` section
   0, `__mixed_TODOs.md`) and the date of the last review (`skills/README.md`). If the workspace is not readable, STOP and ask.
3. **Procedure:**
   1. **Inventory.** List every rule file with its size; flag files or sections that grew without adding new rules (history
      logs, repeated examples, narrative of past sessions).
   2. **One home per rule.** Find rules stated in two or more places; keep the best home (shared -> `.clinerules`, Architect ->
      `__AI_Architect.md`, Worker -> `__AI_worker.md`, moment -> a skill, domain -> its guide) and replace the others with a
      pointer. Find contradictions; decide which is right.
   3. **Re-test the workarounds.** For every rule that exists because a tool was hard (capture recipes, batch-mode fallbacks,
      focus / layout workarounds, MCP quirks, git-from-the-VM steps): is it still needed? Re-run the smallest check that proves
      it (one command, one capture, one test) and record the result. A workaround that is no longer needed is deleted; one that
      changed is rewritten.
   4. **Still followed?** Sample the last ~5 Worker summaries and Architect reviews: rules nobody follows are unclear, too long,
      or wrong -- fix the rule, not the people.
   5. **Token weight.** Estimate what an agent must read per session; cut what does not change behaviour (history, rationale
      already in git, examples that repeat the rule).
4. **Result:** a short dated change list -- for each item: file + section, ADD / MODIFY / REMOVE / MOVE, the exact new text (or
   the pointer that replaces it), the evidence (the re-test result or the duplicate locations), and the token saving. Nothing is
   applied before the developer approves; after approval, apply it, write the date in `skills/README.md`, commit
   ("Tidy the project guidelines"). If nothing needs to change, say so in one line.
