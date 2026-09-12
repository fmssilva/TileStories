import os, re, glob

root = r'C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Tests'
paths = glob.glob(os.path.join(root, '**', '*.cs'), recursive=True)

# Convert inline "x_norm = <x>, y_norm = <y>" (within a POIData initializer) into captured_position.
# Captures: has_captured_position = true, captured_position = new CapturedPosition { x = <x>, y = 0, z = <y> }
pat_inline = re.compile(
    r'x_norm\s*=\s*(-?[\d.]+)f?\s*,\s*y_norm\s*=\s*(-?[\d.]+)f?',
    re.IGNORECASE)

def fix_file(path):
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()
    def repl(m):
        x = m.group(1)
        y = m.group(2)
        return f'has_captured_position = true, captured_position = new CapturedPosition {{ x = {x}, y = 0f, z = {y} }}'
    new = pat_inline.sub(repl, content)
    if new != content:
        with open(path, 'w', encoding='utf-8') as f:
            f.write(new)
        return True
    return False

total = 0
for p in paths:
    if fix_file(p):
        total += 1
print('changed files:', total)