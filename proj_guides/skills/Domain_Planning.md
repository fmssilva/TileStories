# SKILL: Domain Planning

1. **Name:** "plan the <domain> domain" -- turn an idea into a domain guide a Worker can build from without this chat.
2. **Trigger:** before a new domain is built, or when an existing plan should be re-checked (e.g. `_3.2` / `_3.3` before they
   start). Input: the domain name, `_0_work_plan.md`, any existing guide / prototype / design board for it. If the domain or the
   work plan is missing, STOP and ask.
3. **Procedure** (planning only -- no code in this chat):
   1. **Ground.** Read `.clinerules/` (all lines), `10-structure.md`, `_0_work_plan.md`, the existing docs of the domain and its
      neighbours. Say what is already decided, built, and planned for this domain. Flag now: does it have a visual part (Phase A /
      B tests, `40-testing.md` 4.4) and an Editor-time tool touching runtime objects (Edit-Mode parity, 4.4.1)?
   2. **Explore (repeat as needed, discuss, don't build).** The developer's ideas + research: how other products and the
      literature solve it, build-vs-library options (packages, repos) with trade-offs, what fits a mobile AR framework for ANY
      heritage wall. Save real citations as you go (this is a thesis project). Propose valuable ideas the developer did not ask
      for. Every behaviour a wall may want different becomes an Editor option (`00-process.md`, Framework Completeness).
   3. **Write the guide** `_x.y_<Domain>.md`: TODOs, a status table of small steps (block-sized), WHAT / HOW / WHERE / WHY per
      step, concrete code only where it de-risks, the config ontology (every field with its Editor label, default, (i)), the
      Phase A / B order and the verification tier + pass criterion per step, references. Also patch `_0_work_plan.md`, `_5.1`,
      `10-structure.md` or other guides when the plan changes them, and say what changed.
   4. **Verify, twice, and fix in the file:** Pass 1 -- against the chat: every feature or option discussed is in the guide as a
      real Editor-configurable field or a named step, nothing silently dropped or left as vague "future work". Pass 2 -- against
      the guide itself: every cross-reference points at the right place, every described behaviour has a mechanism (a formula, an
      owner, an order of calls) traced through one concrete example, no "resolve this later" hedges. Then re-read once more for
      consistency after the fixes.
4. **Result:** the guide file on disk, the list of other files patched (with what changed), the citations added, the open
   decisions for the developer (options + recommendation), and the first block's Worker brief. Never hand over a guide that
   failed Pass 1 or 2.
