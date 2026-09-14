#!/usr/bin/env python3
"""Fix stale line 76 -> 96 in _2.7_Corrections_TODO.md"""
import os, re
path = r"C:\Users\franc\Desktop\TileStories\proj_guides\_2.7_Corrections_TODO.md"
with open(path, 'r', encoding='utf-8') as f:
    content = f.read()
old = "line 76, `internal bool IsRigInSyncWithConfig(out int outOfSyncCount)`"
new = "line 96, `internal bool IsRigInSyncWithConfig(out int outOfSyncCount)`"
count = content.count(old)
print(f"Found {count} occurrences")
if count:
    content = content.replace(old, new)
    with open(path, 'w', encoding='utf-8') as f:
        f.write(content)
    print("SUCCESS")
else:
    print("FAILED: not found")
    for m in re.finditer(r'.{0,30}line 76.{0,30}', content):
        print(f"Context: {repr(m.group())}")