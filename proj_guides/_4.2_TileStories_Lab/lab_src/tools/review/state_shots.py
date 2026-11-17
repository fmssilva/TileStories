"""The profile matrix of the app's states (ported from the World Lab's tools/review/state_shots.py).
usage: python tools/review/state_shots.py <outdir> [states,comma|all] [devices,comma|all] [profiles: all|light:default,dark:larger...]
One contact sheet per state, <outdir>/sheets/<state>.png: a row per profile (light / dark x text default / larger), a column per
device (phone and tablet, portrait and landscape); each cell in <outdir>/raw/<state>__<mode>_<text>__<device>.png."""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from labpage import PROFILES, DEVICES, open_page, run, base, shot, sheet, main

# each state from the base (labpage.BASE): the router's own calls, as the app makes them
STATES = {
    'wall': '',
    'explore': 'LAB.noCamera=true;goScreen("explore",{stop:"closed",reset:true});',
    'card_peek': 'goScreen("card",{stop:"peek"});',
    'card_half': 'goScreen("card",{stop:"half"});',
    'card_full': 'goScreen("card",{stop:"full"});',
    'settings': 'goScreen("card",{stop:"half"});openOverlay("settings");',
    'dock_tour': 'goScreen("card",{stop:"half"});renderAll(false);startTour();',
}
WAIT = {'dock_tour': 2500}

async def shots(p):
    a = sys.argv
    outdir = a[1]
    states = list(STATES) if len(a) < 3 or a[2] == 'all' else a[2].split(',')
    devs = DEVICES if len(a) < 4 or a[3] == 'all' else a[3].split(',')
    profs = PROFILES if len(a) < 5 or a[4] == 'all' else [tuple(x.split(':')) for x in a[4].split(',')]
    os.makedirs(outdir + '/raw', exist_ok=True); os.makedirs(outdir + '/sheets', exist_ok=True)
    b, pg, errs = await open_page(p)
    for st in states:
        rows = []
        for mode, text in profs:
            row = []
            for dv in devs:
                await run(pg, base(dv, mode, text) + STATES[st] + 'renderAll(false);""')
                await pg.wait_for_timeout(WAIT.get(st, 900))
                row.append(await shot(pg, '%s/raw/%s__%s_%s__%s.png' % (outdir, st, mode, text, dv)))
            rows.append(('%s %s' % (mode, text), row))
        print(st, sheet(st + '  (rows: mode / text; columns: ' + ', '.join(devs) + ')', rows, '%s/sheets/%s.png' % (outdir, st)))
    print('page errors:', errs[:5])
    await b.close()

if __name__ == '__main__':
    main(shots)
