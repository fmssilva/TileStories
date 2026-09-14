#!/usr/bin/env python3
"""Fix stale captured_position reference in _0_work_plan.md"""
import os, re
path = r"C:\Users\franc\Desktop\TileStories\proj_guides\_0_work_plan.md"
with open(path, 'r', encoding='utf-8') as f:
    content = f.read()

old = "(e.g. `captured_position: {x, y, z}`), separate from `x_norm`/`y_norm` (which stay"
new = "(a `Vector3` field; `POIData.position` in the simplified 2026-09-13 model), replacing any prior placeholder or null. No separate `captured_position` field, no `x_norm`/`y_norm` pair, and no calibration-anchor interpolation step is involved — those belonged to the earlier position model that was replaced on 2026-09-13."

count = content.count(old)
print(f"Found {count} occurrences of old text in _0_work_plan.md")
if count > 0:
    content = content.replace(old, new)
    with open(path, 'w', encoding='utf-8') as f:
        f.write(content)
    print(f"SUCCESS: replaced {count} occurrence(s)")
else:
    print("FAILED: old text not found - trying to find similar text")
    for m in re.finditer(r'.{0,30}captured_position: \{x, y, z\}.{0,30}', content):
        print(f"  Context: {repr(m.group())}")

print(f"File size: {os.path.getsize(path)} bytes")