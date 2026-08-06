## 5.1. Terminal Usage (PowerShell)

- Chain commands with `;`, not `&&`.
- Do not pipe command output through filters (`| findstr`, `| Select-String`, etc.)
  directly in the terminal. Redirect output to a log file instead
  (`> __out.txt 2>&1`), then read the file. This keeps the full, unfiltered output
  available if something unexpected needs investigating.

---

## 5.2. Tool & Context Usage Guidelines

### Tool Execution & Argument Validation
- **Strict Parameter Compliance:** Always verify tool schemas before executing. Never emit missing required parameters (e.g., `path` for `read_file` or `write_to_file`; or 'regex' for `search_files`).
- **File Reading Limits:** Do NOT attempt to read entire large files directly with `read_file`.
  - For target edits/searches, use `search_files` or `codebase_search` first.
  - When using `read_file` on large files, always specify `start_line` and `end_line` ranges.
- **Error Handling & Pivot Strategy:**
  - If a tool execution fails or returns an error response, analyze the error output immediately.
  - Do NOT repeat the exact same failing tool call with identical arguments.
  - If a tool continuously fails, pivot to an alternative tool or execute a shell command via `execute_command` (e.g., fallback file inspection).

