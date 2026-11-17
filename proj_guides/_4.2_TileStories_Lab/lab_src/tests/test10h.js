// test10h (Playwright + the installed Edge): node test10h.js -> PASS/FAIL per check. Merge round P1b.1: the one dark mode and the
// theme's picks, by real clicks (core/theme.js, editor/app-tab.js, phone/redraw.js, phone/settings.js).
// [mode] the visitor's Settings › Dark (real taps on the card's chrome, then Dark): the phone's mode class swaps and nothing else of its
//   theme classes; its ground and ink are Unity's dark sheet-open and ink; Light again; the preview bar's ☾ does the same; a visitor
//   choice takes no Undo step; the wall's Default Mode = Dark (App tab, a real click) darkens a visitor on Follow the phone;
// [one] one switch: no element carries the old "dark" class, no stylesheet rule keys on .dark / .cv.dark / #ph.dark, P has no dark,
//   the card has no mode, accent or roundness of its own, no style but the theme's sets a --ts-* token, every Editor preview (.cv)
//   is a marked root in the phone's mode;
// [look] App › Look = Playful (a real click): the phone's look class, its rounded titles (Fredoka), its picks (cobalt, round) reach
//   the phone; Undo (the top bar's button) returns Premium, Fraunces and heritage; Redo Playful again;
// [accent] App › Accent = Garden (a swatch, a real click): the Story block's Open Row in Unity's garden; Undo returns heritage;
// [knobs] App › Roundness = Sharp, Density = Compact, Glass = Solid (real clicks): the card's pictures' corners, Explore's gaps and the
//   wall's Explore button's glass change as the table says.
// MUTATE=1: the mode never leaves light, the wall's picks are ignored and one module keeps a dark rule of its own: [mode], [one], [look],
//   [accent] and [knobs] turn red.
const { chromium, labUrl } = require('./pw');
const url = labUrl('#probe');
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url, { timeout: 90000 }); await p.waitForFunction(() => window.LAB && window.LAB.run, null, { timeout: 60000 }); await p.waitForTimeout(600);
  const R = src => p.evaluate(s => LAB.run(s), src), W = ms => p.waitForTimeout(ms), T0 = Date.now();
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  const tap = async sel => { await p.locator(sel).first().scrollIntoViewIfNeeded(); await p.click(sel); await W(350) };
  if (process.env.MUTATE) {
    await R('themeMode=function(){return"light"};themePicks=function(){return{look:"premium",accent:"heritage",roundness:"soft",density:"comfortable",glass:"blur",motion:"follow"}};var st=document.createElement("style");st.textContent=".dark .sec{color:#F00}";document.head.appendChild(st);""');
    console.log('MUTATION: the mode never leaves light, the wall\'s picks ignored, a module\'s own .dark rule');
  }
  const reset = 'if(G.on)stopTour();if(G.gd.pages)gdPagesEnd("test",false);gdStop("test");closeOverlays();LAB.previewBar=false;LAB.noCamera=false;D.guidance.on=false;D.card.hints.on=false;P.guideMute=true;P.introSeen=true;P.lang="en";P.vis.mode="phone";P.reduce=false;' +
    'Object.assign(D.wall.theme,{mode:"light",look:"premium",accent:"heritage",roundness:"soft",density:"comfortable",glass:"blur",motion:"follow"});applyTokens();pointOpen("painting");goScreen("card",{stop:"half",reset:true});ui.tab="app";ui.level=9;ui.op={};renderAll(false);undoPast.length=0;undoFuture.length=0;emit("undo.changed");""';
  await R(reset); await W(500);
  const cls = el => R(`[].slice.call((${el}).classList).filter(function(c){return/^ts-/.test(c)}).join(" ")`);
  const look = () => R('var c=getComputedStyle(ph),s=getComputedStyle(scr);JSON.stringify({bg:s.backgroundColor==="rgba(0, 0, 0, 0)"?c.backgroundColor:s.backgroundColor,ink:c.color})').then(JSON.parse);

  // ---------- [mode] the visitor's Settings, by real taps ----------
  const c0 = await cls('ph'), l0 = await look();
  await R('var c=[].slice.call(ph.querySelectorAll(".chrome i, .topband i")).filter(function(i){return i.querySelector("svg")&&i.innerHTML.indexOf("M4 7h9")>=0})[0];c&&c.setAttribute("data-t10h","set");""');
  await tap('#ph [data-t10h="set"]'); await tap('#ph [data-vis="mode|dark"]');
  const c1 = await cls('ph'), l1 = await look(), steps1 = await R('undoPast.length');
  const diff = (a, b) => { const A = a.split(' '), B = b.split(' '); return { gone: A.filter(x => !B.includes(x)), came: B.filter(x => !A.includes(x)) } };
  const d1 = diff(c0, c1);
  ok('[mode] Settings › Dark (real taps): one class swapped on the phone (ts-mode-light -> ts-mode-dark), the other theme classes kept', JSON.stringify(d1) === '{"gone":["ts-mode-light"],"came":["ts-mode-dark"]}', { c0, c1 });
  ok('[mode] ...the phone on Unity\'s dark tokens: the ground sheet-open #0F1012, the ink #F6F1E8 (it was #F8F6F2 / #14110D)', l1.ink === 'rgb(246, 241, 232)' && l0.ink === 'rgb(20, 17, 13)' && /rgb\(15, 16, 18\)/.test(l1.bg) && /rgb\(248, 246, 242\)/.test(l0.bg), { l0, l1 });
  ok('[mode] ...a visitor\'s choice: P.vis.mode, no Undo step', (await R('P.vis.mode')) === 'dark' && steps1 === 0, steps1);
  await tap('#ph [data-vis="mode|light"]');
  ok('[mode] Settings › Light: light again (the same classes as before)', (await cls('ph')) === c0, await cls('ph'));
  await R('closeOverlays();LAB.previewBar=true;renderPhone(true);""'); await W(300);
  await tap('#ph #ptool [data-pt="dark"]'); const c2 = await cls('ph'); await tap('#ph #ptool [data-pt="dark"]'); const c3 = await cls('ph');
  ok('[mode] the preview bar\'s ☾ (the visitor\'s mode): dark, then light, the same one class', JSON.stringify(diff(c0, c2)) === JSON.stringify(d1) && c3 === c0, { c2, c3 });
  await R('LAB.previewBar=false;P.vis.mode="phone";renderAll(true);""'); await W(300);
  await tap('#ed [data-set="@D.wall.theme.mode"][data-v="dark"]');
  ok('[mode] App › Default mode = Dark (a real click): a visitor on Follow the phone sees dark; one Undo step', (await cls('ph')).indexOf('ts-mode-dark') >= 0 && (await R('undoPast.length')) === 1, await cls('ph'));
  await tap('#undo'); ok('[mode] ...Undo: light again', (await cls('ph')).indexOf('ts-mode-light') >= 0, await cls('ph'));

  // ---------- [one] one switch ----------
  const one = JSON.parse(await R(`(function(){var rules=[];[].forEach.call(document.styleSheets,function(s){var rs;try{rs=s.cssRules}catch(e){return}
    (function walk(list){[].forEach.call(list,function(r){if(r.cssRules&&!r.selectorText)walk(r.cssRules);if(r.selectorText&&/(^|[\\s,>+~])\\.dark\\b|\\.cv\\.dark|#ph\\.dark/.test(r.selectorText))rules.push(r.selectorText.slice(0,80))})})(rs)});
    var tokenSetters=0;[].forEach.call(document.styleSheets,function(s){if(s.ownerNode&&s.ownerNode.id==="ts-theme")return;var rs;try{rs=s.cssRules}catch(e){return}
      (function walk(list){[].forEach.call(list,function(r){if(r.cssRules&&!r.selectorText)walk(r.cssRules);if(r.style)for(var i=0;i<r.style.length;i++)if(/^--ts-/.test(r.style[i]))tokenSetters++})})(rs)});
    [].forEach.call(document.querySelectorAll("[style]"),function(e){for(var i=0;i<e.style.length;i++)if(/^--ts-/.test(e.style[i]))tokenSetters++});
    var cvs=[].slice.call(ed.querySelectorAll(".cv")),mode="ts-mode-"+themeMode();
    return JSON.stringify({darkEls:document.querySelectorAll(".dark").length,rules:rules,pDark:"dark" in P,card:["mode","accent","round"].filter(function(k){return k in D.card.cardCfg}),motionLevel:"motionLevel" in D.card,tokenSetters:tokenSetters,
      cvs:cvs.length,cvsMarked:cvs.filter(function(c){return c.classList.contains("ts-app")&&c.classList.contains(mode)}).length})})()`));
  ok('[one] no element with the old "dark" class, no rule keyed on .dark / .cv.dark / #ph.dark', one.darkEls === 0 && one.rules.length === 0, one.rules.slice(0, 5));
  ok('[one] no P.dark; the card has no mode, accent or roundness of its own, no motion level; no style but the theme\'s sets a --ts-* token', !one.pDark && one.card.length === 0 && !one.motionLevel && one.tokenSetters === 0, one);
  await R('ui.tab="card";renderEditor();P.vis.mode="dark";emit("profileChanged","mode");modeInPlace();""'); await W(300);
  const pv = JSON.parse(await R('var cvs=[].slice.call(ed.querySelectorAll(".cv"));JSON.stringify({n:cvs.length,dark:cvs.filter(function(c){return c.classList.contains("ts-app")&&c.classList.contains("ts-mode-dark")}).length})'));
  ok('[one] every Editor preview (.cv) is a marked root and swaps to dark with the phone', pv.n > 20 && pv.dark === pv.n, pv);
  await R('P.vis.mode="phone";emit("profileChanged","mode");ui.tab="app";renderAll(false);""'); await W(300);

  // ---------- [look] App › Look = Playful, Undo, Redo ----------
  const titleFont = () => R('var t=scr.querySelector(".hero h2, .hero .w h2, .el-title, h2");t?getComputedStyle(t).fontFamily:""');
  const accentNow = () => R('getComputedStyle(ph).getPropertyValue("--ts-accent").trim()');
  const f0 = await titleFont();
  await tap('#ed [data-set="@D.wall.theme.look"][data-v="playful"]');
  const pl = { cls: await cls('ph'), font: await titleFont(), acc: await accentNow() };
  ok('[look] App › Look = Playful (a real click): the phone\'s look class and its picks (cobalt, round, solid)', /ts-look-playful/.test(pl.cls) && /ts-accent-cobalt/.test(pl.cls) && /ts-roundness-round/.test(pl.cls) && /ts-glass-solid/.test(pl.cls), pl.cls);
  ok('[look] ...its rounded titles (Fredoka) and the cobalt accent (#2638B8) on the phone', /Fredoka/.test(pl.font) && /Fraunces/.test(f0) && pl.acc === 'rgb(38, 56, 184)', [f0, pl.font, pl.acc]);
  await tap('#undo');
  const un = { cls: await cls('ph'), font: await titleFont(), acc: await accentNow() };
  ok('[look] ...Undo (the top bar): Premium, Fraunces and heritage (#1E3A8A) again', /ts-look-premium/.test(un.cls) && /ts-accent-heritage/.test(un.cls) && /Fraunces/.test(un.font) && un.acc === 'rgb(30, 58, 138)', un);
  await tap('#redo'); ok('[look] ...Redo: Playful again', /ts-look-playful/.test(await cls('ph')), await cls('ph'));
  await tap('#undo');

  // ---------- [accent] App › Accent = Garden ----------
  await R('sheetTo("full");renderPhone(false);""'); await W(300);
  const openRow = () => R('var s=blockAt("Story"),e=s&&scr.querySelector(s.sel.split("|")[0]+" .open");e?getComputedStyle(e).color:"none"');
  const a0 = await openRow();
  await tap('#ed [data-set="@D.wall.theme.accent"][data-v="garden"]');
  const a1 = await openRow();
  ok('[accent] App › Accent = Garden (a swatch, a real click): the Story block\'s Open Row in Unity\'s garden (#174F33), heritage before', a0 === 'rgb(30, 58, 138)' && a1 === 'rgb(23, 79, 51)', [a0, a1]);
  await tap('#undo'); ok('[accent] ...Undo: heritage again', (await openRow()) === 'rgb(30, 58, 138)', await openRow());

  // ---------- [knobs] roundness, density, glass ----------
  const rad = () => R('var e=scr.querySelector(".fr");e?getComputedStyle(e).borderTopLeftRadius:"none"');
  const r0 = await rad(); await tap('#ed [data-set="@D.wall.theme.roundness"][data-v="sharp"]'); const r1 = await rad();
  ok('[knobs] Roundness = Sharp: the card\'s pictures\' corners change', r0 !== r1 && r1 !== 'none' && /ts-roundness-sharp/.test(await cls('ph')), [r0, r1]);
  await R('goScreen("explore",{stop:"closed"});renderPhone(false);""'); await W(300);
  const gap = () => R('var e=scr.querySelector(".xp-list");e?getComputedStyle(e).rowGap:"none"');
  const g0 = await gap(); await tap('#ed [data-set="@D.wall.theme.density"][data-v="compact"]'); await R('goScreen("explore",{stop:"closed"});renderPhone(false);""'); await W(300); const g1 = await gap();
  ok('[knobs] Density = Compact: Explore\'s gaps one step down (--ts-space-3 12 px -> 8 px)', g0 === '12px' && g1 === '8px', [g0, g1]);
  await R('goScreen("card",{stop:"closed"});renderPhone(false);""'); await W(300);
  const glass = () => R('var e=ph.querySelector(".xp-open");e?getComputedStyle(e).backgroundColor+" | "+getComputedStyle(e).backdropFilter:"none"');
  const s0 = await glass(); await tap('#ed [data-set="@D.wall.theme.glass"][data-v="solid"]'); await R('goScreen("card",{stop:"closed"});renderPhone(false);""'); await W(300); const s1 = await glass();
  ok('[knobs] Glass = Solid: the wall\'s Explore button on the solid tint (0.84, no blur), the blurred glass before (0.78, blur 28 px)', /0\.78\)/.test(s0) && /blur\(28px\)/.test(s0) && /0\.84\)/.test(s1) && /blur\(0px\)/.test(s1), [s0, s1]);

  ok('No page errors', errs.length === 0, errs.slice(0, 5));
  console.log('TIME ' + Math.round((Date.now() - T0) / 1000) + ' s');
  console.log(fails ? 'FAILED ' + fails : 'ALL PASS'); await b.close(); process.exit(fails ? 1 : 0);
})();
