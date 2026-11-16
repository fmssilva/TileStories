"""usage: python compare.py base.json new.json -- lists every check, header and screen that changed."""
import json, sys
def load(p):
    with open(p, encoding="utf-8") as f: return json.load(f)
a, b = load(sys.argv[1]), load(sys.argv[2]); diffs = 0
for k in a["checks"]:
    if a["checks"][k]["ok"] != b["checks"].get(k, {}).get("ok"): print("CHECK", k, a["checks"][k]["ok"], "->", b["checks"].get(k)); diffs += 1
for t in a["tabs"]:
    ha, hb = a["tabs"][t]["headers"], b["tabs"].get(t, {}).get("headers", [])
    for x in ha:
        if x not in hb: print(f"HEADER GONE [{t}] {x}"); diffs += 1
    for x in hb:
        if x not in ha: print(f"HEADER NEW  [{t}] {x}"); diffs += 1
    if a["tabs"][t]["rows"] != b["tabs"].get(t, {}).get("rows"): print(f"ROWS [{t}] {a['tabs'][t]['rows']} -> {b['tabs'].get(t, {}).get('rows')}"); diffs += 1
for s in a["screens"]:
    if a["screens"][s] != b["screens"].get(s): print("SCREEN", s, a["screens"][s], "->", b["screens"].get(s)); diffs += 1
print("errors:", len(b["errors"]), "| differences:", diffs); sys.exit(1 if diffs else 0)
