#!/usr/bin/env python3
"""Fix stale CapturedPosition reference in _1-1_Base_AR_status.md"""
import os, re
path = r"C:\Users\franc\Desktop\TileStories\proj_guides\_1-1_Base_AR_status.md"
with open(path, 'r', encoding='utf-8') as f:
    content = f.read()
old = "- WallConfigData + POIData + CapturedPosition classes."
new = "- WallConfigData + POIData (with `PositionData position` + `position_verified`; simplified model, no CapturedPosition class; `null` position = origin fallback; position_verified is editor-only QA)."
count = content.count(old)
print(f"Found {count} occurrences")
if count:
    content = content.replace(old, new)
    with open(path, 'w', encoding='utf-8') as f:
        f.write(content)
    print("SUCCESS")
else:
    print("FAILED: not found")
    for m in re.finditer(r'.{0,30}CapturedPosition.{0,30}', content):
        print(f"Context: {repr(m.group())}")