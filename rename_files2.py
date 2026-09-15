#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Step 2: Rename files inside POIEditor/ and test files."""
import os
import shutil
import sys

EDITOR_DIR = r"C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Editor\POIEditor"
TESTS_DIR = r"C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Tests\Editor"

def rename_in_dir(base_dir, renames):
    """Rename files in a directory. renames is list of (old_name, new_name)."""
    for old, new in renames:
        old_path = os.path.join(base_dir, old)
        new_path = os.path.join(base_dir, new)
        if os.path.isfile(old_path):
            if os.path.exists(new_path):
                print(f"  SKIP (exists): {new}")
                continue
            shutil.move(old_path, new_path)
            print(f"  OK: {old} -> {new}")

def main():
    # Subfolders and their file renames
    subfolders = {
        "": [
            ("POIAuthoringToolWindow.Constants.cs", "POIEditorToolWindow.Constants.cs"),
            ("POIAuthoringToolWindow.cs", "POIEditorToolWindow.cs"),
            ("POIAuthoringToolWindow.cs.meta", "POIEditorToolWindow.cs.meta"),
        ],
        "AssetPaths": [
            ("POIAuthoringToolWindow.AssetPaths.cs", "POIEditorToolWindow.AssetPaths.cs"),
            ("POIAuthoringToolWindow.AssetPaths.cs.meta", "POIEditorToolWindow.AssetPaths.cs.meta"),
        ],
        "ConfigData": [
            ("POIAuthoringToolWindow.ConfigFileIO.cs", "POIEditorToolWindow.ConfigFileIO.cs"),
            ("POIAuthoringToolWindow.ConfigFileIO.cs.meta", "POIEditorToolWindow.ConfigFileIO.cs.meta"),
            ("POIAuthoringToolWindow.ConfigHistory.cs", "POIEditorToolWindow.ConfigHistory.cs"),
            ("POIAuthoringToolWindow.ConfigHistory.cs.meta", "POIEditorToolWindow.ConfigHistory.cs.meta"),
            ("POIAuthoringToolWindow.ConfigValidation.cs", "POIEditorToolWindow.ConfigValidation.cs"),
            ("POIAuthoringToolWindow.ConfigValidation.cs.meta", "POIEditorToolWindow.ConfigValidation.cs.meta"),
        ],
        "GlobalScene": [
            ("POIAuthoringToolWindow.Displacement.cs", "POIEditorToolWindow.Displacement.cs"),
            ("POIAuthoringToolWindow.Displacement.cs.meta", "POIEditorToolWindow.Displacement.cs.meta"),
            ("POIAuthoringToolWindow.GlobalScene.cs", "POIEditorToolWindow.GlobalScene.cs"),
            ("POIAuthoringToolWindow.GlobalScene.cs.meta", "POIEditorToolWindow.GlobalScene.cs.meta"),
            ("POIAuthoringToolWindow.LodZoom.cs", "POIEditorToolWindow.LodZoom.cs"),
            ("POIAuthoringToolWindow.LodZoom.cs.meta", "POIEditorToolWindow.LodZoom.cs.meta"),
            ("POIAuthoringToolWindow.SearchFilter.cs", "POIEditorToolWindow.SearchFilter.cs"),
            ("POIAuthoringToolWindow.SearchFilter.cs.meta", "POIEditorToolWindow.SearchFilter.cs.meta"),
        ],
        "RigLifecycle": [
            ("POIAuthoringToolWindow.RigLifecycle.cs", "POIEditorToolWindow.RigLifecycle.cs"),
            ("POIAuthoringToolWindow.RigLifecycle.cs.meta", "POIEditorToolWindow.RigLifecycle.cs.meta"),
        ],
        "Shared": [
            ("POIAuthoringToolWindow.SymbolTable.cs", "POIEditorToolWindow.SymbolTable.cs"),
            ("POIAuthoringToolWindow.SymbolTable.cs.meta", "POIEditorToolWindow.SymbolTable.cs.meta"),
        ],
        "SpecificMarker": [
            ("POIAuthoringToolWindow.PositionTabs.cs", "POIEditorToolWindow.PositionTabs.cs"),
            ("POIAuthoringToolWindow.PositionTabs.cs.meta", "POIEditorToolWindow.PositionTabs.cs.meta"),
            ("POIAuthoringToolWindow.SpecificMarker.cs", "POIEditorToolWindow.SpecificMarker.cs"),
            ("POIAuthoringToolWindow.SpecificMarker.cs.meta", "POIEditorToolWindow.SpecificMarker.cs.meta"),
        ],
    }
    
    print("=== Renaming files in POIEditor/ ===")
    for subdir, files in subfolders.items():
        path = os.path.join(EDITOR_DIR, subdir) if subdir else EDITOR_DIR
        if os.path.isdir(path):
            print(f"\n  Subfolder: {subdir or '(root)'}")
            rename_in_dir(path, files)
        else:
            print(f"  SKIP (not found): {subdir}")
    
    # Test files
    print("\n=== Renaming test files ===")
    test_renames = [
        ("POIAuthoringAddPoiTests.cs", "POIEditorAddPoiTests.cs"),
        ("POIAuthoringAddPoiTests.cs.meta", "POIEditorAddPoiTests.cs.meta"),
        ("POIAuthoringToolWriteBackTests.cs", "POIEditorToolWriteBackTests.cs"),
        ("POIAuthoringToolWriteBackTests.cs.meta", "POIEditorToolWriteBackTests.cs.meta"),
        ("POIAuthoringToolSearchRoundTripTests.cs", "POIEditorToolSearchRoundTripTests.cs"),
        ("POIAuthoringToolSearchRoundTripTests.cs.meta", "POIEditorToolSearchRoundTripTests.cs.meta"),
        ("PoiAuthoringVisualHierarchyTests.cs", "POIEditorVisualHierarchyTests.cs"),
        ("PoiAuthoringVisualHierarchyTests.cs.meta", "POIEditorVisualHierarchyTests.cs.meta"),
    ]
    rename_in_dir(TESTS_DIR, test_renames)
    
    print("\nDone.")

if __name__ == "__main__":
    main()