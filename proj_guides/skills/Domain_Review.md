# SKILL: Domain Review

1. **Name:** "review the <domain> domain" -- a close-out audit, then fixes.
2. **Trigger:** a domain's status table is all done, or a domain needs a health check. Input: the DOMAIN GUIDE (+ its
   _Vision_Tests / _Human_Tests if any), optional FOCUS / SKIP, and in FIX mode the fix-list chunk. If the domain guide is missing
   or unclear, STOP and ask.
3. **Procedure:** AUDIT mode (sections below, a report only), then FIX mode per chunk. Worker, Opus-class.
4. **Result:** `proj_guides/_<domain>.1_Audit.md` with all sections below (none skipped without a reason) and a fix list in
   block-sized chunks; the chat finish listed at the end. FIX mode: the chunk built, tested, captured, docs updated.

## How to call it (paste into the Worker's `## And now` slot)

```markdown
## `And now: Domain Review -- <DOMAIN NAME>, mode <AUDIT | FIX>`
Run proj_guides/skills/Domain_Review.md with:
- DOMAIN GUIDE: proj_guides/<_x.y_Domain>.md   (+ its _Vision_Tests / _Human_Tests files if any)
- FOCUS (optional): <extra questions or areas for this domain, e.g. "the keyword ontology", "the demo grid">
- SKIP (optional): <sections not relevant to this domain>
- FIX LIST (FIX mode only): <the chunk of the audit's fix list to implement>
```

## Rules for the session

- Shared rules: `.clinerules/` (all files, all lines). Session procedure: `__AI_worker.md` (GATE, keep going, summaries).
- Required reading: the DOMAIN GUIDE (all lines), `_5.1_Editor_Tab.md` "HOW TO USE THIS FILE" + section "0. Guidelines",
  `.clinerules/40-testing.md` 4.2, 4.2.3, 4.2.4b, 4.5. Check the real code; the docs are claims, not proof.
- **AUDIT mode writes no features.** Allowed: running tests, captures, the report, and fixes the brief names explicitly.
- **FIX mode** implements only the fix-list chunk named in the call, with its tests and captures.
- Test plan: full EditMode + PlayMode at the START of an audit (counts, times and slow fixtures are audit data).

## AUDIT: what the report covers (sections of `_<domain>.1_Audit.md`)

1. **Developer flow.** The normal sequence of actions in the Editor and in Play Mode for this domain, step by step: what a
   developer sets first, what a new item (POI, row, block) gets by default, and whether defaults should be copied from a
   reference item. Where the flow is awkward, say so.
2. **Option coverage matrix.** One row per feature x variant x field / choice option (include domain-wide settings). Columns:
   Editor row | (i) help | default (per wall / per item) | undo-redo-save round trip | live sync Scene / Play Mode | demo or
   gallery entry | real-scene test | capture. Mark each cell ok / gap / n.a. Build it from the code (definitions, settings
   classes, tests), say how, and name every gap. This answers "can a developer choose AND test every option in the Editor?".
3. **Missing features.** What a complete framework for this domain should offer that is not built: compare the domain guide
   (incl. its deferred list), the design canvas, similar products, and what wall apps would need. For each: value (framework /
   app), cost, and recommend now / later / no. Also features that exist in code but have no Editor control.
4. **Config ontology.** Are the settings grouped by one responsibility each, with clear names, in an order that matches how a
   developer thinks? Propose regroupings / renames where it is muddled.
5. **Editor quality.** The domain's Editor sections against `_5.1` section 0, captured at default width and 620 pt: rows and
   widths, the (i) column, tables, popups, warnings, fields shown that do not apply to the chosen option, the Test sub-foldout
   (Scene / Play Mode / Device guides: app-agnostic, naming Editor controls not code).
6. **Demo and testability.** Can a developer SEE every option working (demo grid, gallery, fixture POIs)? Is every demo control
   itself tested and captured? What demo would make the domain easy to try?
7. **Code health.** Files over ~300 lines, duplicated concepts, dead code, needless complexity, rule scans (literals, visitor
   strings, ASCII), owners and their release paths, dependency direction (Framework never names an App).
8. **Test health.** Full-run counts and times, the 10 slowest fixtures, flaky or focus-dependent tests, tests that only prove a
   mock, options with no test.
9. **Product pass.** The domain seen as a visitor sees it (e.g. the demo POI's card scrolled end to end at 390 px, EN and PT),
   judged with the 4.5 design questions: does it read, is it the right size, does it look finished, what would confuse.
10. **Docs vs code.** The domain guide, `10-structure.md`, `_5.1`; then a triage of every open TODO of the domain: fix now
    (which chunk) / later (which domain or stage) / drop (why).
11. **Fix list.** Prioritised MUST / SHOULD / LATER, grouped into block-sized chunks (one owner OR 2 heavy OR 4-6 light items),
    each with its acceptance tests and captures.

Plus FOCUS items from the call, as their own sections.

## Finish (in chat, English)

GATE items, suites + counts + times, the matrix headline (gaps per column), the top 5 findings, the missing-feature
recommendations, what could not be verified, a plain summary of what the domain does and how to test each option, and one
short natural commit message. Commit only if the developer asks.
