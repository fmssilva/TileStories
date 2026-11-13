"""Packs a voice set's MP3s into a few script files for the published Lab.

The published Lab is one artifact that holds at most 511 files, so a set of 500+ lines cannot go up one MP3 per line. Each pack is a
script that adds its lines to window.VOICEPACK as data URIs, keyed "<set id>/<file>"; out/packs.js lists the packs, and the Lab
(guide/voice.js) loads them when it is not opened from this computer's disk (there, the MP3s beside the manifest are read directly).

    python pack.py azure-ryan-duarte          # writes out/azure-ryan-duarte/pack-1.js ... and out/packs.js

Unity does not need this: its VoiceSet asset holds the clips themselves.
"""
import base64, io, json, os, sys

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'out')
LIMIT = 7 * 1024 * 1024  # characters a pack, well under the artifact's 16 MB a file


def pack(set_id):
    d = os.path.join(ROOT, set_id)
    for f in os.listdir(d):
        if f.startswith('pack-') and f.endswith('.js'):
            os.remove(os.path.join(d, f))
    mp3 = sorted(f for f in os.listdir(d) if f.endswith('.mp3'))
    packs, cur, size = [], [], 0
    for f in mp3:
        uri = 'data:audio/mpeg;base64,' + base64.b64encode(open(os.path.join(d, f), 'rb').read()).decode('ascii')
        if cur and size + len(uri) > LIMIT:
            packs.append(cur)
            cur, size = [], 0
        cur.append((f, uri))
        size += len(uri)
    if cur:
        packs.append(cur)
    names = []
    for i, p in enumerate(packs, 1):
        name = 'pack-%d.js' % i
        body = ',\n'.join('%s:%s' % (json.dumps(set_id + '/' + f), json.dumps(u)) for f, u in p)
        io.open(os.path.join(d, name), 'w', encoding='ascii', newline='\n').write(
            '(function(v){var p={\n' + body + '\n};for(var k in p)v[k]=p[k]})(window.VOICEPACK=window.VOICEPACK||{});\n')
        names.append(set_id + '/' + name)
    return names, len(mp3)


def main():
    ids = sys.argv[1:]
    if not ids:
        sys.exit('usage: python pack.py <set id> [<set id> ...]')
    listing = os.path.join(ROOT, 'packs.js')
    allp = []
    for s in ids:
        names, n = pack(s)
        allp += names
        print(s, n, 'lines in', len(names), 'packs')
    io.open(listing, 'w', encoding='utf-8', newline='\n').write('window.VOICEPACKS=' + json.dumps(allp) + ';\n')
    print('wrote', listing)


if __name__ == '__main__':
    main()
