"""probe: LAB=... python probe.py <state> <devs> <profs: prem:l:m,...|all> <js expr returning JSON-able>"""
import pathlib, asyncio, os, sys, json
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from state_shots import STATES, PROFILES
from playwright.async_api import async_playwright
async def main(st, devs, profs, expr):
    async with async_playwright() as p:
        b = await p.chromium.launch(args=["--no-sandbox"]); pg = await b.new_page(viewport={"width": 1440, "height": 900})
        errs = []; pg.on("pageerror", lambda e: errs.append(str(e)))
        await pg.goto(pathlib.Path(os.environ.get("LAB", "out/dev4_F.html")).resolve().as_uri()); await pg.wait_for_timeout(1200)
        await pg.evaluate("document.querySelector('#tab-app').click()")
        for (lk, m, tx) in profs:
            for dv in devs:
                vis = "children" if lk == "playful" else "adults"
                js = (f"(()=>{{ui.device='{dv}';buildDevice();setScreen('app');D.app.appearance='{m}';ui.appearance='{m}';ui.forceMode=null;D.profile.appearance=null;"
                      f"D.profile.visitors='{vis}';D.profile.look='{lk}';D.profile.time='classic';D.profile.text='{tx}';lookCache=null;ui.coachDone=true;ui.lensOpen=false;"
                      f"ui.compPreview=null;ui.previewLayout=null;tapState.view=null;ui.cam={{...CAM0}};{STATES[st]}renderPhone();}})()")
                await pg.evaluate(js); await pg.wait_for_timeout(900)
                r = await pg.evaluate(expr)
                print(f"{lk[:4]}:{m[0]}:{tx} {dv}:", json.dumps(r))
        print("errors", errs[:5]); await b.close()
a = sys.argv
pr = PROFILES if a[3] == "all" else [x for x in PROFILES if (x[0][:4], x[1][0], x[2]) in [tuple(t.split(":")) for t in a[3].split(",")]]
asyncio.run(main(a[1], a[2].split(","), pr, a[4]))
