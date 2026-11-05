"""Builds the Block Components Lab: joins the modules in MODULES and the styles in STYLES, and embeds the assets.
It never rewrites code. python build.py -> block-components-lab.html (not committed). PROTOTYPE_STRUCTURE.md says what each module owns."""
import base64, io, os, subprocess, sys
D = os.path.dirname(os.path.abspath(__file__))
M3 = os.path.join(D, '..', '..', 'mascote', '3d')

# the order is the load order: a module may use what an earlier one defines
MODULES = ['v3.js', 'v4.js', 'v5.js', 'v6.js', 'v7.js',
           'v8a.js', 'v8b.js', 'v8c.js', 'v8d.js', 'v8e.js', 'v8f.js', 'v8g.js', 'v8h.js', 'v8i.js', 'v8j.js', 'v8k.js', 'v8l.js',
           'v8m.js', 'v8n.js', 'v8o.js', 'v8r.js', 'v8p.js', 'v8q.js', 'v8s.js', 'v8t.js', 'v8u.js', 'v8v.js', 'v8w.js', 'v8x.js',
           'v8y.js', 'v8z.js', 'v9a.js', 'v9b.js',
           'boot.js']
STYLES = ['v2.css', 'v3extra.css', 'v5.css', 'v6.css', 'v7.css', 'v8.css', 'v8b.css', 'v8c.css', 'v8d.css', 'v8e.css', 'v8f.css',
          'v8g.css', 'v8h.css', 'v8i.css', 'v8j.css', 'v8k.css', 'v8l.css', 'v8m.css', 'v8n.css', 'v8o.css', 'v8r.css', 'v8p.css',
          'v8q.css', 'v8s.css', 'v8t.css', 'v8u.css', 'v8v.css', 'v8w.css', 'v8x.css', 'v8y.css', 'v8z.css']


def rd(*p):
    return io.open(os.path.join(D, *p), encoding='utf-8').read()


def b64(path):
    return base64.b64encode(open(path, 'rb').read()).decode()


# the assets: each placeholder in the code becomes its file, as a data: URL
ASSETS = [('__REALSTR__', None, os.path.join(D, 'realstrings.json'))]
ASSETS += [('__' + k + '__', 'image/jpeg', os.path.join(D, f + '.jpg')) for k, f in
           (('HERO', 'paco_hero'), ('G1', 'paco_gallery_1'), ('G2', 'paco_gallery_2'), ('G3', 'paco_gallery_3'),
            ('BEFORE', 'paco_before_1755'), ('BELEM', 'belem_wall'))]
ASSETS += [('__AZUGLB__', 'model/gltf-binary', os.path.join(M3, 'azu_tile.glb'))]
ASSETS += [('__AZUGLB_' + k + '__', 'model/gltf-binary', os.path.join(M3, f)) for k, f in
           (('AZU', 'azu.glb'), ('MOUSE', 'mouse.glb'), ('EXPLORER', 'explorer.glb'), ('POET', 'poet.glb'), ('SOLDIER', 'soldier.glb'))]
ASSETS += [('__AZTHUMB_' + k + '__', 'image/png', os.path.join(M3, 'thumbs', k.lower() + '.png')) for k in
           ('AZU', 'TILE', 'MOUSE', 'EXPLORER', 'POET', 'SOLDIER')]


def build():
    js = '\n'.join(rd(m) for m in MODULES)
    for key, mime, path in ASSETS:
        assert key in js, key
        data = io.open(path, encoding='utf-8').read() if mime is None else 'data:' + mime + ';base64,' + b64(path)
        js = js.replace(key, data)
    css = ''.join(rd(s) for s in STYLES)
    html = rd('head.html') + '<style>' + css + '</style>\n' + rd('page.html') + '\n<script>\n' + js + '\n</script>\n'
    io.open(os.path.join(D, 'block-components-lab.html'), 'w', encoding='utf-8').write(html)
    print('built', len(html))


if __name__ == '__main__':
    build()
    if os.path.exists(os.path.join(D, 'check_structure.js')) and '--no-check' not in sys.argv:
        sys.exit(subprocess.call(['node', os.path.join(D, 'check_structure.js')]))
