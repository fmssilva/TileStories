"""One contact sheet per Editor fold with a demo: its first demo row's demo (editor/demo.js's showRowDemo, the one runner a row's
change or its label's press runs), under every profile and device (ported from the World Lab's tools/review/section_shots.py).
usage: python tools/review/section_shots.py <outdir> [tab=card] [ids,comma|all] [devices,comma|all]
inventory.json first (python tools/review/inventory.py). Sheets: <outdir>/sheets/<tab>__<id>.png."""
import json, os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from labpage import PROFILES, DEVICES, open_page, run, base, shot, sheet, main, parse_profiles

INV = json.load(open(os.path.join(os.path.dirname(os.path.abspath(__file__)), 'inventory.json'), encoding='utf-8'))

async def shots(p):
    a = sys.argv
    outdir, tab = a[1], (a[2] if len(a) > 2 else 'card')
    ids = (a[3] if len(a) > 3 else 'all').split(',')
    devs = DEVICES if len(a) < 5 or a[4] == 'all' else a[4].split(',')
    folds = [f for f in INV[tab] if f['demo'] and (ids == ['all'] or f['id'] in ids)]
    os.makedirs(outdir + '/raw', exist_ok=True); os.makedirs(outdir + '/sheets', exist_ok=True)
    b, pg, errs = await open_page(p)
    for f in folds:
        rows = []
        for prof in PROFILES:
            row = []
            for dv in devs:
                ok = await run(pg, base(dv, prof) + 'LAB.rowDemo=true;ui.tab="%s";renderAll(false);showRowDemo(%s)' % (tab, json.dumps(f['key'])))
                await pg.wait_for_timeout(1300)
                row.append(await shot(pg, '%s/raw/%s__%s__%s__%s.png' % (outdir, tab, f['id'].replace(':', '_').replace('*', '_'), '_'.join(prof), dv)))
            rows.append((' '.join(prof), row))
        name = f['id'].replace(':', '_').replace('*', '_')
        print(f['id'], 'ok' if ok else 'NO DEMO RAN', sheet('%s > %s (%s): the demo of %s' % (tab, f['id'], f['title'], f['key']), rows, '%s/sheets/%s__%s.png' % (outdir, tab, name), H=360))
    print('page errors:', errs[:5])
    await b.close()

if __name__ == '__main__':
    main(shots)
