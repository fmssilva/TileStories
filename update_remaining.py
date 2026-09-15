#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Update remaining .cs and .md files with POIEditor replacements."""
import os
import sys

ROOT_DIR = r"C:\Users\franc\Desktop\TileStories"
EDITOR_DIR = os.path.join(ROOT_DIR, "TileStories", "Assets", "Framework", "Editor")
TESTS_DIR = os.path.join(ROOT_DIR, "TileStories", "Assets", "Framework", "Tests", "Editor")

CONTENT_REPLACEMENTS = [
    ('"TileStories/POI Authoring/Rig Safety Prompt on Play/Build"', '"TileStories/POI Editor/Rig Safety Prompt on Play/Build"'),
    ('"TileStories/POI Authoring Tool #P"', '"TileStories/POI Editor #P"'),
    ('"POI Authoring Tool"', '"POI Editor"'),
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
    ('POI authoring', 'POI editor'),
]

def process_file(path):
    """Replace strings in a file. Returns True if changes made."""
    try:
        with open(path, 'r', encoding='utf-8-sig') as f:
            content = f.read()
    except Exception as e:
        print(f"  ERROR reading {path}: {e}")
        return False
    
    original = content
    for old, new in CONTENT_REPLACEMENTS:
        content = content.replace(old, new)
    
    if content != original:
        with open(path, 'w', encoding='utf-8') as f:
            f.write(content)
        return True
    return False

print("=== Updating test files ===")
for f in os.listdir(TESTS_DIR):
    if f.endswith('.cs'):
        path = os.path.join(TESTS_DIR, f)
        if os.path.isfile(path):
            if process_file(path):
                print(f"  CHANGED: {f}")

print("\n=== Updating .md files ===")
md_changed = 0
for root, dirs, files in os.walk(os.path.join(ROOT_DIR, "TileStories", "proj_guides")):
    for f in files:
        if f.endswith('.md'):
            path = os.path.join(root, f)
            if process_file(path):
                md_changed += 1
                print(f"  CHANGED: {os.path.relpath(path, os.path.join(ROOT_DIR, 'TileStories'))}")

print(f"\n.md files changed: {md_changed}")

print("\n=== Final verification ===")
remaining = []
for root, dirs, files in os.walk(os.path.join(ROOT_DIR, "TileStories")):
    rel = os.path.relpath(root, ROOT_DIR)
    if any(rel.startswith(prefix) for prefix in ['Library', 'Packages', 'Temp', 'obj', '.git']):
        continue
    for f in files:
        if f.endswith('.cs') or f.endswith('.md'):
            path = os.path.join(root, f)
            try:
                with open(path, 'r', encoding='utf-8-sig') as fh:
                    content = fh.read()
                if 'POIAuthoring' in content or 'POI Authoring' in content:
                    remaining.append((path, content))
            except:
                pass

if remaining:
    print(f"\n{len(remaining)} files still have POIAuthoring/POI Authoring:")
    for path, content in remaining:
        print(f"\n  {os.path.relpath(path, ROOT_DIR)}:")
        for i, line in enumerate(content.split('\n'), 1):
            if 'POIAuthoring' in line or 'POI Authoring' in line:
                print(f"    L{i}: {line.rstrip()}")
else:
    print("SUCCESS: No remaining POIAuthoring/POI Authoring references found!")

print("\nDone.")