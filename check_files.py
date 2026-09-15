#!/usr/bin/env python3
import os
import sys

proj = r"C:\Users\franc\Desktop\TileStories"
files_to_check = [
    os.path.join(proj, "proj_guides", "_1.2__Marker_Positioning_future_notes.md"),
    os.path.join(proj, "proj_guides", "_5.1_Editor_Tab.md"),
    os.path.join(proj, "proj_guides", "_2.7_Corrections_TODO.md"),
    os.path.join(proj, "proj_guides", "_0_work_plan.md"),
    os.path.join(proj, "proj_guides", "_1-1_Base_AR_status.md"),
    os.path.join(proj, "proj_guides", "__curr_plan_tracker.md"),
    os.path.join(proj, "TileStories", "Assets", "Framework", "Editor", "POIEditor", "POIEditorToolWindow.RigLifecycle.cs"),
]
for f in files_to_check:
    exists = os.path.exists(f)
    size = os.path.getsize(f) if exists else 0
    status = "EXISTS" if exists else "MISSING"
    print(f"{status} | {size:>10} bytes | {os.path.basename(f)}")
    if exists and size > 0:
        print(f"   full path: {f}")
