// test10a (Playwright + the installed Edge): node test10a.js -> PASS/FAIL per check. Guidance round 22: the tour as a demo of the app
// (guide/demo-hand.js, guide/tour-scroll.js, card/page-open.js, guide/focus.js's left bar, editor/card-tab.js's switches).
// [hand] during a tour, from the flip card to the story (the tour going on): the hand starts about 2 cm away (the test's figure: 60-130 px
//   from the tap) and its tip ends within 6 px of the tapped preview's centre, the ripple there, then one click on the preview, as a
//   visitor's tap fires (an event count) and the same opening (PO.log); a visitor's real touch on the card removes the hand at once and
//   pauses the tour; Resume goes on to the story; the hand never over the words being told (the teleprompter, the panel) on any frame.
// [scroll] sampled every frame: monotonic, eased (the first 15 % of its time under 12 % of the way, no frame over 12 % of it), 450-1300 ms;
//   the pill shown near the bottom moving down, near the top moving up, gone after.
// [opening] entering the story plays the same opening as a visitor's tap: the page's rectangle starts at the preview's (left and width
//   within 6 %, its top within 30 px) and grows to the whole screen; back plays the closing into the preview, the card where it was.
// [blocks] each covered block changes and returns: the before / after's --x (across past 70 % and 30 %, back to its start), the 3D model's
//   turn (its picture moved, back), the flip card (flipped, back), the gallery's strip (one picture on, back); once per block per tour;
//   never while the visitor holds a finger on the screen (it waits, then plays); the hand's flips never count as the visitor's try.
// [focus] the bar on the block (in the margin, its right edge 6 px from the block's left edge within 2, the block's drawn height within
//   2), 3:1 or more on the card at its faintest (light and dark), nothing outside the block darker (the rendered colours before and during,
//   by the test's own luma), Dim the rest still dims (the same regions 15 % darker or more).
// [reduced motion] no glide (no hand drawn), no pulse on the bar, the ripple and the actions (the story opens with its fade).
// [editor] each new row changes what the visitor sees (Off: no hand, the story still opens; Show blocks in use Off: no flip; Quick: no
//   pill; Focus by real clicks) and its ▸ (a real click on its label) plays it.
// [switches] the Detail Card tab has no Foldouts row; One section open at a time (real clicks) closes the open section.
// MUTATE=1: the hand starts on its target, the scroll a linear jump, no opening zoom, no block in use, the bar never drawn and the rest dimmed,
//   reduced motion ignored: the checks named [hand], [scroll], [opening], [blocks], [focus] and [reduced motion] turn red.
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage({ viewport: { width: 1400, height: 1000 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url, { timeout: 150000 }); await p.waitForFunction(() => window.LAB, null, { timeout: 150000 }); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(40); } return false; };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info).slice(0, 700) : '')); };
  const T0 = Date.now(), tsec = s => console.log('TIME ' + s + ' ' + Math.round((Date.now() - T0) / 1000) + ' s');
  const MUT = !!process.env.MUTATE;
  if (MUT) {
    await R('DH_FROM=0;tsMs=function(){return 16};tsEase=function(u){return u>0?1:0};PO_MS=0;dhUseOf=function(){return null};fcBar=function(){};S.guideCfg.cue="spotlight";' +
      'var __m=document.createElement("style");__m.textContent="#ph.touring.gspotting .scr{filter:brightness(.8)}";document.head.appendChild(__m);""');
    console.log('MUTATION: the hand starts on its target, the scroll jumps, no opening zoom, no block in use, no bar and the rest dimmed, reduced motion ignored');
  }
  const base = (o) => `SCREENS.phone=[390,844];if(G.on)stopTour();if(G.gd.pages)gdPagesEnd("test",false);gdBeatClose();gdStop("test");if(G.ga.on)gaEnd("test");if(G.ga.card)gaCardLeave();G.te.open=false;
    S.hints.on=false;S.previewBar=false;S.lang="en";S.guideMute=true;S.introSeen=true;S.guideCfg.intro="off";S.reduce=${!!(o && o.reduce)};S.dark=${!!(o && o.dark)};S.cardCfg.mode="${o && o.dark ? 'dark' : 'light'}";
    S.guideCfg.voice="device";S.guideCfg.panel="dock";S.guideCfg.dockStart="mini";S.guideCfg.speed=1;S.vis.speed=null;S.guideCfg.music=false;S.vis.music=null;S.ui.rowDemo=false;S.guideCfg.mascot=true;
    S.guideCfg.tourWords="tele";S.guideCfg.hand="shown";S.guideCfg.inUse=true;S.guideCfg.scroll="natural";S.guideCfg.cue="${MUT ? 'spotlight' : 'bar'}";
    S.guidance.on=true;S.guidance.kinds.forEach(function(k){k.on=false});G.gd.manual=true;G.gd.demo=false;gdChip(null);DH.log=[];DH.useLog=[];PO.log=[];S.view="card";S.stop="full";renderAll(false);""`;
  // the recorder: every frame, the hand's tip and opacity, the ripples, the pill, the scroll, the screen's rectangle, the words' boxes
  await R(`window.T10={on:false,f:[],start:function(){T10.f=[];T10.on=true;var t0=performance.now();(function tick(){if(!T10.on)return;var h=ph.querySelector(".dhand"),rp=ph.querySelector(".dhripple"),c=ph.querySelector(".gscue"),p=ph.getBoundingClientRect(),s=scr.getBoundingClientRect(),
      fl=scr.querySelector("[data-flip]"),words=dhWords().filter(function(w){return w.r-w.l>1});
      T10.f.push({t:Math.round(performance.now()-t0),tip:h&&h.dataset.tip?h.dataset.tip.split(",").map(Number):null,op:h?+getComputedStyle(h).opacity:0,hb:h?(function(){var r=dhRectIn(h);return r})():null,
        rip:rp?[parseFloat(rp.style.left),parseFloat(rp.style.top)]:null,cue:c&&c.classList.contains("on")?{dir:c.dataset.dir,top:parseFloat(c.style.top)}:null,y:scr.scrollTop,view:S.view,
        scr:[s.left-p.left,s.top-p.top,s.width,s.height].map(Math.round),flip:fl?fl.classList.contains("on"):null,words:words});requestAnimationFrame(tick)})()},stop:function(){T10.on=false;return T10.f}};
    window.T10c=[];document.addEventListener("click",function(e){var g=e.target.closest&&e.target.closest('[data-go="story"]');if(g)T10c.push({trusted:e.isTrusted,view:S.view})},true);""`);
  const inter = (a, b) => Math.max(0, Math.min(a.r, b.r) - Math.max(a.l, b.l)) * Math.max(0, Math.min(a.b, b.b) - Math.max(a.t, b.t));

  // ---------- [hand] + [opening]: from the flip card (g2) to the story (g3), the tour going on ----------
  await R(base()); await R('S.guideCfg.inUse=false;startTour({id:"g2"});""'); await W(1500);
  await R('T10c=[];T10.start();stopSpeech();G.paused=false;next();""');
  const arrived = await until('S.view==="story"&&!G.trav&&G.i===3', 9000); await W(500);
  let F = JSON.parse(await R('JSON.stringify(T10.stop())'));
  const log = JSON.parse(await R('JSON.stringify(DH.log)')), po = JSON.parse(await R('JSON.stringify(PO.log)'));
  const tapE = log.find(e => e.tap), glE = log.find(e => e.glide), rpE = log.find(e => e.ripple);
  const shownF = F.filter(f => f.tip && f.op > 0.05), firstF = shownF[0], lastF = shownF.filter(f => f.t <= (tapE ? 1e9 : 0)).slice(-1)[0];
  const dist0 = firstF && tapE ? Math.hypot(firstF.tip[0] - tapE.pt[0], firstF.tip[1] - tapE.pt[1]) : -1;
  // the tip at the press: the frame just before the ripple
  const ripF = F.find(f => f.rip), pressF = ripF ? F.filter(f => f.t <= ripF.t && f.tip).slice(-1)[0] : null;
  const tipErr = pressF && ripF ? Math.hypot(pressF.tip[0] - ripF.rip[0], pressF.tip[1] - ripF.rip[1]) : 99;
  const ctrErr = tapE && po[0] ? (() => { const fr = po[0].from; return Math.abs(tapE.pt[0] - (fr[0] + fr[2] / 2)) })() : 99;
  const over = F.reduce((m, f) => { if (!f.hb || f.op < 0.05) return m; let a = 0; f.words.forEach(w => { a += inter(f.hb, w) }); return Math.max(m, a) }, 0);
  ok('[hand] the tour going on from the flip card to the story: the hand appears about 2 cm away (60-130 px from where it taps; the module says 88)', dist0 >= 60 && dist0 <= 130 && glE && glE.dist >= 60, { dist0: Math.round(dist0), glide: glE });
  ok('[hand] ...its tip ends on the target (within 6 px of the ripple, the ripple on the preview\'s picture centre within 6 px across)', tipErr <= 6 && ctrErr <= 6 && !!rpE, { tipErr, ctrErr, press: pressF && pressF.tip, rip: ripF && ripF.rip });
  ok('[hand] ...then the same action a visitor\'s tap fires: one click on the story\'s preview (on the card), the story open at step 4', arrived && (await R('JSON.stringify(T10c)')) === '[{"trusted":false,"view":"card"}]' && po.length === 1 && po[0].dir === 'open' && po[0].ref === 'b:b_story', { arrived, clicks: await R('JSON.stringify(T10c)'), po });
  ok('[hand] ...never over the words being told (the teleprompter, the panel, the captions) on any frame', over === 0 && shownF.length > 5, { over, frames: shownF.length });
  // [opening]: the screen's rectangle from the preview's, growing
  const opF = F.filter(f => f.view === 'story' && f.t >= (ripF ? ripF.t : 0)).slice(0, 12), fr = po[0] ? po[0].from : [0, 0, 0, 0];
  const o0 = opF[0] ? opF[0].scr : null, grows = opF.length > 3 && opF.every((f, i) => i === 0 || f.scr[2] >= opF[i - 1].scr[2] - 0.5) && opF[opF.length - 1].scr[2] >= 385;
  ok('[opening] entering the story plays the opening: the page\'s rectangle starts at the preview\'s (left, width within 6 %, top within 30 px) and grows to the screen', !!o0 && Math.abs(o0[0] - fr[0]) <= 0.06 * fr[2] + 2 && Math.abs(o0[2] - fr[2]) <= 0.06 * fr[2] + 2 && Math.abs(o0[1] - fr[1]) <= 30 && grows, { from: fr, first: o0, widths: opF.map(f => f.scr[2]) });
  // a visitor's own tap on the preview: the same click and the same opening
  await R(base()); await W(900); await R('var e=scr.querySelector(\'[data-ref="b:b_story"]\');scr.scrollTop+=e.getBoundingClientRect().top-scr.getBoundingClientRect().top-90;T10c=[];PO.log=[];""'); await W(400);
  let bx = await p.locator('#ph [data-ref="b:b_story"] img').first().boundingBox(); await p.mouse.click(bx.x + bx.width / 2, bx.y + bx.height / 2); await W(700);
  const vis = JSON.parse(await R('JSON.stringify([T10c,PO.log,S.view])'));
  ok('[opening] ...the same as a visitor\'s real tap on the preview (one click, the same opening from the same preview)', vis[2] === 'story' && vis[0].length === 1 && vis[1].length === 1 && vis[1][0].dir === 'open' && vis[1][0].ref === 'b:b_story' && !vis[1][0].fade, vis);
  // back: the closing into the preview, the card where it was
  await R('scr.scrollTop=200;""'); await W(200); bx = await p.locator('#ph .topbar [data-go="card"]').first().boundingBox(); await p.mouse.click(bx.x + bx.width / 2, bx.y + bx.height / 2); await W(150);
  const cl = JSON.parse(await R('JSON.stringify([PO.log.slice(-1)[0],!!ph.querySelector(".po-ghost"),S.view])')); await W(700);
  const clAfter = JSON.parse(await R('(function(){var e=scr.querySelector(\'[data-ref="b:b_story"]\');return JSON.stringify([Math.round(e.getBoundingClientRect().top-scr.getBoundingClientRect().top),!!ph.querySelector(".po-ghost")])})()'));
  ok('[opening] back plays the closing: the page shrinks into the preview, the card where the visitor left it (the preview 90 px down, as before)', cl[0] && cl[0].dir === 'close' && cl[1] && cl[2] === 'card' && Math.abs(clAfter[0] - 90) <= 3 && !clAfter[1], { cl, clAfter });
  tsec('hand, opening');

  // a visitor's touch removes the hand (and pauses), Resume goes on
  await R(base()); await R('S.guideCfg.inUse=false;startTour({id:"g2"});""'); await W(1500);
  await R('stopSpeech();G.paused=false;next();""');
  const seen = await until('!!ph.querySelector(".dhand")&&+getComputedStyle(ph.querySelector(".dhand")).opacity>0.2', 5000);
  bx = await p.locator('#ph').boundingBox(); await p.mouse.move(bx.x + 8, bx.y + 300); await p.mouse.down(); await W(30);
  const gone = JSON.parse(await R('JSON.stringify([!!ph.querySelector(".dhand"),G.paused,!!G.trav,S.view])')); await p.mouse.up(); await W(300);
  await R('resumeTour();""'); const on2 = await until('S.view==="story"&&G.i===3&&!G.trav', 8000);
  ok('[hand] a visitor\'s real touch on the card removes the hand at once and pauses the tour; Resume goes on to the story', seen && !gone[0] && gone[1] === true && !gone[2] && gone[3] === 'card' && on2, { seen, gone, on2 });
  tsec('touch');

  // ---------- [scroll]: the gallery (g8) to the quiz (g9), then back (Previous) ----------
  await R(base()); await R('S.guideCfg.inUse=false;startTour({id:"g8"});""'); await W(2200);
  await R('TS.log=[];T10.start();stopSpeech();G.paused=false;next();""'); await until('TS.log.length>3&&!TS.tw', 5000); await W(600);
  F = JSON.parse(await R('JSON.stringify(T10.stop())')); let sl = JSON.parse(await R('JSON.stringify(TS.log)'));
  const scrollCheck = (sl, dir) => { if (sl.length < 4) return { ok: false, n: sl.length }; const y0 = sl[0][1], y1 = sl[sl.length - 1][1], d = y1 - y0, ms = sl[sl.length - 1][0] - sl[0][0]; let mono = true, maxStep = 0;
    for (let i = 1; i < sl.length; i++) { const s = (sl[i][1] - sl[i - 1][1]) * Math.sign(d); if (s < -0.6) mono = false; maxStep = Math.max(maxStep, Math.abs(sl[i][1] - sl[i - 1][1])) }
    const early = sl.filter(x => x[0] - sl[0][0] <= ms * 0.15).slice(-1)[0], share = early ? Math.abs(early[1] - y0) / Math.abs(d) : 1;
    return { ok: mono && Math.sign(d) === (dir === 'down' ? 1 : -1) && Math.abs(d) > 60 && maxStep <= 0.12 * Math.abs(d) && share < 0.12 && ms >= 420 && ms <= 1300, d: Math.round(d), ms, maxStep: Math.round(maxStep), share: Math.round(share * 100) / 100, mono }; };
  const sd = scrollCheck(sl, 'down'), cueD = F.filter(f => f.cue), cueAfter = await R('!ph.querySelector(".gscue.on")');
  ok('[scroll] the tour scrolls down to the quiz like a flick: monotonic, eased (slow first 15 %), no frame over 12 % of the way, 0.42-1.3 s', sd.ok, sd);
  ok('[scroll] ...the pill shown while it moves, near the bottom, pointing down; gone after', cueD.length > 3 && cueD.every(f => f.cue.dir === 'down' && f.cue.top > 422) && cueAfter, { n: cueD.length, top: cueD[0] && cueD[0].cue, cueAfter });
  await W(900); await R('TS.log=[];T10.start();stopSpeech();G.paused=false;prev();""'); await until('TS.log.length>3&&!TS.tw', 5000); await W(500);
  F = JSON.parse(await R('JSON.stringify(T10.stop())')); sl = JSON.parse(await R('JSON.stringify(TS.log)'));
  const su = scrollCheck(sl, 'up'), cueU = F.filter(f => f.cue);
  ok('[scroll] Previous scrolls up the same way, the pill near the top pointing up', su.ok && cueU.length > 3 && cueU.every(f => f.cue.dir === 'up' && f.cue.top < 300), { su, top: cueU[0] && cueU[0].cue });
  tsec('scroll');

  // ---------- [blocks] ----------
  await R(base()); await R('T10.start();startTour({id:"g2"});""'); await until('DH.useLog.length&&DH.useLog[0].end', 9000); await W(200);
  F = JSON.parse(await R('JSON.stringify(T10.stop())')); const flipU = JSON.parse(await R('JSON.stringify([DH.useLog,G.tryDone,scr.querySelector("[data-flip]").classList.contains("on")])'));
  ok('[blocks] the flip card: flipped by the hand and flipped back (the block\'s own tap), not counted as the visitor\'s try', flipU[0][0] && flipU[0][0].kind === 'flip' && F.some(f => f.flip === true) && flipU[2] === false && !flipU[1], { use: flipU[0], flippedOnce: F.some(f => f.flip === true), end: flipU[2], tryDone: flipU[1] });
  await R(base()); await R('window.T10x=[];startTour({id:"g6"});(function f(){var e=scr.querySelector("[data-ba]");if(e)T10x.push(parseFloat(e.style.getPropertyValue("--x")));if(T10x.length<400&&G.on)requestAnimationFrame(f)})();""');
  await until('DH.useLog.length&&DH.useLog[0].end', 9000); await W(200);
  const bax = JSON.parse(await R('JSON.stringify([Math.max.apply(null,T10x),Math.min.apply(null,T10x),parseFloat(scr.querySelector("[data-ba]").style.getPropertyValue("--x")),DH.useLog])'));
  ok('[blocks] the before and after: dragged across (past 70 % and 30 %) and back to where it was (50 %)', bax[0] > 70 && bax[1] < 30 && Math.abs(bax[2] - 50) <= 1 && bax[3][0].kind === 'ba', bax.slice(0, 3));
  // once per block per tour: Previous then Next back to it
  await R('stopSpeech();G.paused=false;prev();""'); await W(1500); await R('stopSpeech();G.paused=false;next();""'); await W(2600);
  ok('[blocks] ...once per block per tour (Previous, then Next back to it: no second drag)', await R('DH.useLog.filter(function(u){return u.kind==="ba"}).length') === 1);
  await R(base()); await R('startTour({id:"g8"});""'); await R('window.T10x=[];(function f(){var e=scr.querySelector(\'[data-ref="b:b_gal"] .el-car\');if(e)T10x.push(e.scrollLeft);if(T10x.length<500&&G.on)requestAnimationFrame(f)})();""');
  await until('DH.useLog.length&&DH.useLog[0].end', 9000); await W(200);
  const gal = JSON.parse(await R('JSON.stringify([Math.max.apply(null,T10x),scr.querySelector(\'[data-ref="b:b_gal"] .el-car\').scrollLeft,DH.useLog[0]])'));
  ok('[blocks] the gallery\'s strip: swiped one picture on and back', gal[2] && gal[2].kind === 'swipe' && gal[0] > gal[2].l0 + 100 && Math.abs(gal[1] - gal[2].l0) <= 2, gal);
  // the 3D model: no point's tour has one; a 3D Model block on the card, the hand's own turn through its drag handler
  await R(base()); await R('S.card.splice(2,0,{id:"b_3d",kind:"3D Model",grp:"inherit",enter:"inherit",tap:"inherit",orow:"inherit"});renderPhone(false);var e=scr.querySelector(\'[data-ref="b:b_3d"]\');scr.scrollTop+=e.getBoundingClientRect().top-scr.getBoundingClientRect().top-150;""'); await W(400);
  await R('window.T10x=[];var w=dhUseOf(scr.querySelector(\'[data-ref="b:b_3d"]\'));if(w)dhUse(w,"b:b_3d");(function f(){var e=scr.querySelector("[data-turnt]");if(e)T10x.push(e.style.backgroundPositionX||"50%");if(T10x.length<300&&DH.job)requestAnimationFrame(f)})();""');
  await until('DH.useLog.length&&DH.useLog[0].end', 6000); await W(100);
  const md = JSON.parse(await R('JSON.stringify([T10x.filter(function(v){return v!=="50%"}).length,scr.querySelector("[data-turnt]").style.backgroundPositionX,DH.useLog[0]&&DH.useLog[0].kind])'));
  ok('[blocks] the 3D model: turned a little by its own drag and back (its picture at 50 % again)', md[0] > 3 && parseFloat(md[1]) === 50 && md[2] === 'turn', md);
  await R('S.card.splice(2,1);renderPhone(false);""');
  // never while the visitor holds a finger on the screen
  await R(base()); bx = await p.locator('#ph').boundingBox(); await p.mouse.move(bx.x + 8, bx.y + 420); await p.mouse.down();
  await R('startTour({id:"g6"});""'); await W(2400); const held = await R('DH.useLog.length'); await p.mouse.up();
  const after = await until('DH.useLog.length>0', 5000);
  ok('[blocks] never while the visitor holds a finger on the screen: it waits, and plays once the finger lifts', held === 0 && after, { held, after });
  tsec('blocks');

  // ---------- [focus] ----------
  const luma = async (shots, rects) => p.evaluate(async ([a, rects]) => { const L = async src => { const im = new Image(); im.src = src; await im.decode(); const c = document.createElement('canvas'); c.width = im.width; c.height = im.height; const x = c.getContext('2d'); x.drawImage(im, 0, 0);
      return rects.map(r => { const d = x.getImageData(r[0], r[1], r[2], r[3]).data; let s = 0; for (let i = 0; i < d.length; i += 4) s += 0.2126 * d[i] + 0.7152 * d[i + 1] + 0.0722 * d[i + 2]; return s / (d.length / 4) }) };
    return [await L(a[0]), await L(a[1])] }, [shots, rects]);
  const focusAt = async (o) => {
    await R(base(o)); await R(`S.guideCfg.inUse=false;S.guideCfg.cue="${o.cue}";startTour({id:"g1"});""`); await W(2600);
    const geo = JSON.parse(await R(`(function(){var el=stepEl(S.guideSteps[G.i]),bar=ph.querySelector(".gtold.on"),p=ph.getBoundingClientRect(),q=p.width/ph.offsetWidth,r=fcBarRect(el),t=Math.max(r.top,p.top+cardTopClear()*q),m=ph.querySelector(".mascot"),M=m?m.getBoundingClientRect():null,tl=ph.querySelector(".gtele:not(.tl-out)")||ph.querySelector(".gdock"),TL=tl.getBoundingClientRect(),eb=el.getBoundingClientRect();
      var B=bar?bar.getBoundingClientRect():null;return JSON.stringify({bar:B?[B.left-p.left,B.top-p.top,B.width,B.height]:null,blockL:r.left-p.left,blockT:t-p.top,blockB:r.bottom-p.top,elT:eb.top-p.top,elB:eb.bottom-p.top,mascot:M?[M.left-p.left,M.top-p.top,M.right-p.left,M.bottom-p.top]:null,panelT:TL.top-p.top,
        col:bar?getComputedStyle(bar).backgroundColor:null,paper:getComputedStyle(scr).backgroundColor,anim:bar?bar.getAnimations().length:-1,top:scr.scrollTop,clear:cardTopClear()})})()`));
    const during = (await p.locator('#ph').screenshot()).toString('base64');
    await R('var t=scr.scrollTop;stopTour();scr.scrollTop=t;""'); await W(500);
    const before = (await p.locator('#ph').screenshot()).toString('base64');
    const x1 = 22, x2 = Math.round((geo.mascot ? geo.mascot[0] : 320) - 6);
    const rects = [[x1, Math.round(geo.clear + 6), x2 - x1, Math.max(4, Math.round(geo.elT - geo.clear - 14))], [x1, Math.round(geo.elB + 8), x2 - x1, Math.max(4, Math.round(Math.min(geo.panelT, geo.elB + 160) - geo.elB - 16))]];
    const lu = await luma(['data:image/png;base64,' + before, 'data:image/png;base64,' + during], rects);
    return { geo, rects, before: lu[0].map(Math.round), during: lu[1].map(Math.round) };
  };
  const lin = c => { c /= 255; return c <= 0.03928 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4) }, lum = a => 0.2126 * lin(a[0]) + 0.7152 * lin(a[1]) + 0.0722 * lin(a[2]);
  const rgb = s => (s || '').match(/[\d.]+/g).map(Number);
  const contrast = (bar, paper, alpha) => { const B = rgb(bar), P = rgb(paper), m = B.map((v, i) => i < 3 ? v * alpha + P[i] * (1 - alpha) : 1), a = lum(m), c = lum(P); return (Math.max(a, c) + 0.05) / (Math.min(a, c) + 0.05) };
  const fl = await focusAt({ cue: 'bar' }), g = fl.geo;
  ok('[focus] the left bar beside the block told: in the margin, its right edge 6 px from the block\'s left edge (within 2), the block\'s drawn height (within 2)', !!g.bar && Math.abs(g.bar[0] + g.bar[2] - (g.blockL - 6)) <= 2 && Math.abs(g.bar[1] - g.blockT) <= 2 && Math.abs(g.bar[3] - (g.blockB - g.blockT)) <= 2 && g.bar[2] >= 3 && g.bar[2] <= 4.5, g);
  ok('[focus] ...nothing outside the block darker: the card above and below it as bright as before the tour (within 2 %)', fl.during.every((v, i) => v >= fl.before[i] * 0.98), { before: fl.before, during: fl.during, rects: fl.rects });
  const cL = g.col ? contrast(g.col, g.paper, 0.78) : 0;
  const fd = await focusAt({ cue: 'bar', dark: true }), cD = fd.geo.col ? contrast(fd.geo.col, fd.geo.paper, 0.78) : 0;
  ok('[focus] ...its contrast on the card 3:1 or more at its faintest (0.78), light and dark; nothing darker in dark either', cL >= 3 && cD >= 3 && fd.during.every((v, i) => v >= fd.before[i] * 0.98), { light: Math.round(cL * 10) / 10, dark: Math.round(cD * 10) / 10, col: [g.col, fd.geo.col] });
  const fs = await focusAt({ cue: 'spotlight' });
  ok('[focus] Dim the rest (the option) still dims the rest: the same regions 15 % darker or more, no bar', fs.during.every((v, i) => v <= fs.before[i] * 0.85) && !fs.geo.bar, { before: fs.before, during: fs.during });
  tsec('focus');

  // ---------- [reduced motion] ----------
  await R(base({ reduce: true })); await R('S.guideCfg.inUse=false;startTour({id:"g1"});""'); await W(1800);
  const rmBar = JSON.parse(await R('(function(){var b=ph.querySelector(".gtold.on");return JSON.stringify(b?[b.getAnimations().length,getComputedStyle(b).animationName]:null)})()'));
  await R('stopSpeech();G.paused=false;next();""'); await W(1500); await R('DH.log=[];PO.log=[];T10.start();stopSpeech();G.paused=false;next();""');
  const rmGo = await until('S.view==="story"&&G.i===3&&!G.trav', 8000); await W(300); F = JSON.parse(await R('JSON.stringify(T10.stop())'));
  const rl = JSON.parse(await R('JSON.stringify([DH.log,PO.log])'));
  ok('[reduced motion] no glide (no hand drawn on any frame), the ripple and the tap, the story opens with its fade; the bar does not pulse', rmGo && !F.some(f => f.tip) && rl[0].some(e => e.ripple) && !rl[0].some(e => e.glide) && rl[1][0] && rl[1][0].fade && rmBar && rmBar[0] === 0, { rmGo, hand: F.some(f => f.tip), log: rl[0].map(e => Object.keys(e)[0]), po: rl[1], rmBar });
  tsec('reduced motion');

  // ---------- [editor] ----------
  await R(base()); await R('S.tab="guide";S.op={};S.op.gt1=true;S.op.s7focus=true;S.op.s7move=true;S.ui.rowDemo=false;renderEditor();""'); await W(500);
  const rowEl = lab => p.locator('#ed [data-fo="' + (lab === 'Focus' ? 's7focus' : 's7move') + '"] .row', { has: p.locator(`label:text-is("${lab}")`) }).first();
  const pick = async (lab, txt) => { const r = rowEl(lab); await r.scrollIntoViewIfNeeded(); await r.locator('button', { hasText: txt }).first().click(); await W(250); };
  const demo = async lab => { const r = rowEl(lab); await r.scrollIntoViewIfNeeded(); await r.locator('label.rd').first().click(); };
  const E = {};
  await pick('The visitor\'s hand', 'Off'); E.hand = await R('S.guideCfg.hand');
  await R('S.guideCfg.inUse=false;startTour({id:"g2"});""'); await W(1200); await R('DH.log=[];T10.start();stopSpeech();G.paused=false;next();""'); E.offGo = await until('S.view==="story"&&G.i===3&&!G.trav', 8000);
  F = JSON.parse(await R('JSON.stringify(T10.stop())')); E.offHand = F.some(f => f.tip || f.rip); await R('if(G.on)stopTour();""');
  await pick('The visitor\'s hand', 'Shown'); await demo('The visitor\'s hand'); E.handDemo = await until('!!ph.querySelector(".dhand")', 6000);
  await R('if(G.on)stopTour();""'); await pick('Show blocks in use', 'Off');
  await R('DH.useLog=[];startTour({id:"g2"});""'); await W(3000); E.useOff = await R('DH.useLog.length'); await R('if(G.on)stopTour();""');
  await pick('Show blocks in use', 'On'); await R('DH.useLog=[];""'); await demo('Show blocks in use'); E.useDemo = await until('DH.useLog.some(function(u){return u.kind==="flip"})', 6000);
  await R('if(G.on)stopTour();""'); await pick('The scroll', 'Quick'); await R('startTour({id:"g8"});""'); await W(1800); await R('T10.start();stopSpeech();G.paused=false;next();""'); await W(1200);
  F = JSON.parse(await R('JSON.stringify(T10.stop())')); E.quickCue = F.some(f => f.cue); await R('if(G.on)stopTour();""');
  await pick('The scroll', 'Natural'); await R('T10.start();""'); await demo('The scroll'); await W(2600); F = JSON.parse(await R('JSON.stringify(T10.stop())')); E.natCue = F.some(f => f.cue); await R('if(G.on)stopTour();""');
  await pick('Focus', 'Dim the rest'); await demo('Focus'); await W(1600); E.dim = await R('[!!ph.querySelector(".gspot.on"),!!ph.querySelector(".gtold.on")]');
  await pick('Focus', 'Off'); await demo('Focus'); await W(1600); E.off = await R('[!!ph.querySelector(".gspot.on"),!!ph.querySelector(".gtold.on")]');
  await pick('Focus', 'The left bar'); await demo('Focus'); await W(1600); E.bar = await R('[!!ph.querySelector(".gspot.on"),!!ph.querySelector(".gtold.on"),document.body.dataset.rowdemo||""]');
  ok('[editor] The visitor\'s hand Off: no hand and no ripple, the story still opens; its ▸ plays the hand', E.hand === 'off' && E.offGo && !E.offHand && E.handDemo, E);
  ok('[editor] Show blocks in use Off: no flip by the hand; its ▸ plays the flip', E.useOff === 0 && E.useDemo, { useOff: E.useOff, useDemo: E.useDemo });
  ok('[editor] The scroll Quick: no pill; its ▸ (Natural) plays the scroll with its pill', !E.quickCue && E.natCue, { quick: E.quickCue, nat: E.natCue });
  ok('[editor] Focus by real clicks: Dim the rest (the spotlight, no bar), Off (neither), The left bar (the bar, no spotlight), each ▸ played', E.dim[0] && !E.dim[1] && !E.off[0] && !E.off[1] && !E.bar[0] && E.bar[1] && /guideCfg.cue\|found/.test(E.bar[2]), { dim: E.dim, off: E.off, bar: E.bar });
  await R('if(G.on)stopTour();""');
  tsec('editor');

  // ---------- [switches] ----------
  await R('S.tab="card";S.op={};S.ui.accOne=false;renderEditor();""'); await W(400);
  const sw = JSON.parse(await R('JSON.stringify([!!ed.querySelector(\'[data-set="ui.single"]\'),/Many open/.test(ed.textContent),!!ed.querySelector(\'.accbar [data-set="ui.accOne"]\'),/Open to level/.test(ed.textContent)])'));
  await p.locator('.accbar [data-set="ui.accOne"][data-v="true"]').first().click(); await W(300);
  await p.locator('#ed > details[data-fo="s1"] > summary').first().click(); await W(300);
  await p.locator('#ed > details[data-fo="s2"] > summary').first().click(); await W(300);
  const one = JSON.parse(await R('JSON.stringify([S.ui.accOne,!!(ed.querySelector(\':scope > details[data-fo="s1"]\')||{}).open,!!(ed.querySelector(\':scope > details[data-fo="s2"]\')||{}).open])'));
  ok('[switches] the Detail Card tab: the old Foldouts row gone, Open to level kept; One section open at a time (real clicks) closes the open section', !sw[0] && !sw[1] && sw[2] && sw[3] && one[0] === true && !one[1] && one[2], { sw, one });
  await R('S.ui.accOne=false;S.op={};S.tab="guide";renderEditor();""');
  tsec('switches');

  ok('No page errors', errs.length === 0, errs.slice(0, 5));
  console.log(fails ? 'FAILED ' + fails : 'ALL PASS'); await b.close(); process.exit(fails ? 1 : 0);
})();
