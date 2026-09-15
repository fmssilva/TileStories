#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Step 3: Update content in all .cs files (replace POIAuthoring/POI Authoring with POIEditor)."""
import os
import sys

EDITOR_DIR = r"C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Editor\POIEditor"
TESTS_DIR = r"C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Tests\Editor"
ROOT_DIR = r"C:\Users\franc\Desktop\TileStories"

# Patterns to replace (order matters: more specific first, then general)
CONTENT_REPLACEMENTS = [
    # Menu and UI strings
    ('"TileStories/POI Authoring/Rig Safety Prompt on Play/Build"', '"TileStories/POI Editor/Rig Safety Prompt on Play/Build"'),
    ('"TileStories/POI Authoring Tool #P"', '"TileStories/POI Editor #P"'),
    ("\"POI Authoring Tool\"", "\"POI Editor\""),
    ("'POI Authoring Tool'", "'POI Editor'"),
    ('"POI Authoring Rig"', '"POI Editor Rig"'),
    ("'POI Authoring Rig'", "'POI Editor Rig'"),
    ('"POI Authoring"', '"POI Editor"'),
    ("'POI Authoring'", "'POI Editor'"),

    # Class name references in code
    ('POIAuthoringToolWindow.', 'POIEditorToolWindow.'),
    ('POIAuthoringToolWindow', 'POIEditorToolWindow'),
    ('POIAuthoringRig', 'POIEditorRig'),
    ('POIAuthoring', 'POIEditor'),

    # Comment/doc strings
    ('POI Authoring Tool', 'POI Editor'),
    ('POI Authoring Rig', 'POI Editor Rig'),
    ('POI Authoring', 'POI Editor'),
]

def process_file(path):
    """Process a single .cs file. Returns True if changes were made."""
    if not os.path.isfile(path):
        return False
    
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

def main():
    print("=== Updating content in .cs files ===")
    
    # Collect all .cs files to process
    cs_files = []
    
    # Files in POIEditor/
    for root, dirs, files in os.walk(EDITOR_DIR):
        for f in files:
            if f.endswith('.cs'):
                cs_files.append(os.path.join(root, f))
    
    # Test files
    for f in os.listdir(TESTS_DIR):
        if f.endswith('.cs'):
            cs_files.append(os.path.join(TESTS_DIR, f))
    
    # Also check root dir for any remaining files (e.g., old top-level files that weren't moved)
    for f in os.listdir(EDITOR_DIR):
        if f.endswith('.cs'):
            full = os.path.join(EDITOR_DIR, f)
            if os.path.isfile(full) and full not in cs_files:
                cs_files.append(full)
    
    changed_count = 0
    for path in cs_files:
        if process_file(path):
            changed_count += 1
            print(f"  CHANGED: {os.path.basename(path)}")
    
    print(f"\nFiles changed: {changed_count}/{len(cs_files)}")
    
    # Also check for any remaining POIAuthoring in other .cs files in the project
    print("\n=== Checking for remaining POIAuthoring references ===")
    remaining = []
    for root, dirs, files in os.walk(ROOT_DIR):
        # Skip Library, Packages, Temp, obj
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
        print(f"\nWARNING: {len(remaining)} .cs files still contain POIAuthoring references:")
        for p in remaining:
            print(f"  {os.path.relpath(p, ROOT_DIR)}")
        
        # Show lines
        print("\nLines with references:")
        for p in remaining:
            try:
                with open(p, 'r', encoding='utf-8-sig') as fh:
                    for i, line in enumerate(fh, 1):
                        if 'POIAuthoring' in line or 'POI Authoring' in line:
                            print(f"  {p}:{i}: {line.rstrip()}")
            except:
                pass
    else:
        print("No remaining POIAuthoring references found!")
    
    print("\nDone.")

if __name__ == "__main__":
    main()