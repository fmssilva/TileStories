# Skills -- procedures used at specific moments

A skill stores a job that repeats. The everyday rules live elsewhere (shared rules in `.clinerules/`, the Worker's session
procedure in `__AI_worker.md`, the Architect's in `__AI_Architect.md`); a skill adds only the steps for one kind of moment and
points to those rules instead of copying them.

| Skill | Trigger (when + input) | Who | Result |
|---|---|---|---|
| `Domain_Planning.md` | Before building a new domain (or re-planning one): domain name, work plan, prior docs | Architect chat | The domain guide `_x.y_<Domain>.md`, verified twice |
| `Domain_Review.md` | A domain's status table is all done, or it needs a health check: domain guide, FOCUS / SKIP | Worker, Opus-class | `_x.y.1_Audit.md` + fix list; then FIX chunks |
| `Guidelines_Review.md` | Monthly, after a tool / Unity / package upgrade, or every ~10 blocks | Architect chat | A dated change list for the guides, applied after approval |
| `End_Of_Task_Guidelines_Review.md` | End of a long chat: the chat itself | any agent | Proposed process lessons, awaiting approval |

Last guidelines review: none yet (write the date here after each `Guidelines_Review.md` run).

## Skill anatomy (every skill file starts with these four lines)

1. **Name** -- what you would naturally say ("review the card domain").
2. **Trigger** -- when it applies and which input it needs.
3. **Procedure** -- short steps.
4. **Result** -- the format, the limits, what must never be missing.

The Result line is what makes it a skill and not a saved prompt: without it every run aims at a different target. A good skill
also refuses: if an essential input is missing, it asks for it instead of inventing it.

## How to use one
Paste its "How to call it" block (or name it in the chat) and fill the inputs. To adapt it to one case, add FOCUS lines in the
call; edit the skill file only when the change helps every future use.

## Adding a skill (keep it small)
The four-line header, a "How to call it" block, the steps, the result. No rules that already live in `.clinerules/` or the agent
files. Add one row to the table above.
