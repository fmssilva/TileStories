"""Lists every Editor fold of every tab into inventory.json beside this script, each with its depth, its rows and whether a row
of it has a demo (ported from the World Lab's tools/review/inventory.py). usage: python tools/review/inventory.py
A fold is a foldout (details[data-fo]); a row's demo is its rowDemos spec (editor/demo.js: rowKey, rowDemoOf)."""
import json, os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from labpage import open_page, run, main

LIST = r'''var out={};ST_TABS.forEach(function(tab){ui.tab=tab;ui.level=9;ui.op={};renderEditor();
  out[tab]=[].map.call(ed.querySelectorAll("details[data-fo]"),function(d){var s=d.querySelector(":scope > summary"),dp=0,e=d.parentElement;
    while(e&&e!==ed){if(e.matches("details[data-fo]"))dp++;e=e.parentElement}
    var rows=[].filter.call(d.querySelectorAll(".row"),function(r){return r.closest("details[data-fo]")===d}),demo=rows.map(rowKey).filter(function(k){return k&&rowDemoOf(k)});
    return{id:d.dataset.fo,title:s?s.textContent.replace(/\s+/g," ").trim().slice(0,80):"",depth:dp,rows:rows.length,demo:demo.length>0,key:demo[0]||null}})});
ui.tab="card";ui.level=1;ui.op={};renderEditor();JSON.stringify(out)'''

async def inv(p):
    b, pg, errs = await open_page(p)
    out = json.loads(await run(pg, LIST))
    path = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'inventory.json')
    json.dump(out, open(path, 'w', encoding='utf-8'), indent=1, ensure_ascii=False)
    for t, L in out.items():
        print(t, len(L), 'folds,', sum(1 for f in L if f['demo']), 'with a demo')
    print('page errors:', errs[:5])
    await b.close()

if __name__ == '__main__':
    main(inv)
