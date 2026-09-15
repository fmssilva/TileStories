# -*- coding: utf-8 -*-
# Fix: E4b left an unconditional bare block (MouseDown guard was eaten).
# Restore the click-outside guard. Deleted after use.
import io

path = r"C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Editor\POIEditor\SpecificMarker\POIEditorToolWindow.SpecificMarker.cs"
with io.open(path, "r", encoding="utf-8-sig", newline="") as f:
    content = f.read()

nl = "\r\n"
old = (
    "                    // Click outside the field commits the rename." + nl +
    "                    {" + nl +
    "                        // Click outside the field commits the rename." + nl +
    "                        var textToSave = SessionState.GetString(editValueKey, newName);" + nl +
    "                        SaveNameChange(poi, foldoutKey, editModeKey, editValueKey, textToSave);" + nl +
    "                    }" + nl +
    "                }"
)
new = (
    "                    // Click outside the field commits the rename." + nl +
    "                    if (e.type == EventType.MouseDown && !editRect.Contains(e.mousePosition))" + nl +
    "                    {" + nl +
    "                        SaveNameChange(poi, foldoutKey, editModeKey, editValueKey, newName);" + nl +
    "                    }" + nl +
    "                }"
)
n = content.count(old)
if n != 1:
    raise SystemExit(f"FAIL: found {n}")
content = content.replace(old, new)
with io.open(path, "w", encoding="utf-8", newline="") as f:
    f.write(content)
print("FIX-OK")