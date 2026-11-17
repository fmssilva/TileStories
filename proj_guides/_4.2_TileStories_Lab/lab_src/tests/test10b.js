// test10b (Playwright + the installed Edge): node test10b.js -> PASS/FAIL per check. Merge round P1a.1: the state's four roots
// (STATE_MAP.md: D the author's, P the visitor's, ui what is happening, LAB the Lab's) and Undo / Redo on D (core/undo.js, the Editor's
// one write point in editor/shell.js).
// [roots] no S; D's parts named after their Unity homes; P, ui, LAB; a path names its root (setPath / getPath).
// [undo] an Editor change by a real click (Detail Card › Sections › Style: Cards) is one step: Undo (the top bar's button) shows the
//   card as it was, Redo as changed; typing a field (the hero's title, real keys) is one step for the whole edit: Ctrl+Z outside the
//   field shows the old title on the phone, Ctrl+Y the new one; Ctrl+Z inside a text field is the field's own (no step undone);
//   the buttons say what there is to undo / redo; the card's mode undone lightens the phone again (what its Editor change also set);
//   the open point's tour and its written tour stay one after an undo; at most 300 steps.
// [never] a visitor's choice on the phone (Settings › The guide, real taps: P) and a tap on the phone (the grabber: ui) never enter
//   the history, and the self-test's walk (noDraw) takes no step.
// [visitor] Settings › The guide writes P.guide (the visitor's speed and voice), never D.guide; the guide speaks at the visitor's speed;
//   Forget clears it, back to the author's.
// MUTATE=1: the Editor's changes take no step (undoSnap never keeps one): the [undo] checks turn red.
const { chromium, labUrl } = require('./pw');
const url = labUrl('#probe');
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe' });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  await p.goto(url); await p.waitForFunction(() => window.LAB && window.LAB.run, null, { timeout: 30000 }); await p.waitForTimeout(600);
  const R = src => p.evaluate(s => LAB.run(s), src), W = ms => p.waitForTimeout(ms), T0 = Date.now();
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  if (process.env.MUTATE) { await R('undoSnap=function(){return false}'); console.log('MUTATION: the Editor\'s changes take no step'); }
  await R('P.guideMute=true;P.introSeen=true;D.guidance.on=false;D.card.hints.on=false;ui.view="card";ui.stop="half";renderAll(false);""'); await W(500);

  // ---------- [roots] ----------
  const roots = JSON.parse(await R('JSON.stringify({S:typeof S,D:Object.keys(D),P:typeof P,ui:typeof ui,LAB:typeof LAB,run:typeof LAB.run})'));
  ok('[roots] no S left; D\'s parts are its Unity homes (guide, agent, tour, guidance, card, poi, wall, words, blocks); P, ui and LAB are there',
    roots.S === 'undefined' && roots.D.join() === 'guide,agent,tour,guidance,card,poi,wall,words,blocks' && roots.P === 'object' && roots.ui === 'object' && roots.LAB === 'object', roots);
  ok('[roots] a path names its root: setPath / getPath ("D.card.secCfg.look", "LAB.test.on", "ui.level")',
    await R('var a=getPath("D.card.secCfg.look")===D.card.secCfg.look&&getPath("LAB.test.on")===LAB.test.on&&getPath("ui.level")===ui.level;var k=D.card.secCfg.tint;setPath("D.card.secCfg.tint",77);var c=D.card.secCfg.tint===77;setPath("D.card.secCfg.tint",k);a&&c'));

  // ---------- [undo] a real click ----------
  await R('undoPast.length=0;undoFuture.length=0;emit("undo.changed");ui.tab="card";ui.level=9;ui.op={};renderEditor();""'); await W(300);
  const bar0 = JSON.parse(await R('JSON.stringify([document.getElementById("undo").disabled,document.getElementById("redo").disabled])'));
  const before = await R('D.card.secCfg.look+"|"+(scr.innerHTML.indexOf("margin:12px;border-radius:18px")>=0?"drawn as cards":"drawn open")');
  const cards = '[data-set="@D.card.secCfg.look"][data-v="cards"]';
  await p.locator(cards).first().scrollIntoViewIfNeeded(); await p.locator(cards).first().click(); await W(300);
  const after = await R('D.card.secCfg.look+"|"+(scr.innerHTML.indexOf("margin:12px;border-radius:18px")>=0?"drawn as cards":"drawn open")');
  const st1 = JSON.parse(await R('JSON.stringify([undoPast.length,undoFuture.length,document.getElementById("undo").disabled,document.getElementById("undo").title])'));
  ok('[undo] an Editor change by a real click (Sections › Style: Cards) is one step; the top bar\'s Undo is on and says so', bar0[0] && bar0[1] && before === 'open|drawn open' && after === 'cards|drawn as cards' && before !== after && st1[0] === 1 && st1[1] === 0 && !st1[2] && /1 to undo/.test(st1[3]), { bar0, before, after, st1 });
  await p.locator('#undo').click({ timeout: 3000 }).catch(() => {}); await W(300);
  const und = await R('D.card.secCfg.look+"|"+(scr.innerHTML.indexOf("margin:12px;border-radius:18px")>=0?"drawn as cards":"drawn open")');
  const st2 = JSON.parse(await R('JSON.stringify([undoPast.length,undoFuture.length,document.getElementById("redo").disabled,scr.innerHTML.indexOf("cards")>=0])'));
  ok('[undo] Undo (the top bar): the phone shows the card as it was (Open), and Redo is on', und === before && st2[0] === 0 && st2[1] === 1 && !st2[2], { und, before, st2 });
  await p.locator('#redo').click({ timeout: 3000 }).catch(() => {}); await W(300);
  const red = await R('D.card.secCfg.look+"|"+(scr.innerHTML.indexOf("margin:12px;border-radius:18px")>=0?"drawn as cards":"drawn open")');
  ok('[undo] Redo: the phone shows the change again (Cards)', red === after && await R('undoPast.length===1&&undoFuture.length===0'), { red, after });
  await R('undo();""'); await W(200);

  // ---------- [undo] typing a field: one step; Ctrl+Z / Ctrl+Y outside it; inside it, the field's own ----------
  await R('undoPast.length=0;undoFuture.length=0;emit("undo.changed");ui.tab="marker";ui.level=9;ui.op={};ui.sel.page="header";renderEditor();ui.view="card";ui.stop="full";renderPhone(false);scr.scrollTop=0;""'); await W(500);
  const title0 = await R('L(D.poi.hero.title)');
  const inp = p.locator('input[data-path="D.poi.hero.title.en"]').first();
  await inp.scrollIntoViewIfNeeded(); await inp.click(); await p.keyboard.press('End'); await p.keyboard.type(' Undo test', { delay: 30 }); await W(200);
  const phoneNew = await R('scr.querySelector(".hero").textContent');
  await p.keyboard.press('Control+z'); await W(150);
  const inField = JSON.parse(await R('JSON.stringify([undoPast.length,L(D.poi.hero.title)])'));
  await p.locator('.edtitle').click(); await W(300);
  const steps = await R('undoPast.length');
  ok('[undo] typing in the hero\'s title (real keys) shows on the phone; Ctrl+Z inside the field is the field\'s own (no step undone); the whole edit is one step once the field is left',
    phoneNew.indexOf(' Undo test') >= 0 && inField[0] === 0 && steps === 1, { phoneNew: phoneNew.slice(0, 60), inField, steps });
  const typed = await R('L(D.poi.hero.title)');
  await p.keyboard.press('Control+z'); await W(400);
  const phoneOld = JSON.parse(await R('JSON.stringify([L(D.poi.hero.title),scr.querySelector(".hero").textContent,ed.querySelector(\'input[data-path="D.poi.hero.title.en"]\').value])'));
  ok('[undo] Ctrl+Z outside a text field: the phone shows the old title again (and the Editor\'s field)', phoneOld[0] === title0 && phoneOld[1].indexOf(' Undo test') < 0 && phoneOld[1].indexOf(title0) >= 0 && phoneOld[2] === title0, { title0, phoneOld });
  await p.keyboard.press('Control+y'); await W(400);
  const phoneRe = JSON.parse(await R('JSON.stringify([L(D.poi.hero.title),scr.querySelector(".hero").textContent])'));
  ok('[undo] Ctrl+Y: the new title again, on the phone', phoneRe[0] === typed && / Undo tes/.test(typed) && phoneRe[1].indexOf(typed) >= 0, { typed, phoneRe });
  await R('undo();""'); await W(200);

  // ---------- [undo] the card's mode: its Editor change also darkens the phone; undone, the phone is light again ----------
  await R('undoPast.length=0;undoFuture.length=0;emit("undo.changed");P.dark=false;D.card.cardCfg.mode="light";ui.tab="card";ui.level=9;ui.op={};ui.view="card";ui.stop="half";renderAll(false);""'); await W(300);
  const dk = p.locator('[data-set="@D.card.cardCfg.mode"][data-v="dark"]').first();
  await dk.scrollIntoViewIfNeeded(); await dk.click(); await W(300);
  const m1 = await R('[D.card.cardCfg.mode,P.dark,ph.classList.contains("dark")].join()');
  await p.locator('#undo').click({ timeout: 3000 }).catch(() => {}); await W(300);
  const m2 = await R('[D.card.cardCfg.mode,P.dark,ph.classList.contains("dark")].join()');
  await p.locator('#redo').click({ timeout: 3000 }).catch(() => {}); await W(300);
  const m3 = await R('[D.card.cardCfg.mode,P.dark,ph.classList.contains("dark")].join()');
  ok('[undo] the card\'s mode set to Dark darkens the phone; Undo: the phone light again; Redo: dark', m1 === 'dark,true,true' && m2 === 'light,false,false' && m3 === 'dark,true,true', { m1, m2, m3 });
  await R('undo();P.dark=false;renderAll(true);""'); await W(200);

  // ---------- [undo] one tour after an undo; at most 300 steps ----------
  const shared = await R('var a=D.tour.guideSteps===D.tour.tours.painting.written;undoSnap(JSON.stringify(D).replace("\\"svForget\\":\\"7\\"","\\"svForget\\":\\"30\\""));undo();[a,D.tour.guideSteps===D.tour.tours.painting.written,D.guide.svForget]');
  ok('[undo] the open point\'s tour and its written tour are one array before and after an undo (a step is written into D in place)', shared[0] === true && shared[1] === true && shared[2] === '30', shared);
  await R('D.guide.svForget="7";undoPast.length=0;undoFuture.length=0;""');
  ok('[undo] at most 300 steps are kept', await R('var b=JSON.stringify(D);for(var i=0;i<310;i++)undoSnap(b.replace("\\"svForget\\":\\"7\\"","\\"svForget\\":\\""+i+"\\""));var n=undoPast.length;undoPast.length=0;undoFuture.length=0;emit("undo.changed");n===300'));

  // ---------- [never] the visitor's choices, a tap on the phone, the self-test's walk ----------
  await R('ui.view="card";ui.stop="half";setOverlay("settings",true);renderAll(false);""'); await W(500);
  const d0 = await R('JSON.stringify(D)');
  await p.locator('#ph [data-gset="speed|1.2"]').first().click(); await W(200); await p.locator('#ph [data-gset="voice|device"]').first().click(); await W(200);
  await p.locator('#ph [data-vis="mode|dark"]').first().click(); await W(200);
  const vis = JSON.parse(await R(`JSON.stringify({steps:undoPast.length,same:JSON.stringify(D)===${JSON.stringify(d0)},pg:P.guide,dg:[D.guide.speed,D.guide.voice],speed:guideSpeed(),voice:guideSetting("voice"),dark:P.dark,pressed:ph.querySelector('[data-gset="speed|1.2"]').getAttribute("aria-pressed")})`));
  ok('[never] a visitor\'s choices on the phone (Settings › The guide: Faster, Device voice; Dark; real taps) write P, never D, and take no step',
    vis.steps === 0 && vis.same && vis.pg.speed === 1.2 && vis.pg.voice === 'device' && vis.dg[0] === 1 && vis.dg[1] === 'recorded' && vis.dark === true && vis.pressed === 'true', vis);
  ok('[visitor] the guide speaks at the visitor\'s speed and voice (guideSetting: P.guide over D.guide)', vis.speed === 1.2 && vis.voice === 'device', vis);
  await R('P.dark=false;setOverlay("settings",false);ui.stop="half";renderAll(false);""'); await W(500);
  const g = await p.locator('#grab').boundingBox(); await p.mouse.click(g.x + g.width / 2, g.y + g.height / 2); await W(600);
  const tap = JSON.parse(await R('JSON.stringify([ui.stop,undoPast.length])'));
  ok('[never] a tap on the phone (the grabber: the card\'s stop, ui) takes no step', tap[0] !== 'half' && tap[1] === 0, tap);
  await R('ui.tab="card";ui.level=9;ui.op={};renderEditor();noDraw=true;""');
  await p.locator('[data-set="@D.card.secCfg.look"][data-v="cards"]').first().click(); await W(200);
  const walk = JSON.parse(await R('var r=[D.card.secCfg.look,undoPast.length];noDraw=false;D.card.secCfg.look="open";renderAll(true);JSON.stringify(r)'));
  ok('[never] the self-test\'s walk (noDraw) takes no step', walk[0] === 'cards' && walk[1] === 0, walk);

  // ---------- [visitor] Forget ----------
  await R('ui.tab="card";ui.level=9;ui.op={};renderEditor();""'); await W(300);
  const fg = p.locator('[data-forget]').first();
  if (await fg.count()) { await fg.scrollIntoViewIfNeeded(); await fg.click(); await W(300) }
  const fo = JSON.parse(await R('JSON.stringify([Object.keys(P.guide).length,guideSpeed(),guideSetting("voice"),undoPast.length])'));
  ok('[visitor] Forget clears the visitor\'s guide choices: the author\'s speed and voice again (and no step)', fo[0] === 0 && fo[1] === 1 && fo[2] === 'recorded' && fo[3] === 0, fo);

  ok('No page errors', errs.length === 0, errs.slice(0, 5));
  console.log('TIME ' + Math.round((Date.now() - T0) / 1000) + ' s');
  console.log(fails ? 'FAILED ' + fails : 'ALL PASS'); await b.close(); process.exit(fails ? 1 : 0);
})();
