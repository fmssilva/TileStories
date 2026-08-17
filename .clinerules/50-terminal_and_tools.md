## 5.1. Terminal Usage (PowerShell)

- Chain commands with `;`, not `&&`.
- Do not pipe command output through filters (`| findstr`, `| Select-String`, etc.)
  directly in the terminal. Redirect output to a log file instead
  (`> __out.txt 2>&1`), then read the file. This keeps the full, unfiltered output
  available if something unexpected needs investigating.

---

## 5.2. Tool Usage Guidelines

### Tool Execution & Argument Validation
- **Strict Parameter Compliance:** Always verify tool schemas before executing. Never emit missing required parameters (e.g., `path` for `read_file` or `write_to_file`; or 'regex' for `search_files`).
- **File Reading Limits:** Do NOT attempt to read entire large files directly with `read_file`.
  - For target edits/searches, use `search_files` or `codebase_search` first.
  - When using `read_file` on large files, always specify `start_line` and `end_line` ranges.
- **Error Handling & Pivot Strategy:**
  - If a tool execution fails or returns an error response, analyze the error output immediately.
  - Do NOT repeat the exact same failing tool call with identical arguments.
  - If a tool continuously fails, pivot to an alternative tool or execute a shell command via `execute_command` (e.g., fallback file inspection).


## 5.3. File Editing Discipline 

**Primary tool: the `editor` tool (direct file edit, one step). Python is fallback for edge cases.**

### How to edit a file (the fast, smooth way)
The `editor` tool writes directly to the file in one call. No Python script file to create, run, or
delete. This is the default for every edit. Know its one quirk and you never need Python indirection.

1. **For Create a new file**: omit `old_text` (or pass empty string). Provide `new_text` with the full
   file content. Done in one call -- zero matching, zero whitespace issues. This is the fastest path
   and works perfectly every time.

2. **For Replace text in an existing file**: 
   - `old_text` must match EXACTLY, **including ALL leading whitespace ON EVERY matched line**. So, copy the line verbatim from a `read_files` output into `old_text`. 
   - **Critical bug to avoid**: if `old_text` omits the line's leading whitespace (starts
   at a non-whitespace character mid-line), the tool PREPENDS the original line's indentation to
   the replacement text. This causes indentation doubling (12 spaces -> 24 -> 36). So ALWAYS include the full line with its indentation in `old_text` to prevent this.

3. **Column-precise edits**: use `unityMCP__apply_text_edits` with exact line/col coordinates and
   the current `precondition_sha256` (from `unityMCP__get_sha`). This is immune to the whitespace
   doubling bug because it operates on line/column ranges, not text matching. Drawback: requires
   the file SHA to match (fetch a fresh SHA if the file changed). Use this when you cannot
   reliably reproduce the exact `old_text` (e.g., whitespace you can't count).

4. **Verify after every edit**: re-read the edited region with `read_files`. If it returns
   "outdated" (stale cache after an external write), fall back to `unityMCP__get_sha` or read
   via PowerShell `gc <path>`. Then `refresh_unity` (force compile, zero `error CS`) and
   `run_tests` (EditMode + PlayMode, zero failures) to confirm.

5. **Complex multi-edit**: if exact `old_text` matching is too fragile (e.g., you can't
   determine exact whitespace), write a small `__edit.py` and run it. Prefer `apply_text_edits`
   or the `editor` tool with full-line `old_text` over Python whenever possible. See the
   Fallback section below for the pre-made helper.

   > Never use inline `python -c` with nested quotes or `$_ $()` -- the command bridge strips
   > them and corrupts string literals. Write a script file instead.

### One-line mental model
Edit = READ region verbatim -> assert-anchored single replacement -> WRITE back unchanged encoding
-> re-READ to verify -> run the relevant test suite. Never rewrite a file from memory.

### Fallback: `edit_file.py` (when `old_text` whitespace can't be matched exactly)
A persistent helper at the workspace root. Write `__old.txt` (search text) and `__new.txt`
(replacement), then run `python edit_file.py <path> > __out.txt 2>&1` and read `__out.txt`.
It does exact substring matching (no whitespace counting needed), auto-detects BOM/encoding,
strips a trailing newline to avoid CRLF mismatches, and asserts exactly one match (fails loud
if 0 or 2+). Delete the two `.txt` files after.

What agents forget by heart: old_text must be unique; new_text >= 3000 chars -> split into
two sequential edits; if the assert fails with "found N times", the anchor is ambiguous --
stop, re-read, do NOT force it.

### Alternative: coordinate-based edits via `unityMCP__apply_text_edits`
Use when the `editor` tool cannot match exact whitespace. Takes explicit line/col ranges
(whitespace-exact, immune to the doubling bug) -- but column counting is error-prone, so
re-read the exact range first. `unityMCP__script_apply_edits` handles whole-method/class
changes by name. Verify with a re-read + tests.

### Read rules (unchanged, but re-emphasized)
- Always `read_files` with start_line/end_line; never dump a whole big file.
- Copy anchors verbatim from that read -- never from memory or an earlier plan.
- If a read looks stale after an edit, re-read via a fresh path before trusting it.

### Verify after every edit (the un-skippable gate)
- Re-read the edited region. Balanced braces/parens, correct indentation.
- Then `refresh_unity` -> ZERO `error CS`; `run_tests` EditMode + PlayMode -> ZERO failures.
- Touched a `.prefab`/`.asset`/texture? Trigger AssetDatabase refresh (Rule 40 sec.4.3)
  before reporting done.

### Temp-file hygiene
- All scratch files prefixed `__` and deleted after use. 
