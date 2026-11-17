"""The theme's ratchet (merge P1b.1, THEME_MAP.md): what is not on the theme's tokens yet, counted per file, and the counts only go down.
  colours     colour literals (#hex, rgb(), rgba(), hsl()) outside core/theme.js: the dock, the beats, the skins, the chips, the always-dark
              surfaces keep their values until P3 ports them onto the tokens
  small_type  phone font sizes under 13 px (the 13 pt floor, .clinerules/30-ui-content.md; _4.1.7 §2): a size a round touches comes
              from the type tokens (--ts-size-*) and never under 13
The Editor (src/editor/: its skin is the Lab's, never themed) is left out. tools/theme_ratchet.json holds each file's counts: a count
going up, or a new file with any, fails the build; a lower count asks for the json to be lowered (python tools/theme_ratchet.py --lower).
python tools/theme_ratchet.py [--lower]   (build.py runs it after the structure check and the Unity lint)"""
import io, json, os, re, sys
LAB = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SRC = os.path.join(LAB, 'src')
JSON = os.path.join(LAB, 'tools', 'theme_ratchet.json')
SKIP_DIRS = ('editor',)
SKIP_FILES = ('core/theme.js',)
# a colour literal: a #hex of 3, 4, 6 or 8 digits (not an HTML entity &#9790; nor longer), or a colour function
COLOUR = re.compile(r'(?<![&\w])#(?:[0-9a-fA-F]{8}|[0-9a-fA-F]{6}|[0-9a-fA-F]{3,4})(?![\w-])|\b(?:rgba?|hsla?)\(')
# a font size in px: font-size:Npx, or the size of a font: shorthand (its first px)
SIZE = re.compile(r'font-size:\s*([0-9.]+)px|font:[^;{}"\'<>]*?\b([0-9.]+)px')


def count():
    out = {}
    for root, _, files in os.walk(SRC):
        rel_root = os.path.relpath(root, SRC).replace('\\', '/')
        if rel_root.split('/')[0] in SKIP_DIRS:
            continue
        for f in sorted(files):
            if not f.endswith(('.js', '.css')):
                continue
            rel = (rel_root + '/' + f).lstrip('./')
            if rel in SKIP_FILES:
                continue
            s = io.open(os.path.join(root, f), encoding='utf-8').read()
            c = len(COLOUR.findall(s))
            t = sum(1 for m in SIZE.finditer(s) if float(m.group(1) or m.group(2)) < 13)
            if c or t:
                out[rel] = {'colours': c, 'small_type': t}
    return out


def main():
    now = count()
    base = json.load(io.open(JSON, encoding='utf-8')) if os.path.exists(JSON) else {}
    if '--lower' in sys.argv or not base:
        json.dump(now, io.open(JSON, 'w', encoding='utf-8', newline='\n'), indent=1, sort_keys=True)
        base = now
    up, down = [], []
    for f, c in now.items():
        for k in ('colours', 'small_type'):
            was = base.get(f, {}).get(k, 0)
            if c[k] > was:
                up.append('%s %s %d (allowed %d)' % (f, k, c[k], was))
            elif c[k] < was:
                down.append('%s %s %d (was %d)' % (f, k, c[k], was))
    for f, c in base.items():
        if f not in now:
            down.append('%s gone (was %s)' % (f, c))
    tot = lambda d, k: sum(v[k] for v in d.values())
    print('%s   theme ratchet: %d colour literals and %d phone sizes under 13 px in %d files (allowed %d / %d); %d increases, %d counts to lower%s' % (
        'FAIL' if up else 'ok', tot(now, 'colours'), tot(now, 'small_type'), len(now), tot(base, 'colours'), tot(base, 'small_type'), len(up), len(down),
        ' (python tools/theme_ratchet.py --lower)' if down else ''))
    for u in up:
        print('  up: ' + u)
    return 1 if up else 0


if __name__ == '__main__':
    sys.exit(main())
