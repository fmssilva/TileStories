#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Step 7: Update all .md guide files and verify no remaining references."""
import os

ROOT_DIR = r"C:\Users\franc\Desktop\TileStories"

# Content replacements for .md files
MD_REPLACEMENTS = [
    ('POIAuthoringToolWindow.', 'POIEditorToolWindow.'),
    ('POIAuthoringToolWindow', 'POIEditorToolWindow'),
    ('POIAuthoringRig', 'POIEditorRig'),
    ('POIAuthoring', 'POIEditor'),
    ('"POI Authoring Tool"', '"POI Editor"'),
    ("'POI Authoring Tool'", "'POI Editor'"),
    ('POI Authoring Tool', 'POI Editor'),
    ('POI Authoring Rig', 'POI Editor Rig'),
    ('POI Authoring', 'POI Editor'),
    ('POI authoring', 'POI editor'),
]

def update_md_file(path):
    """Update a single .md file. Returns True if changes made."""
    try:
        with open(path, 'r', encoding='utf-8-sig') as f:
            content = f.read()
    except Exception as e:
        print(f"  ERROR reading {path}: {e}")
        return False
    
    original = content
    for old, new in MD_REPLACEMENTS:
        content = content.replace(old, new)
    
    if content != original:
        with open(path, 'w', encoding='utf-8') as f:
            f.write(content)
        return True
    return False

print("=== Updating .md files ===")
md_changed = 0
md_checked = 0
for root, dirs, files in os.walk(ROOT_DIR):
    # Skip Library, Packages, Temp, obj, .git
    rel = os.path.relpath(root, ROOT_DIR)
    if any(rel.startswith(prefix) for prefix in ['Library', 'Packages', 'Temp', 'obj', '.git', 'Node_modules']):
        continue
    for f in files:
        if f.endswith('.md'):
            path = os.path.join(root, f)
            md_checked += 1
            if update_md_file(path):
                md_changed += 1
                print(f"  CHANGED: {os.path.relpath(path, ROOT_DIR)}")

print(f"\n.md files checked: {md_checked}")
print(f".md files changed: {md_changed}")

# Final verification pass
print("\n=== Final verification ===")
import re

remaining_cs = []
remaining_md = []

for root, dirs, files in os.walk(ROOT_DIR):
    rel = os.path.relpath(root, ROOT_DIR)
    if any(rel.startswith(prefix) for prefix in ['Library', 'Packages', 'Temp', 'obj', '.git', 'Node_modules']):
        continue
    for f in files:
        path = os.path.join(root, f)
        try:
            with open(path, 'r', encoding='utf-8-sig') as fh:
                content = fh.read()
            if 'POIAuthoring' in content or 'POI Authoring' in content:
                if f.endswith('.cs'):
                    remaining_cs.append((path, content))
                elif f.endswith('.md'):
                    remaining_md.append((path, content))
        except:
            pass

if remaining_cs:
    print(f"\nWARNING: {len(remaining_cs)} .cs files still have POIAuthoring/POI Authoring:")
    for path, content in remaining_cs:
        print(f"\n  {os.path.relpath(path, ROOT_DIR)}:")
        for i, line in enumerate(content.split('\n'), 1):
            if 'POIAuthoring' in line or 'POI Authoring' in line:
                print(f"    L{i}: {line.rstrip()}")
else:
    print("No remaining POIAuthoring/POI Authoring references in .cs files!")

if remaining_md:
    print(f"\n{len(remaining_md)} .md files still have POIAuthoring/POI Authoring (may be intentional in history):")
    # Just count, don't show all lines for .md files
    for path, content in remaining_md:
        pattern = re.compile(r'POIAuthoring|POI Authoring')
        count = len(pattern.findall(content))
        print(f"  {os.path.relpath(path, ROOT_DIR)}: {count} occurrences")
else:
    print("No remaining references in .md files.")

print("\nDone.")