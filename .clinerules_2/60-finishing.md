## 6. Finishing a Task

This file's actual content had been overwritten with a duplicate copy of
`40-testing.md`'s content at some point before 2026-08-07 -- reconstructed
here from the finishing-related instructions that were still being given
correctly elsewhere (task templates, chat instructions) even while this
file itself was wrong. If anything below conflicts with a clearer memory of
what this file used to say, the clearer memory wins -- flag the discrepancy
rather than silently trusting this reconstruction as final.

### 6.1 Verify before reporting anything done

Do not report a task as finished on the strength of the code looking right
or a log line saying success. Before saying something is done:

- Compile check: batch-mode compile log, zero `error CS` lines (`40-testing.md` §4.2).
- If the change touched an asset file rather than only going through
  Unity's own Editor UI: confirm the AssetDatabase refresh actually
  happened (`40-testing.md` §4.3), not just that the file on disk changed.
- Re-run the actual EditMode/PlayMode tests relevant to the change and read
  the result XML directly -- don't infer a pass from "no errors were
  printed." If the task added or changed a test, confirm that specific
  test's name appears in the results, not just that the total count went up.
- If the task involved a claim from an earlier session or a different
  agent ("this was already implemented," "the config is already correct"),
  that claim gets verified against the actual current files before being
  repeated as fact -- never forwarded unchecked (`00-process.md` §1's
  evidence-discipline rule, applied at the finish line, not just the start).

### 6.2 Update the plan file itself, in place

- If a `_N.M_DomainX_Design.md` (or similar) plan file exists for this
  work, update its own Implementation Status block and/or the specific
  section's checklist to reflect what was actually verified -- immediately,
  in that same file, before moving to the next task. Never a separate
  progress-notes file, never batched to the end of a longer session.
- Check off only what was actually re-verified this session, per §6.1 --
  not what a plan file already claimed before this session started.
- If a bug is found and fixed, the fix's write-up goes in that same plan
  file, in a numbered corrections section -- not a new document.
- If the plan file has grown large and most of a domain's work is now
  done, that's a signal to archive the completed narrative out to a
  companion `_ARCHIVE.md` file per `50-terminal_and_tools.md`'s
  "Plan/Design Doc Hygiene" rule -- do this as part of finishing, not as a
  separately-scheduled cleanup task that may never happen.

### 6.3 Chat summary

- End with a short summary in chat: files touched, what changed, and the
  resulting behaviour. Don't create a separate summary file for this --
  the plan file update in §6.2 is the durable record; the chat message is
  for the person reading right now.
- State plainly what was and wasn't independently re-verified this
  session, per §6.1 -- don't blur "I re-ran this and confirmed it" together
  with "the plan file already said this was done."

### 6.4 Before ending the session

- Remind about a git commit if meaningful file changes were made and
  haven't been committed -- don't assume it happens automatically or that
  it's out of scope to mention.
- If anything was left genuinely open (not just "could be improved later,"
  but something the task was supposed to cover and didn't fully reach),
  say so explicitly in the chat summary rather than letting a partial
  result read as complete.
