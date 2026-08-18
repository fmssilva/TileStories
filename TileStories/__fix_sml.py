import os
path = 'Assets/Framework/Editor/POIAuthoring/SpecificMarker/POIAuthoringToolWindow.SpecificMarker.cs'
raw = open(path, 'rb').read()
bom = raw[:3] == b'\xef\xbb\xbf'
body = raw[3:] if bom else raw
le = '\r\n' if b'\r\n' in body else '\n\n'.replace('\n\n','\n') if False else ('\r\n' if b'\r\n' in body else '\n')
# normalize: detect line ending
if b'\r\n' in body:
    le = '\r\n'
else:
    le = '\n'
lines = body.decode('utf-8').split(le)
def lead(s):
    n = 0
    for c in s:
        if c == ' ':
            n += 1
        else:
            break
    return n
# Correct indent = leading spaces of the sibling foldoutKey line (line index 23).
correct = lead(lines[23])
prefix = 'expanded = EditorGUILayout.Foldout(expanded,'
for i, ln in enumerate(lines):
    st = ln.lstrip(' ')
    if st.startswith(prefix):
        old = lead(ln)
        lines[i] = (' ' * correct) + st
        print('line', i + 1, 'indent', old, '->', correct)
out = le.join(lines).encode('utf-8')
open(path, 'wb').write((b'\xef\xbb\xbf' if bom else b'') + out)
print('done; correct indent =', correct)
