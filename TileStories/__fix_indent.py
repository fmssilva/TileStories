import sys

def read(path):
    raw = open(path, 'rb').read()
    bom = raw[:3] == b'\xef\xbb\xbf'
    if bom:
        raw = raw[3:]
    le = '\r\n' if b'\r\n' in raw else '\n'
    text = raw.decode('utf-8')
    lines = text.split(le)
    return bom, le, lines

def write(path, bom, le, lines):
    text = le.join(lines)
    raw = (b'\xef\xbb\xbf' if bom else b'') + text.encode('utf-8')
    open(path, 'wb').write(raw)

def count_leading_spaces(s):
    n = 0
    for ch in s:
        if ch == ' ':
            n += 1
        else:
            break
    return n

targets = [
    ("Assets/Framework/Editor/POIAuthoring/POIAuthoringToolWindow.Constants.cs", [
        ("// Specific Marker tab: per-POI header foldouts pick a stable color from", 8),
    ]),
    ("Assets/Framework/Editor/POIAuthoring/SpecificMarker/POIAuthoringToolWindow.SpecificMarker.cs", [
        ('expanded = EditorGUILayout.Foldout(expanded, "${i + 1}. {poi.name} ({poi.id})", true, CreateFoldoutStyle(PoiHeaderColorFor(foldoutKey, i)));', 16),
    ]),
    ("Assets/Framework/Editor/POIAuthoring/POIAuthoringToolWindow.cs", [
        ('// Tab buttons with constant base colors (Global Scene = blue, Specific Marker = green)', 12),
        ('GUI.backgroundColor = originalBgColor;', 12),
    ]),
]

changed = 0
for path, fixes in targets:
    bom, le, lines = read(path)
    fixes_by_text = {stripped: want for stripped, want in fixes}
    for i, ln in enumerate(lines):
        stripped = ln.lstrip(' ')
        if stripped in fixes_by_text:
            want = fixes_by_text[stripped]
            cur = count_leading_spaces(ln)
            if cur != want:
                old_line = ln
                lines[i] = (' ' * want) + stripped
                print("FIXED %s line %d: %d -> %d spaces" % (path, i + 1, cur, want))
                print("  WAS: [%r]" % old_line)
                print("  NOW: [%r]" % lines[i])
                changed += 1
    write(path, bom, le, lines)

print("total fixed: %d" % changed)