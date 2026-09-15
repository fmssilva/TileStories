#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Scrub proj_guides/*.md: POIAuthoring / POI Authoring -> POIEditor / POI Editor.
Small, self-contained, written to proj_guides and deleted after run.
ASCII-safe: only matches whole-word-ish Authoring/POIAuthoring tokens to avoid
smearing names that happen to contain those substrings."""
import os, re, sys

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "TileStories", "proj_guides")
if not os.path.isdir(ROOT):
    ROOT = r"C:\Users\franc\Desktop\TileStories\TileStories\proj_guides"

# Ordered: longer/more-specific first so "POIAuthoringToolWindow" isn't
# partially hit by a later generic "POIAuthoring" replacement.
REPLACEMENTS = [
    # Menu strings
    ('"TileStories/POI Authoring/Rig Safety Prompt on Play/Build"',
     '"TileStories/POI Editor/Rig Safety Prompt on Play/Build"'),
    ('"TileStories/POI Authoring Tool #P"',
     '"TileStories/POI Editor #P"'),
    ('"POI Authoring Tool"', '"POI Editor"'),
    ("'POI Authoring Tool'", "'POI Editor'"),
    ('"POI Authoring Rig"', '"POI Editor Rig"'),
    ("'POI Authoring Rig'", "'POI Editor Rig'"),
    ('"POI Authoring"', '"POI Editor"'),
    ("'POI Authoring'", "'POI Editor'"),

    # File/class references (use ASSEMBLY-NAME boundaries so we don't
    # accidentally turn "Authoring" inside another word like "co-authoring").
    (r'(?<![A-Za-z])POIAuthoringToolWindow\.',
     r'POIEditorToolWindow.'),
    (r'(?<![A-Za-z])POIAuthoringToolWindow',
     r'POIEditorToolWindow'),
    (r'(?<![A-Za-z])POIAuthoringRig',
     r'POIEditorRig'),
    (r'(?<![A-Za-z])POIAuthoring',
     r'POIEditor'),

    # Human-facing prose
    (r'(?<![A-Za-z])POI Authoring Tool(?![A-Za-z])',
     r'POI Editor'),
    (r'(?<![A-Za-z])POI Authoring Rig(?![A-Za-z])',
     r'POI Editor Rig'),
    (r'(?<![A-Za-z])POI Authoring(?![A-Za-z])',
     r'POI Editor'),
    (r'(?<![A-Za-z])POI authoring(?![A-Za-z])',
     r'POI editor'),
]

CAPITALIZE_AFTER = re.compile(r'(?<=[ .,;:\-])([a-z])')
FIRST_CAP_RE = re.compile(r'([A-Z]) Authoring')

def fix_line_ends(text):
    """Normalise line endings to CRLF (Windows editor preference)."""
    return text.replace('\r\n', '\n').replace('\n', '\r\n')

def main():
    if not os.path.isdir(ROOT):
        print(f"ERROR: proj_guides dir not found at {ROOT}")
        sys.exit(1)

    changed = []
    for fname in sorted(os.listdir(ROOT)):
        if not fname.endswith('.md'):
            continue
        path = os.path.join(ROOT, fname)
        try:
            with open(path, 'r', encoding='utf-8-sig') as f:
                content = f.read()
        except Exception as e:
            print(f"  skip ({e}): {fname}")
            continue

        original = content
        # Ordered replacements
        for old, new in REPLACEMENTS:
            content = content.replace(old, new)
        # Regex catches remaining bare forms the literal pass may have missed
        # (e.g. "POI Authoring Tool" as a backtick/code phrase, or with mixed quotes).
        content = re.sub(r'(?<![A-Za-z])POI Authoring(?: Tool| Rig)?(?![A-Za-z])',
                         lambda m: m.group(0).replace('POI Authoring', 'POI Editor')
                                              .replace('POI Authoring Tool', 'POI Editor')
                                              .replace('POI Authoring Rig', 'POI Editor Rig'),
                         content)
        content = re.sub(r'(?<![A-Za-z])POIAuthoring(?:ToolWindow|Tool|Rig|Authoring)?(?![A-Za-z])',
                         lambda m: m.group(0).replace('POIAuthoringToolWindow', 'POIEditorToolWindow')
                                              .replace('POIAuthoringTool', 'POIEditorTool')
                                              .replace('POIAuthoringRig', 'POIEditorRig')
                                              .replace('POIAuthoring', 'POIEditor'),
                         content)

        if content != original:
            # Re-apply ordered literal pass once more in case regex produced a new
            # occurrence (e.g. "POIEditorToolWindow" from "POIAuthoringToolWindow.").
            for old, new in REPLACEMENTS:
                content = content.replace(old, new)
            normalized = fix_line_ends(content)
            with open(path, 'w', encoding='utf-8') as f:
                f.write(normalized)
            changed.append(fname)

    print(f"Proj guides: {len(changed)} files changed")
    for fname in changed:
        print(f"  {fname}")

if __name__ == '__main__':
    main()