#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Step 1: Rename the main POIAuthoring folder to POIEditor."""
import os
import shutil
import sys

EDITOR_DIR = r"C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Editor"
OLD_FOLDER = os.path.join(EDITOR_DIR, "POIAuthoring")
NEW_FOLDER = os.path.join(EDITOR_DIR, "POIEditor")

if not os.path.isdir(OLD_FOLDER):
    print(f"ERROR: Source folder not found: {OLD_FOLDER}")
    sys.exit(1)

if os.path.isdir(NEW_FOLDER):
    print(f"ERROR: Destination already exists: {NEW_FOLDER}")
    sys.exit(1)

print(f"Renaming folder: {OLD_FOLDER}")
print(f"            -> {NEW_FOLDER}")
shutil.move(OLD_FOLDER, NEW_FOLDER)
print("OK")
