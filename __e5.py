# -*- coding: utf-8 -*-
# E5: HelpInfo button right of the Focus button (+10f gap). Deleted after use.
import io

path = r"C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Editor\POIEditor\SpecificMarker\POIEditorToolWindow.SpecificMarker.cs"
with io.open(path, "r", encoding="utf-8-sig", newline="") as f:
    content = f.read()

nl = "\r\n"
old = (
    "                    using (new EditorGUI.DisabledScope(!CanFocusPoiInScene(poi)))" + nl +
    "                    {" + nl +
    "                        Color poiColor = PoiHeaderColorFor(foldoutKey, i);" + nl +
    "                        var prevBg = GUI.backgroundColor;" + nl +
    "                        GUI.backgroundColor = poiColor;" + nl +
    "                        if (GUILayout.Button(\"Focus in Scene\", GUILayout.Width(140f)))" + nl +
    "                            FocusPoiInScene(poi);" + nl +
    "                        GUI.backgroundColor = prevBg;" + nl +
    "                    }"
)
new = (
    "                    using (new EditorGUILayout.HorizontalScope())" + nl +
    "                    {" + nl +
    "                        using (new EditorGUI.DisabledScope(!CanFocusPoiInScene(poi)))" + nl +
    "                        {" + nl +
    "                            Color poiColor = PoiHeaderColorFor(foldoutKey, i);" + nl +
    "                            var prevBg = GUI.backgroundColor;" + nl +
    "                            GUI.backgroundColor = poiColor;" + nl +
    "                            if (GUILayout.Button(\"Focus in Scene\", GUILayout.Width(140f)))" + nl +
    "                                FocusPoiInScene(poi);" + nl +
    "                            GUI.backgroundColor = prevBg;" + nl +
    "                        }" + nl +
    "" + nl +
    "                        GUILayout.Space(10f);" + nl +
    "                        HelpInfoButton.Draw(\"Focus in Scene\", FocusInSceneHelpBody);" + nl +
    "                    }"
)
n = content.count(old)
if n != 1:
    raise SystemExit(f"FAIL E5: found {n}")
content = content.replace(old, new)
with io.open(path, "w", encoding="utf-8", newline="") as f:
    f.write(content)
print("E5-OK")