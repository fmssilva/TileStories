// v8k checks (Playwright + the installed Edge): node test8k.js -> PASS/FAIL per check
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 20000 }); await p.waitForTimeout(400);
  const R = src => p.evaluate(s => LAB.run(s), src);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  await R('S.hints.on=false;S.previewBar=false;S.lang="en";S.guideMute=true;S.view="card";S.stop="full";renderAll(false)');
  // record every reaction class Azu gets
  await R('window.RX=[];new MutationObserver(function(ms){ms.forEach(function(m){var c=m.target.className&&m.target.className.baseVal===undefined?m.target.className:"";(c.match(/r-\\w+|blink/g)||[]).forEach(function(x){if(RX[RX.length-1]!==x)RX.push(x)})})}).observe(ph,{subtree:true,attributes:true,attributeFilter:["class"]})');
  const start = async (extra) => { await R(`if(G.on)stopTour();gdResetVisitor();${extra || ''};startTour();G.i=0;runStep()`); await p.waitForTimeout(700); };
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await p.waitForTimeout(60); } return false; };
  const tap = async () => { const m = await p.locator('#ph .mascot').boundingBox(); await p.mouse.click(m.x + m.width / 2, m.y + m.height / 2); };
  // 1. a tap pokes; the tour carries on
  await start(); await R('RX=[]'); await tap(); await p.waitForTimeout(150);
  ok('A tap on Azu: a reaction, and the tour is not paused', await R('RX.some(function(x){return/^r-/.test(x)})&&!G.paused'), await R('RX'));
  // 2. Lively: never one of the last three again
  const seq = []; for (let i = 0; i < 9; i++) { seq.push(await R('azPoke()')); await p.waitForTimeout(30); }
  ok('Lively pokes: never one of the last 3 reactions', seq.every((r, i) => seq.slice(Math.max(0, i - 3), i).indexOf(r) < 0), seq);
  await R('stopTour();S.stop="closed";renderPhone(false);gdResetVisitor();window.__k0=S.guidance.kinds.map(function(k){return k.on});S.guidance.kinds.forEach(function(k){k.on=k.id==="fun"});G.gd.touchAt=null;G.pokes=0;azPoke();azPoke();azPoke()');
  ok('...and a joke after three pokes (Guide › Guidance › Fun › Play › A joke, on the wall; its words from Languages & Terms)', await until('var b=G.gd.cur;!!b&&b.item.id==="joke"&&[TXT("mascot_joke_1"),TXT("mascot_joke_2"),TXT("mascot_joke_3")].indexOf(b.text)>=0&&!!ph.querySelector(".gdbub:not(.gd-out)")', 4000), await R('G.gd.last&&G.gd.last.why'));
  await R('gdStop("test");S.guidance.kinds.forEach(function(k,i){k.on=window.__k0[i]});');
  // 3. Calm: a wink or a blush, no jokes
  await start('S.guideCfg.life="calm";G.pokes=0'); const cs = []; for (let i = 0; i < 6; i++) cs.push(await R('azPoke()')); await p.waitForTimeout(900);
  await R('stopTour();S.stop="closed";renderPhone(false);gdResetVisitor();window.__k0=S.guidance.kinds.map(function(k){return k.on});S.guidance.kinds.forEach(function(k){k.on=k.id==="fun"});G.gd.touchAt=null;azPoke();azPoke();azPoke()'); await p.waitForTimeout(1600);
  ok('Calm: a poke gets a wink or a blush, and no joke', cs.every(r => r === 'r-wink' || r === 'r-blush') && await R('(!ph.querySelector(".azbub")||[TXT("mascot_joke_1"),TXT("mascot_joke_2"),TXT("mascot_joke_3")].indexOf(ph.querySelector(".azbub").textContent)<0)&&!(G.gd.cur&&G.gd.cur.item.id==="joke")'), cs);
  await R('S.guidance.kinds.forEach(function(k,i){k.on=window.__k0[i]});');
  await R('S.guideCfg.life="lively"');
  // 4. three quick taps tickle; a long press knocks Azu over
  await start(); await R('RX=[]'); for (let i = 0; i < 3; i++) { await tap(); await p.waitForTimeout(120); }
  ok('Three quick taps: Azu laughs ("tickles")', await R('RX.indexOf("r-laugh")>=0&&ph.querySelector(".azbub").textContent===TXT("mascot_tickle")'), await R('RX'));
  await p.waitForTimeout(1400); await R('RX=[]');
  { const m = await p.locator('#ph .mascot').boundingBox(); await p.mouse.move(m.x + 28, m.y + 30); await p.mouse.down(); await p.waitForTimeout(650); await p.mouse.up(); }
  ok('A long press: Azu falls over and gets up ("Oops")', await R('RX.indexOf("r-fall")>=0&&ph.querySelector(".azbub").textContent===TXT("mascot_oops")&&!G.paused'), await R('RX'));
  // 5. moods by step, and Solemn
  await R('G.i=1;runStep()'); await p.waitForTimeout(500);
  ok('Step 2 (lost in 1755): a serious face, its brows shown', await R('var m=ph.querySelector(".mascot");m.dataset.mood==="serious"&&getComputedStyle(m.querySelector(".az-brow")).opacity==="1"'));
  await R('S.m.solemn=true;G.i=2;runStep()'); await p.waitForTimeout(500);
  ok('Solemn point: "surprised" becomes thoughtful', await R('ph.querySelector(".mascot").dataset.mood') === 'thinking');
  await R('stopTour();S.stop="half";renderPhone(false);gdResetVisitor();window.__k0=S.guidance.kinds.map(function(k){return k.on});S.guidance.kinds.forEach(function(k){k.on=k.id==="fun"});G.gd.touchAt=null;azPoke();azPoke();azPoke()'); await p.waitForTimeout(1600);
  ok('Solemn point: no joke on the third poke (Why not now?: a Solemn point, no fun here)', await R('(!ph.querySelector(".azbub")||[TXT("mascot_joke_1"),TXT("mascot_joke_2"),TXT("mascot_joke_3")].indexOf(ph.querySelector(".azbub").textContent)<0)&&!(G.gd.cur&&G.gd.cur.item.id==="joke")&&/a Solemn point: no fun here/.test(gdWhyNot(gdItems(),S.gdVisitor,Object.assign(gdContext(),{touchAt:null,remark:null,showing:null}),gdNow(),"joke"))'));
  await R('S.guidance.kinds.forEach(function(k,i){k.on=window.__k0[i]});');
  await R('S.m.solemn=false');
  // 6. the mouth follows the words (and stays still when asked)
  await start('S.guideCfg.speed=0.8'); await p.waitForTimeout(400);
  const ds = new Set(); for (let i = 0; i < 30; i++) { ds.add(await R('ph.querySelector(".mascot .mouth").getAttribute("d")')); await p.waitForTimeout(50); }
  ok('The mouth changes shape with the words (3+ shapes in 1.5 s)', ds.size >= 3, ds.size);
  await start('S.guideCfg.lips=false'); await p.waitForTimeout(400);
  const ds2 = new Set(); for (let i = 0; i < 20; i++) { ds2.add(await R('ph.querySelector(".mascot .mouth").getAttribute("d")')); await p.waitForTimeout(50); }
  ok('Mouth Still: one shape', ds2.size === 1, ds2.size);
  await R('S.guideCfg.lips=true;RX=[]'); await p.waitForTimeout(5300);
  ok('Azu blinks (every 2.5-5 s)', await R('RX.indexOf("blink")>=0'));
  // 7. the water break (round 11): held while the tour plays (Fun never during a tour); shown as the Editor's Try does, the glass
  // beside Azu, one tap, then the tour goes on
  await start('S.guidance.items.water.talkFor=1'); await R('G.talkFrom=performance.now()-5000;next()'); await p.waitForTimeout(600);
  // round 12: the water break is the breather, a beat of Fun › On the tour: it plays only at its seam of the tour's plan (Why not now? answers from the plan)
  ok('Water break: never between the steps of a playing tour but at its planned seam (round 12: the breather; Why not now? answers from the plan)', await R('!G.moment&&!ph.querySelector(".azmoment")&&/plan/.test((function(id){var a=gdAll(gdItems(),S.gdVisitor,gdContext(),gdNow()).filter(function(x){return x.it.id===id})[0];return a?String(a.r.no||""):""})("water"))'));
  await R('stopSpeech();G.i=S.guideSteps.findIndex(function(s){return s.id==="g8"});runStep()'); await p.waitForTimeout(1200); // a step with pictures beside Azu: room for the glass
  await R('pauseTour();azMoment("water",function(){resumeTour()})'); await p.waitForFunction(() => LAB.run('!!document.querySelector("#ph .azmoment")'), null, { timeout: 5000 });
  const mo = await R('[G.moment,!!ph.querySelector(".azmoment [data-azprop=water]"),ph.querySelector(".mascot").classList.contains("center")]');
  ok('...its look (the Try): the glass beside Azu, Azu where it was (beside the words)', mo[0] === 'water' && mo[1] && !mo[2], mo);
  await R('RX=[]'); await p.locator('#ph [data-azprop]').click(); await p.waitForTimeout(3000);
  ok('...one tap on the glass: Azu drinks, says "Ahh", and the tour carries on', await R('RX.indexOf("r-drink")>=0&&!G.paused&&!G.moment&&!ph.querySelector(".azmoment")'), await R('[G.i,G.moment,RX]'));
  // 8. no tap: it carries on by itself after 8 s
  await start(''); await R('azMoment("water",function(){window.MDONE=1})'); await p.waitForTimeout(8400);
  ok('No tap: after 8 s the tour carries on by itself', await R('!G.moment&&window.MDONE===1'));
  // 9. the cookie, before the end (round 11: held in a playing tour; its look as the Try shows it)
  await start('S.guidance.items.water.talkFor=600'); await R('G.i=S.guideSteps.length-2;next()'); await p.waitForTimeout(400);
  ok('Before the end: no cookie but at its planned seam (round 12: a beat of the tour; Why not now? answers from the plan)', await R('!G.moment&&/plan/.test((function(id){var a=gdAll(gdItems(),S.gdVisitor,gdContext(),gdNow()).filter(function(x){return x.it.id===id})[0];return a?String(a.r.no||""):""})("cookie"))'));
  await R('stopSpeech();G.i=S.guideSteps.findIndex(function(s){return s.id==="g8"});runStep()'); await p.waitForTimeout(1200);
  await R('pauseTour();azMoment("cookie",function(){resumeTour()})'); await p.waitForFunction(() => LAB.run('!!document.querySelector("#ph [data-azprop=cookie]")'), null, { timeout: 5000 });
  ok('...its look (the Try): a cookie', await R('G.moment==="cookie"&&!!ph.querySelector("[data-azprop=cookie]")'));
  await R('RX=[]'); await p.locator('#ph [data-azprop]').click(); await p.waitForTimeout(1800);
  ok('...a tap: Azu eats it and thanks', await R('RX.indexOf("r-eat")>=0&&ph.querySelector(".azbub").textContent===TXT("mascot_thanks")'));
  // 10. Calm or Solemn: nothing unprompted
  for (const [n, x] of [['Calm', 'S.guideCfg.life="calm"'], ['Solemn', 'S.m.solemn=true']]) {
    await start(`${x};S.guidance.items.water.talkFor=1`); await R('G.talkFrom=performance.now()-5000;G.i=S.guideSteps.length-2;next()'); await p.waitForTimeout(300);
    ok(`${n}: no water break, no cookie`, await R('!G.moment'));
    await R('S.guideCfg.life="lively";S.m.solemn=false');
  }
  await R('S.guidance.items.water.talkFor=60');
  // 11. Reduce Motion: a poke moves nothing
  await start('S.reduce=true'); await R('G.lastPokes=["r-wink","r-blush","hey"];azPoke()'); await p.waitForTimeout(100);
  ok('Reduce Motion: a poke moves nothing', await R('ph.querySelector(".mascot svg").getAnimations().filter(function(a){return a.playState==="running"}).length===0'));
  await R('S.reduce=false');
  // 12. the bubble's words: 4.5:1 or more, light and dark
  for (const dark of [false, true]) {
    await start(`S.dark=${dark};renderAll(false)`); await R('azBubble("Test words",4000)'); await p.waitForTimeout(300);
    const c = await R('var b=ph.querySelector(".azbub"),px=function(c){var m=c.match(/[\\d.]+/g).map(Number),k=c.indexOf("color(")===0?255:1;return[m[0]*k,m[1]*k,m[2]*k]},lum=function(a){return a.map(function(v){v/=255;return v<=.03928?v/12.92:Math.pow((v+.055)/1.055,2.4)}).reduce(function(s,v,i){return s+v*[.2126,.7152,.0722][i]},0)},x=lum(px(getComputedStyle(b).color)),y=lum(px(getComputedStyle(b).backgroundColor));Math.round((Math.max(x,y)+.05)/(Math.min(x,y)+.05)*100)/100');
    ok(`${dark ? 'dark' : 'light'}: Azu's words at 4.5:1 or more`, c >= 4.5, c);
  }
  await R('S.dark=false;renderAll(false);stopTour()');
  // 13. the Editor
  ok('Editor: Mascot has Liveliness, Mouth, Moods, Moments, Water break after and five Try', await R('S.tab="guide";renderEditor();var f=ed.querySelector(\'[data-fo="s7mascot"]\');!!f&&["Liveliness","Mouth","Moods","Moments","Water break after"].every(function(t){return f.textContent.indexOf(t)>=0})&&f.querySelectorAll("[data-aztry]").length===5'));
  ok('Azu\'s words are on the Mascot (Languages & Terms; the jokes are A joke\'s; round 12: the old water and cookie asks are the Try\'s words again)', await R('(TXO.s7mascot||[]).length') >= 8 && await R('TXOWN.mascot_joke_1==="gd:i:joke"&&TXOWN.mascot_water==="s7mascot"'), await R('(TXO.s7mascot||[]).length'));
  await R('S.tab="marker";S.sel.gstep="g2";renderEditor()'); await p.waitForTimeout(300);
  ok('Guide script: Solemn point, and Azu\'s mood on the step', await R('/Solemn point/.test(ed.querySelector(\'[data-fo="m3"]\').textContent)&&!!ed.querySelector("[data-azmood=g2]")'));
  await p.locator('select[data-azmood="g2"]').selectOption('thinking');
  ok('...picking a mood sets it', await R('byId(S.guideSteps,"g2").mood') === 'thinking');
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
