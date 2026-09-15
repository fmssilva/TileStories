#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Step 4: Rename remaining top-level files and update all content."""
import os
import sys

EDITOR_DIR = r"C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Editor"
TESTS_DIR = r"C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Tests\Editor"
ROOT_DIR = r"C:\Users\franc\Desktop\TileStories"

def rename_file(old_name, new_name, base_dir):
    old_path = os.path.join(base_dir, old_name)
    new_path = os.path.join(base_dir, new_name)
    if os.path.isfile(old_path):
        if os.path.exists(new_path):
            print(f"  SKIP (exists): {new_name}")
            return False
        os.rename(old_path, new_path)
        print(f"  OK: {old_name} -> {new_name}")
        return True
    return False

def update_content(path, replacements):
    """Replace strings in a file. Returns True if changes made."""
    if not os.path.isfile(path):
        return False
    try:
        with open(path, 'r', encoding='utf-8-sig') as f:
            content = f.read()
    except Exception as e:
        print(f"  ERROR reading {path}: {e}")
        return False
    
    original = content
    for old, new in replacements:
        content = content.replace(old, new)
    
    if content != original:
        with open(path, 'w', encoding='utf-8') as f:
            f.write(content)
        return True
    return False

# Replacements to apply in content
CONTENT_REPLACEMENTS = [
    ('"TileStories/POI Authoring/Rig Safety Prompt on Play/Build"', '"TileStories/POI Editor/Rig Safety Prompt on Play/Build"'),
    ('"TileStories/POI Authoring Tool #P"', '"TileStories/POI Editor #P"'),
    ("\"POI Authoring Tool\"", "\"POI Editor\""),
    ("'POI Authoring Tool'", "'POI Editor'"),
    ('"POI Authoring Rig"', '"POI Editor Rig"'),
    ("'POI Authoring Rig'", "'POI Editor Rig'"),
    ('"POI Authoring"', '"POI Editor"'),
    ("'POI Authoring'", "'POI Editor'"),
    ('POIAuthoringToolWindow.', 'POIEditorToolWindow.'),
    ('POIAuthoringToolWindow', 'POIEditorToolWindow'),
    ('POIAuthoringRig', 'POIEditorRig'),
    ('POIAuthoring', 'POIEditor'),
    ('POI Authoring Tool', 'POI Editor'),
    ('POI Authoring Rig', 'POI Editor Rig'),
    ('POI Authoring', 'POI Editor'),
    ('POI authoring', 'POI editor'),  # lowercase variant
]

print("=== Step 4: Rename remaining top-level files ===")

# Rename the two remaining files in Editor/
rename_file("POIAuthoringRigBuildCheck.cs", "POIEditorRigBuildCheck.cs", EDITOR_DIR)
rename_file("POIAuthoringRigBuildCheck.cs.meta", "POIEditorRigBuildCheck.cs.meta", EDITOR_DIR)
rename_file("POIAuthoringRigSafetyCheck.cs", "POIEditorRigSafetyCheck.cs", EDITOR_DIR)
rename_file("POIAuthoringRigSafetyCheck.cs.meta", "POIEditorRigSafetyCheck.cs.meta", EDITOR_DIR)

print("\n=== Step 5: Update content in Editor/ files ===")
editor_cs_files = []
for f in os.listdir(EDITOR_DIR):
    if f.endswith('.cs'):
        editor_cs_files.append(os.path.join(EDITOR_DIR, f))

# Also include files in POIEditor subdirs that might still have old references
for root, dirs, files in os.walk(EDITOR_DIR):
    for f in files:
        if f.endswith('.cs'):
            full = os.path.join(root, f)
            if full not in editor_cs_files:
                editor_cs_files.append(full)

changed = 0
for path in editor_cs_files:
    if update_content(path, CONTENT_REPLACEMENTS):
        changed += 1
        print(f"  CHANGED: {os.path.basename(path)}")

print(f"\nEditor files changed: {changed}")

print("\n=== Step 6: Update content in test files ===")
test_cs_files = []
for f in os.listdir(TESTS_DIR):
    if f.endswith('.cs'):
        test_cs_files.append(os.path.join(TESTS_DIR, f))

changed = 0
for path in test_cs_files:
    if update_content(path, CONTENT_REPLACEMENTS):
        changed += 1
        print(f"  CHANGED: {os.path.basename(path)}")

print(f"\nTest files changed: {changed}")

print("\n=== Checking for remaining POIAuthoring references ===")
remaining = []
for root, dirs, files in os.walk(ROOT_DIR):
    rel = os.path.relpath(root, ROOT_DIR)
    if rel.startswith('Library') or rel.startswith('Packages') or rel.startswith('Temp') or rel.startswith('obj'):
        continue
    for f in files:
        if f.endswith('.cs'):
            path = os.path.join(root, f)
            try:
                with open(path, 'r', encoding='utf-8-sig') as fh:
                    content = fh.read()
                if 'POIAuthoring' in content or 'POI Authoring' in content:
                    remaining.append(path)
            except:
                pass

if remaining:
    print(f"\nWARNING: {len(remaining)} files still contain POIAuthoring/POI Authoring:")
    for p in remaining:
        print(f"  {os.path.relpath(p, ROOT_DIR)}")
        try:
            with open(p, 'r', encoding='utf-8-sig') as fh:
                for i, line in enumerate(fh, 1):
                    if 'POIAuthoring' in line or 'POI Authoring' in line:
                        print(f"    L{i}: {line.rstrip()}")
        except:
            pass
else:
    print("No remaining references to POIAuthoring/POI Authoring in .cs files!")

print("\nDone.")