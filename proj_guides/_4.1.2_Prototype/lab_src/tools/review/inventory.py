"""Lists every editor fold (App, Global, Specific) into inventory.json beside this script. usage: LAB=out/lab.html python inventory.py"""
import pathlib, asyncio, os, json
from playwright.async_api import async_playwright
async def main():
    async with async_playwright() as p:
        b = await p.chromium.launch(args=["--no-sandbox"]); pg = await b.new_page(viewport={"width": 1440, "height": 900})
        await pg.goto(pathlib.Path(os.environ.get("LAB", "out/lab.html")).resolve().as_uri()); await pg.wait_for_timeout(1200)
        out = {}
        for tab in ["app", "global", "specific"]:
            out[tab] = await pg.evaluate(f"""(()=>{{document.querySelector('#tab-{tab}').click();
              const seen=new Set();for(let k=0;k<6;k++){{document.querySelectorAll('#edb .fold:not(.open) > .fh').forEach(h=>{{const id=h.parentElement.dataset.id;if(!seen.has(id)){{seen.add(id);ui.open[id]=true}}}});renderEditor();}}
              return [...document.querySelectorAll('#edb .fold')].map(f=>{{let d=0,e=f.parentElement;while(e&&e.id!=='edb'){{if(e.classList.contains('fold'))d++;e=e.parentElement}}
                return {{id:f.dataset.id,title:f.querySelector(':scope > .fh .ft')?.textContent,depth:d,demo:f.classList.contains('hasdemo'),rows:f.querySelectorAll(':scope > .fb > .urow').length}}}})}})()""")
        json.dump(out, open(os.path.join(os.path.dirname(os.path.abspath(__file__)), "inventory.json"), "w", encoding="utf-8"), indent=1)
        for t, L in out.items(): print(t, len(L), "folds,", sum(1 for f in L if f["demo"]), "with demo")
        await b.close()
asyncio.run(main())
