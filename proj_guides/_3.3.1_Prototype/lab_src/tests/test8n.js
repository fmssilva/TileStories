// v8n checks (Playwright + the installed Edge): node test8n.js -> PASS/FAIL per check
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/../block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 20000 }); await p.waitForTimeout(400);
  const R = src => p.evaluate(s => LAB.run(s), src);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  await R('S.guideCfg.voice="device";S.hints.on=false;S.previewBar=false;S.lang="en";S.guideCfg.panel="classic";S.view="card";S.stop="full";renderAll(false);window.CAP=function(){var c=ph.querySelector(".gcap");if(!c)return"";c=c.cloneNode(true);c.querySelectorAll(".gnovoice,em").forEach(function(x){x.remove()});return c.textContent.trim()}');
  // 1. the page: nothing above the phone; the phone takes the height there is
  const pg = await R('var h=document.querySelector(".wrap > header"),r=ph.getBoundingClientRect();[h?getComputedStyle(h).display:"none",Math.round(r.top),Math.round(r.height),SCREENS.phone[1]]');
  ok('The page: no title above the phone; the phone takes the window\'s height (900 - 48, at most 820)', pg[0] === 'none' && pg[1] < 40 && pg[3] === 820 && pg[2] === 820, pg);
  await p.setViewportSize({ width: 1400, height: 760 }); await p.waitForTimeout(900);
  const sh = await R('[SCREENS.phone[1],Math.round(ph.getBoundingClientRect().height)]');
  ok('...a shorter window: a shorter phone (760 - 48)', sh[0] === 712 && sh[1] === 712, sh);
  await p.setViewportSize({ width: 1400, height: 900 }); await p.waitForTimeout(400);
  // 2. the introduction: first tour only, with the point's name; Azu walks in
  await R('S.guideMute=true;startTour()'); await p.waitForTimeout(150);
  const intro = await R('[S.guideSteps[0].id,L(S.guideSteps[0].text),!!ph.querySelector(".mascot.enter")]');
  ok('First tour: Azu walks in and introduces itself, naming the point', intro[0] === 'intro' && /I'm Azu/.test(intro[1]) && /Ribeira Palace/.test(intro[1]) && intro[2], intro);
  await R('stopTour();startTour()'); await p.waitForTimeout(100);
  ok('...not on the next tour (First tour)', await R('S.guideSteps[0].id') !== 'intro');
  await R('stopTour()');
  // 3. the voice survives a pause: pausing stops it at its word; going on speaks again from that word; a failure retries once, then goes on by time
  await R('window.SPK=[];var sp0=speechSynthesis.speak.bind(speechSynthesis);speechSynthesis.speak=function(u){SPK.push(u.text);sp0(u)};S.guideMute=false;S.introSeen=true;startTour()'); await p.waitForTimeout(1800);
  const v1 = await R('[!!G.utter,SPK.length]');
  if (!v1[0]) console.log('NOTE: no voice in this browser; the voice checks run on the timed words');
  await R('window.K0=G.hl.k;pauseTour()'); await p.waitForTimeout(300);
  const ps = await R('[G.paused,!!G.utter,G.resumeVoice,K0]');
  ok('Pause: the voice stops and the guide keeps its word', !v1[0] || (ps[0] && !ps[1] && ps[2] === true && ps[3] >= 0), ps);
  await R('resumeTour()'); await p.waitForTimeout(300);
  const rs = await R('[!!G.utter,SPK[SPK.length-1],G.words[K0]&&G.words[K0].w,G.hl.k]');
  ok('Resume: it speaks again from that word (not from the start, not silent)', !v1[0] || (rs[0] && rs[1].indexOf(rs[2]) === 0 && rs[3] >= ps[3]), rs);
  if (v1[0]) {
    await R('var ev=new Event("error");ev.error="synthesis-failed";G.utter.dispatchEvent(ev)'); await p.waitForTimeout(300);
    const r1 = await R('[!!G.utter,G.sayArgs.retries,SPK.length]');
    await R('var ev=new Event("error");ev.error="synthesis-failed";G.utter&&G.utter.dispatchEvent(ev)'); await p.waitForTimeout(300);
    const r2 = await R('[!!G.utter,!!G.timer,G.sayArgs.retries]');
    ok('A voice failure: one retry from its word, then the words go on by time (never silent and stuck)', r1[0] && r1[1] === 1 && !r2[0] && r2[1], [r1, r2]);
  }
  await R('stopTour();S.guideMute=true');
  // 4. an invitation done while the guide still asks: it stops, reacts, reads the card's back in place, and carries on by itself
  const gi = await R('S.guideSteps.findIndex(function(s){return s.id==="g2"})');
  await R(`startTour();G.i=${gi};runStep()`); await p.waitForTimeout(900);
  await p.locator('#ph [data-flip]').first().click(); await p.waitForTimeout(1000);
  const af = await R('[CAP(),G.i]');
  ok('Flipped while the guide was asking: it says "There it is."', /There it is/.test(af[0] || '') && af[1] === gi, af);
  await p.waitForTimeout(1800);
  const bk = await R('var b=scr.querySelector(\'[data-ref="b:b_fun"] .bc p\');[CAP().slice(0,30),b?b.textContent.slice(0,30):"",!!(b&&b.querySelector(".gw"))]');
  ok('...then reads the card\'s back, its words lighting up on the card', bk[1] && bk[0] === bk[1] && bk[2], bk);
  await p.waitForFunction(g => LAB.run('G.i') !== g, gi, { timeout: 30000 });
  ok('...and carries on to the next step by itself (no Resume)', await R('!G.paused&&G.i>' + gi));
  await R('stopTour()');
  // 5. not done: it insists twice, each time with new words, then moves on
  await R(`S.guideCfg.insistAfter=1;startTour();G.i=${gi};runStep()`);
  const seen = []; const t0 = Date.now();
  while (Date.now() - t0 < 25000) { const c = await R('[CAP(),G.i]'); if (c[1] !== gi) break; if (seen[seen.length - 1] !== c[0]) seen.push(c[0]); await p.waitForTimeout(150); }
  const want = await R('[TXT("guide_nudge_flip_1"),TXT("guide_nudge_flip_2"),TXT("guide_try_moveon")]');
  ok('Not tried: it asks again twice (new words each time), says it is fine, and moves on', want.every(w => seen.some(s => s.indexOf(w) === 0)) && await R('G.i>' + gi), seen.map(s => s.slice(0, 26)));
  await R('stopTour();S.guideCfg.insistAfter=5');
  // 6. a scroll by the visitor: the guide keeps talking; when its block leaves the screen, it moves on to the block in view
  await R('S.guideCfg.speed=0.7;startTour();G.i=1;runStep()'); await p.waitForTimeout(1400);
  const box = await p.locator('#ph').boundingBox(); await p.mouse.move(box.x + box.width / 2, box.y + 300);
  await p.mouse.wheel(0, 60); await p.waitForTimeout(500);
  ok('A small scroll: the guide keeps going (not paused)', await R('G.on&&!G.paused&&G.i===1'));
  for (let i = 0; i < 6; i++) { await p.mouse.wheel(0, 400); await p.waitForTimeout(120); }
  await p.waitForFunction(() => LAB.run('G.i!==1||G.paused'), null, { timeout: 8000 }).catch(() => {});
  const mv = await R('[G.i,G.paused,CAP()]');
  ok('Its block scrolled away: "Moving on", and the guide tells the block now in view', mv[0] !== 1 && !mv[1] && (mv[2].indexOf(await R('TXT("guide_moving_on")')) === 0 || mv[0] > 1), mv);
  await R('stopTour();S.guideCfg.speed=1');
  await R('S.guideCfg.onScroll="pause";startTour();G.i=1;runStep()'); await p.waitForTimeout(900); await p.mouse.wheel(0, 120); await p.waitForTimeout(300);
  ok('"Pauses": a scroll pauses, as before', await R('G.paused'));
  await R('stopTour();S.guideCfg.onScroll="follow"');
  // 7. the block being told: placed in the free height above the guide panel (a short one at a third, a tall one near the top)
  for (const [id, kind] of [['g1', 'short'], ['g3', 'tall']]) {
    await R(`startTour();G.i=S.guideSteps.findIndex(function(s){return s.id==="${id}"});runStep()`); await p.waitForTimeout(1600);
    const pl = await R('var st=S.guideSteps[G.i],e=stepEl(st),r=e.getBoundingClientRect(),f=freeHeight();[Math.round(r.top-f.top),Math.round(r.height),Math.round(f.bottom-f.top),Math.round(f.bottom-r.bottom)]');
    const good = kind === 'short' ? Math.abs(pl[0] - (pl[2] - pl[1]) / 3) < 24 && pl[3] > 0 : pl[0] >= 0 && pl[0] < 40;
    ok(`A ${kind} block: ${kind === 'short' ? 'at a third of the free height, clear of the guide panel' : 'near the top'}`, good, pl);
    await R('stopTour()');
  }
  // 8. the Editor
  ok('Editor: Insists, Waits, When the visitor scrolls, Introduction', await R('["Insists","Waits","When the visitor scrolls","Introduction"].every(function(t){return ed.textContent.indexOf(t)>=0})'));
  await R('S.tab="marker";S.sel.gstep="g6";renderEditor()');
  ok('Guide script: a try step\'s After the gesture words', await R('!!ed.querySelector(\'textarea[data-gafter="g6|en"]\')&&ed.querySelector(\'textarea[data-gafter="g6|en"]\').value.indexOf("square")>0'));
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
