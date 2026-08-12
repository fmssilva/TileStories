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

**Default tier = Python repair-script; fail over to structured scripts only if python is unavailable.**
NEVER use inline `python -c` / inline `perl -e` / inline shell with `$_ $() `` ;` or nested quotes --
the command bridge strips quotes and backslashes and corrupts string literals. That is the single
most common way edits go wrong in this workspace. If you are tempted, stop: write a script file instead.

### The one-line mental model
Edit = READ region verbatim -> assert-anchored single replacement -> WRITE back unchanged encoding
-> re-READ to verify -> run the relevant test suite. Never rewrite a file from memory.

### TIER 1 (DEFAULT): Python repair-script (robust, asserts, encoding-safe)
One logical change per script. Prefix with `__`. Delete it after editing.

1. READ the exact region with `read_files` (start_line/end_line). Copy the target line(s)
   verbatim, including ALL leading whitespace, into the script as the anchor.
2. Write `__edit.py` at the workspace ROOT. Use this template:

   import sys
   path = r"TileStories\\Assets\\Framework\\Runtime\\POI\\MarkerHierarchyResolver.cs"
   has_bom = open(path, "rb").read(3) == b"\\xef\\xbb\\xbf"
   with open(path, "r", encoding="utf-8", newline="") as f:
       s = f.read()
   OLD = "    private static int _x = 0;"      # verbatim, with indentation
   NEW = "    private static int _x = 1;"
   n = s.count(OLD)
   assert n == 1, f"anchor found {n} times (want exactly 1): {OLD!r}"
   s = s.replace(OLD, NEW, 1)
   enc = "utf-8-sig" if has_bom else "utf-8"    # preserve byte-for-byte encoding
   with open(path, "w", encoding=enc, newline="") as f:
       f.write(s)
   print("[ok]", path)

3. Run it: `python __edit.py > __out.txt 2>&1` (remember: `;` to chain, redirect to a log, never `| findstr`).
   Read `__out.txt`. A missing/ambiguous anchor must fail the assert loudly -- never a silent no-op.
4. Re-READ the edited region with `read_files` to confirm: old anchor gone, new content present,
   indentation + braces balanced.
5. For INSERTIONS: set `OLD` to a single unique line and `NEW = OLD + "\\n" + inserted_lines`, so the
   anchor line survives and line numbers below stay valid. Keep each NEW under ~3000 chars.

### TIER 2 (FALLBACK): structured/coordinate Unity MCP edit tools
Use only if python is unavailable. `unityMCP__apply_text_edits` takes explicit line/col ranges
(whitespace-exact, immune to the substring-indentation bug) -- but column counting is error-prone, so
re-read the exact range first. `unityMCP__script_apply_edits` handles whole-method/class changes by name.
Verify identically with a re-read + tests.

### Read rules (unchanged, but re-emphasized)
- Always `read_files` with start_line/end_line; never dump a whole big file.
- Copy anchors verbatim from that read -- never from memory or an earlier plan.
- If a read looks stale (e.g. a region you just edited shows old content), re-read via a fresh path
  (e.g. dump region to a temp log, then read the log) before trusting it.
- For ASCII-only search within a known file prefer `unityMCP__find_in_file`; if it rejects a dotted
  filename (`POIAuthoringToolWindow.GlobalScene.cs`) or a global search sweeps binary `Library/`, fall
  back to `read_files` with line ranges. Never trust a regex over the whole repo for a single file's layout.

### Verify after every edit (the un-skippable gate)
- Re-read the edited region. Balanced braces/parens, correct indentation.
- Then `refresh_unity` (force compile) -> ZERO `error CS`; `run_tests` EditMode + PlayMode -> ZERO failures.
- If the change touched a `.prefab`/`.asset`/texture (not just a text file), trigger an AssetDatabase
  refresh (Rule 40 §4.3) before reporting done.

### Temp-file hygiene
- All scratch files prefixed `__` and deleted after use. Keep `__TODO_work_plan.md`.
- Do NOT delete files that look like user docs (`__claude.md`, `__cline.md`, `__models.md`, `__notes.md`).

### Fail-fast
- `s.count(OLD)` != 1 -> the anchor is ambiguous/missing: stop, re-read, do NOT force the replace.
- `new_text >= 3000` chars -> split into two sequential edits.
- Never rebuild a whole section/file in one edit from memory.
- Tool fails -> read the error, pivot (don't repeat identical failing call).

