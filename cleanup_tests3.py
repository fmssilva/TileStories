import os, re, glob

root = r'C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Tests'
paths = glob.glob(os.path.join(root, '**', '*.cs'), recursive=True)

# Fix "x = 0.2, y = 0f, z = 0.8" so x and z have f suffix
pat = re.compile(r'x\s*=\s*([0-9]+)\s*, y = 0f, z\s*=\s*([0-9]+)')
# handle decimals too
pat_dec = re.compile(
    r'(new CapturedPosition \{ x\s*=\s*)(-?[\d.]+)(, y = 0f, z\s*=\s*)(-?[\d.]+)( \})'
)

def fix(path):
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()
    def repl(m):
        return m.group(1) + m.group(2) + 'f' + m.group(3) + m.group(4) + 'f' + m.group(5)
    new = pat_dec.sub(repl, content)
    if new != content:
        with open(path, 'w', encoding='utf-8') as f:
            f.write(new)
        return True
    return False

total = 0
for p in paths:
    if fix(p):
        total += 1
print('fixed:', total)