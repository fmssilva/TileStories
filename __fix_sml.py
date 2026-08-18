path = 'Assets/Framework/Editor/POIAuthoring/SpecificMarker/POIAuthoringToolWindow.SpecificMarker.cs'
raw = open(path, 'rb').read()
bom = raw[:3] == b'\xef\xbb\xbf'
body = raw[3:] if bom else raw
le = '\r\n' if b'\r\n' in body else '\n'
lines = body.decode('utf-8').split(le)
prefix = 'expanded = EditorGUILayout.Foldout(expanded,'
suffix = 'CreateFoldoutStyle(PoiHeaderColorFor(foldoutKey, i)));'
want = 16
fixed = 0
for i, ln in enumerate(lines):
    st = ln.lstrip(' ')
    if st.startswith(prefix) and st.endswith(suffix):
        cur = len(ln) - len(st)
        if cur != want:
            lines[i] = (' ' * want) + st
            print('FIXED line', i + 1, 'spaces', cur, '->', want)
            fixed += 1
out = le.join(lines).encode('utf-8')
open(path, 'wb').write((b'\xef\xbb\xbf' if bom else b'') + out)
print('fixed:', fixed)