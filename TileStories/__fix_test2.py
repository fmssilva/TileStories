path='Assets/Framework/Tests/Editor/PoiAuthoringVisualHierarchyTests.cs'
raw=open(path,'rb').read()
bom=raw[:3]==b'\xef\xbb\xbf'
body=raw[3:] if bom else raw
le='\r\n' if b'\r\n' in body else '\n'
lines=body.decode('utf-8').split(le)
def idx_of(pred):
    for i,l in enumerate(lines):
        if l.lstrip(' ').startswith(pred): return i
    return -1
i=idx_of('Assert.IsTrue(src.Contains("PoiHeaderPalette"),')
assert i>=0, 'palette assert not found'
assert lines[i+1].lstrip(' ').startswith('"POI header foldouts must reference the PoiHeaderPalette array"),'), 'msg line mismatch'
del lines[i:i+2]
j=idx_of('Assert.IsTrue(src.Contains("CreateFoldoutStyle(PoiHeaderColorFor"),')
assert j>=0, 'CreateFoldoutStyle assert not found'
assert lines[j+1].lstrip(' ').startswith('"POI header foldout must be rendered bold+colored via CreateFoldoutStyle(PoiHeaderColorFor'), 'msg line mismatch'
new_lines=['            Assert.IsFalse(src.Contains("CreateFoldoutStyle(PoiHeaderColor)"),',
           '                "old CreateFoldoutStyle(PoiHeaderColor) call must be gone, replaced by CreateFoldoutStyle(PoiHeaderColorFor(...))");']
lines[j+2:j+2]=new_lines
out=(b'\xef\xbb\xbf' if bom else b'')+le.join(lines).encode('utf-8')
open(path,'wb').write(out)
print('patched; total lines now', len(lines))
