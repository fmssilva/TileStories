import sys
path='Assets/Framework/Tests/Editor/PoiAuthoringVisualHierarchyTests.cs'
raw=open(path,'rb').read()
bom=raw[:3]==b'\xef\xbb\xbf'
body=raw[3:] if bom else raw
le='\r\n' if b'\r\n' in body else '\n'
lines=body.decode('utf-8').split(le)
def lead(s):
    n=0
    for c in s:
        if c==' ': n+=1
        else: break
    return n
fixes=[
  ('ReflectColor("SearchFilterSectionColor");', 12),
  ('Assert.IsNull(ReflectField("SearchKeywordsSectionColor"),', 12),
  ('// Structural guard: POI header foldouts are bold + colored via the shared', 8),
  ('Assert.IsFalse(src.Contains("SearchKeywordsSectionColor"),', 12),
]
total=0
for i,ln in enumerate(lines):
    st=ln.lstrip(' ')
    for pred,want in fixes:
        if st.startswith(pred):
            cur=lead(ln)
            if cur!=want:
                lines[i]=(' '*want)+st
                print('FIXED test line',i+1,cur,'->',want)
                total+=1
            break
out=(b'\xef\xbb\xbf' if bom else b'')+le.join(lines).encode('utf-8')
open(path,'wb').write(out)
print('total fixed',total)
