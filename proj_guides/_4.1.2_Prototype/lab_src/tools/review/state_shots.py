"""v20 state shots: usage LAB=out/dev4.html python state_shots.py <outdir> <states,comma|all> [devices] [profiles=all|p,k]
sheet per state: rows = profiles, cols = devices -> <outdir>/sheets/<state>.png ; raw in <outdir>/raw"""
import pathlib, asyncio, os, sys
from playwright.async_api import async_playwright
from PIL import Image, ImageDraw
PROFILES = [("premium", "light", "m"), ("premium", "dark", "m"), ("playful", "light", "m"), ("playful", "dark", "m"), ("premium", "light", "l"), ("playful", "light", "l")]
TAPF = "const i=D.pois.findIndex(p=>p.id==='twin_tower_church');ui.appPoi=i;ui.appStep='tapped';ui.tapAnchor=null;tapState.view=null;ui.cam=camCover(camPoseFor(D.pois[i].pos,50,30,1.25));"
STATES = {
  "onb": "ui.appStep='onboarding';",
  "onb_end": "ui.appStep='onboarding';renderPhone();setTimeout(()=>{const s=[...document.querySelectorAll('.ph .ds-onb *')].find(e=>e.scrollHeight>e.clientHeight+20&&getComputedStyle(e).overflowY!=='visible');if(s)s.scrollTop=9999},50);",
  "main": "ui.appStep='main';ui.cam={...CAM0};",
  "main_filter": "ui.appStep='main';ui.cam={...CAM0};ui.lensOpen=true;",
  "settings": "ui.appStep='settings';",
  "settings_end": "ui.appStep='settings';renderPhone();setTimeout(()=>{const s=document.querySelector('.ph .sheet-set .ds-scroll');if(s)s.scrollTop=9999},50);",
  "you": "ui.appStep='you';",
  "imm": "ui.previewLayout='tap_layout_3';" + TAPF,
  "pan": "ui.previewLayout='tap_layout_4';" + TAPF,
  "soon": "ui.previewLayout='tap_layout_3';soonDemo(false);",
  "soon_chip": "ui.previewLayout='tap_layout_2';soonDemo(false);",
  "soon_page": "soonDemo(true);",
}
async def main(outdir, states, devs, profs):
    os.makedirs(outdir + "/raw", exist_ok=True); os.makedirs(outdir + "/sheets", exist_ok=True)
    async with async_playwright() as p:
        b = await p.chromium.launch(args=["--no-sandbox"]); pg = await b.new_page(viewport={"width": 1440, "height": 900})
        errs = []; pg.on("pageerror", lambda e: errs.append(str(e)))
        await pg.goto(pathlib.Path(os.environ.get("LAB", "out/lab.html")).resolve().as_uri()); await pg.wait_for_timeout(1200)
        await pg.evaluate("document.querySelector('#tab-app').click(); document.fonts.load('600 20px Fredoka')")
        for st in states:
            rows = []
            for (lk, m, tx) in profs:
                row = []
                for dv in devs:
                    vis = "children" if lk == "playful" else "adults"
                    js = (f"(()=>{{ui.device='{dv}';buildDevice();setScreen('app');D.app.appearance='{m}';ui.appearance='{m}';ui.forceMode=null;D.profile.appearance=null;"
                          f"D.profile.visitors='{vis}';D.profile.look='{lk}';D.profile.time='classic';D.profile.text='{tx}';ui.coachDone=true;ui.lensOpen=false;"
                          f"ui.compPreview=null;ui.previewLayout=null;tapState.view=null;ui.cam={{...CAM0}};{STATES[st]}renderPhone();}})()")
                    await pg.evaluate(js); await pg.wait_for_timeout(1500)
                    path = f"{outdir}/raw/{st}__{lk}_{m}_{tx}__{dv}.png"
                    await (await pg.query_selector(".dev")).screenshot(path=path); row.append(path)
                rows.append(((lk, m, tx), row))
            ims = [[Image.open(pth) for pth in r] for _, r in rows]; H = 420
            ims = [[im.resize((int(im.size[0] * H / im.size[1]), H)) for im in r] for r in ims]
            W = max(sum(im.size[0] for im in r) for r in ims) + 90
            S = Image.new("RGB", (W, H * len(ims) + 24), "white"); d = ImageDraw.Draw(S); d.text((6, 6), st, fill=(0, 0, 0))
            for ri, ((lk, m, tx), _) in enumerate(rows):
                y = 24 + ri * H; d.text((4, y + H // 2), f"{lk}\n{m}\n{tx}", fill=(0, 0, 0)); x = 90
                for im in ims[ri]: S.paste(im, (x, y)); x += im.size[0]
            S.save(f"{outdir}/sheets/{st}.png"); print(st, "ok")
        print("errors", errs[:5]); await b.close()
if __name__ == "__main__":
    a = sys.argv; st = list(STATES) if a[2] == "all" else a[2].split(",")
    pr = PROFILES if len(a) < 5 or a[4] == "all" else [x for x in PROFILES if (x[0][:4], x[1][0], x[2]) in [tuple(t.split(":")) for t in a[4].split(",")]] 
    asyncio.run(main(a[1], st, (a[3] if len(a) > 3 else "phone_p").split(","), pr))
