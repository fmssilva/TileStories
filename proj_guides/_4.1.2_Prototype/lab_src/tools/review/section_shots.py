"""One contact sheet per editor section/subsection: the demo the fold opens, under every design profile.
usage: LAB=out/dev3.html python section_shots.py <outdir> [tab=app] [ids=comma list|all] [devices=phone_p,phone_l]
rows = profiles (premium/playful × light/dark, + text L), cols = devices. Sheets: <outdir>/sheets/<tab>__<id>.png"""
import pathlib, asyncio, os, sys, json
from playwright.async_api import async_playwright
from PIL import Image, ImageDraw
INV = json.load(open(os.path.join(os.path.dirname(os.path.abspath(__file__)), "inventory.json"), encoding="utf-8"))
PROFILES = [("premium", "light", "m"), ("premium", "dark", "m"), ("playful", "light", "m"), ("playful", "dark", "m"), ("premium", "light", "l"), ("playful", "light", "l")]

def ancestors(tab):
    """id -> list of ancestor ids, from the inventory's document order and depths"""
    out, stack = {}, []
    for f in INV[tab]:
        while stack and stack[-1][1] >= f["depth"]: stack.pop()
        out[f["id"]] = [s[0] for s in stack]; stack.append((f["id"], f["depth"]))
    return out

async def main(outdir, tab, ids, devs):
    os.makedirs(outdir + "/raw", exist_ok=True); os.makedirs(outdir + "/sheets", exist_ok=True)
    anc = ancestors(tab); folds = [f for f in INV[tab] if ids == ["all"] or f["id"] in ids]
    async with async_playwright() as p:
        b = await p.chromium.launch(args=["--no-sandbox"]); pg = await b.new_page(viewport={"width": 1440, "height": 900})
        errs = []; pg.on("pageerror", lambda e: errs.append(str(e)))
        await pg.goto(pathlib.Path(os.environ.get("LAB", "out/lab.html")).resolve().as_uri()); await pg.wait_for_timeout(1200)
        await pg.evaluate(f"document.querySelector('#tab-{tab}').click(); document.fonts.load('600 20px Fredoka')")
        for f in folds:
            rows = []
            for (lk, m, tx) in PROFILES:
                row = []
                for dv in devs:
                    vis = "children" if lk == "playful" else "adults"
                    js = (f"(()=>{{ui.device='{dv}';buildDevice();D.app.appearance='{m}';ui.appearance='{m}';ui.forceMode=null;D.profile.appearance=null;"
                          f"D.profile.visitors='{vis}';D.profile.look='{lk}';D.profile.time='classic';D.profile.text='{tx}';lookCache=null;ui.coachDone=true;"
                          f"ui.compPreview=null;ui.previewLayout=null;ui.cam={{...CAM0}};ui.open={{}};" +
                          "".join(f"ui.open['{a}']=true;" for a in anc[f["id"]]) + "renderEditor();"
                          f"const h=document.querySelector('#edb .fold[data-id=\"{f['id']}\"] > .fh'); if(h) h.click(); return !!h}})()")
                    ok = await pg.evaluate(js); await pg.wait_for_timeout(1300)
                    path = f"{outdir}/raw/{tab}__{f['id']}__{lk}_{m}_{tx}__{dv}.png"
                    await (await pg.query_selector(".dev")).screenshot(path=path); row.append(path)
                rows.append(((lk, m, tx), row))
            # contact sheet
            ims = [[Image.open(pth) for pth in r] for _, r in rows]; H = 360
            ims = [[im.resize((int(im.size[0] * H / im.size[1]), H)) for im in r] for r in ims]
            W = max(sum(im.size[0] for im in r) for r in ims) + 120
            S = Image.new("RGB", (W, H * len(ims) + 30), "white"); d = ImageDraw.Draw(S)
            d.text((6, 6), f"{tab} · {f['id']} · {f['title']}", fill=(0, 0, 0))
            for ri, ((lk, m, tx), _) in enumerate(rows):
                y = 30 + ri * H; d.text((6, y + H // 2), f"{lk}\n{m}\ntext {tx}", fill=(0, 0, 0)); x = 120
                for im in ims[ri]: S.paste(im, (x, y)); x += im.size[0]
            S.save(f"{outdir}/sheets/{tab}__{f['id']}.png")
            print(f["id"], "ok" if ok else "NO HEADER")
        print("errors", errs[:5]); await b.close()

if __name__ == "__main__":
    a = sys.argv
    asyncio.run(main(a[1], a[2] if len(a) > 2 else "app", (a[3] if len(a) > 3 else "all").split(","), (a[4] if len(a) > 4 else "phone_p,phone_l").split(",")))
