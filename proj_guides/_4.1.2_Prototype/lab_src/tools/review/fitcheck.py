"""A1 fit check: for each state x profile x device, report controls that leave their scroller / the screen horizontally,
text that overflows its box, and targets under 48 pt. usage: LAB=out/x.html python fitcheck.py [states] [devices]"""
import pathlib, asyncio, os, sys, json
from playwright.async_api import async_playwright
PROFILES = [("premium", "light", "m"), ("premium", "dark", "m"), ("playful", "light", "m"), ("playful", "dark", "m"), ("premium", "light", "l"), ("playful", "light", "l")]
ST = {"onb": "ui.appStep='onboarding';", "settings": "ui.appStep='settings';", "you": "ui.appStep='you';"}
CHECK = r"""(()=>{const out=[];const ph=document.querySelector('.ph');const scr=[...ph.querySelectorAll('.tapc')].pop();const pr=document.createElement('div');pr.style.cssText='position:absolute;width:calc(100 * var(--P));height:1px';scr.append(pr);const P=pr.getBoundingClientRect().width/100;pr.remove();
const SR=scr.getBoundingClientRect();
const scroller=e=>{let p=e.parentElement;while(p&&p!==scr){const s=getComputedStyle(p);if(/auto|scroll|hidden/.test(s.overflowX+s.overflowY))return p;p=p.parentElement}return scr};
const name=e=>(e.className&&e.className.baseVal===undefined?e.className:'')+' "'+(e.textContent||'').trim().slice(0,24)+'"';
for(const e of scr.querySelectorAll('.ds-opt,.oq,.ds-seg span,.lchip,.ds-btn,.srow,.ds-row,.oex-tag,.ychip,.ystat,.ds-sw,.ds-x,.oex,.yhead,.ds-seg')){
 const r=e.getBoundingClientRect();if(!r.width)continue;const c=scroller(e).getBoundingClientRect();
 if(r.left<c.left-1||r.right>c.right+1)out.push('outside scroller: '+name(e)+' '+Math.round(r.left-c.left)+'/'+Math.round(c.right-r.right));
 if(r.left<SR.left-1||r.right>SR.right+1)out.push('outside screen: '+name(e));
 if(e.matches('.oq,.ds-seg span,.lchip,.ds-btn,.ds-x,.ds-row.hit')&&!e.matches('.ds-seg span')){const a=getComputedStyle(e,'::after');let hh=r.height;if(a.content!=='none'&&a.position==='absolute')hh=r.height-Math.min(0,parseFloat(a.top)||0)-Math.min(0,parseFloat(a.bottom)||0);if(hh<47.5*P)out.push('target<48: '+name(e)+' '+Math.round(hh/P))}
}
for(const e of scr.querySelectorAll('b,small,span,p,i,div')){if(e.children.length)continue;const t=(e.textContent||'').trim();if(!t)continue;
 if(e.scrollWidth>e.clientWidth+2&&e.clientWidth>0)out.push('text overflow: '+name(e)+' '+e.scrollWidth+'>'+e.clientWidth);
 const r=e.getBoundingClientRect(),pr=e.parentElement.getBoundingClientRect();
 if(r.width&&(r.right>pr.right+2||r.left<pr.left-2)&&getComputedStyle(e).position!=='absolute')out.push('text past parent: '+name(e)+' '+Math.round(r.right-pr.right));}
return [...new Set(out)]})()"""
async def main(states, devs):
    async with async_playwright() as p:
        b = await p.chromium.launch(args=["--no-sandbox"]); pg = await b.new_page(viewport={"width": 1440, "height": 900})
        await pg.goto(pathlib.Path(os.environ.get("LAB", "out/dev4.html")).resolve().as_uri()); await pg.wait_for_timeout(1200)
        await pg.evaluate("document.querySelector('#tab-app').click()"); n = 0
        for st in states:
            for (lk, m, tx) in PROFILES:
                for dv in devs:
                    vis = "children" if lk == "playful" else "adults"
                    await pg.evaluate(f"(()=>{{ui.device='{dv}';buildDevice();setScreen('app');D.app.appearance='{m}';ui.appearance='{m}';ui.forceMode=null;D.profile.appearance=null;D.profile.visitors='{vis}';D.profile.look='{lk}';D.profile.time='classic';D.profile.text='{tx}';ui.coachDone=true;ui.compPreview=null;ui.previewLayout=null;tapState.view=null;{ST[st]}renderPhone();}})()")
                    await pg.wait_for_timeout(250)
                    r = await pg.evaluate(CHECK)
                    if r: n += len(r); print(f"{st} {lk}/{m}/{tx} {dv}:"); [print("   ", x) for x in r]
        print("issues", n); await b.close()
a = sys.argv
asyncio.run(main((a[1] if len(a) > 1 else "onb,settings,you").split(","), (a[2] if len(a) > 2 else "phone_p,phone_l,tablet_p,tablet_l").split(",")))
