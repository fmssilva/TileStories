## 5. Terminal Usage (PowerShell)

- Chain commands with `;`, not `&&`.
- Do not pipe command output through filters (`| findstr`, `| Select-String`, etc.)
  directly in the terminal. Redirect output to a log file instead
  (`> __out.txt 2>&1`), then read the file. This keeps the full, unfiltered output
  available if something unexpected needs investigating.

---