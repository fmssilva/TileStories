// v8g checks with real input (Playwright + the installed Edge): node test8g.js  ->  PASS/FAIL per check
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 20000 }); await p.waitForTimeout(400);
  const R = src => p.evaluate(s => LAB.run(s), src);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  const click = async sel => { const bx = await p.locator(sel).first().boundingBox(); await p.mouse.click(bx.x + bx.width / 2, bx.y + bx.height / 2); await p.waitForTimeout(500); };
  const view = () => R('S.view');

  // 1. the demo: tile -> Demo Components -> demo card; X and Back return
  await R('S.view="card";S.stop="full";S.test.on=true;renderAll(false)'); await p.waitForTimeout(600);
  const tile = await R('var t=scr.querySelector(".testtile");t?[t.dataset.go,t.querySelector("strong").textContent]:null');
  ok('the card\'s tile says Demo Components and opens that page', tile && tile[0] === 'demo' && tile[1] === 'Demo Components', tile);
  await R('var t=scr.querySelector(".testtile");scr.scrollTop+=t.getBoundingClientRect().top-scr.getBoundingClientRect().top-200'); await p.waitForTimeout(300);
  await click('.scr .testtile');
  ok('tap the tile -> Demo Components', await view() === 'demo');
  const btn = await R('var t=scr.querySelector(".demoopen");t?[t.dataset.go,t.querySelector("strong").textContent,t.getBoundingClientRect().top-scr.getBoundingClientRect().top<160]:null');
  ok('its first thing is the Demo card button', btn && btn[0] === 'democard' && btn[1] === 'Demo card' && btn[2], btn);
  await click('.scr .demoopen');
  ok('tap it -> the full demo card, with no tile of its own', await view() === 'democard' && await R('!scr.querySelector(".testtile")'));
  await click('.scr .chrome i:first-child');
  ok('demo card X -> back to Demo Components', await view() === 'demo');
  await click('.scr .topbar [data-go]');
  ok('Demo Components Back -> the card, at its stop', await view() === 'card' && await R('S.stop') === 'full');
  await R('S.test.on=false;renderPhone(true)');
  ok('Tile on every card Off -> no tile', await R('!scr.querySelector(".testtile")'));
  await R('S.test.on=true;renderAll(true)');
  const edrows = await R('var f=ed.querySelector(\'[data-fo="s0card"] > .inner\');[!!ed.querySelector(\'[data-fo="s0tile"]\'),Array.prototype.map.call(f.querySelectorAll(":scope > .row > label"),function(l){return l.firstChild.textContent.trim()}).join("|"),(TXO.s0card||[]).indexOf("test_tile_title")>=0]');
  ok('Editor: one Demo card component (no tile foldout, no On marker), the tile\'s words owned by it', !edrows[0] && /^Tile on every card\|Demo Components\|Demo card/.test(edrows[1]) && !/On marker/.test(edrows[1]) && edrows[2], edrows);

  // 2. hints: each at its moment, one at a time, Off and Reduce Motion respected
  ok('Editor: the Hints component with a Try per hint', await R('ed.querySelectorAll(\'[data-fo="s5hints"] [data-hinttry]\').length') === 6);
  await R('hintTry("navbar")'); await p.waitForTimeout(1600);
  const tip = await R('var t=ph.querySelector(".hinttip"),nb=document.getElementById("navbar");t?[t.textContent,Math.round(nb.getBoundingClientRect().top-t.getBoundingClientRect().bottom)]:null');
  ok('Navigation Bar: the tip "Jump to any part" sits over the bar', tip && tip[0] === 'Jump to any part' && tip[1] > 4, tip);
  await p.waitForTimeout(3200);
  ok('...and goes after about 3 s', await R('!ph.querySelector(".hinttip")'));
  await R('scr.scrollTop=0'); await p.waitForTimeout(500); await R('scr.scrollTop=scr.querySelector(".hero").offsetHeight+80'); await p.waitForTimeout(2600);
  ok('Once a visit: not again when the bar comes back', await R('!ph.querySelector(".hinttip")'));
  await R('S.hints.navbar="off"'); await R('hintTry("navbar")'); await p.waitForTimeout(2600);
  ok('Off: no tip', await R('!ph.querySelector(".hinttip")')); await R('S.hints.navbar="visit";S.hintDone.navbar=true');
  await R('hintTry("strip")'); await p.waitForTimeout(1800);
  ok('Swipe strips: the first strip in view slides once', await R('S.hintDone.strip===true'));
  await R('hintTry("ba")'); const x0 = await R('var e=scr.querySelector("[data-ba]");e?e.style.getPropertyValue("--x"):"none"');
  await R('window.__x=[];var t0=performance.now();(function f(){var e=scr.querySelector("[data-ba]");if(e)window.__x.push(parseFloat(e.style.getPropertyValue("--x")));if(performance.now()-t0<3000)requestAnimationFrame(f)})()'); await p.waitForTimeout(3100);
  const xs = await p.evaluate(() => window.__x); const span = Math.max(...xs) - Math.min(...xs);
  ok('Before / After: the slider sweeps about 12% each way and returns', x0 !== 'none' && span > 20 && Math.abs(xs[xs.length - 1] - parseFloat(x0)) < 0.5, { x0, span: Math.round(span), end: xs[xs.length - 1] });
  await R('hintTry("term")'); await p.waitForTimeout(1900);
  ok('Underlined words: the first one glows once', await R('S.hintDone.term===true'));
  await R('S.reduce=true;hintTry("strip")'); await p.waitForTimeout(2000);
  ok('Reduce Motion: the strip hint is skipped', await R('S.hintDone.strip') === false); await R('S.reduce=false');
  // one at a time: the Locate label is showing -> the bar's tip waits until it folds
  await R('S.hintDone={};S.hintCard={};S.locSeen=false;S.locFolded=false;S.locShown=0;S.nudged=true;S.view="card";S.stop="closed";renderPhone(false)'); await p.waitForTimeout(300);
  await R('ph.querySelector("[data-marker]").click()'); await p.waitForTimeout(800); await R('scr.scrollTop=scr.querySelector(".hero").offsetHeight'); await p.waitForTimeout(2000);
  const w1 = await R('[labelShowing(),!!ph.querySelector(".hinttip"),getComputedStyle(document.getElementById("navbar")).display!=="none"]');
  await p.waitForTimeout(4500);
  const w2 = await R('[labelShowing(),S.hintDone.navbar===true]');
  ok('one at a time: the bar\'s tip waits while the label shows, then comes', w1[0] && !w1[1] && w1[2] && !w2[0] && w2[1], { during: w1, after: w2 });

  // 3. How to use the card: from the settings panel only; the Guide's tour on the card's own controls
  await R('S.view="card";S.stop="half";S.settingsOpen=true;renderPhone(false)'); await p.waitForTimeout(400);
  ok('the settings panel ends with How to use the card', await R('var b=ph.querySelector(".vset .vhow");b?b.textContent.indexOf("How to use the card")===0:false'));
  await click('.vset .vhow'); await p.waitForTimeout(400);
  const t1 = await R('[G.on,S.guideSteps[G.i].id,!!ph.querySelector("#grab.gfocus"),S.stop,!ph.querySelector(".vset")]');
  ok('it starts the tour at the grabber (marked), full, settings closed', t1[0] && t1[1] === 'how0' && t1[2] && t1[3] === 'full' && t1[4], t1);
  await R('next()'); await p.waitForTimeout(400);
  ok('step 2 marks the settings mark', await R('!!ph.querySelector(".chrome i:last-child.gfocus")&&!ph.querySelector("#grab.gfocus")'));
  await R('stopTour()'); await p.waitForTimeout(200);
  ok('stopping restores the card\'s own guide steps', await R('!G.on&&S.guideSteps[0].id==="g0"'));
  await R('S.hints.howto=false;S.settingsOpen=true;renderPhone(true)');
  ok('How to use Hidden -> no button', await R('!ph.querySelector(".vhow")')); await R('S.hints.howto=true;S.settingsOpen=false;renderPhone(true)');

  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
