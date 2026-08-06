## 6. Finishing a Task

### 6.1 Verify before reporting anything done

Do not report a task as finished on the strength of the code looking right
or a log line saying success. Before saying something is done:

- Compile check: zero `error CS` lines, confirmed via Unity MCP `refresh_unity`
  (preferred) or batch-mode compile log (`40-testing.md` §4.2).
- If the change touched an asset file rather than only going through
  Unity's own Editor UI: confirm the AssetDatabase refresh actually
  happened (`40-testing.md` §4.3), not just that the file on disk changed.
- Re-run the actual EditMode/PlayMode tests relevant to the change via Unity MCP
  `run_tests` + `get_test_job` (preferred) or batch-mode XML. Acceptance gate:
  **zero failed tests** — never a fixed count. If the task added or changed a test,
  confirm that specific test's name appears in the results, not just that a count
  went up.
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
  companion archive file — do this as part of finishing, not as a
  separately-scheduled cleanup task that may never happen.
  **Archive file naming rule:** use the same naming convention as the live
  file, incrementing the minor version number (e.g. `_2.2_Marker_Design.md`
  → `_2.2.1_Marker_Design_Archive.md`, not `_2_2_Marker_Design_ARCHIVE.md`).
  Internal cross-references inside the archive must use the live file's
  current filename. Update any cross-references in the live file to point
  at the new archive filename before the session ends.


### 6.3 Update the project structure guide when files change

Any task that **creates, deletes, moves, or renames** a file anywhere under
`Assets/` must update `10-structure.md`'s tree to match before the session
ends. This is not optional and not batched to a future cleanup — the whole
value of that file is that it's accurate right now.

**Scope:** if you added a new `.cs` file, added it to the tree with a
one-sentence description. If you deleted one, remove it. If you moved or
renamed one, update its entry. If you created a whole new subfolder, add
the folder with a one-line comment describing its purpose.

**Do not** re-audit the entire tree every session. Only update the entries
that the current task actually touched. The goal is continuous accuracy
without making every agent re-verify the whole project.

The structure guide lives at `.clinerules/10-structure.md`. Update it as
the final step of §6.2's plan-file update, immediately before the §6.3
chat summary.


### 6.4 Chat summary

- End with a detailed summary in chat: files touched, what changed, and the
  resulting behaviour. Don't create a separate summary file for this --
  the plan file update in §6.2 is the durable record; the chat message is
  for the person reading right now.
- State plainly what was and wasn't independently re-verified this
  session, per §6.1 -- don't blur "I re-ran this and confirmed it" together
  with "the plan file already said this was done."



