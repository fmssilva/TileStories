// test9db (Playwright + the installed Edge): node test9db.js -> PASS/FAIL per check. The second half of round 9's test9d (split in
// guidance round 10, by its own sections; every check kept as it was): each Editor row's values on the phone; each row's demo (the Guide tab, 6 · Audiences: a press on each label, a change) and the self-test failing a row without one.
// MUTATE=1 (round 21): the Mini's controls ignore Controls at Mini: the rows' values check turns red
// (test9da keeps its own mutation, the toast's Resume; test9n mutates the NO DEMO check itself).
// Guidance round 17b: split in two to keep each file under 300 s (it ran 530 s): test9db the rows' values, the first half of the rows'
// demos (and the controls' three) and the NO DEMO checks; test9dc.js (HALF=2, this file) the second half of the rows' demos. Every check kept.
const { chromium, labUrl } = require('./pw');
const fs = require('fs'), path = require('path');
const url = labUrl('#probe');
const OUT = path.join(__dirname, '..', '..', 'voicebench', 'out', 'azure-sonia-raquel');
const MP3 = fs.readFileSync(path.join(OUT, fs.readdirSync(OUT).filter(f => f.endsWith('.mp3')).sort()[0]));
(async () => {
  const b = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader', '--autoplay-policy=no-user-gesture-required'] });
  const p = await b.newPage({ viewport: { width: 1400, height: 900 } });
  const errs = []; p.on('pageerror', e => errs.push(e.message));
  const el = { n: 0 };
  await p.route('**/api.elevenlabs.io/**', r => { const body = JSON.parse(r.request().postData() || '{}'), t = body.text || '', st = [], en = []; el.n++; for (let i = 0; i < t.length; i++) { st.push(i * .06); en.push(i * .06 + .05); }
    r.fulfill({ status: 200, contentType: 'application/json', headers: { 'access-control-allow-origin': '*' }, body: JSON.stringify({ audio_base64: MP3.toString('base64'), alignment: { characters: t.split(''), character_start_times_seconds: st, character_end_times_seconds: en } }) }); });
  await p.goto(url); await p.waitForFunction(() => window.LAB, null, { timeout: 90000 }); await p.waitForTimeout(800);
  const R = src => p.evaluate(s => LAB.run(s), src); const W = ms => p.waitForTimeout(ms);
  const until = async (src, ms) => { const t0 = Date.now(); while (Date.now() - t0 < ms) { if (await R(src)) return true; await W(60); } return false; };
  let fails = 0; const ok = (name, cond, info) => { if (!cond) fails++; console.log((cond ? 'PASS ' : 'FAIL ') + name + (info !== undefined ? '  ' + JSON.stringify(info) : '')); };
  // a finger on the phone: a real click at the middle of an element, a real drag on it
  const box = async sel => p.locator('#ph ' + sel).first().boundingBox();
  const tap = async sel => { const r = await box(sel); if (!r) return false; await p.mouse.click(r.x + r.width / 2, r.y + r.height / 2); return true; };
  const drag = async (sel, dy) => { const r = await box(sel); await p.mouse.move(r.x + r.width / 2, r.y + r.height / 2); await p.mouse.down(); await p.mouse.move(r.x + r.width / 2, r.y + r.height / 2 + dy, { steps: 6 }); await p.mouse.up(); };
  const size = async sel => { const r = await box(sel); return r ? [Math.round(r.width), Math.round(r.height)] : [0, 0]; };
  // the device voice, muted: its words go on by time (one every ~350 ms), the same on every run
  const base = 'D.card.hints.on=false;LAB.previewBar=false;P.lang="en";P.guideMute=true;P.introSeen=true;P.reduce=false;P.dark=false;D.guide.voice="device";D.guide.panel="dock";D.guide.dockStart="mini";D.guide.miniCtl="pausestop";D.guide.swipeCompact=true;D.guide.stopUndo="toast";D.guide.gbtn="both";D.guide.gbIcon="star";D.guide.gbSide="left";D.guide.gbFold=true;D.guide.azuTap="play";D.guide.life="lively";D.poi.solemn=false;D.guide.captions=true;G.resumeAt=null;hideStopUi();LAB.wall="ribeira";setOverlay("ask",false);setOverlay("settings",false);';
  const tourAt = async (id, h) => { await R(base + `if(G.on)stopTour();ui.view="card";ui.stop="full";renderAll(false);startTour();var j=D.tour.guideSteps.findIndex(function(s){return s.id==="${id}"});if(j>0){stopSpeech();G.i=j;runStep()}ui.dockH="${h || 'mini'}";renderGuide()`); await W(500); };
  const past = async () => { await R(base + 'if(G.on)stopTour();setOverlay("menu",false);ui.view="card";ui.stop="full";renderAll(false);scr.scrollTop=900;fabLayer()'); await W(400); };

  const T0 = Date.now(), tsec = s => console.log('TIME ' + s + ' ' + Math.round((Date.now() - T0) / 1000) + ' s');
  const HALF2 = process.env.HALF === '2';
  // round 21 (21.10, the full tests: MUTATE=1 here caught nothing, the mutation was only test9da's): a mutation of what this file checks:
  // the dock's controls at Mini come out as Pause only whatever the row says (a dead demo made the file wait past its 10 minutes)
  if (process.env.MUTATE) { await R('dkMiniCtl=function(){return\'<div class="gd-mini">\'+dkPP("gd-mc gd-mpp")+\'</div>\'}'); console.log('MUTATION: the Mini\'s controls ignore Controls at Mini'); }
  if (!HALF2) {
  // ---------- each Editor row's values on the phone (real clicks on the Editor) ----------
  await R(base + 'ui.tab="guide";ui.op={gt1:true,s7dock:true,"gt:ways":true,s7gbtn:true,s7mascot:true};ui.view="card";ui.stop="full";renderAll(false)');
  // round 19 moved rows into 1 · The mascot's foldouts (round 20: they start collapsed): open every foldout above the row, as the developer would
  const pick = async (key, v) => { const s = `[data-set="@D.guide.${key}"][data-v="${v}"]`; await R(`var b=ed.querySelector('${s}');for(var d=b&&b.closest("details");d;d=d.parentElement&&d.parentElement.closest("details"))d.open=true`); await p.locator(s).scrollIntoViewIfNeeded(); await p.locator(s).click(); await W(150); await R('clearTimeout(G.demoT);G.demoSeq++;if(G.on)stopTour();hideStopUi()'); };
  const startAt = async () => { await R('ui.view="card";ui.stop="full";renderPhone(false);startTour();G.i=D.tour.guideSteps.findIndex(function(s){return s.id==="g1"});stopSpeech();runStep()'); await W(400); };
  const want = [];
  for (const [v, chk] of [['compact', '!ph.querySelector(".gdock")&&!!ph.querySelector(".gbtn.tour .gb-ring")'], ['peek', '!!ph.querySelector(".gdock.h-peek")'], ['mini', '!!ph.querySelector(".gdock.h-mini")']]) { await pick('dockStart', v); await startAt(); want.push(['Starts at ' + v, await R(chk)]); await R('stopTour()'); }
  for (const [v, chk] of [['pause', '!!ph.querySelector(\'.gd-mini [data-g="pause"]\')&&!ph.querySelector(\'.gd-mini [data-g="stop"]\')'], ['line', '!ph.querySelector(".gd-mini")&&!!ph.querySelector(".gd-main .gd-kb")'], ['pausestop', '!!ph.querySelector(\'.gd-mini [data-g="pause"]\')&&!!ph.querySelector(\'.gd-mini [data-g="stop"]\')&&!!ph.querySelector(\'.gd-mini [data-g="back10"]\')']]) { /* round 21 (21.3): the Mini has back 10 s too */ await pick('miniCtl', v); await startAt(); want.push(['Controls at Mini ' + v, await R(chk)]); await R('stopTour()'); }
  for (const [v, chk] of [['face', '!!ph.querySelector(".gbtn.ic-face .gb-ic svg rect")'], ['word', '!ph.querySelector(".gbtn .gb-ic")&&ph.querySelector(".gbtn").textContent==="Guide"'], ['star', '!!ph.querySelector(".gbtn.ic-star .gb-ic svg")']]) { await pick('gbIcon', v); await R('ui.view="card";ui.stop="full";renderPhone(false);scr.scrollTop=900;fabLayer()'); want.push(['Icon ' + v, await R(chk)]); }
  for (const [v, chk] of [['right', 'ph.querySelector(".gbtn-wrap").getBoundingClientRect().left-ph.getBoundingClientRect().left>180&&ph.querySelector(".azfab").getBoundingClientRect().right<=ph.querySelector(".gbtn").getBoundingClientRect().left'], ['left', 'ph.querySelector(".gbtn-wrap").getBoundingClientRect().left-ph.getBoundingClientRect().left<20']]) { await pick('gbSide', v); await R('ui.view="card";ui.stop="full";renderPhone(false);scr.scrollTop=900;fabLayer()'); const drawn = await until('!!ph.querySelector(".gbtn-wrap")&&!!ph.querySelector(".azfab")&&!!ph.querySelector(".gbtn")', 3000); want.push(['Side ' + v, drawn && await R(chk)]); }
  for (const [v, chk] of [['azu', '!ph.querySelector(".gbtn")&&!!ph.querySelector(".azfab")'], ['star', '!!ph.querySelector(".gbtn")&&!ph.querySelector(".azfab")'], ['both', '!!ph.querySelector(".gbtn")&&!!ph.querySelector(".azfab")']]) { await pick('gbtn', v); await R('ui.view="card";ui.stop="full";renderPhone(false);scr.scrollTop=900;fabLayer()'); want.push(['Guide button ' + v, await R(chk)]); }
  await pick('stopUndo', 'ask'); want.push(['Stop ask', await R('D.guide.stopUndo==="ask"')]); await pick('stopUndo', 'toast');
  await pick('swipeCompact', 'false'); want.push(['Swipe off', await R('D.guide.swipeCompact===false')]); await pick('swipeCompact', 'true');
  await pick('gbFold', 'false'); want.push(['Label folds off', await R('D.guide.gbFold===false')]); await pick('gbFold', 'true');
  for (const v of ['point', 'menu', 'play']) { await pick('azuTap', v); want.push(['Tap on Azu ' + v, await R(`D.guide.azuTap==="${v}"`)]); }
  ok('Each new row, clicked in the Editor, reaches the phone (Starts at, Controls at Mini, Icon, Side, Guide button) or the guide (Stop, Swipe, Label folds, Tap on Azu)', want.every(w => w[1]), want.filter(w => !w[1]));
  // round 17b: the ✦ menu's Resume the tour (gb_resume) is the one offer's sv_continue now: Guide button owns 8 words, not 9
  ok('...the Editor: Guide panel has Controls at Mini, Swipe down to Compact, Stop; Guide button in Ways in (round 14); Azu has Tap on Azu; the old row is gone', await R('var d=ed.querySelector(\'[data-fo="s7dock"]\'),g=ed.querySelector(\'[data-fo="s7gbtn"]\');["Starts at","Controls at Mini","Swipe down to Compact","Stop"].every(function(t){return!!rowEl(ed,"s7dock",t)})&&g.parentElement.closest("[data-fo]").dataset.fo==="gt:ways"&&["Guide button","Icon","Side","Label folds after 4 s"].every(function(t){return!!rowEl(ed,"s7gbtn",t)})&&!!rowEl(ed,"gt:manner","Tap on the mascot")&&!/Azu as the guide/.test(ed.textContent)&&(TXO.s7gbtn||[]).length===8&&TXOWN.gb_hint==="gd:i:gbhint"'));
  }

  // ---------- each row's demo (the Guide tab, 6 · Audiences) ----------
  await R(base + 'ui.tab="guide";ui.level=9;ui.op={};ui.view="card";ui.stop="full";renderAll(false)');
  const rowKeys = sel => R('var o=[];ed.querySelectorAll(\'' + sel + '\').forEach(function(r){var k=rowKey(r);if(k&&o.indexOf(k)<0)o.push(k)});o');
  const keysG = await rowKeys('.row'); await R('ui.tab="card";renderEditor()'); const keysA = await rowKeys('[data-fo="s8"] .row'), keys = keysG.concat(keysA);
  const nod = await R('ui.tab="guide";renderEditor();var a=rowsWithoutDemo(guideSecIds());ui.tab="card";renderEditor();a.concat(rowsWithoutDemo(["s8"])).map(function(r){return r.key})');
  if (!HALF2) ok(`Every row of the Guide tab and 6 · Audiences declares its demo (${keys.length} rows)`, keys.length >= 70 && nod.length === 0, nod);
  // pressed here: every row but 4 · Guidance's, which test9e presses one by one (every row of that section) and test9f again for its new ones
  // round 17b: test9db presses the first half (and the controls' three keys its last check reads), test9dc the rest
  const pressAll = keys.filter(k => !/^D\.guidance\./.test(k)), mid = Math.ceil(pressAll.length / 2), CTL = ['D.guide.miniCtl', 'D.guide.stopUndo', 'D.guide.gbtn', 'D.guide.azuTap'];
  const shown = [], missing = [], pressKeys = HALF2 ? pressAll.slice(mid).filter(k => CTL.indexOf(k) < 0) : pressAll.slice(0, mid).concat(pressAll.slice(mid).filter(k => CTL.indexOf(k) >= 0));
  for (const k of pressKeys) {
    await R(`ui.tab="${keysG.indexOf(k) >= 0 ? 'guide' : 'card'}";if(ed.dataset.tabNow!==ui.tab){ed.dataset.tabNow=ui.tab;renderEditor()}`);
    await R('document.body.dataset.rowdemo=""');
    const lb = await R(`var c=ed.querySelector('[data-set="@${k}"],[data-sel="@${k}"],[data-num="@${k}"],[data-set="${k}"],[data-sel="${k}"],[data-num="${k}"]'),l=c.closest(".row").querySelector("label.rd");l.scrollIntoView({block:"center"});var r=l.getBoundingClientRect();[r.left+12,r.top+r.height/2]`);
    await p.mouse.click(lb[0], lb[1]);
    let v = ''; const t0 = Date.now(); while (Date.now() - t0 < 6000) { v = await R('document.body.dataset.rowdemo||""'); if (v) break; await W(80); }
    (/\|found\|/.test(v) ? shown : missing).push(k + ' ' + v.split('|').slice(1).join(' '));
  }
  ok(`...a press on each row\'s label shows its demo on the phone, its part ringed (${HALF2 ? 'the second half of the ' + pressAll.length + ' rows' : 'the first half of the ' + pressAll.length + ' rows; test9dc the rest'})`, missing.length === 0 && shown.length === pressKeys.length && pressAll.length >= 70 && pressKeys.length >= 30, [missing, pressKeys.length]);
  if (!HALF2) {
  await R('ui.tab="guide";ui.op={gt1:true,s7dock:true};renderEditor()'); await R('document.body.dataset.rowdemo=""'); await p.locator('[data-set="@D.guide.dockStart"][data-v="compact"]').click();
  await until('document.body.dataset.rowdemo', 5000);
  ok('...a change to a row shows its demo too: Starts at = Compact, the tour starts hidden, the ✦ ringed', /^D\.guide\.dockStart\|found\|\.gbtn-wrap\.intour/.test(await R('document.body.dataset.rowdemo')) && await R('G.on&&ui.dockH==="compact"'), await R('document.body.dataset.rowdemo'));
  await R('stopTour();D.guide.dockStart="mini";renderEditor()');
  ok('...the controls\' demos: Controls at Mini rings the Mini controls; Stop shows the toast; Guide button the ✦ and Azu past the hero', shown.some(s => /^D\.guide\.miniCtl found \.gd-mini/.test(s)) && shown.some(s => /^D\.guide\.stopUndo found \.gtoast/.test(s)) && shown.some(s => /^D\.guide\.gbtn found \.gbtn-wrap/.test(s)) && shown.some(s => /^D\.guide\.azuTap found/.test(s)), shown.filter(s => /miniCtl|stopUndo|gbtn |azuTap/.test(s)));
  // the self-test fails a row of the Guide tab or 6 · Audiences that has no demo (a row added without one)
  await R('on("edCard.html",function(h){if(!window.__fake)return h;var d=document.createElement("div");d.innerHTML=h;addRows(d,"s7dock",row("A row with no demo",pseg("D.guide.noDemoRow",["a","b"],["A","B"]),""));return d.innerHTML});window.__fake=1;D.guide.noDemoRow="a"');
  const st = await R('var j=extPoint("selfTest.res").filter(function(f){return String(f).indexOf("rowsWithoutDemo")>=0})[0],r=j([]);showSelfTest(r);[r.map(function(x){return x.key+" "+x.result.slice(0,7)}),JSON.parse(document.getElementById("selftest").dataset.json).bad.length]');
  await R('window.__fake=0;delete D.guide.noDemoRow;renderEditor();document.getElementById("selftest").innerHTML=""');
  ok('The self-test fails a row of the Guide tab with no demo (NO DEMO, counted as bad)', st[0].length === 1 && /^@D\.guide\.noDemoRow NO DEMO/.test(st[0][0]) && st[1] === 1, st);
  }

  tsec('the Editor rows and each row\'s demo');
  ok('no page errors', errs.length === 0, errs.slice(0, 3));
  console.log(fails ? `${fails} FAILED` : 'ALL PASS');
  await b.close();
})();
