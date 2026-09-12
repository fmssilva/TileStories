import os, re, glob

root = r'C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Tests'
paths = glob.glob(os.path.join(root, '**', '*.cs'), recursive=True)

# 1) Remove lines that set x_norm/y_norm as POI fields
xnorm_line = re.compile(r'^\s*(x_norm|y_norm)\s*=.*$')

# 2) Fix TryResolvePosition signatures
def fix_resolve(m):
    # m group1 = everything up to first ", out", group2 = the out param
    return m.group(1) + 'out ' + m.group(2)

pairs = [
    (re.compile(r'TryResolvePosition\(poi,\s*config\.calibration_anchors\.ToArray\(\),\s*out var worldPos\)'),
     'TryResolvePosition(poi, out var worldPos)'),
    (re.compile(r'TryResolvePosition\(poi,\s*calibrationAnchors,\s*out Vector3 localPos\)'),
     'TryResolvePosition(poi, out Vector3 localPos)'),
    (re.compile(r'TryResolvePosition\(poi,\s*anchors,\s*out Vector3 localPos\)'),
     'TryResolvePosition(poi, out Vector3 localPos)'),
    (re.compile(r'TryResolvePosition\(poi,\s*_config\.calibration_anchors\.ToArray\(\)?,\s*out var worldPos\)'),
     'TryResolvePosition(poi, out var worldPos)'),
    (re.compile(r'TryResolvePosition\(poi,\s*null,\s*out Vector3 pos\)'),
     'TryResolvePosition(poi, out Vector3 pos)'),
    (re.compile(r'TryResolvePosition\(poi,\s*anchors,\s*out var pos\)'),
     'TryResolvePosition(poi, out var pos)'),
    (re.compile(r'TryResolvePosition\(poi,\s*calibrationAnchors\),\s*out'),
     'TryResolvePosition(poi, out'),
    (re.compile(r'TryResolvePosition\(poi,\s*null,\s*out var pos\)'),
     'TryResolvePosition(poi, out var pos)'),
    (re.compile(r'TryResolvePosition\(poi,\s*calibrationAnchors,\s*out var pos\)'),
     'TryResolvePosition(poi, out var pos)'),
]

def fix_file(path):
    with open(path, 'r', encoding='utf-8') as f:
        out_lines = []
        changed = False
        for line in f:
            newline = line
            for pat, repl in pairs:
                if pat.search(newline):
                    newline = pat.sub(repl, newline)
                    changed = True
            if xnorm_line.match(line):
                # skip the line entirely (removing x_norm/y_norm field assignment)
                changed = True
                continue
            out_lines.append(newline)
        if changed:
            with open(path, 'w', encoding='utf-8') as f:
                f.writelines(out_lines)
        return changed

total = 0
for p in paths:
    if fix_file(p):
        total += 1
print('changed files:', total)