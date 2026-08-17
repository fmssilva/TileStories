#!/usr/bin/env python3
"""
edit_file.py -- reliable text-replace helper for the workspace.

Usage:
    python edit_file.py <absolute_path_to_target_file>

Reads search text from __old.txt and replacement text from __new.txt
(both at workspace root). Performs one assert-anchored replacement on
the target file. Preserves BOM and encoding (utf-8-sig vs utf-8).

FALLBACK: use this when the `editor` tool cannot reproduce the exact
`old_text` whitespace (the indentation-doubling bug described in 5.3).
For all other cases, prefer the `editor` tool -- one call, no indirection.
"""
import sys
import os

WORKSPACE_ROOT = os.path.dirname(os.path.abspath(__file__))
OLD_FILE = os.path.join(WORKSPACE_ROOT, "__old.txt")
NEW_FILE = os.path.join(WORKSPACE_ROOT, "__new.txt")


def decode_text(path):
    """Read a file and decode it, auto-detecting UTF-8/UTF-16 BOM."""
    with open(path, "rb") as f:
        raw = f.read()
    if raw.startswith(b"\xef\xbb\xbf"):
        return raw[3:].decode("utf-8")      # UTF-8 with BOM
    if raw.startswith(b"\xff\xfe"):
        return raw.decode("utf-16-le")       # UTF-16 LE (PowerShell Set-Content)
    if raw.startswith(b"\xfe\xff"):
        return raw.decode("utf-16-be")       # UTF-16 BE
    try:
        return raw.decode("utf-8")           # UTF-8 no BOM
    except UnicodeDecodeError:
        return raw.decode("utf-8", errors="replace")


def strip_trailing_nl(text):
    """Strip exactly one trailing newline (CRLF or LF)."""
    if text.endswith("\r\n"):
        return text[:-2]
    if text.endswith("\n"):
        return text[:-1]
    return text


def main():
    if len(sys.argv) < 2:
        print("USAGE: python edit_file.py <path_to_target_file>")
        sys.exit(1)

    target = os.path.normpath(sys.argv[1])

    if not os.path.isfile(OLD_FILE):
        print(f"FAIL: {OLD_FILE} not found -- write the old text there first")
        sys.exit(1)
    if not os.path.isfile(NEW_FILE):
        print(f"FAIL: {NEW_FILE} not found -- write the new text there first")
        sys.exit(1)

    # Read old/new text, auto-detecting encoding (UTF-8 or UTF-16
    # from PowerShell Set-Content). Strip a single trailing newline
    # so CRLF-vs-LF mismatches from different write tools don't cause
    # false "not found" errors.
    old_text = decode_text(OLD_FILE)
    new_text = decode_text(NEW_FILE)
    old_text = strip_trailing_nl(old_text)
    new_text = strip_trailing_nl(new_text)

    # Detect BOM in the TARGET file (not the txt files).
    with open(target, "rb") as f:
        first3 = f.read(3)
    has_bom = first3 == b"\xef\xbb\xbf"
    encoding = "utf-8-sig" if has_bom else "utf-8"

    # Read target with newline="" to preserve CRLF/LF exactly.
    with open(target, "r", encoding=encoding, newline="") as f:
        content = f.read()

    count = content.count(old_text)
    if count == 0:
        preview = old_text[:80].replace("\n", "\\n")
        print(f"FAIL: old_text not found in {target}")
        print(f"  searched for: {preview!r}")
        sys.exit(1)
    if count > 1:
        print(f"FAIL: old_text found {count} times in {target} (want exactly 1)")
        print(f"  first 80 chars: {old_text[:80]!r}")
        sys.exit(1)

    content = content.replace(old_text, new_text, 1)

    with open(target, "w", encoding=encoding, newline="") as f:
        f.write(content)

    print(f"[ok] {target}: 1 replacement applied ({len(old_text)} chars -> {len(new_text)} chars)")


if __name__ == "__main__":
    main()
