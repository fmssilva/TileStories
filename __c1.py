# -*- coding: utf-8 -*-
# Constants.cs: Focus-in-Scene help body (ASCII only). Deleted after use.
import io

path = r"C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Editor\POIEditor\POIEditorToolWindow.Constants.cs"
with io.open(path, "r", encoding="utf-8-sig", newline="") as f:
    content = f.read()

nl = "\r\n"
old = "        internal const string EditIconAssetPath = \"Assets/Framework/Editor/POIEditor/SpecificMarker/Icons/edit-icon.png\";"
new = old + nl + nl + (
    "        // Focus-in-Scene help (per-POI header row, (i) button right of the Focus button)." + nl +
    "        internal static readonly string FocusInSceneHelpBody =" + nl +
    "            \"Selects this POI in the Hierarchy and frames the Scene view camera on it. \" +" + nl +
    "            \"Move the marker with Unity's Move tool (toolbar or W key), then press Capture Position in the Position foldout to store the coordinates. \" +" + nl +
    "            \"Scene camera: right-drag or Alt+left-drag orbits, Alt+Ctrl+left-drag pans, mouse wheel zooms.\";"
)
n = content.count(old)
if n != 1:
    raise SystemExit(f"FAIL C1: found {n}")
content = content.replace(old, new)
with io.open(path, "w", encoding="utf-8", newline="") as f:
    f.write(content)
print("C1-OK")