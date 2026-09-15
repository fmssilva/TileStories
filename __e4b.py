# -*- coding: utf-8 -*-
# E4b: remove the obsolete post-field key block (old chain -> plain MouseDown if).
import io

path = r"C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Editor\POIEditor\SpecificMarker\POIEditorToolWindow.SpecificMarker.cs"
with io.open(path, "r", encoding="utf-8-sig", newline="") as f:
    content = f.read()

nl = "\r\n"
old = (
    "                    // Handle Enter/Escape to exit edit mode" + nl +
    "                    var e = Event.current;" + nl +
    "                    // Gate on the keys, not focus name: the TextField can consume" + nl +
    "                    // the first Return (releasing focus) before this runs, which" + nl +
    "                    // used to swallow the first Enter and force a second press." + nl +
    "                    if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter || e.keyCode == KeyCode.Escape))" + nl +
    "                    {" + nl +
    "                        if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)" + nl +
    "                        {" + nl +
    "                            // Save on Enter - strips the \"N. \" numbering prefix." + nl +
    "                            var textToSave = SessionState.GetString(editValueKey, newName);" + nl +
    "                            SaveNameChange(poi, foldoutKey, editModeKey, editValueKey, textToSave);" + nl +
    "                            e.Use();" + nl +
    "                        }" + nl +
    "                        else if (e.keyCode == KeyCode.Escape)" + nl +
    "                        {" + nl +
    "                            // Cancel on Escape -- discard edits, keep the old name." + nl +
    "                            SessionState.SetBool(editModeKey, false);" + nl +
    "                            SessionState.EraseString(editValueKey);" + nl +
    "                            SessionState.EraseString(editValueKey + \"_focused\");" + nl +
    "                            GUI.FocusControl(null);" + nl +
    "                            e.Use();" + nl +
    "                        }" + nl +
    "                    }" + nl +
    "                    else if (e.type == EventType.MouseDown && !editRect.Contains(e.mousePosition))"
)
new = (
    "                    // Click outside the field commits the rename."
)
n = content.count(old)
if n != 1:
    raise SystemExit(f"FAIL E4b: found {n}")
content = content.replace(old, new)
with io.open(path, "w", encoding="utf-8", newline="") as f:
    f.write(content)
print("E4b-OK")