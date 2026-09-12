# -*- coding: utf-8 -*-
import io

path = r"c:\Users\franc\Desktop\TileStories\.clinerules\10-structure.md"
with io.open(path, "r", encoding="utf-8") as f:
    content = f.read()

# Exact old text (continuation lines have spaces, not box-drawing chars)
old = (
    "                                                   FocusPoiInScene (per-POI Focus\n"
    "                                                   in Scene button: Selection + ping +\n"
    "                                                   SceneView.FrameSelected) and\n"
    "                                                   ApplyPoiEditorRotation (Edit\n"
    "                                                   Rotation slider row:\n"
    "                                                   config-driven dev-only yaw live-\n"
    "                                                   applied to the rig child).\n"
)

new = (
    "                                                   FocusPoiInScene (per-POI Focus\n"
    "                                                   in Scene button: Selection + ping +\n"
    "                                                   SceneView.FrameSelected).\n"
    "\u2502   \u2502   \u2502   \u2502   \u2502   \u2502   \u251c\u2500\u2500\u2500 POIAuthoringToolWindow.PositionTabs.cs \u2190 DrawPositionTabs (Draft / Precise tabs inside\n"
    "\u2502   \u2502   \u2502   \u2502   \u2502   \u2502   \u2502                                                    "
    "Position foldout): Draft shows x_norm/y_norm\n"
    "\u2502   \u2502   \u2502   \u2502   \u2502   \u2502   \u2502                                                    "
    "+ Capture Position button; Precise shows\n"
    "\u2502   \u2502   \u2502   \u2502   \u2502   \u2502   \u2502                                                    "
    "captured_position read-only + Clear Capture\n"
    "\u2502   \u2502   \u2502   \u2502   \u2502   \u2502   \u2502                                                    "
    "button; validation warning when Precise without\n"
    "\u2502   \u2502   \u2502   \u2502   \u2502   \u2502   \u2502                                                    "
    "capture. Edit Rotation slider always visible\n"
    "\u2502   \u2502   \u2502   \u2502   \u2502   \u2502   \u2502                                                    "
    "below both tabs + ApplyPoiEditorRotation\n"
    "\u2502   \u2502   \u2502   \u2502   \u2502   \u2502   \u2502                                                    "
    "(config-driven dev-only yaw live-applied to\n"
    "\u2502   \u2502   \u2502   \u2502   \u2502   \u2502   \u2502                                                    "
    "the rig child). CaptureSinglePoi per-POI\n"
    "\u2502   \u2502   \u2502   \u2502   \u2502   \u2502   \u2502                                                    "
    "capture.\n"
)

if old not in content:
    raise SystemExit("OLD TEXT NOT FOUND")

content = content.replace(old, new, 1)

with io.open(path, "w", encoding="utf-8") as f:
    f.write(content)

print("OK")
