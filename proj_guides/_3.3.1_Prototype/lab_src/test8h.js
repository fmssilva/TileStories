// v8h checks (Playwright + the installed Edge): node test8h.js -> PASS/FAIL per check
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/block-components-lab.html#probe';
const LOOKS = { loading: ['skeleton', 'spinner', 'mascot', 'tiles', 'scan'], lost: ['frame', 'mascot', 'simple'], empty: ['mascot', 'simple'], offline: ['banner', 'mascot'], done: ['button', 'toast', 'mascot'] };
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 20000 }); await p.waitForTimeout(400);
  const R = src => p.evaluate(s => LAB.run(s), src);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  await R('S.hints.on=false;S.previewBar=false;renderAll(false)');
  // the lowest contrast of the state's words over the colour really behind them (its own box; over the camera, its own pill)
  await R(`window.stContrast=function(){var lum=function(c){var m=c.match(/[\\d.]+/g).map(Number);return [m[0],m[1],m[2]].map(function(v){v/=255;return v<=.03928?v/12.92:Math.pow((v+.055)/1.055,2.4)}).reduce(function(a,v,i){return a+v*[.2126,.7152,.0722][i]},0)};
    var bgOf=function(el){while(el&&el!==document.body){var c=getComputedStyle(el).backgroundColor,m=c.match(/[\\d.]+/g);if(m&&(m.length<4||+m[3]>.5))return c;el=el.parentElement}return "rgb(255,255,255)"};
    var min=99,worst="";ph.querySelectorAll(".stv b,.stv span:not(.st-mascot),.stv .st-pill,.stv .st-toast,.stv button,.stv .st-slow").forEach(function(el){if(!el.textContent.trim()||!el.getClientRects().length||getComputedStyle(el).opacity==="0")return;
      var a=lum(getComputedStyle(el).color),c=lum(bgOf(el)),r=(Math.max(a,c)+.05)/(Math.min(a,c)+.05);if(r<min){min=r;worst=el.className||el.tagName}});return [Math.round(min*100)/100,worst]}`);
  for (const dark of [false, true]) for (const k of Object.keys(LOOKS)) for (const l of LOOKS[k]) {
    await R(`S.dark=${dark};S.states.${k}="${l}";stateTry("${k}")`); await p.waitForTimeout(k === 'done' ? 1700 : 1000);
    if (k === 'loading') { await R('var s=ph.querySelector(".stv");s&&s.classList.add("slow")'); await p.waitForTimeout(450); }
    const c = await R('var s=ph.querySelector(".stv");s?[s.className].concat(stContrast()):null');
    ok(`${dark ? 'dark' : 'light'} ${k} / ${l}: shown, words at 4.5:1 or more`, c && c[0].indexOf('look-' + l) >= 0 && c[1] >= 4.5, c);
  }
  // the words are the state's own (Languages & Terms), the done button turns into Saved
  await R('S.dark=false;S.states.done="button";stateTry("done")'); await p.waitForTimeout(1700);
  ok('On the button: the action turns into \u2713 Saved', await R('var m=ph.querySelector(".st-morph");m&&m.classList.contains("done")&&getComputedStyle(m.querySelector(".b")).opacity==="1"&&m.querySelector(".b").textContent.indexOf("Saved")>0'));
  await R('S.tx.state_lost_title={en:"Wall?",pt:"Parede?"};S.states.lost="frame";stateTry("lost")'); await p.waitForTimeout(900);
  ok('the words come from Languages & Terms', await R('ph.querySelector(".st-lost b").textContent') === 'Wall?');
  // Reduce Motion: nothing in a state moves
  for (const l of LOOKS.loading) {
    await R(`S.reduce=true;S.states.loading="${l}";stateTry("loading")`); await p.waitForTimeout(900);
    ok(`Reduce Motion: loading / ${l} is still`, await R('var s=ph.querySelector(".stv");s&&s.getAnimations({subtree:true}).filter(function(a){return a.playState==="running"}).length===0'));
  }
  await R('S.reduce=false;S.states.lost="frame";stateTry("lost")'); await p.waitForTimeout(800);
  ok('with motion: the AR frame breathes', await R('ph.querySelector(".stv").getAnimations({subtree:true}).length>0'));
  // they go: by themselves, on a tap, and Nothing here's button
  await R('S.states.done="toast";stateTry("done")'); await p.waitForTimeout(3600);
  ok('Done goes by itself (2.6 s)', await R('!ph.querySelector(".stv")'));
  await R('S.states.lost="simple";stateTry("lost")'); await p.waitForTimeout(800);
  const bx = await p.locator('.stv .st-card').boundingBox(); await p.mouse.click(bx.x + 20, bx.y + 20); await p.waitForTimeout(300);
  ok('a tap dismisses a state', await R('!ph.querySelector(".stv")'));
  await R('S.states.empty="simple";stateTry("empty")'); await p.waitForTimeout(800);
  await p.locator('.stv [data-stclose]').click(); await p.waitForTimeout(300);
  ok('Clear filters closes Nothing here', await R('!ph.querySelector(".stv")'));
  // the Editor: one component after Hints, a Try per state
  ok('Editor: Loading & states after Hints, a Try per state, its words', await R('var h=ed.querySelector(\'[data-fo="s5hints"]\'),s=ed.querySelector(\'[data-fo="s5states"]\');!!s&&h.nextElementSibling===s&&s.querySelectorAll("[data-sttry]").length===5&&(TXO.s5states||[]).length===11'));
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
