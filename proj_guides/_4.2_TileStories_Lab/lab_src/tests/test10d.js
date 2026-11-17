// test10d (Playwright + the installed Edge): node test10d.js -> PASS/FAIL per check. Merge round P1a.2: the screen router
// (screen/router.js) and the non-AR path (screen/explore.js), by real taps.
// [order] a marker opens the card (the sheet, no screen change), a page and back (the history: a screen returned to leaves it),
//   Explore from the wall's button and back; [back] the top overlay first, then the sheet, then the screen before, then nothing;
// [overlays] Settings (the card's chrome), the ✦ menu and Ask about the view (the ✦ menu's item) in ui.overlays, in order, their old
//   flags gone; [event] exactly one screenChanged per change, none for a change to the same state; [routed] the build's count of
//   the router's state written elsewhere is 0;
// [explore] the non-AR path end to end: the preview bar's No camera, Explore (no wall, every point with a card), a point's card at
//   Full by a tap, its tour from the hero's Start, until the guide speaks a line; closing the card goes back to Explore.
// MUTATE=1: back ignores the overlays, every change is told twice, and the camera is never off: the checks named [back], [event]
//   and [explore] turn red.
const { chromium, labUrl } = require('./pw');
const { execFileSync } = require('child_process'), path = require('path');
const url = labUrl('#probe');
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url, { timeout: 90000 }); await p.waitForFunction(() => window.LAB && window.LAB.run, null, { timeout: 60000 }); await p.waitForTimeout(600);
  const R = src => p.evaluate(s => LAB.run(s), src), W = ms => p.waitForTimeout(ms), T0 = Date.now();
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  const tap = async sel => { await p.click(sel); await W(350) };
  // the preview bar (a Lab tool floating over the phone's top) shown only to press its own buttons
  const pbar = async sel => { await R('LAB.previewBar=true;renderPhone(true);""'); await p.click(sel); await W(350); await R('LAB.previewBar=false;renderPhone(true);""'); await W(150) };
  // the events, as the page tells them: each change's kind and ids
  await R('window.__sc=[];on("screenChanged",function(d){window.__sc.push(d.kind+":"+(d.id||d.to)+(d.kind==="stop"?":"+d.stop:""))});""');
  const ev = () => R('var e=window.__sc.slice();window.__sc.length=0;JSON.stringify(e)').then(JSON.parse);
  const st = () => R('JSON.stringify({view:ui.view,stop:ui.stop,history:ui.history,overlays:ui.overlays})').then(JSON.parse);
  if (process.env.MUTATE) { await R('var b0=back;back=function(){ui.overlays.length=0;return b0()};var s0=screenChanged;screenChanged=function(d){s0(d);s0(d)};cameraOn=function(){return true};""'); console.log('MUTATION: back ignores the overlays, every change told twice, the camera never off'); }
  const reset = 'if(G.on)stopTour();if(G.gd.pages)gdPagesEnd("test",false);gdStop("test");closeOverlays();LAB.previewBar=false;LAB.noCamera=false;D.guidance.on=false;D.card.hints.on=false;P.guideMute=true;P.introSeen=true;P.lang="en";pointOpen("painting");goScreen("card",{stop:"closed",reset:true});renderAll(false);window.__sc.length=0;""';
  await R(reset); await W(400);

  // ---------- [routed] ----------
  const cs = execFileSync('node', [path.join(__dirname, '..', 'check_structure.js')], { encoding: 'utf8' });
  ok('[routed] the structure check: no write of ui.view, ui.stop, ui.history, ui.overlays or ui.device outside src/screen/ (0 allowed)', /ok\s+screen writes outside the router 0 \(allowed 0\)/.test(cs), cs.split('\n').filter(l => /screen writes/.test(l)));
  ok('[overlays] the old flags are gone (ui.settingsOpen, ui.contentsOpen, G.fab, G.cam.open, G.gd.tryOpen)', await R('!("settingsOpen" in ui)&&!("contentsOpen" in ui)&&typeof G.fab==="undefined"&&!("open" in G.cam)&&!("tryOpen" in G.gd)'));

  // ---------- [order] [event]: the card, a page, back ----------
  await tap('#ph [data-marker="painting"]');
  let s = await st(), e = await ev();
  ok('[order] a tap on the marker: the card sheet opens at its stop over the wall (the same screen, nothing in the history)', s.view === 'card' && s.stop === await R('D.card.cardCfg.openAt') && s.history.length === 0 && s.overlays.length === 0, s);
  ok('[event] ...one screenChanged (the sheet\'s stop)', e.length === 1 && /^stop:card:/.test(e[0]), e);
  await R('sheetTo("full");renderPhone(false);window.__sc.length=0;""'); await W(400);
  await R('scr.scrollTop=0;var t=scr.querySelector(\'[data-go="story"],[data-go="gallery"]\');t&&t.scrollIntoView({block:"center"});""'); await W(300);
  const pageSel = await R('var t=scr.querySelector(\'[data-go="story"],[data-go="gallery"]\');t?(t.dataset.go):""');
  await tap(`#scr [data-go="${pageSel}"]`);
  s = await st(); e = await ev();
  ok('[order] a page opened by a tap: its screen, the card in the history', s.view === pageSel && JSON.stringify(s.history) === '["card"]', [pageSel, s]);
  ok('[event] ...one screenChanged (screen)', e.length === 1 && e[0] === 'screen:' + pageSel, e);
  await tap('#scr .topbar [data-go="card"]');
  s = await st(); e = await ev();
  ok('[order] the page\'s back arrow (go card): back to the card at its stop, the card out of the history (no loop)', s.view === 'card' && s.stop === 'full' && s.history.length === 0, s);
  ok('[event] ...one screenChanged', e.length === 1 && e[0] === 'screen:card', e);
  await R('goScreen("card");goScreen("card",{stop:"full"});setOverlay("menu",false);""'); e = await ev();
  ok('[event] a change to the same state tells nothing', e.length === 0, e);

  // ---------- [overlays] [back] ----------
  await R('sheetTo("half");renderPhone(false);window.__sc.length=0;""'); await W(400);
  const setSel = await R('var c=[].slice.call(ph.querySelectorAll(".chrome i, .topband i")).filter(function(i){return i.querySelector("svg")&&i.innerHTML.indexOf("M4 7h9")>=0})[0];c?(c.setAttribute("data-t10d","set"),1):0');
  await tap('#ph [data-t10d="set"]');
  s = await st(); e = await ev();
  ok('[overlays] Settings by a real tap on the card\'s chrome: the overlay "settings", its panel shown', JSON.stringify(s.overlays) === '["settings"]' && await R('!!ph.querySelector(".vset")'), [setSel, s]);
  ok('[event] ...one screenChanged (open:settings)', e.length === 1 && e[0] === 'open:settings', e);
  let k = await R('back()'); s = await st(); e = await ev();
  ok('[back] back with Settings open: Settings closes first, the card stays at its stop', k === 'settings' && s.overlays.length === 0 && s.view === 'card' && s.stop === 'half' && await R('!ph.querySelector(".vset")'), [k, s]);
  ok('[event] ...one screenChanged (close:settings)', e.length === 1 && e[0] === 'close:settings', e);
  k = await R('back()'); s = await st(); e = await ev();
  ok('[back] back again: the sheet closes, the wall', k === 'sheet' && s.view === 'card' && s.stop === 'closed', [k, s]);
  ok('[event] ...one screenChanged (stop closed)', e.length === 1 && e[0] === 'stop:card:closed', e);
  ok('[back] back on the wall with nothing behind: nothing (null)', (await R('back()')) === null && (await ev()).length === 0);
  // the ✦ menu and Ask about the view on the wall
  await tap('#ph .gbtn');
  s = await st(); e = await ev();
  ok('[overlays] the ✦ by a real tap on the wall: the overlay "menu", its menu shown', JSON.stringify(s.overlays) === '["menu"]' && await R('!!ph.querySelector(".gbmenu")'), s);
  await tap('#ph .gbmenu [data-gbgo="view"]');
  s = await st(); const e2 = await ev();
  ok('[overlays] its Ask about the view: the menu closes, the overlay "ask" opens (in that order)', JSON.stringify(s.overlays) === '["ask"]' && JSON.stringify(e2) === '["close:menu","open:ask"]', [s, e2]);
  await R('openOverlay("settings");renderPhone(true);window.__sc.length=0;""');
  ok('[overlays] two overlays in the order they opened', JSON.stringify((await st()).overlays) === '["ask","settings"]');
  k = await R('back()'); const k2 = await R('back()'); s = await st();
  ok('[back] back twice: the top one first (settings), then ask', k === 'settings' && k2 === 'ask' && s.overlays.length === 0 && s.view === 'card', [k, k2, s]);
  await ev();

  // ---------- [order] Explore from the wall (the camera on) ----------
  await R(reset); await W(400);
  await tap('#ph .xp-open');
  s = await st(); e = await ev();
  ok('[order] the wall\'s Explore button: the Explore screen, the wall in the history', s.view === 'explore' && JSON.stringify(s.history) === '["card"]' && await R('!!scr.querySelector(".xp .xp-row")'), s);
  ok('[event] ...one screenChanged', e.length === 1 && e[0] === 'screen:explore', e);
  await tap('#scr [data-xpwall]');
  s = await st();
  ok('[order] Explore\'s ✕ (with the camera): back to the wall, nothing in the history', s.view === 'card' && s.stop === 'closed' && s.history.length === 0, s);
  await ev();

  // ---------- [explore] the non-AR path, end to end ----------
  await pbar('#ph #ptool [data-pt="camera"]');
  s = await st(); e = await ev();
  const ex = await R('var w=document.getElementById("wall");JSON.stringify({noCam:LAB.noCamera,wall:w?getComputedStyle(w).display:"none",cam:document.getElementById("camera").style.backgroundImage,rows:[].map.call(scr.querySelectorAll("[data-xpopen]"),function(r){return r.dataset.xpopen}),want:explorePoints().map(function(p){return p.id}),x:!!scr.querySelector("[data-xpwall]"),label:(ph.querySelector(\'[data-pt="camera"]\')||{}).textContent})');
  const x = JSON.parse(ex);
  ok('[explore] the preview bar\'s No camera: Explore is home (no history), no wall drawn, no way back to it', s.view === 'explore' && s.history.length === 0 && x.noCam && x.wall === 'none' && x.cam === 'none' && !x.x && x.label === 'No camera', [s, x]);
  ok('[explore] ...every point with a card, in the wall\'s order (the Ribeira Palace, the Torreão, Terreiro do Paço, the Tagus)', x.rows.length === 4 && JSON.stringify(x.rows) === JSON.stringify(x.want) && x.rows[0] === 'painting', x.rows);
  ok('[event] ...one screenChanged', e.length === 1 && e[0] === 'screen:explore', e);
  await tap('#scr [data-xpopen="torreao"]');
  s = await st(); e = await ev();
  const c1 = await R('JSON.stringify({title:L(D.poi.hero.title),hero:!!scr.querySelector(".hero"),top:scr.style.top,wall:(document.getElementById("wall")||{style:{}}).style.display,cam:document.getElementById("camera").style.backgroundImage})');
  ok('[explore] a tap on the Torreão: its card at Full (no wall behind it), Explore in the history', s.view === 'card' && s.stop === 'full' && JSON.stringify(s.history) === '["explore"]' && JSON.parse(c1).title === 'The Torreão' && JSON.parse(c1).hero && JSON.parse(c1).top === '0px' && JSON.parse(c1).wall === 'none' && JSON.parse(c1).cam === 'none', [s, JSON.parse(c1)]);
  ok('[event] ...one screenChanged (the screen and its stop together)', e.length === 1 && e[0] === 'screen:card', e);
  await R('P.guideMute=false;scr.scrollTop=0;""'); await W(300);
  await tap('#ph .hero .gstart');
  await p.waitForFunction(() => LAB.run('!!(G.on&&G.sayNow&&G.sayNow.text)'), null, { timeout: 15000 }).catch(() => {});
  const say = JSON.parse(await R('JSON.stringify({on:!!G.on,say:G.sayNow&&{src:G.sayNow.src,text:G.sayNow.text},step:G.on&&D.tour.guideSteps[G.i]&&L(D.tour.guideSteps[G.i].text),poi:ui.m.poi,view:ui.view,cam:LAB.noCamera})'));
  ok('[explore] the hero\'s Start (a real tap): the Torreão\'s tour plays and the guide speaks its line (the voice\'s source and the words)', say.on && say.say && !!say.say.src && say.say.text && say.say.text.length > 10 && say.poi === 'torreao' && say.view === 'card' && say.cam === true, say);
  await R('stopTour();P.guideMute=true;""'); await W(300); await ev();
  k = await R('back()'); s = await st();
  ok('[explore] back from the card without the camera: Explore again (no wall, no closed stop to show)', k === 'sheet' && s.view === 'explore' && s.stop === 'closed' && s.history.length === 0, [k, s]);
  await R('goScreen("card",{stop:"full"});renderPhone(false);""'); await W(300); await ev();
  await R('sheetTo("closed");renderPhone(false);""'); s = await st(); e = await ev();
  ok('[explore] the card closed (its ✕, a swipe down) without the camera: Explore, told once', s.view === 'explore' && e.length === 1 && e[0] === 'screen:explore', [s, e]);
  await pbar('#ph #ptool [data-pt="camera"]');
  s = await st();
  ok('[explore] Camera back on: the wall is home again', s.view === 'card' && s.stop === 'closed' && !(await R('LAB.noCamera')) && await R('getComputedStyle(document.getElementById("wall")).display!=="none"'), s);

  ok('No page errors', errs.length === 0, errs.slice(0, 5));
  console.log('TIME ' + Math.round((Date.now() - T0) / 1000) + ' s');
  console.log(fails ? 'FAILED ' + fails : 'ALL PASS'); await b.close(); process.exit(fails ? 1 : 0);
})();
