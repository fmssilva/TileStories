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
fixed=0
for i,ln in enumerate(lines):
    st=ln.lstrip(' ')
    if st=='Assert.IsTrue(src.Contains("PoiHeaderColorFor"),':
        if lead(ln)!=12:
            lines[i]=(' '*12)+st
            print('FIXED line',i+1,lead(ln),'-> 12')
            fixed+=1
out=(b'\xef\xbb\xbf' if bom else b'')+le.join(lines).encode('utf-8')
open(path,'wb').write(out)
print('fixed',fixed)
