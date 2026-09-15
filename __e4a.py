# -*- coding: utf-8 -*-
# E4a: insert pre-field Enter/Escape resolution. Deleted after use.
import io

path = r"C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Editor\POIEditor\SpecificMarker\POIEditorToolWindow.SpecificMarker.cs"
with io.open(path, "r", encoding="utf-8-sig", newline="") as f:
    content = f.read()

nl = "\r\n"
old = (
    "                    string editValueKey = $\"editValue_{foldoutKey}\";" + nl +
    "                    GUI.SetNextControlName(fieldRectName);"
)
new = (
    "                    string editValueKey = $\"editValue_{foldoutKey}\";" + nl +
    "                    var e = Event.current;" + nl +
    "" + nl +
    "                    // Resolve Enter/Escape BEFORE the TextField draws. Unity's" + nl +
    "                    // TextField consumes the first Return it sees (commit +" + nl +
    "                    // keyboard-focus release), turning the event into" + nl +
    "                    // EventType.Used before any post-field check could see it --" + nl +
    "                    // the root cause of the \"press Enter twice\" bug. Consuming" + nl +
    "                    // the event pre-field is deterministic; the decision table" + nl +
    "                    // lives in PoiRenameKeys (pure, Tier-0 tested)." + nl +
    "                    var keyAction = PoiRenameKeys.Resolve(e.type, e.keyCode);" + nl +
    "                    if (keyAction == PoiRenameKeys.Action.Commit)" + nl +
    "                    {" + nl +
    "                        // The draft mirror is current as of the last keystroke pass." + nl +
    "                        SaveNameChange(poi, foldoutKey, editModeKey, editValueKey," + nl +
    "                            SessionState.GetString(editValueKey, displayName));" + nl +
    "                        e.Use();" + nl +
    "                    }" + nl +
    "                    else if (keyAction == PoiRenameKeys.Action.Cancel)" + nl +
    "                    {" + nl +
    "                        // Cancel on Escape -- discard edits, keep the old name." + nl +
    "                        SessionState.SetBool(editModeKey, false);" + nl +
    "                        SessionState.EraseString(editValueKey);" + nl +
    "                        SessionState.EraseString(editValueKey + \"_focused\");" + nl +
    "                        GUI.FocusControl(null);" + nl +
    "                        e.Use();" + nl +
    "                    }" + nl +
    "" + nl +
    "                    GUI.SetNextControlName(fieldRectName);"
)
n = content.count(old)
if n != 1:
    raise SystemExit(f"FAIL E4a: found {n}")
content = content.replace(old, new)
with io.open(path, "w", encoding="utf-8", newline="") as f:
    f.write(content)
print("E4a-OK")