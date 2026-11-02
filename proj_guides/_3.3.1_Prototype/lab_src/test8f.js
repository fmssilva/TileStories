// v8f checks with real input (Playwright + the installed Edge): node test8f.js  ->  prints PASS/FAIL per check
const { chromium } = require(process.env.PW || 'C:/Users/franc/Desktop/TileStories_Website/node_modules/playwright');
const url = 'file:///' + __dirname.replace(/\\/g, '/') + '/block-components-lab.html#probe';
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src);
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  const open = async (stop, y) => { await R(`S.view="card";S.stop="${stop}";S.settingsOpen=false;renderAll(false)`); await p.waitForTimeout(700); if (y) { await R(`scr.scrollTop=${y}`); await p.waitForTimeout(300); } };
  const stop = () => R('S.stop'), view = () => R('S.view');
  const box = async sel => p.locator(sel).first().boundingBox();

  // 1. close and settings stay at the sheet's top while the content scrolls
  for (const st of ['half', 'full']) {
    await open(st, 0);
    const a = await R('[scr.querySelector(".chrome i").getBoundingClientRect().top-scr.getBoundingClientRect().top]');
    await R('scr.scrollTop=200'); await p.waitForTimeout(300);
    const c = await R('[scr.querySelector(".chrome i").getBoundingClientRect().top-scr.getBoundingClientRect().top, getComputedStyle(scr.querySelector(".chrome")).visibility, scr.scrollTop]');
    ok(`close/settings pinned at ${st} (scrolled 200, under the top band's start)`, Math.abs(a[0] - c[0]) < 1 && c[1] === 'visible' && c[2] === 200, { before: a[0], after: c[0], vis: c[1], scrollTop: c[2] });
  }
  await R('scr.scrollTop=900'); await p.waitForTimeout(300);
  ok('top band shows: the pinned pair hides (no double)', await R('[ph.querySelector(".topband").style.display!=="none", getComputedStyle(scr.querySelector(".chrome")).visibility].join()') === 'true,hidden');

  // 2. the hero keeps one action
  await open('full', 0);
  ok('hero has the guide button and no Listen pill', await R('!!scr.querySelector(".hero .gstart")&&!scr.querySelector(".hlisten")'));
  ok('Header options no longer offer Listen', await R('KOPT.Header.every(function(o){return o[0]!=="listen"})&&!("listen" in S.blockDefs.Header.o)'));

  // 3. the grabber: tap = one stop; short drag = one stop its way; keyboard
  await open('half', 0);
  let g = await box('#grab'); await p.mouse.click(g.x + g.width / 2, g.y + g.height / 2); await p.waitForTimeout(500);
  ok('tap grabber: half -> full', await stop() === 'full');
  ok('grabber names itself Collapse at full', await R('document.getElementById("grab").getAttribute("aria-label")') === 'Collapse card');
  g = await box('#grab'); await p.mouse.click(g.x + g.width / 2, g.y + g.height / 2); await p.waitForTimeout(500);
  ok('tap grabber: full -> half', await stop() === 'half');
  g = await box('#grab'); await p.mouse.move(g.x + 48, g.y + 16); await p.mouse.down(); await p.mouse.move(g.x + 48, g.y - 10, { steps: 4 }); await p.mouse.move(g.x + 48, g.y - 44, { steps: 4 }); await p.mouse.up(); await p.waitForTimeout(500);
  ok('short drag up (60px): half -> full', await stop() === 'full');
  g = await box('#grab'); await p.mouse.move(g.x + 48, g.y + 16); await p.mouse.down(); await p.mouse.move(g.x + 48, g.y + 76, { steps: 6 }); await p.mouse.up(); await p.waitForTimeout(500);
  ok('short drag down (60px): full -> half', await stop() === 'half');
  g = await box('#grab'); await p.mouse.move(g.x + 48, g.y + 16); await p.mouse.down(); await p.mouse.move(g.x + 48, g.y + 36, { steps: 3 }); await p.mouse.up(); await p.waitForTimeout(500);
  ok('tiny drag (20px) stays at half', await stop() === 'half');
  await p.focus('#grab'); await p.keyboard.press('Enter'); await p.waitForTimeout(400);
  ok('Enter on the grabber: half -> full', await stop() === 'full');
  await open('half', 0);
  const h = await box('.scr .hero'); await p.mouse.click(h.x + h.width * 0.75, h.y + 60); await p.waitForTimeout(500);
  ok('tap the half card picture: half -> full', await stop() === 'full');
  await open('peek', 0);
  const pr = await box('.peekrow'); await p.mouse.click(pr.x + pr.width / 2, pr.y + 20); await p.waitForTimeout(500);
  ok('tap the peek row: peek -> half', await stop() === 'half');
  await open('full', 0);
  const hf = await box('.scr .hero'); await p.mouse.click(hf.x + hf.width * 0.75, hf.y + 120); await p.waitForTimeout(400);
  ok('a tap on the full card picture does nothing', await stop() === 'full');

  // 4. the demo card closes back to where it came from; its settings open
  await open('full', 0);
  await R('S.view="democard";renderPhone(false)'); await p.waitForTimeout(600);
  let x = await box('.scr .chrome i:first-child'); await p.mouse.click(x.x + 17, x.y + 17); await p.waitForTimeout(500);
  ok('demo card X -> back to the card at full', (await view()) === 'card' && (await stop()) === 'full');
  await open('closed', 0);
  await R('S.view="democard";renderPhone(false)'); await p.waitForTimeout(600);
  x = await box('.scr .chrome i:last-child'); await p.mouse.click(x.x + 17, x.y + 17); await p.waitForTimeout(400);
  ok('demo card settings open the panel', await R('!!ph.querySelector(".vset")'));
  await p.locator('[data-vset-close]').click(); await p.waitForTimeout(300);
  x = await box('.scr .chrome i:first-child'); await p.mouse.click(x.x + 17, x.y + 17); await p.waitForTimeout(500);
  ok('demo card X (opened from the wall) -> the wall', (await view()) === 'card' && (await stop()) === 'closed');
  await open('full', 900);
  const tx = await box('.topband i:first-child'); await p.mouse.click(tx.x + 10, tx.y + 10); await p.waitForTimeout(400);
  ok('top band X closes the card', await stop() === 'closed');

  // 5. a change redraws in place: scroll, hero and what is shown stay
  for (const [name, sel] of [['light/dark', '#ptool [data-pt="dark"]'], ['language', '#ptool [data-pt="lang"]'], ['motion', '#ptool [data-pt="reduce"]']]) {
    await open('full', 700);
    const s0 = await R('[scr.scrollTop,scr.querySelector(".hero").offsetHeight,scr.querySelectorAll(".mv.in").length,S.dark,S.lang,S.reduce]');
    await R('window.__s=[];var t0=performance.now();(function f(){window.__s.push([scr.scrollTop,scr.querySelector(".hero").offsetHeight]);if(performance.now()-t0<900)requestAnimationFrame(f)})()');
    await p.click(sel); await p.waitForTimeout(1000);
    const s = await p.evaluate(() => window.__s), s1 = await R('[scr.scrollTop,scr.querySelector(".hero").offsetHeight,scr.querySelectorAll(".mv.in").length,S.dark,S.lang,S.reduce]');
    const moved = s.filter(r => r[0] !== s0[0] || r[1] !== s0[1]).length;
    ok(`${name}: the setting changed and nothing moved`, s0.slice(3).join() !== s1.slice(3).join() && moved === 0 && s1[2] >= s0[2], { before: s0, after: s1, framesMoved: moved });
  }
  await open('full', 0);
  const darkMs = await R('var a=performance.now();document.querySelector(\'#ptool [data-pt="dark"]\').click();Math.round(performance.now()-a)');
  ok('light/dark repaints without rebuilding the editor (< 150 ms)', darkMs < 150, { ms: darkMs });
  ok('editor previews follow dark', await R('S.dark===ed.querySelector(".cv").classList.contains("dark")'));

  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
