"""The review tools' shared part (ported from the World Lab's tools/review): the page over the Lab's local server, the profile
matrix and the contact sheets. Python Playwright with the installed Edge (as the Node tests: no browser is downloaded).
  PROFILES  rows: light / dark x text default / larger (P.vis.mode, P.vis.text: the visitor's Settings). Looks join at P1b.
  DEVICES   columns: screen/device.js's four (phone_p, phone_l, tablet_p, tablet_l).
A cell: the state's code run in the page (#probe: LAB.run, the page's scope), then the phone's frame captured (#ph)."""
import os, subprocess, sys
from playwright.async_api import async_playwright
from PIL import Image, ImageDraw

LAB = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))   # lab_src
EDGE = os.environ.get('EDGE', r'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe')
PORT = int(os.environ.get('LAB_PORT') or 8642)
URL = 'http://localhost:%d/lab_src/tilestories-lab.html#probe' % PORT
PROFILES = [('light', 'default'), ('dark', 'default'), ('light', 'larger'), ('dark', 'larger')]
DEVICES = ['phone_p', 'phone_l', 'tablet_p', 'tablet_l']
# every cell starts here: no tour, no overlay, no tip or hint, the preview bar hidden, the Ribeira Palace's card, the device and
# the profile set as the visitor's Settings sets them
BASE = ('if(G.on)stopTour();if(G.gd.pages)gdPagesEnd("review",false);gdStop("review");closeOverlays();D.guidance.on=false;D.card.hints.on=false;'
        'LAB.previewBar=false;LAB.noCamera=false;LAB.wall="ribeira";ui.camX=.4;ui.camY=.5;ui.camZoom=1;P.guideMute=true;P.introSeen=true;P.reduce=true;P.lang="en";pointOpen("painting");'
        'ui.device="{dev}";P.vis.mode="{mode}";P.dark={dark};P.vis.text="{text}";goScreen("card",{{stop:"closed",reset:true}});')


def ensure_server():
    r = subprocess.run([sys.executable, os.path.join(LAB, 'tools', 'serve.py'), '--ensure'], capture_output=True, text=True)
    if r.returncode:
        sys.exit('The Lab server did not start: ' + r.stdout.strip())


async def open_page(p, w=2000, h=1400):
    b = await p.chromium.launch(executable_path=EDGE, args=['--no-sandbox'])
    pg = await b.new_page(viewport={'width': w, 'height': h})
    errs = []
    pg.on('pageerror', lambda e: errs.append(str(e)))
    await pg.goto(URL)
    await pg.wait_for_function('() => window.LAB && window.LAB.run', timeout=30000)
    await pg.wait_for_timeout(800)
    return b, pg, errs


async def run(pg, src):
    return await pg.evaluate('s => LAB.run(s)', src)


def base(dev, mode, text):
    return BASE.format(dev=dev, mode=mode, dark='true' if mode == 'dark' else 'false', text=text)


async def shot(pg, path):
    await (await pg.query_selector('#ph')).screenshot(path=path)
    return path


def sheet(title, rows, path, H=420):
    """rows: [(label, [png...])] -> one contact sheet: a row per profile, a column per device"""
    ims = [[Image.open(p) for p in r] for _, r in rows]
    ims = [[im.resize((max(1, int(im.size[0] * H / im.size[1])), H)) for im in r] for r in ims]
    W = max(sum(im.size[0] + 8 for im in r) for r in ims) + 110
    S = Image.new('RGB', (W, (H + 8) * len(ims) + 30), 'white')
    d = ImageDraw.Draw(S)
    d.text((6, 8), title, fill=(0, 0, 0))
    for ri, (lab, _) in enumerate(rows):
        y = 30 + ri * (H + 8)
        d.text((6, y + H // 2), lab.replace(' ', '\n'), fill=(0, 0, 0))
        x = 110
        for im in ims[ri]:
            S.paste(im, (x, y))
            x += im.size[0] + 8
    S.save(path)
    return path


def main(coro):
    import asyncio
    ensure_server()
    async def run_all():
        async with async_playwright() as p:
            await coro(p)
    asyncio.run(run_all())
