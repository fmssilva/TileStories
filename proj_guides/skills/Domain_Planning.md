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
      for. Run every candidate through the filter in `00-process.md` (Framework Completeness): option, fixed rule or dropped.
      Also check that the authoring flow is Unity's normal one.
   3. **Feature map, then one prototype per group (repeat per group, discuss, don't build).**
      - List every feature and option the domain should offer.
      - Group them by affinity into feature groups: what a developer configures together (e.g. `_4.1`: Point / Region /
        Wall Life / Label + Tap).
      - For each group, ask how a developer will use and configure it in the Editor, and which Unity tool authors its
        content.
      - List the group's fixed rules (with their reasons) next to its options.
      - Run `skills/Feature_Prototyping.md` for each group: a spec section plus an Editor Lab (an Editor mock next to a
        Scene / Play view).
      - Record every developer decision in the spec, with a date.
      - Groups that depend on another domain wait for it; note them in the TODOs.
   4. **Consolidate (after every group is prototyped).** Look at all the groups together and decide:
      - the config ontology (one JSON shape for the domain);
      - the data folders;
      - the code layout: App vs Framework, Runtime vs Editor, pure rules vs views vs owners;
      - which mechanisms are shared and which stay isolated;
      - where existing code needs a refactor first (no shims);
      - the block order.
      Prefer one simple mechanism used by several groups over one mechanism per group.
   5. **Write the guide** `_x.y_<Domain>.md`:
      - TODOs;
      - a status table of small, block-sized steps, with WHAT / HOW / WHERE / WHY per step;
      - concrete code only where it removes a risk;
      - the config ontology (every field with its Editor label, default and (i));
      - the Phase A / B order, and the verification tier and pass criterion per step;
      - references.

      The group specs and Labs become the guide's design sections, or are linked from it as design records. They are not
      copied twice. Also patch `_0_work_plan.md`, `_5.1`, `10-structure.md` or other guides when the plan changes them, and
      say what changed.
   6. **Verify, twice, and fix in the file:** Pass 1 -- against the chat: every feature or option discussed is in the guide as a
      real Editor-configurable field or a named step, nothing silently dropped or left as vague "future work". Pass 2 -- against
      the guide itself: every cross-reference points at the right place, every described behaviour has a mechanism (a formula, an
      owner, an order of calls) traced through one concrete example, no "resolve this later" hedges. Then re-read once more for
      consistency after the fixes.
4. **Result:** the guide file on disk, the Lab links and their record copies, the list of other files patched (with what changed), the citations added, the open
   decisions for the developer (options + recommendation), and the first block's Worker brief. Never hand over a guide that
   failed Pass 1 or 2.
