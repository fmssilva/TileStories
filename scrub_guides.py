#!/usr/bin/env python3
import os, sys

ROOT = r"C:\Users\franc\Desktop\TileStories\TileStories\proj_guides"
OLD = "POIAuthoringToolWindow"
NEW = "POIEditorToolWindow"

count = 0
for root, dirs, files in os.walk(ROOT):
    for f in files:
        if not f.endswith('.md'):
            continue
        path = os.path.join(root, f)
        with open(path, 'r', encoding='utf-8-sig') as fh:
            content = fh.read()
        if OLD in content or "POI Authoring" in content:
            content = content.replace(OLD, NEW)
            content = content.replace("POIAuthoring", "POIEditor")
            content = content.replace("POI Authoring Tool", "POI Editor")
            content = content.replace("POI Authoring Rig", "POI Editor Rig")
            content = content.replace("POI Authoring", "POI Editor")
            content = content.replace("POI authoring", "POI editor")
            with open(path, 'w', encoding='utf-8') as fh:
                fh.write(content)
            print(f"  scrubbed: {f}")
            count += 1
print(f"\nDone: {count} .md files scrubbed")