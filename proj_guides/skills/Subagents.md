# SKILL: Delegating to subagents

1. **Name:** "hand this to a subagent" / "fan this out". It sends one self-contained piece of work to a fresh agent (the `Agent` tool)
   and brings back only its conclusion.
2. **Trigger:** any session, when a task passes the test in `.clinerules/00-process.md` "Delegating to subagents" (that section holds
   the rules: when, which rung, verification, Unity's one owner). Input: the task, the paths it needs, the answer's shape. If the task
   cannot be said in one paragraph with its paths, it is not ready: do it inline or split it first.
3. **Procedure:**
   1. **Decide.** Delegate when the answer fits in ~40 lines and the work would otherwise fill your context: many files, web pages,
      long logs, or N independent items checked against the same rule. Keep inline:
      - one coherent artifact being built (one file, one page, one class): splitting it costs more than it saves;
      - anything that needs the developer, a commit or a guide edit;
      - anything quicker to do than to brief.
   2. **Pick the rung** (`00-process.md`; the ceiling is Opus + medium):

      | Task | Model + effort |
      |---|---|
      | Find, list, pull failures out of a log | Haiku + low (`Explore` when read-only) |
      | Web research, an audit against one rule, a draft, mechanical edits from an exact spec | Sonnet + medium |
      | Compare options for a closed problem, one isolated unit with its tests | Opus + medium |
   3. **Write the brief.** A fresh agent sees nothing of your chat; a `fork` sees all of it but costs your context. Every brief has:
      - `GOAL:` one sentence, and why it matters;
      - `READ:` the exact paths, plus the rule sections the task needs (never the whole session procedure);
      - `DO NOT:` edit (for research), run Unity, commit, use git, widen the scope;
      - `RETURN:` the verdict first, then the evidence as `file:line` or a URL for every claim, under N lines.
   4. **Fan out only independent pieces:** one agent per research angle, per file group or per rule; never two agents on the same file.
      - **Pictures:** save them to the scratchpad and pass their paths. An agent sees only files.
      - **Visual reviews:** give the capture paths and the questions to answer (contrast, overlap, what looks broken), not "look at it".
   5. **Check before you use it** (Evidence Discipline):
      - Code: read the diff, then compile and run the tests yourself.
      - Research: open the 2-3 claims the decision rests on. A report with no URLs is memory: mark it unverified or re-run it.
      - A weak result: re-run once, one rung higher. Never patch it inline into a conclusion.
4. **Result:** the conclusion in your own work, checked; the report's Subagents line (`60-finishing.md`) names each agent's task,
   model, effort and whether its result held up.

## How to call it
"Use a subagent for <task>: <paths>, return <shape>." Or name the rung: "a Sonnet medium pass over <files> against <rule>".

## Lessons so far
- A research agent that ran no web lookups wrote from memory with confident numbers. Ask for a URL per claim, and check that the
  report has them.
- A long self-test that rendered every view per control ran for hours in a headless browser. Give an agent a time budget and a
  cheaper check (compare strings, not screens) before it runs anything long.
