import os
"""Camera sweep: every on-tap layout, the tapped POI placed across the screen, at 2 distances and 2 tilts; checks the
layout rules in each pose. usage: python sweep.py <index.html> [--verbose]"""
import asyncio, json, os, pathlib, sys
from playwright.async_api import async_playwright
JS = r"""(args) => {
 const [li, pi, sx, sy, z, r, yaw] = args, L = D.tap_layouts[li], p = D.pois[pi];
 ui.cam = camPoseFor(p.pos, sx, sy, z, r, yaw || 0, 0); ui.tapAnchor = null;
 ui.appPoi = pi; ui.appStep = "tapped"; ui.previewLayout = L.key; tapState.view = null; ui.camMoving = false; applyCam(); paintAR(true);
 const pr = dev.scr.getBoundingClientRect(), b = 0;
 const scr = { l: pr.left + b - 1, r: pr.right - b + 1, t: pr.top + b - 1, btm: pr.bottom - b + 1 };
 const tl = document.querySelector(".ph .tapc"); if (!tl) return { err: "no tap layer" };
 const gl = document.querySelector(".ph .glue .tapc"), R = el => el.getBoundingClientRect(), q = (s) => [...tl.querySelectorAll(s), ...(gl ? gl.querySelectorAll(s) : [])];
 const tag = q(":scope > .t-label, :scope > .t-chip, :scope > .t-card, :scope > .t-imm, :scope > .t-pan .tx, :scope > .t-pan .im").map(R);
 const btns = q(":scope > .a-rad .c, :scope > .a-cap, :scope > .irow, :scope > .a-dock").map(R);
 const card = q(":scope > .s-card").map(R);
 const dot = tl.querySelector(".f-dot") || gl?.querySelector(".f-dot"), d = dot ? R(dot) : null, P = d ? { x: (d.left + d.right) / 2, y: (d.top + d.bottom) / 2 } : null;
 const inside = rc => rc.left >= scr.l && rc.right <= scr.r && rc.top >= scr.t && rc.bottom <= scr.btm;
 const area = rc => Math.max(0, rc.right - rc.left) * Math.max(0, rc.bottom - rc.top);
 const ov = (A, B) => { const w = Math.min(A.right, B.right) - Math.max(A.left, B.left), hh = Math.min(A.bottom, B.bottom) - Math.max(A.top, B.top); return w > 0 && hh > 0 ? w * hh / Math.min(area(A), area(B)) : 0; };
 const covers = (rc, pt) => pt && pt.x > rc.left + 2 && pt.x < rc.right - 2 && pt.y > rc.top + 2 && pt.y < rc.bottom - 2;
 const v = [];
 [...tag, ...btns, ...card].forEach((rc, i) => { if (!inside(rc)) v.push("R1 off screen"); });
 tag.forEach(rc => { if (covers(rc, P)) v.push(gl && yaw ? "(info) side view: a glued tag passes in front of the POI" : "R2 tag covers the POI"); });
 for (let i = 0; i < btns.length; i++) for (let j = i + 1; j < btns.length; j++) if (ov(btns[i], btns[j]) > 0.2) v.push("R3 buttons overlap");
 btns.forEach(bb => tag.forEach(tt => { if (ov(bb, tt) > 0.2) v.push("R3 buttons overlap the tag"); }));
 btns.forEach(bb => { if (covers(bb, P) && bb.width < 80) v.push("R4 a button covers the POI"); });
 card.forEach(cc => [...tag, ...btns].forEach(x => { if (ov(x, cc) > 0.05) v.push("R5 hidden by the screen card"); }));
 card.forEach(cc => { if (covers(cc, P)) v.push("R6 the screen card covers the POI"); });
 return { v };
}"""
async def main(path, verbose):
    async with async_playwright() as p:
        kw = {"executable_path": "/usr/bin/google-chrome"} if os.path.exists("/usr/bin/google-chrome") else {}
        b = await p.chromium.launch(args=["--no-sandbox"], **kw)
        pg = await b.new_page(viewport={"width": 1440, "height": 900}); errs = []
        pg.on("pageerror", lambda e: errs.append(str(e)))
        await pg.goto(pathlib.Path(path).resolve().as_uri()); await pg.wait_for_timeout(600)
        if os.environ.get("SWEEP_INIT"): await pg.evaluate(os.environ["SWEEP_INIT"]); await pg.wait_for_timeout(200)   # e.g. a visitor profile: setProfile('visitors','children')
        await pg.click("#tab-app"); await pg.wait_for_timeout(300)
        n_lay = await pg.evaluate("D.tap_layouts.length"); names = await pg.evaluate("D.tap_layouts.map(l=>l.name)")
        poses = [(sx, sy, z, r, 0) for sx in (8, 30, 50, 70, 92) for sy in (15, 40, 65, 85) for z in (1.1, 2.5) for r in (0, 25)]
        side = [(sx, sy, 1.1, 0, yaw) for sx in (8, 30, 50, 70, 92) for sy in (15, 40, 65, 85) for yaw in (-35, 35)]
        table, examples, total = {}, {}, 0
        for d in ("phone_p", "phone_l", "tablet_p", "tablet_l"):
            await pg.evaluate(f"ui.device='{d}'; buildDevice(); renderPhone()")
            for li in range(n_lay):
                cnt = {}
                for pose in poses + (side if d == "phone_p" else []):
                    res = await pg.evaluate(JS, [li, 0, *pose]); total += 1
                    for k in set(res.get("v", [res.get("err")])) if (res.get("v") or res.get("err")) else []:
                        cnt[k] = cnt.get(k, 0) + 1; examples.setdefault((d, names[li], k), pose)
                table[f"{d:9s} {names[li]}"] = cnt
        # edge case: a big marker (3D model): distance to the marker 12 on every style, phone portrait
        await pg.evaluate("ui.device='phone_p'; buildDevice(); renderPhone(); D.tap_layouts.forEach(L => { L._g = L.gap; L.gap = 12; })")
        for li in range(n_lay):
            cnt = {}
            for pose in poses:
                res = await pg.evaluate(JS, [li, 0, *pose]); total += 1
                for k in set(res.get("v", [res.get("err")])) if (res.get("v") or res.get("err")) else []:
                    cnt[k] = cnt.get(k, 0) + 1; examples.setdefault(("gap12", names[li], k), pose)
            table[f"gap 12    {names[li]}"] = cnt
        await pg.evaluate("D.tap_layouts.forEach(L => { L.gap = L._g; delete L._g; }); renderPhone()")
        # off-screen behaviour, each mode
        off = {}
        for mode in ("pin", "close", "keep"):
            await pg.evaluate(f"D.app.tap.offscreen='{mode}'")
            res = await pg.evaluate(JS, [0, 0, 130, 40, 1.1, 0])
            state = await pg.evaluate("({step: ui.appStep, pin: !!document.querySelector('.ph .tapc .pin'), tag: !!document.querySelector('.ph .tapc .t-chip')})")
            ok = (mode == "pin" and state["pin"] and not state["tag"]) or (mode == "close" and state["step"] == "main") or (mode == "keep" and state["tag"] and not [x for x in res.get("v", []) if x.startswith("R1")])
            off[mode] = "ok" if ok else f"FAIL {state} {res.get('v')}"
        await pg.evaluate("D.app.tap.offscreen='pin'")
        await b.close()
    print(f"{total} poses (4 devices × {n_lay} styles × {len(poses)}, + {len(side)} side views, + {n_lay * len(poses)} with a big marker); page errors: {len(errs)}")
    for n, c in table.items(): print(f"  {n:28s} " + (", ".join(f"{k}: {v}" for k, v in sorted(c.items())) if c else "clean"))
    print("  off-screen:", off)
    for e in errs[:3]: print("  page error:", e[:200])
    if verbose:
        for (d, n, k), pose in examples.items(): print(f"    e.g. {d} · {n} · {k} at screen ({pose[0]}%, {pose[1]}%) distance {pose[2]} tilt {pose[3]}° side {pose[4]}°")
    return table, off
if __name__ == "__main__":
    asyncio.run(main(sys.argv[1], "--verbose" in sys.argv))
