"""The TileStories Lab's publish list: the files of one artifact, at the paths the page asks for (README.md, "Publish").

    python tools/publish.py                  the list's summary: each batch's files and size, the total
    python tools/publish.py --json <file>    also writes the files maps, one per publish: {"published/path": "absolute source path"}
    python tools/publish.py --stage <dir>    copies every file to <dir> at its published path (the published layout, for its proof)

The published layout (the page's relative URLs work from it and from tools/serve.py's folder, which has lab_src/ beside
voicebench/ and music/: the page tries "voicebench/..." and "music/..." first, then "../"):
    tilestories-lab.html                      lab_src/tilestories-lab.html (python build.py)
    assets/...                                lab_src/assets/ (the pictures; the mascot's thumbnails and its models as their
                                              <id>.glb.js scripts, build.py: the host serves no .glb, so the .glb files stay out)
    voicebench/out/voicesets.js, packs.js     the public voice sets (never out-private: Edge, this computer only)
    voicebench/out/<set>/pack-N.js            a set packed by voicebench/pack.py (its MP3s are not published: too many files)
    voicebench/out/<set>/<line>.mp3           a set not packed: its files one by one
    music/tracks.js and its tracks            ../music
The Artifact tool's limits: 16 MB a file, 64 MB and 255 files a publish; more go in another publish to the same url, so the list
is split into batches in this order (the page and its pictures first). A file over 16 MB, one with an extension the host does not
serve (SERVED), or a page not built, fails the list."""
import json, os, shutil, sys

LAB = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))   # lab_src
ROOT = os.path.dirname(LAB)                                         # the Lab's folder: lab_src, voicebench, music
FILE_MAX, PUB_MAX, PUB_FILES = 16 * 10 ** 6, 64 * 10 ** 6, 255   # decimal MB: the safer reading of the limits
# the extensions the artifact host serves (no .glb under any type: the models go as their .glb.js scripts); any other fails the list
SERVED = {'.html', '.css', '.js', '.mjs', '.wasm', '.json', '.map', '.txt', '.md', '.csv', '.xml',
          '.png', '.jpg', '.jpeg', '.gif', '.webp', '.svg', '.avif', '.ico', '.woff', '.woff2', '.ttf', '.otf',
          '.mp3', '.wav', '.ogg', '.mp4', '.webm', '.pdf'}


def files():
    """[(published path, source path)] in publish order"""
    out = []
    page = os.path.join(LAB, 'tilestories-lab.html')
    if not os.path.isfile(page):
        sys.exit('The page is not built: python build.py')
    out.append(('tilestories-lab.html', page))
    for base, _, fs in sorted(os.walk(os.path.join(LAB, 'assets'))):
        for f in sorted(fs):
            if f.endswith('.glb'):   # the host serves no .glb: its <id>.glb.js goes instead (build.py)
                continue
            src = os.path.join(base, f)
            out.append(('assets/' + os.path.relpath(src, os.path.join(LAB, 'assets')).replace('\\', '/'), src))
    vo = os.path.join(ROOT, 'voicebench', 'out')
    for f in ('voicesets.js', 'packs.js'):
        if os.path.isfile(os.path.join(vo, f)):
            out.append(('voicebench/out/' + f, os.path.join(vo, f)))
    for s in sorted(d for d in os.listdir(vo) if os.path.isdir(os.path.join(vo, d))):
        sd = os.path.join(vo, s)
        packs = sorted(f for f in os.listdir(sd) if f.startswith('pack-') and f.endswith('.js'))
        for f in packs or sorted(os.listdir(sd)):
            if os.path.isfile(os.path.join(sd, f)):
                out.append(('voicebench/out/%s/%s' % (s, f), os.path.join(sd, f)))
    mu = os.path.join(ROOT, 'music')
    for f in sorted(os.listdir(mu)):
        if os.path.isfile(os.path.join(mu, f)):
            out.append(('music/' + f, os.path.join(mu, f)))
    return out


def batches(fl):
    """the list in publishes of at most PUB_MAX bytes and PUB_FILES files, in order"""
    out, cur, size = [], [], 0
    for pub, src in fl:
        n = os.path.getsize(src)
        if os.path.splitext(pub)[1].lower() not in SERVED:
            sys.exit('%s: the artifact host does not serve %s files (it serves %s)' % (pub, os.path.splitext(pub)[1] or 'extensionless', ' '.join(sorted(SERVED))))
        if n > FILE_MAX:
            sys.exit('%s is %.1f MB: over the 16 MB a file' % (pub, n / 1e6))
        if cur and (size + n > PUB_MAX or len(cur) >= PUB_FILES):
            out.append(cur)
            cur, size = [], 0
        cur.append((pub, src))
        size += n
    if cur:
        out.append(cur)
    return out


def mb(fl):
    return sum(os.path.getsize(s) for _, s in fl) / 1e6


if __name__ == '__main__':
    fl = files()
    bs = batches(fl)
    assert not any('out-private' in s for _, s in fl)
    for i, b in enumerate(bs):
        groups = {}
        for pub, src in b:
            g = pub.split('/')[0] if '/' in pub else pub
            if pub.startswith('voicebench/out/') and pub.count('/') > 2:
                g = 'voicebench/out/' + pub.split('/')[2]
            groups.setdefault(g, []).append((pub, src))
        print('publish %d: %d files, %.1f MB' % (i + 1, len(b), mb(b)))
        for g, l in groups.items():
            print('  %-58s %4d files %7.2f MB' % (g, len(l), mb(l)))
    print('total: %d files, %.1f MB, in %d publish(es) to the same url (limits: 16 MB a file, 64 MB and 255 files a publish)' % (len(fl), mb(fl), len(bs)))
    if '--json' in sys.argv:
        p = sys.argv[sys.argv.index('--json') + 1]
        json.dump([{pub: os.path.abspath(src) for pub, src in b} for b in bs], open(p, 'w', encoding='utf-8'), indent=1)
        print('files maps: ' + p)
    if '--stage' in sys.argv:
        d = sys.argv[sys.argv.index('--stage') + 1]
        for pub, src in fl:
            dst = os.path.join(d, *pub.split('/'))
            os.makedirs(os.path.dirname(dst), exist_ok=True)
            shutil.copy2(src, dst)
        print('staged in ' + d)
