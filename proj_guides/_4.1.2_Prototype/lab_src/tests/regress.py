"""Feature inventory of the World Editor Lab: run it on a build, get a JSON report; compare two reports.
usage: python regress.py <index.html> <report.json>"""
import asyncio, json, os, pathlib, sys
OPEN_ALL = """(()=>{const ids=new Set();for(let i=0;i<8;i++){const hs=[...document.querySelectorAll('#edb .fold:not(.open) > .fh:not(.locked)')];if(!hs.length)break;hs.slice(0,60).forEach(h=>h.click())}})()"""
from playwright.async_api import async_playwright

async def main(page_path, out_path):
    R = {"errors": [], "checks": {}, "tabs": {}, "screens": {}}
    def check(name, ok, info=""): R["checks"][name] = {"ok": bool(ok), "info": info}
    async with async_playwright() as p:
        b = await p.chromium.launch(**({"executable_path": "/usr/bin/google-chrome"} if os.path.exists("/usr/bin/google-chrome") else {}), args=["--no-sandbox"])
        pg = await b.new_page(viewport={"width": 1440, "height": 900})
        pg.on("console", lambda m: R["errors"].append(m.text) if m.type == "error" else None)
        pg.on("pageerror", lambda e: R["errors"].append("PAGEERR " + str(e)))
        await pg.goto(pathlib.Path(page_path).resolve().as_uri()); await pg.wait_for_timeout(600)
        js = lambda s: pg.evaluate(s)
        default_mode = await js("D.app.appearance"); await js("D.app.appearance='follow'")   # the tests switch the preview between the designs
        # the whole wall on a landscape tablet, seen straight on
        await pg.evaluate("ui.device='tablet_l'; buildDevice(); ui.cam={cx:.5,cy:.5,z:Math.min(dev.SW/IMA,dev.SH)/dev.base,r:0,yaw:0,pitch:0}; camChanged(false)"); await pg.wait_for_timeout(300)
        async def click_wall(x, y):   # wall px (640 × 441.5) → through the camera → a click
            c = await pg.evaluate(f"wallToClient({x},{y})"); await pg.mouse.click(c["x"], c["y"])
        async def nonblank():
            return await js("(()=>{const c=document.getElementById('cv'),x=c.getContext('2d'),d=x.getImageData(0,0,c.width,c.height).data;let s=0,s2=0,n=0;for(let i=0;i<d.length;i+=4000){s+=d[i];s2+=d[i]*d[i];n++}const m=s/n;return s2/n-m*m>50})()")
        async def headers():
            return await js("[...document.querySelectorAll('#edb .fh .ft, #edb .grp, #edb .poih .ft, #edb .dcard h4')].map(e=>e.textContent.trim())")
        async def editor_tab(tid, name):
            await pg.click(tid); await pg.wait_for_timeout(250)
            R["tabs"][name] = {"headers": await headers(), "rows": await js("document.querySelectorAll('#edb .urow').length")}
        # editor tabs (default: every section collapsed)
        check("every editor section starts collapsed", await js("(()=>{document.querySelector('#tab-global').click();return document.querySelectorAll('#edb .fold.open').length===0})()"))
        for tid, name in [("#tab-app", "App"), ("#tab-global", "Global"), ("#tab-specific", "Specific"), ("#tab-exp", "Experiences"), ("#tab-lab", "Decisions")]:
            await editor_tab(tid, name)
        # screens
        await pg.click("#tab-global")
        for s in ["wall", "style", "precision", "life"]:
            await pg.click(f"#stabs button[data-screen={s}]"); await pg.wait_for_timeout(500)
            R["screens"][s] = {"nonblank": await nonblank(), "hud": await js("document.getElementById('hud').textContent")}
        await pg.click("#stabs button[data-screen=app]"); await pg.wait_for_timeout(300)
        R["screens"]["app"] = {"phone": await js("!!document.querySelector('.ph')"), "device buttons": await js("document.querySelectorAll('#devbtns button').length")}
        # features (camera back to the whole wall: the Style and Precision screens frame their own view)
        await pg.evaluate("ui.cam={cx:.5,cy:.5,z:Math.min(dev.SW/IMA,dev.SH)/dev.base,r:0,yaw:0,pitch:0}"); 
        await pg.click("#stabs button[data-screen=wall]"); await pg.click("#tab-global"); await pg.wait_for_timeout(500)
        await pg.evaluate("cancelAnimationFrame(camAnim); ui.cam={cx:.5,cy:.5,z:Math.min(dev.SW/IMA,dev.SH)/dev.base,r:0,yaw:0,pitch:0}; camChanged(false)")
        await pg.evaluate("ui.open['styles']=true; renderEditor()"); await pg.wait_for_timeout(200)
        check("region styles = 9", await js("document.querySelectorAll('#edb canvas.sw').length") == 9)
        check("precision chips in cm", (await js("document.querySelector('[data-chip]').textContent")).startswith("hides"))
        await pg.evaluate("ui.open['style_body']=true; renderEditor()"); await pg.evaluate(OPEN_ALL)
        check("fill modes = 6", await js("(()=>{ui.open['st_fill']=true;renderEditor();return [...document.querySelectorAll('#edb select')].find(s=>[...s.options].some(o=>o.value==='media'))?.options.length})()") == 6)
        check("focus presets include appear", await js("[...document.querySelectorAll('#edb select option')].some(o=>o.value==='appear') || (ui.open['st_motion']=true,renderEditor(),[...document.querySelectorAll('#edb select option')].some(o=>o.value==='appear'))"))
        await pg.evaluate("ui.open['levels']=true; renderEditor()")
        check("levels layers P R T M", await js("document.querySelectorAll('#edb input[type=checkbox][title], #edb label[title] input').length") >= 12)
        check("categories editable", await js("(()=>{ui.open['cats']=true;renderEditor();return document.querySelectorAll('#edb input[type=color]').length>=3})()"))
        # specific: draw a polygon, undo
        await pg.click("#tab-specific"); await pg.evaluate("setScreen('wall');ui.cam=camCover({cx:.2,cy:.6,z:1,r:0,yaw:0,pitch:0});camChanged(false)"); await pg.wait_for_timeout(200)
        n0 = await js("D.pois[0].world.shapes.length")
        await pg.click("#tools button[data-tool=polygon]")
        for x, y in [(40, 250), (110, 250), (110, 320)]: await click_wall(x, y)
        await pg.keyboard.press("Enter"); await pg.wait_for_timeout(200)
        check("draw a polygon adds a shape", await js("D.pois[0].world.shapes.length") == n0 + 1)
        await pg.click("#undo"); await pg.wait_for_timeout(200)
        check("undo removes it", await js("D.pois[0].world.shapes.length") == n0)
        await pg.evaluate(OPEN_ALL)
        check("shape media select for the door", await js("(()=>{ui.poi=0;ui.shape=1;ui.open['shape']=true;renderEditor();return [...document.querySelectorAll('#edb select option')].some(o=>o.value==='media_1')})()"))
        check("content for actions: model moments", await js("(()=>{ui.tab='specific';ui.open['poi_0']=true;ui.open['acts_0']=true;renderEditor();return [...document.querySelectorAll('#edb input')].some(i=>i.value==='1755: the earthquake')})()"))
        await pg.wait_for_timeout(600)   # let the section's framing animation finish first
        await pg.evaluate("setScreen('wall'); ui.cam={cx:.5,cy:.5,z:Math.min(dev.SW/IMA,dev.SH)/dev.base,r:0,yaw:0,pitch:0}; camChanged(false)"); await pg.wait_for_timeout(500)   # opening a section shows its demo (and frames its POI); back to the whole wall for Play
        # play: tap the twin-tower church (quiet style appears), door media, sheet text by tap result
        await pg.click("#play"); await pg.wait_for_timeout(1600)
        await click_wall(536, 300); await pg.wait_for_timeout(700)
        check("tap shows the app default style on the wall (Immersive, buttons inside)", await js("!!document.querySelector('#tapwall .t-imm') && document.querySelectorAll('#tapwall .t-imm .irow .i').length >= 3"))
        check("focus set on the church", await js("focus") == 0)
        await click_wall(20, 30); await pg.wait_for_timeout(300)
        check("tap wall deselects", await js("focus") is None)
        # wall life
        await pg.click("#stabs button[data-screen=life]"); await pg.wait_for_timeout(1500)
        check("wall life kinds K1..K6 present", sorted(await js("allContent().map(o=>KINDS[o.c.kind].code)")) == ["K1", "K2", "K3", "K4", "K5", "K6"])
        await click_wall(579, 131); await pg.wait_for_timeout(1200)
        check("shutters open on focus", await js("lifeState(allContent().find(o=>o.c.kind==='hinged'), performance.now()/1000).env") > 0.5)
        await pg.click("#play")
        # app
        await pg.evaluate("ui.device='phone_p'; buildDevice(); ui.cam={...CAM0}; ui.appStep='main'; ui.compPreview=null")
        await pg.click("#tab-app"); await pg.wait_for_timeout(300)
        check("app main: edge arrow", await js("document.querySelectorAll('.ph .edge').length") >= 1)
        await pg.locator(".ph .mk").nth(0).click(); await pg.wait_for_timeout(800)
        check("app tapped: Hub uses the app default (Immersive)", await js("!!document.querySelector('.ph .t-imm')"))
        check("a level can pick its own style (Detail → Minimal)", await js("tapStyleOf(D.pois.find(p=>levelOf(p).key==='level_3')).key==='tap_layout_1'"))
        check("radial: 4 buttons + more", await js("(()=>{ui.previewLayout='tap_layout_2';renderPhone();const n=document.querySelectorAll('.ph .a-rad').length;ui.previewLayout=null;renderPhone();return n})()") == 5)
        check("app tapped: the tag is shown", await js("!!document.querySelector('.ph .t-imm, .ph .t-chip')"))
        for k, sel_ in [("tap_layout_1", ".a-cap"), ("tap_layout_3", ".t-imm"), ("tap_layout_4", ".t-pan"), ("tap_layout_5", ".tiles"), ("tap_layout_6", ".s-card .ban")]:
            await pg.evaluate(f"(()=>{{ui.previewLayout='{k}';renderPhone();}})()"); await pg.wait_for_timeout(300)
            check(f"layout {k} renders {sel_}", await js(f"!!document.querySelector('.ph {sel_}')"))
        for a_, sel_ in [("pano", ".pano"), ("then_now", ".wipe"), ("model_wall", ".cube"), ("card", ".tv")]:
            await pg.evaluate(f"tapAction(D.pois[0], '{a_}', renderPhone)"); await pg.wait_for_timeout(150)
            check(f"action view {a_}", await js(f"!!document.querySelector('.ph {sel_}')"))
            await pg.evaluate("tapState.view=null; renderPhone()")
        await pg.evaluate("tapAction(D.pois[0], 'favourite', renderPhone)")
        check("favourite toggles", await js("tapState.favs['twin_tower_church'] === true"))
        await pg.wait_for_timeout(2100)   # let the toast's re-render pass first
        await pg.evaluate("ui.previewLayout=null; ui.compPreview=null; ui.appPoi=0; ui.cam={...CAM0}; tapState.view=null; demoLayout('tap_layout_3')"); await pg.wait_for_timeout(120)
        await pg.wait_for_timeout(500)   # the demo first frames the POI (an editor convenience), then the finger taps
        check("demo: a finger taps the POI", await js("!!document.querySelector('.ph .tapfx')"), "")
        await pg.wait_for_timeout(1500)
        check("demo: then the layout opens", await js("!!document.querySelector('.ph .t-imm')"))
        await pg.evaluate("ui.open['app_tap']=true; ui.open['tap_styles']=true; renderEditor()")
        check("layouts are one foldout each", await js("document.querySelectorAll('#edb .fold[data-step=tapped] > .fh').length") >= await js("D.tap_layouts.length"))
        await pg.evaluate("ui.appStep='explore'; renderPhone()"); await pg.wait_for_timeout(200)
        check("app explore lists tours+games+moments", await js("document.querySelectorAll('.ph .ds-explore .ds-row').length") == 6)
        await pg.evaluate("ui.open['app_ds']=true; ui.open['ds_look']=true; renderEditor()"); await pg.wait_for_timeout(100)
        await pg.evaluate("setProfile('visitors','children')"); await pg.wait_for_timeout(200)
        check("profile: Kids opens the Playful look (its own tokens over Premium)", await js("activeLook()") == "playful" and await js("dev.scr.classList.contains('look-playful') && getComputedStyle(dev.scr).getPropertyValue('--ts').trim()==='1.063'"))
        await pg.evaluate("setProfile('visitors','adults'); ui.compPreview=null; renderEditor()")
        # camera: the photo media, off-screen modes, the safe area
        await pg.evaluate("ui.appStep='tapped'; ui.appPoi=D.pois.findIndex(p=>p.id==='castle_hill'); tapAction(D.pois[ui.appPoi], 'then_now', renderPhone)"); await pg.wait_for_timeout(200)
        check("then & now uses the castle photo", await js("!!document.querySelector('.ph .wipe') && !document.querySelector('.ph .wipe.nophoto')"))
        await pg.evaluate("ui.compPreview=null; tapState.view=null; ui.appStep='tapped'; ui.appPoi=0; D.app.tap.offscreen='pin'; ui.cam={cx:0.1,cy:0.5,z:1.1,r:0}; renderPhone()"); await pg.wait_for_timeout(300)
        check("off-screen POI shrinks to a pin", await js("!!document.querySelector('.ph .pin')"))
        check("the pin stays inside the phone", await js("(()=>{const a=document.querySelector('.ph .pin').getBoundingClientRect(),b=document.querySelector('.ph').getBoundingClientRect();return a.left>=b.left&&a.right<=b.right&&a.top>=b.top&&a.bottom<=b.bottom})()"))
        await pg.evaluate("ui.cam={cx:0.84,cy:0.5,z:1.1,r:0}; renderPhone()")
        await pg.evaluate("ui.tab='app'; ui.open['app_main']=true; ui.open['app_point']=true; renderEditor()")
        check("editor has the tap behaviour rows", await js("[...document.querySelectorAll('#edb .urow label')].some(l=>l.textContent.includes('leaves the screen'))"))
        # devices, side views, glued parts, pointing
        for d in ["phone_l", "tablet_p", "tablet_l", "phone_p"]:
            await pg.evaluate(f"ui.device='{d}'; buildDevice(); renderPhone()"); await pg.wait_for_timeout(150)
            r = await js("(()=>{const r=dev.scr.getBoundingClientRect();return r.width/r.height})()"); want = await js(f"DEVICES['{d}'].w/DEVICES['{d}'].h")
            check(f"device {d} has its screen shape", abs(r - want) < 0.01, f"{r:.3f} vs {want:.3f}")
        check("unproject inverts the projection (side view)", await js("(()=>{ui.cam={cx:.5,cy:.45,z:1.3,r:12,yaw:35,pitch:-15}; applyCam(); const c=wallToClient(300,200), u=unproject(c.x,c.y); return Math.hypot(u.x-300,u.y-200)<0.5})()"))
        await pg.evaluate("ui.cam={...CAM0}; ui.appStep='tapped'; ui.appPoi=0; tapState.view=null; ui.previewLayout='tap_layout_4'; ui.tapAnchor=null; camChanged(false)"); await pg.wait_for_timeout(300)
        check("floating panels are glued to the wall", await js("!!document.querySelector('.ph .glue .t-pan') && !document.querySelector('.ph .ui .t-pan')"))
        g0 = await js("(()=>{const r=document.querySelector('.ph .glue .t-pan .tx').getBoundingClientRect();return [r.left,r.top]})()")
        await pg.evaluate("ui.cam.cx += 0.05; camChanged(false)"); await pg.wait_for_timeout(200)
        g1 = await js("(()=>{const r=document.querySelector('.ph .glue .t-pan .tx').getBoundingClientRect();return [r.left,r.top]})()")
        m0 = await js("wallToClient(D.pois[0].pos.x, D.pois[0].pos.y)")
        check("glued parts move with the wall", abs(g1[0] - g0[0]) > 5, f"{g0} → {g1}")
        await pg.evaluate("ui.cam={...CAM0, yaw:45}; camChanged(false)"); await pg.wait_for_timeout(200)
        par = await js("(()=>{const a=wallToClient(D.pois[0].pos.x,D.pois[0].pos.y), q=camProject(D.pois[0].pos.x,D.pois[0].pos.y,ui.cam,10), r=dev.scr.getBoundingClientRect(); return Math.abs(a.x-(r.left+q.x/100*r.width))})()")
        check("from the side, 10 cm off the wall shows parallax", par > 8, f"{par:.1f}px")
        await pg.evaluate("ui.cam={...CAM0}; ui.previewLayout=null; ui.appStep='main'; D.app.pointing.others='top'; D.app.pointing.max=1; renderPhone()"); await pg.wait_for_timeout(200)
        n1 = await js("document.querySelectorAll('.ph .edge').length")
        await pg.evaluate("D.app.pointing.others='all'; renderPhone()"); await pg.wait_for_timeout(200)
        nall = await js("document.querySelectorAll('.ph .edge').length")
        await pg.evaluate("D.app.pointing.others='off'; renderPhone()"); await pg.wait_for_timeout(200)
        n0 = await js("document.querySelectorAll('.ph .edge').length")
        check("pointing: off / best 1 / all", n0 == 0 and n1 == 1 and nall >= 2, f"{n0}/{n1}/{nall}")
        await pg.evaluate("D.app.pointing.others='top'; D.app.pointing.max=3; renderPhone()")
        check("the castle uses its photos; the other POIs their own tile crop (no wrong photo)", await js("D.pois.filter(p=>p.media&&p.media.hero).map(p=>p.id).join()==='castle_hill'"))
        jg = await js("jsonGlobal()")
        check("json preview has the on-tap components", all(k in jg for k in ["tag_components", "action_components", "button_look", "default_style"]))
        # v12: defaults, components, overrides, buttons from content, distance to the marker, device buttons, sticky titles
        await pg.evaluate("ui.device='phone_p'; buildDevice(); ui.cam={...CAM0}; ui.appStep='main'; renderPhone()")
        await pg.click("#devbtns button[data-dev=phone]"); await pg.wait_for_timeout(150)
        d1 = await js("ui.device"); await pg.click("#devbtns button[data-dev=tablet]"); await pg.wait_for_timeout(150)
        d2 = await js("ui.device"); await pg.click("#devbtns button[data-dev=tablet]"); await pg.wait_for_timeout(150)
        d3 = await js("ui.device"); await pg.click("#devbtns button[data-dev=phone]"); await pg.wait_for_timeout(150)
        check("device buttons: click again turns it", (d1, d2, d3) == ("phone_l", "tablet_p", "tablet_l"), f"{d1} {d2} {d3}")
        check("no side panel next to the device", await js("!document.querySelector('.dside')"))
        check("a level with no style uses the app default", await js("(()=>{const l=D.levels[2],k=l.tap_layout;l.tap_layout='';const s=tapStyleOf(D.pois.find(p=>levelOf(p)===l)||{level:l.key}).key;l.tap_layout=k;return s===D.app.tap.default_layout})()"))
        check("region style falls back to the app default", await js("(()=>{const p=D.pois[0],S=p.world.shapes[0],a=[S.style,p.world.style_override,levelOf(p).region_style];S.style='';p.world.style_override='';levelOf(p).region_style='';const k=styleKeyFor(p,S);[S.style,p.world.style_override,levelOf(p).region_style]=a;return k===D.region_default})()"))
        check("POI field override: castle gap 6, rest inherited", await js("(()=>{const c=D.pois.find(p=>p.id==='castle_hill'),L=tapLayoutOf(c);return L.gap===6&&L.key===tapStyleOf(c).key})()"))
        check("buttons come from content (no model → no 3D)", await js("(()=>{const g=D.pois.find(p=>p.id==='arched_gate'),t=D.pois[0];return !poiActions(g).includes('model_wall')&&poiActions(t).includes('model_wall')})()"))
        check("a POI can hide a button (castle: no Save)", await js("!poiActions(D.pois.find(p=>p.id==='castle_hill')).includes('favourite')"))
        gap = await js("""(()=>{const meas=g=>{const L=D.tap_layouts[1],o=L.gap;L.gap=g;ui.appStep='tapped';ui.appPoi=0;ui.previewLayout=L.key;ui.cam=camPoseFor(D.pois[0].pos,50,40,1.1);renderPhone();
          const t=document.querySelector('.ph .ui .t-chip').getBoundingClientRect(),c=wallToClient(D.pois[0].pos.x,D.pois[0].pos.y);L.gap=o;return Math.hypot((t.left+t.right)/2-c.x,(t.top+t.bottom)/2-c.y)};return [meas(0),meas(12)]})()""")
        check("distance to the marker moves the tag away", gap[1] > gap[0] + 15, f"{gap[0]:.0f} → {gap[1]:.0f} px")
        check("tag component without picture", await js("(()=>{const T=D.tap_tags[3],m=T.media;T.media='none';ui.previewLayout='tap_layout_3';renderPhone();const e=document.querySelector('.ph .t-imm');const ok=e&&!e.style.backgroundImage.includes('url');T.media=m;return ok})()"))
        await pg.evaluate("ui.previewLayout=null; ui.appStep='main'; ui.cam={...CAM0, cx:0.3}; D.app.pointing.others='all'; D.app.pointing.how_far='m'; renderPhone()"); await pg.wait_for_timeout(200)
        check("pointing: how far in metres", await js("[...document.querySelectorAll('.ph .edge small')].some(s=>/\\d m$/.test(s.textContent))"))
        await pg.evaluate("D.app.pointing.how_far='deg'; renderPhone()")
        check("pointing: how far in degrees", await js("[...document.querySelectorAll('.ph .edge small')].some(s=>/°$/.test(s.textContent))"))
        await pg.evaluate("D.app.pointing.how_far='off'; D.app.pointing.others='top'; ui.cam={...CAM0}; ui.tab='app'; ui.open['app_tap']=true; ui.open['tap_styles']=true; ui.open['tapL_tap_layout_2']=true; renderEditor(); document.querySelector('#edb').scrollTop=500")
        await pg.wait_for_timeout(150)
        # v12.1: component previews, the marker stays put, readable panels, numbered sections
        await pg.evaluate("ui.compPreview={type:'buttons'}; ui.appStep='tapped'; ui.appPoi=0; ui.cam={...CAM0}; renderPhone()"); await pg.wait_for_timeout(150)
        check("base elements preview: every button in the gallery", await js("document.querySelectorAll('.ph .gal .galg .gi').length===D.tap_actions.length"))
        ok = True
        for t, cls in [("tag_comp_1", "t-label"), ("tag_comp_2", "t-chip"), ("tag_comp_3", "t-card"), ("tag_comp_4", "t-imm"), ("tag_comp_5", "t-pan")]:
            ok = ok and await js(f"(()=>{{ui.compPreview={{type:'tag',key:'{t}'}};renderPhone();return !!document.querySelector('.ph .{cls}')}})()")
        check("every tag component has its own preview", ok)
        ok = True
        for t, cls in [("act_comp_1", "a-rad"), ("act_comp_2", "a-cap"), ("act_comp_3", "irow"), ("act_comp_4", "a-dock"), ("act_comp_5", "tiles")]:
            ok = ok and await js(f"(()=>{{ui.compPreview={{type:'act',key:'{t}'}};renderPhone();return !!document.querySelector('.ph .{cls}')}})()")
        check("every action component has its own preview", ok)
        check("icons inside a compact card are full size", await js("(()=>{ui.compPreview={type:'act',key:'act_comp_3'};renderPhone();const s=document.querySelector('.ph .t-card .irow .i svg').getBoundingClientRect();return s.width>8})()"))
        mk = await js("(()=>{ui.compPreview=null;ui.previewLayout='tap_layout_4';ui.tapAnchor=null;ui.cam={...CAM0};renderPhone();ui.cam.yaw=45;camChanged(false);const d=document.querySelector('.ph .ui .f-dot').getBoundingClientRect(),c=wallToClient(D.pois[0].pos.x,D.pois[0].pos.y);return Math.hypot((d.left+d.right)/2-c.x,(d.top+d.bottom)/2-c.y)})()")
        check("distance from the wall never moves the marker", mk < 3, f"{mk:.1f}px")
        check("floating panel has a glass backing", await js("(()=>{const e=document.querySelector('.ph .t-pan .tx');const c=getComputedStyle(e);return c.backdropFilter.includes('blur')&&c.backgroundImage.includes('gradient')})()"))
        await pg.evaluate("ui.previewLayout=null; ui.cam={...CAM0}; renderPhone()")
        check("App sections are numbered 0-7", await js("(()=>{ui.tab='app';renderEditor();const t=[...document.querySelectorAll('#edb > .fold > .fh .ft')].map(e=>e.textContent);return t.length===8&&t.every((x,i)=>x.startsWith(i+' ·'))})()"))
        await pg.evaluate("ui.open['app_tap']=true; ui.open['tap_styles']=true; ui.open['tapL_tap_layout_2']=true; renderEditor(); document.querySelector('#edb').scrollTop=500")
        # v12.2: glass controls, buttons inside a tag, the gallery row on the wall, defaults inside their sections
        # two designs: dark and light, each with its own tokens
        g = await js("(()=>{ui.compPreview={type:'tag',key:'tag_comp_2'};ui.appStep='tapped';ui.appearance='dark';renderPhone();const e=()=>document.querySelector('.ph .t-chip');const a=[getComputedStyle(e()).backgroundColor,getComputedStyle(e()).color];ui.appearance='light';renderPhone();const b=[getComputedStyle(e()).backgroundColor,getComputedStyle(e()).color];ui.appearance='dark';renderPhone();return [a,b]})()")
        check("dark and light designs give different tags", g[0][0] != g[1][0] and g[0][1] != g[1][1], str(g))
        bt = await js("(()=>{ui.compPreview=null;ui.previewLayout='tap_layout_2';ui.appearance='light';renderPhone();const T=D.themes.light.buttons,o=[T.color,T.tint];T.color='#FF0000';T.tint=1;renderPhone();const c=getComputedStyle(document.querySelector('.ph .a-rad .c')).backgroundColor;[T.color,T.tint]=o;ui.appearance='dark';ui.previewLayout=null;renderPhone();return c})()")
        check("a light-design button colour reaches the buttons", bt == "rgb(255, 0, 0)", bt)
        check("the dark design is untouched by a light edit", await js("(()=>{ui.previewLayout='tap_layout_2';renderPhone();const c=getComputedStyle(document.querySelector('.ph .a-rad .c')).backgroundColor;ui.previewLayout=null;renderPhone();return c!=='rgb(255, 0, 0)'})()"))
        check("glass blur reaches the surfaces", await js("(()=>{ui.compPreview={type:'tag',key:'tag_comp_2'};D.themes.dark.glass.blur=30;renderPhone();const r=getComputedStyle(document.querySelector('.ph .t-chip')).backdropFilter.includes('30px');D.themes.dark.glass.blur=22;ui.compPreview=null;renderPhone();return r})()"))
        check("screen card, CTA and wall labels follow the design", await js("(()=>{ui.previewLayout='tap_layout_5';ui.appearance='light';renderPhone();const s=getComputedStyle(document.querySelector('.ph .s-card')).backgroundColor,c=getComputedStyle(document.querySelector('.ph .tcta')).backgroundColor,l=getComputedStyle(document.querySelector('.ph .mk .lbl')).color;ui.appearance='dark';ui.previewLayout=null;renderPhone();return s.startsWith('rgba(248, 246, 242')&&c==='rgb(30, 58, 138)'&&l==='rgb(20, 17, 13)'})()"))
        check("'Always dark' locks the app to the dark design", await js("(()=>{D.app.appearance='dark';ui.appearance='light';renderPhone();const ok=dev.scr.classList.contains('dark');D.app.appearance='follow';ui.appearance='dark';renderPhone();return ok})()"))
        bi = await js("(()=>{const T=D.tap_tags[2],o=T.buttons;ui.compPreview={type:'tag',key:T.key};T.buttons=false;renderPhone();const a=document.querySelectorAll('.ph .t-card .irow .i').length;T.buttons=true;renderPhone();const b=document.querySelectorAll('.ph .t-card .irow .i').length;T.buttons=o;return [a,b]})()")
        check("a tag component can show the buttons inside", bi[0] == 0 and bi[1] >= 3, str(bi))
        check("base elements gallery has a row straight on the wall", await js("(()=>{ui.compPreview={type:'buttons'};renderPhone();return document.querySelectorAll('.ph .gal .galw .gi').length===4})()"))
        check("the default style lives in On-tap styles", await js("(()=>{ui.tab='app';ui.open['app_tap']=true;ui.open['tap_styles']=true;renderEditor();const f=[...document.querySelectorAll('#edb .fold')].find(x=>x.querySelector(':scope > .fh .ft')?.textContent==='On-tap styles');return !!f&&f.querySelector('.fb').textContent.includes('Default for the whole app')})()"))
        check("floating panel post is visible", await js("(()=>{ui.compPreview={type:'tag',key:'tag_comp_5'};renderPhone();const l=document.querySelector('.ph .t-pan .ln');return l.getBoundingClientRect().width>=1.5})()"))
        await pg.evaluate("ui.compPreview=null; renderPhone()")
        # v12.3: the room fills the screen, tablet scale, the needle stands on the marker, one type family
        check("the room around the wall fills the screen", await js("(()=>{const r=document.querySelector('.ph .room');return !!r&&r.style.backgroundImage.startsWith('url(')})()"))
        check("tablet UI unit is 1.2× the phone's", await js("(()=>{ui.device='tablet_p';buildDevice();const t=dev.U/dev.SW*820;ui.device='phone_p';buildDevice();renderPhone();const ph=dev.U/dev.SW*390;return Math.abs(t/ph-1.2)<0.01})()"))
        nd = await js("(()=>{ui.compPreview=null;ui.previewLayout='tap_layout_4';ui.appStep='tapped';ui.appPoi=0;ui.tapAnchor=null;ui.cam={...CAM0};renderPhone();ui.cam.yaw=45;camChanged(false);const n=document.querySelector('.ph .glue .needle'),c=wallToClient(D.pois[0].pos.x,D.pois[0].pos.y);if(!n)return 999;const r=n.getBoundingClientRect();return Math.min(Math.hypot(r.left-c.x,r.top-c.y),Math.hypot(r.left-c.x,r.bottom-c.y),Math.hypot(r.right-c.x,r.top-c.y),Math.hypot(r.right-c.x,r.bottom-c.y))})()")
        check("side view: the needle stands on the marker", nd < 6, f"{nd:.1f}px")
        check("visitor UI uses no monospace", await js("(()=>{ui.cam={...CAM0};ui.previewLayout='tap_layout_2';renderPhone();return [...document.querySelectorAll('.ph .tapc *')].every(e=>!/mono|Courier/i.test(getComputedStyle(e).fontFamily))})()"))
        await pg.evaluate("ui.previewLayout=null; renderPhone()")
        # v13: the Main AR screen (search, lenses, mini-map, near chip, buttons, Azu, coach mark, out-of-view hint), Pointing inside it
        await pg.evaluate("ui.device='phone_p'; buildDevice(); ui.cam={...CAM0}; ui.coachDone=false; ui.lens='all'; ui.appStep='main'; renderPhone()"); await pg.wait_for_timeout(150)
        check("main defaults are calm: search with 'Near …', bar with Azu; no chips, no map", await js("(()=>{const q=s=>document.querySelector('.ph .mainc '+s);return q('.msearch .ph1 i')?.textContent.startsWith('Near')&&q('.mbar .mazu.mascot')&&!q('.mlens')&&!q('.mmap')})()"))
        check("main: the filter button opens the lenses", await js("(()=>{document.querySelector('.ph .msearch .mi.hit').click();const a=!!document.querySelector('.ph .mlens .lchip');ui.lensOpen=false;renderPhone();return a})()"))
        check("main: every part can be shown (chips, chip, map)", await js("(()=>{const M=D.app.main,k=JSON.stringify(M);M.lenses.at='search';M.near='chip';M.minimap.at='bottom';renderPhone();const ok=['.mlens .lchip','.mnear','.mmap .vw'].every(s=>document.querySelector('.ph .mainc '+s));Object.assign(M,JSON.parse(k));renderPhone();return ok})()"))
        check("main: the coach mark sits on a POI", await js("(()=>{const c=document.querySelector('.ph .mcoach');if(!c)return false;const r=c.getBoundingClientRect(),n=nearPoi()||D.pois[0];const k=wallToClient(n.pos.x,n.pos.y);return Math.hypot(r.left-k.x,r.top-k.y)<4})()"))
        check("main: the coach mark goes after the first tap", await js("(()=>{tapPoiOnPhone(0);ui.appStep='main';renderPhone();return !document.querySelector('.ph .mcoach')})()"))
        check("main: a lens dims the other POIs", await js("(()=>{ui.cam={...CAM0,cx:0.6,z:0.5};camChanged(false);ui.lens='category_2';renderPhone();const mk=[...document.querySelectorAll('.ph .mk')];const ok=mk.some(m=>m.style.opacity==='0.25')&&mk.some(m=>m.style.opacity==='1');ui.lens='all';ui.cam={...CAM0};camChanged(false);return ok})()"))
        check("main: the mini-map window follows the camera", await js("(()=>{D.app.main.minimap.at='bottom';renderPhone();const a=document.querySelector('.ph .mmap .vw').style.left;ui.cam.cx-=0.2;camChanged(false);const b=document.querySelector('.ph .mmap .vw').style.left;ui.cam={...CAM0};D.app.main.minimap.at='lost';camChanged(false);return a!==b})()"))
        check("main: out of view → the hint and the mini-map, no pointers", await js("(()=>{ui.cam={...CAM0,cx:1.4};camChanged(false);const a=!!document.querySelector('.ph .mhint')&&!!document.querySelector('.ph .mmap.lost')&&!document.querySelector('.ph .edge');ui.cam={...CAM0};camChanged(false);return a&&!document.querySelector('.ph .mhint')})()"))
        check("main: corners layout keeps the bottom parts clear of the corner buttons", await js("(()=>{const B=D.app.main.buttons,S=D.app.main.search;B.arrange='corners';S.at='bottom';renderPhone();const s=document.querySelector('.ph .msearch').getBoundingClientRect(),c=document.querySelector('.ph .mc.bl').getBoundingClientRect();B.arrange='bar';S.at='top';renderPhone();return s.bottom<=c.top})()"))
        check("main: every part can be switched off", await js("(()=>{const M=D.app.main,k=JSON.stringify(M);M.search.at='off';M.lenses.at='off';M.minimap.at='off';M.near='off';M.buttons.azu='off';M.buttons.explore=false;M.hints.coach=false;renderPhone();const n=document.querySelectorAll('.ph .mainc .msearch,.ph .mainc .mlens,.ph .mainc .mmap,.ph .mainc .mnear,.ph .mainc .mazu,.ph .mainc .mcoach').length;Object.assign(M,JSON.parse(k));renderPhone();return n===0})()"))
        check("main: Pointing is a subsection of Main AR", await js("(()=>{ui.tab='app';ui.open['app_main']=true;renderEditor();const f=[...document.querySelectorAll('#edb > .fold')].find(x=>x.querySelector(':scope > .fh .ft').textContent.startsWith('4 ·'));return !!f.querySelector('.fold .fh .ft')&&[...f.querySelectorAll('.fold .fh .ft')].some(e=>e.textContent.startsWith('Pointing'))})()"))
        ok = True
        for d in ["phone_l", "tablet_p", "tablet_l"]:
            ok = ok and await js(f"(()=>{{ui.device='{d}';buildDevice();renderPhone();const p=dev.scr.getBoundingClientRect();return [...document.querySelectorAll('.ph .mainc .g')].every(e=>{{const r=e.getBoundingClientRect();return r.left>=p.left-1&&r.right<=p.right+1&&r.top>=p.top-1&&r.bottom<=p.bottom+1}})}})()")
        await pg.evaluate("ui.device='phone_p'; buildDevice(); renderPhone()")
        check("main: every part stays inside the screen on every device", ok)
        check("demo of a tiles component frames the POI so the card opens", await js("(()=>{ui.cam={...CAM0};ui.compPreview={type:'act',key:'act_comp_5'};ui.appStep='tapped';ui.appPoi=0;renderPhone();frameForDemo();return true})()"))
        await pg.wait_for_timeout(500)
        check("…and the tiles show", await js("(()=>{renderPhone();const ok=document.querySelectorAll('.ph .tiles .tile').length>=4&&!document.querySelector('.ph .s-card.tuck');ui.compPreview=null;ui.cam={...CAM0};renderPhone();return ok})()"))
        # v14: App › 1 Design system: tokens shared by every block, the UI kit, overrides
        check("Profiles then Design system open the App tab", await js("(()=>{ui.tab='app';renderEditor();const f=[...document.querySelectorAll('#edb > .fold > .fh .ft')].map(e=>e.textContent);return f[0]==='0 · Profiles'&&f[1]==='1 · Design system'})()"))
        check("opening it shows the UI kit", await js("(()=>{ui.open['app_ds']=false;renderEditor();document.querySelectorAll('#edb > .fold > .fh')[1].click();return !!document.querySelector('.ph .kit .kitp .kh')&&document.querySelectorAll('#edb .fold.open > .fb > .fold.hasdemo').length>=11})()"))
        tok = await js("""(()=>{const S=D.ds,o=JSON.stringify(S),pg=k=>{ui.compPreview={type:'kit',page:k};ui.appStep='tapped';renderPhone();},q=s=>getComputedStyle(document.querySelector('.ph .kit '+s));
          const read=()=>{pg('type');const a=[q('.ds-h1').fontSize,q('.ds-h1').fontFamily];pg('panels');a.push(q('.ds-panel').borderTopLeftRadius,q('.ds-panel').paddingTop,q('.ds-photo .scr').backgroundImage);pg('controls');a.push(q('.lchip').borderTopLeftRadius,q('svg').strokeWidth);pg('depth');a.push(q('.ktrack i').animationTimingFunction);return a};
          const a=read();S.text_size=1.15;S.titles='sans';S.corner=8;S.density=0.75;S.scrim=0.6;S.icon_stroke=2.25;S.scheme='expressive';const b=read();Object.assign(S,JSON.parse(o));ui.compPreview=null;renderPhone();return [a,b]})()""")
        check("tokens reach the components (text size, titles, corners, density, scrim, chip shape, icon line, motion style)", all(x != y for x, y in zip(tok[0], tok[1])), str(tok))
        check("design-system buttons used everywhere; On tap can override", await js("""(()=>{ui.compPreview=null;ui.previewLayout='tap_layout_2';ui.appStep='tapped';ui.appPoi=0;ui.cam=camPoseFor(D.pois[0].pos,50,40,1.1);D.ds.button.shape='circle';renderPhone();const a=document.querySelector('.ph .tapc').classList.contains('b-round');
          D.tap_button.use_ds=false;D.tap_button.shape='rounded';renderPhone();const b=document.querySelector('.ph .tapc').classList.contains('b-round');D.tap_button.use_ds=true;D.ds.button.shape='rounded';ui.previewLayout=null;renderPhone();return !a&&b})()"""))
        check("reduced motion removes the springs", await js("(()=>{D.ds.motion='reduced';ui.compPreview={type:'tag',key:'tag_comp_2'};renderPhone();const n=getComputedStyle(document.querySelector('.ph .t-chip')).animationName;D.ds.motion='full';ui.compPreview=null;renderPhone();return n==='mfade'})()"))
        check("the depth token sets the shadow", await js("(()=>{D.ds.depth=0;ui.compPreview={type:'kit'};renderPhone();const a=getComputedStyle(document.querySelector('.ph .kit .ds-panel')).boxShadow;D.ds.depth=1;renderPhone();const b=getComputedStyle(document.querySelector('.ph .kit .ds-panel')).boxShadow;ui.compPreview=null;renderPhone();return a!==b})()"))
        # v15: the Design system as a UI kit: one page per subsection, components, patterns; every editor section has a demo
        pages = await js("""(()=>{ui.tab='app';ui.open={app_ds:true};renderEditor();const r={};for(const f of [...document.querySelectorAll('#edb .fold.hasdemo[data-step]')].filter(f=>f.querySelector(':scope>.fh .ft')&&f.parentElement.closest('.fold')?.querySelector(':scope>.fh .ft')?.textContent==='1 · Design system')){const t=f.querySelector(':scope>.fh .ft').textContent;f.querySelector(':scope>.fh').click();r[t]=ui.compPreview?.page||null;ui.open={app_ds:true};renderEditor();}return r})()""")
        check("every Design system subsection opens its own page on the phone", len(pages) >= 11 and len(set(pages.values())) == len(pages) and None not in pages.values(), str(pages))
        check("Design system: foundations, components, patterns", await js("[...document.querySelectorAll('#edb .fold.open > .fb > .grp')].map(e=>e.firstChild.textContent).join('|')") == "Foundations|Components|Patterns")
        nav = await js("""(()=>{const r=[];runDemo(kitDemo('nav'),true);const q=s=>document.querySelector('.ph '+s);q('.ds-tab:nth-of-type(2)').click();r.push(q('.ds-top b').textContent);q('.ds-view .ds-row').click();r.push(q('.ds-top b').textContent,!!q('.ds-back'));q('.ds-back').click();r.push(q('.ds-top b').textContent);q('.ds-tab:nth-of-type(1)').click();r.push(!!q('.ds-app.onwall'));return r})()""")
        check("nav bar: tabs switch, a row pushes a page, back returns", nav == ["Explore", "Tours", True, "Explore", True], str(nav))
        check("nav bar settings: destinations, labels, indicator, rail in landscape", await js("""(()=>{const N=D.ds.nav,o=JSON.stringify(N);N.tabs='5';N.labels='off';N.style='underline';renderPhone();const a=document.querySelectorAll('.ph .ds-tab').length===5&&getComputedStyle(document.querySelector('.ph .ds-tab .tl')).display==='none'&&document.querySelector('.ph .ds-nav.ind-underline');
          ui.device='phone_l';buildDevice();renderPhone();const b=!!document.querySelector('.ph .ds-nav.rail');ui.device='tablet_l';buildDevice();Object.assign(N,JSON.parse(o));renderPhone();return a&&b})()"""))
        sh = await js("""(()=>{const K=kitS();K.sheet=null;K.dialog=false;runDemo(kitDemo('sheets'),true);const q=s=>document.querySelector('.ph '+s);const a=q('.ds-sheet').className;q('.ds-sheet .grab').click();const b=q('.ds-sheet').className;[...document.querySelectorAll('.ph .ds-trig .ds-btn')][1].click();const c=!!q('.ds-dialog');q('.ds-dialog .ds-btn.main').click();return [a,b,c,!q('.ds-dialog')]})()""")
        check("sheet: three heights from the grabber; a dialog opens and closes", sh == ["ds-sheet d-half", "ds-sheet d-full", True, True], str(sh))
        check("controls respond (chip, segment, switch, search)", await js("""(()=>{runDemo(kitDemo('controls'),true);const q=s=>document.querySelector('.ph '+s);const K=kitS(),o=JSON.stringify(K);q('.ks .ds-seg span:nth-child(3)').click();q('.ds-sw').click();q('.msearch').click();const ok=K.seg===2&&!!q('.msearch.focus')&&q('.ds-sw').classList.contains('on')!==JSON.parse(o).sw[0];Object.assign(K,JSON.parse(o));return ok})()"""))
        check("the app's screens use the kit (start, onboarding, explore)", await js("""(()=>{ui.compPreview=null;const r=[];for(const s of ['start','onboarding','explore']){ui.appStep=s;renderPhone();r.push(!!document.querySelector('.ph .scrn .ds-btn, .ph .scrn .ds-row'))}ui.appStep='main';renderPhone();return r.every(Boolean)})()"""))
        nodemo = await js("""(()=>{const out=[];for(const t of ['app','global','specific','exp']){ui.tab=t;ui.open={};renderEditor();for(let i=0;i<8;i++){const hs=[...document.querySelectorAll('#edb .fold:not(.open) > .fh:not(.locked)')];if(!hs.length)break;hs.forEach(h=>h.click());}
          out.push(...[...document.querySelectorAll('#edb .fold')].filter(f=>!f.closest('[data-screen]')&&!f.closest('.hasdemo')&&!f.querySelector(':scope>.fh.locked')).map(f=>f.querySelector(':scope>.fh .ft').textContent));}ui.tab='app';ui.open={};renderEditor();return out})()""")
        check("every editor section has a demo on the phone (except plans with nothing built and the JSON preview)", set(nodemo) <= {"Geo-reference", "Ambient sound", "Capture & share", "config.json (live preview)"}, str(nodemo))
        check("a click inside a section puts the phone on its demo", await js("""(()=>{ui.tab='app';ui.open={app_ds:true,ds_buttons:true,app_tap:true,tap_base:true};renderEditor();ui.compPreview=null;ui.appStep='main';renderPhone();const fire=el=>el.dispatchEvent(new PointerEvent('pointerdown',{bubbles:true}));
          fire(document.querySelector('#edb .fold.hasdemo .fold.open .urow label'));const a=ui.compPreview?.page;fire([...document.querySelectorAll('#edb .fold.open')].find(f=>f.querySelector(':scope>.fh .ft')?.textContent==='Base elements').querySelector('.fb .urow, .fb .guide'));const b=ui.compPreview?.type;ui.open={};renderEditor();ui.compPreview=null;return [a,b].join()==='buttons,buttons'})()"""))
        # v16: the final tokens follow the Unity theme
        check("the Unity faces are embedded and used (Fraunces titles, Atkinson body)", await js("""document.fonts.load('600 20px Fraunces').then(()=>{runDemo(kitDemo('type'),true);const f=s=>getComputedStyle(document.querySelector('.ph .kit '+s)).fontFamily;return document.fonts.check('600 20px Fraunces')&&/^"?Fraunces/.test(f('.ds-h1'))&&f('.ds-body').startsWith('"Atkinson Hyperlegible Next"')})"""))
        small = await js("""(()=>{let small=[];for(const k of Object.keys(KIT_PAGES)){runDemo(kitDemo(k),true);const pt=dev.U/3.9;document.querySelectorAll('.ph .kit *').forEach(e=>{if(!e.childNodes.length||![...e.childNodes].some(n=>n.nodeType===3&&n.textContent.trim()))return;const fs=parseFloat(getComputedStyle(e).fontSize)/pt;if(fs<12.9)small.push(k+':'+e.className+':'+fs.toFixed(1))})}return small.slice(0,8)})()""")
        check("nothing under 13 pt on the kit pages", small == [], str(small))
        await js("ui.appearance='light'")   # the earlier checks leave the preview on dark
        check("accent is a closed list that sets both designs; the main button is the accent", await js("""(()=>{const o=D.ds.accent;D.ds.accent='garden';setAccent('garden');runDemo(kitDemo('buttons'),true);const c=getComputedStyle(document.querySelector('.ph .ds-btn.main')).backgroundColor;const ok=D.themes.light.accent==='#24532F'&&D.themes.dark.accent==='#8CC99B'&&c==='rgb(36, 83, 47)';D.ds.accent=o;setAccent(o);renderPhone();return ok})()"""))
        check("glass: solid = no blur and a dense tint (Unity 6.3)", await js("""(()=>{D.ds.glass='solid';runDemo(kitDemo('panels'),true);const e=document.querySelector('.ph .ds-panel'),cs=getComputedStyle(e);const ok=cs.backdropFilter==='none'&&/0[.]84/.test(document.querySelector('.ph .kit').style.getPropertyValue('--tg'));D.ds.glass='blur';renderPhone();return ok})()"""))
        check("controls are 48 pt high (buttons, search) and chips 32", await js("""(()=>{const pt=dev.U/3.9;runDemo(kitDemo('buttons'),true);const b=document.querySelector('.ph .ds-btn.main').getBoundingClientRect().height/pt;runDemo(kitDemo('controls'),true);const s=document.querySelector('.ph .msearch').getBoundingClientRect().height/pt,c=document.querySelector('.ph .lchip').getBoundingClientRect().height/pt;return Math.abs(b-48)<1&&Math.abs(s-48)<1&&Math.abs(c-32)<1})()"""))
        check("nav bar: Raised lifts the chosen icon into a disc", await js("""(()=>{ui.device='phone_p';buildDevice();D.ds.nav.style='raised';kitS().tab=1;kitS().push=null;runDemo(kitDemo('nav'),true);const t=document.querySelector('.ph .ds-tab.on .ti'),bar=document.querySelector('.ph .ds-nav');return t.getBoundingClientRect().top<bar.getBoundingClientRect().top})()"""))
        check("light is the default mode", default_mode == "light")
        check("state demos: the wall out of view, the coach mark, a tapped POI off the screen", await js("(()=>{mainDemo('away');const a=!!document.querySelector('.ph .mhint, .ph .mmap');mainDemo('coach');const b=!!document.querySelector('.ph .mcoach');mainDemo('offsel');const c=!!document.querySelector('.ph .pin');ui.appStep='main';ui.cam={...CAM0};renderPhone();return a&&b&&c})()"))
        check("labels toggles reach the links, dock and tiles", await js("""(()=>{ui.appStep='tapped';ui.appPoi=0;const r=[];for(const k of ['act_comp_3','act_comp_4','act_comp_5']){const A=D.tap_action_comps.find(x=>x.key===k),o=A.labels;ui.compPreview={type:'act',key:k};renderPhone();const a=document.querySelector('.ph .tapc').innerHTML;A.labels=!o;renderPhone();r.push(a!==document.querySelector('.ph .tapc').innerHTML);A.labels=o}ui.compPreview=null;renderPhone();return r.every(Boolean)})()"""))
        check("onboarding off: Start opens the camera", await js("(()=>{D.app.onboarding.on=false;ui.appStep='onboarding';renderPhone();const ok=ui.appStep==='main';D.app.onboarding.on=true;return ok})()"))
        check("section titles stick to the top", await js("(()=>{ui.open['app_ds']=true;ui.open['ds_type']=true;ui.open['ds_colour']=true;ui.open['theme_dark']=true;renderEditor();document.querySelector('#edb').scrollTop=400;const e=document.querySelector('#edb .fold.open > .fh'),b=document.querySelector('#edb').getBoundingClientRect();return getComputedStyle(e).position==='sticky'&&Math.abs(e.getBoundingClientRect().top-b.top)<8})()"))

        # v17: markers and regions
        check("v17: five point styles, default glass chip", await js("D.point_styles.length===5 && D.point_default==='point_style_1'"))
        check("v17: levels pick point styles", await js("D.levels.every(l=>!!l.point_style)"))
        check("v17: app markers drawn as canvases", await js("(()=>{ui.tab='app';ui.appStep='main';ui.compPreview=null;renderPhone();return document.querySelectorAll('.ph .mk canvas.mkc').length})()") >= 1)
        check("v17: nine region presets", await js("D.styles.length") == 9)
        check("v17: corner smoothing rounds outlines", await js("chaikin([[0,0],[10,0],[10,10],[0,10]],true,1).length") > 4)
        check("v17: wall ink is the same in light and dark", await js("(()=>{const m=D.app.appearance;D.app.appearance='light';const a=regionCols({color_mode:'accent'}).col;D.app.appearance='dark';const b=regionCols({color_mode:'accent'}).col;D.app.appearance=m;return a===b&&a===WALL_INK})()"))
        check("v17: every region style has a demo shape", await js("D.styles.every(s=>styleSample(s).length>0)"))
        check("v17: brackets and spotlight presets", await js("D.styles.some(s=>s.line.pattern==='brackets') && D.styles.some(s=>s.fill.mode==='spotlight')"))
        check("v17: badge marks differ per status (never colour alone)", await js("(()=>{const c=document.createElement('canvas').getContext('2d');const P={...pointStyleByKey('point_style_6')};const out=[];for(const st of ['intact','partial_damage','destroyed']){c.clearRect(0,0,300,150);drawMarker(c,60,60,1,{...D.pois[0],status:st},P,{a:1});out.push(c.getImageData(70,40,30,30).data.join(','))}return new Set(out).size===3})()"))
        check("v17: label sits past its marker", await js("(()=>{ui.tab='app';ui.appStep='main';renderPhone();const m=document.querySelector('.ph .mk');const c=m.querySelector('canvas').getBoundingClientRect(),l=m.querySelector('.lbl').getBoundingClientRect(),cx=c.left+c.width/2,cy=c.top+c.height/2;return !(l.left<cx&&l.right>cx&&l.top<cy&&l.bottom>cy)})()"))
        check("v17: corner smoothing rounds a sampled outline", await js("(()=>{const P=[];for(let i=0;i<=20;i++)P.push([i,0]);for(let i=1;i<=20;i++)P.push([20,i]);for(let i=19;i>=0;i--)P.push([i,20]);for(let i=19;i>0;i--)P.push([0,i]);const Q=roundCorners(P,true,1);return Q.length>=32&&!Q.some(q=>q[0]===20&&q[1]===20)})()"))
        check("v17: the Point styles section previews its style on the wall", await js("(()=>{ui.tab='global';setScreen('wall');ui.open['pointstyles']=true;ui.pstyle=3;const ok=pointPreview()&&pointStyleOf(D.pois[0]).key===D.point_styles[3].key;ui.open['pointstyles']=false;return !!ok})()"))
        check("v17: ranges are sliders (corner, density, icon line)", await js("['corner','density','icon_stroke'].every(k=>typeof D.ds[k]==='number')"))

        # v18: visitor profiles, one editor
        check("v18: profile bar has look, mode, visitors, time, text", await js("document.querySelectorAll('#pbar .pseg').length") == 5)
        check("v18: a token edited in Playful leaves Premium alone", await js("(()=>{setProfile('look','playful');const p0=D.looks.premium.tokens.density;D.ds.density=1.2;const ok=D.looks.playful.tokens.density===1.2&&D.looks.premium.tokens.density===p0;D.ds.density=1.1;setProfile('look','premium');return ok&&D.ds.density===p0})()"))
        check("v18: a colour edited in dark leaves light alone; shared tokens follow both", await js("(()=>{setProfile('mode','dark');const l0=D.themes.light.glass.color;D.themes.dark.glass.color='#101010';const ok=D.themes.light.glass.color===l0&&D.looks.premium.colours.dark.glass.color==='#101010';const c0=D.ds.corner;D.ds.corner=14;setProfile('mode','light');const ok2=D.ds.corner===14;D.ds.corner=c0;D.looks.premium.colours.dark.glass.color='#1A1B1E';return ok&&ok2})()"))
        check("v18: back to the base value keeps no difference", await js("(()=>{setProfile('look','playful');const b=D.looks.premium.tokens.depth;D.ds.depth=0.5;D.ds.depth=b;const ok=!('depth' in D.looks.playful.tokens);setProfile('look','premium');return ok})()"))
        check("v18: fields show their scope; Playful marks inherited and own values", await js("(()=>{ui.tab='app';ui.open={app_ds:true,ds_space:true,ds_depth:true};setProfile('look','playful');const inh=document.querySelectorAll('#edb .sct.inh').length,own=document.querySelectorAll('#edb .sct.own').length;setProfile('look','premium');return inh>=1&&own>=2})()"))
        check("v18: no light / dark fold pairs left", await js("(()=>{ui.tab='app';renderEditor();return !document.querySelector('#edb .fold[data-id$=_light], #edb .fold[data-id$=_dark]') && typeof themeFold==='undefined'})()"))
        check("v18: undo keeps the views", await js("(()=>{snap();D.ds.scrim=0.7;undo();return typeof D.ds.corner==='number'&&D.themes.light.glass.color.startsWith('#')})()"))
        check("v18: Quick shows only the top levels on the main screen", await js("(()=>{setScreen('app');ui.appStep='main';ui.compPreview=null;const gi=D.pois.findIndex(p=>p.id==='arched_gate');ui.cam=camPoseFor(D.pois[gi].pos,50,50,1.1);setProfile('time','classic');renderPhone();const a=!!document.querySelector('.ph .mk[data-i=\"'+gi+'\"]');setProfile('time','quick');renderPhone();const b=!!document.querySelector('.ph .mk[data-i=\"'+gi+'\"]');setProfile('time','classic');ui.cam={...CAM0};renderPhone();return a&&!b})()"))
        check("v18: card text by time, children's text by visitors", await js("(()=>{const p=D.pois[0];D.profile.time='quick';const q=storyOf(p);D.profile.time='deep';const d=storyOf(p);D.profile.time='classic';D.profile.visitors='children';const k=storyOf(p);D.profile.visitors='adults';return q.length<d.length&&k!==q&&k!==d})()"))
        check("v18: children: mascot coach line and games first", await js("(()=>{setProfile('visitors','children');mainDemo('coach');renderPhone();const t=document.querySelector('.ph .mtip')?.textContent||'';ui.appStep='explore';renderPhone();const first=document.querySelector('.ph .ds-explore .ds-sh')?.textContent;setProfile('visitors','adults');ui.appStep='main';renderPhone();return /adventure/.test(t)&&first==='Games'})()"))
        check("v18: onboarding sets the profile (and the look follows)", await js("(()=>{ui.appStep='onboarding';renderPhone();D.profile.look='auto';renderPhone();const c=[...document.querySelectorAll('.ph .ds-onbp .oaud .ds-opt')];c[0].click();const a=D.profile.visitors==='children'&&activeLook()==='playful';[...document.querySelectorAll('.ph .ds-opts.three .ds-opt')][2].click();const b=D.profile.time==='deep';[...document.querySelectorAll('.ph .ds-onbp .oaud .ds-opt')][1].click();[...document.querySelectorAll('.ph .ds-opts.three .ds-opt')][1].click();return a&&b&&D.profile.visitors==='adults'})()"))
        check("v18: Settings lets the visitor change everything", await js("(()=>{ui.appStep='settings';renderPhone();const n=document.querySelectorAll('.ph .sheet-set .srow').length;ui.appStep='main';renderPhone();return n})()") >= 12)
        check("v18: the Playful font is embedded", await js("document.fonts.load('600 20px Fredoka').then(f=>f.length>0)"))
        check("v18: config.json carries the profiles", await js("(()=>{const j=JSON.parse(jsonGlobal());return !!(j.app.profiles&&j.app.profiles.looks.playful&&j.app.profiles.time.quick)})()"))

        check("v18: a POI hidden by Time is not tappable in Play", await js("(()=>{const gi=D.pois.findIndex(p=>p.id==='arched_gate');D.profile.time='quick';const was=playing;playing=true;const h=hitPoi(D.pois[gi].pos);playing=was;D.profile.time='classic';return !h||h.pi!==gi})()"))
        check("v18: choosing on the phone refreshes the editor's profile", await js("(()=>{ui.tab='app';renderEditor();ui.appStep='onboarding';setScreen('app');renderPhone();D.profile.look='auto';renderPhone();[...document.querySelectorAll('.ph .ds-onbp .oaud .ds-opt')][0].click();const t=document.querySelector('#pbar .pseg button[aria-pressed=true]').textContent;[...document.querySelectorAll('.ph .ds-onbp .oaud .ds-opt')][1].click();return t==='Playful'})()"))
        check("v18: the visitor's light / dark does not change the app's default", await js("(()=>{const d=D.app.appearance;ui.appStep='settings';renderPhone();const segs=[...document.querySelectorAll('.ph .sheet-set .srow')][4].querySelectorAll('.ds-seg span');segs[2].click();const ok=appearance()==='dark'&&D.app.appearance===d;D.profile.appearance=null;renderPhone();return ok})()"))
        await b.close()
    with open(out_path, "w", encoding="utf-8") as f: json.dump(R, f, indent=1)
    bad = [k for k, v in R["checks"].items() if not v["ok"]]
    print(f"{len(R['checks'])} checks, {len(bad)} failed {bad}; errors {len(R['errors'])}; headers " + ", ".join(f"{k}:{len(v['headers'])}" for k, v in R["tabs"].items()))
asyncio.run(main(sys.argv[1], sys.argv[2]))
